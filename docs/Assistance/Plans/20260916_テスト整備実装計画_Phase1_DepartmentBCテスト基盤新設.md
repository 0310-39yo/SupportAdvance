# Phase 1: Department BC テストプロジェクト新設 実装計画

**作成日:** 2026-09-16
**対象読者:** 本タスクを実装する担当者（人間 または 別セッションのAIエージェント）
**位置づけ:** テスト・テスト仕様書整備プロジェクトの Phase 1。**Phase 0（整合性修正）の完了後**に着手すること。
**前提知識:** リポジトリのルート `CLAUDE.md`（クリーンアーキテクチャ原則）、`src/Contexts/CLAUDE.md`（Bounded Context の命名・依存ルール）を事前に読むこと。

---

## 0. 背景・目的

Department Bounded Context (`src/Contexts/Department/`) は Domain 層のみテストプロジェクト（`tests/Contexts/Department.Domain.Tests`）が存在し、**Application層・Infrastructure層にはテストプロジェクト自体が存在しない**（`.csproj` が無い）。Employee BC には対応する `Employee.Application.Tests` / `Employee.Infrastructure.Tests` が存在する（Phase 0 でビルド修復・slnx登録済みの前提）ため、Department BC にも同様の構成を新設し、テスト対象クラスの単体テストを追加する。

## 1. 対象ソースクラス

### Department.Application (`src/Contexts/Department/Department.Application/`)

| クラス | ファイル | 種別 |
|---|---|---|
| `DepartmentQueryService` | `Queries/DepartmentQueryService.cs` | DomainService/汎用（ジェネリック Query Service パターン実装） |

`DepartmentQueryService` は `IQueryService<Department, DepartmentRowId>` を実装し、コンストラクタで `IDepartmentRepository` を受け取る。公開メソッドは `GetByIdAsync(DepartmentRowId id)` のみで、`id` の null チェック後、`_repository.GetByIdAsync(id)` に委譲するだけの薄いクラス。

### Department.Infrastructure (`src/Contexts/Department/Department.Infrastructure/`)

| クラス | ファイル | 種別 |
|---|---|---|
| `DepartmentMapper` | `Mappers/DepartmentMapper.cs` | 汎用（Mapper、DB非依存の純粋変換ロジック） |
| `DepartmentRepository` | `Repositories/DepartmentRepository.cs` | 汎用（Repository実装、Dapper/RepoDb使用、DB接続が必要） |

`DepartmentMapper` の主要メソッド:
- `ToDomainEntity(DepartmentDbModel dbModel)`: `DepartmentRowId`/`DepartmentCode`/`HierarchyLevel`/`ParentDepartmentRowId`/`ManagerEmployeeRowId`/`AbolishedOn` それぞれに `TryFromDbValue` を呼び、失敗時は `InvalidOperationException` を投げてから `Department.Reconstruct(...)` を呼ぶ（Mapper層のnull吸収パターン。ルート `CLAUDE.md` の「null 厳格性原則」参照）
- `ToDbModel(Department entity)`: Domain → DbModel 変換。監査フィールド（CreatedAt/UpdatedAt等）は設定しない（Repositoryの責務）

`DepartmentRepository` のコンストラクタ: `DepartmentRepository(SqlQueryLoader queryLoader, DepartmentMapper mapper, IDbConnectionFactory connectionFactory, ICurrentUserService currentUser, IClock clock)`（`IDepartmentRepository` を実装）。メソッド: `GetByIdAsync`, `GetByCodeAsync`, `GetAllAsync`, `SaveAsync`（新規/更新判定は `dbModel.CreatedAt == default` で分岐し、監査フィールドを設定してから RepoDb の `InsertAsync`/`UpdateAsync`）, `DeleteAsync`（論理削除、`DeletedAt`/`DeletedBy` を設定して `UpdateAsync`）。実SQL Server接続が前提の実装。

## 2. 新設するテストプロジェクト

### 2-1. `tests/Contexts/Department.Application.Tests`

`tests/Contexts/Employee.Application.Tests/Employee.Application.Tests.csproj`（Phase 0 でビルド修復済みの前提）の構成を踏襲する。以下の内容で新規作成する:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="10.0.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.8.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../../src/Application/Application.csproj" />
    <ProjectReference Include="../../../src/Common/Common.csproj" />
    <ProjectReference Include="../../../src/SharedKernel/SharedKernel.csproj" />
    <ProjectReference Include="../../../src/Contexts/Department/Department.Domain/Department.Domain.csproj" />
    <ProjectReference Include="../../../src/Contexts/Department/Department.Application/Department.Application.csproj" />
  </ItemGroup>

