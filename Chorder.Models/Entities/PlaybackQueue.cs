using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.Models.Entities
{
    public class PlaybackQueue{
        public List<PlaybackQueueItem> Items { get; set; } = new();

        public int CurrentIndex { get; private set; } = -1;

        public PlaybackQueueItem? Current =>
            (CurrentIndex >= 0 && CurrentIndex < Items.Count)
            ? Items[CurrentIndex]
            : null;

        public void Add(PlaybackQueueItem item)
        {
            Items.Add(item);
        }

        public void SetCurrent(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
                CurrentIndex = index;
            }
        }

        public void PlayNext()
        {
            if (Items.Count == 0) return;

            CurrentIndex++;

            if (CurrentIndex >= Items.Count)
                CurrentIndex = 0; // 循环
        }

        public void PlayPrevious()
        {
            if (Items.Count == 0) return;

            CurrentIndex--;

            if (CurrentIndex < 0)
                CurrentIndex = Items.Count - 1;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Items.Count) return;

            Items.RemoveAt(index);

            if (index < CurrentIndex)
                CurrentIndex--;
            else if (index == CurrentIndex)
                CurrentIndex = -1;
        }

        public void Clear()
        {
            Items.Clear();
            CurrentIndex = -1;
        }
    }
    public class PlaybackQueueItem { 
        public Track track{ get; set;} 
        public bool IsCurrent { get; set; } 
    }
}
