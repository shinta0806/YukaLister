namespace YukaLister
{
	partial class FormImport
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImport));
			this.LabelSrc = new System.Windows.Forms.Label();
			this.ButtonAbort = new System.Windows.Forms.Button();
			this.TextBoxLog = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// LabelSrc
			// 
			this.LabelSrc.AutoSize = true;
			this.LabelSrc.Location = new System.Drawing.Point(16, 16);
			this.LabelSrc.Name = "LabelSrc";
			this.LabelSrc.Size = new System.Drawing.Size(11, 12);
			this.LabelSrc.TabIndex = 0;
			this.LabelSrc.Text = "-";
			// 
			// ButtonAbort
			// 
			this.ButtonAbort.Location = new System.Drawing.Point(600, 40);
			this.ButtonAbort.Name = "ButtonAbort";
			this.ButtonAbort.Size = new System.Drawing.Size(96, 28);
			this.ButtonAbort.TabIndex = 1;
			this.ButtonAbort.Text = "中止 (&A)";
			this.ButtonAbort.UseVisualStyleBackColor = true;
			// 
			// TextBoxLog
			// 
			this.TextBoxLog.Location = new System.Drawing.Point(16, 80);
			this.TextBoxLog.Multiline = true;
			this.TextBoxLog.Name = "TextBoxLog";
			this.TextBoxLog.ReadOnly = true;
			this.TextBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextBoxLog.Size = new System.Drawing.Size(680, 240);
			this.TextBoxLog.TabIndex = 2;
			// 
			// FormImport
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(711, 335);
			this.Controls.Add(this.TextBoxLog);
			this.Controls.Add(this.ButtonAbort);
			this.Controls.Add(this.LabelSrc);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormImport";
			this.Load += new System.EventHandler(this.FormImport_Load);
			this.Shown += new System.EventHandler(this.FormImport_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LabelSrc;
		private System.Windows.Forms.Button ButtonAbort;
		private System.Windows.Forms.TextBox TextBoxLog;
	}
}