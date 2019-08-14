// ============================================================================
// 
// データベーステーブル定義をカプセル化
// 
// ============================================================================

using Shinta;

using System;
using System.Data.Linq.Mapping;

using YukaLister.Models.SharedMisc;

namespace YukaLister.Models.Database
{
	// ====================================================================
	// 基礎テーブルのレコードインターフェース
	// ====================================================================

	public interface IRcBase
	{
		// ID
		String Id { get; set; }

		// インポートフラグ
		Boolean Import { get; set; }

		// 無効フラグ
		Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		Double UpdateTime { get; set; }

		// Dirty フラグ
		Boolean Dirty { get; set; }
	}

	// ====================================================================
	// マスターテーブルのレコードインターフェース
	// ====================================================================

	public interface IRcMaster : IRcBase
	{
		// 名
		String Name { get; set; }

		// フリガナ
		String Ruby { get; set; }

		// 検索ワード
		String Keyword { get; set; }

		// --------------------------------------------------------------------
		// 以下はデータベースに保存しない
		// --------------------------------------------------------------------

		// データベースアクセス用
		EnvironmentModel Environment { get; set; }

		// 同名の区別が付くように表示する
		Boolean AvoidSameName { get; set; }

		// 表示名
		String DisplayName { get; }
	}

	// ====================================================================
	// カテゴリー持ちテーブルのレコードインターフェース
	// ====================================================================

	public interface IRcCategorizable : IRcMaster
	{
		// カテゴリー ID ＜参照項目＞
		String CategoryId { get; set; }

		// リリース日（修正ユリウス日）
		Double ReleaseDate { get; set; }
	}

	// ====================================================================
	// 別名テーブルのレコードインターフェース
	// ====================================================================

	public interface IRcAlias : IRcBase
	{
		// 楽曲別名
		String Alias { get; set; }

		// 元の楽曲 ID ＜参照項目＞
		String OriginalId { get; set; }
	}

	// ====================================================================
	// 紐付テーブルのレコードインターフェース
	// ====================================================================

	public interface IRcSequence : IRcBase
	{
		// 連番
		Int32 Sequence { get; set; }

		// ID ＜参照項目＞
		String LinkId { get; set; }
	}

	// ====================================================================
	// 楽曲マスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_SONG)]
	public class TSong : IRcCategorizable
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
		// IRcBase
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
		// IRcMaster
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

		// データベースアクセス用
		public EnvironmentModel Environment { get; set; }

		// 同名の区別が付くように DisplayName を設定する
		public Boolean AvoidSameName { get; set; }

		// 表示名
		private String mDisplayName;
		public String DisplayName
		{
			get
			{
				if (String.IsNullOrEmpty(mDisplayName))
				{
					if (AvoidSameName)
					{
						mDisplayName = Name + "（" + (String.IsNullOrEmpty(Keyword) ? "キーワード無し" : Keyword) + "）";
					}
					else
					{
						mDisplayName = Name;
					}
				}
				return mDisplayName;
			}
		}

		// --------------------------------------------------------------------
		// IRcCategorizable
		// --------------------------------------------------------------------

