# 単体テスト仕様書 — SystemClock

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Common.Clocks.SystemClock`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

SystemClock は、IClock インターフェースの本番環境実装。システムの現在時刻を LocalDateTime で返します。

**設計上の特徴:**
- **型**: sealed class、IClock インターフェース実装
- **目的**: 本番環境でシステム時刻を取得
- **タイムゾーン**: JST（日本標準時）で統一
- **責務**: DateTime.Now → LocalDateTime への変換
- **実装済みテスト**: SystemClockTests.cs

本仕様書は、SystemClock の時刻取得・タイムゾーン変換・インターフェース適合性を確認するテスト仕様。

---

## 1. テスト目的

SystemClock が以下を満たすことを確認する：

- **時刻取得**: JstNow で現在の JST 時刻を LocalDateTime で返す
- **単調性**: 複数呼び出しで時間は進む（または等しい）
- **型変換**: DateTime → LocalDateTime への変換が正しい
- **インターフェース**: IClock の契約を満たす

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | SystemClock |
| **型別分類** | sealed class |
| **名前空間** | SupportAdvance.Common.Clocks |
| **依拠仕様** | IClock インターフェース / LocalDateTime 設計 |
| **実装** | IClock |
| **テストファイル** | tests/Common.Tests/Clocks/SystemClockTests.cs |

---

## 3. テスト対象メソッド・プロパティ

| メンバー | シグネチャ | 責務 |
|---------|-----------|------|
| **JstNow** | `public LocalDateTime JstNow { get; }` | 現在の JST 時刻を LocalDateTime で取得 |
| **UtcNow** | `public LocalDateTime UtcNow { get; }` | 現在の UTC 時刻を LocalDateTime で取得（オプション） |

---

## 4. テスト観点

### IC: IClock インターフェース実装

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| IC-01 | JstNow が LocalDateTime を返す | 正常系 | Test-1 |
| IC-02 | JstNow の値は null ではない | 正常系 | Test-1 |

### TZ: タイムゾーン（時刻取得）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| TZ-01 | JstNow で取得した時刻は JST（UTC+9） | 正常系 | Test-2 |
| TZ-02 | JstNow と UtcNow の差は約 9 時間 | 正常系 | Test-3 |

### MO: 単調性（Monotonicity）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| MO-01 | 連続した JstNow 呼び出しは単調非減少 | 正常系 | Test-4 |
| MO-02 | 短時間での呼び出しでは時刻が同じ場合がある | 正常系 | 内包 |

### TC: 型変換（Type Conversion）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| TC-01 | DateTime → LocalDateTime への変換は情報損失がない | 正常系 | Test-5 |

---

## 5. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert
- **テスト実装**: `tests/Common.Tests/Clocks/SystemClockTests.cs`
- **依存モック**: なし（System.DateTime.Now は実行環境に依存）
- **DB接続**: 不要
- **タイムゾーン**: テスト実行環境が JST である前提（またはタイムゾーン非依存テスト）
- **時刻依存**: テストは実行時刻に依存（固定値での比較は不可）
- **スキップテスト**: なし（全テストが実施可能）

---

## 6. テスト結果統計

**実装済みテスト: 4-5 件**

| テスト項目 | テスト数 | 観点カバレッジ |
|-----------|---------|------------|
| インターフェース実装 | 1 | IC-01, IC-02 |
| タイムゾーン | 2 | TZ-01, TZ-02 |
| 単調性 | 1 | MO-01, MO-02 |
| 型変換 | 1 | TC-01 |
| **合計** | **5** | **8観点** |

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-1 Common.Clocks テスト文書化） |

