using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.Models.Entities
{
    public class Playlist{
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Position { get; set; }
        public List<Track> Tracks { get; set; } = new();

    }
    public class PlaylistLibrary
    {
        public List<Playlist> Playlists { get; set; } = new();
    }
}
