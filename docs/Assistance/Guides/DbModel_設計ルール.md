# DbModel 設計ルール

Infrastructure の DbModel（データベース永続化モデル）の設計原則です。

---

## 📋 基本原則

### DbModel の役割

DbModel は **Entity ↔ Database のブリッジ** です。

```
Domain Entity
    ↓ (Mapper)
DbModel (技術的な永続化表現)
    ↓ (ORM)
SQL Server (datetime2, bigint, nvarchar など)
```

### 責務

- **Entity プロパティの正確な転写**: ビジネスロジックなし
- **LocalDateTime 使用**: JST タイムゾーン一貫性
- **監査カラムの8つ**: row_id, row_version, created_at/by, updated_at/by, deleted_at/by
- **ORM 用プロパティ**: テーブルカラムと 1:1 対応

---

## 💡 実装パターン

### 基本的な DbModel

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/DataAccess/Models/YourEntityDbModel.cs

using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;

/// <summary>
/// YourEntity のデータベース永続化モデル
/// 
/// 【責務】
/// - Entity → DbModel 変換時の型テンプレート
/// - SQL Server t_YourEntity テーブルとの 1:1 対応
/// - LocalDateTime 型でタイムゾーン統一
/// 
/// 【監査カラム（必須）】
/// - row_id: システム主キー（Sequence で自動採番）
/// - row_version: 楽観ロック用タイムスタンプ
/// - created_at/by: 作成日時/者
/// - updated_at/by: 更新日時/者（初期値 NULL）
/// - deleted_at/by: 削除日時/者（論理削除用）
/// </summary>
public class YourEntityDbModel
{
    // 【監査カラム】
    
    /// <summary>
    /// 主キー（row_id）
    /// Sequence で自動採番、テーブル全体で一意
    /// </summary>
    public long RowId { get; set; }

    /// <summary>
    /// 楽観ロック用タイムスタンプ
    /// SQL Server が自動更新（UPDATE 時に変更）
    /// </summary>
    public byte[] RowVersion { get; set; } = null!;

    /// <summary>
    /// 作成日時
    /// LocalDateTime（JST）で格納（datetime2(7) にマッピング）
    /// Repository が自動設定
    /// </summary>
    public LocalDateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者（従業員 row_id）
    /// FK → m_persons(row_id)
    /// Repository が自動設定（ICurrentUserService から取得）
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時（初期値 NULL）
    /// 初回作成時は NULL、UPDATE 時に設定
    /// </summary>
    public LocalDateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者（従業員 row_id）
    /// 初期値 NULL、UPDATE 時に設定
    /// </summary>
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 削除日時（論理削除用）
    /// NULL = 有効、datetime2 = 削除済み
    /// </summary>
    public LocalDateTime? DeletedAt { get; set; }

    /// <summary>
    /// 削除者（従業員 row_id）
    /// 初期値 NULL、論理削除時に設定
    /// </summary>
    public long? DeletedBy { get; set; }

    // 【ビジネスカラム】

    /// <summary>
    /// ビジネス ID（Entity の YourBusinessId に対応）
    /// ユーザー/クライアントが識別する番号
    /// </summary>
    public int YourBusinessId { get; set; }

    /// <summary>
    /// 名前
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 金額（nullable な場合）
    /// </summary>
    public decimal? Amount { get; set; }
}
```

### ValueObject マッピングの例

```csharp
// Entity: ValueObject を使用
public class YourEntity : Entity<long>
{
    public YourBusinessId YourBusinessId { get; }  // ValueObject
    public Money Amount { get; }                    // ValueObject
}

// DbModel: primitive 型を使用
public class YourEntityDbModel
{
    public int YourBusinessId { get; set; }       // int (ValueObject.Value を転写)
    public decimal? Amount { get; set; }          // decimal (Money.Amount を転写)
}

// Mapper: 型変換を実施
public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel>
{
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        return new YourEntityDbModel
        {
            RowId = entity.Id,
            YourBusinessId = entity.YourBusinessId.Value,  // ValueObject → int
            Amount = entity.Amount?.Amount,                // Money → decimal
            CreatedAt = entity.CreatedAt,                  // LocalDateTime そのまま
        };
    }

    public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
    {
        var businessId = YourBusinessId.From(dbModel.YourBusinessId);      // int → ValueObject
        var amount = dbModel.Amount.HasValue ? Money.From(dbModel.Amount.Value) : null;  // decimal → Money
        
        return new YourEntity(dbModel.RowId, businessId, amount, clock);
    }
}
```

### Aggregate DbModel

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/DataAccess/Models/YourAggregateDbModel.cs

using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;

public class YourAggregateDbModel
{
    // 【監査カラム】
    public long RowId { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    public LocalDateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public LocalDateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public LocalDateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    // 【ビジネスカラム】
    public int AggregateBusinessId { get; set; }

    // 【子要素】
    public List<YourChildEntityDbModel> Children { get; set; } = new();
}
```

---

## ✅ 実装チェックリスト

### DbModel 定義

- [ ] **監査カラム**: 8つすべてを含める
  - [ ] RowId (long, PK)
  - [ ] RowVersion (byte[])
  - [ ] CreatedAt (LocalDateTime)
  - [ ] CreatedBy (long)
  - [ ] UpdatedAt (LocalDateTime?)
  - [ ] UpdatedBy (long?)
  - [ ] DeletedAt (LocalDateTime?)
  - [ ] DeletedBy (long?)

