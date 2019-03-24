// ============================================================================
// 
// 環境設定を行うウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Shinta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Navigation;
using YukaLister.Shared;

namespace YukaLister
{
	/// <summary>
	/// YukaListerSettingsWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class YukaListerSettingsWindow : Window
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 強制再取得をユーザーから指示されたか
		public Boolean RegetSyncDataNeeded;

		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public YukaListerSettingsWindow(YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
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

		// リスト化対象ファイルの拡張子
		private List<String> mTargetExts = new List<String>();

		// ウィンドウハンドル
		private IntPtr mHandle;

		// リスト出力先フォルダー参照用
		CommonOpenFileDialog mOpenFileDialogOutputListFolder;

		// 各種ファイル参照用
		OpenFileDialog mOpenFileDialogMisc;

		// ログ保存参照用
		SaveFileDialog mSaveFileDialogLog;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ドラッグ中のデータがファイルならばドラッグを受け入れる
		// --------------------------------------------------------------------
		private void AcceptDragEnterIfFileExists(DragEventArgs oDragEventArgs)
		{
			oDragEventArgs.Effects = DragDropEffects.None;
			oDragEventArgs.Handled = true;

			// ファイル類のときのみ、受け付けるかどうかの判定をする
			if (oDragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
			{
				String[] aDropFiles = (String[])oDragEventArgs.Data.GetData(DataFormats.FileDrop, false);
				if (aDropFiles == null)
				{
					return;
				}
				foreach (String aDropFile in aDropFiles)
				{
					if (File.Exists(YlCommon.ExtendPath(aDropFile)))
					{
						// ファイルが含まれていたら受け付ける
						oDragEventArgs.Effects = DragDropEffects.Copy;
						return;
					}
				}
			}
		}

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
			if (mTargetExts.Contains(aExt))
			{
				throw new Exception("既に追加されています。");
			}

			// 追加
			mTargetExts.Add(aExt);
			ListBoxTargetExts.Items.Refresh();
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
			if (mTargetExts.Count == 0)
			{
				throw new Exception("リスト化対象ファイルの拡張子を指定して下さい。");
			}

			// メンテナンスタブ
			if ((Boolean)CheckBoxSyncMusicInfoDb.IsChecked)
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
				if (String.IsNullOrEmpty(PasswordBoxSyncPassword.Password))
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
			mYukaListerSettings.TargetExts.AddRange(mTargetExts);
			mYukaListerSettings.TargetExts.Sort();

			// リスト出力タブ
			mYukaListerSettings.ListOutputFolder = TextBoxListFolder.Text;
			mYukaListerSettings.ClearPrevList = (Boolean)CheckBoxClearPrevList.IsChecked;

			// メンテナンスタブ
			mYukaListerSettings.CheckRss = (Boolean)CheckBoxCheckRss.IsChecked;
			mYukaListerSettings.SyncMusicInfoDb = (Boolean)CheckBoxSyncMusicInfoDb.IsChecked;
			mYukaListerSettings.SyncServer = TextBoxSyncServer.Text;
			mYukaListerSettings.SyncAccount = TextBoxSyncAccount.Text;
			mYukaListerSettings.SyncPassword = YlCommon.Encrypt(PasswordBoxSyncPassword.Password);
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Title = "環境設定";
#if DEBUG
			Title = "［デバッグ］" + Title;
#endif
			// メッセージハンドラー
			WindowInteropHelper aHelper = new WindowInteropHelper(this);
			mHandle = aHelper.Handle;
			HwndSource aSource = HwndSource.FromHwnd(mHandle);
			aSource.AddHook(new HwndSourceHook(WndProc));

			// リスト出力形式
			mOutputWriters = new List<OutputWriter>();
			mOutputWriters.Add(new HtmlOutputWriter());
			mOutputWriters.Add(new CsvOutputWriter());
			LoadOutputSettings();
			foreach (OutputWriter aOutputWriter in mOutputWriters)
			{
				// 現時点で設定可能なプロパティー（変わらないもの）は設定しておく
				aOutputWriter.LogWriter = mLogWriter;
				aOutputWriter.Owner = this;

				// 追加
				ComboBoxListFormat.Items.Add(aOutputWriter.FormatName);
			}
			ComboBoxListFormat.SelectedIndex = 0;

			// 共用されるダイアログはここで生成
			mOpenFileDialogMisc = new OpenFileDialog();

			Common.CascadeWindow(this);
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
		// 進捗系のコンポーネントをすべて元に戻す
		// --------------------------------------------------------------------
		private void MakeAllComposNormal()
		{
			ProgressBarCheckRss.Visibility = Visibility.Hidden;
			ButtonCheckRss.IsEnabled = true;
		}

		// --------------------------------------------------------------------
		// 最新情報確認コンポーネントを進捗中にする
		// --------------------------------------------------------------------
		private void MakeLatestComposRunning()
		{
			ProgressBarCheckRss.Visibility = Visibility.Visible;

			// ボタンは全部無効化
			ButtonCheckRss.IsEnabled = false;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void RadioButtonImport_Checked(Object oSender, RoutedEventArgs oRoutedEventArgs)
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
			mTargetExts.Clear();
			mTargetExts.AddRange(mYukaListerSettings.TargetExts);
			ListBoxTargetExts.ItemsSource = mTargetExts;
			UpdateButtonAddExt();
			UpdateButtonRemoveExt();

			// リスト出力タブ
			TextBoxListFolder.Text = mYukaListerSettings.ListOutputFolder;
			CheckBoxClearPrevList.IsChecked = mYukaListerSettings.ClearPrevList;

			// メンテナンスタブ
			CheckBoxCheckRss.IsChecked = mYukaListerSettings.CheckRss;
			CheckBoxSyncMusicInfoDb.IsChecked = mYukaListerSettings.SyncMusicInfoDb;
			TextBoxSyncServer.Text = mYukaListerSettings.SyncServer;
			TextBoxSyncAccount.Text = mYukaListerSettings.SyncAccount;
			PasswordBoxSyncPassword.Password = YlCommon.Decrypt(mYukaListerSettings.SyncPassword);
			UpdateSyncComponentsEnabled();

			// インポートタブ
			UpdateImportComponentsEnabled();
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void TextBoxImportAnisonInfoCsv_TextChanged(Object oSender, TextChangedEventArgs oTextChangedEventArgs)
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
			ButtonAddExt.IsEnabled = !String.IsNullOrEmpty(TextBoxTargetExt.Text);
		}

		// --------------------------------------------------------------------
		// 削除ボタン（拡張子）の状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonRemoveExt()
		{
			ButtonRemoveExt.IsEnabled = ListBoxTargetExts.SelectedIndex >= 0;
		}

		// --------------------------------------------------------------------
		// インポート関連コンポーネントの有効無効の切替
		// --------------------------------------------------------------------
		private void UpdateImportComponentsEnabled()
		{
			TextBoxImportYukaLister.IsEnabled = ButtonBrowseImportYukaLister.IsEnabled = (Boolean)RadioButtonImportYukaLister.IsChecked;
			TextBoxImportProgramCsv.IsEnabled = ButtonBrowseImportProgramCsv.IsEnabled = (Boolean)RadioButtonImportAnisonInfoCsv.IsChecked;
			TextBoxImportAnisonCsv.IsEnabled = ButtonBrowseImportAnisonCsv.IsEnabled = (Boolean)RadioButtonImportAnisonInfoCsv.IsChecked;
			TextBoxImportSfCsv.IsEnabled = ButtonBrowseImportSfCsv.IsEnabled = (Boolean)RadioButtonImportAnisonInfoCsv.IsChecked;
			TextBoxImportGameCsv.IsEnabled = ButtonBrowseImportGameCsv.IsEnabled = (Boolean)RadioButtonImportAnisonInfoCsv.IsChecked;
			TextBoxImportNicoKaraLister.IsEnabled = ButtonBrowseImportNicoKaraLister.IsEnabled = (Boolean)RadioButtonImportNicoKaraLister.IsChecked;
		}

		// --------------------------------------------------------------------
		// 同期関連コンポーネントの有効無効の切替
		// --------------------------------------------------------------------
		private void UpdateSyncComponentsEnabled()
		{
			Boolean aEnabled = (Boolean)CheckBoxSyncMusicInfoDb.IsChecked;
			TextBoxSyncServer.IsEnabled = aEnabled;
			TextBoxSyncAccount.IsEnabled = aEnabled;
			PasswordBoxSyncPassword.IsEnabled = aEnabled;
			ButtonReget.IsEnabled = aEnabled;
		}

		// --------------------------------------------------------------------
		// ちょちょいと自動更新の画面が何かしら表示された
		// --------------------------------------------------------------------
		private void WMUpdaterUIDisplayed()
		{
			MakeAllComposNormal();
		}

		// --------------------------------------------------------------------
		// メッセージハンドラ
		// --------------------------------------------------------------------
		private IntPtr WndProc(IntPtr oHWnd, Int32 oMsg, IntPtr oWParam, IntPtr oLParam, ref Boolean oHandled)
		{
			oHandled = true;
			switch ((Wm)oMsg)
			{
				case (Wm)UpdaterLauncher.WM_UPDATER_UI_DISPLAYED:
					WMUpdaterUIDisplayed();
					break;
				default:
					oHandled = false;
					break;
			}

			return IntPtr.Zero;
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "環境設定ウィンドウを開きます。");
				Init();

				SettingsToCompos();
				RadioButtonImportYukaLister.IsChecked = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "環境設定ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				CheckInput();
				ComposToSettings();
				mYukaListerSettings.Save();
				DialogResult = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
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

		private void ButtonBrowseImportProgramCsv_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				mOpenFileDialogMisc.Filter = IMPORT_ANISON_INFO_FILTER;
				mOpenFileDialogMisc.Title = YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV + " または "
						+ YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_ZIP;
				if (!(Boolean)mOpenFileDialogMisc.ShowDialog())
				{
					return;
				}
				TextBoxImportProgramCsv.Text = mOpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV + " 参照ボタンクリック時エラー：\n"
						+ oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseImportAnisonCsv_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				mOpenFileDialogMisc.Filter = IMPORT_ANISON_INFO_FILTER;
				mOpenFileDialogMisc.Title = YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV + " または "
						+ YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_ZIP;
				if (!(Boolean)mOpenFileDialogMisc.ShowDialog())
				{
					return;
				}
				TextBoxImportAnisonCsv.Text = mOpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV + " 参照ボタンクリック時エラー：\n"
						+ oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseImportSfCsv_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				mOpenFileDialogMisc.Filter = IMPORT_ANISON_INFO_FILTER;
				mOpenFileDialogMisc.Title = YlCommon.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV + " または "
						+ YlCommon.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_ZIP;
				if (!(Boolean)mOpenFileDialogMisc.ShowDialog())
				{
					return;
				}
				TextBoxImportSfCsv.Text = mOpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, YlCommon.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV + " 参照ボタンクリック時エラー：\n"
						+ oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseImportGameCsv_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				mOpenFileDialogMisc.Filter = IMPORT_ANISON_INFO_FILTER;
				mOpenFileDialogMisc.Title = YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV + " または "
						+ YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_ZIP;
				if (!(Boolean)mOpenFileDialogMisc.ShowDialog())
				{
					return;
				}
				TextBoxImportGameCsv.Text = mOpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV + " 参照ボタンクリック時エラー：\n"
						+ oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonImport_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ImportWindow aImportWindow = new ImportWindow(mYukaListerSettings, mLogWriter);
				aImportWindow.Owner = this;

				// ゆかりすたーでエクスポートしたファイルをインポート
				aImportWindow.IsYukaListerMode = (Boolean)RadioButtonImportYukaLister.IsChecked;
				aImportWindow.YklInfoPath = TextBoxImportYukaLister.Text;

				// anison.info CSV をインポート
				aImportWindow.IsAnisonInfoMode = (Boolean)RadioButtonImportAnisonInfoCsv.IsChecked;
				aImportWindow.ProgramCsvPath = TextBoxImportProgramCsv.Text;
				aImportWindow.AnisonCsvPath = TextBoxImportAnisonCsv.Text;
				aImportWindow.SfCsvPath = TextBoxImportSfCsv.Text;
				aImportWindow.GameCsvPath = TextBoxImportGameCsv.Text;

				// ニコカラりすたーでエクスポートしたファイルをインポート
				aImportWindow.IsNicoKaraListerMode = (Boolean)RadioButtonImportNicoKaraLister.IsChecked;
				aImportWindow.NklInfoPath = TextBoxImportNicoKaraLister.Text;

				aImportWindow.ShowDialog();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "インポートボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseImportNicoKaraLister_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				mOpenFileDialogMisc.Filter = "ニコカラりすたー情報ファイル|*" + YlCommon.FILE_EXT_NKLINFO;
				mOpenFileDialogMisc.Title = "ニコカラりすたー情報ファイル";
				if (!(Boolean)mOpenFileDialogMisc.ShowDialog())
				{
					return;
				}
				TextBoxImportNicoKaraLister.Text = mOpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ニコカラりすたー参照ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxSyncMusicInfoDb_Checked(object sender, RoutedEventArgs e)
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

		private void ButtonOutputList_Click(object sender, RoutedEventArgs e)
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

		private void ButtonBrowseListFolder_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (mOpenFileDialogOutputListFolder == null)
				{
					// フォルダー追加ダイアログを生成
					mOpenFileDialogOutputListFolder = new CommonOpenFileDialog();
					mOpenFileDialogOutputListFolder.IsFolderPicker = true;
				}

				if (mOpenFileDialogOutputListFolder.ShowDialog() != CommonFileDialogResult.Ok)
				{
					return;
				}
				TextBoxListFolder.Text = mOpenFileDialogOutputListFolder.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リスト出力先フォルダー参照ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxYukariConfigPathSeed_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				YukaListerSettings aTempYukaListerSettings = new YukaListerSettings();
				try
				{
					aTempYukaListerSettings.YukariConfigPathSeed = TextBoxYukariConfigPathSeed.Text;
					TextBoxYukariListFolder.Text = Path.GetDirectoryName(aTempYukaListerSettings.YukariListDbInDiskPath());
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

		private void ButtonYukariListSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				YukariOutputWriter aYukariOutputWriter = new YukariOutputWriter();
				aYukariOutputWriter.LogWriter = mLogWriter;
				aYukariOutputWriter.Owner = this;
				aYukariOutputWriter.OutputSettings.Load();

				if (!(Boolean)aYukariOutputWriter.ShowDialog())
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

		private void ButtonListSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				OutputWriter aSelectedOutputWriter = SelectedOutputWriter();

				if (!(Boolean)aSelectedOutputWriter.ShowDialog())
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

		private void HyperlinkHelp_RequestNavigate(object sender, RequestNavigateEventArgs e)
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

		private void TabItemImport_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				AcceptDragEnterIfFileExists(e);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "インポートタブドラッグエンター時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TabItemImport_Drop(object sender, DragEventArgs e)
		{
			try
			{
				Activate();

				String[] aDropFiles = (String[])e.Data.GetData(DataFormats.FileDrop, false);
				String aNotHandledFiles = null;
				foreach (String aDropFile in aDropFiles)
				{
					if (!File.Exists(aDropFile))
					{
						continue;
					}

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
							aNotHandledFiles += Path.GetFileName(aDropFile) + "\n";
						}
						RadioButtonImportAnisonInfoCsv.IsChecked = true;
					}
					else if (aExt == YlCommon.FILE_EXT_NKLINFO)
					{
						// ニコカラりすたーインポート
						TextBoxImportNicoKaraLister.Text = aDropFile;
						RadioButtonImportNicoKaraLister.IsChecked = true;
					}
					else
					{
						aNotHandledFiles += Path.GetFileName(aDropFile) + "\n";
					}

				}
				if (!String.IsNullOrEmpty(aNotHandledFiles))
				{
					throw new Exception("ドロップされたファイルの種類を自動判定できませんでした。\n参照ボタンからファイルを指定して下さい。\n" + aNotHandledFiles);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "インポートタブドラッグ＆ドロップ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxTargetExts_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

		private void TextBoxTargetExt_TextChanged(object sender, TextChangedEventArgs e)
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

		private void ButtonAddExt_Click(object sender, RoutedEventArgs e)
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

		private void ButtonRemoveExt_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// 選択されていない場合はボタンが押されないはずだが念のため
				if (ListBoxTargetExts.SelectedIndex < 0)
				{
					throw new Exception("削除したい拡張子を選択してください。");
				}

				// 削除
				mTargetExts.RemoveAt(ListBoxTargetExts.SelectedIndex);
				ListBoxTargetExts.Items.Refresh();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonBrowseYukariConfigPathSeed_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				mOpenFileDialogMisc.Filter = "ゆかり設定ファイル|config" + Common.FILE_EXT_INI;
				mOpenFileDialogMisc.Title = "ゆかり設定ファイル";
				mOpenFileDialogMisc.FileName = YlCommon.FILE_NAME_YUKARI_CONFIG;
				if (!(Boolean)mOpenFileDialogMisc.ShowDialog())
				{
					return;
				}
				TextBoxYukariConfigPathSeed.Text = mOpenFileDialogMisc.FileName;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ゆかり設定ファイル参照ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxCheckRss_Checked(object sender, RoutedEventArgs e)
		{
			try
			{
				if ((Boolean)CheckBoxCheckRss.IsChecked)
				{
					return;
				}
				if (MessageBox.Show("最新情報・更新版の確認を無効にすると、" + YlCommon.APP_NAME_J
						+ "の新版がリリースされても自動的にインストールされず、古いバージョンを使い続けることになります。\n"
						+ "本当に無効にしてもよろしいですか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning)
						!= MessageBoxResult.Yes)
				{
					CheckBoxCheckRss.IsChecked = true;
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "更新有効無効変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonCheckRss_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				MakeLatestComposRunning();
				if (!YlCommon.LaunchUpdater(true, true, mHandle, true, false))
				{
					MakeAllComposNormal();
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "最新情報確認時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonLog_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (mSaveFileDialogLog == null)
				{
					mSaveFileDialogLog = new SaveFileDialog();
					mSaveFileDialogLog.Filter = "ログファイル|*.lga";
				}
				mSaveFileDialogLog.FileName = "YukaListerLog_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
				if (!(Boolean)mSaveFileDialogLog.ShowDialog())
				{
					return;
				}

				// 環境情報保存
				YlCommon.LogEnvironmentInfo();

				ZipFile.CreateFromDirectory(YlCommon.SettingsPath(), mSaveFileDialogLog.FileName, CompressionLevel.Optimal, true);
				mLogWriter.ShowLogMessage(TraceEventType.Information, "ログ保存完了：\n" + mSaveFileDialogLog.FileName);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ログ保存時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonReget_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (SyncClient.RunningInstanceExists())
				{
					throw new Exception("現在、同期処理を実行中のため、合わせられません。\n同期処理が終了してから合わせてください。");
				}

				if (MessageBox.Show("ローカルの楽曲情報データベースを全て削除してから、内容をサーバーに合わせます。\n"
						+ "サーバーにアップロードしていないデータは全て失われます。\nよろしいですか？", "確認",
						MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
				{
					return;
				}

				mYukaListerSettings.LastSyncDownloadDate = 0.0;
				RegetSyncDataNeeded = true;
				mLogWriter.ShowLogMessage(TraceEventType.Information, "環境設定ウィンドウを閉じると処理を開始します。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "強制的に合わせる時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TabItemSettings_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				AcceptDragEnterIfFileExists(e);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "設定タブドラッグエンター時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TabItemSettings_Drop(object sender, DragEventArgs e)
		{
			try
			{
				Activate();

				String[] aDropFiles = (String[])e.Data.GetData(DataFormats.FileDrop, false);
				String aNotHandledFiles = null;
				foreach (String aDropFile in aDropFiles)
				{
					if (!File.Exists(aDropFile))
					{
						continue;
					}

					String aExt = Path.GetExtension(aDropFile).ToLower();
					if (aExt == Common.FILE_EXT_INI)
					{
						TextBoxYukariConfigPathSeed.Text = aDropFile;
					}
					else
					{
						aNotHandledFiles += Path.GetFileName(aDropFile) + "\n";
					}
				}
				if (!String.IsNullOrEmpty(aNotHandledFiles))
				{
					throw new Exception("ドロップされたファイルの種類を自動判定できませんでした。\n参照ボタンからファイルを指定して下さい。\n" + aNotHandledFiles);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "設定タブドラッグ＆ドロップ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxClearPrevList_Checked(object sender, RoutedEventArgs e)
		{
			try
			{
				if ((Boolean)CheckBoxClearPrevList.IsChecked)
				{
					return;
				}
				if (MessageBox.Show("前回のリストをクリアしないと、存在しないファイルがリストに残り齟齬が生じる可能性があります。\n"
						+ "本当にクリアしなくてよろしいですか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning)
						!= MessageBoxResult.Yes)
				{
					CheckBoxClearPrevList.IsChecked = true;
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "前回リストクリア変更時エラー：\n" + oExcep.Message);
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

		private void TabControlYukaListerSettings_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				switch (TabControlYukaListerSettings.SelectedIndex)
				{
					case 0:
					case 4:
						AcceptDragEnterIfFileExists(e);
						break;
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タブコントロールドラッグ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TabControlYukaListerSettings_Drop(object sender, DragEventArgs e)
		{
			try
			{
				switch (TabControlYukaListerSettings.SelectedIndex)
				{
					case 0:
						TabItemSettings_Drop(sender, e);
						break;
					case 4:
						TabItemImport_Drop(sender, e);
						break;
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タブコントロールドロップ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

	}
	// public partial class YukaListerSettingsWindow ___END___
}
// namespace YukaLister ___END___
