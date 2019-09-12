// ============================================================================
// 
// 楽曲情報データベースデータベース
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using YukaLister.Models.SharedMisc;

namespace YukaLister.Models.Database
{
	public class MusicInfoDatabaseInDisk : DatabaseInDisk
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public MusicInfoDatabaseInDisk(EnvironmentModel oEnvironment) : base(oEnvironment, MusicInfoDbInDiskPath())
		{
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 旧バージョンのゆかりすたーを使用していてテーブルやレコードが不足している場合用
		// --------------------------------------------------------------------
		public void AddRemoveToOlderVersionIfNeeded()
		{
			// データベース作成
			CreateDatabaseIfNeeded();

			// カテゴリーマスターテーブルの調整
			using (DataContext aContext = new DataContext(Connection))
			{
				Table<TCategory> aTableCategory = aContext.GetTable<TCategory>();

				// 後発の "一般" レコードを挿入
				if (YlCommon.SelectMastersByName<TCategory>(aContext, "一般").Count == 0)
				{
					aTableCategory.InsertOnSubmit(CreateCategoryRecord(103, "一般", "イッパン"));
				}

				// 廃止となった "歌ってみた" レコードを削除
				aTableCategory.DeleteAllOnSubmit(YlCommon.SelectMastersByName<TCategory>(aContext, "歌ってみた"));

				aContext.SubmitChanges();
			}

			// タグ関係のテーブルを作成
			List<String> aTables = LinqUtils.Tables(Connection);
			if (!aTables.Contains(TTag.TABLE_NAME_TAG))
			{
				using (SQLiteCommand aCmd = new SQLiteCommand(Connection))
				{
					CreateTable(aCmd, typeof(TTag), TTag.FIELD_NAME_TAG_NAME);
					CreateTable(aCmd, typeof(TTagSequence));
				}
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースのバックアップを作成する
		// --------------------------------------------------------------------
		public void Backup()
		{
			Backup(MusicInfoDbInDiskPath());
		}

		// --------------------------------------------------------------------
		// カテゴリーテーブルのレコードを作成
		// --------------------------------------------------------------------
		public TCategory CreateCategoryRecord(Int32 oIdNumber, String oName, String oRuby = null, String oKeyword = null)
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
				// IRcBase
				Id = YlConstants.MUSIC_INFO_SYSTEM_ID_PREFIX + YlConstants.MUSIC_INFO_ID_SECOND_PREFIXES[(Int32)MusicInfoDbTables.TCategory] + oIdNumber.ToString("D3"),
				Import = false,
				Invalid = false,
				UpdateTime = YlConstants.INVALID_MJD,
				Dirty = true,

				// IRcMaster
				Name = oName,
				Ruby = oRuby,
				Keyword = oKeyword,
			};
		}

		// --------------------------------------------------------------------
		// データベース新規作成（既存がある場合はクリア）
		// --------------------------------------------------------------------
		public void CreateDatabase()
		{
			Backup();
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報データベースを準備しています...");

			// クリア
			LinqUtils.DropAllTables(Connection);

			// 新規作成
			CreateMusicInfoTable();
			CreatePropertyTable();

			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報データベースを作成しました。");
		}

		// --------------------------------------------------------------------
		// データベース新規作成（既存がある場合は作成しない）
		// --------------------------------------------------------------------
		public void CreateDatabaseIfNeeded()
		{
			//Debug.WriteLine("MusicInfoDatabaseInDisk.CreateDatabaseIfNeeded()");
			if (ValidPropertyExists())
			{
				// 既存のデータベースがある場合はクリアしない
				return;
			}

			CreateDatabase();
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// --------------------------------------------------------------------
		// フォルダー名
		// --------------------------------------------------------------------

		// --------------------------------------------------------------------
		// ファイル名
		// --------------------------------------------------------------------
		private const String FILE_NAME_MUSIC_INFO = "MusicInfo" + Common.FILE_EXT_SQLITE3;

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 楽曲情報データベースの中にテーブルを作成
		// --------------------------------------------------------------------
		private void CreateMusicInfoTable()
		{
			using (SQLiteCommand aCmd = new SQLiteCommand(Connection))
			{
				List<String> aIndices = new List<String>();

				// マスターテーブル
				aIndices.Clear();
				aIndices.Add(TSong.FIELD_NAME_SONG_NAME);
				aIndices.Add(TSong.FIELD_NAME_SONG_CATEGORY_ID);
				aIndices.Add(TSong.FIELD_NAME_SONG_OP_ED);
				CreateTable(aCmd, typeof(TSong), aIndices);

				CreateTable(aCmd, typeof(TPerson), TPerson.FIELD_NAME_PERSON_NAME);

				aIndices.Clear();
				aIndices.Add(TTieUp.FIELD_NAME_TIE_UP_NAME);
				aIndices.Add(TTieUp.FIELD_NAME_TIE_UP_CATEGORY_ID);
				CreateTable(aCmd, typeof(TTieUp), aIndices);

				CreateTable(aCmd, typeof(TCategory), TCategory.FIELD_NAME_CATEGORY_NAME);
				InsertCategoryDefaultRecords();

				CreateTable(aCmd, typeof(TTieUpGroup), TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_NAME);

				CreateTable(aCmd, typeof(TMaker), TMaker.FIELD_NAME_MAKER_NAME);

				CreateTable(aCmd, typeof(TTag), TTag.FIELD_NAME_TAG_NAME);

				// 別名テーブル
				CreateTable(aCmd, typeof(TSongAlias), TSongAlias.FIELD_NAME_SONG_ALIAS_ALIAS);

				CreateTable(aCmd, typeof(TPersonAlias), TPersonAlias.FIELD_NAME_PERSON_ALIAS_ALIAS);

				CreateTable(aCmd, typeof(TTieUpAlias), TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ALIAS);

				CreateTable(aCmd, typeof(TCategoryAlias), TCategoryAlias.FIELD_NAME_CATEGORY_ALIAS_ALIAS);

				CreateTable(aCmd, typeof(TTieUpGroupAlias), TTieUpGroupAlias.FIELD_NAME_TIE_UP_GROUP_ALIAS_ALIAS);

				CreateTable(aCmd, typeof(TMakerAlias), TMakerAlias.FIELD_NAME_MAKER_ALIAS_ALIAS);

				// 紐付テーブル
				CreateTable(aCmd, typeof(TArtistSequence));

				CreateTable(aCmd, typeof(TLyristSequence));

				CreateTable(aCmd, typeof(TComposerSequence));

				CreateTable(aCmd, typeof(TArrangerSequence));

				CreateTable(aCmd, typeof(TTieUpGroupSequence));

				CreateTable(aCmd, typeof(TTagSequence));
			}
		}

		// --------------------------------------------------------------------
		// カテゴリーマスターテーブルの既定レコードを挿入
		// ニコニコ動画のカテゴリータグおよび anison.info のカテゴリーから主要な物を抽出
		// --------------------------------------------------------------------
		private void InsertCategoryDefaultRecords()
		{
			using (DataContext aContext = new DataContext(Connection))
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
				// 102 は欠番（旧：歌ってみた）
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(103, "一般", "イッパン"));

				aContext.SubmitChanges();
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースファイルのフルパス
		// 基底クラス構築時にも呼び出されるので static 関数とする
		// --------------------------------------------------------------------
		private static String MusicInfoDbInDiskPath()
		{
			return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + YlConstants.FOLDER_NAME_DATABASE + FILE_NAME_MUSIC_INFO;
		}

	}
	// public class YukariListDatabaseInDisk ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
