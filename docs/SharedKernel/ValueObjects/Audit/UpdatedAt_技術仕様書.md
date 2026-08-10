# UpdatedAt 技術仕様書

**バージョン:** 1.0  
**作成日:** 2025年  

## 1. 概要

### 1.1 クラス説明

`UpdatedAt` は、エンティティが最後に更新された日時を表す**値オブジェクト**です。ValueObject の本質である不変性により、スレッドセーフな設計を実現しています。

- **継承**: `PrimitiveValueObject<DateTime?>`
- **IsSet で状態管理**: `IsSet = true` なら更新済み、`IsSet = false` なら未更新
- **シール**: `sealed class` （拡張不可）

**特性:** ValueObject の不変性 → 値による等価性、依存性の低い設計、テスト容易性の向上

---

## 2. プロパティ & メソッド

### 2.1 プロパティ

#### `Value : DateTime? { get; }`

**役割:** 保持する日時値を読み取り専用で取得（IsSet = true のときのみ有効）

```csharp
IClock clock = /* DI から注入 */;
var updatedAt = UpdatedAt.From(clock.JstNow);
DateTime? dt = updatedAt.Value;  // DateTime? を取得
```

#### `IsSet : bool { get; }` (PrimitiveValueObject から継承)

**役割:** 更新済み/未更新を判定

```csharp
IClock clock = /* DI から注入 */;
var updated = UpdatedAt.From(clock.JstNow);
Assert.True(updated.IsSet);  // true = 更新済み

var unset = UpdatedAt.Unset();
Assert.False(unset.IsSet);  // false = 未更新
```

#### `HasUpdated : bool { get; }`

**役割:** 更新済み状態を判定（IsSet の別名）

```csharp
var updated = UpdatedAt.From(clock.JstNow);
Assert.True(updated.HasUpdated);  // true

var unset = UpdatedAt.Unset();
Assert.False(unset.HasUpdated);  // false
```

### 2.2 ファクトリメソッド

#### `From(LocalDateTime value) : UpdatedAt`

**役割:** IClock から取得した LocalDateTime から UpdatedAt を生成

```csharp
IClock clock = /* DI から注入 */;
var updatedAt = UpdatedAt.From(clock.JstNow);
```

**例外:**
- `ArgumentException` : 値が LocalDateTime.MinValue または LocalDateTime.MaxValue の場合

#### `Unset() : UpdatedAt`

**役割:** 未更新状態の UpdatedAt を生成（Unset）

```csharp
var unset = UpdatedAt.Unset();
Assert.False(unset.IsSet);
Assert.Null(unset.Value);
```

#### `TryFrom(LocalDateTime? input, out UpdatedAt result) : bool`

**役割:** null安全な生成

```csharp
IClock clock = /* DI から注入 */;
LocalDateTime? input = null;  // null が来た場合

bool success = UpdatedAt.TryFrom(input, out var result);
if (success && !result.IsSet)
{
	// null → Unset() で成功
}
```

**実行フロー:**
1. `input == null || !input.HasValue` → `result = Unset()` 返却、**true** を返す
2. `From(input.Value)` 呼び出し → 成功時 true、例外時 false

---

## 3. 等価性と比較

### 3.1 Equals(object? obj) メソッド

**役割:** 汎用オブジェクト比較

```csharp
IClock clock = /* DI から注入 */;
object updated1 = UpdatedAt.From(clock.JstNow);
var updated2 = UpdatedAt.From(clock.JstNow);
Assert.True(updated1.Equals(updated2));  // 値で比較
```

### 3.2 Equals(UpdatedAt? other) メソッド

**役割:** 型安全な UpdatedAt 比較

```csharp
IClock clock = /* DI から注入 */;
var updated1 = UpdatedAt.From(clock.JstNow);
var updated2 = UpdatedAt.From(clock.JstNow);
Assert.True(updated1.Equals(updated2));  // true（同じ時刻なら等価）
```

**比較ロジック:**
- null チェック → false
- 参照同一性チェック → true
- 値同一性チェック（DateTime比較） → 結果

### 3.3 GetHashCode() メソッド

**役割:** HashMap/HashSet 互換のハッシュコード生成

```csharp
IClock clock = /* DI から注入 */;
var updated1 = UpdatedAt.From(clock.JstNow);
var updated2 = UpdatedAt.From(clock.JstNow);
Assert.Equal(updated1.GetHashCode(), updated2.GetHashCode());
```

**特性:**
- 等価性と一貫性を保証
- HashMap/HashSet で正確に機能

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
private UpdatedAt(DateTime? value, bool isSet) : base(value, isSet)
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
// IClock から現在のJST日時でUpdatedAtを生成
private readonly IClock _clock;  // DI で注入

var updatedAt = UpdatedAt.From(_clock.JstNow);

// 日時値の参照
DateTime dt = updatedAt.Value;
Console.WriteLine(updatedAt);  // ISO 8601 形式で出力
```

### 6.2 Entity での更新パターン

```csharp
public class Entity
{
    private readonly IClock _clock;
    public UpdatedAt UpdatedAt { get; private set; }
    
    public Entity(IClock clock)
    {
        _clock = clock;
        UpdatedAt = UpdatedAt.Unset();  // ← 初期状態: 未更新
    }
    
    public void Update(string newName)
    {
        Name = newName;
        UpdatedAt = UpdatedAt.From(_clock.JstNow);  // ← 更新時に新しい日時に更新
    }
}
```

### 6.3 等価性比較

```csharp
private readonly IClock _clock;

var updated1 = UpdatedAt.From(_clock.JstNow);
var updated2 = UpdatedAt.From(_clock.JstNow);

if (updated1 == updated2)
{
	Console.WriteLine("同じ更新日時");
}
```

### 6.4 HashMap/HashSet での使用

```csharp
IClock clock = /* DI から注入 */;
var updateDict = new Dictionary<UpdatedAt, string>();
updateDict.Add(UpdatedAt.From(clock.JstNow), "entity1");

var updateSet = new HashSet<UpdatedAt>();
updateSet.Add(UpdatedAt.From(clock.JstNow));
updateSet.Add(UpdatedAt.Unset());  // Unset も格納可能
```

---

## 7. 実装上の注意点

| 項目 | 説明 |
|---|---|
| **DateTime Kind** | UTC/Local 区別なし。任意の Kind を受け入れる |
| **タイムゾーン** | UpdatedAt 側では管理しない。利用側で統一を推奨 |
| **精度** | DateTime は 100ナノ秒単位の精度を保持 |
| **比較** | Equals は値同一性に基づく（参照同一性ではない） |

---

## 8. IEquatable<UpdatedAt> 実装

UpdatedAt は `IEquatable<UpdatedAt>` を実装し、以下の等価性ルールに従います：

- **同一インスタンス**: `ReferenceEquals` で true
- **IsSet 判定**: IsSet が異なれば異なる
- **値同一**: `ValueField`（DateTime?）の同一性で判定
- **null との比較**: false を返す

---

## まとめ

`UpdatedAt` は DateTime? 値オブジェクトとして、以下を達成します：

✅ **安全性**: DateTime を直接扱わずカプセル化  
✅ **状態管理**: IsSet で「更新済み/未更新」を管理  
✅ **Unset対応**: 未更新状態を `Unset()` で表現  
✅ **検証**: MinValue/MaxValue 排除  
✅ **不変性**: 作成後変更不可  
✅ **等価性**: DateTime? の値同一性と IsSet で判定  
✅ **ハッシング**: HashMap/HashSet 対応  
✅ **スレッドセーフ**: 不変性により同期化不要
