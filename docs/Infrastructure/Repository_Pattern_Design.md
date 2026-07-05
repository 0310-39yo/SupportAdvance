# Repository パターン設計書 - 共通

**プロジェクト:** SupportAdvance  
**レイヤ:** Infrastructure  
**対象:** ValueObject 型の永続化・復元パターン  
**DB ORM:** RepoDb  
**版:** 1.0 / 2026-07-04

---

## 1. 位置づけ

SupportAdvance における **Repository パターン**は、以下の責務を持つ。

- **DB 操作の抽象化**: Application / Domain 層から SQL 詳細を隠蔽
- **ValueObject との型安全なマッピング**: DB の素の int/string 値と Domain ValueObject を相互変換
- **IsSet フラグ永続化の対応**: Null と 0 を意味的に区別（Unset vs Unknown など）
- **RepoDb の統一的な使用**: クエリ構文、トランザクション管理、パフォーマンス

---

## 2. 基本アーキテクチャ

### 層構成

```
Domain Layer
	↑ ← Entity / Aggregate（ValueObject 使用）
	│
Application Layer  
	↑ ← Use Case Service（ビジネスロジック）
	│
Infrastructure Layer
	├─ Repository Interface（抽象）
	├─ Repository Implementation（RepoDb ベース）
	└─ Mapper / Converter（DB 値 ↔ ValueObject 変換）
	│
Persistence Layer
	└─ RepoDb Connection / Query
	│
Database
	└─ SQL Server / MySQL など
```

### 責務分離

| レイヤ | 責務 | 例 |
|------|------|-----|
| **Repository Interface** | メソッドシグネチャ定義 | `IDriverRepository { GetDriver(id), AddDriver(...) }` |
| **Repository Impl** | RepoDb クエリ実行 | `connection.Query<Driver>(...)` |
| **Mapper** | DB 型 ↔ ValueObject 変換 | `DbDriver(carModelId: 0) → Driver(CarModel.Unknown)` |

---

## 3. Repository インターフェース設計

### 3.1 基本インターフェース（ジェネリック基底）

```csharp
/// <summary>
/// 汎用 Repository Interface
/// </summary>
/// <typeparam name="TEntity">Domain Entity 型</typeparam>
/// <typeparam name="TId">主キー型</typeparam>
public interface IRepository<TEntity, TId> where TEntity : class
{
  /// <summary>
  /// 主キーで Entity を取得
  /// </summary>
  Task<TEntity?> GetByIdAsync(TId id);

  /// <summary>
  /// 複数の Entity を取得
  /// </summary>
  Task<IEnumerable<TEntity>> GetAllAsync();

  /// <summary>
  /// Entity を追加（新規挿入）
  /// </summary>
  Task AddAsync(TEntity entity);

  /// <summary>
  /// Entity を更新
  /// </summary>
  Task UpdateAsync(TEntity entity);

  /// <summary>
  /// Entity を削除
  /// </summary>
  Task DeleteAsync(TId id);

  /// <summary>
  /// トランザクション開始
  /// </summary>
  Task<ITransaction> BeginTransactionAsync();
}
```

---

### 3.2 データマッパー（DB モデル ↔ Domain Entity）

```csharp
/// <summary>
/// DB 型 ↔ Domain Entity 型 の相互変換
/// </summary>
/// <typeparam name="TDbModel">DB テーブルモデル型</typeparam>
/// <typeparam name="TEntity">Domain Entity 型</typeparam>
public interface IDataMapper<TDbModel, TEntity> where TDbModel : class where TEntity : class
{
  /// <summary>
  /// DB モデル → Domain Entity
  /// </summary>
  TEntity ToDomain(TDbModel dbModel);

  /// <summary>
  /// Domain Entity → DB モデル
  /// </summary>
  TDbModel ToDatabase(TEntity entity);

  /// <summary>
  /// DB モデルが有効か検証
  /// </summary>
  /// <remarks>
  /// 例：CarModel の場合、carModelValue が -1 や 99 など無効値でないか確認
  /// </remarks>
  bool Validate(TDbModel dbModel);
}
```

