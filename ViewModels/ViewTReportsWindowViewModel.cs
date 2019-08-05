// ============================================================================
// 
// リスト問題報告一覧ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using Shinta;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Text;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class ViewTReportsWindowViewModel : ViewModel
	{
		// Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// ウィンドウタイトル（デフォルトが null だと実行時にエラーが発生するので Empty にしておく）
		private String mTitle = String.Empty;
		public String Title
		{
			get => mTitle;
			set => RaisePropertyChangedIfSet(ref mTitle, value);
		}

		// 要対応のみ表示
		private Boolean mShowOpened;
		public Boolean ShowOpened
		{
			get => mShowOpened;
			set
			{
				if (RaisePropertyChangedIfSet(ref mShowOpened, value))
				{
					UpdateTReports();
				}
			}
		}

		// すべて表示
		private Boolean mShowAll;
		public Boolean ShowAll
		{
			get => mShowAll;
			set
			{
				if (RaisePropertyChangedIfSet(ref mShowAll, value))
				{
					UpdateTReports();
				}
			}
		}

		// 報告群
		private List<TReport> mTReports;
		public List<TReport> TReports
		{
			get => mTReports;
			set => RaisePropertyChangedIfSet(ref mTReports, value);
		}

		// 選択された報告
		private TReport mSelectedTReport;
		public TReport SelectedTReport
		{
			get => mSelectedTReport;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedTReport, value))
				{
					ButtonEditDetailClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region ヘルプリンクの制御
		public ListenerCommand<String> HelpClickedCommand
		{
			get => Environment?.HelpClickedCommand;
		}
		#endregion

		#region データグリッドダブルクリックの制御
		private ViewModelCommand mDataGridDoubleClickedCommand;

		public ViewModelCommand DataGridDoubleClickedCommand
		{
			get
			{
				if (mDataGridDoubleClickedCommand == null)
				{
					mDataGridDoubleClickedCommand = new ViewModelCommand(DataGridDoubleClicked);
				}
				return mDataGridDoubleClickedCommand;
			}
		}

		public void DataGridDoubleClicked()
		{
			try
			{
				EditDetail();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "データグリッドダブルクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 詳細ボタンの制御
		private ViewModelCommand mButtonEditDetailClickedCommand;

		public ViewModelCommand ButtonEditDetailClickedCommand
		{
			get
			{
				if (mButtonEditDetailClickedCommand == null)
				{
					mButtonEditDetailClickedCommand = new ViewModelCommand(ButtonEditDetailClicked, CanButtonEditDetailClicked);
				}
				return mButtonEditDetailClickedCommand;
			}
		}

		public Boolean CanButtonEditDetailClicked()
		{
			return SelectedTReport != null;
		}

		public void ButtonEditDetailClicked()
		{
			try
			{
				EditDetail();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "詳細ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// 本関数を呼ぶ前に Environment 等を設定しておく必要がある
		// --------------------------------------------------------------------
		public void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");

			try
			{
				// タイトルバー
				Title = "リスト問題報告一覧";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif
				// データベース確保
				mReportDbInDisk = new ReportDatabaseInDisk(Environment);

				// 絞り込み：データベース確保の後でやる
				ShowOpened = true;

			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "リスト問題報告一覧ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ウィンドウクローズ
		// --------------------------------------------------------------------
		protected override void Dispose(Boolean oIsDisposing)
		{
			try
			{
				if (!mIsDisposed)
				{
					// マネージドリソース解放
					if (oIsDisposing)
					{
						DisposeManagedResource(mReportDbInDisk);
					}

					// アンマネージドリソース解放
					// 今のところ無し

					// 基底呼び出し
					base.Dispose(oIsDisposing);
				}

				mIsDisposed = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "リスト問題報告一覧ウィンドウビューモデル破棄時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// リスト問題報告データベース
		private ReportDatabaseInDisk mReportDbInDisk;

		// Dispose フラグ
		private Boolean mIsDisposed = false;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// マネージドリソース解放
		// --------------------------------------------------------------------
		private void DisposeManagedResource(IDisposable oResource)
		{
			if (oResource != null)
			{
				oResource.Dispose();
			}
		}

		// --------------------------------------------------------------------
		// 詳細編集
		// --------------------------------------------------------------------
		private void EditDetail()
		{
			if (SelectedTReport == null)
			{
				return;
			}

			// リスト問題報告編集ウィンドウを開く
			using (EditReportWindowViewModel aEditReportWindowViewModel = new EditReportWindowViewModel())
			{
				aEditReportWindowViewModel.Environment = Environment;
				aEditReportWindowViewModel.TReport = SelectedTReport;
				Messenger.Raise(new TransitionMessage(aEditReportWindowViewModel, "OpenEditReportWindow"));
			}
		}

		// --------------------------------------------------------------------
		// TReports を現在のオプションに合わせて上書き
		// --------------------------------------------------------------------
		private void UpdateTReports()
		{
			using (DataContext aContext = new DataContext(mReportDbInDisk.Connection))
			{
				Table<TReport> aTableReport = aContext.GetTable<TReport>();
				IQueryable<TReport> aQueryResult =
						from x in aTableReport
						where ShowAll ? true : x.Status <= (Int32)ReportStatus.Progress
						orderby x.RegistTime descending
						select x;
				TReports = aQueryResult.ToList();
			}
		}

	}
	// public class ViewReportsWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
