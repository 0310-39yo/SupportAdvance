# Employee Entity 実装修正計画

**対象:** Employee Bounded Context の実装修正  
**目的:** 設計方針書に基づいて既存の Employee Entity 実装を修正  
**版:** 1.0 / 2026-08-09

---

## 1. 修正の概要

現在の Employee Entity 実装は暫定的なものです。設計方針書（Employee_設計方針書.md）に基づいて、以下の項目を修正します。

### 修正スコープ
- ✅ Employee Entity 構造
- ✅ DbModel 設計
- ✅ Mapper 実装
- ✅ Repository 実装
- ✅ ストアドプロシージャ設計

---

## 2. 修正フェーズ

### Phase 1: Employee Entity コード修正

**ファイル:** `src/Contexts/Employee/Employee.Domain/Entities/Employee.cs`

**修正項目：**

1. **公開プロパティの追加**
   ```csharp
   // Entity<EmployeeId> から継承で自動公開
   // public EmployeeId Id { get; } ← 集約根ID
   public EmployeeCode EmployeeCode { get; private set; }
   public LocalDateTime? RetiredAt { get; private set; }    // 雇用終了日
   ```

2. **コンストラクタ修正**
   - `employeeId` パラメータを追加
   - `employeeCode` パラメータを追加

3. **Create ファクトリメソッド修正**
   ```csharp
   public static Employee Create(
       EmployeeId employeeId,         // 集約根ID
       EmployeeRowId rowId,           // 初期値 0L
       EmployeeCode employeeCode,
       LocalDateTime? retiredAt = null)
   ```

4. **Reconstruct ファクトリメソッド修正**
   - Insert 後に RowId を反映して Entity を再作成

5. **ToString メソッド追加**
   ```csharp
   public override string ToString() => $"Employee(AggId={Id.Value}, Code={EmployeeCode})";
   ```

**チェックリスト:**
- [x] Entity<EmployeeId> に変更
- [x] EmployeeCode プロパティ追加
- [x] RetiredAt プロパティ追加（公開プロパティ）
- [x] コンストラクタ更新（employeeId, rowId, employeeCode, retiredAt）
- [x] Create メソッド更新
- [x] Reconstruct メソッド更新
- [x] ToString メソッド更新
- [ ] ビルド確認

---

### Phase 2: DbModel 設計・実装

**ファイル:** `src/Contexts/Employee/Employee.Infrastructure/DataAccess/Models/EmployeeDbModel.cs`

**設計項目：**

1. **監査カラム（上に配置）**
   ```csharp
   public long RowId { get; set; }
   public byte[] RowVersion { get; set; }
   public LocalDateTime CreatedAt { get; set; }
   public long CreatedBy { get; set; }
   public LocalDateTime? UpdatedAt { get; set; }
   public long? UpdatedBy { get; set; }
   public LocalDateTime? DeletedAt { get; set; }
   public long? DeletedBy { get; set; }
   ```

2. **ビジネスカラム**
   ```csharp
   public int EmployeeId { get; set; }           // 従業員ID（1001以上、UNIQUE）
   public DateTime? RetiredAt { get; set; }      // 雇用終了日（NULL許可）
   ```

3. **テーブルマッピング属性**
   ```csharp
   [Table("m_employees")]
   public class EmployeeDbModel
   ```

**チェックリスト:**
- [x] クラス作成
- [x] 監査カラム（上配置）
- [x] ビジネスカラム（employee_id のみ）
- [x] テーブル属性設定
- [ ] ビルド確認

---

### Phase 3: Mapper 実装

**ファイル:** `src/Contexts/Employee/Employee.Infrastructure/Mappers/EmployeeMapper.cs`

**責務：**
- Domain Entity ↔ DbModel の双方向変換
- ValueObject ↔ プリミティブ型の変換

**実装項目：**

1. **ToDomainEntity メソッド**
   ```csharp
   public Employee ToDomainEntity(EmployeeDbModel dbModel)
   {
       var employeeId = EmployeeId.From(dbModel.EmployeeId);  // 集約根ID
       var rowId = EmployeeRowId.From(dbModel.RowId);
       
       // EmployeeCode は EmployeeId のみから構成
       var code = EmployeeCode.From(employeeId);
       
       // 雇用終了日を変換（DateTime → LocalDateTime）
       var retiredAt = dbModel.RetiredAt.HasValue
           ? LocalDateTime.From(dbModel.RetiredAt.Value)
           : (LocalDateTime?)null;
       
       return Employee.Reconstruct(
           employeeId,
           rowId,
           code,
           retiredAt
       );
   }
   ```

