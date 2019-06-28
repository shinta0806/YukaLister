// ============================================================================
// 
// 楽曲情報データベース同期クライアント
// 
// ============================================================================

// ----------------------------------------------------------------------------
// SQLite は異なるコネクションであればスレッドセーフであることを前提としている
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;
using YukaLister.ViewModels;

namespace YukaLister.Models.Http
{
	public class SyncClient
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public SyncClient(EnvironmentModel oEnvironment, MainWindowViewModel oMainWindowViewModel, Boolean oIsReget = false)
		{
			mEnvironment = oEnvironment;
			mMainWindowViewModel = oMainWindowViewModel;
			mIsReget = oIsReget;

			// ログ初期化
			// さほど大量のログは発生しないため、世代等はデフォルトのまま
			mLogWriterSync = new LogWriter(YlConstants.APP_ID + "Sync");
			mLogWriterSync.SimpleTraceListener.LogFileName = Path.GetDirectoryName(mLogWriterSync.SimpleTraceListener.LogFileName) + "\\" + FILE_NAME_SYNC_LOG;
			mLogWriterSync.ApplicationQuitToken = mEnvironment.AppCancellationTokenSource.Token;
			mLogWriterSync.SimpleTraceListener.MaxOldGenerations = 3;

			// 詳細ログ初期化
			// 大量のログが発生するため、世代・サイズともに拡大
			mLogWriterSyncDetail = new LogWriter(YlConstants.APP_ID + "SyncDetail");
			mLogWriterSyncDetail.SimpleTraceListener.LogFileName = Path.GetDirectoryName(mLogWriterSync.SimpleTraceListener.LogFileName) + "\\" + FILE_NAME_SYNC_DETAIL_LOG;
			mLogWriterSyncDetail.ApplicationQuitToken = mEnvironment.AppCancellationTokenSource.Token;
			mLogWriterSyncDetail.SimpleTraceListener.MaxOldGenerations = 5;
			mLogWriterSyncDetail.SimpleTraceListener.MaxSize = 5 * 1024 * 1024;
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 同期実行中のインスタンスがあるか
		// --------------------------------------------------------------------
		public static Boolean RunningInstanceExists()
		{
			Boolean aLockTaken = Monitor.TryEnter(mTaskLockSync, 0);
			if (aLockTaken)
			{
				Monitor.Exit(mTaskLockSync);
			}
			return !aLockTaken;
		}

		// --------------------------------------------------------------------
		// 非同期に実行開始
		// --------------------------------------------------------------------
		public Task RunAsync()
		{
			return YlCommon.LaunchTaskAsync<Object>(SyncMusicInfoDbByWorker, mTaskLockSync, null, mEnvironment.LogWriter);
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ファイル名
		private const String FILE_NAME_CP_LOGIN = "CPLogin" + Common.FILE_EXT_PHP;
		private const String FILE_NAME_CP_MAIN = "CPMain" + Common.FILE_EXT_PHP;
		private const String FILE_NAME_SYNC_DETAIL_LOG = YlConstants.APP_ID + "SyncDetail" + Common.FILE_EXT_LOG;
		private const String FILE_NAME_SYNC_INFO = "SyncInfo" + Common.FILE_EXT_TXT;
		private const String FILE_NAME_SYNC_LOG = YlConstants.APP_ID + "Sync" + Common.FILE_EXT_LOG;

		// 同期モード
		private const String SYNC_MODE_NAME_DOWNLOAD_POST_ERROR = "DownloadPostError";
		private const String SYNC_MODE_NAME_DOWNLOAD_REJECT_DATE = "DownloadRejectDate";
		private const String SYNC_MODE_NAME_DOWNLOAD_SYNC_DATA = "DownloadSyncData";
		private const String SYNC_MODE_NAME_LOGIN = "Login";
		private const String SYNC_MODE_NAME_UPLOAD_SYNC_DATA = "UploadSyncData";

		// FILE_NAME_SYNC_INFO の中のパラメーター
		private const String SYNC_INFO_PARAM_DATE = "Date";

		// その他
		private const Int32 SYNC_INTERVAL = 200;
		private const String SYNC_NO_DATA = "NoData";
		private const Int32 SYNC_UPLOAD_BLOCK = 100;
		private const String SYNC_URL_DATE_FORMAT = "yyyyMMdd";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ダウンローダー
		private Downloader mDownloader;

		// 環境設定類
		private EnvironmentModel mEnvironment;

		// VM
		private MainWindowViewModel mMainWindowViewModel;

		// サーバーデータ再取得
		private Boolean mIsReget;

		// ログ（同期専用）
		private LogWriter mLogWriterSync;

		// 詳細ログ（同期専用）
		private LogWriter mLogWriterSyncDetail;

		// 排他制御
		private static Object mTaskLockSync = new Object();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// SyncInfo.txt の内容を返す
		// ＜引数＞ zip ファイルを解凍したフォルダー（末尾は '\\'）
		// --------------------------------------------------------------------
		private Dictionary<String, String> AnalyzeSyncInfo(String oExtractFolder)
		{
			Dictionary<String, String> aSyncInfos = new Dictionary<String, String>();
			String[] aSyncInfoLines = File.ReadAllLines(oExtractFolder + FILE_NAME_SYNC_INFO, Encoding.UTF8);
			foreach (String aLine in aSyncInfoLines)
			{
				Int32 aPos = aLine.IndexOf('=');
				if (aPos < 0)
				{
					continue;
				}
				aSyncInfos[aLine.Substring(0, aPos)] = aLine.Substring(aPos + 1);
			}
			return aSyncInfos;
		}

		// --------------------------------------------------------------------
		// Boolean を文字列で送信する同期データに変換
		// --------------------------------------------------------------------
		private String BooleanToSyncData(Boolean oBoolean)
		{
			if (oBoolean)
			{
				return "1";
			}
			else
			{
				return "0";
			}
		}

		// --------------------------------------------------------------------
		// 再取得の場合は楽曲情報データベースを初期化
		// --------------------------------------------------------------------
		private void CreateMusicInfoDbIfNeeded()
		{
			if (mIsReget)
			{
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
				{
					aMusicInfoDbInDisk.CreateDatabase();
				}
			}
		}

		// --------------------------------------------------------------------
		// アップロードを拒否されたレコードの更新日をサーバーからダウンロード
		// mEnvironment.YukaListerSettings.LastSyncDownloadDate を更新し、次回ダウンロード時に拒否レコードが上書きされるようにする
		// --------------------------------------------------------------------
		private void DownloadRejectDate()
		{
			try
			{
				String aRejectDateString = mDownloader.Download(SyncUrl(SYNC_MODE_NAME_DOWNLOAD_REJECT_DATE), Encoding.UTF8);
				if (String.IsNullOrEmpty(aRejectDateString))
				{
					throw new Exception("サーバーからの確認結果が空です。");
				}

				DateTime aRejectDate = DateTime.ParseExact(aRejectDateString, SYNC_URL_DATE_FORMAT, null);
				Double aRejectMjd = JulianDay.DateTimeToModifiedJulianDate(aRejectDate);
				if (aRejectMjd < mEnvironment.YukaListerSettings.LastSyncDownloadDate)
				{
					mEnvironment.YukaListerSettings.LastSyncDownloadDate = aRejectMjd;
				}
			}
			catch (Exception oExcep)
			{
				throw new Exception("アップロード拒否日付を確認できませんでした。\n" + oExcep.Message);
			}

		}

		// --------------------------------------------------------------------
		// 同期データをサーバーからダウンロード
		// LastSyncDownloadDate も再度ダウンロードする（同日にデータが追加されている可能性があるため）
		// ＜例外＞
		// --------------------------------------------------------------------
		private void DownloadSyncData(out Int32 oNumTotalDownloads, out Int32 oNumTotalImports)
		{
			// ダウンロード開始時刻の記録
			DateTime aTaskBeginDateTime = DateTime.UtcNow;

			if (mEnvironment.YukaListerSettings.LastSyncDownloadDate < YlConstants.INVALID_MJD)
			{
				mEnvironment.YukaListerSettings.LastSyncDownloadDate = YlConstants.INVALID_MJD;
			}
			DateTime aTargetDate = JulianDay.ModifiedJulianDateToDateTime(mEnvironment.YukaListerSettings.LastSyncDownloadDate);
			oNumTotalDownloads = 0;
			oNumTotalImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ダウンロード中... ");
			for (; ; )
			{
				mEnvironment.YukaListerSettings.LastSyncDownloadDate = JulianDay.DateTimeToModifiedJulianDate(aTargetDate);

				// ダウンロード
				String aDownloadPath = YlCommon.TempFilePath();
				mDownloader.Download(SyncUrl(SYNC_MODE_NAME_DOWNLOAD_SYNC_DATA) + "&Date=" + aTargetDate.ToString(SYNC_URL_DATE_FORMAT), aDownloadPath);

				FileInfo aFileInfo = new FileInfo(aDownloadPath);
				if (aFileInfo.Length == 0)
				{
					throw new Exception("サーバーからダウンロードしたファイルが空でした。");
				}
				if (aFileInfo.Length == SYNC_NO_DATA.Length)
				{
					// aTargetDate 以降の同期データがなかった
					break;
				}

				// 解凍
				String aExtractFolder = aDownloadPath + "_Extract\\";
				ZipFile.ExtractToDirectory(aDownloadPath, aExtractFolder);

				// 情報解析
				Dictionary<String, String> aSyncInfos = AnalyzeSyncInfo(aExtractFolder);

				// インポート
				mLogWriterSync.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "インポート中... " + aSyncInfos[SYNC_INFO_PARAM_DATE]);
				String[] aCsvs = Directory.GetFiles(aExtractFolder, "*" + Common.FILE_EXT_CSV);
				foreach (String aCsv in aCsvs)
				{
					Int32 aNumDownloads;
					Int32 aNumImports;
					ImportSyncData(aCsv, out aNumDownloads, out aNumImports);
					oNumTotalDownloads += aNumDownloads;
					oNumTotalImports += aNumImports;
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				// 状況
				mMainWindowViewModel.SetStatusBarMessageWithInvoke(Common.TRACE_EVENT_TYPE_STATUS, "同期データをダウンロード中...（" + aSyncInfos[SYNC_INFO_PARAM_DATE] + "）：合計 "
						+ oNumTotalDownloads.ToString("#,0") + " 件");

				// 日付更新
				aTargetDate = DateTime.ParseExact(aSyncInfos[SYNC_INFO_PARAM_DATE], SYNC_URL_DATE_FORMAT, null).AddDays(1);
				if (aTargetDate > DateTime.UtcNow.Date)
				{
					// 今日を超えたら抜ける（mEnvironment.YukaListerSettings.LastSyncDownloadDate は今日の日付のままにしておく）
					break;
				}

				Thread.Sleep(SYNC_INTERVAL);
				mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
			}

			mEnvironment.YukaListerSettings.LastSyncDownloadDate = JulianDay.DateTimeToModifiedJulianDate(aTaskBeginDateTime.Date);
		}

		// --------------------------------------------------------------------
		// 同期データのインポート
		// 常にローカルデータよりサーバーデータを優先する
		// --------------------------------------------------------------------
		private void ImportSyncData(String oCsvPath, out Int32 oNumDownloads, out Int32 oNumImports)
		{
			oNumDownloads = 0;
			oNumImports = 0;

			// CSV ロード
			List<List<String>> aCsvContents = CsvManager.LoadCsv(oCsvPath, Encoding.UTF8);
			if (aCsvContents.Count < 1)
			{
				return;
			}

			// サーバーの仕様変更によりカラム順序が変わっても対応できるよう、Dictionary にする
			List<Dictionary<String, String>> aSyncData = new List<Dictionary<String, String>>();
			for (Int32 i = 1; i < aCsvContents.Count; i++)
			{
				Dictionary<String, String> aRecord = new Dictionary<String, String>();
				for (Int32 j = 0; j < aCsvContents[0].Count; j++)
				{
					aRecord[aCsvContents[0][j]] = aCsvContents[i][j];
				}
				aSyncData.Add(aRecord);
			}
			oNumDownloads = aSyncData.Count;

			// インポート
			String aTableName = Path.GetFileNameWithoutExtension(oCsvPath);
			switch (aTableName)
			{
				case TSong.TABLE_NAME_SONG:
					oNumImports = ImportSyncDataTSong(aSyncData);
					break;
				case TPerson.TABLE_NAME_PERSON:
					oNumImports = ImportSyncDataTPerson(aSyncData);
					break;
				case TTieUp.TABLE_NAME_TIE_UP:
					oNumImports = ImportSyncDataTTieUp(aSyncData);
					break;
				case TTieUpGroup.TABLE_NAME_TIE_UP_GROUP:
					oNumImports = ImportSyncDataTTieUpGroup(aSyncData);
					break;
				case TMaker.TABLE_NAME_MAKER:
					oNumImports = ImportSyncDataTMaker(aSyncData);
					break;
				case TSongAlias.TABLE_NAME_SONG_ALIAS:
					oNumImports = ImportSyncDataTSongAlias(aSyncData);
					break;
				case TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS:
					oNumImports = ImportSyncDataTTieUpAlias(aSyncData);
					break;
				case TArtistSequence.TABLE_NAME_ARTIST_SEQUENCE:
					oNumImports = ImportSyncDataTArtistSequence(aSyncData);
					break;
				case TLyristSequence.TABLE_NAME_LYRIST_SEQUENCE:
					oNumImports = ImportSyncDataTLyristSequence(aSyncData);
					break;
				case TComposerSequence.TABLE_NAME_COMPOSER_SEQUENCE:
					oNumImports = ImportSyncDataTComposerSequence(aSyncData);
					break;
				case TArrangerSequence.TABLE_NAME_ARRANGER_SEQUENCE:
					oNumImports = ImportSyncDataTArrangerSequence(aSyncData);
					break;
				case TTieUpGroupSequence.TABLE_NAME_TIE_UP_GROUP_SEQUENCE:
					oNumImports = ImportSyncDataTTieUpGroupSequence(aSyncData);
					break;
				default:
					mLogWriterSync.ShowLogMessage(TraceEventType.Error, "ダウンロード：未対応のテーブルデータがありました：" + aTableName, true);
					break;
			}

			mLogWriterSync.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aTableName + "：ダウンロード " + oNumDownloads.ToString("#,0") + " 件、うちインポート "
					+ oNumImports.ToString("#,0") + " 件");
		}

		// --------------------------------------------------------------------
		// TArrangerSequence インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTArrangerSequence(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "編曲者紐付インポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TArrangerSequence> aTableArrangerSequence = aContext.GetTable<TArrangerSequence>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TArrangerSequence aDbNewRecord = new TArrangerSequence();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// IRcSequence
					aDbNewRecord.Sequence = SyncDataToInt32(aOneData[TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_SEQUENCE]);
					aDbNewRecord.LinkId = YlCommon.NormalizeDbString(aOneData[TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_LINK_ID]);

					// ID・連番 が既にテーブル内にあるか確認
					TArrangerSequence aDbExistRecord = aTableArrangerSequence.SingleOrDefault(x => x.Id == aDbNewRecord.Id && x.Sequence == aDbNewRecord.Sequence);
					if (aDbExistRecord == null)
					{
						// 新規登録（IRcSequence は紐付きの増減で無効データが生成され、ローカルデータと競合する可能性があるため、無効データも含めて登録する）
						aTableArrangerSequence.InsertOnSubmit(aDbNewRecord);
						mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
						aNumImports++;
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TArtistSequence インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTArtistSequence(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "歌手紐付インポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TArtistSequence> aTableArtistSequence = aContext.GetTable<TArtistSequence>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TArtistSequence aDbNewRecord = new TArtistSequence();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// IRcSequence
					aDbNewRecord.Sequence = SyncDataToInt32(aOneData[TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_SEQUENCE]);
					aDbNewRecord.LinkId = YlCommon.NormalizeDbString(aOneData[TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_LINK_ID]);

					// ID・連番 が既にテーブル内にあるか確認
					TArtistSequence aDbExistRecord = aTableArtistSequence.SingleOrDefault(x => x.Id == aDbNewRecord.Id && x.Sequence == aDbNewRecord.Sequence);
					if (aDbExistRecord == null)
					{
						// 新規登録（IRcSequence は紐付きの増減で無効データが生成され、ローカルデータと競合する可能性があるため、無効データも含めて登録する）
						aTableArtistSequence.InsertOnSubmit(aDbNewRecord);
						mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
						aNumImports++;
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TComposerSequence インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTComposerSequence(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "作曲者紐付インポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TComposerSequence> aTableComposerSequence = aContext.GetTable<TComposerSequence>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TComposerSequence aDbNewRecord = new TComposerSequence();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// IRcSequence
					aDbNewRecord.Sequence = SyncDataToInt32(aOneData[TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_SEQUENCE]);
					aDbNewRecord.LinkId = YlCommon.NormalizeDbString(aOneData[TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_LINK_ID]);

					// ID・連番 が既にテーブル内にあるか確認
					TComposerSequence aDbExistRecord = aTableComposerSequence.SingleOrDefault(x => x.Id == aDbNewRecord.Id && x.Sequence == aDbNewRecord.Sequence);
					if (aDbExistRecord == null)
					{
						// 新規登録（IRcSequence は紐付きの増減で無効データが生成され、ローカルデータと競合する可能性があるため、無効データも含めて登録する）
						aTableComposerSequence.InsertOnSubmit(aDbNewRecord);
						mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
						aNumImports++;
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TLyristSequence インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTLyristSequence(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "作詞者紐付インポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TLyristSequence> aTableLyristSequence = aContext.GetTable<TLyristSequence>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TLyristSequence aDbNewRecord = new TLyristSequence();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// IRcSequence
					aDbNewRecord.Sequence = SyncDataToInt32(aOneData[TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_SEQUENCE]);
					aDbNewRecord.LinkId = YlCommon.NormalizeDbString(aOneData[TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_LINK_ID]);

					// ID・連番 が既にテーブル内にあるか確認
					TLyristSequence aDbExistRecord = aTableLyristSequence.SingleOrDefault(x => x.Id == aDbNewRecord.Id && x.Sequence == aDbNewRecord.Sequence);
					if (aDbExistRecord == null)
					{
						// 新規登録（IRcSequence は紐付きの増減で無効データが生成され、ローカルデータと競合する可能性があるため、無効データも含めて登録する）
						aTableLyristSequence.InsertOnSubmit(aDbNewRecord);
						mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
						aNumImports++;
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TMaker インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTMaker(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "制作会社マスターインポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TMaker> aTableMaker = aContext.GetTable<TMaker>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TMaker aDbNewRecord = new TMaker();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TMaker.FIELD_NAME_MAKER_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TMaker.FIELD_NAME_MAKER_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TMaker.FIELD_NAME_MAKER_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TMaker.FIELD_NAME_MAKER_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// TMaster
					aDbNewRecord.Name = YlCommon.NormalizeDbString(aOneData[TMaker.FIELD_NAME_MAKER_NAME]);
					aDbNewRecord.Ruby = YlCommon.NormalizeDbRuby(aOneData[TMaker.FIELD_NAME_MAKER_RUBY]);
					aDbNewRecord.Keyword = YlCommon.NormalizeDbString(aOneData[TMaker.FIELD_NAME_MAKER_KEYWORD]);

					// メーカー ID が既にテーブル内にあるか確認
					TMaker aDbExistRecord = aTableMaker.SingleOrDefault(x => x.Id == aDbNewRecord.Id);
					if (aDbExistRecord == null)
					{
						if (aDbNewRecord.Invalid)
						{
							// 無効データを新規登録する意味は無いのでスキップ
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
						}
						else
						{
							// 新規登録
							aTableMaker.InsertOnSubmit(aDbNewRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
							aNumImports++;
						}
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TPerson インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTPerson(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "人物マスターインポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TPerson> aTablePerson = aContext.GetTable<TPerson>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TPerson aDbNewRecord = new TPerson();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TPerson.FIELD_NAME_PERSON_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TPerson.FIELD_NAME_PERSON_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TPerson.FIELD_NAME_PERSON_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TPerson.FIELD_NAME_PERSON_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// TMaster
					aDbNewRecord.Name = YlCommon.NormalizeDbString(aOneData[TPerson.FIELD_NAME_PERSON_NAME]);
					aDbNewRecord.Ruby = YlCommon.NormalizeDbRuby(aOneData[TPerson.FIELD_NAME_PERSON_RUBY]);
					aDbNewRecord.Keyword = YlCommon.NormalizeDbString(aOneData[TPerson.FIELD_NAME_PERSON_KEYWORD]);

					// 人物 ID が既にテーブル内にあるか確認
					TPerson aDbExistRecord = aTablePerson.SingleOrDefault(x => x.Id == aDbNewRecord.Id);
					if (aDbExistRecord == null)
					{
						if (aDbNewRecord.Invalid)
						{
							// 無効データを新規登録する意味は無いのでスキップ
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
						}
						else
						{
							// 新規登録
							aTablePerson.InsertOnSubmit(aDbNewRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
							aNumImports++;
						}
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TSong インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTSong(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲マスターインポート中...");

			//Debug.WriteLine("ImportSyncDataTSong() before aMusicInfoDbInDisk");
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TSong> aTableSong = aContext.GetTable<TSong>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TSong aDbNewRecord = new TSong();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TSong.FIELD_NAME_SONG_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TSong.FIELD_NAME_SONG_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TSong.FIELD_NAME_SONG_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TSong.FIELD_NAME_SONG_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// TMaster
					aDbNewRecord.Name = YlCommon.NormalizeDbString(aOneData[TSong.FIELD_NAME_SONG_NAME]);
					aDbNewRecord.Ruby = YlCommon.NormalizeDbRuby(aOneData[TSong.FIELD_NAME_SONG_RUBY]);
					aDbNewRecord.Keyword = YlCommon.NormalizeDbString(aOneData[TSong.FIELD_NAME_SONG_KEYWORD]);

					// TSong
					aDbNewRecord.ReleaseDate = SyncDataToDouble(aOneData[TSong.FIELD_NAME_SONG_RELEASE_DATE]);
					aDbNewRecord.TieUpId = YlCommon.NormalizeDbString(aOneData[TSong.FIELD_NAME_SONG_TIE_UP_ID]);
					aDbNewRecord.CategoryId = YlCommon.NormalizeDbString(aOneData[TSong.FIELD_NAME_SONG_CATEGORY_ID]);
					aDbNewRecord.OpEd = YlCommon.NormalizeDbString(aOneData[TSong.FIELD_NAME_SONG_OP_ED]);

					// 楽曲 ID が既にテーブル内にあるか確認
					TSong aDbExistRecord = aTableSong.SingleOrDefault(x => x.Id == aDbNewRecord.Id);
					if (aDbExistRecord == null)
					{
						if (aDbNewRecord.Invalid)
						{
							// 無効データを新規登録する意味は無いのでスキップ
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
						}
						else
						{
							// 新規登録
							aTableSong.InsertOnSubmit(aDbNewRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
							aNumImports++;
						}
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}
			//Debug.WriteLine("ImportSyncDataTSong() after aMusicInfoDbInDisk");

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TSongAlias インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTSongAlias(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲別名インポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TSongAlias> aTableSongAlias = aContext.GetTable<TSongAlias>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TSongAlias aDbNewRecord = new TSongAlias();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TSongAlias.FIELD_NAME_SONG_ALIAS_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TSongAlias.FIELD_NAME_SONG_ALIAS_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TSongAlias.FIELD_NAME_SONG_ALIAS_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TSongAlias.FIELD_NAME_SONG_ALIAS_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// TAlias
					aDbNewRecord.Alias = YlCommon.NormalizeDbString(aOneData[TSongAlias.FIELD_NAME_SONG_ALIAS_ALIAS]);
					aDbNewRecord.OriginalId = YlCommon.NormalizeDbString(aOneData[TSongAlias.FIELD_NAME_SONG_ALIAS_ORIGINAL_ID]);

					// メーカー ID が既にテーブル内にあるか確認
					TSongAlias aDbExistRecord = aTableSongAlias.SingleOrDefault(x => x.Id == aDbNewRecord.Id);
					if (aDbExistRecord == null)
					{
						if (aDbNewRecord.Invalid)
						{
							// 無効データを新規登録する意味は無いのでスキップ
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Alias);
						}
						else
						{
							// 新規登録
							aTableSongAlias.InsertOnSubmit(aDbNewRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Alias);
							aNumImports++;
						}
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Alias);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Alias);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TTieUp インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTTieUp(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップマスターインポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TTieUp> aTableTieUp = aContext.GetTable<TTieUp>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TTieUp aDbNewRecord = new TTieUp();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TTieUp.FIELD_NAME_TIE_UP_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TTieUp.FIELD_NAME_TIE_UP_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TTieUp.FIELD_NAME_TIE_UP_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TTieUp.FIELD_NAME_TIE_UP_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// TMaster
					aDbNewRecord.Name = YlCommon.NormalizeDbString(aOneData[TTieUp.FIELD_NAME_TIE_UP_NAME]);
					aDbNewRecord.Ruby = YlCommon.NormalizeDbRuby(aOneData[TTieUp.FIELD_NAME_TIE_UP_RUBY]);
					aDbNewRecord.Keyword = YlCommon.NormalizeDbString(aOneData[TTieUp.FIELD_NAME_TIE_UP_KEYWORD]);

					// TTieUp
					aDbNewRecord.CategoryId = YlCommon.NormalizeDbString(aOneData[TTieUp.FIELD_NAME_TIE_UP_CATEGORY_ID]);
					aDbNewRecord.MakerId = YlCommon.NormalizeDbString(aOneData[TTieUp.FIELD_NAME_TIE_UP_MAKER_ID]);
					aDbNewRecord.AgeLimit = SyncDataToInt32(aOneData[TTieUp.FIELD_NAME_TIE_UP_AGE_LIMIT]);
					aDbNewRecord.ReleaseDate = SyncDataToDouble(aOneData[TTieUp.FIELD_NAME_TIE_UP_RELEASE_DATE]);

					// タイアップ ID が既にテーブル内にあるか確認
					TTieUp aDbExistRecord = aTableTieUp.SingleOrDefault(x => x.Id == aDbNewRecord.Id);
					if (aDbExistRecord == null)
					{
						if (aDbNewRecord.Invalid)
						{
							// 無効データを新規登録する意味は無いのでスキップ
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
						}
						else
						{
							// 新規登録
							aTableTieUp.InsertOnSubmit(aDbNewRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
							aNumImports++;
						}
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TTieUpAlias インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTTieUpAlias(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップ別名インポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TTieUpAlias> aTableTieUpAlias = aContext.GetTable<TTieUpAlias>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TTieUpAlias aDbNewRecord = new TTieUpAlias();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// TAlias
					aDbNewRecord.Alias = YlCommon.NormalizeDbString(aOneData[TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ALIAS]);
					aDbNewRecord.OriginalId = YlCommon.NormalizeDbString(aOneData[TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ORIGINAL_ID]);

					// メーカー ID が既にテーブル内にあるか確認
					TTieUpAlias aDbExistRecord = aTableTieUpAlias.SingleOrDefault(x => x.Id == aDbNewRecord.Id);
					if (aDbExistRecord == null)
					{
						if (aDbNewRecord.Invalid)
						{
							// 無効データを新規登録する意味は無いのでスキップ
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Alias);
						}
						else
						{
							// 新規登録
							aTableTieUpAlias.InsertOnSubmit(aDbNewRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Alias);
							aNumImports++;
						}
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Alias);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Alias);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TTieUpGroup インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTTieUpGroup(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップグループマスターインポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TTieUpGroup> aTableTieUpGroup = aContext.GetTable<TTieUpGroup>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TTieUpGroup aDbNewRecord = new TTieUpGroup();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// TMaster
					aDbNewRecord.Name = YlCommon.NormalizeDbString(aOneData[TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_NAME]);
					aDbNewRecord.Ruby = YlCommon.NormalizeDbRuby(aOneData[TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_RUBY]);
					aDbNewRecord.Keyword = YlCommon.NormalizeDbString(aOneData[TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_KEYWORD]);

					// タイアップグループ ID が既にテーブル内にあるか確認
					TTieUpGroup aDbExistRecord = aTableTieUpGroup.SingleOrDefault(x => x.Id == aDbNewRecord.Id);
					if (aDbExistRecord == null)
					{
						if (aDbNewRecord.Invalid)
						{
							// 無効データを新規登録する意味は無いのでスキップ
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
						}
						else
						{
							// 新規登録
							aTableTieUpGroup.InsertOnSubmit(aDbNewRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
							aNumImports++;
						}
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Name);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// TTieUpGroupSequence インポート
		// --------------------------------------------------------------------
		private Int32 ImportSyncDataTTieUpGroupSequence(List<Dictionary<String, String>> oSyncData)
		{
			Int32 aNumImports = 0;
			mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "シリーズ紐付インポート中...");

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TTieUpGroupSequence> aTableTieUpGroupSequence = aContext.GetTable<TTieUpGroupSequence>();

				foreach (Dictionary<String, String> aOneData in oSyncData)
				{
					TTieUpGroupSequence aDbNewRecord = new TTieUpGroupSequence();

					// IRcBase
					aDbNewRecord.Id = YlCommon.NormalizeDbString(aOneData[TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_ID]);
					aDbNewRecord.Import = SyncDataToBoolean(aOneData[TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_IMPORT]);
					aDbNewRecord.Invalid = SyncDataToBoolean(aOneData[TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_INVALID]);
					aDbNewRecord.UpdateTime = SyncDataToDouble(aOneData[TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_UPDATE_TIME]);
					aDbNewRecord.Dirty = false;

					// IRcSequence
					aDbNewRecord.Sequence = SyncDataToInt32(aOneData[TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_SEQUENCE]);
					aDbNewRecord.LinkId = YlCommon.NormalizeDbString(aOneData[TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_LINK_ID]);

					// ID・連番 が既にテーブル内にあるか確認
					TTieUpGroupSequence aDbExistRecord = aTableTieUpGroupSequence.SingleOrDefault(x => x.Id == aDbNewRecord.Id && x.Sequence == aDbNewRecord.Sequence);
					if (aDbExistRecord == null)
					{
						// 新規登録（IRcSequence は紐付きの増減で無効データが生成され、ローカルデータと競合する可能性があるため、無効データも含めて登録する）
						aTableTieUpGroupSequence.InsertOnSubmit(aDbNewRecord);
						mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "新規登録：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
						aNumImports++;
					}
					else
					{
						if (aDbExistRecord.UpdateTime == aDbNewRecord.UpdateTime)
						{
							// 更新日時がサーバーと同じ場合は同期に支障ないので更新しない（ローカルで編集中の場合はローカルの編集が生きることになる）
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新不要：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
						}
						else
						{
							// 更新日時がサーバーと異なる場合はそのままではアップロードできないのでサーバーデータで上書きする
							Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
							mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "更新：" + aDbNewRecord.Id + " / " + aDbNewRecord.Sequence);
							aNumImports++;
						}
					}
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				aContext.SubmitChanges();
			}

			return aNumImports;
		}

		// --------------------------------------------------------------------
		// ネットワークが利用可能かどうか（簡易判定）
		// --------------------------------------------------------------------
		private Boolean IsNetworkAvailable()
		{
			try
			{
				mDownloader.Download("https://www.google.com/", Encoding.UTF8);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベース同期サーバーにログインする
		// --------------------------------------------------------------------
		private void LoginToSyncServer()
		{
			// ログイン情報送信
			Dictionary<String, String> aPostParams = new Dictionary<String, String>
			{
				// HTML Name 属性
				{ "Name",mEnvironment.YukaListerSettings.SyncAccount },
				{ "PW", YlCommon.Decrypt(mEnvironment.YukaListerSettings.SyncPassword) },
				{ "Mode", SYNC_MODE_NAME_LOGIN },
			};
			mLogWriterSync.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報データベース同期サーバーにログインします...");
			Post(aPostParams);

#if DEBUGz
			// クッキーの値を抜き出す
			CookieCollection cookies = oDownloader.ClientHandler.CookieContainer.GetCookies(new Uri(mEnvironment.YukaListerSettings.SyncServer));
			String aCookies = String.Empty;
			foreach (System.Net.Cookie cook in cookies)
			{
				aCookies += "<" + cook.Name + ">=(" + cook.Value + "), ";
			}
			mLogWriter.ShowLogMessage(TraceEventType.Verbose, "LoginToSyncServer() クッキー: " + aCookies);
#endif

			// ログイン結果確認
			String aErrMessage;
			if (SyncPostErrorExists(out aErrMessage))
			{
				throw new Exception("楽曲情報データベース同期サーバーにログインできませんでした。アカウント名およびパスワードを確認して下さい。");
			}

			mMainWindowViewModel.SetStatusBarMessageWithInvoke(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報データベース同期サーバーにログインしました。同期処理中です...");
			Thread.Sleep(SYNC_INTERVAL);
			mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// POST データ送信
		// ＜例外＞ Exception, OperationCanceledException
		// --------------------------------------------------------------------
		private void Post(Dictionary<String, String> oPostParams, Dictionary<String, String> oFiles = null)
		{
			try
			{
				mDownloader.Post(mEnvironment.YukaListerSettings.SyncServer + FILE_NAME_CP_MAIN, oPostParams, oFiles);
				Thread.Sleep(SYNC_INTERVAL);
				mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
			}
			catch (Exception)
			{
				if (IsNetworkAvailable())
				{
					// ネットワークが利用可能なのに例外になった場合は、サーバーアドレスが間違っている可能性が高い
					throw new Exception("楽曲情報データベース同期サーバーに接続できませんでした。サーバーアドレスを確認して下さい。");
				}
				else
				{
					throw new Exception("楽曲情報データベース同期サーバーに接続できませんでした。インターネットが使えません。");
				}
			}
		}

		// --------------------------------------------------------------------
		// TArrangerSequence の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTArrangerSequence(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_ID);
			oCsvHead.Add(TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_IMPORT);
			oCsvHead.Add(TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_INVALID);
			oCsvHead.Add(TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_UPDATE_TIME);

			// IRcSequence
			oCsvHead.Add(TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_SEQUENCE);
			oCsvHead.Add(TArrangerSequence.FIELD_NAME_ARRANGER_SEQUENCE_LINK_ID);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TArrangerSequence> aTableArrangerSequence = aContext.GetTable<TArrangerSequence>();
				IQueryable<TArrangerSequence> aQueryResult =
						from x in aTableArrangerSequence
						where x.Dirty == true
						select x;
				foreach (TArrangerSequence aArrangerSequence in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aArrangerSequence.Id);
					aRecord.Add(BooleanToSyncData(aArrangerSequence.Import));
					aRecord.Add(BooleanToSyncData(aArrangerSequence.Invalid));
					aRecord.Add(aArrangerSequence.UpdateTime.ToString());

					// IRcSequence
					aRecord.Add(aArrangerSequence.Sequence.ToString());
					aRecord.Add(aArrangerSequence.LinkId);

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TArtistSequence の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTArtistSequence(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_ID);
			oCsvHead.Add(TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_IMPORT);
			oCsvHead.Add(TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_INVALID);
			oCsvHead.Add(TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_UPDATE_TIME);

			// IRcSequence
			oCsvHead.Add(TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_SEQUENCE);
			oCsvHead.Add(TArtistSequence.FIELD_NAME_ARTIST_SEQUENCE_LINK_ID);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TArtistSequence> aTableArtistSequence = aContext.GetTable<TArtistSequence>();
				IQueryable<TArtistSequence> aQueryResult =
						from x in aTableArtistSequence
						where x.Dirty == true
						select x;
				foreach (TArtistSequence aArtistSequence in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aArtistSequence.Id);
					aRecord.Add(BooleanToSyncData(aArtistSequence.Import));
					aRecord.Add(BooleanToSyncData(aArtistSequence.Invalid));
					aRecord.Add(aArtistSequence.UpdateTime.ToString());

					// IRcSequence
					aRecord.Add(aArtistSequence.Sequence.ToString());
					aRecord.Add(aArtistSequence.LinkId);

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TComposerSequence の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTComposerSequence(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_ID);
			oCsvHead.Add(TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_IMPORT);
			oCsvHead.Add(TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_INVALID);
			oCsvHead.Add(TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_UPDATE_TIME);

			// IRcSequence
			oCsvHead.Add(TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_SEQUENCE);
			oCsvHead.Add(TComposerSequence.FIELD_NAME_COMPOSER_SEQUENCE_LINK_ID);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TComposerSequence> aTableComposerSequence = aContext.GetTable<TComposerSequence>();
				IQueryable<TComposerSequence> aQueryResult =
						from x in aTableComposerSequence
						where x.Dirty == true
						select x;
				foreach (TComposerSequence aComposerSequence in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aComposerSequence.Id);
					aRecord.Add(BooleanToSyncData(aComposerSequence.Import));
					aRecord.Add(BooleanToSyncData(aComposerSequence.Invalid));
					aRecord.Add(aComposerSequence.UpdateTime.ToString());

					// IRcSequence
					aRecord.Add(aComposerSequence.Sequence.ToString());
					aRecord.Add(aComposerSequence.LinkId);

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TLyristSequence の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTLyristSequence(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_ID);
			oCsvHead.Add(TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_IMPORT);
			oCsvHead.Add(TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_INVALID);
			oCsvHead.Add(TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_UPDATE_TIME);

			// IRcSequence
			oCsvHead.Add(TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_SEQUENCE);
			oCsvHead.Add(TLyristSequence.FIELD_NAME_LYRIST_SEQUENCE_LINK_ID);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TLyristSequence> aTableLyristSequence = aContext.GetTable<TLyristSequence>();
				IQueryable<TLyristSequence> aQueryResult =
						from x in aTableLyristSequence
						where x.Dirty == true
						select x;
				foreach (TLyristSequence aLyristSequence in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aLyristSequence.Id);
					aRecord.Add(BooleanToSyncData(aLyristSequence.Import));
					aRecord.Add(BooleanToSyncData(aLyristSequence.Invalid));
					aRecord.Add(aLyristSequence.UpdateTime.ToString());

					// IRcSequence
					aRecord.Add(aLyristSequence.Sequence.ToString());
					aRecord.Add(aLyristSequence.LinkId);

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TMaker の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTMaker(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TMaker.FIELD_NAME_MAKER_ID);
			oCsvHead.Add(TMaker.FIELD_NAME_MAKER_IMPORT);
			oCsvHead.Add(TMaker.FIELD_NAME_MAKER_INVALID);
			oCsvHead.Add(TMaker.FIELD_NAME_MAKER_UPDATE_TIME);

			// TMaster
			oCsvHead.Add(TMaker.FIELD_NAME_MAKER_NAME);
			oCsvHead.Add(TMaker.FIELD_NAME_MAKER_RUBY);
			oCsvHead.Add(TMaker.FIELD_NAME_MAKER_KEYWORD);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TMaker> aTableMaker = aContext.GetTable<TMaker>();
				IQueryable<TMaker> aQueryResult =
						from x in aTableMaker
						where x.Dirty == true
						select x;
				foreach (TMaker aMaker in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aMaker.Id);
					aRecord.Add(BooleanToSyncData(aMaker.Import));
					aRecord.Add(BooleanToSyncData(aMaker.Invalid));
					aRecord.Add(aMaker.UpdateTime.ToString());

					// TMaster
					aRecord.Add(aMaker.Name);
					aRecord.Add(aMaker.Ruby);
					aRecord.Add(aMaker.Keyword);

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TPerson の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTPerson(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TPerson.FIELD_NAME_PERSON_ID);
			oCsvHead.Add(TPerson.FIELD_NAME_PERSON_IMPORT);
			oCsvHead.Add(TPerson.FIELD_NAME_PERSON_INVALID);
			oCsvHead.Add(TPerson.FIELD_NAME_PERSON_UPDATE_TIME);

			// TMaster
			oCsvHead.Add(TPerson.FIELD_NAME_PERSON_NAME);
			oCsvHead.Add(TPerson.FIELD_NAME_PERSON_RUBY);
			oCsvHead.Add(TPerson.FIELD_NAME_PERSON_KEYWORD);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TPerson> aTablePerson = aContext.GetTable<TPerson>();
				IQueryable<TPerson> aQueryResult =
						from x in aTablePerson
						where x.Dirty == true
						select x;
				foreach (TPerson aPerson in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aPerson.Id);
					aRecord.Add(BooleanToSyncData(aPerson.Import));
					aRecord.Add(BooleanToSyncData(aPerson.Invalid));
					aRecord.Add(aPerson.UpdateTime.ToString());

					// TMaster
					aRecord.Add(aPerson.Name);
					aRecord.Add(aPerson.Ruby);
					aRecord.Add(aPerson.Keyword);

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TSong の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTSong(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TSong.FIELD_NAME_SONG_ID);
			oCsvHead.Add(TSong.FIELD_NAME_SONG_IMPORT);
			oCsvHead.Add(TSong.FIELD_NAME_SONG_INVALID);
			oCsvHead.Add(TSong.FIELD_NAME_SONG_UPDATE_TIME);

			// TMaster
			oCsvHead.Add(TSong.FIELD_NAME_SONG_NAME);
			oCsvHead.Add(TSong.FIELD_NAME_SONG_RUBY);
			oCsvHead.Add(TSong.FIELD_NAME_SONG_KEYWORD);

			// TSong
			oCsvHead.Add(TSong.FIELD_NAME_SONG_RELEASE_DATE);
			oCsvHead.Add(TSong.FIELD_NAME_SONG_TIE_UP_ID);
			oCsvHead.Add(TSong.FIELD_NAME_SONG_CATEGORY_ID);
			oCsvHead.Add(TSong.FIELD_NAME_SONG_OP_ED);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TSong> aTableSong = aContext.GetTable<TSong>();
				IQueryable<TSong> aQueryResult =
						from x in aTableSong
						where x.Dirty == true
						select x;
				foreach (TSong aSong in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aSong.Id);
					aRecord.Add(BooleanToSyncData(aSong.Import));
					aRecord.Add(BooleanToSyncData(aSong.Invalid));
					aRecord.Add(aSong.UpdateTime.ToString());

					// TMaster
					aRecord.Add(aSong.Name);
					aRecord.Add(aSong.Ruby);
					aRecord.Add(aSong.Keyword);

					// TSong
					aRecord.Add(aSong.ReleaseDate.ToString());
					aRecord.Add(aSong.TieUpId);
					aRecord.Add(aSong.CategoryId);
					aRecord.Add(aSong.OpEd);

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TSongAlias の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTSongAlias(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TSongAlias.FIELD_NAME_SONG_ALIAS_ID);
			oCsvHead.Add(TSongAlias.FIELD_NAME_SONG_ALIAS_IMPORT);
			oCsvHead.Add(TSongAlias.FIELD_NAME_SONG_ALIAS_INVALID);
			oCsvHead.Add(TSongAlias.FIELD_NAME_SONG_ALIAS_UPDATE_TIME);

			// TAlias
			oCsvHead.Add(TSongAlias.FIELD_NAME_SONG_ALIAS_ALIAS);
			oCsvHead.Add(TSongAlias.FIELD_NAME_SONG_ALIAS_ORIGINAL_ID);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TSongAlias> aTableSongAlias = aContext.GetTable<TSongAlias>();
				IQueryable<TSongAlias> aQueryResult =
						from x in aTableSongAlias
						where x.Dirty == true
						select x;
				foreach (TSongAlias aSongAlias in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aSongAlias.Id);
					aRecord.Add(BooleanToSyncData(aSongAlias.Import));
					aRecord.Add(BooleanToSyncData(aSongAlias.Invalid));
					aRecord.Add(aSongAlias.UpdateTime.ToString());

					// TAlias
					aRecord.Add(aSongAlias.Alias);
					aRecord.Add(aSongAlias.OriginalId);

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TTieUp の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTTieUp(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_ID);
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_IMPORT);
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_INVALID);
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_UPDATE_TIME);

			// TMaster
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_NAME);
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_RUBY);
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_KEYWORD);

			// TTieUp
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_CATEGORY_ID);
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_MAKER_ID);
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_AGE_LIMIT);
			oCsvHead.Add(TTieUp.FIELD_NAME_TIE_UP_RELEASE_DATE);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TTieUp> aTableTieUp = aContext.GetTable<TTieUp>();
				IQueryable<TTieUp> aQueryResult =
						from x in aTableTieUp
						where x.Dirty == true
						select x;
				foreach (TTieUp aTieUp in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aTieUp.Id);
					aRecord.Add(BooleanToSyncData(aTieUp.Import));
					aRecord.Add(BooleanToSyncData(aTieUp.Invalid));
					aRecord.Add(aTieUp.UpdateTime.ToString());

					// TMaster
					aRecord.Add(aTieUp.Name);
					aRecord.Add(aTieUp.Ruby);
					aRecord.Add(aTieUp.Keyword);

					// TTieUp
					aRecord.Add(aTieUp.CategoryId);
					aRecord.Add(aTieUp.MakerId);
					aRecord.Add(aTieUp.AgeLimit.ToString());
					aRecord.Add(aTieUp.ReleaseDate.ToString());

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TTieUpAlias の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTTieUpAlias(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ID);
			oCsvHead.Add(TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_IMPORT);
			oCsvHead.Add(TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_INVALID);
			oCsvHead.Add(TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_UPDATE_TIME);

			// TAlias
			oCsvHead.Add(TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ALIAS);
			oCsvHead.Add(TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ORIGINAL_ID);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TTieUpAlias> aTableTieUpAlias = aContext.GetTable<TTieUpAlias>();
				IQueryable<TTieUpAlias> aQueryResult =
						from x in aTableTieUpAlias
						where x.Dirty == true
						select x;
				foreach (TTieUpAlias aTieUpAlias in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aTieUpAlias.Id);
					aRecord.Add(BooleanToSyncData(aTieUpAlias.Import));
					aRecord.Add(BooleanToSyncData(aTieUpAlias.Invalid));
					aRecord.Add(aTieUpAlias.UpdateTime.ToString());

					// TAlias
					aRecord.Add(aTieUpAlias.Alias);
					aRecord.Add(aTieUpAlias.OriginalId);

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TTieUpGroup の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTTieUpGroup(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_ID);
			oCsvHead.Add(TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_IMPORT);
			oCsvHead.Add(TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_INVALID);
			oCsvHead.Add(TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_UPDATE_TIME);

			// TMaster
			oCsvHead.Add(TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_NAME);
			oCsvHead.Add(TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_RUBY);
			oCsvHead.Add(TTieUpGroup.FIELD_NAME_TIE_UP_GROUP_KEYWORD);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TTieUpGroup> aTableTieUpGroup = aContext.GetTable<TTieUpGroup>();
				IQueryable<TTieUpGroup> aQueryResult =
						from x in aTableTieUpGroup
						where x.Dirty == true
						select x;
				foreach (TTieUpGroup aTieUpGroup in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aTieUpGroup.Id);
					aRecord.Add(BooleanToSyncData(aTieUpGroup.Import));
					aRecord.Add(BooleanToSyncData(aTieUpGroup.Invalid));
					aRecord.Add(aTieUpGroup.UpdateTime.ToString());

					// TMaster
					aRecord.Add(aTieUpGroup.Name);
					aRecord.Add(aTieUpGroup.Ruby);
					aRecord.Add(aTieUpGroup.Keyword);

					oCsvContents.Add(aRecord);
				}
			}
		}

		// --------------------------------------------------------------------
		// TTieUpGroupSequence の同期アップロードデータを準備
		// --------------------------------------------------------------------
		private void PrepareUploadDataTTieUpGroupSequence(List<String> oCsvHead, List<List<String>> oCsvContents)
		{
			// ヘッダー
			// IRcBase（Dirty を除く）
			oCsvHead.Add(TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_ID);
			oCsvHead.Add(TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_IMPORT);
			oCsvHead.Add(TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_INVALID);
			oCsvHead.Add(TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_UPDATE_TIME);

			// IRcSequence
			oCsvHead.Add(TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_SEQUENCE);
			oCsvHead.Add(TTieUpGroupSequence.FIELD_NAME_TIE_UP_GROUP_SEQUENCE_LINK_ID);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TTieUpGroupSequence> aTableTieUpGroupSequence = aContext.GetTable<TTieUpGroupSequence>();
				IQueryable<TTieUpGroupSequence> aQueryResult =
						from x in aTableTieUpGroupSequence
						where x.Dirty == true
						select x;
				foreach (TTieUpGroupSequence aTieUpGroupSequence in aQueryResult)
				{
					List<String> aRecord = new List<String>();

					// IRcBase（Dirty を除く）
					aRecord.Add(aTieUpGroupSequence.Id);
					aRecord.Add(BooleanToSyncData(aTieUpGroupSequence.Import));
					aRecord.Add(BooleanToSyncData(aTieUpGroupSequence.Invalid));
					aRecord.Add(aTieUpGroupSequence.UpdateTime.ToString());

					// IRcSequence
					aRecord.Add(aTieUpGroupSequence.Sequence.ToString());
					aRecord.Add(aTieUpGroupSequence.LinkId);

					oCsvContents.Add(aRecord);
				}
			}
		}

#if false
		// --------------------------------------------------------------------
		// バックグラウンド動作状況を表示
		// 画面下部のステータスバーに表示（主に同期など、直ちにはリスト化に影響しない情報）
		// --------------------------------------------------------------------
		private void SetStatusLabelMessageWithInvoke(TraceEventType oTraceEventType, String oMsg)
		{
			mStatusLabel.Dispatcher.Invoke(new Action(() =>
			{
				YlCommon.SetStatusLabelMessage(mStatusLabel, oTraceEventType, oMsg);
			}));
			mLogWriterSync.ShowLogMessage(oTraceEventType, oMsg, true);
		}
#endif

		// --------------------------------------------------------------------
		// 文字列で受信した同期データを Boolean に変換
		// --------------------------------------------------------------------
		private Boolean SyncDataToBoolean(String oString)
		{
			if (String.IsNullOrEmpty(oString))
			{
				return false;
			}

			return oString[0] != '0';
		}

		// --------------------------------------------------------------------
		// 文字列で受信した同期データを Double に変換
		// --------------------------------------------------------------------
		private Double SyncDataToDouble(String oString)
		{
			Double aDouble;
			Double.TryParse(oString, out aDouble);
			return aDouble;
		}

		// --------------------------------------------------------------------
		// 文字列で受信した同期データを Int32 に変換
		// --------------------------------------------------------------------
		private Int32 SyncDataToInt32(String oString)
		{
			Int32 aInt32;
			Int32.TryParse(oString, out aInt32);
			return aInt32;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベース同期タスク実行
		// ワーカースレッドで実行されることが前提
		// --------------------------------------------------------------------
		private void SyncMusicInfoDbByWorker(Object oDummy)
		{
			try
			{
				mDownloader = new Downloader();

				// ログイン
				LoginToSyncServer();

				// 楽曲情報データベースバックアップ
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(mEnvironment))
				{
					aMusicInfoDbInDisk.Backup();
				}

				// 再取得の場合は楽曲情報データベース初期化
				CreateMusicInfoDbIfNeeded();

				// ダウンロード
				Int32 aNumTotalDownloads;
				Int32 aNumTotalImports;
				DownloadSyncData(out aNumTotalDownloads, out aNumTotalImports);

				// アップロード
				Int32 aNumTotalUploads;
				UploadSyncData(out aNumTotalUploads);
				if (aNumTotalUploads > 0)
				{
					// アップロードを行った場合は、自身がアップロードしたデータの更新日・Dirty を更新するために再ダウンロードが必要
					DownloadSyncData(out aNumTotalDownloads, out aNumTotalImports);
				}

				// 完了表示
				mMainWindowViewModel.SetStatusBarMessageWithInvoke(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報データベース同期完了（ダウンロード"
						+ (aNumTotalDownloads == 0 ? "無" : " " + aNumTotalDownloads.ToString("#,0") + " 件、うち " + aNumTotalImports.ToString("#,0") + " 件インポート")
						+ "、アップロード" + (aNumTotalUploads == 0 ? "無" : " " + aNumTotalUploads.ToString("#,0") + " 件") + "）");
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception oExcep)
			{
				mMainWindowViewModel.SetStatusBarMessageWithInvoke(TraceEventType.Error, "楽曲情報データベース同期タスク実行時エラー：" + oExcep.Message);
				mLogWriterSync.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				if (mDownloader != null)
				{
					mDownloader.Dispose();
				}
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベース同期サーバーへの POST でエラーが発生したかどうか
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private Boolean SyncPostErrorExists(out String oErrMessage)
		{
			try
			{
				String aStatus = mDownloader.Download(SyncUrl(SYNC_MODE_NAME_DOWNLOAD_POST_ERROR), Encoding.UTF8);
				if (String.IsNullOrEmpty(aStatus))
				{
					throw new Exception("サーバーからの確認結果が空です。");
				}
				if (aStatus[0] == '0')
				{
					oErrMessage = null;
					return false;
				}

				oErrMessage = aStatus.Substring(1);
				return true;
			}
			catch (Exception oExcep)
			{
				throw new Exception("楽曲情報データベースへの送信結果を確認できませんでした。\n" + oExcep.Message);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベース同期コマンド URL
		// --------------------------------------------------------------------
		private String SyncUrl(String oMode)
		{
			return mEnvironment.YukaListerSettings.SyncServer + FILE_NAME_CP_MAIN + "?Mode=" + oMode;
		}

		// --------------------------------------------------------------------
		// 同期データをサーバーへアップロード
		// ＜返値＞ アップロード件数合計
		// --------------------------------------------------------------------
		private void UploadSyncData(out Int32 oNumTotalUploads)
		{
			oNumTotalUploads = 0;
			for (MusicInfoDbTables i = 0; i < MusicInfoDbTables.__End__; i++)
			{
				// アップロードデータ準備
				List<String> aCsvHead = new List<String>();
				List<List<String>> aCsvContents = new List<List<String>>();
				switch (i)
				{
					case MusicInfoDbTables.TSong:
						PrepareUploadDataTSong(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TPerson:
						PrepareUploadDataTPerson(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TTieUp:
						PrepareUploadDataTTieUp(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TTieUpGroup:
						PrepareUploadDataTTieUpGroup(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TMaker:
						PrepareUploadDataTMaker(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TSongAlias:
						PrepareUploadDataTSongAlias(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TTieUpAlias:
						PrepareUploadDataTTieUpAlias(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TArtistSequence:
						PrepareUploadDataTArtistSequence(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TLyristSequence:
						PrepareUploadDataTLyristSequence(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TComposerSequence:
						PrepareUploadDataTComposerSequence(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TArrangerSequence:
						PrepareUploadDataTArrangerSequence(aCsvHead, aCsvContents);
						break;
					case MusicInfoDbTables.TTieUpGroupSequence:
						PrepareUploadDataTTieUpGroupSequence(aCsvHead, aCsvContents);
						break;
				}
				if (aCsvContents.Count == 0)
				{
					continue;
				}

				// 一定数ずつアップロード
				mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "アップロード中... " + YlConstants.MUSIC_INFO_DB_TABLE_NAMES[(Int32)i]);
				for (Int32 j = 0; j < (aCsvContents.Count + SYNC_UPLOAD_BLOCK - 1) / SYNC_UPLOAD_BLOCK; j++)
				{
					List<List<String>> aUploadContents = new List<List<String>>();
					aUploadContents.Add(aCsvHead);
					aUploadContents.AddRange(aCsvContents.GetRange(j * SYNC_UPLOAD_BLOCK, Math.Min(SYNC_UPLOAD_BLOCK, aCsvContents.Count - j * SYNC_UPLOAD_BLOCK)));
					String aUploadFolder = YlCommon.TempFilePath();
					Directory.CreateDirectory(aUploadFolder);
					String aUploadPath = aUploadFolder + "\\" + YlConstants.MUSIC_INFO_DB_TABLE_NAMES[(Int32)i];
					CsvManager.SaveCsv(aUploadPath, aUploadContents, "\n", Encoding.UTF8);
					Dictionary<String, String> aUploadFiles = new Dictionary<String, String>
					{
						{ "File", aUploadPath },
					};
					Dictionary<String, String> aPostParams = new Dictionary<String, String>
					{
						// HTML Name 属性
						{ "Mode", SYNC_MODE_NAME_UPLOAD_SYNC_DATA },
					};
					for (Int32 k = 1; k < aUploadContents.Count; k++)
					{
						mLogWriterSyncDetail.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "データ：" + aUploadContents[k][0]);
					}
					Post(aPostParams, aUploadFiles);

					// アップロード結果確認
					String aErrMessage;
					if (SyncPostErrorExists(out aErrMessage))
					{
						throw new Exception("同期データをアップロードできませんでした：" + aErrMessage);
					}

					// 状況
					oNumTotalUploads += aUploadContents.Count - 1;
					mMainWindowViewModel.SetStatusBarMessageWithInvoke(Common.TRACE_EVENT_TYPE_STATUS, "同期データをアップロード中... 合計 "
							+ oNumTotalUploads.ToString("#,0") + " 件");
					Thread.Sleep(SYNC_INTERVAL);
					mEnvironment.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			DownloadRejectDate();
		}


	}
	// public class SyncClient ___END___

}
// namespace YukaLister.Models.Http ___END___
