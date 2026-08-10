# 技術仕様書 — RespondentPersonId

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain層  
**種別:** 値オブジェクト（ValueObject）  
**版:** 1.0 / 2026-01-10

---

## 1. 位置づけ

`RespondentPersonId` は SupportAdvance プロジェクトにおいて、**回答者の個人ID を表す値オブジェクト** である。

本値オブジェクトの目的は以下に集約される。

- 回答者を一意に識別する整数ID（1000～9999）を型安全に管理する
- ID の不正値（範囲外など）を構築時に検出・排除する
- ID が設定されていない状態（未設定）をOptional値として表現する
- 等価性、ハッシュ、文字列化を通じて、値オブジェクトのセマンティクスを実装する

> **🎯 原則**  
> 回答者ID は「単なる整数」ではなく、ビジネス意味を持つ一級の型である。型安全性と不変性により、誤用を防ぐ。

---

## 2. メンバー仕様

### 2.1 `IsSet` プロパティ

`RespondentPersonId` が値を持つ状態（IsSet=true）か、未設定状態（IsSet=false）かを示す。

| 項目 | 内容 |
|------|------|
| シグネチャ | `bool IsSet { get; }` |
| 戻り値 | true: 値が設定されている、false: 未設定状態 |
| 例外 | 例外を投げない |
| 用途 | Optional値の存在を判定する際に使用 |

**設計判断**

- IOptionalValueObject インターフェースの要件を満たすため、IsSet プロパティを公開
- 読み取り専用で、外部から変更不可
- 基底クラス PrimitiveValueObject から継承

---

### 2.2 `ValueField` フィールド

`IsSet=true` の場合のみ有効な、実際のID値を保持する。

| 項目 | 内容 |
|------|------|
| 型 | `int` |
| アクセス修飾子 | `protected readonly` |
| デフォルト値 | `default(int)` （IsSet=false の場合） |
| 有効範囲 | 1000～9999 |

**設計判断**

- protected readonly により、派生クラスからの読み取りのみ可能
- 不変性を確保し、インスタンス生成後の変更を防止

---

### 2.2b `Value` プロパティ

個人IDへの公開アクセスポイント（get のみ）

| 項目 | 内容 |
|------|------|
| 型 | `public int? { get; }` |
| 戻り値 | IsSet=true なら個人ID（1000～9999）、false なら null |
| 例外 | 例外を投げない |
| イミュータビリティ | get のみ（セッター不可） |

**実装**
```csharp
public int? Value => IsSet ? ValueField : null;
```

**Value と TryGetValue の使い分け**

| 用途 | メソッド | 利用場面 |
|------|---------|--------|
| **直接アクセス** | `Value` プロパティ | Domain ロジック（IsSet 既知） |
| **安全なアクセス** | `TryGetValue()` | 外部入力・レイヤ境界 |

---

### 2.3 `Unset()` メソッド

未設定状態の RespondentPersonId を生成する静的ファクトリメソッド。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static RespondentPersonId Unset()` |
| 戻り値 | IsSet=false の RespondentPersonId インスタンス |
| 例外 | 例外を投げない |
| 用途 | Optional値パターンで「値がない」状態を表現 |

**設計判断**

- IOptionalValueObject の static abstract メソッドを実装
- 毎回新インスタンスを生成（null ではなくインスタンス提供）

---

### 2.4 `From(int value)` メソッド

指定された整数値から RespondentPersonId を生成する静的ファクトリメソッド。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static RespondentPersonId From(int value)` |
| 戻り値 | IsSet=true で指定値を保有する RespondentPersonId インスタンス |
| 例外 | `ArgumentOutOfRangeException` （値が 1000～9999 の範囲外） |
| 用途 | 確定した有効値から ID を構築する場合 |

**設計判断**

- 値の正規化・検証は基底クラス PrimitiveValueObject が担当
- 無効値の場合は例外を投げる（Try パターンではなく Throw パターン）
- クライアント側で例外処理が必要

---

### 2.5 `TryFrom(int input, out RespondentPersonId result)` メソッド

