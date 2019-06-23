// ============================================================================
// 
// ゆかりすたー共通で使用する、定数・関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Hnx8.ReadJEnc;
using Livet;
using Livet.Messaging;
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
using YukaLister.Models.Database;
using YukaLister.Models.OutputWriters;
using YukaLister.ViewModels;

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
		public const String APP_VER = "Ver 2.00 α";
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

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コンテキストメニューにアイテムを追加
		// ToDo: MVVM 的なやり方でのコンテキストメニューへのコマンド登録方法が分からなかったのでこの方法としている
		// List<ViewModelCommand> をバインドするとコマンドの制御はできるが、表示文字列の制御ができない
		// --------------------------------------------------------------------
		public static void AddContextMenuItem(List<MenuItem> oItems, String oLabel, RoutedEventHandler oClick)
		{
			MenuItem aMenuItem = new MenuItem();
			aMenuItem.Header = oLabel;
			aMenuItem.Click += oClick;
			oItems.Add(aMenuItem);
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

#if false
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
#endif

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
		// 紐付テーブルのレコードを作成
		// --------------------------------------------------------------------
		public static T CreateSequenceRecord<T>(String oId, Int32 oSequence, String oLinkId, Boolean oIsImport = false) where T : IRcSequence, new()
		{
			return new T
			{
				// IDbBase
				Id = oId,
				Import = false,
				Invalid = false,
				UpdateTime = INVALID_MJD,
				Dirty = true,

				// IDbSequence
				Sequence = oSequence,
				LinkId = oLinkId,
			};
		}

		// --------------------------------------------------------------------
		// ちょちょいと自動更新起動を作成
		// --------------------------------------------------------------------
		public static UpdaterLauncher CreateUpdaterLauncher(Boolean oCheckLatest, Boolean oForceShow, Boolean oClearUpdateCache, Boolean oForceInstall, LogWriter oLogWriter)
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
			aUpdaterLauncher.LogWriter = oLogWriter;
			aUpdaterLauncher.ForceShow = oForceShow;
			aUpdaterLauncher.NotifyHWnd = IntPtr.Zero;
			aUpdaterLauncher.ClearUpdateCache = oClearUpdateCache;
			aUpdaterLauncher.ForceInstall = oForceInstall;

			// 起動
			return aUpdaterLauncher;
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
		// ファイル命名規則とフォルダー固定値を適用した情報を得る
		// --------------------------------------------------------------------
		public static Dictionary<String, String> DicByFile(String oPathExLen)
		{
			FolderSettingsInDisk aFolderSettingsInDisk = LoadFolderSettings2Ex(Path.GetDirectoryName(oPathExLen));
			FolderSettingsInMemory aFolderSettingsInMemory = CreateFolderSettingsInMemory(aFolderSettingsInDisk);
			return YlCommon.MatchFileNameRulesAndFolderRule(Path.GetFileNameWithoutExtension(oPathExLen), aFolderSettingsInMemory);
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
		public static void InputIdPrefixIfNeededWithInvoke(ViewModel oViewModel, EnvironmentModel oEnvironment)
		{
			if (!String.IsNullOrEmpty(oEnvironment.YukaListerSettings.IdPrefix))
			{
				return;
			}

			Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				using (InputIdPrefixWindowViewModel aInputIdPrefixWindowViewModel = new InputIdPrefixWindowViewModel())
				{
					aInputIdPrefixWindowViewModel.Environment = oEnvironment;
					oViewModel.Messenger.Raise(new TransitionMessage(aInputIdPrefixWindowViewModel, "OpenInputIdPrefixWindow"));
				}
			}));

			if (String.IsNullOrEmpty(oEnvironment.YukaListerSettings.IdPrefix))
			{
				throw new OperationCanceledException();
			}
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcAlias）
		// --------------------------------------------------------------------
		public static Boolean IsRcAliasUpdated(IRcAlias oExistRecord, IRcAlias oNewRecord)
		{
			Boolean? aIsRcBaseUpdated = IsRcBaseUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcBaseUpdated != null)
			{
				return aIsRcBaseUpdated.Value;
			}

			return oExistRecord.Alias != oNewRecord.Alias
					|| oExistRecord.OriginalId != oNewRecord.OriginalId;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcMaster）
		// --------------------------------------------------------------------
		public static Boolean IsRcMasterUpdated(IRcMaster oExistRecord, IRcMaster oNewRecord)
		{
			return IsRcMasterUpdatedCore(oExistRecord, oNewRecord) ?? false;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcSequence）
		// --------------------------------------------------------------------
		public static Boolean IsRcSequenceUpdated(IRcSequence oExistRecord, IRcSequence oNewRecord)
		{
			Boolean? aIsRcBaseUpdated = IsRcBaseUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcBaseUpdated != null)
			{
				return aIsRcBaseUpdated.Value;
			}

			return oExistRecord.LinkId != oNewRecord.LinkId;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TSong）
		// --------------------------------------------------------------------
		public static Boolean IsRcSongUpdated(TSong oExistRecord, TSong oNewRecord)
		{
			Boolean? aIsRcCategorizableUpdated = IsRcCategorizableUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcCategorizableUpdated != null)
			{
				return aIsRcCategorizableUpdated.Value;
			}

			return oExistRecord.TieUpId != oNewRecord.TieUpId
					|| oExistRecord.OpEd != oNewRecord.OpEd;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TTieUp）
		// --------------------------------------------------------------------
		public static Boolean IsRcTieUpUpdated(TTieUp oExistRecord, TTieUp oNewRecord)
		{
			Boolean? aIsRcCategorizableUpdated = IsRcCategorizableUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcCategorizableUpdated != null)
			{
				return aIsRcCategorizableUpdated.Value;
			}

			return oExistRecord.MakerId != oNewRecord.MakerId
					|| oExistRecord.AgeLimit != oNewRecord.AgeLimit;
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
		// 関数を非同期駆動
		// --------------------------------------------------------------------
		public static Task LaunchTaskAsync<T>(TaskAsyncDelegate<T> oDelegate, Object oTaskLock, T oVar, LogWriter oLogWriter)
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
						oLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バックグラウンド処理開始：" + oDelegate.Method.Name);
						oDelegate(oVar);
#if DEBUGz
						Thread.Sleep(5000);
#endif
						oLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バックグラウンド処理終了：" + oDelegate.Method.Name);
					}
				}
				catch (Exception oExcep)
				{
					oLogWriter.ShowLogMessage(TraceEventType.Error, "バックグラウンド処理 " + oDelegate.Method.Name + "実行時エラー：\n" + oExcep.Message);
					oLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
				}
			});
		}

