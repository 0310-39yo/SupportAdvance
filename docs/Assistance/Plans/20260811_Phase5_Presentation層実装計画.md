---
title: Phase 5 - Presentation 層実装計画
author: Claude Code
date: 2026-08-11
version: 1.0
---

# Phase 5 - Presentation 層実装計画

**目的:** Employee Context を完全に閉じ、API エンドポイントを実装して Presentation 層を完成させる。

**完成基準:**
- [ ] Employee API コントローラー実装（Create/GetById/GetByPersonRowId/Update/Delete）
- [ ] DI 設定完了（Program.cs）
- [ ] 統合テスト 30-40 個以上
- [ ] ビルド 0 エラー、全テスト成功
- [ ] Architecture.Tests 合格（依存関係検証）

---

## 📐 実装スコープ

### API エンドポイント

| メソッド | エンドポイント | 責務 |
|---------|---|---|
| POST | `/api/employees` | 新規従業員を作成 |
| GET | `/api/employees/{id}` | EmployeeId で従業員を取得 |
| GET | `/api/employees/by-person/{personRowId}` | PersonRowId で従業員を取得 |
| PUT | `/api/employees/{id}` | 従業員情報を更新 |
| DELETE | `/api/employees/{id}` | 従業員を削除（論理削除） |

### DTOs

```
Request:
├─ CreateEmployeeRequest
├─ UpdateEmployeeRequest
└─ DeleteEmployeeRequest

Response:
├─ EmployeeResponse
├─ CreateEmployeeResponse
└─ ErrorResponse
```

### DI 設定

```csharp
// Program.cs
builder.Services
  .AddEmployeeDomain()
  .AddEmployeeApplication()
  .AddEmployeeInfrastructure()
  .AddEmployeePresentationControllers()
```

---

## 🏗️ ファイル構成

```
src/Presentation/
├─ Employee.Controllers/
│  ├─ Employee.Controllers.csproj
│  ├─ EmployeesController.cs
│  ├─ Dtos/
│  │  ├─ CreateEmployeeRequest.cs
│  │  ├─ UpdateEmployeeRequest.cs
│  │  ├─ DeleteEmployeeRequest.cs
│  │  ├─ EmployeeResponse.cs
│  │  └─ ErrorResponse.cs
│  └─ Extensions/
│     └─ DependencyInjection.cs

tests/
├─ Presentation.Tests/
│  ├─ Presentation.Tests.csproj
│  ├─ Controllers/
│  │  ├─ EmployeesControllerTests.cs
│  │  ├─ CreateEmployeeTests.cs
│  │  ├─ GetEmployeeTests.cs
│  │  ├─ UpdateEmployeeTests.cs
│  │  └─ DeleteEmployeeTests.cs
│  └─ Fixtures/
│     ├─ EmployeeControllerFixture.cs
│     └─ TestDataBuilder.cs
```

---

## 🔄 実装フロー（TDD）

### フェーズ 1: テスト基盤構築
**所要時間:** 2-3 時間

1. **Test プロジェクト作成**
   - `Presentation.Tests.csproj` 作成
   - xUnit, Moq, FluentAssertions 参照追加
   - Controller テスト用 Fixture 作成

2. **ControllerFixture 実装**
   ```csharp
   public class EmployeeControllerFixture
   {
       private readonly Mock<ICreateEmployeeUseCase> _createUseCase;
       private readonly Mock<IGetEmployeeUseCase> _getUseCase;
       // ... 他の UseCase mock
       
       public EmployeesController CreateController() { }
   }
   ```

3. **TestDataBuilder 実装**
   ```csharp
   public class EmployeeTestDataBuilder
   {
       public CreateEmployeeRequest CreateValidRequest() { }
       public UpdateEmployeeRequest CreateValidUpdateRequest() { }
       // ... 他のテストデータ
   }
   ```

### フェーズ 2: Controller テスト・実装（Red-Green-Refactor）
**所要時間:** 4-5 時間

#### 1. Create エンドポイント
- テスト: 正常系、バリデーション失敗、使用例外
- 実装: POST `/api/employees`、201 Created 返却

