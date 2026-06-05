using Chorder.Repository;
using Chorder.Services.Player;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Chorder.UI.ViewModels
{
    public partial class StatisticsViewModel : ObservableObject
    {
        private readonly StatisticsService _statisticsService;

        public ObservableCollection<StatPageViewModel> Pages { get; } = new();

        [ObservableProperty]
        private int currentPageIndex;

        [ObservableProperty]
        private StatPageViewModel? currentPage;

        public StatisticsViewModel(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
            InitializePages();
        }

        private void InitializePages()
        {
            Pages.Add(new TopPlayedPage(_statisticsService));
            Pages.Add(new PlayHistoryPage(_statisticsService));
            Pages.Add(new SummaryPage(_statisticsService));

            if (Pages.Count > 0)
            {
                CurrentPageIndex = 0;
                CurrentPage = Pages[0];
            }
        }

        [RelayCommand]
        public void SwitchPage(object parameter)
        {
            if (int.TryParse(parameter?.ToString(), out int index) && index >= 0 && index < Pages.Count)
            {
                CurrentPageIndex = index;
                CurrentPage = Pages[index];
                CurrentPage?.Refresh();
            }
        }

        [RelayCommand]
        public void NextPage()
        {
            if (CurrentPageIndex < Pages.Count - 1)
            {
                SwitchPage(CurrentPageIndex + 1);
            }
        }

        [RelayCommand]
        public void PreviousPage()
        {
            if (CurrentPageIndex > 0)
            {
                SwitchPage(CurrentPageIndex - 1);
            }
        }

        [RelayCommand]
        public void RefreshCurrentPage()
        {
            CurrentPage?.Refresh();
        }
    }

    public abstract class StatPageViewModel : ObservableObject
    {
        public abstract string Title { get; }

        public abstract void Refresh();
    }

    public partial class TopPlayedPage : StatPageViewModel
    {
        private readonly StatisticsService _statisticsService;

        public override string Title => "播放最多统计";

        public ObservableCollection<PlayCountStat> TopTracks { get; } = new();

        [ObservableProperty]
        private PlayCountStat? firstPlace;

        [ObservableProperty]
        private PlayCountStat? secondPlace;

        [ObservableProperty]
        private PlayCountStat? thirdPlace;

        public TopPlayedPage(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        public override void Refresh()
        {
            TopTracks.Clear();
            var data = _statisticsService.GetTopPlayedTracks(10);
            foreach (var item in data)
            {
                TopTracks.Add(item);
            }

            // 更新前三名
            FirstPlace = data.Count > 0 ? data[0] : null;
            SecondPlace = data.Count > 1 ? data[1] : null;
            ThirdPlace = data.Count > 2 ? data[2] : null;
        }
    }

    public partial class PlayHistoryPage : StatPageViewModel
    {
        private readonly StatisticsService _statisticsService;

        public override string Title => "近期播放记录";

        public ObservableCollection<PlayDateStat> DailyStats { get; } = new();

        public PlayHistoryPage(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        public override void Refresh()
        {
            DailyStats.Clear();
            var data = _statisticsService.GetRecentPlayHistory(7);
            foreach (var item in data)
            {
                DailyStats.Add(item);
            }
        }
    }

    public partial class SummaryPage : StatPageViewModel
    {
        private readonly StatisticsService _statisticsService;

        public override string Title => "总体统计";

        [ObservableProperty]
        private int totalPlayCount;

        [ObservableProperty]
        private string totalPlayTime = string.Empty;

        public SummaryPage(StatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        public override void Refresh()
        {
            TotalPlayCount = _statisticsService.GetTotalPlayCount();
            var totalSeconds = _statisticsService.GetTotalPlayedSeconds();
            var ts = TimeSpan.FromSeconds(totalSeconds);
            TotalPlayTime = $"{ts.Days}天 {ts.Hours}小时 {ts.Minutes}分钟";
        }
    }
}