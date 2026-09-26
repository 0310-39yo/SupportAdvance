# Phase 3: Application / Infrastructure層 テスト・ドキュメント整備 実装計画

**作成日:** 2026-09-16
**対象読者:** 本タスクを実装する担当者（人間 または 別セッションのAIエージェント）
**位置づけ:** テスト・テスト仕様書整備プロジェクトの Phase 3。**Phase 2（Domain層整備）完了後**の着手を推奨（UseCase/RepositoryはEntity/ValueObjectに依存するため）。
**前提知識:** ルート `CLAUDE.md`、`src/Infrastructure/CLAUDE.md`（Infrastructure層のnull処理・依存ルール）を事前に読むこと。

---

## 0. 背景・目的

Application層（UseCase）とInfrastructure層（Mapper/Repository/横断的関心事）は、Domain層の次に優先度が高い。特に **Authentication BC は実装は進んでいるがテストがプレースホルダのままの箇所が多い**（`Assert.True(true)` のみ）ため、セキュリティ上重要な機能（パスワードハッシュ化、認証フロー）が実質未検証の状態にある。本Phaseで解消する。

**テンプレート:**
- UseCase: `docs/Assistance/Templates/Test/TEMPLATE_単体テスト仕様書_UseCase.md`
- Mapper/Repository/DIサービス等: `docs/Assistance/Templates/Test/TEMPLATE_単体テスト仕様書_汎用.md`

**ドキュメント配置の慣習:** Application層は「1 Bounded Context 1ファイルに全UseCaseをまとめる」形式（既存例: `docs/Contexts/Employee/Application/Application_単体テスト仕様書.md`）を踏襲する。Infrastructure層も同様に1ファイルにまとめてよい（Phase 1 の `docs/Contexts/Department/Infrastructure/Infrastructure_単体テスト仕様書.md` と同じ形式）。

**着手前の必須確認:** 記載のクラス名・シグネチャは2026-09-16時点の調査結果。実装着手前に必ず対象クラスを `Read` して現在のAPIを確認すること（Phase 0 で Employee.Application/Infrastructure のテストコードが大きくbit-rotしていたことが判明しており、他のBCでも同様のドリフトが無いとは限らない）。

---

## 1. Authentication BC（最優先・最も手薄）

対象: `src/Contexts/Authentication/`。テストプロジェクト: `tests/Contexts/Authentication.Application.Tests/`, `tests/Contexts/Authentication.Infrastructure.Tests/`（いずれも`.slnx`登録済み、ビルドは通る）。`docs/Contexts/Authentication/` は現在空。

### 1-1. Application層

| クラス | ファイル | 現状 | 対応 |
|---|---|---|---|
| `UseCases.AuthenticateLocalUserUseCase` | `Authentication.Application/UseCases/AuthenticateLocalUserUseCase.cs` | テストは `tests/Contexts/Authentication.Application.Tests/UseCases/AuthenticateLocalUserUseCaseTests.cs` に存在するが `Placeholder_RunsWithoutError` / `Assert.True(true)` のみ | 実ロジック（5ステップ認証フロー、3つの失敗パス）を検証する本格的なテストに差し替える |
| `UseCases.LogoutUseCase` | `Authentication.Application/UseCases/LogoutUseCase.cs` | テストファイル無し | 新規作成 |
| `UseCases.FindEmployeeByADUseCase` | `Authentication.Application/UseCases/FindEmployeeByADUseCase.cs` | 未実装（`// 実装予定`のみ） | **本体が未実装のため対象外。実装された時点で別途対応** |
| `Dtos.AuthenticateLocalUserRequest`/`Response` | `Authentication.Application/Dtos/` | テスト無し | 優先度低（トリビアルなDTO）。UseCaseテストの中で間接的に検証されれば十分、独立テストは任意 |
| `Queries.ILoginCredentialsQuery`/`LoginCredentialsQueryResult` | `Authentication.Application/Queries/` | インターフェース+DTO | インターフェースは対象外。DTOは低優先度 |
| `Services.IPasswordHashService` | `Authentication.Application/Services/` | インターフェース | 対象外（実装側 `PasswordHashService` でテストする） |

