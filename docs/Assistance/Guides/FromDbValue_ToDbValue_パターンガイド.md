# FromDbValue / ToDbValue パターンガイド（DB 日時の変換）

**バージョン**: 2.0  
**最終更新**: 2026-09-26  
**適用範囲**: 全層（日時の DB 変換）  
**重要度**: HIGH

---

## 📋 概要

**DB の日時型（`DateTime`）と Domain の日時型（`LocalDateTime`）の変換は、Infrastructure（Mapper / Repository）が行う。**
Domain / SharedKernel / Application の型（値オブジェクトを含む）は `DateTime` を知らない。

- **DB → Domain**: Infrastructure が `DateTime?` を `LocalDateTime?` に変換し、値オブジェクトの `TryFrom(LocalDateTime?)` に渡す（`null` は `TryFrom` が `Unset()` または失敗に変換）
- **Domain → DB**: Infrastructure が値オブジェクトの `LocalDateTime`（`.Value` が `DateTime`）を DbModel に設定する

> **方針の決定**: 2026-09-26。旧バージョン（1.0）は、値オブジェクトが `FromDbValue(DateTime)` / `TryFromDbValue(DateTime?)` / `ToDbValue()` を持つ設計だった。「Domain が DB の型を知らない」という原則に合わせて、変換を Infrastructure に移す。
>
> **移行完了**: 2026-09-26。旧形式は、8 つの値オブジェクト（`CreatedAt` / `UpdatedAt` / `DeletedAt`、`AbolishedOn`、`RetiredOn`、`EndOn`、`ExpirationOn`、`EffectiveAt`）から全て削除した（[原則完全準拠 実装計画](../Plans/20260926_原則完全準拠_実装計画.md) のフェーズ 4）。旧形式は新規に追加しない。

### 対象外（現状維持）

日時以外の値の DB 変換（`RowId` の `long`、`BizDivision` の `char`、コードや名称などの文字列）にも `FromDbValue` / `ToDbValue` / `TryFromDbValue` がある。**これらは今回の方針の対象外**で、当面は現状のまま。同じ考え方が当てはまるため、扱いは別途決める（実装計画の「4-E 対象外」）。

---

## 🎯 なぜ Infrastructure で変換するか

1. **層の責務**: `DateTime` は DB ネイティブの型。Domain の値オブジェクトが DB の型を知ると、Domain が Infrastructure の事情に依存する
2. **型不一致**: DB は `DateTime`（タイムゾーンなし）、Domain は `LocalDateTime`（JST を型で保証）。変換は境界で 1 回だけ行う
3. **テスト容易性**: 値オブジェクトのテストが DB の型に依存しない。変換のテストは Mapper / Repository 側に集まる

---

## 📌 実装パターン

値オブジェクト側の入口は `From(LocalDateTime)` / `TryFrom(LocalDateTime?)` / `Unset()` のみ。

### パターン 1: 必須フィールド（CreatedAt）

```csharp
// 値オブジェクト（SharedKernel）
public sealed class CreatedAt : PrimitiveValueObject<LocalDateTime>
{
    public static CreatedAt From(LocalDateTime value) => new(value);

    // null は失敗（NOT NULL の DB カラム）
    public static bool TryFrom(LocalDateTime? input, out CreatedAt result)
    {
        if (!input.HasValue) { result = null!; return false; }
        try { result = From(input.Value); return true; }
        catch (ArgumentException) { result = null!; return false; }
    }
}
```

```csharp
// Infrastructure（Repository / Mapper）：DB の DateTime を変換して渡す
if (!CreatedAt.TryFrom(dbModel.CreatedAt.ToLocalDateTime(), out var createdAt))
    throw new InvalidOperationException("Invalid CreatedAt for Employee");
```

**特徴**: null は失敗、Domain では常に有効な値を保持

### パターン 2: オプションフィールド（UpdatedAt / DeletedAt / RetiredOn）

```csharp
// 値オブジェクト（SharedKernel / Domain）
public sealed class UpdatedAt : PrimitiveValueObject<LocalDateTime?>, IEquatable<UpdatedAt>
{
    public static UpdatedAt From(LocalDateTime value) => new(value, true);

    // 未更新状態を表現（IsSet=false）
    public static UpdatedAt Unset() => new(LocalDateTime.MinValue, false);

    // null は Unset に変換して成功
    public static bool TryFrom(LocalDateTime? input, out UpdatedAt result)
    {
        if (input == null) { result = Unset(); return true; }
        try { result = From(input.Value); return true; }
        catch (ArgumentException) { result = null!; return false; }
    }
}
```

