# Mapper パターンガイド

Domain Entity と Infrastructure DbModel 間の純粋なマッピング実装ガイドです。

---

## 📋 基本原則

### 責務の分離

Mapper の責務は **型変換のみ**です。ビジネスコンテキストや認証情報の管理は Repository の責務です。

| 層 | 責務 | 例 |
|---|---|---|
| **Mapper** | 純粋な型変換 | Entity.Id → DbModel.RowId |
| **Repository** | ビジネスコンテキスト管理 | createdBy を ICurrentUserService から取得 |

### マッピング方向

```
Domain Entity ←→ DbModel（SQL Server）
    ↑                ↓
  ビジネス        技術的な永続化形式
   ロジック
```

---

## 💡 実装パターン

### インターフェース定義

```csharp
// src/Infrastructure/Mappers/IEntityMapper.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Infrastructure.Mappers;

/// <summary>
/// Entity ↔ DbModel マッパー（汎用インターフェース）
/// 
/// 【型パラメータ】
/// - TEntity: Entity<TId>（ID型をサポート）
/// - TDbModel: データベースモデル
/// - TId: Entity の ID 型（ValueObject など、通常は RowId）
/// </summary>
public interface IEntityMapper<TEntity, TDbModel, TId>
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull
{
    /// <summary>
    /// Domain Entity → DbModel（保存用）
    /// </summary>
    TDbModel ToDbModel(TEntity entity);

    /// <summary>
    /// DbModel → Domain Entity（読み取り用）
    /// </summary>
    TEntity ToDomainEntity(TDbModel dbModel, IClock clock);
}
```

### Mapper 実装例

#### シンプルな Entity マッピング

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/Mappers/YourEntityMapper.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.Mappers;

public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel, RowId>
{
    /// <summary>
    /// Domain Entity → DbModel（保存用）
    /// 【責務】RowId ValueObject → long 変換、その他は直接マッピング
    /// 【注意】監査情報（createdBy など）は Repository で設定される
    /// </summary>
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new YourEntityDbModel
        {
            // row_id は Entity.Id（RowId ValueObject）から long に変換
            RowId = entity.Id.Value,  // RowId → long 変換

            // ビジネスカラム：Entity から直接マッピング
            YourBusinessId = entity.YourBusinessId.Value,
            Name = entity.Name,
            Amount = entity.Amount?.Amount,  // ValueObject → primitive

            // 監査情報：Repository で設定されるため、ここでは初期値のみ
            CreatedAt = entity.CreatedAt,
            CreatedBy = 0,  // Repository が上書き
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = null,  // Repository が上書き
            DeletedAt = null,
            DeletedBy = null
        };
    }

    /// <summary>
    /// DbModel → Domain Entity（読み取り用）
    /// 【責務】long → RowId ValueObject 変換、その他は直接マッピング
    /// 【注意】DbModel には LocalDateTime が格納されている
    /// </summary>
    public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        // 1. DbModel の値から ValueObject を構築
        var yourBusinessId = YourBusinessId.From(dbModel.YourBusinessId);
        var amount = dbModel.Amount.HasValue ? Money.From(dbModel.Amount.Value) : null;
        var rowId = RowId.From(dbModel.RowId);  // long → RowId ValueObject 変換

        // 2. Entity を構築（rowId を RowId ValueObject として渡す）
        var entity = new YourEntity(
            yourBusinessId,
            dbModel.Name,
            amount,
            clock,
            rowId  // RowId ValueObject
        );

        // 3. Entity は構築完了
        return entity;
    }
}
```

#### 複雑な Entity マッピング

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/Mappers/YourAggregateMapper.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.Mappers;

public class YourAggregateMapper : IEntityMapper<YourAggregate, YourAggregateDbModel>
{
    private readonly YourChildEntityMapper _childMapper;

    public YourAggregateMapper(YourChildEntityMapper childMapper)
    {
        _childMapper = childMapper ?? throw new ArgumentNullException(nameof(childMapper));
    }

    public YourAggregateDbModel ToDbModel(YourAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        // 親の DbModel を構築
        var dbModel = new YourAggregateDbModel
        {
            RowId = aggregate.Id,
            AggregateBusinessId = aggregate.AggregateId.Value,
            CreatedAt = aggregate.CreatedAt,
            CreatedBy = 0,  // Repository で設定
            UpdatedAt = aggregate.UpdatedAt,
            UpdatedBy = null,  // Repository で設定
            DeletedAt = null,
            DeletedBy = null
        };

        // 子要素も DbModel に変換
        dbModel.Children = aggregate.Children
            .Select(_childMapper.ToDbModel)
            .ToList();

        return dbModel;
    }

    public YourAggregate ToDomainEntity(YourAggregateDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        // 親の Entity を構築
        var aggregateId = YourAggregateBusinessId.From(dbModel.AggregateBusinessId);
        var aggregate = new YourAggregate(dbModel.RowId, aggregateId, clock);

        // 子要素も Entity に変換して追加
        foreach (var childDbModel in dbModel.Children ?? Enumerable.Empty<YourChildEntityDbModel>())
        {
            var child = _childMapper.ToDomainEntity(childDbModel, clock);
            aggregate.AddChild(child);
        }

        return aggregate;
    }
}
```

