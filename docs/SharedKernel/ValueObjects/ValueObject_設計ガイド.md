# ValueObject 設計ガイド — プロジェクト全体指針

**プロジェクト:** SupportAdvance  
**レイヤ:** SharedKernel / Domain 層  
**種別:** 設計ガイド  
**版:** 1.0 / 2026-07-07

---

## 📋 目次

1. [概要](#1-概要)
2. [ValueObject の階層構造](#2-valueobject-の階層構造)
3. [設計の基本原則](#3-設計の基本原則)
4. [Unset（未設定）状態の扱い](#4-unset未設定状態の扱い)
5. [実装パターン](#5-実装パターン)
6. [命名規約](#6-命名規約)
7. [よくある実装パターン](#7-よくある実装パターン)
8. [ファクトリメソッド設計](#8-ファクトリメソッド設計)
9. [等価性判定の設計](#9-等価性判定の設計)
10. [検証・正規化の責務分離](#10-検証正規化の責務分離)
11. [レイヤ制約](#11-レイヤ制約)

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
  ├─ PrimitiveValueObject<TValue>（スカラ値）
  │   ├─ CreatedAt, UpdatedAt
  │   ├─ RespondentId, RespondentName（例）
  │   └─ {その他スカラ値型}
  │
  └─ EnumValueObject<TValue>（選択肢型）
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

### 2.3 EnumValueObject&lt;TValue&gt;（選択肢型の基底）

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

## 4. Unset（未設定）状態の扱い

### 4.1 IsSet フラグの役割

**IsSet = false** — ValueObject が値を保持していない（Unset 状態）

- `ValueField` は default 値（使用不可）
- `ToString()` は "Unset" を返す
- `TryGetValue()` は false を返す
- 等価性判定：IsSet も含まれるため、Unset 同士は等価だが、値ありと Unset は異なる

```csharp
var unset1 = RespondentName.Unset();
var unset2 = RespondentName.Unset();
var value = RespondentName.From("山田太郎");

Assert.AreEqual(unset1, unset2);     // true（両方 Unset）
Assert.AreNotEqual(unset1, value);   // true（状態が異なる）
```

### 4.2 Unset の生成方法

**PrimitiveValueObject:**
```csharp
// 派生クラスで static メソッドを用意
public static RespondentName Unset() => new(false);

// または IOptionalValueObject インターフェース
public static RespondentName Unset() => new(false);
```

**EnumValueObject:**
```csharp
// 引数なしコンストラクタで IsSet=false に初期化
public sealed class Status : EnumValueObject<int>
{
    public static readonly Status Unset = new();  // IsSet=false

    private Status() : base(default, false) { }
    private Status(int value) : base(value) { }
}
```

### 4.3 Unset 状態での等価性判定

**重要:** IsSet が等価性に含まれるため、Unset の扱いは慎重に。

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

### 6.3 プロパティ名

**ルール:** 値フィールドへのアクセスは `Value` プロパティで提供。

```csharp
public sealed class RespondentName : PrimitiveValueObject<string>
{
    protected readonly string ValueField;  // protected readonly

    public string Value => ValueField;  // public property
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

### 7.3 スカラ値型 — 日時

```csharp
public sealed class CreatedAt : PrimitiveValueObject<DateTime>
{
    private CreatedAt(DateTime value) : base(value, true) { }

    public static CreatedAt From(DateTime value) => new(value);
    public static CreatedAt Unset() => new(false);
    public DateTime Value => ValueField;

    public override void Validate(DateTime normalized)
    {
        if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
            throw new ArgumentException("有効な日時ではありません。");
    }

    protected override string Format(DateTime value)
    {
        // 表示形式をカスタマイズ
        return value.ToString("yyyy-MM-dd HH:mm:ss");
    }

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

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
public static bool TryFrom(DateTime? input, out CreatedAt result)
{
    // null の場合は失敗
    if (!input.HasValue)
    {
        result = null!;
        return false;  // null は無効
    }

    try
    {
        result = From(input.Value);
        return true;  // 成功
    }
    catch (ArgumentException)
    {
        result = null!;
        return false;  // 検証失敗
    }
}

// 使用方法
if (CreatedAt.TryFrom(inputDateTime, out var createdAt))
{
    // 検証成功
}
else
{
    // 検証失敗 または null 入力
}
```

**戻り値:**
- **true** — null でない有効な値（result に検証済みインスタンス）
- **false** — null 入力 **または** 検証失敗（result は null!）

**いつ使うか:**
- API リクエスト入力で null チェック + 検証を同時に行いたい場合
- DB読み込み時の型変換（null は異常）
- 入力必須な値の処理（null は無効な入力扱い）

---

#### パターン B: オプション値オブジェクト（IOptionalValueObject 実装）

**仕様:** null 入力は正常な未設定状態と見なす。

**実装クラス:**
- `RespondentName`（文字列）
- `RespondentAge`（数値）
- `RespondentPersonId`（数値）
- `CarModel`（選択肢型）
- その他 IOptionalValueObject 実装クラス

```csharp
public sealed class RespondentName : PrimitiveValueObject<string>, IOptionalValueObject<RespondentName, string>
{
    private RespondentName(bool isSet) : base(isSet) { }
    private RespondentName(string value, bool isSet) : base(value, isSet) { }

    public static RespondentName Unset() => new(false);
    public static RespondentName From(string value) => new(value, true);

    public static bool TryFrom(string? input, out RespondentName result)
    {
        // null の場合は Unset として成功
        if (input is null)
        {
            result = Unset();
            return true;  // null → Unset は正常
        }

        try
        {
            result = From(input);
            return true;  // 成功
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;  // 検証失敗
        }
    }
}

// 使用方法
if (RespondentName.TryFrom(inputName, out var name))
{
    // null 入力の場合：name は Unset（IsSet=false）
    // 有効な値の場合：name は設定済み（IsSet=true）
}
else
{
    // 検証失敗のみ（null は失敗ではない）
}
```

**戻り値:**
- **true** — null 入力（Unset 返却）**または** 検証成功（result に検証済みインスタンス）
- **false** — 検証失敗のみ（result は Unset）

**いつ使うか:**
- API リクエスト入力で null 許容の場合（フォーム送信など）
- JSON/クエリパラメータの解析（null は「未指定」として扱う）
- UI から未設定状態が発生する可能性がある場合
- 外部システム連携で null が有効な入力の場合

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

```
コンストラクタ呼び出し
    ↓
Normalize(value) 実行 → 正規化済み値を取得
    ↓
Validate(正規化済み値) 実行 → 検証（例外あり得る）
    ↓
ValueField に格納
    ↓
IsSet = true に設定
    ↓
インスタンス返却
```

---

## 11. レイヤ制約

### 11.1 配置されるべきレイヤ

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

### 11.2 依存関係ルール

**ルール:** ValueObject は以下への依存が許可される：

| 依存先 | 許可 | 理由 |
|--------|------|------|
| 他の ValueObject | ✅ | 値の合成が必要な場合 |
| .NET BCL（System など） | ✅ | 基本型のサポート必須 |
| Application 層 | ❌ | Domain が Application に依存しない |
| Infrastructure 層 | ❌ | Domain が Infrastructure に依存しない |
| ロガー（ILogger） | ❌ | Domain は副作用を持たない |
| DateTime.Now / UtcNow | ❌ | 時刻が必要な場合は `IClock` 経由 |

### 11.3 外部依存の回避

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

## 12. チェックリスト — 新しい ValueObject を実装するとき

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

- [ValueObject 技術仕様書](../Abstractions/ValueObject_技術仕様書.md)
- [PrimitiveValueObject 技術仕様書](../Abstractions/PrimitiveValueObject_技術仕様書.md)
- [EnumValueObject 技術仕様書](../Abstractions/EnumValueObject_技術仕様書.md)
- [ValueObject コンポーネント正規化器 技術仕様書](../Abstractions/ValueObjectComponentNormalizer_技術仕様書.md)

---

**版履歴**

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-07 | Claude Code | 初版作成 — 現在の実装に基づくガイド |
