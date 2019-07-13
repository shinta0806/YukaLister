﻿// ============================================================================
// 
// 雑多なクラス群
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;

namespace YukaLister.Models.SharedMisc
{
	// ====================================================================
	// ドライブ接続時にゆかり検索対象フォルダーに自動的に追加するための情報
	// ====================================================================
	public class AutoTargetInfo
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public AutoTargetInfo()
		{
			Folders = new List<String>();
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 前回接続時に追加されていたフォルダー群（ドライブレターを除き '\\' から始まる）
		public List<String> Folders { get; set; }
	}

	// ====================================================================
	// フォルダー設定ウィンドウでのプレビュー情報
	// ====================================================================
	public class PreviewInfo
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ファイル名（パス無）
		public String FileName { get; set; }

		// 取得項目
		public String Items { get; set; }

		// ファイル最終更新日時（修正ユリウス日）
		public Double LastWriteTime { get; set; }

		// ファイル最終更新日時（修正ユリウス日）表示用
		public String LastWriteTimeText
		{
			get => JulianDay.ModifiedJulianDateToDateTime(LastWriteTime).ToString(YlConstants.DATE_FORMAT);
		}

	} // public class PreviewInfo ___END___

}
// namespace YukaLister.Models.SharedMisc ___END___
