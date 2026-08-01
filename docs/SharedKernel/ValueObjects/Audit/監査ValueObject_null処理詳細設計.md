# 監査 ValueObject - TryFrom での null ハンドリング設計

**バージョン:** 1.0  
**作成日:** 2025年07月  

> **🚀 クイックガイド**: 実装時の簡潔なガイドについては、[監査ValueObject_null処理戦略.md](../../../docs/Assistance/Guides/監査ValueObject_null処理戦略.md) を参照してください。

---

## 1. 概要

`CreatedAt`、`UpdatedAt`、`DeletedAt` の三つの監査 ValueObject は、永続化層との層間で null 値を適切にハンドルする必要があります。

- **ドメイン層**: null を一切許容しない（完全に null-free）
- **永続化層**: CreatedAt は NOT NULL、UpdatedAt/DeletedAt は NULL OK
- **層間の境界 (TryFrom)**: null を「Unset 状態」に変換して Domain に渡す

このドキュメントは、TryFrom メソッドが null 値をどのように処理するか、なぜこのような設計になっているかを説明します。

---

## 2. 永続化層 (Persistence Layer) での null の扱い

### 2.1 データベーススキーマ

```sql
-- CreatedAt: NOT NULL（作成日時は必須）
CREATE TABLE Entities
(
    Id BIGINT PRIMARY KEY,
    CreatedAtUtc DATETIME NOT NULL,
    
    UpdatedAtUtc DATETIME NULL,  -- NULL 許容（未更新時）
    DeletedAtUtc DATETIME NULL,  -- NULL 許容（未削除時）
);
```

### 2.2 永続化層での null の意味

| ValueObject | DB null の意味 | Unset 状態の意味 |
|---|---|---|
| **CreatedAt** | ❌ あり得ない（NOT NULL） | なし |
| **UpdatedAt** | ✓ 「未更新」を表現 | `Unset()` で Domain に表現 |
| **DeletedAt** | ✓ 「未削除」を表現 | `Unset()` で Domain に表現 |

---

## 3. TryFrom での null 変換フロー

### 3.1 CreatedAt の TryFrom

```csharp
public static bool TryFrom(LocalDateTime? input, out CreatedAt result)
{
    if (!input.HasValue)
    {
        // ← DB から null が返された = ❌ エラー（NOT NULL 違反）
        result = null!;
        return false;  // 生成失敗
    }

    try
    {
        result = From(input.Value);
        return true;
    }
    catch (ArgumentException)
    {
        // MinValue / MaxValue による検証失敗
        result = null!;
        return false;
    }
}
```

**CreatedAt の null は層境界でエラー:**
- DB スキーマが NOT NULL だから
- 永続化層で null が返ってきた = データベース整合性エラー

### 3.2 UpdatedAt の TryFrom

```csharp
public static bool TryFrom(LocalDateTime? input, out UpdatedAt result)
{
    if (input == null || !input.HasValue)
    {
        // ← DB から null が返された = ✓ 「未更新」状態
        result = Unset();  // ← Domain では Unset() で表現
        return true;       // ← 成功（変換成功）
    }

    try
    {
        result = From(input.Value);
        return true;
    }
    catch (ArgumentException)
    {
        result = null!;
        return false;
    }
}
```

**UpdatedAt の null は Unset に変換:**
- DB の NULL = 「未更新」の意味
- Domain では `UpdatedAt.Unset()` で表現（null-free）
- TryFrom で自動変換（Layer Boundary での処理）

### 3.3 DeletedAt の TryFrom

```csharp
public static bool TryFrom(LocalDateTime? input, out DeletedAt result)
{
    if (input == null || !input.HasValue)
    {
        // ← DB から null が返された = ✓ 「未削除」状態
        result = Unset();  // ← Domain では Unset() で表現
        return true;       // ← 成功（変換成功）
    }

    try
    {
        result = From(input.Value);
        return true;
    }
    catch (ArgumentException)
    {
        result = null!;
        return false;
    }
}
```

