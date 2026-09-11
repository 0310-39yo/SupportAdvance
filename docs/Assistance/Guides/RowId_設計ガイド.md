---
name: RowId_設計ガイド
description: 集約を一意識別する long ベース RowId パターン。システム主キー、採番方法、複数テーブル集約対応
metadata:
  type: ガイドライン
---

# RowId 設計ガイド

集約（AggregateRoot）を一意に識別するための **long ベースの RowId パターン** です。

---

## 📋 基本原則

### RowId とは

**RowId** = 集約ルートの物理キーを一意に識別するシステムID

| 項目 | 内容 |
|------|------|
| **型** | long ベースの ValueObject |
| **用途** | テーブルの主キー（システム技術的な識別子） |
| **採番方法** | ISequenceProvider で採番（ApplicationService で INSERT 前に確定） |
| **人間可読性** | なし（システム用） |
| **テーブル構成** | テーブルに依存する物理キー |

---

## 🏗️ 実装パターン

### 1. 基底クラス：RowId（SharedKernel）

```csharp
// src/SharedKernel/ValueObjects/Identifiers/RowId.cs

namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// テーブル行の物理キーを表す long ベース ValueObject の基底クラス
/// 
/// 【責務】
/// - long 値の保持（1 以上、またはオプション型は null を Unset に変換）
/// - 等価性判定（long ベース）
/// - IsSet フラグで未設定状態を管理
/// 
/// 【継承】
/// 各集約は RowId を継承し、固有の ID クラスを定義
/// 例：EmployeeRowId, DepartmentRowId, PersonRowId など
/// 
/// 【型安全性】
/// - 異なる集約の RowId を型チェックで区別
/// - EmployeeRowId と DepartmentRowId は互換性なし
/// 
/// 【2つのコンストラクタパターン】
/// - 必須型: protected RowId(long value, bool isSet) で IsSet=true
/// - オプション型: protected RowId(bool isSet) で IsSet=false（Unset状態）
/// </summary>
public abstract class RowId : ValueObject
{
    /// <summary>
    /// long 値（1 以上、またはオプション型は未設定時の既定値）
    /// </summary>
    public long Value { get; protected set; }

    /// <summary>
    /// 値が設定されているかを示すフラグ
    /// 【責務】オプション型での未設定状態を型安全に管理
    /// </summary>
    public bool IsSet { get; protected set; }

    /// <summary>
    /// 必須型用コンストラクタ（IsSet=true）
    /// </summary>
    protected RowId(long value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    /// <summary>
    /// オプション型用コンストラクタ（Unset状態、IsSet=false）
    /// </summary>
    protected RowId(bool isSet)
    {
        Value = 0;  // Unset時のデフォルト値
        IsSet = isSet;
    }

    /// <summary>
    /// 指定された long 値を検証する（派生クラスで実装）
    /// 【実装責務】MinValue チェック、範囲検証
    /// </summary>
    /// <param name="value">検証対象値</param>
    /// <exception cref="ArgumentOutOfRangeException">不正な値の場合</exception>
    public abstract void Validate(long value);
}
```

### 2. 集約固有の RowId 実装例

#### 必須型：EmployeeRowId（従業員）

```csharp
// src/Contexts/Employee/Employee.Domain/ValueObjects/Employee/EmployeeRowId.cs

using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 従業員の行ID（必須型）
/// 【用途】t_employees.row_id を型安全に管理
/// 【特性】IsSet=true が常に成立（未設定状態なし）
/// </summary>
public sealed class EmployeeRowId : RowId, IEquatable<EmployeeRowId>
{
    public const long MinValue = 1L;

    public long Value => ValueField;

    private EmployeeRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 従業員行IDを生成する（ISequenceProvider で採番後）
    /// </summary>
    public static EmployeeRowId From(long value) => new(value);

    /// <summary>
    /// DB値から復元する
    /// </summary>
    public static bool TryFromDbValue(long value, out EmployeeRowId result)
    {
        try
        {
            result = From(value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            result = null!;
            return false;
        }
    }

    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                $"EmployeeRowId must be >= {MinValue}");
    }
}
```

#### オプション型：ManagerEmployeeRowId（部署管理者）

