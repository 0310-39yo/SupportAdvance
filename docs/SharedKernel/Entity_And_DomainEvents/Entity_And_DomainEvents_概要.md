# Entity基底クラス＆ドメインイベント設計書群

## 📚 ドキュメント構成

このフォルダには、Entity基底クラスとドメインイベント機構の実装に関する包括的なドキュメントが含まれています。

### ドキュメント一覧

| ドキュメント | 対象者 | 目的 |
|---|---|---|
| **[Entity_And_DomainEvents_技術仕様.md](Entity_And_DomainEvents_技術仕様.md)** | 開発者（使用者） | Entity と IDomainEvent の使用方法、インターフェース仕様 |
| **[Entity_And_DomainEvents_詳細設計.md](Entity_And_DomainEvents_詳細設計.md)** | AI実装者 | 実装指示書、クラス構成、メソッド詳細、検証ルール |
| **[Entity_And_DomainEvents_単体テスト仕様.md](Entity_And_DomainEvents_単体テスト仕様.md)** | テスト実装者 | テストケース仕様、テストシナリオ、期待動作 |

---

## 🎯 実装目的

### Domain層の設計原則遵守

- Domain層はビジネスロジックの純粋性を保つ
- ドメインイベント発行で Application層への疎結合通知を実現
- LocalDateTime（JST）による統一的な日時管理

### 監査・ロギング対応

- 状態変化をドメインイベントで記録
- Application層でイベント処理・ログ出力
- 将来的な監査ログ自動生成基盤

---

## 📋 実装スコープ

### SharedKernel に実装する成果物

```
src/SharedKernel/
├── Entities/
│   ├── Abstractions/
│   │   ├── IDomainEvent.cs           # ドメインイベント基本インターフェース
│   │   ├── Entity.cs                 # Entity基底クラス
│   │   └── AggregateRoot.cs          # AggregateRoot基底クラス
│   └── DomainEvents/
│       └── IDomainEventHandler.cs    # イベントハンドラーインターフェース
```

### 主要クラス

1. **IDomainEvent**
   - ドメインイベントのマーカーインターフェース
   - OccurredAt（LocalDateTime）を必須で持つ

2. **Entity<TId>**
   - ドメインオブジェクト基底クラス
   - DomainEvents リスト管理
   - RaiseDomainEvent() メソッド提供
   - 等価性判定（TId ベース）

3. **AggregateRoot<TId>**
   - Entity<TId> を継承
   - ドメイン集約ルートの基盤
   - トランザクション境界を表現

4. **IDomainEventHandler<TEvent>**
   - Application層でのイベント消費インターフェース
   - HandleAsync(TEvent) を定義

---

## 🔄 実装フロー

### フェーズ1: ドキュメント確認（このステップ）
- [ ] Technical_Specification.md を読むこと
- [ ] Detailed_Design.md で実装詳細を確認
- [ ] Unit_Test_Specification.md でテスト要件を理解

### フェーズ2: 実装（AI実装者向け）
- [ ] Detailed_Design.md に基づいて IDomainEvent 実装
- [ ] Entity<TId> 実装
- [ ] AggregateRoot<TId> 実装
- [ ] IDomainEventHandler<TEvent> 実装

### フェーズ3: 単体テスト
- [ ] Unit_Test_Specification.md に基づいてテスト実装
- [ ] すべてのテストケースが GREEN であることを確認

### フェーズ4: インテグレーション
- [ ] CarPreferences.Domain で Entity を継承した具体クラスを実装
- [ ] Application層でイベントハンドラーを実装

---

## 📐 アーキテクチャ図

