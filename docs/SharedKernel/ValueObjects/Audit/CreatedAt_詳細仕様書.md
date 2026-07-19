# CreatedAt 詳細仕様書

**バージョン:** 1.0  
**作成日:** 2025年  

## 1. ValueObject としての設計

### 1.1 ValueObject の定義

`CreatedAt` は **ValueObject** として実装されます。

**ValueObject の本質的特性:**
- **値による等価性**: 異なるインスタンスでも、保持する値が同じなら等価
- **不変性**: 一度生成されたら状態を変更できない
- **スレッドセーフ**: 不変性により同期化なしで並行処理に対応
- **一度限りの生成**: 作成日時は Entity 生成時に固定される

### 1.2 クラス定義と不変性実装

```csharp
public sealed class CreatedAt : PrimitiveValueObject<DateTime>, IEquatable<CreatedAt>
```

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
// 基底クラス: PrimitiveValueObject<DateTime>
// ↓
// CreatedAt （sealed）
```

**継承選択の理由:**
- DateTime を単一のプリミティブ値として保持
- PrimitiveValueObject の等価性・ハッシング実装を再利用
- ドメイン固有の検証ロジック（MinValue/MaxValue 除外）を追加

---

## 2. ファクトリメソッドの処理フロー

### 2.1 From メソッド（推奨）

```csharp
public static CreatedAt From(LocalDateTime value) => new(value.Value);
```

**処理フロー:**

```
From(LocalDateTime value)  ← IClock.JstNow から取得
  ↓
  new CreatedAt(value.Value, true)  [コンストラクタ呼び出し]
  ↓
  PrimitiveValueObject<DateTime> コンストラクタ
    ↓
    Normalize(value.Value)  [自動実行]
    ↓
    Validate(normalized)  [自動実行]
      ↓
      MinValue/MaxValue チェック
      ↓
      ArgumentException スロー（チェック失敗時）
  ↓
  CreatedAt インスタンス返却
```

**例外処理:**
- `ArgumentException` : DateTime.MinValue または DateTime.MaxValue の場合

**使用シーン:**
- Entity 生成時に IClock から取得した現在時刻を記録: `CreatedAt.From(_clock.JstNow)`
- データベースから読み込んだ値を復元: `CreatedAt.From(new LocalDateTime(dbValue))`

### 2.2 TryFrom メソッド（推奨・nullable対応）

```csharp
public static bool TryFrom(LocalDateTime? input, out CreatedAt result)
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
    catch (ArgumentException)
    {
        result = null!;
        return false;
    }
}
```

**処理:**
- input.HasValue確認（IClock.JstNow は常に値あり）
- null → false返却
- From呼び出し → 成功時true、例外時false

**使用シーン:**
- ユーザー入力の安全な処理（LocalDateTime として変換後）
- 外部 API から受け取る値の検証（LocalDateTime に変換後）
- null 許容の API との連携

---

## 3. プロパティ設計

### 3.1 Value プロパティ

```csharp
public DateTime Value => ValueField;
```

- PrimitiveValueObject<DateTime>のprotectedフィールドValueFieldを公開
- get-onlyで読み取り専用を保証
- 外部から変更不可（ValueObject の契約）

**アクセス用途:**
- Entity の作成日時を参照: `entity.CreatedAt.Value`
- 日付フォーマット処理: `createdAt.Value.ToString("yyyy-MM-dd")`
- データベース書き込み: `entity.CreatedAt.Value`

### 3.2 IsSet プロパティ（継承）

```csharp
public bool IsSet { get; }  // PrimitiveValueObject から継承
```

- CreatedAt は常に `true`（必ず値を持つ）
- 監査情報として、作成日時は必須フィールド

---

## 4. 値による等価性実装

### 4.1 Equals(object?) メソッド

```csharp
public override bool Equals(object? obj) => Equals(obj as CreatedAt);
```

- 汎用オブジェクト比較
- 強い型のEqualsへ委譲

**使用シーン:**
- Collection の Contains チェック
- テスト時の比較

### 4.2 Equals(CreatedAt?) メソッド

```csharp
public bool Equals(CreatedAt? other)
{
    if (other is null) return false;
    if (ReferenceEquals(this, other)) return true;
    return ValueField == other.ValueField;
}
```

**処理:**
- null チェック
- 参照同一性チェック（パフォーマンス最適化）
- 値同一性チェック（DateTime比較）

**ValueObject の等価性:**
- インスタンスの参照は異なるが、値が同じなら等価
- 例: `new CreatedAt(Date1) == new CreatedAt(Date1)` → true

### 4.3 GetHashCode() メソッド

```csharp
public override int GetHashCode() => ValueField.GetHashCode();
```

- DateTime.GetHashCode() に委譲
- HashMap/HashSet の要として機能
- 等価性と一貫性を保証（Equals true ⇒ GetHashCode 同値）

**使用シーン:**
- Dictionary/HashSet での格納: `var set = new HashSet<CreatedAt> { createdAt };`
- GroupBy の キー: `entities.GroupBy(e => e.CreatedAt)`

### 4.4 GetEqualityComponents() メソッド

```csharp
protected override IEnumerable<object?> GetEqualityComponents()
{
    yield return ValueField;
}
```

- 等価性比較に使用するコンポーネントを列挙
- PrimitiveValueObjectの等価性ロジックで使用

---

## 5. 検証ロジック設計

### 5.1 Validate() メソッド

```csharp
protected override void Validate(DateTime normalized)
{
    base.Validate(normalized);

    if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
    {
        throw new ArgumentException(
            $"CreatedAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}."
        );
    }
}
```

**検証フロー:**

```
Validate(DateTime normalized)
  ↓
  base.Validate(normalized)  [基底クラスの検証]
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
- DateTime.MinValue/MaxValue は「無効な日時」として扱う
- 実用的な日時範囲（0001-01-01 00:00:01 ～ 9999-12-31 23:59:58）のみを受け入れ
- ドメイン固有ルールの実装例

