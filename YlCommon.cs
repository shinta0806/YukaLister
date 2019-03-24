// ============================================================================
// 
// ゆかりすたー共通で使用する、定数・関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Hnx8.ReadJEnc;
using Shinta;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YukaLister.Shared
{
	// ====================================================================
	// public 列挙子
	// ====================================================================

	// --------------------------------------------------------------------
	// CSV ファイルの文字コード
	// --------------------------------------------------------------------
	public enum CsvEncoding
	{
		AutoDetect,
		ShiftJis,
		Jis,
		EucJp,
		Utf16Le,
		Utf16Be,
		Utf8,
		__End__,
	}

	// --------------------------------------------------------------------
	// フォルダー除外設定の状態
	// --------------------------------------------------------------------
	public enum FolderExcludeSettingsStatus
	{
		False,      // 除外しない
		True,       // 除外する
		Unchecked,  // 未確認
		__End__
	}

	// --------------------------------------------------------------------
	// フォルダー設定の状態
	// --------------------------------------------------------------------
	public enum FolderSettingsStatus
	{
		None,       // 設定ファイルが存在しない
		Set,        // 当該フォルダーに設定ファイルが存在する
		Inherit,    // 親フォルダーの設定を引き継ぐ
		Unchecked,  // 未確認
		__End__
	}

	// --------------------------------------------------------------------
	// フォルダーに対する操作
	// --------------------------------------------------------------------
	public enum FolderTask
	{
		FindSubFolders, // サブフォルダーの検索（親の場合のみ）
		AddFileName,    // 追加（ファイル名のみ）
		AddInfo,        // 追加（ファイルが追加されたレコードに対してその他の情報を付与）
		Remove,         // 削除
		Update,         // 更新
		__End__
	}

	// --------------------------------------------------------------------
	// フォルダーに対する操作の動作状況
	// --------------------------------------------------------------------
	public enum FolderTaskStatus
	{
		Queued,         // 待機
		Running,        // 実行中
		Error,          // エラー
		DoneInMemory,   // 完了（インメモリデータベースへの反映）
		DoneInDisk,     // 完了（ゆかり用データベースへの反映）
	}

	// --------------------------------------------------------------------
	// ゆかり検索対象フォルダー DGV の列
	// --------------------------------------------------------------------
	public enum FolderColumns
	{
		Acc,            // アコーディオン
		Status,         // 状態
		Folder,         // フォルダー
		SettingsExist,  // 設定有無
	}

	// --------------------------------------------------------------------
	// 楽曲情報データベースのテーブル
	// --------------------------------------------------------------------
	public enum MusicInfoDbTables
	{
		TSong,
		TPerson,
		TTieUp,
		TCategory,
		TTieUpGroup,
		TMaker,
		TSongAlias,
		TPersonAlias,
		TTieUpAlias,
		TCategoryAlias,
		TTieUpGroupAlias,
		TMakerAlias,
		TArtistSequence,
		TLyristSequence,
		TComposerSequence,
		TArrangerSequence,
		TTieUpGroupSequence,
		__End__,
	}

	// --------------------------------------------------------------------
	// リスト出力する項目（ほぼ TFound 準拠）
	// --------------------------------------------------------------------
	public enum OutputItems
	{
		Path,                   // フルパス
		FileName,               // ファイル名
		Head,                   // 頭文字
		Worker,                 // ニコカラ制作者
		Track,                  // トラック
		SmartTrack,             // スマートトラック
		Comment,                // 備考
		LastWriteTime,          // 最終更新日時
		FileSize,               // ファイルサイズ
		SongName,               // 楽曲名
		SongRuby,               // 楽曲フリガナ
		SongOpEd,               // 摘要
		SongReleaseDate,        // リリース日
		ArtistName,             // 歌手名
		ArtistRuby,             // 歌手フリガナ
		LyristName,             // 作詞者名
		LyristRuby,             // 作詞者フリガナ
		ComposerName,           // 作曲者名
		ComposerRuby,           // 作曲者フリガナ
		ArrangerName,           // 編曲者名
		ArrangerRuby,           // 編曲者フリガナ
		TieUpName,              // タイアップ名
		TieUpRuby,              // タイアップフリガナ
		TieUpAgeLimit,          // 年齢制限
		Category,               // カテゴリー
		TieUpGroupName,         // タイアップグループ名
		TieUpGroupRuby,         // タイアップグループフリガナ
		MakerName,              // 制作会社名
		MakerRuby,              // 制作会社フリガナ
		__End__,
	}

	// --------------------------------------------------------------------
	// プレビュー DGV の列
	// --------------------------------------------------------------------
	public enum PreviewColumns
	{
		File,
		Matches,
		Edit,
	}

	// --------------------------------------------------------------------
	// program_alias.csv の列インデックス
	// --------------------------------------------------------------------
	public enum ProgramAliasCsvColumns
	{
		LineIndex,  // CSV には列が無く、プログラム側で付与
		NameOrId,
		Alias,
		ForceId,
		__End__,
	}

	// --------------------------------------------------------------------
	// program.csv の列インデックス
	// --------------------------------------------------------------------
	public enum ProgramCsvColumns
	{
		LineIndex,  // CSV には列が無く、プログラム側で付与
		Id,
		Category,
		GameCategory,
		Name,
		Ruby,
		SubName,
		SubRuby,
		NumStories,
		AgeLimit,
		BeginDate,
		__End__,
	}

	// --------------------------------------------------------------------
	// anison_alias.csv 等の列インデックス
	// --------------------------------------------------------------------
	public enum SongAliasCsvColumns
	{
		LineIndex,  // CSV には列が無く、プログラム側で付与
		NameOrId,
		Alias,
		ForceId,
		__End__,
	}

	// --------------------------------------------------------------------
	// anison.csv 等の列インデックス
	// --------------------------------------------------------------------
	public enum SongCsvColumns
	{
		LineIndex,  // CSV には列が無く、プログラム側で付与
		ProgramId,
		Category,
		ProgramName,
		OpEd,
		CastSeq,
		Id,
		Name,
		Artist,
		__End__,
	}

	// --------------------------------------------------------------------
	// メッセージ定数
	// --------------------------------------------------------------------
	public enum Wm : UInt32
	{
		// タスク系
		LaunchFolderTaskRequested = WindowsApi.WM_APP,
		LaunchListTaskRequested,

		// メインウィンドウ
		UpdateYukaListerStatusRequested,
		UpdateDataGridItemSourceRequested,

		// 一覧ウィンドウ
		FindKeywordRequested,
		FindCellRequested,
	}

	// --------------------------------------------------------------------
	// ゆかりすたーの動作状況
	// --------------------------------------------------------------------
	public enum YukaListerStatus
	{
		Ready,      // 待機
		Running,    // 実行中
		Error,      // エラー
		__End__
	}

	// --------------------------------------------------------------------
	// ゆかりすたー実行中に表示するメッセージ
	// --------------------------------------------------------------------
	public enum YukaListerStatusRunningMessage
	{
		AddTargetFolder,
		RemoveTargetFolder,
		DoFolderTask,
		ListTask,
		__End__
	}

	// ====================================================================
	// public デリゲート
	// ====================================================================
	public delegate void TaskAsyncDelegate<T>(T oVar);
	public delegate YukaListerStatus YukaListerStatusDelegate();

	// ====================================================================
	// ゆかりすたー共通
	// ====================================================================

	public class YlCommon
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		// --------------------------------------------------------------------
		// アプリの基本情報
		// --------------------------------------------------------------------
		public const String APP_ID = "YukaLister";
		public const String APP_GENERATION = "METEOR";
		public const String APP_NAME_J = "ゆかりすたー " + APP_GENERATION + " ";
		public const String APP_VER = "Ver 1.35 β";
		public const String COPYRIGHT_J = "Copyright (C) 2019 by SHINTA";

		// --------------------------------------------------------------------
		// フォルダー名
		// --------------------------------------------------------------------
		public const String FOLDER_NAME_DATABASE = "Database\\";
		public const String FOLDER_NAME_TEMPLATES = "Templates\\";
		public const String FOLDER_NAME_YUKA_LISTER = APP_ID + "\\";

		// --------------------------------------------------------------------
		// ファイル名
		// --------------------------------------------------------------------
		public const String FILE_BODY_ANISON_INFO_CSV_ANISON = "anison";
		public const String FILE_BODY_ANISON_INFO_CSV_ANISON_ALIAS = "anison_alias";
		public const String FILE_BODY_ANISON_INFO_CSV_GAME = "game";
		public const String FILE_BODY_ANISON_INFO_CSV_GAME_ALIAS = "game_alias";
		public const String FILE_BODY_ANISON_INFO_CSV_MISC = "misc";
		public const String FILE_BODY_ANISON_INFO_CSV_MISC_ALIAS = "misc_alias";
		public const String FILE_BODY_ANISON_INFO_CSV_PROGRAM = "program";
		public const String FILE_BODY_ANISON_INFO_CSV_PROGRAM_ALIAS = "program_alias";
		public const String FILE_BODY_ANISON_INFO_CSV_SF = "sf";
		public const String FILE_BODY_ANISON_INFO_CSV_SF_ALIAS = "sf_alias";
		public const String FILE_NAME_MUSIC_INFO = "MusicInfo" + Common.FILE_EXT_SQLITE3;
		public const String FILE_NAME_NICO_KARA_LISTER_CONFIG = "NicoKaraLister" + Common.FILE_EXT_CONFIG;
		public const String FILE_NAME_YUKARI_CONFIG = "config" + Common.FILE_EXT_INI;
		public const String FILE_NAME_YUKA_LISTER_CONFIG = APP_ID + Common.FILE_EXT_CONFIG;
		public const String FILE_NAME_YUKA_LISTER_EXCLUDE_CONFIG = APP_ID + "Exclude" + Common.FILE_EXT_CONFIG;
		public const String FILE_PREFIX_INFO = "Info";

		// --------------------------------------------------------------------
		// 拡張子
		// --------------------------------------------------------------------
		public const String FILE_EXT_NKLINFO = ".nklinfo";

		// --------------------------------------------------------------------
		// 楽曲情報データベース
		// --------------------------------------------------------------------

		// 楽曲情報データベースのテーブル名
		public static readonly String[] MUSIC_INFO_DB_TABLE_NAMES =
		{
			TSong.TABLE_NAME_SONG, TPerson.TABLE_NAME_PERSON, TTieUp.TABLE_NAME_TIE_UP,
			TCategory.TABLE_NAME_CATEGORY, TTieUpGroup.TABLE_NAME_TIE_UP_GROUP, TMaker.TABLE_NAME_MAKER,
			TSongAlias.TABLE_NAME_SONG_ALIAS, TPersonAlias.TABLE_NAME_PERSON_ALIAS, TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS,
			TCategoryAlias.TABLE_NAME_CATEGORY_ALIAS, TTieUpGroupAlias.TABLE_NAME_TIE_UP_GROUP_ALIAS, TMakerAlias.TABLE_NAME_MAKER_ALIAS,
			TArtistSequence.TABLE_NAME_ARTIST_SEQUENCE, TLyristSequence.TABLE_NAME_LYRIST_SEQUENCE, TComposerSequence.TABLE_NAME_COMPOSER_SEQUENCE,
			TArrangerSequence.TABLE_NAME_ARRANGER_SEQUENCE, TTieUpGroupSequence.TABLE_NAME_TIE_UP_GROUP_SEQUENCE,
		};

		// 楽曲情報データベースの ID 列名
		public static readonly String[] MUSIC_INFO_DB_ID_COLUMN_NAMES =
		{
			TSong.FIELD_NAME_SONG_ID, TPerson.FIELD_NAME_PERSON_ID, TTieUp.FIELD_NAME_TIE_UP_ID,
			TCategory.FIELD_NAME_CATEGORY_ID, TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_ID, TMaker.FIELD_NAME_MAKER_ID,
			TSongAlias.FIELD_NAME_SONG_ALIAS_ID, TPersonAlias.FIELD_NAME_PERSON_ALIAS_ID, TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ID,
			TCategoryAlias.FIELD_NAME_CATEGORY_ALIAS_ID, TTieUpGroupAlias.FIELD_NAME_TIE_UP_GROUP_ALIAS_ID, TMakerAlias.FIELD_NAME_MAKER_ALIAS_ID,
			TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_ID, TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_ID, TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_ID,
			TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_ID, TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_ID,
		};

		// 楽曲情報データベースの名前列名
		public static readonly String[] MUSIC_INFO_DB_NAME_COLUMN_NAMES =
		{
			TSong.FIELD_NAME_SONG_NAME, TPerson.FIELD_NAME_PERSON_NAME, TTieUp.FIELD_NAME_TIE_UP_NAME,
			TCategory.FIELD_NAME_CATEGORY_NAME, TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_NAME, TMaker.FIELD_NAME_MAKER_NAME,
			null, null, null, null, null, null,
			null, null, null, null, null,
		};

		// 楽曲情報データベースのフリガナ列名
		public static readonly String[] MUSIC_INFO_DB_RUBY_COLUMN_NAMES =
		{
			TSong.FIELD_NAME_SONG_RUBY, TPerson.FIELD_NAME_PERSON_RUBY, TTieUp.FIELD_NAME_TIE_UP_RUBY,
			TCategory.FIELD_NAME_CATEGORY_RUBY, TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_RUBY, TMaker.FIELD_NAME_MAKER_RUBY,
			null, null, null, null, null, null,
			null, null, null, null, null,
		};

		// 楽曲情報データベースの検索ワード列名
		public static readonly String[] MUSIC_INFO_DB_KEYWORD_COLUMN_NAMES =
		{
			TSong.FIELD_NAME_SONG_KEYWORD, TPerson.FIELD_NAME_PERSON_KEYWORD, TTieUp.FIELD_NAME_TIE_UP_KEYWORD,
			TCategory.FIELD_NAME_CATEGORY_KEYWORD, TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_KEYWORD, TMaker.FIELD_NAME_MAKER_KEYWORD,
			null, null, null, null, null, null,
			null, null, null, null, null,
		};

		// 楽曲情報データベースの無効列名
		public static readonly String[] MUSIC_INFO_DB_INVALID_COLUMN_NAMES =
		{
			TSong.FIELD_NAME_SONG_INVALID, TPerson.FIELD_NAME_PERSON_INVALID, TTieUp.FIELD_NAME_TIE_UP_INVALID,
			TCategory.FIELD_NAME_CATEGORY_INVALID, TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_INVALID, TMaker.FIELD_NAME_MAKER_INVALID,
			TSongAlias.FIELD_NAME_SONG_ALIAS_INVALID, TPersonAlias.FIELD_NAME_PERSON_ALIAS_INVALID, TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_INVALID,
			TCategoryAlias.FIELD_NAME_CATEGORY_ALIAS_INVALID, TTieUpGroupAlias.FIELD_NAME_TIE_UP_GROUP_ALIAS_INVALID, TMakerAlias.FIELD_NAME_MAKER_ALIAS_INVALID,
			TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_INVALID, TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_INVALID, TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_INVALID,
			TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_INVALID, TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_INVALID,
		};

		// 楽曲情報データベースのシステム ID 接頭辞（ユーザーは指定できない文字 '_' を含める）
		public const String MUSIC_INFO_SYSTEM_ID_PREFIX = "_SYS";

		// 楽曲情報データベースの各テーブルの ID 第二接頭辞
		public static readonly String[] MUSIC_INFO_ID_SECOND_PREFIXES =
		{
			"_S_", "_P_", "_T_","_C_", "_G_", "_M_",
			"_SA_", "_PA_", "_TA_","_CA_", "_GA_", "_MA_",
			null, null, null, null, null,
		};

		// --------------------------------------------------------------------
		// アプリ独自ルールでの変数名（小文字で表記）
		// --------------------------------------------------------------------

		// 番組マスターにも同様の項目があるもの
		public const String RULE_VAR_CATEGORY = "category";
		public const String RULE_VAR_GAME_CATEGORY = "gamecategory";
		public const String RULE_VAR_PROGRAM = "program";
		//public const String RULE_VAR_PROGRAM_SUB = "programsub";
		//public const String RULE_VAR_NUM_STORIES = "numstories";
		public const String RULE_VAR_AGE_LIMIT = "agelimit";
		//public const String RULE_VAR_BEGINDATE = "begindate";

		// 楽曲マスターにも同様の項目があるもの
		public const String RULE_VAR_OP_ED = "oped";
		//public const String RULE_VAR_CAST_SEQ = "castseq";
		public const String RULE_VAR_TITLE = "title";
		public const String RULE_VAR_ARTIST = "artist";

		// ファイル名からのみ取得可能なもの
		public const String RULE_VAR_TITLE_RUBY = "titleruby";
		public const String RULE_VAR_WORKER = "worker";
		public const String RULE_VAR_TRACK = "track";
		public const String RULE_VAR_ON_VOCAL = "onvocal";
		public const String RULE_VAR_OFF_VOCAL = "offvocal";
		//public const String RULE_VAR_COMPOSER = "composer";
		//public const String RULE_VAR_LYRIST = "lyrist";
		public const String RULE_VAR_COMMENT = "comment";

		// その他
		public const String RULE_VAR_ANY = "*";

		// 開始終了
		public const String RULE_VAR_BEGIN = "<";
		public const String RULE_VAR_END = ">";

		// --------------------------------------------------------------------
		// 出力設定
		// --------------------------------------------------------------------

		// 新着日数の最小値
		public const Int32 NEW_DAYS_MIN = 1;

		// enum.OutputItems の表示名
		public static readonly String[] OUTPUT_ITEM_NAMES = new String[] { "フルパス", "ファイル名", "頭文字", "ニコカラ制作者", "トラック", "スマートトラック",
				"備考", "最終更新日時", "ファイルサイズ", "楽曲名", "楽曲フリガナ", "摘要", "リリース日",
				"歌手名", "歌手フリガナ", "作詞者名", "作詞者フリガナ", "作曲者名", "作曲者フリガナ", "編曲者名", "編曲者フリガナ",
				"タイアップ名", "タイアップフリガナ", "年齢制限", "カテゴリー", "タイアップグループ名", "タイアップグループフリガナ", "制作会社名", "制作会社フリガナ" };

		// --------------------------------------------------------------------
		// 年齢制限
		// --------------------------------------------------------------------

		public const Int32 AGE_LIMIT_CERO_B = 12;
		public const Int32 AGE_LIMIT_CERO_C = 15;
		public const Int32 AGE_LIMIT_CERO_D = 17;
		public const Int32 AGE_LIMIT_CERO_Z = 18;

		// --------------------------------------------------------------------
		// リソース名
		// --------------------------------------------------------------------

		public const String RSRC_NAME_RAISED_LIGHT_BUTTON = "MaterialDesignRaisedLightButton";

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------

		// extended-length なパス表記の先頭に付与する文字列
		public const String EXTENDED_LENGTH_PATH_PREFIX = @"\\?\";

		// FolderSettingsStatus に対応する文字列
		public static readonly String[] FOLDER_SETTINGS_STATUS_TEXTS = { "無", "有", "親に有", "未確認" };

		// YukaListerStatusRunningMessage に対応する文字列
		public static readonly String[] YUKA_LISTER_STATUS_RUNNING_MESSAGES =
		{
			"検索対象フォルダーを追加しています...\n",
			"検索対象フォルダーから削除しています...",
			"フォルダー情報を更新しています...",
			"リストを更新しています...",
		};

		// グループの「その他」
		public const String GROUP_MISC = "その他";

		// 番組分類の「NEW」
		public const String CATEGORY_NEW = "NEW";

		// 頭文字の「その他」
		public const String HEAD_MISC = GROUP_MISC;

		// 日付の書式指定子
		public const String DATE_FORMAT = "yyyy/MM/dd";

		// 時刻の書式指定子
		public const String TIME_FORMAT = "HH:mm:ss";

		// RULE_VAR_ON_VOCAL / RULE_VAR_OFF_VOCAL のデフォルト値
		public const Int32 RULE_VALUE_VOCAL_DEFAULT = 1;

		// 変数の値を区切る文字
		public const String VAR_VALUE_DELIMITER = ",";

		// スマートトラックでトラック有りの場合の印
		public const String SMART_TRACK_VALID_MARK = "○";

		// スマートトラックでトラック無しの場合の印
		public const String SMART_TRACK_INVALID_MARK = "×";

		// 楽曲情報データベースバックアップ世代数
		public const Int32 NUM_MUSIC_INFO_DB_BACKUP_GENERATIONS = 31;

		// 日付が指定されていない場合はこの年にする
		public const Int32 INVALID_YEAR = 1900;

		// 日付が指定されていない場合の修正ユリウス日
		public static readonly Double INVALID_MJD = JulianDay.DateTimeToModifiedJulianDate(new DateTime(INVALID_YEAR, 1, 1));

		// TCP タイムアウト [ms]
		public const Int32 TCP_TIMEOUT = 10 * 1000;

		// TCP リトライ回数
		public const Int32 TCP_NUM_RETRIES = 5;

		// ツールチップの長めの表示時間 [ms]
		public const Int32 TOOL_TIP_LONG_INTERVAL = 10 * 1000;

		// サムネイルの横幅として指定可能なサイズ [px]
		public static readonly Int32[] THUMB_WIDTH_LIST = new Int32[] { 80, 128, 160, 240, 320 };

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// extended-length なパス表記を使用するかどうか
		public static Boolean IsExLenEnabled { get; set; }

		// ゆかり用データベース（作業用インメモリ）；閉じると消滅するのでアプリ起動中ずっと開きっぱなし
		public static SQLiteConnection YukariDbInMemoryConnection { get; set; }

		// ログ
		public static LogWriter LogWriter { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コンテキストメニューにアイテムを追加
		// --------------------------------------------------------------------
		public static void AddContextMenuItem(FrameworkElement oElement, String oLabel, RoutedEventHandler oClick)
		{
			MenuItem aMenuItem = new MenuItem();
			aMenuItem.Header = oLabel;
			aMenuItem.Click += oClick;
			oElement.ContextMenu.Items.Add(aMenuItem);
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースのバックアップを作成する
		// --------------------------------------------------------------------
		public static void BackupMusicInfoDb()
		{
			try
			{
				if (!File.Exists(MusicInfoDbPath()))
				{
					return;
				}

				FileInfo aDbFileInfo = new FileInfo(MusicInfoDbPath());
				String aBackupDbPath = Path.GetDirectoryName(MusicInfoDbPath()) + "\\" + Path.GetFileNameWithoutExtension(MusicInfoDbPath())
						+ aDbFileInfo.LastWriteTime.ToString("_yyyy_MM_dd") + Common.FILE_EXT_BAK;
				if (File.Exists(aBackupDbPath))
				{
					return;
				}

				// バックアップ
				File.Copy(MusicInfoDbPath(), aBackupDbPath);
				LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報データベースのバックアップ作成：" + aBackupDbPath);

				// 溢れたバックアップを削除
				List<FileInfo> aBackupFileInfos = new List<FileInfo>();
				String[] aBackupFiles = Directory.GetFiles(Path.GetDirectoryName(MusicInfoDbPath()),
						Path.GetFileNameWithoutExtension(MusicInfoDbPath()) + "_(bak)_*" + Common.FILE_EXT_BAK);
				foreach (String aBackupFile in aBackupFiles)
				{
					aBackupFileInfos.Add(new FileInfo(aBackupFile));
				}
				aBackupFileInfos.Sort((a, b) => -a.LastWriteTime.CompareTo(b.LastWriteTime));
				for (Int32 i = aBackupFileInfos.Count - 1; i >= NUM_MUSIC_INFO_DB_BACKUP_GENERATIONS; i--)
				{
					File.Delete(aBackupFileInfos[i].FullName);
					LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報データベースのバックアップ削除：" + aBackupFileInfos[i].FullName);
				}
			}
			catch (Exception oExcep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Warning, "楽曲情報データベースのバックアップ作成が完了しませんでした。\n" + oExcep.Message, true);
			}
		}

		// --------------------------------------------------------------------
		// ID 接頭辞の正当性を確認
		// ＜返値＞ 正規化後の ID 接頭辞
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public static String CheckIdPrefix(String oIdPrefix)
		{
			oIdPrefix = NormalizeDbString(oIdPrefix);

			if (String.IsNullOrEmpty(oIdPrefix))
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞を入力して下さい。");
			}
			if (oIdPrefix.Length > ID_PREFIX_MAX_LENGTH)
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞は " + ID_PREFIX_MAX_LENGTH + "文字以下にして下さい。");
			}
			if (oIdPrefix.IndexOf('_') >= 0)
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞に \"_\" は使えません。");
			}
			if (oIdPrefix.IndexOf(',') >= 0)
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞に \",\" は使えません。");
			}

			return oIdPrefix;
		}

		// --------------------------------------------------------------------
		// カテゴリーテーブルのレコードを作成
		// --------------------------------------------------------------------
		public static TCategory CreateCategoryRecord(Int32 oIdNumber, String oName, String oRuby = null, String oKeyword = null)
		{
			oName = YlCommon.NormalizeDbString(oName);
			if (String.IsNullOrEmpty(oRuby))
			{
				oRuby = oName;
			}
			oRuby = YlCommon.NormalizeDbRuby(oRuby);
			oKeyword = YlCommon.NormalizeDbString(oKeyword);

			return new TCategory
			{
				// TBase
				Id = YlCommon.MUSIC_INFO_SYSTEM_ID_PREFIX + YlCommon.MUSIC_INFO_ID_SECOND_PREFIXES[(Int32)MusicInfoDbTables.TCategory] + oIdNumber.ToString("D3"),
				Import = false,
				Invalid = false,
				UpdateTime = YlCommon.INVALID_MJD,
				Dirty = true,

				// TMaster
				Name = oName,
				Ruby = oRuby,
				Keyword = oKeyword,
			};
		}

		// --------------------------------------------------------------------
		// 番組分類統合用マップを作成
		// --------------------------------------------------------------------
		public static Dictionary<String, String> CreateCategoryUnityMap()
		{
			Dictionary<String, String> aMap = new Dictionary<String, String>();

			aMap["Webアニメーション"] = "アニメ";
			aMap["オリジナルビデオアニメーション"] = "アニメ";
			aMap["テレビアニメーション"] = "アニメ";
			aMap["劇場用アニメーション"] = "アニメ";
			aMap["Webラジオ"] = "ラジオ";
			aMap["Web特撮"] = "特撮";
			aMap["オリジナル特撮ビデオ"] = "特撮";
			aMap["テレビ特撮"] = "特撮";
			aMap["テレビ特撮スペシャル"] = "特撮";
			aMap["劇場用特撮"] = "特撮";

			return aMap;
		}

		// --------------------------------------------------------------------
		// データベースに接続
		// --------------------------------------------------------------------
		public static SQLiteConnection CreateDbConnection(String oPath)
		{
			SQLiteConnectionStringBuilder aConnectionString = new SQLiteConnectionStringBuilder
			{
				DataSource = oPath,
			};
			SQLiteConnection aConnection = new SQLiteConnection(aConnectionString.ToString());
			return aConnection.OpenAndReturn();
		}

		// --------------------------------------------------------------------
		// データベースの中にプロパティーテーブルを作成
		// --------------------------------------------------------------------
		public static void CreateDbPropertyTable(SQLiteConnection oConnection)
		{
			using (SQLiteCommand aCmd = new SQLiteCommand(oConnection))
			{
				// テーブル作成
				LinqUtils.CreateTable(aCmd, typeof(TProperty));
			}

			// 更新
			UpdateDbProperty(oConnection);
		}

		// --------------------------------------------------------------------
		// 設定ファイルのルールを動作時用に変換
		// --------------------------------------------------------------------
		public static FolderSettingsInMemory CreateFolderSettingsInMemory(FolderSettingsInDisk oFolderSettingsInDisk)
		{
			FolderSettingsInMemory aFolderSettingsInMemory = new FolderSettingsInMemory();
			String aRule;
			List<String> aGroups;

			// フォルダー命名規則を辞書に格納
			foreach (String aInDisk in oFolderSettingsInDisk.FolderNameRules)
			{
				Int32 aEqualPos = aInDisk.IndexOf('=');
				if (aEqualPos < 2)
				{
					continue;
				}
				if (aInDisk[0] != RULE_VAR_BEGIN[0])
				{
					continue;
				}
				if (aInDisk[aEqualPos - 1] != RULE_VAR_END[0])
				{
					continue;
				}

				aFolderSettingsInMemory.FolderNameRules[aInDisk.Substring(1, aEqualPos - 2).ToLower()] = aInDisk.Substring(aEqualPos + 1);
			}

			// ファイル命名規則を正規表現に変換
			for (Int32 i = 0; i < oFolderSettingsInDisk.FileNameRules.Count; i++)
			{
				// ワイルドカードのみ <> で囲まれていないので、処理をやりやすくするために <> で囲む
				String aFileNameRule = oFolderSettingsInDisk.FileNameRules[i].Replace(RULE_VAR_ANY, RULE_VAR_BEGIN + RULE_VAR_ANY + RULE_VAR_END);

				MakeRegexPattern(aFileNameRule, out aRule, out aGroups);
				aFolderSettingsInMemory.FileNameRules.Add(aRule);
				aFolderSettingsInMemory.FileRegexGroups.Add(aGroups);
			}

			return aFolderSettingsInMemory;
		}

		// --------------------------------------------------------------------
		// 空の楽曲情報データベースを作成（既存のものは削除）
		// ユニーク制約のカラムにはインデックスが自動作成される（速度実験により確認済み）
		// 主キーもユニーク制約がかかるのでインデックスは自動作成されると思われる（未確認）
		// --------------------------------------------------------------------
		public static void CreateMusicInfoDb()
		{
			BackupMusicInfoDb();

			LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報データベースを新規作成します...");

			Directory.CreateDirectory(Path.GetDirectoryName(YlCommon.MusicInfoDbPath()));

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				// 既存テーブルがある場合は削除
				LinqUtils.DropAllTables(aConnection);

				using (SQLiteCommand aCmd = new SQLiteCommand(aConnection))
				{
					List<String> aIndices = new List<String>();

					// マスターテーブル
					aIndices.Clear();
					aIndices.Add(TSong.FIELD_NAME_SONG_NAME);
					aIndices.Add(TSong.FIELD_NAME_SONG_CATEGORY_ID);
					aIndices.Add(TSong.FIELD_NAME_SONG_OP_ED);
					CreateMusicInfoDbTable(aCmd, typeof(TSong), aIndices);

					CreateMusicInfoDbTable(aCmd, typeof(TPerson), TPerson.FIELD_NAME_PERSON_NAME);

					aIndices.Clear();
					aIndices.Add(TTieUp.FIELD_NAME_TIE_UP_NAME);
					aIndices.Add(TTieUp.FIELD_NAME_TIE_UP_CATEGORY_ID);
					CreateMusicInfoDbTable(aCmd, typeof(TTieUp), aIndices);

					CreateMusicInfoDbTable(aCmd, typeof(TCategory), TCategory.FIELD_NAME_CATEGORY_NAME);
					InsertMusicInfoDbCategoryDefaultRecords(aConnection);

					CreateMusicInfoDbTable(aCmd, typeof(TTieUpGroup), TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_NAME);

					CreateMusicInfoDbTable(aCmd, typeof(TMaker), TMaker.FIELD_NAME_MAKER_NAME);

					// 別名テーブル
					CreateMusicInfoDbTable(aCmd, typeof(TSongAlias), TSongAlias.FIELD_NAME_SONG_ALIAS_ALIAS);

					CreateMusicInfoDbTable(aCmd, typeof(TPersonAlias), TPersonAlias.FIELD_NAME_PERSON_ALIAS_ALIAS);

					CreateMusicInfoDbTable(aCmd, typeof(TTieUpAlias), TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ALIAS);

					CreateMusicInfoDbTable(aCmd, typeof(TCategoryAlias), TCategoryAlias.FIELD_NAME_CATEGORY_ALIAS_ALIAS);

					CreateMusicInfoDbTable(aCmd, typeof(TTieUpGroupAlias), TTieUpGroupAlias.FIELD_NAME_TIE_UP_GROUP_ALIAS_ALIAS);

					CreateMusicInfoDbTable(aCmd, typeof(TMakerAlias), TMakerAlias.FIELD_NAME_MAKER_ALIAS_ALIAS);

					// 紐付テーブル
					CreateMusicInfoDbTable(aCmd, typeof(TArtistSequence));

					CreateMusicInfoDbTable(aCmd, typeof(TLyristSequence));

					CreateMusicInfoDbTable(aCmd, typeof(TComposerSequence));

					CreateMusicInfoDbTable(aCmd, typeof(TArrangerSequence));

					CreateMusicInfoDbTable(aCmd, typeof(TTieUpGroupSequence));
				}

				// プロパティーテーブル
				CreateDbPropertyTable(aConnection);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースに接続
		// --------------------------------------------------------------------
		public static SQLiteConnection CreateMusicInfoDbConnection()
		{
			return CreateDbConnection(MusicInfoDbPath());
		}

		// --------------------------------------------------------------------
		// アプリ独自の変数を格納する変数を生成し、定義済みキーをすべて初期化（キーには <> は含まない）
		// ・キーが無いと LINQ で例外が発生することがあるため
		// ・キーの有無と値の null の 2 度チェックは面倒くさいため
		// --------------------------------------------------------------------
		public static Dictionary<String, String> CreateRuleDictionary()
		{
			Dictionary<String, String> aVarMapWith = CreateRuleDictionaryWithDescription();
			Dictionary<String, String> aVarMap = new Dictionary<String, String>();

			foreach (String aKey in aVarMapWith.Keys)
			{
				aVarMap[aKey] = null;
			}

			return aVarMap;
		}

		// --------------------------------------------------------------------
		// アプリ独自の変数とその説明
		// --------------------------------------------------------------------
		public static Dictionary<String, String> CreateRuleDictionaryWithDescription()
		{
			Dictionary<String, String> aVarMap = new Dictionary<String, String>();

			// 番組マスターにも同様の項目があるもの
			aVarMap[RULE_VAR_CATEGORY] = "番組分類";
			//aVarMap[RULE_VAR_GAME_CATEGORY] = "ゲーム種別";
			aVarMap[RULE_VAR_PROGRAM] = "番組名";
			aVarMap[RULE_VAR_AGE_LIMIT] = "年齢制限";

			// 楽曲マスターにも同様の項目があるもの
			aVarMap[RULE_VAR_OP_ED] = "摘要（OP/ED 別）";
			aVarMap[RULE_VAR_TITLE] = "楽曲名";

			// ファイル名からのみ取得可能なもの
			aVarMap[RULE_VAR_TITLE_RUBY] = "ガッキョクメイ";

			// 楽曲マスターにも同様の項目があるもの
			aVarMap[RULE_VAR_ARTIST] = "歌手名";

			// ファイル名からのみ取得可能なもの
			aVarMap[RULE_VAR_WORKER] = "ニコカラ制作者";
			aVarMap[RULE_VAR_TRACK] = "トラック情報";
			aVarMap[RULE_VAR_ON_VOCAL] = "オンボーカルトラック";
			aVarMap[RULE_VAR_OFF_VOCAL] = "オフボーカルトラック";
			//aVarMap[NklCommon.RULE_VAR_COMPOSER] = "作曲者";
			//aVarMap[NklCommon.RULE_VAR_LYRIST] = "作詞者";
			aVarMap[RULE_VAR_COMMENT] = "コメント";

			// その他
			aVarMap[RULE_VAR_ANY] = "無視する部分";

			return aVarMap;
		}

		// --------------------------------------------------------------------
		// 編曲者者紐付テーブルのレコードを作成
		// --------------------------------------------------------------------
		public static TArrangerSequence CreateTArrangerSequence(String oSongId, Int32 oSequence, String oPersonId)
		{
			return new TArrangerSequence
			{
				// TBase
				Import = false,
				Invalid = false,
				UpdateTime = INVALID_MJD,
				Dirty = true,

				// TSequence
				SongId = oSongId,
				Sequence = oSequence,
				PersonId = oPersonId,
			};
		}

		// --------------------------------------------------------------------
		// 歌手紐付テーブルのレコードを作成
		// --------------------------------------------------------------------
		public static TArtistSequence CreateTArtistSequence(String oSongId, Int32 oSequence, String oPersonId, Boolean oIsImport = false)
		{
			return new TArtistSequence
			{
				// TBase
				Import = oIsImport,
				Invalid = false,
				UpdateTime = INVALID_MJD,
				Dirty = true,

				// TSequence
				SongId = oSongId,
				Sequence = oSequence,
				PersonId = oPersonId,
			};
		}

		// --------------------------------------------------------------------
		// 作曲者紐付テーブルのレコードを作成
		// --------------------------------------------------------------------
		public static TComposerSequence CreateTComposerSequence(String oSongId, Int32 oSequence, String oPersonId)
		{
			return new TComposerSequence
			{
				// TBase
				Import = false,
				Invalid = false,
				UpdateTime = INVALID_MJD,
				Dirty = true,

				// TSequence
				SongId = oSongId,
				Sequence = oSequence,
				PersonId = oPersonId,
			};
		}

		// --------------------------------------------------------------------
		// 作詞者紐付テーブルのレコードを作成
		// --------------------------------------------------------------------
		public static TLyristSequence CreateTLyristSequence(String oSongId, Int32 oSequence, String oPersonId)
		{
			return new TLyristSequence
			{
				// TBase
				Import = false,
				Invalid = false,
				UpdateTime = INVALID_MJD,
				Dirty = true,

				// TSequence
				SongId = oSongId,
				Sequence = oSequence,
				PersonId = oPersonId,
			};
		}

		// --------------------------------------------------------------------
		// タイアップグループ紐付テーブルのレコードを作成
		// --------------------------------------------------------------------
		public static TTieUpGroupSequence CreateTTieUpGroupSequence(String oTieUpId, Int32 oSequence, String oTieUpGroupId)
		{
			return new TTieUpGroupSequence
			{
				// TBase
				Import = false,
				Invalid = false,
				UpdateTime = INVALID_MJD,
				Dirty = true,

				// TSequence
				TieUpId = oTieUpId,
				Sequence = oSequence,
				TieUpGroupId = oTieUpGroupId,
			};
		}

		// --------------------------------------------------------------------
		// ゆかり用リストデータベース（ディスク）に接続
		// --------------------------------------------------------------------
		public static SQLiteConnection CreateYukariListDbInDiskConnection(YukaListerSettings oYukaListerSettings)
		{
			return YlCommon.CreateDbConnection(oYukaListerSettings.YukariListDbInDiskPath());
		}

		// --------------------------------------------------------------------
		// ゆかり用サムネイルデータベース（ディスク）に接続
		// --------------------------------------------------------------------
		public static SQLiteConnection CreateYukariThumbDbInDiskConnection(YukaListerSettings oYukaListerSettings)
		{
			return YlCommon.CreateDbConnection(oYukaListerSettings.YukariThumbDbInDiskPath());
		}

		// --------------------------------------------------------------------
		// 暗号化して Base64 になっている文字列を復号化する
		// --------------------------------------------------------------------
		public static String Decrypt(String oBase64Text)
		{
			if (String.IsNullOrEmpty(oBase64Text))
			{
				return null;
			}

			Byte[] aCipherBytes = Convert.FromBase64String(oBase64Text);

			using (AesManaged aAes = new AesManaged())
			{
				using (ICryptoTransform aDecryptor = aAes.CreateDecryptor(ENCRYPT_KEY, ENCRYPT_IV))
				{
					using (MemoryStream aWriteStream = new MemoryStream())
					{
						// 復号化
						using (CryptoStream aCryptoStream = new CryptoStream(aWriteStream, aDecryptor, CryptoStreamMode.Write))
						{
							aCryptoStream.Write(aCipherBytes, 0, aCipherBytes.Length);
						}

						// 文字列化
						Byte[] aPlainBytes = aWriteStream.ToArray();
						return Encoding.Unicode.GetString(aPlainBytes);
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 指定されたフォルダーの除外設定有無
		// --------------------------------------------------------------------
		public static FolderExcludeSettingsStatus DetectFolderExcludeSettingsStatus(String oFolderExLen)
		{
			String aFolderExcludeSettingsFolder = FindExcludeSettingsFolder2Ex(oFolderExLen);
			if (String.IsNullOrEmpty(aFolderExcludeSettingsFolder))
			{
				return FolderExcludeSettingsStatus.False;
			}
			else
			{
				return FolderExcludeSettingsStatus.True;
			}
		}

		// --------------------------------------------------------------------
		// 指定されたフォルダーの設定有無
		// --------------------------------------------------------------------
		public static FolderSettingsStatus DetectFolderSettingsStatus2Ex(String oFolderExLen)
		{
			String aFolderSettingsFolder = FindSettingsFolder2Ex(oFolderExLen);
			if (String.IsNullOrEmpty(aFolderSettingsFolder))
			{
				return FolderSettingsStatus.None;
			}
			else if (IsSamePath(oFolderExLen, aFolderSettingsFolder))
			{
				return FolderSettingsStatus.Set;
			}
			else
			{
				return FolderSettingsStatus.Inherit;
			}
		}

		// --------------------------------------------------------------------
		// CsvEncoding から Encoding を得る
		// --------------------------------------------------------------------
		public static Encoding EncodingFromCsvEncoding(CsvEncoding oCsvEncoding)
		{
			Encoding aEncoding = null;
			switch (oCsvEncoding)
			{
				case CsvEncoding.ShiftJis:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_SHIFT_JIS);
					break;
				case CsvEncoding.Jis:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_JIS);
					break;
				case CsvEncoding.EucJp:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_EUC_JP);
					break;
				case CsvEncoding.Utf16Le:
					aEncoding = Encoding.Unicode;
					break;
				case CsvEncoding.Utf16Be:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_UTF_16_BE);
					break;
				case CsvEncoding.Utf8:
					aEncoding = Encoding.UTF8;
					break;
				default:
					Debug.Assert(false, "EncodingFromCsvEncoding() bad csv encoding");
					break;
			}
			return aEncoding;
		}

		// --------------------------------------------------------------------
		// 文字列を AES 256 bit 暗号化して Base64 で返す
		// --------------------------------------------------------------------
		public static String Encrypt(String oPlainText)
		{
			if (String.IsNullOrEmpty(oPlainText))
			{
				return null;
			}

			Byte[] aPlainBytes = Encoding.Unicode.GetBytes(oPlainText);

			using (AesManaged aAes = new AesManaged())
			{
				using (ICryptoTransform aEncryptor = aAes.CreateEncryptor(ENCRYPT_KEY, ENCRYPT_IV))
				{
					using (MemoryStream aWriteStream = new MemoryStream())
					{
						// 暗号化
						using (CryptoStream aCryptoStream = new CryptoStream(aWriteStream, aEncryptor, CryptoStreamMode.Write))
						{
							aCryptoStream.Write(aPlainBytes, 0, aPlainBytes.Length);
						}

						// Base64
						Byte[] aCipherBytes = aWriteStream.ToArray();
						return Convert.ToBase64String(aCipherBytes);
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// extended-length なパス表記でない場合は extended-length なパス表記に変換
		// MAX_PATH (=260) 文字以上のパスを扱えるようにする
		// IsExLenEnabled == true の場合のみ変換する
		// --------------------------------------------------------------------
		public static String ExtendPath(String oPath)
		{
			if (!IsExLenEnabled || oPath.StartsWith(EXTENDED_LENGTH_PATH_PREFIX))
			{
				return oPath;
			}

			// MAX_PATH 文字以上のフォルダー名をダイアログから取得した場合など、短いファイル名形式になっていることがあるため、
			// Path.GetFullPath() で長いファイル名形式に変換する
			return EXTENDED_LENGTH_PATH_PREFIX + Path.GetFullPath(oPath);
		}

		// --------------------------------------------------------------------
		// 指定されたフォルダーのフォルダー除外設定ファイルがあるフォルダーを返す
		// --------------------------------------------------------------------
		public static String FindExcludeSettingsFolder2Ex(String oFolderExLen)
		{
			while (!String.IsNullOrEmpty(oFolderExLen))
			{
				if (File.Exists(oFolderExLen + "\\" + FILE_NAME_YUKA_LISTER_EXCLUDE_CONFIG))
				{
					return oFolderExLen;
				}
				oFolderExLen = Path.GetDirectoryName(oFolderExLen);
			}
			return null;
		}

		// --------------------------------------------------------------------
		// 指定されたフォルダーのフォルダー設定ファイルがあるフォルダーを返す
		// 互換性維持のため、ニコカラりすたーの設定ファイルも扱う
		// --------------------------------------------------------------------
		public static String FindSettingsFolder2Ex(String oFolderExLen)
		{
			while (!String.IsNullOrEmpty(oFolderExLen))
			{
				if (File.Exists(oFolderExLen + "\\" + FILE_NAME_YUKA_LISTER_CONFIG))
				{
					return oFolderExLen;
				}
				if (File.Exists(oFolderExLen + "\\" + FILE_NAME_NICO_KARA_LISTER_CONFIG))
				{
					return oFolderExLen;
				}
				oFolderExLen = Path.GetDirectoryName(oFolderExLen);
			}
			return null;
		}

		// --------------------------------------------------------------------
		// データベースのプロパティーを取得
		// --------------------------------------------------------------------
		public static TProperty GetDbProperty(SQLiteConnection oConnection)
		{
			try
			{
				using (DataContext aContext = new DataContext(oConnection))
				{
					Table<TProperty> aTableProperty = aContext.GetTable<TProperty>();

					IQueryable<TProperty> aQueryResult =
							from x in aTableProperty
							select x;
					foreach (TProperty aProperty in aQueryResult)
					{
						return aProperty;
					}
				}
			}
			catch (Exception)
			{
			}

			return new TProperty();
		}

		// --------------------------------------------------------------------
		// 頭文字を返す
		// ひらがな（濁点なし）、その他、のいずれか
		// --------------------------------------------------------------------
		public static String Head(String oString)
		{
			if (String.IsNullOrEmpty(oString))
			{
				return HEAD_MISC;
			}

			Char aChar = oString[0];

			// カタカナをひらがなに変換
			if ('ァ' <= aChar && aChar <= 'ヶ')
			{
				aChar = (Char)(aChar - 0x0060);
			}

			// 濁点・小文字をノーマルに変換
			Int32 aHeadConvertPos = HEAD_CONVERT_FROM.IndexOf(aChar);
			if (aHeadConvertPos >= 0)
			{
				aChar = HEAD_CONVERT_TO[aHeadConvertPos];
			}

			// ひらがなを返す
			if ('あ' <= aChar && aChar <= 'ん')
			{
				return new string(aChar, 1);
			}

			return HEAD_MISC;
		}

		// --------------------------------------------------------------------
		// ID 接頭辞が未設定ならばユーザーに入力してもらう
		// ＜例外＞ OperationCanceledException
		// --------------------------------------------------------------------
		public static void InputIdPrefixIfNeededWithInvoke(Window oOwner, YukaListerSettings oYukaListerSettings)
		{
			if (!String.IsNullOrEmpty(oYukaListerSettings.IdPrefix))
			{
				return;
			}

			InputIdPrefixWindow aInputIdPrefixWindow = new InputIdPrefixWindow(LogWriter);
			oOwner.Dispatcher.Invoke(new Action(() =>
			{
				aInputIdPrefixWindow.Owner = oOwner;
				if (!(Boolean)aInputIdPrefixWindow.ShowDialog())
				{
					throw new OperationCanceledException();
				}
				oYukaListerSettings.IdPrefix = aInputIdPrefixWindow.IdPrefix;
			}));
		}

		// --------------------------------------------------------------------
		// 同一のファイル・フォルダーかどうか
		// 末尾の '\\' 有無や大文字小文字にかかわらず比較する
		// いずれかが null の場合は false とする
		// パスは extended-length でもそうでなくても可
		// --------------------------------------------------------------------
		public static Boolean IsSamePath(String oPath1, String oPath2)
		{
			if (String.IsNullOrEmpty(oPath1) || String.IsNullOrEmpty(oPath2))
			{
				return false;
			}

			// 末尾の '\\' を除去
			if (oPath1[oPath1.Length - 1] == '\\')
			{
				oPath1 = oPath1.Substring(0, oPath1.Length - 1);
			}
			if (oPath2[oPath2.Length - 1] == '\\')
			{
				oPath2 = oPath2.Substring(0, oPath2.Length - 1);
			}
			return (oPath1.ToLower() == oPath2.ToLower());
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TArrangerSequence）
		// プライマリーキーは比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TArrangerSequence oExistRecord, TArrangerSequence oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.PersonId != oNewRecord.PersonId;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TArtistSequence）
		// プライマリーキーは比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TArtistSequence oExistRecord, TArtistSequence oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.PersonId != oNewRecord.PersonId;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TComposerSequence）
		// プライマリーキーは比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TComposerSequence oExistRecord, TComposerSequence oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.PersonId != oNewRecord.PersonId;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TLyristSequence）
		// プライマリーキーは比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TLyristSequence oExistRecord, TLyristSequence oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.PersonId != oNewRecord.PersonId;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TMaker）
		// ID は比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TMaker oExistRecord, TMaker oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.Name != oNewRecord.Name
					|| oExistRecord.Ruby != oNewRecord.Ruby
					|| oExistRecord.Keyword != oNewRecord.Keyword;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TPerson）
		// ID は比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TPerson oExistRecord, TPerson oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.Name != oNewRecord.Name
					|| oExistRecord.Ruby != oNewRecord.Ruby
					|| oExistRecord.Keyword != oNewRecord.Keyword;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TSong）
		// ID は比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TSong oExistRecord, TSong oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.Name != oNewRecord.Name
					|| oExistRecord.Ruby != oNewRecord.Ruby
					|| oExistRecord.ReleaseDate != oNewRecord.ReleaseDate
					|| oExistRecord.TieUpId != oNewRecord.TieUpId
					|| oExistRecord.CategoryId != oNewRecord.CategoryId
					|| oExistRecord.OpEd != oNewRecord.OpEd
					|| oExistRecord.Keyword != oNewRecord.Keyword;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TSongAlias）
		// ID は比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TSongAlias oExistRecord, TSongAlias oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.Alias != oNewRecord.Alias
					|| oExistRecord.OriginalId != oNewRecord.OriginalId;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TTieUp）
		// ID は比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TTieUp oExistRecord, TTieUp oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.Name != oNewRecord.Name
					|| oExistRecord.Ruby != oNewRecord.Ruby
					|| oExistRecord.CategoryId != oNewRecord.CategoryId
					|| oExistRecord.MakerId != oNewRecord.MakerId
					|| oExistRecord.AgeLimit != oNewRecord.AgeLimit
					|| oExistRecord.ReleaseDate != oNewRecord.ReleaseDate
					|| oExistRecord.Keyword != oNewRecord.Keyword;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TTieUpAlias）
		// ID は比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TTieUpAlias oExistRecord, TTieUpAlias oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.Alias != oNewRecord.Alias
					|| oExistRecord.OriginalId != oNewRecord.OriginalId;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TTieUpGroup）
		// ID は比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TTieUpGroup oExistRecord, TTieUpGroup oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.Name != oNewRecord.Name
					|| oExistRecord.Ruby != oNewRecord.Ruby
					|| oExistRecord.Keyword != oNewRecord.Keyword;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TTieUpGroupSequence）
		// プライマリーキーは比較しない
		// --------------------------------------------------------------------
		public static Boolean IsUpdated(TTieUpGroupSequence oExistRecord, TTieUpGroupSequence oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			return oExistRecord.TieUpGroupId != oNewRecord.TieUpGroupId;
		}

		// --------------------------------------------------------------------
		// 関数を非同期駆動
		// --------------------------------------------------------------------
		public static Task LaunchTaskAsync<T>(TaskAsyncDelegate<T> oDelegate, Object oTaskLock, T oVar)
		{
			return Task.Run(() =>
			{
				try
				{
					// 終了時に強制終了されないように設定
					Thread.CurrentThread.IsBackground = false;

					lock (oTaskLock)
					{
						// 関数処理
						LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バックグラウンド処理開始：" + oDelegate.Method.Name);
						oDelegate(oVar);
#if DEBUGz
						Thread.Sleep(5000);
#endif
						LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バックグラウンド処理終了：" + oDelegate.Method.Name);
					}
				}
				catch (Exception oExcep)
				{
					LogWriter.ShowLogMessage(TraceEventType.Error, "バックグラウンド処理 " + oDelegate.Method.Name + "実行時エラー：\n" + oExcep.Message);
					LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
				}
			});
		}

		// --------------------------------------------------------------------
		// ちょちょいと自動更新を起動
		// --------------------------------------------------------------------
		public static Boolean LaunchUpdater(Boolean oCheckLatest, Boolean oForceShow, IntPtr oHWnd, Boolean oClearUpdateCache, Boolean oForceInstall)
		{
			// 固定部分
			UpdaterLauncher aUpdaterLauncher = new UpdaterLauncher();
			aUpdaterLauncher.ID = APP_ID;
			aUpdaterLauncher.Name = APP_NAME_J;
			aUpdaterLauncher.Wait = 3;
			aUpdaterLauncher.UpdateRss = "http://shinta.coresv.com/soft/YukaListerMeteor_AutoUpdate.xml";
			aUpdaterLauncher.CurrentVer = APP_VER;

			// 変動部分
			if (oCheckLatest)
			{
				aUpdaterLauncher.LatestRss = "http://shinta.coresv.com/soft/YukaListerMeteor_JPN.xml";
			}
			aUpdaterLauncher.LogWriter = LogWriter;
			aUpdaterLauncher.ForceShow = oForceShow;
			aUpdaterLauncher.NotifyHWnd = oHWnd;
			aUpdaterLauncher.ClearUpdateCache = oClearUpdateCache;
			aUpdaterLauncher.ForceInstall = oForceInstall;

			// 起動
			return aUpdaterLauncher.Launch(oForceShow);
		}

		// --------------------------------------------------------------------
		// 環境設定の文字コードに従って CSV ファイルを読み込む
		// 下処理も行う
		// oNumColumns: 行番号も含めた列数
		// --------------------------------------------------------------------
		public static List<List<String>> LoadCsv(String oPath, YukaListerSettings oYukaListerSettings, Int32 oNumColumns)
		{
			List<List<String>> aCsv;

			try
			{
				Encoding aEncoding;
				if (oYukaListerSettings.CsvEncoding == CsvEncoding.AutoDetect)
				{
					// 文字コード自動判別
					FileInfo aFileInfo = new FileInfo(oPath);
					using (FileReader aReader = new FileReader(aFileInfo))
					{
						aEncoding = aReader.Read(aFileInfo).GetEncoding();
					}
				}
				else
				{
					aEncoding = EncodingFromCsvEncoding(oYukaListerSettings.CsvEncoding);
				}
				if (aEncoding == null)
				{
					throw new Exception("文字コードを判定できませんでした。");
				}
				aCsv = CsvManager.LoadCsv(oPath, aEncoding, true, true);

				// 規定列数に満たない行を削除
				for (Int32 i = aCsv.Count - 1; i >= 0; i--)
				{
					if (aCsv[i].Count != oNumColumns)
					{
						LogWriter.ShowLogMessage(TraceEventType.Warning,
								(Int32.Parse(aCsv[i][0]) + 2).ToString("#,0") + " 行目は項目数の過不足があるため無視します。", true);
						aCsv.RemoveAt(i);
					}
				}

				// 空白削除
				for (Int32 i = 0; i < aCsv.Count; i++)
				{
					List<String> aRecord = aCsv[i];
					for (Int32 j = 0; j < aRecord.Count; j++)
					{
						aRecord[j] = aRecord[j].Trim();
					}
				}
			}
			catch (Exception oExcep)
			{
				aCsv = new List<List<String>>();
				LogWriter.ShowLogMessage(TraceEventType.Warning, "CSV ファイルを読み込めませんでした。\n" + oExcep.Message + "\n" + oPath, true);
			}
			return aCsv;
		}

		// --------------------------------------------------------------------
		// フォルダー設定を読み込む
		// FILE_NAME_YUKA_LISTER_CONFIG 優先、無い場合は FILE_NAME_NICO_KARA_LISTER_CONFIG
		// 見つからない場合は null ではなく空のインスタンスを返す
		// --------------------------------------------------------------------
		public static FolderSettingsInDisk LoadFolderSettings2Ex(String oFolderExLen)
		{
			FolderSettingsInDisk aFolderSettings = new FolderSettingsInDisk();
			try
			{
				String aFolderSettingsFolder = FindSettingsFolder2Ex(oFolderExLen);
				if (!String.IsNullOrEmpty(aFolderSettingsFolder))
				{
					if (File.Exists(aFolderSettingsFolder + "\\" + FILE_NAME_YUKA_LISTER_CONFIG))
					{
						aFolderSettings = Common.Deserialize<FolderSettingsInDisk>(aFolderSettingsFolder + "\\" + FILE_NAME_YUKA_LISTER_CONFIG);
					}
					else
					{
						aFolderSettings = Common.Deserialize<FolderSettingsInDisk>(aFolderSettingsFolder + "\\" + FILE_NAME_NICO_KARA_LISTER_CONFIG);
					}
				}
			}
			catch (Exception)
			{
			}

			// 項目が null の場合はインスタンスを作成
			if (aFolderSettings.FileNameRules == null)
			{
				aFolderSettings.FileNameRules = new List<String>();
			}
			if (aFolderSettings.FolderNameRules == null)
			{
				aFolderSettings.FolderNameRules = new List<String>();
			}

			return aFolderSettings;
		}

		// --------------------------------------------------------------------
		// 環境情報をログする
		// --------------------------------------------------------------------
		public static void LogEnvironmentInfo()
		{
			SystemEnvironment aSE = new SystemEnvironment();
			aSE.LogEnvironment(LogWriter);
		}

		// --------------------------------------------------------------------
		// ファイル名とファイル命名規則がマッチするか確認し、マッチしたマップを返す
		// ＜引数＞ oFileNameBody: 拡張子無し
		// --------------------------------------------------------------------
		public static Dictionary<String, String> MatchFileNameRules(String oFileNameBody, FolderSettingsInMemory oFolderSettingsInMemory)
		{
			Dictionary<String, String> aDic = CreateRuleDictionary();
			Match aMatch = null;
			Int32 aMatchIndex = -1;

			// ファイル名と合致する命名規則を探す
			for (Int32 i = 0; i < oFolderSettingsInMemory.FileNameRules.Count; i++)
			{
				aMatch = Regex.Match(oFileNameBody, oFolderSettingsInMemory.FileNameRules[i], RegexOptions.None);
				if (aMatch.Success)
				{
					aMatchIndex = i;
					break;
				}
			}
			if (aMatchIndex < 0)
			{
				return aDic;
			}

			for (Int32 i = 0; i < oFolderSettingsInMemory.FileRegexGroups[aMatchIndex].Count; i++)
			{
				// 定義されているキーのみ格納する
				if (aDic.ContainsKey(oFolderSettingsInMemory.FileRegexGroups[aMatchIndex][i]))
				{
					// aMatch.Groups[0] にはマッチした全体の値が入っているので無視し、[1] から実際の値が入っている
					if (String.IsNullOrEmpty(aDic[oFolderSettingsInMemory.FileRegexGroups[aMatchIndex][i]]))
					{
						aDic[oFolderSettingsInMemory.FileRegexGroups[aMatchIndex][i]] = aMatch.Groups[i + 1].Value.Trim();
					}
					else
					{
						aDic[oFolderSettingsInMemory.FileRegexGroups[aMatchIndex][i]] += VAR_VALUE_DELIMITER + aMatch.Groups[i + 1].Value.Trim();
					}
				}
			}

			// 正規化
			aDic[RULE_VAR_CATEGORY] = NormalizeDbString(aDic[RULE_VAR_CATEGORY]);
			aDic[RULE_VAR_PROGRAM] = NormalizeDbString(aDic[RULE_VAR_PROGRAM]);
			aDic[RULE_VAR_AGE_LIMIT] = NormalizeDbString(aDic[RULE_VAR_AGE_LIMIT]);
			aDic[RULE_VAR_OP_ED] = NormalizeDbString(aDic[RULE_VAR_OP_ED]);
			aDic[RULE_VAR_TITLE] = NormalizeDbString(aDic[RULE_VAR_TITLE]);
			aDic[RULE_VAR_TITLE_RUBY] = NormalizeDbRuby(aDic[RULE_VAR_TITLE_RUBY]);
			aDic[RULE_VAR_ARTIST] = NormalizeDbString(aDic[RULE_VAR_ARTIST]);
			aDic[RULE_VAR_WORKER] = NormalizeDbString(aDic[RULE_VAR_WORKER]);
			aDic[RULE_VAR_TRACK] = NormalizeDbString(aDic[RULE_VAR_TRACK]);
			aDic[RULE_VAR_COMMENT] = NormalizeDbString(aDic[RULE_VAR_COMMENT]);

			return aDic;
		}

		// --------------------------------------------------------------------
		// ファイル名とファイル命名規則・フォルダー固定値がマッチするか確認し、マッチしたマップを返す
		// ＜引数＞ oFileNameBody: 拡張子無し
		// --------------------------------------------------------------------
		public static Dictionary<String, String> MatchFileNameRulesAndFolderRule(String oFileNameBody, FolderSettingsInMemory oFolderSettingsInMemory)
		{
			// ファイル名命名規則
			Dictionary<String, String> aDic = YlCommon.MatchFileNameRules(oFileNameBody, oFolderSettingsInMemory);

			// フォルダー命名規則をマージ
			foreach (KeyValuePair<String, String> aFolderRule in oFolderSettingsInMemory.FolderNameRules)
			{
				if (aDic.ContainsKey(aFolderRule.Key) && String.IsNullOrEmpty(aDic[aFolderRule.Key]))
				{
					aDic[aFolderRule.Key] = aFolderRule.Value;
				}
			}

			return aDic;
		}

		// --------------------------------------------------------------------
		// 日付に合わせてテキストボックスを設定
		// --------------------------------------------------------------------
		public static void MjdToTextBox(Double oMjd, TextBox oTextBoxYear, TextBox oTextBoxMonth, TextBox oTextBoxDay)
		{
			if (oMjd <= YlCommon.INVALID_MJD)
			{
				oTextBoxYear.Text = null;
				oTextBoxMonth.Text = null;
				oTextBoxDay.Text = null;
			}
			else
			{
				DateTime aReleaseDate = JulianDay.ModifiedJulianDateToDateTime(oMjd);
				oTextBoxYear.Text = aReleaseDate.Year.ToString();
				oTextBoxMonth.Text = aReleaseDate.Month.ToString();
				oTextBoxDay.Text = aReleaseDate.Day.ToString();
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースファイルのフルパス
		// --------------------------------------------------------------------
		public static String MusicInfoDbPath()
		{
			return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + FOLDER_NAME_DATABASE + FILE_NAME_MUSIC_INFO;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースに登録するフリガナの表記揺れを減らす
		// ＜返値＞ フリガナ表記 or null（空になる場合）
		// --------------------------------------------------------------------
		public static String NormalizeDbRuby(String oString)
		{
			Debug.Assert(NORMALIZE_DB_RUBY_FROM.Length == NORMALIZE_DB_RUBY_TO.Length, "NormalizeDbRuby() different NORMALIZE_DB_FURIGANA_FROM NORMALIZE_DB_FURIGANA_TO length");

			if (String.IsNullOrEmpty(oString))
			{
				return null;
			}

			StringBuilder aKatakana = new StringBuilder();

			for (Int32 i = 0; i < oString.Length; i++)
			{
				Char aChar = oString[i];

				// 小文字・半角カタカナ等を全角カタカナに変換
				Int32 aPos = NORMALIZE_DB_RUBY_FROM.IndexOf(aChar);
				if (aPos >= 0)
				{
					aKatakana.Append(NORMALIZE_DB_RUBY_TO[aPos]);
					continue;
				}

				// 上記以外の全角カタカナ・音引きはそのまま
				if ('ア' <= aChar && aChar <= 'ン' || aChar == 'ー')
				{
					aKatakana.Append(aChar);
					continue;
				}

				// 上記以外のひらがなをカタカナに変換
				if ('あ' <= aChar && aChar <= 'ん')
				{
					aKatakana.Append((Char)(aChar + 0x60));
					continue;
				}

				// その他の文字は無視する
			}

			String aKatakanaString = aKatakana.ToString();
			if (String.IsNullOrEmpty(aKatakanaString))
			{
				return null;
			}

			return aKatakanaString;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースに登録する文字列の表記揺れを減らす
		// 半角チルダ・波ダッシュは全角チルダに変換する（波ダッシュとして全角チルダが用いられているため）
		// ＜返値＞ 正規化後表記 or null（空になる場合）
		// --------------------------------------------------------------------
		public static String NormalizeDbString(String oString)
		{
			Debug.Assert(NORMALIZE_DB_STRING_FROM.Length == NORMALIZE_DB_STRING_TO.Length, "NormalizeDbString() different NORMALIZE_DB_STRING_FROM NORMALIZE_DB_STRING_TO length");

			if (String.IsNullOrEmpty(oString))
			{
				return null;
			}

			StringBuilder aNormalized = new StringBuilder();

			for (Int32 i = 0; i < oString.Length; i++)
			{
				Char aChar = oString[i];

				// 一部記号・全角英数を半角に変換
				if ('！' <= aChar && aChar <= '｝')
				{
					aNormalized.Append((Char)(aChar - 0xFEE0));
					continue;
				}

				// テーブルによる変換
				Int32 aPos = NORMALIZE_DB_STRING_FROM.IndexOf(aChar);
				if (aPos >= 0)
				{
					aNormalized.Append(NORMALIZE_DB_STRING_TO[aPos]);
					continue;
				}

				// 変換なし
				aNormalized.Append(aChar);
			}

			String aNormalizedString = aNormalized.ToString().Trim();
			if (String.IsNullOrEmpty(aNormalizedString))
			{
				return null;
			}

			return aNormalizedString;
		}

		// --------------------------------------------------------------------
		// 空文字列を null に変換する
		// --------------------------------------------------------------------
		public static String NullIfEmpty(String oString)
		{
			if (String.IsNullOrEmpty(oString))
			{
				return null;
			}
			return oString;
		}

		// --------------------------------------------------------------------
		// リスト出力
		// ＜引数＞ oOutputWriter: FolderPath は設定済みの前提
		//          oYukaListerSettings: TFound データベース接続用
		// --------------------------------------------------------------------
		public static void OutputList(OutputWriter oOutputWriter, YukaListerSettings oYukaListerSettings)
		{
			oOutputWriter.LogWriter = LogWriter;
			oOutputWriter.OutputSettings.Load();
			using (DataContext aYukariDbContext = new DataContext(YlCommon.YukariDbInMemoryConnection))
			{
				Table<TFound> aTableFound = aYukariDbContext.GetTable<TFound>();
				oOutputWriter.TableFound = aTableFound;
				oOutputWriter.Output();
			}
		}

		// --------------------------------------------------------------------
		// 編曲者紐付テーブルに新規登録または更新
		// --------------------------------------------------------------------
		public static void RegisterArrangerSequence(DataContext oContext, String oSongId, List<String> oArrangerIds)
		{
			// 新規レコード
			List<TArrangerSequence> aNewArrangerSequences = new List<TArrangerSequence>();
			for (Int32 i = 0; i < oArrangerIds.Count; i++)
			{
				TArrangerSequence aNewArrangerSequence = CreateTArrangerSequence(oSongId, i, oArrangerIds[i]);
				aNewArrangerSequences.Add(aNewArrangerSequence);
			}

			// 既存レコード
			List<TArrangerSequence> aExistArrangerSequences = SelectArrangerSequencesBySongId(oContext, oSongId, true);

			// 既存レコードがある場合は更新
			for (Int32 i = 0; i < Math.Min(aNewArrangerSequences.Count, aExistArrangerSequences.Count); i++)
			{
				if (YlCommon.IsUpdated(aExistArrangerSequences[i], aNewArrangerSequences[i]))
				{
					aNewArrangerSequences[i].UpdateTime = aExistArrangerSequences[i].UpdateTime;
					Common.ShallowCopy(aNewArrangerSequences[i], aExistArrangerSequences[i]);
					LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "編曲者紐付テーブル更新：" + oSongId + " / " + i.ToString());
				}
			}

			// 既存レコードがない部分は新規登録
			Table<TArrangerSequence> aTableArrangerSequence = oContext.GetTable<TArrangerSequence>();
			for (Int32 i = aExistArrangerSequences.Count; i < aNewArrangerSequences.Count; i++)
			{
				aTableArrangerSequence.InsertOnSubmit(aNewArrangerSequences[i]);
				LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "編曲者紐付テーブル新規登録：" + oSongId + " / " + i.ToString());
			}

			// 既存レコードが余る部分は無効化
			for (Int32 i = aNewArrangerSequences.Count; i < aExistArrangerSequences.Count; i++)
			{
				aExistArrangerSequences[i].Invalid = true;
				aExistArrangerSequences[i].Dirty = true;
				LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "編曲者紐付テーブル無効化：" + oSongId + " / " + i.ToString());
			}
		}

		// --------------------------------------------------------------------
		// 歌手紐付テーブルに新規登録または更新
		// --------------------------------------------------------------------
		public static void RegisterArtistSequence(DataContext oContext, String oSongId, List<String> oArtistIds, Boolean oIsImport = false)
		{
			// 新規レコード
			List<TArtistSequence> aNewArtistSequences = new List<TArtistSequence>();
			for (Int32 i = 0; i < oArtistIds.Count; i++)
			{
				TArtistSequence aNewArtistSequence = CreateTArtistSequence(oSongId, i, oArtistIds[i], oIsImport);
				aNewArtistSequences.Add(aNewArtistSequence);
			}

			// 既存レコード
			List<TArtistSequence> aExistArtistSequences = SelectArtistSequencesBySongId(oContext, oSongId, true);

			// 既存レコードがインポートではなく新規レコードがインポートの場合は更新しない
			if (aExistArtistSequences.Count > 0 && !aExistArtistSequences[0].Import
					&& aNewArtistSequences.Count > 0 && aNewArtistSequences[0].Import)
			{
				return;
			}

			// 既存レコードがある場合は更新
			for (Int32 i = 0; i < Math.Min(aNewArtistSequences.Count, aExistArtistSequences.Count); i++)
			{
				if (YlCommon.IsUpdated(aExistArtistSequences[i], aNewArtistSequences[i]))
				{
					aNewArtistSequences[i].UpdateTime = aExistArtistSequences[i].UpdateTime;
					Common.ShallowCopy(aNewArtistSequences[i], aExistArtistSequences[i]);
					if (!oIsImport)
					{
						LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "歌手紐付テーブル更新：" + oSongId + " / " + i.ToString());
					}
				}
			}

			// 既存レコードがない部分は新規登録
			Table<TArtistSequence> aTableArtistSequence = oContext.GetTable<TArtistSequence>();
			for (Int32 i = aExistArtistSequences.Count; i < aNewArtistSequences.Count; i++)
			{
				aTableArtistSequence.InsertOnSubmit(aNewArtistSequences[i]);
				if (!oIsImport)
				{
					LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "歌手紐付テーブル新規登録：" + oSongId + " / " + i.ToString());
				}
			}

			// 既存レコードが余る部分は無効化
			for (Int32 i = aNewArtistSequences.Count; i < aExistArtistSequences.Count; i++)
			{
				aExistArtistSequences[i].Invalid = true;
				aExistArtistSequences[i].Dirty = true;
				if (!oIsImport)
				{
					LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "歌手紐付テーブル無効化：" + oSongId + " / " + i.ToString());
				}
			}
		}

		// --------------------------------------------------------------------
		// 作曲者紐付テーブルに新規登録または更新
		// --------------------------------------------------------------------
		public static void RegisterComposerSequence(DataContext oContext, String oSongId, List<String> oComposerIds)
		{
			// 新規レコード
			List<TComposerSequence> aNewComposerSequences = new List<TComposerSequence>();
			for (Int32 i = 0; i < oComposerIds.Count; i++)
			{
				TComposerSequence aNewComposerSequence = CreateTComposerSequence(oSongId, i, oComposerIds[i]);
				aNewComposerSequences.Add(aNewComposerSequence);
			}

			// 既存レコード
			List<TComposerSequence> aExistComposerSequences = SelectComposerSequencesBySongId(oContext, oSongId, true);

			// 既存レコードがある場合は更新
			for (Int32 i = 0; i < Math.Min(aNewComposerSequences.Count, aExistComposerSequences.Count); i++)
			{
				if (YlCommon.IsUpdated(aExistComposerSequences[i], aNewComposerSequences[i]))
				{
					aNewComposerSequences[i].UpdateTime = aExistComposerSequences[i].UpdateTime;
					Common.ShallowCopy(aNewComposerSequences[i], aExistComposerSequences[i]);
					LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "作曲者紐付テーブル更新：" + oSongId + " / " + i.ToString());
				}
			}

			// 既存レコードがない部分は新規登録
			Table<TComposerSequence> aTableComposerSequence = oContext.GetTable<TComposerSequence>();
			for (Int32 i = aExistComposerSequences.Count; i < aNewComposerSequences.Count; i++)
			{
				aTableComposerSequence.InsertOnSubmit(aNewComposerSequences[i]);
				LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "作曲者紐付テーブル新規登録：" + oSongId + " / " + i.ToString());
			}

			// 既存レコードが余る部分は無効化
			for (Int32 i = aNewComposerSequences.Count; i < aExistComposerSequences.Count; i++)
			{
				aExistComposerSequences[i].Invalid = true;
				aExistComposerSequences[i].Dirty = true;
				LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "作曲者紐付テーブル無効化：" + oSongId + " / " + i.ToString());
			}
		}

		// --------------------------------------------------------------------
		// 作詞者紐付テーブルに新規登録または更新
		// --------------------------------------------------------------------
		public static void RegisterLyristSequence(DataContext oContext, String oSongId, List<String> oLyristIds)
		{
			// 新規レコード
			List<TLyristSequence> aNewLyristSequences = new List<TLyristSequence>();
			for (Int32 i = 0; i < oLyristIds.Count; i++)
			{
				TLyristSequence aNewLyristSequence = CreateTLyristSequence(oSongId, i, oLyristIds[i]);
				aNewLyristSequences.Add(aNewLyristSequence);
			}

			// 既存レコード
			List<TLyristSequence> aExistLyristSequences = SelectLyristSequencesBySongId(oContext, oSongId, true);

			// 既存レコードがある場合は更新
			for (Int32 i = 0; i < Math.Min(aNewLyristSequences.Count, aExistLyristSequences.Count); i++)
			{
				if (YlCommon.IsUpdated(aExistLyristSequences[i], aNewLyristSequences[i]))
				{
					aNewLyristSequences[i].UpdateTime = aExistLyristSequences[i].UpdateTime;
					Common.ShallowCopy(aNewLyristSequences[i], aExistLyristSequences[i]);
					LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "作詞者紐付テーブル更新：" + oSongId + " / " + i.ToString());
				}
			}

			// 既存レコードがない部分は新規登録
			Table<TLyristSequence> aTableLyristSequence = oContext.GetTable<TLyristSequence>();
			for (Int32 i = aExistLyristSequences.Count; i < aNewLyristSequences.Count; i++)
			{
				aTableLyristSequence.InsertOnSubmit(aNewLyristSequences[i]);
				LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "作詞者紐付テーブル新規登録：" + oSongId + " / " + i.ToString());
			}

			// 既存レコードが余る部分は無効化
			for (Int32 i = aNewLyristSequences.Count; i < aExistLyristSequences.Count; i++)
			{
				aExistLyristSequences[i].Invalid = true;
				aExistLyristSequences[i].Dirty = true;
				LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "作詞者紐付テーブル無効化：" + oSongId + " / " + i.ToString());
			}
		}

		// --------------------------------------------------------------------
		// タイアップグループ紐付テーブルに新規登録または更新
		// --------------------------------------------------------------------
		public static void RegisterTieUpGroupSequence(DataContext oContext, String oTieUpId, List<String> oTieUpGroupIds)
		{
			List<TTieUpGroup> aExistTieUpGroups = SelectTieUpGroupsByTieUpId(oContext, oTieUpId, true);
			Table<TTieUpGroupSequence> aTableTieUpGroupSequence = oContext.GetTable<TTieUpGroupSequence>();
			if (aExistTieUpGroups.Count == 0)
			{
				// 新規登録
				for (Int32 i = 0; i < oTieUpGroupIds.Count; i++)
				{
					TTieUpGroupSequence aNewTieUpGroupSequence = CreateTTieUpGroupSequence(oTieUpId, i, oTieUpGroupIds[i]);
					aTableTieUpGroupSequence.InsertOnSubmit(aNewTieUpGroupSequence);
					LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップグループ紐付テーブル新規登録：" + oTieUpId + " / " + i.ToString());
				}
			}
			else
			{
				// 更新判定（oTieUpId に紐付くレコードのどこか一箇所でも違ったら更新する
				Boolean aNeedsUpdate = false;
				if (oTieUpGroupIds.Count != aExistTieUpGroups.Count)
				{
					aNeedsUpdate = true;
				}
				else
				{
					for (Int32 i = 0; i < oTieUpGroupIds.Count; i++)
					{
						TTieUpGroupSequence aNewTieUpGroupSequence = CreateTTieUpGroupSequence(oTieUpId, i, oTieUpGroupIds[i]);
						TTieUpGroupSequence aExistTieUpGroupSequence = SelectTieUpGroupSequenceByPrimaryKey(oContext, oTieUpId, i, true);
						if (IsUpdated(aExistTieUpGroupSequence, aNewTieUpGroupSequence))
						{
							aNeedsUpdate = true;
							break;
						}
					}
				}
				if (aNeedsUpdate)
				{
					// 更新（一度削除してから全レコード登録しなおし）
					IQueryable<TTieUpGroupSequence> aQueryResult =
							from x in aTableTieUpGroupSequence
							where x.TieUpId == oTieUpId
							select x;
					aTableTieUpGroupSequence.DeleteAllOnSubmit(aQueryResult);
					for (Int32 i = 0; i < oTieUpGroupIds.Count; i++)
					{
						TTieUpGroupSequence aNewTieUpGroupSequence = CreateTTieUpGroupSequence(oTieUpId, i, oTieUpGroupIds[i]);
						aTableTieUpGroupSequence.InsertOnSubmit(aNewTieUpGroupSequence);
						LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップグループ紐付テーブル更新：" + oTieUpId + " / " + i.ToString());
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲に紐付く編曲者を検索
		// oIncludesInvalid が true の場合、無効 ID に null を紐付ける
		// --------------------------------------------------------------------
		public static List<TPerson> SelectArrangersBySongId(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false)
		{
			List<TPerson> aArrangers = new List<TPerson>();

			Table<TArrangerSequence> aTable = oContext.GetTable<TArrangerSequence>();
			IQueryable<TArrangerSequence> aQueryResult =
					from x in aTable
					where x.SongId == oSongId && (oIncludesInvalid ? true : x.Invalid == false)
					select x;
			foreach (TArrangerSequence aArrangerSequence in aQueryResult)
			{
				TPerson aPerson = SelectPersonById(oContext, aArrangerSequence.PersonId);
				if (aPerson != null || oIncludesInvalid)
				{
					aArrangers.Add(aPerson);
				}
			}

			return aArrangers;
		}

		// --------------------------------------------------------------------
		// 編曲者紐付データベースから紐付を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TArrangerSequence SelectArrangerSequenceByPrimaryKey(DataContext oContext, String oSongId, Int32 oSequence, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oSongId))
			{
				return null;
			}

			Table<TArrangerSequence> aTableArrangerSequence = oContext.GetTable<TArrangerSequence>();
			return aTableArrangerSequence.SingleOrDefault(x => x.SongId == oSongId && x.Sequence == oSequence && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 歌手紐付データベースから紐付を検索
		// --------------------------------------------------------------------
		public static List<TArrangerSequence> SelectArrangerSequencesBySongId(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false)
		{
			List<TArrangerSequence> aArrangerSequences = new List<TArrangerSequence>();

			if (!String.IsNullOrEmpty(oSongId))
			{
				Table<TArrangerSequence> aTableArrangerSequence = oContext.GetTable<TArrangerSequence>();
				IQueryable<TArrangerSequence> aQueryResult =
						from x in aTableArrangerSequence
						where x.SongId == oSongId && (oIncludesInvalid ? true : x.Invalid == false)
						orderby x.Sequence
						select x;
				foreach (TArrangerSequence aArrangerSequence in aQueryResult)
				{
					aArrangerSequences.Add(aArrangerSequence);
				}
			}

			return aArrangerSequences;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲に紐付く歌手を検索
		// oIncludesInvalid が true の場合、無効 ID に null を紐付ける
		// --------------------------------------------------------------------
		public static List<TPerson> SelectArtistsBySongId(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false)
		{
			List<TPerson> aArtists = new List<TPerson>();

			Table<TArtistSequence> aTable = oContext.GetTable<TArtistSequence>();
			IQueryable<TArtistSequence> aQueryResult =
					from x in aTable
					where x.SongId == oSongId && (oIncludesInvalid ? true : x.Invalid == false)
					select x;
			foreach (TArtistSequence aArtistSequence in aQueryResult)
			{
				TPerson aPerson = SelectPersonById(oContext, aArtistSequence.PersonId);
				if (aPerson != null || oIncludesInvalid)
				{
					aArtists.Add(aPerson);
				}
			}

			return aArtists;
		}

		// --------------------------------------------------------------------
		// 歌手紐付データベースから紐付を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TArtistSequence SelectArtistSequenceByPrimaryKey(DataContext oContext, String oSongId, Int32 oSequence, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oSongId))
			{
				return null;
			}

			Table<TArtistSequence> aTableArtistSequence = oContext.GetTable<TArtistSequence>();
			return aTableArtistSequence.SingleOrDefault(x => x.SongId == oSongId && x.Sequence == oSequence && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 歌手紐付データベースから紐付を検索
		// --------------------------------------------------------------------
		public static List<TArtistSequence> SelectArtistSequencesBySongId(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false)
		{
			List<TArtistSequence> aArtistSequences = new List<TArtistSequence>();

			if (!String.IsNullOrEmpty(oSongId))
			{
				Table<TArtistSequence> aTableArtistSequence = oContext.GetTable<TArtistSequence>();
				IQueryable<TArtistSequence> aQueryResult =
						from x in aTableArtistSequence
						where x.SongId == oSongId && (oIncludesInvalid ? true : x.Invalid == false)
						orderby x.Sequence
						select x;
				foreach (TArtistSequence aArtistSequence in aQueryResult)
				{
					aArtistSequences.Add(aArtistSequence);
				}
			}

			return aArtistSequences;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからカテゴリーを検索
		// --------------------------------------------------------------------
		public static List<TCategory> SelectCategoriesByName(SQLiteConnection oConnection, String oName, Boolean oIncludesInvalid = false)
		{
			List<TCategory> aCategories = new List<TCategory>();

			if (!String.IsNullOrEmpty(oName))
			{
				using (DataContext aContext = new DataContext(oConnection))
				{
					Table<TCategory> aTableCategory = aContext.GetTable<TCategory>();
					IQueryable<TCategory> aQueryResult =
							from x in aTableCategory
							where x.Name == oName && (oIncludesInvalid ? true : x.Invalid == false)
							select x;
					foreach (TCategory aCategory in aQueryResult)
					{
						aCategories.Add(aCategory);
					}
				}
			}
			return aCategories;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからカテゴリーを検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TCategory SelectCategoryById(DataContext oContext, String oId, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oId))
			{
				return null;
			}

			Table<TCategory> aTableCategory = oContext.GetTable<TCategory>();
			return aTableCategory.SingleOrDefault(x => x.Id == oId && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからカテゴリーを検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TCategory SelectCategoryById(SQLiteConnection oConnection, String oId, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectCategoryById(aContext, oId, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからカテゴリーを列挙
		// --------------------------------------------------------------------
		public static List<String> SelectCategoryNames(SQLiteConnection oConnection, Boolean oIncludesInvalid = false)
		{
			List<String> aCategoryNames = new List<String>();
			using (DataContext aContext = new DataContext(oConnection))
			{
				Table<TCategory> aTableCategory = aContext.GetTable<TCategory>();
				IQueryable<TCategory> aQueryResultCategory =
						from x in aTableCategory
						where oIncludesInvalid ? true : x.Invalid == false
						select x;
				foreach (TCategory aCategory in aQueryResultCategory)
				{
					aCategoryNames.Add(aCategory.Name);
				}
			}
			return aCategoryNames;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲に紐付く作曲者を検索
		// oIncludesInvalid が true の場合、無効 ID に null を紐付ける
		// --------------------------------------------------------------------
		public static List<TPerson> SelectComposersBySongId(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false)
		{
			List<TPerson> aComposerss = new List<TPerson>();

			Table<TComposerSequence> aTable = oContext.GetTable<TComposerSequence>();
			IQueryable<TComposerSequence> aQueryResult =
					from x in aTable
					where x.SongId == oSongId && (oIncludesInvalid ? true : x.Invalid == false)
					select x;
			foreach (TComposerSequence aComposerSequence in aQueryResult)
			{
				TPerson aPerson = SelectPersonById(oContext, aComposerSequence.PersonId);
				if (aPerson != null || oIncludesInvalid)
				{
					aComposerss.Add(aPerson);
				}
			}

			return aComposerss;
		}

		// --------------------------------------------------------------------
		// 作曲者紐付データベースから紐付を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TComposerSequence SelectComposerSequenceByPrimaryKey(DataContext oContext, String oSongId, Int32 oSequence, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oSongId))
			{
				return null;
			}

			Table<TComposerSequence> aTableComposerSequence = oContext.GetTable<TComposerSequence>();
			return aTableComposerSequence.SingleOrDefault(x => x.SongId == oSongId && x.Sequence == oSequence && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 歌手紐付データベースから紐付を検索
		// --------------------------------------------------------------------
		public static List<TComposerSequence> SelectComposerSequencesBySongId(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false)
		{
			List<TComposerSequence> aComposerSequences = new List<TComposerSequence>();

			if (!String.IsNullOrEmpty(oSongId))
			{
				Table<TComposerSequence> aTableComposerSequence = oContext.GetTable<TComposerSequence>();
				IQueryable<TComposerSequence> aQueryResult =
						from x in aTableComposerSequence
						where x.SongId == oSongId && (oIncludesInvalid ? true : x.Invalid == false)
						orderby x.Sequence
						select x;
				foreach (TComposerSequence aComposerSequence in aQueryResult)
				{
					aComposerSequences.Add(aComposerSequence);
				}
			}

			return aComposerSequences;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲に紐付く作詞者を検索
		// oIncludesInvalid が true の場合、無効 ID に null を紐付ける
		// --------------------------------------------------------------------
		public static List<TPerson> SelectLyristsBySongId(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false)
		{
			List<TPerson> aLyrists = new List<TPerson>();

			Table<TLyristSequence> aTable = oContext.GetTable<TLyristSequence>();
			IQueryable<TLyristSequence> aQueryResult =
					from x in aTable
					where x.SongId == oSongId && (oIncludesInvalid ? true : x.Invalid == false)
					select x;
			foreach (TLyristSequence aLyristSequence in aQueryResult)
			{
				TPerson aPerson = SelectPersonById(oContext, aLyristSequence.PersonId);
				if (aPerson != null || oIncludesInvalid)
				{
					aLyrists.Add(aPerson);
				}
			}

			return aLyrists;
		}

		// --------------------------------------------------------------------
		// 作詞者紐付データベースから紐付を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TLyristSequence SelectLyristSequenceByPrimaryKey(DataContext oContext, String oSongId, Int32 oSequence, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oSongId))
			{
				return null;
			}

			Table<TLyristSequence> aTableLyristSequence = oContext.GetTable<TLyristSequence>();
			return aTableLyristSequence.SingleOrDefault(x => x.SongId == oSongId && x.Sequence == oSequence && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 歌手紐付データベースから紐付を検索
		// --------------------------------------------------------------------
		public static List<TLyristSequence> SelectLyristSequencesBySongId(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false)
		{
			List<TLyristSequence> aLyristSequences = new List<TLyristSequence>();

			if (!String.IsNullOrEmpty(oSongId))
			{
				Table<TLyristSequence> aTableLyristSequence = oContext.GetTable<TLyristSequence>();
				IQueryable<TLyristSequence> aQueryResult =
						from x in aTableLyristSequence
						where x.SongId == oSongId && (oIncludesInvalid ? true : x.Invalid == false)
						orderby x.Sequence
						select x;
				foreach (TLyristSequence aLyristSequence in aQueryResult)
				{
					aLyristSequences.Add(aLyristSequence);
				}
			}

			return aLyristSequences;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから制作会社を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TMaker SelectMakerById(DataContext oContext, String oId, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oId))
			{
				return null;
			}

			Table<TMaker> aTableMaker = oContext.GetTable<TMaker>();
			return aTableMaker.SingleOrDefault(x => x.Id == oId && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから制作会社を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TMaker SelectMakerById(SQLiteConnection oConnection, String oId, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectMakerById(aContext, oId, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから制作会社を検索
		// --------------------------------------------------------------------
		public static List<TMaker> SelectMakersByName(SQLiteConnection oConnection, String oName, Boolean oIncludesInvalid = false)
		{
			List<TMaker> aMakers = new List<TMaker>();

			if (!String.IsNullOrEmpty(oName))
			{
				using (DataContext aContext = new DataContext(oConnection))
				{
					Table<TMaker> aTableMaker = aContext.GetTable<TMaker>();
					IQueryable<TMaker> aQueryResult =
							from x in aTableMaker
							where x.Name == oName && (oIncludesInvalid ? true : x.Invalid == false)
							select x;
					foreach (TMaker aMaker in aQueryResult)
					{
						aMakers.Add(aMaker);
					}
				}
			}
			return aMakers;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから人を検索
		// --------------------------------------------------------------------
		public static List<TPerson> SelectPeopleByName(DataContext oContext, String oName, Boolean oIncludesInvalid = false)
		{
			List<TPerson> aPeople = new List<TPerson>();

			if (!String.IsNullOrEmpty(oName))
			{
				Table<TPerson> aTablePerson = oContext.GetTable<TPerson>();
				IQueryable<TPerson> aQueryResult =
						from x in aTablePerson
						where x.Name == oName && (oIncludesInvalid ? true : x.Invalid == false)
						select x;
				foreach (TPerson aPerson in aQueryResult)
				{
					aPeople.Add(aPerson);
				}
			}
			return aPeople;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから人を検索
		// --------------------------------------------------------------------
		public static List<TPerson> SelectPeopleByName(SQLiteConnection oConnection, String oName, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectPeopleByName(aContext, oName, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから人物を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TPerson SelectPersonById(DataContext oContext, String oId, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oId))
			{
				return null;
			}

			Table<TPerson> aTablePerson = oContext.GetTable<TPerson>();
			return aTablePerson.SingleOrDefault(x => x.Id == oId && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから人物を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TPerson SelectPersonById(SQLiteConnection oConnection, String oId, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectPersonById(aContext, oId, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲別名を検索
		// --------------------------------------------------------------------
		public static List<TSongAlias> SelectSongAliasesByAlias(DataContext oContext, String oAlias, Boolean oIncludesInvalid = false)
		{
			List<TSongAlias> aSongAliases = new List<TSongAlias>();

			if (!String.IsNullOrEmpty(oAlias))
			{
				Table<TSongAlias> aTableSongAlias = oContext.GetTable<TSongAlias>();
				IQueryable<TSongAlias> aQueryResult =
						from x in aTableSongAlias
						where x.Alias == oAlias && (oIncludesInvalid ? true : x.Invalid == false)
						select x;
				foreach (TSongAlias aSongAlias in aQueryResult)
				{
					aSongAliases.Add(aSongAlias);
				}
			}

			return aSongAliases;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲別名を検索
		// --------------------------------------------------------------------
		public static List<TSongAlias> SelectSongAliasesByAlias(SQLiteConnection oConnection, String oAlias, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectSongAliasesByAlias(aContext, oAlias, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TSong SelectSongById(DataContext oContext, String oId, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oId))
			{
				return null;
			}

			Table<TSong> aTableSong = oContext.GetTable<TSong>();
			return aTableSong.SingleOrDefault(x => x.Id == oId && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TSong SelectSongById(SQLiteConnection oConnection, String oId, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectSongById(aContext, oId, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲を検索
		// --------------------------------------------------------------------
		public static List<TSong> SelectSongsByName(DataContext oContext, String oName, Boolean oIncludesInvalid = false)
		{
			List<TSong> aSongs = new List<TSong>();

			if (!String.IsNullOrEmpty(oName))
			{
				Table<TSong> aTableSong = oContext.GetTable<TSong>();
				IQueryable<TSong> aQueryResult =
						from x in aTableSong
						where x.Name == oName && (oIncludesInvalid ? true : x.Invalid == false)
						select x;
				foreach (TSong aSong in aQueryResult)
				{
					aSongs.Add(aSong);
				}
			}
			return aSongs;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲を検索
		// --------------------------------------------------------------------
		public static List<TSong> SelectSongsByName(SQLiteConnection oConnection, String oName, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectSongsByName(aContext, oName, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップ別名を検索
		// --------------------------------------------------------------------
		public static List<TTieUpAlias> SelectTieUpAliasesByAlias(DataContext oContext, String oAlias, Boolean oIncludesInvalid = false)
		{
			List<TTieUpAlias> aTieUpAliases = new List<TTieUpAlias>();

			if (!String.IsNullOrEmpty(oAlias))
			{
				Table<TTieUpAlias> aTableTieUpAlias = oContext.GetTable<TTieUpAlias>();
				IQueryable<TTieUpAlias> aQueryResult =
						from x in aTableTieUpAlias
						where x.Alias == oAlias && (oIncludesInvalid ? true : x.Invalid == false)
						select x;
				foreach (TTieUpAlias aTieUpAlias in aQueryResult)
				{
					aTieUpAliases.Add(aTieUpAlias);
				}
			}
			return aTieUpAliases;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップ別名を検索
		// --------------------------------------------------------------------
		public static List<TTieUpAlias> SelectTieUpAliasesByAlias(SQLiteConnection oConnection, String oAlias, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectTieUpAliasesByAlias(aContext, oAlias, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップを検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TTieUp SelectTieUpById(DataContext oContext, String oId, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oId))
			{
				return null;
			}

			Table<TTieUp> aTableTieUp = oContext.GetTable<TTieUp>();
			return aTableTieUp.SingleOrDefault(x => x.Id == oId && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップを検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TTieUp SelectTieUpById(SQLiteConnection oConnection, String oId, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectTieUpById(aContext, oId, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップを検索
		// --------------------------------------------------------------------
		public static List<TTieUp> SelectTieUpsByName(DataContext oContext, String oName, Boolean oIncludesInvalid = false)
		{
			List<TTieUp> aTieUps = new List<TTieUp>();

			if (!String.IsNullOrEmpty(oName))
			{
				Table<TTieUp> aTableTieUp = oContext.GetTable<TTieUp>();
				IQueryable<TTieUp> aQueryResult =
						from x in aTableTieUp
						where x.Name == oName && (oIncludesInvalid ? true : x.Invalid == false)
						select x;
				foreach (TTieUp aTieUp in aQueryResult)
				{
					aTieUps.Add(aTieUp);
				}
			}
			return aTieUps;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップを検索
		// --------------------------------------------------------------------
		public static List<TTieUp> SelectTieUpsByName(SQLiteConnection oConnection, String oName, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectTieUpsByName(aContext, oName, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップグループを検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TTieUpGroup SelectTieUpGroupById(DataContext oContext, String oId, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oId))
			{
				return null;
			}

			Table<TTieUpGroup> aTableTieUpGroup = oContext.GetTable<TTieUpGroup>();
			return aTableTieUpGroup.SingleOrDefault(x => x.Id == oId && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップグループを検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TTieUpGroup SelectTieUpGroupById(SQLiteConnection oConnection, String oId, Boolean oIncludesInvalid = false)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectTieUpGroupById(aContext, oId, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップグループを検索
		// --------------------------------------------------------------------
		public static List<TTieUpGroup> SelectTieUpGroupsByName(SQLiteConnection oConnection, String oName, Boolean oIncludesInvalid = false)
		{
			List<TTieUpGroup> aTieUpGroups = new List<TTieUpGroup>();

			if (!String.IsNullOrEmpty(oName))
			{
				using (DataContext aContext = new DataContext(oConnection))
				{
					Table<TTieUpGroup> aTableTieUpGroup = aContext.GetTable<TTieUpGroup>();
					IQueryable<TTieUpGroup> aQueryResult =
							from x in aTableTieUpGroup
							where x.Name == oName && (oIncludesInvalid ? true : x.Invalid == false)
							select x;
					foreach (TTieUpGroup aTieUpGroup in aQueryResult)
					{
						aTieUpGroups.Add(aTieUpGroup);
					}
				}
			}
			return aTieUpGroups;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップに紐付くタイアップグループを検索
		// --------------------------------------------------------------------
		public static List<TTieUpGroup> SelectTieUpGroupsByTieUpId(DataContext oContext, String oTieUpId, Boolean oIncludesInvalid = false)
		{
			List<TTieUpGroup> aTieUpGroups = new List<TTieUpGroup>();

			Table<TTieUpGroupSequence> aTable = oContext.GetTable<TTieUpGroupSequence>();
			IQueryable<TTieUpGroupSequence> aQueryResult =
					from x in aTable
					where x.TieUpId == oTieUpId && (oIncludesInvalid ? true : x.Invalid == false)
					select x;
			foreach (TTieUpGroupSequence aTieUpGroupSequence in aQueryResult)
			{
				TTieUpGroup aTieUpGroup = SelectTieUpGroupById(oContext, aTieUpGroupSequence.TieUpGroupId);
				if (aTieUpGroup != null)
				{
					aTieUpGroups.Add(aTieUpGroup);
				}
			}

			return aTieUpGroups;
		}

		// --------------------------------------------------------------------
		// タイアップグループ紐付データベースから紐付を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static TTieUpGroupSequence SelectTieUpGroupSequenceByPrimaryKey(DataContext oContext, String oTieUpId, Int32 oSequence, Boolean oIncludesInvalid = false)
		{
			if (String.IsNullOrEmpty(oTieUpId))
			{
				return null;
			}

			Table<TTieUpGroupSequence> aTableTieUpGroupSequence = oContext.GetTable<TTieUpGroupSequence>();
			return aTableTieUpGroupSequence.SingleOrDefault(x => x.TieUpId == oTieUpId && x.Sequence == oSequence && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// カテゴリーメニューに値を設定
		// --------------------------------------------------------------------
		public static void SetContextMenuItemCategories(FrameworkElement oElement, RoutedEventHandler oClick)
		{
			using (SQLiteConnection aConnection = CreateMusicInfoDbConnection())
			{
				using (DataContext aContext = new DataContext(aConnection))
				{
					Table<TCategory> aTableCategory = aContext.GetTable<TCategory>();
					IQueryable<TCategory> aQueryResult =
							from x in aTableCategory
							select x;
					foreach (TCategory aCategory in aQueryResult)
					{
						AddContextMenuItem(oElement, aCategory.Name, oClick);
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// ステータスバーにメッセージを表示
		// --------------------------------------------------------------------
		public static void SetStatusLabelMessage(Label oStatusLabel, TraceEventType oTraceEventType, String oMsg)
		{
			oStatusLabel.Content = oMsg;
			if (oTraceEventType == TraceEventType.Error)
			{
				oStatusLabel.Foreground = new SolidColorBrush(Colors.Red);
			}
			else
			{
				oStatusLabel.Foreground = new SolidColorBrush(Colors.Black);
			}
		}

		// --------------------------------------------------------------------
		// 設定保存フォルダのパス（末尾 '\\'）
		// 存在しない場合は作成する
		// --------------------------------------------------------------------
		public static String SettingsPath()
		{
			String aPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify)
					+ "\\" + Common.FOLDER_NAME_SHINTA + YlCommon.FOLDER_NAME_YUKA_LISTER;

			if (!Directory.Exists(aPath))
			{
				Directory.CreateDirectory(aPath);
			}
			return aPath;
		}

		// --------------------------------------------------------------------
		// extended-length でないパス表記に戻す
		// EXTENDED_LENGTH_PATH_PREFIX を除去するだけなので、長さは MAX_PATH を超えることもありえる
		// --------------------------------------------------------------------
		public static String ShortenPath(String oPath)
		{
			if (!oPath.StartsWith(EXTENDED_LENGTH_PATH_PREFIX))
			{
				return oPath;
			}

			return oPath.Substring(EXTENDED_LENGTH_PATH_PREFIX.Length);
		}

		// --------------------------------------------------------------------
		// ヘルプの表示
		// --------------------------------------------------------------------
		public static void ShowHelp(String oAnchor = null)
		{
			String aHelpPath = null;

			try
			{
				String aHelpPathBase = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";

				// アンカーが指定されている場合は状況依存型ヘルプを表示
				if (!String.IsNullOrEmpty(oAnchor))
				{
					aHelpPath = aHelpPathBase + FOLDER_NAME_HELP_PARTS + FILE_NAME_HELP_PREFIX + "_" + oAnchor + Common.FILE_EXT_HTML;
					try
					{
						Process.Start(aHelpPath);
						return;
					}
					catch (Exception oExcep)
					{
						LogWriter.ShowLogMessage(TraceEventType.Error, "状況に応じたヘルプを表示できませんでした：\n" + oExcep.Message + "\n" + aHelpPath
								+ "\n通常のヘルプを表示します。");
					}
				}

				// アンカーが指定されていない場合・状況依存型ヘルプを表示できなかった場合は通常のヘルプを表示
				aHelpPath = aHelpPathBase + FILE_NAME_HELP_PREFIX + Common.FILE_EXT_HTML;
				Process.Start(aHelpPath);
			}
			catch (Exception oExcep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "ヘルプを表示できませんでした。\n" + oExcep.Message + "\n" + aHelpPath);
			}
		}

		// --------------------------------------------------------------------
		// カンマ区切り ID をリストに分割
		// 引数が空の場合は null ではなく空リストを返す
		// --------------------------------------------------------------------
		public static List<String> SplitIds(String oIds)
		{
			List<String> aSplit = new List<String>();
			if (!String.IsNullOrEmpty(oIds))
			{
				aSplit.AddRange(oIds.Split(','));
			}
			return aSplit;
		}

		// --------------------------------------------------------------------
		// テンポラリファイルのパス（呼びだす度に異なるファイル、拡張子なし）
		// --------------------------------------------------------------------
		public static String TempFilePath()
		{
			// マルチスレッドでも安全にインクリメント
			Int32 aCounter = Interlocked.Increment(ref smTempFilePathCounter);
			return TempPath() + aCounter.ToString() + "_" + Thread.CurrentThread.ManagedThreadId.ToString();
		}

		// --------------------------------------------------------------------
		// テンポラリフォルダのパス（末尾 '\\'）
		// 存在しない場合は作成する
		// --------------------------------------------------------------------
		public static String TempPath()
		{
			String aPath = Path.GetTempPath() + FOLDER_NAME_YUKA_LISTER + Process.GetCurrentProcess().Id.ToString() + "\\";
			if (!Directory.Exists(aPath))
			{
				try
				{
					Directory.CreateDirectory(aPath);
				}
				catch
				{
				}
			}
			return aPath;
		}

		// --------------------------------------------------------------------
		// 年月日のテキストボックスから日付を生成
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public static Double TextBoxToMjd(String oCaption, TextBox oTextBoxYear, TextBox oTextBoxMonth, TextBox oTextBoxDay)
		{
			if (String.IsNullOrEmpty(oTextBoxYear.Text))
			{
				// 年が入力されていない場合は、月日も空欄でなければならない
				if (!String.IsNullOrEmpty(oTextBoxMonth.Text) || !String.IsNullOrEmpty(oTextBoxDay.Text))
				{
					throw new Exception(oCaption + "の年が入力されていません。");
				}

				return INVALID_MJD;
			}

			// 年の確認
			Int32 aYear = Common.StringToInt32(oTextBoxYear.Text);
			Int32 aNowYear = DateTime.Now.Year;
			if (aYear < 0)
			{
				throw new Exception(oCaption + "の年にマイナスの値を入力することはできません。");
			}
			if (aYear < 100)
			{
				// 2 桁の西暦を 4 桁に変換する
				if (aYear <= aNowYear % 100)
				{
					aYear += (aNowYear / 100) * 100;
				}
				else
				{
					aYear += (aNowYear / 100 - 1) * 100;
				}
			}
			if (aYear < 1000)
			{
				throw new Exception(oCaption + "の年に 3 桁の値を入力することはできません。");
			}
			if (aYear < INVALID_YEAR)
			{
				throw new Exception(oCaption + "の年は " + INVALID_YEAR + " 以上を入力して下さい。");
			}
			if (aYear > aNowYear)
			{
				throw new Exception(oCaption + "の年は " + aNowYear + " 以下を入力して下さい。");
			}

			// 月の確認
			if (String.IsNullOrEmpty(oTextBoxMonth.Text) && !String.IsNullOrEmpty(oTextBoxDay.Text))
			{
				// 年と日が入力されている場合は、月も入力されていなければならない
				throw new Exception(oCaption + "の月が入力されていません。");
			}
			Int32 aMonth;
			if (String.IsNullOrEmpty(oTextBoxMonth.Text))
			{
				// 月が空欄の場合は 1 とする
				aMonth = 1;
			}
			else
			{
				aMonth = Common.StringToInt32(oTextBoxMonth.Text);
				if (aMonth < 1 || aMonth > 12)
				{
					throw new Exception(oCaption + "の月は 1～12 を入力して下さい。");
				}
			}

			// 日の確認
			Int32 aDay;
			if (String.IsNullOrEmpty(oTextBoxDay.Text))
			{
				// 日が空欄の場合は 1 とする
				aDay = 1;
			}
			else
			{
				aDay = Common.StringToInt32(oTextBoxDay.Text);
				if (aDay < 1 || aDay > 31)
				{
					throw new Exception(oCaption + "の日は 1～31 を入力して下さい。");
				}
			}

			return JulianDay.DateTimeToModifiedJulianDate(new DateTime(aYear, aMonth, aDay));
		}

		// --------------------------------------------------------------------
		// 同名のタイアップがある場合でも見分けがつく名前を返す
		// --------------------------------------------------------------------
		public static String TieUpNameAvoidingSameName(SQLiteConnection oConnection, TTieUp oTieUp)
		{
			List<TTieUp> aTieUps = YlCommon.SelectTieUpsByName(oConnection, oTieUp.Name);
			if (aTieUps.Count <= 1)
			{
				// 同名のタイアップが無い場合はタイアップ名のみ
				return oTieUp.Name;
			}
			else
			{
				// 同名タイアップが複数ある場合は見分けやすいようにする
				TCategory aCategory = YlCommon.SelectCategoryById(oConnection, oTieUp.CategoryId);
				String aCategoryName = null;
				if (aCategory != null)
				{
					aCategoryName = aCategory.Name;
				}
				return oTieUp.Name + "（" + (String.IsNullOrEmpty(aCategoryName) ? "カテゴリー無し" : aCategoryName) + ", "
						+ (String.IsNullOrEmpty(oTieUp.Keyword) ? "キーワード無し" : oTieUp.Keyword) + "）";
			}
		}

		// --------------------------------------------------------------------
		// データベースのプロパティーを更新
		// --------------------------------------------------------------------
		public static void UpdateDbProperty(SQLiteConnection oConnection)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				Table<TProperty> aTableProperty = aContext.GetTable<TProperty>();

				// 古いプロパティーを削除
				IQueryable<TProperty> aDelTargets =
						from x in aTableProperty
						select x;
				aTableProperty.DeleteAllOnSubmit(aDelTargets);
				aContext.SubmitChanges();

				// 新しいプロパティーを挿入
				aTableProperty.InsertOnSubmit(new TProperty { AppId = APP_ID, AppVer = APP_GENERATION + "," + APP_VER });
				aContext.SubmitChanges();
			}
		}

		// --------------------------------------------------------------------
		// ルビの一部が削除されたら警告
		// ＜例外＞ OperationCanceledException
		// --------------------------------------------------------------------
		public static void WarnRubyDeletedIfNeeded(String oOriginalRuby, String oNormalizedRuby)
		{
			if (!String.IsNullOrEmpty(oOriginalRuby)
					&& (String.IsNullOrEmpty(oNormalizedRuby) || oOriginalRuby.Length != oNormalizedRuby.Length))
			{
				if (MessageBox.Show("フリガナはカタカナのみ登録可能のため、カタカナ以外は削除されます。\n"
						+ oOriginalRuby + " → " + oNormalizedRuby + "\nよろしいですか？", "確認",
						MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
				{
					throw new OperationCanceledException();
				}
			}
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// --------------------------------------------------------------------
		// ファイル名
		// --------------------------------------------------------------------
		private const String FILE_NAME_HELP_PREFIX = APP_ID + "_JPN";
		private const String FOLDER_NAME_HELP_PARTS = "HelpParts\\";

		// --------------------------------------------------------------------
		// DB 変換
		// --------------------------------------------------------------------

		// NormalizeDbRuby() 用：フリガナ正規化対象文字（小文字・濁点のカナ等）
		private const String NORMALIZE_DB_RUBY_FROM = "ァィゥェォッャュョヮヵヶガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポヰヱヴヷヸヹヺｧｨｩｪｫｯｬｭｮ"
				+ "ぁぃぅぇぉっゃゅょゎゕゖがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽゐゑゔ" + NORMALIZE_DB_FORBIDDEN_FROM;
		private const String NORMALIZE_DB_RUBY_TO = "アイウエオツヤユヨワカケカキクケコサシスセソタチツテトハヒフヘホハヒフヘホイエウワイエヲアイウエオツヤユヨ"
				+ "アイウエオツヤユヨワカケカキクケコサシスセソタチツテトハヒフヘホハヒフヘホイエウ" + NORMALIZE_DB_FORBIDDEN_TO;

		// NormalizeDbString() 用：禁則文字（全角スペース、一部の半角文字等）
		private const String NORMALIZE_DB_STRING_FROM = "　\u2019ｧｨｩｪｫｯｬｭｮﾞﾟ｡｢｣､･~\u301C" + NORMALIZE_DB_FORBIDDEN_FROM;
		private const String NORMALIZE_DB_STRING_TO = " 'ァィゥェォッャュョ゛゜。「」、・～～" + NORMALIZE_DB_FORBIDDEN_TO;

		// NormalizeDbXXX() 用：変換後がフリガナ対象の禁則文字（半角カタカナ）
		private const String NORMALIZE_DB_FORBIDDEN_FROM = "ｦｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝ";
		private const String NORMALIZE_DB_FORBIDDEN_TO = "ヲーアイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワン";

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------

		// ID 接頭辞の最大長（同期サーバーデータベースの都合上、ID のトータル長が UTF-8 で 255 バイト以下になるようにする）
		private const Int32 ID_PREFIX_MAX_LENGTH = 20;

		// 暗号化キー（256 bit = 32 byte）
		private static readonly Byte[] ENCRYPT_KEY =
		{
			0x07, 0xC1, 0x19, 0x4A, 0x99, 0x9A, 0xF0, 0x2D, 0x0C, 0x52, 0xB0, 0x65, 0x48, 0xE6, 0x1F, 0x61,
			0x9C, 0x37, 0x9C, 0xA1, 0xC2, 0x31, 0xBA, 0xD1, 0x64, 0x1D, 0x85, 0x46, 0xCA, 0xF4, 0xE6, 0x5F,
		};

		// 暗号化 IV（128 bit = 16 byte）
		private static readonly Byte[] ENCRYPT_IV =
		{
			0x80, 0xB5, 0x40, 0x56, 0x9A, 0xE0, 0x3A, 0x9F, 0xd0, 0x90, 0xC6, 0x7C, 0xAA, 0xCD, 0xE7, 0x53,
		};

		// 頭文字変換用
		private const String HEAD_CONVERT_FROM = "ぁぃぅぇぉゕゖゃゅょゎゔがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽゐゑ";
		private const String HEAD_CONVERT_TO = "あいうえおかけやゆよわうかきくけこさしすせそたちつてとはひふへほはひふへほいえ";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// TempFilePath() 用カウンター（同じスレッドでもファイル名が分かれるようにするため）
		private static Int32 smTempFilePathCounter = 0;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// DB の中にテーブルを作成（汎用関数）
		// --------------------------------------------------------------------
		private static void CreateMusicInfoDbTable(SQLiteCommand oCmd, Type oTypeOfTable, String oIndexColumn = null)
		{
			List<String> aIndices;
			if (String.IsNullOrEmpty(oIndexColumn))
			{
				aIndices = null;
			}
			else
			{
				aIndices = new List<String>();
				aIndices.Add(oIndexColumn);
			}
			CreateMusicInfoDbTable(oCmd, oTypeOfTable, aIndices);
		}

		// --------------------------------------------------------------------
		// DB の中にテーブルを作成（汎用関数）
		// --------------------------------------------------------------------
		private static void CreateMusicInfoDbTable(SQLiteCommand oCmd, Type oTypeOfTable, List<String> oIndices)
		{
			// テーブル作成
			LinqUtils.CreateTable(oCmd, oTypeOfTable);

			// インデックス作成（JOIN および検索の高速化）
			LinqUtils.CreateIndex(oCmd, LinqUtils.TableName(oTypeOfTable), oIndices);
		}

		// --------------------------------------------------------------------
		// カテゴリーマスターテーブルの既定レコードを挿入
		// ニコニコ動画のカテゴリータグおよび anison.info のカテゴリーから主要な物を抽出
		// --------------------------------------------------------------------
		private static void InsertMusicInfoDbCategoryDefaultRecords(SQLiteConnection oConnection)
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				Table<TCategory> aTableCategory = aContext.GetTable<TCategory>();

				// 主にタイアップ用
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(1, "アニメ"));
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(2, "イベント/舞台/公演", "イベントブタイコウエン"));
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(3, "ゲーム"));
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(4, "時代劇", "ジダイゲキ"));
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(5, "特撮", "トクサツ"));
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(6, "ドラマ"));
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(7, "ラジオ"));

				// 主にタイアップの無い楽曲用
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(101, "VOCALOID", "ボーカロイド"));
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(102, "歌ってみた", "ウタッテミタ"));
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(103, "一般", "イッパン"));

				aContext.SubmitChanges();
			}
		}

		// --------------------------------------------------------------------
		// 設定ファイルのルール表記を正規表現に変換
		// --------------------------------------------------------------------
		private static void MakeRegexPattern(String oRuleInDisk, out String oRuleInMemory, out List<String> oGroups)
		{
			oGroups = new List<String>();

			// 元が空なら空で返す
			if (String.IsNullOrEmpty(oRuleInDisk))
			{
				oRuleInMemory = String.Empty;
				return;
			}

			StringBuilder aSB = new StringBuilder();
			aSB.Append("^");
			Int32 aBeginPos = 0;
			Int32 aEndPos;
			Boolean aLongestExists = false;
			while (aBeginPos < oRuleInDisk.Length)
			{
				if (oRuleInDisk[aBeginPos] == RULE_VAR_BEGIN[0])
				{
					// 変数を解析
					aEndPos = MakeRegexPatternFindVarEnd(oRuleInDisk, aBeginPos + 1);
					if (aEndPos < 0)
					{
						throw new Exception("命名規則の " + (aBeginPos + 1) + " 文字目の < に対応する > がありません。\n" + oRuleInDisk);
					}

					// 変数の <> は取り除く
					String aVarName = oRuleInDisk.Substring(aBeginPos + 1, aEndPos - aBeginPos - 1).ToLower();
					oGroups.Add(aVarName);

					// 番組名・楽曲名は区切り文字を含むこともあるため最長一致で検索する
					// また、最低 1 つは最長一致が無いとマッチしない
					if (aVarName == RULE_VAR_PROGRAM || aVarName == RULE_VAR_TITLE || !aLongestExists && aEndPos == oRuleInDisk.Length - 1)
					{
						aSB.Append("(.*)");
						aLongestExists = true;
					}
					else
					{
						aSB.Append("(.*?)");
					}

					aBeginPos = aEndPos + 1;
				}
				else if (@".$^{[(|)*+?\".IndexOf(oRuleInDisk[aBeginPos]) >= 0)
				{
					// エスケープが必要な文字
					aSB.Append('\\');
					aSB.Append(oRuleInDisk[aBeginPos]);
					aBeginPos++;
				}
				else
				{
					// そのまま追加
					aSB.Append(oRuleInDisk[aBeginPos]);
					aBeginPos++;
				}
			}
			aSB.Append("$");
			oRuleInMemory = aSB.ToString();
		}

		// --------------------------------------------------------------------
		// <Title> 等の開始 < に対する終了 > の位置を返す
		// ＜引数＞ oBeginPos：開始 < の次の位置
		// --------------------------------------------------------------------
		private static Int32 MakeRegexPatternFindVarEnd(String oString, Int32 oBeginPos)
		{
			while (oBeginPos < oString.Length)
			{
				if (oString[oBeginPos] == RULE_VAR_END[0])
				{
					return oBeginPos;
				}
				oBeginPos++;
			}
			return -1;
		}
	}
	// public class YlCommon ___END___

	// ====================================================================
	// ドライブ接続時にゆかり検索対象フォルダーに自動的に追加するための情報
	// ====================================================================
	public class AutoTargetInfo
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public AutoTargetInfo()
		{
			Folders = new List<String>();
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 前回接続時に追加されていたフォルダー群（ドライブレターを除き '\\' から始まる）
		public List<String> Folders { get; set; }
	}

	// ====================================================================
	// フォルダー設定ウィンドウでのプレビュー情報
	// ====================================================================
	public class PreviewInfo
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ファイル名（パス無、ShLen 形式）
		public String FileName { get; set; }

		// 取得項目
		public String Items { get; set; }

	} // public class PreviewInfo ___END___

	// ====================================================================
	// ゆかり検索対象フォルダーの情報
	// ====================================================================

	public class TargetFolderInfo
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 親フォルダーかどうか
		public Boolean IsParent { get; set; }

		// 親フォルダーの場合のみ有効：サブフォルダーを表示しているかどうか
		public Boolean IsOpen { get; set; }

		// 親フォルダーの場合のみ有効：サブフォルダーが動作しているかどうか
		public Boolean IsChildRunning { get; set; }

		// 親フォルダーの場合のみ有効：親フォルダー＋サブフォルダーの数
		public Int32 NumTotalFolders { get; set; }

		// フォルダーパス（ExLen 形式）
		public String Path { get; set; }

		// 親フォルダーのパス（ソート用）（ExLen 形式）（親の場合は Path と同じ値にすること）
		public String ParentPath { get; set; }

		// 操作
		public FolderTask FolderTask { get; set; }

		// 動作状況
		public FolderTaskStatus FolderTaskStatus { get; set; }

		// フォルダー除外設定の状態
		public FolderExcludeSettingsStatus FolderExcludeSettingsStatus
		{
			get
			{
				if (mFolderExcludeSettingsStatus == FolderExcludeSettingsStatus.Unchecked)
				{
					mFolderExcludeSettingsStatus = YlCommon.DetectFolderExcludeSettingsStatus(Path);
				}
				return mFolderExcludeSettingsStatus;
			}
			set
			{
				mFolderExcludeSettingsStatus = value;
			}
		}

		// フォルダー設定の状態
		public FolderSettingsStatus FolderSettingsStatus
		{
			get
			{
				if (mFolderSettingsStatus == FolderSettingsStatus.Unchecked)
				{
					mFolderSettingsStatus = YlCommon.DetectFolderSettingsStatus2Ex(Path);
				}
				return mFolderSettingsStatus;
			}
			set
			{
				mFolderSettingsStatus = value;
			}
		}

		// UI に表示するかどうか
		public Boolean Visible { get; set; }

		// 表示用：アコーディオン
		public String AccLabel
		{
			get
			{
				if (IsParent)
				{
					return IsOpen ? "∨" : "＞";
				}
				else
				{
					return null;
				}
			}
		}

		// 表示用：状態
		public String FolderTaskStatusLabel
		{
			get
			{
				String aLabel;
				FolderTaskStatus aStatusForLabelColor;
				GetFolderTaskStatus(out aLabel, out aStatusForLabelColor);
				return aLabel;
			}
		}

		// 表示用：パス
		public String PathLabel
		{
			get
			{
				return YlCommon.ShortenPath(Path);
			}
		}

		// 表示用：設定有無
		public String FolderSettingsStatusLabel
		{
			get
			{
				return YlCommon.FOLDER_SETTINGS_STATUS_TEXTS[(Int32)FolderSettingsStatus];
			}
		}

		// 表示用：色分け
		public FolderTaskStatus StatusForLabelColor
		{
			get
			{
				String aLabel;
				FolderTaskStatus aStatusForLabelColor;
				GetFolderTaskStatus(out aLabel, out aStatusForLabelColor);
				return aStatusForLabelColor;
			}
		}

		// ゆかりすたーステータス取得
		public static YukaListerStatusDelegate MainWindowYukaListerStatus { get; set; }

		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public TargetFolderInfo(String oParentPathExLen, String oPathExLen)
		{
			IsParent = false;
			IsOpen = false;
			IsChildRunning = false;
			NumTotalFolders = 0;
			Path = oPathExLen;
			ParentPath = oParentPathExLen;
			FolderTask = FolderTask.AddFileName;
			FolderTaskStatus = FolderTaskStatus.Queued;
			FolderExcludeSettingsStatus = FolderExcludeSettingsStatus.Unchecked;
			FolderSettingsStatus = FolderSettingsStatus.Unchecked;
			Visible = false;
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ソート用比較関数
		// 例えば @"C:\A" 配下と @"C:\A 2" を正しく並べ替えるために ParentPath が必要
		// --------------------------------------------------------------------
		public static Int32 Compare(TargetFolderInfo oLhs, TargetFolderInfo oRhs)
		{
			if (oLhs.ParentPath != oRhs.ParentPath)
			{
				return String.Compare(oLhs.ParentPath, oRhs.ParentPath);
			}
			return String.Compare(oLhs.Path, oRhs.Path);
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// プロパティー FolderSettingsStatus の実体
		private FolderSettingsStatus mFolderSettingsStatus;

		// プロパティー FolderExcludeSettingsStatus の実体
		private FolderExcludeSettingsStatus mFolderExcludeSettingsStatus;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 状態
		// --------------------------------------------------------------------
		private void GetFolderTaskStatus(out String oLabel, out FolderTaskStatus oStatusForLabelColor)
		{
			if (MainWindowYukaListerStatus() == YukaListerStatus.Error)
			{
				oLabel = "エラー解決待ち";
				oStatusForLabelColor = FolderTaskStatus.Error;
				return;
			}

			switch (FolderTaskStatus)
			{
				case FolderTaskStatus.Queued:
					switch (FolderTask)
					{
						case FolderTask.AddFileName:
							oLabel = "追加予定";
							break;
						case FolderTask.AddInfo:
							oLabel = "ファイル名検索可";
							break;
						case FolderTask.Remove:
							oLabel = "削除予定";
							break;
						case FolderTask.Update:
							oLabel = "更新予定";
							break;
						default:
							Debug.Assert(false, "GetFolderTaskStatus() bad FolderTask in FolderTaskStatus.Queued");
							oLabel = null;
							break;
					}
					oStatusForLabelColor = FolderTaskStatus.Queued;
					break;
				case FolderTaskStatus.Running:
					switch (FolderTask)
					{
						case FolderTask.AddFileName:
							oLabel = "ファイル名確認中";
							break;
						case FolderTask.AddInfo:
							oLabel = "ファイル名検索可＋属性確認中";
							break;
						case FolderTask.FindSubFolders:
							oLabel = "サブフォルダー検索中";
							break;
						case FolderTask.Remove:
							oLabel = "削除中";
							break;
						case FolderTask.Update:
							oLabel = "更新中";
							break;
						default:
							Debug.Assert(false, "GetFolderTaskStatus() bad FolderTask in FolderTaskStatus.Running");
							oLabel = null;
							break;
					}
					oStatusForLabelColor = FolderTaskStatus.Running;
					break;
				case FolderTaskStatus.Error:
					oLabel = "エラー";
					oStatusForLabelColor = FolderTaskStatus.Error;
					break;
				case FolderTaskStatus.DoneInMemory:
					if (IsParent && IsChildRunning)
					{
						oLabel = "サブフォルダー待ち";
						oStatusForLabelColor = FolderTaskStatus.Running;
					}
					else
					{
						switch (FolderTask)
						{
							case FolderTask.AddFileName:
								oLabel = "ファイル名確認済";
								break;
							case FolderTask.AddInfo:
								oLabel = "ファイル名検索可＋属性確認済";
								break;
							case FolderTask.Remove:
								oLabel = "削除準備完了";
								break;
							case FolderTask.Update:
								oLabel = "更新準備完了";
								break;
							default:
								Debug.Assert(false, "GetFolderTaskStatus() bad FolderTask in FolderTaskStatus.DoneInMemory");
								oLabel = null;
								break;
						}
						oStatusForLabelColor = FolderTaskStatus.Queued;
					}
					break;
				case FolderTaskStatus.DoneInDisk:
					switch (FolderTask)
					{
						case FolderTask.AddFileName:
							Debug.Assert(false, "GetFolderTaskStatus() bad FolderTask in FolderTaskStatus.DoneInDisk - FolderTask.AddFileName");
							oLabel = null;
							break;
						case FolderTask.AddInfo:
							oLabel = "追加完了";
							break;
						case FolderTask.Remove:
							oLabel = "削除完了";
							break;
						case FolderTask.Update:
							oLabel = "更新完了";
							break;
						default:
							Debug.Assert(false, "GetFolderTaskStatus() bad oInfo.FolderTask in FolderTaskStatus.DoneInDisk");
							oLabel = null;
							break;
					}
					oStatusForLabelColor = FolderTaskStatus.DoneInDisk;
					break;
				default:
					Debug.Assert(false, "GetFolderTaskStatus() bad FolderTaskStatus");
					oLabel = null;
					oStatusForLabelColor = FolderTaskStatus.Error;
					break;
			}

			if (FolderExcludeSettingsStatus == FolderExcludeSettingsStatus.True)
			{
				oLabel = "対象外";
				oStatusForLabelColor = FolderTaskStatus.Queued;
			}
		}

	}
	// public class TargetFolderInfo ___END___

}
// namespace YukaLister.Shared ___END___