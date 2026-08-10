# RoleAssignment - 技術仕様書

**対象者**: 開発者（RoleAssignment を実装・使用する実装者）

**版**: 1.0  
**作成日**: 2026-08-10

---

## 📖 概要

RoleAssignment は、Employee 集約に属する Child Entity です。従業員に割り当てられたロール（役割）とその有効期間を型安全に管理します。

- **何を実装するのか**: 従業員のロール割り当てと有効期間を管理
- **どこに実装するのか**: `src/Contexts/Employee/Employee.Domain/Entities/RoleAssignment.cs`
- **親Entity**: Employee（集約根）
- **特徴**: GUID ベースの集約、有効期間チェック機能

---

## 🎯 基本仕様

### 集約構成

```
Employee（集約根）
  └─ RoleAssignment（Child Entity）
       ├─ Id: RoleAssignmentId（GUID）
       ├─ RoleCode: RoleCode
       ├─ EffectiveDate: LocalDateTime
       └─ ExpirationDate: LocalDateTime?
```

### プロパティ仕様

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| **Id** | RoleAssignmentId | NOT NULL, PK | 集約根ID（GUID採番） |
| **RoleCode** | RoleCode | NOT NULL | ロールコード（M1234など） |
| **EffectiveDate** | LocalDateTime | NOT NULL | 有効開始日時（JST） |
| **ExpirationDate** | LocalDateTime? | NULL許可 | 有効終了日時（null=無期限） |

---

## 🏗️ API 仕様

### ファクトリメソッド

#### Create - 新規生成

```csharp
public static RoleAssignment Create(
    RoleCode roleCode,
    LocalDateTime effectiveDate,
    LocalDateTime? expirationDate = null)
{
    // 新しい RoleAssignmentId を自動採番
    // パラメータ検証は呼び出し元で実施
    // RoleAssignment インスタンスを生成して返す
}
```

**パラメータ**:
- `roleCode`: ロールコード（事前検証済み ValueObject）
- `effectiveDate`: 有効開始日時
- `expirationDate`: 有効終了日時（省略可）

**戻り値**: 生成された RoleAssignment

**例外**: 
- パラメータが null の場合は ArgumentNullException
- 検証失敗時は ArgumentException

#### Reconstruct - DB復元

```csharp
public static RoleAssignment Reconstruct(
    RoleAssignmentId id,
    RoleCode roleCode,
    LocalDateTime effectiveDate,
    LocalDateTime? expirationDate = null)
{
    // DB から読み込んだ値で RoleAssignment を復元
    // 値の再検証はしない（DB由来のため信頼できる）
}
```

**パラメータ**:
- `id`: DB から読み込んだ RoleAssignmentId
- `roleCode`: ロールコード
- `effectiveDate`: 有効開始日時
- `expirationDate`: 有効終了日時

**戻り値**: 復元された RoleAssignment

---

### ビジネスロジック

#### IsActive - 有効期間チェック

```csharp
public bool IsActive(LocalDateTime asOf)
{
    // 戻り値: 指定時点でロールが有効な場合 true
    // 
    // ロジック:
    // 1. asOf < EffectiveDate → false（開始前）
    // 2. ExpirationDate が null → true（無期限）
    // 3. asOf < ExpirationDate → true（有効期間内）
    // 4. asOf >= ExpirationDate → false（終了以後）
}
```

**パラメータ**:
- `asOf`: 判定時点（JST）

**戻り値**: 有効ならtrue、無効ならfalse

**例**:
```csharp
var roleAssignment = RoleAssignment.Create(
    roleCode,
    new LocalDateTime(new DateTime(2026, 1, 1)),
    new LocalDateTime(new DateTime(2026, 12, 31))
);

roleAssignment.IsActive(new LocalDateTime(new DateTime(2026, 6, 15)))  // → true
roleAssignment.IsActive(new LocalDateTime(new DateTime(2027, 1, 1)))   // → false
```

---

## 📋 使用例

### Employee に RoleAssignment を追加

```csharp
public class Employee : Entity<EmployeeId>
{
    private List<RoleAssignment> _roleAssignments = new();
    
    public void AssignRole(
        RoleCode roleCode,
        LocalDateTime effectiveDate,
        LocalDateTime? expirationDate = null)
    {
        var roleAssignment = RoleAssignment.Create(
            roleCode,
            effectiveDate,
            expirationDate);
        _roleAssignments.Add(roleAssignment);
    }
}
```

### Repository での復元

```csharp
public async Task<Employee> GetByIdAsync(EmployeeId id)
{
    var dbModel = await _context.Employees.FindAsync(id);
    
    var roleAssignments = dbModel.RoleAssignments
        .Select(r => RoleAssignment.Reconstruct(
            RoleAssignmentId.From(r.Id),
            RoleCode.From(r.Code),
            new LocalDateTime(r.EffectiveDate),
            r.ExpirationDate.HasValue 
                ? new LocalDateTime(r.ExpirationDate.Value)
                : null))
        .ToList();
    
    return Employee.Reconstruct(id, ..., roleAssignments);
}
```

---

## ✅ 検証ルール

### RoleCode 検証

- **必須**: NULL不可
- **形式**: 英数字アンダースコア（正規表現: `^[A-Za-z0-9_.]+$`）
- **長さ**: 1～50文字

### EffectiveDate 検証

- **必須**: NULL不可
- **形式**: LocalDateTime
- **制約**: 過去日時も許可（バックデート対応）

### ExpirationDate 検証

- **任意**: NULL許可（無期限）
- **形式**: LocalDateTime
- **制約**: EffectiveDate より後であることを推奨（検証はしない）

---

## 🔄 等価性

### 等価性判定

```csharp
// RoleAssignment は Entity<TId> なので、Id で比較
var roleA = RoleAssignment.Create(roleCode1, effectiveDate1);
var roleB = RoleAssignment.Create(roleCode1, effectiveDate1);

roleA == roleB  // → false（異なる Id）
```

### ハッシュコード

```csharp
var role = RoleAssignment.Create(roleCode, effectiveDate);
var dict = new Dictionary<RoleAssignment, string>();
dict.Add(role, "Role1");

dict[role]  // → "Role1"（Id で識別）
```

---

## 🚫 使用時の注意点

### ❌ null チェック不要

```csharp
// ❌ 間違い
if (roleAssignment != null) { ... }

// ✅ 正しい
// Entity は常に有効なインスタンス
```

### ⚠️ 有効期間の管理

```csharp
// DB では有効期間で管理
// Application / Presentation では IsActive() で判定
var now = _clock.JstNow;
var isActive = roleAssignment.IsActive(now);
```

### 📝 ExpirationDate なし（無期限）

```csharp
// ExpirationDate が null の場合、EffectiveDate 以後は常に有効
var roleAssignment = RoleAssignment.Create(
    roleCode,
    new LocalDateTime(new DateTime(2026, 1, 1))
    // expirationDate を省略
);

roleAssignment.IsActive(new LocalDateTime(new DateTime(2099, 12, 31)))
// → true（無期限なので常に有効）
```

---

## 参考資料

- 詳細設計: `RoleAssignment_詳細設計書.md`
- 単体テスト仕様: `RoleAssignment_単体テスト仕様書.md`
- 関連Entity: `DepartmentMembership_技術仕様書.md`
- ValueObject: `RoleCode_技術仕様書.md`
