// ============================================================================
// 
// ID 接頭辞の入力を行うフォーム
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using YukaLister.Shared;
using Shinta;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace YukaLister
{
	public partial class FormInputIdPrefix : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormInputIdPrefix(LogWriter oLogWriter)
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
			Text = "［デバッグ］" + Text;
#endif

			Common.CascadeForm(this);
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				IdPrefix = YlCommon.CheckIdPrefix(TextBoxIdPrefix.Text);
				DialogResult = DialogResult.OK;
			}
			catch (Exception oExcep)
			{
			mLogWriter.	ShowLogMessage(TraceEventType.Error, "ID 接頭辞決定時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormInputIdPrefix_Load(object sender, EventArgs e)
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

		private void FormInputIdPrefix_FormClosed(object sender, FormClosedEventArgs e)
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

		private void ButtonHelp_Click(object sender, EventArgs e)
		{
			try
			{
				YlCommon.ShowHelp(/*"ShinkinoIdnoSentouniFuyosuruMojiretsu"*/);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ヘルプボタン（ID 接頭辞入力ウィンドウ）クリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void LinkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				YlCommon.ShowHelp("IdSettouji");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormInputIdPrefix ___END___

}
// namespace NicoKaraLister ___END___
