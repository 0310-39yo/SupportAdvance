# 新規プロジェクト追加時のチェックリスト

新しいプロジェクト（.csproj）をプロジェクトに追加する際に実施すべき確認事項のチェックリスト。

---

## 📋 プリチェック

### プロジェクト計画フェーズ

- [ ] プロジェクトが属するレイヤーを決定
  - [ ] SharedKernel / Common / Crosscutting / Domain / Application / Infrastructure / Presentation のいずれか
  - [ ] 理由を簡潔に記述（例：「カープリファレンスドメインロジック実装」）

- [ ] プロジェクト名の命名規則を確認
  - [ ] Bounded Context（コンテキスト）を含むか：`SupportAdvance.Contexts.Samples.CarPreferences.Domain`
  - [ ] レイヤー名を含むか：`.Domain` / `.Application` / `.Infrastructure`

- [ ] 既存プロジェクトとの関係を定義
  - [ ] 依存先プロジェクトをリストアップ
  - [ ] 依存関係マトリックスで許可されているか確認

---

## 🔧 プロジェクトファイル（.csproj）検証

### ProjectReference の確認

```bash
# プロジェクトファイルを確認
cat src/YourContext/YourProject/YourProject.csproj
```

**チェック項目:**

- [ ] `<PropertyGroup>` が正しく設定されている
  ```xml
  <TargetFramework>net10.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  ```

- [ ] `<ProjectReference>` が依存関係ルールに準拠
  
  **許可の例：**
  ```xml
  <!-- Application プロジェクトの場合 -->
  <ProjectReference Include="..\Domain\CarPreferences.Domain.csproj" />
  <ProjectReference Include="..\..\SharedKernel\SharedKernel.csproj" />
  <ProjectReference Include="..\..\Common\Common.csproj" />
  ```

  **禁止の例：**
  ```xml
  <!-- Domain が Application を参照 → ✗ NG -->
  <ProjectReference Include="..\Application\CarPreferences.Application.csproj" />
  ```

- [ ] 不要な参照がないか
  - [ ] 実際に using で使用されているか確認（下記参照）

- [ ] 循環参照がないか
  - [ ] A → B → A のような循環がないか

**検証コマンド:**
```bash
# 循環参照を検出
dotnet build --list-path --log-output-file build.log
grep -i "circular" build.log
```

---

## 📝 コードの using 宣言確認

### Namespace の検証

```bash
# using 宣言を列挙
grep "^using " src/YourContext/YourProject/**/*.cs
```

**チェック項目:**

- [ ] using 宣言が依存関係ルールに準拠
  
  **許可の例：**
  ```csharp
  // Infrastructure プロジェクト内
  using SupportAdvance.SharedKernel;
  using SupportAdvance.Common;
  using SupportAdvance.Crosscutting;
  using SupportAdvance.Contexts.Samples.CarPreferences.Domain;
  ```

  **禁止の例：**
  ```csharp
  // Infrastructure プロジェクト内で ✗ NG
  using SupportAdvance.Application;
  using SupportAdvance.Contexts.Samples.CarPreferences.Application;
  ```

- [ ] 逆方向依存がないか
  ```bash
  # Domain が Application を参照していないか確認
  grep -r "using.*Application" src/YourContext/YourProject/
  # マッチなし = OK ✓
  
  # Infrastructure が Application を参照していないか確認
  grep -r "using.*Application" src/Infrastructure/
  # マッチなし = OK ✓
  ```

- [ ] 不要な using がないか
  - [ ] 実装で実際に使用されているか確認
  - [ ] テストしながら削除確認

---

## 🏗️ アーキテクチャの検証

### ビルド検証

```bash
# インクリメンタルビルドをスキップして完全ビルド
cd D:\SupportAdvance
dotnet build --no-incremental

# エラーが表示されないこと
# 出力例：
# Build succeeded. 0 Warning(s)
```

**チェック項目:**

- [ ] ビルド成功（error が 0）
- [ ] 依存関係エラーがないか
- [ ] 警告レベルのみ（存在する場合）

### 自動解析ツール（オプション）

```bash
# FxCop / Code Analysis（VS統合）
dotnet format --verify-no-changes --verbosity diagnostic

# SlnArch（将来導入予定）
slnarch analyze --config architecture.json
```

---

## 🧪 機能テストの実施

### Unit Test プロジェクト作成（Domain / Application の場合）

- [ ] テストプロジェクトが正しく作成されているか
  ```
  tests/Contexts/YourContext/YourProject.Tests/
  ```

- [ ] テストプロジェクトの .csproj が正しい参照を持つか
  ```xml
  <ProjectReference Include="..\..\..\src\Contexts\YourContext\YourProject\YourProject.csproj" />
  <PackageReference Include="xunit" Version="2.x" />
  <PackageReference Include="Moq" Version="4.x" />
  ```

