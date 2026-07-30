# Crosscutting - 横断的関心事の実装

## 責務

- ロギング基盤
- 監査ログ機構
- 共通バリデーション
- 例外変換
- 複数の層で共有される横断的関心事

## 依存関係

**許可:**
- SharedKernel
- Common

**禁止:**
- Domain（原則として直接参照しない）
- Application（原則として直接参照しない）
- Infrastructure（循環参照になるため禁止）
- Presentation

## 実装ガイドライン

- Domain / Application への依存は避ける
- ロギング等の実装（NLog など）は NuGet パッケージとして直接参照。Infrastructure を経由しない
- インターフェース と実装の両方をこの層で自己完結させる（例：`IAppLogging<T>` と `FrameworkLoggingAdapter`）

---

詳細は [CLEAN_ARCHITECTURE_GUIDELINES.md](../../docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md) を参照
