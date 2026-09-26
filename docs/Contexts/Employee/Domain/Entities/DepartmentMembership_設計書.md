# DepartmentMembership（所属）設計書

**版:** 1.0
**作成日:** 2026-09-26
**対象読者:** 実装者・テスト実装者
**実装:** [DepartmentMembership.cs](../../../../../src/Contexts/Employee/Employee.Domain/Entities/DepartmentMembership.cs)
**テスト:** [DepartmentMembershipTests.cs](../../../../../tests/Contexts/Employee.Domain.Tests/Entities/DepartmentMembershipTests.cs)

---

## 1. 位置づけ

`DepartmentMembership` は、従業員（`Employee` 集約）がどの部署に所属するかを表すエンティティ。`Employee` 集約の内側にあり、`Employee.DepartmentMemberships` から辿る。対応するテーブルは `m_department_memberships`（[DDL](../../../../Database/SQL/CREATE_m_department_memberships.sql)）。

部署そのもの（`Department` 集約）は、別の Bounded Context（Department）が持つ。所属は部署の行ID（`DepartmentRowId`）で参照するだけで、Department の型には依存しない。

## 2. 項目

| プロパティ | 型 | 必須 | 説明 |
|---|---|---|---|
| `RowId` | `DepartmentMembershipRowId` | ✓ | 所属の行ID |
| `EmployeeRowId` | `EmployeeRowId` | ✓ | 所属する従業員 |
| `DepartmentRowId` | `DepartmentRowId`（SharedKernel） | ✓ | 所属先の部署 |
| `IsPrimary` | `IsPrimary` | ✓ | 主部署か副部署か（`Primary()` / `Secondary()`） |
| `EndOn` | `EndOn` | - | 所属の終了日。未設定（`Unset()`）は無期限 |
| `DepartmentDisplayName` | `DepartmentDisplayName` | - | 表示用の部署名。未設定（`Unset()`）は名前なし |

Domain 層に `null` は現れない。任意の項目は、型の `Unset()` で未設定を表す（[null 厳格性設計ガイド](../../../../Assistance/Guides/null厳格性設計ガイド.md)）。

## 3. 生成

| メソッド | 用途 |
|---|---|
| `Create(membershipRowId, employeeRowId, departmentRowId, isPrimary, endOn = null, departmentDisplayName = null)` | 新規作成。`endOn`・`departmentDisplayName` を省略すると、いずれも `Unset()` になる |
| `Reconstruct(membershipRowId, employeeRowId, departmentRowId, isPrimary, endOn, departmentDisplayName = null)` | DB からの復元（Mapper が使用）。`endOn` は必須 |

## 4. ビジネスルール

### 4-1. `IsActive(LocalDateTime asOf)`

| 条件 | 結果 |
|---|---|
| `EndOn` が未設定（無期限） | 常に有効（`true`） |
| `asOf < EndOn` | 有効 |
| `asOf >= EndOn` | 無効（**終了日の時刻ちょうどから無効**） |

### 4-2. `DepartmentDisplayName` が Unset を持つ理由

所属の部署名は、DB では `m_department_memberships.department_name`（NULL 可）に持ち、部署マスターとの LEFT JOIN で取得する。部署が見つからない場合などに NULL があり得るため、必須の値オブジェクトにはできない。

| 型 | 場所 | 必須か |
|---|---|---|
| `DepartmentName` | Department.Domain | 必須（1〜50 文字。DB の `department_name` は `nvarchar(50)`。2026-09-26 に 100 から修正） |
| `DepartmentDisplayName` | Employee.Domain | 任意（`Unset()` あり。別名 `HasName`） |

`DepartmentDisplayName.Value` は、未設定なら空文字を返す（`null` ではない）。DTO への変換（`EmployeeExtensions.ToDto`）は、この空文字をそのまま部署名の連結に使う。

## 5. DB との変換（Infrastructure）

[EmployeeMapper](../../../../../src/Contexts/Employee/Employee.Infrastructure/Mappers/EmployeeMapper.cs) が変換する。

| 方向 | 変換 |
|---|---|
| DbModel → Entity | `department_name` が `null` または空文字 → `DepartmentDisplayName.Unset()`。値あり → `From(値)` |
| Entity → DbModel | `HasName` が `false` → `null`。`true` → 値 |
| `end_on`（`DateTime?`） | 値あり → `EndOn.From(new LocalDateTime(値))`、`null` → `EndOn.Unset()`。逆方向は `HasEnded` なら値、そうでなければ `null` |

## 6. 既知の挙動

- 所属の一覧から部署名を連結するとき（`EmployeeExtensions.ToDto`）、主部署が先頭になり「, 」で区切る。部署名が未設定の所属と設定済みの所属が混在すると、空の要素が入る（例:「営業部, 」）。現行の実装の挙動で、テストでは固定していない

## 7. 関連ドキュメント

- [日付系 ValueObject 仕様書](../ValueObjects/日付系ValueObject_仕様書.md)（`EndOn` など）
- [Employee_設計方針書](Employee_設計方針書.md)
- [Employee Infrastructure 単体テスト仕様書](../../Infrastructure/Infrastructure_単体テスト仕様書.md)（Mapper の部署名の変換）
