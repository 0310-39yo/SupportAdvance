# SharedKernel レイヤーでの作業ルール

依存関係の全体像・許可/禁止の完全な一覧はルートの [CLAUDE.md](../../CLAUDE.md) を参照。ここには `SharedKernel` 特有の注意点のみ書く。

- `Common` 以外への `ProjectReference` を追加しない
- ビジネスロジックを置かない。ValueObject基底クラス、Entity基底クラス、監査ValueObject（`CreatedAt`/`UpdatedAt`/`DeletedAt`）など、全層から参照される基盤型のみ
- 監査ValueObjectで日時を扱う場合は `Common` の `LocalDateTime`/`IClock` 経由で取得する（`DateTime` の直接使用は禁止。ルート CLAUDE.md の「LocalDateTime 使用規則」を参照）
- **値オブジェクトの公開メンバー（引数・戻り値・プロパティ）に `DateTime` を持たせない**。DB の型との変換（`FromDbValue(DateTime)` / `TryFromDbValue(DateTime?)` / `ToDbValue()`）は持たず、Infrastructure（Mapper / Repository）が行う。入口は `From(LocalDateTime)` / `TryFrom(LocalDateTime?)` のみ（例外なし）。現状の `CreatedAt` / `UpdatedAt` / `DeletedAt` には旧形式が残っており、[実装計画](../../docs/Assistance/Plans/20260926_原則完全準拠_実装計画.md) のフェーズ 4 で削除する（移行完了まで新規追加は禁止）

## 🚫 null 厳格性原則

すべての ValueObject は **未設定状態を null ではなく型で表現**します（IsSet フラグ）。

### 実装ルール

- **Unset()** は null ではなく有効なインスタンス（Value = 型のデフォルト値）
  ```csharp
  public static UpdatedAt Unset()
      => new(LocalDateTime.MinValue, false);  // Value を常に保持
  ```

- **Domain層では null チェック不要**（IsSet で判定）
  ```csharp
  // ❌ 間違い
  if (entity.UpdatedAt == null) { ... }
  
  // ✅ 正しい
  if (entity.UpdatedAt.HasUpdated) { ... }
  ```

- **TryFrom** で null を自動的に Unset() に変換（DB の `DateTime?` は、Infrastructure が `LocalDateTime?` に変換してから渡す）
  ```csharp
  UpdatedAt.TryFrom(dbModel.UpdatedAt.ToLocalDateTimeOrNull(), out var updatedAt);
  // 入力が null の場合、updatedAt は Unset() 状態で渡される
  ```

### 参考

詳細は [null 厳格性設計ガイド](../../docs/Assistance/Guides/null厳格性設計ガイド.md) を参照してください。
