# DeletedAt 詳細設計書

**バージョン:** 1.0  
**作成日:** 2025年  

## 1. ValueObject としての設計

### 1.1 null許容型 ValueObject の定義

`DeletedAt` は **null許容型の ValueObject** として実装されます。

**ValueObject の本質的特性:**
- **値による等価性**: 異なるインスタンスでも、保持する値が同じなら等価
- **不変性**: 一度生成されたら状態を変更できない
- **スレッドセーフ**: 不変性により同期化なしで並行処理に対応
- **null 表現**: null = 未削除、値あり = 削除済み

### 1.2 クラス定義と不変性実装

```csharp
public sealed class DeletedAt : PrimitiveValueObject<DateTime?>, IEquatable<DeletedAt>
```

**null許容型の選択理由:**
- 削除フラグの代わりに、null の有無で状態を表現
- ドメイン的に自然（日時がない = まだ削除されていない）
- ディスク容量節約（boolean フラグ不要）

**不変性の実装方法:**

| 要素 | 実装方法 | 効果 |
|---|---|---|
| **クラス** | sealed | 拡張を禁止し ValueObject の契約を守る |
| **コンストラクタ** | private | ファクトリメソッド経由のみの生成を強制 |
| **フィールド** | readonly | 外部から変更不可 |
| **プロパティ** | get-only | 再代入不可 |
| **メソッド** | 純粋関数 | 状態を変更しない |

**スレッドセーフ性の確保:**
- 不変性により => 複数スレッドからの同時アクセスが安全
- 同期化コード不要 => パフォーマンス向上

### 1.3 クラス継承設計

```csharp
// 基底クラス: PrimitiveValueObject<DateTime?>
// ↓
// DeletedAt （sealed）
```

**継承選択の理由:**
- DateTime? を単一のプリミティブ値として保持
- PrimitiveValueObject の等価性・ハッシング実装を再利用
- ドメイン固有の検証ロジック（MinValue/MaxValue 除外）を追加
- null 許容型に対応

---

## 2. ファクトリメソッドの処理フロー

### 2.1 From メソッド（削除状態作成）

```csharp
public static DeletedAt From(DateTime value) => new(value, true);
```

**処理フロー:**

```
From(DateTime value)
  ↓
  new DeletedAt(value, true)  [コンストラクタ呼び出し]
  ↓
  PrimitiveValueObject<DateTime?> コンストラクタ
    ↓
    Normalize(value)  [自動実行]
    ↓
    Validate(normalized)  [自動実行]
      ↓
      MinValue/MaxValue チェック
      ↓
      ArgumentException スロー（チェック失敗時）
  ↓
  DeletedAt インスタンス返却（Value = 削除日時）
```

**例外処理:**
- `ArgumentException` : DateTime.MinValue または DateTime.MaxValue の場合

**使用シーン:**
- Entity を論理削除: `entity.DeletedAt = DeletedAt.From(_clock.JstNow)`（Entity が IClock を DI 受け取り）
- 削除日時を指定: `DeletedAt.From(localDateTime)`（LocalDateTime から生成）

### 2.2 NotDeleted メソッド（未削除状態作成）

```csharp
public static DeletedAt NotDeleted() => new(null, false);
```

**処理フロー:**

```
NotDeleted()
  ↓
  new DeletedAt(null, false)  [コンストラクタ呼び出し]
  ↓
  Value = null として保持
  ↓
  DeletedAt インスタンス返却（未削除状態）
```

**特性:**
- Value は null （削除日時なし）
- IsSet は false （値未設定）
- IsDeleted は false （削除されていない）

**使用シーン:**
- Entity 初期化時: `entity.DeletedAt = DeletedAt.NotDeleted()`
- 削除取消（再生成）: `entity.DeletedAt = DeletedAt.NotDeleted()`

### 2.3 TryFrom メソッド（nullable対応）

```csharp
public static bool TryFrom(DateTime? input, out DeletedAt result)
{
    if (input == null)
    {
        result = NotDeleted();
        return true;
    }

    try
    {
        result = From(input.Value);
        return true;
    }
    catch (ArgumentException)
    {
        result = null!;
        return false;
    }
}
```

**処理:**
- input == null → `NotDeleted()` 返却、true を返す
- input.HasValue → `From(input.Value)` 呼び出し
- 成功時 true、例外時 false

**使用シーン:**
- ユーザー入力の安全な処理
- 外部 API から受け取る値の検証
- データベースから読み込んだ値の復元

