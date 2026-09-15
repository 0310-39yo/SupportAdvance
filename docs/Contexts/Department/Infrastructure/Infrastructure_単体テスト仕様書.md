# Department Context Infrastructure層 - 単体テスト仕様書

**バージョン:** 1.0  
**作成日:** 2026-09-16  
**対象読者:** テスト実装者  
**内容:** テストケース一覧、グループ定義、検証方法

---

## 📋 概要

本書は、Department Context Infrastructure層の **単体テスト仕様** です。

**2 つのクラスを統合ドキュメント化:**
- **DepartmentMapper**: DB 非依存の純粋ロジック（実装済み、実行可能）
- **DepartmentRepository**: DB 依存の統合テスト（未実装、Phase 5 へ延期）

---

## 0. 本書の位置づけ

### Mapper と Repository の役割分離

本ドキュメントは Mapper と Repository の 2 つのクラスを 1 つの仕様書でカバーしますが、責務が異なります：

| クラス | 責務 | テスト段階 | 依存性 |
|--------|------|---------|--------|
| **DepartmentMapper** | Entity ↔ DbModel 型変換（純粋ロジック） | Phase 1（完了） | なし（TestFixture のみ） |
| **DepartmentRepository** | DB 永続化・取得（統合テスト） | Phase 5（計画中） | SQL Server DB 接続必須 |

### Mapper テストの特性

DepartmentMapper は **DB 非依存の純粋関数型** であり、以下のテストが実行可能：

- `ToDomainEntity(DbModel)`: DbModel から Entity への変換
- `ToDbModel(Entity)`: Entity から DbModel への変換
- ValueObject の型変換（DateTime ↔ LocalDateTime）
- null 処理と Unset 状態への変換
- 監査フィールド非設定の検証（Repository が設定する責務）

### Repository テストの現状

DepartmentRepository テストは **すべて Skip** されており、以下の理由により Phase 5 に延期：

1. **SQL Server 接続が必須** — 本番環境/テスト環境の DB が必要
2. **統合テストへの分類** — 単体テストではなく結合テストとして再設計予定
3. **スキャフォルディング段階** — テスト構造は整備済みだが実装は不完全

Phase 5 では DI コンテナ、データベース初期化、トランザクション管理等を含める統合テストスイートとして再実装します。

---

## 1. テスト目的

### Mapper テストの目的

DepartmentMapper の各メソッドが、以下の仕様を満たすことを確認する：

- **型変換の正確性**: DbModel → Entity / Entity → DbModel の双方向変換が正しいこと
- **ValueObject の構築**: DepartmentCode, HierarchyLevel, etc. が正しく構築されること
- **null 処理**: DB null を Unset 状態に正確に変換すること
- **Optional フィールド**: ParentDepartmentRowId, ManagerEmployeeRowId の IsSet フラグ管理
- **監査フィールド非設定**: Mapper は CreatedAt/UpdatedAt/DeletedAt を設定しないこと
- **ラウンドトリップ**: DbModel → Entity → DbModel で データが保持されること

### Repository テストの目的（Phase 5 予定）

- CRUD 操作の正確性（GetByIdAsync, SaveAsync, DeleteAsync 等）
- 監査フィールド自動設定（CreatedAt/CreatedBy, UpdatedAt/UpdatedBy など）
- 論理削除の実装確認（DeletedAt/DeletedBy の自動設定）
- トランザクション管理、楽観ロック検証

---

## 2. テスト対象クラス

### 2.1 DepartmentMapper

| 項目 | 内容 |
|------|------|
| **クラス名** | DepartmentMapper |
| **クラス分類** | ☑ Utility ☐ Helper ☑ Converter ☐ Validator ☐ Calculator ☐ その他 |
| **名前空間** | SupportAdvance.Contexts.Department.Infrastructure.Mappers |
| **責務** | Entity ↔ DbModel の型変換（DB 非依存の純粋ロジック） |
| **依存クラス** | DepartmentDbModel, Department, ValueObjects（DepartmentCode 等） |
| **テスト段階** | Phase 1（実装完了、実行可能） |

### 2.2 DepartmentRepository

| 項目 | 内容 |
|------|------|
| **クラス名** | DepartmentRepository |
| **クラス分類** | ☐ Utility ☐ Helper ☐ Converter ☐ Validator ☐ Calculator ☑ その他（Repository） |
| **名前空間** | SupportAdvance.Contexts.Department.Infrastructure.Repositories |
| **実装インターフェース** | IDepartmentRepository |
| **責務** | Department Aggregate の DB 永続化・検索・削除 |
| **依存クラス** | DbConnection, Dapper/RepoDb, DepartmentMapper, IClock, ICurrentUserService |
| **テスト段階** | Phase 5（DB 接続テストとして再設計予定） |
| **現状** | スキャフォルディングのみ、テスト実装は Skip |

