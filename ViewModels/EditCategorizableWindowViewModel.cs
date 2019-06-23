// ============================================================================
// 
// IRcCategorizable 編集ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using YukaLister.Models;
using System.Windows.Controls;
using System.Diagnostics;
using YukaLister.Models.SharedMisc;
using System.Windows;
using YukaLister.Models.Database;
using System.Data.Linq;

namespace YukaLister.ViewModels
{
	public abstract class EditCategorizableWindowViewModel : EditMasterWindowViewModel
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

		// カテゴリーあり
		private Boolean mHasCategory;
		public Boolean HasCategory
		{
			get => mHasCategory;
			set
			{
				if (RaisePropertyChangedIfSet(ref mHasCategory, value))
				{
					ButtonSelectCategoryClickedCommand.RaiseCanExecuteChanged();
					if (!mHasCategory)
					{
						CategoryId = null;
						CategoryName = null;
					}
					HasCategoryChanged();
				}
			}
		}

		// カテゴリー選択ボタンのコンテキストメニュー
		public List<MenuItem> ContextMenuButtonSelectCategoryItems { get; set; }

		// カテゴリー名
		private String mCategoryName;
		public String CategoryName
		{
			get => mCategoryName;
			set => RaisePropertyChangedIfSet(ref mCategoryName, value);
		}

		// リリース年
		private String mReleaseYear;
		public String ReleaseYear
		{
			get => mReleaseYear;
			set => RaisePropertyChangedIfSet(ref mReleaseYear, value);
		}

		// リリース月
		private String mReleaseMonth;
		public String ReleaseMonth
		{
			get => mReleaseMonth;
			set => RaisePropertyChangedIfSet(ref mReleaseMonth, value);
		}

		// リリース日
		private String mReleaseDay;
		public String ReleaseDay
		{
			get => mReleaseDay;
			set => RaisePropertyChangedIfSet(ref mReleaseDay, value);
		}

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// カテゴリー ID
		public String CategoryId { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region カテゴリー選択ボタンの制御
		private ViewModelCommand mButtonSelectCategoryClickedCommand;

		public ViewModelCommand ButtonSelectCategoryClickedCommand
		{
			get
			{
				if (mButtonSelectCategoryClickedCommand == null)
				{
					mButtonSelectCategoryClickedCommand = new ViewModelCommand(ButtonSelectCategoryClicked, CanButtonSelectCategoryClicked);
				}
				return mButtonSelectCategoryClickedCommand;
			}
		}

		public Boolean CanButtonSelectCategoryClicked()
		{
			return HasCategory;
		}

		public void ButtonSelectCategoryClicked()
		{

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
				// 基底クラス初期化
				base.Initialize();

				// カテゴリー選択ボタンのコンテキストメニュー
				ContextMenuButtonSelectCategoryItems = new List<MenuItem>();
				YlCommon.SetContextMenuItemCategories(ContextMenuButtonSelectCategoryItems, ContextMenuButtonSelectCategoryItem_Click, Environment);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "IRcCategorizable 編集ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
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
			if (HasCategory && String.IsNullOrEmpty(CategoryId))
			{
				throw new Exception("カテゴリーが「あり」になっていますが指定されていません。");
			}
		}

		// --------------------------------------------------------------------
		// HasCategory が変更された
		// --------------------------------------------------------------------
		protected virtual void HasCategoryChanged()
		{
		}

		// --------------------------------------------------------------------
		// プロパティーの内容を Master に格納
		// --------------------------------------------------------------------
		protected override void PropertiesToRecord(IRcMaster oMaster)
		{
			base.PropertiesToRecord(oMaster);

			IRcCategorizable aCategorizable = (IRcCategorizable)oMaster;

			// IRcCategorizable
			aCategorizable.CategoryId = CategoryId;
			aCategorizable.ReleaseDate = YlCommon.StringsToMjd("リリース日", ReleaseYear, ReleaseMonth, ReleaseDay);
		}

		// --------------------------------------------------------------------
		// Categorizable の内容をプロパティーに反映
		// --------------------------------------------------------------------
		protected override void RecordToProperties(IRcMaster oMaster)
		{
			base.RecordToProperties(oMaster);

			IRcCategorizable aCategorizable = (IRcCategorizable)oMaster;

			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				// カテゴリー関係
				if (String.IsNullOrEmpty(aCategorizable.CategoryId))
				{
					HasCategory = false;
				}
				else
				{
					HasCategory = true;
					TCategory aCategory = YlCommon.SelectMasterById<TCategory>(aContext, aCategorizable.CategoryId);
					if (aCategory != null)
					{
						CategoryId = aCategory.Id;
						CategoryName = aCategory.Name;
					}
					else
					{
						CategoryId = null;
						CategoryName = null;
					}
				}

				// リリース日
				YlCommon.MjdToStrings(aCategorizable.ReleaseDate, out String aReleaseYear, out String aReleaseMonth, out String aReleaseDay);
				ReleaseYear = aReleaseYear;
				ReleaseMonth = aReleaseMonth;
				ReleaseDay = aReleaseDay;
			}
		}


		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ContextMenuButtonSelectCategoryItem_Click(Object oSender, RoutedEventArgs oRoutedEventArgs)
		{
			try
			{
				MenuItem aItem = (MenuItem)oSender;
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					List<TCategory> aCategories = YlCommon.SelectMastersByName<TCategory>(aMusicInfoDbInDisk.Connection, (String)aItem.Header);
					if (aCategories.Count > 0)
					{
						CategoryId = aCategories[0].Id;
						CategoryName = aCategories[0].Name;
					}
				}
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "カテゴリー選択メニュークリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

	}
	// public abstract class EditCategorizableWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
