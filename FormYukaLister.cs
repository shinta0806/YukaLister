// ============================================================================
// 
// メインウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ・ワーカースレッドで、mTargetFolderInfos をロックしながら Invoke() してはいけない
// （mTargetFolderInfos をロックしながら mLogWriter.ShowLogMessage() でメッセージボックスを表示するのもダメ）
// メインスレッドが mTargetFolderInfos をロックした際にデッドロックになる
// ワーカースレッドから呼びだされる関数では、Invoke() してからロックする
// ・DGV の行番号は、非表示の行も勘定に入っている
// ・外部に書き出すパスはすべて extended-length なパス表記ではないものにする
// ----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
// ToDo:
// 検索ウィンドウで検索ワードも検索対象とする
// ファイル名から命名規則で取得できる情報を精査する
// データベースクラスのプロパティーをインターフェースにしたらどうか？
// 同期ダウンロード時、IdPrefix が一致するものは無効でもダウンロードする
// （Id カウンターリセット時にユニーク制約に引っかかってアップロードできないのを防止）
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YukaLister.Shared;

namespace YukaLister
{
	public partial class FormYukaLister : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormYukaLister()
		{
			InitializeComponent();
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// メッセージハンドラ
		// --------------------------------------------------------------------
		protected override void WndProc(ref Message oMsg)
		{
			switch ((UInt32)oMsg.Msg)
			{
				case WindowsApi.WM_DEVICECHANGE:
					WmDeviceChange(ref oMsg);
					break;
				case WindowsApi.WM_SHNOTIFY:
					WmShNotify(ref oMsg);
					break;
				case YlCommon.WM_LAUNCH_FOLDER_TASK_REQUESTED:
					WmLaunchFolderTaskRequested();
					break;
				case YlCommon.WM_LAUNCH_LIST_TASK_REQUESTED:
					WmLaunchListTaskRequested();
					break;
			}
			base.WndProc(ref oMsg);
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ゆかりすたーの動作状況表示
		private readonly Color[] YUKA_LISTER_STATUS_BACK_COLORS = { Color.FromArgb(225, 225, 255), Color.FromArgb(225, 255, 225), Color.FromArgb(255, 225, 225) };
		private readonly Color[] YUKA_LISTER_STATUS_COLORS = { Color.Blue, Color.LimeGreen, Color.Red };
		private readonly String[] YUKA_LISTER_STATUS_ICONS = { "●", ">>>", "●" };

		// 背景色（YUKA_LISTER_STATUS_BACK_COLORS 以外）
		//private readonly Color BACK_COLOR_NORMAL = Color.FromArgb(255, 255, 255);

		// スマートトラック判定用の単語（小文字表記、両端を | で括る）
		private const String OFF_VOCAL_WORDS = "|cho|cut|dam|guide|guidevocal|inst|inst+cho|joy|off|off+cho|offcho|offvocal|offのみ|vc|オフボ|オフボーカル|ボイキャン|ボーカルキャンセル|配信|";
		private const String BOTH_VOCAL_WORDS = "|2tr|2ch|onoff|offon|";

		// 自動追加情報記録ファイル名
		private const String FILE_NAME_AUTO_TARGET_INFO = YlCommon.APP_ID + "AutoTarget" + Common.FILE_EXT_CONFIG;

		// 改訂履歴ファイル
		private const String FILE_NAME_HISTORY = "YukaLister_History_JPN.txt";

		// アプリケーション構成ファイル
		private const String FILE_NAME_APP_CONFIG = "YukaLister.exe.config";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 動作状況
		private YukaListerStatus mYukaListerStatus;

		// 動作状況メッセージ
		private String mYukaListerStatusMessage;
		private String mYukaListerStatusSubMessage;
		private Boolean[] mEnabledYukaListerStatusRunningMessages = new Boolean[(Int32)YukaListerStatusRunningMessage.__End__];

		// ゆかり検索対象フォルダー
		private List<TargetFolderInfo> mTargetFolderInfos = new List<TargetFolderInfo>();

		// 環境設定
		private YukaListerSettings mYukaListerSettings = new YukaListerSettings();

		// 番組分類統合用
		private Dictionary<String, String> mCategoryUnityMap;

		// DGV セルスタイル
		private DataGridViewCellStyle[] mCellStyles;

		// DoFolderTaskByWorker() による DGV 更新判定用：削除により実際には当該行が存在しない可能性があることに注意
		private Int32 mDirtyDgvLineMin;
		private Int32 mDirtyDgvLineMax;

		// 終了時タスク安全中断用
		private CancellationTokenSource mClosingCancellationTokenSource = new CancellationTokenSource();

		// ログ
		private LogWriter mLogWriter;

		// メインウィンドウ上で時間のかかるタスクが多重起動されるのを抑止する
		private Object mGeneralTaskLock = new Object();

		// フォルダータスクが多重起動されるのを抑止する
		private Object mFolderTaskLock = new Object();

		// リストタスクが多重起動されるのを抑止する
		private Object mListTaskLock = new Object();

		// 同期タスクが多重起動されるのを抑止する
		private Object mSyncTaskLock = new Object();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// カテゴリーマスターテーブルの既定レコードを挿入
		// 旧バージョンのゆかりすたーを使用していてレコードが不足している場合用
		// --------------------------------------------------------------------
		private void AddMusicInfoDbCategoryDefaultRecordsIfNeeded()
		{
			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				using (DataContext aContext = new DataContext(aConnection))
				{
					Table<TCategory> aTableCategory = aContext.GetTable<TCategory>();

					if (YlCommon.SelectCategoriesByName(aConnection, "一般").Count == 0)
					{
						aTableCategory.InsertOnSubmit(YlCommon.CreateCategoryRecord(103, "一般", "イッパン"));
					}

					aContext.SubmitChanges();
				}
			}
		}

		// --------------------------------------------------------------------
		// フォルダー（サブフォルダー含む）を対象フォルダーに追加
		// 汎用ワーカースレッドで実行されることが前提
		// ＜引数＞ oParentFolder: （extended-length ではない）通常表記
		// --------------------------------------------------------------------
		private void AddTargetFolderByWorker(String oParentFolder)
		{
			try
			{
				// 正当性の確認
				if (String.IsNullOrEmpty(oParentFolder))
				{
					return;
				}

				// "E:" のような '\\' 無しのドライブ名は挙動が変なので 3 文字以上を対象とする
				if (oParentFolder.Length < 3)
				{
					return;
				}

				String aParentFolderExLen = YlCommon.ExtendPath(oParentFolder);
				String aParentFolderForDisp = YlCommon.ShortenPath(aParentFolderExLen);
				if (!Directory.Exists(aParentFolderExLen))
				{
					mLogWriter.ShowLogMessage(TraceEventType.Error, aParentFolderForDisp + " が見つかりません。", true);
					return;
				}

				// 準備
				DisableComponentsWithInvoke();
				SetCursorWithInvoke(Cursors.WaitCursor);
				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.AddTargetFolder] = true;
				SetYukaListerStatusWithInvoke();
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aParentFolderForDisp + " とそのサブフォルダーを検索対象に追加予定としています...");

