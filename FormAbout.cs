// ============================================================================
// 
// バージョン情報ウィンドウ
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
	public partial class FormAbout : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormAbout(LogWriter oLogWriter)
		{
			InitializeComponent();

			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// LinkLabel のクリックを集約
		// --------------------------------------------------------------------
		private void LinkLabels_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string aLink = String.Empty;

			try
			{
				// MSDN を見ると e.Link.LinkData がリンク先のように読めなくもないが、実際には
				// 値が入っていないので sender をキャストしてリンク先を取得する
				e.Link.Visited = true;
				aLink = ((LinkLabel)sender).Text;
				Process.Start(aLink);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リンク先を表示できませんでした。\n" + aLink);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormAbout_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バージョン情報ウィンドウを開きます。");

				// 表示
				Text = YlCommon.APP_NAME_J + "のバージョン情報";
#if DEBUG
				Text = "［デバッグ］" + Text;
#endif
				LabelAppName.Text = YlCommon.APP_NAME_J;
				LabelAppVer.Text = YlCommon.APP_VER;
				LabelCopyright.Text = YlCommon.COPYRIGHT_J;

				// コントロール
				ActiveControl = ButtonOK;

				Common.CascadeForm(this);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "バージョン情報ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormAbout_FormClosed(object sender, FormClosedEventArgs e)
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
	}
	// public partial class FormAbout ___END___

}
// namespace YukaLister ___END___
