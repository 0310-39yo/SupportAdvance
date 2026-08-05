了解しました。**DML（SELECT/INSERT/UPDATE/DELETE）のSQL群をファイル管理する** ですね。

## 推奨構成

```
src/Infrastructure/
└── Persistence/
    ├── Sql/
    │   ├── Cars/
    │   │   ├── GetCarById.sql
    │   │   ├── GetAllCars.sql
    │   │   ├── InsertCar.sql
    │   │   └── UpdateCar.sql
    │   ├── Auth/
    │   │   ├── GetUserByEmail.sql
    │   │   └── InsertUser.sql
    │   └── Users/
    │       └── GetUserById.sql
    └── Repositories/
        └── CarRepository.cs     # SQL ファイルを読み込んで Dapper で実行
```

## 実装（2ステップ）

**ステップ1: SQL ファイルローダーを作成**

```csharp
// src/Infrastructure/Persistence/SqlQueryLoader.cs
public static class SqlQueryLoader
{
    public static string LoadQuery(string queryPath)
    {
        var assembly = typeof(SqlQueryLoader).Assembly;
        var resourceName = $"SupportAdvance.Infrastructure.Persistence.Sql.{queryPath}.sql";
        
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"SQL ファイルが見つかりません: {queryPath}");
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
```

**ステップ2: `.csproj` に埋め込み設定**

```xml
<!-- src/Infrastructure/Infrastructure.csproj -->
<ItemGroup>
    <EmbeddedResource Include="Persistence/Sql/**/*.sql" />
</ItemGroup>
```

**ステップ3: Repository で使用**

```csharp
public class CarRepository : ICarRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CarRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Car?> GetByIdAsync(int id)
    {
        var sql = SqlQueryLoader.LoadQuery("Cars.GetCarById");
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Car>(sql, new { Id = id });
    }

    public async Task InsertAsync(Car car)
    {
        var sql = SqlQueryLoader.LoadQuery("Cars.InsertCar");
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, car);
    }
}
```

**SQL ファイル例:**

```sql
-- src/Infrastructure/Persistence/Sql/Cars/GetCarById.sql
SELECT 
    Id,
    Name,
    CreatedAt,
    UpdatedAt
FROM Cars
WHERE Id = @Id;
```

------

## メリット

✅ SQL Server → PostgreSQL への切り替えが **SQL ファイルだけの変更** で済む
 ✅ SQL が C# コードから独立
 ✅ DBA や SQL チームが単独で保守可能
 ✅ ビルド時に DLL に埋め込まれ、ファイルシステム依存なし

**今からこの構造で実装していきたいですか？** 既存のリポジトリがあれば、一緒に移行を進められます。