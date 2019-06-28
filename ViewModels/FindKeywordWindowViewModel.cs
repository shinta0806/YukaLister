// ============================================================================
// 
// キーワード検索ウィンドウの ViewModel
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
using System.Diagnostics;

using YukaLister.Models;

namespace YukaLister.ViewModels
{
	public class FindKeywordWindowViewModel : ViewModel
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

		// アクティブ
		private Boolean mIsActive;
		public Boolean IsActive
		{
			get => mIsActive;
			set => RaisePropertyChangedIfSet(ref mIsActive, value);
		}

		// キーワード
		private String mKeyword;
		public String Keyword
		{
			get => mKeyword;
			set
			{
				if (RaisePropertyChangedIfSet(ref mKeyword, value))
				{
					ButtonFindClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// キーワードフォーカス
		private Boolean mIsKeywordFocused;
		public Boolean IsKeywordFocused
		{
			get => mIsKeywordFocused;
			set
			{
				// 再度フォーカスを当てられるように強制伝播
				mIsKeywordFocused = value;
				RaisePropertyChanged(nameof(IsKeywordFocused));
			}
		}

		// キーワード選択
		private String mSelectedKeyword;
		public String SelectedKeyword
		{
			get => mSelectedKeyword;
			set => RaisePropertyChangedIfSet(ref mSelectedKeyword, value);
		}

		// 大文字小文字の区別
		private Boolean mCaseSensitive;
		public Boolean CaseSensitive
		{
			get => mCaseSensitive;
			set => RaisePropertyChangedIfSet(ref mCaseSensitive, value);
		}

		// 全体一致
		private Boolean mWholeMatch;
		public Boolean WholeMatch
		{
			get => mWholeMatch;
			set => RaisePropertyChangedIfSet(ref mWholeMatch, value);
		}


		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// ファイル一覧ウィンドウの ViewModel
		public ViewModel ViewTFoundsWindowViewModel { get; set; }

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// 検索方向
		public Int32 Direction { get; set; }

		// ウィンドウが閉じられた
		public Boolean IsClosed { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region 次を検索・前を検索ボタンの制御
		private ListenerCommand<String> mButtonFindClickedCommand;

		public ListenerCommand<String> ButtonFindClickedCommand
		{
			get
			{
				if (mButtonFindClickedCommand == null)
				{
					mButtonFindClickedCommand = new ListenerCommand<String>(ButtonFindClicked, CanButtonFindClicked);
				}
				return mButtonFindClickedCommand;
			}
		}

		public Boolean CanButtonFindClicked()
		{
			return !String.IsNullOrEmpty(Keyword);
		}

		public void ButtonFindClicked(String oParameter)
		{
			try
			{
				Direction = oParameter == "Backward" ? -1 : 1;
				ViewTFoundsWindowViewModel.Messenger.Raise(new InteractionMessage("FindKeyword"));
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 閉じるボタンの制御
		private ViewModelCommand mButtonCancelClickedCommand;

		public ViewModelCommand ButtonCancelClickedCommand
		{
			get
			{
				if (mButtonCancelClickedCommand == null)
				{
					mButtonCancelClickedCommand = new ViewModelCommand(ButtonCancelClicked);
				}
				return mButtonCancelClickedCommand;
			}
		}

		public void ButtonCancelClicked()
		{
			try
			{
				Messenger.Raise(new WindowActionMessage("Close"));
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "閉じるボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ウィンドウをアクティブ化する
		// --------------------------------------------------------------------
		public void Activate()
		{
			try
			{
				IsActive = true;
				IsKeywordFocused = true;

				// キーワード全選択
				String aBak = Keyword;
				Keyword = null;
				SelectedKeyword = aBak;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "アクティブ化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 設定されるべきプロパティーをコピー
		// --------------------------------------------------------------------
		public void CopyFrom(FindKeywordWindowViewModel oSource)
		{
			Keyword = oSource.Keyword;
			CaseSensitive = oSource.CaseSensitive;
			WholeMatch = oSource.WholeMatch;
			ViewTFoundsWindowViewModel = oSource.ViewTFoundsWindowViewModel;
			Environment = oSource.Environment;
		}

		// --------------------------------------------------------------------
		// 初期化
		// 本関数を呼ぶ前に Environment 等を設定しておく必要がある
		// --------------------------------------------------------------------
		public void Initialize()
		{
			Debug.Assert(ViewTFoundsWindowViewModel != null, "ViewTFoundsWindowViewModel is null");
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				// タイトルバー
				Title = "キーワード検索";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "キーワード検索ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リソース解放
		// --------------------------------------------------------------------
		protected override void Dispose(Boolean oDisposing)
		{
			IsClosed = true;
			base.Dispose(oDisposing);
		}


	}
	// public class FindKeywordWindowViewModel ___END___
}
// YukaLister.ViewModels ___END___
