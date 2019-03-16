// ============================================================================
// 
// 楽曲情報データベースを検索するウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using MaterialDesignThemes.Wpf;
using Shinta;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YukaLister.Shared;

namespace YukaLister
{
	/// <summary>
	/// SearchMusicInfoWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class SearchMusicInfoWindow : Window
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public SearchMusicInfoWindow(String oItemName, MusicInfoDbTables oTableIndex, String oDefaultKeyword, LogWriter oLogWriter)
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
		// LabelFounds を見かけ上空欄にする
		// --------------------------------------------------------------------
		private void ClearLabelFounds()
		{
			// null にするとラベルの高さが変わってしまうためスペースを入れる
			LabelFounds.Content = " ";
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Title = mItemName + "を検索";
#if DEBUG
			Title = "［デバッグ］" + Title;
#endif

			// 説明
			LabelDescription.Content = mItemName + "を、既に登録されている情報から検索します。";

			// キーワード
			TextBoxKeyword.ToolTip = "ここに検索したいキーワードを入力して下さい。";
			HintAssist.SetHint(TextBoxKeyword, TextBoxKeyword.ToolTip);
			ToolTipService.SetShowDuration(TextBoxKeyword, YlCommon.TOOL_TIP_LONG_INTERVAL);

			Common.CascadeWindow(this);
		}

		// --------------------------------------------------------------------
		// 検索
		// --------------------------------------------------------------------
		private void Search()
		{
			try
			{
				String aKeyword = YlCommon.NormalizeDbString(TextBoxKeyword.Text);
				if (String.IsNullOrEmpty(aKeyword))
				{
					return;
				}

				Cursor = Cursors.Wait;
				ListBoxFounds.ItemsSource = null;
				ClearLabelFounds();

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
				LabelFounds.Content = aHits.Count.ToString("#,0") + " 個の結果が見つかりました。";

				// リストボックスに表示
				ListBoxFounds.ItemsSource = aHits;
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
				Cursor = Cursors.Arrow;
			}
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

			DialogResult = true;
		}

		// --------------------------------------------------------------------
		// 検索ボタンの状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonSearch()
		{
			String aKeyword = YlCommon.NormalizeDbString(TextBoxKeyword.Text);
			ButtonSearch.IsEnabled = !String.IsNullOrEmpty(aKeyword);
		}

		// --------------------------------------------------------------------
		// 選択ボタンの状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonSelect()
		{
			ButtonSelect.IsEnabled = ListBoxFounds.SelectedIndex >= 0;
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "データベース検索ウィンドウを開きます。");
				Init();

				UpdateButtonSearch();
				ClearLabelFounds();
				UpdateButtonSelect();
				TextBoxKeyword.Focus();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "データベース検索ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearch_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Search();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxFounds_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

		private void ButtonSelect_Click(object sender, RoutedEventArgs e)
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
				if (e.Key == Key.Enter)
				{
					Search();
					e.Handled = true;
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "検索キーワード入力時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
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

		private void TextBoxKeyword_TextChanged(object sender, TextChangedEventArgs e)
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

		private void ListBoxFounds_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

	}
}
