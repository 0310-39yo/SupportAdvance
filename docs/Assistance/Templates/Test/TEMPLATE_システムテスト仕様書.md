# システムテスト仕様書テンプレート

**プロジェクト:** SupportAdvance  
**テスト対象:** [業務シナリオ / エンドツーエンドワークフロー]  
**テストレベル:** システムテスト  
**版:** 1.0 / [作成日]

---

## 0. 本書の位置づけ

本書は、Advance プロジェクトにおいて、
**複数の UseCase にまたがるビジネスシナリオが、業務仕様を満たすことを確認するテスト仕様書**である。

システムテストは以下の特徴を持つ：
- 複数層（Presentation・Application・Domain・Infrastructure）を統合したテスト
- 業務フロー全体の正確性を確認
- 単体テスト・結合テストで検出できない問題（例：タイミング問題、複数リソースの相互作用）を検出

本書に従って作成されたシステムテスト仕様書は、テストコード実装の拠り所となる。

---

## 1. テスト目的

[複数の UseCase にまたがる業務シナリオが、業務要件を満たすことを確認する。例を示す：]

```
例1：業務シナリオテスト
  "注文作成から出荷までのワークフロー全体が、システム全体として
   期待通りに動作し、業務要件を満たすことを確認する。"

例2：ロギング・監査トレール検証
  "注文作成から出荷までのすべてのプロセスが、監査ログに記録され、
   業務イベントの完全な追跡が可能であることを確認する。"

例3：トランザクション整合性検証
  "複数の エンティティ変更を含む注文作成プロセスが、
   コミット／ロールバック時に一貫性を保つことを確認する。"
```

---

## 2. テスト対象のビジネスシナリオ

| 項目 | 内容 |
|------|------|
| **業務領域** | [Order / Customer / Inventory 等] |
| **シナリオ名** | [例：注文作成から出荷まで] |
| **関連する UseCase** | [CreateOrderUseCase, ApproveOrderUseCase, ShipOrderUseCase 等] |
| **複数ステップ** | ステップ1 → ステップ2 → ... → ステップN |
| **依拠業務仕様書** | [業務仕様書名・セクション] |

---

## 3. テスト観点一覧

### 観点グループ [シナリオ名]

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|---------|
| SYS-[領域]-01 | [観点の説明] | [正常系/異常系等] | [参照セクション] | [使用するテスト実装] |
| SYS-[領域]-02 | [観点の説明] | [正常系/異常系等] | [参照セクション] | [使用するテスト実装] |
| SYS-[領域]-03 | [観点の説明] | [正常系/異常系等] | [参照セクション] | [使用するテスト実装] |

---

## 4. 前提条件

### 4.1 テスト環境

| 項目 | 条件 |
|------|------|
| **テスト用DB** | テスト用データベース（本番スキーマと同一） |
| **テスト用実装** | FixedClock(2026-05-22 10:00:00 JST), MemoryLogger |
| **初期データ** | [セットアップスクリプト参照] |
| **外部システム連携** | スタブ/モック（または接続しない） |

### 4.2 参照する結合テスト観点

このテストは、以下の結合テスト観点の統合版である。

| 結合テスト観点ID | 説明 |
|----------------|------|
| INTG-APP-DOM-01 | [説明] |
| INTG-APP-INFRA-01 | [説明] |
| INTG-PRES-APP-01 | [説明] |

---

## 5. ビジネスシナリオ詳細

### 5.1 シナリオの流れ（高レベル）

