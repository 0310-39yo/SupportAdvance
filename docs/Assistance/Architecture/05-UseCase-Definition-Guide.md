# UseCase 定義ガイド - SupportAdvance

**最終更新**: 2026-06-30

---

## 目次

1. [UseCase とは](#usecase-とは)
2. [基本構造](#基本構造)
3. [命名規則](#命名規則)
4. [実装ステップ](#実装ステップ)
5. [Request/Response DTO](#requestresponse-dto)
6. [例外戦略](#例外戦略)
7. [テスト方法](#テスト方法)

---

## UseCase とは

### 定義

**UseCase** は、1つのビジネスユーザーのアクションに対応する処理単位です。

```
ユーザーアクション
  ↓
UseCase (Application層)
  ↓ ビジネスルール実行
Domain層
  ↓ 永続化
Repository (Infrastructure層)
  ↓ 結果返却
Response (DTO)
```

### 例

| ユーザーアクション | UseCase | 処理内容 |
|---------|---------|---------|
| 車両設定を取得 | GetCarPreferenceUseCase | ID指定で取得 |
| 車両設定を更新 | UpdateCarPreferenceUseCase | 設定値を更新 |
| 車両設定を削除 | DeleteCarPreferenceUseCase | 論理削除実行 |

### 責務

```
✅ UseCase の責務:
  1. Request を受け取る
  2. Domain層のビジネスルール適用
  3. Repository呼び出し
  4. Response を返す

❌ UseCase の責務外:
  - UI操作（Presentation層）
  - DB操作（Infrastructure層）
  - ビジネスルール実装（Domain層）
```

---

## 基本構造

### インターフェース定義

```csharp
// Application/UseCases/IUseCase.cs
public interface IUseCase<in TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IResponse
{
    Task<TResponse> Execute(TRequest request);
}

// マーカーインターフェース
public interface IRequest { }
public interface IResponse { }
```

### UseCase実装テンプレート

```csharp
public class [ActionName]UseCase 
    : IUseCase<[ActionName]Request, [ActionName]Response>
{
    private readonly I[Entity]Repository _repository;
    private readonly ILogger<[ActionName]UseCase> _logger;
    
    public [ActionName]UseCase(
        I[Entity]Repository repository,
        ILogger<[ActionName]UseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<[ActionName]Response> Execute(
        [ActionName]Request request)
    {
        try
        {
            // ステップ1: 入力検証
            ValidateRequest(request);
            
            // ステップ2: 既存データ取得
            var entity = await _repository.GetById(request.Id);
            if (entity == null)
                return [ActionName]Response.NotFound();
            
            // ステップ3: ビジネスルール実行
            // (Domain層で実装)
            entity.PerformAction(request.Param);
            
            // ステップ4: 永続化
            await _repository.Save(entity);
            
            // ステップ5: 結果作成
            return [ActionName]Response.Success(entity);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning($"Business rule violation: {ex.Message}");
            return [ActionName]Response.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex}");
            return [ActionName]Response.Error("予期しないエラーが発生しました");
        }
    }
    
    private void ValidateRequest([ActionName]Request request)
    {
        // 入力チェック
        if (request.Id <= 0)
            throw new ArgumentException("IDは正の値である必要があります");
    }
}
```

---

## 命名規則

### UseCase名

**形式**: `[ビジネスアクション]UseCase`

```
✅ 良い例:
  - GetCarPreferenceUseCase       (取得)
  - CreateCarPreferenceUseCase    (作成)
  - UpdateCarPreferenceUseCase    (更新)
  - DeleteCarPreferenceUseCase    (削除)
  - ListCarPreferencesUseCase     (一覧)

❌ 悪い例:
  - CarPreferenceUseCase          (曖昧)
  - DoSomethingUseCase            (不明確)
  - CarPrefServiceUseCase         (命名規則外)
```

### Request/Response名

**形式**: `[ビジネスアクション]Request` / `[ビジネスアクション]Response`

```
✅ 良い例:
  GetCarPreferenceRequest / GetCarPreferenceResponse
  UpdateCarPreferenceRequest / UpdateCarPreferenceResponse

❌ 悪い例:
  CarPrefGetRequest / CarPrefGetReply
  RequestForGetting / ResponseOfGetting
```

### ディレクトリ構成

```
Application/
├─ UseCases/
│  ├─ IUseCase.cs
│  ├─ IRequest.cs
│  ├─ IResponse.cs
│  └─ Exceptions/
│     └─ UseCaseException.cs

CarPreferences.Application/
├─ UseCases/
│  ├─ Get/
│  │  ├─ GetCarPreferenceUseCase.cs
│  │  ├─ GetCarPreferenceRequest.cs
│  │  └─ GetCarPreferenceResponse.cs
│  ├─ Create/
│  │  ├─ CreateCarPreferenceUseCase.cs
│  │  ├─ CreateCarPreferenceRequest.cs
│  │  └─ CreateCarPreferenceResponse.cs
│  ├─ Update/
│  │  ├─ UpdateCarPreferenceUseCase.cs
│  │  ├─ UpdateCarPreferenceRequest.cs
│  │  └─ UpdateCarPreferenceResponse.cs
│  └─ Delete/
│     ├─ DeleteCarPreferenceUseCase.cs
│     ├─ DeleteCarPreferenceRequest.cs
│     └─ DeleteCarPreferenceResponse.cs
```

---

## 実装ステップ

### ステップ1: Request DTO作成

```csharp
// CarPreferences.Application/UseCases/Update/UpdateCarPreferenceRequest.cs
public class UpdateCarPreferenceRequest : IRequest
{
    public int Id { get; set; }
    public string Category { get; set; }
    
    public UpdateCarPreferenceRequest(int id, string category)
    {
        this.Id = id;
        this.Category = category;
    }
}
```

### ステップ2: Response DTO作成

```csharp
// CarPreferences.Application/UseCases/Update/UpdateCarPreferenceResponse.cs
public class UpdateCarPreferenceResponse : IResponse
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }
    public CarPreferenceDto? Data { get; private set; }
    
    public static UpdateCarPreferenceResponse Success(
        CarPreferenceDto data)
    {
        return new()
        {
            IsSuccess = true,
            Message = "更新成功",
            Data = data
        };
    }
    
    public static UpdateCarPreferenceResponse Error(string message)
    {
        return new()
        {
            IsSuccess = false,
            Message = message,
            Data = null
        };
    }
}

// DTO
public class CarPreferenceDto
{
    public int Id { get; set; }
    public string Model { get; set; }
    public string Category { get; set; }
}
```

### ステップ3: UseCase実装

```csharp
// CarPreferences.Application/UseCases/Update/UpdateCarPreferenceUseCase.cs
public class UpdateCarPreferenceUseCase
    : IUseCase<UpdateCarPreferenceRequest, UpdateCarPreferenceResponse>
{
    private readonly ICarPreferenceRepository _repository;
    private readonly ILogger<UpdateCarPreferenceUseCase> _logger;
    
    public UpdateCarPreferenceUseCase(
        ICarPreferenceRepository repository,
        ILogger<UpdateCarPreferenceUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<UpdateCarPreferenceResponse> Execute(
        UpdateCarPreferenceRequest request)
    {
        try
        {
            // 入力検証
            if (request.Id <= 0)
                return UpdateCarPreferenceResponse.Error(
                    "IDが不正です");
            
            // データ取得
            var carPref = await _repository.GetById(request.Id);
            if (carPref == null)
                return UpdateCarPreferenceResponse.Error(
                    "データが見つかりません");
            
            // ビジネスロジック実行（Domain層）
            var category = new PreferenceCategory(request.Category);
            carPref.UpdatePreference(category);
            
            // 永続化
            await _repository.Save(carPref);
            
            _logger.LogInformation(
                $"Updated car preference: {request.Id}");
            
            // 結果返却
            return UpdateCarPreferenceResponse.Success(
                new CarPreferenceDto
                {
                    Id = carPref.Id,
                    Model = carPref.Model,
                    Category = carPref.Category.Value
                });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning($"Domain error: {ex.Message}");
            return UpdateCarPreferenceResponse.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex}");
            return UpdateCarPreferenceResponse.Error(
                "予期しないエラーが発生しました");
        }
    }
}
```

### ステップ4: DI登録

```csharp
// CarPreferences.Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddCarPreferencesApplicationModels(
        this IServiceCollection services)
    {
        services.AddScoped<
            IUseCase<UpdateCarPreferenceRequest, UpdateCarPreferenceResponse>,
            UpdateCarPreferenceUseCase>();
        
        return services;
    }
}
```

---

## Request/Response DTO

### Request パターン

```csharp
// 単純な取得
public class GetCarPreferenceRequest : IRequest
{
    public int Id { get; set; }
}

// 検索（複数パラメータ）
public class SearchCarPreferencesRequest : IRequest
{
    public string? Category { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// 作成（複数値）
public class CreateCarPreferenceRequest : IRequest
{
    public string Model { get; set; }
    public string Category { get; set; }
}
```

### Response パターン

```csharp
// 成功結果
public class GetCarPreferenceResponse : IResponse
{
    public bool IsSuccess { get; private set; }
    public CarPreferenceDto? Data { get; private set; }
    
    public static GetCarPreferenceResponse Success(CarPreferenceDto data)
        => new() { IsSuccess = true, Data = data };
    
    public static GetCarPreferenceResponse NotFound()
        => new() { IsSuccess = false, Data = null };
}

// 一覧結果（ページネーション）
public class SearchCarPreferencesResponse : IResponse
{
    public bool IsSuccess { get; private set; }
    public List<CarPreferenceDto> Items { get; private set; } = new();
    public int Total { get; private set; }
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
}
```

---

## 例外戦略

### 層別の例外処理

```
Domain層
  └─ throw DomainException ← ビジネスルール違反

Application層
  └─ catch DomainException
     └─ return Response.Error(message) ← ユーザー向けメッセージ

Presentation層
  └─ if (!response.IsSuccess)
       └─ 画面にメッセージ表示
```

### コード例

```csharp
public async Task<UpdateCarPreferenceResponse> Execute(
    UpdateCarPreferenceRequest request)
{
    try
    {
        // ビジネスルール実行
        carPref.UpdatePreference(category);
        // ← DomainException がここで throw される
    }
    catch (DomainException ex)  // ← ここで catch
    {
        // ❌ 悪い例: ただ再スロー
        // throw;
        
        // ✅ 良い例: Response に変換
        return UpdateCarPreferenceResponse.Error(ex.Message);
    }
}
```

---

## テスト方法

### ユニットテスト例

```csharp
public class UpdateCarPreferenceUseCaseTests
{
    private readonly Mock<ICarPreferenceRepository> _mockRepository;
    private readonly UpdateCarPreferenceUseCase _useCase;
    
    public UpdateCarPreferenceUseCaseTests()
    {
        _mockRepository = new Mock<ICarPreferenceRepository>();
        _useCase = new UpdateCarPreferenceUseCase(
            _mockRepository.Object,
            NullLogger<UpdateCarPreferenceUseCase>.Instance);
    }
    
    [Fact]
    public async Task Execute_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new UpdateCarPreferenceRequest(1, "Comfort");
        var carPref = new CarPreference(1, "Model A");
        
        _mockRepository
            .Setup(x => x.GetById(1))
            .ReturnsAsync(carPref);
        
        // Act
        var response = await _useCase.Execute(request);
        
        // Assert
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        _mockRepository.Verify(x => x.Save(carPref), Times.Once);
    }
    
    [Fact]
    public async Task Execute_WithInvalidId_ReturnsError()
    {
        // Arrange
        var request = new UpdateCarPreferenceRequest(-1, "Comfort");
        
        // Act
        var response = await _useCase.Execute(request);
        
        // Assert
        Assert.False(response.IsSuccess);
        _mockRepository.Verify(x => x.Save(It.IsAny<CarPreference>()), 
            Times.Never);
    }
    
    [Fact]
    public async Task Execute_WhenRepositoryReturnsNull_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateCarPreferenceRequest(999, "Comfort");
        
        _mockRepository
            .Setup(x => x.GetById(It.IsAny<int>()))
            .ReturnsAsync((CarPreference?)null);
        
        // Act
        var response = await _useCase.Execute(request);
        
        // Assert
        Assert.False(response.IsSuccess);
    }
}
```

---

## チェックリスト

### UseCase実装時

- [ ] Request クラス作成（IRequest継承）
- [ ] Response クラス作成（IResponse継承）
- [ ] UseCase クラス作成（IUseCase<TRequest, TResponse>実装）
- [ ] DomainException catch 処理
- [ ] Response.Success() / Response.Error() 返却
- [ ] ロギング実装
- [ ] DI登録
- [ ] ユニットテスト作成

### コードレビュー時

- [ ] 命名規則準拠か？
- [ ] Request/Response DTO分離されているか？
- [ ] Domain層のビジネスロジック呼び出しか？
- [ ] 例外処理が適切か？
- [ ] Presentation/Infrastructure参照していないか？

---

**作成日**: 2026-06-30  
**参考**: 01-Layer-Architecture.md / 03-CleanArchitecture-Principles.md
