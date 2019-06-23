// ============================================================================
// 
// ID 接頭辞入力ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel;

using YukaLister.Models;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class InputIdPrefixWindowViewModel : ViewModel
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

		// ID 接頭辞
		private String mIdPrefix;
		public String IdPrefix
		{
			get => mIdPrefix;
			set
			{
				if (RaisePropertyChangedIfSet(ref mIdPrefix, value))
				{
					ButtonOKClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// ID 接頭辞フォーカス
		private Boolean mIsIdPrefixFocused;
		public Boolean IsIdPrefixFocused
		{
			get => mIsIdPrefixFocused;
			set
			{
				// 再度フォーカスを当てられるように強制伝播
				mIsIdPrefixFocused = value;
				RaisePropertyChanged(nameof(IsIdPrefixFocused));
			}
		}



		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region ヘルプリンクの制御
		private ListenerCommand<String> mHelpClickedCommand;

		public ListenerCommand<String> HelpClickedCommand
		{
			get
			{
				if (mHelpClickedCommand == null)
				{
					mHelpClickedCommand = new ListenerCommand<String>(HelpClicked);
				}
				return mHelpClickedCommand;
			}
		}

		public void HelpClicked(String oParameter)
		{
			try
			{
				YlCommon.ShowHelp(Environment, oParameter);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region OK ボタンの制御
		private ViewModelCommand mButtonOKClickedCommand;

		public ViewModelCommand ButtonOKClickedCommand
		{
			get
			{
				if (mButtonOKClickedCommand == null)
				{
					mButtonOKClickedCommand = new ViewModelCommand(CButtonOKClicked, CanButtonOKClicked);
				}
				return mButtonOKClickedCommand;
			}
		}

		public Boolean CanButtonOKClicked()
		{
			return !String.IsNullOrEmpty(IdPrefix);
		}

		public void CButtonOKClicked()
		{
			try
			{
				if (IdPrefix == null)
				{
					return;
				}

				Environment.YukaListerSettings.IdPrefix = IdPrefix;
				Environment.YukaListerSettings.Save();

				Messenger.Raise(new WindowActionMessage("Close"));
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
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
				Title = "ID 接頭辞の設定";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				// フォーカス
				IsIdPrefixFocused = true;

			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ID 接頭辞入力ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}
