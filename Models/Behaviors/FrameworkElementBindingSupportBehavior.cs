// ============================================================================
// 
// FrameworkElement のバインド可能なプロパティーを増やすためのビヘイビア
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Interactivity;

namespace YukaLister.Models.Behaviors
{
	public class FrameworkElementBindingSupportBehavior : Behavior<FrameworkElement>
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// FrameworkElement.ActualHeight をバインド可能にする
		public Double ActualHeight
		{
			get => (Double)GetValue(ActualHeightProperty);
			set => SetValue(ActualHeightProperty, value);
		}

		// FrameworkElement.ActualWidth をバインド可能にする
		public Double ActualWidth
		{
			get => (Double)GetValue(ActualWidthProperty);
			set => SetValue(ActualWidthProperty, value);
		}

		// ====================================================================
		// public メンバー変数
		// ====================================================================

		// FrameworkElement.ActualHeight
		public static readonly DependencyProperty ActualHeightProperty =
				DependencyProperty.Register("ActualHeight", typeof(Double), typeof(FrameworkElementBindingSupportBehavior),
				new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null));

		// FrameworkElement.ActualWidth
		public static readonly DependencyProperty ActualWidthProperty =
				DependencyProperty.Register("ActualWidth", typeof(Double), typeof(FrameworkElementBindingSupportBehavior),
				new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null));

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// アタッチ時の準備作業
		// --------------------------------------------------------------------
		protected override void OnAttached()
		{
			base.OnAttached();

			AssociatedObject.SizeChanged += ControlSizeChanged;
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// View 側でサイズが変更された
		// --------------------------------------------------------------------
		private void ControlSizeChanged(Object oSender, SizeChangedEventArgs oArgs)
		{
			FrameworkElement aElement = (FrameworkElement)oSender;

			ActualHeight = aElement.ActualHeight;
			ActualWidth = aElement.ActualWidth;
		}
	}
	// public class FrameworkElementBindingSupportBehavior ___END___

}
// YukaLister.Models.Behaviors ___END___
