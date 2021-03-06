﻿// ============================================================================
// 
// 複数人物編集ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ビューは EditSequenceWindow を使う。
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;

using Shinta;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

using YukaLister.Models;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.ViewModels
{
	public class EditPeopleWindowViewModel : EditSequenceWindowViewModel
	{
		// Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).

		// This method would be called from View, when ContentRendered event was raised.

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// 人物区分
		public String PersonKind { get; set; }

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// 本関数を呼ぶ前に Environment を設定しておく必要がある
		// --------------------------------------------------------------------
		public override void Initialize()
		{
			Debug.Assert(Environment != null, "Environment is null");
			try
			{
				// 基底クラス初期化
				base.Initialize();

				// タイトルバー
				Title = PersonKind + "たちの編集";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				// 説明
				Description = "「検索して追加」ボタンで" + PersonKind + "を追加して下さい。複数名の指定も可能です。";
				HelpCommandParameter = "KasyuSakushisyaSakkyokusyaHenkyokusyanoSentaku";

				// 人物群
				DataGridHeader = "人物";
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					for (Int32 i = 0; i < InitialIds.Count; i++)
					{
						TPerson aPerson = YlCommon.SelectBaseById<TPerson>(aContext, InitialIds[i]);
						if (aPerson != null)
						{
							List<TPerson> aSameNamePeople = YlCommon.SelectMastersByName<TPerson>(aContext, aPerson.Name);
							aPerson.Environment = Environment;
							aPerson.AvoidSameName = aSameNamePeople.Count > 1;
							Masters.Add(aPerson);
						}
					}
				}

				// ボタン
				ButtonEditContent = "人物詳細編集 (_E)";
				ButtonNewContent = "新規人物作成 (_N)";

			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "複数人物編集ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
				Environment.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
			}
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// マスターを検索して追加
		// --------------------------------------------------------------------
		protected override void Add()
		{
			using (SearchMusicInfoWindowViewModel aSearchMusicInfoWindowViewModel = new SearchMusicInfoWindowViewModel())
			{
				aSearchMusicInfoWindowViewModel.Environment = Environment;
				aSearchMusicInfoWindowViewModel.ItemName = PersonKind;
				aSearchMusicInfoWindowViewModel.TableIndex = MusicInfoDbTables.TPerson;
				aSearchMusicInfoWindowViewModel.SelectedKeyword = null;
				Messenger.Raise(new TransitionMessage(aSearchMusicInfoWindowViewModel, "OpenSearchMusicInfoWindow"));
				mIsMasterSearched = true;
				if (String.IsNullOrEmpty(aSearchMusicInfoWindowViewModel.DecidedName))
				{
					return;
				}

				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					List<TPerson> aMasters = YlCommon.SelectMastersByName<TPerson>(aMusicInfoDbInDisk.Connection, aSearchMusicInfoWindowViewModel.DecidedName);
					if (aMasters.Count == 0)
					{
						throw new Exception(aSearchMusicInfoWindowViewModel.DecidedName + "がデータベースに登録されていません。");
					}
					if (MastersIndexOfById(aMasters[0]) >= 0)
					{
						throw new Exception(aSearchMusicInfoWindowViewModel.DecidedName + "は既に追加されています。");
					}
					aMasters[0].Environment = Environment;
					aMasters[0].AvoidSameName = aMasters.Count > 1;
					Masters.Add(aMasters[0]);
					SelectedMaster = aMasters[0];
				}
			}
		}

		// --------------------------------------------------------------------
		// 人物を編集
		// --------------------------------------------------------------------
		protected override void Edit()
		{
			if (SelectedMaster == null)
			{
				return;
			}

			// 既存レコード（同名の人物すべて）を用意
			List<TPerson> aMasters;
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			{
				aMasters = YlCommon.SelectMastersByName<TPerson>(aMusicInfoDbInDisk.Connection, SelectedMaster.Name);
			}

			// 新規作成用を追加
			TPerson aNewRecord = new TPerson
			{
				// IRcBase
				Id = null,
				Import = false,
				Invalid = false,
				UpdateTime = YlConstants.INVALID_MJD,
				Dirty = true,

				// IRcMaster
				Name = SelectedMaster.Name,
				Ruby = null,
				Keyword = null,
			};
			aMasters.Insert(0, aNewRecord);

			TPerson aMaster = OpenEditPersonWindow(aMasters, SelectedMaster.Id);
			if (aMaster != null)
			{
				Masters[Masters.IndexOf(SelectedMaster)] = aMaster;
				SelectedMaster = aMaster;
			}
		}

		// --------------------------------------------------------------------
		// マスターを新規作成
		// --------------------------------------------------------------------
		protected override void New()
		{
			if (!mIsMasterSearched)
			{
				throw new Exception("新規人物作成の前に一度、目的の人物が未登録かどうか検索して下さい。");
			}

			if (MessageBox.Show("目的の人物が未登録の場合（検索してもヒットしない場合）に限り、新規人物作成を行って下さい。\n"
					+ "新規人物作成を行いますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
			{
				return;
			}

			// 新規人物
			List<TPerson> aMasters = new List<TPerson>();
			TPerson aNewRecord = new TPerson
			{
				// IRcBase
				Id = null,
				Import = false,
				Invalid = false,
				UpdateTime = YlConstants.INVALID_MJD,
				Dirty = true,

				// IRcMaster
				Name = null,
				Ruby = null,
				Keyword = null,
			};
			aMasters.Insert(0, aNewRecord);

			TPerson aMaster = OpenEditPersonWindow(aMasters, null);
			if (aMaster != null)
			{
				Masters.Add(aMaster);
				SelectedMaster = aMaster;
			}
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 編集ウィンドウを開く
		// ＜返値＞ 編集された TPerson（キャンセルの場合は null）
		// --------------------------------------------------------------------
		private TPerson OpenEditPersonWindow(List<TPerson> oMasters, String oDefaultId)
		{
			using (EditPersonWindowViewModel aEditPersonWindowViewModel = new EditPersonWindowViewModel())
			{
				aEditPersonWindowViewModel.Environment = Environment;
				aEditPersonWindowViewModel.SetMasters(oMasters);
				aEditPersonWindowViewModel.DefaultId = oDefaultId;
				Messenger.Raise(new TransitionMessage(aEditPersonWindowViewModel, "OpenEditPersonWindow"));

				if (String.IsNullOrEmpty(aEditPersonWindowViewModel.OkSelectedId))
				{
					return null;
				}

				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					TPerson aMaster = YlCommon.SelectBaseById<TPerson>(aContext, aEditPersonWindowViewModel.OkSelectedId);
					if (aMaster != null)
					{
						List<TPerson> aSameNamePeople = YlCommon.SelectMastersByName<TPerson>(aContext, aMaster.Name);
						aMaster.Environment = Environment;
						aMaster.AvoidSameName = aSameNamePeople.Count > 1;
					}
					return aMaster;
				}
			}
		}

	}
	// public class EditPeopleWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
