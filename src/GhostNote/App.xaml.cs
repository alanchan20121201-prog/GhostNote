using System.Windows;
using GhostNote.Interop;
using GhostNote.Models;
using GhostNote.Services;
using GhostNote.Views;

namespace GhostNote;

/// <summary>
/// 应用入口。程序启动后只有托盘图标，没有任何主窗口。
/// </summary>
public partial class App : Application
{
    private NoteState _state = new();
    private GhostWindow? _ghost;
    private InputWindow? _input;
    private TrayService? _tray;
    private HotkeyHost? _hotkey;
    private MouseHook? _mouseHook;

    private bool HasText => !string.IsNullOrWhiteSpace(_state.Text);

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _state = StateStore.Load();

        // 1. 水印窗口（点不中、永远置顶）
        _ghost = new GhostWindow();
        _ghost.SetText(_state.Text);
        _ghost.ApplyStyle(_state);
        _ghost.PositionChanged += (_, _) => CapturePosition();

        // 2. 输入窗口（Ctrl+Alt+N 弹出）
        _input = new InputWindow();
        _input.Submitted += OnTextSubmitted;

        // 3. 托盘图标
        _tray = new TrayService();
        _tray.EditRequested += (_, _) => ShowInput();
        _tray.ToggleRequested += (_, _) => ToggleVisible();
        _tray.FontSizeSelected += OnFontSizeSelected;
        _tray.ExitRequested += (_, _) => ExitApp();

        // 4. 全局快捷键 Ctrl+Alt+N
        _hotkey = new HotkeyHost();
        if (_hotkey.Register())
        {
            _hotkey.Pressed += (_, _) => ShowInput();
        }
        else
        {
            _tray.Notify("快捷键被占用", "Ctrl+Alt+N 已被其它程序占用，请从托盘菜单打开输入框。");
        }

        // 5. 全局鼠标钩子：Ctrl+Alt+左键拖动水印
        _mouseHook = new MouseHook();
        _mouseHook.DragStart += OnDragStart;
        _mouseHook.DragMove += OnDragMove;
        _mouseHook.DragEnd += OnDragEnd;
        _mouseHook.Install();

        // 6. 恢复上次的文字与位置
        if (HasText && _state.Visible)
        {
            _ghost.Show();
            _ghost.ApplyPosition(_state);
            _ghost.SavePositionTo(_state);
        }

        _tray.SetVisibleState(_ghost.IsVisible);
        _tray.SetFontSize(_state.FontSize);
    }

    private void OnFontSizeSelected(double size)
    {
        _state.FontSize = size;
        _ghost?.ApplyStyle(_state);
        _tray?.SetFontSize(size);
        SaveState();
    }

    private void ShowInput()
    {
        _input?.ShowForEdit(_state.Text);
    }

    private void OnTextSubmitted(string text)
    {
        if (_ghost == null)
        {
            return;
        }

        // 留空提交 = 清掉水印
        if (string.IsNullOrWhiteSpace(text))
        {
            _state.Text = string.Empty;
            _state.Visible = false;
            _ghost.Hide();
            _tray?.SetVisibleState(false);
            SaveState();
            return;
        }

        _state.Text = text;
        _state.Visible = true;

        _ghost.SetText(text);
        _ghost.ApplyStyle(_state);
        _ghost.Show();
        _ghost.ApplyPosition(_state);
        _ghost.SavePositionTo(_state);

        _tray?.SetVisibleState(true);
        SaveState();
    }

    private void ToggleVisible()
    {
        if (_ghost == null)
        {
            return;
        }

        if (_ghost.IsVisible)
        {
            _ghost.Hide();
            _state.Visible = false;
        }
        else
        {
            if (!HasText)
            {
                ShowInput();
                return;
            }

            _ghost.Show();
            _ghost.ApplyPosition(_state);
            _state.Visible = true;
        }

        _tray?.SetVisibleState(_ghost.IsVisible);
        SaveState();
    }

    private bool OnDragStart(System.Windows.Point cursor)
    {
        if (_ghost == null || !_ghost.IsVisible)
        {
            return false;
        }

        // 只有 Ctrl + Alt 同时按住才进入拖拽模式
        var ctrl = NativeMethods.IsKeyDown(NativeMethods.VK_CONTROL);
        var alt = NativeMethods.IsKeyDown(NativeMethods.VK_MENU);
        Trace.Log($"[app] dragstart ctrl={ctrl} alt={alt}");

        return ctrl && alt && _ghost.BeginDrag(cursor);
    }

    private void OnDragMove(System.Windows.Point cursor)
    {
        _ghost?.DragTo(cursor);
    }

    private void OnDragEnd(System.Windows.Point cursor)
    {
        // EndDrag 内部会触发 PositionChanged，从而把新位置写回 JSON
        _ghost?.EndDrag();
    }

    private void CapturePosition()
    {
        if (_ghost == null)
        {
            return;
        }

        _ghost.SavePositionTo(_state);
        _state.Visible = _ghost.IsVisible;
        SaveState();
    }

    private void SaveState()
    {
        StateStore.Save(_state);
    }

    private void ExitApp()
    {
        CapturePosition();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        CapturePosition();
        _mouseHook?.Dispose();
        _hotkey?.Dispose();
        _tray?.Dispose();
        base.OnExit(e);
    }
}
