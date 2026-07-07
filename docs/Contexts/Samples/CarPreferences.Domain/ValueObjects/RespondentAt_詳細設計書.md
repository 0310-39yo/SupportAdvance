# RespondentAt 詳細設計書

**バージョン:** 1.0  
**作成日:** 2026-07-08  
**対象:** `SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects.RespondentAt`  

---

## 1. 設計概要

### 1.1 クラス定義

```csharp
public sealed class RespondentAt : PrimitiveValueObject<DateTime>, 
	IOptionalValidateWithClock<RespondentAt, DateTime>,
	IEquatable<RespondentAt>
```

**特徴:**
- `sealed class` で拡張を禁止（ValueObject パターン遵守）
- `PrimitiveValueObject<DateTime>` で汎用VO基盤を継承
- `IOptionalValidateWithClock<RespondentAt, DateTime>` で Clock 依存検証をサポート
- `IEquatable<RespondentAt>` で型安全な等価性比較をサポート

### 1.2 プロジェクト構成

```
src/
└── Contexts/Samples/CarPreferences.Domain/
	└── ValueObjects/
		└── RespondentAt.cs
```

---

## 2. コンストラクタ設計

### 2.1 プライベートコンストラクタ群

```csharp
private RespondentAt(bool isSet) : base(isSet)
{
}

private RespondentAt(DateTime value, bool isSet) : base(value, true)
{
}
```

**設計理由:**
- **カプセル化:** 直接インスタンス化を防止
- **Factory メソッド経由:** `Unset()` / `From()` / `TryFrom()` の明確な制御フロー
- **基本クラス委譲:** `PrimitiveValueObject` のコンストラクタが自動的に `Validate()` / `ValidateWithClock()` を呼び出す

**実行シーケンス:**

```
RespondentAt(bool isSet) 呼び出し
  ↓
PrimitiveValueObject(bool isSet) の基本コンストラクタ呼び出し
  ├─ IsSet = isSet を設定
  └─ ValueField = default! を設定

---

RespondentAt(DateTime value, bool isSet) 呼び出し
  ↓
PrimitiveValueObject(DateTime value, bool isSet) の基本コンストラクタ呼び出し
  ├─ IsSet = true を設定
  ├─ ValueField = Normalize(value)（DateTime は normalize なし）
  └─ Validate(ValueField) を自動実行 ← MinValue/MaxValue チェック
```

**【注意】:** `ValidateWithClock()` は基本コンストラクタでは呼ばれず、`From()` メソッドで明示的に呼び出される

---

## 2.2 プロパティ設計

### 2.2.1 IsSet プロパティ（PrimitiveValueObject から継承）

```csharp
public bool IsSet { get; protected init; }
```

**役割:** 値が設定されているかを判定

**設計判断:**
- `protected init` で初期化時のみ設定可能
- コンストラクタで `IsSet = true` または `IsSet = false` を設定
- 等価性判定・ハッシュコード計算に含まれる

**使用例:**
```csharp
var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);
Assert.True(respondentAt.IsSet);  // true

var unset = RespondentAt.Unset();
Assert.False(unset.IsSet);  // false
```

### 2.2.2 Value プロパティ

```csharp
public DateTime Value => ValueField;
```

**役割:** 保持する DateTime 値への公開アクセス（get のみ）

**実装詳細:**
- IsSet=true なら ValueField（DateTime 値）を返す
- IsSet=false なら DateTime.MinValue を返す（注意：null でない）
- イミュータビリティのため get のみ（セッター不可）

**設計判断:**
- Domain ロジック内での直接アクセスに使用
- 外部入力処理では TryGetValue() を推奨

**使用例:**
```csharp
var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);
DateTime dt = respondentAt.Value;  // 2026-07-08 10:30:00

var unset = RespondentAt.Unset();
DateTime value = unset.Value;  // DateTime.MinValue（注意）
```

### 2.2.3 Value と TryGetValue() の比較表

| 項目 | Value プロパティ | TryGetValue() メソッド |
|------|:---:|:---:|
| **用途** | Domain ロジック | 外部入力処理 |
| **IsSet 既知** | ✅ 既知 | ❌ 不確定 |
| **返り値型** | DateTime | bool（out DateTime） |
| **例外** | なし | なし |
| **使用場面** | 値確定後のロジック | 入力値検証後の変換 |

