# SupportAdvance 監査方式 設計方針案

## 1. 目的

本システムにおける監査の目的は、以下を追跡可能にすることである。

- 誰が変更したか
- いつ変更したか
- 何を変更したか
- 変更前はどのような値であったか

監査は業務ロジックではなく、データ変更履歴の保全を目的とする。

------

# 2. 基本方針

SupportAdvance では監査専用ライブラリや独自監査ログテーブルを使用せず、SQL Server の Temporal Table 機能を利用する。

構成は以下とする。

```
Application（IClock経由で時刻取得）
  ↓
RepoDB
  ↓
SQL Server
  ├─ Trigger
  ├─ Temporal Table
  └─ Session Context
```

------

# 3. テーブル設計

全業務テーブルに以下の審査項目を持たせる。

| 項目 | 内容 | 設定者 | タイミング |
|---|---|---|---|
| CreatedAt | 作成日時 | Application | INSERT時、IClock経由で取得 |
| CreatedBy | 作成者 | Trigger | INSERT時、Session Context から自動設定 |
| UpdatedAt | 更新日時 | Trigger | UPDATE時、SYSUTCDATETIME()で自動設定 |
| UpdatedBy | 更新者 | Trigger | UPDATE時、Session Context から自動設定 |
| UpdatedSource | 更新元 | Trigger | UPDATE時、Session Context の有無で自動判定 |
| IsDeleted | 論理削除フラグ | Trigger | DELETE時、論理削除で対応 |

------

# 4. Temporal Table の利用

主要テーブルは Temporal Table とする。

対象例：
- Orders
- OrderDetails
- Customers
- Products
- ProductionPlans

Temporal Table を有効にすると SQL Server が履歴テーブルを自動管理する。

例：
```
Orders（主テーブル）
  ↓
OrdersHistory（履歴テーブル、SQL Serverが自動管理）
```

### 更新時

```
UPDATE Orders SET Amount = 100
  ↓
Trigger発火：
  - UpdatedAt = SYSUTCDATETIME()
  - UpdatedBy = SESSION_CONTEXT(N'UserId')
  - UpdatedSource = 'APP' or 'DB'
  ↓
SQL Serverが自動管理：
  - 旧データを OrdersHistory に保存
  - 新データを Orders に保存
```

### 削除時（論理削除）

```
DELETE Orders WHERE OrderId = 1
  ↓
Trigger発火：
  - IsDeleted = 1（論理削除フラグを設定）
  - UpdatedAt = SYSUTCDATETIME()
  - UpdatedBy = SESSION_CONTEXT(N'UserId')
  ↓
SQL Serverが自動管理：
  - 削除前データを OrdersHistory に保存
  - Orders には IsDeleted=1 のレコードが残存
```

**重要：** アプリケーション側で履歴保存処理は不要。SQL Server が自動管理する。

------

# 5. IsDeleted（論理削除）のルール

### 5.1 IsDeleted を使ってよい場合

| 条件 | 例 | 監査上の扱い |
|---|---|---|
| **誤登録の取り消し** | 入力ミスで登録されたレコード | Temporal Table で削除前データを保持 |
| **物理削除不可の制約への対処** | 外部キー制約・監査要件により削除できない | Temporal Table で削除履歴を管理 |
| **ドメイン上「存在しない」ことが業務的に自明** | 削除後に参照・言及される可能性がない | IsDeleted=1 で非表示、履歴可視 |

### 5.2 IsDeleted を使ってはならない場合

| 条件 | なぜ不可か | 正しい設計 |
|---|---|---|
| **業務上の状態が変わった** （退職・休職・無効化など） | 単なる削除ではなく、業務的に意味のある状態遷移 | ドメインの状態 ValueObject / enum で表現 |
| **過去の状態を履歴として参照する必要がある** | 「誰がいつ状態変化したか」を追跡する要件 | 状態遷移として設計し、IsDeleted とは分離 |
| **「削除された理由」が業務的に意味を持つ** | 削除理由そのものが業務ルール | 理由を表す ValueObject またはイベントを設計 |

### 5.3 Temporal Table と IsDeleted の関係

論理削除（IsDeleted=1 に更新）後も Temporal Table により履歴が自動保持される。

