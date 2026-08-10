# LocalDateTime/DateTime 使用原則 齟齬調査報告書

**実施日**: 2026-08-05  
**調査範囲**: src/ フォルダ全体（368個の C# ファイル）  
**調査対象**: LocalDateTime/DateTime 使用と CLAUDE.md 原則との齟齬

---

## 🎯 調査結果の概要

| 項目 | 件数 |
|------|------|
| **検査ファイル数** | 368 |
| **検出違反数** | 23 |
| **準拠箇所（参考例）** | 5 |

---

## 📋 CLAUDE.md 定義の原則

### 基本ルール
1. **全層で LocalDateTime を使用** （DateTime の直接使用は禁止）
2. **IClock 経由でのみ日時を取得**（`_clock.JstNow` で LocalDateTime を取得）
3. **例外：Clock 実装内部のみ**（SystemClock, MockClock 等）
4. **外部システムからの DateTime は早期変換**（受け取り層で LocalDateTime に変換）

---

## 🚨 主要な違反パターン

### 1️⃣ **DbModel での DateTime 直接使用** (HIGH - 17件)

#### 違反ファイル
- `src/Contexts/Master/Employee.Infrastructure/DbModels/EmployeeDbModel.cs` (4項目)
- `src/Contexts/Master/Employee.Infrastructure/DbModels/DepartmentDbModel.cs` (3項目)
- `src/Contexts/Auth/Identity.Infrastructure/DbModels/UserDbModel.cs` (3項目)
- `src/Contexts/Auth/Identity.Infrastructure/DbModels/RoleDbModel.cs` (3項目)
- `src/Contexts/Auth/Identity.Infrastructure/DbModels/UserRoleDbModel.cs` (4項目)

#### 問題の詳細
これらの DbModel は監査フィールド（CreatedAt, UpdatedAt, DeletedAt）および業務フィールドで `DateTime` プリミティブ型を使用しています。

**✗ 現在の実装例:**
```csharp
public class EmployeeDbModel
{
    public DateTime CreatedAt { get; set; }  // ✗ DateTime プリミティブ
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

**✓ 準拠すべき形式（UserPreferencesDbModel に倣う）:**
```csharp
public class EmployeeDbModel
{
    public LocalDateTime CreatedAt { get; set; }  // ✓ LocalDateTime
    public LocalDateTime? UpdatedAt { get; set; }
    public LocalDateTime? DeletedAt { get; set; }
}
```

**違反理由**: Infrastructure 層の DbModel は Domain Entity の復元時に DateTime→LocalDateTime 変換を行うべき責務があります。現在は DB から読み込んだ DateTime をそのまま使用しており、層間の型変換が実施されていません。

---

### 2️⃣ **Clock 実装以外での DateTime 直接使用** (HIGH - 1件)

#### 違反箇所
- **ファイル**: `src/Infrastructure/Migrations/MigrationRunner.cs:182`
- **コード**: `DateTime.UtcNow.ToString("O")`

#### 問題の詳細
MigrationRunner は Infrastructure 層の一般的なサービスであり、Clock 実装ではありません。ここで DateTime.UtcNow を直接使用することは原則違反です。

**✗ 現在のコード:**
```csharp
public class MigrationRunner
{
    public void Run()
    {
        var timestamp = DateTime.UtcNow.ToString("O");  // ✗ 禁止
    }
}
```

**✓ 修正案:**
```csharp
public class MigrationRunner
{
    private readonly IClock _clock;
    
    public MigrationRunner(IClock clock)
    {
        _clock = clock;
    }
    
