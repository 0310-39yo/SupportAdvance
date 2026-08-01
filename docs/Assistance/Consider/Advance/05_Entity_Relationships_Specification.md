# 人・部署・ログインの関係仕様書

**バージョン:** 1.0  
**作成日:** 2026-07-19  
**対象:** Advance データベース設計  

---

## 📋 概要

SupportAdvance システムにおける「人」「従業員」「部署」「ログイン認証」の関係を定義。複数部門への所属、ログイン認証方式の選択肢、監査情報との統合を含む。

---

## 🎯 基本概念

### 1. 「人（Person）」とは

**定義:** システムに記録されるすべての人物情報

**対象範囲:**
- ✅ 従業員（社員）
- ✅ 取引先担当者
- ✅ その他の登録者

**責務:**
- 個人の基本情報を保持
- 名前、カナ名などの個人属性

**例:**
- 田中太郎（従業員）
- 鈴木花子（取引先）
- System 管理者

---

### 2. 「従業員（Employee）」とは

**定義:** 人のうち、組織に属する社員

**条件:**
- m_persons と 1:1 の関係
- person_row_id は NOT NULL（必ず person を参照）
- employee_code は UNIQUE（一意に特定）

**属性:**
- employee_code: 1000 ～ N（1000 は System 管理者）
- retire_on: 退職日（NULL なら在職中）

**例:**
- employee_code=1001, 田中太郎（在職）
- employee_code=1002, 鈴木花子（退職日: 2026-06-30）

---

### 3. 「部署（Department）」とは

**定義:** 組織階層を構成する部門単位

**特徴:**
- **階層構造:** hierarchy_level 0～4（トップから係まで）
- **self-join:** parent_department_row_id で親部署参照
- **部長管理:** manager_employee_row_id で責任者を指定

**階層の例:**
```
hierarchy_level: 0
├─ トップ（親なし）
│  ├─ hierarchy_level: 1 - 本部
│  │  ├─ hierarchy_level: 2 - 部
│  │  │  ├─ hierarchy_level: 3 - グループ・課
│  │  │  │  ├─ hierarchy_level: 4 - 係・チーム
```

**状態管理:**
- `deleted_at IS NULL`: 有効な部署
- `deleted_at IS NOT NULL`: 廃止部署
- `abolished_on IS NOT NULL`: 公式廃止日

---

### 4. 「ログイン認証（Login Credentials）」とは

**定義:** ユーザーが認証される方法

**認証方式:**
1. **Windows ログイン（SSO）** - 推奨
2. **ID/パスワード認証** - フォールバック

**関係:**
- m_employees と 1:N（1人が複数のログイン方式を持つ可能性）
- employee_row_id で従業員を特定

---

## 🔄 関係フロー

### フロー1: 人が従業員になる場合

```
1. m_persons に「人」を登録
   └─ [row_id, last_name, first_name, ...]

2. m_employees に「従業員」を登録
   └─ [person_row_id → m_persons.row_id]
   └─ [employee_code: 1001]

3. m_employee_department に「所属」を登録
   └─ [employee_row_id → m_employees.row_id]
   └─ [department_row_id → m_department.row_id]
   └─ [is_primary: 1] (主たる所属)

4. m_login_credentials に「ログイン情報」を登録（オプション）
   └─ [employee_row_id → m_employees.row_id]
   └─ [login_id: "tanaka_taro"]
```

---

### フロー2: 従業員が複数部門に所属する場合

**シナリオ:** 営業部所属の営業担当者が、企画室でプロジェクト管理も行う

```
m_employee_department テーブルに2行作成：

Row 1: employee_row_id=1001, department_row_id=(営業部), is_primary=1, end_on=NULL
       → 主たる所属（営業部）

Row 2: employee_row_id=1001, department_row_id=(企画室), is_primary=0, end_on=NULL
       → 兼務（企画室）
```

**検索例:**
```sql
-- 従業員1001の主所属部署を取得
SELECT * FROM m_employee_department
WHERE employee_row_id = 1001 AND is_primary = 1 AND deleted_at IS NULL;

-- 従業員1001のすべての所属部署を取得
SELECT * FROM m_employee_department
WHERE employee_row_id = 1001 AND deleted_at IS NULL;
```

---

### フロー3: ログイン認証（Windows ログイン）

**前提:** Windows ドメイン環境、ログインID = M + employee_code

**処理フロー:**

