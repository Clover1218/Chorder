using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Chorder.UI.ViewModels
{
    public partial class PlaybackItemViewModel : ObservableObject
    {
        public string? Title { get; set; }
        public string? Bvid { get; set; }
        public int Page { get; set; }

        // UI状态（关键！）
        [ObservableProperty]
        private bool isPlaying;

        [ObservableProperty]
        private bool isSelected;
    }
}
