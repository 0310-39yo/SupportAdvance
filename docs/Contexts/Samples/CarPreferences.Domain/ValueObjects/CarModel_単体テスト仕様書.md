# 単体テスト仕様書 — CarModel

**プロジェクト:** SupportAdvance  
**対象:** `SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects.CarModel`  
**テストファイル:** `tests/Contexts/Samples/CarPreferences.Domain.Tests/ValueObjects/CarModelTests.cs`  
**版:** 1.0 / 2026-07-04

---

## 1. テスト対象の位置づけ

`CarModel` は Domain 層の ValueObject であり、**車のモデル（車種）を型安全に表現する選択肢型値オブジェクト**である。

本テスト仕様書は、以下の要件を検証する単位テストの定義である：

- ✅ 7 つの静的フィールド（Unknown～Other）が正しく初期化される
- ✅ Unset インスタンスが IsSet=false で管理される
- ✅ From(int) メソッドが有効値（0～6）を受け入れ、無効値で例外をスローする
- ✅ Unset() メソッドが Unset インスタンスをシングルトンで返す
- ✅ TryFrom(int?, out CarModel) が null と無効値を適切に処理する
- ✅ Validate メソッドが 0～6 の範囲チェックを実施する
- ✅ GetDisplayName メソッドが各値の業務名称を返す
- ✅ ToString() が IsSet に応じて表示名または "Unset" を返す
- ✅ 等価性判定（Equals / GetHashCode）が IsSet と ValueField で実施される
- ✅ IOptionalValueObject インターフェースが正しく実装されている

---

## 2. テスト観点と設計

### 2.1 正常系テスト

**T-001: 静的フィールド初期化**

| テストID | テスト名 | 入力 | 期待値 | 検証項目 |
|---------|--------|------|--------|--------|
| T-001-1 | Unknown フィールド | - | IsSet=true, ValueField=0 | 初期化完了、Validate パス |
| T-001-2 | Sedan フィールド | - | IsSet=true, ValueField=1 | 初期化完了 |
| T-001-3 | SportUtility フィールド | - | IsSet=true, ValueField=2 | 初期化完了 |
| T-001-4 | Hatchback フィールド | - | IsSet=true, ValueField=3 | 初期化完了 |
| T-001-5 | Coupe フィールド | - | IsSet=true, ValueField=4 | 初期化完了 |
| T-001-6 | Minivan フィールド | - | IsSet=true, ValueField=5 | 初期化完了 |
| T-001-7 | Other フィールド | - | IsSet=true, ValueField=6 | 初期化完了 |

**T-002: From メソッド（正常値）**

| テストID | テスト名 | 入力 | 期待値 | 検証項目 |
|---------|--------|------|--------|---------|
| T-002-1 | From(0) | 0 | CarModel インスタンス | IsSet=true, ValueField=0、Sedan と等価 ではなく Unknown と等価 |
| T-002-2 | From(1) | 1 | CarModel インスタンス | IsSet=true, ValueField=1、Sedan と等価 |
| T-002-3 | From(6) | 6 | CarModel インスタンス | IsSet=true, ValueField=6、Other と等価 |

**T-003: Unset メソッド**

| テストID | テスト名 | 入力 | 期待値 | 検証項目 |
|---------|--------|------|--------|---------|
| T-003-1 | Unset() | - | CarModel インスタンス | IsSet=false, ValueField=0（default(int)） |
| T-003-2 | Unset シングルトン | Unset() 複数回呼び出し | 同一インスタンス | ReferenceEquals で true |

**T-004: TryFrom(int?, out CarModel) メソッド**

| テストID | テスト名 | 入力 | 期待値 | 戻り値 |
|---------|--------|------|--------|--------|
| T-004-1 | TryFrom(null) | null | Unset() | true |
| T-004-2 | TryFrom(0) | 0 | From(0) と等価 | true |
| T-004-3 | TryFrom(3) | 3 | From(3) と等価 | true |
| T-004-4 | TryFrom(6) | 6 | From(6) と等価 | true |

**T-005: TryFrom(int, out CarModel) メソッド（IOptionalValueObject 実装）**

| テストID | テスト名 | 入力 | 期待値 | 戻り値 |
|---------|--------|------|--------|--------|
| T-005-1 | TryFrom(1) | 1 | From(1) と等価 | true |
| T-005-2 | TryFrom(6) | 6 | From(6) と等価 | true |

**T-006: GetDisplayName / ToString 正常系**

| テストID | テスト名 | 入力 | 期待値 |
|---------|--------|------|--------|
| T-006-1 | Unknown.ToString() | - | "不明" |
| T-006-2 | Sedan.ToString() | - | "セダン" |
| T-006-3 | SportUtility.ToString() | - | "SUV" |
| T-006-4 | Hatchback.ToString() | - | "ハッチバック" |
| T-006-5 | Coupe.ToString() | - | "クーペ" |
| T-006-6 | Minivan.ToString() | - | "ワンボックス" |
| T-006-7 | Other.ToString() | - | "その他" |
| T-006-8 | Unset().ToString() | - | "Unset" |
| T-006-9 | Unset().TryGetValue(out int) | - | false (IsSet=false のため) |

