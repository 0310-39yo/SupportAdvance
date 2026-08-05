# Entity基底クラス＆ドメインイベント - 単体テスト仕様書

**対象者**: テスト実装者

**目的**: Entity と ドメインイベント機構の単体テスト仕様を定義。

---

## 📋 テスト対象

### テスト対象クラス

1. **IDomainEvent** - インターフェース（実装テストは不要）
2. **Entity<TId>** - 基底クラス
3. **AggregateRoot<TId>** - 基底クラス
4. **IDomainEventHandler<TEvent>** - インターフェース（実装テストは不要）

### テスト対象メソッド・プロパティ

| クラス | メンバー | テストケース数 |
|---|---|---|
| Entity<TId> | RaiseDomainEvent() | 2 |
| Entity<TId> | DomainEvents | 3 |
| Entity<TId> | ClearDomainEvents() | 1 |
| Entity<TId> | Equals(object?) | 3 |
| Entity<TId> | Equals(Entity<TId>?) | 3 |
| Entity<TId> | GetHashCode() | 2 |
| AggregateRoot<TId> | 継承動作 | 2 |
| **計** | | **16テストケース** |

---

## 🧪 テストクラス設計

### テストプロジェクト構成

```
tests/
└── SharedKernel.Tests/
    ├── Entities/
    │   ├── EntityTests.cs           ← Entity<TId> テスト
    │   ├── AggregateRootTests.cs    ← AggregateRoot<TId> テスト
    │   └── Fixtures/
    │       ├── TestEntity.cs        ← テスト用Entity
    │       ├── TestAggregateRoot.cs ← テスト用AggregateRoot
    │       ├── TestDomainEvent.cs   ← テスト用Event
    │       └── TestId.cs            ← テスト用ID ValueObject
```

### テスト用クラス（Fixture）

```csharp
// tests/SharedKernel.Tests/Entities/Fixtures/TestId.cs
public sealed class TestId : IEquatable<TestId>
{
    public string Value { get; }

    public TestId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value is required", nameof(value));
        Value = value;
    }

    public override bool Equals(object? obj) => Equals(obj as TestId);
    public bool Equals(TestId? other) => other != null && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}

// tests/SharedKernel.Tests/Entities/Fixtures/TestEntity.cs
public sealed class TestEntity : Entity<TestId>
{
    public string Name { get; private set; }

    public TestEntity(TestId id, string name = "Test")
    {
        Id = id;
        Name = name;
    }

    public void UpdateName(string newName, IClock clock)
    {
        Name = newName;
        this.RaiseDomainEvent(new TestDomainEvent(DomainEventId.New(), this.Id, newName, clock.JstNow));
    }
}

// tests/SharedKernel.Tests/Entities/Fixtures/TestAggregateRoot.cs
public sealed class TestAggregateRoot : AggregateRoot<TestId>
{
    public string Status { get; private set; }

    public TestAggregateRoot(TestId id)
    {
        Id = id;
        Status = "Initial";
    }

    public void ChangeStatus(string newStatus)
    {
        Status = newStatus;
    }
}

// tests/SharedKernel.Tests/Entities/Fixtures/TestDomainEvent.cs
public sealed class TestDomainEvent : IDomainEvent
{
    public DomainEventId EventId { get; }
    public TestId EntityId { get; }
    public string Message { get; }
    public LocalDateTime OccurredAt { get; }

    public TestDomainEvent(DomainEventId eventId, TestId entityId, string message, LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(eventId);

        EventId = eventId;
        EntityId = entityId;
        Message = message;
        OccurredAt = occurredAt;
    }
}
```

---

## 📝 テストケース仕様

### 1. RaiseDomainEvent() メソッドテスト

#### TC-1.1: 正常系 - イベント発行

**テスト名**: `RaiseDomainEvent_WithValidEvent_ShouldAddEventToDomainEvents`

**前提条件**:
- TestEntity インスタンス生成済み

**実行ステップ（Arrange → Act → Assert）**:

```csharp
[Test]
public void RaiseDomainEvent_WithValidEvent_ShouldAddEventToDomainEvents()
{
    // Arrange
    var entityId = new TestId("entity-001");
    var entity = new TestEntity(entityId);
    var clock = new SystemClock();
    var eventMessage = "Test event";
    var @event = new TestDomainEvent(DomainEventId.New(), entityId, eventMessage, clock.JstNow);

    // Act
    entity.UpdateName("NewName", clock);  // RaiseDomainEvent() が呼ばれる

    // Assert
    Assert.That(entity.DomainEvents, Has.Count.EqualTo(1));
    Assert.That(entity.DomainEvents[0], Is.InstanceOf<TestDomainEvent>());
    var actualEvent = (TestDomainEvent)entity.DomainEvents[0];
    Assert.That(actualEvent.Message, Is.EqualTo("NewName"));
}
```

**期待動作**:
- DomainEvents リストにイベントが1件追加される
- イベント型が正しい
- イベントプロパティが期待値と一致

---

#### TC-1.2: 異常系 - null イベント発行

**テスト名**: `RaiseDomainEvent_WithNullEvent_ShouldThrowArgumentNullException`

**前提条件**:
- TestEntity インスタンス生成済み

**実行ステップ**:

```csharp
[Test]
public void RaiseDomainEvent_WithNullEvent_ShouldThrowArgumentNullException()
{
    // Arrange
    var entity = new TestEntity(new TestId("entity-001"));

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() =>
    {
        entity.GetType()
            .GetMethod("RaiseDomainEvent", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(entity, new object?[] { null });
    });
}
```

**期待動作**:
- ArgumentNullException がスローされる

---

### 2. DomainEvents プロパティテスト

#### TC-2.1: 複数イベント発行

**テスト名**: `DomainEvents_WithMultipleRaisedEvents_ShouldContainAllEvents`

**実行ステップ**:

```csharp
[Test]
public void DomainEvents_WithMultipleRaisedEvents_ShouldContainAllEvents()
{
    // Arrange
    var entityId = new TestId("entity-001");
    var entity = new TestEntity(entityId);
    var clock = new SystemClock();

    // Act - 複数回イベント発行
    entity.UpdateName("Name1", clock);
    entity.UpdateName("Name2", clock);
    entity.UpdateName("Name3", clock);

    // Assert
    Assert.That(entity.DomainEvents, Has.Count.EqualTo(3));
    var messages = entity.DomainEvents
        .Cast<TestDomainEvent>()
        .Select(e => e.Message)
        .ToList();
    Assert.That(messages, Is.EqualTo(new[] { "Name1", "Name2", "Name3" }));
}
```

**期待動作**:
- 発行順序を保持して3件のイベントが存在
- イベントのメッセージが期待値と一致

---

#### TC-2.2: DomainEvents は読み取り専用

**テスト名**: `DomainEvents_IsReadOnlyList_CannotModify`

**実行ステップ**:

```csharp
[Test]
public void DomainEvents_IsReadOnlyList_CannotModify()
{
    // Arrange
    var entity = new TestEntity(new TestId("entity-001"));
    var clock = new SystemClock();
    entity.UpdateName("Name", clock);

    // Act & Assert
    var domainEvents = entity.DomainEvents;
    Assert.Throws<NotSupportedException>(() =>
    {
        domainEvents.Add(new TestDomainEvent(new TestId("id"), "msg", clock.JstNow));
    });
}
```

**期待動作**:
- NotSupportedException がスローされる（読み取り専用）

---

#### TC-2.3: イベント発行なし時は空リスト

**テスト名**: `DomainEvents_WithoutRaisingEvents_ShouldReturnEmptyList`

**実行ステップ**:

```csharp
[Test]
public void DomainEvents_WithoutRaisingEvents_ShouldReturnEmptyList()
{
    // Arrange
    var entity = new TestEntity(new TestId("entity-001"));

    // Act & Assert
    Assert.That(entity.DomainEvents, Is.Empty);
    Assert.That(entity.DomainEvents.Count, Is.EqualTo(0));
}
```

**期待動作**:
- イベント未発行時は空の ReadOnlyList を返す

---

### 3. ClearDomainEvents() メソッドテスト

#### TC-3.1: イベントリストクリア

