# LocalDateTime タイムゾーン ガイド

プロジェクト全体で統一された日時型（LocalDateTime）の使用ルールです。

---

## 📋 基本原則

### LocalDateTime を強制

SupportAdvance は **日本標準時（JST / UTC+9）** のみで動作します。すべての日時は LocalDateTime で統一します。

```
✓ LocalDateTime（推奨）: JST タイムゾーン情報を保持
✗ DateTime / DateTimeOffset: タイムゾーン曖昧で禁止
✗ DateTime.UtcNow / DateTime.Now: IClock 経由のみ
```

### タイムゾーン方針

| シナリオ | 型 | 取得方法 | 例 |
|---|---|---|---|
| **アプリケーション層** | LocalDateTime | IClock.JstNow | var now = _clock.JstNow; |
| **Entity** | LocalDateTime | コンストラクタで受け取り | new Entity(rowId, ..., clock) |
| **DbModel** | DateTime | ORM マッピング用プリミティブ型（JST として解釈、Mapper で変換） | dbModel.CreatedAt = DateTime |
| **SQL Server** | datetime2(7) | JST として解釈 | [created_at] datetime2(7) |
| **外部 API** | DateTime / ISO 8601 | 受け取り後に変換 | LocalDateTime.FromDateTime(...) |

---

## 💡 実装パターン

### 1. IClock インターフェース

```csharp
// src/Common/Clocks/IClock.cs

using NodaTime;

namespace SupportAdvance.Common.Clocks;

public interface IClock
{
    /// <summary>
    /// 現在の日本標準時（JST / UTC+9）
    /// </summary>
    LocalDateTime JstNow { get; }
}
```

### 2. SystemClock 実装（本番用）

```csharp
// src/Common/Clocks/SystemClock.cs

using NodaTime;

namespace SupportAdvance.Common.Clocks;

public class SystemClock : IClock
{
    private static readonly DateTimeZone JstTimeZone = DateTimeZoneProviders.Tzdb["Asia/Tokyo"];

    public LocalDateTime JstNow
    {
        get
        {
            var instant = SystemClock.Instance.GetCurrentInstant();
            var zonedDateTime = instant.InZone(JstTimeZone);
            return zonedDateTime.LocalDateTime;
        }
    }
}
```

### 3. テスト用の FixedClock

```csharp
// src/Common/Clocks/FixedClock.cs

using NodaTime;

namespace SupportAdvance.Common.Clocks;

public class FixedClock : IClock
{
    private readonly LocalDateTime _fixedTime;

    public LocalDateTime JstNow => _fixedTime;

    public FixedClock(LocalDateTime fixedTime)
    {
        _fixedTime = fixedTime;
    }
}
```

### 4. Entity での LocalDateTime 使用

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourEntity.cs

using NodaTime;
using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

public class YourEntity : Entity<long>
{
    private LocalDateTime _createdAt;
    private LocalDateTime? _updatedAt;

    public LocalDateTime CreatedAt => _createdAt;
    public LocalDateTime? UpdatedAt => _updatedAt;

    public YourEntity(long rowId, YourBusinessId businessId, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        Id = rowId;
        _createdAt = clock.JstNow;  // ✓ IClock から LocalDateTime を取得
    }

    public void Update(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        
        _updatedAt = clock.JstNow;  // ✓ LocalDateTime で更新
    }
}
```

### 5. DbModel での DateTime 使用（ORM マッピング用）

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/DataAccess/Models/YourEntityDbModel.cs

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;

public class YourEntityDbModel
{
    public long RowId { get; set; }
    
    // ✓ DateTime を使用（ORM マッピング用プリミティブ型、JST として解釈）
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }
}
```

### 6. Mapper での DateTime ↔ LocalDateTime 変換

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/Mappers/YourEntityMapper.cs

using NodaTime;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.Mappers;

