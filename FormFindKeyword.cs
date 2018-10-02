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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YukaLister.Shared;

namespace YukaLister
{
	public partial class FormFindKeyword : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormFindKeyword(LogWriter oLogWriter)
		{
			InitializeComponent();

			// 初期化
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// キーワード
		public String Keyword { get; set; }

		// 大文字小文字の区別
		public Boolean CaseSensitive { get; set; }

		// 全体一致
		public Boolean WholeMatch { get; set; }

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コンポーネントの値をプロパティーに格納
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void ComposToProperties()
		{
			Keyword = TextBoxKeyword.Text;
			if (String.IsNullOrEmpty(Keyword))
			{
				throw new Exception("キーワードを入力して下さい。");
			}
			CaseSensitive = CheckBoxCaseSensitive.Checked;
			WholeMatch = CheckBoxWholeMatch.Checked;
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = "キーワード検索";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif
		}

		// ====================================================================
		// IDE 生成イベントハンドラー（キーワード検索ウィンドウ）
		// ====================================================================

		private void FormFindKeyword_Load(object sender, EventArgs e)
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

		private void FormFindKeyword_FormClosed(object sender, FormClosedEventArgs e)
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

		private void ButtonCancel_Click(object sender, EventArgs e)
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

		private void ButtonFindNext_Click(object sender, EventArgs e)
		{
			try
			{
				ComposToProperties();
				WindowsApi.PostMessage(Owner.Handle, YlCommon.WM_FIND_KEYWORD_NEXT_REQUESTED, (IntPtr)0, (IntPtr)0);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "次を検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonFindPrev_Click(object sender, EventArgs e)
		{
			try
			{
				ComposToProperties();
				WindowsApi.PostMessage(Owner.Handle, YlCommon.WM_FIND_KEYWORD_PREV_REQUESTED, (IntPtr)0, (IntPtr)0);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "前を検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}
