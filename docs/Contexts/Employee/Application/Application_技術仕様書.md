# Employee Context Application層 - 技術仕様書

**作成日:** 2026-08-11  
**対象読者:** 開発者  
**内容:** Use Case API仕様、DTO定義、実装パターン

---

## 📖 概要

本書は、Employee Context の各 Use Case の **API仕様** と **実装パターン** を定義します。

開発者は本書を参照して、Use Case の入出力仕様、エラーハンドリング、ビジネスロジックの流れを理解します。

---

## 🎯 Use Cases API 仕様

### 1. CreateEmployeeUseCase

**責務:** 新規従業員を作成して DB に保存

#### Request

```csharp
public record CreateEmployeeRequest
{
    /// <summary>
    /// 従業員の人事マスタ行ID
    /// 【値域】1以上
    /// 【制約】必須、既存 m_persons.row_id への外部参照
    /// </summary>
    public required long PersonRowId { get; init; }

    /// <summary>
    /// 従業員区分コード（M/T/C のいずれか）
    /// 【値域】M=正社員, T=派遣, C=契約
    /// 【制約】必須、正確な大文字1文字
    /// </summary>
    public required string DivisionCode { get; init; }

    /// <summary>
    /// 従業員番号（1001以上9999以下）
    /// 【値域】1001-9999
    /// 【制約】必須、同一部門内で一意
    /// </summary>
    public required int EmployeeNumber { get; init; }
}
```

#### Response

```csharp
public record EmployeeDto
{
    /// <summary>
    /// 従業員集約根ID（GUID）
    /// 【対応カラム】employee_id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// データベース行ID
    /// 【対応カラム】row_id
    /// </summary>
    public required long RowId { get; init; }

    /// <summary>
    /// 従業員コード（DDD/NNNN形式）
    /// 【例】M/1234
    /// 【対応カラム】employee_code_division + employee_code_number
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// 人事マスタ行ID
    /// 【対応カラム】person_row_id
    /// </summary>
    public required long PersonRowId { get; init; }

    /// <summary>
    /// 作成日時（JST）
    /// 【対応カラム】created_at
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
```

#### エラーハンドリング

| エラー | 条件 | 例外型 | HTTP |
|------|------|--------|------|
| 無効な PersonRowId | PersonRowId <= 0 | ArgumentException | 400 |
| 無効な DivisionCode | "M", "T", "C" 以外 | ArgumentException | 400 |
| 無効な EmployeeNumber | 1000以下または10000以上 | ArgumentException | 400 |
| 重複チェック失敗 | (今後実装予定) | BusinessRuleException | 409 |

#### 実装フロー

```
1. Request 検証
   ├─ PersonRowId > 0 ？
   ├─ DivisionCode in ["M", "T", "C"] ？
   └─ EmployeeNumber in [1001, 9999] ？
      └─ ✗ → ArgumentException 投げ

2. ValueObject 生成
   ├─ EmployeeDivision.From(DivisionCode)
   ├─ EmployeeNumber.From(EmployeeNumber)
   ├─ EmployeeCode.From(division, number)
   └─ PersonRowId.From(PersonRowId)

3. Domain Entity 生成
   └─ Employee.Create(id, rowId, code, personRowId)

4. Repository 保存
   └─ await _repository.AddAsync(employee)

5. Response 変換
   └─ MapToDto(employee) → EmployeeDto

6. 返却
   └─ return dto
```

---

### 2. GetEmployeeByIdUseCase

**責務:** ID で従業員を取得

#### Request

```csharp
public record GetEmployeeByIdRequest
{
    /// <summary>
    /// 従業員集約根ID
    /// 【値域】有効な GUID
    /// 【制約】必須
    /// </summary>
    public required Guid EmployeeId { get; init; }
}
```

#### Response

```csharp
public record GetEmployeeByIdResponse
{
    /// <summary>
    /// 見つかった従業員（見つからない場合は null）
    /// </summary>
    public EmployeeDto? Employee { get; init; }
}
```

#### エラーハンドリング

| エラー | 条件 | 例外型 | 備考 |
|------|------|--------|------|
| 見つからない | ID が存在しない | null 返却 | 404 ではなく null |
| 削除済み | deleted_at != null | null 返却 | 論理削除は見えない |

#### 実装フロー

