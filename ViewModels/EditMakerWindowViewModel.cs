// ============================================================================
// 
// 制作会社詳細編集ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ビューは EditMasterWindow を使う。
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;

using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class EditMakerWindowViewModel : EditMasterWindowViewModel
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

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

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
				mCaption = "制作会社";

				// 基底クラス初期化
				base.Initialize();

				// タイトルバー
				Title = "制作会社詳細情報の編集";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				// ヒント
				NameHint = "株式会社・有限会社などの法人格は入力しないで下さい。";

			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "制作会社詳細情報編集ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// レコード保存
		// --------------------------------------------------------------------
		protected override String Save()
		{
			TMaker aNewRecord = new TMaker();
			PropertiesToRecord(aNewRecord);

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				if (aNewRecord.Id == NewIdForDisplay())
				{
					// 新規登録
					YlCommon.InputIdPrefixIfNeededWithInvoke(this, Environment);
					aNewRecord.Id = Environment.YukaListerSettings.PrepareLastId(aMusicInfoDbInDisk.Connection, MusicInfoDbTables.TMaker);
					Table<TMaker> aTableMaker = aContext.GetTable<TMaker>();
					aTableMaker.InsertOnSubmit(aNewRecord);
					Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "制作会社テーブル新規登録：" + aNewRecord.Id + " / " + aNewRecord.Name);
				}
				else
				{
					TMaker aExistRecord = YlCommon.SelectMasterById<TMaker>(aContext, aNewRecord.Id, true);
					if (YlCommon.IsRcMasterUpdated(aExistRecord, aNewRecord))
					{
						// 更新（既存のレコードが無効化されている場合は有効化も行う）
						aNewRecord.UpdateTime = aExistRecord.UpdateTime;
						Common.ShallowCopy(aNewRecord, aExistRecord);
						Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "制作会社テーブル更新：" + aNewRecord.Id + " / " + aNewRecord.Name);
					}
				}

				aContext.SubmitChanges();
			}

			return aNewRecord.Id;
		}

		// --------------------------------------------------------------------
		// 同名検索用関数
		// --------------------------------------------------------------------
		protected override List<IRcMaster> SelectMastersByName(SQLiteConnection oConnection, String oName)
		{
			return YlCommon.SelectMastersByName<TMaker>(oConnection, oName).ToList<IRcMaster>();
		}


	}
	// public class EditMakerWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
