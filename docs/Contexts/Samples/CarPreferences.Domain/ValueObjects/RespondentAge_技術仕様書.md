# 技術仕様書 — RespondentAge

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** プリミティブ値オブジェクト技術仕様書  
**依存技術仕様書:** PrimitiveValueObject<TValue> 技術仕様書 v1.0  
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
| ジェネリック制約 | なし |

---

### 1.2 責務

- **年齢値の管理**: 0～150 歳の正整数値をスカラー値オブジェクトとして管理する
- **値の検証**: ビジネスルール（0～150歳）に従って妥当性チェックを実施
- **未設定状態対応**: IOptionalValueObject を実装し、未設定状態（Unset）に対応
- **等価性判定**: IsSet と ValueField に基づく同一性判定
- **業務名称の管理**: ValueField に対応する表示名の生成（将来拡張用）
- **安全な取得**: TryGetValue メソッドで out パラメータ経由の取得

---

### 1.3 協調クラス

```
RespondentAge
  ├─ extends ──▶ PrimitiveValueObject<int>
  │              ├─ Normalize(int) 抽象メソッド
  │              ├─ Validate(int) 抽象メソッド
  │              ├─ TryGetValue(out int) メソッド
  │              └─ IsSet プロパティ（継承元から）
  │
  └─ implements ──▶ IOptionalValueObject<RespondentAge, int>
					├─ From(int) 静的メソッド
					├─ Unset() 静的メソッド
					├─ TryFrom(int?, out RespondentAge) 静的メソッド
					└─ TryFrom(int, out RespondentAge) 静的メソッド

```

---

## 2. プロパティ・メソッド仕様

### 2.1 IsSet プロパティ

| 項目 | 内容 |
|------|------|
| シグネチャ | `public bool IsSet { get; protected init; }` |
| 戻り値 | true（常に。IsSet=true のみが有効状態） |
| 例外 | 例外を投げない |
| 用途 | 等価性判定、ToString 判定、**0 vs null の識別** |

**設計判断：デフォルト値 + IsSet による 0 vs null 識別**

- **コア概念**: `ValueField = default(0)` でも、`IsSet` フラグにより実際の意味を区別する
  - `IsSet=true` + `ValueField=0` → 値確定状態。例：生まれたての新生児（0歳）は有効な年齢
  - `IsSet=false` + `ValueField=0`（default）→ **未設定状態**。NULL に相当。例：`RespondentAge.Unset()`
  - **重要**: 初期値 0 そのものは区別の根拠ではなく、`IsSet` フラグが判定の根拠
- RespondentAge は常に値を持つため、IsSet は常に true
- IOptionalValueObject を実装する（具体型で static abstract メソッドを提供）
- ValueObject 基礎との整合性を保つため、IsSet プロパティは継承元から取得

---

### 2.2 ValueField プロパティ

年齢に対応する内部値（通常は int）。

| 項目 | 内容 |
|------|------|
| 型 | `protected readonly int` |
| アクセス修飾子 | `protected` |
| セッタ | なし（readonly） |
| デフォルト値 | 派生クラスのコンストラクタで設定 |

**設計判断**

- `protected readonly` で派生クラスからの読み取りを許可し、外部からの変更は禁止
- TValue 型は struct 制約により、値型に限定（int, short, DateTime 等）
- 初期化はコンストラクタの base(value) 呼び出し時に実施

### 2.2b Value プロパティ

年齢値への公開アクセスポイント（get のみ）

| 項目 | 内容 |
|------|------|
| 型 | `public int? { get; }` |
| 戻り値 | IsSet=true なら年齢値、false なら null |
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

```csharp
// Value プロパティ（Domain ロジック）
if (age.Value.HasValue && age.Value < 18)
{
    // 未成年
}

// TryGetValue（外部入力処理）
if (age.TryGetValue(out var ageValue))
{
    ProcessAge(ageValue);
}
```

---

### 2.3 From(int value) 静的メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static RespondentAge From(int value)` |
| 戻り値 | 指定されたint値を持つ RespondentAge インスタンス |
| パラメータ | `value`: 年齢を表す整数値 |
| 例外 | `ArgumentOutOfRangeException`：値が 0～150 範囲外の場合 |
| 用途 | 指定された年齢値を持つ RespondentAge オブジェクトの生成 |

**処理フロー**

1. `new(value, true)` でコンストラクタを呼び出す（IsSet=true）
2. PrimitiveValueObject コンストラクタ内で：
   - `Normalize(value)` を実行（戻り値をそのまま使用）
   - `Validate(normalized)` を実行（ビジネスルール検証）
