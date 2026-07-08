# Infrastructure - Database / External Service implementations

## 責務

- データベース実装（Repository パターン）
- ORM 統合（Dapper, RepoDB など）
- 外部API/サービスの実装
- ファイルシステムアクセス
- Configuration プロバイダー

## 依存関係

**許可:**
- SharedKernel
- Common
- Crosscutting（ロギング）
- Domain（Entity/Value Object のマッピング用）

**禁止:**
- ✓ Application（修正済み）
- Presentation

## 実装ガイドライン

- 技術的な詳細を隠蔽
- Application / Domain のインターフェースを実装
- Application が定義したインターフェースを実装する

---

詳細は [CLEAN_ARCHITECTURE_GUIDELINES.md](../../docs/CLEAN_ARCHITECTURE_GUIDELINES.md) を参照
