# SharedKernel - プロジェクト全体で共有される基盤層

## 責務

- Value Objects（ValueObject基底クラス）の定義
- エンティティの基底型
- 監査フィールド（CreatedAt, UpdatedBy など）
- プロジェクト全体で使用される列挙型・定数

## 依存関係

**許可:**
- Common

**禁止:**
- Application, Infrastructure, Presentation, Crosscutting

## 実装ガイドライン

- 他層への依存がないコードのみを実装
- Business Logic を含まない
- すべての層から参照可能な基本型のみ

---

詳細は [CLEAN_ARCHITECTURE_GUIDELINES.md](../../docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md) を参照
