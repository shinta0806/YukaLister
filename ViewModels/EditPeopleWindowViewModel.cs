// ============================================================================
// 
// 複数人物編集ウィンドウの ViewModel
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
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Windows;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class EditPeopleWindowViewModel : ViewModel
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

		// 説明
		private String mDescription;
		public String Description
		{
			get => mDescription;
			set => RaisePropertyChangedIfSet(ref mDescription, value);
		}

		// 編集中の人物群
		public ObservableCollection<TPerson> People { get; set; } = new ObservableCollection<TPerson>();

		// 選択された人物
		private TPerson mSelectedPerson;
		public TPerson SelectedPerson
		{
			get => mSelectedPerson;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedPerson, value))
				{
					ButtonRemoveClickedCommand.RaiseCanExecuteChanged();
					ButtonUpClickedCommand.RaiseCanExecuteChanged();
					ButtonDownClickedCommand.RaiseCanExecuteChanged();
					ButtonEditClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}


		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// 人物区分
		public String Caption { get; set; }

		// 初期人物 ID 群
		public List<String> InitialIds { get; set; }

		// OK ボタンが押された時の人物群
		public List<TPerson> OkSelectedPeople { get; set; }

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
				Edit();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "データグリッドダブルクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 検索して追加ボタンの制御
		private ViewModelCommand mButtonAddClickedCommand;

		public ViewModelCommand ButtonAddClickedCommand
		{
			get
			{
				if (mButtonAddClickedCommand == null)
				{
					mButtonAddClickedCommand = new ViewModelCommand(ButtonAddClicked);
				}
				return mButtonAddClickedCommand;
			}
		}

		public void ButtonAddClicked()
		{
			try
			{
				using (SearchMusicInfoWindowViewModel aSearchMusicInfoWindowViewModel = new SearchMusicInfoWindowViewModel())
				{
					aSearchMusicInfoWindowViewModel.Environment = Environment;
					aSearchMusicInfoWindowViewModel.ItemName = Caption;
					aSearchMusicInfoWindowViewModel.TableIndex = MusicInfoDbTables.TPerson;
					aSearchMusicInfoWindowViewModel.SelectedKeyword = null;
					Messenger.Raise(new TransitionMessage(aSearchMusicInfoWindowViewModel, "OpenSearchMusicInfoWindow"));
					mIsPersonSearched = true;
					if (String.IsNullOrEmpty(aSearchMusicInfoWindowViewModel.DecidedName))
					{
						return;
					}

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					{
						List<TPerson> aMasters = YlCommon.SelectMastersByName<TPerson>(aMusicInfoDbInDisk.Connection, aSearchMusicInfoWindowViewModel.DecidedName);
						if (aMasters.Count == 0)
						{
							throw new Exception(aSearchMusicInfoWindowViewModel.DecidedName + "がデータベースに登録されていません。");
						}
						if (PeopleIndexOfById(aMasters[0]) >= 0)
						{
							throw new Exception(aSearchMusicInfoWindowViewModel.DecidedName + "は既に追加されています。");
						}
						aMasters[0].Environment = Environment;
						aMasters[0].AvoidSameName = aMasters.Count > 1;
						People.Add(aMasters[0]);
						SelectedPerson = aMasters[0];
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "追加ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 削除ボタンの制御
		private ViewModelCommand mButtonRemoveClickedCommand;

		public ViewModelCommand ButtonRemoveClickedCommand
		{
			get
			{
				if (mButtonRemoveClickedCommand == null)
				{
					mButtonRemoveClickedCommand = new ViewModelCommand(ButtonRemoveClicked, CanButtonRemoveClicked);
				}
				return mButtonRemoveClickedCommand;
			}
		}

		public Boolean CanButtonRemoveClicked()
		{
			return SelectedPerson != null;
		}

		public void ButtonRemoveClicked()
		{
			try
			{
				People.Remove(SelectedPerson);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "削除ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 上へボタンの制御
		private ViewModelCommand mButtonUpClickedCommand;

		public ViewModelCommand ButtonUpClickedCommand
		{
			get
			{
				if (mButtonUpClickedCommand == null)
				{
					mButtonUpClickedCommand = new ViewModelCommand(ButtonUpClicked, CanButtonUpClicked);
				}
				return mButtonUpClickedCommand;
			}
		}

		public Boolean CanButtonUpClicked()
		{
			Int32 aIndex = People.IndexOf(SelectedPerson);
			return aIndex >= 1;
		}

		public void ButtonUpClicked()
		{
			try
			{
				Int32 aSelectedIndex = People.IndexOf(SelectedPerson);
				if (aSelectedIndex < 1)
				{
					return;
				}
				TPerson aItem = SelectedPerson;
				People.Remove(aItem);
				People.Insert(aSelectedIndex - 1, aItem);
				SelectedPerson = aItem;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "上へボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 下へボタンの制御
		private ViewModelCommand mButtonDownClickedCommand;

		public ViewModelCommand ButtonDownClickedCommand
		{
			get
			{
				if (mButtonDownClickedCommand == null)
				{
					mButtonDownClickedCommand = new ViewModelCommand(ButtonDownClicked, CanButtonDownClicked);
				}
				return mButtonDownClickedCommand;
			}
		}

		public Boolean CanButtonDownClicked()
		{
			Int32 aIndex = People.IndexOf(SelectedPerson);
			return 0 <= aIndex && aIndex < People.Count - 1;
		}

		public void ButtonDownClicked()
		{
			try
			{
				Int32 aSelectedIndex = People.IndexOf(SelectedPerson);
				if (aSelectedIndex < 0 || aSelectedIndex >= People.Count - 1)
				{
					return;
				}
				TPerson aItem = SelectedPerson;
				People.Remove(aItem);
				People.Insert(aSelectedIndex + 1, aItem);
				SelectedPerson = aItem;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "下へボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 人物詳細編集ボタンの制御
		private ViewModelCommand mButtonEditClickedCommand;

		public ViewModelCommand ButtonEditClickedCommand
		{
			get
			{
				if (mButtonEditClickedCommand == null)
				{
					mButtonEditClickedCommand = new ViewModelCommand(ButtonEditClicked, CanButtonEditClicked);
				}
				return mButtonEditClickedCommand;
			}
		}

		public Boolean CanButtonEditClicked()
		{
			return SelectedPerson != null;
		}

		public void ButtonEditClicked()
		{
			try
			{
				Edit();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 新規人物作成ボタンの制御
		private ViewModelCommand mButtonNewClickedCommand;

		public ViewModelCommand ButtonNewClickedCommand
		{
			get
			{
				if (mButtonNewClickedCommand == null)
				{
					mButtonNewClickedCommand = new ViewModelCommand(ButtonNewClicked);
				}
				return mButtonNewClickedCommand;
			}
		}

		public void ButtonNewClicked()
		{
			try
			{
				if (!mIsPersonSearched)
				{
					throw new Exception("新規人物作成の前に一度、目的の人物が未登録かどうか検索して下さい。");
				}

				if (MessageBox.Show("目的の人物が未登録の場合（検索してもヒットしない場合）に限り、新規人物作成を行って下さい。\n"
						+ "新規人物作成を行いますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
				{
					return;
				}

				// 新規人物
				List<TPerson> aMasters = new List<TPerson>();
				TPerson aNewRecord = new TPerson
				{
					// IRcBase
					Id = null,
					Import = false,
					Invalid = false,
					UpdateTime = YlConstants.INVALID_MJD,
					Dirty = true,

					// IRcMaster
					Name = null,
					Ruby = null,
					Keyword = null,
				};
				aMasters.Insert(0, aNewRecord);

				using (EditPersonWindowViewModel aEditPersonWindowViewModel = new EditPersonWindowViewModel())
				{
					aEditPersonWindowViewModel.Environment = Environment;
					aEditPersonWindowViewModel.SetMasters(aMasters);
					aEditPersonWindowViewModel.DefaultId = null;
					Messenger.Raise(new TransitionMessage(aEditPersonWindowViewModel, "OpenEditPersonWindow"));

					if (String.IsNullOrEmpty(aEditPersonWindowViewModel.OkSelectedId))
					{
						return;
					}

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
					{
						TPerson aMaster = YlCommon.SelectBaseById<TPerson>(aContext, aEditPersonWindowViewModel.OkSelectedId);
						if (aMaster != null)
						{
							List<TPerson> aSameNamePeople = YlCommon.SelectMastersByName<TPerson>(aContext, aMaster.Name);
							aMaster.Environment = Environment;
							aMaster.AvoidSameName = aSameNamePeople.Count > 1;
							People.Add(aMaster);
							SelectedPerson = aMaster;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "新規人物作成ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region OK ボタンの制御
		private ViewModelCommand mButtonOkClickedCommand;

		public ViewModelCommand ButtonOkClickedCommand
		{
			get
			{
				if (mButtonOkClickedCommand == null)
				{
					mButtonOkClickedCommand = new ViewModelCommand(ButtonOkClicked);
				}
				return mButtonOkClickedCommand;
			}
		}

		public void ButtonOkClicked()
		{
			try
			{
				OkSelectedPeople = People.ToList();
				Messenger.Raise(new WindowActionMessage("Close"));
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
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
		public void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				// タイトルバー
				Title = Caption + "の編集";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				// 説明
				Description = "「検索して追加」ボタンで" + Caption + "を追加して下さい。複数名の指定も可能です。";

				// 人物群
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					for (Int32 i = 0; i < InitialIds.Count; i++)
					{
						TPerson aPerson = YlCommon.SelectBaseById<TPerson>(aContext, InitialIds[i]);
						if (aPerson != null)
						{
							List<TPerson> aSameNamePeople = YlCommon.SelectMastersByName<TPerson>(aContext, aPerson.Name);
							aPerson.Environment = Environment;
							aPerson.AvoidSameName = aSameNamePeople.Count > 1;
							People.Add(aPerson);
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "複数人物編集ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 検索したかどうか
		private Boolean mIsPersonSearched = false;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 人物を編集
		// --------------------------------------------------------------------
		private void Edit()
		{
			if (SelectedPerson == null)
			{
				return;
			}

			// 既存レコード（同名の人物すべて）を用意
			List<TPerson> aMasters;
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			{
				aMasters = YlCommon.SelectMastersByName<TPerson>(aMusicInfoDbInDisk.Connection, SelectedPerson.Name);
			}

			// 新規作成用を追加
			TPerson aNewRecord = new TPerson
			{
				// IRcBase
				Id = null,
				Import = false,
				Invalid = false,
				UpdateTime = YlConstants.INVALID_MJD,
				Dirty = true,

				// IRcMaster
				Name = SelectedPerson.Name,
				Ruby = null,
				Keyword = null,
			};
			aMasters.Insert(0, aNewRecord);

			using (EditPersonWindowViewModel aEditPersonWindowViewModel = new EditPersonWindowViewModel())
			{
				aEditPersonWindowViewModel.Environment = Environment;
				aEditPersonWindowViewModel.SetMasters(aMasters);
				aEditPersonWindowViewModel.DefaultId = SelectedPerson.Id;
				Messenger.Raise(new TransitionMessage(aEditPersonWindowViewModel, "OpenEditPersonWindow"));

				if (String.IsNullOrEmpty(aEditPersonWindowViewModel.OkSelectedId))
				{
					return;
				}

				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					TPerson aMaster = YlCommon.SelectBaseById<TPerson>(aContext, aEditPersonWindowViewModel.OkSelectedId);
					if (aMaster != null)
					{
						List<TPerson> aSameNamePeople = YlCommon.SelectMastersByName<TPerson>(aContext, aMaster.Name);
						aMaster.Environment = Environment;
						aMaster.AvoidSameName = aSameNamePeople.Count > 1;
						People[People.IndexOf(SelectedPerson)] = aMaster;
						SelectedPerson = aMaster;
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// People 中に oTargetPerson が存在するか
		// People.IndexOf() だとインスタンスが異なる場合は期待通りの動作をしないため本関数が必要
		// --------------------------------------------------------------------
		private Int32 PeopleIndexOfById(TPerson oTargetPerson)
		{
			for (Int32 i = 0; i < People.Count; i++)
			{
				if (oTargetPerson.Id == People[i].Id)
				{
					return i;
				}
			}
			return -1;
		}
	}
	// public class EditPeopleWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
