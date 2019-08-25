// ============================================================================
// 
// タグ設定を管理
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace YukaLister.Models.SharedMisc
{
	// 設定の保存場所を Application.UserAppDataPath 配下にする
	[SettingsProvider(typeof(TagSettingsProvider))]
	public class TagSettings : ApplicationSettingsBase
	{
		// ====================================================================
		// public プロパティ
		// ====================================================================

		// --------------------------------------------------------------------
		// タグ
		// --------------------------------------------------------------------

		// フォルダーごとのタグ設定
		// キーは、ドライブレターを除き '\\' から始まるフォルダー名
		private const String KEY_NAME_FOLDER_TAGS = "FolderTags";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public ConcurrentDictionary<String, String> FolderTags
		{
			get
			{
				return (ConcurrentDictionary<String, String>)this[KEY_NAME_FOLDER_TAGS];
			}
			set
			{
				this[KEY_NAME_FOLDER_TAGS] = value;
			}
		}

		// 保存専用（プログラム中では使用しないこと）
		private const String KEY_NAME_FOLDER_TAGS_SAVE = "FolderTagsSave";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public List<SerializableKeyValuePair<String, String>> FolderTagsSave
		{
			get
			{
				return (List<SerializableKeyValuePair<String, String>>)this[KEY_NAME_FOLDER_TAGS_SAVE];
			}
			set
			{
				this[KEY_NAME_FOLDER_TAGS_SAVE] = value;
			}
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 保存
		// --------------------------------------------------------------------
		public override void Save()
		{
			// FolderTags を保存用にコピー
			FolderTagsSave.Clear();
			foreach (KeyValuePair<String, String> aKVP in FolderTags)
			{
				SerializableKeyValuePair<String, String> aSaveKVP = new SerializableKeyValuePair<String, String>(aKVP.Key, aKVP.Value);
				FolderTagsSave.Add(aSaveKVP);
			}

			base.Save();
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定が読み込まれた
		// --------------------------------------------------------------------
		protected override void OnSettingsLoaded(Object oSender, SettingsLoadedEventArgs oSettingsLoadedEventArgs)
		{
			base.OnSettingsLoaded(oSender, oSettingsLoadedEventArgs);

			// FolderTags を復元
			FolderTags.Clear();
			foreach (SerializableKeyValuePair<String, String> aSaveKVP in FolderTagsSave)
			{
				FolderTags[aSaveKVP.Key] = aSaveKVP.Value;
			}
		}

	}
	// public class YukaListerSettings ___END___

}
// namespace YukaLister.Shared ___END___


