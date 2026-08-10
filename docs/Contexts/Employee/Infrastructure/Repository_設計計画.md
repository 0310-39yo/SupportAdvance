# Employee Context - Repository 設計計画

**対象者**: 設計者、開発者

**版**: 1.0  
**作成日**: 2026-08-10

---

## 📖 概要

Employee Context の Repository パターンを実装します。Domain エンティティ（Employee、DepartmentMembership、RoleAssignment、PermissionAssignment）をデータベースに永続化し、復元します。

---

## 🎯 実装スコープ

### Phase 3.1: Employee Repository
- IEmployeeRepository（Application層で定義）
- EmployeeRepository（Infrastructure層で実装）
- EmployeeDbModel（DB マッピング用）
- EmployeeMapper（Entity ↔ DbModel 変換）

### Phase 3.2: 複合エンティティマッピング
- DepartmentMembership, RoleAssignment, PermissionAssignment の DbModel
- 複数テーブル集約のマッピング戦略

### Phase 3.3: データアクセス層
- DbContext 統合
- トランザクション管理

---

## 📐 アーキテクチャ

### 依存関係

```
Domain (Entity) 
  ↑
  │
Application (IEmployeeRepository インターフェース)
  ↑
  │
Infrastructure (EmployeeRepository 実装)
  ↓
  DB
```

### 層別責務

| 層 | 責務 | ファイル例 |
|-------|------|----------|
| **Application** | Repository インターフェース定義 | `IEmployeeRepository.cs` |
| **Infrastructure** | Repository 実装、DB操作 | `EmployeeRepository.cs` |
| **Infrastructure** | Entity ↔ DbModel マッピング | `EmployeeMapper.cs` |
| **Infrastructure** | DB スキーママッピング | `EmployeeDbModel.cs` |

---

## 🏗️ EmployeeRepository 構成

### 1. IEmployeeRepository インターフェース

**責務**: Repository 契約の定義

```csharp
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(EmployeeId id);
    Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId);
    Task<IReadOnlyList<Employee>> GetByPersonRowIdAsync(PersonRowId personRowId);
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(EmployeeId id);
}
```

**メソッド仕様**:
- **GetByIdAsync**: 集約根ID（GUID）で検索
- **GetByRowIdAsync**: DB行ID（long）で検索
- **GetByPersonRowIdAsync**: 人事マスタ行ID で複数検索
- **AddAsync**: 新規 Employee を登録
- **UpdateAsync**: 既存 Employee を更新
- **DeleteAsync**: Employee を論理削除

---

### 2. EmployeeDbModel

**責務**: DB テーブルマッピング

```csharp
public class EmployeeDbModel
{
    // 主キー・行識別子
    public long RowId { get; set; }
    public Guid EmployeeId { get; set; }  // 集約根ID
    
    // ビジネスプロパティ
    public string EmployeeCodeDivision { get; set; }  // M/T/C
    public int EmployeeCodeNumber { get; set; }       // 1234-9999
    public long PersonRowId { get; set; }              // m_persons.row_id
    
    // 監査カラム
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }
    public byte[] RowVersion { get; set; }
    
    // 子エンティティ
    public List<DepartmentMembershipDbModel> DepartmentMemberships { get; set; }
    public List<RoleAssignmentDbModel> RoleAssignments { get; set; }
    public List<PermissionAssignmentDbModel> PermissionAssignments { get; set; }
}
```

---

### 3. EmployeeMapper

**責務**: Entity ↔ DbModel 双方向マッピング

```csharp
public class EmployeeMapper
{
    // Entity → DbModel（保存用）
    public EmployeeDbModel ToDbModel(Employee entity)
    {
        return new EmployeeDbModel
        {
            RowId = entity.RowId.Value,
            EmployeeId = entity.Id.Value,
            EmployeeCodeDivision = entity.Code.Division.Value switch 
            { 
                0 => "M", 1 => "T", 2 => "C" 
            },
            EmployeeCodeNumber = entity.Code.Number.Value,
            PersonRowId = entity.PersonRowId.Value,
            // 監査カラムは Repository で設定
        };
    }
    
    // DbModel → Entity（復元用）
    public Employee ToDomainEntity(EmployeeDbModel dbModel)
    {
        var division = dbModel.EmployeeCodeDivision switch
        {
            "M" => EmployeeDivision.RegularEmployee(),
            "T" => EmployeeDivision.Dispatched(),
            "C" => EmployeeDivision.Contractor(),
            _ => throw new InvalidOperationException()
        };
        
        var code = EmployeeCode.From(division, EmployeeNumber.From(dbModel.EmployeeCodeNumber));
        
        return Employee.Reconstruct(
            EmployeeId.From(dbModel.EmployeeId),
            EmployeeRowId.From(dbModel.RowId),
            code,
            PersonRowId.From(dbModel.PersonRowId)
        );
    }
}
```

