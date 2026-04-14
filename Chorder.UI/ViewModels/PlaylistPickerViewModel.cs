using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chorder.Services.Player;
using Chorder.Models.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Chorder.UI.ViewModels {


public partial class PlaylistPickerViewModel : ObservableObject
{
    private readonly PlaylistService _playlistService;
    private readonly Track _track;

    public ObservableCollection<PlaylistLibarayItemViewModel> Playlists { get; } = new();
    public Action? OnRequestClose;
    public PlaylistPickerViewModel(PlaylistService playlistService, Track track)
    {
        _playlistService = playlistService;
        _track = track;

        foreach (var p in playlistService.Library.Playlists)
        {
            Playlists.Add(new PlaylistLibarayItemViewModel(p)
            {
                Name = p.Name,
            });
        }
    }

    [RelayCommand]
    public void AddToPlaylist(PlaylistLibarayItemViewModel playlist)
    {
        playlist.Source.Tracks.Add(_track);
        OnRequestClose?.Invoke();
    }
}
}
