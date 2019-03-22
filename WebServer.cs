// ============================================================================
// 
// ゆかり用の Web サーバー機能
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 【アクセス仕様】
// ・サムネイル画像取得
//   <アドレス>:<ポート>/thumb?uid=<ファイル番号>[&width=<横幅>]
//   http://localhost:13582/thumb?uid=7&width=80
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
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

namespace YukaLister.Shared
{
	public class WebServer
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public WebServer(YukaListerSettings oYukaListerSettings, CancellationToken oCancellationToken, LogWriter oLogWriter)
		{
			mYukaListerSettings = oYukaListerSettings;
			mCancellationToken = oCancellationToken;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 非同期に実行開始
		// --------------------------------------------------------------------
		public Task RunAsync()
		{
			return YlCommon.LaunchTaskAsync<Object>(WebServerByWorker, mTaskLockWebServer, null);
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// 直ちに起動できるタスクの数（アプリケーション全体）
		private const Int32 APP_WORKER_THREADS = 32;

		// Web サーバー以外用に残しておくタスクの数
		private const Int32 GENERAL_WORKER_THREADS = 1;

		// サムネイルのアスペクト比
		private const Double THUMB_ASPECT_RATIO = 16.0 / 9;

		// コマンド
		private const String SERVER_COMMAND_THUMB = "thumb";

		// コマンドオプション
		private const String OPTION_NAME_UID = "uid";

		// サムネイル生成時のタイムアウト [ms]
		private const Int32 THUMB_TIMEOUT = 10 * 1000;

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// タスク上限
		private Int32 mWebServerTasksLimit;

		// 環境設定
		private YukaListerSettings mYukaListerSettings;

		// キャンセル用
		private CancellationToken mCancellationToken;

		// ログ
		private LogWriter mLogWriter;

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
		// サムネイルを JPEG 形式で作成
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private JpegBitmapEncoder CreateThumb(Dictionary<String, String> oOptions)
		{
			if (!oOptions.ContainsKey(OPTION_NAME_UID))
			{
				throw new Exception("Parameter " + OPTION_NAME_UID + " is not specified.");
			}
			Int32 aUid = Int32.Parse(oOptions[OPTION_NAME_UID]);

			TFound aTarget = null;
			using (DataContext aYukariDbContext = new DataContext(YlCommon.YukariDbInMemoryConnection))
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

			// 動画を開いてすぐに一時停止する
			MediaPlayer aPlayer = new MediaPlayer
			{
				IsMuted = true,
				ScrubbingEnabled = true,
			};
			aPlayer.Open(new Uri(aTarget.Path, UriKind.Absolute));
			aPlayer.Play();
			aPlayer.Pause();

			// 指定位置へシーク
			aPlayer.Position = TimeSpan.FromSeconds(mYukaListerSettings.ThumbSeekPos);
			Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] CreateThumb() Position: " + aPlayer.Position.ToString() + " " + Environment.TickCount.ToString("#,0"));

			// 読み込みが完了するまで待機
			Int32 aTick = Environment.TickCount;
			while (aPlayer.DownloadProgress < 1.0 ||/* aPlayer.IsBuffering || aPlayer.BufferingProgress < 1.0 ||*/ aPlayer.NaturalVideoWidth == 0)
			{
				Thread.Sleep(Common.GENERAL_SLEEP_TIME);
				if (Environment.TickCount - aTick > THUMB_TIMEOUT)
				{
					break;
				}
			}
#if true
			if (!aPlayer.HasVideo)
			{
				throw new Exception("Movie doesn't have any video.");
			}
#endif

			// 生成するサムネイルのサイズを計算
			Int32 aThumbWidth = mYukaListerSettings.ThumbDefaultWidth;
			Int32 aThumbHeight = (Int32)(aThumbWidth / THUMB_ASPECT_RATIO);
			Debug.WriteLine("CreateThumb() Thumb size: " + aThumbWidth + " x " + aThumbHeight + " " + Environment.TickCount.ToString("#,0"));

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
			Debug.WriteLine("CreateThumb() Resize size: " + aResizeWidth + " x " + aResizeHeight);

			// 描画用の Visual に動画を描画
			DrawingVisual aVisual = new DrawingVisual();
			using (DrawingContext aContext = aVisual.RenderOpen())
			{
				aContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, aThumbWidth, aThumbHeight));
				aContext.DrawVideo(aPlayer, new Rect((aThumbWidth - aResizeWidth) / 2, (aThumbHeight - aResizeHeight) / 2, aResizeWidth, aResizeHeight));
			}

