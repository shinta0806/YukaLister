namespace YukaLister
{
	partial class FormInputIdPrefix
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormInputIdPrefix));
			this.label1 = new System.Windows.Forms.Label();
			this.TextBoxIdPrefix = new System.Windows.Forms.TextBox();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.LinkLabelHelp = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(239, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "各種 ID の先頭に付与する文字列を設定します。";
			// 
			// TextBoxIdPrefix
			// 
			this.TextBoxIdPrefix.Location = new System.Drawing.Point(16, 60);
			this.TextBoxIdPrefix.Name = "TextBoxIdPrefix";
			this.TextBoxIdPrefix.Size = new System.Drawing.Size(344, 19);
			this.TextBoxIdPrefix.TabIndex = 2;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(265, 92);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 5;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(152, 92);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(96, 28);
			this.ButtonOK.TabIndex = 4;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(16, 36);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(226, 12);
			this.label2.TabIndex = 1;
			this.label2.Text = "パソコンごとに異なる文字列を指定してください。";
			// 
			// LinkLabelHelp
			// 
			this.LinkLabelHelp.Location = new System.Drawing.Point(248, 32);
			this.LinkLabelHelp.Name = "LinkLabelHelp";
			this.LinkLabelHelp.Size = new System.Drawing.Size(64, 20);
			this.LinkLabelHelp.TabIndex = 13;
			this.LinkLabelHelp.TabStop = true;
			this.LinkLabelHelp.Text = "詳細情報";
			this.LinkLabelHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.LinkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelHelp_LinkClicked);
			// 
			// FormInputIdPrefix
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(378, 135);
			this.Controls.Add(this.LinkLabelHelp);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.TextBoxIdPrefix);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormInputIdPrefix";
			this.Text = "ID 接頭辞の設定";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormInputIdPrefix_FormClosed);
			this.Load += new System.EventHandler(this.FormInputIdPrefix_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox TextBoxIdPrefix;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel LinkLabelHelp;
	}
}