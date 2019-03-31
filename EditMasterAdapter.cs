// ============================================================================
// 
// TMaster 詳細編集ウィンドウ用アダプタークラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// TMaster テーブルからイメージ上派生している各種テーブルを統一的に扱うためのクラス
// ----------------------------------------------------------------------------

using MaterialDesignThemes.Wpf;
using Shinta;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YukaLister.Shared;

namespace YukaLister
{
	// ====================================================================
	// TMaster 詳細編集ウィンドウ用アダプタークラス
	// ====================================================================

	public abstract class EditMasterAdapter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public EditMasterAdapter()
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 編集対象の名称
		public String Caption { get; set; }

		// ヘルプリンク先
		public String HelpAnchor { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力値を確認し、問題なければデータベースを更新
		// --------------------------------------------------------------------
		public abstract void CheckAndSave();

		// --------------------------------------------------------------------
		// コンポーネント初期化
		// --------------------------------------------------------------------
		public virtual void Init()
		{
			mTextBoxKeyword.ToolTip = "キーワード、コメントなど。複数入力する際は、半角カンマ「 , 」で区切って下さい。";
			HintAssist.SetHint(mTextBoxKeyword, mTextBoxKeyword.ToolTip);
			ToolTipService.SetShowDuration(mTextBoxKeyword, YlCommon.TOOL_TIP_LONG_INTERVAL);
		}

		// --------------------------------------------------------------------
		// 表示用新規 ID
		// --------------------------------------------------------------------
		public String NewIdForDisplay()
		{
			return "（新規" + Caption + "）";
		}

		// --------------------------------------------------------------------
		// レコードの内容をコンポーネントに表示
		// --------------------------------------------------------------------
		public abstract void RecordToCompos();

		// --------------------------------------------------------------------
		// 新規作成時、既存レコードに同名がある場合は警告
		// --------------------------------------------------------------------
		public abstract void WarnDuplicateIfNeeded();

		// ====================================================================
		// protected メンバー変数
		// ====================================================================

		// コンポーネント
		protected ComboBox mComboBoxId;
		protected Label mLabelIdInfo;
		protected TextBox mTextBoxRuby;
		protected TextBox mTextBoxName;
		protected TextBox mTextBoxKeyword;

		// 環境設定
		protected YukaListerSettings mYukaListerSettings;

		// ログ
		protected LogWriter mLogWriter;

		// 重複警告を表示した名前
		protected String mDupWarningedName;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// TMaster 分について入力値を確認
		// ＜例外＞ Exception, OperationCanceledException
		// --------------------------------------------------------------------
		protected void CheckInput(String oInitialName, Int32 oNumDup)
		{
			String aId = (String)mComboBoxId.SelectedItem;
			String aNormalizedName = YlCommon.NormalizeDbString(mTextBoxName.Text);
			String aNormalizedRuby = YlCommon.NormalizeDbRuby(mTextBoxRuby.Text);
			String aNormalizedKeyword = YlCommon.NormalizeDbString(mTextBoxKeyword.Text);

			// 名前が入力されているか
			if (String.IsNullOrEmpty(aNormalizedName))
			{
				throw new Exception(Caption + "名を入力して下さい。");
			}

			// 名前が初期値と変わっている場合
			if (!String.IsNullOrEmpty(oInitialName) && oInitialName != aNormalizedName)
			{
				if (oNumDup > 0)
				{
					if (String.IsNullOrEmpty(aNormalizedKeyword))
					{
						// 変更後の名前が既に登録されている場合、キーワードがなければ名前の変更は禁止
						mLogWriter.ShowLogMessage(TraceEventType.Error, Caption + "名を「" + oInitialName + "」から「" + aNormalizedName
								+ "」に変更しようとしていますが、変更後の名前は既に登録されています。\n検索ワードを入力して識別できるようにしてください。");
						throw new OperationCanceledException();
					}
					else
					{
						// ユーザーに確認
						if (MessageBox.Show(Caption + "名を「" + oInitialName + "」から「" + aNormalizedName
								+ "」に変更しようとしていますが、変更後の名前は既に登録されています。\nこのまま登録してよろしいですか？", "確認",
								MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
						{
							mTextBoxName.Text = oInitialName;
							throw new OperationCanceledException();
						}
					}
				}
				else
				{
					// ユーザーに確認
					if (MessageBox.Show(Caption + "名を「" + oInitialName + "」から「" + aNormalizedName
							+ "」に変更しようとしています。\nこのまま登録してよろしいですか？", "確認",
							MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
					{
						mTextBoxName.Text = oInitialName;
						throw new OperationCanceledException();
					}
				}
			}

			// 新規作成時、同名が既に登録されている場合
			if (aId == NewIdForDisplay() && oNumDup > 0)
			{
				if (String.IsNullOrEmpty(aNormalizedKeyword))
				{
					// キーワードがなければ同名の登録は禁止
					mLogWriter.ShowLogMessage(TraceEventType.Error, Caption + "「" + aNormalizedName + "」は既に登録されています。\n"
							+ "検索ワードを入力して識別できるようにしてください。");
					throw new OperationCanceledException();
				}
				else
				{
					// ユーザーに確認
					if (MessageBox.Show("登録しようとしている" + Caption + "「" + aNormalizedName + "」は既に登録されています。\n"
							+ "このまま登録すると同じ名前で複数の登録となりますが、よろしいですか？", "確認",
							MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
					{
						throw new OperationCanceledException();
					}
				}
			}

			// フリガナとして使えない文字がある場合は警告
			YlCommon.WarnRubyDeletedIfNeeded(mTextBoxRuby.Text, aNormalizedRuby);

			// データベースをバックアップ
			YlCommon.BackupMusicInfoDb();
		}

		// --------------------------------------------------------------------
		// LabelIdInfo の更新
		// --------------------------------------------------------------------
		protected void UpdateLabelInfo()
		{
			if (mComboBoxId.Items.Count <= 1)
			{
				mLabelIdInfo.Content = null;
			}
			else if (mComboBoxId.Items.Count == 2)
			{
				if ((String)mComboBoxId.SelectedItem == NewIdForDisplay())
				{
					mLabelIdInfo.Content = "（同名の登録が既にあります）";
				}
				else
				{
					mLabelIdInfo.Content = null;
				}
			}
			else
			{
				mLabelIdInfo.Content = "（同名の登録が複数あります）";
			}
			mLabelIdInfo.Foreground = new SolidColorBrush(Colors.Red);
		}

		// --------------------------------------------------------------------
		// 新規作成時、既存レコードに同名がある場合は警告
		// --------------------------------------------------------------------
		protected void WarnDuplicateIfNeededCore()
		{
			if ((String)mComboBoxId.SelectedItem != NewIdForDisplay())
			{
				return;
			}

			String aNormalizedName = YlCommon.NormalizeDbString(mTextBoxName.Text);
			if (aNormalizedName == mDupWarningedName)
			{
				return;
			}

			mLogWriter.ShowLogMessage(TraceEventType.Warning, "新規登録しようとしている" + Caption + "名「" + aNormalizedName
					+ "」は既にデータベースに登録されています。\n" + Caption + "名は同じでも" + Caption + "自体が異なる場合は、このまま作業を続行して下さい。\n"
					+ "それ以外の場合は、重複登録を避けるために、" + Caption + " ID コンボボックスから既存の" + Caption + "情報を選択して下さい。");
			mDupWarningedName = aNormalizedName;
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

	}
	// public abstract class EditMasterAdapter ___END___

	// ====================================================================
	// TMaker 詳細編集用アダプタークラス
	// ====================================================================

	public class EditMasterAdapterTMaker : EditMasterAdapter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public EditMasterAdapterTMaker(EditMasterWindow oOwner, List<TMaker> oInitialMakers, YukaListerSettings oYukaListerSettings)
		{
			// 変数初期化
			mOwner = oOwner;
			mInitialMakers = oInitialMakers;
			mYukaListerSettings = oYukaListerSettings;
			Caption = "制作会社";
			HelpAnchor = "SeisakugaisyajouhounoShinkitourokutoHenkou";
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力値を確認し、問題なければデータベースを更新
		// --------------------------------------------------------------------
		public override void CheckAndSave()
		{
			TMaker aInitialMaker = mInitialMakers[mOwner.ComboBoxId.SelectedIndex];
			String aId = (String)mOwner.ComboBoxId.SelectedItem;
			String aNormalizedRuby = YlCommon.NormalizeDbRuby(mOwner.TextBoxRuby.Text);
			String aNormalizedName = YlCommon.NormalizeDbString(mOwner.TextBoxName.Text);
			String aNormalizedKeyword = YlCommon.NormalizeDbString(mOwner.TextBoxKeyword.Text);

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				List<TMaker> aMakers = YlCommon.SelectMakersByName(aConnection, aNormalizedName);

				CheckInput(aInitialMaker.Name, aMakers.Count);

				// 新規作成時、制作会社名が同じで、かつ、検索キーワードが同じものがあると拒否
				if (aId == NewIdForDisplay() && aMakers.Count > 0)
				{
					foreach (TMaker aMaker in aMakers)
					{
						if (String.IsNullOrEmpty(aNormalizedKeyword) || aNormalizedKeyword == aMaker.Keyword)
						{
							throw new Exception("登録しようとしている" + Caption + "「" + aNormalizedName + "」は既に登録されています。\n"
									+ Caption + " ID を切り替えて登録済みの" + Caption + "を選択してください。\n"
									+ "同名の別" + Caption + "を登録しようとしている場合は、検索ワードを入力して見分けが付くようにして下さい。");
						}
					}
				}

				TMaker aNewMaker = new TMaker
				{
					// TBase
					Id = aId,
					Import = false,
					Invalid = false,
					UpdateTime = YlCommon.INVALID_MJD,
					Dirty = true,

					// TMaster
					Name = aNormalizedName,
					Ruby = aNormalizedRuby,
					Keyword = aNormalizedKeyword,
				};

				using (DataContext aContext = new DataContext(aConnection))
				{
					if (aNewMaker.Id == NewIdForDisplay())
					{
						// 新規登録
						YlCommon.InputIdPrefixIfNeededWithInvoke(mOwner, mYukaListerSettings);
						aNewMaker.Id = mOwner.mYukaListerSettings.PrepareLastId(aConnection, MusicInfoDbTables.TMaker);
						Table<TMaker> aTableMaker = aContext.GetTable<TMaker>();
						aTableMaker.InsertOnSubmit(aNewMaker);
						mOwner.mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "制作会社テーブル新規登録：" + aNewMaker.Id + " / " + aNewMaker.Name);
					}
					else
					{
						TMaker aExistMaker = YlCommon.SelectMakerById(aContext, aNewMaker.Id, true);
						if (YlCommon.IsUpdated(aExistMaker, aNewMaker))
						{
							// 更新（既存のレコードが無効化されている場合は有効化も行う）
							aNewMaker.UpdateTime = aExistMaker.UpdateTime;
							Common.ShallowCopy(aNewMaker, aExistMaker);
							mOwner.mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "制作会社テーブル更新：" + aNewMaker.Id + " / " + aNewMaker.Name);
						}
					}

					aContext.SubmitChanges();
				}

				mOwner.RegisteredId = aNewMaker.Id;
			}
		}

		// --------------------------------------------------------------------
		// コンポーネント初期化
		// --------------------------------------------------------------------
		public override void Init()
		{
			// コンポーネント
			mComboBoxId = mOwner.ComboBoxId;
			mLabelIdInfo = mOwner.LabelIdInfo;
			mTextBoxRuby = mOwner.TextBoxRuby;
			mTextBoxName = mOwner.TextBoxName;
			mTextBoxKeyword = mOwner.TextBoxKeyword;

			// ログ
			mLogWriter = mOwner.mLogWriter;

			// ラベル
			mOwner.LabelId.Content = "制作会社 " + mOwner.LabelId.Content;
			mOwner.LabelName.Content = "制作会社" + mOwner.LabelName.Content;

			// ヒント
			mTextBoxName.ToolTip = "株式会社・有限会社などの法人格は入力しないで下さい。";
			HintAssist.SetHint(mTextBoxName, mTextBoxName.ToolTip);
			ToolTipService.SetShowDuration(mTextBoxName, YlCommon.TOOL_TIP_LONG_INTERVAL);

			// ID コンボボックス設定
			foreach (TMaker aMaker in mInitialMakers)
			{
				if (String.IsNullOrEmpty(aMaker.Id))
				{
					// 新規タイアップの ID を可読的に設定
					aMaker.Id = NewIdForDisplay();
				}
				mOwner.ComboBoxId.Items.Add(aMaker.Id);
			}
			UpdateLabelInfo();

			base.Init();
		}

		// --------------------------------------------------------------------
		// レコードの内容をコンポーネントに表示
		// --------------------------------------------------------------------
		public override void RecordToCompos()
		{
			TMaker aMaker = mInitialMakers[mOwner.ComboBoxId.SelectedIndex];
			Debug.Assert(aMaker.Id == (String)mOwner.ComboBoxId.SelectedItem, "RecordToCompos() diff inner/display maker info");

			UpdateLabelInfo();

			// 名称関係
			mOwner.TextBoxRuby.Text = aMaker.Ruby;
			mOwner.TextBoxName.Text = aMaker.Name;

			// その他
			mOwner.TextBoxKeyword.Text = aMaker.Keyword;
		}

		// --------------------------------------------------------------------
		// 新規作成時、既存レコードに同名がある場合は警告
		// --------------------------------------------------------------------
		public override void WarnDuplicateIfNeeded()
		{
			String aNormalizedName = YlCommon.NormalizeDbString(mOwner.TextBoxName.Text);

			List<TMaker> aMakers;
			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				aMakers = YlCommon.SelectMakersByName(aConnection, aNormalizedName);
				if (aMakers.Count == 0)
				{
					return;
				}
			}

			// 同名のレコードが編集対象になっていない場合はコンボボックスに追加する
			foreach (TMaker aMaker in aMakers)
			{
				if (mOwner.ComboBoxId.Items.IndexOf(aMaker.Id) >= 0)
				{
					continue;
				}

				mInitialMakers.Add(aMaker);
				mOwner.ComboBoxId.Items.Add(aMaker.Id);
			}

			WarnDuplicateIfNeededCore();
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// オーナー
		private EditMasterWindow mOwner;

		// 各レコードの初期値
		private List<TMaker> mInitialMakers;
	}
	// public class EditMasterAdapterTMaker ___END___

	// ====================================================================
	// TSong 詳細編集用アダプタークラス
	// ====================================================================

	public class EditMasterAdapterTSong : EditMasterAdapter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public EditMasterAdapterTSong(EditSongWindow oOwner, List<TSong> oInitialSongs, YukaListerSettings oYukaListerSettings)
		{
			// 変数初期化
			mOwner = oOwner;
			mInitialSongs = oInitialSongs;
			mYukaListerSettings = oYukaListerSettings;
			Caption = "楽曲";
			HelpAnchor = "GakkyokujouhounoShinkitourokutoHenkou";
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力値を確認し、問題なければデータベースを更新
		// --------------------------------------------------------------------
		public override void CheckAndSave()
		{
			TSong aInitialSong = mInitialSongs[mOwner.ComboBoxId.SelectedIndex];

			// TBase
			String aId = (String)mOwner.ComboBoxId.SelectedItem;

			// TMaster
			String aNormalizedRuby = YlCommon.NormalizeDbRuby(mOwner.TextBoxRuby.Text);
			String aNormalizedName = YlCommon.NormalizeDbString(mOwner.TextBoxName.Text);
			String aNormalizedKeyword = YlCommon.NormalizeDbString(mOwner.TextBoxKeyword.Text);

			// TSong
			String aTieUpId = (String)mOwner.LabelTieUp.Tag;
			String aOpEd = YlCommon.NormalizeDbString(mOwner.TextBoxOpEd.Text);
			String aCategoryId = (String)mOwner.LabelCategory.Tag;
			List<String> aArtistIds = YlCommon.SplitIds((String)mOwner.LabelArtist.Tag);
			List<String> aLyristIds = YlCommon.SplitIds((String)mOwner.LabelLyrist.Tag);
			List<String> aComposerIds = YlCommon.SplitIds((String)mOwner.LabelComposer.Tag);
			List<String> aArrangerIds = YlCommon.SplitIds((String)mOwner.LabelArranger.Tag);
			Double aReleaseDate = YlCommon.TextBoxToMjd("リリース日", mOwner.TextBoxReleaseYear, mOwner.TextBoxReleaseMonth, mOwner.TextBoxReleaseDay);

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				List<TSong> aSongs = YlCommon.SelectSongsByName(aConnection, aNormalizedName);

				CheckInput(aInitialSong.Name, aSongs.Count);

				// チェックされているのに指定されていない項目を確認
				if ((Boolean)mOwner.CheckBoxTieUp.IsChecked && String.IsNullOrEmpty(aTieUpId))
				{
					throw new Exception("タイアップが「あり」になっていますが指定されていません。");
				}
				if ((Boolean)mOwner.CheckBoxCategory.IsChecked && String.IsNullOrEmpty(aCategoryId))
				{
					throw new Exception("カテゴリーが「あり」になっていますが指定されていません。");
				}
				if ((Boolean)mOwner.CheckBoxArtist.IsChecked && aArtistIds.Count == 0)
				{
					throw new Exception("歌手が「あり」になっていますが指定されていません。");
				}
				if ((Boolean)mOwner.CheckBoxLyrist.IsChecked && aLyristIds.Count == 0)
				{
					throw new Exception("作詞者が「あり」になっていますが指定されていません。");
				}
				if ((Boolean)mOwner.CheckBoxComposer.IsChecked && aComposerIds.Count == 0)
				{
					throw new Exception("作曲者が「あり」になっていますが指定されていません。");
				}
				if ((Boolean)mOwner.CheckBoxArranger.IsChecked && aArrangerIds.Count == 0)
				{
					throw new Exception("編曲者が「あり」になっていますが指定されていません。");
				}

				// 新規作成時、楽曲名が同じで、かつ、タイアップ名・検索キーワードが同じものがあると拒否
				if (aId == NewIdForDisplay() && aSongs.Count > 0)
				{
					String aNewTieUpName = null;
					TTieUp aNewTieUp = YlCommon.SelectTieUpById(aConnection, aTieUpId);
					if (aNewTieUp != null)
					{
						aNewTieUpName = aNewTieUp.Name;
					}
					foreach (TSong aSong in aSongs)
					{
						String aTieUpName = null;
						TTieUp aTieUp = YlCommon.SelectTieUpById(aConnection, aSong.TieUpId);
						if (aTieUp != null)
						{
							aTieUpName = aTieUp.Name;
						}

						if (aNewTieUpName == aTieUpName && (String.IsNullOrEmpty(aNormalizedKeyword) || aNormalizedKeyword == aSong.Keyword))
						{
							throw new Exception("登録しようとしている" + Caption + "「" + aNormalizedName + "」は既に登録されています。\n"
									+ Caption + " ID を切り替えて登録済みの" + Caption + "を選択してください。\n"
									+ "同名の別" + Caption + "を登録しようとしている場合は、検索ワードを入力して見分けが付くようにして下さい。");
						}
					}
				}

				TSong aNewSong = new TSong
				{
					// TBase
					Id = aId,
					Import = false,
					Invalid = false,
					UpdateTime = YlCommon.INVALID_MJD,
					Dirty = true,

					// TMaster
					Name = aNormalizedName,
					Ruby = aNormalizedRuby,
					Keyword = aNormalizedKeyword,

					// TSong
					ReleaseDate = aReleaseDate,
					TieUpId = aTieUpId,
					CategoryId = aCategoryId,
					OpEd = aOpEd,
				};

				using (DataContext aContext = new DataContext(aConnection))
				{
					if (aNewSong.Id == NewIdForDisplay())
					{
						// 新規登録
						YlCommon.InputIdPrefixIfNeededWithInvoke(mOwner, mYukaListerSettings);
						aNewSong.Id = mOwner.mYukaListerSettings.PrepareLastId(aConnection, MusicInfoDbTables.TSong);
						Table<TSong> aTableSong = aContext.GetTable<TSong>();
						aTableSong.InsertOnSubmit(aNewSong);
						mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲テーブル新規登録：" + aNewSong.Id + " / " + aNewSong.Name);
					}
					else
					{
						TSong aExistSong = YlCommon.SelectSongById(aContext, aNewSong.Id, true);
						if (YlCommon.IsUpdated(aExistSong, aNewSong))
						{
							// 更新（既存のレコードが無効化されている場合は有効化も行う）
							aNewSong.UpdateTime = aExistSong.UpdateTime;
							Common.ShallowCopy(aNewSong, aExistSong);
							mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲テーブル更新：" + aNewSong.Id + " / " + aNewSong.Name);
						}
					}

					// 人物紐付け
					YlCommon.RegisterArtistSequence(aContext, aNewSong.Id, aArtistIds);
					YlCommon.RegisterLyristSequence(aContext, aNewSong.Id, aLyristIds);
					YlCommon.RegisterComposerSequence(aContext, aNewSong.Id, aComposerIds);
					YlCommon.RegisterArrangerSequence(aContext, aNewSong.Id, aArrangerIds);

					aContext.SubmitChanges();
				}

				mOwner.RegisteredId = aNewSong.Id;
			}

		}

		// --------------------------------------------------------------------
		// コンポーネント初期化
		// --------------------------------------------------------------------
		public override void Init()
		{
			// コンポーネント
			mComboBoxId = mOwner.ComboBoxId;
			mLabelIdInfo = mOwner.LabelIdInfo;
			mTextBoxRuby = mOwner.TextBoxRuby;
			mTextBoxName = mOwner.TextBoxName;
			mTextBoxKeyword = mOwner.TextBoxKeyword;

			// ログ
			mLogWriter = mOwner.mLogWriter;

			// ID コンボボックス設定
			foreach (TSong aSong in mInitialSongs)
			{
				if (String.IsNullOrEmpty(aSong.Id))
				{
					// 新規楽曲の ID を可読的に設定
					aSong.Id = NewIdForDisplay();
				}
				mOwner.ComboBoxId.Items.Add(aSong.Id);
			}
			UpdateLabelInfo();

			base.Init();
		}

		// --------------------------------------------------------------------
		// レコードの内容をコンポーネントに表示
		// --------------------------------------------------------------------
		public override void RecordToCompos()
		{
			TSong aSong = mInitialSongs[mOwner.ComboBoxId.SelectedIndex];
			Debug.Assert(aSong.Id == (String)mOwner.ComboBoxId.SelectedItem, "RecordToCompos() diff inner/display song info");

			UpdateLabelInfo();

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				// 名称関係
				mOwner.TextBoxRuby.Text = aSong.Ruby;
				mOwner.TextBoxName.Text = aSong.Name;

				// タイアップ関係
				if (String.IsNullOrEmpty(aSong.TieUpId))
				{
					mOwner.CheckBoxTieUp.IsChecked = false;
				}
				else
				{
					mOwner.CheckBoxTieUp.IsChecked = true;
					TTieUp aTieUp = YlCommon.SelectTieUpById(aConnection, aSong.TieUpId);
					if (aTieUp != null)
					{
						mOwner.LabelTieUp.Tag = aTieUp.Id;
						mOwner.LabelTieUp.Content = YlCommon.TieUpNameAvoidingSameName(aConnection, aTieUp);
					}
					else
					{
						mOwner.LabelTieUp.Tag = null;
						mOwner.LabelTieUp.Content = null;
					}
				}
				mOwner.TextBoxOpEd.Text = aSong.OpEd;
				if (String.IsNullOrEmpty(aSong.CategoryId))
				{
					mOwner.CheckBoxCategory.IsChecked = false;
				}
				else
				{
					mOwner.CheckBoxCategory.IsChecked = true;
					TCategory aCategory = YlCommon.SelectCategoryById(aConnection, aSong.CategoryId);
					if (aCategory != null)
					{
						mOwner.LabelCategory.Tag = aCategory.Id;
						mOwner.LabelCategory.Content = aCategory.Name;
					}
					else
					{
						mOwner.LabelCategory.Tag = null;
						mOwner.LabelCategory.Content = null;
					}
				}

				// 人物関係
				using (DataContext aContext = new DataContext(aConnection))
				{
					SetPersonComponents(aConnection, YlCommon.SelectArtistsBySongId(aContext, aSong.Id), mOwner.CheckBoxArtist, mOwner.LabelArtist);
					SetPersonComponents(aConnection, YlCommon.SelectLyristsBySongId(aContext, aSong.Id), mOwner.CheckBoxLyrist, mOwner.LabelLyrist);
					SetPersonComponents(aConnection, YlCommon.SelectComposersBySongId(aContext, aSong.Id), mOwner.CheckBoxComposer, mOwner.LabelComposer);
					SetPersonComponents(aConnection, YlCommon.SelectArrangersBySongId(aContext, aSong.Id), mOwner.CheckBoxArranger, mOwner.LabelArranger);
				}

				// その他
				YlCommon.MjdToTextBox(aSong.ReleaseDate, mOwner.TextBoxReleaseYear, mOwner.TextBoxReleaseMonth, mOwner.TextBoxReleaseDay);
				mOwner.TextBoxKeyword.Text = aSong.Keyword;
			}

		}

		// --------------------------------------------------------------------
		// 新規作成時、既存レコードに同名がある場合は警告
		// --------------------------------------------------------------------
		public override void WarnDuplicateIfNeeded()
		{
			String aNormalizedName = YlCommon.NormalizeDbString(mOwner.TextBoxName.Text);

			List<TSong> aSongs;
			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				aSongs = YlCommon.SelectSongsByName(aConnection, aNormalizedName);
				if (aSongs.Count == 0)
				{
					return;
				}
			}

			foreach (TSong aSong in aSongs)
			{
				if (mOwner.ComboBoxId.Items.IndexOf(aSong.Id) >= 0)
				{
					continue;
				}

				mInitialSongs.Add(aSong);
				mOwner.ComboBoxId.Items.Add(aSong.Id);
			}

			WarnDuplicateIfNeededCore();
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// オーナー
		private EditSongWindow mOwner;

		// 各レコードの初期値
		private List<TSong> mInitialSongs;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 人物コンポーネントの値を設定
		// --------------------------------------------------------------------
		private void SetPersonComponents(SQLiteConnection oConnection, List<TPerson> oPeople, CheckBox oCheckBox, Label oLabel)
		{
			if (oPeople.Count == 0)
			{
				oCheckBox.IsChecked = false;
			}
			else
			{
				oCheckBox.IsChecked = true;
				for (Int32 i = 0; i < oPeople.Count; i++)
				{
					if (i == 0)
					{
						oLabel.Tag = oPeople[i].Id;
						oLabel.Content = oPeople[i].Name;
					}
					else
					{
						oLabel.Tag += "," + oPeople[i].Id;
						oLabel.Content += "," + oPeople[i].Name;
					}
				}
			}
		}
	}
	// public class EditMasterAdapterTSong ___END___

	// ====================================================================
	// TPerson 詳細編集用アダプタークラス
	// タイトルバーのみ、編集対象である Caption 名を表示するが、それ以外は、
	// データベーステーブル名である「人物」での表示とする
	// ====================================================================

	public class EditMasterAdapterTPerson : EditMasterAdapter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public EditMasterAdapterTPerson(EditMasterWindow oOwner, List<TPerson> oInitialPeople, YukaListerSettings oYukaListerSettings, String oCaption)
		{
			// 変数初期化
			mOwner = oOwner;
			mInitialPeople = oInitialPeople;
			mYukaListerSettings = oYukaListerSettings;
			Caption = oCaption;
			HelpAnchor = "KasyuSakushisyaSakkyokusyaHenkyokusyajouhounoShinkitourokutoHenkou";
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力値を確認し、問題なければデータベースを更新
		// --------------------------------------------------------------------
		public override void CheckAndSave()
		{
			TPerson aInitialPerson = mInitialPeople[mOwner.ComboBoxId.SelectedIndex];
			String aId = (String)mOwner.ComboBoxId.SelectedItem;
			String aNormalizedRuby = YlCommon.NormalizeDbRuby(mOwner.TextBoxRuby.Text);
			String aNormalizedName = YlCommon.NormalizeDbString(mOwner.TextBoxName.Text);
			String aNormalizedKeyword = YlCommon.NormalizeDbString(mOwner.TextBoxKeyword.Text);

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				List<TPerson> aPeople = YlCommon.SelectPeopleByName(aConnection, aNormalizedName);

				CheckInput(aInitialPerson.Name, aPeople.Count);

				// 新規作成時、人物名が同じで、かつ、検索キーワードが同じものがあると拒否
				if (aId == NewIdForDisplay() && aPeople.Count > 0)
				{
					foreach (TPerson aPerson in aPeople)
					{
						if (String.IsNullOrEmpty(aNormalizedKeyword) || aNormalizedKeyword == aPerson.Keyword)
						{
							throw new Exception("登録しようとしている" + Caption + "「" + aNormalizedName + "」は既に登録されています。\n"
									+ Caption + " ID を切り替えて登録済みの" + Caption + "を選択してください。\n"
									+ "同名の別" + Caption + "を登録しようとしている場合は、検索ワードを入力して見分けが付くようにして下さい。");
						}
					}
				}

				TPerson aNewPerson = new TPerson
				{
					// TBase
					Id = aId,
					Import = false,
					Invalid = false,
					UpdateTime = YlCommon.INVALID_MJD,
					Dirty = true,

					// TMaster
					Name = aNormalizedName,
					Ruby = aNormalizedRuby,
					Keyword = aNormalizedKeyword,
				};

				using (DataContext aContext = new DataContext(aConnection))
				{
					if (aNewPerson.Id == NewIdForDisplay())
					{
						// 新規登録
						YlCommon.InputIdPrefixIfNeededWithInvoke(mOwner, mYukaListerSettings);
						aNewPerson.Id = mOwner.mYukaListerSettings.PrepareLastId(aConnection, MusicInfoDbTables.TPerson);
						Table<TPerson> aTablePerson = aContext.GetTable<TPerson>();
						aTablePerson.InsertOnSubmit(aNewPerson);
						mOwner.mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "人物テーブル新規登録：" + aNewPerson.Id + " / " + aNewPerson.Name);
					}
					else
					{
						TPerson aExistPerson = YlCommon.SelectPersonById(aContext, aNewPerson.Id, true);
						if (YlCommon.IsUpdated(aExistPerson, aNewPerson))
						{
							// 更新（既存のレコードが無効化されている場合は有効化も行う）
							aNewPerson.UpdateTime = aExistPerson.UpdateTime;
							Common.ShallowCopy(aNewPerson, aExistPerson);
							mOwner.mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "人物テーブル更新：" + aNewPerson.Id + " / " + aNewPerson.Name);
						}
					}

					aContext.SubmitChanges();
				}

				mOwner.RegisteredId = aNewPerson.Id;
			}
		}

