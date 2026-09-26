# クリーンアーキテクチャ実装ガイドライン

## 概要

このドキュメントは、SupportAdvance プロジェクトにおけるクリーンアーキテクチャの原則と実装ガイドラインを定義します。

**基本原則：** 依存関係は外側から内側にのみ向かう。内側の層が外側の層に依存してはいけない。

---

## アーキテクチャレイヤー構成

依存は常に「外側 → 内側」に向かう。最も内側（依存ゼロ）は `Common`、その上に `SharedKernel` が乗る。`Infrastructure` と `Presentation` は互いに独立した外側の層で、`Application`/`Domain` を挟んで対称に位置する（Infrastructure は Presentation より内側ではない）。

```
                     ┌───────────────────────────┐
                     │        Common             │  ← 依存ゼロ（最内層）
                     │ (IClock, LocalDateTime,    │
                     │  IApplicationSettings 等)  │
                     └─────────────▲─────────────┘
                                    │ 依存
                     ┌──────────────┴─────────────┐
                     │       SharedKernel          │
                     │ (ValueObject, Entity基底,   │
                     │  監査ValueObject 等)        │
                     └──────────────▲─────────────┘
                                    │ 依存
                     ┌──────────────┴─────────────┐
                     │           Domain             │
                     │  (Entities / Aggregates)      │
                     └──────────────▲─────────────┘
                                    │ 依存
                     ┌──────────────┴─────────────┐
                     │        Application           │
                     │  (Use Cases)                  │
                     └──▲───────────────────────▲──┘
              実装を注入 │                       │ 呼び出し
     ┌────────────────┴───┐               ┌───┴────────────────┐
     │   Infrastructure     │               │    Presentation      │
     │ (DB / ORM / 外部API) │               │ (UI / ViewModel)     │
     └───────────▲───────────┘               └──────────▲───────────┘
                 │ 実装                                  │ Composition Root（Program.cs / WPF は App.xaml.cs）でのみ
                 └──────────────── 参照 ──────────────────┘
                Crosscutting はロギング等のインターフェース＋実装を自己完結で持つ横断的関心事
                （実装に必要な外部ライブラリはNuGetで直接取得。Infrastructureには依存しない）
```

**ポイント:**
- `Infrastructure` は最も内側ではなく、`Presentation` と対称な**最も外側の層**（実装の詳細）
- `Crosscutting` はロギング等のインターフェースと実装の両方を持つ（例：`IAppLogging<T>` とその NLog 実装 `FrameworkLoggingAdapter`）。実装に必要な NLog 等は NuGet パッケージとして Crosscutting が直接参照し、`Infrastructure` プロジェクトには依存しない。したがって参照方向は **Infrastructure → Crosscutting** の一方向のみ
- `Presentation` から `Infrastructure` への参照は、DIコンテナを組み立てる **Composition Root（`Program.cs`。WPF は `App.xaml.cs`）に限定**される（詳細は後述の「自動検証の導入」参照）

---

## 各層の詳細定義

### 1. **Shared Kernel** (`src/SharedKernel`)

**責務:**
- プロジェクト全体で共有される基盤型の定義
- Value Objects（ValueObject基底クラス）
- エンティティの基底型
- 監査フィールド（CreatedAt, UpdatedBy など）
- プロジェクト全体で使用される列挙型・定数

**特徴:**
- `Common` の上に構築される、ドメインに近い基盤層
- 外側のすべての層から参照可能
- 監査 ValueObject（`CreatedAt` / `UpdatedAt` / `DeletedAt` など）が `Common` の `LocalDateTime` / `IClock` を利用するため、`Common` への依存を持つ

**許可される参照:**
- Common

**禁止される参照:**
- Application, Infrastructure, Presentation, Crosscutting

**例:**
```csharp
namespace SupportAdvance.SharedKernel.ValueObjects;

public abstract class ValueObject : IEquatable<ValueObject>
{
    // ValueObject基底クラスの実装
}

public record PersonId(Guid Value) : ValueObject;
```

```csharp
// SharedKernel/ValueObjects/Audit/CreatedAt.cs
using SupportAdvance.Common.Clocks; // Common の LocalDateTime を利用

namespace SupportAdvance.SharedKernel.ValueObjects.Audit;

public sealed record CreatedAt(LocalDateTime Value) : ValueObject;
```

---

### 2. **Common** (`src/Common`)

**責務:**
- アプリケーション全体で使用される汎用ユーティリティ
- 設定インターフェース（IApplicationSettings など）
- クロック実装（IClock）
- 共通ヘルパー・エクステンション

**特徴:**
- プロジェクト全体で**最も内側**のレイヤー（依存ゼロ）
- インフラストラクチャに依存しない汎用ライブラリ
- Business Logic を含まない
- `SharedKernel` はこの層に依存するが、逆方向（Common → SharedKernel）は循環参照になるため禁止

**許可される参照:**
- なし（他プロジェクトへの参照を持たない）

**禁止される参照:**
- SharedKernel, Domain, Application, Infrastructure, Presentation, Crosscutting

**例:**
```csharp
namespace SupportAdvance.Common.Clocks;

public interface IClock
{
    DateTime UtcNow { get; }
}
```