---

## 3. テスト対象メソッド

### 3.1 Mapper メソッド

| # | メソッド名 | シグネチャ | 責務 | テスト段階 |
|---|----------|---------|------|---------|
| 1 | ToDomainEntity | `Department ToDomainEntity(DepartmentDbModel dbModel)` | DbModel → Entity 変換 | Phase 1（実装済み） |
| 2 | ToDbModel | `DepartmentDbModel ToDbModel(Department entity)` | Entity → DbModel 変換 | Phase 1（実装済み） |

### 3.2 Repository メソッド

| # | メソッド名 | シグネチャ | 責務 | テスト段階 |
|---|----------|---------|------|---------|
| 1 | GetByIdAsync | `Task<Department?> GetByIdAsync(DepartmentRowId id)` | ID で取得 | Phase 5（Skip 中） |
| 2 | GetByCodeAsync | `Task<Department?> GetByCodeAsync(DepartmentCode code)` | コードで取得 | Phase 5（Skip 中） |
| 3 | GetAllAsync | `Task<IReadOnlyList<Department>> GetAllAsync()` | 全件取得 | Phase 5（Skip 中） |
| 4 | SaveAsync | `Task SaveAsync(Department entity)` | 保存（Insert/Update） | Phase 5（Skip 中） |
| 5 | DeleteAsync | `Task DeleteAsync(DepartmentRowId id)` | 論理削除 | Phase 5（Skip 中） |

---

## 4. テスト観点一覧

### 観点グループ MAP-TO-DOM：Mapper.ToDomainEntity テスト

| 観点ID | 観点（説明） | 分類 | テスト用実装 | テスト段階 |
|--------|------|------|------------|---------|
| MAP-TO-DOM-01 | 最小限の DbModel から Entity が生成される | 正常系 | Test Data Builder | Phase 1 ✓ |
| MAP-TO-DOM-02 | Optional フィールド（ParentId, ManagerId）が IsSet フラグ付きで変換される | 正常系 | Test Data Builder | Phase 1 ✓ |
| MAP-TO-DOM-03 | AbolishedOn（DateTime?）が AbolishedOn ValueObject に変換される | 正常系 | Test Data Builder | Phase 1 ✓ |
| MAP-TO-DOM-04 | null DbModel で ArgumentNullException が発生 | 異常系 | DIFixture | Phase 1 ✓ |
| MAP-TO-DOM-05 | 空の DepartmentCode で InvalidOperationException が発生 | 異常系 | DIFixture | Phase 1 ✓ |
| MAP-TO-DOM-06 | 範囲外の HierarchyLevel で InvalidOperationException が発生 | 異常系 | DIFixture | Phase 1 ✓ |

### 観点グループ MAP-TO-DB：Mapper.ToDbModel テスト

| 観点ID | 観点（説明） | 分類 | テスト用実装 | テスト段階 |
|--------|------|------|------------|---------|
| MAP-TO-DB-01 | 最小限の Entity から DbModel が生成される | 正常系 | Test Data Builder | Phase 1 ✓ |
| MAP-TO-DB-02 | Optional フィールド（ParentId, ManagerId）が IsSet に応じて null/値に変換される | 正常系 | Test Data Builder | Phase 1 ✓ |
| MAP-TO-DB-03 | AbolishedOn（LocalDateTime）が DateTime に変換される | 正常系 | Test Data Builder | Phase 1 ✓ |
| MAP-TO-DB-04 | 監査フィールド（CreatedAt, UpdatedAt, DeletedAt）は設定されない | 正常系（検証） | DIFixture | Phase 1 ✓ |
| MAP-TO-DB-05 | null Entity で ArgumentNullException が発生 | 異常系 | DIFixture | Phase 1 ✓ |
| MAP-TO-DB-RT-01 | DbModel → Entity → DbModel ラウンドトリップでデータが保持される | 正常系（等価性） | Test Data Builder | Phase 1 ✓ |

### 観点グループ REP-SKIP：Repository テスト（Phase 5 へ延期）

