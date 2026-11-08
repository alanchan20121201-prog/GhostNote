using System;
using System.Windows.Interop;
using GhostNote.Interop;

namespace GhostNote.Services;

/// <summary>
/// 全局快捷键宿主。
/// 一个不可见的 message-only 窗口，专门用来接收 WM_HOTKEY。
/// </summary>
public sealed class HotkeyHost : IDisposable
{
    private const int HotkeyId = 0xA001;

    private HwndSource? _source;
    private bool _registered;

    /// <summary>快捷键被按下。</summary>
    public event EventHandler? Pressed;

    /// <summary>
    /// 注册 Ctrl+Alt+N。返回 false 表示快捷键已被别的软件占用。
    /// </summary>
    public bool Register()
    {
        if (_registered)
        {
            return true;
        }

        var parameters = new HwndSourceParameters("GhostNote.HotkeyHost")
        {
            Width = 0,
            Height = 0,
            ParentWindow = NativeMethods.HWND_MESSAGE, // 消息专用窗口，桌面不可见
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);

        _registered = NativeMethods.RegisterHotKey(
            _source.Handle,
            HotkeyId,
            NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT | NativeMethods.MOD_NOREPEAT,
            NativeMethods.VK_N);

        return _registered;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            handled = true;
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_source == null)
        {
            return;
        }

        if (_registered)
        {
            NativeMethods.UnregisterHotKey(_source.Handle, HotkeyId);
            _registered = false;
        }

        _source.RemoveHook(WndProc);
        _source.Dispose();
        _source = null;
    }
}