**テスト名**: `ClearDomainEvents_AfterEvents_ShouldClearAll`

**実行ステップ**:

```csharp
[Test]
public void ClearDomainEvents_AfterEvents_ShouldClearAll()
{
    // Arrange
    var entity = new TestEntity(new TestId("entity-001"));
    var clock = new SystemClock();
    entity.UpdateName("Name1", clock);
    entity.UpdateName("Name2", clock);
    Assert.That(entity.DomainEvents, Has.Count.EqualTo(2));

    // Act
    entity.GetType()
        .GetMethod("ClearDomainEvents", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.Invoke(entity, Array.Empty<object>());

    // Assert
    Assert.That(entity.DomainEvents, Is.Empty);
}
```

**期待動作**:
- ClearDomainEvents() 実行後、DomainEvents が空になる

---

### 4. Equals() メソッドテスト

#### TC-4.1: 同じ ID を持つ Entity は等価

**テスト名**: `Equals_WithSameId_ShouldReturnTrue`

**実行ステップ**:

```csharp
[Test]
public void Equals_WithSameId_ShouldReturnTrue()
{
    // Arrange
    var testId = new TestId("entity-001");
    var entity1 = new TestEntity(testId, "Name1");
    var entity2 = new TestEntity(testId, "Name2");  // 同じ ID、異なる Name

    // Act & Assert
    Assert.That(entity1.Equals(entity2), Is.True);
    Assert.That(entity2.Equals(entity1), Is.True);
}
```

**期待動作**:
- 同じ ID なら他のプロパティが異なっても等価

---

#### TC-4.2: 異なる ID を持つ Entity は非等価

**テスト名**: `Equals_WithDifferentId_ShouldReturnFalse`

**実行ステップ**:

```csharp
[Test]
public void Equals_WithDifferentId_ShouldReturnFalse()
{
    // Arrange
    var entity1 = new TestEntity(new TestId("entity-001"));
    var entity2 = new TestEntity(new TestId("entity-002"));

    // Act & Assert
    Assert.That(entity1.Equals(entity2), Is.False);
    Assert.That(entity2.Equals(entity1), Is.False);
}
```

**期待動作**:
- 異なる ID なら非等価

---

#### TC-4.3: null との比較

**テスト名**: `Equals_WithNull_ShouldReturnFalse`

**実行ステップ**:

```csharp
[Test]
public void Equals_WithNull_ShouldReturnFalse()
{
    // Arrange
    var entity = new TestEntity(new TestId("entity-001"));

    // Act & Assert
    Assert.That(entity.Equals(null), Is.False);
    Assert.That(entity.Equals((Entity<TestId>?)null), Is.False);
}
```

**期待動作**:
- null との比較は false を返す

---

#### TC-4.4: 参照が同じ場合

**テスト名**: `Equals_WithSameReference_ShouldReturnTrue`

**実行ステップ**:

```csharp
[Test]
public void Equals_WithSameReference_ShouldReturnTrue()
{
    // Arrange
    var entity = new TestEntity(new TestId("entity-001"));

    // Act & Assert
    Assert.That(entity.Equals(entity), Is.True);
}
```

**期待動作**:
- 参照が同じなら true を返す

---

### 5. GetHashCode() メソッドテスト

#### TC-5.1: 同じ ID なら同じハッシュコード

**テスト名**: `GetHashCode_WithSameId_ShouldReturnSameHashCode`

**実行ステップ**:

```csharp
[Test]
public void GetHashCode_WithSameId_ShouldReturnSameHashCode()
{
    // Arrange
    var testId = new TestId("entity-001");
    var entity1 = new TestEntity(testId);
    var entity2 = new TestEntity(testId);

    // Act
    var hash1 = entity1.GetHashCode();
    var hash2 = entity2.GetHashCode();

    // Assert
    Assert.That(hash1, Is.EqualTo(hash2));
}
```

**期待動作**:
- 同じ ID なら同じハッシュコード

---

#### TC-5.2: HashSet で同じ ID を持つ Entity は重複排除

**テスト名**: `GetHashCode_InHashSet_ShouldDeduplicate`

**実行ステップ**:

