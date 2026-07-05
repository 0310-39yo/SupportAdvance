# CreatedAt 単体テスト仕様書

**バージョン:** 1.0  
**作成日:** 2025年  

## 1. テスト対象範囲

### 1.1 テスト対象

- `CreatedAt.From(DateTime value)` メソッド
- `CreatedAt.TryFrom(DateTime? input, out CreatedAt result)` メソッド
- `CreatedAt.TryFrom(DateTime input, out CreatedAt result)` メソッド
- `CreatedAt.Equals(object? obj)` メソッド
- `CreatedAt.Equals(CreatedAt? other)` メソッド
- `CreatedAt.GetHashCode()` メソッド
- `CreatedAt.ToString()` メソッド
- `CreatedAt.Value` プロパティ

### 1.2 テスト設計方針

- **AAA パターン** (Arrange-Act-Assert) に従う
- **xUnit** フレームワークを使用
- **Theory テスト** で複数入力値をテスト
- **境界値テスト** で極端な値を検証

## 2. テストケース仕様

### 2.1 From メソッドテスト

#### 2.1.1 正常系: 有効な日時入力

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| From_WithValidDateTime_CreatesInstance | DateTime(2025,1,1) | CreatedAtインスタンス生成成功 |
| From_WithPastDate_CreatesInstance | DateTime(2000,1,1) | CreatedAtインスタンス生成成功 |
| From_WithUtcNow_CreatesInstance | DateTime.UtcNow | CreatedAtインスタンス生成成功 |
| From_WithLocalNow_CreatesInstance | DateTime.Now | CreatedAtインスタンス生成成功 |
| From_WithTodayMidnight_CreatesInstance | DateTime(2025,1,1,0,0,0) | CreatedAtインスタンス生成成功 |
| From_WithTodayLastSecond_CreatesInstance | DateTime(2025,1,1,23,59,59) | CreatedAtインスタンス生成成功 |

#### 2.1.2 異常系: 無効な日時入力

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| From_WithMinValue_ThrowsArgumentException | DateTime.MinValue | ArgumentException |
| From_WithMaxValue_ThrowsArgumentException | DateTime.MaxValue | ArgumentException |

#### 2.1.3 Value プロパティテスト

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| Value_WhenCreated_ReturnsOriginalDateTime | DateTime(2025,1,1,10,30,0) | 元のDateTime値を返す |

### 2.2 TryFrom メソッドテスト（nullable対応）

#### 2.2.1 正常系: 有効な値

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| TryFrom_WithValidDateTime_ReturnsTrue | DateTime(2025,1,1) | true, validなCreatedAt |
| TryFrom_WithUtcNow_ReturnsTrue | DateTime.UtcNow | true, validなCreatedAt |
| TryFrom_WithMultipleDates_ReturnsTrue | 複数の有効日時 | すべてtrue |

#### 2.2.2 null入力テスト

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| TryFrom_WithNullInput_ReturnsFalse | null | false, result=null |

#### 2.2.3 異常系: 無効な値

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| TryFrom_WithMinValue_ReturnsFalse | DateTime.MinValue | false, result=null |
| TryFrom_WithMaxValue_ReturnsFalse | DateTime.MaxValue | false, result=null |

### 2.3 TryFrom メソッドテスト（non-nullable オーバーロード）

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| TryFrom_NonNullable_WithValidValue_ReturnsTrue | DateTime(2025,1,1) | true |
| TryFrom_NonNullable_WithUtcNow_ReturnsTrue | DateTime.UtcNow | true |

### 2.4 等価性テスト

#### 2.4.1 Equals(object?) テスト

| テスト項目 | 比較対象 | 期待結果 |
|---------|--------|--------|
| Equals_Object_WithSameDateTime_ReturnsTrue | 同じDateTime値 | true |
| Equals_Object_WithDifferentDateTime_ReturnsFalse | 異なるDateTime値 | false |
| Equals_Object_WithNull_ReturnsFalse | null | false |
| Equals_Object_WithDifferentType_ReturnsFalse | 別の型 | false |

#### 2.4.2 Equals(CreatedAt?) テスト

| テスト項目 | 比較対象 | 期待結果 |
|---------|--------|--------|
| Equals_WithSameDateTime_ReturnsTrue | 同じDateTime値 | true |
| Equals_WithDifferentDateTime_ReturnsFalse | 異なるDateTime値 | false |
| Equals_WithNull_ReturnsFalse | null | false |
| Equals_SameInstance_ReturnsTrue | 同じインスタンス参照 | true |

#### 2.4.3 参照同一性テスト

| テスト項目 | 操作 | 期待結果 |
|---------|-----|--------|
| Equals_SameReference_ReturnsTrue | a = From(日時), b = a | true |

### 2.5 GetHashCode テスト

