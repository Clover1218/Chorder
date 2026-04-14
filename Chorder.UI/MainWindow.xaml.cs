using Chorder.Clients.Searcher;
using Chorder.Models.Entities;
using Chorder.Services;
using Chorder.Services.Player;
using Chorder.UI.ViewModels;
using LibVLCSharp.Shared;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Chorder.UI.ViewModels.MainViewModel;

namespace Chorder.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlayerService _playerService = new PlayerService();

        public MainWindow()
        {
            InitializeComponent();
            var BiliBili_searcher=new BiliBiliSearcher();
            var search_service = new SearchService(BiliBili_searcher); // Service
            var playbackQueue_service=new PlaybackQueueService();
            var playlist_service= new PlaylistService();
            var vm = new MainViewModel(search_service,_playerService,playbackQueue_service,playlist_service); // ViewModel
        
            this.DataContext = vm; // ⭐ 关键

        }
        private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel vm &&
                e.NewValue is MainViewModel.BiliBiliItemNode node)
            {
                await vm.LoadPagesAsync(node);
            }
        }
        private async void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (DataContext is MainViewModel vm)
            //{
            //    if (sender is TreeViewItem item && item.DataContext is BiliBiliItemNode node)
            //    {
            //        await this._playerService.Play(node.Bvid,1);
            //    }
            //}
        }
        //private void PlaylistLibarayItem_DoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (sender is ListBoxItem item &&
        //        item.DataContext is PlaylistLibarayItemViewModel vm){
        //        vm.IsEditing = true;
        //        e.Handled = true;
        //    }
        //}
        private ListBoxItem _lastClickedItem;
        private System.DateTime _lastClickTime;
        private void PlaylistLibarayItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListBoxItem item ||
                item.DataContext is not PlaylistLibarayItemViewModel vm)
                return;
        
            var now = DateTime.Now;
        
            // ⭐ 条件1：已经选中
            bool isAlreadySelected = item.IsSelected;
            (this.DataContext as MainViewModel).SelectedPlaylistChanged(vm);
            // ⭐ 条件2：是同一个项
            bool isSameItem = _lastClickedItem == item;
        
            // ⭐ 条件3：时间间隔（防止太慢）
            bool isQuickClick = (now - _lastClickTime).TotalMilliseconds < 500;
        
            if (isAlreadySelected && isSameItem && isQuickClick)
            {
                vm.IsEditing = true;
                //e.Handled = true;
            }
        
            _lastClickedItem = item;
            _lastClickTime = now;
        }
        private void ClosePopup(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ClosePlaylistPopup();
            }
        }
    }
}