2. **ToDbModel メソッド（Insert用）**
   ```csharp
   public EmployeeDbModel ToDbModelForInsert(Employee entity)
   {
       return new EmployeeDbModel
       {
           EmployeeId = entity.Id.Value,
           RetiredAt = entity.RetiredAt?.Value,
           // 監査カラム（CreatedAt/By など）は Repository で設定
       };
   }
   ```

3. **ToDbModel メソッド（Update用）**
   - 同様に実装

**チェックリスト:**
- [x] インターフェース定義
- [x] ToDomainEntity 実装
- [x] ToDbModelForInsert 実装
- [x] ToDbModelForUpdate 実装
- [x] ValueObject 変換確認
- [ ] ビルド確認

---

### Phase 4: Repository 実装

**ファイル:** `src/Contexts/Employee/Employee.Infrastructure/Repositories/EmployeeRepository.cs`

**責務：**
- Employee の永続化・復元
- 監査情報の自動設定
- RowId の生成・更新

**実装項目：**

1. **AddAsync メソッド**
   ```csharp
   public async Task AddAsync(Employee entity)
   {
       // 1. DbModel に変換（RowId は DB で生成）
       var dbModel = _mapper.ToDbModelForInsert(entity);
       
       // 2. 監査情報を設定
       dbModel.CreatedAt = _clock.JstNow;
       dbModel.CreatedBy = _currentUser.PersonRowId;
       
       // 3. ストアドプロシージャ実行（Insert + RowId自動採番）
       var generatedRowId = await _dataAccess.InsertAndReturnRowIdAsync(dbModel);
       
       // 4. 生成された RowId で Entity を再作成
       var createdEmployee = Employee.Reconstruct(
           entity.Id,                              // EmployeeId（集約根ID）
           EmployeeRowId.From(generatedRowId),
           entity.EmployeeCode
       );
       
       // 注: 実際には createdEmployee をメモリに保持する必要があるかは
       //     アプリケーション要件に依存（返り値にするか、キャッシュするか）
   }
   ```

2. **GetByCodeAsync メソッド**
   ```csharp
   public async Task<Employee?> GetByCodeAsync(EmployeeCode code)
   {
       var dbModel = await _dataAccess.GetByEmployeeIdAsync(code.EmployeeId.Value);
       return dbModel == null ? null : _mapper.ToDomainEntity(dbModel);
   }
   
   public async Task<Employee?> GetByEmployeeIdAsync(EmployeeId employeeId)
   {
       var dbModel = await _dataAccess.GetByEmployeeIdAsync(employeeId.Value);
       return dbModel == null ? null : _mapper.ToDomainEntity(dbModel);
   }
   ```

3. **GetPersonInfoAsync メソッド**
   ```csharp
   public async Task<PersonInfo?> GetPersonInfoAsync(EmployeeAggId aggId)
   {
       // t_employee_attributes から attribute_type='Person' の行を取得
   }
   ```

4. **GetRolesByEmployeeAsync メソッド**
   ```csharp
   public async Task<List<EmployeeRole>> GetRolesByEmployeeAsync(EmployeeAggId aggId)
   {
       // t_employee_attributes から attribute_type='Role' を取得
   }
   ```

**チェックリスト:**
- [ ] インターフェース定義（IEmployeeRepository）
- [ ] AddAsync 実装
- [ ] GetByAggIdAsync 実装（集約根IDでの検索）
- [ ] GetByCodeAsync 実装（コード検索）
- [ ] GetPersonInfoAsync 実装（Person属性取得）
- [ ] GetRolesByEmployeeAsync 実装（Role属性取得）
- [ ] UpdateAsync 実装
- [ ] DeleteAsync 実装（論理削除）
- [ ] ビルド確認

---

### Phase 5: DataAccess（Dapper/RepoDb）実装

**ファイル:** `src/Contexts/Employee/Employee.Infrastructure/DataAccess/EmployeeDataAccess.cs`

**実装項目：**

1. **InsertAndReturnRowIdAsync メソッド**
   - ストアドプロシージャを呼び出し
   - 生成された RowId を返す

2. **GetByAggIdAsync メソッド**
   - employee_agg_id で検索

3. **GetByEmployeeIdAsync メソッド**
   - employee_id で検索

4. **GetAllAsync メソッド**
   - 有効な従業員のみ取得（deleted_at IS NULL）

**チェックリスト:**
- [ ] インターフェース定義
- [ ] InsertAndReturnRowIdAsync 実装
- [ ] GetByAggIdAsync 実装
- [ ] GetByEmployeeIdAsync 実装
- [ ] GetAllAsync 実装
- [ ] ビルド確認