				// 検索と重複チェック
#if DEBUG
				Int32 aTick2 = Environment.TickCount;
#endif
				List<String> aFolders = FindSubFolders(aParentFolderExLen);
#if DEBUG
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "サブフォルダー検索所要時間：" + (Environment.TickCount - aTick2).ToString("#,0"));
#endif
				Boolean aFullAdded = false;
				Boolean aPartialAdded = false;
				lock (mTargetFolderInfos)
				{
					for (Int32 i = 0; i < aFolders.Count; i++)
					{
						Int32 aRowIndex = FindTargetFolderInfo(aFolders[i]);
						if (aRowIndex >= 0)
						{
							aFullAdded = (i == 0);
							aPartialAdded = (i != 0);
							break;
						}
					}
				}
				if (aFullAdded)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Error, aParentFolderForDisp + "\nは既に追加されています。");
					return;
				}
				if (aPartialAdded)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Error, aParentFolderForDisp
							+ "\nのサブフォルダーが既に追加されています。\nサブフォルダーを一旦削除してから追加しなおして下さい。");
					return;
				}

				Invoke(new Action(() =>
				{
					lock (mTargetFolderInfos)
					{
						Int32 aParentIndex = mTargetFolderInfos.Count;

						// 追加
						foreach (String aFolder in aFolders)
						{
							TargetFolderInfo aTargetFolderInfo = new TargetFolderInfo();
							aTargetFolderInfo.Path = aFolder;
							aTargetFolderInfo.ParentPath = aFolders[0];
							aTargetFolderInfo.FolderTask = FolderTask.Add;
							aTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.Queued;
							aTargetFolderInfo.FolderExcludeSettingsStatus = FolderExcludeSettingsStatus.Unchecked;
							aTargetFolderInfo.FolderSettingsStatus = FolderSettingsStatus.Unchecked;
							mTargetFolderInfos.Add(aTargetFolderInfo);
						}

						// 親設定
						mTargetFolderInfos[aParentIndex].IsParent = true;
						mTargetFolderInfos[aParentIndex].IsOpen = true;
						mTargetFolderInfos[aParentIndex].NumTotalFolders = aFolders.Count;

						// その他
						mTargetFolderInfos.Sort(TargetFolderInfo.Compare);

						// ロック中での表示
						UpdateDataGridViewTargetFolders();
					}
				}));
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aFolders.Count.ToString("#,0")
						+ " 個のフォルダーを検索対象に追加予定としました。");
				mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();


				// 自動対象情報更新
				AdjustAutoTargetInfoIfNeeded(aParentFolderExLen);

				Invoke(new Action(() =>
				{
					// フォルダータスク実行
					WindowsApi.PostMessage(Handle, YlCommon.WM_LAUNCH_FOLDER_TASK_REQUESTED, (IntPtr)0, (IntPtr)0);

					// 表示
					UpdateButtonTFounds();
				}));
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー追加タスク実行時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 後片付け
				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.AddTargetFolder] = false;
				SetYukaListerStatusWithInvoke();
				SetCursorWithInvoke(Cursors.Default);
				EnableComponentsWithInvoke();
			}
		}

		// --------------------------------------------------------------------
		// 自動追加フォルダーを最適化
		// --------------------------------------------------------------------
		private void AdjustAutoTargetInfoIfNeeded(String oFolderExLen)
		{
			if (!IsAutoTargetDrive(oFolderExLen))
			{
				return;
			}

			String aDriveLetterExLen = oFolderExLen.Substring(0, YlCommon.EXTENDED_LENGTH_PATH_PREFIX.Length + 2);
			AutoTargetInfo aAutoTargetInfo = new AutoTargetInfo();
			lock (mTargetFolderInfos)
			{
				for (Int32 i = 0; i < mTargetFolderInfos.Count; i++)
				{
					if (mTargetFolderInfos[i].IsParent && mTargetFolderInfos[i].FolderTask != FolderTask.Remove
							&& mTargetFolderInfos[i].Path.StartsWith(aDriveLetterExLen, StringComparison.OrdinalIgnoreCase))
					{
						aAutoTargetInfo.Folders.Add(mTargetFolderInfos[i].Path.Substring(YlCommon.EXTENDED_LENGTH_PATH_PREFIX.Length + 2));
					}
				}
			}
			SaveAutoTargetInfo(oFolderExLen, aAutoTargetInfo);
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

			String[] aTracks = oTrack.Split(new Char[] { '-', '_', ',', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
		// ＜引数＞ oFolder: extended-length でも通常表記でも可
		// --------------------------------------------------------------------
		private String AutoTargetInfoPath(String oFolder)
		{
			return YlCommon.ShortenPath(oFolder).Substring(0, 3) + FILE_NAME_AUTO_TARGET_INFO;
		}

		// --------------------------------------------------------------------
		// DGV 更新用フラグをクリア
		// --------------------------------------------------------------------
		private void ClearDirtyDgvLines()
		{
			mDirtyDgvLineMin = Int32.MaxValue;
			mDirtyDgvLineMax = Int32.MinValue;
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
		// 指定されたデータベース内にある、指定されたテーブルの件数を数える
		// --------------------------------------------------------------------
		private Int32 CountDbRecord<T>(SQLiteConnection oConnection) where T : class
		{
			try
			{
				using (DataContext aContext = new DataContext(oConnection))
				{
					Table<T> aTable = aContext.GetTable<T>();
					return aTable.Count();
				}
			}
			catch (Exception)
			{
				// DB が存在してテーブルが存在しない場合は
				// SQL logic error or missing database no such table: <TableName>
				// のような例外が発生する
				return 0;
			}
		}

		// --------------------------------------------------------------------
		// ゆかり用データベース（ディスク、メモリ両方）を作成
		// --------------------------------------------------------------------
		private void CreateYukariDb()
		{
			if (!IsYukariConfigPathSet())
			{
				return;
			}

			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ゆかり用データベースを作成します...");

			// 作業用インメモリデータベースを作成
			YlCommon.YukariDbInMemoryConnection = YlCommon.CreateDbConnection(":memory:");
			using (SQLiteCommand aCmd = new SQLiteCommand(YlCommon.YukariDbInMemoryConnection))
			{
				CreateYukariDbFoundTable(aCmd);
			}
			YlCommon.CreateDbPropertyTable(YlCommon.YukariDbInMemoryConnection);

			// ディスクにコピー
			Directory.CreateDirectory(Path.GetDirectoryName(mYukaListerSettings.YukariDbInDiskPath()));
			YlCommon.CopyYukariDb(mYukaListerSettings);
		}

		// --------------------------------------------------------------------
		// ゆかり用データベースの中にテーブルを作成
		// --------------------------------------------------------------------
		private void CreateYukariDbFoundTable(SQLiteCommand oCmd)
		{
			// テーブル作成
			List<String> aUniques = new List<String>();
			aUniques.Add(TFound.FIELD_NAME_FOUND_UID);
			LinqUtils.CreateTable(oCmd, typeof(TFound), aUniques);

			// インデックス作成
			List<String> aIndices = new List<String>();
			aIndices.Add(TFound.FIELD_NAME_FOUND_HEAD);
			aIndices.Add(TFound.FIELD_NAME_FOUND_LAST_WRITE_TIME);
			aIndices.Add(TSong.FIELD_NAME_SONG_NAME);
			aIndices.Add(TSong.FIELD_NAME_SONG_RUBY);
			aIndices.Add(TSong.FIELD_NAME_SONG_RELEASE_DATE);
			aIndices.Add(TFound.FIELD_NAME_FOUND_TIE_UP_NAME);
			aIndices.Add(TTieUp.FIELD_NAME_TIE_UP_RUBY);
			aIndices.Add(TFound.FIELD_NAME_FOUND_CATEGORY_NAME);
			LinqUtils.CreateIndex(oCmd, LinqUtils.TableName(typeof(TFound)), aIndices);
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// ＜引数＞ oDriveLetter: "A:" のようにコロンまで
		// --------------------------------------------------------------------
		private async void DeviceArrival(String oDriveLetter)
		{
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "リムーバブルドライブが接続されました：" + oDriveLetter);
			AutoTargetInfo aAutoTargetInfo = LoadAutoTargetInfo(oDriveLetter + "\\");

			// 複数フォルダーが指定されている場合に同時実行できないため、async の終了を待ってから次のフォルダーを処理する
			foreach (String aFolder in aAutoTargetInfo.Folders)
			{
				await YlCommon.LaunchTaskAsync(AddTargetFolderByWorker, mGeneralTaskLock, oDriveLetter + aFolder);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private async void DeviceRemoveComplete(String oDriveLetter)
		{
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "リムーバブルドライブが切断されました：" + oDriveLetter);

			String aDriveLetterExLen = YlCommon.ExtendPath(oDriveLetter);
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

			// 複数フォルダーが指定されている場合に同時実行できないため、async の終了を待ってから次のフォルダーを処理する
			foreach (String aFolder in aRemoveFolders)
			{
				await YlCommon.LaunchTaskAsync(RemoveTargetFolderByWorker, mGeneralTaskLock, aFolder);
			}
		}

		// --------------------------------------------------------------------
		// UI 無効化（時間のかかる処理実行時用）
		// --------------------------------------------------------------------
		private void DisableComponentsWithInvoke()
		{
			Invoke(new Action(() =>
			{
				ButtonYukaListerSettings.Enabled = false;
				DataGridViewTargetFolders.Enabled = false;
				ButtonAddTargetFolder.Enabled = false;
				ButtonRemoveTargetFolder.Enabled = false;
				ButtonFolderSettings.Enabled = false;
			}));
		}

		// --------------------------------------------------------------------
		// フォルダータスク実行
		// フォルダータスクワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorker(Object oDummy)
		{
			try
			{
				ClearDirtyDgvLines();
				TargetFolderInfo aPrevParentTargetFolderInfo = null;

				for (; ; )
				{
					if (mYukaListerStatus == YukaListerStatus.Error)
					{
						mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "エラー発生中のためフォルダータスクを実行できません。");
						break;
					}

					// 実行すべきタスクを確認
					TargetFolderInfo aTargetFolderInfo = null;
					Int32 aTargetFolderInfoIndex = -1;
					lock (mTargetFolderInfos)
					{
						for (Int32 i = 0; i < mTargetFolderInfos.Count; i++)
						{
							if (mTargetFolderInfos[i].FolderTaskStatus == FolderTaskStatus.Queued)
							{
								aTargetFolderInfo = mTargetFolderInfos[i];
								aTargetFolderInfoIndex = i;
								break;
							}
						}
					}
					if (aTargetFolderInfo == null)
					{
						DoFolderTaskByWorkerPrevParentChangedIfNeededWithInvoke(aPrevParentTargetFolderInfo, aTargetFolderInfo);
						mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "実行予定のフォルダータスクをすべて実行しました。");

						// リストタスク実行
						Invoke(new Action(() =>
						{
							WindowsApi.PostMessage(Handle, YlCommon.WM_LAUNCH_LIST_TASK_REQUESTED, (IntPtr)0, (IntPtr)0);
						}));
						break;
					}

					// 情報更新
					aTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.Running;
					mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "対象：" + aTargetFolderInfo.FolderTaskStatus.ToString() + " / " + aTargetFolderInfo.Path);
					//InvalidateDataGridViewTargetFoldersWithInvoke(aTargetFolderInfo.Path);
					ExtendDirtyDgvLines(aTargetFolderInfoIndex);
					if (!mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.DoFolderTask])
					{
						mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.DoFolderTask] = true;
						SetYukaListerStatusWithInvoke();
					}

					// 親の情報更新
					TargetFolderInfo aParentTargetFolderInfo = null;
					Int32 aParentTargetFolderInfoIndex;
					lock (mTargetFolderInfos)
					{
						aParentTargetFolderInfoIndex = FindTargetFolderInfo(aTargetFolderInfo.ParentPath);
						aParentTargetFolderInfo = mTargetFolderInfos[aParentTargetFolderInfoIndex];
					}
					DoFolderTaskByWorkerPrevParentChangedIfNeededWithInvoke(aPrevParentTargetFolderInfo, aTargetFolderInfo);
					if (!aParentTargetFolderInfo.IsChildRunning)
					{
						aParentTargetFolderInfo.IsChildRunning = true;
						//InvalidateDataGridViewTargetFoldersWithInvoke(aParentTargetFolderInfo.Path);
						ExtendDirtyDgvLines(aParentTargetFolderInfoIndex);
					}

					// FolderTask ごとの処理
					switch (aTargetFolderInfo.FolderTask)
					{
						case FolderTask.Add:
							DoFolderTaskByWorkerAdd(aTargetFolderInfo, aParentTargetFolderInfo);
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

					ExtendDirtyDgvLines(aTargetFolderInfoIndex);

					// 次の準備
					aPrevParentTargetFolderInfo = aParentTargetFolderInfo;

#if false
					// 次のフォルダータスク実行
					Invoke(new Action(() =>
					{
						WindowsApi.PostMessage(Handle, YlCommon.WM_LAUNCH_FOLDER_TASK_REQUESTED, (IntPtr)0, (IntPtr)0);
					}));
#endif
				}
			}
			catch (OperationCanceledException)
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "フォルダータスクを中止しました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダータスク実行時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				if (!mClosingCancellationTokenSource.IsCancellationRequested)
				{
					mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.DoFolderTask] = false;
					SetYukaListerStatusWithInvoke();
					UpdateDataGridViewTargetFoldersWithInvoke();
					Invoke(new Action(() =>
					{
						UpdateButtonTFounds();
					}));
				}
			}
		}

		// --------------------------------------------------------------------
		// フォルダー追加タスク実行
		// フォルダータスクワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerAdd(TargetFolderInfo oTargetFolderInfo, TargetFolderInfo oParentTargetFolderInfo)
		{
			// 検索
			FindNicoKaraFiles(oTargetFolderInfo.Path);

#if DEBUGz
			Thread.Sleep(1000);
#endif

			// 状況更新
			// mTargetFolderInfos にはアクセスしないが RemoveTargetFolderByWorker() の状況更新と競合しないようにロックした上で実行する
			lock (mTargetFolderInfos)
			{
				// 追加している間にユーザーから削除指定された場合は削除を優先するので状態を更新しない
				// 追加している間に環境設定が変わって待機中になった場合は新たな設定で追加が必要なので状態を更新しない
				if (oTargetFolderInfo.FolderTask == FolderTask.Add && oTargetFolderInfo.FolderTaskStatus == FolderTaskStatus.Running)
				{
					oTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.Done;
				}
			}

#if false
			// 情報更新
			InvalidateDataGridViewTargetFoldersWithInvoke(oTargetFolderInfo.Path);

			// 親の情報更新
			if (oParentTargetFolderInfo != null)
			{
				oParentTargetFolderInfo.IsChildRunning = false;
				InvalidateDataGridViewTargetFoldersWithInvoke(oParentTargetFolderInfo.Path);
			}
#endif
		}

		// --------------------------------------------------------------------
		// 直前の親と違う親になった：直前の親のタスクは追加
		// Invoke() および lock されていることが必須
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerPrevParentChangedAdd(Int32 oPrevParentTargetFolderInfoIndex, TargetFolderInfo oPrevParentTargetFolderInfo)
		{
			oPrevParentTargetFolderInfo.IsChildRunning = false;
			ExtendDirtyDgvLines(oPrevParentTargetFolderInfoIndex);
		}

		// --------------------------------------------------------------------
		// 直前の親と違う親になった場合に処理を実行
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerPrevParentChangedIfNeededWithInvoke(TargetFolderInfo oPrevParentTargetFolderInfo, TargetFolderInfo oParentTargetFolderInfo)
		{
			if (oPrevParentTargetFolderInfo != null && oPrevParentTargetFolderInfo != oParentTargetFolderInfo)
			{
				Invoke(new Action(() =>
				{
					lock (mTargetFolderInfos)
					{
						// 直前の親の子供のタスクがすべて完了しているか確認する
						// 途中でユーザーがフォルダーを追加した場合など、すべて完了していない場合でも親が変わる場合があるので、確認が必要
						Debug.Assert(oPrevParentTargetFolderInfo.IsParent, "DoFolderTaskByWorkerPrevParentChangedIfNeededWithInvoke() child");
						Int32 aPrevParentTargetFolderInfoIndex = FindTargetFolderInfo(oPrevParentTargetFolderInfo.Path);
						Boolean aAllDone = true;
						for (Int32 i = aPrevParentTargetFolderInfoIndex; i < aPrevParentTargetFolderInfoIndex + oPrevParentTargetFolderInfo.NumTotalFolders; i++)
						{
							if (mTargetFolderInfos[i].FolderTaskStatus != FolderTaskStatus.Done)
							{
								aAllDone = false;
								break;
							}
						}
						if (!aAllDone)
						{
							mLogWriter.ShowLogMessage(TraceEventType.Verbose, "DoFolderTaskByWorkerPrevParentChangedIfNeededWithInvoke() not all done");
							return;
						}

						// 子供のタスクが完了しているので処理を実行
						switch (oPrevParentTargetFolderInfo.FolderTask)
						{
							case FolderTask.Add:
								DoFolderTaskByWorkerPrevParentChangedAdd(aPrevParentTargetFolderInfoIndex, oPrevParentTargetFolderInfo);
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
				}));
			}
		}

		// --------------------------------------------------------------------
		// 直前の親と違う親になった：直前の親のタスクは削除
		// Invoke() および lock されていることが必須
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerPrevParentChangedRemove(Int32 oPrevParentTargetFolderInfoIndex, TargetFolderInfo oPrevParentTargetFolderInfo)
		{
			// 子もろとも削除
			mTargetFolderInfos.RemoveRange(oPrevParentTargetFolderInfoIndex, oPrevParentTargetFolderInfo.NumTotalFolders);

			// 情報更新
			UpdateDataGridViewTargetFolders();
		}

		// --------------------------------------------------------------------
		// フォルダー削除タスク実行
		// フォルダータスクワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void DoFolderTaskByWorkerRemove(TargetFolderInfo oTargetFolderInfo, TargetFolderInfo oParentTargetFolderInfo)
		{
			// 削除
			RemoveNicoKaraFiles(oTargetFolderInfo.Path);


#if DEBUGz
			Thread.Sleep(1000);
#endif

			// 状況更新
			lock (mTargetFolderInfos)
			{
				oTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.Done;
#if false
				if (oTargetFolderInfo.IsParent)
				{
					// 親フォルダーの場合
					if (oTargetFolderInfo.NumTotalFolders == 1)
					{
						// 子供がいない場合は削除
						mTargetFolderInfos.Remove(oTargetFolderInfo);
					}
					else
					{
						// 子供がいる場合は待機
						oTargetFolderInfo.FolderTaskStatus = FolderTaskStatus.Done;
					}
				}
				else
				{
					// 子フォルダーの場合は自身（子供）を削除
					mTargetFolderInfos.Remove(oTargetFolderInfo);

					// 親を操作
					oParentTargetFolderInfo.NumTotalFolders--;
					if (oParentTargetFolderInfo.NumTotalFolders == 1)
					{
						mTargetFolderInfos.Remove(oParentTargetFolderInfo);
					}
				}
#endif
			}
		}

		// --------------------------------------------------------------------
		// UI 有効化
		// --------------------------------------------------------------------
		private void EnableComponentsWithInvoke()
		{
			if (mClosingCancellationTokenSource.Token.IsCancellationRequested)
			{
				return;
			}

			Invoke(new Action(() =>
			{
				ButtonYukaListerSettings.Enabled = true;
				DataGridViewTargetFolders.Enabled = true;
				ButtonAddTargetFolder.Enabled = true;
				ButtonRemoveTargetFolder.Enabled = true;
				ButtonFolderSettings.Enabled = true;
			}));
		}

		// --------------------------------------------------------------------
		// 指定された行を、DGV の更新が必要な行範囲に含める
		// --------------------------------------------------------------------
		private void ExtendDirtyDgvLines(Int32 oDirtyLine)
		{
			if (oDirtyLine < mDirtyDgvLineMin)
			{
				mDirtyDgvLineMin = oDirtyLine;
			}
			if (oDirtyLine > mDirtyDgvLineMax)
			{
				mDirtyDgvLineMax = oDirtyLine;
			}
		}

		// --------------------------------------------------------------------
		// 指定フォルダ内のファイルを検索してゆかり用データベースに追加
		// ファイルは再帰検索しない
		// --------------------------------------------------------------------
		private void FindNicoKaraFiles(String oFolderPath)
		{
			// フォルダー設定を読み込む
			FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings(oFolderPath);
			if (aFolderSettingsInDisk.IsExclude)
			{
				return;
			}
			FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);

			using (SQLiteConnection aMusicInfoDbConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				using (SQLiteCommand aMusicInfoDbCmd = new SQLiteCommand(aMusicInfoDbConnection))
				{
					using (DataContext aMusicInfoDbContext = new DataContext(aMusicInfoDbConnection))
					{
						using (DataContext aYukariDbContext = new DataContext(YlCommon.YukariDbInMemoryConnection))
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
								aAllPathes = Directory.GetFiles(oFolderPath);
							}
							catch (Exception)
							{
								return;
							}

							// カテゴリー正規化用
							List<String> aCategoryNames = YlCommon.SelectCategoryNames(aMusicInfoDbConnection);

							// 挿入
							foreach (String aPath in aAllPathes)
							{
								if (!mYukaListerSettings.TargetExts.Contains(Path.GetExtension(aPath).ToLower()))
								{
									continue;
								}

								TFound aRecord = new TFound();
								aRecord.Uid = aUid;
								aRecord.Path = YlCommon.ShortenPath(aPath);
								aRecord.Folder = Path.GetDirectoryName(aRecord.Path).ToLower();
								FileInfo aFileInfo = new FileInfo(aPath);
								aRecord.LastWriteTime = JulianDay.DateTimeToModifiedJulianDate(aFileInfo.LastWriteTime);
								aRecord.FileSize = aFileInfo.Length;
								SetTFoundValue(aRecord, aFolderSettingsInMemory, aMusicInfoDbCmd, aMusicInfoDbContext, aCategoryNames);
								aTableFound.InsertOnSubmit(aRecord);

								aUid++;
							}

							mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

							// コミット
							aYukariDbContext.SubmitChanges();

							mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();
						}
					}
				}
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
			mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 指定フォルダー
			oFolders.Add(oFolderExLen);
			mYukaListerStatusSubMessage = YlCommon.ShortenPath(oFolderExLen);

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
		// oPath を持つ TargetFolderInfo を探してインデックスを返す
		// 呼びだし元において lock(mTargetFolderInfos) しておく必要がある
		// --------------------------------------------------------------------
		private Int32 FindTargetFolderInfo(String oPath)
		{
			for (Int32 i = 0; i < mTargetFolderInfos.Count; i++)
			{
				if (YlCommon.IsSamePath(oPath, mTargetFolderInfos[i].Path))
				{
					return i;
				}
			}

			return -1;
		}

#if false
		// --------------------------------------------------------------------
		// フォルダーの設定有無を表す文字列
		// --------------------------------------------------------------------
		private String FolderSettingsStatusString(FolderSettingsStatus oStatus)
		{
			switch (oStatus)
			{
				case FolderSettingsStatus.None:
					return "無";
				case FolderSettingsStatus.Set:
					return "有";
				case FolderSettingsStatus.Inherit:
					return "親に有";
				default:
					Debug.Assert(false, "FolderSettingsStatusString() bad FolderSettingsStatus");
					return null;
			}
		}
#endif

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			Debug.Assert(YUKA_LISTER_STATUS_BACK_COLORS.Length == (Int32)YukaListerStatus.__End__, "Init() bad YUKA_LISTER_STATUS_BACK_COLORS.Length");
			Debug.Assert(YUKA_LISTER_STATUS_COLORS.Length == (Int32)YukaListerStatus.__End__, "Init() bad YUKA_LISTER_STATUS_COLORS.Length");
			Debug.Assert(YUKA_LISTER_STATUS_ICONS.Length == (Int32)YukaListerStatus.__End__, "Init() bad YUKA_LISTER_STATUS_ICONS.Length");
			Debug.Assert(YlCommon.FOLDER_SETTINGS_STATUS_TEXTS.Length == (Int32)FolderSettingsStatus.__End__, "Init() bad YlCommon.FOLDER_SETTINGS_STATUS_TEXTS.Length");
			Debug.Assert(YlCommon.MUSIC_INFO_DB_TABLE_NAMES.Length == (Int32)MusicInfoDbTables.__End__, "Init() bad YlCommon.MUSIC_INFO_DB_TABLE_NAMES.Length");
			Debug.Assert(YlCommon.MUSIC_INFO_DB_ID_COLUMN_NAMES.Length == (Int32)MusicInfoDbTables.__End__, "Init() bad YlCommon.MUSIC_INFO_DB_ID_COLUMN_NAMES.Length");
			Debug.Assert(YlCommon.MUSIC_INFO_DB_NAME_COLUMN_NAMES.Length == (Int32)MusicInfoDbTables.__End__, "Init() bad YlCommon.MUSIC_INFO_DB_NAME_COLUMN_NAMES.Length");
			Debug.Assert(YlCommon.MUSIC_INFO_DB_RUBY_COLUMN_NAMES.Length == (Int32)MusicInfoDbTables.__End__, "Init() bad YlCommon.MUSIC_INFO_DB_RUBY_COLUMN_NAMES.Length");
			Debug.Assert(YlCommon.MUSIC_INFO_DB_KEYWORD_COLUMN_NAMES.Length == (Int32)MusicInfoDbTables.__End__, "Init() bad YlCommon.MUSIC_INFO_DB_KEYWORD_COLUMN_NAMES.Length");
			Debug.Assert(YlCommon.MUSIC_INFO_ID_SECOND_PREFIXES.Length == (Int32)MusicInfoDbTables.__End__, "Init() bad YlCommon.MUSIC_INFO_ID_SECOND_PREFIXES.Length");
			Debug.Assert(YlCommon.YUKA_LISTER_STATUS_RUNNING_MESSAGES.Length == (Int32)YukaListerStatusRunningMessage.__End__, "Init() bad YlCommon.YUKA_LISTER_STATUS_RUNNING_MESSAGES.Length");
			Debug.Assert(YlCommon.OUTPUT_ITEM_NAMES.Length == (Int32)OutputItems.__End__, "Init() bad YlCommon.OUTPUT_ITEM_NAMES.Length");
#if DEBUG
			for (MusicInfoDbTables i = 0; i < MusicInfoDbTables.__End__; i++)
			{
				Debug.Assert(String.Compare(i.ToString(), YlCommon.MUSIC_INFO_DB_TABLE_NAMES[(Int32)i].Replace("_", ""), true) == 0, "Init() bad YlCommon.MUSIC_INFO_DB_TABLE_NAMES: " + i.ToString());
			}
#endif

			// ログ初期化
			// 頻度はまれだが、インポート時に大量のログが発生することがあるので、ファイルサイズを大きくしておく
			mLogWriter = new LogWriter(YlCommon.APP_ID);
			mLogWriter.ApplicationQuitToken = mClosingCancellationTokenSource.Token;
			mLogWriter.SimpleTraceListener.MaxSize = 5 * 1024 * 1024;
			YlCommon.LogWriter = mLogWriter;
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "起動しました：" + YlCommon.APP_NAME_J + " "
					+ YlCommon.APP_VER + " ====================");
