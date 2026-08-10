# Contexts フォルダでの作業ルール

依存関係の全体像・許可/禁止の完全な一覧はルートの [CLAUDE.md](../../CLAUDE.md) を参照。ここには `Contexts` フォルダ特有の注意点のみ書く。

## 命名規則

```
Contexts/
└── <グループ名>/                    # 例: Samples, Auth, Master
    ├── <Context名>.Domain/          # 例: CarPreferences.Domain
    ├── <Context名>.Application/     # 例: CarPreferences.Application
    └── <Context名>.Infrastructure/  # 例: CarPreferences.Infrastructure
```

新しい Bounded Context を追加する際は、既存の `Samples/CarPreferences.*`（唯一の実装済みサンプル）の `ProjectReference` 構成を参考にする。`Auth/Identity.*` と `Master/Employee.*` はフォルダのみのスキャフォールドで、実装（`.cs`/`.csproj`）はまだ存在しない。

## 必ず守ること

- **異なる Bounded Context 間は直接参照しない**（例：`CarPreferences.Domain` → `Identity.Domain` は禁止）。連携が必要な場合は Presentation 層や汎用 Application 層でのオーケストレーション、またはドメインイベント経由の疎結合連携を検討する
- 各層（Domain/Application/Infrastructure）の許可される参照はルート CLAUDE.md の依存関係表に従う。「Context別」という点以外の追加ルールはない
- Presentation 層からは `<Context名>.Application` のみを参照させる（`<Context名>.Infrastructure` への参照は Composition Root の `Program.cs` のみ）