**選択基準:**
```csharp
// Value プロパティを使用（Domain ロジック）
if (respondentAt.IsSet)
{
    DateTime dt = respondentAt.Value;
    ProcessResponseTime(dt);
}

// TryGetValue を使用（外部入力処理）
if (respondentAt.TryGetValue(out var dt))
{
    ProcessResponseTime(dt);
}
else
{
    LogUnsetResponseTime();
}
```

---

## 3. メソッド設計

### 3.1 ファクトリメソッド

#### 3.1.1 Unset() : RespondentAt

**実装:**
```csharp
public static RespondentAt Unset() => new(false);
```

**役割:** 未設定状態の RespondentAt インスタンスを生成

**実行フロー:**
```
Unset() 呼び出し
  ↓
new RespondentAt(false) の呼び出し
  ↓
PrimitiveValueObject(bool isSet=false) の基本コンストラクタ
  ├─ IsSet = false
  └─ ValueField = default!（DateTime.MinValue）
```

**使用例:**
```csharp
var unset = RespondentAt.Unset();
Assert.False(unset.IsSet);  // true
```

---

#### 3.1.2 From(DateTime value, IClock clock) : RespondentAt

**実装:**
```csharp
public static RespondentAt From(DateTime value, IClock clock)
{
    var instance = new RespondentAt(value, true);
    instance.ValidateWithClock(value, clock);
    return instance;
}
```

**役割:** DateTime 値から設定済み RespondentAt を生成（例外ベース）

**実行フロー:**
```
From(DateTime value, IClock clock) 呼び出し
  ↓
new RespondentAt(value, true) の呼び出し
  ├─ PrimitiveValueObject(DateTime value, bool isSet=true) の基本コンストラクタ
  │  ├─ IsSet = true
  │  ├─ ValueField = value（DateTime は normalize なし）
  │  └─ Validate(ValueField) ← MinValue/MaxValue チェック
  │
  └─ ValidateWithClock(value, clock) の明示的呼び出し ← 未来日チェック
     └─ 検証失敗時に ArgumentException をスロー
```

**例外処理:**
- `ArgumentException`: DateTime.MinValue / DateTime.MaxValue
- `ArgumentException`: 未来日（現在時刻より後）

```csharp
// 例外シナリオ
try
{
    RespondentAt.From(DateTime.MinValue, clock);
}
catch (ArgumentException ex)
{
    Assert.Contains("must be a valid system timestamp", ex.Message);
}

var futureDate = DateTime.UtcNow.AddHours(1);
try
{
    RespondentAt.From(futureDate, clock);
}
catch (ArgumentException ex)
{
    Assert.Contains("cannot be in the future", ex.Message);
}
```

---

#### 3.1.3 TryFrom(DateTime? input, IClock clock, out RespondentAt result) : bool

**実装:**
```csharp
public static bool TryFrom(DateTime? input, IClock clock, out RespondentAt result)
{
    if (!input.HasValue)
    {
        result = Unset();
        return true;
    }

    try
    {
        result = From(input.Value, clock);
        return true;
    }
    catch (ArgumentException)
    {
        result = Unset();
        return false;
    }
}
```

**役割:** null 入力を吸収する安全なファクトリメソッド

**実行フロー:**
```
TryFrom(DateTime? input, IClock clock, out RespondentAt result) 呼び出し
  ↓
  ├─ if (!input.HasValue) 
  │  ├─ result = Unset()
  │  └─ return true（null 入力を吸収）
  │
  ├─ try {
  │  ├─ result = From(input.Value, clock)
  │  └─ return true（成功）
  │  }
  │
  └─ catch (ArgumentException) {
     ├─ result = Unset()
     └─ return false（検証失敗）
     }
```

**【重要】戻り値マトリックス:**

| 入力 | 検証結果 | 戻り値 | 出力インスタンス | 説明 |
|------|--------|-------|-----------------|-----|
| null | — | **true** | Unset | 入力なし（吸収） |
| 有効な値 | ✓ | **true** | 設定済み | 成功 |
| 無効な値 | ✗ | **false** | Unset | 検証失敗 |

**パターン: 入力オプショナル（null許容）**

