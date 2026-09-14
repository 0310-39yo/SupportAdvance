# 詳細改修計画書：RowId 事前採番と Entity<TId> 型制約追加

**作成日:** 2026-08-11  
**更新日:** 2026-08-12  
**対象:** Employee Context を中心とした Entity/RowId 架構整備  
**総所要時間:** 約 6.5 時間  
**テスト数:** 504 個（全テスト実行）

---

## 🎯 SupportAdvance の大原則（設計哲学）

このプロジェクトは以下の大原則に基づいています：

1. **RowId の二重責務（許容）**

   - 主キー（システム基本ID）
   - 時系列マーカー（採番順序 = 業務日付）
   - 朝一の採番値を記憶 → 翌日は「RowId X～Y = その日のデータ」と特定可能

2. **論理削除の必須化**

   - 物理削除禁止（データ欠損を絶対許さない）
   - RowId の連番が保持される → 時系列完全性を守る

3. **RowId 事前採番**

   - DB 保存前に Sequence から RowId を確保
   - Entity 生成時点で同一性（RowId）を確定
   - ドメインイベント発行時に RowId が確定している

4. **業務排他制御**

   - 別テーブル（m_row_id_locks）で使用状態管理
   - 編集中の Entity に他のユーザーがアクセス時に「使用中」を表示

---

## 📋 改修の全体像

### 変更内容サマリー

| フェーズ    | 改修内容                                   | ファイル数 | テスト数 | 所要時間 |
| ----------- | ------------------------------------------ | ---------- | -------- | -------- |
| **Phase 1** | SharedKernel.RowId を abstract 化          | 1          | 7        | 30分     |
| **Phase 2** | Employee Context の 5つの RowId 型改修     | 5          | 112      | 1時間    |
| **Phase 3** | Entity<TId> 型制約 + DomainEventId 改修    | 3          | 27       | 30分     |
| **Phase 4** | Employee Entity 再設計 + RetiredOn         | 4          | 52       | 1.5時間  |
| **Phase 5** | Mapper/Repository 改修                     | 4          | 19       | 1時間    |
| **Phase 6** | Application層 RowId 事前採番               | 3          | 24       | 30分     |
| **Phase 7** | テスト実行・検証                           | -          | 504      | 1時間    |

### 改修対象ファイル一覧（参考）

| # | 層 | ファイル | 対象 |
|----|-----|---------|------|
| **1** | SharedKernel | `RowId.cs` | abstract 化 |
| **2** | Domain | `PersonRowId.cs`, `DepartmentRowId.cs`, `EmployeeRowId.cs`, `ManagerEmployeeRowId.cs`, `ParentDepartmentRowId.cs` | RowId 継承 |
| **3** | SharedKernel | `Entity.cs` | where TId : ValueObject 追加 |
| **3** | SharedKernel | `AggregateRoot.cs` | where TId : ValueObject 追加 |
| **3** | SharedKernel | `IDomainEvent.cs` | **EventId 削除、AggregateRootId(long) に統一** |
| **3** | SharedKernel | `DomainEventId.cs` | **削除 または EventId 削除** |
| **4** | Domain | `Employee.cs` | Entity<EmployeeId> → AggregateRoot<EmployeeRowId> |
| **4** | Domain | `RetiredOn.cs` | **新規作成** |
| **4** | Domain | `DepartmentMembership.cs`, `RoleAssignment.cs`, `PermissionAssignment.cs` | Entity<EmployeeRowId> に統一 |
| **5** | Infrastructure | `EmployeeMapper.cs` | EmployeeRowId マッピング |
| **5** | Infrastructure | `EmployeeRepository.cs` | EmployeeRowId キー処理 |
| **6** | Application | `ISequenceProvider.cs` | **新規作成** |
| **6** | Application | `CreateEmployeeUseCase.cs` | RowId 事前採番フロー |
| **6** | Infrastructure | `EmployeeSequenceProvider.cs` | **新規作成** |

---

## 🔧 Phase 1: SharedKernel.RowId の抽象化

### 目的

sealed → abstract に変更し、各 Context の RowId が継承可能にする

### 対象ファイル

- `src/SharedKernel/ValueObjects/Identifiers/RowId.cs`

### 改修内容

**現在の実装:**

```csharp
public sealed class RowId : ValueObject, IEquatable<RowId>
{
    private readonly long _value;
    public static RowId From(long value) { ... }
    public static RowId New() { ... }
}
```