#if DEBUG
			mLogWriter.ShowLogMessage(TraceEventType.Verbose, "デバッグモード：" + Common.DEBUG_ENABLED_MARK);
#endif

			// カレントフォルダー正規化（ゆかりから起動された場合はゆかりのフォルダーになっているため）
			Environment.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);

			// タイトルバー
			Text = YlCommon.APP_NAME_J;
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif
			mLogWriter.ShowLogMessage(TraceEventType.Verbose, "ProcessName: " + Process.GetCurrentProcess().ProcessName);

			// データグリッドビュー
			InitDataGridView();

			// 設定の読み込み
			mYukaListerSettings.Reload();
			if (mYukaListerSettings.TargetExts == null)
			{
				mYukaListerSettings.TargetExts = new List<String>();
			}
			if (mYukaListerSettings.TargetExts.Count == 0)
			{
				// 動画をアルファベット順に追加（比較的メジャーで現在もサポートが行われている形式のみ）
				mYukaListerSettings.TargetExts.Add(Common.FILE_EXT_AVI);
				mYukaListerSettings.TargetExts.Add(Common.FILE_EXT_MKV);
				mYukaListerSettings.TargetExts.Add(Common.FILE_EXT_MOV);
				mYukaListerSettings.TargetExts.Add(Common.FILE_EXT_MP4);
				mYukaListerSettings.TargetExts.Add(Common.FILE_EXT_MPG);
				mYukaListerSettings.TargetExts.Add(Common.FILE_EXT_WMV);
			}
