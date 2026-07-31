# SharedKernel レイヤーでの作業ルール

依存関係の全体像・許可/禁止の完全な一覧はルートの [CLAUDE.md](../../CLAUDE.md) を参照。ここには `SharedKernel` 特有の注意点のみ書く。

- `Common` 以外への `ProjectReference` を追加しない
- ビジネスロジックを置かない。ValueObject基底クラス、Entity基底クラス、監査ValueObject（`CreatedAt`/`UpdatedAt`/`DeletedAt`）など、全層から参照される基盤型のみ
- 監査ValueObjectで日時を扱う場合は `Common` の `LocalDateTime`/`IClock` 経由で取得する（`DateTime` の直接使用は禁止。ルート CLAUDE.md の「LocalDateTime 使用規則」を参照）
