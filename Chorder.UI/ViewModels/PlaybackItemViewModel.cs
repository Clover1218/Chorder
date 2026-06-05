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
        public string? Author { get; set; }
        public string? Bvid { get; set; }
        public int Page { get; set; }
        public string? CoverPath { get; set; }

        [ObservableProperty]
        private string? nickname;

        public string DisplayTitle => !string.IsNullOrEmpty(Nickname) ? Nickname : (Title ?? "");

        partial void OnNicknameChanged(string? value)
        {
            OnPropertyChanged(nameof(DisplayTitle));
        }

        public int Position;

        [ObservableProperty]
        private bool isPlaying;

        [ObservableProperty]
        private bool isSelected;
    }
}