		// --------------------------------------------------------------------
		// コンポーネント初期化
		// --------------------------------------------------------------------
		public override void Init()
		{
			// コンポーネント
			mComboBoxId = mOwner.ComboBoxId;
			mLabelIdInfo = mOwner.LabelIdInfo;
			mTextBoxRuby = mOwner.TextBoxRuby;
			mTextBoxName = mOwner.TextBoxName;
			mTextBoxKeyword = mOwner.TextBoxKeyword;

			// ログ
			mLogWriter = mOwner.mLogWriter;

			// ラベル
			mOwner.LabelId.Content = "人物 " + mOwner.LabelId.Content;
			mOwner.LabelName.Content = "人物" + mOwner.LabelName.Content;

			// ヒント
			mTextBoxName.ToolTip = "一人分の人物名のみを入力して下さい（複数名をまとめないで下さい）。";
			HintAssist.SetHint(mTextBoxName, mTextBoxName.ToolTip);
			ToolTipService.SetShowDuration(mTextBoxName, YlCommon.TOOL_TIP_LONG_INTERVAL);

			// ID コンボボックス設定
			foreach (TPerson aPerson in mInitialPeople)
			{
				if (String.IsNullOrEmpty(aPerson.Id))
				{
					// 新規タイアップの ID を可読的に設定
					aPerson.Id = NewIdForDisplay();
				}
				mOwner.ComboBoxId.Items.Add(aPerson.Id);
			}
			UpdateLabelInfo();

			base.Init();
		}

