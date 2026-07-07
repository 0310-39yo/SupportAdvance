# RespondentName 詳細設計書

**バージョン:** 1.0  
**作成日:** 2026-07-04  
**対象:** `SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects.RespondentName`  

---

## 1. 設計概要

### 1.1 クラス定義

```csharp
public sealed class RespondentName : PrimitiveValueObject<string>, 
	IOptionalValueObject<RespondentName, string>,
	IEquatable<RespondentName>
```

**特徴:**
- `sealed class` で拡張を禁止（ValueObject パターン遵守）
- `PrimitiveValueObject<string>` で汎用VO基盤を継承
- `IOptionalValueObject<RespondentName, string>` で未設定状態をサポート
- `IEquatable<RespondentName>` で型安全な等価性比較をサポート

### 1.2 プロジェクト構成

```
src/
└── Contexts/Samples/CarPreferences.Domain/
	└── ValueObjects/
		└── RespondentName.cs
```

---

## 2. コンストラクタ設計

### 2.1 プライベートコンストラクタ群

```csharp
private RespondentName(bool isSet) : base(isSet)
{
}

private RespondentName(string value, bool isSet) : base(value, isSet)
{
}
```

**設計理由:**
- **カプセル化:** 直接インスタンス化を防止
- **Factory メソッド経由:** `Unset()` / `From()` / `TryFrom()` の明確な制御フロー
- **基本クラス委譲:** `PrimitiveValueObject` のコンストラクタが自動的に `Validate()` / `Normalize()` を実行

**実行シーケンス:**

```
RespondentName(bool isSet) 呼び出し
  ↓
PrimitiveValueObject(bool isSet) の基本コンストラクタ呼び出し
  ├─ IsSet = isSet を設定
  └─ ValueField = default! を設定

---

RespondentName(string value, bool isSet) 呼び出し
  ↓
PrimitiveValueObject(TValue value, bool isSet) の基本コンストラクタ呼び出し
  ├─ IsSet = isSet を設定
  ├─ if (isSet) { ValueField = Normalize(value) }
  └─ if (isSet) { Validate(ValueField) }  ← 自動実行
```

---

## 3. Factory メソッド設計

### 3.1 Unset() メソッド

```csharp
public static RespondentName Unset() => new(false);
```

**実装戦略:** 未設定状態のインスタンスを生成

**呼び出しチェーン:**
```
Unset()
  ↓ new RespondentName(false)
  ↓ RespondentName(bool isSet) コンストラクタ
  ↓ base(isSet) → PrimitiveValueObject(bool isSet)
  ↓ IsSet = false, ValueField = null (default)
```

**使用例:**
```csharp
var unset = RespondentName.Unset();
// unset.IsSet == false
// unset.Value == null (読み取り不可)
// unset.ToString() == "Unset"
```

### 3.2 From() メソッド

```csharp
public static RespondentName From(string value)
{
	ArgumentNullException.ThrowIfNull(value);
	return new RespondentName(value, true);
}
```

**実装戦略:** 
1. null チェック（`ArgumentNullException`）
2. インスタンス生成（`isSet = true`）
3. 基本クラスで自動的に Validate が実行

**例外処理:**
- `ArgumentNullException` : value が null の場合

**呼び出しチェーン:**
```
From(string value)
  ↓ ArgumentNullException.ThrowIfNull(value)
  ↓ new RespondentName(value, true)
  ↓ PrimitiveValueObject(value, true) コンストラクタ
  ├─ IsSet = true
  ├─ ValueField = Normalize(value)  (デフォルトは value そのまま)
  └─ Validate(ValueField)  ← 基本クラスで自動呼び出し
```

**使用例:**
```csharp
var name = RespondentName.From("田中太郎");
// name.IsSet == true
// name.Value == "田中太郎"
// name.ToString() == "田中太郎"

// null を渡すとエラー
RespondentName.From(null);  // ArgumentNullException
```

### 3.3 TryFrom() メソッド

```csharp
public static bool TryFrom(string? input, out RespondentName result)
{
	if (input is null)
	{
		result = Unset();
		return true;  // null は成功（Unset を返す）
	}

	try
	{
		result = From(input);
		return true;
	}
	catch (ArgumentException)
	{
		result = Unset();
		return false;  // 例外は失敗（false を返す）
	}
}
```

**実装戦略:**
- null 入力 → `Unset()` を返して **true**（null 安全）
- 検証失敗時 → `Unset()` を返して **false**（例外を例外として送出しない）

**実行フロー:**

```
TryFrom(string? input, out RespondentName result)
  ↓
if (input is null)
  ├─ Yes: result = Unset(), return true
  └─ No: 次へ

try
  ↓ result = From(input)
  ├─ 成功: return true
  └─ 失敗（引数例外）

catch (ArgumentException)
  ├─ result = Unset()
  └─ return false
```

