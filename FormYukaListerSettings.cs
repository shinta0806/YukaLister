// ============================================================================
// 
// 環境設定を行うウィンドウ
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
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YukaLister.Shared;

namespace YukaLister
{
	public partial class FormYukaListerSettings : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormYukaListerSettings(YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mYukaListerSettings = oYukaListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------
		private const String IMPORT_ANISON_INFO_FILTER = "anison.info ファイル|*" + Common.FILE_EXT_CSV + ";*" + Common.FILE_EXT_ZIP;

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// リスト出力
		private List<OutputWriter> mOutputWriters;

		// 環境設定
		private YukaListerSettings mYukaListerSettings;

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 拡張子をリストボックスに追加
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void AddExt()
		{
			String aExt = TextBoxTargetExt.Text;

			// 入力が空の場合はボタンは押されないはずだが念のため
			if (String.IsNullOrEmpty(aExt))
			{
				throw new Exception("拡張子を入力して下さい。");
			}

			// ワイルドカード等を除去
			aExt = aExt.Replace("*", "");
			aExt = aExt.Replace("?", "");
			aExt = aExt.Replace(".", "");

			// 除去で空になっていないか
			if (String.IsNullOrEmpty(aExt))
			{
				throw new Exception("有効な拡張子を入力して下さい。");
			}

			// 先頭にピリオド付加
			aExt = "." + aExt;

			// 小文字化
			aExt = aExt.ToLower();

			// 重複チェック
			if (ListBoxTargetExts.Items.Contains(aExt))
			{
				throw new Exception("既に追加されています。");
			}

			// 追加
			ListBoxTargetExts.Items.Add(aExt);
			TextBoxTargetExt.Text = null;
			ListBoxTargetExts.SelectedIndex = ListBoxTargetExts.Items.Count - 1;
		}

		// --------------------------------------------------------------------
		// 入力された値が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckInput()
		{
			// 設定タブ
			if (String.IsNullOrEmpty(TextBoxYukariConfigPathSeed.Text))
			{
				throw new Exception("ゆかり設定ファイルを指定して下さい。");
			}

			// リスト対象タブ
			if (ListBoxTargetExts.Items.Count == 0)
			{
				throw new Exception("リスト化対象ファイルの拡張子を指定して下さい。");
			}

			// メンテナンスタブ
			if (CheckBoxSyncMusicInfoDb.Checked)
			{
				if (String.IsNullOrEmpty(TextBoxSyncServer.Text) || TextBoxSyncServer.Text == "http://" || TextBoxSyncServer.Text == "https://")
				{
					throw new Exception("同期用のサーバー URL を指定して下さい。");
				}
				if (TextBoxSyncServer.Text.IndexOf("http://") != 0 && TextBoxSyncServer.Text.IndexOf("https://") != 0)
				{
					throw new Exception("http:// または https:// で始まる同期用のサーバー URL を指定して下さい。");
				}
				if (String.IsNullOrEmpty(TextBoxSyncAccount.Text))
				{
					throw new Exception("同期用のアカウント名を指定して下さい。");
				}
				if (String.IsNullOrEmpty(TextBoxSyncPassword.Text))
				{
					throw new Exception("同期用のパスワードを指定して下さい。");
				}

				// 補完
				if (TextBoxSyncServer.Text[TextBoxSyncServer.Text.Length - 1] != '/')
				{
					TextBoxSyncServer.Text += "/";
				}
			}
		}

		// --------------------------------------------------------------------
		// コンポーネントから設定に反映
		// --------------------------------------------------------------------
		private void ComposToSettings()
		{
			// 設定タブ
			mYukaListerSettings.YukariConfigPathSeed = TextBoxYukariConfigPathSeed.Text;

			// リスト対象タブ
			mYukaListerSettings.TargetExts.Clear();
			for (Int32 i = 0; i < ListBoxTargetExts.Items.Count; i++)
			{
				mYukaListerSettings.TargetExts.Add((String)ListBoxTargetExts.Items[i]);
			}
			mYukaListerSettings.TargetExts.Sort();

			// リスト出力タブ
			mYukaListerSettings.ListOutputFolder = TextBoxListFolder.Text;

			// メンテナンスタブ
			mYukaListerSettings.SyncMusicInfoDb = CheckBoxSyncMusicInfoDb.Checked;
			mYukaListerSettings.SyncServer = TextBoxSyncServer.Text;
			mYukaListerSettings.SyncAccount = TextBoxSyncAccount.Text;
			mYukaListerSettings.SyncPassword = YlCommon.Encrypt(TextBoxSyncPassword.Text);
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = "環境設定";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			// リスト出力形式
			mOutputWriters = new List<OutputWriter>();
			mOutputWriters.Add(new HtmlOutputWriter());
			mOutputWriters.Add(new CsvOutputWriter());
			LoadOutputSettings();
			foreach (OutputWriter aOutputWriter in mOutputWriters)
			{
				// 現時点で設定可能なプロパティー（変わらないもの）は設定しておく
				aOutputWriter.LogWriter = mLogWriter;

				// 追加
				ComboBoxListFormat.Items.Add(aOutputWriter.FormatName);
			}
			ComboBoxListFormat.SelectedIndex = 0;

			Common.CascadeForm(this);
		}

		// --------------------------------------------------------------------
		// すべての出力者の OutputSettings を読み込む
		// --------------------------------------------------------------------
		private void LoadOutputSettings()
		{
			foreach (OutputWriter aOutputWriter in mOutputWriters)
			{
				aOutputWriter.OutputSettings.Load();
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void RadioButtonImport_CheckedChanged(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				UpdateImportComponentsEnabled();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ラジオボタンインポートチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 選択された出力アドオン
		// --------------------------------------------------------------------
		private OutputWriter SelectedOutputWriter()
		{
			String aFormatName = (String)ComboBoxListFormat.Items[ComboBoxListFormat.SelectedIndex];

			foreach (OutputWriter aOutputWriter in mOutputWriters)
			{
				if (aOutputWriter.FormatName == aFormatName)
				{
					return aOutputWriter;
				}
			}

			return null;
		}

		// --------------------------------------------------------------------
		// テキストボックスのファイル名を設定
		// テキストボックスのテキストが空で、かつ、ファイルが存在する場合のみ
		// --------------------------------------------------------------------
		private void SetTextoBoxImportAnisonInfoCsv(TextBox oTextBoxTarget, String oFileBody, TextBox oTextBoxStandard)
		{
			if (!String.IsNullOrEmpty(oTextBoxTarget.Text))
			{
				return;
			}

			// CSV がある場合に設定
			if (File.Exists(Path.GetDirectoryName(oTextBoxStandard.Text) + "\\" + oFileBody + Common.FILE_EXT_CSV))
			{
				oTextBoxTarget.Text = Path.GetDirectoryName(oTextBoxStandard.Text) + "\\" + oFileBody + Common.FILE_EXT_CSV;
				return;
			}

			// ZIP がある場合に設定
			if (File.Exists(Path.GetDirectoryName(oTextBoxStandard.Text) + "\\" + oFileBody + Common.FILE_EXT_ZIP))
			{
				oTextBoxTarget.Text = Path.GetDirectoryName(oTextBoxStandard.Text) + "\\" + oFileBody + Common.FILE_EXT_ZIP;
			}
		}

		// --------------------------------------------------------------------
		// 設定をコンポーネントに反映
		// --------------------------------------------------------------------
		private void SettingsToCompos()
		{
			// 設定タブ
			TextBoxYukariConfigPathSeed.Text = mYukaListerSettings.YukariConfigPathSeed;

			// リスト対象タブ
			ListBoxTargetExts.Items.Clear();
			ListBoxTargetExts.Items.AddRange(mYukaListerSettings.TargetExts.ToArray());
			UpdateButtonAddExt();
			UpdateButtonRemoveExt();

			// リスト出力タブ
			TextBoxListFolder.Text = mYukaListerSettings.ListOutputFolder;

			// メンテナンスタブ
			CheckBoxSyncMusicInfoDb.Checked = mYukaListerSettings.SyncMusicInfoDb;
			TextBoxSyncServer.Text = mYukaListerSettings.SyncServer;
			TextBoxSyncAccount.Text = mYukaListerSettings.SyncAccount;
			TextBoxSyncPassword.Text = YlCommon.Decrypt(mYukaListerSettings.SyncPassword);
			UpdateSyncComponentsEnabled();

			// インポートタブ
			UpdateImportComponentsEnabled();
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void TextBoxImportAnisonInfoCsv_TextChanged(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				TextBox aTextBoxSender = (TextBox)oSender;
				if (String.IsNullOrEmpty(aTextBoxSender.Text))
				{
					return;
				}

				if (aTextBoxSender != TextBoxImportProgramCsv)
				{
					SetTextoBoxImportAnisonInfoCsv(TextBoxImportProgramCsv, YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM, aTextBoxSender);
				}
				if (aTextBoxSender != TextBoxImportAnisonCsv)
				{
					SetTextoBoxImportAnisonInfoCsv(TextBoxImportAnisonCsv, YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON, aTextBoxSender);
				}
				if (aTextBoxSender != TextBoxImportSfCsv)
				{
					SetTextoBoxImportAnisonInfoCsv(TextBoxImportSfCsv, YlCommon.FILE_BODY_ANISON_INFO_CSV_SF, aTextBoxSender);
				}
				if (aTextBoxSender != TextBoxImportGameCsv)
				{
					SetTextoBoxImportAnisonInfoCsv(TextBoxImportGameCsv, YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME, aTextBoxSender);
				}

			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "anison.info CSV テキスト変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 追加ボタン（拡張子）の状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonAddExt()
		{
			ButtonAddExt.Enabled = !String.IsNullOrEmpty(TextBoxTargetExt.Text);
		}

		// --------------------------------------------------------------------
		// 削除ボタン（拡張子）の状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonRemoveExt()
		{
			ButtonRemoveExt.Enabled = ListBoxTargetExts.SelectedIndex >= 0;
		}

		// --------------------------------------------------------------------
		// インポート関連コンポーネントの有効無効の切替
		// --------------------------------------------------------------------
		private void UpdateImportComponentsEnabled()
		{
			TextBoxImportYukaLister.Enabled = ButtonBrowseImportYukaLister.Enabled = RadioButtonImportYukaLister.Checked;
			TextBoxImportProgramCsv.Enabled = ButtonBrowseImportProgramCsv.Enabled = RadioButtonImportAnisonInfoCsv.Checked;
			TextBoxImportAnisonCsv.Enabled = ButtonBrowseImportAnisonCsv.Enabled = RadioButtonImportAnisonInfoCsv.Checked;
			TextBoxImportSfCsv.Enabled = ButtonBrowseImportSfCsv.Enabled = RadioButtonImportAnisonInfoCsv.Checked;
			TextBoxImportGameCsv.Enabled = ButtonBrowseImportGameCsv.Enabled = RadioButtonImportAnisonInfoCsv.Checked;
			TextBoxImportNicoKaraLister.Enabled = ButtonBrowseImportNicoKaraLister.Enabled = RadioButtonImportNicoKaraLister.Checked;
		}

		// --------------------------------------------------------------------
		// 同期関連コンポーネントの有効無効の切替
		// --------------------------------------------------------------------
		private void UpdateSyncComponentsEnabled()
		{
			Boolean aEnabled = CheckBoxSyncMusicInfoDb.Checked;
			TextBoxSyncServer.Enabled = aEnabled;
			TextBoxSyncAccount.Enabled = aEnabled;
			TextBoxSyncPassword.Enabled = aEnabled;
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormYukaListerSettings_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "環境設定ウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "環境設定ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormYukaListerSettings_Shown(object sender, EventArgs e)
		{
			try
			{
				SettingsToCompos();
				RadioButtonImportYukaLister.Checked = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "環境設定ウィンドウ表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseYukariConfig_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialogYukariConfigPathSeed.FileName = YlCommon.FILE_NAME_YUKARI_CONFIG;
				if (OpenFileDialogYukariConfigPathSeed.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				TextBoxYukariConfigPathSeed.Text = OpenFileDialogYukariConfigPathSeed.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ゆかり設定ファイル参照ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				CheckInput();
				ComposToSettings();
				mYukaListerSettings.Save();
				DialogResult = DialogResult.OK;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormYukaListerSettings_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "環境設定ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "環境設定ウィンドウクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseImportProgramCsv_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialogMisc.Filter = IMPORT_ANISON_INFO_FILTER;
				OpenFileDialogMisc.Title = YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV + " または "
						+ YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_ZIP;
				if (OpenFileDialogMisc.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				TextBoxImportProgramCsv.Text = OpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV + " 参照ボタンクリック時エラー：\n"
						+ oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseImportAnisonCsv_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialogMisc.Filter = IMPORT_ANISON_INFO_FILTER;
				OpenFileDialogMisc.Title = YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV + " または "
						+ YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_ZIP;
				if (OpenFileDialogMisc.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				TextBoxImportAnisonCsv.Text = OpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV + " 参照ボタンクリック時エラー：\n"
						+ oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseImportSfCsv_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialogMisc.Filter = IMPORT_ANISON_INFO_FILTER;
				OpenFileDialogMisc.Title = YlCommon.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV + " または "
						+ YlCommon.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_ZIP;
				if (OpenFileDialogMisc.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				TextBoxImportSfCsv.Text = OpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, YlCommon.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV + " 参照ボタンクリック時エラー：\n"
						+ oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseImportGameCsv_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialogMisc.Filter = IMPORT_ANISON_INFO_FILTER;
				OpenFileDialogMisc.Title = YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV + " または "
						+ YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_ZIP;
				if (OpenFileDialogMisc.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				TextBoxImportGameCsv.Text = OpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV + " 参照ボタンクリック時エラー：\n"
						+ oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonImport_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormImport aFormImport = new FormImport(mYukaListerSettings, mLogWriter))
				{
					// ゆかりすたーでエクスポートしたファイルをインポート
					aFormImport.IsYukaListerMode = RadioButtonImportYukaLister.Checked;
					aFormImport.YklInfoPath = TextBoxImportYukaLister.Text;

					// anison.info CSV をインポート
					aFormImport.IsAnisonInfoMode = RadioButtonImportAnisonInfoCsv.Checked;
					aFormImport.ProgramCsvPath = TextBoxImportProgramCsv.Text;
					aFormImport.AnisonCsvPath = TextBoxImportAnisonCsv.Text;
					aFormImport.SfCsvPath = TextBoxImportSfCsv.Text;
					aFormImport.GameCsvPath = TextBoxImportGameCsv.Text;

					// ニコカラりすたーでエクスポートしたファイルをインポート
					aFormImport.IsNicoKaraListerMode = RadioButtonImportNicoKaraLister.Checked;
					aFormImport.NklInfoPath = TextBoxImportNicoKaraLister.Text;

					aFormImport.ShowDialog(this);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "インポートボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseImportNicoKaraLister_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialogMisc.Filter = "ニコカラりすたー情報ファイル|*" + YlCommon.FILE_EXT_NKLINFO;
				OpenFileDialogMisc.Title = "ニコカラりすたー情報ファイル";
				if (OpenFileDialogMisc.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				TextBoxImportNicoKaraLister.Text = OpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ニコカラりすたー参照ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxSyncMusicInfoDb_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateSyncComponentsEnabled();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "同期チェック変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOutput_Click(object sender, EventArgs e)
		{
			try
			{
				// 確認
				String aOutputFolderPath = TextBoxListFolder.Text;
				if (String.IsNullOrEmpty(aOutputFolderPath))
				{
					throw new Exception("リスト出力先フォルダーを指定して下さい。");
				}
				if (aOutputFolderPath[aOutputFolderPath.Length - 1] != '\\')
				{
					aOutputFolderPath += "\\";
				}
				if (!Directory.Exists(aOutputFolderPath))
				{
					Directory.CreateDirectory(aOutputFolderPath);
				}

				// 出力
				OutputWriter aSelectedOutputWriter = SelectedOutputWriter();
				aSelectedOutputWriter.FolderPath = aOutputFolderPath;
				YlCommon.OutputList(aSelectedOutputWriter, mYukaListerSettings);

				mLogWriter.ShowLogMessage(TraceEventType.Information, "リスト出力が完了しました。");

				// 表示
				String aOutputFilePath = aOutputFolderPath + aSelectedOutputWriter.TopFileName;
				try
				{
					Process.Start(aOutputFilePath);
				}
				catch (Exception)
				{
					throw new Exception("出力先ファイルを開けませんでした。\n" + aOutputFilePath);
				}

				// 状態保存（ウィンドウのキャンセルボタンが押されても保存されるように）
				mYukaListerSettings.ListOutputFolder = TextBoxListFolder.Text;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リスト出力ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseOutputPath_Click(object sender, EventArgs e)
		{
			try
			{
				FolderBrowserDialogOutputList.SelectedPath = TextBoxListFolder.Text;
				if (FolderBrowserDialogOutputList.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				TextBoxListFolder.Text = FolderBrowserDialogOutputList.SelectedPath;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リスト出力先フォルダー参照ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxYukariConfigPathSeed_TextChanged(object sender, EventArgs e)
		{
			try
			{
				YukaListerSettings aTempYukaListerSettings = new YukaListerSettings();
				try
				{
					aTempYukaListerSettings.YukariConfigPathSeed = TextBoxYukariConfigPathSeed.Text;
					TextBoxYukariListFolder.Text = Path.GetDirectoryName(aTempYukaListerSettings.YukariDbPath());
				}
				catch (Exception)
				{
					// エラーは無視する
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ゆかり設定ファイル変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonYukariListSettings_Click(object sender, EventArgs e)
		{
			try
			{
				YukariOutputWriter aYukariOutputWriter = new YukariOutputWriter();
				aYukariOutputWriter.LogWriter = mLogWriter;
				aYukariOutputWriter.OutputSettings.Load();

				if (aYukariOutputWriter.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				// 設定変更をすべての出力者に反映
				LoadOutputSettings();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ゆかりリクエスト用リスト出力設定ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonListSettings_Click(object sender, EventArgs e)
		{
			try
			{
				OutputWriter aSelectedOutputWriter = SelectedOutputWriter();

				if (aSelectedOutputWriter.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				// 設定変更をすべての出力者に反映
				LoadOutputSettings();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "閲覧用用リスト出力設定ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void LinkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				YlCommon.ShowHelp("Kankyousettei");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TabPageImport_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				e.Effect = DragDropEffects.None;

				// ファイル類のときのみ、受け付けるかどうかの判定をする
				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					String[] aDropFiles = (String[])e.Data.GetData(DataFormats.FileDrop, false);
					if (aDropFiles == null)
					{
						return;
					}
					foreach (String aDropFile in aDropFiles)
					{
						if (File.Exists(YlCommon.ExtendPath(aDropFile)))
						{
							// ファイルが含まれていたら受け付ける
							e.Effect = DragDropEffects.Copy;
							break;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "インポートタブドラッグエンター時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TabPageImport_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				Activate();

				String[] aDropFiles = (String[])e.Data.GetData(DataFormats.FileDrop, false);
				foreach (String aDropFile in aDropFiles)
				{
					if (!File.Exists(aDropFile))
					{
						continue;
					}

					Boolean aNotHandled = false;
					String aExt = Path.GetExtension(aDropFile).ToLower();
					String aFileName = Path.GetFileName(aDropFile);
					if (aExt == Common.FILE_EXT_CSV || aExt == Common.FILE_EXT_ZIP)
					{
						// anison.info CSV インポート
						if (aFileName.IndexOf(YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							TextBoxImportProgramCsv.Text = aDropFile;
						}
						else if (aFileName.IndexOf(YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							TextBoxImportAnisonCsv.Text = aDropFile;
						}
						else if (aFileName.IndexOf(YlCommon.FILE_BODY_ANISON_INFO_CSV_SF, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							TextBoxImportSfCsv.Text = aDropFile;
						}
						else if (aFileName.IndexOf(YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							TextBoxImportGameCsv.Text = aDropFile;
						}
						else
						{
							aNotHandled = true;
						}
						RadioButtonImportAnisonInfoCsv.Checked = true;
					}
					else if (aExt == YlCommon.FILE_EXT_NKLINFO)
					{
						// ニコカラりすたーインポート
						TextBoxImportNicoKaraLister.Text = aDropFile;
						RadioButtonImportNicoKaraLister.Checked = true;
					}
					else
					{
						aNotHandled = true;
					}

					if (aNotHandled)
					{
						mLogWriter.ShowLogMessage(TraceEventType.Error, "ドロップされたファイルの種類を自動判定できませんでした。\n参照ボタンからファイルを指定して下さい。\n" + aDropFile);
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "インポートタブドラッグ＆ドロップ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxTargetExts_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateButtonRemoveExt();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "拡張子リスト選択変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxTargetExt_TextChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateButtonAddExt();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "拡張子入力時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonAddExt_Click(object sender, EventArgs e)
		{
			try
			{
				AddExt();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "拡張子追加ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonRemoveExt_Click(object sender, EventArgs e)
		{
			try
			{
				// 選択されていない場合はボタンが押されないはずだが念のため
				if (ListBoxTargetExts.SelectedIndex < 0)
				{
					throw new Exception("削除したい拡張子を選択してください。");
				}

				// 削除
				ListBoxTargetExts.Items.RemoveAt(ListBoxTargetExts.SelectedIndex);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseYukariConfigPathSeed_Click(object sender, EventArgs e)
		{
			try
			{
				OpenFileDialogMisc.Filter = "ゆかり設定ファイル|config" + Common.FILE_EXT_INI;
				OpenFileDialogMisc.Title = "ゆかり設定ファイル";
				if (OpenFileDialogMisc.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				TextBoxYukariConfigPathSeed.Text = OpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ゆかり設定ファイル参照ボタンクリック時エラー：\n"
						+ oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormYukaListerSettings ___END___

}
// namespace YukaLister ___END___