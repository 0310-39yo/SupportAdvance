# IClock 依存性注入設計ドキュメント

**バージョン:** 1.0  
**作成日:** 2025年07月  

---

## 1. 概要

SupportAdvance プロジェクトでは、**唯一の時間源として IClock インターフェース**を使用します。

すべての日時取得（CreatedAt, UpdatedAt, DeletedAt）は、IClock を経由して LocalDateTime で行われます。

**目的:**
- テスト時の時刻制御（固定時刻でテスト可能）
- ビジネスロジック層での DateTime.UtcNow / DateTime.Now の直接使用を禁止
- システム全体の時刻を唯一の源から管理

---

## 2. IClock インターフェース定義

```csharp
public interface IClock
{
    /// <summary>
    /// 現在の日本標準時（JST）を LocalDateTime で返します。
    /// </summary>
    LocalDateTime JstNow { get; }
}
```

**特性:**
- `LocalDateTime JstNow` プロパティのみ公開
- タイムゾーン: 日本標準時（JST、UTC+9）
- 戻り値: LocalDateTime（タイムゾーン情報なし）

---

## 3. 実装クラス

### 3.1 SystemClock（本番環境）

```csharp
public sealed class SystemClock : IClock
{
    public LocalDateTime JstNow
    {
        get
        {
            var utcNow = DateTime.UtcNow;
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            var jstNow = TimeZoneInfo.ConvertTime(utcNow, timeZone);
            return new LocalDateTime(jstNow);
        }
    }
}
```

**用途:**
- 本番環境での実運用時に使用
- 毎回呼び出し時に現在の JST を計算

### 3.2 MockClock（テスト環境）

```csharp
public sealed class MockClock : IClock
{
    private readonly DateTime _fixedTime;
    
    public MockClock(DateTime fixedTime)
    {
        _fixedTime = fixedTime;
    }
    
    public LocalDateTime JstNow => new(_fixedTime);
}
```

**用途:**
- ユニットテスト・統合テストでの固定時刻テスト
- `new MockClock(new DateTime(2025, 1, 15, 10, 30, 0))` で時刻指定

---

## 4. DI コンテナでの登録

### 4.1 本番環境での登録

```csharp
// Program.cs

var services = new ServiceCollection();

// ✅ 正しい: IClock インターフェース型で登録
var clockInstance = new SystemClock();
services.AddSingleton<IClock>(clockInstance);

// ❌ 間違い: 具象型で登録（以下は避ける）
// services.AddSingleton(clockInstance);  // ← 型が SystemClock になる
// services.AddSingleton(typeof(SystemClock), clockInstance);
```

**重要ポイント:**
- **`AddSingleton<IClock>(instance)`** でインターフェース型を明示
- 具象型（SystemClock）ではなく **IClock で登録**
- プロジェクト全体で同一インスタンスを共有（シングルトン）

### 4.2 テスト環境での登録

```csharp
// Unit Test

[Fact]
public void TestWithFixedTime()
{
    // テスト用の固定時刻を持つ MockClock を生成
    var mockClock = new MockClock(
        new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified)
    );
    
    // DI コンテナにテスト用クロックを登録
    var services = new ServiceCollection();
    services.AddSingleton<IClock>(mockClock);
    
    var serviceProvider = services.BuildServiceProvider();
    var createdAt = serviceProvider.GetRequiredService<IClock>();
    
    // ✅ 固定時刻でテスト可能
    Assert.Equal(new DateTime(2025, 1, 15, 10, 30, 0), createdAt.JstNow.Value);
}
```

---

## 5. Entity での IClock の使用

### 5.1 コンストラクタでの依存性注入

```csharp
public class Entity
{
    private readonly IClock _clock;  // ← DI で注入
    
    public CreatedAt CreatedAt { get; }
    public UpdatedAt UpdatedAt { get; private set; }
    public DeletedAt DeletedAt { get; private set; }
    
    // ✅ IClock を DI で受け取る
    public Entity(IClock clock)
    {
        _clock = clock;
        
        // IClock.JstNow を使用して ValueObject を生成
        CreatedAt = CreatedAt.From(_clock.JstNow);
        UpdatedAt = UpdatedAt.Unset();
        DeletedAt = DeletedAt.Unset();
    }
    
    public void Update(string name)
    {
        Name = name;
        // 更新時も IClock.JstNow を使用
        UpdatedAt = UpdatedAt.From(_clock.JstNow);
    }
    
    public void SoftDelete()
    {
        if (!DeletedAt.IsDeleted)
        {
            DeletedAt = DeletedAt.From(_clock.JstNow);
        }
    }
}
```

---

## 6. Application Layer での使用

```csharp
public class UpdateEntityUseCase
{
    private readonly IClock _clock;
    private readonly IEntityRepository _repository;
    
    public UpdateEntityUseCase(IClock clock, IEntityRepository repository)
    {
        _clock = clock;
        _repository = repository;
    }
    
    public async Task ExecuteAsync(long entityId, string newName)
    {
        var entity = await _repository.GetByIdAsync(entityId);
        
        // Entity内で IClock を使用
        entity.Update(newName);
        
        await _repository.SaveAsync(entity);
    }
}
```

**注意:**
- Application Layer では直接 IClock.JstNow を使用しない
- Entity のメソッド内で IClock を使用させる
- 責任分離: Entity 内のビジネスロジックで日時を扱う

---

## 7. 禁止パターン

### 7.1 DateTime.UtcNow / DateTime.Now の直接使用

