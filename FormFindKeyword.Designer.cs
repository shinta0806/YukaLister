namespace YukaLister
{
	partial class FormFindKeyword
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFindKeyword));
			this.TextBoxKeyword = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ButtonFindPrev = new System.Windows.Forms.Button();
			this.ButtonFindNext = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.CheckBoxCaseSensitive = new System.Windows.Forms.CheckBox();
			this.CheckBoxWholeMatch = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// TextBoxKeyword
			// 
			this.TextBoxKeyword.Location = new System.Drawing.Point(104, 16);
			this.TextBoxKeyword.Name = "TextBoxKeyword";
			this.TextBoxKeyword.Size = new System.Drawing.Size(320, 19);
			this.TextBoxKeyword.TabIndex = 12;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 20);
			this.label1.TabIndex = 11;
			this.label1.Text = "キーワード (&K)：";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ButtonFindPrev
			// 
			this.ButtonFindPrev.Location = new System.Drawing.Point(104, 104);
			this.ButtonFindPrev.Name = "ButtonFindPrev";
			this.ButtonFindPrev.Size = new System.Drawing.Size(96, 28);
			this.ButtonFindPrev.TabIndex = 13;
			this.ButtonFindPrev.Text = "前を検索 (&P)";
			this.ButtonFindPrev.UseVisualStyleBackColor = true;
			this.ButtonFindPrev.Click += new System.EventHandler(this.ButtonFindPrev_Click);
			// 
			// ButtonFindNext
			// 
			this.ButtonFindNext.Location = new System.Drawing.Point(216, 104);
			this.ButtonFindNext.Name = "ButtonFindNext";
			this.ButtonFindNext.Size = new System.Drawing.Size(96, 28);
			this.ButtonFindNext.TabIndex = 14;
			this.ButtonFindNext.Text = "次を検索 (&N)";
			this.ButtonFindNext.UseVisualStyleBackColor = true;
			this.ButtonFindNext.Click += new System.EventHandler(this.ButtonFindNext_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(328, 104);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 15;
			this.ButtonCancel.Text = "閉じる";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// CheckBoxCaseSensitive
			// 
			this.CheckBoxCaseSensitive.AutoSize = true;
			this.CheckBoxCaseSensitive.Location = new System.Drawing.Point(104, 48);
			this.CheckBoxCaseSensitive.Name = "CheckBoxCaseSensitive";
			this.CheckBoxCaseSensitive.Size = new System.Drawing.Size(176, 16);
			this.CheckBoxCaseSensitive.TabIndex = 16;
			this.CheckBoxCaseSensitive.Text = "大文字と小文字を区別する (&C)";
			this.CheckBoxCaseSensitive.UseVisualStyleBackColor = true;
			// 
			// CheckBoxWholeMatch
			// 
			this.CheckBoxWholeMatch.AutoSize = true;
			this.CheckBoxWholeMatch.Location = new System.Drawing.Point(104, 72);
			this.CheckBoxWholeMatch.Name = "CheckBoxWholeMatch";
			this.CheckBoxWholeMatch.Size = new System.Drawing.Size(247, 16);
			this.CheckBoxWholeMatch.TabIndex = 17;
			this.CheckBoxWholeMatch.Text = "セルの内容全体が一致するものを検索する (&W)";
			this.CheckBoxWholeMatch.UseVisualStyleBackColor = true;
			// 
			// FormFindKeyword
			// 
			this.AcceptButton = this.ButtonFindNext;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(441, 147);
			this.Controls.Add(this.CheckBoxWholeMatch);
			this.Controls.Add(this.CheckBoxCaseSensitive);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonFindNext);
			this.Controls.Add(this.ButtonFindPrev);
			this.Controls.Add(this.TextBoxKeyword);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormFindKeyword";
			this.ShowInTaskbar = false;
			this.TopMost = true;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormFindKeyword_FormClosed);
			this.Load += new System.EventHandler(this.FormFindKeyword_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox TextBoxKeyword;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button ButtonFindPrev;
		private System.Windows.Forms.Button ButtonFindNext;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.CheckBox CheckBoxCaseSensitive;
		private System.Windows.Forms.CheckBox CheckBoxWholeMatch;
	}
}