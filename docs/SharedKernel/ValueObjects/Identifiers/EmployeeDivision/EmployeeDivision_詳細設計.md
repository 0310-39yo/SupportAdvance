# EmployeeDivision - 詳細設計書

**プロジェクト:** SupportAdvance  
**レイヤ:** SharedKernel（基盤層）  
**種別:** ValueObject 詳細設計  
**依拠技術仕様書:** EmployeeDivision技術仕様書 v1.0  
**版:** 1.0 / 2026-08-09

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `EmployeeDivision` |
| 種別 | `sealed class` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects.Identifiers` |
| 実装インターフェース | `IEquatable<EmployeeDivision>` |
| 継承元 | `EnumValueObject<char>` |
| 配置レイヤ | SharedKernel（全層から参照される基盤型） |

### 1.2 責務

- **固定値の管理**: M（従業員）、T（派遣社員）、C（請負者）の3つの区分値を定数として提供
- **値の検証**: 指定された値が有効な区分値（M/T/C）であることを検証
- **日本語名の提供**: 区分値に対応する日本語名（従業員/派遣社員/請負者）を提供
- **DB値との双方向変換**: char/string 値から EmployeeDivision を生成、逆変換でDB保存値を取得
- **型安全な等価性判定**: ValueObject として型システムレベルで等価性を保証

### 1.3 協調クラス

```
EmployeeDivision  ──inherits──▶  EnumValueObject<char>
                                     └─ Validate(char)
                                     └─ GetDisplayName()

EmployeeDivision  ──implements──▶  IEquatable<EmployeeDivision>
                                     └─ Equals(EmployeeDivision?)
