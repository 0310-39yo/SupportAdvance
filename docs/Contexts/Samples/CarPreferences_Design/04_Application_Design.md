# CarPreferences Context - Application層設計書

**対象者**: 開発者  
**目的**: CarPreferences Context の Application層（UseCase・ハンドラー）の詳細設計

---

## 📋 Application層の責務

```
Request (UI/API)
    ↓
UseCase (Application層)
    ├─ [1] パラメータ検証
    ├─ [2] Domain層呼び出し（Entity）
    ├─ [3] イベント処理（EventDispatcher）
    ├─ [4] 永続化（Repository）
    └─ [5] Response 返却
```

---

## 🎯 UseCase 一覧

| UseCase | 入力 | 出力 | Domain層呼び出し | イベント |
|---|---|---|---|---|
| CreateUserPreferences | UserId, 回答内容 | Response | Entity 作成 | PreferencesCreatedEvent |
| UpdateUserPreferences | UserId, 更新内容 | Response | Entity.UpdatePreferences() | 複数イベント |
| GetUserPreferences | UserId | PreferencesDTO | Entity 読み取り | - |

---

## 📝 UseCase 詳細設計

### 1. CreateUserPreferencesUseCase

**責務**: 新規ユーザーの初期好み設定を作成

**シーン**: ユーザー登録直後、アンケート完了時

**実装仕様**:

```csharp
namespace SupportAdvance.Contexts.Samples.CarPreferences.Application;

/// <summary>
/// ユーザープリファレンス作成 UseCase
/// 【責務】新規ユーザーの初期設定を Domain層で作成し、
///         イベント処理と永続化を行う
/// </summary>
public class CreateUserPreferencesUseCase 
    : IUseCase<CreateUserPreferencesRequest, CreateUserPreferencesResponse>
{
    private readonly IUserPreferencesRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IAppLogging<CreateUserPreferencesUseCase> _logger;
    private readonly IClock _clock;

    public CreateUserPreferencesUseCase(
        IUserPreferencesRepository repository,
        IEventDispatcher eventDispatcher,
        IAppLogging<CreateUserPreferencesUseCase> logger,
        IClock clock)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
        _clock = clock;
    }

    public async Task<CreateUserPreferencesResponse> ExecuteAsync(
        CreateUserPreferencesRequest request)
    {
        // [1] パラメータ検証
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.UserId);

        _logger.LogInformation(
            $"Creating preferences for user {request.UserId}");

        // [2] Domain層呼び出し（Entity 作成）
        var preferences = new UserPreferences(
            request.UserId,
            request.RespondedAt,
            _clock);

        // 初期設定を適用
        if (request.PreferredModel != null)
        {
            preferences.UpdatePreferredModel(request.PreferredModel, _clock);
        }

        if (request.PreferredBodyType.HasValue)
        {
            preferences.UpdateBodyType(request.PreferredBodyType.Value, _clock);
        }

        if (request.BudgetFrom.HasValue || request.BudgetTo.HasValue)
        {
            preferences.UpdateBudget(
                request.BudgetFrom,
                request.BudgetTo,
                _clock);
        }

        // [3] イベント処理
        var domainEvents = preferences.DomainEvents;
        foreach (var @event in domainEvents)
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        // PreferencesCreatedEvent を発行
        await _eventDispatcher.DispatchAsync(
            new PreferencesCreatedEvent(preferences.Id, _clock.JstNow));

        // [4] 永続化
        await _repository.AddAsync(preferences);

        // [5] イベントクリア（重複発行防止）
        preferences.ClearDomainEvents();

        _logger.LogInformation(
            $"Preferences created for user {request.UserId}");

        return new CreateUserPreferencesResponse
        {
            UserId = preferences.Id,
            CreatedAt = preferences.CreatedAt
        };
    }
}
```

