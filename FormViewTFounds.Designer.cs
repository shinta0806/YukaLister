namespace YukaLister
{
	partial class FormViewTFounds
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormViewTFounds));
			this.DataGridViewList = new System.Windows.Forms.DataGridView();
			this.ButtonEditMusicInfo = new System.Windows.Forms.Button();
			this.ButtonFolderSettings = new System.Windows.Forms.Button();
			this.ButtonFindNormalCell = new System.Windows.Forms.Button();
			this.ButtonFindEmptyCell = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonFind = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.LinkLabelHelp = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewList)).BeginInit();
			this.SuspendLayout();
			// 
			// DataGridViewList
			// 
			this.DataGridViewList.AllowUserToAddRows = false;
			this.DataGridViewList.AllowUserToDeleteRows = false;
			this.DataGridViewList.AllowUserToResizeRows = false;
			this.DataGridViewList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DataGridViewList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.DataGridViewList.Location = new System.Drawing.Point(16, 40);
			this.DataGridViewList.MultiSelect = false;
			this.DataGridViewList.Name = "DataGridViewList";
			this.DataGridViewList.ReadOnly = true;
			this.DataGridViewList.RowHeadersVisible = false;
			this.DataGridViewList.RowTemplate.Height = 21;
			this.DataGridViewList.Size = new System.Drawing.Size(952, 468);
			this.DataGridViewList.TabIndex = 0;
			this.DataGridViewList.VirtualMode = true;
			this.DataGridViewList.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewList_CellDoubleClick);
			this.DataGridViewList.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.DataGridViewList_CellValueNeeded);
			this.DataGridViewList.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridViewList_ColumnHeaderMouseClick);
			// 
			// ButtonEditMusicInfo
			// 
			this.ButtonEditMusicInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonEditMusicInfo.Location = new System.Drawing.Point(504, 520);
			this.ButtonEditMusicInfo.Name = "ButtonEditMusicInfo";
			this.ButtonEditMusicInfo.Size = new System.Drawing.Size(112, 28);
			this.ButtonEditMusicInfo.TabIndex = 7;
			this.ButtonEditMusicInfo.Text = "編集 (&E)";
			this.ButtonEditMusicInfo.UseVisualStyleBackColor = true;
			this.ButtonEditMusicInfo.Click += new System.EventHandler(this.ButtonEditMusicInfo_Click);
			// 
			// ButtonFolderSettings
			// 
			this.ButtonFolderSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonFolderSettings.Location = new System.Drawing.Point(376, 520);
			this.ButtonFolderSettings.Name = "ButtonFolderSettings";
			this.ButtonFolderSettings.Size = new System.Drawing.Size(112, 28);
			this.ButtonFolderSettings.TabIndex = 8;
			this.ButtonFolderSettings.Text = "フォルダー設定 (&S)";
			this.ButtonFolderSettings.UseVisualStyleBackColor = true;
			this.ButtonFolderSettings.Click += new System.EventHandler(this.ButtonFolderSettings_Click);
			// 
			// ButtonFindNormalCell
			// 
			this.ButtonFindNormalCell.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonFindNormalCell.BackgroundImage = global::YukaLister.Properties.Resources.ButtonFindNormalCell;
			this.ButtonFindNormalCell.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.ButtonFindNormalCell.Location = new System.Drawing.Point(240, 520);
			this.ButtonFindNormalCell.Name = "ButtonFindNormalCell";
			this.ButtonFindNormalCell.Size = new System.Drawing.Size(96, 28);
			this.ButtonFindNormalCell.TabIndex = 10;
			this.ButtonFindNormalCell.Text = "　　　↓検索 (&H)";
			this.ButtonFindNormalCell.UseVisualStyleBackColor = true;
			this.ButtonFindNormalCell.Click += new System.EventHandler(this.ButtonFindNormalCell_Click);
			// 
			// ButtonFindEmptyCell
			// 
			this.ButtonFindEmptyCell.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonFindEmptyCell.BackgroundImage = global::YukaLister.Properties.Resources.ButtonFindEmptyCell;
			this.ButtonFindEmptyCell.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.ButtonFindEmptyCell.Location = new System.Drawing.Point(128, 520);
			this.ButtonFindEmptyCell.Name = "ButtonFindEmptyCell";
			this.ButtonFindEmptyCell.Size = new System.Drawing.Size(96, 28);
			this.ButtonFindEmptyCell.TabIndex = 9;
			this.ButtonFindEmptyCell.Text = "　　　↓検索 (&G)";
			this.ButtonFindEmptyCell.UseVisualStyleBackColor = true;
			this.ButtonFindEmptyCell.Click += new System.EventHandler(this.ButtonFindEmptyCell_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(872, 520);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 12;
			this.ButtonCancel.Text = "閉じる";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonFind
			// 
			this.ButtonFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonFind.Location = new System.Drawing.Point(16, 520);
			this.ButtonFind.Name = "ButtonFind";
			this.ButtonFind.Size = new System.Drawing.Size(96, 28);
			this.ButtonFind.TabIndex = 13;
			this.ButtonFind.Text = "検索 (&F)";
			this.ButtonFind.UseVisualStyleBackColor = true;
			this.ButtonFind.Click += new System.EventHandler(this.ButtonFind_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(403, 12);
			this.label1.TabIndex = 14;
			this.label1.Text = "検索対象のすべてのファイルについて、リストに表示される内容を一覧で確認できます。";
			// 
			// LinkLabelHelp
			// 
			this.LinkLabelHelp.AutoSize = true;
			this.LinkLabelHelp.Location = new System.Drawing.Point(424, 16);
			this.LinkLabelHelp.Name = "LinkLabelHelp";
			this.LinkLabelHelp.Size = new System.Drawing.Size(53, 12);
			this.LinkLabelHelp.TabIndex = 15;
			this.LinkLabelHelp.TabStop = true;
			this.LinkLabelHelp.Text = "詳細情報";
			this.LinkLabelHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.LinkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelHelp_LinkClicked);
			// 
			// FormViewTFounds
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(984, 561);
			this.Controls.Add(this.LinkLabelHelp);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ButtonFind);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonFindNormalCell);
			this.Controls.Add(this.ButtonFindEmptyCell);
			this.Controls.Add(this.ButtonFolderSettings);
			this.Controls.Add(this.ButtonEditMusicInfo);
			this.Controls.Add(this.DataGridViewList);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.Name = "FormViewTFounds";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormViewTFounds_FormClosed);
			this.Load += new System.EventHandler(this.FormViewTFounds_Load);
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewList)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView DataGridViewList;
		private System.Windows.Forms.Button ButtonEditMusicInfo;
		private System.Windows.Forms.Button ButtonFolderSettings;
		private System.Windows.Forms.Button ButtonFindEmptyCell;
		private System.Windows.Forms.Button ButtonFindNormalCell;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonFind;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel LinkLabelHelp;
	}
}