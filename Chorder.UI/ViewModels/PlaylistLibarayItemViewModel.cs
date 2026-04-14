using Chorder.Models.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.UI.ViewModels
{
    public partial class PlaylistLibarayItemViewModel : ObservableObject
    {
        public Playlist Source { get; }

        public PlaylistLibarayItemViewModel(Playlist source)
        {
            Source = source;
        }
        public string Name
        {
            get => Source.Name;
            set
            {
                if (Source.Name != value)
                {
                    Source.Name = value;
                    OnPropertyChanged();
                }
            }
        }
        // UI状态（关键！）
        [ObservableProperty]
        public bool isSelected;
        [ObservableProperty]
        public bool isEditing;
    }
}
