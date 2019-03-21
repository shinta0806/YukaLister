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
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;
using YukaLister.Shared;

namespace YukaLister
{
	/// <summary>
	/// ViewTFoundsWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class ViewTFoundsWindow : Window
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public ViewTFoundsWindow(YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mYukaListerSettings = oYukaListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 出力項目
		List<OutputItems> mRuntimeOutputItems;

		// ファイル群
		List<TFound> mTFounds;

		// 最後に選択されたセルの位置（フォーカスを失うと取得できないので保存しておく）
		Int32 mLastSelectedRowIndex = -1;
		Int32 mLastSelectedColumnIndex = -1;

		// ソート中の項目
		//OutputItems mSortedItem = OutputItems.__End__;

		// 検索ウィンドウ
		FindKeywordWindow mFindKeywordWindow;

		// 環境設定
		private YukaListerSettings mYukaListerSettings;

		// ウィンドウハンドル
		private IntPtr mHandle;

		// Ctrl+F 捕捉用
		private RoutedCommand mRoutedCommandCtrlF = new RoutedCommand();

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// セルに表示されている値
		// oRowIndex, oColumnIndex の範囲チェックはしない（呼びだし元でチェック済みである必要がある）
		// --------------------------------------------------------------------
		private String CellValue(Int32 oRowIndex, Int32 oColumnIndex)
		{
			PropertyInfo aPropertyInfo = typeof(TFound).GetProperty(mRuntimeOutputItems[oColumnIndex].ToString());
			Object aValue = aPropertyInfo.GetValue(mTFounds[oRowIndex]);
			if (aValue == null)
			{
				return null;
			}
			else
			{
				return aValue.ToString();
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ExecutedRoutedCommandCtrlF(object oSender, ExecutedRoutedEventArgs oExecutedRoutedEventArgs)
		{
			try
			{
				ShowFindKeywordWindow();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "Ctrl+F 時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 未登録または登録済みの項目を検索して選択
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void FindEmptyOrNonEmptyCell(Boolean oFindEmpty)
		{
			if (mLastSelectedRowIndex < 0 || mLastSelectedRowIndex >= mTFounds.Count
					|| mLastSelectedColumnIndex < 0 || mLastSelectedColumnIndex >= mRuntimeOutputItems.Count)
			{
				throw new Exception("セルを選択して下さい。");
			}

			for (Int32 i = mLastSelectedRowIndex + 1; i < mTFounds.Count; i++)
			{
				if (String.IsNullOrEmpty(CellValue(i, mLastSelectedColumnIndex)) == oFindEmpty)
				{
					// 発見
					SelectDataGridCell(i, mLastSelectedColumnIndex);
					return;
				}
			}

			throw new Exception("選択されたセルより下には、" + YlCommon.OUTPUT_ITEM_NAMES[(Int32)mRuntimeOutputItems[mLastSelectedColumnIndex]] + "が空欄"
					+ (oFindEmpty ? "の" : "ではない") + "セルはありません。");
		}

		// --------------------------------------------------------------------
		// 名称の編集ウィンドウを開く
		// --------------------------------------------------------------------
		private void EditMusicInfo()
		{
			Int32 aRowIndex = DataGridList.SelectedIndex;
			if (aRowIndex < 0 || aRowIndex >= mTFounds.Count)
			{
				return;
			}

			String aPath = mTFounds[aRowIndex].Path;

			// ファイル命名規則とフォルダー固定値を適用
			FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings2Ex(Path.GetDirectoryName(aPath));
			FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
			Dictionary<String, String> aDic = YlCommon.MatchFileNameRulesAndFolderRule
					(Path.GetFileNameWithoutExtension(aPath), aFolderSettingsInMemory);

			// 楽曲名が取得できていない場合は編集不可
			if (String.IsNullOrEmpty(aDic[YlCommon.RULE_VAR_TITLE]))
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名から楽曲名を取得できていないため、編集できません。\nファイル命名規則を確認して下さい。");
				return;
			}

			EditMusicInfoWindow aEditMusicInfoWindow = new EditMusicInfoWindow(Path.GetFileName(aPath), aDic, mYukaListerSettings, mLogWriter);
			aEditMusicInfoWindow.Owner = this;
			aEditMusicInfoWindow.ShowDialog();
		}

		// --------------------------------------------------------------------
		// キーワード検索ウィンドウの情報を元に検索
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void FindKeyword()
		{
			if (String.IsNullOrEmpty(mFindKeywordWindow.Keyword))
			{
				throw new Exception("キーワードが指定されていません。");
			}

			Int32 aBeginRowIndex = mLastSelectedRowIndex;
			Int32 aDirection = mFindKeywordWindow.Direction;
			Debug.Assert(aDirection != 0, "FindKeyword() direction not set");
			if (aDirection == 1)
			{
				if (aBeginRowIndex < 0)
				{
					aBeginRowIndex = 0;
				}
			}
			else
			{
				if (aBeginRowIndex < 0)
				{
					aBeginRowIndex = mTFounds.Count - 1;
				}
			}

			for (Int32 i = aBeginRowIndex; aDirection == 1 ? i < mTFounds.Count : i >= 0; i += aDirection)
			{
				Int32 aBeginColumnIndex;
				if (i == aBeginRowIndex)
				{
					aBeginColumnIndex = mLastSelectedColumnIndex + aDirection;
				}
				else
				{
					if (aDirection == 1)
					{
						aBeginColumnIndex = 0;
					}
					else
					{
						aBeginColumnIndex = mRuntimeOutputItems.Count - 1;
					}
				}

				for (Int32 j = aBeginColumnIndex; aDirection == 1 ? j < mRuntimeOutputItems.Count : j >= 0; j += aDirection)
				{
					if (mFindKeywordWindow.WholeMatch)
					{
						if (String.Compare(CellValue(i, j), mFindKeywordWindow.Keyword, !mFindKeywordWindow.CaseSensitive) == 0)
						{
							// 発見
							SelectDataGridCell(i, j);
							return;
						}
					}
					else
					{
						if (!String.IsNullOrEmpty(CellValue(i, j))
								&& CellValue(i, j).IndexOf(mFindKeywordWindow.Keyword,
								mFindKeywordWindow.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0)
						{
							// 発見
							SelectDataGridCell(i, j);
							return;
						}
					}
				}
			}

			throw new Exception("キーワード「" + mFindKeywordWindow.Keyword + "」は\n見つかりませんでした。");
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Common.DisableMinimizeBox(this);
			Title = "ゆかり検索対象ファイル一覧";
#if DEBUG
			Title = "［デバッグ］" + Title;
#endif

			// データベース読み込み
			using (DataContext aYukariDbContext = new DataContext(YlCommon.YukariDbInMemoryConnection))
			{
				Table<TFound> aTableFound = aYukariDbContext.GetTable<TFound>();
				IQueryable<TFound> aQueryResult =
						from x in aTableFound
						select x;
				mTFounds = new List<TFound>(aQueryResult);
			}
			DataGridList.ItemsSource = mTFounds;

			// 出力項目
			OutputSettings aOutputSettings = new OutputSettings();
			aOutputSettings.Load();
			mRuntimeOutputItems = aOutputSettings.RuntimeOutputItems();
#if DEBUGz
			// デバッグ時の歌手フリガナ追加
			if (mRuntimeOutputItems.IndexOf(OutputItems.ArtistRuby) < 0)
			{
				mRuntimeOutputItems.Insert(4, OutputItems.ArtistRuby);
			}
#endif
#if DEBUG
			// デバッグ時の作曲者追加
			if (mRuntimeOutputItems.IndexOf(OutputItems.ComposerName) < 0)
			{
				mRuntimeOutputItems.Insert(5, OutputItems.ComposerName);
			}
#endif

			// メッセージハンドラー
			WindowInteropHelper aHelper = new WindowInteropHelper(this);
			mHandle = aHelper.Handle;
			HwndSource aSource = HwndSource.FromHwnd(mHandle);
			aSource.AddHook(new HwndSourceHook(WndProc));

			// データグリッド
			InitDataGrid();

			// ショートカットキー
			mRoutedCommandCtrlF.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));
			CommandBinding aCommandBindingCtrlF = new CommandBinding(mRoutedCommandCtrlF, ExecutedRoutedCommandCtrlF);
			CommandBindings.Add(aCommandBindingCtrlF);
		}

		// --------------------------------------------------------------------
		// データグリッド初期化
		// --------------------------------------------------------------------
		private void InitDataGrid()
		{
			// カラム作成
			foreach (OutputItems aOutputItem in mRuntimeOutputItems)
			{
				DataGridTextColumn aColumn = new DataGridTextColumn();
				aColumn.Binding = new Binding(aOutputItem.ToString());
				if (aOutputItem == OutputItems.SmartTrack)
				{
					aColumn.Header = "On/Off";
				}
				else
				{
					aColumn.Header = YlCommon.OUTPUT_ITEM_NAMES[(Int32)aOutputItem];
				}
				//aColumn.CanUserSort = false;
				DataGridList.Columns.Add(aColumn);
			}
		}

		// --------------------------------------------------------------------
		// 自ウィンドウ宛へメッセージを送信
		// --------------------------------------------------------------------
		private void PostMessage(Wm oMessage, Int32 oWParam, Int32 oLParam)
		{
			WindowsApi.PostMessage(mHandle, (UInt32)oMessage, (IntPtr)oWParam, (IntPtr)oLParam);
		}

		// --------------------------------------------------------------------
		// データグリッドのセルを選択
		// --------------------------------------------------------------------
		private void SelectDataGridCell(Int32 oRowIndex, Int32 oColumnIndex)
		{
			if (Common.SelectDataGridCell(DataGridList, oRowIndex, oColumnIndex))
			{
				mLastSelectedRowIndex = oRowIndex;
				mLastSelectedColumnIndex = oColumnIndex;
			}
			else
			{
				mLastSelectedRowIndex = -1;
				mLastSelectedColumnIndex = -1;
			}
		}

		// --------------------------------------------------------------------
		// 検索ウィンドウを表示する
		// --------------------------------------------------------------------
		private void ShowFindKeywordWindow()
		{
			// 検索ウィンドウ準備
			if (mFindKeywordWindow != null && !mFindKeywordWindow.IsVisible)
			{
				mFindKeywordWindow = new FindKeywordWindow(mFindKeywordWindow);
				mFindKeywordWindow.Owner = this;
			}
			if (mFindKeywordWindow == null)
			{
				mFindKeywordWindow = new FindKeywordWindow(mLogWriter);
				mFindKeywordWindow.Owner = this;
			}

			// 表示
			if (mFindKeywordWindow.IsVisible)
			{
				mFindKeywordWindow.Activate();
			}
			else
			{
				mFindKeywordWindow.Show();
			}
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
		private void WmFindKeywordRequested()
		{
			try
			{
				FindKeyword();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "検索時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// メッセージハンドラ
		// --------------------------------------------------------------------
		private IntPtr WndProc(IntPtr oHWnd, Int32 oMsg, IntPtr oWParam, IntPtr oLParam, ref Boolean oHandled)
		{
			oHandled = true;
			switch ((Wm)oMsg)
			{
				case Wm.FindKeywordRequested:
					WmFindKeywordRequested();
					break;
				default:
					oHandled = false;
					break;
			}

			return IntPtr.Zero;
		}

		// ====================================================================
		// IDE 生成イベントハンドラー（ファイル一覧ウィンドウ）
		// ====================================================================

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				Init();

				// 左上のセルを選択
				Common.SelectDataGridCell(DataGridList, 0, 0);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル一覧ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_SourceInitialized(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ファイル一覧ウィンドウを開きます。");

				// 設計時サイズ以下にできないようにする
				MinWidth = ActualWidth;
				MinHeight = ActualHeight;

				Common.CascadeWindow(this);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ウィンドウ初期化時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditMusicInfo_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				EditMusicInfo();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridList_Sorting(object sender, DataGridSortingEventArgs e)
		{
			try
			{
				// 並び替えの方向（昇順か降順か）を決める
				ListSortDirection aNewDirection;
				if (e.Column.SortDirection == ListSortDirection.Ascending)
				{
					aNewDirection = ListSortDirection.Descending;
				}
				else
				{
					aNewDirection = ListSortDirection.Ascending;
				}

				// データのソート
				if (aNewDirection == ListSortDirection.Ascending)
				{
					switch (mRuntimeOutputItems[e.Column.DisplayIndex])
					{
						case OutputItems.Path:
							mTFounds.Sort((x, y) => String.Compare(x.Path, y.Path, true));
							break;
						case OutputItems.FileName:
							mTFounds.Sort((x, y) => String.Compare(x.FileName, y.FileName, true));
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
							Debug.Assert(false, "DataGridViewList_ColumnHeaderMouseClick() bad specified target item: " + mRuntimeOutputItems[e.Column.DisplayIndex].ToString());
							break;
					}
				}
				else
				{
					switch (mRuntimeOutputItems[e.Column.DisplayIndex])
					{
						case OutputItems.Path:
							mTFounds.Sort((x, y) => -String.Compare(x.Path, y.Path, true));
							break;
						case OutputItems.FileName:
							mTFounds.Sort((x, y) => -String.Compare(x.FileName, y.FileName, true));
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
							Debug.Assert(false, "DataGridViewList_ColumnHeaderMouseClick() bad specified target item: " + mRuntimeOutputItems[e.Column.DisplayIndex].ToString());
							break;
					}
				}

				// 並び替えグリフの表示
				e.Column.SortDirection = aNewDirection;

				// 結果の表示
				DataGridList.Items.Refresh();
				e.Handled = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "DGV ヘッダークリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			try
			{
				EditMusicInfo();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "DGV ダブルクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonFind_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ShowFindKeywordWindow();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonFindEmptyCell_Click(object sender, RoutedEventArgs e)
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

		private void ButtonFindNormalCell_Click(object sender, RoutedEventArgs e)
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

		private void ButtonFolderSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Int32 aRowIndex = DataGridList.SelectedIndex;
				if (aRowIndex < 0 || aRowIndex >= mTFounds.Count)
				{
					return;
				}

				FolderSettingsWindow aFolderSettingsWindow = new FolderSettingsWindow(Path.GetDirectoryName(mTFounds[aRowIndex].Path), mYukaListerSettings, mLogWriter);
				aFolderSettingsWindow.Owner = this;
				aFolderSettingsWindow.ShowDialog();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ファイル一覧ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォームクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			try
			{
				YlCommon.ShowHelp(e.Uri.OriginalString);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DialogResult = false;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "キャンセルボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				Common.GetDataGridCellPosition(DataGridList, e.MouseDevice.DirectlyOver as DependencyObject, out mLastSelectedRowIndex, out mLastSelectedColumnIndex);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "データグリッドマウスボタン押下プレビュー時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class ViewTFoundsWindow ___END___
}
// namespace YukaLister ___END___
