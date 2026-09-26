# Entity基底クラス＆ドメインイベント - 詳細設計書

**対象者**: AI実装者

**目的**: 以下のファイルを実装するための詳細な設計書。AI に指示して実装を完了できるレベルの詳細さを目指す。

---

## 📋 実装ファイル一覧

```
src/SharedKernel/ValueObjects/Identifiers/
└── AggregateId.cs                  ← 実装対象（集約ID基底クラス）

src/SharedKernel/Entities/
├── Abstractions/
│   ├── IDomainEvent.cs              ← 実装対象
│   ├── Entity.cs                    ← 実装対象
│   └── AggregateRoot.cs             ← 実装対象
└── DomainEvents/
    └── IDomainEventHandler.cs       ← 実装対象
```

---

## 0️⃣ AggregateId.cs（基底クラス）

### 目的
集約を一意に識別する GUID ベースの ValueObject 基底クラス。すべての集約ID（OrderId, EmployeeId など）がこれを継承。

### 実装仕様

```csharp
namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 集約を一意識別する GUID ベース ValueObject の基底クラス
/// 
/// 【責務】
/// - GUID 値の保持
/// - 等価性判定（GUID ベース）
/// - 集約ID の基盤実装
/// 
/// 【継承】
/// 各集約は AggregateId を継承し、固有の ID クラスを定義
/// 例：OrderId, EmployeeId, UserPreferencesId など
/// 
/// 【型安全性】
/// - 異なる集約のID を型チェックで区別
/// - OrderId と EmployeeId は互換性なし
/// 
/// 【使用パターン】
/// public class OrderId : AggregateId
/// {
///     public OrderId(Guid value) : base(value) { }
///     public static OrderId New() => new(Guid.NewGuid());
///     public static OrderId From(Guid value) => new(value);
/// }
/// </summary>
public abstract class AggregateId : ValueObject
{
    /// <summary>
    /// GUID 値
    /// 
    /// 【型】Guid
    /// 【必須】Empty（00000000-0000-0000-0000-000000000000）は不可
    /// 【検証】コンストラクタで Empty チェック
    /// </summary>
    public Guid Value { get; protected set; }

    /// <summary>
    /// コンストラクタ
    /// 
    /// 【パラメータ】value: GUID 値
    /// 【例外】ArgumentException if value == Guid.Empty
    /// </summary>
    /// <param name="value">GUID 値</param>
    /// <exception cref="ArgumentException">value が Empty の場合</exception>
    protected AggregateId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AggregateId cannot be empty.", nameof(value));
        
        Value = value;
    }

    /// <summary>
    /// 等価性判定（GUID ベース）
    /// 
    /// 【実装】ValueObject の GetAtomicValues() で GUID を返す
    /// </summary>
    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    /// <summary>
    /// 文字列表現（デバッグ用）
    /// 
    /// 【形式】{Value}.ToString() （Guid の標準形式）
    /// 【用途】ログ出力、デバッグ
    /// </summary>
    public override string ToString() => Value.ToString();
}
```

### 実装ポイント

1. **ValueObject を継承**
   - 等価性判定は GUID ベース
   - `GetAtomicValues()` で GUID を返す

2. **Empty チェック**
   - コンストラクタで `if (value == Guid.Empty) throw ...`
   - 不正な集約ID を防止

3. **protected コンストラクタ**
   - 派生クラスのみから呼び出し可能

4. **派生クラスの実装パターン**
   ```csharp
   public class OrderId : AggregateId
   {
       public OrderId(Guid value) : base(value) { }
       public static OrderId New() => new(Guid.NewGuid());
       public static OrderId From(Guid value) => new(value);
   }
   ```

### テスト観点
- Empty Guid で例外が発生
- 異なる Guid を持つ AggregateId は非等価
- 同じ Guid を持つ AggregateId は等価
- GetHashCode() が一貫している

---

## 1️⃣ IDomainEvent.cs

### 目的
ドメインイベントの基本インターフェース。すべてのドメインイベントはこれを実装。

### 実装仕様

