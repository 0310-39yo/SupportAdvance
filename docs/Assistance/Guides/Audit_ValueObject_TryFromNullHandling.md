# Domain層 null厳格性の実装戦略

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

### 2. UpdatedAt（永続化層では NULL OK）

```csharp
// TryFrom - 【失敗条件】
public static bool TryFrom(DateTime? input, out UpdatedAt result)
{
    result = default!;
    
    // ✗ null が来たら失敗（Domain層は値が必須）
    if (!input.HasValue)
        return false;  // ← null はDomain層へ通さない
    
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
- UpdatedAt が存在するなら、必ず有効な DateTime を持つ
- Domain層の Code: `if (entity.UpdatedAt.Equals(createdAt))` は安全

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
        
        // updatedAt: DB では NULL OK だが、null なら TryFrom は失敗
        UpdatedAt? updatedAt = null;
        if (dto.UpdatedAtDb.HasValue)
        {
            if (!UpdatedAt.TryFrom(dto.UpdatedAtDb.Value, out var updated))
            {
                throw new InvalidOperationException(
                    $"Car {carId}: UpdatedAt が無効な日時です");
            }
            updatedAt = updated;
        }
        // null の場合、updatedAt を Domain層へ渡さない
        
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

- [ ] **CreatedAt.TryFrom**: `input == null` → `false`
- [ ] **UpdatedAt.TryFrom**: `input == null` → `false`
- [ ] **DeletedAt.TryFrom**: `input == null` → `NotDeleted()`（成功）

### Repository で

- [ ] CreatedAt が null → 例外を投げる（DB制約違反）
- [ ] UpdatedAt が null → Domain層に Unset を渡さない（または省略）
- [ ] DeletedAt が null → Domain層に `NotDeleted()` を渡す

### Domain層では

- [ ] `createdAt.Value` を安全に呼べる
- [ ] `updatedAt?.Value` で null安全にアクセス
- [ ] `deletedAt.IsDeleted` で論理削除判定（null を見ない）

---

## 📖 実装例

### Domain層のEntity（null を知らない）

```csharp
public class Car : Entity
{
    public CarId CarId { get; }
    public CreatedAt CreatedAt { get; }
    public UpdatedAt? UpdatedAt { get; private set; }
    public DeletedAt DeletedAt { get; private set; }
    
    // ドメインメソッド
    public bool IsDeleted => DeletedAt.IsDeleted;  // null を見ない
    
    public void Update(string name)
    {
        Name = name;
        // UpdatedAt は Infrastructure 層で設定（Entity は IClock を持たない）
    }
    
    // Reconstruct パターン
    public static Car Reconstruct(
        CarId carId,
        CreatedAt createdAt,
        UpdatedAt? updatedAt,  // null OK
        DeletedAt deletedAt)   // IsSet で管理
    {
        return new Car
        {
            CarId = carId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            DeletedAt = deletedAt
        };
    }
}
```

---

## 🎯 まとめ

| 層 | 責務 | 例 |
|---|---|---|
| **Infrastructure** | null チェック + TryFrom 呼び出し | Repository |
| **TryFrom** | null/invalid を吸収 | Layer Boundary |
| **Domain** | null を見ない、IsSet で状態管理 | Entity ロジック |

**原則:** 
> Domain層が null を目にすることは決してない。  
> TryFrom がその前で null を「処理」し、Domain層へ渡す値に変換する。
