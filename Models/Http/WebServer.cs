// ============================================================================
// 
// ゆかり用の Web サーバー機能
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 【アクセス仕様】
// ・サムネイル画像取得
//   <アドレス>:<ポート>/thumb?uid=<ファイル番号>[&width=<横幅>][&easypass=<簡易認証キーワード>]
//   横幅として指定可能な値は YlCommon.THUMB_WIDTH_LIST 参照
//   http://localhost:13582/thumb?uid=7&width=80
// ・動画プレビュー
//   <アドレス>:<ポート>/preview?uid=<ファイル番号>[&easypass=<簡易認証キーワード>]
//   http://localhost:13582/preview?uid=123
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.Models.Http
{
	public class WebServer
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public WebServer(EnvironmentModel oEnvironment, YukariListDatabaseInMemory oYukariListDbInMemory, CancellationToken oToken)
		{
			mEnvironment = oEnvironment;
			mYukariListDbInMemory = oYukariListDbInMemory;
			mToken = oToken;
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 非同期に実行開始
		// --------------------------------------------------------------------
		public Task RunAsync()
		{
			return YlCommon.LaunchTaskAsync<Object>(WebServerByWorker, mTaskLockWebServer, null, mEnvironment.LogWriter);
		}

		// --------------------------------------------------------------------
		// 非同期に停止
		// --------------------------------------------------------------------
		public Task StopAsync()
		{
			return Task.Run(() =>
			{
				try
				{
					// ダミーデータを送信してサーバーの待機を終了させる
					TcpClient aClient = new TcpClient("localhost", mEnvironment.YukaListerSettings.WebServerPort);
					using (NetworkStream aNetworkStream = aClient.GetStream())
					{
						aNetworkStream.ReadTimeout = YlConstants.TCP_TIMEOUT;
						aNetworkStream.WriteTimeout = YlConstants.TCP_TIMEOUT;
						Byte[] aSendBytes = Encoding.UTF8.GetBytes("End");
						aNetworkStream.Write(aSendBytes, 0, aSendBytes.Length);
					}
					aClient.Close();

					mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "プレビューサーバー終了");
				}
				catch (Exception oExcep)
				{
					mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "プレビューサーバー終了時エラー：" + oExcep.Message, true);
				}
			});
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// 直ちに起動できるタスクの数（アプリケーション全体）
		private const Int32 APP_WORKER_THREADS = 16;

		// Web サーバー以外用に残しておくタスクの数
		private const Int32 GENERAL_WORKER_THREADS = 1;

		// サムネイルのアスペクト比
		private const Double THUMB_ASPECT_RATIO = 16.0 / 9;

		// サムネイルの横幅の最大値
		private const Int32 THUMB_WIDTH_MAX = 320;

		// コマンド
		private const String SERVER_COMMAND_PREVIEW = "preview";
		private const String SERVER_COMMAND_THUMB = "thumb";

		// コマンドオプション
		private const String OPTION_NAME_EASY_PASS = "easypass";
		private const String OPTION_NAME_UID = "uid";
		private const String OPTION_NAME_WIDTH = "width";

		// サムネイル生成時のタイムアウト [ms]
		private const Int32 THUMB_TIMEOUT = 10 * 1000;

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// タスク上限
		private Int32 mWebServerTasksLimit;

		// 環境設定類
		private EnvironmentModel mEnvironment;

		// ゆかり用リストデータベース（作業用インメモリ）
		private YukariListDatabaseInMemory mYukariListDbInMemory;

		// 終了用
		private CancellationToken mToken;

		// 排他制御
		private static Object mTaskLockWebServer = new Object();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// URL 引数を解析
		// --------------------------------------------------------------------
		private Dictionary<String, String> AnalyzeCommandOptions(String oCommand)
		{
			Dictionary<String, String> aOptions = new Dictionary<String, String>();

			Int32 aQuesPos = oCommand.IndexOf('?');
			if (0 <= aQuesPos && aQuesPos < oCommand.Length - 1)
			{
				String[] aOptionStrings = oCommand.Substring(aQuesPos + 1).Split('&');
				for (Int32 i = 0; i < aOptionStrings.Length; i++)
				{
					Int32 aEqPos = aOptionStrings[i].IndexOf('=');
					if (0 < aEqPos && aEqPos < aOptionStrings[i].Length - 1)
					{
						aOptions[aOptionStrings[i].Substring(0, aEqPos)] = aOptionStrings[i].Substring(aEqPos + 1);
					}
				}
			}

			return aOptions;
		}

		// --------------------------------------------------------------------
		// 簡易認証を満たしているかどうか
		// ＜返値＞ 満たしている、または、認証不要の場合は true
		// --------------------------------------------------------------------
		private Boolean CheckEasyAuth(Dictionary<String, String> oOptions)
		{
			if (!mEnvironment.YukaListerSettings.YukariUseEasyAuth)
			{
				// 認証不要
				return true;
			}

			if (oOptions.ContainsKey(OPTION_NAME_EASY_PASS) && oOptions[OPTION_NAME_EASY_PASS] == mEnvironment.YukaListerSettings.YukariEasyAuthKeyword)
			{
				// 認証成功
				return true;
			}

			return false;
		}

		// --------------------------------------------------------------------
		// サムネイルを JPEG 形式で作成
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private TCacheThumb CreateThumb(String oPathExLen, Int32 oWidth)
		{
			// MediaPlayer がいつまで生きていればサムネイルが確定されるか不明のため、最後に Close() できるよう、最初に生成しておく
			MediaPlayer aPlayer = new MediaPlayer
			{
				IsMuted = true,
				ScrubbingEnabled = true,
			};

			try
			{
				// 動画を開いてすぐに一時停止する
				// Uri は extended-length パスをサポートしていない模様なので短くする
				aPlayer.Open(new Uri("file://" + mEnvironment.ShortenPath(oPathExLen), UriKind.Absolute));
				aPlayer.Play();
				aPlayer.Pause();

				// 指定位置へシーク
				aPlayer.Position = TimeSpan.FromSeconds(mEnvironment.YukaListerSettings.ThumbSeekPos);

				// 読み込みが完了するまで待機
				Int32 aTick = Environment.TickCount;
				while (aPlayer.DownloadProgress < 1.0 || aPlayer.NaturalVideoWidth == 0)
				{
					Thread.Sleep(Common.GENERAL_SLEEP_TIME);
					if (Environment.TickCount - aTick > THUMB_TIMEOUT)
					{
						throw new Exception("Movie read timeout.");
					}
				}

				// 描画用の Visual に動画を描画
				// 縮小して描画するとニアレストネイバー法で縮小されて画質が悪くなる
				// RenderOptions.SetBitmapScalingMode() も効かないようなので、元のサイズで描画する
				DrawingVisual aOrigVisual = new DrawingVisual();
				using (DrawingContext aContext = aOrigVisual.RenderOpen())
				{
					aContext.DrawVideo(aPlayer, new Rect(0, 0, aPlayer.NaturalVideoWidth, aPlayer.NaturalVideoHeight));
				}

				// ビットマップに Visual を描画
				Boolean aIsSave = true;
				aTick = Environment.TickCount;
				RenderTargetBitmap aOrigBitmap = new RenderTargetBitmap(aPlayer.NaturalVideoWidth, aPlayer.NaturalVideoHeight, 96, 96, PixelFormats.Pbgra32);
				for (; ; )
				{
					aOrigBitmap.Render(aOrigVisual);
					if (IsRenderDone(aOrigBitmap))
					{
						Debug.WriteLine("CreateThumb() render done time: " + (Environment.TickCount - aTick));
						break;
					}
					Thread.Sleep(Common.GENERAL_SLEEP_TIME);
					if (Environment.TickCount - aTick > THUMB_TIMEOUT)
					{
						// サムネイルが黒い場合もタイムアウトとなるので、キャッシュに保存はしないが送信はする
						aIsSave = false;
						Debug.WriteLine("CreateThumb() time out: ビットマップに Visual を描画時");
						break;
					}
				}

				// 生成するサムネイルのサイズを計算
				Int32 aThumbWidth = oWidth;
				Int32 aThumbHeight = (Int32)(aThumbWidth / THUMB_ASPECT_RATIO);
				Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] CreateThumb() Thumb size: " + aThumbWidth + " x " + aThumbHeight);

				// 動画のリサイズサイズを計算
				Double aPlayerAspectRatio = (Double)aPlayer.NaturalVideoWidth / aPlayer.NaturalVideoHeight;
				Int32 aResizeWidth;
				Int32 aResizeHeight;
				if (aPlayerAspectRatio > THUMB_ASPECT_RATIO)
				{
					aResizeWidth = aThumbWidth;
					aResizeHeight = (Int32)(aResizeWidth / THUMB_ASPECT_RATIO);
				}
				else
				{
					aResizeHeight = aThumbHeight;
					aResizeWidth = (Int32)(aResizeHeight * THUMB_ASPECT_RATIO);
				}
				Double aScale = (Double)aResizeWidth / aPlayer.NaturalVideoWidth;
				Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] CreateThumb() Resize size: " + aResizeWidth + " x " + aResizeHeight);

				// 縮小
				var aScaledBitmap = new TransformedBitmap(aOrigBitmap, new ScaleTransform(aScale, aScale));

				// サムネイルサイズにはめる
				DrawingVisual aThumbVisual = new DrawingVisual();
				using (DrawingContext aContext = aThumbVisual.RenderOpen())
				{
					aContext.DrawImage(aScaledBitmap, new Rect((aThumbWidth - aResizeWidth) / 2, (aThumbHeight - aResizeHeight) / 2, aResizeWidth, aResizeHeight));
				}

				// ビットマップに Visual を描画
				aTick = Environment.TickCount;
				RenderTargetBitmap aThumbBitmap = new RenderTargetBitmap(aThumbWidth, aThumbHeight, 96, 96, PixelFormats.Pbgra32);
				aThumbBitmap.Render(aThumbVisual);

				// JPEG にエンコード
				JpegBitmapEncoder aJpegEncoder = new JpegBitmapEncoder();
				aJpegEncoder.Frames.Add(BitmapFrame.Create(aThumbBitmap));

				// キャッシュに保存
				TCacheThumb aCacheThumb = SaveCache(aIsSave, oPathExLen, aJpegEncoder);

				return aCacheThumb;
			}
			finally
			{
				aPlayer.Close();
			}
		}

		// --------------------------------------------------------------------
		// サムネイルキャッシュデータベースを検索
		// --------------------------------------------------------------------
		private TCacheThumb FindCache(String oPathExLen, Int32 oWidth)
		{
			String aFileName = Path.GetFileName(oPathExLen);
			using (YukariThumbnailDatabaseInDisk aYukariThumbDbInDisk = new YukariThumbnailDatabaseInDisk(mEnvironment))
			using (DataContext aCacheDbContext = new DataContext(aYukariThumbDbInDisk.Connection))
			{
				Table<TCacheThumb> aTableCache = aCacheDbContext.GetTable<TCacheThumb>();
				IQueryable<TCacheThumb> aQueryResult =
						from x in aTableCache
						where x.FileName == aFileName && x.Width == oWidth
						select x;
				foreach (TCacheThumb aRecord in aQueryResult)
				{
					// ファイルのタイムスタンプを比較
					FileInfo aFileInfo = new FileInfo(oPathExLen);
					if (aRecord.FileLastWriteTime == JulianDay.DateTimeToModifiedJulianDate(aFileInfo.LastWriteTime))
					{
						Debug.WriteLine("FindCache() Hit: " + oPathExLen);
						return aRecord;
					}

					// 不一致のキャッシュは削除し、後のキャッシュ保存が可能となるようにする
					try
					{
						aTableCache.DeleteOnSubmit(aRecord);
						aCacheDbContext.SubmitChanges();
					}
					catch (Exception)
					{
					}
				}
			}

			Debug.WriteLine("FindCache() Miss: " + oPathExLen);
			return null;
		}

		// --------------------------------------------------------------------
		// URL 引数から動画ファイルのパスを解析
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void GetPathOption(Dictionary<String, String> oOptions, out String oPathExLen)
		{
			if (!oOptions.ContainsKey(OPTION_NAME_UID))
			{
				throw new Exception("Parameter " + OPTION_NAME_UID + " is not specified.");
			}
			Int32 aUid = Int32.Parse(oOptions[OPTION_NAME_UID]);

			// ゆかり用データベースから UID を検索
			TFound aTarget = null;
			using (DataContext aYukariDbContext = new DataContext(mYukariListDbInMemory.Connection))
			{
				Table<TFound> aTableFound = aYukariDbContext.GetTable<TFound>();
				IQueryable<TFound> aQueryResult =
						from x in aTableFound
						where x.Uid == aUid
						select x;
				foreach (TFound aRecord in aQueryResult)
				{
					aTarget = aRecord;
					break;
				}
			}
			if (aTarget == null)
			{
				throw new Exception("Bad " + OPTION_NAME_UID + ".");
			}
			oPathExLen = mEnvironment.ExtendPath(aTarget.Path);
		}

		// --------------------------------------------------------------------
		// URL 引数からサムネイル用オプションを解析
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void GetThumbOptions(Dictionary<String, String> oOptions, out String oPathExLen, out Int32 oWidth)
		{
			GetPathOption(oOptions, out oPathExLen);

			// 横幅を解析
			if (!oOptions.ContainsKey(OPTION_NAME_WIDTH))
			{
				oWidth = mEnvironment.YukaListerSettings.ThumbDefaultWidth;
			}
			else
			{
				oWidth = Int32.Parse(oOptions[OPTION_NAME_WIDTH]);
				if (oWidth < YlConstants.THUMB_WIDTH_LIST[0] || oWidth > YlConstants.THUMB_WIDTH_LIST[YlConstants.THUMB_WIDTH_LIST.Length - 1])
				{
					throw new Exception("Bad width.");
				}
			}

			// 既定の横幅に調整
			Int32 aIndex = 0;
			for (Int32 i = YlConstants.THUMB_WIDTH_LIST.Length - 1; i >= 0; i--)
			{
				if (oWidth >= YlConstants.THUMB_WIDTH_LIST[i])
				{
					aIndex = i;
					break;
				}
			}
			oWidth = YlConstants.THUMB_WIDTH_LIST[aIndex];
		}

		// --------------------------------------------------------------------
		// Visual に描画された動画がいつビットマップに転写されるか分からないため
		// ビットマップ中央が黒以外になったら転写完了と判断する
		// 動画そのものが黒い場合もあるため、無限ループにならないよう呼び出し元で注意が必要
		// --------------------------------------------------------------------
		private Boolean IsRenderDone(RenderTargetBitmap oBitmap)
		{
			Int32 aWidth = oBitmap.PixelWidth;
			Int32 aHeight = oBitmap.PixelHeight;
			Byte[] aPixels = new Byte[aWidth * aHeight * oBitmap.Format.BitsPerPixel / 8];
			Int32 aStride = (aWidth * oBitmap.Format.BitsPerPixel + 7) / 8;

			// ピクセルデータを配列にコピー
			oBitmap.CopyPixels(aPixels, aStride, 0);

			// 中央の位置
			Int32 aOffset = (aHeight / 2) * aStride + (aWidth / 2) * oBitmap.Format.BitsPerPixel / 8;

			// RGB いずれかが 0 以外なら転写完了
			return aPixels[aOffset] != 0 || aPixels[aOffset + 1] != 0 || aPixels[aOffset + 2] != 0;
		}

		// --------------------------------------------------------------------
		// キャッシュデータベースにレコードを保存する
		// --------------------------------------------------------------------
		private TCacheThumb SaveCache(Boolean oIsSave, String oPathExLen, JpegBitmapEncoder oJpegEncoder)
		{
			TCacheThumb aCacheThumb = new TCacheThumb();
			aCacheThumb.FileName = Path.GetFileName(oPathExLen);
			aCacheThumb.Width = (Int32)oJpegEncoder.Frames[0].Width;
			Debug.WriteLine("SaveCache() width: " + aCacheThumb.Width);

			// サムネイル画像データを取得
			using (MemoryStream aMemStream = new MemoryStream())
			{
				oJpegEncoder.Save(aMemStream);
				aCacheThumb.Image = new Byte[aMemStream.Length];
				aMemStream.Seek(0, SeekOrigin.Begin);
				aMemStream.Read(aCacheThumb.Image, 0, (Int32)aMemStream.Length);
			}

			FileInfo aFileInfo = new FileInfo(oPathExLen);
			aCacheThumb.FileLastWriteTime = JulianDay.DateTimeToModifiedJulianDate(aFileInfo.LastWriteTime);
			aCacheThumb.ThumbLastWriteTime = JulianDay.DateTimeToModifiedJulianDate(DateTime.UtcNow);

			if (oIsSave)
			{
				using (YukariThumbnailDatabaseInDisk aYukariThumbDbInDisk = new YukariThumbnailDatabaseInDisk(mEnvironment))
				using (DataContext aCacheDbContext = new DataContext(aYukariThumbDbInDisk.Connection))
				{
					Table<TCacheThumb> aTableCache = aCacheDbContext.GetTable<TCacheThumb>();

					// ユニーク ID の決定
					IQueryable<Int64> aQueryResult =
							from x in aTableCache
							select x.Uid;
					aCacheThumb.Uid = (aQueryResult.Count() == 0 ? 0 : aQueryResult.Max()) + 1;

					// 保存
					aTableCache.InsertOnSubmit(aCacheThumb);

					try
					{
						aCacheDbContext.SubmitChanges();
					}
					catch (Exception)
					{
						// 他のスレッドが、同一 Uid や同一ファイル名・横幅のレコードを先に書き込んだ場合は例外となるが、
						// キャッシュを保存できなくても致命的ではないため、速やかにクライアントに画像を返すためにリトライはしない
						Debug.WriteLine("SaveCache() save err");
					}
				}
			}

			return aCacheThumb;
		}

		// --------------------------------------------------------------------
		// クライアントにエラーメッセージを返す
		// メッセージは ASCII のみとする
		// --------------------------------------------------------------------
		private void SendErrorResponse(StreamWriter oWriter, String oMessage)
		{
			oWriter.WriteLine("HTTP/1.1 404 Not Found");
			oWriter.WriteLine("Content-Length: " + oMessage.Length);
			oWriter.WriteLine("Content-Type: text/plain");
			oWriter.WriteLine();
			oWriter.WriteLine(oMessage);
		}

		// --------------------------------------------------------------------
		// クライアントにファイルの内容を返す
		// ＜例外＞ Exception, OperationCanceledException
		// --------------------------------------------------------------------
		private void SendFile(NetworkStream oNetworkStream, String oPathExLen)
		{
			FileInfo aFileInfo = new FileInfo(oPathExLen);
			String aContentType;

			// タイプ
			switch (Path.GetExtension(oPathExLen).ToLower())
			{
				case Common.FILE_EXT_AVI:
					aContentType = "video/x-msvideo";
					break;
				case Common.FILE_EXT_MOV:
					aContentType = "video/quicktime";
					break;
				case Common.FILE_EXT_MP4:
					aContentType = "video/mp4";
					break;
				case Common.FILE_EXT_MPEG:
				case Common.FILE_EXT_MPG:
					aContentType = "video/mpeg";
					break;
				default:
					aContentType = "application/octet-stream";
					break;
			}

			String aHeader = "HTTP/1.1 200 OK\n"
					+ "Content-Length: " + aFileInfo.Length.ToString() + "\n"
					+ "Content-Type: " + aContentType + "\n\n";
			Byte[] aSendBytes = Encoding.UTF8.GetBytes(aHeader);
			oNetworkStream.Write(aSendBytes, 0, aSendBytes.Length);

#if DEBUG
			Int32 aReadSizes = 0;
#endif
			Byte[] aBuf = new Byte[1024 * 1024];
			using (FileStream aFileStream = new FileStream(oPathExLen, FileMode.Open, FileAccess.Read))
			{
				for (; ; )
				{
					Int32 aReadSize = aFileStream.Read(aBuf, 0, aBuf.Length);
					if (aReadSize == 0)
					{
						break;
					}

					Int32 aNumRetries = 0;
					while (aNumRetries < YlConstants.TCP_NUM_RETRIES)
					{
						try
						{
							oNetworkStream.Write(aBuf, 0, aReadSize);
							break;
						}
						catch (Exception oExcep)
						{
							mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー内容送信エラー：\n" + oExcep.Message + "\nリトライ回数：" + aNumRetries, true);
							mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
						}
						aNumRetries++;
						mToken.ThrowIfCancellationRequested();

						// 少し待ってみる
						Thread.Sleep(5 * 1000);
					}
					if (aNumRetries >= YlConstants.TCP_NUM_RETRIES)
					{
						throw new OperationCanceledException();
					}

#if DEBUG
					aReadSizes += aReadSize;
#endif
				}
			}

#if DEBUG
			mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "SendFile() sent: " + aReadSizes.ToString("#,0") + " / " + aFileInfo.Length.ToString("#,0"));