```csharp
namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ドメインイベントの基本インターフェース
/// 
/// Domain層で発生した重要な事象を表現し、Application層への通知を可能にする。
/// 各ドメインイベント型はこのインターフェースを実装する必要がある。
/// 
/// 【タイムゾーン】すべてのイベント発生時刻は JST（日本標準時）
/// 【不変性】イベントは発行後に変更されない前提
/// 【識別子】
/// - EventId（GUID ValueObject）：このイベント自体を識別
/// - AggregateRootId（集約固有のID）：どの集約が変更されたかを特定
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// イベント一意識別子（GUID ValueObject）
    /// 
    /// 【型】DomainEventId（GUID を保持する ValueObject）
    /// 【用途】このイベント自体を識別、トレーシング
    /// 【生成】毎回新規（DomainEventId.New()）
    /// 【必須】すべてのイベント実装で必ず値を持つこと
    /// </summary>
    DomainEventId EventId { get; }

    /// <summary>
    /// イベント発生時刻（JST）
    /// 
    /// 【型】LocalDateTime（System の型体系を使用）
    /// 【タイムゾーン】常に JST（Tokyo Standard Time）
    /// 【用途】監査ログ、イベント順序付けなど
    /// 【必須】すべてのイベント実装で必ず値を持つこと
    /// 【取得】Clock.JstNow から取得
    /// </summary>
    LocalDateTime OccurredAt { get; }
    
    /// <summary>
    /// 注記：AggregateRootId は具体的なイベント実装で型パラメータとして指定
    /// 
    /// 【型】集約固有のID（OrderId, EmployeeId, UserPreferencesId など）
    /// 【用途】どの集約が変更されたかを特定
    /// 【例】public OrderId AggregateRootId { get; }
    /// </summary>
}
```

### 実装方針

- `interface` として定義（抽象基底クラスではない）
- `EventId` プロパティ（DomainEventId）を追加
- `LocalDateTime` 型の `OccurredAt` プロパティは継続
- `AggregateRootId` は具体的なイベント実装で指定
- XML コメントは詳細に記述
- `DateTime.UtcNow` は絶対に使用しない

### テスト観点
- IDomainEvent を実装したクラスは `EventId` プロパティを必ず持つ
- IDomainEvent を実装したクラスは `OccurredAt` プロパティを必ず持つ
- `EventId` は `DomainEventId` 型
- `OccurredAt` は `LocalDateTime` 型
- 具体的なイベント実装は `AggregateRootId`（集約ID型）を持つ

---

## 2️⃣ Entity.cs

### 目的
ドメインエンティティの基底クラス。すべての Entity はこれを継承。

### 実装仕様

