# Entity基底クラス＆ドメインイベント - 技術仕様書

**対象者**: 開発者（Entity を継承する実装者、イベントハンドラー実装者）

---

## 📖 概要

このドキュメントは、SharedKernel に実装される Entity基底クラスとドメインイベント機構の使用方法を定義します。

- **何を実装するのか**: Domain層の状態変化をドメインイベントで記録
- **どこに実装するのか**: src/SharedKernel/Entities/Abstractions/
- **誰が使うのか**: Identity.Domain, Employee.Domain など各 Bounded Context の Domain層

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
    /// イベント一意識別子（GUID ValueObject）
    /// 【用途】このイベント自体を識別、トレーシング
    /// </summary>
    DomainEventId EventId { get; }

    /// <summary>
    /// イベント発生時刻（JST）
    /// </summary>
    LocalDateTime OccurredAt { get; }
    
    /// <summary>
    /// 注記：AggregateRootId は具体的なイベント実装で型パラメータとして指定
    /// 例：OrderId, EmployeeId, UserPreferencesId など集約固有のID
    /// </summary>
}
```

**要件**:
- `EventId` は DomainEventId ValueObject（GUID）で各イベントを一意識別
- `LocalDateTime` 型で JST を格納
- イベント発生時刻は必須
- `AggregateRootId` は具体的なイベント型で、集約固有のID ValueObject を指定

**使用例**:
```csharp
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.CarPreferences.Domain.ValueObjects;

public class UserPreferencesUpdatedEvent : IDomainEvent
{
    public DomainEventId EventId { get; }  // イベント自体のID（GUID）
    public UserPreferencesId AggregateRootId { get; }  // ← 集約のID（GUID ベース ValueObject）
    public string ChangeType { get; }
    public string OldValue { get; }
    public string NewValue { get; }
    public LocalDateTime OccurredAt { get; }  // 必須

    public UserPreferencesUpdatedEvent(
        DomainEventId eventId,
        UserPreferencesId aggregateRootId,
        string changeType,
        string oldValue,
        string newValue,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(eventId);
        ArgumentNullException.ThrowIfNull(aggregateRootId);

        EventId = eventId;
        AggregateRootId = aggregateRootId;
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
/// <typeparam name="TId">ID型（集約固有のID ValueObject）</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Entity の識別子（集約固有のID ValueObject）
    /// 【例】OrderId, EmployeeId, UserPreferencesId など
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
    /// 【用法】RaiseDomainEvent(new YourEventType(...))
    /// </summary>
    /// <param name="domainEvent">発行するイベント</param>
    /// <exception cref="ArgumentNullException">イベントが null の場合</exception>
    protected void RaiseDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Entity の等価性判定（ID ベース）
    /// </summary>
    public override bool Equals(object? obj);

    /// <summary>
    /// Entity の等価性判定（ID ベース、型安全版）
    /// </summary>
    public bool Equals(Entity<TId>? other);

    /// <summary>
    /// ハッシュコード取得（ID ベース）
    /// </summary>
    public override int GetHashCode();
}
```

**要件**:
- Generic 型パラメータ `TId` は集約固有の ID ValueObject（AggregateId を継承）
- `DomainEvents` は読み取り専用リスト
- `RaiseDomainEvent()` は protected で Domain層内でのみアクセス可
- Entity の等価性判定は ID ベース

**使用例**:
```csharp
using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.CarPreferences.Domain.ValueObjects;

// Domain層
public class UserPreferences : AggregateRoot<UserPreferencesId>
{
    private RowId _preferencesRowId;  // テーブルの物理キー（プライベート）
    private RespondentPersonId _userId;
    private CarModel? _preferredModel;
    private LocalDateTime _updatedAt;

    public RespondentPersonId UserId => _userId;
    public CarModel? PreferredModel => _preferredModel;

    // コンストラクタ
    public UserPreferences(
        UserPreferencesId id,
        RespondentPersonId userId,
        RowId? preferencesRowId = null,
        IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(userId);

        Id = id;  // 集約ID（UserPreferencesId）
        _userId = userId;
        _preferencesRowId = preferencesRowId ?? RowId.New();
        _updatedAt = clock?.JstNow ?? LocalDateTime.Now;

        if (clock != null)
        {
            RaiseDomainEvent(new UserPreferencesCreatedEvent(
                DomainEventId.New(),
                this.Id,  // ← AggregateRootId = UserPreferencesId
                _updatedAt
            ));
        }
    }

