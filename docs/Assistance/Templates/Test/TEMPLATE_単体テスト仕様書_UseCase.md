# 単体テスト仕様書テンプレート — UseCase

**プロジェクト:** SupportAdvance  
**テスト対象:** UseCase（ユースケース / Application Service）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-05-22

---

## 0. 本書の位置づけ

本書は、Application層の UseCase クラスが、業務仕様書および設計書で定義された
**ユースケース実行・トランザクション管理・外部依存の制御・結果返却** を満たすことを確認するテスト仕様書。

---

## 1. テスト目的

UseCase の各メンバーが、以下の仕様を満たすことを確認する：

- **ユースケース実行**：入力に対して期待される業務処理が実行される
- **トランザクション管理**：処理の成功/失敗に応じた一貫性が保証される
- **外部依存制御**：Repository・DomainService・ロギング等が正しく呼び出される
- **例外処理**：エラー時の適切なハンドリングと DTO の返却
- **副作用管理**：ログ出力・キャッシュ更新等が適切に実行される

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | [対象UseCase名：例 CreateOrderUseCase] |
| **名前空間** | Advance.Application.UseCases.Orders |
| **入力** | Request DTO（例 CreateOrderRequest） |
| **出力** | Response DTO（例 CreateOrderResponse） |
| **依拠仕様** | UseCase設計書 / 業務仕様書 |
| **前提** | Entity / DomainService テストが完了していること |

---

## 2.X テスト用実装と DI 設定

このテンプレートでは、ユースケースのテストに使用するテスト用実装を明記します。

| 実装名 | 役割 | 対応する観点ID | 説明 |
|--------|------|----------------|------|
| InMemoryRepository | Repository の代替 | UC-EX, UC-DEP, UC-TRN | テストデータの保存・取得 |
| FixedClock | 時刻の制御 | UC-EX, UC-TRN | タイムスタンプの固定化 |
| MemoryLogger | ログ出力の捕捉 | UC-SE-01, UC-DEP-03 | ログ記録内容の検証 |
| DIFixture | DI コンテナの構成 | 全観点 | 本番実装をテスト実装で上書き |
| Mock DomainService | DomainService の代替 | UC-DEP-02 | 依存サービスの制御・検証 |

---

## 3. テスト観点一覧

### 観点グループ EX：ユースケース実行（Happy Path）

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| UC-EX-01 | UseCase メソッドが正常に実行される | 正常系 | § 5.1 | InMemoryRepository, DIFixture |
| UC-EX-02 | 入力値が正しく Entity に変換される | 正常系 | § 5.1 | InMemoryRepository, DIFixture |
| UC-EX-03 | DomainService が正しく呼び出される | 正常系 | § 5.1 | Mock DomainService |
| UC-EX-04 | Entity が Repository に保存される | 正常系 | § 5.1 | InMemoryRepository |
| UC-EX-05 | Response DTO が正しく構築される | 正常系 | § 5.1 | DIFixture |

### 観点グループ VA：入力検証

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| UC-VA-01 | 不正な入力で例外が発生する | 異常系 | § 5.2 | DIFixture |
| UC-VA-02 | 必須フィールド未設定で検証エラー | 異常系 | § 5.2 | DIFixture |
| UC-VA-03 | 形式エラー（メールアドレス等）で検証エラー | 異常系 | § 5.2 | DIFixture |
| UC-VA-04 | 範囲外の値で検証エラー | 異常系 | § 5.2 | DIFixture |

### 観点グループ TRN：トランザクション管理

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| UC-TRN-01 | 成功時に全変更が Commit される | 正常系 | § 5.3 | InMemoryRepository |
| UC-TRN-02 | 途中失敗時に全変更が Rollback される | 例外発生 | § 5.3 | Mock Repository |
| UC-TRN-03 | Repository 呼び出し失敗時にロールバック | 例外発生 | § 5.3 | Mock Repository |
| UC-TRN-04 | Idempotency が保証される（重複実行） | 準正常系 | § 5.3 | InMemoryRepository |

### 観点グループ DEP：依存コンポーネント呼び出し（Mock/Spy検証）

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| UC-DEP-01 | Repository.Add が呼び出される | 依存呼び出し | § 5.4 | InMemoryRepository |
| UC-DEP-02 | DomainService メソッドが呼び出される | 依存呼び出し | § 5.4 | Mock DomainService |
| UC-DEP-03 | Logger が呼び出される | 依存呼び出し | § 5.4 | MemoryLogger |
| UC-DEP-04 | 複数 Repository への操作が同期される | 依存呼び出し | § 5.4 | InMemoryRepository |

