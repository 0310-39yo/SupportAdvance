# Employee Context Infrastructure層 - 単体テスト仕様書

**作成日:** 2026-09-16
**改訂日:** 2026-09-26（実在するテストに合わせて全面改訂）
**対象読者:** テスト実装者
**内容:** EmployeeMapper のテストケース一覧

---

## 📋 概要

本書は、Employee Context Infrastructure層の **Mapper の単体テスト仕様** です。Entity ↔ DbModel の変換（型・null・LocalDateTime）を、DB なしで検証します。

`EmployeeRepository` のテストは実 DB（SQL Server）に接続する結合テストのため、[EmployeeRepository_結合テスト仕様書.md](EmployeeRepository_結合テスト仕様書.md) で扱います。`tests/Contexts/Employee.Infrastructure.Tests/Repositories/EmployeeRepositoryTests.cs` がその実装です。

> **改訂の経緯**: 旧版は `PersonMapper`・`PersonRepository` のテストや、`VO_MAP_01_…` 形式のメソッド名を「実装済み」として列挙していたが、いずれも実在しなかった（Person は Employee 集約の一部として `EmployeeMapper` が変換する）。実在するテストに合わせて書き直した。

---

## 🧪 テスト構成

```
tests/Contexts/Employee.Infrastructure.Tests/
├── Employee.Infrastructure.Tests.csproj
├── Mappers/
│   └── EmployeeMapperTests.cs        ← 本書の対象（12 テスト、Theory の展開を含めて 13 ケース）
└── Repositories/
    └── EmployeeRepositoryTests.cs    ← 結合テスト（結合テスト仕様書を参照）
```

`EmployeeMapper` は `IClock` を保持しない（`ToDomainEntity(dbModel, clock)` の引数で受け取る）。テストでは `SystemClock` を渡す。

---

## 1. テスト観点一覧

### EmployeeMapper

観点IDは、この仕様書内の管理番号。テストメソッド名は `TestXxxNN_…` の形式で、観点IDはメソッド名に含まれない。

#### MAP: Entity → DbModel

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| MAP-01 | 正社員の Entity が DbModel に正しく変換される | 正常系 | ✅ EmployeeMapperTests.cs::TestToDbModel01_WithValidEmployeeConvertsCorrectly |
| MAP-02 | 派遣社員の区分が変換される | 正常系 | ✅ EmployeeMapperTests.cs::TestToDbModel02_WithDispatchedEmployeeConvertsCorrectly |
| MAP-03 | 契約社員の区分が変換される | 正常系 | ✅ EmployeeMapperTests.cs::TestToDbModel03_WithContractorEmployeeConvertsCorrectly |

#### MAP: DbModel → Entity

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| MAP-04 | 正社員の DbModel が Entity に正しく変換される | 正常系 | ✅ EmployeeMapperTests.cs::TestToDomainEntity01_WithValidDbModelConvertsCorrectly |
| MAP-05 | 派遣社員の区分が変換される | 正常系 | ✅ EmployeeMapperTests.cs::TestToDomainEntity02_WithDispatchedDivisionConvertsCorrectly |
| MAP-06 | 契約社員の区分が変換される | 正常系 | ✅ EmployeeMapperTests.cs::TestToDomainEntity03_WithContractorDivisionConvertsCorrectly |
| MAP-07 | 不正な区分の DbModel で例外が発生する | 異常系 | ✅ EmployeeMapperTests.cs::TestToDomainEntity04_WithInvalidDivisionThrowsException |
| MAP-08 | Entity → DbModel → Entity の往復で内容が一貫する | 正常系 | ✅ EmployeeMapperTests.cs::TestRoundTrip01_EntityToDbModelToEntityIsConsistent |

#### MAP: 所属の部署名（`DepartmentDisplayName`）

DB の `department_name` は LEFT JOIN で NULL があり得るため、Domain では未設定（`Unset()`）を持つ値オブジェクトで受ける（フェーズ 5）。

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| MAP-09 | DbModel の部署名が Entity の設定済みの部署名になる | 正常系 | ✅ EmployeeMapperTests.cs::TestToDomainEntity05_WithMembershipDepartmentName_ConvertsToSetName |
| MAP-10 | DbModel の部署名が null または空文字なら Unset になる（Theory 2 ケース） | 正常系 | ✅ EmployeeMapperTests.cs::TestToDomainEntity06_WithNullOrEmptyDepartmentName_ConvertsToUnset |
| MAP-11 | Entity の部署名が Unset なら DbModel は null になる | 正常系 | ✅ EmployeeMapperTests.cs::TestToDepartmentMembershipDbModel01_WithUnsetName_ConvertsToNull |
| MAP-12 | Entity の部署名が設定済みなら DbModel に値が入る | 正常系 | ✅ EmployeeMapperTests.cs::TestToDepartmentMembershipDbModel02_WithName_ConvertsToValue |

### 観点として未整備のもの

| 内容 | 状況 |
|------|------|
| 削除済み Entity の `deleted_at` の変換 | Mapper は監査フィールドを扱わない（Repository の責務）ため、Mapper のテスト対象外。結合テスト仕様書で扱う |
| `DateTime` ↔ `LocalDateTime` の変換 | 変換ヘルパー（`DbDateTimeExtensions`）自体のテストは `tests/Infrastructure.Tests` にある |
| Person の Mapper・Repository | 実在しない（Person は Employee 集約の一部） |

---

## 📊 テストケース集計

| コンポーネント | テスト数 |
|----------|---------|
| EmployeeMapper | 12（Theory の展開を含めて 13 ケース） |

---

## 📋 テスト実装状況

- [x] EmployeeMapperTests（12 テスト）
- Repository テストは結合テスト仕様書を参照

---

## 🎓 参考資料

- [EmployeeRepository_結合テスト仕様書.md](EmployeeRepository_結合テスト仕様書.md) - Repository の結合テスト
- [Employee.Application テスト](../Application/Application_単体テスト仕様書.md) - Application テスト参考
- [Mapper_パターンガイド](../../../Assistance/Guides/Mapper_パターンガイド.md) - Mapper の責務（純粋な型変換）
