namespace YukaLister
{
	partial class FormEditPeople
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditPeople));
			this.LabelDescription = new System.Windows.Forms.Label();
			this.ListBoxPeople = new System.Windows.Forms.ListBox();
			this.ButtonAdd = new System.Windows.Forms.Button();
			this.ButtonRemove = new System.Windows.Forms.Button();
			this.ButtonEdit = new System.Windows.Forms.Button();
			this.ButtonNew = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.ButtonUp = new System.Windows.Forms.Button();
			this.ButtonDown = new System.Windows.Forms.Button();
			this.LinkLabelHelp = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// LabelDescription
			// 
			this.LabelDescription.Location = new System.Drawing.Point(16, 16);
			this.LabelDescription.Name = "LabelDescription";
			this.LabelDescription.Size = new System.Drawing.Size(384, 20);
			this.LabelDescription.TabIndex = 22;
			this.LabelDescription.Text = "-";
			this.LabelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ListBoxPeople
			// 
			this.ListBoxPeople.FormattingEnabled = true;
			this.ListBoxPeople.ItemHeight = 12;
			this.ListBoxPeople.Location = new System.Drawing.Point(16, 44);
			this.ListBoxPeople.Name = "ListBoxPeople";
			this.ListBoxPeople.Size = new System.Drawing.Size(320, 256);
			this.ListBoxPeople.TabIndex = 23;
			this.ListBoxPeople.SelectedIndexChanged += new System.EventHandler(this.ListBoxPeople_SelectedIndexChanged);
			this.ListBoxPeople.DoubleClick += new System.EventHandler(this.ListBoxPeople_DoubleClick);
			// 
			// ButtonAdd
			// 
			this.ButtonAdd.Location = new System.Drawing.Point(352, 56);
			this.ButtonAdd.Name = "ButtonAdd";
			this.ButtonAdd.Size = new System.Drawing.Size(112, 28);
			this.ButtonAdd.TabIndex = 24;
			this.ButtonAdd.Text = "検索して追加 (&A)";
			this.ButtonAdd.UseVisualStyleBackColor = true;
			this.ButtonAdd.Click += new System.EventHandler(this.ButtonAdd_Click);
			// 
			// ButtonRemove
			// 
			this.ButtonRemove.Location = new System.Drawing.Point(352, 92);
			this.ButtonRemove.Name = "ButtonRemove";
			this.ButtonRemove.Size = new System.Drawing.Size(112, 28);
			this.ButtonRemove.TabIndex = 25;
			this.ButtonRemove.Text = "削除 (&R)";
			this.ButtonRemove.UseVisualStyleBackColor = true;
			this.ButtonRemove.Click += new System.EventHandler(this.ButtonRemove_Click);
			// 
			// ButtonEdit
			// 
			this.ButtonEdit.Location = new System.Drawing.Point(352, 216);
			this.ButtonEdit.Name = "ButtonEdit";
			this.ButtonEdit.Size = new System.Drawing.Size(112, 28);
			this.ButtonEdit.TabIndex = 26;
			this.ButtonEdit.Text = "人物詳細編集 (&E)";
			this.ButtonEdit.UseVisualStyleBackColor = true;
			this.ButtonEdit.Click += new System.EventHandler(this.ButtonEdit_Click);
			// 
			// ButtonNew
			// 
			this.ButtonNew.Location = new System.Drawing.Point(352, 252);
			this.ButtonNew.Name = "ButtonNew";
			this.ButtonNew.Size = new System.Drawing.Size(112, 28);
			this.ButtonNew.TabIndex = 27;
			this.ButtonNew.Text = "新規人物作成 (&N)";
			this.ButtonNew.UseVisualStyleBackColor = true;
			this.ButtonNew.Click += new System.EventHandler(this.ButtonNew_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(369, 328);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 168;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(256, 328);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(96, 28);
			this.ButtonOK.TabIndex = 167;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Location = new System.Drawing.Point(0, 312);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(681, 5);
			this.panel2.TabIndex = 169;
			// 
			// ButtonUp
			// 
			this.ButtonUp.Location = new System.Drawing.Point(352, 136);
			this.ButtonUp.Name = "ButtonUp";
			this.ButtonUp.Size = new System.Drawing.Size(32, 28);
			this.ButtonUp.TabIndex = 170;
			this.ButtonUp.Text = "↑";
			this.ButtonUp.UseVisualStyleBackColor = true;
			this.ButtonUp.Click += new System.EventHandler(this.ButtonUp_Click);
			// 
			// ButtonDown
			// 
			this.ButtonDown.Location = new System.Drawing.Point(352, 172);
			this.ButtonDown.Name = "ButtonDown";
			this.ButtonDown.Size = new System.Drawing.Size(32, 28);
			this.ButtonDown.TabIndex = 171;
			this.ButtonDown.Text = "↓";
			this.ButtonDown.UseVisualStyleBackColor = true;
			this.ButtonDown.Click += new System.EventHandler(this.ButtonDown_Click);
			// 
			// LinkLabelHelp
			// 
			this.LinkLabelHelp.Location = new System.Drawing.Point(400, 16);
			this.LinkLabelHelp.Name = "LinkLabelHelp";
			this.LinkLabelHelp.Size = new System.Drawing.Size(64, 20);
			this.LinkLabelHelp.TabIndex = 172;
			this.LinkLabelHelp.TabStop = true;
			this.LinkLabelHelp.Text = "詳細情報";
			this.LinkLabelHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.LinkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelHelp_LinkClicked);
			// 
			// FormEditPeople
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(481, 369);
			this.Controls.Add(this.LinkLabelHelp);
			this.Controls.Add(this.ButtonDown);
			this.Controls.Add(this.ButtonUp);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.ButtonNew);
			this.Controls.Add(this.ButtonEdit);
			this.Controls.Add(this.ButtonRemove);
			this.Controls.Add(this.ButtonAdd);
			this.Controls.Add(this.ListBoxPeople);
			this.Controls.Add(this.LabelDescription);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormEditPeople";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormEditPeople_FormClosed);
			this.Load += new System.EventHandler(this.FormEditPeople_Load);
			this.Shown += new System.EventHandler(this.FormEditPeople_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.Label LabelDescription;
		private System.Windows.Forms.ListBox ListBoxPeople;
		private System.Windows.Forms.Button ButtonAdd;
		private System.Windows.Forms.Button ButtonRemove;
		private System.Windows.Forms.Button ButtonEdit;
		private System.Windows.Forms.Button ButtonNew;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button ButtonUp;
		private System.Windows.Forms.Button ButtonDown;
		private System.Windows.Forms.LinkLabel LinkLabelHelp;
	}
}