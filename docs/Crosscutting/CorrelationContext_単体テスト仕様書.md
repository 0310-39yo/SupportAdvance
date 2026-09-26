# 単体テスト仕様書 — CorrelationContext

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Crosscutting.Logging.CorrelationContext`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

CorrelationContext は、AsyncLocal を使用した相関ID（Correlation ID）の管理フレームワークです。

**設計上の特徴:**
- **型**: sealed class、静的相関ID管理
- **目的**: 分散トレーシング用に、リクエスト全体を通じて同一の相関IDを保持
- **責務**: Set/Get/Clear による相関ID管理、AsyncLocal による非同期コンテキスト分離
- **実装済みテスト**: CorrelationContextTests.cs

本仕様書は、CorrelationContext の相関ID管理・AsyncLocal分離・スレッド安全性を確認するテスト仕様。

---

## 1. テスト目的

CorrelationContext が以下を満たすことを確認する：

- **相関ID管理**: Set/Get で相関ID を保存・取得できる
- **デフォルト値**: 未設定時に自動生成される（またはデフォルト値を返す）
- **AsyncLocal分離**: 非同期タスク間で相関ID が独立している
- **Clear機能**: Clear で相関ID をリセットできる

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | CorrelationContext |
| **名前空間** | SupportAdvance.Crosscutting.Logging |
| **型** | sealed static class（静的API） |
| **テストファイル** | tests/Crosscutting.Tests/Logging/CorrelationContextTests.cs |

---

## 3. テスト対象メソッド

| メソッド | シグネチャ | 責務 |
|---------|-----------|------|
| **Set** | `public static void Set(string correlationId)` | 相関ID を設定 |
| **Get** | `public static string? Get()` | 相関ID を取得 |
| **GetOrCreate** | `public static string GetOrCreate()` | 相関ID を取得（未設定時は自動生成） |
| **Clear** | `public static void Clear()` | 相関ID をリセット |

---

## 4. テスト観点

### CR: 相関ID管理（Correlation ID）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| CR-01 | Set で相関ID を設定できる | 正常系 | Test-1 |
| CR-02 | Get で設定した相関ID を取得できる | 正常系 | Test-1 |
| CR-03 | 複数の Set で最後の値が有効 | 正常系 | Test-2 |

### CR-DEF: デフォルト値（Default Value）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| CR-DEF-01 | 未設定時に Get は null を返す | 正常系 | Test-3 |
| CR-DEF-02 | GetOrCreate で自動生成される | 正常系 | Test-4 |
| CR-DEF-03 | 自動生成値は一意（GUID等） | 正常系 | Test-5 |

### AL: AsyncLocal分離（Async Local）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| AL-01 | 異なるタスク間で相関ID が独立 | 正常系 | Test-6 |
| AL-02 | 親タスク終了後、子タスク独立 | 正常系 | Test-7 |

### CL: Clear機能（Clear）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| CL-01 | Clear で相関ID がリセット | 正常系 | Test-8 |
| CL-02 | Clear 後の Get は null を返す | 正常系 | Test-8 |

---

## 5. テスト仕様別の検証シナリオ

### 観点 AL-01: 非同期タスク間での独立性

#### 5.1.1 テスト観点

Task A で Set した相関ID が、並行実行の Task B に影響しない。

#### 5.1.2 テストパターン

| パターン | 説明 |
|---------|------|
| 5.1.2.1 | Task A: Set("corr-A")、Task B: Set("corr-B")、並行実行 |
| 5.1.2.2 | Task B 内 Get は "corr-B"、Task A 内 Get は "corr-A" |

#### 5.1.3 期待結果

- [ ] Task A と Task B が異なる相関ID を保持
- [ ] 値が交錯しない

---

## 6. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert
- **テスト実装**: `tests/Crosscutting.Tests/Logging/CorrelationContextTests.cs`
- **非同期処理**: すべてのテストで async/await を使用
- **並行実行**: Task.WhenAll で複数タスク同時実行
- **クリーンアップ**: 各テスト終了時に Clear を呼び出し

---

## 7. テスト結果統計

**実装済みテスト: 8 件**

| テスト項目 | テスト数 | 観点カバレッジ |
|-----------|---------|------------|
| 相関ID管理 | 2 | CR-01, CR-02, CR-03 |
| デフォルト値 | 2 | CR-DEF-01, CR-DEF-02, CR-DEF-03 |
| AsyncLocal分離 | 2 | AL-01, AL-02 |
| Clear機能 | 2 | CL-01, CL-02 |
| **合計** | **8** | **10観点** |

---

## 8. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-5 Crosscutting層テスト文書化） |

