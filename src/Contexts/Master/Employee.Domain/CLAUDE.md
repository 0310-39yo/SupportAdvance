# Employee.Domain レイヤー

従業員管理のドメインロジック。

## エンティティ設計

### Employee
- `AggregateRoot<EmployeeId>` パターン（GUID ベースの AggregateId）
- RowId をプライベート属性として保持（テーブルの物理キー）
- EmployeeNumber（社員番号、ユニーク）を識別子として使用
- FirstName, LastName で姓名を分離
- Email、DepartmentId、JobTitle を属性に持つ
- HireDate（入社日）に LocalDateTime（JST）を使用
- IsActive フラグで有効/無効を管理
- 変更メソッド：TransferDepartment, ChangeJobTitle, SetActive

### Department
- `AggregateRoot<DepartmentId>` パターン（GUID ベースの AggregateId）
- RowId をプライベート属性として保持（テーブルの物理キー）
- Name（ユニーク）を主要識別子
- ParentDepartmentId で部門階層をサポート
- IsActive フラグで有効/無効を管理
- 変更メソッド：UpdateName, SetActive

## ID 設計

### AggregateId（集約ビジネスID）
- EmployeeId, DepartmentId は AggregateId を継承
- GUID ベース（型安全性確保）
- Entity.Id で参照

### RowId（テーブル物理キー）
- Entity のプライベート属性
- テーブルの行を一意に識別
- DB 採番前は `RowId.New()`（value=0）
- DB 採番後は `RowId.From(dbValue)` で作成

詳細は [SharedKernel](../../SharedKernel/ValueObjects/Identifiers/) 参照。

## LocalDateTime 使用

- HireDate に LocalDateTime（JST）を使用
- 全層で同一タイムゾーン管理を保証

## 依存関係

### 許可される参照
- SharedKernel（Entity, ValueObject 基底等）
- Common（ユーティリティ）

### 禁止される参照
- Application / Infrastructure / Presentation
- Domain 層は完全独立

## 注意事項

- Domain は技術詳細（DB、ORM）を一切含まない
- Repository インターフェースは Application で定義