#if false
			if (mYukaListerSettings.TargetFoldersExLen == null)
			{
				mYukaListerSettings.TargetFoldersExLen = new List<String>();
			}
#endif
			if (mYukaListerSettings.LastIdNumbers == null)
			{
				mYukaListerSettings.LastIdNumbers = new List<Int32>();
			}
			if (mYukaListerSettings.LastIdNumbers.Count < (Int32)MusicInfoDbTables.__End__)
			{
				mYukaListerSettings.LastIdNumbers.Clear();
				for (Int32 i = 0; i < (Int32)MusicInfoDbTables.__End__; i++)
				{
					mYukaListerSettings.LastIdNumbers.Add(0);
				}
			}

			// extended-length なパス表記の設定（.NET 4.6.2 以降のみ有効とする）
			SystemEnvironment aSystemEnvironment = new SystemEnvironment();
			Int32 aClrVer;
			aSystemEnvironment.GetClrVersionRegistryNumber(out aClrVer);
			YlCommon.IsExLenEnabled = (aClrVer >= 394802);
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "extended-length パスの使用：" + YlCommon.IsExLenEnabled.ToString());

			// 番組分類統合マップ初期化
			mCategoryUnityMap = YlCommon.CreateCategoryUnityMap();

			// 設計時サイズ以下にできないようにする
			MinimumSize = Size;

			// 終了時の状態を復元
			if (!mYukaListerSettings.Bounds.IsEmpty)
			{
				Bounds = mYukaListerSettings.Bounds;
			}
			Common.ContainFormIfNeeded(this);
			if (mYukaListerSettings.WindowMaximized)
			{
				WindowState = FormWindowState.Maximized;
			}

			// 楽曲情報データベース
			PrepareMusicInfoDb();

			// ゆかり用データベース
			CreateYukariDb();
		}

		// --------------------------------------------------------------------
		// データグリッドビュー初期化
		// --------------------------------------------------------------------
		private void InitDataGridView()
		{
			// セルスタイル
			mCellStyles = new DataGridViewCellStyle[(Int32)YukaListerStatus.__End__];
			for (Int32 i = 0; i < (Int32)YukaListerStatus.__End__; i++)
			{
				mCellStyles[i] = new DataGridViewCellStyle();
				mCellStyles[i].BackColor = YUKA_LISTER_STATUS_BACK_COLORS[i];
			}

			// 開閉
			ColumnAcc.Width = 20;

			// 状態
			ColumnStatus.Width = 100;

			// フォルダー
			ColumnFolder.Width = 500;

			// 設定有無
			ColumnSettingsExist.Width = 80;
		}

		// --------------------------------------------------------------------
		// DGV の状態列の再描画を引き起こす
		// --------------------------------------------------------------------
		private void InvalidateDataGridViewTargetFoldersWithInvoke(String oPath)
		{
			Invoke(new Action(() =>
			{
				lock (mTargetFolderInfos)
				{
					Int32 aRowIndex = FindTargetFolderInfo(oPath);
					if (aRowIndex < 0)
					{
						return;
					}
					DataGridViewTargetFolders.InvalidateCell((Int32)FolderColumns.Status, aRowIndex);
				}
			}));
		}

		// --------------------------------------------------------------------
		// 自動追加対象のドライブかどうか
		// --------------------------------------------------------------------
		private Boolean IsAutoTargetDrive(String oFolderExLen)
		{
			String aDriveLetter = YlCommon.ShortenPath(oFolderExLen).Substring(0, 1);
			DriveInfo aDriveInfo = new DriveInfo(aDriveLetter);
			if (!aDriveInfo.IsReady)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "IsAutoTargetDrive() 準備ができていない：" + aDriveLetter);
				return false;
			}

			// リムーバブルドライブのみを対象としたいが、ポータブル HDD/SSD も Fixed 扱いになるため、Fixed も対象とする
			switch (aDriveInfo.DriveType)
			{
				case DriveType.Fixed:
				case DriveType.Removable:
					mLogWriter.ShowLogMessage(TraceEventType.Verbose, "IsAutoTargetDrive() 対象：" + aDriveLetter);
					return true;
				default:
					mLogWriter.ShowLogMessage(TraceEventType.Verbose, "IsAutoTargetDrive() 非対象：" + aDriveLetter + ", " + aDriveInfo.DriveType.ToString());
					return false;
			}
		}

		// --------------------------------------------------------------------
		// ゆかり設定ファイルが正しく指定されているかどうか
		// --------------------------------------------------------------------
		private Boolean IsYukariConfigPathSet()
		{
			return File.Exists(mYukaListerSettings.YukariConfigPath());
		}

		// --------------------------------------------------------------------
		// 自動追加情報読み込み
		// 見つからない場合は null ではなく空のインスタンスを返す
		// ＜引数＞ oFolder: extended-length でも通常表記でも可
		// --------------------------------------------------------------------
		private AutoTargetInfo LoadAutoTargetInfo(String oFolder)
		{
			AutoTargetInfo aAutoTargetInfo = new AutoTargetInfo();

			try
			{
				aAutoTargetInfo = Common.Deserialize<AutoTargetInfo>(AutoTargetInfoPath(oFolder));
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
			DataGridViewTargetFolders.Invalidate();
		}

		// --------------------------------------------------------------------
		// 新バージョンで初回起動された時の処理を行う
		// --------------------------------------------------------------------
		private void NewVersionLaunched()
		{
			String aNewVerMsg;

			// α・β警告、ならびに、更新時のメッセージ（2017/01/09）
			// 新規・更新のご挨拶
			if (String.IsNullOrEmpty(mYukaListerSettings.PrevLaunchVer))
			{
				// 新規
				aNewVerMsg = "【初回起動】\n\n";
				aNewVerMsg += YlCommon.APP_NAME_J + "をダウンロードしていただき、ありがとうございます。";
			}
			else
			{
				aNewVerMsg = "【更新起動】\n\n";
				aNewVerMsg += YlCommon.APP_NAME_J + "を更新していただき、ありがとうございます。\n";
				aNewVerMsg += "更新内容については［ヘルプ→改訂履歴］メニューをご参照ください。";
			}

			// α・βの注意
			if (YlCommon.APP_VER.IndexOf("α") >= 0)
			{
				aNewVerMsg += "\n\nこのバージョンは開発途上のアルファバージョンです。\n"
						+ "使用前にヘルプをよく読み、注意してお使い下さい。";
			}
			else if (YlCommon.APP_VER.IndexOf("β") >= 0)
			{
				aNewVerMsg += "\n\nこのバージョンは開発途上のベータバージョンです。\n"
						+ "使用前にヘルプをよく読み、注意してお使い下さい。";
			}

			// 表示
			mLogWriter.ShowLogMessage(TraceEventType.Information, aNewVerMsg);

			// Zone ID 削除
			Common.DeleteZoneID(Path.GetDirectoryName(Application.ExecutablePath), SearchOption.AllDirectories);
		}

		// --------------------------------------------------------------------
		// リストタスク実行
		// リストタスクワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void OutputYukariListByWorker(Object oDummy)
		{
			try
			{
				if (!mYukaListerSettings.OutputYukari)
				{
					return;
				}

				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.ListTask] = true;
				SetYukaListerStatusWithInvoke();

				// メモリからディスクへコピー
				// ToDo: リストタスクから分離する
				YlCommon.CopyYukariDb(mYukaListerSettings);

				// リスト出力
				YukariOutputWriter aYukariOutputWriter = new YukariOutputWriter();
				aYukariOutputWriter.FolderPath = Path.GetDirectoryName(mYukaListerSettings.YukariDbInDiskPath()) + "\\";
				YlCommon.OutputList(aYukariOutputWriter, mYukaListerSettings);

				mLogWriter.ShowLogMessage(TraceEventType.Information, "リスト出力が完了しました。", true);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リストタスク実行時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.ListTask] = false;
				SetYukaListerStatusWithInvoke();
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースを準備する
		// --------------------------------------------------------------------
		private void PrepareMusicInfoDb()
		{
			if (!File.Exists(YlCommon.MusicInfoDbPath()))
			{
				// 新規作成
				YlCommon.CreateMusicInfoDb();
				return;
			}

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				TProperty aProperty = YlCommon.GetDbProperty(aConnection);
				if (aProperty.AppId != YlCommon.APP_ID)
				{
					// 何らかの原因（例えば前回データベース作成途中で異常終了したなど）でプロパティーが作成されていないので楽曲情報データベースを再作成
					YlCommon.CreateMusicInfoDb();
					return;
				}
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
					+ " WHERE " + TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS + "." + TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ALIAS + " = @alias";
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
		private void RemoveNicoKaraFiles(String oFolderPath)
		{
			using (DataContext aYukariDbContext = new DataContext(YlCommon.YukariDbInMemoryConnection))
			{
				Table<TFound> aTableFound = aYukariDbContext.GetTable<TFound>();
				IQueryable<TFound> aQueryResult =
						from x in aTableFound
						where x.Folder == YlCommon.ShortenPath(oFolderPath).ToLower()
						select x;
#if DEBUGz
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "RemoveNicoKaraFiles() " + YlCommon.ShortenPath(oFolderPath).ToLower());
				if (aQueryResult == null)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Verbose, "RemoveNicoKaraFiles() result null");
				}
				else
				{
					mLogWriter.ShowLogMessage(TraceEventType.Verbose, "RemoveNicoKaraFiles() del " + aQueryResult.Count() + " 件");
				}
#endif
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
				DisableComponentsWithInvoke();
				SetCursorWithInvoke(Cursors.WaitCursor);
				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.RemoveTargetFolder] = true;
				SetYukaListerStatusWithInvoke();
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, YlCommon.ShortenPath(oParentFolderExLen) + " とそのサブフォルダーを検索対象から削除予定としています...");

				Int32 aNumRemoveFolders;
				lock (mTargetFolderInfos)
				{
					Int32 aParentIndex = FindTargetFolderInfo(oParentFolderExLen);
					if (aParentIndex < 0 || !mTargetFolderInfos[aParentIndex].IsParent)
					{
						mLogWriter.ShowLogMessage(TraceEventType.Error, "削除対象の親フォルダーが見つかりませんでした。", true);
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

				}
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aNumRemoveFolders.ToString("#,0")
						+ " 個のフォルダーを検索対象から削除予定としました。");
				mClosingCancellationTokenSource.Token.ThrowIfCancellationRequested();

