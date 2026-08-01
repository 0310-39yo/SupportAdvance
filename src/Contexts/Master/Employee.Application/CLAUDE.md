# Employee.Application レイヤー

従業員管理の Use Case。

## Repository インターフェース

### IEmployeeRepository
- GetByIdAsync, GetByEmployeeNumberAsync, GetByEmailAsync
- GetByDepartmentIdAsync（部門単位での従業員一覧取得）
- CreateAsync, UpdateAsync, DeleteAsync

### IDepartmentRepository
- GetByIdAsync, GetByNameAsync, GetAllAsync
- GetByParentIdAsync（子部門取得）
- CreateAsync, UpdateAsync, DeleteAsync

## Use Case

### CreateEmployeeUseCase
- EmployeeNumber と Email の重複チェック
- 入社日に IClock.JstNow を使用
- Employee エンティティを生成してリポジトリに保存
- RowId を返す

### TransferEmployeeUseCase
- 従業員が存在することを確認
- DepartmentId を変更
- Repository で更新

### PromoteEmployeeUseCase
- 従業員が存在することを確認
- JobTitle を変更
- Repository で更新

## 依存関係

### 許可される参照
- Employee.Domain（ビジネスロジック）
- SharedKernel（基盤型）
- Common（ユーティリティ）
- Application（汎用層のインターフェース）

### 禁止される参照
- Infrastructure（実装のみ。DI で注入）
- Presentation

## 実装パターン

```csharp
public class CreateEmployeeUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IClock _clock;

    public CreateEmployeeUseCase(IEmployeeRepository employeeRepository, IClock clock)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<RowId> ExecuteAsync(
        string employeeNumber,
        string firstName,
        string lastName,
        string email,
        RowId departmentId,
        string jobTitle)
    {
        var employee = new Employee(
            employeeNumber,
            firstName,
            lastName,
            email,
            departmentId,
            jobTitle,
            _clock.JstNow);  // IClock.JstNow を使用

        await _employeeRepository.CreateAsync(employee);
        return employee.Id; // RowId を返す
    }
}
```

## RowId マッピング

Repository は Mapper 経由で RowId ↔ long を変換：
- Entity の RowId → DbModel の long（ToDbModel）
- DbModel の long → Entity の RowId（ToDomainEntity）

詳細は Employee.Infrastructure の Mapper 参照。