- [ ] **ビジネスカラム**: Entity のプロパティに対応
  - [ ] nullable カラムは C# で nullable 型 (int?, decimal? など)
  - [ ] string は null! アノテーション
  - [ ] ValueObject は primitive 型に変換 (int, string など)

- [ ] **LocalDateTime 使用**: DateTime ではなく LocalDateTime

- [ ] **null 初期化**: null! アノテーション (NRT 対応)

---

## ❌ 避けるべきパターン

### パターン1: DbModel に DateTime

```csharp
// ✗ 禁止: DateTime を使用
public class YourEntityDbModel
{
    public DateTime CreatedAt { get; set; }  // ✗ DateTime（タイムゾーン情報なし）
}
```

**修正:**
```csharp
// ✓ LocalDateTime を使用
public class YourEntityDbModel
{
    public LocalDateTime CreatedAt { get; set; }  // ✓ LocalDateTime（JST 統一）
}
```

### パターン2: DbModel に ValueObject

```csharp
// ✗ 禁止: DbModel が ValueObject を参照
public class YourEntityDbModel
{
    public YourBusinessId YourBusinessId { get; set; }  // ✗ ValueObject
}
```

**修正:**
```csharp
// ✓ DbModel は primitive 型
public class YourEntityDbModel
{
    public int YourBusinessId { get; set; }  // ✓ primitive (int)
}

// Mapper が ValueObject に変換
public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
{
    var businessId = YourBusinessId.From(dbModel.YourBusinessId);  // int → ValueObject
    return new YourEntity(dbModel.RowId, businessId, clock);
}
```

### パターン3: DbModel にビジネスロジック

```csharp
// ✗ 禁止: DbModel がメソッドを持つ
public class YourEntityDbModel
{
    public decimal GetAmountWithTax()  // ✗ ビジネスロジック
    {
        return Amount * 1.1m;
    }
}
```

**修正:**
```csharp
// ✓ DbModel はプロパティのみ
public class YourEntityDbModel
{
    public decimal? Amount { get; set; }
}

// Entity がビジネスロジックを実装
public class YourEntity : Entity<long>
{
    public Money GetTotalWithTax() => Amount * 1.1m;
}
```

### パターン4: 監査カラムの缺落

```csharp
// ✗ 禁止: 監査カラムを一部省略
public class YourEntityDbModel
{
    public long RowId { get; set; }
    public string Name { get; set; }  // ✗ 監査カラムがない
}
```

**修正:**
```csharp
// ✓ すべての監査カラムを含める
public class YourEntityDbModel
{
    public long RowId { get; set; }
    public byte[] RowVersion { get; set; }
    public LocalDateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public LocalDateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public LocalDateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }
    
    public string Name { get; set; }
}
```

---

## 🔗 DbModel と SQL テーブルの対応

### CREATE TABLE テンプレート

```sql
CREATE TABLE [dbo].[t_YourEntity]
(
    -- 監査カラム（必須）
    [row_id] [bigint] NOT NULL PRIMARY KEY 
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
    [row_version] [timestamp] NOT NULL,
    [created_at] [datetime2](7) NOT NULL,
    [created_by] [bigint] NOT NULL,
    [updated_at] [datetime2](7) NULL,
    [updated_by] [bigint] NULL,
    [deleted_at] [datetime2](7) NULL,
    [deleted_by] [bigint] NULL,

    -- ビジネスカラム
    [your_business_id] [int] NOT NULL UNIQUE,
    [name] [nvarchar](100) NOT NULL,
    [amount] [decimal](10,2) NULL
)
GO
```

### DbModel プロパティとカラムの対応

| DbModel プロパティ | SQL カラム | 型 | Nullable |
|---|---|---|---|
| RowId | row_id | bigint | NOT NULL |
| RowVersion | row_version | timestamp | NOT NULL |
| CreatedAt | created_at | datetime2(7) | NOT NULL |
| CreatedBy | created_by | bigint | NOT NULL |
| UpdatedAt | updated_at | datetime2(7) | NULL |
| UpdatedBy | updated_by | bigint | NULL |
| DeletedAt | deleted_at | datetime2(7) | NULL |
| DeletedBy | deleted_by | bigint | NULL |
| YourBusinessId | your_business_id | int | NOT NULL |
| Name | name | nvarchar(100) | NOT NULL |
| Amount | amount | decimal(10,2) | NULL |

---

## 📁 ファイル構造

```
YourContext.Infrastructure/
├── DataAccess/
│   ├── Models/
│   │   ├── YourEntityDbModel.cs
│   │   ├── YourAggregateDbModel.cs
│   │   └── YourChildEntityDbModel.cs
│   ├── IYourEntityDataAccess.cs
│   └── YourEntityDataAccess.cs
├── Mappers/
│   └── YourEntityMapper.cs
└── Repositories/
    └── YourEntityRepository.cs
```

---

## 参考資料

- **Mapper_パターンガイド.md**: DbModel ↔ Entity 変換
- **Entity_設計ガイドライン.md**: Entity 設計
- **ORM_マッピング戦略.md**: LocalDateTime ↔ datetime2 マッピング
- **TABLE_DESIGN_STANDARDS.md**: SQL Server テーブル設計
- **Repository_パターンガイド.md**: DbModel 使用場所