#if DEBUGz
				Thread.Sleep(2000);
#endif

				// 表示
				UpdateDataGridViewTargetFoldersWithInvoke();

				// 自動対象情報更新
				AdjustAutoTargetInfoIfNeeded(oParentFolderExLen);

				// フォルダータスク実行
				Invoke(new Action(() =>
				{
					WindowsApi.PostMessage(Handle, YlCommon.WM_LAUNCH_FOLDER_TASK_REQUESTED, (IntPtr)0, (IntPtr)0);
				}));
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー削除タスク実行時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 後片付け
				mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.RemoveTargetFolder] = false;
				SetYukaListerStatusWithInvoke();
				SetCursorWithInvoke(Cursors.Default);
				EnableComponentsWithInvoke();
			}
		}

		// --------------------------------------------------------------------
		// プレビュー設定が有効ならプレビュー用サーバーを開始
		// --------------------------------------------------------------------
		private void RunPreviewServerIfNeeded()
		{
			if (!mYukaListerSettings.ProvideYukariPreview)
			{
				return;
			}

			PreviewServer aPreviewServer = new PreviewServer(mYukaListerSettings, mClosingCancellationTokenSource.Token, mLogWriter);

			// async を待機しない
			Task aSuppressWarning = aPreviewServer.RunAsync();
		}

		// --------------------------------------------------------------------
		// 同期設定が有効なら同期処理を開始
		// --------------------------------------------------------------------
		private void RunSyncClientIfNeeded(Boolean oIsReget = false)
		{
			if (!mYukaListerSettings.SyncMusicInfoDb)
			{
				return;
			}

			SyncClient aSyncClient = new SyncClient(mYukaListerSettings, ToolStripStatusLabelBgStatus, mClosingCancellationTokenSource.Token, oIsReget);

			// async を待機しない
			Task aSuppressWarning = aSyncClient.RunAsync();
		}

		// --------------------------------------------------------------------
		// 自動追加情報保存
		// --------------------------------------------------------------------
		private void SaveAutoTargetInfo(String oFolderExLen, AutoTargetInfo oAutoTargetInfo)
		{
			try
			{
				String aPath = AutoTargetInfoPath(oFolderExLen);

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
		// データグリッドビューで選択されている TargetFolderInfo を返す
		// --------------------------------------------------------------------
		private TargetFolderInfo SelectedTargetFolderInfo()
		{
			foreach (DataGridViewRow aRow in DataGridViewTargetFolders.SelectedRows)
			{
				lock (mTargetFolderInfos)
				{
					if (aRow.Index < 0 || aRow.Index >= mTargetFolderInfos.Count)
					{
						return null;
					}
					return mTargetFolderInfos[aRow.Index];
				}
			}

			return null;
		}

		// --------------------------------------------------------------------
		// カーソル形状の設定
		// --------------------------------------------------------------------
		private void SetCursor(Cursor oCursor)
		{
			Capture = true;
			Cursor = oCursor;
			Capture = false;
		}

		// --------------------------------------------------------------------
		// カーソル形状の設定
		// --------------------------------------------------------------------
		private void SetCursorWithInvoke(Cursor oCursor)
		{
			if (mClosingCancellationTokenSource.Token.IsCancellationRequested)
			{
				return;
			}

			Invoke(new Action(() =>
			{
				SetCursor(oCursor);
			}));
		}

		// --------------------------------------------------------------------
		// 検出ファイルレコードの値を、フォルダー設定や楽曲情報データベースから検索して設定する
		// --------------------------------------------------------------------
		private void SetTFoundValue(TFound oRecord, FolderSettingsInMemory oFolderSettingsInMemory, SQLiteCommand oMusicInfoDbCmd, DataContext oMusicInfoDbContext,
				List<String> oCategoryNames)
		{
			// ファイル名・フォルダー固定値と合致する命名規則を探す
			Dictionary<String, String> aDicByFile = YlCommon.MatchFileNameRulesAndFolderRule(Path.GetFileNameWithoutExtension(oRecord.Path), oFolderSettingsInMemory);
			aDicByFile[YlCommon.RULE_VAR_PROGRAM] = ProgramOrigin(aDicByFile[YlCommon.RULE_VAR_PROGRAM], oMusicInfoDbCmd);
			aDicByFile[YlCommon.RULE_VAR_TITLE] = SongOrigin(aDicByFile[YlCommon.RULE_VAR_TITLE], oMusicInfoDbCmd);
			if (aDicByFile[YlCommon.RULE_VAR_CATEGORY] != null)
			{
				if (oCategoryNames.IndexOf(aDicByFile[YlCommon.RULE_VAR_CATEGORY]) < 0)
				{
					aDicByFile[YlCommon.RULE_VAR_CATEGORY] = null;
				}
			}

			// 楽曲情報データベースを適用
			SetTFoundValueByMusicInfoDb(oRecord, aDicByFile, oMusicInfoDbCmd, oMusicInfoDbContext);

			// 楽曲情報データベースに無かった項目をファイル名・フォルダー固定値から取得
			oRecord.Category = oRecord.Category == null ? aDicByFile[YlCommon.RULE_VAR_CATEGORY] : oRecord.Category;
			oRecord.TieUpName = oRecord.TieUpName == null ? aDicByFile[YlCommon.RULE_VAR_PROGRAM] : oRecord.TieUpName;
			oRecord.TieUpAgeLimit = oRecord.TieUpAgeLimit == 0 ? Common.StringToInt32(aDicByFile[YlCommon.RULE_VAR_AGE_LIMIT]) : oRecord.TieUpAgeLimit;
			oRecord.SongOpEd = oRecord.SongOpEd == null ? aDicByFile[YlCommon.RULE_VAR_OP_ED] : oRecord.SongOpEd;
			oRecord.SongName = oRecord.SongName == null ? aDicByFile[YlCommon.RULE_VAR_TITLE] : oRecord.SongName;
			if (oRecord.ArtistName == null && aDicByFile[YlCommon.RULE_VAR_ARTIST] != null)
			{
				// ファイル名から歌手名を取得できている場合は、楽曲情報データベースからフリガナを探す
				List<TPerson> aArtists;
				aArtists = YlCommon.SelectPeopleByName(oMusicInfoDbContext, aDicByFile[YlCommon.RULE_VAR_ARTIST]);
				if (aArtists.Count > 0)
				{
					// 歌手名が楽曲情報データベースに登録されていた場合はその情報を使う
					oRecord.ArtistName = aDicByFile[YlCommon.RULE_VAR_ARTIST];
					oRecord.ArtistRuby = aArtists[0].Ruby;
				}
				else
				{
					// 歌手名そのままでは楽曲情報データベースに登録されていない場合
					if (aDicByFile[YlCommon.RULE_VAR_ARTIST].IndexOf(YlCommon.VAR_VALUE_DELIMITER) >= 0)
					{
						// 区切り文字で区切られた複数の歌手名が記載されている場合は分解して解析する
						String[] aArtistNames = aDicByFile[YlCommon.RULE_VAR_ARTIST].Split(YlCommon.VAR_VALUE_DELIMITER[0]);
						foreach (String aArtistName in aArtistNames)
						{
							List<TPerson> aArtistsTmp = YlCommon.SelectPeopleByName(oMusicInfoDbContext, aArtistName);
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
						oRecord.ArtistName = aDicByFile[YlCommon.RULE_VAR_ARTIST];
					}
				}
			}
			oRecord.SongRuby = oRecord.SongRuby == null ? aDicByFile[YlCommon.RULE_VAR_TITLE_RUBY] : oRecord.SongRuby;
			oRecord.Worker = oRecord.Worker == null ? aDicByFile[YlCommon.RULE_VAR_WORKER] : oRecord.Worker;
			oRecord.Track = oRecord.Track == null ? aDicByFile[YlCommon.RULE_VAR_TRACK] : oRecord.Track;
			oRecord.SmartTrackOnVocal = !oRecord.SmartTrackOnVocal ? aDicByFile[YlCommon.RULE_VAR_ON_VOCAL] != null : oRecord.SmartTrackOnVocal;
			oRecord.SmartTrackOffVocal = !oRecord.SmartTrackOffVocal ? aDicByFile[YlCommon.RULE_VAR_OFF_VOCAL] != null : oRecord.SmartTrackOffVocal;
			oRecord.Comment = oRecord.Comment == null ? aDicByFile[YlCommon.RULE_VAR_COMMENT] : oRecord.Comment;

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
			if (oDicByFile[YlCommon.RULE_VAR_TITLE] == null)
			{
				return;
			}

			List<TSong> aSongs;
			// ファイル名で検索
			aSongs = YlCommon.SelectSongsByName(oMusicInfoDbContext, oDicByFile[YlCommon.RULE_VAR_TITLE]);

			// タイアップ名で絞り込み
			if (aSongs.Count > 1 && oDicByFile[YlCommon.RULE_VAR_PROGRAM] != null)
			{
				List<TSong> aSongsWithTieUp = new List<TSong>();
				foreach (TSong aSong in aSongs)
				{
					TTieUp aTieUp = YlCommon.SelectTieUpById(oMusicInfoDbContext, aSong.TieUpId);
					if (aTieUp != null && aTieUp.Name == oDicByFile[YlCommon.RULE_VAR_PROGRAM])
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
			if (aSongs.Count > 1 && oDicByFile[YlCommon.RULE_VAR_CATEGORY] != null)
			{
				List<TSong> aSongsWithCategory = new List<TSong>();
				foreach (TSong aSong in aSongs)
				{
					TCategory aCategory = YlCommon.SelectCategoryById(oMusicInfoDbContext, aSong.CategoryId);
					if (aCategory != null && aCategory.Name == oDicByFile[YlCommon.RULE_VAR_CATEGORY])
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
			if (aSongs.Count > 1 && oDicByFile[YlCommon.RULE_VAR_ARTIST] != null)
			{
				List<TSong> aSongsWithArtist = new List<TSong>();
				foreach (TSong aSong in aSongs)
				{
					String aArtistName;
					String aArtistRuby;
					ConcatPersonNameAndRuby(YlCommon.SelectArtistsBySongId(oMusicInfoDbContext, aSong.Id), out aArtistName, out aArtistRuby);
					if (!String.IsNullOrEmpty(aArtistName) && aArtistName == oDicByFile[YlCommon.RULE_VAR_ARTIST])
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
				if (oDicByFile[YlCommon.RULE_VAR_PROGRAM] != null)
				{
					List<TTieUp> aTieUps = YlCommon.SelectTieUpsByName(oMusicInfoDbContext, oDicByFile[YlCommon.RULE_VAR_PROGRAM]);
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
				aTieUpOfSong = YlCommon.SelectTieUpById(oMusicInfoDbContext, aSelectedSong.TieUpId);
			}

			if (aTieUpOfSong != null)
			{
				TCategory aCategoryOfTieUp = YlCommon.SelectCategoryById(oMusicInfoDbContext, aTieUpOfSong.CategoryId);
				if (aCategoryOfTieUp != null)
				{
					// TCategory 由来項目の設定
					oRecord.Category = aCategoryOfTieUp.Name;
				}

				TMaker aMakerOfTieUp = YlCommon.SelectMakerById(oMusicInfoDbContext, aTieUpOfSong.MakerId);
				if (aMakerOfTieUp != null)
				{
					// TMaker 由来項目の設定
					oRecord.MakerName = aMakerOfTieUp.Name;
					oRecord.MakerRuby = aMakerOfTieUp.Ruby;
				}

				List<TTieUpGroup> aTieUpGroups = YlCommon.SelectTieUpGroupsByTieUpId(oMusicInfoDbContext, aTieUpOfSong.Id);
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
			ConcatPersonNameAndRuby(YlCommon.SelectArtistsBySongId(oMusicInfoDbContext, aSelectedSong.Id), out aName, out aRuby);
			oRecord.ArtistName = aName;
			oRecord.ArtistRuby = aRuby;
			ConcatPersonNameAndRuby(YlCommon.SelectLyristsBySongId(oMusicInfoDbContext, aSelectedSong.Id), out aName, out aRuby);
			oRecord.LyristName = aName;
			oRecord.LyristRuby = aRuby;
			ConcatPersonNameAndRuby(YlCommon.SelectComposersBySongId(oMusicInfoDbContext, aSelectedSong.Id), out aName, out aRuby);
			oRecord.ComposerName = aName;
			oRecord.ComposerRuby = aRuby;
			ConcatPersonNameAndRuby(YlCommon.SelectArrangersBySongId(oMusicInfoDbContext, aSelectedSong.Id), out aName, out aRuby);
			oRecord.ArrangerName = aName;
			oRecord.ArrangerRuby = aRuby;

			// TSong 由来項目の設定
			oRecord.SongName = aSelectedSong.Name;
			oRecord.SongRuby = aSelectedSong.Ruby;
			oRecord.SongOpEd = aSelectedSong.OpEd;
			if (oRecord.SongReleaseDate <= YlCommon.INVALID_MJD && aSelectedSong.ReleaseDate > YlCommon.INVALID_MJD)
			{
				oRecord.SongReleaseDate = aSelectedSong.ReleaseDate;
			}
			if (String.IsNullOrEmpty(oRecord.Category))
			{
				TCategory aCategoryOfSong = YlCommon.SelectCategoryById(oMusicInfoDbContext, aSelectedSong.CategoryId);
				if (aCategoryOfSong != null)
				{
					oRecord.Category = aCategoryOfSong.Name;
				}
			}

		}

		// --------------------------------------------------------------------
		// ゆかりすたーの動作状況とその表示を更新
		// エラーを最初に判定する
		// --------------------------------------------------------------------
		private void SetYukaListerStatusWithInvoke()
		{
			if (mClosingCancellationTokenSource.Token.IsCancellationRequested)
			{
				return;
			}

			if (!IsYukariConfigPathSet())
			{
				// 設定ファイルエラー
				mYukaListerStatus = YukaListerStatus.Error;
				mYukaListerStatusMessage = "ゆかり設定ファイルが正しく指定されていません。";
			}
			else if (!File.Exists(FILE_NAME_APP_CONFIG))
			{
				// アプリケーション構成ファイルエラー
				mYukaListerStatus = YukaListerStatus.Error;
				mYukaListerStatusMessage = "アプリケーション構成ファイルが見つかりません。再インストールして下さい。";
			}
			else
			{
				// エラーがなかったので、一旦待機中を仮定
				mYukaListerStatus = YukaListerStatus.Ready;
				mYukaListerStatusMessage = YlCommon.APP_NAME_J + "は正常に動作しています。";

				// 実行中のものがある場合は実行中にする
				for (Int32 i = 0; i < (Int32)YukaListerStatusRunningMessage.__End__; i++)
				{
					if (mEnabledYukaListerStatusRunningMessages[i])
					{
						mYukaListerStatus = YukaListerStatus.Running;
						mYukaListerStatusMessage = YlCommon.YUKA_LISTER_STATUS_RUNNING_MESSAGES[i];
						break;
					}
				}
			}

			// 動作状況更新
			mYukaListerStatusSubMessage = null;
			if (mYukaListerStatus == YukaListerStatus.Error)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, mYukaListerStatusMessage, true);
			}

			// 表示更新
			Invoke(new Action(() =>
			{
				UpdateYukaListerStatus();
				TimerUpdateStatus.Enabled = (mYukaListerStatus == YukaListerStatus.Running);
			}));
		}

		// --------------------------------------------------------------------
		// 改訂履歴の表示
		// --------------------------------------------------------------------
		private void ShowHistory()
		{
			try
			{
				Process.Start(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + FILE_NAME_HISTORY);
			}
			catch (Exception)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "改訂履歴を表示できませんでした。\n" + FILE_NAME_HISTORY);
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
					+ " WHERE " + TSongAlias.TABLE_NAME_SONG_ALIAS + "." + TSongAlias.FIELD_NAME_SONG_ALIAS_ALIAS + " = @alias";
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
		// プレビューサーバーを停止
		// --------------------------------------------------------------------
		private Task StopPreviewServerAsync()
		{
			return Task.Run(() =>
			{
				try
				{
					// ダミーデータを送信してサーバーの待機を終了させる
					TcpClient aClient = new TcpClient("localhost", mYukaListerSettings.YukariPreviewPort);
					using (NetworkStream aNetworkStream = aClient.GetStream())
					{
						aNetworkStream.ReadTimeout = YlCommon.TCP_TIMEOUT;
						aNetworkStream.WriteTimeout = YlCommon.TCP_TIMEOUT;
						Byte[] aSendBytes = Encoding.UTF8.GetBytes("End");
						aNetworkStream.Write(aSendBytes, 0, aSendBytes.Length);
					}
					aClient.Close();

					mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "プレビューサーバー終了");
				}
				catch (Exception oExcep)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Error, "プレビューサーバー終了時エラー：" + oExcep.Message, true);
				}
			});
		}

		// --------------------------------------------------------------------
		// 削除ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonRemoveTargetFolderAndButtonFolderSettings()
		{
			TargetFolderInfo aTargetFolderInfo = SelectedTargetFolderInfo();

			ButtonRemoveTargetFolder.Enabled = (aTargetFolderInfo != null);
			ButtonFolderSettings.Enabled = (aTargetFolderInfo != null);
		}

		// --------------------------------------------------------------------
		// ファイル一覧ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonTFounds()
		{
			lock (mTargetFolderInfos)
			{
				ButtonTFounds.Enabled = mTargetFolderInfos.Count > 0;
			}
		}

		// --------------------------------------------------------------------
		// ゆかり検索対象フォルダー一覧を更新（必要最小限のみ）
		// Invoke() および lock されていることが必須
		// --------------------------------------------------------------------
		private void UpdateDataGridViewTargetFolders()
		{
			// DGV の行数を調整
			Int32 aTargetCount;
			aTargetCount = mTargetFolderInfos.Count;
			if (DataGridViewTargetFolders.Rows.Count < aTargetCount)
			{
				// 行数不足なので追加
				for (Int32 i = DataGridViewTargetFolders.Rows.Count; i < aTargetCount; i++)
				{
					DataGridViewTargetFolders.Rows.Add();
				}
			}
			else if (DataGridViewTargetFolders.Rows.Count > aTargetCount)
			{
				// 行数オーバーなので削除
				for (Int32 i = DataGridViewTargetFolders.Rows.Count - 1; i >= aTargetCount; i--)
				{
					DataGridViewTargetFolders.Rows.RemoveAt(i);
				}
			}
			Debug.Assert(DataGridViewTargetFolders.Rows.Count == aTargetCount, "UpdateDataGridViewTargetFolders() DGV 数不一致");

			// 各行の表示非表示を反映
			Boolean aPrevIsOpen = true;
			for (Int32 i = 0; i < mTargetFolderInfos.Count; i++)
			{
				DataGridViewRow aRow = DataGridViewTargetFolders.Rows[i];
				TargetFolderInfo aInfo = mTargetFolderInfos[i];
				if (aInfo.IsParent)
				{
					aRow.Visible = true;
					aPrevIsOpen = aInfo.IsOpen;
				}
				else
				{
					aRow.Visible = aPrevIsOpen;
				}
			}

			// 表示更新
			DataGridViewTargetFolders.Invalidate();
		}

		// --------------------------------------------------------------------
		// ゆかり検索対象フォルダー一覧を更新
		// --------------------------------------------------------------------
		private void UpdateDataGridViewTargetFoldersWithInvoke()
		{
			Invoke(new Action(() =>
			{
				lock (mTargetFolderInfos)
				{
					UpdateDataGridViewTargetFolders();
				}
			}));
		}

		// --------------------------------------------------------------------
		// ゆかり検索対象フォルダー一覧の更新が必要な行が表示範囲に含まれている場合は表示を更新する
		// 高速化のため、mDirtyDgvLineMin と mDirtyDgvLineMax はロックしない
		// 本関数とフォルダータスクが同時に mDirtyDgvLineXXX を更新することにより競合が発生し、
		// 正しく DGV が更新されない場合があるが、フォルダータスク完了時には必ず DGV が更新されるため、
		// 一時的な事象として許容する
		// --------------------------------------------------------------------
		private void UpdateDirtyDgvLines()
		{
			// DGV の表示範囲を調べる
			Int32 aLastDisplayedScrollingRowIndex = DataGridViewTargetFolders.FirstDisplayedScrollingRowIndex;
			while (aLastDisplayedScrollingRowIndex + 1 < DataGridViewTargetFolders.RowCount && DataGridViewTargetFolders.Rows[aLastDisplayedScrollingRowIndex + 1].Displayed)
			{
				aLastDisplayedScrollingRowIndex++;
			}

			// 更新必要判定
			Boolean aIsUpdateNeeded = DataGridViewTargetFolders.FirstDisplayedScrollingRowIndex <= mDirtyDgvLineMin && mDirtyDgvLineMin <= aLastDisplayedScrollingRowIndex
					|| DataGridViewTargetFolders.FirstDisplayedScrollingRowIndex <= mDirtyDgvLineMax && mDirtyDgvLineMax <= aLastDisplayedScrollingRowIndex
					|| mDirtyDgvLineMin < DataGridViewTargetFolders.FirstDisplayedScrollingRowIndex && mDirtyDgvLineMax > aLastDisplayedScrollingRowIndex;

			// 更新の必要性に関わらずフラグはクリアする
			ClearDirtyDgvLines();
			mLogWriter.ShowLogMessage(TraceEventType.Verbose, "UpdateDirtyDgvLines() aIsUpdateNeeded: " + aIsUpdateNeeded.ToString());

			if (!aIsUpdateNeeded)
			{
				return;
			}

			// 表示更新
			DataGridViewTargetFolders.Invalidate();
		}

		// --------------------------------------------------------------------
		// ゆかりすたーの動作状況表示を更新
		// 画面上部のステータス欄を更新（リスト化に直接影響する情報）
		// --------------------------------------------------------------------
		private void UpdateYukaListerStatus()
		{
			// アイコン
			LabelIcon.Text = YUKA_LISTER_STATUS_ICONS[(Int32)mYukaListerStatus];
			LabelIcon.ForeColor = YUKA_LISTER_STATUS_COLORS[(Int32)mYukaListerStatus];
			LabelIcon.BackColor = YUKA_LISTER_STATUS_BACK_COLORS[(Int32)mYukaListerStatus];

			// メッセージ
			LabelYukaListerStatus.Text = mYukaListerStatusMessage + mYukaListerStatusSubMessage;
			LabelYukaListerStatus.BackColor = YUKA_LISTER_STATUS_BACK_COLORS[(Int32)mYukaListerStatus];
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void WmDeviceChange(ref Message oMsg)
		{
			try
			{
				switch ((DBT)oMsg.WParam.ToInt32())
				{
					case DBT.DBT_DEVICEARRIVAL:
					case DBT.DBT_DEVICEREMOVECOMPLETE:
						break;
					default:
						return;
				}
				if (oMsg.LParam == IntPtr.Zero)
				{
					return;
				}

				WindowsApi.DEV_BROADCAST_HDR aHdr = (WindowsApi.DEV_BROADCAST_HDR)Marshal.PtrToStructure(oMsg.LParam, typeof(WindowsApi.DEV_BROADCAST_HDR));
				if (aHdr.dbch_devicetype != (Int32)DBT_DEVTYP.DBT_DEVTYP_VOLUME)
				{
					return;
				}

				WindowsApi.DEV_BROADCAST_VOLUME aVolume = (WindowsApi.DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(oMsg.LParam, typeof(WindowsApi.DEV_BROADCAST_VOLUME));
				UInt32 aUnitMask = aVolume.dbcv_unitmask;
				if (aUnitMask == 0)
				{
					return;
				}

				Char aNumShift = (Char)0;
				String aDriveLetter;
				while (aUnitMask != 1)
				{
					aUnitMask >>= 1;
					aNumShift++;
				}
				aDriveLetter = new String((Char)('A' + aNumShift), 1) + ":";

				if ((DBT)oMsg.WParam.ToInt32() == DBT.DBT_DEVICEARRIVAL)
				{
					DeviceArrival(aDriveLetter);
				}
				else
				{
					DeviceRemoveComplete(aDriveLetter);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "デバイス変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void WmLaunchFolderTaskRequested()
		{
			try
			{
				// DoFolderTask() がメッセージ経由で本関数を呼びだした場合、mFolderTaskLock のロックが
				// 外れていない恐れがあるため、一旦スリープして制御を手放す
				Thread.Sleep(Common.GENERAL_SLEEP_TIME);

				// フォルダータスクを開始（async を待機しない）
				Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(DoFolderTaskByWorker, mFolderTaskLock, null);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダータスクリクエスト時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void WmLaunchListTaskRequested()
		{
			try
			{
				// リストタスクを開始（async を待機しない）
				Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(OutputYukariListByWorker, mListTaskLock, null);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リストタスクリクエスト時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void WmShNotify(ref Message oMsg)
		{
			try
			{
				switch ((SHCNE)oMsg.LParam)
				{
					case SHCNE.SHCNE_MEDIAINSERTED:
					case SHCNE.SHCNE_MEDIAREMOVED:
						break;
					default:
						return;
				}

				WindowsApi.SHNOTIFYSTRUCT aShNotifyStruct = (WindowsApi.SHNOTIFYSTRUCT)Marshal.PtrToStructure(oMsg.WParam, typeof(WindowsApi.SHNOTIFYSTRUCT));
				StringBuilder aDriveRoot = new StringBuilder();
				WindowsApi.SHGetPathFromIDList((IntPtr)aShNotifyStruct.dwItem1, aDriveRoot);
				String aDriveLetter = aDriveRoot.ToString().Substring(0, 2);
				if ((SHCNE)oMsg.LParam == SHCNE.SHCNE_MEDIAINSERTED)
				{
					DeviceArrival(aDriveLetter);
				}
				else
				{
					DeviceRemoveComplete(aDriveLetter);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "メディア装着状態変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// IDE 生成イベントハンドラー（メインウィンドウ）
		// ====================================================================

		private void FormYukaLister_Load(object sender, EventArgs e)
		{
			try
			{
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "起動時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private async void FormYukaLister_Shown(object sender, EventArgs e)
		{
			try
			{
				// 更新起動時とパス変更時の記録
				// 新規起動時は、両フラグが立つのでダブらないように注意
				Boolean aVerChanged = mYukaListerSettings.PrevLaunchVer != YlCommon.APP_VER;
				if (aVerChanged)
				{
					// ユーザーにメッセージ表示する前にログしておく
					if (String.IsNullOrEmpty(mYukaListerSettings.PrevLaunchVer))
					{
						mLogWriter.LogMessage(TraceEventType.Information, "新規起動：" + YlCommon.APP_VER);
					}
					else
					{
						mLogWriter.LogMessage(TraceEventType.Information, "更新起動：" + mYukaListerSettings.PrevLaunchVer + "→" + YlCommon.APP_VER);
					}
				}
				Boolean aPathChanged = (String.Compare(mYukaListerSettings.PrevLaunchPath, Application.ExecutablePath, true) != 0);
				if (aPathChanged && !String.IsNullOrEmpty(mYukaListerSettings.PrevLaunchPath))
				{
					mLogWriter.LogMessage(TraceEventType.Information, "パス変更起動：" + mYukaListerSettings.PrevLaunchPath + "→" + Application.ExecutablePath);
				}

				// 更新起動時とパス変更時の処理
				if (aVerChanged || aPathChanged)
				{
					YlCommon.LogEnvironmentInfo();
				}
				if (aVerChanged)
				{
					NewVersionLaunched();
					AddMusicInfoDbCategoryDefaultRecordsIfNeeded();
				}

				// 必要に応じてちょちょいと自動更新を起動
				if (mYukaListerSettings.IsCheckRssNeeded())
				{
					if (YlCommon.LaunchUpdater(true, false, IntPtr.Zero, false, false))
					{
						mYukaListerSettings.RssCheckDate = DateTime.Now.Date;
						mYukaListerSettings.Save();
					}
				}

				Common.CloseIfNet45IsnotInstalled(this, YlCommon.APP_NAME_J, mLogWriter);

				// マルチカードリーダーイベント準備
				WindowsApi.SHChangeNotifyEntry aShChangeNotifyEntry = new WindowsApi.SHChangeNotifyEntry();
				aShChangeNotifyEntry.pidl = IntPtr.Zero;
				aShChangeNotifyEntry.fRecursive = true;
				WindowsApi.SHChangeNotifyRegister(Handle, SHCNRF.SHCNRF_ShellLevel, SHCNE.SHCNE_MEDIAINSERTED | SHCNE.SHCNE_MEDIAREMOVED,
						WindowsApi.WM_SHNOTIFY, 1, ref aShChangeNotifyEntry);

				SetYukaListerStatusWithInvoke();
				UpdateButtonRemoveTargetFolderAndButtonFolderSettings();
				UpdateButtonTFounds();

				// 前回のリストが残っていてリクエストできない曲がリスト化されるのを防ぐために、前回のリストを削除
				await YlCommon.LaunchTaskAsync<Object>(OutputYukariListByWorker, mListTaskLock, null);
#if DEBUGz
				Thread.Sleep(20 * 1000);
#endif

				// 終了時に追加されていたフォルダーを追加
				String[] aDrives = Directory.GetLogicalDrives();
				foreach (String aDrive in aDrives)
				{
					DeviceArrival(aDrive.Substring(0, 2));
				}
#if false
				foreach (String aPath in mYukaListerSettings.TargetFoldersExLen)
				{
					await YlCommon.LaunchTaskAsync(AddTargetFolderByWorker, mGeneralTaskLock, aPath);
				}
#endif

				// 楽曲情報データベースの同期
				ToolStripStatusLabelBgStatus.Text = null;
				RunSyncClientIfNeeded();

				// ゆかり用プレビュー
				RunPreviewServerIfNeeded();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "メインウィンドウ表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonAddTargetFolder_Click(object sender, EventArgs e)
		{
			try
			{
				if (FolderBrowserDialogFolder.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				// async を待機しない
				Task aSuppressWarning = YlCommon.LaunchTaskAsync(AddTargetFolderByWorker, mGeneralTaskLock, FolderBrowserDialogFolder.SelectedPath);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "追加ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormYukaLister_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				// 終了時タスクキャンセル
				mClosingCancellationTokenSource.Cancel();
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "終了処理中...");

				// プレビューサーバー終了
				if (mYukaListerSettings.ProvideYukariPreview)
				{
					// async を待機しない
					Task aSuppressWarning = StopPreviewServerAsync();
				}

				// 終了時の状態
				mYukaListerSettings.PrevLaunchPath = Application.ExecutablePath;
				mYukaListerSettings.PrevLaunchVer = YlCommon.APP_VER;
				if (WindowState == FormWindowState.Normal)
				{
					mYukaListerSettings.Bounds = Bounds;
				}
				else
				{
					mYukaListerSettings.Bounds = RestoreBounds;
				}
				mYukaListerSettings.WindowMaximized = (WindowState == FormWindowState.Maximized);
#if false
				mYukaListerSettings.TargetFoldersExLen.Clear();
				lock (mTargetFolderInfos)
				{
					for (Int32 i = 0; i < mTargetFolderInfos.Count; i++)
					{
						if (mTargetFolderInfos[i].IsParent && mTargetFolderInfos[i].FolderTask != FolderTask.Remove)
						{
							mYukaListerSettings.TargetFoldersExLen.Add(YlCommon.ShortenPath(mTargetFolderInfos[i].Path));
						}
					}
				}
#endif
				mYukaListerSettings.Save();

				// テンポラリーフォルダー削除
				try
				{
					Directory.Delete(YlCommon.TempPath(), true);
				}
				catch
				{
				}

				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "終了しました：" + YlCommon.APP_NAME_J + " "
							+ YlCommon.APP_VER + " --------------------");

			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "終了時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormYukaLister_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				e.Effect = DragDropEffects.None;

				// ファイル類のときのみ、受け付けるかどうかの判定をする
				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					String[] aDropFiles = (String[])e.Data.GetData(DataFormats.FileDrop, false);
					if (aDropFiles == null)
					{
						return;
					}
					foreach (String aDropFile in aDropFiles)
					{
						if (Directory.Exists(YlCommon.ExtendPath(aDropFile)))
						{
							// フォルダーが含まれていたら受け付ける
							e.Effect = DragDropEffects.Copy;
							break;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ドラッグ受け入れ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private async void FormYukaLister_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				Activate();

				String[] aDropFiles = (String[])e.Data.GetData(DataFormats.FileDrop, false);
				foreach (String aDropFile in aDropFiles)
				{
					if (!Directory.Exists(YlCommon.ExtendPath(aDropFile)))
					{
						// フォルダーでない場合は何もしない
						continue;
					}

					// 複数フォルダーが指定されている場合に同時実行できないため、async の終了を待ってから次のフォルダーを処理する
					await YlCommon.LaunchTaskAsync(AddTargetFolderByWorker, mGeneralTaskLock, aDropFile);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ドロップ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridViewTargetFolders_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (e.RowIndex < 0)
				{
					// 選択解除
					DataGridViewTargetFolders.ClearSelection();
					return;
				}
				if (e.ColumnIndex != (Int32)FolderColumns.Acc)
				{
					return;
				}

				lock (mTargetFolderInfos)
				{
					if (!mTargetFolderInfos[e.RowIndex].IsParent)
					{
						return;
					}

					// アコーディオン開閉
					SetCursor(Cursors.WaitCursor);
					mTargetFolderInfos[e.RowIndex].IsOpen = !mTargetFolderInfos[e.RowIndex].IsOpen;
					for (Int32 i = e.RowIndex + 1; i < e.RowIndex + mTargetFolderInfos[e.RowIndex].NumTotalFolders; i++)
					{
						DataGridViewTargetFolders.Rows[i].Visible = mTargetFolderInfos[e.RowIndex].IsOpen;
					}
					SetCursor(Cursors.Default);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "セルクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridViewTargetFolders_SelectionChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateButtonRemoveTargetFolderAndButtonFolderSettings();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "選択変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonRemoveTargetFolder_Click(object sender, EventArgs e)
		{
			try
			{
				TargetFolderInfo aTargetFolderInfo = SelectedTargetFolderInfo();
				if (aTargetFolderInfo == null)
				{
					return;
				}

				if (MessageBox.Show(YlCommon.ShortenPath(aTargetFolderInfo.ParentPath) + "\nおよびサブフォルダーをゆかり検索対象から削除しますか？",
						"確認", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
				{
					return;
				}

				// async を待機しない
				Task aSuppressWarning = YlCommon.LaunchTaskAsync(RemoveTargetFolderByWorker, mGeneralTaskLock, aTargetFolderInfo.ParentPath);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "削除ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonYukaListerSettings_Click(object sender, EventArgs e)
		{
			try
			{
				String aYukariConfigPathBak = mYukaListerSettings.YukariConfigPath();
				Boolean aSyncMusicInfoDbBak = mYukaListerSettings.SyncMusicInfoDb;
				String aSyncServerBak = mYukaListerSettings.SyncServer;
				String aSyncAccountBak = mYukaListerSettings.SyncAccount;
				String aSyncPasswordBak = mYukaListerSettings.SyncPassword;
				DateTime aMusicInfoDbTimeBak = new FileInfo(YlCommon.MusicInfoDbPath()).LastWriteTime;
				Boolean aRegetSyncDataNeeded;

				using (FormYukaListerSettings aFormYukaListerSettings = new FormYukaListerSettings(mYukaListerSettings, mLogWriter))
				{
					aFormYukaListerSettings.ShowDialog(this);
					aRegetSyncDataNeeded = aFormYukaListerSettings.RegetSyncDataNeeded;
				}

				// ゆかり設定ファイルのフルパスが変更された場合は処理を行う
				if (mYukaListerSettings.YukariConfigPath() != aYukariConfigPathBak)
				{
					SetYukaListerStatusWithInvoke();

					if (mYukaListerStatus == YukaListerStatus.Error)
					{
						MakeAllFolderTasksQueued();
					}
					else
					{
						// 新しい設定でゆかり用データベースを準備する
						CreateYukariDb();

						// フォルダータスクやり直し
						MakeAllFolderTasksQueued();
						Invoke(new Action(() =>
						{
							WindowsApi.PostMessage(Handle, YlCommon.WM_LAUNCH_FOLDER_TASK_REQUESTED, (IntPtr)0, (IntPtr)0);
						}));
					}
				}

				if (aRegetSyncDataNeeded)
				{
					// 再取得が指示された場合は再取得
					RunSyncClientIfNeeded(true);
				}
				else
				{
					// 同期設定が変更された場合・インポートで楽曲情報データベースが更新された場合は同期を行う
					DateTime aMusicInfoDbTime = new FileInfo(YlCommon.MusicInfoDbPath()).LastWriteTime;
					if (mYukaListerSettings.SyncMusicInfoDb != aSyncMusicInfoDbBak
							|| mYukaListerSettings.SyncServer != aSyncServerBak
							|| mYukaListerSettings.SyncAccount != aSyncAccountBak
							|| mYukaListerSettings.SyncPassword != aSyncPasswordBak
							|| aMusicInfoDbTime != aMusicInfoDbTimeBak)
					{
						RunSyncClientIfNeeded();
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "環境設定ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonHelp_Click(object sender, EventArgs e)
		{
			try
			{
				ContextMenuStripHelp.Show(Cursor.Position);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ヘルプボタン（メインウィンドウ）クリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ヘルプHToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				YlCommon.ShowHelp();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ヘルプメニュークリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void 改訂履歴UToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				ShowHistory();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "改訂履歴メニュークリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void バージョン情報AToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormAbout aFormAbout = new FormAbout(mLogWriter))
				{
					aFormAbout.ShowDialog(this);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "バージョン情報メニュークリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TimerUpdateStatus_Tick(object sender, EventArgs e)
		{
			try
			{
				UpdateYukaListerStatus();
				if (mEnabledYukaListerStatusRunningMessages[(Int32)YukaListerStatusRunningMessage.DoFolderTask])
				{
					UpdateDirtyDgvLines();
				}
			}
			catch (Exception oExcep)
			{
				TimerUpdateStatus.Enabled = false;
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイマー時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridViewTargetFolders_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				//Debug.WriteLine("DataGridViewTargetFolders_CellValueNeeded() " + e.RowIndex + " 行 " + e.ColumnIndex + " 列");
				if (e.RowIndex < 0)
				{
					return;
				}

				lock (mTargetFolderInfos)
				{
					if (e.RowIndex >= mTargetFolderInfos.Count)
					{
						return;
					}
					TargetFolderInfo aInfo = mTargetFolderInfos[e.RowIndex];

					switch ((FolderColumns)e.ColumnIndex)
					{
						case FolderColumns.Acc:
							e.Value = aInfo.IsParent ? "親" : null;
							break;
						case FolderColumns.Status:
							DataGridViewRow aRow = DataGridViewTargetFolders.Rows[e.RowIndex];
							switch (aInfo.FolderTaskStatus)
							{
								case FolderTaskStatus.Done:
									if (aInfo.IsParent && aInfo.IsChildRunning)
									{
										e.Value = "サブフォルダー待ち";
										aRow.Cells[e.ColumnIndex].Style = mCellStyles[(Int32)YukaListerStatus.Running];
									}
									else
									{
										if (aInfo.FolderTask == FolderTask.Remove)
										{
											e.Value = "削除済";
										}
										else
										{
											if (aInfo.FolderExcludeSettingsStatus == FolderExcludeSettingsStatus.Unchecked)
											{
												FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings(aInfo.Path);
												aInfo.FolderExcludeSettingsStatus = aFolderSettingsInDisk.IsExclude ? FolderExcludeSettingsStatus.True : FolderExcludeSettingsStatus.False;
											}
											if (aInfo.FolderExcludeSettingsStatus == FolderExcludeSettingsStatus.True)
											{
												e.Value = "対象外";
											}
											else
											{
												e.Value = "済";
											}
										}
										aRow.Cells[e.ColumnIndex].Style = mCellStyles[(Int32)YukaListerStatus.Ready];
									}
									break;
								case FolderTaskStatus.Error:
									e.Value = "エラー";
									aRow.Cells[e.ColumnIndex].Style = mCellStyles[(Int32)YukaListerStatus.Error];
									break;
								default:
									if (mYukaListerStatus == YukaListerStatus.Error)
									{
										e.Value = "エラー解決待ち";
										aRow.Cells[e.ColumnIndex].Style = mCellStyles[(Int32)YukaListerStatus.Error];
									}
									else
									{
										String aLabel = null;
										switch (aInfo.FolderTask)
										{
											case FolderTask.Add:
												aLabel = "追加";
												break;
											case FolderTask.Remove:
												aLabel = "削除";
												break;
											case FolderTask.Update:
												aLabel = "更新";
												break;
											default:
												Debug.Assert(false, "DataGridViewTargetFolders_CellValueNeeded() bad aInfo.FolderTask");
												break;
										}
										if (aInfo.FolderTaskStatus == FolderTaskStatus.Queued)
										{
											e.Value = aLabel + "予定";
											aRow.Cells[e.ColumnIndex].Style = DataGridViewTargetFolders.DefaultCellStyle;
										}
										else
										{
											e.Value = aLabel + "中";
											aRow.Cells[e.ColumnIndex].Style = mCellStyles[(Int32)YukaListerStatus.Running];
										}
									}
									break;
							}
							break;
						case FolderColumns.Folder:
							e.Value = YlCommon.ShortenPath(aInfo.Path);
							break;
						case FolderColumns.SettingsExist:
							if (aInfo.FolderSettingsStatus == FolderSettingsStatus.Unchecked)
							{
								aInfo.FolderSettingsStatus = YlCommon.DetectFolderSettingsStatus(aInfo.Path);
							}
							e.Value = YlCommon.FOLDER_SETTINGS_STATUS_TEXTS[(Int32)aInfo.FolderSettingsStatus];
							break;
						default:
							Debug.Assert(false, "DataGridViewTargetFolders_CellValueNeeded() bad e.ColumnIndex");
							break;
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "DGV セル値必要時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonFolderSettings_Click(object sender, EventArgs e)
		{
			try
			{
				TargetFolderInfo aTargetFolderInfo = SelectedTargetFolderInfo();
				if (aTargetFolderInfo == null)
				{
					return;
				}

				DateTime aMusicInfoDbTimeBak = new FileInfo(YlCommon.MusicInfoDbPath()).LastWriteTime;

				using (FormFolderSettings aFormFolderSettings = new FormFolderSettings(aTargetFolderInfo.Path, mYukaListerSettings, mLogWriter))
				{
					aFormFolderSettings.ShowDialog(this);
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
						aIndex++;
					}
				}
				DataGridViewTargetFolders.Invalidate();

				// 楽曲情報データベースが更新された場合は同期を行う
				DateTime aMusicInfoDbTime = new FileInfo(YlCommon.MusicInfoDbPath()).LastWriteTime;
				if (aMusicInfoDbTime != aMusicInfoDbTimeBak)
				{
					RunSyncClientIfNeeded();
				}

			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonTFounds_Click(object sender, EventArgs e)
		{
			try
			{
				DateTime aMusicInfoDbTimeBak = new FileInfo(YlCommon.MusicInfoDbPath()).LastWriteTime;

				using (FormViewTFounds aFormViewTFounds = new FormViewTFounds(mYukaListerSettings, mLogWriter))
				{
					aFormViewTFounds.ShowDialog(this);
				}

				// 楽曲情報データベースが更新された場合は同期を行う
				DateTime aMusicInfoDbTime = new FileInfo(YlCommon.MusicInfoDbPath()).LastWriteTime;
				if (aMusicInfoDbTime != aMusicInfoDbTimeBak)
				{
					RunSyncClientIfNeeded();
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル一覧ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormYukaLister ___END___

}
// namespace YukaLister ___END___
