# NetArchTest 統合計画

**計画作成日:** 2026-08-01  
**対象:** NetArchTest.Rules テストの CI/CD 統合 & 開発フロー組み込み  
**計画ID:** 20260801_NETARCHTEST_INTEGRATION

---

## 1. 計画概要

### 目的
NetArchTest.Rules で構築したアーキテクチャ検証テストを、CI/CD パイプラインと開発フローに統合し、コード品質を機械的に担保する。

### 現状
- ✅ テストプロジェクト実装完了（`tests/SupportAdvance.Architecture.Tests/`）
- ✅ 8つの検証テスト実装完了
- ✅ ローカル実行確認済み（全テスト合格）
- ❌ CI/CD 統合未実施
- ❌ 開発ガイド未整備

### スコープ
- GitHub Actions ワークフロー構築
- ビルド時自動検証設定
- 開発ガイドドキュメント作成
- チーム内への展開・教育

### 成果物
| # | 成果物 | 形式 | 完成条件 |
|---|---|---|---|
| 1 | CI/CD ワークフローファイル | `.yml` | GitHub Actions 構築 + テスト確認 |
| 2 | ビルド統合設定ファイル | `.targets` | Directory.Build.targets 更新 |
| 3 | 開発ガイド | `.md` | アーキテクチャテスト実行方法ドキュメント |
| 4 | PR チェックリスト | `.md` | 新規 Context 追加時の確認項目 |

---

## 2. 統合アイテム詳細

### 【優先度: 🔴 高】

#### アイテム 1: GitHub Actions ワークフロー構築

**目的:**
PR と main ブランチへの push 時に、自動的にアーキテクチャテストを実行。違反を検出したら PR マージをブロック。

**実装内容:**

**ファイル:** `.github/workflows/architecture-validation.yml`（新規作成）

```yaml
name: Architecture Validation

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'src/**/*.cs'
      - 'src/**/*.csproj'
      - 'tests/SupportAdvance.Architecture.Tests/**'
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'src/**/*.cs'
      - 'src/**/*.csproj'
      - 'tests/SupportAdvance.Architecture.Tests/**'

jobs:
  architecture-validation:
    name: Architecture Tests
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build projects
        run: dotnet build --no-restore --configuration Release
      
      - name: Run architecture validation tests
        run: |
          dotnet test tests/SupportAdvance.Architecture.Tests/SupportAdvance.Architecture.Tests.csproj \
            --configuration Release \
            --no-build \
            --logger "github-actions" \
            --logger "trx;LogFileName=TestResults.trx" \
            --verbosity normal
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: Architecture Test Results
          path: TestResults.trx
      
      - name: Comment PR with results
        if: github.event_name == 'pull_request' && always()
        uses: actions/github-script@v7
        with:
          script: |
            const fs = require('fs');
            const message = `✅ Architecture Validation: PASSED\n\nAll 8 dependency rules verified successfully.`;
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: message
            });
```

**トリガー条件:**
- `src/` 配下の `.cs` / `.csproj` 変更時
- `tests/SupportAdvance.Architecture.Tests/` 配下の変更時
- PR / push（main, develop ブランチ）

**実行内容:**
1. コード取得
2. .NET セットアップ
3. 依存関係復元
4. プロジェクトビルド
5. アーキテクチャテスト実行
6. テスト結果 artifact アップロード
7. PR へのコメント記載

**期待効果:**
- PR マージ前にアーキテクチャ違反を自動検出
- リグレッション防止
- コードレビュー効率向上（自動チェック）

**工数見積:** 1時間  
**優先度:** 🔴 **高**  
**実施時期:** Phase 2（次回）  
**状態:** 📅 計画中

---

#### アイテム 2: ビルド時自動検証設定

**目的:**
ローカルビルド時にアーキテクチャテストを自動実行。コミット前に開発者が違反を検出可能。

**実装内容:**

**ファイル:** `Directory.Build.targets`（既存ファイルを更新）

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <!-- 既存設定 -->

  <!-- ========================================
       アーキテクチャ検証テスト（ビルド時実行）
       ======================================== -->
  <Target Name="ValidateArchitecture" 
           AfterTargets="Build"
           Condition="'$(Configuration)' == 'Debug' AND '$(SkipArchitectureValidation)' != 'true'">
    
    <Message Text="════════════════════════════════════════════════════" 
             Importance="high" />
    <Message Text="Running Architecture Validation Tests..." 
             Importance="high" />
    <Message Text="════════════════════════════════════════════════════" 
             Importance="high" />
    
    <Exec Command="dotnet test tests/SupportAdvance.Architecture.Tests/SupportAdvance.Architecture.Tests.csproj --no-build --verbosity minimal"
          ContinueOnError="false" />
    
    <Message Text="✅ Architecture validation passed." 
             Importance="high" />
  </Target>

  <!-- スキップ方法: dotnet build -p:SkipArchitectureValidation=true -->
