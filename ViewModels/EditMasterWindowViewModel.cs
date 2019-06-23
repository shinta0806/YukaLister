// ============================================================================
// 
// Master 詳細編集ウィンドウの ViewModel 基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 本クラスは EditMasterWindow を使わない。
// EditMakerWindowViewModel などの派生クラスが EditMasterWindow を使う。
// abstract にすると VisualStudio が EditMasterWindow のプレビューを表示しなくなるので通常のクラスにしておく。
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class EditMasterWindowViewModel : ViewModel
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

		// ID キャプション
		private String mIdCaption;
		public String IdCaption
		{
			get => mIdCaption;
			set => RaisePropertyChangedIfSet(ref mIdCaption, value);
		}

		// 選択可能な ID 群
		public ObservableCollection<String> Ids { get; set; } = new ObservableCollection<String>();

		// 選択された ID
		private String mSelectedId;
		public String SelectedId
		{
			get => mSelectedId;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedId, value))
				{
					try
					{
						RecordToProperties(SelectedMaster());
						UpdateIdInfo();
					}
					catch (Exception oExcep)
					{
						Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "SelectedId 設定時エラー：\n" + oExcep.Message);
						Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
					}
				}
			}
		}

		// ID の補足情報
		private String mIdInfo;
		public String IdInfo
		{
			get => mIdInfo;
			set => RaisePropertyChangedIfSet(ref mIdInfo, value);
		}

		// フリガナ
		private String mRuby;
		public String Ruby
		{
			get => mRuby;
			set => RaisePropertyChangedIfSet(ref mRuby, value);
		}

		// 名前キャプション
		private String mNameCaption;
		public String NameCaption
		{
			get => mNameCaption;
			set => RaisePropertyChangedIfSet(ref mNameCaption, value);
		}

		// 名前
		private String mName;
		public String Name
		{
			get => mName;
			set
			{
				if (RaisePropertyChangedIfSet(ref mName, value))
				{
					WarnNameChangeIfNeeded();
				}
			}
		}

		// 名前のヒント
		private String mNameHint;
		public String NameHint
		{
			get => mNameHint;
			set => RaisePropertyChangedIfSet(ref mNameHint, value);
		}

		// 検索ワード
		private String mKeyword;
		public String Keyword
		{
			get => mKeyword;
			set => RaisePropertyChangedIfSet(ref mKeyword, value);
		}

		// 検索ワードのヒント
		private String mKeywordHint;
		public String KeywordHint
		{
			get => mKeywordHint;
			set => RaisePropertyChangedIfSet(ref mKeywordHint, value);
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

		// 選択可能な Master 群
		public ObservableCollection<IRcMaster> Masters { get; set; } = new ObservableCollection<IRcMaster>();

		// 初期表示する ID
		public String DefaultId { get; set; }

		// OK ボタンが押された時に選択されていた ID
		public String OkSelectedId { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

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

				CheckInput(SelectedMaster());
				OkSelectedId = Save();
				Messenger.Raise(new WindowActionMessage("Close"));
			}
			catch (OperationCanceledException)
			{
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "OK ボタンクリック時処理を中止しました。");
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// 本関数を呼ぶ前に Environment を設定しておく必要がある
		// --------------------------------------------------------------------
		public virtual void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				// リスナー
				Masters.CollectionChanged += MastersChanged;

				// 本関数が呼ばれる前の変更を反映
				MastersChanged(null, null);

				// キャプション
				IdCaption = mCaption + " ID (_I)：";
				NameCaption = mCaption + "名 (_N)：";

				// デフォルト ID を選択
				if (!String.IsNullOrEmpty(DefaultId) && Ids.Contains(DefaultId))
				{
					SelectedId = DefaultId;
				}
				else
				{
					SelectedId = Ids[0];
				}

				// ヒント
				KeywordHint = "キーワード、コメントなど。複数入力する際は、半角カンマ「 , 」で区切って下さい。";
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "Master 編集ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// Masters の要素を設定
		// --------------------------------------------------------------------
		public void SetMasters<T>(List<T> oItems) where T : IRcMaster
		{
			Masters.Clear();
			foreach (T aItem in oItems)
			{
				Masters.Add(aItem);
			}
		}

		// ====================================================================
		// protected メンバー変数
		// ====================================================================

		// 編集対象の名称
		protected String mCaption;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力値を確認する
		// ＜例外＞ Exception, OperationCanceledException
		// --------------------------------------------------------------------
		protected virtual void CheckInput(IRcMaster oInitialMaster)
		{
			String aNormalizedName = YlCommon.NormalizeDbString(Name);
			String aNormalizedRuby = YlCommon.NormalizeDbRuby(Ruby);
			String aNormalizedKeyword = YlCommon.NormalizeDbString(Keyword);

			// 名前が入力されているか
			if (String.IsNullOrEmpty(aNormalizedName))
			{
				throw new Exception(mCaption + "名を入力して下さい。");
			}

			// 同名の既存レコード数をカウント
			List<IRcMaster> aDup;
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			{
				aDup = SelectMastersByName(aMusicInfoDbInDisk.Connection, aNormalizedName);
			}
			Int32 aNumDup = 0;
			foreach (IRcMaster aMaster in aDup)
			{
				if (aMaster.Id != SelectedId)
				{
					aNumDup++;
				}
			}

			// 同名が既に登録されている場合
			if (aNumDup > 0)
			{
				if (String.IsNullOrEmpty(aNormalizedKeyword))
				{
					// キーワードがなければ同名の登録は禁止
					Environment.LogWriter.ShowLogMessage(TraceEventType.Error, mCaption + "「" + aNormalizedName + "」は既に登録されています。\n"
							+ "検索ワードを入力して識別できるようにしてください。");
					throw new OperationCanceledException();
				}
				else
				{
					// キーワードが同じものがあると登録は禁止
					foreach (IRcMaster aMaster in aDup)
					{
						if (aMaster.Id != SelectedId && aNormalizedKeyword == aMaster.Keyword)
						{
							throw new Exception("登録しようとしている" + mCaption + "「" + aNormalizedName + "」は既に登録されており、検索ワードも同じです。\n"
									+ mCaption + " ID を切り替えて登録済みの" + mCaption + "を選択してください。\n"
									+ "同名の別" + mCaption + "を登録しようとしている場合は、検索ワードを見分けが付くようにして下さい。");
						}
					}
				}
			}

			// フリガナとして使えない文字がある場合は警告
			WarnRubyDeletedIfNeeded(Ruby, aNormalizedRuby);

			// データベースをバックアップ
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			{
				aMusicInfoDbInDisk.Backup();
			}
		}

		// --------------------------------------------------------------------
		// 表示用新規 ID
		// --------------------------------------------------------------------
		protected String NewIdForDisplay()
		{
			return "（新規" + mCaption + "）";
		}

		// --------------------------------------------------------------------
		// プロパティーの内容を Master に格納
		// --------------------------------------------------------------------
		protected virtual void PropertiesToRecord(IRcMaster oMaster)
		{
			// IRcBase
			oMaster.Id = SelectedId;
			oMaster.Import = false;
			oMaster.Invalid = false;
			oMaster.UpdateTime = YlCommon.INVALID_MJD;
			oMaster.Dirty = true;

			// IRcMaster
			oMaster.Name = YlCommon.NormalizeDbString(Name);
			oMaster.Ruby = YlCommon.NormalizeDbRuby(Ruby);
			oMaster.Keyword = YlCommon.NormalizeDbString(Keyword);
		}

		// --------------------------------------------------------------------
		// Master の内容をプロパティーに反映
		// --------------------------------------------------------------------
		protected virtual void RecordToProperties(IRcMaster oMaster)
		{
			Ruby = oMaster.Ruby;
			Name = oMaster.Name;
			Keyword = oMaster.Keyword;
		}

		// --------------------------------------------------------------------
		// レコード保存
		// ＜返値＞ 保存したレコードの Id
		// --------------------------------------------------------------------
		protected virtual String Save()
		{
			Debug.Assert(false, "Save() derived function needed");
			return null;
		}

		// --------------------------------------------------------------------
		// 同名検索用関数
		// EditMasterWindowViewModel をテンプレートクラスにすればそもそも本関数は不要となる気がするが、
		// テンプレートクラスにすると VisualStudio が EditMasterWindow のプレビュー表示をしてくれなくなる
		// 気がするので本関数で対応する。
		// --------------------------------------------------------------------
		protected virtual List<IRcMaster> SelectMastersByName(SQLiteConnection oConnection, String oName)
		{
			Debug.Assert(false, "SelectMastersByName() derived function needed");
			return null;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void MastersChanged(Object oSender, NotifyCollectionChangedEventArgs oNotifyCollectionChangedEventArgs)
		{
			foreach (IRcMaster aMaster in Masters)
			{
				String aId;
				if (String.IsNullOrEmpty(aMaster.Id))
				{
					aId = NewIdForDisplay();
				}
				else
				{
					aId = aMaster.Id;
				}
				if (Ids.IndexOf(aId) < 0)
				{
					Ids.Add(aId);
				}
			}
		}

		// --------------------------------------------------------------------
		// 選択された Master
		// --------------------------------------------------------------------
		private IRcMaster SelectedMaster()
		{
			foreach (IRcMaster aMaster in Masters)
			{
				if (aMaster.Id == SelectedId || String.IsNullOrEmpty(aMaster.Id) && SelectedId == NewIdForDisplay())
				{
					return aMaster;
				}
			}

			throw new Exception("Master が選択されていません。");
		}

		// --------------------------------------------------------------------
		// IdInfo を更新
		// --------------------------------------------------------------------
		private void UpdateIdInfo()
		{
			if (Ids.Count <= 1)
			{
				IdInfo = null;
			}
			else if (Ids.Count == 2)
			{
				if ((String)SelectedId == NewIdForDisplay())
				{
					IdInfo = "（同名の登録が既にあります）";
				}
				else
				{
					IdInfo = null;
				}
			}
			else
			{
				IdInfo = "（同名の登録が複数あります）";
			}
		}

		// --------------------------------------------------------------------
		// 名称変更時に本当に変更するか確認
		// --------------------------------------------------------------------
		private void WarnNameChangeIfNeeded()
		{
			String aNormalizedName = YlCommon.NormalizeDbString(Name);
			IRcMaster aSelectedMaster = SelectedMaster();
			if (String.IsNullOrEmpty(aNormalizedName) || aNormalizedName == aSelectedMaster.Name)
			{
				return;
			}

			// 編集中の ID 以外で同名があるか検索
			List<IRcMaster> aDup;
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			{
				aDup = SelectMastersByName(aMusicInfoDbInDisk.Connection, aNormalizedName);
			}
			Int32 aNumDup = 0;
			foreach (IRcMaster aMaster in aDup)
			{
				if (aMaster.Id != SelectedId)
				{
					aNumDup++;
				}
			}

			// 確認
			if (String.IsNullOrEmpty(aSelectedMaster.Name))
			{
				if (aNumDup > 0)
				{
					// 空の名前から変更しようとしている場合は、同名がある場合のみ警告
					Environment.LogWriter.ShowLogMessage(TraceEventType.Warning, "登録しようとしている" + mCaption + "名「" + aNormalizedName
							+ "」は既にデータベースに登録されています。\n" + mCaption + "名は同じでも" + mCaption + "自体が異なる場合は、このまま作業を続行して下さい。\n"
							+ "それ以外の場合は、重複登録を避けるために、" + mCaption + " ID コンボボックスから既存の" + mCaption + "情報を選択して下さい。");
				}
			}
			else
			{
				String aAdd = null;
				if (aNumDup > 0)
				{
					aAdd = "\n\n【注意】\n変更後の名前は既にデータベースに登録されています。";
				}
				if (MessageBox.Show(mCaption + "名を「" + aSelectedMaster.Name + "」から「" + aNormalizedName + "」に変更しますか？" + aAdd, "確認",
						MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
				{
					Name = aSelectedMaster.Name;
					return;
				}
			}

			// 同名のレコードが編集対象になっていない場合は追加する
			foreach (IRcMaster aMaster in aDup)
			{
				if (Ids.IndexOf(aMaster.Id) >= 0)
				{
					continue;
				}

				Masters.Add(aMaster);
			}
		}

		// --------------------------------------------------------------------
		// ルビの一部が削除されたら警告
		// ＜例外＞ OperationCanceledException
		// --------------------------------------------------------------------
		private void WarnRubyDeletedIfNeeded(String oOriginalRuby, String oNormalizedRuby)
		{
			if (!String.IsNullOrEmpty(oOriginalRuby)
					&& (String.IsNullOrEmpty(oNormalizedRuby) || oOriginalRuby.Length != oNormalizedRuby.Length))
			{
				if (MessageBox.Show("フリガナはカタカナのみ登録可能のため、カタカナ以外は削除されます。\n"
						+ oOriginalRuby + " → " + oNormalizedRuby + "\nよろしいですか？", "確認",
						MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
				{
					throw new OperationCanceledException();
				}
			}
		}


	}
	// public class EditMasterWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