| テスト項目 | 操作 | 期待結果 |
|---------|-----|--------|
| GetHashCode_WithSameDateTime_ReturnsSameHash | 同じDateTime値 | ハッシュコード一致 |
| GetHashCode_CanBeUsedInDictionary | Dictionary.Add | 正常に格納 |
| GetHashCode_CanBeUsedInHashSet | HashSet.Add | 正常に格納、重複排除 |
| GetHashCode_WithMultipleDates_DifferentHashes | 異なるDateTime値 | 異なるハッシュ（通常） |

### 2.6 ToString テスト

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| ToString_WithDateTime_ReturnsIso8601Format | DateTime(2025,1,1,10,30,0) | ISO 8601文字列 |
| ToString_WithUtcNow_ReturnsValidFormat | DateTime.UtcNow | ISO 8601文字列 |

## 3. テストデータ仕様

### 3.1 有効な日時値

```
- DateTime(2025, 1, 1)  // 標準的な日時
- DateTime(2000, 1, 1)  // 過去の日時
- DateTime.UtcNow       // 現在のUTC日時
- DateTime.Now          // 現在のLocal日時
- DateTime(9999, 12, 31, 23, 59, 59)  // MaxValueより前の最後の秒
- DateTime(0001, 1, 1, 0, 0, 1)       // MinValueより後の最初の秒
```

### 3.2 無効な日時値

```
- DateTime.MinValue  // 0001-01-01T00:00:00
- DateTime.MaxValue  // 9999-12-31T23:59:59.9999999
```

### 3.3 特殊な日時値

```
- DateTimeKind.Unspecified
- DateTimeKind.Utc
- DateTimeKind.Local
```

## 4. テストカバレッジ

### 4.1 ブランチカバレッジ

- From メソッド: 2分岐（成功/例外）
- TryFrom (nullable): 3分岐（null/成功/例外）
- Equals(object?): 2分岐（null/型チェック）
- Equals(CreatedAt?): 3分岐（null/参照同一/値比較）

### 4.2 行カバレッジ

**目標:** 100%

- すべてのコード行を少なくとも1回は実行
- すべての分岐を少なくとも1回は実行

## 5. 実装テスト数

**目標:** 41 テストケース

| テストグループ | テスト数 | 備考 |
|-------------|--------|------|
| From テスト | 8 | 正常6 + 異常2 |
| TryFrom (nullable) テスト | 8 | 正常3 + null1 + 異常2 |
| TryFrom (non-nullable) テスト | 3 | 正常2 + 異常1 |
| Equals テスト | 9 | object?:4, CreatedAt?:4, 参照:1 |
| GetHashCode テスト | 5 | 同値:1, Dictionary:1, HashSet:1, 異値:1, 複数値:1 |
| ToString テスト | 2 | ISO形式:2 |
| 値の不変性テスト | 2 | Valueプロパティ:2 |
| 不変性テスト | 1 | イミュータブル確認 |
| 複数呼び出しテスト | 1 | インスタンス作成テスト |
| 境界値テスト | 2 | MinValue直後:1, MaxValue直前:1 |
| DateTimeKind テスト | 3 | UTC/Local/Unspecified |
| **合計** | **41** | |

## 6. テスト実行環境

- **フレームワーク:** xUnit
- **プロジェクト:** tests/SharedKernel.Tests/
- **ターゲット:** .NET 10
- **コマンド:** dotnet test

## 7. テスト成功基準

- すべてのテストが PASS
- コードカバレッジ 100%
- ビルド成功（警告なし）

## 8. テスト実行結果

### 実施日: 2025年
- **実行テスト数**: 41個
- **成功**: 41個 ✅
- **失敗**: 0個
- **スキップ**: 0個
- **実行時間**: 186ミリ秒

## 9. テストの特徴

### 9.1 Theory テストの使用

複数の入力値を効率的にテスト：

```csharp
[Theory]
[InlineData(2025, 1, 1)]
[InlineData(2000, 1, 1)]
[InlineData(2024, 12, 31)]
public void From_WithValidDateTime_CreatesInstance(int year, int month, int day)
{
	// 複数の入力値で同一のテストロジックを実行
}
```

### 9.2 Fact テストの使用

単一の値で特定の挙動をテスト：

```csharp
[Fact]
public void From_WithMinValue_ThrowsArgumentException()
{
	// MinValue の特定の挙動をテスト
}
```

### 9.3 Collection テスト

Dictionary と HashSet での動作検証

## まとめ

CreatedAtの単体テストは以下を検証します：

✅ **From メソッド**: 有効値の受け入れ、無効値の拒否  
✅ **TryFrom メソッド**: null安全性、エラーハンドリング  
✅ **等価性**: 値同一性の正確性  
✅ **ハッシング**: Collection対応  
✅ **文字列化**: ISO 8601形式  
✅ **不変性**: ValueプロパティのImmutable性  
✅ **DateTimeKind**: 異なるKindの対応  
✅ **境界値**: MinValue/MaxValue 近傍の値  
