# Audit 監査タイムスタンプ — LocalDateTime 大原則 修正計画

**作成日**: 2026-08-08  
**優先度**: 🔴 高（クリーンアーキテクチャ違反、型安全性喪失）  
**影響範囲**: SharedKernel, CarPreferences Context 全層, 他の BC（拡張時）

---

## 🔴 現状の違反

### 1. Audit ValueObject が DateTime を保持（Domain層での型安全性喪失）

**ファイル**: `src/SharedKernel/ValueObjects/Audit/CreatedAt.cs` ほか

```csharp
// ✗ 違反: Domain層なのに DateTime を保持
public sealed class CreatedAt : PrimitiveValueObject<DateTime>
{
    public static CreatedAt From(LocalDateTime value) => new(value.Value);  // ← LocalDateTime → DateTime に変換
    public DateTime Value => ValueField;  // ← DateTime を返す
}
```

**問題**:
- Domain層では LocalDateTime を使うべき（CLAUDE.md）
- Audit ValueObject は Domain層で使われる
- 内部値が DateTime なので型安全性がない
- イベント発行時に再度 `new LocalDateTime(value.Value)` が必要 ← 不正

### 2. DbModel が LocalDateTime を使用（Infrastructure層での違反）

**ファイル**: `src/Contexts/Samples/CarPreferences.Infrastructure/DataAccess/Models/UserPreferencesDbModel.cs`

```csharp
// ✗ 違反: Infrastructure層が LocalDateTime を持つべきではない
public class UserPreferencesDbModel
{
    public LocalDateTime CreatedAt { get; set; }
    public LocalDateTime? UpdatedAt { get; set; }
    public LocalDateTime? DeletedAt { get; set; }
}
```

**問題**:
- CLAUDE.md では「Infrastructure/DbModel層: DateTime プリミティブ型のみ」と明記
- LocalDateTime は共通ユーティリティ（Domain層向け）
- DbModel が Domain層の型に依存している

### 3. Mapper での不正な型変換

**ファイル**: `src/Contexts/Samples/CarPreferences.Infrastructure/Mappers/UserPreferencesMapper.cs`

```csharp
// ToDbModel: Entity → DbModel
public UserPreferencesDbModel ToDbModel(UserPreferences entity)
{
    return new UserPreferencesDbModel
    {
        // ✗ DateTime → LocalDateTime（逆方向）
        CreatedAt = new LocalDateTime(entity.CreatedAt.Value),
        UpdatedAt = entity.UpdatedAt.HasUpdated ? new LocalDateTime(entity.UpdatedAt.Value!.Value) : null,
        DeletedAt = entity.DeletedAt.IsDeleted ? new LocalDateTime(entity.DeletedAt.Value!.Value) : null,
    };
}

// ToDomainEntity: DbModel → Domain Entity
public UserPreferences ToDomainEntity(UserPreferencesDbModel dbModel, IClock clock)
{
    // TryFrom に LocalDateTime? を渡す（正しい）
    if (!CreatedAt.TryFrom(dbModel.CreatedAt, out var createdAt))
    {
        // ...
    }
}
```

**問題**:
- `ToDbModel()` で DateTime → LocalDateTime 変換（本来は不要）
- `ToDomainEntity()` で LocalDateTime を TryFrom に渡す
- 循環型変換が発生（DateTime → LocalDateTime → DateTime）

### 4. Entity での型変換の冗長性

**ファイル**: `src/Contexts/Samples/CarPreferences.Domain/Entities/UserPreferences.cs`

```csharp
// イベント発行時
this.RaiseDomainEvent(new PreferencesUpdatedEvent(
    DomainEventId.New(),
    PreferenceChangeType.ModelUpdated,
    oldModel?.ToString() ?? "未設定",
    model.ToString(),
    new LocalDateTime(_updatedAt.Value!.Value)  // ← ValueObject から DateTime を抽出して再度 LocalDateTime に
));
```

**問題**:
- UpdatedAt.Value が DateTime なので、再度 LocalDateTime に変換している
- Audit ValueObject が LocalDateTime を保持していれば不要

---

## ✅ 目指すべき設計（CLAUDE.md に準拠）

