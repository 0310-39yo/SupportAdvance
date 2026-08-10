# EmployeeNumber - 技術仕様書

**対象者**: 開発者（EmployeeNumber を使用する実装者）

**版**: 1.0  
**作成日**: 2026-08-09

---

## 📖 概要

EmployeeNumber は、従業員の通し番号を表現する ValueObject です。

- **何を実装するのか**: 従業員番号の管理と検証
- **どこに実装するのか**: `src/SharedKernel/ValueObjects/Identifiers/EmployeeNumber.cs`
- **誰が使うのか**: Employee Entity、給与管理、勤務管理など

---

## 🎯 基本仕様

### 番号の有効範囲

| 項目 | 内容 |
|------|------|
| **有効範囲** | 1001～8499 |
| **予約番号** | 1000（システム管理者用） |
| **範囲外の割り当て** | Entity レベルで実装（VオブジェクトでなくEntity責務） |

### 表示形式

| 項目 | 内容 |
|------|------|
| **Value プロパティ** | int（例：1234） |
| **ToString()** | 左0埋めで5桁（例："01234"） |
| **DB保存** | int をそのまま保存 |

---

## 🏗️ API 仕様

### 生成メソッド

```csharp
// int 値から生成（検証あり）
var number = EmployeeNumber.From(1234);

// int? 値から安全に生成（null → false）
if (EmployeeNumber.TryFrom(intValue, out var number))
{
    // 生成成功
}

// DB値（int）から生成
var number = EmployeeNumber.FromDbValue(1234);

// DB値から安全に生成（null → false）
if (EmployeeNumber.TryFromDbValue(dbValue, out var number))
{
    // 生成成功
}
```

### 値の取得

```csharp
var number = EmployeeNumber.From(1234);

number.Value               // → 1234
number.ToString()          // → "01234"（5桁左0埋め）
```

---

## 📋 使用例

### Employee Entity での使用

```csharp
public class Employee : Entity<EmployeeId>
{
    public EmployeeDivision Division { get; private set; }
    public EmployeeNumber Number { get; private set; }
    
    // コンストラクタ
    public Employee(
        EmployeeId id,
        EmployeeDivision division,
        EmployeeNumber number,
        PersonRowId personRowId)
    {
        Id = id;
        Division = division;
        Number = number;
        PersonRowId = personRowId;
        
        // 範囲検証は Entity レベルで実施
        ValidateNumberRange(division, number);
    }
    
    // 範囲検証（Entity責務）
    private void ValidateNumberRange(EmployeeDivision division, EmployeeNumber number)
    {
        // 従業員（M）: 1001～6999, 10000～
        // 派遣社員（T）: 7500～7999, 70000～
        // 請負者（C）: 8000～8499, 80000～
        // この検証ロジックは Entity で実装
    }
}
```

### ログイン認証での使用

```csharp
// 従業員ID（従業員区分+従業員No）から従業員を検索
if (EmployeeNumber.TryFromDbValue(employeeNumberDb, out var number))
{
    var employee = await _employeeRepository.FindByDivisionAndNumber(
        division,
        number);
}
```

---

## ✅ 検証ルール

### 有効な番号値

- ✅ 1001～8499 のいずれか
- ❌ 1000（システム管理者予約） → 例外 `ArgumentOutOfRangeException`
- ❌ 0 以下、9000 以上などの範囲外 → 例外
- ❌ null（TryFrom で false を返す）

### 例外ハンドリング

```csharp
// ❌ 無効な値で例外発生
try
{
    var number = EmployeeNumber.From(1000);  // ArgumentOutOfRangeException
}
catch (ArgumentOutOfRangeException ex)
{
    // エラー処理
}

// ✅ TryFrom で安全に処理
if (!EmployeeNumber.TryFrom(userInput, out var number))
{
    // 無効な入力として処理
}
```

---

## 🔄 型安全性

### ValueObject としての等価性

```csharp
var num1 = EmployeeNumber.From(1234);
var num2 = EmployeeNumber.From(1234);

num1 == num2  // → true（値が同じなら等価）
num1.Equals(num2)  // → true

var num3 = EmployeeNumber.From(5678);
num1 == num3  // → false（値が異なる）
```

### DB保存・復元

```csharp
// Entity → DbModel（保存時）
var dbModel = new EmployeeDbModel
{
    EmployeeNumber = number.Value  // → 1234（int）
};

// DbModel → Entity（復元時）
if (EmployeeNumber.TryFromDbValue(dbModel.EmployeeNumber, out var restored))
{
    var employee = new Employee(id, restored, ...);
}
```

---

## 🚫 使用時の注意点

### ❌ null チェック不要（Domain層では null 禁止）

```csharp
// ❌ 間違い
if (number != null) { ... }

// ✅ 正しい
// ValueObject は常に有効な値を保持
```

### ❌ ToString() の値を計算ロジックに使用しない

```csharp
// ❌ 間違い
var display = number.ToString();  // "01234"
int calc = int.Parse(display);    // 1234 に戻す手間

// ✅ 正しい
int value = number.Value;  // 直接 int を取得
```

### ⚠️ 範囲チェックは Entity レベルで実施

ValueObject は 1001～8499 の基本検証のみ。
従業員区分ごとの詳細範囲チェック（例：派遣は 7500～7999 または 70000～）は Employee Entity で実装。

```csharp
// Entity での実装例
private void ValidateNumberRange(EmployeeDivision division, EmployeeNumber number)
{
    if (division.IsRegularEmployee)
    {
        // 従業員: 1001～6999, 10000～（Entity責務）
        if (!IsValidRegularEmployeeNumber(number.Value))
            throw new DomainException("...");
    }
    // ...
}
```

---

## 参考資料

- 詳細設計: `EmployeeNumber_詳細設計書.md`
- 単体テスト仕様: `EmployeeNumber_単体テスト仕様書.md`
- PrimitiveValueObject 基底クラス: `src/SharedKernel/ValueObjects/Abstractions/PrimitiveValueObject.cs`
