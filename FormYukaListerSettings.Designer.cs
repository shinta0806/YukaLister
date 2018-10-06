namespace YukaLister
{
	partial class FormYukaListerSettings
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormYukaListerSettings));
			this.ButtonOK = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.OpenFileDialogYukariConfigPathSeed = new System.Windows.Forms.OpenFileDialog();
			this.TabControlYukaListerSettings = new System.Windows.Forms.TabControl();
			this.TabPageSettings = new System.Windows.Forms.TabPage();
			this.ButtonBrowseYukariConfigPathSeed = new System.Windows.Forms.Button();
			this.TextBoxYukariConfigPathSeed = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.TabPageTarget = new System.Windows.Forms.TabPage();
			this.label16 = new System.Windows.Forms.Label();
			this.ListBoxTargetExts = new System.Windows.Forms.ListBox();
			this.ButtonRemoveExt = new System.Windows.Forms.Button();
			this.ButtonAddExt = new System.Windows.Forms.Button();
			this.TextBoxTargetExt = new System.Windows.Forms.TextBox();
			this.LabelTargetExt = new System.Windows.Forms.Label();
			this.TabPageOutput = new System.Windows.Forms.TabPage();
			this.label15 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label14 = new System.Windows.Forms.Label();
			this.TextBoxYukariListFolder = new System.Windows.Forms.TextBox();
			this.ButtonYukariListSettings = new System.Windows.Forms.Button();
			this.label13 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.ButtonOutputList = new System.Windows.Forms.Button();
			this.label10 = new System.Windows.Forms.Label();
			this.TextBoxListFolder = new System.Windows.Forms.TextBox();
			this.ButtonBrowseListFolder = new System.Windows.Forms.Button();
			this.ButtonListSettings = new System.Windows.Forms.Button();
			this.ComboBoxListFormat = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.TabPageMaintenance = new System.Windows.Forms.TabPage();
			this.TextBoxSyncPassword = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.TextBoxSyncAccount = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.TextBoxSyncServer = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.CheckBoxSyncMusicInfoDb = new System.Windows.Forms.CheckBox();
			this.TabPageImport = new System.Windows.Forms.TabPage();
			this.panel5 = new System.Windows.Forms.Panel();
			this.ButtonImport = new System.Windows.Forms.Button();
			this.ButtonBrowseImportNicoKaraLister = new System.Windows.Forms.Button();
			this.TextBoxImportNicoKaraLister = new System.Windows.Forms.TextBox();
			this.RadioButtonImportNicoKaraLister = new System.Windows.Forms.RadioButton();
			this.label5 = new System.Windows.Forms.Label();
			this.ButtonBrowseImportGameCsv = new System.Windows.Forms.Button();
			this.TextBoxImportGameCsv = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.ButtonBrowseImportSfCsv = new System.Windows.Forms.Button();
			this.TextBoxImportSfCsv = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.ButtonBrowseImportAnisonCsv = new System.Windows.Forms.Button();
			this.TextBoxImportAnisonCsv = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.ButtonBrowseImportProgramCsv = new System.Windows.Forms.Button();
			this.TextBoxImportProgramCsv = new System.Windows.Forms.TextBox();
			this.RadioButtonImportAnisonInfoCsv = new System.Windows.Forms.RadioButton();
			this.ButtonBrowseImportYukaLister = new System.Windows.Forms.Button();
			this.TextBoxImportYukaLister = new System.Windows.Forms.TextBox();
			this.RadioButtonImportYukaLister = new System.Windows.Forms.RadioButton();
			this.TabPageExport = new System.Windows.Forms.TabPage();
			this.OpenFileDialogMisc = new System.Windows.Forms.OpenFileDialog();
			this.FolderBrowserDialogOutputList = new System.Windows.Forms.FolderBrowserDialog();
			this.LabelDescription = new System.Windows.Forms.Label();
			this.LinkLabelHelp = new System.Windows.Forms.LinkLabel();
			this.TabControlYukaListerSettings.SuspendLayout();
			this.TabPageSettings.SuspendLayout();
			this.TabPageTarget.SuspendLayout();
			this.TabPageOutput.SuspendLayout();
			this.TabPageMaintenance.SuspendLayout();
			this.TabPageImport.SuspendLayout();
			this.SuspendLayout();
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(328, 424);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(96, 28);
			this.ButtonOK.TabIndex = 9;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(440, 424);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 10;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// OpenFileDialogYukariConfigPathSeed
			// 
			this.OpenFileDialogYukariConfigPathSeed.Filter = "設定ファイル|*.ini";
			// 
			// TabControlYukaListerSettings
			// 
			this.TabControlYukaListerSettings.Controls.Add(this.TabPageSettings);
			this.TabControlYukaListerSettings.Controls.Add(this.TabPageTarget);
			this.TabControlYukaListerSettings.Controls.Add(this.TabPageOutput);
			this.TabControlYukaListerSettings.Controls.Add(this.TabPageMaintenance);
			this.TabControlYukaListerSettings.Controls.Add(this.TabPageImport);
			this.TabControlYukaListerSettings.Controls.Add(this.TabPageExport);
			this.TabControlYukaListerSettings.Location = new System.Drawing.Point(0, 4);
			this.TabControlYukaListerSettings.Name = "TabControlYukaListerSettings";
			this.TabControlYukaListerSettings.SelectedIndex = 0;
			this.TabControlYukaListerSettings.Size = new System.Drawing.Size(552, 408);
			this.TabControlYukaListerSettings.TabIndex = 12;
			// 
			// TabPageSettings
			// 
			this.TabPageSettings.BackColor = System.Drawing.SystemColors.Control;
			this.TabPageSettings.Controls.Add(this.ButtonBrowseYukariConfigPathSeed);
			this.TabPageSettings.Controls.Add(this.TextBoxYukariConfigPathSeed);
			this.TabPageSettings.Controls.Add(this.label1);
			this.TabPageSettings.Location = new System.Drawing.Point(4, 22);
			this.TabPageSettings.Name = "TabPageSettings";
			this.TabPageSettings.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageSettings.Size = new System.Drawing.Size(544, 382);
			this.TabPageSettings.TabIndex = 0;
			this.TabPageSettings.Text = "設定";
			// 
			// ButtonBrowseYukariConfigPathSeed
			// 
			this.ButtonBrowseYukariConfigPathSeed.Location = new System.Drawing.Point(432, 16);
			this.ButtonBrowseYukariConfigPathSeed.Name = "ButtonBrowseYukariConfigPathSeed";
			this.ButtonBrowseYukariConfigPathSeed.Size = new System.Drawing.Size(96, 28);
			this.ButtonBrowseYukariConfigPathSeed.TabIndex = 11;
			this.ButtonBrowseYukariConfigPathSeed.Text = "参照 (&B)";
			this.ButtonBrowseYukariConfigPathSeed.UseVisualStyleBackColor = true;
			this.ButtonBrowseYukariConfigPathSeed.Click += new System.EventHandler(this.ButtonBrowseYukariConfigPathSeed_Click);
			// 
			// TextBoxYukariConfigPathSeed
			// 
			this.TextBoxYukariConfigPathSeed.Location = new System.Drawing.Point(144, 20);
			this.TextBoxYukariConfigPathSeed.Name = "TextBoxYukariConfigPathSeed";
			this.TextBoxYukariConfigPathSeed.Size = new System.Drawing.Size(280, 19);
			this.TextBoxYukariConfigPathSeed.TabIndex = 10;
			this.TextBoxYukariConfigPathSeed.TextChanged += new System.EventHandler(this.TextBoxYukariConfigPathSeed_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(128, 20);
			this.label1.TabIndex = 9;
			this.label1.Text = "ゆかり設定ファイル (&Y)：";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TabPageTarget
			// 
			this.TabPageTarget.BackColor = System.Drawing.SystemColors.Control;
			this.TabPageTarget.Controls.Add(this.label16);
			this.TabPageTarget.Controls.Add(this.ListBoxTargetExts);
			this.TabPageTarget.Controls.Add(this.ButtonRemoveExt);
			this.TabPageTarget.Controls.Add(this.ButtonAddExt);
			this.TabPageTarget.Controls.Add(this.TextBoxTargetExt);
			this.TabPageTarget.Controls.Add(this.LabelTargetExt);
			this.TabPageTarget.Location = new System.Drawing.Point(4, 22);
			this.TabPageTarget.Name = "TabPageTarget";
			this.TabPageTarget.Size = new System.Drawing.Size(544, 382);
			this.TabPageTarget.TabIndex = 5;
			this.TabPageTarget.Text = "リスト対象";
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(16, 160);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(176, 20);
			this.label16.TabIndex = 12;
			this.label16.Text = "（追加したい拡張子）";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ListBoxTargetExts
			// 
			this.ListBoxTargetExts.FormattingEnabled = true;
			this.ListBoxTargetExts.ItemHeight = 12;
			this.ListBoxTargetExts.Location = new System.Drawing.Point(192, 16);
			this.ListBoxTargetExts.Name = "ListBoxTargetExts";
			this.ListBoxTargetExts.Size = new System.Drawing.Size(208, 100);
			this.ListBoxTargetExts.TabIndex = 11;
			this.ListBoxTargetExts.SelectedIndexChanged += new System.EventHandler(this.ListBoxTargetExts_SelectedIndexChanged);
			// 
			// ButtonRemoveExt
			// 
			this.ButtonRemoveExt.Location = new System.Drawing.Point(304, 124);
			this.ButtonRemoveExt.Name = "ButtonRemoveExt";
			this.ButtonRemoveExt.Size = new System.Drawing.Size(96, 28);
			this.ButtonRemoveExt.TabIndex = 10;
			this.ButtonRemoveExt.Text = "× 削除 (&R)";
			this.ButtonRemoveExt.UseVisualStyleBackColor = true;
			this.ButtonRemoveExt.Click += new System.EventHandler(this.ButtonRemoveExt_Click);
			// 
			// ButtonAddExt
			// 
			this.ButtonAddExt.Location = new System.Drawing.Point(192, 124);
			this.ButtonAddExt.Name = "ButtonAddExt";
			this.ButtonAddExt.Size = new System.Drawing.Size(96, 28);
			this.ButtonAddExt.TabIndex = 9;
			this.ButtonAddExt.Text = "↑ 追加 (&A)";
			this.ButtonAddExt.UseVisualStyleBackColor = true;
			this.ButtonAddExt.Click += new System.EventHandler(this.ButtonAddExt_Click);
			// 
			// TextBoxTargetExt
			// 
			this.TextBoxTargetExt.AccessibleDescription = "";
			this.TextBoxTargetExt.Location = new System.Drawing.Point(192, 160);
			this.TextBoxTargetExt.Name = "TextBoxTargetExt";
			this.TextBoxTargetExt.Size = new System.Drawing.Size(208, 19);
			this.TextBoxTargetExt.TabIndex = 8;
			this.TextBoxTargetExt.TextChanged += new System.EventHandler(this.TextBoxTargetExt_TextChanged);
			// 
			// LabelTargetExt
			// 
			this.LabelTargetExt.Location = new System.Drawing.Point(16, 16);
			this.LabelTargetExt.Name = "LabelTargetExt";
			this.LabelTargetExt.Size = new System.Drawing.Size(176, 20);
			this.LabelTargetExt.TabIndex = 4;
			this.LabelTargetExt.Text = "リスト化対象ファイルの拡張子 (&E)：";
			this.LabelTargetExt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TabPageOutput
			// 
			this.TabPageOutput.BackColor = System.Drawing.SystemColors.Control;
			this.TabPageOutput.Controls.Add(this.label15);
			this.TabPageOutput.Controls.Add(this.panel2);
			this.TabPageOutput.Controls.Add(this.label14);
			this.TabPageOutput.Controls.Add(this.TextBoxYukariListFolder);
			this.TabPageOutput.Controls.Add(this.ButtonYukariListSettings);
			this.TabPageOutput.Controls.Add(this.label13);
			this.TabPageOutput.Controls.Add(this.label12);
			this.TabPageOutput.Controls.Add(this.label11);
			this.TabPageOutput.Controls.Add(this.panel1);
			this.TabPageOutput.Controls.Add(this.ButtonOutputList);
			this.TabPageOutput.Controls.Add(this.label10);
			this.TabPageOutput.Controls.Add(this.TextBoxListFolder);
			this.TabPageOutput.Controls.Add(this.ButtonBrowseListFolder);
			this.TabPageOutput.Controls.Add(this.ButtonListSettings);
			this.TabPageOutput.Controls.Add(this.ComboBoxListFormat);
			this.TabPageOutput.Controls.Add(this.label9);
			this.TabPageOutput.Location = new System.Drawing.Point(4, 22);
			this.TabPageOutput.Name = "TabPageOutput";
			this.TabPageOutput.Size = new System.Drawing.Size(544, 382);
			this.TabPageOutput.TabIndex = 4;
			this.TabPageOutput.Text = "リスト出力";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(16, 132);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(81, 12);
			this.label15.TabIndex = 39;
			this.label15.Text = "　閲覧用リスト　";
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Location = new System.Drawing.Point(0, 136);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(850, 5);
			this.panel2.TabIndex = 38;
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(16, 68);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(408, 20);
			this.label14.TabIndex = 37;
			this.label14.Text = "リスト出力先は、ゆかり設定ファイルがあるフォルダーの配下となります。";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TextBoxYukariListFolder
			// 
			this.TextBoxYukariListFolder.Location = new System.Drawing.Point(160, 96);
			this.TextBoxYukariListFolder.Name = "TextBoxYukariListFolder";
			this.TextBoxYukariListFolder.ReadOnly = true;
			this.TextBoxYukariListFolder.Size = new System.Drawing.Size(264, 19);
			this.TextBoxYukariListFolder.TabIndex = 36;
			// 
			// ButtonYukariListSettings
			// 
			this.ButtonYukariListSettings.Location = new System.Drawing.Point(432, 36);
			this.ButtonYukariListSettings.Name = "ButtonYukariListSettings";
			this.ButtonYukariListSettings.Size = new System.Drawing.Size(96, 28);
			this.ButtonYukariListSettings.TabIndex = 35;
			this.ButtonYukariListSettings.Text = "出力設定 (&S)";
			this.ButtonYukariListSettings.UseVisualStyleBackColor = true;
			this.ButtonYukariListSettings.Click += new System.EventHandler(this.ButtonYukariListSettings_Click);
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(16, 40);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(408, 20);
			this.label13.TabIndex = 34;
			this.label13.Text = "ゆかりリクエスト用リストは、常に自動的に出力・更新されます。";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(16, 96);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(144, 20);
			this.label12.TabIndex = 33;
			this.label12.Text = "リスト出力先フォルダー：";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(16, 16);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(160, 12);
			this.label11.TabIndex = 32;
			this.label11.Text = "　ゆかりリクエスト用リスト（PHP）　";
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Location = new System.Drawing.Point(0, 20);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(850, 5);
			this.panel1.TabIndex = 31;
			// 
			// ButtonOutputList
			// 
			this.ButtonOutputList.Location = new System.Drawing.Point(328, 232);
			this.ButtonOutputList.Name = "ButtonOutputList";
			this.ButtonOutputList.Size = new System.Drawing.Size(200, 28);
			this.ButtonOutputList.TabIndex = 30;
			this.ButtonOutputList.Text = "閲覧用リスト出力 (&O)";
			this.ButtonOutputList.UseVisualStyleBackColor = true;
			this.ButtonOutputList.Click += new System.EventHandler(this.ButtonOutput_Click);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(16, 196);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(144, 20);
			this.label10.TabIndex = 15;
			this.label10.Text = "リスト出力先フォルダー (&O)：";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TextBoxListFolder
			// 
			this.TextBoxListFolder.Location = new System.Drawing.Point(160, 196);
			this.TextBoxListFolder.Name = "TextBoxListFolder";
			this.TextBoxListFolder.Size = new System.Drawing.Size(264, 19);
			this.TextBoxListFolder.TabIndex = 14;
			// 
			// ButtonBrowseListFolder
			// 
			this.ButtonBrowseListFolder.Location = new System.Drawing.Point(432, 192);
			this.ButtonBrowseListFolder.Name = "ButtonBrowseListFolder";
			this.ButtonBrowseListFolder.Size = new System.Drawing.Size(96, 28);
			this.ButtonBrowseListFolder.TabIndex = 13;
			this.ButtonBrowseListFolder.Text = "参照 (&B)";
			this.ButtonBrowseListFolder.UseVisualStyleBackColor = true;
			this.ButtonBrowseListFolder.Click += new System.EventHandler(this.ButtonBrowseOutputPath_Click);
			// 
			// ButtonListSettings
			// 
			this.ButtonListSettings.Location = new System.Drawing.Point(432, 152);
			this.ButtonListSettings.Name = "ButtonListSettings";
			this.ButtonListSettings.Size = new System.Drawing.Size(96, 28);
			this.ButtonListSettings.TabIndex = 12;
			this.ButtonListSettings.Text = "出力設定 (&T)";
			this.ButtonListSettings.UseVisualStyleBackColor = true;
			this.ButtonListSettings.Click += new System.EventHandler(this.ButtonListSettings_Click);
			// 
			// ComboBoxListFormat
			// 
			this.ComboBoxListFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboBoxListFormat.FormattingEnabled = true;
			this.ComboBoxListFormat.Location = new System.Drawing.Point(160, 156);
			this.ComboBoxListFormat.Name = "ComboBoxListFormat";
			this.ComboBoxListFormat.Size = new System.Drawing.Size(264, 20);
			this.ComboBoxListFormat.TabIndex = 11;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 156);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(144, 20);
			this.label9.TabIndex = 10;
			this.label9.Text = "リスト出力形式 (&F)：";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TabPageMaintenance
			// 
			this.TabPageMaintenance.BackColor = System.Drawing.SystemColors.Control;
			this.TabPageMaintenance.Controls.Add(this.TextBoxSyncPassword);
			this.TabPageMaintenance.Controls.Add(this.label8);
			this.TabPageMaintenance.Controls.Add(this.TextBoxSyncAccount);
			this.TabPageMaintenance.Controls.Add(this.label7);
			this.TabPageMaintenance.Controls.Add(this.TextBoxSyncServer);
			this.TabPageMaintenance.Controls.Add(this.label6);
			this.TabPageMaintenance.Controls.Add(this.CheckBoxSyncMusicInfoDb);
			this.TabPageMaintenance.Location = new System.Drawing.Point(4, 22);
			this.TabPageMaintenance.Name = "TabPageMaintenance";
			this.TabPageMaintenance.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageMaintenance.Size = new System.Drawing.Size(544, 382);
			this.TabPageMaintenance.TabIndex = 1;
			this.TabPageMaintenance.Text = "メンテナンス";
			// 
			// TextBoxSyncPassword
			// 
			this.TextBoxSyncPassword.Location = new System.Drawing.Point(152, 92);
			this.TextBoxSyncPassword.Name = "TextBoxSyncPassword";
			this.TextBoxSyncPassword.Size = new System.Drawing.Size(376, 19);
			this.TextBoxSyncPassword.TabIndex = 16;
			this.TextBoxSyncPassword.UseSystemPasswordChar = true;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(48, 92);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(104, 20);
			this.label8.TabIndex = 15;
			this.label8.Text = "パスワード (&P)：";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxSyncAccount
			// 
			this.TextBoxSyncAccount.Location = new System.Drawing.Point(152, 68);
			this.TextBoxSyncAccount.Name = "TextBoxSyncAccount";
			this.TextBoxSyncAccount.Size = new System.Drawing.Size(376, 19);
			this.TextBoxSyncAccount.TabIndex = 14;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(48, 68);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(104, 20);
			this.label7.TabIndex = 13;
			this.label7.Text = "アカウント名 (&A)：";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxSyncServer
			// 
			this.TextBoxSyncServer.Location = new System.Drawing.Point(152, 44);
			this.TextBoxSyncServer.Name = "TextBoxSyncServer";
			this.TextBoxSyncServer.Size = new System.Drawing.Size(376, 19);
			this.TextBoxSyncServer.TabIndex = 12;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(48, 44);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(104, 20);
			this.label6.TabIndex = 11;
			this.label6.Text = "サーバー URL (&U)：";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CheckBoxSyncMusicInfoDb
			// 
			this.CheckBoxSyncMusicInfoDb.Location = new System.Drawing.Point(16, 16);
			this.CheckBoxSyncMusicInfoDb.Name = "CheckBoxSyncMusicInfoDb";
			this.CheckBoxSyncMusicInfoDb.Size = new System.Drawing.Size(512, 20);
			this.CheckBoxSyncMusicInfoDb.TabIndex = 0;
			this.CheckBoxSyncMusicInfoDb.Text = "楽曲情報データベースを同期する (&S)";
			this.CheckBoxSyncMusicInfoDb.UseVisualStyleBackColor = true;
			this.CheckBoxSyncMusicInfoDb.CheckedChanged += new System.EventHandler(this.CheckBoxSyncMusicInfoDb_CheckedChanged);
			// 
			// TabPageImport
			// 
			this.TabPageImport.AllowDrop = true;
			this.TabPageImport.BackColor = System.Drawing.SystemColors.Control;
			this.TabPageImport.Controls.Add(this.panel5);
			this.TabPageImport.Controls.Add(this.ButtonImport);
			this.TabPageImport.Controls.Add(this.ButtonBrowseImportNicoKaraLister);
			this.TabPageImport.Controls.Add(this.TextBoxImportNicoKaraLister);
			this.TabPageImport.Controls.Add(this.RadioButtonImportNicoKaraLister);
			this.TabPageImport.Controls.Add(this.label5);
			this.TabPageImport.Controls.Add(this.ButtonBrowseImportGameCsv);
			this.TabPageImport.Controls.Add(this.TextBoxImportGameCsv);
			this.TabPageImport.Controls.Add(this.label4);
			this.TabPageImport.Controls.Add(this.ButtonBrowseImportSfCsv);
			this.TabPageImport.Controls.Add(this.TextBoxImportSfCsv);
			this.TabPageImport.Controls.Add(this.label3);
			this.TabPageImport.Controls.Add(this.ButtonBrowseImportAnisonCsv);
			this.TabPageImport.Controls.Add(this.TextBoxImportAnisonCsv);
			this.TabPageImport.Controls.Add(this.label2);
			this.TabPageImport.Controls.Add(this.ButtonBrowseImportProgramCsv);
			this.TabPageImport.Controls.Add(this.TextBoxImportProgramCsv);
			this.TabPageImport.Controls.Add(this.RadioButtonImportAnisonInfoCsv);
			this.TabPageImport.Controls.Add(this.ButtonBrowseImportYukaLister);
			this.TabPageImport.Controls.Add(this.TextBoxImportYukaLister);
			this.TabPageImport.Controls.Add(this.RadioButtonImportYukaLister);
			this.TabPageImport.Location = new System.Drawing.Point(4, 22);
			this.TabPageImport.Name = "TabPageImport";
			this.TabPageImport.Size = new System.Drawing.Size(544, 382);
			this.TabPageImport.TabIndex = 2;
			this.TabPageImport.Text = "インポート";
			this.TabPageImport.DragDrop += new System.Windows.Forms.DragEventHandler(this.TabPageImport_DragDrop);
			this.TabPageImport.DragEnter += new System.Windows.Forms.DragEventHandler(this.TabPageImport_DragEnter);
			// 
			// panel5
			// 
			this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel5.Location = new System.Drawing.Point(-8, 324);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(850, 5);
			this.panel5.TabIndex = 30;
			// 
			// ButtonImport
			// 
			this.ButtonImport.Location = new System.Drawing.Point(328, 340);
			this.ButtonImport.Name = "ButtonImport";
			this.ButtonImport.Size = new System.Drawing.Size(200, 28);
			this.ButtonImport.TabIndex = 29;
			this.ButtonImport.Text = "インポート (&I)";
			this.ButtonImport.UseVisualStyleBackColor = true;
			this.ButtonImport.Click += new System.EventHandler(this.ButtonImport_Click);
			// 
			// ButtonBrowseImportNicoKaraLister
			// 
			this.ButtonBrowseImportNicoKaraLister.Location = new System.Drawing.Point(432, 284);
			this.ButtonBrowseImportNicoKaraLister.Name = "ButtonBrowseImportNicoKaraLister";
			this.ButtonBrowseImportNicoKaraLister.Size = new System.Drawing.Size(96, 28);
			this.ButtonBrowseImportNicoKaraLister.TabIndex = 28;
			this.ButtonBrowseImportNicoKaraLister.Text = "参照 (&6)";
			this.ButtonBrowseImportNicoKaraLister.UseVisualStyleBackColor = true;
			this.ButtonBrowseImportNicoKaraLister.Click += new System.EventHandler(this.ButtonBrowseImportNicoKaraLister_Click);
			// 
			// TextBoxImportNicoKaraLister
			// 
			this.TextBoxImportNicoKaraLister.Location = new System.Drawing.Point(32, 288);
			this.TextBoxImportNicoKaraLister.Name = "TextBoxImportNicoKaraLister";
			this.TextBoxImportNicoKaraLister.Size = new System.Drawing.Size(392, 19);
			this.TextBoxImportNicoKaraLister.TabIndex = 27;
			// 
			// RadioButtonImportNicoKaraLister
			// 
			this.RadioButtonImportNicoKaraLister.AutoSize = true;
			this.RadioButtonImportNicoKaraLister.Location = new System.Drawing.Point(16, 260);
			this.RadioButtonImportNicoKaraLister.Name = "RadioButtonImportNicoKaraLister";
			this.RadioButtonImportNicoKaraLister.Size = new System.Drawing.Size(285, 16);
			this.RadioButtonImportNicoKaraLister.TabIndex = 26;
			this.RadioButtonImportNicoKaraLister.Text = "ニコカラりすたーでエクスポートしたファイルをインポート (&N)";
			this.RadioButtonImportNicoKaraLister.UseVisualStyleBackColor = true;
			this.RadioButtonImportNicoKaraLister.CheckedChanged += new System.EventHandler(this.RadioButtonImport_CheckedChanged);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(32, 220);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(128, 20);
			this.label5.TabIndex = 25;
			this.label5.Text = "game.csv (.zip) (&G)：";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ButtonBrowseImportGameCsv
			// 
			this.ButtonBrowseImportGameCsv.Location = new System.Drawing.Point(432, 216);
			this.ButtonBrowseImportGameCsv.Name = "ButtonBrowseImportGameCsv";
			this.ButtonBrowseImportGameCsv.Size = new System.Drawing.Size(96, 28);
			this.ButtonBrowseImportGameCsv.TabIndex = 24;
			this.ButtonBrowseImportGameCsv.Text = "参照 (&5)";
			this.ButtonBrowseImportGameCsv.UseVisualStyleBackColor = true;
			this.ButtonBrowseImportGameCsv.Click += new System.EventHandler(this.ButtonBrowseImportGameCsv_Click);
			// 
			// TextBoxImportGameCsv
			// 
			this.TextBoxImportGameCsv.Location = new System.Drawing.Point(160, 220);
			this.TextBoxImportGameCsv.Name = "TextBoxImportGameCsv";
			this.TextBoxImportGameCsv.Size = new System.Drawing.Size(264, 19);
			this.TextBoxImportGameCsv.TabIndex = 23;
			this.TextBoxImportGameCsv.TextChanged += new System.EventHandler(this.TextBoxImportAnisonInfoCsv_TextChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(32, 184);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(128, 20);
			this.label4.TabIndex = 22;
			this.label4.Text = "sf.csv (.zip) (&S)：";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ButtonBrowseImportSfCsv
			// 
			this.ButtonBrowseImportSfCsv.Location = new System.Drawing.Point(432, 180);
			this.ButtonBrowseImportSfCsv.Name = "ButtonBrowseImportSfCsv";
			this.ButtonBrowseImportSfCsv.Size = new System.Drawing.Size(96, 28);
			this.ButtonBrowseImportSfCsv.TabIndex = 21;
			this.ButtonBrowseImportSfCsv.Text = "参照 (&4)";
			this.ButtonBrowseImportSfCsv.UseVisualStyleBackColor = true;
			this.ButtonBrowseImportSfCsv.Click += new System.EventHandler(this.ButtonBrowseImportSfCsv_Click);
			// 
			// TextBoxImportSfCsv
			// 
			this.TextBoxImportSfCsv.Location = new System.Drawing.Point(160, 184);
			this.TextBoxImportSfCsv.Name = "TextBoxImportSfCsv";
			this.TextBoxImportSfCsv.Size = new System.Drawing.Size(264, 19);
			this.TextBoxImportSfCsv.TabIndex = 20;
			this.TextBoxImportSfCsv.TextChanged += new System.EventHandler(this.TextBoxImportAnisonInfoCsv_TextChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(32, 148);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(128, 20);
			this.label3.TabIndex = 19;
			this.label3.Text = "anison.csv (.zip) (&O)：";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ButtonBrowseImportAnisonCsv
			// 
			this.ButtonBrowseImportAnisonCsv.Location = new System.Drawing.Point(432, 144);
			this.ButtonBrowseImportAnisonCsv.Name = "ButtonBrowseImportAnisonCsv";
			this.ButtonBrowseImportAnisonCsv.Size = new System.Drawing.Size(96, 28);
			this.ButtonBrowseImportAnisonCsv.TabIndex = 18;
			this.ButtonBrowseImportAnisonCsv.Text = "参照 (&3)";
			this.ButtonBrowseImportAnisonCsv.UseVisualStyleBackColor = true;
			this.ButtonBrowseImportAnisonCsv.Click += new System.EventHandler(this.ButtonBrowseImportAnisonCsv_Click);
			// 
			// TextBoxImportAnisonCsv
			// 
			this.TextBoxImportAnisonCsv.Location = new System.Drawing.Point(160, 148);
			this.TextBoxImportAnisonCsv.Name = "TextBoxImportAnisonCsv";
			this.TextBoxImportAnisonCsv.Size = new System.Drawing.Size(264, 19);
			this.TextBoxImportAnisonCsv.TabIndex = 17;
			this.TextBoxImportAnisonCsv.TextChanged += new System.EventHandler(this.TextBoxImportAnisonInfoCsv_TextChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 112);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(128, 20);
			this.label2.TabIndex = 16;
			this.label2.Text = "program.csv (.zip) (&P)：";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ButtonBrowseImportProgramCsv
			// 
			this.ButtonBrowseImportProgramCsv.Location = new System.Drawing.Point(432, 108);
			this.ButtonBrowseImportProgramCsv.Name = "ButtonBrowseImportProgramCsv";
			this.ButtonBrowseImportProgramCsv.Size = new System.Drawing.Size(96, 28);
			this.ButtonBrowseImportProgramCsv.TabIndex = 15;
			this.ButtonBrowseImportProgramCsv.Text = "参照 (&2)";
			this.ButtonBrowseImportProgramCsv.UseVisualStyleBackColor = true;
			this.ButtonBrowseImportProgramCsv.Click += new System.EventHandler(this.ButtonBrowseImportProgramCsv_Click);
			// 
			// TextBoxImportProgramCsv
			// 
			this.TextBoxImportProgramCsv.Location = new System.Drawing.Point(160, 112);
			this.TextBoxImportProgramCsv.Name = "TextBoxImportProgramCsv";
			this.TextBoxImportProgramCsv.Size = new System.Drawing.Size(264, 19);
			this.TextBoxImportProgramCsv.TabIndex = 14;
			this.TextBoxImportProgramCsv.TextChanged += new System.EventHandler(this.TextBoxImportAnisonInfoCsv_TextChanged);
			// 
			// RadioButtonImportAnisonInfoCsv
			// 
			this.RadioButtonImportAnisonInfoCsv.AutoSize = true;
			this.RadioButtonImportAnisonInfoCsv.Location = new System.Drawing.Point(16, 80);
			this.RadioButtonImportAnisonInfoCsv.Name = "RadioButtonImportAnisonInfoCsv";
			this.RadioButtonImportAnisonInfoCsv.Size = new System.Drawing.Size(183, 16);
			this.RadioButtonImportAnisonInfoCsv.TabIndex = 13;
			this.RadioButtonImportAnisonInfoCsv.Text = "anison.info CSV をインポート (&A)";
			this.RadioButtonImportAnisonInfoCsv.UseVisualStyleBackColor = true;
			this.RadioButtonImportAnisonInfoCsv.CheckedChanged += new System.EventHandler(this.RadioButtonImport_CheckedChanged);
			// 
			// ButtonBrowseImportYukaLister
			// 
			this.ButtonBrowseImportYukaLister.Location = new System.Drawing.Point(432, 40);
			this.ButtonBrowseImportYukaLister.Name = "ButtonBrowseImportYukaLister";
			this.ButtonBrowseImportYukaLister.Size = new System.Drawing.Size(96, 28);
			this.ButtonBrowseImportYukaLister.TabIndex = 12;
			this.ButtonBrowseImportYukaLister.Text = "参照 (&1)";
			this.ButtonBrowseImportYukaLister.UseVisualStyleBackColor = true;
			// 
			// TextBoxImportYukaLister
			// 
			this.TextBoxImportYukaLister.Location = new System.Drawing.Point(32, 44);
			this.TextBoxImportYukaLister.Name = "TextBoxImportYukaLister";
			this.TextBoxImportYukaLister.Size = new System.Drawing.Size(392, 19);
			this.TextBoxImportYukaLister.TabIndex = 1;
			// 
			// RadioButtonImportYukaLister
			// 
			this.RadioButtonImportYukaLister.AutoSize = true;
			this.RadioButtonImportYukaLister.Checked = true;
			this.RadioButtonImportYukaLister.Location = new System.Drawing.Point(16, 16);
			this.RadioButtonImportYukaLister.Name = "RadioButtonImportYukaLister";
			this.RadioButtonImportYukaLister.Size = new System.Drawing.Size(270, 16);
			this.RadioButtonImportYukaLister.TabIndex = 0;
			this.RadioButtonImportYukaLister.TabStop = true;
			this.RadioButtonImportYukaLister.Text = "ゆかりすたーでエクスポートしたファイルをインポート (&Y)";
			this.RadioButtonImportYukaLister.UseVisualStyleBackColor = true;
			this.RadioButtonImportYukaLister.CheckedChanged += new System.EventHandler(this.RadioButtonImport_CheckedChanged);
			// 
			// TabPageExport
			// 
			this.TabPageExport.BackColor = System.Drawing.SystemColors.Control;
			this.TabPageExport.Location = new System.Drawing.Point(4, 22);
			this.TabPageExport.Name = "TabPageExport";
			this.TabPageExport.Size = new System.Drawing.Size(544, 382);
			this.TabPageExport.TabIndex = 3;
			this.TabPageExport.Text = "エクスポート";
			// 
			// LabelDescription
			// 
			this.LabelDescription.Location = new System.Drawing.Point(16, 420);
			this.LabelDescription.Name = "LabelDescription";
			this.LabelDescription.Size = new System.Drawing.Size(296, 20);
			this.LabelDescription.TabIndex = 23;
			this.LabelDescription.Text = "設定やデータベースのメンテナンス等を行います。";
			this.LabelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LinkLabelHelp
			// 
			this.LinkLabelHelp.Location = new System.Drawing.Point(16, 440);
			this.LinkLabelHelp.Name = "LinkLabelHelp";
			this.LinkLabelHelp.Size = new System.Drawing.Size(64, 20);
			this.LinkLabelHelp.TabIndex = 24;
			this.LinkLabelHelp.TabStop = true;
			this.LinkLabelHelp.Text = "詳細情報";
			this.LinkLabelHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.LinkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelHelp_LinkClicked);
			// 
			// FormYukaListerSettings
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(553, 467);
			this.Controls.Add(this.LinkLabelHelp);
			this.Controls.Add(this.LabelDescription);
			this.Controls.Add(this.TabControlYukaListerSettings);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormYukaListerSettings";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormYukaListerSettings_FormClosed);
			this.Load += new System.EventHandler(this.FormYukaListerSettings_Load);
			this.Shown += new System.EventHandler(this.FormYukaListerSettings_Shown);
			this.TabControlYukaListerSettings.ResumeLayout(false);
			this.TabPageSettings.ResumeLayout(false);
			this.TabPageSettings.PerformLayout();
			this.TabPageTarget.ResumeLayout(false);
			this.TabPageTarget.PerformLayout();
			this.TabPageOutput.ResumeLayout(false);
			this.TabPageOutput.PerformLayout();
			this.TabPageMaintenance.ResumeLayout(false);
			this.TabPageMaintenance.PerformLayout();
			this.TabPageImport.ResumeLayout(false);
			this.TabPageImport.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.OpenFileDialog OpenFileDialogYukariConfigPathSeed;
		private System.Windows.Forms.TabControl TabControlYukaListerSettings;
		private System.Windows.Forms.TabPage TabPageSettings;
		private System.Windows.Forms.Button ButtonBrowseYukariConfigPathSeed;
		private System.Windows.Forms.TextBox TextBoxYukariConfigPathSeed;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TabPage TabPageMaintenance;
		private System.Windows.Forms.TabPage TabPageImport;
		private System.Windows.Forms.Button ButtonBrowseImportYukaLister;
		private System.Windows.Forms.TextBox TextBoxImportYukaLister;
		private System.Windows.Forms.RadioButton RadioButtonImportYukaLister;
		private System.Windows.Forms.TabPage TabPageExport;
		private System.Windows.Forms.RadioButton RadioButtonImportAnisonInfoCsv;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button ButtonBrowseImportGameCsv;
		private System.Windows.Forms.TextBox TextBoxImportGameCsv;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button ButtonBrowseImportSfCsv;
		private System.Windows.Forms.TextBox TextBoxImportSfCsv;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button ButtonBrowseImportAnisonCsv;
		private System.Windows.Forms.TextBox TextBoxImportAnisonCsv;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button ButtonBrowseImportProgramCsv;
		private System.Windows.Forms.TextBox TextBoxImportProgramCsv;
		private System.Windows.Forms.RadioButton RadioButtonImportNicoKaraLister;
		private System.Windows.Forms.Button ButtonBrowseImportNicoKaraLister;
		private System.Windows.Forms.TextBox TextBoxImportNicoKaraLister;
		private System.Windows.Forms.OpenFileDialog OpenFileDialogMisc;
		private System.Windows.Forms.Button ButtonImport;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.CheckBox CheckBoxSyncMusicInfoDb;
		private System.Windows.Forms.TextBox TextBoxSyncServer;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox TextBoxSyncPassword;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox TextBoxSyncAccount;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TabPage TabPageOutput;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox TextBoxListFolder;
		private System.Windows.Forms.Button ButtonBrowseListFolder;
		private System.Windows.Forms.Button ButtonListSettings;
		private System.Windows.Forms.ComboBox ComboBoxListFormat;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Button ButtonOutputList;
		private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialogOutputList;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TextBox TextBoxYukariListFolder;
		private System.Windows.Forms.Button ButtonYukariListSettings;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Panel panel2;
		internal System.Windows.Forms.Label LabelDescription;
		private System.Windows.Forms.LinkLabel LinkLabelHelp;
		private System.Windows.Forms.TabPage TabPageTarget;
		private System.Windows.Forms.ListBox ListBoxTargetExts;
		private System.Windows.Forms.Button ButtonRemoveExt;
		private System.Windows.Forms.Button ButtonAddExt;
		private System.Windows.Forms.TextBox TextBoxTargetExt;
		private System.Windows.Forms.Label LabelTargetExt;
		private System.Windows.Forms.Label label16;
	}
}