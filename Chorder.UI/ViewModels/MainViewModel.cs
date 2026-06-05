using Chorder.Models.Entities;
using Chorder.Services;
using Chorder.Services.Player;
using Chorder.ViewModels.Player;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;


namespace Chorder.UI.ViewModels{
    public enum ViewMode
    {
        Playlist,
        CurrentPlay,
        Search,
        NowPlaying,
        Statistics
    }

    public partial class MainViewModel : ObservableObject{

        private readonly SearchService _searchService;
        private readonly PlayerService _playerService;
        private readonly PlaybackQueueService _playbackQueueService;
        private readonly PlaylistService _playlistService;
        private readonly TrackInfoService _trackInfoService;
        public ObservableCollection<BiliBiliItemNode> SearchItems { get; } = new();
        public ObservableCollection<PlaybackItemViewModel> PlaybackQueue { get; } = new();

        public ObservableCollection<PlaylistLibarayItemViewModel> Playlists{ get; } = new();
        public ObservableCollection<PlaylistItemViewModel> PlaylistItems{get; } = new();
        public class BiliBiliItemNode{
            public string? Bvid { get; set; }
            public string? Title { get; set; }
            public string? Author { get; set; }
            public string? Duration { get; set; }
            public string? CoverPath { get; set; }
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
                    Author=node.Author,
                    CoverPath=p.CoverPath
                });
            }
            node.IsLoaded = true;
        }
        private void SyncQueue()
        {
            var source = _playbackQueueService.Queue.Items;
            Application.Current.Dispatcher.Invoke(()=>{
                PlaybackQueue.Clear();

                foreach (var item in source)
                {
                    var nickname = _trackInfoService.GetNickname(item.Track.Bvid, item.Track.Page);
                    var coverPath = item.Track.CoverPath ?? _trackInfoService.GetCoverPath(item.Track.Bvid, item.Track.Page);
                    PlaybackQueue.Add(new PlaybackItemViewModel
                    {
                        Title = item.Track.Title,
                        Author = item.Track.Author,
                        Bvid = item.Track.Bvid,
                        Page = item.Track.Page,
                        CoverPath = coverPath,
                        Position = item.Position,
                        IsPlaying = item.IsPlaying,
                        Nickname = nickname,
                    });
                }
            });
            RefreshNowPlaying();
        }
        private void SyncPlaylist()
        {
            var selectedId = SelectedPlaylist?.Source.Id;
            Playlists.Clear();
            PlaylistLibarayItemViewModel? newSelected = null;
            foreach (var item in _playlistService.Library.Playlists) {
                var vm = new PlaylistLibarayItemViewModel(item){
                    Name = item.Name,
                };
                Playlists.Add(vm);
                if (selectedId.HasValue && item.Id == selectedId.Value)
                    newSelected = vm;
            }
            if (newSelected != null)
                SelectedPlaylist = newSelected;
        }
        [ObservableProperty]
        private string keyword;
        [ObservableProperty]
        private string selectedPlaylistIndex;
        [ObservableProperty]
        private PlayerViewModel playerDataContext;
        [ObservableProperty]
        private StatisticsViewModel statisticsViewModel;
        [ObservableProperty]
        private ViewMode currentView = ViewMode.Search;
        public MainViewModel(
            SearchService searchService,PlayerService playerService,
            PlaybackQueueService playbackQueueService,PlaylistService playlistService,
            TrackInfoService trackInfoService, StatisticsService statisticsService)
        {
            _searchService = searchService;
            _playerService = playerService;
            _playbackQueueService = playbackQueueService;
            _playbackQueueService.QueueChanged += SyncQueue;
            _playlistService=playlistService;
            _playlistService.PlaylistLibraryChanged+=SyncPlaylist;
            _playlistService.LoadFromDatabase();
            _trackInfoService=trackInfoService;
            _playbackQueueService.PlayChanged += OnPlayChanged;
            _playerService.PlayEnded += OnPlayerPlayEnded;
            playerDataContext=new PlayerViewModel(playerService,playbackQueueService,trackInfoService);
            statisticsViewModel = new StatisticsViewModel(statisticsService);
            _playbackQueueService.GetDataFromDatabase();
        }
        [RelayCommand]
        public async Task Search(){
            var result = await _searchService.SearchAsync(Keyword);
            SearchItems.Clear();
            foreach (var v in result)
            {
                SearchItems.Add(new BiliBiliItemNode{Title=v.Title,Bvid=v.Bvid,Author=v.Author,Duration=v.Duration,Page=1,CoverPath=v.CoverPath});
            }
        }
        [RelayCommand]
        public void AddToPlaybackQueue(BiliBiliItemNode node)
        {
            int p = node.IsPage ? node.Page : 1;
            Track track=new Track(){Title=node.Title,Author=node.Author,Bvid=node.Bvid,Page=p,CoverPath=node.CoverPath};
            _trackInfoService.EnsureTrack(track);
            this._playbackQueueService.Add(track);
        }
        [RelayCommand]
        public void PlayFromSearch(BiliBiliItemNode node)
        {
            int p=1;
            if (node.IsPage)
            {
                p=node.Page;
            }
            Track track=new Track(){Title=node.Title,Author=node.Author,Bvid=node.Bvid,Page=p,CoverPath=node.CoverPath};
            _trackInfoService.EnsureTrack(track);
            int index = _playbackQueueService.Add(track);

            _playbackQueueService.Play(index);
            PlaybackQueueItem current=_playbackQueueService.CurrentPlay();
            _playerService.Play(current.Track);
        }
        [RelayCommand]
        public void PlaybackQueueMove((int oldIndex, int newIndex) param)
        {
            _playbackQueueService.Move(param.oldIndex, param.newIndex);
        }
        [RelayCommand]
        public void CreatePlaylist()
        {
            _playlistService.CreatePlaylist("新歌单");
        }
        [RelayCommand]
        public void DeletePlaylist(PlaylistLibarayItemViewModel playlist)
        {
            if (playlist == null) return;

            _playlistService.DeletePlaylist(playlist.Source);

            if (SelectedPlaylist == playlist)
            {
                SelectedPlaylist = null;
                SelectedPlaylistTracks.Clear();
            }
        }

        [ObservableProperty]
        private bool isPlaylistPopupOpen=false;

        [ObservableProperty]
        private PlaylistPickerViewModel? playlistPicker;

        private void OpenPlaylistPopup(Track track)
        {
            PlaylistPicker = new PlaylistPickerViewModel(_playlistService, track);
            PlaylistPicker.OnRequestClose = () =>
            {
                IsPlaylistPopupOpen = false;
            };
            IsPlaylistPopupOpen = true;
        }

        [RelayCommand]
        public void AddToPlaylist(BiliBiliItemNode node)
        {
            int p = node.IsPage ? node.Page : 1;

            var track = new Track
            {
                Title = node.Title,
                Author = node.Author,
                Bvid = node.Bvid,
                Page = p,
                CoverPath = node.CoverPath
            };
            _trackInfoService.EnsureTrack(track);
            OpenPlaylistPopup(track);
        }

        [RelayCommand]
        public void AddPlaybackItemToPlaylist(PlaybackItemViewModel item)
        {
            var track = new Track
            {
                Title = item.Title,
                Author = item.Author,
                Bvid = item.Bvid,
                Page = item.Page,
                CoverPath = item.CoverPath
            };
            _trackInfoService.EnsureTrack(track);
            OpenPlaylistPopup(track);
        }

        [RelayCommand]
        public void AddPlaylistTrackToPlaylist(PlaylistTrackItemViewModel item)
        {
            _trackInfoService.EnsureTrack(item.Source);
            OpenPlaylistPopup(item.Source);
        }

        [RelayCommand]
        public void ClosePlaylistPopup()
        {
            IsPlaylistPopupOpen = false;

        }

        [RelayCommand]
        public void RemovePlaybackQueueItem(PlaybackItemViewModel item)
        {
            if (item == null) return;
            _playbackQueueService.Remove(item.Position);
        }

        [RelayCommand]
        public void RemoveSelectedPlaylistTrack(PlaylistTrackItemViewModel item)
        {
            if (item == null || SelectedPlaylist == null) return;

            _playlistService.RemoveTrack(SelectedPlaylist.Source, item.Source);
            SelectedPlaylistTracks.Remove(item);
        }

        [ObservableProperty]
        private PlaylistLibarayItemViewModel? selectedPlaylist;

        [ObservableProperty]
        private ObservableCollection<PlaylistTrackItemViewModel> selectedPlaylistTracks=new();

        // ===== 当前播放信息 =====
        [ObservableProperty]
        private string nowPlayingTitle = "";

        [ObservableProperty]
        private string nowPlayingOriginalTitle = "";

        [ObservableProperty]
        private string nowPlayingAuthor = "";

        [ObservableProperty]
        private string nowPlayingBvid = "";

        [ObservableProperty]
        private int nowPlayingPage;

        [ObservableProperty]
        private bool nowPlayingIsCached;

        [ObservableProperty]
        private bool nowPlayingIsDownloading;

        [ObservableProperty]
        private string? nowPlayingCoverPath;

        private void OnPlayChanged(Track track)
        {
            RefreshNowPlaying();
        }

        private void OnPlayerPlayEnded()
        {
            var nextTrack = _playbackQueueService.AdvanceToNext();
            if (nextTrack != null)
            {
                _playerService.Play(nextTrack);
                RefreshNowPlaying();
            }
        }

        public void RefreshNowPlaying()
        {
            var current = _playbackQueueService.CurrentPlay();
            if (current == null)
            {
                NowPlayingTitle = "";
                NowPlayingOriginalTitle = "";
                NowPlayingAuthor = "";
                NowPlayingBvid = "";
                NowPlayingPage = 0;
                NowPlayingIsCached = false;
                NowPlayingCoverPath = null;
                return;
            }

            var track = current.Track;
            var nickname = _trackInfoService.GetNickname(track.Bvid, track.Page);
            NowPlayingTitle = !string.IsNullOrEmpty(nickname) ? nickname : track.Title;
            NowPlayingOriginalTitle = track.Title;
            NowPlayingAuthor = track.Author;
            NowPlayingBvid = track.Bvid;
            NowPlayingPage = track.Page;
            NowPlayingIsCached = TrackInfoService.IsFileCached(track.Bvid, track.Page);
            // 优先使用 track 的 CoverPath，否则从数据库获取
            NowPlayingCoverPath = track.CoverPath ?? _trackInfoService.GetCoverPath(track.Bvid, track.Page);
        }

        partial void OnSelectedPlaylistChanged(PlaylistLibarayItemViewModel? value)
        {
            SelectedPlaylistTracks?.Clear();
            if (value != null)
            {
                foreach(Track source in SelectedPlaylist.Source.Tracks) {
                    var nickname = _trackInfoService.GetNickname(source.Bvid, source.Page);
                    SelectedPlaylistTracks.Add(new PlaylistTrackItemViewModel(source)
                    {
                        Nickname = nickname,
                    });
                }
            }
        }
        [RelayCommand]
        public void PlayFromPlaybackQueue(PlaybackItemViewModel node)
        {
            Track track=new Track(){Title=node.Title,Bvid=node.Bvid,Page=node.Page, Author=node.Author};
            System.Diagnostics.Debug.WriteLine(node.Position);
            _playbackQueueService.Play(node.Position);
            PlaybackQueueItem current=_playbackQueueService.CurrentPlay();
            _playerService.Play(current.Track);
        }

        [RelayCommand]
        public void PlayTrack(PlaylistTrackItemViewModel node)
        {
            Track track = new Track
            {
                Title = node.Title,
                Bvid = node.Bvid,
                Page = node.Page,
                Author = node.Author,
            };
            int index = _playbackQueueService.Add(track);
            _playbackQueueService.Play(index);
            PlaybackQueueItem current = _playbackQueueService.CurrentPlay();
            _playerService.Play(current.Track);
        }

        [RelayCommand]
        public void AddTrackToQueue(PlaylistTrackItemViewModel node)
        {
            var track = new Track
            {
                Title = node.Title,
                Bvid = node.Bvid,
                Page = node.Page,
                Author = node.Author,
            };
            _trackInfoService.EnsureTrack(track);
            _playbackQueueService.Add(track);
        }

        [RelayCommand]
        public void PlaylistMove((int oldIndex, int newIndex) param)
        {
            var list = _playlistService.Library.Playlists;
            if (param.oldIndex < 0 || param.oldIndex >= list.Count) return;
            if (param.newIndex < 0 || param.newIndex >= list.Count) return;

            var item = list[param.oldIndex];
            list.RemoveAt(param.oldIndex);
            list.Insert(param.newIndex, item);

            for (int i = 0; i < list.Count; i++)
            {
                _playlistService.UpdatePlaylistPosition(list[i], i);
            }

            PlaylistLibraryChanged();
        }

        [RelayCommand]
        public void PlaylistTrackMove((int oldIndex, int newIndex) param)
        {
            if (SelectedPlaylist == null) return;

            var list = SelectedPlaylist.Source.Tracks;
            if (param.oldIndex < 0 || param.oldIndex >= list.Count) return;
            if (param.newIndex < 0 || param.newIndex >= list.Count) return;

            var track = list[param.oldIndex];
            list.RemoveAt(param.oldIndex);
            list.Insert(param.newIndex, track);

            var vm = SelectedPlaylistTracks[param.oldIndex];
            SelectedPlaylistTracks.RemoveAt(param.oldIndex);
            SelectedPlaylistTracks.Insert(param.newIndex, vm);

            for (int i = 0; i < list.Count; i++)
            {
                _playlistService.UpdateTrackPosition(list[i], i);
            }
        }

        // ===== 重命名弹出框 =====
        [ObservableProperty]
        private bool isRenamePopupOpen = false;

        [ObservableProperty]
        private string renameText = "";

        private object? _renameTarget;

        [RelayCommand]
        public void OpenRenamePlaybackItem(PlaybackItemViewModel item)
        {
            if (item == null) return;
            _renameTarget = item;
            RenameText = item.Nickname ?? item.Title ?? "";
            IsRenamePopupOpen = true;
        }

        [RelayCommand]
        public void OpenRenamePlaylistTrack(PlaylistTrackItemViewModel item)
        {
            if (item == null) return;
            _renameTarget = item;
            RenameText = item.Nickname ?? item.Title;
            IsRenamePopupOpen = true;
        }

        [RelayCommand]
        public void OpenRenamePlaylist(PlaylistLibarayItemViewModel item)
        {
            if (item == null) return;
            _renameTarget = item;
            RenameText = item.Name;
            IsRenamePopupOpen = true;
        }

        [RelayCommand]
        public void ConfirmRename()
        {
            if (_renameTarget == null) return;

            if (_renameTarget is PlaybackItemViewModel playbackItem)
            {
                _trackInfoService.UpdateNickname(playbackItem.Bvid, playbackItem.Page, RenameText);
                playbackItem.Nickname = RenameText;
            }
            else if (_renameTarget is PlaylistTrackItemViewModel playlistItem)
            {
                _trackInfoService.UpdateNickname(playlistItem.Bvid, playlistItem.Page, RenameText);
                playlistItem.Nickname = RenameText;
            }
            else if (_renameTarget is PlaylistLibarayItemViewModel playlistLibItem)
            {
                _playlistService.UpdatePlaylistName(playlistLibItem.Source, RenameText);
                playlistLibItem.Name = RenameText;
            }

            _renameTarget = null;
            IsRenamePopupOpen = false;
        }

        [RelayCommand]
        public void CancelRename()
        {
            _renameTarget = null;
            IsRenamePopupOpen = false;
        }

        private void PlaylistLibraryChanged()
        {
            var selectedId = SelectedPlaylist?.Source.Id;
            Playlists.Clear();
            PlaylistLibarayItemViewModel? newSelected = null;
            foreach (var item in _playlistService.Library.Playlists)
            {
                var vm = new PlaylistLibarayItemViewModel(item)
                {
                    Name = item.Name,
                };
                Playlists.Add(vm);
                if (selectedId.HasValue && item.Id == selectedId.Value)
                    newSelected = vm;
            }
            if (newSelected != null)
                SelectedPlaylist = newSelected;
        }

        [RelayCommand]
        public void SwitchToPlaylist()
        {
            CurrentView = ViewMode.Playlist;
        }

        [RelayCommand]
        public void SwitchToCurrentPlay()
        {
            CurrentView = ViewMode.CurrentPlay;
        }

        [RelayCommand]
        public void SwitchToSearch()
        {
            CurrentView = ViewMode.Search;
        }

        [RelayCommand]
        public void SwitchToNowPlaying()
        {
            RefreshNowPlaying();
            CurrentView = ViewMode.NowPlaying;
        }

        [RelayCommand]
        public void SwitchToStatistics()
        {
            CurrentView = ViewMode.Statistics;
        }

        [RelayCommand]
        public async Task DownloadCache()
        {
            if (string.IsNullOrEmpty(NowPlayingBvid)) return;
            NowPlayingIsDownloading = true;
            var result = await _trackInfoService.DownloadCacheAsync(NowPlayingBvid, NowPlayingPage);
            NowPlayingIsDownloading = false;
            NowPlayingIsCached = result != null;
        }

        [RelayCommand]
        public void DeleteCache()
        {
            if (string.IsNullOrEmpty(NowPlayingBvid)) return;
            _trackInfoService.DeleteCache(NowPlayingBvid, NowPlayingPage);
            NowPlayingIsCached = false;
        }
    }
}
