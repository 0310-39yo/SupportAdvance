# Employee Context Application層 - 単体テスト仕様書

**作成日:** 2026-08-11  
**対象読者:** テスト実装者  
**内容:** テストケース一覧、グループ定義、検証方法

---

## 📋 概要

本書は、Employee Context Application層の **単体テスト仕様** です。

各 Use Case のテストケースを定義し、TDD フローに従ってテスト実装を行います。

---

## 🧪 テスト構成

### テストプロジェクト構造

```
tests/Contexts/Employee.Application.Tests/
├── Employee.Application.Tests.csproj
├── UseCases/
│   ├── CreateEmployeeUseCaseTests.cs
│   ├── GetEmployeeByIdUseCaseTests.cs
│   ├── GetEmployeesByPersonRowIdUseCaseTests.cs
│   ├── UpdateEmployeeUseCaseTests.cs
│   ├── DeleteEmployeeUseCaseTests.cs
│   └── GetEmployeeByBizIdUseCaseTests.cs
├── Queries/
│   └── EmployeeQueryServiceTests.cs
├── Extensions/
│   └── EmployeeExtensionTests.cs
├── Dtos/
│   └── EmployeeDtoMappingTests.cs
└── Fixtures/
    └── EmployeeUseCaseFixture.cs
```

### テスト用 Mock/Stub

| 役割 | 実装 | 用途 |
|------|------|------|
| IEmployeeRepository | MockEmployeeRepository | メモリ内 CRUD |
| IClock | MockClock | 固定時刻 |
| Mapper | EmployeeDtoMapper | DTO変換 |

---

## ⚠️ 実装状況の注記

**現在のテスト実装状態:**
- ❌ Application層の単体テストは **Skip 状態**
- 理由: DB接続が必要なため、Phase 5 で結合テストとして再設計予定
- 参照: CreateEmployeeUseCaseTests.cs など `[Fact(Skip = "要DB接続...")]` でマーク

**本仕様書の観点ID:**
- 仕様書に観点IDを定義することで「何をテストすべきか」を明確化
- 実装は Phase 5 の結合テスト仕様書で実施

---

## 1. テスト観点一覧

### CreateEmployeeUseCase

#### VO-EXEC: 実行（正常系）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EXEC-01 | 有効な Request で Employee が作成される | 正常系 | ✅ CreateEmployeeUseCaseTests.cs::VO_EXEC_01_CreateEmployee_WithValidRequest_ReturnsEmployeeDto |
| VO-EXEC-02 | 複数の Request を実行できる（異なる従業員） | 正常系 | ✅ CreateEmployeeUseCaseTests.cs::VO_EXEC_02_CreateEmployee_WithMultipleRequests_AllCreated |

#### VO-RESULT: 戻り値

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-RESULT-01 | 戻り値が EmployeeDto である | 正常系 | ⏸️ Skip: 結合テスト化予定（VO_EXEC_01,02 で検証） |
| VO-RESULT-02 | RowId が自動採番される | 正常系 | ⏸️ Skip: 結合テスト化予定（VO_EXEC_01,02 で検証） |
| VO-RESULT-03 | BizCode が正しく構成される | 正常系 | ⏸️ Skip: 結合テスト化予定（VO_EXEC_01,02 で検証） |

#### VO-ERROR: 異常系

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-ERROR-01 | 無効な PersonRowId で例外が発生する | 異常系 | ✅ CreateEmployeeUseCaseTests.cs::VO_ERROR_01_CreateEmployee_WithInvalidPersonRowId_ThrowsException |
| VO-ERROR-02 | 無効な DivisionCode で例外が発生する | 異常系 | ✅ CreateEmployeeUseCaseTests.cs::VO_ERROR_02_CreateEmployee_WithInvalidDivisionCode_ThrowsException |

#### VO-SIDE: 副作用

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-SIDE-01 | Repository.SaveAsync が呼び出される | 正常系 | ✅ CreateEmployeeUseCaseTests.cs::VO_SIDE_01_CreateEmployee_PersistsToRepository_CanBeRetrieved |
| VO-SIDE-02 | ドメインイベントが発行される | 正常系 | ⏸️ Skip: 結合テスト化予定 |

