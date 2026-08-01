# CarPreferences Context - ドメインイベント設計書

**対象者**: 開発者  
**目的**: UserPreferences で発行するドメインイベントの詳細設計

---

## 🎯 ドメインイベント概要

UserPreferences での状態変化は、以下のドメインイベントで通知されます。

**発行タイミング**: Entity メソッド実行時（ビジネスロジック実行後）  
**消費者**: Application層のイベントハンドラー

---

## 📋 イベント一覧

| # | イベント名 | 発行条件 | 用途 |
|---|---|---|---|
| 1 | PreferencesUpdatedEvent | 希望車種が変更 | ログ、監査ログ、通知 |
| 2 | BudgetUpdatedEvent | 予算範囲が変更 | ログ、キャッシュ更新 |
| 3 | BodyTypeUpdatedEvent | ボディタイプが変更 | ログ、検索インデックス更新 |
| 4 | TransmissionPreferenceUpdatedEvent | トランスミッション希望が変更 | ログ |
| 5 | PreferencesCreatedEvent | ユーザー初回設定時 | ログ、ウェルカムメール送信 |

---

## 📝 イベント詳細設計

### 1. PreferencesUpdatedEvent

**発行**: `UserPreferences.UpdatePreferredModel()`

**構造**:
```csharp
public class PreferencesUpdatedEvent : IDomainEvent
{
    /// <summary>ユーザーID</summary>
    public UserId UserId { get; }

    /// <summary>変更タイプ（ModelUpdated）</summary>
    public PreferenceChangeType ChangeType { get; }

    /// <summary>変更前の値</summary>
    public string OldValue { get; }

    /// <summary>変更後の値</summary>
    public string NewValue { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public PreferencesUpdatedEvent(
        UserId userId,
        PreferenceChangeType changeType,
        string oldValue,
        string newValue,
        LocalDateTime occurredAt)
    {
        UserId = userId;
        ChangeType = changeType;
        OldValue = oldValue;
        NewValue = newValue;
        OccurredAt = occurredAt;
    }
}
```

**OccurredAt**: `clock.JstNow`（Entity メソッド内で取得）

**用途**:
- ✅ ログ出力（変更内容記録）
- ✅ 監査ログ（何が変わったか）
- ✅ ユーザー通知（メール・アプリ通知）

**例**:
```
ユーザー "user-001" が希望車種を "Toyota Prius" から "Tesla Model 3" に変更しました
```

---

### 2. BudgetUpdatedEvent

**発行**: `UserPreferences.UpdateBudget()`

**構造**:
```csharp
public class BudgetUpdatedEvent : IDomainEvent
{
    /// <summary>ユーザーID</summary>
    public UserId UserId { get; }

    /// <summary>変更前の下限</summary>
    public Money? OldBudgetFrom { get; }

    /// <summary>変更前の上限</summary>
    public Money? OldBudgetTo { get; }

    /// <summary>変更後の下限</summary>
    public Money? NewBudgetFrom { get; }

    /// <summary>変更後の上限</summary>
    public Money? NewBudgetTo { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public BudgetUpdatedEvent(
        UserId userId,
        Money? oldFrom,
        Money? oldTo,
        Money? newFrom,
        Money? newTo,
        LocalDateTime occurredAt)
    {
        UserId = userId;
        OldBudgetFrom = oldFrom;
        OldBudgetTo = oldTo;
        NewBudgetFrom = newFrom;
        NewBudgetTo = newTo;
        OccurredAt = occurredAt;
    }
}
```

**用途**:
- ✅ ログ出力（予算範囲の変更）
- ✅ キャッシュ無効化（ユーザーの予算フィルター結果キャッシュをクリア）
- ✅ 検索インデックス更新（予算条件でのフィルタリング対象更新）

**例**:
```
予算範囲が ¥2,000,000-¥3,000,000 から ¥2,500,000-¥4,000,000 に変更
```

---

### 3. BodyTypeUpdatedEvent

**発行**: `UserPreferences.UpdateBodyType()`

**構造**:
```csharp
public class BodyTypeUpdatedEvent : IDomainEvent
{
    /// <summary>ユーザーID</summary>
    public UserId UserId { get; }

    /// <summary>変更前のボディタイプ</summary>
    public BodyType? OldBodyType { get; }

    /// <summary>変更後のボディタイプ</summary>
    public BodyType NewBodyType { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public BodyTypeUpdatedEvent(
        UserId userId,
        BodyType? oldBodyType,
        BodyType newBodyType,
        LocalDateTime occurredAt)
    {
        UserId = userId;
        OldBodyType = oldBodyType;
        NewBodyType = newBodyType;
        OccurredAt = occurredAt;
    }
}
```

**用途**:
- ✅ ログ出力
- ✅ 検索インデックス更新（ボディタイプフィルター対応）
- ✅ マッチング関数の再計算

**例**:
```
ボディタイプが "Sedan" から "SUV" に変更
```

---

### 4. TransmissionPreferenceUpdatedEvent

**発行**: `UserPreferences.UpdateTransmissionPreference()`

