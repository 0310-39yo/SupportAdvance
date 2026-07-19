# 監査 ValueObject 比較表

**バージョン:** 1.0  
**作成日:** 2025年  

---

## 1. 基本特性比較

| 特性 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **役割** | 作成日時 | 最終更新日時 | 論理削除日時 |
| **型** | `PrimitiveValueObject<DateTime>` | `PrimitiveValueObject<DateTime?>` | `PrimitiveValueObject<DateTime?>` |
| **IsSet管理** | 常にtrue | あり（true=更新済み） | あり（true=削除済み） |
| **Unset状態** | なし | `Unset()` で表現 | `Unset()` で表現 |
| **null許容** | ✗（必須） | ✓（Unset状態） | ✓（Unset状態） |
| **クラス定義** | `sealed class` | `sealed class` | `sealed class` |
| **基底クラス** | PrimitiveValueObject<DateTime> | PrimitiveValueObject<DateTime?> | PrimitiveValueObject<DateTime?> |

---

## 2. 生ライフサイクル比較

| 段階 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **初期化** | Entity生成時に一度だけ設定 | Entity生成時にUnset()で初期化 | Entity生成時にUnset()で未削除状態に設定 |
| **値の由来** | `_clock.JstNow` (IClock) | `_clock.JstNow` (IClock) | 削除操作時に `_clock.JstNow` (IClock) |
| **クロック** | 🕐 システム唯一クロック使用 | 🕐 システム唯一クロック使用 | 🕐 システム唯一クロック使用 |
| **更新頻度** | なし（完全不変） | 変更のたびに更新 | 最大1回（削除時のみ） |
| **変更可能性** | 不可 | 可能 | 実質不可（一度削除後は変更なし） |
| **取消** | 不可 | 不可（上書きのみ） | 可能（Unset()で復旧） |

---

## 3. プロパティ比較

### 3.1 共通プロパティ

| プロパティ | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **Value** | `DateTime` | `DateTime` | `DateTime?` |
| **IsSet** | 常に `true` | 常に `true` | false（未削除）or true（削除済み） |

### 3.2 専門プロパティ

| プロパティ | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **IsCreated** | — | — | — |
| **IsUpdated** | — | — | — |
| **IsDeleted** | — | — | Value != null |

---

## 4. ファクトリメソッド比較

| メソッド | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **From(LocalDateTime)** | ✓ | ✓ | ✓ |
| **Unset()** | — | ✓ | ✓ |
| **TryFrom(LocalDateTime?)** | ✓ | ✓ | ✓ |

**説明:**
- ✅ 推奨: LocalDateTime（IClock.JstNow から取得）
- CreatedAt: From のみ使用
- UpdatedAt/DeletedAt: From（値あり）or Unset()（未設定）で生成

---

## 5. 検証ルール比較

| 検証項目 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **null** | 許可しない（必須） | 許可しない（必須） | 許可（未削除状態） |
| **DateTime.MinValue** | 除外 | 除外 | 除外 |
| **DateTime.MaxValue** | 除外 | 除外 | 除外 |
| **過去の日時** | 許可 | 許可 | 許可 |
| **未来の日時** | 許可 | 許可 | 許可 |
| **クロック源** | IClock（JST=Unspecified） | IClock（JST=Unspecified） | IClock（JST=Unspecified） |
| **UTC/Local** | 区別しない | 区別しない | 区別しない |

---

## 6. 等価性・ハッシング比較

| 特性 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **Equals(object?)** | ✓ | ✓ | ✓ |
| **Equals(ValueObject?)** | ✓ | ✓ | ✓ |
| **GetHashCode()** | ✓ | ✓ | ✓ |
| **HashSet 対応** | ✓ | ✓ | ✓ |
| **Dictionary キー** | ✓ | ✓ | ✓ |

**等価性の判定基準:**
- CreatedAt: Value（DateTime）が等価 → インスタンス等価
- UpdatedAt: Value（DateTime）が等価 → インスタンス等価
- DeletedAt: Value（DateTime?）が等価 → インスタンス等価（null同士も等価）

---

## 7. 使用目的比較

| 目的 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **監査追跡** | 作成者の特定（日時から） | 最終変更者の特定 | 削除者の特定 |
| **期間検索** | "2025年1月に作成" | "2025年1月に更新" | "2025年1月に削除" |
| **ソート** | 作成順 | 更新順（最新優先） | 削除順 |
| **データ保持期間管理** | 保持開始日 | 最後のアクティビティ | 削除日（廃棄判定） |
| **ビジネス論理** | Entity の履歴開始点 | Entity の変更履歴 | ソフト削除の状態判定 |

