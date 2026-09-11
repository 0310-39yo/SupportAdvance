# FromDbValue / ToDbValue パターンガイド

**バージョン**: 1.0  
**最終更新**: 2026-09-12  
**適用範囲**: 全層  
**重要度**: HIGH

---

## 📋 概要

FromDbValue / ToDbValue は、**DB 層と Domain 層の型変換責務を明確に分離** するパターンです。

- **FromDbValue**: DB の DateTime を Domain の LocalDateTime に変換
- **ToDbValue**: Domain の LocalDateTime を DB の DateTime に変換

これにより、DB ネイティブ型と Domain の型安全性を両立させます。

---

## 🎯 なぜ必要か

### 問題

1. **型不一致**: DB は DateTime（UTC タイムゾーン無視）、Domain は LocalDateTime（タイムゾーン意識）
2. **責務の混在**: Mapper が DB null を判定して ValueObject を生成するのは不自然
3. **層の越境**: Application 層が DB null を処理する可能性

### 解決

- **Mapper**: 純粋な型変換のみ（LocalDateTime ↔ DateTime）
- **Repository**: DB null → Domain Unset の「層間フィルター」責務を持つ
- **ValueObject**: FromDbValue/ToDbValue で型変換ロジックを一元化

---

## 📌 3 つの実装パターン

### パターン 1: 必須フィールド（CreatedAt）

```csharp
public sealed class CreatedAt : PrimitiveValueObject<LocalDateTime>
{
    // Domain層: LocalDateTime を直接使用
    public static CreatedAt From(LocalDateTime value) => new(value);

    // Infrastructure層: DB DateTime を LocalDateTime に変換
    public static CreatedAt FromDbValue(DateTime value) 
        => new(new LocalDateTime(value));

    // Mapper: LocalDateTime → DateTime に変換
    public DateTime ToDbValue() => ValueField.Value;

    // Repository: null安全に生成（null=失敗）
    public static bool TryFromDbValue(DateTime? input, out CreatedAt result)
    {
        if (input == null)  // ← null は失敗
        {
            result = null!;
            return false;
        }

        try
        {
            result = FromDbValue(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            result = null!;
            return false;
        }
    }
}
```

**特徴**:
- ✅ null は失敗（NOT NULL DB カラム対応）
- ✅ Domain では常に有効な値を保持
- ✅ 必須フィールド検証を早期に実行

**使用例**:
```csharp
// Repository.GetByIdAsync
if (!CreatedAt.TryFromDbValue(dbModel.CreatedAt, out var createdAt))
    throw new InvalidOperationException($"Invalid CreatedAt for Employee");
```

---

### パターン 2: オプションフィールド（UpdatedAt / DeletedAt）

```csharp
public sealed class UpdatedAt : PrimitiveValueObject<LocalDateTime?>, IEquatable<UpdatedAt>
{
    // Domain層: LocalDateTime を直接使用
    public static UpdatedAt From(LocalDateTime value) => new(value, true);

    // 未更新状態を表現（IsSet=false）
    public static UpdatedAt Unset() => new(LocalDateTime.MinValue, false);

    // Infrastructure層: DB DateTime を LocalDateTime に変換
    public static UpdatedAt FromDbValue(DateTime value) 
        => new(new LocalDateTime(value), true);

    // Mapper: LocalDateTime → DateTime に変換
    public DateTime ToDbValue() => IsSet && ValueField.HasValue 
        ? ValueField.Value.Value 
        : throw new InvalidOperationException("UpdatedAt is not set.");

    // Repository: null安全に生成（null=Unset()）
    public static bool TryFromDbValue(DateTime? input, out UpdatedAt result)
    {
        if (input == null)  // ← null は Unset に自動変換
        {
            result = Unset();
            return true;
        }

        try
        {
            result = FromDbValue(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            result = null!;
            return false;
        }
    }
}
```

**特徴**:
- ✅ null は自動的に Unset() に変換（NULLABLE DB カラム対応）
- ✅ Domain では IsSet フラグで未設定状態を判定
- ✅ null チェックが不要（型で安全）

**使用例**:
```csharp
// Repository.GetByIdAsync
if (!UpdatedAt.TryFromDbValue(dbModel.UpdatedAt, out var updatedAt))
    throw new InvalidOperationException($"Invalid UpdatedAt");

// Domain層での判定
if (employee.UpdatedAt.HasUpdated)  // IsSet フラグを確認
{
    // 更新済みの処理
}
```

---

## 🔄 データフロー

### 読み込み（DB → Domain）

```
Repository.GetByIdAsync
  ↓
SQL実行 → DbModel（DateTime型）
  ↓
TryFromDbValue(dbModel.UpdatedAt, out var updatedAt)  ← 層間フィルター
  ├─ DB null → Unset() に変換
  ├─ DateTime → LocalDateTime に変換
  └─ out updatedAt（Domain型）
  ↓
Mapper.ToDomainEntity(dbModel, ...)  ← Mapper は変換結果を受け取る
  ↓
Entity（Domain Model）← Domain での null チェック不要
```

### 書き込み（Domain → DB）

```
UpdateAsync(entity)
  ↓
Mapper.ToDbModel(entity)  ← ビジネスフィールドのみ変換
  ├─ entity.UpdatedAt.ToDbValue()  ← LocalDateTime → DateTime
  └─ DbModel生成
  ↓
SetUpdatedAtAudit(dbModel)  ← Repository が監査フィールド設定
  ├─ dbModel.UpdatedAt = Clock.JstNow.Value
  └─ dbModel.UpdatedBy = CurrentUser.EmployeeRowId
  ↓
SQL UPDATE
  ↓
DB（DateTime型）
```

---

## 🛡️ 層間フィルター（Repository での実装）

