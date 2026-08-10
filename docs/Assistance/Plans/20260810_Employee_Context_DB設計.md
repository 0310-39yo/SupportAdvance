# Employee Context — DB設計

**プロジェクト:** SupportAdvance  
**フェーズ:** DB設計  
**作成日:** 2026-08-10  
**状態:** 実装前提での設計確定

---

## 📋 概要

Employee Context のデータベーススキーマ定義。以下のテーブルで構成：
- `m_persons`（人物マスタ）
- `m_employees`（従業員マスタ）
- `m_departments`（部署マスタ）
- `m_roles`（ロールマスタ）
- `m_permissions`（権限マスタ）
- `t_employee_attributes`（従業員属性）

---

## 📊 テーブル定義

### 1. m_persons（人物マスタ）

```sql
CREATE TABLE [dbo].[m_persons]
(
    -- 監査カラム
    [row_id] [bigint] NOT NULL PRIMARY KEY DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
    [row_version] [timestamp] NOT NULL,
    [created_at] [datetime2](7) NOT NULL,
    [created_by] [bigint] NOT NULL,
    [updated_at] [datetime2](7) NULL,
    [updated_by] [bigint] NULL,
    [deleted_at] [datetime2](7) NULL,
    [deleted_by] [bigint] NULL,
    
    -- ビジネスカラム
    [first_name] [nvarchar](100) NOT NULL,             -- 姓
    [first_name_kana] [nvarchar](100) NOT NULL,        -- 姓（カナ表記）
    [last_name] [nvarchar](100) NOT NULL,              -- 名
    [last_name_kana] [nvarchar](100) NOT NULL,         -- 名（カナ表記）
    
    -- 外部キー制約
    CONSTRAINT [FK_mPersons_CreatedBy] FOREIGN KEY ([created_by]) REFERENCES [m_persons]([row_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_mPersons_UpdatedBy] FOREIGN KEY ([updated_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL,
    CONSTRAINT [FK_mPersons_DeletedBy] FOREIGN KEY ([deleted_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL
)
```

**カラム説明:**
- `row_id`: システム行ID（主キー）
- `first_name`: 姓（例：山田）
- `first_name_kana`: 姓（カナ表記、例：ヤマダ）
- `last_name`: 名（例：太郎）
- `last_name_kana`: 名（カナ表記、例：タロウ）

**Index:**
```sql
CREATE NONCLUSTERED INDEX [IX_mPersons_FirstName] 
    ON [dbo].[m_persons]([first_name]);

CREATE NONCLUSTERED INDEX [IX_mPersons_LastName] 
    ON [dbo].[m_persons]([last_name]);
```

---

### 2. m_employees（従業員マスタ）

```sql
CREATE TABLE [dbo].[m_employees]
(
    -- 監査カラム
    [row_id] [bigint] NOT NULL PRIMARY KEY DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
    [row_version] [timestamp] NOT NULL,
    [created_at] [datetime2](7) NOT NULL,
    [created_by] [bigint] NOT NULL,
    [updated_at] [datetime2](7) NULL,
    [updated_by] [bigint] NULL,
    [deleted_at] [datetime2](7) NULL,
    [deleted_by] [bigint] NULL,
    
    -- ビジネスカラム
    [employee_agg_id] [uniqueidentifier] NOT NULL UNIQUE,
    [employee_id] [int] NOT NULL UNIQUE,
    [employee_division] [char](1) NOT NULL,            -- M:正社員, T:派遣, C:請負
    [retired_on] [datetime2](7) NOT NULL DEFAULT '9999-12-31 23:59:59.9999999',
    
    -- 外部キー制約
    CONSTRAINT [FK_mEmployees_CreatedBy] FOREIGN KEY ([created_by]) REFERENCES [m_persons]([row_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_mEmployees_UpdatedBy] FOREIGN KEY ([updated_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL,
    CONSTRAINT [FK_mEmployees_DeletedBy] FOREIGN KEY ([deleted_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL
)
```