**`AuthenticateLocalUserUseCase` の作業手順:**
1. `Authentication.Application/UseCases/AuthenticateLocalUserUseCase.cs` を `Read` し、5ステップの認証フロー（ログインID検索 → パスワード検証 → セッション発行 等、実装を正として確認する）と3つの失敗パス（該当ユーザーなし、パスワード不一致、その他）を把握する。
2. 依存する `IPasswordHashService`, `ILoginCredentialsQuery`, `IUserAuthSessionRepository` 等はテストダブル（Mock/Fake）を用意して置き換える。`docs/Assistance/Templates/Test/TEMPLATE_単体テスト仕様書_UseCase.md` の「2.X テスト用実装とDI設定」章にある `InMemoryRepository`/`FixedClock`/`Mock DomainService` パターンを参照。
3. `tests/Contexts/Authentication.Application.Tests/UseCases/AuthenticateLocalUserUseCaseTests.cs` を書き直す（既存のプレースホルダクラスを置き換える）。
4. `LogoutUseCase` は同様の手順で新規に `tests/Contexts/Authentication.Application.Tests/UseCases/LogoutUseCaseTests.cs` を作成する。

配置先ドキュメント: `docs/Contexts/Authentication/Application/Application_単体テスト仕様書.md`（1ファイルに `AuthenticateLocalUserUseCase`/`LogoutUseCase` をまとめる）

### 1-2. Infrastructure層

| クラス | ファイル | 現状 | 対応 |
|---|---|---|---|
| `Services.PasswordHashService` | `Authentication.Infrastructure/Services/PasswordHashService.cs` | テストファイル無し | **セキュリティ上最重要**。PBKDF2ハッシュ生成・検証ロジックが完全に未検証。優先度最高 |
| `Repositories.UserAuthSessionRepository` | `Authentication.Infrastructure/Repositories/UserAuthSessionRepository.cs` | `tests/Contexts/Authentication.Infrastructure.Tests/Repositories/UserAuthSessionRepositoryTests.cs` は「Phase 4-Dスケルトン実装」段階のプレースホルダのみ | Save/Get/GetLatest/Update の実装を検証するテストに差し替える。DB接続が必要な場合はPhase 0-1・Phase 1の `EmployeeRepository`/`DepartmentRepository` と同じ方針（実DB依存部分は結合テストへ切り出し）で対応する |
| `Mappers.UserAuthSessionMapper` | `Authentication.Infrastructure/Mappers/UserAuthSessionMapper.cs` | テストファイル無し | 新規作成。DB非依存の純粋変換ロジックのため着手コスト低 |
| `Queries.LoginCredentialsQueryService` | `Authentication.Infrastructure/Queries/LoginCredentialsQueryService.cs` | テストファイル無し | 新規作成 |
| `DbModels.LoginCredentialsDbModel`/`UserAuthSessionDbModel` | — | POCO | 対象外 |
| E2E `LoginDialog` UI フロー | `tests/Contexts/Authentication.Infrastructure.Tests/E2E/LoginDialogUITests.cs` | 「Phase 4-Eスケルトン実装」プレースホルダ。`E2E_TEST_GUIDE.md` への参照があるが所在未確認 | 本Phaseの対象外（Presentation層寄りのE2Eテストのため、必要なら別タスクとして扱う） |

**`PasswordHashService` の作業手順（最優先）:**
1. `Authentication.Infrastructure/Services/PasswordHashService.cs` を `Read` し、PBKDF2のハッシュ生成・検証APIのシグネチャを確認する。
2. `tests/Contexts/Authentication.Infrastructure.Tests/Services/PasswordHashServiceTests.cs` を新規作成し、以下を検証する:
   - 同じ平文パスワードでも、ハッシュ生成のたびに異なるハッシュ値（salt）が生成されること
   - 正しいパスワードでの検証が成功すること
   - 誤ったパスワードでの検証が失敗すること
   - 空文字列・null 等の異常入力に対するガード

