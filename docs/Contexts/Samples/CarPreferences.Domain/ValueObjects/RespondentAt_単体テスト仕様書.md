# 単体テスト仕様書 — RespondentAt

**プロジェクト:** SupportAdvance  
**テスト対象:** ValueObject（RespondentAt）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-07-08

---

## 0. 本書の位置づけ

本書は、Domain 層の ValueObject クラス `RespondentAt` が、技術仕様書および詳細設計書で定義された以下の特性を満たすことを確認するテスト仕様書である：

- **値の不変性**: オブジェクト生成後、内部状態が変更されない
- **等価性比較**: 同じ値を持つ 2 つのオブジェクトは等価
- **ハッシュ整合性**: Equals=true のオブジェクトは同一ハッシュ値
- **Clock 依存検証**: 現在時刻を基準とした未来日チェック
- **IsSet 状態管理**: IsSet フラグが正しく機能する
- **Optional 値操作**: Unset・From・TryFrom・TryGetValue の動作

---

## 1. テスト目的

RespondentAt の各メンバーが以下を満たすことを確認：

1. **値オブジェクトセマンティクス**: 不変性、等価性、ハッシュコード整合性
2. **IOptionalValidateWithClock 契約**: Unset、From、TryFrom、TryGetValue の動作
3. **IEquatable 実装**: 型安全な等価性判定
4. **Clock 依存検証**: 基本検証（MinValue/MaxValue）とビジネス検証（未来日チェック）
5. **null 安全性**: Try パターンで例外を最小化

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | RespondentAt |
| **名前空間** | SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects |
| **依拠仕様** | RespondentAt 技術仕様書 v1.0 + 詳細設計書 v1.0 |
| **前提** | ValueObject・PrimitiveValueObject のテストが完了していること |

---

## 2.X テスト用実装と DI 設定

| 実装名 | 役割 | 対応する観点 | 説明 |
|--------|------|------------|------|
| Test Constructor | テスト用コンストラクタ | VO-IS-01～02 | RespondentAt の初期化検証 |
| Equals/GetHashCode | 標準メソッド | VO-EQ, VO-NE, VO-HC, VO-OP, VO-TS | .NET の Equals・GetHashCode・ToString 検証 |
| MockClock | モック実装 | VO-CLK-01～05 | 時刻固定で検証実施 |

---

## 3. テスト観点一覧

### グループ IS：`IsSet` プロパティ

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-IS-01 | IsSet=true で構築したオブジェクトは IsSet が true を返す | 正常系 | § 2.1 |
| VO-IS-02 | IsSet=false で構築したオブジェクトは IsSet が false を返す | 正常系 | § 2.1 |

### グループ CLK：Clock 依存検証

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-CLK-01 | 過去日は検証成功、IsSet=true で返す | 正常系 | 技術仕様書 § 2.3 |
| VO-CLK-02 | 現在日時は検証成功、IsSet=true で返す | 正常系 | 技術仕様書 § 2.3 |
| VO-CLK-03 | 未来日は ArgumentException をスロー | 異常系 | 技術仕様書 § 2.3 |
| VO-CLK-04 | DateTime.MinValue は ArgumentException をスロー | 異常系 | 技術仕様書 § 2.3 |
| VO-CLK-05 | DateTime.MaxValue は ArgumentException をスロー | 異常系 | 技術仕様書 § 2.3 |