**検証除外事項:**
- ✗ 未来の日時は許可（予約システムなど対応）
- ✗ 過去の日時も許可（レガシーデータの復元対応）
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

## 7. DateTime型の特性と対応

| 特性 | 対応 | 理由 |
|---|---|---|
| **Kind（タイムゾーン情報）** | 区別しない（任意のKindを受け入れ） | CreatedAtは時刻のみを保持 |
| **タイムゾーン管理** | CreatedAt側では管理しない | 利用側で統一を推奨（UTC推奨） |
| **精度** | 100ナノ秒単位の精度を保持 | DateTime の仕様に従う |
| **文字列化** | ISO 8601形式を返す | 国際標準の採用 |

---

## 8. CreatedAt の生ライフサイクル

### 8.1 生成フェーズ

```
Entity 生成
  ↓
  IClock.JstNow を取得
  ↓
  CreatedAt.From(_clock.JstNow)  ← 唯一の生成タイミング
  ↓
  Entity に保持
```

**特性:**
- Entity の生成時に一度だけ生成
- 以降、変更されない（不変）
- 監査証跡として機能

### 8.2 参照フェーズ

```
Entity ライフサイクル中
  ↓
  CreatedAt.Value で参照
  ↓
  等価性判定
  ↓
  ハッシング
```

**用途:**
- 作成日時を表示（UI）
- 期間検索（作成日 ≥ X）
- 統計処理（作成数/日）

### 8.3 永続化フェーズ

```
データベース保存
  ↓
  CreatedAt.Value を DateTime として保存
  ↓
  読み込み時に CreatedAt.From(new LocalDateTime(dbValue)) で復元
```

---

## 9. 集約ルートとの関係

### 9.1 CreatedAt の配置

```
Aggregate Root (Entity)
  ↓
  CreatedAt (ValueObject) ... 不変の作成日時
  ↓
  UpdatedAt (ValueObject) ... 更新可能な更新日時
```

**責任分離:**
- **Entity**: 識別子を持つ、ライフサイクルを持つ
- **CreatedAt**: 作成日時のみ保持、不変
- **UpdatedAt**: 更新日時を保持、可変（Entity によって更新）

### 9.2 CreatedAt の使用方法

