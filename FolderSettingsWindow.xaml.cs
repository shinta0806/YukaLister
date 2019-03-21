// ============================================================================
// 
// FolderSettings の設定を行うウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using YukaLister.Shared;

namespace YukaLister
{
	/// <summary>
	/// FolderSettingsWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class FolderSettingsWindow : Window
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FolderSettingsWindow(String oFolderExLen, YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mFolder = oFolderExLen;
			mYukaListerSettings = oYukaListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 設定対象フォルダー（ExLen 形式）
		private String mFolder;

		// カテゴリー一覧
		private List<String> mCategoryNames;

		// プレビュー情報
		private List<PreviewInfo> mPreviewInfos = new List<PreviewInfo>();

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
		// ButtonVar のコンテキストメニューにアイテムを追加
		// --------------------------------------------------------------------
		private void AddContextMenuItemToButtonVar(String oLabel)
		{
			YlCommon.AddContextMenuItem(ButtonVar, oLabel, ContextMenuButtonVarItem_Click);
		}

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
				if (ListBoxFileNameRules.SelectedIndex == i && !oCheckSelectedLine)
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

			aFolderSettings.AppGeneration = YlCommon.APP_GENERATION;
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
		private void ContextMenuButtonVarItem_Click(Object oSender, RoutedEventArgs oRoutedEventArgs)
		{
			try
			{
				MenuItem aItem = (MenuItem)oSender;
				String aKey = FindRuleVarName((String)aItem.Header);
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
		private void DisableComponentsWithInvoke()
		{
			Dispatcher.Invoke(new Action(() =>
			{
				TabControlRules.IsEnabled = false;
				ButtonPreview.IsEnabled = false;
				ButtonDeleteSettings.IsEnabled = false;
				ButtonOK.IsEnabled = false;
			}));
		}

		// --------------------------------------------------------------------
		// 名称の編集
		// --------------------------------------------------------------------
		private void EditInfo()
		{
			if (DataGridPreview.SelectedIndex < 0)
			{
				return;
			}
			String aFileName = mPreviewInfos[DataGridPreview.SelectedIndex].FileName;

			// ファイル命名規則とフォルダー固定値を適用
			FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings2Ex(mFolder);
			FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
			Dictionary<String, String> aDic = YlCommon.MatchFileNameRulesAndFolderRule
					(Path.GetFileNameWithoutExtension(aFileName), aFolderSettingsInMemory);

			// 楽曲名が取得できていない場合は編集不可
			if (String.IsNullOrEmpty(aDic[YlCommon.RULE_VAR_TITLE]))
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名から楽曲名を取得できていないため、編集できません。\nファイル命名規則を確認して下さい。");
				return;
			}

			EditMusicInfoWindow aEditMusicInfoWindow = new EditMusicInfoWindow(aFileName, aDic, mYukaListerSettings, mLogWriter);
			aEditMusicInfoWindow.Owner = this;
			aEditMusicInfoWindow.ShowDialog();
		}

		// --------------------------------------------------------------------
		// UI 有効化
		// --------------------------------------------------------------------
		private void EnableComponentsWithInvoke()
		{
			Dispatcher.Invoke(new Action(() =>
			{
				TabControlRules.IsEnabled = true;

				// ButtonPreview は状況によって状態が異なる
				UpdateButtonPreview();

				// ButtonDeleteSettings は状況によって状態が異なる
				UpdateSettingsFileStatus();

				ButtonOK.IsEnabled = true;
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
			if (ComboBoxFolderNameRuleValue.IsVisible)
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
			Common.DisableMinimizeBox(this);
			UpdateTitleBar();

			// ラベル
			LabelFolder.Content = YlCommon.ShortenPath(mFolder);

			// タグボタン
			List<String> aLabels = CreateRuleVarLabels();
			foreach (String aLabel in aLabels)
			{
				// オンボーカル・オフボーカルは除外
				if (aLabel.IndexOf(YlCommon.RULE_VAR_ON_VOCAL, StringComparison.OrdinalIgnoreCase) < 0
						&& aLabel.IndexOf(YlCommon.RULE_VAR_OFF_VOCAL, StringComparison.OrdinalIgnoreCase) < 0)
				{
					AddContextMenuItemToButtonVar(aLabel);
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

			// 設計時サイズ以下にできないようにする
			MinWidth = ActualWidth;
			MinHeight = ActualHeight;

			Common.CascadeWindow(this);
		}

		// --------------------------------------------------------------------
		// CheckBoxExclude の状態から除外設定を読み取る
		// --------------------------------------------------------------------
		private Boolean IsExclude()
		{
			return CheckBoxExclude.IsChecked.HasValue && CheckBoxExclude.IsChecked.Value;
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
				DisableComponentsWithInvoke();
				SetCursorWithInvoke(Cursors.Wait);

				Int32 aRowIndex = -1;
				Dispatcher.Invoke(new Action(() =>
				{
					aRowIndex = DataGridPreview.SelectedIndex;
				}));

				// マッチ準備
				FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings2Ex(mFolder);
				FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);

				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					using (DataContext aContext = new DataContext(aConnection))
					{
						for (; ; )
						{
							aRowIndex++;
							if (aRowIndex >= mPreviewInfos.Count)
							{
								mLogWriter.ShowLogMessage(TraceEventType.Information, "ファイル名から取得した楽曲情報・番組情報が楽曲情報データベースに未登録のファイルは見つかりませんでした。");
								Dispatcher.Invoke(new Action(() =>
								{
									DataGridPreview.SelectedIndex = -1;
								}));
								return;
							}

							// ファイル命名規則とフォルダー固定値を適用
							Dictionary<String, String> aDic = YlCommon.MatchFileNameRulesAndFolderRule(
										Path.GetFileNameWithoutExtension(mPreviewInfos[aRowIndex].FileName), aFolderSettingsInMemory);

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

				Dispatcher.Invoke(new Action(() =>
				{
					Common.SelectDataGridCell(DataGridPreview, aRowIndex, 0);
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
				SetCursorWithInvoke(Cursors.Arrow);
				EnableComponentsWithInvoke();
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
						"確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation))
				{
					case MessageBoxResult.Yes:
						AddFileNameRule();
						break;
					case MessageBoxResult.No:
						break;
					case MessageBoxResult.Cancel:
						throw new OperationCanceledException("保存を中止しました。");
				}
			}
			String aFolderNameRuleValue = ComboBoxFolderNameRuleValue.IsVisible ? (String)ComboBoxFolderNameRuleValue.SelectedItem : TextBoxFolderNameRuleValue.Text;
			if (!String.IsNullOrEmpty(aFolderNameRuleValue))
			{
				switch (MessageBox.Show("固定値項目に入力中の\n" + aFolderNameRuleValue + "\nはまだ固定値として追加されていません。\n追加しますか？",
						"確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation))
				{
					case MessageBoxResult.Yes:
						AddFolderNameRule();
						break;
					case MessageBoxResult.No:
						break;
					case MessageBoxResult.Cancel:
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

			// 除外設定の保存
			String aYukaListerExcludeConfigPath = mFolder + "\\" + YlCommon.FILE_NAME_YUKA_LISTER_EXCLUDE_CONFIG;
			if (IsExclude())
			{
				if (!File.Exists(aYukaListerExcludeConfigPath))
				{
					File.Create(aYukaListerExcludeConfigPath);
				}
			}
			else
			{
				if (File.Exists(aYukaListerExcludeConfigPath))
				{
					File.Delete(aYukaListerExcludeConfigPath);
				}
			}

			UpdateSettingsFileStatus();
			mIsDirty = false;
		}

		// --------------------------------------------------------------------
		// カーソル形状の設定
		// --------------------------------------------------------------------
		private void SetCursorWithInvoke(Cursor oCursor)
		{
			Dispatcher.Invoke(new Action(() =>
			{
				Cursor = oCursor;
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
			FolderSettingsInDisk aSettings = YlCommon.LoadFolderSettings2Ex(mFolder);
			foreach (String aFileNameRule in aSettings.FileNameRules)
			{
				ListBoxFileNameRules.Items.Add(aFileNameRule);
			}
			foreach (String aFolderNameRule in aSettings.FolderNameRules)
			{
				ListBoxFolderNameRules.Items.Add(aFolderNameRule);
			}

			// 除外設定
			CheckBoxExclude.IsChecked = YlCommon.DetectFolderExcludeSettingsStatus(mFolder) == FolderExcludeSettingsStatus.True;
		}

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
		// 名称の編集ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonEditInfo()
		{
			ButtonEditInfo.IsEnabled = !IsExclude() && DataGridPreview.SelectedIndex >= 0;
		}

		// --------------------------------------------------------------------
		// 未登録検出ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonJump()
		{
			ButtonJump.IsEnabled = !IsExclude() && mPreviewInfos.Count > 0;
		}

		// --------------------------------------------------------------------
		// ファイル検索ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonPreview()
		{
			ButtonPreview.IsEnabled = !IsExclude();
			DataGridPreview.IsEnabled = !IsExclude();
		}

		// --------------------------------------------------------------------
		// データグリッドビューを更新
		// --------------------------------------------------------------------
		private void UpdateDataGridViewPreview(Object oDummy)
		{
			try
			{
				// 準備
				DisableComponentsWithInvoke();
				SetCursorWithInvoke(Cursors.Wait);

				// クリア
				mPreviewInfos.Clear();

				// 検索
				String[] aAllPathes = Directory.GetFiles(mFolder);

				// マッチをリストに追加
				FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings2Ex(mFolder);
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

					// ファイル
					PreviewInfo aPreviewInfo = new PreviewInfo();
					aPreviewInfo.FileName = Path.GetFileName(aPath);

					// 項目と値
					StringBuilder aSB = new StringBuilder();
					foreach (KeyValuePair<String, String> aKvp in aDic)
					{
						if (aKvp.Key != YlCommon.RULE_VAR_ANY && !String.IsNullOrEmpty(aKvp.Value))
						{
							aSB.Append(aRuleMap[aKvp.Key] + "=" + aKvp.Value + ", ");
						}
					}
					aPreviewInfo.Items = aSB.ToString();

					// 追加
					mPreviewInfos.Add(aPreviewInfo);
				}

				// 表示に反映
				Dispatcher.Invoke(new Action(() =>
				{
					DataGridPreview.ItemsSource = mPreviewInfos;
					DataGridPreview.Items.Refresh();
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
				SetCursorWithInvoke(Cursors.Arrow);
				EnableComponentsWithInvoke();
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
				ComboBoxFolderNameRuleValue.Visibility = Visibility.Visible;
				TextBoxFolderNameRuleValue.Visibility = Visibility.Collapsed;

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
				ComboBoxFolderNameRuleValue.Visibility = Visibility.Collapsed;
				TextBoxFolderNameRuleValue.Visibility = Visibility.Visible;
			}
		}

		// --------------------------------------------------------------------
		// 設定ファイルの状況を表示
		// --------------------------------------------------------------------
		private void UpdateSettingsFileStatus()
		{
			FolderSettingsStatus aStatus = YlCommon.DetectFolderSettingsStatus2Ex(mFolder);

			switch (aStatus)
			{
				case FolderSettingsStatus.None:
					LabelSettingsFileStatus.Content = "このフォルダーの設定がありません。";
					ButtonDeleteSettings.IsEnabled = false;
					break;
				case FolderSettingsStatus.Set:
					LabelSettingsFileStatus.Content = "このフォルダーは設定済みです。";
					ButtonDeleteSettings.IsEnabled = true;
					break;
				case FolderSettingsStatus.Inherit:
					LabelSettingsFileStatus.Content = "親フォルダーの設定を参照しています（設定変更すると親フォルダーとは別の設定になります）。";
					ButtonDeleteSettings.IsEnabled = false;
					break;
				default:
					Debug.Assert(false, "UpdateLabelSettingsFileStatus() bad FolderSettingsStatus");
					break;
			}
		}

		// --------------------------------------------------------------------
		// タブ内のコンポーネントの状態を更新
		// --------------------------------------------------------------------
		private void UpdateTabControlRules()
		{
			// ファイル名命名規則
			ButtonAddFileNameRule.IsEnabled = !String.IsNullOrEmpty(TextBoxFileNameRule.Text);
			ButtonReplaceFileNameRule.IsEnabled = !String.IsNullOrEmpty(TextBoxFileNameRule.Text) && (ListBoxFileNameRules.SelectedIndex >= 0);
			ButtonDeleteFileNameRule.IsEnabled = (ListBoxFileNameRules.SelectedIndex >= 0);
			ButtonUpFileNameRule.IsEnabled = (ListBoxFileNameRules.SelectedIndex > 0);
			ButtonDownFileNameRule.IsEnabled = (0 <= ListBoxFileNameRules.SelectedIndex && ListBoxFileNameRules.SelectedIndex < ListBoxFileNameRules.Items.Count - 1);

			// フォルダー固定値
			if (ComboBoxFolderNameRuleValue.IsVisible)
			{
				ButtonAddFolderNameRule.IsEnabled = !String.IsNullOrEmpty((String)ComboBoxFolderNameRuleValue.SelectedItem);
			}
			else
			{
				ButtonAddFolderNameRule.IsEnabled = !String.IsNullOrEmpty(TextBoxFolderNameRuleValue.Text);
			}
			ButtonDeleteFolderNameRule.IsEnabled = (ListBoxFolderNameRules.SelectedIndex >= 0);
			ButtonUpFolderNameRule.IsEnabled = (ListBoxFolderNameRules.SelectedIndex > 0);
			ButtonDownFolderNameRule.IsEnabled = (0 <= ListBoxFolderNameRules.SelectedIndex && ListBoxFolderNameRules.SelectedIndex < ListBoxFolderNameRules.Items.Count - 1);
		}

		// --------------------------------------------------------------------
		// タイトルバーを更新
		// --------------------------------------------------------------------
		private void UpdateTitleBar()
		{
			Title = "フォルダー設定";
#if DEBUG
			Title = "［デバッグ］" + Title;
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

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "フォルダー設定ウィンドウを開きます。");
				Init();

				UpdateSettingsFileStatus();
				SettingsToCompos();
				UpdateTabControlRules();
				UpdateFolderNameRuleComponents();
				UpdateButtonJump();
				UpdateButtonEditInfo();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_SourceInitialized(object sender, EventArgs e)
		{
			try
			{

			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ウィンドウ初期化時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonVar_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ButtonVar.ContextMenu.IsOpen = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "変数メニュー表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonAddFileNameRule_Click(object sender, RoutedEventArgs e)
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

		private void TextBoxFileNameRule_TextChanged(object sender, TextChangedEventArgs e)
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

		private void ListBoxFileNameRules_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

		private void ButtonReplaceFileNameRule_Click(object sender, RoutedEventArgs e)
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

		private void ButtonDeleteFileNameRule_Click(object sender, RoutedEventArgs e)
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

		private void ButtonUpFileNameRule_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Int32 aIndex = ListBoxFileNameRules.SelectedIndex;
				SwapListItem(ListBoxFileNameRules, aIndex - 1, aIndex);
				ListBoxFileNameRules.SelectedIndex = aIndex - 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則順番繰り上げ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDownFileNameRule_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Int32 aIndex = ListBoxFileNameRules.SelectedIndex;
				SwapListItem(ListBoxFileNameRules, aIndex + 1, aIndex);
				ListBoxFileNameRules.SelectedIndex = aIndex + 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則順番繰り下げ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonPreview_Click(object sender, RoutedEventArgs e)
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

		private void TextBoxFolderNameRuleValue_TextChanged(object sender, TextChangedEventArgs e)
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

		private void ListBoxFolderNameRules_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

		private void ButtonAddFolderNameRule_Click(object sender, RoutedEventArgs e)
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

		private void ComboBoxFolderNameRuleName_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

		private void ButtonDeleteFolderNameRule_Click(object sender, RoutedEventArgs e)
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

		private void ButtonUpFolderNameRule_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Int32 aIndex = ListBoxFolderNameRules.SelectedIndex;
				SwapListItem(ListBoxFolderNameRules, aIndex - 1, aIndex);
				ListBoxFolderNameRules.SelectedIndex = aIndex - 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目順番繰り上げ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDownFolderNameRule_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Int32 aIndex = ListBoxFolderNameRules.SelectedIndex;
				SwapListItem(ListBoxFolderNameRules, aIndex + 1, aIndex);
				ListBoxFolderNameRules.SelectedIndex = aIndex + 1;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目順番繰り下げ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFileNameRules_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

		private void ListBoxFolderNameRules_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
				if (ComboBoxFolderNameRuleValue.IsVisible)
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

		private void ButtonDeleteSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (MessageBox.Show("フォルダー設定を削除します。\nよろしいですか？", "確認",
						MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
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
				if (File.Exists(mFolder + "\\" + YlCommon.FILE_NAME_YUKA_LISTER_EXCLUDE_CONFIG))
				{
					File.Delete(mFolder + "\\" + YlCommon.FILE_NAME_YUKA_LISTER_EXCLUDE_CONFIG);
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

		private void ButtonOK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SaveSettingsIfNeeded();
				DialogResult = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定保存時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditInfo_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				EditInfo();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "名称の編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
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

		private void ButtonJump_Click(object sender, RoutedEventArgs e)
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

		private void DataGridPreview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			try
			{
				EditInfo();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー一覧ダブルクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ComboBoxFolderNameRuleValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

		private void CheckBoxExclude_Checked(object sender, RoutedEventArgs e)
		{
			try
			{
				FolderExcludeSettingsStatus aFolderExcludeSettingsStatus = YlCommon.DetectFolderExcludeSettingsStatus(mFolder);
				mIsDirty |= (aFolderExcludeSettingsStatus != FolderExcludeSettingsStatus.False) != IsExclude();

				// コンポーネントに反映
				TabControlRules.IsEnabled = !IsExclude();
				UpdateButtonPreview();
				UpdateButtonJump();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー除外チェックボックスクリック時エラー：\n" + oExcep.Message);
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

		private void DataGridPreview_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				UpdateButtonEditInfo();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "プレビュー一覧選択変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FolderSettingsWindow ___END___
}
// namespace YukaLister ___END___