---

## 8. Entity での使用パターン比較

### 8.1 宣言パターン

```csharp
public class Entity
{
    // CreatedAt: 作成日時（必須、不変）
    public CreatedAt CreatedAt { get; }
    
    // UpdatedAt: 更新日時（必須、可変）
    public UpdatedAt UpdatedAt { get; private set; }
    
    // DeletedAt: 削除日時（null許容、実質不変）
    public DeletedAt DeletedAt { get; private set; }
}
```

### 8.2 初期化パターン

| 段階 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **Entity生成** | `CreatedAt.From(_clock.JstNow)` | `UpdatedAt.Unset()` | `DeletedAt.Unset()` |
| **Entityコピー復元** | 元の値を保持 | 復元日時に更新 | 元の値を保持 |
| **Entity更新** | 変更なし | `UpdatedAt.From(_clock.JstNow)` | 変更なし |
| **Entity削除** | 変更なし | 変更なし | `DeletedAt.From(_clock.JstNow)` |

---

## 9. 実装時の責任分離

| 責任 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **値の生成** | Entity コンストラクタ | Entity の変更メソッド | Entity の削除メソッド |
| **値の更新** | 不可 | Entity/Application層 | Entity/Application層 |
| **値の読み取り** | いつでも可 | いつでも可 | いつでも可 |
| **状態判定** | IsSet（常にtrue） | IsSet（常にtrue） | IsDeleted（null判定） |

---

## 10. クエリでの使用パターン比較

### 10.1 LINQ での利用例

```csharp
// CreatedAt での検索
var entitiesCreatedInJanuary = entities
    .Where(e => e.CreatedAt.Value.Month == 1)
    .ToList();

// UpdatedAt での検索
var recentlyUpdated = entities
    .Where(e => e.UpdatedAt.Value >= DateTime.UtcNow.AddDays(-7))
    .ToList();

// DeletedAt での検索
var activeEntities = entities
    .Where(e => !e.DeletedAt.IsDeleted)
    .ToList();

var deletedInJanuary = entities
    .Where(e => e.DeletedAt.IsDeleted && 
               e.DeletedAt.Value?.Month == 1)
    .ToList();
```

### 10.2 Specification パターン

```csharp
// CreatedAt による Specification
public class CreatedAfterSpecification : Specification<Entity>
{
    public CreatedAfterSpecification(DateTime date)
    {
        AddCriteria(e => e.CreatedAt.Value >= date);
    }
}

// UpdatedAt による Specification
public class UpdatedBetweenSpecification : Specification<Entity>
{
    public UpdatedBetweenSpecification(DateTime start, DateTime end)
    {
        AddCriteria(e => e.UpdatedAt.Value >= start && 
                        e.UpdatedAt.Value <= end);
    }
}

// DeletedAt による Specification
public class NotDeletedSpecification : Specification<Entity>
{
    public NotDeletedSpecification()
    {
        AddCriteria(e => !e.DeletedAt.IsDeleted);
    }
}

public class DeletedBetweenSpecification : Specification<Entity>
{
    public DeletedBetweenSpecification(DateTime start, DateTime end)
    {
        AddCriteria(e => e.DeletedAt.IsDeleted && 
                        e.DeletedAt.Value >= start && 
                        e.DeletedAt.Value <= end);
    }
}
```

---

## 11. Repository での使用パターン比較

### 11.1 取得メソッド

| メソッド | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **GetAllAsync()** | — | — | 削除済みを除外（デフォルト） |
| **GetByCreatedAtAsync(date)** | ✓ | — | — |
| **GetUpdatedAfterAsync(date)** | — | ✓ | — |
| **GetActiveAsync()** | — | — | ✓（IsDeleted=false） |
| **GetDeletedAsync()** | — | — | ✓（IsDeleted=true） |

### 11.2 実装例

