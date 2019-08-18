// ============================================================================
// 
// インポートウィンドウ・エクスポートウィンドウの基底 ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 本クラスは ImportExportWindow を使わない。
// ImportWindowViewModel などの派生クラスが ImportExportWindow を使う。
// abstract にすると VisualStudio が ImportExportWindow のプレビューを表示しなくなるので通常のクラスにしておく。
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;

using Shinta;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class ImportExportWindowViewModel : ViewModel
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
			set
			{
				if (RaisePropertyChangedIfSet(ref mDescription, value))
				{
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, mDescription);
				}
			}
		}

		// 進捗
		private String mProgress;
		public String Progress
		{
			get => mProgress;
			set
			{
				if (RaisePropertyChangedIfSet(ref mProgress, value))
				{
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, mProgress);
				}
			}
		}

		// ログ
		public ObservableCollection<String> Logs { get; set; } = new ObservableCollection<String>();

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// インポートまたはエクスポート
		public String Kind { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region ウィンドウを閉じられるかの制御
		private ListenerCommand<CancelEventArgs> mWindowClosingCommand;

		public ListenerCommand<CancelEventArgs> WindowClosingCommand
		{
			get
			{
				if (mWindowClosingCommand == null)
				{
					mWindowClosingCommand = new ListenerCommand<CancelEventArgs>(WindowClosing);
				}
				return mWindowClosingCommand;
			}
		}

		public void WindowClosing(CancelEventArgs oCancelEventArgs)
		{
			try
			{
				if (!CancelImportExportIfNeeded())
				{
					// インポートをキャンセルしなかった場合はクローズをキャンセル
					oCancelEventArgs.Cancel = true;
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "クローズ処理時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 中止ボタンの制御
		private ViewModelCommand mButtonAbortClickedCommand;

		public ViewModelCommand ButtonAbortClickedCommand
		{
			get
			{
				if (mButtonAbortClickedCommand == null)
				{
					mButtonAbortClickedCommand = new ViewModelCommand(ButtonAbortClicked);
				}
				return mButtonAbortClickedCommand;
			}
		}

		public void ButtonAbortClicked()
		{
			try
			{
				if (CancelImportExportIfNeeded())
				{
					Messenger.Raise(new WindowActionMessage("Close"));
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "中止ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ログ文字列に追加
		// --------------------------------------------------------------------
		public void AppendDisplayText(String oText)
		{
			Logs.Add(oText);
		}

		// --------------------------------------------------------------------
		// 初期化
		// 本関数を呼ぶ前に Environment を設定しておく必要がある
		// --------------------------------------------------------------------
		public virtual async void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				Environment.LogWriter.AppendDisplayText = AppendDisplayText;

				await ImportExportAsync();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "インポートエクスポートウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 終了確認を出さないようにする
				mAbortCancellationTokenSource.Cancel();

				Environment.LogWriter.AppendDisplayText = null;
				Messenger.Raise(new WindowActionMessage("Close"));
			}
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// タスク中止用
		protected CancellationTokenSource mAbortCancellationTokenSource = new CancellationTokenSource();

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// インポート・エクスポート処理
		// --------------------------------------------------------------------
		protected virtual void ImportExport()
		{
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// インポート・エクスポートをキャンセル
		// ＜返値＞ true: キャンセルした（または既にされている）, false: キャンセルしなかった
		// --------------------------------------------------------------------
		private Boolean CancelImportExportIfNeeded()
		{
			if (mAbortCancellationTokenSource.IsCancellationRequested)
			{
				// 既にキャンセル処理中
				return true;
			}

			if (MessageBox.Show(Kind + "を中止してよろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.No)
			{
				// 新たにキャンセル
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, Kind + "を中止しています...");
				mAbortCancellationTokenSource.Cancel();
				return true;
			}

			// キャンセルしない
			return false;
		}

		// --------------------------------------------------------------------
		// インポート・エクスポート処理
		// --------------------------------------------------------------------
		private Task ImportExportAsync()
		{
			return Task.Run(() =>
			{
				try
				{
					// 終了時に強制終了されないように設定
					Thread.CurrentThread.IsBackground = false;

					ImportExport();

					if (!mAbortCancellationTokenSource.IsCancellationRequested)
					{
						Environment.LogWriter.ShowLogMessage(TraceEventType.Information, "完了");
					}
				}
				catch (OperationCanceledException)
				{
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "インポート・エクスポートを中止しました。");
				}
				catch (Exception oExcep)
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "インポート・エクスポート時エラー：\n" + oExcep.Message);
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
				}
			});
		}


	}
	// public class ImportWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
