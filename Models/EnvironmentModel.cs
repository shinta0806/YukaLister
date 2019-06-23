// ============================================================================
// 
// 環境設定類を管理する
// 
// ============================================================================

using Livet;

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using YukaLister.Models.Database;
using YukaLister.Models.SharedMisc;

namespace YukaLister.Models
{
	public class EnvironmentModel : NotificationObject
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public EnvironmentModel()
		{
			// 最初にログの設定をする
			SetLogWriter();

			// その他の設定
			SetYukaListerSettings();
			SetExLenEnabled();

			// カレントフォルダー正規化（ゆかりから起動された場合はゆかりのフォルダーになっているため）
			Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			// 環境の変化に対応
			DoVerChangedIfNeeded();
			LaunchUpdaterIfNeeded();
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ログ
		public LogWriter LogWriter { get; private set; }

		// ユーザー設定
		public YukaListerSettings YukaListerSettings { get; private set; }

		// extended-length なパス表記を使用するかどうか
		public Boolean IsExLenEnabled { get; private set; }

		// アプリケーション終了時タスク安全中断用
		public CancellationTokenSource AppCancellationTokenSource { get; private set; } = new CancellationTokenSource();

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// extended-length なパス表記でない場合は extended-length なパス表記に変換
		// MAX_PATH (=260) 文字以上のパスを扱えるようにする
		// IsExLenEnabled == true の場合のみ変換する
		// --------------------------------------------------------------------
		public String ExtendPath(String oPath)
		{
			if (String.IsNullOrEmpty(oPath))
			{
				return null;
			}

			if (!IsExLenEnabled || oPath.StartsWith(YlCommon.EXTENDED_LENGTH_PATH_PREFIX))
			{
				return oPath;
			}

			// MAX_PATH 文字以上のフォルダー名をダイアログから取得した場合など、短いファイル名形式になっていることがあるため、
			// Path.GetFullPath() で長いファイル名形式に変換する
			return YlCommon.EXTENDED_LENGTH_PATH_PREFIX + Path.GetFullPath(oPath);
		}

		// --------------------------------------------------------------------
		// 終了処理
		// --------------------------------------------------------------------
		public void Quit()
		{
			// 終了時の状態
			YukaListerSettings.PrevLaunchPath = Assembly.GetEntryAssembly().Location;
			YukaListerSettings.PrevLaunchGeneration = YlCommon.APP_GENERATION;
			YukaListerSettings.PrevLaunchVer = YlCommon.APP_VER;
			YukaListerSettings.Save();

			// テンポラリーフォルダー削除
			try
			{
				Directory.Delete(YlCommon.TempPath(), true);
			}
			catch
			{
			}

			LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "終了しました：" + YlCommon.APP_NAME_J + " "
						+ YlCommon.APP_VER + " --------------------");
		}

		// --------------------------------------------------------------------
		// extended-length でないパス表記に戻す
		// EXTENDED_LENGTH_PATH_PREFIX を除去するだけなので、長さは MAX_PATH を超えることもありえる
		// --------------------------------------------------------------------
		public String ShortenPath(String oPath)
		{
			if (String.IsNullOrEmpty(oPath))
			{
				return null;
			}

			if (!oPath.StartsWith(YlCommon.EXTENDED_LENGTH_PATH_PREFIX))
			{
				return oPath;
			}

			return oPath.Substring(YlCommon.EXTENDED_LENGTH_PATH_PREFIX.Length);
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// カテゴリーマスターテーブルの既定レコードを挿入
		// 旧バージョンのゆかりすたーを使用していてレコードが不足している場合用
		// --------------------------------------------------------------------
		private void AddMusicInfoDbCategoryDefaultRecordsIfNeeded()
		{
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(this))
			using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
			{
				Table<TCategory> aTableCategory = aContext.GetTable<TCategory>();

				if (YlCommon.SelectMastersByName<TCategory>(aMusicInfoDbInDisk.Connection, "一般").Count == 0)
				{
					aTableCategory.InsertOnSubmit(aMusicInfoDbInDisk.CreateCategoryRecord(103, "一般", "イッパン"));
				}

				aContext.SubmitChanges();
			}
		}

		// --------------------------------------------------------------------
		// バージョン更新時の処理
		// --------------------------------------------------------------------
		private void DoVerChangedIfNeeded()
		{
			// 更新起動時とパス変更時の記録
			// 新規起動時は、両フラグが立つのでダブらないように注意
			Boolean aVerChanged = YukaListerSettings.PrevLaunchVer != YlCommon.APP_VER;
			if (aVerChanged)
			{
				// ユーザーにメッセージ表示する前にログしておく
				if (String.IsNullOrEmpty(YukaListerSettings.PrevLaunchVer))
				{
					LogWriter.LogMessage(TraceEventType.Information, "新規起動：" + YlCommon.APP_VER);
				}
				else
				{
					LogWriter.LogMessage(TraceEventType.Information, "更新起動：" + YukaListerSettings.PrevLaunchVer + "→" + YlCommon.APP_VER);
				}
			}
			Boolean aPathChanged = (String.Compare(YukaListerSettings.PrevLaunchPath, Assembly.GetEntryAssembly().Location, true) != 0);
			if (aPathChanged && !String.IsNullOrEmpty(YukaListerSettings.PrevLaunchPath))
			{
				LogWriter.LogMessage(TraceEventType.Information, "パス変更起動：" + YukaListerSettings.PrevLaunchPath + "→" + Assembly.GetEntryAssembly().Location);
			}

			// 更新起動時とパス変更時の処理
			if (aVerChanged || aPathChanged)
			{
				YlCommon.LogEnvironmentInfo(LogWriter);
			}
			if (aVerChanged)
			{
				NewVersionLaunched();
				AddMusicInfoDbCategoryDefaultRecordsIfNeeded();
			}

		}

