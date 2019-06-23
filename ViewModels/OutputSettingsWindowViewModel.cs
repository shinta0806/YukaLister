// ============================================================================
// 
// リスト出力設定ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using YukaLister.Models;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using YukaLister.Models.OutputWriters;
using System.Reflection;
using Shinta;
using System.IO;
using System.Xml;
using System.Windows.Markup;
using System.Windows;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class OutputSettingsWindowViewModel : ViewModel
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

		// タブアイテム
		public ObservableCollection<TabItem> TabItems { get; set; } = new ObservableCollection<TabItem>();

		// 選択タブ
		private Int32 mSelectedTabIndex;
		public Int32 SelectedTabIndex
		{
			get => mSelectedTabIndex;
			set => RaisePropertyChangedIfSet(ref mSelectedTabIndex, value);
		}

		// タブコントロールの高さ
		private Double mActualTabControlHeight;
		public Double ActualTabControlHeight
		{
			get => mActualTabControlHeight;
			set
			{
				if(RaisePropertyChangedIfSet(ref mActualTabControlHeight, value))
				{
					MinTabControlHeight = mActualTabControlHeight;
				}
			}
		}

		// タブコントロールの最小高さ
		private Double mMinTabControlHeight;
		public Double MinTabControlHeight
		{
			get => mMinTabControlHeight;
			set => RaisePropertyChangedIfSet(ref mMinTabControlHeight, value);
		}

		// タブコントロールの幅
		private Double mActualTabControlWidth;
		public Double ActualTabControlWidth
		{
			get => mActualTabControlWidth;
			set
			{
				if (RaisePropertyChangedIfSet(ref mActualTabControlWidth, value))
				{
					MinTabControlWidth = mActualTabControlWidth;
				}
			}
		}

		// タブコントロールの最小幅
		private Double mMinTabControlWidth;
		public Double MinTabControlWidth
		{
			get => mMinTabControlWidth;
			set => RaisePropertyChangedIfSet(ref mMinTabControlWidth, value);
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

		#endregion

		#region 基本設定タブのプロパティー

		// 出力項目のタイプ
		private Boolean mOutputAllItems;
		public Boolean OutputAllItems
		{
			get => mOutputAllItems;
			set => RaisePropertyChangedIfSet(ref mOutputAllItems, value);
		}

		// 出力項目のタイプの逆
		// 動的 XAML 読み込みで BooleanInvertConverter が使えないために必要となる
		private Boolean mOutputAllItemsInvert/* = true*/;
		public Boolean OutputAllItemsInvert
		{
			get => mOutputAllItemsInvert;
			set => RaisePropertyChangedIfSet(ref mOutputAllItemsInvert, value);
		}

		// 出力されない項目
		public ObservableCollection<String> RemovedOutputItems { get; set; } = new ObservableCollection<String>();

		// 選択されている出力されない項目
		private String mSelectedRemovedOutputItem;
		public String SelectedRemovedOutputItem
		{
			get => mSelectedRemovedOutputItem;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedRemovedOutputItem, value))
				{
					ButtonAddOutputItemClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// 出力される項目
		public ObservableCollection<String> AddedOutputItems { get; set; } = new ObservableCollection<String>();

		// 選択されている出力される項目
		private String mSelectedAddedOutputItem;
		public String SelectedAddedOutputItem
		{
			get => mSelectedAddedOutputItem;
			set
			{
				if (RaisePropertyChangedIfSet(ref mSelectedAddedOutputItem, value))
				{
					ButtonRemoveOutputItemClickedCommand.RaiseCanExecuteChanged();
					ButtonUpOutputItemClickedCommand.RaiseCanExecuteChanged();
					ButtonDownOutputItemClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		#endregion

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 環境設定類
		public EnvironmentModel Environment { get; set; }

		// 出力設定
		public OutputWriter OutputWriter { get; set; }

		// OK ボタンが押されたか
		public Boolean IsOk { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region 基本設定タブのコマンド

		#region ヘルプリンクの制御
		private ListenerCommand<String> mHelpClickedCommand;

		public ListenerCommand<String> HelpClickedCommand
		{
			get
			{
				if (mHelpClickedCommand == null)
				{
					mHelpClickedCommand = new ListenerCommand<String>(HelpClicked);
				}
				return mHelpClickedCommand;
			}
		}

		public void HelpClicked(String oParameter)
		{
			try
			{
				YlCommon.ShowHelp(Environment, oParameter);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 出力項目追加ボタンの制御
		private ViewModelCommand mButtonAddOutputItemClickedCommand;

		public ViewModelCommand ButtonAddOutputItemClickedCommand
		{
			get
			{
				if (mButtonAddOutputItemClickedCommand == null)
				{
					mButtonAddOutputItemClickedCommand = new ViewModelCommand(ButtonAddOutputItemClicked, CanButtonAddOutputItemClicked);
				}
				return mButtonAddOutputItemClickedCommand;
			}
		}

		public Boolean CanButtonAddOutputItemClicked()
		{
			return SelectedRemovedOutputItem != null;
		}

		public void ButtonAddOutputItemClicked()
		{
			try
			{
				if (SelectedRemovedOutputItem == null)
				{
					return;
				}

				AddedOutputItems.Add(SelectedRemovedOutputItem);
				SelectedAddedOutputItem = SelectedRemovedOutputItem;
				RemovedOutputItems.Remove(SelectedRemovedOutputItem);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "出力項目追加ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 出力項目削除ボタンの制御
		private ViewModelCommand mButtonRemoveOutputItemClickedCommand;

		public ViewModelCommand ButtonRemoveOutputItemClickedCommand
		{
			get
			{
				if (mButtonRemoveOutputItemClickedCommand == null)
				{
					mButtonRemoveOutputItemClickedCommand = new ViewModelCommand(ButtonRemoveOutputItemClicked, CanButtonRemoveOutputItemClicked);
				}
				return mButtonRemoveOutputItemClickedCommand;
			}
		}

		public Boolean CanButtonRemoveOutputItemClicked()
		{
			return SelectedAddedOutputItem != null;
		}

		public void ButtonRemoveOutputItemClicked()
		{
			try
			{
				if (SelectedAddedOutputItem == null)
				{
					return;
				}

				RemovedOutputItems.Add(SelectedAddedOutputItem);
				SelectedRemovedOutputItem = SelectedAddedOutputItem;
				AddedOutputItems.Remove(SelectedAddedOutputItem);
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "出力項目削除ボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 出力項目上へボタンの制御
		private ViewModelCommand mButtonUpOutputItemClickedCommand;

		public ViewModelCommand ButtonUpOutputItemClickedCommand
		{
			get
			{
				if (mButtonUpOutputItemClickedCommand == null)
				{
					mButtonUpOutputItemClickedCommand = new ViewModelCommand(ButtonUpOutputItemClicked, CanButtonUpOutputItemClicked);
				}
				return mButtonUpOutputItemClickedCommand;
			}
		}

		public Boolean CanButtonUpOutputItemClicked()
		{
			Int32 aIndex = AddedOutputItems.IndexOf(SelectedAddedOutputItem);
			return aIndex >= 1;
		}

		public void ButtonUpOutputItemClicked()
		{
			try
			{
				Int32 aSelectedIndex = AddedOutputItems.IndexOf(SelectedAddedOutputItem);
				if (aSelectedIndex < 1)
				{
					return;
				}
				String aItem = SelectedAddedOutputItem;
				AddedOutputItems.Remove(aItem);
				AddedOutputItems.Insert(aSelectedIndex - 1, aItem);
				SelectedAddedOutputItem = aItem;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "出力項目上へボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

		#region 出力項目下へボタンの制御
		private ViewModelCommand mButtonDownOutputItemClickedCommand;

		public ViewModelCommand ButtonDownOutputItemClickedCommand
		{
			get
			{
				if (mButtonDownOutputItemClickedCommand == null)
				{
					mButtonDownOutputItemClickedCommand = new ViewModelCommand(ButtonDownOutputItemClicked, CanButtonDownOutputItemClicked);
				}
				return mButtonDownOutputItemClickedCommand;
			}
		}

		public Boolean CanButtonDownOutputItemClicked()
		{
			Int32 aIndex = AddedOutputItems.IndexOf(SelectedAddedOutputItem);
			return 0 <= aIndex && aIndex < AddedOutputItems.Count - 1;
		}

		public void ButtonDownOutputItemClicked()
		{
			try
			{
				Int32 aSelectedIndex = AddedOutputItems.IndexOf(SelectedAddedOutputItem);
				if (aSelectedIndex < 0 || aSelectedIndex >= AddedOutputItems.Count - 1)
				{
					return;
				}
				String aItem = SelectedAddedOutputItem;
				AddedOutputItems.Remove(aItem);
				AddedOutputItems.Insert(aSelectedIndex + 1, aItem);
				SelectedAddedOutputItem = aItem;
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "出力項目下へボタンクリック時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
		#endregion

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
				OutputWriter.OutputSettings.Save();
				IsOk = true;
				Messenger.Raise(new WindowActionMessage("Close"));
			}
			catch (OperationCanceledException)
			{
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "設定変更を中止しました。");
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

		// --------------------------------------------------------------------
		// 初期化
		// 本関数を呼ぶ前に Environment を設定しておく必要がある
		// --------------------------------------------------------------------
		public virtual void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				// タイトルバー
				Title = "出力設定：" + OutputWriter.FormatName;
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				AddTabItems();
				SettingsToProperties();
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "リスト出力設定ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リソースからユーザーコントロールを読み込んでタブとして追加
		// --------------------------------------------------------------------
		protected void AddTabItem(String oControlName, String oCaption)
		{
			Assembly aAssembly = Assembly.GetExecutingAssembly();
			Stream aStream = aAssembly.GetManifestResourceStream("YukaLister.Views.UserControls." + oControlName + Common.FILE_EXT_XAML);
			using (StreamReader aReader = new StreamReader(aStream))
			{
				XmlReader aXml = XmlReader.Create(aReader.BaseStream);
				FrameworkElement aElement = XamlReader.Load(aXml) as FrameworkElement;
				TabItem aTabItem = new TabItem()
				{
					Header = oCaption,
					Content = aElement,
				};
				TabItems.Add(aTabItem);
			}
		}

		// --------------------------------------------------------------------
		// タブアイテムにタブを追加
		// --------------------------------------------------------------------
		protected virtual void AddTabItems()
		{
			AddTabItem("OutputSettingsTabItemBasic", "基本設定");
		}

		// --------------------------------------------------------------------
		// 設定画面に入力された値が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		protected virtual void CheckInput()
		{
		}

		// --------------------------------------------------------------------
		// プロパティーから設定に反映
		// --------------------------------------------------------------------
		protected virtual void PropertiesToSettings()
		{
			// 出力項目のタイプ
			OutputWriter.OutputSettings.OutputAllItems = OutputAllItems;

			// 出力項目のリスト
			OutputWriter.OutputSettings.SelectedOutputItems.Clear();
			for (Int32 i = 0; i < AddedOutputItems.Count; i++)
			{
				Int32 aItem = Array.IndexOf(YlCommon.OUTPUT_ITEM_NAMES, (String)AddedOutputItems[i]);
				if (aItem < 0)
				{
					continue;
				}
				OutputWriter.OutputSettings.SelectedOutputItems.Add((OutputItems)aItem);
			}
		}

		// --------------------------------------------------------------------
		// 設定をプロパティーに反映
		// --------------------------------------------------------------------
		protected virtual void SettingsToProperties()
		{
			// 出力項目のタイプ
			OutputAllItems = OutputWriter.OutputSettings.OutputAllItems;
			OutputAllItemsInvert = !OutputWriter.OutputSettings.OutputAllItems;

			// 出力されない項目
			OutputItems[] aOutputItems = (OutputItems[])Enum.GetValues(typeof(OutputItems));
			for (Int32 i = 0; i < aOutputItems.Length - 1; i++)
			{
				if (!OutputWriter.OutputSettings.SelectedOutputItems.Contains(aOutputItems[i]))
				{
					RemovedOutputItems.Add(YlCommon.OUTPUT_ITEM_NAMES[(Int32)aOutputItems[i]]);
				}
			}

			// 出力される項目
			for (Int32 i = 0; i < OutputWriter.OutputSettings.SelectedOutputItems.Count; i++)
			{
				AddedOutputItems.Add(YlCommon.OUTPUT_ITEM_NAMES[(Int32)OutputWriter.OutputSettings.SelectedOutputItems[i]]);
			}
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================


	}
	// public class OutputSettingsWindowViewModel ___END___

}
// namespace YukaLister.ViewModels ___END___
