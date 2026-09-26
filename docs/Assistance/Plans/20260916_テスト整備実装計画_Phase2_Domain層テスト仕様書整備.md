# Phase 2: Domain層 テスト・ドキュメント整備 実装計画

**作成日:** 2026-09-16
**対象読者:** 本タスクを実装する担当者（人間 または 別セッションのAIエージェント）
**位置づけ:** テスト・テスト仕様書整備プロジェクトの Phase 2。**Phase 0（整合性修正）完了後**に着手可能。Phase 1（Department BCテスト基盤）とは独立に並行着手可。
**前提知識:** リポジトリのルート `CLAUDE.md`（特に「LocalDateTime 使用規則」「Domain層 null 厳格性原則」）を事前に読むこと。

---

## 0. 背景・目的

Domain層（Entity / ValueObject / DomainService）はビジネスルールそのものであり、テスト・ドキュメント整備の中で最優先度が高い。全ソース棚卸しの結果、以下のクラスでテストコード・テスト仕様書の一方または両方が欠落していることが判明した。本Phaseでこれらをまとめて解消する。

**重要な注意（Phase 0 で判明した事実）:** ソースコードは頻繁にリファクタリングされている（例: Employee ドメインの ValueObject 名がリファクタリングで変わっていた）。本ドキュメントに記載のクラス名・シグネチャは調査時点（2026-09-16）のものであり、実装着手前に必ず対象クラスの実ソースファイルを `Read` して現在のAPIを確認すること。

**テンプレート:**
- ValueObject: `docs/Assistance/Templates/Test/TEMPLATE_単体テスト仕様書_ValueObject.md`
- Entity: `docs/Assistance/Templates/Test/TEMPLATE_単体テスト仕様書_Entity.md`
- 汎用（ドメインイベント等）: `docs/Assistance/Templates/Test/TEMPLATE_単体テスト仕様書_汎用.md`

**ドキュメント配置の慣習:** ValueObject/Entity は1クラス1ファイル（例: `docs/SharedKernel/ValueObjects/Audit/CreatedAt_単体テスト仕様書.md`）。`docs/` 配下のパスは `src/` のフォルダ構成（Context名 → Domain → ValueObjects/Entities → サブフォルダ）に対応させる。

**参考にすべき「完成度の高い」既存ドキュメント:**
- ValueObject: `docs/SharedKernel/ValueObjects/Audit/CreatedAt_単体テスト仕様書.md`
- Entity: `docs/Contexts/Employee/Domain/Entities/Employee/Employee_単体テスト仕様書.md`

---

## 1. Common層 — `src/Common/Clocks/`（テストコード・ドキュメントとも0件）

対応するテストプロジェクト `tests/Common.Tests` は `.slnx` 登録済みだがテストファイルが1つもない（Phase 0-7 で確認済み）。本Phaseで実体を追加する。

| クラス | ファイル | 種別 | 優先度 | 備考 |
|---|---|---|---|---|
| `LocalDateTime` | `src/Common/Clocks/LocalDateTime.cs` | ValueObject(struct) | **高** | JST/UTC変換、比較演算子、加減算ロジックを持つ。プロジェクト全体で最も使用頻度が高い基盤型 |
| `ClockFactory` | `src/Common/Clocks/ClockFactory.cs` | 汎用（ファクトリ） | 中 | `ClockType` 分岐・不正値での例外ロジックあり |
| `SystemClock` | `src/Common/Clocks/SystemClock.cs` | 汎用（IClock実装） | 中 | `DateTime.UtcNow`→`LocalDateTime`変換の実装確認 |
| `OffsetClock` | `src/Common/Clocks/OffsetClock.cs` | 汎用（IClock実装） | 中 | 固定オフセットを加算する実装 |
| `TickingClock` | `src/Common/Clocks/TickingClock.cs` | 汎用（IClock実装） | 中 | `Timer`/`lock`を使うステートフルな実装。テスト時は待機時間に注意 |
| `MockClock` | `src/Common/Clocks/MockClock.cs` | 汎用（IClock実装、テストダブル） | 中 | `SetDateTime`/`Advance`/`Reset`の検証ロジックを持つ公開クラス。他プロジェクトのテストで多用されているため正しい振る舞いの保証価値が高い |

