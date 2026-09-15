# Identity BC を Authentication BC へ名称変更 実装計画

**作成日**: 2026-09-16
**作成者**: Claude + User (tyokkoto@hotmail.com)
**方式**: インプレース改名（既存BCを直接リネーム。コピー＆削除方式は不採用）
**IDE/ツール**: Visual Studio 2026 + ReSharper

---

## 背景・方針決定の経緯

- Identity BC は「認証（Authentication）に特化したBC」として既に設計・実装済み（[Authentication_BC_設計ガイド.md](../Guides/Authentication_BC_設計ガイド.md)）。名称を実態に合わせて `Authentication` に変更する。
- 事前調査の結果、Identity BC は下記の理由から **インプレース改名** を採用した:
  - ソース約25ファイル＋テスト9ファイルと規模が小さい
  - 外部参照は `WinTrial/Program.cs` と `WinTrial/ViewModels/LoginDialogViewModel.cs` の2ファイルのみ
  - `.sln`（`.slnx`）は存在するが1ファイルのみで、参照修正範囲は限定的
  - DBテーブル名（`t_user_auth_sessions` / `m_login_credentials`）に "Identity" を含まず、マイグレーション改名は不要
  - 他BCから `IQueryService<UserAuthSession, ...>` 等での参照もなし
  - コピー＆削除方式は二重実装のメンテナンスコストとDI二重登録などのミスの余地が増えるだけで、この規模ではメリットが薄い

## 現状棚卸し

| 種別                   | 内容                                                                                                                                                                     |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| プロジェクト（src）    | `src/Contexts/Identity/{Identity.Domain, Identity.Application, Identity.Infrastructure}`                                                                                 |
| プロジェクト（tests）  | `tests/Contexts/{Identity.Domain.Tests, Identity.Application.Tests, Identity.Infrastructure.Tests}`（`/tests/Contexts/` 直下、Identity専用ソリューションフォルダはなし） |
| ソリューションフォルダ | `SupportAdvance.slnx` 内 `Folder Name="/src/Contexts/Identity/"`                                                                                                         |
| 名前空間               | `SupportAdvance.Contexts.Identity.{Domain,Application,Infrastructure}`（対象25ファイル）                                                                                 |
| DI拡張メソッド         | `AddIdentityApplicationModels()` / `AddIdentityInfrastructureModels()`                                                                                                   |
| 外部参照元             | `WinTrial/Program.cs`、`WinTrial/ViewModels/LoginDialogViewModel.cs`、`WinTrial.csproj`（ProjectReference×2）                                                            |
| DB                     | テーブル名に "Identity" なし → 変更不要                                                                                                                                  |
| ドキュメント           | `docs/Assistance/Guides/Identity_BC_設計ガイド.md`、`src/Contexts/CLAUDE.md`（"Auth/Identity.*" という実態と食い違う記述あり）                                           |
| メモリ                 | `identity_bc_design_decision.md` + `MEMORY.md` 該当行                                                                                                                    |

**ReSharperの限界**: ReSharperは名前空間の一括リネーム・シンボル（メソッド名等）のリネームは得意だが、プロジェクトの物理フォルダ移動や `.csproj` ファイル名変更そのものはReSharperの機能範囲外（VS標準のプロジェクトリネームでも物理フォルダは追従しない）。そのため物理リネームとコード上のリネームを2フェーズに分ける。

---

## Phase 1: 物理リネーム（Claude・`git mv`）

VSを閉じた状態で実施（ファイルロック回避のため）。

- `git mv` で以下をリネーム
  - `src/Contexts/Identity/` → `src/Contexts/Authentication/`
  - `Identity.Domain` → `Authentication.Domain`（フォルダ＋`.csproj`）
  - `Identity.Application` → `Authentication.Application`（フォルダ＋`.csproj`）
  - `Identity.Infrastructure` → `Authentication.Infrastructure`（フォルダ＋`.csproj`）
  - `tests/Contexts/Identity.Domain.Tests` → `tests/Contexts/Authentication.Domain.Tests`（フォルダ＋`.csproj`）
  - `tests/Contexts/Identity.Application.Tests` → `tests/Contexts/Authentication.Application.Tests`（フォルダ＋`.csproj`）
  - `tests/Contexts/Identity.Infrastructure.Tests` → `tests/Contexts/Authentication.Infrastructure.Tests`（フォルダ＋`.csproj`）
