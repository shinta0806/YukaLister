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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
			//Debug.WriteLine("MusicInfoDatabaseInDisk() constructor");
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 楽曲情報データベースのバックアップを作成する
		// --------------------------------------------------------------------
		public void Backup()
		{
			try
			{
				if (!File.Exists(MusicInfoDbInDiskPath()))
				{
					return;
				}

				// バックアップ先の決定（既に存在する場合はバックアップをスキップ：1 日 1 回まで）
				FileInfo aDbFileInfo = new FileInfo(MusicInfoDbInDiskPath());
				String aBackupDbPath = Path.GetDirectoryName(MusicInfoDbInDiskPath()) + "\\" + Path.GetFileNameWithoutExtension(MusicInfoDbInDiskPath())
						+ "_(bak)_" + aDbFileInfo.LastWriteTime.ToString("yyyy_MM_dd") + Common.FILE_EXT_BAK;
				if (File.Exists(aBackupDbPath))
				{
					return;
				}

				// バックアップ
				File.Copy(MusicInfoDbInDiskPath(), aBackupDbPath);
				mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報データベースのバックアップ作成：" + aBackupDbPath);

				// 溢れたバックアップを削除
				List<FileInfo> aBackupFileInfos = new List<FileInfo>();
				String[] aBackupFiles = Directory.GetFiles(Path.GetDirectoryName(MusicInfoDbInDiskPath()),
						Path.GetFileNameWithoutExtension(MusicInfoDbInDiskPath()) + "_(bak)_*" + Common.FILE_EXT_BAK);
				foreach (String aBackupFile in aBackupFiles)
				{
					aBackupFileInfos.Add(new FileInfo(aBackupFile));
				}
				aBackupFileInfos.Sort((a, b) => -a.LastWriteTime.CompareTo(b.LastWriteTime));
				for (Int32 i = aBackupFileInfos.Count - 1; i >= NUM_MUSIC_INFO_DB_BACKUP_GENERATIONS; i--)
				{
					File.Delete(aBackupFileInfos[i].FullName);
					mEnvironment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報データベースのバックアップ削除：" + aBackupFileInfos[i].FullName);
				}
			}
			catch (Exception oExcep)
			{
				mEnvironment.LogWriter.ShowLogMessage(TraceEventType.Warning, "楽曲情報データベースのバックアップ作成が完了しませんでした。\n" + oExcep.Message, true);
			}
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
				Id = YlCommon.MUSIC_INFO_SYSTEM_ID_PREFIX + YlCommon.MUSIC_INFO_ID_SECOND_PREFIXES[(Int32)MusicInfoDbTables.TCategory] + oIdNumber.ToString("D3"),
				Import = false,
				Invalid = false,
				UpdateTime = YlCommon.INVALID_MJD,
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
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// フォルダー名
		// --------------------------------------------------------------------
		private const String FOLDER_NAME_DATABASE = "Database\\";

		// --------------------------------------------------------------------
		// ファイル名
		// --------------------------------------------------------------------
		private const String FILE_NAME_MUSIC_INFO = "MusicInfo" + Common.FILE_EXT_SQLITE3;

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------

		// 楽曲情報データベースバックアップ世代数
		private const Int32 NUM_MUSIC_INFO_DB_BACKUP_GENERATIONS = 31;


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
				CreateMusicInfoTable(aCmd, typeof(TSong), aIndices);

				CreateMusicInfoTable(aCmd, typeof(TPerson), TPerson.FIELD_NAME_PERSON_NAME);

				aIndices.Clear();
				aIndices.Add(TTieUp.FIELD_NAME_TIE_UP_NAME);
				aIndices.Add(TTieUp.FIELD_NAME_TIE_UP_CATEGORY_ID);
				CreateMusicInfoTable(aCmd, typeof(TTieUp), aIndices);

				CreateMusicInfoTable(aCmd, typeof(TCategory), TCategory.FIELD_NAME_CATEGORY_NAME);
				InsertCategoryDefaultRecords();

				CreateMusicInfoTable(aCmd, typeof(TTieUpGroup), TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_NAME);

				CreateMusicInfoTable(aCmd, typeof(TMaker), TMaker.FIELD_NAME_MAKER_NAME);

				// 別名テーブル
				CreateMusicInfoTable(aCmd, typeof(TSongAlias), TSongAlias.FIELD_NAME_SONG_ALIAS_ALIAS);

				CreateMusicInfoTable(aCmd, typeof(TPersonAlias), TPersonAlias.FIELD_NAME_PERSON_ALIAS_ALIAS);

				CreateMusicInfoTable(aCmd, typeof(TTieUpAlias), TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ALIAS);

				CreateMusicInfoTable(aCmd, typeof(TCategoryAlias), TCategoryAlias.FIELD_NAME_CATEGORY_ALIAS_ALIAS);

				CreateMusicInfoTable(aCmd, typeof(TTieUpGroupAlias), TTieUpGroupAlias.FIELD_NAME_TIE_UP_GROUP_ALIAS_ALIAS);

				CreateMusicInfoTable(aCmd, typeof(TMakerAlias), TMakerAlias.FIELD_NAME_MAKER_ALIAS_ALIAS);

				// 紐付テーブル
				CreateMusicInfoTable(aCmd, typeof(TArtistSequence));

				CreateMusicInfoTable(aCmd, typeof(TLyristSequence));

				CreateMusicInfoTable(aCmd, typeof(TComposerSequence));

				CreateMusicInfoTable(aCmd, typeof(TArrangerSequence));

				CreateMusicInfoTable(aCmd, typeof(TTieUpGroupSequence));
			}
		}

		// --------------------------------------------------------------------
		// DB の中にテーブルを作成（汎用関数）
		// --------------------------------------------------------------------
		private void CreateMusicInfoTable(SQLiteCommand oCmd, Type oTypeOfTable, String oIndexColumn = null)
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
			CreateMusicInfoTable(oCmd, oTypeOfTable, aIndices);
		}

		// --------------------------------------------------------------------
		// DB の中にテーブルを作成（汎用関数）
		// --------------------------------------------------------------------
		private void CreateMusicInfoTable(SQLiteCommand oCmd, Type oTypeOfTable, List<String> oIndices)
		{
			// テーブル作成
			LinqUtils.CreateTable(oCmd, oTypeOfTable);

			// インデックス作成（JOIN および検索の高速化）
			LinqUtils.CreateIndex(oCmd, LinqUtils.TableName(oTypeOfTable), oIndices);
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
				aTableCategory.InsertOnSubmit(CreateCategoryRecord(102, "歌ってみた", "ウタッテミタ"));
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
			return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + FOLDER_NAME_DATABASE + FILE_NAME_MUSIC_INFO;
		}




	}
	// public class YukariListDatabaseInDisk ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
