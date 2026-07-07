# 詳細設計書 — RespondentAge

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** プリミティブ値オブジェクト詳細設計書  
**依存技術仕様書:** RespondentAge 技術仕様書 v1.0  
**版:** 1.0 / 2026-07-04

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `RespondentAge` |
| 種別 | `public sealed class` |
| 名前空間 | `SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects` |
| 実装インターフェース | `IOptionalValueObject<RespondentAge, int>`, `IEquatable<RespondentAge>` |
| 継承元 | `PrimitiveValueObject<int>` |
| 配置レイヤ | Domain 層（値オブジェクト基礎） |

---

### 1.2 責務

- **年齢値の管理**: 0～150 歳の正整数値をスカラー値オブジェクトとして管理する
- **値の検証**: ビジネスルール（0～150歳）に従って妥当性チェックを実施
- **未設定状態対応**: IOptionalValueObject を実装し、未設定状態（Unset）に対応
- **等価性判定**: IsSet と ValueField に基づく同一性判定
- **安全な取得**: TryGetValue メソッドで out パラメータ経由の取得

---

### 1.3 協調クラス図

```
RespondentAge
  ├─ extends ──▶ PrimitiveValueObject<int>
  │              ├─ IsSet プロパティ（bool）
  │              ├─ ValueField フィールド（readonly int）
  │              ├─ Normalize(int) 抽象メソッド
  │              ├─ Validate(int) 抽象メソッド
  │              └─ TryGetValue(out int) メソッド
  │
  └─ implements ──▶ IOptionalValueObject<RespondentAge, int>
					├─ From(int) 静的メソッド
					├─ Unset() 静的メソッド
					├─ TryFrom(int?, out RespondentAge) 静的メソッド
					└─ TryFrom(int, out RespondentAge) 静的メソッド
```

---

## 2. フィールド・プロパティ設計

### 2.1 IsSet プロパティ（継承）

| 項目 | 内容 |
|------|------|
| 型 | `public bool` |
| アクセス | `{ get; protected init; }` |
| 値 | 常に true（有効状態をマーク） |
| 用途 | 等価性判定、未設定状態判断の根拠 |

**設計判断**

- 基底クラス ValueObject から取得
- Unset() インスタンスのみ false を保持
- From() で生成されるすべてのインスタンスは true

---

### 2.2 ValueField フィールド（継承）

| 項目 | 内容 |
|------|------|
| 型 | `protected readonly int` |
| アクセス | `protected` |
| デフォルト値 | IsSet=false の場合は 0（default）、IsSet=true の場合は Normalize 後の値 |
| 用途 | 年齢値の保持 |

**設計判断**

- protected readonly で派生クラスからは読み取り可能、外部からは変更不可
- IsSet=false 時でも ValueField=0 だが、IsSet フラグが判定の根拠

---

### 2.2b Value プロパティ

| 項目 | 内容 |
|------|------|
| 型 | `public int?` |
| アクセス | `{ get; }` |
| 実装 | `IsSet ? ValueField : null` |
| 用途 | Domain ロジック内での値取得 |

**実装**
```csharp
public int? Value => IsSet ? ValueField : null;
```

**設計判断:**
- IsSet=true なら年齢値（0～150）を返す
- IsSet=false なら null を返す
- イミュータビリティのため get のみ

**Value と TryGetValue の役割分担:**

| 用途 | メソッド | 用いられるシーン |
|------|---------|:----------:|
| **直接参照** | `Value` プロパティ | Domain ロジック（IsSet 既知） |
| **安全取得** | `TryGetValue(out int)` | 外部入力・レイヤ境界 |

**使用例:**
```csharp
var age = RespondentAge.From(30);

// Value プロパティ（Domain ロジック内）
if (age.Value.HasValue && age.Value < 20)
{
    Console.WriteLine("未成年");
}

// TryGetValue（外部入力処理）
if (age.TryGetValue(out var ageValue))
{
    ProcessAge(ageValue);
}
```

---

### 2.3 UnsetInstance 静的フィールド

| 項目 | 内容 |
|------|------|
| 型 | `private static readonly RespondentAge` |
| 用途 | 未設定インスタンスのシングルトン管理 |

**実装例**

```csharp
private static readonly RespondentAge UnsetInstance = new();
```

---

## 3. コンストラクタ設計

### 3.1 未設定インスタンス用コンストラクタ（private）

```csharp
private RespondentAge(bool isSet) : base(isSet)
{
}
```

**責務**

- 未設定状態（isSet=false）の RespondentAge を生成
- PrimitiveValueObject コンストラクタに isSet を委譲

