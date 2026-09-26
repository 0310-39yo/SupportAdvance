# 日付系 ValueObject 仕様書

**版:** 1.0
**作成日:** 2026-09-26
**対象読者:** 実装者・テスト実装者
**対象:** 任意の日付を持つ値オブジェクト（Employee・Department・Authentication）

日付を持つ値オブジェクトは、**未設定を `null` ではなく型で表す**（[null 厳格性設計ガイド](../../../../Assistance/Guides/null厳格性設計ガイド.md)）。日付は `LocalDateTime` で持ち、`DateTime`（DB の型）は知らない。DB との変換は Infrastructure（Mapper / Repository）が行う（[LocalDateTime 使用規則](../../../../../CLAUDE.md#-localdatetime-使用規則)）。

---

## 1. 一覧

| 型 | 場所 | 意味 | 未設定の生成 | 「設定済み」の別名 | 未設定の `ToString()` |
|---|---|---|---|---|---|
| `RetiredOn` | Employee.Domain（`ValueObjects/Employee`） | 退職日 | `Unset()` | `HasRetired` | 現職 |
| `EndOn` | Employee.Domain（`ValueObjects/DepartmentMembership`） | 所属の終了日 | `Unset()` | `HasEnded` | 無期限 |
| `ExpirationOn` | Employee.Domain（`ValueObjects/Role`） | ロール・権限の失効日 | `Unlimited`（プロパティ） | `HasExpiration` | 無期限 |
| `EffectiveAt` | Employee.Domain（`ValueObjects/Role`） | ロール・権限の有効開始日時（**必須**。未設定なし） | - | - | - |
| `AbolishedOn` | Department.Domain（`ValueObjects`） | 部署の廃止日 | `Unset()` | `IsAbolished` | Not Abolished |
| `LoggedOutAt` | Authentication.Domain（`ValueObjects`） | ログアウト日時 | `Unset()` | `HasLoggedOut` | 未ログアウト |

## 2. 共通の規則

- **生成**: `From(LocalDateTime)`（設定済み）。`TryFrom(LocalDateTime?)` を持つ型（`AbolishedOn`、`LoggedOutAt`）は、`null` を `Unset()` に変換して成功を返す
- **`Value`**: 未設定のときは `LocalDateTime.MinValue`（`null` ではない）。ただし `EndOn.Value` だけは `LocalDateTime?` で、未設定は `null`（`HasEnded` で判定する）
- **等価性**: 「設定済みか」と「値」の両方が同じなら等価。未設定どうしは等価。未設定と設定済みは非等価。`null` との比較は `false`
- **判定は別名で行う**: `entity.RetiredOn == null` のような判定は Domain では起こらない。`HasRetired` などの別名プロパティを使う（[ValueObject 別名プロパティガイド](../../../../Assistance/Guides/ValueObject別名プロパティガイド.md)）
- **DB 変換メソッドを持たない**: `FromDbValue(DateTime)`・`TryFromDbValue(DateTime?)`・`ToDbValue()` は 2026-09-26 に全て削除済み

## 3. 型ごとの補足

### `EndOn`（所属の終了日）

`DepartmentMembership.IsActive(asOf)` が使う。`asOf < EndOn` の間は有効で、終了日の時刻ちょうどから無効（[DepartmentMembership 設計書](../Entities/DepartmentMembership_設計書.md) §4-1）。

### `ExpirationOn`（失効日）

`Unlimited` は、メソッドではなく**静的プロパティ**（`ExpirationOn.Unlimited`）。他の型の `Unset()` に相当する。

### `EffectiveAt`（有効開始日時）

必須の値。`Unset` を持たない。`RoleAssignment` / `PermissionAssignment` の `IsActive(asOf)` は、`EffectiveDate`（型は `EffectiveAt`）と `ExpirationDate`（型は `ExpirationOn`）で有効期間を判定する（開始日と同日は有効、失効日以後は無効）。

### `LoggedOutAt`（ログアウト日時）

未設定（`HasLoggedOut == false`）は、アプリが正常に終了していない、またはまだログイン中であることを表す。`UserAuthSession.IsActive()` は `LoginSuccess && !LoggedOutAt.HasLoggedOut`。

## 4. テスト

| 型 | テスト |
|---|---|
| `RetiredOn` | `tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/RetiredOnTests.cs` |
| `EndOn` | `tests/Contexts/Employee.Domain.Tests/ValueObjects/DepartmentMembership/EndOnTests.cs` |
| `ExpirationOn`、`EffectiveAt` | `tests/Contexts/Employee.Domain.Tests/ValueObjects/Role/` |
| `AbolishedOn` | `tests/Contexts/Department.Domain.Tests/ValueObjects/AbolishedOnTests.cs` |
| `LoggedOutAt` | `tests/Contexts/Authentication.Domain.Tests/ValueObjects/LoggedOutAtTests.cs`、`Entities/UserAuthSessionTests.cs` |

いずれも、生成・未設定・等価性・`ToString` を確認する最小のテスト。観点ID（`VO-XXX-nn`）形式の仕様表は作っていない（型が単純なため、本書の規則が仕様）。