```
【ステップフロー】

Step 1：顧客情報の確認
  └─ 既存の顧客データが存在するか確認
  └─ 使用 UseCase：GetCustomerUseCase
  └─ 使用 Repository：InMemoryCustomerRepository / テスト用DB

Step 2：注文を作成
  └─ 新しい注文を作成
  └─ 使用 UseCase：CreateOrderUseCase
  └─ 使用 Repository：InMemoryOrderRepository / テスト用DB

Step 3：注文を承認
  └─ 注文の承認プロセスを実行
  └─ 使用 UseCase：ApproveOrderUseCase
  └─ 使用 Repository：InMemoryOrderRepository / テスト用DB

Step 4：出荷を実行
  └─ 注文を出荷
  └─ 使用 UseCase：ShipOrderUseCase
  └─ 使用 Repository：InMemoryOrderRepository / テスト用DB

Step 5：全プロセスのログ検証
  └─ MemoryLogger のログが完全に記録されているか確認
  └─ 監査トレーサビリティの確認
```

---

## 6. テスト観点別の検証シナリオ

### 観点 SYS-[領域]-01：[観点名]

#### 6.1.1 テスト観点

[観点の詳細説明]

```
例：
  "顧客が新しい注文を作成し、それを承認して出荷するワークフロー全体が
   期待通りに実行される。"
```

#### 6.1.2 シナリオの前提条件

| 項目 | 条件 |
|------|------|
| **初期データ** | 顧客ID: CUST-001 が存在 |
| **現在時刻** | 2026-05-22 10:00:00 JST |
| **DB状態** | Orders テーブルが空 |
| **ロギング** | MemoryLogger が初期化されている |

#### 6.1.3 入力条件（シナリオ入力）

| ステップ | 入力 |
|---------|------|
| Step 1：GetCustomer | CustomerId = "CUST-001" |
| Step 2：CreateOrder | CustomerId = "CUST-001", Items = [商品1(数量2), 商品2(数量1)] |
| Step 3：ApproveOrder | OrderId = [Step 2で作成], ApprovalReason = "正常な注文" |
| Step 4：ShipOrder | OrderId = [Step 2で作成], CarrierId = "CARRIER-001" |

#### 6.1.4 期待結果（シナリオ出力）

| ステップ | 期待される結果 |
|---------|---------|
| Step 1 | Customer が取得される、CustomerId = "CUST-001", Name = "Alice" |
| Step 2 | Order が作成される、OrderId が生成される、Status = "Created" |
| Step 3 | Order.Status が "Approved" に変更される、ApprovedAt が記録される |
| Step 4 | Order.Status が "Shipped" に変更される、ShippedAt が記録される |
| Step 5 | ログに全ステップが記録される（計N件のログメッセージ） |

#### 6.1.5 判定基準

- [ ] Step 1 で顧客情報が正しく取得されている
- [ ] Step 2 で新規注文が作成されている（DB に INSERT されている）
- [ ] Step 3 で Order.Status が "Approved" に更新されている（DB に UPDATE されている）
- [ ] Step 4 で Order.Status が "Shipped" に更新されている（DB に UPDATE されている）
- [ ] 全タイムスタンプが FixedClock の時刻で記録されている（2026-05-22 10:00:00 JST）
- [ ] MemoryLogger に以下のログが含まれている：
  - "顧客取得開始" + "顧客取得完了"
  - "注文作成開始" + "注文作成完了"
  - "承認開始" + "承認完了"
  - "出荷開始" + "出荷完了"

---

### 観点 SYS-[領域]-02：[観点名]

#### 6.2.1 テスト観点

[観点の詳細説明]

#### 6.2.2 シナリオの前提条件

[前提条件を記載]

#### 6.2.3 入力条件（シナリオ入力）

[入力条件を記載]

#### 6.2.4 期待結果（シナリオ出力）

[期待結果を記載]

#### 6.2.5 判定基準

- [ ] [判定項目1]
- [ ] [判定項目2]

---

### 観点 SYS-LOGGING-01：監査トレール検証

#### 6.3.1 テスト観点

全ビジネスシナリオを通じて、重要なビジネスイベントが監査ログに記録され、
イベントの完全な追跡が可能であることを確認する。

#### 6.3.2 シナリオの前提条件

| 項目 | 条件 |
|------|------|
| **ロギング** | MemoryLogger が有効（全ログメッセージを蓄積） |
| **対象イベント** | 注文作成、承認、出荷の各ステップ |

