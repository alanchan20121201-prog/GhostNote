# GhostNote

Etch a single line of text onto your desktop as a watermark: always on top, fully click-through, never in the way.

A tiny Windows desktop utility built with .NET 8 + WPF. It lives in the system tray and has no main window.

**Documentation**: [简体中文](README.md) · [繁體中文](README.zh-TW.md) · [English](README.en.md) · [日本語](README.ja.md)

## Four language builds

All four executables are functionally identical — only the UI strings and fonts differ. Download whichever you need:

| File | Language | Font |
| --- | --- | --- |
| `GhostNote-zh-cn.exe` | Simplified Chinese | Microsoft YaHei Light |
| `GhostNote-zh-tw.exe` | Traditional Chinese | Microsoft JhengHei UI Light |
| `GhostNote-en.exe` | English | Segoe UI Light |
| `GhostNote-ja.exe` | Japanese | Yu Gothic UI Light |

The language is fixed at **compile time** via the `GhostNoteLang` MSBuild property. There is no runtime locale detection and no satellite assemblies, so each exe contains exactly one set of strings.

## Features

- Starts with a tray icon only — no main window.
- Global hotkey `Ctrl + Alt + N` opens a minimal dark translucent input box in the center of the screen; `Enter` saves, `Esc` cancels.
- The text becomes a borderless, titlebar-free floating watermark: light grey with a dark outline and a soft shadow, readable on white documents and dark wallpapers alike.
- The window uses `WS_EX_TRANSPARENT | WS_EX_LAYERED` plus `HWND_TOPMOST`: clicks pass through to whatever is underneath, and it stays on top.
- Hold `Ctrl + Alt` and left-drag to move the watermark anywhere (a global low-level mouse hook is required, since the window itself receives no mouse messages).
- Text and screen position are saved to `%AppData%\GhostNote\state.json` and restored on the next launch.
- Tray menu: Edit text / Font size (Small · Medium · Large) / Hide or show / Exit.

## Layout

```
GhostNote/
├─ .github/workflows/release.yml   # builds four languages and publishes a Release on tag push
├─ docs/RELEASE_NOTES_TEMPLATE.md  # four-language release notes template
├─ README*.md                      # documentation in four languages
├─ tools/make-icon.ps1             # generates the multi-size icon
├─ tools/InputSim/                 # simulates Ctrl+Alt dragging for regression tests
├─ tools/verify-lang.ps1           # checks that each exe contains only its own strings
└─ src/GhostNote/
   ├─ GhostNote.csproj             # single-file publish / language switch / size settings
   ├─ App.xaml(.cs)                # startup without a main window, wiring, state persistence
   ├─ Localization/Strings.cs      # all four languages and fonts (compile-time #if branches)
   ├─ Interop/NativeMethods.cs     # every P/Invoke declaration
   ├─ Services/MouseHook.cs        # WH_MOUSE_LL hook that powers dragging
   └─ Views/
      ├─ GhostWindow.xaml(.cs)     # click-through, topmost watermark
      └─ InputWindow.xaml(.cs)     # dark translucent input box
```

## Building locally

```bash
# Debug run (defaults to Simplified Chinese, produces GhostNote-zh-cn.exe)
dotnet run --project src/GhostNote/GhostNote.csproj

# Publish one language
dotnet publish src/GhostNote/GhostNote.csproj -c Release -p:GhostNoteLang=en -o publish/en

# Publish all four
for lang in zh-cn zh-tw en ja; do
  dotnet publish src/GhostNote/GhostNote.csproj -c Release -p:GhostNoteLang=$lang -o publish/$lang
done
```

Output: `publish/<lang>/GhostNote-<lang>.exe`, roughly 470 KB each.

All UI strings live in `src/GhostNote/Localization/Strings.cs`. To add a language, add an `#elif` branch there and register the tag in the csproj and the workflow language list.

## Troubleshooting

**Dragging doesn't work**: launch with `GHOSTNOTE_TRACE=1`. It writes `%TEMP%\ghostnote-trace.log` with hook installation results, Ctrl/Alt state on every left click, and drag deltas.

```bash
GHOSTNOTE_TRACE=1 ./publish/en/GhostNote-en.exe
```

**Regression tests**: `tools/InputSim` simulates holding Ctrl+Alt and dragging; `tools/verify-lang.ps1` confirms no cross-language contamination between builds.

```bash
dotnet run --project tools/InputSim -c Release -- 400 400 240 120
```

One hard-won detail: a global low-level mouse hook must **never swallow `WM_MOUSEMOVE`** (return non-zero). Doing so freezes the cursor and makes the watermark look impossible to drag. This project only swallows the down/up pair.

## Size and trimming

The original requirement asked for `PublishSingleFile=true` + `PublishTrimmed=true` with a size under 5MB. There is an objective constraint:

**WPF does not support trimming.** With `PublishTrimmed` enabled the build only warns, but XAML/BAML reflection dependencies get cut and the exe crashes on launch — there is no configuration workaround. This project therefore uses `PublishTrimmed=false`.

The size target is met with "framework-dependent + single file" instead:

| Approach | Setting | Size | User dependency |
| --- | --- | --- | --- |
| This project | `SelfContained=false` + single file | ~0.5 MB | .NET 8 Desktop Runtime required |
| Self-contained | `SelfContained=true` + single file | 60 MB+ | None |

If "zero dependencies" matters more, set `SelfContained` to `true` in the csproj — at the cost of a far larger file. On first launch, if Windows asks for a runtime, install the [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0).

## Releasing

```bash
git tag -a v1.1.0 -m "GhostNote v1.1.0"
git push origin v1.1.0
```

On tag push, GitHub Actions: publishes each of the four languages in turn → verifies each exe contains only its own strings and is under 5MB → reads the four-language release template and appends the name, size and SHA256 of each of the four files → creates the Release with all four exes attached.

## State file

`%AppData%\GhostNote\state.json`:

```json
{
  "Text": "Call back at 3",
  "X": 620,
  "Y": 300,
  "HasPosition": true,
  "Visible": true,
  "FontSize": 36,
  "Foreground": "#B3E8E8E8"
}
```

`Foreground` is ARGB hex — the first two digits are alpha. You rarely need to edit `FontSize` by hand: the tray's Font size menu offers Small (28) / Medium (36) / Large (48) and applies instantly. All four language builds share the same state file.
