# Employee Context Application層 - 詳細設計書

**作成日:** 2026-08-11  
**対象読者:** AI実装者  
**内容:** クラス設計、メソッド実装指示、DI設計

---

## 📋 概要

本書は、Employee Context Application層の **実装者向け** 詳細設計です。

各クラスの責務、メソッドシグネチャ、実装フロー、DI設定を記載します。

---

## 🏗️ クラス構成

### プロジェクト構造

```
Employee.Application/
├── UseCases/
│   ├── CreateEmployeeUseCase.cs
│   ├── GetEmployeeByIdUseCase.cs
│   ├── GetEmployeesByPersonRowIdUseCase.cs
│   ├── UpdateEmployeeUseCase.cs
│   └── DeleteEmployeeUseCase.cs
├── Dtos/
│   ├── Requests/
│   │   ├── CreateEmployeeRequest.cs
│   │   ├── UpdateEmployeeRequest.cs
│   │   └── DeleteEmployeeRequest.cs
│   ├── Responses/
│   │   ├── EmployeeDto.cs
│   │   ├── GetEmployeeByIdResponse.cs
│   │   └── GetEmployeesByPersonRowIdResponse.cs
│   └── Mappers/
│       └── EmployeeDtoMapper.cs
├── Exceptions/
│   ├── EmployeeApplicationException.cs
│   ├── EntityNotFoundException.cs
│   ├── OptimisticLockException.cs
│   └── AlreadyDeletedException.cs
└── Services/
    └── EmployeeApplicationService.cs
```

---

## 📝 Use Case 実装指示

### CreateEmployeeUseCase

**ファイル:** `UseCases/CreateEmployeeUseCase.cs`

**クラス定義:**
```csharp
namespace SupportAdvance.Contexts.Employee.Application.UseCases;

using SupportAdvance.Application.Repositories;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 新規従業員を作成する Use Case
/// 【責務】Request 検証 → ValueObject 生成 → Entity 生成 → Repository 保存 → DTO 返却
/// </summary>
public class CreateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;
    private readonly IClock _clock;

    public CreateEmployeeUseCase(IEmployeeRepository repository, IClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// 従業員を新規作成
    /// </summary>
    /// <param name="request">作成リクエスト</param>
    /// <returns>作成後の従業員DTO</returns>
    /// <exception cref="ArgumentException">入力値が無効な場合</exception>
    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeRequest request)
    {
        // 【Step 1】入力検証
        if (request.PersonRowId <= 0)
            throw new ArgumentException("PersonRowId must be > 0", nameof(request.PersonRowId));

        if (!IsValidDivisionCode(request.DivisionCode))
            throw new ArgumentException($"Invalid DivisionCode: {request.DivisionCode}", 
                nameof(request.DivisionCode));

        if (request.EmployeeNumber < 1001 || request.EmployeeNumber > 9999)
            throw new ArgumentException("EmployeeNumber must be between 1001 and 9999", 
                nameof(request.EmployeeNumber));

        // 【Step 2】ValueObject 生成
        var division = ConvertToDivision(request.DivisionCode);
        var number = EmployeeNumber.From(request.EmployeeNumber);
        var code = EmployeeCode.From(division, number);
        var personRowId = PersonRowId.From(request.PersonRowId);

        // 【Step 3】Domain Entity 生成
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(0),  // DB が採番
            code,
            personRowId
        );

        // 【Step 4】Repository で永続化
        await _repository.AddAsync(employee);

        // 【Step 5】DTO に変換して返却
        return employee.ToDto();
    }

    private bool IsValidDivisionCode(string? code)
    {
        return code is "M" or "T" or "C";
    }

    private EmployeeDivision ConvertToDivision(string code)
    {
        return code switch
        {
            "M" => EmployeeDivision.RegularEmployee(),
            "T" => EmployeeDivision.Dispatched(),
            "C" => EmployeeDivision.Contractor(),
            _ => throw new ArgumentException($"Invalid division code: {code}")
        };
    }
}
```

**実装ポイント:**
- ✅ 入力値の完全な検証
- ✅ ValueObject への早期変換（型安全性）
- ✅ Entity.Create() で新規生成
- ✅ Repository.AddAsync() で永続化
- ✅ DTO.ToDto() で変換して返却

---

