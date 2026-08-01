# CarPreferences Context - イベントフロー図

**対象者**: 全員  
**目的**: ドメインイベント駆動フローの可視化

---

## 🔄 全体フロー

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Presentation層（UI/API）                          │
│  POST /api/users/{id}/preferences - UpdatePreferencesRequest        │
└────────────────────────┬────────────────────────────────────────────┘
                         │ リクエスト
                         ↓
┌─────────────────────────────────────────────────────────────────────┐
│               Application層（UseCases）                               │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │ UpdatePreferencesUseCase.ExecuteAsync(request)                  │ │
│ │                                                                  │ │
│ │ [1] パラメータ検証                                               │ │
│ │     - 権限チェック                                               │ │
│ │     - ユーザー存在確認                                           │ │
│ │                                                                  │ │
│ │ [2] Domain層呼び出し                                             │ │
│ │     UserPreferences entity = await repo.GetAsync(userId)        │ │
│ │     entity.UpdatePreferences(model, type, ..., clock)           │ │
│ │     ↓                                                            │ │
│ └─────────────────────────┬──────────────────────────────────────┘ │
│                           │ DomainEvents 取得                       │
│ ┌─────────────────────────↓──────────────────────────────────────┐ │
│ │ [3] イベント処理（EventDispatcher）                              │ │
│ │                                                                  │ │
│ │ foreach (var @event in entity.DomainEvents)                     │ │
│ │ {                                                                │ │
│ │    await eventDispatcher.DispatchAsync(@event)                  │ │
│ │ }                                                                │ │
│ │                                                                  │ │
│ └─────────────┬──────────────────┬──────────────────┬──────────────┘ │
│               │                  │                  │                │
└───────────────┼──────────────────┼──────────────────┼────────────────┘
                │                  │                  │
                ↓                  ↓                  ↓
      ┌──────────────────────────────────────────────────────┐
      │          Application層（イベントハンドラー）            │
      │                                                       │
      │  PreferencesUpdatedEventHandler                       │
      │  ├─ ログ出力                                          │
      │  ├─ 監査ログ記録                                      │
      │  └─ ユーザー通知（メール）                             │
      │                                                       │
      │  BudgetUpdatedEventHandler                           │
      │  ├─ キャッシュ無効化                                  │
      │  └─ 検索インデックス更新                               │
      │                                                       │
      │  BodyTypeUpdatedEventHandler                         │
      │  ├─ 検索インデックス更新                               │
      │  └─ マッチング再計算                                  │
      └──────────────────────────────────────────────────────┘
                │
                ↓
      ┌──────────────────────────────────────────────────────┐
      │     Infrastructure層（外部システム連携）               │
      │                                                       │
      │  ・ログシステム（NLog）                              │
      │  ・メール送信（SMTP）                                 │
      │  ・キャッシュ（Redis等）                              │
      │  ・検索エンジン（Elasticsearch等）                    │
      └──────────────────────────────────────────────────────┘
