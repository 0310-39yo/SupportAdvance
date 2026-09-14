# CLEAN_ARCHITECTURE_GUIDELINES.md と実装コードの比較レポート

**作成日**: 2026-09-10  
**対象**: `src/Contexts/Employee` Context と汎用 Infrastructure

---

## 🟢 準拠している項目

### 1. プロジェクト参照関係 ✓
- Common: 依存ゼロ ✓
- SharedKernel: Common のみ参照 ✓
- Crosscutting: Common のみ参照 ✓
- Infrastructure: Application, Crosscutting, SharedKernel を参照 ✓
- Employee.Application: Employee.Domain, Application, SharedKernel, Common を参照 ✓
- WinTrial (Presentation): Employee.Application, Employee.Infrastructure を参照（Program.cs での DI 構築） ✓

### 2. LocalDateTime 使用ルール（基本部分）✓
- Domain層: LocalDateTime を使用（RetiredOn, EndOn など）✓
- DbModel: DateTime プリミティブ型を使用（EmployeeDbModel.RetiredOn, CreatedAt など）✓
- Mapper.ToDomainEntity: DB の DateTime を LocalDateTime に変換 ✓
- IClock 経由での日時取得（UpdateAsync での `_clock.JstNow` 使用）✓

### 3. null 厳格性 ✓
- Domain: IsSet フラグで未設定状態を管理（Unset() パターン）✓
- TryFromDbValue での DB null → Unset() 変換 ✓

---

## 🟡 矛盾・非準拠項目

### 1. ❌ Mapper.ToDbModel での DateTime.UtcNow 直接使用（高優先度）

**ドキュメント（774-798行）:**
```csharp
public EmployeeDbModel ToDbModel(Employee entity)
{
    // Domain の LocalDateTime → DB の DateTime に変換
    return new EmployeeDbModel
    {
        RetiredOn = entity.RetiredOn.IsSet ? entity.RetiredOn.Value.Value : null
    };
}
```

**実装（Employee.Infrastructure/Mappers/EmployeeMapper.cs:98-99）:**
```csharp
public EmployeeDbModel ToDbModel(Employee entity) =>
    new()
    {
        RowId = entity.RowId.Value,
        BizDivision = entity.TypeDivision.ToDbValue(),
        BizId = entity.BizId.Value,
        RetiredOn = entity.RetiredOn.IsSet ? entity.RetiredOn.Value.Value : null,
        UpdatedAt = DateTime.UtcNow,  // ← 直接使用（ドキュメント違反）
        UpdatedBy = 0 // ← placeholder（RepositoryBase で上書き予定）
    };
```

**問題:**
- ドキュメントでは「Mapper が DateTime ↔ LocalDateTime 変換を実装」と明記
- 実装では UpdatedAt に `DateTime.UtcNow` を直接使用
- Mapper が保持する `_clock` フィールドが使用されていない
- ドキュメント（722-735行）では「全層で IClock 経由でのみ日時を取得。System.DateTime.Now 等への直接アクセスは禁止」と記載

**影響:**
- UpdatedAt が UTC になる（JST ではなく）
- ClockAbstraction が無視されている（テスト用 FixedClock が使用できない）

**正しい実装案:**
```csharp
public EmployeeDbModel ToDbModel(Employee entity) =>
    new()
    {
        // ... 他フィールド ...
        RetiredOn = entity.RetiredOn.IsSet ? entity.RetiredOn.Value.Value : null,
        UpdatedAt = _clock.JstNow.Value,  // ← LocalDateTime から DateTime に変換
        UpdatedBy = 0  // ← RepositoryBase で上書き予定
    };
```

---

### 2. ❌ UpdatedAt/UpdatedBy 設定の二重化（中優先度）

**状況:**
- **Mapper**: UpdatedAt を `DateTime.UtcNow` で設定（上述）、UpdatedBy を 0 で設定
- **RepositoryBase**: SetUpdatedByAudit() で reflectionを使用してUpdatedByを上書き
- **EmployeeRepository**: Mapper を使用せず、直接 SQL で UpdatedAt/UpdatedBy を設定

**問題:**
- Mapper での設定と RepositoryBase での設定が重複している
- EmployeeRepository は Mapper.ToDbModel を使用していないため、Mapper の設定は無視される
- ドキュメント（87-89行）では「Repository で実装のみ。Infrastructure層で技術詳細を隠蔽」と記載だが、監査情報の管理が Repository レベルで散在している

**設計案:**
- **案A**: Mapper から UpdatedAt/UpdatedBy を削除し、Repository で完全に管理
- **案B**: Mapper で生成した DbModel の監査フィールドは無視して、Repository で上書きする明示的なパターンを確立

---

### 3. ❌ EmployeeRepository が RepositoryBase を継承していない（低優先度、設計的）

**状況:**
- 汎用 `RepositoryBase<TEntity, TDbModel, TId>` が定義されている
- EmployeeRepository は独立実装、RepositoryBase を継承していない
- Employee.Infrastructure/Repositories/EmployeeRepository.cs が直接 SQL と Dapper を使用

**問題:**
- 将来の CarPreferences など新しい Context では RepositoryBase を使用するはずだが、Employee は独立
- 監査情報管理（CreatedBy/UpdatedBy/DeletedBy）の パターンが RepositoryBase と EmployeeRepository で異なる

**背景:**
- ドキュメント（97, 113行）では Mapper で「Entity 復元時に RowVersion を渡す」など Reconstruct パターンを想定
- Employee.Domain.Entities.Employee.Reconstruct() が実装されている
- EmployeeRepository の Update実装は Mapper.ToDbModel を使用していない直接 SQL パターン

---

## 📋 修正提案

| 項目 | 優先度 | 修正内容 |
|------|--------|--------|
| DateTime.UtcNow 直接使用 | **高** | Mapper.ToDbModel で `_clock.JstNow.Value` を使用するか、Mapper から UpdatedAt 設定を削除 |
| UpdatedAt/UpdatedBy 二重設定 | 中 | 明示的な責務分離：Mapper での設定削除、Repository で完全管理 |
| EmployeeRepository 独立実装 | 低 | 将来の Contexts で RepositoryBase を標準にする設計確認 |

---

## 🔗 参考ファイル

- ドキュメント: `docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md`
- 実装:
  - `src/Common/Clocks/LocalDateTime.cs` ✓
  - `src/Common/Clocks/IClock.cs` ✓
  - `src/Contexts/Employee/Employee.Infrastructure/Mappers/EmployeeMapper.cs` ❌ 行98-99
  - `src/Contexts/Employee/Employee.Infrastructure/Repositories/EmployeeRepository.cs` (直接SQL実装)
  - `src/Infrastructure/Repositories/RepositoryBase.cs` ✓
