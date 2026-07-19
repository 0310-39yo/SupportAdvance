# CreatedAt 技術仕様書

**バージョン:** 1.0  
**作成日:** 2025年  
**責務:** エンティティの作成日時を管理する値オブジェクト

---

## 1. 概要

### 1.1 クラス説明

`CreatedAt` は、エンティティが作成された日時を表す**値オブジェクト**です。ValueObject の本質である不変性により、スレッドセーフな設計を実現しています。

- **継承**: `PrimitiveValueObject<DateTime>`
- **シール**: `sealed class` （拡張不可）
- **用途**: 監査情報として、エンティティの作成日時を記録・参照する
- **特性**: ValueObject の不変性 → 値による等価性、依存性の低い設計、テスト容易性の向上

### 1.2 責務

| 責務 | 説明 |
|---|---|
| **DateTime の安全な保管** | DateTime 値をラップし、カプセル化された形で提供 |
| **日時値の検証** | MinValue/MaxValue 等の無効な日時を排除 |
| **等価性判定** | 作成日時が同一のインスタンスを等価と判定 |
| **ハッシング対応** | HashMap や HashSet への格納に対応 |

---

## 2. プロパティ & メソッド

### 2.1 プロパティ

#### `Value : DateTime { get; }`

**役割:** 保持する日時値を読み取り専用で取得

```csharp
IClock clock = /* DI から注入 */;
var createdAt = CreatedAt.From(clock.JstNow);
DateTime dt = createdAt.Value;  // 2025-01-01T10:30:00 を取得
```

#### `IsSet : bool { get; }` (PrimitiveValueObject から継承)

**役割:** 値が設定されているかを判定（CreatedAt は常に true）

```csharp
IClock clock = /* DI から注入 */;
var createdAt = CreatedAt.From(clock.JstNow);
Assert.True(createdAt.IsSet);  // 常に true
```

### 2.2 ファクトリメソッド

#### `From(LocalDateTime value) : CreatedAt`

**役割:** IClock から取得した LocalDateTime から CreatedAt を生成

```csharp
IClock clock = /* DI から注入 */;
var createdAt = CreatedAt.From(clock.JstNow);
```

**例外:**
- `ArgumentException` : 値が DateTime.MinValue または DateTime.MaxValue の場合

#### `TryFrom(LocalDateTime? input, out CreatedAt result) : bool`

**役割:** IClock 経由で取得した null安全な生成

```csharp
IClock clock = /* DI から注入 */;
var localDateTime = clock.JstNow as LocalDateTime?;
bool success = CreatedAt.TryFrom(localDateTime, out var createdAt);
if (!success)
{
	// 生成失敗
}
```

**実行フロー:**
1. `input.HasValue` が false → false を返す
2. `From(input.Value)` 呼び出し → 成功時 true、例外時 false

### 2.3 等価性メソッド

#### `Equals(object? obj) : bool`

**役割:** オブジェクト等価性の判定

```csharp
IClock clock = /* DI から注入 */;
var a = CreatedAt.From(clock.JstNow);
var b = CreatedAt.From(clock.JstNow);
Assert.Equal(a, b);  // true（同じ時刻なら等価）
```

#### `Equals(CreatedAt? other) : bool`

**役割:** CreatedAt 間の強い型チェック

```csharp
if (createdAt1.Equals(createdAt2))
{
	// 同一の日時を持つ
}
```

#### `GetHashCode() : int`

**役割:** ハッシュコード提供、HashMap/HashSet 互換

```csharp
IClock clock = /* DI から注入 */;
var set = new HashSet<CreatedAt>();
set.Add(CreatedAt.From(clock.JstNow));
```

### 2.4 文字列化

#### `ToString() : string`

**役割:** 日時の ISO 8601 形式文字列を返す

```csharp
IClock clock = /* DI から注入 */;
var createdAt = CreatedAt.From(clock.JstNow);
string str = createdAt.ToString();  // "2025-01-01T10:30:00"
```

---

## 3. 検証ルール

### 3.1 Validate メソッド