---

## ✅ 実装チェックリスト

### ToDbModel（Entity → DbModel）

- [ ] **Entity.Id** → **DbModel.RowId**
- [ ] **ValueObject** → **primitive 型** に変換
- [ ] **監査情報** は初期値のみ（Repository が上書き）
- [ ] **null チェック**: ArgumentNullException
- [ ] **ビジネスID** は ValueObject から .Value で抽出

### ToDomainEntity（DbModel → Entity）

- [ ] **DbModel.RowId** → **Entity.Id**
- [ ] **primitive 型** → **ValueObject** に変換
- [ ] **LocalDateTime** はそのまま使用（変換不要）
- [ ] **null チェック**: ArgumentNullException
- [ ] **IClock** を引数に受け取る（コンストラクタに渡す）

---

## ❌ 避けるべきパターン

### パターン1: Mapper が認証情報を管理

```csharp
// ✗ 禁止: Mapper が ICurrentUserService に依存
public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel>
{
    private readonly ICurrentUserService _currentUser;

    public YourEntityMapper(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        return new YourEntityDbModel
        {
            RowId = entity.Id,
            CreatedBy = _currentUser.EmployeeRowId,  // ✗ Mapper が認証を管理
        };
    }
}
```

**修正:**
```csharp
// ✓ Mapper は純粋な型変換のみ
public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel>
{
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        return new YourEntityDbModel
        {
            RowId = entity.Id,
            CreatedBy = 0,  // Repository で設定される
        };
    }
}
```

### パターン2: Mapper がビジネスロジックを実行

```csharp
// ✗ 禁止: Mapper がビジネス変換を実行
public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel>
{
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        var dbModel = new YourEntityDbModel();
        
        // ✗ ビジネスロジック：税金計算など
        dbModel.TotalWithTax = entity.Amount * 1.1m;
        
        return dbModel;
    }
}
```

**修正:**
```csharp
// ✓ Entity でビジネスロジックを実行、Mapper は結果を転送
public class YourEntity : Entity<long>
{
    public Money GetTotalWithTax() => Amount * 1.1m;  // Entity がビジネスロジック
}

public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel>
{
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        return new YourEntityDbModel
        {
            RowId = entity.Id,
            TotalWithTax = entity.GetTotalWithTax().Amount,  // 結果を転送
        };
    }
}
```

### パターン3: DbModel が DateTime を使用

```csharp
// ✗ 禁止: DbModel に DateTime を使用
public class YourEntityDbModel
{
    public DateTime CreatedAt { get; set; }  // ✗ DateTime
}

public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel>
{
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        return new YourEntityDbModel
        {
            CreatedAt = entity.CreatedAt.ToDateTime(TimeOnly.MinValue),  // ✗ 変換が必要
        };
    }
}
```

**修正:**
```csharp
// ✓ DbModel は LocalDateTime を使用
public class YourEntityDbModel
{
    public LocalDateTime CreatedAt { get; set; }  // ✓ LocalDateTime
}

public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel>
{
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        return new YourEntityDbModel
        {
            CreatedAt = entity.CreatedAt,  // ✓ 直接マッピング
        };
    }
}
```

---

## 🔗 LocalDateTime マッピング

DbModel と Entity の両方が **LocalDateTime** を使用します。

```csharp
// Entity: LocalDateTime
public class YourEntity : Entity<long>
{
    public LocalDateTime CreatedAt { get; }
    public LocalDateTime? UpdatedAt { get; }
}

// DbModel: LocalDateTime
public class YourEntityDbModel
{
    public LocalDateTime CreatedAt { get; set; }
    public LocalDateTime? UpdatedAt { get; set; }
}

// Mapper: 直接マッピング（変換不要）
public YourEntityDbModel ToDbModel(YourEntity entity)
{
    return new YourEntityDbModel
    {
        CreatedAt = entity.CreatedAt,  // ✓ LocalDateTime → LocalDateTime
        UpdatedAt = entity.UpdatedAt   // ✓ LocalDateTime → LocalDateTime
    };
}
```

---

## 📁 ファイル構造

```
YourContext.Infrastructure/
├── Mappers/
│   ├── YourEntityMapper.cs
│   ├── YourChildEntityMapper.cs
│   └── YourAggregateMapper.cs
├── DataAccess/
│   ├── Models/
│   │   ├── YourEntityDbModel.cs
│   │   └── YourAggregateDbModel.cs
```

---

## 参考資料

- **Repository_パターンガイド.md**: Mapper の使用方法
- **Entity_設計ガイドライン.md**: Entity と ValueObject
- **DbModel_設計ルール.md**: DbModel の設計原則
- **src/Contexts/Samples/CarPreferences.Infrastructure/Mappers**: 実装例
