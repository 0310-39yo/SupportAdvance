# 技術仕様書 — Employee Context ValueObject

**プロジェクト:** SupportAdvance  
**コンテキスト:** Employee（従業員）  
**実装フェーズ:** Phase 1 - ValueObject  
**対象範囲:** 監査情報 ValueObject（CreatedBy/UpdatedBy/DeletedBy）、識別子系 ValueObject  
**版:** 1.0 / 2026-08-10

---

## 1. 位置づけ

Employee Context の ValueObject は、ドメイン層の基盤を形成する型安全な値オブジェクトである。

本 ValueObject 群の目的は以下に集約される。

- **型安全性の確保** — 従業員ID・監査情報を型レベルで表現し、無効な値の混同を防ぐ
- **null厳格性の実現** — DB の NULL/MaxValue を Domain の Unset() に変換し、ドメイン層に null が混入することを防ぐ
- **ビジネスルール検証** — 値の範囲・形式をコンストラクタで検証し、無効な状態の発生を予防
- **値の不変性** — オブジェクト生成後、内部状態は変更不可。Entity の構成要素として安全に使用可能

> **📌 原則**  
> - **ValueObject の定義**: 値で同一性を判定し、ビジネス上意味を持つ複合値をカプセル化したオブジェクト
> - **null厳格性**: Domain層に null が存在しない。未設定状態は IsSet フラグで表現（CLAUDE.md「null厳格性設計ガイド」参照）
> - **プリミティブ型の拒否**: 従業員ID（long）などをそのまま使用せず、ValueObject でラップ
> - **Layer制約**: ValueObject は SharedKernel/Domain層のみ所属

---

## 2. ValueObject グループの分類

### 2.1 監査情報 ValueObject（Phase 1 最優先）

作成・更新・削除の責任者と日時を追跡。ドメイン層の重要な監査情報。

| ValueObject | 型 | IsSet対応 | 説明 |
|-------------|-----|----------|------|
| `CreatedAt` | LocalDateTime | 必須（IsSet=true のみ） | 作成日時（NULL なし） |
| `CreatedBy` | long（EmployeeRowId） | 必須（IsSet=true のみ） | 作成者ID（NULL なし） |
| `UpdatedAt` | LocalDateTime | オプション（Unset対応） | 更新日時（NULL = 未更新） |
| `UpdatedBy` | long | オプション（Unset対応） | 更新者ID（NULL = 未更新） |
| `DeletedAt` | LocalDateTime | オプション（Unset対応） | 削除日時（NULL = 存続） |
| `DeletedBy` | long | オプション（Unset対応） | 削除者ID（NULL = 存続） |

---

## 3. メンバー仕様

### 3.1 ファクトリメソッド（全 ValueObject 共通）

#### From() — 正規値からの生成（必須値パターン）

必須の ValueObject（IsSet=true のみ）で使用。null は失敗を throw。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static {ValueObjectType} From({InputType} value)` |
| 戻り値 | 検証済みの ValueObject インスタンス |
| 例外 | `ArgumentException`, `ArgumentOutOfRangeException`（検証失敗時） |
| 用途 | Entity 構築時、値が確実に存在する場合 |

**設計判断**

- null を受け入れない（null は呼び出し元で処理）
- 検証失敗時は例外を throw
- Domain層では From を使用（Application層は TryFrom を使用）

---

#### TryFrom() — null 安全版の生成

オプション ValueObject で使用。null は Unset に変換して成功を返す。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom({InputType}? input, out {ValueObjectType} result)` |
| 戻り値 | 成功時 true、失敗時 false（例外なし） |
| result パラメータ | 成功時=検証済みインスタンス、失敗時=Unset インスタンス |
| 例外 | 投げない（Try パターン） |
| 用途 | Application層の入力処理、DB→Entity変換時に使用 |

**設計判断**

- null 入力時は true + Unset（null を「未設定」として扱う）
- 検証失敗時は false + Unset（例外ではなく bool で結果を示す）
- 例外を投げない

---

#### TryFromDbValue() — DB値からの変換

DB から読み込んだ NULL/DateTime.MaxValue を Domain の Unset に自動変換。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFromDbValue({DbType}? input, out {ValueObjectType} result)` |
| 戻り値 | 成功時 true、失敗時 false |
| result パラメータ | 検証済みインスタンス（失敗時は Unset） |
| 例外 | 投げない |
| 用途 | Mapper の DB→Entity 変換時に使用 |

**設計判断**

- DB の NULL → Unset() に自動変換
- DateTime.MaxValue（9999-12-31）も Unset と同等に扱う
- Repository で呼び出される（Infrastructure層の責務）

---

### 3.2 IsSet プロパティ（IOptionalValueObject 実装時）

オプション ValueObject における未設定状態を表現。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public bool IsSet { get; }` |
| 戻り値 | 設定済み：true、未設定：false |
| 用途 | Entity のビジネスロジックで状態判定 |