3. 検証で例外が発生した場合、インスタンス生成は失敗

**設計判断**

- protected コンストラクタを呼び出すため、派生クラスまたは内部のみアクセス可能
- 派生クラスは private コンストラクタで本メソッドを呼び出す
- 例外スロー時はインスタンス生成を中止

---

### 2.4 Unset() 静的メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static RespondentAge Unset()` |
| 戻り値 | 未設定状態の RespondentAge インスタンス |
| パラメータ | なし |
| 例外 | 例外を投げない |
| 用途 | 未設定状態の RespondentAge を表現する |

**処理フロー**

1. `new(false)` でコンストラクタを呼び出す（IsSet=false）
2. PrimitiveValueObject コンストラクタ内で：
   - IsSet = false を設定
   - ValueField = default（0）を設定
   - Validate は実行されない（if (isSet) により skip）

**設計判断**

- 静的インスタンスキャッシュ（UnsetInstance）を使用して、同一の未設定インスタンスを返す
- 等価性判定で常に同じ結果を返す

---

### 2.5 TryFrom(int? input, out RespondentAge result) 静的メソッド（IOptionalValueObject 実装）【重要】

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom(int? input, out RespondentAge result)` |
| 戻り値 | **true** = null 入力（Unset）**または** 有効値 / **false** = 無効値のみ |
| パラメータ | `input`: 年齢を表す nullable int（null 許容） |
| 例外 | 例外を投げない |
| 用途 | API 入力・UI フォーム：null 許容の年齢値を安全に処理 |

**【重要】処理フロー（IOptionalValueObject 仕様）**

```csharp
public static bool TryFrom(int? input, out RespondentAge result)
{
    // ケース 1: null 入力 → Unset + true（正常な未設定）
    if (!input.HasValue)
    {
        result = Unset();
        return true;  // ✅ null は成功扱い
    }

    try
    {
        // ケース 2: 有効な値（0～150） → From + true
        result = From(input.Value);
        return true;  // ✅ 検証成功
    }
    catch (ArgumentOutOfRangeException)
    {
        // ケース 3: 無効な値（-1, 151以上） → Unset + false
        result = Unset();
        return false;  // ❌ 検証失敗のみ失敗
    }
}
```

**【重要】戻り値の解釈**

| 戻り値 | result.IsSet | 意味 |
|------|------------|------|
| **true** | false | null 入力 → 未設定（正常） |
| **true** | true | 有効値（0～150） → 設定済み（正常） |
| **false** | false | 無効値（-1,151以上） → 検証失敗 |

**設計判断**

- ✅ null は「正常な未設定」として扱う（return true）
  - フォーム未入力は有効な入力パターン
- ❌ 値の範囲外は「例外的な未設定」として扱う（return false）
  - 入力値は存在するが、ビジネスルール違反
- out パラメータは常に値を受け取る（失敗時は Unset()）

---

### 2.6 TryFrom(int input, out RespondentAge result) 静的メソッド（オーバーロード）

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom(int input, out RespondentAge result)` |
| 戻り値 | 生成に成功した場合 true、失敗した場合 false |
| パラメータ | `input`: 年齢を表す整数値（null 不許容） |
| 例外 | 例外を投げない |
| 用途 | nullable でない int 値から RespondentAge の生成を試みる |

**処理フロー**

1. `TryFrom((int?)input, out result)` に委譲（オーバーロード）
   - 自動的に `(int?)input` で nullable に変換
   - 上記の nullable メソッドと同じフローで処理

---

### 2.7 TryGetValue(out int value) メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public new bool TryGetValue(out int value)` |
| 戻り値 | IsSet が true の場合 true、false の場合 false |
| パラメータ | `value`: 取得する年齢値の格納先 |
| 例外 | 例外を投げない |
| 用途 | 年齢値を安全に取得する |

**処理フロー**

1. IsSet が false の場合：
   - `value = 0` を設定
   - `return false`
2. IsSet が true の場合：
   - `value = ValueField` を設定
   - `return true`

**設計判断**

- 基底クラス PrimitiveValueObject の メソッドをオーバーライド（`new` キーワード）
- IsSet が false 時の戻り値は 0（default）
- NULL と有効な 0 は等価レベルでは区別されるが、out 値としては両方 `default(0)` を返す

---

## 3. 等価性・ハッシング仕様

### 3.1 GetValueComponents() メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override IEnumerable<object?> GetValueComponents()` |
| 用途 | 等価性判定で使用する値コンポーネントを列挙 |

**処理フロー**

