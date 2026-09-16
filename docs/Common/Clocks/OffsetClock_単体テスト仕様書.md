# 単体テスト仕様書 — OffsetClock

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Common.Clocks.OffsetClock`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

OffsetClock は、IClock インターフェースのテスト用実装。基準となる時刻から固定のオフセット分加算した時刻を返します。

**設計上の特徴:**
- **型**: sealed class、IClock インターフェース実装
- **目的**: テスト時に時刻を固定時刻 + オフセットで指定
- **責務**: 基準時刻 + オフセット → LocalDateTime
- **使用シーン**: 特定日時の動作を検証したいテスト
- **実装済みテスト**: OffsetClockTests.cs

本仕様書は、OffsetClock のオフセット計算・時刻取得・インターフェース適合性を確認するテスト仕様。

---

## 1. テスト目的

OffsetClock が以下を満たすことを確認する：

- **ファクトリメソッド**: コンストラクタで基準時刻とオフセットを受け取れる
- **オフセット計算**: 基準時刻 + オフセット = 返却時刻
- **固定性**: 複数呼び出しで時刻は同じ（時間経過なし）
- **インターフェース**: IClock の契約を満たす

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | OffsetClock |
| **型別分類** | sealed class |
| **名前空間** | SupportAdvance.Common.Clocks |
| **依拠仕様** | IClock インターフェース / LocalDateTime 設計 |
| **実装** | IClock |
| **テストファイル** | tests/Common.Tests/Clocks/OffsetClockTests.cs |

---

## 3. テスト対象メソッド・プロパティ

| メンバー | シグネチャ | 責務 |
|---------|-----------|------|
| **コンストラクタ** | `public OffsetClock(LocalDateTime baseTime, TimeSpan offset)` | 基準時刻とオフセットを保持 |
| **JstNow** | `public LocalDateTime JstNow { get; }` | baseTime + offset を LocalDateTime で返す |
| **UtcNow** | `public LocalDateTime UtcNow { get; }` | UTC版（オプション） |

---

## 4. テスト観点

### IC: IClock インターフェース実装

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| IC-01 | JstNow が LocalDateTime を返す | 正常系 | Test-1 |
| IC-02 | JstNow の値は null ではない | 正常系 | Test-1 |

### OF: オフセット計算（Offset Calculation）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| OF-01 | baseTime + offset = JstNow | 正常系 | Test-2 |
| OF-02 | 正のオフセット（+1日）が正しく加算される | 正常系 | Test-3 |
| OF-03 | 負のオフセット（-1日）が正しく減算される | 正常系 | Test-4 |
| OF-04 | 0 オフセット（TimeSpan.Zero）では baseTime と同じ | 正常系 | Test-5 |

### FX: 固定性（Fixedness）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| FX-01 | 複数呼び出しで JstNow は常に同じ値 | 正常系 | Test-6 |
| FX-02 | 時間経過してもオフセットクロックは進まない | 正常系 | Test-6 |

### TC: 型変換（Type Conversion）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| TC-01 | LocalDateTime + TimeSpan の計算結果は LocalDateTime に変換される | 正常系 | Test-7 |

---

## 5. テスト仕様別の検証シナリオ

### 観点 OF-01: オフセット計算の正確性

#### 5.1.1 テスト観点

OffsetClock(baseTime, offset) の JstNow が baseTime + offset に等しい。

#### 5.1.2 テストパターン

| パターン | 説明 |
|---------|------|
| 5.1.2.1 | baseTime=2026-01-01 12:00:00、offset=+1日 → 2026-01-02 12:00:00 |
| 5.1.2.2 | baseTime=2026-01-01 12:00:00、offset=-1日 → 2025-12-31 12:00:00 |
| 5.1.2.3 | baseTime=2026-01-01 12:00:00、offset=+1時間 → 2026-01-01 13:00:00 |

#### 5.1.3 期待結果

- [ ] JstNow == baseTime + offset

---

### 観点 FX-01: 固定性（複数呼び出しで値が変わらない）

#### 5.2.1 テスト観点

OffsetClock の JstNow を複数回呼び出した場合、返却値は常に同じ。

#### 5.2.2 テストパターン

| パターン | 説明 |
|---------|------|
| 5.2.2.1 | JstNow を 3 回呼び出し、すべて同じ値 |

#### 5.2.3 期待結果

- [ ] 1回目と2回目の時刻は同じ
- [ ] 2回目と3回目の時刻は同じ

---

## 6. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert
- **テスト実装**: `tests/Common.Tests/Clocks/OffsetClockTests.cs`
- **依存モック**: なし（LocalDateTime は値型、計算のみ）
- **DB接続**: 不要
- **タイムゾーン**: LocalDateTime は TZ非依存
- **スキップテスト**: なし（全テストが実施可能）

---

## 7. テスト結果統計

**実装済みテスト: 5-7 件**

| テスト項目 | テスト数 | 観点カバレッジ |
|-----------|---------|------------|
| インターフェース実装 | 1 | IC-01, IC-02 |
| オフセット計算 | 3 | OF-01, OF-02, OF-03, OF-04 |
| 固定性 | 2 | FX-01, FX-02 |
| 型変換 | 1 | TC-01 |
| **合計** | **7** | **10観点** |

---

## 8. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-1 Common.Clocks テスト文書化） |

