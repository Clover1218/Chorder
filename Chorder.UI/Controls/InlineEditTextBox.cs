using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chorder.UI.Controls
{
    public class InlineEditTextBox : TextBox { 
        public static readonly DependencyProperty IsEditingProperty = 
            DependencyProperty.Register( nameof(IsEditing), typeof(bool), typeof(InlineEditTextBox), new PropertyMetadata(false, OnIsEditingChanged)); 
        public bool IsEditing { 
            get => (bool)GetValue(IsEditingProperty); 
            set => SetValue(IsEditingProperty, value); } 
        public static readonly DependencyProperty CommitCommandProperty = 
            DependencyProperty.Register( nameof(CommitCommand), typeof(ICommand), typeof(InlineEditTextBox)); 
        public ICommand CommitCommand { 
            get => (ICommand)GetValue(CommitCommandProperty); 
            set => SetValue(CommitCommandProperty, value); } 
        public static readonly DependencyProperty CommitCommandParameterProperty = 
            DependencyProperty.Register( nameof(CommitCommandParameter), typeof(object), typeof(InlineEditTextBox)); 
        public object CommitCommandParameter { 
            get => GetValue(CommitCommandParameterProperty); 
            set => SetValue(CommitCommandParameterProperty, value); } 
        protected override void OnLostFocus(RoutedEventArgs e) { 
            base.OnLostFocus(e); Commit(); } 
        private string _originalValue;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter)
        {
            Commit();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Cancel();
            e.Handled = true;
        }
    }

    private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InlineEditTextBox tb)
        {
            if ((bool)e.NewValue)
            {
                tb._originalValue = tb.Text;

                tb.Dispatcher.BeginInvoke(new Action(() =>
                {
                    tb.Focus();
                    tb.SelectAll();
                }));
            }
        }
    }

    private bool _isCommitting;

    private void Commit()
    {
        if (_isCommitting) return;
        _isCommitting = true;
        CommitCommand?.Execute(CommitCommandParameter);
        IsEditing = false;
        _isCommitting = false;
    }

    private void Cancel()
    {
        Text = _originalValue; // ⭐关键
        IsEditing = false;
                Dispatcher.BeginInvoke(() =>
    {
        var parent = this.TemplatedParent as UIElement;
        parent?.Focus();
    });
    }
    }
}