```

---

## 📊 UpdatePreferences シーケンス図

```
Client              Controller        UseCase          Domain            EventDispatcher     Handlers
  │                    │                  │              │                   │                │
  │ POST /preferences  │                  │              │                   │                │
  ├───────────────────→│                  │              │                   │                │
  │                    │ UpdateRequest    │              │                   │                │
  │                    ├─────────────────→│              │                   │                │
  │                    │                  │ GetAsync()   │                   │                │
  │                    │                  ├─────────────────→                │                │
  │                    │                  │              entity              │                │
  │                    │                  │←─────────────────┤                │                │
  │                    │                  │              │                   │                │
  │                    │                  │ UpdatePreferences(model, ...)    │                │
  │                    │                  ├─────────────→│                   │                │
  │                    │                  │              │                   │                │
  │                    │                  │              ├─ PreferencesUpdatedEvent         │
  │                    │                  │              ├─ BudgetUpdatedEvent              │
  │                    │                  │              ├─ BodyTypeUpdatedEvent           │
  │                    │                  │              │                   │                │
  │                    │                  │←─────────────┤                   │                │
  │                    │                  │ DomainEvents │                   │                │
  │                    │                  │              │                   │                │
  │                    │                  │ SaveAsync()  │                   │                │
  │                    │                  ├─────────────────────────→         │                │
  │                    │                  │              │                   │                │
  │                    │                  │ for each @event in DomainEvents  │                │
  │                    │                  ├────────────────────────────────→  │                │
  │                    │                  │              │                   │                │
  │                    │                  │              │                   ├─→ Handler1    │
  │                    │                  │              │                   │  (LogHandler)  │
  │                    │                  │              │                   │  (MailHandler) │
  │                    │                  │              │                   │                │
  │                    │                  │              │                   ├─→ Handler2    │
  │                    │                  │              │                   │  (CacheHandler)│
  │                    │                  │              │                   │                │
  │                    │                  │              │                   ├─→ Handler3    │
  │                    │                  │              │                   │  (IndexHandler)│
  │                    │                  │              │                   │                │
  │                    │                  │ ClearDomainEvents()            │                │
  │                    │                  ├─────────────→│                   │                │
  │                    │                  │              │                   │                │
  │                    │                  │ Response 200 │                   │                │
  │                    │←─────────────────┤              │                   │                │
  │←───────────────────┤                  │              │                   │                │
```

---

## 🎯 イベント発行フロー（更新時）

### シーン: ユーザーが希望車種を変更

```
ユーザー入力
  ↓
UpdatePreferencesUseCase.ExecuteAsync(request)
  ├─ [1] パラメータ検証
  │       └─ userId, model が妥当か確認
  │
  ├─ [2] Entity 取得
  │       └─ repository.GetAsync(userId)
  │           → UserPreferences entity
  │
  ├─ [3] Domain層呼び出し
  │       └─ entity.UpdatePreferredModel(newModel, clock)
  │           │
  │           ├─ _preferredModel = newModel
  │           ├─ _updatedAt = clock.JstNow
  │           │
  │           └─ this.RaiseDomainEvent(new PreferencesUpdatedEvent(
  │                   userId, changeType, oldValue, newValue, clock.JstNow
  │               ))
  │               → イベントが _domainEvents リストに追加
  │
  ├─ [4] イベント取得
  │       └─ events = entity.DomainEvents
  │           → [PreferencesUpdatedEvent]
  │
  ├─ [5] イベント処理
  │       └─ foreach (var @event in events)
  │           {
  │               await eventDispatcher.DispatchAsync(@event);
  │           }
  │           ├─ PreferencesUpdatedEventHandler.HandleAsync()
  │           │   └─ _logger.LogInformation("...")
  │           └─ PreferencesUpdatedEventHandler (別実装)
  │               └─ await _mailService.SendAsync(...)
  │
  ├─ [6] 保存
  │       └─ repository.SaveAsync(entity)
  │           → DB に保存
  │
  └─ [7] イベントクリア
          └─ entity.ClearDomainEvents()
              → _domainEvents リストを空にする（重複発行防止）
```

---

## 📈 複数イベント発行フロー

```
entity.UpdatePreferences(
    model: Tesla Model 3,
    bodyType: SUV,
    prefersAutomatic: true,
    budgetFrom: ¥2.5M,
    budgetTo: ¥4M,
    clock
)
  │
  ├─ [1] model が null でない
  │       └─ entity.UpdatePreferredModel(model, clock)
  │           └─ RaiseDomainEvent(PreferencesUpdatedEvent)
  │               → _domainEvents[0]
  │
  ├─ [2] bodyType が値を持つ
  │       └─ entity.UpdateBodyType(bodyType, clock)
  │           └─ RaiseDomainEvent(BodyTypeUpdatedEvent)
  │               → _domainEvents[1]
  │
  ├─ [3] prefersAutomatic が値を持つ
  │       └─ entity.UpdateTransmissionPreference(prefersAutomatic, clock)
  │           └─ RaiseDomainEvent(TransmissionPreferenceUpdatedEvent)
  │               → _domainEvents[2]
  │
  ├─ [4] budget パラメータが有効
  │       └─ entity.UpdateBudget(budgetFrom, budgetTo, clock)
  │           └─ RaiseDomainEvent(BudgetUpdatedEvent)
  │               → _domainEvents[3]
  │
  └─ Application層でイベント処理
      ├─ DispatchAsync(PreferencesUpdatedEvent)
      ├─ DispatchAsync(BodyTypeUpdatedEvent)
      ├─ DispatchAsync(TransmissionPreferenceUpdatedEvent)
      └─ DispatchAsync(BudgetUpdatedEvent)