### GetEmployeeByIdUseCase

**ファイル:** `UseCases/GetEmployeeByIdUseCase.cs`

```csharp
public class GetEmployeeByIdUseCase
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeeByIdUseCase(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// ID で従業員を検索
    /// </summary>
    /// <param name="employeeId">検索対象ID</param>
    /// <returns>見つかった従業員、見つからない場合は null</returns>
    public async Task<EmployeeDto?> ExecuteAsync(Guid employeeId)
    {
        // EmployeeId に変換
        var id = EmployeeId.From(employeeId);

        // Repository で検索
        var employee = await _repository.GetByIdAsync(id);

        // 結果を DTO に変換（null チェックは呼び出し元で）
        return employee?.ToDto();
    }
}
```

---

### GetEmployeesByPersonRowIdUseCase

**ファイル:** `UseCases/GetEmployeesByPersonRowIdUseCase.cs`

```csharp
public class GetEmployeesByPersonRowIdUseCase
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeesByPersonRowIdUseCase(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 人事マスタ行ID で従業員群を検索
    /// </summary>
    /// <param name="personRowId">人事マスタ行ID</param>
    /// <returns>見つかった従業員のリスト（見つからない場合は空リスト）</returns>
    public async Task<IReadOnlyList<EmployeeDto>> ExecuteAsync(long personRowId)
    {
        if (personRowId <= 0)
            throw new ArgumentException("PersonRowId must be > 0", nameof(personRowId));

        var id = PersonRowId.From(personRowId);
        var employees = await _repository.GetByPersonRowIdAsync(id);

        return employees.Select(e => e.ToDto()).ToList();
    }
}
```

---

### UpdateEmployeeUseCase

**ファイル:** `UseCases/UpdateEmployeeUseCase.cs`

```csharp
public class UpdateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;

    public UpdateEmployeeUseCase(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 従業員情報を更新
    /// 【注意】Entity に ChangeCode() メソッドを追加する必要がある
    /// </summary>
    public async Task<EmployeeDto> ExecuteAsync(UpdateEmployeeRequest request)
    {
        if (request.EmployeeId == Guid.Empty)
            throw new ArgumentException("Invalid EmployeeId", nameof(request.EmployeeId));

        var id = EmployeeId.From(request.EmployeeId);

        // 既存 Entity を取得
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
            throw new EntityNotFoundException($"Employee not found: {id}");

        // 更新内容を適用（DivisionCode/EmployeeNumber が指定された場合）
        if (request.DivisionCode != null && request.EmployeeNumber.HasValue)
        {
            var division = ConvertToDivision(request.DivisionCode);
            var number = EmployeeNumber.From(request.EmployeeNumber.Value);
            var newCode = EmployeeCode.From(division, number);
            // TODO: employee.ChangeCode(newCode) を呼び出し
            // Entity.ChangeCode() メソッドが必要
        }

        // 更新を永続化
        await _repository.UpdateAsync(employee);

        return employee.ToDto();
    }

    private EmployeeDivision ConvertToDivision(string code)
    {
        return code switch
        {
            "M" => EmployeeDivision.RegularEmployee(),
            "T" => EmployeeDivision.Dispatched(),
            "C" => EmployeeDivision.Contractor(),
            _ => throw new ArgumentException($"Invalid division code: {code}")
        };
    }
}
```

---

### DeleteEmployeeUseCase

**ファイル:** `UseCases/DeleteEmployeeUseCase.cs`

```csharp
public class DeleteEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;

    public DeleteEmployeeUseCase(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 従業員を論理削除
    /// </summary>
    public async Task ExecuteAsync(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Invalid EmployeeId", nameof(employeeId));

        var id = EmployeeId.From(employeeId);

        // 存在確認（削除前にチェック）
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
            throw new EntityNotFoundException($"Employee not found: {id}");

        // 論理削除
        await _repository.DeleteAsync(id);
    }
}
```

---

## 🎯 DTO 実装指示

### Request DTOs

**CreateEmployeeRequest.cs:**
```csharp
namespace SupportAdvance.Contexts.Employee.Application.Dtos;

public record CreateEmployeeRequest
{
    public required long PersonRowId { get; init; }
    public required string DivisionCode { get; init; }
    public required int EmployeeNumber { get; init; }
}
```

