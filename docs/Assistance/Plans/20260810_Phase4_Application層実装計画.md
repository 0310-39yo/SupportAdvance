# Phase 4: Application層実装計画

**日付:** 2026-08-10  
**対象:** Employee Context の Application層（Use Cases）  
**開発方法:** テスト駆動開発 (TDD)

---

## 📋 概要

Phase 4 では、Employee Context の **Application層（Use Cases）** を実装します。

Domain層（Entity/ValueObject）と Infrastructure層（Repository）が完成している状態で、それらを組み合わせて「ビジネスユースケース」を実装します。

### 実装スコープ

| # | Use Case | 責務 | 入力 | 出力 |
|---|----------|------|------|------|
| 1 | CreateEmployeeUseCase | 新規従業員を作成 | CreateEmployeeRequest | EmployeeDto |
| 2 | GetEmployeeByIdUseCase | ID で従業員を取得 | EmployeeId | EmployeeDto? |
| 3 | GetEmployeesByPersonRowIdUseCase | 人事マスタ行ID で従業員群を取得 | PersonRowId | List\<EmployeeDto\> |
| 4 | UpdateEmployeeUseCase | 従業員情報を更新 | UpdateEmployeeRequest | EmployeeDto |
| 5 | DeleteEmployeeUseCase | 従業員を論理削除 | EmployeeId | void |

---

## 🏗️ アーキテクチャ設計

### 層間の責務分離

```
┌─────────────────────────────────────────┐
│ Presentation Layer                      │
│ (Controllers, ViewModels, Commands)     │
└──────────────┬──────────────────────────┘
               │ (DTO/Request/Response)
┌──────────────▼──────────────────────────┐
│ Application Layer (Use Cases)           │
│ • CreateEmployeeUseCase                 │
│ • GetEmployeeByIdUseCase                │
│ • GetEmployeesByPersonRowIdUseCase      │
│ • UpdateEmployeeUseCase                 │
│ • DeleteEmployeeUseCase                 │
│ • EmployeeService (オーケストレーション)   │
└──────────────┬──────────────────────────┘
               │ (IEmployeeRepository)
┌──────────────▼──────────────────────────┐
│ Domain Layer                            │
│ • Employee, DepartmentMembership, etc.  │
└──────────────┬──────────────────────────┘
               │ (Mapper, DbModel)
┌──────────────▼──────────────────────────┐
│ Infrastructure Layer                    │
│ • EmployeeRepository (実装)             │
│ • EmployeeMapper                        │
└─────────────────────────────────────────┘
```

### 依存関係

**許可:**
- Employee.Application → Employee.Domain ✓
- Employee.Application → SharedKernel ✓
- Employee.Application → Common ✓
- Employee.Application → Application (IUseCase等) ✓
- Employee.Application → IEmployeeRepository (DI) ✓

**禁止:**
- Employee.Application → Employee.Infrastructure ✗（Repositories は Interface経由で DI）

---

## 📝 実装パターン

### Use Case 基本構造

```csharp
// Request (DTO)
public record CreateEmployeeRequest
{
    public required long PersonRowId { get; init; }
    public required string DivisionCode { get; init; }  // M/T/C
    public required int EmployeeNumber { get; init; }
}

// Response (DTO)
public record EmployeeDto
{
    public required Guid Id { get; init; }
    public required long RowId { get; init; }
    public required string Code { get; init; }
    public required long PersonRowId { get; init; }
}

// Use Case
public class CreateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;
    private readonly IClock _clock;

    public CreateEmployeeUseCase(IEmployeeRepository repository, IClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeRequest request)
    {
        // 1. Request 検証
        if (request.PersonRowId <= 0)
            throw new ArgumentException("Invalid PersonRowId");

        // 2. ValueObject に変換
        var division = request.DivisionCode switch
        {
            "M" => EmployeeDivision.RegularEmployee(),
            "T" => EmployeeDivision.Dispatched(),
            "C" => EmployeeDivision.Contractor(),
            _ => throw new ArgumentException($"Invalid division: {request.DivisionCode}")
        };

        var number = EmployeeNumber.From(request.EmployeeNumber);
        var code = EmployeeCode.From(division, number);
        var personRowId = PersonRowId.From(request.PersonRowId);

        // 3. Domain Entity を生成
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(0),  // DB が採番
            code,
            personRowId
        );

        // 4. Repository に保存
        await _repository.AddAsync(employee);

        // 5. Response DTO に変換して返す
        return MapToDto(employee);
    }

    private EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id.Value,
            RowId = employee.RowId.Value,
            Code = employee.Code.ToString(),
            PersonRowId = employee.PersonRowId.Value
        };
    }
}
```

---

## 🧪 テスト戦略

### グループ化戦略

各 Use Case のテストは以下のグループで構成：

| グループ | テスト内容 | 数 |
|---------|----------|-----|
| 正常系 | 有効な入力での処理成功 | 2-3 |
| 異常系 | 無効な入力への例外処理 | 2-3 |
| ビジネスルール | ドメインルール検証 | 1-2 |
| 統合 | Repository との連携 | 1-2 |

### テストファイル構成