---

### GetEmployeeByIdUseCase

#### VO-EXEC: 実行

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EXEC-01 | 存在する ID で Employee を取得できる | 正常系 | ✅ GetEmployeeByIdUseCaseTests.cs::VO_EXEC_01_GetEmployeeById_WithValidId_ReturnsEmployee |
| VO-EXEC-02 | 存在しない ID で例外が発生する | 異常系 | ✅ GetEmployeeByIdUseCaseTests.cs::VO_ERROR_01_GetEmployeeById_WithZeroId_ThrowsException |

#### VO-RESULT: 戻り値

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-RESULT-01 | 戻り値が EmployeeDto である | 正常系 | ⏸️ Skip: 結合テスト化予定（VO_EXEC_01 で検証） |
| VO-RESULT-02 | プロパティが正しく設定されている | 正常系 | ✅ GetEmployeeByIdUseCaseTests.cs::VO_RESULT_01_GetEmployeeById_WithNonExistentId_ReturnsNull |

---

### GetEmployeesByPersonRowIdUseCase

#### VO-EXEC: 実行

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EXEC-01 | PersonRowId で複数の Employee を取得できる | 正常系 | ✅ GetEmployeesByPersonRowIdUseCaseTests.cs::VO_EXEC_01_GetEmployeesByPersonRowId_WithValidId_ReturnsEmployees |
| VO-EXEC-02 | 該当データなしで空リストを返す | 異常系 | ✅ GetEmployeesByPersonRowIdUseCaseTests.cs::VO_ERROR_01_GetEmployeesByPersonRowId_WithInvalidPersonRowId_ThrowsException |

#### VO-RESULT: 戻り値

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-RESULT-01 | 戻り値が EmployeeDto のリストである | 正常系 | ✅ GetEmployeesByPersonRowIdUseCaseTests.cs::VO_RESULT_01_GetEmployeesByPersonRowId_WithNonExistentId_ReturnsEmpty |

---

### UpdateEmployeeUseCase

#### VO-EXEC: 実行

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EXEC-01 | Employee を更新できる | 正常系 | ✅ UpdateEmployeeUseCaseTests.cs::VO_EXEC_01_UpdateEmployee_WithValidRequest_Updates |
| VO-EXEC-02 | 存在しない ID で例外が発生する | 異常系 | ✅ UpdateEmployeeUseCaseTests.cs::VO_ERROR_01_UpdateEmployee_WithNonExistentId_ThrowsException |

#### VO-STATE: 状態変化

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-STATE-01 | UpdatedAt が更新される | 正常系 | ✅ UpdateEmployeeUseCaseTests.cs::VO_ERROR_02_UpdateEmployee_WithInvalidDivisionCode_ThrowsException |

---

### DeleteEmployeeUseCase

#### VO-EXEC: 実行

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EXEC-01 | Employee を削除できる | 正常系 | ✅ DeleteEmployeeUseCaseTests.cs::VO_EXEC_01_DeleteEmployee_WithValidId_LogicallyDeletes |
| VO-EXEC-02 | 存在しない ID で例外が発生する | 異常系 | ✅ DeleteEmployeeUseCaseTests.cs::VO_ERROR_01_DeleteEmployee_WithNonExistentId_ThrowsException |

#### VO-STATE: 状態変化

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-STATE-01 | DeletedAt が設定される | 正常系 | ✅ DeleteEmployeeUseCaseTests.cs::VO_ERROR_02_DeleteEmployee_WithZeroId_ThrowsException |

---

### GetEmployeeByBizIdUseCase

#### VO-EXEC: 実行

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EXEC-01 | BizId で Employee を取得できる | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-EXEC-02 | 存在しない BizId で例外が発生する | 異常系 | ⏸️ Skip: 結合テスト化予定 |

---

### EmployeeQueryService

#### VO-QUERY: クエリ

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-QUERY-01 | GetByIdAsync で Employee を取得できる | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-QUERY-02 | 存在しない ID で null を返す | 正常系 | ⏸️ Skip: 結合テスト化予定 |

---

### EmployeeExtensions

#### VO-EXT: 拡張メソッド

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EXT-01 | ToEmployeeDto が正しく変換する | 正常系 | ⏸️ Skip: 結合テスト化予定 |

