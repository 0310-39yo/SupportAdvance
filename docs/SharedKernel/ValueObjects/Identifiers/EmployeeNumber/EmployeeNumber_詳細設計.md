# EmployeeNumber - 詳細設計書

**プロジェクト:** SupportAdvance  
**レイヤ:** SharedKernel（基盤層）  
**種別:** ValueObject 詳細設計  
**依拠技術仕様書:** EmployeeNumber技術仕様書 v1.0  
**版:** 1.0 / 2026-08-09

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `EmployeeNumber` |
| 種別 | `sealed class` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects.Identifiers` |
| 実装インターフェース | `IEquatable<EmployeeNumber>` |
| 継承元 | `PrimitiveValueObject<int>` |
| 配置レイヤ | SharedKernel（全層から参照される基盤型） |

### 1.2 責務

- **従業員番号の管理**: 1001～8499 の範囲内の int 値を保持
- **値の検証**: 指定された値が有効な範囲であることを検証（1000は予約）
- **表示形式の提供**: 左0埋めで5桁の文字列表現を提供（ToString）
- **DB値との双方向変換**: int 値から EmployeeNumber を生成、逆変換でDB保存値を取得
- **型安全な等価性判定**: ValueObject として型システムレベルで等価性を保証

### 1.3 協調クラス

```
EmployeeNumber  ──inherits──▶  PrimitiveValueObject<int>
                                  └─ Validate(int)

EmployeeNumber  ──implements──▶  IEquatable<EmployeeNumber>
                                  └─ Equals(EmployeeNumber?)

Employee Entity  ──uses──▶  EmployeeNumber
                              └─ ValidateNumberRange (Entity責務)
