# 結合テスト仕様書テンプレート

**プロジェクト:** SupportAdvance  
**テスト対象:** [対象の層間 / コンポーネント]  
**テストレベル:** 結合テスト  
**版:** 1.0 / [作成日]

---

## 0. 本書の位置づけ

本書は、Advance プロジェクトのクリーンアーキテクチャにおいて、
**層間（外層・内層）の統合が、設計仕様を満たすことを確認するテスト仕様書**である。

テスト対象となる層間は以下の3パターン：
- **Presentation ↔ Application**：UI層とビジネスロジック層の統合
- **Application ↔ Domain**：ビジネスロジック層とドメイン層の統合
- **Application ↔ Infrastructure**：ビジネスロジック層とデータアクセス層の統合

本書に従って作成された結合テスト仕様書は、テストコード実装の拠り所となる。

---

## 1. テスト目的

[対象の層間統合が、設計仕様を満たすことを確認する。例を示す：]

```
例1：Application ↔ Domain 統合
  "CreateOrderUseCase が Order Entity および OrderDomainService と正しく統合し、
   注文作成のビジネスロジックが期待通り実行されることを確認する。"

例2：Application ↔ Infrastructure 統合
  "CreateOrderUseCase が InMemoryOrderRepository / FixedClock と正しく統合し、
   注文データが正しく保存・取得される、かつ時刻が正しく制御されることを確認する。"

例3：Presentation ↔ Application 統合
  "OrderController が CreateOrderUseCase と正しく統合し、
   リクエストが UseCase へ正しく渡され、レスポンスが正しく構築されることを確認する。"

例4：Crosscutting（ロギング）統合
  "LoggingDecorator が UseCase 呼び出し時に正しく動作し、
   ログメッセージが期待通りに記録されることを確認する。"
```

---

## 2. テスト対象の層間・コンポーネント

| 項目 | 内容 |
|------|------|
| **外側（呼び出し元）** | [層・コンポーネント名] |
| **内側（呼び出し先）** | [層・コンポーネント名] |
| **テスト用実装** | InMemoryRepository / FixedClock / MemoryLogger 等の組み合わせ |
| **依拠設計書** | [設計書名・セクション] |

---

## 3. テスト観点一覧

### 観点グループ [グループ名]

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|---------|
| INTG-[外]-[内]-01 | [観点の説明] | [正常系/異常系等] | [参照セクション] | [使用するテスト実装] |
| INTG-[外]-[内]-02 | [観点の説明] | [正常系/異常系等] | [参照セクション] | [使用するテスト実装] |
| INTG-[外]-[内]-03 | [観点の説明] | [正常系/異常系等] | [参照セクション] | [使用するテスト実装] |

---

## 4. 前提条件

### 4.1 テスト環境

| 項目 | 条件 |
|------|------|
| **使用するテスト実装** | InMemoryOrderRepository, FixedClock(2026-05-22 10:00:00 JST), MemoryLogger |
| **DI設定** | DIFixture で本番実装をテスト実装で上書き |
| **初期状態** | [テスト対象のメモリが空である等] |

### 4.2 対応する単体テスト観点

このテストは、以下の単体テスト観点の統合版である。

| 単体テスト観点ID | 説明 |
|----------------|------|
| [VO / UT観点ID] | [説明] |
| [VO / UT観点ID] | [説明] |

---

## 5. テスト観点別の検証シナリオ

### 観点 INTG-[外]-[内]-01：[観点名]

#### 5.1.1 テスト観点

[観点の詳細説明]

```
例：
  "Application層の CreateOrderUseCase が Domain層の OrderDomainService を
   正しく呼び出し、Order Entity が期待通りに生成される。"
```

#### 5.1.2 前提条件

- [テスト用実装の状態]
- [初期データ]

#### 5.1.3 入力条件

| 項目 | 値 |
|------|-----|
| CustomerId | "CUST-001" |
| OrderItems | [商品1(数量2), 商品2(数量1)] |
| 現在時刻（IClock） | 2026-05-22 10:00:00 JST |

#### 5.1.4 期待結果

| 項目 | 期待値 |
|------|--------|
| Order.Id | [新規生成されたOrderId] |
| Order.CustomerId | "CUST-001" |
| Order.Items | [入力と同じ] |
| Order.CreatedAt | 2026-05-22 10:00:00 JST |
| Repository.Add呼び出し回数 | 1回 |

#### 5.1.5 判定基準

- [ ] UseCase が Domain Service を呼び出している
- [ ] Domain Service が Repository.AddAsync を呼び出している
- [ ] Order Entity が期待通りに生成されている
- [ ] 時刻が FixedClock から取得されている

---

### 観点 INTG-[外]-[内]-02：[観点名]

#### 5.2.1 テスト観点

[観点の詳細説明]

#### 5.2.2 前提条件

[前提条件を記載]

#### 5.2.3 入力条件

[入力条件を記載]

#### 5.2.4 期待結果

[期待結果を記載]

#### 5.2.5 判定基準

- [ ] [判定項目1]
- [ ] [判定項目2]

---

## 6. テスト用実装の設定

### 6.1 InMemoryOrderRepository