```
削除前の状態                  削除後の状態
━━━━━━━━━━━━━━━━           ━━━━━━━━━━━━━━━━
Orders テーブル              Orders テーブル
  OrderId = 1                  OrderId = 1
  Amount = 1000     DELETE →   IsDeleted = 1
  IsDeleted = 0               UpdatedAt = 2026-07-05

OrdersHistory テーブル（SQL Server 自動管理）
  削除前データ保存
  OrderId = 1, Amount = 1000, IsDeleted = 0, ValidFrom = 2026-07-04 10:00
  OrderId = 1, Amount = 1000, IsDeleted = 1, ValidTo = 2026-07-05 14:30
```

**過去時点のデータをクエリ可能：**
```sql
-- 2026-06-01 時点での状態を確認
SELECT * FROM Orders FOR SYSTEM_TIME AS OF '2026-06-01'
WHERE OrderId = 1

-- 削除履歴を確認
SELECT * FROM OrdersHistory
WHERE OrderId = 1
ORDER BY ValidFrom DESC
```

### 5.4 業務状態変化の例（IsDeleted は不可）

#### ❌ 不正な例

```sql
-- これは誤り：Employee の「退職」を IsDeleted で表現してはいけない
DELETE FROM Employees WHERE EmployeeId = 123
-- → 過去の給与計算、勤務履歴を参照できなくなる
```

#### ✅ 正しい例

```sql
-- 正しい：Employee の「退職」を EmploymentStatus で表現
UPDATE Employees
SET EmploymentStatus = 'Resigned'  -- ValueObject / enum
    ResignedAt = GETDATE()
    ResignedReason = '定年退職'
WHERE EmployeeId = 123

-- Temporal Table が自動的に履歴を保存
-- → 過去の給与、勤務履歴が全て参照可能
```

### 5.5 Claude ルール（設計準拠）

設計書・実装を検証する際、以下のいずれかに IsDeleted が使われていると設計違反として指摘する：

- 退職・無効化・対象外化など、**業務状態の変化を IsDeleted で表現している**
- IsDeleted 以外に状態を表すフィールドがなく、**履歴参照の要件が仕様に存在する**

------

# 6. Session Context 利用方針

アプリ接続時にログインユーザーを Session Context に設定する。

```sql
EXEC sp_set_session_context
  @key = N'UserId',
  @value = N'KATO';
```

------

# 7. Trigger による監査項目自動更新

### 7.1 INSERT トリガー（新規登録時）

```sql
CREATE TRIGGER trg_Orders_Insert ON Orders
AFTER INSERT
AS
BEGIN
  UPDATE Orders
  SET CreatedBy = COALESCE(
        SESSION_CONTEXT(N'UserId'),
        ORIGINAL_LOGIN()
      )
  WHERE OrderId IN (SELECT OrderId FROM inserted)
END
```

**注記：** CreatedAt はアプリ側で IClock を使用して取得し、INSERT時に値を指定。

### 7.2 UPDATE トリガー（更新時）

#### 更新日時
```sql
UpdatedAt = SYSUTCDATETIME()
```

#### 更新者
```sql
UpdatedBy = COALESCE(
  SESSION_CONTEXT(N'UserId'),
  ORIGINAL_LOGIN()
)
```

#### 更新元
```sql
UpdatedSource = CASE
  WHEN SESSION_CONTEXT(N'UserId') IS NULL
    THEN 'DB'
  ELSE 'APP'
END
```

### 7.3 DELETE トリガー（論理削除時）

```sql
CREATE TRIGGER trg_Orders_Delete ON Orders
AFTER DELETE
AS
BEGIN
  INSERT INTO Orders_Deleted_Backup
  SELECT * FROM deleted;
  
  UPDATE Orders
  SET IsDeleted = 1,
      UpdatedAt = SYSUTCDATETIME(),
      UpdatedBy = COALESCE(
        SESSION_CONTEXT(N'UserId'),
        ORIGINAL_LOGIN()
      ),
      UpdatedSource = CASE
        WHEN SESSION_CONTEXT(N'UserId') IS NULL
          THEN 'DB'
        ELSE 'APP'
      END
  WHERE OrderId IN (SELECT OrderId FROM deleted)
END
```

**注記：** ON DELETE ではなく ON UPDATE で論理削除を実装。物理削除は禁止。

------

# 8. 取得できる監査情報

Temporal Table と監査項目により以下を取得可能。

| 内容 | 取得可否 |
|---|---|
| 変更前の値 | ○ |
| 変更後の値 | ○ |
| 更新日時 | ○ |
| 更新者 | ○ |
| アプリ更新か DB 更新か | ○ |
| 削除前の値（論理削除） | ○ |
| 過去時点データ（Time Travel Query） | ○ |

