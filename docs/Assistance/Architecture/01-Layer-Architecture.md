# 層アーキテクチャの詳細 - SupportAdvance

**最終更新**: 2026-06-30

---

## 目次

1. [Presentation層](#presentation層)
2. [Application層](#application層)
3. [Domain層](#domain層)
4. [Infrastructure層](#infrastructure層)
5. [Crosscutting層](#crosscutting層)
6. [Common層](#common層)

---

## Presentation層

### 責務

- ✅ UI実装（Windows Forms / WPF）
- ✅ DI設定・依存関係解決
- ✅ UI共通機能（デコレーター等）
- ✅ ユーザーイベント入力の受け付け

### 禁止事項

- ❌ ビジネスロジック実装
- ❌ DB操作（直接SQL等）
- ❌ Infrastructure依存（DI設定以外）

### プロジェクト

| プロジェクト | 説明 |
|-----------|------|
| **Presentation.Shared** | DI設定、UI共通機能、デコレーター |
| **Presentation.WinTrial** | Windows Forms UI |
| **Presentation.WpfTrial** | WPF UI |

### 参照可能な層

```
Presentation
  → Application (UseCase呼び出し)
  → Common (設定参照)
  → Crosscutting (ロギング等)
```

### 実装例

```csharp
// Presentation.WinTrial/Program.cs
public static class Program
{
    [STAThread]
    private static void Main()
    {
        using var host = HostBuilderFactory.Create(...)
            .Build();
        
        // DI経由でUseCaseを取得
        var useCase = host.Services.GetRequiredService<ICarPreferenceUseCase>();
        
        // UseCase実行（Application層に委譲）
        var result = await useCase.Execute(request);
    }
}
```

---

## Application層

### 責務

- ✅ UseCase定義と実装
- ✅ DTO（Request/Response）定義
- ✅ ビジネスロジックの編成
- ✅ Domain層との通信

### 禁止事項

- ❌ Presentation層への参照
- ❌ DB操作（直接実装）
- ❌ UI操作

### プロジェクト

| プロジェクト | 説明 |
|-----------|------|
| **Application** | UseCase基盤、DTO基盤 |
| **CarPreferences.Application** | 車両設定のUseCase実装 |

### 参照可能な層

```
Application
  → Domain (ビジネスルール、エンティティ)
  → SharedKernel (基底クラス)
  → Common (設定)
```

### ファイル構成

```
Application/
├─ UseCases/
│  ├─ IUseCase.cs               ← 抽象インターフェース
│  ├─ IRequest.cs
│  └─ IResponse.cs
├─ DependencyInjection.cs        ← DI登録

CarPreferences.Application/
├─ UseCases/
│  ├─ GetCarPreferenceUseCase.cs
│  ├─ UpdateCarPreferenceUseCase.cs
│  └─ Dto/
│      ├─ GetCarPreferenceRequest.cs
│      └─ CarPreferenceResponse.cs
└─ DependencyInjection.cs
```

### 実装例

```csharp
// IUseCase インターフェース
public interface IUseCase<in TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IResponse
{
    Task<TResponse> Execute(TRequest request);
}

// UseCase実装
public class GetCarPreferenceUseCase 
    : IUseCase<GetCarPreferenceRequest, CarPreferenceResponse>
{
    private readonly ICarPreferenceRepository _repository;
    
    public async Task<CarPreferenceResponse> Execute(
        GetCarPreferenceRequest request)
    {
        // Domain層のビジネスルールを使用
        var carPreference = await _repository.GetById(request.Id);
        
        return new CarPreferenceResponse 
        { 
            Id = carPreference.Id,
            // ... mapping
        };
    }
}
```

---

## Domain層

### 責務

- ✅ ビジネスルール実装（ドメインロジック）
- ✅ エンティティ定義
- ✅ 値オブジェクト定義
- ✅ ドメインイベント定義

### 禁止事項

- ❌ Infrastructure参照
- ❌ Presentation参照
- ❌ Application参照
- ❌ 技術的詳細（SQL、ORM等）
- ❌ null使用

### プロジェクト

| プロジェクト | 説明 |
|-----------|------|
| **SharedKernel** | エンティティ基底、ドメインイベント基底 |
| **CarPreferences.Domain** | ビジネスルール（車両設定） |

### 参照可能な層

```
Domain
  → Common (設定、時刻等)
  → SharedKernel (基底クラス)
```

### ファイル構成

```
SharedKernel/
├─ Entities/
│  └─ Entity.cs                  ← エンティティ基底クラス
├─ ValueObjects/
│  └─ ValueObject.cs             ← 値オブジェクト基底
└─ DomainEvents/
   └─ DomainEvent.cs             ← ドメインイベント基底

CarPreferences.Domain/
├─ Entities/
│  └─ CarPreference.cs
├─ ValueObjects/
│  └─ PreferenceCategory.cs
└─ DomainEvents/
   └─ CarPreferenceUpdatedEvent.cs
```

### 実装例

```csharp
// エンティティ定義
public class CarPreference : Entity
{
    public string Model { get; private set; }
    public PreferenceCategory Category { get; private set; }
    
    // ビジネスルール（ドメインロジック）
    public void UpdatePreference(PreferenceCategory newCategory)
    {
        if (newCategory == null)
            throw new DomainException("カテゴリは必須です");
        
        this.Category = newCategory;
        
        // ドメインイベント発行
        this.RaiseDomainEvent(
            new CarPreferenceUpdatedEvent(this.Id, newCategory));
    }
}

// 値オブジェクト
public class PreferenceCategory : ValueObject
{
    public string Value { get; }
    
    public PreferenceCategory(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new DomainException("値は必須です");
        this.Value = value;
    }
}
```

---

## Infrastructure層

### 責務

- ✅ DB接続管理
- ✅ Repository実装（永続化）
- ✅ ORM設定（Dapper, RepoDB）
- ✅ 外部API連携

### 禁止事項

- ❌ Domain層への参照
- ❌ ビジネスロジック実装
- ❌ Presentation層への依存

### プロジェクト

| プロジェクト | 説明 |
|-----------|------|
| **Infrastructure** | DB接続、ORM設定（汎用） |
| **CarPreferences.Infrastructure** | Repository実装（Context固有） |

### 参照可能な層

```
Infrastructure
  → Application (インターフェース参照)
  → Crosscutting (ロギング等)
  → Common (設定)
```

### ファイル構成

```
Infrastructure/
├─ Data/
│  ├─ Connections/
│  │  └─ SqlConnection.cs
│  └─ Repositories/
│      └─ BaseRepository.cs
├─ ORM/
│  ├─ Dapper/
│  │  └─ DapperTypeHandlerRegistration.cs ✅ 移動完了
│  └─ RepoDB/
└─ DependencyInjection.cs

CarPreferences.Infrastructure/
├─ Repositories/
│  └─ CarPreferenceRepository.cs
└─ DependencyInjection.cs
```

### 実装例

```csharp
// Repositoryインターフェース（Application層定義）
public interface ICarPreferenceRepository
{
    Task<CarPreference> GetById(int id);
    Task Save(CarPreference carPreference);
}

// Repository実装（Infrastructure層）
public class CarPreferenceRepository : ICarPreferenceRepository
{
    private readonly IDbConnection _connection;
    
    public async Task<CarPreference> GetById(int id)
    {
        var sql = "SELECT * FROM CarPreferences WHERE Id = @id";
        return await _connection.QueryFirstAsync<CarPreference>(
            sql, new { id });
    }
    
    public async Task Save(CarPreference carPreference)
    {
        // RepoDB等で保存
    }
}
```

---

## Crosscutting層

### 責務

- ✅ ロギング機能（NLog統合）
- ✅ トランザクション管理（将来）
- ✅ 例外マッピング（将来）
- ✅ 横断的な関心事処理

### 禁止事項

- ❌ ビジネスロジック
- ❌ UI操作
- ❌ Domain層への依存

### プロジェクト

| プロジェクト | 説明 |
|-----------|------|
| **Crosscutting** | ロギング等の横断処理 |

### 参照可能な層

```
Crosscutting
  → Common (設定)
```

### ファイル構成

```
Crosscutting/
├─ Logging/
│  └─ NLogInitializer.cs
└─ DependencyInjection.cs
```

### 実装例

```csharp
// NLog初期化（アプリケーション起動時）
public static class NLogInitializer
{
    public static void Initialize(AppSettings appSettings)
    {
        var candidates = new[]
        {
            Path.Combine(appSettings.FolderName, "Configuration", "NLog.config"),
            Path.Combine(appSettings.FolderName, "NLog.config")
        };
        
        var existing = candidates.FirstOrDefault(File.Exists);
        if (existing != null)
        {
            LogManager.Setup().LoadConfigurationFromFile(existing);
        }
    }
}
```

---

## Common層

### 責務

- ✅ 設定値管理（AppSettings）
- ✅ 時刻取得抽象化（IClock）
- ✅ 純粋なユーティリティ

### 禁止事項

- ❌ 他層への参照
- ❌ 副作用のある処理（IO、DB等）

### プロジェクト

| プロジェクト | 説明 |
|-----------|------|
| **Common** | 設定、時刻、ユーティリティ |

### ファイル構成

```
Common/
├─ Configuration/
│  ├─ IAppSettings.cs
│  └─ AppSettings.cs ✅ 移動完了
├─ Clocks/
│  ├─ IClock.cs
│  └─ ClockSettings.cs ✅ 移動完了
└─ (副作用なし、他依存なし)
```

---

## 層間通信パターン

### 1. 上位層 → 下位層（通常）

```
Presentation (UseCase呼び出し)
  ↓
Application (ビジネスロジック実行)
  ↓
Domain (ビジネスルール適用)
  ↓
Repository (永続化)
```

### 2. 下位層 → 上位層（インターフェース経由）

```
Infrastructure (Repository実装)
  → Application (IRepository インターフェース経由)
```

### 3. 横断処理

```
全層 ← Crosscutting (ロギング等)
全層 ← Common (設定、時刻等)
```

---

## 設計上の留意点

### ✅ 良い例

```csharp
// Presentation がApplication経由でDomain操作
var response = await useCase.Execute(request);

// Domain がBusinessLogicのみ実装
carPreference.UpdatePreference(newCategory);

// Infrastructure がIRepository経由でApplication連携
await repository.Save(carPreference);
```

### ❌ 悪い例

```csharp
// Presentation がDB直接操作
using var conn = new SqlConnection(connectionString);
await conn.ExecuteAsync("UPDATE ...");

// Domain が SQL知識を持つ
public class CarPreference
{
    public string GenerateSqlUpdate() { ... }  // ❌
}

// Infrastructure が BusinessLogic実装
public class Repository
{
    public async Task Save(CarPreference cp)
    {
        // 複雑なビジネスロジック ❌
    }
}
```

---

**作成日**: 2026-06-30
