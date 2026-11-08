using System;
using System.Runtime.InteropServices;
using System.Windows;
using GhostNote.Interop;

namespace GhostNote.Services;

/// <summary>
/// 全局鼠标低级钩子（WH_MOUSE_LL）。
///
/// 为什么必须要钩子？
/// 幽灵窗口带了 WS_EX_TRANSPARENT，鼠标消息根本不会到达它，
/// 所以“按住 Ctrl+Alt 拖拽”只能从系统层面监听全局鼠标事件。
///
/// 钩子只读取坐标，不拦截任何正常输入（除了开始拖拽的那一对按下/抬起）。
/// </summary>
public sealed class MouseHook : IDisposable
{
    private readonly NativeMethods.LowLevelMouseProc _proc;
    private IntPtr _hook;
    private bool _dragging;

    public MouseHook()
    {
        // 必须持有委托实例，否则会被 GC 回收导致回调野指针。
        _proc = HookCallback;
    }

    /// <summary>Ctrl+Alt+左键按下。返回 true 表示接管这次拖拽（后续移动/抬起都会被消费）。</summary>
    public event Func<Point, bool>? DragStart;

    /// <summary>拖拽过程中鼠标移动，参数是屏幕物理像素坐标。</summary>
    public event Action<Point>? DragMove;

    /// <summary>拖拽结束。</summary>
    public event Action<Point>? DragEnd;

    public bool Install()
    {
        if (_hook != IntPtr.Zero)
        {
            return true;
        }

        _hook = NativeMethods.SetWindowsHookEx(
            NativeMethods.WH_MOUSE_LL,
            _proc,
            NativeMethods.GetModuleHandle(null),
            0);

        return _hook != IntPtr.Zero;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var data = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            var point = new Point(data.pt.X, data.pt.Y);
            var message = wParam.ToInt32();

            if (message == NativeMethods.WM_LBUTTONDOWN)
            {
                // 只有 Ctrl + Alt 同时按住时才进入拖拽模式
                if (NativeMethods.IsKeyDown(NativeMethods.VK_CONTROL) &&
                    NativeMethods.IsKeyDown(NativeMethods.VK_MENU))
                {
                    _dragging = DragStart?.Invoke(point) == true;
                    if (_dragging)
                    {
                        // 吞掉这次点击，避免顺便点到底下的窗口/桌面图标
                        return new IntPtr(1);
                    }
                }
            }
            else if (message == NativeMethods.WM_MOUSEMOVE)
            {
                if (_dragging)
                {
                    DragMove?.Invoke(point);
                    return new IntPtr(1);
                }
            }
            else if (message == NativeMethods.WM_LBUTTONUP)
            {
                if (_dragging)
                {
                    _dragging = false;
                    DragEnd?.Invoke(point);
                    return new IntPtr(1);
                }
            }
        }

        return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hook != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hook);
            _hook = IntPtr.Zero;
        }
    }
}
