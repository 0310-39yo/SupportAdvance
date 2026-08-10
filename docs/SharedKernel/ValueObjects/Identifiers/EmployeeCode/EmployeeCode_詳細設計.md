# EmployeeCode - 詳細設計書

**プロジェクト:** SupportAdvance  
**レイヤ:** SharedKernel（基盤層）  
**種別:** ValueObject 詳細設計  
**依拠技術仕様書:** EmployeeCode技術仕様書 v1.0  
**版:** 1.0 / 2026-08-09

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `EmployeeCode` |
| 種別 | `sealed class` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects.Identifiers` |
| 実装インターフェース | `IEquatable<EmployeeCode>` |
| 継承元 | `ValueObject`（複合値オブジェクト） |
| 配置レイヤ | SharedKernel（全層から参照される基盤型） |

### 1.2 責務

- **複合値の管理**: EmployeeDivision と EmployeeNumber の組み合わせを保持
- **値の検証**: 区分と番号の組み合わせが有効か検証（範囲チェック）
- **表示形式の提供**: M1234 形式の文字列表現を提供
- **文字列パース**: "M1234" から EmployeeCode を復元
- **DB値との双方向変換**: char/int から EmployeeCode を生成、逆変換
- **型安全な等価性判定**: ValueObject として型システムレベルで等価性を保証

### 1.3 協調クラス

```
EmployeeCode  ──contains──▶  EmployeeDivision
                                  └─ Validate
                                  └─ Value

EmployeeCode  ──contains──▶  EmployeeNumber
                                  └─ Validate
                                  └─ Value

EmployeeCode  ──implements──▶  IEquatable<EmployeeCode>
                                  └─ Equals(EmployeeCode?)
                                  
Employee Entity  ──uses──▶  EmployeeCode
                              └─ 範囲検証済み値を信頼
