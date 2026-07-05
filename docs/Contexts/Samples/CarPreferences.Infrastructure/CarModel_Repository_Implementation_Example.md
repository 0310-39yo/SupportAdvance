# Repository 実装パターン例 - CarModel 対応

**プロジェクト:** SupportAdvance  
**レイヤ:** Infrastructure / Persistence  
**対象:** DriverRepository の RepoDb 実装例  
**DB ORM:** RepoDb  
**版:** 1.0 / 2026-07-04

---

## 1. 概要

本ドキュメントは、**RepoDb を使用した DriverRepository の具体的な実装パターン例**を示す。

- IsSet フラグを含む CarModel の正確なマッピング
- RepoDb の SELECT / INSERT / UPDATE パターン
- エラーハンドリング、ロギング
- トランザクション管理

---

## 2. 実装の事前段取り

### 2.1 必要なファイル・インターフェース

```
src/Infrastructure/Persistence/
├─ Repositories/
│  ├─ IRepository.cs               # 基本インターフェース
│  ├─ DriverRepository.cs          # 実装（本ドキュメント対象）
│
├─ Mappers/
│  ├─ ICarModelMapper.cs           # Mapper インターフェース
│  └─ CarModelMapper.cs            # Mapper 実装
│
└─ Models/
   ├─ DbDriver.cs                  # DB モデル
   └─ (etc.)

src/Contexts/Samples/CarPreferences.Domain/
├─ Entities/
│  └─ Driver.cs                    # Domain Entity
│
└─ ValueObjects/
   └─ CarModel.cs                  # ValueObject
```

### 2.2 RepoDb 初期化

```csharp
// Program.cs or Startup.cs
using RepoDb;
using RepoDb.Enumerations;

// RepoDb 初期化（SQL Server の場合）
SqlServerBootstrap.Initialize();
```

---

## 3. DriverRepository の実装例

### 3.1 クラス定義

