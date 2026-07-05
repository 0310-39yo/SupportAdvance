# 詳細設計書 — CarModel

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**コンテキスト:** CarPreferences (Samples)  
**対象:** `SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects.CarModel`  
**版:** 1.0 / 2026-07-04

---

## 1. クラス定義

```csharp
public sealed class CarModel 
	: EnumValueObject<int>, 
	  IOptionalValueObject<CarModel, int>, 
	  IEquatable<CarModel>
```

**修飾子の説明**

| 修飾子 | 意味 | 理由 |
|--------|------|------|
| `public` | 他クラスから参照可能 | Domain Entity の プロパティ型として使用 |
| `sealed` | 派生禁止 | 不変性保証、選択肢固定化 |
| `: EnumValueObject<int>` | 基底クラス | 選択肢型値オブジェクトの共通実装 |
| `: IOptionalValueObject<CarModel, int>` | インターフェース | UI null 入力対応、static abstract メソッド実装 |
| `: IEquatable<CarModel>` | インターフェース | 等価性判定最適化 |

---

## 2. 静的フィールド・インスタンス

### 2.1 選択肢インスタンス（public static readonly）

```csharp
/// <summary>不明</summary>
public static readonly CarModel Unknown = new(0);

/// <summary>セダン</summary>
public static readonly CarModel Sedan = new(1);

/// <summary>SUV</summary>
public static readonly CarModel SportUtility = new(2);

/// <summary>ハッチバック</summary>
public static readonly CarModel Hatchback = new(3);

/// <summary>クーペ</summary>
public static readonly CarModel Coupe = new(4);

/// <summary>ワンボックス</summary>
public static readonly CarModel Minivan = new(5);

/// <summary>その他</summary>
public static readonly CarModel Other = new(6);
```

**初期化順序**
- `new(0)` → `private CarModel(int value) : base(value)` → `IsSet=true, ValueField=0, Validate(0)` 実行
- `new(1)` 以降、同様に各値で初期化

### 2.2 Unset インスタンス（private static readonly）

```csharp
/// <summary>未設定状態の CarModel インスタンス</summary>
private static readonly CarModel UnsetInstance = new();
```

**初期化**
- `new()` → `private CarModel() : base()` → `IsSet=false, ValueField=0, Validate() スキップ`

**アクセス方法**
- 外部からは `Unset()` メソッド経由のみ

---

## 3. コンストラクタと初期化フロー

### 3.1 値設定用コンストラクタ

```csharp
private CarModel(int value) : base(value)
{
}
```

**呼び出しフロー**

```
CarModel.From(1)
  ↓
new(1)  // private ctor
  ↓
base(1)  // EnumValueObject(int value)
  ↓
this(1, true)  // EnumValueObject(TValue value, bool isSet)
  ↓
IsSet = true
ValueField = 1
Validate(1)  // ArgumentOutOfRangeException チェック
```

### 3.2 Unset 用コンストラクタ

```csharp
private CarModel() : base()
{
}
```

**呼び出しフロー**

```
CarModel.Unset()
  ↓
UnsetInstance  // キャッシュから返す
  // 初回アクセス時のみ初期化:
  ↓
new()  // private ctor
  ↓
base()  // EnumValueObject()
  ↓
this(default(int), false)  // EnumValueObject(TValue value, bool isSet)
  ↓
IsSet = false
ValueField = 0 (default(int))
// Validate() は実行されない
```

---

## 4. メソッド実装

### 4.1 From メソッド

```csharp
/// <summary>
/// 指定された内部値から CarModel のインスタンスを生成する
/// </summary>
/// <param name="value">内部値</param>
/// <returns>生成された CarModel のインスタンス</returns>
public static CarModel From(int value) => new(value);
```

**エラーハンドリング**

| 入力 | 結果 | 例外 |
|-----|------|------|
| 0 | `Unknown` | なし（有効） |
| 1～6 | 対応インスタンス | なし（有効） |
| -1, 7以上 | スロー | ArgumentOutOfRangeException |

### 4.2 Unset メソッド

```csharp
/// <summary>
/// 未設定状態の CarModel のインスタンスを生成する
/// </summary>
/// <returns>未設定状態の CarModel のインスタンス</returns>
public static CarModel Unset() => UnsetInstance;
```

**特性**

- シングルトン（常に同一インスタンスを返す）
- `IsSet == false`
- `ToString()` → `"Unset"`

### 4.3 TryFrom メソッド（null 許容版）

