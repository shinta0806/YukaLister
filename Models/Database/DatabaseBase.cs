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
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
		// データベースの中にプロパティーテーブルを作成
		// --------------------------------------------------------------------
		protected void CreatePropertyTable()
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
		protected DatabaseInDisk(EnvironmentModel oEnvironment, String oPath) : base(oEnvironment, oPath)
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
	}
	// public class DatabaseInDisk ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