		// --------------------------------------------------------------------
		// レコードの内容をコンポーネントに表示
		// --------------------------------------------------------------------
		public override void RecordToCompos()
		{
			TPerson aPerson = mInitialPeople[mOwner.ComboBoxId.SelectedIndex];
			Debug.Assert(aPerson.Id == (String)mOwner.ComboBoxId.SelectedItem, "RecordToCompos() diff inner/display person info");

			UpdateLabelInfo();

			// 名称関係
			mOwner.TextBoxRuby.Text = aPerson.Ruby;
			mOwner.TextBoxName.Text = aPerson.Name;

			// その他
			mOwner.TextBoxKeyword.Text = aPerson.Keyword;
		}

		// --------------------------------------------------------------------
		// 新規作成時、既存レコードに同名がある場合は警告
		// --------------------------------------------------------------------
		public override void WarnDuplicateIfNeeded()
		{
			String aNormalizedName = YlCommon.NormalizeDbString(mOwner.TextBoxName.Text);

			List<TPerson> aPeople;
			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				aPeople = YlCommon.SelectPeopleByName(aConnection, aNormalizedName);
				if (aPeople.Count == 0)
				{
					return;
				}
			}

			// 同名のレコードが編集対象になっていない場合はコンボボックスに追加する
			foreach (TPerson aPerson in aPeople)
			{
				if (mOwner.ComboBoxId.Items.IndexOf(aPerson.Id) >= 0)
				{
					continue;
				}

				mInitialPeople.Add(aPerson);
				mOwner.ComboBoxId.Items.Add(aPerson.Id);
			}