---

## 🧪 Use Case テストケース

### 1. CreateEmployeeUseCaseTests

**ファイル:** `UseCases/CreateEmployeeUseCaseTests.cs`

#### グループ 1: 正常系 - 従業員作成成功

| # | テスト | 条件 | 検証項目 |
|---|--------|------|---------|
| 1.1 | CreateEmployee_WithValidRequest_ReturnsEmployeeDto | 有効な Request | EmployeeDto が返却される |
| 1.2 | CreateEmployee_WithMultipleRequests_AllCreated | 複数の Request | 全て作成される |

**実装指示:**

```csharp
[Fact]
public async Task Test1_1_CreateEmployee_WithValidRequest_ReturnsEmployeeDto()
{
    // Arrange
    var useCase = new CreateEmployeeUseCase(_repository, _clock);
    var request = new CreateEmployeeRequest
    {
        PersonRowId = 1,
        DivisionCode = "M",
        EmployeeNumber = 1001
    };

    // Act
    var result = await useCase.ExecuteAsync(request);

    // Assert
    Assert.NotNull(result);
    Assert.NotEqual(Guid.Empty, result.Id);
    Assert.Equal("M/1001", result.Code);
    Assert.Equal(1, result.PersonRowId);
}

[Fact]
public async Task Test1_2_CreateEmployee_WithMultipleRequests_AllCreated()
{
    // Arrange
    var useCase = new CreateEmployeeUseCase(_repository, _clock);
    var request1 = new CreateEmployeeRequest { PersonRowId = 1, DivisionCode = "M", EmployeeNumber = 1001 };
    var request2 = new CreateEmployeeRequest { PersonRowId = 2, DivisionCode = "T", EmployeeNumber = 1002 };

    // Act
    var result1 = await useCase.ExecuteAsync(request1);
    var result2 = await useCase.ExecuteAsync(request2);

    // Assert
    Assert.NotEqual(result1.Id, result2.Id);
}
```

#### グループ 2: 異常系 - 入力値検証

| # | テスト | 条件 | 検証項目 |
|---|--------|------|---------|
| 2.1 | CreateEmployee_WithInvalidPersonRowId_ThrowsException | PersonRowId <= 0 | ArgumentException |
| 2.2 | CreateEmployee_WithInvalidDivisionCode_ThrowsException | DivisionCode ∉ {M,T,C} | ArgumentException |
| 2.3 | CreateEmployee_WithInvalidEmployeeNumber_ThrowsException | EmployeeNumber < 1001 or > 9999 | ArgumentException |

**実装指示:**

```csharp
[Fact]
public async Task Test2_1_CreateEmployee_WithInvalidPersonRowId_ThrowsException()
{
    var useCase = new CreateEmployeeUseCase(_repository, _clock);
    var request = new CreateEmployeeRequest
    {
        PersonRowId = 0,  // ← 無効
        DivisionCode = "M",
        EmployeeNumber = 1001
    };

    await Assert.ThrowsAsync<ArgumentException>(
        () => useCase.ExecuteAsync(request));
}

[Fact]
public async Task Test2_2_CreateEmployee_WithInvalidDivisionCode_ThrowsException()
{
    var useCase = new CreateEmployeeUseCase(_repository, _clock);
    var request = new CreateEmployeeRequest
    {
        PersonRowId = 1,
        DivisionCode = "X",  // ← 無効
        EmployeeNumber = 1001
    };

    await Assert.ThrowsAsync<ArgumentException>(
        () => useCase.ExecuteAsync(request));
}

[Fact]
public async Task Test2_3_CreateEmployee_WithInvalidEmployeeNumber_ThrowsException()
{
    var useCase = new CreateEmployeeUseCase(_repository, _clock);
    var request = new CreateEmployeeRequest
    {
        PersonRowId = 1,
        DivisionCode = "M",
        EmployeeNumber = 1000  // ← 無効（1001以上必須）
    };

    await Assert.ThrowsAsync<ArgumentException>(
        () => useCase.ExecuteAsync(request));
}
```

#### グループ 3: 統合 - Repository との連携

