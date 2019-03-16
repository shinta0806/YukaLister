// ============================================================================
// 
// キーワード検索ウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YukaLister.Shared;

namespace YukaLister
{
	/// <summary>
	/// FindKeywordWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class FindKeywordWindow : Window
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FindKeywordWindow(LogWriter oLogWriter)
		{
			InitializeComponent();

			// 初期化
			mLogWriter = oLogWriter;
		}

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FindKeywordWindow(FindKeywordWindow oSource)
		{
			InitializeComponent();

			// 初期化
			mLogWriter = oSource.mLogWriter;

			// プロパティー初期化
			Keyword = oSource.Keyword;
			Direction = oSource.Direction;
			CaseSensitive = oSource.CaseSensitive;
			WholeMatch = oSource.WholeMatch;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// キーワード
		public String Keyword { get; set; }

		// 検索方向
		public Int32 Direction { get; set; }

		// 大文字小文字の区別
		public Boolean CaseSensitive { get; set; }

		// 全体一致
		public Boolean WholeMatch { get; set; }

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// オーナーウィンドウハンドル
		private IntPtr mOwnerHandle;

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コンポーネントの値をプロパティーに格納
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void ComposToProperties(Int32 oDirection)
		{
			if (String.IsNullOrEmpty(Keyword))
			{
				throw new Exception("キーワードを入力して下さい。");
			}
			Direction = oDirection;
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Title = "キーワード検索";
#if DEBUG
			Title = "［デバッグ］" + Title;
#endif

			// 設定を反映
			TextBoxKeyword.Text = Keyword;
			CheckBoxCaseSensitive.IsChecked = CaseSensitive;
			CheckBoxWholeMatch.IsChecked = WholeMatch;

			// オーナーのメッセージハンドラー
			WindowInteropHelper aHelper = new WindowInteropHelper(Owner);
			mOwnerHandle = aHelper.Handle;
		}

		// --------------------------------------------------------------------
		// オーナーウィンドウ宛へメッセージを送信
		// --------------------------------------------------------------------
		private void PostMessageToOwner(Wm oMessage)
		{
			WindowsApi.PostMessage(mOwnerHandle, (UInt32)oMessage, (IntPtr)0, (IntPtr)0);
		}

		// ====================================================================
		// IDE 生成イベントハンドラー（キーワード検索ウィンドウ）
		// ====================================================================

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "キーワード検索ウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "キーワード検索ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "キーワード検索ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "キーワード検索ウィンドウクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Close();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "閉じるボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonFindNext_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ComposToProperties(1);
				PostMessageToOwner(Wm.FindKeywordRequested);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "次を検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonFindPrev_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ComposToProperties(-1);
				PostMessageToOwner(Wm.FindKeywordRequested);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "前を検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxCaseSensitive_Checked(object sender, RoutedEventArgs e)
		{
			try
			{
				CaseSensitive = (Boolean)CheckBoxCaseSensitive.IsChecked;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "大文字小文字区別チェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxWholeMatch_Checked(object sender, RoutedEventArgs e)
		{
			try
			{
				WholeMatch = (Boolean)CheckBoxWholeMatch.IsChecked;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "セル全体チェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxKeyword_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				Keyword = TextBoxKeyword.Text;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "キーワード変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}
