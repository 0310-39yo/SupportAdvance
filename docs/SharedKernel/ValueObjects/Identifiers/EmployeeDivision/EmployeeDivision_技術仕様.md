# EmployeeDivision - 技術仕様書

**対象者**: 開発者（EmployeeDivision を使用する実装者）

**版**: 1.0  
**作成日**: 2026-08-09

---

## 📖 概要

EmployeeDivision は、従業員の区分（従業員、派遣社員、請負者）を表現する ValueObject です。

- **何を実装するのか**: 固定値型 ValueObject（区分の管理と検証）
- **どこに実装するのか**: `src/SharedKernel/ValueObjects/Identifiers/EmployeeDivision.cs`
- **誰が使うのか**: Employee Entity、ログイン認証機能、給与・勤務管理など

---

## 🎯 基本仕様

### 区分値の定義

| 値 | 定数名 | 日本語名 | 説明 |
|----|------|--------|------|
| `M` | `RegularEmployeeValue` | 従業員 | 直雇用の正規従業員 |
| `T` | `DispatchedValue` | 派遣社員 | 派遣会社経由の雇用 |
| `C` | `ContractorValue` | 請負者 | 業務委託契約者 |

---

## 🏗️ API 仕様

### ファクトリメソッド（推奨）

```csharp
// 従業員を生成
var division = EmployeeDivision.RegularEmployee();

// 派遣社員を生成
var division = EmployeeDivision.Dispatched();

// 請負者を生成
var division = EmployeeDivision.Contractor();
```

### 生成メソッド

```csharp
// char 値から生成（検証あり）
var division = EmployeeDivision.From('M');

// char? 値から安全に生成（null → false）
if (EmployeeDivision.TryFrom(charValue, out var division))
{
    // 生成成功
}

// DB値（string）から生成（最初の文字を使用）
var division = EmployeeDivision.FromDbValue("M");

// DB値から安全に生成（null → false）
if (EmployeeDivision.TryFromDbValue(dbValue, out var division))
{
    // 生成成功
}
```

### 判定メソッド

```csharp
var division = EmployeeDivision.RegularEmployee();

division.IsRegularEmployee   // → true
division.IsDispatched        // → false
division.IsContractor        // → false
```

### 値の取得

```csharp
var division = EmployeeDivision.RegularEmployee();

division.Value               // → 'M'
division.ToString()          // → "従業員"（日本語名）
```

---

## 📋 使用例

### Employee Entity での使用

```csharp
public class Employee : Entity<EmployeeId>
{
    public EmployeeDivision Division { get; private set; }
    
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
    }
    
    // ビジネスロジック例：権限判定
    public bool CanApproveLeaveRequest()
    {
        // 派遣社員は申請不可
        return !Division.IsDispatched;
    }
}
```

### ログイン認証での使用

```csharp
// ログインID（従業員区分+従業員No）から従業員を検索
if (EmployeeDivision.TryFromDbValue(divisionChar, out var division))
{
    var employee = await _employeeRepository.FindByDivisionAndNumber(
        division,
        employeeNumber);
}
```

---

## ✅ 検証ルール

### 有効な区分値

- ✅ 'M', 'T', 'C' のいずれか
- ❌ その他の char 値（例外 `ArgumentOutOfRangeException`）
- ❌ null（TryFrom で false を返す）

### 例外ハンドリング

```csharp
// ❌ 無効な値で例外発生
try
{
    var division = EmployeeDivision.From('X');  // ArgumentOutOfRangeException
}
catch (ArgumentOutOfRangeException ex)
{
    // エラー処理
}

// ✅ TryFrom で安全に処理
if (!EmployeeDivision.TryFrom(userInput, out var division))
{
    // 無効な入力として処理
}
```

---

## 🔄 型安全性

### ValueObject としての等価性

```csharp
var div1 = EmployeeDivision.RegularEmployee();
var div2 = EmployeeDivision.From('M');

div1 == div2  // → true（値が同じなら等価）
div1.Equals(div2)  // → true

var div3 = EmployeeDivision.Dispatched();
div1 == div3  // → false（値が異なる）
```

### DB保存・復元

```csharp
// Entity → DbModel（保存時）
var dbModel = new EmployeeDbModel
{
    Division = division.Value  // → 'M'
};

// DbModel → Entity（復元時）
if (EmployeeDivision.TryFromDbValue(dbModel.Division, out var restored))
{
    var employee = new Employee(id, restored, ...);
}
```

---

## 🚫 使用時の注意点

### ❌ null チェック不要（Domain層では null 禁止）

```csharp
// ❌ 間違い
if (division != null) { ... }

// ✅ 正しい
if (division.IsRegularEmployee) { ... }
```

### ❌ ToString() の値を DB に保存しない

```csharp
// ❌ 間違い
dbModel.DivisionName = division.ToString();  // "従業員" を保存

// ✅ 正しい
dbModel.Division = division.Value;  // 'M' を保存
```

### ❌ 新しい区分値の追加は制限

現在対応している区分は M, T, C のみ。新規区分追加時は以下を実施：
1. DB スキーマを確認
2. EmployeeDivision.Validate() を更新
3. GetDisplayName() に日本語名を追加
4. 定数を追加
5. テストを拡張

---

## 参考資料

- 詳細設計: `EmployeeDivision_詳細設計.md`
- 単体テスト仕様: `EmployeeDivision_単体テスト仕様.md`
- EnumValueObject 基底クラス: `src/SharedKernel/ValueObjects/Abstractions/EnumValueObject.cs`
