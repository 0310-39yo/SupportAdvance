# Employee Context — 要件定義とドメイン設計

**プロジェクト:** SupportAdvance  
**フェーズ:** 要件定義 + ドメイン設計  
**作成日:** 2026-08-10  
**状態:** 実装前提での設計確定

---

## 📋 目次

1. [要件定義](#要件定義)
2. [ドメイン設計](#ドメイン設計)
3. [子Entity の詳細設計](#子entityの詳細設計)
4. [マスタ管理](#マスタ管理)
5. [層間のデータフロー](#層間のデータフロー)
6. [実装時の注意点](#実装時の注意点)

---

## 要件定義

### 1. Employee Context の責務

基幹システムにおいて、以下を責務とする：
- 従業員の識別・管理
- 従業員の部署所属管理（複数所属対応）
- 従業員のロール・権限管理
- 従業員の監査ログ（誰がこのシステムで何をしたか）
- 開発検証時の権限置き換え・委譲機能

**範囲外:**
- 給与・勤怠情報（外部クラウドシステム）
- 人事ライフサイクル管理（別システム）

---

### 2. 識別情報（Requirement A）

#### EmployeeId（従業員識別用数値）
- 純粋な従業員識別用の数値（1001以上）
- 採番は別部署で行われ、システムでは制御しない
- 1000 = システム管理者ユーザー（プログラムが生成したレコード用）
- 1001以上 = 実際の人間
- **欠番がある**（基幹システムを使わない部門の人は飛ばされる）

#### EmployeeCode（従業員コード）
- 形式：`M1234`（雇用形態 + EmployeeId）
- 採番体系により、同じ ID を持つことはない
  - M（正社員）：1001-6999 または 10000以上
  - T（派遣）：7500-7999 または 70000以上
  - C（請負）：8000-8499 または 80000以上
- 外部入力（ログイン認証など）で頻繁に使用
- BC間で共有される主要識別子

#### 命名統一
- 旧 EmployeeNumber → EmployeeId に統一

---

### 3. 個人情報（Requirement B）

#### 方針：紐づけテーブル管理
- Employee Entity は個人情報を直接保有しない
- Person マスタ（名前、連絡先など）は t_employee_attributes で管理
- 必要時に Application層を通じて取得

#### 部署情報（Department）
- **最も重要な情報**：権限・ロール決定の基礎
- **パターンB 採用**：紐づけテーブルで管理（BC間参照を制限）
- 部署構成：ツリー構造（親子階層）、本部/部/グループなどのレベル
- **複数所属対応**：プライマリフラグで主たる所属を識別
- EmployeeRowId と DepartmentRowId の紐づけテーブル（t_employee_departments）に失効日を持つ

---

### 4. ステータス情報（Requirement C）

#### RetiredAt（退職日）
- 退職後も過去データは保持
- `RetiredAt IS NULL` で「現在の従業員」をフィルタリング
- **ライフサイクル管理は別システム**（入社日などは不要）

#### deleted_at（技術的なデータ管理）
- Employee レコード削除時に使用（非常にレア）
- ビジネス上の削除は `deleted_at` ではなく `RetiredAt` を使用

---

### 5. 権限・ロール（Requirement D）

#### 基本方針
- ロール = 権限の集合
- **基本は部署単位で決定される**
- **個人単位での追加・削除が可能**（例：この人だけ特別な権限）

#### 権限の粒度
- **機能・操作・データ** すべてについて管理必要
- 例：
  - 機能：「従業員管理機能」
  - 操作：「Create、Read、Update、Delete」
  - データ：「自部署のデータのみ」「全データ」

#### メニュー表示制御
- 実行者（操作者）の権限に基づいて、**対象者の権限情報をフィルタリング**
- Role-based Access Control (RBAC)

#### 権限フィルタリングロジック
- **Crosscutting層に PermissionPolicy を配置**
- 複数 Use Case で再利用可能
- 権限ルール変更時に一元管理

---

### 6. 権限置き換え機能（Requirement E）

#### 検証用：ImpersonationContext（成りすまし）
- 開発・検証環境（テストDB）のみ
- 実行者がログイン中のまま、別の人の権限で動作
- 監査ログなし

#### 本番用：DelegationContext（権限委譲）
- 権限の所有者から別の人に権限を一時移譲
- パターン：ユーザーがログアウト → 再ログイン時に権限委譲が適用される
- 有効期限：LocalDateTime（MaxValue = 期限なし）
- 監査ログ：移譲者・代理人・内容を記録

---

### 7. 監査情報（Requirement F）

#### 記録対象
- **全 BC における全操作を記録**
- 後で不要な項目は削除可能

#### 記録内容
- 操作者（実際のログインユーザー）
- 代理人（権限委譲時）
- 操作内容
- タイムスタンプ

---

## ドメイン設計

### 1. 集約構造

#### Employee 集約（集約ルート）

```
Employee（集約ルート）
├── Id: EmployeeId（集約根ID）
├── EmployeeCode: EmployeeCode（ValueObject）
├── EmployeeRowId: EmployeeRowId（DB行ID、公開）
├── RetiredAt: RetiredAt（null厳格性）
│
├── DepartmentMemberships: List<DepartmentMembership>（子Entity）
│   ├── Id: DepartmentMembershipId
│   ├── DepartmentCode: DepartmentCode（値）
│   ├── IsPrimary: bool
│   └── ExpiredAt: ExpirationDate（null厳格性）
│
├── RoleAssignments: List<RoleAssignment>（子Entity）
│   ├── Id: RoleAssignmentId
│   ├── RoleCode: RoleCode（値）
│   ├── EffectiveFrom: LocalDateTime
│   └── EffectiveUntil: EffectiveDate（null厳格性）
│
└── PermissionAssignments: List<PermissionAssignment>（子Entity）
    ├── Id: PermissionAssignmentId
    ├── PermissionCode: PermissionCode（値）
    ├── EffectiveFrom: LocalDateTime
    └── EffectiveUntil: EffectiveDate（null厳格性）
```

#### 集約ルート以下は独立集約ではない
- Department、Role、Permission は **独立集約ではなく、値のみを保有**
- 詳細情報が必要な場合は Application層で マスタから取得

---

### 2. Entity 設計

#### Employee Entity

```csharp
public sealed class Employee : Entity<EmployeeId>
{
    public EmployeeId EmployeeId => Id;  // 集約根ID のエイリアス
    public EmployeeCode EmployeeCode { get; private set; }
    public EmployeeRowId EmployeeRowId { get; private set; }
    public RetiredAt RetiredAt { get; private set; }
    
    // 子Entity（コレクション）
    internal List<DepartmentMembership> DepartmentMemberships { get; private set; }
    internal List<RoleAssignment> RoleAssignments { get; private set; }
    internal List<PermissionAssignment> PermissionAssignments { get; private set; }
    
    // ビジネスロジック（必須情報に限定）
    public bool IsActive() => !RetiredAt.IsSet;
    
    public bool IsRetired(LocalDateTime asOf) 
        => RetiredAt.IsSet && RetiredAt.Value <= asOf;
    
    // 子Entity 追加メソッド
    public void AddDepartmentMembership(DepartmentCode code, bool isPrimary)
    {
        DepartmentMemberships.Add(
            new DepartmentMembership(
                DepartmentMembershipId.New(),
                code,
                isPrimary,
                ExpirationDate.NoExpiration()));
    }
    
    public void AddRoleAssignment(RoleCode code, LocalDateTime effectiveFrom)
    {
        RoleAssignments.Add(
            new RoleAssignment(
                RoleAssignmentId.New(),
                code,
                effectiveFrom,
                EffectiveDate.Permanent()));
    }
    
    public void AddPermissionAssignment(PermissionCode code, LocalDateTime effectiveFrom)
    {
        PermissionAssignments.Add(
            new PermissionAssignment(
                PermissionAssignmentId.New(),
                code,
                effectiveFrom,
                EffectiveDate.Permanent()));
    }
}
```

**特徴:**
- `internal` で BC外からのアクセスを制限
- ビジネスロジックは必須情報に限定
- 詳細情報（部署名、権限内容）は Application層で取得

---

### 3. ValueObject 設計

#### 識別子系（Identifiers）
```
src/Contexts/Employee/Domain/ValueObjects/Identifiers/
├── EmployeeId（int 型、1001以上）
├── EmployeeCode（M1234形式）
├── EmployeeDivision（M/T/C）
├── EmployeeRowId（DB行ID）
├── DepartmentCode（G100形式）
├── DepartmentRowId（DB行ID）
├── DepartmentMembershipId（子Entity ID）
├── RoleAssignmentId（子Entity ID）
├── PermissionAssignmentId（子Entity ID）
├── RoleCode（値）
└── PermissionCode（値）
```

#### 部署関連（Department）
```
src/Contexts/Employee/Domain/ValueObjects/Department/
├── DepartmentName（正式名称）
├── ShortDepartmentName（略称、null厳格性）
├── Level（本部/部/グループなど）
├── ParentDepartmentCode（親部署、null厳格性）
├── ManagerEmployeeRowId（管理者、null厳格性）
└── AbolishedOn（廃止日、null厳格性）
```

#### その他
```
src/Contexts/Employee/Domain/ValueObjects/Employee/
├── RetiredAt（退職日、null厳格性）
├── ExpirationDate（失効日、null厳格性）
└── EffectiveDate（有効期限、null厳格性）
```

#### null厳格性の実装例
```csharp
public sealed class ExpirationDate : ValueObject
{
    public LocalDateTime Value { get; }
    public bool IsSet { get; }  // false = 期限なし（null相当）
    
    private ExpirationDate(LocalDateTime value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }
    
    public static ExpirationDate NoExpiration() 
        => new(LocalDateTime.MaxValue, false);  // 期限なし
    
    public static ExpirationDate From(LocalDateTime date)
        => new(date, true);  // 期限あり
    
    public bool IsExpired(LocalDateTime asOf) 
        => IsSet && Value <= asOf;
}
```

---

## 子Entity の詳細設計

### DepartmentMembership（部署所属）

```csharp
public class DepartmentMembership : Entity<DepartmentMembershipId>
{
    public DepartmentMembershipId Id { get; private set; }
    public DepartmentCode DepartmentCode { get; private set; }
    public bool IsPrimary { get; private set; }
    public ExpirationDate ExpiredAt { get; private set; }  // null厳格性
    
    public bool IsActive(LocalDateTime asOf) 
        => !ExpiredAt.IsExpired(asOf);
    
    public bool IsPrimaryMembership() 
        => IsPrimary;
}
```

**責務:**
- 従業員の部署所属を管理
- 有効期限（失効日）を保持
- プライマリ部署フラグで主たる所属を識別

---

### RoleAssignment（ロール割り当て）

```csharp
public class RoleAssignment : Entity<RoleAssignmentId>
{
    public RoleAssignmentId Id { get; private set; }
    public RoleCode RoleCode { get; private set; }
    public LocalDateTime EffectiveFrom { get; private set; }
    public EffectiveDate EffectiveUntil { get; private set; }  // null厳格性
    
    public bool IsActive(LocalDateTime asOf) 
        => EffectiveFrom <= asOf && !EffectiveUntil.IsExpired(asOf);
}
```

**責務:**
- 従業員に割り当てられたロールを管理
- 有効期間（開始日〜終了日）を保持

---

### PermissionAssignment（権限割り当て）

```csharp
public class PermissionAssignment : Entity<PermissionAssignmentId>
{
    public PermissionAssignmentId Id { get; private set; }
    public PermissionCode PermissionCode { get; private set; }
    public LocalDateTime EffectiveFrom { get; private set; }
    public EffectiveDate EffectiveUntil { get; private set; }  // null厳格性
    
    public bool IsActive(LocalDateTime asOf) 
        => EffectiveFrom <= asOf && !EffectiveUntil.IsExpired(asOf);
}
```

**責務:**
- 従業員に割り当てられた権限を管理
- 有効期間を保持

---

## マスタ管理

### 方針
- マスタ管理は **Domain Entity ではない**
- **ビジネスロジックがない** ため、Application層 + Infrastructure層で実装

### テーブル一覧
```
m_departments（部署マスタ）
  ├─ row_id（PK）
  ├─ department_code（UNIQUE、G100形式）
  ├─ department_name（正式名称）
  ├─ short_name（略称、NULL許可）
  ├─ level（レベル）
  ├─ parent_row_id（親部署のrow_id）
  ├─ manager_employee_row_id（管理者のEmployeeRowId）
  ├─ abolished_on（廃止日、NULL = 有効）
  └─ 監査カラム（created_at, created_by, updated_at, updated_by, deleted_at, deleted_by）

m_roles（ロールマスタ）
  ├─ row_id（PK）
  ├─ role_code（UNIQUE、ビジネスコード）
  ├─ role_name（表示名）
  └─ 監査カラム

m_permissions（権限マスタ）
  ├─ row_id（PK）
  ├─ permission_code（UNIQUE、ビジネスコード）
  ├─ permission_name（表示名）
  └─ 監査カラム
```

### マスタメンテナンス Use Case
```
CreateDepartmentMasterUseCase
UpdateDepartmentMasterUseCase
DeleteDepartmentMasterUseCase

CreateRoleMasterUseCase
UpdateRoleMasterUseCase
DeleteRoleMasterUseCase

CreatePermissionMasterUseCase
UpdatePermissionMasterUseCase
DeletePermissionMasterUseCase
```

---

## 層間のデータフロー

### 例：従業員の名前一覧を表示

```
【Presentation層】
View（XAML）
  ↓ Button.Click
ViewModel.LoadEmployeesCommand
  ↓ (RelayCommand)
  
【Application層】
GetEmployeeListUseCase.ExecuteAsync()
  ├─ Repository.GetAllAsync()（Employee 集約を全件取得）
  └─ 各 Employee について Repository.GetPersonAsync()（Person 属性を取得）
      ↓
  EmployeeListItemDto に変換
      ↓
  ViewModel に返す
      ↓
  
【Presentation層】
ViewModel.Employees = List<EmployeeListItemDto>
  ↓
UI 更新（リスト表示）
```

### 重要な原則
- **Entity は internal で保護**（BC外からのアクセス制限）
- **Application層が DTO に変換して提供**
- **他の BC は Application層経由でのみアクセス可能**
- **Entity がビジネスロジックを必要とする場合、必要な情報をパラメータで受け取る**

```csharp
// ❌ 間違い（Entity が他の集約に依存）
public bool CanCreateEmployee()
{
    var role = await _roleRepository.GetByEmployeeAsync(this.Id);  // 逆依存！
    return role.IsAdmin;
}

// ✅ 正しい（Application層で調整）
public bool CanCreateEmployee(List<Role> roles)  // パラメータで受け取る
{
    return roles.Any(r => r.IsAdmin);
}

// Application層で実装
public class CreateEmployeeUseCase
{
    public async Task Execute(...)
    {
        var employee = await _employeeRepository.GetByIdAsync(...);
        var roles = await _roleRepository.GetByEmployeeAsync(...);
        
        if (!employee.CanCreateEmployee(roles))
            throw new PermissionDeniedException();
    }
}
```

---

## 実装時の注意点

### 1. BC間の参照を厳密に守る
- Employee の property は `internal` で BC外からアクセス不可
- 他の BC が Employee 情報を参照する場合は、必ず Application層の Use Case を介す
- DTO を使用して必要な情報のみを提供

### 2. null厳格性の徹底
- `LocalDateTime?` ではなく null厳格性の ValueObject を使用
- すべての optional フィールドは `IsSet` フラグで状態を表現

### 3. Entity の責務を限定
- ビジネスロジックは **必須情報に限定**
- 詳細情報が必要な場合は Application層で取得

### 4. 子Entity の更新
- Entity メソッド（`AddDepartmentMembership()` など）で状態を変更
- Repository への永続化は Application層で実施

### 5. 権限フィルタリング
- Crosscutting層の PermissionPolicy を使用
- 複数 Use Case での再利用を前提

### 6. マスタ管理
- マスタ（m_departments、m_roles など）はテーブルのみ
- CRUD Use Case は Application層で実装
- ビジネスロジックは不要

---

## 参考資料

- **CLAUDE.md**: プロジェクト全体の設計指針
- **CLEAN_ARCHITECTURE_GUIDELINES.md**: 依存関係のルール
- **null厳格性設計ガイド.md**: null 厳格性の詳細

---

## 更新履歴

| 日付 | 更新内容 |
|-----|--------|
| 2026-08-10 | 初版作成。要件定義とドメイン設計を統合 |