```csharp
namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ドメインエンティティの基底クラス
/// 
/// 【責務1】ビジネスオブジェクト（Entity）を表現
/// 【責務2】ドメインイベント発行・管理
/// 【責務3】Entity の等価性判定（ID ベース）
/// 
/// 【ライフサイクル】
/// - Entity は TId（集約固有のID ValueObject）を持つ
/// - Entity のインスタンスは ID で一意に識別される
/// - 同じ ID を持つ Entity は等価（他のプロパティは無視）
/// 
/// 【ドメインイベント】
/// - Entity がビジネスロジック実行中に発行
/// - Application層でディスパッチされる
/// - イベント処理後は DomainEvents をクリア（Application層の責任）
/// 
/// 【集約ID】TId = 集約固有のID（AggregateId を継承した型）
/// </summary>
/// <typeparam name="TId">
/// エンティティ ID の型
/// 
/// 【制約】
/// - AggregateId を継承した ValueObject であること（例：OrderId, EmployeeId）
/// - notnull（null 許容不可）
/// - Equals / GetHashCode をオーバーライドしていること
/// </typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Entity の識別子（集約固有のID）
    /// 
    /// 【型】TId（AggregateId 継承の ValueObject）
    /// 【例】OrderId, EmployeeId, UserPreferencesId など
    /// 【アクセス】protected set（派生クラスでのみ設定可能）
    /// 【不変性】設定後の変更は推奨されない（ただし強制しない）
    /// 【用途】等価性判定、集約ルートの識別
    /// </summary>
    public TId Id { get; protected set; }

    /// <summary>
    /// 発行されたドメインイベント（読み取り専用）
    /// 
    /// 【要素】IDomainEvent を実装したイベント
    /// 【順序】発行順序を保持
    /// 【寿命】Application層がイベント処理後にクリア
    /// 【アクセス】DomainEvents プロパティで取得
    /// </summary>
    private protected List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// ドメインイベント取得（読み取り専用）
    /// 
    /// 【戻り値】不変リスト（変更不可）
    /// 【用途】Application層でのイベント処理
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// ドメインイベント発行
    /// 
    /// 【責務】ドメインロジック実行中にイベントを発行
    /// 【アクセス】protected（派生 Entity クラスでのみ呼び出し可能）
    /// 【実行階層】Domain層のビジネスメソッド内
    /// 【例外】イベントが null の場合 ArgumentNullException をスロー
    /// 
    /// 【使用例】
    /// protected void UpdateStatus(Status newStatus, IClock clock)
    /// {
    ///     _status = newStatus;
    ///     this.RaiseDomainEvent(new StatusChangedEvent(
    ///         DomainEventId.New(),       // ← イベント自体のID（毎回新規）
    ///         this.Id,                   // ← AggregateRootId（この Entity の ID）
    ///         newStatus,
    ///         clock.JstNow
    ///     ));
    /// }
    /// </summary>
    /// <param name="domainEvent">
    /// 発行するドメインイベント
    /// 
    /// 【要件】
    /// - IDomainEvent を実装
    /// - EventId に有効な DomainEventId を含む
    /// - OccurredAt に有効な LocalDateTime を含む
    /// - null 不可
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// domainEvent が null の場合
    /// </exception>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// ドメインイベントをクリア
    /// 
    /// 【責務】Application層でイベント処理後、イベントリストをクリア
    /// 【アクセス】internal（同じアセンブリ内でのみアクセス可能）
    /// 【呼び出し元】Application層の EventDispatcher など
    /// 【タイミング】すべてのイベント処理完了後
    /// </summary>
    internal void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Entity の等価性判定（ID ベース）
    /// 
    /// 【判定基準】ID のみで判定
    /// 【戻り値】同じ TId 値を持つなら true
    /// 【注記】他のプロパティ値は無視
    /// 
    /// 【例】
    /// var order1 = new Order(OrderId.From(guid123), ...);
    /// var order2 = new Order(OrderId.From(guid123), ...);
    /// Assert.True(order1.Equals(order2));  // ID が同じなら等価
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    /// <summary>
    /// Entity の等価性判定（型安全版）
    /// 
    /// 【判定基準】
    /// 1. 参照が同じなら true
    /// 2. ID が等しいなら true
    /// 3. それ以外は false
    /// 
    /// 【null 対応】other が null なら false
    /// </summary>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    /// <summary>
    /// ハッシュコード取得（ID ベース）
    /// 
    /// 【用途】HashSet, Dictionary など集合型での使用
    /// 【実装】Id.GetHashCode() をそのまま返す
    /// 【注記】Equals をオーバーライドしたので必ず実装
    /// </summary>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Entity の文字列表現
    /// 
    /// 【形式】"{ClassName} {{ Id = {Id} }}"
    /// 【用途】デバッグ時の表示
    /// </summary>
    public override string ToString() => $"{GetType().Name} {{ Id = {Id} }}";
}
```

### 実装ポイント

1. **Generic 型パラメータ**
   - `where TId : notnull` で null を許さない
   - AggregateId 継承の ValueObject を想定

2. **DomainEvents 管理**
   - `private protected List<IDomainEvent>` で内部保持
   - `public IReadOnlyList<IDomainEvent> DomainEvents` で読み取り専用公開
   - `internal void ClearDomainEvents()` でクリア

3. **等価性判定**
   - ID ベースのみ（他のプロパティは無視）
   - `Equals()` と `GetHashCode()` の両方をオーバーライド

4. **RaiseDomainEvent メソッド**
   - `protected` で派生クラスのみアクセス可能
   - null チェック（ArgumentNullException）
   - イベント内に EventId（新規生成）と AggregateRootId（this.Id）を渡す

### テスト観点
- Entity インスタンスは ID で等価と判定される
- 異なる ID を持つ Entity は非等価
- DomainEvents は読み取り専用リスト
- RaiseDomainEvent で null を渡すと例外
- ClearDomainEvents で イベントリストが空になる

---

## 3️⃣ AggregateRoot.cs

### 目的
AggregateRoot の基底クラス。Entity<TId> を継承。

### 実装仕様

