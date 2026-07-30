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
                 │ 実装                                  │ Program.cs（Composition Root）でのみ
                 └──────────────── 参照 ──────────────────┘
                Crosscutting はロギング等のインターフェース＋実装を自己完結で持つ横断的関心事
                （実装に必要な外部ライブラリはNuGetで直接取得。Infrastructureには依存しない）
```

**ポイント:**
- `Infrastructure` は最も内側ではなく、`Presentation` と対称な**最も外側の層**（実装の詳細）
- `Crosscutting` はロギング等のインターフェースと実装の両方を持つ（例：`IAppLogging<T>` とその NLog 実装 `FrameworkLoggingAdapter`）。実装に必要な NLog 等は NuGet パッケージとして Crosscutting が直接参照し、`Infrastructure` プロジェクトには依存しない。したがって参照方向は **Infrastructure → Crosscutting** の一方向のみ
- `Presentation` から `Infrastructure` への参照は、DIコンテナを組み立てる **Composition Root（`Program.cs`）に限定**される（詳細は後述の「自動検証の導入」参照）

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
- そのため参照方向は **Infrastructure → Crosscutting** の一方向のみ。Crosscutting → Infrastructure は禁止（循環参照になるため）
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
- Crosscutting（**ドメインイベント発行のみ**）

**禁止される参照:**
- Application
- Infrastructure
- Presentation

**例:**
```csharp
namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;

public class Car : Entity
{
    public CarModel Model { get; private set; }
    public DateTime PurchasedAt { get; private set; }

    public void UpdateModel(CarModel model)
    {
        // ビジネスルールを実装
        Model = model;
        RaiseDomainEvent(new CarModelUpdatedEvent(this.Id, model));
    }
}
```

---

### 5. **Application Layer** - Use Cases / Application Services

**プロジェクト:**
- `src/Application` - 汎用アプリケーションサービス
- `src/Contexts/Samples/CarPreferences.Application` - Use Cases（サンプル）

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

**禁止される参照:**
- Infrastructure（直接参照は禁止、DI により注入）
- Presentation

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

---

### 6. **Infrastructure Layer**

**プロジェクト:**
- `src/Infrastructure` - Database / External Service implementations

**責務:**
- データベース実装（Repository パターン）
- ORM 統合（Dapper, RepoDB など）
- 外部API/サービスの実装
- ファイルシステムアクセス
- Configuration プロバイダー

**特徴:**
- **最も外側のレイヤー**
- Application / Domain のインターフェースを実装
- 技術的な詳細を隠蔽

**許可される参照:**
- SharedKernel
- Common
- Crosscutting（ロギング）
- Domain（Entity/Value Object のマッピング用）

**禁止される参照:**
- Application
- Presentation

**例:**
```csharp
namespace SupportAdvance.Infrastructure.Data.Repositories;

public class CarPreferenceRepository : ICarPreferenceRepository
{
    private readonly IDbConnection _connection;

    public async Task<Car> GetByIdAsync(CarId id)
    {
        var result = await _connection.QuerySingleOrDefaultAsync<CarDto>(
            "SELECT * FROM cars WHERE id = @Id",
            new { Id = id.Value });

        return result?.ToDomain();
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
- Infrastructure（**Program.cs を除く**）

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
| **Domain** | ✓ | ✓ | ✓* | - | ✗ | ✗ | ✗ |
| **Application** | ✓ | ✓ | ✓ | ✓ | - | ✗ | ✗ |
| **Infrastructure** | ✓ | ✓ | ✓ | ✓ | ✗ | - | ✗ |
| **Presentation** | ✓ | ✓ | ✓ | ✗ | ✓ | ✓** | - |

- `✓` = 許可
- `✗` = 禁止
- `*` = Domain → Crosscutting：ドメインイベント発行のみ許可
- `**` = Presentation → Infrastructure：Program.cs（Composition Root）のみ許可。プロジェクト参照上は Infrastructure に到達可能な構成だが、`Program` 型を除く全ての型が Infrastructure 名前空間に依存しないことを [自動検証](#自動検証の導入) で担保する
- `Common` は依存ゼロの最内層。`Crosscutting → Infrastructure` は禁止（実装は Infrastructure が Crosscutting のインターフェースを実装する一方向のみ）

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

- `Presentation` → `Infrastructure` は `Program.cs`（Composition Root）のみ許可（例：`WinTrial.csproj` は DI 配線のため `CarPreferences.Infrastructure` を参照せざるを得ないが、`Program` 型以外がそれを使ってはならない）
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

    // 「Program.cs のみ Infrastructure 参照可」を型レベルで検証。
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
| 2026-07-31 | 実コード（各 `.csproj` の `ProjectReference` / `using` 宣言）との不一致を修正。①Common⇔SharedKernelの依存方向を実装に合わせて反転（Common起点に修正）②Crosscutting→Infrastructureの循環参照定義を削除しInfrastructure→Crosscuttingの一方向に統一③冒頭図をInfrastructureが最内層に見える誤った表現から同心円型に修正④編集し忘れの記述（「✓ 修正済み」）を削除⑤自動検証をSlnArch（未検証）からNetArchTest.Rulesの具体的なテストコード例に置き換え |
| 2026-07-09 | 初版作成。各層の責務と依存関係を定義 |