**DeletedAt の null は Unset に変換:**
- DB の NULL = 「未削除」の意味
- Domain では `DeletedAt.Unset()` で表現（null-free）
- TryFrom で自動変換（Layer Boundary での処理）

---

## 4. null-free ドメイン層の実装

### 4.1 Entity コンストラクタでの初期化

```csharp
public class Entity
{
    private readonly IClock _clock;
    
    // 監査情報は常に初期化
    public CreatedAt CreatedAt { get; }
    public UpdatedAt UpdatedAt { get; private set; }
    public DeletedAt DeletedAt { get; private set; }
    
    public Entity(IClock clock)
    {
        _clock = clock;
        
        // ✅ Domain では null を一切使用しない
        CreatedAt = CreatedAt.From(_clock.JstNow);  // 必須（値あり）
        UpdatedAt = UpdatedAt.Unset();              // 未更新（Unset）
        DeletedAt = DeletedAt.Unset();              // 未削除（Unset）
    }
    
    public void Update(string name)
    {
        Name = name;
        // ✅ null ではなく Unset() から From() に遷移
        UpdatedAt = UpdatedAt.From(_clock.JstNow);
    }
    
    public void SoftDelete()
    {
        if (!DeletedAt.IsDeleted)
        {
            // ✅ null ではなく Unset() から From() に遷移
            DeletedAt = DeletedAt.From(_clock.JstNow);
        }
    }
}
```

### 4.2 Domain ロジック内での null チェック不要

```csharp
// ❌ 間違い（null チェック）
if (entity.UpdatedAt != null)  // ← Domain では不要
{
    var dt = entity.UpdatedAt.Value;
}

// ✅ 正しい（IsSet チェック）
if (entity.UpdatedAt.IsSet)  // ← または entity.UpdatedAt.HasUpdated
{
    var dt = entity.UpdatedAt.Value;
}
```

---

## 5. 永続化層での復元フロー

### 5.1 Database → ValueObject への変換

```csharp
public class EntityRepository : IEntityRepository
{
    public async Task<Entity> GetByIdAsync(long id)
    {
        var dbEntity = await _context.Entities.FindAsync(id);
        
        // DB から読み込んだ値（null の可能性あり）
        var createdAtDb = dbEntity.CreatedAtUtc;       // NOT NULL
        var updatedAtDb = dbEntity.UpdatedAtUtc;       // NULL OK
        var deletedAtDb = dbEntity.DeletedAtUtc;       // NULL OK
        
        // ✅ TryFrom で null を Unset に自動変換
        if (!CreatedAt.TryFrom(new LocalDateTime(createdAtDb), out var createdAt))
            throw new InvalidOperationException("Invalid CreatedAt from DB");
        
        var hasUpdated = UpdatedAt.TryFrom(
            updatedAtDb.HasValue ? new LocalDateTime(updatedAtDb.Value) : null,
            out var updatedAt
        );
        if (!hasUpdated) updatedAt = UpdatedAt.Unset();
        
        var hasDeleted = DeletedAt.TryFrom(
            deletedAtDb.HasValue ? new LocalDateTime(deletedAtDb.Value) : null,
            out var deletedAt
        );
        if (!hasDeleted) deletedAt = DeletedAt.Unset();
        
        // ✅ Domain では完全に null-free
        entity = new Entity(id, createdAt, updatedAt, deletedAt);
        return entity;
    }
}
```

### 5.2 ValueObject → Database への保存

```csharp
public async Task SaveAsync(Entity entity)
{
    var dbEntity = new DbEntity
    {
        Id = entity.Id,
        
        // CreatedAt: 常に値がある
        CreatedAtUtc = entity.CreatedAt.Value,
        
        // UpdatedAt: IsSet に基づいて null / 値 を決定
        UpdatedAtUtc = entity.UpdatedAt.IsSet ? entity.UpdatedAt.Value : (DateTime?)null,
        
        // DeletedAt: IsDeleted に基づいて null / 値 を決定
        DeletedAtUtc = entity.DeletedAt.IsDeleted ? entity.DeletedAt.Value : (DateTime?)null,
    };
    
    await _context.SaveChangesAsync();
}
```