---

### 3. **Crosscutting** (`src/Crosscutting`)

**責務:**
- 複数の層で共有される横断的関心事
- ロギング基盤
- 監査ログ機構
- 共通バリデーション
- 例外変換

**特徴:**
- ロギング・監査などの**インターフェースと実装の両方**をこの層だけで完結させる（例：`IAppLogging<T>` と NLog 実装 `FrameworkLoggingAdapter`）
- 実装に必要な技術要素（NLog など）は **NuGet パッケージとして直接参照**し、Infrastructure プロジェクトには依存しない
- そのため参照方向は **Infrastructure → Crosscutting** の一方向のみ。Crosscutting → Infrastructure は禁止
  - 理由：Crosscutting が Infrastructure（技術実装の詳細）に依存すると、横断的関心事が技術詳細に結合し、再利用性が低下するため
- Domain への依存も避ける

**許可される参照:**
- SharedKernel
- Common

**禁止される参照:**
- Domain, Application（**原則として**）, Infrastructure

**例（`src/Crosscutting/Logging/IAppLogging.cs`）:**
```csharp
namespace SupportAdvance.Crosscutting.Logging;

public interface IAppLogging<T>
{
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
}
```

この `IAppLogging<T>` の実装（`FrameworkLoggingAdapter`、NLog 連携）も同じ `Crosscutting` プロジェクト内に存在する。`Crosscutting.csproj` が `NLog.Extensions.Logging` を NuGet パッケージとして直接参照しているため、`Infrastructure` を経由する必要がない。

---

### 4. **Domain Layer** - 各 Bounded Context

**プロジェクト:**
- `src/SharedKernel` - 共有ドメインモデル
- `src/Contexts/Samples/CarPreferences.Domain` - ドメインロジック（サンプル）

**責務:**
- ビジネスルールの実装
- Entities（エンティティ）
- Aggregates（集約）
- Value Objects（値オブジェクト）
- Domain Events（ドメインイベント）
- ドメイン例外

**特徴:**
- **依存が最小限**
- 外界への依存はない
- Infrastructure への依存は許可されない
- Application への依存は許可されない

**許可される参照:**
- SharedKernel
- Common

**禁止される参照:**
- Crosscutting（ドメインロジックは完全独立。ドメインイベント発行は Entity.RaiseDomainEvent() で内部完結）
- Application
- Infrastructure
- Presentation

**例:**
```csharp
namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;

public class Car : AggregateRoot<RowId>
{
    public CarModel Model { get; private set; }
    public LocalDateTime UpdatedAt { get; private set; }

    public void UpdateModel(CarModel model, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(clock);

        // ビジネスルールを実装
        Model = model;
        UpdatedAt = clock.JstNow;
        RaiseDomainEvent(new CarModelUpdatedEvent(DomainEventId.New(), model, clock.JstNow));
    }
}
```

---

### 5. **Application Layer** - Use Cases / Application Services

**プロジェクト:**
- `src/Application` - 汎用アプリケーションサービス（基盤インターフェース）
- `src/Contexts/Samples/CarPreferences.Application` - Use Cases（Bounded Context実装）

**汎用 Application層 vs Bounded Context別 Application層:**

Application層は2つの役割に分かれている：

1. **`src/Application`（汎用層）** — インターフェースのみ定義
   - `IUseCase`、`IRequest`、`IResponse` などの基盤抽象化
   - すべての Bounded Context から参照される
   - 具体的な Use Case 実装は持たない

2. **`src/Contexts/*/Application`（Context別層）** — 実装
   - 汎用層の IUseCase / IRequest / IResponse を実装
   - ドメインロジック（Domain層）と Infrastructure の仲介役
   - Context固有の Use Case を実装

**Bounded Context別 Application が汎用 Application に依存することは許可される**（インターフェース実装パターン）

#### ファイル構造例

**汎用層:**
```
src/Application/
├── Application.csproj
├── IUseCase.cs              # インターフェースのみ定義
├── IRequest.cs
├── IResponse.cs
└── Abstractions/
    └── 共有抽象化のみ（具体実装は持たない）
```

**Context別層:**
```
src/Contexts/Samples/CarPreferences.Application/
├── CarPreferences.Application.csproj
├── UseCases/
│   ├── UpdateCarPreferenceUseCase.cs  # IUseCase を実装
│   └── GetCarPreferenceUseCase.cs
├── Commands/
│   ├── UpdateCarPreferenceCommand.cs  # IRequest を実装
│   └── GetCarPreferenceCommand.cs
└── DTOs/
    ├── UpdateCarPreferenceDto.cs      # IResponse を実装
    └── GetCarPreferenceDto.cs
```

#### 参照フロー図

```
汎用Application層:
├── IUseCase（インターフェース定義）
├── IRequest（インターフェース定義）
└── IResponse（インターフェース定義）
        ▲
        │ implements（実装）
        │
Context別Application層（CarPreferences.Application）:
├── UpdateCarPreferenceUseCase implements IUseCase ✓
├── UpdateCarPreferenceCommand implements IRequest ✓
└── UpdateCarPreferenceDto implements IResponse ✓
        │
        ├─ depends on ──→ Domain: Car（ビジネスロジック呼び出し）
        ├─ depends on ──→ Crosscutting: IAppLogging（ロギング機能）
        └─ depends on ──→ Infrastructure（DI で注入）※直接参照なし
```

