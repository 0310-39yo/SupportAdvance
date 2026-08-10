# RoleAssignment - 概要書

**対象者**: 設計者、開発者、テスト実装者

**版**: 1.0  
**作成日**: 2026-08-10

---

## 📖 概要

RoleAssignment は、従業員に割り当てられたロール（役割）とその有効期間を管理する Child Entity です。

Employee 集約に属し、従業員が複数のロールを保持できます。

---

## 🎯 責務

| 責務 | 説明 |
|------|------|
| **ロール割り当ての管理** | 従業員のロールと有効期間を管理 |
| **有効期間チェック** | 指定時点でロールが有効かを判定 |
| **不変性の確保** | 生成後は値の変更なし |
| **型安全性** | RoleCode で型安全にロール識別 |

---

## 🏗️ 構成

### 集約根ID

- **型**: RoleAssignmentId（GUID ベース）
- **責務**: Child Entity の一意識別

### 公開プロパティ

| プロパティ | 型 | 説明 | 必須 |
|-----------|-----|------|------|
| **Id** | RoleAssignmentId | ロール割り当てID（集約根ID） | ✅ |
| **RoleCode** | RoleCode | ロールコード（例："Admin", "Manager"） | ✅ |
| **EffectiveDate** | LocalDateTime | 有効開始日時 | ✅ |
| **ExpirationDate** | LocalDateTime? | 有効終了日時（null=無期限） | ❌ |

### ビジネスロジック

```csharp
// 指定時点でロールが有効かを判定
public bool IsActive(LocalDateTime asOf)
{
    // EffectiveDate が asOf より前、かつ
    // ExpirationDate がない、または asOf が ExpirationDate より前なら有効
}
```

---

## 📋 ライフサイクル

### 生成（Create）

```
新規RoleAssignment生成 → ID自動採番（GUID） → IsActive = true
```

### 復元（Reconstruct）

```
DBから読み込み → 値検証 → Entity復元 → IsActive 判定可能
```

---

## 🔄 使用例

### Employee Entity での使用

```csharp
public class Employee : Entity<EmployeeId>
{
    private List<RoleAssignment> _roleAssignments = new();
    
    public IReadOnlyList<RoleAssignment> RoleAssignments 
        => _roleAssignments.AsReadOnly();
    
    public void AssignRole(RoleCode roleCode, LocalDateTime effectiveDate, LocalDateTime? expirationDate = null)
    {
        var roleAssignment = RoleAssignment.Create(roleCode, effectiveDate, expirationDate);
        _roleAssignments.Add(roleAssignment);
    }
}
```

### Repository での使用

```csharp
// 従業員の現在有効なロール一覧を取得
var now = _clock.JstNow;
var activeRoles = employee.RoleAssignments
    .Where(r => r.IsActive(now))
    .Select(r => r.RoleCode)
    .ToList();
```

---

## ✅ テスト観点

### VO-01: 生成メソッド（Create）

- 正常系: 新しいRoleAssignmentが生成される
- 異常系: 無効な RoleCode でエラー

### VO-02: 復元メソッド（Reconstruct）

- 正常系: DBから読み込んだ値で復元される

### BL-01: IsActive ビジネスロジック

- EffectiveDate 前: 無効
- EffectiveDate ～ ExpirationDate: 有効
- ExpirationDate 後: 無効
- ExpirationDate なし: 常に有効（EffectiveDate 以後）

### EQ-01: 等価性

- 同じ ID なら等価
- 異なる ID なら非等価

---

## 📁 ドキュメント体系

1. **RoleAssignment_概要.md** ← 現在
2. **RoleAssignment_技術仕様書.md** - API仕様、検証ルール
3. **RoleAssignment_詳細設計書.md** - クラス設計、実装詳細
4. **RoleAssignment_単体テスト仕様書.md** - テストケース、検証方法

---

## 関連ドキュメント

- DepartmentMembership_概要.md（同じパターンのChild Entity）
- Employee_技術仕様書.md（親Entity）
- RoleCode_技術仕様書.md（ValueObject）