public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel>
{
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        return new YourEntityDbModel
        {
            RowId = entity.Id,
            CreatedAt = entity.CreatedAt.ToDbValue(),      // ✓ LocalDateTime → DateTime（ValueObject の変換メソッド）
            UpdatedAt = entity.UpdatedAt.HasUpdated ? entity.UpdatedAt.ToDbValue() : null    // ✓ LocalDateTime? → DateTime?
        };
    }

    public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
    {
        if (!CreatedAt.TryFromDbValue(dbModel.CreatedAt, out var createdAt))
            throw new InvalidOperationException($"Failed to convert CreatedAt: {dbModel.CreatedAt}");

        return new YourEntity(
            dbModel.RowId,
            createdAt,  // ✓ DateTime → LocalDateTime（ValueObject の変換メソッド）
            clock
        );
    }
}
```

### 7. 外部システムからの DateTime 変換

```csharp
// 外部 API から DateTime を受け取る場合

public async Task ProcessExternalData(string externalTimestamp)
{
    // 外部システムは UTC で送信する場合
    var utcDateTime = DateTime.Parse(externalTimestamp, null, System.Globalization.DateTimeStyles.AssumeUtc);
    
    // 1. UTC DateTime を Instant に変換
    var instant = Instant.FromDateTimeUtc(utcDateTime);
    
    // 2. Instant を JST に変換
    var jstZone = DateTimeZoneProviders.Tzdb["Asia/Tokyo"];
    var jstDateTime = instant.InZone(jstZone).LocalDateTime;
    
    // 3. Entity/DbModel で使用
    var entity = new YourEntity(rowId, businessId, jstDateTime, clock);
}
```

---

## ✅ 実装チェックリスト

### 全層共通

- [ ] **LocalDateTime のみ使用**: DateTime, DateTimeOffset は禁止
- [ ] **IClock 注入**: すべての Entity/UseCase/Repository
- [ ] **clock.JstNow**: 現在時刻の取得方法
- [ ] **LocalDateTime?**: nullable 日時（updated_at, deleted_at など）

### Entity 層

- [ ] **LocalDateTime 型**: CreatedAt, UpdatedAt など
- [ ] **IClock コンストラクタ**: Entity が clock を引数に受け取る
- [ ] **ドメインイベント**: イベントにも LocalDateTime を含める

### DbModel 層

- [ ] **DateTime 型**: すべての日時カラム（プリミティブ型、ORM マッピング用）
- [ ] **LocalDateTime は禁止**: Mapper の責務を侵害

### Mapper 層

- [ ] **型変換あり**: LocalDateTime ↔ DateTime の明示的な変換を実装
  - Entity → DbModel: LocalDateTime.ToDbValue() で DateTime に変換
  - DbModel → Entity: CreatedAt.TryFromDbValue() で DateTime から LocalDateTime に変換
- [ ] **ValueObject メソッド使用**: 監査ValueObjects (CreatedAt/UpdatedAt/DeletedAt) の ToDbValue/TryFromDbValue を使用

### ORM レベル

- [ ] **DapperTypeHandlerRegistration**: LocalDateTime → DbType.DateTime2
- [ ] **RepoDbTypeMapperRegistration**: LocalDateTime → DbType.DateTime2
- [ ] **DependencyInjection**: 起動時に登録

---

## ❌ 避けるべきパターン

### パターン1: DateTime.Now / DateTime.UtcNow の直接使用

```csharp
// ✗ 禁止: IClock を経由しない
public class YourUseCase
{
    public async Task Execute(YourRequest request)
    {
        var now = DateTime.Now;          // ✗ DateTime.Now
        var entity = new YourEntity(rowId, businessId, now, clock);
    }
}
```

**修正:**
```csharp
// ✓ IClock 経由で取得
public class YourUseCase
{
    private readonly IClock _clock;