**カラム説明:**
- `row_id`: システム行ID（主キー）
- `row_version`: 楽観ロック用タイムスタンプ
- `employee_agg_id`: 集約ID（GUID）
- `employee_id`: 従業員ID（1001以上）
- `employee_division`: 雇用形態（M/T/C）
- `retired_on`: 退職日（未設定時は 9999-12-31）

**Index:**
```sql
CREATE NONCLUSTERED INDEX [IX_mEmployees_EmployeeId] 
    ON [dbo].[m_employees]([employee_id]);

CREATE NONCLUSTERED INDEX [IX_mEmployees_EmployeeDivision] 
    ON [dbo].[m_employees]([employee_division]);

CREATE NONCLUSTERED INDEX [IX_mEmployees_AggId] 
    ON [dbo].[m_employees]([employee_agg_id]);
```

---

### 3. m_departments（部署マスタ）

```sql
CREATE TABLE [dbo].[m_departments]
(
    -- 監査カラム
    [row_id] [bigint] NOT NULL PRIMARY KEY DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
    [row_version] [timestamp] NOT NULL,
    [created_at] [datetime2](7) NOT NULL,
    [created_by] [bigint] NOT NULL,
    [updated_at] [datetime2](7) NULL,
    [updated_by] [bigint] NULL,
    [deleted_at] [datetime2](7) NULL,
    [deleted_by] [bigint] NULL,
    
    -- ビジネスカラム
    [department_code] [char](4) NOT NULL UNIQUE,
    [department_name] [nvarchar](200) NOT NULL,
    [short_name] [nvarchar](100) NOT NULL DEFAULT '',
    [hierarchy_level] [int] NOT NULL,                  -- 0:会社, 1:本部, 2:部, 3:グループ, 4:チーム
    [parent_row_id] [bigint] NULL,                     -- 親部署（NULL = ルート部署）
    [manager_employee_row_id] [bigint] NULL,           -- 管理者（NULL = 管理者なし）
    [abolished_on] [datetime2](7) NOT NULL DEFAULT '9999-12-31 23:59:59.9999999',
    
    -- 外部キー制約
    CONSTRAINT [FK_mDepartments_ParentRowId] FOREIGN KEY ([parent_row_id]) REFERENCES [m_departments]([row_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_mDepartments_ManagerEmployeeRowId] FOREIGN KEY ([manager_employee_row_id]) REFERENCES [m_employees]([row_id]) ON DELETE SET NULL,
    CONSTRAINT [FK_mDepartments_CreatedBy] FOREIGN KEY ([created_by]) REFERENCES [m_persons]([row_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_mDepartments_UpdatedBy] FOREIGN KEY ([updated_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL,
    CONSTRAINT [FK_mDepartments_DeletedBy] FOREIGN KEY ([deleted_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL
)
```

**カラム説明:**
- `department_code`: 部署コード（固定4文字、例：G100）
- `hierarchy_level`: 階層レベル（0=会社, 1=本部, 2=部, 3=グループ, 4=チーム）
- `parent_row_id`: 親部署の行ID（ツリー構造）
- `manager_employee_row_id`: 管理者の従業員行ID
- `abolished_on`: 廃止日（未設定時は 9999-12-31）

**Index:**
```sql
CREATE NONCLUSTERED INDEX [IX_mDepartments_DepartmentCode] 
    ON [dbo].[m_departments]([department_code]);

CREATE NONCLUSTERED INDEX [IX_mDepartments_ParentRowId] 
    ON [dbo].[m_departments]([parent_row_id]);

CREATE NONCLUSTERED INDEX [IX_mDepartments_HierarchyLevel] 
    ON [dbo].[m_departments]([hierarchy_level]);
```

---

### 4. m_roles（ロールマスタ）

