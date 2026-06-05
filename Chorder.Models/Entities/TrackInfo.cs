using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.Models.Entities
{
    public class TrackInfo
    {
        public Guid Id { get; set; }
        public string Bvid { get; set; } = "";
        public int Page { get; set; } = 1;
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string? Nickname { get; set; } = "";
        public string? CachePath { get; set; } = "";
        public string? CoverPath { get; set; } = "";
    }
}
