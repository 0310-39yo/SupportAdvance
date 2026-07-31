# Infrastructure レイヤーでの作業ルール

依存関係の全体像・許可/禁止の完全な一覧はルートの [CLAUDE.md](../../CLAUDE.md) を参照。ここには `Infrastructure` 特有の注意点のみ書く。

- `Application` / `Presentation` への `ProjectReference` を追加しない
- Domain / Application が定義したインターフェース（Repository など）の実装を置く場所。技術的な詳細（DB接続、ORM、外部API呼び出し）はここに隠蔽し、上位層に漏らさない
- Bounded Context 別の Infrastructure（`src/Contexts/*/Infrastructure`）は同一Context の Domain と、この汎用 `Infrastructure` プロジェクトのみ参照する
