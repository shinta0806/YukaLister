// ============================================================================
// 
// ID 接頭辞の入力を行うフォーム
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
    /// InputIdPrefixWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InputIdPrefixWindow : Window
    {
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public InputIdPrefixWindow(Shinta.LogWriter oLogWriter)
        {
            InitializeComponent();

			// 初期化
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ID 接頭辞
		public String IdPrefix { get; set; }

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
#if DEBUG
			Title = "［デバッグ］" + Title;
#endif

			Common.CascadeWindow(this);
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void ButtonOK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				IdPrefix = YlCommon.CheckIdPrefix(TextBoxIdPrefix.Text);
				DialogResult = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ID 接頭辞決定時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ID 接頭辞入力フォームを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ID 接頭辞入力フォームロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ID 接頭辞入力フォームを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ID 接頭辞入力フォームクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			try
			{
				YlCommon.ShowHelp(e.Uri.OriginalString);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DialogResult = false;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "キャンセルボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class InputIdPrefixWindow ___END___
}
// namespace YukaLister ___END___
