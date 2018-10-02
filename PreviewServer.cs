// ============================================================================
// 
// ゆかりのプレビュー用サーバー
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace YukaLister.Shared
{
	public class PreviewServer
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public PreviewServer(YukaListerSettings oYukaListerSettings, CancellationToken oCancellationToken, LogWriter oLogWriter)
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
			return YlCommon.LaunchTaskAsync<Object>(PreviewByWorker, mTaskLockPreview, null);
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// パス識別子
		private const String PATH_BEGIN_MARK = "Path=";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 環境設定
		private YukaListerSettings mYukaListerSettings;

		// キャンセル用
		private CancellationToken mCancellationToken;

		// ログ
		private LogWriter mLogWriter;

		// 排他制御
		private static Object mTaskLockPreview = new Object();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ゆかり用のプレビュー機能を提供する（サーバーとして動作）
		// ワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void PreviewByWorker(Object oDummy)
		{
			TcpListener aListener = null;
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "プレビュータスク開始");

				// IPv4 と IPv6 の全ての IP アドレスを Listen する
				aListener = new TcpListener(IPAddress.IPv6Any, mYukaListerSettings.YukariPreviewPort);
				aListener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
				aListener.Start();
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "IP アドレス：" + ((IPEndPoint)aListener.LocalEndpoint).Address
						+ ", ポート：" + ((IPEndPoint)aListener.LocalEndpoint).Port);

				for (; ; )
				{
					try
					{
						// 接続要求があったら受け入れる
						TcpClient aClient = aListener.AcceptTcpClient();
						SendResponseAsync(aClient);
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
		private void SendResponseAsync(TcpClient oClient)
		{
			Task.Run(() =>
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

						// ヘッダーの 1 行目は [GET|POST] /[Path] HTTP/1.1 のようになっている
						String[] aRequests = aHeaders[0].Split(' ');
						if (aRequests.Length < 3 || String.IsNullOrEmpty(aRequests[1]))
						{
							throw new Exception("ヘッダーでパスが指定されていません。");
						}

						// パス解析（先頭の '/' を除く）
						String aPath = null;
						if (aRequests[1].Length == 1)
						{
							SendErrorResponse(aWriter, "File is not specified.");
						}
						else
						{
							aPath = YlCommon.ExtendPath(HttpUtility.UrlDecode(aRequests[1], Encoding.UTF8).Substring(1).Replace('/', '\\'));
							if (File.Exists(aPath))
							{
								SendFile(aNetworkStream, aPath);
							}
							else
							{
								SendErrorResponse(aWriter, "File not found.");
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
			});
		}

	}
	// public class PreviewServer

}
// namespace YukaLister.Shared
