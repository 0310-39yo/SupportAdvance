# ManagerEmployeeRowId — 単体テスト仕様書

**対象:** ManagerEmployeeRowId ValueObject  
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
| 3 | From_WithZero_ThrowsArgumentException | 0 | ArgumentException |
| 4 | From_WithNegative_ThrowsArgumentException | -1 | ArgumentException |

### グループ3: Unset メソッド

| # | テストケース | 期待値 |
|---|---|---|
| 5 | Unset_ReturnsInstanceWithIsSetFalse | IsSet=false, Value=null |

### グループ4: TryFrom メソッド

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 6 | TryFrom_With1_ReturnsTrue | 1 | true, IsSet=true |
| 7 | TryFrom_WithNull_ReturnsTrueAndUnset | null | true, IsSet=false（null吸収） |
| 8 | TryFrom_WithZero_ReturnsFalse | 0 | false |
| 9 | TryFrom_WithNegative_ReturnsFalse | -1 | false |

### グループ5: TryFromDbValue メソッド

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 10 | TryFromDbValue_With1_ReturnsTrue | 1 | true |
| 11 | TryFromDbValue_WithNull_ReturnsTrueAndUnset | null | true, IsSet=false |

### グループ6: HasManager プロパティ

| # | テストケース | 期待値 |
|---|---|---|
| 12 | HasManager_WithValue_ReturnsTrue | true |
| 13 | HasManager_WithUnset_ReturnsFalse | false |

### グループ7: 等価性（Equals）

| # | テストケース | 入力1 | 入力2 | 期待値 |
|---|---|---|---|---|
| 14 | Equals_SameValues_ReturnsTrue | 1 | 1 | true |
| 15 | Equals_DifferentValues_ReturnsFalse | 1 | 2 | false |
| 16 | Equals_BothUnset_ReturnsTrue | Unset | Unset | true |
| 17 | Equals_DifferentIsSet_ReturnsFalse | 1 | Unset | false |

### グループ8: ハッシュコード

| # | テストケース | 入力1 | 入力2 | 期待値 |
|---|---|---|---|---|
| 18 | GetHashCode_SameValues_SameHash | 1 | 1 | hash1 == hash2 |
| 19 | GetHashCode_DifferentIsSet_DifferentHash | 1 | Unset | hash1 ≠ hash2 |

### グループ9: ToString

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 20 | ToString_WithValue_ReturnsStringRepresentation | 1 | "1" |
| 21 | ToString_WithUnset_ReturnsUnset | Unset | "Unset" |

---

## 実装時の確認項目

- [ ] From メソッドで 0以下 チェック実装
- [ ] Unset() メソッドで IsSet=false を返す
- [ ] TryFrom で null は Unset に変換して true を返す（失敗ではない）
- [ ] TryFromDbValue で DB NULL も Unset に変換
- [ ] HasManager プロパティで IsSet を返す
- [ ] Equals で IsSet と Value の両方を比較
- [ ] ToString で "Unset" または 値の文字列表現を返す

---

## テスト実行順序

1. グループ1-2（From メソッド）
2. グループ3（Unset メソッド）
3. グループ4-5（TryFrom/TryFromDbValue）
4. グループ6（HasManager プロパティ）
5. グループ7-9（等価性・ハッシュ・文字列化）

**総テストケース数: 21**

