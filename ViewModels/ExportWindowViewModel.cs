// ============================================================================
// 
// エクスポートウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// エクスポートしたデータベースにはインデックスが付与されない。
// そのまま楽曲情報データベースとして使われることは想定しておらず、あくまでもインポートして使う。
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using YukaLister.Models;
using System.Diagnostics;
using Shinta;
using YukaLister.Models.SharedMisc;
using YukaLister.Models.Database;
using System.Data.Linq;
using System.Data.SQLite;
using System.IO.Compression;
using System.IO;
using System.Threading;

namespace YukaLister.ViewModels
{
	public class ExportWindowViewModel : ImportExportWindowViewModel
	{
		// Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// エクスポート先
		public String ExportYukaListerPath { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// 本関数を呼ぶ前に Environment を設定しておく必要がある
		// --------------------------------------------------------------------
		public override void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				Title = "ゆかりすたー情報ファイルのエクスポート";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				Kind = "エクスポート";

				base.Initialize();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "エクスポートウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// エクスポート処理
		// --------------------------------------------------------------------
		protected override void ImportExport()
		{
			try
			{
				CheckExportInfo();

				String aTempExportPath = YlCommon.TempFilePath();
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				using (DataContext aMusicInfoDbContext = new DataContext(aMusicInfoDbInDisk.Connection))
				using (DatabaseInDisk aExportDbInDisk = new DatabaseInDisk(Environment, aTempExportPath))
				using (SQLiteCommand aExportDbCmd = new SQLiteCommand(aExportDbInDisk.Connection))
				using (DataContext aExportDbContext = new DataContext(aExportDbInDisk.Connection))
				{
					// 準備
					aExportDbInDisk.CreatePropertyTable();

					// 有効なマスターテーブルをエクスポート（カテゴリー以外）
					ExportTable<TSong>("楽曲", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TPerson>("人物", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TTieUp>("タイアップ", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TTieUpGroup>("シリーズ", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TMaker>("制作会社", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TTag>("タグ", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);

					// 有効な別名テーブルをエクスポート
					ExportTable<TSongAlias>("楽曲別名", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TTieUpAlias>("タイアップ別名", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);

					// 有効な紐付テーブルをエクスポート
					ExportTable<TArtistSequence>("歌手紐付", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TLyristSequence>("作詞者紐付", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TComposerSequence>("作曲者紐付", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TArrangerSequence>("編曲者紐付", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TTieUpGroupSequence>("シリーズ紐付", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
					ExportTable<TTagSequence>("タグ紐付", aMusicInfoDbContext, aExportDbCmd, aExportDbContext);
				}

				// 古いファイルを削除
				try
				{
					File.Delete(ExportYukaListerPath);
				}
				catch (Exception)
				{
				}

				// 出力
				// データベースファイルをそのまま圧縮しようとするとプロセスが使用中というエラーになることがある（2 回に 1 回くらい）ため、
				// いったんデータベースファイルをコピーしてから圧縮する
				Description = "保存しています...";
				String aTempFolder = YlCommon.TempFilePath();
				Directory.CreateDirectory(aTempFolder);
				File.Copy(aTempExportPath, aTempFolder + "\\" + FILE_NAME_EXPORT_MUSIC_INFO);
				ZipFile.CreateFromDirectory(aTempFolder, ExportYukaListerPath, CompressionLevel.Optimal, false);
				mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
			}
			catch (OperationCanceledException)
			{
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ゆかりすたー情報ファイルのエクスポートを中止しました。");
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ゆかりすたー情報ファイルエクスポート時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ファイル名
		private const String FILE_NAME_EXPORT_MUSIC_INFO = "ExportMusicInfo" + Common.FILE_EXT_SQLITE3;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// エクスポート用の設定確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckExportInfo()
		{
			if (String.IsNullOrEmpty(ExportYukaListerPath))
			{
				throw new Exception("エクスポート先ファイルを指定してください。");
			}
		}

		// --------------------------------------------------------------------
		// 1 つのテーブルをエクスポート
		// --------------------------------------------------------------------
		private void ExportTable<T>(String oName, DataContext oMusicInfoDbContext, SQLiteCommand oExportDbCmd, DataContext oExportDbContext) where T : class, IRcBase
		{
			Description = oName + "情報をエクスポート中...";

			LinqUtils.CreateTable(oExportDbCmd, typeof(T));
			oExportDbContext.SubmitChanges();

			Table<T> aMusicInfoDbTable = oMusicInfoDbContext.GetTable<T>();
			IQueryable<T> aQueryResult =
					from x in aMusicInfoDbTable
					where !x.Invalid
					select x;

			Table<T> aExportDbTable = oExportDbContext.GetTable<T>();
			aExportDbTable.InsertAllOnSubmit(aQueryResult);

			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			oExportDbContext.SubmitChanges();
		}
	}
	// public class ExportWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
