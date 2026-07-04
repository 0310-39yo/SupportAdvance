# マルチコンテキスト設計パターン - SupportAdvance

**最終更新**: 2026-06-30

---

## 目次

1. [マルチコンテキストとは](#マルチコンテキストとは)
2. [Context間の通信](#context間の通信)
3. [依存関係ルール](#依存関係ルール)
4. [現在のContext構成](#現在のcontext構成)
5. [新規Context追加ガイド](#新規context追加ガイド)
6. [設計パターン](#設計パターン)

---

## マルチコンテキストとは

### 定義

**Context** は、独立したビジネスドメインの実装単位です。

```
SupportAdvance (統合システム)
├─ CarPreferences Context    ← 車両設定管理
│  ├─ Domain
│  ├─ Application
│  └─ Infrastructure
│
├─ [Future Context 1]        ← 別のビジネスドメイン
├─ [Future Context 2]        ← さらに別のドメイン
│
└─ 共有層
   ├─ Common
   ├─ Crosscutting
   └─ Presentation
```

### 目的

- ✅ **責務分離**: 異なるビジネスドメインを独立管理
- ✅ **スケーラビリティ**: 新ドメイン追加が容易
- ✅ **保守性**: Context内の変更が他に影響しない
- ✅ **チーム分割**: 複数チームが並行開発可能

### 利点

```
❌ モノリシック設計:
   すべての機能が1つのプロジェクト
   → 変更の影響範囲が広い

✅ マルチコンテキスト設計:
   ビジネスドメインごとに分離
   → 変更が局所化
```

---

## Context間の通信

### 通信方法

#### 1. Domain Event（ドメインイベント）

```
Context A
  └─ Domain で CarPreferenceUpdatedEvent 発行
     ↓ (非同期)
Context B
  └─ Application で イベント購読・処理

✅ 疎結合な通信方法
✅ 非同期対応可能
✅ Context間の直接参照なし
```

**実装例**:
```csharp
// Context A: Domain
public class CarPreference : Entity
{
    public void Update(PreferenceCategory category)
    {
        this.Category = category;
        
        // ドメインイベント発行
        this.RaiseDomainEvent(
            new CarPreferenceUpdatedEvent(this.Id, category));
    }
}

// Context A: Application
public class UpdateCarPreferenceUseCase
{
    public async Task Execute(UpdateRequest request)
    {
        var cp = await _repository.GetById(request.Id);
        cp.Update(request.Category);
        await _repository.Save(cp);
        
        // イベント発行
        foreach (var @event in cp.GetDomainEvents())
        {
            await _eventBus.Publish(@event);  // ← Context B へ通知
        }
    }
}

// Context B: Application でイベント購読
public class CarPreferenceUpdatedEventHandler
{
    public async Task Handle(CarPreferenceUpdatedEvent @event)
    {
        // Context B の処理
        await DoSomethingInContextB(@event.CarPreferenceId);
    }
}
```

#### 2. 共通DTO

```
Context A
  └─ Application が共通DTO返却
     ↓
Presentation層
  └─ 複数Context使用時に共通DTO経由

✅ Context間の直接参照なし
✅ 明確なコントラクト
```

**実装例**:
```csharp
// Common or Shared層で定義
public class SharedCarPreferenceDto
{
    public int Id { get; set; }
    public string Model { get; set; }
    public string Category { get; set; }
}

// Context A でDTO返却
public class GetCarPreferenceResponse
{
    public SharedCarPreferenceDto Data { get; set; }
}

// Context B でも同じDTO参照
```

#### 3. Infrastructure統合

```
複数Context共通のインフラ
  ├─ DB接続
  ├─ キャッシュ
  └─ メッセージキュー

✅ 技術インフラの共有
✅ ビジネスロジック分離
```

**実装例**:
```csharp
// Infrastructure層: 共有DB接続
public class SharedDbConnection : IDbConnection
{
    // 複数Context が使用
}

// Context A: 独立したRepository
public class CarPreferenceRepository : ICarPreferenceRepository
{
    private readonly IDbConnection _connection;  // 共有
}

// Context B: 独立したRepository
public class AnotherRepository : IAnotherRepository
{
    private readonly IDbConnection _connection;  // 同じ接続
}
```

---

## 依存関係ルール

### ❌ 禁止されるContext間通信

```
❌ 直接参照:
Context A Project → Context B Project

❌ 循環依存:
Context A → Context B → Context A

❌ Application層での通信:
CarPreferences.Application → Other.Application
```

### ✅ 許可されるContext間通信

```
✅ イベント経由:
CarPreferences.Domain
  → DomainEvent発行
    → EventBus
      → Other.Application でハンドル

✅ 共通層経由:
CarPreferences.Application
  → Common (共通DTO)
    → Other.Application

✅ 共有インフラ:
CarPreferences.Infrastructure
  → Infrastructure (共有DB等)
    ← Other.Infrastructure
```

---

## 現在のContext構成

### CarPreferences Context

```
Contexts/Samples/CarPreferences/
├─ Domain/
│  ├─ Entities/
│  │  └─ CarPreference.cs
│  ├─ ValueObjects/
│  │  └─ PreferenceCategory.cs
│  ├─ DomainEvents/
│  │  └─ CarPreferenceUpdatedEvent.cs
│  └─ CarPreferences.Domain.csproj
│     参照: SharedKernel, Common
│
├─ Application/
│  ├─ UseCases/
│  │  ├─ Get/
│  │  ├─ Create/
│  │  ├─ Update/
│  │  └─ Delete/
│  ├─ Dto/
│  │  └─ CarPreferenceDto.cs
│  ├─ DependencyInjection.cs
│  └─ CarPreferences.Application.csproj
│     参照: Application, CarPreferences.Domain, Common
│
└─ Infrastructure/
   ├─ Repositories/
   │  └─ CarPreferenceRepository.cs
   ├─ DependencyInjection.cs
   └─ CarPreferences.Infrastructure.csproj
      参照: Infrastructure, CarPreferences.Application, Common
```

### ディレクトリツリー

```
src/
├─ 共有層 (全Context共通)
│  ├─ Common/
│  ├─ Crosscutting/
│  ├─ Application/
│  ├─ Infrastructure/
│  └─ Presentation/
│
└─ Contexts/Samples/
   ├─ CarPreferences/
   │  ├─ Domain/
   │  ├─ Application/
   │  └─ Infrastructure/
   │
   └─ [NewContext]/ (追加予定)
      ├─ Domain/
      ├─ Application/
      └─ Infrastructure/
```

---

## 新規Context追加ガイド

### ステップ1: ディレクトリ作成

```
Contexts/Samples/[NewContextName]/
├─ Domain/
│  ├─ Entities/
│  ├─ ValueObjects/
│  ├─ DomainEvents/
│  ├─ Exceptions/
│  └─ [NewContextName].Domain.csproj
│
├─ Application/
│  ├─ UseCases/
│  ├─ Dto/
│  ├─ DependencyInjection.cs
│  └─ [NewContextName].Application.csproj
│
└─ Infrastructure/
   ├─ Repositories/
   ├─ DependencyInjection.cs
   └─ [NewContextName].Infrastructure.csproj
```

### ステップ2: .csproj ファイル作成

**[NewContextName].Domain.csproj**:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference 
      Include="..\..\..\SharedKernel\SharedKernel.csproj" />
    <ProjectReference 
      Include="..\..\..\Common\Common.csproj" />
  </ItemGroup>
</Project>
```

**[NewContextName].Application.csproj**:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference 
      Include="..\..\..\Application\Application.csproj" />
    <ProjectReference 
      Include="..\Domain\[NewContextName].Domain.csproj" />
    <ProjectReference 
      Include="..\..\..\Common\Common.csproj" />
  </ItemGroup>
</Project>
```

**[NewContextName].Infrastructure.csproj**:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference 
      Include="..\..\..\Infrastructure\Infrastructure.csproj" />
    <ProjectReference 
      Include="..\Application\[NewContextName].Application.csproj" />
    <ProjectReference 
      Include="..\Domain\[NewContextName].Domain.csproj" />
    <ProjectReference 
      Include="..\..\..\Common\Common.csproj" />
  </ItemGroup>
</Project>
```

### ステップ3: Domain層実装

```csharp
// [NewContextName].Domain/Entities/[Entity].cs
public class [Entity] : Entity
{
    public string Name { get; private set; }
    
    public [Entity](int id, string name)
    {
        this.Id = id;
        this.Name = name;
    }
}
```

### ステップ4: Application層実装

```csharp
// [NewContextName].Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection Add[NewContextName]ApplicationModels(
        this IServiceCollection services)
    {
        services.AddScoped<
            IUseCase<Get[Entity]Request, Get[Entity]Response>,
            Get[Entity]UseCase>();
        
        return services;
    }
}
```

### ステップ5: Infrastructure層実装

```csharp
// [NewContextName].Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection Add[NewContextName]InfrastructureModels(
        this IServiceCollection services)
    {
        services.AddScoped<
            I[Entity]Repository,
            [Entity]Repository>();
        
        return services;
    }
}
```

### ステップ6: Presentation層に登録

```csharp
// Presentation.Shared/HostBuilderFactory.cs
.ConfigureServices((context, services) =>
{
    // ... 既存
    services.AddCarPreferencesApplicationModels();
    services.AddCarPreferencesInfrastructureModels();
    
    // 新規Context追加
    services.Add[NewContextName]ApplicationModels();     // ← 追加
    services.Add[NewContextName]InfrastructureModels(); // ← 追加
})
```

---

## 設計パターン

### パターン1：独立Context

```
各Context完全独立
├─ 別ビジネスドメイン
├─ 別DB/データストア
└─ 共通層(Common/Crosscutting)のみ参照

✅ 最も疎結合
✅ チーム分割に最適
```

### パターン2：イベント駆動

```
Context A
  └─ ビジネスイベント発行
     ↓ (イベントバス)
Context B
  └─ イベント購読・反応

✅ 非同期対応
✅ Context間の依存なし
```

### パターン3：Shared Kernel

```
SharedKernel
├─ エンティティ基底
├─ 値オブジェクト基底
└─ ドメインイベント基底

全Context が参照 ✅
```

---

## チェックリスト（Context追加時）

- [ ] ディレクトリ構造作成
- [ ] [ContextName].Domain.csproj 作成・参照設定
- [ ] [ContextName].Application.csproj 作成・参照設定
- [ ] [ContextName].Infrastructure.csproj 作成・参照設定
- [ ] 依存関係は正しい方向か確認
- [ ] Domain層実装
- [ ] Application層実装（DependencyInjection含む）
- [ ] Infrastructure層実装（DependencyInjection含む）
- [ ] Presentation層で DI登録
- [ ] ビルド確認
- [ ] テスト実装

---

## よくある質問

### Q: 複数Contextが同じ Entity を使いたい場合？

**A**: 以下のいずれか

1. **SharedKernel に共通Entity配置**（推奨）
2. **各Context が独立Entity実装**（正式）
3. **イベント経由で共有**

### Q: Context間でデータ共有は？

**A**: イベント駆動が推奨

```csharp
// Context A が変更
cp.Update(category);

// ドメインイベント発行
_eventBus.Publish(new CarPreferenceUpdatedEvent(...));

// Context B がイベント購読
eventHandler.Handle(event);
```

### Q: 新しいContext がいつ必要？

**A**: 以下の場合

1. 独立したビジネスドメイン
2. 別チームが開発
3. 別DB/データストア
4. ビジネスロジックが異なる

---

**作成日**: 2026-06-30  
**参考**: 01-Layer-Architecture.md / 02-Dependency-Rules.md