**Request**:
```csharp
public class CreateUserPreferencesRequest : IRequest
{
    public UserId UserId { get; set; } = null!;
    public LocalDateTime RespondedAt { get; set; }
    public CarModel? PreferredModel { get; set; }
    public BodyType? PreferredBodyType { get; set; }
    public Money? BudgetFrom { get; set; }
    public Money? BudgetTo { get; set; }
}
```

**Response**:
```csharp
public class CreateUserPreferencesResponse : IResponse
{
    public UserId UserId { get; set; } = null!;
    public LocalDateTime CreatedAt { get; set; }
}
```

**発行イベント**:
- `PreferencesCreatedEvent` (Application層で発行)
- その他：リクエストで指定した初期設定に応じた更新イベント

---

### 2. UpdateUserPreferencesUseCase

**責務**: ユーザーの好みを更新

**実装仕様**:

```csharp
public class UpdateUserPreferencesUseCase 
    : IUseCase<UpdateUserPreferencesRequest, UpdateUserPreferencesResponse>
{
    private readonly IUserPreferencesRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IAppLogging<UpdateUserPreferencesUseCase> _logger;
    private readonly IClock _clock;

    public UpdateUserPreferencesUseCase(
        IUserPreferencesRepository repository,
        IEventDispatcher eventDispatcher,
        IAppLogging<UpdateUserPreferencesUseCase> logger,
        IClock clock)
    {
        _repository = repository;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
        _clock = clock;
    }

    public async Task<UpdateUserPreferencesResponse> ExecuteAsync(
        UpdateUserPreferencesRequest request)
    {
        // [1] パラメータ検証
        ArgumentNullException.ThrowIfNull(request);
        
        _logger.LogInformation(
            $"Updating preferences for user {request.UserId}");

        // [2] Entity 取得
        var preferences = await _repository.GetAsync(request.UserId);
        if (preferences == null)
        {
            throw new UserPreferencesNotFoundException(
                $"Preferences not found for user {request.UserId}");
        }

        // [3] Domain層呼び出し（更新）
        preferences.UpdatePreferences(
            request.PreferredModel,
            request.PreferredBodyType,
            request.PrefersAutomatic,
            request.BudgetFrom,
            request.BudgetTo,
            _clock);

        // [4] イベント処理
        var domainEvents = preferences.DomainEvents;
        foreach (var @event in domainEvents)
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        // [5] 永続化
        await _repository.UpdateAsync(preferences);

        // [6] イベントクリア
        preferences.ClearDomainEvents();

        _logger.LogInformation(
            $"Preferences updated for user {request.UserId}");

        return new UpdateUserPreferencesResponse
        {
            UserId = preferences.Id,
            UpdatedAt = preferences.UpdatedAt
        };
    }
}
```

**Request**:
```csharp
public class UpdateUserPreferencesRequest : IRequest
{
    public UserId UserId { get; set; } = null!;
    public CarModel? PreferredModel { get; set; }      // null = 更新しない
    public BodyType? PreferredBodyType { get; set; }  // null = 更新しない
    public bool? PrefersAutomatic { get; set; }       // null = 更新しない
    public Money? BudgetFrom { get; set; }            // null = 更新しない
    public Money? BudgetTo { get; set; }              // null = 更新しない
}
```

**Response**:
```csharp
public class UpdateUserPreferencesResponse : IResponse
{
    public UserId UserId { get; set; } = null!;
    public LocalDateTime UpdatedAt { get; set; }
}
```

**発行イベント**: 更新対象のプロパティに応じた複数イベント

---

### 3. GetUserPreferencesUseCase

**責務**: ユーザーの好み情報を取得

**実装仕様**:

```csharp
public class GetUserPreferencesUseCase 
    : IUseCase<GetUserPreferencesRequest, GetUserPreferencesResponse>
{
    private readonly IUserPreferencesRepository _repository;
    private readonly IAppLogging<GetUserPreferencesUseCase> _logger;

    public GetUserPreferencesUseCase(
        IUserPreferencesRepository repository,
        IAppLogging<GetUserPreferencesUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<GetUserPreferencesResponse> ExecuteAsync(
        GetUserPreferencesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            $"Fetching preferences for user {request.UserId}");

        var preferences = await _repository.GetAsync(request.UserId);
        if (preferences == null)
        {
            throw new UserPreferencesNotFoundException(
                $"Preferences not found for user {request.UserId}");
        }

        return new GetUserPreferencesResponse
        {
            UserId = preferences.Id,
            PreferredModel = preferences.PreferredModel?.Name,
            PreferredBodyType = preferences.PreferredBodyType,
            PrefersAutomatic = preferences.PrefersAutomatic,
            BudgetFrom = preferences.BudgetFrom?.Amount,
            BudgetTo = preferences.BudgetTo?.Amount,
            CreatedAt = preferences.CreatedAt,
            UpdatedAt = preferences.UpdatedAt
        };
    }
}
```

**Request**:
```csharp
public class GetUserPreferencesRequest : IRequest
{
    public UserId UserId { get; set; } = null!;
}
```

**Response**:
```csharp
public class GetUserPreferencesResponse : IResponse
{
    public UserId UserId { get; set; } = null!;
    public string? PreferredModel { get; set; }
    public BodyType? PreferredBodyType { get; set; }
    public bool PrefersAutomatic { get; set; }
    public decimal? BudgetFrom { get; set; }
    public decimal? BudgetTo { get; set; }
    public LocalDateTime CreatedAt { get; set; }
    public LocalDateTime UpdatedAt { get; set; }
}
```

**発行イベント**: なし（読み取り専用）

---

## 🔔 イベントハンドラー一覧

### Domain層イベント → ハンドラー マッピング

| イベント | ハンドラー | 責務 |
|---|---|---|
| PreferencesUpdatedEvent | PreferencesUpdatedLoggingHandler | ログ出力 |
| PreferencesUpdatedEvent | PreferencesUpdatedAuditHandler | 監査ログ記録 |
| PreferencesUpdatedEvent | PreferencesUpdatedNotificationHandler | メール通知 |
| BudgetUpdatedEvent | BudgetUpdatedCacheHandler | キャッシュ無効化 |
| BudgetUpdatedEvent | BudgetUpdatedIndexHandler | 検索インデックス更新 |
| BodyTypeUpdatedEvent | BodyTypeUpdatedIndexHandler | 検索インデックス更新 |
| TransmissionPreferenceUpdatedEvent | TransmissionUpdatedLoggingHandler | ログ出力 |
| PreferencesCreatedEvent | PreferencesCreatedWelcomeHandler | ウェルカムメール |

### ハンドラー実装例

#### PreferencesUpdatedLoggingHandler

```csharp
public class PreferencesUpdatedLoggingHandler 
    : IDomainEventHandler<PreferencesUpdatedEvent>
{
    private readonly IAppLogging<PreferencesUpdatedLoggingHandler> _logger;

    public async Task HandleAsync(PreferencesUpdatedEvent @event)
    {
        _logger.LogInformation(
            $"User preferences updated - UserId: {@event.UserId}, " +
            $"Change: {@event.OldValue} → {@event.NewValue}, " +
            $"Timestamp: {@event.OccurredAt}");

        await Task.CompletedTask;  // 非同期ダミー
    }
}
```

#### PreferencesUpdatedNotificationHandler

```csharp
public class PreferencesUpdatedNotificationHandler 
    : IDomainEventHandler<PreferencesUpdatedEvent>
{
    private readonly IMailService _mailService;
    private readonly IAppLogging<PreferencesUpdatedNotificationHandler> _logger;

    public async Task HandleAsync(PreferencesUpdatedEvent @event)
    {
        try
        {
            await _mailService.SendAsync(
                userId: @event.UserId,
                subject: "Your car preferences have been updated",
                body: $"Your preference changed from {
                    @event.OldValue} to {@event.NewValue}");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Failed to send notification for user {@event.UserId}", ex);
            throw;  // 呼び出し元で例外処理
        }
    }
}
```