**改修後:**

```csharp
public abstract class RowId : PrimitiveValueObject<long>, IEquatable<RowId>
{
    protected RowId(long value, bool isSet) : base(value, isSet) { }
    protected RowId(bool isSet) : base(isSet) { }
    
    public long Value => ValueField;
    public abstract override void Validate(long normalized);
    public override string ToString() => ValueField.ToString();
    // Equals/GetHashCode は基底クラスから継承
}
```

### テスト方法

```bash
dotnet test tests/SharedKernel.Tests/ValueObjects/Identifiers/ --filter "RowId"
# 期待結果: 7 テスト成功
```

### リスク評価

- **リスク:** SharedKernel.RowId の既存使用箇所への影響（Infrastructure 層の DB採番処理）
- **対策:** Phase 2 実行後にビルド確認

### チェックポイント

- [ ] ビルド成功
- [ ] RowIdTests (7個) 成功

---

## 🔧 Phase 2: Employee Context の 5つの RowId 型改修

### 目的

既存の RowId 型を RowId を継承する形にリファクタ

### 対象ファイル

#### グループA: 必須型（3個）

- `src/Contexts/Employee/Employee.Domain/ValueObjects/Employee/PersonRowId.cs`
- `src/Contexts/Employee/Employee.Domain/ValueObjects/Employee/DepartmentRowId.cs`
- `src/Contexts/Employee/Employee.Domain/ValueObjects/Employee/EmployeeRowId.cs`

#### グループB: オプション型（2個）

- `src/Contexts/Employee/Employee.Domain/ValueObjects/Employee/ManagerEmployeeRowId.cs`
- `src/Contexts/Employee/Employee.Domain/ValueObjects/Employee/ParentDepartmentRowId.cs`

### 改修パターン

#### グループA: 必須型の改修パターン

**現在:**

```csharp
public sealed class PersonRowId : PrimitiveValueObject<long>, IEquatable<PersonRowId>
{
    public const long MinValue = 1L;
    public long Value => ValueField;
    
    private PersonRowId(long value) : base(value, true) { }
    
    public static PersonRowId From(long value) => new(value);
    public static bool TryFrom(long value, out PersonRowId result) { ... }
    public static bool TryFromDbValue(long value, out PersonRowId result) { ... }
    
    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
            throw new ArgumentOutOfRangeException(...);
    }
}
```

**改修後:**

```csharp
public sealed class PersonRowId : RowId, IEquatable<PersonRowId>
{
    public const long MinValue = 1L;
    
    private PersonRowId(long value) : base(value, true) { }
    
    public static PersonRowId From(long value) => new(value);
    public static bool TryFrom(long value, out PersonRowId result)
    {
        result = null!;
        try { result = From(value); return true; }
        catch (ArgumentOutOfRangeException) { return false; }
    }
    
    public static bool TryFromDbValue(long value, out PersonRowId result)
    {
        result = null!;
        try { result = From(value); return true; }
        catch (ArgumentOutOfRangeException) { return false; }
    }
    
    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
            throw new ArgumentOutOfRangeException(...);
    }
    
    // IEquatable 実装は削除可能（基底クラスで十分）
    public bool Equals(PersonRowId? other) => ... // 残す場合
}
```

**主な変更点:**

1. `: PrimitiveValueObject<long>` → `: RowId`
2. `using` を `ValueObjects.Identifiers` に変更
3. `Abstractions` の using を削除
4. 他の メソッド/プロパティは基本的に同じ

#### グループB: オプション型の改修パターン

**現在:**

```csharp
public sealed class ManagerEmployeeRowId : PrimitiveValueObject<long?>, IEquatable<ManagerEmployeeRowId>
{
    public static ManagerEmployeeRowId Unset() => new(null, false);
    public bool HasManager => IsSet;
    
    // ... TryFrom で null 吸収ロジック ...
}
```

**改修後:**

```csharp
public sealed class ManagerEmployeeRowId : RowId, IEquatable<ManagerEmployeeRowId>
{
    // long? ではなく long を使用（Unset 状態は IsSet=false で表現）
    
    public static ManagerEmployeeRowId Unset() => new(false);  // IsSet=false のコンストラクタを使用
    public bool HasManager => IsSet;
    
    public static bool TryFrom(long? input, out ManagerEmployeeRowId result)
    {
        result = null!;
        if (!input.HasValue)
        {
            result = Unset();
            return true;
        }
        try { result = new(input.Value); return true; }
        catch { return false; }
    }
}
```