```sql
CREATE TABLE [dbo].[m_roles]
(
    -- 監査カラム
    [row_id] [bigint] NOT NULL PRIMARY KEY DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
    [row_version] [timestamp] NOT NULL,
    [created_at] [datetime2](7) NOT NULL,
    [created_by] [bigint] NOT NULL,
    [updated_at] [datetime2](7) NULL,
    [updated_by] [bigint] NULL,
    [deleted_at] [datetime2](7) NULL,
    [deleted_by] [bigint] NULL,
    
    -- ビジネスカラム
    [role_code] [nvarchar](50) NOT NULL UNIQUE,
    [role_name] [nvarchar](200) NOT NULL,
    
    -- 外部キー制約
    CONSTRAINT [FK_mRoles_CreatedBy] FOREIGN KEY ([created_by]) REFERENCES [m_persons]([row_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_mRoles_UpdatedBy] FOREIGN KEY ([updated_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL,
    CONSTRAINT [FK_mRoles_DeletedBy] FOREIGN KEY ([deleted_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL
)
```

**カラム説明:**
- `role_code`: ロールコード（例：ADMIN, MANAGER, VIEWER）
- `role_name`: ロール表示名

**Index:**
```sql
CREATE NONCLUSTERED INDEX [IX_mRoles_RoleCode] 
    ON [dbo].[m_roles]([role_code]);
```

---

### 5. m_permissions（権限マスタ）

```sql
CREATE TABLE [dbo].[m_permissions]
(
    -- 監査カラム
    [row_id] [bigint] NOT NULL PRIMARY KEY DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
    [row_version] [timestamp] NOT NULL,
    [created_at] [datetime2](7) NOT NULL,
    [created_by] [bigint] NOT NULL,
    [updated_at] [datetime2](7) NULL,
    [updated_by] [bigint] NULL,
    [deleted_at] [datetime2](7) NULL,
    [deleted_by] [bigint] NULL,
    
    -- ビジネスカラム
    [permission_code] [nvarchar](100) NOT NULL UNIQUE,
    [permission_name] [nvarchar](200) NOT NULL,
    
    -- 外部キー制約
    CONSTRAINT [FK_mPermissions_CreatedBy] FOREIGN KEY ([created_by]) REFERENCES [m_persons]([row_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_mPermissions_UpdatedBy] FOREIGN KEY ([updated_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL,
    CONSTRAINT [FK_mPermissions_DeletedBy] FOREIGN KEY ([deleted_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL
)
```

**カラム説明:**
- `permission_code`: 権限コード（例：Employee.Create, Employee.Read.Own）
- `permission_name`: 権限表示名

**Index:**
```sql
CREATE NONCLUSTERED INDEX [IX_mPermissions_PermissionCode] 
    ON [dbo].[m_permissions]([permission_code]);
```

---

### 6. t_employee_attributes（従業員属性）

```sql
CREATE TABLE [dbo].[t_employee_attributes]
(
    -- 監査カラム
    [row_id] [bigint] NOT NULL PRIMARY KEY DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
    [row_version] [timestamp] NOT NULL,
    [created_at] [datetime2](7) NOT NULL,
    [created_by] [bigint] NOT NULL,
    [updated_at] [datetime2](7) NULL,
    [updated_by] [bigint] NULL,
    [deleted_at] [datetime2](7) NULL,
    [deleted_by] [bigint] NULL,
    
    -- ビジネスカラム
    [employee_row_id] [bigint] NOT NULL,
    [attribute_type] [nvarchar](50) NOT NULL,          -- DepartmentMembership/RoleAssignment/PermissionAssignment
    [attribute_row_id] [bigint] NOT NULL,              -- FK → m_departments/m_roles/m_permissions
    
    -- 子Entity 共通カラム（attribute_type により使い分け）
    [is_primary] [bit] NULL,                           -- DepartmentMembership のみ
    [effective_from] [datetime2](7) NULL,              -- RoleAssignment/PermissionAssignment
    [effective_until] [datetime2](7) NULL,             -- RoleAssignment/PermissionAssignment
    [expired_on] [datetime2](7) NULL,                  -- DepartmentMembership のみ
    
    -- 外部キー制約
    CONSTRAINT [FK_tEmployeeAttributes_EmployeeRowId] FOREIGN KEY ([employee_row_id]) REFERENCES [m_employees]([row_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tEmployeeAttributes_CreatedBy] FOREIGN KEY ([created_by]) REFERENCES [m_persons]([row_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_tEmployeeAttributes_UpdatedBy] FOREIGN KEY ([updated_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL,
    CONSTRAINT [FK_tEmployeeAttributes_DeletedBy] FOREIGN KEY ([deleted_by]) REFERENCES [m_persons]([row_id]) ON DELETE SET NULL
)
```

