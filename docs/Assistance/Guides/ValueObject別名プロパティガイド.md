# ValueObject 別名プロパティガイド

**バージョン**: 1.0  
**最終更新**: 2026-09-12  
**適用範囲**: Domain 層 ValueObject  
**重要度**: MEDIUM

---

## 📋 概要

ValueObject の **別名プロパティ（Alias Properties）** は、IsSet フラグをビジネス意味のある名前で公開するパターンです。

**目的**:
- Domain 層のコード可読性向上
- ビジネス文脈に合わせた直感的な命名
- null チェック（Domain では不可）の代わりに状態判定

---

## 🎯 なぜ必要か

### 問題

```csharp
// ❌ 低い可読性
if (employee.UpdatedAt.IsSet)  // IsSet が何を意味するか不明確
{
    // 更新済みの処理
}
```

### 解決

```csharp
// ✅ 高い可読性
if (employee.UpdatedAt.HasUpdated)  // 「更新済みか」が明確
{
    // 更新済みの処理
}
```

---

## 📌 実装パターン

### パターン 1: HasUpdated（更新状態）

**用途**: UpdatedAt / UpdatedBy でオプション値の「更新済み状態」を判定

```csharp
public sealed class UpdatedAt : PrimitiveValueObject<LocalDateTime?>
{
    // IsSet フラグをビジネス文脈に合わせて別名化
    /// <summary>
    /// 更新済み状態を判定する（IsSet の別名）
    /// 【責務】更新済みか未更新かを判定する
    /// </summary>
    /// <returns>更新済みの場合は true、未更新の場合は false</returns>
    public bool HasUpdated => IsSet;
}

// 使用例
if (employee.UpdatedAt.HasUpdated)
{
    Console.WriteLine($"Last updated: {employee.UpdatedAt.Value}");
}
else
{
    Console.WriteLine("Not yet updated");
}
```

### パターン 2: IsDeleted（削除状態）

**用途**: DeletedAt / DeletedBy でオプション値の「削除済み状態」を判定

```csharp
public sealed class DeletedAt : PrimitiveValueObject<LocalDateTime?>
{
    // IsSet フラグをビジネス文脈に合わせて別名化
    /// <summary>
    /// 削除済み状態を判定する（IsSet の別名）
    /// 【責務】削除済みか未削除かを判定する
    /// </summary>
    /// <returns>削除済みの場合は true、未削除の場合は false</returns>
    public bool IsDeleted => IsSet;
}

// 使用例
if (employee.DeletedAt.IsDeleted)
{
    Console.WriteLine($"Deleted at: {employee.DeletedAt.Value}");
}
else
{
    Console.WriteLine("Still active");
}
```

---

## 🔍 別名プロパティの種類

### 標準的な別名

| ValueObject | IsSet フラグ | 別名プロパティ | 意味 |
|---|---|---|---|
| **UpdatedAt** | 更新済み判定 | HasUpdated | 更新済みか |
| **UpdatedBy** | 更新者設定判定 | HasUpdated | 更新者が設定されたか |
| **DeletedAt** | 削除済み判定 | IsDeleted | 削除済みか |
| **DeletedBy** | 削除者設定判定 | IsDeleted | 削除者が設定されたか |

### 命名規則

**オプション値（Unset 可能）の場合**:

| IsSet フラグが表す状態 | 別名プロパティ | 例文 |
|---|---|---|
| 「更新された」 | **Has + 過去分詞** | HasUpdated, HasRetired |
| 「削除された」 | **Is + 過去分詞** | IsDeleted, IsRetired |
| 「設定された」 | **Has + 過去分詞** | HasValue, HasAssignment |

---

## 💡 実装ガイドライン

### 1. 別名プロパティは単純な Alias

```csharp
// ✅ 正しい（単純な別名）
public bool HasUpdated => IsSet;

// ❌ 間違い（ロジックを含む）
public bool HasUpdated
{
    get
    {
        return IsSet && ValueField.HasValue && ValueField.Value > DateTime.Now;
        // 別名はロジックなし、単純な転送のみ
    }
}
```

### 2. XML ドキュメント（XMLDoc）を必須に

```csharp
/// <summary>
/// 更新済み状態を判定する（IsSet の別名）
/// 【責務】更新済みか未更新かを判定する
/// </summary>
/// <returns>更新済みの場合は true、未更新の場合は false</returns>
public bool HasUpdated => IsSet;
```

**メリット**:
- IDE の IntelliSense で別名の意味が表示される
- ドキュメント生成ツール（Doxygen など）で自動抽出
- 新規実装者が迷わない

### 3. 別名は IsSet フラグのみ

```csharp
// ✅ 正しい（IsSet のみを別名化）
public bool HasUpdated => IsSet;
public bool IsDeleted => IsSet;

// ❌ 間違い（他のプロパティは別名化しない）
public LocalDateTime? UpdatedAtValue => ValueField;  // Value プロパティで十分
```

---

