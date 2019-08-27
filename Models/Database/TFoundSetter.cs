// ============================================================================
// 
// TFound の項目を埋める
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YukaLister.Models.SharedMisc;

namespace YukaLister.Models.Database
{
	public class TFoundSetter : IDisposable
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// oMusicInfoDbInDisk はインスタンス存在期間中は存在している前提
		// --------------------------------------------------------------------
		public TFoundSetter(MusicInfoDatabaseInDisk oMusicInfoDbInDisk, DataContext oYukariDbContext = null)
		{
			mMusicInfoDbContext = new DataContext(oMusicInfoDbInDisk.Connection);
			mMusicInfoDbCmd = new SQLiteCommand(oMusicInfoDbInDisk.Connection);
			mCategoryNames = YlCommon.SelectCategoryNames(oMusicInfoDbInDisk.Connection);
			mYukariDbContext = oYukariDbContext;
		}

		// --------------------------------------------------------------------
		// デストラクター
		// --------------------------------------------------------------------
		~TFoundSetter()
		{
			Dispose(false);
		}

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// IDisposable.Dispose()
		// --------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// --------------------------------------------------------------------
		// 別名から元のタイアップ名を取得
		// mMusicInfoDbCmd を書き換えることに注意
		// DataContext で行うと 30 倍くらい遅くなる
		// --------------------------------------------------------------------
		public virtual String ProgramOrigin(String oAlias)
		{
			if (String.IsNullOrEmpty(oAlias))
			{
				return null;
			}

			mMusicInfoDbCmd.CommandText = "SELECT * FROM " + TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS + " LEFT OUTER JOIN " + TTieUp.TABLE_NAME_TIE_UP
					+ " ON " + TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS + "." + TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ORIGINAL_ID + " = " + TTieUp.TABLE_NAME_TIE_UP + "." + TTieUp.FIELD_NAME_TIE_UP_ID
					+ " WHERE " + TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS + "." + TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_ALIAS + " = @alias"
					+ " AND " + TTieUpAlias.TABLE_NAME_TIE_UP_ALIAS + "." + TTieUpAlias.FIELD_NAME_TIE_UP_ALIAS_INVALID + " = 0";
			mMusicInfoDbCmd.Parameters.Add(new SQLiteParameter("@alias", oAlias));

			using (SQLiteDataReader aReader = mMusicInfoDbCmd.ExecuteReader())
			{
				while (aReader.Read())
				{
					return aReader[TTieUp.FIELD_NAME_TIE_UP_NAME].ToString();
				}
			}

			return oAlias;
		}

