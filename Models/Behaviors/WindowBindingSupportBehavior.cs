// ============================================================================
// 
// Window のバインド可能なプロパティーを増やすためのビヘイビア
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace YukaLister.Models.Behaviors
{
	public class WindowBindingSupportBehavior : Behavior<Window>
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// Window.Closing をコマンドで扱えるようにする
		public ICommand ClosingCommand
		{
			get => (ICommand)GetValue(ClosingCommandProperty);
			set => SetValue(ClosingCommandProperty, value);
		}

		// Window.IsActive をバインド可能にする
		public Boolean IsActive
		{
			get => (Boolean)GetValue(IsActiveProperty);
			set => SetValue(IsActiveProperty, value);
		}

		// ====================================================================
		// public メンバー変数
		// ====================================================================

		// Window.Closing
		public static readonly DependencyProperty ClosingCommandProperty
				= DependencyProperty.RegisterAttached("ClosingCommand", typeof(ICommand), typeof(WindowBindingSupportBehavior),
				new PropertyMetadata(null, SourceClosingCommandChanged));

		// Window.IsActive は元々読み取り専用だが変更可能とするためにコールバックを登録する
		public static readonly DependencyProperty IsActiveProperty =
				DependencyProperty.Register("IsActive", typeof(Boolean), typeof(WindowBindingSupportBehavior),
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SourceIsActiveChanged));

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// アタッチ時の準備作業
		// --------------------------------------------------------------------
		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject.Activated += ControlActivated;
			AssociatedObject.Deactivated += ControlDeactivated;
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// View 側で IsActive が変更された
		// --------------------------------------------------------------------
		private void ControlActivated(Object oSender, EventArgs oArgs)
		{
			IsActive = true;
		}

		// --------------------------------------------------------------------
		// View 側で Closing された
		// --------------------------------------------------------------------
		private void ControlClosing(Object oSender, CancelEventArgs oCancelEventArgs)
		{
			if (ClosingCommand == null || !ClosingCommand.CanExecute(null))
			{
				return;
			}

			// イベント引数を引数としてコマンドを実行
			ClosingCommand.Execute(oCancelEventArgs);
		}

		// --------------------------------------------------------------------
		// View 側で IsActive が変更された
		// --------------------------------------------------------------------
		private void ControlDeactivated(Object oSender, EventArgs oArgs)
		{
			IsActive = false;
		}

		// --------------------------------------------------------------------
		// ViewModel 側で ClosingCommand が変更された
		// --------------------------------------------------------------------
		private static void SourceClosingCommandChanged(DependencyObject oObject, DependencyPropertyChangedEventArgs oArgs)
		{
			WindowBindingSupportBehavior aThisObject = oObject as WindowBindingSupportBehavior;
			if (aThisObject == null || aThisObject.AssociatedObject == null)
			{
				return;
			}

			if (oArgs.NewValue != null)
			{
				// コマンドが設定された場合はイベントハンドラーを有効にする
				aThisObject.AssociatedObject.Closing += aThisObject.ControlClosing;
			}
			else
			{
				// コマンドが解除された場合はイベントハンドラーを無効にする
				aThisObject.AssociatedObject.Closing -= aThisObject.ControlClosing;
			}
		}

		// --------------------------------------------------------------------
		// ViewModel 側で IsActive が変更された
		// --------------------------------------------------------------------
		private static void SourceIsActiveChanged(DependencyObject oObject, DependencyPropertyChangedEventArgs oArgs)
		{
			WindowBindingSupportBehavior aThisObject = oObject as WindowBindingSupportBehavior;
			if (aThisObject == null || aThisObject.AssociatedObject == null)
			{
				return;
			}

			if ((Boolean)oArgs.NewValue)
			{
				aThisObject.AssociatedObject.Activate();
			}
		}


	}
	// public class WindowBindingSupportBehavior ___END___

}
// YukaLister.Models.Behaviors ___END___
