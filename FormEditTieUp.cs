// ============================================================================
// 
// タイアップ詳細編集ウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// カテゴリー等の ID 情報はラベルの Tag プロパティーに格納する
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
	public partial class FormEditTieUp : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormEditTieUp(YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
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

		// 制作会社を検索したかどうか
		private Boolean mIsMakerSearched = false;

		// タイアップグループを検索したかどうか
		private Boolean mIsTieUpGroupSearched = false;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ContextMenuStripAgeLimitsItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				ToolStripItem aItem = (ToolStripItem)oSender;
				String aItemText = aItem.Text;
				Int32 aAgeLimit = Common.StringToInt32(aItemText);
				if (aAgeLimit == 0)
				{
					TextBoxAgeLimit.Text = null;
				}
				else
				{
					TextBoxAgeLimit.Text = aAgeLimit.ToString();
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "年齢制限選択メニュークリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

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
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = "タイアップ詳細情報の編集";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			Adapter.Init();

			// カテゴリー
			YlCommon.SetContextMenuStripCategories(ContextMenuStripCategories, ContextMenuStripCategoriesItem_Click);

			// 年齢制限
			ContextMenuStripAgeLimits.Items.Add("全年齢対象（CERO A 相当）", null, ContextMenuStripAgeLimitsItem_Click);
			ContextMenuStripAgeLimits.Items.Add(YlCommon.AGE_LIMIT_CERO_B.ToString() + " 才以上対象（CERO B 相当）", null, ContextMenuStripAgeLimitsItem_Click);
			ContextMenuStripAgeLimits.Items.Add(YlCommon.AGE_LIMIT_CERO_C.ToString() + " 才以上対象（CERO C 相当）", null, ContextMenuStripAgeLimitsItem_Click);
			ContextMenuStripAgeLimits.Items.Add(YlCommon.AGE_LIMIT_CERO_D.ToString() + " 才以上対象（CERO D 相当）", null, ContextMenuStripAgeLimitsItem_Click);
			ContextMenuStripAgeLimits.Items.Add(YlCommon.AGE_LIMIT_CERO_Z.ToString() + " 才以上対象（CERO Z 相当）", null, ContextMenuStripAgeLimitsItem_Click);

			Common.CascadeForm(this);
		}

		// --------------------------------------------------------------------
		// カテゴリー関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateCategoryComponents()
		{
			ButtonSelectCategory.Enabled = CheckBoxCategory.Checked;
			if (!CheckBoxCategory.Checked)
			{
				LabelCategory.Tag = null;
				LabelCategory.Text = null;
			}
		}

		// --------------------------------------------------------------------
		// 制作会社関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateMakerComponents()
		{
			ButtonSearchMaker.Enabled = CheckBoxMaker.Checked;
			ButtonEditMaker.Enabled = CheckBoxMaker.Checked;
			if (!CheckBoxMaker.Checked)
			{
				LabelMaker.Tag = null;
				LabelMaker.Text = null;
			}
		}

		// --------------------------------------------------------------------
		// タイアップグループ関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateTieUpGroupComponents()
		{
			ButtonSearchTieUpGroup.Enabled = CheckBoxTieUpGroup.Checked;
			ButtonEditTieUpGroup.Enabled = CheckBoxTieUpGroup.Checked;
			if (!CheckBoxTieUpGroup.Checked)
			{
				LabelTieUpGroup.Tag = null;
				LabelTieUpGroup.Text = null;
			}
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormEditTieUp_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップ詳細編集ウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ詳細編集ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditTieUp_Shown(object sender, EventArgs e)
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
				UpdateCategoryComponents();
				UpdateMakerComponents();
				UpdateTieUpGroupComponents();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲詳細編集ウィンドウ表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditTieUp_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップ詳細編集ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ詳細編集ウィンドウクローズ時エラー：\n" + oExcep.Message);
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
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ ID 選択変更時エラー：\n" + oExcep.Message);
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

		private void TextBoxName_Leave(object sender, EventArgs e)
		{
			try
			{
				Adapter.WarnDuplicateIfNeeded();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ名フォーカス解除時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxCategory_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateCategoryComponents();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "カテゴリーチェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxMaker_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateMakerComponents();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "制作会社チェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxTieUpGroup_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateTieUpGroupComponents();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップグループチェックボックスチェック時エラー：\n" + oExcep.Message);
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

		private void ButtonSelectAgeLimit_Click(object sender, EventArgs e)
		{
			try
			{
				ContextMenuStripAgeLimits.Show(ButtonSelectAgeLimit, 0, ButtonSelectAgeLimit.Height);
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "年齢制限選択ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchMaker_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormSearchMusicInfo aFormSearchMusicInfo = new FormSearchMusicInfo("制作会社", MusicInfoDbTables.TMaker, LabelMaker.Text, mLogWriter))
				{
					if (aFormSearchMusicInfo.ShowDialog(this) == DialogResult.OK)
					{
						using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
						{
							List<TMaker> aMakers = YlCommon.SelectMakersByName(aConnection, aFormSearchMusicInfo.SelectedName);
							if (aMakers.Count > 0)
							{
								LabelMaker.Tag = aMakers[0].Id;
								LabelMaker.Text = aMakers[0].Name;
							}
						}
					}
				}
				mIsMakerSearched = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "制作会社検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditMaker_Click(object sender, EventArgs e)
		{
			try
			{
				if (String.IsNullOrEmpty(LabelMaker.Text))
				{
					if (!mIsMakerSearched)
					{
						throw new Exception("制作会社が選択されていないため新規制作会社情報作成となりますが、その前に一度、目的の制作会社が未登録かどうか検索して下さい。");
					}

					if (MessageBox.Show("制作会社が選択されていません。\n新規に制作会社情報を作成しますか？\n"
							+ "（目的の制作会社が未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
							MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
					{
						return;
					}
				}

				// 既存レコードを用意
				List<TMaker> aMakers;
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					aMakers = YlCommon.SelectMakersByName(aConnection, LabelMaker.Text);
				}

				// 新規作成用を追加
				TMaker aNewMaker = new TMaker
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
				aMakers.Insert(0, aNewMaker);

				using (FormEditMaster aFormEditMaster = new FormEditMaster(mYukaListerSettings, mLogWriter))
				{
					EditMasterAdapter aAdapter = new EditMasterAdapterTMaker(aFormEditMaster, aMakers, mYukaListerSettings);
					aFormEditMaster.Adapter = aAdapter;
					aFormEditMaster.DefaultId = (String)LabelMaker.Tag;

					if (aFormEditMaster.ShowDialog(this) != DialogResult.OK)
					{
						return;
					}

					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						TMaker aMaker = YlCommon.SelectMakerById(aConnection, aFormEditMaster.RegisteredId);
						if (aMaker != null)
						{
							LabelMaker.Tag = aMaker.Id;
							LabelMaker.Text = aMaker.Name;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "制作会社詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchTieUpGroup_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormSearchMusicInfo aFormSearchMusicInfo = new FormSearchMusicInfo("シリーズ", MusicInfoDbTables.TTieUpGroup, LabelTieUpGroup.Text, mLogWriter))
				{
					if (aFormSearchMusicInfo.ShowDialog(this) == DialogResult.OK)
					{
						using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
						{
							List<TTieUpGroup> aTieUpGroups = YlCommon.SelectTieUpGroupsByName(aConnection, aFormSearchMusicInfo.SelectedName);
							if (aTieUpGroups.Count > 0)
							{
								LabelTieUpGroup.Tag = aTieUpGroups[0].Id;
								LabelTieUpGroup.Text = aTieUpGroups[0].Name;
							}
						}
					}
				}
				mIsTieUpGroupSearched = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "シリーズ検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditTieUpGroup_Click(object sender, EventArgs e)
		{
			try
			{
				if (String.IsNullOrEmpty(LabelTieUpGroup.Text))
				{
					if (!mIsTieUpGroupSearched)
					{
						throw new Exception("シリーズが選択されていないため新規シリーズ情報作成となりますが、その前に一度、目的のシリーズが未登録かどうか検索して下さい。");
					}

					if (MessageBox.Show("シリーズが選択されていません。\n新規にシリーズ情報を作成しますか？\n"
							+ "（目的のシリーズが未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
							MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
					{
						return;
					}
				}

				// 既存レコードを用意
				List<TTieUpGroup> aTieUpGroups;
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					aTieUpGroups = YlCommon.SelectTieUpGroupsByName(aConnection, LabelTieUpGroup.Text);
				}

				// 新規作成用を追加
				TTieUpGroup aNewTieUpGroup = new TTieUpGroup
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
				aTieUpGroups.Insert(0, aNewTieUpGroup);

				using (FormEditMaster aFormEditMaster = new FormEditMaster(mYukaListerSettings, mLogWriter))
				{
					EditMasterAdapter aAdapter = new EditMasterAdapterTTieUpGroup(aFormEditMaster, aTieUpGroups, mYukaListerSettings);
					aFormEditMaster.Adapter = aAdapter;
					aFormEditMaster.DefaultId = (String)LabelTieUpGroup.Tag;

					if (aFormEditMaster.ShowDialog(this) != DialogResult.OK)
					{
						return;
					}

					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						TTieUpGroup aTieUpGroup = YlCommon.SelectTieUpGroupById(aConnection, aFormEditMaster.RegisteredId);
						if (aTieUpGroup != null)
						{
							LabelTieUpGroup.Tag = aTieUpGroup.Id;
							LabelTieUpGroup.Text = aTieUpGroup.Name;
						}
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "シリーズ詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}
