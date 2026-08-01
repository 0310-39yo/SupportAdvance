# UpdatedAt 詳細設計書

**バージョン:** 1.0  
**作成日:** 2025年  

## 1. ValueObject としての設計

### 1.1 ValueObject の定義

`UpdatedAt` は **ValueObject** として実装されます。

**ValueObject の本質的特性:**
- **値による等価性**: 異なるインスタンスでも、保持する値が同じなら等価
- **不変性**: 一度生成されたら状態を変更できない
- **スレッドセーフ**: 不変性により同期化なしで並行処理に対応

### 1.2 クラス定義と不変性実装

```csharp
public sealed class UpdatedAt : PrimitiveValueObject<DateTime?>, IEquatable<UpdatedAt>
```

**IsSet で状態管理:**
- `IsSet = true`: 更新済み
- `IsSet = false`: 未更新（`Unset()` で生成）

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
// UpdatedAt （sealed）
```

**継承選択の理由:**
- DateTime を単一のプリミティブ値として保持
- PrimitiveValueObject の等価性・ハッシング実装を再利用
- ドメイン固有の検証ロジック（MinValue/MaxValue 除外）を追加

---

## 2. ファクトリメソッドの処理フロー

### 2.1 From メソッド（推奨）

```csharp
public static UpdatedAt From(LocalDateTime value) => new(value.Value);
```

**処理フロー:**

```
From(LocalDateTime value)  ← IClock.JstNow から取得
  ↓
  new UpdatedAt(value.Value, true)  [コンストラクタ呼び出し]
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
  UpdatedAt インスタンス返却
```

**例外処理:**
- `ArgumentException` : DateTime.MinValue または DateTime.MaxValue の場合

### 2.2 Unset メソッド（未更新状態）

```csharp
public static UpdatedAt Unset() => new(null, isSet: false);
```

**役割:** 未更新状態の UpdatedAt を生成

### 2.3 TryFrom メソッド（推奨・nullable対応）

```csharp
public static bool TryFrom(LocalDateTime? input, out UpdatedAt result)
{
	// null が来たら Unset を返す（成功）
	if (input == null || !input.HasValue)
	{
		result = Unset();
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
- input.HasValue確認
- null → false返却
- From呼び出し → 成功時true、例外時false

### 2.3 TryFrom メソッド（non-nullable オーバーロード）

```csharp
public static bool TryFrom(LocalDateTime input, out UpdatedAt result) 
	=> TryFrom((LocalDateTime?)input, out result);
```

**設計意図:**
- DateTime （non-nullable）も受け入れ可能にする
- 内部的には nullable 版へ委譲（DRY 原則）

---

## 3. プロパティ設計

### 3.1 Value プロパティ

```csharp
public DateTime Value => ValueField;
```

- PrimitiveValueObject<DateTime>のprotectedフィールドValueFieldを公開
- get-onlyで読み取り専用を保証
- 外部から変更不可（ValueObject の契約）

---

## 4. 値による等価性実装

### 4.1 Equals(object?) メソッド

```csharp
public override bool Equals(object? obj) => Equals(obj as UpdatedAt);
```

- 汎用オブジェクト比較
- 強い型のEqualsへ委譲

### 4.2 Equals(UpdatedAt?) メソッド

```csharp
public bool Equals(UpdatedAt? other)
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
- 例: `new UpdatedAt(Date1) == new UpdatedAt(Date1)` → true

### 4.3 GetHashCode() メソッド

```csharp
public override int GetHashCode() => ValueField.GetHashCode();
```

- DateTime.GetHashCode() に委譲
- HashMap/HashSet の要として機能
- 等価性と一貫性を保証（Equals true ⇒ GetHashCode 同値）

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
			$"UpdatedAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}."
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
| **Kind（タイムゾーン情報）** | 区別しない（任意のKindを受け入れ） | UpdatedAtは時刻のみを保持 |
| **タイムゾーン管理** | UpdatedAt側では管理しない | 利用側で統一を推奨 |
| **精度** | 100ナノ秒単位の精度を保持 | DateTime の仕様に従う |
| **文字列化** | ISO 8601形式を返す | 国際標準の採用 |

---

## 8. メモリ効率とGCへの考慮

- UpdatedAtはvalue type（DateTime）を保持するsealed class
- ValueObject は参照型だが、イミュータブルなためGCの圧力が低い
- ファクトリメソッドで新規インスタンス生成のみ（リサイクル不可）
- 長寿命オブジェクトとしての使用に適す

---

## 9. 実装上の注意点

| 項目 | 説明 |
|---|---|
| **null許容** | TryFromではnull許容、FromではArgumentException |
| **例外タイプ** | ArgumentExceptionのみ使用 |
| **ログ出力** | UpdatedAt側では行わない |
| **自動生成** | IClock.JstNowを経由して日時を取得し、From()で生成。DateTime.UtcNow/DateTime.Nowの直接使用は禁止（Clock実装内部のみ許可） |
| **等価性演算子** | == / != は自動的に Equals に委譲される（.NET仕様） |

---

## 10. DDD との関連

### 10.1 ValueObject としての位置づけ

```
Domain Driven Design
  ↓
ValueObject (不変性 + 値による等価性)
  ↓
UpdatedAt (DateTime代替 + 検証ロジック)
```

### 10.2 Entity と ValueObject の区別

| 特性 | Entity | ValueObject |
|---|---|---|
| **同一性** | ID で判定 | 値で判定 |
| **等価性** | 参照 or ID | 値 |
| **可変性** | 可変 | 不変 |
| **ライフサイクル** | 独立 | Entity に属す |
| **例** | Aggregate Root | UpdatedAt, Amount |

---

## 11. まとめ

UpdatedAtの詳細設計の要点：

✅ **ValueObject の本質**: 不変性と値による等価性を実装  
✅ **sealed class** により継承を禁止し ValueObject 契約を守る  
✅ **private constructor** でファクトリメソッド経由のみの生成を強制  
✅ **Validate** で MinValue/MaxValue を除外  
✅ **IEquatable** で効率的な値による等価性比較  
✅ **GetHashCode** で HashMap/HashSet 互換を確保  
✅ **スレッドセーフ**: 不変性により同期化コード不要  
✅ **DDD 準拠**: ValueObject としての形式を完全に実装
