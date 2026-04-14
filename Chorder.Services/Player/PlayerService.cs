using Chorder.Clients;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chorder.Services.Player
{
    public class PlayerService
{
    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;
    private Process? _mpvProcess;
    private MpvClient _player;
    public MediaPlayer MediaPlayer => _mediaPlayer;

    public PlayerService()
    {
        Core.Initialize();
        _player = new MpvClient();
        _libVLC = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVLC);

    }

    public async Task Play(string bvid, int page)
    {
        try
        {
            string url = $"https://www.bilibili.com/video/{bvid}";
            if(page>1)
                url+=$"?p={page}";
            _player.Play(url);
            //string realUrl = await this.GetPlayUrl(url);

            //if (string.IsNullOrWhiteSpace(realUrl))
            //    return;

            //Stop();
            //var psi = new ProcessStartInfo
            //{
            //    FileName = "mpv",
            //    Arguments = $"--no-video {url}",
            //    UseShellExecute = false,
            //    CreateNoWindow = true
            //};

            //_mpvProcess = Process.Start(psi);



            //var media = new Media(_libVLC, new Uri(realUrl));
            //_mediaPlayer.Play(media);
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

        return output.Split('\n')[0].Trim(); // 取第一条流
    } 
    public void Stop()
    {
        if (_mpvProcess != null && !_mpvProcess.HasExited)
        {
            _mpvProcess.Kill();
        }
        _mediaPlayer.Stop();
    }
}
}