```

---

## 2. プロパティ設計

### 2.1 プロパティ（public property）

#### Value: int

| 項目 | 内容 |
|------|------|
| 型 | `int` |
| アクセス | `public get` |
| 実装 | `=> ValueField` （PrimitiveValueObject<int> の protected readonly フィールド） |
| 用途 | 従業員番号の int 値を取得、DB保存時の値として使用 |
| 範囲 | 1001～8499 |
| 設計判断 | 読み取り専用（不変性を確保）。ValueField への直接参照で効率化。 |

---

## 3. メソッド設計

### 3.1 生成メソッド

#### From(int value): EmployeeNumber

```csharp
public static EmployeeNumber From(int value) => new(value);
```

| 項目 | 内容 |
|------|------|
| パラメータ | `int value`: 従業員番号（1001～8499） |
| 戻り値 | `EmployeeNumber`（IsSet=true） |
| 例外 | `ArgumentOutOfRangeException`: 無効な値（1000, 0以下, 9000以上など） |
| 処理フロー | 1. int を受け取る<br/>2. コンストラクタで Validate を実行<br/>3. 例外なら ArgumentOutOfRangeException を throw |
| 用途 | int 値から確定的に EmployeeNumber を生成（検証あり） |
| 設計判断 | 入力値が確定している場合に使用。Domain層での明示的な生成用。 |

#### TryFrom(int? input, out EmployeeNumber result): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `int? input`: 従業員番号（null 許容） |
| 戻り値 | `bool`: 成功時 true、失敗時 false |
| out パラメータ | `EmployeeNumber result`: 生成されたインスタンス |
| 処理フロー | 1. input が null または HasValue=false → false を返す<br/>2. From(value) を呼び出す<br/>3. ArgumentOutOfRangeException をキャッチ → false を返す<br/>4. 成功時 true、result に値を設定 |
| 用途 | Application層の入力（DTO）から null安全に生成 |
| 設計判断 | null 入力を許容し false で返す（例外ではなくフロー制御）。 |

#### FromDbValue(int value): EmployeeNumber

```csharp
public static EmployeeNumber FromDbValue(int value) => new(value);
```

| 項目 | 内容 |
|------|------|
| パラメータ | `int value`: DB読み込み値 |
| 戻り値 | `EmployeeNumber`（IsSet=true） |
| 例外 | `ArgumentOutOfRangeException`: 無効な値 |
| 処理フロー | 1. int を受け取る<br/>2. コンストラクタで Validate 実行 |
| 用途 | Infrastructure層で DbModel から Entity に変換時 |
| 設計判断 | From と同じ実装（DB値は int ネイティブ型そのもの） |

#### TryFromDbValue(int? input, out EmployeeNumber result): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `int? input`: DB読み込み値（null 許容） |
| 戻り値 | `bool`: 成功時 true、失敗時 false |
| 処理フロー | 1. input が null または HasValue=false → false を返す<br/>2. FromDbValue を呼び出す<br/>3. ArgumentOutOfRangeException をキャッチ → false を返す |
| 用途 | Infrastructure層で nullable カラム対応 |
| 設計判断 | null を失敗 false で返す。DB NOT NULL 制約の場合は From を使用。 |

### 3.2 検証メソッド

#### Validate(int value): void (override)

```csharp
public override void Validate(int value)
{
    // 1000 は管理者予約
    if (value == 1000)
    {
        throw new ArgumentOutOfRangeException(
            nameof(value),
            value,
            $"EmployeeNumber 1000 is reserved for system administrator.");
    }
    
    // 1001～8499 の範囲内か確認
    if (value < 1001 || value > 8499)
    {
        throw new ArgumentOutOfRangeException(
            nameof(value),
            value,
            $"EmployeeNumber must be in range 1001-8499 (1000 is reserved).");
    }
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | `int value`: 検証対象の番号 |
| 戻り値 | `void` |
| 例外 | `ArgumentOutOfRangeException`: 無効な値（1000、1000以下、8499以上） |
| 処理フロー | 1. 1000 であれば例外 throw<br/>2. 1001～8499 範囲内でなければ例外 throw |
| 用途 | PrimitiveValueObject<int> のコンストラクタから自動呼び出し |
| 設計判断 | 基本的な範囲チェックのみ。派遣の 7500～7999 vs 70000～ 等の詳細チェックは Entity責務。 |

### 3.3 表示メソッド

#### ToString(): string (override)

```csharp
public override string ToString() => ValueField.ToString("D5");
```

| 項目 | 内容 |
|------|------|
| 戻り値 | `string`: 左0埋めで5桁（例："01234"） |
| 実装 | `ValueField.ToString("D5")` — int の標準フォーマット指定子 |
| 用途 | UI表示・ログ出力・ユーザーへの表示 |
| 例 | 1234 → "01234", 8499 → "08499" |
| 設計判断 | .NET の ToString(format) で効率的に実装。 |

### 3.4 等価性メソッド

#### Equals(object? obj): bool (override)

```csharp
public override bool Equals(object? obj) => Equals(obj as EmployeeNumber);
```

#### Equals(EmployeeNumber? other): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `EmployeeNumber? other`: 比較対象 |
| 戻り値 | `bool`: 等価時 true |
| 処理フロー | 1. other が null → false<br/>2. 自己参照チェック（ReferenceEquals）→ true<br/>3. ValueField を比較 → ValueField == other.ValueField |
| 用途 | ValueObject の等価性判定 |
| 設計判断 | IsSet は常に true のため比較不要（PrimitiveValueObject<int>の仕様） |

#### GetHashCode(): int (override)

```csharp
public override int GetHashCode() => ValueField.GetHashCode();
```

| 項目 | 内容 |
|------|------|
| 戻り値 | `int`: ValueField のハッシュコード |
| 用途 | ディクショナリ・ハッシュセット内での使用 |
| 設計判断 | Equals=true なら同じハッシュ値を返す保証。 |

---

## 4. レイヤ制約確認

| 項目 | 確認 |
|------|------|
| Domain層への依存なし | ✓ PrimitiveValueObject<int> のみ使用 |
| Application層への依存なし | ✓ 使用されない |
| Infrastructure層への依存なし | ✓ 使用されない |
| Presentation層への依存なし | ✓ 使用されない |
| SharedKernel のみ参照 | ✓ PrimitiveValueObject<int> を継承 |

---

## 5. 実装上の注意点

### 5.1 コンストラクタ（private）

```csharp
private EmployeeNumber(int value) : base(value, true)
{
}
```

- private 修飾子で外部の `new` を禁止
- ファクトリメソッド（From等）経由のみで生成
- base(value, true) で PrimitiveValueObject のコンストラクタを呼び出し、Validate が自動実行

### 5.2 ToString フォーマット指定子

- `"D5"`: Decimal 5-digit format（整数を5桁の10進数で表現、左0埋め）
- 例：1 → "00001", 1234 → "01234", 8499 → "08499"

### 5.3 Entity での範囲チェック実装

EmployeeNumber は 1001～8499 を検証するが、Employee Entity では以下の詳細チェックを実装：

```csharp
// Entity内での実装例
private void ValidateNumberRange(EmployeeDivision division, EmployeeNumber number)
{
    int num = number.Value;
    
    if (division.IsRegularEmployee)
    {
        // 1001～6999 か 10000～ 
        if (!((1001 <= num && num <= 6999) || num >= 10000))
            throw new DomainException("Invalid employee number for regular employee");
    }
    else if (division.IsDispatched)
    {
        // 7500～7999 か 70000～
        if (!((7500 <= num && num <= 7999) || num >= 70000))
            throw new DomainException("Invalid employee number for dispatched");
    }
    else if (division.IsContractor)
    {
        // 8000～8499 か 80000～
        if (!((8000 <= num && num <= 8499) || num >= 80000))
            throw new DomainException("Invalid employee number for contractor");
    }
}
```

---

## 参考資料

- 技術仕様書: `EmployeeNumber_技術仕様書.md`
- 単体テスト仕様: `EmployeeNumber_単体テスト仕様書.md`
- PrimitiveValueObject 基底: `src/SharedKernel/ValueObjects/Abstractions/PrimitiveValueObject.cs`