### グループ OPT：`IOptionalValidateWithClock` メソッド

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-OPT-01 | Unset() は IsSet=false のインスタンスを返す | 正常系 | 技術仕様書 § 2.2 |
| VO-OPT-02 | From(value, clock) は IsSet=true のインスタンスを返す | 正常系 | 技術仕様書 § 2.2 |
| VO-OPT-03 | TryFrom(null, clock) は true を返し、Unset インスタンスを返す | 正常系（null 吸収） | 技術仕様書 § 2.3 |
| VO-OPT-04 | TryFrom(valid, clock) は true を返し、設定済みインスタンスを返す | 正常系 | 技術仕様書 § 2.3 |
| VO-OPT-05 | TryFrom(invalid, clock) は false を返し、Unset インスタンスを返す | 異常系（検証失敗） | 技術仕様書 § 2.3 |
| VO-OPT-06 | TryGetValue(out value) は IsSet=true で true を返す | 正常系 | 技術仕様書 § 2.4 |
| VO-OPT-07 | TryGetValue(out value) は IsSet=false で false を返す | 正常系 | 技術仕様書 § 2.4 |

### グループ EQ：`Equals` — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-EQ-01 | 同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価 | 正常系 | 詳細設計書 § 3.4 |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系（自己参照） | 詳細設計書 § 3.4 |
| VO-EQ-03 | 複数コンポーネント（IsSet + ValueField）がすべて一致する場合は等価 | 正常系 | 詳細設計書 § 4 |
| VO-EQ-04 | 両方が IsSet=false かつ値が同じ場合は等価 | 正常系 | 詳細設計書 § 3.4 |

### グループ NE：`Equals` — 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-NE-01 | コンポーネント値が異なる場合は非等価 | 異常系 | 詳細設計書 § 3.4 |
| VO-NE-02 | IsSet が異なる場合は非等価 | 境界値テスト | 詳細設計書 § 3.4 |
| VO-NE-03 | 型が異なる場合は非等価（値が同じでも） | 異常系 | 詳細設計書 § 3.4 |
| VO-NE-04 | null との比較は非等価 | 例外/異常系 | 詳細設計書 § 3.4 |
| VO-NE-05 | 複数コンポーネントの一部が異なる場合は非等価 | 境界値テスト | 詳細設計書 § 3.4 |

### グループ HC：`GetHashCode` ハッシュコード

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値 | 正常系 | 詳細設計書 § 3.5 |
| VO-HC-02 | IsSet が異なるとハッシュ値が異なる | 境界値テスト | 詳細設計書 § 3.5 |
| VO-HC-03 | コンポーネント値が異なるとハッシュ値が異なる | 境界値テスト | 詳細設計書 § 3.5 |
| VO-HC-04 | ハッシュ値は複数呼び出しで一貫している | 正常系（副作用なし） | 詳細設計書 § 3.5 |

### グループ OP：`==` / `!=` 演算子

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-OP-01 | 等価なオブジェクトに == を適用すると true | 正常系 | 詳細設計書 § 3.6 |
| VO-OP-02 | 非等価なオブジェクトに == を適用すると false | 異常系 | 詳細設計書 § 3.6 |
| VO-OP-03 | 両辺が null のとき == は true | 準正常系（null チェック） | 詳細設計書 § 3.6 |
| VO-OP-04 | 片方のみ null のとき == は false | 異常系 | 詳細設計書 § 3.6 |
| VO-OP-05 | != は == の否定と一致 | 正常系 | 詳細設計書 § 3.6 |

### グループ TS：`ToString` 文字列化

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-TS-01 | IsSet=false のとき "Unset" を返す | 正常系 | 詳細設計書 § 3.7 |
| VO-TS-02 | IsSet=true のとき、DateTime を文字列で返す | 正常系 | 詳細設計書 § 3.7 |
| VO-TS-03 | ToString 出力に "IsSet" が含まれない | 正常系 | 詳細設計書 § 3.7 |

---

## 4. テスト検証シナリオ

### シナリオ 4.1：IsSet 管理

**観点**: VO-IS-01, VO-IS-02  
**説明**: IsSet フラグが正しく初期化・管理される

**パターン**:
- From(過去日, clock) で IsSet=true
- Unset() で IsSet=false

**期待結果**: IsSet の値がそれぞれ一致

---

### シナリオ 4.2：Clock 依存検証

