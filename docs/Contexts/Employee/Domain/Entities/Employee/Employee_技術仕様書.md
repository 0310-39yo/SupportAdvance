# Employee - 技術仕様書

**対象者**: 開発者（Employee Entity を使用する実装者）

**版**: 1.0  
**作成日**: 2026-08-09

---

## 📖 概要

Employee は、従業員エンティティを表現する Domain Entity です。

- **何を実装するのか**: 従業員ドメインモデル
- **どこに実装するのか**: `src/Contexts/Samples/CarPreferences/Domain/Entities/Employee.cs`
- **誰が使うのか**: 従業員管理 Use Cases、Repository

---

## 🎯 基本仕様

### 構成

| 項目 | 型 | 説明 |
|------|-----|------|
| **Id** | EmployeeId | 集約根ID（ビジネスID、GUID） |
| **RowId** | EmployeeRowId | DB行ID（t_employees.row_id） |
| **Code** | EmployeeCode | 従業員コード（区分+番号） |
| **PersonRowId** | PersonRowId | 人物マスタへの外部キー（m_persons.row_id） |

### 責務

- **従業員情報の保持**: ID、コード、人物レコードへの参照を管理
- **型安全な集約ID の提供**: Entity<EmployeeId> として型システムで管理
- **不変性の確保**: 生成後の値は読み取り専用

### 集約構造

```
Employee（集約根）
  ├─ EmployeeId（集約根ID）
  ├─ EmployeeRowId（DB行ID）
  ├─ EmployeeCode
  │   ├─ EmployeeDivision（従業員区分）
  │   └─ EmployeeNumber（従業員番号）
  └─ PersonRowId（外部キー参照）
```

---

## 🏗️ API 仕様

### 生成メソッド

```csharp
// Entity 生成
var employee = new Employee(
    id: EmployeeId.NewId(),
    rowId: EmployeeRowId.From(12345L),
    code: EmployeeCode.From(
        EmployeeDivision.RegularEmployee(),
        EmployeeNumber.From(1234)),
    personRowId: PersonRowId.From(67890L));

// 安全な生成（TryFrom は実装しない。Domain層では生成失敗は例外とする）
```

### プロパティアクセス

```csharp
var employee = new Employee(...);

employee.Id           // → EmployeeId
employee.RowId        // → EmployeeRowId
employee.Code         // → EmployeeCode
employee.PersonRowId  // → PersonRowId
```

---

## 📋 使用例

### Application層での使用

```csharp
public class CreateEmployeeUseCase
{
    public async Task Execute(CreateEmployeeDto request)
    {
        // 従業員コードの構築
        var code = EmployeeCode.From(
            EmployeeDivision.From(request.DivisionChar),
            EmployeeNumber.From(request.Number));

        // Employee の生成
        var employee = new Employee(
            id: EmployeeId.NewId(),
            rowId: EmployeeRowId.From(rowIdFromDb),
            code: code,
            personRowId: PersonRowId.From(request.PersonRowId));

        // Repository に保存
        await _employeeRepository.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### Repository での使用

```csharp
// 従業員コードで検索
var employee = await _employeeRepository.FindByCodeAsync(code);

// EmployeeId で検索
var employee = await _employeeRepository.FindByIdAsync(employeeId);

// RowId で検索
var employee = await _employeeRepository.FindByRowIdAsync(rowId);
```

---

## ✅ 検証ルール

### 生成時の検証

```csharp
// ✅ 有効な組み合わせ
var employee = new Employee(
    EmployeeId.NewId(),
    EmployeeRowId.From(1L),
    EmployeeCode.From(
        EmployeeDivision.RegularEmployee(),
        EmployeeNumber.From(1234)),
    PersonRowId.From(1L));

// ❌ 無効な値は ValueObject 生成時に検証
// EmployeeCode.From で範囲検証が行われるため、
// Entity での再検証は不要
```

### 不変性

```csharp
var employee = new Employee(...);

// すべてのプロパティは読み取り専用
employee.Id        // get のみ
employee.RowId     // get のみ
employee.Code      // get のみ
employee.PersonRowId // get のみ

// 変更は許可されない
// employee.Code = newCode;  // CS0200: Property cannot be assigned
```

---

## 🔄 型安全性

### Entity<EmployeeId> パターン

Employee は `Entity<EmployeeId>` を継承します。

```csharp
public class Employee : Entity<EmployeeId>
{
    // Id は Entity<EmployeeId> から自動的に EmployeeId 型
    public EmployeeId Id { get; private set; }
    
    // その他のプロパティ
    public EmployeeRowId RowId { get; private set; }
    public EmployeeCode Code { get; private set; }
    public PersonRowId PersonRowId { get; private set; }
}
```

### Repository での型安全性

```csharp
// 型安全
var employee = await _employeeRepository.FindByIdAsync(employeeId);
// Employee 型が保証される

// 型チェック
if (employee is null) { ... }
```

---

## 🚫 使用時の注意点

### ❌ null チェック不要（Domain層では null 禁止）

```csharp
// ❌ 間違い
if (employee?.Code != null) { ... }

// ✅ 正しい
// Employee は常に有効な Code を保持
if (employee.Code.Division.IsRegularEmployee) { ... }
```

### ⚠️ 検証責務の分離

- **ValueObject**: 個別の値の検証（EmployeeCode での範囲検証など）
- **Entity**: 集約内の一貫性の検証（今回は不要。ValueObject で検証済み）
- **Repository**: ビジネスルールの検証（例：同じコードの従業員が複数存在しないか）

```csharp
// ValueObject で検証済み
var code = EmployeeCode.From(division, number);  // 無効なら例外

// Entity では信頼して使用
var employee = new Employee(id, rowId, code, personRowId);  // 追加検証なし
```

---

## 参考資料

- 詳細設計: `Employee_詳細設計書.md`
- 単体テスト仕様: `Employee_単体テスト仕様書.md`
- ValueObject: `../../../SharedKernel/ValueObjects/Identifiers/`
- Entity基底クラス: `../../../SharedKernel/Entities/Entity.cs`