		// --------------------------------------------------------------------
		// 検出ファイルレコードの値を、フォルダー設定や楽曲情報データベースから検索して設定する
		// oRecord.Path は事前に設定されている必要がある
		// --------------------------------------------------------------------
		public void SetTFoundValue(TFound oRecord, FolderSettingsInMemory oFolderSettingsInMemory)
		{
			// ファイル名・フォルダー固定値と合致する命名規則を探す
			Dictionary<String, String> aDicByFile = YlCommon.MatchFileNameRulesAndFolderRule(Path.GetFileNameWithoutExtension(oRecord.Path), oFolderSettingsInMemory);
			aDicByFile[YlConstants.RULE_VAR_PROGRAM] = ProgramOrigin(aDicByFile[YlConstants.RULE_VAR_PROGRAM]);
			aDicByFile[YlConstants.RULE_VAR_TITLE] = SongOrigin(aDicByFile[YlConstants.RULE_VAR_TITLE]);
			if (aDicByFile[YlConstants.RULE_VAR_CATEGORY] != null)
			{
				if (mCategoryNames.IndexOf(aDicByFile[YlConstants.RULE_VAR_CATEGORY]) < 0)
				{
					aDicByFile[YlConstants.RULE_VAR_CATEGORY] = null;
				}
			}

			// 楽曲情報データベースを適用
			SetTFoundValueByMusicInfoDb(oRecord, aDicByFile);

			// 楽曲情報データベースに無かった項目をファイル名・フォルダー固定値から取得
			oRecord.Category = oRecord.Category == null ? aDicByFile[YlConstants.RULE_VAR_CATEGORY] : oRecord.Category;
			oRecord.TieUpName = oRecord.TieUpName == null ? aDicByFile[YlConstants.RULE_VAR_PROGRAM] : oRecord.TieUpName;
			oRecord.TieUpAgeLimit = oRecord.TieUpAgeLimit == 0 ? Common.StringToInt32(aDicByFile[YlConstants.RULE_VAR_AGE_LIMIT]) : oRecord.TieUpAgeLimit;
			oRecord.SongOpEd = oRecord.SongOpEd == null ? aDicByFile[YlConstants.RULE_VAR_OP_ED] : oRecord.SongOpEd;
			oRecord.SongName = oRecord.SongName == null ? aDicByFile[YlConstants.RULE_VAR_TITLE] : oRecord.SongName;

			// SongId が無い場合は楽曲名を採用（フォルダー設定の歌手名やタグを紐付できるように）
			if (String.IsNullOrEmpty(oRecord.SongId))
			{
				oRecord.SongId = TEMP_ID_PREFIX + oRecord.SongName;
			}

			SetTFoundArtistByDic(oRecord, aDicByFile);
			oRecord.SongRuby = oRecord.SongRuby == null ? aDicByFile[YlConstants.RULE_VAR_TITLE_RUBY] : oRecord.SongRuby;
			oRecord.Worker = oRecord.Worker == null ? aDicByFile[YlConstants.RULE_VAR_WORKER] : oRecord.Worker;
			oRecord.Track = oRecord.Track == null ? aDicByFile[YlConstants.RULE_VAR_TRACK] : oRecord.Track;
			oRecord.SmartTrackOnVocal = !oRecord.SmartTrackOnVocal ? aDicByFile[YlConstants.RULE_VAR_ON_VOCAL] != null : oRecord.SmartTrackOnVocal;
			oRecord.SmartTrackOffVocal = !oRecord.SmartTrackOffVocal ? aDicByFile[YlConstants.RULE_VAR_OFF_VOCAL] != null : oRecord.SmartTrackOffVocal;
			oRecord.Comment = oRecord.Comment == null ? aDicByFile[YlConstants.RULE_VAR_COMMENT] : oRecord.Comment;

			// トラック情報からスマートトラック解析
			Boolean aHasOn;
			Boolean aHasOff;
			AnalyzeSmartTrack(oRecord.Track, out aHasOn, out aHasOff);
			oRecord.SmartTrackOnVocal |= aHasOn;
			oRecord.SmartTrackOffVocal |= aHasOff;

			// ルビが無い場合は漢字を採用
			if (String.IsNullOrEmpty(oRecord.TieUpRuby))
			{
				oRecord.TieUpRuby = oRecord.TieUpName;
			}
			if (String.IsNullOrEmpty(oRecord.SongRuby))
			{
				oRecord.SongRuby = oRecord.SongName;
			}

			// 頭文字
			if (!String.IsNullOrEmpty(oRecord.TieUpRuby))
			{
				oRecord.Head = YlCommon.Head(oRecord.TieUpRuby);
			}
			else
			{
				oRecord.Head = YlCommon.Head(oRecord.SongRuby);
			}

			// 番組名が無い場合は頭文字を採用（ボカロ曲等のリスト化用）
			if (String.IsNullOrEmpty(oRecord.TieUpName))
			{
				oRecord.TieUpName = oRecord.Head;
			}
		}

