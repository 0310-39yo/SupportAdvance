# Common レイヤーでの作業ルール

依存関係の全体像・許可/禁止の完全な一覧はルートの [CLAUDE.md](../../CLAUDE.md) を参照。ここには `Common` 特有の注意点のみ書く。

- `Common.csproj` に `ProjectReference` を追加しない。依存ゼロを維持すること（SharedKernel を含め、他プロジェクトへの参照は禁止）
- ビジネスロジック・ドメイン固有の判断ロジックを置かない。`IClock` / `LocalDateTime` / `IApplicationSettings` のような汎用型・汎用ユーティリティのみ
- ここに置く型はプロジェクト全体（Domain, Application, Infrastructure, Presentation すべて）から参照される前提で設計する
