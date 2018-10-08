﻿// ============================================================================
// 
// 複数人物編集ウィンドウ
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
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
	public partial class FormEditPeople : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormEditPeople(String oCaption, List<String> oInitialPeople, YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mCaption = oCaption;
			mInitialPeople = oInitialPeople;
			mYukaListerSettings = oYukaListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 登録された ID
		public List<String> RegisteredIds { get; set; }

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// 人物区分
		private String mCaption;

		// 初期値（ID のみ）
		private List<String> mInitialPeople;

		// 現在値（ID のみ）
		private List<String> mCurrentPeople;

		// 検索したかどうか
		private Boolean mIsPersonSearched = false;

		// 環境設定
		private YukaListerSettings mYukaListerSettings;

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = mCaption + "の編集";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			// 説明
			LabelDescription.Text = "「検索して追加」ボタンで" + mCaption + "追加して下さい。複数名の指定も可能です。";

			// 現在値にコピー
			mCurrentPeople = new List<String>();
			mCurrentPeople.AddRange(mInitialPeople);

			Common.CascadeForm(this);
		}

		// --------------------------------------------------------------------
		// 人物編集フォームを表示
		// ＜返値＞ 決定された ID
		// --------------------------------------------------------------------
		private String ShowFormEditMaster(List<TPerson> oPeople)
		{
			// 新規作成用レコードを追加
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
			oPeople.Insert(0, aNewPerson);

			using (FormEditMaster aFormEditMaster = new FormEditMaster(mYukaListerSettings, mLogWriter))
			{
				EditMasterAdapter aAdapter = new EditMasterAdapterTPerson(aFormEditMaster, oPeople, mYukaListerSettings, mCaption);
				aFormEditMaster.Adapter = aAdapter;
				if (ListBoxPeople.SelectedIndex >= 0)
				{
					aFormEditMaster.DefaultId = mCurrentPeople[ListBoxPeople.SelectedIndex];
				}

				if (aFormEditMaster.ShowDialog(this) != DialogResult.OK)
				{
					return null;
				}

				return aFormEditMaster.RegisteredId;
			}
		}

		// --------------------------------------------------------------------
		// ボタンの状態を更新
		// --------------------------------------------------------------------
		private void UpdateButtons()
		{
			Int32 aSelectedIndex = ListBoxPeople.SelectedIndex;

			ButtonRemove.Enabled = aSelectedIndex >= 0;
			ButtonEdit.Enabled = aSelectedIndex >= 0;

			ButtonUp.Enabled = aSelectedIndex > 0;
			ButtonDown.Enabled = 0 <= aSelectedIndex && aSelectedIndex < ListBoxPeople.Items.Count - 1;
		}

		// --------------------------------------------------------------------
		// リストボックスに mCurrentPeople の内容を反映
		// --------------------------------------------------------------------
		private void UpdateListBoxPeople()
		{
			String aSelectedName = null;
			if (ListBoxPeople.SelectedIndex >= 0)
			{
				aSelectedName = (String)ListBoxPeople.SelectedItem;
			}

			ListBoxPeople.Items.Clear();

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				foreach (String aId in mCurrentPeople)
				{
					TPerson aPerson = YlCommon.SelectPersonById(aConnection, aId);
					if (aPerson == null)
					{
						// 行数を合わせるためダミーアイテムを追加
						ListBoxPeople.Items.Add("（ID " + aId + " が見つかりません");
					}
					else
					{
						List<TPerson> aSameNamePeople = YlCommon.SelectPeopleByName(aConnection, aPerson.Name);
						if (aSameNamePeople.Count == 1)
						{
							// 通常は人物名のみを表示
							ListBoxPeople.Items.Add(aPerson.Name);
						}
						else
						{
							// 同名がいるため見分けられるようにキーワードを併記
							ListBoxPeople.Items.Add(aPerson.Name + "　（" + (String.IsNullOrEmpty(aPerson.Keyword) ? "検索ワード無し" : aPerson.Keyword) + "）");
						}
					}
				}
			}

			if (!String.IsNullOrEmpty(aSelectedName))
			{
				ListBoxPeople.SelectedIndex = ListBoxPeople.Items.IndexOf(aSelectedName);
			}
			else
			{
				if (ListBoxPeople.Items.Count > 0)
				{
					ListBoxPeople.SelectedIndex = 0;
				}
			}
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormEditPeople_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "複数人物編集ウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "複数人物編集ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditPeople_Shown(object sender, EventArgs e)
		{
			try
			{
				UpdateListBoxPeople();
				UpdateButtons();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "複数人物編集ウィンドウ表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditPeople_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "複数人物編集ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "複数人物編集ウィンドウクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonAdd_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormSearchMusicInfo aFormSearchMusicInfo = new FormSearchMusicInfo(mCaption, MusicInfoDbTables.TPerson, null, mLogWriter))
				{
					if (aFormSearchMusicInfo.ShowDialog(this) == DialogResult.OK)
					{
						using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
						{
							List<TPerson> aPeople = YlCommon.SelectPeopleByName(aConnection, aFormSearchMusicInfo.SelectedName);
							if (aPeople.Count == 0)
							{
								throw new Exception(aFormSearchMusicInfo.SelectedName + "がデータベースの登録されていません。");
							}
							if (mCurrentPeople.IndexOf(aPeople[0].Id) >= 0)
							{
								throw new Exception(aFormSearchMusicInfo.SelectedName + "は既に追加されています。");
							}
							mCurrentPeople.Add(aPeople[0].Id);
							UpdateListBoxPeople();
							ListBoxPeople.SelectedIndex = ListBoxPeople.Items.Count - 1;
						}
					}
				}
				mIsPersonSearched = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "追加ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxPeople_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				UpdateButtons();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リスト選択時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonRemove_Click(object sender, EventArgs e)
		{
			try
			{
				mCurrentPeople.RemoveAt(ListBoxPeople.SelectedIndex);
				UpdateListBoxPeople();
				UpdateButtons();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "削除ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonUp_Click(object sender, EventArgs e)
		{
			try
			{
				Int32 aSelectedIndex = ListBoxPeople.SelectedIndex;
				String aItem = mCurrentPeople[aSelectedIndex];
				mCurrentPeople.RemoveAt(aSelectedIndex);
				mCurrentPeople.Insert(aSelectedIndex - 1, aItem);
				UpdateListBoxPeople();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "上へボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonDown_Click(object sender, EventArgs e)
		{
			try
			{
				Int32 aSelectedIndex = ListBoxPeople.SelectedIndex;
				String aItem = mCurrentPeople[aSelectedIndex];
				mCurrentPeople.RemoveAt(aSelectedIndex);
				mCurrentPeople.Insert(aSelectedIndex + 1, aItem);
				UpdateListBoxPeople();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "下へボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEdit_Click(object sender, EventArgs e)
		{
			try
			{
				// 既存レコード（同名の人物すべて）を用意
				List<TPerson> aPeople;
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					TPerson aPerson = YlCommon.SelectPersonById(aConnection, mCurrentPeople[ListBoxPeople.SelectedIndex]);
					if (aPerson == null)
					{
						return;
					}
					aPeople = YlCommon.SelectPeopleByName(aConnection, aPerson.Name);
				}

				String aRegisteredId = ShowFormEditMaster(aPeople);
				if (String.IsNullOrEmpty(aRegisteredId))
				{
					return;
				}

				Int32 aSelectedIndex = ListBoxPeople.SelectedIndex;
				mCurrentPeople[aSelectedIndex] = aRegisteredId;
				UpdateListBoxPeople();
				ListBoxPeople.SelectedIndex = aSelectedIndex;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonNew_Click(object sender, EventArgs e)
		{
			try
			{
				if (!mIsPersonSearched)
				{
					throw new Exception("新規人物作成の前に一度、目的の人物が未登録かどうか検索して下さい。");
				}

				if (MessageBox.Show("目的の人物が未登録の場合（検索してもヒットしない場合）に限り、新規人物作成を行って下さい。\n"
						+ "新規人物作成を行いますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
				{
					return;
				}

				List<TPerson> aPeople = new List<TPerson>();
				String aRegisteredId = ShowFormEditMaster(aPeople);
				if (String.IsNullOrEmpty(aRegisteredId))
				{
					return;
				}

				mCurrentPeople.Add(aRegisteredId);
				UpdateListBoxPeople();
				ListBoxPeople.SelectedIndex = ListBoxPeople.Items.Count - 1;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "新規作成ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				RegisteredIds = mCurrentPeople;
				DialogResult = DialogResult.OK;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ListBoxPeople_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				ButtonEdit.PerformClick();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "リストダブルクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void LinkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				YlCommon.ShowHelp("KasyuSakushisyaSakkyokusyaHenkyokusyanoSentaku");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}