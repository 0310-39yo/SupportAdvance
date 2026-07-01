# Domain層でのログ出力の考え方とベストプラクティス

## 概要

Domain層はビジネスロジックの純粋性と独立性を保つため、**直接ログを出力しません**。その代わり、複数のアーキテクチャパターンを使用してDomain層の重要な事象を記録します。

---

## 🏗️ 推奨アーキテクチャ

### パターン1: デコレーター + Application層で完全ハンドル（**最推奨**）

```
Presentation層
	↓
ErrorHandlingDecorator（ステップ1で実装）
PerformanceDecorator（ステップ2で実装）
LoggingDecorator（ステップ3で実装）
	↓
Application層
├─ UseCase実装
└─ Domain層の呼び出し
	↓
Domain層
└─ ビジネスロジック（ログなし）
```

**特徴:**
- Domain層は**ログを出力しない**
- ログはすべてApplication/Presentation層で集約
- Domain層の関数は**単純で純粋**

**実装例：**

```csharp
// ❌ Domain層: ログを出力しない
public class GetUserCarPreferencesService
{
	public GetUserCarPreferencesResponse Execute(GetUserCarPreferencesRequest request)
	{
		// ビジネスロジックのみ
		var preferences = _repository.GetPreferences(request.UserId);

		if (preferences == null)
			throw new PreferencesNotFoundException($"User {request.UserId} not found");

		return new GetUserCarPreferencesResponse { Preferences = preferences };
	}
}

// ✅ Application層: UseCase + デコレーター
public class GetUserCarPreferencesUseCase : IUseCase<GetUserCarPreferencesRequest, GetUserCarPreferencesResponse>
{
	private readonly GetUserCarPreferencesService _service;

	public async Task<GetUserCarPreferencesResponse> ExecuteAsync(GetUserCarPreferencesRequest request)
	{
		// UseCase層で呼び出し、デコレーターがログを処理
		return await Task.FromResult(_service.Execute(request));
	}
}

// ✅ DI登録（Presentation層）
services.RegisterUseCaseWithDecorators<
	GetUserCarPreferencesRequest,
	GetUserCarPreferencesResponse,
	GetUserCarPreferencesUseCase
>();

// ログ出力はすべてデコレーターで自動処理
```

---

### パターン2: ドメインイベント＆イベントハンドラー

Domain層で重要な事象が発生した場合、ドメインイベントを発行し、Application層がリッスンしてログを記録。

**構造:**

```
Domain層
  ├─ Entity（ドメインロジック）
  ├─ Event（PreferencesChangedEvent など）
  └─ AggregateRoot
		└─ RaisEvent(PreferencesChangedEvent)

Application層
  ├─ EventHandler
  │   └─ ログ出力、他のアクション
  └─ EventPublisher
```

**実装例：**

```csharp
// ✅ Domain層: イベント定義（ログなし）
public class PreferencesChangedEvent : IDomainEvent
{
	public string UserId { get; set; }
	public DateTime ChangedAt { get; set; }
	public string ChangeDescription { get; set; }
}

// ✅ Domain層: Entityでイベント発行
public class UserPreferences : AggregateRoot
{
	public void UpdatePreferences(CarModel model)
	{
		// ビジネスロジック
		this.PreferredModel = model;

		// イベント発行（ログなし）
		RaiseEvent(new PreferencesChangedEvent 
		{ 
			UserId = this.UserId,
			ChangeDescription = $"Updated to {model.Name}",
			ChangedAt = DateTime.UtcNow
		});
	}
}

// ✅ Application層: イベントハンドラー
public class PreferencesChangedEventHandler : IEventHandler<PreferencesChangedEvent>
{
	private readonly IAppLogging<PreferencesChangedEventHandler> _logger;

	public async Task HandleAsync(PreferencesChangedEvent @event)
	{
		// ここでログ出力
		_logger.LogInformation(
			$"User preferences changed - UserId: {@event.UserId}, " +
			$"Description: {@event.ChangeDescription}, " +
			$"Timestamp: {@event.ChangedAt}");

		// 必要に応じて他の処理（メール送信など）
		await SendNotificationAsync(@event);
	}
}
```

**メリット:**
- Domain層は完全に独立（テスト容易）
- イベント駆動で疎結合
- 複数のハンドラーを容易に追加可能

---

### パターン3: 統計情報・メトリクスの記録

ドメインサービスが計算結果や統計情報を返し、Application層が記録。

**実装例：**

```csharp
// ✅ Domain層: 統計情報を返す（ログなし）
public class CarPreferencesStatisticsService
{
	public CarPreferencesStatistics GetStatistics(List<UserPreferences> preferences)
	{
		var stats = new CarPreferencesStatistics
		{
			TotalUsers = preferences.Count,
			PopularModels = preferences
				.GroupBy(p => p.PreferredModel)
				.Select(g => new { Model = g.Key, Count = g.Count() })
				.OrderByDescending(x => x.Count)
				.ToList(),
			RecordedAt = DateTime.UtcNow
		};

		return stats;
	}
}

// ✅ Application層: UseCase
public class GetCarPreferencesStatisticsUseCase 
	: IUseCase<GetStatisticsRequest, GetStatisticsResponse>
{
	private readonly CarPreferencesStatisticsService _service;
	private readonly IAppLogging<GetCarPreferencesStatisticsUseCase> _logger;

	public async Task<GetStatisticsResponse> ExecuteAsync(GetStatisticsRequest request)
	{
		var stats = _service.GetStatistics(request.PreferencesList);

		// Application層でログ出力
		_logger.LogInformation(
			$"Statistics calculated - Total Users: {stats.TotalUsers}, " +
			$"Popular Models: {string.Join(", ", stats.PopularModels)}");

		return new GetStatisticsResponse { Statistics = stats };
	}
}
```