指定された整数値から RespondentPersonId の生成を試みる。失敗時は Unset を返す。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom(int input, out RespondentPersonId result)` |
| 戻り値 | true: 生成成功、false: 生成失敗（無効値） |
| 例外 | 例外を投げない |
| 用途 | 外部入力（ユーザー入力、API リクエスト）から変換する場合 |

**設計判断**

- Try パターンで false を返す（例外ではなく）
- null チェックなしの場合、値そのものをチェック

---

### 2.6 `TryFrom(int? input, out RespondentPersonId result)` メソッド（IOptionalValueObject 実装）【重要】

Nullable な整数値から RespondentPersonId の生成を試みる（IOptionalValueObject 仕様）

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom(int? input, out RespondentPersonId result)` |
| 戻り値 | **true** = null 入力（Unset）**または** 有効値 / **false** = 無効値のみ |
| 例外 | 例外を投げない |
| 用途 | API 入力・UI フォーム：null 許容の個人ID値を安全に処理 |

**【重要】処理フロー（IOptionalValueObject 仕様）**

```csharp
public static bool TryFrom(int? input, out RespondentPersonId result)
{
    // ケース 1: null 入力 → Unset + true（正常な未設定）
    if (!input.HasValue)
    {
        result = Unset();
        return true;  // ✅ null は成功扱い
    }

    try
    {
        // ケース 2: 有効な値（1000～9999） → From + true
        result = From(input.Value);
        return true;  // ✅ 検証成功
    }
    catch (ArgumentOutOfRangeException)
    {
        // ケース 3: 無効な値（999以下,10000以上） → Unset + false
        result = Unset();
        return false;  // ❌ 検証失敗のみ失敗
    }
}
```

**【重要】戻り値の解釈**

| 戻り値 | result.IsSet | 意味 |
|------|------------|------|
| **true** | false | null 入力 → 未設定（正常） |
| **true** | true | 有効値（1000～9999） → 設定済み（正常） |
| **false** | false | 無効値（999以下,10000以上） → 検証失敗 |

**設計判断**

- ✅ input が null の場合、true を返し Unset インスタンスを result に設定（正常処理）
  - UI フォームの未入力は有効な入力パターン
- ❌ input に値がある場合、その値が無効なら false を返す（異常処理）
  - 入力値は存在するが、ビジネスルール違反

---

### 2.7 `TryGetValue(out int value)` メソッド

保有する値を取得する。IsSet=true の場合のみ値を返す。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public new bool TryGetValue(out int value)` |
| 戻り値 | true: 値が設定されている、false: 未設定 |
| 例外 | 例外を投げない |
| 用途 | Optional値パターンで安全に値を取得 |

**設計判断**

- IsSet=false の場合、value に 0 を設定して false を返す
- IOptionalValueObject インターフェースの要件を満たす

---

### 2.8 `Equals(RespondentPersonId? other)` メソッド

指定された RespondentPersonId と等価性を判定する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public bool Equals(RespondentPersonId? other)` |
| 戻り値 | true: 等価、false: 非等価 |
| 例外 | 例外を投げない |
| 用途 | 2 つの RespondentPersonId が同じ値を持つか判定 |

**設計判断**

- IsSet フラグと値の両方を比較
- 同じ IsSet・同じ値なら等価

---

### 2.9 `Equals(object? obj)` メソッド

指定されたオブジェクトと等価性を判定する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override bool Equals(object? obj)` |
| 戻り値 | true: 等価、false: 非等価 |
| 例外 | 例外を投げない |
| 用途 | 異なる型を含むオブジェクト比較 |

**設計判断**

- obj を RespondentPersonId にキャストして Equals に委譲
- null 参照や型変換失敗は自動的に false となる

---

### 2.10 `GetHashCode()` メソッド

オブジェクトのハッシュコードを取得する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override int GetHashCode()` |
| 戻り値 | IsSet フラグと ValueField を組み合わせたハッシュ値 |
| 例外 | 例外を投げない |
| 用途 | Dictionary / HashSet などのコレクションで使用 |

**設計判断**

- HashCode.Combine(IsSet, ValueField) で生成
- Equals=true なら同一ハッシュ値を保証

---

### 2.11 `GetValueComponents()` メソッド