```csharp
using RepoDb;
using RepoDb.Enumerations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SupportAdvance.Infrastructure.Persistence.Repositories
{
  /// <summary>
  /// Driver Entity の Repository 実装（RepoDb ベース）
  /// </summary>
  public class DriverRepository : IRepository<Driver, int>
  {
	private readonly string _connectionString;
	private readonly ICarModelMapper _carModelMapper;
	private readonly ILogger<DriverRepository> _logger;

	public DriverRepository(
	  string connectionString,
	  ICarModelMapper carModelMapper,
	  ILogger<DriverRepository> logger)
	{
	  _connectionString = connectionString;
	  _carModelMapper = carModelMapper;
	  _logger = logger;
	}

	/// <summary>
	/// 主キーから Driver を取得
	/// </summary>
	public async Task<Driver?> GetByIdAsync(int id)
	{
	  try
	  {
		using (var connection = new SqlConnection(_connectionString))
		{
		  // RepoDb で DbDriver を検索
		  var dbDriver = await connection.QueryAsync<DbDriver>(
			new { Id = id }
		  );

		  if (!dbDriver.Any())
			return null;

		  // DB モデル → Domain Entity に変換
		  return MapToDomain(dbDriver.First());
		}
	  }
	  catch (Exception ex)
	  {
		_logger.LogError($"GetByIdAsync failed for ID {id}: {ex.Message}");
		throw;
	  }
	}

	/// <summary>
	/// 全 Driver を取得
	/// </summary>
	public async Task<IEnumerable<Driver>> GetAllAsync()
	{
	  try
	  {
		using (var connection = new SqlConnection(_connectionString))
		{
		  // RepoDb で全 DbDriver を検索
		  var dbDrivers = await connection.QueryAllAsync<DbDriver>();

		  return dbDrivers
			.Select(MapToDomain)
			.ToList();
		}
	  }
	  catch (Exception ex)
	  {
		_logger.LogError($"GetAllAsync failed: {ex.Message}");
		throw;
	  }
	}

	/// <summary>
	/// 新規 Driver を追加
	/// </summary>
	public async Task AddAsync(Driver driver)
	{
	  if (driver is null)
		throw new ArgumentNullException(nameof(driver));

	  try
	  {
		var dbDriver = MapToDatabase(driver);

		using (var connection = new SqlConnection(_connectionString))
		{
		  var identity = await connection.InsertAsync<DbDriver>(dbDriver);
		  driver.Id = (int)identity;

		  _logger.LogInformation($"Driver inserted with ID {identity}");
		}
	  }
	  catch (Exception ex)
	  {
		_logger.LogError($"AddAsync failed: {ex.Message}");
		throw;
	  }
	}

	/// <summary>
	/// 既存 Driver を更新
	/// </summary>
	public async Task UpdateAsync(Driver driver)
	{
	  if (driver is null)
		throw new ArgumentNullException(nameof(driver));

	  try
	  {
		var dbDriver = MapToDatabase(driver);

		using (var connection = new SqlConnection(_connectionString))
		{
		  var affectedRows = await connection.UpdateAsync<DbDriver>(dbDriver);

		  if (affectedRows == 0)
			_logger.LogWarning($"No rows updated for ID {driver.Id}");
		  else
			_logger.LogInformation($"Driver updated: ID {driver.Id}");
		}
	  }
	  catch (Exception ex)
	  {
		_logger.LogError($"UpdateAsync failed for ID {driver.Id}: {ex.Message}");
		throw;
	  }
	}

	/// <summary>
	/// Driver を削除
	/// </summary>
	public async Task DeleteAsync(int id)
	{
	  try
	  {
		using (var connection = new SqlConnection(_connectionString))
		{
		  var affectedRows = await connection.DeleteAsync<DbDriver>(
			new { Id = id }
		  );

		  if (affectedRows == 0)
			_logger.LogWarning($"No rows deleted for ID {id}");
		  else
			_logger.LogInformation($"Driver deleted: ID {id}");
		}
	  }
	  catch (Exception ex)
	  {
		_logger.LogError($"DeleteAsync failed for ID {id}: {ex.Message}");
		throw;
	  }
	}

	/// <summary>
	/// トランザクション開始
	/// </summary>
	public async Task<ITransaction> BeginTransactionAsync()
	{
	  var connection = new SqlConnection(_connectionString);
	  await connection.OpenAsync();
	  return connection.BeginTransaction();
	}

	/// <summary>
	/// 複数 Driver を条件付きで取得（拡張メソッド）
	/// </summary>
	public async Task<IEnumerable<Driver>> GetByFilterAsync(
	  System.Linq.Expressions.Expression<Func<DbDriver, bool>> predicate)
	{
	  try
	  {
		using (var connection = new SqlConnection(_connectionString))
		{
		  var dbDrivers = await connection.QueryAsync<DbDriver>(predicate);
		  return dbDrivers.Select(MapToDomain).ToList();
		}
	  }
	  catch (Exception ex)
	  {
		_logger.LogError($"GetByFilterAsync failed: {ex.Message}");
		throw;
	  }
	}

	/// <summary>
	/// DB モデル → Domain Entity 変換（* 重要 *）
	/// </summary>
	private Driver MapToDomain(DbDriver dbDriver)
	{
	  if (dbDriver is null)
		throw new ArgumentNullException(nameof(dbDriver));

	  return new Driver
	  {
		Id = dbDriver.Id,
		Name = dbDriver.Name,
		CarModel = _carModelMapper.FromDb(dbDriver.CarModelValue),  // ★ IsSet 区別
		CreatedAt = dbDriver.CreatedAt,
		UpdatedAt = dbDriver.UpdatedAt
	  };
	}

	/// <summary>
	/// Domain Entity → DB モデル 変換（* 重要 *）
	/// </summary>
	private DbDriver MapToDatabase(Driver driver)
	{
	  if (driver is null)
		throw new ArgumentNullException(nameof(driver));

	  return new DbDriver
	  {
		Id = driver.Id,
		Name = driver.Name,
		CarModelValue = _carModelMapper.ToDb(driver.CarModel),  // ★ IsSet 区別
		CreatedAt = driver.CreatedAt,
		UpdatedAt = DateTime.UtcNow
	  };
	}
  }
}
```

