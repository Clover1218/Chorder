using System;
using System.Runtime.InteropServices;

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

        // ❗关键修正
        [DllImport("libmpv-2.dll")]
        public static extern int mpv_command(IntPtr ctx, IntPtr args);

        [DllImport("libmpv-2.dll")]
        public static extern int mpv_set_property_string(IntPtr ctx, string name, string value);

        [DllImport("libmpv-2.dll")]
        public static extern int mpv_set_property_double(IntPtr ctx, string name, double value);

        [DllImport("libmpv-2.dll")]
        public static extern double mpv_get_property_double(IntPtr ctx, string name);
    }
public class MpvClient : IDisposable
{
    private IntPtr _ctx;

    public MpvClient()
    {
        _ctx = MpvNative.mpv_create();

        if (_ctx == IntPtr.Zero)
            throw new Exception("mpv_create failed");

        MpvNative.mpv_initialize(_ctx);

        // 🎧 音频模式
        Set("video", "no");
        Set("audio", "yes");
        Set("force-window", "no");
        Set("osc", "no");
        Set("keep-open", "yes");
    }

    // ======================
    // 播放
    // ======================

    public void Play(string url)
    {
        Command("loadfile", url, "replace");
    }

    // ======================
    // 暂停
    // ======================

    public void TogglePause()
    {
        Command("cycle", "pause");
    }

    // ======================
    // 停止
    // ======================

    public void Stop()
    {
        Command("stop");
    }

    // ======================
    // 音量
    // ======================

    public double Volume
    {
        get => Get("volume");
        set => SetDouble("volume", value);
    }

    // ======================
    // 进度
    // ======================

    public double Position
    {
        get => Get("percent-pos") / 100.0;
        set => SetDouble("percent-pos", value * 100);
    }

    public double Time => Get("time-pos");
    public double Duration => Get("duration");

    // ======================
    // Command 封装（核心）
    // ======================

    private void Command(params string[] args)
    {
        var ptrs = new IntPtr[args.Length + 1];

        try
        {
            for (int i = 0; i < args.Length; i++)
            {
                ptrs[i] = Marshal.StringToHGlobalAnsi(args[i]);
            }

            ptrs[args.Length] = IntPtr.Zero;

            IntPtr argv = Marshal.AllocHGlobal(IntPtr.Size * ptrs.Length);

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

    // ======================
    // 属性
    // ======================

    private void Set(string name, string value)
        => MpvNative.mpv_set_property_string(_ctx, name, value);

    private void SetDouble(string name, double value)
        => MpvNative.mpv_set_property_double(_ctx, name, value);

    private double Get(string name)
        => MpvNative.mpv_get_property_double(_ctx, name);

    // ======================
    // 释放
    // ======================

    public void Dispose()
    {
        if (_ctx != IntPtr.Zero)
        {
            MpvNative.mpv_terminate_destroy(_ctx);
            _ctx = IntPtr.Zero;
        }
    }
}
}