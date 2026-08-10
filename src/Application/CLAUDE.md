# Application レイヤーでの作業ルール

依存関係の全体像・許可/禁止の完全な一覧はルートの [CLAUDE.md](../../CLAUDE.md) を参照。ここには `Application` 特有の注意点のみ書く。

このレイヤーは2種類のプロジェクトを含む。どちらで作業しているか区別すること：

1. **汎用Application層**（`src/Application` 直下）— `IUseCase` / `IRequest` / `IResponse` などのインターフェース定義のみ。具体的な実装や Bounded Context 固有のロジックを追加しない
2. **Bounded Context別Application層**（`src/Contexts/*/Application`）— 汎用層のインターフェースを実装し、Use Case を実装する場所

- Context別Application が汎用Application（`IUseCase` 等）に依存するのは許可されたパターン。逆に汎用Application が Context別層を参照することは禁止
- Context別Application から `Infrastructure` を直接参照しない。Repository 等は DI により注入される

## 🚫 Application層での null 処理

Application層は Domain への入り口です。外部入力（JSON/UI/API）での null を許容し、TryFrom で自動的に Unset() に変換してから Domain に渡します。

### 実装パターン

```csharp
// 外部入力で null チェック → TryFrom で変換
if (!RespondentName.TryFrom(request.Name, out var name))
    throw new ArgumentException("Invalid input");

// 以降、Domain層では name が null-free であることが保証される
var entity = new Entity(name, ...);
```

### オプション入力の扱い

```csharp
// TryFrom で null は自動的に Unset() に変換される
RespondentName.TryFrom(request.PreferredName, out var preferredName);

if (preferredName.IsSet)
    ProcessName(preferredName.Value);  // 値あり
else
    // 未指定状態の処理
```

### 参考

詳細は [null 厳格性設計ガイド](../../docs/Assistance/Guides/null厳格性設計ガイド.md) — セクション 2.2 Application層