```

---

## 2. プロパティ設計

### 2.1 定数（public const char）

#### RegularEmployeeValue = 'M'

| 項目 | 内容 |
|------|------|
| 値 | `'M'` |
| 型 | `char` |
| 修飾子 | `public const` |
| 用途 | 従業員区分の識別子、ファクトリメソッドの引数 |
| 設計判断 | DB カラム `employee_division` と一致させるため char を採用。共有する定数として public const で外部公開。 |

#### DispatchedValue = 'T'

| 項目 | 内容 |
|------|------|
| 値 | `'T'` |
| 型 | `char` |
| 修飾子 | `public const` |
| 用途 | 派遣社員区分の識別子 |
| 設計判断 | 同上 |

#### ContractorValue = 'C'

| 項目 | 内容 |
|------|------|
| 値 | `'C'` |
| 型 | `char` |
| 修飾子 | `public const` |
| 用途 | 請負者区分の識別子 |
| 設計判断 | 同上 |

### 2.2 プロパティ（public property）

#### Value: char

| 項目 | 内容 |
|------|------|
| 型 | `char` |
| アクセス | `public get` |
| 実装 | `=> ValueField` （EnumValueObject<char> の protected readonly フィールド） |
| 用途 | 区分値を取得、DB保存時の値として使用 |
| 設計判断 | 読み取り専用（不変性を確保）。ValueField への直接参照で効率化。 |

#### IsRegularEmployee: bool

| 項目 | 内容 |
|------|------|
| 型 | `bool` |
| アクセス | `public get` |
| 実装 | `=> ValueField == RegularEmployeeValue` |
| 用途 | 従業員区分かどうかを判定 |
| 設計判断 | ビジネスロジック側で明示的な判定メソッドを提供することで可読性向上。 |

#### IsDispatched: bool

| 項目 | 内容 |
|------|------|
| 型 | `bool` |
| アクセス | `public get` |
| 実装 | `=> ValueField == DispatchedValue` |
| 用途 | 派遣社員区分かどうかを判定 |
| 設計判断 | 同上 |

#### IsContractor: bool

| 項目 | 内容 |
|------|------|
| 型 | `bool` |
| アクセス | `public get` |
| 実装 | `=> ValueField == ContractorValue` |
| 用途 | 請負者区分かどうかを判定 |
| 設計判断 | 同上 |

---

## 3. メソッド設計

### 3.1 ファクトリメソッド（static, public）

#### RegularEmployee(): EmployeeDivision

```csharp
public static EmployeeDivision RegularEmployee() => new(RegularEmployeeValue);
```

| 項目 | 内容 |
|------|------|
| 戻り値 | `EmployeeDivision`（IsSet=true） |
| パラメータ | なし |
| 用途 | 従業員区分を生成（呼び出し側で 'M' を記述不要） |
| 設計判断 | Ubiquitous Language を反映し、`new(RegularEmployeeValue)` より意図的な名前を使用。 |

#### Dispatched(): EmployeeDivision

```csharp
public static EmployeeDivision Dispatched() => new(DispatchedValue);
```

同上（派遣社員区分）

#### Contractor(): EmployeeDivision

```csharp
public static EmployeeDivision Contractor() => new(ContractorValue);
```

同上（請負者区分）

### 3.2 生成メソッド

#### From(char value): EmployeeDivision

```csharp
public static EmployeeDivision From(char value) => new(value);
```

| 項目 | 内容 |
|------|------|
| パラメータ | `char value`: 区分値（M/T/C） |
| 戻り値 | `EmployeeDivision`（IsSet=true） |
| 例外 | `ArgumentOutOfRangeException`: 無効な値の場合 |
| 処理フロー | 1. char を受け取る<br/>2. コンストラクタで Validate を実行<br/>3. 例外なら ArgumentOutOfRangeException を throw |
| 用途 | char 値から確定的に EmployeeDivision を生成（検証あり） |
| 設計判断 | 入力値が確定している場合に使用。Domain層での明示的な生成用。 |

#### TryFrom(char? input, out EmployeeDivision result): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `char? input`: 区分値（null 許容） |
| 戻り値 | `bool`: 成功時 true、失敗時 false |
| out パラメータ | `EmployeeDivision result`: 生成されたインスタンス |
| 処理フロー | 1. input が null または HasValue=false → false を返す<br/>2. From(value) を呼び出す<br/>3. ArgumentOutOfRangeException をキャッチ → false を返す<br/>4. 成功時 true、result に値を設定 |
| 用途 | Application層の入力（DTO）から null安全に生成 |
| 設計判断 | null 入力を許容し false で返す（例外ではなくフロー制御）。 |

#### FromDbValue(string value): EmployeeDivision

```csharp
public static EmployeeDivision FromDbValue(string value)
{
    if (string.IsNullOrEmpty(value) || value.Length == 0)
        throw new ArgumentException("...");
    return new(value[0]);  // 最初の文字を抽出
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | `string value`: DB読み込み値（nvarchar） |
| 戻り値 | `EmployeeDivision`（IsSet=true） |
| 例外 | `ArgumentException`: 空文字列の場合 |
| 処理フロー | 1. string が null または空 → ArgumentException throw<br/>2. 最初の文字を char に変換<br/>3. コンストラクタで Validate 実行 |
| 用途 | Infrastructure層で DbModel から Entity に変換時 |
| 設計判断 | DB値は nvarchar で格納されるため string から char に変換。複数文字の場合は最初の1文字のみ使用。 |

#### TryFromDbValue(string? input, out EmployeeDivision result): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `string? input`: DB読み込み値（null 許容） |
| 戻り値 | `bool`: 成功時 true、失敗時 false |
| 処理フロー | 1. input が null または空 → false を返す<br/>2. FromDbValue を呼び出す<br/>3. ArgumentException をキャッチ → false を返す |
| 用途 | Infrastructure層で null安全にDB値から生成 |
| 設計判断 | DB層の nullable カラム対応。null を失敗 false で返す。 |

### 3.3 検証メソッド

#### Validate(char value): void (override)

```csharp
public override void Validate(char value)
{
    if (value != RegularEmployeeValue && 
        value != DispatchedValue && 
        value != ContractorValue)
    {
        throw new ArgumentOutOfRangeException(nameof(value), value, 
            $"EmployeeDivision must be one of: '{RegularEmployeeValue}' (従業員), ...");
    }
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | `char value`: 検証対象の区分値 |
| 戻り値 | `void` |
| 例外 | `ArgumentOutOfRangeException`: M/T/C 以外の値 |
| 処理フロー | 3つの定数値と比較。いずれでもなければ例外 throw |
| 用途 | EnumValueObject<char> のコンストラクタから自動呼び出し |
| 設計判断 | 値の許可リスト（M/T/C）で厳密に検証。新規区分追加時はここを更新。 |

### 3.4 表示名メソッド

#### GetDisplayName(): string (override, protected)

```csharp
protected override string GetDisplayName()
{
    return ValueField switch
    {
        RegularEmployeeValue => "従業員",
        DispatchedValue => "派遣社員",
        ContractorValue => "請負者",
        _ => "不明"  // Validate で排除されるため到達不可
    };
}
```

| 項目 | 内容 |
|------|------|
| 戻り値 | `string`: 区分の日本語名 |
| 用途 | ToString() で呼ばれ、UI表示・ログ出力で使用 |
| 設計判断 | switch 式で char → 日本語名を変換。Validate で無効値は排除されるため _ は防御的記述。 |

### 3.5 等価性メソッド

#### Equals(object? obj): bool (override)

```csharp
public override bool Equals(object? obj) => Equals(obj as EmployeeDivision);
```

#### Equals(EmployeeDivision? other): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `EmployeeDivision? other`: 比較対象 |
| 戻り値 | `bool`: 等価時 true |
| 処理フロー | 1. other が null → false<br/>2. 自己参照チェック（ReferenceEquals）→ true<br/>3. ValueField を比較 → ValueField == other.ValueField |
| 用途 | ValueObject の等価性判定 |
| 設計判断 | IsSet は EnumValueObject で自動処理（常に true のため比較不要） |

#### GetHashCode(): int (override)

```csharp
public override int GetHashCode() => ValueField.GetHashCode();
```

| 項目 | 内容 |
|------|------|
| 戻り値 | `int`: ValueField のハッシュコード |
| 用途 | ディクショナリ・ハッシュセット内での使用 |
| 設計判断 | Equals=true なら同じハッシュ値を返す保証。IsSet は無視（常に true）。 |

---

## 4. レイヤ制約確認

| 項目 | 確認 |
|------|------|
| Domain層への依存なし | ✓ Common, Identifiers の定数のみ使用 |
| Application層への依存なし | ✓ 使用されない |
| Infrastructure層への依存なし | ✓ 使用されない |
| Presentation層への依存なし | ✓ 使用されない |
| SharedKernel のみ参照 | ✓ EnumValueObject<char> を継承 |

---

## 5. 実装上の注意点

### 5.1 コンストラクタ（private）

```csharp
private EmployeeDivision(char value) : base(value)
{
}
```

- private 修飾子で外部の `new` を禁止
- ファクトリメソッド（From, RegularEmployee等）経由のみで生成
- base(value) で EnumValueObject のコンストラクタを呼び出し、Validate が自動実行

### 5.2 新規区分追加時の手順

将来的に新しい区分（例：G「業務委託グループ」）を追加する場合：

1. 定数 `GroupValue = 'G'` を追加
2. ファクトリメソッド `public static EmployeeDivision Group() => new('G');` を追加
3. 判定プロパティ `public bool IsGroup => ValueField == GroupValue;` を追加
4. Validate メソッドの条件に `value != GroupValue` を追加
5. GetDisplayName の switch に `GroupValue => "業務委託グループ",` を追加
6. テストケースを拡張

---

## 参考資料

- 技術仕様書: `EmployeeDivision_技術仕様書.md`
- 単体テスト仕様: `EmployeeDivision_単体テスト仕様書.md`
- EnumValueObject 基底: `src/SharedKernel/ValueObjects/Abstractions/EnumValueObject.cs`
