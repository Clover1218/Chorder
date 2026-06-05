using Chorder.UI.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chorder.UI.Behaviors
{
    public class ListBoxDragDropBehavior : Behavior<ListBox>
    {
        private Point _dragStartPoint;
        private Type _draggedType;

        protected override void OnAttached()
        {
            AssociatedObject.AllowDrop = true;
            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseDown;
            AssociatedObject.PreviewMouseMove += OnMouseMove;
            AssociatedObject.Drop += OnDrop;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseDown;
            AssociatedObject.PreviewMouseMove -= OnMouseMove;
            AssociatedObject.Drop -= OnDrop;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _draggedType = null;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var currentPosition = e.GetPosition(null);

            if (Math.Abs(currentPosition.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPosition.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            var listBox = (ListBox)sender;

            if (listBox.SelectedItem == null)
                return;

            _draggedType = listBox.SelectedItem.GetType();

            DragDrop.DoDragDrop(listBox, listBox.SelectedItem, DragDropEffects.Move);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.DataContext is not MainViewModel vm)
                return;

            var targetElement = e.OriginalSource as FrameworkElement;
            if (targetElement == null) return;

            var targetItem = targetElement.DataContext;
            if (targetItem == null) return;

            if (e.Data.GetDataPresent(typeof(PlaybackItemViewModel)))
            {
                var sourceItem = e.Data.GetData(typeof(PlaybackItemViewModel)) as PlaybackItemViewModel;
                if (sourceItem == null || sourceItem == targetItem) return;

                int oldIndex = listBox.Items.IndexOf(sourceItem);
                int newIndex = listBox.Items.IndexOf(targetItem);
                if (oldIndex < 0 || newIndex < 0) return;

                vm.PlaybackQueueMoveCommand.Execute((oldIndex, newIndex));
            }
            else if (e.Data.GetDataPresent(typeof(PlaylistLibarayItemViewModel)))
            {
                var sourceItem = e.Data.GetData(typeof(PlaylistLibarayItemViewModel)) as PlaylistLibarayItemViewModel;
                if (sourceItem == null || sourceItem == targetItem) return;

                int oldIndex = listBox.Items.IndexOf(sourceItem);
                int newIndex = listBox.Items.IndexOf(targetItem);
                if (oldIndex < 0 || newIndex < 0) return;

                vm.PlaylistMoveCommand.Execute((oldIndex, newIndex));
            }
            else if (e.Data.GetDataPresent(typeof(PlaylistTrackItemViewModel)))
            {
                var sourceItem = e.Data.GetData(typeof(PlaylistTrackItemViewModel)) as PlaylistTrackItemViewModel;
                if (sourceItem == null || sourceItem == targetItem) return;

                int oldIndex = listBox.Items.IndexOf(sourceItem);
                int newIndex = listBox.Items.IndexOf(targetItem);
                if (oldIndex < 0 || newIndex < 0) return;

                vm.PlaylistTrackMoveCommand.Execute((oldIndex, newIndex));
            }
        }
    }
}
