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
using System.Data.SQLite;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using YukaLister.Shared;

namespace YukaLister
{
	/// <summary>
	/// EditTieUpWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class EditTieUpWindow : Window
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public EditTieUpWindow(YukaListerSettings oYukaListerSettings, LogWriter oLogWriter)
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
		private void ContextMenuButtonSelectAgeLimitItem_Click(Object oSender, EventArgs oEventArgs)
		{
			try
			{
				MenuItem aItem = (MenuItem)oSender;
				Int32 aAgeLimit = Common.StringToInt32((String)aItem.Header);
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
		// 初期化
		// --------------------------------------------------------------------
		private void Init()
		{
			// タイトルバー
			Title = "タイアップ詳細情報の編集";
#if DEBUG
			Title = "［デバッグ］" + Title;
#endif

			Adapter.Init();

			// カテゴリー
			YlCommon.SetContextMenuItemCategories(ButtonSelectCategory, ContextMenuButtonSelectCategoryItem_Click);

			// 年齢制限
			YlCommon.AddContextMenuItem(ButtonSelectAgeLimit, "全年齢対象（CERO A 相当）", ContextMenuButtonSelectAgeLimitItem_Click);
			YlCommon.AddContextMenuItem(ButtonSelectAgeLimit, YlCommon.AGE_LIMIT_CERO_B.ToString() + " 才以上対象（CERO B 相当）", ContextMenuButtonSelectAgeLimitItem_Click);
			YlCommon.AddContextMenuItem(ButtonSelectAgeLimit, YlCommon.AGE_LIMIT_CERO_C.ToString() + " 才以上対象（CERO C 相当）", ContextMenuButtonSelectAgeLimitItem_Click);
			YlCommon.AddContextMenuItem(ButtonSelectAgeLimit, YlCommon.AGE_LIMIT_CERO_D.ToString() + " 才以上対象（CERO D 相当）", ContextMenuButtonSelectAgeLimitItem_Click);
			YlCommon.AddContextMenuItem(ButtonSelectAgeLimit, YlCommon.AGE_LIMIT_CERO_Z.ToString() + " 才以上対象（CERO Z 相当）", ContextMenuButtonSelectAgeLimitItem_Click);

			Common.CascadeWindow(this);
		}

		// --------------------------------------------------------------------
		// カテゴリー関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateCategoryComponents()
		{
			ButtonSelectCategory.IsEnabled = (Boolean)CheckBoxCategory.IsChecked;
			if (!(Boolean)CheckBoxCategory.IsChecked)
			{
				LabelCategory.Tag = null;
				LabelCategory.Content = null;
			}
		}

		// --------------------------------------------------------------------
		// 制作会社関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateMakerComponents()
		{
			ButtonSearchMaker.IsEnabled = (Boolean)CheckBoxMaker.IsChecked;
			ButtonEditMaker.IsEnabled = (Boolean)CheckBoxMaker.IsChecked;
			if (!(Boolean)CheckBoxMaker.IsChecked)
			{
				LabelMaker.Tag = null;
				LabelMaker.Content = null;
			}
		}

		// --------------------------------------------------------------------
		// タイアップグループ関係のコンポーネントを更新
		// --------------------------------------------------------------------
		private void UpdateTieUpGroupComponents()
		{
			ButtonSearchTieUpGroup.IsEnabled = (Boolean)CheckBoxTieUpGroup.IsChecked;
			ButtonEditTieUpGroup.IsEnabled = (Boolean)CheckBoxTieUpGroup.IsChecked;
			if (!(Boolean)CheckBoxTieUpGroup.IsChecked)
			{
				LabelTieUpGroup.Tag = null;
				LabelTieUpGroup.Content = null;
			}
		}

		// ====================================================================
		// IDE 生成イベントハンドラー
		// ====================================================================

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "タイアップ詳細編集ウィンドウを開きます。");
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
				UpdateCategoryComponents();
				UpdateMakerComponents();
				UpdateTieUpGroupComponents();
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ詳細編集ウィンドウロード時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
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
				mLogWriter.ShowLogMessage(TraceEventType.Error, "タイアップ ID 選択変更時エラー：\n" + oExcep.Message);
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

		private void TextBoxName_LostFocus(object sender, RoutedEventArgs e)
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

		private void CheckBoxCategory_Checked(object sender, RoutedEventArgs e)
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

		private void CheckBoxMaker_Checked(object sender, RoutedEventArgs e)
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

		private void CheckBoxTieUpGroup_Checked(object sender, RoutedEventArgs e)
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

		private void ButtonSelectAgeLimit_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ButtonSelectAgeLimit.ContextMenu.IsOpen = true;
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "年齢制限選択ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchMaker_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SearchMusicInfoWindow aSearchMusicInfoWindow = new SearchMusicInfoWindow("制作会社", MusicInfoDbTables.TMaker, (String)LabelMaker.Content, mLogWriter);
				aSearchMusicInfoWindow.Owner = this;
				if ((Boolean)aSearchMusicInfoWindow.ShowDialog())
				{
					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						List<TMaker> aMakers = YlCommon.SelectMakersByName(aConnection, aSearchMusicInfoWindow.SelectedName);
						if (aMakers.Count > 0)
						{
							LabelMaker.Tag = aMakers[0].Id;
							LabelMaker.Content = aMakers[0].Name;
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

		private void ButtonEditMaker_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (String.IsNullOrEmpty((String)LabelMaker.Content))
				{
					if (!mIsMakerSearched)
					{
						throw new Exception("制作会社が選択されていないため新規制作会社情報作成となりますが、その前に一度、目的の制作会社が未登録かどうか検索して下さい。");
					}

					if (MessageBox.Show("制作会社が選択されていません。\n新規に制作会社情報を作成しますか？\n"
							+ "（目的の制作会社が未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
							MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
					{
						return;
					}
				}

				// 既存レコードを用意
				List<TMaker> aMakers;
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					aMakers = YlCommon.SelectMakersByName(aConnection, (String)LabelMaker.Content);
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

				EditMasterWindow aEditMasterWindow = new EditMasterWindow(mYukaListerSettings, mLogWriter);
				EditMasterAdapter aAdapter = new EditMasterAdapterTMaker(aEditMasterWindow, aMakers, mYukaListerSettings);
				aEditMasterWindow.Adapter = aAdapter;
				aEditMasterWindow.DefaultId = (String)LabelMaker.Tag;
				aEditMasterWindow.Owner = this;
				if (!(Boolean)aEditMasterWindow.ShowDialog())
				{
					return;
				}

				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					TMaker aMaker = YlCommon.SelectMakerById(aConnection, aEditMasterWindow.RegisteredId);
					if (aMaker != null)
					{
						LabelMaker.Tag = aMaker.Id;
						LabelMaker.Content = aMaker.Name;
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "制作会社詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
				mLogWriter.ShowLogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		private void ButtonSearchTieUpGroup_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				SearchMusicInfoWindow aSearchMusicInfoWindow = new SearchMusicInfoWindow("シリーズ", MusicInfoDbTables.TTieUpGroup, (String)LabelTieUpGroup.Content, mLogWriter);
				aSearchMusicInfoWindow.Owner = this;
				if ((Boolean)aSearchMusicInfoWindow.ShowDialog())
				{
					using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
					{
						List<TTieUpGroup> aTieUpGroups = YlCommon.SelectTieUpGroupsByName(aConnection, aSearchMusicInfoWindow.SelectedName);
						if (aTieUpGroups.Count > 0)
						{
							LabelTieUpGroup.Tag = aTieUpGroups[0].Id;
							LabelTieUpGroup.Content = aTieUpGroups[0].Name;
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

		private void ButtonEditTieUpGroup_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (String.IsNullOrEmpty((String)LabelTieUpGroup.Content))
				{
					if (!mIsTieUpGroupSearched)
					{
						throw new Exception("シリーズが選択されていないため新規シリーズ情報作成となりますが、その前に一度、目的のシリーズが未登録かどうか検索して下さい。");
					}

					if (MessageBox.Show("シリーズが選択されていません。\n新規にシリーズ情報を作成しますか？\n"
							+ "（目的のシリーズが未登録の場合（検索してもヒットしない場合）に限り、新規作成を行って下さい）", "確認",
							MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
					{
						return;
					}
				}

				// 既存レコードを用意
				List<TTieUpGroup> aTieUpGroups;
				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					aTieUpGroups = YlCommon.SelectTieUpGroupsByName(aConnection, (String)LabelTieUpGroup.Content);
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

				EditMasterWindow aEditMasterWindow = new EditMasterWindow(mYukaListerSettings, mLogWriter);
				EditMasterAdapter aAdapter = new EditMasterAdapterTTieUpGroup(aEditMasterWindow, aTieUpGroups, mYukaListerSettings);
				aEditMasterWindow.Adapter = aAdapter;
				aEditMasterWindow.DefaultId = (String)LabelTieUpGroup.Tag;
				aEditMasterWindow.Owner = this;
				if (!(Boolean)aEditMasterWindow.ShowDialog())
				{
					return;
				}

				using (SQLiteConnection aConnection = YlCommon.CreateMusicInfoDbConnection())
				{
					TTieUpGroup aTieUpGroup = YlCommon.SelectTieUpGroupById(aConnection, aEditMasterWindow.RegisteredId);
					if (aTieUpGroup != null)
					{
						LabelTieUpGroup.Tag = aTieUpGroup.Id;
						LabelTieUpGroup.Content = aTieUpGroup.Name;
					}
				}
			}
			catch (Exception oExcep)
			{
				mLogWriter.ShowLogMessage(TraceEventType.Error, "シリーズ詳細編集ボタンクリック時エラー：\n" + oExcep.Message);
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
	// public partial class EditTieUpWindow ___END___
}
// namespace YukaLister ___END___
