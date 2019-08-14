// ============================================================================
// 
// 紐付編集ウィンドウの基底 ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 本クラスは EditSequenceWindow を使わない。
// EditPeopleWindowViewModel などの派生クラスが EditSequenceWindow を使う。
// abstract にすると VisualStudio が EditSequenceWindow のプレビューを表示しなくなるので通常のクラスにしておく。
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
	public class EditSequenceWindowViewModel : ViewModel
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

		// ヘルプ引数
		private String mHelpCommandParameter;
		public String HelpCommandParameter
		{
			get => mHelpCommandParameter;
			set => RaisePropertyChangedIfSet(ref mHelpCommandParameter, value);
		}

		// データグリッドヘッダー
		private String mDataGridHeader;
		public String DataGridHeader
		{
			get => mDataGridHeader;
			set => RaisePropertyChangedIfSet(ref mDataGridHeader, value);
		}

		// 編集中のマスター群
		public ObservableCollection<IRcMaster> Masters { get; set; } = new ObservableCollection<IRcMaster>();

		// 選択されたマスター
		private IRcMaster mSelectedMaster;
		public IRcMaster SelectedMaster
		{
			get => mSelectedMaster;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedMaster, value))
				{
					ButtonRemoveClickedCommand.RaiseCanExecuteChanged();
					ButtonUpClickedCommand.RaiseCanExecuteChanged();
					ButtonDownClickedCommand.RaiseCanExecuteChanged();
					ButtonEditClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// 編集ボタンのコンテンツ
		private String mButtonEditContent;
		public String ButtonEditContent
		{
			get => mButtonEditContent;
			set => RaisePropertyChangedIfSet(ref mButtonEditContent, value);
		}

		// 新規作成ボタンのコンテンツ
		private String mButtonNewContent;
		public String ButtonNewContent
		{
			get => mButtonNewContent;
			set => RaisePropertyChangedIfSet(ref mButtonNewContent, value);
		}

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// 初期マスター ID 群
		public List<String> InitialIds { get; set; }

		// OK ボタンが押された時のマスター群
		public List<IRcMaster> OkSelectedMasters { get; set; }

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
				Add();
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
			return SelectedMaster != null;
		}

		public void ButtonRemoveClicked()
		{
			try
			{
				Masters.Remove(SelectedMaster);
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
			Int32 aIndex = Masters.IndexOf(SelectedMaster);
			return aIndex >= 1;
		}

		public void ButtonUpClicked()
		{
			try
			{
				Int32 aSelectedIndex = Masters.IndexOf(SelectedMaster);
				if (aSelectedIndex < 1)
				{
					return;
				}
				IRcMaster aItem = SelectedMaster;
				Masters.Remove(aItem);
				Masters.Insert(aSelectedIndex - 1, aItem);
				SelectedMaster = aItem;
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
			Int32 aIndex = Masters.IndexOf(SelectedMaster);
			return 0 <= aIndex && aIndex < Masters.Count - 1;
		}

		public void ButtonDownClicked()
		{
			try
			{
				Int32 aSelectedIndex = Masters.IndexOf(SelectedMaster);
				if (aSelectedIndex < 0 || aSelectedIndex >= Masters.Count - 1)
				{
					return;
				}
				IRcMaster aItem = SelectedMaster;
				Masters.Remove(aItem);
				Masters.Insert(aSelectedIndex + 1, aItem);
				SelectedMaster = aItem;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "下へボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region マスター詳細編集ボタンの制御
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
			return SelectedMaster != null;
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

		#region 新規マスター作成ボタンの制御
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
				New();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "新規作成ボタンクリック時エラー：\n" + oExcep.Message);
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
				OkSelectedMasters = Masters.ToList();
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
		public virtual void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "紐付編集ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// protected メンバー変数
		// ====================================================================

		// 検索したかどうか
		protected Boolean mIsMasterSearched = false;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// マスターを検索して追加
		// --------------------------------------------------------------------
		protected virtual void Add()
		{
		}

		// --------------------------------------------------------------------
		// マスターを編集
		// --------------------------------------------------------------------
		protected virtual void Edit()
		{
		}

		// --------------------------------------------------------------------
		// Masters 中に oTargetMaster が存在するか
		// Masters.IndexOf() だとインスタンスが異なる場合は期待通りの動作をしないため本関数が必要
		// --------------------------------------------------------------------
		protected Int32 MastersIndexOfById<T>(T oTargetMaster) where T : IRcMaster
		{
			for (Int32 i = 0; i < Masters.Count; i++)
			{
				if (oTargetMaster.Id == Masters[i].Id)
				{
					return i;
				}
			}
			return -1;
		}

		// --------------------------------------------------------------------
		// マスターを新規作成
		// --------------------------------------------------------------------
		protected virtual void New()
		{
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ====================================================================
		// private メンバー関数
		// ====================================================================

	}
	// public class EditSequenceWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
