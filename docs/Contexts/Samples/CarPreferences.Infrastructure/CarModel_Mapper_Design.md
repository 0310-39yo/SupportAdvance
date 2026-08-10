# CarModel マッパー設計書

**プロジェクト:** SupportAdvance  
**レイヤ:** Infrastructure / Persistence  
**対象:** `CarModel` ValueObject の DB マッピング  
**DB ORM:** RepoDb  
**版:** 1.0 / 2026-07-04

---

## 1. 位置づけ

`CarModel` は Domain 層の ValueObject であり、**DB の素の int? 値と Domain オブジェクトを相互変換する Mapper** が必要。

- **復元時**: DB の `carModelValue` (int?) → `CarModel` インスタンス
- **永続化時**: `CarModel` インスタンス → DB の `carModelValue` (int?)
- **特殊性**: IsSet フラグにより、0 と NULL を意味的に区別

---

## 2. DB スキーマ

### 2.1 テーブル定義例

```sql
CREATE TABLE Drivers (
  Id INT PRIMARY KEY IDENTITY(1,1),
  Name NVARCHAR(100) NOT NULL,
  CarModelValue INT NULL,  -- 0～6 または NULL
  CreatedAt DATETIME2 DEFAULT GETDATE(),
  UpdatedAt DATETIME2 DEFAULT GETDATE(),
  CONSTRAINT CHK_CarModelValue CHECK (CarModelValue IS NULL OR (CarModelValue BETWEEN 0 AND 6))
);
```

### 2.2 カラム仕様

| カラム名 | 型 | NULL | 説明 |
|---------|-----|------|------|
| `Id` | INT | NO | Primary Key |
| `Name` | NVARCHAR(100) | NO | ドライバー名 |
| `CarModelValue` | INT | YES | CarModel 値（0～6）※ NULL = Unset |
| `CreatedAt` | DATETIME2 | NO | 作成日時 |
| `UpdatedAt` | DATETIME2 | NO | 更新日時 |

---

## 3. マッピング変換ロジック

### 3.1 DB読み込み時（復元）

**フロー図**

```
DB (carModelValue)
  │
  ├─ NULL → CarModel.Unset()        (IsSet=false)
  ├─ 0    → CarModel.Unknown        (IsSet=true, Unknown)
  ├─ 1-6  → CarModel.From(value)    (IsSet=true, 各選択肢)
  └─ other → InvalidOperationException
```

**コード例**

```csharp
public static CarModel ToCarModel(int? dbValue)
{
  return dbValue switch
  {
	null => CarModel.Unset(),
	0    => CarModel.Unknown,
	1    => CarModel.Sedan,
	2    => CarModel.SportUtility,
	3    => CarModel.Hatchback,
	4    => CarModel.Coupe,
	5    => CarModel.Minivan,
	6    => CarModel.Other,
	_    => throw new InvalidOperationException(
			  $"Invalid CarModel DB value: {dbValue}. Expected 0-6 or NULL.")
  };
}

// 別パターン：TryFrom() を活用（推奨）
public static bool TryToCarModel(int? dbValue, out CarModel model)
{
  model = null!;

  if (dbValue is null)
  {
	model = CarModel.Unset();
	return true;
  }

  return CarModel.TryFrom(dbValue.Value, out model);
}
```

### 3.2 DB書き込み時（永続化）

**フロー図**

```
CarModel Instance
  │
  ├─ IsSet=false → null               (DB: NULL)
  ├─ IsSet=true, ValueField=0 → 0     (DB: 0 as Unknown)
  └─ IsSet=true, ValueField=1-6 → value
```

**コード例**

```csharp
public static int? ToDbValue(CarModel carModel)
{
  if (carModel is null)
	throw new ArgumentNullException(nameof(carModel));

  if (!carModel.IsSet)
	return null;  // Unset → NULL

  return carModel.ValueField;  // 0～6
}
```

---

## 4. マッパークラス設計

### 4.1 インターフェース定義

```csharp
/// <summary>
/// CarModel と DB 値の相互マッピングを行う
/// </summary>
public interface ICarModelMapper
{
  /// <summary>
  /// DB 値（int?）から CarModel インスタンスに変換
  /// </summary>
  /// <param name="dbValue">DB に保存された値（0-6 または NULL）</param>
  /// <returns>CarModel インスタンス</returns>
  /// <exception cref="InvalidOperationException">無効な DB 値の場合</exception>
  CarModel FromDb(int? dbValue);

  /// <summary>
  /// CarModel インスタンスから DB 値（int?）に変換
  /// </summary>
  /// <param name="carModel">Domain の CarModel インスタンス</param>
  /// <returns>DB に保存する値（0-6 または NULL）</returns>
  int? ToDb(CarModel carModel);

  /// <summary>
  /// DB 値が有効か検証
  /// </summary>
  /// <param name="dbValue">検証対象の値</param>
  /// <returns>true = 有効 (NULL or 0-6), false = 無効</returns>
  bool IsValidDbValue(int? dbValue);
}
```

### 4.2 実装例

