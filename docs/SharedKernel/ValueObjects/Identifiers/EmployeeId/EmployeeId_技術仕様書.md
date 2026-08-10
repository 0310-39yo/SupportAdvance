# EmployeeId - 技術仕様書

**対象者**: 開発者（EmployeeId を使用する実装者）

**版**: 1.0  
**作成日**: 2026-08-09

---

## 📖 概要

EmployeeId は、従業員を一意に識別するビジネスID を表現する ValueObject です。

- **何を実装するのか**: Employee Entity の集約根ID（GUID ベース）
- **どこに実装するのか**: `src/SharedKernel/ValueObjects/Identifiers/EmployeeId.cs`
- **誰が使うのか**: Employee Entity、Domain Events、Repository

---

## 🎯 基本仕様

### 構成

| 項目 | 内容 |
|------|------|
| **基底型** | `Guid` |
| **値の範囲** | 任意の Guid（有効な GUID のみ） |
| **生成方式** | システム採番（Guid.NewGuid）または既知のID |

### 特徴

- **GUID ベース**: ビジネスロジックから独立した一意のID
- **型安全**: EmployeeId として型システムで管理
- **グローバル一意**: 複数DBテーブル対応（複数テーブル集約）
- **不変性**: 生成後は値の変更なし

---

## 🏗️ API 仕様

### 生成メソッド

```csharp
// 新規生成（システム採番）
var employeeId = EmployeeId.NewId();

// 既知のID から生成
var employeeId = EmployeeId.From(new Guid("12345678-1234-1234-1234-123456789012"));

// 安全な生成
if (EmployeeId.TryFrom(guidValue, out var employeeId))
{
    // 生成成功
}

// DB値から生成
if (EmployeeId.TryFromDbValue(guidValue, out var employeeId))
{
    // 生成成功
}
```

### 値の取得

```csharp
var employeeId = EmployeeId.From(new Guid("12345678-1234-1234-1234-123456789012"));

employeeId.Value  // → Guid
```

---

## 📋 使用例

### Employee Entity での使用

```csharp
public class Employee : Entity<EmployeeId>
{
    public EmployeeRowId RowId { get; private set; }
    public EmployeeCode Code { get; private set; }
    public PersonRowId PersonRowId { get; private set; }
    
    public Employee(
        EmployeeId id,
        EmployeeRowId rowId,
        EmployeeCode code,
        PersonRowId personRowId)
    {
        Id = id;  // EmployeeId が Entity<EmployeeId>.Id
        RowId = rowId;
        Code = code;
        PersonRowId = personRowId;
    }
}
```

### Repository での使用

```csharp
// EmployeeId で従業員を検索
var employee = await _employeeRepository.FindByIdAsync(employeeId);

// Domain Event で使用
public class EmployeeCreatedEvent : IDomainEvent
{
    public AggregateRootId AggregateRootId => EmployeeId.Value;
    // ...
}
```

---

## ✅ 検証ルール

### 有効な値

```csharp
// ✅ 任意のGuid
EmployeeId.From(Guid.NewGuid())  // OK

// ✅ 既知のID
EmployeeId.From(new Guid("12345678-1234-1234-1234-123456789012"))  // OK
```

### 無効な値

```csharp
// ❌ Empty Guid（全ゼロ）は許可しない
EmployeeId.From(Guid.Empty)  // ArgumentOutOfRangeException
```

---

## 🔄 型安全性

### ValueObject としての等価性

```csharp
var id1 = EmployeeId.From(guidValue);
var id2 = EmployeeId.From(guidValue);

id1 == id2  // → true（値が同じなら等価）
id1.Equals(id2)  // → true
```

### DB保存・復元

```csharp
// Entity → DbModel（保存時）
var dbModel = new EmployeeDbModel
{
    EmployeeId = employeeId.Value  // Guid
};

// DbModel → Entity（復元時）
if (EmployeeId.TryFromDbValue(dbModel.EmployeeId, out var restored))
{
    var employee = new Employee(restored, ...);
}
```

---

## 🚫 使用時の注意点

### ❌ null チェック不要（Domain層では null 禁止）

```csharp
// ❌ 間違い
if (employeeId != null) { ... }

// ✅ 正しい
// ValueObject は常に有効な値を保持
```

### ⚠️ Guid.Empty は禁止

EmployeeId は Guid.Empty を許可しません。常に有効なGuidを保持します。

```csharp
// ❌ 無効
EmployeeId.From(Guid.Empty)  // ArgumentOutOfRangeException

// ✅ 正しい
EmployeeId.From(Guid.NewGuid())  // OK
EmployeeId.NewId()  // OK（新規採番）
```

---

## 参考資料

- 詳細設計: `EmployeeId_詳細設計書.md`
- 単体テスト仕様: `EmployeeId_単体テスト仕様書.md`
- アーキテクチャガイド: `../../../Guides/AggregateId_設計ガイド.md`
