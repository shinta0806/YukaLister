// ============================================================================
// 
// HTML / PHP リスト出力設定用基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;

namespace YukaLister.Shared
{
	public class WebOutputSettings : OutputSettings
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public WebOutputSettings()
		{
			// 初期化（リストはデシリアライズ時に重複するため初期化しない）
			EnableNew = true;
			NewDays = NEW_DAYS_DEFAULT;
		}

		// ====================================================================
		// public プロパティ
		// ====================================================================

		// NEW を出力する
		public Boolean EnableNew { get; set; }

		// NEW と見なす日数
		public Int32 NewDays { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 読み込み
		// --------------------------------------------------------------------
		public override void Load()
		{
			// 自クラス読み込み
			try
			{
				WebOutputSettings aTmp = Common.Deserialize<WebOutputSettings>(YlCommon.SettingsPath() + FILE_NAME_WEB_OUTPUT_SETTINGS_CONFIG);
				Common.ShallowCopy(aTmp, this);
			}
			catch (Exception)
			{
			}

			// 基底クラス読み込み
			base.Load();

			// 初期化
			InitIfNeeded();
		}

		// --------------------------------------------------------------------
		// 保存
		// --------------------------------------------------------------------
		public override void Save()
		{
			// 自クラス保存
			try
			{
				WebOutputSettings aTmp = new WebOutputSettings();
				Common.ShallowCopy(this, aTmp);
				Common.Serialize(YlCommon.SettingsPath() + FILE_NAME_WEB_OUTPUT_SETTINGS_CONFIG, aTmp);
			}
			catch (Exception)
			{
			}

			// 基底クラス保存
			base.Save();
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// 保存用ファイル名
		private const String FILE_NAME_WEB_OUTPUT_SETTINGS_CONFIG = "WebOutputSettings" + Common.FILE_EXT_CONFIG;

		// NEW と見なす日数のデフォルト
		private const Int32 NEW_DAYS_DEFAULT = 31;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 必要に応じて初期化
		// --------------------------------------------------------------------
		private void InitIfNeeded()
		{
			if (NewDays < YlCommon.NEW_DAYS_MIN)
			{
				NewDays = NEW_DAYS_DEFAULT;
			}
		}

	}
	// public class WebOutputSettings ___END___

}
// namespace YukaLister.Shared ___END___
