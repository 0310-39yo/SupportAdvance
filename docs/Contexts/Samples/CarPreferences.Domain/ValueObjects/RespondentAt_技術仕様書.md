# RespondentAt 技術仕様書

**バージョン:** 1.0  
**作成日:** 2026-07-08  
**責務:** アンケート回答者の回答日時を管理する値オブジェクト  

---

## 1. 概要

### 1.1 クラス説明

`RespondentAt` は、アンケート回答者の回答日時を表す**値オブジェクト**です。ValueObject の本質である不変性により、スレッドセーフな設計を実現しています。特に Clock に依存した検証により、現在時刻を基準としたビジネスロジック検証を実施します。

- **継承**: `PrimitiveValueObject<DateTime>`
- **シール**: `sealed class`（拡張不可）
- **実装**: `IOptionalValidateWithClock<RespondentAt, DateTime>`、`IEquatable<RespondentAt>`
- **用途**: ドメインモデルのエンティティにおいて、回答日時を型安全に表現する
- **特性**: ValueObject の不変性 → 値による等価性、依存性の低い設計、テスト容易性の向上

### 1.2 責務

| 責務 | 説明 |
|---|---|
| **DateTime 値の安全な保管** | DateTime 値をラップし、ビジネスルール検証を自動実行 |
| **未設定状態の表現** | null ではなく型で未設定状態（Unset）を表現（Option パターン） |
| **等価性判定** | 日時の内容が同一なら等価（値オブジェクト） |
| **ハッシング対応** | HashMap や HashSet への格納に対応 |
| **時刻ベースの検証** | Clock インターフェースを使用した未来日チェック |

### 1.3 制約・ビジネスルール

| ルール | 詳細 |
|---|---|
| **基本検証** | `DateTime.MinValue`、`DateTime.MaxValue` は不許可（無効な時刻表現） |
| **未来日禁止** | 回答日時は常に過去であるべき（現在時刻より後は不許可） |
| **Clock 依存性** | ビジネスロジック検証は Clock インターフェース経由で現在時刻を取得 |
| **未設定状態の共有** | `Unset()` で生成した複数のインスタンスは等価（値が等しい） |
| **イミュータビリティ** | 一度生成されたインスタンスは変更不可（ValueObject） |

---

## 2. プロパティ & メソッド

### 2.1 プロパティ

#### `IsSet : bool { get; }` (PrimitiveValueObject から継承)

**役割:** 値が設定されているかを判定

```csharp
var respondentAt = RespondentAt.From(DateTime.Now, clock);
Assert.True(respondentAt.IsSet);  // true

var unset = RespondentAt.Unset();
Assert.False(unset.IsSet);  // false
```

#### `Value : DateTime { get; }`

**役割:** 保持する DateTime 値を読み取り専用で取得

```csharp
var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);
DateTime dt = respondentAt.Value;  // 2026-07-08 10:30:00

var unset = RespondentAt.Unset();
DateTime value = unset.Value;  // DateTime.MinValue（注意）
```

**特性:**
- IsSet=true なら値、false なら DateTime.MinValue を返す
- Domain ロジック内で IsSet が既知の場合に使用
- イミュータビリティのため get のみ

**Value プロパティと TryGetValue() の使い分け:**

| 用途 | メソッド | 利用場面 |
|------|---------|--------|
| **直接アクセス** | `Value` プロパティ | Domain ロジック内（IsSet 既知） |
| **安全なアクセス** | `TryGetValue()` | 外部入力・レイヤ境界での値取得 |

```csharp
// Value プロパティ（Domain ロジック）
if (respondentAt.IsSet)
{
    DateTime dt = respondentAt.Value;
    ProcessResponseTime(dt);
}

// TryGetValue（外部入力処理）
if (respondentAt.TryGetValue(out var dt))
{
    ProcessResponseTime(dt);
}
```

### 2.2 ファクトリメソッド

#### `Unset() : RespondentAt`

**役割:** 未設定状態の RespondentAt インスタンスを生成

```csharp
var unset = RespondentAt.Unset();
Assert.False(unset.IsSet);
Assert.False(unset.TryGetValue(out _));
```

**特性:**
- `IsSet = false` なインスタンスを返す
- Unset() 複数呼び出しの戻り値は等価

---

#### `From(DateTime value, IClock clock) : RespondentAt`

**役割:** DateTime 値から設定済み RespondentAt を生成（例外ベース）

```csharp
var dt = new DateTime(2026, 7, 8, 10, 30, 0);
var respondentAt = RespondentAt.From(dt, clock);  // 値検証実施
Assert.True(respondentAt.IsSet);
Assert.Equal(dt, respondentAt.Value);
```