**注意点:** オプション型は long ではなく long? ではなく、IsSet フラグで Unset を表現

### 改修手順

1. **PersonRowId を改修** （既に完了）

   ```bash
   # 確認: grep "RowId" src/Contexts/.../PersonRowId.cs
   ```

2. **DepartmentRowId を改修**

   - `PrimitiveValueObject<long>` → `RowId` に変更
   - using を更新

3. **EmployeeRowId を改修**

   - 同じパターン

4. **ManagerEmployeeRowId を改修** （オプション型）

   - PrimitiveValueObject<long?> → RowId に変更
   - Unset() の実装を IsSet フラグベースに変更
   - TryFrom で null 吸収ロジック調整

5. **ParentDepartmentRowId を改修** （オプション型）

   - ManagerEmployeeRowId と同じパターン

### テスト方法

```bash
# 各改修後に実行
dotnet test tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/ --filter "RowId"
# 期待結果: 112 テスト成功（リグレッション 0）
```

### リスク評価

- **リスク:** Validate メソッドが abstract になったため、すべての派生クラスが実装必須
- **対策:** コンパイラが強制。ビルド時に全実装をチェック

### チェックポイント

- [ ] 5つの RowId ファイルすべてが PrimitiveValueObject から RowId 継承に変更
- [ ] 全RowIdTests (112個) 成功
- [ ] ビルド成功（コンパイラエラーなし）

---

## 🔧 Phase 3: Entity<TId> 型制約追加

### 目的

Entity のジェネリック型パラメータに ValueObject 制約を追加

### 対象ファイル

- `src/SharedKernel/Entities/Abstractions/Entity.cs`
- `src/SharedKernel/Entities/Abstractions/AggregateRoot.cs`

### 改修内容

**Entity.cs:**

```csharp
// 現在
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull

// 改修後
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull, ValueObject
```

**AggregateRoot.cs:**

```csharp
// 現在
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull

// 改修後
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull, ValueObject  // 明示的に記載（可読性）
```

### テスト方法

```bash
dotnet test tests/SharedKernel.Tests/Entities/
# 期待結果: 27 テスト成功
```

### リスク評価

- **リスク:** 低（既存すべての ID 型が ValueObject を継承しているため互換性あり）
- **対策:** ビルド時に型チェック

### チェックポイント

- [ ] SharedKernel.Entities のビルド成功
- [ ] Architecture.Tests (27個) 成功
- [ ] Employee.Domain.Entities のビルド成功

---

## 🔧 Phase 4: Employee Entity 再設計

### 目的

Employee Entity を EmployeeRowId ベースに再構築

### 対象ファイル

#### 主要改修

- `src/Contexts/Employee/Employee.Domain/Entities/Employee.cs`

#### 副次改修（子Entity）

- `src/Contexts/Employee/Employee.Domain/Entities/DepartmentMembership.cs`
- `src/Contexts/Employee/Employee.Domain/Entities/RoleAssignment.cs`
- `src/Contexts/Employee/Employee.Domain/Entities/PermissionAssignment.cs`

### 改修内容

#### Employee.cs の全面改修

**現在の構造:**

```csharp
public sealed class Employee : Entity<EmployeeId>  // GUID ベース
{
    public EmployeeCode Code { get; }
    public PersonRowId PersonRowId { get; }
    // ... その他プロパティ ...
}
```

**改修後の構造:**