**作業手順:**
1. `src/Common/Clocks/` 配下の各クラスを `Read` し、現在のAPIを確認する。
2. `tests/Common.Tests/Clocks/` 配下に `LocalDateTimeTests.cs`, `ClockFactoryTests.cs`, `SystemClockTests.cs`, `OffsetClockTests.cs`, `TickingClockTests.cs`, `MockClockTests.cs` を作成する。
3. ドキュメントは `docs/Common/Clocks/{ClassName}_単体テスト仕様書.md` に作成する（`docs/Common/` フォルダは `SupportAdvance.slnx` に solution folder `/docs/Common/` として既に定義済み）。`LocalDateTime` はValueObjectテンプレート、他はIClock実装として汎用テンプレートを使用。

---

## 2. Employee.Domain — Entity 2件・ValueObject 12件・DomainEvent 1件が欠落

対象: `src/Contexts/Employee/Employee.Domain/`。テストプロジェクト: `tests/Contexts/Employee.Domain.Tests/`（ビルド green 確認済み）。

### 2-1. テストコード・ドキュメントとも欠落

| クラス | ファイル | 種別 |
|---|---|---|
| `Entities.Person` | `Entities/Person.cs` | Entity |
| `Entities.DepartmentMembership` | `Entities/DepartmentMembership.cs` | Entity |
| `DomainEvents.EmployeeRetiredEvent` | `DomainEvents/EmployeeRetiredEvent.cs` | 汎用（ドメインイベント。`Employee.RetireEmployee()` から発行される） |
| `ValueObjects.Employee.BizCode` | `ValueObjects/Employee/BizCode.cs` | ValueObject |
| `ValueObjects.Employee.BizDivision` | `ValueObjects/Employee/BizDivision.cs` | ValueObject（Enum系） |
| `ValueObjects.Employee.BizId` | `ValueObjects/Employee/BizId.cs` | ValueObject |
| `ValueObjects.Employee.RetiredOn` | `ValueObjects/Employee/RetiredOn.cs` | ValueObject（IOptionalValueObject） |
| `ValueObjects.DepartmentMembership.EndOn` | `ValueObjects/DepartmentMembership/EndOn.cs` | ValueObject（IOptionalValueObject） |
| `ValueObjects.DepartmentMembership.IsPrimary` | `ValueObjects/DepartmentMembership/IsPrimary.cs` | ValueObject |
| `ValueObjects.Person.FirstName` | `ValueObjects/Person/FirstName.cs` | ValueObject |
| `ValueObjects.Person.FirstNameKana` | `ValueObjects/Person/FirstNameKana.cs` | ValueObject |
| `ValueObjects.Person.LastName` | `ValueObjects/Person/LastName.cs` | ValueObject |
| `ValueObjects.Person.LastNameKana` | `ValueObjects/Person/LastNameKana.cs` | ValueObject |
| `ValueObjects.Role.EffectiveAt` | `ValueObjects/Role/EffectiveAt.cs` | ValueObject |
| `ValueObjects.Role.ExpirationOn` | `ValueObjects/Role/ExpirationOn.cs` | ValueObject（IOptionalValueObject） |

作成先テストコード: `tests/Contexts/Employee.Domain.Tests/Entities/{Person,DepartmentMembership}Tests.cs`、`tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/*Tests.cs`（既存ファイルが集まっているフォルダ。命名は既存の兄弟ファイル `BizCodeTests.cs` 等と揃える）、`tests/Contexts/Employee.Domain.Tests/DomainEvents/EmployeeRetiredEventTests.cs`。