- パス修正
  - `SupportAdvance.slnx`：`Folder Name="/src/Contexts/Identity/"` → `.../Authentication/`、および該当する `Project Path` 全件（src 3件、tests 3件）
  - `Authentication.Application.csproj` 内 `ProjectReference`（→ Authentication.Domain）
  - `Authentication.Infrastructure.csproj` 内 `ProjectReference`（→ Authentication.Application, Authentication.Domain）
  - 3つのテスト `.csproj` 内 `ProjectReference`
  - `WinTrial.csproj` の `ProjectReference` ×2
- この時点では名前空間・DIメソッド名は "Identity" のままでよい（ビルドが通る状態を維持）

## Phase 2: 名前空間・シンボルのリネーム（ユーザー・Visual Studio + ReSharper）

1. VSでソリューションを再オープン
2. ReSharperの「Rename」リファクタリング（名前空間ノードを選択 → Refactor → Rename）で以下を変更
   - `SupportAdvance.Contexts.Identity.Domain` → `...Authentication.Domain`
   - `SupportAdvance.Contexts.Identity.Application` → `...Authentication.Application`
   - `SupportAdvance.Contexts.Identity.Infrastructure` → `...Authentication.Infrastructure`
     （solution全体の `using` 文・完全修飾参照が自動追従）
3. Renameダイアログの「コメント・文字列リテラルも検索して置換」オプションを有効化し、日本語コメント（例:「Identity Context を登録」）も併せて更新
4. シンボルRename（Shift+F6）
   - `AddIdentityApplicationModels` → `AddAuthenticationApplicationModels`
   - `AddIdentityInfrastructureModels` → `AddAuthenticationInfrastructureModels`
     （呼び出し元 `Program.cs` も自動更新）
5. 全プロジェクトの `bin`/`obj` を削除してクリーンビルド

## Phase 3: ドキュメント・メモリ更新（Claude）

- `docs/Assistance/Guides/Identity_BC_設計ガイド.md` → `Authentication_BC_設計ガイド.md`（ファイル名＋本文の "Identity BC" → "Authentication BC"）
- `src/Contexts/CLAUDE.md` の "Auth/Identity.*" という古い記述を実態（`Contexts/Authentication/Authentication.*`、Employee/Departmentと同じくグルーピングなしのフラット構成）に合わせて修正
- grep でヒットした残り（`docs/Assistance/DraftIdeas/認証について.md` 等）を個別確認し、BC名としての言及のみ修正（DDD用語としての一般的な "identity" は対象外）
- 記憶ファイル `identity_bc_design_decision.md` → `authentication_bc_design_decision.md` にリネーム＋内容更新、`MEMORY.md` 該当行更新

## 実施ログ

### Phase 1（完了）

- `git mv` による物理リネーム、`.slnx`・各`.csproj`のパス修正を実施
- `dotnet build` で成功確認（この時点では名前空間・DIメソッド名は "Identity" のまま）

### Phase 2（完了）