1. IsSet が true の場合のみ：
   - `yield return ValueField` で年齢値を返す
2. IsSet が false の場合：
   - ValueField は返さない（未設定状態は ValueField に依存しない）
3. IsSet フラグは基底クラスで自動的に等価性判定に含まれる

**設計判断**

- IsSet フラグを常に含める（0 vs null 区別の根本）
- IsSet=false + ValueField=0 と IsSet=true + ValueField=0 は異なる（IsSet が判定の根拠）

---

### 3.2 Equals(RespondentAge? other) メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public bool Equals(RespondentAge? other)` |
| 戻り値 | 等価である場合 true、異なる場合 false |
| パラメータ | `other`: 比較対象の RespondentAge |

**処理フロー**

1. `other is null` の場合 → `return false`
2. `ReferenceEquals(this, other)` の場合 → `return true`
3. `IsSet == other.IsSet && ValueField == other.ValueField` の場合 → `return true`
4. その他 → `return false`

---

### 3.3 Equals(object? obj) メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override bool Equals(object? obj)` |
| 戻り値 | 等価である場合 true、異なる場合 false |
| パラメータ | `obj`: 比較対象のオブジェクト |

**処理フロー**

1. `obj as RespondentAge` で RespondentAge にキャスト
2. `Equals(RespondentAge? other)` に委譲

---

### 3.4 GetHashCode() メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override int GetHashCode()` |
| 戻り値 | ハッシュコード（整数値） |

**処理フロー**

1. `HashCode.Combine(IsSet, ValueField)` でハッシュコード生成
   - IsSet と ValueField 両方を含める（0 vs null 区別保証）

---

## 4. メソッド設計

### 4.1 Normalize(int input) メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected virtual int Normalize(int input)` |
| 戻り値 | 正規化済み値 |
| パラメータ | `input`: 入力値 |

**処理フロー**

- デフォルト実装: 入力値をそのまま返す（正規化不要）
- 派生クラスがオーバーライド可能

**設計判断**

- PrimitiveValueObject の基盤メソッド
- 当クラスでは正規化処理は不要（override しない）

---

### 4.2 Validate(int normalized) メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override void Validate(int normalized)` |
| パラメータ | `normalized`: 正規化済み値 |
| 例外 | `ArgumentOutOfRangeException`：値が 0～150 範囲外の場合 |

**処理フロー**

1. 基底クラス `base.Validate(normalized)` を呼び出し（前処理）
2. minAge = 0, maxAge = 150 を定義
3. `normalized < minAge || normalized > maxAge` の場合：
   - `throw new ArgumentOutOfRangeException(nameof(normalized), $"Age must be between {minAge} and {maxAge}.")`

**設計判断**

- ビジネスルール: 年齢は 0～150 歳の範囲に制限
- 基底クラスの検証を優先（chain of responsibility）
- エラーメッセージは英語（国際化対応予定）

---

## 5. 文字列化仕様

### 5.1 ToString() メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override string ToString()` |
| 戻り値 | 文字列表現 |
| 用途 | ログ出力、UI 表示、デバッグ |

**処理フロー**

- 基底クラス PrimitiveValueObject の実装を使用
- IsSet=true の場合: `ValueField.ToString()` → "0", "18", "75" 等
- IsSet=false の場合: "Unset"

---

## 6. 設計判断・注釈

### 6.1 IOptionalValueObject 実装

резpondentAge は UI からの入力が未設定である可能性があるため、IOptionalValueObject を実装する。これにより、Null 安全性と業務ルール（未設定と0歳の区別）を両立させる。

### 6.2 0歳の扱い

0 歳（新生児）は有効な年齢値である。Unset()（未設定値）とは IsSet フラグで区別する。

### 6.3 sealed クラス

派生を禁止（sealed）することで、Validate / Normalize / GetDisplayName の拡張を意図的に制限し、年齢値オブジェクトとしての一貫性を保つ。

---

## 附録

### A. 定数

| 定数 | 値 | 用途 |
|------|-----|------|
| minAge | 0 | 年齢最小値 |
| maxAge | 150 | 年齢最大値 |

---

### B. 関連ドキュメント

- [PrimitiveValueObject 技術仕様書](../../SharedKernel/ValueObjects/PrimitiveValueObject_技術仕様書.md)
- [IOptionalValueObject インターフェース仕様](../../SharedKernel/Interfaces/IOptionalValueObject_技術仕様書.md)
- [ValueObject 基礎設計](../../SharedKernel/ValueObjects/ValueObject_技術仕様書.md)