```
Domain/Application層:
  Audit ValueObject: LocalDateTime 保持
  ↓
  (型安全, type-safe)
  ↓
Mapper層:
  LocalDateTime ↔ DateTime 双方向変換
  ↓
Infrastructure層:
  DbModel: DateTime プリミティブ型のみ
  (ORM マッピング用, DB ネイティブ型)
```

### 変更後のコード例

**1. Audit ValueObject（CreatedAt.cs）**

```csharp
public sealed class CreatedAt : PrimitiveValueObject<LocalDateTime>, IEquatable<CreatedAt>
{
    private CreatedAt(LocalDateTime value) : base(value, true) { }
    
    // LocalDateTime から生成（Domain層で推奨）
    public static CreatedAt From(LocalDateTime value) => new(value);
    
    // DateTime から生成（Infrastructure層で使用）
    public static CreatedAt FromDbValue(DateTime value) => new(new LocalDateTime(value));
    
    // DB値への変換
    public DateTime ToDbValue() => ValueField.Value;
    
    public LocalDateTime Value => ValueField;
}
```

**2. DbModel（UserPreferencesDbModel.cs）**

```csharp
public class UserPreferencesDbModel
{
    // DateTime プリミティブ型のみ
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

**3. Mapper（UserPreferencesMapper.cs）**

```csharp
public UserPreferencesDbModel ToDbModel(UserPreferences entity)
{
    return new UserPreferencesDbModel
    {
        // LocalDateTime → DateTime（正方向）
        CreatedAt = entity.CreatedAt.ToDbValue(),
        UpdatedAt = entity.UpdatedAt.HasUpdated ? entity.UpdatedAt.ToDbValue() : null,
        DeletedAt = entity.DeletedAt.IsDeleted ? entity.DeletedAt.ToDbValue() : null,
    };
}

