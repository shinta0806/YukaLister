namespace YukaLister
{
	partial class FormSearchMusicInfo
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSearchMusicInfo));
			this.label6 = new System.Windows.Forms.Label();
			this.TextBoxKeyword = new System.Windows.Forms.TextBox();
			this.ButtonSearch = new System.Windows.Forms.Button();
			this.ListBoxFounds = new System.Windows.Forms.ListBox();
			this.ButtonSelect = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.LabelDescription = new System.Windows.Forms.Label();
			this.LabelFounds = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 48);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(112, 20);
			this.label6.TabIndex = 1;
			this.label6.Text = "検索キーワード (&W)：";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxKeyword
			// 
			this.TextBoxKeyword.Location = new System.Drawing.Point(128, 48);
			this.TextBoxKeyword.Name = "TextBoxKeyword";
			this.TextBoxKeyword.Size = new System.Drawing.Size(168, 19);
			this.TextBoxKeyword.TabIndex = 2;
			this.TextBoxKeyword.TextChanged += new System.EventHandler(this.TextBoxKeyword_TextChanged);
			this.TextBoxKeyword.Enter += new System.EventHandler(this.TextBoxKeyword_Enter);
			this.TextBoxKeyword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxKeyword_KeyDown);
			this.TextBoxKeyword.Leave += new System.EventHandler(this.TextBoxKeyword_Leave);
			// 
			// ButtonSearch
			// 
			this.ButtonSearch.Location = new System.Drawing.Point(304, 44);
			this.ButtonSearch.Name = "ButtonSearch";
			this.ButtonSearch.Size = new System.Drawing.Size(96, 28);
			this.ButtonSearch.TabIndex = 3;
			this.ButtonSearch.Text = "検索 (&S)";
			this.ButtonSearch.UseVisualStyleBackColor = true;
			this.ButtonSearch.Click += new System.EventHandler(this.ButtonSearch_Click);
			// 
			// ListBoxFounds
			// 
			this.ListBoxFounds.FormattingEnabled = true;
			this.ListBoxFounds.ItemHeight = 12;
			this.ListBoxFounds.Location = new System.Drawing.Point(16, 112);
			this.ListBoxFounds.Name = "ListBoxFounds";
			this.ListBoxFounds.Size = new System.Drawing.Size(384, 196);
			this.ListBoxFounds.TabIndex = 4;
			this.ListBoxFounds.SelectedIndexChanged += new System.EventHandler(this.ListBoxFounds_SelectedIndexChanged);
			this.ListBoxFounds.DoubleClick += new System.EventHandler(this.ListBoxFounds_DoubleClick);
			// 
			// ButtonSelect
			// 
			this.ButtonSelect.Location = new System.Drawing.Point(192, 320);
			this.ButtonSelect.Name = "ButtonSelect";
			this.ButtonSelect.Size = new System.Drawing.Size(96, 28);
			this.ButtonSelect.TabIndex = 5;
			this.ButtonSelect.Text = "選択";
			this.ButtonSelect.UseVisualStyleBackColor = true;
			this.ButtonSelect.Click += new System.EventHandler(this.ButtonSelect_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(304, 320);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 6;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// LabelDescription
			// 
			this.LabelDescription.Location = new System.Drawing.Point(16, 16);
			this.LabelDescription.Name = "LabelDescription";
			this.LabelDescription.Size = new System.Drawing.Size(384, 20);
			this.LabelDescription.TabIndex = 0;
			this.LabelDescription.Text = "-";
			this.LabelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelFounds
			// 
			this.LabelFounds.Location = new System.Drawing.Point(16, 80);
			this.LabelFounds.Name = "LabelFounds";
			this.LabelFounds.Size = new System.Drawing.Size(384, 20);
			this.LabelFounds.TabIndex = 7;
			this.LabelFounds.Text = "-";
			this.LabelFounds.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FormSearchMusicInfo
			// 
			this.AcceptButton = this.ButtonSelect;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(414, 362);
			this.Controls.Add(this.LabelFounds);
			this.Controls.Add(this.LabelDescription);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonSelect);
			this.Controls.Add(this.ListBoxFounds);
			this.Controls.Add(this.ButtonSearch);
			this.Controls.Add(this.TextBoxKeyword);
			this.Controls.Add(this.label6);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormSearchMusicInfo";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormSearchOrigin_FormClosed);
			this.Load += new System.EventHandler(this.FormSearchOrigin_Load);
			this.Shown += new System.EventHandler(this.FormSearchOrigin_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox TextBoxKeyword;
		private System.Windows.Forms.Button ButtonSearch;
		private System.Windows.Forms.ListBox ListBoxFounds;
		private System.Windows.Forms.Button ButtonSelect;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Label LabelDescription;
		private System.Windows.Forms.Label LabelFounds;
	}
}