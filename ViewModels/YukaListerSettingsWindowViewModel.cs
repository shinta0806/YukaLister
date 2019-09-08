// ============================================================================
// 
// 環境設定ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;

using Shinta;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.Http;
using YukaLister.Models.OutputWriters;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class YukaListerSettingsWindowViewModel : ViewModel
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

		#region ウィンドウのプロパティー

		// ウィンドウタイトル（デフォルトが null だと実行時にエラーが発生するので Empty にしておく）
		private String mTitle = String.Empty;
		public String Title
		{
			get => mTitle;
			set => RaisePropertyChangedIfSet(ref mTitle, value);
		}

		// 選択タブ
		private Int32 mSelectedTabIndex;
		public Int32 SelectedTabIndex
		{
			get => mSelectedTabIndex;
			set => RaisePropertyChangedIfSet(ref mSelectedTabIndex, value);
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

		// UpdaterLauncher
		private UpdaterLauncher mUpdaterLauncher;
		public UpdaterLauncher UpdaterLauncher
		{
			get => mUpdaterLauncher;
			set => RaisePropertyChangedIfSet(ref mUpdaterLauncher, value);
		}

		#endregion

		#region 設定タブのプロパティー

		// ゆかり設定ファイル
		private String mYukariConfigPathSeed;
		public String YukariConfigPathSeed
		{
			get => mYukariConfigPathSeed;
			set
			{
				if (RaisePropertyChangedIfSet(ref mYukariConfigPathSeed, value))
				{
					// ゆかり用リスト出力先フォルダーの算出
					YukaListerSettings aTempYukaListerSettings = new YukaListerSettings();
					try
					{
						aTempYukaListerSettings.YukariConfigPathSeed = mYukariConfigPathSeed;
						YukariListFolder = Path.GetDirectoryName(aTempYukaListerSettings.YukariListDbInDiskPath());
					}
					catch (Exception)
					{
						// エラーは無視する
					}
				}
			}
		}

		// リムーバブルメディア接続時、前回のフォルダーを自動的に追加する
		private Boolean mAddFolderOnDeviceArrived;
		public Boolean AddFolderOnDeviceArrived
		{
			get => mAddFolderOnDeviceArrived;
			set => RaisePropertyChangedIfSet(ref mAddFolderOnDeviceArrived, value);
		}

		// ゆかりでのプレビューを可能にするか
		private Boolean mProvideYukariPreview;
		public Boolean ProvideYukariPreview
		{
			get => mProvideYukariPreview;
			set => RaisePropertyChangedIfSet(ref mProvideYukariPreview, value);
		}

		// ID 接頭辞
		private String mIdPrefix;
		public String IdPrefix
		{
			get => mIdPrefix;
			set => RaisePropertyChangedIfSet(ref mIdPrefix, value);
		}

		#endregion

		#region リスト対象タブのプロパティー
		// リスト化対象ファイルの拡張子
		public ObservableCollection<String> TargetExts { get; set; } = new ObservableCollection<String>();

		// リストで選択されている拡張子
		private String mSelectedTargetExt;
		public String SelectedTargetExt
		{
			get => mSelectedTargetExt;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedTargetExt, value))
				{
					ButtonRemoveExtClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// 追加したい拡張子
		private String mAddingTargetExt;
		public String AddingTargetExt
		{
			get => mAddingTargetExt;
			set
			{
				if (RaisePropertyChangedIfSet(ref mAddingTargetExt, value))
				{
					ButtonAddExtClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion

		#region リスト出力タブのプロパティー
		// ゆかり用リスト出力先フォルダー
		private String mYukariListFolder;
		public String YukariListFolder
		{
			get => mYukariListFolder;
			set => RaisePropertyChangedIfSet(ref mYukariListFolder, value);
		}

		// ゆかりリクエスト用リスト出力前に確認する
		private Boolean mConfirmOutputYukariList;
		public Boolean ConfirmOutputYukariList
		{
			get => mConfirmOutputYukariList;
			set => RaisePropertyChangedIfSet(ref mConfirmOutputYukariList, value);
		}

		// 起動時に前回のゆかりリクエスト用リストをクリアする
		private Boolean mClearPrevList;
		public Boolean ClearPrevList
		{
			get => mClearPrevList;
			set
			{
				if (mClearPrevList && !value
						&& MessageBox.Show("前回のリストをクリアしないと、存在しないファイルがリストに残り齟齬が生じる可能性があります。\n"
						+ "本当にクリアしなくてよろしいですか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning)
						!= MessageBoxResult.Yes)
				{
					return;
				}

				RaisePropertyChangedIfSet(ref mClearPrevList, value);
			}
		}

		// リスト出力形式
		public ObservableCollection<String> ListFormats { get; set; } = new ObservableCollection<String>();

		// 選択されたリスト出力形式
		private String mSelectedListFormat;
		public String SelectedListFormat
		{
			get => mSelectedListFormat;
			set => RaisePropertyChangedIfSet(ref mSelectedListFormat, value);
		}

		// リスト出力先フォルダー
		private String mListFolder;
		public String ListFolder
		{
			get => mListFolder;
			set => RaisePropertyChangedIfSet(ref mListFolder, value);
		}
		#endregion

		#region メンテナンスタブのプロパティー

		// ゆかりすたーの最新情報・更新版を自動的に確認する
		private Boolean mCheckRss;
		public Boolean CheckRss
		{
			get => mCheckRss;
			set
			{
				if (mCheckRss && !value
						&& MessageBox.Show("最新情報・更新版の確認を無効にすると、" + YlConstants.APP_NAME_J
						+ "の新版がリリースされても自動的にインストールされず、古いバージョンを使い続けることになります。\n"
						+ "本当に無効にしてもよろしいですか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning)
						!= MessageBoxResult.Yes)
				{
					return;
				}

				RaisePropertyChangedIfSet(ref mCheckRss, value);
			}
		}

		// プログレスバー表示
		private Visibility mProgressBarCheckRssVisibility;
		public Visibility ProgressBarCheckRssVisibility
		{
			get => mProgressBarCheckRssVisibility;
			set
			{
				if (RaisePropertyChangedIfSet(ref mProgressBarCheckRssVisibility, value))
				{
					ButtonCheckRssClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// デフォルトログファイル名
		public String DefaultLogFileName
		{
			get => "YukaListerLog_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
			set { }
		}

		// 楽曲情報データベースを同期する
		private Boolean mSyncMusicInfoDb;
		public Boolean SyncMusicInfoDb
		{
			get => mSyncMusicInfoDb;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSyncMusicInfoDb, value))
				{
					ButtonRegetClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// サーバー URL
		private String mSyncServer;
		public String SyncServer
		{
			get => mSyncServer;
			set => RaisePropertyChangedIfSet(ref mSyncServer, value);
		}

		// アカウント名
		private String mSyncAccount;
		public String SyncAccount
		{
			get => mSyncAccount;
			set => RaisePropertyChangedIfSet(ref mSyncAccount, value);
		}

		// パスワード
		private String mSyncPassword;
		public String SyncPassword
		{
			get => mSyncPassword;
			set => RaisePropertyChangedIfSet(ref mSyncPassword, value);
		}

		#endregion

		#region インポートタブのプロパティー

		// ゆかりすたーでエクスポートしたファイルをインポート
		private Boolean mImportYukaListerMode;
		public Boolean ImportYukaListerMode
		{
			get => mImportYukaListerMode;
			set
			{
				if (RaisePropertyChangedIfSet(ref mImportYukaListerMode, value))
				{
					ButtonBrowseImportYukaListerClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// ゆかりすたーでエクスポートしたファイルのパス
		private String mImportYukaListerPath;
		public String ImportYukaListerPath
		{
			get => mImportYukaListerPath;
			set => RaisePropertyChangedIfSet(ref mImportYukaListerPath, value);
		}

		// タグ情報をインポートする
		private Boolean mImportTag;
		public Boolean ImportTag
		{
			get => mImportTag;
			set => RaisePropertyChangedIfSet(ref mImportTag, value);
		}

		// anison.info CSV をインポート
		private Boolean mImportAnisonInfoMode;
		public Boolean ImportAnisonInfoMode
		{
			get => mImportAnisonInfoMode;
			set
			{
				if (RaisePropertyChangedIfSet(ref mImportAnisonInfoMode, value))
				{
					ButtonBrowseImportProgramCsvClickedCommand.RaiseCanExecuteChanged();
					ButtonBrowseImportAnisonCsvClickedCommand.RaiseCanExecuteChanged();
					ButtonBrowseImportSfCsvClickedCommand.RaiseCanExecuteChanged();
					ButtonBrowseImportGameCsvClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// program.csv のパス
		private String mImportProgramCsvPath;
		public String ImportProgramCsvPath
		{
			get => mImportProgramCsvPath;
			set
			{
				if (RaisePropertyChangedIfSet(ref mImportProgramCsvPath, value))
				{
					CompleteAnisonInfo(mImportProgramCsvPath);
				}
			}
		}

		// anison.csv のパス
		private String mImportAnisonCsvPath;
		public String ImportAnisonCsvPath
		{
			get => mImportAnisonCsvPath;
			set
			{
				if (RaisePropertyChangedIfSet(ref mImportAnisonCsvPath, value))
				{
					CompleteAnisonInfo(mImportAnisonCsvPath);
				}
			}
		}

		// sf.csv のパス
		private String mImportSfCsvPath;
		public String ImportSfCsvPath
		{
			get => mImportSfCsvPath;
			set
			{
				if (RaisePropertyChangedIfSet(ref mImportSfCsvPath, value))
				{
					CompleteAnisonInfo(mImportSfCsvPath);
				}
			}
		}

		// game.csv のパス
		private String mImportGameCsvPath;
		public String ImportGameCsvPath
		{
			get => mImportGameCsvPath;
			set
			{
				if (RaisePropertyChangedIfSet(ref mImportGameCsvPath, value))
				{
					CompleteAnisonInfo(mImportGameCsvPath);
				}
			}
		}

		// ニコカラりすたーでエクスポートしたファイルをインポート
		private Boolean mImportNicoKaraListerMode;
		public Boolean ImportNicoKaraListerMode
		{
			get => mImportNicoKaraListerMode;
			set
			{
				if (RaisePropertyChangedIfSet(ref mImportNicoKaraListerMode, value))
				{
					ButtonBrowseImportNicoKaraListerClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// ニコカラりすたーでエクスポートしたファイルのパス
		private String mImportNicoKaraListerPath;
		public String ImportNicoKaraListerPath
		{
			get => mImportNicoKaraListerPath;
			set => RaisePropertyChangedIfSet(ref mImportNicoKaraListerPath, value);
		}

		#endregion

		#region エクスポートタブのプロパティー

		// ゆかりすたー情報ファイルのパス
		private String mExportYukaListerPath;
		public String ExportYukaListerPath
		{
			get => mExportYukaListerPath;
			set => RaisePropertyChangedIfSet(ref mExportYukaListerPath, value);
		}

		#endregion

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// ゆかり用リストデータベース（作業用インメモリ）
		public YukariListDatabaseInMemory YukariListDbInMemory { get; set; }

		// OK ボタンが押されたか
		public Boolean IsOk { get; set; }

		// 強制再取得をユーザーから指示されたか
		public Boolean RegetSyncDataNeeded;

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region ウィンドウのコマンド

		#region ヘルプリンクの制御
		public ListenerCommand<String> HelpClickedCommand
		{
			get => Environment?.HelpClickedCommand;
		}
		#endregion

		#region ファイルドロップの制御
		private ListenerCommand<String[]> mTabControlFileDropCommand;

		public ListenerCommand<String[]> TabControlFileDropCommand
		{
			get
			{
				if (mTabControlFileDropCommand == null)
				{
					mTabControlFileDropCommand = new ListenerCommand<String[]>(TabControlFileDrop);
				}
				return mTabControlFileDropCommand;
			}
		}

		public void TabControlFileDrop(String[] oFiles)
		{
			try
			{
				switch (SelectedTabIndex)
				{
					case 0:
						TabItemSettingsFileDrop(oFiles);
						break;
					case 4:
						TabItemImportFileDrop(oFiles);
						break;
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "タブコントロールファイルドロップ時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ちょちょいと自動更新の UI が表示された
		private ViewModelCommand mUpdaterUiDisplayedCommand;

		public ViewModelCommand UpdaterUiDisplayedCommand
		{
			get
			{
				if (mUpdaterUiDisplayedCommand == null)
				{
					mUpdaterUiDisplayedCommand = new ViewModelCommand(UpdaterUiDisplayed);
				}
				return mUpdaterUiDisplayedCommand;
			}
		}

		public void UpdaterUiDisplayed()
		{
			Debug.WriteLine("UpdaterUiDisplayed()");
			ProgressBarCheckRssVisibility = Visibility.Hidden;
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
					mButtonOkClickedCommand = new ViewModelCommand(ButtonOkClicked);
				}
				return mButtonOkClickedCommand;
			}
		}

		public void ButtonOkClicked()
		{
			try
			{
				// Enter キーでボタンが押された場合はテキストボックスからフォーカスが移らずプロパティーが更新されないため強制フォーカス
				IsButtonOkFocused = true;

				CheckInput();
				PropertiesToSettings();
				Environment.YukaListerSettings.Save();
				IsOk = true;
				Messenger.Raise(new WindowActionMessage("Close"));
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#endregion

		#region 設定タブのコマンド

		#region ゆかり設定ファイル参照ボタンの制御
		private ViewModelCommand mButtonBrowseYukariConfigPathSeedClickedCommand;

		public ViewModelCommand ButtonBrowseYukariConfigPathSeedClickedCommand
		{
			get
			{
				if (mButtonBrowseYukariConfigPathSeedClickedCommand == null)
				{
					mButtonBrowseYukariConfigPathSeedClickedCommand = new ViewModelCommand(ButtonBrowseYukariConfigPathSeedClicked);
				}
				return mButtonBrowseYukariConfigPathSeedClickedCommand;
			}
		}

		public void ButtonBrowseYukariConfigPathSeedClicked()
		{
			try
			{
				String aPath = PathByOpeningDialog("ゆかり設定ファイル", "ゆかり設定ファイル|" + YlConstants.FILE_NAME_YUKARI_CONFIG, YlConstants.FILE_NAME_YUKARI_CONFIG);
				if (aPath != null)
				{
					YukariConfigPathSeed = aPath;
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ゆかり設定ファイル参照ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#endregion

		#region リスト対象タブのコマンド

		#region 追加ボタンの制御
		private ViewModelCommand mButtonAddExtClickedCommand;

		public ViewModelCommand ButtonAddExtClickedCommand
		{
			get
			{
				if (mButtonAddExtClickedCommand == null)
				{
					mButtonAddExtClickedCommand = new ViewModelCommand(ButtonAddExtClicked, CanButtonAddExtClicked);
				}
				return mButtonAddExtClickedCommand;
			}
		}

		public Boolean CanButtonAddExtClicked()
		{
			return !String.IsNullOrEmpty(AddingTargetExt);
		}

		public void ButtonAddExtClicked()
		{
			try
			{
				String aExt = AddingTargetExt;

				// 入力が空の場合はボタンは押されないはずだが念のため
				if (String.IsNullOrEmpty(aExt))
				{
					throw new Exception("拡張子を入力して下さい。");
				}

				// ワイルドカード等を除去
				aExt = aExt.Replace("*", "");
				aExt = aExt.Replace("?", "");
				aExt = aExt.Replace(".", "");

				// 除去で空になっていないか
				if (String.IsNullOrEmpty(aExt))
				{
					throw new Exception("有効な拡張子を入力して下さい。");
				}

				// 先頭にピリオド付加
				aExt = "." + aExt;

				// 小文字化
				aExt = aExt.ToLower();

				// 重複チェック
				if (TargetExts.Contains(aExt))
				{
					throw new Exception("既に追加されています。");
				}

				// 追加
				TargetExts.Add(aExt);
				SelectedTargetExt = aExt;
				AddingTargetExt = null;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "追加ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 削除ボタンの制御
		private ViewModelCommand mButtonRemoveExtClickedCommand;

		public ViewModelCommand ButtonRemoveExtClickedCommand
		{
			get
			{
				if (mButtonRemoveExtClickedCommand == null)
				{
					mButtonRemoveExtClickedCommand = new ViewModelCommand(ButtonRemoveExtClicked, CanButtonRemoveExtClicked);
				}
				return mButtonRemoveExtClickedCommand;
			}
		}

		public Boolean CanButtonRemoveExtClicked()
		{
			return !String.IsNullOrEmpty(SelectedTargetExt);
		}

		public void ButtonRemoveExtClicked()
		{
			try
			{
				// 選択されていない場合はボタンが押されないはずだが念のため
				if (String.IsNullOrEmpty(SelectedTargetExt))
				{
					throw new Exception("削除したい拡張子を選択してください。");
				}

				// 削除
				TargetExts.Remove(SelectedTargetExt);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "削除ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#endregion

		#region リスト出力タブのコマンド

		#region ゆかり用リスト出力設定ボタンの制御
		private ViewModelCommand mButtonYukariListSettingsClickedCommand;

		public ViewModelCommand ButtonYukariListSettingsClickedCommand
		{
			get
			{
				if (mButtonYukariListSettingsClickedCommand == null)
				{
					mButtonYukariListSettingsClickedCommand = new ViewModelCommand(ButtonYukariListSettingsClicked);
				}
				return mButtonYukariListSettingsClickedCommand;
			}
		}

		public void ButtonYukariListSettingsClicked()
		{
			try
			{
				YukariOutputWriter aYukariOutputWriter = new YukariOutputWriter(Environment);
				aYukariOutputWriter.OutputSettings.Load();

				using (OutputSettingsWindowViewModel aOutputSettingsWindowViewModel = aYukariOutputWriter.CreateOutputSettingsWindowViewModel())
				{
					aOutputSettingsWindowViewModel.Environment = Environment;
					aOutputSettingsWindowViewModel.OutputWriter = aYukariOutputWriter;
					Messenger.Raise(new TransitionMessage(aOutputSettingsWindowViewModel, "OpenOutputSettingsWindow"));

					if (!aOutputSettingsWindowViewModel.IsOk)
					{
						return;
					}
				}

				// 設定変更をすべての出力者に反映
				LoadOutputSettings();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ゆかりリクエスト用リスト出力設定ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 閲覧用リスト出力設定ボタンの制御
		private ViewModelCommand mButtonListSettingsClickedCommand;

		public ViewModelCommand ButtonListSettingsClickedCommand
		{
			get
			{
				if (mButtonListSettingsClickedCommand == null)
				{
					mButtonListSettingsClickedCommand = new ViewModelCommand(ButtonListSettingsClicked);
				}
				return mButtonListSettingsClickedCommand;
			}
		}

		public void ButtonListSettingsClicked()
		{
			try
			{
				OutputWriter aSelectedOutputWriter = SelectedOutputWriter();

				using (OutputSettingsWindowViewModel aOutputSettingsWindowViewModel = aSelectedOutputWriter.CreateOutputSettingsWindowViewModel())
				{
					aOutputSettingsWindowViewModel.Environment = Environment;
					aOutputSettingsWindowViewModel.OutputWriter = aSelectedOutputWriter;
					Messenger.Raise(new TransitionMessage(aOutputSettingsWindowViewModel, "OpenOutputSettingsWindow"));

					if (!aOutputSettingsWindowViewModel.IsOk)
					{
						return;
					}
				}

				// 設定変更をすべての出力者に反映
				LoadOutputSettings();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "閲覧用リスト出力設定ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}

		}
		#endregion

		#region 閲覧用リスト出力ボタンの制御
		private ViewModelCommand mButtonOutputListClickedCommand;

		public ViewModelCommand ButtonOutputListClickedCommand
		{
			get
			{
				if (mButtonOutputListClickedCommand == null)
				{
					mButtonOutputListClickedCommand = new ViewModelCommand(ButtonOutputListClicked);
				}
				return mButtonOutputListClickedCommand;
			}
		}

		public void ButtonOutputListClicked()
		{
			try
			{
				// 確認
				String aOutputFolderPath = ListFolder;
				if (String.IsNullOrEmpty(aOutputFolderPath))
				{
					throw new Exception("リスト出力先フォルダーを指定して下さい。");
				}
				if (aOutputFolderPath[aOutputFolderPath.Length - 1] != '\\')
				{
					aOutputFolderPath += "\\";
				}
				if (!Directory.Exists(aOutputFolderPath))
				{
					Directory.CreateDirectory(aOutputFolderPath);
				}

				// 出力
				OutputWriter aSelectedOutputWriter = SelectedOutputWriter();
				aSelectedOutputWriter.FolderPath = aOutputFolderPath;
				YlCommon.OutputList(aSelectedOutputWriter, Environment, YukariListDbInMemory);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Information, "リスト出力が完了しました。");

				// 表示
				String aOutputFilePath = aOutputFolderPath + aSelectedOutputWriter.TopFileName;
				try
				{
					Process.Start(aOutputFilePath);
				}
				catch (Exception)
				{
					throw new Exception("出力先ファイルを開けませんでした。\n" + aOutputFilePath);
				}

				// 状態保存（ウィンドウのキャンセルボタンが押されても保存されるように）
				Environment.YukaListerSettings.ListOutputFolder = ListFolder;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "リスト出力ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#endregion

		#region メンテナンスタブのコマンド

		#region 今すぐ最新情報を確認するボタンの制御
		private ViewModelCommand mButtonCheckRssClickedCommand;

		public ViewModelCommand ButtonCheckRssClickedCommand
		{
			get
			{
				if (mButtonCheckRssClickedCommand == null)
				{
					mButtonCheckRssClickedCommand = new ViewModelCommand(ButtonCheckRssClicked, CanButtonCheckRssClicked);
				}
				return mButtonCheckRssClickedCommand;
			}
		}

		public Boolean CanButtonCheckRssClicked()
		{
			return ProgressBarCheckRssVisibility != Visibility.Visible;
		}

		public void ButtonCheckRssClicked()
		{
			try
			{
				ProgressBarCheckRssVisibility = Visibility.Visible;
				UpdaterLauncher = YlCommon.CreateUpdaterLauncher(true, true, true, false, Environment.LogWriter);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "最新情報確認時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ログ保存ボタンの制御
		private ViewModelCommand mButtonLogClickedCommand;

		public ViewModelCommand ButtonLogClickedCommand
		{
			get
			{
				if (mButtonLogClickedCommand == null)
				{
					mButtonLogClickedCommand = new ViewModelCommand(ButtonLogClicked);
				}
				return mButtonLogClickedCommand;
			}
		}

		public void ButtonLogClicked()
		{
			try
			{
				String aPath = PathBySavingDialog("ログ保存", "ログファイル|*" + Common.FILE_EXT_LGA, "YukaListerLog_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss"));
				if (aPath == null)
				{
					return;
				}

				// 環境情報保存
				YlCommon.LogEnvironmentInfo(Environment.LogWriter);

				ZipFile.CreateFromDirectory(YlCommon.SettingsPath(), aPath, CompressionLevel.Optimal, true);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Information, "ログ保存完了：\n" + aPath);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ログ保存時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 強制的に合わせるボタンの制御
		private ViewModelCommand mButtonRegetClickedCommand;

		public ViewModelCommand ButtonRegetClickedCommand
		{
			get
			{
				if (mButtonRegetClickedCommand == null)
				{
					mButtonRegetClickedCommand = new ViewModelCommand(ButtonRegetClicked, CanButtonRegetClicked);
				}
				return mButtonRegetClickedCommand;
			}
		}

		public Boolean CanButtonRegetClicked()
		{
			return SyncMusicInfoDb;
		}

		public void ButtonRegetClicked()
		{
			try
			{
				if (SyncClient.RunningInstanceExists())
				{
					throw new Exception("現在、同期処理を実行中のため、合わせられません。\n同期処理が終了してから合わせてください。");
				}

				if (MessageBox.Show("ローカルの楽曲情報データベースを全て削除してから、内容をサーバーに合わせます。\n"
						+ "タグ情報および、サーバーにアップロードしていないデータは全て失われます。\n"
						+ "事前にエクスポートすることをお薦めします。\n内容をサーバーに合わせてよろしいですか？", "確認",
						MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
				{
					return;
				}

				Environment.YukaListerSettings.LastSyncDownloadDate = 0.0;
				RegetSyncDataNeeded = true;
				Environment.LogWriter.ShowLogMessage(TraceEventType.Information, "環境設定ウィンドウを閉じると処理を開始します。");
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "強制的に合わせる時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#endregion

		#region インポートタブのコマンド

		#region ゆかりすたーファイル参照ボタンの制御
		private ViewModelCommand mButtonBrowseImportYukaListerClickedCommand;

		public ViewModelCommand ButtonBrowseImportYukaListerClickedCommand
		{
			get
			{
				if (mButtonBrowseImportYukaListerClickedCommand == null)
				{
					mButtonBrowseImportYukaListerClickedCommand = new ViewModelCommand(ButtonBrowseImportYukaListerClicked, CanButtonBrowseImportYukaListerClicked);
				}
				return mButtonBrowseImportYukaListerClickedCommand;
			}
		}

		public Boolean CanButtonBrowseImportYukaListerClicked()
		{
			return ImportYukaListerMode;
		}

		public void ButtonBrowseImportYukaListerClicked()
		{
			try
			{
				String aPath = PathByOpeningDialog("ゆかりすたー情報ファイル", "ゆかりすたー情報ファイル|*" + YlConstants.FILE_EXT_YLINFO + "|楽曲情報データベースバックアップ|*" + Common.FILE_EXT_BAK);
				if (aPath != null)
				{
					ImportYukaListerPath = aPath;
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ゆかりすたー情報ファイル参照ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region program.csv 参照ボタンの制御
		private ViewModelCommand mButtonBrowseImportProgramCsvClickedCommand;

		public ViewModelCommand ButtonBrowseImportProgramCsvClickedCommand
		{
			get
			{
				if (mButtonBrowseImportProgramCsvClickedCommand == null)
				{
					mButtonBrowseImportProgramCsvClickedCommand = new ViewModelCommand(ButtonBrowseImportProgramCsvClicked, CanButtonBrowseImportProgramCsvClicked);
				}
				return mButtonBrowseImportProgramCsvClickedCommand;
			}
		}

		public Boolean CanButtonBrowseImportProgramCsvClicked()
		{
			return ImportAnisonInfoMode;
		}

		public void ButtonBrowseImportProgramCsvClicked()
		{
			try
			{
				String aPath = PathByOpeningDialog(YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV
						+ " または " + YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_ZIP,
						IMPORT_ANISON_INFO_FILTER);
				if (aPath != null)
				{
					ImportProgramCsvPath = aPath;
					CompleteAnisonInfo(ImportProgramCsvPath);
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV +
						"参照ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region anison.csv 参照ボタンの制御

		private ViewModelCommand mButtonBrowseImportAnisonCsvClickedCommand;

		public ViewModelCommand ButtonBrowseImportAnisonCsvClickedCommand
		{
			get
			{
				if (mButtonBrowseImportAnisonCsvClickedCommand == null)
				{
					mButtonBrowseImportAnisonCsvClickedCommand = new ViewModelCommand(ButtonBrowseImportAnisonCsvClicked, CanButtonBrowseImportAnisonCsvClicked);
				}
				return mButtonBrowseImportAnisonCsvClickedCommand;
			}
		}

		public Boolean CanButtonBrowseImportAnisonCsvClicked()
		{
			return ImportAnisonInfoMode;
		}

		public void ButtonBrowseImportAnisonCsvClicked()
		{
			try
			{
				String aPath = PathByOpeningDialog(YlConstants.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV
						+ " または " + YlConstants.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_ZIP,
						IMPORT_ANISON_INFO_FILTER);
				if (aPath != null)
				{
					ImportAnisonCsvPath = aPath;
					CompleteAnisonInfo(ImportAnisonCsvPath);
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV +
						"参照ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region sf.csv 参照ボタンの制御
		private ViewModelCommand mButtonBrowseImportSfCsvClickedCommand;

		public ViewModelCommand ButtonBrowseImportSfCsvClickedCommand
		{
			get
			{
				if (mButtonBrowseImportSfCsvClickedCommand == null)
				{
					mButtonBrowseImportSfCsvClickedCommand = new ViewModelCommand(ButtonBrowseImportSfCsvClicked, CanButtonBrowseImportSfCsvClicked);
				}
				return mButtonBrowseImportSfCsvClickedCommand;
			}
		}

		public Boolean CanButtonBrowseImportSfCsvClicked()
		{
			return ImportAnisonInfoMode;
		}

		public void ButtonBrowseImportSfCsvClicked()
		{
			try
			{
				String aPath = PathByOpeningDialog(YlConstants.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV
						+ " または " + YlConstants.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_ZIP,
						IMPORT_ANISON_INFO_FILTER);
				if (aPath != null)
				{
					ImportSfCsvPath = aPath;
					CompleteAnisonInfo(ImportSfCsvPath);
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, YlConstants.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV +
						"参照ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region game.csv 参照ボタンの制御
		private ViewModelCommand mButtonBrowseImportGameCsvClickedCommand;

		public ViewModelCommand ButtonBrowseImportGameCsvClickedCommand
		{
			get
			{
				if (mButtonBrowseImportGameCsvClickedCommand == null)
				{
					mButtonBrowseImportGameCsvClickedCommand = new ViewModelCommand(ButtonBrowseImportGameCsvClicked, CanButtonBrowseImportGameCsvClicked);
				}
				return mButtonBrowseImportGameCsvClickedCommand;
			}
		}

		public Boolean CanButtonBrowseImportGameCsvClicked()
		{
			return ImportAnisonInfoMode;
		}

		public void ButtonBrowseImportGameCsvClicked()
		{
			try
			{
				String aPath = PathByOpeningDialog(YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV
						+ " または " + YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_ZIP,
						IMPORT_ANISON_INFO_FILTER);
				if (aPath != null)
				{
					ImportGameCsvPath = aPath;
					CompleteAnisonInfo(ImportGameCsvPath);
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV +
						"参照ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ニコカラりすたーファイル参照ボタンの制御
		private ViewModelCommand mButtonBrowseImportNicoKaraListerClickedCommand;

		public ViewModelCommand ButtonBrowseImportNicoKaraListerClickedCommand
		{
			get
			{
				if (mButtonBrowseImportNicoKaraListerClickedCommand == null)
				{
					mButtonBrowseImportNicoKaraListerClickedCommand = new ViewModelCommand(ButtonBrowseImportNicoKaraListerClicked, CanButtonBrowseImportNicoKaraListerClicked);
				}
				return mButtonBrowseImportNicoKaraListerClickedCommand;
			}
		}

		public Boolean CanButtonBrowseImportNicoKaraListerClicked()
		{
			return mImportNicoKaraListerMode;
		}

		public void ButtonBrowseImportNicoKaraListerClicked()
		{
			try
			{
				String aPath = PathByOpeningDialog("ニコカラりすたー情報ファイル", "ニコカラりすたー情報ファイル|*" + YlConstants.FILE_EXT_NKLINFO);
				if (aPath != null)
				{
					ImportNicoKaraListerPath = aPath;
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ニコカラりすたー参照ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region インポートボタンの制御
		private ViewModelCommand mButtonImportClickedCommand;

		public ViewModelCommand ButtonImportClickedCommand
		{
			get
			{
				if (mButtonImportClickedCommand == null)
				{
					mButtonImportClickedCommand = new ViewModelCommand(ButtonImportClicked);
				}
				return mButtonImportClickedCommand;
			}
		}

		public void ButtonImportClicked()
		{
			try
			{
				using (ImportWindowViewModel aImportWindowViewModel = new ImportWindowViewModel())
				{
					aImportWindowViewModel.Environment = Environment;

					// ゆかりすたーでエクスポートしたファイルをインポート
					aImportWindowViewModel.ImportYukaListerMode = ImportYukaListerMode;
					aImportWindowViewModel.ImportYukaListerPath = ImportYukaListerPath;
					aImportWindowViewModel.ImportTag = ImportTag;

					// anison.info CSV をインポート
					aImportWindowViewModel.ImportAnisonInfoMode = ImportAnisonInfoMode;
					aImportWindowViewModel.ImportProgramCsvPath = ImportProgramCsvPath;
					aImportWindowViewModel.ImportAnisonCsvPath = ImportAnisonCsvPath;
					aImportWindowViewModel.ImportSfCsvPath = ImportSfCsvPath;
					aImportWindowViewModel.ImportGameCsvPath = ImportGameCsvPath;

					// ニコカラりすたーでエクスポートしたファイルをインポート
					aImportWindowViewModel.ImportNicoKaraListerMode = ImportNicoKaraListerMode;
					aImportWindowViewModel.ImportNicoKaraListerPath = ImportNicoKaraListerPath;

					Messenger.Raise(new TransitionMessage(aImportWindowViewModel, "OpenImportExportWindow"));

					// IdPrefix の更新を反映
					IdPrefix = Environment.YukaListerSettings.IdPrefix;
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "インポートボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#endregion

		#region エクスポートタブのコマンド

		#region 参照ボタンの制御
		private ViewModelCommand mButtonBrowseExportYukaListerClickedCommand;

		public ViewModelCommand ButtonBrowseExportYukaListerClickedCommand
		{
			get
			{
				if (mButtonBrowseExportYukaListerClickedCommand == null)
				{
					mButtonBrowseExportYukaListerClickedCommand = new ViewModelCommand(ButtonBrowseExportYukaListerClicked);
				}
				return mButtonBrowseExportYukaListerClickedCommand;
			}
		}

		public void ButtonBrowseExportYukaListerClicked()
		{
			try
			{
				String aPath = PathBySavingDialog("エクスポート", "ゆかりすたー情報ファイル|*" + YlConstants.FILE_EXT_YLINFO, "YukaListerInfo_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss"));
				if (aPath != null)
				{
					ExportYukaListerPath = aPath;
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ログ保存時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}

		}
		#endregion

		#region エクスポートボタンの制御
		private ViewModelCommand mButtonExportClickedCommand;

		public ViewModelCommand ButtonExportClickedCommand
		{
			get
			{
				if (mButtonExportClickedCommand == null)
				{
					mButtonExportClickedCommand = new ViewModelCommand(ButtonExportClicked);
				}
				return mButtonExportClickedCommand;
			}
		}

		public void ButtonExportClicked()
		{
			try
			{
				using (ExportWindowViewModel aExportWindowViewModel = new ExportWindowViewModel())
				{
					aExportWindowViewModel.Environment = Environment;
					aExportWindowViewModel.ExportYukaListerPath = ExportYukaListerPath;

					Messenger.Raise(new TransitionMessage(aExportWindowViewModel, "OpenImportExportWindow"));
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "エクスポートボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

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
				Title = "環境設定";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				// リスト出力形式
				mOutputWriters = new List<OutputWriter>();
				mOutputWriters.Add(new HtmlOutputWriter(Environment));
				mOutputWriters.Add(new CsvOutputWriter(Environment));
				LoadOutputSettings();

				// プログレスバー
				ProgressBarCheckRssVisibility = Visibility.Hidden;

				SettingsToProperties();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "環境設定ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void ListFolderSelected(FolderSelectionMessage oFolderSelectionMessage)
		{
			try
			{
				if (String.IsNullOrEmpty(oFolderSelectionMessage.Response))
				{
					return;
				}
				ListFolder = oFolderSelectionMessage.Response;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "リスト出力先フォルダー選択時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void LogFileSelected(SavingFileSelectionMessage oSavingFileSelectionMessage)
		{

		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		public void YukariConfigPathSeedSelected(OpeningFileSelectionMessage oOpeningFileSelectionMessage)
		{
			try
			{
				if (oOpeningFileSelectionMessage.Response == null)
				{
					return;
				}

				YukariConfigPathSeed = oOpeningFileSelectionMessage.Response[0];
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ゆかり設定ファイル選択時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------
		private const String IMPORT_ANISON_INFO_FILTER = "anison.info ファイル|*" + Common.FILE_EXT_CSV + ";*" + Common.FILE_EXT_ZIP;

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// リスト出力
		private List<OutputWriter> mOutputWriters;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力された値が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckInput()
		{
			// 設定タブ
			if (String.IsNullOrEmpty(YukariConfigPathSeed))
			{
				throw new Exception("ゆかり設定ファイルを指定して下さい。");
			}
			IdPrefix = YlCommon.CheckIdPrefix(IdPrefix, true);

			// リスト対象タブ
			if (TargetExts.Count == 0)
			{
				throw new Exception("リスト化対象ファイルの拡張子を指定して下さい。");
			}

			// メンテナンスタブ
			if (SyncMusicInfoDb)
			{
				if (String.IsNullOrEmpty(SyncServer) || SyncServer == "http://" || SyncServer == "https://")
				{
					throw new Exception("同期用のサーバー URL を指定して下さい。");
				}
				if (SyncServer.IndexOf("http://") != 0 && SyncServer.IndexOf("https://") != 0)
				{
					throw new Exception("http:// または https:// で始まる同期用のサーバー URL を指定して下さい。");
				}
				if (String.IsNullOrEmpty(SyncAccount))
				{
					throw new Exception("同期用のアカウント名を指定して下さい。");
				}
				if (String.IsNullOrEmpty(SyncPassword))
				{
					throw new Exception("同期用のパスワードを指定して下さい。");
				}

				// 補完
				if (SyncServer[SyncServer.Length - 1] != '/')
				{
					SyncServer += "/";
				}
			}
		}

		// --------------------------------------------------------------------
		// anison.info CSV パス欄を補完
		// --------------------------------------------------------------------
		private void CompleteAnisonInfo(String oStandardPath)
		{
			try
			{
				// program.csv
				if (String.IsNullOrEmpty(ImportProgramCsvPath))
				{
					ImportProgramCsvPath = CompleteAnisonInfo(oStandardPath, YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM);
				}

				// anison.csv
				if (String.IsNullOrEmpty(ImportAnisonCsvPath))
				{
					ImportAnisonCsvPath = CompleteAnisonInfo(oStandardPath, YlConstants.FILE_BODY_ANISON_INFO_CSV_ANISON);
				}

				// sf.csv
				if (String.IsNullOrEmpty(ImportSfCsvPath))
				{
					ImportSfCsvPath = CompleteAnisonInfo(oStandardPath, YlConstants.FILE_BODY_ANISON_INFO_CSV_SF);
				}

				// game.csv
				if (String.IsNullOrEmpty(ImportGameCsvPath))
				{
					ImportGameCsvPath = CompleteAnisonInfo(oStandardPath, YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME);
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "anison.info CSV 補完時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// anison.info CSV パス欄を補完のサブ関数
		// --------------------------------------------------------------------
		private String CompleteAnisonInfo(String oStandardPath, String oFileBody)
		{
			// フォルダー名
			String aFolder;
			try
			{
				aFolder = Path.GetDirectoryName(oStandardPath);
			}
			catch (Exception)
			{
				// 指定されたパスがパスの形式ではない場合は補完しない
				return null;
			}

			// Path.GetDirectoryName() はルートフォルダーの場合のみ末尾が '\\' となるため、常に '\\' となるように変換する
			if (!String.IsNullOrEmpty(aFolder) && aFolder[aFolder.Length - 1] != '\\')
			{
				aFolder += "\\";
			}

			// CSV がある場合に設定
			if (File.Exists(aFolder + oFileBody + Common.FILE_EXT_CSV))
			{
				return aFolder + oFileBody + Common.FILE_EXT_CSV;
			}

			// ZIP がある場合に設定
			if (File.Exists(aFolder + oFileBody + Common.FILE_EXT_ZIP))
			{
				return aFolder + oFileBody + Common.FILE_EXT_ZIP;
			}

			return null;
		}

		// --------------------------------------------------------------------
		// すべての出力者の OutputSettings を読み込む
		// --------------------------------------------------------------------
		private void LoadOutputSettings()
		{
			foreach (OutputWriter aOutputWriter in mOutputWriters)
			{
				aOutputWriter.OutputSettings.Load();
			}
		}

		// --------------------------------------------------------------------
		// 開くダイアログを表示し、ファイルパスを取得
		// --------------------------------------------------------------------
		private String PathByOpeningDialog(String oTitle, String oFilter, String oFileName = null)
		{
			OpeningFileSelectionMessage aMessage = new OpeningFileSelectionMessage("OpenOpenFileDialog");
			aMessage.Title = oTitle;
			aMessage.Filter = oFilter;
			aMessage.FileName = oFileName;
			Messenger.Raise(aMessage);
			if (aMessage.Response == null)
			{
				return null;
			}

			return aMessage.Response[0];
		}

		// --------------------------------------------------------------------
		// 保存ダイアログを表示し、ファイルパスを取得
		// --------------------------------------------------------------------
		private String PathBySavingDialog(String oTitle, String oFilter, String oFileName = null)
		{
			SavingFileSelectionMessage aMessage = new SavingFileSelectionMessage("OpenSaveFileDialog");
			aMessage.Title = oTitle;
			aMessage.Filter = oFilter;
			aMessage.FileName = oFileName;
			Messenger.Raise(aMessage);
			if (aMessage.Response == null)
			{
				return null;
			}

			return aMessage.Response[0];
		}

		// --------------------------------------------------------------------
		// プロパティーから設定に反映
		// --------------------------------------------------------------------
		private void PropertiesToSettings()
		{
			// 設定タブ
			Environment.YukaListerSettings.YukariConfigPathSeed = YukariConfigPathSeed;
			Environment.YukaListerSettings.AddFolderOnDeviceArrived = AddFolderOnDeviceArrived;
			Environment.YukaListerSettings.ProvideYukariPreview = ProvideYukariPreview;
			Environment.YukaListerSettings.IdPrefix = IdPrefix;

			// リスト対象タブ
			Environment.YukaListerSettings.TargetExts.Clear();
			Environment.YukaListerSettings.TargetExts.AddRange(TargetExts);
			Environment.YukaListerSettings.TargetExts.Sort();

			// リスト出力タブ
			Environment.YukaListerSettings.ListOutputFolder = ListFolder;
			Environment.YukaListerSettings.ConfirmOutputYukariList = ConfirmOutputYukariList;
			Environment.YukaListerSettings.ClearPrevList = ClearPrevList;

			// メンテナンスタブ
			Environment.YukaListerSettings.CheckRss = CheckRss;
			Environment.YukaListerSettings.SyncMusicInfoDb = SyncMusicInfoDb;
			Environment.YukaListerSettings.SyncServer = SyncServer;
			Environment.YukaListerSettings.SyncAccount = SyncAccount;
			Environment.YukaListerSettings.SyncPassword = YlCommon.Encrypt(SyncPassword);
		}

		// --------------------------------------------------------------------
		// 選択された出力アドオン
		// --------------------------------------------------------------------
		private OutputWriter SelectedOutputWriter()
		{
			foreach (OutputWriter aOutputWriter in mOutputWriters)
			{
				if (aOutputWriter.FormatName == SelectedListFormat)
				{
					return aOutputWriter;
				}
			}

			return null;
		}

		// --------------------------------------------------------------------
		// 設定をプロパティーに反映
		// --------------------------------------------------------------------
		private void SettingsToProperties()
		{
			// 設定タブ
			YukariConfigPathSeed = Environment.YukaListerSettings.YukariConfigPathSeed;
			AddFolderOnDeviceArrived = Environment.YukaListerSettings.AddFolderOnDeviceArrived;
			ProvideYukariPreview = Environment.YukaListerSettings.ProvideYukariPreview;
			IdPrefix = Environment.YukaListerSettings.IdPrefix;

			// リスト対象タブ
			foreach (String aExt in Environment.YukaListerSettings.TargetExts)
			{
				TargetExts.Add(aExt);
			}

			// リスト出力タブ
			ConfirmOutputYukariList = Environment.YukaListerSettings.ConfirmOutputYukariList;
			ClearPrevList = Environment.YukaListerSettings.ClearPrevList;
			foreach (OutputWriter aOutputWriter in mOutputWriters)
			{
				ListFormats.Add(aOutputWriter.FormatName);
			}
			SelectedListFormat = ListFormats[0];
			ListFolder = Environment.YukaListerSettings.ListOutputFolder;

			// メンテナンスタブ
			CheckRss = Environment.YukaListerSettings.CheckRss;
			SyncMusicInfoDb = Environment.YukaListerSettings.SyncMusicInfoDb;
			SyncServer = Environment.YukaListerSettings.SyncServer;
			SyncAccount = Environment.YukaListerSettings.SyncAccount;
			SyncPassword = YlCommon.Decrypt(Environment.YukaListerSettings.SyncPassword);

			// インポートタブ
			ImportYukaListerMode = true;
		}

		// --------------------------------------------------------------------
		// インポートタブのファイルドロップ
		// --------------------------------------------------------------------
		private void TabItemImportFileDrop(String[] oFiles)
		{
			String aNotHandledFiles = null;
			foreach (String aFile in oFiles)
			{
				if (!File.Exists(aFile))
				{
					continue;
				}

				String aExt = Path.GetExtension(aFile).ToLower();
				String aFileName = Path.GetFileName(aFile);
				if (aExt == Common.FILE_EXT_CSV || aExt == Common.FILE_EXT_ZIP)
				{
					// anison.info CSV インポート
					if (aFileName.IndexOf(YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						ImportProgramCsvPath = aFile;
					}
					else if (aFileName.IndexOf(YlConstants.FILE_BODY_ANISON_INFO_CSV_ANISON, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						ImportAnisonCsvPath = aFile;
					}
					else if (aFileName.IndexOf(YlConstants.FILE_BODY_ANISON_INFO_CSV_SF, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						ImportSfCsvPath = aFile;
					}
					else if (aFileName.IndexOf(YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						ImportGameCsvPath = aFile;
					}
					else
					{
						aNotHandledFiles += Path.GetFileName(aFile) + "\n";
					}
					ImportAnisonInfoMode = true;
				}
				else if (aExt == YlConstants.FILE_EXT_NKLINFO)
				{
					// ニコカラりすたーインポート
					ImportNicoKaraListerPath = aFile;
					ImportNicoKaraListerMode = true;
				}
				else
				{
					aNotHandledFiles += Path.GetFileName(aFile) + "\n";
				}

			}
			if (!String.IsNullOrEmpty(aNotHandledFiles))
			{
				throw new Exception("ドロップされたファイルの種類を自動判定できませんでした。\n参照ボタンからファイルを指定して下さい。\n" + aNotHandledFiles);
			}
		}

		// --------------------------------------------------------------------
		// 設定タブのファイルドロップ
		// --------------------------------------------------------------------
		private void TabItemSettingsFileDrop(String[] oFiles)
		{
			String aNotHandledFiles = null;
			foreach (String aFile in oFiles)
			{
				if (!File.Exists(aFile))
				{
					continue;
				}

				String aExt = Path.GetExtension(aFile).ToLower();
				if (aExt == Common.FILE_EXT_INI)
				{
					YukariConfigPathSeed = aFile;
				}
				else
				{
					aNotHandledFiles += Path.GetFileName(aFile) + "\n";
				}
			}
			if (!String.IsNullOrEmpty(aNotHandledFiles))
			{
				throw new Exception("ドロップされたファイルの種類を自動判定できませんでした。\n参照ボタンからファイルを指定して下さい。\n" + aNotHandledFiles);
			}
		}



	}
	// public class YukaListerSettingsWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