```csharp
namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// AggregateRoot（集約ルート）の基底クラス
/// 
/// 【意味論】
/// - AggregateRoot はトランザクション境界を表現
/// - 一度に保存・削除される複数の Entity をグループ化
/// - DDD の集約パターンの実装
/// 
/// 【Entity<TId> との違い】
/// - 機能的には完全に同じ
/// - 意味論的に「これは集約ルートである」ことを表現
/// 
/// 【TId について】
/// - AggregateId を継承した集約固有のID（OrderId, EmployeeId など）
/// - テーブル構成に依存しない論理ID
/// 
/// 【将来拡張】
/// - AggregateRoot 固有の機能（例：子Entity管理）は将来追加予定
/// </summary>
/// <typeparam name="TId">
/// 集約ルート ID の型（AggregateId 継承の ValueObject）
/// </typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    // Entity<TId> を継承
    // 現在は固有の実装なし
    // 将来、子Entity管理などが追加される場合の基盤
}
```

### 実装ポイント

1. **Entity<TId> の単純継承**
   - 現在は機能追加なし
   - 今後、子Entity管理などが追加される場合の基盤

2. **意味論的な分離**
   - Entity<TId>: 単純なドメインオブジェクト
   - AggregateRoot<TId>: 集約ルート（トランザクション境界）
   - 機能的には同じ（意味の違いのみ）

3. **TId について**
   - AggregateId を継承した集約固有のID
   - 例：OrderId, EmployeeId, UserPreferencesId

### テスト観点
- AggregateRoot は Entity と同じ動作
- Entity のテストケースがすべて通ること
- ジェネリック型パラメータが正しく機能

---

## 4️⃣ IDomainEventHandler.cs

### 目的
Application層でドメインイベントを処理するハンドラーのインターフェース。

### 実装仕様

```csharp
namespace SupportAdvance.SharedKernel.Entities.DomainEvents;

/// <summary>
/// ドメインイベントハンドラーのインターフェース
/// 
/// 【責務】Application層でドメインイベントを消費・処理
/// 【実行階層】Application層のみ
/// 【型安全性】Generic で特定イベント型を指定
/// 
/// 【使用方法】
/// 1. IDomainEventHandler<TEvent> を実装したクラスを作成
/// 2. HandleAsync メソッドに処理を実装
/// 3. DI コンテナに登録
/// 4. EventDispatcher がイベント処理時に呼び出し
/// 
/// 【非同期対応】
/// - すべてのハンドラーは async Task で実装
/// - 複数ハンドラーを並行実行可能（将来）
/// 
/// 【AggregateRootId の使用】
/// - イベントの AggregateRootId から対象エンティティを特定
/// - 例：var order = await _repository.GetByIdAsync(@event.AggregateRootId);
/// </summary>
/// <typeparam name="TEvent">
/// 処理するドメインイベント型
/// 
/// 【制約】
/// - IDomainEvent を実装していること
/// </typeparam>
public interface IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// ドメインイベントの処理
    /// 
    /// 【責務】
    /// - イベント情報からログ出力
    /// - 外部システムへの通知（メール送信など）
    /// - 関連データの更新（キャッシュクリアなど）
    /// 
    /// 【実装時の注意】
    /// - Domain層のビジネスロジックを記述しない
    /// - イベント処理結果を Entity に反映しない
    /// - 例外が発生した場合は呼び出し元で処理
    /// 
    /// 【イベント属性の利用】
    /// - AggregateRootId（集約ID）から対象を特定
    /// - EventId（イベント自体のID）でトレーシング
    /// - OccurredAt（発生時刻）でログ記録
    /// 
    /// 【使用例】
    /// public async Task HandleAsync(UserPreferencesUpdatedEvent @event)
    /// {
    ///     // AggregateRootId から対象 UserPreferences を特定
    ///     var userPreferencesId = @event.AggregateRootId;
    ///     
    ///     _logger.LogInformation(
    ///         $"UserPreferences updated - Id: {userPreferencesId}, " +
    ///         $"EventId: {@event.EventId}, " +
    ///         $"At: {@event.OccurredAt}");
    ///     
    ///     // 外部システム通知など
    ///     await _notificationService.NotifyAsync(userPreferencesId);
    /// }
    /// </summary>
    /// <param name="event">
    /// 処理するドメインイベント
    /// 
    /// 【要件】
    /// - null ではない
    /// - EventId に有効な DomainEventId を持つ
    /// - AggregateRootId（集約固有のID）を持つ
    /// - OccurredAt に有効な LocalDateTime を持つ
    /// </param>
    /// <returns>処理完了タスク</returns>
    Task HandleAsync(TEvent @event);
}
```

