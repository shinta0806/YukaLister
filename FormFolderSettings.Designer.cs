namespace YukaLister
{
	partial class FormFolderSettings
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFolderSettings));
			this.label1 = new System.Windows.Forms.Label();
			this.LabelFolder = new System.Windows.Forms.Label();
			this.LabelSettingsFileStatus = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.ButtonDeleteSettings = new System.Windows.Forms.Button();
			this.TabControlRules = new System.Windows.Forms.TabControl();
			this.TabPageFileNameRules = new System.Windows.Forms.TabPage();
			this.ButtonReplaceFileNameRule = new System.Windows.Forms.Button();
			this.ButtonDownFileNameRule = new System.Windows.Forms.Button();
			this.ButtonUpFileNameRule = new System.Windows.Forms.Button();
			this.ListBoxFileNameRules = new System.Windows.Forms.ListBox();
			this.ButtonDeleteFileNameRule = new System.Windows.Forms.Button();
			this.ButtonAddFileNameRule = new System.Windows.Forms.Button();
			this.ButtonVar = new System.Windows.Forms.Button();
			this.TextBoxFileNameRule = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.TabPageFolderNameRules = new System.Windows.Forms.TabPage();
			this.ComboBoxFolderNameRuleValue = new System.Windows.Forms.ComboBox();
			this.ButtonDownFolderNameRule = new System.Windows.Forms.Button();
			this.ButtonUpFolderNameRule = new System.Windows.Forms.Button();
			this.ListBoxFolderNameRules = new System.Windows.Forms.ListBox();
			this.ButtonDeleteFolderNameRule = new System.Windows.Forms.Button();
			this.ButtonAddFolderNameRule = new System.Windows.Forms.Button();
			this.TextBoxFolderNameRuleValue = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.ComboBoxFolderNameRuleName = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.DataGridViewPreview = new System.Windows.Forms.DataGridView();
			this.ColumnFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnAnalyze = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnEdit = new System.Windows.Forms.DataGridViewButtonColumn();
			this.ButtonPreview = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ContextMenuVarNames = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.LabelPreview = new System.Windows.Forms.Label();
			this.ButtonJump = new System.Windows.Forms.Button();
			this.LinkLabelHelp = new System.Windows.Forms.LinkLabel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.CheckBoxExclude = new System.Windows.Forms.CheckBox();
			this.TabControlRules.SuspendLayout();
			this.TabPageFileNameRules.SuspendLayout();
			this.TabPageFolderNameRules.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewPreview)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(112, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "設定対象フォルダー：";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LabelFolder
			// 
			this.LabelFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelFolder.Location = new System.Drawing.Point(128, 16);
			this.LabelFolder.Name = "LabelFolder";
			this.LabelFolder.Size = new System.Drawing.Size(640, 20);
			this.LabelFolder.TabIndex = 1;
			this.LabelFolder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelSettingsFileStatus
			// 
			this.LabelSettingsFileStatus.AutoSize = true;
			this.LabelSettingsFileStatus.Location = new System.Drawing.Point(128, 36);
			this.LabelSettingsFileStatus.Name = "LabelSettingsFileStatus";
			this.LabelSettingsFileStatus.Size = new System.Drawing.Size(11, 12);
			this.LabelSettingsFileStatus.TabIndex = 2;
			this.LabelSettingsFileStatus.Text = "-";
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Location = new System.Drawing.Point(0, 60);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1000, 5);
			this.panel2.TabIndex = 3;
			// 
			// ButtonDeleteSettings
			// 
			this.ButtonDeleteSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonDeleteSettings.Location = new System.Drawing.Point(16, 593);
			this.ButtonDeleteSettings.Name = "ButtonDeleteSettings";
			this.ButtonDeleteSettings.Size = new System.Drawing.Size(96, 28);
			this.ButtonDeleteSettings.TabIndex = 11;
			this.ButtonDeleteSettings.Text = "設定削除 (&D)";
			this.ButtonDeleteSettings.UseVisualStyleBackColor = true;
			this.ButtonDeleteSettings.Click += new System.EventHandler(this.ButtonDeleteSettings_Click);
			// 
			// TabControlRules
			// 
			this.TabControlRules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TabControlRules.Controls.Add(this.TabPageFileNameRules);
			this.TabControlRules.Controls.Add(this.TabPageFolderNameRules);
			this.TabControlRules.Location = new System.Drawing.Point(8, 72);
			this.TabControlRules.Name = "TabControlRules";
			this.TabControlRules.SelectedIndex = 0;
			this.TabControlRules.Size = new System.Drawing.Size(768, 228);
			this.TabControlRules.TabIndex = 0;
			// 
			// TabPageFileNameRules
			// 
			this.TabPageFileNameRules.BackColor = System.Drawing.SystemColors.Control;
			this.TabPageFileNameRules.Controls.Add(this.ButtonReplaceFileNameRule);
			this.TabPageFileNameRules.Controls.Add(this.ButtonDownFileNameRule);
			this.TabPageFileNameRules.Controls.Add(this.ButtonUpFileNameRule);
			this.TabPageFileNameRules.Controls.Add(this.ListBoxFileNameRules);
			this.TabPageFileNameRules.Controls.Add(this.ButtonDeleteFileNameRule);
			this.TabPageFileNameRules.Controls.Add(this.ButtonAddFileNameRule);
			this.TabPageFileNameRules.Controls.Add(this.ButtonVar);
			this.TabPageFileNameRules.Controls.Add(this.TextBoxFileNameRule);
			this.TabPageFileNameRules.Controls.Add(this.label2);
			this.TabPageFileNameRules.Location = new System.Drawing.Point(4, 22);
			this.TabPageFileNameRules.Name = "TabPageFileNameRules";
			this.TabPageFileNameRules.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageFileNameRules.Size = new System.Drawing.Size(760, 202);
			this.TabPageFileNameRules.TabIndex = 0;
			this.TabPageFileNameRules.Text = "ファイル命名規則";
			// 
			// ButtonReplaceFileNameRule
			// 
			this.ButtonReplaceFileNameRule.Location = new System.Drawing.Point(312, 64);
			this.ButtonReplaceFileNameRule.Name = "ButtonReplaceFileNameRule";
			this.ButtonReplaceFileNameRule.Size = new System.Drawing.Size(96, 28);
			this.ButtonReplaceFileNameRule.TabIndex = 4;
			this.ButtonReplaceFileNameRule.Text = "↓　置換 (&2)";
			this.ButtonReplaceFileNameRule.UseVisualStyleBackColor = true;
			this.ButtonReplaceFileNameRule.Click += new System.EventHandler(this.ButtonReplaceFileNameRule_Click);
			// 
			// ButtonDownFileNameRule
			// 
			this.ButtonDownFileNameRule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonDownFileNameRule.Location = new System.Drawing.Point(712, 148);
			this.ButtonDownFileNameRule.Name = "ButtonDownFileNameRule";
			this.ButtonDownFileNameRule.Size = new System.Drawing.Size(32, 28);
			this.ButtonDownFileNameRule.TabIndex = 8;
			this.ButtonDownFileNameRule.Text = "↓";
			this.ButtonDownFileNameRule.UseVisualStyleBackColor = true;
			this.ButtonDownFileNameRule.Click += new System.EventHandler(this.ButtonDownFileNameRule_Click);
			// 
			// ButtonUpFileNameRule
			// 
			this.ButtonUpFileNameRule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonUpFileNameRule.Location = new System.Drawing.Point(712, 112);
			this.ButtonUpFileNameRule.Name = "ButtonUpFileNameRule";
			this.ButtonUpFileNameRule.Size = new System.Drawing.Size(32, 28);
			this.ButtonUpFileNameRule.TabIndex = 7;
			this.ButtonUpFileNameRule.Text = "↑";
			this.ButtonUpFileNameRule.UseVisualStyleBackColor = true;
			this.ButtonUpFileNameRule.Click += new System.EventHandler(this.ButtonUpFileNameRule_Click);
			// 
			// ListBoxFileNameRules
			// 
			this.ListBoxFileNameRules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ListBoxFileNameRules.FormattingEnabled = true;
			this.ListBoxFileNameRules.ItemHeight = 12;
			this.ListBoxFileNameRules.Location = new System.Drawing.Point(16, 100);
			this.ListBoxFileNameRules.Name = "ListBoxFileNameRules";
			this.ListBoxFileNameRules.Size = new System.Drawing.Size(688, 88);
			this.ListBoxFileNameRules.TabIndex = 6;
			this.ListBoxFileNameRules.SelectedIndexChanged += new System.EventHandler(this.ListBoxFileNameRules_SelectedIndexChanged);
			this.ListBoxFileNameRules.DoubleClick += new System.EventHandler(this.ListBoxFileNameRules_DoubleClick);
			// 
			// ButtonDeleteFileNameRule
			// 
			this.ButtonDeleteFileNameRule.Location = new System.Drawing.Point(424, 64);
			this.ButtonDeleteFileNameRule.Name = "ButtonDeleteFileNameRule";
			this.ButtonDeleteFileNameRule.Size = new System.Drawing.Size(96, 28);
			this.ButtonDeleteFileNameRule.TabIndex = 5;
			this.ButtonDeleteFileNameRule.Text = "×　削除 (&3)";
			this.ButtonDeleteFileNameRule.UseVisualStyleBackColor = true;
			this.ButtonDeleteFileNameRule.Click += new System.EventHandler(this.ButtonDeleteFileNameRule_Click);
			// 
			// ButtonAddFileNameRule
			// 
			this.ButtonAddFileNameRule.Location = new System.Drawing.Point(200, 64);
			this.ButtonAddFileNameRule.Name = "ButtonAddFileNameRule";
			this.ButtonAddFileNameRule.Size = new System.Drawing.Size(96, 28);
			this.ButtonAddFileNameRule.TabIndex = 3;
			this.ButtonAddFileNameRule.Text = "↓　追加 (&1)";
			this.ButtonAddFileNameRule.UseVisualStyleBackColor = true;
			this.ButtonAddFileNameRule.Click += new System.EventHandler(this.ButtonAddFileNameRule_Click);
			// 
			// ButtonVar
			// 
			this.ButtonVar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonVar.Location = new System.Drawing.Point(712, 32);
			this.ButtonVar.Name = "ButtonVar";
			this.ButtonVar.Size = new System.Drawing.Size(32, 28);
			this.ButtonVar.TabIndex = 2;
			this.ButtonVar.Text = "<->";
			this.ButtonVar.UseVisualStyleBackColor = true;
			this.ButtonVar.Click += new System.EventHandler(this.ButtonVar_Click);
			// 
			// TextBoxFileNameRule
			// 
			this.TextBoxFileNameRule.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxFileNameRule.Location = new System.Drawing.Point(16, 36);
			this.TextBoxFileNameRule.Name = "TextBoxFileNameRule";
			this.TextBoxFileNameRule.Size = new System.Drawing.Size(688, 19);
			this.TextBoxFileNameRule.TabIndex = 1;
			this.TextBoxFileNameRule.TextChanged += new System.EventHandler(this.TextBoxFileNameRule_TextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(16, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(360, 12);
			this.label2.TabIndex = 0;
			this.label2.Text = "このフォルダー内にあるニコカラファイルの命名規則 (&R)　※拡張子は除きます";
			// 
			// TabPageFolderNameRules
			// 
			this.TabPageFolderNameRules.BackColor = System.Drawing.SystemColors.Control;
			this.TabPageFolderNameRules.Controls.Add(this.ComboBoxFolderNameRuleValue);
			this.TabPageFolderNameRules.Controls.Add(this.ButtonDownFolderNameRule);
			this.TabPageFolderNameRules.Controls.Add(this.ButtonUpFolderNameRule);
			this.TabPageFolderNameRules.Controls.Add(this.ListBoxFolderNameRules);
			this.TabPageFolderNameRules.Controls.Add(this.ButtonDeleteFolderNameRule);
			this.TabPageFolderNameRules.Controls.Add(this.ButtonAddFolderNameRule);
			this.TabPageFolderNameRules.Controls.Add(this.TextBoxFolderNameRuleValue);
			this.TabPageFolderNameRules.Controls.Add(this.label5);
			this.TabPageFolderNameRules.Controls.Add(this.ComboBoxFolderNameRuleName);
			this.TabPageFolderNameRules.Controls.Add(this.label4);
			this.TabPageFolderNameRules.Controls.Add(this.label3);
			this.TabPageFolderNameRules.Location = new System.Drawing.Point(4, 22);
			this.TabPageFolderNameRules.Name = "TabPageFolderNameRules";
			this.TabPageFolderNameRules.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageFolderNameRules.Size = new System.Drawing.Size(760, 202);
			this.TabPageFolderNameRules.TabIndex = 1;
			this.TabPageFolderNameRules.Text = "固定値項目";
			// 
			// ComboBoxFolderNameRuleValue
			// 
			this.ComboBoxFolderNameRuleValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboBoxFolderNameRuleValue.FormattingEnabled = true;
			this.ComboBoxFolderNameRuleValue.Location = new System.Drawing.Point(416, 36);
			this.ComboBoxFolderNameRuleValue.Name = "ComboBoxFolderNameRuleValue";
			this.ComboBoxFolderNameRuleValue.Size = new System.Drawing.Size(328, 20);
			this.ComboBoxFolderNameRuleValue.TabIndex = 10;
			this.ComboBoxFolderNameRuleValue.SelectedIndexChanged += new System.EventHandler(this.ComboBoxFolderNameRuleValue_SelectedIndexChanged);
			// 
			// ButtonDownFolderNameRule
			// 
			this.ButtonDownFolderNameRule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonDownFolderNameRule.Location = new System.Drawing.Point(712, 148);
			this.ButtonDownFolderNameRule.Name = "ButtonDownFolderNameRule";
			this.ButtonDownFolderNameRule.Size = new System.Drawing.Size(32, 28);
			this.ButtonDownFolderNameRule.TabIndex = 9;
			this.ButtonDownFolderNameRule.Text = "↓";
			this.ButtonDownFolderNameRule.UseVisualStyleBackColor = true;
			this.ButtonDownFolderNameRule.Click += new System.EventHandler(this.ButtonDownFolderNameRule_Click);
			// 
			// ButtonUpFolderNameRule
			// 
			this.ButtonUpFolderNameRule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonUpFolderNameRule.Location = new System.Drawing.Point(712, 112);
			this.ButtonUpFolderNameRule.Name = "ButtonUpFolderNameRule";
			this.ButtonUpFolderNameRule.Size = new System.Drawing.Size(32, 28);
			this.ButtonUpFolderNameRule.TabIndex = 8;
			this.ButtonUpFolderNameRule.Text = "↑";
			this.ButtonUpFolderNameRule.UseVisualStyleBackColor = true;
			this.ButtonUpFolderNameRule.Click += new System.EventHandler(this.ButtonUpFolderNameRule_Click);
			// 
			// ListBoxFolderNameRules
			// 
			this.ListBoxFolderNameRules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ListBoxFolderNameRules.FormattingEnabled = true;
			this.ListBoxFolderNameRules.ItemHeight = 12;
			this.ListBoxFolderNameRules.Location = new System.Drawing.Point(16, 100);
			this.ListBoxFolderNameRules.Name = "ListBoxFolderNameRules";
			this.ListBoxFolderNameRules.Size = new System.Drawing.Size(688, 88);
			this.ListBoxFolderNameRules.TabIndex = 7;
			this.ListBoxFolderNameRules.SelectedIndexChanged += new System.EventHandler(this.ListBoxFolderNameRules_SelectedIndexChanged);
			this.ListBoxFolderNameRules.DoubleClick += new System.EventHandler(this.ListBoxFolderNameRules_DoubleClick);
			// 
			// ButtonDeleteFolderNameRule
			// 
			this.ButtonDeleteFolderNameRule.Location = new System.Drawing.Point(368, 64);
			this.ButtonDeleteFolderNameRule.Name = "ButtonDeleteFolderNameRule";
			this.ButtonDeleteFolderNameRule.Size = new System.Drawing.Size(96, 28);
			this.ButtonDeleteFolderNameRule.TabIndex = 6;
			this.ButtonDeleteFolderNameRule.Text = "×　削除 (&5)";
			this.ButtonDeleteFolderNameRule.UseVisualStyleBackColor = true;
			this.ButtonDeleteFolderNameRule.Click += new System.EventHandler(this.ButtonDeleteFolderNameRule_Click);
			// 
			// ButtonAddFolderNameRule
			// 
			this.ButtonAddFolderNameRule.Location = new System.Drawing.Point(256, 64);
			this.ButtonAddFolderNameRule.Name = "ButtonAddFolderNameRule";
			this.ButtonAddFolderNameRule.Size = new System.Drawing.Size(96, 28);
			this.ButtonAddFolderNameRule.TabIndex = 5;
			this.ButtonAddFolderNameRule.Text = "↓　追加 (&4)";
			this.ButtonAddFolderNameRule.UseVisualStyleBackColor = true;
			this.ButtonAddFolderNameRule.Click += new System.EventHandler(this.ButtonAddFolderNameRule_Click);
			// 
			// TextBoxFolderNameRuleValue
			// 
			this.TextBoxFolderNameRuleValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxFolderNameRuleValue.Location = new System.Drawing.Point(416, 36);
			this.TextBoxFolderNameRuleValue.Name = "TextBoxFolderNameRuleValue";
			this.TextBoxFolderNameRuleValue.Size = new System.Drawing.Size(328, 19);
			this.TextBoxFolderNameRuleValue.TabIndex = 4;
			this.TextBoxFolderNameRuleValue.TextChanged += new System.EventHandler(this.TextBoxFolderNameRuleValue_TextChanged);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(368, 36);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(48, 20);
			this.label5.TabIndex = 3;
			this.label5.Text = "値 (&V)：";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ComboBoxFolderNameRuleName
			// 
			this.ComboBoxFolderNameRuleName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboBoxFolderNameRuleName.FormattingEnabled = true;
			this.ComboBoxFolderNameRuleName.Location = new System.Drawing.Point(72, 36);
			this.ComboBoxFolderNameRuleName.Name = "ComboBoxFolderNameRuleName";
			this.ComboBoxFolderNameRuleName.Size = new System.Drawing.Size(280, 20);
			this.ComboBoxFolderNameRuleName.TabIndex = 2;
			this.ComboBoxFolderNameRuleName.SelectedIndexChanged += new System.EventHandler(this.ComboBoxFolderNameRuleName_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 36);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 20);
			this.label4.TabIndex = 1;
			this.label4.Text = "項目 (&K)：";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(16, 16);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(274, 12);
			this.label3.TabIndex = 0;
			this.label3.Text = "このフォルダー内にあるニコカラファイルに一律で適用する値";
			// 
			// DataGridViewPreview
			// 
			this.DataGridViewPreview.AllowUserToAddRows = false;
			this.DataGridViewPreview.AllowUserToDeleteRows = false;
			this.DataGridViewPreview.AllowUserToResizeRows = false;
			this.DataGridViewPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DataGridViewPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.DataGridViewPreview.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnFile,
            this.ColumnAnalyze,
            this.ColumnEdit});
			this.DataGridViewPreview.Location = new System.Drawing.Point(16, 352);
			this.DataGridViewPreview.MultiSelect = false;
			this.DataGridViewPreview.Name = "DataGridViewPreview";
			this.DataGridViewPreview.ReadOnly = true;
			this.DataGridViewPreview.RowHeadersVisible = false;
			this.DataGridViewPreview.RowTemplate.Height = 21;
			this.DataGridViewPreview.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.DataGridViewPreview.Size = new System.Drawing.Size(752, 172);
			this.DataGridViewPreview.TabIndex = 8;
			this.DataGridViewPreview.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewPreview_CellContentClick);
			this.DataGridViewPreview.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewPreview_CellContentDoubleClick);
			this.DataGridViewPreview.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewPreview_CellDoubleClick);
			// 
			// ColumnFile
			// 
			this.ColumnFile.HeaderText = "ファイル";
			this.ColumnFile.Name = "ColumnFile";
			this.ColumnFile.ReadOnly = true;
			this.ColumnFile.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// ColumnAnalyze
			// 
			this.ColumnAnalyze.HeaderText = "項目と値";
			this.ColumnAnalyze.Name = "ColumnAnalyze";
			this.ColumnAnalyze.ReadOnly = true;
			this.ColumnAnalyze.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// ColumnEdit
			// 
			this.ColumnEdit.HeaderText = "編集";
			this.ColumnEdit.Name = "ColumnEdit";
			this.ColumnEdit.ReadOnly = true;
			// 
			// ButtonPreview
			// 
			this.ButtonPreview.Location = new System.Drawing.Point(16, 312);
			this.ButtonPreview.Name = "ButtonPreview";
			this.ButtonPreview.Size = new System.Drawing.Size(96, 28);
			this.ButtonPreview.TabIndex = 5;
			this.ButtonPreview.Text = "ファイル検索 (&S)";
			this.ButtonPreview.UseVisualStyleBackColor = true;
			this.ButtonPreview.Click += new System.EventHandler(this.ButtonPreview_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Location = new System.Drawing.Point(0, 577);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1000, 5);
			this.panel1.TabIndex = 9;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOK.Location = new System.Drawing.Point(560, 593);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(96, 28);
			this.ButtonOK.TabIndex = 12;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(672, 593);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 13;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ContextMenuVarNames
			// 
			this.ContextMenuVarNames.Name = "ContextMenuVarNames";
			this.ContextMenuVarNames.Size = new System.Drawing.Size(61, 4);
			// 
			// LabelPreview
			// 
			this.LabelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelPreview.Location = new System.Drawing.Point(120, 316);
			this.LabelPreview.Name = "LabelPreview";
			this.LabelPreview.Size = new System.Drawing.Size(544, 20);
			this.LabelPreview.TabIndex = 6;
			this.LabelPreview.Text = "フォルダー内のファイルの名前がどのように解析されるか確認できます。";
			this.LabelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ButtonJump
			// 
			this.ButtonJump.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonJump.Location = new System.Drawing.Point(672, 312);
			this.ButtonJump.Name = "ButtonJump";
			this.ButtonJump.Size = new System.Drawing.Size(96, 28);
			this.ButtonJump.TabIndex = 7;
			this.ButtonJump.Text = "未登録検出 (&E)";
			this.ButtonJump.UseVisualStyleBackColor = true;
			this.ButtonJump.Click += new System.EventHandler(this.ButtonJump_Click);
			// 
			// LinkLabelHelp
			// 
			this.LinkLabelHelp.AutoSize = true;
			this.LinkLabelHelp.Location = new System.Drawing.Point(144, 36);
			this.LinkLabelHelp.Name = "LinkLabelHelp";
			this.LinkLabelHelp.Size = new System.Drawing.Size(53, 12);
			this.LinkLabelHelp.TabIndex = 14;
			this.LinkLabelHelp.TabStop = true;
			this.LinkLabelHelp.Text = "詳細情報";
			this.LinkLabelHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.LinkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelHelp_LinkClicked);
			// 
			// panel3
			// 
			this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel3.Location = new System.Drawing.Point(0, 536);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(1000, 5);
			this.panel3.TabIndex = 15;
			// 
			// CheckBoxExclude
			// 
			this.CheckBoxExclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CheckBoxExclude.AutoSize = true;
			this.CheckBoxExclude.Location = new System.Drawing.Point(16, 552);
			this.CheckBoxExclude.Name = "CheckBoxExclude";
			this.CheckBoxExclude.Size = new System.Drawing.Size(200, 16);
			this.CheckBoxExclude.TabIndex = 16;
			this.CheckBoxExclude.Text = "このフォルダーを検索対象としない (&E)";
			this.CheckBoxExclude.UseVisualStyleBackColor = true;
			this.CheckBoxExclude.CheckedChanged += new System.EventHandler(this.CheckBoxExclude_CheckedChanged);
			// 
			// FormFolderSettings
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(784, 635);
			this.Controls.Add(this.CheckBoxExclude);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.LinkLabelHelp);
			this.Controls.Add(this.ButtonJump);
			this.Controls.Add(this.LabelPreview);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.ButtonPreview);
			this.Controls.Add(this.DataGridViewPreview);
			this.Controls.Add(this.TabControlRules);
			this.Controls.Add(this.ButtonDeleteSettings);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.LabelSettingsFileStatus);
			this.Controls.Add(this.LabelFolder);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.Name = "FormFolderSettings";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormFolderSettings_FormClosed);
			this.Load += new System.EventHandler(this.FormFolderSettings_Load);
			this.Shown += new System.EventHandler(this.FormFolderSettings_Shown);
			this.TabControlRules.ResumeLayout(false);
			this.TabPageFileNameRules.ResumeLayout(false);
			this.TabPageFileNameRules.PerformLayout();
			this.TabPageFolderNameRules.ResumeLayout(false);
			this.TabPageFolderNameRules.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DataGridViewPreview)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label LabelFolder;
		private System.Windows.Forms.Label LabelSettingsFileStatus;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button ButtonDeleteSettings;
		private System.Windows.Forms.TabControl TabControlRules;
		private System.Windows.Forms.TabPage TabPageFileNameRules;
		private System.Windows.Forms.Button ButtonDownFileNameRule;
		private System.Windows.Forms.Button ButtonUpFileNameRule;
		private System.Windows.Forms.ListBox ListBoxFileNameRules;
		private System.Windows.Forms.Button ButtonDeleteFileNameRule;
		private System.Windows.Forms.Button ButtonAddFileNameRule;
		private System.Windows.Forms.Button ButtonVar;
		private System.Windows.Forms.TextBox TextBoxFileNameRule;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TabPage TabPageFolderNameRules;
		private System.Windows.Forms.ComboBox ComboBoxFolderNameRuleName;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button ButtonDownFolderNameRule;
		private System.Windows.Forms.Button ButtonUpFolderNameRule;
		private System.Windows.Forms.ListBox ListBoxFolderNameRules;
		private System.Windows.Forms.Button ButtonDeleteFolderNameRule;
		private System.Windows.Forms.Button ButtonAddFolderNameRule;
		private System.Windows.Forms.TextBox TextBoxFolderNameRuleValue;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.DataGridView DataGridViewPreview;
		private System.Windows.Forms.Button ButtonPreview;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonReplaceFileNameRule;
		private System.Windows.Forms.ContextMenuStrip ContextMenuVarNames;
		private System.Windows.Forms.Label LabelPreview;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFile;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAnalyze;
		private System.Windows.Forms.DataGridViewButtonColumn ColumnEdit;
		private System.Windows.Forms.Button ButtonJump;
		private System.Windows.Forms.ComboBox ComboBoxFolderNameRuleValue;
		private System.Windows.Forms.LinkLabel LinkLabelHelp;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.CheckBox CheckBoxExclude;
	}
}