**観点**: VO-CLK-01, VO-CLK-02, VO-CLK-03, VO-CLK-04, VO-CLK-05  
**説明**: Clock を基準とした時刻検証が正確

**パターン**:
- From(過去日, clock) → 成功、IsSet=true
- From(現在日時, clock) → 成功、IsSet=true
- From(未来日, clock) → ArgumentException
- From(DateTime.MinValue, clock) → ArgumentException
- From(DateTime.MaxValue, clock) → ArgumentException

**期待結果**: 各ケースが仕様通りの結果を返す

---

### シナリオ 4.3：Optional 値の作成・変換

**観点**: VO-OPT-01, VO-OPT-02, VO-OPT-03, VO-OPT-04, VO-OPT-05  
**説明**: Unset・From・TryFrom の動作が正確

**パターン**:
- Unset() → IsSet=false
- From(過去日, clock) → IsSet=true, 値=指定値
- From(未来日, clock) → ArgumentException
- TryFrom(null, clock) → true, Unset
- TryFrom(過去日, clock) → true, 設定済み
- TryFrom(未来日, clock) → false, Unset

**期待結果**: 各メソッドが仕様通りの値・戻り値を返す

---

### シナリオ 4.4：値の取得（TryGetValue）

**観点**: VO-OPT-06, VO-OPT-07  
**説明**: TryGetValue が IsSet に応じた戻り値を返す

**パターン**:
- IsSet=true で TryGetValue() → true, 値を out に
- IsSet=false で TryGetValue() → false, default(DateTime) を out に

**期待結果**: 戻り値と out パラメータが一致

---

### シナリオ 4.5：等価性（Equals）

**観点**: VO-EQ-01, VO-EQ-02, VO-EQ-03, VO-EQ-04  
**説明**: 同じ値を持つインスタンスは等価

**パターン**:
- From(同じ値, clock) と From(同じ値, clock) → Equals=true
- From(値A, clock) と From(値B, clock) → Equals=false
- Unset() と Unset() → Equals=true
- obj と obj（同一参照） → Equals=true

**期待結果**: Equals の戻り値が一致

---

### シナリオ 4.6：非等価性（Equals）

**観点**: VO-NE-01, VO-NE-02, VO-NE-03, VO-NE-04, VO-NE-05  
**説明**: 異なる値・異なる IsSet・異なる型は非等価

**パターン**:
- From(値A, clock) と From(値B, clock) → Equals=false
- From(値, clock) と Unset() → Equals=false
- From(値, clock) と "値"（文字列） → Equals=false
- obj と null → Equals=false

**期待結果**: すべて false を返す

---

### シナリオ 4.7：ハッシュコード（GetHashCode）

**観点**: VO-HC-01, VO-HC-02, VO-HC-03, VO-HC-04  
**説明**: ハッシュコード整合性を確認

**パターン**:
- From(同じ値, clock) のハッシュと From(同じ値, clock) のハッシュ → 同一
- From(値A, clock) のハッシュと From(値B, clock) のハッシュ → 異なる（高確率）
- Unset() のハッシュと Unset() のハッシュ → 同一
- GetHashCode() の複数呼び出し → 同一値

**期待結果**: 等価なら同一ハッシュ、異なれば異なるハッシュ

---

### シナリオ 4.8：演算子（== / !=）

**観点**: VO-OP-01, VO-OP-02, VO-OP-03, VO-OP-04, VO-OP-05  
**説明**: == / != 演算子が Equals と一貫

**パターン**:
- From(同じ値, clock) == From(同じ値, clock) → true
- From(値A, clock) == From(値B, clock) → false
- null == null → true
- From(値, clock) == null → false
- != は == の否定

**期待結果**: 演算子の戻り値が一致

---

### シナリオ 4.9：文字列化（ToString）

**観点**: VO-TS-01, VO-TS-02, VO-TS-03  
**説明**: ToString が仕様通りのフォーマット