		// --------------------------------------------------------------------
		// 別名から元の楽曲名を取得
		// oInfoDbCmd を書き換えることに注意
		// --------------------------------------------------------------------
		public virtual String SongOrigin(String oAlias)
		{
			if (String.IsNullOrEmpty(oAlias))
			{
				return null;
			}

			mMusicInfoDbCmd.CommandText = "SELECT * FROM " + TSongAlias.TABLE_NAME_SONG_ALIAS + " LEFT OUTER JOIN " + TSong.TABLE_NAME_SONG
					+ " ON " + TSongAlias.TABLE_NAME_SONG_ALIAS + "." + TSongAlias.FIELD_NAME_SONG_ALIAS_ORIGINAL_ID + " = " + TSong.TABLE_NAME_SONG + "." + TSong.FIELD_NAME_SONG_ID
					+ " WHERE " + TSongAlias.TABLE_NAME_SONG_ALIAS + "." + TSongAlias.FIELD_NAME_SONG_ALIAS_ALIAS + " = @alias "
					+ " AND " + TSongAlias.TABLE_NAME_SONG_ALIAS + "." + TSongAlias.FIELD_NAME_SONG_ALIAS_INVALID + " = 0";
			mMusicInfoDbCmd.Parameters.Add(new SQLiteParameter("@alias", oAlias));

			using (SQLiteDataReader aReader = mMusicInfoDbCmd.ExecuteReader())
			{
				while (aReader.Read())
				{
					return aReader[TSong.FIELD_NAME_SONG_NAME].ToString();
				}
			}

			return oAlias;
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リソース解放
		// --------------------------------------------------------------------
		protected virtual void Dispose(Boolean oIsDisposing)
		{
			if (mIsDisposed)
			{
				return;
			}

			// マネージドリソース解放
			if (oIsDisposing)
			{
				DisposeManagedResource(mMusicInfoDbContext);
				DisposeManagedResource(mMusicInfoDbCmd);
			}

			// アンマネージドリソース解放
			// 今のところ無し

			mIsDisposed = true;
		}

		// --------------------------------------------------------------------
		// マネージドリソース解放
		// --------------------------------------------------------------------
		protected void DisposeManagedResource(IDisposable oResource)
		{
			if (oResource != null)
			{
				oResource.Dispose();
			}
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// スマートトラック判定用の単語（小文字表記、両端を | で括る）
		private const String OFF_VOCAL_WORDS = "|cho|cut|dam|guide|guidevocal|inst|joy|off|offcho|offvocal|offのみ|vc|オフ|オフボ|オフボーカル|ボイキャン|ボーカルキャンセル|配信|";
		private const String BOTH_VOCAL_WORDS = "|2tr|2ch|onoff|offon|";

		// 一時的に付与する ID の接頭辞
		private const String TEMP_ID_PREFIX = "!";

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// データベースアクセス（アロケート）
		private DataContext mMusicInfoDbContext;
		private SQLiteCommand mMusicInfoDbCmd;

		// データベースアクセス（参照）
		private DataContext mYukariDbContext;

		// カテゴリー正規化用
		List<String> mCategoryNames;

		// Dispose フラグ
		private Boolean mIsDisposed = false;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// トラック情報からオンボーカル・オフボーカルがあるか解析する
		// --------------------------------------------------------------------
		private void AnalyzeSmartTrack(String oTrack, out Boolean oHasOn, out Boolean oHasOff)
		{
			oHasOn = false;
			oHasOff = false;

			if (String.IsNullOrEmpty(oTrack))
			{
				return;
			}

			String[] aTracks = oTrack.Split(new Char[] { '-', '_', '+', ',', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			for (Int32 i = 0; i < aTracks.Length; i++)
			{
				Int32 aBothPos = BOTH_VOCAL_WORDS.IndexOf("|" + aTracks[i] + "|", StringComparison.OrdinalIgnoreCase);
				if (aBothPos >= 0)
				{
					oHasOff = true;
					oHasOn = true;
					return;
				}

				Int32 aOffPos = OFF_VOCAL_WORDS.IndexOf("|" + aTracks[i] + "|", StringComparison.OrdinalIgnoreCase);
				if (aOffPos >= 0)
				{
					oHasOff = true;
				}
				else
				{
					oHasOn = true;
				}
			}
		}

		// --------------------------------------------------------------------
		// 複数の IRcMaster をカンマで結合
		// --------------------------------------------------------------------
		private void ConcatMasterNameAndRuby(List<IRcMaster> oMasters, out String oName, out String oRuby)
		{
			if (oMasters.Count == 0)
			{
				oName = null;
				oRuby = null;
				return;
			}

			StringBuilder aSbName = new StringBuilder();
			StringBuilder aSbRuby = new StringBuilder();
			aSbName.Append(oMasters[0].Name);
			aSbRuby.Append(oMasters[0].Ruby);

			for (Int32 i = 1; i < oMasters.Count; i++)
			{
				aSbName.Append("," + oMasters[i].Name);
				aSbRuby.Append("," + oMasters[i].Ruby);
			}

			oName = aSbName.ToString();
			oRuby = aSbRuby.ToString();
		}

		// --------------------------------------------------------------------
		// （oDicByFile から取得した）人物情報をゆかり用リストデータベースに登録
		// --------------------------------------------------------------------
		private void RegistPerson<T>(TFound oFound, TPerson oPerson) where T : class, IRcSequence, new()
		{
			if (mYukariDbContext == null)
			{
				return;
			}

			// 人物は人物テーブルに登録済みか？
			TPerson aPersonRecord = null;
			if (!String.IsNullOrEmpty(oPerson.Id))
			{
				aPersonRecord = oPerson;
			}
			if (aPersonRecord == null)
			{
				// 人物テーブルにフォルダー設定の人物情報と同名の人物があるか？
				Table<TPerson> aTablePerson = mYukariDbContext.GetTable<TPerson>();
				IQueryable<TPerson> aPersonResult3 =
						from x in aTablePerson
						where x.Name.Equals(oPerson.Name)
						select x;
				aPersonRecord = YlCommon.FirstOrDefault(aPersonResult3);
				if (aPersonRecord == null)
				{
					// 同名のタグが無いので、同 Id とするタグがまだ存在しなければ作成
					String aPersonId = TEMP_ID_PREFIX + oPerson.Name;
					IQueryable<TPerson> aPersonResult2 =
							from x in aTablePerson
							where x.Id.Equals(aPersonId)
							select x;
					aPersonRecord = YlCommon.FirstOrDefault(aPersonResult2);
					if (aPersonRecord == null)
					{
						aPersonRecord = new TPerson
						{
							// IRcBase
							Id = aPersonId,
							Import = false,
							Invalid = false,
							UpdateTime = YlConstants.INVALID_MJD,
							Dirty = true,

							// IRcMaster
							Name = oPerson.Name,
							Ruby = null,
							Keyword = null,
						};
						aTablePerson.InsertOnSubmit(aPersonRecord);
					}
				}
			}

			// TXxxSequence にフォルダー設定のタグ情報が無ければ保存
			// where で == を使うと FirstOrDefault() でエラーが発生するため Equals() を使う
			Table<T> aTablePersonSequence = mYukariDbContext.GetTable<T>();
			IQueryable<T> aPersonSequenceResult =
					from x in aTablePersonSequence
					where x.Id.Equals(oFound.SongId) && x.LinkId.Equals(aPersonRecord.Id)
					select x;
			T aPersonSequenceRecord = YlCommon.FirstOrDefault(aPersonSequenceResult);
			if (aPersonSequenceRecord == null)
			{
				IQueryable<Int32> aSequenceResult =
						from x in aTablePersonSequence
						where x.Id.Equals(oFound.SongId)
						select x.Sequence;
				Int32 aSeqMax = -1;
				foreach (Int32 aSequence in aSequenceResult)
				{
					aSeqMax = Math.Max(aSeqMax, aSequence);
				}
				aPersonSequenceRecord = new T
				{
					// IDbBase
					Id = oFound.SongId,
					Import = false,
					Invalid = false,
					UpdateTime = YlConstants.INVALID_MJD,
					Dirty = true,

					// IDbSequence
					Sequence = aSeqMax + 1,
					LinkId = aPersonRecord.Id,
				};
				aTablePersonSequence.InsertOnSubmit(aPersonSequenceRecord);
			}

			mYukariDbContext.SubmitChanges();
		}

		// --------------------------------------------------------------------
		// 歌手情報を oDicByFile から設定
		// --------------------------------------------------------------------
		private void SetTFoundArtistByDic(TFound oRecord, Dictionary<String, String> oDicByFile)
		{
			if (oRecord.ArtistName == null && oDicByFile[YlConstants.RULE_VAR_ARTIST] != null)
			{
				// ファイル名から歌手名を取得できている場合は、楽曲情報データベースからフリガナを探す
				List<TPerson> aArtists;
				aArtists = YlCommon.SelectMastersByName<TPerson>(mMusicInfoDbContext, oDicByFile[YlConstants.RULE_VAR_ARTIST]);
				if (aArtists.Count > 0)
				{
					// 歌手名が楽曲情報データベースに登録されていた場合はその情報を使う
					oRecord.ArtistName = aArtists[0].Name;
					oRecord.ArtistRuby = aArtists[0].Ruby;
					RegistPerson<TArtistSequence>(oRecord, aArtists[0]);
				}
				else
				{
					// 歌手名そのままでは楽曲情報データベースに登録されていない場合
					if (oDicByFile[YlConstants.RULE_VAR_ARTIST].IndexOf(YlConstants.VAR_VALUE_DELIMITER) >= 0)
					{
						// 区切り文字で区切られた複数の歌手名が記載されている場合は分解して解析する
						String[] aArtistNames = oDicByFile[YlConstants.RULE_VAR_ARTIST].Split(YlConstants.VAR_VALUE_DELIMITER[0]);
						foreach (String aArtistName in aArtistNames)
						{
							List<TPerson> aArtistsTmp = YlCommon.SelectMastersByName<TPerson>(mMusicInfoDbContext, aArtistName);
							if (aArtistsTmp.Count > 0)
							{
								// 区切られた歌手名が楽曲情報データベースに存在する
								aArtists.Add(aArtistsTmp[0]);
							}
							else
							{
								// 区切られた歌手名が楽曲情報データベースに存在しないので仮の人物を作成
								TPerson aArtistTmp = new TPerson();
								aArtistTmp.Name = aArtistTmp.Ruby = aArtistName;
								aArtists.Add(aArtistTmp);
							}
						}
						String aArtistName2;
						String aArtistRuby2;
						ConcatMasterNameAndRuby(aArtists.ToList<IRcMaster>(), out aArtistName2, out aArtistRuby2);
						oRecord.ArtistName = aArtistName2;
						oRecord.ArtistRuby = aArtistRuby2;
						for (Int32 i = 0; i < aArtists.Count; i++)
						{
							RegistPerson<TArtistSequence>(oRecord, aArtists[i]);
						}
					}
					else
					{
						// 楽曲情報データベースに登録されていないので漢字のみ格納
						oRecord.ArtistName = oDicByFile[YlConstants.RULE_VAR_ARTIST];
						TPerson aArtist = new TPerson
						{
							Name = oDicByFile[YlConstants.RULE_VAR_ARTIST],
						};
						RegistPerson<TArtistSequence>(oRecord, aArtist);
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 検出ファイルレコードの値を、楽曲情報データベースから検索して設定する
		// ファイル名を元に検索し、結果が複数ある場合は他の情報も照らし合わせて最も近い物を設定する
		// --------------------------------------------------------------------
		private void SetTFoundValueByMusicInfoDb(TFound oRecord, Dictionary<String, String> oDicByFile)
		{
			if (oDicByFile[YlConstants.RULE_VAR_TITLE] == null)
			{
				return;
			}

			List<TSong> aSongs;

			// 楽曲名で検索
			aSongs = YlCommon.SelectMastersByName<TSong>(mMusicInfoDbContext, oDicByFile[YlConstants.RULE_VAR_TITLE]);

			// タイアップ名で絞り込み
			if (aSongs.Count > 1 && oDicByFile[YlConstants.RULE_VAR_PROGRAM] != null)
			{
				List<TSong> aSongsWithTieUp = new List<TSong>();
				foreach (TSong aSong in aSongs)
				{
					TTieUp aTieUp = YlCommon.SelectBaseById<TTieUp>(mMusicInfoDbContext, aSong.TieUpId);
					if (aTieUp != null && aTieUp.Name == oDicByFile[YlConstants.RULE_VAR_PROGRAM])
					{
						aSongsWithTieUp.Add(aSong);
					}
				}
				if (aSongsWithTieUp.Count > 0)
				{
					aSongs = aSongsWithTieUp;
				}
			}

			// カテゴリーで絞り込み
			if (aSongs.Count > 1 && oDicByFile[YlConstants.RULE_VAR_CATEGORY] != null)
			{
				List<TSong> aSongsWithCategory = new List<TSong>();
				foreach (TSong aSong in aSongs)
				{
					TCategory aCategory = YlCommon.SelectBaseById<TCategory>(mMusicInfoDbContext, aSong.CategoryId);
					if (aCategory != null && aCategory.Name == oDicByFile[YlConstants.RULE_VAR_CATEGORY])
					{
						aSongsWithCategory.Add(aSong);
					}
				}
				if (aSongsWithCategory.Count > 0)
				{
					aSongs = aSongsWithCategory;
				}
			}

			// 歌手名で絞り込み
			if (aSongs.Count > 1 && oDicByFile[YlConstants.RULE_VAR_ARTIST] != null)
			{
				List<TSong> aSongsWithArtist = new List<TSong>();
				foreach (TSong aSong in aSongs)
				{
					String aArtistName;
					String aArtistRuby;
					ConcatMasterNameAndRuby(YlCommon.SelectSequencePeopleBySongId<TArtistSequence>(mMusicInfoDbContext, aSong.Id).ToList<IRcMaster>(), out aArtistName, out aArtistRuby);
					if (!String.IsNullOrEmpty(aArtistName) && aArtistName == oDicByFile[YlConstants.RULE_VAR_ARTIST])
					{
						aSongsWithArtist.Add(aSong);
					}
				}
				if (aSongsWithArtist.Count > 0)
				{
					aSongs = aSongsWithArtist;
				}
			}

			TTieUp aTieUpOfSong = null;
			TSong aSelectedSong = null;
			if (aSongs.Count == 0)
			{
				// 楽曲情報データベース内に曲情報が無い場合は、タイアップ情報があるか検索
				if (oDicByFile[YlConstants.RULE_VAR_PROGRAM] != null)
				{
					List<TTieUp> aTieUps = YlCommon.SelectMastersByName<TTieUp>(mMusicInfoDbContext, oDicByFile[YlConstants.RULE_VAR_PROGRAM]);
					if (aTieUps.Count > 0)
					{
						aTieUpOfSong = aTieUps[0];
					}
				}
				if (aTieUpOfSong == null)
				{
					// 曲情報もタイアップ情報も無い場合は諦める
					return;
				}
			}
			else
			{
				// 楽曲情報データベース内に曲情報がある場合は、曲に紐付くタイアップを得る
				aSelectedSong = aSongs[0];
				aTieUpOfSong = YlCommon.SelectBaseById<TTieUp>(mMusicInfoDbContext, aSelectedSong.TieUpId);
			}

			if (aTieUpOfSong != null)
			{
				TCategory aCategoryOfTieUp = YlCommon.SelectBaseById<TCategory>(mMusicInfoDbContext, aTieUpOfSong.CategoryId);
				if (aCategoryOfTieUp != null)
				{
					// TCategory 由来項目の設定
					oRecord.Category = aCategoryOfTieUp.Name;
				}

				TMaker aMakerOfTieUp = YlCommon.SelectBaseById<TMaker>(mMusicInfoDbContext, aTieUpOfSong.MakerId);
				if (aMakerOfTieUp != null)
				{
					// TMaker 由来項目の設定
					oRecord.MakerName = aMakerOfTieUp.Name;
					oRecord.MakerRuby = aMakerOfTieUp.Ruby;
				}

				List<TTieUpGroup> aTieUpGroups = YlCommon.SelectSequenceTieUpGroupsByTieUpId(mMusicInfoDbContext, aTieUpOfSong.Id);
				if (aTieUpGroups.Count > 0)
				{
					// TTieUpGroup 由来項目の設定
					oRecord.TieUpGroupName = aTieUpGroups[0].Name;
					oRecord.TieUpGroupRuby = aTieUpGroups[0].Ruby;
				}

				// TieUp 由来項目の設定
				oRecord.TieUpName = aTieUpOfSong.Name;
				oRecord.TieUpRuby = aTieUpOfSong.Ruby;
				oRecord.TieUpAgeLimit = aTieUpOfSong.AgeLimit;
				oRecord.SongReleaseDate = aTieUpOfSong.ReleaseDate;
			}

			if (aSelectedSong == null)
			{
				return;
			}

			// 人物系
			String aName;
			String aRuby;
			ConcatMasterNameAndRuby(YlCommon.SelectSequencePeopleBySongId<TArtistSequence>(mMusicInfoDbContext, aSelectedSong.Id).ToList<IRcMaster>(), out aName, out aRuby);
			oRecord.ArtistName = aName;
			oRecord.ArtistRuby = aRuby;
			ConcatMasterNameAndRuby(YlCommon.SelectSequencePeopleBySongId<TLyristSequence>(mMusicInfoDbContext, aSelectedSong.Id).ToList<IRcMaster>(), out aName, out aRuby);
			oRecord.LyristName = aName;
			oRecord.LyristRuby = aRuby;
			ConcatMasterNameAndRuby(YlCommon.SelectSequencePeopleBySongId<TComposerSequence>(mMusicInfoDbContext, aSelectedSong.Id).ToList<IRcMaster>(), out aName, out aRuby);
			oRecord.ComposerName = aName;
			oRecord.ComposerRuby = aRuby;
			ConcatMasterNameAndRuby(YlCommon.SelectSequencePeopleBySongId<TArrangerSequence>(mMusicInfoDbContext, aSelectedSong.Id).ToList<IRcMaster>(), out aName, out aRuby);
			oRecord.ArrangerName = aName;
			oRecord.ArrangerRuby = aRuby;

			// TSong 由来項目の設定
			oRecord.SongId = aSelectedSong.Id;
			oRecord.SongName = aSelectedSong.Name;
			oRecord.SongRuby = aSelectedSong.Ruby;
			oRecord.SongOpEd = aSelectedSong.OpEd;
			if (oRecord.SongReleaseDate <= YlConstants.INVALID_MJD && aSelectedSong.ReleaseDate > YlConstants.INVALID_MJD)
			{
				oRecord.SongReleaseDate = aSelectedSong.ReleaseDate;
			}
			if (String.IsNullOrEmpty(oRecord.Category))
			{
				TCategory aCategoryOfSong = YlCommon.SelectBaseById<TCategory>(mMusicInfoDbContext, aSelectedSong.CategoryId);
				if (aCategoryOfSong != null)
				{
					oRecord.Category = aCategoryOfSong.Name;
				}
			}

			// タグ
			ConcatMasterNameAndRuby(YlCommon.SelectSequenceTagsBySongId(mMusicInfoDbContext, aSelectedSong.Id).ToList<IRcMaster>(), out aName, out aRuby);
			oRecord.TagName = aName;
			oRecord.TagRuby = aRuby;
		}

	}
	// public class TFoundSetter ___END___
}
// namespace YukaLister.Models.Database ___END___