#### 2. GetById エンドポイント
- テスト: 正常系、未発見時 404
- 実装: GET `/api/employees/{id}`

#### 3. GetByPersonRowId エンドポイント
- テスト: 正常系、未発見時 404
- 実装: GET `/api/employees/by-person/{personRowId}`

#### 4. Update エンドポイント
- テスト: 正常系、未発見時 404、バリデーション失敗
- 実装: PUT `/api/employees/{id}`、200 OK 返却

#### 5. Delete エンドポイント
- テスト: 正常系、未発見時 404、二重削除防止
- 実装: DELETE `/api/employees/{id}`、204 No Content

### フェーズ 3: DI 設定・統合
**所要時間:** 2-3 時間

1. **Employee.Controllers.csproj 作成**
   - Microsoft.AspNetCore.Mvc.Core
   - 参照: Employee.Application

2. **DependencyInjection.cs 実装**
   ```csharp
   public static class DependencyInjection
   {
       public static IServiceCollection AddEmployeePresentationControllers(
           this IServiceCollection services) { }
   }
   ```

3. **Program.cs 統合**
   ```csharp
   var builder = WebApplicationBuilder.CreateBuilder(args);
   builder.Services
       .AddEmployeeDomain()
       .AddEmployeeApplication()
       .AddEmployeeInfrastructure()
       .AddEmployeePresentationControllers();
   ```

### フェーズ 4: 統合テスト・検証
**所要時間:** 3-4 時間

1. **WebApplicationFactory 構築**
   ```csharp
   public class EmployeeApiFixture : IAsyncLifetime
   {
       private readonly WebApplicationFactory<Program> _factory;
       public HttpClient Client { get; private set; }
   }
   ```

2. **E2E テスト実装**
   - Create → GetById → Update → Delete フロー
   - エラーハンドリング
   - バリデーション

3. **Architecture.Tests 検証**
   - Presentation → Infrastructure 依存禁止（Program.cs 除く）
   - Presentation → Domain 依存禁止
   - 参照チェーン: Controller → UseCase → Repository

---

## 📝 テストケース一覧（30-40 個）

### EmployeesController Tests (10-12)
```
1. CreateEmployee_ValidRequest_ReturnsCreatedAtRoute
2. CreateEmployee_MissingRequiredField_ReturnsBadRequest
3. CreateEmployee_DuplicatePersonRowId_ReturnsConflict
4. CreateEmployee_UseCaseThrowsException_ReturnsInternalServerError
5. GetEmployeeById_ValidId_ReturnsEmployee
6. GetEmployeeById_InvalidId_ReturnsNotFound
7. GetEmployeeByPersonRowId_ValidPersonRowId_ReturnsEmployee
8. GetEmployeeByPersonRowId_InvalidPersonRowId_ReturnsNotFound
9. UpdateEmployee_ValidRequest_ReturnsOkWithUpdatedEmployee
10. UpdateEmployee_InvalidId_ReturnsNotFound
11. DeleteEmployee_ValidId_ReturnsNoContent
12. DeleteEmployee_InvalidId_ReturnsNotFound
```

### Integration Tests (15-20)
```
1. CreateAndGetEmployee_ReturnsCorrectData
2. CreateAndUpdateEmployee_UpdatesCorrectly
3. CreateAndDeleteEmployee_LogicalDeleteWorks
4. GetByPersonRowId_AfterCreate_ReturnsCreatedEmployee
5. MultipleCreates_IndependentRows
6. ConcurrentOperations_HandleCorrectly
7. ErrorHandling_DatabaseConnection_ReturnsServiceUnavailable
8. ValidationErrors_DetailedErrorMessages
9. Conflict_DuplicatePersonRowId_ReturnsConflict
10. NotFound_DeletedEmployee_ReturnsNotFound
11. UpdateDeleted_ReturnsNotFound
12. AuditFields_AutoPopulated_OnCreate
13. AuditFields_UpdatedCorrectly_OnUpdate
14. AuditFields_DeletedBySet_OnDelete
15. Concurrency_OptimisticLock_Conflict
```

