# EmployeeRowId - 技術仕様書

**対象者**: 開発者（EmployeeRowId を使用する実装者）

**版**: 1.0  
**作成日**: 2026-08-09

---

## 📖 概要

EmployeeRowId は、データベース上の従業員レコードの行ID（rowId）を表現する ValueObject です。

- **何を実装するのか**: DB の `t_employees.row_id` を型安全に管理
- **どこに実装するのか**: `src/SharedKernel/ValueObjects/Identifiers/EmployeeRowId.cs`
- **誰が使うのか**: Employee Entity の構成要素、Repository での照会キー

---

## 🎯 基本仕様

### 構成

| 項目 | 内容 |
|------|------|
| **基底型** | `long` |
| **値の範囲** | 1 以上（`long.MaxValue` 以下） |

### 特徴

- **システム採番**: DB の `s_row_id_sequence` で自動生成
- **一意性**: データベース内で一意（PK）
- **型安全**: `long` をラップして EmployeeRowId 型として管理
- **不変性**: 生成後は値の変更なし

---

## 🏗️ API 仕様

### 生成メソッド

```csharp
// 新規生成（DB採番）
var rowId = EmployeeRowId.From(12345L);

// 安全な生成
if (EmployeeRowId.TryFrom(12345L, out var rowId))
{
    // 生成成功
}

// DB値から生成
if (EmployeeRowId.TryFromDbValue(12345L, out var rowId))
{
    // 生成成功
}
```

### 値の取得

```csharp
var rowId = EmployeeRowId.From(12345L);

rowId.Value  // → 12345L
```

---

## 📋 使用例

### Employee Entity での使用

```csharp
public class Employee : Entity<EmployeeId>
{
    public EmployeeRowId RowId { get; private set; }  // DB行ID
    public EmployeeCode Code { get; private set; }
    public PersonRowId PersonRowId { get; private set; }
    
    public Employee(
        EmployeeId id,
        EmployeeRowId rowId,
        EmployeeCode code,
        PersonRowId personRowId)
    {
        Id = id;
        RowId = rowId;
        Code = code;
        PersonRowId = personRowId;
    }
}
```

### Repository での使用

```csharp
// RowId で従業員を検索
var employee = await _employeeRepository.FindByRowIdAsync(rowId);

// RowId を条件に削除
await _employeeRepository.DeleteByRowIdAsync(rowId);
```

---

## ✅ 検証ルール

### 有効な値

```csharp
// ✅ 正の値
EmployeeRowId.From(1L)           // OK
EmployeeRowId.From(12345L)       // OK
EmployeeRowId.From(9223372036854775807L)  // OK（long.MaxValue）
```

### 無効な値

```csharp
// ❌ 0 以下
EmployeeRowId.From(0L)       // ArgumentOutOfRangeException
EmployeeRowId.From(-1L)      // ArgumentOutOfRangeException
```

---

## 🔄 型安全性

### ValueObject としての等価性

```csharp
var rowId1 = EmployeeRowId.From(12345L);
var rowId2 = EmployeeRowId.From(12345L);

rowId1 == rowId2  // → true（値が同じなら等価）
rowId1.Equals(rowId2)  // → true
```

### DB保存・復元

```csharp
// Entity → DbModel（保存時）
var dbModel = new EmployeeDbModel
{
    RowId = rowId.Value  // 12345L
};

// DbModel → Entity（復元時）
if (EmployeeRowId.TryFromDbValue(dbModel.RowId, out var restored))
{
    var employee = new Employee(id, restored, ...);
}
```

---

## 🚫 使用時の注意点

### ❌ null チェック不要（Domain層では null 禁止）

```csharp
// ❌ 間違い
if (rowId != null) { ... }

// ✅ 正しい
// ValueObject は常に有効な値を保持
```

### ⚠️ DB値との対応

EmployeeRowId の値は DB の `t_employees.row_id` カラムの値と一致する必要があります。

```csharp
// DB: t_employees.row_id = 12345
var rowId = EmployeeRowId.From(12345L);  // 一致
```

---

## 参考資料

- 詳細設計: `EmployeeRowId_詳細設計書.md`
- 単体テスト仕様: `EmployeeRowId_単体テスト仕様書.md`
- アーキテクチャガイド: `../../../Guides/AggregateId_設計ガイド.md`