```
1. ユーザーが Windows でログイン
   └─ Windows ログインID: M1001

2. アプリケーションが employee_code を抽出
   └─ 1001 を取得

3. m_employees.employee_code で検索
   ┌─ 見つかる
   │  └─ ✅ 自動ログイン（ダイアログなし）
   │     └─ 該当従業員のすべての情報を取得
   │
   └─ 見つからない
      └─ ログインダイアログを表示
         └─ ID/パスワード認証に移行
```

---

### フロー4: ログイン認証（ID/パスワード）

**シナリオ:** 取引先担当者やテスト環境での認証

**処理フロー:**

```
1. ユーザーが login_id と password を入力

2. m_login_credentials で検索
   └─ WHERE login_id = @id AND is_active = 1 AND deleted_at IS NULL

3. password_hash と比較
   └─ ✅ 認証成功 → employee_row_id で従業員を特定
   └─ ❌ 認証失敗 → エラーメッセージ

4. last_login_at を更新
   └─ UPDATE m_login_credentials SET last_login_at = GETDATE()
```

---

## 🛠️ 操作シナリオ

### シナリオ1: 新規従業員を採用

```
Step 1: m_persons に採用者の情報を登録
INSERT INTO m_persons 
  (last_name, first_name, last_name_kana, first_name_kana, inserted_at, inserted_by)
VALUES 
  ('新員', '太郎', 'シンイン', 'タロウ', GETDATE(), @CurrentUserRowId);

Step 2: m_employees に従業員情報を登録
INSERT INTO m_employees
  (person_row_id, employee_code, inserted_at, inserted_by)
VALUES
  (@PersonRowId, 1003, GETDATE(), @CurrentUserRowId);

Step 3: m_employee_department に配属先を登録
INSERT INTO m_employee_department
  (employee_row_id, department_row_id, is_primary, inserted_at, inserted_by)
VALUES
  (@EmployeeRowId, @DepartmentRowId, 1, GETDATE(), @CurrentUserRowId);

Step 4: m_login_credentials にログイン情報を登録（オプション）
INSERT INTO m_login_credentials
  (employee_row_id, login_id, password_hash, is_active, inserted_at, inserted_by)
VALUES
  (@EmployeeRowId, 'shin_in_taro', @HashedPassword, 1, GETDATE(), @CurrentUserRowId);
```

---

### シナリオ2: 従業員が部門異動

```
Step 1: 前の配属を終了
UPDATE m_employee_department
SET end_on = GETDATE()
WHERE employee_row_id = @EmployeeRowId 
  AND is_primary = 1 
  AND deleted_at IS NULL;

Step 2: 新しい配属先に追加（主たる所属）
INSERT INTO m_employee_department
  (employee_row_id, department_row_id, is_primary, inserted_at, inserted_by)
VALUES
  (@EmployeeRowId, @NewDepartmentRowId, 1, GETDATE(), @CurrentUserRowId);

Result:
- 前配属: end_on = 2026-07-19（終了）
- 新配属: is_primary = 1, end_on = NULL（開始）
```

**検索例:** 従業員の所属履歴を取得
```sql
SELECT 
  ed.employee_row_id,
  d.department_name,
  ed.inserted_at AS AssignmentStart,
  ed.end_on AS AssignmentEnd
FROM m_employee_department ed
JOIN m_department d ON ed.department_row_id = d.row_id
WHERE ed.employee_row_id = @EmployeeRowId
ORDER BY ed.inserted_at DESC;
```

---

### シナリオ3: 従業員を退職させる

```
Step 1: 所属を終了
UPDATE m_employee_department
SET end_on = GETDATE()
WHERE employee_row_id = @EmployeeRowId AND deleted_at IS NULL;

Step 2: 従業員レコードに退職日を記録
UPDATE m_employees
SET retire_on = GETDATE()
WHERE row_id = @EmployeeRowId;

Step 3: ログイン認証を無効化
UPDATE m_login_credentials
SET is_active = 0
WHERE employee_row_id = @EmployeeRowId;

Result:
- m_employees.retire_on = 2026-07-19（退職日を記録）
- m_login_credentials.is_active = 0（ログイン禁止）
```

**検証:** 退職者がログインできないこと
```sql
SELECT * FROM m_login_credentials
WHERE employee_row_id = @EmployeeRowId 
  AND is_active = 0
  AND deleted_at IS NULL;
```

---

