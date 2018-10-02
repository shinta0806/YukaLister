// ============================================================================
// 
// ファイル名・フォルダー固定値から取得した楽曲情報等の編集を行うウィンドウ
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
	public partial class FormEditMusicInfo : Form
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public FormEditMusicInfo(String oFileName, Dictionary<String, String> oDicByFile, YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
		{
			InitializeComponent();

			// 変数初期化
			mFileName = oFileName;
			mDicByFile = oDicByFile;
			mYukaListerSettings = oYukaListerSettings;
			mLogWriter = oLogWriter;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// ファイル名（パス無し）
		private String mFileName;

		// ファイル名・フォルダー固定値から取得した情報
		private Dictionary<String, String> mDicByFile;

		// 楽曲を検索したかどうか
		private Boolean mIsSongSearched = false;

		// タイアップを検索したかどうか
		private Boolean mIsTieUpSearched = false;

		// 環境設定
		private YukaListerSettings mYukaListerSettings;

		// ログ
		private LogWriter mLogWriter;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 適用可能な楽曲名の別名を検索してコンポーネントに反映
		// --------------------------------------------------------------------
		private void ApplySongAlias(SQLiteConnection oConnection)
		{
			if (String.IsNullOrEmpty(mDicByFile[YlCommon.RULE_VAR_TITLE]))
			{
				return;
			}

			List<TSongAlias> aSongAliases = YlCommon.SelectSongAliasesByAlias(oConnection, mDicByFile[YlCommon.RULE_VAR_TITLE]);
			if (aSongAliases.Count > 0)
			{
				TSong aSong = YlCommon.SelectSongById(oConnection, aSongAliases[0].OriginalId);
				if (aSong != null)
				{
					CheckBoxUseSongAlias.Checked = true;
					LabelSongOrigin.Text = aSong.Name;
					return;
				}
			}

			if (YlCommon.SelectSongsByName(oConnection, mDicByFile[YlCommon.RULE_VAR_TITLE]).Count == 0)
			{
				CheckBoxUseSongAlias.Checked = true;
				LabelSongOrigin.Text = null;
			}
		}

		// --------------------------------------------------------------------
		// 適用可能なタイアップ名の別名を検索してコンポーネントに反映
		// --------------------------------------------------------------------
		private void ApplyTieUpAlias(SQLiteConnection oConnection)
		{
			if (String.IsNullOrEmpty(mDicByFile[YlCommon.RULE_VAR_PROGRAM]))
			{
				return;
			}

			List<TTieUpAlias> aTieUpAliases = YlCommon.SelectTieUpAliasesByAlias(oConnection, mDicByFile[YlCommon.RULE_VAR_PROGRAM]);
			if (aTieUpAliases.Count > 0)
			{
				TTieUp aTieUp = YlCommon.SelectTieUpById(oConnection, aTieUpAliases[0].OriginalId);
				if (aTieUp != null)
				{
					CheckBoxUseTieUpAlias.Checked = true;
					LabelTieUpOrigin.Text = aTieUp.Name;
					return;
				}
			}

			if (YlCommon.SelectTieUpsByName(oConnection, mDicByFile[YlCommon.RULE_VAR_PROGRAM]).Count == 0)
			{
				CheckBoxUseTieUpAlias.Checked = true;
				LabelTieUpOrigin.Text = null;
			}
		}

		// --------------------------------------------------------------------
		// 入力値の確認（別名に関するもののみ）
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckInput(out String oSongOriginalId, out String oTieUpOriginalId)
		{
			oSongOriginalId = null;
			oTieUpOriginalId = null;

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				// 楽曲別名
				if (CheckBoxUseSongAlias.Checked)
				{
					String aSongOrigin = LabelSongOrigin.Text;
					if (String.IsNullOrEmpty(aSongOrigin))
					{
						throw new Exception("楽曲名の正式名称を検索して指定して下さい。");
					}
					if (aSongOrigin == mDicByFile[YlCommon.RULE_VAR_TITLE])
					{
						throw new Exception("ファイル名・フォルダー固定値から取得した楽曲名と正式名称が同じです。\n"
								+ "楽曲名を揃えるのが不要の場合は、「楽曲名を揃える」のチェックを外して下さい。");
					}
					List<TSong> aSongs = YlCommon.SelectSongsByName(aConnection, aSongOrigin);
					if (aSongs.Count == 0)
					{
						throw new Exception("楽曲名の正式名称が正しく検索されていません。");
					}
					oSongOriginalId = aSongs[0].Id;
				}

				// タイアップ別名
				if (CheckBoxUseTieUpAlias.Checked)
				{
					String aTieUpOrigin = LabelTieUpOrigin.Text;
					if (String.IsNullOrEmpty(aTieUpOrigin))
					{
						throw new Exception("タイアップ名の正式名称を検索して指定して下さい。");
					}
					if (aTieUpOrigin == mDicByFile[YlCommon.RULE_VAR_PROGRAM])
					{
						throw new Exception("ファイル名・フォルダー固定値から取得したタイアップ名と正式名称が同じです。\n"
								+ "タイアップ名を揃えるのが不要の場合は、「タイアップ名を揃える」のチェックを外して下さい。");
					}
					List<TTieUp> aTieUps = YlCommon.SelectTieUpsByName(aConnection, aTieUpOrigin);
					if (aTieUps.Count == 0)
					{
						throw new Exception("タイアップ名の正式名称が正しく検索されていません。");
					}
					oTieUpOriginalId = aTieUps[0].Id;
				}
			}
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Text = "名称の編集";
#if DEBUG
			Text = "［デバッグ］" + Text;
#endif

			Common.CascadeForm(this);
		}

		// --------------------------------------------------------------------
		// 別名を保存
		// --------------------------------------------------------------------
		private void Save(String oSongOriginalId, String oTieUpOriginalId)
		{
			YlCommon.BackupMusicInfoDb();

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				using (DataContext aContext = new DataContext(aConnection))
				{
					// 楽曲別名
					Table<TSongAlias> aTableSongAlias = aContext.GetTable<TSongAlias>();
					if (CheckBoxUseSongAlias.Checked)
					{
						List<TSongAlias> aSongAliases = YlCommon.SelectSongAliasesByAlias(aContext, mDicByFile[YlCommon.RULE_VAR_TITLE], true);
						TSongAlias aNewSongAlias = new TSongAlias
						{
							// TBase
							Id = null,
							Import = false,
							Invalid = false,
							UpdateTime = YlCommon.INVALID_MJD,
							Dirty = true,

							// TAlias
							Alias = mDicByFile[YlCommon.RULE_VAR_TITLE],
							OriginalId = oSongOriginalId,
						};

						if (aSongAliases.Count == 0)
						{
							// 新規登録
							YlCommon.InputIdPrefixIfNeededWithInvoke(this, mYukaListerSettings);
							aNewSongAlias.Id = mYukaListerSettings.PrepareLastId(aConnection, MusicInfoDbTables.TSongAlias);
							aTableSongAlias.InsertOnSubmit(aNewSongAlias);
							mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲別名テーブル新規登録：" + aNewSongAlias.Id + " / " + aNewSongAlias.Alias);
						}
						else if (YlCommon.IsUpdated(aSongAliases[0], aNewSongAlias))
						{
							// 更新（既存のレコードが無効化されている場合は有効化も行う）
							aNewSongAlias.Id = aSongAliases[0].Id;
							aNewSongAlias.UpdateTime = aSongAliases[0].UpdateTime;
							Common.ShallowCopy(aNewSongAlias, aSongAliases[0]);
							mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲別名テーブル更新：" + aNewSongAlias.Id + " / " + aNewSongAlias.Alias);
						}
					}
					else
					{
						List<TSongAlias> aSongAliases = YlCommon.SelectSongAliasesByAlias(aContext, mDicByFile[YlCommon.RULE_VAR_TITLE], false);
						if (aSongAliases.Count > 0)
						{
							// 無効化
							aSongAliases[0].Invalid = true;
							mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲別名テーブル無効化：" + aSongAliases[0].Id + " / " + aSongAliases[0].Alias);
						}
					}

					// タイアップ別名
					Table<TTieUpAlias> aTableTieUpAlias = aContext.GetTable<TTieUpAlias>();
					if (CheckBoxUseTieUpAlias.Checked)
					{
						List<TTieUpAlias> aTieUpAliases = YlCommon.SelectTieUpAliasesByAlias(aContext, mDicByFile[YlCommon.RULE_VAR_PROGRAM], true);
						TTieUpAlias aNewTieUpAlias = new TTieUpAlias
						{
							// TBase
							Id = null,
							Import = false,
							Invalid = false,
							UpdateTime = YlCommon.INVALID_MJD,
							Dirty = true,

							// TAlias
							Alias = mDicByFile[YlCommon.RULE_VAR_PROGRAM],
							OriginalId = oTieUpOriginalId,
						};

						if (aTieUpAliases.Count == 0)
						{
							// 新規登録
							YlCommon.InputIdPrefixIfNeededWithInvoke(this, mYukaListerSettings);
							aNewTieUpAlias.Id = mYukaListerSettings.PrepareLastId(aConnection, MusicInfoDbTables.TTieUpAlias);
							aTableTieUpAlias.InsertOnSubmit(aNewTieUpAlias);
							mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップ別名テーブル新規登録：" + aNewTieUpAlias.Id + " / " + aNewTieUpAlias.Alias);
						}
						else if (YlCommon.IsUpdated(aTieUpAliases[0], aNewTieUpAlias))
						{
							// 更新（既存のレコードが無効化されている場合は有効化も行う）
							aNewTieUpAlias.Id = aTieUpAliases[0].Id;
							aNewTieUpAlias.UpdateTime = aTieUpAliases[0].UpdateTime;
							Common.ShallowCopy(aNewTieUpAlias, aTieUpAliases[0]);
							mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップ別名テーブル更新：" + aNewTieUpAlias.Id + " / " + aNewTieUpAlias.Alias);
						}
					}
					else
					{
						List<TTieUpAlias> aTieUpAliases = YlCommon.SelectTieUpAliasesByAlias(aContext, mDicByFile[YlCommon.RULE_VAR_PROGRAM], false);
						if (aTieUpAliases.Count > 0)
						{
							// 無効化
							aTieUpAliases[0].Invalid = true;
							mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップ別名テーブル無効化：" + aTieUpAliases[0].Id + " / " + aTieUpAliases[0].Alias);
						}
					}

					aContext.SubmitChanges();
				}
			}
		}

		// --------------------------------------------------------------------
		// データベース登録済み状況表示ラベルの設定
		// --------------------------------------------------------------------
		private void SetDbRegisteredLabel(Label oLabel, String oName, Int32 oNumDbRegistered)
		{
			if (String.IsNullOrEmpty(oName))
			{
				oLabel.Text = null;
				return;
			}

			if (oNumDbRegistered > 0)
			{
				oLabel.Text = "（データベース登録済）";
				oLabel.ForeColor = Color.Black;
			}
			else
			{
				oLabel.Text = "（データベース未登録）";
				oLabel.ForeColor = Color.Red;
			}
		}

		// --------------------------------------------------------------------
		// 楽曲別名コンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateSongAliasComponents()
		{
			ButtonSearchSongOrigin.Enabled = CheckBoxUseSongAlias.Checked;
			if (!CheckBoxUseSongAlias.Checked)
			{
				LabelSongOrigin.Text = null;
			}
		}

		// --------------------------------------------------------------------
		// タイアップ別名コンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateTieUpAliasComponents()
		{
			ButtonSearchTieUpOrigin.Enabled = CheckBoxUseTieUpAlias.Checked;
			if (!CheckBoxUseTieUpAlias.Checked)
			{
				LabelTieUpOrigin.Text = null;
			}
		}


		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void FormEditMusicInfo_Load(object sender, EventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報等編集ウィンドウを開きます。");
				Init();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲情報等編集ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditMusicInfo_Shown(object sender, EventArgs e)
		{
			try
			{
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					// パス
					LabelPath.Text = mFileName;

					// 「ファイル名・フォルダー固定値から取得した情報」欄
					LabelSongName.Text = mDicByFile[YlCommon.RULE_VAR_TITLE];
					SetDbRegisteredLabel(LabelSongNameRegistered, mDicByFile[YlCommon.RULE_VAR_TITLE], YlCommon.SelectSongsByName(aConnection, mDicByFile[YlCommon.RULE_VAR_TITLE]).Count);
					LabelTieUpName.Text = mDicByFile[YlCommon.RULE_VAR_PROGRAM];
					SetDbRegisteredLabel(LabelTieUpNameRegistered, mDicByFile[YlCommon.RULE_VAR_PROGRAM], YlCommon.SelectTieUpsByName(aConnection, mDicByFile[YlCommon.RULE_VAR_PROGRAM]).Count);

					// 別名解決
					ApplySongAlias(aConnection);
					ApplyTieUpAlias(aConnection);

					// エイリアスコンポーネント
					UpdateSongAliasComponents();
					UpdateTieUpAliasComponents();

					// タイアップ名が指定されていない場合は関連コンポーネントを無効にする
					if (String.IsNullOrEmpty(mDicByFile[YlCommon.RULE_VAR_PROGRAM]))
					{
						CheckBoxUseTieUpAlias.Enabled = false;
						ButtonEditTieUp.Enabled = false;
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲情報等編集ウィンドウ表示時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void FormEditMusicInfo_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲情報等編集ウィンドウを閉じました。");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲情報等編集ウィンドウクローズ時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxUseSongAlias_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (CheckBoxUseSongAlias.Checked)
				{
					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						if (YlCommon.SelectSongsByName(aConnection, mDicByFile[YlCommon.RULE_VAR_TITLE]).Count > 0)
						{
							CheckBoxUseSongAlias.Checked = false;
							throw new Exception("ファイル名・フォルダー固定値から取得した楽曲名はデータベースに登録済みのため、楽曲名を揃えるのは不要です。");
						}
					}
				}

				UpdateSongAliasComponents();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲別名チェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void CheckBoxUseTieUpAlias_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (CheckBoxUseTieUpAlias.Checked)
				{
					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						if (YlCommon.SelectTieUpsByName(aConnection, mDicByFile[YlCommon.RULE_VAR_PROGRAM]).Count > 0)
						{
							CheckBoxUseTieUpAlias.Checked = false;
							throw new Exception("ファイル名・フォルダー固定値から取得したタイアップ名はデータベースに登録済みのため、タイアップ名を揃えるのは不要です。");
						}
					}
				}

				UpdateTieUpAliasComponents();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ別名チェックボックスチェック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchSongOrigin_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormSearchMusicInfo aFormSearchMusicInfo = new FormSearchMusicInfo("楽曲名の正式名称", MusicInfoDbTables.TSong, LabelSongOrigin.Text, mLogWriter))
				{
					if (aFormSearchMusicInfo.ShowDialog(this) == DialogResult.OK)
					{
						LabelSongOrigin.Text = aFormSearchMusicInfo.SelectedName;
					}
				}
				mIsSongSearched = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲検索ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchTieUpOrigin_Click(object sender, EventArgs e)
		{
			try
			{
				using (FormSearchMusicInfo aFormSearchMusicInfo = new FormSearchMusicInfo("タイアップ名の正式名称", MusicInfoDbTables.TTieUp, LabelTieUpOrigin.Text, mLogWriter))
				{
					if (aFormSearchMusicInfo.ShowDialog(this) == DialogResult.OK)
					{
						LabelTieUpOrigin.Text = aFormSearchMusicInfo.SelectedName;
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

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			try
			{
				String aSongOriginalId;
				String aTieUpOriginalId;
				CheckInput(out aSongOriginalId, out aTieUpOriginalId);
				Save(aSongOriginalId, aTieUpOriginalId);
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

		private void ButtonEditSong_Click(object sender, EventArgs e)
		{
			try
			{
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					// ファイル名から取得した楽曲名が未登録でかつ未検索は検索を促す
					if (YlCommon.SelectSongsByName(aConnection, mDicByFile[YlCommon.RULE_VAR_TITLE]).Count == 0 && String.IsNullOrEmpty(LabelSongOrigin.Text))
					{
						if (!mIsSongSearched)
						{
							throw new Exception("楽曲の正式名称が選択されていないため新規楽曲情報作成となりますが、その前に一度、目的の楽曲が未登録かどうか検索して下さい。");
						}

						if (MessageBox.Show("楽曲の正式名称が選択されていません。\n新規に楽曲情報を作成しますか？\n"
								+ "（目的の楽曲が未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
								MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
						{
							return;
						}
					}
				}

				// 対象楽曲名の選択
				String aSongName;
				if (!String.IsNullOrEmpty(LabelSongOrigin.Text))
				{
					aSongName = LabelSongOrigin.Text;
				}
				else
				{
					aSongName = mDicByFile[YlCommon.RULE_VAR_TITLE];
				}

				// タイアップ名の選択（null もありえる）
				String aTieUpName;
				if (!String.IsNullOrEmpty(LabelTieUpOrigin.Text))
				{
					aTieUpName = LabelTieUpOrigin.Text;
				}
				else
				{
					aTieUpName = mDicByFile[YlCommon.RULE_VAR_PROGRAM];
				}

				// 情報準備
				List<TSong> aSongs;
				//List<TPerson> aArtists;
				List<TTieUp> aTieUps;
				List<TCategory> aCategories;
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					aSongs = YlCommon.SelectSongsByName(aConnection, aSongName);
					//aArtists = YlCommon.SelectPeopleByName(aConnection, mDicByFile[YlCommon.RULE_VAR_ARTIST]);
					aTieUps = YlCommon.SelectTieUpsByName(aConnection, aTieUpName);
					aCategories = YlCommon.SelectCategoriesByName(aConnection, mDicByFile[YlCommon.RULE_VAR_CATEGORY]);
				}

				// 新規作成用の追加
				TSong aNewSong = new TSong
				{
					// TBase
					Id = null,
					Import = false,
					Invalid = false,
					UpdateTime = YlCommon.INVALID_MJD,
					Dirty = true,

					// TMaster
					Name = aSongName,
					Ruby = mDicByFile[YlCommon.RULE_VAR_TITLE_RUBY],
					Keyword = null,

					// TSong
					ReleaseDate = YlCommon.INVALID_MJD,
					TieUpId = aTieUps.Count > 0 ? aTieUps[0].Id : null,
					CategoryId = aTieUps.Count == 0 && aCategories.Count > 0 ? aCategories[0].Id : null,
					OpEd = mDicByFile[YlCommon.RULE_VAR_OP_ED],
				};
				aSongs.Insert(0, aNewSong);

				using (FormEditSong aFormEditSong = new FormEditSong(mYukaListerSettings, mLogWriter))
				{
					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						EditMasterAdapter aAdapter = new EditMasterAdapterTSong(aFormEditSong, aSongs, mYukaListerSettings);
						aFormEditSong.Adapter = aAdapter;

						// デフォルト ID の指定
						if (aSongs.Count == 1)
						{
							// 新規作成のみの場合は指定しない
						}
						else if (aSongs.Count == 2 && String.IsNullOrEmpty(aTieUpName))
						{
							// 既存楽曲が 1 つのみの場合で、タイアップが指定されていない場合は、既存楽曲のタイアップに関わらずデフォルトに指定する
							aFormEditSong.DefaultId = aSongs[1].Id;
						}
						else
						{
							// 既存楽曲が 1 つ以上の場合は、タイアップ名が一致するものがあれば優先し、そうでなければ新規をデフォルトにする
							for (Int32 i = 1; i < aSongs.Count; i++)
							{
								TTieUp aTieUpOfSong = YlCommon.SelectTieUpById(aConnection, aSongs[i].TieUpId);
								if (aTieUpOfSong == null && String.IsNullOrEmpty(aTieUpName) || aTieUpOfSong != null && aTieUpOfSong.Name == aTieUpName)
								{
									aFormEditSong.DefaultId = aSongs[i].Id;
									break;
								}
							}
						}

						if (aFormEditSong.ShowDialog(this) != DialogResult.OK)
						{
							return;
						}

						TSong aSong = YlCommon.SelectSongById(aConnection, aFormEditSong.RegisteredId);
						if (aSong != null)
						{
							if (aSong.Name == mDicByFile[YlCommon.RULE_VAR_TITLE])
							{
								CheckBoxUseSongAlias.Checked = false;
							}
							else
							{
								CheckBoxUseSongAlias.Checked = true;
								LabelSongOrigin.Text = aSong.Name;
							}
						}

						SetDbRegisteredLabel(LabelSongNameRegistered, mDicByFile[YlCommon.RULE_VAR_TITLE], YlCommon.SelectSongsByName(aConnection, mDicByFile[YlCommon.RULE_VAR_TITLE]).Count);
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonEditTieUp_Click(object sender, EventArgs e)
		{
			try
			{
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					// ファイル名から取得したタイアップ名が未登録でかつ未検索は検索を促す
					if (YlCommon.SelectTieUpsByName(aConnection, mDicByFile[YlCommon.RULE_VAR_PROGRAM]).Count == 0 && String.IsNullOrEmpty(LabelTieUpOrigin.Text))
					{
						if (!mIsTieUpSearched)
						{
							throw new Exception("タイアップの正式名称が選択されていないため新規タイアップ情報作成となりますが、その前に一度、目的のタイアップが未登録かどうか検索して下さい。");
						}

						if (MessageBox.Show("タイアップの正式名称が選択されていません。\n新規にタイアップ情報を作成しますか？\n"
								+ "（目的のタイアップが未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
								MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
						{
							return;
						}
					}
				}

				// 対象タイアップ名の選択
				String aTieUpName;
				if (!String.IsNullOrEmpty(LabelTieUpOrigin.Text))
				{
					aTieUpName = LabelTieUpOrigin.Text;
				}
				else
				{
					aTieUpName = mDicByFile[YlCommon.RULE_VAR_PROGRAM];
				}

				// 情報準備
				List<TTieUp> aTieUps;
				List<TCategory> aCategories;
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					aTieUps = YlCommon.SelectTieUpsByName(aConnection, aTieUpName);
					aCategories = YlCommon.SelectCategoriesByName(aConnection, mDicByFile[YlCommon.RULE_VAR_CATEGORY]);
				}

				// 新規作成用の追加
				TTieUp aNewTieUp = new TTieUp
				{
					// TBase
					Id = null,
					Import = false,
					Invalid = false,
					UpdateTime = YlCommon.INVALID_MJD,
					Dirty = true,

					// TMaster
					Name = aTieUpName,
					Ruby = null,
					Keyword = null,

					// TTieUp
					CategoryId = aCategories.Count > 0 ? aCategories[0].Id : null,
					MakerId = null,
					AgeLimit = Common.StringToInt32(mDicByFile[YlCommon.RULE_VAR_AGE_LIMIT]),
					ReleaseDate = YlCommon.INVALID_MJD,
				};
				aTieUps.Insert(0, aNewTieUp);

				using (FormEditTieUp aFormEditTieUp = new FormEditTieUp(mYukaListerSettings, mLogWriter))
				{
					EditMasterAdapter aAdapter = new EditMasterAdapterTTieUp(aFormEditTieUp, aTieUps, mYukaListerSettings);
					aFormEditTieUp.Adapter = aAdapter;
					if (aTieUps.Count > 1)
					{
						aFormEditTieUp.DefaultId = aTieUps[1].Id;
					}

					if (aFormEditTieUp.ShowDialog(this) != DialogResult.OK)
					{
						return;
					}

					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						TTieUp aTieUp = YlCommon.SelectTieUpById(aConnection, aFormEditTieUp.RegisteredId);
						if (aTieUp != null)
						{
							if (String.IsNullOrEmpty(mDicByFile[YlCommon.RULE_VAR_PROGRAM]) || aTieUp.Name == mDicByFile[YlCommon.RULE_VAR_PROGRAM])
							{
								CheckBoxUseTieUpAlias.Checked = false;
							}
							else
							{
								CheckBoxUseTieUpAlias.Checked = true;
								LabelTieUpOrigin.Text = aTieUp.Name;
							}
						}

						SetDbRegisteredLabel(LabelTieUpNameRegistered, mDicByFile[YlCommon.RULE_VAR_PROGRAM], YlCommon.SelectTieUpsByName(aConnection, mDicByFile[YlCommon.RULE_VAR_PROGRAM]).Count);
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void LinkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				YlCommon.ShowHelp("NamaewoSoroeru");
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "詳細情報リンククリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}
