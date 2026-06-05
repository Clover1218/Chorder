using Chorder.Clients.Searcher;
using Chorder.Models.Entities;
using Chorder.Repository;
using Chorder.Services;
using Chorder.Services.Player;
using Chorder.UI.ViewModels;
using Chorder.ViewModels.Player;
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

        public MainWindow()
        {
            InitializeComponent();
            var sqlConnection_factory= new SQLServerConnectionFactory();
            var playbackQueue_repository=new PlaybackQueueRepository(sqlConnection_factory);
            var playHistory_repository=new PlayHistoryRepository(sqlConnection_factory);
            var playlist_repository=new PlaylistRepository(sqlConnection_factory);
            var trackInfo_repository=new TrackInfoRepository(sqlConnection_factory);
            var statistics_repository = new StatisticsRepository(sqlConnection_factory);

            var trackInfo_service=new TrackInfoService(trackInfo_repository);
            var BiliBili_searcher=new BiliBiliSearcher();
            var search_service = new SearchService(BiliBili_searcher);
            var playbackQueue_service=new PlaybackQueueService(playbackQueue_repository);
           
            var playlist_service= new PlaylistService(playlist_repository);
            var player_service=new PlayerService(playHistory_repository, trackInfo_service);
            var statistics_service = new StatisticsService(statistics_repository);

            var vm = new MainViewModel(search_service,player_service,playbackQueue_service,playlist_service, trackInfo_service, statistics_service);
            this.DataContext = vm;
            
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
        private void ClosePopup(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ClosePlaylistPopup();
            }
        }
    }
}