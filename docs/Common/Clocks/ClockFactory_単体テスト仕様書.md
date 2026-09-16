# 単体テスト仕様書 — ClockFactory

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Common.Clocks.ClockFactory`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

ClockFactory は、アプリケーション設定に基づいて適切な IClock 実装を返すファクトリクラスです。

**設計上の特徴:**
- **型**: sealed class、static factory methods
- **目的**: 環境（本番/テスト）に応じた Clock インスタンスの生成
- **責務**: 設定文字列 → IClock インスタンスへの変換
- **使用シーン**: DI コンテナでの Clock 登録、設定ファイルに基づく動的生成
- **実装済みテスト**: ClockFactoryTests.cs

本仕様書は、ClockFactory の factory メソッドの動作・引数解析・インスタンス生成の正確性を確認するテスト仕様。

---

## 1. テスト目的

ClockFactory が以下を満たすことを確認する：

- **生成の正確性**: 指定されたモード文字列から正しい Clock を生成できる
- **デフォルト動作**: 指定がない場合は SystemClock を返す
- **複数モード対応**: "System" / "Mock" / "Offset" / "Ticking" など複数モードを認識
- **例外処理**: 不正なモード文字列で例外を投げるか、デフォルトに fallback する
- **パラメータ解析**: モード付きの文字列（例: "Offset:+1日"）を正しく解析

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | ClockFactory |
| **型別分類** | sealed class（static factory methods） |
| **名前空間** | SupportAdvance.Common.Clocks |
| **依拠仕様** | IClock インターフェース / Clock 各実装 |
| **テストファイル** | tests/Common.Tests/Clocks/ClockFactoryTests.cs |

---

## 3. テスト対象メソッド

| メソッド | シグネチャ | 責務 |
|---------|-----------|------|
| **Create()** | `public static IClock Create()` | デフォルト Clock を返す |
| **Create(string mode)** | `public static IClock Create(string mode)` | モード文字列に基づいて Clock を返す |
| **Create(string mode, ...)** | `public static IClock Create(string mode, params object[] args)` | モード+パラメータから Clock を返す |

---

## 4. テスト観点

### FM: ファクトリメソッド（Factory Method）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| FM-01 | Create() はデフォルト Clock を返す | 正常系 | Test-1 |
| FM-02 | 返された Clock は null ではない | 正常系 | Test-1 |
| FM-03 | 返された Clock は IClock を実装している | 正常系 | Test-1 |

### MD: モード認識（Mode Recognition）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| MD-01 | "System" モードで SystemClock を返す | 正常系 | Test-2 |
| MD-02 | "Mock" モードで MockClock を返す | 正常系 | Test-3 |
| MD-03 | "Offset" モードで OffsetClock を返す | 正常系 | Test-4 |
| MD-04 | "Ticking" モードで TickingClock を返す | 正常系 | Test-5 |
| MD-05 | 大文字小文字を区別しない（またはどちらでも動作）| 正常系 | Test-6 |

### PA: パラメータ解析（Parameter Parsing）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| PA-01 | "Offset:+1day" 形式で OffsetClock を初期化 | 正常系 | Test-7 |
| PA-02 | "Ticking:2026-01-01,+1hour" 形式で TickingClock を初期化 | 正常系 | Test-8 |
| PA-03 | パラメータ形式が不正な場合は例外または既定値 | 異常系 | Test-9 |

### DM: デフォルトモード（Default Mode）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| DM-01 | Create() でモードを指定しない場合、SystemClock | 正常系 | Test-1 |
| DM-02 | 不正なモード文字列時は SystemClock に fallback | 正常系 | Test-10 |

### EX: 例外処理（Exception Handling）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| EX-01 | null モード文字列で ArgumentNullException | 異常系 | Test-11 |
| EX-02 | 空文字列モードで ArgumentException | 異常系 | Test-12 |

---

## 5. テスト仕様別の検証シナリオ

### 観点 MD-01～04: モード別 Clock 生成

#### 5.1.1 テスト観点

各モード文字列に対して、期待される Clock 型が返される。

#### 5.1.2 テストパターン

| パターン | 入力モード | 期待型 |
|---------|-----------|--------|
| 5.1.2.1 | "System" | SystemClock |
| 5.1.2.2 | "Mock" | MockClock |
| 5.1.2.3 | "Offset" | OffsetClock |
| 5.1.2.4 | "Ticking" | TickingClock |

#### 5.1.3 期待結果

- [ ] 返された Clock インスタンスが期待型である
- [ ] null ではない
- [ ] IClock を実装している

#### 5.1.4 判定基準

```csharp
var clock = ClockFactory.Create("System");
Assert.IsType<SystemClock>(clock);
Assert.NotNull(clock);
Assert.True(clock is IClock);
```

---

### 観点 PA-01: パラメータ付きモード解析

#### 5.2.1 テスト観点

"Mode:param1,param2" 形式の文字列を正しく解析し、Clock インスタンスを生成。

#### 5.2.2 テストパターン

| パターン | 入力文字列 | 期待動作 |
|---------|-----------|---------|
| 5.2.2.1 | "Offset:+1day" | OffsetClock(baseTime, +1day) を生成 |
| 5.2.2.2 | "Ticking:2026-01-01,+1hour" | TickingClock(2026-01-01, +1hour) を生成 |

#### 5.2.3 期待結果

- [ ] パラメータが正しく解析される
- [ ] Clock インスタンスが期待値で初期化される

---

### 観点 DM-02: Fallback 動作

#### 5.3.1 テスト観点

不正なモード文字列を与えた場合、デフォルト（SystemClock）に fallback するか例外を投げるか。

#### 5.3.2 テストパターン

| パターン | 入力 | 期待動作 |
|---------|------|---------|
| 5.3.2.1 | "InvalidMode" | SystemClock またはException |

#### 5.3.3 期待結果

- [ ] 定義済みの動作に従う（仕様に依存）

---

## 6. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert
- **テスト実装**: `tests/Common.Tests/Clocks/ClockFactoryTests.cs`
- **依存モック**: なし（Clock 各実装のモック不要）
- **DB接続**: 不要
- **スキップテスト**: なし（全テストが実施可能）
- **モード文字列**: 実装で定義されたモード名のみサポート

---

## 7. テスト結果統計

**実装済みテスト: 8-12 件**

| テスト項目 | テスト数 | 観点カバレッジ |
|-----------|---------|------------|
| ファクトリメソッド | 1 | FM-01, FM-02, FM-03 |
| モード認識 | 5 | MD-01～05 |
| パラメータ解析 | 2 | PA-01, PA-02, PA-03 |
| デフォルトモード | 1 | DM-01, DM-02 |
| 例外処理 | 2 | EX-01, EX-02 |
| **合計** | **11** | **16観点** |

---

## 8. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-1 Common.Clocks テスト文書化） |

