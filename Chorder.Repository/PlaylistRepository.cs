using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chorder.Models.Entities;

namespace Chorder.Repository {
    public class PlaylistRepository {
        private readonly SQLServerConnectionFactory _factory;

        public PlaylistRepository(SQLServerConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<Playlist> GetAllPlaylists()
        {
            var playlists = new List<Playlist>();

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT Id, Name, UpdatedAt, Position
                FROM PlaylistLibaray
                ORDER BY Position, UpdatedAt DESC
            ", conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                playlists.Add(new Playlist
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    UpdatedAt = reader.GetDateTime(2),
                    Position = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    Tracks = new List<Track>()
                });
            }
            reader.Close();

            foreach (var playlist in playlists)
            {
                playlist.Tracks = GetTracks(playlist.Id);
            }

            return playlists;
        }

        public List<Track> GetTracks(Guid playlistId)
        {
            var tracks = new List<Track>();

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                SELECT Id, Bvid, Page, Title, Author, Position
                FROM PlaylistItems
                WHERE PlaylistId = @PlaylistId
                ORDER BY Position
            ", conn);

            cmd.Parameters.AddWithValue("@PlaylistId", playlistId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tracks.Add(new Track
                {
                    Id = reader.GetGuid(0),
                    Bvid = reader.GetString(1),
                    Page = reader.GetInt32(2),
                    Title = reader.GetString(3),
                    Author = reader.GetString(4),
                    Position = reader.GetInt32(5)
                });
            }
            return tracks;
        }

        public void AddPlaylist(Playlist playlist)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            int position;
            using (var posCmd = new SqlCommand(
                "SELECT ISNULL(MAX(Position), -1) + 1 FROM PlaylistLibaray",
                conn))
            {
                position = (int)posCmd.ExecuteScalar();
            }

            using var cmd = new SqlCommand(@"
                INSERT INTO PlaylistLibaray (Id, Name, UpdatedAt, Position)
                VALUES (@Id, @Name, @UpdatedAt, @Position)
            ", conn);

            cmd.Parameters.AddWithValue("@Id", playlist.Id);
            cmd.Parameters.AddWithValue("@Name", playlist.Name);
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@Position", position);

            cmd.ExecuteNonQuery();

            playlist.Position = position;
        }

        public void DeletePlaylist(Guid playlistId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var deleteItemsCmd = new SqlCommand(@"
                DELETE FROM PlaylistItems WHERE PlaylistId = @PlaylistId
            ", conn);

            deleteItemsCmd.Parameters.AddWithValue("@PlaylistId", playlistId);
            deleteItemsCmd.ExecuteNonQuery();

            using var deletePlaylistCmd = new SqlCommand(@"
                DELETE FROM PlaylistLibaray WHERE Id = @Id
            ", conn);

            deletePlaylistCmd.Parameters.AddWithValue("@Id", playlistId);
            deletePlaylistCmd.ExecuteNonQuery();
        }

        public void UpdatePlaylistName(Guid playlistId, string name)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                UPDATE PlaylistLibaray
                SET Name = @Name, UpdatedAt = @UpdatedAt
                WHERE Id = @Id
            ", conn);

            cmd.Parameters.AddWithValue("@Id", playlistId);
            cmd.Parameters.AddWithValue("@Name",name );
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

            cmd.ExecuteNonQuery();
        }

        public void UpdatePlaylistPosition(Guid playlistId, int newPosition)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                UPDATE PlaylistLibaray
                SET Position = @Position
                WHERE Id = @Id
            ", conn);

            cmd.Parameters.AddWithValue("@Id", playlistId);
            cmd.Parameters.AddWithValue("@Position", newPosition);

            cmd.ExecuteNonQuery();
        }

        public void AddTrack(Guid playlistId, Track track)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            int position;
            using (var posCmd = new SqlCommand(
                "SELECT ISNULL(MAX(Position), -1) + 1 FROM PlaylistItems WHERE PlaylistId = @PlaylistId",
                conn))
            {
                posCmd.Parameters.AddWithValue("@PlaylistId", playlistId);
                position = (int)posCmd.ExecuteScalar();
            }

            var newId = Guid.NewGuid();

            using var cmd = new SqlCommand(@"
                INSERT INTO PlaylistItems (Id, PlaylistId, Bvid, Page, Title, Author, Position)
                VALUES (@Id, @PlaylistId, @Bvid, @Page, @Title, @Author, @Position)
            ", conn);

            cmd.Parameters.AddWithValue("@Id", newId);
            cmd.Parameters.AddWithValue("@PlaylistId", playlistId);
            cmd.Parameters.AddWithValue("@Bvid", track.Bvid);
            cmd.Parameters.AddWithValue("@Page", track.Page);
            cmd.Parameters.AddWithValue("@Title", track.Title);
            cmd.Parameters.AddWithValue("@Author", track.Author);
            cmd.Parameters.AddWithValue("@Position", position);

            cmd.ExecuteNonQuery();

            track.Id = newId;
            track.Position = position;
        }

        public void RemoveTrack(Guid trackId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                DELETE FROM PlaylistItems WHERE Id = @Id
            ", conn);

            cmd.Parameters.AddWithValue("@Id", trackId);

            cmd.ExecuteNonQuery();
        }

        public void UpdateTrackPosition(Guid trackId, int newPosition)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = new SqlCommand(@"
                UPDATE PlaylistItems
                SET Position = @Position
                WHERE Id = @Id
            ", conn);

            cmd.Parameters.AddWithValue("@Id", trackId);
            cmd.Parameters.AddWithValue("@Position", newPosition);

            cmd.ExecuteNonQuery();
        }
    }
}