---

### 4. EmployeeRepository 実装

**責務**: CRUD操作の実装

```csharp
public class EmployeeRepository : IEmployeeRepository
{
    private readonly DbContext _context;
    private readonly EmployeeMapper _mapper;
    private readonly IClock _clock;
    
    public async Task<Employee?> GetByIdAsync(EmployeeId id)
    {
        var dbModel = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == id.Value);
        
        return dbModel != null ? _mapper.ToDomainEntity(dbModel) : null;
    }
    
    public async Task AddAsync(Employee employee)
    {
        var dbModel = _mapper.ToDbModel(employee);
        // 監査カラムを設定
        var now = _clock.JstNow;
        dbModel.CreatedAt = now.Value;
        dbModel.CreatedBy = SystemUserId;
        
        _context.Employees.Add(dbModel);
        await _context.SaveChangesAsync();
    }
    
    // 他のメソッド...
}
```

---

## 🗄️ DB スキーマ（参考）

### t_employees テーブル

```sql
CREATE TABLE [dbo].[t_employees] (
    [row_id] [bigint] NOT NULL PRIMARY KEY,
    [employee_id] [uniqueidentifier] NOT NULL UNIQUE,
    [employee_code_division] [nvarchar](1) NOT NULL,
    [employee_code_number] [int] NOT NULL,
    [person_row_id] [bigint] NOT NULL,
    
    -- 監査カラム
    [created_at] [datetime2](7) NOT NULL,
    [created_by] [bigint] NOT NULL,
    [updated_at] [datetime2](7) NULL,
    [updated_by] [bigint] NULL,
    [deleted_at] [datetime2](7) NULL,
    [deleted_by] [bigint] NULL,
    [row_version] [timestamp] NOT NULL,
    
    CONSTRAINT [FK_t_employees_m_persons_person_row_id] 
        FOREIGN KEY ([person_row_id]) REFERENCES [dbo].[m_persons]([row_id])
)
```

---

## 📋 実装チェックリスト

### ファイル作成
- [ ] `src/Application/Repositories/IEmployeeRepository.cs`
- [ ] `src/Contexts/Employee/Employee.Infrastructure/Models/EmployeeDbModel.cs`
- [ ] `src/Contexts/Employee/Employee.Infrastructure/Models/DepartmentMembershipDbModel.cs`
- [ ] `src/Contexts/Employee/Employee.Infrastructure/Models/RoleAssignmentDbModel.cs`
- [ ] `src/Contexts/Employee/Employee.Infrastructure/Models/PermissionAssignmentDbModel.cs`
- [ ] `src/Contexts/Employee/Employee.Infrastructure/Mappers/EmployeeMapper.cs`
- [ ] `src/Contexts/Employee/Employee.Infrastructure/Repositories/EmployeeRepository.cs`

### テスト作成
- [ ] `tests/Contexts/Employee.Infrastructure.Tests/Repositories/EmployeeRepositoryTests.cs`
- [ ] `tests/Contexts/Employee.Infrastructure.Tests/Mappers/EmployeeMapperTests.cs`

### テスト項目
- [ ] GetByIdAsync: 存在する ID で検索
- [ ] GetByIdAsync: 存在しない ID で検索
- [ ] AddAsync: 新規 Employee を登録
- [ ] UpdateAsync: 既存 Employee を更新
- [ ] DeleteAsync: Employee を論理削除（deleted_at を設定）
- [ ] Mapper: Entity → DbModel 変換
- [ ] Mapper: DbModel → Entity 変換

---

## 🔄 実装順序

1. **DbModel クラス作成**（ DB スキーママッピング）
2. **IEmployeeRepository 定義**（Application層）
3. **Mapper 実装**（変換ロジック）
4. **Repository 実装**（CRUD操作）
5. **テスト作成**（TDD）

---

## 参考資料

- Repository パターンガイド: `docs/Assistance/Guides/Repository_パターンガイド.md`
- Mapper パターンガイド: `docs/Assistance/Guides/Mapper_パターンガイド.md`
- DbModel 設計ルール: `docs/Assistance/Guides/DbModel_設計ルール.md`
- CarPreferences.Infrastructure: `src/Contexts/Samples/CarPreferences.Infrastructure/`（実装例）
