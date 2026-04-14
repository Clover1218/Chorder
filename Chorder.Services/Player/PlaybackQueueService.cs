using Chorder.Models.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Chorder.Services.Player
{
    public class PlaybackQueueService
    {
        public PlaybackQueue Queue {get;set;}= new();

        public event Action? QueueChanged;
        public PlaybackQueueService()
        {
        }

        public void Add(string title, string bvid, int page)
        {
            var entity = new PlaybackQueueItem
            {
                track = new Track
                {
                    Bvid = bvid,
                    Title = title,
                    Page = page
                }
            };

            Queue.Add(entity);
            QueueChanged?.Invoke();
        }
        public async Task Play(int index)
        {
            //_queue.SetCurrent(index);
            //await PlayCurrent();
        }
        public async Task PlayCurrent()
        {
            //var current = _queue.Current;
            //if (current == null) return;

            //UpdatePlayingState();

            //await _playerService.Play(
            //    current.track.Bvid,
            //    current.track.Page);
        }
        public void Remove(int index)
        {
            Queue.RemoveAt(index);
            QueueChanged?.Invoke();
        }
        public void Clear()
        {
            Queue.Clear();
            QueueChanged?.Invoke();
        }
        //private void UpdatePlayingState()
        //{
        //    for (int i = 0; i < ViewItems.Count; i++)
        //    {
        //        ViewItems[i].IsPlaying = (i == _queue.CurrentIndex);
        //    }
        //}
        //private async void OnPlaybackEnded(object? sender, EventArgs e)
        //{
        //    await PlayNext();
        //}
        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;
        
            var item = Queue.Items[oldIndex];
        
            Queue.Items.RemoveAt(oldIndex);
        
            // ⭐ 插入修正（关键）
            if (newIndex > oldIndex)
                newIndex--;
        
            Queue.Items.Insert(newIndex, item);
        
            // ⭐ 当前播放 index 修正（核心🔥）
        
            if (Queue.CurrentIndex == oldIndex)
            {
                // 当前播放项被拖动
                Queue.SetCurrent(newIndex);
            }
            else
            {
                // 被插入影响 index
                if (oldIndex < Queue.CurrentIndex && newIndex >= Queue.CurrentIndex)
                {
                    Queue.SetCurrent(Queue.CurrentIndex - 1);
                }
                else if (oldIndex > Queue.CurrentIndex && newIndex <= Queue.CurrentIndex)
                {
                    Queue.SetCurrent(Queue.CurrentIndex + 1);
                }
            }
            QueueChanged?.Invoke();
        }
    }
}
