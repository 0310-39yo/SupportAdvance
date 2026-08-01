# CLEAN_ARCHITECTURE_GUIDELINES 修正計画書

**計画作成日:** 2026-08-01  
**対象:** CLEAN_ARCHITECTURE_GUIDELINES.md の曖昧性・ドキュメント不一致の修正  
**計画ID:** 20260801_CLEAN_ARCHITECTURE_FIX

---

## 1. 計画概要

### 目的
CLEAN_ARCHITECTURE_GUIDELINES.md とプロジェクト実装の齟齬を解消し、ドキュメントの正確性と実装との整合性を確保する。

### スコープ
- ドキュメント修正（3項目）
- 自動検証テスト導入（1項目）
- Application層ドキュメント強化（1項目）
- **実装コード修正:** なし（実装は既に準拠）

### 主要成果物
| # | 成果物 | 形式 | 対象 |
|---|---|---|---|
| 1 | CLEAN_ARCHITECTURE_GUIDELINES.md 修正版 | .md | ドキュメント |
| 2 | NetArchTest.Rules テストコード | .cs | テストプロジェクト |
| 3 | Application層ドキュメント補足 | .md | ドキュメント |

---

## 2. 修正アイテム詳細

### 【高優先度 - フェーズ1】

#### 修正項目 1: ドキュメント 318行の Infrastructure → Application 参照を明確化

**現状（ドキュメント 318行）:**
```
- Application***（インターフェース実装パターンのみ）
```

**問題点:**
- 汎用 Infrastructure vs Context別 Infrastructure の区別がない
- 実装では Context別 Infrastructure のみが Application を参照しており、汎用層は参照していない
- 読者がドキュメント例を実装する際に混乱する可能性

**修正案:**
```markdown
- Application***（インターフェース実装パターンのみ）
  * **重要:** 汎用 Infrastructure プロジェクト自体は稀にのみ Application を参照
  * 通常は **Bounded Context別 Infrastructure**（例：CarPreferences.Infrastructure）が
    Context固有のインターフェース実装を担当
  * 参考：CarPreferences.Infrastructure.csproj が Application を参照している実装例を確認のこと
```

**修正対象ファイル:**
- `docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md`（318行周辺）
- ついで参照：410行（依存マトリックス備考欄）も同様に明記

**実装工数:** 15分  
**検証方法:** ドキュメント レビュー

**優先度:** 🔴 **高**（複数の読者が実装パターンを誤解する可能性）

---

#### 修正項目 2: ドキュメント 327行のコード例に実装位置を明記

**現状（ドキュメント 327行の CarPreferenceRepository 例）:**
```csharp
namespace SupportAdvance.Infrastructure.Data.Repositories;

public class CarPreferenceRepository : ICarPreferenceRepository
{
    // ...
}
```

**問題点:**
- 名前空間が `SupportAdvance.Infrastructure` （汎用層）のように見える
- 実装では該当クラスは存在せず、ドキュメント例のみ
- 新規 Context 追加時の参考になるコード例が不明確

**修正案:**
```csharp
// ✓ OK：Bounded Context別 Infrastructure での実装例
// ファイル: src/Contexts/Samples/CarPreferences.Infrastructure/Repositories/CarPreferenceRepository.cs
// 所属: Bounded Context別 Infrastructure（汎用 Infrastructure ではない）

namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Repositories;

public class CarPreferenceRepository : ICarPreferenceRepository
{
    private readonly IDbConnection _connection;

    public async Task<Car> GetByIdAsync(CarId id)
    {
        var result = await _connection.QuerySingleOrDefaultAsync<CarDto>(
            "SELECT * FROM cars WHERE id = @Id",
            new { Id = id.Value });

        return result?.ToDomain();
    }
}
```

**修正対象ファイル:**
- `docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md`（327-339行）

**実装工数:** 10分  
**検証方法:** コード例の正確性確認

**優先度:** 🟡 **中**（新規 Context 追加時のリファレンス価値向上）

