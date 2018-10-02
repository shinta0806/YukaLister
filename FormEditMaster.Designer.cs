namespace YukaLister
{
	partial class FormEditMaster
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditMaster));
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label21 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.TextBoxKeyword = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.TextBoxRuby = new System.Windows.Forms.TextBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.LabelName = new System.Windows.Forms.Label();
			this.TextBoxName = new System.Windows.Forms.TextBox();
			this.LabelIdInfo = new System.Windows.Forms.Label();
			this.ComboBoxId = new System.Windows.Forms.ComboBox();
			this.LabelId = new System.Windows.Forms.Label();
			this.LabelNote = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(872, 168);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 161;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(760, 168);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(96, 28);
			this.ButtonOK.TabIndex = 160;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Location = new System.Drawing.Point(0, 152);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1036, 5);
			this.panel1.TabIndex = 158;
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(560, 96);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(408, 20);
			this.label21.TabIndex = 157;
			this.label21.Text = "キーワード、コメントなど。複数入力する際は、半角カンマ「 , 」で区切って下さい。";
			this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(456, 68);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(104, 20);
			this.label16.TabIndex = 156;
			this.label16.Text = "検索ワード (&W)：";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxKeyword
			// 
			this.TextBoxKeyword.Location = new System.Drawing.Point(560, 68);
			this.TextBoxKeyword.Name = "TextBoxKeyword";
			this.TextBoxKeyword.Size = new System.Drawing.Size(408, 19);
			this.TextBoxKeyword.TabIndex = 155;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 68);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 20);
			this.label2.TabIndex = 131;
			this.label2.Text = "フリガナ (&F)：";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxRuby
			// 
			this.TextBoxRuby.Location = new System.Drawing.Point(112, 68);
			this.TextBoxRuby.Name = "TextBoxRuby";
			this.TextBoxRuby.Size = new System.Drawing.Size(296, 19);
			this.TextBoxRuby.TabIndex = 130;
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Location = new System.Drawing.Point(0, 52);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1036, 5);
			this.panel2.TabIndex = 129;
			// 
			// LabelName
			// 
			this.LabelName.Location = new System.Drawing.Point(16, 96);
			this.LabelName.Name = "LabelName";
			this.LabelName.Size = new System.Drawing.Size(96, 20);
			this.LabelName.TabIndex = 128;
			this.LabelName.Text = "名 (&N)：";
			this.LabelName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxName
			// 
			this.TextBoxName.Location = new System.Drawing.Point(112, 96);
			this.TextBoxName.Name = "TextBoxName";
			this.TextBoxName.Size = new System.Drawing.Size(296, 19);
			this.TextBoxName.TabIndex = 127;
			this.TextBoxName.Leave += new System.EventHandler(this.TextBoxName_Leave);
			// 
			// LabelIdInfo
			// 
			this.LabelIdInfo.Location = new System.Drawing.Point(416, 16);
			this.LabelIdInfo.Name = "LabelIdInfo";
			this.LabelIdInfo.Size = new System.Drawing.Size(200, 20);
			this.LabelIdInfo.TabIndex = 126;
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
			this.ComboBoxId.TabIndex = 125;
			this.ComboBoxId.SelectedIndexChanged += new System.EventHandler(this.ComboBoxId_SelectedIndexChanged);
			// 
			// LabelId
			// 
			this.LabelId.Location = new System.Drawing.Point(16, 16);
			this.LabelId.Name = "LabelId";
			this.LabelId.Size = new System.Drawing.Size(96, 20);
			this.LabelId.TabIndex = 124;
			this.LabelId.Text = " ID (&I)：";
			this.LabelId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LabelNote
			// 
			this.LabelNote.Location = new System.Drawing.Point(112, 124);
			this.LabelNote.Name = "LabelNote";
			this.LabelNote.Size = new System.Drawing.Size(856, 20);
			this.LabelNote.TabIndex = 162;
			this.LabelNote.Text = "-";
			this.LabelNote.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FormEditMaster
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(983, 210);
			this.Controls.Add(this.LabelNote);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label21);
			this.Controls.Add(this.label16);
			this.Controls.Add(this.TextBoxKeyword);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.TextBoxRuby);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.LabelName);
			this.Controls.Add(this.TextBoxName);
			this.Controls.Add(this.LabelIdInfo);
			this.Controls.Add(this.ComboBoxId);
			this.Controls.Add(this.LabelId);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormEditMaster";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormEditMaster_FormClosed);
			this.Load += new System.EventHandler(this.FormEditMaster_Load);
			this.Shown += new System.EventHandler(this.FormEditMaster_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel2;
		internal System.Windows.Forms.ComboBox ComboBoxId;
		internal System.Windows.Forms.TextBox TextBoxKeyword;
		internal System.Windows.Forms.TextBox TextBoxRuby;
		internal System.Windows.Forms.TextBox TextBoxName;
		internal System.Windows.Forms.Label LabelId;
		internal System.Windows.Forms.Label LabelName;
		internal System.Windows.Forms.Label LabelIdInfo;
		internal System.Windows.Forms.Label LabelNote;
	}
}