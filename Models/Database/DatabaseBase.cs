// ============================================================================
// 
// SQLite データベースを管理するクラスの基礎
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
using System.Linq;
using System.Reflection;
using YukaLister.Models.SharedMisc;

namespace YukaLister.Models.Database
{
	// ====================================================================
	// データベース基底クラス
	// ====================================================================

	public class DatabaseBase : IDisposable
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 接続
		public SQLiteConnection Connection { get; private set; }

		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		protected DatabaseBase(EnvironmentModel oEnvironment, String oPath)
		{
			mEnvironment = oEnvironment;
			Debug.Assert(mEnvironment != null, "DatabaseBase() mEnvironment is null");

			// ディスク用データベースの場合は保存先のフォルダーを作成
			if (!oPath.Equals(IN_MEMORY_PATH))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(oPath));
			}

			SQLiteConnectionStringBuilder aConnectionString = new SQLiteConnectionStringBuilder
			{
				DataSource = oPath,
				BusyTimeout = 100, // default = 0
				PrepareRetries = 10, // default = 0
			};
			Connection = new SQLiteConnection(aConnectionString.ToString());
			Connection.Open();
		}

		// --------------------------------------------------------------------
		// デストラクター
		// --------------------------------------------------------------------
		~DatabaseBase()
		{
			Dispose(false);
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// データベースの中にプロパティーテーブルを作成
		// --------------------------------------------------------------------
		public void CreatePropertyTable()
		{
			using (SQLiteCommand aCmd = new SQLiteCommand(Connection))
			{
				// テーブル作成
				LinqUtils.CreateTable(aCmd, typeof(TProperty));
			}

			// 更新
			UpdateProperty();
		}

		// --------------------------------------------------------------------
		// IDisposable.Dispose()
		// --------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// ====================================================================
		// protected メンバー定数
		// ====================================================================

		// インメモリーデータベースの仮想パス
		protected const String IN_MEMORY_PATH = ":memory:";

		// ====================================================================
		// protected メンバー変数
		// ====================================================================

		// 環境設定類
		protected EnvironmentModel mEnvironment;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// データベースの中にテーブルを作成し、インデックスを作成
		// --------------------------------------------------------------------
		protected void CreateTable(SQLiteCommand oCmd, Type oTypeOfTable, String oIndexColumn = null)
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
			CreateTable(oCmd, oTypeOfTable, aIndices);
		}

		// --------------------------------------------------------------------
		// データベースの中にテーブルを作成し、インデックスを作成
		// --------------------------------------------------------------------
		protected void CreateTable(SQLiteCommand oCmd, Type oTypeOfTable, List<String> oIndices)
		{
			// テーブル作成
			LinqUtils.CreateTable(oCmd, oTypeOfTable);

			// インデックス作成（JOIN および検索の高速化）
			LinqUtils.CreateIndex(oCmd, LinqUtils.TableName(oTypeOfTable), oIndices);
		}

		// --------------------------------------------------------------------
		// リソース解放
		// --------------------------------------------------------------------
		protected virtual void Dispose(Boolean oIsDisposing)
		{
			if (mIsDisposed)
			{
				return;
			}

			// マネージドリソース解放
			if (oIsDisposing)
			{
				DisposeManagedResource(Connection);
			}

			// アンマネージドリソース解放
			// 今のところ無し

			mIsDisposed = true;
		}

		// --------------------------------------------------------------------
		// マネージドリソース解放
		// --------------------------------------------------------------------
		protected void DisposeManagedResource(IDisposable oResource)
		{
			if (oResource != null)
			{
				oResource.Dispose();
			}
		}

		// --------------------------------------------------------------------
		// データベースのプロパティーを取得
		// --------------------------------------------------------------------
		protected TProperty Property()
		{
			try
			{
				using (DataContext aContext = new DataContext(Connection))
				{
					Table<TProperty> aTableProperty = aContext.GetTable<TProperty>();

					IQueryable<TProperty> aQueryResult =
							from x in aTableProperty
							select x;
					foreach (TProperty aProperty in aQueryResult)
					{
						return aProperty;
					}
				}
			}
			catch (Exception)
			{
			}

			return new TProperty();
		}

		// --------------------------------------------------------------------
		// データベースのプロパティーを更新
		// --------------------------------------------------------------------
		protected void UpdateProperty()
		{
			using (DataContext aContext = new DataContext(Connection))
			{
				Table<TProperty> aTableProperty = aContext.GetTable<TProperty>();

				// 古いプロパティーを削除
				IQueryable<TProperty> aDelTargets =
						from x in aTableProperty
						select x;
				aTableProperty.DeleteAllOnSubmit(aDelTargets);
				aContext.SubmitChanges();

				// 新しいプロパティーを挿入
				aTableProperty.InsertOnSubmit(new TProperty { AppId = YlConstants.APP_ID, AppVer = YlConstants.APP_GENERATION + "," + YlConstants.APP_VER });
				aContext.SubmitChanges();
			}
		}

		// --------------------------------------------------------------------
		// データベース中に有効なプロパティー情報が存在するか
		// --------------------------------------------------------------------
		protected Boolean ValidPropertyExists()
		{
			TProperty aProperty = Property();
			return aProperty.AppId == YlConstants.APP_ID;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// Dispose フラグ
		private Boolean mIsDisposed = false;
	}
	// public class DatabaseBase ___END___

	// ====================================================================
	// ディスク内のデータベース
	// ====================================================================

	public class DatabaseInDisk : DatabaseBase
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public DatabaseInDisk(EnvironmentModel oEnvironment, String oPath) : base(oEnvironment, oPath)
		{
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ファイルの最終更新日時
		// --------------------------------------------------------------------
		public DateTime LastWriteTime()
		{
			return new FileInfo(Connection.FileName).LastWriteTime;
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// データベースのバックアップを作成する
		// --------------------------------------------------------------------
		protected void Backup(String oSrcPath)
		{
			String aFileNameForLog = Path.GetFileNameWithoutExtension(oSrcPath);

			try
			{
				if (!File.Exists(oSrcPath))
				{
					return;
				}

				// バックアップ先の決定（既に存在する場合はバックアップをスキップ：1 日 1 回まで）
				FileInfo aDbFileInfo = new FileInfo(oSrcPath);
				String aBackupDbPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + YlConstants.FOLDER_NAME_DATABASE
						+ Path.GetFileNameWithoutExtension(oSrcPath) + "_(bak)_" + aDbFileInfo.LastWriteTime.ToString("yyyy_MM_dd") + Common.FILE_EXT_BAK;
				if (File.Exists(aBackupDbPath))
				{
					return;
				}

				// バックアップ
				File.Copy(oSrcPath, aBackupDbPath);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "データベース " + aFileNameForLog + " のバックアップ作成：" + aBackupDbPath);

				// 溢れたバックアップを削除
				List<FileInfo> aBackupFileInfos = new List<FileInfo>();
				String[] aBackupFiles = Directory.GetFiles(Path.GetDirectoryName(oSrcPath),
						Path.GetFileNameWithoutExtension(oSrcPath) + "_(bak)_*" + Common.FILE_EXT_BAK);
				foreach (String aBackupFile in aBackupFiles)
				{
					aBackupFileInfos.Add(new FileInfo(aBackupFile));
				}
				aBackupFileInfos.Sort((a, b) => -a.LastWriteTime.CompareTo(b.LastWriteTime));
				for (Int32 i = aBackupFileInfos.Count - 1; i >= NUM_MUSIC_INFO_DB_BACKUP_GENERATIONS; i--)
				{
					File.Delete(aBackupFileInfos[i].FullName);
					mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "データベース " + aFileNameForLog + " のバックアップ削除：" + aBackupFileInfos[i].FullName);
				}
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Warning, "データベース " + aFileNameForLog + " のバックアップ作成が完了しませんでした。\n" + oExcep.Message, true);
			}
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// 楽曲情報データベースバックアップ世代数
		private const Int32 NUM_MUSIC_INFO_DB_BACKUP_GENERATIONS = 31;

	}
	// public class DatabaseInDisk ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