```csharp
// Infrastructure（Repository / Mapper）：DB の DateTime? を変換して渡す
if (!UpdatedAt.TryFrom(dbModel.UpdatedAt.ToLocalDateTimeOrNull(), out var updatedAt))
    throw new InvalidOperationException("Invalid UpdatedAt");

// Domain 層での判定
if (employee.UpdatedAt.HasUpdated)  // IsSet フラグを確認（null チェックは不要）
{
    // 更新済みの処理
}
```

**特徴**: null は自動的に Unset() に変換、Domain では IsSet フラグで判定

### 書き込み（Domain → DB）

```csharp
// Mapper.ToDbModel
RetiredOn = entity.RetiredOn.HasRetired
    ? entity.RetiredOn.Value?.Value   // LocalDateTime.Value = DateTime
    : null;

// 未設定の値オブジェクトは Value が MinValue を持つため、必ず IsSet（別名プロパティ）で判定してから取り出す
```

---

## 🧰 変換ヘルパー（Infrastructure）

`DateTime` ↔ `LocalDateTime` の変換は、Infrastructure の拡張メソッド `DbDateTimeExtensions`（`SupportAdvance.Infrastructure.Mappers`。`src/Infrastructure/Mappers/DbDateTimeExtensions.cs`）にまとめている。

| メソッド | 変換 |
|---|---|
| `DateTime.ToLocalDateTime()` | `DateTime` → `LocalDateTime`（NOT NULL の列用。`TryFrom(LocalDateTime?)` には暗黙に渡せる） |
| `DateTime?.ToLocalDateTimeOrNull()` | `DateTime?` → `LocalDateTime?`（null はそのまま null） |
| `LocalDateTime.Value` | `LocalDateTime` → `DateTime`（既存のプロパティ。ヘルパー不要） |

- `LocalDateTime(DateTime)` のコンストラクターが、DB の値を JST として扱う変換の実体
- ヘルパーは Infrastructure に置く（Domain / SharedKernel には置かない）

---

## 🔄 データフロー

### 読み込み（DB → Domain）

```
Repository.GetByIdAsync
  ↓
SQL実行 → DbModel（DateTime / DateTime? 型）
  ↓
dbModel.UpdatedAt.ToLocalDateTimeOrNull()   ← DateTime? → LocalDateTime?（Infrastructure）
  ↓
UpdatedAt.TryFrom(localDateTime, out var updatedAt)  ← 層間フィルター
  ├─ null → Unset() に変換
  └─ out updatedAt（Domain 型）
  ↓
Mapper.ToDomainEntity(dbModel, clock)  ← ビジネスフィールドの変換（DateTime → LocalDateTime も Mapper が実施）
  ↓
Entity（Domain Model）← Domain での null チェック不要
```

### 書き込み（Domain → DB）

```
UpdateAsync(entity)
  ↓
Mapper.ToDbModel(entity)  ← ビジネスフィールドのみ変換
  ├─ entity.RetiredOn.Value?.Value  ← LocalDateTime → DateTime
  └─ DbModel生成
  ↓
SetUpdatedAtAudit(dbModel)  ← Repository が監査フィールド設定
  ├─ dbModel.UpdatedAt = Clock.JstNow.Value
  └─ dbModel.UpdatedBy = CurrentUser.EmployeeRowId
  ↓
SQL UPDATE
```

---

## 🛡️ 層間フィルター（Repository での実装）

Repository は「層間フィルター」の役割を果たし、DB の null を Domain の Unset に変換する。

```csharp
public async Task<Employee?> GetByIdAsync(EmployeeRowId id)
{
    var dbModel = await connection.QueryFirstOrDefaultAsync<EmployeeDbModel>(...);
    if (dbModel == null) return null;

    // ============ 層間フィルター ============
    // DB の DateTime? を LocalDateTime? に変換し、TryFrom で null を Unset() に変換（Infrastructure 層の責務）
    if (!CreatedAt.TryFrom(dbModel.CreatedAt.ToLocalDateTime(), out var createdAt))
        throw new InvalidOperationException($"Invalid CreatedAt for Employee RowId={id.Value}");

    if (!UpdatedAt.TryFrom(dbModel.UpdatedAt.ToLocalDateTimeOrNull(), out var updatedAt))
        throw new InvalidOperationException($"Invalid UpdatedAt for Employee RowId={id.Value}");

    if (!DeletedAt.TryFrom(dbModel.DeletedAt.ToLocalDateTimeOrNull(), out var deletedAt))
        throw new InvalidOperationException($"Invalid DeletedAt for Employee RowId={id.Value}");

    return _mapper.ToDomainEntity(dbModel, _clock);
}
```

---

## 🎯 Mapper と Repository の責務分離

| 責務 | Mapper | Repository |
|---|---|---|
| ビジネスフィールドの変換（`DateTime` ↔ `LocalDateTime` を含む） | ✓ | - |
| 監査フィールドの読み込み（DB → 値オブジェクト） | - | ✓ |
| 監査フィールドの設定（CreatedAt / UpdatedAt / DeletedAt / *_by） | ✗ | ✓ |
| Clock 依存 | なし（Reconstruct への受け渡しは引数で受け取った `IClock` を使用） | あり（DI 経由） |