```csharp
[Test]
public void GetHashCode_InHashSet_ShouldDeduplicate()
{
    // Arrange
    var testId = new TestId("entity-001");
    var entity1 = new TestEntity(testId);
    var entity2 = new TestEntity(testId);
    var hashSet = new HashSet<Entity<TestId>>();

    // Act
    hashSet.Add(entity1);
    hashSet.Add(entity2);

    // Assert
    Assert.That(hashSet, Has.Count.EqualTo(1));  // 同じ ID なので1件に重複排除
}
```

**期待動作**:
- HashSet で同じ ID を持つ Entity は重複排除される

---

### 6. AggregateRoot<TId> テスト

#### TC-6.1: AggregateRoot は Entity と同じ機能を持つ

**テスト名**: `AggregateRoot_InheritFromEntity_ShouldHaveSameFunctionality`

**実行ステップ**:

```csharp
[Test]
public void AggregateRoot_InheritFromEntity_ShouldHaveSameFunctionality()
{
    // Arrange
    var id = new TestId("root-001");
    var root = new TestAggregateRoot(id);

    // Act & Assert
    Assert.That(root.Id, Is.EqualTo(id));
    Assert.That(root.DomainEvents, Is.Empty);
}
```

**期待動作**:
- AggregateRoot も Entity と同じプロパティを持つ

---

#### TC-6.2: AggregateRoot も等価性判定が ID ベース

**テスト名**: `AggregateRoot_Equals_ShouldUseIdBase`

**実行ステップ**:

```csharp
[Test]
public void AggregateRoot_Equals_ShouldUseIdBase()
{
    // Arrange
    var id = new TestId("root-001");
    var root1 = new TestAggregateRoot(id);
    var root2 = new TestAggregateRoot(id);

    // Act & Assert
    Assert.That(root1.Equals(root2), Is.True);
}
```

**期待動作**:
- AggregateRoot も ID ベースの等価性判定

---

## 🧪 テスト実行コマンド

```bash
# すべてのテストを実行
dotnet test tests/SharedKernel.Tests/

# 特定のテストクラスのみ実行
dotnet test tests/SharedKernel.Tests/ --filter "FullyQualifiedName~EntityTests"

# テスト結果をレポート出力
dotnet test tests/SharedKernel.Tests/ --logger "console;verbosity=detailed"
```

---

## ✅ テスト確認チェックリスト

### テスト実装時

- [ ] 全16テストケースが実装されている
- [ ] すべてのテストが GREEN（Pass）
- [ ] Fixture クラス（TestEntity, TestDomainEvent等）が正しく実装
- [ ] LocalDateTime を使用（DateTime.UtcNow ではない）
- [ ] Arrange-Act-Assert パターンが統一されている

### カバレッジ確認

```bash
# コードカバレッジ測定
dotnet test tests/SharedKernel.Tests/ \
  /p:CollectCoverage=true \
  /p:CoverageFormat=opencover \
  /p:Exclude="[*]*.Tests.*"
```

**目標**: Entity, AggregateRoot クラスで 95% 以上のコードカバレッジ

---

## 📊 テスト結果の見方

### 期待される結果

```
Test Run Successful.
Total tests: 16
     Passed: 16
     Failed: 0
  Skipped: 0
```

### テスト失敗時の対処

| メッセージ | 原因 | 対処 |
|---|---|---|
| `Assert.That(entity.DomainEvents, Has.Count.EqualTo(1))` が失敗 | RaiseDomainEvent() が呼ばれていない | Entity.UpdateName() の実装を確認 |
| `ArgumentNullException` が発生しない | null チェックが実装されていない | Entity.RaiseDomainEvent() に ArgumentNullException チェックを追加 |
| GetHashCode() の値が異なる | ID のハッシュコード実装がおかしい | TestId.GetHashCode() を確認 |

---

## 🔗 関連ドキュメント

- [Entity_And_DomainEvents_詳細設計.md](Entity_And_DomainEvents_詳細設計.md) - 実装仕様
- [Entity_And_DomainEvents_技術仕様.md](Entity_And_DomainEvents_技術仕様.md) - 使用方法

---

## 📝 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-08-01 | 初版作成。16テストケース仕様を定義 |