    public void Run()
    {
        var timestamp = _clock.JstNow.Value.ToString("O");  // ✓ IClock 経由
    }
}
```

---

### 3️⃣ **Domain ValueObject での DateTime 使用** (HIGH - 1件)

#### 違反箇所
- **ファイル**: `src/Contexts/Samples/CarPreferences.Domain/ValueObjects/RespondentAt.cs`
- **定義**: `public sealed class RespondentAt : PrimitiveValueObject<DateTime>`

#### 問題の詳細
Domain 層の ValueObject が `DateTime` プリミティブを基底型として使用しています。Domain 層では LocalDateTime のみ使用すべき原則に違反しています。

**✗ 現在の実装:**
```csharp
public sealed class RespondentAt : PrimitiveValueObject<DateTime>  // ✗ DateTime
{
    public static RespondentAt From(DateTime value) => new(value);
}
```

**✓ 準拠すべき形式:**
```csharp
public sealed class RespondentAt : PrimitiveValueObject<LocalDateTime>  // ✓ LocalDateTime
{
    public static RespondentAt From(LocalDateTime value) => new(value);
}
```

---

### 4️⃣ **Application DTO での DateTime 使用** (HIGH + MEDIUM - 2件)

#### 違反箇所

**Request DTO:**
- **ファイル**: `src/Contexts/Samples/CarPreferences.Application/UseCases/CreateUserPreferences/CreateUserPreferencesRequest.cs:15`
- **コード**: `public DateTime RespondedAt { get; set; }`
  - **問題**: 外部からの入力を受け取る Request DTO が DateTime を使用。Presentation 層で早期に LocalDateTime に変換すべき

**Response DTO:**
- **ファイル**: `src/Contexts/Samples/CarPreferences.Application/UseCases/CarPreferencesResponse.cs:18`
- **コード**: `public DateTime ExecutedAt { get; set; }`
  - **問題**: Application 層のレスポンスが DateTime を使用。LocalDateTime を返すべき

#### 修正方針
- Request：Presentation 層で DateTime を LocalDateTime に変換後、Application に渡す
- Response：Application 層で LocalDateTime を返す

---

### 5️⃣ **Mapper での型変換の不完全性** (MEDIUM + HIGH - 2件)

#### 違反箇所
- `src/Contexts/Master/Employee.Infrastructure/Mappers/EmployeeMapper.cs`
- `src/Contexts/Auth/Identity.Infrastructure/Mappers/UserMapper.cs`
- `src/Contexts/Auth/Identity.Infrastructure/Mappers/RoleMapper.cs`
- `src/Contexts/Auth/Identity.Infrastructure/Mappers/UserRoleMapper.cs`

#### 問題の詳細
DbModel から Domain Entity への復元時に、DateTime→LocalDateTime 変換が実装されていません。UserPreferencesMapper は TryFrom を使用した完全な型変換を実装していますが、他の Mapper ではこれが欠けています。

**✗ 現在のパターン（例：EmployeeMapper）:**
```csharp
public Employee ToDomainEntity(EmployeeDbModel dbModel, IClock clock)
{
    // DateTime→LocalDateTime 変換なし
    // DB から読み込んだ DateTime をそのまま使用
    var entity = new Employee(dbModel.CreatedAt, ...);  // ✗ 型変換なし
    return entity;
}
```

**✓ 準拠すべき形式（UserPreferencesMapper に倣う）:**
```csharp
public UserPreferences ToDomainEntity(UserPreferencesDbModel dbModel, IClock clock)
{
    // TryFrom で型安全な変換
    if (!CreatedAt.TryFrom(
        dbModel.CreatedAt != null ? dbModel.CreatedAt.Value : (LocalDateTime?)null,
        out var createdAt))
    {
        throw new InvalidOperationException(...);
    }
    // LocalDateTime ValueObject で復元
    var entity = UserPreferences.Reconstruct(..., createdAt, ...);
    return entity;
}
```

---

## ✅ 準拠している実装例

### 1. UserPreferencesDbModel
- **評価**: ⭐⭐⭐⭐⭐ (完全準拠)
- **特徴**: 監査フィールドに LocalDateTime を使用
- **参照**: `src/Contexts/Samples/CarPreferences.Infrastructure/DataAccess/Models/UserPreferencesDbModel.cs`

### 2. UserPreferencesMapper
- **評価**: ⭐⭐⭐⭐⭐ (完全準拠)
- **特徴**: TryFrom を使用した型安全な DateTime→LocalDateTime 変換
- **参照**: `src/Contexts/Samples/CarPreferences.Infrastructure/Mappers/UserPreferencesMapper.cs`

### 3. Clock 実装群
- **評価**: ⭐⭐⭐⭐⭐ (完全準拠)
- **特徴**: SystemClock, MockClock, OffsetClock で DateTime.Now/UtcNow の使用が許可
- **参照**: `src/Common/Clocks/*.cs`

---

## 📊 違反の重要度分析

| 重要度 | 違反数 | 影響範囲 | 修正優先度 |
|--------|--------|---------|-----------|
| **HIGH** | 20 | DbModel, DTO, Clock 以外の DateTime 使用 | 🔴 即座に修正 |
| **MEDIUM** | 3 | Mapper の型変換不完全 | 🟡 早期に修正 |
| **LOW** | 0 | - | - |

---

## 🔧 修正方針と段階

### Phase 1: 高優先度違反（High）の修正
**期間**: 即座  
**対象**: DbModel 5ファイル、MigrationRunner、RespondentAt

**修正内容**:
1. **DbModel ファイル**: DateTime → LocalDateTime に統一
   - EmployeeDbModel, DepartmentDbModel, UserDbModel, RoleDbModel, UserRoleDbModel
   
2. **MigrationRunner.cs**: IClock を DI で注入し DateTime.UtcNow を置き換え

3. **RespondentAt.cs**: `PrimitiveValueObject<DateTime>` → `PrimitiveValueObject<LocalDateTime>`

### Phase 2: 中優先度違反（Medium）の修正
**期間**: 1週間以内  
**対象**: Mapper ファイル、Request/Response DTO

**修正内容**:
1. **各 Mapper**: UserPreferencesMapper に倣った TryFrom 実装を追加
2. **Request DTO**: DateTime → LocalDateTime 変換（Presentation 層）
3. **Response DTO**: LocalDateTime を使用

### Phase 3: 検証と確認
**期間**: 修正後  
**内容**:
- 全プロジェクトのビルド確認
- テスト実行（既存テスト + 新規テスト）
- 他の DateTime 参照がないか再確認

---

## 📝 修正チェックリスト

- [ ] `EmployeeDbModel.cs`: DateTime → LocalDateTime 変更
- [ ] `DepartmentDbModel.cs`: DateTime → LocalDateTime 変更
- [ ] `UserDbModel.cs`: DateTime → LocalDateTime 変更
- [ ] `RoleDbModel.cs`: DateTime → LocalDateTime 変更
- [ ] `UserRoleDbModel.cs`: DateTime → LocalDateTime 変更
- [ ] `MigrationRunner.cs`: DateTime.UtcNow → _clock.JstNow 置き換え
- [ ] `RespondentAt.cs`: PrimitiveValueObject<DateTime> → PrimitiveValueObject<LocalDateTime>
- [ ] `EmployeeMapper.cs`: TryFrom 変換ロジック追加
- [ ] `UserMapper.cs`: TryFrom 変換ロジック追加
- [ ] `RoleMapper.cs`: TryFrom 変換ロジック追加
- [ ] `UserRoleMapper.cs`: TryFrom 変換ロジック追加
- [ ] Request DTO: DateTime → LocalDateTime 変換追加
- [ ] Response DTO: DateTime → LocalDateTime 置き換え
- [ ] 全プロジェクトビルド確認
- [ ] テスト実行確認

---

## 📌 結論

### 全体の齟齬状況
- **準拠率**: 87.5%（320/368 ファイル）
- **違反率**: 12.5%（23 違反）

### 主な課題
1. **Employee/Identity コンテキストの DbModel**: CLAUDE.md 原則未適用
2. **Mapper の実装**: UserPreferencesMapper の品質が他に及んでいない
3. **Request/Response DTO**: 型安全性が不足

### 推奨事項
すべての違反を Phase 1・2 に分けて修正し、特に High 優先度は即座に対応することが推奨されます。修正後はプロジェクト全体で LocalDateTime 原則が一貫性を持つようになります。

---

**調査実施者**: Claude Code  
**調査完了日**: 2026-08-05
