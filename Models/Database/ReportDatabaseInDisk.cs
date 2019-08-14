// ============================================================================
// 
// リスト問題報告データベース
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
	public class ReportDatabaseInDisk : DatabaseInDisk
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public ReportDatabaseInDisk(EnvironmentModel oEnvironment) : base(oEnvironment, oEnvironment.YukaListerSettings.ReportDbInDiskPath())
		{
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// データベース新規作成（既存がある場合は作成しない）
		// --------------------------------------------------------------------
		public void CreateDatabaseIfNeeded()
		{
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
		// データベース新規作成（既存がある場合はクリア）
		// --------------------------------------------------------------------
		private void CreateDatabase()
		{
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "リスト問題報告データベースを準備しています...");

			// クリア
			LinqUtils.DropAllTables(Connection);

			// 新規作成
			CreateReportTable();
			CreatePropertyTable();
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "リスト問題報告データベースを作成しました。");
		}

		// --------------------------------------------------------------------
		// リスト問題報告データベースの中にテーブルを作成
		// --------------------------------------------------------------------
		private void CreateReportTable()
		{
			using (SQLiteCommand aCmd = new SQLiteCommand(Connection))
			{
				List<String> aIndices = new List<String>();
				aIndices.Add(TReport.FIELD_NAME_REPORT_REGIST_TIME);
				aIndices.Add(TReport.FIELD_NAME_REPORT_STATUS);
				CreateTable(aCmd, typeof(TReport), aIndices);
			}
		}

	}
	// public class YukariListDatabaseInMemory ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
