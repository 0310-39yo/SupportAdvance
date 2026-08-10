# DeletedBy 技術仕様書

**バージョン:** 1.0  
**作成日:** 2026年08月10日  
**責務:** エンティティの削除者（従業員行ID）を管理する値オブジェクト

---

## 1. 概要

### 1.1 クラス説明

`DeletedBy` は、エンティティが削除（論理削除）された時点での実行者（従業員行ID）を表す**値オブジェクト**です。未削除の場合は Unset 状態で表現します。

- **継承**: `PrimitiveValueObject<long?>`
- **IsSet で状態管理**: `IsSet = true` なら削除済み、`IsSet = false` なら存続
- **シール**: `sealed class` （拡張不可）
- **用途**: 監査情報として、エンティティを削除した従業員を追跡する（論理削除）

### 1.2 責務

| 責務 | 説明 |
|---|---|
| **従業員行IDの安全な保管** | long? 値をラップし、カプセル化された形で提供 |
| **値の検証** | 設定時は正の数値のみを受け入れ |
| **Unset状態管理** | null/未削除を IsSet フラグで表現 |
| **等価性判定** | 削除者IDと IsSet が同一のインスタンスを等価と判定 |
| **ハッシング対応** | HashMap や HashSet への格納に対応 |

---

## 2. プロパティ & メソッド

### 2.1 プロパティ

#### `Value : long? { get; }`

**役割:** 保持する従業員行ID値を読み取り専用で取得（IsSet = true のときのみ有効）

```csharp
var deleted = DeletedBy.From(1003L);
long? rowId = deleted.Value;  // 1003 を取得

var unset = DeletedBy.Unset();
long? rowId = unset.Value;  // null を取得
```

#### `IsSet : bool { get; }` (PrimitiveValueObject から継承)

**役割:** 削除済み/未削除を判定

```csharp
var deleted = DeletedBy.From(1003L);
Assert.True(deleted.IsSet);  // true = 削除済み

var unset = DeletedBy.Unset();
Assert.False(unset.IsSet);  // false = 未削除
```

#### `IsDeleted : bool { get; }`

**役割:** 削除済み状態を判定（IsSet の別名）

```csharp
var deleted = DeletedBy.From(1003L);
Assert.True(deleted.IsDeleted);  // true

var unset = DeletedBy.Unset();
Assert.False(unset.IsDeleted);  // false
```

### 2.2 ファクトリメソッド

#### `From(long value) : DeletedBy`

**役割:** 従業員行IDから DeletedBy を生成

```csharp
var deletedBy = DeletedBy.From(1003L);
```

**例外:**
- `ArgumentException` : 値が 0 以下の場合

#### `Unset() : DeletedBy`

**役割:** 未削除状態の DeletedBy を生成

```csharp
var unset = DeletedBy.Unset();
Assert.False(unset.IsSet);
Assert.Null(unset.Value);
```

#### `TryFrom(long? input, out DeletedBy result) : bool`

**役割:** null安全な生成（null は Unset に変換）

```csharp
long? input = null;  // null が来た場合
bool success = DeletedBy.TryFrom(input, out var result);
if (success && !result.IsSet)
{
    // null → Unset() で成功
}
```

**実行フロー:**
1. `input == null || !input.HasValue` → `result = Unset()` 返却、**true** を返す
2. `From(input.Value)` 呼び出し → 成功時 true、例外時 false

#### `TryFromDbValue(long? input, out DeletedBy result) : bool`

**役割:** DB から読み込んだ long? 値から DeletedBy を生成（DB NULL は Unset に変換）

```csharp
long? dbValue = reader.GetInt64OrNull("deleted_by");
bool success = DeletedBy.TryFromDbValue(dbValue, out var deletedBy);
// dbValue が null の場合、deletedBy は Unset()
```

**実行フロー:**
1. `input == null || !input.HasValue` → `result = Unset()` 返却、**true** を返す（DB NULL → Unset）
2. `From(input.Value)` 呼び出し → 成功時 true、例外時 false

### 2.3 等価性メソッド

#### `Equals(object? obj) : bool`

**役割:** オブジェクト等価性の判定

```csharp
var a = DeletedBy.From(1003L);
var b = DeletedBy.From(1003L);
Assert.Equal(a, b);  // true（同じ従業員行ID、同じ IsSet）

var unset1 = DeletedBy.Unset();
var unset2 = DeletedBy.Unset();
Assert.Equal(unset1, unset2);  // true（どちらも Unset）

var deleted = DeletedBy.From(1003L);
var unset = DeletedBy.Unset();
Assert.NotEqual(deleted, unset);  // false（IsSet が異なる）
```

#### `GetHashCode() : int`

**役割:** ハッシュコード提供、HashMap/HashSet 互換

```csharp
var set = new HashSet<DeletedBy>();
set.Add(DeletedBy.From(1003L));
set.Add(DeletedBy.Unset());
```

### 2.4 文字列化

#### `ToString() : string`

