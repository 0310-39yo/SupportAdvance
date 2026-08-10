# CreatedBy 技術仕様書

**バージョン:** 1.0  
**作成日:** 2026年08月10日  
**責務:** エンティティの作成者（従業員行ID）を管理する値オブジェクト

---

## 1. 概要

### 1.1 クラス説明

`CreatedBy` は、エンティティが作成された時点での実行者（従業員行ID）を表す**値オブジェクト**です。ValueObject の本質である不変性により、スレッドセーフな設計を実現しています。

- **継承**: `PrimitiveValueObject<long>`
- **シール**: `sealed class` （拡張不可）
- **用途**: 監査情報として、エンティティを作成した従業員を追跡する
- **特性**: ValueObject の不変性 → 値による等価性、依存性の低い設計

### 1.2 責務

| 責務 | 説明 |
|---|---|
| **従業員行IDの安全な保管** | long 値をラップし、カプセル化された形で提供 |
| **値の検証** | 正の数値のみを受け入れ（従業員行IDは 1 以上） |
| **等価性判定** | 作成者IDが同一のインスタンスを等価と判定 |
| **ハッシング対応** | HashMap や HashSet への格納に対応 |

---

## 2. プロパティ & メソッド

### 2.1 プロパティ

#### `Value : long { get; }`

**役割:** 保持する従業員行ID値を読み取り専用で取得

```csharp
var createdBy = CreatedBy.From(1001L);
long employeeRowId = createdBy.Value;  // 1001 を取得
```

#### `IsSet : bool { get; }` (PrimitiveValueObject から継承)

**役割:** 値が設定されているかを判定（CreatedBy は常に true）

```csharp
var createdBy = CreatedBy.From(1001L);
Assert.True(createdBy.IsSet);  // 常に true
```

### 2.2 ファクトリメソッド

#### `From(long value) : CreatedBy`

**役割:** 従業員行IDから CreatedBy を生成

```csharp
var createdBy = CreatedBy.From(1001L);
```

**例外:**
- `ArgumentException` : 値が 0 以下の場合

#### `TryFrom(long? input, out CreatedBy result) : bool`

**役割:** null安全な生成（null は失敗）

```csharp
long? input = 1001L;
bool success = CreatedBy.TryFrom(input, out var createdBy);
if (!success)
{
    // 生成失敗（null または無効な値）
}
```

**実行フロー:**
1. `input.HasValue` が false → false を返す
2. `From(input.Value)` 呼び出し → 成功時 true、例外時 false

#### `TryFromDbValue(long? input, out CreatedBy result) : bool`

**役割:** DB から読み込んだ long? 値から CreatedBy を生成

```csharp
long? dbValue = reader.GetInt64OrNull("created_by");
bool success = CreatedBy.TryFromDbValue(dbValue, out var createdBy);
```

**実行フロー:**
1. `input.HasValue` が false → false を返す（DB NULL は失敗）
2. `From(input.Value)` 呼び出し → 成功時 true、例外時 false

### 2.3 等価性メソッド

#### `Equals(object? obj) : bool`

**役割:** オブジェクト等価性の判定

```csharp
var a = CreatedBy.From(1001L);
var b = CreatedBy.From(1001L);
Assert.Equal(a, b);  // true（同じ従業員行ID）
```

#### `Equals(CreatedBy? other) : bool`

**役割:** CreatedBy 間の強い型チェック

```csharp
if (createdBy1.Equals(createdBy2))
{
    // 同一の作成者
}
```

#### `GetHashCode() : int`

**役割:** ハッシュコード提供、HashMap/HashSet 互換

```csharp
var set = new HashSet<CreatedBy>();
set.Add(CreatedBy.From(1001L));
```

### 2.4 文字列化

#### `ToString() : string`

**役割:** 従業員行IDの文字列表現を返す

```csharp
var createdBy = CreatedBy.From(1001L);
string str = createdBy.ToString();  // "1001"
```