### 2.4 TryFrom メソッド（non-nullable オーバーロード）

```csharp
public static bool TryFrom(DateTime input, out DeletedAt result) 
    => TryFrom((DateTime?)input, out result);
```

**設計意図:**
- DateTime（non-nullable）も受け入れ可能にする
- 内部的には nullable 版へ委譲（DRY 原則）

**使用シーン:**
- null 不可の DateTime 型での検証
- 型安全性を維持しつつ検証可能

---

## 3. プロパティ設計

### 3.1 Value プロパティ

```csharp
public DateTime? Value => ValueField;
```

- PrimitiveValueObject<DateTime?>のprotectedフィールドValueFieldを公開
- get-onlyで読み取り専用を保証
- 外部から変更不可（ValueObject の契約）

**アクセス用途:**
- 削除日時を参照: `entity.DeletedAt.Value` (null or DateTime)
- 日付フォーマット処理: `deletedAt.Value?.ToString("yyyy-MM-dd")`
- データベース書き込み: `entity.DeletedAt.Value`

### 3.2 IsSet プロパティ（継承）

```csharp
public bool IsSet { get; }  // PrimitiveValueObject から継承
```

- 値が設定されているかを判定
- `true` = 削除済み（Value != null）
- `false` = 未削除（Value == null）

### 3.3 IsDeleted プロパティ（拡張）

```csharp
public bool IsDeleted => Value.HasValue;
```

- 論理削除状態を判定するヘルパープロパティ
- ドメイン的により直感的（IsSet と同義だが、名前がビジネス用語）

**使用シーン:**
- 削除状態の判定: `if (entity.IsDeleted) { ... }`
- クエリフィルタリング: `.Where(e => !e.IsDeleted)`

---

## 4. 値による等価性実装

### 4.1 Equals(object?) メソッド

```csharp
public override bool Equals(object? obj) => Equals(obj as DeletedAt);
```

- 汎用オブジェクト比較
- 強い型のEqualsへ委譲

### 4.2 Equals(DeletedAt?) メソッド

```csharp
public bool Equals(DeletedAt? other)
{
    if (other is null) return false;
    if (ReferenceEquals(this, other)) return true;
    
    // nullable な DateTime の比較
    if (ValueField.HasValue != other.ValueField.HasValue)
        return false;
    
    return ValueField == other.ValueField;
}
```

**処理:**
- null チェック
- 参照同一性チェック（パフォーマンス最適化）
- null有無の確認（nullable対応）
- 値同一性チェック（DateTime比較）

**ValueObject の等価性:**
- インスタンスの参照は異なるが、値が同じなら等価
- 例: `new DeletedAt(Date1) == new DeletedAt(Date1)` → true
- 例: `DeletedAt.NotDeleted() == DeletedAt.NotDeleted()` → true

### 4.3 GetHashCode() メソッド

```csharp
public override int GetHashCode() => ValueField.GetHashCode();
```

- `DateTime?.GetHashCode()` に委譲
- HashMap/HashSet の要として機能
- 等価性と一貫性を保証（Equals true ⇒ GetHashCode 同値）
- null の場合も安全にハッシュ化可能（.NET仕様）

**使用シーン:**
- Dictionary/HashSet での格納: `var set = new HashSet<DeletedAt> { deletedAt };`
- GroupBy の キー: `entities.GroupBy(e => e.DeletedAt)`

### 4.4 GetEqualityComponents() メソッド

```csharp
protected override IEnumerable<object?> GetEqualityComponents()
{
    yield return ValueField;
}
```

- 等価性比較に使用するコンポーネントを列挙
- PrimitiveValueObjectの等価性ロジックで使用
- null も含まれる

---

## 5. 検証ロジック設計

### 5.1 Validate() メソッド

```csharp
protected override void Validate(DateTime? normalized)
{
    base.Validate(normalized);

    // null は許可（未削除状態）
    if (normalized == null)
        return;

    // 有効な DateTime のみ検証
    if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
    {
        throw new ArgumentException(
            $"DeletedAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}."
        );
    }
}
```

**検証フロー:**

```
Validate(DateTime? normalized)
  ↓
  base.Validate(normalized)  [基底クラスの検証]
  ↓
  normalized == null ?
    ├─ true → OK（未削除状態）
    └─ false
      ↓
      normalized == DateTime.MinValue ?
        ├─ true → ArgumentException スロー
        └─ false
          ↓
          normalized == DateTime.MaxValue ?
            ├─ true → ArgumentException スロー
            └─ false → OK
```

