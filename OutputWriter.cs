// ============================================================================
// 
// リスト出力用基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Shinta;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;

namespace YukaLister.Shared
{
	public abstract class OutputWriter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public OutputWriter()
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 出力形式（表示用）
		public String FormatName { get; protected set; }

		// 出力先フォルダー（末尾 '\\' 付き）
		private String mFolderPath;
		public String FolderPath
		{
			get
			{
				return mFolderPath;
			}
			set
			{
				mFolderPath = value;
				if (!String.IsNullOrEmpty(mFolderPath) && mFolderPath[mFolderPath.Length - 1] != '\\')
				{
					mFolderPath += '\\';
				}
			}
		}

		// 出力先インデックスファイル名（パス無し）
		public String TopFileName { get; protected set; }

		// 検索結果テーブル
		public Table<TFound> TableFound { get; set; }

		// 出力設定
		public OutputSettings OutputSettings { get; set; }

		// ログ
		public LogWriter LogWriter { get; set; }

		// オーナーウィンドウ
		public Window Owner { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定画面に入力された値が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public virtual void CheckInput()
		{
		}

		// --------------------------------------------------------------------
		// コンポーネントから設定に反映
		// --------------------------------------------------------------------
		public virtual void ComposToSettings()
		{
			// 出力項目のタイプ
			OutputSettings.OutputAllItems = (Boolean)RadioButtonOutputAllItems.IsChecked;

			// 出力項目のリスト
			OutputSettings.SelectedOutputItems.Clear();
			for (Int32 i = 0; i < ListBoxAddedItems.Items.Count; i++)
			{
				Int32 aItem = Array.IndexOf(YlCommon.OUTPUT_ITEM_NAMES, (String)ListBoxAddedItems.Items[i]);
				if (aItem < 0)
				{
					continue;
				}
				OutputSettings.SelectedOutputItems.Add((OutputItems)aItem);
			}
		}

		// --------------------------------------------------------------------
		// 設定画面のタブページ
		// --------------------------------------------------------------------
		public virtual List<TabItem> DialogTabItems()
		{
			List<TabItem> aTabItems = new List<TabItem>();

			// TabItemOutputSettings
			TabItemOutputSettings = new TabItem();
			TabItemOutputSettings.Header = "基本設定";

			// StackPanelOutputSettings
			StackPanel aStackPanelOutputSettings = new StackPanel();
			TabItemOutputSettings.Content = aStackPanelOutputSettings;
			{
				// StackPanelOutputItems
				StackPanel aStackPanelOutputItems = new StackPanel();
				aStackPanelOutputItems.Orientation = Orientation.Horizontal;
				aStackPanelOutputItems.Margin = new Thickness(20, 20, 0, 0);
				aStackPanelOutputSettings.Children.Add(aStackPanelOutputItems);
				{
					// LabelOutputItems
					Label aLabelOutputItems = new Label();
					aLabelOutputItems.Content = "出力項目：";
					aStackPanelOutputItems.Children.Add(aLabelOutputItems);

					// RadioButtonOutputAllItems
					RadioButtonOutputAllItems = new RadioButton();
					RadioButtonOutputAllItems.Content = "すべて (_L)";
					RadioButtonOutputAllItems.VerticalAlignment = VerticalAlignment.Center;
					RadioButtonOutputAllItems.Checked += RadioButtonOutputItems_Checked;
					aStackPanelOutputItems.Children.Add(RadioButtonOutputAllItems);

					// RadioButtonOutputAddedItems
					RadioButtonOutputAddedItems = new RadioButton();
					RadioButtonOutputAddedItems.Content = "以下で追加した項目のみ (_O)";
					RadioButtonOutputAddedItems.VerticalAlignment = VerticalAlignment.Center;
					RadioButtonOutputAddedItems.Margin = new Thickness(20, 0, 0, 0);
					RadioButtonOutputAddedItems.Checked += RadioButtonOutputItems_Checked;
					aStackPanelOutputItems.Children.Add(RadioButtonOutputAddedItems);

					// LabelHelp
					Label aLabelHelp = new Label();
					aLabelHelp.Margin = new Thickness(20, 0, 0, 0);
					aStackPanelOutputItems.Children.Add(aLabelHelp);
					{
						// Hyperlink
						Hyperlink aHyperlink = new Hyperlink();
						aHyperlink.NavigateUri = new Uri("Kihonsetteitab", UriKind.Relative);
						aHyperlink.RequestNavigate += HyperlinkHelp_RequestNavigate;
						aLabelHelp.Content = aHyperlink;
						{
							// TextBlock
							TextBlock aTextBlock = new TextBlock();
							aTextBlock.Text = "詳細情報";
							aHyperlink.Inlines.Add(aTextBlock);
						}
					}
				}

				// StackPanelListBoxCaption
				StackPanel aStackPanelListBoxCaption = new StackPanel();
				aStackPanelListBoxCaption.Orientation = Orientation.Horizontal;
				aStackPanelListBoxCaption.Margin = new Thickness(20, 10, 0, 0);
				aStackPanelOutputSettings.Children.Add(aStackPanelListBoxCaption);
				{
					// LabelRemovedItems
					LabelRemovedItems = new Label();
					LabelRemovedItems.Content = "（出力されない項目）";
					LabelRemovedItems.Width = 270;
					aStackPanelListBoxCaption.Children.Add(LabelRemovedItems);

					// LabelAddedItems
					LabelAddedItems = new Label();
					LabelAddedItems.Content = "（出力される項目）";
					aStackPanelListBoxCaption.Children.Add(LabelAddedItems);
				}

				// StackPanelListBox
				StackPanel aStackPanelListBox = new StackPanel();
				aStackPanelListBox.Orientation = Orientation.Horizontal;
				aStackPanelListBox.Margin = new Thickness(20, 0, 20, 20);
				aStackPanelOutputSettings.Children.Add(aStackPanelListBox);
				{
					// ListBoxRemovedItems
					ListBoxRemovedItems = new ListBox();
					ListBoxRemovedItems.Width = 150;
					ListBoxRemovedItems.Height = 180;
					ListBoxRemovedItems.SelectionChanged += ListBoxRemovedItems_SelectionChanged;
					ScrollViewer.SetVerticalScrollBarVisibility(ListBoxRemovedItems, ScrollBarVisibility.Visible);
					aStackPanelListBox.Children.Add(ListBoxRemovedItems);

					// StackPanelMoveButtons
					StackPanel aStackPanelMoveButtons = new StackPanel();
					aStackPanelMoveButtons.Margin = new Thickness(10, 0, 10, 0);
					aStackPanelMoveButtons.VerticalAlignment = VerticalAlignment.Center;
					aStackPanelListBox.Children.Add(aStackPanelMoveButtons);
					{
						Style aLightStyle = (Style)Owner.FindResource(YlCommon.RSRC_NAME_RAISED_LIGHT_BUTTON);

						// ButtonAddItem
						ButtonAddItem = new Button();
						ButtonAddItem.Height = Double.NaN;
						ButtonAddItem.Content = "→ 追加 (_D)";
						ButtonAddItem.Style = aLightStyle;
						ButtonAddItem.Margin = new Thickness(0, 0, 0, 0);
						ButtonAddItem.Width = 100;
						ButtonAddItem.Padding = new Thickness(0, 4, 0, 4);
						ButtonAddItem.Click += ButtonAddItem_Click;
						aStackPanelMoveButtons.Children.Add(ButtonAddItem);

						// ButtonRemoveItem
						ButtonRemoveItem = new Button();
						ButtonRemoveItem.Height = Double.NaN;
						ButtonRemoveItem.Content = "× 削除 (_M)";
						ButtonRemoveItem.Style = aLightStyle;
						ButtonRemoveItem.Margin = new Thickness(0, 10, 0, 0);
						ButtonRemoveItem.Padding = new Thickness(0, 4, 0, 4);
						ButtonRemoveItem.Click += ButtonRemoveItem_Click;
						aStackPanelMoveButtons.Children.Add(ButtonRemoveItem);

						// ButtonUpItem
						ButtonUpItem = new Button();
						ButtonUpItem.Height = Double.NaN;
						ButtonUpItem.Content = "↑ 上へ (_U)";
						ButtonUpItem.Style = aLightStyle;
						ButtonUpItem.Margin = new Thickness(0, 20, 0, 0);
						ButtonUpItem.Padding = new Thickness(0, 4, 0, 4);
						ButtonUpItem.Click += ButtonUpItem_Click;
						aStackPanelMoveButtons.Children.Add(ButtonUpItem);

						// ButtonDownItem
						ButtonDownItem = new Button();
						ButtonDownItem.Height = Double.NaN;
						ButtonDownItem.Content = "↓ 下へ (_W)";
						ButtonDownItem.Style = aLightStyle;
						ButtonDownItem.Margin = new Thickness(0, 10, 0, 0);
						ButtonDownItem.Padding = new Thickness(0, 4, 0, 4);
						ButtonDownItem.Click += ButtonDownItem_Click;
						aStackPanelMoveButtons.Children.Add(ButtonDownItem);
					}

					// ListBoxAddedItems
					ListBoxAddedItems = new ListBox();
					ListBoxAddedItems.Width = 150;
					ListBoxAddedItems.Height = 180;
					ListBoxAddedItems.SelectionChanged += ListBoxAddedItems_SelectionChanged;
					ScrollViewer.SetVerticalScrollBarVisibility(ListBoxAddedItems, ScrollBarVisibility.Visible);
					aStackPanelListBox.Children.Add(ListBoxAddedItems);
				}
			}

			aTabItems.Add(TabItemOutputSettings);

			return aTabItems;
		}

		// --------------------------------------------------------------------
		// 設定画面を有効化するかどうか
		// --------------------------------------------------------------------
		public abstract Boolean IsDialogEnabled();

		// --------------------------------------------------------------------
		// リスト出力
		// --------------------------------------------------------------------
		public abstract void Output();

		// --------------------------------------------------------------------
		// 設定をコンポーネントに反映
		// --------------------------------------------------------------------
		public virtual void SettingsToCompos()
		{
			// 出力項目
			RadioButtonOutputAllItems.IsChecked = OutputSettings.OutputAllItems;
			RadioButtonOutputAddedItems.IsChecked = !OutputSettings.OutputAllItems;
			UpdateOutputItemListBoxes();

			// 出力しない項目
			OutputItems[] aOutputItems = (OutputItems[])Enum.GetValues(typeof(OutputItems));
			for (Int32 i = 0; i < aOutputItems.Length - 1; i++)
			{
				if (!OutputSettings.SelectedOutputItems.Contains(aOutputItems[i]))
				{
					ListBoxRemovedItems.Items.Add(YlCommon.OUTPUT_ITEM_NAMES[(Int32)aOutputItems[i]]);
				}
			}

			// 出力する項目
			for (Int32 i = 0; i < OutputSettings.SelectedOutputItems.Count; i++)
			{
				ListBoxAddedItems.Items.Add(YlCommon.OUTPUT_ITEM_NAMES[(Int32)OutputSettings.SelectedOutputItems[i]]);
			}
		}

		// --------------------------------------------------------------------
		// 設定画面表示
		// --------------------------------------------------------------------
		public Nullable<Boolean> ShowDialog()
		{
			if (!IsDialogEnabled())
			{
				return false;
			}

			OutputSettingsWindow aOutputSettingsWindow = new OutputSettingsWindow(this, LogWriter);
			aOutputSettingsWindow.Owner = Owner;
			return aOutputSettingsWindow.ShowDialog();
		}

		// ====================================================================
		// protected 定数
		// ====================================================================

		// ====================================================================
		// protected メンバー変数
		// ====================================================================

		// 実際の出力項目
		protected List<OutputItems> mRuntimeOutputItems;

		// コンポーネント
		protected TabItem TabItemOutputSettings;
		protected RadioButton RadioButtonOutputAllItems;
		protected RadioButton RadioButtonOutputAddedItems;
		protected Label LabelRemovedItems;
		protected ListBox ListBoxRemovedItems;
		protected Button ButtonAddItem;
		protected Button ButtonRemoveItem;
		protected Button ButtonUpItem;
		protected Button ButtonDownItem;
		protected Label LabelAddedItems;
		protected ListBox ListBoxAddedItems;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// テンプレート読み込み
		// --------------------------------------------------------------------
		protected String LoadTemplate(String oFileNameBody)
		{
			return File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + YlCommon.FOLDER_NAME_TEMPLATES
					+ oFileNameBody + Common.FILE_EXT_TPL);
		}