---

## 4. RepoDb クエリの具体例

### 4.1 単一行取得（Query）

```csharp
// SQL 相当: SELECT * FROM Drivers WHERE Id = 1
var dbDriver = await connection.QueryAsync<DbDriver>(
  new { Id = 1 }
);
var driver = dbDriver.FirstOrDefault();

// 結果例
// DbDriver { Id=1, Name="Taro", CarModelValue=1, CreatedAt=..., UpdatedAt=... }
```

### 4.2 複数行取得（QueryAll）

```csharp
// SQL 相当: SELECT * FROM Drivers
var dbDrivers = await connection.QueryAllAsync<DbDriver>();

// 結果例
// [
//   DbDriver { Id=1, Name="Taro", CarModelValue=1, ... },
//   DbDriver { Id=2, Name="Hanako", CarModelValue=null, ... }
// ]
```

### 4.3 複数行取得（条件付き Query）

```csharp
// SQL 相当: SELECT * FROM Drivers WHERE IsActive = 1
var dbDrivers = await connection.QueryAsync<DbDriver>(
  new { IsActive = true }
);
```

### 4.4 新規行挿入（Insert）

```csharp
var dbDriver = new DbDriver
{
  Name = "Taro Tanaka",
  CarModelValue = 2,  // SUV（Unset なら null）
  CreatedAt = DateTime.UtcNow,
  UpdatedAt = DateTime.UtcNow
};

var identity = await connection.InsertAsync<DbDriver>(dbDriver);
// 結果: identity = 3 (新しい主キー)
```

### 4.5 行更新（Update）

```csharp
var dbDriver = new DbDriver
{
  Id = 1,
  Name = "Taro Tanaka",
  CarModelValue = null,  // Unset に変更
  UpdatedAt = DateTime.UtcNow
};

var affectedRows = await connection.UpdateAsync<DbDriver>(dbDriver);
// 結果: affectedRows = 1（1行更新）
```

### 4.6 行削除（Delete）

```csharp
var affectedRows = await connection.DeleteAsync<DbDriver>(
  new { Id = 1 }
);
// 結果: affectedRows = 1（1行削除）
```

---

## 5. CarModel マッピングの実行フロー

### 5.1 取得時のフロー

```
DB: CarModelValue = 0
  ↓
DbDriver.CarModelValue = 0
  ↓
ICarModelMapper.FromDb(0)
  ↓
CarModel.Unknown (IsSet=true, ValueField=0)
  ↓
Driver.CarModel = CarModel.Unknown
```

### 5.2 保存時のフロー

```
Driver.CarModel = CarModel.Unset()
  ↓  
ICarModelMapper.ToDb(CarModel.Unset())
  ↓
null
  ↓
DbDriver.CarModelValue = null
  ↓
INSERT/UPDATE で DB に NULL 保存
```

---

## 6. エラーハンドリング例

### 6.1 無効な DB 値の検出

```csharp
// DB に 99 が入っていた場合（不整合）
var dbDriver = new DbDriver { CarModelValue = 99 };

try
{
  // FromDb() で InvalidOperationException が発生
  var carModel = _carModelMapper.FromDb(dbDriver.CarModelValue);
}
catch (InvalidOperationException ex)
{
  _logger.LogError($"Invalid CarModel value in DB: {ex.Message}");
  // 代替処理
  // Option 1: Unset() として扱う
  dbDriver.CarModelValue = null;
  // Option 2: 例外をスロー
  throw;
}
```

### 6.2 トランザクション例

