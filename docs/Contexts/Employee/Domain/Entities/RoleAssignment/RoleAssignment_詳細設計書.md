# RoleAssignment - 詳細設計書

**対象者**: AI実装者（コード生成指示）

**版**: 1.0  
**作成日**: 2026-08-10

---

## 📐 クラス構成

### クラス定義

```csharp
namespace SupportAdvance.Contexts.Employee.Domain.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ロール割り当てエンティティ（ロール有効期間管理）
///
/// 【集約根ID】RoleAssignmentId（GUID ベース）、Entity&lt;TId&gt;.Id で公開
/// 【公開プロパティ】RoleCode（ロール）、EffectiveDate（開始日）、ExpirationDate（終了日）
/// 【責務】従業員のロール割り当てと有効期間を管理、有効期限チェック
/// </summary>
public sealed class RoleAssignment : Entity<RoleAssignmentId>
{
    // ... 実装内容（後続セクション参照）
}
```

---

## 🏗️ 実装構成

### 1. プロパティ（get-only）

```csharp
/// <summary>ロールコードを取得する</summary>
public RoleCode RoleCode { get; private set; }

/// <summary>有効開始日時を取得する</summary>
public LocalDateTime EffectiveDate { get; private set; }

/// <summary>有効終了日時を取得する（null=無期限）</summary>
public LocalDateTime? ExpirationDate { get; private set; }
```

---

### 2. プライベートコンストラクタ

```csharp
/// <summary>
/// 指定されたプロパティから RoleAssignment を生成する（プライベートコンストラクタ）
/// </summary>
/// <param name="id">ロール割り当てID</param>
/// <param name="roleCode">ロールコード</param>
/// <param name="effectiveDate">有効開始日時</param>
/// <param name="expirationDate">有効終了日時（null許可）</param>
/// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
private RoleAssignment(
    RoleAssignmentId id,
    RoleCode roleCode,
    LocalDateTime effectiveDate,
    LocalDateTime? expirationDate = null)
{
    Id = id;
    RoleCode = roleCode;
    EffectiveDate = effectiveDate;
    ExpirationDate = expirationDate;
}
```

**責務**: 
- Entity<TId> の基本構成（Id, RoleCode, EffectiveDate, ExpirationDate）
- プライベート化で不正生成を防止

---

### 3. Create ファクトリメソッド

```csharp
/// <summary>
/// 新しい RoleAssignment を生成する（ファクトリメソッド）
/// </summary>
/// <param name="roleCode">ロールコード</param>
/// <param name="effectiveDate">有効開始日時</param>
/// <param name="expirationDate">有効終了日時（null許可）</param>
/// <returns>生成された RoleAssignment インスタンス</returns>
/// <remarks>
/// パラメータはすべて検証済みの ValueObject として渡される。
/// RoleAssignment レベルでの追加検証は不要。
/// </remarks>
public static RoleAssignment Create(
    RoleCode roleCode,
    LocalDateTime effectiveDate,
    LocalDateTime? expirationDate = null)
{
    return new(
        RoleAssignmentId.NewId(),
        roleCode,
        effectiveDate,
        expirationDate);
}
```

**責務**:
- Application 層での新規 RoleAssignment 生成
- RoleAssignmentId を自動採番（GUID）
- パラメータは既に検証済みと仮定

---

### 4. Reconstruct ファクトリメソッド

```csharp
/// <summary>
/// DB から読み込んだ値から RoleAssignment を復元する（ファクトリメソッド）
/// </summary>
/// <param name="id">ロール割り当てID</param>
/// <param name="roleCode">ロールコード</param>
/// <param name="effectiveDate">有効開始日時</param>
/// <param name="expirationDate">有効終了日時（null許可）</param>
/// <returns>復元された RoleAssignment インスタンス</returns>
/// <remarks>
/// DB 値は既に検証済みと仮定。検証なしで復元。
/// </remarks>
public static RoleAssignment Reconstruct(
    RoleAssignmentId id,
    RoleCode roleCode,
    LocalDateTime effectiveDate,
    LocalDateTime? expirationDate = null)
{
    return new(id, roleCode, effectiveDate, expirationDate);
}
```

