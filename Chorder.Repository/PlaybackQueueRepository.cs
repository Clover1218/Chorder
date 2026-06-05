using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chorder.Models.Entities;
namespace Chorder.Repository {
    public class PlaybackQueueRepository {
        private readonly SQLServerConnectionFactory _factory;

        public PlaybackQueueRepository(SQLServerConnectionFactory factory)
        {
            _factory = factory;
        }
        public List<PlaybackQueueItem> GetAllItems(string queueId)
        {
            var list = new List<PlaybackQueueItem>();

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT Bvid, Page, Title, Author,Id, Position
                FROM PlaybackQueueItems
                WHERE QueueId = @QueueId
                ORDER BY Position
            ", conn);

            cmd.Parameters.AddWithValue("@QueueId", queueId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var track=new Track(){ 
                    Bvid=reader.IsDBNull(0) ? null : reader.GetString(0),
                    Page=reader.GetInt32(1),
                    Title=reader.IsDBNull(2) ? null : reader.GetString(2),
                    Author=reader.IsDBNull(3) ? null : reader.GetString(3)
                };
                list.Add(new PlaybackQueueItem
                {
                    Track=track,
                    Id = reader.GetGuid(4),
                    Position = reader.GetInt32(5)
                });
            }
            return list;
        }
        public Guid AddItem(string queueId, PlaybackQueueItem item)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            // ⭐ 自动计算 Position（放到最后）
            int position;
            using (var posCmd = new SqlCommand(
                "SELECT ISNULL(MAX(Position), -1) + 1 FROM PlaybackQueueItems WHERE QueueId = @QueueId",
                conn))
            {
                posCmd.Parameters.AddWithValue("@QueueId", queueId);
                position = (int)posCmd.ExecuteScalar();
            }

            var newId = Guid.NewGuid();

            using var cmd = new SqlCommand(@"
                INSERT INTO PlaybackQueueItems
                (Id, QueueId, Bvid, Page, Title, Author, Position)
                VALUES
                (@Id, @QueueId, @Bvid, @Page, @Title, @Author, @Position)
            ", conn);

            cmd.Parameters.AddWithValue("@Id", newId);
            cmd.Parameters.AddWithValue("@QueueId", queueId);
            cmd.Parameters.AddWithValue("@Bvid", (object?)item.Track.Bvid ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Page", (object?)item.Track.Page ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Title", (object?)item.Track.Title ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Author", (object?)item.Track.Author ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Position", position);

            cmd.ExecuteNonQuery();

            item.Id = newId;
            item.Position = position;

            return newId;
        }

        public void RemoveItem(Guid itemId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                DELETE FROM PlaybackQueueItems
                WHERE Id = @Id
            ", conn);

            cmd.Parameters.AddWithValue("@Id", itemId);

            cmd.ExecuteNonQuery();
        }

        public void ClearAll(string queueId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                DELETE FROM PlaybackQueueItems
                WHERE QueueId = @QueueId
            ", conn);

            cmd.Parameters.AddWithValue("@QueueId", queueId);

            cmd.ExecuteNonQuery();
        }

        public void UpdatePosition(Guid itemId, int newPosition)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                UPDATE PlaybackQueueItems
                SET Position = @Position
                WHERE Id = @Id
            ", conn);

            cmd.Parameters.AddWithValue("@Id", itemId);
            cmd.Parameters.AddWithValue("@Position", newPosition);

            cmd.ExecuteNonQuery();
        }
    }
}