**例外:**
- `ArgumentException`: DateTime.MinValue / DateTime.MaxValue が渡された場合
- `ArgumentException`: 未来日（現在時刻より後）が渡された場合

**【重要】検証の二段階実施:**

| 段階 | メソッド | 検証内容 | 例外 |
|------|---------|--------|------|
| 1. 基本検証 | `Validate()` | MinValue/MaxValue 除外 | ArgumentException |
| 2. ビジネス検証 | `ValidateWithClock()` | 未来日チェック | ArgumentException |

```csharp
// 基本検証に失敗
RespondentAt.From(DateTime.MinValue, clock);  // → ArgumentException

// ビジネス検証に失敗
var futureDate = DateTime.UtcNow.AddHours(1);
RespondentAt.From(futureDate, clock);  // → ArgumentException（未来日）
```

---

#### `TryFrom(DateTime? input, IClock clock, out RespondentAt result) : bool`

**役割:** null 入力を吸収する安全なファクトリメソッド（Try パターン）

```csharp
// null 入力 → Unset を返す
bool success = RespondentAt.TryFrom(null, clock, out var unset);
Assert.True(success);
Assert.False(unset.IsSet);

// 有効な値 → 設定済みインスタンス
var dt = new DateTime(2026, 7, 8, 10, 30, 0);
bool success = RespondentAt.TryFrom(dt, clock, out var instance);
Assert.True(success);
Assert.True(instance.IsSet);
Assert.Equal(dt, instance.Value);

// 無効な値 → Unset を返す（false）
bool success = RespondentAt.TryFrom(DateTime.MaxValue, clock, out var unset);
Assert.False(success);
Assert.False(unset.IsSet);
```

**【重要】IOptionalValueObject TryFrom 契約:**

| 入力 | 検証結果 | 戻り値 | 出力インスタンス |
|------|--------|-------|-----------------|
| null | — | **true** | Unset（IsSet=false） |
| 有効な値 | 成功 | **true** | 設定済み（IsSet=true） |
| 無効な値 | 失敗 | **false** | Unset（IsSet=false） |

**パターン: 入力オプショナル（null許容）**

このメソッドはパターン B（入力オプショナル）を実装：
- null 入力を "入力がない" として解釈
- 成功時も失敗時も Unset を返すが、戻り値で区別
- 外部入力処理に最適（例：API リクエスト、フォーム入力）

---

### 2.3 検証メソッド

#### `Validate(DateTime normalized) : void`

**役割:** 基本検証 — DateTime 形式の妥当性をチェック

```csharp
// MinValue/MaxValue チェック
try
{
    RespondentAt.From(DateTime.MinValue, clock);
}
catch (ArgumentException ex)
{
    Assert.Contains("must be a valid system timestamp", ex.Message);
}
```

**検証内容:**
- `DateTime.MinValue` の除外
- `DateTime.MaxValue` の除外

**【重要】Clock を必要としない理由:**
- DateTime の形式的な有効性のチェックのため、現在時刻は不要
- ビジネスロジック的な検証（未来日チェック）は `ValidateWithClock()` で実施

---

#### `ValidateWithClock(DateTime value, IClock clock) : void`

**役割:** ビジネスロジック検証 — 現在時刻を基準とした妥当性チェック

```csharp
var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));

// 過去日は OK
var pastDate = new DateTime(2026, 7, 8, 10, 0, 0);
RespondentAt.From(pastDate, clock);  // ✓ 成功

// 未来日は例外
var futureDate = new DateTime(2026, 7, 8, 14, 0, 0);
RespondentAt.From(futureDate, clock);  // ✗ ArgumentException
```

**検証内容:**
- `value > clock.JstNow` の場合に例外

**実行タイミング:**
- `From()` メソッド内で自動的に呼び出される
- `TryFrom()` メソッドでは内部的に実行（例外をキャッチして false を返す）

---

### 2.4 値取得メソッド

#### `TryGetValue(out DateTime value) : bool`

**役割:** 設定状態の確認と値取得を同時実施

```csharp
var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);

if (respondentAt.TryGetValue(out var dt))
{
    Console.WriteLine($"回答日時: {dt}");  // 2026-07-08 10:30:00
}

var unset = RespondentAt.Unset();
if (!unset.TryGetValue(out var dt))
{
    Console.WriteLine("回答日時は未設定");
}
```

**戻り値:**
- IsSet=true → true を返し、out パラメータに値を格納
- IsSet=false → false を返し、out パラメータに default(DateTime) を格納

