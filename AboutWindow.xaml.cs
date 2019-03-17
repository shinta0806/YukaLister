// ============================================================================
// 
// バージョン情報ウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using YukaLister.Shared;

namespace YukaLister
{
	/// <summary>
	/// AboutWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class AboutWindow : Window
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public AboutWindow(LogWriter oLogWriter)
		{
			InitializeComponent();

			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			try
			{
				Process.Start(e.Uri.AbsoluteUri);
				e.Handled = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リンク先を表示できませんでした。\n" + e.Uri.AbsoluteUri);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バージョン情報ウィンドウを開きます。");

				// 表示
				Title = YlCommon.APP_NAME_J + "のバージョン情報";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif
				LabelAppName.Content = YlCommon.APP_NAME_J;
				LabelAppVer.Content = YlCommon.APP_VER;
				LabelCopyright.Content = YlCommon.COPYRIGHT_J;

				// コントロール
				ButtonOK.Focus();

				Common.CascadeWindow(this);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "バージョン情報ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バージョン情報ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "バージョン情報ウィンドウクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DialogResult = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class AboutWindow ___END___

}
// namespace YukaLister ___END___
