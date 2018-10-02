namespace YukaLister
{
	partial class FormEditMusicInfo
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditMusicInfo));
			this.GroupBoxByFile = new System.Windows.Forms.GroupBox();
			this.LabelTieUpNameRegistered = new System.Windows.Forms.Label();
			this.LabelSongNameRegistered = new System.Windows.Forms.Label();
			this.LabelTieUpName = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.LabelSongName = new System.Windows.Forms.Label();
			this.GroupBoxAlias = new System.Windows.Forms.GroupBox();
			this.LabelTieUpOrigin = new System.Windows.Forms.Label();
			this.LabelSongOrigin = new System.Windows.Forms.Label();
			this.ButtonSearchTieUpOrigin = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.CheckBoxUseTieUpAlias = new System.Windows.Forms.CheckBox();
			this.ButtonSearchSongOrigin = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.CheckBoxUseSongAlias = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.ButtonEditTieUp = new System.Windows.Forms.Button();
			this.ButtonEditSong = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.label13 = new System.Windows.Forms.Label();
			this.LabelPath = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.LinkLabelHelp = new System.Windows.Forms.LinkLabel();
			this.GroupBoxByFile.SuspendLayout();
			this.GroupBoxAlias.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// GroupBoxByFile
			// 
			this.GroupBoxByFile.Controls.Add(this.LabelTieUpNameRegistered);
			this.GroupBoxByFile.Controls.Add(this.LabelSongNameRegistered);
			this.GroupBoxByFile.Controls.Add(this.LabelTieUpName);
			this.GroupBoxByFile.Controls.Add(this.label12);
			this.GroupBoxByFile.Controls.Add(this.label7);
			this.GroupBoxByFile.Controls.Add(this.LabelSongName);
			this.GroupBoxByFile.Location = new System.Drawing.Point(16, 52);
			this.GroupBoxByFile.Name = "GroupBoxByFile";
			this.GroupBoxByFile.Size = new System.Drawing.Size(304, 184);
			this.GroupBoxByFile.TabIndex = 1;
			this.GroupBoxByFile.TabStop = false;
			this.GroupBoxByFile.Text = "ファイル名・フォルダー固定値から取得した名称";
			// 
			// LabelTieUpNameRegistered
			// 
			this.LabelTieUpNameRegistered.AutoEllipsis = true;
			this.LabelTieUpNameRegistered.Location = new System.Drawing.Point(88, 72);
			this.LabelTieUpNameRegistered.Name = "LabelTieUpNameRegistered";
			this.LabelTieUpNameRegistered.Size = new System.Drawing.Size(200, 20);
			this.LabelTieUpNameRegistered.TabIndex = 11;
			this.LabelTieUpNameRegistered.Text = "-";
			this.LabelTieUpNameRegistered.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelSongNameRegistered
			// 
			this.LabelSongNameRegistered.AutoEllipsis = true;
			this.LabelSongNameRegistered.Location = new System.Drawing.Point(88, 152);
			this.LabelSongNameRegistered.Name = "LabelSongNameRegistered";
			this.LabelSongNameRegistered.Size = new System.Drawing.Size(200, 20);
			this.LabelSongNameRegistered.TabIndex = 10;
			this.LabelSongNameRegistered.Text = "-";
			this.LabelSongNameRegistered.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelTieUpName
			// 
			this.LabelTieUpName.AutoEllipsis = true;
			this.LabelTieUpName.Location = new System.Drawing.Point(88, 48);
			this.LabelTieUpName.Name = "LabelTieUpName";
			this.LabelTieUpName.Size = new System.Drawing.Size(200, 20);
			this.LabelTieUpName.TabIndex = 6;
			this.LabelTieUpName.Text = "-";
			this.LabelTieUpName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(16, 48);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(72, 20);
			this.label12.TabIndex = 5;
			this.label12.Text = "タイアップ名：";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 128);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(72, 20);
			this.label7.TabIndex = 1;
			this.label7.Text = "楽曲名：";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LabelSongName
			// 
			this.LabelSongName.AutoEllipsis = true;
			this.LabelSongName.Location = new System.Drawing.Point(88, 128);
			this.LabelSongName.Name = "LabelSongName";
			this.LabelSongName.Size = new System.Drawing.Size(200, 20);
			this.LabelSongName.TabIndex = 2;
			this.LabelSongName.Text = "-";
			this.LabelSongName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// GroupBoxAlias
			// 
			this.GroupBoxAlias.Controls.Add(this.LabelTieUpOrigin);
			this.GroupBoxAlias.Controls.Add(this.LabelSongOrigin);
			this.GroupBoxAlias.Controls.Add(this.ButtonSearchTieUpOrigin);
			this.GroupBoxAlias.Controls.Add(this.label5);
			this.GroupBoxAlias.Controls.Add(this.CheckBoxUseTieUpAlias);
			this.GroupBoxAlias.Controls.Add(this.ButtonSearchSongOrigin);
			this.GroupBoxAlias.Controls.Add(this.label1);
			this.GroupBoxAlias.Controls.Add(this.CheckBoxUseSongAlias);
			this.GroupBoxAlias.Location = new System.Drawing.Point(328, 52);
			this.GroupBoxAlias.Name = "GroupBoxAlias";
			this.GroupBoxAlias.Size = new System.Drawing.Size(384, 184);
			this.GroupBoxAlias.TabIndex = 2;
			this.GroupBoxAlias.TabStop = false;
			this.GroupBoxAlias.Text = "名称を揃える（データベース登録済の名称に名寄せする）";
			// 
			// LabelTieUpOrigin
			// 
			this.LabelTieUpOrigin.AutoEllipsis = true;
			this.LabelTieUpOrigin.Location = new System.Drawing.Point(168, 48);
			this.LabelTieUpOrigin.Name = "LabelTieUpOrigin";
			this.LabelTieUpOrigin.Size = new System.Drawing.Size(200, 20);
			this.LabelTieUpOrigin.TabIndex = 15;
			this.LabelTieUpOrigin.Text = "-";
			this.LabelTieUpOrigin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelSongOrigin
			// 
			this.LabelSongOrigin.AutoEllipsis = true;
			this.LabelSongOrigin.Location = new System.Drawing.Point(168, 128);
			this.LabelSongOrigin.Name = "LabelSongOrigin";
			this.LabelSongOrigin.Size = new System.Drawing.Size(200, 20);
			this.LabelSongOrigin.TabIndex = 14;
			this.LabelSongOrigin.Text = "-";
			this.LabelSongOrigin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ButtonSearchTieUpOrigin
			// 
			this.ButtonSearchTieUpOrigin.Location = new System.Drawing.Point(96, 48);
			this.ButtonSearchTieUpOrigin.Name = "ButtonSearchTieUpOrigin";
			this.ButtonSearchTieUpOrigin.Size = new System.Drawing.Size(64, 20);
			this.ButtonSearchTieUpOrigin.TabIndex = 12;
			this.ButtonSearchTieUpOrigin.Text = "検索";
			this.ButtonSearchTieUpOrigin.UseVisualStyleBackColor = true;
			this.ButtonSearchTieUpOrigin.Click += new System.EventHandler(this.ButtonSearchTieUpOrigin_Click);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(32, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(64, 20);
			this.label5.TabIndex = 10;
			this.label5.Text = "正式名称：";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CheckBoxUseTieUpAlias
			// 
			this.CheckBoxUseTieUpAlias.Location = new System.Drawing.Point(16, 24);
			this.CheckBoxUseTieUpAlias.Name = "CheckBoxUseTieUpAlias";
			this.CheckBoxUseTieUpAlias.Size = new System.Drawing.Size(352, 20);
			this.CheckBoxUseTieUpAlias.TabIndex = 9;
			this.CheckBoxUseTieUpAlias.Text = "タイアップ名を揃える";
			this.CheckBoxUseTieUpAlias.UseVisualStyleBackColor = true;
			this.CheckBoxUseTieUpAlias.CheckedChanged += new System.EventHandler(this.CheckBoxUseTieUpAlias_CheckedChanged);
			// 
			// ButtonSearchSongOrigin
			// 
			this.ButtonSearchSongOrigin.Location = new System.Drawing.Point(96, 128);
			this.ButtonSearchSongOrigin.Name = "ButtonSearchSongOrigin";
			this.ButtonSearchSongOrigin.Size = new System.Drawing.Size(64, 20);
			this.ButtonSearchSongOrigin.TabIndex = 7;
			this.ButtonSearchSongOrigin.Text = "検索";
			this.ButtonSearchSongOrigin.UseVisualStyleBackColor = true;
			this.ButtonSearchSongOrigin.Click += new System.EventHandler(this.ButtonSearchSongOrigin_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(32, 128);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 20);
			this.label1.TabIndex = 2;
			this.label1.Text = "正式名称：";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CheckBoxUseSongAlias
			// 
			this.CheckBoxUseSongAlias.Location = new System.Drawing.Point(16, 104);
			this.CheckBoxUseSongAlias.Name = "CheckBoxUseSongAlias";
			this.CheckBoxUseSongAlias.Size = new System.Drawing.Size(352, 20);
			this.CheckBoxUseSongAlias.TabIndex = 0;
			this.CheckBoxUseSongAlias.Text = "楽曲名を揃える";
			this.CheckBoxUseSongAlias.UseVisualStyleBackColor = true;
			this.CheckBoxUseSongAlias.CheckedChanged += new System.EventHandler(this.CheckBoxUseSongAlias_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.ButtonEditTieUp);
			this.groupBox1.Controls.Add(this.ButtonEditSong);
			this.groupBox1.Location = new System.Drawing.Point(720, 52);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(144, 184);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "データベース編集";
			// 
			// ButtonEditTieUp
			// 
			this.ButtonEditTieUp.Location = new System.Drawing.Point(16, 48);
			this.ButtonEditTieUp.Name = "ButtonEditTieUp";
			this.ButtonEditTieUp.Size = new System.Drawing.Size(112, 20);
			this.ButtonEditTieUp.TabIndex = 9;
			this.ButtonEditTieUp.Text = "タイアップ詳細編集";
			this.ButtonEditTieUp.UseVisualStyleBackColor = true;
			this.ButtonEditTieUp.Click += new System.EventHandler(this.ButtonEditTieUp_Click);
			// 
			// ButtonEditSong
			// 
			this.ButtonEditSong.Location = new System.Drawing.Point(16, 128);
			this.ButtonEditSong.Name = "ButtonEditSong";
			this.ButtonEditSong.Size = new System.Drawing.Size(112, 20);
			this.ButtonEditSong.TabIndex = 8;
			this.ButtonEditSong.Text = "楽曲詳細編集";
			this.ButtonEditSong.UseVisualStyleBackColor = true;
			this.ButtonEditSong.Click += new System.EventHandler(this.ButtonEditSong_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(769, 248);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 8;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(656, 248);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(96, 28);
			this.ButtonOK.TabIndex = 7;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(16, 16);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(56, 20);
			this.label13.TabIndex = 9;
			this.label13.Text = "ファイル：";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LabelPath
			// 
			this.LabelPath.AutoEllipsis = true;
			this.LabelPath.Location = new System.Drawing.Point(72, 16);
			this.LabelPath.Name = "LabelPath";
			this.LabelPath.Size = new System.Drawing.Size(792, 20);
			this.LabelPath.TabIndex = 10;
			this.LabelPath.Text = "-";
			this.LabelPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.AutoEllipsis = true;
			this.label2.Location = new System.Drawing.Point(16, 252);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(496, 20);
			this.label2.TabIndex = 11;
			this.label2.Text = "ファイル名等から取得した名称が間違っている場合でも楽曲情報データベースを適用できるようにします。";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LinkLabelHelp
			// 
			this.LinkLabelHelp.Location = new System.Drawing.Point(512, 252);
			this.LinkLabelHelp.Name = "LinkLabelHelp";
			this.LinkLabelHelp.Size = new System.Drawing.Size(64, 20);
			this.LinkLabelHelp.TabIndex = 12;
			this.LinkLabelHelp.TabStop = true;
			this.LinkLabelHelp.Text = "詳細情報";
			this.LinkLabelHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.LinkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelHelp_LinkClicked);
			// 
			// FormEditMusicInfo
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(880, 291);
			this.Controls.Add(this.LinkLabelHelp);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.LabelPath);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.GroupBoxAlias);
			this.Controls.Add(this.GroupBoxByFile);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormEditMusicInfo";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormEditMusicInfo_FormClosed);
			this.Load += new System.EventHandler(this.FormEditMusicInfo_Load);
			this.Shown += new System.EventHandler(this.FormEditMusicInfo_Shown);
			this.GroupBoxByFile.ResumeLayout(false);
			this.GroupBoxAlias.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox GroupBoxByFile;
		private System.Windows.Forms.Label LabelTieUpName;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label LabelSongName;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.GroupBox GroupBoxAlias;
		private System.Windows.Forms.Button ButtonSearchSongOrigin;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox CheckBoxUseSongAlias;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button ButtonEditSong;
		private System.Windows.Forms.Button ButtonSearchTieUpOrigin;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox CheckBoxUseTieUpAlias;
		private System.Windows.Forms.Button ButtonEditTieUp;
		private System.Windows.Forms.Label LabelTieUpNameRegistered;
		private System.Windows.Forms.Label LabelSongNameRegistered;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label LabelPath;
		private System.Windows.Forms.Label LabelTieUpOrigin;
		private System.Windows.Forms.Label LabelSongOrigin;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel LinkLabelHelp;
	}
}