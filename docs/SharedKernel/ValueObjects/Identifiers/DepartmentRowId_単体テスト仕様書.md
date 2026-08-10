# DepartmentRowId — 単体テスト仕様書

**対象:** DepartmentRowId ValueObject  
**版:** 1.0  
**作成日:** 2026-08-10

---

## テストケース一覧

### グループ1: From メソッド（正系）

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 1 | From_With1_ReturnsInstance | 1 | IsSet=true, Value=1 |
| 2 | From_WithLargeValue_ReturnsInstance | 9999 | IsSet=true, Value=9999 |

### グループ2: From メソッド（異常系）

| # | テストケース | 入力 | 期待動作 |
|---|---|---|---|
| 3 | From_WithZero_ThrowsArgumentOutOfRangeException | 0 | ArgumentOutOfRangeException |
| 4 | From_WithNegativeValue_ThrowsArgumentOutOfRangeException | -1 | ArgumentOutOfRangeException |

### グループ3: TryFrom メソッド

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 5 | TryFrom_With1_ReturnsTrue | 1 | true, result.Value=1 |
| 6 | TryFrom_WithZero_ReturnsFalse | 0 | false |
| 7 | TryFrom_WithNegative_ReturnsFalse | -1 | false |

### グループ4: TryFromDbValue メソッド

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 8 | TryFromDbValue_With1_ReturnsTrue | 1 | true |
| 9 | TryFromDbValue_WithZero_ReturnsFalse | 0 | false |

### グループ5: 等価性（Equals）

| # | テストケース | 入力1 | 入力2 | 期待値 |
|---|---|---|---|---|
| 10 | Equals_SameValues_ReturnsTrue | 1 | 1 | true |
| 11 | Equals_DifferentValues_ReturnsFalse | 1 | 2 | false |
| 12 | Equals_WithNull_ReturnsFalse | 1 | null | false |

### グループ6: ハッシュコード

| # | テストケース | 入力1 | 入力2 | 期待値 |
|---|---|---|---|---|
| 13 | GetHashCode_SameValues_SameHash | 1 | 1 | hash1 == hash2 |
| 14 | GetHashCode_DifferentValues_DifferentHash | 1 | 2 | hash1 ≠ hash2 |

### グループ7: ToString

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 15 | ToString_ReturnsLongString | 1 | "1" |

### グループ8: 演算子オーバーロード

| # | テストケース | 入力1 | 入力2 | 期待値 |
|---|---|---|---|
| 16 | OperatorEqual_SameValues_ReturnsTrue | 1 | 1 | true |
| 17 | OperatorNotEqual_DifferentValues_ReturnsTrue | 1 | 2 | true |

---

## 実装時の確認項目

- [ ] From メソッドで範囲チェック（1以上）実装
- [ ] TryFrom で例外をキャッチして false を返す
- [ ] Equals/GetHashCode を正しく実装
- [ ] == 演算子をオーバーロード
- [ ] IsSet は常に true

---

## テスト実行順序

1. グループ1-2（From メソッド）
2. グループ3（TryFrom メソッド）
3. グループ4（TryFromDbValue）
4. グループ5-7（等価性・ハッシュ・文字列化）
5. グループ8（演算子）

**総テストケース数: 17**