```csharp
public sealed class Employee : AggregateRoot<EmployeeRowId>  // long ベース
{
    // 同一性を表す ID
    public EmployeeRowId EntId { get; private set; }  // Entity.Id と同期
    
    // ビジネス属性
    public EmployeeDivision Division { get; private set; }
    public EmployeeNumber Number { get; private set; }
    public EmployeeCode Code { get; private set; }
    public PersonRowId PersonRowId { get; private set; }
    
    // 監査フィールド（ValueObject）
    public CreatedAt CreatedAt { get; private set; }
    public CreatedBy CreatedBy { get; private set; }
    public UpdatedAt? UpdatedAt { get; private set; }
    public UpdatedBy? UpdatedBy { get; private set; }
    public DeletedAt? DeletedAt { get; private set; }
    public DeletedBy? DeletedBy { get; private set; }
    
    // 退職情報（新規ValueObject）
    public RetiredOn? RetiredOn { get; private set; }
    
    // ドメインロジック
    public bool IsActive(LocalDateTime asOf)
    {
        if (!RetiredOn.HasRetired)
            return true;
        return asOf < RetiredOn.Value;
    }
    
    // ドメインイベント発行メソッド
    // ドメインイベント識別：EntId（RowId）で識別
    // 理由：Entry が DB 保存前に RowId が確定しているため
    protected void RetireEmployee(LocalDateTime retiredOn, IClock clock)
    {
        RetiredOn = RetiredOn.FromValue(retiredOn);
        this.RaiseDomainEvent(new EmployeeRetiredEvent(
            this.EntId,              // ← RowId で識別可能（事前採番済み）
            this.Division,           // ← ナチュラルキー（可視化用）
            this.Number,             // ← ナチュラルキー（可視化用）
            retiredOn,
            clock.JstNow
        ));
    }
}
```

#### 新規 ValueObject: RetiredOn

**実装場所:** `src/Contexts/Employee/Employee.Domain/ValueObjects/Employee/RetiredOn.cs`

```csharp
public sealed class RetiredOn : ValueObject, IEquatable<RetiredOn>
{
    private readonly LocalDateTime _value;
    
    private RetiredOn(LocalDateTime value, bool isSet)
    {
        _value = value;
        IsSet = isSet;
    }
    
    public LocalDateTime Value => _value;
    public bool HasRetired => IsSet;
    
    public static RetiredOn Unset() => new(LocalDateTime.MinValue, false);
    public static RetiredOn FromValue(LocalDateTime value) => new(value, true);
    
    public static bool TryFromDbValue(DateTime? dbValue, out RetiredOn result)
    {
        result = null!;
        if (!dbValue.HasValue)
        {
            result = Unset();
            return true;
        }
        try
        {
            var localDateTime = new LocalDateTime(dbValue.Value);
            result = FromValue(localDateTime);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return IsSet;
        yield return _value;
    }
    
    public bool Equals(RetiredOn? other) => ... // ValueObject 標準実装
    public override bool Equals(object? obj) => Equals(obj as RetiredOn);
    public override int GetHashCode() => HashCode.Combine(IsSet, _value);
    public override string ToString() => IsSet ? _value.ToString() : "Not Retired";
}
```

#### 子Entity の改修パターン

**現在:**

```csharp
public sealed class DepartmentMembership : Entity<DepartmentMembershipId>
{
    public EmployeeRowId EmployeeRowId { get; }
    // ...
}
```

**改修後:**

```csharp
public sealed class DepartmentMembership : Entity<EmployeeRowId>
{
    public EmployeeRowId EntId { get; private set; }  // 親Entity との紐付け用
    // ...
}
```

### RowId 事前採番フロー（Application層）

**CreateEmployeeUseCase での実装：**

```csharp
public class CreateEmployeeUseCase
{
    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeRequest request)
    {
        // 1. Sequence から RowId を事前採番
        var employeeRowId = await _sequenceProvider.AllocateNextRowIdAsync("t_employees");
        
        // 2. Entity を生成（RowId を持った状態で生成）
        var employee = Employee.Create(
            rowId: employeeRowId,
            division: EmployeeDivision.From(request.Division),
            number: EmployeeNumber.From(request.EmployeeNumber),
            personRowId: PersonRowId.From(request.PersonRowId),
            createdBy: CreatedBy.From(currentUser.PersonRowId),
            ...);
        
        // 3. DB に保存（同じ RowId で INSERT）
        await _repository.AddAsync(employee);
        
        // 4. ドメインイベント処理（この時点で RowId は確定）
        foreach (var evt in employee.DomainEvents)
        {
            await _eventBus.PublishAsync(evt);
        }
    }
}
```

### 改修手順

1. **RetiredOn ValueObject を新規作成**

   - 実装: LocalDateTime をラップ、Unset 対応

