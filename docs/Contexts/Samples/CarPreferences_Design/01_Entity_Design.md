# CarPreferences Context - Entity 設計書

**対象者**: 開発者  
**目的**: UserPreferences Entity の詳細設計

---

## 📐 Entity 概要

### UserPreferences AggregateRoot

**責務**:
- ユーザーの自動車関連の好みを管理
- 好み変更時にドメインイベントを発行
- ビジネスルール検証

**ID型**: `UserId`（ValueObject）

**生存期間**: ユーザー登録～退会まで

---

## 📋 プロパティ一覧

| プロパティ | 型 | 説明 | 必須 | ビジネスルール |
|---|---|---|---|---|
| **Id** | UserId | ユーザーID（識別子） | ✅ | 不変 |
| **PreferredModel** | CarModel | 希望車種 | ❌ | 妥当性チェックは Application層 |
| **PreferredBodyType** | BodyType | 希望ボディタイプ | ❌ | 選択肢のみ許可 |
| **PrefersAutomatic** | bool | オートマ希望 | ✅ | デフォルト: true |
| **BudgetFrom** | Money? | 予算下限 | ❌ | null 許容 |
| **BudgetTo** | Money? | 予算上限 | ❌ | null 許容、From < To を検証 |
| **RespondentAt** | RespondentAt | 回答日時 | ✅ | LocalDateTime（JST） |
| **UpdatedAt** | LocalDateTime | 最終更新日時 | ✅ | LocalDateTime（JST） |
| **CreatedAt** | LocalDateTime | 作成日時 | ✅ | LocalDateTime（JST） |

---

## 🔧 メソッド一覧

### 1. コンストラクタ

```csharp
public UserPreferences(UserId id, LocalDateTime respondedAt, IClock clock)
{
    // 【責務】Entity の初期化
    // - Id を設定
    // - RespondentAt を設定（JST）
    // - CreatedAt と UpdatedAt を clock.JstNow で設定
    // - PrefersAutomatic をデフォルト値 true で初期化
}
```

**パラメータ**:
- `UserId id`: ユーザーID（必須）
- `LocalDateTime respondedAt`: ユーザーが質問に回答した日時
- `IClock clock`: 現在時刻を取得するための Clock インターフェース

**検証**:
- id が null の場合は ArgumentNullException
- respondedAt が未来日の場合は ArgumentException

---

### 2. UpdatePreferredModel

```csharp
public void UpdatePreferredModel(CarModel model, IClock clock)
{
    // 【責務】希望車種を更新してイベント発行
    ArgumentNullException.ThrowIfNull(model);
    
    var oldModel = _preferredModel;
    _preferredModel = model;
    _updatedAt = clock.JstNow;
    
    this.RaiseDomainEvent(new PreferencesUpdatedEvent(
        this.Id,
        PreferenceChangeType.ModelUpdated,
        oldModel?.Name ?? "未設定",
        model.Name,
        clock.JstNow
    ));
}
```

**パラメータ**:
- `CarModel model`: 新しい希望車種（not null）
- `IClock clock`: 更新時刻取得用

**発行イベント**: `PreferencesUpdatedEvent` (Type: ModelUpdated)

**検証**:
- model が null の場合は ArgumentNullException
- 同じモデルへの更新でもイベント発行（トレーサビリティ重視）

---

### 3. UpdateBudget

```csharp
public void UpdateBudget(Money? from, Money? to, IClock clock)
{
    // 【責務】予算範囲を更新してイベント発行
    // ビジネスルール: from != null && to != null の場合、from <= to を検証
    
    if (from.HasValue && to.HasValue && from > to)
    {
        throw new ArgumentException(
            "Budget.From must be less than or equal to Budget.To",
            nameof(from));
    }
    
    var oldFrom = _budgetFrom;
    var oldTo = _budgetTo;
    
    _budgetFrom = from;
    _budgetTo = to;
    _updatedAt = clock.JstNow;
    
    this.RaiseDomainEvent(new BudgetUpdatedEvent(
        this.Id,
        oldFrom, oldTo,
        from, to,
        clock.JstNow
    ));
}
```

**パラメータ**:
- `Money? from`: 予算下限（null 許容）
- `Money? to`: 予算上限（null 許容）
- `IClock clock`: 更新時刻取得用

**発行イベント**: `BudgetUpdatedEvent`

**ビジネスルール**:
- from と to の両方が値を持つ場合: `from <= to` を検証
- from のみ値を持つ: to は null（下限のみ設定）
- to のみ値を持つ: from は null（上限のみ設定）
- 両方 null: 予算制限なし

---

### 4. UpdatePreferences (統合メソッド)