- ユーザーがVisual Studio + ReSharperで名前空間リネーム（`SupportAdvance.Contexts.Identity.{Domain,Application,Infrastructure}` → `Authentication.*`）を実施
- 実施後の確認で以下の不足が判明したため、Claudeが直接修正:
  - テストプロジェクト自身の名前空間（`*.Domain.Tests` / `*.Application.Tests` / `*.Infrastructure.Tests`）はReSharperのリネーム対象に含まれておらず "Identity" のままだったため、8ファイルを手動修正
  - コメント・XMLドキュメントコメント・SQL/Markdown内の "Identity" 表記（`Program.cs`, `SystemCurrentUserService.cs`, DI拡張メソッドのXMLコメント, `FindEmployeeByADUseCase.cs`, `AuthorityRowId.cs`, `SqlQueryLoader.cs`, `ISequenceProvider.cs`, テスト用SQL/E2Eガイド等）を手動修正
  - DI拡張メソッド名 `AddIdentityApplicationModels` / `AddIdentityInfrastructureModels` はReSharperでリネームされていなかったため、定義側・`Program.cs`呼び出し側とも手動修正
  - `tests/Architecture.Tests/BoundedContextIsolationTests.cs` のテストメソッド名・namespace文字列も "Identity" → "Authentication" に修正（あわせて、実装と一致していなかった `SupportAdvance.Contexts.Auth.Identity` という誤った名前空間文字列を実際の `SupportAdvance.Contexts.Authentication` に訂正。同ファイル内の `Master.Employee` も同様に実装と不一致だが、本タスクの範囲外のため別issueとして起票）
  - `src/Contexts/CLAUDE.md` の古い記述（"Auth/Identity.* はスキャフォールドのみ"）も実態に合わせて修正
- `dotnet build --no-incremental`: 0エラー（既存警告25件のみ、Identity関連の新規警告なし）
- `dotnet test`: 全件成功（Authentication.Domain.Tests 98件、Authentication.Application.Tests 1件、Authentication.Infrastructure.Tests 2件、Architecture.Tests 54件を含む）
- `grep -r "Identity" src/ tests/` の残存は RepoDb の `.Identity(e => e.RowId)`（ORM API、無関係）のみ

### Phase 3（完了）
- `docs/Assistance/Guides/Identity_BC_設計ガイド.md` → `Authentication_BC_設計ガイド.md` にリネームし、本文中の "Identity BC"・`SupportAdvance.Contexts.Identity.*`・`Identity.Domain/Application/Infrastructure`・DIメソッド名を "Authentication" に置換（`IdentityRowId` という、実装には存在しない設計初期段階の型名は範囲外として保持）
- `docs/Assistance/Guides/SQL_ファイル管理_設計ガイド.md` も同様に置換（`SupportAdvance.Contexts.Identity.Identity.Infrastructure` という既存の誤字も併せて修正）。ただしこのガイドはテーブル名 `m_user_auth_sessions`（実際は `t_user_auth_sessions`）や `IdentityRowId` 列など、実装とは別の初期設計を例示したまま残っており、リネーム範囲外の内容不整合として別issueで起票
- `docs/Assistance/Guides/SQL_ファイル管理_設計ガイド.md`・`docs/SharedKernel/Entity_And_DomainEvents/Entity_And_DomainEvents_技術仕様.md` 内の実装言及リンク・namespace例も修正
- `src/Contexts/CLAUDE.md` の修正は Phase 2 で対応済み
- 日付入り Plans/Reports（`20260807_...`, `20260805_...`）、`docs/WorkMemo/2026-09-13.md`、`docs/Assistance/DraftIdeas/認証について.md` は当時の記録として意図的に未変更（過去のスナップショットのため）
- 記憶: `identity_bc_design_decision.md` → `authentication_bc_design_decision.md` にリネームし内容を更新（改名の経緯を注記として追加）。`MEMORY.md`、`current_user_service_fk_risk_resolved.md`、`current_user_service_stub.md`、`phase_4e_completion.md` 内の参照も更新。`aggregate_id_design_decision.md` は既に廃版マーク済みのため未変更
- `dotnet build --no-incremental`: 0エラーを再確認

## Phase 4: 検証（共同）

- `dotnet build --no-incremental`
- 全テスト実行
- 残存チェック: `grep -r "Identity" src/ tests/ docs/`（`Microsoft.IdentityModel.*` などNuGetパッケージ由来の無関係なヒットは除外）
- 問題なければコミット（分割コミット案: ①物理リネーム ②名前空間/シンボルリネーム ③ドキュメント/メモリ更新）

---

## 更新履歴

| 日付       | 更新内容                                                           |
| ---------- | ------------------------------------------------------------------ |
| 2026-09-16 | 初版作成。インプレース改名方式を採用し、Phase 1-4 の実装計画を策定 |