#### BudgetUpdatedCacheHandler

```csharp
public class BudgetUpdatedCacheHandler 
    : IDomainEventHandler<BudgetUpdatedEvent>
{
    private readonly ICache _cache;

    public async Task HandleAsync(BudgetUpdatedEvent @event)
    {
        // ユーザーの予算フィルター結果をクリア
        _cache.Remove($"user-budget-filtered:{@event.UserId}");
        
        await Task.CompletedTask;
    }
}
```

---

## 🔌 EventDispatcher 実装仕様

**責務**: ドメインイベントを適切なハンドラーにルーティング

```csharp
public interface IEventDispatcher
{
    /// <summary>
    /// ドメインイベントをディスパッチして全ハンドラーを実行
    /// </summary>
    Task DispatchAsync(IDomainEvent @event);
}

public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppLogging<EventDispatcher> _logger;

    public async Task DispatchAsync(IDomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // イベント型からハンドラー型を特定
        var eventType = @event.GetType();
        var handlerType = typeof(IDomainEventHandler<>)
            .MakeGenericType(eventType);

        // DI から該当ハンドラーを取得
        var handlers = _serviceProvider.GetService(
            typeof(IEnumerable<>).MakeGenericType(handlerType)) 
            as System.Collections.IEnumerable;

        if (handlers == null)
        {
            _logger.LogInformation(
                $"No handlers found for event {eventType.Name}");
            return;
        }

        // 全ハンドラーを実行
        foreach (var handler in handlers)
        {
            try
            {
                var handleMethod = handlerType.GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    await (Task)handleMethod.Invoke(handler, new[] { @event })!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error handling event {eventType.Name}", ex);
                throw;  // 呼び出し元で処理
            }
        }
    }
}
```

---

## 📋 DI 登録

**Presentation層の Program.cs**:

```csharp
services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
services.AddScoped<IEventDispatcher, EventDispatcher>();

// UseCase
services.AddScoped<CreateUserPreferencesUseCase>();
services.AddScoped<UpdateUserPreferencesUseCase>();
services.AddScoped<GetUserPreferencesUseCase>();

// ハンドラー（複数登録）
services.AddScoped<
    IDomainEventHandler<PreferencesUpdatedEvent>,
    PreferencesUpdatedLoggingHandler>();
services.AddScoped<
    IDomainEventHandler<PreferencesUpdatedEvent>,
    PreferencesUpdatedNotificationHandler>();
services.AddScoped<
    IDomainEventHandler<BudgetUpdatedEvent>,
    BudgetUpdatedCacheHandler>();
// ... 他のハンドラー
```

---

## 🧪 テスト観点

### UseCase テスト

1. **CreateUserPreferencesUseCase**
   - Entity が正しく作成される
   - イベントが発行される
   - Repository に保存される
   - イベントハンドラーが呼ばれる

2. **UpdateUserPreferencesUseCase**
   - Entity が見つからない場合は例外
   - 複数イベント発行を確認
   - キャッシュ無効化が呼ばれる

3. **GetUserPreferencesUseCase**
   - Entity が見つからない場合は例外
   - DTOが正しく変換される

### ハンドラー テスト

1. **ログハンドラー**
   - ログが出力される
   - イベント情報が含まれる

2. **メール ハンドラー**
   - メール送信が呼ばれる
   - 例外時に再スロー

3. **キャッシュ ハンドラー**
   - キャッシュ削除が呼ばれる

---

## 🔗 関連ドキュメント

- [01_Entity_Design.md](01_Entity_Design.md) - Entity 設計
- [02_DomainEvents_Design.md](02_DomainEvents_Design.md) - イベント設計
- [03_EventFlow_Diagram.md](03_EventFlow_Diagram.md) - フロー図

---

## 📝 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-08-01 | 初版作成。3つの UseCase とハンドラー実装仕様 |