```csharp
public class EntityRepository : IEntityRepository
{
    // CreatedAt での検索
    public async Task<List<Entity>> GetCreatedInDateRangeAsync(
        DateTime start, DateTime end)
    {
        return await _context.Entities
            .Where(e => e.CreatedAt.Value >= start && 
                       e.CreatedAt.Value <= end)
            .ToListAsync();
    }
    
    // UpdatedAt での検索
    public async Task<List<Entity>> GetUpdatedAfterAsync(DateTime date)
    {
        return await _context.Entities
            .Where(e => e.UpdatedAt.Value >= date)
            .OrderByDescending(e => e.UpdatedAt.Value)
            .ToListAsync();
    }
    
    // DeletedAt での検索
    public async Task<List<Entity>> GetActiveAsync()
    {
        return await _context.Entities
            .Where(e => !e.DeletedAt.IsDeleted)
            .ToListAsync();
    }
    
    public async Task<List<Entity>> GetDeletedInDateRangeAsync(
        DateTime start, DateTime end)
    {
        return await _context.Entities
            .Where(e => e.DeletedAt.IsDeleted && 
                       e.DeletedAt.Value >= start && 
                       e.DeletedAt.Value <= end)
            .ToListAsync();
    }
}
```

---

## 12. テスト戦略比較

| テスト観点 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **生成テスト** | From のみ | From / Unset | From / Unset |
| **更新テスト** | 更新不可テスト | 更新可能テスト | 削除状態トグル不可テスト |
| **等価性テスト** | DateTime の値で比較 | DateTime の値で比較 | DateTime? の値で比較（null含む） |
| **不変性テスト** | 完全不変を確認 | 部分的可変を確認 | 実質不変を確認 |
| **クエリテスト** | 期間検索テスト | 更新順ソートテスト | 削除状態フィルタテスト |

---

## 13. マイグレーション・データベース設計

### 13.1 カラム定義

```sql
-- CreatedAt: NULL NOT ALLOWED
ALTER TABLE Entities ADD CreatedAtUtc DATETIME NOT NULL DEFAULT GETUTCDATE();

-- UpdatedAt: NULL NOT ALLOWED
ALTER TABLE Entities ADD UpdatedAtUtc DATETIME NOT NULL DEFAULT GETUTCDATE();

-- DeletedAt: NULL ALLOWED（null=未削除, 値=削除済み）
ALTER TABLE Entities ADD DeletedAtUtc DATETIME NULL DEFAULT NULL;
```

### 13.2 インデックス戦略

| インデックス | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **単一カラムインデックス** | ✓（期間検索用） | ✓（ソート用） | ✓（削除状態フィルタ用） |
| **複合インデックス** | (DeletedAtUtc, CreatedAtUtc) | (DeletedAtUtc, UpdatedAtUtc) | — |
| **パーティション** | 作成年月で分割 | — | — |

### 13.3 クエリ最適化

```sql
-- CreatedAt での効率的なクエリ
SELECT * FROM Entities
WHERE DeletedAtUtc IS NULL
  AND CreatedAtUtc BETWEEN @StartDate AND @EndDate
ORDER BY CreatedAtUtc DESC;

-- UpdatedAt での効率的なクエリ
SELECT * FROM Entities
WHERE DeletedAtUtc IS NULL
  AND UpdatedAtUtc >= @Date
ORDER BY UpdatedAtUtc DESC;

-- DeletedAt での効率的なクエリ
SELECT * FROM Entities
WHERE DeletedAtUtc IS NULL;  -- アクティブなエンティティのみ
```

---

## 14. 監査・コンプライアンスでの役割

| 観点 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **データ作成元の特定** | ✓（日時から推定） | — | — |
| **最後のアクティビティ** | — | ✓ | — |
| **削除履歴の記録** | — | — | ✓ |
| **データ保持期間管理** | ✓（開始点） | ✓（終了点） | ✓（削除判定） |
| **GDPR "右の忘却"** | — | — | ✓（削除タイムスタンプ） |
| **監査ログとの連携** | 作成者名は別途管理 | 更新者名は別途管理 | 削除者名は別途管理 |

---

## 15. 実装上の推奨事項

### 15.1 システム唯一クロック（IClock）の使用

```csharp
// 推奨: IClock（システム唯一の時間源）を使用
public class Entity
{
    private readonly IClock _clock;
    
    public Entity(IClock clock)
    {
        _clock = clock;
        CreatedAt = CreatedAt.From(_clock.JstNow);
        UpdatedAt = UpdatedAt.From(_clock.JstNow);
        DeletedAt = DeletedAt.NotDeleted();
    }
    
    public void Update(string name)
    {
        Name = name;
        UpdatedAt = UpdatedAt.From(_clock.JstNow);
    }
    
    public void SoftDelete()
    {
        if (!DeletedAt.IsDeleted)
        {
            DeletedAt = DeletedAt.From(_clock.JstNow);
        }
    }
}

// 非推奨: DateTime.UtcNow を直接使用
// CreatedAt.From(DateTime.UtcNow)  // ← テスト困難、時刻制御不可
```

