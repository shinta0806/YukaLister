// ============================================================================
// 
// 報告されたリストの問題を管理
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Livet;
using Livet.Messaging;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;
using YukaLister.ViewModels;

namespace YukaLister.Models
{
	public class ReportModel : NotificationObject
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public ReportModel(EnvironmentModel oEnvironment, MainWindowViewModel oMainWindowViewModel)
		{
			mEnvironment = oEnvironment;
			mMainWindowViewModel = oMainWindowViewModel;
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void ButtonReportsClicked()
		{
			// ViewModel 経由でウィンドウを開く
			using (ViewTReportsWindowViewModel aViewReportsWindowViewModel = new ViewTReportsWindowViewModel())
			{
				aViewReportsWindowViewModel.Environment = mEnvironment;
				mMainWindowViewModel.Messenger.Raise(new TransitionMessage(aViewReportsWindowViewModel, "OpenViewTReportsWindow"));
			}
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		public void Initialize()
		{
			UpdateReportsBadge();

			// リスト問題報告データベース監視
			mFileSystemWatcherReportDb = new FileSystemWatcher();
			mFileSystemWatcherReportDb.Created += new FileSystemEventHandler(FileSystemWatcherReportDb_Changed);
			mFileSystemWatcherReportDb.Deleted += new FileSystemEventHandler(FileSystemWatcherReportDb_Changed);
			mFileSystemWatcherReportDb.Changed += new FileSystemEventHandler(FileSystemWatcherReportDb_Changed);
			SetFileSystemWatcherReportDb();
		}

		// --------------------------------------------------------------------
		// メインウィンドウのバッジを更新
		// --------------------------------------------------------------------
		public void UpdateReportsBadge()
		{
			Int32 aNumProgress;
			using (ReportDatabaseInDisk aReportDbInDisk = new ReportDatabaseInDisk(mEnvironment))
			using (DataContext aContext = new DataContext(aReportDbInDisk.Connection))
			{
				Table<TReport> aTableReport = aContext.GetTable<TReport>();
				IQueryable<TReport> aQueryResult =
						from x in aTableReport
						where x.Status <= (Int32)ReportStatus.Progress
						orderby x.RegistTime descending
						select x;
				aNumProgress = aQueryResult.Count();
			}

			if (aNumProgress == 0)
			{
				mMainWindowViewModel.ReportsBadge = null;
			}
			else
			{
				mMainWindowViewModel.ReportsBadge = aNumProgress.ToString();
			}
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 環境設定類
		private EnvironmentModel mEnvironment;

		// VM
		private MainWindowViewModel mMainWindowViewModel;

		// リスト問題報告データベース監視用
		private FileSystemWatcher mFileSystemWatcherReportDb;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void FileSystemWatcherReportDb_Changed(Object oSender, FileSystemEventArgs oFileSystemEventArgs)
		{
			mMainWindowViewModel.SetStatusBarMessageWithInvoke(TraceEventType.Information, "リスト問題報告データベースが更新されました。");
			UpdateReportsBadge();
		}

		// --------------------------------------------------------------------
		// リスト問題報告データベースの監視設定
		// --------------------------------------------------------------------
		private void SetFileSystemWatcherReportDb()
		{
			mFileSystemWatcherReportDb.Path = Path.GetDirectoryName(mEnvironment.YukaListerSettings.ReportDbInDiskPath());
			mFileSystemWatcherReportDb.Filter = Path.GetFileName(mEnvironment.YukaListerSettings.ReportDbInDiskPath());
			mFileSystemWatcherReportDb.EnableRaisingEvents = true;
		}


	}
	// public class ReportModel ___END___
}
// namespace YukaLister.Models ___END___
