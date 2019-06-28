// ============================================================================
// 
// 楽曲詳細情報編集ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet.Commands;
using Livet.Messaging;

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using YukaLister.Models.SharedMisc;
using YukaLister.Models.Database;

namespace YukaLister.ViewModels
{
	public class EditSongWindowViewModel : EditCategorizableWindowViewModel
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

		// タイアップチェックボックスの有効無効
		private Boolean mIsTieUpEnabled;
		public Boolean IsTieUpEnabled
		{
			get => mIsTieUpEnabled;
			set
			{
				if (RaisePropertyChangedIfSet(ref mIsTieUpEnabled, value))
				{
					SetIsCategoryEnabled();
				}
			}
		}

		// タイアップあり
		private Boolean mHasTieUp;
		public Boolean HasTieUp
		{
			get => mHasTieUp;
			set
			{
				if (RaisePropertyChangedIfSet(ref mHasTieUp, value))
				{
					ButtonSearchTieUpClickedCommand.RaiseCanExecuteChanged();
					ButtonEditTieUpClickedCommand.RaiseCanExecuteChanged();
					if (!mHasTieUp)
					{
						TieUpId = null;
						TieUpName = null;
					}
					SetIsCategoryEnabled();
				}
			}
		}

		// タイアップ名
		private String mTieUpName;
		public String TieUpName
		{
			get => mTieUpName;
			set => RaisePropertyChangedIfSet(ref mTieUpName, value);
		}

		// 摘要選択ボタンのコンテキストメニュー
		public List<MenuItem> ContextMenuButtonSelectOpEdItems { get; set; }

		// 摘要
		private String mOpEd;
		public String OpEd
		{
			get => mOpEd;
			set => RaisePropertyChangedIfSet(ref mOpEd, value);
		}

		// カテゴリーチェックボックスの有効無効
		private Boolean mIsCategoryEnabled;
		public Boolean IsCategoryEnabled
		{
			get => mIsCategoryEnabled;
			set
			{
				if (RaisePropertyChangedIfSet(ref mIsCategoryEnabled, value))
				{
					SetIsTieUpEnabled();
				}
			}
		}

		// 歌手あり
		private Boolean mHasArtist;
		public Boolean HasArtist
		{
			get => mHasArtist;
			set
			{
				if (RaisePropertyChangedIfSet(ref mHasArtist, value))
				{
					ButtonSearchArtistClickedCommand.RaiseCanExecuteChanged();
					ButtonEditArtistClickedCommand.RaiseCanExecuteChanged();
					if (!mHasArtist)
					{
						ArtistId = null;
						ArtistName = null;
					}
				}
			}
		}

		// 歌手名
		private String mArtistName;
		public String ArtistName
		{
			get => mArtistName;
			set => RaisePropertyChangedIfSet(ref mArtistName, value);
		}

		// 作詞者あり
		private Boolean mHasLyrist;
		public Boolean HasLyrist
		{
			get => mHasLyrist;
			set
			{
				if (RaisePropertyChangedIfSet(ref mHasLyrist, value))
				{
					ButtonSearchLyristClickedCommand.RaiseCanExecuteChanged();
					ButtonSameLyristClickedCommand.RaiseCanExecuteChanged();
					ButtonEditLyristClickedCommand.RaiseCanExecuteChanged();
					if (!mHasLyrist)
					{
						LyristId = null;
						LyristName = null;
					}
				}
			}
		}

		// 作詞者名
		private String mLyristName;
		public String LyristName
		{
			get => mLyristName;
			set => RaisePropertyChangedIfSet(ref mLyristName, value);
		}

		// 作曲者あり
		private Boolean mHasComposer;
		public Boolean HasComposer
		{
			get => mHasComposer;
			set
			{
				if (RaisePropertyChangedIfSet(ref mHasComposer, value))
				{
					ButtonSearchComposerClickedCommand.RaiseCanExecuteChanged();
					ButtonSameComposerClickedCommand.RaiseCanExecuteChanged();
					ButtonEditComposerClickedCommand.RaiseCanExecuteChanged();
					if (!mHasComposer)
					{
						ComposerId = null;
						ComposerName = null;
					}
				}
			}
		}

