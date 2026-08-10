# PermissionCode — 単体テスト仕様書

**対象:** PermissionCode ValueObject  
**版:** 1.0  
**作成日:** 2026-08-10

---

## テストケース一覧

| # | テストケース | 入力 | 期待値 |
|---|---|---|---|
| 1 | From_ValidCode_ReturnsInstance | "Employee.Create" | IsSet=true, Value="Employee.Create" |
| 2 | From_WithMultipleDots_ReturnsInstance | "Employee.Read.Own" | IsSet=true |
| 3 | From_WithUnderscore_ReturnsInstance | "employee_read" | IsSet=true |
| 4 | From_MaxLength_ReturnsInstance | "A"*100 | IsSet=true |
| 5 | From_WithNull_ThrowsArgumentException | null | ArgumentException |
| 6 | From_WithEmptyString_ThrowsArgumentException | "" | ArgumentException |
| 7 | From_With101Characters_ThrowsArgumentException | "A"*101 | ArgumentException |
| 8 | From_WithHyphen_ThrowsArgumentException | "Employee-Create" | ArgumentException |
| 9 | From_WithSpace_ThrowsArgumentException | "Employee Create" | ArgumentException |
| 10 | TryFrom_Valid_ReturnsTrue | "Employee.Create" | true |
| 11 | TryFrom_WithNull_ReturnsFalse | null | false |
| 12 | TryFrom_WithEmpty_ReturnsFalse | "" | false |
| 13 | Equals_SameValues_ReturnsTrue | "Employee.Create", "Employee.Create" | true |
| 14 | Equals_DifferentValues_ReturnsFalse | "Employee.Create", "Employee.Read" | false |
| 15 | Equals_CaseSensitive_ReturnsFalse | "Employee.Create", "employee.create" | false |
| 16 | GetHashCode_SameValues_SameHash | "Employee.Create", "Employee.Create" | hash1 == hash2 |
| 17 | ToString_ReturnsStringValue | "Employee.Create" | "Employee.Create" |

**総テストケース数: 17**

