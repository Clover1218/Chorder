using Chorder.Clients;
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
    public class PlayerService{
        private MpvClient _player;
        private Track _currentTrack;
        private TrackInfoService _trackInfoService;
        public Action? PlayEnded;
        private PlayHistoryRepository _playHistoryRepository;
        public PlayerService(PlayHistoryRepository playHistoryRepository, TrackInfoService trackInfoService)
        {
            _player = new MpvClient();
            _player.PlayEnded+=this.onPlayEnded;
            _playHistoryRepository=playHistoryRepository;
            _trackInfoService=trackInfoService;
        }
        private DateTime? _playStartTime;
        private double _accumulatedSeconds;
        private bool _isPlaying;
        private void StartPlaying()
        {
            _playStartTime = DateTime.Now;
            _isPlaying = true;
        }
        private void PausePlaying()
        {
            if (_isPlaying && _playStartTime.HasValue)
            {
                _accumulatedSeconds += (DateTime.Now - _playStartTime.Value).TotalSeconds;
            }
        
            _isPlaying = false;
            _playStartTime = null;
        }
        private void ResumePlaying()
        {
            if (!_isPlaying)
            {
                _playStartTime = DateTime.Now;
                _isPlaying = true;
            }
        }
        private double FinishPlaying()
        {
            PausePlaying(); 
        
            var total = _accumulatedSeconds;
        
            _accumulatedSeconds = 0;
            _playStartTime = null;
            _isPlaying = false;
        
            return total;
        }
        private void onPlayEnded() {
            SaveCurrentIfNeeded();
            this.PlayEnded?.Invoke();
        } 
        public void Play(Track track)
        {
            try
            {
                SaveCurrentIfNeeded();

                _currentTrack = track;

                var cachePath = _trackInfoService.GetCachePath(track.Bvid, track.Page);
                if (cachePath != null)
                {
                    _player.Play(cachePath);

                }
                else
                {
                    string url = $"https://www.bilibili.com/video/{track.Bvid}";
                    if(track.Page>1)
                        url+=$"?p={track.Page}";
                    _player.Play(url);
                }

                _accumulatedSeconds = 0;
                StartPlaying();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public async Task<string> GetPlayUrl(string url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-f bestaudio -g \"{url}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);

            string output = await process.StandardOutput.ReadToEndAsync();

            return output.Split('\n')[0].Trim();
        } 
        public void TogglePause() {
            _player.TogglePause();

            if (_isPlaying)
                PausePlaying();
            else
                ResumePlaying();
        }
        public void Stop() {
            SaveCurrentIfNeeded();
            _player.Stop();
        }
        private void SaveCurrentIfNeeded()
        {
            if (_currentTrack == null) return;
        
            var playedSeconds = FinishPlaying();
        
            if (playedSeconds < 5) return;
        
            try
            {
                _playHistoryRepository.Add(
                    _currentTrack,
                    _player.Duration,
                    playedSeconds
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public double Volume {
            get { return _player.Volume; }
            set { _player.Volume = value; }
        }
        public double Position {
            get { return _player.Position; }
            set { _player.Position = value; }
        }
        public double Time { get { return _player.Time; } }

        public double Duration {
            get { return _player.Duration; }
        }
    }
}
