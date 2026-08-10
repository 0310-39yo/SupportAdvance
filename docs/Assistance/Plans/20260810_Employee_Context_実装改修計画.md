# Employee Context — 実装・改修計画

**プロジェクト:** SupportAdvance  
**フェーズ:** 実装計画  
**作成日:** 2026-08-10  
**実装戦略:** ボトムアップ（ValueObject → Entity → Repository → Use Case）

---

## 📋 目次

1. [実装戦略](#実装戦略)
2. [Phase 1: ValueObject 実装](#phase-1-valueobject-実装)
3. [Phase 2: Entity 実装](#phase-2-entity-実装)
4. [Phase 3: Repository 実装](#phase-3-repository-実装)
5. [Phase 4: Use Case 実装](#phase-4-use-case-実装)
6. [Phase 5: 既存 BC 統合](#phase-5-既存-bc-統合)
7. [テスト戦略](#テスト戦略)
8. [リスク・懸念事項](#リスク懸念事項)

---

## 実装戦略

### ボトムアップアプローチの利点

| 観点 | メリット |
|------|---------|
| **依存関係** | 下層から上層へ進むため、依存関係が明確 |
| **テスト** | 各層を独立してテスト可能 |
| **統合** | 上層で統合時にエラーが見つかりやすい |
| **並列実装** | ValueObject 実装中に Entity 設計を詳細化できる |

### 段階的統合の方針

**Phase 1-4:** Employee Context を独立して実装  
**Phase 5:** 既存 BC（CarPreferences など）を新設計に移行

---

## Phase 1: ValueObject 実装

### 目標
すべての ValueObject を実装し、ドメイン層の基盤を整える。

### 作業項目

#### 1.1 創建者系 ValueObject（最優先）

```
src/Contexts/Employee/Domain/ValueObjects/Audit/
├── CreatedBy.cs（新規、優先度：最高）
├── UpdatedBy.cs（新規、優先度：高）
└── DeletedBy.cs（新規、優先度：高）
```

**実装順序（最初に実装）:**
1. **CreatedBy**（従業員 row_id、必須）
2. **UpdatedBy**（従業員 row_id、オプション）
3. **DeletedBy**（従業員 row_id、オプション）

**実装のポイント:**
```csharp
// CreatedBy: 必須パターン（null は失敗）
public static bool TryFrom(long? input, out CreatedBy result)
{
    result = null!;
    if (!input.HasValue) return false;
    try { result = From(input.Value); return true; }
    catch { return false; }
}

// UpdatedBy: オプションパターン（null は Unset に変換）
public static bool TryFrom(long? input, out UpdatedBy result)
{
    if (!input.HasValue)
    {
        result = Unset();  // ← null を Unset に変換
        return true;
    }
    try { result = From(input.Value); return true; }
    catch { return false; }
}
```

---

#### 1.2 識別子系 ValueObject

```
src/Contexts/Employee/Domain/ValueObjects/Identifiers/
├── EmployeeId.cs（既存の移動・修正）
├── EmployeeCode.cs（既存の移動）
├── EmployeeDivision.cs（既存の移動）
├── EmployeeRowId.cs（既存の移動）
├── DepartmentCode.cs（新規）
├── DepartmentRowId.cs（新規）
├── DepartmentMembershipId.cs（新規）
├── RoleAssignmentId.cs（新規、仮実装対象）
├── PermissionAssignmentId.cs（新規、仮実装対象）
├── RoleCode.cs（新規、仮実装対象）
└── PermissionCode.cs（新規、仮実装対象）
```

**実装順序:**
1. EmployeeCode, EmployeeDivision, EmployeeId（既存の確認・修正）
2. DepartmentCode（新規）
3. 各 Id 系（DepartmentMembershipId など）

**確認作業:**
- [ ] 既存 ValueObject が要件に合致しているか確認
- [ ] EmployeeNumber は削除、EmployeeId に統一
- [ ] 新規 ValueObject の実装

---

#### 1.3 ビジネス値 ValueObject

```
src/Contexts/Employee/Domain/ValueObjects/Employee/
├── RetiredAt.cs（null厳格性）
├── ExpirationDate.cs（null厳格性）
└── EffectiveDate.cs（null厳格性）

src/Contexts/Employee/Domain/ValueObjects/Department/
├── DepartmentName.cs
├── ShortDepartmentName.cs（null厳格性）
├── Level.cs（階層レベル：0-4）
├── ParentDepartmentCode.cs（null厳格性）
├── ManagerEmployeeRowId.cs（null厳格性）
└── AbolishedOn.cs（null厳格性）
```

**実装順序:**
1. 基本型（DepartmentName など）
2. null厳格性型（RetiredAt, ExpirationDate など）

**テスト:**
- [ ] 値の範囲検証（例：hierarchy_level 0-4）
- [ ] null厳格性の IsSet フラグ動作
- [ ] 等価性判定（ValueObject.Equals）

---

### テスト計画（Phase 1）

**ユニットテスト:**
```
tests/Contexts/Employee.Domain.Tests/ValueObjects/Identifiers/
├── EmployeeCodeTests.cs
├── EmployeeDivisionTests.cs
├── DepartmentCodeTests.cs
└── ...

tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/
├── RetiredAtTests.cs
├── ExpirationDateTests.cs
└── ...
```

**テストケース例:**
```csharp
[Fact]
public void EmployeeCode_From_InvalidRange_ThrowsException()
{
    // Arrange
    var division = EmployeeDivision.RegularEmployee();
    var number = EmployeeNumber.From(9999);  // M系では無効な範囲
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() => EmployeeCode.From(division, number));
}

[Fact]
public void RetiredAt_Unset_IsSetIsFalse()
{
    // Arrange & Act
    var retiredAt = RetiredAt.Unset();
    
    // Assert
    Assert.False(retiredAt.IsSet);
}
```

---

### 成果物
- ✅ 創建者系 ValueObject（CreatedBy/UpdatedBy/DeletedBy）が実装・テスト完了
- ✅ 全識別子系 ValueObject が実装・テスト完了
- ✅ 全ビジネス値 ValueObject が実装・テスト完了
- ✅ 要件設計書に記載のすべての値が型で表現される

### 所要期間
**1-2週間**（ValueObject の数、複雑さ次第）  
※ 創建者系 ValueObject は Phase の最初に優先実装

---

## Phase 2: Entity 実装

### 目標
Employee 集約と子Entity を実装。ドメインロジックの中核を構築。

### 作業項目

#### 2.1 子Entity の実装

```
src/Contexts/Employee/Domain/Entities/Employee/
├── DepartmentMembership.cs（本実装）
├── RoleAssignment.cs（仮実装）
└── PermissionAssignment.cs（仮実装）
```

**🚧 仮実装について:**
- **DepartmentMembership**: 本実装（部署管理は必須機能）
- **RoleAssignment**: 仮実装（ログイン・権限管理は後フェーズ）
- **PermissionAssignment**: 仮実装（ログイン・権限管理は後フェーズ）

仮実装では、基本的なプロパティと Equals/GetHashCode のみ実装し、ビジネスロジック（有効期限チェック等）は後フェーズで詳細実装。

**実装順序:**
1. DepartmentMembership（最もシンプル、本実装）
2. RoleAssignment（仮実装 - 基本枠組みのみ）
3. PermissionAssignment（仮実装 - 基本枠組みのみ）

**各 Entity の構成:**
```csharp
public class DepartmentMembership : Entity<DepartmentMembershipId>
{
    public DepartmentMembershipId Id { get; private set; }
    public DepartmentCode DepartmentCode { get; private set; }
    public bool IsPrimary { get; private set; }
    public ExpirationDate ExpiredAt { get; private set; }
    
    public bool IsActive(LocalDateTime asOf) 
        => !ExpiredAt.IsExpired(asOf);
}
```

---

#### 2.2 Employee Entity の実装

```
src/Contexts/Employee/Domain/Entities/Employee/
└── Employee.cs
```

**構成:**
```csharp
public sealed class Employee : Entity<EmployeeId>
{
    public EmployeeId EmployeeId => Id;
    public EmployeeCode EmployeeCode { get; private set; }
    public EmployeeRowId EmployeeRowId { get; private set; }
    public RetiredAt RetiredAt { get; private set; }
    
    // 監査情報（Entity基底クラスから継承）
    public CreatedAt CreatedAt { get; private set; }
    public CreatedBy CreatedBy { get; private set; }
    public UpdatedAt UpdatedAt { get; private set; }
    public UpdatedBy UpdatedBy { get; private set; }
    public DeletedAt DeletedAt { get; private set; }
    public DeletedBy DeletedBy { get; private set; }
    
    internal List<DepartmentMembership> DepartmentMemberships { get; private set; }
    internal List<RoleAssignment> RoleAssignments { get; private set; }
    internal List<PermissionAssignment> PermissionAssignments { get; private set; }
    
    // ビジネスロジック
    public bool IsActive() => !RetiredAt.IsSet;
    
    public void AddDepartmentMembership(DepartmentCode code, bool isPrimary)
    {
        // ...
    }
}
```

---

### テスト計画（Phase 2）

**ユニットテスト:**
```
tests/Contexts/Employee.Domain.Tests/Entities/
├── EmployeeTests.cs
├── DepartmentMembershipTests.cs
├── RoleAssignmentTests.cs
└── PermissionAssignmentTests.cs
```

**テストケース例:**
```csharp
[Fact]
public void Employee_AddDepartmentMembership_AddsToList()
{
    // Arrange
    var employee = Employee.Create(...);
    var deptCode = DepartmentCode.From("G100");
    
    // Act
    employee.AddDepartmentMembership(deptCode, isPrimary: true);
    
    // Assert
    Assert.Single(employee.DepartmentMemberships);
    Assert.True(employee.DepartmentMemberships[0].IsPrimary);
}

[Fact]
public void DepartmentMembership_IsActive_ReturnsTrueWhenNotExpired()
{
    // Arrange
    var membership = new DepartmentMembership(...);
    var now = LocalDateTime.Now;
    
    // Act & Assert
    Assert.True(membership.IsActive(now));
}
```

---

### 成果物
- ✅ Employee Entity と子Entity が実装完了
- ✅ すべてのビジネスロジックが Domain層に実装
- ✅ null厳格性が確保される

### 所要期間
**1-2週間**

---

## Phase 3: Repository 実装

### 目標
Employee の永続化層を実装。DB との相互作用を確立。

### 作業項目

#### 3.1 Repository Interface の定義

```
src/Contexts/Employee/Application/Repositories/
└── IEmployeeRepository.cs
```

```csharp
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(EmployeeId employeeId);
    Task<Employee?> GetByCodeAsync(EmployeeCode code);
    Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId);
    Task<List<Employee>> GetAllAsync();
    Task<List<Employee>> SearchAsync(EmployeeSearchCriteria criteria);
    
    // 属性取得
    Task<PersonDto> GetPersonAsync(EmployeeId employeeId);
    Task<List<DepartmentMembership>> GetDepartmentsAsync(EmployeeId employeeId);
    Task<List<RoleAssignment>> GetRolesAsync(EmployeeId employeeId);
    Task<List<PermissionAssignment>> GetPermissionsAsync(EmployeeId employeeId);
    
    // CRUD
    Task AddAsync(Employee entity);
    Task UpdateAsync(Employee entity);
    Task DeleteAsync(EmployeeId employeeId);
}
```

---

#### 3.2 Repository 実装

```
src/Contexts/Employee/Infrastructure/Repositories/
└── EmployeeRepository.cs
```

**主要な責務:**
1. DbModel ↔ Entity の双方向変換（Mapper）
2. DB アクセス（IDataAccess 経由）
3. null厳格性の保証（DB の NULL/MaxValue を Domain の Unset() に変換）
4. トランザクション管理

**実装のポイント:**
```csharp
public class EmployeeRepository : IEmployeeRepository
{
    private readonly IDataAccess _dataAccess;
    private readonly IEmployeeMapper _mapper;
    
    public async Task<Employee?> GetByIdAsync(EmployeeId employeeId)
    {
        var dbModel = await _dataAccess.QuerySingleOrDefaultAsync(
            "SELECT * FROM m_employees WHERE employee_id = @id AND deleted_at IS NULL",
            new { id = employeeId.Value });
        
        return dbModel != null ? _mapper.ToDomainEntity(dbModel) : null;
    }
    
    public async Task<PersonDto> GetPersonAsync(EmployeeId employeeId)
    {
        var employee = await GetByIdAsync(employeeId);
        if (employee == null) return null;
        
        // t_employee_attributes から Person 属性を取得
        var personDbRow = await _dataAccess.QuerySingleOrDefaultAsync(
            @"SELECT p.* FROM m_persons p
              JOIN t_employee_attributes ea 
              ON p.row_id = ea.attribute_row_id
              WHERE ea.employee_row_id = @employeeRowId
              AND ea.attribute_type = 'Person'
              AND ea.deleted_at IS NULL",
            new { employeeRowId = employee.EmployeeRowId.Value });
        
        return _mapper.ToPersonDto(personDbRow);
    }
}
```

---

#### 3.3 Mapper の実装

```
src/Contexts/Employee/Infrastructure/Mappers/
└── EmployeeMapper.cs
```

**責務:**
- DbModel → Employee Entity への変換
- Entity → DbModel への変換
- DB の NULL/MaxValue → Domain の Unset() への変換

```csharp
public class EmployeeMapper : IEmployeeMapper
{
    private readonly IClock _clock;
    
    public Employee ToDomainEntity(EmployeeDbModel dbModel)
    {
        // DB値を Domain Entity に変換
        var retiredAt = dbModel.RetiredOn == DateTime.MaxValue
            ? RetiredAt.Unset()
            : RetiredAt.From(new LocalDateTime(dbModel.RetiredOn));
        
        // 監査情報の変換
        CreatedAt.TryFromDbValue(dbModel.CreatedAt, out var createdAt);
        CreatedBy.TryFrom(dbModel.CreatedBy, out var createdBy);
        UpdatedAt.TryFromDbValue(dbModel.UpdatedAt, out var updatedAt);
        UpdatedBy.TryFrom(dbModel.UpdatedBy, out var updatedBy);
        DeletedAt.TryFromDbValue(dbModel.DeletedAt, out var deletedAt);
        DeletedBy.TryFrom(dbModel.DeletedBy, out var deletedBy);
        
        return Employee.Reconstruct(
            employeeId: EmployeeId.From(dbModel.EmployeeId),
            employeeCode: EmployeeCode.FromDbValues(dbModel.EmployeeDivision, dbModel.EmployeeId),
            employeeRowId: EmployeeRowId.From(dbModel.RowId),
            retiredAt: retiredAt,
            createdAt: createdAt,
            createdBy: createdBy,
            updatedAt: updatedAt,
            updatedBy: updatedBy,
            deletedAt: deletedAt,
            deletedBy: deletedBy);
    }
    
    public EmployeeDbModel ToDbModel(Employee entity)
    {
        return new EmployeeDbModel
        {
            RowId = entity.EmployeeRowId.Value,
            EmployeeId = entity.EmployeeId.Value,
            EmployeeDivision = entity.EmployeeCode.Division.Value,
            RetiredOn = entity.RetiredAt.IsSet ? entity.RetiredAt.Value.Value : DateTime.MaxValue,
            CreatedAt = entity.CreatedAt.Value.Value,
            CreatedBy = entity.CreatedBy.Value,
            UpdatedAt = entity.UpdatedAt.HasUpdated ? entity.UpdatedAt.Value.Value : (DateTime?)null,
            UpdatedBy = entity.UpdatedBy.HasUpdated ? entity.UpdatedBy.Value : (long?)null,
            DeletedAt = entity.DeletedAt.IsDeleted ? entity.DeletedAt.Value.Value : (DateTime?)null,
            DeletedBy = entity.DeletedBy.IsDeleted ? entity.DeletedBy.Value : (long?)null
        };
    }
}
```

---

### テスト計画（Phase 3）

**統合テスト（実DB接続）:**
```
tests/Contexts/Employee.Infrastructure.Tests/Repositories/
└── EmployeeRepositoryTests.cs
```

**テストセットアップ:**
- テスト用 in-memory DB または テスト DB を使用
- Transaction で各テスト後にロールバック

**テストケース例:**
```csharp
[Fact]
public async Task GetByIdAsync_ExistingEmployee_ReturnsEntity()
{
    // Arrange
    var testData = await InsertTestEmployee();
    var employeeId = testData.EmployeeId;
    
    // Act
    var employee = await _repository.GetByIdAsync(employeeId);
    
    // Assert
    Assert.NotNull(employee);
    Assert.Equal(employeeId, employee.EmployeeId);
}

[Fact]
public async Task GetPersonAsync_ReturnsPersonInfo()
{
    // Arrange
    var employee = await InsertTestEmployee();
    
    // Act
    var personDto = await _repository.GetPersonAsync(employee.EmployeeId);
    
    // Assert
    Assert.NotNull(personDto);
    Assert.NotNull(personDto.FullName);
}
```

---

### 成果物
- ✅ Repository が実装・テスト完了
- ✅ DB との双方向変換が機能
- ✅ Mapper で null厳格性が保証される

### 所要期間
**2-3週間**（DB操作の複雑さ次第）

---

## Phase 4: Use Case 実装

### 目標
Application層で Employee の操作を Use Case として実装。

### 作業項目

#### 4.1 基本的な Use Case

```
src/Contexts/Employee/Application/UseCases/
├── GetEmployeeUseCase.cs
├── GetEmployeeListUseCase.cs
├── GetEmployeeWithDepartmentsUseCase.cs
├── CreateEmployeeUseCase.cs
├── UpdateEmployeeUseCase.cs
└── ...
```

**実装順序:**
1. **GetEmployeeUseCase**（読み取り、最もシンプル）
2. **GetEmployeeListUseCase**（一覧取得）
3. **CreateEmployeeUseCase**（作成）
4. **UpdateEmployeeUseCase**（更新）

**実装例:**
```csharp
public class GetEmployeeWithDepartmentsUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    
    public GetEmployeeWithDepartmentsUseCase(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }
    
    public async Task<EmployeeDetailDto> ExecuteAsync(EmployeeId employeeId)
    {
        // 1. Employee 集約を取得
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
            throw new EmployeeNotFoundException();
        
        // 2. 部署情報を取得
        var departments = await _employeeRepository.GetDepartmentsAsync(employeeId);
        var primaryDept = departments.FirstOrDefault(d => d.IsPrimary);
        
        // 3. DTO に変換して返す
        return new EmployeeDetailDto(employee, departments);
    }
}
```

---

#### 4.2 権限関連 Use Case

```
src/Contexts/Employee/Application/UseCases/
├── AddEmployeeDepartmentUseCase.cs
├── AddEmployeeRoleUseCase.cs
├── AddEmployeePermissionUseCase.cs
└── ...
```

**実装ポイント:**
- Application層で Entity メソッドを呼び出し
- Repository に保存
- 権限チェック（PermissionPolicy）

```csharp
public class AddEmployeeDepartmentUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPermissionPolicy _permissionPolicy;
    
    public async Task ExecuteAsync(EmployeeId targetId, DepartmentCode deptCode, bool isPrimary)
    {
        // 1. 実行者の権限をチェック
        var operatorEmployee = await _employeeRepository.GetByIdAsync(_currentUser.EmployeeId);
        var targetEmployee = await _employeeRepository.GetByIdAsync(targetId);
        
        if (!_permissionPolicy.CanEditPermission(operatorEmployee, targetEmployee, "EditDepartment"))
            throw new PermissionDeniedException();
        
        // 2. Entity で状態を変更
        targetEmployee.AddDepartmentMembership(deptCode, isPrimary);
        
        // 3. 永続化
        await _employeeRepository.UpdateAsync(targetEmployee);
    }
}
```

---

### テスト計画（Phase 4）

**ユニットテスト（Repository モック）:**
```
tests/Contexts/Employee.Application.Tests/UseCases/
├── GetEmployeeUseCaseTests.cs
├── CreateEmployeeUseCaseTests.cs
└── ...
```

**テストケース例:**
```csharp
[Fact]
public async Task GetEmployeeUseCase_ReturnsEmployeeDto()
{
    // Arrange
    var mockRepository = new Mock<IEmployeeRepository>();
    var employee = Employee.Create(...);
    mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<EmployeeId>()))
        .ReturnsAsync(employee);
    
    var useCase = new GetEmployeeUseCase(mockRepository.Object);
    
    // Act
    var result = await useCase.ExecuteAsync(employee.EmployeeId);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(employee.EmployeeId.Value, result.EmployeeId);
}

[Fact]
public async Task AddEmployeeDepartmentUseCase_WithoutPermission_ThrowsException()
{
    // Arrange
    var mockRepository = new Mock<IEmployeeRepository>();
    var mockPermissionPolicy = new Mock<IPermissionPolicy>();
    mockPermissionPolicy.Setup(p => p.CanEditPermission(It.IsAny<Employee>(), It.IsAny<Employee>(), It.IsAny<string>()))
        .Returns(false);
    
    var useCase = new AddEmployeeDepartmentUseCase(mockRepository.Object, mockPermissionPolicy.Object);
    
    // Act & Assert
    await Assert.ThrowsAsync<PermissionDeniedException>(async () =>
        await useCase.ExecuteAsync(targetId, deptCode, isPrimary: true));
}
```

---

### 成果物
- ✅ 基本的な Use Case が実装・テスト完了
- ✅ 権限チェックが機能
- ✅ DTO 変換が機能

### 所要期間
**2-3週間**

---

## Phase 5: 既存 BC 統合

### 目標
既存の CarPreferences など他の BC を新 Employee Context 設計に統合。

### 作業項目

#### 5.1 CarPreferences の移行

**現在:** Employee Entity が Samples/CarPreferences に存在  
**移行先:** Contexts/Employee に統合

**作業:**
1. [ ] 既存 Employee Entity を新設計に置き換え
2. [ ] CarPreferences の参照を Employee BC の Use Case に更新
3. [ ] テスト更新

---

#### 5.2 SharedKernel からの移動

**移動対象:**
```
SharedKernel/ValueObjects/Identifiers/
  ├─ EmployeeId.cs → Employee/Domain/ValueObjects/Identifiers/
  ├─ EmployeeCode.cs → Employee/Domain/ValueObjects/Identifiers/
  ├─ EmployeeDivision.cs → Employee/Domain/ValueObjects/Identifiers/
  ├─ EmployeeRowId.cs → Employee/Domain/ValueObjects/Identifiers/
  ├─ PersonRowId.cs → Employee/Domain/ValueObjects/Identifiers/
  └─ ...
```

**手順:**
1. [ ] Contexts/Employee にコピー
2. [ ] namespace を更新
3. [ ] SharedKernel から削除
4. [ ] 依存関係をリファクタ（grep で確認）
5. [ ] テスト実行

---

### テスト計画（Phase 5）

**回帰テスト:**
- CarPreferences のすべてのテストが通ることを確認
- 統合テスト（Employee BC + CarPreferences）

---

### 成果物
- ✅ Employee Context が他 BC から独立
- ✅ 既存 BC が新設計に統合
- ✅ すべてのテストが通る

### 所要期間
**1-2週間**

---

## テスト戦略

### 層別テスト方針

| 層 | テスト種別 | スコープ | モック |
|----|----------|--------|--------|
| **Domain** | ユニットテスト | ValueObject、Entity | なし |
| **Application** | ユニットテスト | Use Case | Repository をモック |
| **Infrastructure** | 統合テスト | Repository | 実DB（テスト専用） |

### テストの優先順位

1. **Phase 1: ValueObject テスト**（最も重要）
   - 範囲検証
   - null厳格性
   - 等価性

2. **Phase 2: Entity テスト**
   - ビジネスロジック
   - 集約の一貫性
   - 子Entity 管理

3. **Phase 3: Repository テスト**
   - DB との相互作用
   - Mapper の正確性
   - トランザクション

4. **Phase 4: Use Case テスト**
   - ワークフロー
   - エラーハンドリング
   - 権限チェック

### テスト環境

**開発時:**
- in-memory DB（高速）
- ローカル SQL Server（実DB との一貫性確認）

**CI/CD:**
- Docker コンテナの SQL Server
- 自動テスト実行

---

## リスク・懸念事項

### 🚨 リスク1: SharedKernel からの移動で依存関係が複雑化

**懸念:** ValueObject を共通から Employee Context に移動すると、他 BC が参照できなくなる

**対策:**
- Phase 5 で慎重にリファクタ
- grep で依存関係を全検索
- テストで回帰を確認

---

### 🚨 リスク2: Mapper での null 厳格性変換ミス

**懸念:** DB の NULL/MaxValue → Domain の Unset() への変換で誤りがあると、ドメインに null が混入

**対策:**
- Mapper のテストを徹底
- 各 ValueObject の TryFromDbValue を活用
- Code Review で Mapper 実装を検証

---

### 🚨 リスク3: スパース構造（t_employee_attributes）での NULLカラム処理

**懸念:** attribute_type に応じて NULL になるカラムの扱いが不明確だと、バグの温床に

**対策:**
- Repository で attribute_type に応じた変換を明示的に実装
- 型チェック（例：`if (row.AttributeType == "DepartmentMembership")`）を厳密に
- テストで各パターンを網羅

---

### 🚨 リスク4: FK の ON DELETE CASCADE でデータが意図せず削除される

**懸念:** m_departments 削除時に、t_employee_attributes の子部署記録も自動削除

**対策:**
- CASCADE の設計を再確認（想定通りなら OK）
- テストで CASCADE 動作を明示的に検証
- 重要な削除操作は管理画面で確認を求める

---

### 💡 懸念1: CarPreferences との統合タイミング

**懸念:** 既存テストが失敗する可能性

**対策:**
- Phase 5 で十分なテスト時間を確保
- Hotfix ブランチを準備（緊急時）

---

### 💡 懸念2: 権限チェック（PermissionPolicy）の実装タイミング

**懸念:** Phase 4 で PermissionPolicy の実装が間に合わない

**対策:**
- Crosscutting 層の実装を Phase 4 と並行して進める
- 初期は簡易版（常に許可）で Phase 4 を進める
- 後で詳細実装に置き換え

---

## 全体スケジュール

| Phase | 作業 | 期間 | 開始 |
|-------|------|------|------|
| **1** | ValueObject | 1-2週間 | Week 1 |
| **2** | Entity | 1-2週間 | Week 2-3 |
| **3** | Repository | 2-3週間 | Week 4-6 |
| **4** | Use Case | 2-3週間 | Week 7-9 |
| **5** | 既存 BC 統合 | 1-2週間 | Week 10-11 |
| | **計** | **8-12週間** | |

---

## 成果物チェックリスト

### Phase 1
- [ ] ValueObject 実装（全数）
- [ ] ValueObject テスト（全カバレッジ）
- [ ] 既存 ValueObject の確認・修正完了

### Phase 2
- [ ] Entity 実装
- [ ] 子Entity 実装
- [ ] Entity テスト
- [ ] ビジネスロジック検証

### Phase 3
- [ ] Repository 実装
- [ ] Mapper 実装
- [ ] 統合テスト（実DB）
- [ ] 性能確認

### Phase 4
- [ ] 基本 Use Case 実装
- [ ] 権限関連 Use Case 実装
- [ ] ユニットテスト
- [ ] 権限チェック機能確認

### Phase 5
- [ ] 既存 BC の移行完了
- [ ] SharedKernel クリーンアップ
- [ ] 全テスト通過
- [ ] ドキュメント更新

---

## 更新履歴

| 日付 | 更新内容 |
|-----|--------|
| 2026-08-10（後）| 創建者系 ValueObject（CreatedBy/UpdatedBy/DeletedBy）を Phase 1 最優先に、仮実装表記追加、Mapper に監査情報変換を組み込み |
| 2026-08-10 | 初版作成。5つの Phase、テスト戦略、リスク分析 |