```
【設定内容】
  初期状態：メモリが空（データなし）
  動作：List<Order> をバッキングストアとして、Add/Get/Update/Delete を実装
  
【検証ポイント】
  ├─ AddAsync が正しく呼び出されたか
  ├─ GetByIdAsync が正しいデータを返すか
  ├─ 重複キーの Add 時に例外が発生するか
```

### 6.2 FixedClock

```
【設定内容】
  時刻：2026-05-22 10:00:00 JST（固定）
  
【検証ポイント】
  └─ 複数回呼び出された場合も同じ時刻を返すか
```

### 6.3 MemoryLogger

```
【設定内容】
  ログ蓄積：メモリ上の List<LogEntry> に蓄積
  
【検証ポイント】
  ├─ LogInformation / LogWarning / LogError が正しく記録されるか
  ├─ ログメッセージが期待通りであるか
  ├─ ログレベルが正しいか
```

### 6.4 DIFixture 設定例

```
【構成】
  - UseCase に対して本番実装を注入
  - Repository に対してテスト用実装（InMemoryOrderRepository）を注入
  - IClock に対してテスト用実装（FixedClock）を注入
  - IAppLogging<T> に対してテスト用実装（MemoryLogger）を注入

【設定手順】
  1. DIFixture を初期化
  2. AddScoped<IOrderRepository, InMemoryOrderRepository>() で上書き
  3. AddSingleton<IClock, FixedClock>() で上書き
  4. AddScoped<IAppLogging<>, MemoryLogger>() で上書き
  5. UseCase を取得してテスト実行
```

---

## 7. 判定基準

### 7.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **層間の呼び出し** | 外側が内側を正しく呼び出しているか |
| **データの受け渡し** | データが正しく変換／受け渡しされているか |
| **状態変化** | テスト用実装の状態が期待通りに変化しているか |
| **ロギング** | MemoryLogger にメッセージが記録されているか |
| **時刻制御** | FixedClock の時刻が使用されているか |
| **例外処理** | 期待される例外が発生しているか |

### 7.2 テスト用実装の検証方法

```
【InMemoryRepository】
  └─ _data（内部状態）を検査
  └─ Add/Get/Update/Delete の呼び出し回数を確認

【FixedClock】
  └─ JstNow プロパティの値が固定値であることを確認

【MemoryLogger】
  └─ _logs リストの内容を検査
  └─ ログメッセージ、レベル、タイムスタンプを確認

【DIFixture】
  └─ GetService<T>() で取得したインスタンスがテスト用実装であることを確認
```

---

## 8. テスト観点の対応関係

### 8.1 参照する単体テスト観点

```
INTG-APP-DOM-01
  ↓ 参照 ↓
  VO-IS-01, VO-EQ-01, VO-HC-01 等

INTG-APP-INFRA-01
  ↓ 参照 ↓
  UT-REPO-01, UT-CLOCK-01 等
```

### 8.2 参照されるシステムテスト観点

```
SYS-ORD-01, SYS-ORD-02
  ↑ 参照 ↑
  INTG-APP-DOM-01, INTG-APP-INFRA-01, INTG-PRES-APP-01
```

---

## 9. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | [作成日] | Advance Team | 初版作成 |

---

## 付録A：テスト用実装の仕様詳細

### A.1 InMemoryOrderRepository の内部動作

```csharp
【メモリ構造】
private readonly List<Order> _data = new();

【AddAsync】
├─ 同じ OrderId が存在する場合は InvalidOperationException
├─ データを List に追加
└─ Task.CompletedTask を返す

【GetByIdAsync】
├─ OrderId に合致するデータを検索
├─ 見つかった場合はそのデータを返す
├─ 見つからない場合は null を返す
└─ 常に Task で返す

【検証ポイント】
└─ テスト後に _data の内容を検査すれば、呼び出し履歴が追跡可能
```

### A.2 FixedClock の設定方法

```
【使用例】
  var clock = new FixedClock(new DateTime(2026, 5, 22, 10, 0, 0));
  
【検証ポイント】
  └─ clock.JstNow を複数回呼び出しても同じ値を返す
```

### A.3 MemoryLogger の検証方法

```
【使用例】
  var logger = new MemoryLogger();
  logger.LogInformation("注文作成開始");
  
【検証ポイント】
  └─ logger.Logs リストに LogEntry(Level=Information, Message="注文作成開始") が含まれている
```

---

## 付録B：よくあるエラーパターンと検証方法

### B.1 エラーパターン：Repository が呼び出されていない

**症状:**
- テスト後に InMemoryOrderRepository._data が空である

**検証方法:**
```
Assert: InMemoryOrderRepository._data.Count == 1
        InMemoryOrderRepository._data[0].Id == 期待されるOrderId
```

### B.2 エラーパターン：時刻が制御されていない

**症状:**
- Order.CreatedAt が実行時刻になっている

**検証方法:**
```
Assert: Order.CreatedAt == FixedClock.JstNow（2026-05-22 10:00:00）
```

### B.3 エラーパターン：ロギングが記録されていない

**症状:**
- MemoryLogger.Logs が空である

**検証方法:**
```
Assert: MemoryLogger.Logs.Count > 0
        MemoryLogger.Logs.Any(l => l.Message.Contains("注文作成開始"))
```

---