**カラム説明:**
- `employee_row_id`: 従業員の行ID
- `attribute_type`: 属性タイプ（DepartmentMembership/RoleAssignment/PermissionAssignment）
- `attribute_row_id`: 参照先行ID（マスタテーブルの row_id）
- `is_primary`: プライマリフラグ（DepartmentMembership のみ）
- `effective_from`: 有効開始日（RoleAssignment/PermissionAssignment）
- `effective_until`: 有効終了日（RoleAssignment/PermissionAssignment）
- `expired_on`: 失効日（DepartmentMembership のみ）

**Index:**
```sql
CREATE NONCLUSTERED INDEX [IX_tEmployeeAttributes_EmployeeRowId] 
    ON [dbo].[t_employee_attributes]([employee_row_id]);

CREATE NONCLUSTERED INDEX [IX_tEmployeeAttributes_AttributeType] 
    ON [dbo].[t_employee_attributes]([attribute_type]);

CREATE NONCLUSTERED INDEX [IX_tEmployeeAttributes_EmployeeRowId_AttributeType] 
    ON [dbo].[t_employee_attributes]([employee_row_id], [attribute_type]);

CREATE NONCLUSTERED INDEX [IX_tEmployeeAttributes_EffectiveFrom] 
    ON [dbo].[t_employee_attributes]([effective_from])
    WHERE [effective_from] IS NOT NULL;
```

---

## 🔑 外部キー制約の設定

### ON DELETE の戦略

| FK | ON DELETE | 理由 |
|----|-----------|------|
| `created_by` | CASCADE | 作成者は必須情報。削除時は関連レコード削除 |
| `updated_by` | SET NULL | 更新者は オプション。削除時は NULL に |
| `deleted_by` | SET NULL | 削除者も オプション。削除時は NULL に |

### リスク回避

- **CASCADE は慎重に使用**（親削除時に子も自動削除される）
- **m_persons削除は極めてレア** → CASCADE で問題なし
- **m_departments親削除** → 子部署は CASCADE で階層全削除（想定通り）

---

## 📈 パフォーマンス考慮

### クエリパターン

**従業員と部署を取得:**
```sql
SELECT 
    e.row_id, e.employee_id, e.employee_division,
    d.row_id, d.department_code, d.department_name
FROM m_employees e
JOIN t_employee_attributes ea 
    ON e.row_id = ea.employee_row_id 
    AND ea.attribute_type = 'DepartmentMembership'
    AND ea.is_primary = 1
JOIN m_departments d 
    ON d.row_id = ea.attribute_row_id
WHERE e.retired_on = '9999-12-31 23:59:59.9999999'
```

**Index により以下が最適化される：**
- `IX_tEmployeeAttributes_EmployeeRowId_AttributeType`: employee + type でのスキャン削減
- `IX_mDepartments_DepartmentCode`: 部署検索高速化

---

## ✅ 注意点

1. **hierarchy_level は値の検証が必須**（0-4の範囲）
2. **attribute_type は限定値のみ許可**（CHECK制約追加検討）
3. **日付フィールドの NULL vs MaxValue**: DB上は datetime2、Domain層で Unset() に変換
4. **スパース構造**: attribute_type により使わないカラムが NULL → リポジトリで適切に変換

---

## 更新履歴

| 日付 | 更新内容 |
|-----|--------|
| 2026-08-10（後）| m_persons テーブル追加。first_name/first_name_kana/last_name/last_name_kana カラム定義 |
| 2026-08-10 | 初版作成。全テーブル、Index、FK定義 |
