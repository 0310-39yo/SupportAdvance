# 単体テスト仕様書 — RespondentAge

**プロジェクト:** SupportAdvance  
**対象:** RespondentAge クラス  
**テスト対象層:** Domain 層（値オブジェクト）  
**テストフレームワーク:** xUnit.net  
**版:** 1.0 / 2026-07-04

---

## 1. テスト対象範囲

| 項目 | 対象 | 理由 |
|------|------|------|
| 静的ファクトリメソッド | ✅ From, Unset, TryFrom | インスタンス生成ロジック |
| インスタンスメソッド | ✅ TryGetValue, Equals, GetHashCode, ToString | 値の取得・等価性・文字列化 |
| 検証ロジック | ✅ Validate（0～150範囲） | ビジネスルール |
| 等価性判定 | ✅ IsSet + ValueField ベース | 0 vs null 区別 |
| 例外処理 | ✅ ArgumentOutOfRangeException | エラーハンドリング |
| 未設定状態 | ✅ Unset() シングルトン | IOptionalValueObject 対応 |

---

## 2. テスト設計方針

### 2.1 テスト分類

| 分類 | テスト項目数 | 用途 |
|------|----------|------|
| 正常系（有効値） | 複数 | 0歳、中間値、最大値150歳のテスト |
| 異常系（範囲外） | 複数 | 負数、151歳以上のテスト |
| 未設定状態 | 複数 | Unset() とその等価性 |
| null 対応 | 複数 | TryFrom(null) での正常処理 |
| 等価性 | 複数 | 同値判定、非等値判定 |
| ハッシング | 複数 | コレクション動作検証 |
| 文字列化 | 複数 | ToString の出力検証 |

### 2.2 テスト対象値の選択

```
有効な年齢値:
  - 0 (境界値：最小)
  - 1 (通常値)
  - 25 (代表値)
  - 50 (代表値)
  - 150 (境界値：最大)

無効な年齢値:
  - -1 (範囲外：負数)
  - 151 (範囲外：超過)
  - 999 (範囲外：大幅超過)

未設定・null:
  - null (nullable int)
  - Unset() (未設定インスタンス)
```

---

## 3. テストケース仕様

### 3.1 From メソッドテスト

#### 3.1.1 正常系：有効な年齢値

**テスト項目:** From_WithValidAge_CreatesInstance

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| 入力値 0（境界値最小） | `From(0)` | インスタンス生成成功、IsSet=true, ValueField=0 |
| 入力値 1 | `From(1)` | インスタンス生成成功、IsSet=true, ValueField=1 |
| 入力値 25（代表値） | `From(25)` | インスタンス生成成功、IsSet=true, ValueField=25 |
| 入力値 150（境界値最大） | `From(150)` | インスタンス生成成功、IsSet=true, ValueField=150 |

**実施方法**

複数の入力値に対して From メソッドを呼び出し、以下を検証：
- インスタンスが生成されること
- IsSet プロパティが true であること
- ValueField プロパティが入力値を保持すること
- ToString の戻り値が入力値の文字列表現であること

#### 3.1.2 異常系：無効な年齢値

**テスト項目:** From_WithInvalidAge_ThrowsArgumentOutOfRangeException

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| 入力値 -1 | `From(-1)` | ArgumentOutOfRangeException 発生 |
| 入力値 151 | `From(151)` | ArgumentOutOfRangeException 発生 |
| 入力値 999 | `From(999)` | ArgumentOutOfRangeException 発生 |

**実施方法**

Assert.Throws<ArgumentOutOfRangeException>() で例外を検証

---

### 3.2 Unset メソッドテスト

#### 3.2.1 未設定インスタンス生成

**テスト項目:** Unset_CreatesUnsetInstance

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| 基本動作 | `Unset()` | インスタンス生成成功、IsSet=false, ValueField=0 |
| 複数呼び出し | `Unset()` 複数回呼び出し | 等価なインスタンスを返す（Equals で確認） |
| ToString | `Unset().ToString()` | "Unset" |

**実施方法**

