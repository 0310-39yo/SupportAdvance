# 単体テスト仕様書 — SequenceProvider

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Infrastructure.Providers.SequenceProvider`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

SequenceProvider は、データベースの Sequence（採番機構）から一意のシーケンス値を取得する Infrastructure Service です。

**設計上の特徴:**
- **型**: sealed class、ISequenceProvider インターフェース実装
- **目的**: t_employees.row_id・t_departments.row_id など、各テーブルのPK採番
- **責務**: SQL Server の NEXT VALUE FOR s_row_id_sequence を実行して次の採番値を取得
- **実装済みテスト**: SequenceProviderTests.cs（本物の SequenceProvider を対象）

本仕様書は、SequenceProvider の採番値取得・一貫性・連続性を確認するテスト仕様。

---

## 1. テスト目的

SequenceProvider が以下を満たすことを確認する：

- **採番値取得**: GetNextIdAsync で次の一意なID値を取得できる
- **連続性**: 複数呼び出しで、採番値が単調増加する
- **スレッド安全性**: 並行呼び出しでも採番値が重複しない
- **DB連続性**: SQL Server Sequence と同期している

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | SequenceProvider |
| **名前空間** | SupportAdvance.Infrastructure.Providers |
| **依拠仕様** | ISequenceProvider インターフェース |
| **実装** | ISequenceProvider |
| **テストファイル** | tests/Infrastructure.Tests/Providers/SequenceProviderTests.cs |

---

## 3. テスト対象メソッド

| メソッド | シグネチャ | 責務 |
|---------|-----------|------|
| **GetNextIdAsync** | `Task<long> GetNextIdAsync()` | 次の採番値を SQL Server Sequence から取得 |

---

## 4. テスト観点

### SQ: Sequence 採番（Sequencing）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| SQ-01 | GetNextIdAsync で正の long 値が返される | 正常系 | Test-1 |
| SQ-02 | 複数呼び出しで異なる値が返される | 正常系 | Test-2 |
| SQ-03 | 採番値が単調増加する（n < n+1） | 正常系 | Test-3 |

### CO: 並行実行（Concurrency）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| CO-01 | 並行呼び出しで採番値が重複しない | 正常系 | Test-4 |
| CO-02 | スレッドセーフに複数のタスクが実行される | 正常系 | Test-5 |

### DB: データベース連続性（Database Sync）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|-----|------|-----------|
| DB-01 | SQL Server s_row_id_sequence と同期している | 正常系 | Test-6 |

---

## 5. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert
- **テスト実装**: `tests/Infrastructure.Tests/Providers/SequenceProviderTests.cs`
- **DB接続**: SQL Server への実接続が必須（テスト用 Sequence が必要）
- **初期値**: s_row_id_sequence の START WITH は大きな値（スキップ領域回避）
- **スレッド安全性**: Task.WhenAll で並行実行テスト

---

## 6. テスト結果統計

**実装済みテスト: 6 件**

| テスト項目 | テスト数 | 観点カバレッジ |
|-----------|---------|------------|
| 採番値取得 | 3 | SQ-01, SQ-02, SQ-03 |
| 並行実行 | 2 | CO-01, CO-02 |
| DB同期 | 1 | DB-01 |
| **合計** | **6** | **6観点** |

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-5 横断的関心事テスト文書化） |