			WarnDuplicateIfNeededCore();
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// オーナー
		private EditMasterWindow mOwner;

		// 各レコードの初期値
		private List<TPerson> mInitialPeople;
	}
	// public class EditMasterAdapterTPerson ___END___

	// ====================================================================
	// TTieUp 詳細編集用アダプタークラス
	// ====================================================================

	public class EditMasterAdapterTTieUp : EditMasterAdapter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public EditMasterAdapterTTieUp(EditTieUpWindow oOwner, List<TTieUp> oInitialTieUps, YukaListerSettings oYukaListerSettings)
		{
			// 変数初期化
			mOwner = oOwner;
			mInitialTieUps = oInitialTieUps;
			mYukaListerSettings = oYukaListerSettings;
			Caption = "タイアップ";
			HelpAnchor = "TieUpjouhounoShinkitourokutoHenkou";
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力値を確認し、問題なければデータベースを更新
		// --------------------------------------------------------------------
		public override void CheckAndSave()
		{
			TTieUp aInitialTieUp = mInitialTieUps[mOwner.ComboBoxId.SelectedIndex];

			// TBase
			String aId = (String)mOwner.ComboBoxId.SelectedItem;

			// TMaster
			String aNormalizedRuby = YlCommon.NormalizeDbRuby(mOwner.TextBoxRuby.Text);
			String aNormalizedName = YlCommon.NormalizeDbString(mOwner.TextBoxName.Text);
			String aNormalizedKeyword = YlCommon.NormalizeDbString(mOwner.TextBoxKeyword.Text);

			// TTieUp
			String aCategoryId = (String)mOwner.LabelCategory.Tag;
			List<String> aTieUpGroupIds = YlCommon.SplitIds((String)mOwner.LabelTieUpGroup.Tag);
			String aMakerId = (String)mOwner.LabelMaker.Tag;
			Int32 aAgeLimit = Common.StringToInt32(mOwner.TextBoxAgeLimit.Text);
			Double aReleaseDate = YlCommon.TextBoxToMjd("リリース日", mOwner.TextBoxReleaseYear, mOwner.TextBoxReleaseMonth, mOwner.TextBoxReleaseDay);

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				List<TTieUp> aTieUps = YlCommon.SelectTieUpsByName(aConnection, aNormalizedName);

				CheckInput(aInitialTieUp.Name, aTieUps.Count);

				// チェックされているのに指定されていない項目を確認
				if ((Boolean)mOwner.CheckBoxCategory.IsChecked && String.IsNullOrEmpty(aCategoryId))
				{
					throw new Exception("カテゴリーが「あり」になっていますが指定されていません。");
				}
				if ((Boolean)mOwner.CheckBoxMaker.IsChecked && String.IsNullOrEmpty(aMakerId))
				{
					throw new Exception("制作会社が「あり」になっていますが指定されていません。");
				}
				if ((Boolean)mOwner.CheckBoxTieUpGroup.IsChecked && aTieUpGroupIds.Count == 0)
				{
					throw new Exception("シリーズが「あり」になっていますが指定されていません。");
				}

				TTieUp aNewTieUp = new TTieUp
				{
					// TBase
					Id = aId,
					Import = false,
					Invalid = false,
					UpdateTime = YlCommon.INVALID_MJD,
					Dirty = true,

					// TMaster
					Name = aNormalizedName,
					Ruby = aNormalizedRuby,
					Keyword = aNormalizedKeyword,

					// TTieUp
					CategoryId = aCategoryId,
					MakerId = aMakerId,
					AgeLimit = aAgeLimit,
					ReleaseDate = aReleaseDate,
				};

				using (DataContext aContext = new DataContext(aConnection))
				{
					if (aNewTieUp.Id == NewIdForDisplay())
					{
						// 新規登録
						YlCommon.InputIdPrefixIfNeededWithInvoke(mOwner, mYukaListerSettings);
						aNewTieUp.Id = mOwner.mYukaListerSettings.PrepareLastId(aConnection, MusicInfoDbTables.TTieUp);
						Table<TTieUp> aTableTieUp = aContext.GetTable<TTieUp>();
						aTableTieUp.InsertOnSubmit(aNewTieUp);
						mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップテーブル新規登録：" + aNewTieUp.Id + " / " + aNewTieUp.Name);
					}
					else
					{
						TTieUp aExistTieUp = YlCommon.SelectTieUpById(aContext, aNewTieUp.Id);
						if (YlCommon.IsUpdated(aExistTieUp, aNewTieUp))
						{
							// 更新（既存のレコードが無効化されている場合は有効化も行う）
							aNewTieUp.UpdateTime = aExistTieUp.UpdateTime;
							Common.ShallowCopy(aNewTieUp, aExistTieUp);
							mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップテーブル更新：" + aNewTieUp.Id + " / " + aNewTieUp.Name);
						}
					}

					// タイアップグループ紐付け
					YlCommon.RegisterTieUpGroupSequence(aContext, aNewTieUp.Id, aTieUpGroupIds);

					aContext.SubmitChanges();
				}

				mOwner.RegisteredId = aNewTieUp.Id;
			}

		}

		// --------------------------------------------------------------------
		// コンポーネント初期化
		// --------------------------------------------------------------------
		public override void Init()
		{
			// コンポーネント
			mComboBoxId = mOwner.ComboBoxId;
			mLabelIdInfo = mOwner.LabelIdInfo;
			mTextBoxRuby = mOwner.TextBoxRuby;
			mTextBoxName = mOwner.TextBoxName;
			mTextBoxKeyword = mOwner.TextBoxKeyword;

			// ログ
			mLogWriter = mOwner.mLogWriter;

			// ID コンボボックス設定
			foreach (TTieUp aTieUp in mInitialTieUps)
			{
				if (String.IsNullOrEmpty(aTieUp.Id))
				{
					// 新規楽曲の ID を可読的に設定
					aTieUp.Id = NewIdForDisplay();
				}
				mOwner.ComboBoxId.Items.Add(aTieUp.Id);
			}
			UpdateLabelInfo();

			base.Init();
		}

		// --------------------------------------------------------------------
		// レコードの内容をコンポーネントに表示
		// --------------------------------------------------------------------
		public override void RecordToCompos()
		{
			TTieUp aTieUp = mInitialTieUps[mOwner.ComboBoxId.SelectedIndex];
			Debug.Assert(aTieUp.Id == (String)mOwner.ComboBoxId.SelectedItem, "RecordToCompos() diff inner/display tie up info");

			UpdateLabelInfo();

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				// 名称関係
				mOwner.TextBoxRuby.Text = aTieUp.Ruby;
				mOwner.TextBoxName.Text = aTieUp.Name;

				// カテゴリー関係
				if (String.IsNullOrEmpty(aTieUp.CategoryId))
				{
					mOwner.CheckBoxCategory.IsChecked = false;
				}
				else
				{
					mOwner.CheckBoxCategory.IsChecked = true;
					TCategory aCategory = YlCommon.SelectCategoryById(aConnection, aTieUp.CategoryId);
					if (aCategory != null)
					{
						mOwner.LabelCategory.Tag = aCategory.Id;
						mOwner.LabelCategory.Content = aCategory.Name;
					}
					else
					{
						mOwner.LabelCategory.Tag = null;
						mOwner.LabelCategory.Content = null;
					}
				}
				if (aTieUp.AgeLimit == 0)
				{
					mOwner.TextBoxAgeLimit.Text = null;
				}
				else
				{
					mOwner.TextBoxAgeLimit.Text = aTieUp.AgeLimit.ToString();
				}

				// 制作会社
				if (String.IsNullOrEmpty(aTieUp.MakerId))
				{
					mOwner.CheckBoxMaker.IsChecked = false;
				}
				else
				{
					mOwner.CheckBoxMaker.IsChecked = true;
					TMaker aMaker = YlCommon.SelectMakerById(aConnection, aTieUp.MakerId);
					if (aMaker != null)
					{
						mOwner.LabelMaker.Tag = aMaker.Id;
						mOwner.LabelMaker.Content = aMaker.Name;
					}
					else
					{
						mOwner.LabelMaker.Tag = null;
						mOwner.LabelMaker.Content = null;
					}
				}

				// タイアップグループ
				List<TTieUpGroup> aTieUpGroups;
				using (DataContext aContext = new DataContext(aConnection))
				{
					aTieUpGroups = YlCommon.SelectTieUpGroupsByTieUpId(aContext, aTieUp.Id);
				}
				if (aTieUpGroups.Count == 0)
				{
					mOwner.CheckBoxTieUpGroup.IsChecked = false;
				}
				else
				{
					mOwner.CheckBoxTieUpGroup.IsChecked = true;
					for (Int32 i = 0; i < aTieUpGroups.Count; i++)
					{
						if (i == 0)
						{
							mOwner.LabelTieUpGroup.Tag = aTieUpGroups[i].Id;
							mOwner.LabelTieUpGroup.Content = aTieUpGroups[i].Name;
						}
						else
						{
							mOwner.LabelTieUpGroup.Tag += "," + aTieUpGroups[i].Id;
							mOwner.LabelTieUpGroup.Content += "," + aTieUpGroups[i].Name;
						}
					}
				}

				// その他
				YlCommon.MjdToTextBox(aTieUp.ReleaseDate, mOwner.TextBoxReleaseYear, mOwner.TextBoxReleaseMonth, mOwner.TextBoxReleaseDay);
				mOwner.TextBoxKeyword.Text = aTieUp.Keyword;
			}
		}

		// --------------------------------------------------------------------
		// 新規作成時、既存レコードに同名がある場合は警告
		// --------------------------------------------------------------------
		public override void WarnDuplicateIfNeeded()
		{
			String aNormalizedName = YlCommon.NormalizeDbString(mOwner.TextBoxName.Text);

			List<TTieUp> aTieUps;
			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				aTieUps = YlCommon.SelectTieUpsByName(aConnection, aNormalizedName);
				if (aTieUps.Count == 0)
				{
					return;
				}
			}

			foreach (TTieUp aTieUp in aTieUps)
			{
				if (mOwner.ComboBoxId.Items.IndexOf(aTieUp.Id) >= 0)
				{
					continue;
				}

				mInitialTieUps.Add(aTieUp);
				mOwner.ComboBoxId.Items.Add(aTieUp.Id);
			}

			WarnDuplicateIfNeededCore();
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// オーナー
		private EditTieUpWindow mOwner;

		// 各レコードの初期値
		private List<TTieUp> mInitialTieUps;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

	}
	// public class EditMasterAdapterTTieUp ___END___

	// ====================================================================
	// TTieUpGroup 詳細編集用アダプタークラス
	// ====================================================================

	public class EditMasterAdapterTTieUpGroup : EditMasterAdapter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public EditMasterAdapterTTieUpGroup(EditMasterWindow oOwner, List<TTieUpGroup> oInitialTieUpGroups, YukaListerSettings oYukaListerSettings)
		{
			// 変数初期化
			mOwner = oOwner;
			mInitialTieUpGroups = oInitialTieUpGroups;
			mYukaListerSettings = oYukaListerSettings;
			Caption = "シリーズ";
			HelpAnchor = "SeriesjouhounoShinkitourokutoHenkou";
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力値を確認し、問題なければデータベースを更新
		// --------------------------------------------------------------------
		public override void CheckAndSave()
		{
			TTieUpGroup aInitialTieUpGroup = mInitialTieUpGroups[mOwner.ComboBoxId.SelectedIndex];
			String aId = (String)mOwner.ComboBoxId.SelectedItem;
			String aNormalizedRuby = YlCommon.NormalizeDbRuby(mOwner.TextBoxRuby.Text);
			String aNormalizedName = YlCommon.NormalizeDbString(mOwner.TextBoxName.Text);
			String aNormalizedKeyword = YlCommon.NormalizeDbString(mOwner.TextBoxKeyword.Text);

			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				List<TTieUpGroup> aTieUpGroups = YlCommon.SelectTieUpGroupsByName(aConnection, aNormalizedName);

				CheckInput(aInitialTieUpGroup.Name, aTieUpGroups.Count);

				// 新規作成時、タイアップグループ名が同じものがあると拒否
				if (aId == NewIdForDisplay() && aTieUpGroups.Count > 0)
				{
					throw new Exception("登録しようとしている" + Caption + "「" + aNormalizedName + "」は既に登録されています。\n"
							+ Caption + " ID を切り替えて登録済みの" + Caption + "を選択してください。");
				}

				TTieUpGroup aNewTieUpGroup = new TTieUpGroup
				{
					// TBase
					Id = aId,
					Import = false,
					Invalid = false,
					UpdateTime = YlCommon.INVALID_MJD,
					Dirty = true,

					// TMaster
					Name = aNormalizedName,
					Ruby = aNormalizedRuby,
					Keyword = aNormalizedKeyword,
				};

				using (DataContext aContext = new DataContext(aConnection))
				{
					if (aNewTieUpGroup.Id == NewIdForDisplay())
					{
						// 新規登録
						YlCommon.InputIdPrefixIfNeededWithInvoke(mOwner, mYukaListerSettings);
						aNewTieUpGroup.Id = mOwner.mYukaListerSettings.PrepareLastId(aConnection, MusicInfoDbTables.TTieUpGroup);
						Table<TTieUpGroup> aTableTieUpGroup = aContext.GetTable<TTieUpGroup>();
						aTableTieUpGroup.InsertOnSubmit(aNewTieUpGroup);
						mOwner.mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "シリーズテーブル新規登録：" + aNewTieUpGroup.Id + " / " + aNewTieUpGroup.Name);
					}
					else
					{
						TTieUpGroup aExistTieUpGroup = YlCommon.SelectTieUpGroupById(aContext, aNewTieUpGroup.Id, true);
						if (YlCommon.IsUpdated(aExistTieUpGroup, aNewTieUpGroup))
						{
							// 更新（既存のレコードが無効化されている場合は有効化も行う）
							aNewTieUpGroup.UpdateTime = aExistTieUpGroup.UpdateTime;
							Common.ShallowCopy(aNewTieUpGroup, aExistTieUpGroup);
							mOwner.mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "シリーズテーブル更新：" + aNewTieUpGroup.Id + " / " + aNewTieUpGroup.Name);
						}
					}

					aContext.SubmitChanges();
				}

				mOwner.RegisteredId = aNewTieUpGroup.Id;
			}
		}

		// --------------------------------------------------------------------
		// コンポーネント初期化
		// --------------------------------------------------------------------
		public override void Init()
		{
			// コンポーネント
			mComboBoxId = mOwner.ComboBoxId;
			mLabelIdInfo = mOwner.LabelIdInfo;
			mTextBoxRuby = mOwner.TextBoxRuby;
			mTextBoxName = mOwner.TextBoxName;
			mTextBoxKeyword = mOwner.TextBoxKeyword;

			// ログ
			mLogWriter = mOwner.mLogWriter;

			// ラベル
			mOwner.LabelId.Content = "シリーズ " + mOwner.LabelId.Content;
			mOwner.LabelName.Content = "シリーズ" + mOwner.LabelName.Content;

			// ヒント
			mTextBoxName.ToolTip = "シリーズ名に「" + YlCommon.TIE_UP_GROUP_SUFFIX + "」は含めないで下さい。";
			HintAssist.SetHint(mTextBoxName, mTextBoxName.ToolTip);
			ToolTipService.SetShowDuration(mTextBoxName, YlCommon.TOOL_TIP_LONG_INTERVAL);

			// ID コンボボックス設定
			foreach (TTieUpGroup aTieUpGroup in mInitialTieUpGroups)
			{
				if (String.IsNullOrEmpty(aTieUpGroup.Id))
				{
					// 新規タイアップの ID を可読的に設定
					aTieUpGroup.Id = NewIdForDisplay();
				}
				mOwner.ComboBoxId.Items.Add(aTieUpGroup.Id);
			}
			UpdateLabelInfo();

			base.Init();
		}

		// --------------------------------------------------------------------
		// レコードの内容をコンポーネントに表示
		// --------------------------------------------------------------------
		public override void RecordToCompos()
		{
			TTieUpGroup aTieUpGroup = mInitialTieUpGroups[mOwner.ComboBoxId.SelectedIndex];
			Debug.Assert(aTieUpGroup.Id == (String)mOwner.ComboBoxId.SelectedItem, "RecordToCompos() diff inner/display tie up group info");

			UpdateLabelInfo();

			// 名称関係
			mOwner.TextBoxRuby.Text = aTieUpGroup.Ruby;
			mOwner.TextBoxName.Text = aTieUpGroup.Name;

			// その他
			mOwner.TextBoxKeyword.Text = aTieUpGroup.Keyword;
		}

		// --------------------------------------------------------------------
		// 新規作成時、既存レコードに同名がある場合は警告
		// --------------------------------------------------------------------
		public override void WarnDuplicateIfNeeded()
		{
			String aNormalizedName = YlCommon.NormalizeDbString(mOwner.TextBoxName.Text);

			List<TTieUpGroup> aTieUpGroups;
			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				aTieUpGroups = YlCommon.SelectTieUpGroupsByName(aConnection, aNormalizedName);
				if (aTieUpGroups.Count == 0)
				{
					return;
				}
			}

			// 同名のレコードが編集対象になっていない場合はコンボボックスに追加する
			foreach (TTieUpGroup aTieUpGroup in aTieUpGroups)
			{
				if (mOwner.ComboBoxId.Items.IndexOf(aTieUpGroup.Id) >= 0)
				{
					continue;
				}

				mInitialTieUpGroups.Add(aTieUpGroup);
				mOwner.ComboBoxId.Items.Add(aTieUpGroup.Id);
			}

			WarnDuplicateIfNeededCore();
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// オーナー
		private EditMasterWindow mOwner;

		// 各レコードの初期値
		private List<TTieUpGroup> mInitialTieUpGroups;
	}
	// public class EditMasterAdapterTTieUpGroup ___END___


}
// namespace YukaLister ___END___