public UserPreferences ToDomainEntity(UserPreferencesDbModel dbModel, IClock clock)
{
    // DateTime → LocalDateTime（Audit ValueObject で内部変換）
    var createdAt = CreatedAt.FromDbValue(dbModel.CreatedAt);
    var updatedAt = UpdatedAt.TryFromDbValue(dbModel.UpdatedAt, out var up) ? up : UpdatedAt.Unset();
    var deletedAt = DeletedAt.TryFromDbValue(dbModel.DeletedAt, out var del) ? del : DeletedAt.Unset();
}
```

**4. Entity（イベント発行時）**

```csharp
// UpdatedAt が既に LocalDateTime を保持しているので再度変換不要
this.RaiseDomainEvent(new PreferencesUpdatedEvent(
    DomainEventId.New(),
    PreferenceChangeType.ModelUpdated,
    oldModel?.ToString() ?? "未設定",
    model.ToString(),
    _updatedAt.Value  // ← 直接 LocalDateTime を使用
));
```

---

## 📋 実装フェーズ

### Phase 1: Audit ValueObject の再設計（SharedKernel）

**ファイル**:
- `src/SharedKernel/ValueObjects/Audit/CreatedAt.cs`
- `src/SharedKernel/ValueObjects/Audit/UpdatedAt.cs`
- `src/SharedKernel/ValueObjects/Audit/DeletedAt.cs`

**タスク**:

| # | 内容 | 詳細 |
|---|---|---|
| 1.1 | 内部型を `DateTime` → `LocalDateTime` に変更 | `PrimitiveValueObject<DateTime>` → `PrimitiveValueObject<LocalDateTime>` |
| 1.2 | `From(LocalDateTime)` 実装 | Domain層での推奨メソッド |
| 1.3 | `FromDbValue(DateTime)` 追加 | Infrastructure層で DateTime を受け取る |
| 1.4 | `ToDbValue(): DateTime` 追加 | 型変換メソッド（DateTime 返却） |
| 1.5 | `TryFromDbValue(DateTime?, out T)` 追加 | NULL安全な DB→Domain 変換 |
| 1.6 | `TryFrom(LocalDateTime?, out T)` 更新 | 既存メソッド、仕様明確化 |
| 1.7 | `Value` プロパティ | `LocalDateTime` を返す |
| 1.8 | `GetValueComponents()` 更新 | `LocalDateTime` を返す |
| 1.9 | `Validate(LocalDateTime)` 更新 | LocalDateTime の検証ロジック |
| 1.10 | ユニットテスト更新 | DateTime/LocalDateTime 双方向変換テスト |

**仕様詳細**:

- `CreatedAt`: NULL 不許可（Unset 状態なし）
  - `TryFrom(LocalDateTime?, out CreatedAt)`: NULL は失敗
  - `TryFromDbValue(DateTime?, out CreatedAt)`: NULL は失敗

- `UpdatedAt`/`DeletedAt`: NULL 許容（Unset 状態）
  - `TryFrom(LocalDateTime?, out UpdatedAt)`: NULL → `Unset()` で成功
  - `TryFromDbValue(DateTime?, out UpdatedAt)`: NULL → `Unset()` で成功

---

### Phase 2: DbModel の修正（Infrastructure）

**ファイル**:
- `src/Contexts/Samples/CarPreferences.Infrastructure/DataAccess/Models/UserPreferencesDbModel.cs`

**タスク**:

| # | 内容 |
|---|---|
| 2.1 | `CreatedAt: LocalDateTime` → `CreatedAt: DateTime` |
| 2.2 | `UpdatedAt: LocalDateTime?` → `UpdatedAt: DateTime?` |
| 2.3 | `DeletedAt: LocalDateTime?` → `DeletedAt: DateTime?` |
| 2.4 | using 宣言から `LocalDateTime` を削除 |

---

### Phase 3: Mapper の修正（Infrastructure）

**ファイル**:
- `src/Contexts/Samples/CarPreferences.Infrastructure/Mappers/UserPreferencesMapper.cs`

**現状の問題**:

1. **ToDbModel() での逆方向型変換**
   ```csharp
   // ✗ 現在: DateTime → LocalDateTime（逆方向）
   CreatedAt = new LocalDateTime(entity.CreatedAt.Value),
   UpdatedAt = entity.UpdatedAt.HasUpdated ? new LocalDateTime(entity.UpdatedAt.Value!.Value) : null,
   ```

2. **ToDomainEntity() での TryFrom 混用**
   ```csharp
   // ✗ 現在: LocalDateTime を TryFrom に渡す（DB型が逆）
   if (!CreatedAt.TryFrom(dbModel.CreatedAt, out var createdAt)) { ... }
   ```

**タスク**:

| # | 内容 | 詳細 |
|---|---|---|
| 3.1 | `ToDbModel()` 更新 | Entity の LocalDateTime → DbModel の DateTime に変換 |
| 3.2 | Audit ValueObject 型変換メソッド使い分け | `FromDbValue()` vs `TryFromDbValue()` を明確化 |
| 3.3 | `ToDomainEntity()` 更新 | DbModel の DateTime から Audit ValueObject へ正方向変換 |
| 3.4 | ビジネス ValueObject の特殊処理 | `RespondentAt.TryFrom(clock)` は clock パラメータが必須 |

**変換メソッドの使い分け**:

**Audit ValueObject（CreatedAt/UpdatedAt/DeletedAt）**:
- `FromDbValue(DateTime)`: 単純な型変換（ビジネス検証なし）— ToDbModel 逆方向で使用
- `ToDbValue(): DateTime`: Audit ValueObject → DateTime に変換 — ToDbModel で使用
- `TryFromDbValue(DateTime?, out T)`: NULL 安全な型変換 — ToDomainEntity で使用

**ビジネス ValueObject（RespondentAt）**:
- `TryFrom(LocalDateTime?, IClock clock, out T)`: 
  - **clock パラメータ必須**（未来日チェック等のビジネス検証あり）
  - Mapper の `ToDomainEntity(dbModel, clock)` メソッド内で処理

**修正後のコード例**:

```csharp
/// <summary>
/// Domain Entity → DbModel（保存用、RowId → long 変換、LocalDateTime → DateTime 変換）
/// 【責務】Audit ValueObject の LocalDateTime を DateTime に変換して DB保存用に
/// </summary>
public UserPreferencesDbModel ToDbModel(UserPreferences entity)
{
    ArgumentNullException.ThrowIfNull(entity);

    return new UserPreferencesDbModel
    {
        RowId = entity.Id.Value,
        UserId = entity.UserId.Value ?? 0,
        
        // ✅ 修正: LocalDateTime → DateTime（ToDbValue で変換）
        CreatedAt = entity.CreatedAt.ToDbValue(),
        CreatedBy = 0,
        
        UpdatedAt = entity.UpdatedAt.HasUpdated ? entity.UpdatedAt.ToDbValue() : null,
        UpdatedBy = null,
        
        DeletedAt = entity.DeletedAt.IsDeleted ? entity.DeletedAt.ToDbValue() : null,
        DeletedBy = null,
        
        PreferredModel = entity.PreferredModel?.Value,
        PreferredBodyType = entity.PreferredBodyType?.ToString(),
        PrefersAutomatic = entity.PrefersAutomatic,
        BudgetFrom = entity.BudgetFrom?.Amount,
        BudgetTo = entity.BudgetTo?.Amount
    };
}

