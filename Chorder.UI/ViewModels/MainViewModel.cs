using Chorder.Models.Entities;
using Chorder.Services;
using Chorder.Services.Player;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Chorder.UI.ViewModels{
    public partial class MainViewModel : ObservableObject{

        private readonly SearchService _searchService;
        private readonly PlayerService _playerService;
        private readonly PlaybackQueueService _playbackQueueService;
        private readonly PlaylistService _playlistService;
        public ObservableCollection<BiliBiliItemNode> SearchItems { get; } = new();
        public ObservableCollection<PlaybackItemViewModel> PlaybackQueue { get; } = new();

        public ObservableCollection<PlaylistLibarayItemViewModel> Playlists{ get; } = new();
        public ObservableCollection<PlaylistItemViewModel> PlaylistItems{get; } = new();
        public class BiliBiliItemNode{   
            public string? Bvid { get; set; }
            public string? Title { get; set; }
            public string? Author { get; set; }
            public string? Duration { get; set; }
            public ObservableCollection<BiliBiliItemNode> Children { get; set; }
                = new ObservableCollection<BiliBiliItemNode>();
            public bool IsLoaded {get;set;}
            public bool IsPage {get;set;}

            public int Page{get;set;}
            public IRelayCommand? PlayCommand { get; set; }
        }
        public async Task LoadPagesAsync(BiliBiliItemNode node){
            if (node == null || node.IsLoaded==true ||node.IsPage==true)
                return;
            var pages = await _searchService.GetPageInfo(node.Bvid);
            foreach (var p in pages)
            {
                node.Children.Add(new BiliBiliItemNode
                {
                    Title = $"P{p.Page} - {p.Title}",
                    IsPage = true ,
                    Page=p.Page,
                    Bvid=node.Bvid,
                });
            }
            node.IsLoaded = true;
        }
        private void SyncQueue()
        {
            PlaybackQueue.Clear();
        
            foreach (var item in _playbackQueueService.Queue.Items)
            {
                PlaybackQueue.Add(new PlaybackItemViewModel
                {
                    Title = item.track.Title,
                    Bvid = item.track.Bvid,
                    Page = item.track.Page
                });
            }
        }
        private void SyncPlaylist()
        {
            Playlists.Clear();
            foreach (var item in _playlistService.Library.Playlists) {
                Playlists.Add(new PlaylistLibarayItemViewModel(item){
                    Name = item.Name,

                    IsEditing = false,
                });
            }

        }
        [ObservableProperty]
        private string keyword;
        [ObservableProperty]
        private string selectedPlaylistIndex;
        public MainViewModel(
            SearchService searchService,PlayerService playerService,
            PlaybackQueueService playbackQueueService,PlaylistService playlistService)
        {
            _searchService = searchService;
            _playerService = playerService;
            _playbackQueueService = playbackQueueService;
            _playbackQueueService.QueueChanged += SyncQueue;
            _playlistService=playlistService;
            _playlistService.PlaylistLibraryChanged+=SyncPlaylist;

        }
        [RelayCommand]
        public async Task Search(){
            var result = await _searchService.SearchAsync(Keyword);

            SearchItems.Clear();
            foreach (var v in result){
                SearchItems.Add(new BiliBiliItemNode{Title=v.Title,Bvid=v.Bvid,Author=v.Author,Duration=v.Duration,Page=1});

            }
        }
        [RelayCommand]
        public void AddToPlaybackQueue(BiliBiliItemNode node)
        {
            int p = node.IsPage ? node.Page : 1;

            this._playbackQueueService.Add(node.Title,node.Bvid,p);
        }
        [RelayCommand]
        public async Task Play(BiliBiliItemNode node)
        {
            int p=1;
            if (node.IsPage)
            {
                p=node.Page;
            }
            await _playerService.Play(node.Bvid, p);
        }
        [RelayCommand]
        public void PlaybackQueueMove((int oldIndex, int newIndex) param)
        {
            _playbackQueueService.Move(param.oldIndex, param.newIndex);
        }
        [RelayCommand]
        public void CreatePlaylist()
        {
            _playlistService.CreatePlaylist("111");
        }
        [RelayCommand]
        public void EndEditPlaylist(PlaylistLibarayItemViewModel playlist) {
            if (playlist == null) return;

            playlist.IsEditing = false;

            playlist.Source.Name = playlist.Name;

            //this.SyncPlaylist();
        }

        [ObservableProperty]
        private bool isPlaylistPopupOpen=false;
        
        [ObservableProperty]
        private PlaylistPickerViewModel? playlistPicker;
        [RelayCommand]
        public void AddToPlaylist(BiliBiliItemNode node)
        {
            int p = node.IsPage ? node.Page : 1;
        
            var track = new Track
            {
                Title = node.Title,
                Bvid = node.Bvid,
                Page = p
            };
        
            PlaylistPicker = new PlaylistPickerViewModel(_playlistService, track);
            PlaylistPicker.OnRequestClose = () =>
            {
                IsPlaylistPopupOpen = false;
            };
            IsPlaylistPopupOpen = true;
        }
        [RelayCommand]
        public void ClosePlaylistPopup()
        {
            IsPlaylistPopupOpen = false;

        }
        [ObservableProperty]
        private PlaylistLibarayItemViewModel? selectedPlaylist;
        
        [ObservableProperty]
        private ObservableCollection<Track> selectedPlaylistTracks=new();
        public void SelectedPlaylistChanged(PlaylistLibarayItemViewModel? value)
        {
            SelectedPlaylist=value;
            SelectedPlaylistTracks?.Clear();
            foreach(Track item in SelectedPlaylist.Source.Tracks) {
                SelectedPlaylistTracks.Add(item);
            }
        }
    }
}
