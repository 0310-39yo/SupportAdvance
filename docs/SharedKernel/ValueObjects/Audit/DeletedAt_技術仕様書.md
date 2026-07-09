# DeletedAt 技術仕様書

**バージョン:** 1.0  
**作成日:** 2025年  
**責務:** エンティティの論理削除日時を管理する値オブジェクト

---

## 1. 概要

### 1.1 クラス説明

`DeletedAt` は、エンティティが論理削除された日時を表す**値オブジェクト**です。null許容型の設計により、「削除済み/未削除」の状態を表現します。

- **継承**: `PrimitiveValueObject<DateTime?>` （null許容）
- **シール**: `sealed class` （拡張不可）
- **用途**: ソフト削除（論理削除）により、削除日時を記録
- **特性**: null の場合は「未削除」、値がある場合は「削除済み」

### 1.2 論理削除の基本設計

```
削除状態の判定

IsDeleted = DeletedAt != null
  ├─ true → 削除済み
  └─ false → 未削除
```

**利点:**
- 削除フラグの必要なし（DateTime の有無で判定）
- 削除日時を監査情報として保持
- 削除時刻による履歴管理が可能

### 1.3 責務

| 責務 | 説明 |
|---|---|
| **削除状態の表現** | null/値で削除/未削除を表現 |
| **DateTime の安全な保管** | DateTime 値をラップし、カプセル化 |
| **日時値の検証** | MinValue/MaxValue 等の無効な日時を排除 |
| **等価性判定** | 削除日時が同一のインスタンスを等価と判定 |
| **ハッシング対応** | HashMap や HashSet への格納に対応 |

---

## 2. プロパティ & メソッド

### 2.1 プロパティ

#### `Value : DateTime? { get; }`

**役割:** 保持する削除日時を読み取り専用で取得（null 許容）

```csharp
var deletedAt = DeletedAt.From(new DateTime(2025, 1, 15, 10, 30, 0));
DateTime? dt = deletedAt.Value;  // 2025-01-15T10:30:00 を取得

var notDeleted = DeletedAt.NotDeleted();
DateTime? dt2 = notDeleted.Value;  // null を取得
```

#### `IsSet : bool { get; }` (PrimitiveValueObject から継承)

**役割:** 値が設定されているかを判定

```csharp
var deleted = DeletedAt.From(new DateTime(2025, 1, 15));
Assert.True(deleted.IsSet);  // true

var notDeleted = DeletedAt.NotDeleted();
Assert.False(notDeleted.IsSet);  // false
```

#### `IsDeleted : bool { get; }`

**役割:** 論理削除状態を判定（Value != null と等価）

```csharp
var deleted = DeletedAt.From(new DateTime(2025, 1, 15));
Assert.True(deleted.IsDeleted);  // true

var notDeleted = DeletedAt.NotDeleted();
Assert.False(notDeleted.IsDeleted);  // false
```

### 2.2 ファクトリメソッド

#### `From(LocalDateTime value) : DeletedAt`

**役割:** IClock から取得した LocalDateTime から DeletedAt を生成（削除済みインスタンス）

```csharp
IClock clock = /* DI から注入 */;
var deletedAt = DeletedAt.From(clock.JstNow);
```

#### `From(DateTime value) : DeletedAt` （過去互換性用）

**役割:** DateTime から DeletedAt を生成（非推奨、Entity.SoftDelete では使用しない）

```csharp
// 非推奨: IClock 経由の From(LocalDateTime) を使用
// Entity.SoftDelete() 内では _clock.JstNow を必ず使用すること
var clock = new MockClock(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified));
var deletedAt = DeletedAt.From(clock.JstNow);
```

**例外:**
- `ArgumentException` : 値が DateTime.MinValue または DateTime.MaxValue の場合

#### `NotDeleted() : DeletedAt`

**役割:** 未削除状態の DeletedAt を生成（Value は null）

```csharp
var notDeleted = DeletedAt.NotDeleted();
Assert.False(notDeleted.IsDeleted);
Assert.Null(notDeleted.Value);
```

#### `TryFrom(LocalDateTime? input, out DeletedAt result) : bool`

**役割:** IClock 経由で取得した null安全な生成

```csharp
IClock clock = /* DI から注入 */;
bool success = DeletedAt.TryFrom((LocalDateTime?)clock.JstNow, out var deletedAt);
if (!success)
{
    // 生成失敗（MinValue など）
}
```