```csharp
/// <summary>
/// CarModel マッパー実装
/// </summary>
public class CarModelMapper : ICarModelMapper
{
  private static readonly ILogger<CarModelMapper> _logger = 
	LoggerFactory.Create(b => b.AddConsole()).CreateLogger<CarModelMapper>();

  public CarModel FromDb(int? dbValue)
  {
	if (!IsValidDbValue(dbValue))
	  throw new InvalidOperationException(
		$"Invalid CarModel DB value: {dbValue}. Expected NULL or 0-6.");

	return dbValue switch
	{
	  null => CarModel.Unset(),
	  0    => CarModel.Unknown,
	  1    => CarModel.Sedan,
	  2    => CarModel.SportUtility,
	  3    => CarModel.Hatchback,
	  4    => CarModel.Coupe,
	  5    => CarModel.Minivan,
	  6    => CarModel.Other,
	  _    => throw new InvalidOperationException($"Unexpected value: {dbValue}")
	};
  }

  public int? ToDb(CarModel carModel)
  {
	if (carModel is null)
	  throw new ArgumentNullException(nameof(carModel));

	if (!carModel.IsSet)
	  return null;

	return carModel.ValueField;
  }

  public bool IsValidDbValue(int? dbValue)
  {
	return dbValue is null || (dbValue >= 0 && dbValue <= 6);
  }
}
```

---

## 5. Entity / DTO との連携

### 5.1 DB モデル（DbDriver）

```csharp
/// <summary>
/// RepoDb テーブルマッピング用 DB モデル
/// </summary>
[Map("[dbo].[Drivers]")]
public class DbDriver
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;

  /// <summary>
  /// CarModel の DB 値（素の int?）
  /// </summary>
  public int? CarModelValue { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
```

### 5.2 Domain Entity（Driver）

```csharp
/// <summary>
/// Domain Entity
/// </summary>
public class Driver : AggregateRoot
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;

  /// <summary>
  /// CarModel ValueObject（Domain 層で使用）
  /// </summary>
  public CarModel CarModel { get; set; } = CarModel.Unset();

  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}
```

### 5.3 DTO（DriverReadModel）

```csharp
/// <summary>
/// API / Presentation 層용 DTO
/// </summary>
public class DriverReadModel
{
  public int Id { get; set; }
  public string Name { get; set; } = null!;

  /// <summary>
  /// 個別表現（UI での表示）
  /// </summary>
  public string CarModelDisplayName { get; set; } = null!;

  /// <summary>
  /// 値自体
  /// </summary>
  public int? CarModelValue { get; set; }
}
```

---

## 6. Repository 内での使用

### 6.1 復元時のコード例

```csharp
public async Task<Driver?> GetByIdAsync(int driverId)
{
  using (var connection = new SqlConnection(_connectionString))
  {
	var dbDriver = await connection.QueryAsync<DbDriver>(
	  new { Id = driverId }
	).FirstOrDefaultAsync();

	if (dbDriver is null)
	  return null;

	// CarModel マッピング
	var carModel = _carModelMapper.FromDb(dbDriver.CarModelValue);

	return new Driver
	{
	  Id = dbDriver.Id,
	  Name = dbDriver.Name,
	  CarModel = carModel,  // DB → Domain 変換
	  CreatedAt = dbDriver.CreatedAt,
	  UpdatedAt = dbDriver.UpdatedAt
	};
  }
}
```

### 6.2 永続化時のコード例

```csharp
public async Task UpdateAsync(Driver driver)
{
  if (driver is null)
	throw new ArgumentNullException(nameof(driver));

  var dbDriver = new DbDriver
  {
	Id = driver.Id,
	Name = driver.Name,
	CarModelValue = _carModelMapper.ToDb(driver.CarModel),  // Domain → DB 変換
	CreatedAt = driver.CreatedAt,
	UpdatedAt = _clock.JstNow.Value  // IClock 経由で取得
  };

  using (var connection = new SqlConnection(_connectionString))
  {
	await connection.UpdateAsync(dbDriver);
  }
}
```

---

## 7. テスト観点

### 7.1 マッピング精度テスト

| テストID | 入力 | 期待値 | テストメソッド |
|---------|------|--------|----------|
| M-001 | null | CarModel.Unset() | `FromDb_Null_ReturnsUnset()` |
| M-002 | 0 | CarModel.Unknown | `FromDb_Zero_ReturnsUnknown()` |
| M-003 | 1-6 | CarModel.From(n) | `FromDb_ValidValue_ReturnsBroken()` |
| M-004 | -1, 99 | Exception | `FromDb_InvalidValue_ThrowsException()` |
| M-005 | CarModel.Unset() | null | `ToDb_Unset_ReturnsNull()` |
| M-006 | CarModel.Unknown | 0 | `ToDb_Unknown_ReturnsZero()` |
| M-007 | CarModel.Sedan | 1 | `ToDb_Sedan_ReturnsOne()` |

---

## 8. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------| 
| 1.0 | 2026-07-04 | 加藤 正人 | 初版作成。CarModel マッピング ロジック、マッパークラス設計、Entity/DTO との連携、Repository での使用例を記載 |

