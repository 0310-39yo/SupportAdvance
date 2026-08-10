# PersonRowId - 技術仕様書

**対象者**: 開発者（PersonRowId を使用する実装者）

**版**: 1.0  
**作成日**: 2026-08-09

---

## 📖 概要

PersonRowId は、データベース上の人物レコードの行ID（rowId）を表現する ValueObject です。

- **何を実装するのか**: DB の `m_persons.row_id` を型安全に管理
- **どこに実装するのか**: `src/SharedKernel/ValueObjects/Identifiers/PersonRowId.cs`
- **誰が使うのか**: Employee Entity の構成要素、給与マスタなど

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
- **型安全**: `long` をラップして PersonRowId 型として管理
- **不変性**: 生成後は値の変更なし

---

## 🏗️ API 仕様

### 生成メソッド

```csharp
// 新規生成（DB採番）
var rowId = PersonRowId.From(12345L);

// 安全な生成
if (PersonRowId.TryFrom(12345L, out var rowId))
{
    // 生成成功
}

// DB値から生成
if (PersonRowId.TryFromDbValue(12345L, out var rowId))
{
    // 生成成功
}
```

### 値の取得

```csharp
var rowId = PersonRowId.From(12345L);

rowId.Value  // → 12345L
```

---

## 📋 使用例

### Employee Entity での使用

```csharp
public class Employee : Entity<EmployeeId>
{
    public EmployeeRowId RowId { get; private set; }
    public EmployeeCode Code { get; private set; }
    public PersonRowId PersonRowId { get; private set; }  // 人物レコードへの参照
    
    public Employee(
        EmployeeId id,
        EmployeeRowId rowId,
        EmployeeCode code,
        PersonRowId personRowId)
    {
        Id = id;
        RowId = rowId;
        Code = code;
        PersonRowId = personRowId;  // m_persons への外部キー
    }
}
```

### Repository での使用

```csharp
// PersonRowId で従業員の給与情報を検索
var salary = await _salaryRepository.FindByPersonRowIdAsync(personRowId);
```

---

## ✅ 検証ルール

### 有効な値

```csharp
// ✅ 正の値
PersonRowId.From(1L)           // OK
PersonRowId.From(12345L)       // OK
PersonRowId.From(9223372036854775807L)  // OK（long.MaxValue）
```

### 無効な値

```csharp
// ❌ 0 以下
PersonRowId.From(0L)       // ArgumentOutOfRangeException
PersonRowId.From(-1L)      // ArgumentOutOfRangeException
```

---

## 🔄 型安全性

### ValueObject としての等価性

```csharp
var rowId1 = PersonRowId.From(12345L);
var rowId2 = PersonRowId.From(12345L);

rowId1 == rowId2  // → true（値が同じなら等価）
rowId1.Equals(rowId2)  // → true
```

### DB保存・復元

```csharp
// Entity → DbModel（保存時）
var dbModel = new EmployeeDbModel
{
    PersonRowId = rowId.Value  // 12345L
};

// DbModel → Entity（復元時）
if (PersonRowId.TryFromDbValue(dbModel.PersonRowId, out var restored))
{
    var employee = new Employee(id, ..., restored);
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

PersonRowId の値は DB の `m_persons.row_id` カラムの値と一致する必要があります。

```csharp
// DB: m_persons.row_id = 12345
var rowId = PersonRowId.From(12345L);  // 一致
```

---

## 参考資料

- 詳細設計: `PersonRowId_詳細設計書.md`
- 単体テスト仕様: `PersonRowId_単体テスト仕様書.md`
- 関連: `EmployeeRowId_技術仕様書.md`
