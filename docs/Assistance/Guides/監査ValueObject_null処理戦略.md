# Domain層 null厳格性の実装戦略

> **📚 関連ドキュメント**: より詳細な技術設計については、[監査ValueObject_null処理詳細設計.md](../../../docs/SharedKernel/ValueObjects/Audit/監査ValueObject_null処理詳細設計.md) を参照してください。

---

## 📋 問題設定

| 層 | 特性 | 設計原則 |
|---|---|---|
| **永続化層（DB）** | CreatedAt: NOT NULL<br/>UpdatedAt: NULL OK<br/>DeletedAt: NULL OK | 実装側の制約に従う |
| **Domain層** | すべて null を受け入れない | Domain層は null を知らない |
| **中継地点** | TryFrom は失敗 or Unset を返す | 層間の責任の境界 |

---

## 🔑 核となる設計原理

### PrimitiveValueObject の 3段階フロー

```
DB値（nullable） 
    ↓
TryFrom メソッド（層間の境界）
    ├─ Domain層へ渡す前に null チェック
    ├─ CreatedAt/UpdatedAt: null → false（失敗）
    └─ DeletedAt: null → NotDeleted()（Unset）
    ↓
Domain層（null を一切知らない）
    └─ IsSet = true のインスタンスのみ存在
```

**重点:** TryFrom は「層間のフィルター」として機能する。

---

## ✅ 各ValueObject の TryFrom 仕様（既に設計済み）

### 1. CreatedAt（永続化層では NOT NULL）

```csharp
// TryFrom - 【失敗条件】
public static bool TryFrom(LocalDateTime? input, out CreatedAt result)
{
    result = default!;
    
    // ✗ null が来たら失敗
    if (!input.HasValue) 
        return false;  // ← Domain層は null を見ない
    
    // ✓ 値があれば From() で生成
    try
    {
        result = From(input.Value);  // MinValue/MaxValue チェック
        return true;
    }
    catch
    {
        return false;
    }
}
```

**Domain層での保証:**
- `TryFrom` が true を返したときだけ、CreatedAt インスタンスは null でない DateTime を持つ
- Domain層の任意の場所で `createdAt.Value` を呼べる（null安全）

---

### 2. UpdatedAt（永続化層では NULL OK、未更新状態を Unset で管理）

```csharp
// TryFrom - 【null 対応】
public static bool TryFrom(LocalDateTime? input, out UpdatedAt result)
{
    // ✓ null が来たら Unset()で成功（未更新状態を表現）
    if (input == null || !input.HasValue)
    {
        result = Unset();  // IsSet = false
        return true;       // ← 成功（null は「未更新」として処理）
    }
    
    // ✓ 値があれば From() で生成
    try
    {
        result = From(input.Value);  // MinValue/MaxValue チェック
        return true;
    }
    catch (ArgumentException)
    {
        result = null!;
        return false;
    }
}
```

**Domain層での保証:**
- UpdatedAt が Unset（IsSet=false）なら、未更新状態
- UpdatedAt が Set（IsSet=true）なら、有効な DateTime を持つ
- Domain層には null が存在しない、`IsSet` で「更新済み/未更新」を管理
- Domain層の Code: `if (entity.UpdatedAt.HasUpdated)` で更新状態を判定

---

### 3. DeletedAt（永続化層では NULL OK、かつ「未削除」を表現）

```csharp
// TryFrom - 【成功条件】
public static bool TryFrom(LocalDateTime? input, out DeletedAt result)
{
    // ✓ null が来たら NotDeleted() を返す（成功）
    if (!input.HasValue)
    {
        result = NotDeleted();  // IsSet = false
        return true;  // ← Domain層へ「未削除状態」を渡す
    }
    
    // ✓ 値があれば From() で生成
    try
    {
        result = From(input.Value);  // MinValue/MaxValue チェック
        return true;
    }
    catch
    {
        result = default!;
        return false;
    }
}
```

**Domain層での保証:**
- `DeletedAt.IsDeleted` が false → 未削除（ロジックでは null を見ない）
- `DeletedAt.IsDeleted` が true → 削除済み（日時を参照可能）
- Domain層には null が存在しない、`IsSet` で状態を管理

---

## 🛠️ Infrastructure層（永続化層）での実装パターン

### Repository が DB から受け取った値を Domain層へ渡す

