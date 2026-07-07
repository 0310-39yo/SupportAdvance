# 技術仕様書 — CarModel

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**コンテキスト:** CarPreferences (Samples)  
**種別:** EnumValueObject 派生クラス  
**版:** 1.0 / 2026-07-04

---

## 1. 位置づけ

`CarModel` は SupportAdvance プロジェクトの CarPreferences Context に属する **車のモデル（車種）を表す ValueObject** である。

本クラスの目的は以下に集約される：

- 車のモデルを「固定選択肢（Unknown, Sedan, SUV, ハッチバック, クーペ, ワンボックス, その他）」として型安全に表現する
- UI からの null 入力（未選択状態）に備え、`IOptionalValueObject` を実装して Unset 状態をサポートする
- 内部値と業務名称（日本語表示名）を双方向にマッピングする

> **🖊 原則**  
> Domain 層の ValueObject は null を許容しない。  
> `CarModel` は UI からの null 入力に備え、`IOptionalValueObject<CarModel, int>` を実装し Unset 状態をサポートする。  
> 基底クラス `EnumValueObject<int>` は Unset 定義を提供し、派生クラスが static abstract メソッド（`From`, `Unset`, `TryFrom`）を実装する。

---

## 2. メンバー仕様

### 2.1 静的フィールド

**選択肢インスタンス**（IsSet=true）

| フィールド | 値 | 表示名 | 用途 |
|-----------|-----|-------|------|
| `Unknown` | 0 | 不明 | 有効な選択肢だが「不明」の意味。UI では「選択肢不明」として表示。|
| `Sedan` | 1 | セダン | 標準的な乗用車 |
| `SportUtility` | 2 | SUV | スポーツ / ユーティリティ車両 |
| `Hatchback` | 3 | ハッチバック | 後部ハッチ型 |
| `Coupe` | 4 | クーペ | スポーツタイプ / 2ドア | 
| `Minivan` | 5 | ワンボックス | ファミリー向け / 廃版は `Minivan` でなく `Minivan` の呼称 |
| `Other` | 6 | その他 | その他の車種 |

**Unset インスタンス**（IsSet=false）

| フィールド | IsSet | ValueField | 用途 |
|-----------|--------|-----------|------|
| `UnsetInstance` | false | 0（default（int）） | UI の未選択状態。エラーハンドリング時に返す。Validate() は実行されない。 |

**設計判断**

- 静的フィールドは `public static readonly` で提供し、外部から安全にアクセス可能
- `UnsetInstance` は `private static readonly` で隠蔽し、`Unset()` メソッド経由のみで取得
- 有効値範囲は **0～6**（Unknown=0 を含む）

---

### 2.2 コンストラクタ

**値設定用コンストラクタ（private）**

| シグネチャ | 説明 |
|----------|------|
| `private CarModel(int value) : base(value)` | 指定された内部値から CarModel を生成。Validate により 0～6 の範囲チェックが入る。IsSet=true となる。 |

**Unset 用コンストラクタ（private）**

| シグネチャ | 説明 |
|----------|------|
| `private CarModel() : base()` | Unset インスタンス生成専用。IsSet=false, ValueField=0。Validate は実行されない。 |

**設計判断**

- コンストラクタはすべて `private` で、派生クラスからのみアクセス可能
- 外部からは `From()`, `Unset()`, `TryFrom()` を経由して生成

---

### 2.3 From メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static CarModel From(int value)` |
| 戻り値 | 指定値に対応する CarModel インスタンス |
| 例外 | `value` が 0～6 の範囲外の場合、`ArgumentOutOfRangeException` をスロー |
| 用途 | Application / Presentation 層が、知っている内部値から CarModel を直接生成。null 入力は想定しない。 |

**実装例**
```csharp
public static CarModel From(int value) => new(value);
```

---

### 2.4 Unset メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static CarModel Unset()` |
| 戻り値 | IsSet=false, ValueField=0 の CarModel インスタンス（UnsetInstance） |
| 例外 | 例外をスローしない |
| 用途 | UI の未選択状態、またはエラーハンドリング時に呼び出され、Unset 状態を返す。 |

**実装例**
```csharp
public static CarModel Unset() => UnsetInstance;
```

**【重要】設計判断：シングルトン化**

```csharp
private static readonly CarModel UnsetInstance = new();  // 唯一のインスタンス

public static CarModel Unset() => UnsetInstance;  // 常に同じインスタンスを返す
```

**シングルトン化の理由:**
- ✅ **選択肢が限定** — CarModel は限定された選択肢型（Unknown～Other）
- ✅ **メモリ効率** — Unset は頻繁に生成されるため、シングルトン化で最適化
- ✅ **参照の安定性** — 常に同じインスタンスで参照比較の一貫性を保証
- ✅ **UI 処理での効率** — フォーム入力の未選択状態は常に同じ Unset

