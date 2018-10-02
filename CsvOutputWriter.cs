// ============================================================================
// 
// CSV リスト出力クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace YukaLister.Shared
{
	public class CsvOutputWriter : OutputWriter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public CsvOutputWriter()
		{
			// プロパティー
			FormatName = "CSV";
			TopFileName = "List.csv";
			OutputSettings = new CsvOutputSettings();
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定画面を有効化するかどうか
		// --------------------------------------------------------------------
		public override Boolean IsDialogEnabled()
		{
			return true;
		}

		// --------------------------------------------------------------------
		// リスト出力
		// --------------------------------------------------------------------
		public override void Output()
		{
			StringBuilder aSB = new StringBuilder();
			PrepareOutput();

			// ヘッダー
			aSB.Append("No.");
			foreach (OutputItems aOutputItem in mRuntimeOutputItems)
			{
				switch (aOutputItem)
				{
					case OutputItems.SmartTrack:
						aSB.Append(",On,Off");
						break;
					case OutputItems.LastWriteTime:
						aSB.Append(",最終更新日,最終更新時刻");
						break;
					default:
						aSB.Append("," + YlCommon.OUTPUT_ITEM_NAMES[(Int32)aOutputItem]);
						break;
				}
			}
			aSB.Append("\n");

			IQueryable<TFound> aQueryResult =
					from x in TableFound
					orderby x.Category, x.Head, x.TieUpRuby, x.TieUpName, x.SongRuby, x.SongName
					select x;

			// コンテンツ
			Int32 aIndex = 1;
			foreach (TFound aTFound in aQueryResult)
			{
				aSB.Append(aIndex.ToString());

				foreach (OutputItems aOutputItem in mRuntimeOutputItems)
				{
					switch (aOutputItem)
					{
						case OutputItems.Path:
							aSB.Append(",\"" + aTFound.Path + "\"");
							break;
						case OutputItems.FileName:
							aSB.Append(",\"" + Path.GetFileName(aTFound.Path) + "\"");
							break;
						case OutputItems.Head:
							aSB.Append(",\"" + aTFound.Head + "\"");
							break;
						case OutputItems.Worker:
							aSB.Append(",\"" + aTFound.Worker + "\"");
							break;
						case OutputItems.Track:
							aSB.Append(",\"" + aTFound.Track + "\"");
							break;
						case OutputItems.SmartTrack:
							aSB.Append(",\"" + (aTFound.SmartTrackOnVocal ? YlCommon.SMART_TRACK_VALID_MARK : null) + "\"");
							aSB.Append(",\"" + (aTFound.SmartTrackOffVocal ? YlCommon.SMART_TRACK_VALID_MARK : null) + "\"");
							break;
						case OutputItems.Comment:
							aSB.Append(",\"" + aTFound.Comment + "\"");
							break;
						case OutputItems.LastWriteTime:
							aSB.Append("," + JulianDay.ModifiedJulianDateToDateTime(aTFound.LastWriteTime).ToString(YlCommon.DATE_FORMAT));
							aSB.Append("," + JulianDay.ModifiedJulianDateToDateTime(aTFound.LastWriteTime).ToString(YlCommon.TIME_FORMAT));
							break;
						case OutputItems.FileSize:
							aSB.Append("," + aTFound.FileSize);
							break;
						case OutputItems.SongName:
							aSB.Append(",\"" + aTFound.SongName + "\"");
							break;
						case OutputItems.SongRuby:
							aSB.Append(",\"" + aTFound.SongRuby + "\"");
							break;
						case OutputItems.SongOpEd:
							aSB.Append(",\"" + aTFound.SongOpEd + "\"");
							break;
						case OutputItems.SongReleaseDate:
							if (aTFound.SongReleaseDate <= YlCommon.INVALID_MJD)
							{
								aSB.Append(",");
							}
							else
							{
								aSB.Append("," + JulianDay.ModifiedJulianDateToDateTime(aTFound.SongReleaseDate).ToString(YlCommon.DATE_FORMAT));
							}
							break;
						case OutputItems.ArtistName:
							aSB.Append(",\"" + aTFound.ArtistName + "\"");
							break;
						case OutputItems.ArtistRuby:
							aSB.Append(",\"" + aTFound.ArtistRuby + "\"");
							break;
						case OutputItems.LyristName:
							aSB.Append(",\"" + aTFound.LyristName + "\"");
							break;
						case OutputItems.LyristRuby:
							aSB.Append(",\"" + aTFound.LyristRuby + "\"");
							break;
						case OutputItems.ComposerName:
							aSB.Append(",\"" + aTFound.ComposerName + "\"");
							break;
						case OutputItems.ComposerRuby:
							aSB.Append(",\"" + aTFound.ComposerRuby + "\"");
							break;
						case OutputItems.ArrangerName:
							aSB.Append(",\"" + aTFound.ArrangerName + "\"");
							break;
						case OutputItems.ArrangerRuby:
							aSB.Append(",\"" + aTFound.ArrangerRuby + "\"");
							break;
						case OutputItems.TieUpName:
							aSB.Append(",\"" + aTFound.TieUpName + "\"");
							break;
						case OutputItems.TieUpRuby:
							aSB.Append(",\"" + aTFound.TieUpRuby + "\"");
							break;
						case OutputItems.TieUpAgeLimit:
							aSB.Append(",\"" + aTFound.TieUpAgeLimit + "\"");
							break;
						case OutputItems.Category:
							aSB.Append(",\"" + aTFound.Category + "\"");
							break;
						case OutputItems.TieUpGroupName:
							aSB.Append(",\"" + aTFound.TieUpGroupName + "\"");
							break;
						case OutputItems.TieUpGroupRuby:
							aSB.Append(",\"" + aTFound.TieUpGroupRuby + "\"");
							break;
						case OutputItems.MakerName:
							aSB.Append(",\"" + aTFound.MakerName + "\"");
							break;
						case OutputItems.MakerRuby:
							aSB.Append(",\"" + aTFound.MakerRuby + "\"");
							break;
						default:
							Debug.Assert(false, "Output() bad aOutputItem");
							break;
					}

				}
				aSB.Append("\n");

				aIndex++;

			}
			File.WriteAllText(FolderPath + TopFileName, aSB.ToString(), Encoding.UTF8);
		}
	}
	// public class CsvOutputWriter ___END___

}
// namespace YukaLister.Shared ___END___