**検証の意図:**
- null は許可（未削除状態の正当な表現）
- DateTime.MinValue/MaxValue は「無効な日時」として扱う
- 実用的な日時範囲（0001-01-01 00:00:01 ～ 9999-12-31 23:59:58）のみを受け入れ
- ドメイン固有ルールの実装例

**検除外事項:**
- ✗ 未来の日時は許可（削除予約など対応可能）
- ✗ 過去の日時も許可（過去に削除したレコード復元対応）
- ✗ UTC/Local 区別なし（タイムゾーン管理は利用側へ）

---

## 6. ValueObject としてのコントラクト保証

### 6.1 不変性の契約

**保証:**
- 一度生成されたら変更不可
- プロパティは get-only
- メソッドは副作用なし

**違反検出:**
- コンパイラ：Value プロパティへの再代入で即座に検出
- テスト：不変性テストで実行時に検証

### 6.2 値による等価性の契約

**保証:**
- 異なるインスタンスでも値が同じなら等価
- `a.Equals(b) == (a.Value == b.Value)`

**違反検出:**
- テスト：等価性テストで検証
- Runtime：GetHashCode/Equals 一貫性チェック

### 6.3 スレッドセーフネスの契約

**保証:**
- 不変性により同期化不要
- 複数スレッドからの自由なアクセスが安全

**違反検出:**
- 設計レビュー：コンストラクタ/プロパティが不変であることを確認
- テスト：並行処理テストで実行時に検証

---

## 7. ソフト削除パターンの実装

### 7.1 Entity での使用パターン

```csharp
public class Entity : IAggregateRoot
{
    private readonly IClock _clock;
    public long Id { get; private set; }
    public DeletedAt DeletedAt { get; private set; }
    
    // Entity 生成時に IClock を注入
    public Entity(IClock clock)
    {
        _clock = clock;
        DeletedAt = DeletedAt.NotDeleted();
    }
    
    // 論理削除操作
    public void SoftDelete()
    {
        if (!DeletedAt.IsDeleted)
        {
            DeletedAt = DeletedAt.From(_clock.JstNow);  // ← IClock を使用
        }
    }
    
    // 論理削除状態の確認
    public bool IsDeleted => DeletedAt.IsDeleted;
}
```

### 7.2 Repository でのフィルタリング

```csharp
public class EntityRepository : IEntityRepository
{
    // アクティブなエンティティのみ取得
    public async Task<List<Entity>> GetActiveAsync()
    {
        return await _context.Entities
            .Where(e => !e.DeletedAt.IsDeleted)
            .ToListAsync();
    }
    
    // 削除済みエンティティのみ取得
    public async Task<List<Entity>> GetDeletedAsync()
    {
        return await _context.Entities
            .Where(e => e.DeletedAt.IsDeleted)
            .ToListAsync();
    }
    
    // 削除期間を指定して取得
    public async Task<List<Entity>> GetDeletedBetweenAsync(
        DateTime startDate, DateTime endDate)
    {
        return await _context.Entities
            .Where(e => e.DeletedAt.IsDeleted && 
                       e.DeletedAt.Value >= startDate && 
                       e.DeletedAt.Value <= endDate)
            .ToListAsync();
    }
}
```

### 7.3 クエリ時の削除状態フィルタリング

```csharp
// 通常のビジネスロジック（削除済みを除外）
var specification = Specification<Entity>
    .Create()
    .And(e => !e.DeletedAt.IsDeleted)
    .Build();

// 管理者機能（削除済みのみ表示）
var adminSpecification = Specification<Entity>
    .Create()
    .And(e => e.DeletedAt.IsDeleted)
    .Build();
```

---

## 8. DateTime型の特性と対応

| 特性 | 対応 | 理由 |
|---|---|---|
| **Kind（タイムゾーン情報）** | 区別しない（任意のKindを受け入れ） | DeletedAtは時刻のみを保持 |
| **タイムゾーン管理** | DeletedAt側では管理しない | 利用側で統一を推奨（UTC推奨） |
| **精度** | 100ナノ秒単位の精度を保持 | DateTime の仕様に従う |
| **文字列化** | ISO 8601形式を返す | 国際標準の採用 |
| **null**| nullable DateTime として安全に扱う | 削除/未削除の表現 |

---

## 9. CreatedAt/UpdatedAt との関係

