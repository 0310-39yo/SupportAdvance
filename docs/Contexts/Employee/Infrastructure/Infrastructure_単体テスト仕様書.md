# Employee Context Infrastructure層 - 単体テスト仕様書

**作成日:** 2026-09-16  
**対象読者:** テスト実装者  
**内容:** Mapper・Repository のテストケース一覧

---

## 📋 概要

本書は、Employee Context Infrastructure層の **単体テスト仕様** です。

各 Mapper および Repository のテストケースを定義し、DB マッピング・永続化ロジックを検証します。

---

## 🧪 テスト構成

### テストプロジェクト構造

```
tests/Contexts/Employee.Infrastructure.Tests/
├── Employee.Infrastructure.Tests.csproj
├── Mappers/
│   ├── EmployeeMapperTests.cs
│   └── PersonMapperTests.cs
├── Repositories/
│   ├── EmployeeRepositoryTests.cs
│   └── PersonRepositoryTests.cs
└── Fixtures/
    └── InfrastructureFixture.cs
```

### テスト用 Mock/Stub

| 役割 | 実装 | 用途 |
|------|------|------|
| IDbConnectionFactory | InMemoryDbConnectionFactory | In-memory SQLite |
| IClock | FixedClock | 固定時刻テスト |

---

## 1. テスト観点一覧

### EmployeeMapper

#### VO-MAP: マッピング（Entity ↔ DbModel）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-MAP-01 | Entity → DbModel で全フィールドが正しくマップされる | 正常系 | ✅ EmployeeMapperTests.cs::VO_MAP_01_ToDbModel_WithValidEntity_MapsAllFields |
| VO-MAP-02 | 削除済み Entity で deleted_at がマップされる | 正常系 | ✅ EmployeeMapperTests.cs::VO_MAP_02_ToDbModel_WithDeletedEntity_SetsDeletedAt |
| VO-MAP-03 | DbModel → Entity で全フィールドが正しくマップされる | 正常系 | ✅ EmployeeMapperTests.cs::VO_MAP_03_ToDomainEntity_WithValidDbModel_MapsAllFields |
| VO-MAP-04 | deleted_at が有る場合 IsDeleted が true に設定される | 正常系 | ✅ EmployeeMapperTests.cs::VO_MAP_04_ToDomainEntity_WithDeletedAt_CreatesDeletedState |

#### VO-TYPE: 型変換（DateTime ↔ LocalDateTime）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-TYPE-01 | DateTime → LocalDateTime 変換が正確である | 正常系 | ✅ EmployeeMapperTests.cs::VO_TYPE_01_DateTimeConversion_ToLocalDateTime_Correct |
| VO-TYPE-02 | LocalDateTime → DateTime → LocalDateTime の往復変換が一貫性を保つ | 正常系 | ✅ EmployeeMapperTests.cs::VO_TYPE_02_DateTimeConversion_ToDateTime_RoundTrip |
| VO-TYPE-03 | null フィールドが正しく処理される | 正常系 | ✅ EmployeeMapperTests.cs::VO_TYPE_03_ToDbModel_WithNullableFields_HandlesCorrectly |
| VO-TYPE-04 | null DbModel フィールドが未設定状態に変換される | 正常系 | ✅ EmployeeMapperTests.cs::VO_TYPE_04_ToDomainEntity_WithNullableFields_ConvertsProperly |

---

### PersonMapper

#### VO-MAP: マッピング（Entity ↔ DbModel）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-MAP-01 | Entity → DbModel で全フィールドが正しくマップされる | 正常系 | ✅ PersonMapperTests.cs::VO_MAP_01_ToDbModel_WithValidEntity_MapsAllFields |
| VO-MAP-02 | DbModel → Entity で全フィールドが正しくマップされる | 正常系 | ✅ PersonMapperTests.cs::VO_MAP_02_ToDomainEntity_WithValidDbModel_MapsAllFields |

