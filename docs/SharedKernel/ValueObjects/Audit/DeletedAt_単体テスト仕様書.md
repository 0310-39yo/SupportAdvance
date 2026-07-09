# DeletedAt 単体テスト仕様書

**バージョン:** 1.0  
**作成日:** 2025年  

---

## 1. テスト戦略

### 1.1 テスト分類

```
DeletedAt の単体テスト
  ├── ファクトリメソッドテスト
  │   ├── From(LocalDateTime) メソッド
  │   ├── NotDeleted() メソッド
  │   └── TryFrom(LocalDateTime?) メソッド
  ├── プロパティテスト
  │   ├── Value プロパティ
  │   ├── IsSet プロパティ
  │   └── IsDeleted プロパティ
  ├── 等価性テスト
  │   ├── Equals(object?)
  │   ├── Equals(DeletedAt?)
  │   └── GetHashCode()
  ├── 検証テスト
  │   ├── MinValue/MaxValue 除外
  │   └── 有効な LocalDateTime
  └── 不変性テスト
      ├── 再代入不可
      └── 副作用なし
```

### 1.2 テスト手法

- **ホワイトボックステスト**: 内部実装を意識したテスト
- **境界値テスト**: null, MinValue, MaxValue など境界値
- **等価性テスト**: 値同一性、ハッシング、参照比較
- **不変性テスト**: 生成後の状態変化がないこと
- **MockClock 使用**: IClock 統合テスト

---

## 2. ファクトリメソッドテスト

### 2.1 From(LocalDateTime) メソッドテスト

#### TC-From-001: 有効な LocalDateTime で生成できる（削除済みインスタンス）

**前提条件:**
- テスト対象: `DeletedAt.From(LocalDateTime)`
- 入力: `new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified))`

**テスト手順:**
1. MockClock を生成
2. `DeletedAt.From(clock.JstNow)` を呼び出す
3. 戻り値が DeletedAt インスタンスであることを確認
4. `Value` プロパティが入力値と一致することを確認
5. `IsDeleted` が true であることを確認

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
Assert.NotNull(deletedAt);
Assert.Equal(new DateTime(2025, 1, 15, 10, 30, 0), deletedAt.Value);
Assert.True(deletedAt.IsDeleted);
Assert.True(deletedAt.IsSet);
```

#### TC-From-002: DateTime.MinValue で ArgumentException がスローされる

**期待結果:**
```csharp
var exception = Assert.Throws<ArgumentException>(() =>
    DeletedAt.From(new LocalDateTime(DateTime.MinValue)));
Assert.Contains("MinValue", exception.Message);
```

#### TC-From-003: DateTime.MaxValue で ArgumentException がスローされる

**期待結果:**
```csharp
var exception = Assert.Throws<ArgumentException>(() =>
    DeletedAt.From(new LocalDateTime(DateTime.MaxValue)));
Assert.Contains("MaxValue", exception.Message);
```

#### TC-From-004: 過去の LocalDateTime で生成できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
Assert.Equal(new DateTime(2000, 1, 1), deletedAt.Value);
Assert.True(deletedAt.IsDeleted);
```

#### TC-From-005: 未来の LocalDateTime で生成できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2099, 12, 31, 23, 59, 59, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
Assert.Equal(new DateTime(2099, 12, 31, 23, 59, 59), deletedAt.Value);
Assert.True(deletedAt.IsDeleted);
```

### 2.2 NotDeleted() メソッドテスト

#### TC-NotDeleted-001: 未削除状態を生成できる

**前提条件:**
- テスト対象: `DeletedAt.NotDeleted()`

**テスト手順:**
1. `DeletedAt.NotDeleted()` を呼び出す
2. 戻り値が DeletedAt インスタンスであることを確認
3. `Value` が null であることを確認
4. `IsDeleted` が false であることを確認
5. `IsSet` が false であることを確認

**期待結果:**
```csharp
var notDeleted = DeletedAt.NotDeleted();
Assert.NotNull(notDeleted);
Assert.Null(notDeleted.Value);
Assert.False(notDeleted.IsDeleted);
Assert.False(notDeleted.IsSet);
```

#### TC-NotDeleted-002: 毎回異なるインスタンスが生成される

**期待結果:**
```csharp
var notDeleted1 = DeletedAt.NotDeleted();
var notDeleted2 = DeletedAt.NotDeleted();
Assert.NotSame(notDeleted1, notDeleted2);  // 参照は異なる
Assert.Equal(notDeleted1, notDeleted2);     // 等価性は true
```

### 2.3 TryFrom(LocalDateTime?) メソッドテスト

#### TC-TryFrom-Nullable-001: null 入力で NotDeleted を返す

**期待結果:**
```csharp
bool success = DeletedAt.TryFrom(null as LocalDateTime?, out var result);
Assert.True(success);
Assert.False(result.IsDeleted);
Assert.Null(result.Value);
```

#### TC-TryFrom-Nullable-002: 有効な LocalDateTime で生成できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
bool success = DeletedAt.TryFrom((LocalDateTime?)clock.JstNow, out var result);
Assert.True(success);
Assert.Equal(new DateTime(2025, 1, 15), result.Value);
Assert.True(result.IsDeleted);
```