```csharp
// ❌ 禁止
public class Entity
{
    public void Update(string name)
    {
        Name = name;
        UpdatedAt = UpdatedAt.From(new LocalDateTime(DateTime.UtcNow));  // ← 直接
    }
}

// ✅ 正しい
public class Entity
{
    private readonly IClock _clock;
    
    public void Update(string name)
    {
        Name = name;
        UpdatedAt = UpdatedAt.From(_clock.JstNow);  // ← IClock 経由
    }
}
```

### 7.2 具象型（SystemClock）への依存

```csharp
// ❌ 禁止
public class Entity
{
    private readonly SystemClock _clock;  // ← 具象型に依存
    
    public Entity(SystemClock clock)
    {
        _clock = clock;
    }
}

// ✅ 正しい
public class Entity
{
    private readonly IClock _clock;  // ← インターフェースに依存
    
    public Entity(IClock clock)
    {
        _clock = clock;
    }
}
```

---

## 8. テスト戦略

### 8.1 ユニットテスト with MockClock

```csharp
[Fact]
public void TestCreatedAtIsSetOnConstruction()
{
    var mockClock = new MockClock(
        new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified)
    );
    
    var entity = new Entity(mockClock);
    
    // ✅ CreatedAt が設定されていることを確認
    Assert.True(entity.CreatedAt.IsSet);
    Assert.Equal(new DateTime(2025, 1, 15, 10, 30, 0), entity.CreatedAt.Value);
}

[Fact]
public void TestUpdatedAtChangesOnUpdate()
{
    var initialTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);
    var updateTime = new DateTime(2025, 1, 15, 11, 45, 0, DateTimeKind.Unspecified);
    
    var mockClock = new MockClock(initialTime);
    var entity = new Entity(mockClock);
    
    // ✅ 初期状態: Unset
    Assert.False(entity.UpdatedAt.IsSet);
    
    // Update: 時刻を進める
    mockClock = new MockClock(updateTime);
    entity.Update("new name");
    
    // ✅ 更新後: 値がある
    Assert.True(entity.UpdatedAt.IsSet);
    Assert.Equal(new DateTime(2025, 1, 15, 11, 45, 0), entity.UpdatedAt.Value);
}
```

### 8.2 統合テスト

```csharp
[Fact]
public async Task TestEntityPersistenceWithTimestamp()
{
    var mockClock = new MockClock(
        new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified)
    );
    
    // テスト用 DI コンテナ
    var services = new ServiceCollection();
    services.AddSingleton<IClock>(mockClock);
    services.AddSingleton<IEntityRepository, InMemoryEntityRepository>();
    
    var serviceProvider = services.BuildServiceProvider();
    
    var repository = serviceProvider.GetRequiredService<IEntityRepository>();
    var clock = serviceProvider.GetRequiredService<IClock>();
    
    // Entity 生成・保存
    var entity = new Entity(clock);
    await repository.SaveAsync(entity);
    
    // 読み込み
    var loaded = await repository.GetByIdAsync(entity.Id);
    
    // ✅ タイムスタンプが正しく保持されていることを確認
    Assert.Equal(entity.CreatedAt.Value, loaded.CreatedAt.Value);
}
```

---

## 9. 実装チェックリスト

### DI コンテナ設定

- [ ] `services.AddSingleton<IClock>(instance)` でインターフェース型で登録
- [ ] 具象型（SystemClock）で登録していない
- [ ] テスト時に MockClock に上書き可能

### Entity/UseCase 実装

- [ ] Entity コンストラクタで IClock を DI 受け取り
- [ ] private readonly IClock _clock; でフィールド保持
- [ ] DateTime.UtcNow / DateTime.Now を直接使用していない
- [ ] すべての日時取得が IClock.JstNow 経由

### テストコード

- [ ] MockClock で固定時刻をテスト
- [ ] time.Add/Subtract で時刻遷移をシミュレート
- [ ] Unset() 状態と値がある状態を区別してテスト

---

## 10. トラブルシューティング

### 10.1 テストで時刻が固定されない

**問題:**
```csharp
// ❌ Entity に DI されたシングルトン IClock がテスト専用に上書きされていない
var entity = new Entity(new SystemClock());  // ← 毎回本番用クロック
```

**解決策:**
```csharp
// ✅ テスト用 DI コンテナでの登録
var mockClock = new MockClock(fixedTime);
var services = new ServiceCollection();
services.AddSingleton<IClock>(mockClock);

// または
var entity = new Entity(mockClock);  // 直接テスト用クロックを渡す
```

### 10.2 "Could not find IClock in service provider"

**問題:**
```csharp
var services = new ServiceCollection();
// services.AddSingleton<IClock>(...) が設定されていない
var sp = services.BuildServiceProvider();
var clock = sp.GetRequiredService<IClock>();  // ← 例外発生
```

**解決策:**
```csharp
var services = new ServiceCollection();
services.AddSingleton<IClock>(new SystemClock());  // ← 登録忘れ修正
var sp = services.BuildServiceProvider();
var clock = sp.GetRequiredService<IClock>();  // ← OK
```

---

## 11. まとめ

| 項目 | 要件 |
|---|---|
| **インターフェース** | IClock（LocalDateTime JstNow プロパティのみ） |
| **本番実装** | SystemClock |
| **テスト実装** | MockClock |
| **DI 登録** | `AddSingleton<IClock>(instance)` |
| **使用方法** | Entity / UseCase のコンストラクタで DI |
| **禁止** | DateTime.UtcNow / DateTime.Now の直接使用 |
| **テスト** | MockClock で固定時刻をテスト |

**原則:**
- **唯一の時間源**: システム全体で IClock.JstNow を使用
- **テスト容易性**: MockClock で任意の時刻をシミュレート
- **責任分離**: Entity 内ビジネスロジックで日時を扱う