| 観点ID | 観点（説明） | 分類 | Skip 理由 | 実装予定時期 |
|--------|------|------|---------|---------|
| REP-SKIP-01 | GetByIdAsync で存在する ID から Department が取得される | 正常系 | 要 DB 接続 | Phase 5 |
| REP-SKIP-02 | GetByIdAsync で存在しない ID から null が返される | 正常系 | 要 DB 接続 | Phase 5 |
| REP-SKIP-03 | GetByCodeAsync で有効なコードから Department が取得される | 正常系 | 要 DB 接続 | Phase 5 |
| REP-SKIP-04 | GetAllAsync で全件が IReadOnlyList で返される | 正常系 | 要 DB 接続 | Phase 5 |
| REP-SKIP-05 | SaveAsync で新規 Entity が Insert される | 正常系（Insert） | 要 DB 接続 | Phase 5 |
| REP-SKIP-06 | SaveAsync で CreatedAt/CreatedBy が自動設定される | 正常系（監査） | 要 DB 接続 | Phase 5 |
| REP-SKIP-07 | SaveAsync で既存 Entity が Update される | 正常系（Update） | 要 DB 接続 | Phase 5 |
| REP-SKIP-08 | SaveAsync で UpdatedAt/UpdatedBy が自動設定される | 正常系（監査） | 要 DB 接続 | Phase 5 |
| REP-SKIP-09 | DeleteAsync で Entity が論理削除される | 正常系（Delete） | 要 DB 接続 | Phase 5 |
| REP-SKIP-10 | DeleteAsync で DeletedAt/DeletedBy が自動設定される | 正常系（監査） | 要 DB 接続 | Phase 5 |
| REP-SKIP-11 | SaveAsync に null を渡すと ArgumentNullException が発生 | 異常系 | 要 DB 接続 | Phase 5 |
| REP-SKIP-12 | GetByIdAsync に null を渡すと ArgumentNullException が発生 | 異常系 | 要 DB 接続 | Phase 5 |

---

## 5. テスト仕様

### 5.1 Mapper テスト仕様

#### 観点 MAP-TO-DOM-01：最小限の DbModel から Entity が生成される

**テストコード:** `DepartmentMapperTests.Test1_1_ToDomainEntity_WithValidDbModel_ReturnsDepartmentEntity()`

**テスト手順:**
1. DepartmentMapper インスタンスを作成
2. 最小限の DbModel（ID, Code, Name, Level のみ）を構築
3. `mapper.ToDomainEntity(dbModel)` を呼び出す
4. 戻り値が Department インスタンスであることを確認
5. 各プロパティが正しく変換されていることを確認

**期待結果:**
- Result != null
- RowId = 1L
- DeptCode = "D001"
- Name = "営業部"
- Level = 1

**判定基準:**
- [ ] Department インスタンスが返される
- [ ] すべてのプロパティが期待値と一致

---

#### 観点 MAP-TO-DOM-02：Optional フィールドが IsSet フラグ付きで変換される

**テストコード:** `DepartmentMapperTests.Test1_2_ToDomainEntity_WithAllValueObjects_ConvertsSuccessfully()`

**テスト手順:**
1. ParentDepartmentRowId, ManagerEmployeeRowId を持つ DbModel を構築
2. `mapper.ToDomainEntity(dbModel)` を呼び出す
3. ParentId.IsSet と ManagerId.IsSet が true であることを確認
4. 実際の値が正しいことを確認

**期待結果:**
- ParentId.IsSet = true
- ParentId.Value = 1L
- ManagerId.IsSet = true
- ManagerId.Value = 100L

**判定基準:**
- [ ] Optional フィールドの IsSet フラグが true
- [ ] 実際の値が期待値と一致

---

#### 観点 MAP-TO-DOM-03：AbolishedOn が正しく変換される

**テストコード:** `DepartmentMapperTests.Test1_3_ToDomainEntity_WithAbolishedOn_ConvertsSuccessfully()`

**テスト手順:**
1. AbolishedOn (DateTime) を持つ DbModel を構築
2. `mapper.ToDomainEntity(dbModel)` を呼び出す
3. 戻り値の AbolishedOn.IsAbolished が true であることを確認

**期待結果:**
- AbolishedOn.IsAbolished = true

**判定基準:**
- [ ] AbolishedOn ValueObject が正しく構築される

---

#### 観点 MAP-TO-DOM-04：null DbModel で例外が発生

**テストコード:** `DepartmentMapperTests.Test2_1_ToDomainEntity_WithNullDbModel_ThrowsArgumentNullException()`

**期待結果:**
```csharp
Assert.Throws<ArgumentNullException>(() => mapper.ToDomainEntity(null!));
```

**判定基準:**
- [ ] ArgumentNullException がスローされる

---

#### 観点 MAP-TO-DOM-05：空の Code で例外が発生

**テストコード:** `DepartmentMapperTests.Test2_2_ToDomainEntity_WithInvalidCode_ThrowsInvalidOperationException()`