---

### 【高優先度 - フェーズ2】

#### 修正項目 3: NetArchTest.Rules による自動検証テストの実装

**現状:**
- ドキュメント 720-794行で検証テストコードが記載されているが、実装されていない
- 依存関係の遵守は**コードレビュー（目視）のみ**に依存
- 特に「Program.cs のみ Infrastructure 参照可」という複雑なルールの検証が手動

**実装内容:**

##### 3.1 テストプロジェクトの準備
```
tests/
└── SupportAdvance.Architecture.Tests/（新規作成）
    ├── SupportAdvance.Architecture.Tests.csproj
    ├── DependencyRuleTests.cs
    └── LayerResponsibilityTests.cs
```

**必要な NuGet パッケージ:**
```xml
<PackageReference Include="NetArchTest.Rules" Version="1.4.2" />
<PackageReference Include="xunit" Version="2.6.4" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
```

##### 3.2 実装するテストケース（ドキュメント 733-793行を参考に）

**テスト1: Domain層のアーキテクチャ検証**
```csharp
[Fact]
public void Domain_Should_Not_DependOn_ApplicationOrInfrastructure()
{
    var result = Types.InAssembly(typeof(CarPreferences.Domain.ValueObjects.CarModel).Assembly)
        .ShouldNot()
        .HaveDependencyOnAny(
            "SupportAdvance.Application",
            "SupportAdvance.Contexts.Samples.CarPreferences.Application",
            "SupportAdvance.Infrastructure",
            "SupportAdvance.Crosscutting")
        .GetResult();

    Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
}
```

**テスト2: Common層の依存ゼロ検証**
```csharp
[Fact]
public void Common_Should_Have_No_Dependencies()
{
    var result = Types.InAssembly(typeof(SupportAdvance.Common.Clocks.IClock).Assembly)
        .ShouldNot()
        .HaveDependencyOnAny(
            "SupportAdvance.SharedKernel",
            "SupportAdvance.Domain",
            "SupportAdvance.Application",
            "SupportAdvance.Infrastructure",
            "SupportAdvance.Crosscutting")
        .GetResult();

    Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
}
```

**テスト3: Crosscutting → Infrastructure 禁止検証**
```csharp
[Fact]
public void Crosscutting_Should_Not_DependOn_Infrastructure()
{
    var result = Types.InAssembly(typeof(SupportAdvance.Crosscutting.Logging.IAppLogging<>).Assembly)
        .ShouldNot()
        .HaveDependencyOn("SupportAdvance.Infrastructure")
        .GetResult();

    Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
}
```

**テスト4: Presentation.WinTrial の Program.cs のみ Infrastructure 参照可**
```csharp
[Fact]
public void WinTrial_Only_Program_May_DependOn_Infrastructure()
{
    var result = Types.InAssembly(typeof(SupportAdvance.Presentation.WinTrial.Program).Assembly)
        .That()
        .DoNotHaveName("Program")
        .ShouldNot()
        .HaveDependencyOnAny(
            "SupportAdvance.Infrastructure",
            "SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure")
        .GetResult();

    Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
}
```

**テスト5: Application層の逆方向依存禁止**
```csharp
[Fact]
public void Application_Should_Not_Depend_On_InfrastructureOrPresentation()
{
    var result = Types.InAssembly(typeof(SupportAdvance.Application.IUseCase).Assembly)
        .ShouldNot()
        .HaveDependencyOnAny(
            "SupportAdvance.Infrastructure",
            "SupportAdvance.Presentation")
        .GetResult();

    Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
}
```

##### 3.3 実行方法
```bash
# テスト実行
dotnet test tests/SupportAdvance.Architecture.Tests/SupportAdvance.Architecture.Tests.csproj

# CI/CDに組み込む（GitHub Actions 例）
# .github/workflows/architecture-validation.yml
name: Architecture Validation
on: [push, pull_request]
jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0'
      - run: dotnet test tests/SupportAdvance.Architecture.Tests/
```