**UpdateEmployeeRequest.cs:**
```csharp
public record UpdateEmployeeRequest
{
    public required Guid EmployeeId { get; init; }
    public string? DivisionCode { get; init; }
    public int? EmployeeNumber { get; init; }
}
```

**DeleteEmployeeRequest.cs:**
```csharp
public record DeleteEmployeeRequest
{
    public required Guid EmployeeId { get; init; }
}
```

### Response DTO

**EmployeeDto.cs:**
```csharp
public record EmployeeDto
{
    public required Guid Id { get; init; }
    public required long RowId { get; init; }
    public required string Code { get; init; }
    public required long PersonRowId { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

### Mapper

**EmployeeDtoMapper.cs:**
```csharp
namespace SupportAdvance.Contexts.Employee.Application.Dtos;

using SupportAdvance.Contexts.Employee.Domain.Entities;

public static class EmployeeDtoMapper
{
    public static EmployeeDto ToDto(this Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id.Value,
            RowId = employee.RowId.Value,
            Code = employee.Code.ToString(),
            PersonRowId = employee.PersonRowId.Value,
            CreatedAt = employee.CreatedAt.Value  // Entity に CreatedAt 追加必要
        };
    }
}
```

---

## 🔧 Exception クラス実装指示

**Exceptions/EmployeeApplicationException.cs:**
```csharp
namespace SupportAdvance.Contexts.Employee.Application.Exceptions;

public class EmployeeApplicationException : Exception
{
    public EmployeeApplicationException(string message) : base(message) { }
}

public class EntityNotFoundException : EmployeeApplicationException
{
    public EntityNotFoundException(string message) : base(message) { }
}

public class OptimisticLockException : EmployeeApplicationException
{
    public OptimisticLockException(string message) : base(message) { }
}

public class AlreadyDeletedException : EmployeeApplicationException
{
    public AlreadyDeletedException(string message) : base(message) { }
}
```

---

## 🔌 依存注入設計

### DI Container 設定（Program.cs）

```csharp
// Employee.Application 登録
services.AddScoped<IEmployeeRepository>(sp =>
    new EmployeeRepository(
        new EmployeeMapper(),
        sp.GetRequiredService<IClock>()
    )
);

// Use Cases 登録
services.AddScoped<CreateEmployeeUseCase>();
services.AddScoped<GetEmployeeByIdUseCase>();
services.AddScoped<GetEmployeesByPersonRowIdUseCase>();
services.AddScoped<UpdateEmployeeUseCase>();
services.AddScoped<DeleteEmployeeUseCase>();
```

---

## 📋 実装チェックリスト

### Use Cases
- [ ] CreateEmployeeUseCase 実装
- [ ] GetEmployeeByIdUseCase 実装
- [ ] GetEmployeesByPersonRowIdUseCase 実装
- [ ] UpdateEmployeeUseCase 実装
- [ ] DeleteEmployeeUseCase 実装

### DTOs
- [ ] CreateEmployeeRequest 実装
- [ ] UpdateEmployeeRequest 実装
- [ ] DeleteEmployeeRequest 実装
- [ ] EmployeeDto 実装
- [ ] EmployeeDtoMapper 実装

### Exceptions
- [ ] EmployeeApplicationException 実装
- [ ] EntityNotFoundException 実装
- [ ] OptimisticLockException 実装
- [ ] AlreadyDeletedException 実装

### 統合
- [ ] Employee.Application.csproj 作成
- [ ] ProjectReference 設定
- [ ] ビルド成功確認

---

## 🎓 注意点

### Entity への追加メソッド

以下のメソッドを Employee Entity に追加する必要がある場合があります：

```csharp
// 【Example】Employee.cs に追加
public class Employee : Entity<EmployeeId>
{
    // ...既存コード...

    /// <summary>
    /// 従業員コードを変更
    /// </summary>
    public void ChangeCode(EmployeeCode newCode)
    {
        if (newCode == null)
            throw new ArgumentNullException(nameof(newCode));
        Code = newCode;
    }
}
```

### DTO 変換の早期化

Entity から DTO への変換は **Application層で必ず実施**
- Domain層を Presentation層に公開しない
- Serialization 時にセキュリティリスク回避

---

**次のドキュメント:** [Application_単体テスト仕様書.md](Application_単体テスト仕様書.md)
