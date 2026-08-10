# EmployeeCode - 技術仕様書

**対象者**: 開発者（EmployeeCode を使用する実装者）

**版**: 1.0  
**作成日**: 2026-08-09

---

## 📖 概要

EmployeeCode は、従業員の区分と番号を統合した従業員コードを表現する ValueObject です。

- **何を実装するのか**: 従業員区分＋従業員番号の複合値管理
- **どこに実装するのか**: `src/SharedKernel/ValueObjects/Identifiers/EmployeeCode.cs`
- **誰が使うのか**: Employee Entity、ログイン認証、給与管理など

---

## 🎯 基本仕様

### 構成

| 項目 | 内容 |
|------|------|
| **Division** | EmployeeDivision（M/T/C） |
| **Number** | EmployeeNumber（1001～8499） |

### 表示形式

| 区分 | 番号 | 表示例 |
|------|------|--------|
| M | 1234 | `M1234`（4桁） |
| M | 12345 | `M12345`（5桁） |
| T | 7500 | `T7500` |
| C | 8000 | `C8000` |

**特徴**: 区分＋番号、スペースなし、左0埋めなし

### 有効範囲（区分ごと）

| 区分 | 初期範囲 | 使い切り後 |
|------|---------|----------|
| M（従業員） | 1001～6999 | 10000～ |
| T（派遣社員） | 7500～7999 | 70000～ |
| C（請負者） | 8000～8499 | 80000～ |

---

## 🏗️ API 仕様

### 生成メソッド

```csharp
// ValueObject から生成
var code = EmployeeCode.From(
    EmployeeDivision.RegularEmployee(),
    EmployeeNumber.From(1234));

// 安全な生成
if (EmployeeCode.TryFrom(division, number, out var code))
{
    // 生成成功
}

// 文字列から生成（ログイン認証で使用）
if (EmployeeCode.TryParse("M1234", out var code))
{
    // 生成成功
}

// DB値から生成
if (EmployeeCode.TryFromDbValues(divisionChar, numberInt, out var code))
{
    // 生成成功
}
```

### 値の取得

```csharp
var code = EmployeeCode.From(
    EmployeeDivision.RegularEmployee(),
    EmployeeNumber.From(1234));

code.Division          // → EmployeeDivision.RegularEmployee()
code.Number            // → EmployeeNumber.From(1234)
code.ToString()        // → "M1234"
```

---

## 📋 使用例

### ログイン認証

```csharp
// ログイン画面で「M1234」を入力
if (EmployeeCode.TryParse(userInput, out var code))
{
    var employee = await _employeeRepository.FindByCodeAsync(code);
    // ログイン処理
}
```

### Employee Entity での使用

```csharp
public class Employee : Entity<EmployeeId>
{
    public EmployeeCode Code { get; private set; }  // 従業員コード
    public PersonRowId PersonRowId { get; private set; }
    
    public Employee(
        EmployeeId id,
        EmployeeCode code,
        PersonRowId personRowId)
    {
        Id = id;
        Code = code;  // 範囲検証済み
        PersonRowId = personRowId;
    }
}
```

### 表示

```csharp
var code = EmployeeCode.From(
    EmployeeDivision.RegularEmployee(),
    EmployeeNumber.From(1234));

Console.WriteLine(code);  // → "M1234"
```

---

## ✅ 検証ルール

### 有効な組み合わせ

```csharp
// ✅ 従業員（M）: 1001～6999
EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234))  // OK

// ✅ 従業員（M）: 10000～
EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(10000))  // OK（範囲内）

// ✅ 派遣社員（T）: 7500～7999
EmployeeCode.From(EmployeeDivision.Dispatched(), EmployeeNumber.From(7500))  // OK

// ✅ 派遣社員（T）: 70000～
EmployeeCode.From(EmployeeDivision.Dispatched(), EmployeeNumber.From(70000))  // OK（範囲内）

// ✅ 請負者（C）: 8000～8499
EmployeeCode.From(EmployeeDivision.Contractor(), EmployeeNumber.From(8000))  // OK

// ✅ 請負者（C）: 80000～
EmployeeCode.From(EmployeeDivision.Contractor(), EmployeeNumber.From(80000))  // OK（範囲内）
```

### 無効な組み合わせ

```csharp
// ❌ 従業員（M）に派遣範囲（7500）を割り当て
EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(7500))
// ArgumentException: "従業員の番号範囲が無効"

// ❌ 派遣社員（T）に従業員範囲（1234）を割り当て
EmployeeCode.From(EmployeeDivision.Dispatched(), EmployeeNumber.From(1234))
// ArgumentException: "派遣社員の番号範囲が無効"

// ❌ 無効な文字列形式
EmployeeCode.TryParse("X1234", out _)  // false（区分X は無効）
EmployeeCode.TryParse("M999", out _)   // false（範囲外）
```

---

## 🔄 型安全性

### ValueObject としての等価性

```csharp
var code1 = EmployeeCode.From(
    EmployeeDivision.RegularEmployee(),
    EmployeeNumber.From(1234));
var code2 = EmployeeCode.From(
    EmployeeDivision.RegularEmployee(),
    EmployeeNumber.From(1234));

code1 == code2  // → true（値が同じなら等価）
code1.Equals(code2)  // → true
```

### DB保存・復元

```csharp
// Entity → DbModel（保存時）
var dbModel = new EmployeeDbModel
{
    EmployeeDivision = code.Division.Value,    // 'M'
    EmployeeNumber = code.Number.Value         // 1234
};

// DbModel → Entity（復元時）
if (EmployeeCode.TryFromDbValues(
    dbModel.EmployeeDivision,
    dbModel.EmployeeNumber,
    out var restored))
{
    var employee = new Employee(id, restored, ...);
}
```

---

## 🚫 使用時の注意点

### ❌ null チェック不要（Domain層では null 禁止）

```csharp
// ❌ 間違い
if (code != null) { ... }

// ✅ 正しい
// ValueObject は常に有効な値を保持
```

### ⚠️ 範囲検証は EmployeeCode レベルで完結

EmployeeCode の生成時に、区分と番号の組み合わせが有効か検証される。
Entity レベルの追加検証は不要。

```csharp
// EmployeeCode で検証済み
var code = EmployeeCode.From(division, number);  // 無効なら例外

// Entity では信頼して使用
var employee = new Employee(id, code, personRowId);  // 追加検証なし
```

---

## 参考資料

- 詳細設計: `EmployeeCode_詳細設計書.md`
- 単体テスト仕様: `EmployeeCode_単体テスト仕様書.md`
- EmployeeDivision: `../EmployeeDivision/EmployeeDivision_技術仕様書.md`
- EmployeeNumber: `../EmployeeNumber/EmployeeNumber_技術仕様書.md`