**修正対象ファイル:**
- 新規作成：`tests/SupportAdvance.Architecture.Tests/SupportAdvance.Architecture.Tests.csproj`
- 新規作成：`tests/SupportAdvance.Architecture.Tests/DependencyRuleTests.cs`

**実装工数:** 2-3時間（テストコード記述 + CI設定）  
**検証方法:** `dotnet test` で全テスト合格確認

**優先度:** 🔴 **高**（今後の保守性向上に必須）

**依存関係:** なし（独立実装可能）

---

### 【中優先度 - フェーズ3】

#### 修正項目 4: Application層ドキュメント（227-242行）の明確化

**現状（ドキュメント 227-242行）:**
```markdown
### 5. **Application Layer** - Use Cases / Application Services

**プロジェクト:**
- `src/Application` - 汎用アプリケーションサービス（基盤インターフェース）
- `src/Contexts/Samples/CarPreferences.Application` - Use Cases（Bounded Context実装）

**汎用 Application層 vs Bounded Context別 Application層:**
...
```

**改善点:**
- ファイルパス例の追加
- 実装パターン図の追加
- 依存関係の具体例

**修正案（追加セクション）:**

```markdown
### 5. **Application Layer** - Use Cases / Application Services

#### ファイル構造例

**汎用層:**
```
src/Application/
├── Application.csproj
├── IUseCase.cs          # インターフェースのみ
├── IRequest.cs
├── IResponse.cs
└── Abstractions/        # 共有抽象化のみ
```

**Context別層:**
```
src/Contexts/Samples/CarPreferences.Application/
├── CarPreferences.Application.csproj
├── UseCases/
│   └── UpdateCarPreferenceUseCase.cs  # IUseCase 実装
├── Commands/
│   └── UpdateCarPreferenceCommand.cs  # IRequest 実装
└── DTOs/
    └── UpdateCarPreferenceDto.cs      # IResponse 実装
```

#### 参照フロー図

```
汎用Application: IUseCase（インターフェース定義）
         ▲
         │ implements
         │
Context別Application: UpdateCarPreferenceUseCase（実装）
         │
         ├─ depends on ──→ Domain: Car（ビジネスロジック）
         ├─ depends on ──→ Crosscutting: IAppLogging（ロギング）
         └─ depends on ──→ Infrastructure（DI で注入）※直接参照なし
```

#### 実装パターンの選択

**パターンA: Repository インターフェースを汎用Application に定義（推奨）**
```csharp
// src/Application/Repositories/ICarPreferenceRepository.cs
namespace SupportAdvance.Application.Repositories;
public interface ICarPreferenceRepository
{
    Task<Car> GetByIdAsync(CarId id);
}

// src/Contexts/Samples/CarPreferences.Infrastructure/
// CarPreferences.Infrastructure が ICarPreferenceRepository を実装
```

**パターンB: Repository インターフェースを Context別Applicationに定義**
```csharp
// src/Contexts/Samples/CarPreferences.Application/Repositories/ICarPreferenceRepository.cs
namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.Repositories;
public interface ICarPreferenceRepository
{
    Task<Car> GetByIdAsync(CarId id);
}