このメソッドは **パターン B（入力オプショナル）** を実装：
- null 入力を "入力がない" として解釈
- 成功時と失敗時を戻り値で区別
- 外部入力処理（API リクエスト、フォーム入力）に最適

**使用例:**
```csharp
// null 入力
bool success = RespondentAt.TryFrom(null, clock, out var unset);
Assert.True(success);  // true（吸収）
Assert.False(unset.IsSet);

// 有効な値
var dt = new DateTime(2026, 7, 8, 10, 30, 0);
bool success = RespondentAt.TryFrom(dt, clock, out var instance);
Assert.True(success);  // true
Assert.True(instance.IsSet);

// 無効な値
bool success = RespondentAt.TryFrom(DateTime.MaxValue, clock, out var unset);
Assert.False(success);  // false（検証失敗）
Assert.False(unset.IsSet);
```

---

#### 3.1.4 TryFrom(DateTime input, IClock clock, out RespondentAt result) : bool

**実装:**
```csharp
public static bool TryFrom(DateTime input, IClock clock, out RespondentAt result) 
    => TryFrom((DateTime?)input, clock, out result);
```

**役割:** 非 nullable 版の TryFrom（インターフェース互換性）

**用途:**
- インターフェース実装要件
- nullable でない値の直接的な Try パターン利用

---

### 3.2 検証メソッド

#### 3.2.1 Validate(DateTime normalized) : void

**実装:**
```csharp
public override void Validate(DateTime normalized)
{
    base.Validate(normalized);

    if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
    {
        throw new ArgumentException(
            $"RespondentAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}.");
    }
}
```

**役割:** 基本検証 — DateTime 形式の妥当性をチェック

**検証内容:**
- `DateTime.MinValue` の除外
- `DateTime.MaxValue` の除外

**呼び出しタイミング:**
- `PrimitiveValueObject` の基本コンストラクタから自動実行
- `From()` メソッド内の `new RespondentAt(value, true)` 時点で実行

**例外:**
- `ArgumentException`: MinValue / MaxValue が指定された場合

**設計判断:**

| 検証項目 | 実施場所 | 理由 |
|--------|--------|------|
| MinValue / MaxValue チェック | `Validate()` | DateTime の形式的妥当性 |
| 未来日チェック | `ValidateWithClock()` | 現在時刻が必要（Clock 依存） |

---

#### 3.2.2 ValidateWithClock(DateTime value, IClock clock) : void

**実装:**
```csharp
public void ValidateWithClock(DateTime value, IClock clock)
{
    var nowJst = clock.JstNow.Value;
    if (value > nowJst)
    {
        throw new ArgumentException(
            "RespondentAt cannot be in the future.", nameof(value));
    }
}
```

**役割:** ビジネスロジック検証 — 現在時刻を基準とした妥当性チェック

**検証内容:**
- `value > clock.JstNow` の場合に例外

**呼び出しタイミング:**
- `From()` メソッド内で明示的に呼び出される
- `TryFrom()` メソッド内では try-catch で例外をキャッチして false を返す

**例外:**
- `ArgumentException`: 未来日が指定された場合

**実行例:**
```csharp
var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));

// 過去日は OK
var pastDate = new DateTime(2026, 7, 8, 10, 0, 0);
RespondentAt.From(pastDate, clock);  // ✓ 成功

// 現在日時は OK
var nowDate = new DateTime(2026, 7, 8, 12, 0, 0);
RespondentAt.From(nowDate, clock);  // ✓ 成功

// 未来日は例外
var futureDate = new DateTime(2026, 7, 8, 14, 0, 0);
RespondentAt.From(futureDate, clock);  // ✗ ArgumentException
```

---

### 3.3 値取得メソッド

#### 3.3.1 TryGetValue(out DateTime value) : bool

**実装:**
```csharp
public bool TryGetValue(out DateTime value)
{
    if (IsSet)
    {
        value = ValueField;
        return true;
    }
    else
    {
        value = default;
        return false;
    }
}
```

**役割:** 設定状態の確認と値取得を同時実施

**戻り値:**
- IsSet=true → true を返し、out パラメータに ValueField を格納
- IsSet=false → false を返し、out パラメータに default(DateTime) を格納

**使用例:**
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

---

### 3.4 等価性判定メソッド

#### 3.4.1 Equals(object? obj) : bool

**実装:**
```csharp
public override bool Equals(object? obj) => Equals(obj as RespondentAt);
```