**役割:** 従業員行IDまたは "Unset" の文字列表現を返す

```csharp
var deleted = DeletedBy.From(1003L);
string str = deleted.ToString();  // "1003"

var unset = DeletedBy.Unset();
string str = unset.ToString();  // "Unset"
```

---

## 3. 検証ルール

### 3.1 Validate メソッド

**検証項目:**
- ✓ `null` (Unset状態) : 受け入れ（TryFrom/TryFromDbValue で成功）
- ✓ `0` より大きい値 : 受け入れ
- ✗ `0` 以下の値 : 例外を投げる

**例:**

```csharp
// OK: 正の数値（From）
var ok = DeletedBy.From(1003L);

// OK: null（Unset に変換）
bool success = DeletedBy.TryFrom(null, out var unset);  // success = true, unset.IsSet = false

// OK: Unset 生成
var unset = DeletedBy.Unset();

// NG: 0（From）
var exception1 = Assert.Throws<ArgumentException>(() =>
    DeletedBy.From(0L));

// NG: 負の数（From）
var exception2 = Assert.Throws<ArgumentException>(() =>
    DeletedBy.From(-1L));
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

---

## 5. コンストラクタ

### 5.1 private コンストラクタ

```csharp
private DeletedBy(long? value, bool isSet) : base(value, isSet)
{
}
```

**設計意図:**
- ファクトリメソッド（From, Unset, TryFrom）のみで生成を許可
- isSet フラグで未削除状態を管理
- 不正な値の混入を防止

---

## 6. 使用例

### 6.1 基本的な使用（推奨）

```csharp
var deletedBy = DeletedBy.From(operatorEmployeeRowId);

// 削除済み状態の判定
if (deletedBy.IsDeleted)
{
    long rowId = deletedBy.Value.Value;  // long? → long に変換
    Console.WriteLine($"削除者: {rowId}");
}
```

### 6.2 未削除状態の表現

```csharp
var unset = DeletedBy.Unset();
Assert.False(unset.IsSet);
Assert.Null(unset.Value);
```

### 6.3 Entity での削除時パターン

```csharp
public class Entity
{
    public DeletedBy DeletedBy { get; private set; }
    
    // 初期化時は Unset（未削除）
    public Entity()
    {
        DeletedBy = DeletedBy.Unset();
    }
    
    // 削除時に従業員行IDを設定
    public void Delete(long deletorRowId)
    {
        DeletedBy = DeletedBy.From(deletorRowId);
    }
}
```

### 6.4 DB からの読み込み

```csharp
long? dbValue = reader.GetInt64OrNull("deleted_by");
if (DeletedBy.TryFromDbValue(dbValue, out var deletedBy))
{
    // deletedBy は設定済みまたは Unset
    Console.WriteLine($"削除済み: {deletedBy.IsDeleted}");
}
```

---

## 7. 実装上の注意点

| 項目 | 説明 |
|---|---|
| **IsSet フラグ** | 削除済み/未削除を IsSet で判定（null チェック不要） |
| **値の範囲** | 従業員行IDは 1 以上（0 と負数は不可） |
| **null 処理** | TryFrom(null) は Unset に変換して成功（true を返す） |
| **DB マッピング** | BIGINT NULL (SQL Server) ← → long? (C#) |
| **未削除初期値** | Entity 生成時は DeletedBy.Unset() で初期化 |
| **論理削除** | deleted_at が NULL のとき有効行（IS NULL で有効行フィルタ） |

---

## 8. IEquatable<DeletedBy> 実装

DeletedBy は `IEquatable<DeletedBy>` を実装し、以下の等価性ルールに従います：

- **同一インスタンス**: `ReferenceEquals` で true
- **値と IsSet 両方一致**: `ValueField`（long?）と `IsSet` の同一性で判定
- **IsSet が異なる**: false（`From(1003)` と `Unset()` は非等価）
- **null との比較**: false を返す

---

## 9. 他の監査情報 ValueObject との違い

| ValueObject | CreatedBy | UpdatedBy | DeletedBy |
|-------------|----------|----------|----------|
| 必須/オプション | 必須 | オプション | オプション |
| IsSet | 常に true | IsSet フラグで管理 | IsSet フラグで管理 |
| null | 失敗 | Unset に変換 | Unset に変換 |
| 初期値 | 作成時に指定必須 | Unset（未更新） | Unset（未削除） |
| 意味 | 作成者 | 最終更新者 | 削除者 |

---

## まとめ

`DeletedBy` は long? 値オブジェクトとして、以下を達成します：

✅ **安全性**: long を直接扱わずカプセル化  
✅ **検証**: 正の数値のみを受け入れ  
✅ **null吸収**: TryFrom/TryFromDbValue で null を Unset に変換  
✅ **不変性**: 作成後変更不可  
✅ **等価性**: long と IsSet の値同一性に基づく  
✅ **ハッシング**: HashMap/HashSet 対応  
✅ **論理削除対応**: deleted_at/deleted_by で削除履歴を追跡
