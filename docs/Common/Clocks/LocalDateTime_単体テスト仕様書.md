# 単体テスト仕様書 — LocalDateTime

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Common.Clocks.LocalDateTime`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

LocalDateTime は、タイムゾーン非依存の「ローカル日時」を表す ValueObject（struct）。

**設計上の特徴:**
- **型**: DateTime ラッパー構造体
- **目的**: タイムゾーンの曖昧性を排除し、Domain/Application層では常に LocalDateTime を使用
- **責務**: 日時の型安全な管理、比較・加減演算、文字列化
- **実装済みテスト**: LocalDateTimeTests.cs（比較演算子・等価性・ファクトリメソッド）

本仕様書は、LocalDateTime の等価性・比較・ファクトリメソッド・演算子の動作を確認するテスト仕様。

---

## 1. テスト目的

LocalDateTime が以下を満たすことを確認する：

- **ファクトリメソッド**: コンストラクタ / From() で正しく構築できる
- **等価性**: 同じ値を持つ 2 つのインスタンスが等価である
- **比較演算**: < / <= / > / >= / == / != が時系列を正しく反映する
- **ハッシュ整合性**: Equals=true のインスタンスは同一ハッシュ値
- **文字列化**: ToString が ISO 8601 形式または予定の形式を返す
- **日時演算**: AddDays/AddHours など算術操作が正しく動作

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | LocalDateTime |
| **型別分類** | struct（値型） |
| **名前空間** | SupportAdvance.Common.Clocks |
| **依拠仕様** | LocalDateTime タイムゾーン設計ガイド v1.0 |
| **継承** | IComparable\<LocalDateTime\>, IEquatable\<LocalDateTime\> |
| **テストファイル** | tests/Common.Tests/Clocks/LocalDateTimeTests.cs |

---

## 3. テスト対象メソッド・プロパティ

| メンバー | シグネチャ | 責務 |
|---------|-----------|------|
| **コンストラクタ** | `public LocalDateTime(DateTime value)` | DateTime から LocalDateTime を構築 |
| **Value** | `public DateTime Value { get; }` | 保持する DateTime を取得 |
| **Year/Month/Day/Hour/Minute/Second** | プロパティ | 日時要素の取得 |
| **Equals** | `public override bool Equals(object? obj)` / `public bool Equals(LocalDateTime other)` | 等価性判定 |
| **CompareTo** | `public int CompareTo(LocalDateTime other)` | 大小比較 |
| **GetHashCode** | `public override int GetHashCode()` | ハッシュ値取得 |
| **ToString** | `public override string ToString()` | 文字列化 |
| **==, !=, <, <=, >, >=** | 演算子 | 等価・比較演算 |
| **AddDays/AddHours/AddSeconds** | `public LocalDateTime Add*(int value)` | 日時加減算 |

---

## 4. テスト観点

### VO-EQ: Equals — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EQ-01 | 同じ DateTime 値から生成した 2 つのオブジェクトは等価 | 正常系 | Test-1 |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系 | 内包 |

### VO-NE: Equals — 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-NE-01 | 異なる DateTime 値は非等価 | 異常系 | Test-2 |
| VO-NE-02 | 1 ミリ秒でも異なれば非等価 | 境界値 | Test-2 |

### VO-HC: GetHashCode — ハッシュ整合性

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値 | 正常系 | Test-3 |

### VO-OP: 比較演算子

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-OP-01 | a == b のとき == true | 正常系 | 内包（Test-1） |
| VO-OP-02 | a != b のとき != true | 正常系 | 内包（Test-2） |
| VO-OP-03 | a < b かつ b > a | 正常系 | Test-4 |
| VO-OP-04 | a <= b かつ a < b のとき true | 正常系 | 内包 |
| VO-OP-05 | a >= b かつ a > b のとき true | 正常系 | 内包 |

### CA: 加減算演算（算術操作）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| CA-01 | AddDays(1) で 1 日加算される | 正常系 | Test-5 |
| CA-02 | AddHours(-1) で 1 時間減算される | 正常系 | Test-6 |
| CA-03 | AddSeconds(3600) で 1 時間加算される | 正常系 | Test-7 |

### VO-TS: ToString

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-TS-01 | ToString() で日時文字列を返す | 正常系 | Test-8 |

---

## 5. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert
- **テスト実装**: `tests/Common.Tests/Clocks/LocalDateTimeTests.cs`
- **依存モック**: なし（ValueObject は外部依存なし）
- **DB接続**: 不要
- **タイムゾーン**: テスト実行環境に依存しない（LocalDateTime は TZ非依存）
- **テストデータ**: 任意の有効な DateTime 値

---

## 6. テスト結果統計

**実装済みテスト: 概算 8-10 件**

| テスト項目 | テスト数 | 観点カバレッジ |
|-----------|---------|------------|
| 等価性 | 2 | VO-EQ-01, VO-NE-01 |
| 比較演算子 | 4 | VO-OP-03, VO-OP-04, VO-OP-05 |
| 加減算演算 | 3 | CA-01, CA-02, CA-03 |
| 文字列化 | 1 | VO-TS-01 |
| **合計** | **10+** | **11観点** |

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-1 Common.Clocks テスト文書化） |