#### 実装パターンの選択

Repository インターフェースの定義位置には、2つの主要パターンがある：

**パターンA: Repository インターフェースを汎用Application に定義（推奨な場合も）**
```csharp
// src/Application/Repositories/ICarPreferenceRepository.cs
namespace SupportAdvance.Application.Repositories;
public interface ICarPreferenceRepository
{
    Task<Car> GetByIdAsync(CarId id);
}

// Context別Infrastructure が実装
// src/Contexts/Samples/CarPreferences.Infrastructure/Repositories/CarPreferenceRepository.cs
namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Repositories;
public class CarPreferenceRepository : ICarPreferenceRepository
{
    // 実装
}
```

**パターンB: Repository インターフェースを Context別Application に定義（現在の実装）**
```csharp
// src/Contexts/Samples/CarPreferences.Application/Repositories/ICarPreferenceRepository.cs
namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.Repositories;
public interface ICarPreferenceRepository
{
    Task<Car> GetByIdAsync(CarId id);
}

// Context別Infrastructure が実装
// src/Contexts/Samples/CarPreferences.Infrastructure/Repositories/CarPreferenceRepository.cs
namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Repositories;
public class CarPreferenceRepository : ICarPreferenceRepository
{
    // 実装
}
```

**現在の SupportAdvance 実装:** **パターンB**（Context別層で Repository インターフェースを定義）
- 利点：各 Context がインターフェースを独立管理。Context間の結合度が低い
- 汎用Application は本当に共通部分のインターフェースのみ定義

**責務:**
- Use Cases（ユースケース）の実装
- Application Services（アプリケーションサービス）
- DTO（Data Transfer Objects）
- コマンド / クエリ
- Infrastructure への呼び出し調整

**特徴:**
- **ドメインロジックを含まない**
- Infrastructure への依存は DI により逆転されるべき
- Presentation 層から直接呼び出される

**許可される参照:**
- SharedKernel
- Common
- Domain（各Bounded Context）
- Crosscutting（ロギング等）
- Application（汎用層）* ← Bounded Context別層のみ。汎用層は他を参照しない

**禁止される参照:**
- Infrastructure（直接参照は禁止、DI により注入）
- Presentation

*注：Bounded Context別 Application（例：CarPreferences.Application）が汎用 Application（IUseCase 等）に依存することはインターフェース実装パターンとして許可される。逆に、汎用 Application が Context別層を参照することは禁止。

**例:**
```csharp
namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases;

public class UpdateCarPreferenceUseCase
{
    private readonly ICarPreferenceRepository _repository;
    private readonly IDomainEventPublisher _eventPublisher;

    public UpdateCarPreferenceUseCase(
        ICarPreferenceRepository repository,
        IDomainEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task Execute(UpdateCarPreferenceCommand command)
    {
        var car = await _repository.GetByIdAsync(command.CarId);
        car.UpdateModel(command.NewModel);
        await _eventPublisher.PublishAsync(car.DomainEvents);
    }
}
```

#### Context間のデータ共有：ジェネリック Query Service パターン

複数の Bounded Context が別の Context のドメインモデル（Aggregate）情報を **リアルタイムに読み取る** 場合、**ジェネリック Query Service パターン** を採用します。

**背景:**
- Domain Events は非同期・遅延同期なため、「UseCase実行時の最新データ」が必要な場合には不向き
- Context別 Application層同士の参照は禁止
- 汎用Application層がContext固有のインターフェース（`IEmployeeQuery`, `IInsuranceQuery` など）を定義すると、Context増加時に肥大化する

**解決策：汎用Application層にジェネリックインターフェースを定義**

```csharp
// src/Application/Queries/IQueryService.cs
// ← 汎用層：Aggregate非依存な抽象定義のみ
namespace SupportAdvance.Application.Queries;

public interface IQueryService<TAggregate, TId> 
    where TAggregate : IAggregateRoot
{
    Task<TAggregate?> GetByIdAsync(TId id);
}
```

**各Contextが実装:**

```csharp
// src/Contexts/Employee/Application/Queries/EmployeeQueryService.cs
public class EmployeeQueryService : IQueryService<Employee, EmployeeId>
{
    private readonly IEmployeeRepository _repository;

    public async Task<Employee?> GetByIdAsync(EmployeeId id)
    {
        return await _repository.GetByIdAsync(id);
    }
}

// src/Contexts/Insurance/Application/Queries/InsuranceQueryService.cs
public class InsuranceQueryService : IQueryService<InsuranceProfile, InsuranceId>
{
    private readonly IInsuranceRepository _repository;

    public async Task<InsuranceProfile?> GetByIdAsync(InsuranceId id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
```

**他のContextが使用:**