**T-007: Equals / GetHashCode 正常系**

| テストID | テスト名 | 入力 | 期待値 | 検証項目 |
|---------|--------|------|--------|---------|
| T-007-1 | Sedan.Equals(Sedan) | - | true | 値等価 |
| T-007-2 | From(1).Equals(Sedan) | - | true | 値等価 |
| T-007-3 | Sedan.GetHashCode() == Sedan.GetHashCode() | - | true | ハッシュ同一 |
| T-007-4 | From(1).GetHashCode() == Sedan.GetHashCode() | - | true | ハッシュ同一 |
| T-007-5 | Sedan.Equals(null) | - | false | null 処理 |
| T-007-6 | Sedan.Equals((object)Sedan) | - | true | object キャスト動作 |

---

### 2.2 異常系テスト

**T-008: From メソッド（異常値）**

| テストID | テスト名 | 入力 | 期待値 | 例外 |
|---------|--------|------|--------|------|
| T-008-1 | From(-1) | -1 | - | ArgumentOutOfRangeException |
| T-008-2 | From(7) | 7 | - | ArgumentOutOfRangeException |
| T-008-3 | From(100) | 100 | - | ArgumentOutOfRangeException |

**T-009: TryFrom(int?, out CarModel) メソッド（異常値）**

| テストID | テスト名 | 入力 | 期待値 | 戻り値 |
|---------|--------|------|--------|--------|
| T-009-1 | TryFrom(-1) | -1 | Unset() | false |
| T-009-2 | TryFrom(7) | 7 | Unset() | false |
| T-009-3 | TryFrom(99) | 99 | Unset() | false |

---

### 2.3 等価性・不等価性テスト

**T-010: Equals メソッド（不等価）**

| テストID | テスト名 | 比較 | 期待値 |
|---------|--------|------|--------|
| T-010-1 | Sedan.Equals(Coupe) | Sedan vs Coupe | false |
| T-010-2 | Sedan.Equals(Unset()) | Sedan vs Unset | false（IsSet 異なる） |
| T-010-3 | Unset().Equals(Unset()) | Unset vs Unset | true（同じインスタンス） |
| T-010-4 | Unknown.Equals(Unset()) | Unknown vs Unset | false（IsSet 異なる） |

---

### 2.4 IOptionalValueObject インターフェース実装検証

**T-011: IOptionalValueObject<CarModel, int> メンバー**

| テストID | テスト名 | メンバー | 検証内容 |
|---------|--------|----------|---------|
| T-011-1 | static Unset() が実装される | `Unset()` | IsSet=false のインスタンスを返す |
| T-011-2 | static From(int) が実装される | `From(int)` | IsSet=true のインスタンスを返す |
| T-011-3 | static TryFrom(int?, out CarModel) が実装される | `TryFrom(int?, out)` | null → Unset() + true |
| T-011-4 | static TryFrom(int, out CarModel) が実装される | `TryFrom(int, out)` | IOptionalValueObject コントラクト実装 |
| T-011-5 | instance TryGetValue(out int) が実装される | `TryGetValue(out)` | IsSet=true なら true + 値、IsSet=false なら false |

---

## 3. テスト実装パターン

### 3.1 テストメソッド名規則

```
Test{関数名or対象}_{入力or条件}_{期待値}
```

**例**

```csharp
[Fact]
public void From_ValidValue0_ReturnsUnknownInstance()
{
	// Arrange

	// Act
	var result = CarModel.From(0);

	// Assert
	Assert.True(result.IsSet);
	Assert.Equal(0, result.ValueField);
	Assert.Equal(CarModel.Unknown, result);
}

[Fact]
public void From_InvalidValue7_ThrowsArgumentOutOfRangeException()
{
	// Act & Assert
	var ex = Assert.Throws<ArgumentOutOfRangeException>(() => CarModel.From(7));
	Assert.Contains("0 and 6", ex.Message);
}

[Fact]
public void TryFrom_NullInput_ReturnsUnsetAndTrue()
{
	// Act
	var result = CarModel.TryFrom(null, out var model);

	// Assert
	Assert.True(result);
	Assert.False(model.IsSet);
	Assert.Equal(CarModel.Unset(), model);
}

[Fact]
public void TryFrom_InvalidValue_ReturnsUnsetAndFalse()
{
	// Act
	var result = CarModel.TryFrom(99, out var model);

	// Assert
	Assert.False(result);
	Assert.False(model.IsSet);
}

[Fact]
public void ToString_Sedan_ReturnsJapaneseName()
{
	// Act
	var result = CarModel.Sedan.ToString();

	// Assert
	Assert.Equal("セダン", result);
}

[Fact]
public void ToString_Unset_ReturnsUnset()
{
	// Act
	var result = CarModel.Unset().ToString();

	// Assert
	Assert.Equal("Unset", result);
}

[Fact]
public void Equals_ValuesWithSameValueField_ReturnsTrue()
{
	// Arrange
	var model1 = CarModel.From(1);
	var model2 = CarModel.Sedan;

	// Act & Assert
	Assert.Equal(model1, model2);
}

[Fact]
public void Equals_DifferentIsSetValues_ReturnsFalse()
{
	// Arrange
	var set = CarModel.Unknown;
	var unset = CarModel.Unset();

	// Act & Assert
	Assert.NotEqual(set, unset);
}
```

