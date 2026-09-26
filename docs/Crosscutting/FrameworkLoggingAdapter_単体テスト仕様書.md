# 単体テスト仕様書 — FrameworkLoggingAdapter<T>

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Crosscutting.Logging.FrameworkLoggingAdapter<T>`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

FrameworkLoggingAdapter は、Microsoft.Extensions.Logging.ILogger を Application・Domain層のジェネリック T に適応させるアダプタクラスです。

**設計上の特徴:**
- **型**: generic sealed class `<T>`
- **目的**: 汎用ロギングインターフェース提供（NLog・Serilog等への透過的切り替え）
- **責務**: ILogger を T 型の文脈で LogInformation/LogError/LogWarning を実行
- **実装済みテスト**: FrameworkLoggingAdapterTests.cs（ジェネリック型テスト）

本仕様書は、FrameworkLoggingAdapter のログレベル動作・例外処理・テンプレート構文を確認するテスト仕様。

---

## 1. テスト目的

FrameworkLoggingAdapter<T> が以下を満たすことを確認する：

- **ログ出力**: LogInformation/LogError/LogWarning が正しく ILogger に委譲される
- **テンプレート構文**: メッセージテンプレート+パラメータが正しく構造化される
- **例外処理**: 例外ログが適切に出力される
- **ジェネリック型**: T の型情報がログに含まれる

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | FrameworkLoggingAdapter\<T\> |
| **名前空間** | SupportAdvance.Crosscutting.Logging |
| **ジェネリック** | T: 任意の型（LogContext）|
| **実装** | IFrameworkLogger\<T\> |
| **依存** | Microsoft.Extensions.Logging.ILogger |
| **テストファイル** | tests/Crosscutting.Tests/Logging/FrameworkLoggingAdapterTests.cs |

---

## 3. テスト対象メソッド

| メソッド | シグネチャ | 責務 |
|---------|-----------|------|
| **LogInformation** | `void LogInformation(string message, params object[] args)` | 情報レベルログ出力 |
| **LogError** | `void LogError(string message, Exception? exception = null, params object[] args)` | エラーレベルログ出力 |
| **LogWarning** | `void LogWarning(string message, params object[] args)` | 警告レベルログ出力 |
| **IsEnabled** | `bool IsEnabled(LogLevel level)` | ログレベルが有効か確認 |

---

## 4. テスト観点

### LG: ログ出力（Logging）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| LG-INFO-01 | LogInformation で情報ログが出力される | 正常系 | Test-1 |
| LG-ERR-01 | LogError でエラーログが出力される | 正常系 | Test-2 |
| LG-WARN-01 | LogWarning で警告ログが出力される | 正常系 | Test-3 |

### TP: テンプレート処理（Template）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| TP-01 | メッセージテンプレート+パラメータが正しく処理 | 正常系 | Test-4 |
| TP-02 | 複数パラメータが正しく構造化される | 正常系 | Test-5 |

### EX: 例外処理（Exception）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| EX-01 | LogError で例外が添付される | 正常系 | Test-6 |
| EX-02 | Exception = null でも動作 | 正常系 | Test-7 |

### GN: ジェネリック型（Generic）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| GN-01 | <T> の型情報がログに反映される | 正常系 | Test-8 |

---

## 5. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert、Moq or NSubstitute
- **テスト実装**: `tests/Crosscutting.Tests/Logging/FrameworkLoggingAdapterTests.cs`
- **Mock ILogger**: テストでは Mock<ILogger> を使用
- **ログレベル**: Information/Error/Warning のみテスト対象
- **NullableReferenceTypes**: C# 11 以上での nullable処理

---

## 6. テスト結果統計

**実装済みテスト: 8 件**

| テスト項目 | テスト数 | 観点カバレッジ |
|-----------|---------|------------|
| ログ出力 | 3 | LG-INFO-01, LG-ERR-01, LG-WARN-01 |
| テンプレート | 2 | TP-01, TP-02 |
| 例外処理 | 2 | EX-01, EX-02 |
| ジェネリック型 | 1 | GN-01 |
| **合計** | **8** | **8観点** |

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-5 Crosscutting層テスト文書化） |

