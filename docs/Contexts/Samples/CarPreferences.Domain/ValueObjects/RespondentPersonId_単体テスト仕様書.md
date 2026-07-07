# 単体テスト仕様書 — RespondentPersonId

**プロジェクト:** SupportAdvance  
**テスト対象:** ValueObject（RespondentPersonId）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-07-08

---

## 0. 本書の位置づけ

本書は、Domain 層の ValueObject クラス `RespondentPersonId` が、技術仕様書および詳細設計書で定義された以下の特性を満たすことを確認するテスト仕様書である：

- **値の不変性**: オブジェクト生成後、内部状態が変更されない
- **等価性比較**: 同じ値を持つ2つのオブジェクトは等価
- **ハッシュ整合性**: Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**: ToString が仕様通りの形式で返される
- **IsSet状態管理**: IsSet フラグが正しく機能する
- **Optional値操作**: Unset・From・TryFrom・TryGetValue の動作

---

## 1. テスト目的

RespondentPersonId の各メンバーが以下を満たすことを確認：

1. **値オブジェクトセマンティクス**: 不変性、等価性、ハッシュコード整合性
2. **IOptionalValueObject 契約**: Unset、From、TryFrom、TryGetValue の動作
3. **IEquatable 実装**: 型安全な等価性判定
4. **バリデーション**: 1000～9999 の範囲チェック
5. **null 安全性**: Try パターンで例外を最小化

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | RespondentPersonId |
| **名前空間** | SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects |
| **依拠仕様** | RespondentPersonId 技術仕様書 v1.0 + 詳細設計書 v1.0 |
| **前提** | ValueObject・PrimitiveValueObject のテストが完了していること |

---

## 2.X テスト用実装と DI 設定

| 実装名 | 役割 | 対応する観点 | 説明 |
|--------|------|------------|------|
| Test Constructor | テスト用コンストラクタ | VO-IS-01～02, VO-OPT-01～07 | RespondentPersonId の初期化検証 |
| Equals/GetHashCode | 標準メソッド | VO-EQ, VO-NE, VO-HC, VO-OP, VO-TS | .NET の Equals・GetHashCode・ToString 検証 |

---

## 3. テスト観点一覧

### グループ IS：`IsSet` プロパティ

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-IS-01 | IsSet=true で構築したオブジェクトは IsSet が true を返す | 正常系 | § 2.1 |
| VO-IS-02 | IsSet=false で構築したオブジェクトは IsSet が false を返す | 正常系 | § 2.1 |

### グループ OPT：`IOptionalValueObject` メソッド

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-OPT-01 | Unset() は IsSet=false のインスタンスを返す | 正常系 | 技術仕様書 § 2.3 |
| VO-OPT-02 | From(value) は IsSet=true のインスタンスを返す | 正常系 | 技術仕様書 § 2.4 |
| VO-OPT-03 | TryFrom(null) は true を返し、Unset インスタンスを返す | 正常系（null吸収） | 技術仕様書 § 2.6 |
| VO-OPT-04 | TryFrom(valid) は true を返し、設定済みインスタンスを返す | 正常系 | 技術仕様書 § 2.6 |
| VO-OPT-05 | TryFrom(invalid) は false を返し、Unset インスタンスを返す | 異常系（検証失敗） | 技術仕様書 § 2.6 |
| VO-OPT-06 | TryGetValue(out value) は IsSet=true で true を返す | 正常系 | 技術仕様書 § 2.7 |
| VO-OPT-07 | TryGetValue(out value) は IsSet=false で false を返す | 正常系 | 技術仕様書 § 2.7 |

### グループ EQ：`Equals` — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-EQ-01 | 同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価 | 正常系 | 詳細設計書 § 3.8 |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系（自己参照） | 詳細設計書 § 3.8 |
| VO-EQ-03 | 複数コンポーネント（IsSet + ValueField）がすべて一致する場合は等価 | 正常系 | 詳細設計書 § 3.11 |
| VO-EQ-04 | 両方が IsSet=false かつ値が同じ場合は等価 | 正常系 | 詳細設計書 § 3.8 |

### グループ NE：`Equals` — 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-NE-01 | コンポーネント値が異なる場合は非等価 | 異常系 | 詳細設計書 § 3.8 |
| VO-NE-02 | IsSet が異なる場合は非等価 | 境界値テスト | 詳細設計書 § 3.8 |
| VO-NE-03 | 型が異なる場合は非等価（値が同じでも） | 異常系 | 詳細設計書 § 3.8 |
| VO-NE-04 | null との比較は非等価 | 例外/異常系 | 詳細設計書 § 3.8 |
| VO-NE-05 | 複数コンポーネントの一部が異なる場合は非等価 | 境界値テスト | 詳細設計書 § 3.8 |

### グループ HC：`GetHashCode` ハッシュコード

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値 | 正常系 | 詳細設計書 § 3.10 |
| VO-HC-02 | IsSet が異なるとハッシュ値が異なる | 境界値テスト | 詳細設計書 § 3.10 |
| VO-HC-03 | コンポーネント値が異なるとハッシュ値が異なる | 境界値テスト | 詳細設計書 § 3.10 |
| VO-HC-04 | ハッシュ値は複数呼び出しで一貫している | 正常系（副作用なし） | 詳細設計書 § 3.10 |