**期待結果:**
```csharp
Assert.Throws<InvalidOperationException>(() => mapper.ToDomainEntity(dbModel));
// Code = ""
```

**判定基準:**
- [ ] InvalidOperationException がスローされる

---

#### 観点 MAP-TO-DOM-06：範囲外の Level で例外が発生

**テストコード:** `DepartmentMapperTests.Test2_3_ToDomainEntity_WithInvalidLevel_ThrowsInvalidOperationException()`

**期待結果:**
```csharp
Assert.Throws<InvalidOperationException>(() => mapper.ToDomainEntity(dbModel));
// Level = 99（無効）
```

**判定基準:**
- [ ] InvalidOperationException がスローされる

---

#### 観点 MAP-TO-DB-01：最小限の Entity から DbModel が生成される

**テストコード:** `DepartmentMapperTests.Test3_1_ToDbModel_WithValidEntity_ReturnsDbModel()`

**テスト手順:**
1. Department.Create で最小限の Entity を構築
2. `mapper.ToDbModel(entity)` を呼び出す
3. 戻り値が DepartmentDbModel であることを確認
4. ビジネスフィールドが正しく変換されていることを確認

**期待結果:**
- Result.RowId = 1L
- Result.Code = "D001"
- Result.Name = "営業部"
- Result.Level = 1
- Result.ParentDepartmentRowId = null
- Result.ManagerEmployeeRowId = null

**判定基準:**
- [ ] DepartmentDbModel インスタンスが返される
- [ ] ビジネスフィールドが期待値と一致
- [ ] Optional フィールドが null

---

#### 観点 MAP-TO-DB-02：Optional フィールドが IsSet に応じて変換される

**テストコード:** `DepartmentMapperTests.Test3_2_ToDbModel_WithParentAndManager_ConvertsSuccessfully()`

**期待結果:**
- Result.ParentDepartmentRowId = 1L
- Result.ManagerEmployeeRowId = 100L

**判定基準:**
- [ ] Optional フィールドが IsSet に応じて null/値に変換される

---

#### 観点 MAP-TO-DB-03：AbolishedOn が DateTime に変換される

**テストコード:** `DepartmentMapperTests.Test3_4_ToDbModel_WithAbolishedEntity_ConvertsAbolishedOn()`

**期待結果:**
- Result.AbolishedOn = new DateTime(2026, 9, 30)

**判定基準:**
- [ ] LocalDateTime が DateTime に正しく変換される

---

#### 観点 MAP-TO-DB-04：監査フィールドは設定されない

**テストコード:** `DepartmentMapperTests.Test4_2_ToDbModel_ShouldNotSetAuditFields()`

**テスト手順:**
1. Entity を ToDbModel で変換
2. 返却された DbModel の監査フィールドを確認

**期待結果:**
- Result.CreatedAt = default(DateTime)
- Result.UpdatedAt = null
- Result.CreatedBy = null
- Result.UpdatedBy = null

**判定基準:**
- [ ] 監査フィールドが設定されていない
- [ ] Repository が設定する責務であることを確認

---

#### 観点 MAP-TO-DB-05：null Entity で例外が発生

**テストコード:** `DepartmentMapperTests.Test4_1_ToDbModel_WithNullEntity_ThrowsArgumentNullException()`

**期待結果:**
```csharp
Assert.Throws<ArgumentNullException>(() => mapper.ToDbModel(null!));
```

**判定基準:**
- [ ] ArgumentNullException がスローされる

---

#### 観点 MAP-TO-DB-RT-01：ラウンドトリップでデータが保持される

**テストコード:** `DepartmentMapperTests.Test5_1_RoundTrip_DbModelToDomainAndBack_PreservesData()`

**テスト手順:**
1. 元の DbModel を準備
2. ToDomainEntity で Entity に変換
3. ToDbModel で再び DbModel に変換
4. 元と戻りのデータが同じであることを確認

**期待結果:**
- RowId, Code, Name, Level が一致

**判定基準:**
- [ ] データが往復で保持される
- [ ] 型変換による精度喪失がない

---

### 5.2 Repository テスト仕様（Phase 5 へ延期）

#### Skip テスト一覧

すべての Repository テスト（REP-SKIP-01 ～ REP-SKIP-12）は現在 Skip されています：

```csharp
[Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
public async Task Test1_1_GetByIdAsync_WithValidId_ReturnsDepartment()
{
    // Arrange
    var repository = CreateRepository();
    var departmentId = DepartmentRowId.From(1L);

    // Act
    var result = await repository.GetByIdAsync(departmentId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(departmentId, result.RowId);
}
```