---

## 4. テストカバレッジ目標

| 項目 | 対象 | 目標 |
|------|------|------|
| **メソッドカバレッジ** | 全 public / protected メソッド | 100% |
| **ブランチカバレッジ** | 制御フローの全分岐 | 100% |
| **行カバレッジ** | 実行可能行 | 100% |

**対象メソッド一覧**

- [ ] `From(int)`
- [ ] `Unset()`
- [ ] `TryFrom(int?, out CarModel)`
- [ ] `TryFrom(int, out CarModel)`
- [ ] `Validate(int)` (protected)
- [ ] `GetDisplayName()` (protected)
- [ ] `Equals(CarModel?)`
- [ ] `Equals(object?)`
- [ ] `GetHashCode()`
- [ ] `ToString()` (inherited from base)
- [ ] `TryGetValue(out int)` (inherited from base)

---

## 5. テストデータ

### 5.1 有効値セット

```csharp
public static readonly TheoryData<int, CarModel> ValidValues = new()
{
	{ 0, CarModel.Unknown },
	{ 1, CarModel.Sedan },
	{ 2, CarModel.SportUtility },
	{ 3, CarModel.Hatchback },
	{ 4, CarModel.Coupe },
	{ 5, CarModel.Minivan },
	{ 6, CarModel.Other },
};
```

### 5.2 無効値セット

```csharp
public static readonly TheoryData<int> InvalidValues = new()
{
	-1,
	-100,
	7,
	8,
	99,
	int.MinValue,
	int.MaxValue,
};
```

### 5.3 表示名マッピング

```csharp
public static readonly TheoryData<int, string> DisplayNameMappings = new()
{
	{ 0, "不明" },
	{ 1, "セダン" },
	{ 2, "SUV" },
	{ 3, "ハッチバック" },
	{ 4, "クーペ" },
	{ 5, "ワンボックス" },
	{ 6, "その他" },
};
```

---

## 5.4 永続化マッピング テスト (v1.1 追加)

**T-012: DB 保存値マッピング**

| テストID | テスト名 | 入力 | 期待値 |
|---------|--------|------|--------|
| T-012-1 | Unknown の DB 値 | Unknown | 0 として保存 |
| T-012-2 | Sedan の DB 値 | Sedan | 1 として保存 |
| T-012-3 | Unset の DB 値 | Unset() | NULL として保存 |
| T-012-4 | IsSet フラグ判定（0 と NULL の区別） | Unknown vs Unset | IsSet で判定後に値/NULL を選択 |

**T-013: DB 復元ロジック（将来テスト対象）**

| テストID | テスト名 | DB 値 | 期待値 | 復元メソッド |
|---------|--------|------|--------|----------|
| T-013-1 | 0 から Unknown 復元 | 0 | Unknown | CarModel.From(0) |
| T-013-2 | NULL から Unset 復元 | NULL | Unset() | CarModel.Unset() |
| T-013-3 | TryFrom(null?) で NULL 復元 | NULL | Unset() + true | TryFrom(null, out) |
| T-013-4 | TryFrom(0) で 0 復元 | 0 | Unknown + true | TryFrom(0, out) |
| T-013-5 | TryFrom(99) で無効値復元 | 99 | Unset() + false | TryFrom(99, out) |

---

## 6. テスト実装位置

**ファイルパス:** `tests/Contexts/Samples/CarPreferences.Domain.Tests/ValueObjects/CarModelTests.cs`

**ネームスペース:** `SupportAdvance.Tests.Contexts.Samples.CarPreferences.Domain.ValueObjects`

**テストフレームワーク:** xUnit

**参照アセンブリ:**
- `SupportAdvance.Contexts.Samples.CarPreferences.Domain`
- `SupportAdvance.SharedKernel`
- `Xunit`

---

## 7. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-04 | 加藤 正人 | 初版作成。Unknown（0）、IOptionalValueObject 実装、Unset シングルトンを含むテスト仕様 |

