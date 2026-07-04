# 単体テスト仕様書テンプレート — DomainService

**プロジェクト:** SupportAdvance  
**テスト対象:** DomainService（ドメインサービス）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-05-22

---

## 0. 本書の位置づけ

本書は、Domain層の DomainService クラスが、設計書で定義された
**複数エンティティ間のビジネスロジック・ドメインルール適用・複合検証** を満たすことを確認するテスト仕様書。

---

## 1. テスト目的

DomainService の各メンバーが、以下の仕様を満たすことを確認する：

- **ビジネスロジック**：複数エンティティを操作する複合ロジックが正確に実行される
- **ドメインルール適用**：ルール違反を正しく検出し、例外で拒否する
- **複合検証**：複数の条件を組み合わせた検証が機能する
- **依存コンポーネント呼び出し**：Domain Services / Repositories の呼び出しが正しい
- **副作用の管理**：状態変更が正しくトランザクション的に処理される

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | [対象DomainService名：例 OrderDomainService] |
| **名前空間** | Advance.Domain.Services |
| **依拠仕様** | DomainService設計書 / ドメイン仕様書 |
| **前提** | Entity / ValueObject テストが完了していること |

---

## 2.X テスト用実装と DI 設定

このテンプレートでは、ドメインサービスのテストに使用するテスト用実装を明記します。

| 実装名 | 役割 | 対応する観点ID | 説明 |
|--------|------|----------------|------|
| Mock Repository | 依存 Repository の模擬 | DS-DP-01～04 | Repository 呼び出しの制御・検証 |
| Mock DomainService | 他の DomainService の模擬 | DS-DP-03 | 他サービス呼び出しの制御・検証 |
| Test Entity Factory | テスト用 Entity 生成 | DS-BL, DS-VD | テストケースの前提データ生成 |
| State Validator | 副作用検証用 | DS-SE-01～03 | 状態変更の順序・一貫性検証 |

---

## 3. テスト観点一覧

### 観点グループ BL：ビジネスロジック実行

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| DS-BL-01 | サービスメソッドが正常系で実行される | 正常系 | § 4.1 | Test Entity Factory |
| DS-BL-02 | 複数エンティティへの変更が反映される | 正常系 | § 4.1 | Test Entity Factory |
| DS-BL-03 | 複合ロジック（A かつ B ならば C）が機能する | 正常系 | § 4.1 | Test Entity Factory |

### 観点グループ VD：バリデーション / ドメインルール検証

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| DS-VD-01 | ルール違反時に例外が発生する | 例外発生 | § 4.2 | Test Entity Factory |
| DS-VD-02 | 複数ルールの組み合わせが検証される | 正常系 | § 4.2 | Test Entity Factory |
| DS-VD-03 | 境界値でのルール検証が正しい | 境界値テスト | § 4.2 | Test Entity Factory |
| DS-VD-04 | エラーメッセージが明確である | 例外発生 | § 4.2 | Test Entity Factory |

### 観点グループ DP：依存コンポーネント（Repository / Service）呼び出し

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| DS-DP-01 | Repository が正しく呼び出される | 依存呼び出し | § 4.3 | Mock Repository |
| DS-DP-02 | 複数 Repository への操作が同期される | 依存呼び出し | § 4.3 | Mock Repository |
| DS-DP-03 | 他の DomainService が呼び出される | 依存呼び出し | § 4.3 | Mock DomainService |
| DS-DP-04 | Repository から戻ったエンティティが正しく処理される | 依存呼び出し | § 4.3 | Mock Repository |

### 観点グループ SE：副作用テスト（状態変更の順序・一貫性）

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| DS-SE-01 | 状態変更が正しい順序で実行される | 副作用テスト | § 4.4 | State Validator |
| DS-SE-02 | 中途失敗時にロールバックされる | 例外発生 | § 4.4 | State Validator |
| DS-SE-03 | 複数エンティティの変更が一貫性を保つ | 副作用テスト | § 4.4 | State Validator |

### 観点グループ EH：例外処理

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| DS-EH-01 | 依存コンポーネントの例外が正しく伝播される | 例外発生 | § 4.5 | Mock Repository |
| DS-EH-02 | 例外時に適切なロールバック処理が実行される | 例外発生 | § 4.5 | State Validator |
| DS-EH-03 | 例外メッセージが明確である | 例外発生 | § 4.5 | Test Entity Factory |

---

## 4. テスト観点別の検証シナリオ

### 観点 DS-BL-01：サービスメソッドが正常系で実行される

#### 4.1.1 テスト観点

DomainService のメソッドが、正常なエンティティを入力に、正しく実行され、期待される結果を返す（正常系）。

#### 4.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.1.2.1 | 正常系 | 注文承認サービスの実行 |
| 4.1.2.2 | 正常系 | 複数顧客を扱う処理 |

#### 4.1.3 前提条件

- 有効な Entity / ValueObject が準備されている
- 依存する Repository は Mock で代替