/// <summary>
/// DbModel → Domain Entity（読み取り用、long → RowId ValueObject 変換、DateTime → LocalDateTime 変換）
/// 【責務】DB値を Domain Entity に復元、型変換は Audit ValueObject で処理
/// </summary>
public UserPreferences ToDomainEntity(UserPreferencesDbModel dbModel, IClock clock)
{
    ArgumentNullException.ThrowIfNull(dbModel);
    ArgumentNullException.ThrowIfNull(clock);

    // ① ビジネス ValueObject 変換
    if (!RespondentPersonId.TryFrom(dbModel.UserId, out var userId))
    {
        throw new InvalidOperationException(
            $"Failed to convert UserId: {dbModel.UserId}");
    }

    // ② Audit ValueObject の変換（FromDbValue / TryFromDbValue を使用）
    // CreatedAt: 必ず値を持つ（NULL 不許可）
    if (!CreatedAt.TryFromDbValue(dbModel.CreatedAt, out var createdAt))
    {
        throw new InvalidOperationException(
            $"Failed to convert CreatedAt ValueObject: {dbModel.CreatedAt}");
    }

    // UpdatedAt: NULL → Unset に自動変換
    if (!UpdatedAt.TryFromDbValue(dbModel.UpdatedAt, out var updatedAt))
    {
        throw new InvalidOperationException(
            $"Failed to convert UpdatedAt ValueObject: {dbModel.UpdatedAt}");
    }

    // DeletedAt: NULL → Unset に自動変換
    if (!DeletedAt.TryFromDbValue(dbModel.DeletedAt, out var deletedAt))
    {
        throw new InvalidOperationException(
            $"Failed to convert DeletedAt ValueObject: {dbModel.DeletedAt}");
    }

    // ③ ビジネスロジック検証が必要な ValueObject（clock パラメータ必須）
    // RespondentAt: 未来日チェック等の検証が必要
    if (!RespondentAt.TryFrom(
        dbModel.CreatedAt,  // ← DB の CreatedAt（LocalDateTime？）を使用
        clock, 
        out var respondedAt))
    {
        throw new InvalidOperationException(
            $"Failed to convert CreatedAt to RespondentAt: {dbModel.CreatedAt}");
    }

    // ④ Reconstruct Factory メソッド呼び出し（DB値で復元）
    var entity = UserPreferences.Reconstruct(
        userId,
        respondedAt,
        createdAt,
        updatedAt,
        deletedAt,
        RowId.From(dbModel.RowId),
        dbModel.PreferredModel.HasValue
            ? CarModel.From(dbModel.PreferredModel.Value)
            : null,
        dbModel.PreferredBodyType != null
            ? BodyType.From(dbModel.PreferredBodyType)
            : null,
        dbModel.PrefersAutomatic,
        dbModel.BudgetFrom.HasValue
            ? Money.From(dbModel.BudgetFrom.Value)
            : null,
        dbModel.BudgetTo.HasValue
            ? Money.From(dbModel.BudgetTo.Value)
            : null);

    return entity;
}
```

**重要な注意**:

- **RespondentAt の TryFrom は clock パラメータが必須**
  - CreatedAt/UpdatedAt/DeletedAt の TryFromDbValue とは署名が異なる
  - `TryFromDbValue(DateTime?, out Audit)` vs `TryFrom(LocalDateTime?, IClock, out RespondentAt)`
  - ビジネス検証（未来日チェック）が必要な場合は clock パラメータを使用

- **DbModel は修正後 DateTime 型になるため**
  - 現在の `dbModel.CreatedAt`（LocalDateTime）から
  - 修正後の `dbModel.CreatedAt`（DateTime）に変更される
  - RespondentAt.TryFrom に渡す前に型変換不要（FromDbValue で処理）

---

### Phase 4: Entity の修正（Domain）

**ファイル**:
- `src/Contexts/Samples/CarPreferences.Domain/Entities/UserPreferences.cs`

**タスク**:

| # | 内容 | 詳細 |
|---|---|---|
| 4.1 | イベント発行の型変換削除 | `new LocalDateTime(...Value)` → `_updatedAt.Value` |
| 4.2 | Entity コンストラクタ確認 | 既に正しい（`CreatedAt.From(clock.JstNow)`) |
| 4.3 | Reconstruct 確認 | 既に正しい（`createdAt` ValueObject を受け取る） |

**変更例**:

```csharp
// Before
new LocalDateTime(_updatedAt.Value!.Value)