---

## 📊 層別責務マトリックス

| 責務 | Domain層 | Application層 | Presentation層 |
|-----|---------|--------------|--------------|
| ビジネスロジック実装 | ✅ | - | - |
| ビジネス例外発生 | ✅ | - | - |
| ドメインイベント発行 | ✅ | - | - |
| ログ出力 | ❌ | ✅ | ✅ |
| トランザクション管理 | - | ✅ | - |
| デコレーター処理 | - | - | ✅ |
| HTTP/UI処理 | - | - | ✅ |

---

## 🎯 ベストプラクティス

### ✅ Domain層で許可される

1. **ビジネス例外の発生**
   ```csharp
   throw new InvalidCarModelException("Model not supported");
   ```

2. **ドメインイベントの発行**
   ```csharp
   RaiseEvent(new PreferencesUpdatedEvent(...));
   ```

3. **ドメイン値オブジェクトの作成**
   ```csharp
   var carModel = new CarModel(name, year);
   ```

4. **ビジネスロジックの計算**
   ```csharp
   var discount = CalculateDiscount(userAge, purchaseHistory);
   ```

### ❌ Domain層で禁止

1. **ログ出力**
   ```csharp
   ❌ _logger.LogInformation("User updated");
   ```

2. **データベース直接操作**
   ```csharp
   ❌ _dbContext.SaveChanges();
   ```

3. **外部API呼び出し**
   ```csharp
   ❌ _httpClient.GetAsync(url);
   ```

4. **UI/Presentation処理**
   ```csharp
   ❌ MessageBox.Show("Success");
   ```

---

## 🔄 実装フロー（推奨パターン1）

### シーケンス図

```
ユーザー/クライアント
	↓
[1] Presentation Controller
	↓
[2] ErrorHandlingDecorator（例外キャッチ）
	↓
[3] PerformanceDecorator（時間計測開始）
	↓
[4] LoggingDecorator（ログ開始）
	↓
[5] UseCase.ExecuteAsync()
	│
	├─ Application層の処理
	│   ├─ パラメータ検証
	│   ├─ Domain層呼び出し
	│   │   └─ ビジネスロジック（ログなし）
	│   └─ 結果整形
	│
	└─ 応答
	↓
[4] LoggingDecorator（ログ終了）
	↓
[3] PerformanceDecorator（時間計測終了）
	↓
[2] ErrorHandlingDecorator（例外処理）
	↓
ユーザー/クライアント
```

---

## 📝 実装チェックリスト

### Domain層の設計

- [ ] ビジネスロジックのみに専念
- [ ] ILogger/IAppLogging参照なし
- [ ] 外部依存性なし（Infrastructureへの参照なし）
- [ ] ビジネス例外のみ発生
- [ ] ドメインイベント駆動設計を検討

### Application層の設計

- [ ] UseCase実装でDomain層を呼び出し
- [ ] 検証ロジックはApplication層で実施
- [ ] トランザクション管理
- [ ] ドメインイベント処理
- [ ] 必要に応じてログ出力

### Presentation層の設計

- [ ] デコレーターによる横断的関心事処理
- [ ] CorrelationIdによるトレース
- [ ] HTTP/UIロジック

---

## 🧪 テスト戦略

### Domain層テスト

```csharp
[Test]
public void UpdatePreferences_WithValidModel_ShouldUpdateAndRaiseEvent()
{
	// Arrange
	var preferences = new UserPreferences { UserId = "user123" };
	var model = new CarModel("Tesla", 2024);

	// Act
	preferences.UpdatePreferences(model);

	// Assert
	Assert.AreEqual("Tesla", preferences.PreferredModel.Name);
	Assert.IsTrue(preferences.DomainEvents.Any(e => e is PreferencesChangedEvent));
}
```

**特徴：**
- ログライブラリ不要
- 高速実行
- フォーカスされたテスト

### Application層テスト

```csharp
[Test]
public async Task ExecuteAsync_WithValidRequest_ShouldReturnPreferences()
{
	// Arrange
	var mockRepository = new Mock<IPreferencesRepository>();
	var mockLogger = new Mock<IAppLogging<GetUserCarPreferencesUseCase>>();
	var useCase = new GetUserCarPreferencesUseCase(mockRepository.Object, mockLogger.Object);

	// Act
	var result = await useCase.ExecuteAsync(new GetUserCarPreferencesRequest { UserId = "user123" });

	// Assert
	Assert.IsNotNull(result);
	mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
}
```

---

## 🎁 まとめ

| 項目 | 推奨アプローチ |
|-----|-------------|
| **ログ出力場所** | Application/Presentation層のデコレーター |
| **Domain層の責務** | ビジネスロジッと純粋性 |
| **クロスカッティングコンサーン** | デコレーターパターン |
| **例外処理** | Domain: ビジネス例外発生、Application: ハンドル＆ログ |
| **イベント駆動** | Domain: イベント発行、Application: リッスン＆ログ |

---

## 参考リンク

- [Domain-Driven Design - Evans](https://domainlanguage.com/ddd/)
- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID原則 - 単一責務の原則](https://en.wikipedia.org/wiki/Single_responsibility_principle)
