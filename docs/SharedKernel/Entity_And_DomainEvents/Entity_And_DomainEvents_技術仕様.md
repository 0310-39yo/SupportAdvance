# Entity基底クラス＆ドメインイベント - 技術仕様書

**対象者**: 開発者（Entity を継承する実装者、イベントハンドラー実装者）

---

## 📖 概要

このドキュメントは、SharedKernel に実装される Entity基底クラスとドメインイベント機構の使用方法を定義します。

- **何を実装するのか**: Domain層の状態変化をドメインイベントで記録
- **どこに実装するのか**: src/SharedKernel/Entities/Abstractions/
- **誰が使うのか**: CarPreferences.Domain など各 Bounded Context の Domain層

---

## 🏗️ クラス設計概要

### 主要インターフェース・クラス

#### 1. IDomainEvent インターフェース

ドメインイベントの基本インターフェース。すべてのドメインイベントはこれを実装。

```csharp
namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ドメインイベントの基本インターフェース
/// Domain層で発生した状態変化を表現
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// イベント発生時刻（JST）
    /// </summary>
    LocalDateTime OccurredAt { get; }
}
```

**要件**:
- LocalDateTime 型で JST を格納
- イベント発生時刻は必須

**使用例**:
```csharp
public class PreferencesUpdatedEvent : IDomainEvent
{
    public long RowId { get; }                      // AggregateRoot の rowId
    public PreferenceChangeType ChangeType { get; }
    public string OldValue { get; }
    public string NewValue { get; }
    public LocalDateTime OccurredAt { get; }  // 必須

    public PreferencesUpdatedEvent(
        long rowId,
        PreferenceChangeType changeType,
        string oldValue,
        string newValue,
        LocalDateTime occurredAt)
    {
        RowId = rowId;
        ChangeType = changeType;
        OldValue = oldValue;
        NewValue = newValue;
        OccurredAt = occurredAt;
    }
}
```

#### 2. Entity<TId> 基底クラス

ドメインエンティティの基底クラス。すべての Entity はこれを継承。

```csharp
namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ドメインエンティティの基底クラス
/// </summary>
/// <typeparam name="TId">ID型（ValueObject）</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Entity の識別子
    /// </summary>
    public TId Id { get; protected set; }

    /// <summary>
    /// 発行されたドメインイベント（読み取り専用）
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// 指定されたドメインイベントを発行
    /// 【責務】イベントをリストに追加するのみ
    /// 【実行階層】Domain層（ビジネスロジック内）
    /// </summary>
    /// <param name="domainEvent">発行するイベント</param>
    /// <exception cref="ArgumentNullException">イベントが null の場合</exception>
    protected void RaiseDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Entity の等価性判定（ID ベース）
    /// </summary>
    public override bool Equals(object? obj);

    /// <summary>
    /// Entity の等価性判定（ID ベース）
    /// </summary>
    public bool Equals(Entity<TId>? other);

    /// <summary>
    /// ハッシュコード取得（ID ベース）
    /// </summary>
    public override int GetHashCode();
}
```

**要件**:
- Generic 型パラメータ `TId` は ValueObject など不変値オブジェクト
- `DomainEvents` は読み取り専用リスト
- `RaiseDomainEvent()` は protected で Domain層内でのみアクセス可
- Entity の等価性判定は ID ベース

**使用例**:
```csharp
// Domain層
public class UserPreferences : AggregateRoot<long>  // RowId ベース
{
    private RespondentPersonId _userId;             // ビジネス識別子
    private CarModel? _preferredModel;
    private LocalDateTime _updatedAt;

    // コンストラクタ
    public UserPreferences(RespondentPersonId userId, IClock clock, long rowId = 0)
    {
        Id = rowId;
        _userId = userId;
        _updatedAt = clock.JstNow;
    }

    // ビジネスメソッド
    public void UpdatePreferredModel(CarModel model, IClock clock)
    {
        var oldModel = _preferredModel;
        _preferredModel = model;
        _updatedAt = clock.JstNow;

        // Domain層内でイベント発行
        this.RaiseDomainEvent(new PreferencesUpdatedEvent(
            this.Id,  // RowId
            PreferenceChangeType.ModelUpdated,
            oldModel?.ToString() ?? "未設定",
            model.ToString(),
            _updatedAt
        ));
    }
}
```

#### 3. AggregateRoot<TId> 基底クラス

Entity<TId> を継承した AggregateRoot の基盤クラス。

```csharp
namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// AggregateRoot の基底クラス
/// 集約ルートはトランザクション境界を表現
/// </summary>
/// <typeparam name="TId">ID型（ValueObject）</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    // Entity<TId> を継承
    // AggregateRoot 固有の機能は将来追加予定
}
```

**要件**:
- Entity<TId> との違いは意味論的（AggregateRoot = トランザクション境界）
- 機能的には Entity<TId> と同じ

**使用例**:
```csharp
// 集約ルートは AggregateRoot<long> を継承（RowId ベース）
public class UserPreferences : AggregateRoot<long>
{
    private RespondentPersonId _userId;  // ビジネス識別子は別プロパティ
    // ...
}
```

#### 4. IDomainEventHandler<TEvent> インターフェース

Application層でドメインイベントを処理するハンドラーのインターフェース。

```csharp
namespace SupportAdvance.SharedKernel.Entities.DomainEvents;

/// <summary>
/// ドメインイベントハンドラーの基本インターフェース
/// Application層でイベントを消費するために実装
/// </summary>
/// <typeparam name="TEvent">処理するイベント型</typeparam>
public interface IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// イベント処理（Application層で実装）
    /// 【実行階層】Application層
    /// 【責務】ログ出力、他システムへの通知など
    /// </summary>
    /// <param name="event">処理するイベント</param>
    /// <returns>処理完了タスク</returns>
    Task HandleAsync(TEvent @event);
}
```

