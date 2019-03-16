// ============================================================================
// 
// 出力設定を行うフォーム
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YukaLister.Shared;

namespace YukaLister
{
	/// <summary>
	/// OutputSettingsWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class OutputSettingsWindow : Window
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public OutputSettingsWindow(OutputWriter oOutputWriter, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 初期化
			mOutputWriter = oOutputWriter;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// リスト出力
		private OutputWriter mOutputWriter;

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Title = "出力設定：" + mOutputWriter.FormatName;
#if DEBUG
			Title = "［デバッグ］" + Title;
#endif

			Common.CascadeWindow(this);
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "出力設定ウィンドウを開きます。");
				Init();

				// タブ追加
				List<TabItem> aTabItems = mOutputWriter.DialogTabItems();
				foreach (TabItem aTabItem in aTabItems)
				{
					TabControlOutputSettings.Items.Add(aTabItem);
				}
				TabControlOutputSettings.SelectedIndex = 0;

				mOutputWriter.SettingsToCompos();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "出力設定フォームロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				mOutputWriter.CheckInput();
				mOutputWriter.ComposToSettings();
				mOutputWriter.OutputSettings.Save();
				DialogResult = true;
			}
			catch (OperationCanceledException)
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "設定変更を中止しました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "出力設定フォームを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "出力設定フォームクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TabControlOutputSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				TabControlOutputSettings.MinWidth = TabControlOutputSettings.ActualWidth;
				TabControlOutputSettings.MinHeight = TabControlOutputSettings.ActualHeight;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タブコントロール選択時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}
