# EmployeeRowId - 詳細設計書

**プロジェクト:** SupportAdvance  
**レイヤ:** SharedKernel（基盤層）  
**種別:** ValueObject 詳細設計  
**依拠技術仕様書:** EmployeeRowId技術仕様書 v1.0  
**版:** 1.0 / 2026-08-09

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `EmployeeRowId` |
| 種別 | `sealed class` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects.Identifiers` |
| 実装インターフェース | `IEquatable<EmployeeRowId>` |
| 継承元 | `PrimitiveValueObject<long>`（単純スカラ型 ValueObject） |
| 配置レイヤ | SharedKernel（全層から参照される基盤型） |

### 1.2 責務

- **DB行ID の管理**: `t_employees.row_id` の値を型安全に保持
- **値の検証**: 1 以上の正の値か検証
- **表示形式の提供**: 数値文字列への変換
- **DB値との双方向変換**: `long` から EmployeeRowId を生成、逆変換
- **型安全な等価性判定**: ValueObject として型システムレベルで等価性を保証

### 1.3 協調クラス

```
EmployeeRowId  ──uses──▶  PrimitiveValueObject<long>
                              └─ Validate
                              └─ Value

EmployeeRowId  ──implements──▶  IEquatable<EmployeeRowId>
                                  └─ Equals(EmployeeRowId?)
                                  
Employee Entity  ──uses──▶  EmployeeRowId
                              └─ DB行ID管理