### シナリオ4: 部署を廃止

```
Step 1: 部署の配属を終了
UPDATE m_employee_department
SET end_on = GETDATE()
WHERE department_row_id = @DepartmentRowId AND deleted_at IS NULL;

Step 2: 部署レコードに廃止日を記録
UPDATE m_department
SET abolished_on = GETDATE()
WHERE row_id = @DepartmentRowId;

Step 3: 部署を論理削除（必要に応じて）
UPDATE m_department
SET deleted_at = GETDATE(), deleted_by = @CurrentUserRowId
WHERE row_id = @DepartmentRowId;

Result:
- m_department.abolished_on = 2026-07-19（廃止日）
- m_employee_department.end_on = 2026-07-19（配属終了）
```

---

## 🔐 監査情報の統合

### すべての操作が監査記録される

| 操作 | 監査カラム | 記録内容 |
|---|---|---|
| **作成** | inserted_at, inserted_by | 作成日時、作成者 |
| **更新** | updated_at, updated_by | 更新日時、更新者 |
| **削除** | deleted_at, deleted_by | 削除日時、削除者 |

**例：従業員情報が更新された場合**
```
元の状態:
  m_employees.updated_at = NULL（未更新）
  m_employees.updated_by = NULL

更新後:
  m_employees.updated_at = 2026-07-19 14:30:00
  m_employees.updated_by = 1001（更新者ID）
```

**アプリケーション側の判定:**
```csharp
// 新規レコード判定
if (employee.UpdatedAt == null)
{
    // これは新規レコード
}

// 削除判定
if (employee.DeletedAt != null)
{
    // これは論理削除済み
}
```

---

## ⚠️ 注意事項

### 1. 人と従業員の区別

```
非従業員（取引先など）
├─ m_persons に登録 ✅
├─ m_employees には登録しない ✗
└─ ログイン認証も通常不要

従業員
├─ m_persons に登録 ✅
├─ m_employees に登録 ✅
└─ ログイン認証を設定 ✅
```

### 2. 複数所属の管理

- **是正日（end_on）:** NULL = 現在所属、値 = 過去の所属
- **主所属（is_primary）:** 給与や人事情報の対象を明確にするため必須

### 3. 論理削除の使い分け

```
m_department.abolished_on      → 部署の公式廃止日
m_department.deleted_at        → レコード自体の削除日

例：廃止部署でも、過去の履歴を保持したい場合
  abolished_on ≠ NULL（廃止済み）
  deleted_at IS NULL（レコードは有効）
```

### 4. ログイン認証の検証

```sql
-- ログイン可能な従業員のみ
SELECT e.*, lc.login_id
FROM m_employees e
LEFT JOIN m_login_credentials lc ON e.row_id = lc.employee_row_id
WHERE e.deleted_at IS NULL         -- 有効な従業員
  AND e.retire_on IS NULL          -- 退職していない
  AND lc.is_active = 1             -- ログイン有効
  AND lc.deleted_at IS NULL;       -- 認証情報有効
```

---

## 📊 関係図（テキスト版）

```
m_persons (人名マスタ)
    │
    ├─ 1:0..1
    │
m_employees (従業員マスタ)
    │
    ├─ 1:N ─ m_employee_department (従業員所属)
    │                  │
    │                  └─ N:1 ─ m_department (部署マスタ)
    │                             │
    │                             └─ self-join (親部署参照)
    │
    └─ 1:N ─ m_login_credentials (ログイン認証)
                 
Windows ログイン フロー:
M + employee_code (M1001) 
    → m_employees.employee_code で検索 
        → 見つかる: SSO ログイン
        → 見つからない: ID/Password 認証
```

---

## ✅ 設計確認項目

- [ ] 人と従業員の関係（1:0..1）は正確に理解されているか
- [ ] 複数部門所属の管理方法は明確か
- [ ] ログイン認証の2方式（Windows/ID-PW）が区別されているか
- [ ] 監査情報（inserted_at, updated_at, deleted_at）の使い分けが明確か
- [ ] 部署廃止（abolished_on）と論理削除（deleted_at）の区別が明確か

---

## 📚 参照資料

- [ER図](01_ER_Diagram.html) - テーブル関係図
- [インデックス戦略](02_Index_Strategy.md) - クエリ最適化
- [CREATE TABLE](03_CREATE_TABLE_SQLServer_Fixed.sql) - 実装済みスクリプト
