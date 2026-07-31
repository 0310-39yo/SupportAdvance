# Crosscutting レイヤーでの作業ルール

依存関係の全体像・許可/禁止の完全な一覧はルートの [CLAUDE.md](../../CLAUDE.md) を参照。ここには `Crosscutting` 特有の注意点のみ書く。

- `Infrastructure` への `ProjectReference` を追加しない（循環参照になるため禁止。`Infrastructure` は本層を参照する側であり、逆方向はない）
- ロギング等の実装に必要な技術要素（NLog など）は NuGet パッケージとして直接参照する。Infrastructure プロジェクトを経由しない
- インターフェースと実装の両方をこの層内で完結させる（例：`IAppLogging<T>` とその NLog 実装 `FrameworkLoggingAdapter` は同じ `Logging/` フォルダにある）
- Domain / Application への依存は避ける
