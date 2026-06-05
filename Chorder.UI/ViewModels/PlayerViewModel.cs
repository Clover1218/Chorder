using Chorder.Models.Entities;
using Chorder.Services.Player;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using System.Windows.Threading;
namespace Chorder.ViewModels.Player
{
    public partial class PlayerViewModel : ObservableObject
    {
        private PlayerService _player;
        private PlaybackQueueService _queue;
        private TrackInfoService _trackInfoService;

        public PlayerViewModel(PlayerService player, PlaybackQueueService queue, TrackInfoService trackInfoService)
        {
            _player = player;
            _queue = queue;
            _trackInfoService = trackInfoService;
            LoopMode = _queue.LoopMode;
            IsShuffle = _queue.IsShuffle;
            _queue.PlayChanged += OnQueuePlayChanged;
            StartTimer();
        }

        private void OnQueuePlayChanged(Track track)
        {
            IsPlaying = true;
            OnPropertyChanged(nameof(Title));
        }

        [ObservableProperty]
        private bool isPlaying;

        public string Title
        {
            get
            {
                var current = _queue.CurrentPlay();
                if (current == null) return "未播放";
                return _trackInfoService.ResolveTitle(current.Track);
            }
        }

        [RelayCommand]
        private void TogglePlay()
        {
            _player.TogglePause();
            IsPlaying = !IsPlaying;
        }

        [RelayCommand]
        private void Next()
        {
            var track = _queue.AdvanceToNext();
            if (track != null)
            {
                _player.Play(track);
                IsPlaying = true;
            }
        }

        [RelayCommand]
        private void Previous()
        {
            var track = _queue.AdvanceToPrevious();
            if (track != null)
            {
                _player.Play(track);
                IsPlaying = true;
            }
        }

        public double Position {
            get => _player.Position;
            set {
                _player.Position = value;
                OnPropertyChanged();
            }
        }

        public double CurrentTime => _player.Time;
        public double Duration => _player.Duration;

        public string CurrentTimeString => FormatTime(CurrentTime);
        public string DurationString => FormatTime(Duration);

        public double Volume {
            get => _player.Volume;
            set {
                _player.Volume = value;
                OnPropertyChanged();
            }
        }

        private DispatcherTimer _timer;

        private void StartTimer() {
            _timer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(300)
            };

            _timer.Tick += (_, _) => {
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Position));
                OnPropertyChanged(nameof(CurrentTime));
                OnPropertyChanged(nameof(Duration));
                OnPropertyChanged(nameof(CurrentTimeString));
                OnPropertyChanged(nameof(DurationString));
            };

            _timer.Start();
        }

        private string FormatTime(double seconds) {
            if (seconds <= 0 || double.IsNaN(seconds))
                return "00:00";

            var t = TimeSpan.FromSeconds(seconds);
            return $"{t.Minutes:D2}:{t.Seconds:D2}";
        }

        [ObservableProperty]
        private LoopMode _loopMode;

        partial void OnLoopModeChanged(LoopMode value)
        {
            _queue.LoopMode = value;
        }

        [ObservableProperty]
        private bool _isShuffle;

        partial void OnIsShuffleChanged(bool value)
        {
            _queue.IsShuffle = value;
        }

        [RelayCommand]
        public void ToggleLoopMode(){
            LoopMode = LoopMode switch
            {
                LoopMode.None => LoopMode.All,
                LoopMode.All => LoopMode.One,
                LoopMode.One => LoopMode.None,
                _ => LoopMode.None
            };
        }

        [RelayCommand]
        public void ToggleShuffle(){
            IsShuffle = !IsShuffle;
        }
    }
}
