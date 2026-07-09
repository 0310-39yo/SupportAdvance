# UpdatedAt 単体テスト仕様書

**バージョン:** 1.0  
**作成日:** 2025年  

---

## 1. テスト戦略

### 1.1 テスト分類

```
UpdatedAt の単体テスト
  ├── ファクトリメソッドテスト
  │   ├── From(LocalDateTime) メソッド
  │   └── TryFrom(LocalDateTime?) メソッド
  ├── プロパティテスト
  │   ├── Value プロパティ
  │   └── IsSet プロパティ
  ├── 等価性テスト
  │   ├── Equals(object?)
  │   ├── Equals(UpdatedAt?)
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
- **境界値テスト**: MinValue, MaxValue など極値
- **等価性テスト**: 値同一性、ハッシング、参照比較
- **不変性テスト**: 生成後の状態変化がないこと
- **MockClock 使用**: IClock 統合テスト

---

## 2. ファクトリメソッドテスト

### 2.1 From(LocalDateTime) メソッドテスト

#### TC-From-001: 有効な LocalDateTime で生成できる

**前提条件:**
- テスト対象: `UpdatedAt.From(LocalDateTime)`
- テスト用クロック: `new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified))`

**テスト手順:**
1. MockClock を生成
2. `UpdatedAt.From(clock.JstNow)` を呼び出す
3. 戻り値が UpdatedAt インスタンスであることを確認
4. `Value` プロパティが入力値と一致することを確認
5. `IsSet` が true であることを確認

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var updatedAt = UpdatedAt.From(clock.JstNow);
Assert.NotNull(updatedAt);
Assert.Equal(new DateTime(2025, 1, 15, 10, 30, 0), updatedAt.Value);
Assert.True(updatedAt.IsSet);
```

#### TC-From-002: DateTime.MinValue で ArgumentException がスローされる

**前提条件:**
- テスト対象: `UpdatedAt.From(LocalDateTime)`
- 入力: LocalDateTime(DateTime.MinValue)

**テスト手順:**
1. `DateTime.MinValue` を含む LocalDateTime を作成
2. `UpdatedAt.From(localDateTime)` を呼び出す
3. ArgumentException がスローされることを確認
4. 例外メッセージに "MinValue" を含む

**期待結果:**
```csharp
var exception = Assert.Throws<ArgumentException>(() =>
    UpdatedAt.From(new LocalDateTime(DateTime.MinValue)));
Assert.Contains("MinValue", exception.Message);
```

#### TC-From-003: DateTime.MaxValue で ArgumentException がスローされる

**前提条件:**
- テスト対象: `UpdatedAt.From(LocalDateTime)`
- 入力: LocalDateTime(DateTime.MaxValue)

**テスト手順:**
1. `DateTime.MaxValue` を含む LocalDateTime を作成
2. `UpdatedAt.From(localDateTime)` を呼び出す
3. ArgumentException がスローされることを確認

**期待結果:**
```csharp
var exception = Assert.Throws<ArgumentException>(() =>
    UpdatedAt.From(new LocalDateTime(DateTime.MaxValue)));
Assert.Contains("MaxValue", exception.Message);
```

#### TC-From-004: 過去の LocalDateTime で生成できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
var updatedAt = UpdatedAt.From(clock.JstNow);
Assert.Equal(new DateTime(2000, 1, 1), updatedAt.Value);
```

#### TC-From-005: 未来の LocalDateTime で生成できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2099, 12, 31, 23, 59, 59, DateTimeKind.Unspecified));
var updatedAt = UpdatedAt.From(clock.JstNow);
Assert.Equal(new DateTime(2099, 12, 31, 23, 59, 59), updatedAt.Value);
```

### 2.2 TryFrom メソッドテスト

#### TC-TryFrom-001: 有効な LocalDateTime で生成できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
bool success = UpdatedAt.TryFrom(clock.JstNow, out var result);
Assert.True(success);
Assert.Equal(new DateTime(2025, 1, 15, 10, 30, 0), result.Value);
```

#### TC-TryFrom-002: MinValue で失敗する

**期待結果:**
```csharp
bool success = UpdatedAt.TryFrom(new LocalDateTime(DateTime.MinValue), out var result);
Assert.False(success);
```

#### TC-TryFrom-003: MaxValue で失敗する

**期待結果:**
```csharp
bool success = UpdatedAt.TryFrom(new LocalDateTime(DateTime.MaxValue), out var result);
Assert.False(success);
```

---

## 3. プロパティテスト

### 3.1 Value プロパティテスト

#### TC-Value-001: 生成後、Value を取得できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var updatedAt = UpdatedAt.From(clock.JstNow);
Assert.Equal(new DateTime(2025, 1, 15, 10, 30, 0), updatedAt.Value);
```

#### TC-Value-002: Value プロパティは読み取り専用

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var updatedAt = UpdatedAt.From(clock.JstNow);
// updatedAt.Value = new DateTime(2025, 2, 1);  // ← コンパイルエラー
```

### 3.2 IsSet プロパティテスト

#### TC-IsSet-001: 常に true を返す

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var updatedAt = UpdatedAt.From(clock.JstNow);
Assert.True(updatedAt.IsSet);
```