**責務**:
- Infrastructure 層での RoleAssignment 復元
- DB から読み込んだ値をそのまま使用

---

### 5. ビジネスロジック - IsActive

```csharp
/// <summary>
/// このロール割り当てが指定時点で有効かどうかを判定する
/// </summary>
/// <param name="asOf">判定時点</param>
/// <returns>EffectiveDate 以後かつ ExpirationDate 前なら true</returns>
public bool IsActive(LocalDateTime asOf)
{
    // EffectiveDate より前なら無効
    if (asOf < EffectiveDate)
    {
        return false;
    }

    // ExpirationDate がない場合は常に有効
    if (ExpirationDate == null)
    {
        return true;
    }

    // asOf が ExpirationDate より前なら有効、以後なら無効
    return asOf < ExpirationDate;
}
```

**責務**:
- 指定時点での有効性判定
- 開始日時・終了日時ロジック
- 返り値: true=有効, false=無効

**ロジック詳細**:
1. `asOf < EffectiveDate` → false（開始前）
2. `ExpirationDate == null` → true（無期限）
3. `asOf < ExpirationDate` → true（期間内）
4. `asOf >= ExpirationDate` → false（終了以後）

---

### 6. ToString メソッド

```csharp
/// <summary>
/// RoleAssignment の文字列表現を取得する
/// </summary>
/// <returns>ロール割り当ての説明文字列</returns>
public override string ToString()
    => $"RoleAssignment(Id={Id.Value}, Code={RoleCode}, Effective={EffectiveDate}, Expiration={ExpirationDate})";
```

**責務**:
- ログ出力、デバッグ用の表現
- 形式: `RoleAssignment(Id=..., Code=..., Effective=..., Expiration=...)`

---

## 📝 using ステートメント

```csharp
using SupportAdvance.Common.Clocks;  // LocalDateTime
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;  // RoleAssignmentId, RoleCode
using SupportAdvance.SharedKernel.Entities;  // Entity<TId>
```

---

## 🧪 テスト対象メソッド

| メソッド | テストグループ | テストケース数 |
|---------|---------------|----------------|
| **Create** | グループ1 | 3+ |
| **Reconstruct** | グループ2 | 3+ |
| **IsActive** | グループ3 | 5+ |
| **プロパティ** | グループ4 | 4+ |
| **等価性** | グループ5 | 5+ |
| **統合** | グループ6 | 2+ |

**総テスト数**: 20+

---

## 📋 実装チェックリスト

- [ ] クラス定義: `sealed class RoleAssignment : Entity<RoleAssignmentId>`
- [ ] プロパティ: RoleCode, EffectiveDate, ExpirationDate（すべてprivate set）
- [ ] プライベートコンストラクタ（4パラメータ）
- [ ] Create メソッド（3パラメータ、RoleAssignmentId.NewId()）
- [ ] Reconstruct メソッド（4パラメータ）
- [ ] IsActive メソッド（EffectiveDate/ExpirationDate ロジック）
- [ ] ToString メソッド
- [ ] using ステートメント（3個）
- [ ] XML コメント（すべてのメンバー）

---

## 参考資料

- **DepartmentMembership**: 同じパターンの実装例
  - ファイル: `src/Contexts/Employee/Employee.Domain/Entities/DepartmentMembership.cs`
  - テスト: `tests/Contexts/Employee.Domain.Tests/Entities/DepartmentMembershipTests.cs`

- **ValueObject**: RoleAssignmentId, RoleCode
  - ファイル: `src/Contexts/Employee/Employee.Domain/ValueObjects/Employee/`

- **Entity基底**: Entity<TId>
  - ファイル: `src/SharedKernel/Entities/Entity.cs`