```csharp
/// <summary>
/// 指定された内部値から CarModel の生成を試みる
/// null の場合は Unset() を返して true を返す（正常処理）
/// 値が無効な場合は Unset() を返して false を返す（エラー処理）
/// </summary>
/// <param name="input">内部値（null許容）</param>
/// <param name="result">生成された CarModel のインスタンス</param>
/// <returns>生成に成功した場合またはnullの場合はtrue、失敗した場合はfalse</returns>
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
```

**フロー図**

```
TryFrom(int? input, out CarModel result)
  │
  ├─ input == null
  │   └─ result = Unset()
  │   └─ return true  ← 正常処理
  │
  ├─ input が 0～6
  │   └─ result = From(value)
  │   └─ return true  ← 正常処理
  │
  └─ input が範囲外（-1, 7以上）
	  └─ result = Unset()
	  └─ return false  ← エラー処理
```

### 4.4 TryFrom メソッド（整数値版、IOptionalValueObject 実装）

```csharp
/// <summary>
/// 指定された内部値から CarModel の生成を試みる（整数値による呼び出し）
/// </summary>
/// <param name="input">内部値</param>
/// <param name="result">生成された CarModel のインスタンス</param>
/// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
public static bool TryFrom(int input, out CarModel result) 
	=> TryFrom((int?)input, out result);
```

**用途**

- `IOptionalValueObject<CarModel, int>.TryFrom(int, out CarModel)` の実装
- `TryFrom(int?, ...)` に委譲

### 4.5 等価性メンバー

```csharp
/// <summary>
/// 指定された CarModel と等価かどうかを判定する
/// </summary>
/// <param name="other">比較対象の CarModel</param>
/// <returns>等価である場合は true、そうでない場合は false</returns>
public bool Equals(CarModel? other)
{
	if (other is null) return false;
	if (ReferenceEquals(this, other)) return true;
	return IsSet == other.IsSet && ValueField == other.ValueField;
}

/// <summary>
/// 指定されたオブジェクトと等価かどうかを判定する
/// </summary>
/// <param name="obj">比較対象のオブジェクト</param>
/// <returns>等価である場合は true、そうでない場合は false</returns>
public override bool Equals(object? obj) => Equals(obj as CarModel);

/// <summary>
/// ハッシュコードを取得する
/// </summary>
/// <returns>オブジェクトのハッシュコード</returns>
public override int GetHashCode() => HashCode.Combine(IsSet, ValueField);
```

**等価性判定ロジック**

```
Equals(CarModel? other)
  │
  ├─ other == null
  │   └─ return false
  │
  ├─ ReferenceEquals(this, other)
  │   └─ return true  ← 参照等価（最適化）
  │
  └─ IsSet == other.IsSet && ValueField == other.ValueField
	  └─ return (結果)  ← 値等価
```

**ハッシュコード生成**

```csharp
HashCode.Combine(IsSet, ValueField)
// IsSet: true/false → 0/1
// ValueField: 0～6
// 例：IsSet=true, ValueField=1 → Combine(true, 1)
```

### 4.6 Validate メソッド（オーバーライド）

```csharp
/// <summary>
/// 指定された内部値の妥当性を検証する
/// </summary>
/// <param name="value">検証対象の内部値</param>
/// <exception cref="ArgumentOutOfRangeException">値が有効な範囲外の場合</exception>
protected override void Validate(int value)
{
	if (value < 0 || value > 6)
	{
		throw new ArgumentOutOfRangeException(nameof(value), 
			$"CarModel must be between 0 and 6, but got {value}");
	}
}
```

**検証ルール**

| 値 | 結果 | 説明 |
|-----|------|------|
| 0 | ✅ 通過 | Unknown（有効） |
| 1～6 | ✅ 通過 | Sedan～Other（有効） |
| -1, 7～ | ❌ 例外 | 範囲外 |

**呼び出しタイミング**

- `new(value)` → `base(value)` → `Validate(value)` ← ここで実行
- `new()` → `base()` ← Validate は実行されない（IsSet=false）
- `Unset()` ← Validate は実行されない（キャッシュから返す）

### 4.7 GetDisplayName メソッド（オーバーライド）

```csharp
/// <summary>
/// 車のモデルの業務名称を返す
/// </summary>
/// <returns>業務名称</returns>
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

**マッピング**（switch 式）

```
ValueField
  ├─ 0 → "不明"
  ├─ 1 → "セダン"
  ├─ 2 → "SUV"
  ├─ 3 → "ハッチバック"
  ├─ 4 → "クーペ"
  ├─ 5 → "ワンボックス"
  ├─ 6 → "その他"
  └─ _ → ArgumentOutOfRangeException