- Unset() を複数回呼び出し、等価なインスタンスが返されることを確認
- Equals で等価性を検証

---

### 3.3 TryFrom(int?) メソッドテスト

#### 3.3.1 正常系：有効な値

**テスト項目:** TryFrom_WithValidValue_ReturnsTrue

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| 入力値 0 | `TryFrom(0, out result)` | return true, result.IsSet=true, result.ValueField=0 |
| 入力値 25 | `TryFrom(25, out result)` | return true, result.IsSet=true, result.ValueField=25 |
| 入力値 150 | `TryFrom(150, out result)` | return true, result.IsSet=true, result.ValueField=150 |

#### 3.3.2 null 対応

**テスト項目:** TryFrom_WithNullInput_ReturnsUnsetAndTrue

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| null 入力 | `TryFrom(null, out result)` | return true, result = Unset() と等価 |

**実施方法**

- TryFrom(null, out result) で true を返すこと
- result が Unset() と等価であること（IsSet=false）

#### 3.3.3 異常系：無効な値

**テスト項目:** TryFrom_WithInvalidValue_ReturnsFalseAndUnset

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| 入力値 -1 | `TryFrom(-1, out result)` | return false, result = Unset() と等価 |
| 入力値 151 | `TryFrom(151, out result)` | return false, result = Unset() と等価 |

---

### 3.4 TryFrom(int) メソッドテスト（非nullable版）

#### 3.4.1 正常系・異常系

**テスト項目:** TryFrom_NonNullable_DelegatesTryFromNullable

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| 有効値 | `TryFrom(25, out result)` | return true, result.IsSet=true |
| 無効値 | `TryFrom(-1, out result)` | return false, result = Unset() |

**実施方法**

- nullable版と同じ動作を確認（委譲性を検証）

---

### 3.5 TryGetValue メソッドテスト

#### 3.5.1 IsSet=true の場合

**テスト項目:** TryGetValue_WhenSet_ReturnsTrueWithValue

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| From(0) | `From(0).TryGetValue(out value)` | return true, value=0 |
| From(25) | `From(25).TryGetValue(out value)` | return true, value=25 |
| From(150) | `From(150).TryGetValue(out value)` | return true, value=150 |

#### 3.5.2 IsSet=false の場合

**テスト項目:** TryGetValue_WhenUnset_ReturnsFalseWithDefault

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| Unset() | `Unset().TryGetValue(out value)` | return false, value=0 |

---

### 3.6 等価性テスト

#### 3.6.1 同値判定

**テスト項目:** Equals_WithSameValues_ReturnsTrue

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| From(0) == From(0) | 同じ値 | true |
| From(25) == From(25) | 同じ値 | true |
| Unset() == Unset() | 両方未設定 | true（シングルトン） |

#### 3.6.2 非等値判定

**テスト項目:** Equals_WithDifferentValues_ReturnsFalse

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| From(0) != From(25) | 異なる値 | false |
| From(0) != Unset() | 有効と未設定 | false（IsSetが異なる） |
| From(25) != null | null との比較 | false |

#### 3.6.3 参照同一性

**テスト項目:** Equals_WithSameReference_ReturnsTrue

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| 自己比較 | `instance.Equals(instance)` | true |

---

### 3.7 ハッシッティングテスト

#### 3.7.1 同値オブジェクトのハッシュコード一致

**テスト項目:** GetHashCode_WithSameValues_ReturnsSameHashCode

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| From(0) | `From(0).GetHashCode() == From(0).GetHashCode()` | true |
| From(25) | `From(25).GetHashCode() == From(25).GetHashCode()` | true |

#### 3.7.2 コレクション動作

**テスト項目:** GetHashCode_CanBeUsedInDictionary

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| Dictionary | `Dictionary<RespondentAge, string>` でキー使用 | 正常に動作 |
| HashSet | `HashSet<RespondentAge>` での使用 | 正常に動作 |

---

### 3.8 文字列化テスト

#### 3.8.1 ToString（有効値）

