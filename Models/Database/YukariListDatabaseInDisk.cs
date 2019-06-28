// ============================================================================
// 
// ゆかり用リストデータベース（ディスク）
// 
// ============================================================================

// ----------------------------------------------------------------------------
// インメモリーデータベースからコピーする時のみ使用される
// ----------------------------------------------------------------------------

using Shinta;

namespace YukaLister.Models.Database
{
	public class YukariListDatabaseInDisk : DatabaseInDisk
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public YukariListDatabaseInDisk(EnvironmentModel oEnvironment) : base(oEnvironment, oEnvironment.YukaListerSettings.YukariListDbInDiskPath())
		{
			// インメモリーデータベースからコピーする前提なのでテーブルは作成しない
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// インメモリーデータベースからコピー
		// --------------------------------------------------------------------
		public void CopyFromInMemory(YukariListDatabaseInMemory oInMemory)
		{
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ゆかり用データベースを出力しています...");
			LinqUtils.DropAllTables(Connection);
			oInMemory.Connection.BackupDatabase(Connection, "main", "main", -1, null, 0);
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ゆかり用データベースを出力しました。");
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================




	}
	// public class YukariListDatabaseInDisk ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