---

### Phase 6: ストアドプロシージャ設計・作成

**ファイル:** `src/Contexts/Employee/Employee.Infrastructure/Migrations/001_CreateEmployeeStoredProcedures.sql`

**作成項目：**

1. **sp_InsertEmployee**
   ```sql
   CREATE PROCEDURE [dbo].[sp_InsertEmployee]
       @EmployeeId INT,
       @CreatedAt DATETIME2(7),
       @CreatedBy BIGINT
   AS
   BEGIN
       INSERT INTO [dbo].[m_employees]
       (employee_id, created_at, created_by)
       VALUES
       (@EmployeeId, @CreatedAt, @CreatedBy);
       
       -- 生成された RowId と監査情報を返す
       OUTPUT INSERTED.row_id, INSERTED.row_version, 
              INSERTED.employee_id, 
              INSERTED.created_at, INSERTED.created_by;
   END
   ```

**チェックリスト:**
- [ ] sp_InsertEmployee 作成
- [ ] OUTPUT INSERTED で RowId 返す
- [ ] sp_UpdateEmployee 作成
- [ ] sp_DeleteEmployee 作成（論理削除）
- [ ] Migration ファイル作成

---

### Phase 7: ユニットテスト修正

**ファイル:** `tests/Contexts/Employee.Domain.Tests/Entities/EmployeeTests.cs`

**修正項目：**

1. **テストケース更新**
   - EmployeeId/EmployeeCode での生成テスト
   - 各公開プロパティのアクセステスト
   - RowId = 0L での初期化テスト
   - EmployeeId（集約根ID）のアクセステスト

2. **新規テストケース追加**
   - Insert 後の RowId 反映テスト
   - Reconstruct メソッドのテスト

**チェックリスト:**
- [ ] 既存テスト修正
- [ ] 新規テスト追加
- [ ] 全テスト実行・成功確認

---

### Phase 8: 統合テスト

**実施項目：**

1. **End-to-End フロー**
   - Employee 作成 → DB Insert → RowId 生成 → Entity 再作成

2. **Mapper 検証**
   - Entity → DbModel → Entity のラウンドトリップ

3. **Repository 検証**
   - AddAsync → GetByCodeAsync で復元可能

**チェックリスト:**
- [ ] ローカルでビルド成功
- [ ] 全ユニットテスト成功
- [ ] 統合テスト実行・成功
- [ ] コードレビュー準備完了

---

## 3. 実装の優先順位

### 最初に
1. **Phase 1**: Employee Entity 修正（テスト対象の基盤）
2. **Phase 2**: DbModel 設計（マッピング対象）
3. **Phase 3**: Mapper 実装（変換ロジック）

### 次に
4. **Phase 5**: DataAccess 実装（DB 操作）
5. **Phase 6**: ストアドプロシージャ（DB 側の実装）
6. **Phase 4**: Repository 実装（ビジネスロジック）

### 最後に
7. **Phase 7**: ユニットテスト修正
8. **Phase 8**: 統合テスト

---

## 4. 各フェーズの成果物

| フェーズ | ファイル | 成果物 |
|---------|---------|--------|
| 1 | Employee.cs | 修正済み Entity |
| 2 | EmployeeDbModel.cs | DbModel クラス |
| 3 | EmployeeMapper.cs | Mapper 実装 |
| 4 | EmployeeRepository.cs | Repository 実装 |
| 5 | EmployeeDataAccess.cs | DataAccess 実装 |
| 6 | Migration SQL | ストアド＋テーブル |
| 7 | EmployeeTests.cs | 修正テスト |
| 8 | 統合テスト | 全フロー確認 |

---

## 5. 実装完了チェック

全フェーズ完了後：

- [ ] コンパイル成功
- [ ] 全ユニットテスト成功（20+テスト）
- [ ] 統合テスト成功
- [ ] コードレビュー完了
- [ ] ドキュメント最新化完了

---

## 6. 参考資料

- Employee_設計方針書.md（この実装計画の根拠）
- TABLE_DESIGN_STANDARDS.md（DB テーブル設計）
- Repository_パターンガイド.md（Repository 実装）

---

## 7. 見積もり

**各フェーズの工数目安：**

| フェーズ | 工数 | 難度 |
|---------|------|------|
| 1 | 1h | 低 |
| 2 | 1h | 低 |
| 3 | 2h | 中 |
| 4 | 2h | 中 |
| 5 | 1.5h | 低 |
| 6 | 1.5h | 低 |
| 7 | 2h | 中 |
| 8 | 1h | 低 |
| **合計** | **12h** | |

