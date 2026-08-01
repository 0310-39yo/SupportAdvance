# Identity.Domain レイヤー

認証・認可のドメインロジック。

## エンティティ設計

### User
- `AggregateRoot<RowId>` パターン
- LoginId, Email, HashedPassword を必須プロパティ
- IsActive フラグで有効/無効を管理
- 変更メソッド：ChangePassword, SetActive, UpdateEmail

### Role
- `AggregateRoot<RowId>` パターン
- Name（ユニーク）、Permissions（JSON シリアライズ対象）
- IsActive フラグで有効/無効を管理

### UserRole
- User と Role の関連付け（多対多）
- `AggregateRoot<RowId>` パターン
- AssignedAt に LocalDateTime（JST）を使用

## RowId ValueObject

すべてのエンティティ ID は `RowId` ValueObject を使用。
- `Entity<RowId>` 継承
- DB 採番前は `RowId.New()`（value=0）
- DB 採番後は `RowId.From(dbValue)` で作成

詳細は [SharedKernel](../../SharedKernel/ValueObjects/Identifiers/RowId.cs) 参照。

## 依存関係

### 許可される参照
- SharedKernel（Entity, ValueObject 基底等）
- Common（ユーティリティ）

### 禁止される参照
- Application / Infrastructure / Presentation
- Domain 層は完全独立

## 注意事項

- Domain は技術詳細（DB、ORM）を一切含まない
- 暗号化等のドメインロジックはここで定義
- Repository インターフェースは Application で定義