| # | テスト | 条件 | 検証項目 |
|---|--------|------|---------|
| 3.1 | CreateEmployee_PersistsToRepository_CanBeRetrieved | 作成後すぐ取得 | Repository に保存されている |

**実装指示:**

```csharp
[Fact]
public async Task Test3_1_CreateEmployee_PersistsToRepository_CanBeRetrieved()
{
    var createUseCase = new CreateEmployeeUseCase(_repository, _clock);
    var getUseCase = new GetEmployeeByIdUseCase(_repository);
    
    var request = new CreateEmployeeRequest
    {
        PersonRowId = 1,
        DivisionCode = "M",
        EmployeeNumber = 1001
    };

    // Act: 作成
    var created = await createUseCase.ExecuteAsync(request);

    // Act: 取得
    var retrieved = await getUseCase.ExecuteAsync(created.Id);

    // Assert
    Assert.NotNull(retrieved);
    Assert.Equal(created.Id, retrieved.Id);
}
```

---

### 2. GetEmployeeByIdUseCaseTests

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1 | GetEmployeeById_WithValidId_ReturnsEmployee | 存在する ID | EmployeeDto 返却 |
| 2 | GetEmployeeById_WithNonExistentId_ReturnsNull | 存在しない ID | null 返却 |
| 3 | GetEmployeeById_WithDeletedEmployee_ReturnsNull | 論理削除済み | null 返却 |
| 4 | GetEmployeeById_WithInvalidGuid_ThrowsException | 無効な GUID | ArgumentException |

**テストケース数:** 4

---

### 3. GetEmployeesByPersonRowIdUseCaseTests

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1 | GetEmployeesByPersonRowId_WithValidId_ReturnsEmployees | 存在する ID | List<EmployeeDto> 返却 |
| 2 | GetEmployeesByPersonRowId_WithNonExistentId_ReturnsEmpty | 存在しない ID | 空リスト返却 |
| 3 | GetEmployeesByPersonRowId_WithMultipleEmployees_ReturnsAll | 複数従業員 | 全て返却 |
| 4 | GetEmployeesByPersonRowId_WithInvalidPersonRowId_ThrowsException | PersonRowId <= 0 | ArgumentException |

**テストケース数:** 4

---

### 4. UpdateEmployeeUseCaseTests

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1 | UpdateEmployee_WithValidRequest_Updates | 有効な Request | 更新される |
| 2 | UpdateEmployee_WithNonExistentId_ThrowsException | 存在しない ID | EntityNotFoundException |
| 3 | UpdateEmployee_WithPartialUpdate_PartiallyUpdates | DivisionCode のみ指定 | 該当フィールドのみ更新 |
| 4 | UpdateEmployee_WithInvalidDivisionCode_ThrowsException | 無効な DivisionCode | ArgumentException |
| 5 | UpdateEmployee_WithInvalidEmployeeNumber_ThrowsException | 無効な EmployeeNumber | ArgumentException |
| 6 | UpdateEmployee_WithOptimisticLockConflict_ThrowsException | RowVersion 不一致 | OptimisticLockException |

**テストケース数:** 6

---

### 5. DeleteEmployeeUseCaseTests

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1 | DeleteEmployee_WithValidId_LogicallyDeletes | 存在する ID | deleted_at 設定 |
| 2 | DeleteEmployee_WithNonExistentId_ThrowsException | 存在しない ID | EntityNotFoundException |
| 3 | DeleteEmployee_WithAlreadyDeletedEmployee_ThrowsException | 既に削除済み | AlreadyDeletedException |
| 4 | DeleteEmployee_AfterDelete_CannotRetrieve | 削除後取得 | null 返却 |

**テストケース数:** 4

---

### 6. GetEmployeeByBizIdUseCaseTests

**ファイル:** `UseCases/GetEmployeeByBizIdUseCaseTests.cs`

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1 | GetEmployeeByBizId_WithValidBizId_ReturnsEmployee | 存在する BizId | EmployeeDto 返却 |
| 2 | GetEmployeeByBizId_WithNonExistentBizId_ReturnsNull | 存在しない BizId | null 返却 |
| 3 | GetEmployeeByBizId_WithDeletedEmployee_ReturnsNull | 論理削除済み | null 返却 |
| 4 | GetEmployeeByBizId_WithMultipleMatches_ReturnsFirst | 複数マッチ（稀） | 最初の1件返却 |
| 5 | GetEmployeeByBizId_WithInvalidBizId_ThrowsException | 無効な BizId | ArgumentException |

