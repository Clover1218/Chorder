using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace Chorder.Repository
{
    public class StatisticsRepository
    {
        private readonly SQLServerConnectionFactory _factory;

        public StatisticsRepository(SQLServerConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<PlayCountStat> GetTopPlayCount(int limit = 10)
        {
            var result = new List<PlayCountStat>();
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT TOP (@Limit) a.播放总秒数, COALESCE(NULLIF(b.nickname, ''), b.Title) AS 名称, a.Bvid, a.Page, b.CoverPath
                FROM (
                    SELECT SUM(PlayedSeconds) AS 播放总秒数, Page, Bvid
                    FROM PlayHistory
                    GROUP BY Page, Bvid
                ) a
                INNER JOIN TrackInfo b ON a.Bvid=b.Bvid AND a.Page=b.Page
                ORDER BY a.播放总秒数 DESC
            ", conn);
            cmd.Parameters.AddWithValue("@Limit", limit);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PlayCountStat
                {
                    PlayedSeconds = reader.GetDouble(0),
                    Name = reader.GetString(1),
                    Bvid = reader.GetString(2),
                    Page = reader.GetInt32(3),
                    CoverPath = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }
            return result;
        }

        public List<PlayDateStat> GetPlayCountByDate(int days = 7)
        {
            var result = new List<PlayDateStat>();
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT CONVERT(date, PlayedAt) AS 日期, COUNT(*) AS 播放次数
                FROM PlayHistory
                WHERE PlayedAt >= DATEADD(day, -@Days, GETDATE())
                GROUP BY CONVERT(date, PlayedAt)
                ORDER BY 日期 DESC
            ", conn);
            cmd.Parameters.AddWithValue("@Days", days);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PlayDateStat
                {
                    Date = reader.GetDateTime(0).Date,
                    PlayCount = reader.GetInt32(1)
                });
            }
            return result;
        }

        public int GetTotalPlayCount()
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM PlayHistory
            ", conn);
            return (int)cmd.ExecuteScalar();
        }

        public double GetTotalPlayedSeconds()
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT COALESCE(SUM(PlayedSeconds), 0) FROM PlayHistory
            ", conn);
            var result = cmd.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToDouble(result) : 0;
        }
    }

    public class PlayCountStat
    {
        public double PlayedSeconds { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Bvid { get; set; } = string.Empty;
        public int Page { get; set; }
        public string? CoverPath { get; set; }

        public string PlayedTimeFormatted => FormatSeconds(PlayedSeconds);

        private string FormatSeconds(double seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            if (ts.TotalHours >= 1)
                return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }

    public class PlayDateStat
    {
        public DateTime Date { get; set; }
        public int PlayCount { get; set; }
    }
}