2. **DomainEventId.cs の改修 / 削除検討**（SharedKernel）

   **現在:**

   ```csharp
   public sealed class DomainEventId : PrimitiveValueObject<Guid>
   {
       private DomainEventId(Guid value) : base(value, true) { }
       public static DomainEventId New() => new(Guid.NewGuid());
   }
   ```

   **改修後:**
   RowId 事前採番により、DomainEvent は生成時にすでに AggregateRootId（RowId）を持つ。

   **選択肢A（推奨）:**

   - DomainEventId は削除
   - DomainEvent は AggregateRootId（EntId/RowId）で一意に識別
   - IDomainEvent.AggregateRootId のみで十分

   **選択肢B:**

   - DomainEventId を残すが、生成は不要
   - AggregateRootId（RowId）を主識別子として使用

3. **IDomainEvent インターフェース の見直し**（SharedKernel）

   ```csharp
   public interface IDomainEvent
   {
       // EventId は削除（AggregateRootId で識別可能）
       // public Guid EventId { get; }
       
       // RowId 事前採番のため、AggregateRootId は必須
       long AggregateRootId { get; }  // EntId（RowId）
       
       DateTime OccurredAt { get; }
   }
   ```

4. **Employee.cs を改修**

   - ID 型変更: `Entity<EmployeeId>` → `AggregateRoot<EmployeeRowId>`
   - プロパティ追加: `EntId`, `Division`, `Number`
   - プロパティ追加: `RetiredOn`
   - メソッド追加: `IsActive()`, `RetireEmployee()`
   - ドメインイベント: `EmployeeRetiredEvent` 発行（EntId/RowId で識別）
   - ドメインイベントコード例：

     ```csharp
     var retireEvent = new EmployeeRetiredEvent(
         aggregateRootId: this.EntId.Value,  // RowId（long）
         division: this.Division.Value,
         number: this.Number.Value,
         retiredOn: retiredOn
     );
     this.RaiseDomainEvent(retireEvent);
     ```

5. **DepartmentMembership, RoleAssignment, PermissionAssignment を改修**

   - ID 型変更: `Entity<*Id>` → `Entity<EmployeeRowId>`
   - EntId プロパティ追加

6. **ISequenceProvider インターフェース作成**（Infrastructure層で実装）

   - `AllocateNextRowIdAsync(tableName)` メソッド
   - Sequence から RowId を確保し、m_row_id_locks に記録

### テスト方法

```bash
dotnet test tests/Contexts/Employee.Domain.Tests/Entities/
# 期待結果: 52 テスト成功（EmployeeTests は大幅更新必須）
```

### リスク評価

- **リスク:** 高（ID 型の大幅変更、既存テスト大幅書き換え必須）
- **対策:** 段階的実装、各Entity ごとに テスト実行確認

### チェックポイント

- [ ] RetiredOn ValueObject ビルド成功
- [ ] Employee.cs ビルド成功
- [ ] EmployeeTests が新しい構造に対応
- [ ] EmployeeTests (22個) 成功

---

## 🔧 Phase 5: Mapper/Repository 改修

### 目的

DbModel ↔ Entity の変換ロジックを EmployeeRowId ベースに更新

### 対象ファイル

#### Mapper

- `src/Contexts/Employee/Employee.Infrastructure/Mappers/EmployeeMapper.cs`
- `tests/Contexts/Employee.Infrastructure.Tests/Mappers/EmployeeMapperTests.cs`

#### Repository

- `src/Contexts/Employee/Employee.Infrastructure/Repositories/EmployeeRepository.cs`
- `tests/Contexts/Employee.Infrastructure.Tests/Repositories/EmployeeRepositoryTests.cs`

### 改修内容

#### EmployeeMapper の改修

**現在:**

```csharp
public class EmployeeMapper
{
    public Employee ToDomainEntity(EmployeeDbModel dbModel, IClock clock)
    {
        var employeeId = EmployeeId.From(dbModel.EmployeeId);
        var employee = new Employee(employeeId, ...);
        return employee;
    }
}
```

**改修後:**

