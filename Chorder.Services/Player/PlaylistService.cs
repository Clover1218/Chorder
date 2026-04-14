using Chorder.Models.Entities;
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

        public Playlist CreatePlaylist(string name)
        {
            var p = new Playlist { Name = name };
            Library.Playlists.Add(p);

            PlaylistLibraryChanged?.Invoke();
            return p;
        }

        public void DeletePlaylist(Playlist playlist)
        {
            Library.Playlists.Remove(playlist);
            PlaylistLibraryChanged?.Invoke();
        }

        public void AddTrack(Playlist playlist, Track track)
        {
            playlist.Tracks.Add(track);
            PlaylistLibraryChanged?.Invoke();
        }

        public void RemoveTrack(Playlist playlist, Track track)
        {
            playlist.Tracks.Remove(track);
            PlaylistLibraryChanged?.Invoke();
        }
    }
}
