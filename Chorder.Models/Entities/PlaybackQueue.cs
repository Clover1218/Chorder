using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.Models.Entities
{
    public class PlaybackQueue{
        public List<PlaybackQueueItem> Items { get; set; } = new();
        public LoopMode LoopMode{ get; set; }=LoopMode.All ;
        public bool IsShuffle { get; set; } = false;
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
        
            if (LoopMode == LoopMode.One)
                return;
        
            if (IsShuffle)
            {
                if (_shuffleOrder.Count != Items.Count)
                    GenerateShuffleOrder();
        
                _shuffleIndex++;
        
                if (_shuffleIndex >= _shuffleOrder.Count)
                {
                    if (LoopMode == LoopMode.All)
                    {
                        GenerateShuffleOrder(); // 重新洗牌
                    }
                    else
                    {
                        return; // None 模式停止
                    }
                }
        
                CurrentIndex = _shuffleOrder[_shuffleIndex];
                return;
            }
        
            // 🎯 普通模式
            CurrentIndex++;
        
            if (CurrentIndex >= Items.Count)
            {
                if (LoopMode == LoopMode.All)
                    CurrentIndex = 0;
                else
                    CurrentIndex = Items.Count - 1; // 停在最后
            }
        }

        public void PlayPrevious()
        {
            if (Items.Count == 0) return;

            if (LoopMode == LoopMode.One)
                return;

            if (IsShuffle)
            {
                if (_shuffleOrder.Count != Items.Count)
                    GenerateShuffleOrder();

                _shuffleIndex--;

                if (_shuffleIndex < 0)
                {
                    if (LoopMode == LoopMode.All)
                    {
                        _shuffleIndex = _shuffleOrder.Count - 1;
                    }
                    else
                    {
                        _shuffleIndex = 0;
                        return;
                    }
                }

                CurrentIndex = _shuffleOrder[_shuffleIndex];
                return;
            }

            CurrentIndex--;

            if (CurrentIndex < 0)
            {
                if (LoopMode == LoopMode.All)
                    CurrentIndex = Items.Count - 1;
                else
                    CurrentIndex = 0;
            }
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
        private List<int> _shuffleOrder = new();
        private int _shuffleIndex = -1;
        private void GenerateShuffleOrder()
        {
            var rand = new Random();
        
            _shuffleOrder = Enumerable.Range(0, Items.Count)
                .OrderBy(_ => rand.Next())
                .ToList();
        
            _shuffleIndex = 0;
        }
    }
    public class PlaybackQueueItem { 
        public Track Track{ get; set;} 
        public bool IsCurrent { get; set; } 
        public Guid Id { get; set; }
        public int Position;
        public bool IsPlaying { get; set; }
    }
    public enum LoopMode
    {
        None,
        All,
        One
    }
}
