# Application - Use Cases / Application Services

## 責務

- Use Cases（ユースケース）の実装
- Application Services（アプリケーションサービス）
- DTO（Data Transfer Objects）
- コマンド / クエリ
- Infrastructure への呼び出し調整

## 依存関係

**許可:**
- SharedKernel
- Common
- Domain（各Bounded Context）
- Crosscutting（ロギング等）

**禁止:**
- Infrastructure（直接参照は禁止、DI により注入）
- Presentation

## 実装ガイドライン

- ドメインロジックを含まない
- Infrastructure への依存は DI により逆転されるべき
- Presentation 層から直接呼び出される
- インターフェースを定義し、Infrastructure で実装

---

詳細は [CLEAN_ARCHITECTURE_GUIDELINES.md](../../docs/CLEAN_ARCHITECTURE_GUIDELINES.md) を参照
