using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using GhostNote.Interop;
using GhostNote.Models;

namespace GhostNote.Views;

/// <summary>
/// 幽灵水印窗口。
/// 关键点：无边框 + 分层透明 + WS_EX_TRANSPARENT（鼠标穿透） + 永远置顶。
/// </summary>
public partial class GhostWindow : Window
{
    private bool _dragging;
    private double _dragStartLeft;
    private double _dragStartTop;
    private Point _dragStartCursor;   // 拖拽起点（DIP）

    /// <summary>拖拽结束、位置发生变化时触发，用于落盘。</summary>
    public event EventHandler? PositionChanged;

    public GhostWindow()
    {
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ApplyClickThrough();
    }

    /// <summary>
    /// 鼠标穿透的核心实现。
    ///
    /// WS_EX_LAYERED     —— 分层窗口，配合 AllowsTransparency 才能画出半透明文字；
    /// WS_EX_TRANSPARENT —— 命中测试时“跳过”本窗口，点击直接落到下面的桌面/软件；
    /// WS_EX_TOOLWINDOW  —— 不出现在 Alt+Tab 里，保持“幽灵”感；
    /// WS_EX_NOACTIVATE  —— 点它也不会抢焦点。
    ///
    /// 之后再 SetWindowPos(HWND_TOPMOST) 覆盖一次，确保永远在最上层。
    /// </summary>
    private void ApplyClickThrough()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt64();
        exStyle |= NativeMethods.WS_EX_LAYERED
                   | NativeMethods.WS_EX_TRANSPARENT
                   | NativeMethods.WS_EX_TOOLWINDOW
                   | NativeMethods.WS_EX_NOACTIVATE;

        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(exStyle));

        NativeMethods.SetWindowPos(
            hwnd,
            NativeMethods.HWND_TOPMOST,
            0, 0, 0, 0,
            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
    }

    public void SetText(string text)
    {
        var value = text ?? string.Empty;
        NoteText.Text = value;
        OutlineText.Text = value;
    }

    public string GetText() => NoteText.Text;

    /// <summary>应用字号、颜色等外观。</summary>
    public void ApplyStyle(NoteState state)
    {
        var fontSize = state.FontSize > 0 ? state.FontSize : 36;

        // 描边层必须和正文层用同样的字号与换行宽度，否则两层文字会错位
        NoteText.FontSize = fontSize;
        OutlineText.FontSize = fontSize;

        var brush = ParseBrush(state.Foreground);
        if (brush != null)
        {
            NoteText.Foreground = brush;
        }

        // 太长的句子自动换行，最多占屏幕 80% 宽度
        var maxWidth = Math.Max(320, SystemParameters.PrimaryScreenWidth * 0.8);
        NoteText.MaxWidth = maxWidth;
        OutlineText.MaxWidth = maxWidth;
    }

    /// <summary>恢复上次的位置；没有记录就居中显示。</summary>
    public void ApplyPosition(NoteState state)
    {
        UpdateLayout();

        if (state.HasPosition && IsPositionUsable(state.X, state.Y))
        {
            Left = state.X;
            Top = state.Y;
        }
        else
        {
            CenterOnPrimary();
        }
    }

    /// <summary>把当前窗口位置写回状态对象（拖拽结束后调用）。</summary>
    public void SavePositionTo(NoteState state)
    {
        if (double.IsNaN(Left) || double.IsNaN(Top))
        {
            return;
        }

        state.X = Left;
        state.Y = Top;
        state.HasPosition = true;
    }

    private void CenterOnPrimary()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Left + Math.Max(0, (area.Width - ActualWidth) / 2);
        Top = area.Top + Math.Max(0, (area.Height - ActualHeight) / 2);
    }

    /// <summary>判断保存的坐标是否还落在某个显示器范围内（防止拔掉副屏后水印跑到看不见的地方）。</summary>
    private static bool IsPositionUsable(double x, double y)
    {
        if (double.IsNaN(x) || double.IsNaN(y))
        {
            return false;
        }

        var left = SystemParameters.VirtualScreenLeft;
        var top = SystemParameters.VirtualScreenTop;
        var right = left + SystemParameters.VirtualScreenWidth;
        var bottom = top + SystemParameters.VirtualScreenHeight;

        return x > left - 200 && x < right - 80 && y > top - 80 && y < bottom - 40;
    }

    // ===== 拖拽（由全局鼠标钩子驱动）=====

    /// <summary>开始拖拽。cursor 是屏幕物理像素坐标。</summary>
    public bool BeginDrag(Point cursor)
    {
        if (!IsVisible || string.IsNullOrEmpty(NoteText.Text))
        {
            return false;
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        _dragStartCursor = new Point(cursor.X / dpi.DpiScaleX, cursor.Y / dpi.DpiScaleY);
        _dragStartLeft = Left;
        _dragStartTop = Top;
        _dragging = true;
        return true;
    }

    /// <summary>拖拽中，把鼠标位移换算成窗口位移。</summary>
    public void DragTo(Point cursor)
    {
        if (!_dragging)
        {
            return;
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        var current = new Point(cursor.X / dpi.DpiScaleX, cursor.Y / dpi.DpiScaleY);

        Left = _dragStartLeft + (current.X - _dragStartCursor.X);
        Top = _dragStartTop + (current.Y - _dragStartCursor.Y);
    }

    public void EndDrag()
    {
        if (!_dragging)
        {
            return;
        }

        _dragging = false;
        PositionChanged?.Invoke(this, EventArgs.Empty);
    }

    private static Brush? ParseBrush(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            var color = ColorConverter.ConvertFromString(value);
            return color is Color c ? new SolidColorBrush(c) : null;
        }
        catch
        {
            return null;
        }
    }
}