作成先ドキュメント: `docs/Contexts/Employee/Domain/Entities/{Person,DepartmentMembership}/{ClassName}_単体テスト仕様書.md`、`docs/Contexts/Employee/Domain/ValueObjects/{Employee,DepartmentMembership,Person,Role}/{ClassName}_単体テスト仕様書.md`、`docs/Contexts/Employee/Domain/DomainEvents/EmployeeRetiredEvent_単体テスト仕様書.md`。

**注意:** 既存の `docs/Contexts/Employee/ValueObjects/Identifiers/` という配置（`Domain` を挟まない）は Department BC 分割前の古い慣習である可能性がある。本Phaseの新規作成分は `docs/Contexts/Employee/Domain/...` のように `src` のフォルダ構成（`Employee.Domain/Entities`, `Employee.Domain/ValueObjects`）によりそった配置を採用すること。

### 2-2. テストコードは既存、ドキュメントのみ欠落

| クラス | テストコード |
|---|---|
| `ValueObjects.Permission.PermissionAssignmentRowId` | `tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/PermissionAssignmentRowIdTests.cs` |
| `ValueObjects.Role.RoleAssignmentRowId` | `tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/RoleAssignmentRowIdTests.cs` |
| `ValueObjects.DepartmentMembership.DepartmentMembershipRowId` | `tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/DepartmentMembershipRowIdTests.cs` |

これら3件は実装済みテストコードの内容をそのままドキュメント化するだけでよい（新規テスト作成は不要）。配置先: `docs/Contexts/Employee/Domain/ValueObjects/{Permission,Role,DepartmentMembership}/{ClassName}_単体テスト仕様書.md`

**参考（既にドキュメント化済みの例）:** `docs/Contexts/Employee/ValueObjects/Identifiers/PermissionCode_単体テスト仕様書.md`, `RoleCode_単体テスト仕様書.md` は `Permission.PermissionCode`/`Role.RoleCode` の既存ドキュメント。配置パスの慣習（`Domain/ValueObjects/...` にするか現状の `ValueObjects/Identifiers/...` のままにするか）に迷う場合は、既存ファイルと同じ配置に揃えてよい（`docs/Contexts/Employee/ValueObjects/Identifiers/{ClassName}_単体テスト仕様書.md`）。どちらの配置方針を採るかは着手前に一度リポジトリの既存慣習を確認し、Phase 2〜4全体で一貫させること。

---

## 3. Department.Domain — テストコードは既存、ドキュメントが欠落

対象: `src/Contexts/Department/Department.Domain/`。テストプロジェクト: `tests/Contexts/Department.Domain.Tests/`（登録済み、テストコードは全クラス揃っている）。

**注意:** `DepartmentCode`, `HierarchyLevel`, `ManagerEmployeeRowId`, `ParentDepartmentRowId`, `SharedKernel`版`DepartmentRowId` の5件は **Phase 0-3 で対応済み**（Employee側の古いドキュメントを移設・書き直し）。本Phaseで重複作成しないこと。

本Phaseで新規に作成が必要なもの:

| クラス | ファイル | テストコード |
|---|---|---|
| `Entities.Department` | `Entities/Department.cs` | `tests/Contexts/Department.Domain.Tests/Entities/DepartmentTests.cs` |
| `ValueObjects.AbolishedOn` | `ValueObjects/AbolishedOn.cs` | `tests/Contexts/Department.Domain.Tests/ValueObjects/AbolishedOnTests.cs` |

配置先ドキュメント: `docs/Contexts/Department/Domain/Entities/Department/Department_単体テスト仕様書.md`、`docs/Contexts/Department/Domain/ValueObjects/AbolishedOn_単体テスト仕様書.md`

---

## 4. Authentication.Domain — テストコードは既存（充実）、ドキュメントが0件

対象: `src/Contexts/Authentication/Authentication.Domain/`。テストプロジェクト: `tests/Contexts/Authentication.Domain.Tests/`（登録済み、全クラスに対して充実したテストが既に存在）。`docs/Contexts/Authentication/` は現在完全に空。

