// ============================================================================
// 
// ゆかり検索対象フォルダーの情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace YukaLister.Models.SharedMisc
{
	public class TargetFolderInfo
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 親フォルダーかどうか
		public Boolean IsParent { get; set; }

		// 親フォルダーの場合のみ有効：サブフォルダーを表示しているかどうか：表示用兼用
		private Boolean mIsOpen;
		public Boolean? IsOpen
		{
			get
			{
				if (IsParent && NumTotalFolders > 1)
				{
					return mIsOpen;
				}
				return null;
			}
			set
			{
				if (IsParent && NumTotalFolders > 1 && value != mIsOpen)
				{
					mIsOpen = (Boolean)value;
					IsOpenChanged(this);
				}
			}
		}

		// 親フォルダーの場合のみ有効：サブフォルダーが動作しているかどうか
		public Boolean IsChildRunning { get; set; }

		// 親フォルダーの場合のみ有効：親フォルダー＋サブフォルダーの数
		public Int32 NumTotalFolders { get; set; }

		// フォルダーパス（ExLen 形式）
		public String Path { get; set; }

		// 親フォルダーのパス（ソート用）（ExLen 形式）（親の場合は Path と同じ値にすること）
		public String ParentPath { get; set; }

		// 操作
		public FolderTask FolderTask { get; set; }

		// 動作状況
		public FolderTaskStatus FolderTaskStatus { get; set; }

		// フォルダー除外設定の状態
		public FolderExcludeSettingsStatus FolderExcludeSettingsStatus
		{
			get
			{
				if (mFolderExcludeSettingsStatus == FolderExcludeSettingsStatus.Unchecked)
				{
					mFolderExcludeSettingsStatus = YlCommon.DetectFolderExcludeSettingsStatus(Path);
				}
				return mFolderExcludeSettingsStatus;
			}
			set
			{
				mFolderExcludeSettingsStatus = value;
			}
		}

		// フォルダー設定の状態
		public FolderSettingsStatus FolderSettingsStatus
		{
			get
			{
				if (mFolderSettingsStatus == FolderSettingsStatus.Unchecked)
				{
					mFolderSettingsStatus = YlCommon.DetectFolderSettingsStatus2Ex(Path);
				}
				return mFolderSettingsStatus;
			}
			set
			{
				mFolderSettingsStatus = value;
			}
		}

		// UI に表示するかどうか
		public Boolean Visible { get; set; }

		// 表示用：状態
		public String FolderTaskStatusLabel
		{
			get
			{
				String aLabel;
				FolderTaskStatus aStatusForLabelColor;
				GetFolderTaskStatus(out aLabel, out aStatusForLabelColor);
				return aLabel;
			}
			set
			{
				Debug.Assert(false, "TargetFolderInfo.FolderTaskStatusLabel set: forbidden");
			}
		}

		// 表示用：パス
		public String PathLabel
		{
			get
			{
				return Environment.ShortenPath(Path);
			}
			set
			{
				Debug.Assert(false, "TargetFolderInfo.PathLabel set: forbidden");
			}
		}

		// 表示用：設定有無
		public String FolderSettingsStatusLabel
		{
			get
			{
				return YlConstants.FOLDER_SETTINGS_STATUS_TEXTS[(Int32)FolderSettingsStatus];
			}
			set
			{
				Debug.Assert(false, "TargetFolderInfo.FolderSettingsStatusLabel set: forbidden");
			}
		}

		// 表示用：色分け
		public FolderTaskStatus StatusForLabelColor
		{
			get
			{
				String aLabel;
				FolderTaskStatus aStatusForLabelColor;
				GetFolderTaskStatus(out aLabel, out aStatusForLabelColor);
				return aStatusForLabelColor;
			}
			set
			{
				Debug.Assert(false, "TargetFolderInfo.StatusForLabelColor set: forbidden");
			}
		}

		// 環境設定類
		public static EnvironmentModel Environment { get; set; }

		// ゆかり用リストデータベース構築状況取得
		public static YukaListerStatusDelegate YukariDbYukaListerStatus { get; set; }

		// IsOpen 変更時イベントハンドラー
		public static TargetFolderInfoIsOpenChangedDelegate IsOpenChanged { get; set; }

		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public TargetFolderInfo(String oParentPathExLen, String oPathExLen)
		{
			IsParent = false;
			IsOpen = false;
			IsChildRunning = false;
			NumTotalFolders = 0;
			Path = oPathExLen;
			ParentPath = oParentPathExLen;
			FolderTask = FolderTask.AddFileName;
			FolderTaskStatus = FolderTaskStatus.Queued;
			FolderExcludeSettingsStatus = FolderExcludeSettingsStatus.Unchecked;
			FolderSettingsStatus = FolderSettingsStatus.Unchecked;
			Visible = false;
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ソート用比較関数
		// 例えば @"C:\A" 配下と @"C:\A 2" を正しく並べ替えるために ParentPath が必要
		// --------------------------------------------------------------------
		public static Int32 Compare(TargetFolderInfo oLhs, TargetFolderInfo oRhs)
		{
			if (oLhs.ParentPath != oRhs.ParentPath)
			{
				return String.Compare(oLhs.ParentPath, oRhs.ParentPath);
			}
			return String.Compare(oLhs.Path, oRhs.Path);
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// プロパティー FolderSettingsStatus の実体
		private FolderSettingsStatus mFolderSettingsStatus;

		// プロパティー FolderExcludeSettingsStatus の実体
		private FolderExcludeSettingsStatus mFolderExcludeSettingsStatus;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 状態
		// --------------------------------------------------------------------
		private void GetFolderTaskStatus(out String oLabel, out FolderTaskStatus oStatusForLabelColor)
		{
			if (YukariDbYukaListerStatus() == YukaListerStatus.Error)
			{
				oLabel = "エラー解決待ち";
				oStatusForLabelColor = FolderTaskStatus.Error;
				return;
			}

			switch (FolderTaskStatus)
			{
				case FolderTaskStatus.Queued:
					switch (FolderTask)
					{
						case FolderTask.AddFileName:
							oLabel = "追加予定";
							break;
						case FolderTask.AddInfo:
							oLabel = "ファイル名検索可";
							break;
						case FolderTask.Remove:
							oLabel = "削除予定";
							break;
						case FolderTask.Update:
							oLabel = "更新予定";
							break;
						default:
							Debug.Assert(false, "GetFolderTaskStatus() bad FolderTask in FolderTaskStatus.Queued");
							oLabel = null;
							break;
					}
					oStatusForLabelColor = FolderTaskStatus.Queued;
					break;
				case FolderTaskStatus.Running:
					switch (FolderTask)
					{
						case FolderTask.AddFileName:
							oLabel = "ファイル名確認中";
							break;
						case FolderTask.AddInfo:
							oLabel = "ファイル名検索可＋属性確認中";
							break;
						case FolderTask.FindSubFolders:
							oLabel = "サブフォルダー検索中";
							break;
						case FolderTask.Remove:
							oLabel = "削除中";
							break;
						case FolderTask.Update:
							oLabel = "更新中";
							break;
						default:
							Debug.Assert(false, "GetFolderTaskStatus() bad FolderTask in FolderTaskStatus.Running");
							oLabel = null;
							break;
					}
					oStatusForLabelColor = FolderTaskStatus.Running;
					break;
				case FolderTaskStatus.Error:
					oLabel = "エラー";
					oStatusForLabelColor = FolderTaskStatus.Error;
					break;
				case FolderTaskStatus.DoneInMemory:
					if (IsParent && IsChildRunning)
					{
						oLabel = "サブフォルダー待ち";
						oStatusForLabelColor = FolderTaskStatus.Running;
					}
					else
					{
						switch (FolderTask)
						{
							case FolderTask.AddFileName:
								oLabel = "ファイル名確認済";
								break;
							case FolderTask.AddInfo:
								oLabel = "ファイル名検索可＋属性確認済";
								break;
							case FolderTask.Remove:
								oLabel = "削除準備完了";
								break;
							case FolderTask.Update:
								oLabel = "更新準備完了";
								break;
							default:
								Debug.Assert(false, "GetFolderTaskStatus() bad FolderTask in FolderTaskStatus.DoneInMemory");
								oLabel = null;
								break;
						}
						oStatusForLabelColor = FolderTaskStatus.Queued;
					}
					break;
				case FolderTaskStatus.DoneInDisk:
					switch (FolderTask)
					{
						case FolderTask.AddFileName:
							Debug.Assert(false, "GetFolderTaskStatus() bad FolderTask in FolderTaskStatus.DoneInDisk - FolderTask.AddFileName");
							oLabel = null;
							break;
						case FolderTask.AddInfo:
							oLabel = "追加完了";
							break;
						case FolderTask.Remove:
							oLabel = "削除完了";
							break;
						case FolderTask.Update:
							oLabel = "更新完了";
							break;
						default:
							Debug.Assert(false, "GetFolderTaskStatus() bad oInfo.FolderTask in FolderTaskStatus.DoneInDisk");
							oLabel = null;
							break;
					}
					oStatusForLabelColor = FolderTaskStatus.DoneInDisk;
					break;
				default:
					Debug.Assert(false, "GetFolderTaskStatus() bad FolderTaskStatus");
					oLabel = null;
					oStatusForLabelColor = FolderTaskStatus.Error;
					break;
			}

			if (FolderExcludeSettingsStatus == FolderExcludeSettingsStatus.True)
			{
				oLabel = "対象外";
				oStatusForLabelColor = FolderTaskStatus.Queued;
			}
		}

	}
	// public class TargetFolderInfo ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
