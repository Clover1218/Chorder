using Chorder.Repository;
using System.Collections.Generic;

namespace Chorder.Services.Player
{
    public class StatisticsService
    {
        private readonly StatisticsRepository _repository;

        public StatisticsService(StatisticsRepository repository)
        {
            _repository = repository;
        }

        public List<PlayCountStat> GetTopPlayedTracks(int limit = 10)
        {
            return _repository.GetTopPlayCount(limit);
        }

        public List<PlayDateStat> GetRecentPlayHistory(int days = 7)
        {
            return _repository.GetPlayCountByDate(days);
        }

        public int GetTotalPlayCount()
        {
            return _repository.GetTotalPlayCount();
        }

        public double GetTotalPlayedSeconds()
        {
            return _repository.GetTotalPlayedSeconds();
        }
    }
}