```csharp
try
{
  using (var connection = new SqlConnection(_connectionString))
  {
	using (var transaction = connection.BeginTransaction())
	{
	  // 複数の DML 操作
	  await connection.InsertAsync<DbDriver>(dbDriver1);
	  await connection.UpdateAsync<DbDriver>(dbDriver2);
	  // ...

	  // すべて成功したらコミット
	  transaction.Commit();
	  _logger.LogInformation("Transaction committed successfully");
	}
  }
}
catch (Exception ex)
{
  _logger.LogError($"Transaction failed: {ex.Message}");
  // トランザクション自動ロールバック
  throw;
}
```

---

## 7. 依存注入設定

### 7.1 Composition Root（例：Program.cs）

```csharp
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Infrastructure.Persistence;

public class Program
{
  public static void Main(string[] args)
  {
	var services = new ServiceCollection();

	// Repository, Mapper, Logger を登録
	services.AddScoped<ICarModelMapper, CarModelMapper>();
	services.AddScoped(provider =>
	{
	  var logger = provider.GetRequiredService<ILogger<DriverRepository>>();
	  var mapper = provider.GetRequiredService<ICarModelMapper>();
	  return new DriverRepository(
		"Server=localhost;Database=SupportAdvance;Trusted_Connection=true;",
		mapper,
		logger
	  );
	});

	// ロギング
	services.AddLogging(config =>
	{
	  config.AddConsole();
	  config.SetMinimumLevel(LogLevel.Information);
	});

	var serviceProvider = services.BuildServiceProvider();

	// 使用例
	var repository = serviceProvider.GetRequiredService<DriverRepository>();
	var driver = await repository.GetByIdAsync(1);
  }
}
```

---

## 8. テスト実装例

### 8.1 単体テスト（Mapper）

```csharp
using Xunit;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain;
using SupportAdvance.Infrastructure.Persistence;

public class CarModelMapperTests
{
  private readonly ICarModelMapper _mapper = new CarModelMapper();

  [Fact]
  public void FromDb_Null_ReturnsUnset()
  {
	var result = _mapper.FromDb(null);
	Assert.False(result.IsSet);
	Assert.Equal(CarModel.Unset(), result);
  }

  [Fact]
  public void FromDb_Zero_ReturnsUnknown()
  {
	var result = _mapper.FromDb(0);
	Assert.Equal(CarModel.Unknown, result);
  }

  [Theory]
  [InlineData(1, 1)]
  [InlineData(6, 6)]
  public void FromDb_ValidValue_ReturnsFromValue(int value, int expected)
  {
	var result = _mapper.FromDb(value);
	Assert.True(result.IsSet);
  }

  [Fact]
  public void ToDb_Unset_ReturnsNull()
  {
	var result = _mapper.ToDb(CarModel.Unset());
	Assert.Null(result);
  }

  [Fact]
  public void ToDb_Unknown_ReturnsZero()
  {
	var result = _mapper.ToDb(CarModel.Unknown);
	Assert.Equal(0, result);
  }
}
```

### 8.2 統合テスト（Repository + Mapper）

```csharp
[Fact]
public async Task AddAsync_InsertDriver_ShouldReturnWithId()
{
  var driver = new Driver
  {
	Name = "Test Driver",
	CarModel = CarModel.Sedan
  };

  await _repository.AddAsync(driver);

  Assert.True(driver.Id > 0);
}

[Fact]
public async Task GetByIdAsync_RetrieveInsertedDriver_ShouldMapCorrectly()
{
  var driverId = 1;
  var driver = await _repository.GetByIdAsync(driverId);

  Assert.NotNull(driver);
  Assert.Equal(driverId, driver.Id);
  Assert.True(driver.CarModel.IsSet);
}
```

---

## 9. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------| 
| 1.0 | 2026-07-04 | 加藤 正人 | 初版作成。DriverRepository の RepoDb 実装、マッピング、エラーハンドリング、テスト例を記載 |

