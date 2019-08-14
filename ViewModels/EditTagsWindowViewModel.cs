// ============================================================================
// 
// 複数タグ編集ウィンドウの ViewModel
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
	public class EditTagsWindowViewModel : EditSequenceWindowViewModel
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
				Title = "タグ群の編集";
#if DEBUG
				Title = "［デバッグ］" + Title;
#endif

				// 説明
				Description = "「検索して追加」ボタンでタグを追加して下さい。複数タグの指定も可能です。";
				HelpCommandParameter = "";

				// タグ群
				DataGridHeader = "タグ";
				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					for (Int32 i = 0; i < InitialIds.Count; i++)
					{
						TTag aTag = YlCommon.SelectBaseById<TTag>(aContext, InitialIds[i]);
						if (aTag != null)
						{
							List<TTag> aSameNameTags = YlCommon.SelectMastersByName<TTag>(aContext, aTag.Name);
							Debug.Assert(aSameNameTags.Count <= 1, "Same name tag exists");
							aTag.Environment = Environment;
							aTag.AvoidSameName = false;
							Masters.Add(aTag);
						}
					}
				}

				// ボタン
				ButtonEditContent = "タグ詳細編集 (_E)";
				ButtonNewContent = "新規タグ作成 (_N)";
			}
			catch (Exception oExcep)
			{
				Environment.LogWriter.ShowLogMessage(TraceEventType.Error, "複数タグ編集ウィンドウビューモデル初期化時エラー：\n" + oExcep.Message);
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
				aSearchMusicInfoWindowViewModel.ItemName = "タグ";
				aSearchMusicInfoWindowViewModel.TableIndex = MusicInfoDbTables.TTag;
				aSearchMusicInfoWindowViewModel.SelectedKeyword = null;
				Messenger.Raise(new TransitionMessage(aSearchMusicInfoWindowViewModel, "OpenSearchMusicInfoWindow"));
				mIsMasterSearched = true;
				if (String.IsNullOrEmpty(aSearchMusicInfoWindowViewModel.DecidedName))
				{
					return;
				}

				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				{
					List<TTag> aMasters = YlCommon.SelectMastersByName<TTag>(aMusicInfoDbInDisk.Connection, aSearchMusicInfoWindowViewModel.DecidedName);
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

			// 既存レコードを用意
			List<TTag> aMasters;
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
			{
				aMasters = YlCommon.SelectMastersByName<TTag>(aMusicInfoDbInDisk.Connection, SelectedMaster.Name);
				Debug.Assert(aMasters.Count <= 1, "Edit() same name exists");
			}

			// 新規作成用を追加
			TTag aNewRecord = new TTag
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

			TTag aMaster = OpenEditTagWindow(aMasters, SelectedMaster.Id);
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
				throw new Exception("新規タグ作成の前に一度、目的のタグが未登録かどうか検索して下さい。");
			}

			if (MessageBox.Show("目的のタグが未登録の場合（検索してもヒットしない場合）に限り、新規タグ作成を行って下さい。\n"
					+ "新規タグ作成を行いますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
			{
				return;
			}

			// 新規タグ
			List<TTag> aMasters = new List<TTag>();
			TTag aNewRecord = new TTag
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

			TTag aMaster = OpenEditTagWindow(aMasters, null);
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
		// ＜返値＞ 編集された TTag（キャンセルの場合は null）
		// --------------------------------------------------------------------
		private TTag OpenEditTagWindow(List<TTag> oMasters, String oDefaultId)
		{
			using (EditTagWindowViewModel aEditTagWindowViewModel = new EditTagWindowViewModel())
			{
				aEditTagWindowViewModel.Environment = Environment;
				aEditTagWindowViewModel.SetMasters(oMasters);
				aEditTagWindowViewModel.DefaultId = oDefaultId;
				Messenger.Raise(new TransitionMessage(aEditTagWindowViewModel, "OpenEditTagWindow"));

				if (String.IsNullOrEmpty(aEditTagWindowViewModel.OkSelectedId))
				{
					return null;
				}

				using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(Environment))
				using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					TTag aMaster = YlCommon.SelectBaseById<TTag>(aContext, aEditTagWindowViewModel.OkSelectedId);
					if (aMaster != null)
					{
						List<TTag> aSameNameTags = YlCommon.SelectMastersByName<TTag>(aContext, aMaster.Name);
						Debug.Assert(aSameNameTags.Count <= 1, "OpenEditTagWindow() same name exists");
						aMaster.Environment = Environment;
						aMaster.AvoidSameName = false;
					}
					return aMaster;
				}
			}
		}


	}
	// public class EditTagsWindowViewModel ___END___
}
// namespace YukaLister.ViewModels ___END___
