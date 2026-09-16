# 単体テスト仕様書 — TickingClock

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Common.Clocks.TickingClock`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

TickingClock は、IClock インターフェースのテスト用実装。初期時刻から呼び出しのたびに指定のインクリメント分進む「時間が流れるクロック」を実装します。

**設計上の特徴:**
- **型**: sealed class、IClock インターフェース実装
- **目的**: テスト時に時間経過をシミュレート
- **責務**: 初期時刻 + 呼び出し回数 × インクリメント
- **使用シーン**: 時間経過とともに動作が変わるロジックの検証
- **実装済みテスト**: TickingClockTests.cs

本仕様書は、TickingClock の時間進行・インクリメント計算・インターフェース適合性を確認するテスト仕様。

---

## 1. テスト目的

TickingClock が以下を満たすことを確認する：

- **ファクトリメソッド**: 初期時刻とインクリメントを受け取れる
- **時間進行**: 呼び出すたびに指定のインクリメント分進む
- **呼び出し回数依存**: n回目の呼び出しは「初期時刻 + n×インクリメント」
- **インターフェース**: IClock の契約を満たす

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | TickingClock |
| **型別分類** | sealed class |
| **名前空間** | SupportAdvance.Common.Clocks |
| **依拠仕様** | IClock インターフェース / LocalDateTime 設計 |
| **実装** | IClock |
| **テストファイル** | tests/Common.Tests/Clocks/TickingClockTests.cs |

---

## 3. テスト対象メソッド・プロパティ

| メンバー | シグネチャ | 責務 |
|---------|-----------|------|
| **コンストラクタ** | `public TickingClock(LocalDateTime initialTime, TimeSpan increment)` | 初期時刻とインクリメントを保持 |
| **JstNow** | `public LocalDateTime JstNow { get; }` | initialTime + (呼び出し回数 × increment) を返す |
| **UtcNow** | `public LocalDateTime UtcNow { get; }` | UTC版（オプション） |
| **Restart()** | `public void Restart()` | 呼び出し回数をリセット |
| **Advance(TimeSpan)** | `public void Advance(TimeSpan amount)` | 手動で時刻を進める（オプション） |

---

## 4. テスト観点

### IC: IClock インターフェース実装

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| IC-01 | JstNow が LocalDateTime を返す | 正常系 | Test-1 |
| IC-02 | JstNow の値は null ではない | 正常系 | Test-1 |

### TI: 時間進行（Time Increment）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| TI-01 | 1回目呼び出し = initialTime | 正常系 | Test-2 |
| TI-02 | 2回目呼び出し = initialTime + increment | 正常系 | Test-2 |
| TI-03 | 3回目呼び出し = initialTime + 2×increment | 正常系 | Test-3 |
| TI-04 | n回目呼び出し = initialTime + (n-1)×increment | 正常系 | Test-4 |

### IN: インクリメント（Increment）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| IN-01 | increment=+1日 での時間進行が正しい | 正常系 | Test-5 |
| IN-02 | increment=-1時間 での逆進行が正しい | 正常系 | Test-6 |
| IN-03 | increment=TimeSpan.Zero では時刻が変わらない | 正常系 | Test-7 |

### RS: リセット（Restart）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| RS-01 | Restart() 後の最初の呼び出し = initialTime | 正常系 | Test-8 |
| RS-02 | Restart() で呼び出し回数がリセットされる | 正常系 | Test-8 |

---

## 5. テスト仕様別の検証シナリオ

### 観点 TI-01～04: 時間進行の正確性

#### 5.1.1 テスト観点

TickingClock(initialTime, increment) の JstNow を複数回呼び出した場合、時間が期待通り進む。

#### 5.1.2 テストパターン

| パターン | 説明 |
|---------|------|
| 5.1.2.1 | 初期=2026-01-01 00:00:00、increment=+1日 |
| 5.1.2.2 | 1回目 → 2026-01-01 00:00:00 |
| 5.1.2.3 | 2回目 → 2026-01-02 00:00:00 |
| 5.1.2.4 | 3回目 → 2026-01-03 00:00:00 |

#### 5.1.3 期待結果

| 呼び出し回数 | 期待時刻 | 検証項目 |
|-----------|--------|---------|
| 1回目 | initialTime | 初期時刻と等しい |
| 2回目 | initialTime + 1×increment | 1回分進んでいる |
| 3回目 | initialTime + 2×increment | 2回分進んでいる |

#### 5.1.4 判定基準

- [ ] 複数呼び出しで期待通りに時刻が進む

---

### 観点 RS-01～02: リセット機能

#### 5.2.1 テスト観点

Restart() 後の JstNow は initialTime に戻る。

#### 5.2.2 テストパターン

| パターン | 説明 |
|---------|------|
| 5.2.2.1 | 初回呼び出し |
| 5.2.2.2 | Restart() 実行 |
| 5.2.2.3 | 再度最初の呼び出し |

#### 5.2.3 期待結果

- [ ] Restart() 前後で、最初の呼び出しが同じ値

---

## 6. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert
- **テスト実装**: `tests/Common.Tests/Clocks/TickingClockTests.cs`
- **依存モック**: なし（LocalDateTime は値型、計算のみ）
- **DB接続**: 不要
- **スキップテスト**: なし（全テストが実施可能）
- **スレッド安全性**: 単一スレッドでのテストを想定

---

## 7. テスト結果統計

**実装済みテスト: 6-8 件**

| テスト項目 | テスト数 | 観点カバレッジ |
|-----------|---------|------------|
| インターフェース実装 | 1 | IC-01, IC-02 |
| 時間進行 | 3 | TI-01, TI-02, TI-03, TI-04 |
| インクリメント | 3 | IN-01, IN-02, IN-03 |
| リセット | 2 | RS-01, RS-02 |
| **合計** | **9** | **13観点** |

---

## 8. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-1 Common.Clocks テスト文書化） |

