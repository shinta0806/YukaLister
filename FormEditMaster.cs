// ============================================================================
// 
// TMaster 詳細編集ウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using YukaLister.Shared;

namespace YukaLister
{
	public partial class FormEditMaster : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormEditMaster(YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mYukaListerSettings = oYukaListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// アダプター
		public EditMasterAdapter Adapter { get; set; }

		// 初期表示する ID
		public String DefaultId { get; set; }

		// 登録された ID
		public String RegisteredId { get; set; }

		// ====================================================================
		// internal メンバー変数
		// ====================================================================

		// 環境設定
		internal YukaListerSettings mYukaListerSettings;

		// ログ
		internal LogWriter mLogWriter;

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = Adapter.Caption + "詳細情報の編集";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			Adapter.Init();

			Common.CascadeForm(this);
		}


		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormEditMaster_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "マスター詳細編集ウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "マスター詳細編集ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditMaster_Shown(object sender, EventArgs e)
		{
			try
			{
				// デフォルト ID を選択
				Int32 aIndex;
				if (String.IsNullOrEmpty(DefaultId) || (aIndex = ComboBoxId.Items.IndexOf(DefaultId)) < 0)
				{
					ComboBoxId.SelectedIndex = 0;
				}
				else
				{
					ComboBoxId.SelectedIndex = aIndex;
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "マスター詳細編集ウィンドウ表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditMaster_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "マスター詳細編集ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "マスター詳細編集ウィンドウクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ComboBoxId_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (ComboBoxId.SelectedIndex < 0)
				{
					return;
				}
				Adapter.RecordToCompos();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "マスター ID 選択変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxName_Leave(object sender, EventArgs e)
		{
			try
			{
				Adapter.WarnDuplicateIfNeeded();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "マスター名フォーカス解除時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				Adapter.CheckAndSave();
				DialogResult = DialogResult.OK;
			}
			catch (OperationCanceledException)
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "OK ボタンクリック時処理を中止しました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormEditMaster ___END___

}
// namespace YukaLister ___END___