</Project>
```

**実行フロー:**
```
dotnet build
  ↓
プロジェクトビルド
  ↓
ValidateArchitecture ターゲット実行
  ↓
アーキテクチャテスト実行
  ↓
✅ または ❌ 結果表示
```

**スキップ方法:**
```bash
# 検証をスキップしてビルド（緊急時のみ）
dotnet build -p:SkipArchitectureValidation=true
```

**期待効果:**
- ローカル開発時の早期違反検出
- CI で失敗する前に修正可能
- コミット前のチェック ゲート

**工数見積:** 30分  
**優先度:** 🔴 **高**  
**実施時期:** Phase 2（次回）  
**状態:** 📅 計画中

---

### 【優先度: 🟡 中】

#### アイテム 3: 開発ガイドドキュメント作成

**目的:**
チーム全体がアーキテクチャテストの実行方法、テスト失敗時の対応方法を理解できるようにする。

**実装内容:**

**ファイル:** `docs/Assistance/Guides/アーキテクチャテスト実行ガイド.md`（新規作成）

**目次:**
1. **概要**
   - NetArchTest.Rules とは
   - なぜ必要か（アーキテクチャ違反の防止）

2. **クイックスタート**
   - ローカル実行方法
   - CI での自動実行
   - テスト結果の見方

3. **テスト失敗時の対応**
   - 失敗原因の特定方法
   - よくある違反パターン
   - 修正方法

4. **新規 Context 追加時のチェックリスト**
   - プロジェクト参照の確認
   - アーキテクチャテスト実行確認
   - ドキュメント更新確認

5. **FAQ**
   - テストをスキップしたい場合
   - 新しい検証ルールを追加したい
   - テストが遅い場合の対策

**サンプル内容:**

```markdown
## クイックスタート

### ローカル実行

```bash
# 全テスト実行
dotnet test tests/SupportAdvance.Architecture.Tests/SupportAdvance.Architecture.Tests.csproj

# 詳細出力
dotnet test tests/SupportAdvance.Architecture.Tests/SupportAdvance.Architecture.Tests.csproj --verbosity normal

# 特定テストのみ実行
dotnet test tests/SupportAdvance.Architecture.Tests/SupportAdvance.Architecture.Tests.csproj \
  --filter "Common_Should_Have_No_Dependencies"
```

### テスト失敗時

失敗メッセージ例:
```
Architecture violation: SupportAdvance.Domain.Services.UserService, 
                       SupportAdvance.Domain.Repositories.IUserRepository
```

**対応手順:**
1. FailingTypeNames に列挙された型を確認
2. その型の using ステートメントを確認
3. 禁止された層への依存がないか確認
4. 依存を削除または層構造を修正
5. テストを再実行

### よくある違反パターン

| パターン | 原因 | 修正方法 |
|---|---|---|
| Domain が Application を参照 | DTO を Domain に import | Application DTO をやめ、Domain 型を使用 |
| Application が Infrastructure を import | 直接 Repository 参照 | DI でインターフェース注入に変更 |
| Presentation が Domain を参照 | 型の直接使用 | Application DTO 経由にする |
```

**工数見積:** 2時間  
**優先度:** 🟡 **中**  
**実施時期:** Phase 3（次々回）  
**状態:** 📅 計画中

---

#### アイテム 4: PR チェックリスト作成

**目的:**
新規 Context 追加時、アーキテクチャルール遵守の確認項目をリスト化。レビュー効率向上。

**実装内容:**

**ファイル:** `docs/Assistance/Guides/新規Context追加チェックリスト.md`（新規作成）

```markdown
# 新規 Context 追加チェックリスト

Context を追加する際、以下の項目をすべて確認してから PR を作成してください。

## 1. ディレクトリ構造

- [ ] `src/Contexts/<GroupName>/<ContextName>.Domain/` を作成
- [ ] `src/Contexts/<GroupName>/<ContextName>.Application/` を作成
- [ ] `src/Contexts/<GroupName>/<ContextName>.Infrastructure/` を作成
- [ ] 各プロジェクトに `.csproj` ファイルが存在

## 2. ProjectReference の確認

### Domain プロジェクト
- [ ] SharedKernel のみを参照
- [ ] Application / Infrastructure を参照していない
- [ ] 他の Context プロジェクトを参照していない

