# Infrastructure レイヤーでの作業ルール

依存関係の全体像・許可/禁止の完全な一覧はルートの [CLAUDE.md](../../CLAUDE.md) を参照。ここには `Infrastructure` 特有の注意点のみ書く。

## 依存関係の明確化

### ✓ 許可される参照
- Domain（エンティティ型の実装のため）
- SharedKernel（基盤型のため）
- Common（ユーティリティのため）
- Crosscutting（ロギング・監査のため）
- **Application の インターフェース のみ**（Repository等の実装のため）
  - 例：`IRepository<T>`、`IUserPreferencesRepository` 等のインターフェース実装

### ✗ 禁止される参照
- Application の **実装**（Use Case, Handler等）
- Presentation（あらゆるコンポーネント）

### パターン
```csharp
// ✓ 正: インターフェース（Application層定義）を実装
public class UserRepository : IUserRepository { }

// ✗ 誤: Use Case（Application層実装）に依存
private readonly CreateUserUseCase _useCase; // 禁止
```

## 🚫 Infrastructure層での null 処理（層間フィルター）

Infrastructure層は DB の null を Domain の Unset() に変換する責務があります。すべての ValueObject を null-free 状態で Domain に渡す必要があります。

また、**DB の型（`DateTime`）は Infrastructure の内側に閉じ込めます**。DB の `DateTime?` を `LocalDateTime?` に変換してから、値オブジェクトの `TryFrom(LocalDateTime?)` に渡します。値オブジェクトは `DateTime` を知りません（詳細はルート [CLAUDE.md](../../CLAUDE.md) の「DateTime は Infrastructure の内側に閉じる」）。

### 実装パターン

```csharp
public class UserPreferencesRepository : IUserPreferencesRepository
{
    public async Task<UserPreferences?> GetAsync(UserId userId)
    {
        var dbModel = await _context.UserPreferences.FindAsync(userId.Value);
        if (dbModel == null) return null;
        
        // ============ 層間フィルター ============
        // DB の DateTime? を LocalDateTime? に変換し、TryFrom で null を Unset() に変換
        
        CreatedAt.TryFrom(dbModel.CreatedAtDb.ToLocalDateTimeOrNull(), out var createdAt);
        UpdatedAt.TryFrom(dbModel.UpdatedAtDb.ToLocalDateTimeOrNull(), out var updatedAt);
        DeletedAt.TryFrom(dbModel.DeletedAtDb.ToLocalDateTimeOrNull(), out var deletedAt);
        
        // すべて null-free 状態で Domain に渡す
        return UserPreferences.Reconstruct(userId, createdAt, updatedAt, deletedAt, ...);
    }
}
```

> **移行中の注意**: `ToLocalDateTimeOrNull()`（`DateTime?` → `LocalDateTime?`）は、[原則完全準拠 実装計画](../../docs/Assistance/Plans/20260926_原則完全準拠_実装計画.md) のフェーズ 4 で追加する。現状のコードは、値オブジェクトの旧形式 `TryFromDbValue(DateTime?)` を使っている箇所がある（フェーズ 4 で置き換え）

### 責務

- **DateTime → LocalDateTime の変換と TryFrom の呼び出し**: DB の null を自動的に Unset() に変換
- **例外投げ**: CreatedAt など必須フィールドが null の場合は例外投げ（DB整合性エラー）
- **null-free保証**: Domain に渡すすべての ValueObject が null を含まないこと

### 参考

詳細は [null 厳格性設計ガイド](../../docs/Assistance/Guides/null厳格性設計ガイド.md) — セクション 2.3 Infrastructure層

## その他
- Domain / Application が定義したインターフェース（Repository など）の実装を置く場所。技術的な詳細（DB接続、ORM、外部API呼び出し）はここに隠蔽し、上位層に漏らさない
- Bounded Context 別の Infrastructure（`src/Contexts/*/Infrastructure`）は同一Context の Domain と、この汎用 `Infrastructure` プロジェクトのみ参照する