		// カテゴリー ID ＜参照項目＞（タイアップ ID が null の場合のみ）
		[Column(Name = FIELD_NAME_SONG_CATEGORY_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String CategoryId { get; set; }

		// リリース日（修正ユリウス日）
		[Column(Name = FIELD_NAME_SONG_RELEASE_DATE, DbType = LinqUtils.DB_TYPE_DOUBLE)]
		public Double ReleaseDate { get; set; }

		// --------------------------------------------------------------------
		// TSong 独自項目
		// --------------------------------------------------------------------

		// タイアップ ID ＜参照項目＞
		[Column(Name = FIELD_NAME_SONG_TIE_UP_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String TieUpId { get; set; }

		// 摘要
		[Column(Name = FIELD_NAME_SONG_OP_ED, DbType = LinqUtils.DB_TYPE_STRING)]
		public String OpEd { get; set; }
	}
	// public class TSong ___END___

	// ====================================================================
	// 人物マスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_PERSON)]
	public class TPerson : IRcMaster
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
		// IRcBase
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
		// IRcMaster
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

		// データベースアクセス用
		public EnvironmentModel Environment { get; set; }

		// 同名の区別が付くように DisplayName を設定する
		public Boolean AvoidSameName { get; set; }

		// 表示名
		private String mDisplayName;
		public String DisplayName
		{
			get
			{
				if (String.IsNullOrEmpty(mDisplayName))
				{
					if (AvoidSameName)
					{
						mDisplayName = Name + "（" + (String.IsNullOrEmpty(Keyword) ? "キーワード無し" : Keyword) + "）";
					}
					else
					{
						mDisplayName = Name;
					}
				}
				return mDisplayName;
			}
		}
	}
	// public class TPerson ___END___

	// ====================================================================
	// タイアップマスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TIE_UP)]
	public class TTieUp : IRcCategorizable
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
		// IRcBase
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
		// IRcMaster
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

		// データベースアクセス用
		public EnvironmentModel Environment { get; set; }

		// 同名の区別が付くように DisplayName を設定する
		public Boolean AvoidSameName { get; set; }

		// 表示名
		private String mDisplayName;
		public String DisplayName
		{
			get
			{
				if (String.IsNullOrEmpty(mDisplayName))
				{
					if (AvoidSameName)
					{
						TCategory aCategory;
						using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
						{
							aCategory = YlCommon.SelectBaseById<TCategory>(aMusicInfoDbInDisk.Connection, CategoryId);
						}
						mDisplayName = Name + "（" + (String.IsNullOrEmpty(aCategory?.Name) ? "カテゴリー無し" : aCategory?.Name) + ", "
								+ (String.IsNullOrEmpty(Keyword) ? "キーワード無し" : Keyword) + "）";
					}
					else
					{
						mDisplayName = Name;
					}
				}
				return mDisplayName;
			}
		}

		// --------------------------------------------------------------------
		// IRcCategorizable
		// --------------------------------------------------------------------

		// カテゴリー ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TIE_UP_CATEGORY_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String CategoryId { get; set; }

		// リリース日（修正ユリウス日）
		[Column(Name = FIELD_NAME_TIE_UP_RELEASE_DATE, DbType = LinqUtils.DB_TYPE_DOUBLE)]
		public Double ReleaseDate { get; set; }

		// --------------------------------------------------------------------
		// TTieUp 独自項目
		// --------------------------------------------------------------------

		// 制作会社 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TIE_UP_MAKER_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String MakerId { get; set; }

		// 年齢制限（○歳以上対象）
		[Column(Name = FIELD_NAME_TIE_UP_AGE_LIMIT, DbType = LinqUtils.DB_TYPE_INT32)]
		public Int32 AgeLimit { get; set; }
	}
	// public class TTieUp ___END___

	// ====================================================================
	// カテゴリーマスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_CATEGORY)]
	public class TCategory : IRcMaster
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
		// IRcBase
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
		// IRcMaster
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

		// データベースアクセス用
		public EnvironmentModel Environment { get; set; }

		// 同名の区別が付くように DisplayName を設定する
		public Boolean AvoidSameName { get; set; }

		// 表示名
		private String mDisplayName;
		public String DisplayName
		{
			get
			{
				if (String.IsNullOrEmpty(mDisplayName))
				{
					if (AvoidSameName)
					{
						mDisplayName = Name + "（" + (String.IsNullOrEmpty(Keyword) ? "キーワード無し" : Keyword) + "）";
					}
					else
					{
						mDisplayName = Name;
					}
				}
				return mDisplayName;
			}
		}
	}
	// public class TCategory ___END___

	// ====================================================================
	// タイアップグループマスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TIE_UP_GROUP)]
	public class TTieUpGroup : IRcMaster
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
		// IRcBase
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
		// IRcMaster
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

		// データベースアクセス用
		public EnvironmentModel Environment { get; set; }

		// 同名の区別が付くように DisplayName を設定する
		public Boolean AvoidSameName { get; set; }