```csharp
public class EmployeeMapper
{
    public Employee ToDomainEntity(EmployeeDbModel dbModel, IClock clock)
    {
        // DbModel の employee_row_id → EmployeeRowId に変換
        if (!EmployeeRowId.TryFromDbValue(dbModel.EmployeeRowId, out var rowId))
            throw new InvalidOperationException(...);
        
        // RetiredOn の DateTime → LocalDateTime 変換
        RetiredOn? retiredOn = null;
        if (dbModel.RetiredOn.HasValue)
        {
            RetiredOn.TryFromDbValue(dbModel.RetiredOn, out var ro);
            retiredOn = ro;
        }
        else
        {
            retiredOn = RetiredOn.Unset();
        }
        
        // Entity の再構築
        var employee = new Employee(
            rowId: rowId,
            division: EmployeeDivision.From(dbModel.Division),
            number: EmployeeNumber.From(dbModel.EmployeeNumber),
            ...
            retiredOn: retiredOn
        );
        
        return employee;
    }
    
    public EmployeeDbModel ToDbModel(Employee entity)
    {
        return new EmployeeDbModel
        {
            EmployeeRowId = entity.EntId.Value,  // long 値
            Division = entity.Division.Value,
            EmployeeNumber = entity.Number.Value,
            RetiredOn = entity.RetiredOn?.Value.Value,  // LocalDateTime → DateTime
            ...
        };
    }
}
```

#### Repository の改修

**変更内容:**

- GetByIdAsync の ID パラメータを `EmployeeRowId` に変更
- EmployeeId（ビジネスID）での検索は別メソッド `GetByEmployeeNumberAsync` に分離

```csharp
public class EmployeeRepository : IEmployeeRepository
{
    // PK 検索（RowId ベース）
    public async Task<Employee?> GetByIdAsync(EmployeeRowId id)
    {
        var dbModel = await _dbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeRowId == id.Value);
        return dbModel == null ? null : _mapper.ToDomainEntity(dbModel, _clock);
    }
    
    // ビジネスID 検索（EmployeeNumber ベース）
    public async Task<Employee?> GetByEmployeeNumberAsync(EmployeeNumber number)
    {
        var dbModel = await _dbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeNumber == number.Value);
        return dbModel == null ? null : _mapper.ToDomainEntity(dbModel, _clock);
    }
    
    public async Task<Employee> AddAsync(Employee employee)
    {
        var dbModel = _mapper.ToDbModel(employee);
        await _dbContext.Employees.AddAsync(dbModel);
        await _dbContext.SaveChangesAsync();
        
        // 注：RowId は Application層 で事前採番済み
        // Entity には既に EmployeeRowId が設定されている状態で Repository に渡される
        // INSERT は主キーの衝突をチェックするのみ
        
        return _mapper.ToDomainEntity(dbModel, _clock);
    }
}
```

### テスト方法

```bash
dotnet test tests/Contexts/Employee.Infrastructure.Tests/
# 期待結果: 19 テスト成功（EmployeeMapperTests, EmployeeRepositoryTests）
```

### リスク評価

- **リスク:** 中（DB マッピングロジックの変更。既存テストで新しい ID 型をカバーする必要）
- **対策:** DbModel テスト、Mapper テスト で十分にカバー

### チェックポイント

- [ ] EmployeeMapper ビルド成功
- [ ] EmployeeRepository ビルド成功
- [ ] EmployeeMapperTests (10個) 成功
- [ ] EmployeeRepositoryTests (9個) 成功

---

## 🔧 Phase 6: Application層 RowId 事前採番・Use Case の改修

### 目的

CreateEmployeeUseCase で RowId 事前採番フローを実装

### 対象ファイル

#### ISequenceProvider インターフェース

- `src/Contexts/Employee/Employee.Application/Services/ISequenceProvider.cs`（新規作成）

#### CreateEmployeeUseCase

- `src/Contexts/Employee/Employee.Application/UseCases/CreateEmployeeUseCase.cs`

#### EmployeeSequenceProvider（Infrastructure層実装）

- `src/Contexts/Employee/Employee.Infrastructure/Services/EmployeeSequenceProvider.cs`（新規作成）

#### EmployeeDbModel

- `src/Contexts/Employee/Employee.Infrastructure/DbModels/EmployeeDbModel.cs`

### 実装内容

#### 1. ISequenceProvider インターフェース定義

```csharp
namespace SupportAdvance.Contexts.Employee.Application.Services;

/// <summary>
/// RowId 採番サービスのインターフェース
/// 責務：テーブルごとに次の RowId を事前採番し、業務排他制御用テーブルに記録
/// </summary>
public interface ISequenceProvider
{
    /// <summary>
    /// 指定されたテーブル向けに、次の RowId を採番する
    /// </summary>
    /// <param name="tableName">採番対象テーブル名（例："t_employees"）</param>
    /// <returns>採番された EmployeeRowId</returns>
    Task<EmployeeRowId> AllocateNextRowIdAsync(string tableName);
}
```

#### 2. CreateEmployeeUseCase での RowId 事前採番

