# 詳細設計書 — RespondentPersonId

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain層  
**種別:** 値オブジェクト（ValueObject）  
**依拠技術仕様書:** RespondentPersonId技術仕様書 v1.0  
**版:** 1.0 / 2026-01-10

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `RespondentPersonId` |
| 種別 | `sealed class` |
| 名前空間 | `SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects` |
| 実装インターフェース | `PrimitiveValueObject<int>`, `IOptionalValueObject<RespondentPersonId, int>`, `IEquatable<RespondentPersonId>` |
| 継承元 | `PrimitiveValueObject<int>` |
| 配置レイヤ | Domain 層 |

### 1.2 責務

- **ID値の格納と管理**: 1000～9999 の範囲の整数値を不変的に保持し、外部に提供する
- **値の検証と正規化**: 構築時に値が有効範囲内か検査し、無効値を排除する
- **未設定状態の表現**: null ではなく IsSet フラグで「値がない」状態を明示的に管理する
- **等価性と識別**: 同じID値を持つインスタンスは等価と見なし、ハッシュコード整合性を確保する
- **型安全な変換**: 外部入力からの安全な変換インターフェース（From, TryFrom, TryGetValue）を提供する

### 1.3 協調クラス

```
RespondentPersonId  ──uses──▶  PrimitiveValueObject<int>
								  │
								  ├─ Normalize(int input): int
								  ├─ Validate(int normalized): void
								  └─ Format(int value): string

RespondentPersonId  ──implements──▶  IOptionalValueObject<RespondentPersonId, int>
										 │
										 ├─ Unset()
										 ├─ From(value)
										 └─ TryFrom(input, out result)

RespondentPersonId  ──implements──▶  IEquatable<RespondentPersonId>
										 └─ Equals(RespondentPersonId? other)
```

依存関係：
- **PrimitiveValueObject<int>**: 値の正規化・検証の基盤を提供
- **IOptionalValueObject<RespondentPersonId, int>**: Optional値パターンの契約を定義
- **IEquatable<RespondentPersonId>**: 型安全な等価性判定を提供

---

## 2. プロパティ設計

### 2.1 `IsSet` プロパティ

| 項目 | 内容 |
|------|------|
| 型 | `bool` |
| アクセス修飾子 | `public get` |
| セッター | なし（読み取り専用） |
| デフォルト値 | なし（基底クラスから継承） |

**設計判断**

- 基底クラス PrimitiveValueObject が IsSet フラグを既に保有
- 読み取り専用で、外部から値を変更できない
- 値が設定されているか判定するための必須プロパティ

---

### 2.2 `ValueField` フィールド

| 項目 | 内容 |
|------|------|
| 型 | `int` |
| アクセス修飾子 | `protected readonly` |
| デフォルト値 | `default(int)` （IsSet=false の場合）、1000～9999（IsSet=true の場合） |

**設計判断**

- protected readonly により、派生クラスからの読み取りのみ可能
- 不変性を確保し、構築後の変更を防止
- IsSet=false の場合、値は default(0) だが意味を持たない

---

## 3. メソッド設計

### 3.1 プライベート コンストラクタ （IsSet のみ）

