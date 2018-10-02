// ============================================================================
// 
// リスト出力用基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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

		// 環境設定
		//public NicoKaraListerSettings NicoKaraListerSettings { get; set; }

		// 出力設定
		public OutputSettings OutputSettings { get; set; }

		// ログ
		public LogWriter LogWriter { get; set; }

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
			OutputSettings.OutputAllItems = RadioButtonOutputAllItems.Checked;

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
		public virtual List<TabPage> DialogTabPages()
		{
			List<TabPage> aTabPages = new List<TabPage>();

			// TabPageOutputSettings
			TabPageOutputSettings = new TabPage();
			TabPageOutputSettings.BackColor = SystemColors.Control;
			TabPageOutputSettings.Location = new Point(4, 22);
			TabPageOutputSettings.Padding = new Padding(3);
			TabPageOutputSettings.Size = new Size(456, 386);
			TabPageOutputSettings.Text = "基本設定";

			// LabelOutputItem
			LabelOutputItem = new Label();
			LabelOutputItem.Location = new Point(16, 16);
			LabelOutputItem.Size = new Size(72, 20);
			LabelOutputItem.Text = "出力項目：";
			LabelOutputItem.TextAlign = ContentAlignment.MiddleLeft;
			TabPageOutputSettings.Controls.Add(LabelOutputItem);

			// RadioButtonOutputAllItems
			RadioButtonOutputAllItems = new RadioButton();
			RadioButtonOutputAllItems.Location = new Point(88, 16);
			RadioButtonOutputAllItems.Size = new Size(96, 20);
			RadioButtonOutputAllItems.TabStop = true;
			RadioButtonOutputAllItems.Text = "すべて (&L)";
			RadioButtonOutputAllItems.UseVisualStyleBackColor = true;
			RadioButtonOutputAllItems.CheckedChanged += new EventHandler(RadioButtonOutputItems_CheckedChanged);
			TabPageOutputSettings.Controls.Add(RadioButtonOutputAllItems);

			// RadioButtonOutputAddedItems
			RadioButtonOutputAddedItems = new RadioButton();
			RadioButtonOutputAddedItems.Location = new Point(184, 16);
			RadioButtonOutputAddedItems.Size = new Size(168, 20);
			RadioButtonOutputAddedItems.TabStop = true;
			RadioButtonOutputAddedItems.Text = "以下で追加した項目のみ (&O)";
			RadioButtonOutputAddedItems.UseVisualStyleBackColor = true;
			RadioButtonOutputAddedItems.CheckedChanged += new EventHandler(RadioButtonOutputItems_CheckedChanged);
			TabPageOutputSettings.Controls.Add(RadioButtonOutputAddedItems);

			// LinkLabelHelp
			LinkLabelHelp = new LinkLabel();
			LinkLabelHelp.Location = new Point(384, 16);
			LinkLabelHelp.Size = new Size(64, 20);
			LinkLabelHelp.TabStop = true;
			LinkLabelHelp.Text = "詳細情報";
			LinkLabelHelp.TextAlign = ContentAlignment.MiddleRight;
			LinkLabelHelp.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabelHelp_LinkClicked);

			// LabelRemovedItems
			LabelRemovedItems = new Label();
			LabelRemovedItems.Location = new Point(32, 44);
			LabelRemovedItems.Size = new Size(152, 20);
			LabelRemovedItems.Text = "（出力されない項目）";
			LabelRemovedItems.TextAlign = ContentAlignment.MiddleLeft;
			TabPageOutputSettings.Controls.Add(LabelRemovedItems);

			// ListBoxRemovedItems
			ListBoxRemovedItems = new ListBox();
			ListBoxRemovedItems.FormattingEnabled = true;
			ListBoxRemovedItems.ItemHeight = 12;
			ListBoxRemovedItems.Location = new Point(32, 64);
			ListBoxRemovedItems.Size = new Size(152, 160);
			ListBoxRemovedItems.SelectedIndexChanged += new EventHandler(ListBoxRemovedItems_SelectedIndexChanged);
			TabPageOutputSettings.Controls.Add(ListBoxRemovedItems);

			// ButtonAddItem
			ButtonAddItem = new Button();
			ButtonAddItem.Location = new Point(192, 72);
			ButtonAddItem.Size = new Size(96, 28);
			ButtonAddItem.Text = "→ 追加 (&D)";
			ButtonAddItem.UseVisualStyleBackColor = true;
			ButtonAddItem.Click += new EventHandler(ButtonAddItem_Click);
			TabPageOutputSettings.Controls.Add(ButtonAddItem);

			// ButtonRemoveItem
			ButtonRemoveItem = new Button();
			ButtonRemoveItem.Location = new Point(192, 108);
			ButtonRemoveItem.Size = new Size(96, 28);
			ButtonRemoveItem.Text = "× 削除 (&M)";
			ButtonRemoveItem.UseVisualStyleBackColor = true;
			ButtonRemoveItem.Click += new EventHandler(ButtonRemoveItem_Click);
			TabPageOutputSettings.Controls.Add(ButtonRemoveItem);

			// ButtonUpItem
			ButtonUpItem = new Button();
			ButtonUpItem.Location = new Point(192, 152);
			ButtonUpItem.Size = new Size(96, 28);
			ButtonUpItem.Text = "↑ 上へ (&U)";
			ButtonUpItem.UseVisualStyleBackColor = true;
			ButtonUpItem.Click += new EventHandler(ButtonUpItem_Click);
			TabPageOutputSettings.Controls.Add(ButtonUpItem);

			// ButtonDownItem
			ButtonDownItem = new Button();
			ButtonDownItem.Location = new Point(192, 188);
			ButtonDownItem.Size = new Size(96, 28);
			ButtonDownItem.Text = "↓ 下へ (&W)";
			ButtonDownItem.UseVisualStyleBackColor = true;
			ButtonDownItem.Click += new EventHandler(ButtonDownItem_Click);
			TabPageOutputSettings.Controls.Add(ButtonDownItem);

			// LabelAddedItems
			LabelAddedItems = new Label();
			LabelAddedItems.Location = new Point(296, 44);
			LabelAddedItems.Size = new Size(152, 20);
			LabelAddedItems.Text = "（出力される項目）";
			LabelAddedItems.TextAlign = ContentAlignment.MiddleLeft;
			TabPageOutputSettings.Controls.Add(LabelAddedItems);

			// ListBoxAddedItems
			ListBoxAddedItems = new ListBox();
			ListBoxAddedItems.FormattingEnabled = true;
			ListBoxAddedItems.ItemHeight = 12;
			ListBoxAddedItems.Location = new Point(296, 64);
			ListBoxAddedItems.Size = new Size(152, 160);
			ListBoxAddedItems.SelectedIndexChanged += new EventHandler(ListBoxAddedItems_SelectedIndexChanged);
			TabPageOutputSettings.Controls.Add(ListBoxAddedItems);

			aTabPages.Add(TabPageOutputSettings);

			return aTabPages;
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
			RadioButtonOutputAllItems.Checked = OutputSettings.OutputAllItems;
			RadioButtonOutputAddedItems.Checked = !OutputSettings.OutputAllItems;
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
		public DialogResult ShowDialog(IWin32Window oWindow)
		{
			if (!IsDialogEnabled())
			{
				return DialogResult.Cancel;
			}
			
			using (FormOutputSettings aFormOutputSettings = new FormOutputSettings(this, LogWriter))
			{
				return aFormOutputSettings.ShowDialog(oWindow);
			}
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
		protected TabPage TabPageOutputSettings;
		protected Label LabelOutputItem;
		protected RadioButton RadioButtonOutputAllItems;
		protected RadioButton RadioButtonOutputAddedItems;
		protected LinkLabel LinkLabelHelp;
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
			return File.ReadAllText(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + YlCommon.FOLDER_NAME_TEMPLATES
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
		private void ButtonAddItem_Click(Object oSender, EventArgs oEventArgs)
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
		private void ButtonDownItem_Click(Object oSender, EventArgs oEventArgs)
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
		private void ButtonRemoveItem_Click(Object oSender, EventArgs oEventArgs)
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
		private void ButtonUpItem_Click(Object oSender, EventArgs oEventArgs)
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
		private void LinkLabelHelp_LinkClicked(Object oSender, LinkLabelLinkClickedEventArgs oEventArgs)
		{
			try
			{
				YlCommon.ShowHelp("Kihonsetteitab");
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
		private void ListBoxAddedItems_SelectedIndexChanged(Object oSender, EventArgs oEventArgs)
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
		private void ListBoxRemovedItems_SelectedIndexChanged(Object oSender, EventArgs oEventArgs)
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
		private void RadioButtonOutputItems_CheckedChanged(Object oSender, EventArgs oEventArgs)
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
			ButtonAddItem.Enabled = RadioButtonOutputAddedItems.Checked && (ListBoxRemovedItems.SelectedIndex >= 0);
		}

		// --------------------------------------------------------------------
		// 出力項目関連ボタン（追加以外）を更新
		// --------------------------------------------------------------------
		private void UpdateOutputItemButtons()
		{
			ButtonRemoveItem.Enabled = RadioButtonOutputAddedItems.Checked && (ListBoxAddedItems.SelectedIndex >= 0);
			ButtonUpItem.Enabled = RadioButtonOutputAddedItems.Checked && ListBoxAddedItems.SelectedIndex > 0;
			ButtonDownItem.Enabled = RadioButtonOutputAddedItems.Checked
					&& 0 <= ListBoxAddedItems.SelectedIndex && ListBoxAddedItems.SelectedIndex < ListBoxAddedItems.Items.Count - 1;
		}

		// --------------------------------------------------------------------
		// 出力項目リスト等を更新
		// --------------------------------------------------------------------
		private void UpdateOutputItemListBoxes()
		{
			LabelRemovedItems.Enabled = RadioButtonOutputAddedItems.Checked;
			ListBoxRemovedItems.Enabled = RadioButtonOutputAddedItems.Checked;
			LabelAddedItems.Enabled = RadioButtonOutputAddedItems.Checked;
			ListBoxAddedItems.Enabled = RadioButtonOutputAddedItems.Checked;
		}



	}
	// public class OutputWriter ___END___

}
// namespace YukaLister.Shared ___END___