```

---

## 🔌 EventDispatcher の役割

```
EventDispatcher.DispatchAsync(IDomainEvent @event)
  │
  ├─ @event の型を判定
  │
  ├─ if (@event is PreferencesUpdatedEvent)
  │   └─ IEnumerable<IDomainEventHandler<PreferencesUpdatedEvent>> handlers
  │       ├─ PreferencesUpdatedEventHandler (ログ)
  │       ├─ PreferencesUpdatedEventHandler (メール)
  │       └─ PreferencesUpdatedEventHandler (監査ログ)
  │       └─ foreach (var handler in handlers)
  │           └─ await handler.HandleAsync(@event)
  │
  ├─ if (@event is BudgetUpdatedEvent)
  │   └─ handlers: [BudgetUpdatedEventHandler]
  │       └─ await handler.HandleAsync(@event)
  │           └─ キャッシュ無効化
  │           └─ インデックス更新
  │
  └─ ...他のイベント型
```

---

## 📝 重要な原則

### 1. イベント発行は Entity 内で

```csharp
// ✅ 正しい（Domain層）
public class UserPreferences : AggregateRoot<UserId>
{
    public void UpdateModel(CarModel model, IClock clock)
    {
        _model = model;
        this.RaiseDomainEvent(new PreferencesUpdatedEvent(...));
    }
}

// ❌ 間違い（Application層で発行）
public class UpdatePreferencesUseCase
{
    public async Task ExecuteAsync(...)
    {
        entity.UpdateModel(model);
        // ここでイベント作成・発行してはいけない
        this.RaiseDomainEvent(...);  // ❌ Domain層外での発行
    }
}
```

### 2. イベント処理は Application層で

```csharp
// ✅ 正しい（Application層）
public class PreferencesUpdatedEventHandler : IDomainEventHandler<PreferencesUpdatedEvent>
{
    public async Task HandleAsync(PreferencesUpdatedEvent @event)
    {
        // ログ、メール、キャッシュクリアなど
        _logger.LogInformation("Preferences updated");
        await _mailService.SendAsync(...);
    }
}

// ❌ 間違い（Domain層でハンドリング）
public class UserPreferences
{
    public void UpdateModel(CarModel model, IClock clock)
    {
        _model = model;
        // ハンドラーを呼び出してはいけない
        _eventHandler.Handle(new PreferencesUpdatedEvent(...));  // ❌
    }
}
```

### 3. LocalDateTime (JST) で統一

```csharp
// ✅ 正しい
this.RaiseDomainEvent(new PreferencesUpdatedEvent(
    userId,
    changeType,
    oldValue,
    newValue,
    clock.JstNow  // LocalDateTime
));

// ❌ 間違い
this.RaiseDomainEvent(new PreferencesUpdatedEvent(
    userId,
    changeType,
    oldValue,
    newValue,
    DateTime.UtcNow  // DateTime.UtcNow は禁止
));
```

---

## 🔗 関連ドキュメント

- [01_Entity_Design.md](01_Entity_Design.md) - Entity 設計
- [02_DomainEvents_Design.md](02_DomainEvents_Design.md) - イベント設計
- [04_Application_Design.md](04_Application_Design.md) - Application層設計

---

## 📝 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-08-01 | 初版作成。フロー図とシーケンス図を図解 |