**振る舞い:**
- `TryGetValue()` は `false` を返す（IsSet=false）
- `ToString()` は `"Unset"` を返す
- 参照比較 `obj1 == obj2` でも常に true（同一インスタンス）

**対比：PrimitiveValueObject との違い**
- RespondentName など PrimitiveValueObject の Unset は毎回新規生成
  - 理由：スカラ値は無限の可能性があり、参照比較は不要
  - 値等価性で判定されるため、シングルトン化の効果が限定的

---

### 2.5 TryFrom メソッド（IOptionalValueObject 実装）【重要】

**【重要】仕様：IOptionalValueObject コントラクト**

null 入力は「未選択」として安全に処理（エラーではなく正常な未設定状態）

**null 許容バージョン**

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom(int? input, out CarModel result)` |
| 戻り値 | **true** = null 入力（Unset）**または** 有効値 / **false** = 無効値のみ |
| 処理 | null → Unset + **true**（正常） / 有効値 → From + **true** / 無効値 → Unset + **false** |

**整数値バージョン（IOptionalValueObject 要件）**

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom(int input, out CarModel result)` |
| 戻り値 | `TryFrom((int?)input, out result)` に委譲 |

**戻り値の解釈:**

| 戻り値 | result | 意味 |
|------|--------|------|
| **true** | Unset（IsSet=false） | null 入力 → 未選択（正常） |
| **true** | 設定済み（IsSet=true） | 有効値（0～6）→ 選択済み（正常） |
| **false** | Unset（IsSet=false） | 無効値（-1,7以上） → 検証失敗 |

**実装例**
```csharp
public static bool TryFrom(int? input, out CarModel result)
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
		return false;  // 無効値はエラー
	}
}

public static bool TryFrom(int input, out CarModel result) => TryFrom((int?)input, out result);
```

**設計判断**

- `IOptionalValueObject<CarModel, int>` のコントラクト実装
- null → `Unset() + true`（正常処理）
- 無効値 → `Unset() + false`（エラー処理）
- UI 入力の null や範囲外値を安全にハンドル

---

### 2.6 Validate メソッド（オーバーライド）

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override void Validate(int value)` |
| 検証ルール | `value` が 0～6 の範囲内であることを確認。範囲外の場合、`ArgumentOutOfRangeException` をスロー。 |
| 例外メッセージ | `"CarModel must be between 0 and 6, but got {value}"` |

**実装例**
```csharp
protected override void Validate(int value)
{
	if (value < 0 || value > 6)
	{
		throw new ArgumentOutOfRangeException(nameof(value), 
			$"CarModel must be between 0 and 6, but got {value}");
	}
}
```

**設計判断**

- Unknown（0）を有効な選択肢として許可
- Validate は `IsSet==true` のコンストラクタ呼び出し時のみ実行
- Unset インスタンス（`IsSet==false`）は Validate をスキップ

---

### 2.7 GetDisplayName メソッド（オーバーライド）

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override string GetDisplayName()` |
| 戻り値 | `ValueField` に対応する業務名称（日本語表示名） |
| 用途 | `ToString()` から呼び出され、UI 層での直接表示を実現 |

**マッピング表**

| ValueField | 業務名称 |
|-----------|---------|
| 0 | "不明" |
| 1 | "セダン" |
| 2 | "SUV" |
| 3 | "ハッチバック" |
| 4 | "クーペ" |
| 5 | "ワンボックス" |
| 6 | "その他" |

**実装例**
```csharp
protected override string GetDisplayName() => ValueField switch
{
	0 => "不明",
	1 => "セダン",
	2 => "SUV",
	3 => "ハッチバック",
	4 => "クーペ",
	5 => "ワンボックス",
	6 => "その他",
	_ => throw new ArgumentOutOfRangeException(nameof(ValueField), 
		$"Unknown car model: {ValueField}")
};
```

**設計判断**

- `IsSet==true` のインスタンスのみで呼び出される（Unset は `ToString()` で `"Unset"` を返す）
- ローカライズ対応を想定し、日本語名称を使用
- switch 式で漏れなくカバー

---

### 2.8 等価性メンバー

| メンバー | 内容 |
|---------|------|
| `Equals(CarModel?)` | 指定の `CarModel` と等価か判定。`IsSet` と `ValueField` で比較。 |
| `Equals(object?)` | 指定のオブジェクトを `CarModel?` にキャストして `Equals(CarModel?)` に委譲 |
| `GetHashCode()` | `HashCode.Combine(IsSet, ValueField)` で生成 |

**設計判断**

