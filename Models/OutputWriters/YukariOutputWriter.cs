// ============================================================================
// 
// ゆかり用リスト出力クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Text;
using System.Web;

using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.Models.OutputWriters
{
	// ====================================================================
	// ゆかり用リスト出力クラス
	// ====================================================================

	public class YukariOutputWriter : WebOutputWriter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public YukariOutputWriter(EnvironmentModel oEnvironment) : base(oEnvironment)
		{
			// プロパティー
			FormatName = "ゆかり用 PHP";
			TopFileName = "index" + Common.FILE_EXT_PHP;
			OutputSettings = new YukariOutputSettings();

			// メンバー変数
			String aListLinkArg = ListLinkArg();

			mListExt = Common.FILE_EXT_PHP;
			mAdditionalDescription = "ファイル名をクリックすると、ゆかりでリクエストできます。<br>";
			mAdditionalHeader = "<?php\n"
					+ "$yukarisearchlink = '';\n"
					+ "if (array_key_exists('yukarihost', $_REQUEST)) {\n"
					+ "    $yukarihost = $_REQUEST['yukarihost'];\n"
					+ "    $yukarisearchlink = 'http://'.$yukarihost.'/search_listerdb_filelist.php?anyword=';\n"
					+ "}\n"
					+ "?>\n";
			mAdditionalNavi = "<div class=\"additionalnavi\">"
					+ "<a class=\"additionalnavilink\" href=\"/search.php" + aListLinkArg + "\">検索</a> "
					+ "<a class=\"additionalnavilink\" href=\"/requestlist_only.php" + aListLinkArg + "\">予約一覧</a> "
					+ "</div>";
			mListLinkArg = aListLinkArg;
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 曲情報を文字列に追加する際のテーブル内容を追加
		// --------------------------------------------------------------------
		protected override void AppendSongInfoAddTd(StringBuilder oSB, OutputItems oChapterItem, TFound oTFound)
		{
			base.AppendSongInfoAddTd(oSB, oChapterItem, oTFound);
			oSB.Append("<td class=\"small\"><a href=\"" + FILE_NAME_REPORT_ENTRY + ListLinkArg(YlConstants.SERVER_OPTION_NAME_UID + "=" + oTFound.Uid) + "\">報告</a></td>");
		}

		// --------------------------------------------------------------------
		// 章を開始する際のテーブル見出しを追加
		// --------------------------------------------------------------------
		protected override void BeginChapterAddTh(StringBuilder oSB, OutputItems oChapterItem)
		{
			base.BeginChapterAddTh(oSB, oChapterItem);
			oSB.Append("<th>報告</th>");
		}

		// --------------------------------------------------------------------
		// リストに出力するファイル名の表現
		// ファイル名エスケープに関する備忘
		//   PHP print "" の中
		//     \ と " はファイル名として使われないので気にしなくて良い
		//     ' は "" の中であればエスケープ不要
		//     →従ってエスケープ不要
		//   HTML href "" の中
		//     \ < > はファイル名として使われない
		//     & ' 半角スペースがあっても動作する
		//     →従ってエスケープしなくても動作するようだが、UrlEncode() するほうが作法が良いのでしておく
		// --------------------------------------------------------------------
		protected override String FileNameDescription(String oFileName)
		{
			if (String.IsNullOrEmpty(oFileName))
			{
				return null;
			}

			return "<?php empty($yukarisearchlink) ? print \"" + oFileName + "\" : print \"<a href=\\\"\".$yukarisearchlink.\"" + HttpUtility.UrlEncode(oFileName)
					+ "\\\">" + oFileName + "</a>\";?>";
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// 報告用フォーム（STEP 1：情報入力）
		private const String FILE_NAME_REPORT_ENTRY = "Report_Entry" + Common.FILE_EXT_PHP;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リストのリンクの引数
		// oAdditionalArgs: "hoge=1&fuga=2" の形式
		// --------------------------------------------------------------------
		private String ListLinkArg(String oAdditionalArgs = null)
		{
			return "<?php empty($yukarisearchlink) ? print \"" + (String.IsNullOrEmpty(oAdditionalArgs) ? null : "?" + oAdditionalArgs)
					+ "\" : print \"?yukarihost=\".$yukarihost" + (String.IsNullOrEmpty(oAdditionalArgs) ? null : ".\"&" + oAdditionalArgs + "\"") + ";?>";
		}


	}
	// public class YukariOutputWriter ___END___

}
// namespace YukaLister.Models.OutputWriters ___END___
