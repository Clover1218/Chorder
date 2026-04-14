using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.UI.ViewModels
{
    public partial class PlaylistItemViewModel : ObservableObject
    {
        public string? Name { get; set; }
        // UI状态（关键！）
        [ObservableProperty]
        private bool isSelected;
    
    }
}
