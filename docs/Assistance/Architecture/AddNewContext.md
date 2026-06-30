# 新規Context追加ガイド - SupportAdvance

**最終更新**: 2026-06-30

---

## 目次

1. [概要](#概要)
2. [全体フロー](#全体フロー)
3. [詳細手順](#詳細手順)
4. [チェックリスト](#チェックリスト)
5. [トラブルシューティング](#トラブルシューティング)

---

## 概要

このガイドでは、新規ビジネスドメイン（Context）を SupportAdvance に追加する手順を説明します。

### 前提

- Visual Studio または Visual Studio Code
- git の基本知識
- AGENTS.md および 06-Multi-Context-Design-Pattern.md を理解済み

---

## 全体フロー

```
1. ディレクトリ構造作成
   ↓
2. Domain層実装
   ├─ Entity
   ├─ ValueObject
   └─ DomainEvent
   ↓
3. Application層実装
   ├─ UseCase
   ├─ Request/Response DTO
   └─ DependencyInjection
   ↓
4. Infrastructure層実装
   ├─ Repository実装
   └─ DependencyInjection
   ↓
5. Presentation層登録
   └─ HostBuilderFactory.cs 修正
   ↓
6. ビルド・テスト
```

---

## 詳細手順

### 手順1: ディレクトリ・プロジェクト作成

#### 1-1. ディレクトリ構造

```
# PowerShell / Bash で実行

mkdir -p src/Contexts/Samples/[NewContextName]/Domain
mkdir -p src/Contexts/Samples/[NewContextName]/Domain/Entities
mkdir -p src/Contexts/Samples/[NewContextName]/Domain/ValueObjects
mkdir -p src/Contexts/Samples/[NewContextName]/Domain/DomainEvents

mkdir -p src/Contexts/Samples/[NewContextName]/Application
mkdir -p src/Contexts/Samples/[NewContextName]/Application/UseCases/Get
mkdir -p src/Contexts/Samples/[NewContextName]/Application/UseCases/Create
mkdir -p src/Contexts/Samples/[NewContextName]/Application/UseCases/Update
mkdir -p src/Contexts/Samples/[NewContextName]/Application/UseCases/Delete
mkdir -p src/Contexts/Samples/[NewContextName]/Application/Dto

mkdir -p src/Contexts/Samples/[NewContextName]/Infrastructure
mkdir -p src/Contexts/Samples/[NewContextName]/Infrastructure/Repositories
```

#### 1-2. Visual Studio でプロジェクト追加

```
Visual Studio → ソリューション右クリック
  → 追加 → 新しいプロジェクト

3つのプロジェクトを作成:
1. [NewContextName].Domain
2. [NewContextName].Application
3. [NewContextName].Infrastructure

各プロジェクト:
  - プロジェクトテンプレート: クラスライブラリ
  - フレームワーク: .NET 10.0
  - 配置先: src/Contexts/Samples/[NewContextName]/[Layer]/
```

### 手順2: Domain層実装

#### 2-1. [NewContextName].Domain.csproj 編集

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\SharedKernel\SharedKernel.csproj" />
    <ProjectReference Include="..\..\..\Common\Common.csproj" />
  </ItemGroup>

</Project>
```

#### 2-2. Entity作成

```csharp
// Entities/[Entity].cs
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Samples.[NewContextName].Domain.Entities;

public class [Entity] : Entity
{
    public string Name { get; private set; }
    
    // Factory method
    public static [Entity] Create(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new DomainException("名前は必須です");
        
        return new [Entity] { Name = name };
    }
    
    // Business logic
    public void UpdateName(string newName)
    {
        if (string.IsNullOrEmpty(newName))
            throw new DomainException("名前は必須です");
        
        this.Name = newName;
    }
}
```

#### 2-3. ValueObject作成（必要に応じて）

```csharp
// ValueObjects/[ValueObject].cs
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Samples.[NewContextName].Domain.ValueObjects;

public class [ValueObject] : ValueObject
{
    public string Value { get; }
    
    public [ValueObject](string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new DomainException("値は必須です");
        
        this.Value = value;
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

#### 2-4. DomainEvent作成（必要に応じて）

```csharp
// DomainEvents/[Entity]CreatedEvent.cs
using SupportAdvance.SharedKernel.DomainEvents;

namespace SupportAdvance.Contexts.Samples.[NewContextName].Domain.DomainEvents;

public class [Entity]CreatedEvent : DomainEvent
{
    public int [Entity]Id { get; }
    public string Name { get; }
    
    public [Entity]CreatedEvent(int entityId, string name)
    {
        [Entity]Id = entityId;
        Name = name;
    }
}
```

### 手順3: Application層実装

#### 3-1. [NewContextName].Application.csproj 編集

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\Application\Application.csproj" />
    <ProjectReference Include="..\Domain\[NewContextName].Domain.csproj" />
    <ProjectReference Include="..\..\..\Common\Common.csproj" />
  </ItemGroup>

</Project>
```

#### 3-2. DTO作成

```csharp
// Dto/[Entity]Dto.cs
namespace SupportAdvance.Contexts.Samples.[NewContextName].Application.Dto;

public class [Entity]Dto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

#### 3-3. Get UseCase作成

```csharp
// UseCases/Get/Get[Entity]Request.cs
using SupportAdvance.Application.UseCases;

namespace SupportAdvance.Contexts.Samples.[NewContextName].Application.UseCases.Get;

public class Get[Entity]Request : IRequest
{
    public int Id { get; set; }
}

// UseCases/Get/Get[Entity]Response.cs
using SupportAdvance.Application.UseCases;
using SupportAdvance.Contexts.Samples.[NewContextName].Application.Dto;

public class Get[Entity]Response : IResponse
{
    public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }
    public [Entity]Dto? Data { get; private set; }
    
    private Get[Entity]Response() { }
    
    public static Get[Entity]Response Success([Entity]Dto data)
        => new() { IsSuccess = true, Data = data };
    
    public static Get[Entity]Response NotFound()
        => new() { IsSuccess = false, Message = "データが見つかりません" };
    
    public static Get[Entity]Response Error(string message)
        => new() { IsSuccess = false, Message = message };
}

// UseCases/Get/Get[Entity]UseCase.cs
using Microsoft.Extensions.Logging;
using SupportAdvance.Application.UseCases;

public class Get[Entity]UseCase 
    : IUseCase<Get[Entity]Request, Get[Entity]Response>
{
    private readonly I[Entity]Repository _repository;
    private readonly ILogger<Get[Entity]UseCase> _logger;
    
    public Get[Entity]UseCase(
        I[Entity]Repository repository,
        ILogger<Get[Entity]UseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<Get[Entity]Response> Execute(Get[Entity]Request request)
    {
        try
        {
            var entity = await _repository.GetById(request.Id);
            if (entity == null)
                return Get[Entity]Response.NotFound();
            
            var dto = new [Entity]Dto
            {
                Id = entity.Id,
                Name = entity.Name
            };
            
            return Get[Entity]Response.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting [Entity]");
            return Get[Entity]Response.Error("エラーが発生しました");
        }
    }
}
```

#### 3-4. DependencyInjection.cs 作成

```csharp
// DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Application.UseCases;

namespace SupportAdvance.Contexts.Samples.[NewContextName].Application;

public static class DependencyInjection
{
    public static IServiceCollection Add[NewContextName]ApplicationModels(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        // Get
        services.AddScoped<
            IUseCase<Get[Entity]Request, Get[Entity]Response>,
            Get[Entity]UseCase>();
        
        // Create, Update, Delete も同様に登録
        
        return services;
    }
}
```

### 手順4: Infrastructure層実装

#### 4-1. [NewContextName].Infrastructure.csproj 編集

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Dapper" Version="2.1.79" />
    <PackageReference Include="RepoDB.SqlServer" Version="1.14.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\Infrastructure\Infrastructure.csproj" />
    <ProjectReference Include="..\Application\[NewContextName].Application.csproj" />
    <ProjectReference Include="..\Domain\[NewContextName].Domain.csproj" />
    <ProjectReference Include="..\..\..\Common\Common.csproj" />
  </ItemGroup>

</Project>
```

#### 4-2. Repository実装

```csharp
// Repositories/[Entity]Repository.cs
using System.Data;
using Dapper;
using SupportAdvance.Contexts.Samples.[NewContextName].Domain.Entities;

namespace SupportAdvance.Contexts.Samples.[NewContextName].Infrastructure.Repositories;

public class [Entity]Repository : I[Entity]Repository
{
    private readonly IDbConnection _connection;
    
    public [Entity]Repository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<[Entity]?> GetById(int id)
    {
        const string sql = "SELECT * FROM [Entity] WHERE Id = @id";
        return await _connection.QueryFirstOrDefaultAsync<[Entity]>(
            sql, new { id });
    }
    
    public async Task<IEnumerable<[Entity]>> GetAll()
    {
        const string sql = "SELECT * FROM [Entity]";
        return await _connection.QueryAsync<[Entity]>(sql);
    }
    
    public async Task Save([Entity] entity)
    {
        const string sql = @"
            INSERT INTO [Entity] (Name) VALUES (@name)
            UPDATE [Entity] SET Name = @name WHERE Id = @id";
        
        await _connection.ExecuteAsync(sql, entity);
    }
    
    public async Task Delete([Entity] entity)
    {
        const string sql = "DELETE FROM [Entity] WHERE Id = @id";
        await _connection.ExecuteAsync(sql, new { entity.Id });
    }
}
```

#### 4-3. DependencyInjection.cs 作成

```csharp
// DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;

namespace SupportAdvance.Contexts.Samples.[NewContextName].Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection Add[NewContextName]InfrastructureModels(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddScoped<
            I[Entity]Repository,
            [Entity]Repository>();
        
        return services;
    }
}
```

### 手順5: Presentation層登録

#### 5-1. HostBuilderFactory.cs 修正

```csharp
// Presentation.Shared/HostBuilderFactory.cs

using SupportAdvance.Contexts.Samples.[NewContextName].Application;      // ← 追加
using SupportAdvance.Contexts.Samples.[NewContextName].Infrastructure;  // ← 追加

public static IHostBuilder Create(...)
{
    return Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services
                .AddCrosscuttingModels(context.Configuration)
                .AddInfrastructureModels(context.Configuration)
                .AddCarPreferencesApplicationModels()
                .AddCarPreferencesInfrastructureModels()
                .Add[NewContextName]ApplicationModels()              // ← 追加
                .Add[NewContextName]InfrastructureModels()          // ← 追加
                .AddWinTrialModules();
        });
}
```

### 手順6: ビルド・テスト

```powershell
# ビルド
dotnet build

# テスト
dotnet test

# 実行
dotnet run --project src/Presentation/WinTrial/WinTrial.csproj
```

---

## チェックリスト

### Domain層

- [ ] [NewContextName].Domain.csproj 作成
- [ ] Entity クラス実装（SharedKernel.Entity継承）
- [ ] ValueObject 実装（必要に応じて）
- [ ] DomainEvent 実装（必要に応じて）
- [ ] ビルド確認

### Application層

- [ ] [NewContextName].Application.csproj 作成
- [ ] DTO実装
- [ ] UseCase実装（Get, Create, Update, Delete）
- [ ] Request/Response実装
- [ ] DependencyInjection.cs 作成
- [ ] ビルド確認

### Infrastructure層

- [ ] [NewContextName].Infrastructure.csproj 作成
- [ ] Repository実装
- [ ] DependencyInjection.cs 作成
- [ ] ビルド確認

### Presentation層

- [ ] HostBuilderFactory.cs 修正
- [ ] using文追加
- [ ] DI登録呼び出し追加
- [ ] ビルド・実行確認

### 全体

- [ ] git で変更コミット
- [ ] 依存関係確認（依存性ルール準拠）
- [ ] ユニットテスト実装

---

## トラブルシューティング

### Q: "型を解決できない" というビルドエラー

**原因**: using文忘れまたはプロジェクト参照なし

**解決**:
1. using文確認
2. .csproj の ProjectReference 確認
3. ビルド → クリーン

### Q: "循環参照" という警告

**原因**: Context間の不正な参照

**解決**: 06-Multi-Context-Design-Pattern.md の「依存関係ルール」確認

### Q: DB接続エラー

**原因**: appsettings.json の接続文字列誤り

**解決**:
```json
{
  "AppSettings": {
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=[ContextName]DB;..."
    }
  }
}
```

---

**作成日**: 2026-06-30  
**参考**: 06-Multi-Context-Design-Pattern.md / 01-Layer-Architecture.md
