// ============================================================================
// 
// フォルダー設定ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.Windows;

using Shinta;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class FolderSettingsWindowViewModel : ViewModel
	{
		/* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

		/* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

		/* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

		/* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

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

		// 設定対象フォルダーのパス（ExLen 形式）
		private String mPathExLen;
		public String PathExLen
		{
			get => mPathExLen;
			set
			{
				if (RaisePropertyChangedIfSet(ref mPathExLen, value))
				{
					SettingsToProperties();
				}
			}
		}

		// 設定対象フォルダーのパス（ShLen 形式）
		public String PathShLen
		{
			get => Environment?.ShortenPath(PathExLen);
		}

		// 設定ファイルの状態
		private FolderSettingsStatus mSettingsFileStatus;
		public FolderSettingsStatus SettingsFileStatus
		{
			get => mSettingsFileStatus;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSettingsFileStatus, value))
				{
					ButtonDeleteSettingsClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// （入力中の）ファイル命名規則
		private String mFileNameRule;
		public String FileNameRule
		{
			get => mFileNameRule;
			set
			{
				if (RaisePropertyChangedIfSet(ref mFileNameRule, value))
				{
					ButtonAddFileNameRuleClickedCommand.RaiseCanExecuteChanged();
					ButtonReplaceFileNameRuleClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// （入力中の）ファイル命名規則選択開始位置
		private Int32 mFileNameRuleSelectionStart;
		public Int32 FileNameRuleSelectionStart
		{
			get => mFileNameRuleSelectionStart;
			set => RaisePropertyChangedIfSet(ref mFileNameRuleSelectionStart, value);
		}

		// （入力中の）ファイル命名規則選択長さ
		private Int32 mFileNameRuleSelectionLength;
		public Int32 FileNameRuleSelectionLength
		{
			get => mFileNameRuleSelectionLength;
			set => RaisePropertyChangedIfSet(ref mFileNameRuleSelectionLength, value);
		}

		// （入力中の）ファイル命名規則へのフォーカス
		private Boolean mIsFileNameRuleFocused;
		public Boolean IsFileNameRuleFocused
		{
			get => mIsFileNameRuleFocused;
			set
			{
				// 再度フォーカスを当てられるように強制伝播
				mIsFileNameRuleFocused = value;
				RaisePropertyChanged(nameof(IsFileNameRuleFocused));
			}
		}

		// タグボタンのコンテキストメニュー
		public List<MenuItem> ContextMenuButtonVarItems { get; set; }

		// ファイル命名規則
		public ObservableCollection<String> FileNameRules { get; set; } = new ObservableCollection<String>();

		// 選択されているファイル命名規則
		private String mSelectedFileNameRule;
		public String SelectedFileNameRule
		{
			get => mSelectedFileNameRule;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedFileNameRule, value))
				{
					// 選択された時に入力欄に値を入れる
					FileNameRule = SelectedFileNameRule;

					ButtonReplaceFileNameRuleClickedCommand.RaiseCanExecuteChanged();
					ButtonDeleteFileNameRuleClickedCommand.RaiseCanExecuteChanged();
					ButtonUpFileNameRuleClickedCommand.RaiseCanExecuteChanged();
					ButtonDownFileNameRuleClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// フォルダー固定値項目のルール名
		public List<String> FolderNameRuleNames { get; set; }

		// フォルダー固定値項目のルール名選択
		private String mSelectedFolderNameRuleName;
		public String SelectedFolderNameRuleName
		{
			get => mSelectedFolderNameRuleName;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedFolderNameRuleName, value))
				{
					UpdateFolderNameRuleProperties();
					ButtonAddFolderNameRuleClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// フォルダー固定値のルール値（選択式）の表示状況
		private Visibility mSelectedFolderNameRuleValueVisibility;
		public Visibility SelectedFolderNameRuleValueVisibility
		{
			get => mSelectedFolderNameRuleValueVisibility;
			set => RaisePropertyChangedIfSet(ref mSelectedFolderNameRuleValueVisibility, value);
		}

		// フォルダー固定値のルール値（選択式）
		private List<String> mFolderNameRuleValues;
		public List<String> FolderNameRuleValues
		{
			get => mFolderNameRuleValues;
			set => RaisePropertyChangedIfSet(ref mFolderNameRuleValues, value);
		}

		// 選択されているフォルダー固定値項目のルール値
		private String mSelectedFolderNameRuleValue;
		public String SelectedFolderNameRuleValue
		{
			get => mSelectedFolderNameRuleValue;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedFolderNameRuleValue, value))
				{
					ButtonAddFolderNameRuleClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// フォルダー固定値のルール値（手入力）の表示状況
		private Visibility mInputFolderNameRuleValueVisibility;
		public Visibility InputFolderNameRuleValueVisibility
		{
			get => mInputFolderNameRuleValueVisibility;
			set => RaisePropertyChangedIfSet(ref mInputFolderNameRuleValueVisibility, value);
		}

		// 手入力されているフォルダー固定値項目のルール値
		private String mInputFolderNameRuleValue;
		public String InputFolderNameRuleValue
		{
			get => mInputFolderNameRuleValue;
			set
			{
				if (RaisePropertyChangedIfSet(ref mInputFolderNameRuleValue, value))
				{
					ButtonAddFolderNameRuleClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// フォルダー固定値（名前＋値）
		public ObservableCollection<String> FolderNameRules { get; set; } = new ObservableCollection<String>();

		// 選択されているフォルダー固定値
		private String mSelectedFolderNameRule;
		public String SelectedFolderNameRule
		{
			get => mSelectedFolderNameRule;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedFolderNameRule, value))
				{
					// 選択された時に入力欄に値を入れる
					SelectedFolderNameRuleToNameAndValue();

					ButtonDeleteFolderNameRuleClickedCommand.RaiseCanExecuteChanged();
					ButtonUpFolderNameRuleClickedCommand.RaiseCanExecuteChanged();
					ButtonDownFolderNameRuleClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// プログレスバーの表示状況
		private Visibility mProgressBarPreviewVisibility = Visibility.Hidden;
		public Visibility ProgressBarPreviewVisibility
		{
			get => mProgressBarPreviewVisibility;
			set
			{
				if (RaisePropertyChangedIfSet(ref mProgressBarPreviewVisibility, value))
				{
					ButtonPreviewClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// プレビュー結果
		public DispatcherCollection<PreviewInfo> PreviewInfos { get; set; } = new DispatcherCollection<PreviewInfo>(DispatcherHelper.UIDispatcher);

		// 選択中のプレビュー結果
		private PreviewInfo mSelectedPreviewInfo;
		public PreviewInfo SelectedPreviewInfo
		{
			get => mSelectedPreviewInfo;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedPreviewInfo, value))
				{
					ButtonEditInfoClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// 除外設定
		private Boolean mIsExcluded;
		public Boolean IsExcluded
		{
			get => mIsExcluded;
			set
			{
				if (RaisePropertyChangedIfSet(ref mIsExcluded, value))
				{
					// mIsExcluded が除外ファイルの状態と異なる場合は変更フラグをセット
					FolderExcludeSettingsStatus aFolderExcludeSettingsStatus = YlCommon.DetectFolderExcludeSettingsStatus(PathExLen);
					mIsDirty |= (aFolderExcludeSettingsStatus != FolderExcludeSettingsStatus.False) != mIsExcluded;

					ButtonPreviewClickedCommand.RaiseCanExecuteChanged();
					ButtonJumpClickedCommand.RaiseCanExecuteChanged();
					ButtonEditInfoClickedCommand.RaiseCanExecuteChanged();
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

		#region ファイル命名規則追加ボタンの制御
		private ViewModelCommand mButtonAddFileNameRuleClickedCommand;

		public ViewModelCommand ButtonAddFileNameRuleClickedCommand
		{
			get
			{
				if (mButtonAddFileNameRuleClickedCommand == null)
				{
					mButtonAddFileNameRuleClickedCommand = new ViewModelCommand(ButtonAddFileNameRuleClicked, CanButtonAddFileNameRuleClicked);
				}
				return mButtonAddFileNameRuleClickedCommand;
			}
		}

		public Boolean CanButtonAddFileNameRuleClicked()
		{
			return !String.IsNullOrEmpty(FileNameRule);
		}

		public void ButtonAddFileNameRuleClicked()
		{
			try
			{
				AddFileNameRule();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則追加時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ファイル命名規則置換ボタンの制御
		private ViewModelCommand mButtonReplaceFileNameRuleClickedCommand;

		public ViewModelCommand ButtonReplaceFileNameRuleClickedCommand
		{
			get
			{
				if (mButtonReplaceFileNameRuleClickedCommand == null)
				{
					mButtonReplaceFileNameRuleClickedCommand = new ViewModelCommand(ButtonReplaceFileNameRuleClicked, CanButtonReplaceFileNameRuleClicked);
				}
				return mButtonReplaceFileNameRuleClickedCommand;
			}
		}

		public Boolean CanButtonReplaceFileNameRuleClicked()
		{
			return !String.IsNullOrEmpty(FileNameRule) && !String.IsNullOrEmpty(SelectedFileNameRule);
		}

		public void ButtonReplaceFileNameRuleClicked()
		{
			try
			{
				CheckFileNameRule(false);

				// 置換
				FileNameRules[FileNameRules.IndexOf(SelectedFileNameRule)] = FileNameRule;
				SelectedFileNameRule = FileNameRule;
				FileNameRule = null;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則置換時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ファイル命名規則削除ボタンの制御
		private ViewModelCommand mButtonDeleteFileNameRuleClickedCommand;

		public ViewModelCommand ButtonDeleteFileNameRuleClickedCommand
		{
			get
			{
				if (mButtonDeleteFileNameRuleClickedCommand == null)
				{
					mButtonDeleteFileNameRuleClickedCommand = new ViewModelCommand(ButtonDeleteFileNameRuleClicked, CanButtonDeleteFileNameRuleClicked);
				}
				return mButtonDeleteFileNameRuleClickedCommand;
			}
		}

		public Boolean CanButtonDeleteFileNameRuleClicked()
		{
			return !String.IsNullOrEmpty(SelectedFileNameRule);
		}

		public void ButtonDeleteFileNameRuleClicked()
		{
			try
			{
				FileNameRules.Remove(SelectedFileNameRule);
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則削除時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ファイル命名規則上へボタンの制御
		private ViewModelCommand mButtonUpFileNameRuleClickedCommand;

		public ViewModelCommand ButtonUpFileNameRuleClickedCommand
		{
			get
			{
				if (mButtonUpFileNameRuleClickedCommand == null)
				{
					mButtonUpFileNameRuleClickedCommand = new ViewModelCommand(ButtonUpFileNameRuleClicked, CanButtonUpFileNameRuleClicked);
				}
				return mButtonUpFileNameRuleClickedCommand;
			}
		}

		public Boolean CanButtonUpFileNameRuleClicked()
		{
			return !String.IsNullOrEmpty(SelectedFileNameRule) && FileNameRules.IndexOf(SelectedFileNameRule) > 0;
		}

		public void ButtonUpFileNameRuleClicked()
		{
			try
			{
				String aSelectedFileNameRuleBak = SelectedFileNameRule;
				Int32 aIndex = FileNameRules.IndexOf(aSelectedFileNameRuleBak);
				SwapListItem(FileNameRules, aIndex - 1, aIndex);
				SelectedFileNameRule = aSelectedFileNameRuleBak;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則順番繰り上げ時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ファイル命名規則下へボタンの制御
		private ViewModelCommand mButtonDownFileNameRuleClickedCommand;

		public ViewModelCommand ButtonDownFileNameRuleClickedCommand
		{
			get
			{
				if (mButtonDownFileNameRuleClickedCommand == null)
				{
					mButtonDownFileNameRuleClickedCommand = new ViewModelCommand(ButtonDownFileNameRuleClicked, CanButtonDownFileNameRuleClicked);
				}
				return mButtonDownFileNameRuleClickedCommand;
			}
		}

		public Boolean CanButtonDownFileNameRuleClicked()
		{
			return !String.IsNullOrEmpty(SelectedFileNameRule) && FileNameRules.IndexOf(SelectedFileNameRule) < FileNameRules.Count - 1;
		}

		public void ButtonDownFileNameRuleClicked()
		{
			try
			{
				String aSelectedFileNameRuleBak = SelectedFileNameRule;
				Int32 aIndex = FileNameRules.IndexOf(aSelectedFileNameRuleBak);
				SwapListItem(FileNameRules, aIndex + 1, aIndex);
				SelectedFileNameRule = aSelectedFileNameRuleBak;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名命名規則順番繰り下げ時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region フォルダー固定値追加ボタンの制御
		private ViewModelCommand mButtonAddFolderNameRuleClickedCommand;

		public ViewModelCommand ButtonAddFolderNameRuleClickedCommand
		{
			get
			{
				if (mButtonAddFolderNameRuleClickedCommand == null)
				{
					mButtonAddFolderNameRuleClickedCommand = new ViewModelCommand(ButtonAddFolderNameRuleClicked, CanButtonAddFolderNameRuleClicked);
				}
				return mButtonAddFolderNameRuleClickedCommand;
			}
		}

		public Boolean CanButtonAddFolderNameRuleClicked()
		{
			if (SelectedFolderNameRuleValueVisibility == Visibility.Visible)
			{
				return !String.IsNullOrEmpty(SelectedFolderNameRuleValue);
			}
			else
			{
				return !String.IsNullOrEmpty(InputFolderNameRuleValue);
			}
		}

		public void ButtonAddFolderNameRuleClicked()
		{
			try
			{
				AddFolderNameRule();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目追加時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region フォルダー固定値削除ボタンの制御
		private ViewModelCommand mButtonDeleteFolderNameRuleClickedCommand;

		public ViewModelCommand ButtonDeleteFolderNameRuleClickedCommand
		{
			get
			{
				if (mButtonDeleteFolderNameRuleClickedCommand == null)
				{
					mButtonDeleteFolderNameRuleClickedCommand = new ViewModelCommand(ButtonDeleteFolderNameRuleClicked, CanButtonDeleteFolderNameRuleClicked);
				}
				return mButtonDeleteFolderNameRuleClickedCommand;
			}
		}

		public bool CanButtonDeleteFolderNameRuleClicked()
		{
			return !String.IsNullOrEmpty(SelectedFolderNameRule);
		}

		public void ButtonDeleteFolderNameRuleClicked()
		{
			try
			{
				FolderNameRules.Remove(SelectedFolderNameRule);
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "固定値項目削除時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region フォルダー固定値上へボタンの制御
		private ViewModelCommand mButtonUpFolderNameRuleClickedCommand;

		public ViewModelCommand ButtonUpFolderNameRuleClickedCommand
		{
			get
			{
				if (mButtonUpFolderNameRuleClickedCommand == null)
				{
					mButtonUpFolderNameRuleClickedCommand = new ViewModelCommand(ButtonUpFolderNameRuleClicked, CanButtonUpFolderNameRuleClicked);
				}
				return mButtonUpFolderNameRuleClickedCommand;
			}
		}

		public bool CanButtonUpFolderNameRuleClicked()
		{
			return !String.IsNullOrEmpty(SelectedFolderNameRule) && FolderNameRules.IndexOf(SelectedFolderNameRule) > 0;
		}

		public void ButtonUpFolderNameRuleClicked()
		{
			try
			{
				String aSelectedFolderNameRuleBak = SelectedFolderNameRule;
				Int32 aIndex = FolderNameRules.IndexOf(aSelectedFolderNameRuleBak);
				SwapListItem(FolderNameRules, aIndex - 1, aIndex);
				SelectedFolderNameRule = aSelectedFolderNameRuleBak;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー固定値順番繰り上げ時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region フォルダー固定値下へボタンの制御
		private ViewModelCommand mButtonDownFolderNameRuleClickedCommand;

		public ViewModelCommand ButtonDownFolderNameRuleClickedCommand
		{
			get
			{
				if (mButtonDownFolderNameRuleClickedCommand == null)
				{
					mButtonDownFolderNameRuleClickedCommand = new ViewModelCommand(ButtonDownFolderNameRuleClicked, CanButtonDownFolderNameRuleClicked);
				}
				return mButtonDownFolderNameRuleClickedCommand;
			}
		}

		public bool CanButtonDownFolderNameRuleClicked()
		{
			return !String.IsNullOrEmpty(SelectedFolderNameRule) && FolderNameRules.IndexOf(SelectedFolderNameRule) < FolderNameRules.Count - 1;
		}

		public void ButtonDownFolderNameRuleClicked()
		{
			try
			{
				String aSelectedFolderNameRuleBak = SelectedFolderNameRule;
				Int32 aIndex = FolderNameRules.IndexOf(aSelectedFolderNameRuleBak);
				SwapListItem(FolderNameRules, aIndex + 1, aIndex);
				SelectedFolderNameRule = aSelectedFolderNameRuleBak;
				mIsDirty = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー固定値順番繰り下げ時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 解析結果確認ボタンの制御
		private ViewModelCommand mButtonPreviewClickedCommand;

		public ViewModelCommand ButtonPreviewClickedCommand
		{
			get
			{
				if (mButtonPreviewClickedCommand == null)
				{
					mButtonPreviewClickedCommand = new ViewModelCommand(ButtonPreviewClicked, CanButtonPreviewClicked);
				}
				return mButtonPreviewClickedCommand;
			}
		}

		public bool CanButtonPreviewClicked()
		{
			return !IsExcluded && ProgressBarPreviewVisibility == Visibility.Hidden;
		}

		public void ButtonPreviewClicked()
		{
			try
			{
				// 保存
				SaveSettingsIfNeeded();

				// 検索（async を待機しない）
				Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(UpdatePreviewResult, mTaskLock, null, Environment.LogWriter);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル検索時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 未登録検出ボタンの制御
		private ViewModelCommand mButtonJumpClickedCommand;

		public ViewModelCommand ButtonJumpClickedCommand
		{
			get
			{
				if (mButtonJumpClickedCommand == null)
				{
					mButtonJumpClickedCommand = new ViewModelCommand(ButtonJumpClicked, CanButtonJumpClicked);
				}
				return mButtonJumpClickedCommand;
			}
		}

		public bool CanButtonJumpClicked()
		{
			return !IsExcluded && PreviewInfos.Count > 0;
		}

		public void ButtonJumpClicked()
		{
			try
			{
				// async を待機しない
				Task aSuppressWarning = YlCommon.LaunchTaskAsync<Object>(JumpToNextCandidate, mTaskLock, null, Environment.LogWriter);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "未登録検出クリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 名称の編集ボタンの制御
		private ViewModelCommand mButtonEditInfoClickedCommand;

		public ViewModelCommand ButtonEditInfoClickedCommand
		{
			get
			{
				if (mButtonEditInfoClickedCommand == null)
				{
					mButtonEditInfoClickedCommand = new ViewModelCommand(ButtonEditInfoClicked, CanButtonEditInfoClicked);
				}
				return mButtonEditInfoClickedCommand;
			}
		}

		public bool CanButtonEditInfoClicked()
		{
			return !IsExcluded && SelectedPreviewInfo != null;
		}

		public void ButtonEditInfoClicked()
		{
			try
			{
				if (!IsExcluded && SelectedPreviewInfo != null)
				{
					using (EditMusicInfoWindowViewModel aEditMusicInfoWindowViewModel = new EditMusicInfoWindowViewModel())
					{
						String aPath = PathExLen + "\\" + SelectedPreviewInfo.FileName;
						aEditMusicInfoWindowViewModel.Environment = Environment;
						aEditMusicInfoWindowViewModel.PathExLen = aPath;
						aEditMusicInfoWindowViewModel.DicByFile = YlCommon.DicByFile(aPath);
						Messenger.Raise(new TransitionMessage(aEditMusicInfoWindowViewModel, "OpenEditMusicInfoWindow"));
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "名称の編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ソートの制御
		private ListenerCommand<DataGridSortingEventArgs> mDataGridPreviewSortingCommand;

		public ListenerCommand<DataGridSortingEventArgs> DataGridPreviewSortingCommand
		{
			get
			{
				if (mDataGridPreviewSortingCommand == null)
				{
					mDataGridPreviewSortingCommand = new ListenerCommand<DataGridSortingEventArgs>(DataGridPreviewSorting);
				}
				return mDataGridPreviewSortingCommand;
			}
		}

		public void DataGridPreviewSorting(DataGridSortingEventArgs oDataGridSortingEventArgs)
		{
			try
			{
				PreviewInfo aPrevSelectedPreviewInfo = SelectedPreviewInfo;

				// 並び替えの方向（昇順か降順か）を決める
				ListSortDirection aNewDirection;
				if (oDataGridSortingEventArgs.Column.SortDirection == ListSortDirection.Ascending)
				{
					aNewDirection = ListSortDirection.Descending;
				}
				else
				{
					aNewDirection = ListSortDirection.Ascending;
				}

				// データのソート
				List<PreviewInfo> aNewPreviewInfos = new List<PreviewInfo>();
				if (aNewDirection == ListSortDirection.Ascending)
				{
					switch (oDataGridSortingEventArgs.Column.DisplayIndex)
					{
						case 0:
							// ファイル名でのソート
							aNewPreviewInfos = PreviewInfos.OrderBy(x => x.FileName).ToList();
							break;
						case 1:
							// 項目と値でのソート
							aNewPreviewInfos = PreviewInfos.OrderBy(x => x.Items).ToList();
							break;
						case 2:
							// 更新日でのソート
							aNewPreviewInfos = PreviewInfos.OrderBy(x => x.LastWriteTime).ToList();
							break;
						default:
							Debug.Assert(false, "DataGridPreviewSorting() bad specified target item: " + oDataGridSortingEventArgs.Column.DisplayIndex.ToString());
							break;
					}
				}
				else
				{
					switch (oDataGridSortingEventArgs.Column.DisplayIndex)
					{
						case 0:
							// ファイル名でのソート
							aNewPreviewInfos = PreviewInfos.OrderByDescending(x => x.FileName).ToList();
							break;
						case 1:
							// 項目と値でのソート
							aNewPreviewInfos = PreviewInfos.OrderByDescending(x => x.Items).ToList();
							break;
						case 2:
							// 更新日でのソート
							aNewPreviewInfos = PreviewInfos.OrderByDescending(x => x.LastWriteTime).ToList();
							break;
						default:
							Debug.Assert(false, "DataGridPreviewSorting() bad specified target item: " + oDataGridSortingEventArgs.Column.DisplayIndex.ToString());
							break;
					}
				}

				// 結果の表示
				PreviewInfos.Clear();
				foreach (PreviewInfo aNewPreviewInfo in aNewPreviewInfos)
				{
					PreviewInfos.Add(aNewPreviewInfo);
				}
				SelectedPreviewInfo = aPrevSelectedPreviewInfo;

				// 並び替えグリフの表示
				oDataGridSortingEventArgs.Column.SortDirection = aNewDirection;

				oDataGridSortingEventArgs.Handled = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "DGV ヘッダークリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 設定削除ボタンの制御
		private ViewModelCommand mButtonDeleteSettingsClickedCommand;

		public ViewModelCommand ButtonDeleteSettingsClickedCommand
		{
			get
			{
				if (mButtonDeleteSettingsClickedCommand == null)
				{
					mButtonDeleteSettingsClickedCommand = new ViewModelCommand(ButtonDeleteSettingsClicked, CanButtonDeleteSettingsClicked);
				}
				return mButtonDeleteSettingsClickedCommand;
			}
		}

		public bool CanButtonDeleteSettingsClicked()
		{
			return SettingsFileStatus == FolderSettingsStatus.Set;
		}

		public void ButtonDeleteSettingsClicked()
		{
			try
			{
				if (MessageBox.Show("フォルダー設定を削除します。\nよろしいですか？", "確認",
						MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
				{
					return;
				}

				if (File.Exists(PathExLen + "\\" + YlConstants.FILE_NAME_YUKA_LISTER_CONFIG))
				{
					File.Delete(PathExLen + "\\" + YlConstants.FILE_NAME_YUKA_LISTER_CONFIG);
				}
				if (File.Exists(PathExLen + "\\" + YlConstants.FILE_NAME_NICO_KARA_LISTER_CONFIG))
				{
					File.Delete(PathExLen + "\\" + YlConstants.FILE_NAME_NICO_KARA_LISTER_CONFIG);
				}
				if (File.Exists(PathExLen + "\\" + YlConstants.FILE_NAME_YUKA_LISTER_EXCLUDE_CONFIG))
				{
					File.Delete(PathExLen + "\\" + YlConstants.FILE_NAME_YUKA_LISTER_EXCLUDE_CONFIG);
				}

				// UI に反映
				SettingsToProperties();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "設定削除ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region OK ボタンの制御
		private ViewModelCommand mButtonOKClickedCommand;

		public ViewModelCommand ButtonOKClickedCommand
		{
			get
			{
				if (mButtonOKClickedCommand == null)
				{
					mButtonOKClickedCommand = new ViewModelCommand(ButtonOKClicked);
				}
				return mButtonOKClickedCommand;
			}
		}

		public void ButtonOKClicked()
		{
			try
			{
				SaveSettingsIfNeeded();
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
		// 本関数を呼ぶ前に Environment を設定しておく必要がある
		// --------------------------------------------------------------------
		public void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				// タイトルバー
				Title = "フォルダー設定";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				// タグボタンのコンテキストメニュー
				ContextMenuButtonVarItems = new List<MenuItem>();
				List<String> aLabels = CreateRuleVarLabels();
				foreach (String aLabel in aLabels)
				{
					// オンボーカル・オフボーカルは除外
					if (aLabel.IndexOf(YlConstants.RULE_VAR_ON_VOCAL, StringComparison.OrdinalIgnoreCase) < 0
							&& aLabel.IndexOf(YlConstants.RULE_VAR_OFF_VOCAL, StringComparison.OrdinalIgnoreCase) < 0)
					{
						AddContextMenuItemToButtonVar(aLabel);
					}
				}

				// カテゴリー一覧
				using (MusicInfoDatabaseInDisk aMusicInfoDatabaseInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					mCachedCategoryNames = YlCommon.SelectCategoryNames(aMusicInfoDatabaseInDisk.Connection);
				}

				// 固定値項目（カテゴリー一覧設定後に行う）
				FolderNameRuleNames = new List<String>();
				foreach (String aLabel in aLabels)
				{
					// * は除外
					if (aLabel.IndexOf(YlConstants.RULE_VAR_ANY) < 0)
					{
						FolderNameRuleNames.Add(aLabel);
					}
				}
				SelectedFolderNameRuleName = FolderNameRuleNames[0];

				// リスナーに通知
				RaisePropertyChanged(nameof(PathExLen));
				RaisePropertyChanged(nameof(ContextMenuButtonVarItems));
				RaisePropertyChanged(nameof(FolderNameRuleNames));
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "フォルダー設定ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// カテゴリー一覧の事前読み込み
		private List<String> mCachedCategoryNames;

		// 設定が変更された
		private Boolean mIsDirty = false;

		// フォルダー設定フォーム上で時間のかかるタスクが多重起動されるのを抑止する
		private Object mTaskLock = new Object();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ButtonVar のコンテキストメニューにアイテムを追加
		// --------------------------------------------------------------------
		private void AddContextMenuItemToButtonVar(String oLabel)
		{
			YlCommon.AddContextMenuItem(ContextMenuButtonVarItems, oLabel, ContextMenuButtonVarItem_Click);
		}

		// --------------------------------------------------------------------
		// テキストボックスに入力されているファイル命名規則をリストボックスに追加
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void AddFileNameRule()
		{
			CheckFileNameRule(true);

			// 追加
			FileNameRules.Add(FileNameRule);
			SelectedFileNameRule = FileNameRule;
			FileNameRule = null;
			mIsDirty = true;
		}

		// --------------------------------------------------------------------
		// 選択または入力されたルールを追加
		// --------------------------------------------------------------------
		private void AddFolderNameRule()
		{
			// 追加済みのフォルダー固定値と同じ項目があれば選択する
			SelectedFolderNameRule = SelectedFolderNameRuleFromSelectedFolderNameRuleName();
			String aNewRule = FolderNameRuleFromProperty();

			if (SelectedFolderNameRule == null)
			{
				// 未登録なので新規登録
				FolderNameRules.Add(aNewRule);
			}
			else
			{
				// 既に登録済みなので置換
				FolderNameRules[FolderNameRules.IndexOf(SelectedFolderNameRule)] = aNewRule;
			}
			SelectedFolderNameRule = aNewRule;

			SelectedFolderNameRuleValue = null;
			InputFolderNameRuleValue = null;
			mIsDirty = true;
		}

		// --------------------------------------------------------------------
		// テキストボックスに入力されているファイル命名規則が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckFileNameRule(Boolean oCheckSelectedLine)
		{
			// 入力が空の場合はボタンは押されないはずだが念のため
			if (String.IsNullOrEmpty(FileNameRule))
			{
				throw new Exception("命名規則が入力されていません。");
			}

			// 変数が含まれているか
			if (FileNameRule.IndexOf(YlConstants.RULE_VAR_BEGIN) < 0)
			{
				throw new Exception("命名規則に <変数> が含まれていません。");
			}

			// 既存のものと重複していないか
			if (IsFileNameRuleAdded())
			{
				throw new Exception("同じ命名規則が既に追加されています。");
			}

			// 変数・ワイルドカードが隣り合っているとうまく解析できない
			String aNormalizedNewRule = NormalizeRule(FileNameRule);
			if (aNormalizedNewRule.IndexOf(YlConstants.RULE_VAR_ANY + YlConstants.RULE_VAR_ANY) >= 0)
			{
				throw new Exception("<変数> や " + YlConstants.RULE_VAR_ANY + " が連続していると正常にファイル名を解析できません。");
			}

			// 競合する命名規則が無いか
			for (Int32 i = 0; i < FileNameRules.Count; i++)
			{
				if (SelectedFileNameRule == FileNameRules[i] && !oCheckSelectedLine)
				{
					continue;
				}

				if (NormalizeRule(FileNameRules[i]) == aNormalizedNewRule)
				{
					throw new Exception("競合する命名規則が既に追加されています：\n" + FileNameRules[i]);
				}
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ContextMenuButtonVarItem_Click(Object oSender, RoutedEventArgs oRoutedEventArgs)
		{
			try
			{
				MenuItem aItem = (MenuItem)oSender;
				String aKey = FindRuleVarName((String)aItem.Header);
				String aWrappedVarName = WrapVarName(aKey);

				// カーソル位置に挿入
				Debug.WriteLine("ContextMenuButtonVarItem_Click() FileNameRuleSelectionStart: " + FileNameRuleSelectionStart);
				Debug.WriteLine("ContextMenuButtonVarItem_Click() aWrappedVarName: " + aWrappedVarName);
				Int32 aSelectionStartBak = FileNameRuleSelectionStart;
				if (String.IsNullOrEmpty(FileNameRule))
				{
					FileNameRule = aWrappedVarName;
				}
				else
				{
					FileNameRule = FileNameRule.Substring(0, FileNameRuleSelectionStart) + aWrappedVarName
							+ FileNameRule.Substring(FileNameRuleSelectionStart + FileNameRuleSelectionLength);
				}

				// タグボタンにフォーカスが移っているので戻す
				IsFileNameRuleFocused = true;

				// カーソル位置変更
				FileNameRuleSelectionStart = aSelectionStartBak + aWrappedVarName.Length;
				FileNameRuleSelectionLength = 0;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "変数メニュークリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// ファイル命名規則の変数の表示用文字列を生成
		// --------------------------------------------------------------------
		private List<String> CreateRuleVarLabels()
		{
			List<String> aLabels = new List<String>();
			TextInfo aTextInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
			Dictionary<String, String> aVarMap = YlCommon.CreateRuleDictionaryWithDescription();
			foreach (KeyValuePair<String, String> aVar in aVarMap)
			{
				String aKey;
				if (aVar.Key == YlConstants.RULE_VAR_ANY)
				{
					aKey = aVar.Key;
				}
				else
				{
					aKey = YlConstants.RULE_VAR_BEGIN + aTextInfo.ToTitleCase(aVar.Key) + YlConstants.RULE_VAR_END;
				}
				aLabels.Add(aKey + "（" + aVar.Value + "）");
			}
			return aLabels;
		}

		// --------------------------------------------------------------------
		// 文字列の中に含まれている命名規則の変数名を返す
		// 文字列の中には <Name> 形式で変数名を含んでいる必要がある
		// 返す変数名には <> は含まない
		// --------------------------------------------------------------------
		private String FindRuleVarName(String oString)
		{
			Dictionary<String, String> aVarMap = YlCommon.CreateRuleDictionary();
			foreach (String aKey in aVarMap.Keys)
			{
				if (oString.IndexOf(YlConstants.RULE_VAR_BEGIN + aKey + YlConstants.RULE_VAR_END, StringComparison.CurrentCultureIgnoreCase) >= 0)
				{
					return aKey;
				}
			}
			if (oString.IndexOf(YlConstants.RULE_VAR_ANY) >= 0)
			{
				return YlConstants.RULE_VAR_ANY;
			}
			return null;
		}

		// --------------------------------------------------------------------
		// 選択または入力された固定値（名前＋値）
		// --------------------------------------------------------------------
		private String FolderNameRuleFromProperty()
		{
			String aKey = FindRuleVarName(SelectedFolderNameRuleName);
			return WrapVarName(aKey) + "=" + FolderNameRuleValueFromProperty();
		}

		// --------------------------------------------------------------------
		// 選択または入力された固定値（値）
		// --------------------------------------------------------------------
		private String FolderNameRuleValueFromProperty()
		{
			if (SelectedFolderNameRuleValueVisibility == Visibility.Visible)
			{
				return SelectedFolderNameRuleValue;
			}
			else
			{
				return InputFolderNameRuleValue;
			}
		}

		// --------------------------------------------------------------------
		// 入力中のファイル命名規則と同じものが既に追加されているか
		// --------------------------------------------------------------------
		private Boolean IsFileNameRuleAdded()
		{
			foreach (String aRule in FileNameRules)
			{
				if (FileNameRule == aRule)
				{
					return true;
				}
			}
			return false;
		}

		// --------------------------------------------------------------------
		// 編集する必要がありそうなファイルに飛ぶ
		// （楽曲名・タイアップ名が楽曲情報データベースに未登録なファイル）
		// --------------------------------------------------------------------
		private void JumpToNextCandidate(Object oDummy)
		{
			try
			{
				Int32 aRowIndex = PreviewInfos.IndexOf(SelectedPreviewInfo);

				// マッチ準備
				FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings2Ex(PathExLen);
				FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);

				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					for (; ; )
					{
						aRowIndex++;
						if (aRowIndex >= PreviewInfos.Count)
						{
							Environment.LogWriter.ShowLogMessage(TraceEventType.Information, "ファイル名から取得した楽曲情報・番組情報が楽曲情報データベースに未登録のファイルは見つかりませんでした。");
							SelectedPreviewInfo = null;
							return;
						}

						// ファイル命名規則とフォルダー固定値を適用
						Dictionary<String, String> aDic = YlCommon.MatchFileNameRulesAndFolderRule(
									Path.GetFileNameWithoutExtension(PreviewInfos[aRowIndex].FileName), aFolderSettingsInMemory);

						// 楽曲名が空かどうか
						if (String.IsNullOrEmpty(aDic[YlConstants.RULE_VAR_TITLE]))
						{
							break;
						}

						// 楽曲名が楽曲情報データベースと不一致かどうか
						String aSongNameOrigin = aDic[YlConstants.RULE_VAR_TITLE];
						List<TSongAlias> aSongAliases = YlCommon.SelectAliasesByAlias<TSongAlias>(aContext, aDic[YlConstants.RULE_VAR_TITLE]);
						if (aSongAliases.Count > 0)
						{
							TSong aSongOrigin = YlCommon.SelectMasterById<TSong>(aContext, aSongAliases[0].OriginalId);
							if (aSongOrigin != null)
							{
								aSongNameOrigin = aSongOrigin.Name;
							}
						}
						List<TSong> aSongs = YlCommon.SelectMastersByName<TSong>(aContext, aSongNameOrigin);
						if (aSongs.Count == 0)
						{
							break;
						}

						// 番組名がある場合、番組名が楽曲情報データベースと不一致かどうか
						if (!String.IsNullOrEmpty(aDic[YlConstants.RULE_VAR_PROGRAM]))
						{
							String aProgramNameOrigin = aDic[YlConstants.RULE_VAR_PROGRAM];
							List<TTieUpAlias> aTieUpAliases = YlCommon.SelectAliasesByAlias<TTieUpAlias>(aContext, aDic[YlConstants.RULE_VAR_PROGRAM]);
							if (aTieUpAliases.Count > 0)
							{
								TTieUp aTieUpOrigin = YlCommon.SelectMasterById<TTieUp>(aContext, aTieUpAliases[0].OriginalId);
								if (aTieUpOrigin != null)
								{
									aProgramNameOrigin = aTieUpOrigin.Name;
								}
							}
							List<TTieUp> aTieUps = YlCommon.SelectMastersByName<TTieUp>(aContext, aProgramNameOrigin);
							if (aTieUps.Count == 0)
							{
								break;
							}
						}
					}
				}

				SelectedPreviewInfo = PreviewInfos[aRowIndex];
			}
			catch (OperationCanceledException)
			{
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "未登録検出を中止しました。");
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "未登録検出時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 命名規則の変数部分を全てワイルドカードにする
		// --------------------------------------------------------------------
		private String NormalizeRule(String oRule)
		{
			return Regex.Replace(oRule, @"\<.*?\>", YlConstants.RULE_VAR_ANY);
		}

		// --------------------------------------------------------------------
		// プロパティーの値を設定に格納
		// --------------------------------------------------------------------
		private FolderSettingsInDisk PropertiesToSettings()
		{
			FolderSettingsInDisk aFolderSettings = new FolderSettingsInDisk();

			aFolderSettings.AppGeneration = YlConstants.APP_GENERATION;
			aFolderSettings.AppVer = YlConstants.APP_VER;

			aFolderSettings.FileNameRules = FileNameRules.ToList();
			aFolderSettings.FolderNameRules = FolderNameRules.ToList();

			return aFolderSettings;
		}

		// --------------------------------------------------------------------
		// 設定が更新されていれば保存
		// ＜例外＞ OperationCanceledException, Exception
		// --------------------------------------------------------------------
		private void SaveSettingsIfNeeded()
		{
			// 設定途中のファイル命名規則を確認
			if (!String.IsNullOrEmpty(FileNameRule) && !IsFileNameRuleAdded())
			{
				switch (MessageBox.Show("ファイル命名規則に入力中の\n" + FileNameRule + "\nはまだ命名規則として追加されていません。\n追加しますか？",
						"確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation))
				{
					case MessageBoxResult.Yes:
						AddFileNameRule();
						break;
					case MessageBoxResult.No:
						break;
					case MessageBoxResult.Cancel:
						throw new OperationCanceledException("保存を中止しました。");
				}
			}

			// 設定途中のフォルダー固定値を確認
			if (!String.IsNullOrEmpty(FolderNameRuleValueFromProperty()) && FolderNameRules.IndexOf(FolderNameRuleFromProperty()) < 0)
			{
				switch (MessageBox.Show("固定値項目に入力中の\n" + FolderNameRuleFromProperty() + "\nはまだ固定値として追加されていません。\n追加しますか？",
						"確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation))
				{
					case MessageBoxResult.Yes:
						AddFolderNameRule();
						break;
					case MessageBoxResult.No:
						break;
					case MessageBoxResult.Cancel:
						throw new OperationCanceledException("保存を中止しました。");
				}
			}

			if (!mIsDirty)
			{
				return;
			}

			FolderSettingsInDisk aFolderSettings = PropertiesToSettings();

			// 保存
			String aYukaListerConfigPath = PathExLen + "\\" + YlConstants.FILE_NAME_YUKA_LISTER_CONFIG;
			FileAttributes aPrevAttr = new FileAttributes();
			Boolean aHasPrevAttr = false;
			if (File.Exists(aYukaListerConfigPath))
			{
				aPrevAttr = File.GetAttributes(aYukaListerConfigPath);
				aHasPrevAttr = true;

				// 隠しファイルを直接上書きできないので一旦削除する
				File.Delete(aYukaListerConfigPath);
			}
			Common.Serialize(aYukaListerConfigPath, aFolderSettings);
			if (aHasPrevAttr)
			{
				File.SetAttributes(aYukaListerConfigPath, aPrevAttr);
			}

			// ニコカラりすたーの設定ファイルがある場合は削除
			if (File.Exists(PathExLen + "\\" + YlConstants.FILE_NAME_NICO_KARA_LISTER_CONFIG))
			{
				try
				{
					File.Delete(PathExLen + "\\" + YlConstants.FILE_NAME_NICO_KARA_LISTER_CONFIG);
				}
				catch (Exception)
				{
				}
			}

			// 除外設定の保存
			String aYukaListerExcludeConfigPath = PathExLen + "\\" + YlConstants.FILE_NAME_YUKA_LISTER_EXCLUDE_CONFIG;
			if (IsExcluded)
			{
				if (!File.Exists(aYukaListerExcludeConfigPath))
				{
					File.Create(aYukaListerExcludeConfigPath);
				}
			}
			else
			{
				if (File.Exists(aYukaListerExcludeConfigPath))
				{
					File.Delete(aYukaListerExcludeConfigPath);
				}
			}

			// 設定ファイルの状態
			SettingsFileStatus = YlCommon.DetectFolderSettingsStatus2Ex(PathExLen);

			mIsDirty = false;
		}

		// --------------------------------------------------------------------
		// 選択されているルール名から、選択されるべきルール（名前＋値）を取得
		// --------------------------------------------------------------------
		private String SelectedFolderNameRuleFromSelectedFolderNameRuleName()
		{
			String aKey = FindRuleVarName(SelectedFolderNameRuleName);
			String aVarName = WrapVarName(aKey);
			foreach (String aFolderNameRule in FolderNameRules)
			{
				if (aFolderNameRule.IndexOf(aVarName) == 0)
				{
					return aFolderNameRule;
				}
			}

			return null;
		}

		// --------------------------------------------------------------------
		// 選択されたフォルダー固定値の内容を入力欄に反映
		// --------------------------------------------------------------------
		private void SelectedFolderNameRuleToNameAndValue()
		{
			try
			{
				if (String.IsNullOrEmpty(SelectedFolderNameRule))
				{
					return;
				}

				// 名前設定
				String aKey = FindRuleVarName(SelectedFolderNameRule);
				if (String.IsNullOrEmpty(aKey))
				{
					return;
				}
				String aVarName = WrapVarName(aKey);
				for (Int32 i = 0; i < FolderNameRuleNames.Count; i++)
				{
					if (FolderNameRuleNames[i].IndexOf(aVarName) == 0)
					{
						SelectedFolderNameRuleName = FolderNameRuleNames[i];
						break;
					}
				}

				// 値設定
				Int32 aEqualPos = SelectedFolderNameRule.IndexOf('=');
				String aValue = SelectedFolderNameRule.Substring(aEqualPos + 1);
				if (SelectedFolderNameRuleValueVisibility == Visibility.Visible)
				{
					SelectedFolderNameRuleValue = aValue;
				}
				else
				{
					InputFolderNameRuleValue = aValue;
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "固定値入力反映時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// フォルダー設定を読み込み、プロパティーに反映する
		// --------------------------------------------------------------------
		private void SettingsToProperties()
		{
			try
			{
				// 設定ファイルの状態
				SettingsFileStatus = YlCommon.DetectFolderSettingsStatus2Ex(PathExLen);

				// 読み込み
				FolderSettingsInDisk aSettings = YlCommon.LoadFolderSettings2Ex(PathExLen);

				// 設定反映
				FileNameRules.Clear();
				foreach (String aFileNameRule in aSettings.FileNameRules)
				{
					FileNameRules.Add(aFileNameRule);
				}
				FolderNameRules.Clear();
				foreach (String aFolderNameRule in aSettings.FolderNameRules)
				{
					FolderNameRules.Add(aFolderNameRule);
				}

				// 除外設定
				IsExcluded = YlCommon.DetectFolderExcludeSettingsStatus(PathExLen) == FolderExcludeSettingsStatus.True;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "設定読み込み時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// リストの 2 つのアイテムを入れ替える
		// --------------------------------------------------------------------
		private void SwapListItem<T>(IList<T> oList, Int32 oLhsIndex, Int32 oRhsIndex)
		{
			T aTmp = oList[oLhsIndex];
			oList[oLhsIndex] = oList[oRhsIndex];
			oList[oRhsIndex] = aTmp;
		}

		// --------------------------------------------------------------------
		// SelectedFolderNameRuleName の状況に紐付くプロパティーを更新
		// --------------------------------------------------------------------
		private void UpdateFolderNameRuleProperties()
		{
			// 追加済みのフォルダー固定値と同じ項目があれば選択する
			SelectedFolderNameRule = SelectedFolderNameRuleFromSelectedFolderNameRuleName();

			String aRuleName = FindRuleVarName(SelectedFolderNameRuleName);
			if (aRuleName == YlConstants.RULE_VAR_CATEGORY || aRuleName == YlConstants.RULE_VAR_ON_VOCAL || aRuleName == YlConstants.RULE_VAR_OFF_VOCAL)
			{
				// ルール値の入力は選択式
				SelectedFolderNameRuleValueVisibility = Visibility.Visible;
				InputFolderNameRuleValueVisibility = Visibility.Collapsed;

				// 選択肢の準備
				switch (aRuleName)
				{
					case YlConstants.RULE_VAR_CATEGORY:
						FolderNameRuleValues = mCachedCategoryNames;
						break;
					case YlConstants.RULE_VAR_ON_VOCAL:
					case YlConstants.RULE_VAR_OFF_VOCAL:
						List<String> aOnOffVocalValues = new List<String>();
						aOnOffVocalValues.Add(YlConstants.RULE_VALUE_VOCAL_DEFAULT.ToString());
						FolderNameRuleValues = aOnOffVocalValues;
						break;
					default:
						Debug.Assert(false, "UpdateFolderNameRuleComponents() bad aRuleName");
						break;
				}
				SelectedFolderNameRuleValue = null;
			}
			else
			{
				// ルール値の入力は手入力
				SelectedFolderNameRuleValueVisibility = Visibility.Collapsed;
				InputFolderNameRuleValueVisibility = Visibility.Visible;
			}
		}

		// --------------------------------------------------------------------
		// 検索結果を更新
		// --------------------------------------------------------------------
		private void UpdatePreviewResult(Object oDummy)
		{
			try
			{
				// 準備
				ProgressBarPreviewVisibility = Visibility.Visible;

				// クリア
				PreviewInfos.Clear();
				ButtonJumpClickedCommand.RaiseCanExecuteChanged();

				// 検索
				String[] aAllPathes = Directory.GetFiles(PathExLen);

				// マッチをリストに追加
				FolderSettingsInDisk aFolderSettingsInDisk = YlCommon.LoadFolderSettings2Ex(PathExLen);
				FolderSettingsInMemory aFolderSettingsInMemory = YlCommon.CreateFolderSettingsInMemory(aFolderSettingsInDisk);
				Dictionary<String, String> aRuleMap = YlCommon.CreateRuleDictionaryWithDescription();
				foreach (String aPath in aAllPathes)
				{
					if (!Environment.YukaListerSettings.TargetExts.Contains(Path.GetExtension(aPath).ToLower()))
					{
						continue;
					}

					// ファイル命名規則とフォルダー固定値を適用
					Dictionary<String, String> aDic = YlCommon.MatchFileNameRulesAndFolderRule(Path.GetFileNameWithoutExtension(aPath), aFolderSettingsInMemory);

					// ファイル
					PreviewInfo aPreviewInfo = new PreviewInfo();
					aPreviewInfo.FileName = Path.GetFileName(aPath);
					aPreviewInfo.LastWriteTime = JulianDay.DateTimeToModifiedJulianDate(new FileInfo(aPath).LastWriteTime);

					// 項目と値
					StringBuilder aSB = new StringBuilder();
					foreach (KeyValuePair<String, String> aKvp in aDic)
					{
						if (aKvp.Key != YlConstants.RULE_VAR_ANY && !String.IsNullOrEmpty(aKvp.Value))
						{
							aSB.Append(aRuleMap[aKvp.Key] + "=" + aKvp.Value + ", ");
						}
					}
					aPreviewInfo.Items = aSB.ToString();

					// 追加
					PreviewInfos.Add(aPreviewInfo);
#if DEBUGz
					Thread.Sleep(100);
#endif
				}

				ButtonJumpClickedCommand.RaiseCanExecuteChanged();
			}
			catch (OperationCanceledException)
			{
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ファイル検索結果更新を中止しました。");
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル検索結果更新更新時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				// 後片付け
				ProgressBarPreviewVisibility = Visibility.Hidden;
			}
		}

		// --------------------------------------------------------------------
		// 変数名を <> で囲む
		// --------------------------------------------------------------------
		private String WrapVarName(String oVarName)
		{
			if (oVarName == YlConstants.RULE_VAR_ANY)
			{
				return YlConstants.RULE_VAR_ANY;
			}
			else
			{
				TextInfo aTextInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
				return YlConstants.RULE_VAR_BEGIN + aTextInfo.ToTitleCase(oVarName) + YlConstants.RULE_VAR_END;
			}
		}


	}
	// public class FolderSettingsWindowViewModel ___END___

}
// namespace YukaLister.ViewModels ___END___
