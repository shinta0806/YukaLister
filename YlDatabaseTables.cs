﻿// ============================================================================
// 
// データベーステーブル定義をカプセル化
// 
// ============================================================================

// ----------------------------------------------------------------------------
// TBase / TMaster / TAlias はイメージに過ぎない
// 実際にテーブル定義クラスを継承させようとすると、派生クラスには Table 属性を
// 付けられず、Context.GetTable() できなくなる
// インターフェースでメンバーを返すようにすると、インターフェース関数が SQL サーバー側
// で実行できずに例外が発生する
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Data.Linq.Mapping;

namespace YukaLister.Shared
{
	// ====================================================================
	// 楽曲マスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_SONG)]
	public class TSong
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_SONG = "t_song";
		public const String FIELD_NAME_SONG_ID = "song_id";
		public const String FIELD_NAME_SONG_IMPORT = "song_import";
		public const String FIELD_NAME_SONG_INVALID = "song_invalid";
		public const String FIELD_NAME_SONG_UPDATE_TIME = "song_update_time";
		public const String FIELD_NAME_SONG_DIRTY = "song_dirty";
		public const String FIELD_NAME_SONG_NAME = "song_name";
		public const String FIELD_NAME_SONG_RUBY = "song_ruby";
		public const String FIELD_NAME_SONG_KEYWORD = "song_keyword";
		public const String FIELD_NAME_SONG_RELEASE_DATE = "song_release_date";
		public const String FIELD_NAME_SONG_TIE_UP_ID = "song_tie_up_id";
		public const String FIELD_NAME_SONG_CATEGORY_ID = "song_category_id";
		public const String FIELD_NAME_SONG_OP_ED = "song_op_ed";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// 楽曲 ID
		[Column(Name = FIELD_NAME_SONG_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_SONG_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_SONG_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_SONG_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_SONG_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TMaster
		// --------------------------------------------------------------------

		// 楽曲名
		[Column(Name = FIELD_NAME_SONG_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Name { get; set; }

		// 楽曲フリガナ
		[Column(Name = FIELD_NAME_SONG_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Ruby { get; set; }

		// 検索ワード
		[Column(Name = FIELD_NAME_SONG_KEYWORD, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Keyword { get; set; }

		// --------------------------------------------------------------------
		// TSong
		// --------------------------------------------------------------------

		// リリース日（修正ユリウス日）
		[Column(Name = FIELD_NAME_SONG_RELEASE_DATE, DbType = LinqUtils.DB_TYPE_DOUBLE)]
		public Double ReleaseDate { get; set; }

		// タイアップ ID ＜参照項目＞
		[Column(Name = FIELD_NAME_SONG_TIE_UP_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String TieUpId { get; set; }

		// カテゴリー ID ＜参照項目＞（タイアップ ID が null の場合のみ）
		[Column(Name = FIELD_NAME_SONG_CATEGORY_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String CategoryId { get; set; }

		// 摘要
		[Column(Name = FIELD_NAME_SONG_OP_ED, DbType = LinqUtils.DB_TYPE_STRING)]
		public String OpEd { get; set; }
	}
	// public class TSong ___END___

	// ====================================================================
	// 人物マスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_PERSON)]
	public class TPerson
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_PERSON = "t_person";
		public const String FIELD_NAME_PERSON_ID = "person_id";
		public const String FIELD_NAME_PERSON_IMPORT = "person_import";
		public const String FIELD_NAME_PERSON_INVALID = "person_invalid";
		public const String FIELD_NAME_PERSON_UPDATE_TIME = "person_update_time";
		public const String FIELD_NAME_PERSON_DIRTY = "person_dirty";
		public const String FIELD_NAME_PERSON_NAME = "person_name";
		public const String FIELD_NAME_PERSON_RUBY = "person_ruby";
		public const String FIELD_NAME_PERSON_KEYWORD = "person_keyword";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// 人物 ID
		[Column(Name = FIELD_NAME_PERSON_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_PERSON_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_PERSON_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_PERSON_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_PERSON_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TMaster
		// --------------------------------------------------------------------

		// 人物名
		[Column(Name = FIELD_NAME_PERSON_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Name { get; set; }

		// 人物フリガナ
		[Column(Name = FIELD_NAME_PERSON_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Ruby { get; set; }

		// 検索ワード
		[Column(Name = FIELD_NAME_PERSON_KEYWORD, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Keyword { get; set; }
	}
	// public class TPerson ___END___

	// ====================================================================
	// タイアップマスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TIE_UP)]
	public class TTieUp
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_TIE_UP = "t_tie_up";
		public const String FIELD_NAME_TIE_UP_ID = "tie_up_id";
		public const String FIELD_NAME_TIE_UP_IMPORT = "tie_up_import";
		public const String FIELD_NAME_TIE_UP_INVALID = "tie_up_invalid";
		public const String FIELD_NAME_TIE_UP_UPDATE_TIME = "tie_up_update_time";
		public const String FIELD_NAME_TIE_UP_DIRTY = "tie_up_dirty";
		public const String FIELD_NAME_TIE_UP_NAME = "tie_up_name";
		public const String FIELD_NAME_TIE_UP_RUBY = "tie_up_ruby";
		public const String FIELD_NAME_TIE_UP_KEYWORD = "tie_up_keyword";
		public const String FIELD_NAME_TIE_UP_CATEGORY_ID = "tie_up_category_id";
		public const String FIELD_NAME_TIE_UP_MAKER_ID = "tie_up_maker_id";
		public const String FIELD_NAME_TIE_UP_AGE_LIMIT = "tie_up_age_limit";
		public const String FIELD_NAME_TIE_UP_RELEASE_DATE = "tie_up_release_date";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// タイアップ ID
		[Column(Name = FIELD_NAME_TIE_UP_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_TIE_UP_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_TIE_UP_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_TIE_UP_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_TIE_UP_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TMaster
		// --------------------------------------------------------------------

		// タイアップ名
		[Column(Name = FIELD_NAME_TIE_UP_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Name { get; set; }

		// タイアップフリガナ
		[Column(Name = FIELD_NAME_TIE_UP_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Ruby { get; set; }

		// 検索ワード
		[Column(Name = FIELD_NAME_TIE_UP_KEYWORD, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Keyword { get; set; }

		// --------------------------------------------------------------------
		// TTieUp
		// --------------------------------------------------------------------

		// カテゴリー ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TIE_UP_CATEGORY_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String CategoryId { get; set; }

		// 制作会社 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TIE_UP_MAKER_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String MakerId { get; set; }

		// 年齢制限（○歳以上対象）
		[Column(Name = FIELD_NAME_TIE_UP_AGE_LIMIT, DbType = LinqUtils.DB_TYPE_INT32)]
		public Int32 AgeLimit { get; set; }

		// リリース日（修正ユリウス日）
		[Column(Name = FIELD_NAME_TIE_UP_RELEASE_DATE, DbType = LinqUtils.DB_TYPE_DOUBLE)]
		public Double ReleaseDate { get; set; }
	}
	// public class TTieUp ___END___

	// ====================================================================
	// カテゴリーマスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_CATEGORY)]
	public class TCategory
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_CATEGORY = "t_category";
		public const String FIELD_NAME_CATEGORY_ID = "category_id";
		public const String FIELD_NAME_CATEGORY_IMPORT = "category_import";
		public const String FIELD_NAME_CATEGORY_INVALID = "category_invalid";
		public const String FIELD_NAME_CATEGORY_UPDATE_TIME = "category_update_time";
		public const String FIELD_NAME_CATEGORY_DIRTY = "category_dirty";
		public const String FIELD_NAME_CATEGORY_NAME = "category_name";
		public const String FIELD_NAME_CATEGORY_RUBY = "category_ruby";
		public const String FIELD_NAME_CATEGORY_KEYWORD = "category_keyword";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// カテゴリー ID
		[Column(Name = FIELD_NAME_CATEGORY_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_CATEGORY_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_CATEGORY_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_CATEGORY_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_CATEGORY_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TMaster
		// --------------------------------------------------------------------

		// カテゴリー名
		[Column(Name = FIELD_NAME_CATEGORY_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Name { get; set; }

		// カテゴリーフリガナ
		[Column(Name = FIELD_NAME_CATEGORY_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Ruby { get; set; }

		// 検索ワード
		[Column(Name = FIELD_NAME_CATEGORY_KEYWORD, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Keyword { get; set; }
	}
	// public class TCategory ___END___

	// ====================================================================
	// タイアップグループマスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TIE_UP_GROUP)]
	public class TTieUpGroup
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_TIE_UP_GROUP = "t_tie_up_group";
		public const String FIELD_NAME_TIE_UP_GROUP_ID = "tie_up_group_id";
		public const String FIELD_NAME_TIE_UP_GROUP_IMPORT = "tie_up_group_import";
		public const String FIELD_NAME_TIE_UP_GROUP_INVALID = "tie_up_group_invalid";
		public const String FIELD_NAME_TIE_UP_GROUP_UPDATE_TIME = "tie_up_group_update_time";
		public const String FIELD_NAME_TIE_UP_GROUP_DIRTY = "tie_up_group_dirty";
		public const String FIELD_NAME_TIE_UP_GROUP_NAME = "tie_up_group_name";
		public const String FIELD_NAME_TIE_UP_GROUP_RUBY = "tie_up_group_ruby";
		public const String FIELD_NAME_TIE_UP_GROUP_KEYWORD = "tie_up_group_keyword";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// タイアップグループ ID
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TMaster
		// --------------------------------------------------------------------

		// タイアップグループ名
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Name { get; set; }

		// タイアップグループフリガナ
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Ruby { get; set; }

		// 検索ワード
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_KEYWORD, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Keyword { get; set; }
	}
	// public class TTieUpGroup ___END___

	// ====================================================================
	// 制作会社マスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_MAKER)]
	public class TMaker
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_MAKER = "t_maker";
		public const String FIELD_NAME_MAKER_ID = "maker_id";
		public const String FIELD_NAME_MAKER_IMPORT = "maker_import";
		public const String FIELD_NAME_MAKER_INVALID = "maker_invalid";
		public const String FIELD_NAME_MAKER_UPDATE_TIME = "maker_update_time";
		public const String FIELD_NAME_MAKER_DIRTY = "maker_dirty";
		public const String FIELD_NAME_MAKER_NAME = "maker_name";
		public const String FIELD_NAME_MAKER_RUBY = "maker_ruby";
		public const String FIELD_NAME_MAKER_KEYWORD = "maker_keyword";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// 制作会社 ID
		[Column(Name = FIELD_NAME_MAKER_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_MAKER_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_MAKER_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_MAKER_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_MAKER_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TMaster
		// --------------------------------------------------------------------

		// 制作会社名
		[Column(Name = FIELD_NAME_MAKER_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Name { get; set; }

		// 制作会社フリガナ
		[Column(Name = FIELD_NAME_MAKER_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Ruby { get; set; }

		// 検索ワード
		[Column(Name = FIELD_NAME_MAKER_KEYWORD, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Keyword { get; set; }
	}

	// ====================================================================
	// 楽曲別名テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_SONG_ALIAS)]
	public class TSongAlias
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_SONG_ALIAS = "t_song_alias";
		public const String FIELD_NAME_SONG_ALIAS_ID = "song_alias_id";
		public const String FIELD_NAME_SONG_ALIAS_IMPORT = "song_alias_import";
		public const String FIELD_NAME_SONG_ALIAS_INVALID = "song_alias_invalid";
		public const String FIELD_NAME_SONG_ALIAS_UPDATE_TIME = "song_alias_update_time";
		public const String FIELD_NAME_SONG_ALIAS_SYNC_TIME = "song_alias_sync_time";
		public const String FIELD_NAME_SONG_ALIAS_DIRTY = "song_alias_dirty";
		public const String FIELD_NAME_SONG_ALIAS_ALIAS = "song_alias_alias";
		public const String FIELD_NAME_SONG_ALIAS_ORIGINAL_ID = "song_alias_original_id";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// 楽曲別名 ID
		[Column(Name = FIELD_NAME_SONG_ALIAS_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_SONG_ALIAS_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_SONG_ALIAS_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_SONG_ALIAS_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_SONG_ALIAS_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TAlias
		// --------------------------------------------------------------------

		// 楽曲別名
		[Column(Name = FIELD_NAME_SONG_ALIAS_ALIAS, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Alias { get; set; }

		// 元の楽曲 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_SONG_ALIAS_ORIGINAL_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String OriginalId { get; set; }
	}
	// public class TSongAlias ___END___

	// ====================================================================
	// 人物別名テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_PERSON_ALIAS)]
	public class TPersonAlias
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_PERSON_ALIAS = "t_person_alias";
		public const String FIELD_NAME_PERSON_ALIAS_ID = "person_alias_id";
		public const String FIELD_NAME_PERSON_ALIAS_IMPORT = "person_alias_import";
		public const String FIELD_NAME_PERSON_ALIAS_INVALID = "person_alias_invalid";
		public const String FIELD_NAME_PERSON_ALIAS_UPDATE_TIME = "person_alias_update_time";
		public const String FIELD_NAME_PERSON_ALIAS_DIRTY = "person_alias_dirty";
		public const String FIELD_NAME_PERSON_ALIAS_ALIAS = "person_alias_alias";
		public const String FIELD_NAME_PERSON_ALIAS_ORIGINAL_ID = "person_alias_original_id";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// 人物別名 ID
		[Column(Name = FIELD_NAME_PERSON_ALIAS_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_PERSON_ALIAS_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_PERSON_ALIAS_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_PERSON_ALIAS_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_PERSON_ALIAS_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TAlias
		// --------------------------------------------------------------------

		// 人物別名
		[Column(Name = FIELD_NAME_PERSON_ALIAS_ALIAS, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Alias { get; set; }

		// 元の人物 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_PERSON_ALIAS_ORIGINAL_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String OriginalId { get; set; }
	}
	// public class TPersonAlias ___END___

	// ====================================================================
	// タイアップ別名テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TIE_UP_ALIAS)]
	public class TTieUpAlias
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_TIE_UP_ALIAS = "t_tie_up_alias";
		public const String FIELD_NAME_TIE_UP_ALIAS_ID = "tie_up_alias_id";
		public const String FIELD_NAME_TIE_UP_ALIAS_IMPORT = "tie_up_alias_import";
		public const String FIELD_NAME_TIE_UP_ALIAS_INVALID = "tie_up_alias_invalid";
		public const String FIELD_NAME_TIE_UP_ALIAS_UPDATE_TIME = "tie_up_alias_update_time";
		public const String FIELD_NAME_TIE_UP_ALIAS_DIRTY = "tie_up_alias_dirty";
		public const String FIELD_NAME_TIE_UP_ALIAS_ALIAS = "tie_up_alias_alias";
		public const String FIELD_NAME_TIE_UP_ALIAS_ORIGINAL_ID = "tie_up_alias_original_id";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// タイアップ別名 ID
		[Column(Name = FIELD_NAME_TIE_UP_ALIAS_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_TIE_UP_ALIAS_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_TIE_UP_ALIAS_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_TIE_UP_ALIAS_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_TIE_UP_ALIAS_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TAlias
		// --------------------------------------------------------------------

		// タイアップ別名
		[Column(Name = FIELD_NAME_TIE_UP_ALIAS_ALIAS, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Alias { get; set; }

		// 元のタイアップ ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TIE_UP_ALIAS_ORIGINAL_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String OriginalId { get; set; }
	}
	// public class TTieUpAlias ___END___

	// ====================================================================
	// カテゴリー別名テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_CATEGORY_ALIAS)]
	public class TCategoryAlias
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_CATEGORY_ALIAS = "t_category_alias";
		public const String FIELD_NAME_CATEGORY_ALIAS_ID = "category_alias_id";
		public const String FIELD_NAME_CATEGORY_ALIAS_IMPORT = "category_alias_import";
		public const String FIELD_NAME_CATEGORY_ALIAS_INVALID = "category_alias_invalid";
		public const String FIELD_NAME_CATEGORY_ALIAS_UPDATE_TIME = "category_alias_update_time";
		public const String FIELD_NAME_CATEGORY_ALIAS_DIRTY = "category_alias_dirty";
		public const String FIELD_NAME_CATEGORY_ALIAS_ALIAS = "category_alias_alias";
		public const String FIELD_NAME_CATEGORY_ALIAS_ORIGINAL_ID = "category_alias_original_id";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// カテゴリー別名 ID
		[Column(Name = FIELD_NAME_CATEGORY_ALIAS_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_CATEGORY_ALIAS_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_CATEGORY_ALIAS_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_CATEGORY_ALIAS_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_CATEGORY_ALIAS_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TAlias
		// --------------------------------------------------------------------

		// カテゴリー別名
		[Column(Name = FIELD_NAME_CATEGORY_ALIAS_ALIAS, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Alias { get; set; }

		// 元のカテゴリー ID ＜参照項目＞
		[Column(Name = FIELD_NAME_CATEGORY_ALIAS_ORIGINAL_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String PersonId { get; set; }
	}
	// public class TCategoryAlias ___END___

	// ====================================================================
	// タイアップグループ別名テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TIE_UP_GROUP_ALIAS)]
	public class TTieUpGroupAlias
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_TIE_UP_GROUP_ALIAS = "t_tie_up_group_alias";
		public const String FIELD_NAME_TIE_UP_GROUP_ALIAS_ID = "tie_up_group_alias_id";
		public const String FIELD_NAME_TIE_UP_GROUP_ALIAS_IMPORT = "tie_up_group_alias_import";
		public const String FIELD_NAME_TIE_UP_GROUP_ALIAS_INVALID = "tie_up_group_alias_invalid";
		public const String FIELD_NAME_TIE_UP_GROUP_ALIAS_UPDATE_TIME = "tie_up_group_alias_update_time";
		public const String FIELD_NAME_TIE_UP_GROUP_ALIAS_DIRTY = "tie_up_group_alias_dirty";
		public const String FIELD_NAME_TIE_UP_GROUP_ALIAS_ALIAS = "tie_up_group_alias_alias";
		public const String FIELD_NAME_TIE_UP_GROUP_ALIAS_ORIGINAL_ID = "tie_up_group_alias_original_id";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// タイアップグループ別名 ID
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_ALIAS_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_ALIAS_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_ALIAS_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_ALIAS_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_ALIAS_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TAlias
		// --------------------------------------------------------------------

		// タイアップグループ別名
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_ALIAS_ALIAS, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Alias { get; set; }

		// 元のタイアップグループ ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_ALIAS_ORIGINAL_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String OriginalId { get; set; }
	}
	// public class TTieUpGroupAlias ___END___

	// ====================================================================
	// 制作会社別名テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_MAKER_ALIAS)]
	public class TMakerAlias
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_MAKER_ALIAS = "t_maker_alias";
		public const String FIELD_NAME_MAKER_ALIAS_ID = "maker_alias_id";
		public const String FIELD_NAME_MAKER_ALIAS_IMPORT = "maker_alias_import";
		public const String FIELD_NAME_MAKER_ALIAS_INVALID = "maker_alias_invalid";
		public const String FIELD_NAME_MAKER_ALIAS_UPDATE_TIME = "maker_alias_update_time";
		public const String FIELD_NAME_MAKER_ALIAS_DIRTY = "maker_alias_dirty";
		public const String FIELD_NAME_MAKER_ALIAS_ALIAS = "maker_alias_alias";
		public const String FIELD_NAME_MAKER_ALIAS_ORIGINAL_ID = "maker_alias_original_id";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// 制作会社別名 ID
		[Column(Name = FIELD_NAME_MAKER_ALIAS_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_MAKER_ALIAS_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_MAKER_ALIAS_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_MAKER_ALIAS_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_MAKER_ALIAS_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TAlias
		// --------------------------------------------------------------------

		// 制作会社別名
		[Column(Name = FIELD_NAME_MAKER_ALIAS_ALIAS, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Alias { get; set; }

		// 元の制作会社 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_MAKER_ALIAS_ORIGINAL_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String PersonId { get; set; }
	}
	// public class TMakerAlias ___END___

	// ====================================================================
	// 歌手紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_ARTIST_SEQUENCE)]
	public class TArtistSequence
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_ARTIST_SEQUENCE = "t_artist_sequence";
		public const String FIELD_NAME_ARTIST_SEQUENCE_ID = "artist_sequence_id";
		public const String FIELD_NAME_ARTIST_SEQUENCE_SEQUENCE = "artist_sequence_sequence";
		public const String FIELD_NAME_ARTIST_SEQUENCE_LINK_ID = "artist_sequence_link_id";
		public const String FIELD_NAME_ARTIST_SEQUENCE_IMPORT = "artist_sequence_import";
		public const String FIELD_NAME_ARTIST_SEQUENCE_INVALID = "artist_sequence_invalid";
		public const String FIELD_NAME_ARTIST_SEQUENCE_UPDATE_TIME = "artist_sequence_update_time";
		public const String FIELD_NAME_ARTIST_SEQUENCE_DIRTY = "artist_sequence_dirty";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// 楽曲 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_ARTIST_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String SongId { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_ARTIST_SEQUENCE_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_ARTIST_SEQUENCE_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_ARTIST_SEQUENCE_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_ARTIST_SEQUENCE_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_ARTIST_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// 人物 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_ARTIST_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String PersonId { get; set; }
	}
	// public class TArtistSequence ___END___

	// ====================================================================
	// 作詞者紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_LYRIST_SEQUENCE)]
	public class TLyristSequence
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_LYRIST_SEQUENCE = "t_lyrist_sequence";
		public const String FIELD_NAME_LYRIST_SEQUENCE_ID = "lyrist_sequence_id";
		public const String FIELD_NAME_LYRIST_SEQUENCE_SEQUENCE = "lyrist_sequence_sequence";
		public const String FIELD_NAME_LYRIST_SEQUENCE_LINK_ID = "lyrist_sequence_link_id";
		public const String FIELD_NAME_LYRIST_SEQUENCE_IMPORT = "lyrist_sequence_import";
		public const String FIELD_NAME_LYRIST_SEQUENCE_INVALID = "lyrist_sequence_invalid";
		public const String FIELD_NAME_LYRIST_SEQUENCE_UPDATE_TIME = "lyrist_sequence_update_time";
		public const String FIELD_NAME_LYRIST_SEQUENCE_DIRTY = "lyrist_sequence_dirty";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// 楽曲 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_LYRIST_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String SongId { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_LYRIST_SEQUENCE_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_LYRIST_SEQUENCE_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_LYRIST_SEQUENCE_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_LYRIST_SEQUENCE_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_LYRIST_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// 人物 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_LYRIST_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String PersonId { get; set; }
	}
	// public class TLyristSequence ___END___

	// ====================================================================
	// 作曲者紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_COMPOSER_SEQUENCE)]
	public class TComposerSequence
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_COMPOSER_SEQUENCE = "t_composer_sequence";
		public const String FIELD_NAME_COMPOSER_SEQUENCE_ID = "composer_sequence_id";
		public const String FIELD_NAME_COMPOSER_SEQUENCE_SEQUENCE = "composer_sequence_sequence";
		public const String FIELD_NAME_COMPOSER_SEQUENCE_LINK_ID = "composer_sequence_link_id";
		public const String FIELD_NAME_COMPOSER_SEQUENCE_IMPORT = "composer_sequence_import";
		public const String FIELD_NAME_COMPOSER_SEQUENCE_INVALID = "composer_sequence_invalid";
		public const String FIELD_NAME_COMPOSER_SEQUENCE_UPDATE_TIME = "composer_sequence_update_time";
		public const String FIELD_NAME_COMPOSER_SEQUENCE_DIRTY = "composer_sequence_dirty";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// 楽曲 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_COMPOSER_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String SongId { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_COMPOSER_SEQUENCE_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_COMPOSER_SEQUENCE_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_COMPOSER_SEQUENCE_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_COMPOSER_SEQUENCE_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_COMPOSER_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// 人物 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_COMPOSER_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String PersonId { get; set; }
	}
	// public class TComposerSequence ___END___

	// ====================================================================
	// 編曲者紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_ARRANGER_SEQUENCE)]
	public class TArrangerSequence
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_ARRANGER_SEQUENCE = "t_arranger_sequence";
		public const String FIELD_NAME_ARRANGER_SEQUENCE_ID = "arranger_sequence_id";
		public const String FIELD_NAME_ARRANGER_SEQUENCE_SEQUENCE = "arranger_sequence_sequence";
		public const String FIELD_NAME_ARRANGER_SEQUENCE_LINK_ID = "arranger_sequence_link_id";
		public const String FIELD_NAME_ARRANGER_SEQUENCE_IMPORT = "arranger_sequence_import";
		public const String FIELD_NAME_ARRANGER_SEQUENCE_INVALID = "arranger_sequence_invalid";
		public const String FIELD_NAME_ARRANGER_SEQUENCE_UPDATE_TIME = "arranger_sequence_update_time";
		public const String FIELD_NAME_ARRANGER_SEQUENCE_DIRTY = "arranger_sequence_dirty";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// 楽曲 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_ARRANGER_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String SongId { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_ARRANGER_SEQUENCE_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_ARRANGER_SEQUENCE_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_ARRANGER_SEQUENCE_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_ARRANGER_SEQUENCE_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_ARRANGER_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// 人物 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_ARRANGER_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String PersonId { get; set; }
	}
	// public class TArrangerSequence ___END___

	// ====================================================================
	// タイアップグループ紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TIE_UP_GROUP_SEQUENCE)]
	public class TTieUpGroupSequence
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_TIE_UP_GROUP_SEQUENCE = "t_tie_up_group_sequence";
		public const String FIELD_NAME_TIE_UP_GROUP_SEQUENCE_ID = "tie_up_group_sequence_id";
		public const String FIELD_NAME_TIE_UP_GROUP_SEQUENCE_SEQUENCE = "tie_up_group_sequence_sequence";
		public const String FIELD_NAME_TIE_UP_GROUP_SEQUENCE_LINK_ID = "tie_up_group_sequence_link_id";
		public const String FIELD_NAME_TIE_UP_GROUP_SEQUENCE_IMPORT = "tie_up_group_sequence_import";
		public const String FIELD_NAME_TIE_UP_GROUP_SEQUENCE_INVALID = "tie_up_group_sequence_invalid";
		public const String FIELD_NAME_TIE_UP_GROUP_SEQUENCE_UPDATE_TIME = "tie_up_group_sequence_update_time";
		public const String FIELD_NAME_TIE_UP_GROUP_SEQUENCE_DIRTY = "tie_up_group_sequence_dirty";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TBase
		// --------------------------------------------------------------------

		// タイアップ ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String TieUpId { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_SEQUENCE_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_SEQUENCE_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_SEQUENCE_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_SEQUENCE_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// タイアップグループ ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String TieUpGroupId { get; set; }
	}
	// public class TTieUpGroupSequence ___END___

	// ====================================================================
	// データベースプロパティーテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_PROPERTY)]
	public class TProperty
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_PROPERTY = "t_property";
		public const String FIELD_NAME_PROPERTY_APP_ID = "property_app_id";
		public const String FIELD_NAME_PROPERTY_APP_VER = "property_app_ver";

		// ====================================================================
		// フィールド
		// ====================================================================

		// データベース更新時のアプリケーション ID
		[Column(Name = FIELD_NAME_PROPERTY_APP_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String AppId { get; set; }

		// データベース更新時のアプリケーションのバージョン
		[Column(Name = FIELD_NAME_PROPERTY_APP_VER, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String AppVer { get; set; }
	}
	// public class TProperty ___END___

	// ====================================================================
	// 検出ファイルリストテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_FOUND)]
	public class TFound
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_FOUND = "t_found";
		public const String FIELD_NAME_FOUND_UID = "found_uid";
		public const String FIELD_NAME_FOUND_PATH = "found_path";
		public const String FIELD_NAME_FOUND_FOLDER = "found_folder";
		public const String FIELD_NAME_FOUND_HEAD = "found_head";
		public const String FIELD_NAME_FOUND_TITLE_RUBY = "found_title_ruby";
		public const String FIELD_NAME_FOUND_WORKER = "found_worker";
		public const String FIELD_NAME_FOUND_TRACK = "found_track";
		public const String FIELD_NAME_FOUND_SMART_TRACK_ON = "found_smart_track_on";
		public const String FIELD_NAME_FOUND_SMART_TRACK_OFF = "found_smart_track_off";
		public const String FIELD_NAME_FOUND_COMMENT = "found_comment";
		public const String FIELD_NAME_FOUND_LAST_WRITE_TIME = "found_last_write_time";
		public const String FIELD_NAME_FOUND_FILE_SIZE = "found_file_size";
		public const String FIELD_NAME_FOUND_ARTIST_NAME = "song_artist";   // ニコカラりすたーとの互換性維持
		public const String FIELD_NAME_FOUND_ARTIST_RUBY = "found_artist_ruby";
		public const String FIELD_NAME_FOUND_LYRIST_NAME = "found_lyrist_name";
		public const String FIELD_NAME_FOUND_LYRIST_RUBY = "found_lyrist_ruby";
		public const String FIELD_NAME_FOUND_COMPOSER_NAME = "found_composer_name";
		public const String FIELD_NAME_FOUND_COMPOSER_RUBY = "found_composer_ruby";
		public const String FIELD_NAME_FOUND_ARRANGER_NAME = "found_arranger_name";
		public const String FIELD_NAME_FOUND_ARRANGER_RUBY = "found_arranger_ruby";
		public const String FIELD_NAME_FOUND_TIE_UP_NAME = "program_name";  // ニコカラりすたーとの互換性維持
		public const String FIELD_NAME_FOUND_CATEGORY_NAME = "program_category";    // ニコカラりすたーとの互換性維持

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// TFound 独自
		// --------------------------------------------------------------------

		// ユニーク ID
		[Column(Name = FIELD_NAME_FOUND_UID, DbType = LinqUtils.DB_TYPE_INT64, CanBeNull = false, IsPrimaryKey = true)]
		public Int64 Uid { get; set; }

		// フルパス
		[Column(Name = FIELD_NAME_FOUND_PATH, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String Path { get; set; }

		// フォルダー（項目削除用：小文字に変換して格納）
		[Column(Name = FIELD_NAME_FOUND_FOLDER, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String Folder { get; set; }

		// 頭文字（通常は番組名の頭文字、通常はひらがな（濁点なし））
		[Column(Name = FIELD_NAME_FOUND_HEAD, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Head { get; set; }

		// ニコカラ制作者
		[Column(Name = FIELD_NAME_FOUND_WORKER, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Worker { get; set; }

		// トラック情報
		[Column(Name = FIELD_NAME_FOUND_TRACK, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Track { get; set; }

		// スマートトラック：オンボーカルがあるか
		[Column(Name = FIELD_NAME_FOUND_SMART_TRACK_ON, DbType = LinqUtils.DB_TYPE_BOOLEAN)]
		public Boolean SmartTrackOnVocal { get; set; }

		// スマートトラック：オフボーカルがあるか
		[Column(Name = FIELD_NAME_FOUND_SMART_TRACK_OFF, DbType = LinqUtils.DB_TYPE_BOOLEAN)]
		public Boolean SmartTrackOffVocal { get; set; }

		// 備考
		[Column(Name = FIELD_NAME_FOUND_COMMENT, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Comment { get; set; }

		// ファイル最終更新日時（修正ユリウス日）
		[Column(Name = FIELD_NAME_FOUND_LAST_WRITE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE)]
		public Double LastWriteTime { get; set; }

		// ファイルサイズ
		[Column(Name = FIELD_NAME_FOUND_FILE_SIZE, DbType = LinqUtils.DB_TYPE_INT64)]
		public Int64 FileSize { get; set; }

		// --------------------------------------------------------------------
		// TSong
		// --------------------------------------------------------------------

		// 楽曲名
		[Column(Name = TSong.FIELD_NAME_SONG_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SongName { get; set; }

		// 楽曲フリガナ
		[Column(Name = TSong.FIELD_NAME_SONG_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SongRuby { get; set; }

		// 摘要
		[Column(Name = TSong.FIELD_NAME_SONG_OP_ED, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SongOpEd { get; set; }

		// --------------------------------------------------------------------
		// TSong + TTieUp
		// --------------------------------------------------------------------

		// リリース日（修正ユリウス日）：TTieUp の値を優先
		[Column(Name = TSong.FIELD_NAME_SONG_RELEASE_DATE, DbType = LinqUtils.DB_TYPE_DOUBLE)]
		public Double SongReleaseDate { get; set; }

		// カテゴリー：TTieUp の値を優先
		[Column(Name = FIELD_NAME_FOUND_CATEGORY_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Category { get; set; }

		// --------------------------------------------------------------------
		// TPerson 由来
		// --------------------------------------------------------------------

		// 歌手名
		[Column(Name = FIELD_NAME_FOUND_ARTIST_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ArtistName { get; set; }

		// 歌手フリガナ
		[Column(Name = FIELD_NAME_FOUND_ARTIST_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ArtistRuby { get; set; }

		// 作詞者名
		[Column(Name = FIELD_NAME_FOUND_LYRIST_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String LyristName { get; set; }

		// 作詞者フリガナ
		[Column(Name = FIELD_NAME_FOUND_LYRIST_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String LyristRuby { get; set; }

		// 作曲者名
		[Column(Name = FIELD_NAME_FOUND_COMPOSER_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ComposerName { get; set; }

		// 作曲者フリガナ
		[Column(Name = FIELD_NAME_FOUND_COMPOSER_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ComposerRuby { get; set; }

		// 編曲者名
		[Column(Name = FIELD_NAME_FOUND_ARRANGER_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ArrangerName { get; set; }

		// 編曲者フリガナ
		[Column(Name = FIELD_NAME_FOUND_ARRANGER_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String ArrangerRuby { get; set; }

		// --------------------------------------------------------------------
		// TTieUp
		// --------------------------------------------------------------------

		// タイアップ名
		[Column(Name = FIELD_NAME_FOUND_TIE_UP_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String TieUpName { get; set; }

		// タイアップフリガナ
		[Column(Name = TTieUp.FIELD_NAME_TIE_UP_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String TieUpRuby { get; set; }

		// 年齢制限（○歳以上対象）
		[Column(Name = TTieUp.FIELD_NAME_TIE_UP_AGE_LIMIT, DbType = LinqUtils.DB_TYPE_INT32)]
		public Int32 TieUpAgeLimit { get; set; }

		// --------------------------------------------------------------------
		// TTieUpGroup
		// --------------------------------------------------------------------

		// タイアップグループ名
		[Column(Name = TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String TieUpGroupName { get; set; }

		// タイアップグループフリガナ
		[Column(Name = TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String TieUpGroupRuby { get; set; }

		// --------------------------------------------------------------------
		// TMaker
		// --------------------------------------------------------------------

		// 制作会社名
		[Column(Name = TMaker.FIELD_NAME_MAKER_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String MakerName { get; set; }

		// 制作会社フリガナ
		[Column(Name = TMaker.FIELD_NAME_MAKER_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String MakerRuby { get; set; }
	}
	// public class TFound ___END___
}
// namespace YukaLister.Shared ___END___