#if false
		// --------------------------------------------------------------------
		// ちょちょいと自動更新を起動
		// --------------------------------------------------------------------
		public static Boolean LaunchUpdater(Boolean oCheckLatest, Boolean oForceShow, IntPtr oHWnd, Boolean oClearUpdateCache, Boolean oForceInstall, LogWriter oLogWriter)
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
			aUpdaterLauncher.LogWriter = oLogWriter;
			aUpdaterLauncher.ForceShow = oForceShow;
			aUpdaterLauncher.NotifyHWnd = oHWnd;
			aUpdaterLauncher.ClearUpdateCache = oClearUpdateCache;
			aUpdaterLauncher.ForceInstall = oForceInstall;

			// 起動
			return aUpdaterLauncher.Launch(oForceShow);
		}
#endif

		// --------------------------------------------------------------------
		// 環境設定の文字コードに従って CSV ファイルを読み込む
		// 下処理も行う
		// oNumColumns: 行番号も含めた列数
		// --------------------------------------------------------------------
		public static List<List<String>> LoadCsv(String oPath, EnvironmentModel oEnvironment, Int32 oNumColumns)
		{
			List<List<String>> aCsv;

			try
			{
				Encoding aEncoding;
				if (oEnvironment.YukaListerSettings.CsvEncoding == CsvEncoding.AutoDetect)
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
					aEncoding = EncodingFromCsvEncoding(oEnvironment.YukaListerSettings.CsvEncoding);
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
						oEnvironment.LogWriter.ShowLogMessage(TraceEventType.Warning,
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
				oEnvironment.LogWriter.ShowLogMessage(TraceEventType.Warning, "CSV ファイルを読み込めませんでした。\n" + oExcep.Message + "\n" + oPath, true);
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
		public static void LogEnvironmentInfo(LogWriter oLogWriter)
		{
			SystemEnvironment aSE = new SystemEnvironment();
			aSE.LogEnvironment(oLogWriter);
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
		// 日付に合わせて年月日文字列を設定
		// --------------------------------------------------------------------
		public static void MjdToStrings(Double oMjd, out String oYear, out String oMonth, out String oDay)
		{
			if (oMjd <= YlCommon.INVALID_MJD)
			{
				oYear = null;
				oMonth = null;
				oDay = null;
			}
			else
			{
				DateTime aReleaseDate = JulianDay.ModifiedJulianDateToDateTime(oMjd);
				oYear = aReleaseDate.Year.ToString();
				oMonth = aReleaseDate.Month.ToString();
				oDay = aReleaseDate.Day.ToString();
			}
		}

#if false
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
#endif

#if false
		// --------------------------------------------------------------------
		// 楽曲情報データベースファイルのフルパス
		// --------------------------------------------------------------------
		public static String MusicInfoDbPath()
		{
			return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + FOLDER_NAME_DATABASE + FILE_NAME_MUSIC_INFO;
		}
#endif

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
		// --------------------------------------------------------------------
		public static void OutputList(OutputWriter oOutputWriter, EnvironmentModel oEnvironment, YukariListDatabaseInMemory oYukariListDbInMemory)
		{
			oOutputWriter.OutputSettings.Load();
			using (DataContext aYukariDbContext = new DataContext(oYukariListDbInMemory.Connection))
			{
				Table<TFound> aTableFound = aYukariDbContext.GetTable<TFound>();
				oOutputWriter.TableFound = aTableFound;
				oOutputWriter.Output();
			}
		}

		// --------------------------------------------------------------------
		// 紐付テーブルに新規登録または更新
		// --------------------------------------------------------------------
		public static void RegisterSequence<T>(DataContext oContext, String oId, List<String> oLinkIds, Boolean oIsImport = false) where T : class, IRcSequence, new()
		{
			String aTableName = LinqUtils.TableName(typeof(T));

			// 新規レコード
			List<T> aNewSequences = new List<T>();
			for (Int32 i = 0; i < oLinkIds.Count; i++)
			{
				T aNewSequence = CreateSequenceRecord<T>(oId, i, oLinkIds[i], oIsImport);
				aNewSequences.Add(aNewSequence);
			}

			// 既存レコード
			List<T> aExistSequences = SelectSequencesById<T>(oContext, oId, true);

			// 既存レコードがインポートではなく新規レコードがインポートの場合は更新しない
			if (aExistSequences.Count > 0 && !aExistSequences[0].Import
					&& aNewSequences.Count > 0 && aNewSequences[0].Import)
			{
				return;
			}

			// 既存レコードがある場合は更新
			for (Int32 i = 0; i < Math.Min(aNewSequences.Count, aExistSequences.Count); i++)
			{
				if (YlCommon.IsRcSequenceUpdated(aExistSequences[i], aNewSequences[i]))
				{
					aNewSequences[i].UpdateTime = aExistSequences[i].UpdateTime;
					Common.ShallowCopy(aNewSequences[i], aExistSequences[i]);
					if (!oIsImport)
					{
						// ToDo: ログ
						//LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aTableName + " 紐付テーブル更新：" + oId + " / " + i.ToString());
					}
				}
			}

			// 既存レコードがない部分は新規登録
			Table<T> aTableSequence = oContext.GetTable<T>();
			for (Int32 i = aExistSequences.Count; i < aNewSequences.Count; i++)
			{
				aTableSequence.InsertOnSubmit(aNewSequences[i]);
				if (!oIsImport)
				{
					//LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aTableName + " 紐付テーブル新規登録：" + oId + " / " + i.ToString());
				}
			}

			// 既存レコードが余る部分は無効化
			for (Int32 i = aNewSequences.Count; i < aExistSequences.Count; i++)
			{
				if (!aExistSequences[i].Invalid)
				{
					aExistSequences[i].Invalid = true;
					aExistSequences[i].Dirty = true;
					if (!oIsImport)
					{
						//LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aTableName + " 紐付テーブル無効化：" + oId + " / " + i.ToString());
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから別名を検索
		// --------------------------------------------------------------------
		public static List<T> SelectAliasesByAlias<T>(DataContext oContext, String oAlias, Boolean oIncludesInvalid = false) where T : class, IRcAlias
		{
			if (String.IsNullOrEmpty(oAlias))
			{
				return new List<T>();
			}

			Table<T> aTableAlias = oContext.GetTable<T>();
			IQueryable<T> aQueryResult =
					from x in aTableAlias
					where x.Alias == oAlias && (oIncludesInvalid ? true : x.Invalid == false)
					select x;
			return aQueryResult.ToList();
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから別名を検索
		// --------------------------------------------------------------------
		public static List<T> SelectAliasesByAlias<T>(SQLiteConnection oConnection, String oAlias, Boolean oIncludesInvalid = false) where T : class, IRcAlias
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectAliasesByAlias<T>(aContext, oAlias, oIncludesInvalid);
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
		// 楽曲情報データベースから IRcMaster を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static T SelectMasterById<T>(DataContext oContext, String oId, Boolean oIncludesInvalid = false) where T : class, IRcMaster
		{
			if (String.IsNullOrEmpty(oId))
			{
				return null;
			}

			Table<T> aTableMaster = oContext.GetTable<T>();
			return aTableMaster.SingleOrDefault(x => x.Id == oId && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから IRcMaster を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static T SelectMasterById<T>(SQLiteConnection oConnection, String oId, Boolean oIncludesInvalid = false) where T : class, IRcMaster
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectMasterById<T>(aContext, oId, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから IRcMaster をすべて検索
		// --------------------------------------------------------------------
		public static List<T> SelectMastersByName<T>(DataContext oContext, String oName, Boolean oIncludesInvalid = false) where T : class, IRcMaster
		{
			if (String.IsNullOrEmpty(oName))
			{
				return new List<T>();
			}

			Table<T> aTableMaster = oContext.GetTable<T>();
			IQueryable<T> aQueryResult =
					from x in aTableMaster
					where x.Name == oName && (oIncludesInvalid ? true : x.Invalid == false)
					select x;
			return aQueryResult.ToList();
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから IRcMaster をすべて検索
		// --------------------------------------------------------------------
		public static List<T> SelectMastersByName<T>(SQLiteConnection oConnection, String oName, Boolean oIncludesInvalid = false) where T : class, IRcMaster
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectMastersByName<T>(aContext, oName, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲に紐付く人物を検索
		// oIncludesInvalid が true の場合、無効 ID に null を紐付ける
		// --------------------------------------------------------------------
		public static List<TPerson> SelectSequencePeopleBySongId<T>(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false) where T : class, IRcSequence
		{
			List<T> aSequences = SelectSequencesById<T>(oContext, oSongId, oIncludesInvalid);
			List<TPerson> aPeople = new List<TPerson>();

			foreach (T aSequence in aSequences)
			{
				TPerson aPerson = SelectMasterById<TPerson>(oContext, aSequence.LinkId);
				if (aPerson != null || oIncludesInvalid)
				{
					aPeople.Add(aPerson);
				}
			}

			return aPeople;
		}

		// --------------------------------------------------------------------
		// 紐付データベースから紐付を検索
		// --------------------------------------------------------------------
		public static List<T> SelectSequencesById<T>(DataContext oContext, String oId, Boolean oIncludesInvalid = false) where T : class, IRcSequence
		{
			List<T> aSequences = new List<T>();

			if (!String.IsNullOrEmpty(oId))
			{
				Table<T> aTableSequence = oContext.GetTable<T>();
				IQueryable<T> aQueryResult =
						from x in aTableSequence
						where x.Id == oId && (oIncludesInvalid ? true : x.Invalid == false)
						orderby x.Sequence
						select x;
				aSequences = aQueryResult.ToList();
			}

			return aSequences;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップに紐付くタイアップグループを検索
		// oIncludesInvalid が true の場合、無効 ID に null を紐付ける
		// --------------------------------------------------------------------
		public static List<TTieUpGroup> SelectSequenceTieUpGroupsByTieUpId(DataContext oContext, String oTieUpId, Boolean oIncludesInvalid = false)
		{
			List<TTieUpGroupSequence> aSequences = SelectSequencesById<TTieUpGroupSequence>(oContext, oTieUpId, oIncludesInvalid);
			List<TTieUpGroup> aTieUpGroups = new List<TTieUpGroup>();

			foreach (TTieUpGroupSequence aSequence in aSequences)
			{
				TTieUpGroup aTieUpGroup = SelectMasterById<TTieUpGroup>(oContext, aSequence.LinkId);
				if (aTieUpGroup != null || oIncludesInvalid)
				{
					aTieUpGroups.Add(aTieUpGroup);
				}
			}

			return aTieUpGroups;
		}

		// --------------------------------------------------------------------
		// カテゴリーメニューに値を設定
		// --------------------------------------------------------------------
		public static void SetContextMenuItemCategories(List<MenuItem> oMenuItems, RoutedEventHandler oClick, EnvironmentModel oEnvironment)
		{
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(oEnvironment))
			{
				using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					Table<TCategory> aTableCategory = aContext.GetTable<TCategory>();
					IQueryable<TCategory> aQueryResult =
							from x in aTableCategory
							select x;
					foreach (TCategory aCategory in aQueryResult)
					{
						AddContextMenuItem(oMenuItems, aCategory.Name, oClick);
					}
				}
			}
		}

#if false
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
#endif

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
		public static void ShowHelp(EnvironmentModel oEnvironment, String oAnchor = null)
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
						oEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "状況に応じたヘルプを表示できませんでした：\n" + oExcep.Message + "\n" + aHelpPath
								+ "\n通常のヘルプを表示します。");
					}
				}

				// アンカーが指定されていない場合・状況依存型ヘルプを表示できなかった場合は通常のヘルプを表示
				aHelpPath = aHelpPathBase + FILE_NAME_HELP_PREFIX + Common.FILE_EXT_HTML;
				Process.Start(aHelpPath);
			}
			catch (Exception oExcep)
			{
				oEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "ヘルプを表示できませんでした。\n" + oExcep.Message + "\n" + aHelpPath);
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
		// 年月日の文字列から日付を生成
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public static Double StringsToMjd(String oCaption, String oYearString, String oMonthString, String oDayString)
		{
			if (String.IsNullOrEmpty(oYearString))
			{
				// 年が入力されていない場合は、月日も空欄でなければならない
				if (!String.IsNullOrEmpty(oMonthString) || !String.IsNullOrEmpty(oDayString))
				{
					throw new Exception(oCaption + "の年が入力されていません。");
				}

				return INVALID_MJD;
			}

			// 年の確認
			Int32 aYear = Common.StringToInt32(oYearString);
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
			if (String.IsNullOrEmpty(oMonthString) && !String.IsNullOrEmpty(oDayString))
			{
				// 年と日が入力されている場合は、月も入力されていなければならない
				throw new Exception(oCaption + "の月が入力されていません。");
			}
			Int32 aMonth;
			if (String.IsNullOrEmpty(oMonthString))
			{
				// 月が空欄の場合は 1 とする
				aMonth = 1;
			}
			else
			{
				aMonth = Common.StringToInt32(oMonthString);
				if (aMonth < 1 || aMonth > 12)
				{
					throw new Exception(oCaption + "の月は 1～12 を入力して下さい。");
				}
			}

			// 日の確認
			Int32 aDay;
			if (String.IsNullOrEmpty(oDayString))
			{
				// 日が空欄の場合は 1 とする
				aDay = 1;
			}
			else
			{
				aDay = Common.StringToInt32(oDayString);
				if (aDay < 1 || aDay > 31)
				{
					throw new Exception(oCaption + "の日は 1～31 を入力して下さい。");
				}
			}

			return JulianDay.DateTimeToModifiedJulianDate(new DateTime(aYear, aMonth, aDay));
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

#if false
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
#endif

#if false
		// --------------------------------------------------------------------
		// 同名のタイアップがある場合でも見分けがつく名前を返す
		// --------------------------------------------------------------------
		public static String TieUpNameAvoidingSameName(SQLiteConnection oConnection, TTieUp oTieUp)
		{
			List<TTieUp> aTieUps = YlCommon.SelectMastersByName<TTieUp>(oConnection, oTieUp.Name);
			if (aTieUps.Count <= 1)
			{
				// 同名のタイアップが無い場合はタイアップ名のみ
				return oTieUp.Name;
			}
			else
			{
				// 同名タイアップが複数ある場合は見分けやすいようにする
				TCategory aCategory = YlCommon.SelectMasterById<TCategory>(oConnection, oTieUp.CategoryId);
				String aCategoryName = null;
				if (aCategory != null)
				{
					aCategoryName = aCategory.Name;
				}
				return oTieUp.Name + "（" + (String.IsNullOrEmpty(aCategoryName) ? "カテゴリー無し" : aCategoryName) + ", "
						+ (String.IsNullOrEmpty(oTieUp.Keyword) ? "キーワード無し" : oTieUp.Keyword) + "）";
			}
		}
#endif

#if false
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
#endif

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
		// レコードの内容が更新されたか（IRcBase）
		// より派生型の IsRcXXXUpdated() から呼び出される前提
		// プライマリーキーは比較しない
		// ＜返値＞ true: 更新された, false: 更新されていない, null: より派生型での判断に委ねる
		// --------------------------------------------------------------------
		private static Boolean? IsRcBaseUpdatedCore(IRcBase oExistRecord, IRcBase oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				// 既存レコードがゆかりすたー登録で新規レコードがインポートの場合は、ゆかりすたー登録した既存レコードを優先する
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

			// 派生型の内容が更新されたかどうかで判断すべき
			return null;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcCategorizable）
		// より派生型の IsRcXXXUpdated() から呼び出される前提
		// ＜返値＞ true: 更新された, false: 更新されていない, null: より派生型での判断に委ねる
		// --------------------------------------------------------------------
		private static Boolean? IsRcCategorizableUpdatedCore(IRcCategorizable oExistRecord, IRcCategorizable oNewRecord)
		{
			Boolean? aIsRcMasterUpdated = IsRcMasterUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcMasterUpdated != null)
			{
				return aIsRcMasterUpdated.Value;
			}

			// IRcCategorizable の要素が更新されていれば更新されたことが確定
			if (oExistRecord.CategoryId != oNewRecord.CategoryId
					|| oExistRecord.ReleaseDate != oNewRecord.ReleaseDate)
			{
				return true;
			}

			// 派生型の内容が更新されたかどうかで判断すべき
			return null;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcMaster）
		// より派生型の IsRcXXXUpdated() から呼び出される前提
		// ＜返値＞ true: 更新された, false: 更新されていない, null: より派生型での判断に委ねる
		// --------------------------------------------------------------------
		private static Boolean? IsRcMasterUpdatedCore(IRcMaster oExistRecord, IRcMaster oNewRecord)
		{
			Boolean? aIsRcBaseUpdated = IsRcBaseUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcBaseUpdated != null)
			{
				return aIsRcBaseUpdated.Value;
			}

			// IRcMaster の要素が更新されていれば更新されたことが確定
			if (oExistRecord.Name != oNewRecord.Name
					|| oExistRecord.Ruby != oNewRecord.Ruby
					|| oExistRecord.Keyword != oNewRecord.Keyword)
			{
				return true;
			}

			// 派生型の内容が更新されたかどうかで判断すべき
			return null;
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

		// ファイル名（パス無）
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

		// 親フォルダーの場合のみ有効：サブフォルダーを表示しているかどうか：表示用兼用
		private Boolean mIsOpen;
		public Boolean? IsOpen
		{
			get
			{
				if (IsParent && NumTotalFolders > 1)
				{
					return mIsOpen;
				}
				return null;
			}
			set
			{
				if (IsParent && NumTotalFolders > 1 && value != mIsOpen)
				{
					mIsOpen = (Boolean)value;
					IsOpenChanged(this);
				}
			}
		}

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
			set
			{
				Debug.Assert(false, "TargetFolderInfo.FolderTaskStatusLabel set: forbidden");
			}
		}

		// 表示用：パス
		public String PathLabel
		{
			get
			{
				return YlCommon.ShortenPath(Path);
			}
			set
			{
				Debug.Assert(false, "TargetFolderInfo.PathLabel set: forbidden");
			}
		}

		// 表示用：設定有無
		public String FolderSettingsStatusLabel
		{
			get
			{
				return YlCommon.FOLDER_SETTINGS_STATUS_TEXTS[(Int32)FolderSettingsStatus];
			}
			set
			{
				Debug.Assert(false, "TargetFolderInfo.FolderSettingsStatusLabel set: forbidden");
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
			set
			{
				Debug.Assert(false, "TargetFolderInfo.StatusForLabelColor set: forbidden");
			}
		}

		// ゆかり用リストデータベース構築状況取得
		public static YukaListerStatusDelegate YukariDbYukaListerStatus { get; set; }

		// IsOpen 変更時イベントハンドラー
		public static TargetFolderInfoIsOpenChangedDelegate IsOpenChanged { get; set; }

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
			if (YukariDbYukaListerStatus() == YukaListerStatus.Error)
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