---

## 4. 等価性テスト

### 4.1 Equals(UpdatedAt?) テスト

#### TC-Equals-001: 同一の日時なら等価

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var updatedAt1 = UpdatedAt.From(clock.JstNow);
var updatedAt2 = UpdatedAt.From(clock.JstNow);
Assert.True(updatedAt1.Equals(updatedAt2));
```

#### TC-Equals-002: 異なる日時なら非等価

**期待結果:**
```csharp
var clock1 = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var clock2 = new MockClock(new DateTime(2025, 1, 16, 10, 30, 0, DateTimeKind.Unspecified));
var updatedAt1 = UpdatedAt.From(clock1.JstNow);
var updatedAt2 = UpdatedAt.From(clock2.JstNow);
Assert.False(updatedAt1.Equals(updatedAt2));
```

#### TC-Equals-003: null との比較は false を返す

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var updatedAt = UpdatedAt.From(clock.JstNow);
Assert.False(updatedAt.Equals(null));
```

#### TC-Equals-004: 参照同一なら true を返す

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var updatedAt = UpdatedAt.From(clock.JstNow);
Assert.True(updatedAt.Equals(updatedAt));
```

### 4.2 Equals(object?) テスト

#### TC-EqualsObject-001: 同じ型のインスタンスと比較できる

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
object updatedAt1 = UpdatedAt.From(clock.JstNow);
var updatedAt2 = UpdatedAt.From(clock.JstNow);
Assert.True(updatedAt1.Equals(updatedAt2));
```

### 4.3 GetHashCode テスト

#### TC-GetHashCode-001: 同一の日時のハッシュコードは同じ

**期待結果:**
```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var updatedAt1 = UpdatedAt.From(clock.JstNow);
var updatedAt2 = UpdatedAt.From(clock.JstNow);
Assert.Equal(updatedAt1.GetHashCode(), updatedAt2.GetHashCode());
```

#### TC-GetHashCode-002: HashSet に追加できる

**期待結果:**
```csharp
var clock1 = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var clock2 = new MockClock(new DateTime(2025, 1, 16, 10, 30, 0, DateTimeKind.Unspecified));
var set = new HashSet<UpdatedAt>
{
    UpdatedAt.From(clock1.JstNow),
    UpdatedAt.From(clock1.JstNow),  // 重複
    UpdatedAt.From(clock2.JstNow)
};
Assert.Equal(2, set.Count);  // 2 つのユニークな値
```

---

## 5. 検証テスト

#### TC-Validate-MinValue-001: DateTime.MinValue は除外される

```csharp
var ex = Assert.Throws<ArgumentException>(() =>
    UpdatedAt.From(new LocalDateTime(DateTime.MinValue)));
Assert.Contains("MinValue", ex.Message);
```

#### TC-Validate-MaxValue-001: DateTime.MaxValue は除外される

```csharp
var ex = Assert.Throws<ArgumentException>(() =>
    UpdatedAt.From(new LocalDateTime(DateTime.MaxValue)));
Assert.Contains("MaxValue", ex.Message);
```

---

## 6. 不変性テスト

#### TC-Immutability-001: 複数回のプロパティアクセスで結果が変わらない

```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var updatedAt = UpdatedAt.From(clock.JstNow);
var value1 = updatedAt.Value;
var value2 = updatedAt.Value;
Assert.Equal(value1, value2);
```

---

## 7. 統合シナリオテスト

### 7.1 Entity の更新フロー

#### TC-Integration-001: Entity 更新時に UpdatedAt が更新される

```csharp
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var entity = new Entity(clock)
{
    UpdatedAt = UpdatedAt.From(clock.JstNow)
};
var originalUpdatedAt = entity.UpdatedAt;

// 時刻を進める
clock.Advance(TimeSpan.FromSeconds(10));

// Entity を更新
entity.UpdatedAt = UpdatedAt.From(clock.JstNow);

Assert.NotEqual(originalUpdatedAt, entity.UpdatedAt);
Assert.Equal(new DateTime(2025, 1, 15, 10, 30, 10), entity.UpdatedAt.Value);
```

---

## 8. テスト実装上の注意点

| 項目 | 説明 |
|---|---|
| **MockClock 使用** | 常に MockClock を使用、DateTime.UtcNow 直接使用は禁止 |
| **LocalDateTime** | DateTime ではなく LocalDateTime を入力値に使用 |
| **Kind 指定** | MockClock 生成時に DateTimeKind.Unspecified を明示 |
| **テスト命名** | TC-[Category]-[Number]: [Description] 形式 |
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

UpdatedAt の単体テスト仕様は以下を網羅します：

✅ **ファクトリメソッド**: From, TryFrom の正常/異常系  
✅ **プロパティ**: Value, IsSet の値確認  
✅ **等価性**: Equals, GetHashCode の実装確認  
✅ **検証**: MinValue/MaxValue 除外確認  
✅ **不変性**: 再代入不可、副作用なし  
✅ **統合**: Entity 更新フローの実際のシナリオ  
✅ **MockClock**: テスト環境での IClock 統合確認
