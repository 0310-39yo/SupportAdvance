# PermissionAssignment - 概要書

**対象者**: 設計者、開発者、テスト実装者

**版**: 1.0  
**作成日**: 2026-08-10

---

## 📖 概要

PermissionAssignment は、Employee 集約に属する Child Entity です。従業員に割り当てられた権限（パーミッション）とその有効期間を管理します。

Employee 集約に属し、従業員が複数の権限を保持できます。

---

## 🎯 責務

| 責務 | 説明 |
|------|------|
| **権限割り当ての管理** | 従業員の権限と有効期間を管理 |
| **有効期間チェック** | 指定時点で権限が有効かを判定 |
| **不変性の確保** | 生成後は値の変更なし |
| **型安全性** | PermissionCode で型安全に権限識別 |

---

## 🏗️ 構成

### 集約根ID

- **型**: PermissionAssignmentId（GUID ベース）
- **責務**: Child Entity の一意識別

### 公開プロパティ

| プロパティ | 型 | 説明 | 必須 |
|-----------|-----|------|------|
| **Id** | PermissionAssignmentId | 権限割り当てID（集約根ID） | ✅ |
| **PermissionCode** | PermissionCode | 権限コード（例："read", "write"） | ✅ |
| **EffectiveDate** | LocalDateTime | 有効開始日時 | ✅ |
| **ExpirationDate** | LocalDateTime? | 有効終了日時（null=無期限） | ❌ |

### ビジネスロジック

```csharp
// 指定時点で権限が有効かを判定
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
新規PermissionAssignment生成 → ID自動採番（GUID） → IsActive = true
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
    private List<PermissionAssignment> _permissionAssignments = new();
    
    public IReadOnlyList<PermissionAssignment> PermissionAssignments 
        => _permissionAssignments.AsReadOnly();
    
    public void AssignPermission(PermissionCode permissionCode, LocalDateTime effectiveDate, LocalDateTime? expirationDate = null)
    {
        var permissionAssignment = PermissionAssignment.Create(permissionCode, effectiveDate, expirationDate);
        _permissionAssignments.Add(permissionAssignment);
    }
}
```

### Repository での使用

```csharp
// 従業員の現在有効な権限一覧を取得
var now = _clock.JstNow;
var activePermissions = employee.PermissionAssignments
    .Where(p => p.IsActive(now))
    .Select(p => p.PermissionCode)
    .ToList();
```

---

## ✅ テスト観点

### VO-01: 生成メソッド（Create）

- 正常系: 新しい PermissionAssignment が生成される
- 異常系: 無効な PermissionCode でエラー

### VO-02: 復元メソッド（Reconstruct）

- 正常系: DB から読み込んだ値で復元される

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

1. **PermissionAssignment_概要.md** ← 現在
2. **PermissionAssignment_技術仕様書.md** - API仕様、検証ルール
3. **PermissionAssignment_詳細設計書.md** - クラス設計、実装詳細
4. **PermissionAssignment_単体テスト仕様書.md** - テストケース、検証方法

---

## 関連ドキュメント

- RoleAssignment_概要.md（同じパターンのChild Entity）
- Employee_技術仕様書.md（親Entity）
- PermissionCode_技術仕様書.md（ValueObject）
