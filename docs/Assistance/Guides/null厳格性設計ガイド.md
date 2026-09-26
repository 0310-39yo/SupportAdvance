# Domain層 null 厳格性設計ガイド
— Option/Maybe パターンの C# 実装

**版:** 1.0  
**作成日:** 2026-08-08  
**対象:** SharedKernel, Domain, Application, Infrastructure 全層

---

## 📋 目次

1. [根本原則](#1-根本原則)
2. [レイヤ別責務](#2-レイヤ別責務)
3. [実装パターン（3つの基本型）](#3-実装パターン3つの基本型)
4. [3段階フロー](#4-3段階フロー)
5. [別名関係と表現統一](#5-別名関係と表現統一)
6. [実装ガイド](#6-実装ガイド)
7. [Mapper での双方向変換](#7-mapper-での双方向変換)
8. [よくあるエラーと修正](#8-よくあるエラーと修正)
9. [Q&A](#9-qa)
10. [参考資料](#10-参考資料)
11. [廃版化ドキュメント](#11-廃版化ドキュメント)

---

## 1. 根本原則

### 1.1 理論的背景：Option<T> パターン

Domain層での null 禁止原則は、関数型プログラミングの **Option<T> パターン**（Haskell/Scala の Maybe 型）に独立して設計されたが、その考え方によく似ている。

#### Haskell での Maybe パターン

```haskell
-- Haskell の Maybe 型
data Maybe a = Just a | Nothing

-- 使用方法
case value of
  Just x -> use(x)          -- 値が存在
  Nothing -> handle_absent  -- 値が存在しない
```

#### SupportAdvance の C# 実装

```csharp
// IsSet フラグで状態を表現
var value = RespondentName.From("太郎");

if (value.IsSet)
    use(value.Value);           -- 値が存在
else
    handle_absent();            -- 値が存在しない（Unset状態）
```

**相違点（C# の制約による）：**
- Haskell/Scala: Nothing は値を持たない
- SupportAdvance: Unset() も「型のデフォルト値を保持」（C# にはJavaの null を代替する言語レベルの「何も持たない状態」がない）

### 1.2 なぜ null は禁止か

#### ❌ null の問題点

| 問題 | 例 | 影響 |
|-----|---|----|
| **予測不可能性** | `string? name = null` の場合、呼び出し側が null チェックを忘れる | NullReferenceException |
| **ドメイン曖昧性** | `null` が「未設定」か「エラー」かが不明確 | ビジネスロジックの不確実性 |
| **型安全性の喪失** | `DateTime? date` は DateTime と同じ型に見える | コンパイラが強制できない |
| **制御フローの複雑化** | null チェックが散在 | コード可読性低下 |

#### ✅ IsSet パターンの利点

| 利点 | 方法 | 効果 |
|-----|------|--------|
| **明示的な状態表現** | `if (value.IsSet)` で状態を判定 | ドメイン意図が明確 |
| **型安全性の確保** | 型システムで null を許さない | Domain層での完全な型安全性 |
| **状態の完全性** | Unset も有効なインスタンス | 値が常に決定される |
| **可読性** | `HasUpdated`, `IsDeleted` という意図的な別名 | ビジネスロジックが自己説明的 |

### 1.3 Unset 状態の本質

#### IsSet フラグの役割

```csharp
public sealed class RespondentName : PrimitiveValueObject<string>
{
    // Domain層での保持:
    // Value: 常に string を保持（null ではない）
    // IsSet: false = 未設定状態を表現
    
    public static RespondentName Unset() 
        => new(string.Empty, false);  // Value = "" (デフォルト), IsSet = false
    
    public static RespondentName From(string value)
        => new(value, true);          // Value = 値, IsSet = true
}
```

#### 監査ValueObjects での例

```csharp
public sealed class UpdatedAt : PrimitiveValueObject<LocalDateTime?>
{
    // Domain層での保持:
    // Value: 常に LocalDateTime を保持（null ではない）
    // IsSet: false = 未更新状態（Mapper により DB では null に変換）
    // IsSet: true = 更新済み状態（Mapper により DB では DateTime 値に変換）
    
    public static UpdatedAt Unset()
        => new(LocalDateTime.MinValue, false);  // Unset状態のセンチネル値
    
    public bool HasUpdated => IsSet;  // IsSet の別名
}
```

**重要な保証:**
- ✅ Domain層: Value は常に型のデフォルト値以上（null なし）
- ✅ DB層: Mapper が IsSet フラグに基づいて、Domain の値を DB の null/値に変換
- ✅ IsSet フラグで「存在/非存在」を表現（Domain層では型安全）

---

## 2. レイヤ別責務（完全な分離）

### 2.1 Domain層：null-free 保証

#### null-free の定義

**null-free = Domain層での null 禁止原則**

Domain層が null を完全に排除すること。
すべての値は型で完全に決定され、IsSet フラグで状態を管理する。

#### 原則

```
Domain層は完全に null-free。
すべての値は型で完全に決定される。
ビジネスロジックは IsSet フラグで状態を判定、null チェック不要。
```

#### 実装例

```csharp
public class UserPreferences : Entity
{
    public UpdatedAt UpdatedAt { get; private set; }
    
    // ❌ 間違い：null チェック
    public bool IsRecent()
    {
        if (UpdatedAt == null)  // ← Domain では起こらない
            return false;
        return UpdatedAt.Value > threshold;
    }
    
    // ✅ 正しい：IsSet で判定
    public bool IsRecent()
    {
        if (!UpdatedAt.HasUpdated)  // HasUpdated は IsSet の別名
            return false;
        return UpdatedAt.Value > threshold;
    }
}
```

#### 保証される安全性
- `entity.UpdatedAt.Value` を安全に呼べる（null-safe）
- ビジネスロジックが null-free のため、可読性が高い
- コードレビューで null チェック漏れが発生しない

### 2.2 Application層：入力境界での変換

#### 原則

```
Application層は Domain への入口。
外部入力（JSON/UI/API）で null を許容し、
TryFrom で自動的に Unset() に変換してから Domain に渡す。
```

#### 実装例

```csharp
public class CreateUserPreferencesUseCase
{
    public async Task<Response> ExecuteAsync(CreateRequest request)
    {
        // 外部入力で null を許容（TryFrom で Unset に自動変換）
        if (!RespondentName.TryFrom(request.Name, out var name))
            throw new ArgumentException("Invalid name format");  // 形式エラーのみ
        
        // name は Set（値あり）または Unset（値なし）の状態
        var preferences = new UserPreferences(name, ...);
        
        // 以降、Domain層では name の IsSet で状態判定
        if (name.IsSet)
            ProcessName(name.Value);
    }
}
```

#### IOptionalValueObject インターフェース

```csharp
public interface IOptionalValueObject<TSelf> where TSelf : class
{
    // null を許容し、Unset に変換して成功を返す
    static abstract bool TryFrom(string? input, out TSelf result);
}
```

### 2.3 Infrastructure層：DB層との変換（層間フィルター）

#### 原則

```
Infrastructure層は「汚い外界（DB）」と「clean な Domain」の境界。
DB の DateTime? を LocalDateTime? に変換し、TryFrom で null を自動変換して、
Domain に渡す前にすべての値を null-free にする。
DB の型（DateTime）は Infrastructure の内側に閉じ込め、値オブジェクトには持ち込まない。
```

#### 3段階フロー

```
DB値（nullable DateTime/DateTime?）
    ↓
Infrastructure（Mapper / Repository）：DateTime? → LocalDateTime? に変換
    ↓
値オブジェクトの TryFrom(LocalDateTime?) 呼び出し
    ├─ 必須フィールド: null → false（例外投げ）
    └─ オプション/監査: null → Unset()（成功）
    ↓
Domain層へ渡す
    → すべての ValueObject が null-free 状態
```

#### 実装例

```csharp
public class UserPreferencesRepository
{
    public async Task<UserPreferences?> GetAsync(UserId userId)
    {
        var dbModel = await _context.UserPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId.Value);
        
        // DbModel が存在しない場合は null を返す（層間の境界）
        if (dbModel == null) return null;
        
        // ============ 層間の境界 ============
        // すべての値を null → Unset に変換（フィルター）
        
        // DB の DateTime? は Infrastructure が LocalDateTime? に変換してから TryFrom に渡す
        // （ToLocalDateTime / ToLocalDateTimeOrNull は Infrastructure の変換ヘルパー）

        // createdAt: DB NOT NULL だが、TryFrom で検証
        if (!CreatedAt.TryFrom(dbModel.CreatedAtDb.ToLocalDateTime(), out var createdAt))
            throw new InvalidOperationException("DB integrity error: CreatedAt is invalid");
        
        // updatedAt: DB NULL OK、null → Unset()
        if (!UpdatedAt.TryFrom(dbModel.UpdatedAtDb.ToLocalDateTimeOrNull(), out var updatedAt))
            throw new InvalidOperationException("Invalid UpdatedAt in DB");
        
        // deletedAt: DB NULL OK、null → Unset()（未削除状態）
        if (!DeletedAt.TryFrom(dbModel.DeletedAtDb.ToLocalDateTimeOrNull(), out var deletedAt))
            throw new InvalidOperationException("Invalid DeletedAt in DB");
        
        // ============ Domain層へ渡す ============
        // すべての ValueObject が null-free 状態
        // 【注意】監査フィールド（createdAt, updatedAt, deletedAt）は検証済みだが、
        // Entity のビジネスロジックでは不要なため、Reconstruct に渡さない設計パターンもある。
        // 監査情報は Repository で検証済みなので、Entity は ビジネスロジック専用に保つ。
        return UserPreferences.Reconstruct(userId, ...);
    }
}
```

### 2.4 DB層：ネイティブな null

#### スコープ限定

【対象】DateTime? のみ（DB ネイティブ型と Domain層 LocalDateTime の境界）  
【対象外】int?, bool?, string? などの値型・参照型（Application層の Input/DTO で別途処理）

#### 原則

```
DB層（スキーマ・DbModel）は DateTime/DateTime? を使用。
ORM マッピングで LocalDateTime に自動変換（逆変換は Mapper が担当）。
```

#### スキーマ例

```sql
CREATE TABLE t_UserPreferences (
    row_id BIGINT PRIMARY KEY,
    created_at DATETIME2(7) NOT NULL,    -- CreatedAt（必須）
    updated_at DATETIME2(7) NULL,        -- UpdatedAt（NULL許容 → Unset）
    deleted_at DATETIME2(7) NULL,        -- DeletedAt（NULL許容 → 未削除）
);
```

#### DbModel例

```csharp
public class UserPreferencesDbModel
{
    public DateTime CreatedAtDb { get; set; }      // DB NOT NULL
    public DateTime? UpdatedAtDb { get; set; }     // DB NULL OK
    public DateTime? DeletedAtDb { get; set; }     // DB NULL OK
}
```

---

## 3. 実装パターン（3つの基本型）

### 3.1 必須ValueObject（入力必須）

#### パターン：CreatedAt

```csharp
public sealed class CreatedAt : PrimitiveValueObject<LocalDateTime>
{
    private CreatedAt(LocalDateTime value) : base(value, true) { }
    
    public static CreatedAt From(LocalDateTime value) => new(value);
    
    // TryFrom: null は失敗（入力必須）
    public static bool TryFrom(LocalDateTime? input, out CreatedAt result)
    {
        result = default!;
        
        if (!input.HasValue)
            return false;  // ← null は無効な入力
        
        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
    
    public LocalDateTime Value => ValueField;
}
```

#### 特徴
- **用途**: システム管理フィールド（作成日時、最終更新者など）
- **DB**: NOT NULL
- **Unset 状態**: なし（常に値を持つ）
- **TryFrom(null)**: false（失敗。DB の NOT NULL 制約違反も、Infrastructure が変換した `null` をここで検出する）
- **DB の型（DateTime）は知らない**: `DateTime` → `LocalDateTime` の変換は Infrastructure（`ToLocalDateTime()`）が行う

### 3.2 オプションValueObject（入力オプション）

#### パターン：RespondentName（IOptionalValueObject 実装）

```csharp
public sealed class RespondentName : PrimitiveValueObject<string>, 
    IOptionalValueObject<RespondentName, string>
{
    private RespondentName(bool isSet) : base(isSet) { }
    private RespondentName(string value, bool isSet) : base(value, isSet) { }
    
    public static RespondentName Unset() => new(false);
    public static RespondentName From(string value) => new(value, true);
    
    // TryFrom: null は Unset（オプション入力）
    public static bool TryFrom(string? input, out RespondentName result)
    {
        if (input is null)
        {
            result = Unset();  // ← null は「未設定」として成功
            return true;
        }
        
        try
        {
            result = From(input);
            return true;
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;
        }
    }
    
    public string? Value => IsSet ? ValueField : null;
}
```

#### 特徴
- **用途**: ユーザー入力フィールド（名前、メールアドレスなど）
- **DB**: NULL OK（入力がない場合）
- **Unset 状態**: あり（IsSet=false で表現）
- **TryFrom(null)**: true（成功、Unset 状態）

### 3.3 監査ValueObjects（オプション型の日時）

#### パターン：UpdatedAt, DeletedAt

値オブジェクトは `LocalDateTime?` だけを受け取る。DB の `DateTime?` は、Infrastructure（Mapper / Repository）が `ToLocalDateTimeOrNull()` で変換してから `TryFrom` に渡す。

```csharp
public sealed class UpdatedAt : PrimitiveValueObject<LocalDateTime?>
{
    private UpdatedAt(LocalDateTime? value, bool isSet) : base(value, isSet) { }
    
    public static UpdatedAt Unset() 
        => new(LocalDateTime.MinValue, false);
    
    public static UpdatedAt From(LocalDateTime value) 
        => new(value, true);
    
    // TryFrom: LocalDateTime? null → Unset（DB の null も、変換後の null としてここで Unset になる）
    public static bool TryFrom(LocalDateTime? input, out UpdatedAt result)
    {
        if (input == null || !input.HasValue)
        {
            result = Unset();  // null → Unset（成功）
            return true;
        }
        
        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;
        }
    }
    
    public LocalDateTime? Value => ValueField;  
    // Domain層での保持：IsSet の値に関わらず LocalDateTime を保持（null ではない）
    // IsSet=false: LocalDateTime.MinValue（Unset状態）→ Mapper で DB null に変換
    // IsSet=true: 実際の LocalDateTime 値 → Mapper で DB DateTime 値に変換
    public bool HasUpdated => IsSet;  // IsSet の別名
}
```

```csharp
public sealed class DeletedAt : PrimitiveValueObject<LocalDateTime?>
{
    private DeletedAt(LocalDateTime? value, bool isSet) : base(value, isSet) { }
    
    public static DeletedAt Unset() 
        => new(LocalDateTime.MinValue, false);
    
    public static DeletedAt From(LocalDateTime value) 
        => new(value, true);
    
    // TryFrom: LocalDateTime? null → Unset（未削除状態）
    public static bool TryFrom(LocalDateTime? input, out DeletedAt result)
    {
        if (input == null || !input.HasValue)
        {
            result = Unset();  // null → Unset（成功）
            return true;
        }
        
        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;
        }
    }
    
    public LocalDateTime? Value => ValueField;  // IsSet=false の場合も ValueField = LocalDateTime.MinValue を保持
    public bool IsDeleted => IsSet;  // IsSet の別名
}
```

#### 特徴
- **用途**: 監査フィールド（最終更新日、削除日）
- **DB**: NULL OK（未更新/未削除の場合）
- **Unset 状態**: あり（IsSet=false で表現、Domain では LocalDateTime.MinValue を保持）
- **DB への変換**: Mapper が IsSet=false を null に、IsSet=true を DateTime に変換
- **TryFrom(null)**: true（成功、Unset 状態に変換。DB の null は Infrastructure が `null` のまま渡す）
- **別名**: HasUpdated（UpdatedAt）, IsDeleted（DeletedAt）
- **重要**: LocalDateTime.MinValue はセンチネル値として予約済み。ビジネスロジックとして使用不可

---

## 4. 3段階フロー

### 概念図

```
┌─────────────────────────────────────────────────────┐
│ Stage 1: DB層（永続化層）                            │
│ - DateTime / DateTime? ネイティブ型                  │
│ - NULL が存在する可能性                              │
└──────────────────────┬────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────┐
│ Stage 2: 変換 + TryFrom（層間フィルター）            │
│ - Infrastructure で DateTime? → LocalDateTime? に変換 │
│ - Mapper / Repository から TryFrom を呼び出し       │
│ - 必須: null → false（例外）                        │
│ - オプション/監査: null → Unset()                   │
└──────────────────────┬────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────┐
│ Stage 3: Domain層（ビジネスロジック）                │
│ - LocalDateTime / LocalDateTime? のみ              │
│ - null なし（IsSet フラグで状態管理）               │
│ - ビジネスロジック内で null チェック不要            │
└─────────────────────────────────────────────────────┘
```

### 流れ（UpdatedAt の例）

```
【Stage 1: DB層】
DB: updated_at = NULL（未更新を示す）
  
【Stage 2: Infrastructure の変換と TryFrom（層間フィルター）】
Repository: UpdatedAt.TryFrom(dbModel.UpdatedAt.ToLocalDateTimeOrNull())   // null のまま渡る
  ↓
null → Unset() に変換: new(LocalDateTime.MinValue, isSet: false)
  
【Stage 3: Domain層】
Domain: updatedAt.HasUpdated = false（未更新状態）
  ↓
ビジネスロジック: if (!entity.UpdatedAt.HasUpdated) { ... }
  （null チェック不要、IsSet フラグで状態判定）
```

**逆フロー（Entity → DB）:**
```
【Domain】
updatedAt.IsSet = false, Value = LocalDateTime.MinValue

【Mapper.ToDbModel（層間変換）】
IsSet=false → (DateTime?)null

【DB】
updated_at = NULL
```

---

## 5. 別名関係と表現統一

### フラグ別名の統一表

| 用途 | フィールド名 | プロパティ名 | 使用例 | ValueObject |
|-----|-----------|----------|-------|-----------|
| 設定状態の判定 | IsSet | IsSet | `if (value.IsSet)` | すべて |
| 更新状態の判定 | IsSet | HasUpdated | `if (entity.UpdatedAt.HasUpdated)` | UpdatedAt |
| 削除状態の判定 | IsSet | IsDeleted | `if (entity.DeletedAt.IsDeleted)` | DeletedAt |

#### 重要な注意

```
- IsSet, HasUpdated, IsDeleted はすべて同じ意図（IsSet フラグ）の別名
- 混在してもOK（むしろビジネス意図を明確にするため）
- 実装時の一貫性よりも「可読性」を重視
```

#### 別名の選択基準

```csharp
// ✅ 一般的な ValueObject
if (respondentName.IsSet) { ... }

// ✅ 更新履歴を表現する
if (entity.UpdatedAt.HasUpdated) { ... }

// ✅ 論理削除を表現する
if (entity.IsDeleted) { return; }

// ✅ 別名の混在も許容（意図が明確なら）
if (entity.CreatedAt.IsSet && entity.UpdatedAt.HasUpdated) { ... }
```

---

## 6. 実装ガイド

### 6.1 新しい ValueObject を実装するとき

#### チェックリスト

**設計段階**
- [ ] ビジネス概念を正確に反映した名前か？（例：RespondentName は「回答者の名前」）
- [ ] ValueObject で表現すべきか（Entity ではなく）を確認
- [ ] Unset 状態が必要か？(入力が必須か必須でないか)
  - YES（必須でない） → IOptionalValueObject を実装
  - NO（必須） → Unset() は不要（常に値を持つ）
- [ ] 入力パターンは？
  - **必須**: CreatedAt パターン（入力必須、null → false）
  - **オプション**: RespondentName パターン（入力オプション、null → Unset）
  - **監査**: UpdatedAt パターン（オプション型の日時。DB の DateTime? は Infrastructure が LocalDateTime? に変換して TryFrom に渡す）

**実装段階**
- [ ] コンストラクタは `private` 
- [ ] フィールドは `readonly`
- [ ] `From()` static メソッド（値を持つ状態）
- [ ] `Unset()` static メソッド（未設定状態、不要な場合は省略）
- [ ] `TryFrom()` メソッド（日時は `TryFrom(LocalDateTime?)`。`DateTime` を受け取るメソッドは作らない）
- [ ] `Value` プロパティ（IsSet に応じた値）
- [ ] `Normalize()` メソッド（入力を正規化、必要に応じて）
- [ ] `Validate()` メソッド（ビジネスルール検証）
- [ ] `GetValueComponents()` メソッド（等価性判定）

**テスト段階**
- [ ] 正常系：正規化・検証が期待通り
- [ ] 異常系：不正な値で例外
- [ ] Unset：未設定状態が正しく表現される
- [ ] IsSet フラグ：IsSet に応じた Value の動作
- [ ] 等価性：同じ値は等価、異なる値は非等価

### 6.2 実装テンプレート

#### 必須ValueObject

```csharp
public sealed class YourValueObject : PrimitiveValueObject<TValue>
{
    private YourValueObject(TValue value) : base(value, true) { }
    
    public static YourValueObject From(TValue value) => new(value);
    
    public static bool TryFrom(TValue? input, out YourValueObject result)
    {
        result = default!;
        
        if (input == null)
            return false;  // ← null は失敗
        
        try
        {
            result = From(input);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
    
    public TValue Value => ValueField;
    
    protected override TValue Normalize(TValue input) => input;
    
    public override void Validate(TValue normalized)
    {
        // ビジネスルール検証
    }
    
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return ValueField;
    }
}
```

#### オプションValueObject

```csharp
public sealed class YourOptionalValueObject : PrimitiveValueObject<TValue>, 
    IOptionalValueObject<YourOptionalValueObject, TValue>
{
    private YourOptionalValueObject(bool isSet) : base(isSet) { }
    private YourOptionalValueObject(TValue value, bool isSet) : base(value, isSet) { }
    
    public static YourOptionalValueObject Unset() => new(false);
    
    public static YourOptionalValueObject From(TValue value) => new(value, true);
    
    public static bool TryFrom(TValue? input, out YourOptionalValueObject result)
    {
        if (input == null)
        {
            result = Unset();  // ← null は Unset に変換
            return true;
        }
        
        try
        {
            result = From(input);
            return true;
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;
        }
    }
    
    public TValue? Value => IsSet ? ValueField : null;
    
    protected override TValue Normalize(TValue input) => input;
    
    public override void Validate(TValue normalized)
    {
        // ビジネスルール検証
    }
    
    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

#### 監査ValueObject（UpdatedAt/DeletedAt パターン）

```csharp
public sealed class YourAuditValueObject : PrimitiveValueObject<LocalDateTime?>
{
    private YourAuditValueObject(LocalDateTime? value, bool isSet) : base(value, isSet) { }
    
    public static YourAuditValueObject Unset() 
        => new(LocalDateTime.MinValue, false);
    
    public static YourAuditValueObject From(LocalDateTime value) 
        => new(value, true);
    
    // TryFrom: LocalDateTime? null → Unset
    // （DB の DateTime? は、Infrastructure が LocalDateTime? に変換してからここに渡す。DateTime を受け取るメソッドは作らない）
    public static bool TryFrom(LocalDateTime? input, out YourAuditValueObject result)
    {
        if (input == null || !input.HasValue)
        {
            result = Unset();
            return true;
        }
        
        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;
        }
    }
    
    public LocalDateTime? Value => ValueField;  // IsSet=false の場合も LocalDateTime.MinValue を保持
    
    protected override LocalDateTime? Normalize(LocalDateTime? input) => input;
    
    public override void Validate(LocalDateTime? normalized) { }
    
    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

---

## 7. Mapper での双方向変換

### 7.1 ToDbModel：Entity → DbModel（層間変換：Domain の null-free → DB の null 許容）

```csharp
public DbModel ToDbModel(Entity entity)
{
    return new DbModel
    {
        // CreatedAt: 常に値がある（必須）
        // Domain: LocalDateTime 値（.Value が DateTime）
        // DB: DateTime 値に変換
        CreatedAtDb = entity.CreatedAt.Value.Value,
        
        // UpdatedAt: IsSet フラグに基づいて null/値を決定（層間フィルター）
        // Domain: IsSet=false で LocalDateTime.MinValue → DB では null
        //         IsSet=true で LocalDateTime 値 → DB では DateTime 値
        UpdatedAtDb = entity.UpdatedAt.HasUpdated
            ? entity.UpdatedAt.Value?.Value
            : null,
        
        // DeletedAt: IsDeleted フラグに基づいて null/値を決定（層間フィルター）
        // Domain: IsSet=false で LocalDateTime.MinValue → DB では null（未削除）
        //         IsSet=true で LocalDateTime 値 → DB では DateTime 値（削除済み）
        DeletedAtDb = entity.DeletedAt.IsDeleted
            ? entity.DeletedAt.Value?.Value
            : null,
    };
}
```

**重要な責務分離:**
- **Domain層**: IsSet フラグで状態管理、常に LocalDateTime を保持（null-free）
- **Mapper**: IsSet フラグに基づいて、Domain の値を DB の null/値に変換
- **DB層**: NULL/値をネイティブに保存

### 7.2 ToDomainEntity：DbModel → Entity（層間変換：DB の null 許容 → Domain の null-free）

```csharp
public Entity ToDomainEntity(DbModel dbModel)
{
    // DateTime? → LocalDateTime? の変換後、TryFrom で DB の null → Unset() に変換（層間フィルター）
    
    // CreatedAt: DB NOT NULL → Domain LocalDateTime
    if (!CreatedAt.TryFrom(dbModel.CreatedAtDb.ToLocalDateTime(), out var createdAt))
        throw new InvalidOperationException("Invalid CreatedAt from DB");
    
    // UpdatedAt: DB null → Unset() with LocalDateTime.MinValue / IsSet=false
    //            DB 値 → From(LocalDateTime) with IsSet=true
    if (!UpdatedAt.TryFrom(dbModel.UpdatedAtDb.ToLocalDateTimeOrNull(), out var updatedAt))
        throw new InvalidOperationException("Invalid UpdatedAt from DB");
    
    // DeletedAt: DB null → Unset() with LocalDateTime.MinValue / IsSet=false（未削除）
    //            DB 値 → From(LocalDateTime) with IsSet=true（削除済み）
    if (!DeletedAt.TryFrom(dbModel.DeletedAtDb.ToLocalDateTimeOrNull(), out var deletedAt))
        throw new InvalidOperationException("Invalid DeletedAt from DB");
    
    // ============ 結果 ============
    // すべての ValueObject が null-free 状態で Domain に渡される
    // Domain層は IsSet フラグで状態判定、null チェック不要
    return Entity.Reconstruct(createdAt, updatedAt, deletedAt, ...);
}
```

**層間フィルターの保証:**
- **DB → Repository**: DB の null/値を読み込む
- **Infrastructure の変換ヘルパー**: `DateTime?` を `LocalDateTime?` に変換（`ToLocalDateTime()` / `ToLocalDateTimeOrNull()`）
- **TryFrom**: null を Unset() に変換（必須型は失敗）
- **Domain ← Repository**: すべて null-free の ValueObject が渡される

### 7.3 重要な責務分離

```
Infrastructure（Mapper / Repository）: DateTime ↔ LocalDateTime の型変換（変換ヘルパー、LocalDateTime.Value）
値オブジェクト（Domain / SharedKernel）: LocalDateTime? の null 処理（TryFrom）。DateTime は知らない
Mapper: Entity ↔ DbModel の完全なマッピング
```

> **移行完了**: 値オブジェクトが `FromDbValue(DateTime)` / `TryFromDbValue(DateTime?)` / `ToDbValue()` を持つ旧形式は、2026-09-26 に全て削除した（[原則完全準拠 実装計画](../Plans/20260926_原則完全準拠_実装計画.md) のフェーズ 4）。変換は `DbDateTimeExtensions`（`SupportAdvance.Infrastructure.Mappers`）。詳細は [FromDbValue_ToDbValue_パターンガイド.md](FromDbValue_ToDbValue_パターンガイド.md)。

---

## 8. よくあるエラーと修正

### 表：間違い → 理由 → 正しい

| 間違い | 理由 | 正しい |
|------|------|--------|
| `if (value != null)` | Domain は null を含まない | `if (value.IsSet)` |
| `value.Value?? fallback` | Value は常に値を保持 | 不要 |
| `new LocalDateTime(dbValue)` を Domain 側で書く | 型変換の責務分離（DB の型は Infrastructure に閉じる） | Infrastructure で `ToLocalDateTimeOrNull()` → `TryFrom()` |
| `entity.UpdatedAt.Value!` | Domain では !（null-forced）不要 | `entity.UpdatedAt.Value`（安全） |
| Application での `if (value == null)` | 型チェック後に処理 | TryFrom で事前に Unset に変換 |

### エラー事例と修正

#### ❌ 間違い：Domain での null チェック

```csharp
public class UserPreferences
{
    public void Update()
    {
        // ❌ Domain層では起こらない（null は来ない）
        if (UpdatedAt == null)
            return;
        
        if (UpdatedAt.Value > threshold)
            DoSomething();
    }
}
```

#### ✅ 正しい：IsSet で判定

```csharp
public class UserPreferences
{
    public void Update()
    {
        // ✅ null ではなく IsSet で判定
        if (!UpdatedAt.HasUpdated)
            return;
        
        if (UpdatedAt.Value > threshold)
            DoSomething();
    }
}
```

#### ❌ 間違い：Mapper での型変換ミス

```csharp
public DbModel ToDbModel(Entity entity)
{
    // ❌ LocalDateTime は DateTime ではない
    return new DbModel
    {
        CreatedAtDb = entity.CreatedAt.Value,  // ← 型が合わない
    };
}
```

#### ✅ 正しい：LocalDateTime.Value で明示的変換

```csharp
public DbModel ToDbModel(Entity entity)
{
    // ✅ LocalDateTime.Value（DateTime）で LocalDateTime → DateTime 変換（Mapper の責務）
    return new DbModel
    {
        CreatedAtDb = entity.CreatedAt.Value.Value,
    };
}
```

#### ❌ 間違い：Repository での null 変換漏れ

```csharp
public async Task<Car?> GetByIdAsync(CarId carId)
{
    var dbModel = await _context.Cars.FindAsync(carId.Value);
    if (dbModel == null) return null;
    
    // ❌ TryFrom を呼ばず、直接コンストラクタに渡す
    var updatedAt = new UpdatedAt(dbModel.UpdatedAtDb);  // ← db値をそのまま使用
    
    return Car.Reconstruct(..., updatedAt, ...);
}
```

#### ✅ 正しい：変換して TryFrom で Unset に変換

```csharp
public async Task<Car?> GetByIdAsync(CarId carId)
{
    var dbModel = await _context.Cars.FindAsync(carId.Value);
    if (dbModel == null) return null;
    
    // ✅ DateTime? を LocalDateTime? に変換し、TryFrom で null を自動的に Unset に変換
    if (!UpdatedAt.TryFrom(dbModel.UpdatedAtDb.ToLocalDateTimeOrNull(), out var updatedAt))
        throw new InvalidOperationException("Invalid UpdatedAt");
    
    return Car.Reconstruct(..., updatedAt, ...);
}
```

#### ❌ 間違い：値オブジェクトに DateTime を持ち込む

```csharp
// ❌ 値オブジェクトが DB の型（DateTime）を知っている
public static bool TryFromDbValue(DateTime? input, out UpdatedAt result) { ... }
public DateTime ToDbValue() => ...;
```

日時の入口は `TryFrom(LocalDateTime?)` のみ。DB の型との変換は Infrastructure が行う。

---

## 9. Q&A

### Q1: Value が常にデフォルト値を持つのはなぜ？

C# には Haskell/Scala の `Nothing` のような「何も持たない状態」が言語レベルでない。代わりに、IsSet フラグで「存在するが無視」という状態を表現している。

### Q2: Option/Maybe パターンとの違いは？

| 項目 | Option/Maybe | SupportAdvance |
|-----|-----------|------------|
| **理論** | 同じ（未設定を型で表現） | 同じ |
| **実装** | Nothing は値なし | Unset() はデフォルト値を保持 |
| **理由** | 言語設計の違い | C# の制約 |

### Q3: Domain で IsSet チェックするのは、null チェック と同じでは？

異なる意図：
- **null チェック**：「防御的プログラミング」（予期しない null に備える）
- **IsSet チェック**：「状態判定」（ビジネス意図を明確にする）

Domain層の IsSet チェックは、「このエンティティが更新されているか？」「このエンティティは削除されているか？」というビジネス状態を判定する。

### Q4: なぜ Unset() は null ではなくデフォルト値を返すのか？

型安全性のため：

```csharp
// ❌ null の場合、Value を呼ぶと NullReferenceException
public LocalDateTime? Value => IsSet ? ValueField : null;

// ✅ デフォルト値の場合、常に LocalDateTime を返す
public LocalDateTime Value => ValueField;
```

Domain層が null を含まないことを**型システムで保証**するため。

### Q5: DB の DateTime? はどう Domain に渡すのか？（旧 `TryFromDbValue` との違い）

| 呼び出す層 | 手順 | null 処理 |
|---------|------|---------|
| **Infrastructure（Mapper / Repository）** | `dbModel.UpdatedAt.ToLocalDateTimeOrNull()` で `DateTime?` → `LocalDateTime?` に変換 | null のまま渡す |
| **値オブジェクト（`TryFrom(LocalDateTime?)`）** | 変換後の値を受け取る。Application 層の入力変換と同じ入口 | null → Unset（必須型は失敗） |

旧形式の `TryFromDbValue(DateTime?)`（値オブジェクトが DB の型を受け取る）は、2026-09-26 に廃止方針とした。移行中は現状のコードに残っているため、新規には使わない。

---

## 10. 参考資料

- [Option パターン（Haskell）](https://wiki.haskell.org/Maybe)
- [Scala Option<T>](https://www.scala-lang.org/api/2.13.0/scala/Option.html)
- [C# Nullable Reference Types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [ValueObject_設計ガイド.md](ValueObject_設計ガイド.md)（セクション 12: レイヤ制約）
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CLAUDE.md - LocalDateTime使用規則](../../../CLAUDE.md)

---

## 11. 廃版化ドキュメント

以下のドキュメントは本ガイドに統合されました。参照不要です：

- ❌ `docs/Assistance/Guides/監査ValueObject_null処理戦略.md`（廃版）
- ❌ `docs/SharedKernel/ValueObjects/Audit/監査ValueObject_null処理詳細設計.md`（廃版）

これら2つのドキュメント内容は、本ガイドの以下のセクションに統合されています：

| 元ドキュメント | 統合先セクション | 内容 |
|-------------|-------------|------|
| 戦略.md | [2. レイヤ別責務](#2-レイヤ別責務) | Domain層の null-free保証、Infrastructure層での変換 |
| 戦略.md | [4. 3段階フロー](#4-3段階フロー) | DB → ValueObject → Domain の変換フロー |
| 詳細設計.md | [3. 実装パターン](#3-実装パターン3つの基本型) | CreatedAt/UpdatedAt/DeletedAt の実装パターン |
| 詳細設計.md | [7. Mapper での双方向変換](#7-mapper-での双方向変換) | Entity ↔ DbModel マッピング |
| 両文書 | [8. よくあるエラーと修正](#8-よくあるエラーと修正) | 実装時の一般的なミスと修正方法 |

**今後の参照:**
本ガイド `null厳格性設計ガイド.md` を唯一の真実のドキュメント（SSOT）として使用してください。

---

## 更新履歴

| 版 | 日付 | 内容 |
|---|------|------|
| 1.2 | 2026-09-26 | 日時の DB 変換を値オブジェクトから Infrastructure に移す方針に合わせて改訂。`TryFromDbValue(DateTime?)` / `ToDbValue()` の記述を、`ToLocalDateTimeOrNull()` + `TryFrom(LocalDateTime?)` の形に置き換え。移行中の注意を追加 |
| 1.1 | 2026-09-01 | Mapper での層間変換を明確化。Domain の null-free ← → DB の null 許容の変換フローを詳細説明。IsSet フラグと LocalDateTime.MinValue の役割を統一的に記述。Unset 状態の本質（セクション 1.3）と 3段階フロー（セクション 4）を改善。 |
| 1.0 | 2026-08-08 | 初版：Option/Maybe パターン理論、3段階フロー、レイヤ別責務の統一的記述。既存2つのドキュメントを統合。 |

