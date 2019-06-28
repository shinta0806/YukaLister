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
using System.Data.SQLite;

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
			CreatePropertyTable();
			mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "インメモリーデータベースを作成しました。");
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 検索結果テーブルを作成
		// --------------------------------------------------------------------
		private void CreateFoundTable()
		{
			using (SQLiteCommand aCmd = new SQLiteCommand(Connection))
			{
				// テーブル作成
				List<String> aUniques = new List<String>();
				aUniques.Add(TFound.FIELD_NAME_FOUND_UID);
				LinqUtils.CreateTable(aCmd, typeof(TFound), aUniques);

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
				LinqUtils.CreateIndex(aCmd, LinqUtils.TableName(typeof(TFound)), aIndices);
			}
		}


	}
	// public class YukariListDatabaseInMemory ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