// After
_updatedAt.Value  // LocalDateTime を直接使用
```

---

### Phase 5: ユニットテストの修正（全層）

**ファイル**:
- `tests/SharedKernel.Tests/ValueObjects/Audit/CreatedAtTests.cs`
- `tests/SharedKernel.Tests/ValueObjects/Audit/UpdatedAtTests.cs`
- `tests/SharedKernel.Tests/ValueObjects/Audit/DeletedAtTests.cs`
- `tests/Contexts/Samples/CarPreferences.Infrastructure.Tests/Mappers/UserPreferencesMapperTests.cs`
- `tests/Contexts/Samples/CarPreferences.Domain.Tests/Entities/UserPreferencesTests.cs`

**タスク**:

| # | 内容 | テスト内容 |
|---|---|---|
| 5.1 | Audit ValueObject テスト: LocalDateTime 型確認 | 内部型が LocalDateTime であること |
| 5.2 | `From(LocalDateTime)` テスト | Domain層での推奨メソッド動作確認 |
| 5.3 | `FromDbValue(DateTime)` テスト | Infrastructure層での DateTime → LocalDateTime 変換 |
| 5.4 | `ToDbValue(): DateTime` テスト | Audit ValueObject → DateTime への逆変換 |
| 5.5 | `TryFromDbValue(DateTime?, out T)` テスト | NULL 安全な DB→Domain 変換、NULL → Unset 確認 |
| 5.6 | Mapper ToDbModel() テスト | Entity.LocalDateTime → DbModel.DateTime 変換確認 |
| 5.7 | Mapper ToDomainEntity() テスト | DbModel.DateTime → Entity.LocalDateTime 変換確認 |
| 5.8 | Mapper 型の往復変換テスト | Entity → DbModel → Entity のラウンドトリップ確認 |
| 5.9 | Entity イベント発行テスト | `_updatedAt.Value` が LocalDateTime を正しく返すことを確認 |
| 5.10 | Mapper RespondentAt 処理テスト | clock パラメータが正しく使用されることを確認 |

**Mapper テスト例**:

```csharp
[TestFixture]
public class UserPreferencesMapperTests
{
    private UserPreferencesMapper _mapper = null!;
    private IClock _clock = null!;

    [SetUp]
    public void Setup()
    {
        _mapper = new UserPreferencesMapper();
        _clock = new SystemClock();
    }

    [Test]
    public void ToDbModel_ShouldConvertAuditValuesFromLocalDateTimeToDateTime()
    {
        // Arrange
        var now = _clock.JstNow;
        var entity = new UserPreferences(
            userId: RespondentPersonId.From(1001),
            respondedAt: RespondentAt.From(now, _clock),
            clock: _clock);

        // Act
        var dbModel = _mapper.ToDbModel(entity);

        // Assert
        Assert.That(dbModel.CreatedAt, Is.TypeOf<DateTime>());
        Assert.That(dbModel.CreatedAt, Is.EqualTo(now.Value));
        Assert.That(dbModel.UpdatedAt, Is.TypeOf<DateTime?>());
        Assert.That(dbModel.UpdatedAt, Is.EqualTo(now.Value));
    }