		// --------------------------------------------------------------------
		// コンストラクターでは行えない準備などを実施
		// --------------------------------------------------------------------
		protected virtual void PrepareOutput()
		{
			// OutputSettings.OutputAllItems に基づく設定（コンストラクターでは OutputSettings がロードされていない）
			mRuntimeOutputItems = OutputSettings.RuntimeOutputItems();
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ButtonAddItem_Click(Object oSender, RoutedEventArgs oRoutedEventArgs)
		{
			try
			{
				Int32 aAddItem = Array.IndexOf(YlCommon.OUTPUT_ITEM_NAMES, (String)ListBoxRemovedItems.Items[ListBoxRemovedItems.SelectedIndex]);
				if (aAddItem < 0)
				{
					return;
				}

				ListBoxRemovedItems.Items.RemoveAt(ListBoxRemovedItems.SelectedIndex);
				ListBoxAddedItems.Items.Add(YlCommon.OUTPUT_ITEM_NAMES[aAddItem]);
				ListBoxAddedItems.SelectedIndex = ListBoxAddedItems.Items.Count - 1;
			}
			catch (Exception oExcep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "項目追加ボタンクリック時エラー：\n" + oExcep.Message);
				LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ButtonDownItem_Click(Object oSender, RoutedEventArgs oRoutedEventArgs)
		{
			try
			{
				Int32 aOrgIndex = ListBoxAddedItems.SelectedIndex;
				if (aOrgIndex < 0 || aOrgIndex >= ListBoxAddedItems.Items.Count - 1)
				{
					return;
				}
				String aItem = (String)ListBoxAddedItems.Items[aOrgIndex];
				ListBoxAddedItems.Items.RemoveAt(aOrgIndex);
				ListBoxAddedItems.Items.Insert(aOrgIndex + 1, aItem);
				ListBoxAddedItems.SelectedIndex = aOrgIndex + 1;
			}
			catch (Exception oExcep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "下へボタンクリック時エラー：\n" + oExcep.Message);
				LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ButtonRemoveItem_Click(Object oSender, RoutedEventArgs oRoutedEventArgs)
		{
			try
			{
				Int32 aRemoveItem = Array.IndexOf(YlCommon.OUTPUT_ITEM_NAMES, (String)ListBoxAddedItems.Items[ListBoxAddedItems.SelectedIndex]);
				if (aRemoveItem < 0)
				{
					return;
				}

				ListBoxAddedItems.Items.RemoveAt(ListBoxAddedItems.SelectedIndex);
				ListBoxRemovedItems.Items.Add(YlCommon.OUTPUT_ITEM_NAMES[aRemoveItem]);
				ListBoxRemovedItems.SelectedIndex = ListBoxRemovedItems.Items.Count - 1;
			}
			catch (Exception oExcep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "項目削除ボタンクリック時エラー：\n" + oExcep.Message);
				LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ButtonUpItem_Click(Object oSender, RoutedEventArgs oRoutedEventArgs)
		{
			try
			{
				Int32 aOrgIndex = ListBoxAddedItems.SelectedIndex;
				if (aOrgIndex <= 0)
				{
					return;
				}
				String aItem = (String)ListBoxAddedItems.Items[aOrgIndex];
				ListBoxAddedItems.Items.RemoveAt(aOrgIndex);
				ListBoxAddedItems.Items.Insert(aOrgIndex - 1, aItem);
				ListBoxAddedItems.SelectedIndex = aOrgIndex - 1;
			}
			catch (Exception oExcep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "上へボタンクリック時エラー：\n" + oExcep.Message);
				LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void HyperlinkHelp_RequestNavigate(Object oSender, RequestNavigateEventArgs oRequestNavigateEventArgs)
		{
			try
			{
				YlCommon.ShowHelp(oRequestNavigateEventArgs.Uri.OriginalString);
			}
			catch (Exception oExcep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ListBoxAddedItems_SelectionChanged(Object oSender, SelectionChangedEventArgs oSelectionChangedEventArgs)
		{
			try
			{
				UpdateOutputItemButtons();
			}
			catch (Exception oExcep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "追加項目リスト選択時エラー：\n" + oExcep.Message);
				LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ListBoxRemovedItems_SelectionChanged(Object oSender, SelectionChangedEventArgs oSelectionChangedEventArgs)
		{
			try
			{
				UpdateButtonAddItem();
			}
			catch (Exception oExcep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "削除項目リスト選択時エラー：\n" + oExcep.Message);
				LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void RadioButtonOutputItems_Checked(Object oSender, RoutedEventArgs oRoutedEventArgs)
		{
			try
			{
				UpdateOutputItemListBoxes();
				UpdateButtonAddItem();
				UpdateOutputItemButtons();
			}
			catch (Exception oExcep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "出力項目タイプ選択時エラー：\n" + oExcep.Message);
				LogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 追加ボタン（出力項目）の状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtonAddItem()
		{
			ButtonAddItem.IsEnabled = (Boolean)RadioButtonOutputAddedItems.IsChecked && (ListBoxRemovedItems.SelectedIndex >= 0);
		}

		// --------------------------------------------------------------------
		// 出力項目関連ボタン（追加以外）を更新
		// --------------------------------------------------------------------
		private void UpdateOutputItemButtons()
		{
			ButtonRemoveItem.IsEnabled = (Boolean)RadioButtonOutputAddedItems.IsChecked && (ListBoxAddedItems.SelectedIndex >= 0);
			ButtonUpItem.IsEnabled = (Boolean)RadioButtonOutputAddedItems.IsChecked && ListBoxAddedItems.SelectedIndex > 0;
			ButtonDownItem.IsEnabled = (Boolean)RadioButtonOutputAddedItems.IsChecked
					&& 0 <= ListBoxAddedItems.SelectedIndex && ListBoxAddedItems.SelectedIndex < ListBoxAddedItems.Items.Count - 1;
		}

		// --------------------------------------------------------------------
		// 出力項目リスト等を更新
		// --------------------------------------------------------------------
		private void UpdateOutputItemListBoxes()
		{
			LabelRemovedItems.IsEnabled = (Boolean)RadioButtonOutputAddedItems.IsChecked;
			ListBoxRemovedItems.IsEnabled = (Boolean)RadioButtonOutputAddedItems.IsChecked;
			LabelAddedItems.IsEnabled = (Boolean)RadioButtonOutputAddedItems.IsChecked;
			ListBoxAddedItems.IsEnabled = (Boolean)RadioButtonOutputAddedItems.IsChecked;
		}



	}
	// public class OutputWriter ___END___

}
// namespace YukaLister.Shared ___END___