```csharp
// src/Contexts/CarPreferences/Application/UseCases/UpdateCarPreferencesUseCase.cs
public class UpdateCarPreferencesUseCase
{
    // 汎用Application層の IQueryService<Employee, EmployeeId> に依存
    // → Employee Context の実装が DI で注入される
    private readonly IQueryService<Employee, EmployeeId> _employeeQuery;

    public UpdateCarPreferencesUseCase(
        IQueryService<Employee, EmployeeId> employeeQuery,
        ICarPreferencesRepository preferencesRepo)
    {
        _employeeQuery = employeeQuery;
        _preferencesRepo = preferencesRepo;
    }

    public async Task Execute(EmployeeId employeeId, CarModelRequest request)
    {
        // リアルタイムに Employee 情報を取得
        var employee = await _employeeQuery.GetByIdAsync(employeeId);
        if (employee == null)
            throw new EmployeeNotFoundException();

        var pref = new CarPreferences(employeeId, request.Model);
        await _preferencesRepo.SaveAsync(pref);
    }
}
```

**DI設定:**

```csharp
// Program.cs
public static void Main(string[] args)
{
    var services = new ServiceCollection();
    
    // Employee Context
    services.AddScoped<IQueryService<Employee, EmployeeId>, EmployeeQueryService>();
    services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    
    // Insurance Context
    services.AddScoped<IQueryService<InsuranceProfile, InsuranceId>, InsuranceQueryService>();
    services.AddScoped<IInsuranceRepository, InsuranceRepository>();
    
    // CarPreferences Context
    services.AddScoped<ICarPreferencesRepository, CarPreferencesRepository>();
}
```

**メリット:**

| メリット | 説明 |
|---------|------|
| **汎用層が肥大化しない** | `IQueryService<TAggregate, TId>` という単一のジェネリック定義。Context固有のインターフェースが増えない |
| **スケーラブル** | Aggregate（Employee, Insurance, Benefits など）が増えても、汎用層の構造は不変 |
| **Context独立** | 各Contextが自身の Aggregate を独立管理。Context間の結合度が低い |
| **依存方向が正** | Context別Application→汎用Application（**外から内への正常な依存方向**） |
| **リアルタイムアクセス** | Domain Events（非同期）ではなく、UseCase実行時の最新データを取得可能 |

**参照フロー:**

```
汎用Application層:
└── IQueryService<TAggregate, TId>（ジェネリック定義）
        ▲
        │ implements
        │
Employee Context別Application層:
└── EmployeeQueryService implements IQueryService<Employee, EmployeeId>
        │
        ├── depends on → Employee Domain
        └── depends on → IEmployeeRepository（DI注入）
        
他のContext（CarPreferences等）別Application層:
└── UpdateCarPreferencesUseCase
        │
        ├── depends on → IQueryService<Employee, EmployeeId>（汎用層経由）
        └── depends on → CarPreferences Domain
```

---

### 6. **Infrastructure Layer**

**プロジェクト:**
- `src/Infrastructure` - Database / External Service implementations
- `src/Infrastructure/Repositories` - 汎用 Repository 基底クラス

**責務:**
- データベース実装（Repository パターン）
- ORM 統合（Dapper, RepoDB など）
- 外部API/サービスの実装
- ファイルシステムアクセス
- Configuration プロバイダー
- 監査フィールド管理（CreatedAt, UpdatedAt, DeletedAt など）

**特徴:**
- **最も外側のレイヤー**
- Application / Domain のインターフェースを実装
- 技術的な詳細を隠蔽

**Repository 基底クラス**

Infrastructure層では2つの Repository 基底クラスを提供：

#### RepositoryBase<TEntity, TDbModel, TId>
単一テーブル集約向け。IEntityMapper 実装を前提。

```csharp
// 単一テーブル集約の場合
public class UserRepository(
    IEntityMapper<User, UserDbModel, UserId> mapper,
    ICurrentUserService currentUser,
    IClock clock)
    : RepositoryBase<User, UserDbModel, UserId>(mapper, currentUser, clock)
{
    // CRUD 実装
}
```

#### MultiTableRepositoryBase<TEntity, TDbModel, TId>
複数テーブル集約向け。複数の関連テーブルから Entity を構築する場合に使用。

```csharp
// 複数テーブル集約（m_employees + m_persons + m_department_memberships）の場合
public class EmployeeRepository(
    EmployeeMapper mapper,
    IDbConnectionFactory connectionFactory,
    ICurrentUserService currentUser,
    IClock clock,
    IAppLogging<EmployeeRepository> logger)
    : MultiTableRepositoryBase<Employee, EmployeeDbModel, EmployeeRowId>(currentUser, clock),
      IEmployeeRepository
{
    // 複雑な読み込み・保存ロジック実装
    public async Task<Employee?> GetByIdAsync(EmployeeRowId id) { ... }
}
```

**Mapper の責務分離**

- **Mapper**: 純粋な型変換（ドメイン型 ↔ DB型）。Clock 依存なし。
- **Repository**: 監査フィールド設定（CreatedAt, UpdatedAt, DeletedAt）。SetUpdatedAtAudit(), SetUpdatedByAudit() を使用。