```

---

## 2. プロパティ設計

### 2.1 プロパティ（public property）

#### Division: EmployeeDivision

| 項目 | 内容 |
|------|------|
| 型 | `EmployeeDivision` |
| アクセス | `public get` |
| 実装 | `_division` フィールド（readonly） |
| 用途 | 従業員区分（M/T/C）を取得 |
| 設計判断 | 読み取り専用（不変性を確保）。 |

#### Number: EmployeeNumber

| 項目 | 内容 |
|------|------|
| 型 | `EmployeeNumber` |
| アクセス | `public get` |
| 実装 | `_number` フィールド（readonly） |
| 用途 | 従業員番号を取得 |
| 設計判断 | 読み取り専用（不変性を確保）。 |

---

## 3. メソッド設計

### 3.1 生成メソッド

#### From(EmployeeDivision division, EmployeeNumber number): EmployeeCode

```csharp
public static EmployeeCode From(EmployeeDivision division, EmployeeNumber number)
{
    ValidateDivisionAndNumber(division, number);
    return new(division, number);
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | `division`: EmployeeDivision, `number`: EmployeeNumber |
| 戻り値 | `EmployeeCode` |
| 例外 | `ArgumentException`: 無効な組み合わせ（範囲チェック失敗） |
| 処理フロー | 1. ValidateDivisionAndNumber を呼び出し<br/>2. 例外なら new で生成 |
| 用途 | Entity 生成時、確定的な生成 |
| 設計判断 | 両 ValueObject が既に生成済みの場合に使用。 |

#### TryFrom(EmployeeDivision division, EmployeeNumber number, out EmployeeCode result): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `division`, `number` |
| 戻り値 | `bool`: 成功時 true、失敗時 false |
| 処理フロー | 1. From を呼び出し<br/>2. ArgumentException をキャッチ → false |
| 用途 | Application層での安全な生成 |
| 設計判断 | null 入力は事前に ValueObject の TryFrom で処理済みと仮定。 |

#### TryParse(string? input, out EmployeeCode result): bool

```csharp
public static bool TryParse(string? input, out EmployeeCode result)
{
    // 例：input = "M1234"
    // 1. string を char（区分）と int（番号）に分解
    // 2. EmployeeDivision.TryFromDbValue, EmployeeNumber.TryFromDbValue を呼び出し
    // 3. TryFrom を呼び出し
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | `string? input`: "M1234" 形式 |
| 戻り値 | `bool`: 成功時 true、失敗時 false |
| 処理フロー | 1. string が null または空 → false<br/>2. 最初の char が有効な区分か確認<br/>3. 残り部分を int に変換<br/>4. EmployeeDivision.TryFromDbValue, EmployeeNumber.From で生成<br/>5. TryFrom で組み合わせ検証 |
| 用途 | ログイン認証で "M1234" から EmployeeCode を復元 |
| 設計判断 | 形式エラーは false で返す（例外ではない）。 |

#### TryFromDbValues(char divisionChar, int numberInt, out EmployeeCode result): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `divisionChar`: DB の employee_division, `numberInt`: DB の employee_number |
| 戻り値 | `bool`: 成功時 true、失敗時 false |
| 処理フロー | 1. EmployeeDivision.TryFromDbValue を呼び出し<br/>2. EmployeeNumber.TryFromDbValue を呼び出し<br/>3. TryFrom で組み合わせ検証 |
| 用途 | Infrastructure層で DbModel → Entity 変換 |
| 設計判断 | DB値は既に存在する（検証済み）と仮定。念のため TryFrom で再検証。 |

### 3.2 検証メソッド

#### ValidateDivisionAndNumber(EmployeeDivision division, EmployeeNumber number): void (private)

```csharp
private static void ValidateDivisionAndNumber(EmployeeDivision division, EmployeeNumber number)
{
    int num = number.Value;
    
    if (division.IsRegularEmployee)
    {
        // 1001～6999 か 10000～
        if (!((1001 <= num && num <= 6999) || num >= 10000))
            throw new ArgumentException("Regular employee number must be in range 1001-6999 or 10000+");
    }
    else if (division.IsDispatched)
    {
        // 7500～7999 か 70000～
        if (!((7500 <= num && num <= 7999) || num >= 70000))
            throw new ArgumentException("Dispatched employee number must be in range 7500-7999 or 70000+");
    }
    else if (division.IsContractor)
    {
        // 8000～8499 か 80000～
        if (!((8000 <= num && num <= 8499) || num >= 80000))
            throw new ArgumentException("Contractor number must be in range 8000-8499 or 80000+");
    }
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | `division`, `number` |
| 戻り値 | `void` |
| 例外 | `ArgumentException`: 区分と番号の組み合わせが無効 |
| 処理フロー | 1. Division を判定<br/>2. Number がその区分の有効範囲か確認<br/>3. 範囲外ならArgumentException throw |
| 用途 | From, TryFrom, TryParse, TryFromDbValues から呼び出し |
| 設計判断 | 単一責任原則。範囲チェックロジックを集約。 |

### 3.3 表示メソッド

#### ToString(): string (override)

```csharp
public override string ToString() => $"{Division.Value}{Number.Value}";
```

| 項目 | 内容 |
|------|------|
| 戻り値 | `string`: "M1234" または "M12345" 形式 |
| 実装 | Division.Value + Number.Value を連結 |
| 例 | M1234, M12345, T7500, C8000 |
| 用途 | UI表示・ログ出力 |
| 設計判断 | 区分 + 番号（スペースなし、左0埋めなし）。 |

### 3.4 等価性メソッド

#### Equals(object? obj): bool (override)

```csharp
public override bool Equals(object? obj) => Equals(obj as EmployeeCode);
```

#### Equals(EmployeeCode? other): bool

| 項目 | 内容 |
|------|------|
| パラメータ | `EmployeeCode? other` |
| 戻り値 | `bool` |
| 処理フロー | 1. other が null → false<br/>2. 自己参照 → true<br/>3. Division と Number を比較 |
| 用途 | ValueObject の等価性判定 |

#### GetHashCode(): int (override)

```csharp
public override int GetHashCode() => HashCode.Combine(Division, Number);
```

| 項目 | 内容 |
|------|------|
| 戻り値 | `int` |
| 実装 | HashCode.Combine を使用 |
| 用途 | ディクショナリ・ハッシュセット |

#### GetValueComponents(): IEnumerable<object?> (protected override)

```csharp
protected override IEnumerable<object?> GetValueComponents()
{
    yield return Division;
    yield return Number;
}
```

| 項目 | 内容 |
|------|------|
| 戻り値 | `IEnumerable<object?>` |
| 用途 | ValueObject の基本実装（等価性判定ロジック） |

---

## 4. コンストラクタ

### 4.1 プライベートコンストラクタ

```csharp
private EmployeeCode(EmployeeDivision division, EmployeeNumber number)
{
    _division = division;
    _number = number;
}
```

- private 修飾子で外部の `new` を禁止
- ファクトリメソッド（From等）経由のみで生成
- 検証済みの値のみを受け入れ

---

## 5. レイヤ制約確認

| 項目 | 確認 |
|------|------|
| Domain層への依存なし | ✓ EmployeeDivision, EmployeeNumber のみ使用 |
| Application層への依存なし | ✓ 使用されない |
| Infrastructure層への依存なし | ✓ 使用されない |
| Presentation層への依存なし | ✓ 使用されない |
| SharedKernel のみ参照 | ✓ EmployeeDivision, EmployeeNumber を含む |

---

## 6. 実装上の注意点

### 6.1 複合値オブジェクト（ValueObject）

ValueObject は不変であり、内部フィールドを直接公開しない（プロパティ経由）。

```csharp
// ❌ 直接フィールド公開（禁止）
public EmployeeDivision Division;

// ✅ プロパティ経由
public EmployeeDivision Division { get; }
private EmployeeDivision _division;
```

### 6.2 検証タイミング

検証は生成時（From, TryFrom等）に実行される。
Entity 生成後の追加検証は不要。

```csharp
// EmployeeCode 生成時に検証
var code = EmployeeCode.From(division, number);  // 無効なら例外

// Entity では信頼して使用
var employee = new Employee(id, code, personRowId);  // 再検証なし
```

### 6.3 文字列パースの実装

TryParse での文字列分解：

```csharp
// "M1234" の場合
char divisionChar = input[0];    // 'M'
string numberStr = input[1..];   // "1234"
int number = int.Parse(numberStr);  // 1234
```

---

## 参考資料

- 技術仕様書: `EmployeeCode_技術仕様書.md`
- 単体テスト仕様: `EmployeeCode_単体テスト仕様書.md`
- EmployeeDivision 詳細設計: `../EmployeeDivision/EmployeeDivision_詳細設計.md`
- EmployeeNumber 詳細設計: `../EmployeeNumber/EmployeeNumber_詳細設計.md`