    [Test]
    public void ToDomainEntity_ShouldConvertAuditValuesFromDateTimeToLocalDateTime()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var dbModel = new UserPreferencesDbModel
        {
            RowId = 1,
            UserId = 1001,
            CreatedAt = now,
            CreatedBy = 0,
            UpdatedAt = now,
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null,
            PrefersAutomatic = true
        };

        // Act
        var entity = _mapper.ToDomainEntity(dbModel, _clock);

        // Assert
        Assert.That(entity.CreatedAt.Value, Is.TypeOf<LocalDateTime>());
        Assert.That(entity.CreatedAt.Value.Value, Is.EqualTo(now));
        Assert.That(entity.UpdatedAt.Value, Is.TypeOf<LocalDateTime>());
        Assert.That(entity.DeletedAt.IsDeleted, Is.False);  // NULL → Unset
    }

    [Test]
    public void RoundTrip_EntityToDbModelAndBack_ShouldPreserveAuditValues()
    {
        // Arrange
        var entity1 = new UserPreferences(
            userId: RespondentPersonId.From(1001),
            respondedAt: RespondentAt.From(_clock.JstNow, _clock),
            clock: _clock);

        // Act
        var dbModel = _mapper.ToDbModel(entity1);
        var entity2 = _mapper.ToDomainEntity(dbModel, _clock);

        // Assert
        Assert.That(entity2.CreatedAt.Value, Is.EqualTo(entity1.CreatedAt.Value));
        Assert.That(entity2.UpdatedAt.Value, Is.EqualTo(entity1.UpdatedAt.Value));
    }
}
```

---

## 🔍 検証チェックリスト

実装後、以下の項目を確認してください：

### コード レビュー

- [ ] **Audit ValueObject**
  - [ ] 内部型が `LocalDateTime`
  - [ ] `From(LocalDateTime)` で生成可能
  - [ ] `FromDbValue(DateTime)` でDB値から生成可能
  - [ ] `ToDbValue(): DateTime` で DB値に変換可能
  - [ ] `TryFromDbValue(DateTime?, out T)` が正しく実装
  - [ ] `Validate()` が `LocalDateTime` に対応
  
- [ ] **DbModel**
  - [ ] すべての監査フィールドが `DateTime` プリミティブ型
  - [ ] `LocalDateTime` の import がない

- [ ] **Mapper**
  - [ ] `ToDbModel()`: `entity.CreatedAt.ToDbValue()` で LocalDateTime → DateTime に変換
  - [ ] `ToDbModel()`: UpdatedAt/DeletedAt も同様に`ToDbValue()` で変換
  - [ ] `ToDomainEntity()`: `CreatedAt.TryFromDbValue(dbModel.CreatedAt, out var ca)` で復元
  - [ ] `ToDomainEntity()`: UpdatedAt/DeletedAt も `TryFromDbValue()` で NULL 安全に変換
  - [ ] `ToDomainEntity()`: RespondentAt.TryFrom に clock パラメータを渡す
  - [ ] 不要な `new LocalDateTime()` 変換がない
  - [ ] Mapper テスト: ラウンドトリップ変換が正しいこと

- [ ] **Entity**
  - [ ] イベント発行時に `_updatedAt.Value` を直接使用（LocalDateTime）
  - [ ] 不要な `new LocalDateTime()` がない

### ビルド検証

```bash
dotnet build --no-incremental
# エラーなし ✓
```

### テスト実行

```bash
dotnet test
# すべてのテストが成功 ✓
```

### 型安全性検証

```bash
# LocalDateTime の型チェック
grep -rn "DateTime" src/SharedKernel/ValueObjects/Audit/*.cs
# PrimitiveValueObject<LocalDateTime> 以外でDateTime型を参照していないこと
```

---

## 📐 設計原則との適合性

| 原則 | 現状 | 修正後 |
|---|---|---|
| **LocalDateTime 大原則** | ❌ Audit ValueObject が DateTime を保持 | ✅ Domain/Appで LocalDateTime、Infrastructure で DateTime |
| **層間責務分離** | ❌ DbModel が LocalDateTime を保持 | ✅ DbModel は DateTime のみ |
| **型安全性** | ❌ イベント発行時に再度型変換 | ✅ Audit ValueObject から直接 LocalDateTime を取得 |
| **Mapper の責務** | ❌ 不正な型変換（DateTime → LocalDateTime） | ✅ 正方向変換のみ（LocalDateTime ↔ DateTime） |
| **クリーンアーキテクチャ** | ❌ Infrastructure が Domain層型に依存 | ✅ DbModel が共通型のみを使用 |

---

## 📝 CLAUDE.md への反映

修正完了後、以下ドキュメントを更新してください：

1. **docs/Assistance/Guides/DbModel_設計ルール.md**
   - 「監査カラムは DateTime のみ」を明記

2. **docs/Assistance/Guides/LocalDateTime_タイムゾーン_ガイド.md**
   - Audit ValueObject の LocalDateTime 保持を明記

3. **docs/Assistance/Guides/null厳格性設計ガイド.md**
   - FromDbValue / ToDbValue の仕様を追加（旧「監査ValueObject_null処理詳細設計.md」は本ガイドに統合）

---

## 🚨 注意

### Audit ValueObject メソッド署名の追加

- **`FromDbValue(DateTime): AuditValueObject`** — 新規追加
  - 単純な型変換、ビジネス検証なし
  - Mapper の ToDbModel 逆変換で使用

- **`ToDbValue(): DateTime`** — 新規追加
  - Audit ValueObject → DateTime 変換
  - Mapper の ToDbModel で使用

- **`TryFromDbValue(DateTime?, out T): bool`** — 新規追加
  - NULL 安全な変換（UpdatedAt/DeletedAt は NULL → Unset）
  - Mapper の ToDomainEntity で使用

- **`TryFrom(LocalDateTime?, out T): bool`** — 既存メソッド（仕様変更）
  - 用途：Domain層での型安全な生成
  - 変更前：DateTime? を期待 → 変更後：LocalDateTime? を期待

### Mapper での型変換フロー

```
Entity (LocalDateTime)
    ↓ ToDbValue()
DbModel (DateTime)
    ↓ TryFromDbValue()
Domain (LocalDateTime)
```

### ビジネス検証が必要な ValueObject の特殊処理

- **RespondentAt**: `TryFrom(LocalDateTime?, IClock clock, out T)`
  - clock パラメータが必須（未来日チェック）
  - Mapper の `ToDomainEntity(dbModel, clock)` で clock を注入
  - 他のビジネスロジック検証が必要な ValueObject も同様に処理

### ORM マッピング設定の確認

- DbModel が DateTime を使用するため、ORM グローバルマッピング設定を確認
- LocalDateTime ↔ datetime2 のグローバルマッピングが正しく機能すること
- RepoDb/Dapper の自動マッピングが DateTime に対応していること

---

## 例測 工数

| フェーズ | 内容 | 工数 | 備考 |
|---|---|---|---|
| 1 | Audit ValueObject 再設計 | 4-6h | FromDbValue/ToDbValue/TryFromDbValue メソッド追加 |
| 2 | DbModel 修正 | 1h | シンプルな型変更 |
| 3 | Mapper 修正 | 3-4h | **詳細**: ToDbModel/ToDomainEntity の逆方向変換削除、TryFromDbValue 導入、テスト例コード作成 |
| 4 | Entity 修正 | 1h | イベント発行時の型変換削除 |
| 5 | テスト修正・検証 | 4-5h | **詳細**: Mapper テスト強化（ラウンドトリップ、NULL処理、RespondentAt clock処理） |
| **合計** | | **13-20h** | Mapper の詳細実装が工数増加の要因 |

---

## 関連ドキュメント

- [CLAUDE.md - LocalDateTime 使用規則](../../CLAUDE.md#-localdatetime-使用規則)
- [CLEAN_ARCHITECTURE_GUIDELINES.md](../Guides/CLEAN_ARCHITECTURE_GUIDELINES.md)
- [AggregateId_設計ガイド.md](../Guides/AggregateId_設計ガイド.md)
- [DbModel_設計ルール.md](../Guides/DbModel_設計ルール.md)