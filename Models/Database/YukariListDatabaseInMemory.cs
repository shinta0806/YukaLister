// ============================================================================
// 
// ゆかり用リストデータベース（インメモリー）
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
using System.Linq;

namespace YukaLister.Models.Database
{
	public class YukariListDatabaseInMemory : DatabaseBase
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public YukariListDatabaseInMemory(EnvironmentModel oEnvironment) : base(oEnvironment, IN_MEMORY_PATH)
		{
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "インメモリーデータベースを準備しています...");

			// インメモリーデータベースなので、常にテーブルは新規作成となる
			CreateFoundTable();
			CreatePersonTables();
			CreateTagTables();
			CreatePropertyTable();
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "インメモリーデータベースを作成しました。");
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 
		// --------------------------------------------------------------------
		private void CopyFromMusicInfoDb<T>(DataContext oMusicInfoDbContext, DataContext oYukariListDbContext) where T: class
		{
			// 楽曲情報データベースからレコード全抽出
			Table<T> aTableInMusicInfoDb = oMusicInfoDbContext.GetTable<T>();
			IQueryable<T> aQueryResult =
					from x in aTableInMusicInfoDb
					select x;

			// ゆかり用リストデータベースにレコード書き込み
			Table<T> aTableInYukariListDb = oYukariListDbContext.GetTable<T>();
			aTableInYukariListDb.InsertAllOnSubmit(aQueryResult);
		}

		// --------------------------------------------------------------------
		// 検索結果テーブルを作成
		// --------------------------------------------------------------------
		private void CreateFoundTable()
		{
			using (SQLiteCommand aCmd = new SQLiteCommand(Connection))
			{
				// インデックス作成
				List<String> aIndices = new List<String>();
				aIndices.Add(TFound.FIELD_NAME_FOUND_PATH);
				aIndices.Add(TFound.FIELD_NAME_FOUND_FOLDER);
				aIndices.Add(TFound.FIELD_NAME_FOUND_HEAD);
				aIndices.Add(TFound.FIELD_NAME_FOUND_LAST_WRITE_TIME);
				aIndices.Add(TSong.FIELD_NAME_SONG_NAME);
				aIndices.Add(TSong.FIELD_NAME_SONG_RUBY);
				aIndices.Add(TSong.FIELD_NAME_SONG_RELEASE_DATE);
				aIndices.Add(TFound.FIELD_NAME_FOUND_TIE_UP_NAME);
				aIndices.Add(TTieUp.FIELD_NAME_TIE_UP_RUBY);
				aIndices.Add(TFound.FIELD_NAME_FOUND_CATEGORY_NAME);

				CreateTable(aCmd, typeof(TFound), aIndices);
			}
		}

		// --------------------------------------------------------------------
		// 人物関連のテーブルを作成し、楽曲情報データベースから内容をコピー
		// --------------------------------------------------------------------
		private void CreatePersonTables()
		{
			// テーブル作成
			using (SQLiteCommand aCmd = new SQLiteCommand(Connection))
			{
				CreateTable(aCmd, typeof(TPerson), TPerson.FIELD_NAME_PERSON_NAME);
				CreateTable(aCmd, typeof(TArtistSequence));
				CreateTable(aCmd, typeof(TComposerSequence));
			}

			// 楽曲情報データベースからコピー
			// 楽曲情報データベースにタグ情報が無い場合は例外が発生する
			try
			{
				using (DataContext aYukariListDbContext = new DataContext(Connection))
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
				using (DataContext aMusicInfoDbContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					CopyFromMusicInfoDb<TPerson>(aMusicInfoDbContext, aYukariListDbContext);
					CopyFromMusicInfoDb<TArtistSequence>(aMusicInfoDbContext, aYukariListDbContext);
					CopyFromMusicInfoDb<TComposerSequence>(aMusicInfoDbContext, aYukariListDbContext);
					aYukariListDbContext.SubmitChanges();
				}
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "人物情報コピー時エラー：\n" + oExcep.Message, true);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// タグ関連のテーブルを作成し、楽曲情報データベースから内容をコピー
		// --------------------------------------------------------------------
		private void CreateTagTables()
		{
			// テーブル作成
			using (SQLiteCommand aCmd = new SQLiteCommand(Connection))
			{
				CreateTable(aCmd, typeof(TTag), TTag.FIELD_NAME_TAG_NAME);
				CreateTable(aCmd, typeof(TTagSequence));
			}

			// 楽曲情報データベースからコピー
			// 楽曲情報データベースにタグ情報が無い場合は例外が発生する
			try
			{
				using (DataContext aYukariListDbContext = new DataContext(Connection))
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
				using (DataContext aMusicInfoDbContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					CopyFromMusicInfoDb<TTag>(aMusicInfoDbContext, aYukariListDbContext);
					CopyFromMusicInfoDb<TTagSequence>(aMusicInfoDbContext, aYukariListDbContext);
					aYukariListDbContext.SubmitChanges();
				}
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "タグ情報コピー時エラー：\n" + oExcep.Message, true);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}


	}
	// public class YukariListDatabaseInMemory ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
