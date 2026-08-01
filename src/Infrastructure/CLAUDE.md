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

## その他
- Domain / Application が定義したインターフェース（Repository など）の実装を置く場所。技術的な詳細（DB接続、ORM、外部API呼び出し）はここに隠蔽し、上位層に漏らさない
- Bounded Context 別の Infrastructure（`src/Contexts/*/Infrastructure`）は同一Context の Domain と、この汎用 `Infrastructure` プロジェクトのみ参照する
