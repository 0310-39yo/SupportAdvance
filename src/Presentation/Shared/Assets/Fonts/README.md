# アイコンフォント（Fluent UI System Icons）

WpfTrial・WinTrial で共用するアイコンフォントです。

| 項目 | 内容 |
|---|---|
| 名前 | Fluent UI System Icons |
| 配布元 | https://github.com/microsoft/fluentui-system-icons （`fonts` フォルダー） |
| ファイル | `FluentSystemIcons-Resizable.ttf`（ファミリー名 `FluentSystemIcons-Resizable`） |
| ライセンス | MIT（`LICENSE.txt`。再配布時は著作権表示とライセンス文を同梱すること） |
| 取得日 | 2026-09-26 |

## 使い方

- アイコンは `SupportAdvance.Presentation.Shared.Icons.AppIcon`（用途の名前）と `FluentIconCatalog`（字形の対応表）で管理します
- WpfTrial: `FluentIcon` コントロール（`<controls:FluentIcon Icon="Save" FontSize="18"/>`）
- WinTrial: `FluentIconFont`（アイコンの画像や `Font` を作成）
- フォントファイルはこのフォルダーの 1 つだけを実体とし、各アプリは csproj でリンクして取り込みます（コピーを置かない）

## アイコンの追加

`FluentIconCatalog` の説明を参照してください。コードポイントは、配布元の `FluentSystemIcons-Resizable.json`（キー: `ic_fluent_<名前>_20_regular`）で調べます。

## フォントの更新

新しい版に差し替える場合は、`.ttf` を差し替え、`FluentIconCatalog` の各コードポイントが同じアイコンを指していることを、配布元の JSON で確認してください（版によりコードポイントが変わることがあります）。
