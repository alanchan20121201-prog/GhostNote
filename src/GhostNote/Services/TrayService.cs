using System;
using System.Drawing;
using System.Windows.Forms;
using GhostNote.Localization;

namespace GhostNote.Services;

/// <summary>
/// 托盘图标与右键菜单。用 WinForms 的 NotifyIcon，避免引入第三方托盘库。
/// </summary>
public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly ToolStripMenuItem _fontSmall;
    private readonly ToolStripMenuItem _fontMedium;
    private readonly ToolStripMenuItem _fontLarge;

    /// <summary>字号档位，单位与 WPF 的 FontSize 一致（DIP）。</summary>
    public const double SmallFont = 28;
    public const double MediumFont = 36;
    public const double LargeFont = 48;

    public event EventHandler? EditRequested;
    public event EventHandler? ToggleRequested;
    public event EventHandler? ExitRequested;

    /// <summary>用户在菜单里选了某个字号。</summary>
    public event Action<double>? FontSizeSelected;

    public TrayService()
    {
        var editItem = new ToolStripMenuItem(Strings.MenuEdit);
        editItem.Click += (_, _) => EditRequested?.Invoke(this, EventArgs.Empty);

        _fontSmall = CreateFontItem(Strings.FontSmall, SmallFont);
        _fontMedium = CreateFontItem(Strings.FontMedium, MediumFont);
        _fontLarge = CreateFontItem(Strings.FontLarge, LargeFont);

        var fontMenu = new ToolStripMenuItem(Strings.MenuFontSize);
        fontMenu.DropDownItems.AddRange(new ToolStripItem[] { _fontSmall, _fontMedium, _fontLarge });

        _toggleItem = new ToolStripMenuItem(Strings.MenuHide);
        _toggleItem.Click += (_, _) => ToggleRequested?.Invoke(this, EventArgs.Empty);

        var exitItem = new ToolStripMenuItem(Strings.MenuExit);
        exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        var menu = new ContextMenuStrip
        {
            ShowImageMargin = false,
            Font = new Font("Microsoft YaHei UI", 9F),
        };
        menu.Items.Add(editItem);
        menu.Items.Add(fontMenu);
        menu.Items.Add(_toggleItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadAppIcon(),
            Text = Strings.AppName,
            Visible = true,
            ContextMenuStrip = menu,
        };

        _notifyIcon.DoubleClick += (_, _) => EditRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>同步托盘菜单里“隐藏/显示”的文案。</summary>
    public void SetVisibleState(bool visible)
    {
        _toggleItem.Text = visible ? Strings.MenuHide : Strings.MenuShow;
    }

    /// <summary>把当前字号对应的档位打上勾。</summary>
    public void SetFontSize(double size)
    {
        _fontSmall.Checked = Math.Abs(size - SmallFont) < 0.5;
        _fontMedium.Checked = Math.Abs(size - MediumFont) < 0.5;
        _fontLarge.Checked = Math.Abs(size - LargeFont) < 0.5;
    }

    private ToolStripMenuItem CreateFontItem(string text, double size)
    {
        var item = new ToolStripMenuItem(text) { CheckOnClick = false };
        item.Click += (_, _) => FontSizeSelected?.Invoke(size);
        return item;
    }

    /// <summary>气泡提示（快捷键冲突、热键被占用时用）。</summary>
    public void Notify(string title, string content)
    {
        _notifyIcon.ShowBalloonTip(4000, title, content, ToolTipIcon.Info);
    }

    private static Icon LoadAppIcon()
    {
        try
        {
            var path = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(path))
            {
                var icon = Icon.ExtractAssociatedIcon(path);
                if (icon != null)
                {
                    return icon;
                }
            }
        }
        catch
        {
            // 忽略，退回到系统图标
        }

        return SystemIcons.Application;
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
    }
}