### 観点グループ EH：例外処理

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| UC-EH-01 | DomainService 例外がキャッチされる | 例外発生 | § 5.5 | Mock DomainService |
| UC-EH-02 | Repository 例外がキャッチされる | 例外発生 | § 5.5 | Mock Repository |
| UC-EH-03 | 例外時に Error Response が返される | 例外発生 | § 5.5 | DIFixture |
| UC-EH-04 | ロールバック後のログが記録される | 例外発生 | § 5.5 | MemoryLogger |

### 観点グループ SE：副作用テスト

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| UC-SE-01 | ログが記録される | 副作用テスト | § 5.6 | MemoryLogger |
| UC-SE-02 | イベント（EventEmitter等）が発行される | 副作用テスト | § 5.6 | DIFixture |
| UC-SE-03 | キャッシュが更新される | 副作用テスト | § 5.6 | DIFixture |

### 観点グループ ED：エッジケース / 準正常系

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| UC-ED-01 | 最小値での処理（1件） | 境界値テスト | § 5.7 | InMemoryRepository |
| UC-ED-02 | 大量データでの処理（1000件） | 境界値テスト | § 5.7 | InMemoryRepository |
| UC-ED-03 | 並行実行時の一貫性 | 準正常系 | § 5.7 | InMemoryRepository |
| UC-ED-04 | タイムアウト状況での処理 | 準正常系 | § 5.7 | FixedClock |

---

## 4. テスト観点別の検証シナリオ

### 観点 UC-EX-01：UseCase メソッドが正常に実行される

#### 4.1.1 テスト観点

UseCase の Execute メソッドが、有効な Request DTO を受け取り、正常に実行され、Response DTO を返す（正常系）。

#### 4.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.1.2.1 | 正常系 | 標準的な注文作成 |
| 4.1.2.2 | 正常系 | オプション付き注文作成 |

#### 4.1.3 前提条件

- Request DTO が有効である
- 依存する Repository / Service が Mock で実装されている

#### 4.1.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.1.2.1 | CreateOrderRequest { CustomerId: "CUST-001", Items: [...], Amount: 10000 } | 基本的なリクエスト |
| 4.1.2.2 | CreateOrderRequest { CustomerId: "CUST-001", Items: [...], Amount: 10000, Memo: "..." } | オプション付きリクエスト |

#### 4.1.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.1.2.1 | response.IsSuccess == true | 処理成功 |
| 4.1.2.1 | response.OrderId != null | OrderId が返される |
| 4.1.2.2 | response.IsSuccess == true | オプション付きでも成功 |

#### 4.1.6 判定基準

- [ ] UseCase が正常に実行される
- [ ] Response DTO が返される
- [ ] IsSuccess フラグが true である

---

### 観点 UC-VA-01：不正な入力で例外が発生する

#### 4.2.1 テスト観点

UseCase が不正な入力（null、空文字列、範囲外の値）を受け取った場合、ValidationException を発生させる（異常系）。

#### 4.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.2.2.1 | 異常系 | CustomerId が null |
| 4.2.2.2 | 異常系 | Items が空のリスト |
| 4.2.2.3 | 異常系 | Amount が 0 以下 |
| 4.2.2.4 | 異常系 | Request が null |

#### 4.2.3 前提条件

- 検証ルールが実装されている
- 入力値の型チェックが行われている

#### 4.2.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.2.2.1 | CreateOrderRequest { CustomerId: null, ... } | null 検証 |
| 4.2.2.2 | CreateOrderRequest { CustomerId: "CUST-001", Items: [] } | 空リスト検証 |
| 4.2.2.3 | CreateOrderRequest { CustomerId: "CUST-001", Items: [...], Amount: -1 } | 範囲外検証 |
| 4.2.2.4 | null | null リクエスト |

#### 4.2.5 期待結果

| パターン | 期待例外 | メッセージ | 検証項目 |
|---------|--------|---------|---------|
| 4.2.2.1 | ValidationException | "CustomerId is required" | 例外発生 |
| 4.2.2.2 | ValidationException | "Order must have at least one item" | 例外発生 |
| 4.2.2.3 | ValidationException | "Amount must be positive" | 例外発生 |
| 4.2.2.4 | ArgumentNullException | "request cannot be null" | 例外発生 |

#### 4.2.6 判定基準

- [ ] 不正な入力で例外が発生する
- [ ] 例外メッセージが明確である
- [ ] UseCase は実行されていない（ガード句の動作）

---

### 観点 UC-TRN-02：途中失敗時に全変更が Rollback される

#### 4.3.1 テスト観点

UseCase 実行中に例外が発生した場合、部分的な変更がロールバックされ、全体が一貫性を保つ（トランザクション管理・例外発生時の処理）。

#### 4.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.3.2.1 | 例外発生 | DomainService で例外発生 |
| 4.3.2.2 | 例外発生 | Repository.Add で例外発生 |
| 4.3.2.3 | 例外発生 | 複数操作の途中で例外 |

#### 4.3.3 前提条件