**要件**:
- Application層で実装されるインターフェース
- ハンドラーは async/await に対応

**使用例**:
```csharp
// Application層
public class PreferencesUpdatedEventHandler : IDomainEventHandler<PreferencesUpdatedEvent>
{
    private readonly ILogger<PreferencesUpdatedEventHandler> _logger;

    public PreferencesUpdatedEventHandler(ILogger<PreferencesUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(PreferencesUpdatedEvent @event)
    {
        // ログ出力
        _logger.LogInformation(
            "Preferences updated - RowId: {RowId}, " +
            "ChangeType: {ChangeType}, " +
            "OldValue: {OldValue}, " +
            "NewValue: {NewValue}, " +
            "At: {OccurredAt}",
            @event.RowId,
            @event.ChangeType,
            @event.OldValue,
            @event.NewValue,
            @event.OccurredAt);

        await Task.CompletedTask;
    }
}
```

---

## 🔄 実装パターン

### パターン1: 単純なドメインイベント発行

```csharp
// Domain層：イベント定義
public class CarModelChangedEvent : IDomainEvent
{
    public UserId UserId { get; }
    public CarModel OldModel { get; }
    public CarModel NewModel { get; }
    public LocalDateTime OccurredAt { get; }

    public CarModelChangedEvent(
        UserId userId,
        CarModel oldModel,
        CarModel newModel,
        LocalDateTime occurredAt)
    {
        UserId = userId;
        OldModel = oldModel;
        NewModel = newModel;
        OccurredAt = occurredAt;
    }
}

// Domain層：Entity で使用
public class UserPreferences : AggregateRoot<UserId>
{
    private CarModel _preferredModel;

    public void ChangePreferredModel(CarModel newModel, IClock clock)
    {
        var oldModel = _preferredModel;
        _preferredModel = newModel;

        // イベント発行
        this.RaiseDomainEvent(new CarModelChangedEvent(
            this.Id,
            oldModel,
            newModel,
            clock.JstNow
        ));
    }
}
```

### パターン2: 複数イベント発行

```csharp
public class UserPreferences : AggregateRoot<UserId>
{
    public void BulkUpdatePreferences(
        CarModel model,
        RespondentAge age,
        IClock clock)
    {
        // 複数の変更
        _preferredModel = model;
        _age = age;

        // 複数のイベント発行
        this.RaiseDomainEvent(new ModelUpdatedEvent(this.Id, model, clock.JstNow));
        this.RaiseDomainEvent(new AgeUpdatedEvent(this.Id, age, clock.JstNow));
    }
}
```

### パターン3: Application層でのイベント処理

```csharp
public class UpdatePreferencesUseCase : IUseCase<UpdatePreferencesRequest, UpdatePreferencesResponse>
{
    private readonly IPreferencesRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;

    public async Task<UpdatePreferencesResponse> ExecuteAsync(UpdatePreferencesRequest request)
    {
        // 1. Entity 取得・更新
        var preferences = await _repository.GetAsync(request.UserId);
        preferences.UpdatePreferences(request.Model);

        // 2. イベント処理
        foreach (var @event in preferences.DomainEvents)
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        // 3. 保存
        await _repository.SaveAsync(preferences);

        return new UpdatePreferencesResponse { Success = true };
    }
}
```

---

## ✅ 使用上の注意

### Do（実装すること）

- ✅ Entity は Domain層で状態変化時にイベント発行
- ✅ イベント型は IDomainEvent を実装
- ✅ イベント内の日時は LocalDateTime（JST）を使用
- ✅ ハンドラーは Application層で実装
- ✅ Entity の ID による等価性判定を活用

### Don't（避けること）

- ❌ Domain層内でログ出力しない（イベント発行で対応）
- ❌ Domain層内で DateTime.UtcNow を使用しない（Clock 経由で LocalDateTime を取得）
- ❌ Application層でビジネスロジックを記述しない（Domain層で実装）
- ❌ イベント処理結果を Entity に反映しない（イベントは通知のみ）

---

## 📚 実装チェックリスト

### Entity を継承した具体クラス実装時

- [ ] Entity<TId> または AggregateRoot<TId> を継承
- [ ] ID 型（TId）を ValueObject で定義
- [ ] ビジネスメソッド内で RaiseDomainEvent() を呼び出し
- [ ] イベント型は IDomainEvent を実装
- [ ] イベント内に LocalDateTime（OCCurredAt）を含める
- [ ] DomainEvents プロパティで取得可能

### イベントハンドラー実装時

- [ ] IDomainEventHandler<TEvent> を実装
- [ ] HandleAsync メソッドを async Task で定義
- [ ] イベントのプロパティを活用してログ・処理
- [ ] Application層に配置
- [ ] DI 登録

---

## 🔗 関連ドキュメント

- [Entity_And_DomainEvents_詳細設計.md](Entity_And_DomainEvents_詳細設計.md) - 実装詳細（AI向け）
- [Entity_And_DomainEvents_単体テスト仕様.md](Entity_And_DomainEvents_単体テスト仕様.md) - テスト仕様
- [Domain_Logging_Architecture.md](../Guides/Domain_Logging_Architecture.md) - ドメイン層ログ設計

---

## 📝 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-08-01 | 初版作成。Entity基底クラスとドメインイベントの技術仕様を定義 |