#### VO-TYPE: 型変換（名前フィールド・日時）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-TYPE-01 | 名前フィールド（LastName/FirstName/Kana）が正しくマップされる | 正常系 | ✅ PersonMapperTests.cs::VO_TYPE_01_NameFieldConversion_WithKanaAndKanjiFields |
| VO-TYPE-02 | 日時フィールドの往復変換が一貫性を保つ | 正常系 | ✅ PersonMapperTests.cs::VO_TYPE_02_DateTimeConversion_ConsistentAfterRoundTrip |

---

### EmployeeRepository

#### VO-CRUD: CRUD 操作

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-CRUD-01 | SaveAsync で Entity が DB に保存される | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_CRUD_01_SaveAsync_WithValidEntity_PersistsToDb |
| VO-CRUD-02 | 複数 Entity を SaveAsync できる | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_CRUD_02_SaveAsync_WithMultipleEntities_AllPersisted |
| VO-CRUD-03 | GetByIdAsync で存在する ID から Entity が返される | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_CRUD_03_GetByIdAsync_WithValidId_ReturnsEntity |
| VO-CRUD-04 | 存在しない ID で null が返される | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_CRUD_04_GetByIdAsync_WithNonExistentId_ReturnsNull |
| VO-CRUD-05 | UpdateAsync で Entity が DB で更新される | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_CRUD_05_UpdateAsync_WithValidEntity_UpdatesDb |
| VO-CRUD-06 | DeleteAsync で Entity が論理削除される | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_CRUD_06_DeleteAsync_WithValidId_LogicallyDeletes |
| VO-CRUD-07 | DeleteAsync 後 GetByIdAsync で null が返される | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_CRUD_07_DeleteThenGet_CannotRetrieveDeleted |

#### VO-AUDIT: 監査フィールド

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-AUDIT-01 | SaveAsync で created_at と created_by が自動設定される | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_AUDIT_01_SaveAsync_SetsAuditFields_CreatedAt_CreatedBy |
| VO-AUDIT-02 | UpdateAsync で updated_at が更新される | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_AUDIT_02_UpdateAsync_SetsAuditFields_UpdatedAt |
| VO-AUDIT-03 | DeleteAsync で deleted_by が設定される | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_AUDIT_03_DeleteAsync_SetsDeletedBy_FromClock |

#### VO-TXRX: トランザクション・例外処理

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-TXRX-01 | 存在しない Entity を UpdateAsync すると例外が発生する | 異常系 | ✅ EmployeeRepositoryTests.cs::VO_TXRX_01_UpdateAsync_WithNonExistentEntity_ThrowsException |
| VO-TXRX-02 | Row version 不一致で OptimisticLockException が発生する | 異常系 | ✅ EmployeeRepositoryTests.cs::VO_TXRX_02_UpdateAsync_WithRowVersionConflict_ThrowsException |
| VO-TXRX-03 | 存在しない ID を DeleteAsync すると例外が発生する | 異常系 | ✅ EmployeeRepositoryTests.cs::VO_TXRX_03_DeleteAsync_WithNonExistentId_ThrowsException |

#### VO-INTEG: 統合テスト（Save → Get → Update → Delete）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-INTEG-01 | SaveAsync → GetByIdAsync でラウンドトリップが成功する | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_INTEG_01_SaveThenGet_RoundTrip_RetrievesCorrectly |
| VO-INTEG-02 | UpdateAsync → GetByIdAsync で変更が反映される | 正常系 | ✅ EmployeeRepositoryTests.cs::VO_INTEG_02_UpdateThenGet_ReflectsChanges |

---

### PersonRepository

#### VO-CRUD: CRUD 操作

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-CRUD-01 | SaveAsync で Entity が DB に保存される | 正常系 | ✅ PersonRepositoryTests.cs::VO_CRUD_01_SaveAsync_WithValidEntity_PersistsToDb |
| VO-CRUD-02 | GetByRowIdAsync で存在する ID から Entity が返される | 正常系 | ✅ PersonRepositoryTests.cs::VO_CRUD_02_GetByRowIdAsync_WithValidId_ReturnsEntity |
| VO-CRUD-03 | UpdateAsync で Entity が DB で更新される | 正常系 | ✅ PersonRepositoryTests.cs::VO_CRUD_03_UpdateAsync_WithValidEntity_UpdatesDb |
| VO-CRUD-04 | DeleteAsync で Entity が論理削除される | 正常系 | ✅ PersonRepositoryTests.cs::VO_CRUD_04_DeleteAsync_WithValidId_LogicallyDeletes |
| VO-CRUD-05 | 削除済み Person で null が返される | 正常系 | ✅ PersonRepositoryTests.cs::VO_CRUD_05_GetByRowIdAsync_WithDeletedPerson_ReturnsNull |