// src/Contexts/Samples/CarPreferences.Infrastructure が実装
```

現在の実装はパターンB（Context別Application が Application を参照）。
各 Context で Repository インターフェースを定義し、その Infrastructure が実装。

---

**修正対象ファイル:**
- `docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md`（227-267行周辺に追加）

**実装工数:** 1時間（図作成含む）  
**検証方法:** ドキュメント レビュー + 新規Context参加者の理解確認

**優先度:** 🟡 **中**（新規 Context 追加時の参考資料価値）

---

## 3. 実装スケジュール

### タイムライン

| フェーズ | 修正項目 | 予定期間 | 工数 | 状態 |
|---|---|---|---|---|
| **Phase 1** | アイテム1, 2（ドキュメント修正） | 2026-08-01 ～ 08-02 | 25分 | 📅 計画中 |
| **Phase 2** | アイテム3（NetArchTest導入） | 2026-08-02 ～ 08-05 | 2.5h | 📅 計画中 |
| **Phase 3** | アイテム4（Application層ドキュメント強化） | 2026-08-05 ～ 08-07 | 1h | 📅 計画中 |
| **Validation** | 全項目検証・リグレッション確認 | 2026-08-07 ～ 08-08 | 1.5h | 📅 計画中 |

**総工数:** 約 5.5時間

### マイルストーン

- **M1（2026-08-02）**: ドキュメント修正完了（アイテム1, 2）
- **M2（2026-08-05）**: NetArchTest テスト実装完了（アイテム3）
- **M3（2026-08-07）**: Application層ドキュメント強化完了（アイテム4）
- **M4（2026-08-08）**: 全修正の統合テスト完了・リリース準備

---

## 4. 検証計画

### Phase 1 検証（ドキュメント修正）

```
- [ ] ドキュメント 318行修正内容をレビュー
- [ ] コード例 327行の正確性確認
- [ ] 関連箇所（410行等）の一貫性確認
- [ ] 日本語表現のチェック
```

### Phase 2 検証（NetArchTest導入）

```
- [ ] テストプロジェクト作成確認
- [ ] NuGet 参照追加確認
- [ ] dotnet test で全テスト実行
- [ ] 全テスト合格確認
  ├─ Domain_Should_Not_DependOn_* ✓
  ├─ Common_Should_Have_No_Dependencies ✓
  ├─ Crosscutting_Should_Not_DependOn_Infrastructure ✓
  ├─ WinTrial_Only_Program_May_DependOn_* ✓
  └─ Application_Should_Not_Depend_On_* ✓
```

### Phase 3 検証（Application層ドキュメント）

```
- [ ] ファイル構造図の正確性確認
- [ ] 参照フロー図の正確性確認
- [ ] コード例の正確性確認
- [ ] 新規Context参加者でのドキュメント理解度確認
```

### リグレッション検証

```
- [ ] 既存テスト全て合格
- [ ] ドキュメント全体の一貫性確認
- [ ] 依存関係マトリックスとの整合性確認
```

---

## 5. リスク・課題管理

### 想定リスク

| リスク | 影響度 | 対策 |
|---|---|---|
| NetArchTest の互換性問題 | 中 | 最新バージョンで検証、サポート確認 |
| テスト実行時の false positive | 中 | テストロジックの段階的検証 |
| ドキュメント修正後の漏れ | 低 | 修正リスト照合表で確認 |
| 新規Context追加時の参考例不足 | 低 | Phase 3 で詳細例を追加 |

### 課題

なし（現在）

---

## 6. 成功基準

| # | 基準 | 判定方法 |
|---|---|---|
| 1 | ドキュメント修正が完了・レビュー合格 | コード レビュー合格 |
| 2 | NetArchTest テスト全て合格 | `dotnet test` 実行結果 ✓ |
| 3 | 既存実装がテスト合格 | Phase 2 検証合格 |
| 4 | Application層ドキュメントが新規Context参加者に理解される | レビューコメント確認 |
| 5 | 依存関係ルール違反が機械的に検出可能 | CI/CDで自動実行確認 |

---

## 7. 関連ドキュメント

- `CLEAN_ARCHITECTURE_GUIDELINES実装齟齬レポート.md` — 本計画の根拠
- `アーキテクチャ検証_詳細分析.md` — 検証結果の詳細
- `CLAUDE.md`（ルート）— アーキテクチャ全体ガイド

---

## 8. 承認・署名

| 役割 | 名前 | 日付 | 承認 |
|---|---|---|---|
| 計画立案 | Claude | 2026-08-01 | ✓ |
| プロジェクト所有者 | （確認待ち） | - | ○ |
| アーキテクチャ責任者 | （確認待ち） | - | ○ |

---

**計画書作成日:** 2026-08-01  
**最終更新:** 2026-08-01  
**バージョン:** 1.0