**テスト項目:** ToString_WhenSet_ReturnsValueString

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| From(0) | `ToString()` | "0" |
| From(25) | `ToString()` | "25" |
| From(150) | `ToString()` | "150" |

#### 3.8.2 ToString（未設定）

**テスト項目:** ToString_WhenUnset_ReturnsUnsetString

| テスト観点 | 観点内容 | 期待結果 |
|---------|---------|---------|
| Unset() | `ToString()` | "Unset" |

---

## 4. テストカバレッジ要件

| メンバー | テストケース数 | カバレッジ % |
|---------|-----------|----------|
| From(int) | 5 | 100% （正常：4, 異常：3） |
| Unset() | 3 | 100% |
| TryFrom(int?) | 6 | 100% （正常：3, null:1, 異常：2） |
| TryFrom(int) | 2 | 100% （委譲で確認） |
| TryGetValue(out int) | 4 | 100% （IsSet=true:3, IsSet=false:1） |
| Equals(RespondentAge?) | 5 | 100% |
| Equals(object?) | 3 | 100% |
| GetHashCode() | 3 | 100% |
| ToString() | 5 | 100% |
| **合計** | **36+** | **100%** |

---

## 5. テスト実装ガイドライン

### 5.1 テスト命名規則

```
[Method]_[Scenario]_[Expected]
From_WithValidAge_CreatesInstance
From_WithInvalidAge_ThrowsArgumentOutOfRangeException
TryFrom_WithNullInput_ReturnsUnsetAndTrue
```

### 5.2 AAA パターン（Arrange-Act-Assert）

```csharp
[Fact]
public void From_WithValidAge_CreatesInstance()
{
	// Arrange
	const int age = 25;

	// Act
	var result = RespondentAge.From(age);

	// Assert
	Assert.NotNull(result);
	Assert.True(result.IsSet);
	Assert.Equal(age, result.ValueField);
}
```

### 5.3 Theory パターン（複数データ）

```csharp
[Theory]
[InlineData(0)]
[InlineData(25)]
[InlineData(150)]
public void From_WithValidAge_CreatesInstance(int age)
{
	var result = RespondentAge.From(age);
	Assert.Equal(age, result.ValueField);
}
```

### 5.4 例外検証

```csharp
[Fact]
public void From_WithNegativeAge_ThrowsArgumentOutOfRangeException()
{
	Assert.Throws<ArgumentOutOfRangeException>(() => RespondentAge.From(-1));
}
```

---

## 6. テスト実行方法

### 6.1 ローカル実行

```bash
dotnet test tests/Contexts/Samples/CarPreferences.Domain.Tests/
```

### 6.2 CI/CD 実行

- テスト実行結果が 100% 成功であることを確認
- カバレッジレポートを生成・確認

---

## 7. テスト依存関係

| 依存対象 | 用途 |
|---------|------|
| xUnit.net | テストフレームワーク |
| Xunit.Assert | アサーションメソッド |
| RespondentAge | テスト対象クラス |
| PrimitiveValueObject<int> | 基底クラス |

---

## 8. テスト完了条件

- ✅ すべてのテストケースが実行される
- ✅ すべてのテストが green（合格）
- ✅ コードカバレッジ 100%
- ✅ 実装とドキュメント（仕様書）の齟齬がない

---

## 附録 A. テストデータ一覧

| カテゴリ | 値 | 用途 |
|---------|-----|------|
| 有効値（最小） | 0 | 境界値テスト |
| 有効値（代表） | 1, 25, 50, 75, 100, 125 | 通常ケーステスト |
| 有効値（最大） | 150 | 境界値テスト |
| 無効値（負） | -1, -100 | 範囲外テスト |
| 無効値（超過） | 151, 200, 999 | 範囲外テスト |
| null | null | null 安全性テスト |

---

## 附録 B. 関連ドキュメント

- [RespondentAge 技術仕様書](RespondentAge_技術仕様書.md)
- [RespondentAge 詳細設計書](RespondentAge_詳細設計書.md)
- [CarModel 単体テスト仕様書](CarModel_単体テスト仕様書.md) — 参考設計