**検証項目:**
- ✓ `DateTime.MinValue` は除外 (値：0001-01-01T00:00:00)
- ✓ `DateTime.MaxValue` は除外 (値：9999-12-31T23:59:59.9999999)
- ✗ その他の有効な DateTime は受け入れ（過去・未来・現在・UTC/Local 区別なし）

**例:**

```csharp
IClock clock = /* DI から注入 */;

// OK: 通常の日時（IClock 経由）
var ok1 = CreatedAt.From(clock.JstNow);

// OK: 過去の日時
var clock2 = new MockClock(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
var ok2 = CreatedAt.From(clock2.JstNow);

// NG: DateTime.MinValue
var exception1 = Assert.Throws<ArgumentException>(() =>
    CreatedAt.From(new LocalDateTime(DateTime.MinValue)));  // ArgumentException

// NG: DateTime.MaxValue
var exception2 = Assert.Throws<ArgumentException>(() =>
    CreatedAt.From(new LocalDateTime(DateTime.MaxValue)));  // ArgumentException
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
private CreatedAt(DateTime value) : base(value, true)
{
}
```

**設計意図:**
- ファクトリメソッド（From, TryFrom）のみで生成を許可
- 不正な値の混入を防止

---

## 6. 使用例

### 6.1 基本的な使用（推奨）

```csharp
// IClock から現在のJST日時でCreatedAtを生成
private readonly IClock _clock;  // DI で注入

var createdAt = CreatedAt.From(_clock.JstNow);

// 日時値の参照
DateTime dt = createdAt.Value;  // LocalDateTime の Value は DateTime（Kind = Unspecified）
Console.WriteLine(createdAt);   // ISO 8601 形式で出力
```

### 6.2 Entity での初期化パターン

```csharp
public class Entity
{
    private readonly IClock _clock;
    public CreatedAt CreatedAt { get; }
    
    // Entity 生成時に IClock を注入
    public Entity(IClock clock)
    {
        _clock = clock;
        CreatedAt = CreatedAt.From(_clock.JstNow);  // ← ここで生成
    }
}
```

### 6.3 安全な生成（null安全性）

```csharp
private readonly IClock _clock;

LocalDateTime? input = GetUserInputAsLocalDateTime();

if (CreatedAt.TryFrom(input, out var createdAt))
{
	Console.WriteLine($"作成日時: {createdAt.Value}");
}
else
{
	Console.WriteLine("無効な入力");
}
```

### 6.3 等価性の判定

```csharp
IClock clock = /* DI から注入 */;
var date1 = CreatedAt.From(clock.JstNow);
var date2 = CreatedAt.From(clock.JstNow);

if (date1 == date2)  // Equals メソッド経由
{
	Console.WriteLine("同一の日時");
}

var set = new HashSet<CreatedAt> { date1, date2 };
Console.WriteLine(set.Count);  // 1 （等価なら重複排除）
```

---

## 7. 実装上の注意点

| 項目 | 説明 |
|---|---|
| **DateTime Kind** | UTC/Local 区別なし。任意の Kind を受け入れる |
| **タイムゾーン** | CreatedAt 側では管理しない。利用側で統一を推奨 |
| **精度** | DateTime は 100 ナノ秒単位の精度を保持 |
| **比較** | Equals は値同一性に基づく（参照同一性ではない） |

---

## 8. IEquatable<CreatedAt> 実装

CreatedAt は `IEquatable<CreatedAt>` を実装し、以下の等価性ルールに従います：

- **同一インスタンス**: `ReferenceEquals` で true
- **値同一**: `ValueField`（DateTime）の同一性で判定
- **null との比較**: false を返す

---

## まとめ

`CreatedAt` は DateTime 値オブジェクトとして、以下を達成します：

✅ **安全性**: DateTime を直接扱わずカプセル化  
✅ **検証**: MinValue/MaxValue 排除  
✅ **不変性**: 作成後変更不可  
✅ **等価性**: DateTime の値同一性に基づく  
✅ **ハッシング**: HashMap/HashSet 対応