		// --------------------------------------------------------------------
		// 新バージョンで初回起動された時の処理を行う
		// --------------------------------------------------------------------
		private void LaunchUpdaterIfNeeded()
		{
			if (!YukaListerSettings.IsCheckRssNeeded())
			{
				return;
			}

			UpdaterLauncher aUpdaterLauncher = YlCommon.CreateUpdaterLauncher(true, false, false, false, LogWriter);
			if (aUpdaterLauncher.Launch(aUpdaterLauncher.ForceShow))
			{
				YukaListerSettings.RssCheckDate = DateTime.Now.Date;
				YukaListerSettings.Save();
			}
		}

		// --------------------------------------------------------------------
		// 新バージョンで初回起動された時の処理を行う
		// --------------------------------------------------------------------
		private void NewVersionLaunched()
		{
			String aNewVerMsg;

			// α・β警告、ならびに、更新時のメッセージ（2017/01/09）
			// 新規・更新のご挨拶
			if (String.IsNullOrEmpty(YukaListerSettings.PrevLaunchVer))
			{
				// 新規
				aNewVerMsg = "【初回起動】\n\n";
				aNewVerMsg += YlCommon.APP_NAME_J + "をダウンロードしていただき、ありがとうございます。";
			}
			else
			{
				aNewVerMsg = "【更新起動】\n\n";
				aNewVerMsg += YlCommon.APP_NAME_J + "を更新していただき、ありがとうございます。\n";
				aNewVerMsg += "更新内容については［ヘルプ→改訂履歴］メニューをご参照ください。";
			}

			// α・βの注意
			if (YlCommon.APP_VER.IndexOf("α") >= 0)
			{
				aNewVerMsg += "\n\nこのバージョンは開発途上のアルファバージョンです。\n"
						+ "使用前にヘルプをよく読み、注意してお使い下さい。";
			}
			else if (YlCommon.APP_VER.IndexOf("β") >= 0)
			{
				aNewVerMsg += "\n\nこのバージョンは開発途上のベータバージョンです。\n"
						+ "使用前にヘルプをよく読み、注意してお使い下さい。";
			}

			// 表示
			LogWriter.ShowLogMessage(TraceEventType.Information, aNewVerMsg);

			// Zone ID 削除
			Common.DeleteZoneID(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), SearchOption.AllDirectories);
		}

		// --------------------------------------------------------------------
		// IsExLenEnabled の設定
		// --------------------------------------------------------------------
		private void SetExLenEnabled()
		{
			// .NET 4.6.2 以降のみ有効とする
			SystemEnvironment aSystemEnvironment = new SystemEnvironment();
			Int32 aClrVer;
			aSystemEnvironment.GetClrVersionRegistryNumber(out aClrVer);
			IsExLenEnabled = (aClrVer >= 394802);
			LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "extended-length パスの使用：" + IsExLenEnabled.ToString());
		}

		// --------------------------------------------------------------------
		// LogWriter の設定
		// --------------------------------------------------------------------
		private void SetLogWriter()
		{
			// 頻度はまれだが、インポート時に大量のログが発生することがあるので、ファイルサイズを大きくしておく
			LogWriter = new LogWriter(YlCommon.APP_ID);
			LogWriter.ApplicationQuitToken = AppCancellationTokenSource.Token;
			LogWriter.SimpleTraceListener.MaxSize = 5 * 1024 * 1024;
			LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "起動しました：" + YlCommon.APP_NAME_J + " "
					+ YlCommon.APP_VER + " ====================");
#if DEBUG
			LogWriter.ShowLogMessage(TraceEventType.Verbose, "デバッグモード：" + Common.DEBUG_ENABLED_MARK);
#endif
		}

		// --------------------------------------------------------------------
		// YukaListerSettings の設定
		// --------------------------------------------------------------------
		private void SetYukaListerSettings()
		{
			YukaListerSettings = new YukaListerSettings();
			YukaListerSettings.Reload();
			if (YukaListerSettings.TargetExts == null)
			{
				YukaListerSettings.TargetExts = new List<String>();
			}
			if (YukaListerSettings.TargetExts.Count == 0)
			{
				// 動画をアルファベット順に追加（比較的メジャーで現在もサポートが行われている形式のみ）
				YukaListerSettings.TargetExts.Add(Common.FILE_EXT_AVI);
				YukaListerSettings.TargetExts.Add(Common.FILE_EXT_MKV);
				YukaListerSettings.TargetExts.Add(Common.FILE_EXT_MOV);
				YukaListerSettings.TargetExts.Add(Common.FILE_EXT_MP4);
				YukaListerSettings.TargetExts.Add(Common.FILE_EXT_MPG);
				YukaListerSettings.TargetExts.Add(Common.FILE_EXT_WMV);
			}
			if (YukaListerSettings.LastIdNumbers == null)
			{
				YukaListerSettings.LastIdNumbers = new List<Int32>();
			}
			if (YukaListerSettings.LastIdNumbers.Count < (Int32)MusicInfoDbTables.__End__)
			{
				YukaListerSettings.LastIdNumbers.Clear();
				for (Int32 i = 0; i < (Int32)MusicInfoDbTables.__End__; i++)
				{
					YukaListerSettings.LastIdNumbers.Add(0);
				}
			}
			YukaListerSettings.AnalyzeYukariEasyAuthConfig();

		}

	}
	// public class EnvironmentModel ___END___
}
// namespace YukaLister.Models ___END___
