using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Chorder.UI.Behaviors
{
        public static class FocusBehavior
        {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnIsFocusedChanged));

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb && (bool)e.NewValue)
            {
                // ⭐ 关键：等 UI 渲染完成
                tb.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!tb.IsVisible) return;

                    tb.Focus();
                    tb.SelectAll();
                }), DispatcherPriority.Loaded);
            }
        }
    }
}