```
tests/Contexts/Employee.Application.Tests/
├── UseCases/
│   ├── CreateEmployeeUseCaseTests.cs
│   ├── GetEmployeeByIdUseCaseTests.cs
│   ├── GetEmployeesByPersonRowIdUseCaseTests.cs
│   ├── UpdateEmployeeUseCaseTests.cs
│   └── DeleteEmployeeUseCaseTests.cs
├── Dtos/
│   └── EmployeeDtoMappingTests.cs
└── EmployeeApplicationServiceTests.cs
```

---

## 📚 ドキュメント構成

### Phase 4 ドキュメント（4部構成）

1. **概要書** (`Overvi
ew.md`)
   - Use Cases 一覧
   - 実装スコープ
   - 責務分離
   - 参照資料

2. **技術仕様書** (`技術仕様書.md`)
   - 開発者向けマニュアル
   - 各 Use Case の API 仕様
   - Request/Response DTO 定義
   - 実装パターン
   - エラーハンドリング

3. **詳細設計書** (`詳細設計書.md`)
   - AI実装者向け詳細設計
   - クラス構成
   - メソッド実装指示
   - 依存注入設計
   - Mapper 実装指示

4. **単体テスト仕様書** (`単体テスト仕様書.md`)
   - テスト実装者向け仕様
   - テストケース一覧
   - テストグループ定義
   - Arrange/Act/Assert パターン
   - モック仕様

---

## 🔄 TDD フェーズ

### Phase 4 TDD フローチャート

```
Step 1: ドキュメント作成
  ↓
Step 2: Red フェーズ - テスト作成
  └─ Employee.Application.Tests 作成
  └─ 全テストケース実装（NotImplementedException スタブ）
  ↓
Step 3: Green フェーズ - 実装
  └─ CreateEmployeeUseCase 実装
  └─ GetEmployeeByIdUseCase 実装
  └─ GetEmployeesByPersonRowIdUseCase 実装
  └─ UpdateEmployeeUseCase 実装
  └─ DeleteEmployeeUseCase 実装
  └─ EmployeeDto Mapper 実装
  ↓
Step 4: 全テスト実行 & 検証
  └─ Employee.Application.Tests: ✅ 全成功
  └─ Employee.Domain.Tests: ✅ 全成功（影響なし）
  └─ Employee.Infrastructure.Tests: ✅ 全成功（影響なし）
  └─ Architecture.Tests: ⚠️ 要修正（IEmployeeRepository 配置）
```

---

## 📊 実装チェックリスト

### ドキュメント作成
- [ ] Overview.md 作成
- [ ] 技術仕様書.md 作成
- [ ] 詳細設計書.md 作成
- [ ] 単体テスト仕様書.md 作成

### Red フェーズ
- [ ] Employee.Application.Tests.csproj 作成
- [ ] Employee.Application プロジェクト作成
- [ ] CreateEmployeeUseCaseTests 作成
- [ ] GetEmployeeByIdUseCaseTests 作成
- [ ] GetEmployeesByPersonRowIdUseCaseTests 作成
- [ ] UpdateEmployeeUseCaseTests 作成
- [ ] DeleteEmployeeUseCaseTests 作成
- [ ] EmployeeDtoMappingTests 作成
- [ ] 全テストが「Red」状態を確認

### Green フェーズ
- [ ] CreateEmployeeUseCase 実装
- [ ] GetEmployeeByIdUseCase 実装
- [ ] GetEmployeesByPersonRowIdUseCase 実装
- [ ] UpdateEmployeeUseCase 実装
- [ ] DeleteEmployeeUseCase 実装
- [ ] EmployeeDto 実装
- [ ] Mapper 実装
- [ ] 全テスト成功を確認

### 統合検証
- [ ] 全プロジェクトビルド ✅
- [ ] Employee.Application.Tests ✅
- [ ] Employee.Domain.Tests ✅（影響なし）
- [ ] Employee.Infrastructure.Tests ✅（影響なし）
- [ ] Architecture.Tests: IEmployeeRepository 配置修正

---

## 📍 次のステップ

1. **本ドキュメント** → ドキュメント4部構成の作成
2. **Red フェーズ** → Employee.Application.Tests プロジェクト、全テスト作成
3. **Green フェーズ** → Use Cases 実装
4. **統合検証** → 全テスト実行、アーキテクチャ検証

---

## 👁️ 注意点

### Clean Architecture 準拠

- Use Case は **ビジネスロジック（ドメインルール）を含めない**
- ビジネスロジックは Entity/ValueObject に実装
- Use Case は **オーケストレーション** のみ担当

### 依存注入 (DI)

- Repository は **インターフェース** 経由で注入
- Clock は **インターフェース** 経由で注入
- Infrastructure 実装への直接参照は不可

### エラーハンドリング

- 入力値検証 → ArgumentException（Application責務）
- ビジネスルール違反 → DomainException（Domain責務）
- データアクセス失敗 → 呼び出し元で処理（Repository責務）

---

## 📝 参考資料

- [CLAUDE.md](../../CLAUDE.md) - アーキテクチャ原則
- [Phase 3 Repository計画](../../../docs/Assistance/Plans/Phase3_Repository設計計画.md)
- [Use Case パターンガイド](../../../docs/Assistance/Guides/UseCase_パターンガイド.md)（作成予定）