**役割:** System.Object.Equals を型安全版に委譲

---

#### 3.4.2 Equals(RespondentAt? other) : bool

**実装:**
```csharp
public bool Equals(RespondentAt? other)
{
    if (other is null) return false;
    if (ReferenceEquals(this, other)) return true;
    return IsSet == other.IsSet && (!IsSet || ValueField == other.ValueField);
}
```

**判定ロジック:**

```
1. other が null
   → false（他の ValueObject との等価性は不成立）

2. this と other が同一参照
   → true（パフォーマンス最適化）

3. IsSet が異なる
   → false（一方は設定済み、他方は未設定）

4. IsSet が同じで false
   → true（両方未設定）

5. IsSet が同じで true
   → ValueField == other.ValueField で判定
```

**等価性の例:**
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

#### 3.4.3 GetHashCode() : int

**実装:**
```csharp
public override int GetHashCode()
{
    if (!IsSet)
    {
        return HashCode.Combine(false);
    }
    return HashCode.Combine(true, ValueField);
}
```

**ハッシュコード計算:**
- IsSet=false → `HashCode.Combine(false)`
- IsSet=true → `HashCode.Combine(true, ValueField)`

**整合性:**
- Equals=true → 同一ハッシュ値
- 複数呼び出し → 不変

**実行例:**
```csharp
var dt1 = new DateTime(2026, 7, 8, 10, 30, 0);
var dt2 = new DateTime(2026, 7, 8, 10, 30, 0);

var respondentAt1 = RespondentAt.From(dt1, clock);
var respondentAt2 = RespondentAt.From(dt2, clock);

// Equals=true → ハッシュ値が同一
Assert.Equal(respondentAt1.GetHashCode(), respondentAt2.GetHashCode());

var unset1 = RespondentAt.Unset();
var unset2 = RespondentAt.Unset();
Assert.Equal(unset1.GetHashCode(), unset2.GetHashCode());

// IsSet が異なるとハッシュ値が異なる
Assert.NotEqual(respondentAt1.GetHashCode(), unset1.GetHashCode());
```

---

### 3.5 演算子

#### 3.5.1 == / != 演算子

**実装:** Equals に委譲（PrimitiveValueObject で定義）

```csharp
public static bool operator ==(RespondentAt? left, RespondentAt? right)
    => Equals(left, right);

public static bool operator !=(RespondentAt? left, RespondentAt? right)
    => !Equals(left, right);
```

**使用例:**
```csharp
var respondentAt1 = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);
var respondentAt2 = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);

Assert.True(respondentAt1 == respondentAt2);   // Equals=true
Assert.False(respondentAt1 != respondentAt2);  // Equals=false
```

---

### 3.6 文字列化メソッド

#### 3.6.1 ToString() : string

**実装:**
```csharp
protected override string ToString()
{
    if (!IsSet)
    {
        return "Unset";
    }
    return ValueField.ToString();  // DateTime.ToString() に委譲
}
```

**戻り値:**
- IsSet=false → `"Unset"`
- IsSet=true → `ValueField.ToString()`（e.g., `"2026-07-08T10:30:00"`）

**例:**
```csharp
var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);
Assert.Equal("2026-07-08T10:30:00", respondentAt.ToString());

var unset = RespondentAt.Unset();
Assert.Equal("Unset", unset.ToString());
```

---

## 4. GetValueComponents() メソッド

**実装:**
```csharp
protected override IEnumerable<object?> GetValueComponents()
{
    if (IsSet)
    {
        yield return ValueField;
    }
}
```

**役割:** 等価性判定に含まれるコンポーネント（値）を yield return

**使用目的:**
- 派生クラスで等価性判定に含めるコンポーネントを明示
- ValueObject 基盤で Equals・GetHashCode の実装に活用

**実行例:**
```csharp
var respondentAt1 = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);
var respondentAt2 = RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock);

// GetValueComponents() により値が比較される
Assert.Equal(respondentAt1, respondentAt2);
```

---

## 5. インターフェース実装詳細

### 5.1 IOptionalValidateWithClock<RespondentAt, DateTime>

**定義:**
```csharp
public interface IOptionalValidateWithClock<TSelf, TValue>
{
    static abstract bool TryFrom(TValue? input, IClock clock, out TSelf result);
    void ValidateWithClock(TValue value, IClock clock);
}
```

