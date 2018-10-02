// ============================================================================
// 
// 出力設定を行うフォーム
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using YukaLister.Shared;
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

namespace YukaLister
{
	public partial class FormOutputSettings : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormOutputSettings(OutputWriter oOutputWriter, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 初期化
			mOutputWriter = oOutputWriter;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// リスト出力
		private OutputWriter mOutputWriter;

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
			Text = "出力設定：" + mOutputWriter.FormatName;
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormOutputSettings_Load(object sender, EventArgs e)
		{
			try
			{
				Init();

				Int32 aMaxWidth = 0;
				Int32 aMaxHeight = 0;
				List<TabPage> aTabPages = mOutputWriter.DialogTabPages();
				foreach (TabPage aTabPage in aTabPages)
				{
					if (aTabPage.Width > aMaxWidth)
					{
						aMaxWidth = aTabPage.Width;
					}
					if (aTabPage.Height > aMaxHeight)
					{
						aMaxHeight = aTabPage.Height;
					}

					TabControlOutputSettings.Controls.Add(aTabPage);
				}

				// タブページに合わせてタブコントロールの大きさを調整した場合の変化サイズ
				Int32 aDeltaWidth = aMaxWidth + 8 - TabControlOutputSettings.Width;
				Int32 aDeltaHeight = aMaxHeight + 26 - TabControlOutputSettings.Height;

				// フォームの大きさを調整
				Width += aDeltaWidth;
				Height += aDeltaHeight;

				Common.CascadeForm(this);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "出力設定フォームロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormOutputSettings_Shown(object sender, EventArgs e)
		{
			try
			{
				mOutputWriter.SettingsToCompos();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "出力設定フォーム表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				mOutputWriter.CheckInput();
				mOutputWriter.ComposToSettings();
				mOutputWriter.OutputSettings.Save();
				DialogResult = DialogResult.OK;
			}
			catch (OperationCanceledException)
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "設定変更を中止しました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormOutputSettings_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "出力設定フォームを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "出力設定フォームクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonHelp_Click(object sender, EventArgs e)
		{
			try
			{
				YlCommon.ShowHelp("Shutsuryokusettei");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ヘルプボタン（出力設定フォーム）クリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormOutputSettings ___END___

}
// namespace YukaLister ___END___
