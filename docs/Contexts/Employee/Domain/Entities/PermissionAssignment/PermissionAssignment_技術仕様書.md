# PermissionAssignment - 技術仕様書

**対象者**: 開発者（PermissionAssignment を実装・使用する実装者）

**版**: 1.0  
**作成日**: 2026-08-10

---

## 📖 概要

PermissionAssignment は、Employee 集約に属する Child Entity です。従業員に割り当てられた権限（パーミッション）とその有効期間を型安全に管理します。

- **何を実装するのか**: 従業員の権限割り当てと有効期間を管理
- **どこに実装するのか**: `src/Contexts/Employee/Employee.Domain/Entities/PermissionAssignment.cs`
- **親Entity**: Employee（集約根）
- **特徴**: GUID ベースの集約、有効期間チェック機能

---

## 🎯 基本仕様

### 集約構成

```
Employee（集約根）
  └─ PermissionAssignment（Child Entity）
       ├─ Id: PermissionAssignmentId（GUID）
       ├─ PermissionCode: PermissionCode
       ├─ EffectiveDate: LocalDateTime
       └─ ExpirationDate: LocalDateTime?
```

### プロパティ仕様

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| **Id** | PermissionAssignmentId | NOT NULL, PK | 集約根ID（GUID採番） |
| **PermissionCode** | PermissionCode | NOT NULL | 権限コード（例："read", "write"） |
| **EffectiveDate** | LocalDateTime | NOT NULL | 有効開始日時（JST） |
| **ExpirationDate** | LocalDateTime? | NULL許可 | 有効終了日時（null=無期限） |

---

## 🏗️ API 仕様

### ファクトリメソッド

#### Create - 新規生成

```csharp
public static PermissionAssignment Create(
    PermissionCode permissionCode,
    LocalDateTime effectiveDate,
    LocalDateTime? expirationDate = null)
{
    // 新しい PermissionAssignmentId を自動採番
    // パラメータ検証は呼び出し元で実施
    // PermissionAssignment インスタンスを生成して返す
}
```

**パラメータ**:
- `permissionCode`: 権限コード（事前検証済み ValueObject）
- `effectiveDate`: 有効開始日時
- `expirationDate`: 有効終了日時（省略可）

**戻り値**: 生成された PermissionAssignment

**例外**: 
- パラメータが null の場合は ArgumentNullException
- 検証失敗時は ArgumentException

#### Reconstruct - DB復元

```csharp
public static PermissionAssignment Reconstruct(
    PermissionAssignmentId id,
    PermissionCode permissionCode,
    LocalDateTime effectiveDate,
    LocalDateTime? expirationDate = null)
{
    // DB から読み込んだ値で PermissionAssignment を復元
    // 値の再検証はしない（DB由来のため信頼できる）
}
```

**パラメータ**:
- `id`: DB から読み込んだ PermissionAssignmentId
- `permissionCode`: 権限コード
- `effectiveDate`: 有効開始日時
- `expirationDate`: 有効終了日時

**戻り値**: 復元された PermissionAssignment

---

### ビジネスロジック

#### IsActive - 有効期間チェック

```csharp
public bool IsActive(LocalDateTime asOf)
{
    // 戻り値: 指定時点で権限が有効な場合 true
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
var permissionAssignment = PermissionAssignment.Create(
    permissionCode,
    new LocalDateTime(new DateTime(2026, 1, 1)),
    new LocalDateTime(new DateTime(2026, 12, 31))
);

permissionAssignment.IsActive(new LocalDateTime(new DateTime(2026, 6, 15)))  // → true
permissionAssignment.IsActive(new LocalDateTime(new DateTime(2027, 1, 1)))   // → false
```

---

## 📋 使用例

### Employee に PermissionAssignment を追加

```csharp
public class Employee : Entity<EmployeeId>
{
    private List<PermissionAssignment> _permissionAssignments = new();
    
    public void AssignPermission(
        PermissionCode permissionCode,
        LocalDateTime effectiveDate,
        LocalDateTime? expirationDate = null)
    {
        var permissionAssignment = PermissionAssignment.Create(
            permissionCode,
            effectiveDate,
            expirationDate);
        _permissionAssignments.Add(permissionAssignment);
    }
}
```

### Repository での復元

```csharp
public async Task<Employee> GetByIdAsync(EmployeeId id)
{
    var dbModel = await _context.Employees.FindAsync(id);
    
    var permissionAssignments = dbModel.PermissionAssignments
        .Select(p => PermissionAssignment.Reconstruct(
            PermissionAssignmentId.From(p.Id),
            PermissionCode.From(p.Code),
            new LocalDateTime(p.EffectiveDate),
            p.ExpirationDate.HasValue 
                ? new LocalDateTime(p.ExpirationDate.Value)
                : null))
        .ToList();
    
    return Employee.Reconstruct(id, ..., permissionAssignments);
}
```

---

## ✅ 検証ルール

### PermissionCode 検証

- **必須**: NULL不可
- **形式**: 英数字アンダースコア（正規表現: `^[A-Za-z0-9_.]+$`）
- **長さ**: 1～100文字

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
// PermissionAssignment は Entity<TId> なので、Id で比較
var permA = PermissionAssignment.Create(permissionCode1, effectiveDate1);
var permB = PermissionAssignment.Create(permissionCode1, effectiveDate1);

permA == permB  // → false（異なる Id）
```

### ハッシュコード

```csharp
var perm = PermissionAssignment.Create(permissionCode, effectiveDate);
var dict = new Dictionary<PermissionAssignment, string>();
dict.Add(perm, "Permission1");

dict[perm]  // → "Permission1"（Id で識別）
```

---

## 🚫 使用時の注意点

### ❌ null チェック不要

```csharp
// ❌ 間違い
if (permissionAssignment != null) { ... }

// ✅ 正しい
// Entity は常に有効なインスタンス
```

### ⚠️ 有効期間の管理

```csharp
// DB では有効期間で管理
// Application / Presentation では IsActive() で判定
var now = _clock.JstNow;
var isActive = permissionAssignment.IsActive(now);
```

### 📝 ExpirationDate なし（無期限）

```csharp
// ExpirationDate が null の場合、EffectiveDate 以後は常に有効
var permissionAssignment = PermissionAssignment.Create(
    permissionCode,
    new LocalDateTime(new DateTime(2026, 1, 1))
    // expirationDate を省略
);

permissionAssignment.IsActive(new LocalDateTime(new DateTime(2099, 12, 31)))
// → true（無期限なので常に有効）
```

---

## 参考資料

- 詳細設計: `PermissionAssignment_詳細設計書.md`
- 単体テスト仕様: `PermissionAssignment_単体テスト仕様書.md`
- 関連Entity: `RoleAssignment_技術仕様書.md`
- ValueObject: `PermissionCode_技術仕様書.md`
