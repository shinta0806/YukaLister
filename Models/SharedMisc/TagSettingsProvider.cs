// ============================================================================
// 
// タグ設定をアプリの設定保存用パスに保存する
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Specialized;

namespace YukaLister.Models.SharedMisc
{
	public class TagSettingsProvider : ApplicationSettingsProviderBase
	{
		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public TagSettingsProvider()
		{
			FileName = Common.UserAppDataFolderPath() + FILE_NAME_TAG_CONFIG;
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		public override void Initialize(String oName, NameValueCollection oConfig)
		{
			// 設定プロバイダ名を設定
			base.Initialize("TagSettingsProvider", oConfig);
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		private const String FILE_NAME_TAG_CONFIG = "TagSettings.config";
	}
	// public class TagSettingsProvider ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___

