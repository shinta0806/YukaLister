namespace YukaLister
{
	partial class FormEditSong
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditSong));
			this.LabelIdInfo = new System.Windows.Forms.Label();
			this.ComboBoxId = new System.Windows.Forms.ComboBox();
			this.label22 = new System.Windows.Forms.Label();
			this.TextBoxName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.TextBoxRuby = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.LabelTieUp = new System.Windows.Forms.Label();
			this.ButtonSearchTieUp = new System.Windows.Forms.Button();
			this.ButtonEditTieUp = new System.Windows.Forms.Button();
			this.LabelCategory = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.CheckBoxTieUp = new System.Windows.Forms.CheckBox();
			this.CheckBoxCategory = new System.Windows.Forms.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.TextBoxOpEd = new System.Windows.Forms.TextBox();
			this.ButtonSelectOpEd = new System.Windows.Forms.Button();
			this.CheckBoxArtist = new System.Windows.Forms.CheckBox();
			this.ButtonEditArtist = new System.Windows.Forms.Button();
			this.ButtonSearchArtist = new System.Windows.Forms.Button();
			this.LabelArtist = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.CheckBoxLyrist = new System.Windows.Forms.CheckBox();
			this.ButtonEditLyrist = new System.Windows.Forms.Button();
			this.ButtonSearchLyrist = new System.Windows.Forms.Button();
			this.LabelLyrist = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.CheckBoxComposer = new System.Windows.Forms.CheckBox();
			this.ButtonEditComposer = new System.Windows.Forms.Button();
			this.ButtonSearchComposer = new System.Windows.Forms.Button();
			this.LabelComposer = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.CheckBoxArranger = new System.Windows.Forms.CheckBox();
			this.ButtonEditArranger = new System.Windows.Forms.Button();
			this.ButtonSearchArranger = new System.Windows.Forms.Button();
			this.LabelArranger = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.TextBoxReleaseYear = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.TextBoxReleaseMonth = new System.Windows.Forms.TextBox();
			this.label19 = new System.Windows.Forms.Label();
			this.TextBoxReleaseDay = new System.Windows.Forms.TextBox();
			this.label20 = new System.Windows.Forms.Label();
			this.ButtonSelectCategory = new System.Windows.Forms.Button();
			this.label16 = new System.Windows.Forms.Label();
			this.TextBoxKeyword = new System.Windows.Forms.TextBox();
			this.label21 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.ContextMenuStripOpEds = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ContextMenuStripCategories = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ButtonSameLyrist = new System.Windows.Forms.Button();
			this.ButtonSameComposer = new System.Windows.Forms.Button();
			this.ButtonSameArranger = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// LabelIdInfo
			// 
			this.LabelIdInfo.Location = new System.Drawing.Point(416, 16);
			this.LabelIdInfo.Name = "LabelIdInfo";
			this.LabelIdInfo.Size = new System.Drawing.Size(240, 20);
			this.LabelIdInfo.TabIndex = 5;
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
			this.ComboBoxId.TabIndex = 4;
			this.ComboBoxId.SelectedIndexChanged += new System.EventHandler(this.ComboBoxId_SelectedIndexChanged);
			// 
			// label22
			// 
			this.label22.Location = new System.Drawing.Point(16, 16);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(96, 20);
			this.label22.TabIndex = 3;
			this.label22.Text = "楽曲 ID：";
			this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxName
			// 
			this.TextBoxName.Location = new System.Drawing.Point(112, 96);
			this.TextBoxName.Name = "TextBoxName";
			this.TextBoxName.Size = new System.Drawing.Size(296, 19);
			this.TextBoxName.TabIndex = 14;
			this.TextBoxName.Leave += new System.EventHandler(this.TextBoxName_Leave);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 20);
			this.label1.TabIndex = 15;
			this.label1.Text = "楽曲名 (&N)：";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Location = new System.Drawing.Point(0, 52);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1307, 5);
			this.panel2.TabIndex = 16;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 68);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 20);
			this.label2.TabIndex = 18;
			this.label2.Text = "フリガナ (&F)：";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxRuby
			// 
			this.TextBoxRuby.Location = new System.Drawing.Point(112, 68);
			this.TextBoxRuby.Name = "TextBoxRuby";
			this.TextBoxRuby.Size = new System.Drawing.Size(296, 19);
			this.TextBoxRuby.TabIndex = 17;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 152);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(96, 20);
			this.label3.TabIndex = 19;
			this.label3.Text = "タイアップあり (&T)：";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LabelTieUp
			// 
			this.LabelTieUp.Location = new System.Drawing.Point(208, 152);
			this.LabelTieUp.Name = "LabelTieUp";
			this.LabelTieUp.Size = new System.Drawing.Size(200, 20);
			this.LabelTieUp.TabIndex = 21;
			this.LabelTieUp.Text = "-";
			this.LabelTieUp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ButtonSearchTieUp
			// 
			this.ButtonSearchTieUp.Location = new System.Drawing.Point(136, 152);
			this.ButtonSearchTieUp.Name = "ButtonSearchTieUp";
			this.ButtonSearchTieUp.Size = new System.Drawing.Size(64, 20);
			this.ButtonSearchTieUp.TabIndex = 22;
			this.ButtonSearchTieUp.Text = "検索";
			this.ButtonSearchTieUp.UseVisualStyleBackColor = true;
			this.ButtonSearchTieUp.Click += new System.EventHandler(this.ButtonSearchTieUp_Click);
			// 
			// ButtonEditTieUp
			// 
			this.ButtonEditTieUp.Location = new System.Drawing.Point(408, 152);
			this.ButtonEditTieUp.Name = "ButtonEditTieUp";
			this.ButtonEditTieUp.Size = new System.Drawing.Size(112, 20);
			this.ButtonEditTieUp.TabIndex = 23;
			this.ButtonEditTieUp.Text = "タイアップ詳細編集";
			this.ButtonEditTieUp.UseVisualStyleBackColor = true;
			this.ButtonEditTieUp.Click += new System.EventHandler(this.ButtonEditTieUp_Click);
			// 
			// LabelCategory
			// 
			this.LabelCategory.Location = new System.Drawing.Point(208, 236);
			this.LabelCategory.Name = "LabelCategory";
			this.LabelCategory.Size = new System.Drawing.Size(200, 20);
			this.LabelCategory.TabIndex = 25;
			this.LabelCategory.Text = "-";
			this.LabelCategory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 236);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(96, 20);
			this.label6.TabIndex = 24;
			this.label6.Text = "カテゴリーあり (&C)：";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CheckBoxTieUp
			// 
			this.CheckBoxTieUp.Location = new System.Drawing.Point(112, 152);
			this.CheckBoxTieUp.Name = "CheckBoxTieUp";
			this.CheckBoxTieUp.Size = new System.Drawing.Size(16, 20);
			this.CheckBoxTieUp.TabIndex = 27;
			this.CheckBoxTieUp.UseVisualStyleBackColor = true;
			this.CheckBoxTieUp.CheckedChanged += new System.EventHandler(this.CheckBoxTieUp_CheckedChanged);
			// 
			// CheckBoxCategory
			// 
			this.CheckBoxCategory.Location = new System.Drawing.Point(112, 236);
			this.CheckBoxCategory.Name = "CheckBoxCategory";
			this.CheckBoxCategory.Size = new System.Drawing.Size(16, 20);
			this.CheckBoxCategory.TabIndex = 28;
			this.CheckBoxCategory.UseVisualStyleBackColor = true;
			this.CheckBoxCategory.CheckedChanged += new System.EventHandler(this.CheckBoxCategory_CheckedChanged);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 180);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(96, 20);
			this.label7.TabIndex = 30;
			this.label7.Text = "摘要 (&T)：";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxOpEd
			// 
			this.TextBoxOpEd.Location = new System.Drawing.Point(208, 180);
			this.TextBoxOpEd.Name = "TextBoxOpEd";
			this.TextBoxOpEd.Size = new System.Drawing.Size(200, 19);
			this.TextBoxOpEd.TabIndex = 29;
			// 
			// ButtonSelectOpEd
			// 
			this.ButtonSelectOpEd.Location = new System.Drawing.Point(136, 180);
			this.ButtonSelectOpEd.Name = "ButtonSelectOpEd";
			this.ButtonSelectOpEd.Size = new System.Drawing.Size(64, 20);
			this.ButtonSelectOpEd.TabIndex = 31;
			this.ButtonSelectOpEd.Text = "選択";
			this.ButtonSelectOpEd.UseVisualStyleBackColor = true;
			this.ButtonSelectOpEd.Click += new System.EventHandler(this.ButtonSelectOpEd_Click);
			// 
			// CheckBoxArtist
			// 
			this.CheckBoxArtist.Location = new System.Drawing.Point(632, 68);
			this.CheckBoxArtist.Name = "CheckBoxArtist";
			this.CheckBoxArtist.Size = new System.Drawing.Size(16, 20);
			this.CheckBoxArtist.TabIndex = 36;
			this.CheckBoxArtist.UseVisualStyleBackColor = true;
			this.CheckBoxArtist.CheckedChanged += new System.EventHandler(this.CheckBoxArtist_CheckedChanged);
			// 
			// ButtonEditArtist
			// 
			this.ButtonEditArtist.Location = new System.Drawing.Point(976, 68);
			this.ButtonEditArtist.Name = "ButtonEditArtist";
			this.ButtonEditArtist.Size = new System.Drawing.Size(112, 20);
			this.ButtonEditArtist.TabIndex = 35;
			this.ButtonEditArtist.Text = "歌手詳細編集";
			this.ButtonEditArtist.UseVisualStyleBackColor = true;
			this.ButtonEditArtist.Click += new System.EventHandler(this.ButtonEditArtist_Click);
			// 
			// ButtonSearchArtist
			// 
			this.ButtonSearchArtist.Location = new System.Drawing.Point(656, 68);
			this.ButtonSearchArtist.Name = "ButtonSearchArtist";
			this.ButtonSearchArtist.Size = new System.Drawing.Size(64, 20);
			this.ButtonSearchArtist.TabIndex = 34;
			this.ButtonSearchArtist.Text = "検索";
			this.ButtonSearchArtist.UseVisualStyleBackColor = true;
			this.ButtonSearchArtist.Click += new System.EventHandler(this.ButtonSearchArtist_Click);
			// 
			// LabelArtist
			// 
			this.LabelArtist.Location = new System.Drawing.Point(728, 68);
			this.LabelArtist.Name = "LabelArtist";
			this.LabelArtist.Size = new System.Drawing.Size(200, 20);
			this.LabelArtist.TabIndex = 33;
			this.LabelArtist.Text = "-";
			this.LabelArtist.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(536, 68);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(96, 20);
			this.label9.TabIndex = 32;
			this.label9.Text = "歌手あり (&A)：";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CheckBoxLyrist
			// 
			this.CheckBoxLyrist.Location = new System.Drawing.Point(632, 96);
			this.CheckBoxLyrist.Name = "CheckBoxLyrist";
			this.CheckBoxLyrist.Size = new System.Drawing.Size(16, 20);
			this.CheckBoxLyrist.TabIndex = 41;
			this.CheckBoxLyrist.UseVisualStyleBackColor = true;
			this.CheckBoxLyrist.CheckedChanged += new System.EventHandler(this.CheckBoxLyrist_CheckedChanged);
			// 
			// ButtonEditLyrist
			// 
			this.ButtonEditLyrist.Location = new System.Drawing.Point(976, 96);
			this.ButtonEditLyrist.Name = "ButtonEditLyrist";
			this.ButtonEditLyrist.Size = new System.Drawing.Size(112, 20);
			this.ButtonEditLyrist.TabIndex = 40;
			this.ButtonEditLyrist.Text = "作詞者詳細編集";
			this.ButtonEditLyrist.UseVisualStyleBackColor = true;
			this.ButtonEditLyrist.Click += new System.EventHandler(this.ButtonEditLyrist_Click);
			// 
			// ButtonSearchLyrist
			// 
			this.ButtonSearchLyrist.Location = new System.Drawing.Point(656, 96);
			this.ButtonSearchLyrist.Name = "ButtonSearchLyrist";
			this.ButtonSearchLyrist.Size = new System.Drawing.Size(64, 20);
			this.ButtonSearchLyrist.TabIndex = 39;
			this.ButtonSearchLyrist.Text = "検索";
			this.ButtonSearchLyrist.UseVisualStyleBackColor = true;
			this.ButtonSearchLyrist.Click += new System.EventHandler(this.ButtonSearchLyrist_Click);
			// 
			// LabelLyrist
			// 
			this.LabelLyrist.Location = new System.Drawing.Point(728, 96);
			this.LabelLyrist.Name = "LabelLyrist";
			this.LabelLyrist.Size = new System.Drawing.Size(200, 20);
			this.LabelLyrist.TabIndex = 38;
			this.LabelLyrist.Text = "-";
			this.LabelLyrist.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(536, 96);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(96, 20);
			this.label11.TabIndex = 37;
			this.label11.Text = "作詞者あり (&L)：";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CheckBoxComposer
			// 
			this.CheckBoxComposer.Location = new System.Drawing.Point(632, 124);
			this.CheckBoxComposer.Name = "CheckBoxComposer";
			this.CheckBoxComposer.Size = new System.Drawing.Size(16, 20);
			this.CheckBoxComposer.TabIndex = 46;
			this.CheckBoxComposer.UseVisualStyleBackColor = true;
			this.CheckBoxComposer.CheckedChanged += new System.EventHandler(this.CheckBoxComposer_CheckedChanged);
			// 
			// ButtonEditComposer
			// 
			this.ButtonEditComposer.Location = new System.Drawing.Point(976, 124);
			this.ButtonEditComposer.Name = "ButtonEditComposer";
			this.ButtonEditComposer.Size = new System.Drawing.Size(112, 20);
			this.ButtonEditComposer.TabIndex = 45;
			this.ButtonEditComposer.Text = "作曲者詳細編集";
			this.ButtonEditComposer.UseVisualStyleBackColor = true;
			this.ButtonEditComposer.Click += new System.EventHandler(this.ButtonEditComposer_Click);
			// 
			// ButtonSearchComposer
			// 
			this.ButtonSearchComposer.Location = new System.Drawing.Point(656, 124);
			this.ButtonSearchComposer.Name = "ButtonSearchComposer";
			this.ButtonSearchComposer.Size = new System.Drawing.Size(64, 20);
			this.ButtonSearchComposer.TabIndex = 44;
			this.ButtonSearchComposer.Text = "検索";
			this.ButtonSearchComposer.UseVisualStyleBackColor = true;
			this.ButtonSearchComposer.Click += new System.EventHandler(this.ButtonSearchComposer_Click);
			// 
			// LabelComposer
			// 
			this.LabelComposer.Location = new System.Drawing.Point(728, 124);
			this.LabelComposer.Name = "LabelComposer";
			this.LabelComposer.Size = new System.Drawing.Size(200, 20);
			this.LabelComposer.TabIndex = 43;
			this.LabelComposer.Text = "-";
			this.LabelComposer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(536, 124);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(96, 20);
			this.label13.TabIndex = 42;
			this.label13.Text = "作曲者あり (&C)：";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CheckBoxArranger
			// 
			this.CheckBoxArranger.Location = new System.Drawing.Point(632, 152);
			this.CheckBoxArranger.Name = "CheckBoxArranger";
			this.CheckBoxArranger.Size = new System.Drawing.Size(16, 20);
			this.CheckBoxArranger.TabIndex = 51;
			this.CheckBoxArranger.UseVisualStyleBackColor = true;
			this.CheckBoxArranger.CheckedChanged += new System.EventHandler(this.CheckBoxArranger_CheckedChanged);
			// 
			// ButtonEditArranger
			// 
			this.ButtonEditArranger.Location = new System.Drawing.Point(976, 152);
			this.ButtonEditArranger.Name = "ButtonEditArranger";
			this.ButtonEditArranger.Size = new System.Drawing.Size(112, 20);
			this.ButtonEditArranger.TabIndex = 50;
			this.ButtonEditArranger.Text = "編曲者詳細編集";
			this.ButtonEditArranger.UseVisualStyleBackColor = true;
			this.ButtonEditArranger.Click += new System.EventHandler(this.ButtonEditArranger_Click);
			// 
			// ButtonSearchArranger
			// 
			this.ButtonSearchArranger.Location = new System.Drawing.Point(656, 152);
			this.ButtonSearchArranger.Name = "ButtonSearchArranger";
			this.ButtonSearchArranger.Size = new System.Drawing.Size(64, 20);
			this.ButtonSearchArranger.TabIndex = 49;
			this.ButtonSearchArranger.Text = "検索";
			this.ButtonSearchArranger.UseVisualStyleBackColor = true;
			this.ButtonSearchArranger.Click += new System.EventHandler(this.ButtonSearchArranger_Click);
			// 
			// LabelArranger
			// 
			this.LabelArranger.Location = new System.Drawing.Point(728, 152);
			this.LabelArranger.Name = "LabelArranger";
			this.LabelArranger.Size = new System.Drawing.Size(200, 20);
			this.LabelArranger.TabIndex = 48;
			this.LabelArranger.Text = "-";
			this.LabelArranger.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(536, 152);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(96, 20);
			this.label15.TabIndex = 47;
			this.label15.Text = "編曲者あり (&A)：";
			this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxReleaseYear
			// 
			this.TextBoxReleaseYear.Location = new System.Drawing.Point(632, 208);
			this.TextBoxReleaseYear.Name = "TextBoxReleaseYear";
			this.TextBoxReleaseYear.Size = new System.Drawing.Size(64, 19);
			this.TextBoxReleaseYear.TabIndex = 52;
			this.TextBoxReleaseYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(536, 208);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(96, 20);
			this.label17.TabIndex = 54;
			this.label17.Text = "リリース日 (&R)：";
			this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(696, 208);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(24, 20);
			this.label18.TabIndex = 55;
			this.label18.Text = "年";
			this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TextBoxReleaseMonth
			// 
			this.TextBoxReleaseMonth.Location = new System.Drawing.Point(720, 208);
			this.TextBoxReleaseMonth.Name = "TextBoxReleaseMonth";
			this.TextBoxReleaseMonth.Size = new System.Drawing.Size(32, 19);
			this.TextBoxReleaseMonth.TabIndex = 56;
			this.TextBoxReleaseMonth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(752, 208);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(24, 20);
			this.label19.TabIndex = 57;
			this.label19.Text = "月";
			this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TextBoxReleaseDay
			// 
			this.TextBoxReleaseDay.Location = new System.Drawing.Point(776, 208);
			this.TextBoxReleaseDay.Name = "TextBoxReleaseDay";
			this.TextBoxReleaseDay.Size = new System.Drawing.Size(32, 19);
			this.TextBoxReleaseDay.TabIndex = 58;
			this.TextBoxReleaseDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label20
			// 
			this.label20.Location = new System.Drawing.Point(808, 208);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(24, 20);
			this.label20.TabIndex = 59;
			this.label20.Text = "日";
			this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ButtonSelectCategory
			// 
			this.ButtonSelectCategory.Location = new System.Drawing.Point(136, 236);
			this.ButtonSelectCategory.Name = "ButtonSelectCategory";
			this.ButtonSelectCategory.Size = new System.Drawing.Size(64, 20);
			this.ButtonSelectCategory.TabIndex = 60;
			this.ButtonSelectCategory.Text = "選択";
			this.ButtonSelectCategory.UseVisualStyleBackColor = true;
			this.ButtonSelectCategory.Click += new System.EventHandler(this.ButtonSelectCategory_Click);
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(536, 236);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(96, 20);
			this.label16.TabIndex = 62;
			this.label16.Text = "検索ワード (&W)：";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TextBoxKeyword
			// 
			this.TextBoxKeyword.Location = new System.Drawing.Point(632, 236);
			this.TextBoxKeyword.Name = "TextBoxKeyword";
			this.TextBoxKeyword.Size = new System.Drawing.Size(456, 19);
			this.TextBoxKeyword.TabIndex = 61;
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(632, 264);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(456, 20);
			this.label21.TabIndex = 63;
			this.label21.Text = "キーワード、コメントなど。複数入力する際は、半角カンマ「 , 」で区切って下さい。";
			this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Location = new System.Drawing.Point(0, 292);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1307, 5);
			this.panel1.TabIndex = 64;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(992, 308);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(96, 28);
			this.ButtonCancel.TabIndex = 67;
			this.ButtonCancel.Text = "キャンセル";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(879, 308);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(96, 28);
			this.ButtonOK.TabIndex = 66;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// ContextMenuStripOpEds
			// 
			this.ContextMenuStripOpEds.Name = "ContextMenuStripOpEds";
			this.ContextMenuStripOpEds.Size = new System.Drawing.Size(61, 4);
			// 
			// ContextMenuStripCategories
			// 
			this.ContextMenuStripCategories.Name = "ContextMenuStripCategories";
			this.ContextMenuStripCategories.Size = new System.Drawing.Size(61, 4);
			// 
			// ButtonSameLyrist
			// 
			this.ButtonSameLyrist.Location = new System.Drawing.Point(928, 96);
			this.ButtonSameLyrist.Name = "ButtonSameLyrist";
			this.ButtonSameLyrist.Size = new System.Drawing.Size(40, 20);
			this.ButtonSameLyrist.TabIndex = 68;
			this.ButtonSameLyrist.Text = "同上";
			this.ButtonSameLyrist.UseVisualStyleBackColor = true;
			this.ButtonSameLyrist.Click += new System.EventHandler(this.ButtonSameLyrist_Click);
			// 
			// ButtonSameComposer
			// 
			this.ButtonSameComposer.Location = new System.Drawing.Point(928, 124);
			this.ButtonSameComposer.Name = "ButtonSameComposer";
			this.ButtonSameComposer.Size = new System.Drawing.Size(40, 20);
			this.ButtonSameComposer.TabIndex = 69;
			this.ButtonSameComposer.Text = "同上";
			this.ButtonSameComposer.UseVisualStyleBackColor = true;
			this.ButtonSameComposer.Click += new System.EventHandler(this.ButtonSameComposer_Click);
			// 
			// ButtonSameArranger
			// 
			this.ButtonSameArranger.Location = new System.Drawing.Point(928, 152);
			this.ButtonSameArranger.Name = "ButtonSameArranger";
			this.ButtonSameArranger.Size = new System.Drawing.Size(40, 20);
			this.ButtonSameArranger.TabIndex = 70;
			this.ButtonSameArranger.Text = "同上";
			this.ButtonSameArranger.UseVisualStyleBackColor = true;
			this.ButtonSameArranger.Click += new System.EventHandler(this.ButtonSameArranger_Click);
			// 
			// FormEditSong
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(1107, 350);
			this.Controls.Add(this.ButtonSameArranger);
			this.Controls.Add(this.ButtonSameComposer);
			this.Controls.Add(this.ButtonSameLyrist);
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
			this.Controls.Add(this.CheckBoxArranger);
			this.Controls.Add(this.ButtonEditArranger);
			this.Controls.Add(this.ButtonSearchArranger);
			this.Controls.Add(this.LabelArranger);
			this.Controls.Add(this.label15);
			this.Controls.Add(this.CheckBoxComposer);
			this.Controls.Add(this.ButtonEditComposer);
			this.Controls.Add(this.ButtonSearchComposer);
			this.Controls.Add(this.LabelComposer);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.CheckBoxLyrist);
			this.Controls.Add(this.ButtonEditLyrist);
			this.Controls.Add(this.ButtonSearchLyrist);
			this.Controls.Add(this.LabelLyrist);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.CheckBoxArtist);
			this.Controls.Add(this.ButtonEditArtist);
			this.Controls.Add(this.ButtonSearchArtist);
			this.Controls.Add(this.LabelArtist);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.ButtonSelectOpEd);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.TextBoxOpEd);
			this.Controls.Add(this.CheckBoxCategory);
			this.Controls.Add(this.CheckBoxTieUp);
			this.Controls.Add(this.LabelCategory);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.ButtonEditTieUp);
			this.Controls.Add(this.ButtonSearchTieUp);
			this.Controls.Add(this.LabelTieUp);
			this.Controls.Add(this.label3);
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
			this.Name = "FormEditSong";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormEditSong_FormClosed);
			this.Load += new System.EventHandler(this.FormEditSong_Load);
			this.Shown += new System.EventHandler(this.FormEditSong_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button ButtonSearchTieUp;
		private System.Windows.Forms.Button ButtonEditTieUp;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button ButtonSelectOpEd;
		private System.Windows.Forms.Button ButtonEditArtist;
		private System.Windows.Forms.Button ButtonSearchArtist;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Button ButtonEditLyrist;
		private System.Windows.Forms.Button ButtonSearchLyrist;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Button ButtonEditComposer;
		private System.Windows.Forms.Button ButtonSearchComposer;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Button ButtonEditArranger;
		private System.Windows.Forms.Button ButtonSearchArranger;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.Button ButtonSelectCategory;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.ContextMenuStrip ContextMenuStripOpEds;
		private System.Windows.Forms.ContextMenuStrip ContextMenuStripCategories;
		internal System.Windows.Forms.Label LabelIdInfo;
		internal System.Windows.Forms.ComboBox ComboBoxId;
		internal System.Windows.Forms.TextBox TextBoxName;
		internal System.Windows.Forms.TextBox TextBoxRuby;
		internal System.Windows.Forms.TextBox TextBoxKeyword;
		internal System.Windows.Forms.Label LabelTieUp;
		internal System.Windows.Forms.Label LabelCategory;
		internal System.Windows.Forms.CheckBox CheckBoxTieUp;
		internal System.Windows.Forms.CheckBox CheckBoxCategory;
		internal System.Windows.Forms.TextBox TextBoxOpEd;
		internal System.Windows.Forms.CheckBox CheckBoxArtist;
		internal System.Windows.Forms.Label LabelArtist;
		internal System.Windows.Forms.CheckBox CheckBoxLyrist;
		internal System.Windows.Forms.Label LabelLyrist;
		internal System.Windows.Forms.CheckBox CheckBoxComposer;
		internal System.Windows.Forms.Label LabelComposer;
		internal System.Windows.Forms.CheckBox CheckBoxArranger;
		internal System.Windows.Forms.Label LabelArranger;
		internal System.Windows.Forms.TextBox TextBoxReleaseYear;
		internal System.Windows.Forms.TextBox TextBoxReleaseMonth;
		internal System.Windows.Forms.TextBox TextBoxReleaseDay;
		private System.Windows.Forms.Button ButtonSameLyrist;
		private System.Windows.Forms.Button ButtonSameComposer;
		private System.Windows.Forms.Button ButtonSameArranger;
	}
}