#### Phase 5 での実装予定

**予定時期:** 2026年第 5 四半期  
**スコープ:** SQL Server の実 DB に対する結合テスト  
**前提条件:**
- テスト環境 DB の構築・初期化スクリプト準備
- DI コンテナのテスト設定完成
- IClock, ICurrentUserService のテストダブル準備

**実装順序:**
1. DB 接続テスト基盤（DbConnectionFactory, Transaction 管理）
2. 監査フィールド自動設定（CreatedAt/CreatedBy 等）
3. 論理削除の検証（DeletedAt/DeletedBy）
4. 楽観ロック（RowVersion）検証
5. エラーケース（重複キー、FK 違反 等）

---

## 6. テスト用実装の設定

### 6.1 Mapper テスト環境

**テスティングフレームワーク:** xUnit  
**テストデータ:** Test Data Builder パターン（Department.Create, ValueObjects.From）

```csharp
public class DepartmentMapperTests
{
    private DepartmentMapper _mapper = new DepartmentMapper();

    public DepartmentMapperTests()
    {
        _mapper = new DepartmentMapper();
    }
}
```

### 6.2 Repository テスト環境（Phase 5）

**テスティングフレームワーク:** xUnit  
**DB 接続:** SQL Server LocalDB または テスト DB  
**初期化:** Database Fixtures / MigrationsRunner  

```csharp
public class DepartmentRepositoryTests : IAsyncLifetime
{
    private SqlConnection _connection;
    private IDepartmentRepository _repository;

    public async Task InitializeAsync()
    {
        // DB 接続、スキーマ初期化
    }

    public async Task DisposeAsync()
    {
        // DB クリーンアップ
    }
}
```

---

## 7. 前提条件・制限事項

| 項目 | 内容 |
|------|------|
| **Mapper テスト** | 実装済み、実行可能。DB 接続不要 |
| **Repository テスト** | 現在すべて Skip。Phase 5 で DB 接続テストへ再設計予定 |
| **DI コンテナ** | Mapper テストでは不要（直接インスタンス化）。Repository テストは Phase 5 で構築 |
| **非同期処理** | Repository テストは async/await（Phase 5）。Mapper テストは同期 |
| **言語機能** | C# 11 以上、NullableReferenceTypes 有効 |
| **ValueObject 依存** | DepartmentCode, HierarchyLevel 等の TryFrom/From 実装が完了 |
| **テストダブル** | MockDepartmentRepository（テストコード内に組み込み）、MockClock（LocalDateTime テスト用） |

---

## 8. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-09-16 | Claude Haiku 4.5 | 初版作成。Mapper テスト（6 観点）と Repository Skip テスト（12 観点）を統合。Mapper は Phase 1 完了、Repository は Phase 5 延期を明記 |

---

## 付録：Repository テスト詳細（参考）

### 予定テストケース（Phase 5 実装予定）

#### GetByIdAsync テストケース

```
テスト1: 存在する ID で Department が取得される
- SetUp: DB に departmentId=1 のレコード挿入
- Act: repository.GetByIdAsync(DepartmentRowId.From(1L))
- Assert: Result != null, Result.RowId.Value == 1L

テスト2: 存在しない ID で null が返される
- SetUp: (なし)
- Act: repository.GetByIdAsync(DepartmentRowId.From(99999L))
- Assert: Result == null

テスト3: 論理削除済みの ID で null が返される
- SetUp: DB に deleted_at が設定されたレコード挿入
- Act: repository.GetByIdAsync(deletedId)
- Assert: Result == null（有効行のみ取得）
```

#### SaveAsync（Insert）テストケース

```
テスト1: 新規 Entity が Insert される
- SetUp: Entity 作成（RowId は新規）
- Act: repository.SaveAsync(entity)
- Assert: DB に行が 1 行追加される

テスト2: CreatedAt/CreatedBy が自動設定される
- SetUp: IClock = MockClock(固定時刻)
- Act: repository.SaveAsync(entity)
- Assert: DB の created_at が MockClock の時刻に設定されている
```

#### DeleteAsync（論理削除）テストケース

```
テスト1: DeletedAt/DeletedBy が設定される
- SetUp: DB に departmentId=1 のレコード挿入
- Act: repository.DeleteAsync(DepartmentRowId.From(1L))
- Assert: DB の deleted_at が現在時刻に設定されている、deleted_by が設定されている

テスト2: 削除済みレコードは GetByIdAsync で見えない
- SetUp: 削除済みレコードが存在
- Act: repository.GetByIdAsync(deletedId)
- Assert: Result == null
```

---