		// 作曲者名
		private String mComposerName;
		public String ComposerName
		{
			get => mComposerName;
			set => RaisePropertyChangedIfSet(ref mComposerName, value);
		}

		// 編曲者あり
		private Boolean mHasArranger;
		public Boolean HasArranger
		{
			get => mHasArranger;
			set
			{
				if (RaisePropertyChangedIfSet(ref mHasArranger, value))
				{
					ButtonSearchArrangerClickedCommand.RaiseCanExecuteChanged();
					ButtonSameArrangerClickedCommand.RaiseCanExecuteChanged();
					ButtonEditArrangerClickedCommand.RaiseCanExecuteChanged();
					if (!mHasArranger)
					{
						ArrangerId = null;
						ArrangerName = null;
					}
				}
			}
		}

		// 編曲者名
		private String mArrangerName;
		public String ArrangerName
		{
			get => mArrangerName;
			set => RaisePropertyChangedIfSet(ref mArrangerName, value);
		}

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// タイアップ ID
		public String TieUpId { get; set; }

		// 歌手 ID
		public String ArtistId { get; set; }

		// 作詞者 ID
		public String LyristId { get; set; }

		// 作曲者 ID
		public String ComposerId { get; set; }

		// 編曲者 ID
		public String ArrangerId { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region タイアップ検索ボタンの制御
		private ViewModelCommand mButtonSearchTieUpClickedCommand;

		public ViewModelCommand ButtonSearchTieUpClickedCommand
		{
			get
			{
				if (mButtonSearchTieUpClickedCommand == null)
				{
					mButtonSearchTieUpClickedCommand = new ViewModelCommand(ButtonSearchTieUpClicked, CanButtonSearchTieUpClicked);
				}
				return mButtonSearchTieUpClickedCommand;
			}
		}

		public Boolean CanButtonSearchTieUpClicked()
		{
			return HasTieUp;
		}

		public void ButtonSearchTieUpClicked()
		{
			try
			{
				using (SearchMusicInfoWindowViewModel aSearchMusicInfoWindowViewModel = new SearchMusicInfoWindowViewModel())
				{
					aSearchMusicInfoWindowViewModel.Environment = Environment;
					aSearchMusicInfoWindowViewModel.ItemName = "タイアップ";
					aSearchMusicInfoWindowViewModel.TableIndex = MusicInfoDbTables.TTieUp;
					aSearchMusicInfoWindowViewModel.SelectedKeyword = OriginalTieUpName();
					Messenger.Raise(new TransitionMessage(aSearchMusicInfoWindowViewModel, "OpenSearchMusicInfoWindow"));
					mIsTieUpSearched = true;
					if (String.IsNullOrEmpty(aSearchMusicInfoWindowViewModel.DecidedName))
					{
						return;
					}

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					{
						List<TTieUp> aMasters = YlCommon.SelectMastersByName<TTieUp>(aMusicInfoDbInDisk.Connection, aSearchMusicInfoWindowViewModel.DecidedName);
						if (aMasters.Count > 0)
						{
							aMasters[0].Environment = Environment;
							aMasters[0].AvoidSameName = aMasters.Count > 1;
							TieUpId = aMasters[0].Id;
							TieUpName = aMasters[0].DisplayName;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region タイアップ詳細編集ボタンの制御
		private ViewModelCommand mButtonEditTieUpClickedCommand;

		public ViewModelCommand ButtonEditTieUpClickedCommand
		{
			get
			{
				if (mButtonEditTieUpClickedCommand == null)
				{
					mButtonEditTieUpClickedCommand = new ViewModelCommand(ButtonEditTieUpClicked, CanButtonEditTieUpClicked);
				}
				return mButtonEditTieUpClickedCommand;
			}
		}

		public Boolean CanButtonEditTieUpClicked()
		{
			return HasTieUp;
		}

		public void ButtonEditTieUpClicked()
		{
			try
			{
				if (String.IsNullOrEmpty(TieUpId))
				{
					if (!mIsTieUpSearched)
					{
						throw new Exception("タイアップが選択されていないため新規タイアップ情報作成となりますが、その前に一度、目的のタイアップが未登録かどうか検索して下さい。");
					}

					if (MessageBox.Show("タイアップが選択されていません。\n新規にタイアップ情報を作成しますか？\n"
							+ "（目的のタイアップが未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
							MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
					{
						return;
					}
				}

				// 既存レコードを用意
				List<TTieUp> aMasters;
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					aMasters = YlCommon.SelectMastersByName<TTieUp>(aMusicInfoDbInDisk.Connection, OriginalTieUpName());
				}

				// 新規作成用を追加
				TTieUp aNewRecord = new TTieUp
				{
					// IRcBase
					Id = null,
					Import = false,
					Invalid = false,
					UpdateTime = YlConstants.INVALID_MJD,
					Dirty = true,

					// IRcMaster
					Name = OriginalTieUpName(),
					Ruby = null,
					Keyword = null,
				};
				aMasters.Insert(0, aNewRecord);

				using (EditTieUpWindowViewModel aEditTieUpWindowViewModel = new EditTieUpWindowViewModel())
				{
					aEditTieUpWindowViewModel.Environment = Environment;
					aEditTieUpWindowViewModel.SetMasters(aMasters);
					aEditTieUpWindowViewModel.DefaultId = TieUpId;
					Messenger.Raise(new TransitionMessage(aEditTieUpWindowViewModel, "OpenEditTieUpWindow"));

					if (String.IsNullOrEmpty(aEditTieUpWindowViewModel.OkSelectedId))
					{
						return;
					}

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
					{
						TTieUp aMaster = YlCommon.SelectMasterById<TTieUp>(aContext, aEditTieUpWindowViewModel.OkSelectedId);
						if (aMaster != null)
						{
							List<TTieUp> aSameNameTieUps = YlCommon.SelectMastersByName<TTieUp>(aContext, aMaster.Name);
							aMaster.Environment = Environment;
							aMaster.AvoidSameName = aSameNameTieUps.Count > 1;
							TieUpId = aMaster.Id;
							TieUpName = aMaster.DisplayName;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}

		}
		#endregion

		#region 摘要選択ボタンの制御
		private ViewModelCommand mButtonSelectOpEdClickedCommand;

		public ViewModelCommand ButtonSelectOpEdClickedCommand
		{
			get
			{
				if (mButtonSelectOpEdClickedCommand == null)
				{
					mButtonSelectOpEdClickedCommand = new ViewModelCommand(ButtonSelectOpEdClicked);
				}
				return mButtonSelectOpEdClickedCommand;
			}
		}

		public void ButtonSelectOpEdClicked()
		{

		}
		#endregion

		#region 歌手検索ボタンの制御
		private ViewModelCommand mButtonSearchArtistClickedCommand;

		public ViewModelCommand ButtonSearchArtistClickedCommand
		{
			get
			{
				if (mButtonSearchArtistClickedCommand == null)
				{
					mButtonSearchArtistClickedCommand = new ViewModelCommand(ButtonSearchArtistClicked, CanButtonSearchArtistClicked);
				}
				return mButtonSearchArtistClickedCommand;
			}
		}

		public Boolean CanButtonSearchArtistClicked()
		{
			return HasArtist;
		}

		public void ButtonSearchArtistClicked()
		{
			try
			{
				String aId = ArtistId;
				String aName = ArtistName;
				SearchPerson("歌手", ref aId, ref aName);
				ArtistId = aId;
				ArtistName = aName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "歌手検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 歌手詳細編集ボタンの制御
		private ViewModelCommand mButtonEditArtistClickedCommand;

		public ViewModelCommand ButtonEditArtistClickedCommand
		{
			get
			{
				if (mButtonEditArtistClickedCommand == null)
				{
					mButtonEditArtistClickedCommand = new ViewModelCommand(ButtonEditArtistClicked, CanButtonEditArtistClicked);
				}
				return mButtonEditArtistClickedCommand;
			}
		}

		public Boolean CanButtonEditArtistClicked()
		{
			return HasArtist;
		}

		public void ButtonEditArtistClicked()
		{
			try
			{
				Boolean aHas = HasArtist;
				String aId = ArtistId;
				String aName = ArtistName;
				EditPeople("歌手", ref aHas, ref aId, ref aName);
				HasArtist = aHas;
				ArtistId = aId;
				ArtistName = aName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "歌手詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 作詞者検索ボタンの制御
		private ViewModelCommand mButtonSearchLyristClickedCommand;

		public ViewModelCommand ButtonSearchLyristClickedCommand
		{
			get
			{
				if (mButtonSearchLyristClickedCommand == null)
				{
					mButtonSearchLyristClickedCommand = new ViewModelCommand(ButtonSearchLyristClicked, CanButtonSearchLyristClicked);
				}
				return mButtonSearchLyristClickedCommand;
			}
		}

		public Boolean CanButtonSearchLyristClicked()
		{
			return HasLyrist;
		}

		public void ButtonSearchLyristClicked()
		{
			try
			{
				String aId = LyristId;
				String aName = LyristName;
				SearchPerson("作詞者", ref aId, ref aName);
				LyristId = aId;
				LyristName = aName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "作詞者検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 作詞者同上ボタンの制御
		private ViewModelCommand mButtonSameLyristClickedCommand;

		public ViewModelCommand ButtonSameLyristClickedCommand
		{
			get
			{
				if (mButtonSameLyristClickedCommand == null)
				{
					mButtonSameLyristClickedCommand = new ViewModelCommand(ButtonSameLyristClicked, CanButtonSameLyristClicked);
				}
				return mButtonSameLyristClickedCommand;
			}
		}

		public Boolean CanButtonSameLyristClicked()
		{
			return HasLyrist;
		}

		public void ButtonSameLyristClicked()
		{
			try
			{
				LyristId = ArtistId;
				LyristName = ArtistName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "作詞者同上ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 作詞者詳細編集ボタンの制御
		private ViewModelCommand mButtonEditLyristClickedCommand;

		public ViewModelCommand ButtonEditLyristClickedCommand
		{
			get
			{
				if (mButtonEditLyristClickedCommand == null)
				{
					mButtonEditLyristClickedCommand = new ViewModelCommand(ButtonEditLyristClicked, CanButtonEditLyristClicked);
				}
				return mButtonEditLyristClickedCommand;
			}
		}

		public Boolean CanButtonEditLyristClicked()
		{
			return HasLyrist;
		}

		public void ButtonEditLyristClicked()
		{
			try
			{
				Boolean aHas = HasLyrist;
				String aId = LyristId;
				String aName = LyristName;
				EditPeople("作詞者", ref aHas, ref aId, ref aName);
				HasLyrist = aHas;
				LyristId = aId;
				LyristName = aName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "作詞者詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 作曲者検索ボタンの制御
		private ViewModelCommand mButtonSearchComposerClickedCommand;

		public ViewModelCommand ButtonSearchComposerClickedCommand
		{
			get
			{
				if (mButtonSearchComposerClickedCommand == null)
				{
					mButtonSearchComposerClickedCommand = new ViewModelCommand(ButtonSearchComposerClicked, CanButtonSearchComposerClicked);
				}
				return mButtonSearchComposerClickedCommand;
			}
		}

		public Boolean CanButtonSearchComposerClicked()
		{
			return HasComposer;
		}

		public void ButtonSearchComposerClicked()
		{
			try
			{
				String aId = ComposerId;
				String aName = ComposerName;
				SearchPerson("作曲者", ref aId, ref aName);
				ComposerId = aId;
				ComposerName = aName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "作曲者検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 作曲者同上ボタンの制御
		private ViewModelCommand mButtonSameComposerClickedCommand;

		public ViewModelCommand ButtonSameComposerClickedCommand
		{
			get
			{
				if (mButtonSameComposerClickedCommand == null)
				{
					mButtonSameComposerClickedCommand = new ViewModelCommand(ButtonSameComposerClicked, CanButtonSameComposerClicked);
				}
				return mButtonSameComposerClickedCommand;
			}
		}

		public Boolean CanButtonSameComposerClicked()
		{
			return HasComposer;
		}

		public void ButtonSameComposerClicked()
		{
			try
			{
				ComposerId = LyristId;
				ComposerName = LyristName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "作曲者同上ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 作曲者詳細編集ボタンの制御
		private ViewModelCommand mButtonEditComposerClickedCommand;

		public ViewModelCommand ButtonEditComposerClickedCommand
		{
			get
			{
				if (mButtonEditComposerClickedCommand == null)
				{
					mButtonEditComposerClickedCommand = new ViewModelCommand(ButtonEditComposerClicked, CanButtonEditComposerClicked);
				}
				return mButtonEditComposerClickedCommand;
			}
		}

		public Boolean CanButtonEditComposerClicked()
		{
			return HasComposer;
		}

		public void ButtonEditComposerClicked()
		{
			try
			{
				Boolean aHas = HasComposer;
				String aId = ComposerId;
				String aName = ComposerName;
				EditPeople("作曲者", ref aHas, ref aId, ref aName);
				HasComposer = aHas;
				ComposerId = aId;
				ComposerName = aName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "作曲者詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 編曲者検索ボタンの制御
		private ViewModelCommand mButtonSearchArrangerClickedCommand;

		public ViewModelCommand ButtonSearchArrangerClickedCommand
		{
			get
			{
				if (mButtonSearchArrangerClickedCommand == null)
				{
					mButtonSearchArrangerClickedCommand = new ViewModelCommand(ButtonSearchArrangerClicked, CanButtonSearchArrangerClicked);
				}
				return mButtonSearchArrangerClickedCommand;
			}
		}

		public Boolean CanButtonSearchArrangerClicked()
		{
			return HasArranger;
		}

		public void ButtonSearchArrangerClicked()
		{
			try
			{
				String aId = ArrangerId;
				String aName = ArrangerName;
				SearchPerson("編曲者", ref aId, ref aName);
				ArrangerId = aId;
				ArrangerName = aName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "編曲者検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 編曲者同上ボタンの制御
		private ViewModelCommand mButtonSameArrangerClickedCommand;

		public ViewModelCommand ButtonSameArrangerClickedCommand
		{
			get
			{
				if (mButtonSameArrangerClickedCommand == null)
				{
					mButtonSameArrangerClickedCommand = new ViewModelCommand(ButtonSameArrangerClicked, CanButtonSameArrangerClicked);
				}
				return mButtonSameArrangerClickedCommand;
			}
		}

		public Boolean CanButtonSameArrangerClicked()
		{
			return HasArranger;
		}

		public void ButtonSameArrangerClicked()
		{
			try
			{
				ArrangerId = ComposerId;
				ArrangerName = ComposerName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "編曲者同上ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 編曲者詳細編集ボタンの制御
		private ViewModelCommand mButtonEditArrangerClickedCommand;

		public ViewModelCommand ButtonEditArrangerClickedCommand
		{
			get
			{
				if (mButtonEditArrangerClickedCommand == null)
				{
					mButtonEditArrangerClickedCommand = new ViewModelCommand(ButtonEditArrangerClicked, CanButtonEditArrangerClicked);
				}
				return mButtonEditArrangerClickedCommand;
			}
		}

		public Boolean CanButtonEditArrangerClicked()
		{
			return HasArranger;
		}

		public void ButtonEditArrangerClicked()
		{
			try
			{
				Boolean aHas = HasArranger;
				String aId = ArrangerId;
				String aName = ArrangerName;
				EditPeople("編曲者", ref aHas, ref aId, ref aName);
				HasArranger = aHas;
				ArrangerId = aId;
				ArrangerName = aName;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "編曲者詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// 本関数を呼ぶ前に Environment を設定しておく必要がある
		// --------------------------------------------------------------------
		public override void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				// 基底クラス初期化前の初期化
				mCaption = "楽曲";

				// 基底クラス初期化
				base.Initialize();

				// タイトルバー
				Title = "楽曲詳細情報の編集";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif
				// 摘要ボタンのコンテキストメニュー
				ContextMenuButtonSelectOpEdItems = new List<MenuItem>();
				YlCommon.AddContextMenuItem(ContextMenuButtonSelectOpEdItems, "OP（オープニング）", ContextMenuButtonSelectOpEdItem_Click);
				YlCommon.AddContextMenuItem(ContextMenuButtonSelectOpEdItems, "ED（エンディング）", ContextMenuButtonSelectOpEdItem_Click);
				YlCommon.AddContextMenuItem(ContextMenuButtonSelectOpEdItems, "IN（挿入歌）", ContextMenuButtonSelectOpEdItem_Click);
				YlCommon.AddContextMenuItem(ContextMenuButtonSelectOpEdItems, "IM（イメージソング）", ContextMenuButtonSelectOpEdItem_Click);
				YlCommon.AddContextMenuItem(ContextMenuButtonSelectOpEdItems, "CH（キャラクターソング）", ContextMenuButtonSelectOpEdItem_Click);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "楽曲詳細情報編集ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力値を確認する
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		protected override void CheckInput(IRcMaster oInitialMaster)
		{
			base.CheckInput(oInitialMaster);

			// チェックされているのに指定されていない項目を確認
			if (HasTieUp && String.IsNullOrEmpty(TieUpId))
			{
				throw new Exception("タイアップが「あり」になっていますが指定されていません。");
			}
			List<String> aArtistIds = YlCommon.SplitIds(ArtistId);
			if (HasArtist && aArtistIds.Count == 0)
			{
				throw new Exception("歌手が「あり」になっていますが指定されていません。");
			}
			List<String> aLyristIds = YlCommon.SplitIds(LyristId);
			if (HasLyrist && aLyristIds.Count == 0)
			{
				throw new Exception("作詞者が「あり」になっていますが指定されていません。");
			}
			List<String> aComposerIds = YlCommon.SplitIds(ComposerId);
			if (HasComposer && aComposerIds.Count == 0)
			{
				throw new Exception("作曲者が「あり」になっていますが指定されていません。");
			}
			List<String> aArrangerIds = YlCommon.SplitIds(ArrangerId);
			if (HasArranger && aArrangerIds.Count == 0)
			{
				throw new Exception("編曲者が「あり」になっていますが指定されていません。");
			}
		}

		// --------------------------------------------------------------------
		// HasCategory が変更された
		// --------------------------------------------------------------------
		protected override void HasCategoryChanged()
		{
			SetIsTieUpEnabled();
		}

		// --------------------------------------------------------------------
		// プロパティーの内容を Master に格納
		// --------------------------------------------------------------------
		protected override void PropertiesToRecord(IRcMaster oMaster)
		{
			base.PropertiesToRecord(oMaster);

			TSong aSong = (TSong)oMaster;

			// TSong
			aSong.TieUpId = TieUpId;
			aSong.OpEd = OpEd;
		}

		// --------------------------------------------------------------------
		// TSong の内容をプロパティーに反映
		// --------------------------------------------------------------------
		protected override void RecordToProperties(IRcMaster oMaster)
		{
			base.RecordToProperties(oMaster);

			TSong aSong = (TSong)oMaster;

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				// タイアップ関係
				if (String.IsNullOrEmpty(aSong.TieUpId))
				{
					HasTieUp = false;
				}
				else
				{
					HasTieUp = true;
					TTieUp aTieUp = YlCommon.SelectMasterById<TTieUp>(aContext, aSong.TieUpId);
					if (aTieUp != null)
					{
						List<TTieUp> aSameNameTieUps = YlCommon.SelectMastersByName<TTieUp>(aContext, aTieUp.Name);
						aTieUp.Environment = Environment;
						aTieUp.AvoidSameName = aSameNameTieUps.Count > 1;
						TieUpId = aTieUp.Id;
						TieUpName = aTieUp.DisplayName;
					}
					else
					{
						TieUpId = null;
						TieUpName = null;
					}
				}

				// 概要
				OpEd = aSong.OpEd;

				// 人物関係
				Boolean aHasPerson;
				String aPersonId;
				String aPersonName;

				GetPeopleProperties(YlCommon.SelectSequencePeopleBySongId<TArtistSequence>(aContext, aSong.Id), out aHasPerson, out aPersonId, out aPersonName);
				HasArtist = aHasPerson;
				ArtistId = aPersonId;
				ArtistName = aPersonName;

				GetPeopleProperties(YlCommon.SelectSequencePeopleBySongId<TLyristSequence>(aContext, aSong.Id), out aHasPerson, out aPersonId, out aPersonName);
				HasLyrist = aHasPerson;
				LyristId = aPersonId;
				LyristName = aPersonName;

				GetPeopleProperties(YlCommon.SelectSequencePeopleBySongId<TComposerSequence>(aContext, aSong.Id), out aHasPerson, out aPersonId, out aPersonName);
				HasComposer = aHasPerson;
				ComposerId = aPersonId;
				ComposerName = aPersonName;

				GetPeopleProperties(YlCommon.SelectSequencePeopleBySongId<TArrangerSequence>(aContext, aSong.Id), out aHasPerson, out aPersonId, out aPersonName);
				HasArranger = aHasPerson;
				ArrangerId = aPersonId;
				ArrangerName = aPersonName;
			}

			SetIsTieUpEnabled();
			SetIsCategoryEnabled();
		}

		// --------------------------------------------------------------------
		// レコード保存
		// --------------------------------------------------------------------
		protected override String Save()
		{
			TSong aNewRecord = new TSong();
			PropertiesToRecord(aNewRecord);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				if (aNewRecord.Id == NewIdForDisplay())
				{
					// 新規登録
					YlCommon.InputIdPrefixIfNeededWithInvoke(this, Environment);
					aNewRecord.Id = Environment.YukaListerSettings.PrepareLastId(aMusicInfoDbInDisk.Connection, MusicInfoDbTables.TSong);
					Table<TSong> aTable = aContext.GetTable<TSong>();
					aTable.InsertOnSubmit(aNewRecord);
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲テーブル新規登録：" + aNewRecord.Id + " / " + aNewRecord.Name);
				}
				else
				{
					TSong aExistRecord = YlCommon.SelectMasterById<TSong>(aContext, aNewRecord.Id, true);
					if (YlCommon.IsRcSongUpdated(aExistRecord, aNewRecord))
					{
						// 更新（既存のレコードが無効化されている場合は有効化も行う）
						aNewRecord.UpdateTime = aExistRecord.UpdateTime;
						Common.ShallowCopy(aNewRecord, aExistRecord);
						Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲テーブル更新：" + aNewRecord.Id + " / " + aNewRecord.Name);
					}
				}

				// 人物紐付け
				YlCommon.RegisterSequence<TArtistSequence>(aContext, aNewRecord.Id, YlCommon.SplitIds(ArtistId));
				YlCommon.RegisterSequence<TLyristSequence>(aContext, aNewRecord.Id, YlCommon.SplitIds(LyristId));
				YlCommon.RegisterSequence<TComposerSequence>(aContext, aNewRecord.Id, YlCommon.SplitIds(ComposerId));
				YlCommon.RegisterSequence<TArrangerSequence>(aContext, aNewRecord.Id, YlCommon.SplitIds(ArrangerId));

				aContext.SubmitChanges();
			}

			return aNewRecord.Id;
		}

		// --------------------------------------------------------------------
		// 同名検索用関数
		// --------------------------------------------------------------------
		protected override List<IRcMaster> SelectMastersByName(SQLiteConnection oConnection, String oName)
		{
			return YlCommon.SelectMastersByName<TSong>(oConnection, oName).ToList<IRcMaster>();
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// タイアップを検索したかどうか
		private Boolean mIsTieUpSearched = false;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ContextMenuButtonSelectOpEdItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				MenuItem aItem = (MenuItem)oSender;
				String aItemText = (String)aItem.Header;
				Int32 aPos = aItemText.IndexOf("（");
				if (aPos >= 0)
				{
					OpEd = aItemText.Substring(0, aPos);
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "摘要選択メニュークリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 人物詳細編集
		// --------------------------------------------------------------------
		private void EditPeople(String oCaption, ref Boolean oHas, ref String oId, ref String oName)
		{
			using (EditPeopleWindowViewModel aEditPeopleWindowViewModel = new EditPeopleWindowViewModel())
			{
				aEditPeopleWindowViewModel.Environment = Environment;
				aEditPeopleWindowViewModel.Caption = oCaption;
				aEditPeopleWindowViewModel.InitialIds = YlCommon.SplitIds(oId);
				Messenger.Raise(new TransitionMessage(aEditPeopleWindowViewModel, "OpenEditPeopleWindow"));

				if (aEditPeopleWindowViewModel.OkSelectedPeople == null)
				{
					return;
				}

				GetPeopleProperties(aEditPeopleWindowViewModel.OkSelectedPeople, out oHas, out oId, out oName);
			}
		}

		// --------------------------------------------------------------------
		// 人物プロパティーの値を取得
		// --------------------------------------------------------------------
		private void GetPeopleProperties(List<TPerson> oPeople, out Boolean oHas, out String oId, out String oName)
		{
			oId = null;
			oName = null;
			if (oPeople.Count == 0)
			{
				oHas = false;
			}
			else
			{
				oHas = true;
				for (Int32 i = 0; i < oPeople.Count; i++)
				{
					if (i == 0)
					{
						oId = oPeople[i].Id;
						oName = oPeople[i].Name;
					}
					else
					{
						oId += "," + oPeople[i].Id;
						oName += "," + oPeople[i].Name;
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// TieUpName は同名識別用に変更されている場合があるので TieUpId から正式名称を取得する
		// --------------------------------------------------------------------
		private String OriginalTieUpName()
		{
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			{
				TTieUp aTieUp = YlCommon.SelectMasterById<TTieUp>(aMusicInfoDbInDisk.Connection, TieUpId);
				return aTieUp?.Name;
			}
		}

		// --------------------------------------------------------------------
		// 人物を検索して結果を取得
		// --------------------------------------------------------------------
		private void SearchPerson(String oCaption, ref String oId, ref String oName)
		{
			using (SearchMusicInfoWindowViewModel aSearchMusicInfoWindowViewModel = new SearchMusicInfoWindowViewModel())
			{
				// 人物が複数指定されている場合は先頭のみで検索
				String aKeyword = oName;
				if (!String.IsNullOrEmpty(aKeyword))
				{
					Int32 aPos = aKeyword.IndexOf(',');
					if (aPos > 0)
					{
						aKeyword = aKeyword.Substring(0, aPos);
					}
				}

				aSearchMusicInfoWindowViewModel.Environment = Environment;
				aSearchMusicInfoWindowViewModel.ItemName = oCaption;
				aSearchMusicInfoWindowViewModel.TableIndex = MusicInfoDbTables.TPerson;
				aSearchMusicInfoWindowViewModel.SelectedKeyword = aKeyword;
				Messenger.Raise(new TransitionMessage(aSearchMusicInfoWindowViewModel, "OpenSearchMusicInfoWindow"));
				if (String.IsNullOrEmpty(aSearchMusicInfoWindowViewModel.DecidedName))
				{
					return;
				}

				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					List<TPerson> aMasters = YlCommon.SelectMastersByName<TPerson>(aMusicInfoDbInDisk.Connection, aSearchMusicInfoWindowViewModel.DecidedName);
					if (aMasters.Count > 0)
					{
						oId = aMasters[0].Id;
						oName = aMasters[0].Name;
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// IsCategoryEnabled の設定
		// --------------------------------------------------------------------
		private void SetIsCategoryEnabled()
		{
			IsCategoryEnabled = !(IsTieUpEnabled && HasTieUp);
		}

		// --------------------------------------------------------------------
		// IsTieUpEnabled の設定
		// --------------------------------------------------------------------
		private void SetIsTieUpEnabled()
		{
			IsTieUpEnabled = !(IsCategoryEnabled && HasCategory);
		}


	}
	// public class EditSongWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