```csharp
public class CreateEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;
    private readonly ISequenceProvider _sequenceProvider;
    private readonly IClock _clock;

    public async Task<EmployeeDto> ExecuteAsync(CreateEmployeeRequest request)
    {
        // 1. 【事前採番】Sequence から RowId を採番
        //    この時点で、m_row_id_locks テーブルに記録される
        //    （業務排他制御：この EmployeeRowId は現在使用中）
        var employeeRowId = await _sequenceProvider.AllocateNextRowIdAsync("t_employees");
        
        // 2. Entity を生成（RowId を既に持った状態で）
        var employee = Employee.Create(
            rowId: employeeRowId,
            division: EmployeeDivision.From(request.Division),
            number: EmployeeNumber.From(request.EmployeeNumber),
            personRowId: PersonRowId.From(request.PersonRowId),
            createdBy: CreatedBy.From(currentUser.PersonRowId),
            code: EmployeeCode.From(request.Code),
            ...);
        
        // 3. ドメインイベント取得（この時点で RowId は確定）
        var events = employee.DomainEvents;
        
        // 4. DB に INSERT（同じ RowId で保存）
        await _repository.AddAsync(employee);
        
        // 5. イベント発行
        foreach (var evt in events)
        {
            // evt.AggregateRootId は employee.EntId（既に確定）
            await _eventBus.PublishAsync(evt);
        }
        
        return _mapper.ToDto(employee);
    }
}
```

#### 3. EmployeeSequenceProvider 実装（Infrastructure層）

```csharp
namespace SupportAdvance.Contexts.Employee.Infrastructure.Services;

public class EmployeeSequenceProvider : ISequenceProvider
{
    private readonly IDbConnection _connection;

    public async Task<EmployeeRowId> AllocateNextRowIdAsync(string tableName)
    {
        // SQL Server の ユーザ定義関数を呼び出して次の RowId を採番
        using var command = _connection.CreateCommand();
        command.CommandText = $"SELECT dbo.fn_get_next_row_id('{tableName}')";
        command.CommandType = CommandType.Text;
        
        var result = await command.ExecuteScalarAsync();
        if (result is null or DBNull)
            throw new InvalidOperationException($"Failed to allocate RowId for {tableName}");
        
        var rowId = (long)result;
        
        // 採番した RowId を m_row_id_locks テーブルに記録
        // （業務排他制御：このRowIdは現在使用中）
        await LockRowIdAsync(rowId, tableName);
        
        return EmployeeRowId.From(rowId);
    }

    private async Task LockRowIdAsync(long rowId, string tableName)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO [dbo].[m_row_id_locks] (row_id, table_name, locked_by, locked_at)
            VALUES (@rowId, @tableName, @lockedBy, @lockedAt)
        ";
        
        var parameters = new[]
        {
            new SqlParameter("@rowId", rowId),
            new SqlParameter("@tableName", tableName),
            new SqlParameter("@lockedBy", _currentUser.PersonRowId),
            new SqlParameter("@lockedAt", DateTime.UtcNow)
        };
        
        foreach (var param in parameters)
            command.Parameters.Add(param);
        
        await command.ExecuteNonQueryAsync();
    }
}
```

#### 4. EmployeeDbModel の修正

EmployeeRowId プロパティ型を明確化：

```csharp
public class EmployeeDbModel
{
    // PK: row_id (採番時に既に確定)
    public long EmployeeRowId { get; set; }  // 主キー、事前採番済み
    
    // ビジネスキー
    public string Division { get; set; }
    public string EmployeeNumber { get; set; }
    
    // その他フィールド
    public DateTime? RetiredOn { get; set; }
    // ...監査カラム
}
```

### テスト方法

```bash
dotnet test tests/Contexts/Employee.Application.Tests/
# 期待結果: 24 テスト成功（CreateEmployeeUseCase テストで事前採番を検証）
```

### リスク評価

- **リスク:** 中（新しいインターフェース・実装クラスの追加）
- **対策:** Application層テストで ISequenceProvider を Mock して採番フローを検証

### チェックポイント

- [ ] ISequenceProvider インターフェース定義
- [ ] EmployeeSequenceProvider 実装成功
- [ ] CreateEmployeeUseCase で RowId 事前採番フロー実装
- [ ] Application.Tests (24個) 成功

---

## 🔧 Phase 7: 全テスト実行・検証