```csharp
// 正しい Entity の初期化パターン
public class Entity
{
    private readonly IClock _clock;  // DI で注入
    public CreatedAt CreatedAt { get; }
    
    public Entity(IClock clock)
    {
        _clock = clock;
        // IClock.JstNow を使用して CreatedAt を生成
        CreatedAt = CreatedAt.From(_clock.JstNow);  // ← 推奨
    }
}

// 参照方法
var entity = aggregate.GetEntity();
DateTime createdTime = entity.CreatedAt.Value;  // ValueObject 経由で参照

// 誤り
// DateTime createdTime = DateTime.UtcNow;  // ← 直接取得は禁止
// CreatedAt createdAt = CreatedAt.From(DateTime.Now);  // ← 非推奨
```

---

## 10. DDD との関連

### 10.1 ValueObject としての位置づけ

```
Domain Driven Design
  ↓
ValueObject (不変性 + 値による等加性)
  ↓
CreatedAt (DateTime代替 + 検証ロジック)
```

### 10.2 Entity と ValueObject の区別

| 特性 | Entity | ValueObject |
|---|---|---|
| **同一性** | ID で判定 | 値で判定 |
| **等価性** | 参照 or ID | 値 |
| **可変性** | 可変 | 不変 |
| **ライフサイクル** | 独立 | Entity に属す |
| **例** | Aggregate Root | CreatedAt, Amount |

### 10.3 Bounded Context での使用

**SharedKernel に配置される理由:**
- プロジェクト全体で共通する監査情報
- 複数の Bounded Context で使用
- Domain Model に依存しない基盤型

```
SharedKernel
  ├── ValueObjects
  │   └── Audit
  │       ├── CreatedAt
  │       └── UpdatedAt
  
→ どの Bounded Context でも参照可能
```

---

## 11. 実装上の注意点

| 項目 | 説明 |
|---|---|
| **null許容** | TryFromではnull許容、FromではArgumentException |
| **例外タイプ** | ArgumentExceptionのみ使用 |
| **ログ出力** | CreatedAt側では行わない |
| **自動生成** | From(DateTime.UtcNow)で現在UTC日時を使用推奨 |
| **等価性演算子** | == / != は自動的に Equals に委譲される（.NET仕様） |
| **初期化** | Entity 生成時に必須フィールド |

---

## 12. メモリ効率とGCへの考慮

- CreatedAtはvalue type（DateTime）を保持するsealed class
- ValueObject は参照型だが、イミュータブルなためGCの圧力が低い
- ファクトリメソッドで新規インスタンス生成のみ（リサイクル不可）
- 長寿命オブジェクトとしての使用に適す

---

## 13. UpdatedAt / DeletedAt との違い

| 特性 | CreatedAt | UpdatedAt | DeletedAt |
|---|---|---|---|
| **型** | `DateTime` | `DateTime?` | `DateTime?` |
| **生成タイミング** | Entity 生成時のみ | Entity 生成・更新時 | 削除時のみ |
| **可変性** | 不変（変更不可） | 可変（更新可能） | 実質不変（削除後変更なし） |
| **IsSet管理** | 常に true | IsSet で管理 | IsSet で管理 |
| **Unset状態** | なし | `Unset()` で表現 | `Unset()` で表現 |
| **ビジネスロジック** | 監査情報 | 監査情報・版管理 | 論理削除 |

---

## 14. まとめ

CreatedAt の詳細仕様の要点：

✅ **ValueObject の本質**: 不変性と値による等価性を実装  
✅ **sealed class** により継承を禁止し ValueObject 契約を守る  
✅ **private constructor** でファクトリメソッド経由のみの生成を強制  
✅ **Validate** で MinValue/MaxValue を除外  
✅ **IEquatable** で効率的な値による等価性比較  
✅ **GetHashCode** で HashMap/HashSet 互換を確保  
✅ **スレッドセーフ**: 不変性により同期化コード不要  
✅ **DDD 準拠**: ValueObject としての形式を完全に実装  
✅ **一度限りの生成**: Entity ライフサイクル上で変更不可の監査情報
