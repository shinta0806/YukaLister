// ============================================================================
// 
// ゆかりすたーの設定を管理
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;

namespace YukaLister.Shared
{
	// 設定の保存場所を Application.UserAppDataPath 配下にする
	[SettingsProvider(typeof(ApplicationNameSettingsProvider))]
	public class YukaListerSettings : ApplicationSettingsBase
	{
		// ====================================================================
		// public プロパティ
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定
		// --------------------------------------------------------------------

		// ゆかり設定ファイルのパス（相対または絶対）
		private const String KEY_NAME_YUKARI_CONFIG_PATH_SEED = "YukariConfigPathSeed";
		[UserScopedSetting]
		[DefaultSettingValue(@"..\" + YlCommon.FILE_NAME_YUKARI_CONFIG)]
		public String YukariConfigPathSeed
		{
			get
			{
				return (String)this[KEY_NAME_YUKARI_CONFIG_PATH_SEED];
			}
			set
			{
				this[KEY_NAME_YUKARI_CONFIG_PATH_SEED] = value;
			}
		}

		// 起動時に前回のリストをクリアする
		private const String KEY_NAME_CLEAR_PREV_LIST = "ClearPrevList";
		[UserScopedSetting]
		[DefaultSettingValue(Common.BOOLEAN_STRING_TRUE)]
		public Boolean ClearPrevList
		{
			get
			{
				return (Boolean)this[KEY_NAME_CLEAR_PREV_LIST];
			}
			set
			{
				this[KEY_NAME_CLEAR_PREV_LIST] = value;
			}
		}

		// リスト化対象ファイルの拡張子
		private const String KEY_NAME_TARGET_EXTS = "TargetExts";
		[UserScopedSetting]
		public List<String> TargetExts
		{
			get
			{
				return (List<String>)this[KEY_NAME_TARGET_EXTS];
			}
			set
			{
				this[KEY_NAME_TARGET_EXTS] = value;
			}
		}

		// ID の接頭辞
		private const String KEY_NAME_ID_PREFIX = "IdPrefix";
		[UserScopedSetting]
		public String IdPrefix
		{
			get
			{
				return (String)this[KEY_NAME_ID_PREFIX];
			}
			set
			{
				this[KEY_NAME_ID_PREFIX] = value;
			}
		}

		// ゆかりでのプレビューを可能にするか
		private const String KEY_NAME_PROVIDE_YUKARI_PREVIEW = "ProvideYukariPreview";
		[UserScopedSetting]
		[DefaultSettingValue(Common.BOOLEAN_STRING_TRUE)]
		public Boolean ProvideYukariPreview
		{
			get
			{
				return (Boolean)this[KEY_NAME_PROVIDE_YUKARI_PREVIEW];
			}
			set
			{
				this[KEY_NAME_PROVIDE_YUKARI_PREVIEW] = value;
			}
		}

		// ゆかり用のサーバーポート
		private const String KEY_NAME_WEB_SERVER_PORT = "WebServerPort";
		[UserScopedSetting]
		[DefaultSettingValue("13582")]
		public Int32 WebServerPort
		{
			get
			{
				return (Int32)this[KEY_NAME_WEB_SERVER_PORT];
			}
			set
			{
				this[KEY_NAME_WEB_SERVER_PORT] = value;
			}
		}

		// サムネイルを作成する動画の位置 [S]
		private const String KEY_NAME_THUMB_SEEK_POS = "ThumbSeekPos";
		[UserScopedSetting]
		[DefaultSettingValue("60")]
		public Int32 ThumbSeekPos
		{
			get
			{
				return (Int32)this[KEY_NAME_THUMB_SEEK_POS];
			}
			set
			{
				this[KEY_NAME_THUMB_SEEK_POS] = value;
			}
		}

		// サムネイルのデフォルトの横幅 [px]
		private const String KEY_NAME_THUMB_DEFAULT_WIDTH = "ThumbDefaultWidth";
		[UserScopedSetting]
		[DefaultSettingValue("128")]
		public Int32 ThumbDefaultWidth
		{
			get
			{
				return (Int32)this[KEY_NAME_THUMB_DEFAULT_WIDTH];
			}
			set
			{
				this[KEY_NAME_THUMB_DEFAULT_WIDTH] = value;
			}
		}


		// CSV 読み込み時の文字コード（書き込みは常に UTF-8）
		private const String KEY_NAME_CSV_ENCODING = "CsvEncoding";
		[UserScopedSetting]
		public CsvEncoding CsvEncoding
		{
			get
			{
				return (CsvEncoding)this[KEY_NAME_CSV_ENCODING];
			}
			set
			{
				this[KEY_NAME_CSV_ENCODING] = value;
			}
		}

		// ゆかり用 PHP リストを出力するかどうか
		private const String KEY_NAME_OUTPUT_YUKARI = "OutputYukari";
		[UserScopedSetting]
		[DefaultSettingValue(Common.BOOLEAN_STRING_TRUE)]
		public Boolean OutputYukari
		{
			get
			{
				return (Boolean)this[KEY_NAME_OUTPUT_YUKARI];
			}
			set
			{
				this[KEY_NAME_OUTPUT_YUKARI] = value;
			}
		}


		// --------------------------------------------------------------------
		// メンテナンス
		// --------------------------------------------------------------------

		// 新着情報を確認するかどうか
		private const String KEY_NAME_CHECK_RSS = "CheckRSS";
		[UserScopedSetting]
		[DefaultSettingValue(Common.BOOLEAN_STRING_TRUE)]
		public Boolean CheckRss
		{
			get
			{
				return (Boolean)this[KEY_NAME_CHECK_RSS];
			}
			set
			{
				this[KEY_NAME_CHECK_RSS] = value;
			}
		}

		// 楽曲情報データベースを同期するかどうか
		private const String KEY_NAME_SYNC_MUSIC_INFO_DB = "SyncMusicInfoDb";
		[UserScopedSetting]
		[DefaultSettingValue(Common.BOOLEAN_STRING_FALSE)]
		public Boolean SyncMusicInfoDb
		{
			get
			{
				return (Boolean)this[KEY_NAME_SYNC_MUSIC_INFO_DB];
			}
			set
			{
				this[KEY_NAME_SYNC_MUSIC_INFO_DB] = value;
			}
		}

		// 楽曲情報データベース同期サーバーアドレス
		private const String KEY_NAME_SYNC_SERVER = "SyncServer";
		[UserScopedSetting]
		[DefaultSettingValue("http://")]
		public String SyncServer
		{
			get
			{
				return (String)this[KEY_NAME_SYNC_SERVER];
			}
			set
			{
				this[KEY_NAME_SYNC_SERVER] = value;
			}
		}

		// 楽曲情報データベース同期サーバーアカウント名
		private const String KEY_NAME_SYNC_ACCOUNT = "SyncAccount";
		[UserScopedSetting]
		public String SyncAccount
		{
			get
			{
				return (String)this[KEY_NAME_SYNC_ACCOUNT];
			}
			set
			{
				this[KEY_NAME_SYNC_ACCOUNT] = value;
			}
		}

		// 楽曲情報データベース同期サーバーパスワード
		private const String KEY_NAME_SYNC_PASSWORD = "SyncPassword";
		[UserScopedSetting]
		public String SyncPassword
		{
			get
			{
				return (String)this[KEY_NAME_SYNC_PASSWORD];
			}
			set
			{
				this[KEY_NAME_SYNC_PASSWORD] = value;
			}
		}

		// --------------------------------------------------------------------
		// ゆかりの設定
		// --------------------------------------------------------------------

		// 簡易認証を使用するかどうか
		private const String KEY_NAME_YUKARI_USE_EASY_AUTH = "YukariUseEasyAuth";
		[UserScopedSetting]
		public Boolean YukariUseEasyAuth
		{
			get
			{
				return (Boolean)this[KEY_NAME_YUKARI_USE_EASY_AUTH];
			}
			set
			{
				this[KEY_NAME_YUKARI_USE_EASY_AUTH] = value;
			}
		}

		// 簡易認証キーワード
		private const String KEY_NAME_YUKARI_EASY_AUTH_KEYWORD = "YukariEasyAuthKeyword";
		[UserScopedSetting]
		public String YukariEasyAuthKeyword
		{
			get
			{
				return (String)this[KEY_NAME_YUKARI_EASY_AUTH_KEYWORD];
			}
			set
			{
				this[KEY_NAME_YUKARI_EASY_AUTH_KEYWORD] = value;
			}
		}

		// --------------------------------------------------------------------
		// 終了時の状態
		// --------------------------------------------------------------------

		// 前回発行した ID（次回はインクリメントした番号で発行する）
		private const String KEY_NAME_LAST_ID_NUMBERS = "LastIdNumbers";
		[UserScopedSetting]
		public List<Int32> LastIdNumbers
		{
			get
			{
				return (List<Int32>)this[KEY_NAME_LAST_ID_NUMBERS];
			}
			set
			{
				this[KEY_NAME_LAST_ID_NUMBERS] = value;
			}
		}

		// 前回楽曲情報データベースを同期ダウンロードした日（修正ユリウス日）
		private const String KEY_NAME_LAST_SYNC_DOWNLOAD_DATE = "LastSyncDownloadDate";
		[UserScopedSetting]
		public Double LastSyncDownloadDate
		{
			get
			{
				return (Double)this[KEY_NAME_LAST_SYNC_DOWNLOAD_DATE];
			}
			set
			{
				this[KEY_NAME_LAST_SYNC_DOWNLOAD_DATE] = value;
			}
		}

		// 前回起動時の世代
		private const String KEY_NAME_PREV_LAUNCH_GENERATION = "PrevLaunchGeneration";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public String PrevLaunchGeneration
		{
			get
			{
				return (String)this[KEY_NAME_PREV_LAUNCH_GENERATION];
			}
			set
			{
				this[KEY_NAME_PREV_LAUNCH_GENERATION] = value;
			}
		}

		// 前回起動時のバージョン
		private const String KEY_NAME_PREV_LAUNCH_VER = "PrevLaunchVer";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public String PrevLaunchVer
		{
			get
			{
				return (String)this[KEY_NAME_PREV_LAUNCH_VER];
			}
			set
			{
				this[KEY_NAME_PREV_LAUNCH_VER] = value;
			}
		}

		// 前回起動時のパス
		private const String KEY_NAME_PREV_LAUNCH_PATH = "PrevLaunchPath";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public String PrevLaunchPath
		{
			get
			{
				return (String)this[KEY_NAME_PREV_LAUNCH_PATH];
			}
			set
			{
				this[KEY_NAME_PREV_LAUNCH_PATH] = value;
			}
		}

		// ウィンドウ位置
		private const String KEY_NAME_RESTORE_BOUNDS = "RestoreBounds";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public Rect RestoreBounds
		{
			get
			{
				return (Rect)this[KEY_NAME_RESTORE_BOUNDS];
			}
			set
			{
				this[KEY_NAME_RESTORE_BOUNDS] = value;
			}
		}

		// ウィンドウ状態
		private const String KEY_NAME_WINDOW_STATE = "WindowState";
		[UserScopedSetting]
		public WindowState WindowState
		{
			get
			{
				return (WindowState)this[KEY_NAME_WINDOW_STATE];
			}
			set
			{
				this[KEY_NAME_WINDOW_STATE] = value;
			}
		}

		// 新着情報を確認した日付
		private const String KEY_NAME_RSS_CHECK_DATE = "RSSCheckDate";
		[UserScopedSetting]
		[DefaultSettingValue("")]
		public DateTime RssCheckDate
		{
			get
			{
				return (DateTime)this[KEY_NAME_RSS_CHECK_DATE];
			}
			set
			{
				this[KEY_NAME_RSS_CHECK_DATE] = value;
			}
		}

		// 環境設定のリスト出力先フォルダー
		private const String KEY_NAME_LIST_OUTPUT_FOLDER = "ListOutputFolder";
		[UserScopedSetting]
		public String ListOutputFolder
		{
			get
			{
				return (String)this[KEY_NAME_LIST_OUTPUT_FOLDER];
			}
			set
			{
				this[KEY_NAME_LIST_OUTPUT_FOLDER] = value;
			}
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// RSS の確認が必要かどうか
		// --------------------------------------------------------------------
		public Boolean IsCheckRssNeeded()
		{
			if (!CheckRss)
			{
				return false;
			}
			DateTime aEmptyDate = new DateTime();
			TimeSpan aDay3 = new TimeSpan(3, 0, 0, 0);
			return RssCheckDate == aEmptyDate || DateTime.Now.Date - RssCheckDate >= aDay3;
		}

		// --------------------------------------------------------------------
		// 前回使った ID 文字列
		// --------------------------------------------------------------------
		public String LastId(MusicInfoDbTables oTableIndex)
		{
			Debug.Assert(!String.IsNullOrEmpty(IdPrefix), "LastId() empty IdPrefix");
			return IdPrefix + YlCommon.MUSIC_INFO_ID_SECOND_PREFIXES[(Int32)oTableIndex] + LastIdNumbers[(Int32)oTableIndex].ToString();
		}

		// --------------------------------------------------------------------
		// LastIdNumbers をこれから使う ID 番号に設定
		// ＜返値＞ これから使う ID 文字列
		// --------------------------------------------------------------------
		public String PrepareLastId(SQLiteConnection oConnection, MusicInfoDbTables oTableIndex)
		{
			using (SQLiteCommand aCmd = new SQLiteCommand(oConnection))
			{
				for (; ; )
				{
					LastIdNumbers[(Int32)oTableIndex]++;
					aCmd.CommandText = "SELECT * FROM " + YlCommon.MUSIC_INFO_DB_TABLE_NAMES[(Int32)oTableIndex]
							+ " WHERE " + YlCommon.MUSIC_INFO_DB_ID_COLUMN_NAMES[(Int32)oTableIndex] + " = @id";
					aCmd.Parameters.Add(new SQLiteParameter("@id", LastId(oTableIndex)));

					using (SQLiteDataReader aReader = aCmd.ExecuteReader())
					{
						if (!aReader.Read())
						{
							return LastId(oTableIndex);
						}
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// ゆかり設定ファイルのフルパス
		// --------------------------------------------------------------------
		public String YukariConfigPath()
		{
			return Path.GetFullPath(YukariConfigPathSeed);
		}

		// --------------------------------------------------------------------
		// ゆかり用リストデータベースファイル（ディスク）のフルパス
		// --------------------------------------------------------------------
		public String YukariListDbInDiskPath()
		{
			return Path.GetDirectoryName(YukariConfigPath()) + "\\" + FOLDER_NAME_LIST + FILE_NAME_YUKARI_LIST_DB;
		}

		// --------------------------------------------------------------------
		// ゆかり用サムネイルデータベースファイル（ディスク）のフルパス
		// --------------------------------------------------------------------
		public String YukariThumbDbInDiskPath()
		{
			return Path.GetDirectoryName(YukariConfigPath()) + "\\" + FOLDER_NAME_LIST + FILE_NAME_YUKARI_THUMB_DB;
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------

		// ゆかり用リストデータベースファイル名
		private const String FILE_NAME_YUKARI_LIST_DB = "List" + Common.FILE_EXT_SQLITE3;

		// ゆかり用サムネイルデータベースファイル名
		private const String FILE_NAME_YUKARI_THUMB_DB = "Thumb" + Common.FILE_EXT_SQLITE3;

		// ゆかり用データベースを保存するフォルダー名
		private const String FOLDER_NAME_LIST = "list\\";

	}
	// public class YukaListerSettings ___END___

}
// namespace YukaLister.Shared ___END___


