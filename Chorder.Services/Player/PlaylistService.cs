using Chorder.Models.Entities;
using Chorder.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.Services.Player
{
    public class PlaylistService
    {
        public PlaylistLibrary Library { get; private set; } = new();

        public event Action? PlaylistLibraryChanged;

        private readonly PlaylistRepository _playlistRepository;

        public PlaylistService(PlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public void LoadFromDatabase()
        {
            if (_playlistRepository is null)
                return;

            var data = _playlistRepository.GetAllPlaylists();
            if (data != null)
            {
                Library.Playlists = data;
            }
            PlaylistLibraryChanged?.Invoke();
        }

        public Playlist CreatePlaylist(string name)
        {
            var p = new Playlist
            {
                Name = name,
                UpdatedAt = DateTime.Now
            };

            _playlistRepository.AddPlaylist(p);
            Library.Playlists.Add(p);

            PlaylistLibraryChanged?.Invoke();
            return p;
        }

        public void DeletePlaylist(Playlist playlist)
        {
            _playlistRepository.DeletePlaylist(playlist.Id);
            Library.Playlists.Remove(playlist);

            for (int i = 0; i < Library.Playlists.Count; i++)
            {
                UpdatePlaylistPosition(Library.Playlists[i], i);
            }

            PlaylistLibraryChanged?.Invoke();
        }

        public void UpdatePlaylistName(Playlist playlist, string name)
        {
            playlist.Name = name;
            _playlistRepository.UpdatePlaylistName(playlist.Id, name);
            PlaylistLibraryChanged?.Invoke();
        }

        public void AddTrack(Playlist playlist, Track track)
        {
            _playlistRepository.AddTrack(playlist.Id, track);
            playlist.Tracks.Add(track);
            PlaylistLibraryChanged?.Invoke();
        }

        public void RemoveTrack(Playlist playlist, Track track)
        {
            _playlistRepository.RemoveTrack(track.Id);
            playlist.Tracks.Remove(track);

            for (int i = 0; i < playlist.Tracks.Count; i++)
            {
                UpdateTrackPosition(playlist.Tracks[i], i);
            }

            PlaylistLibraryChanged?.Invoke();
        }

        public void UpdatePlaylistPosition(Playlist playlist, int newPosition)
        {
            _playlistRepository.UpdatePlaylistPosition(playlist.Id, newPosition);
            playlist.Position = newPosition;
        }

        public void UpdateTrackPosition(Track track, int newPosition)
        {
            _playlistRepository.UpdateTrackPosition(track.Id, newPosition);
            track.Position = newPosition;
        }
    }
}