---

## 🧪 Mapper テストケース詳細

### 1. EmployeeMapperTests

**ファイル:** `Mappers/EmployeeMapperTests.cs`

**責務:** DbModel（DB保存型） ↔ Employee Entity（Domain型）の双方向変換

#### グループ 1: ToDbModel — Entity → DbModel

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1.1 | ToDbModel_WithValidEntity_MapsAllFields | 有効な Entity | 全フィールド マップ |
| 1.2 | ToDbModel_WithDeletedEntity_SetsDeletedAt | 削除済み Entity | deleted_at フィールド |
| 1.3 | ToDbModel_WithNullableFields_HandlesCorrectly | 未設定フィールド | NULL 値マップ |

#### グループ 2: ToDomainEntity — DbModel → Entity

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 2.1 | ToDomainEntity_WithValidDbModel_MapsAllFields | 有効な DbModel | 全フィールド マップ |
| 2.2 | ToDomainEntity_WithDeletedAt_CreatesDeletedState | deleted_at 有り | IsDeleted=true |
| 2.3 | ToDomainEntity_WithNullableFields_ConvertsProperly | NULL フィールド | 未設定状態 |

#### グループ 3: DateTime ↔ LocalDateTime

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 3.1 | DateTimeConversion_ToLocalDateTime_Correct | DateTime → LocalDateTime | タイムゾーン正確性 |
| 3.2 | DateTimeConversion_ToDateTime_RoundTrip | LocalDateTime → DateTime → LocalDateTime | 往復変換の一貫性 |

**テストケース数:** 8

---

### 2. PersonMapperTests

**ファイル:** `Mappers/PersonMapperTests.cs`

**責務:** DbModel（Person） ↔ Person Entity の双方向変換

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1 | ToDbModel_WithValidEntity_MapsAllFields | 有効な Entity | 全フィールド マップ |
| 2 | ToDomainEntity_WithValidDbModel_MapsAllFields | 有効な DbModel | 全フィールド マップ |
| 3 | NameFieldConversion_WithKanaAndKanjiFields | 名前フィールド | LastName/FirstName/Kana マップ |
| 4 | DateTimeConversion_ConsistentAfterRoundTrip | 日時フィールド | 往復変換の一貫性 |

**テストケース数:** 4

---

## 🧪 Repository テストケース

### 3. EmployeeRepositoryTests

**ファイル:** `Repositories/EmployeeRepositoryTests.cs`

**責務:** Employee Entity の永続化（Save・Get・Update・Delete）

#### グループ 1: SaveAsync — 新規作成

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1.1 | SaveAsync_WithValidEntity_PersistsToDb | 有効な Entity | DB に保存 |
| 1.2 | SaveAsync_SetsAuditFields_CreatedAt_CreatedBy | 監査フィールド | created_at, created_by 設定 |
| 1.3 | SaveAsync_WithMultipleEntities_AllPersisted | 複数 Entity | 全て保存 |

#### グループ 2: GetByIdAsync — 取得

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 2.1 | GetByIdAsync_WithValidId_ReturnsEntity | 存在する ID | Entity 返却 |
| 2.2 | GetByIdAsync_WithNonExistentId_ReturnsNull | 存在しない ID | null 返却 |
| 2.3 | GetByIdAsync_WithDeletedEntity_ReturnsNull | 論理削除済み | null 返却 |

#### グループ 3: UpdateAsync — 更新

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 3.1 | UpdateAsync_WithValidEntity_UpdatesDb | 有効な Entity | DB 更新 |
| 3.2 | UpdateAsync_SetsAuditFields_UpdatedAt | 監査フィールド | updated_at 設定 |
| 3.3 | UpdateAsync_WithNonExistentEntity_ThrowsException | 非存在 Entity | 例外 |
| 3.4 | UpdateAsync_WithRowVersionConflict_ThrowsException | バージョン不一致 | OptimisticLockException |