- Repository が例外を発生させるよう Mock 設定
- トランザクション管理が実装されている

#### 4.3.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.3.2.1 | CreateOrderRequest { ... }, mockService.Throws<Exception>() | DomainService で例外 |
| 4.3.2.2 | CreateOrderRequest { ... }, mockRepository.Throws<Exception>() | Repository で例外 |
| 4.3.2.3 | CreateOrderRequest { ... }, 複数操作の途中で例外 | 複合操作での例外 |

#### 4.3.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.3.2.1 | 例外発生 | Exception スロー |
| 4.3.2.1 | mockRepository.Add は呼び出されていない | ロールバック |
| 4.3.2.2 | 例外発生 | Exception スロー |
| 4.3.2.2 | State は変更されていない | ロールバック |
| 4.3.2.3 | 例外発生 | Exception スロー |
| 4.3.2.3 | すべての変更がロールバック | 一貫性保証 |

#### 4.3.6 判定基準

- [ ] 例外が正しく伝播される
- [ ] 部分的な変更がロールバックされる
- [ ] 全体の一貫性が保持される

---

### 観点 UC-DEP-01：Repository.Add が呼び出される

#### 4.4.1 テスト観点

UseCase が正常に実行された後、Repository.Add メソッドが期待通りに呼び出される（依存コンポーネント呼び出しテスト・Mock/Spy検証）。

#### 4.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.4.2.1 | 依存呼び出し | Repository.Add が一度呼び出される |
| 4.4.2.2 | 依存呼び出し | 複数エンティティの Add |

#### 4.4.3 前提条件

- Repository が Mock で実装されている
- Verify 機能が利用可能

#### 4.4.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.4.2.1 | CreateOrderRequest { ... } | 標準的なリクエスト |
| 4.4.2.2 | CreateOrderRequest { Items: [...] } | 複数アイテム |

#### 4.4.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.4.2.1 | orderRepository.Add は 1 回呼び出される | メソッド呼び出し確認 |
| 4.4.2.1 | 呼び出しパラメータが有効な Order である | パラメータ確認 |
| 4.4.2.2 | orderRepository.Add は複数回呼び出される | 複数呼び出し |

#### 4.4.6 判定基準

- [ ] Repository メソッドが呼び出されている
- [ ] 呼び出し回数が期待通りである
- [ ] 呼び出しパラメータが正しい

---

### 観点 UC-EH-03：例外時に Error Response が返される

#### 4.5.1 テスト観点

UseCase 実行中に例外が発生した場合、エラー情報を含む Response DTO が返される（例外処理）。

#### 4.5.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.5.2.1 | 例外発生 | ValidationException → Error Response |
| 4.5.2.2 | 例外発生 | BusinessException → Error Response |
| 4.5.2.3 | 例外発生 | 予期しない例外 → Error Response |

#### 4.5.3 前提条件

- エラーハンドリングが実装されている
- Error Response DTO が定義されている

#### 4.5.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.5.2.1 | CreateOrderRequest { CustomerId: null } | バリデーションエラー |
| 4.5.2.2 | CreateOrderRequest { ... }, DomainService throws | ビジネスルール違反 |
| 4.5.2.3 | CreateOrderRequest { ... }, Repository throws | 予期しないエラー |

#### 4.5.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.5.2.1 | response.IsSuccess == false | 失敗フラグ |
| 4.5.2.1 | response.ErrorMessage != null | エラーメッセージ |
| 4.5.2.2 | response.IsSuccess == false | ビジネスエラー |
| 4.5.2.3 | response.IsSuccess == false | システムエラー |

#### 4.5.6 判定基準

- [ ] Response DTO が返される（例外は throw されない）
- [ ] IsSuccess = false である
- [ ] ErrorMessage が設定されている

---

### 観点 UC-SE-01：ログが記録される

#### 4.6.1 テスト観点

UseCase が実行されるたびに、ログが Logger に記録される（副作用テスト）。

#### 4.6.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.6.2.1 | 副作用テスト | 処理開始時に LogInformation |
| 4.6.2.2 | 副作用テスト | 処理完了時に LogInformation |
| 4.6.2.3 | 副作用テスト | エラー時に LogError |

#### 4.6.3 前提条件

- Logger が Mock で実装されている
- ログメッセージが定義されている

#### 4.6.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.6.2.1 | CreateOrderRequest { ... } | 正常系 |
| 4.6.2.2 | CreateOrderRequest { ... } | 正常系 |
| 4.6.2.3 | CreateOrderRequest { ... }, エラー | エラー系 |

#### 4.6.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.6.2.1 | logger.LogInformation("注文作成開始") | ログ記録 |
| 4.6.2.2 | logger.LogInformation("注文作成完了") | ログ記録 |
| 4.6.2.3 | logger.LogError("注文作成失敗:...") | エラーログ |