#### 4.1.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.1.2.1 | order: Order (Created → Approved), approver: Staff | 注文承認 |
| 4.1.2.2 | customers: List<Customer>, criteria: ... | 複数顧客処理 |

#### 4.1.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.1.2.1 | result.IsSuccess == true | メソッドが成功 |
| 4.1.2.1 | order.Status == Approved | エンティティが変更 |
| 4.1.2.2 | result.Count == 複数 | 複数エンティティ処理 |

#### 4.1.6 判定基準

- [ ] メソッドが正常に実行される
- [ ] 戻り値が期待通りである
- [ ] 入力エンティティが正しく変更される

---

### 観点 DS-BL-02：複数エンティティへの変更が反映される

#### 4.2.1 テスト観点

DomainService が複数のエンティティを操作する場合、すべてのエンティティが正しく変更される（正常系）。

#### 4.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.2.2.1 | 正常系 | Order と OrderItem を同時に変更 |
| 4.2.2.2 | 正常系 | Customer と Order を関連付け |

#### 4.2.3 前提条件

- 複数のエンティティが関連付けられている
- 変更順序が定義されている

#### 4.2.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.2.2.1 | order, items: [...], priceUpdate: true | 注文と明細行の更新 |
| 4.2.2.2 | customer, order | 顧客と注文の関連付け |

#### 4.2.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.2.2.1 | order.TotalPrice == 更新後価格 | Order 変更 |
| 4.2.2.1 | items[0].Price == 新価格 | OrderItem 変更 |
| 4.2.2.2 | order.CustomerId == customer.Id | 関連付けされている |

#### 4.2.6 判定基準

- [ ] すべてのエンティティが変更される
- [ ] 変更の一貫性が保たれている
- [ ] 部分的な変更は発生しない

---

### 観点 DS-VD-01：ルール違反時に例外が発生する

#### 4.3.1 テスト観点

DomainService が入力エンティティのルール違反を検出した場合、例外を発生させて処理を中止する（例外発生テスト）。

#### 4.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.3.2.1 | 例外発生 | 注文額が 0 円 |
| 4.3.2.2 | 例外発生 | 不正な状態での操作 |
| 4.3.2.3 | 例外発生 | 顧客が存在しない |

#### 4.3.3 前提条件

- ドメインルールが定義されている
- バリデーションメソッドが実装されている

#### 4.3.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.3.2.1 | order.TotalAmount = 0 | ルール違反 |
| 4.3.2.2 | order.Status = Cancelled (操作試行) | 不正な状態 |
| 4.3.2.3 | order.CustomerId = null | 存在しない参照 |

#### 4.3.5 期待結果

| パターン | 期待例外 | メッセージ | 検証項目 |
|---------|--------|---------|---------|
| 4.3.2.1 | ArgumentException | "Order amount must be positive" | 例外発生 |
| 4.3.2.2 | InvalidOperationException | "Cannot operate on Cancelled order" | 例外発生 |
| 4.3.2.3 | ArgumentException | "Customer not found" | 例外発生 |

#### 4.3.6 判定基準

- [ ] ルール違反で例外が発生する
- [ ] 例外の型が適切である
- [ ] エラーメッセージが明確である
- [ ] エンティティが変更されない（トランザクション性）

---

### 観点 DS-VD-03：境界値でのルール検証が正しい

#### 4.4.1 テスト観点

DomainService のルール検証が、境界値（例：最小金額 1 円、最大金額 999,999 円）で正しく機能する（境界値テスト）。

#### 4.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.4.2.1 | 境界値 | 最小値 - 1（失敗） |
| 4.4.2.2 | 境界値 | 最小値（成功） |
| 4.4.2.3 | 境界値 | 最大値（成功） |
| 4.4.2.4 | 境界値 | 最大値 + 1（失敗） |

#### 4.4.3 前提条件

- ドメインルール：注文額は 1 円 ～ 999,999 円
- Repository が Mock で代替されている

#### 4.4.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.4.2.1 | order.TotalAmount = 0 | 最小値未満 |
| 4.4.2.2 | order.TotalAmount = 1 | 最小値 |
| 4.4.2.3 | order.TotalAmount = 999999 | 最大値 |
| 4.4.2.4 | order.TotalAmount = 1000000 | 最大値超過 |

#### 4.4.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.4.2.1 | ArgumentException | 例外発生 |
| 4.4.2.2 | result.IsSuccess = true | 成功 |
| 4.4.2.3 | result.IsSuccess = true | 成功 |
| 4.4.2.4 | ArgumentException | 例外発生 |

#### 4.4.6 判定基準

- [ ] 境界値の手前で例外が発生する
- [ ] 境界値そのものは成功する
- [ ] 一貫した判定がなされている

---

### 観点 DS-DP-01：Repository が正しく呼び出される

#### 4.5.1 テスト観点