配置先ドキュメント: `docs/Contexts/Authentication/Infrastructure/Infrastructure_単体テスト仕様書.md`（1ファイルに `PasswordHashService`/`UserAuthSessionRepository`/`UserAuthSessionMapper`/`LoginCredentialsQueryService` をまとめる）

---

## 2. Employee BC の残りのApplication/Infrastructure

対象: `src/Contexts/Employee/`。**Phase 0 で `Employee.Application.Tests`/`Employee.Infrastructure.Tests` のビルドが修復済みであることが前提**。

| クラス | ファイル | 現状 | 対応 |
|---|---|---|---|
| `Application.UseCases.GetEmployeeByBizIdUseCase` | `Employee.Application/UseCases/GetEmployeeByBizIdUseCase.cs` | テスト・ドキュメントとも無し（兄弟UseCase `GetEmployeeByIdUseCase`/`GetEmployeesByPersonRowIdUseCase` 等は揃っている） | 新規テスト作成。兄弟UseCaseのテスト（`tests/Contexts/Employee.Application.Tests/UseCases/GetEmployeeByIdUseCaseTests.cs`）と同じパターンで作成し、`docs/Contexts/Employee/Application/Application_単体テスト仕様書.md` に追記する |
| `Application.Extensions.EmployeeExtensions`（`ToDto`拡張メソッド） | `Employee.Application/Extensions/EmployeeExtensions.cs` | テスト・ドキュメント無し。`EmployeeDtoMapper.ToDto` と類似だが `DepartmentMemberships` の部署名結合ロジックを追加で持つ | 新規作成。`EmployeeDtoMapper` との役割の違い（重複していないか）を確認した上でテストを書く |
| `Application.Queries.EmployeeQueryService` | `Employee.Application/Queries/EmployeeQueryService.cs` | テスト・ドキュメント無し。`IQueryServiceWithBizId<IEmployee,...>` と `IEmployeeQueryService` を実装 | 新規作成。Department BC の `DepartmentQueryService`（Phase 1）と同様の委譲パターンのはず |
| `Infrastructure.Mappers.PersonMapper` | `Employee.Infrastructure/Mappers/PersonMapper.cs` | テストファイル無し（`EmployeeMapper` はPhase 0で修復対象） | 新規作成。DB非依存の純粋変換ロジック |
| `Infrastructure.Mappers.EmployeeMapper` | `Employee.Infrastructure/Mappers/EmployeeMapper.cs` | テストはPhase 0で修復済みの前提 | ドキュメントのみ追加 |
| `Infrastructure.Repositories.EmployeeRepository` | `Employee.Infrastructure/Repositories/EmployeeRepository.cs` | テストはPhase 0で修復済みの前提（一部skip） | ドキュメントのみ追加 |

配置先ドキュメント: 既存の `docs/Contexts/Employee/Application/Application_単体テスト仕様書.md` に `GetEmployeeByBizIdUseCase`/`EmployeeExtensions`/`EmployeeQueryService` を追記。Infrastructure層は新規に `docs/Contexts/Employee/Infrastructure/Infrastructure_単体テスト仕様書.md` を作成し `EmployeeMapper`/`PersonMapper`/`EmployeeRepository` をまとめる。

---

## 3. Department BC の Application/Infrastructure

**Phase 1（`20260916_テスト整備実装計画_Phase1_DepartmentBCテスト基盤新設.md`）で対応済み。** 本Phaseでは重複作業しないこと。Phase 1 が未着手の場合は先にそちらを実施すること。

---

## 4. 横断的関心事（Infrastructure 汎用 / Crosscutting）

対象: `src/Infrastructure/`（generic層、Bounded Context非依存）, `src/Crosscutting/`。テストプロジェクト: `tests/Infrastructure.Tests/`（`.slnx`登録済み。Phase 0-2 で `SequenceProvider` 対応済みの前提）, `tests/Crosscutting.Tests/`（登録済みだが空、本Phaseで実体を追加）。

