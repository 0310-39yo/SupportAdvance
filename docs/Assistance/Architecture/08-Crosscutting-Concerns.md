# 横断処理（Crosscutting Concerns） - SupportAdvance

**最終更新**: 2026-06-30

---

## 目次

1. [横断処理とは](#横断処理とは)
2. [ロギング（NLog）](#ロギング-nlog)
3. [時刻取得（IClock）](#時刻取得-iclock)
4. [設定管理（AppSettings）](#設定管理-appsettings)
5. [現在の実装](#現在の実装)
6. [拡張パターン](#拡張パターン)

---

## 横断処理とは

### 定義

**Crosscutting Concern** は、複数の層・Context に共通する関心事です。

```
Domain層   ← ロギング
Application層 ← ロギング
Infrastructure層 ← ロギング

↓ すべてに共通 = 横断処理
```

### 特徴

- ✅ ビジネスロジックに関係ない
- ✅ 複数層で必要
- ✅ 技術的関心事
- ✅ 独立した層として実装

### 種類

| 処理 | 目的 | 現在 |
|------|------|------|
| ロギング | 操作記録・デバッグ | ✅ 実装済 |
| 時刻取得 | テスト可能な時刻 | ✅ 実装済 |
| 設定管理 | 環境別設定 | ✅ 実装済 |
| トランザクション | DB一貫性 | 🔄 将来 |
| 例外マッピング | 層別エラー処理 | 🔄 将来 |
| キャッシング | パフォーマンス | 🔄 将来 |

---

## ロギング（NLog）

### 目的

- ✅ アプリケーション操作記録
- ✅ デバッグ支援
- ✅ エラー追跡
- ✅ パフォーマンス監視

### 実装

#### 現在の構成

```
Crosscutting/
├─ Logging/
│  └─ NLogInitializer.cs       ← 初期化
├─ DependencyInjection.cs      ← DI登録
└─ Crosscutting.csproj
```

#### NLogInitializer

```csharp
// Crosscutting/Logging/NLogInitializer.cs
public static class NLogInitializer
{
    public static void Initialize(AppSettings appSettings, string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        
        // NLog.config を探して読み込む
        var candidates = new[]
        {
            Path.Combine(baseDirectory, "Configuration", "NLog.config"),
            Path.Combine(baseDirectory, "NLog.config")
        };
        
        var existing = candidates.FirstOrDefault(File.Exists);
        if (existing != null)
        {
            LogManager.Setup().LoadConfigurationFromFile(existing);
        }
        else
        {
            // デフォルト設定で初期化
        }
    }
}
```

#### DI登録

```csharp
// Crosscutting/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddCrosscuttingModels(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        
        // ロギング登録（詳細は NLog.config で設定）
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddNLog();
        });
        
        return services;
    }
}
```

#### 使用方法

```csharp
public class GetCarPreferenceUseCase
{
    private readonly ILogger<GetCarPreferenceUseCase> _logger;
    
    public GetCarPreferenceUseCase(
        ILogger<GetCarPreferenceUseCase> logger)
    {
        _logger = logger;  // DI経由で注入
    }
    
    public async Task<Response> Execute(Request request)
    {
        _logger.LogInformation(
            $"Getting car preference: {request.Id}");
        
        try
        {
            // 処理...
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex}");
            throw;
        }
    }
}
```

#### ログレベル

```
🔴 Critical  - システム停止レベル
🔴 Error     - エラー（処理失敗）
🟠 Warning   - 警告（予期しない状況）
🟢 Info      - 情報（通常の動作記録）
🔵 Debug     - デバッグ情報
⚪ Trace     - 詳細トレース
```

#### NLog.config 例

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <targets>
    <!-- ファイルログ -->
    <target name="fileTarget" 
            xsi:type="File" 
            fileName="${basedir}/Logs/app-${shortdate}.log"
            layout="${longdate} [${level:uppercase=true}] ${message}" />
    
    <!-- コンソールログ -->
    <target name="consoleTarget" 
            xsi:type="Console" 
            layout="${longdate} [${level:uppercase=true}] ${message}" />
  </targets>

  <rules>
    <logger name="*" minlevel="Info" writeTo="fileTarget" />
    <logger name="*" minlevel="Debug" writeTo="consoleTarget" />
  </rules>

</nlog>
```

---

## 時刻取得（IClock）

### 目的

- ✅ テスト容易性（時刻を固定化）
- ✅ DateTime.Now への依存排除
- ✅ タイムゾーン対応

### 実装

#### 現在の構成

```
Common/Clocks/
├─ IClock.cs               ← インターフェース
├─ ClockSettings.cs ✅ (moved)
├─ ClockFactory.cs         ← ファクトリ
├─ SystemClock.cs          ← 実装: システム時刻
├─ TickingClock.cs         ← 実装: 進む時刻
├─ OffsetClock.cs          ← 実装: オフセット
├─ MockClock.cs            ← テスト用
└─ LocalDateTime.cs        ← 時刻値オブジェクト
```

#### IClock インターフェース

```csharp
public interface IClock
{
    LocalDateTime UtcNow { get; }
    LocalDateTime Now { get; }
}

// LocalDateTime：時刻値オブジェクト
public sealed class LocalDateTime : ValueObject
{
    public DateTime Value { get; }
    
    public LocalDateTime(DateTime value)
    {
        this.Value = value;
    }
    
    public static LocalDateTime Now
        => new(DateTime.Now);
}
```

#### ClockFactory

```csharp
public static class ClockFactory
{
    public static IClock CreateClock(IClockSettings settings)
    {
        return settings.ClockType switch
        {
            "System" => new SystemClock(),
            "Ticking" => new TickingClock(settings.StartTime, settings.TickIntervalSeconds),
            "Offset" => new OffsetClock(settings.OffsetDateTime),
            _ => new SystemClock()
        };
    }
}
```

#### 使用方法

```csharp
public class CarPreference : Entity
{
    private readonly IClock _clock;
    
    public CarPreference(IClock clock)
    {
        _clock = clock;
    }
    
    public DateTime GetCurrentTime()
    {
        return _clock.UtcNow.Value;  // ❌ DateTime.Now は使わない
    }
}

// DI経由
services.AddSingleton<IClock>(
    ClockFactory.CreateClock(appSettings.ClockSettings));
```

#### テストでの使用

```csharp
[Fact]
public void TestWithMockClock()
{
    // 時刻を固定
    var mockClock = new MockClock(new DateTime(2024, 1, 1));
    var entity = new CarPreference(mockClock);
    
    var time = entity.GetCurrentTime();
    
    Assert.Equal(new DateTime(2024, 1, 1), time);
}
```

---

## 設定管理（AppSettings）

### 目的

- ✅ 環境別設定（Debug/Release）
- ✅ DB接続情報
- ✅ ログパス
- ✅ 外部API設定

### 実装

#### 現在の構成

```
Common/Configuration/
├─ IAppSettings.cs         ← インターフェース
├─ AppSettings.cs ✅ (moved)
├─ IApplicationSettings.cs
├─ IDatabaseSettings.cs
├─ IFileSystemSettings.cs
└─ (必要に応じて追加)
```

#### AppSettings

```csharp
// Common/Configuration/AppSettings.cs
public class AppSettings : IAppsSettings
{
    public string ApplicationBuildType { get; init; }
    public string SolutionName { get; init; }
    public string FolderName { get; init; }
    public string ProjectName { get; init; }
    public string DbServerName { get; init; }
    public string CatalogName { get; init; }
    public Dictionary<string, string> FolderPaths { get; init; }
    public Dictionary<string, string> ConnectionStrings { get; init; }
    public IClockSettings ClockSettings { get; init; }
}
```

#### appsettings.json

```json
{
  "AppSettings": {
    "ApplicationBuildType": "Debug",
    "SolutionName": "SupportAdvance",
    "FolderName": "SupportAdvance",
    "ProjectName": "SupportAdvance",
    "DbServerName": "localhost",
    "CatalogName": "SupportAdvanceDB",
    "FolderPaths": {
      "LogsDirectory": "./Logs",
      "ConfigDirectory": "./Configuration"
    },
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=SupportAdvanceDB;..."
    },
    "ClockSettings": {
      "ClockType": "System",
      "StartTime": "2024-01-01T00:00:00",
      "TickIntervalSeconds": 1
    }
  }
}
```

#### DI登録

```csharp
// Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructureModels(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    var appSettings = configuration
        .GetSection("AppSettings")
        .Get<AppSettings>()
        ?? throw new InvalidOperationException("AppSettings未設定");
    
    services.AddSingleton<IAppSettings>(appSettings);
    services.AddSingleton<IApplicationSettings>(appSettings);
    services.AddSingleton<IFileSystemSettings>(appSettings);
    services.AddSingleton<IDatabaseSettings>(appSettings);
    
    return services;
}
```

#### 使用方法

```csharp
public class MyService
{
    private readonly IAppSettings _settings;
    
    public MyService(IAppSettings settings)
    {
        _settings = settings;
    }
    
    public void DoSomething()
    {
        var logPath = _settings.FolderPaths["LogsDirectory"];
        var connStr = _settings.ConnectionStrings["DefaultConnection"];
    }
}
```

---

## 現在の実装

### 初期化フロー

```
Program.cs (エントリーポイント)
  ↓
HostBuilderFactory.Create()
  ↓
DependencyInjection登録
  ├─ Crosscutting.AddCrosscuttingModels()  ← ロギング初期化
  ├─ Infrastructure.AddInfrastructureModels() ← 設定・時刻初期化
  └─ (他)
```

### 起動時の処理

```csharp
// Presentation.Shared/HostBuilderFactory.cs
public static IHostBuilder Create(...)
{
    return Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            // ステップ1: Crosscutting初期化（ロギング）
            services.AddCrosscuttingModels(context.Configuration);
            
            // ステップ2: Infrastructure初期化（設定・時刻・Dapper）
            services.AddInfrastructureModels(context.Configuration);
            
            // ステップ3: Application初期化（UseCase）
            services.AddCarPreferencesApplicationModels();
            
            // ステップ4: Infrastructure Context固有
            services.AddCarPreferencesInfrastructureModels();
        });
}
```

---

## 拡張パターン

### パターン1：Decorator パターン（ロギング追加）

```csharp
// 既存の処理にロギング追加
public class LoggingUseCase<TRequest, TResponse> 
    : IUseCase<TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IResponse
{
    private readonly IUseCase<TRequest, TResponse> _inner;
    private readonly ILogger _logger;
    
    public LoggingUseCase(
        IUseCase<TRequest, TResponse> inner,
        ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }
    
    public async Task<TResponse> Execute(TRequest request)
    {
        _logger.LogInformation($"Executing: {typeof(TRequest).Name}");
        
        try
        {
            var result = await _inner.Execute(request);
            _logger.LogInformation("Execution completed");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Execution failed: {ex}");
            throw;
        }
    }
}
```

### パターン2：キャッシング（将来）

```csharp
// 取得結果をキャッシュ
public class CachingRepository<T> : IRepository<T>
{
    private readonly IRepository<T> _inner;
    private readonly IMemoryCache _cache;
    
    public async Task<T> GetById(int id)
    {
        var cacheKey = $"{typeof(T).Name}_{id}";
        
        if (_cache.TryGetValue(cacheKey, out T? cached))
            return cached;
        
        var result = await _inner.GetById(id);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        
        return result;
    }
}
```

### パターン3：トランザクション（将来）

```csharp
// DB操作をトランザクション内で実行
public class TransactionalUnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    
    public async Task Execute(Func<Task> action)
    {
        using var transaction = _connection.BeginTransaction();
        try
        {
            await action();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

---

## チェックリスト

### 新規処理追加時

- [ ] Crosscutting層に配置か？
- [ ] ビジネスロジック含まないか？
- [ ] 複数層で使用するか？
- [ ] テスト可能か？
- [ ] DI登録されているか？

---

**作成日**: 2026-06-30  
**参考**: 01-Layer-Architecture.md / 07-Project-Structure.md
