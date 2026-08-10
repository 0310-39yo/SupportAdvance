# RoleCode — 単体テスト仕様書

**対象:** RoleCode ValueObject  
**版:** 1.0  
**作成日:** 2026-08-10

---

## テストケース一覧

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 1 | From_ValidCode_ReturnsInstance | "ADMIN" | IsSet=true, Value="ADMIN" |
| 2 | From_WithUnderscore_ReturnsInstance | "user_viewer" | IsSet=true |
| 3 | From_WithDot_ReturnsInstance | "admin.role" | IsSet=true |
| 4 | From_WithNull_ThrowsArgumentException | null | ArgumentException |
| 5 | From_WithEmptyString_ThrowsArgumentException | "" | ArgumentException |
| 6 | From_With51Characters_ThrowsArgumentException | "A"*51 | ArgumentException |
| 7 | From_WithHyphen_ThrowsArgumentException | "A-D" | ArgumentException |
| 8 | From_WithSpace_ThrowsArgumentException | "A D" | ArgumentException |
| 9 | TryFrom_Valid_ReturnsTrue | "ADMIN" | true |
| 10 | TryFrom_WithNull_ReturnsFalse | null | false |
| 11 | TryFrom_WithEmpty_ReturnsFalse | "" | false |
| 12 | Equals_SameValues_ReturnsTrue | "ADMIN", "ADMIN" | true |
| 13 | Equals_DifferentValues_ReturnsFalse | "ADMIN", "VIEWER" | false |
| 14 | Equals_CaseSensitive_ReturnsFalse | "ADMIN", "admin" | false |
| 15 | GetHashCode_SameValues_SameHash | "ADMIN", "ADMIN" | hash1 == hash2 |
| 16 | ToString_ReturnsStringValue | "ADMIN" | "ADMIN" |

**総テストケース数: 16**