### Architecture Tests (5-8)
```
1. EmployeeControllers_ShouldNotDependOn_Infrastructure
2. EmployeeControllers_ShouldNotDependOn_Domain
3. EmployeeControllers_ShouldDependOn_Application
4. DependencyChain_ShouldBeValid
5. AllControllers_ShouldInheritFromControllerBase
6. AllDtos_ShouldBeSerializable
```

---

## 🔍 実装チェックリスト

### フェーズ 1: 基盤
- [ ] `Presentation.Tests.csproj` 作成
- [ ] `Employee.Controllers.csproj` 作成
- [ ] EmployeeControllerFixture 実装
- [ ] TestDataBuilder 実装
- [ ] プロジェクト参照設定確認

### フェーズ 2: Controller 実装
- [ ] CreateEmployee API テスト・実装
- [ ] GetEmployeeById API テスト・実装
- [ ] GetEmployeeByPersonRowId API テスト・実装
- [ ] UpdateEmployee API テスト・実装
- [ ] DeleteEmployee API テスト・実装
- [ ] エラーハンドリング（BadRequest, NotFound, Conflict）

### フェーズ 3: DI・統合
- [ ] DependencyInjection.cs 実装
- [ ] Program.cs 統合
- [ ] ビルド 0 エラー確認
- [ ] 全テスト成功確認（単体 + 統合）
- [ ] Architecture.Tests 成功確認

### フェーズ 4: 最終検証
- [ ] コード品質レビュー
- [ ] ドキュメント整理
- [ ] リグレッション テスト
- [ ] メモリ更新（完成報告）

---

## 📊 進捗追跡

### テスト成功基準
| フェーズ | テスト数 | 必須テスト数 |
|---------|---------|-----------|
| フェーズ 1 | 5-8 | 100% (Fixture) |
| フェーズ 2 | 20-25 | 100% (すべてのエンドポイント) |
| フェーズ 3 | 10-12 | 100% (統合) |
| **合計** | **35-45** | **100% (1,300+)** |

### ビルド・品質基準
- ✅ ビルド エラー: 0
- ✅ テスト 失敗: 0
- ✅ Architecture.Tests: 100% 合格
- ✅ コード警告: 可能な限り低減

---

## 🚀 開始手順

```bash
# 1. 計画ドキュメント確認
# → このファイル

# 2. プロジェクト作成
dotnet new classlib -n Employee.Controllers -o src/Presentation/Employee.Controllers
dotnet new xunit -n Presentation.Tests -o tests/Presentation.Tests

# 3. 参照追加
# → src/Presentation/Employee.Controllers.csproj に ProjectReference 追加
# → tests/Presentation.Tests.csproj に ProjectReference 追加

# 4. ビルド確認
dotnet build

# 5. TDD 開始
# → フェーズ 1 から順次進行
```

---

## 📚 参考資料

- **CLEAN_ARCHITECTURE_GUIDELINES.md**: Presentation 層の依存ルール
- **Employee.Domain テストパターン**: Domain 層テストの実装例
- **Repository_パターンガイド.md**: Infrastructure 層の参考
- **Entity_And_DomainEvents_概要.md**: ドメインイベント利用（オプション）

---

## 完成時の成果

✅ **Employee Context 完全完成**
- Domain 層: ValueObject + Entity（完成済み、Phase 1-2）
- Application 層: Use Cases + DTOs（完成済み、Phase 3-4）
- **Presentation 層: API Controllers + DI（本フェーズ）**
- Infrastructure 層: Repository + DbModel + Mapper（完成済み）

✅ **テスト体系完成**
- Domain Unit Tests: 515
- Application Unit Tests: (含まれた)
- Infrastructure Tests: 9
- **Presentation Integration Tests: 30-40（本フェーズ）**
- Architecture Validation: 54
- **合計: 1,300+ テスト**

✅ **API 完全運用可能**
- 従業員作成・取得・更新・削除
- エラーハンドリング
- DI 自動解決

---

**Next Step:** フェーズ 1（テスト基盤構築）から開始
