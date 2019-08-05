// ============================================================================
// 
// リスト問題報告編集ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;

using Shinta;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class EditReportWindowViewModel : ViewModel
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

		// 報告内容
		private TReport mTReport;
		public TReport TReport
		{
			get => mTReport;
			set
			{
				if (RaisePropertyChangedIfSet(ref mTReport, value))
				{
					RaisePropertyChanged(nameof(Folder));
					RaisePropertyChanged(nameof(RegistTimeString));
				}
			}
		}

		// フォルダー
		public String Folder
		{
			get => TReport != null ? Path.GetDirectoryName(TReport.Path) : null;
		}

		// 報告日時文字列
		public String RegistTimeString
		{
			get
			{
				if (TReport == null)
				{
					return null;
				}

				DateTime aUtc = JulianDay.ModifiedJulianDateToDateTime(TReport.RegistTime);
				return TimeZoneInfo.ConvertTimeFromUtc(aUtc, TimeZoneInfo.Local).ToString(YlConstants.DATE_FORMAT + "（ddd） " + YlConstants.TIME_FORMAT);
			}
		}

		// 対応コメント
		private String mStatusComment;
		public String StatusComment
		{
			get => mStatusComment;
			set => RaisePropertyChangedIfSet(ref mStatusComment, value);
		}

		// 対応状況群
		private List<String> mStatusStrings;
		public List<String> StatusStrings
		{
			get => mStatusStrings;
			set => RaisePropertyChangedIfSet(ref mStatusStrings, value);
		}

		// 選択された対応状況
		private String mSelectedStatusString;
		public String SelectedStatusString
		{
			get => mSelectedStatusString;
			set => RaisePropertyChangedIfSet(ref mSelectedStatusString, value);
		}

		// OK ボタンフォーカス
		private Boolean mIsButtonOkFocused;
		public Boolean IsButtonOkFocused
		{
			get => mIsButtonOkFocused;
			set
			{
				// 再度フォーカスを当てられるように強制伝播
				mIsButtonOkFocused = value;
				RaisePropertyChanged(nameof(IsButtonOkFocused));
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

		#region 名称の編集ボタンの制御
		private ViewModelCommand mButtonEditMusicInfoClickedCommand;

		public ViewModelCommand ButtonEditMusicInfoClickedCommand
		{
			get
			{
				if (mButtonEditMusicInfoClickedCommand == null)
				{
					mButtonEditMusicInfoClickedCommand = new ViewModelCommand(ButtonEditMusicInfoClicked);
				}
				return mButtonEditMusicInfoClickedCommand;
			}
		}

		public void ButtonEditMusicInfoClicked()
		{
			try
			{
				String aPath = TReport.Path;
				if (!File.Exists(aPath))
				{
					throw new Exception("報告対象のファイルが存在しません。\n" + aPath);
				}

				// ファイル命名規則とフォルダー固定値を適用
				Dictionary<String, String> aDic = YlCommon.DicByFile(aPath);

				// 楽曲名が取得できていない場合は編集不可
				if (String.IsNullOrEmpty(aDic[YlConstants.RULE_VAR_TITLE]))
				{
					throw new Exception("ファイル名から楽曲名を取得できていないため、編集できません。\nファイル命名規則を確認して下さい。");
				}

				// 楽曲情報等編集ウィンドウを開く
				using (EditMusicInfoWindowViewModel aEditMusicInfoWindowViewModel = new EditMusicInfoWindowViewModel())
				{
					aEditMusicInfoWindowViewModel.Environment = Environment;
					aEditMusicInfoWindowViewModel.PathExLen = aPath;
					aEditMusicInfoWindowViewModel.DicByFile = aDic;
					Messenger.Raise(new TransitionMessage(aEditMusicInfoWindowViewModel, "OpenEditMusicInfoWindow"));
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region フォルダー設定ボタンの制御
		private ViewModelCommand mButtonFolderSettingsClickedCommand;

		public ViewModelCommand ButtonFolderSettingsClickedCommand
		{
			get
			{
				if (mButtonFolderSettingsClickedCommand == null)
				{
					mButtonFolderSettingsClickedCommand = new ViewModelCommand(ButtonFolderSettingsClicked);
				}
				return mButtonFolderSettingsClickedCommand;
			}
		}

		public void ButtonFolderSettingsClicked()
		{
			try
			{
				String aPath = TReport.Path;
				if (!File.Exists(aPath))
				{
					throw new Exception("報告対象のファイルが存在しません。\n" + aPath);
				}

				// 設定ファイルがあるフォルダー（設定ファイルが無い場合はファイルのフォルダー）
				String aFolder = Path.GetDirectoryName(aPath);
				String aSettingsFolder = YlCommon.FindSettingsFolder2Ex(aFolder);
				if (String.IsNullOrEmpty(aSettingsFolder))
				{
					aSettingsFolder = aFolder;
				}

				// フォルダー設定ウィンドウを開く
				using (FolderSettingsWindowViewModel aFolderSettingsWindowViewModel = new FolderSettingsWindowViewModel())
				{
					aFolderSettingsWindowViewModel.PathExLen = aSettingsFolder;
					aFolderSettingsWindowViewModel.Environment = Environment;
					Messenger.Raise(new TransitionMessage(aFolderSettingsWindowViewModel, "OpenFolderSettingsWindow"));
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region OK ボタンの制御
		private ViewModelCommand mButtonOkClickedCommand;

		public ViewModelCommand ButtonOkClickedCommand
		{
			get
			{
				if (mButtonOkClickedCommand == null)
				{
					mButtonOkClickedCommand = new ViewModelCommand(ButtonOKClicked);
				}
				return mButtonOkClickedCommand;
			}
		}

		public void ButtonOKClicked()
		{
			try
			{
				// Enter キーでボタンが押された場合はテキストボックスからフォーカスが移らずプロパティーが更新されないため強制フォーカス
				IsButtonOkFocused = true;

				CheckAndSave();
				Messenger.Raise(new WindowActionMessage("Close"));
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
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
				Title = "報告されたリスト問題の管理";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				// 対応状況選択肢
				List<String> aList = new List<String>();
				for (Int32 i = 0; i < (Int32)ReportStatus.__End__; i++)
				{
					aList.Add(YlConstants.REPORT_STATUS_NAMES[i]);
				}
				StatusStrings = aList;

				// 最新情報読込
				using (ReportDatabaseInDisk aReportDbInDisk = new ReportDatabaseInDisk(Environment))
				{
					TReport = YlCommon.SelectBaseById<TReport>(aReportDbInDisk.Connection, TReport.Id);
				}

				// 値反映
				TReportToProperties();

			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "リスト問題報告管理ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 保存
		// --------------------------------------------------------------------
		private void CheckAndSave()
		{
			using (ReportDatabaseInDisk aReportDbInDisk = new ReportDatabaseInDisk(Environment))
			using (DataContext aContext = new DataContext(aReportDbInDisk.Connection))
			{
				TReport aRecord = YlCommon.SelectBaseById<TReport>(aContext, TReport.Id);
				if (aRecord == null)
				{
					throw new Exception("対象の報告が見つかりません：" + TReport.Id);
				}

				aRecord.StatusComment = StatusComment;
				aRecord.Status = Array.IndexOf(YlConstants.REPORT_STATUS_NAMES, SelectedStatusString);
				if (aRecord.Status < 0)
				{
					throw new Exception("対応状況を選択してください。");
				}

				// 保存
				aContext.SubmitChanges();
			}
		}

		// --------------------------------------------------------------------
		// TReport の内容をプロパティーに反映
		// --------------------------------------------------------------------
		private void TReportToProperties()
		{
			StatusComment = TReport.StatusComment;
			SelectedStatusString = TReport.StatusName;
		}

	}
	// public class EditReportWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
