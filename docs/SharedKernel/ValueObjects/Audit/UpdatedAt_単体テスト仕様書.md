# UpdatedAt 単体テスト仕様書

**バージョン:** 1.0  
**作成日:** 2025年  

## 1. テスト対象範囲

### 1.1 テスト対象

- `UpdatedAt.From(DateTime value)` メソッド
- `UpdatedAt.TryFrom(DateTime? input, out UpdatedAt result)` メソッド
- `UpdatedAt.TryFrom(DateTime input, out UpdatedAt result)` メソッド
- `UpdatedAt.Equals(object? obj)` メソッド
- `UpdatedAt.Equals(UpdatedAt? other)` メソッド
- `UpdatedAt.GetHashCode()` メソッド
- `UpdatedAt.ToString()` メソッド
- `UpdatedAt.Value` プロパティ

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
| From_WithValidDateTime_CreatesInstance | DateTime(2025,1,1) | UpdatedAtインスタンス生成成功 |
| From_WithPastDate_CreatesInstance | DateTime(2000,1,1) | UpdatedAtインスタンス生成成功 |
| From_WithUtcNow_CreatesInstance | DateTime.UtcNow | UpdatedAtインスタンス生成成功 |
| From_WithLocalNow_CreatesInstance | DateTime.Now | UpdatedAtインスタンス生成成功 |
| From_WithTodayMidnight_CreatesInstance | DateTime(2025,1,1,0,0,0) | UpdatedAtインスタンス生成成功 |
| From_WithTodayLastSecond_CreatesInstance | DateTime(2025,1,1,23,59,59) | UpdatedAtインスタンス生成成功 |

#### 2.1.2 異常系: 無効な日時入力

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| From_WithMinValue_ThrowsArgumentException | DateTime.MinValue | ArgumentException |
| From_WithMaxValue_ThrowsArgumentException | DateTime.MaxValue | ArgumentException |

#### 2.1.3 Value プロパティテスト

| テスト項目 | 操作 | 期待結果 |
|---------|-----|--------|
| Value_ReturnsStoredDateTime | 指定DateTime取得 | 格納されたDateTime値を返す |
| Value_IsReadOnly | Valueへの再代入試行 | コンパイルエラー（get-only） |

### 2.2 TryFrom (nullable) メソッドテスト

#### 2.2.1 正常系: 有効な日時入力

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| TryFrom_Nullable_WithValidDateTime_ReturnsTrue | DateTime(2025,1,1) | true返却、resultに有効なインスタンス |
| TryFrom_Nullable_WithPastDate_ReturnsTrue | DateTime(2000,1,1) | true返却、resultに有効なインスタンス |
| TryFrom_Nullable_WithUtcNow_ReturnsTrue | DateTime.UtcNow | true返却、resultに有効なインスタンス |

#### 2.2.2 null 入力

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| TryFrom_Nullable_WithNull_ReturnsFalse | null | false返却、resultはnull |

#### 2.2.3 異常系: 無効な日時入力

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| TryFrom_Nullable_WithMinValue_ReturnsFalse | DateTime.MinValue | false返却、resultはnull |
| TryFrom_Nullable_WithMaxValue_ReturnsFalse | DateTime.MaxValue | false返却、resultはnull |
| TryFrom_Nullable_WithInvalidDateTime_ReturnsFalse | DateTime.MinValue | false返却、resultはnull |

### 2.3 TryFrom (non-nullable) メソッドテスト

| テスト項目 | 入力値 | 期待結果 |
|---------|-------|--------|
| TryFrom_NonNullable_WithValidDateTime_ReturnsTrue | DateTime(2025,1,1) | true返却、resultに有効なインスタンス |
| TryFrom_NonNullable_WithMinValue_ReturnsFalse | DateTime.MinValue | false返却、resultはnull |
| TryFrom_NonNullable_WithMaxValue_ReturnsFalse | DateTime.MaxValue | false返却、resultはnull |

### 2.4 Equals (object?) メソッドテスト

| テスト項目 | 操作 | 期待結果 |
|---------|-----|--------|
| Equals_Object_WithSameDateTime_ReturnsTrue | 同じDateTime値を比較 | true |
| Equals_Object_WithDifferentDateTime_ReturnsFalse | 異なるDateTime値を比較 | false |
| Equals_Object_WithNull_ReturnsFalse | null | false |
| Equals_Object_WithDifferentType_ReturnsFalse | 異なる型を比較 | false |

