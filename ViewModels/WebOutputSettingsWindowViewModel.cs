// ============================================================================
// 
// HTML / PHP リスト出力設定ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Diagnostics;

using YukaLister.Models.OutputWriters;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class WebOutputSettingsWindowViewModel : OutputSettingsWindowViewModel
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

		#region HTML タブのプロパティー

		// 新着の使用
		private Boolean mEnableNew;
		public Boolean EnableNew
		{
			get => mEnableNew;
			set
			{
				if (RaisePropertyChangedIfSet(ref mEnableNew, value))
				{

				}
			}
		}

		// 新着の日数
		private String mNewDays;
		public String NewDays
		{
			get => mNewDays;
			set => RaisePropertyChangedIfSet(ref mNewDays, value);
		}

		#endregion

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

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
				base.Initialize();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "HTML / PHP リスト出力設定ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// タブアイテムにタブを追加
		// --------------------------------------------------------------------
		protected override void AddTabItems()
		{
			base.AddTabItems();

			AddTabItem("OutputSettingsTabItemWeb", "HTML");
		}

		// --------------------------------------------------------------------
		// 設定画面に入力された値が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		protected override void CheckInput()
		{
			base.CheckInput();

			// 新着の日数
			if (EnableNew)
			{
				Int32 aNewDays = Common.StringToInt32(NewDays);
				if (aNewDays < YlConstants.NEW_DAYS_MIN)
				{
					throw new Exception("新着の日数は " + YlConstants.NEW_DAYS_MIN.ToString() + " 以上を指定して下さい。");
				}
			}
		}

		// --------------------------------------------------------------------
		// プロパティーから設定に反映
		// --------------------------------------------------------------------
		protected override void PropertiesToSettings()
		{
			base.PropertiesToSettings();

			WebOutputSettings aWebOutputSettings = (WebOutputSettings)OutputWriter.OutputSettings;

			// 新着の使用
			aWebOutputSettings.EnableNew = EnableNew;

			// 新着の日数
			if (aWebOutputSettings.EnableNew)
			{
				aWebOutputSettings.NewDays = Common.StringToInt32(NewDays);
			}
		}

		// --------------------------------------------------------------------
		// 設定をプロパティーに反映
		// --------------------------------------------------------------------
		protected override void SettingsToProperties()
		{
			base.SettingsToProperties();

			WebOutputSettings aWebOutputSettings = (WebOutputSettings)OutputWriter.OutputSettings;

			// 新着の使用
			EnableNew = aWebOutputSettings.EnableNew;

			// 新着の日数
			NewDays = aWebOutputSettings.NewDays.ToString();
		}


	}
	// public class WebOutputSettingsWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