**実行フロー:**
1. `input == null` → `result = NotDeleted()` 返却、true を返す
2. `From(input.Value)` 呼び出し → 成功時 true、例外時 false

#### `TryFrom(LocalDateTime input, out DeletedAt result) : bool`

**役割:** non-nullable LocalDateTime の信号用オーバーロード

```csharp
IClock clock = /* DI から注入 */;
bool success = DeletedAt.TryFrom(clock.JstNow, out var deletedAt);
```

### 2.3 等価性メソッド

#### `Equals(object? obj) : bool`

**役割:** オブジェクト等価性の判定

```csharp
IClock clock = /* DI から注入 */;
var a = DeletedAt.From(clock.JstNow);
var b = DeletedAt.From(clock.JstNow);
Assert.Equal(a, b);  // true（同じ削除日時なら等価）

var c = DeletedAt.NotDeleted();
var d = DeletedAt.NotDeleted();
Assert.Equal(c, d);  // true （両方 null なら等価）
```

#### `Equals(DeletedAt? other) : bool`

**役割:** DeletedAt 間の強い型チェック

```csharp
IClock clock = /* DI から注入 */;
var deletedAt1 = DeletedAt.From(clock.JstNow);
var deletedAt2 = DeletedAt.From(clock.JstNow);
if (deletedAt1.Equals(deletedAt2))
{
    // 同一の削除状態
}
```

#### `GetHashCode() : int`

**役割:** ハッシュコード提供、HashMap/HashSet 互換

```csharp
IClock clock = /* DI から注入 */;
var set = new HashSet<DeletedAt>();
set.Add(DeletedAt.From(clock.JstNow));
set.Add(DeletedAt.NotDeleted());
```

### 2.4 文字列化

#### `ToString() : string`

**役割:** 削除日時の ISO 8601 形式文字列を返す、未削除の場合は "(not deleted)"

```csharp
IClock clock = /* DI から注入 */;
var deleted = DeletedAt.From(clock.JstNow);
string str = deleted.ToString();  // "2025-01-15T10:30:00"

var notDeleted = DeletedAt.NotDeleted();
string str2 = notDeleted.ToString();  // "(not deleted)"
```

---

## 3. 検証ルール

### 3.1 Validate メソッド

**検証項目:**
- ✓ `null` は受け入れ（未削除状態を表現）
- ✓ 通常の有効な DateTime は受け入れ
- ✗ `DateTime.MinValue` は除外 (値：0001-01-01T00:00:00)
- ✗ `DateTime.MaxValue` は除外 (値：9999-12-31T23:59:59.9999999)

**例:**

```csharp
IClock clock = /* DI から注入 */;

// OK: 現在の日時で削除（IClock 経由）
var ok1 = DeletedAt.From(clock.JstNow);

// OK: 過去の日時で削除
var clock2 = new MockClock(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
var ok2 = DeletedAt.From(clock2.JstNow);

// OK: 未削除状態
var ok3 = DeletedAt.NotDeleted();

// NG: DateTime.MinValue
var ng1 = Assert.Throws<ArgumentException>(() =>
    DeletedAt.From(new LocalDateTime(DateTime.MinValue)));  // ArgumentException

// NG: DateTime.MaxValue
var ng2 = Assert.Throws<ArgumentException>(() =>
    DeletedAt.From(new LocalDateTime(DateTime.MaxValue)));  // ArgumentException
```

---

## 4. ValueObject としての不変性保証

ValueObject の本質である不変性は以下により確保される：

| 要素 | 実装方法 | 効果 |
|---|---|---|
| **クラス** | `sealed` | 拡張禁止で ValueObject 契約を守る |
| **フィールド** | `readonly` | 外部から変更不可 |
| **プロパティ** | `get-only` | 再代入不可 |
| **コンストラクタ** | `private` | ファクトリメソッドのみで生成 |
| **メソッド** | 純粋メソッド | 状態を変更しない |

**スレッドセーフ性:**
- 不変性により同期化コード不要
- 複数スレッドからの安全なアクセス

---

## 5. コンストラクタ

### 5.1 private コンストラクタ

```csharp
private DeletedAt(DateTime? value, bool isSet) : base(value, isSet)
{
}
```

