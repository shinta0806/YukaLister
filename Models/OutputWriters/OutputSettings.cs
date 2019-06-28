// ============================================================================
// 
// リスト出力設定用基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 基底部分の設定内容を派生クラス間で共有できるようにするために、
// ・派生クラスで設定を保存する際は、基底部分を別ファイルとして保存する
// ・派生クラスで設定を読み込む際は、別ファイルの基底部分を追加で読み込む
// 本来は ApplicationSettingsBase の派生として実装したいが、Common.ShallowCopy()
//   が使えず上記を実現できないため、通常クラスとして実装する
// ----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Generic;

using YukaLister.Models.SharedMisc;

namespace YukaLister.Models.OutputWriters
{
	public class OutputSettings
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public OutputSettings()
		{
			// 初期化（リストはデシリアライズ時に重複するため初期化しない）
			OutputAllItems = false;
		}

		// ====================================================================
		// public プロパティ
		// ====================================================================

		// 全ての項目を出力する
		public Boolean OutputAllItems { get; set; }

		// 出力項目の選択
		public List<OutputItems> SelectedOutputItems { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 読み込み
		// --------------------------------------------------------------------
		public virtual void Load()
		{
			try
			{
				OutputSettings aTmp = Common.Deserialize<OutputSettings>(YlCommon.SettingsPath() + FILE_NAME_OUTPUT_SETTINGS_CONFIG);
				Common.ShallowCopy(aTmp, this);
			}
			catch (Exception)
			{
			}

			InitIfNeeded();
		}

		// --------------------------------------------------------------------
		// OutputAllItems / SelectedOutputItems を考慮した出力アイテム
		// --------------------------------------------------------------------
		public List<OutputItems> RuntimeOutputItems()
		{
			List<OutputItems> aRuntimeOutputItems;

			if (OutputAllItems)
			{
				aRuntimeOutputItems = new List<OutputItems>();
				OutputItems[] aOutputItems = (OutputItems[])Enum.GetValues(typeof(OutputItems));
				for (Int32 i = 0; i < aOutputItems.Length - 1; i++)
				{
					aRuntimeOutputItems.Add(aOutputItems[i]);
				}
			}
			else
			{
				aRuntimeOutputItems = new List<OutputItems>(SelectedOutputItems);
			}

			return aRuntimeOutputItems;
		}


		// --------------------------------------------------------------------
		// 保存
		// --------------------------------------------------------------------
		public virtual void Save()
		{
			try
			{
				OutputSettings aTmp = new OutputSettings();
				Common.ShallowCopy(this, aTmp);
				Common.Serialize(YlCommon.SettingsPath() + FILE_NAME_OUTPUT_SETTINGS_CONFIG, aTmp);
			}
			catch (Exception)
			{
			}
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// 保存用ファイル名
		private const String FILE_NAME_OUTPUT_SETTINGS_CONFIG = "OutputSettings" + Common.FILE_EXT_CONFIG;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 必要に応じて初期化
		// --------------------------------------------------------------------
		private void InitIfNeeded()
		{
			if (SelectedOutputItems == null)
			{
				SelectedOutputItems = new List<OutputItems>();
			}
			if (SelectedOutputItems.Count == 0)
			{
				SelectedOutputItems.Add(OutputItems.TieUpName);
				SelectedOutputItems.Add(OutputItems.SongOpEd);
				SelectedOutputItems.Add(OutputItems.SongName);
				SelectedOutputItems.Add(OutputItems.ArtistName);
				SelectedOutputItems.Add(OutputItems.SmartTrack);
				SelectedOutputItems.Add(OutputItems.Worker);
				SelectedOutputItems.Add(OutputItems.Comment);
				SelectedOutputItems.Add(OutputItems.FileName);
				SelectedOutputItems.Add(OutputItems.FileSize);
			}
		}

	}
	// public class OutputSettings ___END___

}
// namespace YukaLister.Models.OutputWriters ___END___
