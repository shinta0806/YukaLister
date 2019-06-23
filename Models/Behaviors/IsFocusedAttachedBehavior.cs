// ============================================================================
// 
// IsFocused 添付ビヘイビア
// 
// ============================================================================

// ----------------------------------------------------------------------------
// IsFocused を true にすることにより、コントロールにフォーカスが当たる
// ToDo: ViewModel 側に false が伝播されないので、ViewModel 側は RaisePropertyChangedIfSet() ではなく
// RaisePropertyChanged() で強制発効しないと再度フォーカスを当てられない
// ----------------------------------------------------------------------------

using System;
using System.Windows;

namespace YukaLister.Models.Behaviors
{
	public class IsFocusedAttachedBehavior
	{
		// ====================================================================
		// public メンバー変数
		// ====================================================================

		public static readonly DependencyProperty IsFocusedProperty =
				DependencyProperty.RegisterAttached("IsFocused", typeof(Boolean), typeof(IsFocusedAttachedBehavior),
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SourceIsFocusedChanged));

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 添付プロパティー GET
		// --------------------------------------------------------------------
		public static Boolean GetIsFocused(DependencyObject oObject)
		{
			return (Boolean)oObject.GetValue(IsFocusedProperty);
		}

		// --------------------------------------------------------------------
		// 添付プロパティー SET
		// --------------------------------------------------------------------
		public static void SetIsFocused(DependencyObject oObject, Boolean oValue)
		{
			oObject.SetValue(IsFocusedProperty, oValue);
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// イベントハンドラー
		// --------------------------------------------------------------------
		private static void SourceIsFocusedChanged(DependencyObject oObject, DependencyPropertyChangedEventArgs oArgs)
		{
			if ((Boolean)oArgs.NewValue)
			{
				UIElement aElement = oObject as UIElement;
				aElement?.Focus();

				// 再度フォーカスを当てる際にイベント駆動するように false にしておく
				SetIsFocused(oObject, false);
			}
		}
	}
	// public class IsFocusedAttachedBehavior ___END___
}
// namespace YukaLister.Models.Behaviors ___END___
