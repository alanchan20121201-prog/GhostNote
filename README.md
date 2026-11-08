# GhostNote 幽灵便签

把一句话像浮水印一样刻在桌面上：永远置顶、鼠标完全穿透、不挡任何操作。

Windows 桌面小工具，.NET 8 + WPF 编写，托盘常驻，无主窗口。

## 功能

- 启动后只有系统托盘图标，没有主窗口。
- 全局快捷键 `Ctrl + Alt + N` 在屏幕中央弹出极简深色半透明输入框，`Enter` 保存，`Esc` 取消。
- 文字变成无边框、无标题栏的悬浮水印：微软雅黑 Light、浅灰色、深色描边 + 轻微投影（白底、深色背景上都看得清）。
- 窗口设置 `WS_EX_TRANSPARENT | WS_EX_LAYERED` + `HWND_TOPMOST`：点击直接穿透到下层窗口，且永远置顶。
- `Ctrl + Alt + 鼠标左键` 拖动水印到任意位置（走全局鼠标钩子，因为窗口本身收不到鼠标消息）。
- 文字内容与屏幕坐标保存到 `%AppData%\GhostNote\state.json`，重启后自动恢复。
- 托盘菜单：修改文字 / 字号（小·中·大） / 隐藏·显示水印 / 退出。

## 目录结构

```
GhostNote/
├─ .github/workflows/release.yml   # 打 tag 自动编译并发布 Release
├─ docs/RELEASE_NOTES_TEMPLATE.md  # Release 说明模板（被 workflow 读取）
├─ tools/make-icon.ps1             # 生成多尺寸应用图标（可选，ICO 已提交）
└─ src/GhostNote/
   ├─ GhostNote.csproj             # 单文件发布 / 体积相关配置
   ├─ App.xaml(.cs)                # 无主窗口启动、事件编排、状态落盘
   ├─ app.manifest                 # PerMonitorV2 DPI 感知
   ├─ Interop/NativeMethods.cs     # 全部 P/Invoke 声明
   ├─ Models/NoteState.cs          # 持久化模型
   ├─ Services/
   │  ├─ StateStore.cs             # JSON 读写（原子写入）
   │  ├─ HotkeyHost.cs             # message-only 窗口 + RegisterHotKey
   │  ├─ MouseHook.cs              # WH_MOUSE_LL 全局钩子，实现拖拽
   │  └─ TrayService.cs            # WinForms NotifyIcon 托盘（含字号档位）
   └─ Views/
      ├─ GhostWindow.xaml(.cs)     # 水印窗口：穿透、置顶、拖拽
      └─ InputWindow.xaml(.cs)     # 深色半透明输入框
```

## 本地构建

```bash
# 调试运行
dotnet run --project src/GhostNote/GhostNote.csproj

# 发布单文件（约 1MB）
dotnet publish src/GhostNote/GhostNote.csproj -c Release -o publish
```

产物：`publish/GhostNote.exe`。

## 关于体积与裁剪（重要）

需求里要求 `PublishSingleFile=true` + `PublishTrimmed=true` 且体积 < 5MB，这里必须说明一个客观限制：

**WPF 不支持裁剪。** 开启 `PublishTrimmed` 后，编译阶段只是警告，但 XAML/BAML 的反射依赖会被剪掉，双击运行直接崩溃，且无法通过配置修复。所以本项目采用 `PublishTrimmed=false`。

体积目标是靠「框架依赖 + 单文件」达成的：

| 方案 | 配置 | 体积 | 用户依赖 |
| --- | --- | --- | --- |
| 本项目 | `SelfContained=false` + 单文件 | 约 0.5 MB | 需装 .NET 8 桌面运行时 |
| 自包含 | `SelfContained=true` + 单文件 | 60 MB+ | 无 |

如果更在意"用户零依赖"，把 csproj 里的 `SelfContained` 改成 `true`（同时才允许打开 `EnableCompressionInSingleFile`），代价是体积远超 5MB。

## 发版

```bash
git tag v1.0.0
git push origin v1.0.0
```

推送 tag 后 GitHub Actions 会：

1. 在 `windows-latest` 上安装 .NET 8 SDK 并 `dotnet publish`；
2. 重命名产物为 `GhostNote-v1.0.0-win-x64.exe`；
3. 执行体积门禁：超过 5MB 直接失败；
4. 计算 SHA256，与 `docs/RELEASE_NOTES_TEMPLATE.md` 拼成 Release 正文；
5. 创建 GitHub Release 并上传 exe。

## 状态文件

`%AppData%\GhostNote\state.json`：

```json
{
  "Text": "下午三点回电话",
  "X": 620,
  "Y": 300,
  "HasPosition": true,
  "Visible": true,
  "FontSize": 36,
  "Foreground": "#B3E8E8E8"
}
```

`Foreground` 为 ARGB 十六进制，前两位是透明度；`FontSize` 一般不用手改——托盘菜单的「字号」里有小（28）/ 中（36）/ 大（48）三档，点一下立即生效。