### 目的

すべての改修が正しく実装され、リグレッションがないこと確認

### テスト実行順序

```bash
# Step 1: ビルド確認
dotnet build --no-incremental

# Step 2: 単体テスト（各フェーズの最小単位）
dotnet test tests/SharedKernel.Tests/ValueObjects/Identifiers/ --filter "RowId"
# 期待: 7 成功

dotnet test tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/
# 期待: 112 成功

dotnet test tests/SharedKernel.Tests/Entities/
# 期待: 27 成功

dotnet test tests/Contexts/Employee.Domain.Tests/Entities/
# 期待: 52 成功

dotnet test tests/Contexts/Employee.Infrastructure.Tests/
# 期待: 19 成功

dotnet test tests/Contexts/Employee.Application.Tests/
# 期待: 24 成功

# Step 3: Architecture テスト
dotnet test tests/SharedKernel.Tests/Architecture/
# 期待: 27 成功

# Step 4: 全テスト実行
dotnet test
# 期待: 504 成功（リグレッション 0）
```

### リスク評価と対策

| リスク                                        | 確度 | 対策                               |
| --------------------------------------------- | ---- | ---------------------------------- |
| RowId 基底化で既存コード破損                  | 低   | Phase 1 直後 ビルド確認            |
| Entity<TId : ValueObject> 互換性              | 低   | Phase 3 直後 Architecture テスト   |
| EmployeeId（GUID）→ EmployeeRowId（long）移行 | 中   | Phase 5 単体テスト で十分検証      |
| RetiredOn ValueObject 実装漏れ                | 低   | コンパイラが型チェック             |
| Mapper の DateTime ↔ LocalDateTime 変換エラー | 中   | Phase 5 EmployeeMapperTests で検証 |
| Application層の DTO マッピング 不整合         | 低   | Phase 6 Application.Tests で検証   |

### チェックリスト

- [ ] Phase 1: RowId ビルド確認
- [ ] Phase 2: 5つの RowId ファイル ビルド + テスト 成功
- [ ] Phase 3: Entity<TId> 型制約 ビルド + Architecture テスト
- [ ] Phase 4: Employee Entity 再設計 ビルド + Domain テスト
- [ ] Phase 5: Mapper/Repository ビルド + Infrastructure テスト
- [ ] Phase 6: Application層 確認 ビルド + Application テスト
- [ ] Phase 7: 全テスト実行 504個 すべて 成功

---

## 🚨 ロールバック計画

万が一 Phase 中盤で重大な問題が発生した場合：

1. **Phase 1 直後 ビルド失敗時**

   - RowId.cs を以前のバージョンに戻す
   - （Phase 1 の改修は最小限なため、リスク低い）

2. **Phase 2 テスト大量失敗時**

   - 5つの RowId ファイルを以前のバージョンに戻す
   - RowId.cs に戻す

3. **Phase 3 以降 の場合**

   - Entity.cs / AggregateRoot.cs を戻す
   - 型制約を外す

4. **Phase 4 大規模失敗時**

   - Employee.cs の改修を一度保存して、段階的に実装し直す

---

## 📊 進捗トラッキング

実装中のチェック項目：

| フェーズ | 状態     | 完了条件                                       | 確認日時 |
| -------- | -------- | ---------------------------------------------- | -------- |
| Phase 1  | ⏳ 進行中 | ビルド成功 + RowIdTests 成功                   |          |
| Phase 2  | ⏳ 次     | RowId 型 ビルド成功 + 112 テスト成功           |          |
| Phase 3  | ⏳ 次     | Entity テスト 成功 + Architecture テスト成功   |          |
| Phase 4  | ⏳ 次     | Employee Entity ビルド成功 + Domain テスト成功 |          |
| Phase 5  | ⏳ 次     | Mapper/Repository テスト成功                   |          |
| Phase 6  | ⏳ 次     | Application テスト成功                         |          |
| Phase 7  | ⏳ 次     | 全テスト 504個 成功                            |          |

---

## 📝 注記

### 既存の既知の問題

- EmployeeId（GUID）は将来的に廃止予定（EmployeeNumber に統一）
- DomainEvent の AggregateRootId は EventId（GUID）に統一済み

### 今後の拡張

- AggregateRoot に子Entity管理機能を追加予定
- Repository pattern の非同期最適化
- Domain Event の Outbox パターン実装