```csharp
public EmployeeDbModel ToDbModel(Employee entity) => new()
{
    RowId = entity.RowId.Value,
    BizDivision = entity.TypeDivision.ToDbValue(),          // 日時以外は現状維持（対象外）
    RetiredOn = entity.RetiredOn.HasRetired
        ? entity.RetiredOn.Value?.Value                     // LocalDateTime → DateTime
        : null,
    // ❌ 監査フィールドは設定しない（Repository が責務を持つ）
};
```

---

## 📊 パターン比較表

| 項目 | CreatedAt（必須） | UpdatedAt / DeletedAt / RetiredOn（オプション） |
|---|---|---|
| **DB カラム** | NOT NULL | NULLABLE |
| **`TryFrom(null)`** | 失敗（false） | 成功（Unset） |
| **Domain での null** | なし | IsSet で判定 |
| **別名プロパティ** | — | HasUpdated, IsDeleted, HasRetired など |
| **DB 型の変換** | Infrastructure（`ToLocalDateTime()`） | Infrastructure（`ToLocalDateTimeOrNull()`） |

---

## 💡 よくあるエラー

### ❌ 間違い 1: 値オブジェクトに DateTime を持ち込む

```csharp
// ❌ 間違い：値オブジェクトが DB の型を知っている
public static bool TryFromDbValue(DateTime? input, out UpdatedAt result) { ... }
public DateTime ToDbValue() => ...;
```

**問題**: Domain / SharedKernel が DB の型（Infrastructure の事情）に依存する。日時の入口は `TryFrom(LocalDateTime?)` のみ。

### ❌ 間違い 2: 未設定の値をそのまま DbModel に入れる

```csharp
// ❌ 間違い：Unset の Value は MinValue（0001-01-01）。そのまま DB に保存される
RetiredOn = entity.RetiredOn.Value?.Value,

// ✅ 正しい：IsSet（別名プロパティ）で判定してから取り出す
RetiredOn = entity.RetiredOn.HasRetired ? entity.RetiredOn.Value?.Value : null,
```

### ❌ 間違い 3: Domain で null チェック

```csharp
// ❌ 間違い
if (employee.UpdatedAt == null) { ... }      // Domain では起こらない

// ✅ 正しい
if (!employee.UpdatedAt.HasUpdated) { ... }  // IsSet フラグで判定
```

---

## 🔧 実装チェックリスト

日時を扱う値オブジェクトを新規に作る場合：

- [ ] **`From(LocalDateTime)`**: Domain 型での生成
- [ ] **`TryFrom(LocalDateTime?)`**: null 安全な生成（必須は null → 失敗、オプションは null → Unset）
- [ ] **`Unset()`**: オプション型の場合のみ（IsSet=false）
- [ ] **別名プロパティ**: HasUpdated, IsDeleted など（可読性向上）
- [ ] **`DateTime` を公開メンバーに持たない**（`FromDbValue(DateTime)` / `TryFromDbValue(DateTime?)` / `ToDbValue()` を作らない）
- [ ] Infrastructure 側で `ToLocalDateTime()` / `ToLocalDateTimeOrNull()` を使って変換している

---

## 🔁 旧形式との対応（移行用）

| 旧形式（値オブジェクトに実装。廃止予定） | 新形式 |
|---|---|
| `X.FromDbValue(dbValue)` | `X.From(dbValue.ToLocalDateTime())` |
| `X.TryFromDbValue(dbValue, out var x)`（`DateTime?`） | `X.TryFrom(dbValue.ToLocalDateTimeOrNull(), out var x)` |
| `x.ToDbValue()` | `x.HasXxx ? x.Value?.Value : null`（必須型は `x.Value.Value`） |

---

## 📖 参考ドキュメント

- [null厳格性設計ガイド](null厳格性設計ガイド.md)
- [LocalDateTime_タイムゾーン_ガイド.md](LocalDateTime_タイムゾーン_ガイド.md)
- [Mapper_パターンガイド.md](Mapper_パターンガイド.md)
- [Repository_パターンガイド.md](Repository_パターンガイド.md)
- [原則完全準拠 実装計画](../Plans/20260926_原則完全準拠_実装計画.md)（フェーズ 4）

---

## 更新履歴

| 日付 | 内容 |
|---|---|
| 2026-09-26 | 2.0。日時の DB 変換を値オブジェクトから Infrastructure（Mapper / Repository）に移す方針に全面改訂。値オブジェクトは `TryFrom(LocalDateTime?)` のみ。旧形式との対応表、変換ヘルパー、移行中の注意を追加 |
| 2026-09-12 | 1.0。初版 |