------

# 9. 取得できない情報

以下は Temporal Table では取得できない。

| 内容 | 取得可否 | 代替手段 |
|---|---|---|
| どの画面から変更したか | × | UseCase ログ（Application層で実装） |
| 変更理由 | × | 業務テーブル（ChangeReason カラムを別途追加） |
| 承認した | × | 承認テーブル（Approval Entity として実装） |
| 差戻した | × | ワークフロー状態テーブル（WorkflowStatus として実装） |
| ログイン履歴 | × | SQL Server Audit |

------

# 10. SQL Server Audit の位置づけ

本システムでは原則として DB 直接更新を禁止する。

ただし内部統制上、誰が SQL を実行したかまで追跡する必要が生じた場合は SQL Server Audit を利用する。

役割分担は以下とする。

| 機能 | Temporal Table | SQL Server Audit |
|---|---|---|
| 変更前後の値 | ○ | × |
| 削除履歴（論理削除） | ○ | × |
| 過去時点復元 | ○ | × |
| 実行ユーザー | △（UpdatedBy） | ○ |
| 実行 SQL | × | ○ |
| DB 直接実行の検知 | △（UpdatedSource='DB'） | ○ |

------

# 11. 監査対象テーブル選定基準

### 対象テーブル

- **主要業務テーブル** — Orders, OrderDetails, Customers, Products, ProductionPlans
- **マスタテーブル** — 業務に影響を与えるもの（DepartmentMaster, EmployeeMaster 等）
- **金銭・数量関係** — 金額、在庫数量、生産数量等

### 非対象テーブル

- **一時テーブル** — Session中のみ存在するテーブル
- **ログテーブル** — 既にログ目的の履歴を持つテーブル
- **統計・キャッシュ** — 計算結果キャッシュ等、導出データ

### 判定ルール

監査対象判定フロー：

```
業務テーブルか？
  ├─ YES → マスタか取引データか？
  │          ├─ マスタ → 【対象】
  │          └─ 取引 → 金銭・数量影響か？
  │                     ├─ YES → 【対象】
  │                     └─ NO → 【非対象】
  └─ NO → 【非対象】
```

------

# 12. 履歴保持期間

### 方針

Temporal Table の履歴保持期間は、法令・コンプライアンス要件に基づき定義する。

### 検討項目

| 項目 | 内容 | 決定必須 |
|---|---|---|
| 基本保持期間 | デフォルト何年か | ○ |
| 法令保持期間 | 業法により異なる期間 | ○ |
| アーカイブ方針 | 期間終了後、いつまで保持するか | ○ |
| 容量計画 | 履歴データサイズの見積もり | ○ |

**当面：** 保持期間は運用方針にて決定。初期設定は【未定】。

------

# 13. Advance プロジェクトルール整合性

### 13.1 IClock ルール準拠

- ✅ **CreatedAt** — Application層で IClock 経由で取得
- ✅ **UpdatedAt** — DB側 Trigger で SYSUTCDATETIME() 設定
  - 理由：UPDATE時刻は DB側の状態であり、Application側での取得制御不可

### 13.2 論理削除ルール準拠

- ✅ **IsDeleted** — 誤登録対応のみ
- ✅ **業務状態** — ValueObject / enum で表現（論理削除ではない）

### 13.3 DDD 準拠

- ✅ **監査ロジック** — Trigger/DB側で実装、Domain層に持ち込まない
- ✅ **監査項目** — 技術的関心事として Infrastructure層相当

------

# 14. 結論

SupportAdvance の監査方式としては、以下を採用する：

```
CreatedAt（App側IClock取得）
+ CreatedBy（Trigger自動設定）
+ UpdatedAt（Trigger自動設定）
+ UpdatedBy（Trigger自動設定）
+ UpdatedSource（Trigger自動判定）
+ IsDeleted（論理削除フラグ）
+ SQL Server Temporal Table
+ SESSION_CONTEXT
+ Trigger（自動更新ロジック）
```

これにより：

- ✅ RepoDB に追加ライブラリ不要
- ✅ DDD に監査ロジックを持ち込まない
- ✅ アプリ更新と DB 直接更新を識別可能
- ✅ 変更前後の全履歴を自動保存可能
- ✅ 将来的に SQL Server Audit を追加可能
- ✅ Advance の IClock・論理削除ルールに準拠
- ✅ 業務状態と物理削除の区別が明確

というシンプルで保守性の高い、かつプロジェクトルール準拠の監査基盤を実現できる。
