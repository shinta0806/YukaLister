// ============================================================================
// 
// リムーバブルメディアの着脱を検出する添付ビヘイビア
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using YukaLister.Models.SharedMisc;

namespace YukaLister.Models.Behaviors
{
	public class DeviceChangeAttachedBehavior
	{
		// ====================================================================
		// public メンバー変数
		// ====================================================================

		public static readonly DependencyProperty CommandProperty =
				DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(DeviceChangeAttachedBehavior),
				new PropertyMetadata(null, SourceCommandChanged));

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 添付プロパティー GET
		// --------------------------------------------------------------------
		public static ICommand GetCommand(DependencyObject oObject)
		{
			return (ICommand)oObject.GetValue(CommandProperty);
		}

		// --------------------------------------------------------------------
		// 添付プロパティー SET
		// --------------------------------------------------------------------
		public static void SetCommand(DependencyObject oObject, ICommand oValue)
		{
			oObject.SetValue(CommandProperty, oValue);
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// WndProc
		private static HwndSourceHook smWndProc = new HwndSourceHook(WndProc);

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定されたコマンドが実行可能かどうか
		// --------------------------------------------------------------------
		private static Boolean CanExecuteCommand(Object oSender, out ICommand oCommand)
		{
			oCommand = null;

			UIElement aElement = oSender as UIElement;
			if (aElement == null)
			{
				return false;
			}

			oCommand = GetCommand(aElement);
			if (oCommand == null || !oCommand.CanExecute(null))
			{
				return false;
			}

			return true;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー（Command 変更）
		// --------------------------------------------------------------------
		private static void SourceCommandChanged(DependencyObject oObject, DependencyPropertyChangedEventArgs oArgs)
		{
			Window aWindow = oObject as Window;
			if (aWindow == null)
			{
				return;
			}

			if (GetCommand(aWindow) != null)
			{
				// コマンドが設定された場合はイベントハンドラーを有効にする
				WindowInteropHelper aHelper = new WindowInteropHelper(aWindow);
				HwndSource aWndSource = HwndSource.FromHwnd(aHelper.Handle);
				aWndSource.AddHook(smWndProc);
			}
			else
			{
				// コマンドが解除された場合はイベントハンドラーを無効にする
				WindowInteropHelper aHelper = new WindowInteropHelper(aWindow);
				HwndSource aWndSource = HwndSource.FromHwnd(aHelper.Handle);
				aWndSource.RemoveHook(smWndProc);
			}
		}

		// --------------------------------------------------------------------
		// HWnd から Window を取得
		// --------------------------------------------------------------------
		private static Window WindowFromHWnd(IntPtr oHWnd)
		{
			HwndSource aWndSource = HwndSource.FromHwnd(oHWnd);
			return aWndSource.RootVisual as Window;
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// USB メモリ等の着脱により呼び出される
		// --------------------------------------------------------------------
		private static void WmDeviceChange(IntPtr oHWnd, IntPtr oWParam, IntPtr oLParam)
		{
			ICommand aCommand;
			if (!CanExecuteCommand(WindowFromHWnd(oHWnd), out aCommand))
			{
				return;
			}

			switch ((DBT)oWParam.ToInt32())
			{
				case DBT.DBT_DEVICEARRIVAL:
				case DBT.DBT_DEVICEREMOVECOMPLETE:
					break;
				default:
					return;
			}
			if (oLParam == IntPtr.Zero)
			{
				return;
			}

			WindowsApi.DEV_BROADCAST_HDR aHdr = (WindowsApi.DEV_BROADCAST_HDR)Marshal.PtrToStructure(oLParam, typeof(WindowsApi.DEV_BROADCAST_HDR));
			if (aHdr.dbch_devicetype != (Int32)DBT_DEVTYP.DBT_DEVTYP_VOLUME)
			{
				return;
			}

			WindowsApi.DEV_BROADCAST_VOLUME aVolume = (WindowsApi.DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(oLParam, typeof(WindowsApi.DEV_BROADCAST_VOLUME));
			UInt32 aUnitMask = aVolume.dbcv_unitmask;
			if (aUnitMask == 0)
			{
				return;
			}

			Char aNumShift = (Char)0;
			String aDriveLetter;
			while (aUnitMask != 1)
			{
				aUnitMask >>= 1;
				aNumShift++;
			}
			aDriveLetter = new String((Char)('A' + aNumShift), 1) + ":";

			// 着脱情報を引数としてコマンドを実行
			DeviceChangeInfo aInfo = new DeviceChangeInfo();
			aInfo.Kind = (DBT)oWParam.ToInt32();
			aInfo.DriveLetter = aDriveLetter;
			aCommand.Execute(aInfo);
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// SD カード等の着脱により呼び出される
		// --------------------------------------------------------------------
		private static void WmShNotify(IntPtr oHWnd, IntPtr oWParam, IntPtr oLParam)
		{
			ICommand aCommand;
			if (!CanExecuteCommand(WindowFromHWnd(oHWnd), out aCommand))
			{
				return;
			}

			switch ((SHCNE)oLParam)
			{
				case SHCNE.SHCNE_MEDIAINSERTED:
				case SHCNE.SHCNE_MEDIAREMOVED:
					break;
				default:
					return;
			}

			WindowsApi.SHNOTIFYSTRUCT aShNotifyStruct = (WindowsApi.SHNOTIFYSTRUCT)Marshal.PtrToStructure(oWParam, typeof(WindowsApi.SHNOTIFYSTRUCT));
			StringBuilder aDriveRoot = new StringBuilder();
			WindowsApi.SHGetPathFromIDList((IntPtr)aShNotifyStruct.dwItem1, aDriveRoot);
			String aDriveLetter = aDriveRoot.ToString().Substring(0, 2);

			// 着脱情報を引数としてコマンドを実行
			DeviceChangeInfo aInfo = new DeviceChangeInfo();
			aInfo.Kind = (SHCNE)oLParam == SHCNE.SHCNE_MEDIAINSERTED ? DBT.DBT_DEVICEARRIVAL : DBT.DBT_DEVICEREMOVECOMPLETE;
			aInfo.DriveLetter = aDriveLetter;
			aCommand.Execute(aInfo);
		}

		// --------------------------------------------------------------------
		// メッセージハンドラ
		// --------------------------------------------------------------------
		private static IntPtr WndProc(IntPtr oHWnd, Int32 oMsg, IntPtr oWParam, IntPtr oLParam, ref Boolean oHandled)
		{
			switch ((Wm)oMsg)
			{
				case (Wm)WindowsApi.WM_DEVICECHANGE:
					WmDeviceChange(oHWnd, oWParam, oLParam);
					oHandled = true;
					break;
				case (Wm)WindowsApi.WM_SHNOTIFY:
					WmShNotify(oHWnd, oWParam, oLParam);
					oHandled = true;
					break;
			}

			return IntPtr.Zero;
		}

	}
	// public class DeviceChangeAttachedBehavior ___END___

	public class DeviceChangeInfo
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 着脱種別
		public DBT Kind { get; set; }

		// 着脱されたドライブレター（"A:" のようにコロンまで）
		public String DriveLetter { get; set; }
	}
	// public class DeviceChangeInfo ___END___
}
// namespace YukaLister.Models.Behaviors ___END___