**処理フロー**

1. `base(isSet)` で基底クラスコンストラクタを呼び出し
2. 基底クラスで：
   - IsSet = isSet（false）を設定
   - ValueField = default!（0）を設定
   - Validate は実行されない（if (isSet) により skip）

**設計判断**

- private アクセスにより、派生クラスのみ（実装上は Unset() メソッド）で呼び出し可能

---

### 3.2 値設定インスタンス用コンストラクタ（private）

```csharp
private RespondentAge(int value, bool isSet) : base(value, isSet)
{
}
```

**責務**

- 指定された整数値を持つ RespondentAge を生成
- 値の正規化と検証を実施

**処理フロー**

1. `base(value, isSet)` コンストラクタを呼び出し
2. 基底クラス PrimitiveValueObject で：
   - IsSet = isSet を設定
   - IsSet が true の場合：
	 - normalized = Normalize(value) を実行
	 - ValueField = normalized を設定
	 - Validate(normalized) を実行（例外可能）
   - IsSet が false の場合：
	 - ValueField = default!（0）を設定
	 - Validate はスキップ

**設計判断**

- private アクセスにより、外部からの直接呼び出しは不可
- From() メソッド経由でのみ呼び出し可能

---

## 4. メソッド設計

### 4.1 From(int value) 静的メソッド

**シグネチャ**

```csharp
public static RespondentAge From(int value) 
	=> new(value, true);
```

**責務**

- 指定された年齢値から RespondentAge オブジェクトを生成

**処理フロー**

1. `new(value, true)` でコンストラクタを呼び出す
2. コンストラクタで Normalize → Validate が実行される
3. Validate が ArgumentOutOfRangeException を投げた場合、インスタンス生成は中止

**例外処理**

```
From(0)    → OK（0歳は有効）
From(25)   → OK
From(150)  → OK
From(-1)   → ArgumentOutOfRangeException
From(151)  → ArgumentOutOfRangeException
```

**設計判断**

- 例外スロー時はインスタンス生成を中止（fail-fast）
- 呼び出し元で例外をキャッチして TryFrom へ委譲

---

### 4.2 Unset() 静的メソッド

**シグネチャ**

```csharp
public static RespondentAge Unset() 
	=> UnsetInstance;
```

**責務**

- 未設定状態の RespondentAge オブジェクトを返す

**処理フロー**

1. 静的フィールド UnsetInstance を返す（シングルトン）
2. Validate は実行されない

**設計判断**

- シングルトン化により、メモリ効率と等価性判定の一貫性を確保

---

### 4.3 TryFrom(int? input, out RespondentAge result) 静的メソッド（IOptionalValueObject 実装）【重要】

**シグネチャ**

```csharp
public static bool TryFrom(int? input, out RespondentAge result)
{
	if (!input.HasValue)
	{
		result = Unset();
		return true;  // ✅ null は成功（未設定は正常）
	}

	try
	{
		result = From(input.Value);
		return true;  // ✅ 検証成功
	}
	catch (ArgumentOutOfRangeException)
	{
		result = Unset();
		return false;  // ❌ 検証失敗のみ失敗
	}
}
```

**【重要】責務（IOptionalValueObject 契約）**

- **null 入力は正常な未設定状態** → true + Unset を返す
- **有効な値は設定済み状態** → true + 検証済みインスタンス
- **無効な値は検証失敗** → false + Unset を返す

**【重要】戻り値マトリックス**

| 戻り値 | result.IsSet | 入力例 | 意味 |
|------|:---:|:---:|------|
| **true** | false | `TryFrom(null, ...)` | null 入力 → 未設定（正常） |
| **true** | true | `TryFrom(30, ...)` | 有効値（0～150） → 設定済み（正常） |
| **false** | false | `TryFrom(-1, ...)` または `TryFrom(151, ...)` | 無効値 → 検証失敗 |

**処理フロー図**

```
TryFrom(int? input, out RespondentAge result)
  │
  ├─ input.HasValue == false（null 入力）
  │   └─ result = Unset()
  │   └─ return true  ← 正常な未設定状態
  │
  └─ input.HasValue == true
      └─ try From(input.Value)
          ├─ Success（0～150）
          │   └─ result = 検証済みインスタンス
          │   └─ return true  ← 正常な設定済み状態
          └─ Catch ArgumentOutOfRangeException（-1,151以上）
              └─ result = Unset()
              └─ return false  ← 検証失敗
```

**使用例と戻り値の解釈**

