// ============================================================================
// 
// ゆかりすたーのロジック本体
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ・ターゲットフレームワークを .NET 4.5 にしつつ extended-length なパスを使うには、App.config に AppContextSwitchOverrides が必要
// ・外部に書き出すパスはすべて extended-length なパス表記ではないものにする
// ----------------------------------------------------------------------------

using Livet;
using Livet.Messaging;

using Shinta;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using YukaLister.Models.Database;
using YukaLister.Models.Http;
using YukaLister.ViewModels;

namespace YukaLister.Models
{
	public class YukaListerModel : NotificationObject
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public YukaListerModel(MainWindowViewModel oMainWindowViewModel)
		{
			mMainWindowViewModel = oMainWindowViewModel;
			YukariDb = new YukariDatabaseModel(Environment, oMainWindowViewModel);
			Report = new ReportModel(Environment, oMainWindowViewModel);
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 環境設定
		public EnvironmentModel Environment { get; private set; } = new EnvironmentModel();

		// ゆかり用データベース
		public YukariDatabaseModel YukariDb { get; private set; }

		// リスト問題
		public ReportModel Report { get; private set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void ButtonYukaListerSettingsClicked()
		{
			String aYukariConfigPathBak = Environment.YukaListerSettings.YukariConfigPath();
			Boolean aProvideYukariPreviewBak = Environment.YukaListerSettings.ProvideYukariPreview;
			Boolean aSyncMusicInfoDbBak = Environment.YukaListerSettings.SyncMusicInfoDb;
			String aSyncServerBak = Environment.YukaListerSettings.SyncServer;
			String aSyncAccountBak = Environment.YukaListerSettings.SyncAccount;
			String aSyncPasswordBak = Environment.YukaListerSettings.SyncPassword;
			DateTime aMusicInfoDbTimeBak;
			Boolean aRegetSyncDataNeeded;

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			{
				aMusicInfoDbTimeBak = aMusicInfoDbInDisk.LastWriteTime();
			}

			// ViewModel 経由でウィンドウを開く
			using (YukaListerSettingsWindowViewModel aYukaListerSettingsWindowViewModel = new YukaListerSettingsWindowViewModel())
			{
				aYukaListerSettingsWindowViewModel.Environment = Environment;
				aYukaListerSettingsWindowViewModel.YukariListDbInMemory = YukariDb.YukariListDbInMemory;
				mMainWindowViewModel.Messenger.Raise(new TransitionMessage(aYukaListerSettingsWindowViewModel, "OpenYukaListerSettingsWindow"));

				if (aYukaListerSettingsWindowViewModel.IsOk)
				{
					mMainWindowViewModel.SetStatusBarMessageWithInvoke(TraceEventType.Information, "環境設定を変更しました。");
				}
				aRegetSyncDataNeeded = aYukaListerSettingsWindowViewModel.RegetSyncDataNeeded;
			}

			// ゆかり設定ファイルのフルパスが変更された場合は処理を行う
			if (Environment.YukaListerSettings.YukariConfigPath() != aYukariConfigPathBak)
			{
				Environment.YukaListerSettings.AnalyzeYukariEasyAuthConfig(Environment);
				SetFileSystemWatcherYukariConfig();
				YukariDb.YukariConfigPathChanged();
			}

			// サーバー設定が変更された場合は起動・終了を行う
			if (Environment.YukaListerSettings.ProvideYukariPreview != aProvideYukariPreviewBak)
			{
				if (Environment.YukaListerSettings.ProvideYukariPreview)
				{
					RunPreviewServerIfNeeded();
				}
				else
				{
					StopPreviewServerIfNeeded();
				}
			}

			if (aRegetSyncDataNeeded)
			{
				// 再取得が指示された場合は再取得
				YukariDb.RunSyncClientIfNeeded(true);
			}
			else
			{
				// 同期設定が変更された場合・インポートで楽曲情報データベースが更新された場合は同期を行う
				DateTime aMusicInfoDbTime;
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					aMusicInfoDbTime = aMusicInfoDbInDisk.LastWriteTime();
				}
				if (Environment.YukaListerSettings.SyncMusicInfoDb != aSyncMusicInfoDbBak
						|| Environment.YukaListerSettings.SyncServer != aSyncServerBak
						|| Environment.YukaListerSettings.SyncAccount != aSyncAccountBak
						|| Environment.YukaListerSettings.SyncPassword != aSyncPasswordBak
						|| aMusicInfoDbTime != aMusicInfoDbTimeBak)
				{
					YukariDb.RunSyncClientIfNeeded();
				}
			}
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		public void Initialize()
		{
			YukariDb.Initialize();
			Report.Initialize();

			// ゆかり設定ファイル監視
			mFileSystemWatcherYukariConfig = new FileSystemWatcher();
			mFileSystemWatcherYukariConfig.Created += new FileSystemEventHandler(FileSystemWatcherYukariConfig_Changed);
			mFileSystemWatcherYukariConfig.Deleted += new FileSystemEventHandler(FileSystemWatcherYukariConfig_Changed);
			mFileSystemWatcherYukariConfig.Changed += new FileSystemEventHandler(FileSystemWatcherYukariConfig_Changed);
			SetFileSystemWatcherYukariConfig();

			// ゆかり用データベース初期化後に実行
			RunPreviewServerIfNeeded();
		}

		// --------------------------------------------------------------------
		// 終了処理
		// --------------------------------------------------------------------
		public void Quit()
		{
			// 終了時タスクキャンセル
			Environment.AppCancellationTokenSource.Cancel();
			Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "終了処理中...");

			// 子要素終了処理
			StopPreviewServerIfNeeded();
			YukariDb.Quit();
			Environment.Quit();
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// VM
		private MainWindowViewModel mMainWindowViewModel;

		// Web サーバー（null のままのこともあり得る）
		private WebServer mWebServer;

		// Web サーバー終了用
		private CancellationTokenSource mWebServerTokenSource;

		// config.ini 監視用
		private FileSystemWatcher mFileSystemWatcherYukariConfig;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void FileSystemWatcherYukariConfig_Changed(Object oSender, FileSystemEventArgs oFileSystemEventArgs)
		{
			mMainWindowViewModel.SetStatusBarMessageWithInvoke(TraceEventType.Information, "ゆかり設定ファイルが更新されました。");
			Environment.YukaListerSettings.AnalyzeYukariEasyAuthConfig(Environment);
		}

		// --------------------------------------------------------------------
		// プレビュー設定が有効ならプレビュー用サーバーを開始
		// --------------------------------------------------------------------
		private void RunPreviewServerIfNeeded()
		{
			if (!Environment.YukaListerSettings.ProvideYukariPreview)
			{
				return;
			}
			if (mWebServer != null)
			{
				return;
			}

			mWebServerTokenSource = new CancellationTokenSource();
			mWebServer = new WebServer(Environment, YukariDb.YukariListDbInMemory, mWebServerTokenSource.Token);

			// async を待機しない
			Task aSuppressWarning = mWebServer.RunAsync();
		}

		// --------------------------------------------------------------------
		// ゆかり設定ファイルの監視設定
		// --------------------------------------------------------------------
		private void SetFileSystemWatcherYukariConfig()
		{
			if (Environment.YukaListerSettings.IsYukariConfigPathSet())
			{
				mFileSystemWatcherYukariConfig.Path = Path.GetDirectoryName(Environment.YukaListerSettings.YukariConfigPath());
				mFileSystemWatcherYukariConfig.Filter = Path.GetFileName(Environment.YukaListerSettings.YukariConfigPath());
				mFileSystemWatcherYukariConfig.EnableRaisingEvents = true;
			}
			else
			{
				mFileSystemWatcherYukariConfig.EnableRaisingEvents = false;
			}
		}

		// --------------------------------------------------------------------
		// プレビュー用サーバーが実行中なら終了
		// --------------------------------------------------------------------
		private void StopPreviewServerIfNeeded()
		{
			if (mWebServer != null)
			{
				mWebServerTokenSource.Cancel();
				mWebServerTokenSource = null;

				// async を待機しない
				Task aSuppressWarning = mWebServer.StopAsync();
				mWebServer = null;
			}
		}


	}
	// public class YukaListerModel ___END___
}
// namespace YukaLister.Models ___END___
