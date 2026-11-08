<!--
  GitHub Release 说明模板（四种语言）。
  推 tag 后由 .github/workflows/release.yml 读取本文件作为 Release 正文，
  并在末尾自动追加四个语言附件的名称、大小与 SHA256 表格。
  发布新版本时改「本次更新 / What's new / 変更履歴」即可。
-->

# GhostNote

**简体中文** · **繁體中文** · **English** · **日本語**

把一句话，刻在桌面上。 / 把一句話，刻在桌面上。 / Etch a line onto your desktop. / デスクトップに、一言を刻む。

---

## 简体中文

> 你有没有过这种时刻：正在专注做事，脑子里却挂着另一件事——"下午三点回个电话""记得改 Dockerfile 里的端口"。写在便签纸上会丢，贴在桌面上会挡住图标，用待办软件又得先打开软件再找到那条待办。真正需要的，只是一句话，静静地待在屏幕上，看得见，又不挡路。

GhostNote 没有主界面，只活在系统托盘里。按下 `Ctrl + Alt + N`，敲一行字，回车——它就变成印在桌面上的浮水印：**置顶显示，鼠标完全穿透**，你照样能点开它后面的窗口、拖动图标、框选文件。

**它特别在哪**

- 无边框、无标题栏、无背景，只有一行浅灰色大字，带深色描边，白底文档和深色壁纸上都看得清。
- `WS_EX_TRANSPARENT | WS_EX_LAYERED` + `HWND_TOPMOST`：点击直接穿透，且永远在最上层。
- 按住 `Ctrl + Alt` 拖动即可改位置；关掉再开，文字和坐标原样回来。
- 托盘可切换字号小 / 中 / 大；本体只有一个 exe，不到 1MB，不联网、不写注册表。

**怎么用**

| 操作 | 效果 |
| --- | --- |
| `Ctrl + Alt + N` | 弹出输入框，`Enter` 保存，`Esc` 取消 |
| `Ctrl + Alt + 鼠标左键拖动` | 把水印拖到任意位置 |
| 托盘右键 → 字号 | 小 / 中 / 大三档，立即生效 |
| 托盘右键 | 修改文字 / 隐藏·显示水印 / 退出 |
| 双击托盘图标 | 打开输入框 |

留空并回车等于清掉水印；「隐藏水印」只是暂时藏起来，内容仍在。

