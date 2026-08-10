# HierarchyLevel — 単体テスト仕様書

**対象:** HierarchyLevel ValueObject  
**版:** 1.0  
**作成日:** 2026-08-10

---

## テストケース一覧

### グループ1: From メソッド（正系）

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 1 | From_With0_ReturnsCompanyLevel | 0 | IsSet=true, Value=0 |
| 2 | From_With1_ReturnsDivisionLevel | 1 | IsSet=true, Value=1 |
| 3 | From_With2_ReturnsDepartmentLevel | 2 | IsSet=true, Value=2 |
| 4 | From_With3_ReturnsGroupLevel | 3 | IsSet=true, Value=3 |
| 5 | From_With4_ReturnsTeamLevel | 4 | IsSet=true, Value=4 |

### グループ2: From メソッド（異常系）

| # | テストケース | 入力 | 期待動作 |
|---|---|---|---|
| 6 | From_WithNegative_ThrowsArgumentOutOfRangeException | -1 | ArgumentOutOfRangeException |
| 7 | From_With5_ThrowsArgumentOutOfRangeException | 5 | ArgumentOutOfRangeException |

### グループ3: TryFrom メソッド

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 8 | TryFrom_With0_ReturnsTrue | 0 | true, result.Value=0 |
| 9 | TryFrom_With4_ReturnsTrue | 4 | true, result.Value=4 |
| 10 | TryFrom_WithNegative_ReturnsFalse | -1 | false |
| 11 | TryFrom_With5_ReturnsFalse | 5 | false |

### グループ4: TryFromDbValue メソッド

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 12 | TryFromDbValue_With0_ReturnsTrue | 0 | true |
| 13 | TryFromDbValue_WithNegative_ReturnsFalse | -1 | false |

### グループ5: ファクトリメソッド

| # | テストケース | 期待値 |
|---|---|---|
| 14 | Company_ReturnsLevel0 | Value=0, ToString()="Company" |
| 15 | Division_ReturnsLevel1 | Value=1, ToString()="Division" |
| 16 | Department_ReturnsLevel2 | Value=2, ToString()="Department" |
| 17 | Group_ReturnsLevel3 | Value=3, ToString()="Group" |
| 18 | Team_ReturnsLevel4 | Value=4, ToString()="Team" |

### グループ6: ToString（表示名）

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 19 | ToString_With0_ReturnsCompany | 0 | "Company" |
| 20 | ToString_With1_ReturnsDivision | 1 | "Division" |
| 21 | ToString_With2_ReturnsDepartment | 2 | "Department" |
| 22 | ToString_With3_ReturnsGroup | 3 | "Group" |
| 23 | ToString_With4_ReturnsTeam | 4 | "Team" |

### グループ7: 等価性（Equals）

| # | テストケース | 入力1 | 入力2 | 期待値 |
|---|---|---|---|---|
| 24 | Equals_SameValues_ReturnsTrue | 0 | 0 | true |
| 25 | Equals_DifferentValues_ReturnsFalse | 0 | 1 | false |

### グループ8: 演算子オーバーロード

| # | テストケース | 入力1 | 入力2 | 期待値 |
|---|---|---|---|
| 26 | OperatorEqual_SameValues_ReturnsTrue | 0 | 0 | true |
| 27 | OperatorNotEqual_DifferentValues_ReturnsTrue | 0 | 1 | true |

---

## 実装時の確認項目

- [ ] From メソッドで範囲チェック（0-4）実装
- [ ] TryFrom で例外をキャッチして false を返す
- [ ] GetDisplayName() で各レベルの日本語名を返す
- [ ] ファクトリメソッド（Company, Division など）実装
- [ ] Equals/GetHashCode を正しく実装
- [ ] == 演算子をオーバーロード
- [ ] IsSet は常に true

---

## テスト実行順序

1. グループ1-2（From メソッド）
2. グループ3（TryFrom メソッド）
3. グループ4（TryFromDbValue）
4. グループ5（ファクトリメソッド）
5. グループ6（ToString）
6. グループ7-8（等価性・演算子）

**総テストケース数: 27**