```
┌─────────────────────────────────────┐
│       Application層                  │
│  ┌──────────────────────────────┐   │
│  │  UseCase                     │   │
│  │  - Entity の状態変更を実行   │   │
│  │  - DomainEvents を取得       │   │
│  │  - EventDispatcher に渡す    │   │
│  └──────────────────────────────┘   │
│            ↑                         │
│            │ DomainEvents取得       │
└────────────┼─────────────────────────┘
             │
┌────────────┼─────────────────────────┐
│    Domain層                          │
│  ┌──────────────────────────────┐   │
│  │  Entity<TId> (基底)          │   │
│  │  ┌────────────────────────┐  │   │
│  │  │ protected List<        │  │   │
│  │  │   IDomainEvent>        │  │   │
│  │  │   DomainEvents         │  │   │
│  │  │                        │  │   │
│  │  │ protected void         │  │   │
│  │  │   RaiseDomainEvent(    │  │   │
│  │  │   IDomainEvent)        │  │   │
│  │  └────────────────────────┘  │   │
│  └──────────────────────────────┘   │
│            ↑                         │
│            │ 継承                    │
│  ┌──────────────────────────────┐   │
│  │  UserPreferences :           │   │
│  │  AggregateRoot<long>         │   │
│  │  （RowId ベース識別子）        │   │
│  │  public void UpdateXxx()     │   │
│  │  {                           │   │
│  │    // ビジネスロジック        │   │
│  │    RaiseDomainEvent(         │   │
│  │      new XxxChangedEvent()   │   │
│  │    );                        │   │
│  │  }                           │   │
│  └──────────────────────────────┘   │
└────────────────────────────────────┘
```

---

## 💡 使用例（イメージ）

### Domain層：Entity でイベント発行

```csharp
// Domain層
public class UserPreferences : AggregateRoot<RowId>  // RowId ValueObject ベース
{
    private RespondentPersonId _userId;             // ビジネス識別子
    private CarModel? _preferredModel;

    public void UpdatePreferredModel(CarModel model, IClock clock)
    {
        var oldModel = _preferredModel;
        _preferredModel = model;

        // イベント発行（ログなし）
        this.RaiseDomainEvent(new PreferencesUpdatedEvent(
            DomainEventId.New(),  // イベント ID（GUID ValueObject）
            PreferenceChangeType.ModelUpdated,
            oldModel?.ToString() ?? "未設定",
            model.ToString(),
            clock.JstNow  // LocalDateTime
        ));
    }
}

// Domain層：イベント定義
public class PreferencesUpdatedEvent : IDomainEvent
{
    public DomainEventId EventId { get; }           // イベント一意識別子（GUID ValueObject）
    public PreferenceChangeType ChangeType { get; }
    public string OldValue { get; }
    public string NewValue { get; }
    public LocalDateTime OccurredAt { get; }

    public PreferencesUpdatedEvent(
        DomainEventId eventId,
        PreferenceChangeType changeType,
        string oldValue,
        string newValue,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        EventId = eventId;
        ChangeType = changeType;
        OldValue = oldValue;
        NewValue = newValue;
        OccurredAt = occurredAt;
    }
}
```

### Application層：イベント処理

```csharp
// Application層
public class UpdatePreferencesUseCase : IUseCase<UpdatePreferencesRequest, UpdatePreferencesResponse>
{
    private readonly IPreferencesRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;

    public async Task<UpdatePreferencesResponse> ExecuteAsync(UpdatePreferencesRequest request)
    {
        var preferences = await _repository.GetAsync(request.UserId);
        preferences.UpdatePreferences(request.Model, _clock);

        // イベント処理
        foreach (var @event in preferences.GetDomainEvents())
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        await _repository.SaveAsync(preferences);
        return new UpdatePreferencesResponse { Success = true };
    }
}

// Application層：イベントハンドラー
public class PreferencesUpdatedEventHandler : IDomainEventHandler<PreferencesUpdatedEvent>
{
    private readonly IAppLogging<PreferencesUpdatedEventHandler> _logger;

    public async Task HandleAsync(PreferencesUpdatedEvent @event)
    {
        // ここでログ出力
        _logger.LogInformation(
            $"Preferences updated - UserId: {@event.UserId}, " +
            $"Model: {@event.Model.Name}, " +
            $"At: {@event.OccurredAt}");
    }
}
```

---

## 🔍 参考資料

- [CLAUDE.md - ルートプロジェクト設計原則](../../../CLAUDE.md)
- [Domain_Logging_Architecture.md - ドメイン層ログ設計](../../Assistance/Guides/Domain_Logging_Architecture.md)

---

## 📝 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-08-01 | 初版作成。ドキュメント群の概要と実装スコープを定義 |