    public YourUseCase(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task Execute(YourRequest request)
    {
        var now = _clock.JstNow;  // ✓ IClock から LocalDateTime
        var entity = new YourEntity(rowId, businessId, now, _clock);
    }
}
```

### パターン2: Entity が DateTime を使用

```csharp
// ✗ 禁止: Entity に DateTime
public class YourEntity : Entity<long>
{
    public DateTime CreatedAt { get; }  // ✗ DateTime（タイムゾーン曖昧）
}
```

**修正:**
```csharp
// ✓ Entity は LocalDateTime
public class YourEntity : Entity<long>
{
    public LocalDateTime CreatedAt { get; }  // ✓ LocalDateTime（JST 明確）
}
```

### パターN 3: Clock の実装内外での DateTime 混在

```csharp
// ✗ 禁止: Clock が DateTime と LocalDateTime を混在
public class SystemClock : IClock
{
    public LocalDateTime JstNow
    {
        get
        {
            var dateTime = DateTime.Now;  // ✗ DateTimeとLocalDateTimeを混在
            return LocalDateTime.FromDateTime(dateTime);
        }
    }
}
```

**修正:**
```csharp
// ✓ Clock の実装内のみ DateTime 使用可、外部は LocalDateTime
public class SystemClock : IClock
{
    public LocalDateTime JstNow
    {
        get
        {
            var instant = SystemClock.Instance.GetCurrentInstant();
            var zonedDateTime = instant.InZone(jstZone);
            return zonedDateTime.LocalDateTime;  // ✓ LocalDateTime で返す
        }
    }
}
```

### パターン4: DbModel に LocalDateTime を使用

```csharp
// ✗ 禁止: DbModel が LocalDateTime（型変換の責務混在）
public class YourEntityDbModel
{
    public LocalDateTime CreatedAt { get; set; }  // ✗ LocalDateTime（Mapper 責務を侵害）
}
```

**修正:**
```csharp
// ✓ DbModel は DateTime（ORM マッピング用プリミティブ型）
public class YourEntityDbModel
{
    public DateTime CreatedAt { get; set; }  // ✓ DateTime（JST として解釈、Mapper で変換）
}
```

---

## 🔧 トラブルシューティング

### 問題1: "Cannot convert from 'DateTime' to 'LocalDateTime'"

**原因**: Entity/DbModel が DateTime を使用

**修正**: LocalDateTime に変更

```csharp
// ✗ 誤り
public class YourEntity
{
    public DateTime CreatedAt { get; }  // DateTime
}

// ✓ 正解
public class YourEntity
{
    public LocalDateTime CreatedAt { get; }  // LocalDateTime
}
```

### 問題2: "NodaTime への参照がない"

**原因**: NodaTime パッケージが未インストール

**修正**: パッケージをインストール

```bash
dotnet add package NodaTime
```

### 問題3: テストで固定時刻を使いたい

**修正**: FixedClock を使用

```csharp
// テストで固定時刻を使用
var fixedTime = new LocalDateTime(2024, 1, 1, 12, 0, 0);
var clock = new FixedClock(fixedTime);

var entity = new YourEntity(rowId, businessId, clock);
```

---

## 📚 NodaTime リファレンス

| クラス | 用途 | タイムゾーン |
|---|---|---|
| **LocalDateTime** | 日時（タイムゾーンなし） | なし（JST と解釈） |
| **Instant** | UTC 時刻 | UTC |
| **ZonedDateTime** | タイムゾーン付き日時 | 指定可能 |
| **DateTimeZone** | タイムゾーン | TZdb データベース |

### よく使う操作

```csharp
using NodaTime;

// LocalDateTime 作成
var ldt = new LocalDateTime(2024, 1, 15, 14, 30, 0);

// 現在の JST (IClock 経由)
var now = _clock.JstNow;

// UTC DateTime から LocalDateTime に変換
var utcDt = DateTime.UtcNow;
var instant = Instant.FromDateTimeUtc(utcDt);
var jstZone = DateTimeZoneProviders.Tzdb["Asia/Tokyo"];
var ldt2 = instant.InZone(jstZone).LocalDateTime;

// LocalDateTime → DateTime
var ldt3 = new LocalDateTime(2024, 1, 15, 14, 30, 0);
var dt = ldt3.ToDateTimeUnspecified();
```

---

## 📁 ファイル構造

```
Common/
├── Clocks/
│   ├── IClock.cs
│   ├── SystemClock.cs
│   ├── FixedClock.cs
│   └── OffsetClock.cs (オプション: テスト用)
└── Configuration/
    └── ClockSettings.cs
```

---

## 参考資料

- **ORM_マッピング戦略.md**: LocalDateTime と datetime2 のマッピング
- **Entity_設計ガイドライン.md**: Entity の LocalDateTime 使用
- **DbModel_設計ルール.md**: DbModel の LocalDateTime 使用
- **TABLE_DESIGN_STANDARDS.md**: SQL Server の datetime2(7)
- **NodaTime**: https://nodatime.org/ (公式ドキュメント)