```csharp
// ケース 1: null 入力（UI フォーム未入力）
if (RespondentAge.TryFrom(null, out var age1))
{
    if (age1.IsSet)
        Console.WriteLine($"年齢: {age1.Value}");
    else
        Console.WriteLine("年齢未指定");  // ← 正常な入力
}

// ケース 2: 有効な値
if (RespondentAge.TryFrom(30, out var age2))
{
    if (age2.IsSet)
        Console.WriteLine($"年齢: {age2.Value}");  // ← true, IsSet=true
    else
        Console.WriteLine("予期しない");
}

// ケース 3: 無効な値（範囲外）
if (RespondentAge.TryFrom(-1, out var age3))
{
    Console.WriteLine("検証成功（予期しない）");
}
else
{
    Console.WriteLine("検証失敗（無効な年齢）");  // ← false, IsSet=false
}
```

**設計判断**

- null は「正常な未設定」として扱う（return true）
  - フォーム送信で年齢未指定は有効な入力パターン
- 値の範囲外は「例外的な未設定」として扱う（return false）
  - 入力値は存在するが、ビジネスルール違反

**処理フロー（詳細版）**

1. input.HasValue が false の場合：
   - `result = Unset()` を設定
   - `return true`（null は正常な未設定状態）
2. input.HasValue が true の場合：
   - try: `result = From(input.Value)` 呼び出し
   - 成功時: `return true`
   - catch (ArgumentOutOfRangeException): `result = Unset()` を設定、`return false`

**例外処理**

```
TryFrom(null, out result)       → result = Unset(), return true
TryFrom(25, out result)         → result = From(25), return true
TryFrom(-1, out result)         → result = Unset(), return false
TryFrom(151, out result)        → result = Unset(), return false
```

**設計判断**

- null は「正常な未設定」として扱う（return true）
- 範囲外値は「例外的な未設定」として扱う（return false）
- out パラメータは常に値を受け取る（失敗時は Unset()）

---

### 4.4 TryFrom(int input, out RespondentAge result) 静的メソッド（オーバーロード）

**シグネチャ**

```csharp
public static bool TryFrom(int input, out RespondentAge result) 
	=> TryFrom((int?)input, out result);
```

**責務**

- nullable でない int 値から RespondentAge への変換を試みる

**処理フロー**

1. `(int?)input` で自動的に nullable int に変換
2. 4.3 の nullable メソッドに委譲

**設計判断**

- オーバーロードにより、nullable/non-nullable の両方を統一的に処理

---

### 4.5 TryGetValue(out int value) メソッド

**シグネチャ**

```csharp
public new bool TryGetValue(out int value)
{
	if (!IsSet)
	{
		value = 0;
		return false;
	}

	value = ValueField;
	return true;
}
```

**責務**

- 年齢値を安全に取得する
- 基底クラスのメソッドをオーバーライド

**処理フロー**

1. IsSet が false の場合：
   - value = 0 を設定
   - return false
2. IsSet が true の場合：
   - value = ValueField を設定
   - return true

**設計判断**

- IsSet=true のみが有効な値を返す
- IsSet=false 時は default(0) を返す

---

### 4.6 Normalize(int input) メソッド（オーバーライド不要）

**シグネチャ**

```csharp
// 基底クラスのデフォルト実装を使用
protected virtual int Normalize(int input) 
	=> input;
```

**処理フロー**

- 当クラスではオーバーライドせず、基底クラスのデフォルト実装を使用
- 入力値をそのまま返す（正規化不要）

**設計判断**

- 年齢値は既に正規形（正整数値）

---

### 4.7 Validate(int normalized) メソッド（オーバーライド）

**シグネチャ**

```csharp
protected override void Validate(int normalized)
{
	base.Validate(normalized);

	const int minAge = 0;
	const int maxAge = 150;

	if (normalized < minAge || normalized > maxAge)
	{
		throw new ArgumentOutOfRangeException(
			nameof(normalized), 
			$"Age must be between {minAge} and {maxAge}."
		);
	}
}
```

**責務**

- ビジネスルールに基づく年齢値の検証

**処理フロー**

1. `base.Validate(normalized)` で基底クラスの検証を実施（前処理）
2. 定数定義：minAge = 0, maxAge = 150
3. normalized が範囲外の場合、ArgumentOutOfRangeException を投げる

**設計判断**

- ビジネスルール: 年齢は 0～150 歳の範囲に制限
- エラーメッセージは英語（国際化対応予定）

---

## 5. 等価性・ハッシング設計

### 5.1 GetValueComponents() メソッド

**シグネチャ**