---

## 4. RepoDb の使用パターン

### 4.1 接続管理

```csharp
using (var connection = new SqlConnection(connectionString))
{
  // RepoDb 初期化
  SqlServerBootstrap.Initialize();

  // クエリ実行
  var result = await connection.QueryAsync<DbDriver>();
}
```

### 4.2 SELECT / INSERT / UPDATE パターン

**SELECT**
```csharp
// 主キーで単一取得
var dbDriver = await connection.QueryAsync<DbDriver>(
  new { Id = driverId }
);

// 複数取得（全件）
var dbDrivers = await connection.QueryAllAsync<DbDriver>();

// 条件付き取得
var activeDrivers = await connection.QueryAsync<DbDriver>(
  new { IsActive = true }
);
```

**INSERT**
```csharp
var result = await connection.InsertAsync<DbDriver>(new DbDriver
{
  Name = "Taro Tanaka",
  CarModelValue = 1,  // CarModel.Sedan → 1
});
```

**UPDATE**
```csharp
await connection.UpdateAsync<DbDriver>(new DbDriver
{
  Id = driverId,
  CarModelValue = null,  // CarModel.Unset() → NULL
});
```

---

## 5. ValueObject 永続化パターン

### 5.1 IsSet フラグの DB 対応

**原則**

```
Domain Model               DB Schema
├─ IsSet=true            ├─ NOT NULL (0～6)
│  ValueField=0 (Unknown)│
├─ IsSet=false           ├─ NULL
│  ValueField=0 (Unset)  │
└─ IsSet=true            └─ NOT NULL (1～6)
   ValueField=1～6
```

**実装上の注意**

- DB カラムは **nullable int** (`INT NULL`)
- キー制約: 値が存在する場合は 0～6 の範囲に限定
- `TryFrom()` / `From()` / `Unset()` を活用して復元

### 5.2 マッピング時の NULL 判定

```csharp
int? dbValue = dbDriver.CarModelValue;

var carModel = dbValue switch
{
  null => CarModel.Unset(),           // NULL → Unset
  0    => CarModel.Unknown,           // 0 → Unknown
  >= 1 and <= 6 => CarModel.From(dbValue.Value),  // 1-6 → From()
  _    => throw new InvalidOperationException($"Invalid DB value: {dbValue}")
};
```

---

## 6. エラーハンドリング

### 6.1 DB 値が無効な場合

```csharp
// CarModel の場合、DB に 99 が入っていた（不整合）
int? invalidValue = 99;

bool success = CarModel.TryFrom(invalidValue, out var model);
if (!success)
{
  // エラー処理
  logger.LogError($"DB から無効な CarModel 値を復元: {invalidValue}");
  // Unset() として代替 or エラースロー
  model = CarModel.Unset();
}
```

### 6.2 トランザクション失敗

```csharp
try
{
  using (var transaction = await connection.BeginTransactionAsync())
  {
	await InsertDriverWithCarModelAsync(driver);
	await transaction.CommitAsync();
  }
}
catch (Exception ex)
{
  logger.LogError($"Repository 操作失敗: {ex.Message}");
  throw;
}
```

---

## 7. 実装例の配置

**ファイルパス**

- `src/Infrastructure/Persistence/Repositories/IRepository.cs` —基本 Interface
- `src/Infrastructure/Persistence/Repositories/IDataMapper.cs` — Mapper Interface
- `src/Infrastructure/Persistence/Mappers/DriverMapper.cs` — Driver 用具体 Mapper
- `src/Infrastructure/Persistence/Mappers/CarModelMapper.cs` — CarModel 用 Mapper
- `src/Infrastructure/Persistence/Repositories/DriverRepository.cs` — Driver Repository 実装

---

## 8. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------| 
| 1.0 | 2026-07-04 | 加藤 正人 | 初版作成。Repository パターン、DataMapper、RepoDb 活用パターンを設計 |