		// 表示名
		private String mDisplayName;
		public String DisplayName
		{
			get
			{
				if (String.IsNullOrEmpty(mDisplayName))
				{
					if (AvoidSameName)
					{
						mDisplayName = Name + "（" + (String.IsNullOrEmpty(Keyword) ? "キーワード無し" : Keyword) + "）";
					}
					else
					{
						mDisplayName = Name;
					}
				}
				return mDisplayName;
			}
		}
	}
	// public class TTieUpGroup ___END___

	// ====================================================================
	// 制作会社マスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_MAKER)]
	public class TMaker : IRcMaster
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
		// IRcBase
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
		// IRcMaster
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

		// データベースアクセス用
		public EnvironmentModel Environment { get; set; }

		// 同名の区別が付くように DisplayName を設定する
		public Boolean AvoidSameName { get; set; }

		// 表示名
		private String mDisplayName;
		public String DisplayName
		{
			get
			{
				if (String.IsNullOrEmpty(mDisplayName))
				{
					if (AvoidSameName)
					{
						mDisplayName = Name + "（" + (String.IsNullOrEmpty(Keyword) ? "キーワード無し" : Keyword) + "）";
					}
					else
					{
						mDisplayName = Name;
					}
				}
				return mDisplayName;
			}
		}
	}

	// ====================================================================
	// タグマスターテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TAG)]
	public class TTag : IRcMaster
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_TAG = "t_tag";
		public const String FIELD_NAME_TAG_ID = "tag_id";
		public const String FIELD_NAME_TAG_IMPORT = "tag_import";
		public const String FIELD_NAME_TAG_INVALID = "tag_invalid";
		public const String FIELD_NAME_TAG_UPDATE_TIME = "tag_update_time";
		public const String FIELD_NAME_TAG_DIRTY = "tag_dirty";
		public const String FIELD_NAME_TAG_NAME = "tag_name";
		public const String FIELD_NAME_TAG_RUBY = "tag_ruby";
		public const String FIELD_NAME_TAG_KEYWORD = "tag_keyword";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// IRcBase
		// --------------------------------------------------------------------

		// タグ ID
		[Column(Name = FIELD_NAME_TAG_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_TAG_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_TAG_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_TAG_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_TAG_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// IRcMaster
		// --------------------------------------------------------------------

		// タグ名
		[Column(Name = FIELD_NAME_TAG_NAME, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Name { get; set; }

		// タグフリガナ
		[Column(Name = FIELD_NAME_TAG_RUBY, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Ruby { get; set; }

		// 検索ワード
		[Column(Name = FIELD_NAME_TAG_KEYWORD, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Keyword { get; set; }

		// データベースアクセス用
		public EnvironmentModel Environment { get; set; }

		// 同名の区別が付くように DisplayName を設定する
		public Boolean AvoidSameName { get; set; }

		// 表示名
		private String mDisplayName;
		public String DisplayName
		{
			get
			{
				if (String.IsNullOrEmpty(mDisplayName))
				{
					if (AvoidSameName)
					{
						mDisplayName = Name + "（" + (String.IsNullOrEmpty(Keyword) ? "キーワード無し" : Keyword) + "）";
					}
					else
					{
						mDisplayName = Name;
					}
				}
				return mDisplayName;
			}
		}
	}
	// public class TTag ___END___

	// ====================================================================
	// 楽曲別名テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_SONG_ALIAS)]
	public class TSongAlias : IRcAlias
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
		// IRcBase
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
		// IRcAlias
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
	public class TPersonAlias : IRcAlias
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
		// IRcBase
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
		// IRcAlias
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
	public class TTieUpAlias : IRcAlias
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
		// IRcBase
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
		// IRcAlias
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
	public class TCategoryAlias : IRcAlias
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
		// IRcBase
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
		// IRcAlias
		// --------------------------------------------------------------------

		// カテゴリー別名
		[Column(Name = FIELD_NAME_CATEGORY_ALIAS_ALIAS, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Alias { get; set; }

		// 元のカテゴリー ID ＜参照項目＞
		[Column(Name = FIELD_NAME_CATEGORY_ALIAS_ORIGINAL_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String OriginalId { get; set; }
	}
	// public class TCategoryAlias ___END___

	// ====================================================================
	// タイアップグループ別名テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TIE_UP_GROUP_ALIAS)]
	public class TTieUpGroupAlias : IRcAlias
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
		// IRcBase
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
		// IRcAlias
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
	public class TMakerAlias : IRcAlias
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
		// IRcBase
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
		// IRcAlias
		// --------------------------------------------------------------------

		// 制作会社別名
		[Column(Name = FIELD_NAME_MAKER_ALIAS_ALIAS, DbType = LinqUtils.DB_TYPE_STRING)]
		public String Alias { get; set; }

		// 元の制作会社 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_MAKER_ALIAS_ORIGINAL_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String OriginalId { get; set; }
	}
	// public class TMakerAlias ___END___

	// ====================================================================
	// 歌手紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_ARTIST_SEQUENCE)]
	public class TArtistSequence : IRcSequence
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
		// IRcBase
		// --------------------------------------------------------------------

		// 楽曲 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_ARTIST_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

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
		// IRcSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_ARTIST_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// 人物 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_ARTIST_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String LinkId { get; set; }
	}
	// public class TArtistSequence ___END___

	// ====================================================================
	// 作詞者紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_LYRIST_SEQUENCE)]
	public class TLyristSequence : IRcSequence
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
		// IRcBase
		// --------------------------------------------------------------------

		// 楽曲 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_LYRIST_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

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
		// IRcSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_LYRIST_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// 人物 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_LYRIST_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String LinkId { get; set; }
	}
	// public class TLyristSequence ___END___

	// ====================================================================
	// 作曲者紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_COMPOSER_SEQUENCE)]
	public class TComposerSequence : IRcSequence
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
		// IRcBase
		// --------------------------------------------------------------------

		// 楽曲 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_COMPOSER_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

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
		// IRcSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_COMPOSER_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// 人物 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_COMPOSER_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String LinkId { get; set; }
	}
	// public class TComposerSequence ___END___

	// ====================================================================
	// 編曲者紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_ARRANGER_SEQUENCE)]
	public class TArrangerSequence : IRcSequence
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
		// IRcBase
		// --------------------------------------------------------------------

		// 楽曲 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_ARRANGER_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

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
		// IRcSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_ARRANGER_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// 人物 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_ARRANGER_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String LinkId { get; set; }
	}
	// public class TArrangerSequence ___END___

	// ====================================================================
	// タイアップグループ紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TIE_UP_GROUP_SEQUENCE)]
	public class TTieUpGroupSequence : IRcSequence
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
		// IRcBase
		// --------------------------------------------------------------------

		// タイアップ ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

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
		// IRcSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// タイアップグループ ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TIE_UP_GROUP_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String LinkId { get; set; }
	}
	// public class TTieUpGroupSequence ___END___

	// ====================================================================
	// タグ紐付テーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_TAG_SEQUENCE)]
	public class TTagSequence : IRcSequence
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_TAG_SEQUENCE = "t_tag_sequence";
		public const String FIELD_NAME_TAG_SEQUENCE_ID = "tag_sequence_id";
		public const String FIELD_NAME_TAG_SEQUENCE_SEQUENCE = "tag_sequence_sequence";
		public const String FIELD_NAME_TAG_SEQUENCE_LINK_ID = "tag_sequence_link_id";
		public const String FIELD_NAME_TAG_SEQUENCE_IMPORT = "tag_sequence_import";
		public const String FIELD_NAME_TAG_SEQUENCE_INVALID = "tag_sequence_invalid";
		public const String FIELD_NAME_TAG_SEQUENCE_UPDATE_TIME = "tag_sequence_update_time";
		public const String FIELD_NAME_TAG_SEQUENCE_DIRTY = "tag_sequence_dirty";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// IRcBase
		// --------------------------------------------------------------------

		// 楽曲 ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TAG_SEQUENCE_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_TAG_SEQUENCE_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_TAG_SEQUENCE_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_TAG_SEQUENCE_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_TAG_SEQUENCE_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// IRcSequence
		// --------------------------------------------------------------------

		// 連番
		[Column(Name = FIELD_NAME_TAG_SEQUENCE_SEQUENCE, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false, IsPrimaryKey = true)]
		public Int32 Sequence { get; set; }

		// タグ ID ＜参照項目＞
		[Column(Name = FIELD_NAME_TAG_SEQUENCE_LINK_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String LinkId { get; set; }
	}
	// public class TTagSequence ___END___

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

		// フルパス（shorten 形式）
		[Column(Name = FIELD_NAME_FOUND_PATH, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String Path { get; set; }

		// フォルダー（項目削除用：小文字に変換して格納、shorten 形式）
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

		// 楽曲 ID
		[Column(Name = TSong.FIELD_NAME_SONG_ID, DbType = LinqUtils.DB_TYPE_STRING)]
		public String SongId { get; set; }

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

		// --------------------------------------------------------------------
		// ViewTFoundsWindow 表示用
		// --------------------------------------------------------------------

		// パス無しのファイル名
		public String FileName
		{
			get
			{
				return System.IO.Path.GetFileName(Path);
			}
		}

		// スマートトラック
		public String SmartTrack
		{
			get
			{
				return (SmartTrackOnVocal ? YlConstants.SMART_TRACK_VALID_MARK : YlConstants.SMART_TRACK_INVALID_MARK) + "/"
						+ (SmartTrackOffVocal ? YlConstants.SMART_TRACK_VALID_MARK : YlConstants.SMART_TRACK_INVALID_MARK);
			}
		}

	}
	// public class TFound ___END___

	// ====================================================================
	// サムネイルキャッシュテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_CACHE_THUMB)]
	public class TCacheThumb
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_CACHE_THUMB = "t_cache_thumb";
		public const String FIELD_NAME_CACHE_THUMB_UID = "cache_thumb_uid";
		public const String FIELD_NAME_CACHE_THUMB_FILE_NAME = "cache_thumb_file_name";
		public const String FIELD_NAME_CACHE_THUMB_WIDTH = "cache_thumb_width";
		public const String FIELD_NAME_CACHE_THUMB_IMAGE = "cache_thumb_image";
		public const String FIELD_NAME_CACHE_THUMB_FILE_LAST_WRITE_TIME = "cache_thumb_file_last_write_time";
		public const String FIELD_NAME_CACHE_THUMB_THUMB_LAST_WRITE_TIME = "cache_thumb_thumb_last_write_time";

		// ====================================================================
		// フィールド
		// ====================================================================

		// キャッシュサムネイルユニーク ID
		[Column(Name = FIELD_NAME_CACHE_THUMB_UID, DbType = LinqUtils.DB_TYPE_INT64, CanBeNull = false, IsPrimaryKey = true)]
		public Int64 Uid { get; set; }

		// ファイル名（パス無し）
		[Column(Name = FIELD_NAME_CACHE_THUMB_FILE_NAME, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String FileName { get; set; }

		// サムネイル横サイズ
		[Column(Name = FIELD_NAME_CACHE_THUMB_WIDTH, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false)]
		public Int32 Width { get; set; }

		// サムネイル画像データ
		[Column(Name = FIELD_NAME_CACHE_THUMB_IMAGE, DbType = LinqUtils.DB_TYPE_BLOB, CanBeNull = false)]
		public Byte[] Image { get; set; }

		// 動画ファイル最終更新日時（修正ユリウス日）
		[Column(Name = FIELD_NAME_CACHE_THUMB_FILE_LAST_WRITE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double FileLastWriteTime { get; set; }

		// サムネイル最終更新日時（修正ユリウス日）
		[Column(Name = FIELD_NAME_CACHE_THUMB_THUMB_LAST_WRITE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double ThumbLastWriteTime { get; set; }
	}
	// public class TCacheThumb ___END___

	// ====================================================================
	// 報告テーブルテーブル
	// ====================================================================

	[Table(Name = TABLE_NAME_REPORT)]
	public class TReport : IRcBase
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_REPORT = "t_report";
		public const String FIELD_NAME_REPORT_ID = "report_id";
		public const String FIELD_NAME_REPORT_IMPORT = "report_import";
		public const String FIELD_NAME_REPORT_INVALID = "report_invalid";
		public const String FIELD_NAME_REPORT_UPDATE_TIME = "report_update_time";
		public const String FIELD_NAME_REPORT_DIRTY = "report_dirty";
		public const String FIELD_NAME_REPORT_PATH = "report_path";
		public const String FIELD_NAME_REPORT_ADJUST_KEY = "report_adjust_key";
		public const String FIELD_NAME_REPORT_BAD_VALUE = "report_bad_value";
		public const String FIELD_NAME_REPORT_ADJUST_VALUE = "report_adjust_value";
		public const String FIELD_NAME_REPORT_REPORTER_COMMENT = "report_reporter_comment";
		public const String FIELD_NAME_REPORT_BY = "report_by";
		public const String FIELD_NAME_REPORT_IP = "report_ip";
		public const String FIELD_NAME_REPORT_HOST = "report_host";
		public const String FIELD_NAME_REPORT_REGIST_TIME = "report_regist_time";
		public const String FIELD_NAME_REPORT_STATUS_COMMENT = "report_status_comment";
		public const String FIELD_NAME_REPORT_STATUS = "report_status";
		public const String FIELD_NAME_REPORT_STATUS_BY = "report_status_by";

		// ====================================================================
		// フィールド
		// ====================================================================

		// --------------------------------------------------------------------
		// IRcBase
		// --------------------------------------------------------------------

		// 報告 ID
		[Column(Name = FIELD_NAME_REPORT_ID, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false, IsPrimaryKey = true)]
		public String Id { get; set; }

		// インポートフラグ
		[Column(Name = FIELD_NAME_REPORT_IMPORT, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Import { get; set; }

		// 無効フラグ
		[Column(Name = FIELD_NAME_REPORT_INVALID, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Invalid { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		[Column(Name = FIELD_NAME_REPORT_UPDATE_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double UpdateTime { get; set; }

		// Dirty フラグ
		[Column(Name = FIELD_NAME_REPORT_DIRTY, DbType = LinqUtils.DB_TYPE_BOOLEAN, CanBeNull = false)]
		public Boolean Dirty { get; set; }

		// --------------------------------------------------------------------
		// TReport 独自項目
		// --------------------------------------------------------------------

		// 対象ファイルフルパス
		[Column(Name = FIELD_NAME_REPORT_PATH, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String Path { get; set; }

		// 修正項目インデックス
		[Column(Name = FIELD_NAME_REPORT_ADJUST_KEY, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false)]
		public Int32 AdjustKey { get; set; }

		// 修正前の値
		[Column(Name = FIELD_NAME_REPORT_BAD_VALUE, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = true)]
		public String BadValue { get; set; }

		// 修正後の値
		[Column(Name = FIELD_NAME_REPORT_ADJUST_VALUE, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String AdjustValue { get; set; }

		// 報告コメント
		[Column(Name = FIELD_NAME_REPORT_REPORTER_COMMENT, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = true)]
		public String ReporterComment { get; set; }

		// 報告者名
		[Column(Name = FIELD_NAME_REPORT_BY, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String By { get; set; }

		// 報告者 IP
		[Column(Name = FIELD_NAME_REPORT_IP, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = false)]
		public String Ip { get; set; }

		// 報告者ホスト
		[Column(Name = FIELD_NAME_REPORT_HOST, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = true)]
		public String Host { get; set; }

		// 報告日時 UTC（最初に登録した時の日時）
		[Column(Name = FIELD_NAME_REPORT_REGIST_TIME, DbType = LinqUtils.DB_TYPE_DOUBLE, CanBeNull = false)]
		public Double RegistTime { get; set; }

		// 対応コメント
		[Column(Name = FIELD_NAME_REPORT_STATUS_COMMENT, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = true)]
		public String StatusComment { get; set; }

		// 対応状況インデックス
		[Column(Name = FIELD_NAME_REPORT_STATUS, DbType = LinqUtils.DB_TYPE_INT32, CanBeNull = false)]
		public Int32 Status { get; set; }

		// 対応者
		[Column(Name = FIELD_NAME_REPORT_STATUS_BY, DbType = LinqUtils.DB_TYPE_STRING, CanBeNull = true)]
		public String StatusBy { get; set; }

		// --------------------------------------------------------------------
		// ViewTReportsWindow 表示用
		// --------------------------------------------------------------------

		// パス無しのファイル名
		public String FileName
		{
			get
			{
				return System.IO.Path.GetFileName(Path);
			}
		}

		// 修正項目名
		public String AdjustKeyName
		{
			get
			{
				if (AdjustKey < (Int32)ReportAdjustKey.Invalid || AdjustKey >= (Int32)ReportAdjustKey.__End__)
				{
					return null;
				}
				return YlConstants.REPORT_ADJUST_KEY_NAMES[AdjustKey];
			}
		}

		// 報告日文字列
		public String RegistDateString
		{
			get
			{
				DateTime aUtc = JulianDay.ModifiedJulianDateToDateTime(RegistTime);
				return TimeZoneInfo.ConvertTimeFromUtc(aUtc, TimeZoneInfo.Local).ToString(YlConstants.DATE_FORMAT);
			}
		}

		// 対応状況名
		public String StatusName
		{
			get
			{
				if (Status < 0 || Status >= (Int32)ReportStatus.__End__)
				{
					return null;
				}
				return YlConstants.REPORT_STATUS_NAMES[Status];
			}
		}
	}
	// public class TReport ___END___
}
// namespace YukaLister.Shared ___END___