    // ビジネスメソッド
    public void UpdatePreferredModel(CarModel model, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(clock);

        var oldModel = _preferredModel;
        _preferredModel = model;
        _updatedAt = clock.JstNow;

        // Domain層内でイベント発行
        this.RaiseDomainEvent(new UserPreferencesUpdatedEvent(
            DomainEventId.New(),              // ← イベント自体のID（毎回新規）
            this.Id,                          // ← AggregateRootId = UserPreferencesId
            "PreferredModel",
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
/// AggregateRoot（集約ルート）の基底クラス
/// 
/// 【意味論】
/// - AggregateRoot はトランザクション境界を表現
/// - 一度に保存・削除される複数の Entity をグループ化
/// 
/// 【機能】
/// - Entity<TId> を継承（すべてのEntity機能を保有）
/// - 固有機能は将来追加予定
/// </summary>
/// <typeparam name="TId">ID型（集約固有のID ValueObject）</typeparam>
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
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.CarPreferences.Domain.ValueObjects;

// 集約ルート：UserPreferences
// TId = UserPreferencesId（集約固有のID ValueObject）
public class UserPreferences : AggregateRoot<UserPreferencesId>
{
    public UserPreferencesId Id { get; }  // ← GUID ベースのビジネスID
    private RowId _preferencesRowId { get; }  // ← テーブルの物理キー（プライベート）
    
    public UserPreferences(UserPreferencesId id, RowId? preferencesRowId = null, IClock clock)
    {
        Id = id;
        _preferencesRowId = preferencesRowId ?? RowId.New();
        // ...
    }
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
    /// 【イベント属性の利用】AggregateRootId から対象エンティティを特定
    /// </summary>
    /// <param name="event">処理するイベント</param>
    /// <returns>処理完了タスク</returns>
    Task HandleAsync(TEvent @event);
}
```

**要件**:
- Application層で実装されるインターフェース
- ハンドラーは async/await に対応
- イベントの AggregateRootId を使用して対象を特定

**使用例**:
```csharp
using Microsoft.Extensions.Logging;
using SupportAdvance.SharedKernel.Entities.DomainEvents;
using SupportAdvance.Contexts.CarPreferences.Domain.DomainEvents;

// Application層
public class UserPreferencesUpdatedEventHandler : IDomainEventHandler<UserPreferencesUpdatedEvent>
{
    private readonly ILogger<UserPreferencesUpdatedEventHandler> _logger;

    public UserPreferencesUpdatedEventHandler(ILogger<UserPreferencesUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserPreferencesUpdatedEvent @event)
    {
        // イベントの AggregateRootId から対象の UserPreferences を特定
        var userPreferencesId = @event.AggregateRootId;

        _logger.LogInformation(
            "UserPreferences updated - Id: {UserPreferencesId}, " +
            "ChangeType: {ChangeType}, " +
            "OldValue: {OldValue}, " +
            "NewValue: {NewValue}, " +
            "At: {OccurredAt}",
            userPreferencesId,
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
public class UserPreferencesModelChangedEvent : IDomainEvent
{
    public DomainEventId EventId { get; }
    public UserPreferencesId AggregateRootId { get; }  // ← 集約のID
    public CarModel OldModel { get; }
    public CarModel NewModel { get; }
    public LocalDateTime OccurredAt { get; }

    public UserPreferencesModelChangedEvent(
        DomainEventId eventId,
        UserPreferencesId aggregateRootId,
        CarModel oldModel,
        CarModel newModel,
        LocalDateTime occurredAt)
    {
        EventId = eventId;
        AggregateRootId = aggregateRootId;
        OldModel = oldModel;
        NewModel = newModel;
        OccurredAt = occurredAt;
    }
}

// Domain層：Entity で使用
public class UserPreferences : AggregateRoot<UserPreferencesId>
{
    private CarModel _preferredModel;

    public void ChangePreferredModel(CarModel newModel, IClock clock)
    {
        var oldModel = _preferredModel;
        _preferredModel = newModel;

        // イベント発行
        // EventId：イベント自体のID（毎回新規）
        // AggregateRootId：この集約のID（UserPreferencesId）
        this.RaiseDomainEvent(new UserPreferencesModelChangedEvent(
            DomainEventId.New(),   // ← イベントのID
            this.Id,               // ← 集約のID（UserPreferencesId）
            oldModel,
            newModel,
            clock.JstNow
        ));
    }
}
```

### パターン2: 複数イベント発行

```csharp
public class UserPreferences : AggregateRoot<UserPreferencesId>
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
        this.RaiseDomainEvent(new UserPreferencesModelChangedEvent(
            DomainEventId.New(),
            this.Id,  // ← 集約のID
            model,
            clock.JstNow
        ));
        
        this.RaiseDomainEvent(new UserPreferencesAgeChangedEvent(
            DomainEventId.New(),
            this.Id,  // ← 集約のID
            age,
            clock.JstNow
        ));
    }
}
```

### パターン3: Application層でのイベント処理

```csharp
public class UpdatePreferencesUseCase : IUseCase<UpdatePreferencesRequest, UpdatePreferencesResponse>
{
    private readonly IUserPreferencesRepository _repository;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public async Task<UpdatePreferencesResponse> ExecuteAsync(UpdatePreferencesRequest request)
    {
        // 1. Entity 取得・更新
        var preferences = await _repository.GetByIdAsync(request.UserPreferencesId);
        preferences.UpdatePreferredModel(request.Model, _clock);

        // 2. イベント処理
        await _eventDispatcher.DispatchAsync(preferences.DomainEvents);

        // 3. 保存
        await _repository.UpdateAsync(preferences);

        return new UpdatePreferencesResponse { Success = true };
    }
}
```

---

## ✅ 使用上の注意

### Do（実装すること）

- ✅ Entity は Domain層で状態変化時にイベント発行
- ✅ イベント型は IDomainEvent を実装
- ✅ イベント内に EventId と AggregateRootId を含める
- ✅ イベント内の日時は LocalDateTime（JST）を使用
- ✅ ハンドラーは Application層で実装
- ✅ Entity の ID による等価性判定を活用

### Don't（避けること）

- ❌ Domain層内でログ出力しない（イベント発行で対応）
- ❌ Domain層内で DateTime.UtcNow を使用しない（Clock 経由で LocalDateTime を取得）
- ❌ Application層でビジネスロジックを記述しない（Domain層で実装）
- ❌ イベント処理結果を Entity に反映しない（イベントは通知のみ）
- ❌ AggregateRootId に long（RowId）を使用しない（集約固有のID ValueObject を使用）

---

## 📚 実装チェックリスト

### Entity を継承した具体クラス実装時

- [ ] **Entity<XXXId>** または **AggregateRoot<XXXId>** を継承（XXXId = 集約固有のID）
- [ ] **XXXId ValueObject** を定義（AggregateId を継承、GUID ベース）
- [ ] **TId = XXXId** をジェネリック型パラメータに指定
- [ ] **RowId** をプライベート属性で保持（表示しない）
- [ ] ビジネスメソッド内で **RaiseDomainEvent()** を呼び出し
- [ ] イベント型は **IDomainEvent** を実装
- [ ] イベント内に **EventId**（毎回新規）と **AggregateRootId**（this.Id）を含める
- [ ] イベント内に **LocalDateTime（OccurredAt）** を含める
- [ ] DomainEvents プロパティで取得可能

### ドメインイベント実装時

- [ ] **IDomainEvent** を実装
- [ ] **EventId: DomainEventId** プロパティを持つ
- [ ] **AggregateRootId: XXXId** プロパティを持つ（集約固有のID型）
- [ ] **OccurredAt: LocalDateTime** プロパティを持つ
- [ ] コンストラクタで null チェック実装
- [ ] 不変（読み取り専用プロパティ）

### イベントハンドラー実装時

- [ ] **IDomainEventHandler<TEvent>** を実装
- [ ] **HandleAsync** メソッドを async Task で定義
- [ ] **イベントの AggregateRootId** を使用して対象を特定
- [ ] ログ出力・外部連携などのサイドエフェクト実装
- [ ] **Application層** に配置
- [ ] **DI に登録** （services.AddScoped<IDomainEventHandler<YourEvent>>()）

---

## 🔗 関連ドキュメント

- [Entity_And_DomainEvents_詳細設計.md](Entity_And_DomainEvents_詳細設計.md) — 実装詳細（AI向け）
- [Entity_And_DomainEvents_単体テスト仕様.md](Entity_And_DomainEvents_単体テスト仕様.md) — テスト仕様
- [Entity_設計ガイドライン.md](../Guides/Entity_設計ガイドライン.md) — Entity<TId> パターンの詳細
- [ドメインイベント_設計ガイド.md](../Guides/ドメインイベント_設計ガイド.md) — イベント駆動設計パターン
- [AggregateId_設計ガイド.md](../Guides/AggregateId_設計ガイド.md) — GUID ベース ID の実装

---

## 📝 更新履歴

| 日付 | 更新内容 |
|------|---------|
| 2026-08-07 | 全面改版。Entity<TId> が集約固有のID（XXXId）を使用するように更新。EventId と AggregateRootId の役割分離を明確化。RowId をテーブル物理キーに限定 |

