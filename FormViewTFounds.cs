// ============================================================================
// 
// TFound を一覧するウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YukaLister.Shared;

namespace YukaLister
{
	public partial class FormViewTFounds : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormViewTFounds(YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mFormFindKeyword = null;
			mYukaListerSettings = oYukaListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ショートカットキーハンドラ
		// --------------------------------------------------------------------
		protected override Boolean ProcessCmdKey(ref Message oMsg, Keys oKeyData)
		{
			switch (oKeyData)
			{
				case Keys.Control | Keys.F:
					ButtonFind.PerformClick();
					return true;
			}
			return base.ProcessCmdKey(ref oMsg, oKeyData);
		}

		// --------------------------------------------------------------------
		// メッセージハンドラ
		// --------------------------------------------------------------------
		protected override void WndProc(ref Message oMsg)
		{
			switch ((UInt32)oMsg.Msg)
			{
				case YlCommon.WM_FIND_KEYWORD_NEXT_REQUESTED:
					WmFindKeywordNextRequested();
					break;
				case YlCommon.WM_FIND_KEYWORD_PREV_REQUESTED:
					WmFindKeywordPrevRequested();
					break;
			}
			base.WndProc(ref oMsg);
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 出力項目
		List<OutputItems> mRuntimeOutputItems;

		// ファイル群
		List<TFound> mTFounds;

		// ソート中の項目
		OutputItems mSortedItem = OutputItems.__End__;

		// セルスタイル
		DataGridViewCellStyle mNormalCellStyle;
		DataGridViewCellStyle mEmptyCellStyle;

		// 検索ウィンドウ
		FormFindKeyword mFormFindKeyword;

		// 環境設定
		private YukaListerSettings mYukaListerSettings;

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// セルに表示すべき値
		// oRowIndex, oColumnIndex の範囲チェックはしない（呼びだし元でチェック済みである必要がある）
		// --------------------------------------------------------------------
		private String CellValue(Int32 oRowIndex, Int32 oColumnIndex)
		{
			TFound aTFound = mTFounds[oRowIndex];

			switch (mRuntimeOutputItems[oColumnIndex])
			{
				case OutputItems.Path:
					return aTFound.Path;
				case OutputItems.FileName:
					return Path.GetFileName(aTFound.Path);
				case OutputItems.Head:
					return aTFound.Head;
				case OutputItems.Worker:
					return aTFound.Worker;
				case OutputItems.Track:
					return aTFound.Track;
				case OutputItems.SmartTrack:
					return (aTFound.SmartTrackOnVocal ? YlCommon.SMART_TRACK_VALID_MARK : YlCommon.SMART_TRACK_INVALID_MARK) + "/"
							+ (aTFound.SmartTrackOffVocal ? YlCommon.SMART_TRACK_VALID_MARK : YlCommon.SMART_TRACK_INVALID_MARK);
				case OutputItems.Comment:
					return aTFound.Comment;
				case OutputItems.LastWriteTime:
					return JulianDay.ModifiedJulianDateToDateTime(aTFound.LastWriteTime).ToString(YlCommon.DATE_FORMAT + " " + YlCommon.TIME_FORMAT);
				case OutputItems.FileSize:
					return (aTFound.FileSize / (1024 * 1024)).ToString("#,0") + " MB";
				case OutputItems.SongName:
					return aTFound.SongName;
				case OutputItems.SongRuby:
					return aTFound.SongRuby;
				case OutputItems.SongOpEd:
					return aTFound.SongOpEd;
				case OutputItems.SongReleaseDate:
					if (aTFound.SongReleaseDate <= YlCommon.INVALID_MJD)
					{
						return null;
					}
					else
					{
						return JulianDay.ModifiedJulianDateToDateTime(aTFound.SongReleaseDate).ToString(YlCommon.DATE_FORMAT);
					}
				case OutputItems.ArtistName:
					return aTFound.ArtistName;
				case OutputItems.ArtistRuby:
					return aTFound.ArtistRuby;
				case OutputItems.LyristName:
					return aTFound.LyristName;
				case OutputItems.LyristRuby:
					return aTFound.LyristRuby;
				case OutputItems.ComposerName:
					return aTFound.ComposerName;
				case OutputItems.ComposerRuby:
					return aTFound.ComposerRuby;
				case OutputItems.ArrangerName:
					return aTFound.ArrangerName;
				case OutputItems.ArrangerRuby:
					return aTFound.ArrangerRuby;
				case OutputItems.TieUpName:
					return aTFound.TieUpName;
				case OutputItems.TieUpRuby:
					return aTFound.TieUpRuby;
				case OutputItems.TieUpAgeLimit:
					return aTFound.TieUpAgeLimit.ToString();
				case OutputItems.Category:
					return aTFound.Category;
				case OutputItems.TieUpGroupName:
					return aTFound.TieUpGroupName;
				case OutputItems.TieUpGroupRuby:
					return aTFound.TieUpGroupRuby;
				case OutputItems.MakerName:
					return aTFound.MakerName;
				case OutputItems.MakerRuby:
					return aTFound.MakerRuby;
				default:
					Debug.Assert(false, "CellValue() bad mRuntimeOutputItems[oColumnIndex]");
					return null;
			}
		}

		// --------------------------------------------------------------------
		// 必要に応じてキーワード検索ウィンドウを破棄
		// --------------------------------------------------------------------
		private void DisposeFormFindKeywordIfNeeded()
		{
			if (mFormFindKeyword != null)
			{
				mFormFindKeyword.Dispose();
			}
		}

		// --------------------------------------------------------------------
		// 未登録または登録済みの項目を検索して選択
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void FindEmptyOrNonEmptyCell(Boolean oFindEmpty)
		{
			Int32 aRowIndex = SelectedRowIndex();
			Int32 aColumnIndex = SelectedColumnIndex();
			if (aRowIndex < 0 || aRowIndex >= mTFounds.Count
					|| aColumnIndex < 0 || aColumnIndex >= mRuntimeOutputItems.Count)
			{
				throw new Exception("セルを選択して下さい。");
			}

			for (Int32 i = aRowIndex + 1; i < mTFounds.Count; i++)
			{
				if (String.IsNullOrEmpty(CellValue(i, aColumnIndex)) == oFindEmpty)
				{
					// 発見
					DataGridViewList.Rows[i].Cells[aColumnIndex].Selected = true;
					YlCommon.ScrollDataGridViewIfNeeded(DataGridViewList, i);
					return;
				}
			}

			throw new Exception("選択されたセルより下には、" + YlCommon.OUTPUT_ITEM_NAMES[(Int32)mRuntimeOutputItems[aColumnIndex]] + "が"
					+ (oFindEmpty ? "空欄" : "入力済み") + "のセルはありません。");
		}

		// --------------------------------------------------------------------
		// キーワード検索ウィンドウの情報を元に検索
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void FindKeyword(Boolean oIsNext)
		{
			if (String.IsNullOrEmpty(mFormFindKeyword.Keyword))
			{
				throw new Exception("キーワードが指定されていません。");
			}

			Int32 aBeginRowIndex = SelectedRowIndex();
			Int32 aDelta;
			if (oIsNext)
			{
				if (aBeginRowIndex < 0)
				{
					aBeginRowIndex = 0;
				}
				aDelta = 1;
			}
			else
			{
				if (aBeginRowIndex < 0)
				{
					aBeginRowIndex = mTFounds.Count - 1;
				}
				aDelta = -1;
			}

			Int32 aSelectedColumnIndex = SelectedColumnIndex();

			for (Int32 i = aBeginRowIndex; oIsNext ? i < mTFounds.Count : i >= 0; i += aDelta)
			{
				Int32 aBeginColumnIndex;
				if (i == aBeginRowIndex)
				{
					aBeginColumnIndex = aSelectedColumnIndex + aDelta;
				}
				else
				{
					if (oIsNext)
					{
						aBeginColumnIndex = 0;
					}
					else
					{
						aBeginColumnIndex = mRuntimeOutputItems.Count - 1;
					}
				}

				for (Int32 j = aBeginColumnIndex; oIsNext ? j < mRuntimeOutputItems.Count : j >= 0; j += aDelta)
				{
					if (mFormFindKeyword.WholeMatch)
					{
						if (String.Compare(CellValue(i, j), mFormFindKeyword.Keyword, !mFormFindKeyword.CaseSensitive) == 0)
						{
							// 発見
							DataGridViewList.Rows[i].Cells[j].Selected = true;
							YlCommon.ScrollDataGridViewIfNeeded(DataGridViewList, i);
							return;
						}
					}
					else
					{
						if (!String.IsNullOrEmpty(CellValue(i, j))
								&& CellValue(i, j).IndexOf(mFormFindKeyword.Keyword,
								mFormFindKeyword.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0)
						{
							// 発見
							DataGridViewList.Rows[i].Cells[j].Selected = true;
							YlCommon.ScrollDataGridViewIfNeeded(DataGridViewList, i);
							return;
						}
					}
				}
			}

			throw new Exception("キーワード「" + mFormFindKeyword.Keyword + "」は\n見つかりませんでした。");
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = "ゆかり検索対象ファイル一覧";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			// データベース読み込み
			mTFounds = new List<TFound>();
			using (SQLiteConnection aYukariDbConnection = YlCommon.CreateYukariDbConnection(mYukaListerSettings))
			{
				using (DataContext aYukariDbContext = new DataContext(aYukariDbConnection))
				{
					Table<TFound> aTableFound = aYukariDbContext.GetTable<TFound>();
					IQueryable<TFound> aQueryResult =
							from x in aTableFound
							select x;
					foreach (TFound aTFound in aQueryResult)
					{
						mTFounds.Add(aTFound);
					}
				}
			}

			// 出力項目
			OutputSettings aOutputSettings = new OutputSettings();
			aOutputSettings.Load();
			mRuntimeOutputItems = aOutputSettings.RuntimeOutputItems();
#if DEBUG
			if (mRuntimeOutputItems.IndexOf(OutputItems.ArtistRuby) < 0)
			{
				mRuntimeOutputItems.Insert(4, OutputItems.ArtistRuby);
			}
#endif

			// DGV
			InitDataGridView();

			// 設計時サイズ以下にできないようにする
			MinimumSize = Size;


			Common.CascadeForm(this);
		}

		// --------------------------------------------------------------------
		// データグリッドビュー初期化
		// --------------------------------------------------------------------
		private void InitDataGridView()
		{
			// カラム作成
			foreach (OutputItems aOutputItem in mRuntimeOutputItems)
			{
				DataGridViewTextBoxColumn aColumn = new DataGridViewTextBoxColumn();
				aColumn.Name = aOutputItem.ToString();
				if (aOutputItem == OutputItems.SmartTrack)
				{
					aColumn.HeaderText = "On/Off";
				}
				else
				{
					aColumn.HeaderText = YlCommon.OUTPUT_ITEM_NAMES[(Int32)aOutputItem];
				}
				aColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
				DataGridViewList.Columns.Add(aColumn);
			}

			// 行作成
			for (Int32 i = 0; i < mTFounds.Count; i++)
			{
				DataGridViewList.Rows.Add();
			}

			// セルスタイル
			mNormalCellStyle = new DataGridViewCellStyle();
			mEmptyCellStyle = new DataGridViewCellStyle();
			mEmptyCellStyle.BackColor = Color.FromArgb(255, 225, 225);
		}

		// --------------------------------------------------------------------
		// 選択されている列番号
		// --------------------------------------------------------------------
		private Int32 SelectedColumnIndex()
		{
			Int32 aColumnIndex = -1;
			foreach (DataGridViewCell aCell in DataGridViewList.SelectedCells)
			{
				aColumnIndex = aCell.ColumnIndex;
				break;
			}

			return aColumnIndex;
		}

		// --------------------------------------------------------------------
		// 選択されている行番号
		// --------------------------------------------------------------------
		private Int32 SelectedRowIndex()
		{
			Int32 aRowIndex = -1;
			foreach (DataGridViewCell aCell in DataGridViewList.SelectedCells)
			{
				aRowIndex = aCell.RowIndex;
				break;
			}

			return aRowIndex;
		}

		// --------------------------------------------------------------------
		// SmartTrackOnVocal / SmartTrackOffVocal を数値化
		// --------------------------------------------------------------------
		private Int32 SmartTrackToInt32(TFound oTFound)
		{
			return (oTFound.SmartTrackOnVocal ? 2 : 0) + (oTFound.SmartTrackOffVocal ? 1 : 0);
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void WmFindKeywordNextRequested()
		{
			try
			{
				FindKeyword(true);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "次を検索時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void WmFindKeywordPrevRequested()
		{
			try
			{
				FindKeyword(false);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "前を検索時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// IDE 生成イベントハンドラー（ファイル一覧ウィンドウ）
		// ====================================================================

		private void FormViewTFounds_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ファイル一覧ウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル一覧ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridViewList_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				if (e.RowIndex < 0 || e.RowIndex >= mTFounds.Count)
				{
					return;
				}
				if (e.ColumnIndex < 0 || e.ColumnIndex >= mRuntimeOutputItems.Count)
				{
					return;
				}

				String aCellValue = CellValue(e.RowIndex, e.ColumnIndex);
				e.Value = aCellValue;
				if (String.IsNullOrEmpty(aCellValue))
				{
					DataGridViewList.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = mEmptyCellStyle;
				}
				else
				{
					DataGridViewList.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = mNormalCellStyle;
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "DGV セル値必要時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditMusicInfo_Click(object sender, EventArgs e)
		{
			try
			{
				Int32 aRowIndex = SelectedRowIndex();
				if (aRowIndex < 0 || aRowIndex >= mTFounds.Count)
				{
					return;
				}
				DisposeFormFindKeywordIfNeeded();

				String aPath = mTFounds[aRowIndex].Path;

				// ファイル命名規則とフォルダー固定値を適用
				FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings(Path.GetDirectoryName(aPath));
				FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
				Dictionary<String, String> aDic = YlCommon.MatchFileNameRulesAndFolderRule
						(Path.GetFileNameWithoutExtension(aPath), aFolderSettingsInMemory);

				// 楽曲名が取得できていない場合は編集不可
				if (String.IsNullOrEmpty(aDic[YlCommon.RULE_VAR_TITLE]))
				{
					mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名から楽曲名を取得できていないため、編集できません。\nファイル命名規則を確認して下さい。");
					return;
				}

				using (FormEditMusicInfo aFormEditMusicInfo = new FormEditMusicInfo(Path.GetFileName(aPath), aDic, mYukaListerSettings, mLogWriter))
				{
					aFormEditMusicInfo.ShowDialog(this);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridViewList_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			try
			{
				if (e.ColumnIndex < 0 || e.ColumnIndex >= mRuntimeOutputItems.Count)
				{
					return;
				}
				OutputItems aTargetItem = mRuntimeOutputItems[e.ColumnIndex];
				DataGridViewColumnHeaderCell aTargetHeaderCell = DataGridViewList.Columns[e.ColumnIndex].HeaderCell;
				Debug.WriteLine("DataGridViewList_ColumnHeaderMouseClick() " + aTargetItem.ToString());

				// 並び替えの方向（昇順か降順か）を決める
				SortOrder aSortOrder;
				if (aTargetItem == mSortedItem)
				{
					// 方向反転
					if (aTargetHeaderCell.SortGlyphDirection == SortOrder.Ascending)
					{
						aSortOrder = SortOrder.Descending;
					}
					else
					{
						aSortOrder = SortOrder.Ascending;
					}
				}
				else
				{
					aSortOrder = SortOrder.Ascending;

					if (mSortedItem != OutputItems.__End__)
					{
						// 今までの並び替えグリフを消す
						DataGridViewList.Columns[mRuntimeOutputItems.IndexOf(mSortedItem)].HeaderCell.SortGlyphDirection = SortOrder.None;
					}
				}

				// データのソート
				if (aSortOrder == SortOrder.Ascending)
				{
					switch (aTargetItem)
					{
						case OutputItems.Path:
							mTFounds.Sort((x, y) => String.Compare(x.Path, y.Path, true));
							break;
						case OutputItems.FileName:
							mTFounds.Sort((x, y) => String.Compare(Path.GetFileName(x.Path), Path.GetFileName(y.Path), true));
							break;
						case OutputItems.Head:
							mTFounds.Sort((x, y) => String.Compare(x.Head, y.Head, true));
							break;
						case OutputItems.Worker:
							mTFounds.Sort((x, y) => String.Compare(x.Worker, y.Worker, true));
							break;
						case OutputItems.Track:
							mTFounds.Sort((x, y) => String.Compare(x.Track, y.Track, true));
							break;
						case OutputItems.SmartTrack:
							mTFounds.Sort((x, y) => SmartTrackToInt32(y) - SmartTrackToInt32(x));
							break;
						case OutputItems.Comment:
							mTFounds.Sort((x, y) => String.Compare(x.Comment, y.Comment, true));
							break;
						case OutputItems.LastWriteTime:
							mTFounds.Sort((x, y) => x.LastWriteTime.CompareTo(y.LastWriteTime));
							break;
						case OutputItems.FileSize:
							mTFounds.Sort((x, y) => x.FileSize.CompareTo(y.FileSize));
							break;
						case OutputItems.SongName:
							mTFounds.Sort((x, y) => String.Compare(x.SongName, y.SongName, true));
							break;
						case OutputItems.SongRuby:
							mTFounds.Sort((x, y) => String.Compare(x.SongRuby, y.SongRuby, true));
							break;
						case OutputItems.SongOpEd:
							mTFounds.Sort((x, y) => String.Compare(x.SongOpEd, y.SongOpEd, true));
							break;
						case OutputItems.SongReleaseDate:
							mTFounds.Sort((x, y) => x.SongReleaseDate.CompareTo(y.SongReleaseDate));
							break;
						case OutputItems.ArtistName:
							mTFounds.Sort((x, y) => String.Compare(x.ArtistName, y.ArtistName, true));
							break;
						case OutputItems.ArtistRuby:
							mTFounds.Sort((x, y) => String.Compare(x.ArtistRuby, y.ArtistRuby, true));
							break;
						case OutputItems.LyristName:
							mTFounds.Sort((x, y) => String.Compare(x.LyristName, y.LyristName, true));
							break;
						case OutputItems.LyristRuby:
							mTFounds.Sort((x, y) => String.Compare(x.LyristRuby, y.LyristRuby, true));
							break;
						case OutputItems.ComposerName:
							mTFounds.Sort((x, y) => String.Compare(x.ComposerName, y.ComposerName, true));
							break;
						case OutputItems.ComposerRuby:
							mTFounds.Sort((x, y) => String.Compare(x.ComposerRuby, y.ComposerRuby, true));
							break;
						case OutputItems.ArrangerName:
							mTFounds.Sort((x, y) => String.Compare(x.ArrangerName, y.ArrangerName, true));
							break;
						case OutputItems.ArrangerRuby:
							mTFounds.Sort((x, y) => String.Compare(x.ArrangerRuby, y.ArrangerRuby, true));
							break;
						case OutputItems.TieUpName:
							mTFounds.Sort((x, y) => String.Compare(x.TieUpName, y.TieUpName, true));
							break;
						case OutputItems.TieUpRuby:
							mTFounds.Sort((x, y) => String.Compare(x.TieUpRuby, y.TieUpRuby, true));
							break;
						case OutputItems.TieUpAgeLimit:
							mTFounds.Sort((x, y) => y.TieUpAgeLimit - x.TieUpAgeLimit);
							break;
						case OutputItems.Category:
							mTFounds.Sort((x, y) => String.Compare(x.Category, y.Category, true));
							break;
						case OutputItems.TieUpGroupName:
							mTFounds.Sort((x, y) => String.Compare(x.TieUpGroupName, y.TieUpGroupName, true));
							break;
						case OutputItems.TieUpGroupRuby:
							mTFounds.Sort((x, y) => String.Compare(x.TieUpGroupRuby, y.TieUpGroupRuby, true));
							break;
						case OutputItems.MakerName:
							mTFounds.Sort((x, y) => String.Compare(x.MakerName, y.MakerName, true));
							break;
						case OutputItems.MakerRuby:
							mTFounds.Sort((x, y) => String.Compare(x.MakerRuby, y.MakerRuby, true));
							break;
						default:
							Debug.Assert(false, "DataGridViewList_ColumnHeaderMouseClick() bad specified target item: " + aTargetItem.ToString());
							break;
					}
				}
				else
				{
					switch (aTargetItem)
					{
						case OutputItems.Path:
							mTFounds.Sort((x, y) => -String.Compare(x.Path, y.Path, true));
							break;
						case OutputItems.FileName:
							mTFounds.Sort((x, y) => -String.Compare(Path.GetFileName(x.Path), Path.GetFileName(y.Path), true));
							break;
						case OutputItems.Head:
							mTFounds.Sort((x, y) => -String.Compare(x.Head, y.Head, true));
							break;
						case OutputItems.Worker:
							mTFounds.Sort((x, y) => -String.Compare(x.Worker, y.Worker, true));
							break;
						case OutputItems.Track:
							mTFounds.Sort((x, y) => -String.Compare(x.Track, y.Track, true));
							break;
						case OutputItems.SmartTrack:
							mTFounds.Sort((x, y) => SmartTrackToInt32(x) - SmartTrackToInt32(y));
							break;
						case OutputItems.Comment:
							mTFounds.Sort((x, y) => -String.Compare(x.Comment, y.Comment, true));
							break;
						case OutputItems.LastWriteTime:
							mTFounds.Sort((x, y) => -x.LastWriteTime.CompareTo(y.LastWriteTime));
							break;
						case OutputItems.FileSize:
							mTFounds.Sort((x, y) => -x.FileSize.CompareTo(y.FileSize));
							break;
						case OutputItems.SongName:
							mTFounds.Sort((x, y) => -String.Compare(x.SongName, y.SongName, true));
							break;
						case OutputItems.SongRuby:
							mTFounds.Sort((x, y) => -String.Compare(x.SongRuby, y.SongRuby, true));
							break;
						case OutputItems.SongOpEd:
							mTFounds.Sort((x, y) => -String.Compare(x.SongOpEd, y.SongOpEd, true));
							break;
						case OutputItems.SongReleaseDate:
							mTFounds.Sort((x, y) => -x.SongReleaseDate.CompareTo(y.SongReleaseDate));
							break;
						case OutputItems.ArtistName:
							mTFounds.Sort((x, y) => -String.Compare(x.ArtistName, y.ArtistName, true));
							break;
						case OutputItems.ArtistRuby:
							mTFounds.Sort((x, y) => -String.Compare(x.ArtistRuby, y.ArtistRuby, true));
							break;
						case OutputItems.LyristName:
							mTFounds.Sort((x, y) => -String.Compare(x.LyristName, y.LyristName, true));
							break;
						case OutputItems.LyristRuby:
							mTFounds.Sort((x, y) => -String.Compare(x.LyristRuby, y.LyristRuby, true));
							break;
						case OutputItems.ComposerName:
							mTFounds.Sort((x, y) => -String.Compare(x.ComposerName, y.ComposerName, true));
							break;
						case OutputItems.ComposerRuby:
							mTFounds.Sort((x, y) => -String.Compare(x.ComposerRuby, y.ComposerRuby, true));
							break;
						case OutputItems.ArrangerName:
							mTFounds.Sort((x, y) => -String.Compare(x.ArrangerName, y.ArrangerName, true));
							break;
						case OutputItems.ArrangerRuby:
							mTFounds.Sort((x, y) => -String.Compare(x.ArrangerRuby, y.ArrangerRuby, true));
							break;
						case OutputItems.TieUpName:
							mTFounds.Sort((x, y) => -String.Compare(x.TieUpName, y.TieUpName, true));
							break;
						case OutputItems.TieUpRuby:
							mTFounds.Sort((x, y) => -String.Compare(x.TieUpRuby, y.TieUpRuby, true));
							break;
						case OutputItems.TieUpAgeLimit:
							mTFounds.Sort((x, y) => x.TieUpAgeLimit - y.TieUpAgeLimit);
							break;
						case OutputItems.Category:
							mTFounds.Sort((x, y) => -String.Compare(x.Category, y.Category, true));
							break;
						case OutputItems.TieUpGroupName:
							mTFounds.Sort((x, y) => -String.Compare(x.TieUpGroupName, y.TieUpGroupName, true));
							break;
						case OutputItems.TieUpGroupRuby:
							mTFounds.Sort((x, y) => -String.Compare(x.TieUpGroupRuby, y.TieUpGroupRuby, true));
							break;
						case OutputItems.MakerName:
							mTFounds.Sort((x, y) => -String.Compare(x.MakerName, y.MakerName, true));
							break;
						case OutputItems.MakerRuby:
							mTFounds.Sort((x, y) => -String.Compare(x.MakerRuby, y.MakerRuby, true));
							break;
						default:
							Debug.Assert(false, "DataGridViewList_ColumnHeaderMouseClick() bad specified target item: " + aTargetItem.ToString());
							break;
					}
				}

				// 並び替えグリフの表示
				aTargetHeaderCell.SortGlyphDirection = aSortOrder;
				mSortedItem = aTargetItem;

				// 結果の表示
				DataGridViewList.Invalidate();

			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "DGV ヘッダークリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridViewList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				ButtonEditMusicInfo.PerformClick();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "DGV ダブルクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonFind_Click(object sender, EventArgs e)
		{
			try
			{
				// 検索ウィンドウ準備
				if (mFormFindKeyword == null || mFormFindKeyword.IsDisposed)
				{
					mFormFindKeyword = new FormFindKeyword(mLogWriter);
				}

				// 表示
				if (mFormFindKeyword.Visible)
				{
					mFormFindKeyword.Activate();
				}
				else
				{
					mFormFindKeyword.Show(this);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonFindEmptyCell_Click(object sender, EventArgs e)
		{
			try
			{
				FindEmptyOrNonEmptyCell(true);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "空欄検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonFindNormalCell_Click(object sender, EventArgs e)
		{
			try
			{
				FindEmptyOrNonEmptyCell(false);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "入力済み検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonFolderSettings_Click(object sender, EventArgs e)
		{
			try
			{
				Int32 aRowIndex = SelectedRowIndex();
				if (aRowIndex < 0 || aRowIndex >= mTFounds.Count)
				{
					return;
				}
				DisposeFormFindKeywordIfNeeded();

				using (FormFolderSettings aFormFolderSettings = new FormFolderSettings(Path.GetDirectoryName(mTFounds[aRowIndex].Path), mYukaListerSettings, mLogWriter))
				{
					aFormFolderSettings.ShowDialog(this);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormViewTFounds_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				DisposeFormFindKeywordIfNeeded();
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ファイル一覧ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォームクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void LinkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				YlCommon.ShowHelp("Fileichiranwindow");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}