```csharp
public void UpdatePreferences(
    CarModel? model,
    BodyType? bodyType,
    bool? prefersAutomatic,
    Money? budgetFrom,
    Money? budgetTo,
    IClock clock)
{
    // 【責務】複数の好みを一括更新
    
    if (model != null)
    {
        UpdatePreferredModel(model, clock);
    }
    
    if (bodyType.HasValue)
    {
        UpdateBodyType(bodyType.Value, clock);
    }
    
    if (prefersAutomatic.HasValue && prefersAutomatic != _prefersAutomatic)
    {
        UpdateTransmissionPreference(prefersAutomatic.Value, clock);
    }
    
    if (budgetFrom.HasValue || budgetTo.HasValue)
    {
        UpdateBudget(budgetFrom, budgetTo, clock);
    }
}
```

**用途**: Application層の UpdatePreferencesUseCase で呼び出し

**注記**: 各プロパティが値を持つ場合のみ更新、イベントも各メソッドで発行

---

### 5. UpdateBodyType

```csharp
public void UpdateBodyType(BodyType bodyType, IClock clock)
{
    // 【責務】ボディタイプを更新してイベント発行
    
    if (_preferredBodyType == bodyType)
    {
        return;  // 変更なし
    }
    
    var oldBodyType = _preferredBodyType;
    _preferredBodyType = bodyType;
    _updatedAt = clock.JstNow;
    
    this.RaiseDomainEvent(new BodyTypeUpdatedEvent(
        this.Id,
        oldBodyType,
        bodyType,
        clock.JstNow
    ));
}
```

---

### 6. UpdateTransmissionPreference

```csharp
public void UpdateTransmissionPreference(bool prefersAutomatic, IClock clock)
{
    // 【責務】トランスミッション希望を更新してイベント発行
    
    if (_prefersAutomatic == prefersAutomatic)
    {
        return;  // 変更なし
    }
    
    var oldPreference = _prefersAutomatic;
    _prefersAutomatic = prefersAutomatic;
    _updatedAt = clock.JstNow;
    
    this.RaiseDomainEvent(new TransmissionPreferenceUpdatedEvent(
        this.Id,
        oldPreference,
        prefersAutomatic,
        clock.JstNow
    ));
}
```

---

## 📊 Entity ライフサイクル

```
ユーザー登録
  ↓
[1] UserPreferences インスタンス作成
    └─ コンストラクタで初期化
       ├─ Id: UserId
       ├─ RespondentAt: 回答日時
       ├─ CreatedAt/UpdatedAt: 現在時刻
       └─ イベント未発行
  ↓
[2] 好み更新
    ├─ UpdatePreferredModel() → PreferencesUpdatedEvent
    ├─ UpdateBudget() → BudgetUpdatedEvent
    └─ UpdateBodyType() → BodyTypeUpdatedEvent
  ↓
[3] Application層でイベント処理
    ├─ ログ出力
    ├─ 監査記録
    └─ 他システム通知
  ↓
[4] リポジトリに保存
    └─ イベントをクリア（ClearDomainEvents）
  ↓
ユーザー退会
  ↓
[5] Entity 削除
```

---

## 🔍 ビジネスルール検証

### どこで検証するか

| ルール | 検証場所 | 例外型 |
|---|---|---|
| id が null でない | Entity コンストラクタ | ArgumentNullException |
| RespondentAt が未来日でない | Entity コンストラクタ | ArgumentException |
| Budget.From <= Budget.To | UpdateBudget() メソッド | ArgumentException |
| BodyType は有効な選択肢 | BodyType ValueObject | ArgumentException |
| Money 値は非負 | Money ValueObject | ArgumentException |

---

## 🧪 テスト観点

### Entity テストケース

1. **コンストラクタ**
   - 正常系：有効なパラメータで Entity 作成
   - 異常系：null ID で ArgumentNullException
   - 異常系：未来日の RespondentAt で ArgumentException

2. **UpdatePreferredModel**
   - イベント発行確認
   - UpdatedAt が現在時刻に更新される
   - 同じモデルへの更新でもイベント発行

3. **UpdateBudget**
   - From > To で ArgumentException
   - From のみ、To のみ、両方 null のパターン
   - ビジネスルール検証

4. **UpdatePreferences (統合)**
   - 複数変更時に複数イベント発行
   - null パラメータは無視

---

## 📝 ValueObject 依存関係

以下の ValueObject が必要：

| ValueObject | 説明 | 既存 | 新規 |
|---|---|---|---|
| UserId | ユーザーID | ✅ | - |
| CarModel | 自動車モデル | ✅ | - |
| BodyType | ボディタイプ（Sedan, SUV等） | ❌ | 📝 要実装 |
| Money | 金額 | ❌ | 📝 要実装 |
| RespondentAt | 回答日時 | ✅ | - |

---

## 🔗 関連ドキュメント

- [02_DomainEvents_Design.md](02_DomainEvents_Design.md) - イベント設計
- [03_EventFlow_Diagram.md](03_EventFlow_Diagram.md) - フロー図
- [04_Application_Design.md](04_Application_Design.md) - Application層設計

---

## 📝 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-08-01 | 初版作成。UserPreferences Entity の詳細設計 |