```
1. Request 検証（必要に応じて）
   └─ EmployeeId の形式チェック

2. Repository 検索
   └─ await _repository.GetByIdAsync(id)

3. 結果判定
   ├─ null → null 返却
   └─ Employee → MapToDto() → EmployeeDto 返却
```

---

### 3. GetEmployeesByPersonRowIdUseCase

**責務:** 人事マスタ行ID で従業員群を取得（複数結果）

#### Request

```csharp
public record GetEmployeesByPersonRowIdRequest
{
    /// <summary>
    /// 人事マスタ行ID
    /// 【値域】1以上
    /// 【制約】必須
    /// </summary>
    public required long PersonRowId { get; init; }
}
```

#### Response

```csharp
public record GetEmployeesByPersonRowIdResponse
{
    /// <summary>
    /// 検索結果（複数あり得る）
    /// 【特性】IReadOnlyList（変更不可）
    /// </summary>
    public required IReadOnlyList<EmployeeDto> Employees { get; init; }
}
```

#### エラーハンドリング

| エラー | 条件 | 例外型 |
|------|------|--------|
| 無効な PersonRowId | PersonRowId <= 0 | ArgumentException |
| 見つからない | 結果なし | 空リスト返却 |

#### 実装フロー

```
1. Request 検証
   └─ PersonRowId > 0 ？
      └─ ✗ → ArgumentException 投げ

2. Repository 検索
   └─ await _repository.GetByPersonRowIdAsync(id)

3. 結果変換
   └─ entities.Select(MapToDto).ToList()

4. 返却
   └─ return new List<EmployeeDto>() or results
```

---

### 4. UpdateEmployeeUseCase

**責務:** 従業員情報を更新

#### Request

```csharp
public record UpdateEmployeeRequest
{
    /// <summary>
    /// 更新対象の従業員ID
    /// 【制約】必須、既存ID
    /// </summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>
    /// 新しい部門コード
    /// 【制約】オプション、未指定の場合は変更なし
    /// </summary>
    public string? DivisionCode { get; init; }

    /// <summary>
    /// 新しい従業員番号
    /// 【制約】オプション、未指定の場合は変更なし
    /// </summary>
    public int? EmployeeNumber { get; init; }

    /// <summary>
    /// 楽観ロック用 RowVersion
    /// 【制約】必須（競合検出用）
    /// </summary>
    public required byte[]? RowVersion { get; init; }
}
```

#### Response

```csharp
public record UpdateEmployeeResponse
{
    /// <summary>
    /// 更新後の従業員情報
    /// </summary>
    public required EmployeeDto Employee { get; init; }
}
```

#### エラーハンドリング

| エラー | 条件 | 例外型 | 備考 |
|------|------|--------|------|
| 見つからない | ID が存在しない | EntityNotFoundException | 404 |
| 楽観ロック競合 | RowVersion 不一致 | OptimisticLockException | 409 |
| 無効な値 | DivisionCode/EmployeeNumber が無効 | ArgumentException | 400 |

#### 実装フロー

```
1. Request 検証
   ├─ EmployeeId != Guid.Empty ？
   └─ DivisionCode 指定時: "M"/"T"/"C" ？

2. Entity 取得
   ├─ await _repository.GetByIdAsync(id)
   └─ null → EntityNotFoundException 投げ

3. 更新内容適用（必要なフィールドのみ）
   ├─ DivisionCode 指定時
   │  └─ entity.ChangeCode(newDivision, newNumber)
   └─ (メソッド未実装 → Entity設計時に追加)

4. Repository 更新
   └─ await _repository.UpdateAsync(entity)
      └─ RowVersion 不一致 → OptimisticLockException

5. Response 変換
   └─ MapToDto(entity) → EmployeeDto

6. 返却
```

---

### 5. DeleteEmployeeUseCase

**責務:** 従業員を論理削除

#### Request

```csharp
public record DeleteEmployeeRequest
{
    /// <summary>
    /// 削除対象の従業員ID
    /// 【制約】必須、既存ID
    /// </summary>
    public required Guid EmployeeId { get; init; }
}
```

#### Response

```csharp
// 削除成功時は void または success flag
public record DeleteEmployeeResponse
{
    public required bool Success { get; init; }
}
```

#### エラーハンドリング

