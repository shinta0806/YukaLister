﻿// ============================================================================
// 
// インポートウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// CSV からのインポート時は、主要な項目が既にデータベースに登録されている場合は
// インポートしない（手入力した詳細レコードとインポートによる簡易レコードが重複
// するのを避けるため）
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;

using Shinta;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class ImportWindowViewModel : ViewModel
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

		// ウィンドウを閉じても良いか
		private Boolean mCanClose;
		public Boolean CanClose
		{
			get => mCanClose;
			set => RaisePropertyChangedIfSet(ref mCanClose, value);
		}

		// 説明
		private String mDescription;
		public String Description
		{
			get => mDescription;
			set
			{
				if (RaisePropertyChangedIfSet(ref mDescription, value))
				{
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, mDescription);
				}
			}
		}

		// 進捗
		private String mProgress;
		public String Progress
		{
			get => mProgress;
			set
			{
				if (RaisePropertyChangedIfSet(ref mProgress, value))
				{
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, mProgress);
				}
			}
		}

		// ログ
		public ObservableCollection<String> Logs { get; set; } = new ObservableCollection<String>();

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// ゆかりすたーでエクスポートしたファイルをインポート
		public Boolean ImportYukaListerMode { get; set; }
		public String ImportYukaListerPath { get; set; }

		// anison.info CSV をインポート
		public Boolean ImportAnisonInfoMode { get; set; }
		public String ImportProgramCsvPath { get; set; }
		public String ImportAnisonCsvPath { get; set; }
		public String ImportSfCsvPath { get; set; }
		public String ImportGameCsvPath { get; set; }

		// ニコカラりすたーでエクスポートしたファイルをインポート
		public Boolean ImportNicoKaraListerMode { get; set; }
		public String ImportNicoKaraListerPath { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region ウィンドウを閉じられるかの制御
		private ListenerCommand<CancelEventArgs> mWindowClosingCommand;

		public ListenerCommand<CancelEventArgs> WindowClosingCommand
		{
			get
			{
				if (mWindowClosingCommand == null)
				{
					mWindowClosingCommand = new ListenerCommand<CancelEventArgs>(WindowClosing);
				}
				return mWindowClosingCommand;
			}
		}

		public void WindowClosing(CancelEventArgs oCancelEventArgs)
		{
			try
			{
				if (!CancelImportIfNeeded())
				{
					// インポートをキャンセルしなかった場合はクローズをキャンセル
					oCancelEventArgs.Cancel = true;
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "クローズ処理時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 中止ボタンの制御
		private ViewModelCommand mButtonAbortClickedCommand;

		public ViewModelCommand ButtonAbortClickedCommand
		{
			get
			{
				if (mButtonAbortClickedCommand == null)
				{
					mButtonAbortClickedCommand = new ViewModelCommand(ButtonAbortClicked);
				}
				return mButtonAbortClickedCommand;
			}
		}

		public void ButtonAbortClicked()
		{
			try
			{
				if (CancelImportIfNeeded())
				{
					Messenger.Raise(new WindowActionMessage("Close"));
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "中止ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ログ文字列に追加
		// --------------------------------------------------------------------
		public void AppendDisplayText(String oText)
		{
			Logs.Add(oText);
		}

		// --------------------------------------------------------------------
		// 初期化
		// 本関数を呼ぶ前に Environment を設定しておく必要がある
		// --------------------------------------------------------------------
		public async void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				// タイトルバー
				if (ImportYukaListerMode)
				{
					Title = "ゆかりすたーでエクスポートしたファイルのインポート";
				}
				else if (ImportAnisonInfoMode)
				{
					Title = "anison.info CSV のインポート";
				}
				else if (ImportNicoKaraListerMode)
				{
					Title = "ニコカラりすたーでエクスポートしたファイルのインポート";
				}
				else
				{
					Debug.Assert(false, "ImportWindowViewModel.Initialize() bad mode");
				}
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				mCategoryUnityMap = YlCommon.CreateCategoryUnityMap();
				Environment.LogWriter.AppendDisplayText = AppendDisplayText;

				// 楽曲情報データベースバックアップ
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					aMusicInfoDbInDisk.Backup();
				}

				YlCommon.InputIdPrefixIfNeededWithInvoke(this, Environment);

				// インポートタスクを実行（async の終了を待つ）
				if (ImportYukaListerMode)
				{
					// 未実装
				}
				else if (ImportAnisonInfoMode)
				{
					await ImportAnisonInfoAsync();
				}
				else if (ImportNicoKaraListerMode)
				{
					await ImportNicoKaraListerAsync();
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "インポートウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				Environment.LogWriter.AppendDisplayText = null;
				Messenger.Raise(new WindowActionMessage("Close"));
			}
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// CSV 登録進捗表示間隔
		private const Int32 NUM_CSV_IMPORT_PROGRESS = 1000;

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 挿入待ちの TPerson
		private Dictionary<String, String> mInsertingPersons = new Dictionary<String, String>();

		// 挿入待ちの TSong
		private Dictionary<String, String> mInsertingSongs = new Dictionary<String, String>();

		// 挿入待ちの TTieUp
		private Dictionary<String, String> mInsertingTieUps = new Dictionary<String, String>();

		// 番組分類統合用マップ
		Dictionary<String, String> mCategoryUnityMap;

		// タスク中止用
		private CancellationTokenSource mAbortCancellationTokenSource = new CancellationTokenSource();

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// インポートをキャンセル
		// ＜返値＞ true: キャンセルした（または既にされている）, false: キャンセルしなかった
		// --------------------------------------------------------------------
		private Boolean CancelImportIfNeeded()
		{
			if (mAbortCancellationTokenSource.IsCancellationRequested)
			{
				// 既にキャンセル処理中
				return true;
			}

			if (MessageBox.Show("インポートを中止してよろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.No)
			{
				// 新たにキャンセル
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "インポートを中止しています...");
				mAbortCancellationTokenSource.Cancel();
				return true;
			}

			// キャンセルしない
			return false;
		}

		// --------------------------------------------------------------------
		// anison.info CSV インポート用の設定確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckImportAnisonInfo()
		{
			// 最低 1 つはファイル指定必要
			if (String.IsNullOrEmpty(ImportProgramCsvPath) && String.IsNullOrEmpty(ImportAnisonCsvPath) && String.IsNullOrEmpty(ImportSfCsvPath) && String.IsNullOrEmpty(ImportGameCsvPath))
			{
				throw new Exception("anison.info CSV ファイルを指定して下さい。");
			}

			// program.csv
			if (!String.IsNullOrEmpty(ImportProgramCsvPath))
			{
				if (Path.GetExtension(ImportProgramCsvPath).ToLower() != Common.FILE_EXT_CSV && Path.GetExtension(ImportProgramCsvPath).ToLower() != Common.FILE_EXT_ZIP
						|| Path.GetFileNameWithoutExtension(ImportProgramCsvPath).ToLower() != YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM)
				{
					throw new Exception(YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV + " 指定欄には、"
							+ YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV + " または "
							+ YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_ZIP + " を指定して下さい。");
				}
				if (!File.Exists(ImportProgramCsvPath))
				{
					throw new Exception(ImportProgramCsvPath + " が見つかりません。");
				}
			}

			// anison.csv
			if (!String.IsNullOrEmpty(ImportAnisonCsvPath))
			{
				if (Path.GetExtension(ImportAnisonCsvPath).ToLower() != Common.FILE_EXT_CSV && Path.GetExtension(ImportAnisonCsvPath).ToLower() != Common.FILE_EXT_ZIP
						|| Path.GetFileNameWithoutExtension(ImportAnisonCsvPath).ToLower() != YlConstants.FILE_BODY_ANISON_INFO_CSV_ANISON)
				{
					throw new Exception(YlConstants.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV + " 指定欄には、"
							+ YlConstants.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV + " または "
							+ YlConstants.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_ZIP + " を指定して下さい。");
				}
				if (!File.Exists(ImportAnisonCsvPath))
				{
					throw new Exception(ImportAnisonCsvPath + " が見つかりません。");
				}
			}

			// sf.csv
			if (!String.IsNullOrEmpty(ImportSfCsvPath))
			{
				if (Path.GetExtension(ImportSfCsvPath).ToLower() != Common.FILE_EXT_CSV && Path.GetExtension(ImportSfCsvPath).ToLower() != Common.FILE_EXT_ZIP
					|| Path.GetFileNameWithoutExtension(ImportSfCsvPath).ToLower() != YlConstants.FILE_BODY_ANISON_INFO_CSV_SF)
				{
					throw new Exception(YlConstants.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV + " 指定欄には、"
							+ YlConstants.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV + " または "
							+ YlConstants.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_ZIP + " を指定して下さい。");
				}
				if (!File.Exists(ImportSfCsvPath))
				{
					throw new Exception(ImportSfCsvPath + " が見つかりません。");
				}
			}

			// game.csv
			if (!String.IsNullOrEmpty(ImportGameCsvPath))
			{
				if (Path.GetExtension(ImportGameCsvPath).ToLower() != Common.FILE_EXT_CSV && Path.GetExtension(ImportGameCsvPath).ToLower() != Common.FILE_EXT_ZIP
					|| Path.GetFileNameWithoutExtension(ImportGameCsvPath).ToLower() != YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME)
				{
					throw new Exception(YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV + " 指定欄には、"
							+ YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV + " または "
							+ YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_ZIP + " を指定して下さい。");
				}
				if (!File.Exists(ImportGameCsvPath))
				{
					throw new Exception(ImportGameCsvPath + " が見つかりません。");
				}
			}
		}

		// --------------------------------------------------------------------
		// ニコカラりすたーインポート用の設定確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckImportNicoKaraLister()
		{
			if (String.IsNullOrEmpty(ImportNicoKaraListerPath))
			{
				throw new Exception("ニコカラりすたーでエクスポートしたファイルを指定して下さい。");
			}

			if (Path.GetExtension(ImportNicoKaraListerPath).ToLower() != YlConstants.FILE_EXT_NKLINFO)
			{
				throw new Exception("ニコカラりすたーでエクスポートしたファイルではないファイルが指定されています。");
			}
		}

		// --------------------------------------------------------------------
		// CSV レコード同士の比較（指定された列で比較）
		// --------------------------------------------------------------------
		private Int32 CompareCsv(List<String> oLhs, List<String> oRhs, Int32 oColumnIndex)
		{
			if (oLhs.Count == 0 && oRhs.Count == 0)
			{
				return 0;
			}
			if (oLhs.Count == 0)
			{
				return -1;
			}
			if (oRhs.Count == 0)
			{
				return 1;
			}
			return String.Compare(oLhs[oColumnIndex], oRhs[oColumnIndex]);
		}

		// --------------------------------------------------------------------
		// program_alias.csv レコード同士の比較
		// --------------------------------------------------------------------
		private Int32 CompareProgramAliasCsv(List<String> oLhs, List<String> oRhs)
		{
			return CompareCsv(oLhs, oRhs, (Int32)ProgramAliasCsvColumns.NameOrId);
		}

		// --------------------------------------------------------------------
		// program.csv レコード同士の比較
		// --------------------------------------------------------------------
		private Int32 CompareProgramCsv(List<String> oLhs, List<String> oRhs)
		{
			return CompareCsv(oLhs, oRhs, (Int32)ProgramCsvColumns.Id);
		}

		// --------------------------------------------------------------------
		// anison_alias.csv 等のレコード同士の比較
		// --------------------------------------------------------------------
		private Int32 CompareSongAliasCsv(List<String> oLhs, List<String> oRhs)
		{
			return CompareCsv(oLhs, oRhs, (Int32)SongAliasCsvColumns.NameOrId);
		}

		// --------------------------------------------------------------------
		// anison.csv 等のレコード同士の比較
		// --------------------------------------------------------------------
		private Int32 CompareSongCsv(List<String> oLhs, List<String> oRhs)
		{
			return CompareCsv(oLhs, oRhs, (Int32)SongCsvColumns.Id);
		}

		// --------------------------------------------------------------------
		// "yyyy-mm-dd" をユリウス日に変換
		// 月日が 00 になっている場合があるため、DateTime.ParseExact() は使えない
		// --------------------------------------------------------------------
		private Double CsvDateStringToModifiedJulianDate(String oCsvDateString)
		{
			if (String.IsNullOrEmpty(oCsvDateString) || oCsvDateString.Length < 10)
			{
				return YlConstants.INVALID_MJD;
			}

			Int32 aYear;
			Int32 aMonth;
			Int32 aDay;

			try
			{
				aYear = Int32.Parse(oCsvDateString.Substring(0, 4));
				if (aYear < YlConstants.INVALID_YEAR)
				{
					aYear = YlConstants.INVALID_YEAR;
				}
				aMonth = Int32.Parse(oCsvDateString.Substring(5, 2));
				if (aMonth <= 0 || aMonth > 12)
				{
					aMonth = 1;
				}
				aDay = Int32.Parse(oCsvDateString.Substring(8, 2));
				if (aDay <= 0 || aDay > 31)
				{
					aDay = 1;
				}
			}
			catch (Exception)
			{
				return YlConstants.INVALID_MJD;
			}

			return JulianDay.DateTimeToModifiedJulianDate(new DateTime(aYear, aMonth, aDay));
		}

		// --------------------------------------------------------------------
		// カテゴリー名からカテゴリー ID を検索（無ければ null を返す）
		// ＜返値＞ カテゴリー ID or null
		// --------------------------------------------------------------------
		private String FindCategory(String oCategoryName, DataContext oContext)
		{
			String aNormalizedCategoryName = YlCommon.NormalizeDbString(oCategoryName);
			if (String.IsNullOrEmpty(aNormalizedCategoryName))
			{
				return null;
			}

			if (mCategoryUnityMap.ContainsKey(aNormalizedCategoryName))
			{
				aNormalizedCategoryName = mCategoryUnityMap[aNormalizedCategoryName];
			}

			// 楽曲情報データベースから検索
			List<TCategory> aCategories = YlCommon.SelectMastersByName<TCategory>(oContext, aNormalizedCategoryName);
			if (aCategories.Count > 0)
			{
				return aCategories[0].Id;
			}

			return null;
		}

		// --------------------------------------------------------------------
		// 人物名から人物 ID を検索（無ければ新規挿入）
		// ＜返値＞ 人物 ID 群
		// --------------------------------------------------------------------
		private List<String> FindOrInsertPeople(String oArtistNames, DataContext oContext)
		{
			List<String> aPeople = new List<String>();

			// 全角カンマがあるとうまく区切れないため一度正規化
			String aNormalizedArtistNames = YlCommon.NormalizeDbString(oArtistNames);
			if (String.IsNullOrEmpty(aNormalizedArtistNames))
			{
				return aPeople;
			}

			Table<TPerson> aTable = oContext.GetTable<TPerson>();
			String[] aSplits = aNormalizedArtistNames.Split(',');
			foreach (String aOneName in aSplits)
			{
				// 前後に空白がある可能性があるため再度正規化
				String aNormalizedOneName = YlCommon.NormalizeDbString(aOneName);
				if (String.IsNullOrEmpty(aNormalizedOneName))
				{
					continue;
				}

				// 楽曲情報データベースから検索
				List<TPerson> aDbPeople = YlCommon.SelectMastersByName<TPerson>(oContext, aNormalizedOneName, true);
				if (aDbPeople.Count > 0)
				{
					aPeople.Add(aDbPeople[0].Id);
					continue;
				}

				// 挿入待ちから検索
				if (mInsertingPersons.ContainsKey(aNormalizedOneName))
				{
					aPeople.Add(mInsertingPersons[aNormalizedOneName]);
					continue;
				}

				// 新規挿入
				String aId = Environment.YukaListerSettings.PrepareLastId((SQLiteConnection)oContext.Connection, MusicInfoDbTables.TPerson);
				aTable.InsertOnSubmit(new TPerson
				{
					// TBase
					Id = aId,
					Import = true,
					Invalid = false,
					UpdateTime = YlConstants.INVALID_MJD,
					Dirty = true,

					// TMaster
					Name = aNormalizedOneName,
					Ruby = null,
					Keyword = null,
				});
				mInsertingPersons[aNormalizedOneName] = aId;
				aPeople.Add(aId);
			}

			return aPeople;
		}

		// --------------------------------------------------------------------
		// anison.info CSV をインポート
		// --------------------------------------------------------------------
		private Task ImportAnisonInfoAsync()
		{
			return Task.Run(() =>
			{
				try
				{
					// 終了時に強制終了されないように設定
					Thread.CurrentThread.IsBackground = false;

					CheckImportAnisonInfo();

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
					{
						// タイアップ情報
						if (!String.IsNullOrEmpty(ImportProgramCsvPath))
						{
							ImportProgramCsv(ImportProgramCsvPath, aContext);
						}

						// 楽曲情報
						if (!String.IsNullOrEmpty(ImportAnisonCsvPath))
						{
							ImportSongCsv(ImportAnisonCsvPath, aContext);
						}
						if (!String.IsNullOrEmpty(ImportSfCsvPath))
						{
							ImportSongCsv(ImportSfCsvPath, aContext);
						}
						if (!String.IsNullOrEmpty(ImportGameCsvPath))
						{
							ImportSongCsv(ImportGameCsvPath, aContext);
						}
					}

					Environment.LogWriter.ShowLogMessage(TraceEventType.Information, "完了");
				}
				catch (OperationCanceledException)
				{
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "anison.info CSV インポートを中止しました。");
				}
				catch (Exception oExcep)
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "anison.info CSV インポート時エラー：\n" + oExcep.Message);
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
				}
			});
		}

		// --------------------------------------------------------------------
		// ニコカラりすたーでエクスポートしたファイルをインポート
		// --------------------------------------------------------------------
		private Task ImportNicoKaraListerAsync()
		{
			return Task.Run(() =>
			{
				try
				{
					// 終了時に強制終了されないように設定
					Thread.CurrentThread.IsBackground = false;

					CheckImportNicoKaraLister();

					// 解凍
					String aTempFolder = YlCommon.TempFilePath() + "\\";
					Directory.CreateDirectory(aTempFolder);
					ZipFile.ExtractToDirectory(ImportNicoKaraListerPath, aTempFolder);
					String aExtractedFolderPath = aTempFolder + YlConstants.FILE_PREFIX_INFO + "\\";

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					using (SQLiteCommand aCmd = new SQLiteCommand(aMusicInfoDbInDisk.Connection))
					using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
					{
						// タイアップ情報
						String aProgramCsvPath = aExtractedFolderPath + YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM + Common.FILE_EXT_CSV;
						if (File.Exists(aProgramCsvPath))
						{
							ImportProgramCsv(aProgramCsvPath, aContext);
						}

						// 楽曲情報
						List<String> aSongCsvs = new List<String>();
						aSongCsvs.Add(YlConstants.FILE_BODY_ANISON_INFO_CSV_ANISON + Common.FILE_EXT_CSV);
						aSongCsvs.Add(YlConstants.FILE_BODY_ANISON_INFO_CSV_SF + Common.FILE_EXT_CSV);
						aSongCsvs.Add(YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME + Common.FILE_EXT_CSV);
						aSongCsvs.Add(YlConstants.FILE_BODY_ANISON_INFO_CSV_MISC + Common.FILE_EXT_CSV);
						foreach (String aSongCsv in aSongCsvs)
						{
							String aSongCsvPath = aExtractedFolderPath + aSongCsv;
							if (File.Exists(aSongCsvPath))
							{
								ImportSongCsv(aSongCsvPath, aContext);
							}
						}

						// タイアップエイリアス情報
						String aProgramAliasCsvPath = aExtractedFolderPath + YlConstants.FILE_BODY_ANISON_INFO_CSV_PROGRAM_ALIAS + Common.FILE_EXT_CSV;
						if (File.Exists(aProgramAliasCsvPath))
						{
							ImportProgramAliasCsv(aProgramAliasCsvPath, aContext);
						}

						// 楽曲エイリアス情報
						List<String> aSongAliasCsvs = new List<String>();
						aSongAliasCsvs.Add(YlConstants.FILE_BODY_ANISON_INFO_CSV_ANISON_ALIAS + Common.FILE_EXT_CSV);
						aSongAliasCsvs.Add(YlConstants.FILE_BODY_ANISON_INFO_CSV_SF_ALIAS + Common.FILE_EXT_CSV);
						aSongAliasCsvs.Add(YlConstants.FILE_BODY_ANISON_INFO_CSV_GAME_ALIAS + Common.FILE_EXT_CSV);
						aSongAliasCsvs.Add(YlConstants.FILE_BODY_ANISON_INFO_CSV_MISC_ALIAS + Common.FILE_EXT_CSV);
						foreach (String aSongAliasCsv in aSongAliasCsvs)
						{
							String aSongAliasCsvPath = aExtractedFolderPath + aSongAliasCsv;
							if (File.Exists(aSongAliasCsvPath))
							{
								ImportSongAliasCsv(aSongAliasCsvPath, aContext);
							}
						}
					}

					Environment.LogWriter.ShowLogMessage(TraceEventType.Information, "完了");
				}
				catch (OperationCanceledException)
				{
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ニコカラりすたーインポートを中止しました。");
				}
				catch (Exception oExcep)
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ニコカラりすたーインポート時エラー：\n" + oExcep.Message);
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
				}
			});
		}

		// --------------------------------------------------------------------
		// 番組情報別名 CSV からタイアップ別名テーブルへインポート
		// --------------------------------------------------------------------
		private void ImportProgramAliasCsv(String oProgramAliasCsvPath, DataContext oContext)
		{
			Description = oProgramAliasCsvPath + " からタイアップ別名情報をインポート中...";

			List<List<String>> aCsvContents = YlCommon.LoadCsv(oProgramAliasCsvPath, Environment, (Int32)ProgramAliasCsvColumns.__End__);
			aCsvContents.Sort(CompareProgramAliasCsv);
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TTieUp> aTableTieUp = oContext.GetTable<TTieUp>();
			Table<TTieUpAlias> aTableTieUpAlias = oContext.GetTable<TTieUpAlias>();
			Int32 aNumImports = 0;

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aCsvRecord = aCsvContents[i];

				// 番組 ID の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)ProgramAliasCsvColumns.NameOrId]))
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramAliasCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の番組 ID が指定されていないため無視します。", true);
					continue;
				}

				// 番組 ID が既にテーブル内にあるか確認
				TTieUp aTieUpRecord = aTableTieUp.SingleOrDefault(x => x.Id == aCsvRecord[(Int32)ProgramAliasCsvColumns.NameOrId]);
				if (aTieUpRecord == null && String.IsNullOrEmpty(aCsvRecord[(Int32)ProgramAliasCsvColumns.ForceId]))
				{
					aTieUpRecord = aTableTieUp.SingleOrDefault(x => x.Name == aCsvRecord[(Int32)ProgramAliasCsvColumns.NameOrId]);
				}
				if (aTieUpRecord == null)
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramAliasCsvColumns.LineIndex]) + 2).ToString("#,0")
							+ " 行目の番組 ID がタイアップマスターテーブルに存在しないため無視します。", true);
					continue;
				}

				// 別名が同名でないか確認
				aCsvRecord[(Int32)ProgramAliasCsvColumns.Alias] = YlCommon.NormalizeDbString(aCsvRecord[(Int32)ProgramAliasCsvColumns.Alias]);
				if (aCsvRecord[(Int32)ProgramAliasCsvColumns.Alias] == aTieUpRecord.Name)
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramAliasCsvColumns.LineIndex]) + 2).ToString("#,0")
							+ " 行目の別名が番組名と同一のため無視します。", true);
					continue;
				}

				// 情報が既に別名テーブル内にあるか確認
				TTieUpAlias aDbExistRecord = null;
				IQueryable<TTieUpAlias> aQueryResult =
						from x in aTableTieUpAlias
						where x.Alias == aCsvRecord[(Int32)ProgramAliasCsvColumns.Alias]
						select x;
				foreach (TTieUpAlias aTTieUpAlias in aQueryResult)
				{
					aDbExistRecord = aTTieUpAlias;
				}
				TTieUpAlias aDbNewRecord = TTieUpAliasFromAliasAndId((SQLiteConnection)oContext.Connection, aCsvRecord[(Int32)ProgramAliasCsvColumns.Alias], aTieUpRecord.Id);
				if (aDbExistRecord == null)
				{
					// 新規登録
					aTableTieUpAlias.InsertOnSubmit(aDbNewRecord);
					aNumImports++;
				}
				else if (YlCommon.IsRcAliasUpdated(aDbExistRecord, aDbNewRecord))
				{
					// 既存登録あるが更新が必要
#if DEBUGz
					if (aDbExistRecord.Id != aDbNewRecord.Id)
					{
						Debug.WriteLine("ImportProgramAliasCsv() id change: " + aDbExistRecord.Id + " → " + aDbNewRecord.Id);
					}
#endif
					aDbNewRecord.Id = aDbExistRecord.Id;
					aDbNewRecord.UpdateTime = aDbExistRecord.UpdateTime;
					Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
					aNumImports++;
				}

				if ((i + 1) % NUM_CSV_IMPORT_PROGRESS == 0)
				{
					Progress = "進捗：" + (i + 1).ToString("#,0") + " 個...";
					mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aCsvContents.Count.ToString("#,0") + " 個のデータのうち、重複を除く "
					+ aNumImports.ToString("#,0") + " 個をインポートしました。");
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// 番組情報 CSV からタイアップマスターテーブルへインポート
		// --------------------------------------------------------------------
		private void ImportProgramCsv(String oProgramCsvPath, DataContext oContext)
		{
			Description = oProgramCsvPath + " からタイアップ情報をインポート中...";

			UnzipIfNeeded(ref oProgramCsvPath);

			List<List<String>> aCsvContents = YlCommon.LoadCsv(oProgramCsvPath, Environment, (Int32)ProgramCsvColumns.__End__);
			aCsvContents.Sort(CompareProgramCsv);
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TTieUp> aTable = oContext.GetTable<TTieUp>();
			List<String> aPrevCsvRecord = null;
			Int32 aNumImports = 0;

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aCsvRecord = aCsvContents[i];

				// 番組 ID の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)ProgramCsvColumns.Id]))
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の番組 ID が指定されていないため無視します。", true);
					continue;
				}
				if (aPrevCsvRecord != null && aCsvRecord[(Int32)ProgramCsvColumns.Id] == aPrevCsvRecord[(Int32)ProgramCsvColumns.Id])
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							"CSV 内の番組 ID（" + aCsvRecord[(Int32)ProgramCsvColumns.Id] + "）が重複しているため無視します（"
							+ (aPrevCsvRecord[(Int32)ProgramCsvColumns.Name] == aCsvRecord[(Int32)ProgramCsvColumns.Name] ? aPrevCsvRecord[(Int32)ProgramCsvColumns.Name]
							: "登録済：" + aPrevCsvRecord[(Int32)ProgramCsvColumns.Name] + "、無視：" + aCsvRecord[(Int32)ProgramCsvColumns.Name])
							+ "）。", true);
					continue;
				}

				// 番組名の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)ProgramCsvColumns.Name]))
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の番組名が指定されていないため無視します。", true);
					continue;
				}
				aCsvRecord[(Int32)ProgramCsvColumns.Name] = YlCommon.NormalizeDbString(aCsvRecord[(Int32)ProgramCsvColumns.Name]);
				TTieUp aDupRecord = null;
				IQueryable<TTieUp> aQueryResult =
						from x in aTable
						where x.Name == aCsvRecord[(Int32)ProgramCsvColumns.Name] && x.Id != aCsvRecord[(Int32)ProgramCsvColumns.Id]
						select x;
				foreach (TTieUp aTTieUp in aQueryResult)
				{
					aDupRecord = aTTieUp;
				}
				if (aDupRecord != null
						|| mInsertingTieUps.ContainsKey(aCsvRecord[(Int32)ProgramCsvColumns.Name]) && mInsertingTieUps[aCsvRecord[(Int32)ProgramCsvColumns.Name]] != aCsvRecord[(Int32)ProgramCsvColumns.Id])
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)ProgramCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の番組名が既に登録されているため無視します。", true);
					continue;
				}

				// カテゴリー ID
				String aCategoryId = FindCategory(aCsvRecord[(Int32)ProgramCsvColumns.Category], oContext);

				// 番組 ID が既にテーブル内にあるか確認
				TTieUp aDbExistRecord = aTable.SingleOrDefault(x => x.Id == aCsvRecord[(Int32)ProgramCsvColumns.Id]);
				TTieUp aDbNewRecord = TTieUpFromProgramCsvRecord(aCsvRecord, aCategoryId);
				if (aDbExistRecord == null)
				{
					// 新規登録
					aTable.InsertOnSubmit(aDbNewRecord);
					mInsertingTieUps[aCsvRecord[(Int32)ProgramCsvColumns.Name]] = aCsvRecord[(Int32)ProgramCsvColumns.Id];
					aNumImports++;
				}
				else if (YlCommon.IsRcTieUpUpdated(aDbExistRecord, aDbNewRecord))
				{
					// 既存登録あるが更新が必要
					aDbNewRecord.UpdateTime = aDbExistRecord.UpdateTime;
					Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
					aNumImports++;
				}

				aPrevCsvRecord = aCsvRecord;

				if ((i + 1) % NUM_CSV_IMPORT_PROGRESS == 0)
				{
					Progress = "進捗：" + (i + 1).ToString("#,0") + " 個...";
					mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aCsvContents.Count.ToString("#,0") + " 個のデータのうち、重複を除く "
					+ aNumImports.ToString("#,0") + " 個をインポートしました。");
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// 楽曲情報別名 CSV から楽曲別名テーブルへインポート
		// --------------------------------------------------------------------
		private void ImportSongAliasCsv(String oSongAliasCsvPath, DataContext oContext)
		{
			Description = oSongAliasCsvPath + " から楽曲別名情報をインポート中...";

			List<List<String>> aCsvContents = YlCommon.LoadCsv(oSongAliasCsvPath, Environment, (Int32)SongAliasCsvColumns.__End__);
			aCsvContents.Sort(CompareSongAliasCsv);
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TSong> aTableSong = oContext.GetTable<TSong>();
			Table<TSongAlias> aTableSongAlias = oContext.GetTable<TSongAlias>();
			Int32 aNumImports = 0;

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aCsvRecord = aCsvContents[i];

				// 楽曲 ID の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)SongAliasCsvColumns.NameOrId]))
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongAliasCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の楽曲 ID が指定されていないため無視します。", true);
					continue;
				}

				// 楽曲 ID が既にテーブル内にあるか確認
				TSong aSongRecord = aTableSong.SingleOrDefault(x => x.Id == aCsvRecord[(Int32)SongAliasCsvColumns.NameOrId]);
				if (aSongRecord == null && String.IsNullOrEmpty(aCsvRecord[(Int32)SongAliasCsvColumns.ForceId]))
				{
					aSongRecord = aTableSong.SingleOrDefault(x => x.Name == aCsvRecord[(Int32)SongAliasCsvColumns.NameOrId]);
				}
				if (aSongRecord == null)
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongAliasCsvColumns.LineIndex]) + 2).ToString("#,0")
							+ " 行目の楽曲 ID が楽曲マスターテーブルに存在しないため無視します。", true);
					continue;
				}

				// 別名が同名でないか確認
				aCsvRecord[(Int32)SongAliasCsvColumns.Alias] = YlCommon.NormalizeDbString(aCsvRecord[(Int32)SongAliasCsvColumns.Alias]);
				if (aCsvRecord[(Int32)SongAliasCsvColumns.Alias] == aSongRecord.Name)
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongAliasCsvColumns.LineIndex]) + 2).ToString("#,0")
							+ " 行目の別名が楽曲名と同一のため無視します。", true);
					continue;
				}

				// 情報が既に別名テーブル内にあるか確認
				TSongAlias aDbExistRecord = null;
				IQueryable<TSongAlias> aQueryResult =
						from x in aTableSongAlias
						where x.Alias == aCsvRecord[(Int32)SongAliasCsvColumns.Alias]
						select x;
				foreach (TSongAlias aTSongAlias in aQueryResult)
				{
					aDbExistRecord = aTSongAlias;
				}
				TSongAlias aDbNewRecord = TSongAliasFromAliasAndId((SQLiteConnection)oContext.Connection, aCsvRecord[(Int32)SongAliasCsvColumns.Alias], aSongRecord.Id);
				if (aDbExistRecord == null)
				{
					// 新規登録
					aTableSongAlias.InsertOnSubmit(aDbNewRecord);
					aNumImports++;
				}
				else if (YlCommon.IsRcAliasUpdated(aDbExistRecord, aDbNewRecord))
				{
					// 既存登録あるが更新が必要
					aDbNewRecord.Id = aDbExistRecord.Id;
					aDbNewRecord.UpdateTime = aDbExistRecord.UpdateTime;
					Common.ShallowCopy(aDbNewRecord, aDbExistRecord);
					aNumImports++;
				}

				if ((i + 1) % NUM_CSV_IMPORT_PROGRESS == 0)
				{
					Progress = "進捗：" + (i + 1).ToString("#,0") + " 個...";
					mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aCsvContents.Count.ToString("#,0") + " 個のデータのうち、重複を除く "
					+ aNumImports.ToString("#,0") + " 個をインポートしました。");
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// 楽曲情報 CSV から楽曲マスターテーブルへインポート
		// --------------------------------------------------------------------
		private void ImportSongCsv(String oSongCsvPath, DataContext oContext)
		{
			Description = oSongCsvPath + " から楽曲情報をインポート中...";

			UnzipIfNeeded(ref oSongCsvPath);

			List<List<String>> aCsvContents = YlCommon.LoadCsv(oSongCsvPath, Environment, (Int32)SongCsvColumns.__End__);
			aCsvContents.Sort(CompareSongCsv);
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			Table<TSong> aTable = oContext.GetTable<TSong>();
			List<String> aPrevCsvRecord = null;
			Int32 aNumImports = 0;

			for (Int32 i = 0; i < aCsvContents.Count; i++)
			{
				List<String> aCsvRecord = aCsvContents[i];

				// 楽曲 ID の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)SongCsvColumns.Id]))
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の楽曲 ID が指定されていないため無視します。", true);
					continue;
				}
				if (aPrevCsvRecord != null && aCsvRecord[(Int32)SongCsvColumns.Id] == aPrevCsvRecord[(Int32)SongCsvColumns.Id])
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							"CSV 内の楽曲 ID（" + aCsvRecord[(Int32)SongCsvColumns.Id] + "）が重複しているため無視します（"
							+ (aPrevCsvRecord[(Int32)SongCsvColumns.Name] == aCsvRecord[(Int32)SongCsvColumns.Name] ? aPrevCsvRecord[(Int32)SongCsvColumns.Name]
							: "登録済：" + aPrevCsvRecord[(Int32)SongCsvColumns.Name] + "、無視：" + aCsvRecord[(Int32)SongCsvColumns.Name])
							+ "）。", true);
					continue;
				}

				// 楽曲名の解析
				if (String.IsNullOrEmpty(aCsvRecord[(Int32)SongCsvColumns.Name]))
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の楽曲名が指定されていないため無視します。", true);
					continue;
				}
				aCsvRecord[(Int32)SongCsvColumns.Name] = YlCommon.NormalizeDbString(aCsvRecord[(Int32)SongCsvColumns.Name]);
				TSong aDupRecord = null;
				IQueryable<TSong> aQueryResult =
						from x in aTable
						where x.Name == aCsvRecord[(Int32)SongCsvColumns.Name] && x.Id != aCsvRecord[(Int32)SongCsvColumns.Id]
						select x;
				foreach (TSong aSong in aQueryResult)
				{
					aDupRecord = aSong;
				}
				if (aDupRecord != null
						|| mInsertingSongs.ContainsKey(aCsvRecord[(Int32)SongCsvColumns.Name]) && mInsertingSongs[aCsvRecord[(Int32)SongCsvColumns.Name]] != aCsvRecord[(Int32)SongCsvColumns.Id])
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning,
							(Int32.Parse(aCsvRecord[(Int32)SongCsvColumns.LineIndex]) + 2).ToString("#,0") + " 行目の楽曲名が既に登録されているため無視します。", true);
					continue;
				}

				// 歌手の人物 ID
				List<String> aArtistIds = FindOrInsertPeople(aCsvRecord[(Int32)SongCsvColumns.Artist], oContext);

				// 楽曲 ID が既にテーブル内にあるか確認
				TSong aDbExistRecord = aTable.SingleOrDefault(x => x.Id == aCsvRecord[(Int32)SongCsvColumns.Id]);
				TSong aDbNewRecord = TSongFromSongCsvRecord(aCsvRecord);
				if (aDbExistRecord == null)
				{
					// 新規登録
					aTable.InsertOnSubmit(aDbNewRecord);
					mInsertingSongs[aCsvRecord[(Int32)SongCsvColumns.Name]] = aCsvRecord[(Int32)SongCsvColumns.Id];

					// 歌手紐付け
					YlCommon.RegisterSequence<TArtistSequence>(oContext, aDbNewRecord.Id, aArtistIds, true);
					aNumImports++;
				}
				else if (YlCommon.IsRcSongUpdated(aDbExistRecord, aDbNewRecord))
				{
					// 既存登録あるが更新が必要
					aDbNewRecord.UpdateTime = aDbExistRecord.UpdateTime;
					Common.ShallowCopy(aDbNewRecord, aDbExistRecord);

					// 歌手紐付け
					YlCommon.RegisterSequence<TArtistSequence>(oContext, aDbNewRecord.Id, aArtistIds, true);
					aNumImports++;
				}
				else
				{
					// 既存登録あり、更新が不要の場合は、歌手紐付けも更新しない（Ver 6.25 にて改善）
				}

				aPrevCsvRecord = aCsvRecord;

				if ((i + 1) % NUM_CSV_IMPORT_PROGRESS == 0)
				{
					Progress = "進捗：" + (i + 1).ToString("#,0") + " 個...";
					mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
			}
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();

			// 確定
			oContext.SubmitChanges();
			Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aCsvContents.Count.ToString("#,0") + " 個のデータのうち、重複を除く "
					+ aNumImports.ToString("#,0") + " 個をインポートしました。");
			mAbortCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

		// --------------------------------------------------------------------
		// 別名とオリジナル ID から TTieUpAlias レコードを作成
		// --------------------------------------------------------------------
		private TSongAlias TSongAliasFromAliasAndId(SQLiteConnection oConnection, String oAlias, String oOriginalId)
		{
			String aId = Environment.YukaListerSettings.PrepareLastId(oConnection, MusicInfoDbTables.TSongAlias);

			TSongAlias aSongAlias = new TSongAlias
			{
				// TBase
				Id = aId,
				Import = true,
				Invalid = false,
				UpdateTime = YlConstants.INVALID_MJD,
				Dirty = true,

				// TAlias
				Alias = oAlias,
				OriginalId = oOriginalId,
			};

			return aSongAlias;
		}

		// --------------------------------------------------------------------
		// anison.csv 等のレコードから TSong レコードを作成
		// oCsvRecord[(Int32)SongCsvColumns.Name] は正規化済みの前提
		// --------------------------------------------------------------------
		private TSong TSongFromSongCsvRecord(List<String> oCsvRecord)
		{
			Debug.Assert(oCsvRecord.Count >= (Int32)SongCsvColumns.__End__, "TSongFromSongCsvRecord() bad csv record count");

			TSong aSong = new TSong
			{
				// TBase
				Id = oCsvRecord[(Int32)SongCsvColumns.Id],
				Import = true,
				Invalid = false,
				UpdateTime = YlConstants.INVALID_MJD,
				Dirty = true,

				// TMaster
				Name = oCsvRecord[(Int32)SongCsvColumns.Name],
				Ruby = null,
				Keyword = null,

				// TSong
				ReleaseDate = YlConstants.INVALID_MJD,
				TieUpId = YlCommon.NullIfEmpty(oCsvRecord[(Int32)SongCsvColumns.ProgramId]),
				CategoryId = null,
				OpEd = YlCommon.NullIfEmpty(oCsvRecord[(Int32)SongCsvColumns.OpEd]),
			};

			return aSong;
		}

		// --------------------------------------------------------------------
		// 別名とオリジナル ID から TTieUpAlias レコードを作成
		// --------------------------------------------------------------------
		private TTieUpAlias TTieUpAliasFromAliasAndId(SQLiteConnection oConnection, String oAlias, String oOriginalId)
		{
			String aId = Environment.YukaListerSettings.PrepareLastId(oConnection, MusicInfoDbTables.TTieUpAlias);

			TTieUpAlias aTieUpAlias = new TTieUpAlias
			{
				// TBase
				Id = aId,
				Import = true,
				Invalid = false,
				UpdateTime = YlConstants.INVALID_MJD,
				Dirty = true,

				// TAlias
				Alias = oAlias,
				OriginalId = oOriginalId,
			};

			return aTieUpAlias;
		}

		// --------------------------------------------------------------------
		// program.csv レコードから TTieUp レコードを作成
		// oCsvRecord[(Int32)ProgramCsvColumns.Name] は正規化済みの前提
		// --------------------------------------------------------------------
		private TTieUp TTieUpFromProgramCsvRecord(List<String> oCsvRecord, String oCategoryId)
		{
			Debug.Assert(oCsvRecord.Count >= (Int32)ProgramCsvColumns.__End__, "TTieUpFromCsvRecord() bad csv record count");

			TTieUp aTieUp = new TTieUp
			{
				// TBase
				Id = oCsvRecord[(Int32)ProgramCsvColumns.Id],
				Import = true,
				Invalid = false,
				UpdateTime = YlConstants.INVALID_MJD,
				Dirty = true,

				// TMaster
				Name = oCsvRecord[(Int32)ProgramCsvColumns.Name],
				Ruby = YlCommon.NormalizeDbRuby(oCsvRecord[(Int32)ProgramCsvColumns.Ruby]),
				Keyword = YlCommon.NormalizeDbString(oCsvRecord[(Int32)ProgramCsvColumns.SubName]),

				// TTieUp
				CategoryId = oCategoryId,
				MakerId = null,
				AgeLimit = Common.StringToInt32(oCsvRecord[(Int32)ProgramCsvColumns.AgeLimit]),
				ReleaseDate = CsvDateStringToModifiedJulianDate(oCsvRecord[(Int32)ProgramCsvColumns.BeginDate]),
			};

			return aTieUp;
		}

		// --------------------------------------------------------------------
		// zip ファイルが指定された場合は解凍
		// --------------------------------------------------------------------
		private void UnzipIfNeeded(ref String oCsvPath)
		{
			if (Path.GetExtension(oCsvPath).ToLower() != Common.FILE_EXT_ZIP)
			{
				return;
			}

			// 解凍
			String aTempFolder = YlCommon.TempFilePath() + "\\";
			Directory.CreateDirectory(aTempFolder);
			ZipFile.ExtractToDirectory(oCsvPath, aTempFolder);
			String[] aFiles = Directory.GetFiles(aTempFolder, "*" + Common.FILE_EXT_CSV, SearchOption.AllDirectories);
			if (aFiles.Length > 0)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "UnzipIfNeeded() unzip: " + aFiles[0]);
				oCsvPath = aFiles[0];
			}
		}

	}
	// public class ImportWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___