#### グループ 4: DeleteAsync — 論理削除

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 4.1 | DeleteAsync_WithValidId_LogicallyDeletes | 存在する ID | deleted_at 設定 |
| 4.2 | DeleteAsync_SetsDeletedBy_FromClock | 監査フィールド | deleted_by 設定 |
| 4.3 | DeleteAsync_WithNonExistentId_ThrowsException | 非存在 ID | 例外 |

#### グループ 5: 複合操作

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 5.1 | SaveThenGet_RoundTrip_RetrievesCorrectly | Save → Get | 一貫性 |
| 5.2 | UpdateThenGet_ReflectsChanges | Update → Get | 更新反映 |
| 5.3 | DeleteThenGet_CannotRetrieveDeleted | Delete → Get | null 返却 |

**テストケース数:** 16

---

### 4. PersonRepositoryTests

**ファイル:** `Repositories/PersonRepositoryTests.cs`

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1 | SaveAsync_WithValidEntity_PersistsToDb | 有効な Entity | DB 保存 |
| 2 | GetByRowIdAsync_WithValidId_ReturnsEntity | 存在する ID | Entity 返却 |
| 3 | UpdateAsync_WithValidEntity_UpdatesDb | 有効な Entity | DB 更新 |
| 4 | DeleteAsync_WithValidId_LogicallyDeletes | 存在する ID | deleted_at 設定 |
| 5 | GetByRowIdAsync_WithDeletedPerson_ReturnsNull | 削除済み Person | null 返却 |

**テストケース数:** 5

---

## 📊 テストケース集計

| コンポーネント | テスト数 | 合計 |
|----------|---------|------|
| EmployeeMapper | 8 | |
| PersonMapper | 4 | |
| EmployeeRepository | 16 | |
| PersonRepository | 5 | |
| **合計** | | **33** |

---

## 🔧 テスト Helper/Fixture

### InfrastructureFixture.cs

```csharp
namespace SupportAdvance.Contexts.Employee.Infrastructure.Tests.Fixtures;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Employee.Infrastructure.Repositories;

/// <summary>
/// テスト用 In-Memory DB と Mapper を提供
/// </summary>
public class InfrastructureFixture
{
    private readonly LocalDateTime _fixedTime = new(new DateTime(2026, 9, 16, 12, 0, 0));
    private readonly IClock _clock;

    public InfrastructureFixture()
    {
        _clock = new FixedClock(_fixedTime);
    }

    public IClock Clock => _clock;

    public EmployeeMapper CreateEmployeeMapper()
        => new();

    public PersonMapper CreatePersonMapper()
        => new();

    /// <summary>
    /// In-Memory SQLite DB を初期化
    /// </summary>
    public async Task<IDbConnectionFactory> InitializeDatabaseAsync()
    {
        var factory = new InMemoryDbConnectionFactory();
        await factory.InitializeAsync();
        return factory;
    }
}
```

---

## 📋 テスト実装チェックリスト

### Mapper テストファイル
- [ ] EmployeeMapperTests (8テスト)
- [ ] PersonMapperTests (4テスト)

### Repository テストファイル
- [ ] EmployeeRepositoryTests (16テスト)
- [ ] PersonRepositoryTests (5テスト)

### Fixture/Helper
- [ ] InfrastructureFixture.cs
- [ ] InMemoryDbConnectionFactory.cs

### テスト検証
- [ ] Red 状態確認（33 失敗）
- [ ] 実装実施
- [ ] Green 状態確認（33 成功）
- [ ] DB マイグレーション検証
- [ ] トランザクション動作確認

---

## 🎓 参考資料

- [Infrastructure_技術仕様書.md](Infrastructure_技術仕様書.md) - Mapper/Repository API
- [Infrastructure_詳細設計書.md](Infrastructure_詳細設計書.md) - 実装指示
- [Employee.Application テスト](../Application/Application_単体テスト仕様書.md) - Application テスト参考

---

**ドキュメント完成！** ✅

