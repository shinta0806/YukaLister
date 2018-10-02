// ============================================================================
// 
// FolderSettings の設定を行うウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using YukaLister.Shared;
using Shinta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data.Linq;
using System.Threading.Tasks;

namespace YukaLister
{
	public partial class FormFolderSettings : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormFolderSettings(String oFolder, YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mFolder = oFolder;
			mYukaListerSettings = oYukaListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 設定対象フォルダー
		private String mFolder;

		// カテゴリー一覧
		private List<String> mCategoryNames;

		// 設定が変更された
		private Boolean mIsDirty = false;

		// 環境設定
		private YukaListerSettings mYukaListerSettings;

		// ログ
		private LogWriter mLogWriter;

		// フォルダー設定フォーム上で時間のかかるタスクが多重起動されるのを抑止する
		private Object mTaskLock = new Object();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// テキストボックスに入力されているファイル命名規則をリストボックスに追加
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void AddFileNameRule()
		{
			CheckFileNameRule(true);

			// 追加
			ListBoxFileNameRules.Items.Add(TextBoxFileNameRule.Text);
			ListBoxFileNameRules.SelectedIndex = ListBoxFileNameRules.Items.Count - 1;
			TextBoxFileNameRule.Text = null;
			mIsDirty = true;
		}

		// --------------------------------------------------------------------
		// テキストボックスに入力されている固定値項目をリストボックスに追加
		// --------------------------------------------------------------------
		private void AddFolderNameRule()
		{
			Int32 aListBoxIndex = ListBoxFolderNameRulesIndex();

			if (aListBoxIndex < 0)
			{
				// 未登録なので新規登録
				ListBoxFolderNameRules.Items.Add(FolderNameRuleFromComponent());
				ListBoxFolderNameRules.SelectedIndex = ListBoxFolderNameRules.Items.Count - 1;
			}
			else
			{
				// 既に登録済みなので置換
				ListBoxFolderNameRules.Items[aListBoxIndex] = FolderNameRuleFromComponent();
				ListBoxFolderNameRules.SelectedIndex = aListBoxIndex;
			}

			ComboBoxFolderNameRuleValue.SelectedIndex = -1;
			TextBoxFolderNameRuleValue.Text = null;
			mIsDirty = true;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：プレビュー一覧の編集ボタンがクリックされた
		// --------------------------------------------------------------------
		private void ButtonEditInfoClicked(Int32 oRowIndex)
		{
			if (oRowIndex < 0)
			{
				return;
			}
			String aFileName = (String)DataGridViewPreview.Rows[oRowIndex].Cells[(Int32)PreviewColumns.File].Value;

			// ファイル命名規則とフォルダー固定値を適用
			FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings(mFolder);
			FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
			Dictionary<String, String> aDic = YlCommon.MatchFileNameRulesAndFolderRule
					(Path.GetFileNameWithoutExtension(aFileName), aFolderSettingsInMemory);

			// 楽曲名が取得できていない場合は編集不可
			if (String.IsNullOrEmpty(aDic[YlCommon.RULE_VAR_TITLE]))
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名から楽曲名を取得できていないため、編集できません。\nファイル命名規則を確認して下さい。");
				return;
			}

			using (FormEditMusicInfo aFormEditMusicInfo = new FormEditMusicInfo(aFileName, aDic, mYukaListerSettings, mLogWriter))
			{
				aFormEditMusicInfo.ShowDialog(this);
			}
		}

		// --------------------------------------------------------------------
		// テキストボックスに入力されているファイル命名規則が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckFileNameRule(Boolean oCheckSelectedLine)
		{
			// 入力が空の場合はボタンは押されないはずだが念のため
			if (String.IsNullOrEmpty(TextBoxFileNameRule.Text))
			{
				throw new Exception("命名規則が入力されていません。");
			}

			// 変数が含まれているか
			if (TextBoxFileNameRule.Text.IndexOf(YlCommon.RULE_VAR_BEGIN) < 0)
			{
				throw new Exception("命名規則に <変数> が含まれていません。");
			}

			// 既存のものと重複していないか
			foreach (String aRule in ListBoxFileNameRules.Items)
			{
				if (TextBoxFileNameRule.Text == aRule)
				{
					throw new Exception("同じ命名規則が既に追加されています。");
				}
			}

			// 変数・ワイルドカードが隣り合っているとうまく解析できない
			String aNormalizedNewRule = NormalizeRule(TextBoxFileNameRule.Text);
			if (aNormalizedNewRule.IndexOf(YlCommon.RULE_VAR_ANY + YlCommon.RULE_VAR_ANY) >= 0)
			{
				throw new Exception("<変数> や " + YlCommon.RULE_VAR_ANY + " が連続していると正常にファイル名を解析できません。");
			}

			// 競合する命名規則が無いか
			for (Int32 i = 0; i < ListBoxFileNameRules.Items.Count; i++)
			{
				if (ListBoxFileNameRules.GetSelected(i) && !oCheckSelectedLine)
				{
					continue;
				}

				if (NormalizeRule((String)ListBoxFileNameRules.Items[i]) == aNormalizedNewRule)
				{
					throw new Exception("競合する命名規則が既に追加されています：\n" + (String)ListBoxFileNameRules.Items[i]);
				}
			}
		}

		// --------------------------------------------------------------------
		// コンポーネントの値を設定に格納
		// --------------------------------------------------------------------
		private FolderSettingsInDisk ComposToSettings()
		{
			FolderSettingsInDisk aFolderSettings = new FolderSettingsInDisk();

			aFolderSettings.AppVer = YlCommon.APP_VER;

			foreach (String aItem in ListBoxFileNameRules.Items)
			{
				aFolderSettings.FileNameRules.Add(aItem);
			}

			foreach (String aItem in ListBoxFolderNameRules.Items)
			{
				aFolderSettings.FolderNameRules.Add(aItem);
			}

			return aFolderSettings;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー：変数名メニューがクリックされた
		// --------------------------------------------------------------------
		private void ContextMenuVarNamesItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				String aKey = FindRuleVarName(aItem.Text);
				String aWrappedVarName = WrapVarName(aKey);
				Int32 aSelectionStart = TextBoxFileNameRule.SelectionStart;

				// カーソル位置に挿入
				TextBoxFileNameRule.Text = TextBoxFileNameRule.Text.Substring(0, aSelectionStart) + aWrappedVarName
						+ TextBoxFileNameRule.Text.Substring(aSelectionStart + TextBoxFileNameRule.SelectionLength);

				// <-> ボタンにフォーカスが移っているので戻す
				TextBoxFileNameRule.Focus();
				TextBoxFileNameRule.SelectionStart = aSelectionStart + aWrappedVarName.Length;
				TextBoxFileNameRule.SelectionLength = 0;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "変数メニュークリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}

		}

		// --------------------------------------------------------------------
		// ファイル命名規則の変数の表示用文字列を生成
		// --------------------------------------------------------------------
		private List<String> CreateRuleVarLabels()
		{
			List<String> aLabels = new List<String>();
			TextInfo aTextInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
			Dictionary<String, String> aVarMap = YlCommon.CreateRuleDictionaryWithDescription();
			foreach (KeyValuePair<String, String> aVar in aVarMap)
			{
				String aKey;
				if (aVar.Key == YlCommon.RULE_VAR_ANY)
				{
					aKey = aVar.Key;
				}
				else
				{
					aKey = YlCommon.RULE_VAR_BEGIN + aTextInfo.ToTitleCase(aVar.Key) + YlCommon.RULE_VAR_END;
				}
				aLabels.Add(aKey + "（" + aVar.Value + "）");
			}
			return aLabels;
		}

		// --------------------------------------------------------------------
		// UI 無効化（時間のかかる処理実行時用）
		// --------------------------------------------------------------------
		private void DisableComponents()
		{
			Invoke(new Action(() =>
			{
				TabControlRules.Enabled = false;
				ButtonPreview.Enabled = false;
				ButtonDeleteSettings.Enabled = false;
				ButtonOK.Enabled = false;
			}));
		}

		// --------------------------------------------------------------------
		// UI 有効化
		// --------------------------------------------------------------------
		private void EnableComponents()
		{
			Invoke(new Action(() =>
			{
				TabControlRules.Enabled = true;
				ButtonPreview.Enabled = true;

				// ButtonDeleteSettings は状況によって状態が異なる
				UpdateSettingsFileStatus();

				ButtonOK.Enabled = true;
			}));
		}

		// --------------------------------------------------------------------
		// 文字列の中に含まれている命名規則の変数名を返す
		// 文字列の中には <Name> 形式で変数名を含んでいる必要がある
		// 返す変数名には <> は含まない
		// --------------------------------------------------------------------
		private String FindRuleVarName(String oString)
		{
			Dictionary<String, String> aVarMap = YlCommon.CreateRuleDictionary();
			foreach (String aKey in aVarMap.Keys)
			{
				if (oString.IndexOf(YlCommon.RULE_VAR_BEGIN + aKey + YlCommon.RULE_VAR_END, StringComparison.CurrentCultureIgnoreCase) >= 0)
				{
					return aKey;
				}
			}
			if (oString.IndexOf(YlCommon.RULE_VAR_ANY) >= 0)
			{
				return YlCommon.RULE_VAR_ANY;
			}
			return null;
		}

		// --------------------------------------------------------------------
		// 入力された固定値
		// --------------------------------------------------------------------
		private String FolderNameRuleFromComponent()
		{
			String aKey = FindRuleVarName((String)ComboBoxFolderNameRuleName.Items[ComboBoxFolderNameRuleName.SelectedIndex]);
			String aValue;
			if (ComboBoxFolderNameRuleValue.Visible)
			{
				aValue = (String)ComboBoxFolderNameRuleValue.SelectedItem;
			}
			else
			{
				aValue = TextBoxFolderNameRuleValue.Text;
			}
			return WrapVarName(aKey) + "=" + aValue;
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			UpdateTitleBar();

			// ラベル
			LabelFolder.Text = YlCommon.ShortenPath(mFolder);

			// <-> ボタン
			List<String> aLabels = CreateRuleVarLabels();
			foreach (String aLabel in aLabels)
			{
				// オンボーカル・オフボーカルは除外
				if (aLabel.IndexOf(YlCommon.RULE_VAR_ON_VOCAL, StringComparison.OrdinalIgnoreCase) < 0
						&& aLabel.IndexOf(YlCommon.RULE_VAR_OFF_VOCAL, StringComparison.OrdinalIgnoreCase) < 0)
				{
					ContextMenuVarNames.Items.Add(aLabel, null, ContextMenuVarNamesItem_Click);
				}
			}

			// 固定値項目
			foreach (String aLabel in aLabels)
			{
				// * は除外
				if (aLabel.IndexOf(YlCommon.RULE_VAR_ANY) < 0)
				{
					ComboBoxFolderNameRuleName.Items.Add(aLabel);
				}
			}
			ComboBoxFolderNameRuleName.SelectedIndex = 0;

			// カテゴリー一覧
			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				mCategoryNames = YlCommon.SelectCategoryNames(aConnection);
			}

			// データグリッドビュー
			InitDataGridView();

			// 設計時サイズ以下にできないようにする
			MinimumSize = Size;

			Common.CascadeForm(this);
		}