			// ビットマップに Visual を描画
			aTick = Environment.TickCount;
			RenderTargetBitmap aBitmap = new RenderTargetBitmap(aThumbWidth, aThumbHeight, 96, 96, PixelFormats.Pbgra32);
			for (; ; )
			{
				aBitmap.Render(aVisual);
				if (IsRenderDone(aBitmap))
				{
					Debug.WriteLine("CreateThumb() render done time: " + (Environment.TickCount - aTick));
					break;
				}
				Thread.Sleep(Common.GENERAL_SLEEP_TIME);
				if (Environment.TickCount - aTick > THUMB_TIMEOUT)
				{
					break;
				}
			}

			// JPEG にエンコード
			JpegBitmapEncoder aEncoder = new JpegBitmapEncoder();
			aEncoder.Frames.Add(BitmapFrame.Create(aBitmap));
			return aEncoder;
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
			//Debug.WriteLine("IsRenderDone() aWidth: " + aWidth + ", aHeight: " + aHeight + ", aPixels: " + aPixels.Length + ", aOffset: " + aOffset);
			//Debug.WriteLine("IsRenderDone() BGR: " + aPixels[aOffset] + ", " + aPixels[aOffset + 1] + ", " + aPixels[aOffset + 2]);

			// RGB いずれかが 0 以外なら転写完了
			return aPixels[aOffset] != 0 || aPixels[aOffset + 1] != 0 || aPixels[aOffset + 2] != 0;
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
					while (aNumRetries < YlCommon.TCP_NUM_RETRIES)
					{
						try
						{
							oNetworkStream.Write(aBuf, 0, aReadSize);
							break;
						}
						catch (Exception oExcep)
						{
							mLogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー内容送信エラー：\n" + oExcep.Message + "\nリトライ回数：" + aNumRetries, true);
							mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
						}
						aNumRetries++;
						mCancellationToken.ThrowIfCancellationRequested();

						// 少し待ってみる
						Thread.Sleep(5 * 1000);
					}
					if (aNumRetries >= YlCommon.TCP_NUM_RETRIES)
					{
						throw new OperationCanceledException();
					}

#if DEBUG
					aReadSizes += aReadSize;
#endif
				}
			}

