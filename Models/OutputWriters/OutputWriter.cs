// ============================================================================
// 
// リスト出力用基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Reflection;

using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;
using YukaLister.ViewModels;

namespace YukaLister.Models.OutputWriters
{
	public abstract class OutputWriter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		protected OutputWriter(EnvironmentModel oEnvironment)
		{
			mEnvironment = oEnvironment;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 出力形式（表示用）
		public String FormatName { get; protected set; }

		// 出力先フォルダー（末尾 '\\' 付き）
		private String mFolderPath;
		public String FolderPath
		{
			get
			{
				return mFolderPath;
			}
			set
			{
				mFolderPath = value;
				if (!String.IsNullOrEmpty(mFolderPath) && mFolderPath[mFolderPath.Length - 1] != '\\')
				{
					mFolderPath += '\\';
				}
			}
		}

		// 出力先インデックスファイル名（パス無し）
		public String TopFileName { get; protected set; }

		// 検索結果テーブル
		public Table<TFound> TableFound { get; set; }

		// タグテーブル
		public Table<TTag> TableTag { get; set; }

		// タグ紐付テーブル
		public Table<TTagSequence> TableTagSequence { get; set; }

		// 出力設定
		public OutputSettings OutputSettings { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リスト出力設定ウィンドウの ViewModel を生成
		// --------------------------------------------------------------------
		public virtual OutputSettingsWindowViewModel CreateOutputSettingsWindowViewModel()
		{
			return new OutputSettingsWindowViewModel();
		}

		// --------------------------------------------------------------------
		// リスト出力
		// --------------------------------------------------------------------
		public abstract void Output();

		// ====================================================================
		// protected 定数
		// ====================================================================

		// ====================================================================
		// protected メンバー変数
		// ====================================================================

		// 環境設定類
		protected EnvironmentModel mEnvironment;

		// 実際の出力項目
		protected List<OutputItems> mRuntimeOutputItems;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// テンプレート読み込み
		// --------------------------------------------------------------------
		protected String LoadTemplate(String oFileNameBody)
		{
			return File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + YlConstants.FOLDER_NAME_TEMPLATES
					+ oFileNameBody + Common.FILE_EXT_TPL);
		}

		// --------------------------------------------------------------------
		// コンストラクターでは行えない準備などを実施
		// --------------------------------------------------------------------
		protected virtual void PrepareOutput()
		{
			// OutputSettings.OutputAllItems に基づく設定（コンストラクターでは OutputSettings がロードされていない）
			mRuntimeOutputItems = OutputSettings.RuntimeOutputItems();
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ====================================================================
		// private メンバー関数
		// ====================================================================

	}
	// public class OutputWriter ___END___

}
// namespace YukaLister.Models.OutputWriters ___END___