## 🎨 使用シーン

### シーン 1: Domain Entity での条件分岐

```csharp
public class Employee : AggregateRoot<EmployeeId>
{
    public UpdatedAt UpdatedAt { get; private set; }
    public DeletedAt DeletedAt { get; private set; }

    public void PrintStatus()
    {
        // 別名プロパティで可読性が高い
        if (UpdatedAt.HasUpdated)
        {
            Console.WriteLine($"Updated: {UpdatedAt.Value}");
        }

        if (DeletedAt.IsDeleted)
        {
            Console.WriteLine($"Deleted: {DeletedAt.Value}");
        }
    }
}
```

### シーン 2: Application Service での検証

```csharp
public class UpdateEmployeeUseCase
{
    public async Task Execute(UpdateEmployeeRequest request)
    {
        var employee = await _repository.GetByIdAsync(id);

        // 別名プロパティで直感的な判定
        if (employee.DeletedAt.IsDeleted)
        {
            throw new InvalidOperationException("Cannot update a deleted employee");
        }

        // 更新処理
    }
}
```

### シーン 3: DTO 変換

```csharp
public class EmployeeDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsUpdated { get; set; }
    public bool IsDeleted { get; set; }
}

public EmployeeDto ToDto(Employee entity)
{
    return new EmployeeDto
    {
        Id = entity.Id.Value,
        Name = entity.Name,
        IsUpdated = entity.UpdatedAt.HasUpdated,  // 別名プロパティ使用
        IsDeleted = entity.DeletedAt.IsDeleted    // 別名プロパティ使用
    };
}
```

---

## 📊 別名プロパティの効果

### コード比較

**別名プロパティなし**:
```csharp
// ❌ IsSet の意味が不明確
if (employee.UpdatedAt.IsSet)
{
    var lastUpdated = employee.UpdatedAt.Value;
}

if (employee.DeletedAt.IsSet)
{
    var deletedDate = employee.DeletedAt.Value;
}
```

**別名プロパティあり**:
```csharp
// ✅ ビジネス意味が明確
if (employee.UpdatedAt.HasUpdated)
{
    var lastUpdated = employee.UpdatedAt.Value;
}

if (employee.DeletedAt.IsDeleted)
{
    var deletedDate = employee.DeletedAt.Value;
}
```

### 可読性の向上

| 項目 | 効果 |
|---|---|
| **コードの自己説明性** | IsSet より HasUpdated/IsDeleted の方が意図が明確 |
| **IDE サポート** | XMLDoc により IntelliSense で意味を表示 |
| **保守性** | 新規実装者が迷わない |
| **テスト可能性** | テストコードもビジネス用語で記述可能 |

---

## ✅ 実装チェックリスト

新規 ValueObject に別名プロパティを追加する際：

- [ ] **別名プロパティを定義**: IsSet フラグに対応する別名を追加
- [ ] **XMLDoc を必須**: 別名の意味を明確に記述
- [ ] **単純な Alias のみ**: ロジックは含めない
- [ ] **命名規則に従う**: HasUpdated / IsDeleted などの標準命名
- [ ] **テスト可能性**: テストコードでも別名プロパティを使用
- [ ] **ドキュメント更新**: 新規ガイドラインにクエリして記述

---

## 💥 よくあるエラー

### ❌ エラー 1: 別名にロジックを含める

```csharp
public bool HasUpdated
{
    get
    {
        // ❌ 別名なのに複雑なロジック
        return IsSet && ValueField.HasValue && ValueField.Value.DayOfWeek != DayOfWeek.Sunday;
    }
}
```

**問題**: ValueObject の単一責務に違反  
**正しい**: 
```csharp
public bool HasUpdated => IsSet;  // 単純な別名のみ
```

### ❌ エラー 2: 別名を IsSet 以外に使用

```csharp
public bool UpdatedValue => ValueField.HasValue;  // ❌ ValueField の判定
```

**問題**: IsSet と重複、別名の役割が不明確  
**正しい**: 
```csharp
public bool HasUpdated => IsSet;  // IsSet フラグのみを別名化
```

### ❌ エラー 3: XMLDoc がない

```csharp
public bool HasUpdated => IsSet;  // ❌ コメントなし

// IDE の IntelliSense で意味が表示されない
```

**正しい**:
```csharp
/// <summary>更新済み状態を判定する（IsSet の別名）</summary>
public bool HasUpdated => IsSet;
```

---

## 📖 参考ドキュメント

- [null厳格性設計ガイド.md](null厳格性設計ガイド.md) — IsSet フラグの設計
- [ValueObject_設計ガイド.md](ValueObject_設計ガイド.md) — ValueObject の基本設計
- [IValidateWithClock_パターンガイド.md](IValidateWithClock_パターンガイド.md) — ビジネスロジック検証

---

## 📝 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-09-12 | Claude Code | 初版作成。別名プロパティの設計原則、実装パターン、使用シーン、可読性向上の効果 |
