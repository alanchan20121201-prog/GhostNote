# GhostNote 幽靈便籤

把一句話像浮水印一樣刻在桌面上：永遠置頂、滑鼠完全穿透、不擋任何操作。

Windows 桌面小工具，.NET 8 + WPF 編寫，托盤常駐，無主視窗。

**文件語言**：[简体中文](README.md) · [繁體中文](README.zh-TW.md) · [English](README.en.md) · [日本語](README.ja.md)

## 四種語言版本

四個 exe 功能完全相同，只有介面文案與字型不同，擇一下載即可：

| 檔案名稱 | 語言 | 字型 |
| --- | --- | --- |
| `GhostNote-zh-cn.exe` | 簡體中文 | 微軟雅黑 Light |
| `GhostNote-zh-tw.exe` | 繁體中文 | 微軟正黑體 Light |
| `GhostNote-en.exe` | English | Segoe UI Light |
| `GhostNote-ja.exe` | 日本語 | 游ゴシック UI Light |

語言在**編譯期**由 `GhostNoteLang` 屬性固定，執行時不判斷系統地區、不載入附屬組件，因此每個 exe 只含一種文案。

## 功能

- 啟動後只有系統托盤圖示，沒有主視窗。
- 全域快速鍵 `Ctrl + Alt + N` 在螢幕中央彈出極簡深色半透明輸入框，`Enter` 儲存，`Esc` 取消。
- 文字變成無邊框、無標題列的懸浮浮水印：淺灰色、深色描邊 + 輕微陰影（白底、深色背景都看得清）。
- 視窗設定 `WS_EX_TRANSPARENT | WS_EX_LAYERED` + `HWND_TOPMOST`：點擊直接穿透到下層視窗，且永遠置頂。
- `Ctrl + Alt + 滑鼠左鍵` 拖曳浮水印到任意位置（走全域滑鼠鉤子，因為視窗本身收不到滑鼠訊息）。
- 文字內容與螢幕座標儲存到 `%AppData%\GhostNote\state.json`，重啟後自動還原。
- 托盤選單：修改文字 / 字型大小（小·中·大） / 隱藏·顯示浮水印 / 結束。

## 目錄結構

```
GhostNote/
├─ .github/workflows/release.yml   # 打 tag 自動編譯四語言並發布 Release
├─ docs/RELEASE_NOTES_TEMPLATE.md  # 四語 Release 說明範本
├─ README*.md                      # 四種語言的說明文件
├─ tools/make-icon.ps1             # 產生多尺寸應用程式圖示
├─ tools/InputSim/                 # 聯調工具：模擬 Ctrl+Alt 拖曳
├─ tools/verify-lang.ps1           # 校驗四個 exe 的文案互不混入
└─ src/GhostNote/
   ├─ GhostNote.csproj             # 單檔發布 / 語言分支 / 體積設定
   ├─ App.xaml(.cs)                # 無主視窗啟動、事件編排、狀態落盤
   ├─ Localization/Strings.cs      # 四語文案與字型（編譯期 #if 分支）
   ├─ Interop/NativeMethods.cs     # 全部 P/Invoke 宣告
   └─ Views/
      ├─ GhostWindow.xaml(.cs)     # 浮水印視窗：穿透、置頂、拖曳
      └─ InputWindow.xaml(.cs)     # 深色半透明輸入框
```

## 本機建置

```bash
# 除錯執行（預設簡體中文，產生 GhostNote-zh-cn.exe）
dotnet run --project src/GhostNote/GhostNote.csproj

# 單獨發布某一種語言
dotnet publish src/GhostNote/GhostNote.csproj -c Release -p:GhostNoteLang=zh-tw -o publish/zh-tw

# 一次發布四種語言
for lang in zh-cn zh-tw en ja; do
  dotnet publish src/GhostNote/GhostNote.csproj -c Release -p:GhostNoteLang=$lang -o publish/$lang
done
```

產物：`publish/<lang>/GhostNote-<lang>.exe`，每個約 470KB。

文案集中在 `src/GhostNote/Localization/Strings.cs`，新增語言時加一個 `#elif` 分支，再到 csproj 與 workflow 的語言清單登記即可。

## 疑難排解

**拖不動怎麼查**：以 `GHOSTNOTE_TRACE=1` 啟動，會在 `%TEMP%\ghostnote-trace.log` 記錄鉤子安裝結果、Ctrl/Alt 狀態與位移。

```bash
GHOSTNOTE_TRACE=1 ./publish/zh-tw/GhostNote-zh-tw.exe
```

**回歸測試**：`tools/InputSim` 模擬「按住 Ctrl+Alt 左鍵拖曳」；`tools/verify-lang.ps1` 檢查四個 exe 沒有串語言。

```bash
dotnet run --project tools/InputSim -c Release -- 400 400 240 120
```

注意：全域低階滑鼠鉤子**絕不能吞掉 `WM_MOUSEMOVE`**（回傳非零），否則游標會被凍結，表現為「按住拖曳卻拖不動」。這裡只吞掉按下與抬起那一對訊息。

## 關於體積與裁剪

需求要求 `PublishSingleFile=true` + `PublishTrimmed=true` 且體積 < 5MB，但有一個客觀限制：

**WPF 不支援裁剪。** 開啟 `PublishTrimmed` 後，編譯只會警告，但 XAML/BAML 的反射依賴會被剪掉，雙擊執行直接崩潰，無法透過設定修復。因此本專案使用 `PublishTrimmed=false`。

體積靠「框架依賴 + 單檔」達成：

| 方案 | 設定 | 體積 | 使用者依賴 |
| --- | --- | --- | --- |
| 本專案 | `SelfContained=false` + 單檔 | 約 0.5 MB | 需裝 .NET 8 桌面執行階段 |
| 自包含 | `SelfContained=true` + 單檔 | 60 MB+ | 無 |

若更在意「使用者零依賴」，把 csproj 的 `SelfContained` 改成 `true`，代價是體積遠超 5MB。首次執行若提示缺少執行階段，請安裝 [.NET 8 桌面執行階段（x64）](https://dotnet.microsoft.com/download/dotnet/8.0)。

## 發布

```bash
git tag -a v1.1.0 -m "GhostNote v1.1.0"
git push origin v1.1.0
```

推送 tag 後 GitHub Actions 會：依序以四種語言各 publish 一次 → 校驗每個 exe 只含本語言文案且小於 5MB → 讀取四語範本並附加四個附件的 SHA256 → 建立 Release 並一次上傳四個 exe。

## 狀態檔

`%AppData%\GhostNote\state.json`：

```json
{
  "Text": "下午三點回電話",
  "X": 620,
  "Y": 300,
  "HasPosition": true,
  "Visible": true,
  "FontSize": 36,
  "Foreground": "#B3E8E8E8"
}
```

`Foreground` 為 ARGB 十六進位，前兩位為透明度；`FontSize` 一般不用手改，托盤選單的「字型大小」有小（28）/ 中（36）/ 大（48）三檔，點一下立即生效。四個語言版本共用同一個狀態檔。