### グループ OP：`==` / `!=` 演算子

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-OP-01 | 等価なオブジェクトに == を適用すると true | 正常系 | 詳細設計書 § 3.9 |
| VO-OP-02 | 非等価なオブジェクトに == を適用すると false | 異常系 | 詳細設計書 § 3.9 |
| VO-OP-03 | 両辺が null のとき == は true | 準正常系（null チェック） | 詳細設計書 § 3.9 |
| VO-OP-04 | 片方のみ null のとき == は false | 異常系 | 詳細設計書 § 3.9 |
| VO-OP-05 | != は == の否定と一致 | 正常系 | 詳細設計書 § 3.9 |

### グループ TS：`ToString` 文字列化

| 観点ID | 観点（説明） | 分類 | 依拠仕様 |
|--------|------|------|---------|
| VO-TS-01 | IsSet=false のとき "Unset" を返す | 正常系 | 詳細設計書 § 3.12 |
| VO-TS-02 | IsSet=true のとき、コンポーネントを文字列連結で返す | 正常系 | 詳細設計書 § 3.12 |
| VO-TS-03 | ToString 出力に IsSet の値そのものが含まれない | 正常系 | 詳細設計書 § 3.12 |

---

## 4. テスト検証シナリオ

### シナリオ 4.1：IsSet 管理

**観点**: VO-IS-01, VO-IS-02  
**説明**: IsSet フラグが正しく初期化・管理される

**パターン**:
- From(1000) で IsSet=true
- Unset() で IsSet=false

**期待結果**: IsSet の値がそれぞれ一致

---

### シナリオ 4.2：Optional値の作成・変換

**観点**: VO-OPT-01, VO-OPT-02, VO-OPT-03, VO-OPT-04, VO-OPT-05  
**説明**: Unset・From・TryFrom の動作が正確

**パターン**:
- Unset() → IsSet=false
- From(1500) → IsSet=true, 値=1500
- From(500) → ArgumentOutOfRangeException
- TryFrom(null) → true, Unset
- TryFrom(2000) → true, 設定済み
- TryFrom(999) → false, Unset

**期待結果**: 各メソッドが仕様通りの値・戻り値を返す

---

### シナリオ 4.3：値の取得（TryGetValue）

**観点**: VO-OPT-06, VO-OPT-07  
**説明**: TryGetValue が IsSet に応じた戻り値を返す

**パターン**:
- IsSet=true で TryGetValue() → true, 値を out に
- IsSet=false で TryGetValue() → false, 0 を out に

**期待結果**: 戻り値と out パラメータが一致

---

### シナリオ 4.4：等価性（Equals）

**観点**: VO-EQ-01, VO-EQ-02, VO-EQ-03, VO-EQ-04  
**説明**: 同じ値を持つインスタンスは等価

**パターン**:
- From(1500) と From(1500) → Equals=true
- From(1500) と From(2000) → Equals=false
- Unset() と Unset() → Equals=true
- obj と obj（同一参照） → Equals=true

**期待結果**: Equals の戻り値が一致

---

### シナリオ 4.5：非等価性（Equals）

**観点**: VO-NE-01, VO-NE-02, VO-NE-03, VO-NE-04, VO-NE-05  
**説明**: 異なる値・異なる IsSet・異なる型は非等価

**パターン**:
- From(1500) と From(2000) → Equals=false
- From(1500) と Unset() → Equals=false
- From(1500) と "1500"（文字列） → Equals=false
- obj と null → Equals=false

**期待結果**: すべて false を返す

---

### シナリオ 4.6：ハッシュコード（GetHashCode）

**観点**: VO-HC-01, VO-HC-02, VO-HC-03, VO-HC-04  
**説明**: ハッシュコード整合性を確認

**パターン**:
- From(1500) のハッシュと From(1500) のハッシュ → 同一
- From(1500) のハッシュと From(2000) のハッシュ → 異なる（高確率）
- Unset() のハッシュと Unset() のハッシュ → 同一
- GetHashCode() の複数呼び出し → 同一値

**期待結果**: 等価なら同一ハッシュ、異なれば異なるハッシュ

---

### シナリオ 4.7：演算子（== / !=）

**観点**: VO-OP-01, VO-OP-02, VO-OP-03, VO-OP-04, VO-OP-05  
**説明**: == / != 演算子が Equals と一貫

**パターン**:
- From(1500) == From(1500) → true
- From(1500) == From(2000) → false
- null == null → true
- From(1500) == null → false
- != は == の否定

**期待結果**: 演算子の戻り値が一致

---

### シナリオ 4.8：文字列化（ToString）

**観点**: VO-TS-01, VO-TS-02, VO-TS-03  
**説明**: ToString が仕様通りのフォーマット

**パターン**:
- Unset().ToString() → "Unset"
- From(1500).ToString() → "1500"（または仕様通りのフォーマット）
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
- `VO_EQ_01_EqualsWithSameValue_ReturnsTrue`
- `VO_NE_04_EqualsWithNull_ReturnsFalse`
- `VO_TS_01_ToStringWhenUnset_ReturnsUnsetString`

---

## 6. テスト実装フレームワーク

| 項目 | 内容 |
|------|------|
| **フレームワーク** | xUnit |
| **言語** | C# 13 / .NET 10 |
| **テストプロジェクト** | CarPreferences.Domain.Tests |
| **テストクラス** | RespondentPersonIdTests |

---

## 7. 版管理

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-01-10 | 初版作成 |

