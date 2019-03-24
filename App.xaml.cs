using Shinta;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YukaLister.Shared;

namespace YukaLister
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		[STAThread]
		public static void Main()
		{
			const String SEMAPHORE_NAME = Common.SHINTA + "." + YlCommon.APP_ID;

			// Semaphore クラスのインスタンスを生成し、アプリケーション終了まで保持する
			Boolean aCreatedNew;
			using (Semaphore aSemaphore = new Semaphore(1, 1, SEMAPHORE_NAME, out aCreatedNew))
			{
				if (!aCreatedNew)
				{
					// 既存プロセスが先にセマフォを作っていた場合はそちらをアクティベートしてこちらは終了する
					Common.ActivateSameNameProcessWindow();
					return;
				}

				// 既存プロセスが無いため実行開始
				App app = new App();
				app.InitializeComponent();
				app.Run();
			}
		}
	}
}