#endif
		}

		// --------------------------------------------------------------------
		// クライアントに応答を返す
		// --------------------------------------------------------------------
		private void SendResponse(TcpClient oClient)
		{
			try
			{
				using (NetworkStream aNetworkStream = oClient.GetStream())
				using (StreamReader aReader = new StreamReader(aNetworkStream))
				using (StreamWriter aWriter = new StreamWriter(aNetworkStream))
				{
					// ネットワークストリームの設定
					aNetworkStream.ReadTimeout = YlConstants.TCP_TIMEOUT;
					aNetworkStream.WriteTimeout = YlConstants.TCP_TIMEOUT;

					// ヘッダー部分を読み込む
					//Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] SendResponse() header");
					List<String> aHeaders = new List<String>();
					for (; ; )
					{
						String aLine = aReader.ReadLine();
						if (String.IsNullOrWhiteSpace(aLine))
						{
							break;
						}
						aHeaders.Add(aLine);
					}
					if (aHeaders.Count == 0)
					{
						// ポートノック（ゆかりがプレビューを有効にするかどうかの判定に使用）の場合はヘッダーも無いが、その場合は何もせずに終了する
						return;
					}

					// ヘッダーの 1 行目は [GET|POST] /[DocPath] HTTP/1.1 のようになっている
					String[] aRequests = aHeaders[0].Split(' ');
					if (aRequests.Length < 3)
					{
						throw new Exception("ヘッダーでパスが指定されていません。");
					}
					String aDocPath = aRequests[1];
					//Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] SendResponse() aDocPath: " + aDocPath);
					if (String.IsNullOrEmpty(aDocPath))
					{
						throw new Exception("ヘッダーのパスが空です。");
					}

					// 簡易認証チェック
					Dictionary<String, String> aOptions = AnalyzeCommandOptions(aDocPath);
					if (CheckEasyAuth(aOptions))
					{
						// コマンド解析（先頭が '/' であることに注意）
						if (aDocPath.IndexOf(SERVER_COMMAND_PREVIEW) == 1)
						{
							SendResponsePreview(aNetworkStream, aWriter, aOptions);
						}
						else if (aDocPath.IndexOf(SERVER_COMMAND_THUMB) == 1)
						{
							SendResponseThumb(aNetworkStream, aWriter, aOptions);
						}
						else
						{
							// ToDo: obsolete
							// ゆかり側が新 URL に対応次第削除する
							// パス解析（先頭の '/' を除く）
							String aPath = null;
							if (aDocPath.Length == 1)
							{
								SendErrorResponse(aWriter, "File is not specified.");
							}
							else
							{
								aPath = mEnvironment.ExtendPath(HttpUtility.UrlDecode(aDocPath, Encoding.UTF8).Substring(1).Replace('/', '\\'));
								if (File.Exists(aPath))
								{
									SendFile(aNetworkStream, aPath);
								}
								else
								{
									SendErrorResponse(aWriter, "File not found.");
								}
							}
						}
					}
					else
					{
						SendErrorResponse(aWriter, "Bad auth.");
					}

					aWriter.Flush();
				}
			}
			catch (OperationCanceledException)
			{
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "クライアントへの応答を中止しました。");
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "クライアントへの応答時エラー：\n" + oExcep.Message, true);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 閉じる
				oClient.Close();
			}
		}

		// --------------------------------------------------------------------
		// クライアントにプレビュー用動画データを返す
		// --------------------------------------------------------------------
		private void SendResponsePreview(NetworkStream oNetworkStream, StreamWriter oWriter, Dictionary<String, String> oOptions)
		{
			try
			{
				// 動画の確定
				GetPathOption(oOptions, out String aPathExLen);

				// 送信
				if (File.Exists(aPathExLen))
				{
					SendFile(oNetworkStream, aPathExLen);
				}
				else
				{
					SendErrorResponse(oWriter, "File not found.");
				}
			}
			catch (Exception oExcep)
			{
				SendErrorResponse(oWriter, oExcep.Message);
			}
		}

		// --------------------------------------------------------------------
		// クライアントにサムネイルを返す
		// --------------------------------------------------------------------
		private void SendResponseThumb(NetworkStream oNetworkStream, StreamWriter oWriter, Dictionary<String, String> oOptions)
		{
			try
			{
				// サムネイル対象の確定
				GetThumbOptions(oOptions, out String aPathExLen, out Int32 aWidth);

				// キャッシュから探す
				TCacheThumb aCacheThumb = FindCache(aPathExLen, aWidth);

				if (aCacheThumb == null)
				{
					// キャッシュに無い場合は新規作成
					aCacheThumb = CreateThumb(aPathExLen, aWidth);
				}

				// 更新日
				DateTime aLastModified = JulianDay.ModifiedJulianDateToDateTime(aCacheThumb.ThumbLastWriteTime);
				String aLastModifiedStr = aLastModified.ToString("ddd, dd MMM yyyy HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US")) + " GMT";
				Debug.WriteLine("SendResponseThumb() aLastModifiedStr: " + aLastModifiedStr);

				// ヘッダー
				String aHeader = "HTTP/1.1 200 OK\n"
						+ "Content-Length: " + aCacheThumb.Image.Length + "\n"
						+ "Content-Type: image/jpeg\n"
						+ "Last-Modified: " + aLastModifiedStr + "\n\n";
				Byte[] aSendBytes = Encoding.UTF8.GetBytes(aHeader);
				oNetworkStream.Write(aSendBytes, 0, aSendBytes.Length);

				// サムネイルデータ
				oNetworkStream.Write(aCacheThumb.Image, 0, aCacheThumb.Image.Length);
			}
			catch (Exception oExcep)
			{
				SendErrorResponse(oWriter, oExcep.Message);
			}
		}

		// --------------------------------------------------------------------
		// 外部リクエストにより Web サーバーのタスクが起動されすぎると、他のタスクの起動に悪影響が出るため、Web サーバーのタスク上限を決める
		// --------------------------------------------------------------------
		private void SetWebServerTasksLimit()
		{
			// アプリケーション全体での、直ちに起動できるタスク数を APP_WORKER_THREADS 以上に引き上げる
			ThreadPool.GetMinThreads(out Int32 aWorkerThreads, out Int32 aCompletionPortThreads);
			if (aWorkerThreads < APP_WORKER_THREADS)
			{
				ThreadPool.SetMinThreads(APP_WORKER_THREADS, aCompletionPortThreads);
			}

			// Web サーバーのタスク上限を設定
			mWebServerTasksLimit = APP_WORKER_THREADS - GENERAL_WORKER_THREADS;
		}

		// --------------------------------------------------------------------
		// ゆかり用のプレビュー機能を提供する（サーバーとして動作）
		// ワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void WebServerByWorker(Object oDummy)
		{
			TcpListener aListener = null;
			try
			{
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "プレビュータスク開始");
				SetWebServerTasksLimit();

				// IPv4 と IPv6 の全ての IP アドレスを Listen する
				aListener = new TcpListener(IPAddress.IPv6Any, mEnvironment.YukaListerSettings.WebServerPort);
				aListener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
				aListener.Start();
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "IP アドレス：" + ((IPEndPoint)aListener.LocalEndpoint).Address
							+ ", ポート：" + ((IPEndPoint)aListener.LocalEndpoint).Port);

				Int32 aNumWebServerTasks = 0;
				for (; ; )
				{
					try
					{
						// 接続要求があったら受け入れる
						// ToDo: タイムアウト付きにして CancelToken 判定
						TcpClient aClient = aListener.AcceptTcpClient();

						// タスク上限を超えないように調整
						while (aNumWebServerTasks >= mWebServerTasksLimit)
						{
							mToken.ThrowIfCancellationRequested();
							Thread.Sleep(Common.GENERAL_SLEEP_TIME);
						}

						// タスク数として数えた上でタスク実行
						Interlocked.Increment(ref aNumWebServerTasks);
						Task.Run(() =>
						{
							SendResponse(aClient);
							Interlocked.Decrement(ref aNumWebServerTasks);
						});
					}
					catch (Exception oExcep)
					{
						mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー接続ループエラー（リトライします）：\n" + oExcep.Message, true);
						mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
					}

					mToken.ThrowIfCancellationRequested();
				}
			}
			catch (OperationCanceledException)
			{
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "プレビュー処理を終了しました。");
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー処理エラー：\n" + oExcep.Message, true);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				if (aListener != null)
				{
					aListener.Stop();
				}
			}
		}

	}
	// public class WebServer ___END___

}
// namespace YukaLister.Models.Http ___END___