#### 6.3.3 入力条件（シナリオ入力）

[前述のシナリオと同一]

#### 6.3.4 期待結果（シナリオ出力）

| イベント | ログレベル | ログメッセージの内容 |
|---------|---------|---------|
| 注文作成開始 | Information | "注文作成開始：CustomerId={CUST-001}" |
| 注文作成完了 | Information | "注文作成完了：OrderId={OrderId}" |
| 承認開始 | Information | "承認開始：OrderId={OrderId}" |
| 承認完了 | Information | "承認完了：OrderId={OrderId}" |
| 出荷開始 | Information | "出荷開始：OrderId={OrderId}" |
| 出荷完了 | Information | "出荷完了：OrderId={OrderId}" |

#### 6.3.5 判定基準

- [ ] MemoryLogger.Logs に全イベントのログが記録されている
- [ ] ログのタイムスタンプがすべて 2026-05-22 10:00:00 JST である
- [ ] ログレベルが期待通りである（Information / Warning / Error）
- [ ] ログメッセージが期待する形式で記録されている
- [ ] ログの順序が業務シナリオの実行順と一致している

---

### 観点 SYS-TXN-01：トランザクション整合性検証

#### 6.4.1 テスト観点

複数のエンティティ変更を含む注文作成プロセスが、データベーストランザクション内で
一貫性を保つことを確認する。

#### 6.4.2 シナリオの前提条件

| 項目 | 条件 |
|------|------|
| **DB トランザクション** | 注文作成が単一トランザクション内で実行される |
| **初期データ** | 顧客とインベントリが存在 |

#### 6.4.3 入力条件（シナリオ入力）

| 項目 | 値 |
|------|-----|
| CustomerId | "CUST-001" |
| OrderItems | [商品1(数量100)（在庫不足を想定）] |

#### 6.4.4 期待結果（シナリオ出力）

| 項目 | 期待値 |
|------|--------|
| 注文作成 | 失敗（在庫不足により例外発生） |
| DB状態 | ロールバック（Orders テーブルに新規レコードなし） |
| Inventory状態 | 変更なし（元の在庫数を保持） |
| ログ | エラーログが記録される（「在庫不足により注文作成失敗」等） |

#### 6.4.5 判定基準

- [ ] 例外が発生している（InvalidOperationException: 在庫不足）
- [ ] Orders テーブルに新規レコードが追加されていない
- [ ] Inventory テーブルが更新されていない
- [ ] MemoryLogger にエラーログが記録されている

---

## 7. テスト用実装の設定

### 7.1 テスト用データベース

```
【構成】
  └─ 本番データベースと同一のスキーマを持つテスト用DB
  
【初期データセット】
  ├─ customers
  │   └─ CUST-001, CUST-002, ...
  ├─ products
  │   └─ PROD-001(在庫: 100), PROD-002(在庫: 50), ...
  ├─ orders（テスト前に空）
  └─ order_items（テスト前に空）

【セットアップ】
  └─ テスト実行前に初期データをロード
  └─ テスト実行後にクリーンアップ
```

### 7.2 FixedClock

```
【設定内容】
  時刻：2026-05-22 10:00:00 JST（固定）
  
【用途】
  └─ すべての UseCase / Domain Service が同じ時刻を取得
```

### 7.3 MemoryLogger

```
【設定内容】
  ログ蓄積：メモリ上の List<LogEntry> に蓄積
  
【検証方法】
  └─ テスト後に MemoryLogger.Logs を検査
  └─ 期待するログメッセージ・ログレベル・順序を確認
```

### 7.4 DIFixture 設定例

```
【構成】
  - UseCase に対して本番実装を注入
  - Repository に対して本番実装（テスト用DB経由）を注入
  - IClock に対してテスト用実装（FixedClock）を注入
  - IAppLogging<T> に対してテスト用実装（MemoryLogger）を注入

【設定手順】
  1. テスト用DB接続文字列で DI を初期化
  2. IClock を FixedClock で上書き
  3. IAppLogging<> を MemoryLogger で上書き
  4. 初期データを挿入
  5. UseCase を取得してシナリオを実行
  6. テスト後に DB をロールバック
```