```csharp
// src/Contexts/Employee/Employee.Domain/ValueObjects/Department/ManagerEmployeeRowId.cs

using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 部署管理者（従業員）の行ID（オプション型）
/// 【用途】t_departments.manager_row_id を型安全に管理
/// 【特性】IsSet=true で管理者あり、IsSet=false で「管理者なし」を表現
/// 【DB null 処理】自動的に Unset() に変換（Domain は null 免除）
/// </summary>
public sealed class ManagerEmployeeRowId : RowId, IEquatable<ManagerEmployeeRowId>
{
    private const long MinValue = 1L;

    public long Value => ValueField;

    /// <summary>
    /// Unset状態（管理者なし）のコンストラクタ
    /// </summary>
    private ManagerEmployeeRowId(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 値設定状態のコンストラクタ
    /// </summary>
    private ManagerEmployeeRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 未設定状態を表す Unset インスタンスを生成
    /// 【用途】「管理者なし」を型安全に表現（null 代替）
    /// </summary>
    public static ManagerEmployeeRowId Unset() => new(false);

    /// <summary>
    /// 指定値から生成（管理者あり）
    /// </summary>
    public static ManagerEmployeeRowId From(long value) => new(value);

    /// <summary>
    /// nullable long から型安全に生成
    /// 【null 処理】null は自動的に Unset() に変換（成功を返す）
    /// </summary>
    public static bool TryFrom(long? input, out ManagerEmployeeRowId result)
    {
        result = null!;

        if (!input.HasValue)
        {
            result = Unset();  // null → Unset（成功）
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// DB値から復元（null は Unset に自動変換）
    /// 【責務】DB の null ハンドリングを自動化
    /// </summary>
    public static bool TryFromDbValue(long? input, out ManagerEmployeeRowId result)
    {
        result = null!;

        if (!input.HasValue)
        {
            result = Unset();  // DB NULL → Unset（Domain は null 免除）
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 管理者が設定されているかを判定する（IsSet の別名）
    /// 【用途】Domain層での直感的な状態判定
    /// </summary>
    public bool HasManager => IsSet;

    public override void Validate(long normalized)
    {
        if (normalized <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                $"ManagerEmployeeRowId must be > 0");
    }
}
```

### 3. Entity での RowId パラメータ

```csharp
// ✅ 正しいパターン：RowId は非null 必須パラメータ

public sealed class Employee : AggregateRoot<EmployeeRowId>
{
    /// <summary>
    /// 従業員を生成する（ファクトリメソッド）
    /// 【入力】RowId は事前採番済み（ApplicationService で確定）
    /// </summary>
    public static Employee Create(
        EmployeeRowId rowId,              // ← 非null（ISequenceProvider で採番済み）
        BizDivision typeDivision,
        BizId bizId,
        BizCode bizCode,
        RetiredOn? retiredOn,
        Person person,
        IEnumerable<DepartmentMembership> departmentMemberships) =>
        new(rowId, typeDivision, bizId, bizCode, retiredOn ?? RetiredOn.Unset(), 
            person, departmentMemberships.ToList());

    /// <summary>
    /// DB から復元する（ファクトリメソッド）
    /// 【入力】RowId は DB から読み込み済み
    /// </summary>
    public static Employee Reconstruct(
        EmployeeRowId rowId,              // ← 非null（DB から読み込み済み）
        BizDivision typeDivision,
        BizId bizId,
        BizCode bizCode,
        RetiredOn retiredOn,
        Person person,
        IEnumerable<DepartmentMembership> departmentMemberships,
        byte[]? rowVersion = null)
    {
        var employee = new Employee(rowId, typeDivision, bizId, bizCode, retiredOn, 
            person, departmentMemberships.ToList());
        if (rowVersion != null)
        {
            employee.RowVersion = rowVersion;
        }
        return employee;
    }
}
```

---

## 🔄 RowId の採番フロー

### ApplicationService での採番

```csharp
public class CreateEmployeeUseCase
{
    private readonly ISequenceProvider _sequenceProvider;
    private readonly IEmployeeRepository _repository;

    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeRequest request)
    {
        // Step 1: RowId を採番（プログラム側）
        var sequenceValue = await _sequenceProvider.GetNextValueAsync();
        var rowId = EmployeeRowId.From(sequenceValue);

        // Step 2: Domain Entity を生成（RowId 確定状態）
        var employee = Employee.Create(
            rowId,                    // ← 採番済み
            division,
            bizId,
            bizCode,
            null,                     // RetiredOn はオプション
            person,
            new List<DepartmentMembership>()
        );

        // Step 3: Repository で永続化
        await _repository.AddAsync(employee);

        return employee.ToDto();
    }
}
```

### Repository での読み込み

```csharp
public async Task<Employee?> GetByIdAsync(EmployeeRowId id)
{
    // DB から読み込み
    var dbModel = await _connection.QueryFirstOrDefaultAsync<EmployeeDbModel>(
        "SELECT * FROM m_employees WHERE row_id = @rowId",
        new { rowId = id.Value });

    if (dbModel == null)
        return null;

    // RowId を復元
    if (!EmployeeRowId.TryFromDbValue(dbModel.RowId, out var rowId))
        throw new InvalidOperationException($"Invalid RowId: {dbModel.RowId}");

    // Entity を復元
    return _mapper.ToDomainEntity(dbModel, rowId);
}
```

---

## 📊 RowId と ビジネスID の関係

| 層 | ID 型 | 責務 | 例 |
|---|---|---|---|
| **DB** | bigint (PK) | テーブル行を一意識別 | row_id |
| **Infrastructure** | RowId ValueObject | DB の物理キーをラッピング | EmployeeRowId.From(123) |
| **Domain** | Entity<RowId> | 集約の一意識別（ビジネスセマンティクス） | Employee(rowId, ...) |
| **Application** | DTO | 外部インターフェース | EmployeeId (API では除外) |

---

