// ============================================================================
// 
// インポートを行うウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// CSV からのインポート時は、主要な項目が既にデータベースに登録されている場合は
// インポートしない（手入力した詳細レコードとインポートによる簡易レコードが重複
// するのを避けるため）
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YukaLister.Shared;

namespace YukaLister
{
	public partial class FormImport : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormImport(YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mYukaListerSettings = oYukaListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ゆかりすたーでエクスポートしたファイルをインポート
		public Boolean IsYukaListerMode { get; set; }
		public String YklInfoPath { get; set; }

		// anison.info CSV をインポート
		public Boolean IsAnisonInfoMode { get; set; }
		public String ProgramCsvPath { get; set; }
		public String AnisonCsvPath { get; set; }
		public String SfCsvPath { get; set; }
		public String GameCsvPath { get; set; }

		// ニコカラりすたーでエクスポートしたファイルをインポート
		public Boolean IsNicoKaraListerMode { get; set; }
		public String NklInfoPath { get; set; }

		// ====================================================================
		// private 定数
		// ====================================================================

		// CSV 登録進捗表示間隔
		private const Int32 NUM_CSV_IMPORT_PROGRESS = 1000;

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 挿入待ちの TPerson
		private Dictionary<String, String> mInsertingPersons = new Dictionary<String, String>();

		// 挿入待ちの TSong
		private Dictionary<String, String> mInsertingSongs = new Dictionary<String, String>();

		// 挿入待ちの TTieUp
		private Dictionary<String, String> mInsertingTieUps = new Dictionary<String, String>();

		// 番組分類統合用マップ
		Dictionary<String, String> mCategoryUnityMap;

		// 環境設定
		private YukaListerSettings mYukaListerSettings;

		// ログ
		private LogWriter mLogWriter;

		// タスク中止用
		private CancellationTokenSource mAbortCancellationTokenSource = new CancellationTokenSource();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// anison.info CSV インポート用の設定確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckImportAnisonInfo()
		{
			// 最低 1 つはファイル指定必要
			if (String.IsNullOrEmpty(ProgramCsvPath) && String.IsNullOrEmpty(AnisonCsvPath) && String.IsNullOrEmpty(SfCsvPath) && String.IsNullOrEmpty(GameCsvPath))
			{
				throw new Exception("anison.info CSV ファイルを指定して下さい。");
			}

			// program.csv
			if (!String.IsNullOrEmpty(ProgramCsvPath))
			{
				if (Path.GetExtension(ProgramCsvPath).ToLower() != Common.FILE_EXT_CSV && Path.GetExtension(ProgramCsvPath).ToLower() != Common.FILE_EXT_ZIP
						|| Path.GetFileNameWithoutExtension(ProgramCsvPath).ToLower() != YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM)
				{
					throw new Exception(YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV + " 指定欄には、"
							+ YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV + " または "
							+ YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_ZIP + " を指定して下さい。");
				}
				if (!File.Exists(ProgramCsvPath))
				{
					throw new Exception(ProgramCsvPath + " が見つかりません。");
				}
			}

			// anison.csv
			if (!String.IsNullOrEmpty(AnisonCsvPath))
			{
				if (Path.GetExtension(AnisonCsvPath).ToLower() != Common.FILE_EXT_CSV && Path.GetExtension(AnisonCsvPath).ToLower() != Common.FILE_EXT_ZIP
						|| Path.GetFileNameWithoutExtension(AnisonCsvPath).ToLower() != YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON)
				{
					throw new Exception(YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV + " 指定欄には、"
							+ YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV + " または "
							+ YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_ZIP + " を指定して下さい。");
				}
				if (!File.Exists(AnisonCsvPath))
				{
					throw new Exception(AnisonCsvPath + " が見つかりません。");
				}
			}

			// sf.csv
			if (!String.IsNullOrEmpty(SfCsvPath))
			{
				if (Path.GetExtension(SfCsvPath).ToLower() != Common.FILE_EXT_CSV && Path.GetExtension(SfCsvPath).ToLower() != Common.FILE_EXT_ZIP
					|| Path.GetFileNameWithoutExtension(SfCsvPath).ToLower() != YlCommon.FILE_BODY_ANISON_INFO_CSV_SF)
				{
					throw new Exception(YlCommon.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV + " 指定欄には、"
							+ YlCommon.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV + " または "
							+ YlCommon.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_ZIP + " を指定して下さい。");
				}
				if (!File.Exists(SfCsvPath))
				{
					throw new Exception(SfCsvPath + " が見つかりません。");
				}
			}

			// game.csv
			if (!String.IsNullOrEmpty(GameCsvPath))
			{
				if (Path.GetExtension(GameCsvPath).ToLower() != Common.FILE_EXT_CSV && Path.GetExtension(GameCsvPath).ToLower() != Common.FILE_EXT_ZIP
					|| Path.GetFileNameWithoutExtension(GameCsvPath).ToLower() != YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME)
				{
					throw new Exception(YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV + " 指定欄には、"
							+ YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV + " または "
							+ YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_ZIP + " を指定して下さい。");
				}
				if (!File.Exists(GameCsvPath))
				{
					throw new Exception(GameCsvPath + " が見つかりません。");
				}
			}
		}

		// --------------------------------------------------------------------
		// ニコカラりすたーインポート用の設定確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckImportNicoKaraLister()
		{
			if (String.IsNullOrEmpty(NklInfoPath))
			{
				throw new Exception("ニコカラりすたーでエクスポートしたファイルを指定して下さい。");
			}

			if (Path.GetExtension(NklInfoPath).ToLower() != YlCommon.FILE_EXT_NKLINFO)
			{
				throw new Exception("ニコカラりすたーでエクスポートしたファイルではないファイルが指定されています。");
			}
		}

		// --------------------------------------------------------------------
		// CSV レコード同士の比較（指定された列で比較）
		// --------------------------------------------------------------------
		private Int32 CompareCsv(List<String> oLhs, List<String> oRhs, Int32 oColumnIndex)
		{
			if (oLhs.Count == 0 && oRhs.Count == 0)
			{
				return 0;
			}
			if (oLhs.Count == 0)
			{
				return -1;
			}
			if (oRhs.Count == 0)
			{
				return 1;
			}
			return String.Compare(oLhs[oColumnIndex], oRhs[oColumnIndex]);
		}

		// --------------------------------------------------------------------
		// program_alias.csv レコード同士の比較
		// --------------------------------------------------------------------
		private Int32 CompareProgramAliasCsv(List<String> oLhs, List<String> oRhs)
		{
			return CompareCsv(oLhs, oRhs, (Int32)ProgramAliasCsvColumns.NameOrId);
		}

		// --------------------------------------------------------------------
		// program.csv レコード同士の比較
		// --------------------------------------------------------------------
		private Int32 CompareProgramCsv(List<String> oLhs, List<String> oRhs)
		{
			return CompareCsv(oLhs, oRhs, (Int32)ProgramCsvColumns.Id);
		}

		// --------------------------------------------------------------------
		// anison_alias.csv 等のレコード同士の比較
		// --------------------------------------------------------------------
		private Int32 CompareSongAliasCsv(List<String> oLhs, List<String> oRhs)
		{
			return CompareCsv(oLhs, oRhs, (Int32)SongAliasCsvColumns.NameOrId);
		}

		// --------------------------------------------------------------------
		// anison.csv 等のレコード同士の比較
		// --------------------------------------------------------------------
		private Int32 CompareSongCsv(List<String> oLhs, List<String> oRhs)
		{
			return CompareCsv(oLhs, oRhs, (Int32)SongCsvColumns.Id);
		}

		// --------------------------------------------------------------------
		// "yyyy-mm-dd" をユリウス日に変換
		// 月日が 00 になっている場合があるため、DateTime.ParseExact() は使えない
		// --------------------------------------------------------------------
		private Double CsvDateStringToModifiedJulianDate(String oCsvDateString)
		{
			if (String.IsNullOrEmpty(oCsvDateString) || oCsvDateString.Length < 10)
			{
				return YlCommon.INVALID_MJD;
			}

			Int32 aYear;
			Int32 aMonth;
			Int32 aDay;

			try
			{
				aYear = Int32.Parse(oCsvDateString.Substring(0, 4));
				if (aYear < YlCommon.INVALID_YEAR)
				{
					aYear = YlCommon.INVALID_YEAR;
				}
				aMonth = Int32.Parse(oCsvDateString.Substring(5, 2));
				if (aMonth <= 0 || aMonth > 12)
				{
					aMonth = 1;
				}
				aDay = Int32.Parse(oCsvDateString.Substring(8, 2));
				if (aDay <= 0 || aDay > 31)
				{
					aDay = 1;
				}
			}
			catch (Exception)
			{
				return YlCommon.INVALID_MJD;
			}

			return JulianDay.DateTimeToModifiedJulianDate(new DateTime(aYear, aMonth, aDay));
		}

		// --------------------------------------------------------------------
		// カテゴリー名からカテゴリー ID を検索（無ければ null を返す）
		// ＜返値＞ カテゴリー ID or null
		// --------------------------------------------------------------------
		private String FindCategory(String oCategoryName, DataContext oContext)
		{
			String aNormalizedCategoryName = YlCommon.NormalizeDbString(oCategoryName);
			if (String.IsNullOrEmpty(aNormalizedCategoryName))
			{
				return null;
			}

			if (mCategoryUnityMap.ContainsKey(aNormalizedCategoryName))
			{
				aNormalizedCategoryName = mCategoryUnityMap[aNormalizedCategoryName];
			}

			// 楽曲情報データベースから検索
			Table<TCategory> aTable = oContext.GetTable<TCategory>();
			IQueryable<TCategory> aQueryResult =
					from x in aTable
					where x.Name == aNormalizedCategoryName
					select x;
			foreach (TCategory aCategory in aQueryResult)
			{
				return aCategory.Id;
			}

			return null;
		}

		// --------------------------------------------------------------------
		// 人物名から人物 ID を検索（無ければ新規挿入）
		// ＜返値＞ 人物 ID 群
		// --------------------------------------------------------------------
		private List<String> FindOrInsertPeople(String oArtistNames, DataContext oContext)
		{
			List<String> aPeople = new List<String>();

			// 全角カンマがあるとうまく区切れないため一度正規化
			String aNormalizedArtistNames = YlCommon.NormalizeDbString(oArtistNames);
			if (String.IsNullOrEmpty(aNormalizedArtistNames))
			{
				return aPeople;
			}

			Table<TPerson> aTable = oContext.GetTable<TPerson>();
			String[] aSplits = aNormalizedArtistNames.Split(',');
			foreach (String aOneName in aSplits)
			{
				// 前後に空白がある可能性があるため再度正規化
				String aNormalizedOneName = YlCommon.NormalizeDbString(aOneName);
				if (String.IsNullOrEmpty(aNormalizedOneName))
				{
					continue;
				}

				// 楽曲情報データベースから検索
				List<TPerson> aDbPeople = YlCommon.SelectPeopleByName(oContext, aNormalizedOneName, true);
				if (aDbPeople.Count > 0)
				{
					aPeople.Add(aDbPeople[0].Id);
					continue;
				}

				// 挿入待ちから検索
				if (mInsertingPersons.ContainsKey(aNormalizedOneName))
				{
					aPeople.Add(mInsertingPersons[aNormalizedOneName]);
					continue;
				}

				// 新規挿入
				String aId = mYukaListerSettings.PrepareLastId((SQLiteConnection)oContext.Connection, MusicInfoDbTables.TPerson);
				aTable.InsertOnSubmit(new TPerson
				{
					// TBase
					Id = aId,
					Import = true,
					Invalid = false,
					UpdateTime = YlCommon.INVALID_MJD,
					Dirty = true,

					// TMaster
					Name = aNormalizedOneName,
					Ruby = null,
					Keyword = null,
				});
				mInsertingPersons[aNormalizedOneName] = aId;
				aPeople.Add(aId);
			}

			return aPeople;
		}

		// --------------------------------------------------------------------
		// anison.info CSV をインポート
		// --------------------------------------------------------------------
		private Task ImportAnisonInfoAsync()
		{
			return Task.Run(() =>
			{
				try
				{
					// 終了時に強制終了されないように設定
					Thread.CurrentThread.IsBackground = false;

					CheckImportAnisonInfo();

					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						using (DataContext aContext = new DataContext(aConnection))
						{
							// タイアップ情報
							if (!String.IsNullOrEmpty(ProgramCsvPath))
							{
								ImportProgramCsv(ProgramCsvPath, aContext);
							}

							// 楽曲情報
							if (!String.IsNullOrEmpty(AnisonCsvPath))
							{
								ImportSongCsv(AnisonCsvPath, aContext);
							}
							if (!String.IsNullOrEmpty(SfCsvPath))
							{
								ImportSongCsv(SfCsvPath, aContext);
							}
							if (!String.IsNullOrEmpty(GameCsvPath))
							{
								ImportSongCsv(GameCsvPath, aContext);
							}
						}
					}

					mLogWriter.ShowLogMessage(TraceEventType.Information, "完了");
				}
				catch (Exception oExcep)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Error, "anison.info CSV インポート時エラー：\n" + oExcep.Message);
					mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
				}
			});
		}

		// --------------------------------------------------------------------
		// ニコカラりすたーでエクスポートしたファイルをインポート
		// --------------------------------------------------------------------
		private Task ImportNicoKaraListerAsync()
		{
			return Task.Run(() =>
			{
				try
				{
					// 終了時に強制終了されないように設定
					Thread.CurrentThread.IsBackground = false;

					CheckImportNicoKaraLister();

					// 解凍
					String aTempFolder = YlCommon.TempFilePath() + "\\";
					Directory.CreateDirectory(aTempFolder);
					ZipFile.ExtractToDirectory(NklInfoPath, aTempFolder);
					String aExtractedFolderPath = aTempFolder + YlCommon.FILE_PREFIX_INFO + "\\";

					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						using (SQLiteCommand aCmd = new SQLiteCommand(aConnection))
						{
							using (DataContext aContext = new DataContext(aConnection))
							{
								// タイアップ情報
								String aProgramCsvPath = aExtractedFolderPath + YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV;
								if (File.Exists(aProgramCsvPath))
								{
									ImportProgramCsv(aProgramCsvPath, aContext);
								}

								// 楽曲情報
								List<String> aSongCsvs = new List<String>();
								aSongCsvs.Add(YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV);
								aSongCsvs.Add(YlCommon.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV);
								aSongCsvs.Add(YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV);
								aSongCsvs.Add(YlCommon.FILE_BODY_ANISON_INFO_CSV_MISC + Common.FILE_EXT_CSV);
								foreach (String aSongCsv in aSongCsvs)
								{
									String aSongCsvPath = aExtractedFolderPath + aSongCsv;
									if (File.Exists(aSongCsvPath))
									{
										ImportSongCsv(aSongCsvPath, aContext);
									}
								}

								// タイアップエイリアス情報
								String aProgramAliasCsvPath = aExtractedFolderPath + YlCommon.FILE_BODY_ANISON_INFO_CSV_PROGRAM_ALIAS + Common.FILE_EXT_CSV;
								if (File.Exists(aProgramAliasCsvPath))
								{
									ImportProgramAliasCsv(aProgramAliasCsvPath, aContext);
								}

								// 楽曲エイリアス情報
								List<String> aSongAliasCsvs = new List<String>();
								aSongAliasCsvs.Add(YlCommon.FILE_BODY_ANISON_INFO_CSV_ANISON_ALIAS + Common.FILE_EXT_CSV);
								aSongAliasCsvs.Add(YlCommon.FILE_BODY_ANISON_INFO_CSV_SF_ALIAS + Common.FILE_EXT_CSV);
								aSongAliasCsvs.Add(YlCommon.FILE_BODY_ANISON_INFO_CSV_GAME_ALIAS + Common.FILE_EXT_CSV);
								aSongAliasCsvs.Add(YlCommon.FILE_BODY_ANISON_INFO_CSV_MISC_ALIAS + Common.FILE_EXT_CSV);
								foreach (String aSongAliasCsv in aSongAliasCsvs)
								{
									String aSongAliasCsvPath = aExtractedFolderPath + aSongAliasCsv;
									if (File.Exists(aSongAliasCsvPath))
									{
										ImportSongAliasCsv(aSongAliasCsvPath, aContext);
									}
								}
							}
						}
					}

					mLogWriter.ShowLogMessage(TraceEventType.Information, "完了");
				}
				catch (Exception oExcep)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Error, "anison.info CSV インポート時エラー：\n" + oExcep.Message);
					mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
				}
			});
		}

		// --------------------------------------------------------------------
		// 番組情報別名 CSV からタイアップ別名テーブルへインポート
		// --------------------------------------------------------------------
		private void ImportProgramAliasCsv(String oProgramAliasCsvPath, DataContext oContext)
		{
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, oProgramAliasCsvPath + " からタイアップ別名情報をインポート中...");

			List<List<String>> aCsvContents = YlCommon.LoadCsv(oProgramAliasCsvPath, mYukaListerSettings, (Int32)ProgramAliasCsvColumns.__End__);
			aCsvContents.Sort(CompareProgramAliasCsv);
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TTieUp> aTableTieUp = oContext.GetTable<TTieUp>();
			Table<TTieUpAlias> aTableTieUpAlias = oContext.GetTable<TTieUpAlias>();
			Int32 aNumImports = 0;

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aCsvRecord = aCsvContents[i];

				// 番組 ID の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)ProgramAliasCsvColumns.NameOrId]))
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramAliasCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の番組 ID が指定されていないため無視します。", true);
					continue;
				}

				// 番組 ID が既にテーブル内にあるか確認
				TTieUp aTieUpRecord = aTableTieUp.SingleOrDefault(x => x.Id == aCsvRecord[(Int32)ProgramAliasCsvColumns.NameOrId]);
				if (aTieUpRecord == null && String.IsNullOrEmpty(aCsvRecord[(Int32)ProgramAliasCsvColumns.ForceId]))
				{
					aTieUpRecord = aTableTieUp.SingleOrDefault(x => x.Name == aCsvRecord[(Int32)ProgramAliasCsvColumns.NameOrId]);
				}
				if (aTieUpRecord == null)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramAliasCsvColumns.LineIndex]) + 2).ToString("#,0")
							+ " 行目の番組 ID がタイアップマスターテーブルに存在しないため無視します。", true);
					continue;
				}

				// 別名が同名でないか確認
				aCsvRecord[(Int32)ProgramAliasCsvColumns.Alias] = YlCommon.NormalizeDbString(aCsvRecord[(Int32)ProgramAliasCsvColumns.Alias]);
				if (aCsvRecord[(Int32)ProgramAliasCsvColumns.Alias] == aTieUpRecord.Name)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramAliasCsvColumns.LineIndex]) + 2).ToString("#,0")
							+ " 行目の別名が番組名と同一のため無視します。", true);
					continue;
				}

				// 情報が既に別名テーブル内にあるか確認
				TTieUpAlias aDbExistRecord = null;
				IQueryable<TTieUpAlias> aQueryResult =
						from x in aTableTieUpAlias
						where x.Alias == aCsvRecord[(Int32)ProgramAliasCsvColumns.Alias]
						select x;
				foreach (TTieUpAlias aTTieUpAlias in aQueryResult)
				{
					aDbExistRecord = aTTieUpAlias;
				}
				TTieUpAlias aDbNewRecord = TTieUpAliasFromAliasAndId((SQLiteConnection)oContext.Connection, aCsvRecord[(Int32)ProgramAliasCsvColumns.Alias], aTieUpRecord.Id);
				if (aDbExistRecord == null)
				{
					// 新規登録
					aTableTieUpAlias.InsertOnSubmit(aDbNewRecord);
					aNumImports++;
				}
				else if (YlCommon.IsUpdated(aDbExistRecord, aDbNewRecord))
				{
					// 既存登録あるが更新が必要
#if DEBUGz
					if (aDbExistRecord.Id != aDbNewRecord.Id)
					{
						Debug.WriteLine("ImportProgramAliasCsv() id change: " + aDbExistRecord.Id + " → " + aDbNewRecord.Id);
					}
#endif
					aDbNewRecord.Id = aDbExistRecord.Id;
					aDbNewRecord.UpdateTime = aDbExistRecord.UpdateTime;
					Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
					aNumImports++;
				}

				if ((i + 1) % NUM_CSV_IMPORT_PROGRESS == 0)
				{
					mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "進捗：" + (i + 1).ToString("#,0") + " 個...");
					mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aCsvContents.Count.ToString("#,0") + " 個のデータのうち、重複を除く "
					+ aNumImports.ToString("#,0") + " 個をインポートしました。");
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// 番組情報 CSV からタイアップマスターテーブルへインポート
		// --------------------------------------------------------------------
		private void ImportProgramCsv(String oProgramCsvPath, DataContext oContext)
		{
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, oProgramCsvPath + " からタイアップ情報をインポート中...");

			UnzipIfNeeded(ref oProgramCsvPath);

			List<List<String>> aCsvContents = YlCommon.LoadCsv(oProgramCsvPath, mYukaListerSettings, (Int32)ProgramCsvColumns.__End__);
			aCsvContents.Sort(CompareProgramCsv);
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TTieUp> aTable = oContext.GetTable<TTieUp>();
			List<String> aPrevCsvRecord = null;
			Int32 aNumImports = 0;

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aCsvRecord = aCsvContents[i];

				// 番組 ID の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)ProgramCsvColumns.Id]))
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の番組 ID が指定されていないため無視します。", true);
					continue;
				}
				if (aPrevCsvRecord != null && aCsvRecord[(Int32)ProgramCsvColumns.Id] == aPrevCsvRecord[(Int32)ProgramCsvColumns.Id])
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							"CSV 内の番組 ID（" + aCsvRecord[(Int32)ProgramCsvColumns.Id] + "）が重複しているため無視します（"
							+ (aPrevCsvRecord[(Int32)ProgramCsvColumns.Name] == aCsvRecord[(Int32)ProgramCsvColumns.Name] ? aPrevCsvRecord[(Int32)ProgramCsvColumns.Name]
							: "登録済：" + aPrevCsvRecord[(Int32)ProgramCsvColumns.Name] + "、無視：" + aCsvRecord[(Int32)ProgramCsvColumns.Name])
							+ "）。", true);
					continue;
				}

				// 番組名の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)ProgramCsvColumns.Name]))
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の番組名が指定されていないため無視します。", true);
					continue;
				}
				//mLogWriter.ShowLogMessage(TraceEventType.Verbose, "ImportProgramCsv() raw name: " + aCsvRecord[(Int32)ProgramCsvColumns.Name]);
				aCsvRecord[(Int32)ProgramCsvColumns.Name] = YlCommon.NormalizeDbString(aCsvRecord[(Int32)ProgramCsvColumns.Name]);
				//mLogWriter.ShowLogMessage(TraceEventType.Verbose, "ImportProgramCsv() normalized name: " + aCsvRecord[(Int32)ProgramCsvColumns.Name]);
				TTieUp aDupRecord = null;
				IQueryable<TTieUp> aQueryResult =
						from x in aTable
						where x.Name == aCsvRecord[(Int32)ProgramCsvColumns.Name] && x.Id != aCsvRecord[(Int32)ProgramCsvColumns.Id]
						select x;
				foreach (TTieUp aTTieUp in aQueryResult)
				{
					aDupRecord = aTTieUp;
				}
				if (aDupRecord != null
						|| mInsertingTieUps.ContainsKey(aCsvRecord[(Int32)ProgramCsvColumns.Name]) && mInsertingTieUps[aCsvRecord[(Int32)ProgramCsvColumns.Name]] != aCsvRecord[(Int32)ProgramCsvColumns.Id])
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の番組名が既に登録されているため無視します。", true);
					continue;
				}

				// カテゴリー ID
				String aCategoryId = FindCategory(aCsvRecord[(Int32)ProgramCsvColumns.Category], oContext);

				// 番組 ID が既にテーブル内にあるか確認
				TTieUp aDbExistRecord = aTable.SingleOrDefault(x => x.Id == aCsvRecord[(Int32)ProgramCsvColumns.Id]);
				TTieUp aDbNewRecord = TTieUpFromProgramCsvRecord(aCsvRecord, aCategoryId);
				if (aDbExistRecord == null)
				{
					// 新規登録
					aTable.InsertOnSubmit(aDbNewRecord);
					mInsertingTieUps[aCsvRecord[(Int32)ProgramCsvColumns.Name]] = aCsvRecord[(Int32)ProgramCsvColumns.Id];
					//mLogWriter.ShowLogMessage(TraceEventType.Verbose, "ImportProgramCsv() 新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
					aNumImports++;
				}
				else if (YlCommon.IsUpdated(aDbExistRecord, aDbNewRecord))
				{
					// 既存登録あるが更新が必要
					aDbNewRecord.UpdateTime = aDbExistRecord.UpdateTime;
					Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
					//mLogWriter.ShowLogMessage(TraceEventType.Verbose, "ImportProgramCsv() 更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
					aNumImports++;
				}

				aPrevCsvRecord = aCsvRecord;

				if ((i + 1) % NUM_CSV_IMPORT_PROGRESS == 0)
				{
					mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "進捗：" + (i + 1).ToString("#,0") + " 個...");
					mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aCsvContents.Count.ToString("#,0") + " 個のデータのうち、重複を除く "
					+ aNumImports.ToString("#,0") + " 個をインポートしました。");
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// 楽曲情報別名 CSV から楽曲別名テーブルへインポート
		// --------------------------------------------------------------------
		private void ImportSongAliasCsv(String oSongAliasCsvPath, DataContext oContext)
		{
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, oSongAliasCsvPath + " から楽曲別名情報をインポート中...");

			List<List<String>> aCsvContents = YlCommon.LoadCsv(oSongAliasCsvPath, mYukaListerSettings, (Int32)SongAliasCsvColumns.__End__);
			aCsvContents.Sort(CompareSongAliasCsv);
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TSong> aTableSong = oContext.GetTable<TSong>();
			Table<TSongAlias> aTableSongAlias = oContext.GetTable<TSongAlias>();
			Int32 aNumImports = 0;

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aCsvRecord = aCsvContents[i];

				// 楽曲 ID の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)SongAliasCsvColumns.NameOrId]))
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongAliasCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の楽曲 ID が指定されていないため無視します。", true);
					continue;
				}

				// 楽曲 ID が既にテーブル内にあるか確認
				TSong aSongRecord = aTableSong.SingleOrDefault(x => x.Id == aCsvRecord[(Int32)SongAliasCsvColumns.NameOrId]);
				if (aSongRecord == null && String.IsNullOrEmpty(aCsvRecord[(Int32)SongAliasCsvColumns.ForceId]))
				{
					aSongRecord = aTableSong.SingleOrDefault(x => x.Name == aCsvRecord[(Int32)SongAliasCsvColumns.NameOrId]);
				}
				if (aSongRecord == null)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongAliasCsvColumns.LineIndex]) + 2).ToString("#,0")
							+ " 行目の楽曲 ID が楽曲マスターテーブルに存在しないため無視します。", true);
					continue;
				}

				// 別名が同名でないか確認
				aCsvRecord[(Int32)SongAliasCsvColumns.Alias] = YlCommon.NormalizeDbString(aCsvRecord[(Int32)SongAliasCsvColumns.Alias]);
				if (aCsvRecord[(Int32)SongAliasCsvColumns.Alias] == aSongRecord.Name)
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongAliasCsvColumns.LineIndex]) + 2).ToString("#,0")
							+ " 行目の別名が楽曲名と同一のため無視します。", true);
					continue;
				}

				// 情報が既に別名テーブル内にあるか確認
				TSongAlias aDbExistRecord = null;
				IQueryable<TSongAlias> aQueryResult =
						from x in aTableSongAlias
						where x.Alias == aCsvRecord[(Int32)SongAliasCsvColumns.Alias]
						select x;
				foreach (TSongAlias aTSongAlias in aQueryResult)
				{
					aDbExistRecord = aTSongAlias;
				}
				TSongAlias aDbNewRecord = TSongAliasFromAliasAndId((SQLiteConnection)oContext.Connection, aCsvRecord[(Int32)SongAliasCsvColumns.Alias], aSongRecord.Id);
				if (aDbExistRecord == null)
				{
					// 新規登録
					aTableSongAlias.InsertOnSubmit(aDbNewRecord);
					aNumImports++;
				}
				else if (YlCommon.IsUpdated(aDbExistRecord, aDbNewRecord))
				{
					// 既存登録あるが更新が必要
					aDbNewRecord.Id = aDbExistRecord.Id;
					aDbNewRecord.UpdateTime = aDbExistRecord.UpdateTime;
					Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
					aNumImports++;
				}

				if ((i + 1) % NUM_CSV_IMPORT_PROGRESS == 0)
				{
					mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "進捗：" + (i + 1).ToString("#,0") + " 個...");
					mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aCsvContents.Count.ToString("#,0") + " 個のデータのうち、重複を除く "
					+ aNumImports.ToString("#,0") + " 個をインポートしました。");
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// 楽曲情報 CSV から楽曲マスターテーブルへインポート
		// --------------------------------------------------------------------
		private void ImportSongCsv(String oSongCsvPath, DataContext oContext)
		{
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, oSongCsvPath + " から楽曲情報をインポート中...");

			UnzipIfNeeded(ref oSongCsvPath);

			List<List<String>> aCsvContents = YlCommon.LoadCsv(oSongCsvPath, mYukaListerSettings, (Int32)SongCsvColumns.__End__);
			aCsvContents.Sort(CompareSongCsv);
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TSong> aTable = oContext.GetTable<TSong>();
			List<String> aPrevCsvRecord = null;
			Int32 aNumImports = 0;

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aCsvRecord = aCsvContents[i];

				// 楽曲 ID の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)SongCsvColumns.Id]))
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の楽曲 ID が指定されていないため無視します。", true);
					continue;
				}
				if (aPrevCsvRecord != null && aCsvRecord[(Int32)SongCsvColumns.Id] == aPrevCsvRecord[(Int32)SongCsvColumns.Id])
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							"CSV 内の楽曲 ID（" + aCsvRecord[(Int32)SongCsvColumns.Id] + "）が重複しているため無視します（"
							+ (aPrevCsvRecord[(Int32)SongCsvColumns.Name] == aCsvRecord[(Int32)SongCsvColumns.Name] ? aPrevCsvRecord[(Int32)SongCsvColumns.Name]
							: "登録済：" + aPrevCsvRecord[(Int32)SongCsvColumns.Name] + "、無視：" + aCsvRecord[(Int32)SongCsvColumns.Name])
							+ "）。", true);
					continue;
				}

				// 楽曲名の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)SongCsvColumns.Name]))
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の楽曲名が指定されていないため無視します。", true);
					continue;
				}
				//mLogWriter.ShowLogMessage(TraceEventType.Verbose, "ImportSongCsv() raw name: " + aCsvRecord[(Int32)SongCsvColumns.Name]);
				aCsvRecord[(Int32)SongCsvColumns.Name] = YlCommon.NormalizeDbString(aCsvRecord[(Int32)SongCsvColumns.Name]);
				//mLogWriter.ShowLogMessage(TraceEventType.Verbose, "ImportSongCsv() normalized name: " + aCsvRecord[(Int32)SongCsvColumns.Name]);
				TSong aDupRecord = null;
				IQueryable<TSong> aQueryResult =
						from x in aTable
						where x.Name == aCsvRecord[(Int32)SongCsvColumns.Name] && x.Id != aCsvRecord[(Int32)SongCsvColumns.Id]
						select x;
				foreach (TSong aSong in aQueryResult)
				{
					aDupRecord = aSong;
				}
				if (aDupRecord != null
						|| mInsertingSongs.ContainsKey(aCsvRecord[(Int32)SongCsvColumns.Name]) && mInsertingSongs[aCsvRecord[(Int32)SongCsvColumns.Name]] != aCsvRecord[(Int32)SongCsvColumns.Id])
				{
					mLogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の楽曲名が既に登録されているため無視します。", true);
					continue;
				}

				// 歌手の人物 ID
				//mLogWriter.ShowLogMessage(TraceEventType.Verbose, "ImportSongCsv() artist name: " + aCsvRecord[(Int32)SongCsvColumns.Artist]);
				List<String> aArtistIds = FindOrInsertPeople(aCsvRecord[(Int32)SongCsvColumns.Artist], oContext);

				// 楽曲 ID が既にテーブル内にあるか確認
				TSong aDbExistRecord = aTable.SingleOrDefault(x => x.Id == aCsvRecord[(Int32)SongCsvColumns.Id]);
				TSong aDbNewRecord = TSongFromSongCsvRecord(aCsvRecord);
				if (aDbExistRecord == null)
				{
					// 新規登録
					aTable.InsertOnSubmit(aDbNewRecord);
					mInsertingSongs[aCsvRecord[(Int32)SongCsvColumns.Name]] = aCsvRecord[(Int32)SongCsvColumns.Id];

					// 歌手紐付け
					YlCommon.RegisterArtistSequence(oContext, aDbNewRecord.Id, aArtistIds, true);
					aNumImports++;
				}
				else if (YlCommon.IsUpdated(aDbExistRecord, aDbNewRecord))
				{
					// 既存登録あるが更新が必要
					aDbNewRecord.UpdateTime = aDbExistRecord.UpdateTime;
					Common.ShallowCopy(aDbNewRecord, aDbExistRecord);

					// 歌手紐付け
					YlCommon.RegisterArtistSequence(oContext, aDbNewRecord.Id, aArtistIds, true);
					aNumImports++;
				}
				else
				{
					// 既存登録あり、更新が不要の場合は、歌手紐付けも更新しない（Ver 6.25 にて改善）
				}

				aPrevCsvRecord = aCsvRecord;

				if ((i + 1) % NUM_CSV_IMPORT_PROGRESS == 0)
				{
					mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "進捗：" + (i + 1).ToString("#,0") + " 個...");
					mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aCsvContents.Count.ToString("#,0") + " 個のデータのうち、重複を除く "
					+ aNumImports.ToString("#,0") + " 個をインポートしました。");
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = "環境設定";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif
			mCategoryUnityMap = YlCommon.CreateCategoryUnityMap();

			Common.CascadeForm(this);
		}

		// --------------------------------------------------------------------
		// 別名とオリジナル ID から TTieUpAlias レコードを作成
		// --------------------------------------------------------------------
		private TSongAlias TSongAliasFromAliasAndId(SQLiteConnection oConnection, String oAlias, String oOriginalId)
		{
			String aId = mYukaListerSettings.PrepareLastId(oConnection, MusicInfoDbTables.TSongAlias);

			TSongAlias aSongAlias = new TSongAlias
			{
				// TBase
				Id = aId,
				Import = true,
				Invalid = false,
				UpdateTime = YlCommon.INVALID_MJD,
				Dirty = true,

				// TAlias
				Alias = oAlias,
				OriginalId = oOriginalId,
			};

			return aSongAlias;
		}

		// --------------------------------------------------------------------
		// anison.csv 等のレコードから TSong レコードを作成
		// oCsvRecord[(Int32)SongCsvColumns.Name] は正規化済みの前提
		// --------------------------------------------------------------------
		private TSong TSongFromSongCsvRecord(List<String> oCsvRecord)
		{
			Debug.Assert(oCsvRecord.Count >= (Int32)SongCsvColumns.__End__, "TSongFromSongCsvRecord() bad csv record count");

			TSong aSong = new TSong
			{
				// TBase
				Id = oCsvRecord[(Int32)SongCsvColumns.Id],
				Import = true,
				Invalid = false,
				UpdateTime = YlCommon.INVALID_MJD,
				Dirty = true,

				// TMaster
				Name = oCsvRecord[(Int32)SongCsvColumns.Name],
				Ruby = null,
				Keyword = null,

				// TSong
				ReleaseDate = YlCommon.INVALID_MJD,
				TieUpId = YlCommon.NullIfEmpty(oCsvRecord[(Int32)SongCsvColumns.ProgramId]),
				CategoryId = null,
				OpEd = YlCommon.NullIfEmpty(oCsvRecord[(Int32)SongCsvColumns.OpEd]),
			};

			return aSong;
		}

		// --------------------------------------------------------------------
		// 別名とオリジナル ID から TTieUpAlias レコードを作成
		// --------------------------------------------------------------------
		private TTieUpAlias TTieUpAliasFromAliasAndId(SQLiteConnection oConnection, String oAlias, String oOriginalId)
		{
			String aId = mYukaListerSettings.PrepareLastId(oConnection, MusicInfoDbTables.TTieUpAlias);

			TTieUpAlias aTieUpAlias = new TTieUpAlias
			{
				// TBase
				Id = aId,
				Import = true,
				Invalid = false,
				UpdateTime = YlCommon.INVALID_MJD,
				Dirty = true,

				// TAlias
				Alias = oAlias,
				OriginalId = oOriginalId,
			};

			return aTieUpAlias;
		}

		// --------------------------------------------------------------------
		// program.csv レコードから TTieUp レコードを作成
		// oCsvRecord[(Int32)ProgramCsvColumns.Name] は正規化済みの前提
		// --------------------------------------------------------------------
		private TTieUp TTieUpFromProgramCsvRecord(List<String> oCsvRecord, String oCategoryId)
		{
			Debug.Assert(oCsvRecord.Count >= (Int32)ProgramCsvColumns.__End__, "TTieUpFromCsvRecord() bad csv record count");

			TTieUp aTieUp = new TTieUp
			{
				// TBase
				Id = oCsvRecord[(Int32)ProgramCsvColumns.Id],
				Import = true,
				Invalid = false,
				UpdateTime = YlCommon.INVALID_MJD,
				Dirty = true,

				// TMaster
				Name = oCsvRecord[(Int32)ProgramCsvColumns.Name],
				Ruby = YlCommon.NormalizeDbRuby(oCsvRecord[(Int32)ProgramCsvColumns.Ruby]),
				Keyword = YlCommon.NormalizeDbString(oCsvRecord[(Int32)ProgramCsvColumns.SubName]),

				// TTieUp
				CategoryId = oCategoryId,
				MakerId = null,
				AgeLimit = Common.StringToInt32(oCsvRecord[(Int32)ProgramCsvColumns.AgeLimit]),
				ReleaseDate = CsvDateStringToModifiedJulianDate(oCsvRecord[(Int32)ProgramCsvColumns.BeginDate]),
			};

			return aTieUp;
		}

		// --------------------------------------------------------------------
		// zip ファイルが指定された場合は解凍
		// --------------------------------------------------------------------
		private void UnzipIfNeeded(ref String oCsvPath)
		{
			if (Path.GetExtension(oCsvPath).ToLower() != Common.FILE_EXT_ZIP)
			{
				return;
			}

			// 解凍
			String aTempFolder = YlCommon.TempFilePath() + "\\";
			Directory.CreateDirectory(aTempFolder);
			ZipFile.ExtractToDirectory(oCsvPath, aTempFolder);
			String[] aFiles = Directory.GetFiles(aTempFolder, "*" + Common.FILE_EXT_CSV, SearchOption.AllDirectories);
			if (aFiles.Length > 0)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "UnzipIfNeeded() unzip: " + aFiles[0]);
				oCsvPath = aFiles[0];
			}
		}


		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormImport_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "インポートウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "インポートウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}

		}

		private async void FormImport_Shown(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.TextBoxDisplay = TextBoxLog;
				YlCommon.BackupMusicInfoDb();
				YlCommon.InputIdPrefixIfNeededWithInvoke(this, mYukaListerSettings);

				// インポートタスクを実行（async の終了を待つ）
				if (IsYukaListerMode)
				{
					// 未実装
				}
				else if (IsAnisonInfoMode)
				{
					LabelSrc.Text = "anison.info CSV をインポートします";
					await ImportAnisonInfoAsync();
				}
				else if (IsNicoKaraListerMode)
				{
					LabelSrc.Text = "ニコカラりすたーでエクスポートしたファイルをインポートします";
					await ImportNicoKaraListerAsync();
				}
				else
				{
					Debug.Assert(false, "FormImport_Shown() bad mode");
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "インポートウィンドウ表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				mLogWriter.TextBoxDisplay = null;
				Close();
			}
		}
	}
	// public partial class FormImport ___END___

}
// namespace YukaLister ___END___