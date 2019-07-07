// ============================================================================
// 
// TFound の項目を埋めるが、エイリアスは固定
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;

namespace YukaLister.Models.Database
{
	public class TFoundSetterAliasSpecify : TFoundSetter
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// oMusicInfoDbInDisk はインスタンス存在期間中は存在している前提
		// --------------------------------------------------------------------
		public TFoundSetterAliasSpecify(MusicInfoDatabaseInDisk oMusicInfoDbInDisk) : base(oMusicInfoDbInDisk)
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 元のタイアップ名
		public String SpecifiedProgramOrigin { get; set; }

		// 元の楽曲名
		public String SpecifiedSongOrigin { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 指定された元のタイアップ名を返す
		// --------------------------------------------------------------------
		public override String ProgramOrigin(String oAlias)
		{
			if (!String.IsNullOrEmpty(SpecifiedProgramOrigin))
			{
				return SpecifiedProgramOrigin;
			}

			return oAlias;
		}

		// --------------------------------------------------------------------
		// 指定された元の楽曲名を返す
		// --------------------------------------------------------------------
		public override String SongOrigin(String oAlias)
		{
			if (!String.IsNullOrEmpty(SpecifiedSongOrigin))
			{
				return SpecifiedSongOrigin;
			}

			return oAlias;
		}
	}
	// public class TFoundSetterAliasSpecify ___END___
}
// namespace YukaLister.Models.Database ___END___
