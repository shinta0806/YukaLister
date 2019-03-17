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
using System.Data.SQLite;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using YukaLister.Shared;

namespace YukaLister
{
	/// <summary>
	/// EditSongWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class EditSongWindow : Window
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public EditSongWindow(YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
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
		// ButtonSelectOpEd のコンテキストメニューにアイテムを追加
		// --------------------------------------------------------------------
		private void AddContextMenuItemToButtonSelectOpEd(String oLabel)
		{
			YlCommon.AddContextMenuItem(ButtonSelectOpEd, oLabel, ContextMenuButtonSelectOpEdItem_Click);
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private void ContextMenuButtonSelectCategoryItem_Click(Object oSender, RoutedEventArgs oRoutedEventArgs)
		{
			try
			{
				MenuItem aItem = (MenuItem)oSender;
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					List<TCategory> aCategories = YlCommon.SelectCategoriesByName(aConnection, (String)aItem.Header);
					if (aCategories.Count > 0)
					{
						LabelCategory.Tag = aCategories[0].Id;
						LabelCategory.Content = aCategories[0].Name;
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
		private void ContextMenuButtonSelectOpEdItem_Click(Object oSender, RoutedEventArgs oRoutedEventArgs)
		{
			try
			{
				MenuItem aItem = (MenuItem)oSender;
				String aItemText = (String)aItem.Header;
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
			EditPeopleWindow aEditPeopleWindow = new EditPeopleWindow(oCaption, YlCommon.SplitIds((String)oLabel.Tag), mYukaListerSettings, mLogWriter);
			aEditPeopleWindow.Owner = this;
			if (!(Boolean)aEditPeopleWindow.ShowDialog())
			{
				return;
			}

			oLabel.Tag = null;
			oLabel.Content = null;
			using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
			{
				for (Int32 i = 0; i < aEditPeopleWindow.RegisteredIds.Count; i++)
				{
					TPerson aPerson = YlCommon.SelectPersonById(aConnection, aEditPeopleWindow.RegisteredIds[i]);
					if (i == 0)
					{
						oLabel.Tag = aPerson.Id;
						oLabel.Content = aPerson.Name;
					}
					else
					{
						oLabel.Tag += "," + aPerson.Id;
						oLabel.Content += "," + aPerson.Name;
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Title = "楽曲詳細情報の編集";
#if DEBUG
			Title = "［デバッグ］" + Title;
#endif

			Adapter.Init();

			// 摘要
			AddContextMenuItemToButtonSelectOpEd("OP（オープニング）");
			AddContextMenuItemToButtonSelectOpEd("ED（エンディング）");
			AddContextMenuItemToButtonSelectOpEd("IN（挿入歌）");
			AddContextMenuItemToButtonSelectOpEd("IM（イメージソング）");
			AddContextMenuItemToButtonSelectOpEd("CH（キャラクターソング）");

			// カテゴリー
			YlCommon.SetContextMenuItemCategories(ButtonSelectCategory, ContextMenuButtonSelectCategoryItem_Click);

			Common.CascadeWindow(this);
		}

		// --------------------------------------------------------------------
		// 人物を検索してラベルに設定
		// --------------------------------------------------------------------
		private void SearchPerson(String oCaption, Label oLabel)
		{
			// 人物が複数指定されている場合は先頭のみで検索
			String aKeyword = (String)oLabel.Content;
			if (!String.IsNullOrEmpty(aKeyword))
			{
				Int32 aPos = aKeyword.IndexOf(',');
				if (aPos > 0)
				{
					aKeyword = aKeyword.Substring(0, aPos);
				}
			}

			SearchMusicInfoWindow aSearchMusicInfoWindow = new SearchMusicInfoWindow(oCaption, MusicInfoDbTables.TPerson, aKeyword, mLogWriter);
			aSearchMusicInfoWindow.Owner = this;
			if ((Boolean)aSearchMusicInfoWindow.ShowDialog())
			{
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					List<TPerson> aPeople = YlCommon.SelectPeopleByName(aConnection, aSearchMusicInfoWindow.SelectedName);
					if (aPeople.Count > 0)
					{
						oLabel.Tag = aPeople[0].Id;
						oLabel.Content = aPeople[0].Name;
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 編曲者の同上ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonSameArranger()
		{
			ButtonSameArranger.IsEnabled = (Boolean)CheckBoxArranger.IsChecked;
		}

		// --------------------------------------------------------------------
		// 作曲者の同上ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonSameComposer()
		{
			ButtonSameComposer.IsEnabled = (Boolean)CheckBoxComposer.IsChecked;
		}

		// --------------------------------------------------------------------
		// 作詞者の同上ボタンを更新
		// --------------------------------------------------------------------
		private void UpdateButtonSameLyrist()
		{
			ButtonSameLyrist.IsEnabled = (Boolean)CheckBoxLyrist.IsChecked;
		}

		// --------------------------------------------------------------------
		// カテゴリー関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateCategoryComponents()
		{
			CheckBoxCategory.IsEnabled = !(CheckBoxTieUp.IsEnabled && (Boolean)CheckBoxTieUp.IsChecked);

			ButtonSelectCategory.IsEnabled = (Boolean)CheckBoxCategory.IsChecked;
			if (!(Boolean)CheckBoxCategory.IsChecked)
			{
				LabelCategory.Tag = null;
				LabelCategory.Content = null;
			}
		}

		// --------------------------------------------------------------------
		// 人物関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdatePersonComponents(CheckBox oCheckBox, Button oButtonSearch, Button oButtonEdit, Label oLabel)
		{
			oButtonSearch.IsEnabled = (Boolean)oCheckBox.IsChecked;
			oButtonEdit.IsEnabled = (Boolean)oCheckBox.IsChecked;
			if (!(Boolean)oCheckBox.IsChecked)
			{
				oLabel.Tag = null;
				oLabel.Content = null;
			}
		}

		// --------------------------------------------------------------------
		// タイアップ関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateTieUpComponents()
		{
			CheckBoxTieUp.IsEnabled = !(CheckBoxCategory.IsEnabled && (Boolean)CheckBoxCategory.IsChecked);

			ButtonSearchTieUp.IsEnabled = (Boolean)CheckBoxTieUp.IsChecked;
			ButtonEditTieUp.IsEnabled = (Boolean)CheckBoxTieUp.IsChecked;
			if (!(Boolean)CheckBoxTieUp.IsChecked)
			{
				LabelTieUp.Tag = null;
				LabelTieUp.Content = null;
			}
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "楽曲詳細編集ウィンドウを開きます。");
				Init();

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
				mLogWriter.ShowLogMessage(TraceEventType.Error, "楽曲詳細編集ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
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

		private void ComboBoxId_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

		private void TextBoxName_LostFocus(object sender, RoutedEventArgs e)
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

		private void CheckBoxTieUp_Checked(object sender, RoutedEventArgs e)
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

		private void CheckBoxCategory_Checked(object sender, RoutedEventArgs e)
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

		private void CheckBoxArtist_Checked(object sender, RoutedEventArgs e)
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

		private void CheckBoxLyrist_Checked(object sender, RoutedEventArgs e)
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

		private void CheckBoxComposer_Checked(object sender, RoutedEventArgs e)
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

		private void CheckBoxArranger_Checked(object sender, RoutedEventArgs e)
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

		private void ButtonSearchArtist_Click(object sender, RoutedEventArgs e)
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

		private void ButtonEditArtist_Click(object sender, RoutedEventArgs e)
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

		private void ButtonOK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Adapter.CheckAndSave();
				DialogResult = true;
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

		private void ButtonSearchLyrist_Click(object sender, RoutedEventArgs e)
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

		private void ButtonSearchComposer_Click(object sender, RoutedEventArgs e)
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

		private void ButtonSearchArranger_Click(object sender, RoutedEventArgs e)
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

		private void ButtonEditLyrist_Click(object sender, RoutedEventArgs e)
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

		private void ButtonEditComposer_Click(object sender, RoutedEventArgs e)
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

		private void ButtonEditArranger_Click(object sender, RoutedEventArgs e)
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

		private void ButtonSearchTieUp_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SearchMusicInfoWindow aSearchMusicInfoWindow = new SearchMusicInfoWindow("タイアップ", MusicInfoDbTables.TTieUp, (String)LabelTieUp.Content, mLogWriter);
				aSearchMusicInfoWindow.Owner = this;
				if ((Boolean)aSearchMusicInfoWindow.ShowDialog())
				{
					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						List<TTieUp> aTieUps = YlCommon.SelectTieUpsByName(aConnection, aSearchMusicInfoWindow.SelectedName);
						if (aTieUps.Count > 0)
						{
							LabelTieUp.Tag = aTieUps[0].Id;
							LabelTieUp.Content = aTieUps[0].Name;
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

		private void ButtonEditTieUp_Click(object sender, RoutedEventArgs e)
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
							MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
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

				EditTieUpWindow aEditTieUpWindow = new EditTieUpWindow(mYukaListerSettings, mLogWriter);
				EditMasterAdapter aAdapter = new EditMasterAdapterTTieUp(aEditTieUpWindow, aTieUps, mYukaListerSettings);
				aEditTieUpWindow.Adapter = aAdapter;
				aEditTieUpWindow.DefaultId = (String)LabelTieUp.Tag;
				aEditTieUpWindow.Owner = this;
				if (!(Boolean)aEditTieUpWindow.ShowDialog())
				{
					return;
				}

				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					TTieUp aTieUp = YlCommon.SelectTieUpById(aConnection, aEditTieUpWindow.RegisteredId);
					if (aTieUp != null)
					{
						LabelTieUp.Tag = aTieUp.Id;
						LabelTieUp.Content = YlCommon.TieUpNameAvoidingSameName(aConnection, aTieUp);
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSelectCategory_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ButtonSelectCategory.ContextMenu.IsOpen = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "カテゴリー選択ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSelectOpEd_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ButtonSelectOpEd.ContextMenu.IsOpen = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "摘要選択ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSameLyrist_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				LabelLyrist.Tag = LabelArtist.Tag;
				LabelLyrist.Content = LabelArtist.Content;
				UpdateButtonSameComposer();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "作詞者同上ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSameComposer_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				LabelComposer.Tag = LabelLyrist.Tag;
				LabelComposer.Content = LabelLyrist.Content;
				UpdateButtonSameArranger();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "作曲者同上ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSameArranger_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				LabelArranger.Tag = LabelComposer.Tag;
				LabelArranger.Content = LabelComposer.Content;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "編曲者同上ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonCancel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DialogResult = false;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "キャンセルボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}
	}
}