**構造**:
```csharp
public class TransmissionPreferenceUpdatedEvent : IDomainEvent
{
    /// <summary>ユーザーID</summary>
    public UserId UserId { get; }

    /// <summary>変更前の値（true: オートマ, false: マニュアル）</summary>
    public bool OldPreference { get; }

    /// <summary>変更後の値</summary>
    public bool NewPreference { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public TransmissionPreferenceUpdatedEvent(
        UserId userId,
        bool oldPreference,
        bool newPreference,
        LocalDateTime occurredAt)
    {
        UserId = userId;
        OldPreference = oldPreference;
        NewPreference = newPreference;
        OccurredAt = occurredAt;
    }
}
```

**用途**:
- ✅ ログ出力
- ✅ ユーザー通知

**例**:
```
トランスミッション希望が "オートマ" から "マニュアル" に変更
```

---

### 5. PreferencesCreatedEvent

**発行**: ユーザー初回アンケート回答完了時

**シーン**: 新規ユーザーが最初のプリファレンス設定を完了

**構造**:
```csharp
public class PreferencesCreatedEvent : IDomainEvent
{
    /// <summary>ユーザーID</summary>
    public UserId UserId { get; }

    /// <summary>作成時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public PreferencesCreatedEvent(
        UserId userId,
        LocalDateTime occurredAt)
    {
        UserId = userId;
        OccurredAt = occurredAt;
    }
}
```

**用途**:
- ✅ ウェルカムメール送信（新規ユーザー歓迎）
- ✅ 初期推奨車種送信
- ✅ ログ記録

**発行方法**: Application層の CreateUserPreferencesUseCase から

---

## 🔄 イベント発行フロー

```
Domain層（Entity）
    ↓
UpdatePreferredModel()
    ├─ ビジネスロジック実行
    │   ├─ 値のコピー
    │   ├─ UpdatedAt 更新
    │   └─ ビジネスルール検証
    │
    └─ RaiseDomainEvent(PreferencesUpdatedEvent)
        └─ イベント追加（_domainEvents リスト）

Application層（UseCase）
    ↓
ExecuteAsync()
    ├─ Entity.UpdatePreferredModel() 呼び出し
    ├─ Entity.DomainEvents 取得
    │
    └─ イベントを EventDispatcher に渡す
        ├─ PreferencesUpdatedEventHandler
        │   └─ ログ出力
        ├─ PreferencesUpdatedEventHandler (別実装)
        │   └─ 通知送信
        └─ ...他のハンドラー
```

---

## 📊 イベント vs ビジネスルール

| 項目 | イベント発行 | ビジネスルール検証 |
|---|---|---|
| **タイミング** | 状態変化後（後付け） | 状態変化前（事前チェック） |
| **目的** | 関心事通知（ログ、外部連携） | 不正状態の防止 |
| **場所** | Entity メソッド内 | Entity or Application層 |
| **例** | PreferencesUpdatedEvent | Budget.From <= Budget.To |

### ビジネスルール検証

以下は**Application層**で検証（Entity では発行しない）：

- ユーザーが実在するか
- 権限があるか
- 同時実行制御

---

## 🧪 テスト観点

### イベント発行テスト

1. **更新メソッド実行時にイベント発行確認**
   ```csharp
   entity.UpdatePreferredModel(newModel, clock);
   
   Assert.Single(entity.DomainEvents);
   Assert.IsType<PreferencesUpdatedEvent>(entity.DomainEvents[0]);
   ```

2. **イベント内容の正確性**
   ```csharp
   var @event = (PreferencesUpdatedEvent)entity.DomainEvents[0];
   Assert.Equal(userId, @event.UserId);
   Assert.Equal(oldModel, @event.OldValue);
   Assert.Equal(newModel, @event.NewValue);
   Assert.Equal(clock.JstNow, @event.OccurredAt);
   ```

3. **複数イベント発行（統合メソッド）**
   ```csharp
   entity.UpdatePreferences(model, bodyType, prefersAuto, from, to, clock);
   
   // 3個のイベント発行を期待
   Assert.Equal(3, entity.DomainEvents.Count);
   ```

---

## 📝 実装チェックリスト

### ドメインイベントクラス実装時

- [ ] IDomainEvent を実装
- [ ] OccurredAt プロパティを LocalDateTime で定義
- [ ] コンストラクタで全プロパティを初期化
- [ ] プロパティは読み取り専用（get のみ）
- [ ] ドメインロジックを含まない（純粋なデータ保持）

### Entity メソッド実装時

- [ ] メソッド内で値を更新
- [ ] RaiseDomainEvent() を呼び出し
- [ ] 発行するイベントは IDomainEvent を実装
- [ ] OccurredAt には clock.JstNow を使用
- [ ] 古い値と新しい値をイベントに含める（監査対応）

---

## 🔗 関連ドキュメント

- [01_Entity_Design.md](01_Entity_Design.md) - Entity 設計
- [03_EventFlow_Diagram.md](03_EventFlow_Diagram.md) - フロー図
- [04_Application_Design.md](04_Application_Design.md) - Application層設計

---

## 📝 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-08-01 | 初版作成。5つのドメインイベント詳細設計 |
