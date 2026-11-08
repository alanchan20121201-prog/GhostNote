# GhostNote 幽灵便签

把一句话像浮水印一样刻在桌面上：永远置顶、鼠标完全穿透、不挡任何操作。

Windows 桌面小工具，.NET 8 + WPF 编写，托盘常驻，无主窗口。

**文档语言**：[简体中文](README.md) · [繁體中文](README.zh-TW.md) · [English](README.en.md) · [日本語](README.ja.md)

## 四种语言版本

四个 exe 功能完全相同，只有界面文案与字体不同，按需下载一个即可：

| 文件名 | 语言 | 字体 |
| --- | --- | --- |
| `GhostNote-zh-cn.exe` | 简体中文 | 微软雅黑 Light |
| `GhostNote-zh-tw.exe` | 繁體中文 | 微軟正黑體 Light |
| `GhostNote-en.exe` | English | Segoe UI Light |
| `GhostNote-ja.exe` | 日本語 | 游ゴシック UI Light |

语言在**编译期**通过 `GhostNoteLang` 属性固定，运行时不判断系统区域、不加载附属程序集，因此每个 exe 里只含一种文案。

## 功能

- 启动后只有系统托盘图标，没有主窗口。
- 全局快捷键 `Ctrl + Alt + N` 在屏幕中央弹出极简深色半透明输入框，`Enter` 保存，`Esc` 取消。
- 文字变成无边框、无标题栏的悬浮水印：浅灰色、深色描边 + 轻微投影（白底、深色背景上都看得清）。
- 窗口设置 `WS_EX_TRANSPARENT | WS_EX_LAYERED` + `HWND_TOPMOST`：点击直接穿透到下层窗口，且永远置顶。
- `Ctrl + Alt + 鼠标左键` 拖动水印到任意位置（走全局鼠标钩子，因为窗口本身收不到鼠标消息）。
- 文字内容与屏幕坐标保存到 `%AppData%\GhostNote\state.json`，重启后自动恢复。
- 托盘菜单：修改文字 / 字号（小·中·大） / 隐藏·显示水印 / 退出。

## 目录结构

```
GhostNote/
├─ .github/workflows/release.yml   # 打 tag 自动编译四语言并发布 Release
├─ docs/RELEASE_NOTES_TEMPLATE.md  # 四语 Release 说明模板（被 workflow 读取）
├─ README*.md                      # 四种语言的说明文档
├─ tools/make-icon.ps1             # 生成多尺寸应用图标（可选，ICO 已提交）
├─ tools/InputSim/                 # 联调工具：模拟 Ctrl+Alt 拖动
├─ tools/verify-lang.ps1           # 校验四个 exe 的语言文案互不混入
└─ src/GhostNote/
   ├─ GhostNote.csproj             # 单文件发布 / 语言分支 / 体积配置
   ├─ App.xaml(.cs)                # 无主窗口启动、事件编排、状态落盘
   ├─ app.manifest                 # PerMonitorV2 DPI 感知
   ├─ Localization/Strings.cs      # 四语文案与字体（编译期 #if 分支）
   ├─ Interop/NativeMethods.cs     # 全部 P/Invoke 声明
   ├─ Models/NoteState.cs          # 持久化模型
   ├─ Services/
   │  ├─ StateStore.cs             # JSON 读写（原子写入）
   │  ├─ HotkeyHost.cs             # message-only 窗口 + RegisterHotKey
   │  ├─ MouseHook.cs              # WH_MOUSE_LL 全局钩子，实现拖拽
   │  ├─ Trace.cs                  # 可选诊断日志
   │  └─ TrayService.cs            # WinForms NotifyIcon 托盘（含字号档位）
   └─ Views/
      ├─ GhostWindow.xaml(.cs)     # 水印窗口：穿透、置顶、拖拽
      └─ InputWindow.xaml(.cs)     # 深色半透明输入框
```

## 本地构建

```bash
# 调试运行（默认简体中文，生成 GhostNote-zh-cn.exe）
dotnet run --project src/GhostNote/GhostNote.csproj

# 单独发布某一种语言
dotnet publish src/GhostNote/GhostNote.csproj -c Release -p:GhostNoteLang=ja -o publish/ja

# 一次发布四种语言
for lang in zh-cn zh-tw en ja; do
  dotnet publish src/GhostNote/GhostNote.csproj -c Release -p:GhostNoteLang=$lang -o publish/$lang
done
```

产物：`publish/<lang>/GhostNote-<lang>.exe`，每个约 470KB。

语言文案集中在 `src/GhostNote/Localization/Strings.cs`，新增语言时加一个 `#elif` 分支，再到 csproj 与 workflow 的语言列表里登记即可。

## 排查与联调

**拖拽不工作怎么查**：以 `GHOSTNOTE_TRACE=1` 启动，会在 `%TEMP%\ghostnote-trace.log` 记录钩子安装结果、每次左键按下时的 Ctrl/Alt 状态、拖拽起点与位移。

```bash
GHOSTNOTE_TRACE=1 ./publish/zh-cn/GhostNote-zh-cn.exe
```

**回归测试**：`tools/InputSim` 模拟「按住 Ctrl+Alt 左键拖动」，改过 `MouseHook` 后建议跑一次。

```bash
dotnet run --project tools/InputSim -c Release -- 400 400 240 120
```

**语言隔离校验**：`tools/verify-lang.ps1` 在四个 exe 里按 UTF-16 搜索各自文案，确认没有串语言。

注意一点：全局低级鼠标钩子在回调里**绝不能吞掉 `WM_MOUSEMOVE`**（返回非零），否则光标会被冻结，表现为「按住拖动却拖不动」。这里只吞掉按下和抬起的那一对消息。

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
git tag -a v1.1.0 -m "GhostNote v1.1.0"
git push origin v1.1.0
```

推送 tag 后 GitHub Actions 会：

1. 依次以 `zh-cn` / `zh-tw` / `en` / `ja` 四次 `dotnet publish`；
2. 校验每个 exe 只含本语言文案、且都小于 5MB；
3. 读取四语 Release 模板，追加四个附件的名称、大小与 SHA256；
4. 创建 GitHub Release，一次上传四个 exe。

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

`Foreground` 为 ARGB 十六进制，前两位是透明度；`FontSize` 一般不用手改——托盘菜单的「字号」里有小（28）/ 中（36）/ 大（48）三档，点一下立即生效。四个语言版本共用同一个状态文件。