#### TC-TryFrom-Nullable-003: MinValue で失敗する

**期待結果:**
```csharp
bool success = DeletedAt.TryFrom(new LocalDateTime(DateTime.MinValue), out var result);
Assert.False(success);
```

#### TC-TryFrom-Nullable-004: MaxValue で失敗する

**期待結果:**
```csharp
bool success = DeletedAt.TryFrom(new LocalDateTime(DateTime.MaxValue), out var result);
Assert.False(success);
```

### 2.4 TryFrom(LocalDateTime) メソッド（non-nullable）テスト

#### TC-TryFrom-NonNullable-001: 有効な LocalDateTime で生成できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
bool success = DeletedAt.TryFrom(clock.JstNow, out var result);
Assert.True(success);
Assert.Equal(new DateTime(2025, 1, 15), result.Value);
```

---

## 3. プロパティテスト

### 3.1 Value プロパティテスト

#### TC-Value-001: From で生成した場合、値を取得できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
Assert.Equal(new DateTime(2025, 1, 15), deletedAt.Value);
```

#### TC-Value-002: NotDeleted で生成した場合、null を取得できる

**期待結果:**
```csharp
var notDeleted = DeletedAt.NotDeleted();
Assert.Null(notDeleted.Value);
```

### 3.2 IsDeleted プロパティテスト

#### TC-IsDeleted-001: From で生成した場合、true を返す

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
Assert.True(deletedAt.IsDeleted);
```

#### TC-IsDeleted-002: NotDeleted で生成した場合、false を返す

**期待結果:**
```csharp
var notDeleted = DeletedAt.NotDeleted();
Assert.False(notDeleted.IsDeleted);
```

### 3.3 IsSet プロパティテスト

#### TC-IsSet-001: From で生成した場合、true を返す

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
Assert.True(deletedAt.IsSet);
```

#### TC-IsSet-002: NotDeleted で生成した場合、false を返す

**期待結果:**
```csharp
var notDeleted = DeletedAt.NotDeleted();
Assert.False(notDeleted.IsSet);
```

---

## 4. 等価性テスト

### 4.1 Equals(DeletedAt?) テスト

