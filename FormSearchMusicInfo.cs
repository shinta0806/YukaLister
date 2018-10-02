// ============================================================================
// 
// 楽曲情報データベースを検索するウィンドウ
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
using System.Windows.Forms;
using System.Data.SQLite;

namespace YukaLister
{
	public partial class FormSearchMusicInfo : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormSearchMusicInfo(String oItemName, MusicInfoDbTables oTableIndex, String oDefaultKeyword, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 初期化
			mItemName = oItemName;
			mTableIndex = oTableIndex;
			mLogWriter = oLogWriter;

			// コンポーネント
			TextBoxKeyword.Text = oDefaultKeyword;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 選択された情報
		public String SelectedName { get; set; }

		// ====================================================================
		// prvate メンバー変数
		// ====================================================================

		// 検索項目名
		private String mItemName;

		// テーブルインデックス
		private MusicInfoDbTables mTableIndex;

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// prvate メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = mItemName + "を検索";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			// 説明
			LabelDescription.Text = mItemName + "を、既に登録されている情報から検索します。";

			Common.CascadeForm(this);
		}

		// --------------------------------------------------------------------
		// リストボックスで選択されている値を決定値としてウィンドウを閉じる
		// --------------------------------------------------------------------
		private void SelectAndClose()
		{
			if (ListBoxFounds.SelectedIndex < 0)
			{
				return;
			}
			SelectedName = (String)ListBoxFounds.Items[ListBoxFounds.SelectedIndex];
			if (String.IsNullOrEmpty(SelectedName))
			{
				return;
			}

			DialogResult = DialogResult.OK;
		}

		// --------------------------------------------------------------------
		// 検索ボタンの状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonSearch()
		{
			String aKeyword = YlCommon.NormalizeDbString(TextBoxKeyword.Text);
			ButtonSearch.Enabled = !String.IsNullOrEmpty(aKeyword);
		}

		// --------------------------------------------------------------------
		// 選択ボタンの状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonSelect()
		{
			ButtonSelect.Enabled = ListBoxFounds.SelectedIndex >= 0;
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormSearchOrigin_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "データベース検索ウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "データベース検索ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormSearchOrigin_Shown(object sender, EventArgs e)
		{
			try
			{
				UpdateButtonSearch();
				LabelFounds.Text = null;
				UpdateButtonSelect();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "データベース検索ウィンドウ表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearch_Click(object sender, EventArgs e)
		{
			try
			{
				String aKeyword = YlCommon.NormalizeDbString(TextBoxKeyword.Text);
				if (String.IsNullOrEmpty(aKeyword))
				{
					return;
				}

				Cursor = Cursors.WaitCursor;
				ListBoxFounds.Items.Clear();
				LabelFounds.Text = null;

				// 検索
				List<String> aHits = new List<String>();
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					using (SQLiteCommand aCmd = new SQLiteCommand(aConnection))
					{
						aCmd.CommandText = "SELECT DISTINCT " + YlCommon.MUSIC_INFO_DB_NAME_COLUMN_NAMES[(Int32)mTableIndex]
								+ " FROM " + YlCommon.MUSIC_INFO_DB_TABLE_NAMES[(Int32)mTableIndex]
								+ " WHERE (" + YlCommon.MUSIC_INFO_DB_NAME_COLUMN_NAMES[(Int32)mTableIndex] + " LIKE @keyword1"
								+ " OR " + YlCommon.MUSIC_INFO_DB_KEYWORD_COLUMN_NAMES[(Int32)mTableIndex] + " LIKE @keyword2";
						aCmd.Parameters.Add(new SQLiteParameter("@keyword1", "%" + aKeyword + "%"));
						aCmd.Parameters.Add(new SQLiteParameter("@keyword2", "%" + aKeyword + "%"));

						String aRuby = YlCommon.NormalizeDbRuby(TextBoxKeyword.Text);
						if (!String.IsNullOrEmpty(aRuby) && aRuby.Length == TextBoxKeyword.Text.Length)
						{
							// すべてフリガナとして使える文字が入力された場合は、フリガナでも検索
							aCmd.CommandText += " OR " + YlCommon.MUSIC_INFO_DB_RUBY_COLUMN_NAMES[(Int32)mTableIndex] + " LIKE @ruby1";
							aCmd.Parameters.Add(new SQLiteParameter("@ruby1", "%" + aRuby + "%"));
							// 検索ワードもフリガナでも検索
							aCmd.CommandText += " OR " + YlCommon.MUSIC_INFO_DB_KEYWORD_COLUMN_NAMES[(Int32)mTableIndex] + " LIKE @ruby2";
							aCmd.Parameters.Add(new SQLiteParameter("@ruby2", "%" + aRuby + "%"));
						}

						aCmd.CommandText += ") AND " + YlCommon.MUSIC_INFO_DB_INVALID_COLUMN_NAMES[(Int32)mTableIndex] + " = 0";


						using (SQLiteDataReader aReader = aCmd.ExecuteReader())
						{
							while (aReader.Read())
							{
								aHits.Add(aReader[0].ToString());
							}
						}
					}
				}

				if (aHits.Count == 0)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Error, "「" + aKeyword + "」を含む" + mItemName + "はありません。");
					return;
				}
				aHits.Sort();
				LabelFounds.Text = aHits.Count.ToString("#,0") + " 個の結果が見つかりました。";

				// リストボックスに表示
				ListBoxFounds.Items.AddRange(aHits.ToArray());
				ListBoxFounds.Focus();

				// 選択（完全一致）
				Int32 aSelection = aHits.IndexOf(aKeyword);
				if (aSelection >= 0)
				{
					ListBoxFounds.SelectedIndex = aSelection;
					return;
				}

				// 選択（大文字小文字を区別しない）
				aSelection = aHits.FindIndex(x => String.Compare(x, aKeyword, true) == 0);
				if (aSelection >= 0)
				{
					ListBoxFounds.SelectedIndex = aSelection;
					return;
				}

				ListBoxFounds.SelectedIndex = 0;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "検索時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}

		private void ListBoxFounds_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateButtonSelect();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リスト選択時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSelect_Click(object sender, EventArgs e)
		{
			try
			{
				SelectAndClose();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "選択決定時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxKeyword_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyCode == Keys.Enter)
				{
					ButtonSearch.PerformClick();
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "検索キーワード入力時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxKeyword_Enter(object sender, EventArgs e)
		{
			try
			{
				// テキストボックスがエンターキーを検出できるようにする
				AcceptButton = null;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "検索キーワードフォーカス時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxKeyword_Leave(object sender, EventArgs e)
		{
			try
			{
				AcceptButton = ButtonSelect;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "検索キーワードフォーカス解除時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormSearchOrigin_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "データベース検索ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "データベース検索ウィンドウクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxKeyword_TextChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateButtonSearch();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "検索キーワード変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFounds_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				SelectAndClose();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リストボックスダブルクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
	// public partial class FormSearchMusicInfo ___END___

}
// namespace YukaLister ___END___