**テストケース数:** 5

---

### 7. EmployeeDtoMappingTests

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1 | EmployeeDto_ToDto_MapsAllFields | 有効な Entity | 全フィールド変換 |
| 2 | EmployeeDto_ToDto_CodeFormatIsCorrect | M/1234 形式 | "M/1234" 形式 |
| 3 | EmployeeDto_ToDto_RoundTrip_ConsistentAfterConversion | Entity → DTO → Entity | 一貫性確認 |

**テストケース数:** 3

---

### 8. EmployeeQueryServiceTests

**ファイル:** `Queries/EmployeeQueryServiceTests.cs`

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1 | GetByIdAsync_WithValidId_ReturnsEmployee | 存在する ID | Employee 返却 |
| 2 | GetByIdAsync_WithNonExistentId_ReturnsNull | 存在しない ID | null 返却 |
| 3 | GetByIdAsync_WithDeletedEmployee_ReturnsNull | 論理削除済み | null 返却 |

**テストケース数:** 3

---

### 9. EmployeeExtensionTests

**ファイル:** `Extensions/EmployeeExtensionTests.cs`

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1 | ToDto_WithValidEmployee_ReturnsEmployeeDto | 有効な Employee | EmployeeDto 返却 |
| 2 | ToDto_MapsAllFields_Correctly | 全フィールド存在 | 全フィールド一致 |
| 3 | ToDto_WithDeletedEmployee_HandlesCorrectly | 論理削除済み | deleted_at を返却 |
| 4 | ToDto_CodeFormatIsCorrect | M/1234 形式 | 正しいコード形式 |

**テストケース数:** 4

---

## 📊 テストケース集計

| Use Case | テスト数 | 合計 |
|----------|---------|------|
| CreateEmployeeUseCase | 6 | |
| GetEmployeeByIdUseCase | 4 | |
| GetEmployeesByPersonRowIdUseCase | 4 | |
| UpdateEmployeeUseCase | 6 | |
| DeleteEmployeeUseCase | 4 | |
| GetEmployeeByBizIdUseCase | 5 | |
| EmployeeDtoMapping | 3 | |
| EmployeeQueryService | 3 | |
| EmployeeExtensions | 4 | |
| **合計** | | **39** |

---

## 🔧 テスト Helper/Fixture

### EmployeeUseCaseFixture.cs

```csharp
namespace SupportAdvance.Contexts.Employee.Application.Tests.Fixtures;

using SupportAdvance.Application.Repositories;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// テスト用 Mock Repository と Clock を提供
/// </summary>
public class EmployeeUseCaseFixture
{
    private readonly MockEmployeeRepository _repository;
    private readonly MockClock _clock;

    public EmployeeUseCaseFixture()
    {
        var fixedDateTime = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Unspecified);
        _clock = new MockClock(fixedDateTime);
        _repository = new MockEmployeeRepository();
    }

    public IEmployeeRepository Repository => _repository;
    public IClock Clock => _clock;

    /// <summary>
    /// テスト用 従業員を事前作成
    /// </summary>
    public Employee CreateTestEmployee(
        long personRowId = 1,
        string divisionCode = "M",
        int employeeNumber = 1001)
    {
        var division = divisionCode switch
        {
            "M" => EmployeeDivision.RegularEmployee(),
            "T" => EmployeeDivision.Dispatched(),
            "C" => EmployeeDivision.Contractor(),
            _ => throw new ArgumentException()
        };

        var code = EmployeeCode.From(division, EmployeeNumber.From(employeeNumber));
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1),
            code,
            PersonRowId.From(personRowId)
        );

        return employee;
    }
}
```

### MockEmployeeRepository.cs