#if DEBUG
			mLogWriter.ShowLogMessage(TraceEventType.Verbose, "SendFile() sent: " + aReadSizes.ToString("#,0") + " / " + aFileInfo.Length.ToString("#,0"));
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
					aNetworkStream.ReadTimeout = YlCommon.TCP_TIMEOUT;
					aNetworkStream.WriteTimeout = YlCommon.TCP_TIMEOUT;

					// ヘッダー部分を読み込む
					Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] SendResponse() header");
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
					Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] SendResponse() aDocPath: " + aDocPath);
					if (String.IsNullOrEmpty(aDocPath))
					{
						throw new Exception("ヘッダーのパスが空です。");
					}

					// コマンド解析（先頭が '/' であることに注意）
					if (aDocPath.IndexOf(SERVER_COMMAND_THUMB) == 1)
					{
						SendResponseThumb(aNetworkStream, aWriter, aDocPath);
					}
					else
					{
						// パス解析（先頭の '/' を除く）
						String aPath = null;
						if (aDocPath.Length == 1)
						{
							SendErrorResponse(aWriter, "File is not specified.");
						}
						else
						{
#if DEBUGz
							Thread.Sleep(5 * 1000);
							SendErrorResponse(aWriter, "Test mode.");
							aWriter.Flush();
							return;
#endif
							aPath = YlCommon.ExtendPath(HttpUtility.UrlDecode(aDocPath, Encoding.UTF8).Substring(1).Replace('/', '\\'));
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

					aWriter.Flush();
				}
			}
			catch (OperationCanceledException)
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "プレビュー応答を中止しました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー応答エラー：\n" + oExcep.Message, true);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 閉じる
				oClient.Close();
			}
		}

		// --------------------------------------------------------------------
		// クライアントにサムネイルを返す
		// --------------------------------------------------------------------
		private void SendResponseThumb(NetworkStream oNetworkStream, StreamWriter oWriter, String oCommand)
		{
			try
			{
				Dictionary<String, String> aOptions = AnalyzeCommandOptions(oCommand);
				JpegBitmapEncoder aJpeg = CreateThumb(aOptions);

				using (MemoryStream aMemStream = new MemoryStream())
				{
					aJpeg.Save(aMemStream);
					Debug.WriteLine("SendResponseThumb() jpeg size: " + aMemStream.Length);

					// ヘッダー
					String aHeader = "HTTP/1.1 200 OK\n"
							+ "Content-Length: " + aMemStream.Length + "\n"
							+ "Content-Type: image/jpeg\n\n";
					Byte[] aSendBytes = Encoding.UTF8.GetBytes(aHeader);
					oNetworkStream.Write(aSendBytes, 0, aSendBytes.Length);

					// 画像
					Byte[] aJpegBytes = new Byte[aMemStream.Length];
					aMemStream.Seek(0, SeekOrigin.Begin);
					aMemStream.Read(aJpegBytes, 0, (Int32)aMemStream.Length);
					oNetworkStream.Write(aJpegBytes, 0, (Int32)aMemStream.Length);
				}
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
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "プレビュータスク開始");
				SetWebServerTasksLimit();

				// IPv4 と IPv6 の全ての IP アドレスを Listen する
				aListener = new TcpListener(IPAddress.IPv6Any, mYukaListerSettings.WebServerPort);
				aListener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
				aListener.Start();
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "IP アドレス：" + ((IPEndPoint)aListener.LocalEndpoint).Address
						+ ", ポート：" + ((IPEndPoint)aListener.LocalEndpoint).Port);

				Int32 aNumWebServerTasks = 0;
				for (; ; )
				{
					try
					{
						// 接続要求があったら受け入れる
						// ToDo: タイムアウト付きにして CancelToken 判定
						TcpClient aClient = aListener.AcceptTcpClient();
						Debug.WriteLine("WebServerByWorker() accept");

						// タスク上限を超えないように調整
						while (aNumWebServerTasks >= mWebServerTasksLimit)
						{
							Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] WebServerByWorker() タスク上限待機" + Environment.TickCount.ToString("#,0"));
							mCancellationToken.ThrowIfCancellationRequested();
							Thread.Sleep(Common.GENERAL_SLEEP_TIME);
						}

						// タスク数として数えた上でタスク実行
						Interlocked.Increment(ref aNumWebServerTasks);
						Task.Run(() =>
						{
							Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] WebServerByWorker() タスク開始" + Environment.TickCount.ToString("#,0"));
							SendResponse(aClient);
							Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] WebServerByWorker() タスク終了" + Environment.TickCount.ToString("#,0"));
							Interlocked.Decrement(ref aNumWebServerTasks);
						});
					}
					catch (Exception oExcep)
					{
						mLogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー接続ループエラー（リトライします）：\n" + oExcep.Message, true);
						mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
					}

					mCancellationToken.ThrowIfCancellationRequested();
				}
			}
			catch (OperationCanceledException)
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "プレビュー処理を終了しました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー処理エラー：\n" + oExcep.Message, true);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
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
	// public class WebServer

}
// namespace YukaLister.Shared
