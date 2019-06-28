// ============================================================================
// 
// 楽曲情報データベース検索ウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ToDo: 検索をバックグラウンドタスクにしないとカーソル変更が反映されない模様
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Windows.Input;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class SearchMusicInfoWindowViewModel : ViewModel
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

		// カーソル
		private Cursor mCursor;
		public Cursor Cursor
		{
			get => mCursor;
			set => RaisePropertyChangedIfSet(ref mCursor, value);
		}

		// 説明
		private String mDescription;
		public String Description
		{
			get => mDescription;
			set => RaisePropertyChangedIfSet(ref mDescription, value);
		}

		// 入力されたキーワード
		private String mKeyword;
		public String Keyword
		{
			get => mKeyword;
			set
			{
				if (RaisePropertyChangedIfSet(ref mKeyword, value))
				{
					ButtonSearchClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// キーワード選択
		private String mSelectedKeyword;
		public String SelectedKeyword
		{
			get => mSelectedKeyword;
			set => RaisePropertyChangedIfSet(ref mSelectedKeyword, value);
		}

		// キーワードフォーカス
		private Boolean mIsKeywordFocused;
		public Boolean IsKeywordFocused
		{
			get => mIsKeywordFocused;
			set
			{
				// 再度フォーカスを当てられるように強制伝播
				mIsKeywordFocused = value;
				RaisePropertyChanged(nameof(IsKeywordFocused));
			}
		}

		// 検索結果の説明
		private String mFoundsDescription;
		public String FoundsDescription
		{
			get => mFoundsDescription;
			set => RaisePropertyChangedIfSet(ref mFoundsDescription, value);
		}

		// 検索結果
		private List<String> mFounds;
		public List<String> Founds
		{
			get => mFounds;
			set => RaisePropertyChangedIfSet(ref mFounds, value);
		}

		// 検索結果フォーカス
		private Boolean mAreFoundsFocused;
		public Boolean AreFoundsFocused
		{
			get => mAreFoundsFocused;
			set
			{
				// 再度フォーカスを当てられるように強制伝播
				mAreFoundsFocused = value;
				RaisePropertyChanged(nameof(AreFoundsFocused));
			}
		}

		// 選択された検索結果
		private String mSelectedFound;
		public String SelectedFound
		{
			get => mSelectedFound;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedFound, value))
				{
					ButtonSelectClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}



		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// 検索項目名
		public String ItemName { get; set; }

		// テーブルインデックス
		public MusicInfoDbTables TableIndex { get; set; }

		// 選択ボタンで選択された情報
		public String DecidedName { get; private set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region 検索ボタンの制御
		private ViewModelCommand mButtonSearchClickedCommand;

		public ViewModelCommand ButtonSearchClickedCommand
		{
			get
			{
				if (mButtonSearchClickedCommand == null)
				{
					mButtonSearchClickedCommand = new ViewModelCommand(ButtonSearchClicked, CanButtonSearchClicked);
				}
				return mButtonSearchClickedCommand;
			}
		}

		public Boolean CanButtonSearchClicked()
		{
			return !String.IsNullOrEmpty(YlCommon.NormalizeDbString(Keyword));
		}

		public void ButtonSearchClicked()
		{
			try
			{
				String aKeyword = YlCommon.NormalizeDbString(Keyword);
				if (String.IsNullOrEmpty(aKeyword))
				{
					return;
				}

				Cursor = Cursors.Wait;
				Founds = null;
				ClearLabelFounds();
#if DEBUGz
				Thread.Sleep(3000);
#endif

				// 検索
				List<String> aHits = new List<String>();
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					using (SQLiteCommand aCmd = new SQLiteCommand(aMusicInfoDbInDisk.Connection))
					{
						aCmd.CommandText = "SELECT DISTINCT " + YlConstants.MUSIC_INFO_DB_NAME_COLUMN_NAMES[(Int32)TableIndex]
								+ " FROM " + YlConstants.MUSIC_INFO_DB_TABLE_NAMES[(Int32)TableIndex]
								+ " WHERE (" + YlConstants.MUSIC_INFO_DB_NAME_COLUMN_NAMES[(Int32)TableIndex] + " LIKE @keyword1"
								+ " OR " + YlConstants.MUSIC_INFO_DB_KEYWORD_COLUMN_NAMES[(Int32)TableIndex] + " LIKE @keyword2";
						aCmd.Parameters.Add(new SQLiteParameter("@keyword1", "%" + aKeyword + "%"));
						aCmd.Parameters.Add(new SQLiteParameter("@keyword2", "%" + aKeyword + "%"));

						String aRuby = YlCommon.NormalizeDbRuby(Keyword);
						if (!String.IsNullOrEmpty(aRuby) && aRuby.Length == Keyword.Length)
						{
							// すべてフリガナとして使える文字が入力された場合は、フリガナでも検索
							aCmd.CommandText += " OR " + YlConstants.MUSIC_INFO_DB_RUBY_COLUMN_NAMES[(Int32)TableIndex] + " LIKE @ruby1";
							aCmd.Parameters.Add(new SQLiteParameter("@ruby1", "%" + aRuby + "%"));
							// 検索ワードもフリガナでも検索
							aCmd.CommandText += " OR " + YlConstants.MUSIC_INFO_DB_KEYWORD_COLUMN_NAMES[(Int32)TableIndex] + " LIKE @ruby2";
							aCmd.Parameters.Add(new SQLiteParameter("@ruby2", "%" + aRuby + "%"));
						}

						aCmd.CommandText += ") AND " + YlConstants.MUSIC_INFO_DB_INVALID_COLUMN_NAMES[(Int32)TableIndex] + " = 0";


						using (SQLiteDataReader aReader = aCmd.ExecuteReader())
						{
							while (aReader.Read())
							{
								aHits.Add(aReader[0].ToString());
							}
						}
					}
				}

				if (aHits.Count == 0)
				{
					Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "「" + aKeyword + "」を含む" + ItemName + "はありません。");
					return;
				}
				aHits.Sort();
				FoundsDescription = aHits.Count.ToString("#,0") + " 個の結果が見つかりました。";

				// リストボックスに表示
				Founds = aHits;
				AreFoundsFocused = true;

				// 選択（完全一致）
				if (Founds.IndexOf(aKeyword) >= 0)
				{
					SelectedFound = aKeyword;
					return;
				}

				// 選択（大文字小文字を区別しない）
				SelectedFound = aHits.Find(x => String.Compare(x, aKeyword, true) == 0);

				// 先頭を選択
				if (SelectedFound == null)
				{
					SelectedFound = Founds[0];
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "検索時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
			finally
			{
				Cursor = Cursors.Arrow;
			}
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
				Select();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "データグリッドダブルクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 選択ボタンの制御
		private ViewModelCommand mButtonSelectClickedCommand;

		public ViewModelCommand ButtonSelectClickedCommand
		{
			get
			{
				if (mButtonSelectClickedCommand == null)
				{
					mButtonSelectClickedCommand = new ViewModelCommand(ButtonSelectClicked, CanButtonSelectClicked);
				}
				return mButtonSelectClickedCommand;
			}
		}

		public Boolean CanButtonSelectClicked()
		{
			return SelectedFound != null;
		}

		public void ButtonSelectClicked()
		{
			try
			{
				Select();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "閉じるボタンクリック時エラー：\n" + oExcep.Message);
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
				Title = ItemName + "を検索";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif
				// 説明
				Description = ItemName + "を、既に登録されている情報から検索します。";
				ClearLabelFounds();

				// フォーカス
				IsKeywordFocused = true;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "楽曲情報データベース検索ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// prvate メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// LabelFounds を見かけ上空欄にする
		// --------------------------------------------------------------------
		private void ClearLabelFounds()
		{
			// null にするとラベルの高さが変わってしまうためスペースを入れる
			FoundsDescription = " ";
		}

		// --------------------------------------------------------------------
		// 選択中のアイテムで決定
		// --------------------------------------------------------------------
		private void Select()
		{
			if (SelectedFound == null)
			{
				return;
			}

			DecidedName = SelectedFound;
			Messenger.Raise(new WindowActionMessage("Close"));
		}

	}
	// public class SearchMusicInfoWindowViewModel
}
// namespace YukaLister.ViewModels
