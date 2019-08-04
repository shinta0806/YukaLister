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
using System.Linq;
using System.Text;

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
