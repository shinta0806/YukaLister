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
using System.IO;
using System.Linq;
using System.Text;

using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;
using YukaLister.ViewModels;

namespace YukaLister.Models.OutputWriters
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
		public WebOutputWriter(EnvironmentModel oEnvironment) : base(oEnvironment)
		{
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リスト出力設定ウィンドウの ViewModel を生成
		// --------------------------------------------------------------------
		public override OutputSettingsWindowViewModel CreateOutputSettingsWindowViewModel()
		{
			return new WebOutputSettingsWindowViewModel();
		}

		// --------------------------------------------------------------------
		// リスト出力
		// --------------------------------------------------------------------
		public override void Output()
		{
			PrepareOutput();

			// 内容の生成
			// 生成の順番は GroupNaviCore() と合わせる
			GenerateNew();
			GenerateCategoryAndHeads();
			GenerateTieUpGroupHeadAndTieUpGroups();
			GenerateYearsAndSeasons();
			GeneratePeriodAndHeads();
			GenerateArtistAndHeads();
			GenerateComposerAndHeads();
			GenerateTagHeadAndTags();

			// 内容の調整
			AdjustList(mTopPage);

			// 一時フォルダーへの出力
			OutputList(mTopPage);

			// インデックス系を「更新中」表示にする
			OutputNoticeIndexes();

			// 古いファイルを削除
			DeleteOldListContents();

			// 出力先フォルダーへの出力
			OutputCss();
			OutputJs();

			// その他のファイルの出力
			OutputMisc();

			// 一時フォルダーから移動
			MoveList();
		}

		// ====================================================================
		// protected 定数
		// ====================================================================

		// HTML テンプレートに記載されている変数
		protected const String HTML_VAR_ADDITIONAL_NAVI = "<!-- $AdditionalNavi$ -->";
		protected const String HTML_VAR_GENERATOR = "<!-- $Generator$ -->";

		// ====================================================================
		// protected メンバー変数
		// ====================================================================

		// リストの拡張子（ピリオド含む）
		protected String mListExt;

		// トップページ
		protected PageInfoTree mTopPage;

		// 追加説明
		protected String mAdditionalDescription;

		// 追加 HTML ヘッダー
		protected String mAdditionalHeader;

		// 追加階層ナビゲーション
		protected String mAdditionalNavi;

		// トップページからリストをリンクする際の引数
		protected String mListLinkArg;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 曲情報を文字列に追加する際のテーブル内容を追加
		// --------------------------------------------------------------------
		protected virtual void AppendSongInfoAddTd(StringBuilder oSB, OutputItems oChapterItem, TFound oTFound)
		{
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
						oSB.Append("<td>" + (oTFound.SmartTrackOnVocal ? YlConstants.SMART_TRACK_VALID_MARK : null) + "</td>");
						oSB.Append("<td>" + (oTFound.SmartTrackOffVocal ? YlConstants.SMART_TRACK_VALID_MARK : null) + "</td>");
						break;
					case OutputItems.Comment:
						oSB.Append("<td class=\"small\">" + oTFound.Comment + "</td>");
						break;
					case OutputItems.LastWriteTime:
						oSB.Append("<td class=\"small\">" + JulianDay.ModifiedJulianDateToDateTime(oTFound.LastWriteTime).ToString(
								YlConstants.DATE_FORMAT + " " + YlConstants.TIME_FORMAT) + "</td>");
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
						if (oTFound.SongReleaseDate <= YlConstants.INVALID_MJD)
						{
							oSB.Append("<td></td>");
						}
						else
						{
							oSB.Append("<td class=\"small\">" + JulianDay.ModifiedJulianDateToDateTime(oTFound.SongReleaseDate).ToString(YlConstants.DATE_FORMAT) + "</td>");
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
		}

		// --------------------------------------------------------------------
		// 章を開始する際のテーブル見出しを追加
		// --------------------------------------------------------------------
		protected virtual void BeginChapterAddTh(StringBuilder oSB, OutputItems oChapterItem)
		{
			foreach (OutputItems aOutputItem in mRuntimeOutputItems)
			{
				if (aOutputItem == oChapterItem)
				{
					continue;
				}

				oSB.Append("<th>" + mThNames[(Int32)aOutputItem] + "</th>");
			}
		}

		// --------------------------------------------------------------------
		// その他のファイルの削除
		// --------------------------------------------------------------------
		protected virtual void DeleteMisc()
		{
		}

		// --------------------------------------------------------------------
		// リストに出力するファイル名の表現
		// --------------------------------------------------------------------
		protected abstract String FileNameDescription(String oFileName);

		// --------------------------------------------------------------------
		// その他のファイルの出力
		// --------------------------------------------------------------------
		protected virtual void OutputMisc()
		{
		}

		// --------------------------------------------------------------------
		// コンストラクターでは行えない準備などを実施
		// --------------------------------------------------------------------
		protected override void PrepareOutput()
		{
			base.PrepareOutput();

			// ページ構造の基本を生成
			mTopPage = new PageInfoTree();
			mTopPage.Name = "曲一覧";
			mTopPage.FileName = IndexFileName(false, KIND_FILE_NAME_CATEGORY);

			PageInfoTree aGeneral = new PageInfoTree();
			aGeneral.Name = ZoneName(false);
			mTopPage.AddChild(aGeneral);

			PageInfoTree aAdult = new PageInfoTree();
			aAdult.Name = ZoneName(true);
			mTopPage.AddChild(aAdult);

			// テーブル項目名（原則 YlCommon.OUTPUT_ITEM_NAMES だが一部見やすいよう変更）
			mThNames = new List<String>(YlConstants.OUTPUT_ITEM_NAMES);
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
		private const String KIND_FILE_NAME_TAG = "Tag";
		private const String KIND_FILE_NAME_TIE_UP_GROUP = "Series";

		// HTML テンプレートに記載されている変数
		private const String HTML_VAR_ADDITIONAL_DESCRIPTION = "<!-- $AdditionalDescription$ -->";
		private const String HTML_VAR_ADDITIONAL_HEADER = "<!-- $AdditionalHeader$ -->";
		private const String HTML_VAR_CATEGORY = "<!-- $Category$ -->";
		private const String HTML_VAR_CATEGORY_INDEX = "<!-- $CategoryIndex$ -->";
		private const String HTML_VAR_CHAPTER_NAME = "<!-- $ChapterName$ -->";
		private const String HTML_VAR_CLASS_OF_AL = "<!-- $ClassOfAl$ -->";
		private const String HTML_VAR_CLASS_OF_KANA = "<!-- $ClassOfKana$ -->";
		private const String HTML_VAR_CLASS_OF_MISC = "<!-- $ClassOfMisc$ -->";
		private const String HTML_VAR_CLASS_OF_NUM = "<!-- $ClassOfNum$ -->";
		private const String HTML_VAR_DIRECTORY = "<!-- $Directory$ -->";
		private const String HTML_VAR_GENERATE_DATE = "<!-- $GenerateDate$ -->";
		private const String HTML_VAR_GROUP_NAVI = "<!-- $GroupNavi$ -->";
		private const String HTML_VAR_INDICES = "<!-- $Indices$ -->";
		private const String HTML_VAR_NEIGHBOR = "<!-- $Neighbor$ -->";
		private const String HTML_VAR_NEW = "<!-- $New$ -->";
		private const String HTML_VAR_NUM_SONGS = "<!-- $NumSongs$ -->";
		private const String HTML_VAR_PAGES = "<!-- $Pages$ -->";
		private const String HTML_VAR_PROGRAMS = "<!-- $Programs$ -->";
		private const String HTML_VAR_TITLE = "<!-- $Title$ -->";

		// テーブル非表示
		private const String CLASS_NAME_INVISIBLE = "class=\"invisible\"";

		// 期別リストの年数
		private const Int32 SEASON_YEARS = 5;

		// 文字列を HEX に変換する際の最大長
		// C:\Users\ユーザー名\AppData\Local\Temp\YukaLister\PID..\2_22\List_Artist_GroupName_Hex1_Hex2.html
		// Hex1 / Hex2 は MAX_HEX_SOURCE_LENGTH の 2 倍の長さになる
		// 長くなるのは Hex1 か Hex2 のどちらかという前提で、パスの長さが 256 を超えない程度の指定にする
		private const Int32 MAX_HEX_SOURCE_LENGTH = 70;

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
		// リストの内容を調整する
		// --------------------------------------------------------------------
		private void AdjustList(PageInfoTree oPageInfoTree)
		{
			// HTML テンプレートの内容にどのページでも使われる変数を適用する
			ReplaceListContent(oPageInfoTree, HTML_VAR_ADDITIONAL_HEADER, mAdditionalHeader);
			ReplaceListContent(oPageInfoTree, HTML_VAR_ADDITIONAL_NAVI, mAdditionalNavi);
			ReplaceListContent(oPageInfoTree, HTML_VAR_GROUP_NAVI, GroupNavi(((WebOutputSettings)OutputSettings).EnableNew));
			ReplaceListContent(oPageInfoTree, HTML_VAR_GENERATOR, YlConstants.APP_NAME_J + "  " + YlConstants.APP_VER);
			ReplaceListContent(oPageInfoTree, HTML_VAR_GENERATE_DATE, DateTime.Now.ToString(YlConstants.DATE_FORMAT));

			// その他の調整
			AdjustListMisc(oPageInfoTree);
		}

		// --------------------------------------------------------------------
		// その他の調整
		// --------------------------------------------------------------------
		private void AdjustListMisc(PageInfoTree oPageInfoTree)
		{
			// oPageInfoTree を調整
			if (!String.IsNullOrEmpty(oPageInfoTree.Content))
			{
				oPageInfoTree.Content = oPageInfoTree.Content.Replace(HTML_VAR_TITLE, oPageInfoTree.DirectoryText());
				oPageInfoTree.Content = oPageInfoTree.Content.Replace(HTML_VAR_DIRECTORY, oPageInfoTree.DirectoryLink(mListLinkArg));
				oPageInfoTree.Content = oPageInfoTree.Content.Replace(HTML_VAR_NUM_SONGS, oPageInfoTree.NumTotalSongs.ToString("#,0"));

				// 隣のページ
				if (oPageInfoTree.Parent != null & oPageInfoTree.Parent.Children.Count > 1)
				{
					List<PageInfoTree> aChildren = oPageInfoTree.Parent.Children;
					Int32 aIndex = aChildren.IndexOf(oPageInfoTree);
					StringBuilder aSB = new StringBuilder();
					aSB.Append("<table class=\"centering\"><tr>");
					if (aIndex > 0)
					{
						aSB.Append("<td class=\"exist\"><a href=\"" + aChildren[aIndex - 1].FileName + mListLinkArg + "\">　&lt;&lt;　"
								+ aChildren[aIndex - 1].Name + "　</a></td>");
					}
					aSB.Append("<td>　" + oPageInfoTree.Parent.Name + "　" + oPageInfoTree.Name + "　</td>");
					if (aIndex < aChildren.Count - 1)
					{
						aSB.Append("<td class=\"exist\"><a href=\"" + aChildren[aIndex + 1].FileName + mListLinkArg + "\">　"
								+ aChildren[aIndex + 1].Name + "　&gt;&gt;　</a></td>");
					}
					aSB.Append("</tr></table>\n");
					oPageInfoTree.Content = oPageInfoTree.Content.Replace(HTML_VAR_NEIGHBOR, aSB.ToString());
				}
			}

			// 子ページを調整
			for (Int32 i = 0; i < oPageInfoTree.Children.Count; i++)
			{
				AdjustListMisc(oPageInfoTree.Children[i]);
			}
		}

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
			AppendSongInfoAddTd(oSB, oChapterItem, oTFound);
			oSB.Append("\n  </tr>\n");
		}

		// --------------------------------------------------------------------
		// 章を開始する
		// --------------------------------------------------------------------
		private void BeginChapter(StringBuilder oSB, OutputItems oChapterItem, Int32 oChapterIndex, Int32 oNumChapters, List<TFound> oTFounds)
		{
			// 章名挿入
			oSB.Append("<input type=\"checkbox\" id=\"label" + oChapterIndex + "\" class=\"accparent\"");

			// 章数が 1、かつ、タイアップ名 == 頭文字、の場合（ボカロ等）は、リストが最初から開いた状態にする
			if (oNumChapters == 1 && oTFounds[0].TieUpName == oTFounds[0].Head)
			{
				oSB.Append(" checked=\"checked\"");
			}
			oSB.Append(">\n");
			oSB.Append("<label for=\"label" + oChapterIndex + "\">" + ChapterValue(oChapterItem, oTFounds[0]) + "　（"
					+ oTFounds.Count.ToString("#,0") + " 曲）");
			if (oChapterItem == OutputItems.TieUpName && !String.IsNullOrEmpty(oTFounds[0].TieUpGroupName))
			{
				// 章の区切りがタイアップ名の場合、シリーズがあるなら記載する
				oSB.Append("　<a class=\"series\" href=\"");
				oSB.Append(OutputFileName(oTFounds[0].TieUpAgeLimit >= YlConstants.AGE_LIMIT_CERO_Z, KIND_FILE_NAME_TIE_UP_GROUP,
						TieUpGroupHead(oTFounds[0]), oTFounds[0].TieUpGroupName + YlConstants.TIE_UP_GROUP_SUFFIX) + mListLinkArg);
				oSB.Append("\">" + oTFounds[0].TieUpGroupName + YlConstants.TIE_UP_GROUP_SUFFIX + "</a>");
			}
			oSB.Append("</label>\n");
			oSB.Append("<div class=\"accchild\">\n");

			// テーブルを開く
			oSB.Append("<table>\n");
			oSB.Append("  <tr>\n    ");
			BeginChapterAddTh(oSB, oChapterItem);
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
		// 古いリストを削除（インデックス以外）
		// --------------------------------------------------------------------
		private void DeleteOldListContents()
		{
			Debug.Assert(!String.IsNullOrEmpty(mListExt), "DeleteOldList() mListExt が初期化されていない");

			DeleteOldListContentsCore(KIND_FILE_NAME_NEW);
			DeleteOldListContentsCore(KIND_FILE_NAME_CATEGORY);
			DeleteOldListContentsCore(KIND_FILE_NAME_TIE_UP_GROUP);
			DeleteOldListContentsCore(KIND_FILE_NAME_PERIOD);
			DeleteOldListContentsCore(KIND_FILE_NAME_SEASON);
			DeleteOldListContentsCore(KIND_FILE_NAME_ARTIST);
			DeleteOldListContentsCore(KIND_FILE_NAME_COMPOSER);
			DeleteOldListContentsCore(KIND_FILE_NAME_TAG);
			DeleteMisc();
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
					mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "古いリストファイル " + Path.GetFileName(aPath) + " を削除できませんでした。", true);
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
		// グループ＝歌手別、ページ＝頭文字、章＝歌手名、でページ内容生成
		// --------------------------------------------------------------------
		private void GenerateArtistAndHeads()
		{
			ZonePage(false).AddChild(GenerateArtistAndHeadsCore(false));
			ZonePage(true).AddChild(GenerateArtistAndHeadsCore(true));
		}

		// --------------------------------------------------------------------
		// グループ＝歌手別、ページ＝頭文字、章＝歌手名、でページ内容生成
		// --------------------------------------------------------------------
		private PageInfoTree GenerateArtistAndHeadsCore(Boolean oIsAdult)
		{
			PageInfoTree aPageInfoTree = new PageInfoTree();
			aPageInfoTree.Name = "歌手別";
			aPageInfoTree.FileName = IndexFileName(oIsAdult, KIND_FILE_NAME_ARTIST);

			// タイアップ名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			var aQueryResult =
					from Found in TableFound
					join x in TableArtistSequence on Found.SongId equals x.Id into gj
					from ArtistSequence in gj.DefaultIfEmpty()
					join y in TablePerson on ArtistSequence.LinkId equals y.Id into gj2
					from Person in gj2.DefaultIfEmpty()
					where Found.TieUpName != null && Found.SongId != null && Person != null && (oIsAdult ? Found.TieUpAgeLimit >= YlConstants.AGE_LIMIT_CERO_Z : Found.TieUpAgeLimit < YlConstants.AGE_LIMIT_CERO_Z)
					orderby Person.Ruby, Person.Name, Found.Head, Found.TieUpRuby, Found.TieUpName, Found.SongRuby, Found.SongName
					select new { Found, ArtistSequence, Person };
			var aPrevRecord = new { Found = new TFound(), ArtistSequence = new TArtistSequence(), Person = new TPerson() };
			aPrevRecord = null;
			String aPrevPersonHead = null;

			foreach (var aRecord in aQueryResult)
			{
				String aPersonHead = PersonHead(aRecord.Person);

				if (aPrevRecord != null
						&& (aPersonHead != aPrevPersonHead || aRecord.Person.Ruby != aPrevRecord.Person.Ruby || aRecord.Person.Name != aPrevRecord.Person.Name))
				{
					// 頭文字またはページが新しくなったので 1 ページ分出力
					GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
							KIND_FILE_NAME_ARTIST, aPrevPersonHead, aPrevRecord.Person.Name, OutputItems.TieUpName);
					aPrevRecord = null;
				}

				if (aPrevRecord == null
						|| aPrevRecord != null && aRecord.Found.TieUpName != aPrevRecord.Found.TieUpName)
				{
					// 番組名が新しくなった
					aTieUpNamesAndTFounds[aRecord.Found.TieUpName] = new List<TFound>();
				}

				// 曲情報追加
				aTieUpNamesAndTFounds[aRecord.Found.TieUpName].Add(aRecord.Found);

				// ループ処理
				aPrevRecord = aRecord;
				aPrevPersonHead = aPersonHead;
			}

			if (aPrevRecord != null)
			{
				GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
						KIND_FILE_NAME_ARTIST, aPrevPersonHead, aPrevRecord.Person.Name, OutputItems.TieUpName);
			}

			// インデックス
			GenerateFreestyleIndexPageContent(aPageInfoTree, oIsAdult, KIND_FILE_NAME_ARTIST, "五十音");

			return aPageInfoTree;
		}

		// --------------------------------------------------------------------
		// グループ＝カテゴリー、ページ＝頭文字、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private void GenerateCategoryAndHeads()
		{
			ZonePage(false).AddChild(GenerateCategoryAndHeadsCore(false));
			ZonePage(true).AddChild(GenerateCategoryAndHeadsCore(true));
		}

		// --------------------------------------------------------------------
		// グループ＝カテゴリー、ページ＝頭文字、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private PageInfoTree GenerateCategoryAndHeadsCore(Boolean oIsAdult)
		{
			PageInfoTree aPageInfoTree = new PageInfoTree();
			aPageInfoTree.Name = "カテゴリー別";
			aPageInfoTree.FileName = IndexFileName(oIsAdult, KIND_FILE_NAME_CATEGORY);

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where x.TieUpName != null && (oIsAdult ? x.TieUpAgeLimit >= YlConstants.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlConstants.AGE_LIMIT_CERO_Z)
					orderby x.Category, x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;

			GenerateCategoryAndHeadsCore(aPageInfoTree, aQueryResult, oIsAdult, KIND_FILE_NAME_CATEGORY);

			return aPageInfoTree;
		}

		// --------------------------------------------------------------------
		// グループ＝カテゴリー、ページ＝頭文字、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private void GenerateCategoryAndHeadsCore(PageInfoTree oPageInfoTree, IQueryable<TFound> oQueryResult, Boolean oIsAdult, String oKindFileName)
		{
			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			TFound aPrevTFound = null;

			foreach (TFound aTFound in oQueryResult)
			{
				if (aPrevTFound != null
						&& (aTFound.Category != aPrevTFound.Category || aTFound.Head != aPrevTFound.Head))
				{
					// カテゴリーまたはページが新しくなったので 1 ページ分出力
					GenerateOneList(oPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
							oKindFileName, aPrevTFound.Category, aPrevTFound.Head, OutputItems.TieUpName);
				}

				if (!String.IsNullOrEmpty(aTFound.TieUpName))
				{
					if (aTieUpNamesAndTFounds.Count == 0 || aPrevTFound != null && aTFound.TieUpName != aPrevTFound.TieUpName)
					{
						// 番組名が新しくなった
						aTieUpNamesAndTFounds[aTFound.TieUpName] = new List<TFound>();
					}

					// 曲情報追加
					aTieUpNamesAndTFounds[aTFound.TieUpName].Add(aTFound);
				}

				// ループ処理
				aPrevTFound = aTFound;
			}

			if (aPrevTFound != null)
			{
				GenerateOneList(oPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
						oKindFileName, aPrevTFound.Category, aPrevTFound.Head, OutputItems.TieUpName);
			}

			// インデックス
			GenerateIndexPageContent(oPageInfoTree, oIsAdult, oKindFileName, "カテゴリー名");
		}

		// --------------------------------------------------------------------
		// グループ＝作曲者別、ページ＝頭文字、章＝作曲者名、でページ内容生成
		// --------------------------------------------------------------------
		private void GenerateComposerAndHeads()
		{
			ZonePage(false).AddChild(GenerateComposerAndHeadsCore(false));
			ZonePage(true).AddChild(GenerateComposerAndHeadsCore(true));
		}

		// --------------------------------------------------------------------
		// グループ＝作曲者別、ページ＝頭文字、章＝作曲者名、でファイル出力
		// --------------------------------------------------------------------
		private PageInfoTree GenerateComposerAndHeadsCore(Boolean oIsAdult)
		{
			PageInfoTree aPageInfoTree = new PageInfoTree();
			aPageInfoTree.Name = "作曲者別";
			aPageInfoTree.FileName = IndexFileName(oIsAdult, KIND_FILE_NAME_COMPOSER);

			// タイアップ名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			var aQueryResult =
					from Found in TableFound
					join x in TableComposerSequence on Found.SongId equals x.Id into gj
					from ComposerSequence in gj.DefaultIfEmpty()
					join y in TablePerson on ComposerSequence.LinkId equals y.Id into gj2
					from Person in gj2.DefaultIfEmpty()
					where Found.TieUpName != null && Found.SongId != null && Person != null && (oIsAdult ? Found.TieUpAgeLimit >= YlConstants.AGE_LIMIT_CERO_Z : Found.TieUpAgeLimit < YlConstants.AGE_LIMIT_CERO_Z)
					orderby Person.Ruby, Person.Name, Found.Head, Found.TieUpRuby, Found.TieUpName, Found.SongRuby, Found.SongName
					select new { Found, ComposerSequence, Person };
			var aPrevRecord = new { Found = new TFound(), ComposerSequence = new TComposerSequence(), Person = new TPerson() };
			aPrevRecord = null;
			String aPrevPersonHead = null;

			foreach (var aRecord in aQueryResult)
			{
				String aPersonHead = PersonHead(aRecord.Person);

				if (aPrevRecord != null
						&& (aPersonHead != aPrevPersonHead || aRecord.Person.Ruby != aPrevRecord.Person.Ruby || aRecord.Person.Name != aPrevRecord.Person.Name))
				{
					// 頭文字またはページが新しくなったので 1 ページ分出力
					GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
							KIND_FILE_NAME_COMPOSER, aPrevPersonHead, aPrevRecord.Person.Name, OutputItems.TieUpName);
					aPrevRecord = null;
				}

				if (aPrevRecord == null
						|| aPrevRecord != null && aRecord.Found.TieUpName != aPrevRecord.Found.TieUpName)
				{
					// 番組名が新しくなった
					aTieUpNamesAndTFounds[aRecord.Found.TieUpName] = new List<TFound>();
				}

				// 曲情報追加
				aTieUpNamesAndTFounds[aRecord.Found.TieUpName].Add(aRecord.Found);

				// ループ処理
				aPrevRecord = aRecord;
				aPrevPersonHead = aPersonHead;
			}

			if (aPrevRecord != null)
			{
				GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
						KIND_FILE_NAME_COMPOSER, aPrevPersonHead, aPrevRecord.Person.Name, OutputItems.TieUpName);
			}

			// インデックス
			GenerateFreestyleIndexPageContent(aPageInfoTree, oIsAdult, KIND_FILE_NAME_COMPOSER, "五十音");

			return aPageInfoTree;
		}

		// --------------------------------------------------------------------
		// インデックスページ（ページは任意の文字列ごと）の内容を生成
		// --------------------------------------------------------------------
		private void GenerateFreestyleIndexPageContent(PageInfoTree oIndexPage, Boolean oIsAdult, String oKindFileName, String oChapterName)
		{
			Int32 aGroupIndex = 0;
			StringBuilder aSB = new StringBuilder();
			PageInfoTree aMiscGroup = null;

			// その他以外
			for (Int32 i = 0; i < oIndexPage.Children.Count; i++)
			{
				if (oIndexPage.Children[i].Name == YlConstants.GROUP_MISC)
				{
					aMiscGroup = oIndexPage.Children[i];
					continue;
				}
				GenerateFreestyleIndexPageContentOneGroup(aSB, oIndexPage.Children[i], aGroupIndex, oIsAdult, oKindFileName);
				aGroupIndex++;
			}

			// その他
			if (aMiscGroup != null)
			{
				GenerateFreestyleIndexPageContentOneGroup(aSB, aMiscGroup, aGroupIndex, oIsAdult, oKindFileName);
			}

			// インデックスページ
			String aTopTemplate = LoadTemplate("HtmlIndex");
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_CHAPTER_NAME, oChapterName);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_INDICES, aSB.ToString());
			oIndexPage.Content = aTopTemplate;
		}

		// --------------------------------------------------------------------
		// インデックスページ（ページは任意の文字列ごと）の 1 グループ分の内容を生成
		// --------------------------------------------------------------------
		private void GenerateFreestyleIndexPageContentOneGroup(StringBuilder oSB, PageInfoTree oGroup, Int32 oGroupIndex,
				Boolean oIsAdult, String oKindFileName)
		{
			StringBuilder aSbPages = new StringBuilder();
			Int32 aNumSongs = 0;
			PageInfoTree aMiscGroup = null;

			String aOneTemplate = LoadTemplate("HtmlFreestyleIndexOneGroup");
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_CATEGORY, oGroup.Name);

			// その他以外
			foreach (PageInfoTree aPageInfo in oGroup.Children)
			{
				if (aPageInfo.Name == YlConstants.GROUP_MISC)
				{
					aMiscGroup = aPageInfo;
					continue;
				}

				aSbPages.Append("<tr><td class=\"exist\"><a href=\"" + OutputFileName(oIsAdult, oKindFileName, oGroup.Name, aPageInfo.Name) + mListLinkArg + "\">"
						+ aPageInfo.Name + " （" + aPageInfo.NumSongs.ToString("#,0") + " 曲）</a></td></tr>");
				aNumSongs += aPageInfo.NumSongs;
			}

			// その他
			if (aMiscGroup != null)
			{
				PageInfoTree aPageInfo = aMiscGroup;
				aSbPages.Append("<tr><td class=\"exist\"><a href=\"" + OutputFileName(oIsAdult, oKindFileName, oGroup.Name, aPageInfo.Name) + mListLinkArg + "\">"
						+ aPageInfo.Name + " （" + aPageInfo.NumSongs.ToString("#,0") + " 曲）</a></td></tr>");
				aNumSongs += aPageInfo.NumSongs;
			}

			aOneTemplate = aOneTemplate.Replace(HTML_VAR_PAGES, aSbPages.ToString());
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_CATEGORY_INDEX, oGroupIndex.ToString());
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_NUM_SONGS, "（" + aNumSongs.ToString("#,0") + " 曲）");
			oSB.Append(aOneTemplate);
		}

		// --------------------------------------------------------------------
		// インデックスページ（ページは頭文字ごと）の内容を生成
		// --------------------------------------------------------------------
		private void GenerateIndexPageContent(PageInfoTree oIndexPage, Boolean oIsAdult, String oKindFileName, String oChapterName)
		{
			Int32 aGroupIndex = 0;
			StringBuilder aSB = new StringBuilder();
			PageInfoTree aMiscGroup = null;

			// その他以外
			for (Int32 i = 0; i < oIndexPage.Children.Count; i++)
			{
				if (oIndexPage.Children[i].Name == YlConstants.GROUP_MISC)
				{
					aMiscGroup = oIndexPage.Children[i];
					continue;
				}
				GenerateIndexPageContentOneGroup(aSB, oIndexPage.Children[i], aGroupIndex, oIsAdult, oKindFileName);
				aGroupIndex++;
			}

			// その他
			if (aMiscGroup != null)
			{
				GenerateIndexPageContentOneGroup(aSB, aMiscGroup, aGroupIndex, oIsAdult, oKindFileName);
			}

			// インデックスページ
			String aTopTemplate = LoadTemplate("HtmlIndex");
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_CHAPTER_NAME, oChapterName);
			aTopTemplate = aTopTemplate.Replace(HTML_VAR_INDICES, aSB.ToString());
			oIndexPage.Content = aTopTemplate;
		}

		// --------------------------------------------------------------------
		// インデックスページ（ページは頭文字ごと）の 1 グループ分の内容を生成
		// --------------------------------------------------------------------
		private void GenerateIndexPageContentOneGroup(StringBuilder oSB, PageInfoTree oGroup, Int32 oGroupIndex,
				Boolean oIsAdult, String oKindFileName)
		{
			Boolean aHasKana = false;
			Boolean aHasMisc = false;
			Int32 aNumSongs = 0;

			String aOneTemplate = LoadTemplate("HtmlIndexOneGroup");
			aOneTemplate = aOneTemplate.Replace(HTML_VAR_CATEGORY, oGroup.Name);

			foreach (PageInfoTree aPageInfo in oGroup.Children)
			{
				aOneTemplate = aOneTemplate.Replace("<td>" + aPageInfo.Name + "</td>", "<td class=\"exist\"><a href=\""
						+ OutputFileName(oIsAdult, oKindFileName, oGroup.Name, aPageInfo.Name) + mListLinkArg + "\">" + aPageInfo.Name + "</a></td>");
				aNumSongs += aPageInfo.NumSongs;

				if (aPageInfo.Name == YlConstants.HEAD_MISC)
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
		}

		// --------------------------------------------------------------------
		// グループ＝新着、ページ＝カテゴリー、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private void GenerateNew()
		{
			if (!((WebOutputSettings)OutputSettings).EnableNew)
			{
				return;
			}

			ZonePage(false).AddChild(GenerateNewCore(false));
			ZonePage(true).AddChild(GenerateNewCore(true));
		}

		// --------------------------------------------------------------------
		// グループ＝新着、ページ＝カテゴリー、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private PageInfoTree GenerateNewCore(Boolean oIsAdult)
		{
			PageInfoTree aPageInfoTree = new PageInfoTree();
			aPageInfoTree.Name = "新着";
			aPageInfoTree.FileName = IndexFileName(oIsAdult, KIND_FILE_NAME_NEW);

			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			// 新着とする日付
			Double aNewDate = JulianDay.DateTimeToModifiedJulianDate(DateTime.Now.AddDays(-((WebOutputSettings)OutputSettings).NewDays));

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where x.TieUpName != null && x.LastWriteTime >= aNewDate
					&& (oIsAdult ? x.TieUpAgeLimit >= YlConstants.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlConstants.AGE_LIMIT_CERO_Z)
					orderby x.Category, x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;
			String aPrevCategory = null;

			foreach (TFound aTFound in aQueryResult)
			{
				if (aPrevTFound != null
						&& (aTFound.Category != aPrevCategory))
				{
					// カテゴリーが新しくなったので 1 ページ分出力
					GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult, KIND_FILE_NAME_NEW, "新着",
							String.IsNullOrEmpty(aPrevCategory) ? YlConstants.GROUP_MISC : aPrevCategory, OutputItems.TieUpName);
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
				aPrevCategory = aTFound.Category;
			}

			if (aPrevTFound != null)
			{
				GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult, KIND_FILE_NAME_NEW, "新着",
						String.IsNullOrEmpty(aPrevCategory) ? YlConstants.GROUP_MISC : aPrevCategory, OutputItems.TieUpName);
			}

			// インデックス
			GenerateFreestyleIndexPageContent(aPageInfoTree, oIsAdult, KIND_FILE_NAME_NEW, "新着");

			return aPageInfoTree;
		}

		// --------------------------------------------------------------------
		// リストの 1 ページ分を生成
		// ＜引数＞ oChaptersAndTFounds: 章（橙色の区切り）ごとの楽曲群
		// --------------------------------------------------------------------
		private void GenerateOneList(PageInfoTree oParent, Dictionary<String, List<TFound>> oChaptersAndTFounds,
				Boolean oIsAdult, String oKindFileName, String oGroupName, String oPageName, OutputItems oChapterItem)
		{
			// null を調整
			if (String.IsNullOrEmpty(oGroupName))
			{
				oGroupName = YlConstants.GROUP_MISC;
			}

			PageInfoTree aPageInfoTree = new PageInfoTree();
			aPageInfoTree.Name = oPageName;
			aPageInfoTree.FileName = OutputFileName(oIsAdult, oKindFileName, oGroupName, oPageName);

			String aTemplate = LoadTemplate("HtmlList");

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
			aTemplate = aTemplate.Replace(HTML_VAR_ADDITIONAL_DESCRIPTION, mAdditionalDescription);
			aTemplate = aTemplate.Replace(HTML_VAR_CHAPTER_NAME, YlConstants.OUTPUT_ITEM_NAMES[(Int32)oChapterItem]);
			aTemplate = aTemplate.Replace(HTML_VAR_PROGRAMS, aSB.ToString());

			aPageInfoTree.Content = aTemplate;
			aPageInfoTree.NumSongs = aNumPageSongs;

			// 出力済みの内容をクリア
			oChaptersAndTFounds.Clear();

			// oParent 配下のどこにぶら下げるかを検索
			PageInfoTree aGroup = null;
			for (Int32 i = 0; i < oParent.Children.Count; i++)
			{
				if (oParent.Children[i].Name == oGroupName)
				{
					aGroup = oParent.Children[i];
					break;
				}
			}
			if (aGroup == null)
			{
				// oParent 配下に oGroupName のページを新規作成
				aGroup = new PageInfoTree();
				aGroup.Name = oGroupName;
				oParent.AddChild(aGroup);
			}

			// aGroup にぶら下げる
			aGroup.AddChild(aPageInfoTree);
		}

		// --------------------------------------------------------------------
		// グループ＝年代、ページ＝頭文字、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private void GeneratePeriodAndHeads()
		{
			ZonePage(false).AddChild(GeneratePeriodAndHeadsCore(false));
			ZonePage(true).AddChild(GeneratePeriodAndHeadsCore(true));
		}

		// --------------------------------------------------------------------
		// グループ＝年代、ページ＝頭文字、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private PageInfoTree GeneratePeriodAndHeadsCore(Boolean oIsAdult)
		{
			PageInfoTree aPageInfoTree = new PageInfoTree();
			aPageInfoTree.Name = "年代別";
			aPageInfoTree.FileName = IndexFileName(oIsAdult, KIND_FILE_NAME_PERIOD);

			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			// 年月日設定
			Int32 aSinceYear = DateTime.UtcNow.Year;
			aSinceYear -= aSinceYear % 10;

			while (aSinceYear > YlConstants.INVALID_YEAR)
			{
				Int32 aUntilYear = aSinceYear + 10;

				IQueryable<TFound> aQueryResult =
						from x in TableFound
						where x.TieUpName != null
						&& JulianDay.DateTimeToModifiedJulianDate(new DateTime(aSinceYear, 1, 1)) <= x.SongReleaseDate
						&& x.SongReleaseDate < JulianDay.DateTimeToModifiedJulianDate(new DateTime(aUntilYear, 1, 1))
						&& (oIsAdult ? x.TieUpAgeLimit >= YlConstants.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlConstants.AGE_LIMIT_CERO_Z)
						orderby x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
						select x;
				TFound aPrevTFound = null;

				foreach (TFound aTFound in aQueryResult)
				{
					if (aPrevTFound != null
							&& aTFound.Head != aPrevTFound.Head)
					{
						// 頭文字が新しくなったので 1 ページ分出力
						GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
								KIND_FILE_NAME_PERIOD, aSinceYear.ToString() + " 年代", aPrevTFound.Head, OutputItems.TieUpName);
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
					GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
							KIND_FILE_NAME_PERIOD, aSinceYear.ToString() + " 年代", aPrevTFound.Head, OutputItems.TieUpName);
				}

				aSinceYear -= 10;
			}

			// インデックス
			GenerateIndexPageContent(aPageInfoTree, oIsAdult, KIND_FILE_NAME_PERIOD, "年代");

			return aPageInfoTree;
		}

		// --------------------------------------------------------------------
		// グループ＝タグ名の頭文字、ページ＝タグ名、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private void GenerateTagHeadAndTags()
		{
			ZonePage(false).AddChild(GenerateTagHeadAndTagsCore(false));
			ZonePage(true).AddChild(GenerateTagHeadAndTagsCore(true));
		}

		// --------------------------------------------------------------------
		// グループ＝タグ名の頭文字、ページ＝タグ名、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private PageInfoTree GenerateTagHeadAndTagsCore(Boolean oIsAdult)
		{
			PageInfoTree aPageInfoTree = new PageInfoTree();
			aPageInfoTree.Name = "タグ別";
			aPageInfoTree.FileName = IndexFileName(oIsAdult, KIND_FILE_NAME_TAG);

			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			var aQueryResult =
					from Found in TableFound
					join x in TableTagSequence on Found.SongId equals x.Id into gj
					from TagSequence in gj.DefaultIfEmpty()
					join y in TableTag on TagSequence.LinkId equals y.Id into gj2
					from Tag in gj2.DefaultIfEmpty()
					where Found.TieUpName != null && Found.SongId != null && Tag != null && (oIsAdult ? Found.TieUpAgeLimit >= YlConstants.AGE_LIMIT_CERO_Z : Found.TieUpAgeLimit < YlConstants.AGE_LIMIT_CERO_Z)
					orderby Tag.Ruby, Tag.Name, Found.Head, Found.TieUpRuby, Found.TieUpName, Found.SongRuby, Found.SongName
					select new { Found, TagSequence, Tag };
			var aPrevRecord = new { Found = new TFound(), TagSequence = new TTagSequence(), Tag = new TTag() };
			aPrevRecord = null;
			String aPrevTagHead = null;

			foreach (var aRecord in aQueryResult)
			{
				String aTagHead = TagHead(aRecord.Tag);

				if (aPrevRecord != null
						&& (aTagHead != aPrevTagHead || aRecord.Tag.Ruby != aPrevRecord.Tag.Ruby || aRecord.Tag.Name != aPrevRecord.Tag.Name))
				{
					// 頭文字またはページが新しくなったので 1 ページ分出力
					GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
							KIND_FILE_NAME_TAG, aPrevTagHead, aPrevRecord.Tag.Name, OutputItems.TieUpName);
					aPrevRecord = null;
				}

				if (aPrevRecord == null
						|| aPrevRecord != null && aRecord.Found.TieUpName != aPrevRecord.Found.TieUpName)
				{
					// 番組名が新しくなった
					aTieUpNamesAndTFounds[aRecord.Found.TieUpName] = new List<TFound>();
				}

				// 曲情報追加
				aTieUpNamesAndTFounds[aRecord.Found.TieUpName].Add(aRecord.Found);

				// ループ処理
				aPrevRecord = aRecord;
				aPrevTagHead = aTagHead;
			}

			if (aPrevRecord != null)
			{
				GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
						KIND_FILE_NAME_TAG, aPrevTagHead, aPrevRecord.Tag.Name, OutputItems.TieUpName);
			}

			// インデックス
			GenerateFreestyleIndexPageContent(aPageInfoTree, oIsAdult, KIND_FILE_NAME_TAG, "五十音");

			return aPageInfoTree;
		}

		// --------------------------------------------------------------------
		// グループ＝タイアップグループ名の頭文字、ページ＝タイアップグループ名、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private void GenerateTieUpGroupHeadAndTieUpGroups()
		{
			ZonePage(false).AddChild(GenerateTieUpGroupHeadAndTieUpGroupsCore(false));
			ZonePage(true).AddChild(GenerateTieUpGroupHeadAndTieUpGroupsCore(true));
		}

		// --------------------------------------------------------------------
		// グループ＝タイアップグループ名の頭文字、ページ＝タイアップグループ名、章＝番組名、でページ内容生成
		// --------------------------------------------------------------------
		private PageInfoTree GenerateTieUpGroupHeadAndTieUpGroupsCore(Boolean oIsAdult)
		{
			PageInfoTree aPageInfoTree = new PageInfoTree();
			aPageInfoTree.Name = "シリーズ別";
			aPageInfoTree.FileName = IndexFileName(oIsAdult, KIND_FILE_NAME_TIE_UP_GROUP);

			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where x.TieUpName != null && x.TieUpGroupName != null && (oIsAdult ? x.TieUpAgeLimit >= YlConstants.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlConstants.AGE_LIMIT_CERO_Z)
					orderby x.TieUpGroupRuby, x.TieUpGroupName, x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;
			String aPrevTieUpGroupHead = null;

			foreach (TFound aTFound in aQueryResult)
			{
				String aTieUpGroupHead = TieUpGroupHead(aTFound);

				if (aPrevTFound != null
						&& (aTieUpGroupHead != aPrevTieUpGroupHead || aTFound.TieUpGroupRuby != aPrevTFound.TieUpGroupRuby || aTFound.TieUpGroupName != aPrevTFound.TieUpGroupName))
				{
					// 頭文字またはページが新しくなったので 1 ページ分出力
					GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
							KIND_FILE_NAME_TIE_UP_GROUP, aPrevTieUpGroupHead, aPrevTFound.TieUpGroupName + YlConstants.TIE_UP_GROUP_SUFFIX, OutputItems.TieUpName);
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
				GenerateOneList(aPageInfoTree, aTieUpNamesAndTFounds, oIsAdult,
						KIND_FILE_NAME_TIE_UP_GROUP, aPrevTieUpGroupHead, aPrevTFound.TieUpGroupName + YlConstants.TIE_UP_GROUP_SUFFIX, OutputItems.TieUpName);
			}

			// インデックス
			GenerateFreestyleIndexPageContent(aPageInfoTree, oIsAdult, KIND_FILE_NAME_TIE_UP_GROUP, "五十音");

			return aPageInfoTree;
		}

		// --------------------------------------------------------------------
		// グループ＝年、ページ＝季節、章＝番組名、でページ内容生成
		// 直近 SEASON_YEARS 年分のみ
		// --------------------------------------------------------------------
		private void GenerateYearsAndSeasons()
		{
			ZonePage(false).AddChild(GenerateYearsAndSeasonsCore(false));
			ZonePage(true).AddChild(GenerateYearsAndSeasonsCore(true));
		}

		// --------------------------------------------------------------------
		// グループ＝年、ページ＝季節、章＝番組名、でページ内容生成
		// 直近 SEASON_YEARS 年分のみ
		// --------------------------------------------------------------------
		private PageInfoTree GenerateYearsAndSeasonsCore(Boolean oIsAdult)
		{
			PageInfoTree aPageInfoTree = new PageInfoTree();
			aPageInfoTree.Name = "期別";
			aPageInfoTree.FileName = IndexFileName(oIsAdult, KIND_FILE_NAME_SEASON);

			// 年月日設定
			Int32 aSinceYear = DateTime.UtcNow.Year;
			Int32 aUntilYear = aSinceYear - SEASON_YEARS;

			while (aSinceYear > aUntilYear)
			{
				GenerateYearsAndSeasonsOneSeason(aPageInfoTree, oIsAdult, aSinceYear, 1, aSinceYear, 4, "1 月～3 月：冬");
				GenerateYearsAndSeasonsOneSeason(aPageInfoTree, oIsAdult, aSinceYear, 4, aSinceYear, 7, "4 月～6 月：春");
				GenerateYearsAndSeasonsOneSeason(aPageInfoTree, oIsAdult, aSinceYear, 7, aSinceYear, 10, "7 月～9 月：夏");
				GenerateYearsAndSeasonsOneSeason(aPageInfoTree, oIsAdult, aSinceYear, 10, aSinceYear + 1, 1, "10 月～12 月：秋");
				aSinceYear--;
			}

			// インデックス
			GenerateFreestyleIndexPageContent(aPageInfoTree, oIsAdult, KIND_FILE_NAME_SEASON, "年");

			return aPageInfoTree;
		}

		// --------------------------------------------------------------------
		// 1 期分をページ内容生成
		// --------------------------------------------------------------------
		private void GenerateYearsAndSeasonsOneSeason(PageInfoTree oPageInfoTree, Boolean oIsAdult,
				Int32 oSinceYear, Int32 oSinceMonth, Int32 oUntilYear, Int32 oUntilMonth, String oSeasonName)
		{
			// 番組名とそれに紐付く楽曲群
			Dictionary<String, List<TFound>> aTieUpNamesAndTFounds = new Dictionary<String, List<TFound>>();

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					where x.TieUpName != null
					&& JulianDay.DateTimeToModifiedJulianDate(new DateTime(oSinceYear, oSinceMonth, 1)) <= x.SongReleaseDate
					&& x.SongReleaseDate < JulianDay.DateTimeToModifiedJulianDate(new DateTime(oUntilYear, oUntilMonth, 1))
					&& (oIsAdult ? x.TieUpAgeLimit >= YlConstants.AGE_LIMIT_CERO_Z : x.TieUpAgeLimit < YlConstants.AGE_LIMIT_CERO_Z)
					orderby x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;
			TFound aPrevTFound = null;

			foreach (TFound aTFound in aQueryResult)
			{
				if (aPrevTFound == null
						|| aPrevTFound != null && aTFound.TieUpName != aPrevTFound.TieUpName)
				{
					// 番組名が新しくなった
					Debug.Assert(aTFound.TieUpName != null, "GenerateYearsAndSeasonsOneSeason() tie up name is null");
					aTieUpNamesAndTFounds[aTFound.TieUpName] = new List<TFound>();
				}

				// 曲情報追加
				aTieUpNamesAndTFounds[aTFound.TieUpName].Add(aTFound);

				// ループ処理
				aPrevTFound = aTFound;
			}

			if (aPrevTFound != null)
			{
				GenerateOneList(oPageInfoTree, aTieUpNamesAndTFounds, oIsAdult, KIND_FILE_NAME_SEASON, oSinceYear.ToString() + " 年",
						oSeasonName, OutputItems.TieUpName);
			}
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
		// ナビの順番は Output() と合わせる
		// --------------------------------------------------------------------
		private void GroupNaviCore(StringBuilder oSB, Boolean oIsAdult, Boolean oIsNewExists)
		{
			oSB.Append("<tr>");
			oSB.Append("<td>　" + ZoneName(oIsAdult) + "　</td>");

			// 新着を最優先
			if (oIsNewExists)
			{
				oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_NEW) + mListLinkArg + "\">　新着　</a></td>");
			}

			// 全曲を網羅するカテゴリーと、関連するシリーズは新着に次ぐ優先
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_CATEGORY) + mListLinkArg + "\">　カテゴリー別　</a></td>");
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_TIE_UP_GROUP) + mListLinkArg + "\">　シリーズ別　</a></td>");

			// 利用頻度が高い期別と、関連する年代別
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_SEASON) + mListLinkArg + "\">　期別　</a></td>");
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_PERIOD) + mListLinkArg + "\">　年代別　</a></td>");

			// 人別はさほど優先度が高くない
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_ARTIST) + mListLinkArg + "\">　歌手別　</a></td>");
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_COMPOSER) + mListLinkArg + "\">　作曲者別　</a></td>");

			// PC ごとに異なるタグ別は優先度低
			oSB.Append("<td class=\"exist\"><a href=\"" + IndexFileName(oIsAdult, KIND_FILE_NAME_TAG) + mListLinkArg + "\">　タグ別　</a></td>");
			oSB.Append("</tr>\n");
		}

		// --------------------------------------------------------------------
		// インデックスファイル名
		// --------------------------------------------------------------------
		private String IndexFileName(Boolean oIsAdult, String oKindFileName)
		{
			if (oKindFileName == KIND_FILE_NAME_CATEGORY)
			{
				return Path.GetFileNameWithoutExtension(TopFileName) + (oIsAdult ? "_" + YlConstants.AGE_LIMIT_CERO_Z.ToString() : null) + Path.GetExtension(TopFileName);
			}
			else
			{
				return FILE_NAME_PREFIX + "_index_" + oKindFileName + (oIsAdult ? "_" + YlConstants.AGE_LIMIT_CERO_Z.ToString() : null) + mListExt;
			}
		}

		// --------------------------------------------------------------------
		// 一時フォルダーからリストを移動
		// --------------------------------------------------------------------
		private void MoveList()
		{
			MoveListContentsCore(KIND_FILE_NAME_NEW);
			MoveListIndexCore(false, KIND_FILE_NAME_NEW);
			MoveListIndexCore(true, KIND_FILE_NAME_NEW);

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

			MoveListContentsCore(KIND_FILE_NAME_TAG);
			MoveListIndexCore(false, KIND_FILE_NAME_TAG);
			MoveListIndexCore(true, KIND_FILE_NAME_TAG);
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
					mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "リストファイル " + Path.GetFileName(aPath) + " を移動できませんでした。", true);
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
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "リストファイル " + Path.GetFileName(aIndexFileName) + " を移動できませんでした。", true);
			}
		}

		// --------------------------------------------------------------------
		// CSS を出力
		// HtmlCss テンプレートは WebServer でも使用するので内容を変更せずに出力する前提
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
			return FILE_NAME_PREFIX + "_" + oKindFileName + "_" + (oIsAdult ? YlConstants.AGE_LIMIT_CERO_Z.ToString() + "_" : null)
					+ StringToHex(oGroupName) + (String.IsNullOrEmpty(oPageName) ? null : "_" + StringToHex(oPageName)) + mListExt;
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
		// リストを一時フォルダーに出力
		// --------------------------------------------------------------------
		private void OutputList(PageInfoTree oPageInfoTree)
		{
			// oPageInfoTree の内容を出力
			if (!String.IsNullOrEmpty(oPageInfoTree.FileName))
			{
				try
				{
					File.WriteAllText(mTempFolderPath + oPageInfoTree.FileName, oPageInfoTree.Content, Encoding.UTF8);
				}
				catch (Exception)
				{
					// ファイル名が長すぎる場合はエラーとなる
				}
			}

			// 子ページの内容を出力
			for (Int32 i = 0; i < oPageInfoTree.Children.Count; i++)
			{
				OutputList(oPageInfoTree.Children[i]);
			}
		}

		// --------------------------------------------------------------------
		// 更新中を表示する出力
		// --------------------------------------------------------------------
		private void OutputNoticeIndexes()
		{
			// 内容
			String aTopTemplate = LoadTemplate("HtmlIndexNotice");

			// 新着（実際には新着が無い場合でも、更新中リストとしては 404 にならないように新着も出力しておく）
			File.WriteAllText(FolderPath + IndexFileName(false, KIND_FILE_NAME_NEW), aTopTemplate, Encoding.UTF8);
			File.WriteAllText(FolderPath + IndexFileName(true, KIND_FILE_NAME_NEW), aTopTemplate, Encoding.UTF8);

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
		// 人物の頭文字
		// --------------------------------------------------------------------
		private String PersonHead(TPerson oPerson)
		{
			// 人物データベースにルビが無い場合に名前から頭文字を取るようにすると、「その他」とひらがなが入り乱れてしまうため、
			// ルビが無い場合は常に「その他」を返すようにする
			return !String.IsNullOrEmpty(oPerson.Ruby) ? YlCommon.Head(oPerson.Ruby) : YlConstants.HEAD_MISC;
		}

		// --------------------------------------------------------------------
		// ページ内容を置換
		// --------------------------------------------------------------------
		private void ReplaceListContent(PageInfoTree oPageInfoTree, String oOld, String oNew)
		{
			// oPageInfoTree の内容を置換
			if (!String.IsNullOrEmpty(oPageInfoTree.Content))
			{
				oPageInfoTree.Content = oPageInfoTree.Content.Replace(oOld, oNew);
			}

			// 子ページの内容を置換
			for (Int32 i = 0; i < oPageInfoTree.Children.Count; i++)
			{
				ReplaceListContent(oPageInfoTree.Children[i], oOld, oNew);
			}
		}

		// --------------------------------------------------------------------
		// 文字を UTF-16 HEX に変換
		// --------------------------------------------------------------------
		private String StringToHex(String oString)
		{
			Byte[] aByteData = Encoding.Unicode.GetBytes(oString);
			return BitConverter.ToString(aByteData, 0, Math.Min(aByteData.Length, MAX_HEX_SOURCE_LENGTH)).Replace("-", String.Empty).ToLower();
		}

		// --------------------------------------------------------------------
		// タグの頭文字
		// --------------------------------------------------------------------
		private String TagHead(TTag oTag)
		{
			return !String.IsNullOrEmpty(oTag.Ruby) ? YlCommon.Head(oTag.Ruby) : YlCommon.Head(oTag.Name);
		}

		// --------------------------------------------------------------------
		// タイアップグループ名の頭文字
		// --------------------------------------------------------------------
		private String TieUpGroupHead(TFound oTFound)
		{
			return !String.IsNullOrEmpty(oTFound.TieUpGroupRuby) ? YlCommon.Head(oTFound.TieUpGroupRuby) : YlCommon.Head(oTFound.TieUpGroupName);
		}

		// --------------------------------------------------------------------
		// ゾーニング名称
		// --------------------------------------------------------------------
		private String ZoneName(Boolean oIsAdult)
		{
			return oIsAdult ? "アダルト " : "一般 ";
		}

		// --------------------------------------------------------------------
		// ゾーニングされたページ
		// --------------------------------------------------------------------
		private PageInfoTree ZonePage(Boolean oIsAdult)
		{
			return oIsAdult ? mTopPage.Children[1] : mTopPage.Children[0];
		}

	}
	// public abstract class WebOutputWriter ___END___

	// ====================================================================
	// リスト全体の情報をツリー状に管理
	// ====================================================================

	public class PageInfoTree
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public PageInfoTree()
		{
			Children = new List<PageInfoTree>();
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ページ名
		public String Name { get; set; }

		// ページファイル名
		// null の場合、構造表現専用ページなので、ページ内容はディスクに出力されない
		public String FileName { get; set; }

		// ページ内容
		public String Content { get; set; }

		// ページ単体の曲数
		public Int32 NumSongs { get; set; }

		// ページ単体に加え、子ページとその配下を含む曲数
		public Int32 NumTotalSongs
		{
			get
			{
				Int32 aSongs = NumSongs;
				for (Int32 i = 0; i < Children.Count; i++)
				{
					aSongs += Children[i].NumTotalSongs;
				}
				return aSongs;
			}
		}

		// 子ページ
		// 要素の追加は必ず AddChild() で行うこと
		public List<PageInfoTree> Children { get; set; }

		// 親ページ
		public PageInfoTree Parent { get; private set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 子ページの追加
		// --------------------------------------------------------------------
		public void AddChild(PageInfoTree oPage)
		{
			oPage.Parent = this;
			Children.Add(oPage);
		}

		// --------------------------------------------------------------------
		// 階層テキストのリンク
		// --------------------------------------------------------------------
		public String DirectoryLink(String oListLinkArg)
		{
			if (String.IsNullOrEmpty(mDirectoryLinkCache))
			{
				if (Parent != null)
				{
					mDirectoryLinkCache = Parent.DirectoryLink(oListLinkArg) + " &gt; ";
				}
				if (String.IsNullOrEmpty(FileName))
				{
					// 非リンク
					mDirectoryLinkCache += Name;
				}
				else
				{
					// リンク
					mDirectoryLinkCache += "<a href=\"" + FileName + oListLinkArg + "\">" + Name + "</a>";
				}
			}
			return mDirectoryLinkCache;
		}

		// --------------------------------------------------------------------
		// 階層テキスト
		// --------------------------------------------------------------------
		public String DirectoryText()
		{
			if (String.IsNullOrEmpty(mDirectoryTextCache))
			{
				if (Parent != null)
				{
					mDirectoryTextCache = Parent.DirectoryText() + " &gt; ";
				}
				mDirectoryTextCache += Name;
			}
			return mDirectoryTextCache;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// DirectoryText キャッシュ用
		private String mDirectoryTextCache;

		// DirectoryLink キャッシュ用
		private String mDirectoryLinkCache;
	}
	// public class PageInfoTree ___END___

}
// namespace YukaLister.Models.OutputWriters ___END___
