﻿// ============================================================================
// 
// 各種データベースを管理する
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ・mTargetFolderInfos にアクセスする時は mTargetFolderInfos をロックする
// ・UI スレッドとワーカースレッドで mTargetFolderInfos のロックと UI スレッドの占有（Dispatcher.Invoke()）の順序が異なるとデッドロックになる
// ・データバインディング機構は恐らく UI スレッドを使う
// 　→ワーカースレッドで Dispatcher.Invoke() とロックをしたい場合は、Dispatcher.Invoke() してからロックする
// 　　（mTargetFolderInfos をロックしながら Dispatcher.Invoke() するのはダメ）
// 　　（mTargetFolderInfos をロックしながら mLogWriter.ShowLogMessage() でメッセージボックスを表示するのもダメ）
// 　→UI スレッドでロックをしたい場合はそのままロックする
// ・mTargetFolderInfosVisible は UI スレッドのみがアクセスするようにし、ロックは不要
// ----------------------------------------------------------------------------

using Livet;
using Livet.Messaging;

using Shinta;
using Shinta.Behaviors;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using YukaLister.Models.Database;
using YukaLister.Models.Http;
using YukaLister.Models.OutputWriters;
using YukaLister.Models.SharedMisc;
using YukaLister.ViewModels;

namespace YukaLister.Models
{
	public class YukariDatabaseModel : NotificationObject
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public YukariDatabaseModel(EnvironmentModel oEnvironment, MainWindowViewModel oMainWindowViewModel)
		{
			mEnvironment = oEnvironment;
			mMainWindowViewModel = oMainWindowViewModel;

			// TargetFolderInfo スタティックプロパティー設定
			TargetFolderInfo.Environment = mEnvironment;
			TargetFolderInfo.YukariDbYukaListerStatus = YukariDbYukaListerStatusFunc;
			TargetFolderInfo.IsOpenChanged = TargetFolderInfoIsOpenChangedFunc;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ゆかり用リストデータベース（作業用インメモリ）：閉じると消滅するのでアプリ起動中ずっと開きっぱなし
		public YukariListDatabaseInMemory YukariListDbInMemory { get; private set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// ＜引数＞ oParentFolder: （extended-length ではない）通常表記
		// --------------------------------------------------------------------
		public void AddFolderSelected(String oParentFolderShLen)
		{
			if (String.IsNullOrEmpty(oParentFolderShLen))
			{
				return;
			}

			// async を待機しない
			Task aSuppressWarning = YlCommon.LaunchTaskAsync(AddTargetFolderByWorker, mGeneralTaskLock, oParentFolderShLen, mEnvironment.LogWriter);
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void ButtonFolderSettingsClicked()
		{
			FolderSettings();
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void ButtonTFoundsClicked()
		{
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			{
				DateTime aMusicInfoDbTimeBak = aMusicInfoDbInDisk.LastWriteTime();

				// ViewModel 経由でウィンドウを開く
				using (ViewTFoundsWindowViewModel aViewTFoundsWindowViewModel = new ViewTFoundsWindowViewModel())
				{
					aViewTFoundsWindowViewModel.Environment = mEnvironment;
					aViewTFoundsWindowViewModel.YukariListDbInMemory = YukariListDbInMemory;
					mMainWindowViewModel.Messenger.Raise(new TransitionMessage(aViewTFoundsWindowViewModel, "OpenViewTFoundsWindow"));
				}

				// 楽曲情報データベースが更新された場合は同期を行う
				DateTime aMusicInfoDbTime = aMusicInfoDbInDisk.LastWriteTime();
				if (aMusicInfoDbTime != aMusicInfoDbTimeBak)
				{
					RunSyncClientIfNeeded();
				}
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void ButtonRemoveTargetFolderClicked()
		{
			TargetFolderInfo aTargetFolderInfo = mMainWindowViewModel.SelectedTargetFolderInfo;
			if (aTargetFolderInfo == null)
			{
				return;
			}

			if (MessageBox.Show(mEnvironment.ShortenPath(aTargetFolderInfo.ParentPath) + "\nおよびサブフォルダーをゆかり検索対象から削除しますか？",
					"確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
			{
				return;
			}

			// async を待機しない
			Task aSuppressWarning = YlCommon.LaunchTaskAsync(RemoveTargetFolderByWorker, mGeneralTaskLock, aTargetFolderInfo.ParentPath, mEnvironment.LogWriter);
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void DeviceChange(DeviceChangeInfo oDeviceChangeInfo)
		{
			switch (oDeviceChangeInfo.Kind)
			{
				case DBT.DBT_DEVICEARRIVAL:
					mMainWindowViewModel.SetStatusBarMessageWithInvoke(Common.TRACE_EVENT_TYPE_STATUS, "リムーバブルドライブが接続されました：" + oDeviceChangeInfo.DriveLetter);
					DeviceArrival(oDeviceChangeInfo.DriveLetter);
					break;
				case DBT.DBT_DEVICEREMOVECOMPLETE:
					mMainWindowViewModel.SetStatusBarMessageWithInvoke(Common.TRACE_EVENT_TYPE_STATUS, "リムーバブルドライブが切断されました：" + oDeviceChangeInfo.DriveLetter);
					DeviceRemoveComplete(oDeviceChangeInfo.DriveLetter);
					break;
			}
		}

		// --------------------------------------------------------------------
		// 初期化
		// 時間のかかる処理やコンストラクターでは無効となってしまう処理を行う
		// --------------------------------------------------------------------
		public void Initialize()
		{
			// タイマー
			mTimerUpdateDg = new DispatcherTimer();
			mTimerUpdateDg.Interval = new TimeSpan(0, 0, 1);
			mTimerUpdateDg.Tick += new EventHandler(TimerUpdateDg_Tick);

			// 楽曲情報データベース等構築
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			{
				aMusicInfoDbInDisk.CreateDatabaseIfNeeded();
			}
			using (YukariThumbnailDatabaseInDisk aYukariThumbDbInDisk = new YukariThumbnailDatabaseInDisk(mEnvironment))
			{
				aYukariThumbDbInDisk.CreateDatabaseIfNeeded();
			}

			// ゆかり用データベース構築
			CreateYukariDb();

			// ゆかり用リストデータベース構築状況設定（全般用）
			// コンストラクターで実行してもリスナーがいないので無意味のため、Initialize() で実行
			SetYukaListerStatus();

			// 前回のリストが残っていてリクエストできない曲がリスト化されるのを防ぐために、前回のリストを削除
			if (mEnvironment.YukaListerSettings.ClearPrevList)
			{
				// リストタスク実行（async を待機しない）
				Task aSuppressWarning = OutputYukariListAsync();
			}
			else
			{
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "前回のゆかり用リストをクリアしませんでした。");
			}

			// 終了時に追加されていたフォルダーを追加
			String[] aDrives = Directory.GetLogicalDrives();
			foreach (String aDrive in aDrives)
			{
				DeviceArrival(aDrive.Substring(0, 2));
			}

			// 同期
			RunSyncClientIfNeeded();
		}

		// --------------------------------------------------------------------
		// 終了処理
		// --------------------------------------------------------------------
		public void Quit()
		{

		}

		// --------------------------------------------------------------------
		// 同期設定が有効なら同期処理を開始
		// ToDo: 競合回避をきれいなやりかたにする
		// --------------------------------------------------------------------
		public async void RunSyncClientIfNeeded(Boolean oIsReget = false)
		{
			if (!mEnvironment.YukaListerSettings.SyncMusicInfoDb)
			{
				return;
			}

			SyncClient aSyncClient = new SyncClient(mEnvironment, mMainWindowViewModel, oIsReget);

			if (oIsReget)
			{
				// 楽曲情報データベース初期化中にフォルダータスクが楽曲情報データベースにアクセスするとアクセスできないので競合を回避
				mIsMusicInfoDbInDiskCreating = true;
				SetYukaListerStatus();
				lock (mFolderTaskLock)
				{
				}
				await aSyncClient.RunAsync();
				mIsMusicInfoDbInDiskCreating = false;
				SetYukaListerStatus();

				// フォルダータスク実行（async を待機しない）
				Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(DoFolderTaskByWorker, mFolderTaskLock, null, mEnvironment.LogWriter);
			}
			else
			{
				// async を待機しない
				Task aSuppressWarning = aSyncClient.RunAsync();
			}
		}

		// --------------------------------------------------------------------
		// ゆかりすたーの動作状況とその表示を更新
		// エラーを最初に判定する
		// --------------------------------------------------------------------
		public void SetYukaListerStatus()
		{
			if (mEnvironment.AppCancellationTokenSource.Token.IsCancellationRequested)
			{
				return;
			}

			if (!mEnvironment.YukaListerSettings.IsYukariConfigPathSet())
			{
				// 設定ファイルエラー
				mMainWindowViewModel.YukaListerDbStatus = YukaListerStatus.Error;
				mMainWindowViewModel.YukaListerStatusMessage = "ゆかり設定ファイルが正しく指定されていません。";
			}
			else if (!File.Exists(FILE_NAME_APP_CONFIG))
			{
				// アプリケーション構成ファイルエラー
				mMainWindowViewModel.YukaListerDbStatus = YukaListerStatus.Error;
				mMainWindowViewModel.YukaListerStatusMessage = "アプリケーション構成ファイルが見つかりません。\n" + YlConstants.APP_NAME_J + "を再インストールして下さい。";
			}
			else if (mIsMusicInfoDbInDiskCreating)
			{
				// 楽曲情報データベース構築中のため待機
				mMainWindowViewModel.YukaListerDbStatus = YukaListerStatus.Error;
				mMainWindowViewModel.YukaListerStatusMessage = "楽曲情報データベースの構築を待機中...";
			}
			else
			{
				// エラーがなかったので、一旦待機中を仮定
				mMainWindowViewModel.YukaListerDbStatus = YukaListerStatus.Ready;
				mMainWindowViewModel.YukaListerStatusMessage = YlConstants.APP_NAME_J + "は正常に動作しています。";

				// 実行中のものがある場合は実行中にする
				for (Int32 i = 0; i < (Int32)YukaListerStatusRunningMessage.__End__; i++)
				{
					if (mEnabledYukaListerStatusRunningMessages[i])
					{
						mMainWindowViewModel.YukaListerDbStatus = YukaListerStatus.Running;
						mMainWindowViewModel.YukaListerStatusMessage = YlConstants.YUKA_LISTER_STATUS_RUNNING_MESSAGES[i];
						break;
					}
				}
			}

			// 動作状況更新
			mMainWindowViewModel.YukaListerStatusSubMessage = null;
			if (mMainWindowViewModel.YukaListerDbStatus == YukaListerStatus.Error)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, mMainWindowViewModel.YukaListerStatusMessage, true);
			}

			// メッセージ
			mMainWindowViewModel.RaiseLazyPropertyChanged();

			// タイマー設定
			mTimerUpdateDg.IsEnabled = (mMainWindowViewModel.YukaListerDbStatus == YukaListerStatus.Running);
		}

		// --------------------------------------------------------------------
		// IsOpen が変更された（TargetFolderInfo 用）
		// --------------------------------------------------------------------
		public void TargetFolderInfoIsOpenChangedFunc(TargetFolderInfo oTargetFolderInfo)
		{
			lock (mTargetFolderInfos)
			{
				Int32 aIndex = FindTargetFolderInfo2Ex3All(oTargetFolderInfo.Path);
				if (aIndex >= 0)
				{
					mTargetFolderInfos[aIndex].IsOpen = oTargetFolderInfo.IsOpen;

					// アコーディオン開閉（子アイテムの表示状況変更）
					Boolean aIsVisible = (Boolean)oTargetFolderInfo.IsOpen;
					for (Int32 i = aIndex + 1; i < aIndex + mTargetFolderInfos[aIndex].NumTotalFolders; i++)
					{
						mTargetFolderInfos[i].Visible = aIsVisible;
					}

					UpdateTargetFolderInfosVisible();
				}
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void YukariConfigPathChanged()
		{
			SetYukaListerStatus();

			if (mMainWindowViewModel.YukaListerDbStatus == YukaListerStatus.Error)
			{
				MakeAllFolderTasksQueued();
			}
			else
			{
				// 新しい設定でゆかり用データベースを準備する
				CreateYukariDb();

				// フォルダータスクやり直し
				MakeAllFolderTasksQueued();

				// フォルダータスク実行（async を待機しない）
				Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(DoFolderTaskByWorker, mFolderTaskLock, null, mEnvironment.LogWriter);
			}

		}

		// --------------------------------------------------------------------
		// ゆかり用リストデータベース構築状況（TargetFolderInfo 用）
		// --------------------------------------------------------------------
		public YukaListerStatus YukariDbYukaListerStatusFunc()
		{
			return mMainWindowViewModel.YukaListerDbStatus;
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// アプリケーション構成ファイル
		private const String FILE_NAME_APP_CONFIG = "YukaLister.exe.config";

		// 自動追加情報記録ファイル名
		private const String FILE_NAME_AUTO_TARGET_INFO = YlConstants.APP_ID + "AutoTarget" + Common.FILE_EXT_CONFIG;

		// スマートトラック判定用の単語（小文字表記、両端を | で括る）
		private const String OFF_VOCAL_WORDS = "|cho|cut|dam|guide|guidevocal|inst|joy|off|offcho|offvocal|offのみ|vc|オフ|オフボ|オフボーカル|ボイキャン|ボーカルキャンセル|配信|";
		private const String BOTH_VOCAL_WORDS = "|2tr|2ch|onoff|offon|";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 環境設定類
		private EnvironmentModel mEnvironment;

		// VM
		private MainWindowViewModel mMainWindowViewModel;

		// ゆかり用リストデータベース構築状況（各作業を行っているか）
		private Boolean[] mEnabledYukaListerStatusRunningMessages = new Boolean[(Int32)YukaListerStatusRunningMessage.__End__];

		// ゆかり検索対象フォルダー（全部）：この中から絞って VM の表示用に渡す
		private List<TargetFolderInfo> mTargetFolderInfos = new List<TargetFolderInfo>();

		// DataGrid 表示更新判定用
		// 複数スレッドからロックせずアクセスされるので、一時的に整合性がとれなくなっても最終的にまともに DataGrid が表示されることが必要
		private Boolean mDirtyDg;

		// タイマー
		DispatcherTimer mTimerUpdateDg;

		// 楽曲情報データベース構築待機中
		private Boolean mIsMusicInfoDbInDiskCreating;

		// メインウィンドウ上で時間のかかるタスクが多重起動されるのを抑止する
		private Object mGeneralTaskLock = new Object();

		// フォルダータスクが多重起動されるのを抑止する
		private Object mFolderTaskLock = new Object();

		// リストタスクが多重起動されるのを抑止する
		private Object mListTaskLock = new Object();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ファイルの情報を検索してゆかり用データベースに追加
		// FindNicoKaraFiles() で追加されない情報をすべて付与する
		// ファイルは再帰検索しない
		// --------------------------------------------------------------------
		private void AddNicoKaraInfo(String oFolderPathExLen)
		{
			// フォルダー設定を読み込む
			FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings2Ex(oFolderPathExLen);
			FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
			String aFolderPathLower = mEnvironment.ShortenPath(oFolderPathExLen).ToLower();

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (SQLiteCommand aMusicInfoDbCmd = new SQLiteCommand(aMusicInfoDbInDisk.Connection))
			using (DataContext aMusicInfoDbContext = new DataContext(aMusicInfoDbInDisk.Connection))
			using (DataContext aYukariDbContext = new DataContext(YukariListDbInMemory.Connection))
			{
				Table<TFound> aTableFound = aYukariDbContext.GetTable<TFound>();
				IQueryable<TFound> aQueryResult =
						from x in aTableFound
						where x.Folder == aFolderPathLower
						select x;

				// カテゴリー正規化用
				List<String> aCategoryNames = YlCommon.SelectCategoryNames(aMusicInfoDbInDisk.Connection);

				// 情報付与
				foreach (TFound aRecord in aQueryResult)
				{
					FileInfo aFileInfo = new FileInfo(mEnvironment.ExtendPath(aRecord.Path));
					aRecord.LastWriteTime = JulianDay.DateTimeToModifiedJulianDate(aFileInfo.LastWriteTime);
					aRecord.FileSize = aFileInfo.Length;
					SetTFoundValue(aRecord, aFolderSettingsInMemory, aMusicInfoDbCmd, aMusicInfoDbContext, aCategoryNames);
				}

				// コミット
				aYukariDbContext.SubmitChanges();

				mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
			}
		}

		// --------------------------------------------------------------------
		// フォルダー（サブフォルダー含む）を対象フォルダーに追加
		// 汎用ワーカースレッドで実行されることが前提
		// ＜引数＞ oParentFolder: （extended-length ではない）通常表記
		// --------------------------------------------------------------------
		private void AddTargetFolderByWorker(String oParentFolderShLen)
		{
			try
			{
				// 正当性の確認
				if (String.IsNullOrEmpty(oParentFolderShLen))
				{
					return;
				}

				Debug.Assert(!oParentFolderShLen.StartsWith(YlConstants.EXTENDED_LENGTH_PATH_PREFIX), "AddTargetFolderByWorker() not ShLen");

				// "E:" のような '\\' 無しのドライブ名は挙動が変なので 3 文字以上を対象とする
				if (oParentFolderShLen.Length < 3)
				{
					return;
				}

				String aParentFolderExLen = mEnvironment.ExtendPath(oParentFolderShLen);
				if (!Directory.Exists(aParentFolderExLen))
				{
					mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, oParentFolderShLen + " が見つかりません。", true);
					return;
				}

				// 準備
				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.AddTargetFolder] = true;
				SetYukaListerStatus();
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, oParentFolderShLen + " とそのサブフォルダーを検索対象に追加予定としています...");

				// 親の重複チェック
				Boolean aParentAdded;
				lock (mTargetFolderInfos)
				{
					aParentAdded = FindTargetFolderInfo2Ex3All(aParentFolderExLen) >= 0;
				}
				if (aParentAdded)
				{
					mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, oParentFolderShLen + "\nは既に追加されています。");
					return;
				}

				// ToDo: AddTargetFolderByWorker() 内の Invoke() を BeginInvoke() にすると少し早くなるかもしれない
				// ただし、削除の際は index の検証が必要と思う
				// 親の追加
				TargetFolderInfo aTargetFolderInfo = new TargetFolderInfo(aParentFolderExLen, aParentFolderExLen);
				aTargetFolderInfo.FolderTask = FolderTask.FindSubFolders;
				aTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.Running;
				aTargetFolderInfo.IsParent = true;
				//aTargetFolderInfo.IsOpen = false;
				aTargetFolderInfo.NumTotalFolders = 1;
				aTargetFolderInfo.Visible = true;
				Application.Current.Dispatcher.Invoke(new Action(() =>
				{
					lock (mTargetFolderInfos)
					{
						mTargetFolderInfos.Add(aTargetFolderInfo);
						mTargetFolderInfos.Sort(TargetFolderInfo.Compare);
						UpdateTargetFolderInfosVisible();
					}
				}));

				// 子の検索と重複チェック
				List<String> aFolders = FindSubFolders(aParentFolderExLen);
				Boolean aChildAdded = false;
				lock (mTargetFolderInfos)
				{
					// aFolders[0] は親なので除外
					for (Int32 i = 1; i < aFolders.Count; i++)
					{
						if (FindTargetFolderInfo2Ex3All(aFolders[i]) >= 0)
						{
							aChildAdded = true;
							break;
						}
					}
				}
				if (aChildAdded)
				{
					// 追加済みの親を削除
					Application.Current.Dispatcher.Invoke(new Action(() =>
					{
						lock (mTargetFolderInfos)
						{
							Int32 aParentIndex = FindTargetFolderInfo2Ex3All(aParentFolderExLen);
							mTargetFolderInfos.RemoveAt(aParentIndex);
							UpdateTargetFolderInfosVisible();
						}
					}));

					mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, oParentFolderShLen
							+ "\nのサブフォルダーが既に追加されています。\nサブフォルダーを一旦削除してから追加しなおして下さい。");
					return;
				}

				Application.Current.Dispatcher.Invoke(new Action(() =>
				{
					lock (mTargetFolderInfos)
					{
						// 子の追加
						for (Int32 i = 1; i < aFolders.Count; i++)
						{
							aTargetFolderInfo = new TargetFolderInfo(aParentFolderExLen, aFolders[i]);
							mTargetFolderInfos.Add(aTargetFolderInfo);
						}

						// 親設定
						Int32 aParentIndex = FindTargetFolderInfo2Ex3All(aParentFolderExLen);
						mTargetFolderInfos[aParentIndex].NumTotalFolders = aFolders.Count;
						mTargetFolderInfos[aParentIndex].IsOpen = false;
						mTargetFolderInfos[aParentIndex].FolderTask = FolderTask.AddFileName;
						mTargetFolderInfos[aParentIndex].FolderTaskStatus = FolderTaskStatus.Queued;

						// その他
						mTargetFolderInfos.Sort(TargetFolderInfo.Compare);
#if DEBUGz
						if (mTargetFolderInfos.Count > 0)
						{
							mTargetFolderInfos[0].IsOpen = true;
						}
#endif
						UpdateTargetFolderInfosVisible();
					}
				}));
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aFolders.Count.ToString("#,0")
						+ " 個のフォルダーを検索対象に追加予定としました。");
				mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();

				// 自動対象情報更新
				AdjustAutoTargetInfoIfNeeded2Sh(oParentFolderShLen);

				// フォルダータスク実行（async を待機しない）
				Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(DoFolderTaskByWorker, mFolderTaskLock, null, mEnvironment.LogWriter);

#if DEBUG
				Thread.Sleep(1000);
#endif

				// ボタン制御
				mMainWindowViewModel.ButtonTFoundsClickedCommand.RaiseCanExecuteChanged();
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー追加タスク実行時エラー：\n" + oExcep.Message);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 後片付け
				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.AddTargetFolder] = false;
				SetYukaListerStatus();
			}
		}

		// --------------------------------------------------------------------
		// 自動追加フォルダーを最適化
		// --------------------------------------------------------------------
		private void AdjustAutoTargetInfoIfNeeded2Sh(String oFolderShLen)
		{
			if (!IsAutoTargetDrive2Sh(oFolderShLen))
			{
				return;
			}

			String aDriveRoot = oFolderShLen.Substring(0, 3);
			String aDriveRootExLen = mEnvironment.ExtendPath(aDriveRoot);
			AutoTargetInfo aAutoTargetInfo = new AutoTargetInfo();
			lock (mTargetFolderInfos)
			{
				for (Int32 i = 0; i < mTargetFolderInfos.Count; i++)
				{
					if (mTargetFolderInfos[i].IsParent && mTargetFolderInfos[i].FolderTask != FolderTask.Remove
							&& mTargetFolderInfos[i].Path.StartsWith(aDriveRootExLen, StringComparison.OrdinalIgnoreCase))
					{
						aAutoTargetInfo.Folders.Add(mEnvironment.ShortenPath(mTargetFolderInfos[i].Path).Substring(2));
					}
				}
			}
			SaveAutoTargetInfo2Sh(oFolderShLen, aAutoTargetInfo);
		}

		// --------------------------------------------------------------------
		// トラック情報からオンボーカル・オフボーカルがあるか解析する
		// --------------------------------------------------------------------
		private void AnalyzeSmartTrack(String oTrack, out Boolean oHasOn, out Boolean oHasOff)
		{
			oHasOn = false;
			oHasOff = false;

			if (String.IsNullOrEmpty(oTrack))
			{
				return;
			}

			String[] aTracks = oTrack.Split(new Char[] { '-', '_', '+', ',', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			for (Int32 i = 0; i < aTracks.Length; i++)
			{
				Int32 aBothPos = BOTH_VOCAL_WORDS.IndexOf("|" + aTracks[i] + "|", StringComparison.OrdinalIgnoreCase);
				if (aBothPos >= 0)
				{
					oHasOff = true;
					oHasOn = true;
					return;
				}

				Int32 aOffPos = OFF_VOCAL_WORDS.IndexOf("|" + aTracks[i] + "|", StringComparison.OrdinalIgnoreCase);
				if (aOffPos >= 0)
				{
					oHasOff = true;
				}
				else
				{
					oHasOn = true;
				}
			}
		}

		// --------------------------------------------------------------------
		// 自動追加情報記録ファイル名
		// ＜引数＞ oFolder: 通常表記
		// ＜返値＞ 通常表記のパス
		// --------------------------------------------------------------------
		private String AutoTargetInfoPath2Sh(String oFolderShLen)
		{
			Debug.Assert(!oFolderShLen.StartsWith(YlConstants.EXTENDED_LENGTH_PATH_PREFIX), "AutoTargetInfoPath2Sh() not ShLen");

			return oFolderShLen.Substring(0, 3) + FILE_NAME_AUTO_TARGET_INFO;
		}

		// --------------------------------------------------------------------
		// 複数の人物をフリガナ順に並べてカンマで結合
		// --------------------------------------------------------------------
		private void ConcatPersonNameAndRuby(List<TPerson> oPeople, out String oName, out String oRuby)
		{
			if (oPeople.Count == 0)
			{
				oName = null;
				oRuby = null;
				return;
			}

			oPeople.Sort(ConcatPersonNameAndRubyCompare);

			StringBuilder aSbName = new StringBuilder();
			StringBuilder aSbRuby = new StringBuilder();
			aSbName.Append(oPeople[0].Name);
			aSbRuby.Append(oPeople[0].Ruby);

			for (Int32 i = 1; i < oPeople.Count; i++)
			{
				aSbName.Append("," + oPeople[i].Name);
				aSbRuby.Append("," + oPeople[i].Ruby);
			}

			oName = aSbName.ToString();
			oRuby = aSbRuby.ToString();
		}

		// --------------------------------------------------------------------
		// 比較関数
		// --------------------------------------------------------------------
		private Int32 ConcatPersonNameAndRubyCompare(TPerson oLhs, TPerson oRhs)
		{
			if (oLhs.Ruby == oRhs.Ruby)
			{
				return String.Compare(oLhs.Name, oRhs.Name);
			}

			return String.Compare(oLhs.Ruby, oRhs.Ruby);
		}

		// --------------------------------------------------------------------
		// ゆかり用リストデータベースを作業用インメモリからディスクにコピー
		// --------------------------------------------------------------------
		private void CopyYukariListDb()
		{
			// コピー
			using (YukariListDatabaseInDisk aYukariListDbInDisk = new YukariListDatabaseInDisk(mEnvironment))
			{
				aYukariListDbInDisk.CopyFromInMemory(YukariListDbInMemory);
			}

			// FolderTaskStatus が DoneInMemory のものを更新
			lock (mTargetFolderInfos)
			{
				foreach (TargetFolderInfo aTargetFolderInfo in mTargetFolderInfos)
				{
					if (aTargetFolderInfo.FolderTaskStatus == FolderTaskStatus.DoneInMemory)
					{
						if (aTargetFolderInfo.FolderTask == FolderTask.AddFileName)
						{
							aTargetFolderInfo.FolderTask = FolderTask.AddInfo;
							aTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.Queued;
						}
						else
						{
							aTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.DoneInDisk;
						}
					}
				}
			}

			mDirtyDg = true;
		}

		// --------------------------------------------------------------------
		// ゆかり用リストデータベース（ディスク、メモリ両方）を作成
		// --------------------------------------------------------------------
		private void CreateYukariDb()
		{
			if (!mEnvironment.YukaListerSettings.IsYukariConfigPathSet())
			{
				return;
			}

			// ゆかり用データベースを作業用インメモリデータベースに作成
			YukariListDbInMemory = new YukariListDatabaseInMemory(mEnvironment);

			// リスト DB をディスクにコピー
			Directory.CreateDirectory(Path.GetDirectoryName(mEnvironment.YukaListerSettings.YukariListDbInDiskPath()));
			if (mEnvironment.YukaListerSettings.ClearPrevList || !File.Exists(mEnvironment.YukaListerSettings.YukariListDbInDiskPath()))
			{
				CopyYukariListDb();
			}
			else
			{
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "前回のゆかり用リストデータベースをクリアしませんでした。");
			}

		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// ＜引数＞ oDriveLetter: "A:" のようにコロンまで
		// --------------------------------------------------------------------
		private void DeviceArrival(String oDriveLetter)
		{
			AutoTargetInfo aAutoTargetInfo = LoadAutoTargetInfo2Sh(oDriveLetter + "\\");

			foreach (String aFolder in aAutoTargetInfo.Folders)
			{
				// async を待機しない
				Task aSuppressWarning = YlCommon.LaunchTaskAsync(AddTargetFolderByWorker, mGeneralTaskLock, oDriveLetter + aFolder, mEnvironment.LogWriter);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void DeviceRemoveComplete(String oDriveLetter)
		{
			String aDriveLetterExLen = mEnvironment.ExtendPath(oDriveLetter);
			List<String> aRemoveFolders = new List<String>();
			lock (mTargetFolderInfos)
			{
				foreach (TargetFolderInfo aTargetFolderInfo in mTargetFolderInfos)
				{
					if (!aTargetFolderInfo.IsParent)
					{
						continue;
					}

					if (aTargetFolderInfo.Path.StartsWith(aDriveLetterExLen, StringComparison.OrdinalIgnoreCase))
					{
						aRemoveFolders.Add(aTargetFolderInfo.Path);
					}
				}
			}

			foreach (String aFolder in aRemoveFolders)
			{
				// async を待機しない
				Task aSuppressWarning = YlCommon.LaunchTaskAsync(RemoveTargetFolderByWorker, mGeneralTaskLock, aFolder, mEnvironment.LogWriter);
			}
		}

		// --------------------------------------------------------------------
		// フォルダータスク実行
		// フォルダータスクワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorker(Object oDummy)
		{
			try
			{
				TargetFolderInfo aPrevParentTargetFolderInfo = null;
				FolderTask aPrevFolderTask = FolderTask.__End__;

				for (; ; )
				{
					if (mMainWindowViewModel.YukaListerDbStatus == YukaListerStatus.Error)
					{
						mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "エラー発生中のためフォルダータスクを実行できません。");
						break;
					}

					// 実行すべきタスクを確認
					TargetFolderInfo aTargetFolderInfo;
					FindFolderTaskTarget(out aTargetFolderInfo);
					DoFolderTaskByWorkerPrevParentChangedIfNeeded(aPrevParentTargetFolderInfo, aTargetFolderInfo);

					// フォルダータスクが変更されたらログする
					if (aTargetFolderInfo != null && aTargetFolderInfo.FolderTask != aPrevFolderTask)
					{
						mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "フォルダータスク遷移：" + aPrevFolderTask.ToString() + " → " + aTargetFolderInfo.FolderTask.ToString());
					}

					// ファイル名追加が終わったらゆかり用データベースを一旦出力
					if (aPrevFolderTask == FolderTask.AddFileName && (aTargetFolderInfo == null || aTargetFolderInfo.FolderTask != FolderTask.AddFileName))
					{
						CopyYukariListDb();
						aPrevFolderTask = FolderTask.__End__;
#if DEBUGz
						Thread.Sleep(3 * 1000);
#endif
						continue;
					}

					if (aTargetFolderInfo == null)
					{
						mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "実行予定のフォルダータスクをすべて実行しました。");

						// リストタスク実行（async を待機しない）
						Task aSuppressWarning = OutputYukariListAsync();
						break;
					}

					aPrevFolderTask = aTargetFolderInfo.FolderTask;

					// 情報更新
					aTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.Running;
					SetDirtyDgIfNeeded(aTargetFolderInfo);
					if (!mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.DoFolderTask])
					{
						mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.DoFolderTask] = true;
						SetYukaListerStatus();
					}

					// 親の情報更新
					TargetFolderInfo aParentTargetFolderInfo = null;
					lock (mTargetFolderInfos)
					{
						Int32 aParentTargetFolderInfoIndex = FindTargetFolderInfo2Ex3All(aTargetFolderInfo.ParentPath);
						aParentTargetFolderInfo = mTargetFolderInfos[aParentTargetFolderInfoIndex];
					}
					if (!aParentTargetFolderInfo.IsChildRunning)
					{
						aParentTargetFolderInfo.IsChildRunning = true;
						SetDirtyDgIfNeeded(aParentTargetFolderInfo);
					}

					// FolderTask ごとの処理
					switch (aTargetFolderInfo.FolderTask)
					{
						case FolderTask.AddFileName:
							DoFolderTaskByWorkerAddFileName(aTargetFolderInfo, aParentTargetFolderInfo);
							break;
						case FolderTask.AddInfo:
							DoFolderTaskByWorkerAddInfo(aTargetFolderInfo, aParentTargetFolderInfo);
							break;
						case FolderTask.Remove:
							DoFolderTaskByWorkerRemove(aTargetFolderInfo, aParentTargetFolderInfo);
							break;
						case FolderTask.Update:
							break;
						default:
							Debug.Assert(false, "DoFolderTaskByWorker() bad aTargetFolderInfo.FolderTask");
							break;
					}

					SetDirtyDgIfNeeded(aTargetFolderInfo);

					// 次の準備
					aPrevParentTargetFolderInfo = aParentTargetFolderInfo;
				}
			}
			catch (OperationCanceledException)
			{
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "フォルダータスクを中止しました。");
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "フォルダータスク実行時エラー：\n" + oExcep.Message);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				if (!mEnvironment.AppCancellationTokenSource.IsCancellationRequested)
				{
					mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.DoFolderTask] = false;
					SetYukaListerStatus();
					UpdateDirtyDgWithInvoke(true);
				}
			}
		}

		// --------------------------------------------------------------------
		// ファイル名追加タスク実行
		// フォルダータスクワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerAddFileName(TargetFolderInfo oTargetFolderInfo, TargetFolderInfo oParentTargetFolderInfo)
		{
			// 検索
			FindNicoKaraFiles(oTargetFolderInfo.Path);

			// 状況更新
			// mTargetFolderInfos にはアクセスしないが RemoveTargetFolderByWorker() の状況更新と競合しないようにロックした上で実行する
			lock (mTargetFolderInfos)
			{
				// 追加している間にユーザーから削除指定された場合は削除を優先するので状態を更新しない
				// 追加している間に環境設定が変わって待機中になった場合は新たな設定で追加が必要なので状態を更新しない
				if (oTargetFolderInfo.FolderTask == FolderTask.AddFileName && oTargetFolderInfo.FolderTaskStatus == FolderTaskStatus.Running)
				{
					oTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.DoneInMemory;
				}
			}
		}

		// --------------------------------------------------------------------
		// フォルダー追加タスク実行
		// フォルダータスクワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerAddInfo(TargetFolderInfo oTargetFolderInfo, TargetFolderInfo oParentTargetFolderInfo)
		{
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "属性確認対象：" + oTargetFolderInfo.Path);

			// 情報追加
			AddNicoKaraInfo(oTargetFolderInfo.Path);

			// 状況更新
			// mTargetFolderInfos にはアクセスしないが RemoveTargetFolderByWorker() の状況更新と競合しないようにロックした上で実行する
			lock (mTargetFolderInfos)
			{
				// 追加している間にユーザーから削除指定された場合は削除を優先するので状態を更新しない
				// 追加している間に環境設定が変わって待機中になった場合は新たな設定で追加が必要なので状態を更新しない
				if (oTargetFolderInfo.FolderTask == FolderTask.AddInfo && oTargetFolderInfo.FolderTaskStatus == FolderTaskStatus.Running)
				{
					oTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.DoneInMemory;
				}
			}
		}

		// --------------------------------------------------------------------
		// 直前の親と違う親になった：直前の親のタスクが「追加」の場合
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerPrevParentChangedAdd(TargetFolderInfo oPrevParentTargetFolderInfo)
		{
			oPrevParentTargetFolderInfo.IsChildRunning = false;
			SetDirtyDgIfNeeded(oPrevParentTargetFolderInfo);
		}

		// --------------------------------------------------------------------
		// 直前の親と違う親になった場合に処理を実行
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerPrevParentChangedIfNeeded(TargetFolderInfo oPrevParentTargetFolderInfo, TargetFolderInfo oParentTargetFolderInfo)
		{
			if (oPrevParentTargetFolderInfo != null && oPrevParentTargetFolderInfo != oParentTargetFolderInfo)
			{
				lock (mTargetFolderInfos)
				{
					// 直前の親の子供のタスクがすべて完了しているか確認する
					// 途中でユーザーがフォルダーを追加して処理中の親が移った場合など、すべて完了していない場合でも親が変わる場合があるので、確認が必要
					Debug.Assert(oPrevParentTargetFolderInfo.IsParent, "DoFolderTaskByWorkerPrevParentChangedIfNeeded() child");
					Int32 aPrevParentTargetFolderInfoIndex = FindTargetFolderInfo2Ex3All(oPrevParentTargetFolderInfo.Path);
					Boolean aAllDone = true;
					for (Int32 i = aPrevParentTargetFolderInfoIndex; i < aPrevParentTargetFolderInfoIndex + oPrevParentTargetFolderInfo.NumTotalFolders; i++)
					{
						if (mTargetFolderInfos[i].FolderTaskStatus != FolderTaskStatus.DoneInMemory && mTargetFolderInfos[i].FolderTaskStatus != FolderTaskStatus.DoneInDisk)
						{
							aAllDone = false;
							break;
						}
					}
					if (!aAllDone)
					{
						return;
					}

					// 子供のタスクが完了しているので処理を実行
					switch (oPrevParentTargetFolderInfo.FolderTask)
					{
						case FolderTask.AddFileName:
						case FolderTask.AddInfo:
							DoFolderTaskByWorkerPrevParentChangedAdd(oPrevParentTargetFolderInfo);
							break;
						case FolderTask.Remove:
							DoFolderTaskByWorkerPrevParentChangedRemove(aPrevParentTargetFolderInfoIndex, oPrevParentTargetFolderInfo);
							break;
						case FolderTask.Update:
							break;
						default:
							Debug.Assert(false, "DoFolderTaskByWorkerPrevParentChangedIfNeeded() bad oPrevParentTargetFolderInfo.FolderTask");
							break;
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 直前の親と違う親になった：直前の親のタスクは削除
		// 呼び出し元において lock(mTargetFolderInfos) 必須
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerPrevParentChangedRemove(Int32 oPrevParentTargetFolderInfoIndex, TargetFolderInfo oPrevParentTargetFolderInfo)
		{
			Debug.Assert(Monitor.IsEntered(mTargetFolderInfos), "DoFolderTaskByWorkerPrevParentChangedRemove() not locked");

			// 子もろとも削除
			mTargetFolderInfos.RemoveRange(oPrevParentTargetFolderInfoIndex, oPrevParentTargetFolderInfo.NumTotalFolders);
			SetDirtyDgIfNeeded(oPrevParentTargetFolderInfo);
		}

		// --------------------------------------------------------------------
		// フォルダー削除タスク実行
		// フォルダータスクワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerRemove(TargetFolderInfo oTargetFolderInfo, TargetFolderInfo oParentTargetFolderInfo)
		{
			// 削除
			RemoveNicoKaraFiles(oTargetFolderInfo.Path);

			// 状況更新
			// mTargetFolderInfos にはアクセスしないが RemoveTargetFolderByWorker() の状況更新と競合しないようにロックした上で実行する
			lock (mTargetFolderInfos)
			{
				oTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.DoneInMemory;
			}
		}

		// --------------------------------------------------------------------
		// 次に実行すべきフォルダータスクを検索
		// --------------------------------------------------------------------
		private void FindFolderTaskTarget(out TargetFolderInfo oTargetFolderInfo)
		{
			oTargetFolderInfo = null;

			lock (mTargetFolderInfos)
			{
				// ファイル名追加タスクがあれば優先的に実行する
				for (Int32 i = 0; i < mTargetFolderInfos.Count; i++)
				{
					if (mTargetFolderInfos[i].FolderTaskStatus == FolderTaskStatus.Queued && mTargetFolderInfos[i].FolderTask == FolderTask.AddFileName)
					{
						oTargetFolderInfo = mTargetFolderInfos[i];
						return;
					}
				}

				// その他の待ちタスクを検索
				for (Int32 i = 0; i < mTargetFolderInfos.Count; i++)
				{
					if (mTargetFolderInfos[i].FolderTaskStatus == FolderTaskStatus.Queued)
					{
						oTargetFolderInfo = mTargetFolderInfos[i];
						return;
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 指定フォルダ内のファイルを検索してゆかり用データベースに追加
		// ユニーク ID、フルパス、フォルダーのみ記入する
		// ファイルは再帰検索しない
		// --------------------------------------------------------------------
		private void FindNicoKaraFiles(String oFolderPathExLen)
		{
			// フォルダー除外設定を読み込む
			if (YlCommon.DetectFolderExcludeSettingsStatus(oFolderPathExLen) == FolderExcludeSettingsStatus.True)
			{
				return;
			}

			// フォルダー設定を読み込む
			FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings2Ex(oFolderPathExLen);
			FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
			String aFolderPathLower = mEnvironment.ShortenPath(oFolderPathExLen).ToLower();

			using (DataContext aYukariDbContext = new DataContext(YukariListDbInMemory.Connection))
			{
				Table<TFound> aTableFound = aYukariDbContext.GetTable<TFound>();
				IQueryable<Int64> aQueryResult =
						from x in aTableFound
						select x.Uid;
				Int64 aUid = (aQueryResult.Count() == 0 ? 0 : aQueryResult.Max()) + 1;

				// 検索
				String[] aAllPathes;
				try
				{
					aAllPathes = Directory.GetFiles(oFolderPathExLen);
				}
				catch (Exception)
				{
					return;
				}

				// 挿入
				foreach (String aPath in aAllPathes)
				{
					if (!mEnvironment.YukaListerSettings.TargetExts.Contains(Path.GetExtension(aPath).ToLower()))
					{
						continue;
					}

					TFound aRecord = new TFound();
					aRecord.Uid = aUid;
					aRecord.Path = mEnvironment.ShortenPath(aPath);
					aRecord.Folder = aFolderPathLower;

					// 楽曲名とファイルサイズが両方とも初期値だと、ゆかりが検索結果をまとめてしまうため、ダミーのファイルサイズを入れる
					// （文字列である楽曲名を入れると処理が遅くなるので処理が遅くなりにくい数字のファイルサイズをユニークにする）
					aRecord.FileSize = -aUid;

					aTableFound.InsertOnSubmit(aRecord);
					aUid++;
				}

				// コミット
				aYukariDbContext.SubmitChanges();

				mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
			}
		}

		// --------------------------------------------------------------------
		// リスト化対象フォルダーのサブフォルダーを列挙
		// SearchOption.AllDirectories 付きで Directory.GetDirectories を呼びだすと、
		// ごみ箱のようにアクセス権限の無いフォルダーの中も列挙しようとして例外が
		// 発生し中断してしまう。
		// 面倒だが 1 フォルダーずつ列挙する
		// --------------------------------------------------------------------
		private List<String> FindSubFolders(String oParentFolderExLen)
		{
			List<String> aFolders = new List<String>();

			FindSubFoldersSub(aFolders, oParentFolderExLen);

			return aFolders;
		}

		// --------------------------------------------------------------------
		// FindSubFolders() の子関数
		// --------------------------------------------------------------------
		private void FindSubFoldersSub(List<String> oFolders, String oFolderExLen)
		{
			mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 指定フォルダー
			oFolders.Add(oFolderExLen);
			mMainWindowViewModel.YukaListerStatusSubMessage = mEnvironment.ShortenPath(oFolderExLen);

			// 指定フォルダーのサブフォルダー
			try
			{
				String[] aSubFolders = Directory.GetDirectories(oFolderExLen, "*", SearchOption.TopDirectoryOnly);
				foreach (String aSubFolder in aSubFolders)
				{
					FindSubFoldersSub(oFolders, aSubFolder);
				}
			}
			catch (Exception)
			{
			}
		}

		// --------------------------------------------------------------------
		// mTargetFolderInfos の中から oPath を持つ TargetFolderInfo を探してインデックスを返す
		// 呼び出し元において lock(mTargetFolderInfos) 必須
		// --------------------------------------------------------------------
		private Int32 FindTargetFolderInfo2Ex3All(String oPathExLen)
		{
			Debug.Assert(Monitor.IsEntered(mTargetFolderInfos), "FindTargetFolderInfo2Ex3All() not locked");
			for (Int32 i = 0; i < mTargetFolderInfos.Count; i++)
			{
				if (YlCommon.IsSamePath(oPathExLen, mTargetFolderInfos[i].Path))
				{
					return i;
				}
			}

			return -1;
		}

		// --------------------------------------------------------------------
		// 選択されているフォルダーのフォルダー設定を行う
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void FolderSettings()
		{
			TargetFolderInfo aTargetFolderInfo = mMainWindowViewModel.SelectedTargetFolderInfo;
			if (aTargetFolderInfo == null)
			{
				return;
			}

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			{
				DateTime aMusicInfoDbTimeBak = aMusicInfoDbInDisk.LastWriteTime();

				// ViewModel 経由でフォルダー設定ウィンドウを開く
				using (FolderSettingsWindowViewModel aFolderSettingsWindowViewModel = new FolderSettingsWindowViewModel())
				{
					aFolderSettingsWindowViewModel.PathExLen = aTargetFolderInfo.Path;
					aFolderSettingsWindowViewModel.Environment = mEnvironment;
					mMainWindowViewModel.Messenger.Raise(new TransitionMessage(aFolderSettingsWindowViewModel, "OpenFolderSettingsWindow"));
				}

				// フォルダー設定の有無の表示を更新
				// キャンセルでも実行（設定削除→キャンセルの場合はフォルダー設定の有無が変わる）
				lock (mTargetFolderInfos)
				{
					Int32 aIndex = mTargetFolderInfos.IndexOf(aTargetFolderInfo);
					if (aIndex < 0)
					{
						throw new Exception("フォルダー設定有無を更新する対象が見つかりません。");
					}
					while (aIndex < mTargetFolderInfos.Count)
					{
						if (!mTargetFolderInfos[aIndex].Path.StartsWith(aTargetFolderInfo.Path))
						{
							break;
						}
						mTargetFolderInfos[aIndex].FolderExcludeSettingsStatus = FolderExcludeSettingsStatus.Unchecked;
						mTargetFolderInfos[aIndex].FolderSettingsStatus = FolderSettingsStatus.Unchecked;
						mDirtyDg = true;
						aIndex++;
					}
				}
				UpdateDirtyDgWithInvoke();

				// 楽曲情報データベースが更新された場合は同期を行う
				DateTime aMusicInfoDbTime = aMusicInfoDbInDisk.LastWriteTime();
				if (aMusicInfoDbTime != aMusicInfoDbTimeBak)
				{
					RunSyncClientIfNeeded();
				}
			}
		}

		// --------------------------------------------------------------------
		// 自動追加対象のドライブかどうか
		// --------------------------------------------------------------------
		private Boolean IsAutoTargetDrive2Sh(String oFolderShLen)
		{
			Debug.Assert(!oFolderShLen.StartsWith(YlConstants.EXTENDED_LENGTH_PATH_PREFIX), "IsAutoTargetDrive2Sh() not ShLen");

			String aDriveLetter = oFolderShLen.Substring(0, 1);
			DriveInfo aDriveInfo = new DriveInfo(aDriveLetter);
			if (!aDriveInfo.IsReady)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "IsAutoTargetDrive() 準備ができていない：" + aDriveLetter);
				return false;
			}

			// リムーバブルドライブのみを対象としたいが、ポータブル HDD/SSD も Fixed 扱いになるため、Fixed も対象とする
			switch (aDriveInfo.DriveType)
			{
				case DriveType.Fixed:
				case DriveType.Removable:
					mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "IsAutoTargetDrive() 対象：" + aDriveLetter);
					return true;
				default:
					mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "IsAutoTargetDrive() 非対象：" + aDriveLetter + ", " + aDriveInfo.DriveType.ToString());
					return false;
			}
		}

		// --------------------------------------------------------------------
		// 自動追加情報読み込み
		// 見つからない場合は null ではなく空のインスタンスを返す
		// ＜引数＞ oFolder: 通常表記
		// --------------------------------------------------------------------
		private AutoTargetInfo LoadAutoTargetInfo2Sh(String oFolderShLen)
		{
			Debug.Assert(!oFolderShLen.StartsWith(YlConstants.EXTENDED_LENGTH_PATH_PREFIX), "LoadAutoTargetInfo2Sh() not ShLen");

			AutoTargetInfo aAutoTargetInfo = new AutoTargetInfo();

			try
			{
				aAutoTargetInfo = Common.Deserialize<AutoTargetInfo>(AutoTargetInfoPath2Sh(oFolderShLen));
			}
			catch (Exception)
			{
			}

			return aAutoTargetInfo;
		}

		// --------------------------------------------------------------------
		// 全てのフォルダータスクを待機状態にする
		// --------------------------------------------------------------------
		private void MakeAllFolderTasksQueued()
		{
			lock (mTargetFolderInfos)
			{
				for (Int32 i = 0; i < mTargetFolderInfos.Count; i++)
				{
					mTargetFolderInfos[i].FolderTaskStatus = FolderTaskStatus.Queued;
				}
			}
			mDirtyDg = true;
			UpdateDirtyDgWithInvoke();
		}

		// --------------------------------------------------------------------
		// リストタスク実行
		// --------------------------------------------------------------------
		private Task OutputYukariListAsync()
		{
			return YlCommon.LaunchTaskAsync<Object>(OutputYukariListByWorker, mListTaskLock, null, mEnvironment.LogWriter);
		}

		// --------------------------------------------------------------------
		// リストタスク実行
		// リストタスクワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void OutputYukariListByWorker(Object oDummy)
		{
			try
			{
				if (!mEnvironment.YukaListerSettings.OutputYukari)
				{
					return;
				}

				if (mMainWindowViewModel.YukaListerDbStatus == YukaListerStatus.Error)
				{
					mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "エラー発生中のためリストタスクを実行できません。");
					return;
				}

				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.ListTask] = true;
				SetYukaListerStatus();

				// ゆかり用データベース出力
				CopyYukariListDb();

				// ゆかり用データベースを出力するとフォルダータスクの状態が更新されるため、再描画
				UpdateDirtyDgWithInvoke();

				// リスト出力
				YukariOutputWriter aYukariOutputWriter = new YukariOutputWriter(mEnvironment);
				aYukariOutputWriter.FolderPath = Path.GetDirectoryName(mEnvironment.YukaListerSettings.YukariListDbInDiskPath()) + "\\";
				YlCommon.OutputList(aYukariOutputWriter, mEnvironment, YukariListDbInMemory);

				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Information, "リスト出力が完了しました。", true);
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "リストタスク実行時エラー：\n" + oExcep.Message);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.ListTask] = false;
				SetYukaListerStatus();
			}
		}

		// --------------------------------------------------------------------
		// 別名から元のタイアップ名を取得
		// oMusicInfoDbCmd を書き換えることに注意
		// --------------------------------------------------------------------
		private String ProgramOrigin(String oAlias, SQLiteCommand oMusicInfoDbCmd)
		{
			if (String.IsNullOrEmpty(oAlias))
			{
				return null;
			}

			oMusicInfoDbCmd.CommandText = "SELECT * FROM " + TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS + " LEFT OUTER JOIN " + TTieUp.TABLE_NAME_TIE_UP
					+ " ON " + TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS + "." + TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ORIGINAL_ID + " = " + TTieUp.TABLE_NAME_TIE_UP + "." + TTieUp.FIELD_NAME_TIE_UP_ID
					+ " WHERE " + TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS + "." + TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ALIAS + " = @alias"
					+ " AND " + TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS + "." + TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_INVALID + " = 0";
			oMusicInfoDbCmd.Parameters.Add(new SQLiteParameter("@alias", oAlias));

			using (SQLiteDataReader aReader = oMusicInfoDbCmd.ExecuteReader())
			{
				while (aReader.Read())
				{
					return aReader[TTieUp.FIELD_NAME_TIE_UP_NAME].ToString();
				}
			}

			return oAlias;
		}

		// --------------------------------------------------------------------
		// 指定フォルダ内のファイルをゆかり用データベースから削除
		// --------------------------------------------------------------------
		private void RemoveNicoKaraFiles(String oFolderPathExLen)
		{
			using (DataContext aYukariDbContext = new DataContext(YukariListDbInMemory.Connection))
			{
				Table<TFound> aTableFound = aYukariDbContext.GetTable<TFound>();
				IQueryable<TFound> aQueryResult =
						from x in aTableFound
						where x.Folder == mEnvironment.ShortenPath(oFolderPathExLen).ToLower()
						select x;
				aTableFound.DeleteAllOnSubmit(aQueryResult);
				aYukariDbContext.SubmitChanges();
			}
		}

		// --------------------------------------------------------------------
		// フォルダー（サブフォルダー含む）を対象フォルダーから削除
		// 汎用ワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void RemoveTargetFolderByWorker(String oParentFolderExLen)
		{
			try
			{
				// 準備
				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.RemoveTargetFolder] = true;
				SetYukaListerStatus();
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, mEnvironment.ShortenPath(oParentFolderExLen) + " とそのサブフォルダーを検索対象から削除予定としています...");

				Int32 aNumRemoveFolders;
				lock (mTargetFolderInfos)
				{
					Int32 aParentIndex = FindTargetFolderInfo2Ex3All(oParentFolderExLen);
					if (aParentIndex < 0 || !mTargetFolderInfos[aParentIndex].IsParent)
					{
						mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "削除対象の親フォルダーが見つかりませんでした。", true);
						return;
					}

					// 削除タスク設定
					aNumRemoveFolders = mTargetFolderInfos[aParentIndex].NumTotalFolders;
					for (Int32 i = aParentIndex; i < aParentIndex + aNumRemoveFolders; i++)
					{
						Debug.Assert(i == aParentIndex || !mTargetFolderInfos[i].IsParent, "RemoveTargetFolderByWorker() 別の親を削除しようとした：子の数："
								+ aNumRemoveFolders.ToString());
						mTargetFolderInfos[i].FolderTask = FolderTask.Remove;
						mTargetFolderInfos[i].FolderTaskStatus = FolderTaskStatus.Queued;
					}

					SetDirtyDgIfNeeded(mTargetFolderInfos[aParentIndex]);
				}
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aNumRemoveFolders.ToString("#,0")
							+ " 個のフォルダーを検索対象から削除予定としました。");
				mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();

