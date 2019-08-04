// ============================================================================
// 
// ゆかりすたー共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Shinta;

using System;

using YukaLister.Models.Database;

namespace YukaLister.Models.SharedMisc
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
	// リスト問題の修正項目インデックス
	// --------------------------------------------------------------------
	public enum ReportAdjustKey
	{
		Invalid,
		Misc,
		CategoryName,
		TieUpName,
		OpEd,
		SongName,
		ArtistName,
		Track,
		Worker,
		TieUpGroupName,
		AgeLimit,
		__End__,
	}

	// --------------------------------------------------------------------
	// リスト問題の対応状況
	// --------------------------------------------------------------------
	public enum ReportStatus
	{
		Registered, // 未対応
		Progress,   // 対応中
		Retention,  // 保留
		Invalid,    // 無効
		Done,       // 完了
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
	public delegate void TargetFolderInfoIsOpenChangedDelegate(TargetFolderInfo oTargetFolderInfo);

	public class YlConstants
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
		public const String APP_VER = "Ver 2.25 β";
		public const String COPYRIGHT_J = "Copyright (C) 2019 by SHINTA";

		// --------------------------------------------------------------------
		// フォルダー名
		// --------------------------------------------------------------------
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
		// この他、報告データベースで "R" を使用する
		public static readonly String[] MUSIC_INFO_ID_SECOND_PREFIXES =
		{
			"_S_", "_P_", "_T_","_C_", "_G_", "_M_",
			"_SA_", "_PA_", "_TA_","_CA_", "_GA_", "_MA_",
			null, null, null, null, null,
		};

		// --------------------------------------------------------------------
		// リスト問題報告データベース
		// --------------------------------------------------------------------

		// 修正項目名
		public static readonly String[] REPORT_ADJUST_KEY_NAMES =
		{
			null, "その他", "カテゴリー名", "タイアップ名", "摘要", "楽曲名", "歌手名", "トラック", "制作者", "シリーズ名", "年齢制限",
		};

		// 対応状況名
		public static readonly String[] REPORT_STATUS_NAMES =
		{
			"未対応", "対応中", "保留", "無効", "完了"
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
		// Web サーバーコマンドオプション
		// --------------------------------------------------------------------

		public const String SERVER_OPTION_NAME_EASY_PASS = "easypass";
		public const String SERVER_OPTION_NAME_UID = "uid";
		public const String SERVER_OPTION_NAME_WIDTH = "width";

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

		// タイアップグループ名を表示する際に末尾に付与する文字列
		public const String TIE_UP_GROUP_SUFFIX = "シリーズ";

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
	}
	// public class YlConstants ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
