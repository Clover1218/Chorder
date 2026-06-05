using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Chorder.Clients
{
    internal static class MpvNative
    {
        [DllImport("libmpv-2.dll")]
        public static extern IntPtr mpv_create();

        [DllImport("libmpv-2.dll")]
        public static extern int mpv_initialize(IntPtr ctx);

        [DllImport("libmpv-2.dll")]
        public static extern void mpv_terminate_destroy(IntPtr ctx);

        [DllImport("libmpv-2.dll")]
        public static extern int mpv_command(IntPtr ctx, IntPtr args);

        [DllImport("libmpv-2.dll")]
        public static extern int mpv_set_property_string(
            IntPtr ctx,
            string name,
            string value);

        [DllImport("libmpv-2.dll")]
        public static extern int mpv_set_property(
            IntPtr ctx,
            string name,
            MpvFormat format,
            ref double data);

        [DllImport("libmpv-2.dll")]
        public static extern int mpv_get_property(
            IntPtr ctx,
            string name,
            MpvFormat format,
            ref double data);

        [DllImport("libmpv-2.dll")]
        public static extern int mpv_get_property(
            IntPtr ctx,
            string name,
            MpvFormat format,
            ref long data);

        [DllImport("libmpv-2.dll")]
        public static extern IntPtr mpv_wait_event(
            IntPtr ctx,
            double timeout);

        [StructLayout(LayoutKind.Sequential)]
        public struct mpv_event
        {
            public int event_id;
            public int error;
            public ulong reply_userdata;
            public IntPtr data;
        }

        // ⭐ EndFile 事件数据
        [StructLayout(LayoutKind.Sequential)]
        public struct mpv_event_end_file
        {
            public MpvEndFileReason reason;

            public int error;

            public long playlist_entry_id;

            public long playlist_insert_id;

            public int playlist_insert_num_entries;
        }
    }

    public enum MpvFormat
    {
        None = 0,
        String = 1,
        OSDString = 2,
        Flag = 3,
        Int64 = 4,
        Double = 5
    }

    public enum MpvEventId
    {
        None = 0,
        Shutdown = 1,
        LogMessage = 2,
        GetPropertyReply = 3,
        SetPropertyReply = 4,
        CommandReply = 5,
        StartFile = 6,
        EndFile = 7,
        FileLoaded = 8,
        Idle = 11,
        Tick = 14,
        PropertyChange = 16
    }

    // ⭐ EndFile 原因
    public enum MpvEndFileReason
    {
        EOF = 0,       // 自然播放结束
        Stop = 2,      // stop / replace / loadfile
        Quit = 3,
        Error = 4,
        Redirect = 5
    }

    public enum PlaybackState
    {
        Idle,
        Loading,
        Playing,
        Paused,
        Stopped,
        Ended
    }

    public class MpvClient : IDisposable
    {
        private IntPtr _ctx;

        private readonly CancellationTokenSource _cts = new();

        public PlaybackState State { get; private set; }
            = PlaybackState.Idle;

        public event Action? PlayEnded;

        public event Action<PlaybackState>? StateChanged;

        public MpvClient()
        {
            _ctx = MpvNative.mpv_create();

            if (_ctx == IntPtr.Zero)
                throw new Exception("mpv_create failed");

            MpvNative.mpv_initialize(_ctx);

            Set("video", "no");
            Set("audio", "yes");
            Set("force-window", "no");
            Set("osc", "no");
            Set("keep-open", "no");

            Task.Run(EventLoop);
        }

    //    public void Play(string url)
    //    {
    //        State = PlaybackState.Loading;
    //        RaiseStateChanged();
    //         if (url.Contains('\\') || (url.Length >= 2 && url[1] == ':'))
    //{
    //    // 1. 替换所有反斜杠为正斜杠
    //    string normalizedPath = url.Replace('\\', '/');
        
    //    // 2. 如果路径不包含盘符冒号后跟两个斜杠（即不是 file:// 格式），则加上 file:///
    //    if (!normalizedPath.StartsWith("file://"))
    //    {
    //        // 注意 file:/// 后跟绝对路径：file:///C:/Music/song.mp3
    //        url = "file:///" + normalizedPath;
    //    }
    //}
    //        Command("loadfile", url, "replace");
    //    }
    public void Play(string input)
{
    string target = input;

    // 1. 处理 Windows 本地路径
    if (!target.StartsWith("http://") && !target.StartsWith("https://") && !target.StartsWith("file://"))
    {
        // 转换为 file:/// 前缀
        target = "file:///" + target.Replace('\\', '/');
    }

    // 2. 解析出真正的文件路径部分
    Uri uri;
    if (Uri.TryCreate(target, UriKind.Absolute, out uri) && uri.IsFile)
    {
        // 获取本地路径的绝对地址，并用 Uri.EscapeDataString 手动编码
        string localPath = uri.LocalPath;
        // 重要：只编码文件名和路径，保留正斜杠 '/'
        string encodedPath = string.Join("/", localPath.Split('/')
            .Select(Uri.EscapeDataString));
        // 重新构建 file:/// 链接
        target = "file:///" + encodedPath;
    }

    // 3. 传递给 libmpv
    Command("loadfile", target, "replace");
}
        public void TogglePause()
        {
            Command("cycle", "pause");
        }

        public void Stop()
        {
            State = PlaybackState.Stopped;
            RaiseStateChanged();

            Command("stop");
        }

        public double Volume
        {
            get => Get("volume");
            set => SetDouble("volume", value);
        }

        public double Position
        {
            get => Get("percent-pos") / 100.0;
            set => SetDouble("percent-pos", value * 100);
        }

        public double Time => Get("time-pos");

        public double Duration => Get("duration");

        private void EventLoop()
        {
            while (!_cts.IsCancellationRequested &&
                   _ctx != IntPtr.Zero)
            {
                IntPtr eventPtr =
                    MpvNative.mpv_wait_event(_ctx, 0.1);

                if (eventPtr == IntPtr.Zero)
                    continue;

                var ev =
                    Marshal.PtrToStructure<MpvNative.mpv_event>(
                        eventPtr);
    //            System.Diagnostics.Debug.WriteLine(
    //            $"EndFile Data={ev.data}");
    //            Debug.WriteLine(
    //Marshal.SizeOf<MpvNative.mpv_event>());
                switch ((MpvEventId)ev.event_id)
                {
                    case MpvEventId.StartFile:
                    {
                        State = PlaybackState.Loading;
                        RaiseStateChanged();

                        break;
                    }

                    case MpvEventId.FileLoaded:
                    {
                        State = PlaybackState.Playing;
                        RaiseStateChanged();

                        break;
                    }

                    case MpvEventId.EndFile:
                    {
                        HandleEndFile(ev.data);

                        break;
                    }

                    case MpvEventId.Shutdown:
                    {
                        return;
                    }
                }
            }
        }

        private void HandleEndFile(IntPtr dataPtr)
        {
            var end =
                Marshal.PtrToStructure<MpvNative.mpv_event_end_file>(
                    dataPtr);


            switch (end.reason)
            {
                // ⭐ 真正自然播放结束
                case MpvEndFileReason.EOF:
                {
                    State = PlaybackState.Ended;
                    RaiseStateChanged();

                      PlayEnded?.Invoke();

                    break;
                }

                // ⭐ stop / 切歌 / replace
                case MpvEndFileReason.Stop:
                {
                    State = PlaybackState.Stopped;
                    RaiseStateChanged();

                    break;
                }

                case MpvEndFileReason.Error:
                {
                    State = PlaybackState.Stopped;
                    RaiseStateChanged();

                    break;
                }
            }
        }

        private void RaiseStateChanged()
        {
            StateChanged?.Invoke(State);
        }

        private void Command(params string[] args)
        {
            var ptrs = new IntPtr[args.Length + 1];

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    ptrs[i] =
                        Marshal.StringToHGlobalAnsi(args[i]);
                }

                ptrs[args.Length] = IntPtr.Zero;

                IntPtr argv =
                    Marshal.AllocHGlobal(
                        IntPtr.Size * ptrs.Length);

                Marshal.Copy(ptrs, 0, argv, ptrs.Length);

                MpvNative.mpv_command(_ctx, argv);

                Marshal.FreeHGlobal(argv);
            }
            finally
            {
                foreach (var ptr in ptrs)
                {
                    if (ptr != IntPtr.Zero)
                        Marshal.FreeHGlobal(ptr);
                }
            }
        }

        private void Set(string name, string value)
        {
            MpvNative.mpv_set_property_string(
                _ctx,
                name,
                value);
        }

        private void SetDouble(string name, double value)
        {
            MpvNative.mpv_set_property(
                _ctx,
                name,
                MpvFormat.Double,
                ref value);
        }

        private double Get(string name)
        {
            double value = 0;

            MpvNative.mpv_get_property(
                _ctx,
                name,
                MpvFormat.Double,
                ref value);

            return value;
        }

        public void Dispose()
        {
            _cts.Cancel();

            if (_ctx != IntPtr.Zero)
            {
                try
                {
                    Command("quit");
                }
                catch
                {
                }

                MpvNative.mpv_terminate_destroy(_ctx);

                _ctx = IntPtr.Zero;
            }
        }
    }
}