### 実装ポイント

1. **Generic インターフェース**
   - 型パラメータ `TEvent` で特定イベント型を指定
   - 複数のイベント型に対応するハンドラーを作成可能

2. **非同期メソッド**
   - `async Task` で定義
   - 将来の並行実行に対応

3. **実装者への注意**
   - Application層にのみ実装
   - Domain層のロジックを実装しない
   - AggregateRootId を使用して対象を特定

### テスト観点
- ハンドラーが正しく HandleAsync を呼び出す
- イベントの AggregateRootId を正しく使用
- ログが適切に出力される
- 例外ハンドリング

---

## 🔍 実装上の補足

### LocalDateTime との関係

```csharp
// ❌ Domain層 - 禁止
public class UserPreferences : Entity<UserPreferencesId>
{
    public void Update()
    {
        var now = DateTime.UtcNow;  // ❌ DateTime.UtcNow 禁止
        this.RaiseDomainEvent(new UpdatedEvent(DomainEventId.New(), this.Id, now));
    }
}

// ✅ Domain層 - 正しい
public class UserPreferences : Entity<UserPreferencesId>
{
    public void Update(IClock clock)  // Clock を DI で受け取る
    {
        var now = clock.JstNow;  // ✅ LocalDateTime 使用
        this.RaiseDomainEvent(new UpdatedEvent(
            DomainEventId.New(),
            this.Id,
            now
        ));
    }
}
```

### 依存関係

```
AggregateId.cs
  ↑
  └─ ValueObject

Entity.cs
  ↑
  ├─ 依存なし
  └─ Generic パラメータ TId（AggregateId 継承の ValueObject）

AggregateRoot.cs
  ↑
  └─ Entity<TId>

IDomainEvent.cs
  ↑
  ├─ DomainEventId（SharedKernel.ValueObjects.Identifiers）
  └─ LocalDateTime（Common層）

IDomainEventHandler.cs
  ↑
  └─ IDomainEvent
```

### ファイル配置

```
src/SharedKernel/ValueObjects/Identifiers/
└── AggregateId.cs

src/SharedKernel/Entities/
├── Abstractions/
│   ├── IDomainEvent.cs
│   ├── Entity.cs
│   └── AggregateRoot.cs
└── DomainEvents/
    └── IDomainEventHandler.cs
```

---

## ✅ 実装チェックリスト

### 実装時に確認すること

- [ ] 5つのファイル（AggregateId + 4つ）が上記仕様通りに実装されている
- [ ] XML コメントが適切に記述されている
- [ ] `LocalDateTime` は Common層から参照
- [ ] `ArgumentNullException` チェックが実装されている
- [ ] 等価性判定は ID ベースのみ
- [ ] `IReadOnlyList` で読み取り専用化
- [ ] `abstract class` vs `interface` の使い分けが正しい
- [ ] AggregateId は Empty チェック実装
- [ ] イベント内に EventId（毎回新規）と AggregateRootId を含める

### ビルド確認

```bash
dotnet build src/SharedKernel/
# エラーなし ✓
# 警告なし ✓
```

### 参照確認

```bash
# SharedKernel は他への依存がないこと
grep -r "using SupportAdvance" src/SharedKernel/
# Common へのみ依存 ✓
```

---

## 🔗 関連ドキュメント

- [Entity_And_DomainEvents_技術仕様.md](Entity_And_DomainEvents_技術仕様.md) — 使用方法（開発者向け）
- [Entity_And_DomainEvents_単体テスト仕様.md](Entity_And_DomainEvents_単体テスト仕様.md) — テスト仕様
- [Entity_設計ガイドライン.md](../../Assistance/Guides/Entity_設計ガイドライン.md) — Entity<TId> パターンの詳細
- [CLAUDE.md - 依存関係ルール](../../../CLAUDE.md)

---

## 📝 更新履歴

| 日付 | 更新内容 |
|------|---------|
| 2026-08-07 | 全面改版。AggregateId 基底クラスを追加。イベント内に EventId（毎回新規）と AggregateRootId（集約固有ID）を含める実装に統一。RowId をテーブル物理キーに限定 |