- [ ] テストが実行可能か
  ```bash
  dotnet test tests/Contexts/YourContext/YourProject.Tests/
  ```

### Integration Test（Infrastructure の場合）

- [ ] 実DB接続もしくはテスト用 in-memory DB でテスト可能か
- [ ] Repository 実装が正しく動作するか

---

## 📖 ドキュメント確認

### プロジェクトの README.md

- [ ] プロジェクト層ルートに `README.md` が存在するか
  
  ```
  src/YourContext/YourProject/README.md
  ```

- [ ] README に以下が含まれているか
  - [ ] プロジェクトの責務
  - [ ] 許可される依存関係
  - [ ] 禁止される依存関係
  - [ ] 実装ガイドライン

**テンプレート:**
```markdown
# [プロジェクト名]

## 責務
- ...

## 依存関係

**許可:**
- SharedKernel
- Common

**禁止:**
- Application
- Infrastructure

## 実装ガイドライン
- ...
```

### アーキテクチャドキュメント

- [ ] [docs/CLEAN_ARCHITECTURE_GUIDELINES.md](./CLEAN_ARCHITECTURE_GUIDELINES.md) に新規プロジェクトの情報を追加（必要に応じて）

---

## 👀 コードレビューポイント

### Domain / Application プロジェクト

- [ ] Domain Logic が外界への依存を持たないか
- [ ] Infrastructure への直接参照がないか
- [ ] DI コンテナの使用がないか（Application での Use Case 実装のみ）

### Infrastructure プロジェクト

- [ ] Application への参照がないか ✓ **修正済みパターン**
  ```bash
  grep -r "using.*Application" src/Infrastructure/
  ```

- [ ] Application が定義したインターフェースを実装しているか
  ```csharp
  // Application で定義
  public interface ICarRepository { ... }
  
  // Infrastructure で実装
  public class SqlCarRepository : ICarRepository { ... }
  ```

### Presentation プロジェクト

- [ ] ViewModels / Controllers が Infrastructure に直接依存していないか
  ```bash
  grep -r "using.*Infrastructure" src/Presentation/
  # マッチなし（Program.cs を除く）= OK ✓
  ```

- [ ] Program.cs で DI が適切に構成されているか

---

## ✅ 最終確認

### 全体チェック

- [ ] ビルド成功
  ```bash
  dotnet build
  # Build succeeded. 0 Warning(s)
  ```

- [ ] すべてのテスト成功（該当する場合）
  ```bash
  dotnet test
  # All tests passed
  ```

- [ ] Namespace が正しく構成されているか
  - [ ] レイヤー名が含まれている
  - [ ] Bounded Context が適切に分離されている

- [ ] チーム内でアーキテクチャ確認が完了
  - [ ] レビュアーによる承認取得

### CI/CD への統合

- [ ] 新規プロジェクトが CI/CD パイプラインに含まれるか
- [ ] ビルド・テスト・デプロイが成功するか

---

## 🚨 よくある間違い

### ❌ 間違い1: 不要な参照を削除忘れ

**症状:** プロジェクトが Application を参照しているが、コード上では使用されていない

**確認:**
```bash
grep -r "using SupportAdvance.Application" src/YourProject/
```

**対策:** ProjectReference から削除

---

### ❌ 間違い2: 循環参照

**症状:** ビルドエラー「circular dependency」

**確認:**
```bash
# .csproj を視覚化
dotnet list reference
```

**対策:** 依存関係を整理

---

### ❌ 間違い3: Presentation から Infrastructure への直接依存

**症状:** ViewModel / Controller で Infrastructure 型を使用

```csharp
// ✗ NG
public class UserViewModel
{
    private SqlUserRepository _repo = new();
}
```

**対策:**
```csharp
// ✓ OK
public class UserViewModel
{
    private readonly IGetUsersUseCase _useCase;
    public UserViewModel(IGetUsersUseCase useCase) => _useCase = useCase;
}
```

---

## 📝 チェックリスト出力例

```
プロジェクト: SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure

✅ レイヤー決定: Infrastructure
✅ ProjectReference 確認: Domain, Infrastructure のみ
✅ using 宣言確認: Application なし
✅ ビルド成功
✅ 全テスト成功
✅ README.md 作成
✅ コードレビュー完了

→ プロジェクト追加 OK ✓
```

---

## 📚 参考資料

- [CLEAN_ARCHITECTURE_GUIDELINES.md](./CLEAN_ARCHITECTURE_GUIDELINES.md)
- [.claude/CLAUDE.md](../.claude/CLAUDE.md)
- 各レイヤーの `src/*/CLAUDE.md`

