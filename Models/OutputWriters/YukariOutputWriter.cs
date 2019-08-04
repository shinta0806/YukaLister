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
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
		// その他のファイルの削除
		// --------------------------------------------------------------------
		protected override void DeleteMisc()
		{
			String[] aReportPathes = Directory.GetFiles(FolderPath, "Report_*" + Common.FILE_EXT_PHP);

			foreach (String aPath in aReportPathes)
			{
				try
				{
					File.Delete(aPath);
				}
				catch (Exception)
				{
					mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "古い報告ファイル " + Path.GetFileName(aPath) + " を削除できませんでした。", true);
				}
			}
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

		// --------------------------------------------------------------------
		// その他のファイルの出力
		// --------------------------------------------------------------------
		protected override void OutputMisc()
		{
			OutputReportCommon();
			OutputReportEntry();
			OutputReportRegist();
			CopySyncServerPhp();
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// 同期フォルダー
		private const String FOLDER_NAME_SYNC_SERVER = "SyncServer\\";

		// 同期フォルダーの共通ライブラリフォルダー
		private const String FOLDER_NAME_COMMON_LIB = "common_lib\\";

		// 報告用フォーム（共通）
		private const String FILE_NAME_REPORT_COMMON = "Report_Common" + Common.FILE_EXT_PHP;

		// 報告用フォーム（STEP 1：情報入力）
		private const String FILE_NAME_REPORT_ENTRY = "Report_Entry" + Common.FILE_EXT_PHP;

		// 報告用フォーム（STEP 2：情報登録）
		private const String FILE_NAME_REPORT_REGIST = "Report_Regist" + Common.FILE_EXT_PHP;

		// HTML テンプレートに記載されている変数
		private const String HTML_VAR_ID_PREFIX = "<!-- $IdPrefix$ -->";

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// SyncServer フォルダーから PHP をコピー
		// --------------------------------------------------------------------
		private void CopySyncServerPhp()
		{
			String aSrcFilder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + FOLDER_NAME_SYNC_SERVER + FOLDER_NAME_COMMON_LIB;
			File.Copy(aSrcFilder + "JulianDay.php", FolderPath + "Report_JulianDay.php");
		}

		// --------------------------------------------------------------------
		// リストのリンクの引数
		// oAdditionalArgs: "hoge=1&fuga=2" の形式
		// --------------------------------------------------------------------
		private String ListLinkArg(String oAdditionalArgs = null)
		{
			return "<?php empty($yukarisearchlink) ? print \"" + (String.IsNullOrEmpty(oAdditionalArgs) ? null : "?" + oAdditionalArgs)
					+ "\" : print \"?yukarihost=\".$yukarihost" + (String.IsNullOrEmpty(oAdditionalArgs) ? null : ".\"&" + oAdditionalArgs + "\"") + ";?>";
		}

		// --------------------------------------------------------------------
		// Report_Common.php 出力
		// --------------------------------------------------------------------
		private void OutputReportCommon()
		{
			if (String.IsNullOrEmpty(mEnvironment.YukaListerSettings.IdPrefix))
			{
				throw new Exception("ID 先頭付与文字列が設定されていません。");
			}

			String aTemplate = LoadTemplate("YukariReportCommon");
			aTemplate = aTemplate.Replace(HTML_VAR_ID_PREFIX, mEnvironment.YukaListerSettings.IdPrefix);
			File.WriteAllText(FolderPath + FILE_NAME_REPORT_COMMON, aTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// Report_Entry.php 出力
		// --------------------------------------------------------------------
		private void OutputReportEntry()
		{
			String aTemplate = LoadTemplate("YukariReportEntry");
			aTemplate = ReplacePhpContents(aTemplate);
			File.WriteAllText(FolderPath + FILE_NAME_REPORT_ENTRY, aTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// Report_Regist.php 出力
		// --------------------------------------------------------------------
		private void OutputReportRegist()
		{
			String aTemplate = LoadTemplate("YukariReportRegist");
			aTemplate = ReplacePhpContents(aTemplate);
			File.WriteAllText(FolderPath + FILE_NAME_REPORT_REGIST, aTemplate, Encoding.UTF8);
		}

		// --------------------------------------------------------------------
		// ページ内容を置換
		// --------------------------------------------------------------------
		private String ReplacePhpContents(String oTemplate)
		{
			oTemplate = oTemplate.Replace(HTML_VAR_ADDITIONAL_NAVI, mAdditionalNavi);
			oTemplate = oTemplate.Replace(HTML_VAR_GENERATOR, YlConstants.APP_NAME_J + "  " + YlConstants.APP_VER);
			return oTemplate;
		}


	}
	// public class YukariOutputWriter ___END___

}
// namespace YukaLister.Models.OutputWriters ___END___
