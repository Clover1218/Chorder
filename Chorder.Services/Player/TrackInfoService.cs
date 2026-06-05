using Chorder.Models.Entities;
using Chorder.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.Services.Player
{
    public class TrackInfoService
    {
        private readonly TrackInfoRepository _repository;
        private static readonly string CacheDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "cache");

        public TrackInfoService(TrackInfoRepository repository)
        {
            _repository = repository;
            Directory.CreateDirectory(CacheDir);
        }

        public void EnsureTrack(Track track)
        {
            var existing = _repository.GetByBvidAndPage(track.Bvid, track.Page);
            if (existing != null)
            {
                existing.Title = track.Title;
                existing.Author = track.Author;
                // 如果数据库中没有 CoverPath 且新数据有，则更新
                if (string.IsNullOrEmpty(existing.CoverPath) && !string.IsNullOrEmpty(track.CoverPath))
                {
                    existing.CoverPath = track.CoverPath;
                }
                _repository.Upsert(existing);
            }
            else
            {
                var info = new TrackInfo
                {
                    Bvid = track.Bvid,
                    Page = track.Page,
                    Title = track.Title,
                    Author = track.Author,
                    CoverPath = track.CoverPath ?? "",
                };
                _repository.Upsert(info);
            }
        }

        public string? GetNickname(string bvid, int page)
        {
            var info = _repository.GetByBvidAndPage(bvid, page);
            return info?.Nickname;
        }

        public string? GetCoverPath(string bvid, int page)
        {
            var info = _repository.GetByBvidAndPage(bvid, page);
            return info?.CoverPath;
        }

        public string ResolveTitle(Track track)
        {
            var nickname = GetNickname(track.Bvid, track.Page);
            return !string.IsNullOrEmpty(nickname) ? nickname : track.Title;
        }

        public string ResolveTitle(string bvid, int page, string defaultTitle)
        {
            var nickname = GetNickname(bvid, page);
            return !string.IsNullOrEmpty(nickname) ? nickname : defaultTitle;
        }

        public void UpdateNickname(string bvid, int page, string? nickname)
        {
            _repository.UpdateNickname(bvid, page, nickname);
        }

        public string? GetCachePath(string bvid, int page)
        {
            var info = _repository.GetByBvidAndPage(bvid, page);
            if (info?.CachePath == null) return null;

            var filePath = info.CachePath;
            if (File.Exists(filePath))
                return filePath;

            return null;
        }

        public static string GetExpectedCachePath(string bvid, int page)
        {
            return Path.Combine(CacheDir, $"{bvid}_p{page}.mp3");
        }

        public static bool IsFileCached(string bvid, int page)
        {
            return File.Exists(GetExpectedCachePath(bvid, page));
        }

        public async Task<string?> DownloadCacheAsync(string bvid, int page)
        {
            var outputPath = GetExpectedCachePath(bvid, page);
            Directory.CreateDirectory(CacheDir);

            var url = $"https://www.bilibili.com/video/{bvid}";
            if (page > 1)
                url += $"?p={page}";

            var psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"--extract-audio --audio-format mp3 --audio-quality 0 \"{url}\" -o \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            if (process == null)
                return null;

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                return null;

            var info = _repository.GetByBvidAndPage(bvid, page);
            if (info != null)
            {
                info.CachePath = outputPath;
                _repository.Upsert(info);
            }

            return outputPath;
        }

        public void DeleteCache(string bvid, int page)
        {
            var filePath = GetExpectedCachePath(bvid, page);
            if (File.Exists(filePath))
                File.Delete(filePath);

            var info = _repository.GetByBvidAndPage(bvid, page);
            if (info != null)
            {
                info.CachePath = "";
                _repository.Upsert(info);
            }
        }
    }
}
