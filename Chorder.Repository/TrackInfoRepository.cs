using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chorder.Models.Entities;

namespace Chorder.Repository
{
    public class TrackInfoRepository
    {
        private readonly SQLServerConnectionFactory _factory;

        public TrackInfoRepository(SQLServerConnectionFactory factory)
        {
            _factory = factory;
        }

        public TrackInfo? GetByBvidAndPage(string bvid, int page)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT Id, Bvid, Page, Title, Author, Nickname, CachePath, CoverPath
                FROM TrackInfo
                WHERE Bvid = @Bvid AND Page = @Page
            ", conn);

            cmd.Parameters.AddWithValue("@Bvid", bvid);
            cmd.Parameters.AddWithValue("@Page", page);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new TrackInfo
                {
                    Id = reader.GetGuid(0),
                    Bvid = reader.GetString(1),
                    Page = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    Author = reader.GetString(4),
                    Nickname = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CachePath = reader.IsDBNull(6) ? null : reader.GetString(6),
                    CoverPath = reader.IsDBNull(7) ? null : reader.GetString(7),
                };
            }

            return null;
        }

        public void Upsert(TrackInfo info)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            var existing = GetByBvidAndPage(info.Bvid, info.Page);
            System.Diagnostics.Debug.WriteLine(info);
            if (existing != null)
            {
                using var cmd = new SqlCommand(@"
                    UPDATE TrackInfo
                    SET Title = @Title, Author = @Author,
                        Nickname = @Nickname, CachePath = @CachePath, CoverPath = @CoverPath
                    WHERE Bvid = @Bvid AND Page = @Page
                ", conn);

                cmd.Parameters.AddWithValue("@Bvid", info.Bvid);
                cmd.Parameters.AddWithValue("@Page", info.Page);
                cmd.Parameters.AddWithValue("@Title", info.Title);
                cmd.Parameters.AddWithValue("@Author", info.Author);
                cmd.Parameters.AddWithValue("@Nickname", (object?)info.Nickname ?? "");
                cmd.Parameters.AddWithValue("@CachePath", (object?)info.CachePath ?? "");
                cmd.Parameters.AddWithValue("@CoverPath", (object?)info.CoverPath ?? "");

                cmd.ExecuteNonQuery();
            }
            else
            {
                using var cmd = new SqlCommand(@"
                    INSERT INTO TrackInfo (Id, Bvid, Page, Title, Author, Nickname, CachePath, CoverPath)
                    VALUES (@Id, @Bvid, @Page, @Title, @Author, @Nickname, @CachePath, @CoverPath)
                ", conn);

                cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                cmd.Parameters.AddWithValue("@Bvid", info.Bvid);
                cmd.Parameters.AddWithValue("@Page", info.Page);
                cmd.Parameters.AddWithValue("@Title", info.Title);
                cmd.Parameters.AddWithValue("@Author", info.Author);
                cmd.Parameters.AddWithValue("@Nickname", (object?)info.Nickname ?? "");
                cmd.Parameters.AddWithValue("@CachePath", (object?)info.CachePath ?? "");
                cmd.Parameters.AddWithValue("@CoverPath", (object?)info.CoverPath ?? "");

                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateNickname(string bvid, int page, string? nickname)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                UPDATE TrackInfo
                SET Nickname = @Nickname
                WHERE Bvid = @Bvid AND Page = @Page
            ", conn);

            cmd.Parameters.AddWithValue("@Bvid", bvid);
            cmd.Parameters.AddWithValue("@Page", page);
            cmd.Parameters.AddWithValue("@Nickname", (object?)nickname ?? "");

            cmd.ExecuteNonQuery();
        }
    }
}
