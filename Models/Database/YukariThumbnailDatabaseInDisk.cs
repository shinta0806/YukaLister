// ============================================================================
// 
// ゆかり用サムネイルデータベース
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace YukaLister.Models.Database
{
	public class YukariThumbnailDatabaseInDisk : DatabaseInDisk
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public YukariThumbnailDatabaseInDisk(EnvironmentModel oEnvironment) : base(oEnvironment, oEnvironment.YukaListerSettings.YukariThumbDbInDiskPath())
		{
			//Debug.WriteLine("YukariThumbnailDatabaseInDisk constructor");
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// データベース新規作成（既存がある場合は作成しない）
		// --------------------------------------------------------------------
		public void CreateDatabaseIfNeeded()
		{
			//Debug.WriteLine("YukariThumbnailDatabaseInDisk.CreateDatabaseIfNeeded()");
			if (ValidPropertyExists())
			{
				// 既存のデータベースがある場合はクリアしない
				return;
			}

			CreateDatabase();
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ゆかり用サムネイルデータベースの中にテーブルを作成
		// --------------------------------------------------------------------
		private void CreateCacheThumbTable()
		{
			using (SQLiteCommand aCmd = new SQLiteCommand(Connection))
			{
				// テーブル作成（複数カラムでユニークなので CreateTable() は使えない）
				List<String> aUniques = new List<String>();
				aUniques.Add(TCacheThumb.FIELD_NAME_CACHE_THUMB_UID);
				aUniques.Add(TCacheThumb.FIELD_NAME_CACHE_THUMB_FILE_NAME + "," + TCacheThumb.FIELD_NAME_CACHE_THUMB_WIDTH);
				LinqUtils.CreateTable(aCmd, typeof(TCacheThumb), aUniques);

				// インデックス作成
				List<String> aIndices = new List<String>();
				aIndices.Add(TCacheThumb.FIELD_NAME_CACHE_THUMB_FILE_NAME);
				aIndices.Add(TCacheThumb.FIELD_NAME_CACHE_THUMB_THUMB_LAST_WRITE_TIME);
				LinqUtils.CreateIndex(aCmd, LinqUtils.TableName(typeof(TCacheThumb)), aIndices);
			}
		}

		// --------------------------------------------------------------------
		// データベース新規作成（既存がある場合はクリア）
		// --------------------------------------------------------------------
		private void CreateDatabase()
		{
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ゆかり用サムネイルデータベースを準備しています...");

			// クリア
			LinqUtils.DropAllTables(Connection);

			// 新規作成
			CreateCacheThumbTable();
			CreatePropertyTable();
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ゆかり用サムネイルデータベースを作成しました。");
		}

	}
	// public class YukariListDatabaseInMemory ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