```csharp
// Mapper は型変換のみ
public class EmployeeMapper
{
    public Employee ToDomainEntity(EmployeeDbModel dbModel, PersonDbModel personDbModel)
    {
        // 型変換のみ。監査フィールド設定なし
        return Employee.Reconstruct(...);
    }

    public EmployeeDbModel ToDbModel(Employee entity)
    {
        // ビジネス属性のみ。監査フィールドなし
        return new EmployeeDbModel { ... };
    }
}

// Repository が監査フィールドを設定
public class EmployeeRepository : MultiTableRepositoryBase<...>
{
    public async Task UpdateAsync(Employee employee)
    {
        var dbModel = _mapper.ToDbModel(employee);
        SetUpdatedAtAudit(dbModel);    // Repository が責務を持つ
        SetUpdatedByAudit(dbModel);
        // SQL 実行
    }
}
```

**許可される参照:**
- SharedKernel
- Common
- Crosscutting（ロギング）
- Domain（Entity/Value Object のマッピング用）
- Application***（インターフェース実装パターンのみ）
  - 注意：汎用 `src/Infrastructure` プロジェクト自体は稀にのみ Application を参照
  - 通常は **Bounded Context別 Infrastructure**（例：`src/Contexts/Employee/Employee.Infrastructure`）が、Context固有のインターフェース実装を担当する設計

**禁止される参照:**
- Presentation

**例:**
```csharp
// ✓ OK：複数テーブル集約の Repository 実装例
namespace SupportAdvance.Contexts.Employee.Infrastructure.Repositories;

public class EmployeeRepository(
    EmployeeMapper mapper,
    IDbConnectionFactory connectionFactory,
    ICurrentUserService currentUser,
    IClock clock,
    IAppLogging<EmployeeRepository> logger)
    : MultiTableRepositoryBase<Employee, EmployeeDbModel, EmployeeRowId>(currentUser, clock),
      IEmployeeRepository
{
    public async Task<Employee?> GetByIdAsync(EmployeeRowId id)
    {
        // 複数テーブル（m_employees, m_persons, m_department_memberships）から読み込み
        var employeeDbModel = await LoadEmployeeAsync(id);
        var personDbModel = await LoadPersonAsync(id);
        var memberships = await LoadDepartmentMembershipsAsync(id);
        
        return _mapper.ToDomainEntity(employeeDbModel, personDbModel, memberships);
    }

    public async Task UpdateAsync(Employee employee)
    {
        var empDbModel = _mapper.ToDbModel(employee);
        SetUpdatedAtAudit(empDbModel);      // Repository が監査フィールドを設定
        SetUpdatedByAudit(empDbModel);
        // SQL で保存
    }
}
```

---

### 7. **Presentation Layer** - UI / Controllers

**プロジェクト:**
- `src/Presentation/Shared` - Shared Components
- `src/Presentation/WinTrial` - Windows Forms UI
- `src/Presentation/WpfTrial` - WPF UI

**責務:**
- UI の構築
- ユーザー入力の受け取り
- Use Cases の呼び出し
- 結果の表示

**特徴:**
- **最も外側のレイヤー**
- Application / Infrastructure に依存
- Composition Root（Program.cs）で DI 構築

**許可される参照:**
- Application（Use Case 呼び出し）
- Crosscutting（ロギング）
- Infrastructure（**Composition Root のみ**）
- SharedKernel / Common（型の使用）

**禁止される参照:**
- Domain（直接参照は避け、Application DTO 経由）
- Infrastructure（**Composition Root の `Program.cs` / WPF の `App.xaml.cs` を除く**）

**例:**
```csharp
namespace SupportAdvance.Presentation.WinTrial.ViewModels;

public class CarPreferenceViewModel : INotifyPropertyChanged
{
    private readonly IUpdateCarPreferenceUseCase _useCase;

    public async void UpdatePreference(string carModel)
    {
        var result = await _useCase.Execute(new UpdateCarPreferenceCommand
        {
            CarModel = carModel
        });
        OnPropertyChanged(nameof(CurrentPreference));
    }
}
```

**Program.cs での DI 構築例:**
```csharp
// ✓ 許可：Program.cs のみで Infrastructure への参照
var services = new ServiceCollection();
services.AddInfrastructureModels(configuration);
services.AddApplicationServices();
```

---

## 依存関係マトリックス

| From \ To | Common | SharedKernel | Crosscutting | Domain | Application | Infrastructure | Presentation |
|---|---|---|---|---|---|---|---|
| **Common** | - | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ |
| **SharedKernel** | ✓ | - | ✗ | ✗ | ✗ | ✗ | ✗ |
| **Crosscutting** | ✓ | ✓ | - | ✗ | ✗ | ✗ | ✗ |
| **Domain** | ✓ | ✓ | ✗ | - | ✗ | ✗ | ✗ |
| **Application** | ✓ | ✓ | ✓ | ✓ | - | ✗ | ✗ |
| **Infrastructure** | ✓ | ✓ | ✓ | ✓ | ✓*** | - | ✗ |
| **Presentation** | ✓ | ✓ | ✓ | ✗ | ✓ | ✓** | - |

