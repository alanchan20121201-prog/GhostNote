# GhostNote ゴーストメモ

一言をデスクトップの透かしとして刻むツール：常に最前面、クリックは完全に透過、操作の邪魔をしません。

.NET 8 + WPF で作った Windows 用小物です。システムトレイに常駐し、メインウィンドウはありません。

**ドキュメント**：[简体中文](README.md) · [繁體中文](README.zh-TW.md) · [English](README.en.md) · [日本語](README.ja.md)

## 4 つの言語版

4 つの exe は機能が完全に同一で、異なるのは UI の文言とフォントだけです。必要なものを 1 つ選んでください。

| ファイル名 | 言語 | フォント |
| --- | --- | --- |
| `GhostNote-zh-cn.exe` | 簡体字中国語 | Microsoft YaHei Light |
| `GhostNote-zh-tw.exe` | 繁体字中国語 | Microsoft JhengHei UI Light |
| `GhostNote-en.exe` | 英語 | Segoe UI Light |
| `GhostNote-ja.exe` | 日本語 | Yu Gothic UI Light |

言語は `GhostNoteLang` MSBuild プロパティで**コンパイル時に**固定されます。実行時のロケール判定もサテライト アセンブリもないため、各 exe には 1 言語分の文言しか含まれません。

## 機能

- 起動後はシステムトレイのアイコンだけ。メインウィンドウはありません。
- グローバル ショートカット `Ctrl + Alt + N` で、画面中央にミニマルな濃色半透明の入力欄を表示。`Enter` で保存、`Esc` で取消。
- 入力した文字は枠なし・タイトルバーなしの浮遊する透かしに。薄いグレーに濃い縁取りと軽い影を付け、白い書類にも濃い壁紙にも読めます。
- ウィンドウは `WS_EX_TRANSPARENT | WS_EX_LAYERED` + `HWND_TOPMOST`。クリックは下のウィンドウへ素通りし、表示は常に最上位です。
- `Ctrl + Alt` を押しながら左ドラッグで任意の位置へ移動（ウィンドウ自体はマウス メッセージを受け取らないため、グローバル マウス フックを使用）。
- 文字と画面座標は `%AppData%\GhostNote\state.json` に保存され、再起動時に復元されます。
- トレイ メニュー：テキスト編集 / 文字サイズ（小・中・大） / 表示・非表示 / 終了。

## 構成

```
GhostNote/
├─ .github/workflows/release.yml   # tag で 4 言語をビルドし Release を公開
├─ docs/RELEASE_NOTES_TEMPLATE.md  # 4 言語の Release 説明テンプレート
├─ README*.md                      # 4 言語のドキュメント
├─ tools/make-icon.ps1             # 複数サイズのアイコンを生成
├─ tools/InputSim/                 # Ctrl+Alt ドラッグを再現する検証ツール
├─ tools/verify-lang.ps1           # exe ごとの文言混在を検査
└─ src/GhostNote/
   ├─ GhostNote.csproj             # 単一ファイル公開 / 言語切替 / サイズ設定
   ├─ App.xaml(.cs)                # メインウィンドウなしの起動・配線・状態保存
   ├─ Localization/Strings.cs      # 4 言語の文言とフォント（コンパイル時 #if）
   ├─ Interop/NativeMethods.cs     # P/Invoke 宣言一式
   ├─ Services/MouseHook.cs        # ドラッグを実現する WH_MOUSE_LL フック
   └─ Views/
      ├─ GhostWindow.xaml(.cs)     # クリック透過・最前面の透かし
      └─ InputWindow.xaml(.cs)     # 濃色半透明の入力欄
```

## ローカル ビルド

```bash
# デバッグ実行（既定は簡体字中国語、GhostNote-zh-cn.exe が生成される）
dotnet run --project src/GhostNote/GhostNote.csproj

# 1 言語だけ公開
dotnet publish src/GhostNote/GhostNote.csproj -c Release -p:GhostNoteLang=ja -o publish/ja

# 4 言語まとめて公開
for lang in zh-cn zh-tw en ja; do
  dotnet publish src/GhostNote/GhostNote.csproj -c Release -p:GhostNoteLang=$lang -o publish/$lang
done
```

成果物：`publish/<lang>/GhostNote-<lang>.exe`（各 約 470KB）。

文言は `src/GhostNote/Localization/Strings.cs` に集約されています。言語を増やす場合は `#elif` を 1 つ追加し、csproj と workflow の言語一覧に登録するだけです。

## トラブルシュート

**ドラッグできないとき**：`GHOSTNOTE_TRACE=1` を付けて起動すると、`%TEMP%\ghostnote-trace.log` にフックの登録結果、左クリック時の Ctrl/Alt 状態、移動量が記録されます。

```bash
GHOSTNOTE_TRACE=1 ./publish/ja/GhostNote-ja.exe
```

**回帰テスト**：`tools/InputSim` は「Ctrl+Alt を押しながら左ドラッグ」を再現します。`tools/verify-lang.ps1` は 4 つの exe に他言語の文言が混ざっていないか確認します。

```bash
dotnet run --project tools/InputSim -c Release -- 400 400 240 120
```

重要な注意点：グローバルな低レベル マウス フックで **`WM_MOUSEMOVE` を握りつぶしてはいけません**（非ゼロを返す）。カーソルが凍結し、「押してドラッグしても動かない」という症状になります。ここでは按下と解放の 1 組だけを消費します。

## サイズとトリミングについて

当初の要件は `PublishSingleFile=true` + `PublishTrimmed=true` かつ 5MB 未満でしたが、客観的な制約があります。

**WPF はトリミング非対応です。** `PublishTrimmed` を有効にするとビルドは警告だけで通りますが、XAML/BAML の反射依存が削られ、exe は起動直後にクラッシュします。設定で回避する方法はありません。そのため本プロジェクトは `PublishTrimmed=false` としています。

サイズは「フレームワーク依存 + 単一ファイル」で達成しています：

| 方式 | 設定 | サイズ | 利用者の依存 |
| --- | --- | --- | --- |
| 本プロジェクト | `SelfContained=false` + 単一ファイル | 約 0.5 MB | .NET 8 デスクトップ ランタイムが必要 |
| 自己完結 | `SelfContained=true` + 単一ファイル | 60 MB+ | なし |

「依存ゼロ」を優先するなら csproj の `SelfContained` を `true` にしてください。ただしサイズは 5MB を大きく超えます。初回起動でランタイムを求められた場合は [.NET 8 デスクトップ ランタイム (x64)](https://dotnet.microsoft.com/download/dotnet/8.0) を導入してください。

## リリース

```bash
git tag -a v1.1.0 -m "GhostNote v1.1.0"
git push origin v1.1.0
```

tag を push すると GitHub Actions が：4 言語を順に publish → 各 exe が自言語の文言だけを含み 5MB 未満であることを検証 → 4 言語テンプレートに 4 ファイルの名前・サイズ・SHA256 を追記 → 4 つの exe を添付した Release を作成、という流れで動きます。

## 状態ファイル

`%AppData%\GhostNote\state.json`：

```json
{
  "Text": "15時に電話を返す",
  "X": 620,
  "Y": 300,
  "HasPosition": true,
  "Visible": true,
  "FontSize": 36,
  "Foreground": "#B3E8E8E8"
}
```

`Foreground` は ARGB の 16 進数で、先頭 2 桁が透明度です。`FontSize` を手で編集する必要はほぼありません。トレイの「文字サイズ」に小（28）/ 中（36）/ 大（48）の 3 段階があり、即時反映されます。4 つの言語版は同じ状態ファイルを共有します。
