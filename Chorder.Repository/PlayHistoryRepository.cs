using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chorder.Models.Entities;

namespace Chorder.Repository {
    public class PlayHistoryRepository
    {
        private readonly SQLServerConnectionFactory _factory;

        public PlayHistoryRepository(SQLServerConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Add(Track track, double duration, double playedSeconds)
        {
            using var conn = _factory.CreateConnection(); // 你自己注入
            conn.Open();

            using var cmd = new SqlCommand(@"
                INSERT INTO PlayHistory
                (Id, Bvid, Page, Title, Author, PlayedAt, Duration, PlayedSeconds)
                VALUES
                (@Id, @Bvid, @Page, @Title, @Author, @PlayedAt, @Duration, @PlayedSeconds)
            ", conn);

            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@Bvid", track.Bvid );
            cmd.Parameters.AddWithValue("@Page", track.Page);
            cmd.Parameters.AddWithValue("@Title", track.Title );

            // ⭐ 你说 author 先不管
            cmd.Parameters.AddWithValue("@Author", track.Author );

            cmd.Parameters.AddWithValue("@PlayedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@Duration", duration);
            cmd.Parameters.AddWithValue("@PlayedSeconds", playedSeconds);

            cmd.ExecuteNonQuery();
        }
    }
}