```csharp
private RespondentPersonId(bool isSet) : base(isSet)
{
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | `isSet: bool` - 未設定状態を示すフラグ |
| 呼び出し元 | `Unset()` メソッド |
| 処理 | 基底クラスコンストラクタに isSet フラグを渡し、Unset インスタンスを生成 |

**設計判断**

- private で、クライアント直接呼び出しを禁止
- 基底クラスが IsSet フラグを管理
- ValueField は default(0) で初期化される

---

### 3.2 プライベート コンストラクタ （値+IsSet）

```csharp
private RespondentPersonId(int value, bool isSet) : base(value, isSet)
{
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | `value: int` - ID値、`isSet: bool` - フラグ |
| 呼び出し元 | `From()` メソッド |
| 処理 | 基底クラスコンストラクタで Normalize・Validate を実行 |

**設計判断**

- private で直接呼び出しを禁止
- isSet=true の場合、base() 内で Validate(normalized) が呼ばれ、範囲チェックを実行
- 無効値は ArgumentOutOfRangeException を投げ、構築が中止される

---

### 3.3 `Unset()` 静的メソッド

```csharp
public static RespondentPersonId Unset() => new(false);
```

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static RespondentPersonId Unset()` |
| 戻り値 | IsSet=false のインスタンス |
| 処理フロー | プライベートコンストラクタ(bool) を呼び出す |

**設計判断**

- IOptionalValueObject の要件を満たす static abstract メソッド
- 毎回新しいインスタンスを生成
- null ではなくインスタンスで「値がない」を表現

---

### 3.4 `From(int value)` 静的メソッド

```csharp
public static RespondentPersonId From(int value) => new(value, true);
```

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static RespondentPersonId From(int value)` |
| パラメータ | `value: int` - 設定するID値 |
| 戻り値 | IsSet=true で指定値を保有するインスタンス |
| 例外 | `ArgumentOutOfRangeException` （値が 1000～9999 の範囲外） |
| 処理フロー | プライベートコンストラクタ(int, bool) → Validate() 実行 |

**設計判断**

- Throw パターンで、無効値は例外を投げる
- クライアント側での例外処理が必須
- 値の確定した場合に使用（API 入力からは TryFrom を推奨）

---

### 3.5 `TryFrom(int input, out RespondentPersonId result)` 静的メソッド

```csharp
public static bool TryFrom(int input, out RespondentPersonId result) 
	=> TryFrom((int?)input, out result);
```

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom(int input, out RespondentPersonId result)` |
| パラメータ | `input: int` - 変換元のID値 |
| 戻り値 | true: 成功、false: 失敗 |
| 処理 | nullable int に変換して TryFrom(int?, ...) に委譲 |

**設計判断**

- Nullable 版への委譲で、共通ロジックを集約
- non-nullable int からの呼び出しを簡潔にする

---

### 3.6 `TryFrom(int? input, out RespondentPersonId result)` 静的メソッド

```csharp
public static bool TryFrom(int? input, out RespondentPersonId result)
{
	// null の場合は未設定状態の RespondentPersonId を返す（正常処理）
	if (!input.HasValue)
	{
		result = Unset();
		return true;
	}

	try
	{
		result = From(input.Value);
		return true;
	}
	catch (ArgumentOutOfRangeException)
	{
		// 入力値が不正な場合は、未設定状態の RespondentPersonId を返す（異常処理）
		result = Unset();
		return false;
	}
}
```

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom(int? input, out RespondentPersonId result)` |
| パラメータ | `input: int?` - Nullable な変換元値 |
| 戻り値 | true: 成功または null、false: 値が無効 |
| 処理 | null チェック → From() 呼び出し → 例外キャッチ |

**処理フロー詳細**

1. input.HasValue が false → Unset() を result に設定、true を返す（null は正常処理）
2. input.HasValue が true → From(input.Value) を実行
   - 成功 → result に代入、true を返す
   - ArgumentOutOfRangeException → Unset() を result に設定、false を返す（値が不正）

**設計判断**

- Try パターンで例外を吸収し、戻り値で成否を示す
- null 許容の API 入力を安全に処理
- IOptionalValueObject の要件を満たす

---

### 3.7 `TryGetValue(out int value)` 公開メソッド

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

| 項目 | 内容 |
|------|------|
| シグネチャ | `public new bool TryGetValue(out int value)` |
| パラメータ | `value: out int` - 取得したID値の格納先 |
| 戻り値 | true: IsSet=true で value に値が設定、false: IsSet=false で value=0 |
| 処理 | IsSet フラグをチェック → 値を out パラメータに設定 |

**設計判断**

- IsSet=false の場合、value に 0 を設定して false を返す
- IOptionalValueObject の要件を満たす
- クライアント側で戻り値をチェック後、value を利用

---

### 3.8 `Equals(RespondentPersonId? other)` メソッド

```csharp
public bool Equals(RespondentPersonId? other) 
	=> other is not null && Equals((ValueObject?)other);
```

| 項目 | 内容 |
|------|------|
| シグネチャ | `public bool Equals(RespondentPersonId? other)` |
| パラメータ | `other: RespondentPersonId?` - 比較対象 |
| 戻り値 | true: 等価、false: 非等価 |
| 処理 | null チェック → 基底クラス Equals に委譲 |

**設計判断**

- IEquatable<RespondentPersonId> の実装
- other が null の場合速やかに false を返す
- 実装には基底クラス ValueObject の Equals を使用（GetEqualityComponents で IsSet と ValueField を比較）

---

### 3.9 `Equals(object? obj)` オーバーライド メソッド

```csharp
public override bool Equals(object? obj) => Equals(obj as RespondentPersonId);
```

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override bool Equals(object? obj)` |
| パラメータ | `obj: object?` - 比較対象 |
| 戻り値 | true: 等価、false: 非等価 |
| 処理 | obj を RespondentPersonId にキャスト → Equals(RespondentPersonId?) に委譲 |

**設計判断**

- object レベルの等価性判定をサポート
- キャスト失敗時は null として扱われ、自動的に false が返される

---

### 3.10 `GetHashCode()` オーバーライド メソッド

```csharp
public override int GetHashCode() => HashCode.Combine(IsSet, ValueField);
```

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override int GetHashCode()` |
| 戻り値 | IsSet と ValueField を組み合わせたハッシュ値 |
| 処理 | HashCode.Combine(...) で安全にハッシュ値を生成 |

**設計判断**

- .NET の HashCode.Combine を使用（.NET 5.0 以上）
- IsSet=true と false で異なるハッシュ値を返す
- 同じ IsSet・同じ値なら同一ハッシュ値を保証

---

### 3.11 `GetValueComponents()` 保護オーバーライド メソッド

```csharp
protected override IEnumerable<object?> GetValueComponents()
{
	if (IsSet)
	{
		yield return ValueField;
	}
}
```

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override IEnumerable<object?> GetValueComponents()` |
| 戻り値 | IsSet=true の場合のみ ValueField を返す |
| 処理 | ValueField（存在する場合）のみを yield return（IsSet は基底で自動追加） |

**設計判断**

- 基底クラス ValueObject の Equals で使用される
- IsSet で2つのインスタンスが異なれば、値の比較をスキップ
- Unset インスタンス同士は等価（どちらも IsSet=false）

---

### 3.12 `Validate(int normalized)` 保護オーバーライド メソッド

```csharp
protected override void Validate(int normalized)
{
	base.Validate(normalized);

	const int minId = 1000;
	const int maxId = 9999;

	if (normalized < minId || normalized > maxId)
	{
		throw new ArgumentOutOfRangeException(
			nameof(normalized),
			normalized,
			$"RespondentPersonId must be between {minId} and {maxId}.");
	}
}
```

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override void Validate(int normalized)` |
| パラメータ | `normalized: int` - 正規化済みの値 |
| 例外 | `ArgumentOutOfRangeException` （1000～9999 の範囲外） |
| 処理 | 範囲チェック → 範囲外なら例外を投げる |

**設計判断**

- 基底クラスの Validate を先に呼び出す
- 最小値 1000、最大値 9999（回答者ID の定義に基づく）
- 範囲外なら名前、値、メッセージを含む ArgumentOutOfRangeException を投げる

---

## 4. レイヤ制約確認

### 4.1 許可事項

✅ Domain 層の値オブジェクト  
✅ PrimitiveValueObject<int> からの継承  
✅ IOptionalValueObject インターフェース実装  
✅ 値の不変性と型安全性  

### 4.2 禁止事項

❌ Application 層以上の責務を持たない  
❌ Infrastructure 層の永続化実装を含まない  
❌ UI・プレゼンテーション層の知識を持たない  
❌ セッターによる値変更を許さない  
❌ 派生クラスの作成を許さない

---

## 5. 版管理

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-01-10 | 初版作成 |

