using Chorder.Services.Player;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Chorder.UI.ViewModels {
public partial class PlayerViewModel : ObservableObject
{
    private readonly PlayerService _player;

    public PlayerViewModel(PlayerService player)
    {
        _player = player;

        StartTimer();
    }

    // ======================
    // 播放状态
    // ======================

    [ObservableProperty]
    private bool isPlaying;

    [RelayCommand]
    private void TogglePlay()
    {
        _player.TogglePause();
        IsPlaying = !IsPlaying;
    }

    // ======================
    // 进度
    // ======================

    public double Position
    {
        get => _player.Position;
        set
        {
            _player.Position = value;
            OnPropertyChanged();
        }
    }

    public double Duration => _player.Duration;
    public double CurrentTime => _player.Time;

    // ======================
    // 音量
    // ======================

    public double Volume
    {
        get => _player.Volume;
        set
        {
            _player.Volume = value;
            OnPropertyChanged();
        }
    }

    // ======================
    // 定时刷新（关键）
    // ======================

    private DispatcherTimer _timer;

    private void StartTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };

        _timer.Tick += (_, _) =>
        {
            OnPropertyChanged(nameof(Position));
            OnPropertyChanged(nameof(CurrentTime));
            OnPropertyChanged(nameof(Duration));
        };

        _timer.Start();
    }
}
}