---

### 2.5 等価性判定

#### `Equals(RespondentAt? other) : bool`

**実装:**
```csharp
return IsSet == other.IsSet && (!IsSet || ValueField == other.ValueField);
```

**判定ロジック:**
- IsSet が異なる場合は非等価
- IsSet が同じで true の場合、ValueField が同じかチェック
- IsSet が同じで false の場合は等価

```csharp
var dt1 = new DateTime(2026, 7, 8, 10, 30, 0);
var dt2 = new DateTime(2026, 7, 8, 10, 30, 0);

var respondentAt1 = RespondentAt.From(dt1, clock);
var respondentAt2 = RespondentAt.From(dt2, clock);

Assert.Equal(respondentAt1, respondentAt2);  // true（値が同一）

var unset1 = RespondentAt.Unset();
var unset2 = RespondentAt.Unset();
Assert.Equal(unset1, unset2);  // true（両方未設定）

Assert.NotEqual(respondentAt1, unset1);  // true（IsSet が異なる）
```

---

#### `GetHashCode() : int`

**実装:**
```csharp
if (!IsSet)
{
    return HashCode.Combine(false);
}
return HashCode.Combine(true, ValueField);
```

**整合性:**
- Equals=true のオブジェクトは同一ハッシュ値
- ハッシュ値は複数呼び出しで不変

---

### 2.6 演算子

#### `== / !=`

**実装:** Equals に委譲

```csharp
var respondentAt1 = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);
var respondentAt2 = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);

Assert.True(respondentAt1 == respondentAt2);   // Equals=true
Assert.False(respondentAt1 != respondentAt2);  // Equals=false
```

---

### 2.7 文字列化

#### `ToString() : string`

**実装:**
```csharp
protected override string ToString()
{
    return !IsSet 
        ? "Unset" 
        : ValueField.ToString();
}
```

**例:**
```csharp
var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);
Assert.Equal("2026-07-08T10:30:00", respondentAt.ToString());

var unset = RespondentAt.Unset();
Assert.Equal("Unset", unset.ToString());
```

---

## 3. インターフェース実装

### 3.1 IOptionalValidateWithClock<RespondentAt, DateTime>

```csharp
public interface IOptionalValidateWithClock<TSelf, TValue>
{
    static abstract bool TryFrom(TValue? input, IClock clock, out TSelf result);
    void ValidateWithClock(TValue value, IClock clock);
}
```

**実装内容:**
- `TryFrom()`: null 入力を吸収、例外をキャッチして false を返す
- `ValidateWithClock()`: Clock 依存の検証ロジック

### 3.2 IEquatable<RespondentAt>

```csharp
public interface IEquatable<T>
{
    bool Equals(T? other);
}
```

**実装内容:**
- `Equals()`: 値と IsSet の両方を比較

---

## 4. 依存関係

| 依存 | 用途 |
|-----|------|
| `IClock` | 現在時刻を取得（ビジネスロジック検証用） |
| `PrimitiveValueObject<DateTime>` | ValueObject 基盤 |

---

## 5. 使用例

### 5.1 基本的な利用法

```csharp
var clock = new SystemClock();

// 設定済みインスタンス
var respondentAt = RespondentAt.From(DateTime.Now, clock);
if (respondentAt.TryGetValue(out var dt))
{
    Console.WriteLine($"回答日時: {dt}");
}

// 未設定インスタンス
var unset = RespondentAt.Unset();
Assert.False(unset.TryGetValue(out _));
```

### 5.2 API リクエスト処理

```csharp
public Result ProcessResponse(DateTime? inputDateTime, IClock clock)
{
    if (RespondentAt.TryFrom(inputDateTime, clock, out var respondentAt))
    {
        // 入力オプショナル（null=Unset）か、有効値
        return ProcessWithRespondentAt(respondentAt);
    }
    else
    {
        // 検証失敗（無効な値）
        return Result.Failure("無効な回答日時");
    }
}
```

### 5.3 ドメイン内部での処理

```csharp
public class ResponseAggregate
{
    public required RespondentAt RespondentAt { get; init; }

    public void DisplayResponseTime()
    {
        // Domain 内部では IsSet が既知なので Value を直接使用
        if (RespondentAt.IsSet)
        {
            DateTime dt = RespondentAt.Value;
            Console.WriteLine($"回答日時: {dt}");
        }
    }
}
```

---

## 6. 版管理

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-07-08 | 初版作成（DateTime 型の PrimitiveValueObject、Clock 依存検証） |

