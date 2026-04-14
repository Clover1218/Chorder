using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.Models.Entities
{
    public class Playlist{
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public List<Track> Tracks { get; set; } = new();

    }
    public class PlaylistLibrary
    {
        public List<Playlist> Playlists { get; set; } = new();
    }
}