**実装:**

| メンバー | 実装 | 責務 |
|--------|------|------|
| `TryFrom()` | `public static bool TryFrom(DateTime? input, IClock clock, out RespondentAt result)` | null 吸収、例外をキャッチ |
| `ValidateWithClock()` | `public void ValidateWithClock(DateTime value, IClock clock)` | Clock ベース検証 |

### 5.2 IEquatable<RespondentAt>

**定義:**
```csharp
public interface IEquatable<T>
{
    bool Equals(T? other);
}
```

**実装:**

| メンバー | 実装 |
|--------|------|
| `Equals(RespondentAt? other)` | コンポーネント比較（IsSet + ValueField） |

---

## 6. 依存関係図

```
RespondentAt
├── PrimitiveValueObject<DateTime>
│   ├── ValueObject
│   │   └── 基本検証・等価性・ハッシング基盤
│   └── Normalize / Validate / GetValueComponents
├── IOptionalValidateWithClock<RespondentAt, DateTime>
│   ├── TryFrom (static)
│   └── ValidateWithClock (instance)
├── IEquatable<RespondentAt>
│   └── Equals
└── IClock
    └── JstNow（JST の現在時刻取得）
```

---

## 7. 実装上の注意点

### 7.1 Clock 依存性

**理由:**
- ビジネスロジック検証（未来日チェック）には現在時刻が必要
- テスト時に時刻を固定できるよう IClock インターフェース経由で取得

**テスト例:**
```csharp
var mockClock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
var respondentAt = RespondentAt.From(new DateTime(2026, 7, 8, 10, 0, 0), mockClock);
// 現在時刻を 12:00:00 に固定して検証実施
```

### 7.2 DateTime.MinValue/MaxValue の除外理由

**理由:**
- これらの値は有効なシステムタイムスタンプでない
- ビジネスロジック上の意図的な未設定状態（Unset）と混同を避ける

### 7.3 TryFrom の二つのパターン

**パターン A（入力必須）:**
```csharp
bool TryFrom(DateTime input, out RespondentAt result)
```
- 入力必須（null 不許可）
- 戻り値: true（成功）/ false（検証失敗）

**パターン B（入力オプショナル）:**
```csharp
bool TryFrom(DateTime? input, out RespondentAt result)
```
- 入力オプショナル（null 許容）
- 戻り値: true（成功 or null 吸収）/ false（検証失敗）
- RespondentAt が実装するパターン

---

## 8. 使用パターン

### 8.1 API リクエストハンドリング

```csharp
public Result CreateResponse(CreateResponseRequest request, IClock clock)
{
    // API 入力は optional として扱う
    if (!RespondentAt.TryFrom(request.RespondentAtUtc, clock, out var respondentAt))
    {
        return Result.Failure("無効な回答日時");
    }

    // ドメインロジックでは IsSet を確認してから使用
    if (!respondentAt.IsSet)
    {
        // 未設定時の処理
        return Result.Failure("回答日時が未設定です");
    }

    return CreateResponseAggregate(respondentAt);
}
```

### 8.2 ドメイン内部での処理

```csharp
public class ResponseAggregate
{
    public required RespondentAt RespondentAt { get; init; }

    public string DisplaySummary()
    {
        // Domain 内部では IsSet が既知なので Value を直接使用
        if (RespondentAt.IsSet)
        {
            DateTime dt = RespondentAt.Value;
            return $"回答日時: {dt:yyyy-MM-dd HH:mm:ss}";
        }
        return "未回答";
    }
}
```

### 8.3 ハッシング・コレクション利用

```csharp
// HashSet に格納可能
var respondentAtSet = new HashSet<RespondentAt>
{
    RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock),
    RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock),  // 重複は除外
    RespondentAt.Unset()
};
Assert.Equal(2, respondentAtSet.Count);  // 重複がカウントされない

// Dictionary キーとして使用可能
var responseMap = new Dictionary<RespondentAt, string>
{
    { RespondentAt.From(new DateTime(2026, 7, 8, 10, 30, 0), clock), "有効" }
};
```

---

## 9. 版管理

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-07-08 | 初版作成（DateTime 型 PrimitiveValueObject、Clock 依存検証詳細） |