```csharp
public class CarRepository : ICarRepository
{
    private readonly DbContext _context;
    
    public async Task<Car?> GetByIdAsync(CarId carId)
    {
        var dto = await _context.Cars
            .FirstOrDefaultAsync(c => c.Id == carId.Value);
        
        if (dto == null) return null;
        
        // ============ 層間の境界 ============
        
        // createdAt: DB では NOT NULL だが、念のため TryFrom で検証
        if (!CreatedAt.TryFrom(new LocalDateTime(dto.CreatedAtDb), out var createdAt))
        {
            throw new InvalidOperationException(
                $"Car {carId}: CreatedAt が無効な日時です");
        }
        
        // updatedAt: DB では NULL OK、TryFrom は常に成功（null → Unset で成功）
        if (!UpdatedAt.TryFrom(
                dto.UpdatedAtDb.HasValue 
                    ? new LocalDateTime(dto.UpdatedAtDb.Value) 
                    : null,
                out var updatedAt))
        {
            throw new InvalidOperationException(
                $"Car {carId}: UpdatedAt が無効な日時です");
        }
        // updatedAt は必ず Set or Unset のいずれか（null は存在しない）
        
        // deletedAt: DB では NULL OK、null=未削除として処理
        if (!DeletedAt.TryFrom(
                dto.DeletedAtDb.HasValue 
                    ? new LocalDateTime(dto.DeletedAtDb.Value) 
                    : null,
                out var deletedAt))
        {
            throw new InvalidOperationException(
                $"Car {carId}: DeletedAt が無効な日時です");
        }
        // deletedAt は必ず Unset or 削除済みのいずれか
        
        // ============ Domain層へ渡す ============
        
        return Car.Reconstruct(carId, createdAt, updatedAt, deletedAt);
    }
}
```

---

## 📐 実装のチェックリスト

### TryFrom の責務分離

- [ ] **CreatedAt.TryFrom**: `input == null` → `false`（DB NOT NULL違反）
- [ ] **UpdatedAt.TryFrom**: `input == null` → `Unset()`（成功、未更新状態を表現）
- [ ] **DeletedAt.TryFrom**: `input == null` → `Unset()`（成功、未削除状態を表現）

### Repository で

- [ ] CreatedAt が null → 例外を投げる（DB制約違反）
- [ ] UpdatedAt が null → `Unset()` を Domain層に渡す（未更新を表現）
- [ ] DeletedAt が null → `Unset()` を Domain層に渡す（未削除を表現）

### Domain層では

- [ ] `createdAt.Value` を安全に呼べる
- [ ] `updatedAt.HasUpdated` で更新済み判定（IsSet で状態管理）
- [ ] `updatedAt.Value` は `HasUpdated` が true のときのみ有効
- [ ] `deletedAt.IsDeleted` で論理削除判定（null を見ない）

---

## 📖 実装例

### Domain層のEntity（null を知らない）

```csharp
public class Car : Entity
{
    public CarId CarId { get; }
    public CreatedAt CreatedAt { get; }
    public UpdatedAt UpdatedAt { get; private set; }  // null ではなく Unset 状態で管理
    public DeletedAt DeletedAt { get; private set; }  // null ではなく Unset 状態で管理
    
    // ドメインメソッド
    public bool IsDeleted => DeletedAt.IsDeleted;    // null を見ない
    public bool HasUpdated => UpdatedAt.HasUpdated;  // IsSet で判定
    
    public void Update(string name)
    {
        Name = name;
        // UpdatedAt は Infrastructure 層で設定（Entity は IClock を持たない）
    }
    
    // Reconstruct パターン
    public static Car Reconstruct(
        CarId carId,
        CreatedAt createdAt,
        UpdatedAt updatedAt,   // null ではなく Unset 状態で管理
        DeletedAt deletedAt)   // IsSet で管理
    {
        return new Car
        {
            CarId = carId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,     // Unset() または Set状態
            DeletedAt = deletedAt      // Unset() または Set状態
        };
    }
}
```

---

## 🎯 まとめ

| 層 | 責務 | 例 |
|---|---|---|
| **Infrastructure** | null チェック + TryFrom 呼び出し | Repository |
| **TryFrom** | null → Unset/値 に変換（層間フィルター） | Layer Boundary |
| **Domain** | null を見ない、IsSet で状態管理 | Entity ロジック |

**原則:** 
> Domain層が null を目にすることは決してない。  
> TryFrom がその前で null を「Unset状態に変換」し、Domain層へ渡す。  
> Unset は ValueObject の有効な状態（null ではない）で、IsSet フラグで管理される。

**各ValueObjectの変換:**
- **CreatedAt.TryFrom(null)** → false（失敗・例外処理）
- **UpdatedAt.TryFrom(null)** → Unset()で成功（未更新状態）
- **DeletedAt.TryFrom(null)** → Unset()で成功（未削除状態）