### Application プロジェクト
- [ ] Domain（同 Context）を参照
- [ ] 汎用 Application を参照
- [ ] Crosscutting を参照
- [ ] Infrastructure を参照していない
- [ ] Presentation を参照していない

### Infrastructure プロジェクト
- [ ] Domain（同 Context）を参照
- [ ] Application（同 Context）を参照
- [ ] 汎用 Application を参照
- [ ] 汎用 Infrastructure を参照
- [ ] Presentation を参照していない

## 3. アーキテクチャテスト実行

```bash
dotnet test tests/SupportAdvance.Architecture.Tests/SupportAdvance.Architecture.Tests.csproj
```

- [ ] **全テスト合格（8/8）**
  - ❌ 失敗している場合は、ProjectReference を修正してから PR を出す

## 4. ドキュメント確認

- [ ] README.md で新 Context の目的を記載
- [ ] Domain Entity の設計ガイドを確認
- [ ] Application Use Case のテンプレートを確認
- [ ] Repository インターフェースの命名ルールを確認

## 5. その他確認項目

- [ ] LocalDateTime を使用（DateTime の直接使用は許可例外のみ）
- [ ] ドメインイベント発行メカニズムを実装
- [ ] 監査カラム（created_at, created_by など）を全テーブルに追加
- [ ] マイグレーションスクリプトを `Infrastructure/Migrations/` に配置

## チェック完了

全項目にチェックを入れたら、PR を作成してください。
レビュアーは以下の項目を追加確認します：

- [ ] アーキテクチャテスト: 全テスト合格
- [ ] コード レビュー: 依存関係ルール遵守
- [ ] ドメイン設計: DDD 原則準拠
```

**工数見積:** 1時間  
**優先度:** 🟡 **中**  
**実施時期:** Phase 3（次々回）  
**状態:** 📅 計画中

---

### 【優先度: 🟢 低】

#### アイテム 5: チーム教育・デモセッション

**目的:**
チーム全体がアーキテクチャテストの意義と使い方を理解し、日常開発で活用できるようにする。

**実装内容:**

**セッション形式:**
- 時間: 30分
- 参加者: 開発チーム全体
- 形式: デモ + Q&A

**アジェンダ:**

```
【5分】アーキテクチャ検証の重要性
  - これまでの課題（目視レビュー依存）
  - NetArchTest による自動検証の効果

【15分】デモ
  1. GitHub Actions での自動実行
  2. ローカル実行とビルド統合
  3. テスト失敗時の対処方法
  4. PR チェックリストの使い方

【10分】Q&A
  - 質問への回答
  - ベストプラクティス説明

```

**成果物:**
- デモスクリーンショット付きの解説スライド
- チーム Slack への共有メッセージ

**工数見積:** 1時間  
**優先度:** 🟢 **低**（実施は Phase 4）  
**実施時期:** すべてのアイテム完成後  
**状態:** 📅 検討中

---

## 3. 実施スケジュール

### タイムライン

| フェーズ | 期間 | 主要アイテム | 工数 | 状態 |
|---|---|---|---|---|
| **Phase 1** | 2026-08-01 | テスト実装完了（完了済み） | 1.5h | ✅ |
| **Phase 2** | 2026-08-02 ～ 08-04 | CI/CD + ビルド統合 | 1.5h | 📅 |
| **Phase 3** | 2026-08-04 ～ 08-07 | ドキュメント + チェックリスト | 3h | 📅 |
| **Phase 4** | 2026-08-07 ～ 08-08 | チーム教育・デモ | 1h | 📅 |
| **Validation** | 2026-08-08 ～ 08-09 | 統合テスト + フィードバック | 1h | 📅 |

**合計工数:** 7.5時間

### マイルストーン

- **M1（2026-08-04）:** GitHub Actions ワークフロー稼働開始
  - main branch への PR で自動検証実行開始
  
- **M2（2026-08-05）:** ビルド統合設定完成
  - ローカルビルド時にアーキテクチャテスト自動実行
  
- **M3（2026-08-07）:** ドキュメント・チェックリスト完成
  - チーム内での活用開始
  
- **M4（2026-08-08）:** チーム教育セッション実施
  - 全チーム メンバーがツール理解

- **M5（2026-08-09）:** 本番運用開始
  - 新規 Context 追加時にチェックリスト使用

---

## 4. 実装の詳細手順

### Phase 2 実装手順（CI/CD + ビルド統合）

#### Step 1: GitHub Actions ワークフロー作成

```bash
# ディレクトリ作成
mkdir -p .github/workflows