		// --------------------------------------------------------------------
		// データグリッドビュー初期化
		// --------------------------------------------------------------------
		private void InitDataGridView()
		{
			// ファイル
			ColumnFile.Width = 250;

			// 項目と値
			ColumnAnalyze.Width = 400;

			// 編集
			ColumnEdit.Width = 50;
		}

		// --------------------------------------------------------------------
		// 編集する必要がありそうなファイルに飛ぶ
		// （楽曲名・タイアップ名が楽曲情報データベースに未登録なファイル）
		// --------------------------------------------------------------------
		private void JumpToNextCandidate(Object oDummy)
		{
			try
			{
				// 準備
				DisableComponents();
				SetCursor(Cursors.WaitCursor);

				Invoke(new Action(() =>
				{
					Int32 aRowIndex = -1;
					if (DataGridViewPreview.SelectedRows.Count > 0)
					{
						aRowIndex = DataGridViewPreview.SelectedRows[0].Index;
					}

					// マッチ準備
					FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings(mFolder);
					FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);

					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						using (DataContext aContext = new DataContext(aConnection))
						{
							for (; ; )
							{
								aRowIndex++;
								if (aRowIndex >= DataGridViewPreview.RowCount)
								{
									mLogWriter.ShowLogMessage(TraceEventType.Information, "ファイル名から取得した楽曲情報・番組情報が楽曲情報データベースに未登録のファイルは見つかりませんでした。");
									DataGridViewPreview.ClearSelection();
									return;
								}

								// ファイル命名規則とフォルダー固定値を適用
								Dictionary<String, String> aDic = YlCommon.MatchFileNameRulesAndFolderRule(
										Path.GetFileNameWithoutExtension((String)DataGridViewPreview.Rows[aRowIndex].Cells[(Int32)PreviewColumns.File].Value), aFolderSettingsInMemory);

								// 楽曲名が空かどうか
								if (String.IsNullOrEmpty(aDic[YlCommon.RULE_VAR_TITLE]))
								{
									break;
								}

								// 楽曲名が楽曲情報データベースと不一致かどうか
								String aSongNameOrigin = aDic[YlCommon.RULE_VAR_TITLE];
								List<TSongAlias> aSongAliases = YlCommon.SelectSongAliasesByAlias(aContext, aDic[YlCommon.RULE_VAR_TITLE]);
								if (aSongAliases.Count > 0)
								{
									TSong aSongOrigin = YlCommon.SelectSongById(aContext, aSongAliases[0].OriginalId);
									if (aSongOrigin != null)
									{
										aSongNameOrigin = aSongOrigin.Name;
									}
								}
								List<TSong> aSongs = YlCommon.SelectSongsByName(aContext, aSongNameOrigin);
								if (aSongs.Count == 0)
								{
									break;
								}

								// 番組名がある場合、番組名が楽曲情報データベースと不一致かどうか
								if (!String.IsNullOrEmpty(aDic[YlCommon.RULE_VAR_PROGRAM]))
								{
									String aProgramNameOrigin = aDic[YlCommon.RULE_VAR_PROGRAM];
									List<TTieUpAlias> aTieUpAliases = YlCommon.SelectTieUpAliasesByAlias(aContext, aDic[YlCommon.RULE_VAR_PROGRAM]);
									if (aTieUpAliases.Count > 0)
									{
										TTieUp aTieUpOrigin = YlCommon.SelectTieUpById(aContext, aTieUpAliases[0].OriginalId);
										if (aTieUpOrigin != null)
										{
											aProgramNameOrigin = aTieUpOrigin.Name;
										}
									}
									List<TTieUp> aTieUps = YlCommon.SelectTieUpsByName(aContext, aProgramNameOrigin);
									if (aTieUps.Count == 0)
									{
										break;
									}
								}
							}
						}
					}

					DataGridViewPreview.Rows[aRowIndex].Selected = true;
					YlCommon.ScrollDataGridViewIfNeeded(DataGridViewPreview, aRowIndex);
				}));
			}
			catch (OperationCanceledException)
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "未登録検出を中止しました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "未登録検出時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 後片付け
				SetCursor(Cursors.Default);
				EnableComponents();
			}
		}

		// --------------------------------------------------------------------
		// コンボボックスで選択されている項目は、リストボックスの何番目に登録されているか
		// --------------------------------------------------------------------
		private Int32 ListBoxFolderNameRulesIndex()
		{
			String aKey = FindRuleVarName((String)ComboBoxFolderNameRuleName.Items[ComboBoxFolderNameRuleName.SelectedIndex]);
			String aVarName = WrapVarName(aKey);
			for (Int32 i = 0; i < ListBoxFolderNameRules.Items.Count; i++)
			{
				if (((String)ListBoxFolderNameRules.Items[i]).IndexOf(aVarName) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		// --------------------------------------------------------------------
		// 命名規則の変数部分を全てワイルドカードにする
		// --------------------------------------------------------------------
		private String NormalizeRule(String oRule)
		{
			return Regex.Replace(oRule, @"\<.*?\>", YlCommon.RULE_VAR_ANY);
		}

#if false
		// --------------------------------------------------------------------
		// FormNicoKaraLister.ProgramOrigin() の CSV 版
		// 番組 ID 検索アルゴリズムは CreateInfoDbProgramAliasTableInsert() と同様とする
		// --------------------------------------------------------------------
		private String ProgramOriginCsv(String oProgram)
		{
			List<String> aProgramAliasRecord = YlCommon.FindCsvRecord(mProgramAliasCsvs, (Int32)ProgramAliasCsvColumns.Alias, oProgram);
			if (aProgramAliasRecord == null)
			{
				return oProgram;
			}

			// 番組名が指定されたものとして番組 ID を検索
			List<String> aProgramRecord;
			if (String.IsNullOrEmpty(aProgramAliasRecord[(Int32)ProgramAliasCsvColumns.ForceId]))
			{
				aProgramRecord = YlCommon.FindCsvRecord(mProgramCsvs, (Int32)ProgramCsvColumns.Name, aProgramAliasRecord[(Int32)ProgramAliasCsvColumns.NameOrId]);
				if (aProgramRecord != null)
				{
					return aProgramRecord[(Int32)ProgramCsvColumns.Name];
				}
			}

			// 番組 ID が指定されたものとして番組 ID を検索
			aProgramRecord = YlCommon.FindCsvRecord(mProgramCsvs, (Int32)ProgramCsvColumns.Id, aProgramAliasRecord[(Int32)ProgramAliasCsvColumns.NameOrId]);
			if (aProgramRecord != null)
			{
				return aProgramRecord[(Int32)ProgramCsvColumns.Name];
			}

			return oProgram;
		}
#endif

		// --------------------------------------------------------------------
		// 設定が更新されていれば保存
		// ＜例外＞ OperationCanceledException, Exception
		// --------------------------------------------------------------------
		private void SaveSettingsIfNeeded()
		{
			// 設定途中のものを確認
			if (!String.IsNullOrEmpty(TextBoxFileNameRule.Text))
			{
				switch (MessageBox.Show("ファイル命名規則に入力中の\n" + TextBoxFileNameRule.Text + "\nはまだ命名規則として追加されていません。\n追加しますか？",
						"確認", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
				{
					case DialogResult.Yes:
						AddFileNameRule();
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						throw new OperationCanceledException("保存を中止しました。");
				}
			}
			String aFolderNameRuleValue = ComboBoxFolderNameRuleValue.Visible ? (String)ComboBoxFolderNameRuleValue.SelectedItem : TextBoxFolderNameRuleValue.Text;
			if (!String.IsNullOrEmpty(aFolderNameRuleValue))
			{
				switch (MessageBox.Show("固定値項目に入力中の\n" + aFolderNameRuleValue + "\nはまだ固定値として追加されていません。\n追加しますか？",
						"確認", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
				{
					case DialogResult.Yes:
						AddFolderNameRule();
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						throw new OperationCanceledException("保存を中止しました。");
				}
			}

			if (!mIsDirty)
			{
				return;
			}

			FolderSettingsInDisk aFolderSettings = ComposToSettings();

			// 保存
			String aYukaListerConfigPath = mFolder + "\\" + YlCommon.FILE_NAME_YUKA_LISTER_CONFIG;
			FileAttributes aPrevAttr = new FileAttributes();
			Boolean aHasPrevAttr = false;
			if (File.Exists(aYukaListerConfigPath))
			{
				aPrevAttr = File.GetAttributes(aYukaListerConfigPath);
				aHasPrevAttr = true;

				// 隠しファイルを直接上書きできないので一旦削除する
				File.Delete(aYukaListerConfigPath);
			}
			Common.Serialize(aYukaListerConfigPath, aFolderSettings);
			if (aHasPrevAttr)
			{
				File.SetAttributes(aYukaListerConfigPath, aPrevAttr);
			}

			// ニコカラりすたーの設定ファイルがある場合は削除
			if (File.Exists(mFolder + "\\" + YlCommon.FILE_NAME_NICO_KARA_LISTER_CONFIG))
			{
				try
				{
					File.Delete(mFolder + "\\" + YlCommon.FILE_NAME_NICO_KARA_LISTER_CONFIG);
				}
				catch (Exception)
				{
				}
			}
			UpdateSettingsFileStatus();
		}

		// --------------------------------------------------------------------
		// カーソル形状の設定
		// --------------------------------------------------------------------
		private void SetCursor(Cursor oCursor)
		{
			Invoke(new Action(() =>
			{
				Capture = true;
				Cursor = oCursor;
				Capture = false;
			}));
		}

		// --------------------------------------------------------------------
		// 設定をコンポーネントに反映
		// --------------------------------------------------------------------
		private void SettingsToCompos()
		{
			// クリア
			ListBoxFileNameRules.Items.Clear();
			ListBoxFolderNameRules.Items.Clear();

			// 設定
			FolderSettingsInDisk aSettings = YlCommon.LoadFolderSettings(mFolder);
			foreach (String aFileNameRule in aSettings.FileNameRules)
			{
				ListBoxFileNameRules.Items.Add(aFileNameRule);
			}
			foreach (String aFolderNameRule in aSettings.FolderNameRules)
			{
				ListBoxFolderNameRules.Items.Add(aFolderNameRule);
			}
		}

#if false
		// --------------------------------------------------------------------
		// FormNicoKaraLister.SongOrigin() の CSV 版
		// 楽曲 ID 検索アルゴリズムは CreateInfoDbSongAliasTableInsert() と同様とする
		// --------------------------------------------------------------------
		private String SongOriginCsv(String oTitle)
		{
			List<String> aSongAliasRecord = YlCommon.FindCsvRecord(mSongAliasCsvs, (Int32)SongAliasCsvColumns.Alias, oTitle);
			if (aSongAliasRecord == null)
			{
				return oTitle;
			}

			// 楽曲名が指定されたものとして楽曲 ID を検索
			List<String> aSongRecord;
			if (String.IsNullOrEmpty(aSongAliasRecord[(Int32)SongAliasCsvColumns.ForceId]))
			{
				aSongRecord = YlCommon.FindCsvRecord(mSongCsvs, (Int32)SongCsvColumns.Name, aSongAliasRecord[(Int32)SongAliasCsvColumns.NameOrId]);
				if (aSongRecord != null)
				{
					return aSongRecord[(Int32)SongCsvColumns.Name];
				}
			}

			// 楽曲 ID が指定されたものとして楽曲 ID を検索
			aSongRecord = YlCommon.FindCsvRecord(mSongCsvs, (Int32)SongCsvColumns.Id, aSongAliasRecord[(Int32)SongAliasCsvColumns.NameOrId]);
			if (aSongRecord != null)
			{
				return aSongRecord[(Int32)SongCsvColumns.Name];
			}

			return oTitle;
		}
#endif

		// --------------------------------------------------------------------
		// リストボックスの 2 つのアイテムを入れ替える
		// --------------------------------------------------------------------
		private void SwapListItem(ListBox oListBox, Int32 oLhsIndex, Int32 oRhsIndex)
		{
			String aTmp = (String)oListBox.Items[oLhsIndex];
			oListBox.Items[oLhsIndex] = oListBox.Items[oRhsIndex];
			oListBox.Items[oRhsIndex] = aTmp;
		}

		// --------------------------------------------------------------------
		// データグリッドビューを更新
		// --------------------------------------------------------------------
		private void UpdateButtonJump()
		{
			ButtonJump.Enabled = DataGridViewPreview.Rows.Count > 0;
		}

		// --------------------------------------------------------------------
		// データグリッドビューを更新
		// --------------------------------------------------------------------
		private void UpdateDataGridViewPreview(Object oDummy)
		{
			try
			{
				// 準備
				DisableComponents();
				SetCursor(Cursors.WaitCursor);

				Invoke(new Action(() =>
				{
					// クリア
					DataGridViewPreview.Rows.Clear();

					// 検索
					String[] aAllPathes = Directory.GetFiles(mFolder);

					// マッチをリストに追加
					FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings(mFolder);
					FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
					Dictionary<String, String> aRuleMap = YlCommon.CreateRuleDictionaryWithDescription();
					foreach (String aPath in aAllPathes)
					{
						if (!mYukaListerSettings.TargetExts.Contains(Path.GetExtension(aPath).ToLower()))
						{
							continue;
						}

						// ファイル命名規則とフォルダー固定値を適用
						Dictionary<String, String> aDic = YlCommon.MatchFileNameRulesAndFolderRule(Path.GetFileNameWithoutExtension(aPath), aFolderSettingsInMemory);

						// DGV 追加
						DataGridViewPreview.Rows.Add();
						Int32 aIndex = DataGridViewPreview.Rows.Count - 1;

						// ファイル
						DataGridViewPreview.Rows[aIndex].Cells[(Int32)PreviewColumns.File].Value = Path.GetFileName(aPath);

						// 項目と値
						StringBuilder aSB = new StringBuilder();
						foreach (KeyValuePair<String, String> aKvp in aDic)
						{
							if (aKvp.Key != YlCommon.RULE_VAR_ANY && !String.IsNullOrEmpty(aKvp.Value))
							{
								aSB.Append(aRuleMap[aKvp.Key] + "=" + aKvp.Value + ", ");
							}
						}
						DataGridViewPreview.Rows[aIndex].Cells[(Int32)PreviewColumns.Matches].Value = aSB.ToString();

						// 編集
						DataGridViewPreview.Rows[aIndex].Cells[(Int32)PreviewColumns.Edit].Value = "編集";
					}

					// 選択解除
					DataGridViewPreview.ClearSelection();

					// 次の編集候補ボタン
					UpdateButtonJump();
				}));
			}
			catch (OperationCanceledException)
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ファイル検索結果更新を中止しました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル検索結果更新更新時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 後片付け
				SetCursor(Cursors.Default);
				EnableComponents();
			}
		}

		// --------------------------------------------------------------------
		// ComboBoxFolderNameRuleName の状況に紐付くコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateFolderNameRuleComponents()
		{
			// リストボックスにコンボボックスと同じ項目があれば選択する
			ListBoxFolderNameRules.SelectedIndex = ListBoxFolderNameRulesIndex();

			String aRuleName = FindRuleVarName((String)ComboBoxFolderNameRuleName.Items[ComboBoxFolderNameRuleName.SelectedIndex]);
			if (aRuleName == YlCommon.RULE_VAR_CATEGORY || aRuleName == YlCommon.RULE_VAR_ON_VOCAL || aRuleName == YlCommon.RULE_VAR_OFF_VOCAL)
			{
				// 値をコンボボックスから入力する
				ComboBoxFolderNameRuleValue.Visible = true;
				TextBoxFolderNameRuleValue.Visible = false;

				// 選択肢の準備
				ComboBoxFolderNameRuleValue.Items.Clear();
				switch (aRuleName)
				{
					case YlCommon.RULE_VAR_CATEGORY:
						if (mCategoryNames != null)
						{
							foreach (String aCategoryName in mCategoryNames)
							{
								ComboBoxFolderNameRuleValue.Items.Add(aCategoryName);
							}
						}
						break;
					case YlCommon.RULE_VAR_ON_VOCAL:
					case YlCommon.RULE_VAR_OFF_VOCAL:
						ComboBoxFolderNameRuleValue.Items.Add(YlCommon.RULE_VALUE_VOCAL_DEFAULT.ToString());
						break;
					default:
						Debug.Assert(false, "UpdateFolderNameRuleComponents() bad aRuleName");
						break;
				}
				ComboBoxFolderNameRuleValue.SelectedIndex = -1;
			}
			else
			{
				// 値をテキストボックスから入力する
				ComboBoxFolderNameRuleValue.Visible = false;
				TextBoxFolderNameRuleValue.Visible = true;
			}
		}

		// --------------------------------------------------------------------
		// 設定ファイルの状況を表示
		// --------------------------------------------------------------------
		private void UpdateSettingsFileStatus()
		{
			FolderSettingsStatus aStatus = YlCommon.DetectFolderSettingsStatus(mFolder);

			switch (aStatus)
			{
				case FolderSettingsStatus.None:
					LabelSettingsFileStatus.Text = "このフォルダーの設定がありません。";
					ButtonDeleteSettings.Enabled = false;
					break;
				case FolderSettingsStatus.Set:
					LabelSettingsFileStatus.Text = "このフォルダーは設定済みです。";
					ButtonDeleteSettings.Enabled = true;
					break;
				case FolderSettingsStatus.Inherit:
					LabelSettingsFileStatus.Text = "親フォルダーの設定を参照しています（設定を変更しても親フォルダーには影響ありません）。";
					ButtonDeleteSettings.Enabled = false;
					break;
				default:
					Debug.Assert(false, "UpdateLabelSettingsFileStatus() bad FolderSettingsStatus");
					break;
			}

			LinkLabelHelp.Left = LabelSettingsFileStatus.Right;
		}

		// --------------------------------------------------------------------
		// タブ内のコンポーネントの状態を更新
		// --------------------------------------------------------------------
		private void UpdateTabControlRules()
		{
			// ファイル名命名規則
			ButtonAddFileNameRule.Enabled = !String.IsNullOrEmpty(TextBoxFileNameRule.Text);
			ButtonReplaceFileNameRule.Enabled = !String.IsNullOrEmpty(TextBoxFileNameRule.Text) && (ListBoxFileNameRules.SelectedIndex >= 0);
			ButtonDeleteFileNameRule.Enabled = (ListBoxFileNameRules.SelectedIndex >= 0);
			ButtonUpFileNameRule.Enabled = (ListBoxFileNameRules.SelectedIndex > 0);
			ButtonDownFileNameRule.Enabled = (0 <= ListBoxFileNameRules.SelectedIndex && ListBoxFileNameRules.SelectedIndex < ListBoxFileNameRules.Items.Count - 1);

			// フォルダー固定値
			if (ComboBoxFolderNameRuleValue.Visible)
			{
				ButtonAddFolderNameRule.Enabled = !String.IsNullOrEmpty((String)ComboBoxFolderNameRuleValue.SelectedItem);
			}
			else
			{
				ButtonAddFolderNameRule.Enabled = !String.IsNullOrEmpty(TextBoxFolderNameRuleValue.Text);
			}
			ButtonDeleteFolderNameRule.Enabled = (ListBoxFolderNameRules.SelectedIndex >= 0);
			ButtonUpFolderNameRule.Enabled = (ListBoxFolderNameRules.SelectedIndex > 0);
			ButtonDownFolderNameRule.Enabled = (0 <= ListBoxFolderNameRules.SelectedIndex && ListBoxFolderNameRules.SelectedIndex < ListBoxFolderNameRules.Items.Count - 1);
		}

		// --------------------------------------------------------------------
		// タイトルバーを更新
		// --------------------------------------------------------------------
		private void UpdateTitleBar()
		{
			Text = "フォルダー設定";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif
		}

		// --------------------------------------------------------------------
		// 変数名を <> で囲む
		// --------------------------------------------------------------------
		private String WrapVarName(String oVarName)
		{
			if (oVarName == YlCommon.RULE_VAR_ANY)
			{
				return YlCommon.RULE_VAR_ANY;
			}
			else
			{
				TextInfo aTextInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
				return YlCommon.RULE_VAR_BEGIN + aTextInfo.ToTitleCase(oVarName) + YlCommon.RULE_VAR_END;
			}
		}


		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormFolderSettings_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "フォルダー設定ウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormFolderSettings_Shown(object sender, EventArgs e)
		{
			try
			{
				UpdateSettingsFileStatus();
				SettingsToCompos();
				UpdateTabControlRules();
				UpdateFolderNameRuleComponents();
				UpdateButtonJump();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ウィンドウ表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonVar_Click(object sender, EventArgs e)
		{
			try
			{
				ContextMenuVarNames.Show(Cursor.Position);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "変数メニュー表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonAddFileNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				AddFileNameRule();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則追加時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxFileNameRule_TextChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則テキストボックス変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFileNameRules_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則リストボックス選択時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonReplaceFileNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				CheckFileNameRule(false);

				// 置換
				ListBoxFileNameRules.Items[ListBoxFileNameRules.SelectedIndex] = TextBoxFileNameRule.Text;
				TextBoxFileNameRule.Text = null;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則置換時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDeleteFileNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				ListBoxFileNameRules.Items.RemoveAt(ListBoxFileNameRules.SelectedIndex);
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則削除時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonUpFileNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				SwapListItem(ListBoxFileNameRules, ListBoxFileNameRules.SelectedIndex - 1, ListBoxFileNameRules.SelectedIndex);
				ListBoxFileNameRules.SelectedIndex -= 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則順番繰り上げ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDownFileNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				SwapListItem(ListBoxFileNameRules, ListBoxFileNameRules.SelectedIndex + 1, ListBoxFileNameRules.SelectedIndex);
				ListBoxFileNameRules.SelectedIndex += 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則順番繰り下げ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonPreview_Click(object sender, EventArgs e)
		{
			try
			{
				// 保存
				SaveSettingsIfNeeded();

				// 検索（async を待機しない）
				Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(UpdateDataGridViewPreview, mTaskLock, null);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル検索時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxFolderNameRuleValue_TextChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目テキストボックス変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFolderNameRules_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目リストボックス選択時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonAddFolderNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				AddFolderNameRule();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目追加時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ComboBoxFolderNameRuleName_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateFolderNameRuleComponents();
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目名選択時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDeleteFolderNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				ListBoxFolderNameRules.Items.RemoveAt(ListBoxFolderNameRules.SelectedIndex);
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目削除時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonUpFolderNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				SwapListItem(ListBoxFolderNameRules, ListBoxFolderNameRules.SelectedIndex - 1, ListBoxFolderNameRules.SelectedIndex);
				ListBoxFolderNameRules.SelectedIndex -= 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目順番繰り上げ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDownFolderNameRule_Click(object sender, EventArgs e)
		{
			try
			{
				SwapListItem(ListBoxFolderNameRules, ListBoxFolderNameRules.SelectedIndex + 1, ListBoxFolderNameRules.SelectedIndex);
				ListBoxFolderNameRules.SelectedIndex += 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目順番繰り下げ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFileNameRules_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				if (ListBoxFileNameRules.SelectedIndex < 0)
				{
					return;
				}
				TextBoxFileNameRule.Text = (String)ListBoxFileNameRules.Items[ListBoxFileNameRules.SelectedIndex];
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則ダブルクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFolderNameRules_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				if (ListBoxFolderNameRules.SelectedIndex < 0)
				{
					return;
				}

				// コンボボックス設定
				String aKey = FindRuleVarName((String)ListBoxFolderNameRules.Items[ListBoxFolderNameRules.SelectedIndex]);
				if (String.IsNullOrEmpty(aKey))
				{
					return;
				}
				String aVarName = WrapVarName(aKey);
				for (Int32 i = 0; i < ComboBoxFolderNameRuleName.Items.Count; i++)
				{
					if (ComboBoxFolderNameRuleName.Items[i].ToString().IndexOf(aVarName) == 0)
					{
						ComboBoxFolderNameRuleName.SelectedIndex = i;
						break;
					}
				}

				// 値設定
				String aRule = (String)ListBoxFolderNameRules.Items[ListBoxFolderNameRules.SelectedIndex];
				Int32 aEqualPos = aRule.IndexOf('=');
				String aValue = aRule.Substring(aEqualPos + 1);
				if (ComboBoxFolderNameRuleValue.Visible)
				{
					Int32 aIndex = ComboBoxFolderNameRuleValue.Items.IndexOf(aValue);
					ComboBoxFolderNameRuleValue.SelectedIndex = aIndex;
				}
				else
				{
					TextBoxFolderNameRuleValue.Text = aValue;
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目ダブルクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonHelp_Click(object sender, EventArgs e)
		{
			try
			{
				YlCommon.ShowHelp("FolderSettei");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ヘルプボタン（フォルダー設定フォーム）クリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDeleteSettings_Click(object sender, EventArgs e)
		{
			try
			{
				if (MessageBox.Show("フォルダー設定を削除します。\nよろしいですか？", "確認",
						MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
				{
					return;
				}

				if (File.Exists(mFolder + "\\" + YlCommon.FILE_NAME_YUKA_LISTER_CONFIG))
				{
					File.Delete(mFolder + "\\" + YlCommon.FILE_NAME_YUKA_LISTER_CONFIG);
				}
				if (File.Exists(mFolder + "\\" + YlCommon.FILE_NAME_NICO_KARA_LISTER_CONFIG))
				{
					File.Delete(mFolder + "\\" + YlCommon.FILE_NAME_NICO_KARA_LISTER_CONFIG);
				}

				// UI に反映（フォーム Shown() と同様の処理）
				UpdateSettingsFileStatus();
				SettingsToCompos();
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "設定削除ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				SaveSettingsIfNeeded();
				DialogResult = DialogResult.OK;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定保存時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridViewPreview_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				if (e.ColumnIndex == (Int32)PreviewColumns.Edit)
				{
					ButtonEditInfoClicked(e.RowIndex);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー一覧クリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormFolderSettings_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "フォルダー設定ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ウィンドウクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonJump_Click(object sender, EventArgs e)
		{
			try
			{
				// async を待機しない
				Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(JumpToNextCandidate, mTaskLock, null);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "未登録検出クリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void DataGridViewPreview_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
		}

		private void DataGridViewPreview_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				ButtonEditInfoClicked(e.RowIndex);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー一覧ダブルクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ComboBoxFolderNameRuleValue_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTabControlRules();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目値選択時時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void LinkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				YlCommon.ShowHelp("FolderSettei");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormFolderSettings ___END___

}
// namespace YukaLister ___END___