```

**呼び出しタイミング**

- `ToString()` → `IsSet ? GetDisplayName() : "Unset"` ← ここで実行
- Validate により 0～6 の値のみ存在するため、デフォルトケースは理論的に到達不可

---

## 5. 使用シーン別の処理フロー

### 5.1 UI: ユーザーが車種を選択

```
ComboBox.SelectedItem = CarModel.Sedan
  ↓
Application Service で保存処理
  ↓
var model = (CarModel)comboBox.SelectedItem  // Sedan
var preferences = CarPreferences.Create(model)
```

### 5.2 API: ユーザーが「未選択」で送信

```
POST /api/preferences { "carModelId": null }
  ↓
var carModel = CarModel.TryFrom((int?)request.CarModelId, out result)
  // result = CarModel.Unset(), return true
  ↓
if (carModel)
{
	// null 入力は許可（正常系扱い）
	var preferences = CarPreferences.CreateUnset();
}
```

### 5.3 API: ユーザーが無効値で送信

```
POST /api/preferences { "carModelId": 99 }
  ↓
var carModel = CarModel.TryFrom(99, out result)
  // result = CarModel.Unset(), return false
  ↓
if (!carModel)
{
	// 無効値エラー
	throw new ApplicationException("Invalid car model: 99");
}
```

---

## 6. テスト設計（概要）

### 6.1 単位テストカバレッジ

| テスト対象 | テストケース | 期待値 |
|----------|-----------|--------|
| `From(int)` | From(0)～From(6) | 各値でインスタンス生成成功 |
| | From(-1), From(7) | ArgumentOutOfRangeException |
| `Unset()` | Unset() | IsSet=false のインスタンス |
| `TryFrom(int?)` | TryFrom(null) | Unset() + true |
| | TryFrom(0)～TryFrom(6) | 各値 + true |
| | TryFrom(-1), TryFrom(7) | Unset() + false |
| `Equals()` | Sedan.Equals(Sedan) | true |
| | Sedan.Equals(Coupe) | false |
| | Unknown.Equals(Unset()) | false（IsSet 異なる） |
| `GetHashCode()` | Sedan.GetHashCode() | 安定性 |
| `ToString()` | Sedan.ToString() | "セダン" |
| | Unset().ToString() | "Unset" |

---

## 7. 永続化フロー（v1.1 追加）

### DB 保存時のロジック

```csharp
// Application / Infrastructure 層での実装例
var carModel = /* Entity から取得 */;

int? dbValue = carModel.IsSet 
  ? carModel.ValueField  // IsSet=true なら値を保存（例：0, 1, ... 6）
  : null;                 // IsSet=false なら NULL を保存

database.Update(new { CarModelValue = dbValue });
```

### DB 復元時のロジック（将来実装予定）

```csharp
// Infrastructure / Repository 層での実装例
int? dbValue = database.Read("CarModelValue");

var carModel = CarModel.TryFrom(dbValue, out var result)
  ? result
  : throw new InvalidOperationException($"Invalid DB value: {dbValue}");

// 内部フロー
// dbValue = null     → TryFrom(null) → Unset() + true 返却
// dbValue = 0        → TryFrom(0)    → From(0) → Unknown 返却
// dbValue = 1～6     → TryFrom(val)  → From(val) 返却
// dbValue = 無効値   → TryFrom(val)  → Unset() + false 返却（エラー処理へ）
```

### スキーマ例

```sql
-- テーブル定義
CREATE TABLE Drivers (
  Id INT PRIMARY KEY,
  CarModelValue INT NULL,  -- 0～6 または NULL
  ...
);

-- 制約（オプション）
ALTER TABLE Drivers
ADD CONSTRAINT CHK_CarModelValue CHECK (CarModelValue IS NULL OR (CarModelValue BETWEEN 0 AND 6));
```

---

## 8. 他クラスとの依存関係

```
CarModel
  ↓ extends
EnumValueObject<int>
  ↓ extends
ValueObject
  ↓ uses
IEquatable<CarModel>
IOptionalValueObject<CarModel, int>
```

**外部依存**

- `SupportAdvance.SharedKernel.ValueObjects.EnumValueObject<T>`
- `SupportAdvance.SharedKernel.ValueObjects.IOptionalValueObject<TSelf, TValue>`
- `System.IEquatable<T>` (.NET BCL)

---

## 8. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-04 | 加藤 正人 | 初版作成。Unknown（0）を含む 0～6 の実装詳細を記載 |

