using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GhostNote.Views;

/// <summary>
/// 屏幕中央弹出的极简输入框。Enter 保存，Esc 取消。
/// 同一个实例复用，避免反复创建窗口导致焦点丢失。
/// </summary>
public partial class InputWindow : Window
{
    /// <summary>用户按下 Enter 提交，参数为去掉首尾空白的文本。</summary>
    public event Action<string>? Submitted;

    public InputWindow()
    {
        InitializeComponent();
    }

    public void ShowForEdit(string? currentText)
    {
        InputBox.Text = currentText ?? string.Empty;
        UpdatePlaceholder();

        if (!IsVisible)
        {
            Show();
        }

        Activate();
        InputBox.Focus();
        InputBox.CaretIndex = InputBox.Text.Length;
        InputBox.SelectAll();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Commit();
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            HideInput();
        }
    }

    private void Commit()
    {
        var text = InputBox.Text.Trim();
        HideInput();
        Submitted?.Invoke(text);
    }

    private void HideInput()
    {
        Hide();
        InputBox.Text = string.Empty;
        UpdatePlaceholder();
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdatePlaceholder();
    }

    private void UpdatePlaceholder()
    {
        Placeholder.Visibility = string.IsNullOrEmpty(InputBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        // 点到别的地方就自动收起，不留孤儿窗口
        HideInput();
    }
}
