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
using System.Threading.Tasks;

using YukaLister.Models.SharedMisc;

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
			CreateTables();
			CreatePropertyTable();

			// async を待機しない
			Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(CopyFromMusicInfoDbByWorker, mCopyTaskLock, null, mEnvironment.LogWriter);

			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "インメモリーデータベースを作成しました。");
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 楽曲情報データベースからの内容コピーが終了したか
		public Boolean IsCopied { get; set; }

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// コピータスクが多重起動されるのを抑止する
		private Object mCopyTaskLock = new Object();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 楽曲情報データベースから内容をコピー
		// --------------------------------------------------------------------
		private void CopyFromMusicInfoDb<T>(DataContext oMusicInfoDbContext, DataContext oYukariListDbContext) where T : class
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
		// 楽曲情報データベースから内容をコピー
		// ワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void CopyFromMusicInfoDbByWorker(Object oDummy)
		{
			// 楽曲情報データベースからコピー
			// 楽曲情報データベースにタグ情報等が無い場合は例外が発生する
			try
			{
				using (DataContext aYukariListDbContext = new DataContext(Connection))
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
				using (DataContext aMusicInfoDbContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					// 人物関連のデータをコピー
					CopyFromMusicInfoDb<TPerson>(aMusicInfoDbContext, aYukariListDbContext);
					CopyFromMusicInfoDb<TArtistSequence>(aMusicInfoDbContext, aYukariListDbContext);
					CopyFromMusicInfoDb<TComposerSequence>(aMusicInfoDbContext, aYukariListDbContext);
					aYukariListDbContext.SubmitChanges();

					// タグ関連のデータをコピー
					CopyFromMusicInfoDb<TTag>(aMusicInfoDbContext, aYukariListDbContext);
					CopyFromMusicInfoDb<TTagSequence>(aMusicInfoDbContext, aYukariListDbContext);
					aYukariListDbContext.SubmitChanges();
				}
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Error, "楽曲情報データベースからの内容コピー時エラー：\n" + oExcep.Message, true);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}

			// コピーに失敗した場合でもフラグは立てる（他のスレッドが作業を進められるように）
			IsCopied = true;
		}

		// --------------------------------------------------------------------
		// 検索結果等のテーブルを作成
		// --------------------------------------------------------------------
		private void CreateTables()
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

				// 検索結果テーブル
				CreateTable(aCmd, typeof(TFound), aIndices);

				// 人物関連のテーブル
				CreateTable(aCmd, typeof(TPerson), TPerson.FIELD_NAME_PERSON_NAME);
				CreateTable(aCmd, typeof(TArtistSequence));
				CreateTable(aCmd, typeof(TComposerSequence));

				// タグ関連のテーブル
				CreateTable(aCmd, typeof(TTag), TTag.FIELD_NAME_TAG_NAME);
				CreateTable(aCmd, typeof(TTagSequence));
			}
		}

	}
	// public class YukariListDatabaseInMemory ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