**使用例:**
```csharp
// 成功例
bool ok1 = RespondentName.TryFrom("田中太郎", out var name1);
// ok1 == true, name1.IsSet == true, name1.Value == "田中太郎"

// null 入力（成功として扱う）
bool ok2 = RespondentName.TryFrom(null, out var name2);
// ok2 == true, name2.IsSet == false

// 検証失敗（失敗）
bool ok3 = RespondentName.TryFrom("", out var name3);
// ok3 == false, name3.IsSet == false
```

---

## 4. 等価性メソッド設計

### 4.1 Equals(RespondentName? other) メソッド

```csharp
public bool Equals(RespondentName? other) => base.Equals(other);
```

**実装戦略:** 基本クラス `ValueObject` の `Equals(ValueObject?)` に処理を委譲

**委譲先の実装（ValueObject.cs）:**

```csharp
public bool Equals(ValueObject? other)
{
	if (other is null) return false;
	if (ReferenceEquals(this, other)) return true;
	if (other.GetType() != GetType()) return false;

	var normalizeComponents = ValueObjectComponentNormalizer.Normalize(this, GetEqualityComponents());
	var otherNormalizeComponents = ValueObjectComponentNormalizer.Normalize(other, other.GetEqualityComponents());

	return normalizeComponents.SequenceEqual(otherNormalizeComponents);
}
```

**RespondentName による GetValueComponents() のオーバーライド:**

```csharp
protected override IEnumerable<object?> GetValueComponents()
{
	if (IsSet)
	{
		yield return ValueField;
	}
}
```

**等価性判定の仕組み:**

```
a.Equals(b)
  ↓ RespondentName.Equals(RespondentName? other)
  ↓ base.Equals(other) → ValueObject.Equals(ValueObject? other)
  ↓ GetEqualityComponents() → [IsSet, GetValueComponents()]
  ↓ 比較コンポーネント: [IsSet, ValueField] （IsSet = true の場合）
  ↓ SequenceEqual() で比較

例1: 両者とも同じ名前
  a = ["true", "田中太郎"]
  b = ["true", "田中太郎"]
  → Equal ✅

例2: 名前が異なる
  a = ["true", "田中太郎"]
  b = ["true", "鈴木"]
  → Not Equal ✅

例3: 両者とも未設定
  a = ["false"]
  b = ["false"]
  → Equal ✅

例4: 一方が設定、一方が未設定
  a = ["true", "田中太郎"]
  b = ["false"]
  → Not Equal ✅
```

**使用例:**

```csharp
var a = RespondentName.From("太郎");
var b = RespondentName.From("太郎");
var c = RespondentName.From("花子");

Assert.True(a.Equals(b));   // 値が同じ → 等価
Assert.False(a.Equals(c));  // 値が異なる → 非等価

var u1 = RespondentName.Unset();
var u2 = RespondentName.Unset();
Assert.True(u1.Equals(u2));  // 両者とも未設定 → 等価
```

### 4.2 Equals(object? obj) メソッド

基本クラス `ValueObject` で実装済み：

```csharp
public override bool Equals(object? obj) 
	=> obj is ValueObject other && Equals(other);
```

---

## 5. ToString() メソッド設計

基本クラス `ValueObject` で実装済み：

```csharp
public override string ToString()
{
	if (!IsSet)
		return "Unset";

	return Format(ValueField);  // PrimitiveValueObject.Format() を使用
}
```

**PrimitiveValueObject.Format() のデフォルト実装:**

```csharp
protected virtual string Format(TValue value) => value?.ToString() ?? string.Empty;
```

**動作:**

```csharp
var name = RespondentName.From("田中太郎");
name.ToString()  // → "田中太郎"

var unset = RespondentName.Unset();
unset.ToString()  // → "Unset"
```

---

## 6. GetHashCode() メソッド設計

基本クラス `ValueObject` で実装済み：

```csharp
public override int GetHashCode()
{
	var normalizeComponents = ValueObjectComponentNormalizer.Normalize(this, GetEqualityComponents());

	return normalizeComponents
		.Aggregate(17, (current, component) =>
		{
			unchecked
			{
				return current * 31 + (component?.GetHashCode() ?? 0);
			}
		});
}
```

**ハッシュコード計算:**

```
hashCode = 17
hashCode = hashCode * 31 + IsSet.GetHashCode()
hashCode = hashCode * 31 + ValueField.GetHashCode()  (if IsSet)
```

**使用例:**