---

## 8. 判定基準

### 8.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **シナリオ実行成功** | すべてのステップが期待通りに完了しているか |
| **データベース整合性** | DB 内のデータが期待通りに更新されているか |
| **ログ完全性** | 全イベントが監査ログに記録されているか |
| **時刻制御** | すべてのタイムスタンプが FixedClock から取得されているか |
| **トランザクション整合性** | 失敗時にロールバックが正しく機能するか |
| **エラーハンドリング** | 期待される例外が発生して正しく処理されているか |

### 8.2 データベース検証方法

```
【検証内容】
  └─ テスト後に直接 SQL クエリで DB を検査

【例：注文作成シナリオ後】
  SELECT COUNT(*) FROM orders;  -- 期待値：1
  SELECT * FROM orders WHERE order_id = ?;  -- 期待値：[作成された注文データ]
  SELECT * FROM order_items WHERE order_id = ?;  -- 期待値：[作成された注文アイテム]
```

### 8.3 ログ検証方法

```
【検証内容】
  └─ MemoryLogger.Logs を検査

【例】
  var createdLogs = logger.Logs.Where(l => l.Message.Contains("注文作成完了"));
  Assert: createdLogs.Count() > 0
  Assert: createdLogs.First().Timestamp == FixedClock.JstNow
```

---

## 9. テスト観点の対応関係

### 9.1 参照する結合テスト観点

```
SYS-ORD-01
  ↓ 参照 ↓
  INTG-APP-DOM-01, INTG-APP-INFRA-01, INTG-PRES-APP-01

SYS-LOGGING-01
  ↓ 参照 ↓
  INTG-CROSS-LOG-01

SYS-TXN-01
  ↓ 参照 ↓
  INTG-APP-INFRA-01（DB トランザクション処理）
```

---

## 10. テストデータ管理

### 10.1 初期データセットの定義

```
【ファイル：/tests/Advance.System.Tests/TestData/SYS-ORD-01-InitialData.sql】

INSERT INTO customers (customer_id, name, email) VALUES
('CUST-001', 'Alice Johnson', 'alice@example.com'),
('CUST-002', 'Bob Smith', 'bob@example.com');

INSERT INTO products (product_id, name, price, stock) VALUES
('PROD-001', 'Widget A', 100, 100),
('PROD-002', 'Widget B', 200, 50);
```

### 10.2 テストデータクリーンアップ

```
【ファイル：/tests/Advance.System.Tests/TestData/SYS-ORD-01-Cleanup.sql】

DELETE FROM order_items;
DELETE FROM orders;
-- customers, products は保持（初期データ）
```

---

## 11. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | [作成日] | Advance Team | 初版作成 |

---

## 付録A：よくあるエラーパターンと検証方法

### A.1 エラーパターン：シナリオの一部が実行されない

**症状:**
- Step 3 の承認処理が実行されない
- Order.Status が "Created" のまま

**検証方法:**
```
SELECT status FROM orders WHERE order_id = ?;  -- 期待値：'Approved'
Assert: MemoryLogger.Logs.Any(l => l.Message.Contains("承認完了"))
```

### A.2 エラーパターン：時刻が制御されていない

**症状:**
- ApprovedAt が現在時刻になっている

**検証方法:**
```
SELECT approved_at FROM orders WHERE order_id = ?;  
-- 期待値：2026-05-22 10:00:00
```

### A.3 エラーパターン：トランザクションがロールバックされていない

**症状:**
- 失敗時にも Orders テーブルに新規レコードが残っている

**検証方法:**
```
BEGIN TRANSACTION;
-- シナリオ実行（失敗予定）
ROLLBACK;
SELECT COUNT(*) FROM orders;  -- 期待値：0 または 初期データ数
```

---