#### TC-Equals-001: 同一の削除日時なら等価

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt1 = DeletedAt.From(clock.JstNow);
var deletedAt2 = DeletedAt.From(clock.JstNow);
Assert.True(deletedAt1.Equals(deletedAt2));
```

#### TC-Equals-002: 異なる削除日時なら非等価

**期待結果:**
```csharp
var clock1 = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var clock2 = new MockClock(new DateTime(2025, 1, 16, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt1 = DeletedAt.From(clock1.JstNow);
var deletedAt2 = DeletedAt.From(clock2.JstNow);
Assert.False(deletedAt1.Equals(deletedAt2));
```

#### TC-Equals-003: 両方 NotDeleted なら等価

**期待結果:**
```csharp
var notDeleted1 = DeletedAt.NotDeleted();
var notDeleted2 = DeletedAt.NotDeleted();
Assert.True(notDeleted1.Equals(notDeleted2));
```

#### TC-Equals-004: From と NotDeleted なら非等価

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var deleted = DeletedAt.From(clock.JstNow);
var notDeleted = DeletedAt.NotDeleted();
Assert.False(deleted.Equals(notDeleted));
```

#### TC-Equals-005: null との比較は false を返す

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
Assert.False(deletedAt.Equals(null));
```

#### TC-Equals-006: 参照同一なら true を返す

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
Assert.True(deletedAt.Equals(deletedAt));
```

### 4.2 Equals(object?) テスト

#### TC-EqualsObject-001: 同じ型のインスタンスと比較できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
object deletedAt1 = DeletedAt.From(clock.JstNow);
var deletedAt2 = DeletedAt.From(clock.JstNow);
Assert.True(deletedAt1.Equals(deletedAt2));
```

### 4.3 GetHashCode テスト

#### TC-GetHashCode-001: 同一の削除日時のハッシュコードは同じ

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt1 = DeletedAt.From(clock.JstNow);
var deletedAt2 = DeletedAt.From(clock.JstNow);
Assert.Equal(deletedAt1.GetHashCode(), deletedAt2.GetHashCode());
```

#### TC-GetHashCode-002: HashSet に追加できる

**期待結果:**
```csharp
var clock1 = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var clock2 = new MockClock(new DateTime(2025, 1, 16, 0, 0, 0, DateTimeKind.Unspecified));
var set = new HashSet<DeletedAt>
{
    DeletedAt.From(clock1.JstNow),
    DeletedAt.From(clock1.JstNow),  // 重複
    DeletedAt.From(clock2.JstNow),
    DeletedAt.NotDeleted(),
    DeletedAt.NotDeleted()  // 重複
};
Assert.Equal(3, set.Count);  // 3 つのユニークな値
```

---

## 5. 検証テスト

#### TC-Validate-MinValue-001: DateTime.MinValue は除外される

```csharp
var ex = Assert.Throws<ArgumentException>(() =>
    DeletedAt.From(new LocalDateTime(DateTime.MinValue)));
Assert.Contains("MinValue", ex.Message);
```

#### TC-Validate-MaxValue-001: DateTime.MaxValue は除外される

```csharp
var ex = Assert.Throws<ArgumentException>(() =>
    DeletedAt.From(new LocalDateTime(DateTime.MaxValue)));
Assert.Contains("MaxValue", ex.Message);
```

---

## 6. 不変性テスト

#### TC-Immutability-001: Value プロパティへの再代入でコンパイルエラー

```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
// deletedAt.Value = new DateTime(2025, 2, 1);  // ← コンパイルエラー
```

#### TC-Immutability-002: 複数回のプロパティアクセスで結果が変わらない

```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
var value1 = deletedAt.Value;
var value2 = deletedAt.Value;
Assert.Equal(value1, value2);
```

---

## 7. 統合シナリオテスト

### 7.1 Entity のソフト削除フロー

#### TC-Integration-001: Entity の論理削除フロー

```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));

// 1. Entity 初期化（未削除）
var entity = new Entity(clock);
entity.DeletedAt = DeletedAt.NotDeleted();
Assert.False(entity.IsDeleted);

// 2. Entity 削除
entity.DeletedAt = DeletedAt.From(clock.JstNow);
Assert.True(entity.IsDeleted);
Assert.NotNull(entity.DeletedAt.Value);

// 3. 削除状態で再度削除しない（冪等性）
var deletedAt1 = entity.DeletedAt;
entity.DeletedAt = DeletedAt.From(clock.JstNow);  // 再度削除操作
var deletedAt2 = entity.DeletedAt;
Assert.Equal(deletedAt1, deletedAt2);
```

### 7.2 Repository フィルタリングテスト

#### TC-Integration-002: LINQ WHERE での削除状態フィルタリング

```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Unspecified));

var entities = new List<Entity>
{
    new Entity(clock) { Id = 1, DeletedAt = DeletedAt.NotDeleted() },
    new Entity(clock) { Id = 2, DeletedAt = DeletedAt.From(clock.JstNow) },
    new Entity(clock) { Id = 3, DeletedAt = DeletedAt.NotDeleted() }
};

var activeEntities = entities.Where(e => !e.DeletedAt.IsDeleted).ToList();
Assert.Equal(2, activeEntities.Count);

var deletedEntities = entities.Where(e => e.DeletedAt.IsDeleted).ToList();
Assert.Single(deletedEntities);
```

---

## 8. テスト実装上の注意点

| 項目 | 説明 |
|---|---|
| **MockClock 使用** | 常に MockClock を使用、DateTime.UtcNow 直接使用は禁止 |
| **LocalDateTime** | DateTime ではなく LocalDateTime を入力値に使用 |
| **null 処理** | null 安全性を明示的にテスト |
| **TestCase 命名** | TC-[Category]-[Number]: [Description] 形式 |
| **AAA パターン** | Arrange-Act-Assert を厳密に守る |

---

## 9. カバレッジ目標

| 対象 | 目標 | 備考 |
|---|---|---|
| **コード行カバレッジ** | 95% 以上 | private コンストラクタ以外 |
| **ブランチカバレッジ** | 90% 以上 | if/else 分岐 |
| **例外パスカバレッジ** | 100% | ArgumentException など |

---

## 10. まとめ

DeletedAt の単体テスト仕様は以下を網羅します：

✅ **ファクトリメソッド**: From, NotDeleted, TryFrom の正常/異常系  
✅ **プロパティ**: Value, IsDeleted, IsSet の値確認  
✅ **等価性**: Equals, GetHashCode の実装確認（null 含む）  
✅ **検証**: MinValue/MaxValue 除外確認  
✅ **不変性**: 再代入不可、副作用なし  
✅ **統合**: Entity のソフト削除フロー、Repository フィルタリング  
✅ **MockClock**: テスト環境での IClock 統合確認
