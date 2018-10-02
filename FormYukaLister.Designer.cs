namespace YukaLister
{
	partial class FormYukaLister
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormYukaLister));
			this.DataGridViewTargetFolders = new System.Windows.Forms.DataGridView();
			this.ColumnAcc = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnFolder = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnSettingsExist = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.LabelYukaListerStatus = new System.Windows.Forms.Label();
			this.LabelIcon = new System.Windows.Forms.Label();
			this.panel5 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.ButtonAddTargetFolder = new System.Windows.Forms.Button();
			this.ButtonRemoveTargetFolder = new System.Windows.Forms.Button();
			this.ButtonFolderSettings = new System.Windows.Forms.Button();
			this.ButtonYukaListerSettings = new System.Windows.Forms.Button();
			this.ButtonHelp = new System.Windows.Forms.Button();
			this.FolderBrowserDialogFolder = new System.Windows.Forms.FolderBrowserDialog();
			this.ContextMenuStripHelp = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ヘルプHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.改訂履歴UToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.バージョン情報AToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.TimerUpdateStatus = new System.Windows.Forms.Timer(this.components);
			this.StatusStripBgStatus = new System.Windows.Forms.StatusStrip();
			this.ToolStripStatusLabelBgStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.ButtonTFounds = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewTargetFolders)).BeginInit();
			this.ContextMenuStripHelp.SuspendLayout();
			this.StatusStripBgStatus.SuspendLayout();
			this.SuspendLayout();
			// 
			// DataGridViewTargetFolders
			// 
			this.DataGridViewTargetFolders.AllowUserToAddRows = false;
			this.DataGridViewTargetFolders.AllowUserToDeleteRows = false;
			this.DataGridViewTargetFolders.AllowUserToResizeRows = false;
			this.DataGridViewTargetFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DataGridViewTargetFolders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.DataGridViewTargetFolders.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnAcc,
            this.ColumnStatus,
            this.ColumnFolder,
            this.ColumnSettingsExist});
			this.DataGridViewTargetFolders.Location = new System.Drawing.Point(16, 80);
			this.DataGridViewTargetFolders.MultiSelect = false;
			this.DataGridViewTargetFolders.Name = "DataGridViewTargetFolders";
			this.DataGridViewTargetFolders.ReadOnly = true;
			this.DataGridViewTargetFolders.RowHeadersVisible = false;
			this.DataGridViewTargetFolders.RowTemplate.Height = 21;
			this.DataGridViewTargetFolders.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.DataGridViewTargetFolders.Size = new System.Drawing.Size(752, 260);
			this.DataGridViewTargetFolders.TabIndex = 6;
			this.DataGridViewTargetFolders.VirtualMode = true;
			this.DataGridViewTargetFolders.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewTargetFolders_CellClick);
			this.DataGridViewTargetFolders.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.DataGridViewTargetFolders_CellValueNeeded);
			this.DataGridViewTargetFolders.SelectionChanged += new System.EventHandler(this.DataGridViewTargetFolders_SelectionChanged);
			// 
			// ColumnAcc
			// 
			this.ColumnAcc.HeaderText = "";
			this.ColumnAcc.Name = "ColumnAcc";
			this.ColumnAcc.ReadOnly = true;
			this.ColumnAcc.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.ColumnAcc.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// ColumnStatus
			// 
			this.ColumnStatus.HeaderText = "状態";
			this.ColumnStatus.Name = "ColumnStatus";
			this.ColumnStatus.ReadOnly = true;
			this.ColumnStatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// ColumnFolder
			// 
			this.ColumnFolder.HeaderText = "フォルダー";
			this.ColumnFolder.Name = "ColumnFolder";
			this.ColumnFolder.ReadOnly = true;
			this.ColumnFolder.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// ColumnSettingsExist
			// 
			this.ColumnSettingsExist.HeaderText = "設定有無";
			this.ColumnSettingsExist.Name = "ColumnSettingsExist";
			this.ColumnSettingsExist.ReadOnly = true;
			this.ColumnSettingsExist.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// LabelYukaListerStatus
			// 
			this.LabelYukaListerStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelYukaListerStatus.Location = new System.Drawing.Point(48, 16);
			this.LabelYukaListerStatus.Name = "LabelYukaListerStatus";
			this.LabelYukaListerStatus.Size = new System.Drawing.Size(496, 28);
			this.LabelYukaListerStatus.TabIndex = 1;
			this.LabelYukaListerStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelIcon
			// 
			this.LabelIcon.ForeColor = System.Drawing.SystemColors.ControlText;
			this.LabelIcon.Location = new System.Drawing.Point(16, 16);
			this.LabelIcon.Name = "LabelIcon";
			this.LabelIcon.Size = new System.Drawing.Size(32, 28);
			this.LabelIcon.TabIndex = 0;
			this.LabelIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panel5
			// 
			this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel5.Location = new System.Drawing.Point(0, 60);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(850, 5);
			this.panel5.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(16, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(142, 12);
			this.label2.TabIndex = 5;
			this.label2.Text = "　ゆかり検索対象フォルダー　";
			// 
			// ButtonAddTargetFolder
			// 
			this.ButtonAddTargetFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonAddTargetFolder.Location = new System.Drawing.Point(16, 352);
			this.ButtonAddTargetFolder.Name = "ButtonAddTargetFolder";
			this.ButtonAddTargetFolder.Size = new System.Drawing.Size(96, 28);
			this.ButtonAddTargetFolder.TabIndex = 7;
			this.ButtonAddTargetFolder.Text = "追加 (&A)";
			this.ButtonAddTargetFolder.UseVisualStyleBackColor = true;
			this.ButtonAddTargetFolder.Click += new System.EventHandler(this.ButtonAddTargetFolder_Click);
			// 
			// ButtonRemoveTargetFolder
			// 
			this.ButtonRemoveTargetFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonRemoveTargetFolder.Location = new System.Drawing.Point(128, 352);
			this.ButtonRemoveTargetFolder.Name = "ButtonRemoveTargetFolder";
			this.ButtonRemoveTargetFolder.Size = new System.Drawing.Size(96, 28);
			this.ButtonRemoveTargetFolder.TabIndex = 8;
			this.ButtonRemoveTargetFolder.Text = "削除 (&D)";
			this.ButtonRemoveTargetFolder.UseVisualStyleBackColor = true;
			this.ButtonRemoveTargetFolder.Click += new System.EventHandler(this.ButtonRemoveTargetFolder_Click);
			// 
			// ButtonFolderSettings
			// 
			this.ButtonFolderSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonFolderSettings.Location = new System.Drawing.Point(656, 352);
			this.ButtonFolderSettings.Name = "ButtonFolderSettings";
			this.ButtonFolderSettings.Size = new System.Drawing.Size(112, 28);
			this.ButtonFolderSettings.TabIndex = 9;
			this.ButtonFolderSettings.Text = "フォルダー設定 (&F)";
			this.ButtonFolderSettings.UseVisualStyleBackColor = true;
			this.ButtonFolderSettings.Click += new System.EventHandler(this.ButtonFolderSettings_Click);
			// 
			// ButtonYukaListerSettings
			// 
			this.ButtonYukaListerSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonYukaListerSettings.Location = new System.Drawing.Point(560, 16);
			this.ButtonYukaListerSettings.Name = "ButtonYukaListerSettings";
			this.ButtonYukaListerSettings.Size = new System.Drawing.Size(96, 28);
			this.ButtonYukaListerSettings.TabIndex = 2;
			this.ButtonYukaListerSettings.Text = "環境設定 (&S)";
			this.ButtonYukaListerSettings.UseVisualStyleBackColor = true;
			this.ButtonYukaListerSettings.Click += new System.EventHandler(this.ButtonYukaListerSettings_Click);
			// 
			// ButtonHelp
			// 
			this.ButtonHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonHelp.Location = new System.Drawing.Point(672, 16);
			this.ButtonHelp.Name = "ButtonHelp";
			this.ButtonHelp.Size = new System.Drawing.Size(96, 28);
			this.ButtonHelp.TabIndex = 3;
			this.ButtonHelp.Text = "ヘルプ (&H)";
			this.ButtonHelp.UseVisualStyleBackColor = true;
			this.ButtonHelp.Click += new System.EventHandler(this.ButtonHelp_Click);
			// 
			// FolderBrowserDialogFolder
			// 
			this.FolderBrowserDialogFolder.RootFolder = System.Environment.SpecialFolder.MyComputer;
			this.FolderBrowserDialogFolder.ShowNewFolderButton = false;
			// 
			// ContextMenuStripHelp
			// 
			this.ContextMenuStripHelp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ヘルプHToolStripMenuItem,
            this.toolStripSeparator1,
            this.改訂履歴UToolStripMenuItem,
            this.バージョン情報AToolStripMenuItem});
			this.ContextMenuStripHelp.Name = "ContextMenuStripHelp";
			this.ContextMenuStripHelp.Size = new System.Drawing.Size(162, 76);
			// 
			// ヘルプHToolStripMenuItem
			// 
			this.ヘルプHToolStripMenuItem.Name = "ヘルプHToolStripMenuItem";
			this.ヘルプHToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.ヘルプHToolStripMenuItem.Text = "ヘルプ (&H)";
			this.ヘルプHToolStripMenuItem.Click += new System.EventHandler(this.ヘルプHToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(158, 6);
			// 
			// 改訂履歴UToolStripMenuItem
			// 
			this.改訂履歴UToolStripMenuItem.Name = "改訂履歴UToolStripMenuItem";
			this.改訂履歴UToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.改訂履歴UToolStripMenuItem.Text = "改訂履歴 (&U)";
			this.改訂履歴UToolStripMenuItem.Click += new System.EventHandler(this.改訂履歴UToolStripMenuItem_Click);
			// 
			// バージョン情報AToolStripMenuItem
			// 
			this.バージョン情報AToolStripMenuItem.Name = "バージョン情報AToolStripMenuItem";
			this.バージョン情報AToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.バージョン情報AToolStripMenuItem.Text = "バージョン情報 (&A)";
			this.バージョン情報AToolStripMenuItem.Click += new System.EventHandler(this.バージョン情報AToolStripMenuItem_Click);
			// 
			// TimerUpdateStatus
			// 
			this.TimerUpdateStatus.Interval = 1000;
			this.TimerUpdateStatus.Tick += new System.EventHandler(this.TimerUpdateStatus_Tick);
			// 
			// StatusStripBgStatus
			// 
			this.StatusStripBgStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripStatusLabelBgStatus});
			this.StatusStripBgStatus.Location = new System.Drawing.Point(0, 397);
			this.StatusStripBgStatus.Name = "StatusStripBgStatus";
			this.StatusStripBgStatus.Size = new System.Drawing.Size(784, 22);
			this.StatusStripBgStatus.TabIndex = 10;
			// 
			// ToolStripStatusLabelBgStatus
			// 
			this.ToolStripStatusLabelBgStatus.Name = "ToolStripStatusLabelBgStatus";
			this.ToolStripStatusLabelBgStatus.Size = new System.Drawing.Size(12, 17);
			this.ToolStripStatusLabelBgStatus.Text = "-";
			// 
			// ButtonTFounds
			// 
			this.ButtonTFounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonTFounds.Location = new System.Drawing.Point(272, 352);
			this.ButtonTFounds.Name = "ButtonTFounds";
			this.ButtonTFounds.Size = new System.Drawing.Size(96, 28);
			this.ButtonTFounds.TabIndex = 11;
			this.ButtonTFounds.Text = "ファイル一覧 (&L)";
			this.ButtonTFounds.UseVisualStyleBackColor = true;
			this.ButtonTFounds.Click += new System.EventHandler(this.ButtonTFounds_Click);
			// 
			// FormYukaLister
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 419);
			this.Controls.Add(this.ButtonTFounds);
			this.Controls.Add(this.StatusStripBgStatus);
			this.Controls.Add(this.ButtonHelp);
			this.Controls.Add(this.ButtonYukaListerSettings);
			this.Controls.Add(this.ButtonFolderSettings);
			this.Controls.Add(this.ButtonRemoveTargetFolder);
			this.Controls.Add(this.ButtonAddTargetFolder);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.panel5);
			this.Controls.Add(this.LabelYukaListerStatus);
			this.Controls.Add(this.LabelIcon);
			this.Controls.Add(this.DataGridViewTargetFolders);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormYukaLister";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormYukaLister_FormClosed);
			this.Load += new System.EventHandler(this.FormYukaLister_Load);
			this.Shown += new System.EventHandler(this.FormYukaLister_Shown);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormYukaLister_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormYukaLister_DragEnter);
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewTargetFolders)).EndInit();
			this.ContextMenuStripHelp.ResumeLayout(false);
			this.StatusStripBgStatus.ResumeLayout(false);
			this.StatusStripBgStatus.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.DataGridView DataGridViewTargetFolders;
		private System.Windows.Forms.Label LabelYukaListerStatus;
		private System.Windows.Forms.Label LabelIcon;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAcc;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnStatus;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFolder;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSettingsExist;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button ButtonAddTargetFolder;
		private System.Windows.Forms.Button ButtonRemoveTargetFolder;
		private System.Windows.Forms.Button ButtonFolderSettings;
		private System.Windows.Forms.Button ButtonYukaListerSettings;
		private System.Windows.Forms.Button ButtonHelp;
		private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialogFolder;
		private System.Windows.Forms.ContextMenuStrip ContextMenuStripHelp;
		private System.Windows.Forms.ToolStripMenuItem ヘルプHToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem 改訂履歴UToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem バージョン情報AToolStripMenuItem;
		private System.Windows.Forms.Timer TimerUpdateStatus;
		private System.Windows.Forms.StatusStrip StatusStripBgStatus;
		private System.Windows.Forms.ToolStripStatusLabel ToolStripStatusLabelBgStatus;
		private System.Windows.Forms.Button ButtonTFounds;
	}
}

