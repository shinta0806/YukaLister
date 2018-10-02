namespace YukaLister
{
	partial class FormEditTieUp
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditTieUp));
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label21 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.TextBoxKeyword = new System.Windows.Forms.TextBox();
			this.ButtonSelectCategory = new System.Windows.Forms.Button();
			this.label20 = new System.Windows.Forms.Label();
			this.TextBoxReleaseDay = new System.Windows.Forms.TextBox();
			this.label19 = new System.Windows.Forms.Label();
			this.TextBoxReleaseMonth = new System.Windows.Forms.TextBox();
			this.label18 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.TextBoxReleaseYear = new System.Windows.Forms.TextBox();
			this.CheckBoxMaker = new System.Windows.Forms.CheckBox();
			this.ButtonEditMaker = new System.Windows.Forms.Button();
			this.ButtonSearchMaker = new System.Windows.Forms.Button();
			this.LabelMaker = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.CheckBoxTieUpGroup = new System.Windows.Forms.CheckBox();
			this.ButtonEditTieUpGroup = new System.Windows.Forms.Button();
			this.ButtonSearchTieUpGroup = new System.Windows.Forms.Button();
			this.LabelTieUpGroup = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.TextBoxAgeLimit = new System.Windows.Forms.TextBox();
			this.CheckBoxCategory = new System.Windows.Forms.CheckBox();
			this.LabelCategory = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.TextBoxRuby = new System.Windows.Forms.TextBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.TextBoxName = new System.Windows.Forms.TextBox();
			this.LabelIdInfo = new System.Windows.Forms.Label();
			this.ComboBoxId = new System.Windows.Forms.ComboBox();
			this.label22 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.ButtonSelectAgeLimit = new System.Windows.Forms.Button();
			this.ContextMenuStripCategories = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ContextMenuStripAgeLimits = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.SuspendLayout();
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(872, 284);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 121;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(759, 284);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(96, 28);
			this.ButtonOK.TabIndex = 120;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Location = new System.Drawing.Point(0, 268);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1084, 5);
			this.panel1.TabIndex = 118;
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(560, 236);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(408, 20);
			this.label21.TabIndex = 117;
			this.label21.Text = "キーワード、コメントなど。複数入力する際は、半角カンマ「 , 」で区切って下さい。";
			this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(456, 208);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(104, 20);
			this.label16.TabIndex = 116;
			this.label16.Text = "検索ワード (&W)：";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxKeyword
			// 
			this.TextBoxKeyword.Location = new System.Drawing.Point(560, 208);
			this.TextBoxKeyword.Name = "TextBoxKeyword";
			this.TextBoxKeyword.Size = new System.Drawing.Size(408, 19);
			this.TextBoxKeyword.TabIndex = 115;
			// 
			// ButtonSelectCategory
			// 
			this.ButtonSelectCategory.Location = new System.Drawing.Point(136, 152);
			this.ButtonSelectCategory.Name = "ButtonSelectCategory";
			this.ButtonSelectCategory.Size = new System.Drawing.Size(64, 20);
			this.ButtonSelectCategory.TabIndex = 114;
			this.ButtonSelectCategory.Text = "選択";
			this.ButtonSelectCategory.UseVisualStyleBackColor = true;
			this.ButtonSelectCategory.Click += new System.EventHandler(this.ButtonSelectCategory_Click);
			// 
			// label20
			// 
			this.label20.Location = new System.Drawing.Point(736, 180);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(24, 20);
			this.label20.TabIndex = 113;
			this.label20.Text = "日";
			this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TextBoxReleaseDay
			// 
			this.TextBoxReleaseDay.Location = new System.Drawing.Point(704, 180);
			this.TextBoxReleaseDay.Name = "TextBoxReleaseDay";
			this.TextBoxReleaseDay.Size = new System.Drawing.Size(32, 19);
			this.TextBoxReleaseDay.TabIndex = 112;
			this.TextBoxReleaseDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(680, 180);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(24, 20);
			this.label19.TabIndex = 111;
			this.label19.Text = "月";
			this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TextBoxReleaseMonth
			// 
			this.TextBoxReleaseMonth.Location = new System.Drawing.Point(648, 180);
			this.TextBoxReleaseMonth.Name = "TextBoxReleaseMonth";
			this.TextBoxReleaseMonth.Size = new System.Drawing.Size(32, 19);
			this.TextBoxReleaseMonth.TabIndex = 110;
			this.TextBoxReleaseMonth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(624, 180);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(24, 20);
			this.label18.TabIndex = 109;
			this.label18.Text = "年";
			this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(456, 180);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(104, 20);
			this.label17.TabIndex = 108;
			this.label17.Text = "リリース日 (&R)：";
			this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxReleaseYear
			// 
			this.TextBoxReleaseYear.Location = new System.Drawing.Point(560, 180);
			this.TextBoxReleaseYear.Name = "TextBoxReleaseYear";
			this.TextBoxReleaseYear.Size = new System.Drawing.Size(64, 19);
			this.TextBoxReleaseYear.TabIndex = 107;
			this.TextBoxReleaseYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CheckBoxMaker
			// 
			this.CheckBoxMaker.Location = new System.Drawing.Point(560, 68);
			this.CheckBoxMaker.Name = "CheckBoxMaker";
			this.CheckBoxMaker.Size = new System.Drawing.Size(16, 20);
			this.CheckBoxMaker.TabIndex = 101;
			this.CheckBoxMaker.UseVisualStyleBackColor = true;
			this.CheckBoxMaker.CheckedChanged += new System.EventHandler(this.CheckBoxMaker_CheckedChanged);
			// 
			// ButtonEditMaker
			// 
			this.ButtonEditMaker.Location = new System.Drawing.Point(856, 68);
			this.ButtonEditMaker.Name = "ButtonEditMaker";
			this.ButtonEditMaker.Size = new System.Drawing.Size(112, 20);
			this.ButtonEditMaker.TabIndex = 100;
			this.ButtonEditMaker.Text = "制作会社詳細編集";
			this.ButtonEditMaker.UseVisualStyleBackColor = true;
			this.ButtonEditMaker.Click += new System.EventHandler(this.ButtonEditMaker_Click);
			// 
			// ButtonSearchMaker
			// 
			this.ButtonSearchMaker.Location = new System.Drawing.Point(584, 68);
			this.ButtonSearchMaker.Name = "ButtonSearchMaker";
			this.ButtonSearchMaker.Size = new System.Drawing.Size(64, 20);
			this.ButtonSearchMaker.TabIndex = 99;
			this.ButtonSearchMaker.Text = "検索";
			this.ButtonSearchMaker.UseVisualStyleBackColor = true;
			this.ButtonSearchMaker.Click += new System.EventHandler(this.ButtonSearchMaker_Click);
			// 
			// LabelMaker
			// 
			this.LabelMaker.Location = new System.Drawing.Point(656, 68);
			this.LabelMaker.Name = "LabelMaker";
			this.LabelMaker.Size = new System.Drawing.Size(200, 20);
			this.LabelMaker.TabIndex = 98;
			this.LabelMaker.Text = "-";
			this.LabelMaker.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(456, 68);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(104, 20);
			this.label13.TabIndex = 97;
			this.label13.Text = "制作会社あり (&C)：";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CheckBoxTieUpGroup
			// 
			this.CheckBoxTieUpGroup.Location = new System.Drawing.Point(560, 124);
			this.CheckBoxTieUpGroup.Name = "CheckBoxTieUpGroup";
			this.CheckBoxTieUpGroup.Size = new System.Drawing.Size(16, 20);
			this.CheckBoxTieUpGroup.TabIndex = 91;
			this.CheckBoxTieUpGroup.UseVisualStyleBackColor = true;
			this.CheckBoxTieUpGroup.CheckedChanged += new System.EventHandler(this.CheckBoxTieUpGroup_CheckedChanged);
			// 
			// ButtonEditTieUpGroup
			// 
			this.ButtonEditTieUpGroup.Location = new System.Drawing.Point(856, 124);
			this.ButtonEditTieUpGroup.Name = "ButtonEditTieUpGroup";
			this.ButtonEditTieUpGroup.Size = new System.Drawing.Size(112, 20);
			this.ButtonEditTieUpGroup.TabIndex = 90;
			this.ButtonEditTieUpGroup.Text = "シリーズ詳細編集";
			this.ButtonEditTieUpGroup.UseVisualStyleBackColor = true;
			this.ButtonEditTieUpGroup.Click += new System.EventHandler(this.ButtonEditTieUpGroup_Click);
			// 
			// ButtonSearchTieUpGroup
			// 
			this.ButtonSearchTieUpGroup.Location = new System.Drawing.Point(584, 124);
			this.ButtonSearchTieUpGroup.Name = "ButtonSearchTieUpGroup";
			this.ButtonSearchTieUpGroup.Size = new System.Drawing.Size(64, 20);
			this.ButtonSearchTieUpGroup.TabIndex = 89;
			this.ButtonSearchTieUpGroup.Text = "検索";
			this.ButtonSearchTieUpGroup.UseVisualStyleBackColor = true;
			this.ButtonSearchTieUpGroup.Click += new System.EventHandler(this.ButtonSearchTieUpGroup_Click);
			// 
			// LabelTieUpGroup
			// 
			this.LabelTieUpGroup.Location = new System.Drawing.Point(656, 124);
			this.LabelTieUpGroup.Name = "LabelTieUpGroup";
			this.LabelTieUpGroup.Size = new System.Drawing.Size(200, 20);
			this.LabelTieUpGroup.TabIndex = 88;
			this.LabelTieUpGroup.Text = "-";
			this.LabelTieUpGroup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(456, 124);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(104, 20);
			this.label9.TabIndex = 87;
			this.label9.Text = "シリーズあり (&S)：";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 180);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(96, 20);
			this.label7.TabIndex = 85;
			this.label7.Text = "年齢制限 (&A)：";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxAgeLimit
			// 
			this.TextBoxAgeLimit.Location = new System.Drawing.Point(208, 180);
			this.TextBoxAgeLimit.Name = "TextBoxAgeLimit";
			this.TextBoxAgeLimit.Size = new System.Drawing.Size(64, 19);
			this.TextBoxAgeLimit.TabIndex = 84;
			this.TextBoxAgeLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CheckBoxCategory
			// 
			this.CheckBoxCategory.Location = new System.Drawing.Point(112, 152);
			this.CheckBoxCategory.Name = "CheckBoxCategory";
			this.CheckBoxCategory.Size = new System.Drawing.Size(16, 20);
			this.CheckBoxCategory.TabIndex = 83;
			this.CheckBoxCategory.UseVisualStyleBackColor = true;
			this.CheckBoxCategory.CheckedChanged += new System.EventHandler(this.CheckBoxCategory_CheckedChanged);
			// 
			// LabelCategory
			// 
			this.LabelCategory.Location = new System.Drawing.Point(208, 152);
			this.LabelCategory.Name = "LabelCategory";
			this.LabelCategory.Size = new System.Drawing.Size(200, 20);
			this.LabelCategory.TabIndex = 81;
			this.LabelCategory.Text = "-";
			this.LabelCategory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 152);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(96, 20);
			this.label6.TabIndex = 80;
			this.label6.Text = "カテゴリーあり (&C)：";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 68);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 20);
			this.label2.TabIndex = 75;
			this.label2.Text = "フリガナ (&F)：";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxRuby
			// 
			this.TextBoxRuby.Location = new System.Drawing.Point(112, 68);
			this.TextBoxRuby.Name = "TextBoxRuby";
			this.TextBoxRuby.Size = new System.Drawing.Size(296, 19);
			this.TextBoxRuby.TabIndex = 74;
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Location = new System.Drawing.Point(0, 52);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1084, 5);
			this.panel2.TabIndex = 73;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 20);
			this.label1.TabIndex = 72;
			this.label1.Text = "タイアップ名 (&N)：";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxName
			// 
			this.TextBoxName.Location = new System.Drawing.Point(112, 96);
			this.TextBoxName.Name = "TextBoxName";
			this.TextBoxName.Size = new System.Drawing.Size(296, 19);
			this.TextBoxName.TabIndex = 71;
			this.TextBoxName.Leave += new System.EventHandler(this.TextBoxName_Leave);
			// 
			// LabelIdInfo
			// 
			this.LabelIdInfo.Location = new System.Drawing.Point(416, 16);
			this.LabelIdInfo.Name = "LabelIdInfo";
			this.LabelIdInfo.Size = new System.Drawing.Size(200, 20);
			this.LabelIdInfo.TabIndex = 70;
			this.LabelIdInfo.Text = "-";
			this.LabelIdInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ComboBoxId
			// 
			this.ComboBoxId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboBoxId.FormattingEnabled = true;
			this.ComboBoxId.Location = new System.Drawing.Point(112, 16);
			this.ComboBoxId.Name = "ComboBoxId";
			this.ComboBoxId.Size = new System.Drawing.Size(296, 20);
			this.ComboBoxId.TabIndex = 69;
			this.ComboBoxId.SelectedIndexChanged += new System.EventHandler(this.ComboBoxId_SelectedIndexChanged);
			// 
			// label22
			// 
			this.label22.Location = new System.Drawing.Point(16, 16);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(96, 20);
			this.label22.TabIndex = 68;
			this.label22.Text = "タイアップ ID：";
			this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(280, 180);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(128, 20);
			this.label4.TabIndex = 122;
			this.label4.Text = "才以上対象";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ButtonSelectAgeLimit
			// 
			this.ButtonSelectAgeLimit.Location = new System.Drawing.Point(136, 180);
			this.ButtonSelectAgeLimit.Name = "ButtonSelectAgeLimit";
			this.ButtonSelectAgeLimit.Size = new System.Drawing.Size(64, 20);
			this.ButtonSelectAgeLimit.TabIndex = 123;
			this.ButtonSelectAgeLimit.Text = "選択";
			this.ButtonSelectAgeLimit.UseVisualStyleBackColor = true;
			this.ButtonSelectAgeLimit.Click += new System.EventHandler(this.ButtonSelectAgeLimit_Click);
			// 
			// ContextMenuStripCategories
			// 
			this.ContextMenuStripCategories.Name = "ContextMenuStripCategories";
			this.ContextMenuStripCategories.Size = new System.Drawing.Size(61, 4);
			// 
			// ContextMenuStripAgeLimits
			// 
			this.ContextMenuStripAgeLimits.Name = "ContextMenuStripAgeLimits";
			this.ContextMenuStripAgeLimits.Size = new System.Drawing.Size(61, 4);
			// 
			// FormEditTieUp
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(983, 326);
			this.Controls.Add(this.ButtonSelectAgeLimit);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label21);
			this.Controls.Add(this.label16);
			this.Controls.Add(this.TextBoxKeyword);
			this.Controls.Add(this.ButtonSelectCategory);
			this.Controls.Add(this.label20);
			this.Controls.Add(this.TextBoxReleaseDay);
			this.Controls.Add(this.label19);
			this.Controls.Add(this.TextBoxReleaseMonth);
			this.Controls.Add(this.label18);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.TextBoxReleaseYear);
			this.Controls.Add(this.CheckBoxMaker);
			this.Controls.Add(this.ButtonEditMaker);
			this.Controls.Add(this.ButtonSearchMaker);
			this.Controls.Add(this.LabelMaker);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.CheckBoxTieUpGroup);
			this.Controls.Add(this.ButtonEditTieUpGroup);
			this.Controls.Add(this.ButtonSearchTieUpGroup);
			this.Controls.Add(this.LabelTieUpGroup);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.TextBoxAgeLimit);
			this.Controls.Add(this.CheckBoxCategory);
			this.Controls.Add(this.LabelCategory);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.TextBoxRuby);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.TextBoxName);
			this.Controls.Add(this.LabelIdInfo);
			this.Controls.Add(this.ComboBoxId);
			this.Controls.Add(this.label22);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormEditTieUp";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormEditTieUp_FormClosed);
			this.Load += new System.EventHandler(this.FormEditTieUp_Load);
			this.Shown += new System.EventHandler(this.FormEditTieUp_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Button ButtonSelectCategory;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Button ButtonEditMaker;
		private System.Windows.Forms.Button ButtonSearchMaker;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Button ButtonEditTieUpGroup;
		private System.Windows.Forms.Button ButtonSearchTieUpGroup;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button ButtonSelectAgeLimit;
		internal System.Windows.Forms.TextBox TextBoxKeyword;
		internal System.Windows.Forms.TextBox TextBoxReleaseDay;
		internal System.Windows.Forms.TextBox TextBoxReleaseMonth;
		internal System.Windows.Forms.TextBox TextBoxReleaseYear;
		internal System.Windows.Forms.CheckBox CheckBoxMaker;
		internal System.Windows.Forms.Label LabelMaker;
		internal System.Windows.Forms.CheckBox CheckBoxTieUpGroup;
		internal System.Windows.Forms.Label LabelTieUpGroup;
		internal System.Windows.Forms.TextBox TextBoxAgeLimit;
		internal System.Windows.Forms.CheckBox CheckBoxCategory;
		internal System.Windows.Forms.Label LabelCategory;
		internal System.Windows.Forms.TextBox TextBoxRuby;
		internal System.Windows.Forms.TextBox TextBoxName;
		internal System.Windows.Forms.Label LabelIdInfo;
		internal System.Windows.Forms.ComboBox ComboBoxId;
		private System.Windows.Forms.ContextMenuStrip ContextMenuStripCategories;
		private System.Windows.Forms.ContextMenuStrip ContextMenuStripAgeLimits;
	}
}