---

## 6. TryFrom が「成功/失敗」を判定する基準

### 6.1 成功パターン

```csharp
// CreatedAt: 有効な DateTime が来た
var success1 = CreatedAt.TryFrom(new LocalDateTime(new DateTime(2025, 1, 15)), out var ca1);
// → true

// UpdatedAt: null が来た → Unset に変換
var success2 = UpdatedAt.TryFrom(null, out var ua1);
// → true （null → Unset に自動変換）

// UpdatedAt: 有効な DateTime が来た
var success3 = UpdatedAt.TryFrom(new LocalDateTime(new DateTime(2025, 1, 15)), out var ua2);
// → true

// DeletedAt: null が来た → Unset に変換
var success4 = DeletedAt.TryFrom(null, out var da1);
// → true （null → Unset に自動変換）
```

### 6.2 失敗パターン

```csharp
// CreatedAt: null が来た → エラー（NOT NULL 違反）
var success5 = CreatedAt.TryFrom(null, out var ca2);
// → false （null は許容しない）

// CreatedAt: DateTime.MinValue
var success6 = CreatedAt.TryFrom(new LocalDateTime(DateTime.MinValue), out var ca3);
// → false （検証失敗）

// UpdatedAt: DateTime.MaxValue
var success7 = UpdatedAt.TryFrom(new LocalDateTime(DateTime.MaxValue), out var ua3);
// → false （検証失敗）
```

---

## 7. ドメイン層での null チェック廃止

### 7.1 リファクタリング例

**Before（null-unsafe）:**
```csharp
public bool IsRecentlyUpdated(DateTime threshold)
{
    // ❌ UpdatedAt が null かもしれない
    if (entity.UpdatedAt == null)
        return false;
    
    return entity.UpdatedAt.Value >= threshold;
}
```

**After（null-free）:**
```csharp
public bool IsRecentlyUpdated(DateTime threshold)
{
    // ✅ IsSet で未更新状態をチェック
    if (!entity.UpdatedAt.IsSet)
        return false;
    
    return entity.UpdatedAt.Value >= threshold;
}

// または

public bool IsRecentlyUpdated(DateTime threshold)
{
    // ✅ HasUpdated で一行で表現
    return entity.UpdatedAt.HasUpdated && entity.UpdatedAt.Value >= threshold;
}
```

---

## 8. 実装チェックリスト

### 永続化層での null ハンドリング

- [ ] TryFrom で DB の null を Unset に自動変換している
- [ ] CreatedAt は null チェック→エラー投げ
- [ ] UpdatedAt/DeletedAt は null チェック→Unset() 返却
- [ ] Domain に渡す時点で null が混入していない

### ドメイン層での null-free 確保

- [ ] Entity 初期化時に Unset() で明示的に初期化
- [ ] null チェック（`entity.XXX != null`）が存在しない
- [ ] IsSet / HasUpdated / IsDeleted でのチェックのみ
- [ ] 新規作成・更新・削除時に null を生成していない

### テストでの検証

- [ ] TryFrom(null) → Unset() の動作確認
- [ ] TryFrom(値) → From() の動作確認
- [ ] Unset() 状態での IsSet / IsDeleted の値確認
- [ ] DB 復元時に null が正しく Unset に変換されること

---

## 9. まとめ

| 層 | null の扱い | 実装パターン |
|---|---|---|
| **Persistence (DB)** | CreatedAt: NOT NULL / UpdatedAt,DeletedAt: NULL OK | スキーマで明示 |
| **TryFrom (Layer Boundary)** | null → Unset に自動変換 | UpdatedAt/DeletedAt が null なら Unset() |
| **Domain** | 完全に null-free | Unset() / IsSet / HasUpdated / IsDeleted を使用 |

**原則:**
- ドメイン層は null を見ない
- TryFrom が層の境界で null を Unset に変換する責任を持つ
- Domain logic は IsSet や HasUpdated で状態判定を行う
