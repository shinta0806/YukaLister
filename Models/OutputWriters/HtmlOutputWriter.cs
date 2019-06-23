// ============================================================================
// 
// HTML リスト出力クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;

namespace YukaLister.Models.OutputWriters
{
	// ====================================================================
	// HTML リスト出力クラス
	// ====================================================================

	public class HtmlOutputWriter : WebOutputWriter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public HtmlOutputWriter(EnvironmentModel oEnvironment) : base(oEnvironment)
		{
			// プロパティー
			FormatName = "HTML";
			TopFileName = "index" + Common.FILE_EXT_HTML;
			OutputSettings = new HtmlOutputSettings();

			// メンバー変数
			mListExt = Common.FILE_EXT_HTML;
			mAdditionalDescription = null;
			mAdditionalHeader = null;
			mAdditionalNavi = null;
			mListLinkArg = null;
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リストに出力するファイル名の表現
		// --------------------------------------------------------------------
		protected override String FileNameDescription(String oFileName)
		{
			return oFileName;
		}

	}
	// public class HtmlOutputWriter ___END___

}
// YukaLister.Models.OutputWriters ___END___