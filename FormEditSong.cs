// ============================================================================
// 
// 楽曲詳細編集ウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// タイアップ等の ID 情報はラベルの Tag プロパティーに格納する
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YukaLister.Shared;

namespace YukaLister
{
	public partial class FormEditSong : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormEditSong(YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mYukaListerSettings = oYukaListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// アダプター
		public EditMasterAdapter Adapter { get; set; }

		// 初期表示する ID
		public String DefaultId { get; set; }

		// 登録された ID
		public String RegisteredId { get; set; }

		// ====================================================================
		// internal メンバー変数
		// ====================================================================

		// 環境設定
		internal YukaListerSettings mYukaListerSettings;

		// ログ
		internal LogWriter mLogWriter;

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// タイアップを検索したかどうか
		private Boolean mIsTieUpSearched = false;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ContextMenuStripCategoriesItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					List<TCategory> aCategories = YlCommon.SelectCategoriesByName(aConnection, aItem.Text);
					if (aCategories.Count > 0)
					{
						LabelCategory.Tag = aCategories[0].Id;
						LabelCategory.Text = aCategories[0].Name;
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "カテゴリー選択メニュークリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ContextMenuStripOpEdsItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				String aItemText = aItem.Text;
				Int32 aPos = aItemText.IndexOf("（");
				if (aPos >= 0)
				{
					TextBoxOpEd.Text = aItemText.Substring(0, aPos);
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "摘要選択メニュークリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 人物詳細編集
		// --------------------------------------------------------------------
		private void EditPeople(String oCaption, Label oLabel)
		{
			using (FormEditPeople aFormEditPeople = new FormEditPeople(oCaption, YlCommon.SplitIds((String)oLabel.Tag), mYukaListerSettings, mLogWriter))
			{

				if (aFormEditPeople.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				oLabel.Tag = null;
				oLabel.Text = null;
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					for (Int32 i = 0; i < aFormEditPeople.RegisteredIds.Count; i++)
					{
						TPerson aPerson = YlCommon.SelectPersonById(aConnection, aFormEditPeople.RegisteredIds[i]);
						if (i == 0)
						{
							oLabel.Tag = aPerson.Id;
							oLabel.Text = aPerson.Name;
						}
						else
						{
							oLabel.Tag += "," + aPerson.Id;
							oLabel.Text += "," + aPerson.Name;
						}
					}
				}
			}
		}


#if false
		// --------------------------------------------------------------------
		// 人物詳細編集
		// --------------------------------------------------------------------
		private void EditPerson(String oCaption, Label oLabel)
		{
			if (String.IsNullOrEmpty(oLabel.Text))
			{
				if (MessageBox.Show(oCaption + "が選択されていません。\n新規に" + oCaption + "情報を作成しますか？", "確認",
						MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
				{
					return;
				}
			}

			// 既存レコードを用意
			List<TPerson> aPeople;
			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				aPeople = YlCommon.SelectPeopleByName(aConnection, oLabel.Text);
			}

			// 新規作成用を追加
			TPerson aNewPerson = new TPerson
			{
				// TBase
				Id = null,
				Import = false,
				Invalid = false,
				UpdateTime = YlCommon.INVALID_MJD,
				Dirty = true,

				// TMaster
				Name = null,
				Ruby = null,
				Keyword = null,
			};
			aPeople.Insert(0, aNewPerson);

			using (FormEditMaster aFormEditMaster = new FormEditMaster(mYukaListerSettings, mLogWriter))
			{
				EditMasterAdapter aAdapter = new EditMasterAdapterTPerson(aFormEditMaster, aPeople, oCaption);
				aFormEditMaster.Adapter = aAdapter;
				aFormEditMaster.DefaultId = (String)oLabel.Tag;

				if (aFormEditMaster.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					TPerson aPerson = YlCommon.SelectPersonById(aConnection, aFormEditMaster.RegisteredId);
					if (aPerson != null)
					{
						oLabel.Tag = aPerson.Id;
						oLabel.Text = aPerson.Name;
					}
				}
			}
		}
#endif

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = "楽曲詳細情報の編集";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			Adapter.Init();

			// 摘要
			ContextMenuStripOpEds.Items.Add("OP（オープニング）", null, ContextMenuStripOpEdsItem_Click);
			ContextMenuStripOpEds.Items.Add("ED（エンディング）", null, ContextMenuStripOpEdsItem_Click);
			ContextMenuStripOpEds.Items.Add("IN（挿入歌）", null, ContextMenuStripOpEdsItem_Click);
			ContextMenuStripOpEds.Items.Add("IM（イメージソング）", null, ContextMenuStripOpEdsItem_Click);
			ContextMenuStripOpEds.Items.Add("CH（キャラクターソング）", null, ContextMenuStripOpEdsItem_Click);

			// カテゴリー
			YlCommon.SetContextMenuStripCategories(ContextMenuStripCategories, ContextMenuStripCategoriesItem_Click);

			Common.CascadeForm(this);
		}

		// --------------------------------------------------------------------
		// 人物を検索してラベルに設定
		// --------------------------------------------------------------------
		private void SearchPerson(String oCaption, Label oLabel)
		{
			// 人物が複数指定されている場合は先頭のみで検索
			String aKeyword = oLabel.Text;
			Int32 aPos = aKeyword.IndexOf(',');
			if (aPos > 0)
			{
				aKeyword = aKeyword.Substring(0, aPos);
			}

			using (FormSearchMusicInfo aFormSearchMusicInfo = new FormSearchMusicInfo(oCaption, MusicInfoDbTables.TPerson, aKeyword, mLogWriter))
			{
				if (aFormSearchMusicInfo.ShowDialog(this) == DialogResult.OK)
				{
					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						List<TPerson> aPeople = YlCommon.SelectPeopleByName(aConnection, aFormSearchMusicInfo.SelectedName);
						if (aPeople.Count > 0)
						{
							oLabel.Tag = aPeople[0].Id;
							oLabel.Text = aPeople[0].Name;
						}
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 編曲者の同上ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonSameArranger()
		{
			ButtonSameArranger.Enabled = CheckBoxArranger.Checked;
		}

		// --------------------------------------------------------------------
		// 作曲者の同上ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonSameComposer()
		{
			ButtonSameComposer.Enabled = CheckBoxComposer.Checked;
		}

		// --------------------------------------------------------------------
		// 作詞者の同上ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonSameLyrist()
		{
			ButtonSameLyrist.Enabled = CheckBoxLyrist.Checked;
		}

		// --------------------------------------------------------------------
		// カテゴリー関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateCategoryComponents()
		{
			CheckBoxCategory.Enabled = !(CheckBoxTieUp.Enabled && CheckBoxTieUp.Checked);

			ButtonSelectCategory.Enabled = CheckBoxCategory.Checked;
			if (!CheckBoxCategory.Checked)
			{
				LabelCategory.Tag = null;
				LabelCategory.Text = null;
			}
		}

		// --------------------------------------------------------------------
		// 人物関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdatePersonComponents(CheckBox oCheckBox, Button oButtonSearch, Button oButtonEdit, Label oLabel)
		{
			oButtonSearch.Enabled = oCheckBox.Checked;
			oButtonEdit.Enabled = oCheckBox.Checked;
			if (!oCheckBox.Checked)
			{
				oLabel.Tag = null;
				oLabel.Text = null;
			}
		}

		// --------------------------------------------------------------------
		// タイアップ関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateTieUpComponents()
		{
			CheckBoxTieUp.Enabled = !(CheckBoxCategory.Enabled && CheckBoxCategory.Checked);

			ButtonSearchTieUp.Enabled = CheckBoxTieUp.Checked;
			ButtonEditTieUp.Enabled = CheckBoxTieUp.Checked;
			if (!CheckBoxTieUp.Checked)
			{
				LabelTieUp.Tag = null;
				LabelTieUp.Text = null;
			}
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormEditSong_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲詳細編集ウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲詳細編集ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditSong_Shown(object sender, EventArgs e)
		{
			try
			{
				// デフォルト ID を選択
				Int32 aIndex;
				if (String.IsNullOrEmpty(DefaultId) || (aIndex = ComboBoxId.Items.IndexOf(DefaultId)) < 0)
				{
					ComboBoxId.SelectedIndex = 0;
				}
				else
				{
					ComboBoxId.SelectedIndex = aIndex;
				}

				// チェックボックスの状態を反映
				UpdateTieUpComponents();
				UpdateCategoryComponents();
				UpdatePersonComponents(CheckBoxArtist, ButtonSearchArtist, ButtonEditArtist, LabelArtist);
				UpdatePersonComponents(CheckBoxLyrist, ButtonSearchLyrist, ButtonEditLyrist, LabelLyrist);
				UpdatePersonComponents(CheckBoxComposer, ButtonSearchComposer, ButtonEditComposer, LabelComposer);
				UpdatePersonComponents(CheckBoxArranger, ButtonSearchArranger, ButtonEditArranger, LabelArranger);

				// 同上ボタンの状態を反映
				UpdateButtonSameLyrist();
				UpdateButtonSameComposer();
				UpdateButtonSameArranger();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲詳細編集ウィンドウ表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditSong_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲詳細編集ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲詳細編集ウィンドウクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ComboBoxId_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (ComboBoxId.SelectedIndex < 0)
				{
					return;
				}
				Adapter.RecordToCompos();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲 ID 選択変更時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void TextBoxName_Leave(object sender, EventArgs e)
		{
			try
			{
				Adapter.WarnDuplicateIfNeeded();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲名フォーカス解除時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxTieUp_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTieUpComponents();
				UpdateCategoryComponents();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップチェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxCategory_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateCategoryComponents();
				UpdateTieUpComponents();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "カテゴリーチェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxArtist_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdatePersonComponents(CheckBoxArtist, ButtonSearchArtist, ButtonEditArtist, LabelArtist);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "歌手チェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxLyrist_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdatePersonComponents(CheckBoxLyrist, ButtonSearchLyrist, ButtonEditLyrist, LabelLyrist);
				UpdateButtonSameLyrist();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "作詞者チェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxComposer_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdatePersonComponents(CheckBoxComposer, ButtonSearchComposer, ButtonEditComposer, LabelComposer);
				UpdateButtonSameComposer();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "作曲者チェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxArranger_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdatePersonComponents(CheckBoxArranger, ButtonSearchArranger, ButtonEditArranger, LabelArranger);
				UpdateButtonSameArranger();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "編曲者チェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchArtist_Click(object sender, EventArgs e)
		{
			try
			{
				SearchPerson("歌手", LabelArtist);
				UpdateButtonSameLyrist();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "歌手検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditArtist_Click(object sender, EventArgs e)
		{
			try
			{
				EditPeople("歌手", LabelArtist);
				UpdateButtonSameLyrist();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "歌手詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				Adapter.CheckAndSave();
				DialogResult = DialogResult.OK;
			}
			catch (OperationCanceledException)
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "OK ボタンクリック時処理を中止しました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchLyrist_Click(object sender, EventArgs e)
		{
			try
			{
				SearchPerson("作詞者", LabelLyrist);
				UpdateButtonSameComposer();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "作詞者検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchComposer_Click(object sender, EventArgs e)
		{
			try
			{
				SearchPerson("作曲者", LabelComposer);
				UpdateButtonSameArranger();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "作曲者検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchArranger_Click(object sender, EventArgs e)
		{
			try
			{
				SearchPerson("編曲者", LabelArranger);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "編曲者検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditLyrist_Click(object sender, EventArgs e)
		{
			try
			{
				EditPeople("作詞者", LabelLyrist);
				UpdateButtonSameComposer();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "作詞者詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditComposer_Click(object sender, EventArgs e)
		{
			try
			{
				EditPeople("作曲者", LabelComposer);
				UpdateButtonSameArranger();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "作曲者詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditArranger_Click(object sender, EventArgs e)
		{
			try
			{
				EditPeople("編曲者", LabelArranger);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "編曲者詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchTieUp_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormSearchMusicInfo aFormSearchMusicInfo = new FormSearchMusicInfo("タイアップ", MusicInfoDbTables.TTieUp, LabelTieUp.Text, mLogWriter))
				{
					if (aFormSearchMusicInfo.ShowDialog(this) == DialogResult.OK)
					{
						using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
						{
							List<TTieUp> aTieUps = YlCommon.SelectTieUpsByName(aConnection, aFormSearchMusicInfo.SelectedName);
							if (aTieUps.Count > 0)
							{
								LabelTieUp.Tag = aTieUps[0].Id;
								LabelTieUp.Text = aTieUps[0].Name;
							}
						}
					}
				}
				mIsTieUpSearched = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditTieUp_Click(object sender, EventArgs e)
		{
			try
			{
				if (String.IsNullOrEmpty((String)LabelTieUp.Tag))
				{
					if (!mIsTieUpSearched)
					{
						throw new Exception("タイアップが選択されていないため新規タイアップ情報作成となりますが、その前に一度、目的のタイアップが未登録かどうか検索して下さい。");
					}

					if (MessageBox.Show("タイアップが選択されていません。\n新規にタイアップ情報を作成しますか？\n"
							+ "（目的のタイアップが未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
							MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
					{
						return;
					}
				}

				// 既存レコードを用意
				List<TTieUp> aTieUps = new List<TTieUp>();
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					TTieUp aExistTieUp = YlCommon.SelectTieUpById(aConnection, (String)LabelTieUp.Tag);
					if (aExistTieUp != null)
					{
						aTieUps = YlCommon.SelectTieUpsByName(aConnection, aExistTieUp.Name);
					}
				}

				// 新規作成用を追加
				TTieUp aNewTieUp = new TTieUp
				{
					// TBase
					Id = null,
					Import = false,
					Invalid = false,
					UpdateTime = YlCommon.INVALID_MJD,
					Dirty = true,

					// TMaster
					Name = null,
					Ruby = null,
					Keyword = null,

					// TTieUp
					CategoryId = null,
					MakerId = null,
					AgeLimit = 0,
					ReleaseDate = YlCommon.INVALID_MJD,
				};
				aTieUps.Insert(0, aNewTieUp);

				using (FormEditTieUp aFormEditTieUp = new FormEditTieUp(mYukaListerSettings, mLogWriter))
				{
					EditMasterAdapter aAdapter = new EditMasterAdapterTTieUp(aFormEditTieUp, aTieUps, mYukaListerSettings);
					aFormEditTieUp.Adapter = aAdapter;
					aFormEditTieUp.DefaultId = (String)LabelTieUp.Tag;

					if (aFormEditTieUp.ShowDialog(this) != DialogResult.OK)
					{
						return;
					}

					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						TTieUp aTieUp = YlCommon.SelectTieUpById(aConnection, aFormEditTieUp.RegisteredId);
						if (aTieUp != null)
						{
							LabelTieUp.Tag = aTieUp.Id;
							LabelTieUp.Text = YlCommon.TieUpNameAvoidingSameName(aConnection, aTieUp);
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSelectCategory_Click(object sender, EventArgs e)
		{
			try
			{
				ContextMenuStripCategories.Show(ButtonSelectCategory, 0, ButtonSelectCategory.Height);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "カテゴリー選択ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSelectOpEd_Click(object sender, EventArgs e)
		{
			try
			{
				ContextMenuStripOpEds.Show(ButtonSelectOpEd, 0, ButtonSelectOpEd.Height);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "摘要選択ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSameLyrist_Click(object sender, EventArgs e)
		{
			try
			{
				LabelLyrist.Tag = LabelArtist.Tag;
				LabelLyrist.Text = LabelArtist.Text;
				UpdateButtonSameComposer();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "作詞者同上ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSameComposer_Click(object sender, EventArgs e)
		{
			try
			{
				LabelComposer.Tag = LabelLyrist.Tag;
				LabelComposer.Text = LabelLyrist.Text;
				UpdateButtonSameArranger();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "作曲者同上ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSameArranger_Click(object sender, EventArgs e)
		{
			try
			{
				LabelArranger.Tag = LabelComposer.Tag;
				LabelArranger.Text = LabelComposer.Text;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "編曲者同上ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}