## 🔑 複数テーブル集約での RowId 管理

```csharp
// Employee（親）と Person（子）の複数テーブル集約

public sealed class Employee : AggregateRoot<EmployeeRowId>
{
    private EmployeeRowId _rowId;           // m_employees.row_id
    public Person Person { get; private set; }
}

public sealed class Person : Entity<PersonRowId>
{
    private PersonRowId _rowId;             // m_persons.row_id（FK: employee_row_id）
}

// 各テーブルが独立した RowId を持つ
// 関連は FK（employee_row_id）で管理される
```

---

## ✅ 実装チェックリスト

### 必須型（EmployeeRowId など）

- [ ] **RowId を継承**: `class XXXRowId : RowId, IEquatable<XXXRowId>`
- [ ] **MinValue 定義**: `public const long MinValue = 1L`
- [ ] **コンストラクタ**: `private XXXRowId(long value) : base(value, true)`
- [ ] **From() メソッド**: `public static XXXRowId From(long value) => new(value);`
- [ ] **TryFromDbValue()**: DB long → XXXRowId 変換（例外時は false）
- [ ] **Validate()**: `if (normalized < MinValue) throw ...`
- [ ] **Equals/GetHashCode**: RowId ベースの等価性判定
- [ ] **Entity パラメータ**: Create/Reconstruct で非null パラメータ
- [ ] **採番方法**: ApplicationService で ISequenceProvider を使用

### オプション型（ManagerEmployeeRowId など）

- [ ] **RowId を継承**: `class XXXRowId : RowId, IEquatable<XXXRowId>`
- [ ] **2つのコンストラクタ**: 
  - `private XXXRowId(bool isSet) : base(isSet)` （Unset用）
  - `private XXXRowId(long value) : base(value, true)` （値設定用）
- [ ] **Unset() メソッド**: `public static XXXRowId Unset() => new(false);`
- [ ] **From() メソッド**: `public static XXXRowId From(long value) => new(value);`
- [ ] **TryFrom(long?)**: null は自動的に Unset() に変換して成功を返す
- [ ] **TryFromDbValue(long?)**: DB null → Unset() に自動変換
- [ ] **Validate()**: MinValue チェック（IsSet=true時のみ検証対象）
- [ ] **IsSet 別名**: `public bool HasXxx => IsSet;` で意味を明確化
- [ ] **Equals/GetHashCode**: IsSet と Value の両方で判定
- [ ] **テスト**: 
  - null → Unset の変換確認
  - IsSet フラグの状態管理確認
  - 型安全性テスト

---

## 💡 よくあるエラー

### ❌ エラー 1: RowId を null 許容で定義

```csharp
// ❌ 間違い
public static Employee Create(
    EmployeeRowId? rowId = null,  // null 許容
    ...
)
{
    var id = rowId ?? EmployeeRowId.From(0);  // 問題
}
```

**問題**: RowId は採番前から確定すべき  
**正しい**: 
```csharp
public static Employee Create(
    EmployeeRowId rowId,          // 非null（必須）
    ...
)
```

### ❌ エラー 2: RowId.New() メソッドの使用

```csharp
// ❌ 間違い（RowId.New() は存在しない）
var rowId = EmployeeRowId.New();
```

**正しい**:
```csharp
var sequenceValue = await _sequenceProvider.GetNextValueAsync();
var rowId = EmployeeRowId.From(sequenceValue);
```

### ❌ エラー 3: DB の DEFAULT に頼る

```csharp
// ❌ 間違い（DB での採番を期待）
var employee = Employee.Create(
    EmployeeRowId.From(0),  // 0 で挿入して AUTO_INCREMENT を期待
    ...
);
```

**正しい**:
```csharp
// 先に采番
var rowId = EmployeeRowId.From(
    await _sequenceProvider.GetNextValueAsync()
);
var employee = Employee.Create(rowId, ...);
```

---

## 📖 参考ドキュメント

- [Entity_設計ガイドライン.md](Entity_設計ガイドライン.md) — Entity<RowId> パターン
- [TABLE_DESIGN_STANDARDS.md](TABLE_DESIGN_STANDARDS.md) — テーブル設計での RowId
- [Mapper_パターンガイド.md](Mapper_パターンガイド.md) — DbModel ↔ Entity 変換
- [Repository_パターンガイド.md](Repository_パターンガイド.md) — Repository での RowId 管理

---

## 📝 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.2 | 2026-09-12 | Claude Code | **オプション型パターン追加**。ManagerEmployeeRowId（オプション型）の実装例を追加。Unset()、TryFrom(long?)、TryFromDbValue(long?)、IsSet別名プロパティの説明を追加。必須型 vs オプション型のチェックリストを分離 |
| 1.1 | 2026-09-12 | Claude Code | Entity_設計ガイドライン.md との整合性確認。実装準拠確認済み（long ベース RowId、採番方法、複数テーブル集約パターン） |
| 1.0 | 2026-09-12 | Claude Code | 初版作成。GUID ベース AggregateId から long ベース RowId に転換。采番方法、複数テーブル集約パターン、実装例 |