| クラス | ファイル | 優先度 | 理由・テスト方針 |
|---|---|---|---|
| `Infrastructure.Persistence.SqlQueryLoader` | `src/Infrastructure/Persistence/SqlQueryLoader.cs` | 中 | DB非依存の純粋ロジック（名前空間からのリソース名構築、パース処理）。着手コスト低 |
| `Infrastructure.Persistence.DbConnectionFactory` | `src/Infrastructure/Persistence/DbConnectionFactory.cs` | 中 | `GetConnectionString` のフォールバック順ロジックはDB非依存でテスト可能（`SequenceProvider`と同様の優先順位パターンの可能性、Phase 0-2 の対応を参考にする） |
| `Infrastructure.Repositories.RepositoryBase<TEntity,TDbModel,TId>` | `src/Infrastructure/Repositories/RepositoryBase.cs` | 中 | 監査フィールド（`SetCreatedByAudit`等）をリフレクションで設定するロジックを持つ。全Repositoryの基盤なのでバグ影響範囲が大きい。フェイクの`TDbModel`を用意してテスト |
| `Infrastructure.Repositories.MultiTableRepositoryBase<TEntity,TDbModel,TId>` | `src/Infrastructure/Repositories/MultiTableRepositoryBase.cs` | 中 | 同上（`SetAuditField<T>`の一般化版） |
| `Infrastructure.Migrations.MigrationRunner`（`ExtractMigrationName`部分のみ） | `src/Infrastructure/Migrations/MigrationRunner.cs` | 低 | 大部分がSQL Server I/O。`ExtractMigrationName`のようなファイル名パース等の純粋ロジック部分のみ対象 |
| `Infrastructure.Services.SystemCurrentUserService` | `src/Infrastructure/Services/SystemCurrentUserService.cs` | 低〜中 | 固定値を返すスタブ実装だが契約を保証する価値はある |
| `Infrastructure.DependencyInjection`（AppSettings/ClockSettingsのnullガード部分） | `src/Infrastructure/DependencyInjection.cs` | 低 | 単純なDI登録を超えたnullガード例外ロジックがある場合のみ対象 |
| `Crosscutting.Logging.CorrelationContext` | `src/Crosscutting/Logging/CorrelationContext.cs` | 中 | `AsyncLocal`ベースの`GetOrCreate`/`Set`ロジック |
| `Crosscutting.Logging.FrameworkLoggingAdapter<T>` | `src/Crosscutting/Logging/FrameworkLoggingAdapter.cs` | 中 | `SetContextToGdc`の相関ID・時刻設定ロジック |
| `Crosscutting.Logging.NLogInitializer` | `src/Crosscutting/Logging/NLogInitializer.cs` | 低 | ファイル解決の分岐ロジック |
| `Crosscutting.Logging.NLogRegistrar` | `src/Crosscutting/Logging/NLogRegistrar.cs` | 低 | `Interlocked.CompareExchange`による排他制御ロジック |

**作業手順:** 優先度「中」以上のクラスから着手する。各クラスを `Read` して現在の実装を確認し、DB/ファイルI/O等の外部依存が無い部分（純粋ロジック）から単体テストを作成する。外部依存が強い部分は Phase 5（結合テスト）に回してよい。

配置先ドキュメント: `docs/Infrastructure/{Persistence,Repositories,Migrations,Services}/{ClassName}_単体テスト仕様書.md`（`docs/Infrastructure/` は `.slnx` の solution folder として既存）、`docs/Crosscutting/Logging/{ClassName}_単体テスト仕様書.md`（`docs/Crosscutting/` も既存folder）。

---

## 5. 検証方法

```bash
dotnet build --no-incremental
dotnet test
```

- 新規・修正した全テストが green であること（プレースホルダから実テストへの差し替え分は、差し替え前のプレースホルダが消えていることも確認）
- `PasswordHashService` のテストは特に入念に検証すること（セキュリティ影響が大きい）
- `Crosscutting.Tests` プロジェクトが実体を持つようになったこと

## 6. 本Phaseでやらないこと

- `FindEmployeeByADUseCase`（未実装のため対象外）
- Presentation層のE2Eテスト（`LoginDialogUITests.cs` 等）
- DB接続を要する部分の完全な検証（Phase 5の結合テストで対応）
