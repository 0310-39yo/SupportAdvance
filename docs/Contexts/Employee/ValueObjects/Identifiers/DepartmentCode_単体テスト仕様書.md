# DepartmentCode — 単体テスト仕様書

**対象:** DepartmentCode ValueObject  
**版:** 1.0  
**作成日:** 2026-08-10

---

## テストケース一覧

### グループ1: From メソッド（正系）

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 1 | From_With4Characters_ReturnsInstance | "G100" | IsSet=true, Value="G100" |
| 2 | From_WithDifferentCodes_ReturnsInstance | "D200", "S050" | IsSet=true, Value=入力値 |

### グループ2: From メソッド（異常系）

| # | テストケース | 入力 | 期待動作 |
|---|---|---|---|
| 3 | From_WithNull_ThrowsArgumentException | null | ArgumentException |
| 4 | From_WithEmptyString_ThrowsArgumentException | "" | ArgumentException |
| 5 | From_With3Characters_ThrowsArgumentException | "G10" | ArgumentException |
| 6 | From_With5Characters_ThrowsArgumentException | "G1000" | ArgumentException |
| 7 | From_WithSpecialCharacters_ThrowsArgumentException | "G@00" | ArgumentException |
| 8 | From_WithLowercaseLetters_AcceptsLowercaseButIsStored | "g100" | "g100"（大文字小文字区別） |

### グループ3: TryFrom メソッド

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 9 | TryFrom_With4Characters_ReturnsTrue | "G100" | true, result.Value="G100" |
| 10 | TryFrom_WithNull_ReturnsFalse | null | false |
| 11 | TryFrom_WithEmptyString_ReturnsFalse | "" | false |
| 12 | TryFrom_With3Characters_ReturnsFalse | "G10" | false |
| 13 | TryFrom_WithSpecialCharacters_ReturnsFalse | "G@00" | false |

### グループ4: TryFromDbValue メソッド

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 14 | TryFromDbValue_With4Characters_ReturnsTrue | "G100" | true |
| 15 | TryFromDbValue_WithNull_ReturnsFalse | null | false |

### グループ5: 等価性（Equals）

| # | テストケース | 入力1 | 入力2 | 期待値 |
|---|---|---|---|---|
| 16 | Equals_SameValues_ReturnsTrue | "G100" | "G100" | true |
| 17 | Equals_DifferentValues_ReturnsFalse | "G100" | "D200" | false |
| 18 | Equals_CaseSensitive_ReturnsFalse | "G100" | "g100" | false |
| 19 | Equals_WithNull_ReturnsFalse | "G100" | null | false |

### グループ6: ハッシュコード

| # | テストケース | 入力1 | 入力2 | 期待値 |
|---|---|---|---|---|
| 20 | GetHashCode_SameValues_SameHash | "G100" | "G100" | hash1 == hash2 |
| 21 | GetHashCode_DifferentValues_DifferentHash | "G100" | "D200" | hash1 ≠ hash2 |

### グループ7: ToString

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 22 | ToString_Returns4CharacterString | "G100" | "G100" |

### グループ8: 演算子オーバーロード

| # | テストケース | 入力1 | 入力2 | 期待値 |
|---|---|---|---|
| 23 | OperatorEqual_SameValues_ReturnsTrue | "G100" | "G100" | true |
| 24 | OperatorNotEqual_DifferentValues_ReturnsTrue | "G100" | "D200" | true |

---

## 実装時の確認項目

- [ ] From メソッドで null チェック実装
- [ ] 長さ検証（4文字）実装
- [ ] 文字種検証（英数字のみ）実装
- [ ] TryFrom で例外をキャッチして false を返す
- [ ] Equals/GetHashCode を正しく実装
- [ ] == 演算子をオーバーロード
- [ ] 大文字小文字区別が正しく機能

---

## テスト実行順序

1. グループ1-2（From メソッド）
2. グループ3（TryFrom メソッド）
3. グループ4（TryFromDbValue）
4. グループ5-7（等価性・ハッシュ・文字列化）
5. グループ8（演算子）

**総テストケース数: 24**