Repository は「層間フィルター」の役割を果たし、DB の null を Domain の Unset に変換します。

### GetByIdAsync の実装パターン

```csharp
public async Task<Employee?> GetByIdAsync(EmployeeRowId id)
{
    // ... SQL 実行コード ...
    var dbModel = await connection.QueryFirstOrDefaultAsync<EmployeeDbModel>(...);
    
    if (dbModel == null) return null;

    // ============ 層間フィルター ============
    // DB の null を ValueObject の Unset() に変換（Infrastructure層の責務）
    if (!CreatedAt.TryFromDbValue(dbModel.CreatedAt, out var createdAt))
        throw new InvalidOperationException($"Invalid CreatedAt for Employee RowId={id.Value}");

    if (!UpdatedAt.TryFromDbValue(dbModel.UpdatedAt, out var updatedAt))
        throw new InvalidOperationException($"Invalid UpdatedAt for Employee RowId={id.Value}");

    if (!DeletedAt.TryFromDbValue(dbModel.DeletedAt, out var deletedAt))
        throw new InvalidOperationException($"Invalid DeletedAt for Employee RowId={id.Value}");

    // ここまでで dbModel の null はすべて ValueObject の Unset に変換済み
    // Mapper には「Domain対応型」のみが渡される
    return _mapper.ToDomainEntity(dbModel);
}
```

**重要**:
- Repository が TryFromDbValue を実行
- DB null の判定と変換が一箇所に集約される
- Mapper は **単純な型変換のみ**を実行

---

## 🎯 Mapper と Repository の責務分離

### Mapper の責務

**対象**: ビジネスフィールドのみ  
**変換内容**: LocalDateTime ↔ DateTime

```csharp
public EmployeeDbModel ToDbModel(Employee entity)
{
    return new EmployeeDbModel
    {
        // ビジネスフィールドのみ
        RowId = entity.RowId.Value,
        BizDivision = entity.TypeDivision.ToDbValue(),
        HireDate = entity.HireDate.HasValue 
            ? entity.HireDate.Value.Value  // LocalDateTime → DateTime
            : (DateTime?)null,
        
        // ❌ 監査フィールドは設定しない
        // CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy は省略
    };
}
```

### Repository の責務

**対象**: 監査フィールド（CreatedAt, UpdatedAt, DeletedAt, Created/UpdatedBy, DeletedBy）  
**実行場面**: 
- GetByIdAsync: DB null → Unset 変換（層間フィルター）
- UpdateAsync: 監査フィールドの自動設定

```csharp
public async Task UpdateAsync(Employee employee)
{
    var empDbModel = _mapper.ToDbModel(employee);
    
    // Repository が監査フィールドを設定
    SetUpdatedAtAudit(empDbModel);    // ← 保存時刻を記録
    SetUpdatedByAudit(empDbModel);    // ← 操作者を記録
    
    // SQL UPDATE 実行
    await connection.ExecuteAsync(...);
}
```

---

## 📊 パターン比較表

| 項目 | CreatedAt（必須） | UpdatedAt/DeletedAt（オプション） |
|-----|------------------|-----------------------|
| **DB カラム** | NOT NULL | NULLABLE |
| **TryFromDbValue(null)** | 失敗（false） | 成功（Unset） |
| **Domain での null** | なし | IsSet で判定 |
| **別名メソッド** | — | HasUpdated, IsDeleted |
| **使用例** | 常に有効 | 条件付き処理 |

---

## 💡 よくあるエラー

### ❌ 間違い 1: Mapper で DB null を判定

```csharp
// ❌ 間違い
public Employee ToDomainEntity(EmployeeDbModel dbModel)
{
    var updatedAt = dbModel.UpdatedAt == null 
        ? UpdatedAt.Unset() 
        : UpdatedAt.FromDbValue(dbModel.UpdatedAt.Value);
    
    return new Employee(updatedAt: updatedAt, ...);
}
```

**問題**: Mapper が DB 層の詳細を知ってしまう（責務越境）

### ✅ 正しい

```csharp
// ✅ 正しい
public Employee ToDomainEntity(EmployeeDbModel dbModel)
{
    // Repository が既に TryFromDbValue で変換済み
    // Mapper は単純な型変換のみ
    return new Employee(
        updatedAt: new UpdatedAt(dbModel.UpdatedAt_Converted),
        ...);
}
```

---

### ❌ 間違い 2: Domain で null チェック

```csharp
// ❌ 間違い
if (employee.UpdatedAt == null)  // Domain では起こらない
{
    // 処理
}
```

**問題**: Domain モデルでは null が存在しない（型安全性の崩壊）

### ✅ 正しい

```csharp
// ✅ 正しい
if (!employee.UpdatedAt.HasUpdated)  // IsSet フラグで判定
{
    // 未更新の処理
}
```

---

## 🔧 実装チェックリスト

新規 ValueObject に FromDbValue/ToDbValue を実装する際：

- [ ] **FromDbValue(DateTime)**: DB 値 → Domain 型に変換
- [ ] **ToDbValue()**: Domain 型 → DB 値に変換
- [ ] **TryFromDbValue(DateTime?)**: null安全に変換（null → Unset または失敗）
- [ ] **TryFrom(LocalDateTime?)**: Domain 型での null安全変換
- [ ] **Unset()**: オプション型の場合のみ（IsSet=false）
- [ ] **別名プロパティ**: HasUpdated, IsDeleted など（可読性向上）

---

## 📖 参考ドキュメント

- [null厳格性設計ガイド](null厳格性設計ガイド.md)
- [LocalDateTime_タイムゾーン_ガイド.md](LocalDateTime_タイムゾーン_ガイド.md)
- [Mapper_パターンガイド.md](Mapper_パターンガイド.md)
- [Repository_パターンガイド.md](Repository_パターンガイド.md)
