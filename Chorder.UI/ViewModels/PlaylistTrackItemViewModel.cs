using Chorder.Models.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.UI.ViewModels
{
    public partial class PlaylistTrackItemViewModel : ObservableObject
    {
        public Track Source { get; }

        public PlaylistTrackItemViewModel(Track source)
        {
            Source = source;
        }

        public string Title
        {
            get => Source.Title;
            set
            {
                if (Source.Title != value)
                {
                    Source.Title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Bvid => Source.Bvid;
        public int Page => Source.Page;
        public string Author => Source.Author;

        [ObservableProperty]
        private string? nickname;

        public string DisplayTitle => !string.IsNullOrEmpty(Nickname) ? Nickname : Title;

        partial void OnNicknameChanged(string? value)
        {
            OnPropertyChanged(nameof(DisplayTitle));
        }
    }
}
