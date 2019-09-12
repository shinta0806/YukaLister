// ============================================================================
// 
// ゆかりすたー共通で使用する関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Hnx8.ReadJEnc;

using Livet;
using Livet.Messaging;

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using YukaLister.Models.Database;
using YukaLister.Models.OutputWriters;
using YukaLister.ViewModels;

namespace YukaLister.Models.SharedMisc
{
	public class YlCommon
	{
		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コンテキストメニューにアイテムを追加
		// ToDo: MVVM 的なやり方でのコンテキストメニューへのコマンド登録方法が分からなかったのでこの方法としている
		// List<ViewModelCommand> をバインドするとコマンドの制御はできるが、表示文字列の制御ができない
		// --------------------------------------------------------------------
		public static void AddContextMenuItem(List<MenuItem> oItems, String oLabel, RoutedEventHandler oClick)
		{
			MenuItem aMenuItem = new MenuItem();
			aMenuItem.Header = oLabel;
			aMenuItem.Click += oClick;
			oItems.Add(aMenuItem);
		}

		// --------------------------------------------------------------------
		// ID 接頭辞の正当性を確認
		// ＜返値＞ 正規化後の ID 接頭辞
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public static String CheckIdPrefix(String oIdPrefix, Boolean oIsNullable)
		{
			oIdPrefix = NormalizeDbString(oIdPrefix);

			if (oIsNullable && String.IsNullOrEmpty(oIdPrefix))
			{
				return null;
			}

			if (String.IsNullOrEmpty(oIdPrefix))
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞を入力して下さい。");
			}
			if (oIdPrefix.Length > ID_PREFIX_MAX_LENGTH)
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞は " + ID_PREFIX_MAX_LENGTH + "文字以下にして下さい。");
			}
			if (oIdPrefix.IndexOf('_') >= 0)
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞に \"_\"（アンダースコア）は使えません。");
			}
			if (oIdPrefix.IndexOf(',') >= 0)
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞に \",\"（カンマ）は使えません。");
			}

			// 以下は Ver 2.14 で追加した条件のため、既存データには存在する可能性があることに注意
			if (oIdPrefix.IndexOf(' ') >= 0)
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞に \" \"（スペース）は使えません。");
			}
			if (oIdPrefix.IndexOf('"') >= 0)
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞に \"\"\"（ダブルクオート）は使えません。");
			}
			if (oIdPrefix.IndexOf('\\') >= 0)
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞に \"\\\"（円マーク）は使えません。");
			}

			// 以下は Ver 2.99 で追加した条件のため、既存データには存在する可能性があることに注意（システムが使うための文字）
			if (oIdPrefix.IndexOf('!') >= 0)
			{
				throw new Exception("各種 ID の先頭に付与する接頭辞に \"!\"（エクスクラメーション）は使えません。");
			}


			return oIdPrefix;
		}

		// --------------------------------------------------------------------
		// 番組分類統合用マップを作成
		// --------------------------------------------------------------------
		public static Dictionary<String, String> CreateCategoryUnityMap()
		{
			Dictionary<String, String> aMap = new Dictionary<String, String>();

			aMap["Webアニメーション"] = "アニメ";
			aMap["オリジナルビデオアニメーション"] = "アニメ";
			aMap["テレビアニメーション"] = "アニメ";
			aMap["劇場用アニメーション"] = "アニメ";
			aMap["Webラジオ"] = "ラジオ";
			aMap["Web特撮"] = "特撮";
			aMap["オリジナル特撮ビデオ"] = "特撮";
			aMap["テレビ特撮"] = "特撮";
			aMap["テレビ特撮スペシャル"] = "特撮";
			aMap["劇場用特撮"] = "特撮";

			return aMap;
		}

		// --------------------------------------------------------------------
		// 設定ファイルのルールを動作時用に変換
		// --------------------------------------------------------------------
		public static FolderSettingsInMemory CreateFolderSettingsInMemory(FolderSettingsInDisk oFolderSettingsInDisk)
		{
			FolderSettingsInMemory aFolderSettingsInMemory = new FolderSettingsInMemory();
			String aRule;
			List<String> aGroups;

			// フォルダー命名規則を辞書に格納
			foreach (String aInDisk in oFolderSettingsInDisk.FolderNameRules)
			{
				Int32 aEqualPos = aInDisk.IndexOf('=');
				if (aEqualPos < 2)
				{
					continue;
				}
				if (aInDisk[0] != YlConstants.RULE_VAR_BEGIN[0])
				{
					continue;
				}
				if (aInDisk[aEqualPos - 1] != YlConstants.RULE_VAR_END[0])
				{
					continue;
				}

				aFolderSettingsInMemory.FolderNameRules[aInDisk.Substring(1, aEqualPos - 2).ToLower()] = aInDisk.Substring(aEqualPos + 1);
			}

			// ファイル命名規則を正規表現に変換
			for (Int32 i = 0; i < oFolderSettingsInDisk.FileNameRules.Count; i++)
			{
				// ワイルドカードのみ <> で囲まれていないので、処理をやりやすくするために <> で囲む
				String aFileNameRule = oFolderSettingsInDisk.FileNameRules[i].Replace(YlConstants.RULE_VAR_ANY,
						YlConstants.RULE_VAR_BEGIN + YlConstants.RULE_VAR_ANY + YlConstants.RULE_VAR_END);

				MakeRegexPattern(aFileNameRule, out aRule, out aGroups);
				aFolderSettingsInMemory.FileNameRules.Add(aRule);
				aFolderSettingsInMemory.FileRegexGroups.Add(aGroups);
			}

			return aFolderSettingsInMemory;
		}

		// --------------------------------------------------------------------
		// アプリ独自の変数を格納する変数を生成し、定義済みキーをすべて初期化（キーには <> は含まない）
		// ・キーが無いと LINQ で例外が発生することがあるため
		// ・キーの有無と値の null の 2 度チェックは面倒くさいため
		// --------------------------------------------------------------------
		public static Dictionary<String, String> CreateRuleDictionary()
		{
			Dictionary<String, String> aVarMapWith = CreateRuleDictionaryWithDescription();
			Dictionary<String, String> aVarMap = new Dictionary<String, String>();

			foreach (String aKey in aVarMapWith.Keys)
			{
				aVarMap[aKey] = null;
			}

			return aVarMap;
		}

		// --------------------------------------------------------------------
		// アプリ独自の変数とその説明
		// --------------------------------------------------------------------
		public static Dictionary<String, String> CreateRuleDictionaryWithDescription()
		{
			Dictionary<String, String> aVarMap = new Dictionary<String, String>();

			// タイアップマスターにも同様の項目があるもの
			aVarMap[YlConstants.RULE_VAR_CATEGORY] = "カテゴリー";
			aVarMap[YlConstants.RULE_VAR_PROGRAM] = "タイアップ名";
			aVarMap[YlConstants.RULE_VAR_AGE_LIMIT] = "年齢制限";

			// 楽曲マスターにも同様の項目があるもの
			aVarMap[YlConstants.RULE_VAR_OP_ED] = "摘要（OP/ED 別）";
			aVarMap[YlConstants.RULE_VAR_TITLE] = "楽曲名";
			aVarMap[YlConstants.RULE_VAR_TITLE_RUBY] = "楽曲名フリガナ";
			aVarMap[YlConstants.RULE_VAR_ARTIST] = "歌手名";

			// ファイル名からのみ取得可能なもの
			aVarMap[YlConstants.RULE_VAR_WORKER] = "ニコカラ制作者";
			aVarMap[YlConstants.RULE_VAR_TRACK] = "トラック情報";
			aVarMap[YlConstants.RULE_VAR_ON_VOCAL] = "オンボーカルトラック";
			aVarMap[YlConstants.RULE_VAR_OFF_VOCAL] = "オフボーカルトラック";
			aVarMap[YlConstants.RULE_VAR_COMMENT] = "備考";

			// 楽曲マスターにも同様の項目があるもの
			aVarMap[YlConstants.RULE_VAR_TAG] = "タグ";

			// その他
			aVarMap[YlConstants.RULE_VAR_ANY] = "無視する部分";

			return aVarMap;
		}

		// --------------------------------------------------------------------
		// 紐付テーブルのレコードを作成
		// --------------------------------------------------------------------
		public static T CreateSequenceRecord<T>(String oId, Int32 oSequence, String oLinkId, Boolean oIsImport = false) where T : IRcSequence, new()
		{
			return new T
			{
				// IDbBase
				Id = oId,
				Import = false,
				Invalid = false,
				UpdateTime = YlConstants.INVALID_MJD,
				Dirty = true,

				// IDbSequence
				Sequence = oSequence,
				LinkId = oLinkId,
			};
		}

		// --------------------------------------------------------------------
		// ちょちょいと自動更新起動を作成
		// --------------------------------------------------------------------
		public static UpdaterLauncher CreateUpdaterLauncher(Boolean oCheckLatest, Boolean oForceShow, Boolean oClearUpdateCache, Boolean oForceInstall, LogWriter oLogWriter)
		{
			// 固定部分
			UpdaterLauncher aUpdaterLauncher = new UpdaterLauncher();
			aUpdaterLauncher.ID = YlConstants.APP_ID;
			aUpdaterLauncher.Name = YlConstants.APP_NAME_J;
			aUpdaterLauncher.Wait = 3;
			aUpdaterLauncher.UpdateRss = "http://shinta.coresv.com/soft/YukaListerMeteor_AutoUpdate.xml";
			aUpdaterLauncher.CurrentVer = YlConstants.APP_VER;

			// 変動部分
			if (oCheckLatest)
			{
				aUpdaterLauncher.LatestRss = "http://shinta.coresv.com/soft/YukaListerMeteor_JPN.xml";
			}
			aUpdaterLauncher.LogWriter = oLogWriter;
			aUpdaterLauncher.ForceShow = oForceShow;
			aUpdaterLauncher.NotifyHWnd = IntPtr.Zero;
			aUpdaterLauncher.ClearUpdateCache = oClearUpdateCache;
			aUpdaterLauncher.ForceInstall = oForceInstall;

			// 起動
			return aUpdaterLauncher;
		}

		// --------------------------------------------------------------------
		// 暗号化して Base64 になっている文字列を復号化する
		// --------------------------------------------------------------------
		public static String Decrypt(String oBase64Text)
		{
			if (String.IsNullOrEmpty(oBase64Text))
			{
				return null;
			}

			Byte[] aCipherBytes = Convert.FromBase64String(oBase64Text);

			using (AesManaged aAes = new AesManaged())
			{
				using (ICryptoTransform aDecryptor = aAes.CreateDecryptor(ENCRYPT_KEY, ENCRYPT_IV))
				{
					using (MemoryStream aWriteStream = new MemoryStream())
					{
						// 復号化
						using (CryptoStream aCryptoStream = new CryptoStream(aWriteStream, aDecryptor, CryptoStreamMode.Write))
						{
							aCryptoStream.Write(aCipherBytes, 0, aCipherBytes.Length);
						}

						// 文字列化
						Byte[] aPlainBytes = aWriteStream.ToArray();
						return Encoding.Unicode.GetString(aPlainBytes);
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 指定されたフォルダーの除外設定有無
		// --------------------------------------------------------------------
		public static FolderExcludeSettingsStatus DetectFolderExcludeSettingsStatus(String oFolderExLen)
		{
			String aFolderExcludeSettingsFolder = FindExcludeSettingsFolder2Ex(oFolderExLen);
			if (String.IsNullOrEmpty(aFolderExcludeSettingsFolder))
			{
				return FolderExcludeSettingsStatus.False;
			}
			else
			{
				return FolderExcludeSettingsStatus.True;
			}
		}

		// --------------------------------------------------------------------
		// 指定されたフォルダーの設定有無
		// --------------------------------------------------------------------
		public static FolderSettingsStatus DetectFolderSettingsStatus2Ex(String oFolderExLen)
		{
			String aFolderSettingsFolder = FindSettingsFolder2Ex(oFolderExLen);
			if (String.IsNullOrEmpty(aFolderSettingsFolder))
			{
				return FolderSettingsStatus.None;
			}
			else if (IsSamePath(oFolderExLen, aFolderSettingsFolder))
			{
				return FolderSettingsStatus.Set;
			}
			else
			{
				return FolderSettingsStatus.Inherit;
			}
		}

		// --------------------------------------------------------------------
		// ファイル命名規則とフォルダー固定値を適用した情報を得る
		// --------------------------------------------------------------------
		public static Dictionary<String, String> DicByFile(String oPathExLen)
		{
			FolderSettingsInDisk aFolderSettingsInDisk = LoadFolderSettings2Ex(Path.GetDirectoryName(oPathExLen));
			FolderSettingsInMemory aFolderSettingsInMemory = CreateFolderSettingsInMemory(aFolderSettingsInDisk);
			return YlCommon.MatchFileNameRulesAndFolderRule(Path.GetFileNameWithoutExtension(oPathExLen), aFolderSettingsInMemory);
		}

		// --------------------------------------------------------------------
		// CsvEncoding から Encoding を得る
		// --------------------------------------------------------------------
		public static Encoding EncodingFromCsvEncoding(CsvEncoding oCsvEncoding)
		{
			Encoding aEncoding = null;
			switch (oCsvEncoding)
			{
				case CsvEncoding.ShiftJis:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_SHIFT_JIS);
					break;
				case CsvEncoding.Jis:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_JIS);
					break;
				case CsvEncoding.EucJp:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_EUC_JP);
					break;
				case CsvEncoding.Utf16Le:
					aEncoding = Encoding.Unicode;
					break;
				case CsvEncoding.Utf16Be:
					aEncoding = Encoding.GetEncoding(Common.CODE_PAGE_UTF_16_BE);
					break;
				case CsvEncoding.Utf8:
					aEncoding = Encoding.UTF8;
					break;
				default:
					Debug.Assert(false, "EncodingFromCsvEncoding() bad csv encoding");
					break;
			}
			return aEncoding;
		}

		// --------------------------------------------------------------------
		// 文字列を AES 256 bit 暗号化して Base64 で返す
		// --------------------------------------------------------------------
		public static String Encrypt(String oPlainText)
		{
			if (String.IsNullOrEmpty(oPlainText))
			{
				return null;
			}

			Byte[] aPlainBytes = Encoding.Unicode.GetBytes(oPlainText);

			using (AesManaged aAes = new AesManaged())
			{
				using (ICryptoTransform aEncryptor = aAes.CreateEncryptor(ENCRYPT_KEY, ENCRYPT_IV))
				{
					using (MemoryStream aWriteStream = new MemoryStream())
					{
						// 暗号化
						using (CryptoStream aCryptoStream = new CryptoStream(aWriteStream, aEncryptor, CryptoStreamMode.Write))
						{
							aCryptoStream.Write(aPlainBytes, 0, aPlainBytes.Length);
						}

						// Base64
						Byte[] aCipherBytes = aWriteStream.ToArray();
						return Convert.ToBase64String(aCipherBytes);
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 指定されたフォルダーのフォルダー除外設定ファイルがあるフォルダーを返す
		// --------------------------------------------------------------------
		public static String FindExcludeSettingsFolder2Ex(String oFolderExLen)
		{
			while (!String.IsNullOrEmpty(oFolderExLen))
			{
				if (File.Exists(oFolderExLen + "\\" + YlConstants.FILE_NAME_YUKA_LISTER_EXCLUDE_CONFIG))
				{
					return oFolderExLen;
				}
				oFolderExLen = Path.GetDirectoryName(oFolderExLen);
			}
			return null;
		}

		// --------------------------------------------------------------------
		// 指定されたフォルダーのフォルダー設定ファイルがあるフォルダーを返す
		// 互換性維持のため、ニコカラりすたーの設定ファイルも扱う
		// --------------------------------------------------------------------
		public static String FindSettingsFolder2Ex(String oFolderExLen)
		{
			while (!String.IsNullOrEmpty(oFolderExLen))
			{
				if (File.Exists(oFolderExLen + "\\" + YlConstants.FILE_NAME_YUKA_LISTER_CONFIG))
				{
					return oFolderExLen;
				}
				if (File.Exists(oFolderExLen + "\\" + YlConstants.FILE_NAME_NICO_KARA_LISTER_CONFIG))
				{
					return oFolderExLen;
				}
				oFolderExLen = Path.GetDirectoryName(oFolderExLen);
			}
			return null;
		}

		// --------------------------------------------------------------------
		// 頭文字を返す
		// ひらがな（濁点なし）、その他、のいずれか
		// --------------------------------------------------------------------
		public static String Head(String oString)
		{
			if (String.IsNullOrEmpty(oString))
			{
				return YlConstants.HEAD_MISC;
			}

			Char aChar = oString[0];

			// カタカナをひらがなに変換
			if ('ァ' <= aChar && aChar <= 'ヶ')
			{
				aChar = (Char)(aChar - 0x0060);
			}

			// 濁点・小文字をノーマルに変換
			Int32 aHeadConvertPos = HEAD_CONVERT_FROM.IndexOf(aChar);
			if (aHeadConvertPos >= 0)
			{
				aChar = HEAD_CONVERT_TO[aHeadConvertPos];
			}

			// ひらがなを返す
			if ('あ' <= aChar && aChar <= 'ん')
			{
				return new string(aChar, 1);
			}

			return YlConstants.HEAD_MISC;
		}

		// --------------------------------------------------------------------
		// ID 接頭辞が未設定ならばユーザーに入力してもらう
		// ＜例外＞ OperationCanceledException
		// --------------------------------------------------------------------
		public static void InputIdPrefixIfNeededWithInvoke(ViewModel oViewModel, EnvironmentModel oEnvironment)
		{
			if (!String.IsNullOrEmpty(oEnvironment.YukaListerSettings.IdPrefix))
			{
				return;
			}

			Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				using (InputIdPrefixWindowViewModel aInputIdPrefixWindowViewModel = new InputIdPrefixWindowViewModel())
				{
					aInputIdPrefixWindowViewModel.Environment = oEnvironment;
					oViewModel.Messenger.Raise(new TransitionMessage(aInputIdPrefixWindowViewModel, "OpenInputIdPrefixWindow"));
				}
			}));

			if (String.IsNullOrEmpty(oEnvironment.YukaListerSettings.IdPrefix))
			{
				throw new OperationCanceledException();
			}
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcAlias）
		// --------------------------------------------------------------------
		public static Boolean IsRcAliasUpdated(IRcAlias oExistRecord, IRcAlias oNewRecord)
		{
			Boolean? aIsRcBaseUpdated = IsRcBaseUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcBaseUpdated != null)
			{
				return aIsRcBaseUpdated.Value;
			}

			return oExistRecord.Alias != oNewRecord.Alias
					|| oExistRecord.OriginalId != oNewRecord.OriginalId;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcMaster）
		// --------------------------------------------------------------------
		public static Boolean IsRcMasterUpdated(IRcMaster oExistRecord, IRcMaster oNewRecord)
		{
			return IsRcMasterUpdatedCore(oExistRecord, oNewRecord) ?? false;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcSequence）
		// --------------------------------------------------------------------
		public static Boolean IsRcSequenceUpdated(IRcSequence oExistRecord, IRcSequence oNewRecord)
		{
			Boolean? aIsRcBaseUpdated = IsRcBaseUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcBaseUpdated != null)
			{
				return aIsRcBaseUpdated.Value;
			}

			return oExistRecord.LinkId != oNewRecord.LinkId;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TSong）
		// --------------------------------------------------------------------
		public static Boolean IsRcSongUpdated(TSong oExistRecord, TSong oNewRecord)
		{
			Boolean? aIsRcCategorizableUpdated = IsRcCategorizableUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcCategorizableUpdated != null)
			{
				return aIsRcCategorizableUpdated.Value;
			}

			return oExistRecord.TieUpId != oNewRecord.TieUpId
					|| oExistRecord.OpEd != oNewRecord.OpEd;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（TTieUp）
		// --------------------------------------------------------------------
		public static Boolean IsRcTieUpUpdated(TTieUp oExistRecord, TTieUp oNewRecord)
		{
			Boolean? aIsRcCategorizableUpdated = IsRcCategorizableUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcCategorizableUpdated != null)
			{
				return aIsRcCategorizableUpdated.Value;
			}

			return oExistRecord.MakerId != oNewRecord.MakerId
					|| oExistRecord.AgeLimit != oNewRecord.AgeLimit;
		}

		// --------------------------------------------------------------------
		// 同一のファイル・フォルダーかどうか
		// 末尾の '\\' 有無や大文字小文字にかかわらず比較する
		// いずれかが null の場合は false とする
		// パスは extended-length でもそうでなくても可
		// --------------------------------------------------------------------
		public static Boolean IsSamePath(String oPath1, String oPath2)
		{
			if (String.IsNullOrEmpty(oPath1) || String.IsNullOrEmpty(oPath2))
			{
				return false;
			}

			// 末尾の '\\' を除去
			if (oPath1[oPath1.Length - 1] == '\\')
			{
				oPath1 = oPath1.Substring(0, oPath1.Length - 1);
			}
			if (oPath2[oPath2.Length - 1] == '\\')
			{
				oPath2 = oPath2.Substring(0, oPath2.Length - 1);
			}
			return (oPath1.ToLower() == oPath2.ToLower());
		}

		// --------------------------------------------------------------------
		// 関数を非同期駆動
		// --------------------------------------------------------------------
		public static Task LaunchTaskAsync<T>(TaskAsyncDelegate<T> oDelegate, Object oTaskLock, T oVar, LogWriter oLogWriter)
		{
			return Task.Run(() =>
			{
				try
				{
					// 終了時に強制終了されないように設定
					Thread.CurrentThread.IsBackground = false;

					lock (oTaskLock)
					{
						// 関数処理
						oLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バックグラウンド処理開始：" + oDelegate.Method.Name);
						oDelegate(oVar);
#if DEBUGz
						Thread.Sleep(5000);
#endif
						oLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "バックグラウンド処理終了：" + oDelegate.Method.Name);
					}
				}
				catch (Exception oExcep)
				{
					oLogWriter.ShowLogMessage(TraceEventType.Error, "バックグラウンド処理 " + oDelegate.Method.Name + "実行時エラー：\n" + oExcep.Message);
					oLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + oExcep.StackTrace);
				}
			});
		}

		// --------------------------------------------------------------------
		// 環境設定の文字コードに従って CSV ファイルを読み込む
		// 下処理も行う
		// oNumColumns: 行番号も含めた列数
		// --------------------------------------------------------------------
		public static List<List<String>> LoadCsv(String oPath, EnvironmentModel oEnvironment, Int32 oNumColumns)
		{
			List<List<String>> aCsv;

			try
			{
				Encoding aEncoding;
				if (oEnvironment.YukaListerSettings.CsvEncoding == CsvEncoding.AutoDetect)
				{
					// 文字コード自動判別
					FileInfo aFileInfo = new FileInfo(oPath);
					using (FileReader aReader = new FileReader(aFileInfo))
					{
						aEncoding = aReader.Read(aFileInfo).GetEncoding();
					}
				}
				else
				{
					aEncoding = EncodingFromCsvEncoding(oEnvironment.YukaListerSettings.CsvEncoding);
				}
				if (aEncoding == null)
				{
					throw new Exception("文字コードを判定できませんでした。");
				}
				aCsv = CsvManager.LoadCsv(oPath, aEncoding, true, true);

				// 規定列数に満たない行を削除
				for (Int32 i = aCsv.Count - 1; i >= 0; i--)
				{
					if (aCsv[i].Count != oNumColumns)
					{
						oEnvironment.LogWriter.ShowLogMessage(TraceEventType.Warning,
								(Int32.Parse(aCsv[i][0]) + 2).ToString("#,0") + " 行目は項目数の過不足があるため無視します。", true);
						aCsv.RemoveAt(i);
					}
				}

				// 空白削除
				for (Int32 i = 0; i < aCsv.Count; i++)
				{
					List<String> aRecord = aCsv[i];
					for (Int32 j = 0; j < aRecord.Count; j++)
					{
						aRecord[j] = aRecord[j].Trim();
					}
				}
			}
			catch (Exception oExcep)
			{
				aCsv = new List<List<String>>();
				oEnvironment.LogWriter.ShowLogMessage(TraceEventType.Warning, "CSV ファイルを読み込めませんでした。\n" + oExcep.Message + "\n" + oPath, true);
			}
			return aCsv;
		}

		// --------------------------------------------------------------------
		// LINQ の FirstOrDefault() の代わり
		// IQueryable<IRcBase>.FirstOrDefault() が実行時エラーになるため
		// --------------------------------------------------------------------
		public static T FirstOrDefault<T>(IQueryable<T> oQueryResult) where T : class
		{
			foreach (T aRecord in oQueryResult)
			{
				return aRecord;
			}
			return null;
		}

		// --------------------------------------------------------------------
		// フォルダー設定を読み込む
		// FILE_NAME_YUKA_LISTER_CONFIG 優先、無い場合は FILE_NAME_NICO_KARA_LISTER_CONFIG
		// 見つからない場合は null ではなく空のインスタンスを返す
		// --------------------------------------------------------------------
		public static FolderSettingsInDisk LoadFolderSettings2Ex(String oFolderExLen)
		{
			FolderSettingsInDisk aFolderSettings = new FolderSettingsInDisk();
			try
			{
				String aFolderSettingsFolder = FindSettingsFolder2Ex(oFolderExLen);
				if (!String.IsNullOrEmpty(aFolderSettingsFolder))
				{
					if (File.Exists(aFolderSettingsFolder + "\\" + YlConstants.FILE_NAME_YUKA_LISTER_CONFIG))
					{
						aFolderSettings = Common.Deserialize<FolderSettingsInDisk>(aFolderSettingsFolder + "\\" + YlConstants.FILE_NAME_YUKA_LISTER_CONFIG);
					}
					else
					{
						aFolderSettings = Common.Deserialize<FolderSettingsInDisk>(aFolderSettingsFolder + "\\" + YlConstants.FILE_NAME_NICO_KARA_LISTER_CONFIG);
					}
				}
			}
			catch (Exception)
			{
			}

			// 項目が null の場合はインスタンスを作成
			if (aFolderSettings.FileNameRules == null)
			{
				aFolderSettings.FileNameRules = new List<String>();
			}
			if (aFolderSettings.FolderNameRules == null)
			{
				aFolderSettings.FolderNameRules = new List<String>();
			}

			return aFolderSettings;
		}

		// --------------------------------------------------------------------
		// 環境情報をログする
		// --------------------------------------------------------------------
		public static void LogEnvironmentInfo(LogWriter oLogWriter)
		{
			SystemEnvironment aSE = new SystemEnvironment();
			aSE.LogEnvironment(oLogWriter);
		}

		// --------------------------------------------------------------------
		// ファイル名とファイル命名規則がマッチするか確認し、マッチしたマップを返す
		// ＜引数＞ oFileNameBody: 拡張子無し
		// --------------------------------------------------------------------
		public static Dictionary<String, String> MatchFileNameRules(String oFileNameBody, FolderSettingsInMemory oFolderSettingsInMemory)
		{
			Dictionary<String, String> aDic = CreateRuleDictionary();
			Match aMatch = null;
			Int32 aMatchIndex = -1;

			// ファイル名と合致する命名規則を探す
			for (Int32 i = 0; i < oFolderSettingsInMemory.FileNameRules.Count; i++)
			{
				aMatch = Regex.Match(oFileNameBody, oFolderSettingsInMemory.FileNameRules[i], RegexOptions.None);
				if (aMatch.Success)
				{
					aMatchIndex = i;
					break;
				}
			}
			if (aMatchIndex < 0)
			{
				return aDic;
			}

			for (Int32 i = 0; i < oFolderSettingsInMemory.FileRegexGroups[aMatchIndex].Count; i++)
			{
				// 定義されているキーのみ格納する
				if (aDic.ContainsKey(oFolderSettingsInMemory.FileRegexGroups[aMatchIndex][i]))
				{
					// aMatch.Groups[0] にはマッチした全体の値が入っているので無視し、[1] から実際の値が入っている
					if (String.IsNullOrEmpty(aDic[oFolderSettingsInMemory.FileRegexGroups[aMatchIndex][i]]))
					{
						aDic[oFolderSettingsInMemory.FileRegexGroups[aMatchIndex][i]] = aMatch.Groups[i + 1].Value.Trim();
					}
					else
					{
						aDic[oFolderSettingsInMemory.FileRegexGroups[aMatchIndex][i]] += YlConstants.VAR_VALUE_DELIMITER + aMatch.Groups[i + 1].Value.Trim();
					}
				}
			}

			// 正規化
			aDic[YlConstants.RULE_VAR_CATEGORY] = NormalizeDbString(aDic[YlConstants.RULE_VAR_CATEGORY]);
			aDic[YlConstants.RULE_VAR_PROGRAM] = NormalizeDbString(aDic[YlConstants.RULE_VAR_PROGRAM]);
			aDic[YlConstants.RULE_VAR_AGE_LIMIT] = NormalizeDbString(aDic[YlConstants.RULE_VAR_AGE_LIMIT]);
			aDic[YlConstants.RULE_VAR_OP_ED] = NormalizeDbString(aDic[YlConstants.RULE_VAR_OP_ED]);
			aDic[YlConstants.RULE_VAR_TITLE] = NormalizeDbString(aDic[YlConstants.RULE_VAR_TITLE]);
			aDic[YlConstants.RULE_VAR_TITLE_RUBY] = NormalizeDbRuby(aDic[YlConstants.RULE_VAR_TITLE_RUBY]);
			aDic[YlConstants.RULE_VAR_ARTIST] = NormalizeDbString(aDic[YlConstants.RULE_VAR_ARTIST]);
			aDic[YlConstants.RULE_VAR_WORKER] = NormalizeDbString(aDic[YlConstants.RULE_VAR_WORKER]);
			aDic[YlConstants.RULE_VAR_TRACK] = NormalizeDbString(aDic[YlConstants.RULE_VAR_TRACK]);
			aDic[YlConstants.RULE_VAR_COMMENT] = NormalizeDbString(aDic[YlConstants.RULE_VAR_COMMENT]);

			return aDic;
		}

		// --------------------------------------------------------------------
		// ファイル名とファイル命名規則・フォルダー固定値がマッチするか確認し、マッチしたマップを返す
		// ＜引数＞ oFileNameBody: 拡張子無し
		// --------------------------------------------------------------------
		public static Dictionary<String, String> MatchFileNameRulesAndFolderRule(String oFileNameBody, FolderSettingsInMemory oFolderSettingsInMemory)
		{
			// ファイル名命名規則
			Dictionary<String, String> aDic = YlCommon.MatchFileNameRules(oFileNameBody, oFolderSettingsInMemory);

			// フォルダー命名規則をマージ
			foreach (KeyValuePair<String, String> aFolderRule in oFolderSettingsInMemory.FolderNameRules)
			{
				if (aDic.ContainsKey(aFolderRule.Key) && String.IsNullOrEmpty(aDic[aFolderRule.Key]))
				{
					aDic[aFolderRule.Key] = aFolderRule.Value;
				}
			}

			return aDic;
		}

		// --------------------------------------------------------------------
		// 日付に合わせて年月日文字列を設定
		// --------------------------------------------------------------------
		public static void MjdToStrings(Double oMjd, out String oYear, out String oMonth, out String oDay)
		{
			if (oMjd <= YlConstants.INVALID_MJD)
			{
				oYear = null;
				oMonth = null;
				oDay = null;
			}
			else
			{
				DateTime aReleaseDate = JulianDay.ModifiedJulianDateToDateTime(oMjd);
				oYear = aReleaseDate.Year.ToString();
				oMonth = aReleaseDate.Month.ToString();
				oDay = aReleaseDate.Day.ToString();
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースに登録するフリガナの表記揺れを減らす
		// ＜返値＞ フリガナ表記 or null（空になる場合）
		// --------------------------------------------------------------------
		public static String NormalizeDbRuby(String oString)
		{
			Debug.Assert(NORMALIZE_DB_RUBY_FROM.Length == NORMALIZE_DB_RUBY_TO.Length, "NormalizeDbRuby() different NORMALIZE_DB_FURIGANA_FROM NORMALIZE_DB_FURIGANA_TO length");

			if (String.IsNullOrEmpty(oString))
			{
				return null;
			}

			StringBuilder aKatakana = new StringBuilder();

			for (Int32 i = 0; i < oString.Length; i++)
			{
				Char aChar = oString[i];

				// 小文字・半角カタカナ等を全角カタカナに変換
				Int32 aPos = NORMALIZE_DB_RUBY_FROM.IndexOf(aChar);
				if (aPos >= 0)
				{
					aKatakana.Append(NORMALIZE_DB_RUBY_TO[aPos]);
					continue;
				}

				// 上記以外の全角カタカナ・音引きはそのまま
				if ('ア' <= aChar && aChar <= 'ン' || aChar == 'ー')
				{
					aKatakana.Append(aChar);
					continue;
				}

				// 上記以外のひらがなをカタカナに変換
				if ('あ' <= aChar && aChar <= 'ん')
				{
					aKatakana.Append((Char)(aChar + 0x60));
					continue;
				}

				// その他の文字は無視する
			}

			String aKatakanaString = aKatakana.ToString();
			if (String.IsNullOrEmpty(aKatakanaString))
			{
				return null;
			}

			return aKatakanaString;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースに登録する文字列の表記揺れを減らす
		// 半角チルダ・波ダッシュは全角チルダに変換する（波ダッシュとして全角チルダが用いられているため）
		// ＜返値＞ 正規化後表記 or null（空になる場合）
		// --------------------------------------------------------------------
		public static String NormalizeDbString(String oString)
		{
			Debug.Assert(NORMALIZE_DB_STRING_FROM.Length == NORMALIZE_DB_STRING_TO.Length, "NormalizeDbString() different NORMALIZE_DB_STRING_FROM NORMALIZE_DB_STRING_TO length");

			if (String.IsNullOrEmpty(oString))
			{
				return null;
			}

			StringBuilder aNormalized = new StringBuilder();

			for (Int32 i = 0; i < oString.Length; i++)
			{
				Char aChar = oString[i];

				// 一部記号・全角英数を半角に変換
				if ('！' <= aChar && aChar <= '｝')
				{
					aNormalized.Append((Char)(aChar - 0xFEE0));
					continue;
				}

				// テーブルによる変換
				Int32 aPos = NORMALIZE_DB_STRING_FROM.IndexOf(aChar);
				if (aPos >= 0)
				{
					aNormalized.Append(NORMALIZE_DB_STRING_TO[aPos]);
					continue;
				}

				// 変換なし
				aNormalized.Append(aChar);
			}

			String aNormalizedString = aNormalized.ToString().Trim();
			if (String.IsNullOrEmpty(aNormalizedString))
			{
				return null;
			}

			return aNormalizedString;
		}

		// --------------------------------------------------------------------
		// 空文字列を null に変換する
		// --------------------------------------------------------------------
		public static String NullIfEmpty(String oString)
		{
			if (String.IsNullOrEmpty(oString))
			{
				return null;
			}
			return oString;
		}

		// --------------------------------------------------------------------
		// リスト出力
		// ＜引数＞ oOutputWriter: FolderPath は設定済みの前提
		// --------------------------------------------------------------------
		public static void OutputList(OutputWriter oOutputWriter, EnvironmentModel oEnvironment, YukariListDatabaseInMemory oYukariListDbInMemory)
		{
			oOutputWriter.OutputSettings.Load();
			using (DataContext aYukariDbContext = new DataContext(oYukariListDbInMemory.Connection))
			{
				oOutputWriter.TableFound = aYukariDbContext.GetTable<TFound>();
				oOutputWriter.TablePerson = aYukariDbContext.GetTable<TPerson>();
				oOutputWriter.TableArtistSequence = aYukariDbContext.GetTable<TArtistSequence>();
				oOutputWriter.TableComposerSequence = aYukariDbContext.GetTable<TComposerSequence>();
				oOutputWriter.TableTag = aYukariDbContext.GetTable<TTag>();
				oOutputWriter.TableTagSequence = aYukariDbContext.GetTable<TTagSequence>();
				oOutputWriter.Output();
			}
		}

		// --------------------------------------------------------------------
		// 紐付テーブルに新規登録または更新
		// --------------------------------------------------------------------
		public static void RegisterSequence<T>(DataContext oContext, String oId, List<String> oLinkIds, Boolean oIsImport = false) where T : class, IRcSequence, new()
		{
			String aTableName = LinqUtils.TableName(typeof(T));

			// 新規レコード
			List<T> aNewSequences = new List<T>();
			for (Int32 i = 0; i < oLinkIds.Count; i++)
			{
				T aNewSequence = CreateSequenceRecord<T>(oId, i, oLinkIds[i], oIsImport);
				aNewSequences.Add(aNewSequence);
			}

			// 既存レコード
			List<T> aExistSequences = SelectSequencesById<T>(oContext, oId, true);

			// 既存レコードがインポートではなく新規レコードがインポートの場合は更新しない
			if (aExistSequences.Count > 0 && !aExistSequences[0].Import
					&& aNewSequences.Count > 0 && aNewSequences[0].Import)
			{
				return;
			}

			// 既存レコードがある場合は更新
			for (Int32 i = 0; i < Math.Min(aNewSequences.Count, aExistSequences.Count); i++)
			{
				if (YlCommon.IsRcSequenceUpdated(aExistSequences[i], aNewSequences[i]))
				{
					aNewSequences[i].UpdateTime = aExistSequences[i].UpdateTime;
					Common.ShallowCopy(aNewSequences[i], aExistSequences[i]);
					if (!oIsImport)
					{
						// ToDo: ログ
						//LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aTableName + " 紐付テーブル更新：" + oId + " / " + i.ToString());
					}
				}
			}

			// 既存レコードがない部分は新規登録
			Table<T> aTableSequence = oContext.GetTable<T>();
			for (Int32 i = aExistSequences.Count; i < aNewSequences.Count; i++)
			{
				aTableSequence.InsertOnSubmit(aNewSequences[i]);
				if (!oIsImport)
				{
					//LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aTableName + " 紐付テーブル新規登録：" + oId + " / " + i.ToString());
				}
			}

			// 既存レコードが余る部分は無効化
			for (Int32 i = aNewSequences.Count; i < aExistSequences.Count; i++)
			{
				if (!aExistSequences[i].Invalid)
				{
					aExistSequences[i].Invalid = true;
					aExistSequences[i].Dirty = true;
					if (!oIsImport)
					{
						//LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, aTableName + " 紐付テーブル無効化：" + oId + " / " + i.ToString());
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから別名を検索
		// --------------------------------------------------------------------
		public static List<T> SelectAliasesByAlias<T>(DataContext oContext, String oAlias, Boolean oIncludesInvalid = false) where T : class, IRcAlias
		{
			if (String.IsNullOrEmpty(oAlias))
			{
				return new List<T>();
			}

			Table<T> aTableAlias = oContext.GetTable<T>();
			IQueryable<T> aQueryResult =
					from x in aTableAlias
					where x.Alias == oAlias && (oIncludesInvalid ? true : x.Invalid == false)
					select x;
			return aQueryResult.ToList();
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから別名を検索
		// --------------------------------------------------------------------
		public static List<T> SelectAliasesByAlias<T>(SQLiteConnection oConnection, String oAlias, Boolean oIncludesInvalid = false) where T : class, IRcAlias
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectAliasesByAlias<T>(aContext, oAlias, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから IRcBase を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static T SelectBaseById<T>(DataContext oContext, String oId, Boolean oIncludesInvalid = false) where T : class, IRcBase
		{
			if (String.IsNullOrEmpty(oId))
			{
				return null;
			}

			Table<T> aTableMaster = oContext.GetTable<T>();
			return aTableMaster.SingleOrDefault(x => x.Id == oId && (oIncludesInvalid ? true : x.Invalid == false));
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから IRcMaster を検索
		// 見つからない場合は null
		// --------------------------------------------------------------------
		public static T SelectBaseById<T>(SQLiteConnection oConnection, String oId, Boolean oIncludesInvalid = false) where T : class, IRcBase
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectBaseById<T>(aContext, oId, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからカテゴリーを列挙
		// --------------------------------------------------------------------
		public static List<String> SelectCategoryNames(SQLiteConnection oConnection, Boolean oIncludesInvalid = false)
		{
			List<String> aCategoryNames = new List<String>();
			using (DataContext aContext = new DataContext(oConnection))
			{
				Table<TCategory> aTableCategory = aContext.GetTable<TCategory>();
				IQueryable<TCategory> aQueryResultCategory =
						from x in aTableCategory
						where oIncludesInvalid ? true : x.Invalid == false
						select x;
				foreach (TCategory aCategory in aQueryResultCategory)
				{
					aCategoryNames.Add(aCategory.Name);
				}
			}
			return aCategoryNames;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから IRcMaster をすべて検索
		// --------------------------------------------------------------------
		public static List<T> SelectMastersByName<T>(DataContext oContext, String oName, Boolean oIncludesInvalid = false) where T : class, IRcMaster
		{
			if (String.IsNullOrEmpty(oName))
			{
				return new List<T>();
			}

			Table<T> aTableMaster = oContext.GetTable<T>();
			IQueryable<T> aQueryResult =
					from x in aTableMaster
					where x.Name == oName && (oIncludesInvalid ? true : x.Invalid == false)
					select x;
			return aQueryResult.ToList();
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから IRcMaster をすべて検索
		// --------------------------------------------------------------------
		public static List<T> SelectMastersByName<T>(SQLiteConnection oConnection, String oName, Boolean oIncludesInvalid = false) where T : class, IRcMaster
		{
			using (DataContext aContext = new DataContext(oConnection))
			{
				return SelectMastersByName<T>(aContext, oName, oIncludesInvalid);
			}
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲に紐付く人物を検索
		// oIncludesInvalid が true の場合、無効 ID に null を紐付ける
		// --------------------------------------------------------------------
		public static List<TPerson> SelectSequencePeopleBySongId<T>(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false) where T : class, IRcSequence
		{
			List<T> aSequences = SelectSequencesById<T>(oContext, oSongId, oIncludesInvalid);
			List<TPerson> aPeople = new List<TPerson>();

			foreach (T aSequence in aSequences)
			{
				TPerson aPerson = SelectBaseById<TPerson>(oContext, aSequence.LinkId);
				if (aPerson != null || oIncludesInvalid)
				{
					aPeople.Add(aPerson);
				}
			}

			return aPeople;
		}

		// --------------------------------------------------------------------
		// 紐付データベースから紐付を検索
		// --------------------------------------------------------------------
		public static List<T> SelectSequencesById<T>(DataContext oContext, String oId, Boolean oIncludesInvalid = false) where T : class, IRcSequence
		{
			List<T> aSequences = new List<T>();

			if (!String.IsNullOrEmpty(oId))
			{
				Table<T> aTableSequence = oContext.GetTable<T>();
				IQueryable<T> aQueryResult =
						from x in aTableSequence
						where x.Id == oId && (oIncludesInvalid ? true : x.Invalid == false)
						orderby x.Sequence
						select x;
				aSequences = aQueryResult.ToList();
			}

			return aSequences;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースから楽曲に紐付くタグを検索
		// oIncludesInvalid が true の場合、無効 ID に null を紐付ける
		// --------------------------------------------------------------------
		public static List<TTag> SelectSequenceTagsBySongId(DataContext oContext, String oSongId, Boolean oIncludesInvalid = false)
		{
			List<TTagSequence> aSequences = SelectSequencesById<TTagSequence>(oContext, oSongId, oIncludesInvalid);
			List<TTag> aTags = new List<TTag>();

			foreach (TTagSequence aSequence in aSequences)
			{
				TTag aTag = SelectBaseById<TTag>(oContext, aSequence.LinkId);
				if (aTag != null || oIncludesInvalid)
				{
					aTags.Add(aTag);
				}
			}

			return aTags;
		}

		// --------------------------------------------------------------------
		// 楽曲情報データベースからタイアップに紐付くタイアップグループを検索
		// oIncludesInvalid が true の場合、無効 ID に null を紐付ける
		// --------------------------------------------------------------------
		public static List<TTieUpGroup> SelectSequenceTieUpGroupsByTieUpId(DataContext oContext, String oTieUpId, Boolean oIncludesInvalid = false)
		{
			List<TTieUpGroupSequence> aSequences = SelectSequencesById<TTieUpGroupSequence>(oContext, oTieUpId, oIncludesInvalid);
			List<TTieUpGroup> aTieUpGroups = new List<TTieUpGroup>();

			foreach (TTieUpGroupSequence aSequence in aSequences)
			{
				TTieUpGroup aTieUpGroup = SelectBaseById<TTieUpGroup>(oContext, aSequence.LinkId);
				if (aTieUpGroup != null || oIncludesInvalid)
				{
					aTieUpGroups.Add(aTieUpGroup);
				}
			}

			return aTieUpGroups;
		}

		// --------------------------------------------------------------------
		// カテゴリーメニューに値を設定
		// --------------------------------------------------------------------
		public static void SetContextMenuItemCategories(List<MenuItem> oMenuItems, RoutedEventHandler oClick, EnvironmentModel oEnvironment)
		{
			using (MusicInfoDatabaseInDisk aMusicInfoDbInDisk = new MusicInfoDatabaseInDisk(oEnvironment))
			{
				using (DataContext aContext = new DataContext(aMusicInfoDbInDisk.Connection))
				{
					Table<TCategory> aTableCategory = aContext.GetTable<TCategory>();
					IQueryable<TCategory> aQueryResult =
							from x in aTableCategory
							select x;
					foreach (TCategory aCategory in aQueryResult)
					{
						AddContextMenuItem(oMenuItems, aCategory.Name, oClick);
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 設定保存フォルダのパス（末尾 '\\'）
		// 存在しない場合は作成する
		// --------------------------------------------------------------------
		public static String SettingsPath()
		{
			String aPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify)
					+ "\\" + Common.FOLDER_NAME_SHINTA + YlConstants.FOLDER_NAME_YUKA_LISTER;

			if (!Directory.Exists(aPath))
			{
				Directory.CreateDirectory(aPath);
			}
			return aPath;
		}

		// --------------------------------------------------------------------
		// カンマ区切り ID をリストに分割
		// 引数が空の場合は null ではなく空リストを返す
		// --------------------------------------------------------------------
		public static List<String> SplitIds(String oIds)
		{
			List<String> aSplit = new List<String>();
			if (!String.IsNullOrEmpty(oIds))
			{
				aSplit.AddRange(oIds.Split(','));
			}
			return aSplit;
		}

		// --------------------------------------------------------------------
		// 年月日の文字列から日付を生成
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public static Double StringsToMjd(String oCaption, String oYearString, String oMonthString, String oDayString)
		{
			if (String.IsNullOrEmpty(oYearString))
			{
				// 年が入力されていない場合は、月日も空欄でなければならない
				if (!String.IsNullOrEmpty(oMonthString) || !String.IsNullOrEmpty(oDayString))
				{
					throw new Exception(oCaption + "の年が入力されていません。");
				}

				return YlConstants.INVALID_MJD;
			}

			// 年の確認
			Int32 aYear = Common.StringToInt32(oYearString);
			Int32 aNowYear = DateTime.Now.Year;
			if (aYear < 0)
			{
				throw new Exception(oCaption + "の年にマイナスの値を入力することはできません。");
			}
			if (aYear < 100)
			{
				// 2 桁の西暦を 4 桁に変換する
				if (aYear <= aNowYear % 100)
				{
					aYear += (aNowYear / 100) * 100;
				}
				else
				{
					aYear += (aNowYear / 100 - 1) * 100;
				}
			}
			if (aYear < 1000)
			{
				throw new Exception(oCaption + "の年に 3 桁の値を入力することはできません。");
			}
			if (aYear < YlConstants.INVALID_YEAR)
			{
				throw new Exception(oCaption + "の年は " + YlConstants.INVALID_YEAR + " 以上を入力して下さい。");
			}
			if (aYear > aNowYear)
			{
				throw new Exception(oCaption + "の年は " + aNowYear + " 以下を入力して下さい。");
			}

			// 月の確認
			if (String.IsNullOrEmpty(oMonthString) && !String.IsNullOrEmpty(oDayString))
			{
				// 年と日が入力されている場合は、月も入力されていなければならない
				throw new Exception(oCaption + "の月が入力されていません。");
			}
			Int32 aMonth;
			if (String.IsNullOrEmpty(oMonthString))
			{
				// 月が空欄の場合は 1 とする
				aMonth = 1;
			}
			else
			{
				aMonth = Common.StringToInt32(oMonthString);
				if (aMonth < 1 || aMonth > 12)
				{
					throw new Exception(oCaption + "の月は 1～12 を入力して下さい。");
				}
			}

			// 日の確認
			Int32 aDay;
			if (String.IsNullOrEmpty(oDayString))
			{
				// 日が空欄の場合は 1 とする
				aDay = 1;
			}
			else
			{
				aDay = Common.StringToInt32(oDayString);
				if (aDay < 1 || aDay > 31)
				{
					throw new Exception(oCaption + "の日は 1～31 を入力して下さい。");
				}
			}

			return JulianDay.DateTimeToModifiedJulianDate(new DateTime(aYear, aMonth, aDay));
		}

		// --------------------------------------------------------------------
		// テンポラリファイルのパス（呼びだす度に異なるファイル、拡張子なし）
		// --------------------------------------------------------------------
		public static String TempFilePath()
		{
			// マルチスレッドでも安全にインクリメント
			Int32 aCounter = Interlocked.Increment(ref smTempFilePathCounter);
			return TempPath() + aCounter.ToString() + "_" + Thread.CurrentThread.ManagedThreadId.ToString();
		}

		// --------------------------------------------------------------------
		// テンポラリフォルダのパス（末尾 '\\'）
		// 存在しない場合は作成する
		// --------------------------------------------------------------------
		public static String TempPath()
		{
			String aPath = Path.GetTempPath() + YlConstants.FOLDER_NAME_YUKA_LISTER + Process.GetCurrentProcess().Id.ToString() + "\\";
			if (!Directory.Exists(aPath))
			{
				try
				{
					Directory.CreateDirectory(aPath);
				}
				catch
				{
				}
			}
			return aPath;
		}


		// ====================================================================
		// private 定数
		// ====================================================================

		// --------------------------------------------------------------------
		// DB 変換
		// --------------------------------------------------------------------

		// NormalizeDbRuby() 用：フリガナ正規化対象文字（小文字・濁点のカナ等）
		private const String NORMALIZE_DB_RUBY_FROM = "ァィゥェォッャュョヮヵヶガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポヰヱヴヷヸヹヺｧｨｩｪｫｯｬｭｮ"
				+ "ぁぃぅぇぉっゃゅょゎゕゖがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽゐゑゔ" + NORMALIZE_DB_FORBIDDEN_FROM;
		private const String NORMALIZE_DB_RUBY_TO = "アイウエオツヤユヨワカケカキクケコサシスセソタチツテトハヒフヘホハヒフヘホイエウワイエヲアイウエオツヤユヨ"
				+ "アイウエオツヤユヨワカケカキクケコサシスセソタチツテトハヒフヘホハヒフヘホイエウ" + NORMALIZE_DB_FORBIDDEN_TO;

		// NormalizeDbString() 用：禁則文字（全角スペース、一部の半角文字等）
		private const String NORMALIZE_DB_STRING_FROM = "　\u2019ｧｨｩｪｫｯｬｭｮﾞﾟ｡｢｣､･~\u301C" + NORMALIZE_DB_FORBIDDEN_FROM;
		private const String NORMALIZE_DB_STRING_TO = " 'ァィゥェォッャュョ゛゜。「」、・～～" + NORMALIZE_DB_FORBIDDEN_TO;

		// NormalizeDbXXX() 用：変換後がフリガナ対象の禁則文字（半角カタカナ）
		private const String NORMALIZE_DB_FORBIDDEN_FROM = "ｦｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝ";
		private const String NORMALIZE_DB_FORBIDDEN_TO = "ヲーアイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワン";

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------

		// ID 接頭辞の最大長（同期サーバーデータベースの都合上、ID のトータル長が UTF-8 で 255 バイト以下になるようにする）
		private const Int32 ID_PREFIX_MAX_LENGTH = 20;

		// 暗号化キー（256 bit = 32 byte）
		private static readonly Byte[] ENCRYPT_KEY =
		{
			0x07, 0xC1, 0x19, 0x4A, 0x99, 0x9A, 0xF0, 0x2D, 0x0C, 0x52, 0xB0, 0x65, 0x48, 0xE6, 0x1F, 0x61,
			0x9C, 0x37, 0x9C, 0xA1, 0xC2, 0x31, 0xBA, 0xD1, 0x64, 0x1D, 0x85, 0x46, 0xCA, 0xF4, 0xE6, 0x5F,
		};

		// 暗号化 IV（128 bit = 16 byte）
		private static readonly Byte[] ENCRYPT_IV =
		{
			0x80, 0xB5, 0x40, 0x56, 0x9A, 0xE0, 0x3A, 0x9F, 0xd0, 0x90, 0xC6, 0x7C, 0xAA, 0xCD, 0xE7, 0x53,
		};

		// 頭文字変換用
		private const String HEAD_CONVERT_FROM = "ぁぃぅぇぉゕゖゃゅょゎゔがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽゐゑ";
		private const String HEAD_CONVERT_TO = "あいうえおかけやゆよわうかきくけこさしすせそたちつてとはひふへほはひふへほいえ";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// TempFilePath() 用カウンター（同じスレッドでもファイル名が分かれるようにするため）
		private static Int32 smTempFilePathCounter = 0;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// DB の中にテーブルを作成（汎用関数）
		// --------------------------------------------------------------------
		private static void CreateMusicInfoDbTable(SQLiteCommand oCmd, Type oTypeOfTable, String oIndexColumn = null)
		{
			List<String> aIndices;
			if (String.IsNullOrEmpty(oIndexColumn))
			{
				aIndices = null;
			}
			else
			{
				aIndices = new List<String>();
				aIndices.Add(oIndexColumn);
			}
			CreateMusicInfoDbTable(oCmd, oTypeOfTable, aIndices);
		}

		// --------------------------------------------------------------------
		// DB の中にテーブルを作成（汎用関数）
		// --------------------------------------------------------------------
		private static void CreateMusicInfoDbTable(SQLiteCommand oCmd, Type oTypeOfTable, List<String> oIndices)
		{
			// テーブル作成
			LinqUtils.CreateTable(oCmd, oTypeOfTable);

			// インデックス作成（JOIN および検索の高速化）
			LinqUtils.CreateIndex(oCmd, LinqUtils.TableName(oTypeOfTable), oIndices);
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcBase）
		// より派生型の IsRcXXXUpdated() から呼び出される前提
		// プライマリーキーは比較しない
		// ＜返値＞ true: 更新された, false: 更新されていない, null: より派生型での判断に委ねる
		// --------------------------------------------------------------------
		private static Boolean? IsRcBaseUpdatedCore(IRcBase oExistRecord, IRcBase oNewRecord)
		{
			if (!oExistRecord.Import && oNewRecord.Import)
			{
				// 既存レコードがゆかりすたー登録で新規レコードがインポートの場合は、ゆかりすたー登録した既存レコードを優先する
				return false;
			}

			if (oExistRecord.Invalid)
			{
				if (oNewRecord.Import)
				{
					// 既存レコードが無効の場合は、インポートでは無効解除しない
					return false;
				}

				// 既存レコードが無効の場合は、無効解除されるまでは更新しない、無効解除されたら更新された
				return !oNewRecord.Invalid;
			}

			// 派生型の内容が更新されたかどうかで判断すべき
			return null;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcCategorizable）
		// より派生型の IsRcXXXUpdated() から呼び出される前提
		// ＜返値＞ true: 更新された, false: 更新されていない, null: より派生型での判断に委ねる
		// --------------------------------------------------------------------
		private static Boolean? IsRcCategorizableUpdatedCore(IRcCategorizable oExistRecord, IRcCategorizable oNewRecord)
		{
			Boolean? aIsRcMasterUpdated = IsRcMasterUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcMasterUpdated != null)
			{
				return aIsRcMasterUpdated.Value;
			}

			// IRcCategorizable の要素が更新されていれば更新されたことが確定
			if (oExistRecord.CategoryId != oNewRecord.CategoryId
					|| oExistRecord.ReleaseDate != oNewRecord.ReleaseDate)
			{
				return true;
			}

			// 派生型の内容が更新されたかどうかで判断すべき
			return null;
		}

		// --------------------------------------------------------------------
		// レコードの内容が更新されたか（IRcMaster）
		// より派生型の IsRcXXXUpdated() から呼び出される前提
		// ＜返値＞ true: 更新された, false: 更新されていない, null: より派生型での判断に委ねる
		// --------------------------------------------------------------------
		private static Boolean? IsRcMasterUpdatedCore(IRcMaster oExistRecord, IRcMaster oNewRecord)
		{
			Boolean? aIsRcBaseUpdated = IsRcBaseUpdatedCore(oExistRecord, oNewRecord);
			if (aIsRcBaseUpdated != null)
			{
				return aIsRcBaseUpdated.Value;
			}

			// IRcMaster の要素が更新されていれば更新されたことが確定
			if (oExistRecord.Name != oNewRecord.Name
					|| oExistRecord.Ruby != oNewRecord.Ruby
					|| oExistRecord.Keyword != oNewRecord.Keyword)
			{
				return true;
			}

			// 派生型の内容が更新されたかどうかで判断すべき
			return null;
		}

		// --------------------------------------------------------------------
		// 設定ファイルのルール表記を正規表現に変換
		// --------------------------------------------------------------------
		private static void MakeRegexPattern(String oRuleInDisk, out String oRuleInMemory, out List<String> oGroups)
		{
			oGroups = new List<String>();

			// 元が空なら空で返す
			if (String.IsNullOrEmpty(oRuleInDisk))
			{
				oRuleInMemory = String.Empty;
				return;
			}

			StringBuilder aSB = new StringBuilder();
			aSB.Append("^");
			Int32 aBeginPos = 0;
			Int32 aEndPos;
			Boolean aLongestExists = false;
			while (aBeginPos < oRuleInDisk.Length)
			{
				if (oRuleInDisk[aBeginPos] == YlConstants.RULE_VAR_BEGIN[0])
				{
					// 変数を解析
					aEndPos = MakeRegexPatternFindVarEnd(oRuleInDisk, aBeginPos + 1);
					if (aEndPos < 0)
					{
						throw new Exception("命名規則の " + (aBeginPos + 1) + " 文字目の < に対応する > がありません。\n" + oRuleInDisk);
					}

					// 変数の <> は取り除く
					String aVarName = oRuleInDisk.Substring(aBeginPos + 1, aEndPos - aBeginPos - 1).ToLower();
					oGroups.Add(aVarName);

					// 番組名・楽曲名は区切り文字を含むこともあるため最長一致で検索する
					// また、最低 1 つは最長一致が無いとマッチしない
					if (aVarName == YlConstants.RULE_VAR_PROGRAM || aVarName == YlConstants.RULE_VAR_TITLE || !aLongestExists && aEndPos == oRuleInDisk.Length - 1)
					{
						aSB.Append("(.*)");
						aLongestExists = true;
					}
					else
					{
						aSB.Append("(.*?)");
					}

					aBeginPos = aEndPos + 1;
				}
				else if (@".$^{[(|)*+?\".IndexOf(oRuleInDisk[aBeginPos]) >= 0)
				{
					// エスケープが必要な文字
					aSB.Append('\\');
					aSB.Append(oRuleInDisk[aBeginPos]);
					aBeginPos++;
				}
				else
				{
					// そのまま追加
					aSB.Append(oRuleInDisk[aBeginPos]);
					aBeginPos++;
				}
			}
			aSB.Append("$");
			oRuleInMemory = aSB.ToString();
		}

		// --------------------------------------------------------------------
		// <Title> 等の開始 < に対する終了 > の位置を返す
		// ＜引数＞ oBeginPos：開始 < の次の位置
		// --------------------------------------------------------------------
		private static Int32 MakeRegexPatternFindVarEnd(String oString, Int32 oBeginPos)
		{
			while (oBeginPos < oString.Length)
			{
				if (oString[oBeginPos] == YlConstants.RULE_VAR_END[0])
				{
					return oBeginPos;
				}
				oBeginPos++;
			}
			return -1;
		}
	}
	// public class YlCommon ___END___

}
// namespace YukaLister.Shared ___END___