**安装**：双击 exe 即可，无需安装、不弹 UAC。首次运行若提示缺少运行库，请先装 [.NET 8 桌面运行时（x64）](https://dotnet.microsoft.com/download/dotnet/8.0)。之所以不是零依赖，是为了把体积压在 5MB 内——WPF 不支持裁剪，框架依赖单文件是唯一能同时满足"能跑"和"够小"的方案。

**已知限制**：仅 Windows 10/11 x64；水印不参与输入，无法用鼠标选中复制；全屏独占游戏可能盖住置顶水印；只做一条水印。

**本次更新**

- 首个公开版本：托盘常驻、全局热键输入、鼠标穿透水印、Ctrl+Alt 拖拽、位置与内容本地记忆。
- 水印文字增加深色描边；托盘菜单新增字号小 / 中 / 大三档。
- 修复按住 Ctrl+Alt 拖动时鼠标被冻结、水印几乎拖不动的问题。
- 新增四种语言独立版本：简体中文、繁體中文、English、日本語。

---

## 繁體中文

> 你是否也有過這種時刻：正專心做事，腦子裡卻掛著另一件事——「下午三點回個電話」「記得改 Dockerfile 的連接埠」。寫在便條紙上會弄丟，貼在桌面上會擋住圖示，用待辦軟體又得先打開軟體、再找出那一筆。真正需要的，只是一句話，靜靜待在螢幕上，看得見，卻不擋路。

GhostNote 沒有主視窗，只活在系統托盤。按下 `Ctrl + Alt + N`，打一行字、按 Enter，它就變成印在桌面上的浮水印：**永遠置頂，滑鼠完全穿透**，你依然能點開它後面的視窗、拖動圖示、框選檔案。

**特色**

- 無邊框、無標題列、無背景，只有一行帶深色描邊的淺灰大字，白底文件與深色桌布都清晰可讀。
- `WS_EX_TRANSPARENT | WS_EX_LAYERED` 搭配 `HWND_TOPMOST`：點擊直接穿透，且常駐最上層。
- 按住 `Ctrl + Alt` 拖曳即可移動；關閉再開，文字與座標原樣回來。
- 托盤可切換字型大小小 / 中 / 大；本體只有一個 exe，不到 1MB，不連網、不寫登錄檔。

**操作方式**

| 操作 | 效果 |
| --- | --- |
| `Ctrl + Alt + N` | 開啟輸入框，`Enter` 儲存，`Esc` 取消 |
| `Ctrl + Alt + 滑鼠左鍵拖曳` | 把浮水印拖到任意位置 |
| 托盤右鍵 → 字型大小 | 小 / 中 / 大，立即生效 |
| 托盤右鍵 | 修改文字 / 隱藏·顯示浮水印 / 結束 |
| 雙擊托盤圖示 | 開啟輸入框 |

留空並按 Enter 等於清除浮水印；「隱藏浮水印」只是暫時收起，內容仍保留。

**安裝**：直接雙擊 exe，不需安裝、不跳出 UAC。首次執行若提示缺少執行階段，請先安裝 [.NET 8 桌面執行階段（x64）](https://dotnet.microsoft.com/download/dotnet/8.0)。選擇框架依賴是為了把體積控制在 5MB 以內——WPF 不支援裁剪，這是同時兼顧「能跑」與「夠小」的唯一做法。

**已知限制**：僅支援 Windows 10/11 x64；浮水印不接收滑鼠事件，無法選取複製文字；部分全螢幕獨佔遊戲可能蓋住置頂浮水印；一次只顯示一條。

---

## English

> You know the feeling: you're deep in work, and one thing keeps nagging at the back of your mind — "call back at 3", "fix the port in the Dockerfile". A sticky note gets lost, a desktop widget blocks your icons, and a to-do app means opening an app just to find that one line. What you actually need is a single line that stays on screen: visible, but never in the way.

GhostNote has no main window — it lives in the system tray. Press `Ctrl + Alt + N`, type a line, hit Enter, and it becomes a watermark etched onto your desktop: **always on top, fully click-through**. You can still click the windows behind it, drag icons, and select files.

**Why it's different**

- No border, no title bar, no background — just one large light-grey line with a dark outline, readable on both white documents and dark wallpapers.
- `WS_EX_TRANSPARENT | WS_EX_LAYERED` with `HWND_TOPMOST`: clicks pass straight through, and it stays above everything.
- Hold `Ctrl + Alt` and drag to reposition. Close it, reopen it — the text and position are right where you left them.
- Tray menu offers Small / Medium / Large font sizes. One exe, under 1MB, no network access, no registry changes.

**Usage**

| Action | Result |
| --- | --- |
| `Ctrl + Alt + N` | Open the input box; `Enter` saves, `Esc` cancels |
| `Ctrl + Alt + left-drag` | Move the watermark anywhere |
| Tray → Font size | Small / Medium / Large, applied instantly |
| Tray right-click | Edit text / Hide or show / Exit |
| Double-click tray icon | Open the input box |

Submit an empty line to clear the watermark; "Hide watermark" only hides it temporarily.

**Install**: just run the exe. No installer, no UAC prompt. If Windows asks for a runtime, install the [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0) first. That trade-off keeps the file under 5MB — WPF doesn't support trimming, so framework-dependent single-file publishing is the only way to be both runnable and tiny.

**Known limits**: Windows 10/11 x64 only; the watermark takes no input, so you can't select or copy its text; some exclusive-fullscreen games may cover it; one note at a time, by design.

---

## 日本語

> 作業に集中しているのに、頭の片隅で「15時に電話を返す」「Dockerfile のポートを直す」が離れない——そんな経験はありませんか。付箋は失くなるし、デスクトップに貼ればアイコンの邪魔になるし、TODO アプリなら起動して該当項目を探す手間が要る。本当に欲しいのは、画面に静かに居座る一行だけです。見えるのに、邪魔をしない一行。

GhostNote にメインウィンドウはありません。常駐するのはシステムトレイだけ。`Ctrl + Alt + N` で一行入力して Enter を押すと、それはデスクトップに刻まれた透かしになります。**常に最前面、しかもクリックは完全に素通り**。背後のウィンドウをクリックしても、アイコンをドラッグしても、範囲選択しても影響ありません。

**特徴**

- 枠なし・タイトルバーなし・背景なし。濃い縁取りを付けた薄いグレーの大きな文字だけなので、白い書類にも濃い壁紙にも読めます。
- `WS_EX_TRANSPARENT | WS_EX_LAYERED` と `HWND_TOPMOST` により、クリックは下のウィンドウへ透過し、表示は常に最上位。
- `Ctrl + Alt` を押しながらドラッグで移動。終了して再起動しても、文字と位置がそのまま復元されます。
- トレイから文字サイズを小 / 中 / 大に切替。本体は 1MB 未満の exe ひとつ、通信なし、レジストリ変更なし。

**使い方**

| 操作 | 動作 |
| --- | --- |
| `Ctrl + Alt + N` | 入力欄を開く。`Enter` で保存、`Esc` で取消 |
| `Ctrl + Alt + 左ドラッグ` | 透かしを好きな位置へ移動 |
| トレイ → 文字サイズ | 小 / 中 / 大、即時反映 |
| トレイ右クリック | テキスト編集 / 表示・非表示 / 終了 |
| トレイアイコンのダブルクリック | 入力欄を開く |

空のまま Enter で透かしを消去。「透かしを隠す」は一時的な非表示で、内容は残ります。

**インストール**：exe をダブルクリックするだけ。インストーラーも UAC も不要です。初回起動時にランタイムを求められたら、[.NET 8 デスクトップ ランタイム (x64)](https://dotnet.microsoft.com/download/dotnet/8.0) を導入してください。5MB 以下に抑えるための構成です。WPF はトリミング非対応なので、「フレームワーク依存 + 単一ファイル」が動く・小さいを両立できる唯一の方法です。

**既知の制限**：Windows 10/11 x64 のみ。透かしは入力を受け付けないため文字の選択・コピーはできません。排他フルスクリーンのゲームによっては隠れる場合があります。一度に表示できるのは一行だけです。
