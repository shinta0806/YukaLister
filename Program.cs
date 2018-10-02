using Shinta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YukaLister.Shared;

namespace YukaLister
{
	static class Program
	{
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main()
		{
			// 多重起動のチェック
			Mutex aMutex;
			if (Common.ActivateAnotherProcessWindowIfNeeded(YlCommon.APP_ID, out aMutex))
			{
				// 既存のプロセスをアクティベートしたため終了する
				return;
			}

			// 既存プロセスが無いため実行開始
			try
			{
				// Visual Studio 自動生成コード
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new FormYukaLister());
			}
			finally
			{
				aMutex.ReleaseMutex();
			}
		}
	}
}
