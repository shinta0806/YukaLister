// ============================================================================
// 
// アプリケーション
// 
// ============================================================================

using Livet;

using Shinta;

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

using YukaLister.Models.SharedMisc;

namespace YukaLister
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 多重起動防止用
		// アプリケーション終了までガベージコレクションされないようにメンバー変数で持つ
		private Mutex mMutex;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// スタートアップ
		// --------------------------------------------------------------------
		private void Application_Startup(Object oSender, StartupEventArgs oStartupEventArgs)
		{
			// Livet コード
			DispatcherHelper.UIDispatcher = Dispatcher;

			// 集約エラーハンドラー設定
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			// 多重起動チェック
			if (Common.ActivateAnotherProcessWindowIfNeeded(Common.SHINTA + "_" + YlConstants.APP_ID, out mMutex))
			{
				throw new MultiInstanceException();
			}
		}

		// --------------------------------------------------------------------
		// 集約エラーハンドラー
		// --------------------------------------------------------------------
		private void CurrentDomain_UnhandledException(Object oSender, UnhandledExceptionEventArgs oUnhandledExceptionEventArgs)
		{
			if (oUnhandledExceptionEventArgs.ExceptionObject is MultiInstanceException)
			{
				// 多重起動の場合は何もしない
			}
			else
			{
				MessageBox.Show("不明なエラーが発生しました。アプリケーションを終了します。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			Environment.Exit(1);
		}
	}
	// public partial class App ___END___
}
// namespace YukaLister ___END___
