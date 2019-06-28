// ============================================================================
// 
// 楽曲情報等編集ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ファイル名・フォルダー固定値から取得した楽曲情報等の編集を行うウィンドウ
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.Windows;

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Windows;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class EditMusicInfoWindowViewModel : ViewModel
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

		// ファイル名（パス無し）
		private String mFileName;
		public String FileName
		{
			get => mFileName;
			private set => RaisePropertyChangedIfSet(ref mFileName, value);
		}

		// ファイル名から取得した情報
		public Dictionary<String, String> DicByFile { get; set; }

		// タイアップ名が登録されているか
		public Boolean IsTieUpNameRegistered
		{
			get
			{
				if (Environment == null || DicByFile == null)
				{
					return false;
				}
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					return YlCommon.SelectMastersByName<TTieUp>(aMusicInfoDbInDisk.Connection, DicByFile[YlConstants.RULE_VAR_PROGRAM]).Count > 0;
				}
			}
		}

		// 楽曲名が登録されているか
		public Boolean IsSongNameRegistered
		{
			get
			{
				if (Environment == null || DicByFile == null)
				{
					return false;
				}
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					return YlCommon.SelectMastersByName<TSong>(aMusicInfoDbInDisk.Connection, DicByFile[YlConstants.RULE_VAR_TITLE]).Count > 0;
				}
			}
		}

		// タイアップ名を揃える
		private Boolean mUseTieUpAlias;
		public Boolean UseTieUpAlias
		{
			get => mUseTieUpAlias;
			set
			{
				if (value && IsTieUpNameRegistered)
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名・フォルダー固定値から取得したタイアップ名はデータベースに登録済みのため、タイアップ名を揃えるのは不要です。");
					return;
				}
				if (RaisePropertyChangedIfSet(ref mUseTieUpAlias, value))
				{
					ButtonSearchTieUpOriginClickedCommand.RaiseCanExecuteChanged();
					if (!mUseTieUpAlias)
					{
						TieUpOrigin = null;
					}
				}
			}
		}

		// 元のタイアップ名
		private String mTieUpOrigin;
		public String TieUpOrigin
		{
			get => mTieUpOrigin;
			set => RaisePropertyChangedIfSet(ref mTieUpOrigin, value);
		}

		// 楽曲名を揃える
		private Boolean mUseSongAlias;
		public Boolean UseSongAlias
		{
			get => mUseSongAlias;
			set
			{
				if (value && IsSongNameRegistered)
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "ファイル名・フォルダー固定値から取得した楽曲名はデータベースに登録済みのため、楽曲名を揃えるのは不要です。");
					return;
				}
				if (RaisePropertyChangedIfSet(ref mUseSongAlias, value))
				{
					ButtonSearchSongOriginClickedCommand.RaiseCanExecuteChanged();
					if (!mUseSongAlias)
					{
						SongOrigin = null;
					}
				}
			}
		}

		// 元の楽曲名
		private String mSongOrigin;
		public String SongOrigin
		{
			get => mSongOrigin;
			set => RaisePropertyChangedIfSet(ref mSongOrigin, value);
		}


		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// パス
		private String mPathExLen;
		public String PathExLen
		{
			get => mPathExLen;
			set
			{
				mPathExLen = value;
				mFileName = Path.GetFileName(mPathExLen);
			}
		}

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region タイアップ名検索ボタンの制御
		private ViewModelCommand mButtonSearchTieUpOriginClickedCommand;

		public ViewModelCommand ButtonSearchTieUpOriginClickedCommand
		{
			get
			{
				if (mButtonSearchTieUpOriginClickedCommand == null)
				{
					mButtonSearchTieUpOriginClickedCommand = new ViewModelCommand(ButtonSearchTieUpOriginClicked, CanButtonSearchTieUpOriginClicked);
				}
				return mButtonSearchTieUpOriginClickedCommand;
			}
		}

		public Boolean CanButtonSearchTieUpOriginClicked()
		{
			return UseTieUpAlias;
		}

		public void ButtonSearchTieUpOriginClicked()
		{
			try
			{
				using (SearchMusicInfoWindowViewModel aSearchMusicInfoWindowViewModel = new SearchMusicInfoWindowViewModel())
				{
					aSearchMusicInfoWindowViewModel.Environment = Environment;
					aSearchMusicInfoWindowViewModel.ItemName = "タイアップ名の正式名称";
					aSearchMusicInfoWindowViewModel.TableIndex = MusicInfoDbTables.TTieUp;
					aSearchMusicInfoWindowViewModel.SelectedKeyword = TieUpOrigin;
					Messenger.Raise(new TransitionMessage(aSearchMusicInfoWindowViewModel, "OpenSearchMusicInfoWindow"));

					mIsTieUpSearched = true;
					if (!String.IsNullOrEmpty(aSearchMusicInfoWindowViewModel.DecidedName))
					{
						TieUpOrigin = aSearchMusicInfoWindowViewModel.DecidedName;
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ名検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region タイアップ詳細編集ボタンの制御
		private ViewModelCommand mButtonEditTieUpClickedCommand;

		public ViewModelCommand ButtonEditTieUpClickedCommand
		{
			get
			{
				if (mButtonEditTieUpClickedCommand == null)
				{
					mButtonEditTieUpClickedCommand = new ViewModelCommand(ButtonEditTieUpClicked);
				}
				return mButtonEditTieUpClickedCommand;
			}
		}

		public void ButtonEditTieUpClicked()
		{
			try
			{
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					// ファイル名から取得したタイアップ名が未登録でかつ未検索は検索を促す
					if (YlCommon.SelectMastersByName<TTieUp>(aMusicInfoDbInDisk.Connection, DicByFile[YlConstants.RULE_VAR_PROGRAM]).Count == 0 && String.IsNullOrEmpty(TieUpOrigin))
					{
						if (!mIsTieUpSearched)
						{
							throw new Exception("タイアップの正式名称が選択されていないため新規タイアップ情報作成となりますが、その前に一度、目的のタイアップが未登録かどうか検索して下さい。");
						}

						if (MessageBox.Show("タイアップの正式名称が選択されていません。\n新規にタイアップ情報を作成しますか？\n"
								+ "（目的のタイアップが未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
								MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
						{
							return;
						}
					}
				}

				// 対象タイアップ名の選択
				String aTieUpName;
				if (!String.IsNullOrEmpty(TieUpOrigin))
				{
					aTieUpName = TieUpOrigin;
				}
				else
				{
					aTieUpName = DicByFile[YlConstants.RULE_VAR_PROGRAM];
				}

				// 情報準備
				List<TTieUp> aTieUps;
				List<TCategory> aCategories;
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					aTieUps = YlCommon.SelectMastersByName<TTieUp>(aMusicInfoDbInDisk.Connection, aTieUpName);
					aCategories = YlCommon.SelectMastersByName<TCategory>(aMusicInfoDbInDisk.Connection, DicByFile[YlConstants.RULE_VAR_CATEGORY]);
				}

				// 新規作成用の追加
				TTieUp aNewTieUp = new TTieUp
				{
					// IRcBase
					Id = null,
					Import = false,
					Invalid = false,
					UpdateTime = YlConstants.INVALID_MJD,
					Dirty = true,

					// IRcMaster
					Name = aTieUpName,
					Ruby = null,
					Keyword = null,

					// TTieUp
					CategoryId = aCategories.Count > 0 ? aCategories[0].Id : null,
					MakerId = null,
					AgeLimit = Common.StringToInt32(DicByFile[YlConstants.RULE_VAR_AGE_LIMIT]),
					ReleaseDate = YlConstants.INVALID_MJD,
				};
				aTieUps.Insert(0, aNewTieUp);

				using (EditTieUpWindowViewModel aEditTieUpWindowViewModel = new EditTieUpWindowViewModel())
				{
					aEditTieUpWindowViewModel.Environment = Environment;
					aEditTieUpWindowViewModel.SetMasters(aTieUps);
					if (aTieUps.Count > 1)
					{
						aEditTieUpWindowViewModel.DefaultId = aTieUps[1].Id;
					}
					Messenger.Raise(new TransitionMessage(aEditTieUpWindowViewModel, "OpenEditTieUpWindow"));

					if (String.IsNullOrEmpty(aEditTieUpWindowViewModel.OkSelectedId))
					{
						return;
					}

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					{
						TTieUp aTieUp = YlCommon.SelectMasterById<TTieUp>(aMusicInfoDbInDisk.Connection, aEditTieUpWindowViewModel.OkSelectedId);
						if (aTieUp != null)
						{
							if (String.IsNullOrEmpty(DicByFile[YlConstants.RULE_VAR_PROGRAM]) || aTieUp.Name == DicByFile[YlConstants.RULE_VAR_PROGRAM])
							{
								UseTieUpAlias = false;
							}
							else
							{
								UseTieUpAlias = true;
								TieUpOrigin = aTieUp.Name;
							}
						}
						RaisePropertyChanged(nameof(IsTieUpNameRegistered));
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 楽曲名検索ボタンの制御
		private ViewModelCommand mButtonSearchSongOriginClickedCommand;

		public ViewModelCommand ButtonSearchSongOriginClickedCommand
		{
			get
			{
				if (mButtonSearchSongOriginClickedCommand == null)
				{
					mButtonSearchSongOriginClickedCommand = new ViewModelCommand(ButtonSearchSongOriginClicked, CanButtonSearchSongOriginClicked);
				}
				return mButtonSearchSongOriginClickedCommand;
			}
		}

		public Boolean CanButtonSearchSongOriginClicked()
		{
			return UseSongAlias;
		}

		public void ButtonSearchSongOriginClicked()
		{
			try
			{
				using (SearchMusicInfoWindowViewModel aSearchMusicInfoWindowViewModel = new SearchMusicInfoWindowViewModel())
				{
					aSearchMusicInfoWindowViewModel.Environment = Environment;
					aSearchMusicInfoWindowViewModel.ItemName = "楽曲名の正式名称";
					aSearchMusicInfoWindowViewModel.TableIndex = MusicInfoDbTables.TSong;
					aSearchMusicInfoWindowViewModel.SelectedKeyword = SongOrigin;
					Messenger.Raise(new TransitionMessage(aSearchMusicInfoWindowViewModel, "OpenSearchMusicInfoWindow"));

					mIsSongSearched = true;
					if (!String.IsNullOrEmpty(aSearchMusicInfoWindowViewModel.DecidedName))
					{
						SongOrigin = aSearchMusicInfoWindowViewModel.DecidedName;
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ名検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 楽曲詳細編集ボタンの制御
		private ViewModelCommand mButtonEditSongClickedCommand;

		public ViewModelCommand ButtonEditSongClickedCommand
		{
			get
			{
				if (mButtonEditSongClickedCommand == null)
				{
					mButtonEditSongClickedCommand = new ViewModelCommand(ButtonEditSongClicked);
				}
				return mButtonEditSongClickedCommand;
			}
		}

		public void ButtonEditSongClicked()
		{
			try
			{
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					// ファイル名から取得した楽曲名が未登録でかつ未検索は検索を促す
					if (YlCommon.SelectMastersByName<TSong>(aMusicInfoDbInDisk.Connection, DicByFile[YlConstants.RULE_VAR_TITLE]).Count == 0 && String.IsNullOrEmpty(SongOrigin))
					{
						if (!mIsSongSearched)
						{
							throw new Exception("楽曲の正式名称が選択されていないため新規楽曲情報作成となりますが、その前に一度、目的の楽曲が未登録かどうか検索して下さい。");
						}

						if (MessageBox.Show("楽曲の正式名称が選択されていません。\n新規に楽曲情報を作成しますか？\n"
								+ "（目的の楽曲が未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
								MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
						{
							return;
						}
					}
				}

				// 対象楽曲名の選択
				String aSongName;
				if (!String.IsNullOrEmpty(SongOrigin))
				{
					aSongName = SongOrigin;
				}
				else
				{
					aSongName = DicByFile[YlConstants.RULE_VAR_TITLE];
				}

				// タイアップ名の選択（null もありえる）
				String aTieUpName;
				if (!String.IsNullOrEmpty(TieUpOrigin))
				{
					aTieUpName = TieUpOrigin;
				}
				else
				{
					aTieUpName = DicByFile[YlConstants.RULE_VAR_PROGRAM];
				}

				// 情報準備
				List<TSong> aSongs;
				List<TTieUp> aTieUps;
				List<TCategory> aCategories;
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					aSongs = YlCommon.SelectMastersByName<TSong>(aMusicInfoDbInDisk.Connection, aSongName);
					aTieUps = YlCommon.SelectMastersByName<TTieUp>(aMusicInfoDbInDisk.Connection, aTieUpName);
					aCategories = YlCommon.SelectMastersByName<TCategory>(aMusicInfoDbInDisk.Connection, DicByFile[YlConstants.RULE_VAR_CATEGORY]);
				}

				// 新規作成用の追加
				TSong aNewSong = new TSong
				{
					// IRcBase
					Id = null,
					Import = false,
					Invalid = false,
					UpdateTime = YlConstants.INVALID_MJD,
					Dirty = true,

					// IRcMaster
					Name = aSongName,
					Ruby = DicByFile[YlConstants.RULE_VAR_TITLE_RUBY],
					Keyword = null,

					// TSong
					ReleaseDate = YlConstants.INVALID_MJD,
					TieUpId = aTieUps.Count > 0 ? aTieUps[0].Id : null,
					CategoryId = aTieUps.Count == 0 && aCategories.Count > 0 ? aCategories[0].Id : null,
					OpEd = DicByFile[YlConstants.RULE_VAR_OP_ED],
				};
				aSongs.Insert(0, aNewSong);

				using (EditSongWindowViewModel aEditSongWindowViewModel = new EditSongWindowViewModel())
				{
					aEditSongWindowViewModel.Environment = Environment;
					aEditSongWindowViewModel.SetMasters(aSongs);

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					{
						// デフォルト ID の指定
						if (aSongs.Count == 1)
						{
							// 新規作成のみの場合は指定しない
						}
						else if (aSongs.Count == 2 && String.IsNullOrEmpty(aTieUpName))
						{
							// 既存楽曲が 1 つのみの場合で、タイアップが指定されていない場合は、既存楽曲のタイアップに関わらずデフォルトに指定する
							aEditSongWindowViewModel.DefaultId = aSongs[1].Id;
						}
						else
						{
							// 既存楽曲が 1 つ以上の場合は、タイアップ名が一致するものがあれば優先し、そうでなければ新規をデフォルトにする
							for (Int32 i = 1; i < aSongs.Count; i++)
							{
								TTieUp aTieUpOfSong = YlCommon.SelectMasterById<TTieUp>(aMusicInfoDbInDisk.Connection, aSongs[i].TieUpId);
								if (aTieUpOfSong == null && String.IsNullOrEmpty(aTieUpName) || aTieUpOfSong != null && aTieUpOfSong.Name == aTieUpName)
								{
									aEditSongWindowViewModel.DefaultId = aSongs[i].Id;
									break;
								}
							}
						}
					}

					Messenger.Raise(new TransitionMessage(aEditSongWindowViewModel, "OpenEditSongWindow"));
					if (String.IsNullOrEmpty(aEditSongWindowViewModel.OkSelectedId))
					{
						return;
					}

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					{
						TSong aSong = YlCommon.SelectMasterById<TSong>(aMusicInfoDbInDisk.Connection, aEditSongWindowViewModel.OkSelectedId);
						if (aSong != null)
						{
							if (aSong.Name == DicByFile[YlConstants.RULE_VAR_TITLE])
							{
								UseSongAlias = false;
							}
							else
							{
								UseSongAlias = true;
								SongOrigin = aSong.Name;
							}
						}
						RaisePropertyChanged(nameof(IsSongNameRegistered));
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "楽曲詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region ヘルプリンクの制御
		public ListenerCommand<String> HelpClickedCommand
		{
			get => Environment?.HelpClickedCommand;
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
				String aSongOriginalId;
				String aTieUpOriginalId;
				CheckInput(out aSongOriginalId, out aTieUpOriginalId);
				Save(aSongOriginalId, aTieUpOriginalId);
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
				Title = "名称の編集";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif
				// 別名解決
				ApplySongAlias();
				ApplyTieUpAlias();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "楽曲情報等編集ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 楽曲を検索したかどうか
		private Boolean mIsSongSearched = false;

		// タイアップを検索したかどうか
		private Boolean mIsTieUpSearched = false;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 適用可能な楽曲名の別名を検索してコンポーネントに反映
		// --------------------------------------------------------------------
		private void ApplySongAlias()
		{
			if (String.IsNullOrEmpty(DicByFile[YlConstants.RULE_VAR_TITLE]))
			{
				return;
			}

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			{
				List<TSongAlias> aSongAliases = YlCommon.SelectAliasesByAlias<TSongAlias>(aMusicInfoDbInDisk.Connection, DicByFile[YlConstants.RULE_VAR_TITLE]);
				if (aSongAliases.Count > 0)
				{
					TSong aSong = YlCommon.SelectMasterById<TSong>(aMusicInfoDbInDisk.Connection, aSongAliases[0].OriginalId);
					if (aSong != null)
					{
						UseSongAlias = true;
						SongOrigin = aSong.Name;
						return;
					}
				}

				if (YlCommon.SelectMastersByName<TSong>(aMusicInfoDbInDisk.Connection, DicByFile[YlConstants.RULE_VAR_TITLE]).Count == 0)
				{
					UseSongAlias = true;
					SongOrigin = null;
				}
			}
		}

		// --------------------------------------------------------------------
		// 適用可能なタイアップ名の別名を検索してコンポーネントに反映
		// --------------------------------------------------------------------
		private void ApplyTieUpAlias()
		{
			if (String.IsNullOrEmpty(DicByFile[YlConstants.RULE_VAR_PROGRAM]))
			{
				return;
			}

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			{
				List<TTieUpAlias> aTieUpAliases = YlCommon.SelectAliasesByAlias<TTieUpAlias>(aMusicInfoDbInDisk.Connection, DicByFile[YlConstants.RULE_VAR_PROGRAM]);
				if (aTieUpAliases.Count > 0)
				{
					TTieUp aTieUp = YlCommon.SelectMasterById<TTieUp>(aMusicInfoDbInDisk.Connection, aTieUpAliases[0].OriginalId);
					if (aTieUp != null)
					{
						UseTieUpAlias = true;
						TieUpOrigin = aTieUp.Name;
						return;
					}
				}

				if (YlCommon.SelectMastersByName<TTieUp>(aMusicInfoDbInDisk.Connection, DicByFile[YlConstants.RULE_VAR_PROGRAM]).Count == 0)
				{
					UseTieUpAlias = true;
					TieUpOrigin = null;
				}
			}
		}

		// --------------------------------------------------------------------
		// 入力値の確認（別名に関するもののみ）
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckInput(out String oSongOriginalId, out String oTieUpOriginalId)
		{
			oSongOriginalId = null;
			oTieUpOriginalId = null;

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				// 楽曲別名
				if (UseSongAlias)
				{
					if (String.IsNullOrEmpty(SongOrigin))
					{
						throw new Exception("楽曲名の正式名称を検索して指定して下さい。");
					}
					if (SongOrigin == DicByFile[YlConstants.RULE_VAR_TITLE])
					{
						throw new Exception("ファイル名・フォルダー固定値から取得した楽曲名と正式名称が同じです。\n"
								+ "楽曲名を揃えるのが不要の場合は、「楽曲名を揃える」のチェックを外して下さい。");
					}
					List<TSong> aSongs = YlCommon.SelectMastersByName<TSong>(aContext, SongOrigin);
					if (aSongs.Count == 0)
					{
						throw new Exception("楽曲名の正式名称が正しく検索されていません。");
					}
					oSongOriginalId = aSongs[0].Id;
				}

				// タイアップ別名
				if (UseTieUpAlias)
				{
					if (String.IsNullOrEmpty(TieUpOrigin))
					{
						throw new Exception("タイアップ名の正式名称を検索して指定して下さい。");
					}
					if (TieUpOrigin == DicByFile[YlConstants.RULE_VAR_PROGRAM])
					{
						throw new Exception("ファイル名・フォルダー固定値から取得したタイアップ名と正式名称が同じです。\n"
								+ "タイアップ名を揃えるのが不要の場合は、「タイアップ名を揃える」のチェックを外して下さい。");
					}
					List<TTieUp> aTieUps = YlCommon.SelectMastersByName<TTieUp>(aContext, TieUpOrigin);
					if (aTieUps.Count == 0)
					{
						throw new Exception("タイアップ名の正式名称が正しく検索されていません。");
					}
					oTieUpOriginalId = aTieUps[0].Id;
				}
			}
		}

		// --------------------------------------------------------------------
		// 別名を保存
		// --------------------------------------------------------------------
		private void Save(String oSongOriginalId, String oTieUpOriginalId)
		{
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				// 楽曲別名
				Table<TSongAlias> aTableSongAlias = aContext.GetTable<TSongAlias>();
				if (UseSongAlias)
				{
					List<TSongAlias> aSongAliases = YlCommon.SelectAliasesByAlias<TSongAlias>(aContext, DicByFile[YlConstants.RULE_VAR_TITLE], true);
					TSongAlias aNewSongAlias = new TSongAlias
					{
						// TBase
						Id = null,
						Import = false,
						Invalid = false,
						UpdateTime = YlConstants.INVALID_MJD,
						Dirty = true,

						// TAlias
						Alias = DicByFile[YlConstants.RULE_VAR_TITLE],
						OriginalId = oSongOriginalId,
					};

					if (aSongAliases.Count == 0)
					{
						// 新規登録
						YlCommon.InputIdPrefixIfNeededWithInvoke(this, Environment);
						aNewSongAlias.Id = Environment.YukaListerSettings.PrepareLastId(aMusicInfoDbInDisk.Connection, MusicInfoDbTables.TSongAlias);
						aTableSongAlias.InsertOnSubmit(aNewSongAlias);
						Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲別名テーブル新規登録：" + aNewSongAlias.Id + " / " + aNewSongAlias.Alias);
					}
					else if (YlCommon.IsRcAliasUpdated(aSongAliases[0], aNewSongAlias))
					{
						// 更新（既存のレコードが無効化されている場合は有効化も行う）
						aNewSongAlias.Id = aSongAliases[0].Id;
						aNewSongAlias.UpdateTime = aSongAliases[0].UpdateTime;
						Common.ShallowCopy(aNewSongAlias, aSongAliases[0]);
						Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲別名テーブル更新：" + aNewSongAlias.Id + " / " + aNewSongAlias.Alias);
					}
				}
				else
				{
					List<TSongAlias> aSongAliases = YlCommon.SelectAliasesByAlias<TSongAlias>(aContext, DicByFile[YlConstants.RULE_VAR_TITLE], false);
					if (aSongAliases.Count > 0)
					{
						// 無効化
						aSongAliases[0].Invalid = true;
						Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲別名テーブル無効化：" + aSongAliases[0].Id + " / " + aSongAliases[0].Alias);
					}
				}

				// タイアップ別名
				Table<TTieUpAlias> aTableTieUpAlias = aContext.GetTable<TTieUpAlias>();
				if (UseTieUpAlias)
				{
					List<TTieUpAlias> aTieUpAliases = YlCommon.SelectAliasesByAlias<TTieUpAlias>(aContext, DicByFile[YlConstants.RULE_VAR_PROGRAM], true);
					TTieUpAlias aNewTieUpAlias = new TTieUpAlias
					{
						// TBase
						Id = null,
						Import = false,
						Invalid = false,
						UpdateTime = YlConstants.INVALID_MJD,
						Dirty = true,

						// TAlias
						Alias = DicByFile[YlConstants.RULE_VAR_PROGRAM],
						OriginalId = oTieUpOriginalId,
					};

					if (aTieUpAliases.Count == 0)
					{
						// 新規登録
						YlCommon.InputIdPrefixIfNeededWithInvoke(this, Environment);
						aNewTieUpAlias.Id = Environment.YukaListerSettings.PrepareLastId(aMusicInfoDbInDisk.Connection, MusicInfoDbTables.TTieUpAlias);
						aTableTieUpAlias.InsertOnSubmit(aNewTieUpAlias);
						Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップ別名テーブル新規登録：" + aNewTieUpAlias.Id + " / " + aNewTieUpAlias.Alias);
					}
					else if (YlCommon.IsRcAliasUpdated(aTieUpAliases[0], aNewTieUpAlias))
					{
						// 更新（既存のレコードが無効化されている場合は有効化も行う）
						aNewTieUpAlias.Id = aTieUpAliases[0].Id;
						aNewTieUpAlias.UpdateTime = aTieUpAliases[0].UpdateTime;
						Common.ShallowCopy(aNewTieUpAlias, aTieUpAliases[0]);
						Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップ別名テーブル更新：" + aNewTieUpAlias.Id + " / " + aNewTieUpAlias.Alias);
					}
				}
				else
				{
					List<TTieUpAlias> aTieUpAliases = YlCommon.SelectAliasesByAlias<TTieUpAlias>(aContext, DicByFile[YlConstants.RULE_VAR_PROGRAM], false);
					if (aTieUpAliases.Count > 0)
					{
						// 無効化
						aTieUpAliases[0].Invalid = true;
						Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップ別名テーブル無効化：" + aTieUpAliases[0].Id + " / " + aTieUpAliases[0].Alias);
					}
				}

				aContext.SubmitChanges();
			}
		}

	}
	// public class EditMusicInfoWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