| エラー | 条件 | 例外型 |
|------|------|--------|
| 見つからない | ID が存在しない | EntityNotFoundException |
| 既に削除済み | deleted_at != null | AlreadyDeletedException |

#### 実装フロー

```
1. Repository で削除
   ├─ await _repository.DeleteAsync(employeeId)
   └─ (Repository が論理削除を実装)

2. 削除成功確認
   └─ return { Success = true }
```

---

## 🔧 実装パターン

### Use Case の標準実装形式

```csharp
public class CreateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;
    private readonly IClock _clock;

    // 【DI】コンストラクタ注入
    public CreateEmployeeUseCase(
        IEmployeeRepository repository,
        IClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    // 【Entry Point】Execute メソッド
    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeRequest request)
    {
        // Step 1: 入力検証
        ValidateRequest(request);

        // Step 2: ValueObject 生成
        var division = ParseDivision(request.DivisionCode);
        var number = EmployeeNumber.From(request.EmployeeNumber);
        var code = EmployeeCode.From(division, number);
        var personRowId = PersonRowId.From(request.PersonRowId);

        // Step 3: Domain Entity 生成
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(0),
            code,
            personRowId
        );

        // Step 4: Repository 操作
        await _repository.AddAsync(employee);

        // Step 5: Response 変換
        return MapToDto(employee);
    }

    private void ValidateRequest(CreateEmployeeRequest request)
    {
        if (request.PersonRowId <= 0)
            throw new ArgumentException("PersonRowId must be > 0");
        
        if (!IsValidDivisionCode(request.DivisionCode))
            throw new ArgumentException("Invalid DivisionCode");
        
        if (request.EmployeeNumber < 1001 || request.EmployeeNumber > 9999)
            throw new ArgumentException("EmployeeNumber out of range");
    }

    private EmployeeDivision ParseDivision(string code)
    {
        return code switch
        {
            "M" => EmployeeDivision.RegularEmployee(),
            "T" => EmployeeDivision.Dispatched(),
            "C" => EmployeeDivision.Contractor(),
            _ => throw new ArgumentException($"Invalid division: {code}")
        };
    }

    private bool IsValidDivisionCode(string? code)
    {
        return code is "M" or "T" or "C";
    }

    private EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id.Value,
            RowId = employee.RowId.Value,
            Code = employee.Code.ToString(),
            PersonRowId = employee.PersonRowId.Value,
            CreatedAt = employee.CreatedAt.Value  // TODO: Entity に CreatedAt 追加
        };
    }
}
```

### Exception クラス設計

```csharp
// Base
public class EmployeeApplicationException : Exception
{
    public EmployeeApplicationException(string message) : base(message) { }
}

// Specific
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

## 📐 DTO 設計

### EmployeeDto

```csharp
public record EmployeeDto
{
    /// <summary>
    /// 従業員ID（集約根）
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// DB行ID
    /// </summary>
    public required long RowId { get; init; }

    /// <summary>
    /// 従業員コード（"M/1234" 形式）
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// 人事マスタ行ID
    /// </summary>
    public required long PersonRowId { get; init; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
```

### Mapper 実装

```csharp
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
            CreatedAt = employee.CreatedAt.Value
        };
    }
}
```

---

## 🔐 セキュリティ考慮

### 入力検証

- すべての外部入力を **Application層で検証**
- Domain層では **ビジネスロジックのみ** を検証
- Repository経由の取得データは信頼できるものとして扱う

### 認可

- Use Case は認可チェックを **実装しない**（Presentation層で実施）
- Application層は **認可済み** ユーザーの操作のみを想定

### 監査カラム

- createdAt/createdBy は Repository で自動設定
- updatedAt/updatedBy は Repository で自動設定
- deletedAt/deletedBy は Repository で自動設定

---

## 🎓 参考資料

- [Application_概要.md](Application_概要.md) - 実装スコープ、プロジェクト構成
- [Entity_設計ガイドライン.md](../../../Assistance/Guides/Entity_設計ガイドライン.md) - Entity 設計
- [ドメインイベント_設計ガイド.md](../../../Assistance/Guides/ドメインイベント_設計ガイド.md) - イベント処理

---

**次のドキュメント:** [Application_詳細設計書.md](Application_詳細設計書.md)