```csharp
protected override IEnumerable<object?> GetValueComponents()
{
	if (IsSet)
	{
		yield return ValueField;
	}
}
```

**責務**

- 等価性判定で使用する値コンポーネントを列挙（IsSet は基底クラスで自動追加）

**処理フロー**

1. IsSet を最初に yield return（判定の根拠）
2. IsSet が true の場合のみ ValueField を yield return
3. IsSet が false の場合は ValueField を返さない

**等価性規則**

```
Unset()       → (false)
Unset()       → (false)            ← 等価（IsSet が同じ）

From(0)       → (true, 0)
Unset()       → (false)            ← 非等価（IsSet が異なる）

From(0)       → (true, 0)
From(0)       → (true, 0)          ← 等価（IsSet と ValueField が同じ）

From(25)      → (true, 25)
From(25)      → (true, 25)         ← 等価
```

**設計判断**

- IsSet フラグを常に含める（0 vs null 区別の根本）
- IsSet=false 時は ValueField を返さない（未設定状態の内部表現に依存しない）

---

### 5.2 Equals(RespondentAge? other) メソッド

**シグネチャ**

```csharp
public bool Equals(RespondentAge? other)
{
	if (other is null) return false;
	if (ReferenceEquals(this, other)) return true;
	return IsSet == other.IsSet && ValueField == other.ValueField;
}
```

**責務**

- 同じ型の RespondentAge との等価性判定

**処理フロー**

1. other が null の場合 → false
2. ReferenceEquals(this, other) の場合 → true（同じインスタンス）
3. IsSet と ValueField が両方等しい場合 → true
4. その他 → false

---

### 5.3 Equals(object? obj) メソッド

**シグネチャ**

```csharp
public override bool Equals(object? obj) 
	=> Equals(obj as RespondentAge);
```

**責務**

- 異なる型のオブジェクトとの等価性判定

**処理フロー**

1. obj を RespondentAge にキャスト（失敗時は null）
2. Equals(RespondentAge?) に委譲

---

### 5.4 GetHashCode() メソッド

**シグネチャ**

```csharp
public override int GetHashCode() 
	=> HashCode.Combine(IsSet, ValueField);
```

**責務**

- ハッシュコード生成（コレクション・辞書での使用対応）

**処理フロー**

1. IsSet と ValueField を HashCode.Combine で結合
2. 結果を戻す

**設計判断**

- IsSet と ValueField 両方を含める（0 vs null 区別保証）
- 等価なオブジェクトは同じハッシュコードを持つ

---

## 6. 文字列化設計

### 6.1 ToString() メソッド

**シグネチャ**

```csharp
public override string ToString() 
	=> IsSet ? ValueField.ToString() : "Unset";
```

**責務**

- オブジェクトを文字列表現する

**処理フロー**

1. IsSet が true の場合：
   - ValueField.ToString() → "0", "18", "75" 等
2. IsSet が false の場合：
   - "Unset"

**用途**

- ログ出力
- UI 表示
- デバッグ情報

---

## 7. 設計判断・注釈

### 7.1 IOptionalValueObject 実装

RespondentAge は UI からの入力が未設定である可能性があるため、IOptionalValueObject を実装する。これにより、Null 安全性と業務ルール（未設定と0歳の区別）を両立させる。

### 7.2 0歳の扱い

0 歳（新生児）は有効な年齢値である。Unset()（未設定値）とは IsSet フラグで区別する。

### 7.3 sealed クラス

派生を禁止（sealed）することで、Validate / Normalize / Format の拡張を意図的に制限し、年齢値オブジェクトとしての一貫性を保つ。

### 7.4 staticReadonly UnsetInstance

Unset() メソッドは常に同一のインスタンスを返す（シングルトン）。これにより等価性判定の一貫性を確保し、メモリ効率を向上させる。

---

## 附録

### A. 定数一覧

| 定数 | 値 | 用途 |
|------|-----|------|
| minAge | 0 | 年齢最小値 |
| maxAge | 150 | 年齢最大値 |

---

### B. 依存クラス

- `PrimitiveValueObject<int>`: 基底クラス
- `IOptionalValueObject<RespondentAge, int>`: インターフェース
- `IEquatable<RespondentAge>`: 等価性インターフェース
- `ValueObject`: 基礎クラス（間接継承）

---

### C. 外部参照

- [RespondentAge 技術仕様書](RespondentAge_技術仕様書.md)
- [PrimitiveValueObject 技術仕様書](../../SharedKernel/ValueObjects/PrimitiveValueObject_技術仕様書.md)