DomainService が Repository のメソッドを正しく呼び出す（依存コンポーネント呼び出しテスト・Mock/Spy 検証）。

#### 4.5.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.5.2.1 | 依存呼び出し | Repository.GetByIdAsync が呼び出される |
| 4.5.2.2 | 依存呼び出し | Repository.UpdateAsync が呼び出される |
| 4.5.2.3 | 依存呼び出し | 複数 Repository への呼び出し |

#### 4.5.3 前提条件

- Repository が Mock で実装されている
- Spy / Mock の検証が可能

#### 4.5.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.5.2.1 | orderId: "ORD-001" | GetByIdAsync("ORD-001") 呼び出し |
| 4.5.2.2 | order: Order (変更) | UpdateAsync(order) 呼び出し |
| 4.5.2.3 | order, customer | GetByIdAsync × 2 呼び出し |

#### 4.5.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.5.2.1 | orderRepository.GetByIdAsync("ORD-001") 呼び出し | メソッド呼び出し確認 |
| 4.5.2.1 | 呼び出し回数 == 1 | 一度だけ呼び出される |
| 4.5.2.2 | orderRepository.UpdateAsync(order) 呼び出し | メソッド呼び出し確認 |
| 4.5.2.3 | 2つの Repository への呼び出しが実行される | 複数呼び出し確認 |

#### 4.5.6 判定基準

- [ ] Repository メソッドが呼び出されている
- [ ] 呼び出し回数が期待通りである
- [ ] 呼び出しパラメータが正しい
- [ ] 呼び出し順序が正しい

---

### 観点 DS-SE-02：中途失敗時にロールバックされる

#### 4.6.1 テスト観点

DomainService 実行途中で例外が発生した場合、部分的な変更がロールバックされ、全体が一貫性を保つ（副作用テスト・例外発生時の処理）。

#### 4.6.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.6.2.1 | 例外発生 | Order 変更後、OrderItem 変更時に例外 |
| 4.6.2.2 | 例外発生 | Repository.UpdateAsync 失敗時 |

#### 4.6.3 前提条件

- 複数の操作が実行されるシナリオ
- Repository が例外を発生させるよう Mock 設定

#### 4.6.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.6.2.1 | order, items | Order 変更 → OrderItem 変更時に例外 |
| 4.6.2.2 | order | Repository.UpdateAsync で例外 |

#### 4.6.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.6.2.1 | 例外発生 | Exception スロー |
| 4.6.2.1 | order は変更前の状態 | ロールバック確認 |
| 4.6.2.1 | items は変更前の状態 | ロールバック確認 |
| 4.6.2.2 | 例外発生 | Exception スロー |
| 4.6.2.2 | Repository.UpdateAsync は呼び出されていない | ロールバック確認 |

#### 4.6.6 判定基準

- [ ] 例外が正しく伝播される
- [ ] 部分的な変更がロールバックされる
- [ ] 全体の一貫性が保持される

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **ビジネスロジック** | 複合ロジックが正しく実行されること |
| **ルール検証** | 違反が正しく検出されること |
| **依存呼び出し** | Repository / 他の Service が正しく呼び出されること |
| **副作用管理** | 状態変更が一貫性を保つこと |
| **例外処理** | ロールバック等が正しく実行されること |

### 5.2 検証方法

```
【DS-BL 観点の検証】
  var service = new OrderDomainService(mockRepo);
  var result = service.ApproveOrder(order, approver);
  Assert.True(result.IsSuccess);
  Assert.Equal(OrderStatus.Approved, order.Status);

【DS-VD 観点の検証】
  var service = new OrderDomainService(mockRepo);
  Assert.Throws<ArgumentException>(() =>
    service.ApproveOrder(invalidOrder, approver));

【DS-DP 観点の検証】
  var mockRepo = new Mock<IOrderRepository>();
  var service = new OrderDomainService(mockRepo.Object);
  service.ApproveOrder(order, approver);
  
  mockRepo.Verify(
    x => x.UpdateAsync(order, It.IsAny<CancellationToken>()),
    Times.Once);  // 一度だけ呼び出される

【DS-SE 観点の検証】
  var service = new OrderDomainService(mockRepoThatThrows);
  Assert.Throws<Exception>(() => service.ComplexOperation(...));
  // order は変更されていない（ロールバック確認）
  Assert.Equal(OrderStatus.Created, order.Status);
```

---

## 6. テスト用実装の設定

### 6.1 前提環境

| 項目 | 内容 |
|------|------|
| **テスティングフレームワーク** | xUnit / NUnit |
| **Mock フレームワーク** | Moq / NSubstitute |
| **検証方法** | Verify / Mock 設定 |

### 6.2 テストクラス構成

```
tests/Advance.Unit.Tests/
└── DomainServices/
    ├── OrderDomainServiceTests.cs
    ├── CustomerDomainServiceTests.cs
    └── InventoryDomainServiceTests.cs
```

---

## 7. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-05-22 | Advance Team | 初版作成 |

---