---

## 3. 検証ルール

### 3.1 Validate メソッド

**検証項目:**
- ✓ `0` より大きい値 : 受け入れ
- ✗ `0` 以下の値 : 例外を投げる

**例:**

```csharp
// OK: 正の数値
var ok = CreatedBy.From(1001L);

// NG: 0
var exception1 = Assert.Throws<ArgumentException>(() =>
    CreatedBy.From(0L));

// NG: 負の数
var exception2 = Assert.Throws<ArgumentException>(() =>
    CreatedBy.From(-1L));
```

---

## 4. ValueObject としての不変性保証

ValueObject の本質である不変性は以下により確保される：

| 要素 | 実装方法 | 効果 |
|---|---|---|
| **クラス** | `sealed` | 拡張禁止で ValueObject 契約を守る |
| **フィールド** | `readonly` | 外部から変更不可 |
| **プロパティ** | `get-only` | 再代入不可 |
| **コンストラクタ** | `private` | ファクトリメソッドのみで生成 |
| **メソッド** | 純粋メソッド | 状態を変更しない |

**スレッドセーフ性:**
- 不変性により同期化コード不要
- 複数スレッドからの安全なアクセス

---

## 5. コンストラクタ

### 5.1 private コンストラクタ

```csharp
private CreatedBy(long value) : base(value, true)
{
}
```

**設計意図:**
- ファクトリメソッド（From, TryFrom）のみで生成を許可
- 不正な値の混入を防止

---

## 6. 使用例

### 6.1 基本的な使用（推奨）

```csharp
var createdBy = CreatedBy.From(operatorEmployeeRowId);

// 従業員行IDの参照
long rowId = createdBy.Value;
Console.WriteLine(createdBy);   // "1001" 形式で出力
```

### 6.2 Entity での初期化パターン

```csharp
public class Entity
{
    public CreatedBy CreatedBy { get; }
    
    // Entity 生成時に作成者IDを指定
    public Entity(long createdByRowId)
    {
        CreatedBy = CreatedBy.From(createdByRowId);
    }
}
```

### 6.3 安全な生成（null安全性）

```csharp
long? operatorId = GetOperatorIdFromContext();

if (CreatedBy.TryFrom(operatorId, out var createdBy))
{
    Console.WriteLine($"作成者: {createdBy.Value}");
}
else
{
    Console.WriteLine("作成者の指定がありません");
}
```

### 6.4 等価性の判定

```csharp
var createdBy1 = CreatedBy.From(1001L);
var createdBy2 = CreatedBy.From(1001L);

if (createdBy1 == createdBy2)  // Equals メソッド経由
{
    Console.WriteLine("同一の作成者");
}

var set = new HashSet<CreatedBy> { createdBy1, createdBy2 };
Console.WriteLine(set.Count);  // 1 （等価なら重複排除）
```

---

## 7. 実装上の注意点

| 項目 | 説明 |
|---|---|
| **値の範囲** | 従業員行IDは 1 以上の long 値 |
| **null 処理** | CreatedBy は必須値（null は失敗を返す） |
| **DB マッピング** | BIGINT (SQL Server) / long (C#) |

---

## 8. IEquatable<CreatedBy> 実装

CreatedBy は `IEquatable<CreatedBy>` を実装し、以下の等価性ルールに従います：

- **同一インスタンス**: `ReferenceEquals` で true
- **値同一**: `ValueField`（long）の同一性で判定
- **null との比較**: false を返す

---

## まとめ

`CreatedBy` は long 値オブジェクトとして、以下を達成します：

✅ **安全性**: long を直接扱わずカプセル化  
✅ **検証**: 正の数値のみを受け入れ  
✅ **不変性**: 作成後変更不可  
✅ **等価性**: long の値同一性に基づく  
✅ **ハッシング**: HashMap/HashSet 対応