```

---

## 2. プロパティ設計

### 2.1 定数

| 定数 | 値 | 用途 |
|-----|-----|------|
| `MinValue` | `1L` | 最小有効値 |

### 2.2 プロパティ（継承元から）

#### Value: long

| 項目 | 内容 |
|------|------|
| 型 | `long` |
| アクセス | `public get` |
| 実装 | `PrimitiveValueObject<long>` から継承 |
| 用途 | DB行ID値を取得 |
| 設計判断 | 読み取り専用（不変性を確保）。 |

---

## 3. メソッド設計

### 3.1 生成メソッド

#### From(long value): EmployeeRowId

```csharp
public static EmployeeRowId From(long value) => new(value);
```

| 項目 | 内容 |
|------|------|
| パラメータ | `value`: DB行ID |
| 戻り値 | `EmployeeRowId` |
| 例外 | `ArgumentOutOfRangeException`: 0 以下の値 |
| 処理フロー | 1. Validate を実行<br/>2. new で生成 |
| 用途 | Entity 生成時、確定的な生成 |
| 設計判断 | `long` 値が既に存在する場合に使用。 |

#### TryFrom(long value, out EmployeeRowId result): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `value`: DB行ID |
| 戻り値 | `bool`: 成功時 true、失敗時 false |
| 処理フロー | 1. From を呼び出し<br/>2. ArgumentOutOfRangeException をキャッチ → false |
| 用途 | Application層での安全な生成 |
| 設計判断 | 検証エラーを例外ではなく false で返す。 |

#### TryFromDbValue(long value, out EmployeeRowId result): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `value`: DB の row_id 値 |
| 戻り値 | `bool`: 成功時 true、失敗時 false |
| 処理フロー | 1. From を呼び出し<br/>2. ArgumentOutOfRangeException をキャッチ → false |
| 用途 | Infrastructure層で DbModel → Entity 変換 |
| 設計判断 | DB値は既に存在する（検証済み）と仮定。念のため検証。 |

### 3.2 検証メソッド

#### Validate(long normalized): void (protected override)

```csharp
protected override void Validate(long normalized)
{
    if (normalized < MinValue)
    {
        throw new ArgumentOutOfRangeException(
            nameof(normalized),
            normalized,
            $"EmployeeRowId must be {MinValue} or higher.");
    }
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | `normalized`: 検証対象の値 |
| 戻り値 | `void` |
| 例外 | `ArgumentOutOfRangeException`: 0 以下の値 |
| 処理フロー | 1. 値が MinValue 以上か確認<br/>2. 範囲外なら ArgumentOutOfRangeException throw |
| 用途 | PrimitiveValueObject のコンストラクタから自動実行 |
| 設計判断 | 単一責任原則。値の妥当性チェックのみ。 |

### 3.3 表示メソッド

#### ToString(): string (override)

```csharp
public override string ToString() => Value.ToString();
```

| 項目 | 内容 |
|------|------|
| 戻り値 | `string`: 数値文字列（例："12345"） |
| 実装 | Value を文字列化 |
| 用途 | ログ出力、UI表示 |
| 設計判断 | 数値形式で出力。 |

### 3.4 等価性メソッド

#### Equals(object? obj): bool (override)

```csharp
public override bool Equals(object? obj) => Equals(obj as EmployeeRowId);
```

#### Equals(EmployeeRowId? other): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `EmployeeRowId? other` |
| 戻り値 | `bool` |
| 処理フロー | 1. other が null → false<br/>2. 自己参照 → true<br/>3. Value を比較 |
| 用途 | ValueObject の等価性判定 |

#### GetHashCode(): int (override)

```csharp
public override int GetHashCode() => Value.GetHashCode();
```

| 項目 | 内容 |
|------|------|
| 戻り値 | `int` |
| 実装 | Value のハッシュコード |
| 用途 | ディクショナリ・ハッシュセット |

---

## 4. コンストラクタ

### 4.1 プライベートコンストラクタ

```csharp
private EmployeeRowId(long value) : base(value, true)
{
}
```

- private 修飾子で外部の `new` を禁止
- ファクトリメソッド（From等）経由のみで生成
- 検証済みの値のみを受け入れ
- `base(value, true)` で PrimitiveValueObject を初期化（isSet = true）

---

## 5. レイヤ制約確認

| 項目 | 確認 |
|------|------|
| Domain層への依存なし | ✓ （独立） |
| Application層への依存なし | ✓ （独立） |
| Infrastructure層への依存なし | ✓ （独立） |
| Presentation層への依存なし | ✓ （独立） |
| SharedKernel のみ参照 | ✓ （PrimitiveValueObject のみ） |

---

## 6. 実装上の注意点

### 6.1 単純スカラ型（PrimitiveValueObject）

PrimitiveValueObject<long> は `long` 値を直接ラップする最もシンプルな ValueObject です。

```csharp
// ✅ 最小実装
public sealed class EmployeeRowId : PrimitiveValueObject<long>
{
    public const long MinValue = 1L;
    
    private EmployeeRowId(long value) : base(value, true) { }
    
    public static EmployeeRowId From(long value) => new(value);
    
    protected override void Validate(long normalized)
    {
        if (normalized < MinValue)
            throw new ArgumentOutOfRangeException(...);
    }
}
```

### 6.2 DB値との対応

EmployeeRowId の値は常に DB の `t_employees.row_id` の値と同じであることが保証されます。

```csharp
// DB: t_employees.row_id = 12345
var rowId = EmployeeRowId.From(12345L);  // Value = 12345L
// 一致 ✓
```

### 6.3 null チェック不要

Domain層では `EmployeeRowId` は常に有効な値を保持します。null は存在しません。

```csharp
// ❌ 不要
if (rowId == null) { ... }

// ✅ 正しい
// 常に有効な値として扱う
```

---

## 7. 実装手順

1. **クラス定義**: `sealed class EmployeeRowId : PrimitiveValueObject<long>`
2. **定数定義**: `MinValue = 1L`
3. **プライベートコンストラクタ**: `private EmployeeRowId(long value)`
4. **ファクトリメソッド実装**:
   - `From(long value): EmployeeRowId`
   - `TryFrom(long value, out result): bool`
   - `TryFromDbValue(long value, out result): bool`
5. **検証メソッド実装**: `Validate(long normalized): void`
6. **IEquatable<EmployeeRowId> 実装**:
   - `Equals(EmployeeRowId? other): bool`
   - `GetHashCode(): int`
7. **ToString オーバーライド**

---

## 参考資料

- 技術仕様書: `EmployeeRowId_技術仕様書.md`
- 単体テスト仕様: `EmployeeRowId_単体テスト仕様書.md`
- PrimitiveValueObject: `../../Abstractions/PrimitiveValueObject.cs`
