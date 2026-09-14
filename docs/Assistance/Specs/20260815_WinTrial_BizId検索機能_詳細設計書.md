# WinTrial BizId検索機能 詳細設計書

**作成日**: 2026-08-15  
**バージョン**: 1.0  
**対象プロジェクト**: SupportAdvance.Presentation.WinTrial  
**対象Context**: Employee  

---

## 📋 目次

1. [概要](#概要)
2. [ユースケース](#ユースケース)
3. [アーキテクチャ設計](#アーキテクチャ設計)
4. [各層の実装詳細](#各層の実装詳細)
5. [データ設計](#データ設計)
6. [画面設計](#画面設計)
7. [依存関係](#依存関係)
8. [実装チェックリスト](#実装チェックリスト)
9. [エラーハンドリング](#エラーハンドリング)

---

## 概要

### 機能概要

WinTrial（Windows Forms）のForm1で、EmployeeBizId（従業員番号）を入力し、検索ボタンをクリックすることで、該当従業員の姓名をラベルに表示する機能。

### 非機能要件

- **アーキテクチャ**: クリーンアーキテクチャ準拠
- **フレームワーク**: MVVMToolkit による ViewModel パターン
- **DB**: SQL Server 実データベース（t_employees テーブル）
- **言語**: 日本語ログ・エラーメッセージ
- **応答時間**: < 1秒（DB検索）

### 前提条件

- `Employee.Domain`, `Employee.Application`, `Employee.Infrastructure` が実装済み
- `EmployeeDto` に `BizId`, `PersonLastName`, `PersonFirstName` プロパティ が存在
- DI コンテナ（ServiceCollection）が Program.cs で構成済み
- Form1ViewModel が MVVMToolkit で実装済み

---

## ユースケース

### UC-001: BizIdから従業員検索

| 項目 | 説明 |
|------|------|
| **アクター** | ユーザー（WinTrial 利用者） |
| **前提条件** | Form1 が起動している |
| **フロー** | 1. textBoxExt1 に BizId（例：EMP001）を入力<br/>2. sfButton2「検索」クリック<br/>3. DB から従業員データを検索<br/>4. label1 に「姓 名」を表示 |
| **代替フロー** | BizId が未入力 → エラーメッセージ表示<br/>BizId が見つからない → "見つかりません" と表示 |
| **後処理** | ログにBizId検索を記録 |

---

## アーキテクチャ設計

### 層別フロー図

```
┌──────────────────────────────────────────────────────────────┐
│ PRESENTATION LAYER (WinTrial)                                │
├──────────────────────────────────────────────────────────────┤
│  Form1 (View) → Form1ViewModel (MVVM Binding)               │
│  ├─ textBoxExt1 {Binding: BizIdSearchInput}                 │
│  ├─ sfButton2 {Command: SearchEmployeeByBizIdCommand}       │
│  └─ label1 {Binding: EmployeeFullName}                      │
│                                                              │
└──────────────────────┬───────────────────────────────────────┘
                       │ DI injection
                       ↓
┌──────────────────────────────────────────────────────────────┐
│ APPLICATION LAYER (Employee.Application)                     │
├──────────────────────────────────────────────────────────────┤
│  GetEmployeeByBizIdUseCase                                   │
│  ├─ ExecuteAsync(string bizId): Task<EmployeeDto?>         │
│  └─ IEmployeeRepository _repository (DI)                    │
│                                                              │
└──────────────────────┬───────────────────────────────────────┘
                       │ interface call
                       ↓
┌──────────────────────────────────────────────────────────────┐
│ INFRASTRUCTURE LAYER (Employee.Infrastructure)               │
├──────────────────────────────────────────────────────────────┤
│  EmployeeRepository : IEmployeeRepository                    │
│  ├─ GetByBizIdAsync(int bizId): Task<Employee?>            │
│  │  ├─ SQL: 3-table JOIN (m_employees, m_employee_attr...) │
│  │  └─ Dapper: QueryFirstOrDefaultAsync<EmployeeDbModel>  │
│  └─ DbModel → Entity 変換（Mapper）                        │
│                                                              │
└──────────────────────┬───────────────────────────────────────┘
                       │ SQL query
                       ↓
┌──────────────────────────────────────────────────────────────┐
│ DATABASE LAYER (SQL Server)                                  │
├──────────────────────────────────────────────────────────────┤
│  m_employees (マスタ)                                         │
│  ├─ row_id (PK)                                             │
│  ├─ biz_id (INDEX) ← 検索キー                               │
│  ├─ employee_division                                       │
│  └─ retired_on                                              │
│                                                              │
│  m_employee_attributes (属性マッピング)                      │
│  ├─ employee__row_id → m_employees                          │
│  ├─ attribute_type = 'person'                               │
│  └─ attribute_row_id → m_persons                            │
│                                                              │
│  m_persons (人事マスタ)                                       │
│  ├─ row_id (PK)                                             │
│  ├─ last_name, first_name                                   │
│  └─ last_name_kana, first_name_kana                         │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

### 依存関係マトリックス

| From | To | 許可 | 方法 |
|------|-----|------|------|
| Form1 | Form1ViewModel | ✅ | Ctor DI |
| Form1ViewModel | GetEmployeeByBizIdUseCase | ✅ | Ctor DI |
| GetEmployeeByBizIdUseCase | IEmployeeRepository | ✅ | Ctor DI |
| GetEmployeeByBizIdUseCase | Employee (Domain) | ✅ | Entity 参照 |
| EmployeeRepository | EmployeeDbModel | ✅ | Infrastructure |
| EmployeeRepository | Mapper | ✅ | Infrastructure |
| Form1ViewModel | Infrastructure | ❌ | Program.cs のみ |

---

## 各層の実装詳細

### 1️⃣ PRESENTATION LAYER

#### 1.1 Form1.cs （View コードビハインド）

**現在の状態**:
```csharp
public partial class Form1 : Form
{
    private readonly Form1ViewModel _viewModel;
    
    public Form1(Form1ViewModel viewModel, ...)
    {
        InitializeComponent();
        _viewModel = viewModel;
        sfButton1.Command = _viewModel.ExecuteSampleUseCaseCommand; // 既存
    }
}
```

**必要な追加実装**:
```csharp
// 1. Data Binding 設定（InitializeComponent の後）
this.textBoxExt1.DataBindings.Add("Text", _viewModel, 
    nameof(Form1ViewModel.BizIdSearchInput), true, 
    DataSourceUpdateMode.OnPropertyChanged);

this.label1.DataBindings.Add("Text", _viewModel, 
    nameof(Form1ViewModel.EmployeeFullName), true, 
    DataSourceUpdateMode.Never);

// 2. Button Command Binding
sfButton2.Command = _viewModel.SearchEmployeeByBizIdCommand;
```

**責務**:
- UI イベント → ViewModel コマンド のマッピング
- Data Binding の設定（双方向）
- DI から ViewModel を注入

#### 1.2 Form1ViewModel.cs

**現在の状態**:
```csharp
public partial class Form1ViewModel : ObservableObject
{
    private readonly IAppLogging<Form1ViewModel> _logger;
    // 他のフィールド...
    
    [RelayCommand]
    public void ExecuteSampleUseCase() { ... }
}
```

**必要な追加実装**:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.WinTrial.ViewModels;

public partial class Form1ViewModel : ObservableObject
{
    private readonly GetEmployeeByBizIdUseCase _getEmployeeByBizIdUseCase;
    private readonly IAppLogging<Form1ViewModel> _logger;

    // =============== Observable Properties ===============
    
    /// <summary>
    /// 検索入力用 BizId
    /// </summary>
    [ObservableProperty]
    private string bizIdSearchInput = string.Empty;

    /// <summary>
    /// 検索結果の従業員フルネーム（姓 + 名）
    /// </summary>
    [ObservableProperty]
    private string employeeFullName = string.Empty;

    // =============== Constructor ===============

    public Form1ViewModel(
        GetEmployeeByBizIdUseCase getEmployeeByBizIdUseCase,
        IAppLogging<Form1ViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(getEmployeeByBizIdUseCase);
        ArgumentNullException.ThrowIfNull(logger);

        _getEmployeeByBizIdUseCase = getEmployeeByBizIdUseCase;
        _logger = logger;
        
        _logger.LogInformation("Form1ViewModel initialized.");
    }

    // =============== Commands ===============

    /// <summary>
    /// BizId から従業員を検索するコマンド
    /// </summary>
    [RelayCommand]
    public async Task SearchEmployeeByBizId()
    {
        try
        {
            // 1. 入力検証
            if (string.IsNullOrWhiteSpace(BizIdSearchInput))
            {
                EmployeeFullName = "BizId を入力してください";
                return;
            }

            // 2. Use Case 実行
            var employeeDto = await _getEmployeeByBizIdUseCase.ExecuteAsync(
                BizIdSearchInput.Trim());

            // 3. 結果表示
            if (employeeDto == null)
            {
                EmployeeFullName = "見つかりません";
                _logger.LogWarning(
                    $"Employee not found for BizId: {BizIdSearchInput}");
                return;
            }

            // 4. フルネーム設定
            EmployeeFullName = 
                $"{employeeDto.PersonLastName} {employeeDto.PersonFirstName}";
            
            _logger.LogInformation(
                $"Employee found - BizId: {employeeDto.BizId}, " +
                $"Name: {EmployeeFullName}");
        }
        catch (ArgumentException ex)
        {
            EmployeeFullName = $"エラー: 入力が不正です";
            _logger.LogError("Invalid input", ex);
        }
        catch (Exception ex)
        {
            EmployeeFullName = $"エラー: {ex.Message}";
            _logger.LogError("SearchEmployeeByBizId execution failed", ex);
        }
    }
}
```

**実装ポイント**:
- `[ObservableProperty]` で自動的に PropertyChanged イベント生成
- `[RelayCommand]` で SearchEmployeeByBizIdCommand を自動生成
- 非同期処理（async/await）で UI ブロッキング回避
- 入力検証とエラーハンドリング

---

### 2️⃣ APPLICATION LAYER

#### 2.1 IEmployeeRepository.cs に メソッド追加

**現在のインターフェース**:
```csharp
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(EmployeeRowId id);
    Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId);
    Task<IReadOnlyList<Employee>> GetByPersonRowIdAsync(PersonRowId personRowId);
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(EmployeeRowId id);
}
```

**追加するメソッド**:
```csharp
/// <summary>
/// BizId（従業員番号）で Employee を検索する
/// </summary>
/// <param name="bizId">t_employees.biz_id</param>
/// <returns>見つかった Employee インスタンス、または null</returns>
Task<Employee?> GetByBizIdAsync(string bizId);
```

#### 2.2 新規ファイル: GetEmployeeByBizIdUseCase.cs

**ファイルパス**: `src/Contexts/Employee/Employee.Application/UseCases/GetEmployeeByBizIdUseCase.cs`

```csharp
namespace SupportAdvance.Contexts.Employee.Application.UseCases;

using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Application.Repositories;

/// <summary>
/// BizId（従業員番号）で従業員を取得する Use Case
/// </summary>
public class GetEmployeeByBizIdUseCase
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeeByBizIdUseCase(IEmployeeRepository repository)
    {
        _repository = repository ?? 
            throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 従業員を BizId で検索
    /// </summary>
    /// <param name="bizId">従業員番号（例: "EMP001"）</param>
    /// <returns>見つかった従業員の DTO、または null</returns>
    /// <exception cref="ArgumentException">bizId が空の場合</exception>
    public async Task<EmployeeDto?> ExecuteAsync(string bizId)
    {
        if (string.IsNullOrWhiteSpace(bizId))
            throw new ArgumentException(
                "BizId must not be empty", nameof(bizId));

        var employee = await _repository.GetByBizIdAsync(bizId.Trim());

        return employee?.ToDto();
    }
}
```

**責務**:
- 入力検証（BizId が空でないか）
- Repository 経由で Entity を検索
- Entity → Dto の変換（`ToDto()` extension）
- 検索結果を返す

---

### 3️⃣ INFRASTRUCTURE LAYER

#### 3.1 EmployeeRepository.cs に メソッド実装

**ファイルパス**: `src/Contexts/Employee/Employee.Infrastructure/Repositories/EmployeeRepository.cs`

**追加するメソッド実装**:

```csharp
/// <summary>
/// BizId で従業員を検索する
/// </summary>
public async Task<Employee?> GetByBizIdAsync(string bizId)
{
    if (string.IsNullOrWhiteSpace(bizId))
        throw new ArgumentException("BizId must not be empty", nameof(bizId));

    // SQL: m_employee_attributes経由で m_persons のデータを取得
    const string sql = @"
        SELECT 
            e.[row_id],
            e.[employee_division],
            e.[biz_id],
            e.[retired_on]
        FROM [dbo].[m_employees] e
        INNER JOIN [dbo].[m_employee_attributes] ea 
            ON e.[row_id] = ea.[employee__row_id]
            AND ea.[attribute_type] = 'person'
        WHERE e.[biz_id] = @BizId
            AND e.[deleted_at] IS NULL
            AND ea.[deleted_at] IS NULL";

    using (var connection = _connectionFactory.CreateConnection())
    {
        var dbModel = await connection.QueryFirstOrDefaultAsync<EmployeeDbModel>(
            sql,
            new { BizId = int.Parse(bizId.Trim()) });

        if (dbModel == null)
            return null;

        // DbModel → Entity 変換（属性テーブルは別途で取得）
        return Mapper.ToDomainEntity(dbModel, _clock);
    }
}
```

**実装ポイント**:
- `m_employee_attributes` を `attribute_type='person'` で JOIN
- 属性テーブルから取得する PersonRowId, DepartmentMemberships は別メソッドで処理
- `deleted_at IS NULL` で論理削除済みレコードを除外（両テーブル）
- Dapper の `QueryFirstOrDefaultAsync<T>()` で単件取得
- Mapper で DbModel → Entity に変換

#### 3.2 EmployeeDbModel の設計

**責務**: Infrastructure層の DB マッピングモデル（ビジネスカラムのみ）

```csharp
/// <summary>
/// Employee テーブルマッピングモデル
/// 【テーブル】m_employees
/// 【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持
/// 【特徴】ビジネスカラムのみ（監査カラムは Repository で自動管理）
/// </summary>
public class EmployeeDbModel
{
    /// <summary>
    /// 行ID（主キー、Sequence自動採番）
    /// </summary>
    [Column("row_id")]
    public long RowId { get; set; }

    /// <summary>
    /// 従業員区分（M/D/C のいずれか）
    /// </summary>
    [Column("employee_division")]
    public string EmployeeDivision { get; set; } = string.Empty;

    /// <summary>
    /// ビジネスID（従業員番号、1001以上）
    /// </summary>
    [Column("biz_id")]
    public int BizId { get; set; }

    /// <summary>
    /// 退職日（現職時は NULL）
    /// </summary>
    [Column("retired_on")]
    public DateTime? RetiredOn { get; set; }
}
```

**注記**:
- 監査カラム（created_at, updated_at, deleted_at など）は **DbModel に持たない**
- Repository が SQL 実行時に DB から取得・管理
- PersonRowId, DepartmentMemberships は **属性テーブル経由**で取得

---

## データ設計

### SQL クエリ設計

#### クエリA: BizId 検索

```sql
SELECT 
    e.[biz_id],
    e.[row_id],
    p.[last_name],
    p.[first_name],
    p.[last_name_kana],
    p.[first_name_kana]
FROM [dbo].[m_employees] e
INNER JOIN [dbo].[m_employee_attributes] ea 
    ON e.[row_id] = ea.[employee__row_id]
    AND ea.[attribute_type] = 'person'
INNER JOIN [dbo].[m_persons] p 
    ON ea.[attribute_row_id] = p.[row_id]
WHERE e.[biz_id] = @BizId
    AND e.[deleted_at] IS NULL
    AND ea.[deleted_at] IS NULL;
```

**注記**：
- `m_employees` から `biz_id` で検索
- `m_employee_attributes` で attribute_type='person' として m_persons に接続
- 論理削除済みレコード（deleted_at IS NOT NULL）を除外

**パフォーマンス検討**:
- `m_employees` に `biz_id` カラムの INDEX が必要
  ```sql
  CREATE NONCLUSTERED INDEX IX_m_employees_biz_id 
  ON [dbo].[m_employees]([biz_id]) 
  WHERE [deleted_at] IS NULL;
  ```
- `m_employee_attributes` に複合 INDEX が推奨
  ```sql
  CREATE NONCLUSTERED INDEX IX_m_employee_attributes_lookup
  ON [dbo].[m_employee_attributes]([employee__row_id], [attribute_type])
  WHERE [deleted_at] IS NULL;
  ```
- 結果は最大1件（BizId は一意性を想定）
- 応答時間: < 10ms（INDEX 使用時、3-table JOIN）

### DTO データフロー

```
EmployeeDbModel (DB行単位)
  ├─ row_id: 1
  ├─ biz_id: "EMP001"
  ├─ person_row_id: 100
  ├─ last_name: "山田"
  └─ first_name: "太郎"
          ↓ Mapper.ToDto()
EmployeeDto (Presentation 層)
  ├─ RowId: 1
  ├─ BizId: "EMP001"
  ├─ PersonRowId: 100
  ├─ PersonLastName: "山田"
  └─ PersonFirstName: "太郎"
          ↓ ViewModel
ViewModel.EmployeeFullName = "山田 太郎"
          ↓ UI Binding
label1.Text = "山田 太郎"
```

---

## 画面設計

### WinTrial Form1 レイアウト

| コントロール | 種類 | プロパティ | バインディング |
|-----------|------|----------|--------------|
| `textBoxExt1` | TextBoxExt | Location(31, 94)<br/>Size(100, 23) | Text → BizIdSearchInput |
| `sfButton2` | SfButton | Location(137, 89)<br/>Text="検索" | Command → SearchEmployeeByBizIdCommand |
| `label1` | Label | Location(246, 99)<br/>AutoSize=true | Text ← EmployeeFullName |

### ユーザーインタラクション フロー

```
[ユーザー] textBoxExt1 に「EMP001」を入力
    ↓
[Form1] textBoxExt1.TextChanged イベント
    ↓
[Binding] BizIdSearchInput プロパティを更新
    ↓
[ユーザー] sfButton2 をクリック
    ↓
[Command] SearchEmployeeByBizIdCommand.Execute()
    ↓
[ViewModel] SearchEmployeeByBizId() メソッド実行
    ↓
[Use Case] GetEmployeeByBizIdUseCase.ExecuteAsync("EMP001")
    ↓
[DB] SQL 検索実行
    ↓
[Result] EmployeeDto を取得
    ↓
[ViewModel] EmployeeFullName = "山田 太郎"
    ↓
[Binding] EmployeeFullName プロパティ変更通知
    ↓
[Form1] label1.Text = "山田 太郎"
    ↓
[表示] ユーザーに結果を表示
```

### エラーハンドリング画面

| 入力 | 表示結果 |
|------|--------|
| （空白） | "BizId を入力してください" |
| "INVALID" | "見つかりません" |
| DB接続エラー | "エラー: {error message}" |

---

## 依存関係

### DI 登録アーキテクチャ

DI 登録は **層別の責務分離パターン** に従い、各層の `DependencyInjection.cs` で実装されます。

```
Program.cs (Composition Root)
  ├─ AddCrosscuttingModels()
  ├─ AddInfrastructureModels()
  │  └─ AddEmployeeInfrastructureModels()
  ├─ AddApplicationModels()
  │  └─ AddEmployeeApplicationModels()
  └─ AddWinTrialModules()
```

### 🔧 DI 実装ファイル一覧

| ファイル | 層 | 責務 | 登録内容 |
|---------|----|----|--------|
| `src/Infrastructure/DependencyInjection.cs` | 汎用Infrastructure | DB, Clock, Settings | SequenceProvider, IClock, IAppSettings |
| **`src/Infrastructure/Employee/DependencyInjection.cs`** | **Employee Infrastructure** | **Repository** | **IEmployeeRepository → EmployeeRepository** |
| `src/Application/DependencyInjection.cs` | 汎用Application | Use Case オーケストレーション | AddEmployeeApplicationModels() 呼び出し |
| **`src/Contexts/Employee/Employee.Application/DependencyInjection.cs`** | **Employee Application** | **Use Cases** | **GetEmployeeByBizIdUseCase, 他 Use Cases** |
| `src/Presentation/WinTrial/DependencyInjection.cs` | Presentation | ViewModel, View | Form1ViewModel, Form1 |

### 📝 DI 実装コード

#### 1️⃣ src/Infrastructure/Employee/DependencyInjection.cs (新規作成)

**ファイルパス**: `src/Contexts/Employee/Employee.Infrastructure/DependencyInjection.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Contexts.Employee.Infrastructure.Repositories;

namespace SupportAdvance.Contexts.Employee.Infrastructure;

/// <summary>
/// Employee Context の Infrastructure サービス登録
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Employee Context の Repository を DI に登録
    /// </summary>
    public static IServiceCollection AddEmployeeInfrastructureModels(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Repository を登録（インターフェース型で登録し、実装を隠蔽）
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        return services;
    }
}
```

#### 2️⃣ src/Contexts/Employee/Employee.Application/DependencyInjection.cs (新規作成)

**ファイルパス**: `src/Contexts/Employee/Employee.Application/DependencyInjection.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Employee.Application.UseCases;

namespace SupportAdvance.Contexts.Employee.Application;

/// <summary>
/// Employee Context の Application サービス登録
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Employee Context の Use Cases を DI に登録
    /// </summary>
    public static IServiceCollection AddEmployeeApplicationModels(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Get Use Cases
        services.AddScoped<GetEmployeeByIdUseCase>();
        services.AddScoped<GetEmployeeByBizIdUseCase>();        // ← 新規
        services.AddScoped<GetEmployeesByPersonRowIdUseCase>();

        // Create/Update/Delete Use Cases
        services.AddScoped<CreateEmployeeUseCase>();
        services.AddScoped<UpdateEmployeeUseCase>();
        services.AddScoped<DeleteEmployeeUseCase>();

        return services;
    }
}
```

#### 3️⃣ src/Application/DependencyInjection.cs (修正)

**修正箇所**: `AddApplicationModels()` メソッド

```csharp
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Employee.Application;

namespace SupportAdvance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // 各 Bounded Context の Application Services を登録
        services.AddEmployeeApplicationModels();      // ← 追加

        return services;
    }
}
```

#### 4️⃣ src/Infrastructure/DependencyInjection.cs (修正)

**修正箇所**: `AddInfrastructureModels()` メソッド内

```csharp
using SupportAdvance.Contexts.Employee.Infrastructure;

public static IServiceCollection AddInfrastructureModels(
    this IServiceCollection services,
    IConfiguration configuration)
{
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    // ステップ0a: Dapper 初期化
    DapperTypeHandlerRegistration.Register();

    // ステップ0b: RepoDb 初期化
    RepoDbTypeMapperRegistration.Register();

    // ... 既存の設定コード ...

    // ステップ7: 各 Context の Infrastructure を登録
    services.AddEmployeeInfrastructureModels();      // ← 追加

    return services;
}
```

#### 5️⃣ src/Presentation/WinTrial/DependencyInjection.cs (修正)

**修正箇所**: `AddWinTrialModules()` メソッド

```csharp
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Presentation.WinTrial.ViewModels;
using SupportAdvance.Presentation.WinTrial.Views;

namespace SupportAdvance.Presentation.WinTrial;

public static class DependencyInjection
{
    public static IServiceCollection AddWinTrialModules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Use Cases（Presentation が直接使用する場合）
        services.AddScoped<GetEmployeeByBizIdUseCase>();

        // ViewModels（ViewModel に Use Case を DI する）
        services.AddScoped<Form1ViewModel>();

        // Views（Form に ViewModel を DI する）
        services.AddScoped<Form1>();

        return services;
    }
}
```

### DI チェーン図（実行順序）

```
Program.Main()
  ↓
HostBuilderFactory.Create()
  ↓
IServiceCollection の構築
  ├─ AddCrosscuttingModels()
  │  └─ Logging, Audit などの共通サービス
  │
  ├─ AddInfrastructureModels()
  │  ├─ Clock, Settings, SequenceProvider
  │  └─ AddEmployeeInfrastructureModels()
  │     └─ IEmployeeRepository → EmployeeRepository
  │
  ├─ AddApplicationModels()
  │  └─ AddEmployeeApplicationModels()
  │     ├─ GetEmployeeByBizIdUseCase
  │     ├─ GetEmployeeByIdUseCase
  │     ├─ CreateEmployeeUseCase
  │     └─ ... (他の Use Cases)
  │
  └─ AddWinTrialModules()
     ├─ GetEmployeeByBizIdUseCase (重複登録OK)
     ├─ Form1ViewModel
     │  └─ コンストラクタで GetEmployeeByBizIdUseCase を DI
     │     └─ GetEmployeeByBizIdUseCase は IEmployeeRepository を DI
     │        └─ IEmployeeRepository = EmployeeRepository
     └─ Form1
        └─ コンストラクタで Form1ViewModel を DI

User creates Form1 instance
  ↓
ServiceProvider.GetRequiredService<Form1>()
  ↓
Form1ViewModel instance created
  ├─ GetEmployeeByBizIdUseCase injected
  │  └─ IEmployeeRepository (= EmployeeRepository) injected
  │     └─ IConnectionFactory, IClock などから Connection 取得
  └─ IAppLogging<Form1ViewModel> injected
```

### プロジェクト参照

| プロジェクト | 参照元 | 参照先 | 方法 |
|-----------|------|------|------|
| WinTrial | Presentation | Employee.Application | ProjectReference |
| Employee.Application | Application (Context) | IEmployeeRepository | インターフェース参照 |
| Employee.Infrastructure | Infrastructure | Employee.Application | インターフェースのみ |
| Program.cs | Presentation | Infrastructure | ProjectReference（DI のみ） |
| Form1ViewModel | Presentation | Employee.Application | Ctor DI |

### 🔄 DI 解決フロー例（BizId検索時）

```
1. User clicks sfButton2
   ↓
2. Form1.sfButton2_Click()
   ↓
3. ViewModel.SearchEmployeeByBizIdCommand.Execute()
   ↓
4. GetEmployeeByBizIdUseCase (DI済み)
   ↓
5. IEmployeeRepository.GetByBizIdAsync() (DI済み EmployeeRepository)
   ↓
6. SQL 実行 (IConnectionFactory と IClock から取得)
   ↓
7. DbModel → Entity 変換 (Mapper使用)
   ↓
8. Entity → Dto (ToDto())
   ↓
9. Dto を ViewModel に返す
   ↓
10. ViewModel.EmployeeFullName = "山田 太郎"
   ↓
11. PropertyChanged イベント発火
   ↓
12. Label1.Text 自動更新
```

---

## 実装チェックリスト

### Phase 0: DI 登録（最初に実施）

- [ ] `src/Contexts/Employee/Employee.Infrastructure/DependencyInjection.cs` を新規作成
  - [ ] `AddEmployeeInfrastructureModels()` メソッド実装
  - [ ] `services.AddScoped<IEmployeeRepository, EmployeeRepository>()` 登録
- [ ] `src/Contexts/Employee/Employee.Application/DependencyInjection.cs` を新規作成
  - [ ] `AddEmployeeApplicationModels()` メソッド実装
  - [ ] `services.AddScoped<GetEmployeeByBizIdUseCase>()` 登録
  - [ ] 既存の Use Cases も登録
- [ ] `src/Infrastructure/DependencyInjection.cs` を修正
  - [ ] `AddInfrastructureModels()` メソッド内で `AddEmployeeInfrastructureModels()` を呼び出し
- [ ] `src/Application/DependencyInjection.cs` を修正
  - [ ] `AddApplicationModels()` メソッド内で `AddEmployeeApplicationModels()` を呼び出し
- [ ] `src/Presentation/WinTrial/DependencyInjection.cs` を修正
  - [ ] `AddWinTrialModules()` メソッド内で `GetEmployeeByBizIdUseCase` を登録

### Phase 1: Application Layer

- [ ] `IEmployeeRepository.GetByBizIdAsync()` メソッドをシグネチャのみ追加
- [ ] `GetEmployeeByBizIdUseCase.cs` を作成
- [ ] Use Case の ExecuteAsync() メソッドを実装
- [ ] 入力検証ロジックを実装

### Phase 2: Infrastructure Layer

- [ ] `EmployeeRepository.GetByBizIdAsync()` を実装
- [ ] SQL クエリを作成（JOIN 含む）
- [ ] Dapper を使用してクエリを実行
- [ ] DbModel → Entity 変換を実装

### Phase 3: Presentation Layer

- [ ] `Form1ViewModel` に `BizIdSearchInput` プロパティを追加
- [ ] `Form1ViewModel` に `EmployeeFullName` プロパティを追加
- [ ] `Form1ViewModel` に `SearchEmployeeByBizIdCommand` を実装
- [ ] GetEmployeeByBizIdUseCase を ViewModel に DI
- [ ] `Form1.cs` に Data Binding を設定
- [ ] sfButton2 に コマンドをバインド

### Phase 4: ビルド・デバッグ

- [ ] プロジェクト全体をビルド（ビルドエラーなし）
- [ ] DI 登録が正しく解決されることを確認（デバッガで追跡）
- [ ] Form1 起動時に DI エラーが出ないことを確認
- [ ] sfButton2 クリック時に SearchEmployeeByBizIdCommand が実行されることを確認

### Phase 5: テスト

- [ ] GetEmployeeByBizIdUseCase の単体テスト
- [ ] EmployeeRepository.GetByBizIdAsync() の統合テスト
- [ ] Form1ViewModel.SearchEmployeeByBizId() のテスト
- [ ] UI 統合テスト（手動）
  - [ ] BizId を入力して検索ボタンをクリック
  - [ ] label1 に正しい姓名が表示される
  - [ ] 見つからない場合は "見つかりません" と表示される

### Phase 6: コードレビュー

- [ ] クリーンアーキテクチャ準拠確認
  - [ ] Form1ViewModel が Infrastructure に依存していない
  - [ ] Use Case が ViewModel に依存していない
- [ ] 入力値検証確認
  - [ ] BizId が空の場合の処理
  - [ ] 不正な入力の処理
- [ ] エラーハンドリング確認
  - [ ] DB 接続エラー時の処理
  - [ ] データ変換エラー時の処理
- [ ] ログ出力確認
  - [ ] 検索実行時のログ
  - [ ] エラー時のログ
- [ ] DI 登録確認
  - [ ] すべての DI ファイルが正しく構成されている
  - [ ] 循環参照がない

---

## エラーハンドリング

### エラーケース分類

| # | エラー | 発生箇所 | ハンドリング | ユーザー表示 |
|---|------|--------|-----------|----------|
| 1 | BizId 空白 | ViewModel | ArgumentException | "BizId を入力してください" |
| 2 | BizId 見つからない | Use Case | null 返却 | "見つかりません" |
| 3 | DB 接続エラー | Repository | Exception | "エラー: {詳細}" |
| 4 | Mapper エラー | Repository | Exception | "エラー: データ変換失敗" |
| 5 | 予期しないエラー | ViewModel | catch (Exception) | "エラー: {詳細}" |

### ログ仕様

```csharp
// 成功時
_logger.LogInformation(
    "Employee found - BizId: EMP001, Name: 山田太郎");

// 見つからない場合
_logger.LogWarning("Employee not found for BizId: INVALID");

// エラー時
_logger.LogError("SearchEmployeeByBizId execution failed", ex);
```

---

## 実装順序（推奨）

```
【Phase 0】DI 登録（**最初に実施**）
   ├─ 1. Employee.Infrastructure/DependencyInjection.cs 作成
   ├─ 2. Employee.Application/DependencyInjection.cs 作成
   ├─ 3. src/Infrastructure/DependencyInjection.cs 修正
   ├─ 4. src/Application/DependencyInjection.cs 修正
   └─ 5. src/Presentation/WinTrial/DependencyInjection.cs 修正
        ↓
【Phase 1】Application Layer
   ├─ 6. IEmployeeRepository.GetByBizIdAsync() シグネチャ追加
   └─ 7. GetEmployeeByBizIdUseCase.cs 作成・実装
        ↓
【Phase 2】Infrastructure Layer
   ├─ 8. EmployeeRepository.GetByBizIdAsync() 実装
   └─ 9. SQL クエリ + Mapper 検証
        ↓
【Phase 3】Presentation Layer
   ├─ 10. Form1ViewModel に Observable プロパティ追加
   ├─ 11. Form1ViewModel に SearchEmployeeByBizIdCommand 実装
   └─ 12. Form1.cs に Binding と Command バインディング設定
        ↓
【Phase 4】ビルド・デバッグ
   ├─ 13. プロジェクト全体ビルド
   ├─ 14. DI 解決のデバッグ
   └─ 15. 手動 UI テスト
        ↓
【Phase 5-6】テスト・レビュー
   ├─ 16. 単体テスト作成
   └─ 17. コードレビュー・修正
```

**重要**: Phase 0 を必ず最初に実施してください。DI なしでは各層の連携が動作しません。

---

## 参考資料

- CLAUDE.md - アーキテクチャ原則
- CLEAN_ARCHITECTURE_GUIDELINES.md - 層間依存関係
- Entity_設計ガイドライン.md - Entity 実装パターン
- Mapper_パターンガイド.md - Mapper 実装パターン

---

**設計書版**: 3.0（DB テーブル構造確定、属性テーブル経由実装）  
**最終更新**: 2026-08-16

## 変更履歴

| バージョン | 日付 | 変更内容 |
|----------|------|--------|
| 3.0 | 2026-08-16 | DB テーブル構造を確定（m_employees, m_employee_attributes）。属性テーブル経由で m_persons に接続する SQL クエリに修正。EmployeeDbModel をシンプル化（ビジネスカラムのみ）。パフォーマンス INDEX を明記 |
| 2.0 | 2026-08-15 | DI 登録の実装詳細を追加。5つのファイル（DependencyInjection.cs）の作成・修正方法を明記。DI チェーン図と実装チェックリストを更新 |
| 1.0 | 2026-08-15 | 初版作成。アーキテクチャ設計、各層実装コード、SQL 設計、画面設計を記載 |
