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
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace YukaLister.Shared
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
		public YukariOutputWriter()
		{
			// プロパティー
			FormatName = "ゆかり用 PHP";
			TopFileName = "index" + Common.FILE_EXT_PHP;
			OutputSettings = new YukariOutputSettings();

			// メンバー変数
			String aListLinkArg = "<?php empty($yukarisearchlink) ? print \"\" : print \"?yukarihost=\".$yukarihost;?>";

			mListExt = Common.FILE_EXT_PHP;
			mDirectoryTopName = "ゆかり検索 &gt; " + YlCommon.APP_NAME_J + "一覧";
			mDirectoryTopLink = "<a href=\"/search.php" + aListLinkArg + "\">ゆかり検索</a> &gt; <a href=\"" + TopFileName + aListLinkArg + "\">"
					+ YlCommon.APP_NAME_J + "一覧</a>";
			mAdditionalDescription = "ファイル名をクリックすると、ゆかりでリクエストできます。<br>";
			mAdditionalHeader = "<?php\n"
					+ "$yukarisearchlink = '';\n"
					+ "if (array_key_exists('yukarihost', $_REQUEST)) {\n"
					+ "    $yukarihost = $_REQUEST['yukarihost'];\n"
					+ "    $yukarisearchlink = 'http://'.$yukarihost.'/search_listerdb_songlist.php?filename=';\n"
					+ "}\n"
					+ "?>\n";
			mListLinkArg = aListLinkArg;
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

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

		// ====================================================================
		// private メンバー関数
		// ====================================================================

	}
	// public class YukariOutputWriter ___END___

}
// namespace YukaLister.Shared ___END___