#if DEBUGz
				Thread.Sleep(2000);
#endif

				// 自動対象情報更新
				AdjustAutoTargetInfoIfNeeded2Sh(mEnvironment.ShortenPath(oParentFolderExLen));

				// フォルダータスク実行（async を待機しない）
				Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(DoFolderTaskByWorker, mFolderTaskLock, null, mEnvironment.LogWriter);

				// 情報更新
				UpdateDirtyDgWithInvoke(true);
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー削除タスク実行時エラー：\n" + oExcep.Message);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 後片付け
				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.RemoveTargetFolder] = false;
				SetYukaListerStatus();
			}
		}

		// --------------------------------------------------------------------
		// 自動追加情報保存
		// --------------------------------------------------------------------
		private void SaveAutoTargetInfo2Sh(String oFolderShLen, AutoTargetInfo oAutoTargetInfo)
		{
			try
			{
				String aPath = AutoTargetInfoPath2Sh(oFolderShLen);

				// 隠しファイルを直接上書きできないので一旦削除する
				if (File.Exists(aPath))
				{
					File.Delete(aPath);
				}

				// 保存
				Common.Serialize(aPath, oAutoTargetInfo);
				FileAttributes aAttr = File.GetAttributes(aPath);
				File.SetAttributes(aPath, aAttr | FileAttributes.Hidden);
			}
			catch (Exception)
			{
			}
		}

		// --------------------------------------------------------------------
		// oTargetFolderInfo が DataGrid 表示対象なら DirtyDg をセットする
		// --------------------------------------------------------------------
		private void SetDirtyDgIfNeeded(TargetFolderInfo oTargetFolderInfo)
		{
			if (oTargetFolderInfo.Visible)
			{
				mDirtyDg = true;
			}
		}

		// --------------------------------------------------------------------
		// 検出ファイルレコードの値を、フォルダー設定や楽曲情報データベースから検索して設定する
		// --------------------------------------------------------------------
		private void SetTFoundValue(TFound oRecord, FolderSettingsInMemory oFolderSettingsInMemory, SQLiteCommand oMusicInfoDbCmd, DataContext oMusicInfoDbContext,
				List<String> oCategoryNames)
		{
			// ファイル名・フォルダー固定値と合致する命名規則を探す
			Dictionary<String, String> aDicByFile = YlCommon.MatchFileNameRulesAndFolderRule(Path.GetFileNameWithoutExtension(oRecord.Path), oFolderSettingsInMemory);
			aDicByFile[YlConstants.RULE_VAR_PROGRAM] = ProgramOrigin(aDicByFile[YlConstants.RULE_VAR_PROGRAM], oMusicInfoDbCmd);
			aDicByFile[YlConstants.RULE_VAR_TITLE] = SongOrigin(aDicByFile[YlConstants.RULE_VAR_TITLE], oMusicInfoDbCmd);
			if (aDicByFile[YlConstants.RULE_VAR_CATEGORY] != null)
			{
				if (oCategoryNames.IndexOf(aDicByFile[YlConstants.RULE_VAR_CATEGORY]) < 0)
				{
					aDicByFile[YlConstants.RULE_VAR_CATEGORY] = null;
				}
			}

			// 楽曲情報データベースを適用
			SetTFoundValueByMusicInfoDb(oRecord, aDicByFile, oMusicInfoDbCmd, oMusicInfoDbContext);

			// 楽曲情報データベースに無かった項目をファイル名・フォルダー固定値から取得
			oRecord.Category = oRecord.Category == null ? aDicByFile[YlConstants.RULE_VAR_CATEGORY] : oRecord.Category;
			oRecord.TieUpName = oRecord.TieUpName == null ? aDicByFile[YlConstants.RULE_VAR_PROGRAM] : oRecord.TieUpName;
			oRecord.TieUpAgeLimit = oRecord.TieUpAgeLimit == 0 ? Common.StringToInt32(aDicByFile[YlConstants.RULE_VAR_AGE_LIMIT]) : oRecord.TieUpAgeLimit;
			oRecord.SongOpEd = oRecord.SongOpEd == null ? aDicByFile[YlConstants.RULE_VAR_OP_ED] : oRecord.SongOpEd;
			oRecord.SongName = oRecord.SongName == null ? aDicByFile[YlConstants.RULE_VAR_TITLE] : oRecord.SongName;
			if (oRecord.ArtistName == null && aDicByFile[YlConstants.RULE_VAR_ARTIST] != null)
			{
				// ファイル名から歌手名を取得できている場合は、楽曲情報データベースからフリガナを探す
				List<TPerson> aArtists;
				aArtists = YlCommon.SelectMastersByName<TPerson>(oMusicInfoDbContext, aDicByFile[YlConstants.RULE_VAR_ARTIST]);
				if (aArtists.Count > 0)
				{
					// 歌手名が楽曲情報データベースに登録されていた場合はその情報を使う
					oRecord.ArtistName = aDicByFile[YlConstants.RULE_VAR_ARTIST];
					oRecord.ArtistRuby = aArtists[0].Ruby;
				}
				else
				{
					// 歌手名そのままでは楽曲情報データベースに登録されていない場合
					if (aDicByFile[YlConstants.RULE_VAR_ARTIST].IndexOf(YlConstants.VAR_VALUE_DELIMITER) >= 0)
					{
						// 区切り文字で区切られた複数の歌手名が記載されている場合は分解して解析する
						String[] aArtistNames = aDicByFile[YlConstants.RULE_VAR_ARTIST].Split(YlConstants.VAR_VALUE_DELIMITER[0]);
						foreach (String aArtistName in aArtistNames)
						{
							List<TPerson> aArtistsTmp = YlCommon.SelectMastersByName<TPerson>(oMusicInfoDbContext, aArtistName);
							if (aArtistsTmp.Count > 0)
							{
								// 区切られた歌手名が楽曲情報データベースに存在する
								aArtists.Add(aArtistsTmp[0]);
							}
							else
							{
								// 区切られた歌手名が楽曲情報データベースに存在しないので仮の人物を作成
								TPerson aArtistTmp = new TPerson();
								aArtistTmp.Name = aArtistTmp.Ruby = aArtistName;
								aArtists.Add(aArtistTmp);
							}
						}
						String aArtistName2;
						String aArtistRuby2;
						ConcatPersonNameAndRuby(aArtists, out aArtistName2, out aArtistRuby2);
						oRecord.ArtistName = aArtistName2;
						oRecord.ArtistRuby = aArtistRuby2;
					}
					else
					{
						// 楽曲情報データベースに登録されていないので漢字のみ格納
						oRecord.ArtistName = aDicByFile[YlConstants.RULE_VAR_ARTIST];
					}
				}
			}
			oRecord.SongRuby = oRecord.SongRuby == null ? aDicByFile[YlConstants.RULE_VAR_TITLE_RUBY] : oRecord.SongRuby;
			oRecord.Worker = oRecord.Worker == null ? aDicByFile[YlConstants.RULE_VAR_WORKER] : oRecord.Worker;
			oRecord.Track = oRecord.Track == null ? aDicByFile[YlConstants.RULE_VAR_TRACK] : oRecord.Track;
			oRecord.SmartTrackOnVocal = !oRecord.SmartTrackOnVocal ? aDicByFile[YlConstants.RULE_VAR_ON_VOCAL] != null : oRecord.SmartTrackOnVocal;
			oRecord.SmartTrackOffVocal = !oRecord.SmartTrackOffVocal ? aDicByFile[YlConstants.RULE_VAR_OFF_VOCAL] != null : oRecord.SmartTrackOffVocal;
			oRecord.Comment = oRecord.Comment == null ? aDicByFile[YlConstants.RULE_VAR_COMMENT] : oRecord.Comment;

			// トラック情報からスマートトラック解析
			Boolean aHasOn;
			Boolean aHasOff;
			AnalyzeSmartTrack(oRecord.Track, out aHasOn, out aHasOff);
			oRecord.SmartTrackOnVocal |= aHasOn;
			oRecord.SmartTrackOffVocal |= aHasOff;

			// ルビが無い場合は漢字を採用
			if (String.IsNullOrEmpty(oRecord.TieUpRuby))
			{
				oRecord.TieUpRuby = oRecord.TieUpName;
			}
			if (String.IsNullOrEmpty(oRecord.SongRuby))
			{
				oRecord.SongRuby = oRecord.SongName;
			}

			// 頭文字
			if (!String.IsNullOrEmpty(oRecord.TieUpRuby))
			{
				oRecord.Head = YlCommon.Head(oRecord.TieUpRuby);
			}
			else
			{
				oRecord.Head = YlCommon.Head(oRecord.SongRuby);
			}

			// 番組名が無い場合は頭文字を採用（ボカロ曲等のリスト化用）
			if (String.IsNullOrEmpty(oRecord.TieUpName))
			{
				oRecord.TieUpName = oRecord.Head;
			}
		}

		// --------------------------------------------------------------------
		// 検出ファイルレコードの値を、楽曲情報データベースから検索して設定する
		// ファイル名を元に検索し、結果が複数ある場合は他の情報も照らし合わせて最も近い物を設定する
		// --------------------------------------------------------------------
		private void SetTFoundValueByMusicInfoDb(TFound oRecord, Dictionary<String, String> oDicByFile, SQLiteCommand oMusicInfoDbCmd, DataContext oMusicInfoDbContext)
		{
			if (oDicByFile[YlConstants.RULE_VAR_TITLE] == null)
			{
				return;
			}

			List<TSong> aSongs;
			// 楽曲名で検索
			aSongs = YlCommon.SelectMastersByName<TSong>(oMusicInfoDbContext, oDicByFile[YlConstants.RULE_VAR_TITLE]);

			// タイアップ名で絞り込み
			if (aSongs.Count > 1 && oDicByFile[YlConstants.RULE_VAR_PROGRAM] != null)
			{
				List<TSong> aSongsWithTieUp = new List<TSong>();
				foreach (TSong aSong in aSongs)
				{
					TTieUp aTieUp = YlCommon.SelectMasterById<TTieUp>(oMusicInfoDbContext, aSong.TieUpId);
					if (aTieUp != null && aTieUp.Name == oDicByFile[YlConstants.RULE_VAR_PROGRAM])
					{
						aSongsWithTieUp.Add(aSong);
					}
				}
				if (aSongsWithTieUp.Count > 0)
				{
					aSongs = aSongsWithTieUp;
				}
			}

			// カテゴリーで絞り込み
			if (aSongs.Count > 1 && oDicByFile[YlConstants.RULE_VAR_CATEGORY] != null)
			{
				List<TSong> aSongsWithCategory = new List<TSong>();
				foreach (TSong aSong in aSongs)
				{
					TCategory aCategory = YlCommon.SelectMasterById<TCategory>(oMusicInfoDbContext, aSong.CategoryId);
					if (aCategory != null && aCategory.Name == oDicByFile[YlConstants.RULE_VAR_CATEGORY])
					{
						aSongsWithCategory.Add(aSong);
					}
				}
				if (aSongsWithCategory.Count > 0)
				{
					aSongs = aSongsWithCategory;
				}
			}

			// 歌手名で絞り込み
			if (aSongs.Count > 1 && oDicByFile[YlConstants.RULE_VAR_ARTIST] != null)
			{
				List<TSong> aSongsWithArtist = new List<TSong>();
				foreach (TSong aSong in aSongs)
				{
					String aArtistName;
					String aArtistRuby;
					ConcatPersonNameAndRuby(YlCommon.SelectSequencePeopleBySongId<TArtistSequence>(oMusicInfoDbContext, aSong.Id), out aArtistName, out aArtistRuby);
					if (!String.IsNullOrEmpty(aArtistName) && aArtistName == oDicByFile[YlConstants.RULE_VAR_ARTIST])
					{
						aSongsWithArtist.Add(aSong);
					}
				}
				if (aSongsWithArtist.Count > 0)
				{
					aSongs = aSongsWithArtist;
				}
			}

			TTieUp aTieUpOfSong = null;
			TSong aSelectedSong = null;
			if (aSongs.Count == 0)
			{
				// 楽曲情報データベース内に曲情報が無い場合は、タイアップ情報があるか検索
				if (oDicByFile[YlConstants.RULE_VAR_PROGRAM] != null)
				{
					List<TTieUp> aTieUps = YlCommon.SelectMastersByName<TTieUp>(oMusicInfoDbContext, oDicByFile[YlConstants.RULE_VAR_PROGRAM]);
					if (aTieUps.Count > 0)
					{
						aTieUpOfSong = aTieUps[0];
					}
				}
				if (aTieUpOfSong == null)
				{
					// 曲情報もタイアップ情報も無い場合は諦める
					return;
				}
			}
			else
			{
				// 楽曲情報データベース内に曲情報がある場合は、曲に紐付くタイアップを得る
				aSelectedSong = aSongs[0];
				aTieUpOfSong = YlCommon.SelectMasterById<TTieUp>(oMusicInfoDbContext, aSelectedSong.TieUpId);
			}

			if (aTieUpOfSong != null)
			{
				TCategory aCategoryOfTieUp = YlCommon.SelectMasterById<TCategory>(oMusicInfoDbContext, aTieUpOfSong.CategoryId);
				if (aCategoryOfTieUp != null)
				{
					// TCategory 由来項目の設定
					oRecord.Category = aCategoryOfTieUp.Name;
				}

				TMaker aMakerOfTieUp = YlCommon.SelectMasterById<TMaker>(oMusicInfoDbContext, aTieUpOfSong.MakerId);
				if (aMakerOfTieUp != null)
				{
					// TMaker 由来項目の設定
					oRecord.MakerName = aMakerOfTieUp.Name;
					oRecord.MakerRuby = aMakerOfTieUp.Ruby;
				}

				List<TTieUpGroup> aTieUpGroups = YlCommon.SelectSequenceTieUpGroupsByTieUpId(oMusicInfoDbContext, aTieUpOfSong.Id);
				if (aTieUpGroups.Count > 0)
				{
					// TTieUpGroup 由来項目の設定
					oRecord.TieUpGroupName = aTieUpGroups[0].Name;
					oRecord.TieUpGroupRuby = aTieUpGroups[0].Ruby;
				}

				// TieUp 由来項目の設定
				oRecord.TieUpName = aTieUpOfSong.Name;
				oRecord.TieUpRuby = aTieUpOfSong.Ruby;
				oRecord.TieUpAgeLimit = aTieUpOfSong.AgeLimit;
				oRecord.SongReleaseDate = aTieUpOfSong.ReleaseDate;
			}

			if (aSelectedSong == null)
			{
				return;
			}

			// 人物系
			String aName;
			String aRuby;
			ConcatPersonNameAndRuby(YlCommon.SelectSequencePeopleBySongId<TArtistSequence>(oMusicInfoDbContext, aSelectedSong.Id), out aName, out aRuby);
			oRecord.ArtistName = aName;
			oRecord.ArtistRuby = aRuby;
			ConcatPersonNameAndRuby(YlCommon.SelectSequencePeopleBySongId<TLyristSequence>(oMusicInfoDbContext, aSelectedSong.Id), out aName, out aRuby);
			oRecord.LyristName = aName;
			oRecord.LyristRuby = aRuby;
			ConcatPersonNameAndRuby(YlCommon.SelectSequencePeopleBySongId<TComposerSequence>(oMusicInfoDbContext, aSelectedSong.Id), out aName, out aRuby);
			oRecord.ComposerName = aName;
			oRecord.ComposerRuby = aRuby;
			ConcatPersonNameAndRuby(YlCommon.SelectSequencePeopleBySongId<TArrangerSequence>(oMusicInfoDbContext, aSelectedSong.Id), out aName, out aRuby);
			oRecord.ArrangerName = aName;
			oRecord.ArrangerRuby = aRuby;

			// TSong 由来項目の設定
			oRecord.SongName = aSelectedSong.Name;
			oRecord.SongRuby = aSelectedSong.Ruby;
			oRecord.SongOpEd = aSelectedSong.OpEd;
			if (oRecord.SongReleaseDate <= YlConstants.INVALID_MJD && aSelectedSong.ReleaseDate > YlConstants.INVALID_MJD)
			{
				oRecord.SongReleaseDate = aSelectedSong.ReleaseDate;
			}
			if (String.IsNullOrEmpty(oRecord.Category))
			{
				TCategory aCategoryOfSong = YlCommon.SelectMasterById<TCategory>(oMusicInfoDbContext, aSelectedSong.CategoryId);
				if (aCategoryOfSong != null)
				{
					oRecord.Category = aCategoryOfSong.Name;
				}
			}

		}

		// --------------------------------------------------------------------
		// 別名から元の楽曲名を取得
		// oInfoDbCmd を書き換えることに注意
		// --------------------------------------------------------------------
		private String SongOrigin(String oAlias, SQLiteCommand oInfoDbCmd)
		{
			if (String.IsNullOrEmpty(oAlias))
			{
				return null;
			}

			oInfoDbCmd.CommandText = "SELECT * FROM " + TSongAlias.TABLE_NAME_SONG_ALIAS + " LEFT OUTER JOIN " + TSong.TABLE_NAME_SONG
					+ " ON " + TSongAlias.TABLE_NAME_SONG_ALIAS + "." + TSongAlias.FIELD_NAME_SONG_ALIAS_ORIGINAL_ID + " = " + TSong.TABLE_NAME_SONG + "." + TSong.FIELD_NAME_SONG_ID
					+ " WHERE " + TSongAlias.TABLE_NAME_SONG_ALIAS + "." + TSongAlias.FIELD_NAME_SONG_ALIAS_ALIAS + " = @alias "
					+ " AND " + TSongAlias.TABLE_NAME_SONG_ALIAS + "." + TSongAlias.FIELD_NAME_SONG_ALIAS_INVALID + " = 0";
			oInfoDbCmd.Parameters.Add(new SQLiteParameter("@alias", oAlias));

			using (SQLiteDataReader aReader = oInfoDbCmd.ExecuteReader())
			{
				while (aReader.Read())
				{
					return aReader[TSong.FIELD_NAME_SONG_NAME].ToString();
				}
			}

			return oAlias;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：DataGrid 表示を更新
		// --------------------------------------------------------------------
		private void TimerUpdateDg_Tick(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				if (mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.DoFolderTask])
				{
					UpdateDirtyDgWithInvoke();
				}
			}
			catch (Exception oExcep)
			{
				mTimerUpdateDg.IsEnabled = false;
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "タイマー時エラー：\n" + oExcep.Message);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// ゆかり検索対象フォルダー一覧の表示内容を更新
		// 高速化のため、DirtyDg はロックしない
		// 同時に DirtyDg を更新することにより競合が発生し、
		// 正しく DataGrid が更新されない場合があるが、フォルダータスク完了時には必ず DataGrid が更新されるため、
		// 一時的な事象として許容する
		// --------------------------------------------------------------------
		private void UpdateDirtyDgWithInvoke(Boolean oForceUpdate = false)
		{
			if (!oForceUpdate && !mDirtyDg)
			{
				return;
			}
			//Debug.WriteLine("UpdateDirtyDg() update 実施 " + Environment.TickCount);

			// 表示更新
			Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
			{
				lock (mTargetFolderInfos)
				{
					UpdateTargetFolderInfosVisible();
				}
			}));

			// フラグクリア
			mDirtyDg = false;
		}

		// --------------------------------------------------------------------
		// ゆかり検索対象フォルダー一覧のデータソースを更新（表示されるべき項目の数が変わった場合に呼ばれるべき）
		// UI スレッドのみ呼び出し可、呼び出し元において lock(mTargetFolderInfos) 必須
		// --------------------------------------------------------------------
		private void UpdateTargetFolderInfosVisible()
		{
			Debug.Assert(Thread.CurrentThread.ManagedThreadId == Application.Current.Dispatcher.Thread.ManagedThreadId, "UpdateTargetFolderInfosVisible() not UI thread");
			Debug.Assert(Monitor.IsEntered(mTargetFolderInfos), "UpdateTargetFolderInfosVisible() not locked");
			mMainWindowViewModel.TargetFolderInfosVisible = mTargetFolderInfos.Where(x => x.Visible).ToList();
		}




	}
}
