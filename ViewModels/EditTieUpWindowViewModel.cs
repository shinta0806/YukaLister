// ============================================================================
// 
// タイアップ詳細情報編集ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet.Commands;
using Livet.Messaging;

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class EditTieUpWindowViewModel : EditCategorizableWindowViewModel
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

		// 年齢制限選択ボタンのコンテキストメニュー
		public List<MenuItem> ContextMenuButtonSelectAgeLimitItems { get; set; }

		// 年齢制限
		private String mAgeLimit;
		public String AgeLimit
		{
			get => mAgeLimit;
			set => RaisePropertyChangedIfSet(ref mAgeLimit, value);
		}

		// 制作会社あり
		private Boolean mHasMaker;
		public Boolean HasMaker
		{
			get => mHasMaker;
			set
			{
				if (RaisePropertyChangedIfSet(ref mHasMaker, value))
				{
					ButtonSearchMakerClickedCommand.RaiseCanExecuteChanged();
					ButtonEditMakerClickedCommand.RaiseCanExecuteChanged();
					if (!mHasMaker)
					{
						MakerId = null;
						MakerName = null;
					}
				}
			}
		}

		// 制作会社名
		private String mMakerName;
		public String MakerName
		{
			get => mMakerName;
			set => RaisePropertyChangedIfSet(ref mMakerName, value);
		}

		// タイアップグループあり
		private Boolean mHasTieUpGroup;
		public Boolean HasTieUpGroup
		{
			get => mHasTieUpGroup;
			set
			{
				if (RaisePropertyChangedIfSet(ref mHasTieUpGroup, value))
				{
					ButtonSearchTieUpGroupClickedCommand.RaiseCanExecuteChanged();
					ButtonEditTieUpGroupClickedCommand.RaiseCanExecuteChanged();
					if (!mHasTieUpGroup)
					{
						TieUpGroupId = null;
						TieUpGroupName = null;
					}
				}
			}
		}

		// タイアップグループ名
		private String mTieUpGroupName;
		public String TieUpGroupName
		{
			get => mTieUpGroupName;
			set => RaisePropertyChangedIfSet(ref mTieUpGroupName, value);
		}

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 制作会社 ID
		public String MakerId { get; set; }

		// タイアップグループ ID
		public String TieUpGroupId { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region 制作会社検索ボタンの制御
		private ViewModelCommand mButtonSearchMakerClickedCommand;

		public ViewModelCommand ButtonSearchMakerClickedCommand
		{
			get
			{
				if (mButtonSearchMakerClickedCommand == null)
				{
					mButtonSearchMakerClickedCommand = new ViewModelCommand(ButtonSearchMakerClicked, CanButtonSearchMakerClicked);
				}
				return mButtonSearchMakerClickedCommand;
			}
		}

		public Boolean CanButtonSearchMakerClicked()
		{
			return HasMaker;
		}

		public void ButtonSearchMakerClicked()
		{
			try
			{
				using (SearchMusicInfoWindowViewModel aSearchMusicInfoWindowViewModel = new SearchMusicInfoWindowViewModel())
				{
					aSearchMusicInfoWindowViewModel.Environment = Environment;
					aSearchMusicInfoWindowViewModel.ItemName = "制作会社";
					aSearchMusicInfoWindowViewModel.TableIndex = MusicInfoDbTables.TMaker;
					aSearchMusicInfoWindowViewModel.SelectedKeyword = MakerName;
					Messenger.Raise(new TransitionMessage(aSearchMusicInfoWindowViewModel, "OpenSearchMusicInfoWindow"));
					mIsMakerSearched = true;
					if (String.IsNullOrEmpty(aSearchMusicInfoWindowViewModel.DecidedName))
					{
						return;
					}

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					{
						List<TMaker> aMakers = YlCommon.SelectMastersByName<TMaker>(aMusicInfoDbInDisk.Connection, aSearchMusicInfoWindowViewModel.DecidedName);
						if (aMakers.Count > 0)
						{
							MakerId = aMakers[0].Id;
							MakerName = aMakers[0].Name;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "制作会社検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 制作会社詳細編集ボタンの制御
		private ViewModelCommand mButtonEditMakerClickedCommand;

		public ViewModelCommand ButtonEditMakerClickedCommand
		{
			get
			{
				if (mButtonEditMakerClickedCommand == null)
				{
					mButtonEditMakerClickedCommand = new ViewModelCommand(ButtonEditMakerClicked, CanButtonEditMakerClicked);
				}
				return mButtonEditMakerClickedCommand;
			}
		}

		public Boolean CanButtonEditMakerClicked()
		{
			return HasMaker;
		}

		public void ButtonEditMakerClicked()
		{
			try
			{
				if (String.IsNullOrEmpty(MakerName))
				{
					if (!mIsMakerSearched)
					{
						throw new Exception("制作会社が選択されていないため新規制作会社情報作成となりますが、その前に一度、目的の制作会社が未登録かどうか検索して下さい。");
					}

					if (MessageBox.Show("制作会社が選択されていません。\n新規に制作会社情報を作成しますか？\n"
							+ "（目的の制作会社が未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
							MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
					{
						return;
					}
				}

				// 既存レコードを用意
				List<TMaker> aMasters;
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					aMasters = YlCommon.SelectMastersByName<TMaker>(aMusicInfoDbInDisk.Connection, MakerName);
				}

				// 新規作成用を追加
				TMaker aNewRecord = new TMaker
				{
					// IRcBase
					Id = null,
					Import = false,
					Invalid = false,
					UpdateTime = YlConstants.INVALID_MJD,
					Dirty = true,

					// IRcMaster
					Name = MakerName,
					Ruby = null,
					Keyword = null,
				};
				aMasters.Insert(0, aNewRecord);

				using (EditMakerWindowViewModel aEditMakerWindowViewModel = new EditMakerWindowViewModel())
				{
					aEditMakerWindowViewModel.Environment = Environment;
					aEditMakerWindowViewModel.SetMasters(aMasters);
					aEditMakerWindowViewModel.DefaultId = MakerId;
					Messenger.Raise(new TransitionMessage(aEditMakerWindowViewModel, "OpenEditMakerWindow"));

					if (String.IsNullOrEmpty(aEditMakerWindowViewModel.OkSelectedId))
					{
						return;
					}

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					{
						TMaker aMaster = YlCommon.SelectMasterById<TMaker>(aMusicInfoDbInDisk.Connection, aEditMakerWindowViewModel.OkSelectedId);
						if (aMaster != null)
						{
							MakerId = aMaster.Id;
							MakerName = aMaster.Name;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "制作会社詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region タイアップグループ検索ボタンの制御
		private ViewModelCommand mButtonSearchTieUpGroupClickedCommand;

		public ViewModelCommand ButtonSearchTieUpGroupClickedCommand
		{
			get
			{
				if (mButtonSearchTieUpGroupClickedCommand == null)
				{
					mButtonSearchTieUpGroupClickedCommand = new ViewModelCommand(ButtonSearchTieUpGroupClicked, CanmButtonSearchTieUpGroupClicked);
				}
				return mButtonSearchTieUpGroupClickedCommand;
			}
		}

		public Boolean CanmButtonSearchTieUpGroupClicked()
		{
			return HasTieUpGroup;
		}

		public void ButtonSearchTieUpGroupClicked()
		{
			try
			{
				using (SearchMusicInfoWindowViewModel aSearchMusicInfoWindowViewModel = new SearchMusicInfoWindowViewModel())
				{
					aSearchMusicInfoWindowViewModel.Environment = Environment;
					aSearchMusicInfoWindowViewModel.ItemName = "シリーズ";
					aSearchMusicInfoWindowViewModel.TableIndex = MusicInfoDbTables.TTieUpGroup;
					aSearchMusicInfoWindowViewModel.SelectedKeyword = TieUpGroupName;
					Messenger.Raise(new TransitionMessage(aSearchMusicInfoWindowViewModel, "OpenSearchMusicInfoWindow"));
					mIsTieUpGroupSearched = true;
					if (String.IsNullOrEmpty(aSearchMusicInfoWindowViewModel.DecidedName))
					{
						return;
					}

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					{
						List<TTieUpGroup> aTieUpGroups = YlCommon.SelectMastersByName<TTieUpGroup>(aMusicInfoDbInDisk.Connection, aSearchMusicInfoWindowViewModel.DecidedName);
						if (aTieUpGroups.Count > 0)
						{
							TieUpGroupId = aTieUpGroups[0].Id;
							TieUpGroupName = aTieUpGroups[0].Name;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "シリーズ検索ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region タイアップグループ詳細編集ボタンの制御
		private ViewModelCommand mButtonEditTieUpGroupClickedCommand;

		public ViewModelCommand ButtonEditTieUpGroupClickedCommand
		{
			get
			{
				if (mButtonEditTieUpGroupClickedCommand == null)
				{
					mButtonEditTieUpGroupClickedCommand = new ViewModelCommand(ButtonEditTieUpGroupClicked, CanButtonEditTieUpGroupClicked);
				}
				return mButtonEditTieUpGroupClickedCommand;
			}
		}

		public Boolean CanButtonEditTieUpGroupClicked()
		{
			return HasTieUpGroup;
		}

		public void ButtonEditTieUpGroupClicked()
		{
			try
			{
				if (String.IsNullOrEmpty(TieUpGroupName))
				{
					if (!mIsTieUpGroupSearched)
					{
						throw new Exception("シリーズが選択されていないため新規シリーズ情報作成となりますが、その前に一度、目的のシリーズが未登録かどうか検索して下さい。");
					}

					if (MessageBox.Show("シリーズが選択されていません。\n新規にシリーズ情報を作成しますか？\n"
							+ "（目的のシリーズが未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
							MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
					{
						return;
					}
				}

				// 既存レコードを用意
				List<TTieUpGroup> aMasters;
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					aMasters = YlCommon.SelectMastersByName<TTieUpGroup>(aMusicInfoDbInDisk.Connection, TieUpGroupName);
				}

				// 新規作成用を追加
				TTieUpGroup aNewRecord = new TTieUpGroup
				{
					// IRcBase
					Id = null,
					Import = false,
					Invalid = false,
					UpdateTime = YlConstants.INVALID_MJD,
					Dirty = true,

					// IRcMaster
					Name = TieUpGroupName,
					Ruby = null,
					Keyword = null,
				};
				aMasters.Insert(0, aNewRecord);

				using (EditTieUpGroupWindowViewModel aEditTieUpGroupWindowViewModel = new EditTieUpGroupWindowViewModel())
				{
					aEditTieUpGroupWindowViewModel.Environment = Environment;
					aEditTieUpGroupWindowViewModel.SetMasters(aMasters);
					aEditTieUpGroupWindowViewModel.DefaultId = TieUpGroupId;
					Messenger.Raise(new TransitionMessage(aEditTieUpGroupWindowViewModel, "OpenEditTieUpGroupWindow"));

					if (String.IsNullOrEmpty(aEditTieUpGroupWindowViewModel.OkSelectedId))
					{
						return;
					}

					using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
					{
						TTieUpGroup aMaster = YlCommon.SelectMasterById<TTieUpGroup>(aMusicInfoDbInDisk.Connection, aEditTieUpGroupWindowViewModel.OkSelectedId);
						if (aMaster != null)
						{
							TieUpGroupId = aMaster.Id;
							TieUpGroupName = aMaster.Name;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "シリーズ詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
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
		public override void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				// 基底クラス初期化前の初期化
				mCaption = "タイアップ";

				// 基底クラス初期化
				base.Initialize();

				// タイトルバー
				Title = "タイアップ詳細情報の編集";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				// 年齢制限選択ボタンのコンテキストメニュー
				ContextMenuButtonSelectAgeLimitItems = new List<MenuItem>();
				YlCommon.AddContextMenuItem(ContextMenuButtonSelectAgeLimitItems, "全年齢対象（CERO A 相当）", ContextMenuButtonSelectAgeLimitItem_Click);
				YlCommon.AddContextMenuItem(ContextMenuButtonSelectAgeLimitItems, YlConstants.AGE_LIMIT_CERO_B.ToString() + " 才以上対象（CERO B 相当）", ContextMenuButtonSelectAgeLimitItem_Click);
				YlCommon.AddContextMenuItem(ContextMenuButtonSelectAgeLimitItems, YlConstants.AGE_LIMIT_CERO_C.ToString() + " 才以上対象（CERO C 相当）", ContextMenuButtonSelectAgeLimitItem_Click);
				YlCommon.AddContextMenuItem(ContextMenuButtonSelectAgeLimitItems, YlConstants.AGE_LIMIT_CERO_D.ToString() + " 才以上対象（CERO D 相当）", ContextMenuButtonSelectAgeLimitItem_Click);
				YlCommon.AddContextMenuItem(ContextMenuButtonSelectAgeLimitItems, YlConstants.AGE_LIMIT_CERO_Z.ToString() + " 才以上対象（CERO Z 相当）", ContextMenuButtonSelectAgeLimitItem_Click);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ詳細情報編集ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力値を確認する
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		protected override void CheckInput(IRcMaster oInitialMaster)
		{
			base.CheckInput(oInitialMaster);

			// チェックされているのに指定されていない項目を確認
			if (HasMaker && String.IsNullOrEmpty(MakerId))
			{
				throw new Exception("制作会社が「あり」になっていますが指定されていません。");
			}
			List<String> aTieUpGroupIds = YlCommon.SplitIds(TieUpGroupId);
			if (HasTieUpGroup && aTieUpGroupIds.Count == 0)
			{
				throw new Exception("シリーズが「あり」になっていますが指定されていません。");
			}
		}

		// --------------------------------------------------------------------
		// プロパティーの内容を Master に格納
		// --------------------------------------------------------------------
		protected override void PropertiesToRecord(IRcMaster oMaster)
		{
			base.PropertiesToRecord(oMaster);

			TTieUp aTieUp = (TTieUp)oMaster;

			// TTieUp
			aTieUp.MakerId = MakerId;
			aTieUp.AgeLimit = Common.StringToInt32(AgeLimit);
		}

		// --------------------------------------------------------------------
		// TTieUp の内容をプロパティーに反映
		// --------------------------------------------------------------------
		protected override void RecordToProperties(IRcMaster oMaster)
		{
			base.RecordToProperties(oMaster);

			TTieUp aTieUp = (TTieUp)oMaster;

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				// 年齢制限
				if (aTieUp.AgeLimit == 0)
				{
					AgeLimit = null;
				}
				else
				{
					AgeLimit = aTieUp.AgeLimit.ToString();
				}

				// 制作会社
				if (String.IsNullOrEmpty(aTieUp.MakerId))
				{
					HasMaker = false;
				}
				else
				{
					HasMaker = true;
					TMaker aMaker = YlCommon.SelectMasterById<TMaker>(aContext, aTieUp.MakerId);
					if (aMaker != null)
					{
						MakerId = aMaker.Id;
						MakerName = aMaker.Name;
					}
					else
					{
						MakerId = null;
						MakerName = null;
					}
				}

				// タイアップグループ
				List<TTieUpGroup> aTieUpGroups = YlCommon.SelectSequenceTieUpGroupsByTieUpId(aContext, aTieUp.Id);
				if (aTieUpGroups.Count == 0)
				{
					HasTieUpGroup = false;
				}
				else
				{
					HasTieUpGroup = true;
					for (Int32 i = 0; i < aTieUpGroups.Count; i++)
					{
						if (i == 0)
						{
							TieUpGroupId = aTieUpGroups[i].Id;
							TieUpGroupName = aTieUpGroups[i].Name;
						}
						else
						{
							TieUpGroupId += "," + aTieUpGroups[i].Id;
							TieUpGroupName += "," + aTieUpGroups[i].Name;
						}
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// レコード保存
		// --------------------------------------------------------------------
		protected override String Save()
		{
			TTieUp aNewRecord = new TTieUp();
			PropertiesToRecord(aNewRecord);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				if (aNewRecord.Id == NewIdForDisplay())
				{
					// 新規登録
					YlCommon.InputIdPrefixIfNeededWithInvoke(this, Environment);
					aNewRecord.Id = Environment.YukaListerSettings.PrepareLastId(aMusicInfoDbInDisk.Connection, MusicInfoDbTables.TTieUp);
					Table<TTieUp> aTable = aContext.GetTable<TTieUp>();
					aTable.InsertOnSubmit(aNewRecord);
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップテーブル新規登録：" + aNewRecord.Id + " / " + aNewRecord.Name);
				}
				else
				{
					TTieUp aExistRecord = YlCommon.SelectMasterById<TTieUp>(aContext, aNewRecord.Id, true);
					if (YlCommon.IsRcTieUpUpdated(aExistRecord, aNewRecord))
					{
						// 更新（既存のレコードが無効化されている場合は有効化も行う）
						aNewRecord.UpdateTime = aExistRecord.UpdateTime;
						Common.ShallowCopy(aNewRecord, aExistRecord);
						Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップテーブル更新：" + aNewRecord.Id + " / " + aNewRecord.Name);
					}
				}

				// タイアップグループ紐付け
				YlCommon.RegisterSequence<TTieUpGroupSequence>(aContext, aNewRecord.Id, YlCommon.SplitIds(TieUpGroupId));

				aContext.SubmitChanges();
			}

			return aNewRecord.Id;
		}

		// --------------------------------------------------------------------
		// 同名検索用関数
		// --------------------------------------------------------------------
		protected override List<IRcMaster> SelectMastersByName(SQLiteConnection oConnection, String oName)
		{
			return YlCommon.SelectMastersByName<TTieUp>(oConnection, oName).ToList<IRcMaster>();
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 制作会社を検索したかどうか
		private Boolean mIsMakerSearched = false;

		// タイアップグループを検索したかどうか
		private Boolean mIsTieUpGroupSearched = false;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ContextMenuButtonSelectAgeLimitItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				MenuItem aItem = (MenuItem)oSender;
				Int32 aAgeLimit = Common.StringToInt32((String)aItem.Header);
				if (aAgeLimit == 0)
				{
					AgeLimit = null;
				}
				else
				{
					AgeLimit = aAgeLimit.ToString();
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "年齢制限選択メニュークリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}



	}
	// public class EditTieUpWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___