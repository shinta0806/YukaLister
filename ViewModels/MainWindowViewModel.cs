// ============================================================================
// 
// メインウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ・プロキシープロパティー削減のため、可能な限りプロパティー本体を VM に置く
// ・処理の多くは Model の範疇のため、View からトリガーされた処理は可能な限り Model に置く
//
// ToDo: スプラッシュウィンドウ、同期バグ
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;

using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

using Shinta;
using Shinta.Behaviors;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;

using YukaLister.Models;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		/* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom    : ViewModelCommand
         *  lvcomn   : ViewModelCommand(CanExecute無)
         *  llcom    : ListenerCommand(パラメータ有のコマンド)
         *  llcomn   : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop    : 変更通知プロパティ
         *  lsprop   : 変更通知プロパティ(ショートバージョン)
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

		// データグリッドの選択
		private TargetFolderInfo mSelectedTargetFolderInfo;
		public TargetFolderInfo SelectedTargetFolderInfo
		{
			get => mSelectedTargetFolderInfo;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedTargetFolderInfo, value))
				{
					ButtonRemoveTargetFolderClickedCommand.RaiseCanExecuteChanged();
					ButtonFolderSettingsClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// ゆかり検索対象フォルダー（表示用）
		private List<TargetFolderInfo> mTargetFolderInfosVisible;
		public List<TargetFolderInfo> TargetFolderInfosVisible
		{
			get => mTargetFolderInfosVisible;
			set => RaisePropertyChangedIfSet(ref mTargetFolderInfosVisible, value);
		}

		// ゆかり用リストデータベース構築状況アイコン（一定間隔で表示を更新するため、セット時には Raise しない）
		public String YukaListerStatusIcon
		{
			get => YUKA_LISTER_STATUS_ICONS[(Int32)YukaListerDbStatus];
		}

		// ゆかり用リストデータベース構築状況メッセージ（一定間隔で表示を更新するため、セット時には Raise しない）
		public String YukaListerStatusMessage { get; set; }
		public String YukaListerStatusSubMessage { get; set; }
		public String YukaListerStatusDisplayMessage
		{
			get => YukaListerStatusMessage + YukaListerStatusSubMessage;
		}

		// ゆかり用リストデータベース構築状況（一定間隔で表示を更新するため、セット時には Raise しない）
		public YukaListerStatus YukaListerDbStatus { get; set; }

		// ステータスバーメッセージ
		private String mStatusBarMessage;
		public String StatusBarMessage
		{
			get => mStatusBarMessage;
			set => RaisePropertyChangedIfSet(ref mStatusBarMessage, value);
		}

		// ステータスバー文字色
		private SolidColorBrush mStatusBarColor;
		public SolidColorBrush StatusBarColor
		{
			get => mStatusBarColor;
			set => RaisePropertyChangedIfSet(ref mStatusBarColor, value);
		}

		// --------------------------------------------------------------------
		// 一般プロパティー
		// --------------------------------------------------------------------

		// スプラッシュウィンドウ
		public SplashWindowViewModel SplashWindowVm { get; set; }

		// ゆかりすたー本体
		public YukaListerModel YukaLister { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region 環境設定ボタンの制御
		private ViewModelCommand mButtonYukaListerSettingsClickedCommand;

		public ViewModelCommand ButtonYukaListerSettingsClickedCommand
		{
			get
			{
				if (mButtonYukaListerSettingsClickedCommand == null)
				{
					mButtonYukaListerSettingsClickedCommand = new ViewModelCommand(ButtonYukaListerSettingsClicked);
				}
				return mButtonYukaListerSettingsClickedCommand;
			}
		}

		public void ButtonYukaListerSettingsClicked()
		{
			try
			{
				YukaLister.ButtonYukaListerSettingsClicked();
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "環境設定ボタンクリック時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ヘルプメニューアイテムの制御
		public ListenerCommand<String> MehuItemHelpClickedCommand
		{
			get => YukaLister.Environment.HelpClickedCommand;
		}
		#endregion

		#region 履歴メニューアイテムの制御
		private ViewModelCommand mMenuItemHistoryClickedCommand;

		public ViewModelCommand MenuItemHistoryClickedCommand
		{
			get
			{
				if (mMenuItemHistoryClickedCommand == null)
				{
					mMenuItemHistoryClickedCommand = new ViewModelCommand(MenuItemHistoryClicked);
				}
				return mMenuItemHistoryClickedCommand;
			}
		}

		public void MenuItemHistoryClicked()
		{
			try
			{
				Process.Start(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + FILE_NAME_HISTORY);
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "改訂履歴メニュークリック時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region バージョン情報メニューアイテムの制御
		private ViewModelCommand mMenuItemAboutClickedCommand;

		public ViewModelCommand MenuItemAboutClickedCommand
		{
			get
			{
				if (mMenuItemAboutClickedCommand == null)
				{
					mMenuItemAboutClickedCommand = new ViewModelCommand(MenuItemAboutClicked);
				}
				return mMenuItemAboutClickedCommand;
			}
		}

		public void MenuItemAboutClicked()
		{
			try
			{
				// ViewModel 経由でウィンドウを開く
				using (AboutWindowViewModel aAboutWindowViewModel = new AboutWindowViewModel())
				{
					aAboutWindowViewModel.Environment = YukaLister.Environment;
					Messenger.Raise(new TransitionMessage(aAboutWindowViewModel, "OpenAboutWindow"));
				}
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "バージョン情報メニュークリック時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
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
				YukaLister.YukariDb.ButtonFolderSettingsClicked();
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "データグリッドダブルクリック時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 削除ボタンの制御
		private ViewModelCommand mButtonRemoveTargetFolderClickedCommand;

		public ViewModelCommand ButtonRemoveTargetFolderClickedCommand
		{
			get
			{
				if (mButtonRemoveTargetFolderClickedCommand == null)
				{
					mButtonRemoveTargetFolderClickedCommand = new ViewModelCommand(ButtonRemoveTargetFolderClicked, CanButtonRemoveTargetFolderClick);
				}
				return mButtonRemoveTargetFolderClickedCommand;
			}
		}

		public Boolean CanButtonRemoveTargetFolderClick()
		{
			return SelectedTargetFolderInfo != null;
		}

		public void ButtonRemoveTargetFolderClicked()
		{
			try
			{
				YukaLister.YukariDb.ButtonRemoveTargetFolderClicked();
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "削除ボタンクリック時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ファイル一覧ボタンの制御
		private ViewModelCommand mButtonTFoundsClickedCommand;

		public ViewModelCommand ButtonTFoundsClickedCommand
		{
			get
			{
				if (mButtonTFoundsClickedCommand == null)
				{
					mButtonTFoundsClickedCommand = new ViewModelCommand(ButtonTFoundsClicked, CanButtonTFoundsClick);
				}
				return mButtonTFoundsClickedCommand;
			}
		}

		public Boolean CanButtonTFoundsClick()
		{
			// 現状、（良いか悪いかは別として）フォルダー内のファイルがあるかではなく、フォルダーが登録されているかで判定する
			return TargetFolderInfosVisible?.Count > 0;
		}

		public void ButtonTFoundsClicked()
		{
			try
			{
				YukaLister.YukariDb.ButtonTFoundsClicked();
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル一覧ボタンクリック時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
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
					mButtonFolderSettingsClickedCommand = new ViewModelCommand(ButtonFolderSettingsClicked, CanButtonFolderSettingsClick);
				}
				return mButtonFolderSettingsClickedCommand;
			}
		}

		public bool CanButtonFolderSettingsClick()
		{
			return SelectedTargetFolderInfo != null;
		}

		public void ButtonFolderSettingsClicked()
		{
			try
			{
				YukaLister.YukariDb.ButtonFolderSettingsClicked();
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ボタンクリック時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ファイルドロップの制御
		private ListenerCommand<String[]> mWindowFileDropCommand;

		public ListenerCommand<String[]> WindowFileDropCommand
		{
			get
			{
				if (mWindowFileDropCommand == null)
				{
					mWindowFileDropCommand = new ListenerCommand<String[]>(WindowFileDrop);
				}
				return mWindowFileDropCommand;
			}
		}

		public void WindowFileDrop(String[] oFiles)
		{
			try
			{
				foreach (String aFile in oFiles)
				{
					if (!Directory.Exists(YukaLister.Environment.ExtendPath(aFile)))
					{
						// フォルダーでない場合は何もしない
						continue;
					}

					YukaLister.YukariDb.AddFolderSelected(aFile);
				}
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイルドロップ時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region リムーバブルメディア着脱の制御
		private ListenerCommand<DeviceChangeInfo> mWindowDeviceChangeCommand;

		public ListenerCommand<DeviceChangeInfo> WindowDeviceChangeCommand
		{
			get
			{
				if (mWindowDeviceChangeCommand == null)
				{
					mWindowDeviceChangeCommand = new ListenerCommand<DeviceChangeInfo>(WindowDeviceChange);
				}
				return mWindowDeviceChangeCommand;
			}
		}

		public void WindowDeviceChange(DeviceChangeInfo oDeviceChangeInfo)
		{
			try
			{
				YukaLister.YukariDb.DeviceChange(oDeviceChangeInfo);
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "デバイス着脱時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion


		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void AddFolderSelected(FolderSelectionMessage oFolderSelectionMessage)
		{
			try
			{
				YukaLister.YukariDb.AddFolderSelected(oFolderSelectionMessage.Response);
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "追加フォルダー選択時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		public void Initialize()
		{
			try
			{
				// 子要素の初期化
				YukaLister.Initialize();

				// タイトルバー
				Title = YlConstants.APP_NAME_J;
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				// ステータスバー
				ClearStatusBarMessage();

				// スプラッシュウィンドウを閉じる
				SplashWindowVm.Close();
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "メインウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 一定間隔で更新するプロパティーの変更通知
		// --------------------------------------------------------------------
		public void RaiseLazyPropertyChanged()
		{
			RaisePropertyChanged(nameof(YukaListerDbStatus));
			RaisePropertyChanged(nameof(YukaListerStatusIcon));
			RaisePropertyChanged(nameof(YukaListerStatusDisplayMessage));
		}

		// --------------------------------------------------------------------
		// ステータスバーにメッセージを表示
		// --------------------------------------------------------------------
		public void SetStatusBarMessageWithInvoke(TraceEventType oTraceEventType, String oMsg)
		{
			Application.Current?.Dispatcher.Invoke(new Action(() =>
			{
				StatusBarMessage = oMsg;
				if (oTraceEventType == TraceEventType.Error)
				{
					StatusBarColor = new SolidColorBrush(Colors.Red);
				}
				else
				{
					StatusBarColor = new SolidColorBrush(Colors.Black);
				}
				YukaLister.Environment.LogWriter.ShowLogMessage(oTraceEventType, oMsg, true);
			}));
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ウィンドウクローズ
		// --------------------------------------------------------------------
		protected override void Dispose(Boolean oIsDisposing)
		{
			try
			{
				if (!mIsDisposed)
				{
					// 終了処理
					YukaLister.Quit();

					// マネージドリソース解放
					if (oIsDisposing)
					{
						// 今のところ無し
					}

					// アンマネージドリソース解放
					// 今のところ無し

					// 基底呼び出し
					base.Dispose(oIsDisposing);
				}

				mIsDisposed = true;
			}
			catch (Exception oExcep)
			{
				YukaLister.Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "終了時エラー：\n" + oExcep.Message);
				YukaLister.Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// ゆかり用リストデータベース構築状況アイコン
		private readonly String[] YUKA_LISTER_STATUS_ICONS = { "●", ">>", "●" };

		// 改訂履歴ファイル
		private const String FILE_NAME_HISTORY = "YukaLister_History_JPN.txt";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// Dispose フラグ
		private Boolean mIsDisposed = false;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ステータスバーメッセージをクリア
		// --------------------------------------------------------------------
		private void ClearStatusBarMessage()
		{
			// null にするとステーバスバーの高さが低くなってしまうので String.Empty にする
			StatusBarMessage = String.Empty;
		}

	}
	// public class MainWindowViewModel ___END___

}
// namespace YukaLister.ViewModels ___END___