</Project>
```

（パス階層は `tests/Contexts/Department.Application.Tests/` からの相対パス。実際に作成する際は `tests/Contexts/Employee.Application.Tests/Employee.Application.Tests.csproj` を `Read` して最新の内容を確認し、パッケージバージョン等に差異があればそちらを正とすること。）

作成するテストファイル: `tests/Contexts/Department.Application.Tests/Queries/DepartmentQueryServiceTests.cs`

- `GetByIdAsync` が `IDepartmentRepository.GetByIdAsync` に委譲することを検証（`IDepartmentRepository` のテストダブルを用意）
- `id` が null の場合 `ArgumentNullException`
- Repository が `null` を返す場合（該当なし）、そのまま `null` を返すこと
- Repository が `Department` を返す場合、そのまま返すこと

参考実装パターン: `tests/Contexts/Employee.Application.Tests/UseCases/GetEmployeeByIdUseCaseTests.cs`（`IEmployeeRepository` のテストダブルの作り方）

### 2-2. `tests/Contexts/Department.Infrastructure.Tests`

`tests/Contexts/Employee.Infrastructure.Tests/Employee.Infrastructure.Tests.csproj` の構成を踏襲する:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="10.0.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.8.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../../src/Application/Application.csproj" />
    <ProjectReference Include="../../../src/Common/Common.csproj" />
    <ProjectReference Include="../../../src/SharedKernel/SharedKernel.csproj" />
    <ProjectReference Include="../../../src/Infrastructure/Infrastructure.csproj" />
    <ProjectReference Include="../../../src/Contexts/Department/Department.Domain/Department.Domain.csproj" />
    <ProjectReference Include="../../../src/Contexts/Department/Department.Application/Department.Application.csproj" />
    <ProjectReference Include="../../../src/Contexts/Department/Department.Infrastructure/Department.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

作成するテストファイル:

1. `tests/Contexts/Department.Infrastructure.Tests/Mappers/DepartmentMapperTests.cs`
   - `ToDomainEntity`: 正常なDbModelから正しくEntityが復元されること、各ValueObjectへの変換（`TryFromDbValue` 失敗時に `InvalidOperationException`）を検証
   - `ToDbModel`: EntityからDbModelへの変換で、`ParentId`/`ManagerId` が `IsSet=false` の場合に `null` になること、`AbolishedOn.IsAbolished=false` の場合に `null` になること、監査フィールドが設定されないこと
   - DB接続不要な純粋ロジックのため、通常の単体テストとして問題なく作成できる（`Employee.Infrastructure.Tests` の `Mappers/EmployeeMapperTests.cs` と同様のテストしやすさ）

2. `tests/Contexts/Department.Infrastructure.Tests/Repositories/DepartmentRepositoryTests.cs`
   - `DepartmentRepository` は `SqlQueryLoader` + `IDbConnectionFactory`（実SQL Server接続）に依存する実装であり、**Phase 0 の `EmployeeRepository` と同様の理由でDB接続なしの単体テストが困難**。以下いずれかの方針で対応する:
     - (a) `SaveAsync`/`DeleteAsync` 内の「監査フィールド設定ロジック」（`_clock.JstNow`/`_currentUser.EmployeeRowId` を使う部分）だけを、`DepartmentRepository` から独立したテスト可能な形に切り出せないか検討する（ただし本Phaseの主目的はテスト新設であり、大きなリファクタリングは避ける）
     - (b) 最低限、コンストラクタの null ガード（`queryLoader`/`mapper`/`connectionFactory`/`currentUser`/`clock` いずれかが null の場合に `ArgumentNullException`）のみを単体テストとして書き、本体のCRUDロジックは `[Fact(Skip = "要DB接続。Phase 5で結合テストとして検証")]` として明示的に保留する
   - (b) を採用し、Phase 5（結合テスト仕様書）で実際のCRUD検証を行う方針を推奨する。

## 3. slnx への登録

新設した2プロジェクトを `SupportAdvance.slnx` の `<Folder Name="/tests/Contexts/">`（58行目以降のブロック、`Employee.Domain.Tests`/`Department.Domain.Tests` が登録されている方）に追加する:

```xml
<Project Path="tests/Contexts/Department.Application.Tests/Department.Application.Tests.csproj" />
<Project Path="tests/Contexts/Department.Infrastructure.Tests/Department.Infrastructure.Tests.csproj" />
```

**Phase 0 で得られた教訓**: 新規テストプロジェクトは csproj 作成だけでなく、必ず `.slnx` への登録まで行うこと。登録漏れがあると `dotnet build`/`dotnet test`（ソリューション単位）で静かに除外される。

## 4. ドキュメント作成

`docs/Assistance/Templates/Test/TEMPLATE_単体テスト仕様書_汎用.md`（`DomainService`/Mapper/Repositoryはこれを使う。`DepartmentQueryService`はDomainServiceテンプレート `TEMPLATE_単体テスト仕様書_DomainService.md` でもよいが、実体はシンプルな委譲のみなので汎用テンプレートで十分）を使用し、以下を作成する:

- `docs/Contexts/Department/Application/Application_単体テスト仕様書.md`（`DepartmentQueryService` を記載。`docs/Contexts/Employee/Application/Application_単体テスト仕様書.md` の「1プロジェクト1ファイルに全対象をまとめる」形式を踏襲）
- `docs/Contexts/Department/Infrastructure/Infrastructure_単体テスト仕様書.md`（`DepartmentMapper`/`DepartmentRepository` を記載。同様に1ファイルにまとめる形式。Repositoryのskipしたテストケースについても「未実施（要結合テスト）」として観点一覧に記載すること）

## 5. 検証方法

```bash
dotnet build --no-incremental
dotnet test
```

- 新設2プロジェクトが `SupportAdvance.slnx` 経由でビルド・実行されること
- `[Fact(Skip = "...")]` としたテストがある場合、その理由がドキュメント側にも明記されていること
- ドキュメントが `docs/` 以下で `src/` のフォルダ構成（`Contexts/Department/Application`, `Contexts/Department/Infrastructure`）と対応していること

## 6. 本Phaseでやらないこと

- Department.Domain のテスト・ドキュメント追加（Phase 2 で対応。Domain層のテストコードは既に存在し green、ドキュメントのみ Phase 2 で追加）
- `DepartmentRepository` の実SQL接続を要する統合的な検証（Phase 5）
