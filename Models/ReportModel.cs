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

		// ====================================================================
		// private メンバー関数
		// ====================================================================


	}
	// public class ReportModel ___END___
}
// namespace YukaLister.Models ___END___