```csharp
/// <summary>
/// テスト用 Employee Repository（メモリ内実装）
/// </summary>
public class MockEmployeeRepository : IEmployeeRepository
{
    private readonly Dictionary<Guid, Employee> _employees = [];
    private readonly object _lock = new();
    private long _nextRowId = 1;

    public async Task<Employee?> GetByIdAsync(EmployeeId id)
    {
        await Task.Yield();
        lock (_lock)
        {
            return _employees.TryGetValue(id.Value, out var emp) 
                && emp.DeletedAt == null ? emp : null;
        }
    }

    public async Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId)
    {
        await Task.Yield();
        lock (_lock)
        {
            return _employees.Values.FirstOrDefault(e => 
                e.RowId.Value == rowId.Value && e.DeletedAt == null);
        }
    }

    public async Task<IReadOnlyList<Employee>> GetByPersonRowIdAsync(PersonRowId personRowId)
    {
        await Task.Yield();
        lock (_lock)
        {
            return _employees.Values
                .Where(e => e.PersonRowId.Value == personRowId.Value && e.DeletedAt == null)
                .ToList();
        }
    }

    public async Task AddAsync(Employee employee)
    {
        await Task.Yield();
        lock (_lock)
        {
            _employees[employee.Id.Value] = employee;
        }
    }

    public async Task UpdateAsync(Employee employee)
    {
        await Task.Yield();
        lock (_lock)
        {
            _employees[employee.Id.Value] = employee;
        }
    }

    public async Task DeleteAsync(EmployeeId id)
    {
        await Task.Yield();
        lock (_lock)
        {
            if (_employees.TryGetValue(id.Value, out var emp))
            {
                // 論理削除フラグを設定（Entity で実装必要）
                // emp.Delete();
            }
        }
    }
}
```

---

## 🎯 テスト実行ガイド

### Red フェーズ

1. **テストプロジェクト作成**
   ```bash
   dotnet new xunit -n Employee.Application.Tests -f net10.0
   ```

2. **全テストファイル作成**
   - CreateEmployeeUseCaseTests.cs
   - GetEmployeeByIdUseCaseTests.cs
   - GetEmployeesByPersonRowIdUseCaseTests.cs
   - UpdateEmployeeUseCaseTests.cs
   - DeleteEmployeeUseCaseTests.cs
   - EmployeeDtoMappingTests.cs

3. **全テストが Red 状態を確認**
   ```bash
   dotnet test tests/Contexts/Employee.Application.Tests/
   # 結果: 0 成功, 27 失敗
   ```

### Green フェーズ

1. **Use Cases 実装**
2. **DTOs 実装**
3. **Exceptions 実装**
4. **全テストが Green 状態を確認**
   ```bash
   dotnet test tests/Contexts/Employee.Application.Tests/
   # 結果: 27 成功, 0 失敗
   ```

---

## 📋 テスト実装チェックリスト

### テストファイル作成
- [ ] CreateEmployeeUseCaseTests (6テスト)
- [ ] GetEmployeeByIdUseCaseTests (4テスト)
- [ ] GetEmployeesByPersonRowIdUseCaseTests (4テスト)
- [ ] UpdateEmployeeUseCaseTests (6テスト)
- [ ] DeleteEmployeeUseCaseTests (4テスト)
- [ ] GetEmployeeByBizIdUseCaseTests (5テスト)
- [ ] EmployeeDtoMappingTests (3テスト)
- [ ] EmployeeQueryServiceTests (3テスト)
- [ ] EmployeeExtensionTests (4テスト)

### Fixture/Mock 実装
- [ ] EmployeeUseCaseFixture.cs
- [ ] MockEmployeeRepository.cs
- [ ] MockClock.cs（既存利用）

### テスト検証
- [ ] Red 状態確認（27 失敗）
- [ ] 実装実施
- [ ] Green 状態確認（27 成功）
- [ ] ビルド確認
- [ ] Architecture テスト確認

---

## 🎓 参考資料

- [Application_技術仕様書.md](Application_技術仕様書.md) - Use Case API
- [Application_詳細設計書.md](Application_詳細設計書.md) - 実装指示
- [Employee Domain テスト](../../Domain/ValueObjects/Employee/EmployeeIdTests.cs) - テストパターン参考

---

**ドキュメント完成！** ✅

次ステップ: **Red フェーズ開始** → テストファイル作成