### 2.5 Equals (UpdatedAt?) メソッドテスト

| テスト項目 | 操作 | 期待結果 |
|---------|-----|--------|
| Equals_UpdatedAt_WithSameDateTime_ReturnsTrue | 同じDateTime値を比較 | true |
| Equals_UpdatedAt_WithDifferentDateTime_ReturnsFalse | 異なるDateTime値を比較 | false |
| Equals_UpdatedAt_WithNull_ReturnsFalse | null | false |
| Equals_SameReference_ReturnsTrue | 同じインスタンス参照 | true |

### 2.6 GetHashCode メソッドテスト

| テスト項目 | 操作 | 期待結果 |
|---------|-----|--------|
| GetHashCode_WithSameDateTime_ReturnsSameHash | 同じDateTime値 | ハッシュコード一致 |
| GetHashCode_CanBeUsedInDictionary | Dictionary.Add | 正常に格納 |
| GetHashCode_CanBeUsedInHashSet | HashSet.Add | 正常に格納、重複排除 |
| GetHashCode_WithMultipleDates_DifferentHashes | 異なるDateTime値 | 異なるハッシュ（通常） |

### 2.7 ToString メソッドテスト

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
- DateTime(9999, 12, 31, 23, 59, 59)  // MaxValue より前の最後の秒
- DateTime(0001, 1, 1, 0, 0, 1)       // MinValue より後の最初の秒
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
- Equals(UpdatedAt?): 2分岐（null/参照同一性チェック）

### 4.2 行カバレッジ

**目標:** 100%

- すべてのコード行を少なくとも1回は実行
- すべての分岐を少なくとも1回は実行

## 5. 実装テスト数

**目標:** 41 テストケース

| テストグループ | テスト数 | 備考 |
|-------------|--------|------|
| From テスト | 8 | 正常系6 + 異常系2 |
| TryFrom (nullable) テスト | 8 | 正常系3 + null1 + 異常系2 |
| TryFrom (non-nullable) テスト | 3 | 正常系2 + 異常系1 |
| Equals テスト | 9 | object?:4, UpdatedAt?:4, 参照:1 |
| GetHashCode テスト | 5 | 同値:1, Dictionary:1, HashSet:1, 異値:1, 複数値:1 |
| ToString テスト | 2 | ISO形式:2 |
| 値の不変性テスト | 2 | Valueプロパティ:2 |
| ValueObject 不変性確認テスト | 1 | イミュータブル確認 |
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

## 8. 境界値テスト詳細

### 8.1 最小値周辺

| テスト項目 | 値 | 結果 |
|---------|-----|------|
| DateTime.MinValue | 0001-01-01T00:00:00 | ArgumentException |
| MinValue + 1秒 | 0001-01-01T00:00:01 | 成功 |

### 8.2 最大値周辺

| テスト項目 | 値 | 結果 |
|---------|-----|------|
| DateTime.MaxValue | 9999-12-31T23:59:59... | ArgumentException |
| MaxValue - 1秒 | 9999-12-31T23:59:58 | 成功 |

## 9. DateTimeKind テスト詳細

| Kind | テスト内容 | 期待結果 |
|-----|---------|---------|
| Unspecified | 生成・値取得 | 正常、Kind 保持 |
| Utc | 生成・値取得 | 正常、Kind 保持 |
| Local | 生成・値取得 | 正常、Kind 保持 |

## 10. テスト実行時の注意点

- DateTime.Now / DateTime.UtcNow は実行時に計算されるため、範囲チェックを使用
- 複数インスタンステストでは参照同一性（ReferenceEquals）と値同一性（Equals）を区別
- HashSet テストでは同じ値の追加が拒否されることを確認

## まとめ

UpdatedAt のテスト設計要点：

✅ **完全カバレッジ**: 41 テストケースで全コードパス網羅  
✅ **境界値テスト**: MinValue/MaxValue の以降/以前をテスト  
✅ **等価性検証**: 値による等価性が正確に実装されていることを確認  
✅ **不変性確認**: ValueObject としての不変性を実装時に検証  
✅ **スレッドセーフ**: 並行処理環境での安全性を考慮  
✅ **DDD 準拠**: ValueObject パターンの完全実装を検証