**設計意図:**
- ファクトリメソッド（From, NotDeleted, TryFrom）のみで生成を許可
- 不正な値の混入を防止

---

## 6. ソフト削除との連携

### 6.1 Entity への組み込み方式

```csharp
public class Entity
{
    private readonly IClock _clock;
    public long Id { get; }
    public DeletedAt DeletedAt { get; private set; }
    
    // Entity 生成時に IClock を注入
    public Entity(IClock clock)
    {
        _clock = clock;
        DeletedAt = DeletedAt.NotDeleted();
    }
    
    // 論理削除メソッド
    public void SoftDelete()
    {
        if (!DeletedAt.IsDeleted)
        {
            DeletedAt = DeletedAt.From(_clock.JstNow);  // ← IClock を使用
        }
    }
    
    // クエリメソッド
    public bool IsDeleted => DeletedAt.IsDeleted;
}
```

### 6.2 Repository での削除状態フィルタリング

```csharp
// 削除済みレコードを除外
var activeEntities = entities
    .Where(e => !e.DeletedAt.IsDeleted)
    .ToList();

// 削除済みレコードのみ取得
var deletedEntities = entities
    .Where(e => e.DeletedAt.IsDeleted)
    .ToList();

// 削除日時で範囲検索
var recentlyDeleted = entities
    .Where(e => e.DeletedAt.IsDeleted && 
               e.DeletedAt.Value >= startDate && 
               e.DeletedAt.Value <= endDate)
    .ToList();
```

---

## 7. 使用例

### 7.1 基本的な使用

```csharp
// Entity 作成時は未削除
IClock clock = /* DI から注入 */;
var entity = new Entity(clock);

Console.WriteLine(entity.IsDeleted);  // false

// 削除操作
entity.SoftDelete();  // DeletedAt = DeletedAt.From(_clock.JstNow)
Console.WriteLine(entity.IsDeleted);  // true
Console.WriteLine(entity.DeletedAt);  // "2025-01-15T10:30:00"
```

### 7.2 null安全な生成

```csharp
DateTime? input = GetDeletedAtFromDatabase();

if (DeletedAt.TryFrom(input, out var deletedAt))
{
    if (deletedAt.IsDeleted)
    {
        Console.WriteLine($"削除日時: {deletedAt.Value}");
    }
}
```

### 7.3 等価性の判定

```csharp
var date1 = DeletedAt.From(new DateTime(2025, 1, 15));
var date2 = DeletedAt.From(new DateTime(2025, 1, 15));

if (date1 == date2)  // true
{
    Console.WriteLine("同一の削除日時");
}

var notDeleted1 = DeletedAt.NotDeleted();
var notDeleted2 = DeletedAt.NotDeleted();

if (notDeleted1 == notDeleted2)  // true
{
    Console.WriteLine("両方未削除");
}
```

---

## 8. 実装上の注意点

| 項目 | 説明 |
|---|---|
| **null の意味** | 削除されていない（削除フラグ不要） |
| **DateTime Kind** | UTC/Local 区別なし。UTC 推奨 |
| **タイムゾーン** | DeletedAt 側では管理しない。利用側で統一を推奨 |
| **精度** | DateTime は 100 ナノ秒単位の精度を保持 |
| **比較** | Equals は値同一性に基づく（参照同一性ではない） |
| **削除取消** | 一度削除されたら取消不可（新規インスタンス生成のみ） |

---

## 9. CreatedAt/UpdatedAt との関係

| ValueObject | 役割 | null許容 | 不変性 |
|---|---|---|---|
| **CreatedAt** | 作成日時 | × | 完全不変 |
| **UpdatedAt** | 更新日時 | × | 可変（Entity が更新） |
| **DeletedAt** | 削除日時 | ✓ | 実質不変（一度削除後は変更なし） |

---

## 10. まとめ

`DeletedAt` は DateTime 値オブジェクトとして、以下を達成します：

✅ **安全性**: DateTime を直接扱わずカプセル化  
✅ **論理削除**: null 許容で削除/未削除を表現  
✅ **検証**: MinValue/MaxValue 排除  
✅ **不変性**: 一度生成後、削除状態は変更不可  
✅ **等価性**: DateTime の値同一性に基づく  
✅ **ハッシング**: HashMap/HashSet 対応  
✅ **監査**: 削除日時を記録して履歴管理