| クラス | テストコード | テスト数目安 |
|---|---|---|
| `Entities.UserAuthSession` | `tests/Contexts/Authentication.Domain.Tests/Entities/UserAuthSessionTests.cs` | 16件 |
| `ValueObjects.AuthMethod` | `tests/Contexts/Authentication.Domain.Tests/ValueObjects/AuthMethodTests.cs` | 24件 |
| `ValueObjects.AuthorityRowId` | `tests/Contexts/Authentication.Domain.Tests/ValueObjects/AuthorityRowIdTests.cs` | 13件 |
| `ValueObjects.LoginCredentialsRowId` | `tests/Contexts/Authentication.Domain.Tests/ValueObjects/LoginCredentialsRowIdTests.cs` | 12件 |
| `ValueObjects.LoginId` | `tests/Contexts/Authentication.Domain.Tests/ValueObjects/LoginIdTests.cs` | 18件 |
| `ValueObjects.UserAuthSessionRowId` | `tests/Contexts/Authentication.Domain.Tests/ValueObjects/UserAuthSessionRowIdTests.cs` | 14件 |

すべて**新規テスト作成は不要**、既存テストコードの内容をそのままテンプレートに沿ってドキュメント化するだけでよい（着手コストが低く、成果が出しやすいタスク）。

配置先: `docs/Contexts/Authentication/Domain/Entities/UserAuthSession/UserAuthSession_単体テスト仕様書.md`、`docs/Contexts/Authentication/Domain/ValueObjects/{AuthMethod,AuthorityRowId,LoginCredentialsRowId,LoginId,UserAuthSessionRowId}_単体テスト仕様書.md`

---

## 5. SharedKernel — RowId基底・派生2件が欠落

対象: `src/SharedKernel/ValueObjects/Identifiers/`。テストプロジェクト: `tests/SharedKernel.Tests/`（登録済み）。

| クラス | 現状 |
|---|---|
| `Identifiers.RowId`（抽象基底） | テストは `tests/SharedKernel.Tests/ValueObjects/Identifiers/RowIdTests.cs` に `TestRowId`/`TestPersonRowId` フィクスチャ経由で存在するが、専用のドキュメントが無い |
| `Identifiers.DepartmentRowId`（SharedKernel版） | Phase 0-3 で対応済み（`docs/SharedKernel/ValueObjects/Identifiers/DepartmentRowId_単体テスト仕様書.md` を作成済みの前提） |
| `Identifiers.EmployeeRowId`（SharedKernel版） | テストコード・ドキュメントとも欠落。**Employee.Domain 側の `EmployeeRowId` とは別物なので混同しないこと**（Employee.Domain には独自の `EmployeeRowId` は存在せず、`SharedKernel` 版をそのまま使っている。`tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/EmployeeRowIdTests.cs` は実際にはこの SharedKernel 版を対象にしている） |

**作業手順:**
1. `RowId` 抽象基底クラスのドキュメントを `docs/SharedKernel/ValueObjects/Identifiers/RowId_単体テスト仕様書.md` として作成（既存の `RowIdTests.cs` を正とする）。
2. `EmployeeRowId` は既存テスト `tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/EmployeeRowIdTests.cs` を正としてドキュメント化するだけでよい（新規テスト作成不要）。配置先: `docs/SharedKernel/ValueObjects/Identifiers/EmployeeRowId_単体テスト仕様書.md`。

---

## 6. 検証方法

```bash
dotnet build --no-incremental
dotnet test
```

- 新規追加した全テストクラスが green であること
- `Common.Tests` プロジェクトが実体を持つようになったこと（テスト件数が0から増えること）
- 全ドキュメントがテンプレートの章立てを満たし、既存の完成度の高い例とフォーマットが揃っていること
- `docs/` の配置パスが `src/` のフォルダ構成に対応していること（本Phェーズ内で採用した配置方針をPhase 3・4でも一貫させること）

## 7. 本Phaseでやらないこと

- Application/Infrastructure層のテスト・ドキュメント（Phase 3）
- 結合・システムテスト仕様書（Phase 5）
- Department BC の Application/Infrastructure（Phase 1 の範囲）