### 15.2 削除操作の権限管理

```csharp
// DeletedAt は状態を記録するだけ
// 削除権限は別途チェック
public async Task DeleteEntityAsync(Entity entity, User user)
{
    if (!user.HasDeletePermission())
        throw new UnauthorizedAccessException("削除権限がありません");
    
    entity.SoftDelete();  // DeletedAt.From(_clock.JstNow) - Entity 内で IClock を使用
    await _repository.SaveAsync(entity);
}
```

### 15.3 復旧操作の慎重な実装

```csharp
// 削除復旧は監査ログに記録すること
public async Task RestoreEntityAsync(Entity entity, User user)
{
    if (!user.HasAdministratorRole())
        throw new UnauthorizedAccessException("管理者のみ復旧可能");
    
    var originalDeletedAt = entity.DeletedAt;
    entity.DeletedAt = DeletedAt.Unset();  // ← Unset() で未削除状態に復旧
    
    // 削除復旧を監査ログに記録
    await _auditLog.LogRestoration(entity.Id, originalDeletedAt, user);
    
    await _repository.SaveAsync(entity);
}
```

---

## 16. 比較まとめ表

| 項目 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **型** | DateTime | DateTime | DateTime? |
| **null許容** | × | × | ✓ |
| **生成タイミング** | Entity生成時 | Entity生成・更新時 | Entity削除時 |
| **更新可能** | × | ✓ | 実質× |
| **ビジネス用途** | 作成履歴 | 最終更新時刻 | ソフト削除判定 |
| **クエリ利用** | 期間検索 | ソート・検索 | フィルタリング |
| **監査用途** | 作成日時 | 更新日時 | 削除日時 |
| **テスト重点** | 不変性 | 可変性 | 状態遷移 |

---

## 17. 選択フロー

```
Entity に日時情報を追加する

↓

「Entity の作成日時を記録したい」
→ CreatedAt を使用

↓

「Entity の最終更新日時を追跡したい」
→ UpdatedAt を使用

↓

「Entity を論理削除し、削除日時を記録したい」
→ DeletedAt を使用（削除フラグは不要）
```

---

## 18. 実装チェックリスト

### 18.1 CreatedAt チェックリスト

- [ ] Entity コンストラクタで IClock を DI 受け取り
- [ ] `CreatedAt.From(_clock.JstNow)` で初期化
- [ ] CreatedAt は public readonly、Value は変更不可
- [ ] Repository の期間検索で CreatedAt.Value を使用
- [ ] テストで MockClock を使用し、不変性を確認

### 18.2 UpdatedAt チェックリスト

- [ ] Entity コンストラクタで IClock を DI 受け取り
- [ ] `UpdatedAt.Unset()` で初期化（未更新状態）
- [ ] Entity 更新時に `UpdatedAt = UpdatedAt.From(_clock.JstNow)` で更新
- [ ] Repository でソート時に UpdatedAt.Value を使用
- [ ] テストで MockClock を使用し、更新可能性を確認

### 18.3 DeletedAt チェックリスト

- [ ] Entity コンストラクタで IClock を DI 受け取り
- [ ] `DeletedAt.Unset()` で初期化（未削除状態）
- [ ] Entity 削除メソッドで `DeletedAt = DeletedAt.From(_clock.JstNow)` で設定
- [ ] Repository の GetActiveAsync() で `!e.DeletedAt.IsDeleted` でフィルタリング
- [ ] マイグレーションで DeletedAtUtc DATETIME NULL を追加
- [ ] テストで MockClock を使用し、null と値の等価性を確認

---

## 19. まとめ

3つの監査 ValueObject の比較：

| 特性 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **本質** | 作成タイムスタンプ | 更新タイムスタンプ | 削除タイムスタンプ |
| **不変性** | 完全不変 | 可変 | 実質不変 |
| **ビジネス価値** | 履歴開始点 | 最新アクティビティ | ソフト削除判定 |
| **監査価値** | 高（作成時刻） | 高（変更追跡） | 高（削除記録） |
| **デザインパターン** | 不変ValueObject | 可変Entity属性 | null許容ValueObject |

**結論:**
- **CreatedAt**: Entity の履歴的信頼性を支え、作成元追跡の基点
- **UpdatedAt**: Entity の最新性を保ち、変更履歴の終点
- **DeletedAt**: 削除フラグなしで論理削除を実装し、削除日時を監査情報として内包