**パターン**:
- Unset().ToString() → "Unset"
- From(2026-07-08T10:30:00, clock).ToString() → "2026-07-08T10:30:00"（DateTime.ToString 形式）
- ToString 出力に "IsSet" が含まれない

**期待結果**: 期待値と一致

---

## 5. テストメソッド命名規則

テストメソッド名は以下の形式とする：

```
{観点ID}_{テスト対象メンバー}_{期待される結果}
```

例：
- `VO_OPT_01_Unset_ReturnsUnsetInstance`
- `VO_CLK_01_FromWithPastDate_ReturnsInstance`
- `VO_CLK_03_FromWithFutureDate_ThrowsException`
- `VO_EQ_01_EqualsWithSameValue_ReturnsTrue`
- `VO_TS_01_ToStringWhenUnset_ReturnsUnsetString`

---

## 6. テスト実装フレームワーク

| 項目 | 内容 |
|------|------|
| **フレームワーク** | xUnit |
| **言語** | C# 13 / .NET 10 |
| **テストプロジェクト** | CarPreferences.Domain.Tests |
| **テストクラス** | RespondentAtTests |

---

## 7. MockClock 実装ガイド

### 7.1 概要

Clock を固定化するため、MockClock を実装：

```csharp
public class MockClock : IClock
{
    private readonly DateTime _fixedTime;

    public MockClock(DateTime fixedTime)
    {
        _fixedTime = fixedTime;
    }

    public DateTime? JstNow => _fixedTime;
}
```

### 7.2 使用パターン

```csharp
var now = new DateTime(2026, 7, 8, 12, 0, 0);
var clock = new MockClock(now);

// 過去日テスト
var pastDate = now.AddHours(-1);
var respondentAt = RespondentAt.From(pastDate, clock);  // ✓ 成功

// 未来日テスト
var futureDate = now.AddHours(1);
Assert.Throws<ArgumentException>(() => RespondentAt.From(futureDate, clock));  // ✓ 例外
```

---

## 8. テストデータ・サンプル値

| データ種別 | サンプル値 | 用途 |
|-----------|---------|------|
| **有効な過去日** | 2026-07-08 10:00:00 | 正常系テスト |
| **現在日時** | 2026-07-08 12:00:00（Clock で固定） | 正常系テスト |
| **有効な未来日** | 2026-07-08 14:00:00 | 異常系テスト（未来日チェック） |
| **MinValue** | DateTime.MinValue | 異常系テスト |
| **MaxValue** | DateTime.MaxValue | 異常系テスト |
| **別の有効値** | 2026-06-01 09:30:00 | 非等価性テスト |

---

## 9. テスト実装上の注意点

### 9.1 Clock は必ずモック化

```csharp
// ✓ 良い例（時刻固定）
var clock = new MockClock(new DateTime(2026, 7, 8, 12, 0, 0));
RespondentAt.From(pastDate, clock);

// ✗ 悪い例（実時刻、テスト不安定）
RespondentAt.From(DateTime.Now, new SystemClock());
```

### 9.2 DateTime 比較は精度に注意

DateTime は Millisecond まで精密なため、テストデータを整数秒で設定：

```csharp
// ✓ 良い例
var dt = new DateTime(2026, 7, 8, 10, 30, 0);

// ✗ 悪い例（Millisecond が異なるとテスト失敗）
var dt = DateTime.Now;
```

### 9.3 TryFrom の戻り値と IsSet を両方確認

```csharp
// ✓ 両方確認
bool success = RespondentAt.TryFrom(null, clock, out var result);
Assert.True(success);
Assert.False(result.IsSet);

// ✗ 不完全（戻り値のみ確認）
var success = RespondentAt.TryFrom(null, clock, out _);
Assert.True(success);
```

---

## 10. 版管理

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-07-08 | 初版作成（DateTime 型 PrimitiveValueObject の単体テスト仕様） |