**設計判断**

- 読み取り専用（setter なし）
- null チェック不要（IsSet フラグで判定）

---

### 3.3 Value プロパティ（必須 ValueObject）

内部値を取得するプロパティ。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public {InnerType} Value { get; }` |
| 戻り値 | ラップされた値 |
| 用途 | Mapper で Entity→DbModel 変換時に使用 |

**設計判断**

- setter なし（不変性）
- Value.Value で 2 段階アクセス（LocalDateTime → DateTime など）

---

### 3.4 Unset() — 未設定インスタンスの生成

IsSet=false のインスタンスを生成。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static {ValueObjectType} Unset()` |
| 戻り値 | IsSet=false のインスタンス |
| 用途 | Entity 再構築時、未設定値の明示的表現 |

**設計判断**

- 値はデフォルト型値
- IsSet=false で「未設定」を表現

---

## 4. 設計制約・禁止事項

### 4.1 レイヤ制約

| ❌ 禁止 | ✅ 許可 |
|--------|--------|
| ❌ Application層の DTO/UseCase への参照 | ✅ SharedKernel（基底クラス） |
| ❌ `DateTime.Now`/`UtcNow` | ✅ LocalDateTime（IClock 経由） |
| ❌ DB アクセス | ✅ 値の検証のみ |

### 4.2 実装上の禁止事項

- **❌ public setter の禁止** — 不変性の維持
- **❌ null チェック（Domain層）** — IsSet フラグで状態管理
- **❌ 複雑な計算ロジック** — 値の表現と検証のみ
- **❌ 副作用的な操作** — ログ出力、DB アクセスは禁止

---

## 5. 実装ガイドライン

### 5.1 実装義務

- **等価性実装必須** — Equals(), GetHashCode(), ==, != を override
- **IsSet フラグ管理** — オプション ValueObject は IsSet で状態管理
- **Unset() 提供** — オプション ValueObject には static Unset() を用意
- **Validate() 実装** — 値の範囲・形式チェック
- **ToString() 実装** — IsSet=false 時は "Unset"

### 5.2 設計パターン例

#### 必須 ValueObject（CreatedBy パターン）

```csharp
public sealed class CreatedBy : ValueObject, IEquatable<CreatedBy>
{
    public long Value { get; }
    public bool IsSet => true;  // 常に true
    
    public static CreatedBy From(long value)
    {
        if (value <= 0) throw new ArgumentException("CreatedBy must be positive.");
        return new(value);
    }
    
    public static bool TryFrom(long? input, out CreatedBy result)
    {
        if (!input.HasValue) 
        {
            result = null!;
            return false;
        }
        try 
        { 
            result = From(input.Value); 
            return true; 
        }
        catch 
        { 
            result = null!;
            return false; 
        }
    }
    
    public override bool Equals(object? obj) => Equals(obj as CreatedBy);
    public bool Equals(CreatedBy? other) => other != null && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
}
```

#### オプション ValueObject（UpdatedBy パターン）

```csharp
public sealed class UpdatedBy : ValueObject, IEquatable<UpdatedBy>
{
    public long Value { get; }
    public bool IsSet { get; }
    
    public static UpdatedBy Unset()
        => new(default, false);
    
    public static UpdatedBy From(long value)
    {
        if (value <= 0) throw new ArgumentException("UpdatedBy must be positive.");
        return new(value, true);
    }
    
    public static bool TryFrom(long? input, out UpdatedBy result)
    {
        if (!input.HasValue)
        {
            result = Unset();  // null を Unset に変換
            return true;
        }
        try 
        { 
            result = From(input.Value); 
            return true; 
        }
        catch 
        { 
            result = Unset();
            return false;
        }
    }
    
    public static bool TryFromDbValue(long? input, out UpdatedBy result)
    {
        if (!input.HasValue)
        {
            result = Unset();  // DB NULL → Unset
            return true;
        }
        try 
        { 
            result = From(input.Value); 
            return true; 
        }
        catch 
        { 
            result = Unset();
            return false;
        }
    }
    
    public override bool Equals(object? obj) => Equals(obj as UpdatedBy);
    public bool Equals(UpdatedBy? other) 
        => other != null && IsSet == other.IsSet && Value == other.Value;
    public override int GetHashCode() => HashCode.Combine(IsSet, Value);
    public override string ToString() => IsSet ? Value.ToString() : "Unset";
}
```

---

## 6. 版管理

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-08-10 | 初版作成。監査情報 ValueObject（CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/DeletedAt/DeletedBy） |