等価性の比較に使用する値コンポーネントを列挙する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override IEnumerable<object?> GetValueComponents()` |
| 戻り値 | IsSet=true の場合のみ ValueField を返す（IsSet は基底で自動追加） |
| 例外 | 例外を投げない |
| 用途 | 基底クラス ValueObject による等価性判定の基盤 |

**設計判断**

- ValueField のみを返す（IsSet は基底クラスで自動的に等価性判定に含まれる）

---

### 2.12 `Validate(int normalized)` メソッド

正規化済みの値が有効な範囲内か検証する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override void Validate(int normalized)` |
| パラメータ | `normalized`: 正規化済みの値 |
| 例外 | `ArgumentOutOfRangeException` （1000～9999 の範囲外） |
| 用途 | 値オブジェクト構築時の値検証 |

**設計判断**

- 最小値 1000、最大値 9999
- 範囲外なら ArgumentOutOfRangeException を投げ、構築をキャンセル

---

## 3. 設計制約・禁止事項

### 3.1 レイヤ制約（CLAUDE.md 準拠）

| ❌ 禁止（知ってはならないもの） | ✅ 許可（知ってよいもの） |
|---------|---------|
| ❌ Application 層以上の責務 | ✅ Domain 層の値オブジェクト |
| ❌ Infrastructure 層の永続化実装 | ✅ 値の不変性と型安全性 |
| ❌ UI・プレゼンテーション層の知識 | ✅ IOptionalValueObject インターフェース |

### 3.2 実装上の禁止事項

- **sealed クラス化の必須化**: 値オブジェクトの不変性を確保するため、派生クラスの作成を禁止
- **プロパティセッターの禁止**: 生成後の値変更を不可能にする（ValueField は readonly）
- **パラメータレスコンストラクタの禁止**: 不正な状態での構築を防止
- **ToString オーバーライドの禁止**: IsSet の値そのものは ToString に含めない（仕様要件）

---

## 4. 実装ガイドライン

### 4.1 実装義務

- IOptionalValueObject<RespondentPersonId, int> インターフェースを実装する
- IEquatable<RespondentPersonId> インターフェースを実装する
- GetEqualityComponents() を override し、IsSet と ValueField を返す
- Validate(int normalized) を override し、1000～9999 の範囲チェックを実行する

### 4.2 最小限の実装例（設計意図の説明）

```csharp
// sealed で不変性を ens
public sealed class RespondentPersonId : PrimitiveValueObject<int>, 
	IOptionalValueObject<RespondentPersonId, int>, IEquatable<RespondentPersonId>
{
	// IsSet=false のインスタンスを生成
	public static RespondentPersonId Unset() => new(false);

	// 整数値から生成（バリデーション付き）
	public static RespondentPersonId From(int value) => new(value, true);

	// Try パターン: null 許容で生成試行
	public static bool TryFrom(int? input, out RespondentPersonId result)
	{
		if (!input.HasValue)
		{
			result = Unset();
			return true;  // null は正常処理
		}
		try
		{
			result = From(input.Value);
			return true;
		}
		catch (ArgumentOutOfRangeException)
		{
			result = Unset();
			return false;  // 無効値は異常処理
		}
	}

	// 値を安全に取得
	public new bool TryGetValue(out int value)
	{
		if (!IsSet) { value = 0; return false; }
		value = ValueField;
		return true;
	}

	// オプショナル値の等価性判定
	protected override IEnumerable<object?> GetEqualityComponents()
	{
		yield return IsSet;
		if (IsSet) yield return ValueField;
	}

	// 値の範囲検証
	protected override void Validate(int normalized)
	{
		base.Validate(normalized);
		const int minId = 1000, maxId = 9999;
		if (normalized < minId || normalized > maxId)
			throw new ArgumentOutOfRangeException(
				nameof(normalized), normalized,
				$"RespondentPersonId must be between {minId} and {maxId}.");
	}
}
```

---

## 5. IOptionalValueObject インターフェース対応

RespondentPersonId は IOptionalValueObject<RespondentPersonId, int> を実装し、以下の特性をサポート：

- **Unset 状態管理**: null ではなくインスタンスで「値がない」を表現
- **Try パターン**: From / TryFrom / TryGetValue で例外を最小化
- **null 許容**: API 層での null 入力を安全に処理

---

## 6. 版管理

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-01-10 | 初版作成 |