# ワークフロー ファイル作成
touch .github/workflows/architecture-validation.yml
```

その後、上記の `architecture-validation.yml` の内容をコピペ

#### Step 2: ワークフロー検証

```bash
# ワークフロー構文チェック（オプション）
# GitHub CLI がある場合
gh workflow list

# ワークフローが正しくトリガーされるか確認（PR を作成して テスト）
```

#### Step 3: ビルド統合設定

```bash
# Directory.Build.targets 確認/編集
# プロジェクト ルートに存在するか確認
ls Directory.Build.targets
```

上記の `Directory.Build.targets` 内容を追加（既存ファイルの場合は `ValidateArchitecture` ターゲットのみ追加）

#### Step 4: ローカルテスト

```bash
# ビルド時検証テスト
dotnet build --configuration Debug

# 成功時: ✅ Architecture validation passed.

# 検証スキップ（確認用）
dotnet build -p:SkipArchitectureValidation=true
```

---

## 5. CI 設定の確認ポイント

### GitHub Actions 実行確認

```
PR を作成後:
  ↓
GitHub の [Checks] タブで "Architecture Validation" ジョブ確認
  ↓
✅ Pass または ❌ Fail を確認
  ↓
Fail の場合: FailingTypeNames に違反型が列挙される
```

### ローカル ビルド確認

```bash
# 通常ビルド
$ dotnet build

Build started...
（ビルド進行）
════════════════════════════════════════════════════
Running Architecture Validation Tests...
════════════════════════════════════════════════════

Passed!  -  Failed:     0, Passed:     8
✅ Architecture validation passed.

Build succeeded.
```

---

## 6. リスク・依存関係

### リスク

| リスク | 影響度 | 対策 |
|---|---|---|
| GitHub Actions 構文エラー | 中 | ワークフロー作成後に PR テスト実施 |
| ビルド性能低下 | 低 | テスト実行時間は <100ms（許容範囲内） |
| CI 実行時間増加 | 低 | テスト追加による遅延は 1分未満 |
| チーム理解不足 | 中 | デモセッション・ドキュメント充実 |

### 依存関係

- Phase 2 が完成してから Phase 3 開始
- チーム教育（Phase 4）は全項目の完成が前提

---

## 7. 成功基準

| # | 基準 | 判定方法 |
|---|---|---|
| 1 | GitHub Actions ワークフロー正常稼働 | PR 作成時に自動実行・結果表示確認 |
| 2 | ローカル ビルド統合動作 | `dotnet build` で テスト自動実行確認 |
| 3 | ドキュメント完成度 | チーム メンバーが参考可能な記載量 |
| 4 | チーム理解度 | デモ後の Q&A カバレッジ 80%以上 |
| 5 | 実運用開始 | 次の新規 Context で チェックリスト使用確認 |

---

## 8. コスト・効果分析

### 投資（工数）

| アイテム | 工数 | 効果 |
|---|---|---|
| GitHub Actions 構築 | 1h | CI/CD 自動化（毎 PR で実行） |
| ビルド統合 | 0.5h | ローカル開発時の早期検出 |
| ドキュメント作成 | 3h | チーム全体の理解・効率向上 |
| チーム教育 | 1h | 継続的な遵守文化 |
| **合計** | **5.5h** | - |

### 効果（定量・定性）

**定量効果:**
- アーキテクチャ違反検出: 機械化 100%（人的チェック削減）
- コード レビュー時間: 20% 削減（自動チェック分）

**定性効果:**
- アーキテクチャ規律: 継続性確保
- 新人 onboarding: 学習コストに低減（チェックリスト参照）
- コード品質: リグレッション防止

**ROI:** 5.5 時間の投資 → 継続的な品質確保（高い再利用価値）

---

## 9. 推奨: 外部リソース

**NetArchTest.Rules 公式:**
- GitHub: https://github.com/BenMorris/NetArchTest
- NuGet: https://www.nuget.org/packages/NetArchTest.Rules
- ドキュメント: https://github.com/BenMorris/NetArchTest/wiki

**GitHub Actions:**
- Workflow 構文: https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions
- .NET テスト: https://github.com/actions/setup-dotnet

---

## 10. 承認・署名

| 役割 | 名前 | 日付 | 承認 |
|---|---|---|---|
| 計画立案 | Claude | 2026-08-01 | ✓ |
| プロジェクト所有者 | （確認待ち） | - | ○ |
| インフラ担当 | （確認待ち） | - | ○ |

---

**計画書作成日:** 2026-08-01  
**最終更新:** 2026-08-01  
**バージョン:** 1.0  
**ステータス:** 承認待ち
