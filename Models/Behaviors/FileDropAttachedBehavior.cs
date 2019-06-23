// ============================================================================
// 
// FileDrop 添付ビヘイビア
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Input;

namespace YukaLister.Models.Behaviors
{
	public class FileDropAttachedBehavior
	{
		// ====================================================================
		// public メンバー変数
		// ====================================================================

		public static readonly DependencyProperty CommandProperty =
				DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(FileDropAttachedBehavior),
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
		// イベントハンドラー
		// --------------------------------------------------------------------
		private static void OnDragOver(Object oSender, DragEventArgs oDragEventArgs)
		{
			oDragEventArgs.Effects = DragDropEffects.None;
			oDragEventArgs.Handled = true;

			ICommand aCommand;
			if (!CanExecuteCommand(oSender, out aCommand))
			{
				return;
			}

			if (oDragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// ファイル類のときのみドラッグを受け付ける
				oDragEventArgs.Effects = DragDropEffects.Copy;
			}
		}

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private static void OnDrop(Object oSender, DragEventArgs oDragEventArgs)
		{
			ICommand aCommand;
			if (!CanExecuteCommand(oSender, out aCommand))
			{
				return;
			}

			String[] aDropFiles = oDragEventArgs.Data.GetData(DataFormats.FileDrop, false) as String[];
			if (aDropFiles == null)
			{
				return;
			}

			// ドロップされたファイル類のパスを引数としてコマンドを実行
			aCommand.Execute(aDropFiles);
		}

		// --------------------------------------------------------------------
		// イベントハンドラー（Command 変更）
		// --------------------------------------------------------------------
		private static void SourceCommandChanged(DependencyObject oObject, DependencyPropertyChangedEventArgs oArgs)
		{
			UIElement aElement = oObject as UIElement;
			if (aElement == null)
			{
				return;
			}

			if (GetCommand(aElement) != null)
			{
				// コマンドが設定された場合はイベントハンドラーを有効にする
				aElement.AllowDrop = true;
				aElement.DragOver += OnDragOver;
				aElement.Drop += OnDrop;
			}
			else
			{
				// コマンドが解除された場合はイベントハンドラーを無効にする
				aElement.AllowDrop = false;
				aElement.DragOver -= OnDragOver;
				aElement.Drop -= OnDrop;
			}
		}
	}
	// public class FileDropAttachedBehavior ___END___
}
// namespace YukaLister.Models.Behaviors ___END___
