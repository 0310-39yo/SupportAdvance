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
- Infrastructure（ロギングプロバイダーの実装依存）

**禁止:**
- Domain（原則として直接参照しない）
- Application（原則として直接参照しない）
- Presentation

## 実装ガイドライン

- Domain への依存は最小限に
- Application への依存は避ける
- インターフェース定義は Common / SharedKernel に配置

---

詳細は [CLEAN_ARCHITECTURE_GUIDELINES.md](../../docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md) を参照
