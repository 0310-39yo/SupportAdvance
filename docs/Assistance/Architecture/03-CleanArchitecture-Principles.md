# クリーンアーキテクチャの原則 - SupportAdvance

**最終更新**: 2026-06-30

---

## 目次

1. [DIP：依存性逆転原則](#dip依存性逆転原則)
2. [SOLID原則の応用](#solid原則の応用)
3. [インターフェース設計](#インターフェース設計)
4. [抽象と具象の分離](#抽象と具象の分離)
5. [実装パターン](#実装パターン)

---

## DIP：依存性逆転原則

### 定義

**Dependency Inversion Principle (DIP)**

> 上位モジュールは下位モジュールに依存してはならない。
> 両者ともに抽象に依存すべき。

### 図解

```
❌ 標準的な依存関係:

UseCase (高位)
  ↓ 依存
SqlRepository (低位・具象)

→ 問題: UseCase が具象実装 SqlRepository に依存
  - Repository実装変更 → UseCase影響
  - テスト時にモック不可


✅ 依存性逆転:

UseCase (高位)
  ↓ 依存
IRepository (抽象)
  ↑ 実装
SqlRepository (低位・具象)

→ 利点: UseCase は抽象 IRepository に依存
  - Repository実装変更 → UseCase不変
  - テスト時にモック可能
```

### コード例

```csharp
// ❌ 悪い例：UseCase が具象に依存
public class GetCarPreferenceUseCase
{
    private readonly SqlCarPreferenceRepository _repository;  // 具象
    
    public GetCarPreferenceUseCase()
    {
        _repository = new SqlCarPreferenceRepository();  // 直接生成
    }
}

// ✅ 良い例：UseCase が抽象に依存
public interface ICarPreferenceRepository
{
    Task<CarPreference> GetById(int id);
}

public class GetCarPreferenceUseCase
{
    private readonly ICarPreferenceRepository _repository;  // 抽象
    
    public GetCarPreferenceUseCase(ICarPreferenceRepository repository)
    {
        _repository = repository;  // DI経由で注入
    }
}

// 実装：どれでも選べる
public class SqlCarPreferenceRepository : ICarPreferenceRepository { }
public class DapperCarPreferenceRepository : ICarPreferenceRepository { }
public class MockCarPreferenceRepository : ICarPreferenceRepository { }
```

### メリット

- ✅ **テスト容易性**: モック実装で簡単テスト
- ✅ **実装の入れ替え可能性**: 新しい実装に切り替え可能
- ✅ **疎結合**: 上位と下位が独立
- ✅ **拡張性**: 新規実装追加が容易

---

## SOLID原則の応用

### SRP：単一責任原則

```
❌ 悪い例：複数の責務を持つクラス

public class CarPreferenceRepository
{
    public async Task<CarPreference> GetById(int id)
    {
        // 責務1: DB操作
    }
    
    public void SendNotification(CarPreference cp)
    {
        // 責務2: メール送信 ❌
    }
    
    public void LogActivity(string activity)
    {
        // 責務3: ログ出力 ❌
    }
}

✅ 良い例：単一の責務

// Repository：永続化のみ
public class CarPreferenceRepository : ICarPreferenceRepository
{
    public async Task<CarPreference> GetById(int id) { }
}

// NotificationService：通知のみ
public class CarPreferenceNotificationService
{
    public async Task NotifyUpdate(CarPreference cp) { }
}

// Logger：ログ出力のみ（Crosscutting層）
public class ActivityLogger
{
    public void Log(string activity) { }
}
```

### OCP：開放閉鎖原則

```
❌ 悪い例：新機能追加で既存コード変更

public class RepositoryFactory
{
    public ICarPreferenceRepository Create(string type)
    {
        if (type == "Sql")
            return new SqlCarPreferenceRepository();
        else if (type == "Dapper")  // 新規追加
            return new DapperCarPreferenceRepository();  // 既存コード変更！
        else if (type == "Repository")  // さらに追加
            return new RepoDbCarPreferenceRepository();
    }
}

✅ 良い例：DI経由で新実装を登録

// 起動時に一度だけ設定
services.AddScoped<ICarPreferenceRepository, SqlCarPreferenceRepository>();
// 変更時は設定のみ変更、既存コード不変

public class GetCarPreferenceUseCase
{
    // IRepository受け取り、実装は気にしない
    public GetCarPreferenceUseCase(ICarPreferenceRepository repository) { }
}
```

### LSP：リスコフの置換原則

```
❌ 悪い例：インターフェース違反

public interface ICarPreferenceRepository
{
    Task<CarPreference> GetById(int id);
}

public class MockCarPreferenceRepository : ICarPreferenceRepository
{
    public Task<CarPreference> GetById(int id)
    {
        // インターフェース契約違反：Task返さない
        return null;  // ❌ 呼び出し側が null チェック必要
    }
}

✅ 良い例：インターフェース契約を守る

public class MockCarPreferenceRepository : ICarPreferenceRepository
{
    public Task<CarPreference> GetById(int id)
    {
        return Task.FromResult(new CarPreference { Id = id });
    }
}
```

### ISP：インターフェース分離原則

```
❌ 悪い例：太いインターフェース

public interface ICarPreferenceService
{
    Task<CarPreference> GetById(int id);
    Task Save(CarPreference cp);
    void SendNotification(CarPreference cp);  // なぜここに？
    void LogActivity(string msg);  // なぜここに？
}

✅ 良い例：インターフェース分離

public interface ICarPreferenceRepository
{
    Task<CarPreference> GetById(int id);
    Task Save(CarPreference cp);
}

public interface ICarPreferenceNotifier
{
    Task Notify(CarPreference cp);
}

public interface IActivityLogger
{
    void Log(string message);
}
```

### DIP（既述）

---

## インターフェース設計

### インターフェースは依存される側で定義

**重要ルール**:

```
❌ 禁止パターン:

Infrastructure層でインターフェース定義
public interface IRepository { }

Application層が参照
public class UseCase
{
    public UseCase(IRepository repository) { }
}

→ Application が Infrastructure に依存


✅ 正しいパターン:

Application層でインターフェース定義
public interface IRepository { }

Infrastructure層が実装
public class SqlRepository : IRepository { }

→ Infrastructure が Application に依存（逆向き）
```

### インターフェース設計のコツ

```csharp
// ✅ 良い例：明確な責務を表現

// 1. 単一責務
public interface ICarPreferenceRepository
{
    Task<CarPreference> GetById(int id);
    Task Save(CarPreference cp);
}

// 2. 命名は実装に影響されない
// ❌ ISqlCarPreferenceRepository  ← SQL限定？
// ✅ ICarPreferenceRepository     ← 実装非依存

// 3. 戻り値は抽象
public interface IUnitOfWork
{
    // ❌ SqlCommand ExecuteCommand();  ← 具象
    // ✅ Task Execute();                ← 抽象
}

// 4. 必要なメソッドのみ
public interface ICarPreferenceRepository
{
    Task<CarPreference> GetById(int id);
    Task Save(CarPreference cp);
    // 不要: Delete, Update等は Save に統一可能
}
```

---

## 抽象と具象の分離

### 層ごとの抽象レベル

```
Presentation層
  ↓ (具体的なUI実装)
Application層
  ↓ (抽象: IRepository, IUnitOfWork)
Domain層
  ↓ (最も抽象: ビジネスルール)
Infrastructure層
  ↓ (具体的な実装: SqlRepository, DapperRepository)
```

### ファイル配置ルール

```
✅ Application層
  ├─ UseCases/
  │  └─ IUseCase.cs         ← インターフェース
  ├─ Repositories/
  │  └─ ICarPreferenceRepository.cs  ← インターフェース
  └─ UnitOfWorks/
     └─ IUnitOfWork.cs      ← インターフェース

✅ Infrastructure層
  ├─ Repositories/
  │  └─ CarPreferenceRepository.cs   ← 実装
  ├─ UnitOfWorks/
  │  └─ UnitOfWork.cs      ← 実装
  └─ (IRepository参照のみ)
```

### 参照方向

```
❌ 双方向参照（循環依存）:
Application ↔ Infrastructure

✅ 一方向（依存性逆転）:
Infrastructure → Application
（Application が定義した IRepository を Infrastructure が実装）
```

---

## 実装パターン

### パターン1：Repository パターン

```csharp
// Application層：インターフェース定義
public interface ICarPreferenceRepository
{
    Task<CarPreference> GetById(int id);
    Task<IEnumerable<CarPreference>> GetAll();
    Task Save(CarPreference cp);
}

// Infrastructure層：実装
public class CarPreferenceRepository : ICarPreferenceRepository
{
    private readonly IDbConnection _connection;
    
    public async Task<CarPreference> GetById(int id)
    {
        const string sql = "SELECT * FROM CarPreferences WHERE Id = @id";
        return await _connection.QueryFirstAsync<CarPreference>(sql, new { id });
    }
    
    public async Task Save(CarPreference cp)
    {
        // RepoDB等で保存
    }
}

// Application層：UseCase で使用
public class GetCarPreferenceUseCase
{
    private readonly ICarPreferenceRepository _repository;
    
    public GetCarPreferenceUseCase(ICarPreferenceRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<CarPreferenceResponse> Execute(int id)
    {
        var cp = await _repository.GetById(id);
        return new CarPreferenceResponse { /* ... */ };
    }
}
```

### パターン2：Strategy パターン

```csharp
// 異なる実装を切り替え可能

public interface ICarPreferenceStrategy
{
    Task<CarPreference> GetPreference(int id);
}

public class SqlStrategy : ICarPreferenceStrategy
{
    public async Task<CarPreference> GetPreference(int id) { }
}

public class CacheStrategy : ICarPreferenceStrategy
{
    private readonly ICarPreferenceStrategy _inner;
    
    public CacheStrategy(ICarPreferenceStrategy innerStrategy)
    {
        _inner = innerStrategy;
    }
    
    public async Task<CarPreference> GetPreference(int id)
    {
        // キャッシュ確認
        // なければ _inner.GetPreference() を呼ぶ
    }
}

// 起動時に選択
services.AddScoped<ICarPreferenceStrategy>(
    _ => new CacheStrategy(new SqlStrategy())
);
```

### パターン3：Decorator パターン

```csharp
// Repository に横断的関心事を追加

public interface ICarPreferenceRepository
{
    Task<CarPreference> GetById(int id);
}

public class CarPreferenceRepository : ICarPreferenceRepository
{
    public async Task<CarPreference> GetById(int id) { }
}

// ロギングを追加
public class LoggingCarPreferenceRepository : ICarPreferenceRepository
{
    private readonly ICarPreferenceRepository _inner;
    private readonly ILogger<LoggingCarPreferenceRepository> _logger;
    
    public LoggingCarPreferenceRepository(
        ICarPreferenceRepository inner, 
        ILogger<LoggingCarPreferenceRepository> logger)
    {
        _inner = inner;
        _logger = logger;
    }
    
    public async Task<CarPreference> GetById(int id)
    {
        _logger.LogInformation($"Getting car preference: {id}");
        return await _inner.GetById(id);
    }
}

// 登録
services.AddScoped<ICarPreferenceRepository, CarPreferenceRepository>();
services.Decorate<ICarPreferenceRepository, LoggingCarPreferenceRepository>();
```

---

## 主要な設計判断

### Q: いつインターフェースを作るべき？

**A**: 以下のいずれかに該当する場合

1. ✅ 複数の実装を提供する可能性
2. ✅ テスト時にモック実装が必要
3. ✅ 実装を後から変更したい
4. ✅ 別の層が実装を提供する

### Q: インターフェースが多すぎる場合は？

**A**: 以下を確認

1. ✅ 単一責任か？（複合責務なら分割）
2. ✅ 実装が複数あるか？（1つなら具象で可）
3. ✅ テスト対象か？（テスト対象なら必要）

### Q: 抽象層を増やすと複雑になる

**A**: 短期の複雑さ vs 長期の保守性

- 初期は具象から始める
- テストやハブが必要になったら抽象化
- **過度な抽象化は避ける**（YAGNIの原則）

---

**作成日**: 2026-06-30  
**参考**: 02-Dependency-Rules.md / AGENTS.md
