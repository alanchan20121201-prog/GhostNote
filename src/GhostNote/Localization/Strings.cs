namespace GhostNote.Localization;

/// <summary>
/// 界面文案。四种语言通过编译期常量切换：
///   dotnet publish -p:GhostNoteLang=zh-cn | zh-tw | en | ja
/// 每个语言单独产出一个 exe，运行时不再做语言判断。
/// </summary>
public static class Strings
{
#if GHOSTNOTE_LANG_TW

    public const string AppName = "GhostNote 幽靈便籤";
    public const string MenuEdit = "修改文字 (Ctrl+Alt+N)";
    public const string MenuFontSize = "字型大小";
    public const string FontSmall = "小";
    public const string FontMedium = "中";
    public const string FontLarge = "大";
    public const string MenuHide = "隱藏浮水印";
    public const string MenuShow = "顯示浮水印";
    public const string MenuExit = "結束 GhostNote";
    public const string InputPlaceholder = "寫一句想刻在桌面上的話…";
    public const string InputHint = "Enter 儲存 · Esc 取消 · Ctrl+Alt+滑鼠左鍵拖曳位置";
    public const string HotkeyBusyTitle = "快速鍵被佔用";
    public const string HotkeyBusyBody = "Ctrl+Alt+N 已被其他程式佔用，請從托盤選單開啟輸入框。";
    public const string WatermarkFont = "Microsoft JhengHei UI Light, Microsoft JhengHei, Segoe UI Light, Segoe UI";
    public const string UiFont = "Microsoft JhengHei UI, Microsoft JhengHei, Segoe UI";

#elif GHOSTNOTE_LANG_EN

    public const string AppName = "GhostNote";
    public const string MenuEdit = "Edit text (Ctrl+Alt+N)";
    public const string MenuFontSize = "Font size";
    public const string FontSmall = "Small";
    public const string FontMedium = "Medium";
    public const string FontLarge = "Large";
    public const string MenuHide = "Hide watermark";
    public const string MenuShow = "Show watermark";
    public const string MenuExit = "Exit GhostNote";
    public const string InputPlaceholder = "Type something to etch on your desktop…";
    public const string InputHint = "Enter to save · Esc to cancel · Ctrl+Alt+drag to move";
    public const string HotkeyBusyTitle = "Hotkey unavailable";
    public const string HotkeyBusyBody = "Ctrl+Alt+N is already taken by another app. Open the input box from the tray menu.";
    public const string WatermarkFont = "Segoe UI Light, Segoe UI";
    public const string UiFont = "Segoe UI";

#elif GHOSTNOTE_LANG_JA

    public const string AppName = "GhostNote ゴーストメモ";
    public const string MenuEdit = "テキストを編集 (Ctrl+Alt+N)";
    public const string MenuFontSize = "文字サイズ";
    public const string FontSmall = "小";
    public const string FontMedium = "中";
    public const string FontLarge = "大";
    public const string MenuHide = "透かしを隠す";
    public const string MenuShow = "透かしを表示";
    public const string MenuExit = "GhostNote を終了";
    public const string InputPlaceholder = "デスクトップに刻む一言を入力…";
    public const string InputHint = "Enter で保存 · Esc でキャンセル · Ctrl+Alt+左ドラッグで移動";
    public const string HotkeyBusyTitle = "ショートカットが使用中";
    public const string HotkeyBusyBody = "Ctrl+Alt+N は他のアプリが使用しています。トレイメニューから入力欄を開いてください。";
    public const string WatermarkFont = "Yu Gothic UI Light, Yu Gothic UI, Meiryo UI, Segoe UI Light";
    public const string UiFont = "Yu Gothic UI, Meiryo UI, Segoe UI";

#else

    public const string AppName = "GhostNote 幽灵便签";
    public const string MenuEdit = "修改文字 (Ctrl+Alt+N)";
    public const string MenuFontSize = "字号";
    public const string FontSmall = "小";
    public const string FontMedium = "中";
    public const string FontLarge = "大";
    public const string MenuHide = "隐藏水印";
    public const string MenuShow = "显示水印";
    public const string MenuExit = "退出 GhostNote";
    public const string InputPlaceholder = "写一句想刻在桌面上的话…";
    public const string InputHint = "Enter 保存 · Esc 取消 · Ctrl+Alt+鼠标左键拖动位置";
    public const string HotkeyBusyTitle = "快捷键被占用";
    public const string HotkeyBusyBody = "Ctrl+Alt+N 已被其它程序占用，请从托盘菜单打开输入框。";
    public const string WatermarkFont = "Microsoft YaHei Light, Microsoft YaHei, Segoe UI Light, Segoe UI";
    public const string UiFont = "Microsoft YaHei UI, Microsoft YaHei, Segoe UI";

#endif
}
