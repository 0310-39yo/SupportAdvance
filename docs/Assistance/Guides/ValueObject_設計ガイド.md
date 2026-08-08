# ValueObject 設計ガイド — プロジェクト全体指針

**プロジェクト:** SupportAdvance  
**レイヤ:** SharedKernel / Domain 層  
**種別:** 設計ガイド  
**版:** 2.0 / 2026-08-08

---

## 📋 目次

1. [概要](#1-概要)
2. [ValueObject の階層構造](#2-valueobject-の階層構造)
3. [設計の基本原則](#3-設計の基本原則)
4. [状態管理パターン](#4-状態管理パターンisset-と-unset)
5. [実装パターン](#5-実装パターン)
6. [命名規約](#6-命名規約)
7. [よくある実装パターン](#7-よくある実装パターン)
8. [ファクトリメソッド設計](#8-ファクトリメソッド設計)
9. [等価性判定の設計](#9-等価性判定の設計)
10. [検証・正規化の責務分離](#10-検証正規化の責務分離)
11. [ValueObject パターン別ガイド](#11-valueobject-パターン別ガイド)
12. [レイヤ制約](#12-レイヤ制約)
13. [チェックリスト](#13-チェックリスト-新しい-valueobject-を実装するとき)

---

## 1. 概要

### 1.1 ValueObject とは

ValueObject は **値による等価性を持つ不変オブジェクト** であり、Entity と異なりアイデンティティを持たない。同じ値を持つ 2 つの ValueObject は等価と見なされる。

**特徴:**
- **不変性** — 生成後に値を変更しない（`readonly` フィールド）
- **値による等価性** — 内部値が同じなら同じオブジェクト
- **Unset 対応** — `null` に頼らず型で未設定状態を表現
- **ビジネスロジック** — 正規化・検証・フォーマットを内包

### 1.2 このガイドの位置付け

このガイドは現在の実装（ValueObject v1 設計）に基づき、すべての ValueObject 派生クラスが遵守すべき **設計ルール と実装パターン** を定義する。

参考ドキュメント：
- `ValueObject_技術仕様書.md` — 技術的な詳細（`GetEqualityComponents` の動作、ハッシュコード計算など）
- `PrimitiveValueObject_技術仕様書.md` — スカラ値の実装細節
- `EnumValueObject_技術仕様書.md` — 選択肢型の実装細節

---

## 2. ValueObject の階層構造

```
ValueObject（抽象）
  ├─ PrimitiveValueObject<TValue>（スカラ値：非ID属性）
  │   ├─ CreatedAt, UpdatedAt, DeletedAt（監査）
  │   ├─ RespondentName, RespondentAge（ビジネス属性）
  │   └─ {その他スカラ値型}
  │
  ├─ AggregateId（抽象）【新規】集約の論理的識別子（GUID ベース）
  │   ├─ UserId, RoleId, UserRoleId（Identity Bounded Context）
  │   ├─ EmployeeId, DepartmentId（Employee Bounded Context）
  │   └─ {その他集約ID}
  │
  ├─ ValueObject（特殊パターン）【新規】複合型・物理キー
  │   ├─ RowId（テーブル物理キー：long値、value=0で未採番）
  │   ├─ FullName（複合型：FirstName + LastName）
  │   └─ {その他複合型}
  │
  └─ EnumValueObject<TValue>（選択肢型：固定選択肢集合）
      ├─ Status（bool/int/enum）
      ├─ Priority, Category（例）
      └─ {その他選択肢型}
```

### 2.1 ValueObject（基底抽象クラス）

**責務:**
- すべての ValueObject に対して **等価性判定** を提供（`Equals()`, `==` 演算子）
- **未設定状態（IsSet）** を型で管理
- **ハッシュコード** 計算（等価性に基づく）
- **文字列表現** 生成（デバッグ用）

**核となるメソッド:**
- `Equals(ValueObject? other)` — 等価性判定（派生クラスの `GetValueComponents()` を呼び出す）
- `GetEqualityComponents()` — IsSet + 値コンポーネントを列挙（派生クラスが `GetValueComponents()` を実装）
- `GetHashCode()` — すべてのコンポーネントに基づくハッシュ計算
- `ToString()` — IsSet=false なら "Unset"、true なら値に基づく文字列

### 2.2 PrimitiveValueObject&lt;TValue&gt;（スカラ値の基底）

**責務:**
- 単一の値を保持（`ValueField`）
- 値の **正規化（Normalize）** — トリム、大文字小文字統一、単位変換など
- 値の **検証（Validate）** — ビジネスルールに基づく妥当性チェック
- 値の **フォーマット（Format）** — 文字列表現のカスタマイズ

**設計:**
```csharp
public abstract class PrimitiveValueObject<TValue> : ValueObject
{
    protected readonly TValue ValueField;

    // Unset 状態用
    protected PrimitiveValueObject(bool isSet) { ... }

    // 値を持つ状態用
    protected PrimitiveValueObject(TValue value, bool isSet)
    {
        // Normalize → Validate の順序で自動実行
    }

    // 派生クラスでオーバーライド（optional）
    protected virtual TValue Normalize(TValue input) => input;
    public virtual void Validate(TValue normalized) { }
    protected virtual string Format(TValue value) => value?.ToString() ?? string.Empty;

    // 値への安全なアクセス
    public bool TryGetValue(out TValue value) { ... }
}
```

**実装例：CreatedAt**
```csharp
public sealed class CreatedAt : PrimitiveValueObject<DateTime>, IEquatable<CreatedAt>
{
    private CreatedAt(DateTime value) : base(value, true) { }

    public static CreatedAt From(DateTime value) => new(value);

    public DateTime Value => ValueField;

    public override void Validate(DateTime normalized)
    {
        // DateTime.MinValue / MaxValue は除外
        if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
        {
            throw new ArgumentException("CreatedAt must be a valid system timestamp.");
        }
    }
}
```

### 2.3 AggregateId（抽象）（集約ビジネスID の基底）

**責務:**
- **GUID 値の保持** — 集約を一意識別
- **型安全性** — 異なる集約 ID を型チェックで区別（UserId ≠ RoleId）
- **等価性判定** — GUID ベースの値比較
- **Unset 状態なし** — 常に値を持つ（null 不許容）

**設計:**
```csharp
public abstract class AggregateId : ValueObject
{
    /// <summary>
    /// GUID 値（Guid.Empty は許可されない）
    /// </summary>
    public Guid Value { get; protected set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">GUID 値</param>
    /// <exception cref="ArgumentException">value が Guid.Empty の場合</exception>
    protected AggregateId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AggregateId cannot be empty.", nameof(value));
        Value = value;
    }

    // 派生クラスでオーバーライド（通常は不要）
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }
}
```

**実装例：UserId**
```csharp
public sealed class UserId : AggregateId
{
    private UserId(Guid value) : base(value) { }

    /// <summary>新しい ID を生成（DB 永続化前）</summary>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>既存の GUID から ID を生成（DB 読み込み時）</summary>
    public static UserId From(Guid value) => new(value);
}
```

**特徴:**
- ✅ IsSet フラグなし（常に値を持つ）
- ✅ Unset() メソッドなし（必須フィールド）
- ✅ Normalize / Validate 不要（GUID は構造化済み）
- ✅ Entity の識別子として使用（`AggregateRoot<TId>` パターン）
- ✅ 複数テーブル集約の論理的統一 ID

**使用パターン（Entity<TId>）:**
```csharp
public class User : AggregateRoot<UserId>  // ← TId = UserId
{
    private RowId _rowId;  // ← テーブル物理キー（別途管理）
    
    public User(UserId id, string name, RowId? rowId = null)
    {
        Id = id;  // UserId で識別
        _rowId = rowId ?? RowId.New();
    }
}
```

---

### 2.4 EnumValueObject&lt;TValue&gt;（選択肢型の基底）

**責務:**
- 選択肢の **内部値** を保持（`ValueField`）
- 選択肢の **妥当性を検証** （`Validate(TValue value)`）
- 選択肢の **業務名称を管理** （`GetDisplayName()`）

**設計:**
```csharp
public abstract class EnumValueObject<TValue> : ValueObject
    where TValue : struct  // int, bool, byte など
{
    protected readonly TValue ValueField;

    // Unset 用
    protected EnumValueObject() : this(default, false) { }

    // 値を持つ状態用（IsSet=true で自動初期化）
    protected EnumValueObject(TValue value) : this(value, true) { }

    // 派生クラスで実装必須
    public abstract void Validate(TValue value);  // 無効な値なら例外
    protected abstract string GetDisplayName();   // ValueField → 日本語名

    // 値への安全なアクセス
    public bool TryGetValue(out TValue value) { ... }
}
```

**実装例パターン:**
```csharp
public sealed class RespondentStatus : EnumValueObject<int>
{
    public static readonly RespondentStatus Active = new(1);
    public static readonly RespondentStatus Inactive = new(2);
    public static readonly RespondentStatus Unset = new();

    private RespondentStatus(int value) : base(value) { }

    public override void Validate(int value)
    {
        if (value < 1 || value > 2)
            throw new ArgumentOutOfRangeException(nameof(value), "Invalid status value.");
    }

    protected override string GetDisplayName() => ValueField switch
    {
        1 => "有効",
        2 => "無効",
        _ => "不明"
    };
}
```

---

## 3. 設計の基本原則

### 3.1 値による等価性

**ルール:** 2 つの ValueObject が同じ値を持つなら、等価と見なす。

```csharp
var name1 = RespondentName.From("山田太郎");
var name2 = RespondentName.From("山田太郎");

Assert.AreEqual(name1, name2);  // true（参照ではなく値で比較）
Assert.AreEqual(name1.GetHashCode(), name2.GetHashCode());  // true
```

**実装側の責務:**
- `GetValueComponents()` で比較対象のすべての値フィールドを yield return する
- `GetHashCode()` は ValueObject の基底実装が自動計算するため、派生クラスでオーバーライド不要
- `Equals()` も基底実装を使用（派生クラスでオーバーライド不要）

### 3.2 不変性の強制

**ルール:** 一度生成された ValueObject は変更されない。

```csharp
public sealed class RespondentName : PrimitiveValueObject<string>
{
    // コンストラクタは private
    private RespondentName(string value) : base(value, true) { }

    // ファクトリメソッドで生成を制御
    public static RespondentName From(string value) => new(value);

    // プロパティは get のみ
    public string Value => ValueField;

    // セッター、変更メソッドは存在しない
}
```

**チェックリスト:**
- ✅ コンストラクタは `private` または `protected`
- ✅ フィールドは `readonly`
- ✅ プロパティは get のみ（セッター不可）
- ✅ 変更メソッド（`Set*()`, `Update*()` など）不可

### 3.3 Null Safety と IsSet

**ルール:** `null` を使わず、`IsSet` プロパティと `TryGetValue()` メソッドで未設定状態を表現。

```csharp
// 値を持つ状態
var name = RespondentName.From("山田太郎");
Assert.IsTrue(name.IsSet);
Assert.IsTrue(name.TryGetValue(out var value));
Assert.AreEqual("山田太郎", value);

// 未設定状態
var unset = RespondentName.Unset();
Assert.IsFalse(unset.IsSet);
Assert.IsFalse(unset.TryGetValue(out _));
```

---

## 4. 状態管理パターン（IsSet と Unset）

ValueObject は実装パターンによって状態管理方法が異なります。以下 3パターンの特徴と使い分けを理解することが重要です。

### 4.1 【PrimitiveValueObject 向け】IsSet フラグによる未設定状態管理

**概要:**
- `IsSet` フラグで "値あり" / "未設定" を区別
- ビジネス属性（名前、年齢など）で使用
- Unset() メソッドで未設定インスタンスを生成

**IsSet の動作:**

**IsSet = true** — 値を保持している状態
```csharp
var name = RespondentName.From("山田太郎");
Assert.IsTrue(name.IsSet);
Assert.AreEqual("山田太郎", name.Value);
```

**IsSet = false** — 未設定状態（値を保持していない）
```csharp
var unset = RespondentName.Unset();
Assert.IsFalse(unset.IsSet);
Assert.IsNull(unset.Value);  // Value は null
Assert.IsFalse(unset.TryGetValue(out _));  // 取得失敗
```

**等価性判定:**
- IsSet が等価性に含まれるため、Unset 同士は等価
- IsSet=true と IsSet=false は異なる

```csharp
var unset1 = RespondentName.Unset();
var unset2 = RespondentName.Unset();
var value = RespondentName.From("山田太郎");

Assert.AreEqual(unset1, unset2);     // true（両方 Unset）
Assert.AreNotEqual(unset1, value);   // true（状態が異なる）
```

---

### 4.2 【RowId 向け】value=0 による未採番状態管理

**概要:**
- `IsSet` フラグなし
- `value=0` で DB 採番前（未採番状態）を表現
- テーブル物理キーの管理
- 常に値を持つ（値は 0 以上）

**未採番状態:**
```csharp
var rowId = RowId.New();  // value=0 で初期化
Assert.AreEqual(0L, rowId.Value);
// DB に INSERT される前の状態
```

**採番後の状態:**
```csharp
var rowId = RowId.From(12345);  // DB から読み込まれた value
Assert.AreEqual(12345L, rowId.Value);
```

**特徴:**
- IsSet フラグなし（常に有効な状態）
- Unset() メソッドなし
- value=0 と value>0 で状態を区別
- 等価性判定は value のみ

---

### 4.3 【AggregateId 向け】GUID 必須（Unset 状態なし）

**概要:**
- `IsSet` フラグなし
- Unset 状態なし（常に Guid 値を持つ）
- Guid.Empty は許可されない（コンストラクタで検証）
- 集約の論理的識別子

**生成:**
```csharp
// 新規 ID を生成
var userId = UserId.New();  // Guid.NewGuid()

// 既存 ID から生成（DB 読み込み時）
var userId = UserId.From(new Guid("12345678-1234-1234-1234-123456789012"));

// Guid.Empty は許可されない
// UserId.From(Guid.Empty);  // ← ArgumentException
```

**特徴:**
- IsSet フラグなし（常に値を持つ）
- Unset() / Unset 状態なし（必須フィールド）
- Guid は内部構造化済みなので Normalize / Validate 不要
- 型安全性により異なる集約 ID を区別

---

### 4.4 状態管理パターン別比較表

| 特性 | PrimitiveValueObject | RowId | AggregateId |
|-----|------------------|-------|------------|
| **IsSet フラグ** | ✅ 有 | ❌ 無 | ❌ 無 |
| **Unset 状態** | ✅ IsSet=false | ❌ value=0で未採番 | ❌ 常に値を持つ |
| **Unset() メソッド** | ✅ 有（未設定インスタンス） | ❌ 無 | ❌ 無 |
| **New() メソッド** | ❌ 無 | ✅ 有（value=0） | ✅ 有（Guid.NewGuid） |
| **Value 型** | TValue（string, int など） | long（0以上） | Guid（非Empty） |
| **Value null許容** | ✅ IsSet で制御（null可能） | ❌ 常に long | ❌ 常に Guid |
| **Normalize 必須** | ✅ 通常必須 | ❌ 不要 | ❌ 不要 |
| **Validate 必須** | ✅ 通常必須 | ❌ 最小限 | ❌ Guid.Empty チェックのみ |
| **用途** | ビジネス属性（名前など） | DB行の物理キー | 集約の論理的ID |
| **使用例** | RespondentName | Entity._rowId | Entity.Id |

---

### 4.5 Unset の生成方法と シングルトン化の判断基準

#### PrimitiveValueObject — 毎回新規生成

```csharp
// 派生クラスで static メソッドを用意
public static RespondentName Unset() => new(false);  // 毎回新規インスタンス
```

**特性:**
- 呼び出すたびに新しいインスタンスが生成される
- メモリ負荷は多いが、各インスタンスが独立している
- 値オブジェクトの等価性によって Unset 同士は等価と見なされる

#### EnumValueObject — シングルトン化（推奨）

```csharp
// 選択肢が限定される場合、Unset インスタンスをシングルトン化
public sealed class CarModel : EnumValueObject<int>
{
    public static readonly CarModel Unknown = new(0);
    public static readonly CarModel Sedan = new(1);
    // ...
    
    private static readonly CarModel UnsetInstance = new();  // Unset専用
    
    public static CarModel Unset() => UnsetInstance;  // 常に同じインスタンス
}
```

**特性:**
- 常に同じインスタンスを返す
- メモリ効率が良い
- オブジェクト等価性と値等価性が一致する

#### シングルトン化の判断基準

| 判断基準 | PrimitiveValueObject | EnumValueObject |
|---------|------------------|-----------------|
| **選択肢の固定性** | 無限（スカラ値） | 有限（選択肢型） |
| **生成パターン** | 毎回新規生成 | ✅ シングルトン推奨 |
| **メモリ効率** | 低優先度 | ✅ 高優先度 |
| **使用頻度** | 低（ビジネスロジック内） | ✅ 高（UI入力処理） |
| **参照比較** | 不要 | ✅ 最適化可能 |

**ガイドライン:**
- **EnumValueObject の Unset：シングルトン化（UnsetInstance）**
  - 選択肢が限定されて不変
  - UI 入力処理で頻繁に生成される
  - メモリ最適化が有効
  
- **PrimitiveValueObject の Unset：毎回新規生成**
  - スカラ値として無限の可能性
  - ビジネスロジック内での参照頻度が低い
  - 値等価性で判定されるため参照は不要

### 4.6 Unset 状態での等価性判定（PrimitiveValueObject のみ）

**重要:** PrimitiveValueObject の IsSet が等価性に含まれるため、Unset の扱いは慎重に。

```csharp
// IsSet が異なると非等価
var unset = RespondentName.Unset();
var value = RespondentName.From("山田太郎");

Assert.AreNotEqual(unset, value);

// GetEqualityComponents() の先頭に IsSet が自動追加されるため、
// Unset 同士の比較も値の内容は無視される
var unset1 = RespondentName.Unset();
var unset2 = RespondentName.Unset();
Assert.AreEqual(unset1, unset2);
```

---

## 5. 実装パターン

### 5.1 基本的なスカラ値型の実装テンプレート

```csharp
using System;
using System.Collections.Generic;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.SharedKernel.ValueObjects;

/// <summary>
/// {業務名}を表すValueObject
/// </summary>
public sealed class {ClassName} : PrimitiveValueObject<{TValue}>, IEquatable<{ClassName}>
{
    /// <summary>
    /// {説明}
    /// </summary>
    private {ClassName}({TValue} value) : base(value, true)
    {
    }

    /// <summary>
    /// {説明}からインスタンスを生成する
    /// </summary>
    public static {ClassName} From({TValue} value) => new(value);

    /// <summary>
    /// 未設定インスタンスを返す
    /// </summary>
    public static {ClassName} Unset() => new(false);

    /// <summary>
    /// 値を取得する
    /// </summary>
    public {TValue} Value => ValueField;

    /// <summary>
    /// 正規化処理（オプション）
    /// </summary>
    protected override {TValue} Normalize({TValue} input)
    {
        // 値を正規化（トリム、変換など）
        return input;
    }

    /// <summary>
    /// 検証処理
    /// </summary>
    public override void Validate({TValue} normalized)
    {
        // ビジネスルールに基づく検証
        // 無効な場合は例外をスロー
    }

    // 等価性判定（基底実装を委譲）
    public bool Equals({ClassName}? other) => base.Equals(other);
    public override bool Equals(object? obj) => Equals(obj as {ClassName});

    // ハッシュコード（基底実装を使用）
    public override int GetHashCode() => base.GetHashCode();

    // 値コンポーネント
    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

### 5.1b テーブル物理キーの実装（RowId パターン）

**用途:**
- データベース行を一意識別（テーブルの物理キー）
- 複数テーブル集約のプライベート属性として管理
- Entity の内部実装詳細

**特徴:**
- `long` 値で保持（0以上）
- `value=0` で DB 採番前（未採番状態）を表現
- IsSet フラグなし（常に有効な状態）
- Normalize / Validate 不要
- ValueObject を直接継承（PrimitiveValueObject 非継承）

**実装例：RowId**
```csharp
public sealed class RowId : ValueObject, IEquatable<RowId>
{
    private readonly long _value;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">主キー値（0は未採番状態を表す）</param>
    /// <exception cref="ArgumentException">value が負数の場合</exception>
    private RowId(long value)
    {
        if (value < 0)
            throw new ArgumentException("RowId must be non-negative.", nameof(value));
        _value = value;
    }

    /// <summary>
    /// 未採番状態のRowIdを生成（value=0）
    /// DB採番後に実際のrowIdに更新される想定
    /// </summary>
    public static RowId New() => new(0);

    /// <summary>
    /// DB から読み込まれた値から RowId を生成
    /// </summary>
    public static RowId From(long value) => new(value);

    /// <summary>
    /// 保持する値を取得する
    /// </summary>
    public long Value => _value;

    /// <summary>
    /// 等価性判定のための値コンポーネント
    /// </summary>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return _value;
    }

    public override bool Equals(object? obj) => Equals(obj as RowId);

    public bool Equals(RowId? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return _value == other._value;
    }

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();
}
```

**Entity での使用パターン（RowId はプライベート）:**
```csharp
public class User : AggregateRoot<UserId>
{
    private RowId _rowId = null!;  // ← プライベート属性（DB採番前は null!初期化）

    /// <summary>
    /// 読み取り専用でアクセス可能
    /// </summary>
    public RowId RowId => _rowId;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public User(UserId id, string name, RowId? rowId = null)
    {
        Id = id;  // ← AggregateId（論理的ID）
        _rowId = rowId ?? RowId.New();  // ← RowId（物理キー）
    }
}
```

**特別な特徴:**
- ✅ PrimitiveValueObject ではなく ValueObject を直接継承
- ✅ IsSet フラグなし（常に value を保持）
- ✅ Unset() メソッドなし
- ✅ New() メソッドで value=0 の未採番状態を生成
- ✅ DB 採番前後で状態を管理（value=0 vs value>0）
- ✅ Entity のプライベート属性として管理（public プロパティで読み取りのみ公開）

**Mapper での使用例:**
```csharp
public UserDbModel ToDbModel(User entity)
{
    return new UserDbModel
    {
        RowId = entity.RowId.Value,  // Entity から取得
        UserId = entity.Id.Value,
        // ... 他のフィールド
    };
}

public User ToDomainEntity(UserDbModel dbModel, IClock clock)
{
    var userId = UserId.From(dbModel.UserId);
    var rowId = RowId.From(dbModel.RowId);  // DB から取得

    return new User(
        id: userId,
        name: dbModel.Name,
        rowId: rowId);  // Mapper が RowId を再構成
}
```

---

### 5.2 基本的な選択肢型の実装テンプレート

```csharp
using System.Collections.Generic;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.SharedKernel.ValueObjects;

/// <summary>
/// {業務名}（選択肢型）を表すValueObject
/// </summary>
public sealed class {ClassName} : EnumValueObject<{TValue}>
{
    // 定数インスタンス
    public static readonly {ClassName} Option1 = new({ValueConstant1});
    public static readonly {ClassName} Option2 = new({ValueConstant2});
    public static readonly {ClassName} Unset = new();

    private {ClassName}() : base(default, false) { }
    private {ClassName}({TValue} value) : base(value) { }

    /// <summary>
    /// {説明}からインスタンスを生成する
    /// </summary>
    public static {ClassName} From({TValue} value) => new(value);

    /// <summary>
    /// 値が妥当かを検証
    /// </summary>
    public override void Validate({TValue} value)
    {
        // 無効な値の場合は例外
        if (!IsValidValue(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Invalid value.");
    }

    /// <summary>
    /// 内部値を業務名称に変換
    /// </summary>
    protected override string GetDisplayName() => ValueField switch
    {
        {ValueConstant1} => "{日本語名1}",
        {ValueConstant2} => "{日本語名2}",
        _ => "不明"
    };

    private static bool IsValidValue({TValue} value)
    {
        // 有効な値かチェック
        return true;
    }

    // 値コンポーネント
    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

---

## 6. 命名規約

### 6.1 クラス名

**ルール:** ビジネス概念を直接反映した名前をつける。

| パターン | 例 |
|---------|-----|
| スカラ値（ID） | `RespondentId`, `QuestionnaireId`, `AnswerId` |
| スカラ値（属性） | `RespondentName`, `EmailAddress`, `PhoneNumber` |
| 選択肢型 | `RespondentStatus`, `QuestionType`, `Priority` |
| 日時型 | `CreatedAt`, `UpdatedAt`, `BirthdayDate` |
| 金額型 | `Price`, `Discount`, `TotalAmount` |
| 複合型 | `FullName`, `Address`, `ContactInfo` |

**命名の指針:**
- ✅ ValueObject そのもの = オブジェクトの型（`RespondentName` = 回答者名）
- ❌ 「`VO` サフィックス」は不要（`RespondentNameVO` ← 冗長）
- ❌ 「`Value` サフィックス」は不要（`RespondentNameValue` ← 冗長）
- ✅ 複数形を避ける（`RespondentIds` ← NG、個別の ValueObject で管理）

### 6.2 ファクトリメソッド

**ルール:** static メソッドで生成を制御。

| メソッド | 責務 | 動作（失敗時） |
|---------|------|---------|
| `From(TValue)` | 値から検証済みインスタンスを生成 | 例外をスロー |
| `TryFrom(TValue?, out Self)` | 検証を試みる（null 処理は ValueObject により異なる） | false を返す |
| `Unset()` | 未設定インスタンスを返す | 例外なし |
| `Parse(string)` | 文字列からパース（日時など） | 例外をスロー |
| `TryParse(string, out Self)` | 文字列パース試行 | false を返す |

```csharp
// From — 検証失敗時は例外
var name = RespondentName.From("  山田太郎  ");  // 正規化後："山田太郎"
// var invalid = RespondentName.From("");  // ArgumentException

// TryFrom — 入力必須型（CreatedAt など）
if (CreatedAt.TryFrom(dateTimeValue, out var createdAt))
{
    // 成功（dateTimeValue は null でない有効値）
}
else
{
    // 失敗（dateTimeValue が null または検証失敗）
}

// TryFrom — オプション型（IOptionalValueObject 実装）
if (RespondentName.TryFrom(input, out var name))
{
    // null 入力の場合：name は Unset（IsSet=false）
    // 有効な値の場合：name は設定済み（IsSet=true）
}
else
{
    // 検証失敗のみ
}

// Unset — 未設定インスタンス
var unset = RespondentName.Unset();
```

### 6.3 Value プロパティ

**ルール:** 保持する値へのアクセスは `Value` プロパティで提供。イミュータビリティのため get のみ。

#### 実装パターン

```csharp
public sealed class RespondentName : PrimitiveValueObject<string>
{
    protected readonly string ValueField;  // protected readonly

    public string? Value => IsSet ? ValueField : null;  // get のみ
}
```

#### Value プロパティと TryGetValue() の使い分け

| 用途 | メソッド | 利用場面 |
|------|---------|--------|
| **直接アクセス** | `Value` プロパティ | Domain ロジック内で IsSet が既知の場合 |
| **安全なアクセス** | `TryGetValue()` | 外部入力やレイヤ境界での値取得 |

**Value プロパティの特性:**
- IsSet=true なら値、false なら null を返す
- Domain ロジック内でシンプルに値にアクセス可能
- null チェックで未設定状態を判定

**TryGetValue() メソッドの特性:**
- bool で成功/失敗を明示的に表現
- out パラメータで値を返す
- 例外なしで安全に値取得

**使用例:**
```csharp
var name = RespondentName.From("太郎");

// Value プロパティ（Domain ロジック）
if (name.Value != null)
{
    Console.WriteLine(name.Value);
}

// TryGetValue（外部入力処理）
if (name.TryGetValue(out var value))
{
    ProcessName(value);
}
else
{
    HandleUnset();
}
```

---

## 7. よくある実装パターン

### 7.1 スカラ値型 — 文字列（入力必須）

```csharp
public sealed class RespondentName : PrimitiveValueObject<string>
{
    private RespondentName(string value) : base(value, true) { }

    public static RespondentName From(string value) => new(value);
    public static RespondentName Unset() => new(false);
    public string Value => ValueField;

    protected override string Normalize(string input)
    {
        // 前後の空白を除去、複数の連続空白を単一化
        return input.Trim().Replace("　", " ");
    }

    public override void Validate(string normalized)
    {
        if (string.IsNullOrEmpty(normalized))
            throw new ArgumentException("名前は空にできません。");
        if (normalized.Length > 100)
            throw new ArgumentException("名前は100文字以下です。");
    }

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

### 7.1b スカラ値型 — 文字列（入力オプション・IOptionalValueObject）

```csharp
public sealed class RespondentName : PrimitiveValueObject<string>, IOptionalValueObject<RespondentName, string>
{
    private RespondentName(bool isSet) : base(isSet) { }
    private RespondentName(string value, bool isSet) : base(value, isSet) { }

    public static RespondentName Unset() => new(false);

    public static RespondentName From(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new RespondentName(value, true);
    }

    public static bool TryFrom(string? input, out RespondentName result)
    {
        if (input is null)
        {
            result = Unset();
            return true;  // null は正常な未設定状態
        }

        try
        {
            result = From(input);
            return true;
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;
        }
    }

    public string Value => ValueField;

    protected override string Normalize(string input)
    {
        return input.Trim().Replace("　", " ");
    }

    public override void Validate(string normalized)
    {
        if (string.IsNullOrEmpty(normalized))
            throw new ArgumentException("名前は空にできません。");
        if (normalized.Length > 100)
            throw new ArgumentException("名前は100文字以下です。");
    }

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

### 7.2 スカラ値型 — 数値

```csharp
public sealed class RespondentAge : PrimitiveValueObject<int>
{
    private RespondentAge(int value) : base(value, true) { }

    public static RespondentAge From(int value) => new(value);
    public static RespondentAge Unset() => new(false);
    public int Value => ValueField;

    public override void Validate(int normalized)
    {
        if (normalized < 0 || normalized > 150)
            throw new ArgumentOutOfRangeException(nameof(normalized), "年齢は0～150です。");
    }

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

### 7.3 スカラ値型 — 日時（LocalDateTime）

```csharp
public sealed class CreatedAt : PrimitiveValueObject<DateTime>, IEquatable<CreatedAt>
{
    /// <summary>
    /// PrimitiveValueObject<DateTime> は内部型が DateTime（プリミティブ型）
    /// ファクトリメソッドで LocalDateTime から変換
    /// </summary>
    private CreatedAt(DateTime value) : base(value, true) { }

    /// <summary>
    /// LocalDateTime から CreatedAt を生成（推奨：IClock.JstNow から取得）
    /// </summary>
    public static CreatedAt From(LocalDateTime value) => new(value.Value);

    /// <summary>
    /// DateTime から CreatedAt を生成（層間型変換用）
    /// </summary>
    public static CreatedAt From(DateTime value) => new(value);

    /// <summary>
    /// TryFrom で null 安全に生成（入力必須パターン）
    /// </summary>
    public static bool TryFrom(LocalDateTime? input, out CreatedAt result)
    {
        if (input == null || !input.HasValue)
        {
            result = null!;
            return false;  // null は無効
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

    public DateTime Value => ValueField;

    public override void Validate(DateTime normalized)
    {
        base.Validate(normalized);
        if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
            throw new ArgumentException("有効な日時ではありません。");
    }

    protected override string Format(DateTime value)
    {
        // 表示形式をカスタマイズ
        return value.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public override bool Equals(object? obj) => Equals(obj as CreatedAt);
    public bool Equals(CreatedAt? other) => base.Equals(other);
    public override int GetHashCode() => base.GetHashCode();

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

**層間での Value カプセル化の詳細**

各層における ValueObject の役割と型処理の違いをまとめています。

| 特性 | Infrastructure層 | Application層 | Domain層 |
|-----|-----------------|-------------|---------|
| **内部型** | DateTime（プリミティブ） | LocalDateTime | LocalDateTime |
| **ValueObject の型** | DbModel（型なし、プリミティブのみ） | ValueObject（型安全） | ValueObject（型安全） |
| **型安全性** | 低（文字列、long など） | ✅ 高（型チェック） | ✅ 高（型チェック） |
| **責務** | 型変換のみ | 検証・正規化 | ビジネスロジック |
| **null 処理方針** | null チェック（プリミティブ） | IsSet フラグ | IsSet フラグ |
| **Normalize** | 不要（DbModel はプリミティブ） | ✅ 正規化ロジック | ✅ 正規化ロジック |
| **Validate** | 不要（ORM任せ） | ✅ 検証ロジック | ✅ 検証ロジック |
| **使用例** | `UserDbModel.CreatedAt: DateTime` | `CreateUserRequest.Name: string` → `RespondentName` | `User.Name: RespondentName` |

**層間の責務分離:**

1. **Infrastructure 層（DbModel）**
   - DateTime / DateTime? など OS ネイティブ型を直接保持
   - null で未設定を表現
   - type-safe でない（string, long など共存）

2. **Application 層（Use Case）**
   - 入力値を ValueObject に変換（TryFrom で null 安全に）
   - Application → Domain への受け渡しで型安全性を確保
   - 検証・正規化は ValueObject が実施

3. **Domain 層（Entity）**
   - ValueObject を型安全に扱う（型レベルの検証）
   - IsSet フラグで未設定状態を管理（null-free）
   - ビジネスロジックに集中

**重要：DateTime ↔ LocalDateTime 変換の流れ**

```
【DB ← → Entity の往路】
DbModel.CreatedAt (DateTime)
    ↓ [Mapper.ToDomainEntity]
Entity.CreatedAt (CreatedAt: LocalDateTime)
    ↓ [ビジネスロジック内で使用]
Domain Logic uses: LocalDateTime（型安全）

【Entity → DB への復路】
Entity.CreatedAt (CreatedAt: LocalDateTime)
    ↓ [Mapper.ToDbModel]
DbModel.CreatedAt (DateTime)
    ↓ [ORM / DataAccess]
DB stored as: datetime2(7)
```

---

### 7.4 選択肢型 — 列挙値

```csharp
public sealed class QuestionType : EnumValueObject<int>
{
    public static readonly QuestionType SingleChoice = new(1);
    public static readonly QuestionType MultipleChoice = new(2);
    public static readonly QuestionType FreeText = new(3);
    public static readonly QuestionType Unset = new();

    private QuestionType() : base(default, false) { }
    private QuestionType(int value) : base(value) { }

    public static QuestionType From(int value) => new(value);

    public override void Validate(int value)
    {
        if (value < 1 || value > 3)
            throw new ArgumentOutOfRangeException(nameof(value), "無効な質問タイプです。");
    }

    protected override string GetDisplayName() => ValueField switch
    {
        1 => "単一選択",
        2 => "複数選択",
        3 => "自由記述",
        _ => "不明"
    };

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

### 7.5 複合型（複数フィールド）

```csharp
public sealed class FullName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }

    private FullName(string firstName, string lastName, bool isSet)
    {
        IsSet = isSet;
        if (isSet)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        }
        else
        {
            FirstName = string.Empty;
            LastName = string.Empty;
        }
    }

    public static FullName From(string firstName, string lastName)
        => new(firstName, lastName, true);

    public static FullName Unset() => new(string.Empty, string.Empty, false);

    public string FullNameValue => $"{LastName} {FirstName}";

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet)
        {
            yield return FirstName;
            yield return LastName;
        }
    }

    public override string ToString()
    {
        return !IsSet ? "Unset" : FullNameValue;
    }
}
```

---

## 8. ファクトリメソッド設計

### 8.1 From メソッド — 例外ベース

**責務:** 検証に失敗した場合、例外をスローする。

```csharp
public static RespondentName From(string value) => new(value);

// コンストラクタ内で自動的に Normalize → Validate が実行される
private RespondentName(string value) : base(value, true) { }

// 使用方法
try
{
    var name = RespondentName.From(input);
    // 成功時は name が返される
}
catch (ArgumentException ex)
{
    // 検証失敗時は例外をキャッチ
}
```

**いつ使うか:**
- ドメイン層の確実な値が保証されている場合
- API 入力値のバリデーション後（すでに検証済みの場合）
- 業務ロジック内で不正な値は例外すべき場合

### 8.2 TryFrom メソッド — Result ベース

**責務:** 検証に失敗した場合、false を返す（例外なし）。

**重要:** TryFrom の動作は ValueObject の設計パターンによって異なります。

#### パターン A: 入力必須な ValueObject（例：CreatedAt, UpdatedAt）

**仕様:** null 入力は無効な入力と見なす。

```csharp
public static bool TryFrom(LocalDateTime? input, out CreatedAt result)
{
    // null の場合は失敗
    if (input == null || !input.HasValue)
    {
        result = null!;
        return false;  // null は無効
    }

    try
    {
        result = From(input.Value);  // LocalDateTime から変換
        return true;  // 成功
    }
    catch (ArgumentException)
    {
        result = null!;
        return false;  // 検証失敗
    }
}

// 使用方法
if (CreatedAt.TryFrom(_clock.JstNow, out var createdAt))
{
    // 検証成功（常に成功 — IClock.JstNow は常に有効な値）
}
else
{
    // 検証失敗（通常は発生しない）
}
```

**戻り値:**
- **true** — null でない有効な LocalDateTime（result に検証済みインスタンス）
- **false** — null 入力 **または** 検証失敗（result は null!）

**いつ使うか:**
- API リクエスト入力で null チェック + 検証を同時に行いたい場合
- DB読み込み時の型変換（null は異常）
- 入力必須な日時値の処理（null は無効な入力扱い）
- **推奨:** IClock.JstNow から直接取得した LocalDateTime 値

---

#### パターン B: オプション値オブジェクト（IOptionalValueObject 実装）【重要】

**仕様：** null 入力は **正常な未設定状態** と見なす（エラーではない）。

**実装クラス:**
- `RespondentName`（文字列）
- `RespondentAge`（数値）
- `RespondentPersonId`（数値）
- `CarModel`（選択肢型）
- その他 IOptionalValueObject 実装クラス

```csharp
public sealed class RespondentName : PrimitiveValueObject<string>, 
    IOptionalValueObject<RespondentName, string>
{
    private RespondentName(bool isSet) : base(isSet) { }
    private RespondentName(string value, bool isSet) : base(value, isSet) { }

    public static RespondentName Unset() => new(false);
    public static RespondentName From(string value) => new(value, true);

    /// <summary>
    /// 【重要】IOptionalValueObject の TryFrom 実装
    /// null は「未設定」として扱い、例外ではなく Unset() で返す
    /// </summary>
    public static bool TryFrom(string? input, out RespondentName result)
    {
        // 【ポイント】null は正常な入力 → Unset + true
        if (input is null)
        {
            result = Unset();
            return true;  // ✅ null は成功扱い
        }

        try
        {
            result = From(input);
            return true;  // ✅ 有効値は成功
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;  // ❌ 検証失敗のみ失敗
        }
    }
}

// 使用方法
if (RespondentName.TryFrom(inputName, out var name))
{
    if (name.IsSet)
    {
        Console.WriteLine($"値あり: {name.Value}");  // 有効な値
    }
    else
    {
        Console.WriteLine("未設定");  // null 入力 → Unset
    }
}
else
{
    Console.WriteLine("検証失敗");  // 値が無効（例：空文字列）
}
```

**【重要】戻り値の意味:**

| 戻り値 | result.IsSet | 意味 |
|------|------------|------|
| **true** | false | null 入力 → 未設定状態（正常） |
| **true** | true | 有効な値 → 設定済み状態（正常） |
| **false** | false | 検証失敗（無効な値） |

**パターン A vs パターン B の比較:**

```csharp
// パターン A: 入力必須（null は失敗）
if (CreatedAt.TryFrom(dateInput, out var created))
{
    // true: 有効な日時のみ
}
else
{
    // false: null または検証失敗
}

// パターン B: オプション（null は未設定）
if (RespondentName.TryFrom(nameInput, out var name))
{
    if (name.IsSet)
    {
        // 有効な値
    }
    else
    {
        // null 入力 → 未設定（正常）
    }
}
else
{
    // 検証失敗（値が無効）
}
```

**いつ使うか:**
- ✅ API リクエスト入力で null 許容の場合（フォーム送信など）
- ✅ JSON/クエリパラメータの解析（null は「未指定」）
- ✅ UI から未設定状態が発生する可能性がある場合
- ✅ 外部システム連携で null が有効な入力の場合
- ✅ Domain Entity のオプションプロパティ処理

**パターン A との使い分け:**
| 項目 | パターン A（入力必須） | パターン B（入力オプション） |
|-----|-----------------|-----------------|
| 例 | CreatedAt, UpdatedAt | RespondentName, RespondentAge |
| null 処理 | false を返す（無効） | true を返す（Unset） |
| 用途 | システム管理項目 | ユーザー入力項目 |
| インターフェース | なし | IOptionalValueObject |

### 8.3 デフォルト値を持つ ファクトリ

```csharp
public static RespondentName FromOrDefault(string? value, string defaultValue = "")
{
    if (string.IsNullOrEmpty(value))
        return From(defaultValue);
    return From(value);
}
```

---

## 9. 等価性判定の設計

### 9.1 GetEqualityComponents の実装

**ルール:** `GetValueComponents()` で値フィールドのみを列挙。IsSet は基底で自動追加。

```csharp
// ❌ 間違い（IsSet を重複含める）
protected override IEnumerable<object?> GetValueComponents()
{
    yield return IsSet;  // 重複！
    yield return ValueField;
}

// ✅ 正しい
protected override IEnumerable<object?> GetValueComponents()
{
    if (IsSet) yield return ValueField;
}
```

**理由:**
- `ValueObject.GetEqualityComponents()` がすでに IsSet を先頭に yield する
- `GetValueComponents()` は値のみを返す契約
- ValueObjectComponentNormalizer が IsSet を処理

### 9.2 複合型での等価性

```csharp
public sealed class FullName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet)
        {
            // IsSet が true の場合のみ、すべてのフィールドを列挙
            yield return FirstName;
            yield return LastName;
        }
    }
}

// 等価性判定：
var name1 = FullName.From("太郎", "山田");
var name2 = FullName.From("太郎", "山田");
var name3 = FullName.From("次郎", "山田");

Assert.AreEqual(name1, name2);     // true（名前が同じ）
Assert.AreNotEqual(name1, name3);  // true（名前が異なる）
```

### 9.3 ハッシュコード

**重要:** `GetHashCode()` は基底実装（ValueObject）が自動計算するため、派生クラスで override 不要。

```csharp
// ✅ 基底実装を使用（ほとんどの場合）
public sealed class RespondentName : PrimitiveValueObject<string>
{
    // GetHashCode() は override しない
    // 基底実装が GetEqualityComponents() の結果から自動計算
}

// ❌ override が必要な場合は稀（カスタム等価性ロジックがある場合のみ）
```

**ハッシュコードの計算:**
```csharp
public override int GetHashCode()
{
    var allComponents = GetEqualityComponents();
    var hash = 17;

    foreach (var component in allComponents)
    {
        unchecked
        {
            hash = hash * 31 + (component?.GetHashCode() ?? 0);
        }
    }

    return hash;
}
```

---

## 10. 検証・正規化の責務分離

### 10.1 Normalize — 値の変換

**責務:** ビジネスルール に基づき、入力値を **標準形に変換** する。副作用がない純粋関数。

| 例 | 入力 | 出力 |
|-----|-----|------|
| 文字列トリム | `"  山田太郎  "` | `"山田太郎"` |
| 空白統一 | `"山田　太郎"` | `"山田 太郎"` |
| 大文字統一 | `"taro"` | `"TARO"` |
| 単位変換 | `100 cm` | `1.0 m` |

**実装:**
```csharp
protected override string Normalize(string input)
{
    // 前後の空白を削除、全角スペースを半角に統一
    return input.Trim().Replace("　", " ");
}

// コンストラクタで自動実行
// private RespondentName(string value) : base(value, true)
// → base(Normalize(value), true) 相当が実行される
```

### 10.2 Validate — 値の検証

**責務:** 正規化済み値が **ビジネスルール** に違反しないか確認。違反なら例外。

| 例 | チェック内容 | 例外 |
|-----|----------|------|
| 文字列 | 長さ制限、空文字列禁止 | ArgumentException |
| 数値 | 範囲（最小値・最大値） | ArgumentOutOfRangeException |
| 日時 | 未来日禁止、特定の値（MinValue など）禁止 | ArgumentException |
| 選択肢 | 有効な選択肢か | ArgumentOutOfRangeException |

**実装:**
```csharp
public override void Validate(string normalized)
{
    // 空文字列チェック（正規化後）
    if (string.IsNullOrEmpty(normalized))
        throw new ArgumentException("名前は空にできません。", nameof(normalized));

    // 長さチェック
    if (normalized.Length > 100)
        throw new ArgumentException("名前は100文字以下である必要があります。", nameof(normalized));
}
```

### 10.3 Format — 表示形式

**責務:** 値を **表示用の文字列** にフォーマット（`ToString()` で使用）。

| 例 | 値 | フォーマット結果 |
|-----|-----|----------------|
| 日時 | `2026-07-07T14:30:00` | `"2026-07-07 14:30:00"` |
| 金額 | `1234.567` | `"¥1,234.57"` |
| 電話番号 | `9012345678` | `"090-1234-5678"` |
| パーセント | `0.856` | `"85.6%"` |

**実装:**
```csharp
protected override string Format(DateTime value)
{
    return value.ToString("yyyy-MM-dd HH:mm:ss");
}

public override string ToString()
{
    return !IsSet ? "Unset" : Format(ValueField);
}
```

### 10.4 実行順序

**フローチャート**

```
┌──────────────────────────────────────────┐
│    ValueObject のインスタンス化         │
│   From(value) / new() / TryFrom()       │
└────────────────┬─────────────────────────┘
                 │
                 ↓
        ┌───────────────────┐
        │  Normalize(value)  │
        │ （値を標準形に変換）│
        └────────┬──────────┘
                 │
                 ↓
        ┌─────────────────────┐
        │ Validate(normalized) │
        │ （ビジネスルール検証）│
        └────────┬────────────┘
                 │
         ┌───────┴──────────┐
         │                  │
    成功 ↓              失敗 ↓
         │                  └─→ ArgumentException スロー
         │
    ┌─────────────┐      失敗時の処理:
    │ ValueField  │      - From() は例外をスロー
    │ に値を格納  │      - TryFrom() は false を返す
    └────┬────────┘
         │
         ↓
    ┌─────────────┐
    │ IsSet を設定 │
    │ IsSet=true  │
    └────┬────────┘
         │
         ↓
    ┌─────────────────────┐
    │インスタンスを返却     │
    │（有効な ValueObject）│
    └─────────────────────┘
```

**各ステップの詳細**

| ステップ | 処理 | 例 | 例外時 |
|---------|------|-----|--------|
| **Normalize** | 入力値を業務ルールに基づき標準化 | `"  山田太郎  "` → `"山田太郎"` | スキップ（前処理なし） |
| **Validate** | 正規化済み値がビジネスルール違反していないか確認 | 名前の長さが 0 文字か 100 文字超か | ArgumentException をスロー |
| **格納** | 正規化・検証済み値を ValueField に格納 | ValueField = `"山田太郎"` | - |
| **IsSet 設定** | IsSet フラグを true に設定（設定済み状態を表現） | IsSet = true | - |

**From() vs TryFrom() の使い分け**

```csharp
// From() — 検証失敗時は例外
try
{
    var name = RespondentName.From(input);  // 検証失敗 → ArgumentException
}
catch (ArgumentException ex)
{
    // エラー処理
}

// TryFrom() — 検証失敗時は false を返す
if (RespondentName.TryFrom(input, out var name))
{
    // 成功時のみ処理
    ProcessName(name);
}
else
{
    // null 入力は Unset() に変換される（失敗ではない）
    // または検証失敗時は false が返される
}
```

---

## 11. ValueObject パターン別ガイド

3種類の ValueObject パターンの特徴と選択基準をまとめています。どのパターンを使うべきかを判断するためのチェックリストとフローチャートを提供します。

### 11.1 PrimitiveValueObject を使うべき場合

**判定基準:**
- ✅ スカラ値（単一フィールド）
- ✅ ビジネス属性（名前、価格、年齢など）
- ✅ 入力値のバリデーション・正規化が必要
- ✅ Unset 状態が自然である（オプション項目）

**具体例:**
- RespondentName（文字列：回答者名）
- RespondentAge（数値：年齢）
- CreatedAt / UpdatedAt / DeletedAt（日時：監査）
- Price（金額：商品価格）
- EmailAddress（文字列：メールアドレス）

**実装パターン:**
```csharp
public sealed class RespondentName : PrimitiveValueObject<string>, IEquatable<RespondentName>
{
    private RespondentName(string value) : base(value, true) { }

    public static RespondentName From(string value) => new(value);
    public static RespondentName Unset() => new(false);

    public string Value => ValueField;

    protected override string Normalize(string input) => input.Trim();

    public override void Validate(string normalized)
    {
        if (string.IsNullOrEmpty(normalized))
            throw new ArgumentException("名前は空にできません。");
    }

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

**チェックリスト（使用前に確認）:**
- [ ] 単一フィールド（スカラ値）か
- [ ] ビジネス属性の値を表すか
- [ ] Normalize() で正規化ロジックがあるか
- [ ] Validate() で検証ロジックがあるか
- [ ] IsSet フラグで Unset 状態を表現するか
- [ ] TryGetValue() で安全なアクセスを提供しているか
- [ ] null 許容性が明確か

---

### 11.2 AggregateId を使うべき場合

**判定基準:**
- ✅ Entity の識別子（GUID ベース）
- ✅ 型安全性が必須（UserId vs RoleId を型チェックで区別）
- ✅ 常に値を持つ（null 不許容）
- ✅ Unset 状態が不要
- ✅ 複数テーブル集約の論理的統一 ID

**具体例:**
- UserId（ユーザー集約の識別子）
- RoleId（ロール集約の識別子）
- EmployeeId（従業員集約の識別子）
- OrderId（注文集約の識別子）

**実装パターン:**
```csharp
public sealed class UserId : AggregateId
{
    private UserId(Guid value) : base(value) { }

    /// <summary>新しい ID を生成</summary>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>既存の GUID から ID を生成（DB 読み込み時）</summary>
    public static UserId From(Guid value) => new(value);
}

// Entity での使用
public class User : AggregateRoot<UserId>  // ← TId = UserId
{
    public User(UserId id, string name)
    {
        Id = id;  // AggregateId で識別
    }
}
```

**チェックリスト（使用前に確認）:**
- [ ] 集約を一意識別する目的か
- [ ] GUID 値で十分か
- [ ] 型安全性が重要か（他の ID と混同されると問題になるか）
- [ ] 常に値を持つか（null 許容ではないか）
- [ ] Entity のジェネリック型パラメータとして使用するか
- [ ] Unset 状態は不要か

---

### 11.3 RowId を使うべき場合

**判定基準:**
- ✅ データベーステーブルの行を一意識別（物理キー）
- ✅ Entity のプライベート属性として管理
- ✅ テーブル採番前は value=0 で表現
- ✅ IsSet フラグ不要（value で状態を表現）
- ✅ Mapper で DbModel ↔ Entity の変換に使用

**具体例:**
- User エンティティの _rowId
- Employee エンティティの _rowId
- Role エンティティの _rowId

**実装パターン:**
```csharp
// RowId はプライベート属性
public class User : AggregateRoot<UserId>
{
    private RowId _rowId = null!;
    public RowId RowId => _rowId;  // 読み取り専用で公開

    public User(UserId id, string name, RowId? rowId = null)
    {
        Id = id;
        _rowId = rowId ?? RowId.New();  // value=0 で未採番状態
    }
}

// Mapper で使用
public UserDbModel ToDbModel(User entity)
{
    return new UserDbModel
    {
        RowId = entity.RowId.Value,  // Entity から取得
        UserId = entity.Id.Value,
    };
}
```

**チェックリスト（使用前に確認）:**
- [ ] DB テーブルの行を一意識別するか（物理キー）
- [ ] Entity のプライベート属性か
- [ ] value=0 で未採番状態を表現するか
- [ ] public プロパティで読み取りのみ公開するか
- [ ] Mapper で DbModel のマッピングに使用するか

#### 11.3.2 RowId のライフサイクル

RowId の状態遷移は3段階です。

**段階1：新規作成時（値が未採番状態）**
```csharp
// Entity 新規作成時
var user = new User(
    id: UserId.NewId(),
    name: "Taro Yamada",
    rowId: null  // または省略 → RowId.New() で value=0 に初期化
);

// この時点で user._rowId は value=0（未採番）
```

**段階2：DB 採番後（value > 0 に更新）**
```csharp
// Repository の Create メソッド内で採番
public async Task CreateAsync(User entity)
{
    // INSERT 実行（DB側で IDENTITY/Sequence で自動採番）
    var dbModel = _mapper.ToDbModel(entity);
    var rowId = await _dataAccess.InsertAsync(dbModel);  // 採番された rowId を取得
    
    // Entity 内部の _rowId を更新（value=0 → value=採番値）
    entity.UpdateRowId(rowId);  // または内部メソッドで更新
}
```

**段階3：Entity 再構築時（既存値から復元）**
```csharp
// Repository の Get メソッド内
public async Task<User?> GetAsync(UserId userId)
{
    var dbModel = await _dataAccess.GetAsync(userId);
    if (dbModel == null) return null;
    
    // Mapper で DbModel → Entity に変換時、既存の rowId を復元
    return _mapper.ToDomainEntity(dbModel);
    // この時点で entity._rowId は value=既存採番値（value > 0）
}
```

#### 11.3.3 Mapper での RowId 取得と変換

**ToDbModel（Entity → DbModel）**
```csharp
public UserDbModel ToDbModel(User entity)
{
    return new UserDbModel
    {
        RowId = entity.RowId.Value,  // ← Entity の _rowId から取得（値を直接抽出）
        UserId = entity.Id.Value,
        Name = entity.Name.Value,
        CreatedAt = entity.CreatedAt.Value.Value,  // LocalDateTime → DateTime
        UpdatedAt = entity.UpdatedAt.HasUpdated 
            ? entity.UpdatedAt.Value.Value 
            : (DateTime?)null,
    };
}
```

**ToDomainEntity（DbModel → Entity）**
```csharp
public User ToDomainEntity(UserDbModel dbModel, IClock clock)
{
    // DbModel の RowId から RowId ValueObject を復元
    var rowId = RowId.From(dbModel.RowId);  // value > 0 の既存値
    
    return new User(
        id: UserId.From(dbModel.UserId),
        name: RespondentName.From(dbModel.Name),
        rowId: rowId  // ← 既存の rowId を復元
    );
}
```

**重要:** 
- ToDbModel では Entity.RowId.Value で long 値を抽出
- ToDomainEntity では DbModel.RowId から RowId.From() で復元
- new 時に rowId パラメータを省略すると RowId.New() で value=0 に初期化される

#### 11.3.4 Repository での RowId 管理

**新規作成時**
```csharp
public async Task CreateAsync(User entity)
{
    // Entity は value=0 の RowId を持つ
    var dbModel = _mapper.ToDbModel(entity);
    // DbModel.RowId = 0
    
    // INSERT 実行（DB が IDENTITY で自動採番）
    // SQL: INSERT INTO t_users (name, ...) VALUES (...)
    //      → DB が RowId を採番（e.g., 1001）
    
    var adoptedRowId = await _dataAccess.InsertAsync(dbModel);
    
    // Entity 内部の _rowId を採番値で更新（内部メソッド）
    entity._rowId = RowId.From(adoptedRowId);
    // この後、entity.RowId.Value は 1001
}
```

**既存レコード読み込み時**
```csharp
public async Task<User?> GetAsync(UserId userId)
{
    // SELECT: RowId は既に採番済み（e.g., 1001）
    var dbModel = await _dataAccess.GetAsync(userId);
    
    if (dbModel == null) return null;
    
    // Mapper は DbModel.RowId から RowId.From() で復元
    return _mapper.ToDomainEntity(dbModel);
    // 戻り値の Entity は value=1001 の RowId を持つ
}
```

**delete（論理削除）時**
```csharp
public async Task DeleteAsync(User entity)
{
    // Entity は value > 0 の RowId を持つ
    var dbModel = _mapper.ToDbModel(entity);
    
    // UPDATE: RowId は変わらない（value > 0 のまま）
    // SQL: UPDATE t_users SET deleted_at = ..., deleted_by = ... WHERE row_id = ?
    await _dataAccess.UpdateAsync(dbModel);
}
```

**ガイドライン:**
- 新規作成後の Entity は Repository によって RowId が更新される責務がある
- Mapper は RowId の変換（RowId ↔ long）のみに専念
- Repository は DB 採番値の取得と Entity への反映の両責務を持つ

---

### 11.4 パターン選択フローチャート

```
【ValueObject を設計する】
          │
          ├─ 集約（Aggregate）を識別する？
          │   YES → AggregateId 継承
          │   
          ├─ DB テーブル行の物理キー？
          │   YES → RowId パターン
          │   
          ├─ スカラ値（単一フィールド）かつビジネス属性？
          │   YES → PrimitiveValueObject 継承
          │       ├─ 選択肢型（固定値集合）？
          │       │   YES → EnumValueObject 継承
          │       │   NO → PrimitiveValueObject 継承
          │   
          └─ 複合型（複数フィールド）？
              YES → ValueObject を直接継承
```

**フローの解説:**

1. **最初の判定：集約ID か物理キー か？**
   - 集約の識別子 → AggregateId
   - DB テーブル行 → RowId
   - 上記以外 → 次の判定へ

2. **第二の判定：スカラ値 か複合型 か？**
   - スカラ値 → PrimitiveValueObject / EnumValueObject
   - 複合型（FirstName + LastName など） → ValueObject 直接継承

3. **第三の判定（スカラ値の場合）：選択肢型 か？**
   - 固定選択肢（Status=1|2|3） → EnumValueObject
   - スカラ値（名前、年齢など） → PrimitiveValueObject

---

### 11.5 パターン別比較表（拡張版）

| 特性 | PrimitiveValueObject | AggregateId | RowId |
|-----|------------------|------------|-------|
| **基底クラス** | PrimitiveValueObject<TValue> | AggregateId（抽象） | ValueObject |
| **値型** | TValue（string, int, DateTime など） | Guid | long |
| **IsSet フラグ** | ✅ 有 | ❌ 無 | ❌ 無 |
| **Unset 状態** | ✅ IsSet=false | ❌ 常に Guid | ❌ value=0で未採番 |
| **Unset メソッド** | ✅ Unset() 有 | ❌ 無 | ❌ 無 |
| **New() メソッド** | ❌ 無 | ✅ New()→Guid.NewGuid | ✅ New()→value=0 |
| **from/From メソッド** | ✅ From(value) | ✅ From(Guid) | ✅ From(long) |
| **Normalize** | ✅ 通常実装 | ❌ 不要 | ❌ 不要 |
| **Validate** | ✅ 通常実装 | ✅ Guid.Empty チェック | ✅ value≥0 チェック |
| **Value null許容** | ✅ IsSet で制御 | ❌ 常に Guid | ❌ 常に long |
| **用途** | ビジネス属性 | 集約識別子 | DB物理キー |
| **使用例** | RespondentName | UserId | Entity._rowId |
| **Entity での役割** | Domain ロジック内で使用 | `AggregateRoot<TId>` | プライベート属性 |

---

### 11.6 パターン別実装チェックリスト

#### PrimitiveValueObject チェックリスト

**設計段階:**
- [ ] ビジネス概念を正確に名前に反映しているか（例：`RespondentName`）
- [ ] ValueObject で表現すべきか（Entity でなく）
- [ ] Unset 状態が必要か
- [ ] スカラ値型か、それとも複合型か

**実装段階:**
- [ ] コンストラクタは `private`
- [ ] フィールドは `readonly`
- [ ] `From()` static メソッドを実装
- [ ] `Unset()` static メソッドを実装
- [ ] `Value` プロパティで値へのアクセスを提供
- [ ] `TryGetValue()` メソッドで安全な値取得を提供
- [ ] `Normalize()` をオーバーライド（ビジネスロジック）
- [ ] `Validate()` をオーバーライド（検証ロジック）
- [ ] `GetValueComponents()` をオーバーライド（等価性判定）
- [ ] IEquatable<T> を実装

#### AggregateId チェックリスト

**設計段階:**
- [ ] 集約を一意識別する目的か
- [ ] 型安全性が必須か（他の ID と区別が必要か）
- [ ] 常に値を持つか（必須フィールド）
- [ ] Unset 状態は不要か

**実装段階:**
- [ ] AggregateId を継承
- [ ] コンストラクタで Guid.Empty をチェック
- [ ] `New()` メソッドで新規 ID を生成
- [ ] `From(Guid)` メソッドで既存 ID を復元
- [ ] GetValueComponents() で Guid を yield

#### RowId チェックリスト

**設計段階:**
- [ ] DB テーブル行の物理キーか
- [ ] Entity のプライベート属性として管理するか
- [ ] value=0 で未採番状態を表現するか

**実装段階:**
- [ ] ValueObject を直接継承
- [ ] コンストラクタで value≥0 をチェック
- [ ] `New()` メソッドで value=0 を返す
- [ ] `From(long)` メソッドで既存 rowId を復元
- [ ] public プロパティで読み取りのみ公開

---

## 12. レイヤ制約

### 12.1 配置されるべきレイヤ

**ルール:** すべての ValueObject は **SharedKernel/ValueObjects** フォルダに配置。

```
src/
  ├─ SharedKernel/
  │   └─ ValueObjects/
  │       ├─ Abstractions/
  │       │   ├─ ValueObject.cs（基底）
  │       │   ├─ PrimitiveValueObject.cs
  │       │   ├─ EnumValueObject.cs
  │       │   └─ ValueObjectComponentNormalizer.cs
  │       │
  │       ├─ Audit/
  │       │   ├─ CreatedAt.cs
  │       │   ├─ UpdatedAt.cs
  │       │   └─ ...
  │       │
  │       └─ {ドメイン領域}/
  │           ├─ RespondentId.cs
  │           ├─ RespondentName.cs
  │           ├─ QuestionType.cs
  │           └─ ...
```

### 12.2 依存関係ルール

**ルール:** ValueObject は以下への依存が許可される：

| 依存先 | 許可 | 理由 |
|--------|------|------|
| 他の ValueObject | ✅ | 値の合成が必要な場合 |
| .NET BCL（System など） | ✅ | 基本型のサポート必須 |
| Application 層 | ❌ | Domain が Application に依存しない |
| Infrastructure 層 | ❌ | Domain が Infrastructure に依存しない |
| ロガー（ILogger） | ❌ | Domain は副作用を持たない |
| DateTime.Now / UtcNow | ❌ | 時刻が必要な場合は `IClock` 経由 |

### 12.3 外部依存の回避

**パターン 1: 時刻が必要な場合**

```csharp
// ❌ 間違い（DateTime.Now を直接使用）
public static CreatedAt Now() => new(DateTime.Now);

// ✅ 正しい（IClock を経由）
public sealed class CreatedAt : PrimitiveValueObject<DateTime>, IValidateWithClock
{
    private CreatedAt(DateTime value) : base(value, true) { }

    public static CreatedAt From(DateTime value) => new(value);

    public void ValidateWithClock(IClock clock)
    {
        // 検証ロジック（必要に応じて clock を使用）
    }
}
```

**パターン 2: 複雑な検証が必要な場合**

検証を Application 層（UseCase）で実施し、ValueObject にはシンプルな検証のみを残す。

```csharp
// ValueObject — シンプルな形式チェック
public override void Validate(string normalized)
{
    if (normalized.Length > 100)
        throw new ArgumentException("Too long.");
}

// UseCase — ドメイン検証
public sealed class CreateRespondentUseCase
{
    public void Execute(CreateRespondentRequest request)
    {
        var name = RespondentName.From(request.Name);  // 形式チェック

        // 複雑な検証（DB 検索など）
        if (await _repository.ExistsByName(name))
            throw new BusinessException("その名前は既に登録されています。");
    }
}
```

---

## 13. チェックリスト — 新しい ValueObject を実装するとき

実装前に以下を確認：

### 設計段階

- [ ] ビジネス概念を正確に名前に反映している（例：`RespondentName` は「回答者の名前」）
- [ ] ValueObject で表現すべきか（Entity でなく）を確認
- [ ] Unset 状態が必要かを確認
- [ ] 単一フィールド（PrimitiveValueObject）か複合フィールド（ValueObject）かを判定
- [ ] 選択肢型（EnumValueObject）かスカラ値型（PrimitiveValueObject）かを判定

### 実装段階

- [ ] コンストラクタは `private` または `protected`
- [ ] フィールドは `readonly`
- [ ] `From()` static メソッドを実装
- [ ] `Unset()` static メソッドを実装
- [ ] `Value` プロパティで値へのアクセスを提供
- [ ] `TryGetValue()` メソッドで安全な値取得を提供
- [ ] `Normalize()` をオーバーライド（必要に応じて）
- [ ] `Validate()` をオーバーライド（ビジネスルール）
- [ ] `GetValueComponents()` をオーバーライド（等価性判定）
- [ ] `Format()` をオーバーライド（表示形式が必要な場合）

### テスト段階

- [ ] 正常系：正規化・検証が期待通り動作
- [ ] 異常系：不正な値で例外が発生
- [ ] 等価性：同じ値は等価、異なる値は非等価
- [ ] ハッシュコード：等価なら同じハッシュ値
- [ ] Unset：未設定状態が正しく表現される
- [ ] TryGetValue：IsSet に応じた動作

### ドキュメント段階

- [ ] クラスの責務をコメントで明記
- [ ] メソッドのコメントを記述
- [ ] 検証ルールを詳細に説明（必要に応じて）
- [ ] 使用例をコメントで示す（複雑な場合）

---

## 参考資料

- [ValueObject 技術仕様書](../../SharedKernel/ValueObjects/Abstractions/ValueObject_技術仕様書.md)
- [PrimitiveValueObject 技術仕様書](../../SharedKernel/ValueObjects/Abstractions/PrimitiveValueObject_技術仕様書.md)
- [EnumValueObject 技術仕様書](../../SharedKernel/ValueObjects/Abstractions/EnumValueObject_技術仕様書.md)
- [ValueObject コンポーネント正規化器 技術仕様書](../../SharedKernel/ValueObjects/Abstractions/ValueObjectComponentNormalizer_技術仕様書.md)

---

**版履歴**

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 2.0 | 2026-08-08 | Claude Code | 大規模リファクタ — セクション 11.3 RowId ライフサイクル詳細化、セクション 7.3 層間比較表追加、セクション 10.4 フローチャート図追加 |
| 1.0 | 2026-07-07 | Claude Code | 初版作成 — 現在の実装に基づくガイド |
