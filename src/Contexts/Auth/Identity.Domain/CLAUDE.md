# Identity.Domain レイヤー

認証・認可のドメインロジック。

## エンティティ設計

### User
- `AggregateRoot<UserId>` パターン（GUID ベースの AggregateId）
- RowId をプライベート属性として保持（テーブルの物理キー）
- LoginId, Email, HashedPassword を必須プロパティ
- IsActive フラグで有効/無効を管理
- 変更メソッド：ChangePassword, SetActive, UpdateEmail

### Role
- `AggregateRoot<RoleId>` パターン（GUID ベースの AggregateId）
- RowId をプライベート属性として保持（テーブルの物理キー）
- Name（ユニーク）、Permissions（JSON シリアライズ対象）
- IsActive フラグで有効/無効を管理

### UserRole
- User と Role の関連付け（多対多）
- `AggregateRoot<UserRoleId>` パターン（GUID ベースの AggregateId）
- RowId をプライベート属性として保持（テーブルの物理キー）
- AssignedAt に LocalDateTime（JST）を使用

## ID 設計

### AggregateId（集約ビジネスID）
- UserId, RoleId, UserRoleId は AggregateId を継承
- GUID ベース（型安全性確保）
- Entity.Id で参照

### RowId（テーブル物理キー）
- Entity のプライベート属性
- テーブルの行を一意に識別
- DB 採番前は `RowId.New()`（value=0）
- DB 採番後は `RowId.From(dbValue)` で作成

詳細は [SharedKernel](../../SharedKernel/ValueObjects/Identifiers/) 参照。

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
