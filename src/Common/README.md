# Common - アプリケーション全体で使用される汎用ユーティリティ

## 責務

- 汎用ユーティリティ・ヘルパー
- 設定インターフェース（IApplicationSettings など）
- クロック実装（IClock）
- インフラストラクチャに依存しない汎用ライブラリ

## 依存関係

**許可:**
- SharedKernel

**禁止:**
- Domain, Application, Infrastructure, Presentation

## 実装ガイドライン

- Business Logic を含まない
- インフラストラクチャに依存しない汎用型のみ
- すべての層から参照可能

---

詳細は [CLEAN_ARCHITECTURE_GUIDELINES.md](../../docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md) を参照
