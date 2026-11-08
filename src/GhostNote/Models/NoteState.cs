using System;

namespace GhostNote.Models;

/// <summary>
/// 需要跨进程重启保存的全部状态。
/// </summary>
public sealed class NoteState
{
    /// <summary>水印文字，空字符串表示还没有内容。</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>窗口左上角的 X 坐标（DIP，与 DPI 缩放无关的逻辑坐标）。</summary>
    public double X { get; set; }

    /// <summary>窗口左上角的 Y 坐标（DIP）。</summary>
    public double Y { get; set; }

    /// <summary>是否已经有过坐标。false 表示首次启动，使用屏幕居中。</summary>
    public bool HasPosition { get; set; }

    /// <summary>水印当前是否可见（对应托盘里的隐藏/显示）。</summary>
    public bool Visible { get; set; } = true;

    /// <summary>字号。</summary>
    public double FontSize { get; set; } = 36;

    /// <summary>文字颜色，形如 #B3FFFFFF（ARGB）。</summary>
    public string Foreground { get; set; } = "#B3E8E8E8";

    public NoteState Clone() => new()
    {
        Text = Text,
        X = X,
        Y = Y,
        HasPosition = HasPosition,
        Visible = Visible,
        FontSize = FontSize,
        Foreground = Foreground,
    };
}