- `✓` = 許可
- `✗` = 禁止
- `**` = Presentation → Infrastructure：Composition Root（`Program.cs`。WPF は `App.xaml.cs`）のみ許可。プロジェクト参照上は Infrastructure に到達可能な構成だが、`Program` 型（WPF は `App` 型）を除く全ての型が Infrastructure 名前空間に依存しないことを [自動検証](#自動検証の導入) で担保する
- `***` = Infrastructure → Application：**インターフェース実装パターンのみ許可**。Application層で定義されたインターフェース（例：IRepository）を Infrastructure層が実装する場合、Application プロジェクトへの参照が必須。直接型を参照することは禁止（DI により逆転）
  - 設計パターン：汎用 Infrastructure（`src/Infrastructure`）は稀にのみ参照。通常は **Bounded Context別 Infrastructure**（例：`CarPreferences.Infrastructure`）が汎用 Application を参照し、Context固有のインターフェースを実装する
- `Common` は依存ゼロの最内層
- `Crosscutting → Infrastructure` は禁止 — Crosscutting は NLog などの NuGet パッケージを直接参照し、技術詳細（Infrastructure）に依存しない設計。Infrastructure が Crosscutting のインターフェースを実装する一方向のみ許可
- **Application → Application** = 汎用 Application層（`src/Application`）と Bounded Context別層（`src/Contexts/*/Application`）の関係。汎用層はインターフェース定義のみ、Context別層がそれを実装。Context別層が汎用層に依存することは許可。逆に汎用層が Context別層を参照することは禁止

---

## 新規プロジェクト追加時のチェックリスト

### Step 1: プロジェクト構造の決定

- [ ] プロジェクトが属するレイヤーを決定（Domain / Application / Infrastructure など）
- [ ] Bounded Context 内で独立した責務を持つか確認
- [ ] 既存プロジェクトとの関係を明確にする

### Step 2: プロジェクト ファイル（.csproj）の検証

```xml
<!-- 許可の例：Application コンテキストプロジェクト -->
<ItemGroup>
  <ProjectReference Include="..\CarPreferences.Domain\CarPreferences.Domain.csproj" />
  <ProjectReference Include="..\..\Application\Application.csproj" />
  <ProjectReference Include="..\..\SharedKernel\SharedKernel.csproj" />
</ItemGroup>

<!-- 禁止の例：Domain が Application を参照 -->
<ProjectReference Include="..\CarPreferences.Application\..." /> ✗ NG
```

**チェック項目:**
- [ ] ProjectReference が依存関係マトリックスに準拠しているか
- [ ] 不要な参照が含まれていないか
- [ ] 循環参照がないか

### Step 3: Namespace の確認

```csharp
// ✓ 許可
using SupportAdvance.Contexts.Samples.CarPreferences.Domain;
using SupportAdvance.Common;
using SupportAdvance.SharedKernel;

// ✗ 禁止（Domain が Application を参照）
using SupportAdvance.Contexts.Samples.CarPreferences.Application;
using SupportAdvance.Infrastructure;
```

**チェック項目:**
- [ ] using 宣言が依存関係マトリックスに準拠しているか
- [ ] 逆方向依存がないか
- [ ] 不要な using が含まれていないか

### Step 4: アーキテクチャ分析ツールの実行

```bash
# ビルド検証
dotnet build

# オプション：FxCop 分析
dotnet format --verify-no-changes --verbosity diagnostic
```

**チェック項目:**
- [ ] ビルド が成功するか
- [ ] アーキテクチャ分析ツールでエラーがないか

### Step 5: コードレビュー

- [ ] レイヤーの責務が適切に分離されているか
- [ ] 依存注入が正しく実装されているか
- [ ] Domain Layer に外界への依存がないか

---

## ⏰ LocalDateTime 使用規則

プロジェクト全体で一貫した日時処理を行うため、以下のルールを厳守してください。

### 基本原則

- **Domain/Application 層**: `LocalDateTime` を使用（型安全、ビジネスロジック）
- **Infrastructure/DbModel 層**: `DateTime` プリミティブ型を使用（ORM マッピング必須）
- **Mapper 層**: 双方向変換を実装（`new LocalDateTime(dt)` / `localDateTime.Value`）
- **`IClock` 経由でのみ日時を取得** — System.DateTime.Now 等への直接アクセスは禁止
- **ローカライズされた時刻を一貫して使用** — JST（日本標準時）に統一

### DateTime の使用許可例外

**Clock 実装内部のみ許可:**
- `src/Common/Clocks/SystemClock.cs`
- `src/Common/Clocks/OffsetClock.cs`
- `src/Common/Clocks/TickingClock.cs`

これらの Clock 実装内では `DateTime.Now`、`DateTime.UtcNow` などの使用を許可。Clock はシステム時刻を `LocalDateTime` に変換する責務を持つ。

### Clock の取得と使用

**推奨パターン:**
```csharp
// ✅ OK: 全層で DI を通じて IClock を注入
public class MyUseCase
{
    private readonly IClock _clock;
    
    public MyUseCase(IClock clock)
    {
        _clock = clock;
    }
    
    public async Task Execute()
    {
        var now = _clock.JstNow;  // LocalDateTime を取得
        // ...
    }
}
```

### 外部システム/DB からの DateTime 変換

**原則:** Infrastructure層の Mapper で DB の DateTime を LocalDateTime に変換。以降 Domain/Application では LocalDateTime のみを使用。

**実装パターン:**
```csharp
// ✅ OK: DbModel（DateTime プリミティブ型）から Domain Entity（LocalDateTime）に変換
public class EmployeeMapper
{
    public Employee ToDomainEntity(EmployeeDbModel dbModel)
    {
        // DB値（DateTime）から ValueObject に変換
        var retiredOn = RetiredOn.Unset;
        if (dbModel.RetiredOn.HasValue)
        {
            // DateTime → LocalDateTime（Mapper が変換責務を持つ）
            retiredOn = RetiredOn.From(new LocalDateTime(dbModel.RetiredOn.Value));
        }
        
        return new Employee(..., retiredOn);
    }
    
    public EmployeeDbModel ToDbModel(Employee entity)
    {
        // Domain の LocalDateTime → DB の DateTime に変換
        return new EmployeeDbModel
        {
            RetiredOn = entity.RetiredOn.IsSet ? entity.RetiredOn.Value.Value : null
        };
    }
}
```

### Clock の環境別実装

**本番環境:**
```csharp
// SystemClock のみ使用（実システム時刻）
services.AddSingleton<IClock>(new SystemClock());
```

**テスト環境:**
```csharp
// FixedClock: 固定時刻を返す
// OffsetClock: テスト開始時刻からのオフセット
// TickingClock: シミュレーション用に時刻を進める
services.AddSingleton<IClock>(new FixedClock(new LocalDateTime(2026, 1, 1, 10, 0, 0)));
```

### 多層での使用例

| 層 | 使用パターン | 備考 |
|---|---|---|
| Domain | `LocalDateTime` を ValueObject で保持、`IClock` をパラメータで受け取り検証 | ビジネスロジック検証用 |
| Application | `_clock.JstNow` で現在時刻取得、`LocalDateTime` を使用 | DI注入 |
| Infrastructure | DbModel は `DateTime` プリミティブ型、Mapper が `LocalDateTime` ↔ `DateTime` を変換 | 層間の変換責務 |
| Presentation | `IClock` をDIで参照、表示用に `LocalDateTime` を文字列に変換 | ViewModel で表示用に変換 |

---

## よくある違反パターンと対策

### ❌ パターン1: Application が Infrastructure に直接依存

```csharp
// ✗ NG：Application で Infrastructure 型を直接使用
using SupportAdvance.Infrastructure.Data;

public class UserService
{
    public UserDto GetUser(int id)
    {
        var repository = new SqlUserRepository(); // 直接インスタンス化
        return repository.Get(id);
    }
}
```

**対策:**
```csharp
// ✓ OK：インターフェースを注入
public class UserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository) // DI
    {
        _repository = repository;
    }

    public async Task<UserDto> GetUser(int id)
    {
        return await _repository.GetAsync(id);
    }
}
```

---

### ❌ パターン2: Domain が Application に依存

```csharp
// ✗ NG：Domain で Application DTO を使用
using SupportAdvance.Application.Dtos;

public class Order : Entity
{
    public void UpdateStatus(OrderStatusDto status) // Application の型を使用
    {
        // ...
    }
}
```

**対策:**
```csharp
// ✓ OK：Domain 独立の型を使用
public class Order : Entity
{
    public void UpdateStatus(OrderStatus status) // Domain の型
    {
        // ...
    }
}

// Application で変換
public class UpdateOrderStatusService
{
    public async Task Execute(OrderStatusDto dto)
    {
        var status = MapToOrderStatus(dto);
        var order = await _repository.GetAsync(...);
        order.UpdateStatus(status);
    }
}
```

---

### ❌ パターン3: Infrastructure から Application への参照

```xml
<!-- ✗ NG：修正前の状態 -->
<ProjectReference Include="..\Application\Application.csproj" />
```

**対策:**
```xml
<!-- ✓ OK：参照を削除 -->
<!-- 削除 -->
```

Infrastructure が Application インターフェースを必要とする場合は、インターフェースを下層（Common / Crosscutting）に移動します。

---

### ❌ パターン4: Presentation が Infrastructure に直接依存

```csharp
// ✗ NG：ViewModel から Infrastructure へ直接アクセス
public class UserViewModel
{
    private UserRepository _repository = new UserRepository(); // 直接参照

    public void LoadUsers()
    {
        // ...
    }
}
```

**対策:**
```csharp
// ✓ OK：Program.cs でのみ Infrastructure を参照
// Program.cs
var services = new ServiceCollection();
services.AddInfrastructureModels(configuration);

// ViewModel は Application の Use Case のみ使用
public class UserViewModel
{
    private readonly IGetUsersUseCase _useCase;

    public UserViewModel(IGetUsersUseCase useCase)
    {
        _useCase = useCase;
    }
}
```

---

## 自動検証の導入

現状、依存関係の遵守は**コードレビューによる目視確認のみ**に依存している。特に以下の点は `.csproj` の `ProjectReference` だけでは強制できないため、型レベルの検証が必要：

- `Presentation` → `Infrastructure` は Composition Root（`Program.cs`。WPF は `App.xaml.cs`）のみ許可（例：`WinTrial.csproj` は DI 配線のため `CarPreferences.Infrastructure` を参照せざるを得ないが、`Program` 型（WPF は `App` 型）以外がそれを使ってはならない）
- `Crosscutting` → `Infrastructure` は禁止（循環参照防止）

### NetArchTest.Rules による検証（推奨）

`NetArchTest.Rules`（NuGet）を使い、テストプロジェクトとして `dotnet test` / CI に組み込む。

```bash
dotnet add package NetArchTest.Rules
```

```csharp
// tests/Architecture.Tests/DependencyRuleTests.cs
using NetArchTest.Rules;
using Xunit;

public class DependencyRuleTests
{
    [Fact]
    public void Domain_Should_Not_DependOn_ApplicationOrInfrastructure()
    {
        var result = Types.InAssembly(typeof(CarPreferences.Domain.ValueObjects.CarModel).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SupportAdvance.Application",
                "SupportAdvance.Contexts.Samples.CarPreferences.Application",
                "SupportAdvance.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Common_Should_Have_No_Dependencies()
    {
        var result = Types.InAssembly(typeof(SupportAdvance.Common.Clocks.IClock).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SupportAdvance.SharedKernel",
                "SupportAdvance.Domain",
                "SupportAdvance.Application",
                "SupportAdvance.Infrastructure",
                "SupportAdvance.Crosscutting")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Crosscutting_Should_Not_DependOn_Infrastructure()
    {
        var result = Types.InAssembly(typeof(SupportAdvance.Crosscutting.Logging.IAppLogging<>).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SupportAdvance.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    // 「Composition Root（Program.cs / WPF は App.xaml.cs）のみ Infrastructure 参照可」を型レベルで検証。
    // WinTrial.csproj 自体は Infrastructure への参照を持つが、
    // Program 型以外がそれを使っていなければ合格する。
    [Fact]
    public void WinTrial_Only_Program_May_DependOn_Infrastructure()
    {
        var result = Types.InAssembly(typeof(SupportAdvance.Presentation.WinTrial.Program).Assembly)
            .That()
            .DoNotHaveName("Program")
            .ShouldNot()
            .HaveDependencyOnAny(
                "SupportAdvance.Infrastructure",
                "SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
```

上記4テストで参照している型（`IClock` / `CarModel` / `IAppLogging<T>` / `Program`）は 2026-07-31 時点で実在する型であり、現行の `ProjectReference` 構成・`using` 状況から見て合格する見込みだが、実装時は必ず `dotnet test` で実行結果を確認すること。CI に組み込むことで、将来のコード追加時にも依存方向の逸脱を機械的に検出できる。

---

## 参考資料

- **クリーンアーキテクチャ**: Robert C. Martin の著書『Clean Architecture』
- **Dependency Rule**: 外側の層は内側の層に依存してはいけない
- **Bounded Context**: Domain-Driven Design の概念
- **Composition Root**: DI コンテナの構築場所

---

## 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-09-10 | Infrastructure層の Repository セクションを拡充。2つの基底クラスパターンを確立：①**RepositoryBase** - 単一テーブル集約向け（IEntityMapper実装）②**MultiTableRepositoryBase** - 複数テーブル集約向け（複雑なロジック対応）。Mapper と Repository の責務分離を明確化：Mapper は純粋な型変換のみ（Clock 依存なし、テスト容易性向上）、Repository が監査フィールド設定（CreatedAt/UpdatedAt/DeletedAt）を担当。実装例を Employee で具体化 |
| 2026-09-06 | Application層に「Context間のデータ共有」セクションを追加。ジェネリック Query Service パターン `IQueryService<TAggregate, TId>` を標準パターンとして採用。複数Contextがリアルタイムに異なるAggregateの情報をリアルタイムにアクセスするための依存方向が正しい設計。汎用Application層の肥大化を防止 |
| 2026-09-01 | LocalDateTime 使用規則を修正。「全層で LocalDateTime」という誤りを修正し、正しい層別責務を明記：Domain/Application は LocalDateTime、Infrastructure/DbModel は DateTime プリミティブ型、Mapper が双方向変換。実装例を EmployeeMapper に合わせて更新。外部システムからの DateTime 変換のパターンを Mapper での変換例に改善 |
| 2026-07-31（後）| Application層の説明に「汎用Application vs Bounded Context別Application」の区別を明記。マトリックスで「Application → Application」がインターフェース実装パターン（汎用層→Context別層）として許可されることを明確化。実装検査時に見つかった CarPreferences.Application が Application.UseCases に依存する件について、正当な設計パターンであることをドキュメントで担保 |
| 2026-07-31 | 実コード（各 `.csproj` の `ProjectReference` / `using` 宣言）との不一致を修正。①Common⇔SharedKernelの依存方向を実装に合わせて反転（Common起点に修正）②Crosscutting→Infrastructureの循環参照定義を削除しInfrastructure→Crosscuttingの一方向に統一③冒頭図をInfrastructureが最内層に見える誤った表現から同心円型に修正④編集し忘れの記述（「✓ 修正済み」）を削除⑤自動検証をSlnArch（未検証）からNetArchTest.Rulesの具体的なテストコード例に置き換え |
| 2026-07-09 | 初版作成。各層の責務と依存関係を定義 |

