using Chorder.Models.Entities;
using Chorder.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace Chorder.Services.Player
{
    public class PlaybackQueueService
    {
        public PlaybackQueue Queue {get;set;}= new();
        public String _queueId="420E035A-DB38-F111-B823-E8B0C52A7E74";

        public event Action? QueueChanged;
        public event Action<Track>? PlayChanged;
        
        private PlaybackQueueRepository _playbackQueueRepository;
        public PlaybackQueueService(PlaybackQueueRepository playbackQueueRepository)
        {
            _playbackQueueRepository=playbackQueueRepository;
        }
        public PlaybackQueueItem CurrentPlay() {
            return Queue.Current;
        }
        public LoopMode LoopMode {
            get => Queue.LoopMode;
            set => Queue.LoopMode=value;
        }
        public bool IsShuffle {
            get => Queue.IsShuffle;
            set => Queue.IsShuffle=value;
        }


        public void GetDataFromDatabase() {
            if(_playbackQueueRepository is null)
                return;
            var data=_playbackQueueRepository.GetAllItems(_queueId);
            if(data != null) {
                Queue.Items=data;
            }
            QueueChanged?.Invoke();
        }
        public int Add(Track track)
        {
            var entity = new PlaybackQueueItem
            {
                Track =track
            };
            entity.Position=Queue.Items.Count - 1;
            var id=_playbackQueueRepository.AddItem(_queueId,entity);
            entity.Id=id;
            Queue.Add(entity);
            QueueChanged?.Invoke();
            return entity.Position;
        }
        public void Play(int index)
        {
            System.Diagnostics.Debug.WriteLine(index);
            Queue.SetCurrent(index);
            PlayCurrent();
        }
        public void Play(PlaybackQueueItem item)
        {
            System.Diagnostics.Debug.WriteLine(Queue.Items.IndexOf(item));
            Queue.SetCurrent(Queue.Items.IndexOf(item));
            PlayCurrent();
        }
        public void PlayCurrent()
        {
            System.Diagnostics.Debug.WriteLine(Queue.Current);

            var current = Queue.Current;
            if (current == null) return;

            UpdatePlayingState();
            QueueChanged?.Invoke();
        }
        public void Remove(int index)
        {
            if (index < 0 || index >= Queue.Items.Count) return;

            var item = Queue.Items[index];
            _playbackQueueRepository.RemoveItem(item.Id);
            Queue.RemoveAt(index);

            for (int i = index; i < Queue.Items.Count; i++)
            {
                Queue.Items[i].Position = i;
                _playbackQueueRepository.UpdatePosition(Queue.Items[i].Id, i);
            }

            QueueChanged?.Invoke();
        }
        public void Clear()
        {
            _playbackQueueRepository.ClearAll(_queueId);
            Queue.Clear();
            QueueChanged?.Invoke();
        }
        private void UpdatePlayingState() {
            for (int i = 0; i < Queue.Items.Count; i++) {
                Queue.Items[i].IsPlaying = (i == Queue.CurrentIndex);
            }
        }
        public Track? AdvanceToNext() {
            Queue.PlayNext();
            var item = Queue.Current;
            if (item == null) return null;

            UpdatePlayingState();
            QueueChanged?.Invoke();
            PlayChanged?.Invoke(item.Track);
            return item.Track;
        }

        public Track? AdvanceToPrevious() {
            Queue.PlayPrevious();
            var item = Queue.Current;
            if (item == null) return null;

            UpdatePlayingState();
            QueueChanged?.Invoke();
            PlayChanged?.Invoke(item.Track);
            return item.Track;
        }
        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;
            if (newIndex > oldIndex)
                newIndex--;        
            var item = Queue.Items[oldIndex];
            _playbackQueueRepository.UpdatePosition(item.Id,newIndex);
            var itemn = Queue.Items[newIndex];
            _playbackQueueRepository.UpdatePosition(itemn.Id,oldIndex);
            Queue.Items.RemoveAt(oldIndex);
        
            // ⭐ 插入修正（关键）

        
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
