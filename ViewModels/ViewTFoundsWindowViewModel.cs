// ============================================================================
// 
// ファイル一覧ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.Windows;

using Shinta;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.OutputWriters;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class ViewTFoundsWindowViewModel : ViewModel
	{
		/* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

		/* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

		/* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

		/* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// ウィンドウタイトル（デフォルトが null だと実行時にエラーが発生するので Empty にしておく）
		private String mTitle = String.Empty;
		public String Title
		{
			get => mTitle;
			set => RaisePropertyChangedIfSet(ref mTitle, value);
		}

		// 列
		public ObservableCollection<DataGridColumn> Columns { get; set; } = new ObservableCollection<DataGridColumn>();

		// 選択行
		private TFound mSelectedTFound;
		public TFound SelectedTFound
		{
			get => mSelectedTFound;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedTFound, value))
				{
					ButtonEditMusicInfoClickedCommand.RaiseCanExecuteChanged();
					ButtonFolderSettingsClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// カレントセル位置
		private System.Drawing.Point mCurrentCellLocation;
		public System.Drawing.Point CurrentCellLocation
		{
			get => mCurrentCellLocation;
			set => RaisePropertyChangedIfSet(ref mCurrentCellLocation, value);
		}

		// ファイル群
		private List<TFound> mTFounds;
		public List<TFound> TFounds
		{
			get => mTFounds;
			set => RaisePropertyChangedIfSet(ref mTFounds, value);
		}


		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// ゆかり用リストデータベース（作業用インメモリ）
		public YukariListDatabaseInMemory YukariListDbInMemory { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region ヘルプリンクの制御
		public ListenerCommand<String> HelpClickedCommand
		{
			get => Environment?.HelpClickedCommand;
		}
		#endregion

		#region データグリッドダブルクリックの制御
		private ViewModelCommand mDataGridDoubleClickedCommand;

		public ViewModelCommand DataGridDoubleClickedCommand
		{
			get
			{
				if (mDataGridDoubleClickedCommand == null)
				{
					mDataGridDoubleClickedCommand = new ViewModelCommand(DataGridDoubleClicked);
				}
				return mDataGridDoubleClickedCommand;
			}
		}

		public void DataGridDoubleClicked()
		{
			try
			{
				EditMusicInfo();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "データグリッドダブルクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ソートの制御
		private ListenerCommand<DataGridSortingEventArgs> mDataGridListSortingCommand;

		public ListenerCommand<DataGridSortingEventArgs> DataGridListSortingCommand
		{
			get
			{
				if (mDataGridListSortingCommand == null)
				{
					mDataGridListSortingCommand = new ListenerCommand<DataGridSortingEventArgs>(DataGridListSorting);
				}
				return mDataGridListSortingCommand;
			}
		}

		public void DataGridListSorting(DataGridSortingEventArgs oDataGridSortingEventArgs)
		{
			try
			{
				TFound aPrevSelectedTFound = SelectedTFound;

				// 並び替えの方向（昇順か降順か）を決める
				ListSortDirection aNewDirection;
				if (oDataGridSortingEventArgs.Column.SortDirection == ListSortDirection.Ascending)
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
					switch (mRuntimeOutputItems[oDataGridSortingEventArgs.Column.DisplayIndex])
					{
						case OutputItems.Path:
							TFounds.Sort((x, y) => String.Compare(x.Path, y.Path, true));
							break;
						case OutputItems.FileName:
							TFounds.Sort((x, y) => String.Compare(x.FileName, y.FileName, true));
							break;
						case OutputItems.Head:
							TFounds.Sort((x, y) => String.Compare(x.Head, y.Head, true));
							break;
						case OutputItems.Worker:
							TFounds.Sort((x, y) => String.Compare(x.Worker, y.Worker, true));
							break;
						case OutputItems.Track:
							TFounds.Sort((x, y) => String.Compare(x.Track, y.Track, true));
							break;
						case OutputItems.SmartTrack:
							TFounds.Sort((x, y) => SmartTrackToInt32(y) - SmartTrackToInt32(x));
							break;
						case OutputItems.Comment:
							TFounds.Sort((x, y) => String.Compare(x.Comment, y.Comment, true));
							break;
						case OutputItems.LastWriteTime:
							TFounds.Sort((x, y) => x.LastWriteTime.CompareTo(y.LastWriteTime));
							break;
						case OutputItems.FileSize:
							TFounds.Sort((x, y) => x.FileSize.CompareTo(y.FileSize));
							break;
						case OutputItems.SongName:
							TFounds.Sort((x, y) => String.Compare(x.SongName, y.SongName, true));
							break;
						case OutputItems.SongRuby:
							TFounds.Sort((x, y) => String.Compare(x.SongRuby, y.SongRuby, true));
							break;
						case OutputItems.SongOpEd:
							TFounds.Sort((x, y) => String.Compare(x.SongOpEd, y.SongOpEd, true));
							break;
						case OutputItems.SongReleaseDate:
							TFounds.Sort((x, y) => x.SongReleaseDate.CompareTo(y.SongReleaseDate));
							break;
						case OutputItems.ArtistName:
							TFounds.Sort((x, y) => String.Compare(x.ArtistName, y.ArtistName, true));
							break;
						case OutputItems.ArtistRuby:
							TFounds.Sort((x, y) => String.Compare(x.ArtistRuby, y.ArtistRuby, true));
							break;
						case OutputItems.LyristName:
							TFounds.Sort((x, y) => String.Compare(x.LyristName, y.LyristName, true));
							break;
						case OutputItems.LyristRuby:
							TFounds.Sort((x, y) => String.Compare(x.LyristRuby, y.LyristRuby, true));
							break;
						case OutputItems.ComposerName:
							TFounds.Sort((x, y) => String.Compare(x.ComposerName, y.ComposerName, true));
							break;
						case OutputItems.ComposerRuby:
							TFounds.Sort((x, y) => String.Compare(x.ComposerRuby, y.ComposerRuby, true));
							break;
						case OutputItems.ArrangerName:
							TFounds.Sort((x, y) => String.Compare(x.ArrangerName, y.ArrangerName, true));
							break;
						case OutputItems.ArrangerRuby:
							TFounds.Sort((x, y) => String.Compare(x.ArrangerRuby, y.ArrangerRuby, true));
							break;
						case OutputItems.TieUpName:
							TFounds.Sort((x, y) => String.Compare(x.TieUpName, y.TieUpName, true));
							break;
						case OutputItems.TieUpRuby:
							TFounds.Sort((x, y) => String.Compare(x.TieUpRuby, y.TieUpRuby, true));
							break;
						case OutputItems.TieUpAgeLimit:
							TFounds.Sort((x, y) => y.TieUpAgeLimit - x.TieUpAgeLimit);
							break;
						case OutputItems.Category:
							TFounds.Sort((x, y) => String.Compare(x.Category, y.Category, true));
							break;
						case OutputItems.TieUpGroupName:
							TFounds.Sort((x, y) => String.Compare(x.TieUpGroupName, y.TieUpGroupName, true));
							break;
						case OutputItems.TieUpGroupRuby:
							TFounds.Sort((x, y) => String.Compare(x.TieUpGroupRuby, y.TieUpGroupRuby, true));
							break;
						case OutputItems.MakerName:
							TFounds.Sort((x, y) => String.Compare(x.MakerName, y.MakerName, true));
							break;
						case OutputItems.MakerRuby:
							TFounds.Sort((x, y) => String.Compare(x.MakerRuby, y.MakerRuby, true));
							break;
						default:
							Debug.Assert(false, "DataGridViewList_ColumnHeaderMouseClick() bad specified target item: " + mRuntimeOutputItems[oDataGridSortingEventArgs.Column.DisplayIndex].ToString());
							break;
					}
				}
				else
				{
					switch (mRuntimeOutputItems[oDataGridSortingEventArgs.Column.DisplayIndex])
					{
						case OutputItems.Path:
							TFounds.Sort((x, y) => -String.Compare(x.Path, y.Path, true));
							break;
						case OutputItems.FileName:
							TFounds.Sort((x, y) => -String.Compare(x.FileName, y.FileName, true));
							break;
						case OutputItems.Head:
							TFounds.Sort((x, y) => -String.Compare(x.Head, y.Head, true));
							break;
						case OutputItems.Worker:
							TFounds.Sort((x, y) => -String.Compare(x.Worker, y.Worker, true));
							break;
						case OutputItems.Track:
							TFounds.Sort((x, y) => -String.Compare(x.Track, y.Track, true));
							break;
						case OutputItems.SmartTrack:
							TFounds.Sort((x, y) => SmartTrackToInt32(x) - SmartTrackToInt32(y));
							break;
						case OutputItems.Comment:
							TFounds.Sort((x, y) => -String.Compare(x.Comment, y.Comment, true));
							break;
						case OutputItems.LastWriteTime:
							TFounds.Sort((x, y) => -x.LastWriteTime.CompareTo(y.LastWriteTime));
							break;
						case OutputItems.FileSize:
							TFounds.Sort((x, y) => -x.FileSize.CompareTo(y.FileSize));
							break;
						case OutputItems.SongName:
							TFounds.Sort((x, y) => -String.Compare(x.SongName, y.SongName, true));
							break;
						case OutputItems.SongRuby:
							TFounds.Sort((x, y) => -String.Compare(x.SongRuby, y.SongRuby, true));
							break;
						case OutputItems.SongOpEd:
							TFounds.Sort((x, y) => -String.Compare(x.SongOpEd, y.SongOpEd, true));
							break;
						case OutputItems.SongReleaseDate:
							TFounds.Sort((x, y) => -x.SongReleaseDate.CompareTo(y.SongReleaseDate));
							break;
						case OutputItems.ArtistName:
							TFounds.Sort((x, y) => -String.Compare(x.ArtistName, y.ArtistName, true));
							break;
						case OutputItems.ArtistRuby:
							TFounds.Sort((x, y) => -String.Compare(x.ArtistRuby, y.ArtistRuby, true));
							break;
						case OutputItems.LyristName:
							TFounds.Sort((x, y) => -String.Compare(x.LyristName, y.LyristName, true));
							break;
						case OutputItems.LyristRuby:
							TFounds.Sort((x, y) => -String.Compare(x.LyristRuby, y.LyristRuby, true));
							break;
						case OutputItems.ComposerName:
							TFounds.Sort((x, y) => -String.Compare(x.ComposerName, y.ComposerName, true));
							break;
						case OutputItems.ComposerRuby:
							TFounds.Sort((x, y) => -String.Compare(x.ComposerRuby, y.ComposerRuby, true));
							break;
						case OutputItems.ArrangerName:
							TFounds.Sort((x, y) => -String.Compare(x.ArrangerName, y.ArrangerName, true));
							break;
						case OutputItems.ArrangerRuby:
							TFounds.Sort((x, y) => -String.Compare(x.ArrangerRuby, y.ArrangerRuby, true));
							break;
						case OutputItems.TieUpName:
							TFounds.Sort((x, y) => -String.Compare(x.TieUpName, y.TieUpName, true));
							break;
						case OutputItems.TieUpRuby:
							TFounds.Sort((x, y) => -String.Compare(x.TieUpRuby, y.TieUpRuby, true));
							break;
						case OutputItems.TieUpAgeLimit:
							TFounds.Sort((x, y) => x.TieUpAgeLimit - y.TieUpAgeLimit);
							break;
						case OutputItems.Category:
							TFounds.Sort((x, y) => -String.Compare(x.Category, y.Category, true));
							break;
						case OutputItems.TieUpGroupName:
							TFounds.Sort((x, y) => -String.Compare(x.TieUpGroupName, y.TieUpGroupName, true));
							break;
						case OutputItems.TieUpGroupRuby:
							TFounds.Sort((x, y) => -String.Compare(x.TieUpGroupRuby, y.TieUpGroupRuby, true));
							break;
						case OutputItems.MakerName:
							TFounds.Sort((x, y) => -String.Compare(x.MakerName, y.MakerName, true));
							break;
						case OutputItems.MakerRuby:
							TFounds.Sort((x, y) => -String.Compare(x.MakerRuby, y.MakerRuby, true));
							break;
						default:
							Debug.Assert(false, "DataGridViewList_ColumnHeaderMouseClick() bad specified target item: " + mRuntimeOutputItems[oDataGridSortingEventArgs.Column.DisplayIndex].ToString());
							break;
					}
				}

				// 結果の表示
				List<TFound> aTmp = TFounds;
				TFounds = null;
				TFounds = aTmp;
				SelectedTFound = aPrevSelectedTFound;

				// 並び替えグリフの表示
				oDataGridSortingEventArgs.Column.SortDirection = aNewDirection;

				oDataGridSortingEventArgs.Handled = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "DGV ヘッダークリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 編集ボタンの制御
		private ViewModelCommand mButtonEditMusicInfoClickedCommand;

		public ViewModelCommand ButtonEditMusicInfoClickedCommand
		{
			get
			{
				if (mButtonEditMusicInfoClickedCommand == null)
				{
					mButtonEditMusicInfoClickedCommand = new ViewModelCommand(ButtonEditMusicInfoClicked, CanButtonEditMusicInfoClicked);
				}
				return mButtonEditMusicInfoClickedCommand;
			}
		}

		public bool CanButtonEditMusicInfoClicked()
		{
			return SelectedTFound != null;
		}

		public void ButtonEditMusicInfoClicked()
		{
			try
			{
				EditMusicInfo();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region フォルダー設定ボタンの制御
		private ViewModelCommand mButtonFolderSettingsClickedCommand;

		public ViewModelCommand ButtonFolderSettingsClickedCommand
		{
			get
			{
				if (mButtonFolderSettingsClickedCommand == null)
				{
					mButtonFolderSettingsClickedCommand = new ViewModelCommand(ButtonFolderSettingsClicked, CanButtonFolderSettingsClicked);
				}
				return mButtonFolderSettingsClickedCommand;
			}
		}

		public bool CanButtonFolderSettingsClicked()
		{
			return SelectedTFound != null;
		}

		public void ButtonFolderSettingsClicked()
		{
			try
			{
				if (SelectedTFound == null)
				{
					return;
				}
				CloseFindKeywordWindowIfNeeded();

				// フォルダー設定ウィンドウを開く
				using (FolderSettingsWindowViewModel aFolderSettingsWindowViewModel = new FolderSettingsWindowViewModel())
				{
					aFolderSettingsWindowViewModel.PathExLen = Path.GetDirectoryName(SelectedTFound.Path);
					aFolderSettingsWindowViewModel.Environment = Environment;
					Messenger.Raise(new TransitionMessage(aFolderSettingsWindowViewModel, "OpenFolderSettingsWindow"));
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region キーワード検索ボタンの制御
		private ViewModelCommand mButtonFindKeywordClickedCommand;

		public ViewModelCommand ButtonFindKeywordClickedCommand
		{
			get
			{
				if (mButtonFindKeywordClickedCommand == null)
				{
					mButtonFindKeywordClickedCommand = new ViewModelCommand(ButtonFindClicked);
				}
				return mButtonFindKeywordClickedCommand;
			}
		}

		public void ButtonFindClicked()
		{
			try
			{
				ShowFindKeywordWindow();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "キーワード検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 空きセル・入力済みセル検索ボタンの制御
		private ListenerCommand<String> mButtonFindCellClickedCommand;

		public ListenerCommand<String> ButtonFindCellClickedCommand
		{
			get
			{
				if (mButtonFindCellClickedCommand == null)
				{
					mButtonFindCellClickedCommand = new ListenerCommand<String>(ButtonFindEmptyCellClicked);
				}
				return mButtonFindCellClickedCommand;
			}
		}

		public void ButtonFindEmptyCellClicked(String oParameter)
		{
			try
			{
				FindEmptyOrNonEmptyCell(String.IsNullOrEmpty(oParameter));
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "セル検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// キーワード検索が要求された
		// --------------------------------------------------------------------
		public void FindKeywordRequested()
		{
			try
			{
				FindKeyword();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "検索時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 初期化
		// 本関数を呼ぶ前に Environment 等を設定しておく必要がある
		// --------------------------------------------------------------------
		public void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			Debug.Assert(YukariListDbInMemory != null, "YukariListDbInMemory is null");
			try
			{
				// タイトルバー
				Title = "ゆかり検索対象ファイル一覧";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif
				// 出力項目
				OutputSettings aOutputSettings = new OutputSettings();
				aOutputSettings.Load();
				mRuntimeOutputItems = aOutputSettings.RuntimeOutputItems();

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
						aColumn.Header = YlConstants.OUTPUT_ITEM_NAMES[(Int32)aOutputItem];
					}
					Columns.Add(aColumn);
				}

				// データベース読み込み
				using (DataContext aYukariDbContext = new DataContext(YukariListDbInMemory.Connection))
				{
					Table<TFound> aTableFound = aYukariDbContext.GetTable<TFound>();
					IQueryable<TFound> aQueryResult =
							from x in aTableFound
							select x;
					TFounds = aQueryResult.ToList();
				}

				// カーソルを左上にする（変更検知のため一旦ダミーを設定する）
				CurrentCellLocation = new System.Drawing.Point(1, 0);
				CurrentCellLocation = System.Drawing.Point.Empty;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル一覧ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リソース解放
		// --------------------------------------------------------------------
		protected override void Dispose(Boolean oDisposing)
		{
			if (oDisposing)
			{
				if (mFindKeywordWindowViewModel != null)
				{
					mFindKeywordWindowViewModel.Dispose();
				}
			}
			base.Dispose(oDisposing);
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 出力項目
		List<OutputItems> mRuntimeOutputItems;

		// 検索ウィンドウのビューモデル
		FindKeywordWindowViewModel mFindKeywordWindowViewModel;


		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// セルに表示されている値
		// oX, oY の範囲チェックはしない（呼びだし元でチェック済みである必要がある）
		// --------------------------------------------------------------------
		private String CellValue(Int32 oX, Int32 oY)
		{
			PropertyInfo aPropertyInfo = typeof(TFound).GetProperty(mRuntimeOutputItems[oX].ToString());
			Object aValue = aPropertyInfo.GetValue(TFounds[oY]);
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
		// 必要に応じて検索ウィンドウを閉じる
		// --------------------------------------------------------------------
		private void CloseFindKeywordWindowIfNeeded()
		{
			if (mFindKeywordWindowViewModel != null && !mFindKeywordWindowViewModel.IsClosed)
			{
				mFindKeywordWindowViewModel.Messenger.Raise(new WindowActionMessage("Close"));
			}
		}

		// --------------------------------------------------------------------
		// 指定された TFound に対して編集ウィンドウを開く
		// --------------------------------------------------------------------
		private void EditMusicInfo()
		{
			if (SelectedTFound == null)
			{
				return;
			}
			CloseFindKeywordWindowIfNeeded();

			String aPath = SelectedTFound.Path;

			// ファイル命名規則とフォルダー固定値を適用
			FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings2Ex(Path.GetDirectoryName(aPath));
			FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
			Dictionary<String, String> aDic = YlCommon.MatchFileNameRulesAndFolderRule
					(Path.GetFileNameWithoutExtension(aPath), aFolderSettingsInMemory);

			// 楽曲名が取得できていない場合は編集不可
			if (String.IsNullOrEmpty(aDic[YlConstants.RULE_VAR_TITLE]))
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名から楽曲名を取得できていないため、編集できません。\nファイル命名規則を確認して下さい。");
				return;
			}

			// 楽曲情報等編集ウィンドウを開く
			using (EditMusicInfoWindowViewModel aEditMusicInfoWindowViewModel = new EditMusicInfoWindowViewModel())
			{
				aEditMusicInfoWindowViewModel.Environment = Environment;
				aEditMusicInfoWindowViewModel.PathExLen = SelectedTFound.Path;
				aEditMusicInfoWindowViewModel.DicByFile = YlCommon.DicByFile(SelectedTFound.Path);
				Messenger.Raise(new TransitionMessage(aEditMusicInfoWindowViewModel, "OpenEditMusicInfoWindow"));
			}
		}

		// --------------------------------------------------------------------
		// 未登録または登録済みの項目を検索して選択
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void FindEmptyOrNonEmptyCell(Boolean oFindEmpty)
		{
			if (CurrentCellLocation.Y < 0 || CurrentCellLocation.Y >= TFounds.Count
					|| CurrentCellLocation.X < 0 || CurrentCellLocation.X >= mRuntimeOutputItems.Count)
			{
				throw new Exception("セルを選択して下さい。");
			}

			for (Int32 i = CurrentCellLocation.Y + 1; i < TFounds.Count; i++)
			{
				if (String.IsNullOrEmpty(CellValue(CurrentCellLocation.X, i)) == oFindEmpty)
				{
					// 発見
					CurrentCellLocation = new System.Drawing.Point(CurrentCellLocation.X, i);
					return;
				}
			}

			throw new Exception("選択されたセルより下には、" + YlConstants.OUTPUT_ITEM_NAMES[(Int32)mRuntimeOutputItems[CurrentCellLocation.X]] + "が空欄"
					+ (oFindEmpty ? "の" : "ではない") + "セルはありません。");
		}

		// --------------------------------------------------------------------
		// キーワード検索ウィンドウの情報を元に検索
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void FindKeyword()
		{
			if (String.IsNullOrEmpty(mFindKeywordWindowViewModel.Keyword))
			{
				throw new Exception("キーワードが指定されていません。");
			}
			String aKeyword = mFindKeywordWindowViewModel.Keyword.Trim();
			if (String.IsNullOrEmpty(aKeyword))
			{
				throw new Exception("キーワードが指定されていません。");
			}

			Int32 aBeginRowIndex = CurrentCellLocation.Y;
			Int32 aDirection = mFindKeywordWindowViewModel.Direction;
			Debug.Assert(aDirection == 1 || aDirection == -1, "FindKeyword() direction not set");
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
					aBeginRowIndex = TFounds.Count - 1;
				}
			}

			for (Int32 i = aBeginRowIndex; aDirection == 1 ? i < TFounds.Count : i >= 0; i += aDirection)
			{
				Int32 aBeginColumnIndex;
				if (i == aBeginRowIndex)
				{
					aBeginColumnIndex = CurrentCellLocation.X + aDirection;
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
					if (mFindKeywordWindowViewModel.WholeMatch)
					{
						if (String.Compare(CellValue(j, i), aKeyword, !mFindKeywordWindowViewModel.CaseSensitive) == 0)
						{
							// 発見
							CurrentCellLocation = new System.Drawing.Point(j, i);
							return;
						}
					}
					else
					{
						if (!String.IsNullOrEmpty(CellValue(j, i))
								&& CellValue(j, i).IndexOf(aKeyword,
								mFindKeywordWindowViewModel.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0)
						{
							// 発見
							CurrentCellLocation = new System.Drawing.Point(j, i);
							return;
						}
					}
				}
			}

			throw new Exception("キーワード「" + aKeyword + "」は\n見つかりませんでした。");
		}

		// --------------------------------------------------------------------
		// 検索ウィンドウを表示する
		// --------------------------------------------------------------------
		private void ShowFindKeywordWindow()
		{
			if (mFindKeywordWindowViewModel == null)
			{
				// 新規作成
				mFindKeywordWindowViewModel = new FindKeywordWindowViewModel();
				mFindKeywordWindowViewModel.Environment = Environment;
				mFindKeywordWindowViewModel.ViewTFoundsWindowViewModel = this;
				Messenger.Raise(new TransitionMessage(mFindKeywordWindowViewModel, "OpenFindKeywordWindow"));
			}
			else if (mFindKeywordWindowViewModel.IsClosed)
			{
				// 閉じられたウィンドウからプロパティーを引き継ぐ
				FindKeywordWindowViewModel aOld = mFindKeywordWindowViewModel;
				mFindKeywordWindowViewModel = new FindKeywordWindowViewModel();
				mFindKeywordWindowViewModel.CopyFrom(aOld);
				aOld.Dispose();
				Messenger.Raise(new TransitionMessage(mFindKeywordWindowViewModel, "OpenFindKeywordWindow"));
			}

			// ウィンドウを前面に出すなど
			mFindKeywordWindowViewModel.Messenger.Raise(new InteractionMessage("Activate"));
		}

		// --------------------------------------------------------------------
		// SmartTrackOnVocal / SmartTrackOffVocal を数値化
		// --------------------------------------------------------------------
		private Int32 SmartTrackToInt32(TFound oTFound)
		{
			return (oTFound.SmartTrackOnVocal ? 2 : 0) + (oTFound.SmartTrackOffVocal ? 1 : 0);
		}

	}
	// public class ViewTFoundsWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