#### 4.6.6 判定基準

- [ ] Logger メソッドが呼び出されている
- [ ] ログレベルが適切である（Information / Error）
- [ ] ログメッセージが明確である

---

### 観点 UC-ED-02：大量データでの処理（1000件）

#### 4.7.1 テスト観点

UseCase が大量のデータ（例：1000件）を処理する場合、正常に動作し、パフォーマンスが許容範囲内である（エッジケース / 境界値テスト）。

#### 4.7.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.7.2.1 | 境界値テスト | 1000件の OrderItem |
| 4.7.2.2 | 境界値テスト | 10000件の OrderItem |

#### 4.7.3 前提条件

- 大量データが準備可能
- タイムアウト設定が適切

#### 4.7.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.7.2.1 | Items: 1000件 | 境界値 |
| 4.7.2.2 | Items: 10000件 | 大量データ |

#### 4.7.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.7.2.1 | response.IsSuccess == true | 処理成功 |
| 4.7.2.1 | 実行時間 < 5秒 | パフォーマンス |
| 4.7.2.2 | response.IsSuccess == true または TimeoutException | 制限内での実行 |

#### 4.7.6 判定基準

- [ ] 大量データで処理が完了する
- [ ] パフォーマンスが許容範囲内である
- [ ] メモリリークが発生しない

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **ユースケース実行** | 正常系で期待される結果が返されること |
| **入力検証** | 不正な入力が正しく検出されること |
| **トランザクション管理** | 成功/失敗が一貫性を保つこと |
| **依存呼び出し** | 外部依存が正しく呼び出されること |
| **例外処理** | エラーが適切にハンドリングされること |
| **副作用** | ログ等が正しく記録されること |

### 5.2 検証方法

```
【UC-EX 観点の検証】
  var useCase = new CreateOrderUseCase(mockRepo, mockService, mockLogger);
  var request = new CreateOrderRequest { CustomerId: "CUST-001", ... };
  var response = await useCase.Execute(request);
  
  Assert.True(response.IsSuccess);
  Assert.NotNull(response.OrderId);

【UC-VA 観点の検証】
  var useCase = new CreateOrderUseCase(...);
  var request = new CreateOrderRequest { CustomerId: null, ... };
  
  Assert.Throws<ValidationException>(() => 
    useCase.Execute(request).Result);

【UC-TRN 観点の検証】
  var useCase = new CreateOrderUseCase(mockRepoThatThrows, ...);
  Assert.Throws<Exception>(() => useCase.Execute(validRequest).Result);
  
  // ロールバック確認
  mockRepo.Verify(
    x => x.Add(It.IsAny<Order>()), 
    Times.Never);  // Add は呼び出されていない

【UC-DEP 観点の検証】
  var useCase = new CreateOrderUseCase(mockRepo, mockService, mockLogger);
  await useCase.Execute(validRequest);
  
  mockRepo.Verify(
    x => x.Add(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
    Times.Once);

【UC-EH 観点の検証】
  var useCase = new CreateOrderUseCase(mockRepoThatThrows, ...);
  var response = await useCase.Execute(validRequest);
  
  Assert.False(response.IsSuccess);
  Assert.NotNull(response.ErrorMessage);

【UC-SE 観点の検証】
  var useCase = new CreateOrderUseCase(mockRepo, mockService, mockLogger);
  await useCase.Execute(validRequest);
  
  mockLogger.Verify(
    x => x.LogInformation(It.IsAny<string>()),
    Times.AtLeastOnce);
```

---

## 6. テスト用実装の設定

### 6.1 前提環境

| 項目 | 内容 |
|------|------|
| **テスティングフレームワーク** | xUnit / NUnit |
| **Mock フレームワーク** | Moq / NSubstitute |
| **非同期処理** | async/await 対応 |
| **トランザクション** | UnitOfWork パターン |

### 6.2 テストクラス構成

```
tests/Advance.Unit.Tests/
└── UseCases/
    ├── Orders/
    │   ├── CreateOrderUseCaseTests.cs
    │   ├── UpdateOrderUseCaseTests.cs
    │   └── CancelOrderUseCaseTests.cs
    └── Customers/
        └── CreateCustomerUseCaseTests.cs
```

### 6.3 テストデータビルダー推奨

```csharp
// テストデータの生成を簡素化
public class CreateOrderRequestBuilder
{
    private string _customerId = "CUST-001";
    private List<OrderItem> _items = new();
    
    public CreateOrderRequestBuilder WithCustomerId(string id)
    {
        _customerId = id;
        return this;
    }
    
    public CreateOrderRequest Build()
    {
        return new CreateOrderRequest 
        { 
            CustomerId = _customerId, 
            Items = _items 
        };
    }
}
```

---

## 7. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-05-22 | Advance Team | 初版作成 |

---

