using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
namespace Chorder.Models.Entities
{
    internal class SearchItem
    {
    }
    public class BiliBiliSearchItem
    {   
        public string? Bvid { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Duration { get; set; }
    }
    public class BiliBiliPageItem
    {   
        public int Page { get; set; }
        public string? Title { get; set; }
        public int? Duration { get; set; }
    }
}