| ValueObject | 役割 | null許容 | 更新頻度 | 用途 |
|---|---|---|---|---|
| **CreatedAt** | 作成日時 | × | なし | 作成タイミング記録 |
| **UpdatedAt** | 更新日時 | × | 高い | 最終更新タイミング記録 |
| **DeletedAt** | 削除日時 | ✓ | ほぼなし | 論理削除時刻記録 |

---

## 10. 削除フラグ廃止による利点

### 10.1 従来のフラグ方式との比較

```csharp
// 従来の方式（削除フラグ）
public class OldEntity
{
    public bool IsDeleted { get; set; }  // フラグのみ
}

// 問題点:
// - 削除日時が記録されない
// - 誰がいつ削除したのか不明
// - 監査ログとの連携が別途必要

// 新しい方式（DeletedAt）
public class NewEntity
{
    public DeletedAt DeletedAt { get; set; }  // 日時で状態・監査情報を兼ねる
}

// 利点:
// - 削除日時が自動記録される
// - 削除者は別の監査フィールドで管理
// - 削除タイムスタンプが内包されている
```

---

## 11. メモリ効率とGCへの考慮

- DeletedAtは value type（DateTime?）を保持するsealed class
- ValueObject は参照型だが、イミュータブルなためGCの圧力が低い
- ファクトリメソッドで新規インスタンス生成のみ（リサイクル不可）
- 長寿命オブジェクトとしての使用に適す

---

## 12. 実装上の注意点

| 項目 | 説明 |
|---|---|
| **null の意味** | 削除されていない（删除フラグ不要） |
| **値の意味** | 削除された日時 |
| **DateTime Kind** | UTC/Local 区別なし。UTC 推奨 |
| **自動生成** | From(DateTime.UtcNow)で現在UTC日時を使用推奨 |
| **等価性演算子** | == / != は自動的に Equals に委譲される（.NET仕様） |
| **削除取消** | 一度削除後の取消は新規 NotDeleted() 生成により実現 |
| **削除権限** | 削除操作の権限チェックは Entity/Use Case 層で実施 |

---

## 13. 削除復旧シナリオ

### 13.1 削除の取消（復旧）

```csharp
public void RestoreEntity(Entity entity)
{
    if (entity.IsDeleted)
    {
        // 削除日時をクリア（未削除状態に戻す）
        entity.DeletedAt = DeletedAt.NotDeleted();
    }
}
```

**設計上の考慮:**
- DeletedAt の値を直接変更することはできない（不変性）
- 取消を操作として明示的に行うことで、意図を明確化
- 復旧のため、DeletedAt を再度 NotDeleted() で生成

---

## 14. DDD との関連

### 14.1 ValueObject としての位置づけ

```
Domain Driven Design
  ↓
ValueObject (不変性 + 値による等価性)
  ↓
DeletedAt (DateTime? + ソフト削除ドメインロジック)
```

### 14.2 Entity と ValueObject の区別

| 特性 | Entity | ValueObject |
|---|---|---|
| **同一性** | ID で判定 | 値で判定 |
| **等価性** | 参照 or ID | 値 |
| **可変性** | 可変 | 不変 |
| **ライフサイクル** | 独立 | Entity に属す |
| **例** | Aggregate Root | DeletedAt, Amount |

### 14.3 Bounded Context での使用

**SharedKernel に配置される理由:**
- プロジェクト全体で共通する削除情報
- 複数の Bounded Context で使用
- Domain Model に依存しない基盤型

```
SharedKernel
  ├── ValueObjects
  │   └── Audit
  │       ├── CreatedAt
  │       ├── UpdatedAt
  │       └── DeletedAt
  
→ どの Bounded Context でも参照可能
```

---

## 15. まとめ

DeletedAt の詳細設計の要点：

✅ **ValueObject の本質**: 不変性と値による等価性を実装  
✅ **sealed class** により継承を禁止し ValueObject 契約を守る  
✅ **null許容型** で削除/未削除を自然に表現  
✅ **private constructor** でファクトリメソッド経由のみの生成を強制  
✅ **Validate** で MinValue/MaxValue を除外  
✅ **IEquatable** で効率的な値による等価性比較  
✅ **GetHashCode** で HashMap/HashSet 互換を確保  
✅ **スレッドセーフ**: 不変性により同期化コード不要  
✅ **DDD 準拠**: ValueObject としての形式を完全に実装  
✅ **ソフト削除**: 削除フラグ廃止で監査情報を内包
