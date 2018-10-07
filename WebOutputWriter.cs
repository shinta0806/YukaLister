// ============================================================================
// 
// HTML / PHP リスト出力用基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 出力フロー
// 1. 新しいリストをテンポラリーフォルダーに作成
// 2. インデックス系を「更新中」の表示にする
// 3. インデックス系以外の古いリストを削除
// 4. インデックス系以外の新しいリストと出力フォルダーに移動
// 5. インデックス系を移動
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace YukaLister.Shared
{
	// ====================================================================
	// HTML / PHP リスト出力用基底クラス
	// ====================================================================

	public abstract class WebOutputWriter : OutputWriter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public WebOutputWriter()
		{
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定画面に入力された値が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public override void CheckInput()
		{
			base.CheckInput();

			// 新着の日数
			if (CheckBoxEnableNew.Checked)
			{
				Int32 aNewDays;
				Int32.TryParse(TextBoxNewDays.Text, out aNewDays);
				if (aNewDays < YlCommon.NEW_DAYS_MIN)
				{
					throw new Exception("新着の日数は " + YlCommon.NEW_DAYS_MIN.ToString() + " 以上を指定して下さい。");
				}
			}
		}

		// --------------------------------------------------------------------
		// コンポーネントから設定に反映
		// --------------------------------------------------------------------
		public override void ComposToSettings()
		{
			base.ComposToSettings();

			WebOutputSettings aWebOutputSettings = (WebOutputSettings)OutputSettings;

			// 新着
			aWebOutputSettings.EnableNew = CheckBoxEnableNew.Checked;
			Int32 aNewDays;
			Int32.TryParse(TextBoxNewDays.Text, out aNewDays);
			aWebOutputSettings.NewDays = aNewDays;
		}

		// --------------------------------------------------------------------
		// 設定画面のタブページ
		// --------------------------------------------------------------------
		public override List<TabPage> DialogTabPages()
		{
			List<TabPage> aTabPages = base.DialogTabPages();

			// TabPageWebOutputSettings
			TabPageWebOutputSettings = new TabPage();
			TabPageWebOutputSettings.BackColor = SystemColors.Control;
			TabPageWebOutputSettings.Location = new Point(4, 22);
			TabPageWebOutputSettings.Padding = new Padding(3);
			TabPageWebOutputSettings.Size = new Size(456, 386);
			TabPageWebOutputSettings.Text = "HTML";

			// CheckBoxEnableNew
			CheckBoxEnableNew = new CheckBox();
			CheckBoxEnableNew.Location = new Point(16, 16);
			CheckBoxEnableNew.Size = new Size(24, 20);
			CheckBoxEnableNew.UseVisualStyleBackColor = true;
			CheckBoxEnableNew.CheckedChanged += new EventHandler(this.CheckBoxEnableNew_CheckedChanged);
			TabPageWebOutputSettings.Controls.Add(CheckBoxEnableNew);

			// TextBoxNewDays
			TextBoxNewDays = new TextBox();
			TextBoxNewDays.Location = new Point(40, 16);
			TextBoxNewDays.Size = new Size(40, 19);
			TabPageWebOutputSettings.Controls.Add(TextBoxNewDays);

			// LabelNewDays
			LabelNewDays = new Label();
			LabelNewDays.Location = new Point(88, 16);
			LabelNewDays.Size = new Size(352, 20);
			LabelNewDays.Text = "日以内に更新されたファイルを NEW （新着）に記載する";
			LabelNewDays.TextAlign = ContentAlignment.MiddleLeft;
			TabPageWebOutputSettings.Controls.Add(LabelNewDays);

			aTabPages.Add(TabPageWebOutputSettings);

			return aTabPages;
		}

		// --------------------------------------------------------------------
		// 設定画面を有効化するかどうか
		// --------------------------------------------------------------------
		public override Boolean IsDialogEnabled()
		{
			return true;
		}

		// --------------------------------------------------------------------
		// リスト出力
		// --------------------------------------------------------------------
		public override void Output()
		{
			PrepareOutput();

			// 出力先フォルダーへの出力
			OutputCss();
			OutputJs();

			// 一時フォルダーへの出力
			Boolean aIsNewExists = OutputNew();
			OutputCategoryAndHeads(aIsNewExists);
			OutputTieUpGroupHeadAndTieUpGroups(aIsNewExists);
			OutputPeriodAndHeads(aIsNewExists);
			OutputYearsAndSeasons(aIsNewExists);
			OutputArtistAndHeads(aIsNewExists);
			OutputComposerAndHeads(aIsNewExists);

			// インデックス系を「更新中」表示にする
			OutputNoticeIndexes();

			// 古いファイルを削除
			DeleteOldListContents();

			// 一時フォルダーから移動
			MoveList();
		}

		// --------------------------------------------------------------------
		// 設定をコンポーネントに反映
		// --------------------------------------------------------------------
		public override void SettingsToCompos()
		{
			base.SettingsToCompos();
			WebOutputSettings aWebOutputSettings = (WebOutputSettings)OutputSettings;

			// 新着
			CheckBoxEnableNew.Checked = aWebOutputSettings.EnableNew;
			TextBoxNewDays.Text = aWebOutputSettings.NewDays.ToString();
			UpdateTextBoxNewDays();
		}

		// ====================================================================
		// protected メンバー変数
		// ====================================================================

		// リストの拡張子（ピリオド含む）
		protected String mListExt;

		// 階層トップの名前（タイトル用）
		protected String mDirectoryTopName;

		// 階層トップのリンク（ページの先頭用）
		protected String mDirectoryTopLink;

		// 追加説明
		protected String mAdditionalDescription;

		// 追加 HTML ヘッダー
		protected String mAdditionalHeader;

		// トップページからリストをリンクする際の引数
		protected String mListLinkArg;

		// コンポーネント
		protected TabPage TabPageWebOutputSettings;
		protected CheckBox CheckBoxEnableNew;
		protected TextBox TextBoxNewDays;
		protected Label LabelNewDays;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リストに出力するファイル名の表現
		// --------------------------------------------------------------------
		protected abstract String FileNameDescription(String oFileName);

		// --------------------------------------------------------------------
		// コンストラクターでは行えない準備などを実施
		// --------------------------------------------------------------------
		protected override void PrepareOutput()
		{
			base.PrepareOutput();

			// テーブル項目名（原則 YlCommon.OUTPUT_ITEM_NAMES だが一部見やすいよう変更）
			mThNames = new List<String>(YlCommon.OUTPUT_ITEM_NAMES);
			mThNames[(Int32)OutputItems.Worker] = "制作";
			mThNames[(Int32)OutputItems.SmartTrack] = "On</th><th>Off";
			mThNames[(Int32)OutputItems.FileSize] = "サイズ";

			// 一時フォルダー
			mTempFolderPath = YlCommon.TempFilePath() + "\\";
			Directory.CreateDirectory(mTempFolderPath);
		}


		// ====================================================================
		// private 定数
		// ====================================================================

		// リストファイル名の先頭文字列（カテゴリーインデックス以外）
		private const String FILE_NAME_PREFIX = "List";

		// リストの種類に応じたファイル名
		private const String KIND_FILE_NAME_ARTIST = "Artist";
		private const String KIND_FILE_NAME_CATEGORY = "Category";
		private const String KIND_FILE_NAME_COMPOSER = "Composer";
		private const String KIND_FILE_NAME_NEW = "New";
		private const String KIND_FILE_NAME_PERIOD = "Period";
		private const String KIND_FILE_NAME_SEASON = "Season";
		private const String KIND_FILE_NAME_TIE_UP_GROUP = "Series";

		// HTML テンプレートに記載されている変数
		private const String HTML_VAR_ADDITIONAL_DESCRIPTION = "<!-- $AdditionalDescription$ -->";
		private const String HTML_VAR_ADDITIONAL_HEADER = "<!-- $AdditionalHeader$ -->";
		private const String HTML_VAR_CATEGORY = "<!-- $Category$ -->";
		private const String HTML_VAR_CATEGORY_INDEX = "<!-- $CategoryIndex$ -->";
		private const String HTML_VAR_CLASS_OF_AL = "<!-- $ClassOfAl$ -->";
		private const String HTML_VAR_CLASS_OF_KANA = "<!-- $ClassOfKana$ -->";
		private const String HTML_VAR_CLASS_OF_MISC = "<!-- $ClassOfMisc$ -->";
		private const String HTML_VAR_CLASS_OF_NUM = "<!-- $ClassOfNum$ -->";
		private const String HTML_VAR_DIRECTORY = "<!-- $Directory$ -->";
		private const String HTML_VAR_GENERATE_DATE = "<!-- $GenerateDate$ -->";
		private const String HTML_VAR_GENERATOR = "<!-- $Generator$ -->";
		private const String HTML_VAR_GROUP_NAVI = "<!-- $GroupNavi$ -->";
		private const String HTML_VAR_INDICES = "<!-- $Indices$ -->";
		private const String HTML_VAR_NEW = "<!-- $New$ -->";
		private const String HTML_VAR_NUM_SONGS = "<!-- $NumSongs$ -->";
		private const String HTML_VAR_PAGES = "<!-- $Pages$ -->";
		private const String HTML_VAR_PROGRAMS = "<!-- $Programs$ -->";
		private const String HTML_VAR_TITLE = "<!-- $Title$ -->";

		// テーブル非表示
		private const String CLASS_NAME_INVISIBLE = "class=\"invisible\"";

		// 期別リストの年数
		private const Int32 SEASON_YEARS = 5;

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// テーブルに表示する項目名
		private List<String> mThNames;

		// リストを一時的に出力するフォルダー（末尾 '\\'）
		private String mTempFolderPath;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 曲情報を文字列に追加する
		// --------------------------------------------------------------------
		private void AppendSongInfo(StringBuilder oSB, OutputItems oChapterItem, Int32 oSongsIndex, TFound oTFound)
		{
			oSB.Append("  <tr class=\"");
			if (oSongsIndex % 2 == 0)
			{
				oSB.Append("even");
			}
			else
			{
				oSB.Append("odd");
			}
			oSB.Append("\">\n    ");

			foreach (OutputItems aOutputItem in mRuntimeOutputItems)
			{
				if (aOutputItem == oChapterItem)
				{
					continue;
				}

				switch (aOutputItem)
				{
					case OutputItems.Path:
						oSB.Append("<td class=\"small\">" + FileNameDescription(oTFound.Path) + "</td>");
						break;
					case OutputItems.FileName:
						oSB.Append("<td class=\"small\">" + FileNameDescription(Path.GetFileName(oTFound.Path)) + "</td>");
						break;
					case OutputItems.Head:
						oSB.Append("<td>" + oTFound.Head + "</td>");
						break;
					case OutputItems.Worker:
						oSB.Append("<td>" + oTFound.Worker + "</td>");
						break;
					case OutputItems.Track:
						oSB.Append("<td>" + oTFound.Track + "</td>");
						break;
					case OutputItems.SmartTrack:
						oSB.Append("<td>" + (oTFound.SmartTrackOnVocal ? YlCommon.SMART_TRACK_VALID_MARK : null) + "</td>");
						oSB.Append("<td>" + (oTFound.SmartTrackOffVocal ? YlCommon.SMART_TRACK_VALID_MARK : null) + "</td>");
						break;
					case OutputItems.Comment:
						oSB.Append("<td class=\"small\">" + oTFound.Comment + "</td>");
						break;
					case OutputItems.LastWriteTime:
						oSB.Append("<td class=\"small\">" + JulianDay.ModifiedJulianDateToDateTime(oTFound.LastWriteTime).ToString(
								YlCommon.DATE_FORMAT + " " + YlCommon.TIME_FORMAT) + "</td>");
						break;
					case OutputItems.FileSize:
						oSB.Append("<td class=\"small\">" + (oTFound.FileSize / (1024 * 1024)).ToString("#,0") + " MB</td>");
						break;
					case OutputItems.SongName:
						oSB.Append("<td>" + oTFound.SongName + "</td>");
						break;
					case OutputItems.SongRuby:
						oSB.Append("<td>" + oTFound.SongRuby + "</td>");
						break;
					case OutputItems.SongOpEd:
						oSB.Append("<td>" + oTFound.SongOpEd + "</td>");
						break;
					case OutputItems.SongReleaseDate:
						if (oTFound.SongReleaseDate <= YlCommon.INVALID_MJD)
						{
							oSB.Append("<td></td>");
						}
						else
						{
							oSB.Append("<td class=\"small\">" + JulianDay.ModifiedJulianDateToDateTime(oTFound.SongReleaseDate).ToString(YlCommon.DATE_FORMAT) + "</td>");
						}
						break;
					case OutputItems.ArtistName:
						oSB.Append("<td>" + oTFound.ArtistName + "</td>");
						break;
					case OutputItems.ArtistRuby:
						oSB.Append("<td>" + oTFound.ArtistRuby + "</td>");
						break;
					case OutputItems.LyristName:
						oSB.Append("<td>" + oTFound.LyristName + "</td>");
						break;
					case OutputItems.LyristRuby:
						oSB.Append("<td>" + oTFound.LyristRuby + "</td>");
						break;
					case OutputItems.ComposerName:
						oSB.Append("<td>" + oTFound.ComposerName + "</td>");
						break;
					case OutputItems.ComposerRuby:
						oSB.Append("<td>" + oTFound.ComposerRuby + "</td>");
						break;
					case OutputItems.ArrangerName:
						oSB.Append("<td>" + oTFound.ArrangerName + "</td>");
						break;
					case OutputItems.ArrangerRuby:
						oSB.Append("<td>" + oTFound.ArrangerRuby + "</td>");
						break;
					case OutputItems.TieUpName:
						oSB.Append("<td>" + oTFound.TieUpName + "</td>");
						break;
					case OutputItems.TieUpRuby:
						oSB.Append("<td>" + oTFound.TieUpRuby + "</td>");
						break;
					case OutputItems.TieUpAgeLimit:
						oSB.Append("<td>" + oTFound.TieUpAgeLimit + "</td>");
						break;
					case OutputItems.Category:
						oSB.Append("<td>" + oTFound.Category + "</td>");
						break;
					case OutputItems.TieUpGroupName:
						oSB.Append("<td>" + oTFound.TieUpGroupName + "</td>");
						break;
					case OutputItems.TieUpGroupRuby:
						oSB.Append("<td>" + oTFound.TieUpGroupRuby + "</td>");
						break;
					case OutputItems.MakerName:
						oSB.Append("<td>" + oTFound.MakerName + "</td>");
						break;
					case OutputItems.MakerRuby:
						oSB.Append("<td>" + oTFound.MakerRuby + "</td>");
						break;
					default:
						Debug.Assert(false, "AppendSongInfo() bad aOutputItem");
						break;
				}
			}

			oSB.Append("\n  </tr>\n");
		}

		// --------------------------------------------------------------------
		// 章を開始する
		// --------------------------------------------------------------------
		private void BeginChapter(StringBuilder oSB, OutputItems oChapterItem, Int32 oChapterIndex, Int32 oNumChapters, List<TFound> oTFounds)
		{
			// 章名挿入
			oSB.Append("<input type=\"checkbox\" id=\"label" + oChapterIndex + "\" class=\"accparent\"");

			// 章数が 1、かつ、番組名 == 頭文字、の場合（ボカロ等）は、リストが最初から開いた状態にする
			if (oNumChapters == 1 && oTFounds[0].TieUpName == oTFounds[0].Head)
			{
				oSB.Append(" checked=\"checked\"");
			}
			oSB.Append(">\n");
			oSB.Append("<label for=\"label" + oChapterIndex + "\">" + ChapterValue(oChapterItem, oTFounds[0]) + "　（"
					+ oTFounds.Count.ToString("#,0") + " 曲）" + "</label>\n");
			oSB.Append("<div class=\"accchild\">\n");

			// テーブルを開く
			oSB.Append("<table>\n");
			oSB.Append("  <tr>\n    ");
			foreach (OutputItems aOutputItem in mRuntimeOutputItems)
			{
				if (aOutputItem == oChapterItem)
				{
					continue;
				}

				oSB.Append("<th>" + mThNames[(Int32)aOutputItem] + "</th>");
			}
			oSB.Append("\n  </tr>\n");
		}

		// --------------------------------------------------------------------
		// 章名として使用する値を返す
		// --------------------------------------------------------------------
		private String ChapterValue(OutputItems oChapterItem, TFound oTFound)
		{
			switch (oChapterItem)
			{
				case OutputItems.ArtistName:
					return oTFound.ArtistName;
				case OutputItems.ComposerName:
					return oTFound.ComposerName;
				case OutputItems.TieUpName:
					return oTFound.TieUpName;
				default:
					Debug.Assert(false, "ChapterValue() bad chapter item: " + oChapterItem.ToString());
					return null;
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void CheckBoxEnableNew_CheckedChanged(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				UpdateTextBoxNewDays();
			}
			catch (Exception oExcep)
			{
				YlCommon.LogWriter.ShowLogMessage(TraceEventType.Error, "新着チェックボックスクリック時エラー：\n" + oExcep.Message);
				YlCommon.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 古いリストを削除（インデックス以外）
		// --------------------------------------------------------------------
		private void DeleteOldListContents()
		{
			Debug.Assert(!String.IsNullOrEmpty(mListExt), "DeleteOldList() mListExt が初期化されていない");

			DeleteOldListContentsCore(KIND_FILE_NAME_CATEGORY);
			DeleteOldListContentsCore(KIND_FILE_NAME_TIE_UP_GROUP);
			DeleteOldListContentsCore(KIND_FILE_NAME_PERIOD);
			DeleteOldListContentsCore(KIND_FILE_NAME_SEASON);
			DeleteOldListContentsCore(KIND_FILE_NAME_ARTIST);
			DeleteOldListContentsCore(KIND_FILE_NAME_COMPOSER);
		}

		// --------------------------------------------------------------------
		// 古いリストを削除
		// --------------------------------------------------------------------
		private void DeleteOldListContentsCore(String oKindFileName)
		{
			String[] aListPathes = Directory.GetFiles(FolderPath, FILE_NAME_PREFIX + "_" + oKindFileName + "_*" + mListExt);

			foreach (String aPath in aListPathes)
			{
				try
				{
					File.Delete(aPath);
				}
				catch (Exception)
				{
					LogWriter.ShowLogMessage(TraceEventType.Error, "古いリストファイル " + Path.GetFileName(aPath) + " を削除できませんでした。", true);
				}
			}
		}

		// --------------------------------------------------------------------
		// 章を終了する
		// --------------------------------------------------------------------
		private void EndChapter(StringBuilder oSB)
		{
			oSB.Append("</table>\n");
			oSB.Append("</div>\n");
		}

		// --------------------------------------------------------------------
		// グループナビ文字列を生成
		// --------------------------------------------------------------------
		private String GroupNavi(Boolean oIsNewExists)
		{
			StringBuilder aSB = new StringBuilder();
			aSB.Append("<table>\n");
			GroupNaviCore(aSB, false, oIsNewExists);
			GroupNaviCore(aSB, true, oIsNewExists);
			aSB.Append("</table>\n");
			return aSB.ToString();
		}

		// --------------------------------------------------------------------
		// グループナビ文字列を生成
		// --------------------------------------------------------------------
		private void GroupNaviCore(StringBuilder oSB, Boolean oIsAdult, Boolean oIsNewExists)
		{
			oSB.Append("<tr>");
			oSB.Append("<td>" + (oIsAdult ? "　アダルト　" : "一般") + "</td>");
			if (oIsNewExists)
			{
				oSB.Append("<td class=\"exist\"><a href=\"" + OutputFileName(oIsAdult, "New", "すべて", null) + mListLinkArg + "\">　新着　</a></td>");
			}
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_CATEGORY) + mListLinkArg + "\">　カテゴリー別　</a></td>");
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_TIE_UP_GROUP) + mListLinkArg + "\">　シリーズ別　</a></td>");
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_PERIOD) + mListLinkArg + "\">　年代別　</a></td>");
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_SEASON) + mListLinkArg + "\">　期別　</a></td>");
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_ARTIST) + mListLinkArg + "\">　歌手別　</a></td>");
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_COMPOSER) + mListLinkArg + "\">　作曲者別　</a></td>");
			oSB.Append("</tr>\n");
		}

		// --------------------------------------------------------------------
		// インデックスファイル名
		// --------------------------------------------------------------------
		private String IndexFileName(Boolean oIsAdult, String oKindFileName)
		{
			if (oKindFileName == KIND_FILE_NAME_CATEGORY)
			{
				return Path.GetFileNameWithoutExtension(TopFileName) + (oIsAdult ? "_" + YlCommon.AGE_LIMIT_CERO_Z.ToString() : null) + Path.GetExtension(TopFileName);
			}
			else
			{
				return FILE_NAME_PREFIX + "_index_" + oKindFileName + (oIsAdult ? "_" + YlCommon.AGE_LIMIT_CERO_Z.ToString() : null) + mListExt;
			}
		}

		// --------------------------------------------------------------------
		// 一時フォルダーからリストを移動
		// --------------------------------------------------------------------
		private void MoveList()
		{
			MoveListNew(false);
			MoveListNew(true);

			MoveListContentsCore(KIND_FILE_NAME_CATEGORY);
			MoveListIndexCore(false, KIND_FILE_NAME_CATEGORY);
			MoveListIndexCore(true, KIND_FILE_NAME_CATEGORY);

			MoveListContentsCore(KIND_FILE_NAME_TIE_UP_GROUP);
			MoveListIndexCore(false, KIND_FILE_NAME_TIE_UP_GROUP);
			MoveListIndexCore(true, KIND_FILE_NAME_TIE_UP_GROUP);

			MoveListContentsCore(KIND_FILE_NAME_PERIOD);
			MoveListIndexCore(false, KIND_FILE_NAME_PERIOD);
			MoveListIndexCore(true, KIND_FILE_NAME_PERIOD);

			MoveListContentsCore(KIND_FILE_NAME_SEASON);
			MoveListIndexCore(false, KIND_FILE_NAME_SEASON);
			MoveListIndexCore(true, KIND_FILE_NAME_SEASON);

			MoveListContentsCore(KIND_FILE_NAME_ARTIST);
			MoveListIndexCore(false, KIND_FILE_NAME_ARTIST);
			MoveListIndexCore(true, KIND_FILE_NAME_ARTIST);

			MoveListContentsCore(KIND_FILE_NAME_COMPOSER);
			MoveListIndexCore(false, KIND_FILE_NAME_COMPOSER);
			MoveListIndexCore(true, KIND_FILE_NAME_COMPOSER);
		}

		// --------------------------------------------------------------------
		// 一時フォルダーからリスト（インデックス以外）を移動
		// --------------------------------------------------------------------
		private void MoveListContentsCore(String oKindFileName)
		{
			String[] aListPathes = Directory.GetFiles(mTempFolderPath, FILE_NAME_PREFIX + "_" + oKindFileName + "_*" + mListExt);

			foreach (String aPath in aListPathes)
			{
				try
				{
					File.Move(aPath, FolderPath + Path.GetFileName(aPath));
				}
				catch (Exception)
				{
					LogWriter.ShowLogMessage(TraceEventType.Error, "リストファイル " + Path.GetFileName(aPath) + " を移動できませんでした。", true);
				}
			}
		}

		// --------------------------------------------------------------------
		// 一時フォルダーからリスト（インデックス）を移動
		// --------------------------------------------------------------------
		private void MoveListIndexCore(Boolean oIsAdult, String oKindFileName)
		{
			String aIndexFileName = IndexFileName(oIsAdult, oKindFileName);
			try
			{
				// File.Move() には上書きフラグが無いので File.Copy() を使う
				File.Copy(mTempFolderPath + aIndexFileName, FolderPath + aIndexFileName, true);
				File.Delete(mTempFolderPath + aIndexFileName);
			}
			catch (Exception)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "リストファイル " + Path.GetFileName(aIndexFileName) + " を移動できませんでした。", true);
			}
		}

		// --------------------------------------------------------------------
		// 一時フォルダーからリスト（新着）を移動
		// --------------------------------------------------------------------
		private void MoveListNew(Boolean oIsAdult)
		{
			String aNewFileName = OutputFileName(oIsAdult, KIND_FILE_NAME_NEW, "すべて", null);
			WebOutputSettings aWebOutputSettings = (WebOutputSettings)OutputSettings;
			if (aWebOutputSettings.EnableNew)
			{
				try
				{
					// File.Move() には上書きフラグが無いので File.Copy() を使う
					File.Copy(mTempFolderPath + aNewFileName, FolderPath + aNewFileName, true);
					File.Delete(mTempFolderPath + aNewFileName);
				}
				catch (Exception)
				{
					LogWriter.ShowLogMessage(TraceEventType.Error, "リストファイル " + aNewFileName + " を移動できませんでした。", true);
				}
			}
			else
			{
				try
				{
					File.Delete(FolderPath + aNewFileName);
				}
				catch (Exception)
				{
				}
			}
		}

		// --------------------------------------------------------------------
		// グループ＝歌手別、ページ＝頭文字、章＝歌手名、でファイル出力
		// --------------------------------------------------------------------
		private void OutputArtistAndHeads(Boolean oIsNewExists)
		{
			OutputArtistAndHeadsCore(false, oIsNewExists);
			OutputArtistAndHeadsCore(true, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// グループ＝歌手別、ページ＝頭文字、章＝歌手名、でファイル出力
		// --------------------------------------------------------------------
		private void OutputArtistAndHeadsCore(Boolean oIsAdult, Boolean oIsNewExists)
		{
			// 歌手別（1 要素のみ）とそれに紐付く頭文字群
			Dictionary<String, List<PageInfo>> aArtistsAndHeads = new Dictionary<String, List<PageInfo>>();

			// 歌手名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aArtistNamesAndTFounds = new Dictionary<String, List<TFound>>();

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where x.ArtistName != null && (oIsAdult ? x.TieUpAgeLimit >= YlCommon.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlCommon.AGE_LIMIT_CERO_Z)
					orderby x.ArtistRuby, x.ArtistName, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;
			String aPrevArtistHead = null;
			String aArtistHead = null;

			foreach (TFound aTFound in aQueryResult)
			{
				aArtistHead = !String.IsNullOrEmpty(aTFound.ArtistRuby) ? YlCommon.Head(aTFound.ArtistRuby) : YlCommon.Head(aTFound.ArtistName);

				if (aPrevTFound != null
						&& aArtistHead != aPrevArtistHead)
				{
					// ページが新しくなったので 1 ページ分出力
					OutputOneList(aArtistsAndHeads, aArtistNamesAndTFounds, oIsAdult, "歌手別", KIND_FILE_NAME_ARTIST, "歌手別", aPrevArtistHead,
							OutputItems.ArtistName, oIsNewExists);
				}

				if (/*aPrevTFound == null*/aArtistNamesAndTFounds.Count == 0
						|| aPrevTFound != null && aTFound.ArtistName != aPrevTFound.ArtistName)
				{
					// 歌手名が新しくなった
					aArtistNamesAndTFounds[aTFound.ArtistName] = new List<TFound>();
				}

				// 曲情報追加
				aArtistNamesAndTFounds[aTFound.ArtistName].Add(aTFound);

				// ループ処理
				aPrevTFound = aTFound;
				aPrevArtistHead = aArtistHead;
			}

			if (aPrevTFound != null)
			{
				OutputOneList(aArtistsAndHeads, aArtistNamesAndTFounds, oIsAdult, "歌手別", KIND_FILE_NAME_ARTIST, "歌手別", aPrevArtistHead,
						OutputItems.ArtistName, oIsNewExists);
			}

			// インデックスページ
			OutputIndexPage(aArtistsAndHeads, oIsAdult, KIND_FILE_NAME_ARTIST, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// グループ＝カテゴリー、ページ＝頭文字、章＝番組名、でファイル出力
		// --------------------------------------------------------------------
		private void OutputCategoryAndHeads(Boolean oIsNewExists)
		{
			OutputCategoryAndHeadsCore(false, oIsNewExists);
			OutputCategoryAndHeadsCore(true, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// グループ＝カテゴリー、ページ＝頭文字、章＝番組名、でファイル出力
		// --------------------------------------------------------------------
		private void OutputCategoryAndHeadsCore(Boolean oIsAdult, Boolean oIsNewExists)
		{
			// カテゴリーとそれに紐付く頭文字群
			Dictionary<String, List<PageInfo>> aCategoriesAndHeads = new Dictionary<String, List<PageInfo>>();

			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where oIsAdult ? x.TieUpAgeLimit >= YlCommon.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlCommon.AGE_LIMIT_CERO_Z
					orderby x.Category, x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;

			foreach (TFound aTFound in aQueryResult)
			{
				if (aPrevTFound != null
						&& (aTFound.Category != aPrevTFound.Category || aTFound.Head != aPrevTFound.Head))
				{
					// カテゴリーまたはページが新しくなったので 1 ページ分出力
					OutputOneList(aCategoriesAndHeads, aTieUpNamesAndTFounds, oIsAdult, "カテゴリー別", KIND_FILE_NAME_CATEGORY, aPrevTFound.Category,
							aPrevTFound.Head, OutputItems.TieUpName, oIsNewExists);
				}

				if (/*aPrevTFound == null*/aTieUpNamesAndTFounds.Count == 0
						|| aPrevTFound != null && aTFound.TieUpName != aPrevTFound.TieUpName)
				{
					// 番組名が新しくなった
					aTieUpNamesAndTFounds[aTFound.TieUpName] = new List<TFound>();
				}

				// 曲情報追加
				aTieUpNamesAndTFounds[aTFound.TieUpName].Add(aTFound);

				// ループ処理
				aPrevTFound = aTFound;
			}

			if (aPrevTFound != null)
			{
				OutputOneList(aCategoriesAndHeads, aTieUpNamesAndTFounds, oIsAdult, "カテゴリー別", KIND_FILE_NAME_CATEGORY, aPrevTFound.Category,
						aPrevTFound.Head, OutputItems.TieUpName, oIsNewExists);
			}

			// インデックスページ
			OutputIndexPage(aCategoriesAndHeads, oIsAdult, KIND_FILE_NAME_CATEGORY, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// グループ＝作曲者別、ページ＝頭文字、章＝作曲者名、でファイル出力
		// --------------------------------------------------------------------
		private void OutputComposerAndHeads(Boolean oIsNewExists)
		{
			OutputComposerAndHeads(false, oIsNewExists);
			OutputComposerAndHeads(true, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// グループ＝作曲者別、ページ＝頭文字、章＝作曲者名、でファイル出力
		// --------------------------------------------------------------------
		private void OutputComposerAndHeads(Boolean oIsAdult, Boolean oIsNewExists)
		{
			// 作曲者別（1 要素のみ）とそれに紐付く頭文字群
			Dictionary<String, List<PageInfo>> aComposersAndHeads = new Dictionary<String, List<PageInfo>>();

			// 作曲者名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aComposerNamesAndTFounds = new Dictionary<String, List<TFound>>();

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where x.ComposerName != null && (oIsAdult ? x.TieUpAgeLimit >= YlCommon.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlCommon.AGE_LIMIT_CERO_Z)
					orderby x.ComposerRuby, x.ComposerName, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;
			String aPrevComposerHead = null;
			String aComposerHead = null;

			foreach (TFound aTFound in aQueryResult)
			{
				aComposerHead = !String.IsNullOrEmpty(aTFound.ComposerRuby) ? YlCommon.Head(aTFound.ComposerRuby) : YlCommon.Head(aTFound.ComposerName);

				if (aPrevTFound != null
						&& aComposerHead != aPrevComposerHead)
				{
					// ページが新しくなったので 1 ページ分出力
					OutputOneList(aComposersAndHeads, aComposerNamesAndTFounds, oIsAdult, "作曲者別", KIND_FILE_NAME_COMPOSER, "作曲者別", aPrevComposerHead,
							OutputItems.ComposerName, oIsNewExists);
				}

				if (/*aPrevTFound == null*/aComposerNamesAndTFounds.Count == 0
						|| aPrevTFound != null && aTFound.ComposerName != aPrevTFound.ComposerName)
				{
					// 作曲者名が新しくなった
					aComposerNamesAndTFounds[aTFound.ComposerName] = new List<TFound>();
				}

				// 曲情報追加
				aComposerNamesAndTFounds[aTFound.ComposerName].Add(aTFound);

				// ループ処理
				aPrevTFound = aTFound;
				aPrevComposerHead = aComposerHead;
			}

			if (aPrevTFound != null)
			{
				OutputOneList(aComposersAndHeads, aComposerNamesAndTFounds, oIsAdult, "作曲者別", KIND_FILE_NAME_COMPOSER, "作曲者別", aPrevComposerHead,
						OutputItems.ComposerName, oIsNewExists);
			}

			// インデックスページ
			OutputIndexPage(aComposersAndHeads, oIsAdult, KIND_FILE_NAME_COMPOSER, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// CSS を出力
		// --------------------------------------------------------------------
		private void OutputCss()
		{
			String aTemplate = LoadTemplate("HtmlCss");
			File.WriteAllText(FolderPath + "List.css", aTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// リストファイル名
		// oPageName は null 可
		// --------------------------------------------------------------------
		private String OutputFileName(Boolean oIsAdult, String oKindFileName, String oGroupName, String oPageName)
		{
			return FILE_NAME_PREFIX + "_" + oKindFileName + "_" + (oIsAdult ? YlCommon.AGE_LIMIT_CERO_Z.ToString() + "_" : null)
					+ StringToHex(oGroupName) + (String.IsNullOrEmpty(oPageName) ? null : "_" + StringToHex(oPageName)) + mListExt;
		}

		// --------------------------------------------------------------------
		// インデックスページ（ページは任意の文字列ごと）を出力
		// --------------------------------------------------------------------
		private void OutputFreestyleIndexPage(Dictionary<String, List<PageInfo>> oGroupsAndPages, Boolean oIsAdult, String oKindFileName, Boolean oIsNewExists)
		{
			Int32 aNumTotalSongs = 0;
			Int32 aGroupIndex = 0;
			StringBuilder aSB = new StringBuilder();
			KeyValuePair<String, List<PageInfo>> aMiscGroupAndPages = new KeyValuePair<String, List<PageInfo>>(null, null);

			// その他以外
			foreach (KeyValuePair<String, List<PageInfo>> aGroupAndPages in oGroupsAndPages)
			{
				if (aGroupAndPages.Key == YlCommon.GROUP_MISC)
				{
					aMiscGroupAndPages = aGroupAndPages;
					continue;
				}

				aNumTotalSongs += OutputFreestyleIndexPageOneGroup(aSB, aGroupAndPages, aGroupIndex, oIsAdult, oKindFileName);
				aGroupIndex++;
			}

			// その他
			if (aMiscGroupAndPages.Key != null)
			{
				aNumTotalSongs += OutputFreestyleIndexPageOneGroup(aSB, aMiscGroupAndPages, aGroupIndex, oIsAdult, oKindFileName);
			}

			// インデックスページ
			String aTopTemplate = LoadTemplate("HtmlIndex");
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_TITLE, mDirectoryTopName);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_ADDITIONAL_HEADER, mAdditionalHeader);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GROUP_NAVI, GroupNavi(oIsNewExists));
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_DIRECTORY, mDirectoryTopLink);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_INDICES, aSB.ToString());
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_NUM_SONGS, "（合計 " + aNumTotalSongs.ToString("#,0") + " 曲）");
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GENERATOR, YlCommon.APP_NAME_J + "  " + YlCommon.APP_VER);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GENERATE_DATE, DateTime.Now.ToString(YlCommon.DATE_FORMAT));

			File.WriteAllText(mTempFolderPath + IndexFileName(oIsAdult, oKindFileName), aTopTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// インデックスページの 1 グループ分の文字列を出力
		// --------------------------------------------------------------------
		private Int32 OutputFreestyleIndexPageOneGroup(StringBuilder oSB, KeyValuePair<String, List<PageInfo>> oGroupAndPages, Int32 oGroupIndex, Boolean oIsAdult,
				String oKindFileName)
		{
			String aGroupName = oGroupAndPages.Key;
			StringBuilder aSbPages = new StringBuilder();
			Int32 aNumSongs = 0;

			String aOneTemplate = LoadTemplate("HtmlFreestyleIndexOneGroup");
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_CATEGORY, aGroupName);

			foreach (PageInfo aPageInfo in oGroupAndPages.Value)
			{
				aSbPages.Append("<tr><td class=\"exist\"><a href=\"" + OutputFileName(oIsAdult, oKindFileName, aGroupName, aPageInfo.Name) + mListLinkArg + "\">"
						+ aPageInfo.Name + " （" + aPageInfo.NumSongs.ToString("#,0") + " 曲）</a></td></tr>");
				aNumSongs += aPageInfo.NumSongs;
			}

			aOneTemplate = aOneTemplate.Replace(HTML_VAR_PAGES, aSbPages.ToString());
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_CATEGORY_INDEX, oGroupIndex.ToString());
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_NUM_SONGS, "（" + aNumSongs.ToString("#,0") + " 曲）");
			oSB.Append(aOneTemplate);

			return aNumSongs;
		}

		// --------------------------------------------------------------------
		// インデックスページ（ページは頭文字ごと）を出力
		// --------------------------------------------------------------------
		private void OutputIndexPage(Dictionary<String, List<PageInfo>> oGroupsAndPages, Boolean oIsAdult, String oKindFileName, Boolean oIsNewExists)
		{
			Int32 aNumTotalSongs = 0;
			Int32 aGroupIndex = 0;
			StringBuilder aSB = new StringBuilder();
			KeyValuePair<String, List<PageInfo>> aMiscGroupAndPages = new KeyValuePair<String, List<PageInfo>>(null, null);

			// その他以外
			foreach (KeyValuePair<String, List<PageInfo>> aGroupAndPages in oGroupsAndPages)
			{
				if (aGroupAndPages.Key == YlCommon.GROUP_MISC)
				{
					aMiscGroupAndPages = aGroupAndPages;
					continue;
				}

				aNumTotalSongs += OutputIndexPageOneGroup(aSB, aGroupAndPages, aGroupIndex, oIsAdult, oKindFileName);
				aGroupIndex++;
			}

			// その他
			if (aMiscGroupAndPages.Key != null)
			{
				aNumTotalSongs += OutputIndexPageOneGroup(aSB, aMiscGroupAndPages, aGroupIndex, oIsAdult, oKindFileName);
			}

			// インデックスページ
			String aTopTemplate = LoadTemplate("HtmlIndex");
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_TITLE, mDirectoryTopName);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_ADDITIONAL_HEADER, mAdditionalHeader);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GROUP_NAVI, GroupNavi(oIsNewExists));
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_DIRECTORY, mDirectoryTopLink);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_INDICES, aSB.ToString());
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_NUM_SONGS, "（合計 " + aNumTotalSongs.ToString("#,0") + " 曲）");
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GENERATOR, YlCommon.APP_NAME_J + "  " + YlCommon.APP_VER);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GENERATE_DATE, DateTime.Now.ToString(YlCommon.DATE_FORMAT));

			File.WriteAllText(mTempFolderPath + IndexFileName(oIsAdult, oKindFileName), aTopTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// インデックスページの 1 グループ分の文字列を出力
		// --------------------------------------------------------------------
		private Int32 OutputIndexPageOneGroup(StringBuilder oSB, KeyValuePair<String, List<PageInfo>> oGroupAndPages, Int32 oGroupIndex,
				Boolean oIsAdult, String oKindFileName)
		{
			String aGroupName = oGroupAndPages.Key;
			Boolean aHasKana = false;
			Boolean aHasMisc = false;
			Int32 aNumSongs = 0;

			String aOneTemplate = LoadTemplate("HtmlIndexOneGroup");
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_CATEGORY, aGroupName);

			foreach (PageInfo aPageInfo in oGroupAndPages.Value)
			{
				aOneTemplate = aOneTemplate.Replace("<td>" + aPageInfo.Name + "</td>", "<td class=\"exist\"><a href=\""
						+ OutputFileName(oIsAdult, oKindFileName, aGroupName, aPageInfo.Name) + mListLinkArg + "\">" + aPageInfo.Name + "</a></td>");
				aNumSongs += aPageInfo.NumSongs;

				if (aPageInfo.Name == YlCommon.HEAD_MISC)
				{
					aHasMisc = true;
				}
				else
				{
					aHasKana = true;
				}
			}

			aOneTemplate = aOneTemplate.Replace(HTML_VAR_CATEGORY_INDEX, oGroupIndex.ToString());
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_NUM_SONGS, "（" + aNumSongs.ToString("#,0") + " 曲）");
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_CLASS_OF_KANA, aHasKana ? null : CLASS_NAME_INVISIBLE);
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_CLASS_OF_MISC, aHasMisc ? null : CLASS_NAME_INVISIBLE);
			oSB.Append(aOneTemplate);

			return aNumSongs;
		}

		// --------------------------------------------------------------------
		// JS を出力
		// --------------------------------------------------------------------
		private void OutputJs()
		{
			String aTemplate = LoadTemplate("HtmlJs");
			File.WriteAllText(FolderPath + "List.js", aTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// グループ＝NEW、ページ＝NEW、章＝番組名、でファイル出力（新着）
		// --------------------------------------------------------------------
		private Boolean OutputNew()
		{
			WebOutputSettings aWebOutputSettings = (WebOutputSettings)OutputSettings;
			if (!aWebOutputSettings.EnableNew)
			{
				return false;
			}

			OutputNewCore(false, aWebOutputSettings.NewDays);
			OutputNewCore(true, aWebOutputSettings.NewDays);
			return true;
		}

		// --------------------------------------------------------------------
		// グループ＝NEW、ページ＝NEW、章＝番組名、でファイル出力（新着）
		// --------------------------------------------------------------------
		private void OutputNewCore(Boolean oIsAdult, Int32 oNewDays)
		{
			// ダミー
			Dictionary<String, List<PageInfo>> aDummy = new Dictionary<String, List<PageInfo>>();

			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			Double aNewDate = JulianDay.DateTimeToModifiedJulianDate(DateTime.Now.AddDays(-oNewDays));

			StringBuilder aSB = new StringBuilder();
			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where x.LastWriteTime >= aNewDate && (oIsAdult ? x.TieUpAgeLimit >= YlCommon.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlCommon.AGE_LIMIT_CERO_Z)
					orderby x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;

			foreach (TFound aTFound in aQueryResult)
			{
				if (aPrevTFound == null
						|| aPrevTFound != null && aTFound.TieUpName != aPrevTFound.TieUpName)
				{
					// 番組名が新しくなった
					aTieUpNamesAndTFounds[aTFound.TieUpName] = new List<TFound>();
				}

				// 曲情報追加
				aTieUpNamesAndTFounds[aTFound.TieUpName].Add(aTFound);

				// ループ処理
				aPrevTFound = aTFound;
			}

			OutputOneList(aDummy, aTieUpNamesAndTFounds, oIsAdult, "新着", KIND_FILE_NAME_NEW, "すべて", null, OutputItems.TieUpName, true);
		}

		// --------------------------------------------------------------------
		// 更新中を表示する出力
		// --------------------------------------------------------------------
		private void OutputNoticeIndexes()
		{
			// ヘッダー部分に新着へのリンクを張るかどうかを調査するために設定を読む
			WebOutputSettings aWebOutputSettings = (WebOutputSettings)OutputSettings;

			// 内容
			String aTopTemplate = LoadTemplate("HtmlIndexNotice");
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_ADDITIONAL_HEADER, mAdditionalHeader);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GROUP_NAVI, GroupNavi(aWebOutputSettings.EnableNew));
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GENERATOR, YlCommon.APP_NAME_J + "  " + YlCommon.APP_VER);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_GENERATE_DATE, DateTime.Now.ToString(YlCommon.DATE_FORMAT));

			// 新着（実際には新着が無い場合でも、更新中リストとしては 404 にならないように新着も出力しておく）
			File.WriteAllText(FolderPath + OutputFileName(false, KIND_FILE_NAME_NEW, "すべて", null), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + OutputFileName(true, KIND_FILE_NAME_NEW, "すべて", null), aTopTemplate, Encoding.UTF8);

			// インデックス系
			File.WriteAllText(FolderPath + IndexFileName(false, KIND_FILE_NAME_CATEGORY), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(true, KIND_FILE_NAME_CATEGORY), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(false, KIND_FILE_NAME_TIE_UP_GROUP), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(true, KIND_FILE_NAME_TIE_UP_GROUP), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(false, KIND_FILE_NAME_PERIOD), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(true, KIND_FILE_NAME_PERIOD), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(false, KIND_FILE_NAME_SEASON), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(true, KIND_FILE_NAME_SEASON), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(false, KIND_FILE_NAME_ARTIST), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(true, KIND_FILE_NAME_ARTIST), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(false, KIND_FILE_NAME_COMPOSER), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(true, KIND_FILE_NAME_COMPOSER), aTopTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// 1 ページ分のリストを出力
		// ＜引数＞ oChaptersAndTFounds: 章（橙色の区切り）ごとの楽曲群
		// --------------------------------------------------------------------
		private void OutputOneList(Dictionary<String, List<PageInfo>> oGroupsAndPages, Dictionary<String, List<TFound>> oChaptersAndTFounds, Boolean oIsAdult,
				String oKindName, String oKindFileName, String oGroupName, String oPageName, OutputItems oChapterItem, Boolean oIsNewExists)
		{
			String aTemplate = LoadTemplate("HtmlList");

			// null を調整
			if (String.IsNullOrEmpty(oGroupName))
			{
				oGroupName = YlCommon.GROUP_MISC;
			}

			// リスト本体部分
			StringBuilder aSB = new StringBuilder();
			Int32 aNumPageSongs = 0;
			Int32 aChapterIndex = 0;
			foreach (KeyValuePair<String, List<TFound>> aChapterAndTFounds in oChaptersAndTFounds)
			{
				List<TFound> aList = aChapterAndTFounds.Value;
				BeginChapter(aSB, oChapterItem, aChapterIndex, oChaptersAndTFounds.Count, aList);
				for (Int32 i = 0; i < aList.Count; i++)
				{
					AppendSongInfo(aSB, oChapterItem, i, aList[i]);
				}
				EndChapter(aSB);

				aNumPageSongs += aList.Count;
				aChapterIndex++;
			}

			// テンプレート適用
			String aDirectory = " &gt; " + oKindName + " &gt; " + oGroupName + (String.IsNullOrEmpty(oPageName) ? null : " &gt; " + oPageName);
			aTemplate = aTemplate.Replace(HTML_VAR_TITLE, mDirectoryTopName + aDirectory);
			aTemplate = aTemplate.Replace(HTML_VAR_ADDITIONAL_HEADER, mAdditionalHeader);
			aTemplate = aTemplate.Replace(HTML_VAR_GROUP_NAVI, GroupNavi(oIsNewExists));
			aTemplate = aTemplate.Replace(HTML_VAR_DIRECTORY, mDirectoryTopLink + aDirectory);
			aTemplate = aTemplate.Replace(HTML_VAR_NUM_SONGS, "（" + aNumPageSongs.ToString("#,0") + " 曲）");
			aTemplate = aTemplate.Replace(HTML_VAR_ADDITIONAL_DESCRIPTION, mAdditionalDescription);
			aTemplate = aTemplate.Replace(HTML_VAR_GENERATOR, YlCommon.APP_NAME_J + "  " + YlCommon.APP_VER);
			aTemplate = aTemplate.Replace(HTML_VAR_GENERATE_DATE, DateTime.Now.ToString(YlCommon.DATE_FORMAT));
			aTemplate = aTemplate.Replace(HTML_VAR_PROGRAMS, aSB.ToString());

			File.WriteAllText(mTempFolderPath + OutputFileName(oIsAdult, oKindFileName, oGroupName, oPageName), aTemplate, Encoding.UTF8);

			// 出力済みの内容をクリア
			oChaptersAndTFounds.Clear();

			// グループとページの情報を追加
			PageInfo aPageInfo = new PageInfo();
			aPageInfo.Name = oPageName;
			aPageInfo.NumSongs = aNumPageSongs;
			if (oGroupsAndPages.ContainsKey(oGroupName))
			{
				oGroupsAndPages[oGroupName].Add(aPageInfo);
			}
			else
			{
				List<PageInfo> aPageInfos = new List<PageInfo>();
				aPageInfos.Add(aPageInfo);
				oGroupsAndPages[oGroupName] = aPageInfos;
			}
		}

		// --------------------------------------------------------------------
		// グループ＝年代、ページ＝頭文字、章＝番組名、でファイル出力
		// --------------------------------------------------------------------
		private void OutputPeriodAndHeads(Boolean oIsNewExists)
		{
			OutputPeriodAndHeadsCore(false, oIsNewExists);
			OutputPeriodAndHeadsCore(true, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// グループ＝年代、ページ＝頭文字、章＝番組名、でファイル出力
		// --------------------------------------------------------------------
		private void OutputPeriodAndHeadsCore(Boolean oIsAdult, Boolean oIsNewExists)
		{
			// 年代とそれに紐付く頭文字群
			Dictionary<String, List<PageInfo>> aPeriodsAndHeads = new Dictionary<String, List<PageInfo>>();

			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			// 年月日設定
			Int32 aSinceYear = DateTime.UtcNow.Year;
			aSinceYear -= aSinceYear % 10;

			while (aSinceYear > YlCommon.INVALID_YEAR)
			{
				Int32 aUntilYear = aSinceYear + 10;

				IQueryable<TFound> aQueryResult =
						from x in TableFound
						where JulianDay.DateTimeToModifiedJulianDate(new DateTime(aSinceYear, 1, 1)) <= x.SongReleaseDate
						&& x.SongReleaseDate < JulianDay.DateTimeToModifiedJulianDate(new DateTime(aUntilYear, 1, 1))
						&& (oIsAdult ? x.TieUpAgeLimit >= YlCommon.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlCommon.AGE_LIMIT_CERO_Z)
						orderby x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
						select x;
				TFound aPrevTFound = null;

				foreach (TFound aTFound in aQueryResult)
				{
					if (aPrevTFound != null
							&& aTFound.Head != aPrevTFound.Head)
					{
						// 頭文字が新しくなったので 1 ページ分出力
						OutputOneList(aPeriodsAndHeads, aTieUpNamesAndTFounds, oIsAdult, "年代別", KIND_FILE_NAME_PERIOD, aSinceYear.ToString() + " 年代",
								aPrevTFound.Head, OutputItems.TieUpName, oIsNewExists);
					}

					if (aPrevTFound == null
							|| aPrevTFound != null && aTFound.TieUpName != aPrevTFound.TieUpName)
					{
						// 番組名が新しくなった
						aTieUpNamesAndTFounds[aTFound.TieUpName] = new List<TFound>();
					}

					// 曲情報追加
					aTieUpNamesAndTFounds[aTFound.TieUpName].Add(aTFound);

					// ループ処理
					aPrevTFound = aTFound;
				}

				if (aPrevTFound != null)
				{
					OutputOneList(aPeriodsAndHeads, aTieUpNamesAndTFounds, oIsAdult, "年代別", KIND_FILE_NAME_PERIOD, aSinceYear.ToString() + " 年代",
							aPrevTFound.Head, OutputItems.TieUpName, oIsNewExists);
				}

				aSinceYear -= 10;
			}

			// インデックスページ
			OutputIndexPage(aPeriodsAndHeads, oIsAdult, KIND_FILE_NAME_PERIOD, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// グループ＝タイアップグループ名の頭文字、ページ＝タイアップグループ名、章＝番組名、でファイル出力
		// --------------------------------------------------------------------
		private void OutputTieUpGroupHeadAndTieUpGroups(Boolean oIsNewExists)
		{
			OutputTieUpGroupHeadAndTieUpGroupsCore(false, oIsNewExists);
			OutputTieUpGroupHeadAndTieUpGroupsCore(true, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// グループ＝タイアップグループ名の頭文字、ページ＝タイアップグループ名、章＝番組名、でファイル出力
		// --------------------------------------------------------------------
		private void OutputTieUpGroupHeadAndTieUpGroupsCore(Boolean oIsAdult, Boolean oIsNewExists)
		{
			// 頭文字とそれに紐付くタイアップグループ群
			Dictionary<String, List<PageInfo>> aHeadsAndTieUpGroups = new Dictionary<String, List<PageInfo>>();

			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where x.TieUpGroupName != null && (oIsAdult ? x.TieUpAgeLimit >= YlCommon.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlCommon.AGE_LIMIT_CERO_Z)
					orderby x.TieUpGroupRuby, x.TieUpGroupName, x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;
			String aPrevTieUpGroupHead = null;

			foreach (TFound aTFound in aQueryResult)
			{
				String aTieUpGroupHead = !String.IsNullOrEmpty(aTFound.TieUpGroupRuby) ? YlCommon.Head(aTFound.TieUpGroupRuby) : YlCommon.Head(aTFound.TieUpGroupName);

				if (aPrevTFound != null
						&& (aTieUpGroupHead != aPrevTieUpGroupHead || aTFound.TieUpGroupRuby != aPrevTFound.TieUpGroupRuby || aTFound.TieUpGroupName != aPrevTFound.TieUpGroupName))
				{
					// 頭文字またはページが新しくなったので 1 ページ分出力
					OutputOneList(aHeadsAndTieUpGroups, aTieUpNamesAndTFounds, oIsAdult, "シリーズ別", KIND_FILE_NAME_TIE_UP_GROUP, aPrevTieUpGroupHead,
							aPrevTFound.TieUpGroupName + "シリーズ", OutputItems.TieUpName, oIsNewExists);
				}

				if (aPrevTFound == null
						|| aPrevTFound != null && aTFound.TieUpName != aPrevTFound.TieUpName)
				{
					// 番組名が新しくなった
					aTieUpNamesAndTFounds[aTFound.TieUpName] = new List<TFound>();
				}

				// 曲情報追加
				aTieUpNamesAndTFounds[aTFound.TieUpName].Add(aTFound);

				// ループ処理
				aPrevTFound = aTFound;
				aPrevTieUpGroupHead = aTieUpGroupHead;
			}

			if (aPrevTFound != null)
			{
				OutputOneList(aHeadsAndTieUpGroups, aTieUpNamesAndTFounds, oIsAdult, "シリーズ別", KIND_FILE_NAME_TIE_UP_GROUP, aPrevTieUpGroupHead,
						aPrevTFound.TieUpGroupName + "シリーズ", OutputItems.TieUpName, oIsNewExists);
			}

			// インデックスページ
			OutputFreestyleIndexPage(aHeadsAndTieUpGroups, oIsAdult, KIND_FILE_NAME_TIE_UP_GROUP, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// グループ＝年、ページ＝季節、章＝番組名、でファイル出力
		// 直近 SEASON_YEARS 年分のみ
		// --------------------------------------------------------------------
		private void OutputYearsAndSeasons(Boolean oIsNewExists)
		{
			OutputYearsAndSeasonsCore(false, oIsNewExists);
			OutputYearsAndSeasonsCore(true, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// グループ＝年、ページ＝季節、章＝番組名、でファイル出力
		// 直近 SEASON_YEARS 年分のみ
		// --------------------------------------------------------------------
		private void OutputYearsAndSeasonsCore(Boolean oIsAdult, Boolean oIsNewExists)
		{
			// 年とそれに紐付く季節群
			Dictionary<String, List<PageInfo>> aYearsAndSeasons = new Dictionary<String, List<PageInfo>>();

			// 年月日設定
			Int32 aSinceYear = DateTime.UtcNow.Year;
			Int32 aUntilYear = aSinceYear - SEASON_YEARS;

			while (aSinceYear > aUntilYear)
			{
				OutputYearsAndSeasonsOneSeason(aYearsAndSeasons, oIsAdult, aSinceYear, 1, aSinceYear, 4, "1 月～3 月：冬", oIsNewExists);
				OutputYearsAndSeasonsOneSeason(aYearsAndSeasons, oIsAdult, aSinceYear, 4, aSinceYear, 7, "4 月～6 月：春", oIsNewExists);
				OutputYearsAndSeasonsOneSeason(aYearsAndSeasons, oIsAdult, aSinceYear, 7, aSinceYear, 10, "7 月～9 月：夏", oIsNewExists);
				OutputYearsAndSeasonsOneSeason(aYearsAndSeasons, oIsAdult, aSinceYear, 10, aSinceYear + 1, 1, "10 月～12 月：秋", oIsNewExists);
				aSinceYear--;
			}

			// インデックスページ
			OutputFreestyleIndexPage(aYearsAndSeasons, oIsAdult, KIND_FILE_NAME_SEASON, oIsNewExists);
		}

		// --------------------------------------------------------------------
		// 1 期分を出力
		// --------------------------------------------------------------------
		private void OutputYearsAndSeasonsOneSeason(Dictionary<String, List<PageInfo>> oYearsAndSeasons, Boolean oIsAdult, Int32 oSinceYear, Int32 oSinceMonth,
				Int32 oUntilYear, Int32 oUntilMonth, String oSeasonName, Boolean oIsNewExists)
		{
			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where JulianDay.DateTimeToModifiedJulianDate(new DateTime(oSinceYear, oSinceMonth, 1)) <= x.SongReleaseDate
					&& x.SongReleaseDate < JulianDay.DateTimeToModifiedJulianDate(new DateTime(oUntilYear, oUntilMonth, 1))
					&& (oIsAdult ? x.TieUpAgeLimit >= YlCommon.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlCommon.AGE_LIMIT_CERO_Z)
					orderby x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;

			foreach (TFound aTFound in aQueryResult)
			{
				if (aPrevTFound == null
						|| aPrevTFound != null && aTFound.TieUpName != aPrevTFound.TieUpName)
				{
					// 番組名が新しくなった
					aTieUpNamesAndTFounds[aTFound.TieUpName] = new List<TFound>();
				}

				// 曲情報追加
				aTieUpNamesAndTFounds[aTFound.TieUpName].Add(aTFound);

				// ループ処理
				aPrevTFound = aTFound;
			}

			if (aPrevTFound != null)
			{
				OutputOneList(oYearsAndSeasons, aTieUpNamesAndTFounds, oIsAdult, "期別", KIND_FILE_NAME_SEASON, oSinceYear.ToString() + " 年",
						oSeasonName, OutputItems.TieUpName, oIsNewExists);
			}
		}

		// --------------------------------------------------------------------
		// 文字を UTF-8 HEX に変換
		// --------------------------------------------------------------------
		private String StringToHex(String oString)
		{
			Byte[] aByteData = Encoding.UTF8.GetBytes(oString);
			return BitConverter.ToString(aByteData).Replace("-", String.Empty).ToLower();
		}

		// --------------------------------------------------------------------
		// 新着テキストボックスの状態を更新
		// --------------------------------------------------------------------
		private void UpdateTextBoxNewDays()
		{
			TextBoxNewDays.Enabled = CheckBoxEnableNew.Checked;
		}


	}
	// public class HtmlOutputWriter ___END___

	// ====================================================================
	// リスト 1 ページの情報
	// ====================================================================

	public class PageInfo
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public PageInfo()
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ページ名
		public String Name { get; set; }

		// 当該頭文字の曲数
		public Int32 NumSongs { get; set; }

	}
	// public class PageInfo ___END___

}
// namespace YukaLister.Shared ___END___