- ValueObject 基底の `GetEqualityComponents()` をカスタマイズせず、`IsSet` と `ValueField` に基づく自動実装
- `Unknown` と `Unset` は異なる（`Unknown: IsSet=true, ValueField=0` vs `Unset: IsSet=false, ValueField=0`）
- 参照等価ではなく値等価で比較

---

## 3. 使用例

### 3.1 確定値から生成

```csharp
// 既知の値から直接生成
var sedan = CarModel.From(1);
Console.WriteLine(sedan);  // "セダン"

// 静的フィールドを直接使用
var suv = CarModel.SportUtility;
Console.WriteLine(suv);  // "SUV"

// UI の undefined / null から試験的に生成
if (CarModel.TryFrom(userInputValue, out var model))
{
	// 生成成功（null または有効値）
	Console.WriteLine(model);
}
else
{
	// 無効値
	Console.WriteLine($"Invalid car model: {userInputValue}");
}
```

### 3.2 Unset 状態の扱い

```csharp
// Unset 状態を明示的に生成
var unset = CarModel.Unset();
Console.WriteLine(unset.IsSet);  // false
Console.WriteLine(unset);  // "Unset"

// Unset な値を試験的に取得
if (unset.TryGetValue(out int value))
{
	// IsSet=false なので到達しない
}

// UI: 初期状態は Unset で表示
carModelComboBox.SelectedItem = CarModel.Unset();
```

### 3.3 Application 層の使用例

```csharp
// API リクエストボディから null 入力を受け取る
public class CarPreferencesRequest
{
	public int? CarModelId { get; set; }
}

// Application Service
public class SetCarPreferencesUseCase
{
	public void Execute(CarPreferencesRequest request)
	{
		// TryFrom で安全に変換
		if (!CarModel.TryFrom(request.CarModelId, out var carModel))
		{
			throw new DomainException("Invalid car model");
		}

		// carModel は null 不可（Unset または有効値）
		// Domain ロジックは null チェック不要
		var preferences = CarPreferences.Create(carModel);

		// 保存等...
	}
}
```

---

## 4. 設計制約・禁止事項

### 派生クラスの定義

- ❌ `CarModel` をさらに派生させることは禁止（`sealed` 修飾子）
- ✅ `IOptionalValueObject<CarModel, int>` は既に実装済み

### コンストラクタ

- ❌ 外部から直接コンストラクタを呼び出すことは不可（`private`）
- ✅ `From()`, `Unset()`, `TryFrom()` を経由して生成

### Validate / GetDisplayName

- ❌ 派生クラスが再実装することは不可（基底で override 済み）

### 値の変更

- ❌ 生成後の値変更は不可（不変性）
- ✅ 必要に応じて新しい `CarModel` インスタンスを生成

---

## 5. 永続化ポリシー（v1.1 追加）

### DB マッピング原則

`CarModel` の永続化は、**IsSet フラグで 0（Unknown） と Unset を区別する**。

| 概念 | IsSet | ValueField | DB 保存値 | 復元ロジック |
|-----|-------|-----------|---------|----------|
| **Unknown（確定）** | true | 0 | `0` として保存 | DB値 0 → From(0) → Unknown |
| **Unset（未設定）** | false | 0 | `NULL` として保存 | NULL → Unset() |
| **Sedan など** | true | 1～6 | `1～6` 保存 | FROM(value) で復元 |

### 実装例

```csharp
// Code 側
var unknown = CarModel.Unknown;  // ValueField=0, IsSet=true  
var unset = CarModel.Unset();    // ValueField=0, IsSet=false

// DB 永続化
unknown → 0          # DB に 数値 0 を保存
unset   → NULL       # DB に NULL を保存

// DB 復元ロジック（将来実装予定）
driver.CarModel = input switch
{
  null => CarModel.Unset(),
  0    => CarModel.Unknown,
  1-6  => CarModel.From(input),
  _    => throw new InvalidOperationException()
};
```

### スキーマ要件

- **DB カラム型:** `INT NULL` （nullable int を許容）
- **制約:** 値が存在する場合は 0～6 の範囲に限定
- **デフォルト:** アプリから NULL を明示的に設定（DB デフォルトは不要）

---

## 6. レイヤ制約

**Domain 層コード（CarPreferences.Domain）**

- `CarModel` は Domain Entity / Aggregate から自由に参照可能
- null チェック不要（Unset または確定値のいずれか）

**Application 層コード（CarPreferences.Application）**

- null 入力を `TryFrom()` で `CarModel` に変換
- 変換失敗時は Application Exception をスロー

**Presentation 層コード（WPF / Web）**

- `ToString()` で表示名を直接 UI に表示可能
- ComboBox 等のデータソースに静的フィールドの配列を設定

---

## 6. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-04 | 加藤 正人 | 初版作成。Unknown（0）を含む 0～6 の有効値範囲、IOptionalValueObject 実装による Unset サポート |