```csharp
var name1 = RespondentName.From("太郎");
var name2 = RespondentName.From("太郎");

var set = new HashSet<RespondentName> { name1 };
Assert.True(set.Contains(name2));  // ハッシュコードと等価性で判定

var dict = new Dictionary<RespondentName, string> { { name1, "data" } };
Assert.True(dict.ContainsKey(name2));
```

---

## 7. バリデーション・正規化設計

### 7.1 Validate() メソッド

デフォルト実装：何も検証しない

```csharp
protected virtual void Validate(TValue normalized)
{
	// RespondentName ではデフォルト実装（何もしない）
}
```

**将来の拡張:**

ビジネスルール（例：空文字列の禁止、最大長制限など）を追加する場合：

```csharp
protected override void Validate(string normalized)
{
	if (string.IsNullOrWhiteSpace(normalized))
		throw new ArgumentException("名前は空にできません", nameof(normalized));

	if (normalized.Length > 100)
		throw new ArgumentException("名前は100文字以内である必要があります", nameof(normalized));
}
```

### 7.2 Normalize() メソッド

デフォルト実装：値をそのまま返す

```csharp
protected virtual TValue Normalize(TValue input) => input;
```

**将来の拡張:**

正規化ルール（例：前後の空白をトリムなど）を追加する場合：

```csharp
protected override string Normalize(string input)
{
	return input.Trim();  // 前後の空白を削除
}
```

---

## 8. インターフェース実装

### 8.1 IOptionalValueObject<RespondentName, string>

**契約:**
```csharp
public interface IOptionalValueObject<TSelf, TValue> 
	where TSelf : ValueObject
{
	static abstract TSelf Unset();
	static abstract bool TryFrom(TValue? input, out TSelf result);
}
```

**実装:**
- `Unset()` → 未設定インスタンスを生成
- `TryFrom(string?)` → null 安全なファクトリ

### 8.2 IEquatable<RespondentName>

**契約:**
```csharp
public interface IEquatable<T>
{
	bool Equals(T? other);
}
```

**実装：** 基本クラスに委譲

---

## 9. メモリ効率と不変性

### 9.1 ValueField の管理

```csharp
protected readonly TValue ValueField;
```

- `readonly` で直接の変更を防止
- `IsSet = false` の場合、`ValueField = default!` で領域を最小化
- string は参照型で、複数インスタンスで同じ値文字列を共有可能（JIT 最適化）

### 9.2 不変性保証

すべてのプロパティが read-only により、インスタンス作成後の変更は不可能：

```csharp
var name = RespondentName.From("太郎");
// name.Value は read-only プロパティで、setter なし
// name.IsSet も read-only init で、インスタンス作成後に変更不可
```

---

## 10. DDD パターンの適用

### 10.1 Value Object パターン

- ✅ 不変性（immutability）確保
- ✅ 値による等価性判定
- ✅ ドメイン言語の明確化（string ではなく RespondentName）
- ✅ ビジネスルール検証の一元化（Validate/Normalize）

### 10.2 Option パターン

- ✅ `IsSet` フラグで null の代わりに未設定状態を表現
- ✅ null 参照例外を防止（型安全性）
- ✅ ビジネスロジックが未設定状態を明示的に処理

###10.3 Factory パターン

- ✅ 複数の生成戦略（Unset / From / TryFrom）
- ✅ バリデーション、正規化の一元管理
- ✅ コンストラクタ公開による問題回避

---

## 11. テスト設計ポイント

### 11.1 テスト対象メソッド

| メソッド | テスト項目数 | 注力ポイント |
|---|---|---|
| `From()` | 3-5 | null チェック、成功ケース |
| `TryFrom()` | 5-7 | null 入力、成功ケース、失敗ケース |
| `Unset()` | 2-3 | 未設定状態、等価性 |
| `Equals()` | 6-8 | 値が同じケース、異なるケース、未設定同士 |
| `ToString()` | 3-4 | 通常値、未設定状態 |

### 11.2 境界値テスト

- 空文字列 `""` （現状は許可、将来のルール追加に備え）
- 非常に長い文字列
- 特殊文字を含む文字列

### 11.3 null パターンテスト

- `TryFrom(null)` → Unset を返す
- `From(null)` → ArgumentNullException
- `Equals(null)` → false を返す

---

## 12. 参考資料

- **基本クラス:** `SupportAdvance.SharedKernel.ValueObjects.PrimitiveValueObject<TValue>`
- **インターフェース:** `SupportAdvance.SharedKernel.ValueObjects.IOptionalValueObject<TSelf, TValue>`
- **基盤クラス:** `SupportAdvance.SharedKernel.ValueObjects.ValueObject`
- **関連VO:** `CreatedAt`, `UpdatedAt`, `RespondentAge`
