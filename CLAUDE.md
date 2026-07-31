# SupportAdvance - Claude Code ガイドライン

プロジェクト固有の実装ガイドライン、アーキテクチャ原則、および開発ワークフロー

---

## 📐 アーキテクチャ原則

このプロジェクトは **クリーンアーキテクチャ** の原則に従っています。

### 基本ルール

**依存の法則（Dependency Rule）**: 依存関係は外側から内側にのみ向かう。

```
Presentation → Application → Domain ← Infrastructure
                                ↑
                         SharedKernel
```

### 層間の許可される依存関係（要約）

`Common` が依存ゼロの最内層、その上に `SharedKernel` が構築される。詳細な依存関係マトリックス・図は [CLEAN_ARCHITECTURE_GUIDELINES.md](docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md#依存関係マトリックス) を正とする。

| From | To | 許可 | 備考 |
|---|---|---|---|
| SharedKernel | Common | ✓ | Common は依存ゼロ、SharedKernel はその上に構築 |
| Domain | SharedKernel, Common, Crosscutting | ✓ | Crosscutting はドメインイベント発行のみ |
| Application（Context別）| Domain, SharedKernel, Common, Crosscutting, Application（汎用層） | ✓ | 汎用層の IUseCase 等インターフェースを実装。Infrastructure は DI で注入 |
| Infrastructure | Domain, SharedKernel, Common, Crosscutting | ✓ | ✗ Application は禁止。Crosscutting → Infrastructure は逆方向で禁止（循環参照になるため） |
| Presentation | Application, Crosscutting, SharedKernel, Common | ✓ | Program.cs のみ Infrastructure 可（型レベルでNetArchTestにより検証） |

### 違反してはいけない依存関係

❌ **禁止:**
- Common → SharedKernel（循環参照になるため）
- Domain → Application / Infrastructure / Presentation
- Application → Infrastructure / Presentation
- Infrastructure → Application / Presentation
- Crosscutting → Infrastructure（循環参照になるため）
- Presentation → Domain / Infrastructure（Program.cs を除く）

---

## 📁 ディレクトリ構造と責務

```
src/
├── SharedKernel/          # 基盤型（ValueObject, Entity 基底など）
├── Common/                # 汎用ユーティリティ（Clock, Settings など）
├── Crosscutting/          # ロギング、監査、横断的関心事
├── Application/           # Use Cases / Application Services
├── Infrastructure/        # DB, ORM, 外部サービス実装
├── Contexts/
│   └── Samples/
│       └── CarPreferences/
│           ├── Domain/     # ドメインロジック
│           ├── Application/ # Use Cases
│           └── Infrastructure/ # DB実装
└── Presentation/
    ├── Shared/           # 共有 UI コンポーネント
    ├── WinTrial/         # Windows Forms UI
    └── WpfTrial/         # WPF UI
```

各層の詳細は [docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md](../docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md) を参照

---

## ✅ 新規プロジェクト追加時のチェックリスト

### 1. レイヤーの決定
- [ ] プロジェクトが属するレイヤーを明確に決定
- [ ] Bounded Context 内での責務を定義

### 2. .csproj 参照の検証
- [ ] ProjectReference が依存関係ルールに準拠
- [ ] 不要な参照を削除
- [ ] 循環参照がないこと

**確認コマンド:**
```bash
# .csproj ファイルを直接確認
cat src/YourProject/YourProject.csproj
```

### 3. using 宣言の確認
- [ ] using 宣言が依存ルールに準拠
- [ ] 逆方向依存がないこと

**確認コマンド:**
```bash
# Application namespace への依存を検索
grep -r "using SupportAdvance.Application" src/Infrastructure/
# マッチがなければ OK
```

### 4. ビルド検証
```bash
dotnet build --no-incremental
# エラーがないこと
```

### 5. コードレビューポイント
- Domain層のコード: 他層への依存がないか
- Application層: Infrastructure は注入されているか
- 依存注入: Program.cs で正しく構成されているか

---

## 🔍 アーキテクチャ違反の修正例

### ❌ 違反: Infrastructure → Application

**原因:**
```xml
<!-- Infrastructure.csproj -->
<ProjectReference Include="..\Application\Application.csproj" />
```

**修正:**
```xml
<!-- 削除 -->
```

**確認:**
```bash
# Application 型が使用されていないことを確認
grep -r "using.*Application" src/Infrastructure/
# マッチなし = OK
```

### ❌ 違反: Domain が Application を参照

**原因:**
```csharp
using SupportAdvance.Application.Dtos;

public class Order
{
    public OrderStatusDto Status { get; set; } // ✗ Domain が Application 型を使用
}
```

**修正:**
```csharp
public class Order
{
    public OrderStatus Status { get; set; } // ✓ Domain 独立の型
}

// Application層で変換
public class UpdateOrderService
{
    public async Task Execute(UpdateOrderDto dto)
    {
        var status = MapToOrderStatus(dto); // 変換
        var order = await _repository.GetAsync(dto.OrderId);
        order.UpdateStatus(status);
    }
}
```

---

## 🧪 テスト戦略

### Domain層のテスト
- Unit tests のみ（外界への依存なし）
- Mock / Stub 不要（ビジネスロジックのみ）

### Application層のテスト
- Unit tests with mocked repositories
- Infrastructure への依存は DI でモック化

### Infrastructure層のテスト
- Integration tests（実DB接続）
- または、テスト用 in-memory DB

---

## ⏰ LocalDateTime 使用規則

### 基本原則
- 全層で LocalDateTime を使用（DateTimeの直接使用は禁止）
- IClock 経由でのみ日時を取得

### DateTime の使用禁止の例外
- **Clock の実装内部**：DateTime.Now, DateTime.UtcNow などは使用してもよい
  - Clock はシステム時刻を LocalDateTime に変換する責務を持つ

### Clock の取得と使用
- 全層で DI を通じて IClock を注入
- `var now = _clock.JstNow;` で LocalDateTime を取得

### 例外ケースと対応
1. **外部システム/DB/3rd party からの DateTime**
   - 受け取った層で早期に LocalDateTime に変換
   - 変換処理の責務は受け取った層

2. **Clock の実装**
   - 本番環境：SystemClock のみ使用
   - テスト環境：FixedClock, OffsetClock など使用

---

## 🔧 開発時の注意点

### 新しい機能を実装する際

1. **Domain層から始める**
   ```csharp
   // Domain/Entities に Entity を定義
   public class Car : Entity { }
   ```

2. **Application層で Use Case を定義**
   ```csharp
   // Application/UseCases に Use Case を実装
   public class UpdateCarUseCase { }
   ```

3. **Infrastructure層で実装**
   ```csharp
   // Infrastructure で Repository を実装
   public class CarRepository : ICarRepository { }
   ```

4. **Presentation層で UI を構築**
   ```csharp
   // Presentation/ViewModels で ViewModel を実装
   public class CarViewModel { }
   ```

### インターフェース vs 実装

- Domain / Application: **インターフェースのみ定義**
- Infrastructure: **インターフェースを実装**
- Presentation: Application のインターフェースを使用

---

## 📚 参考資料

- **CLEAN_ARCHITECTURE_GUIDELINES.md**: 詳細なガイドライン
- **各層の `src/*/CLAUDE.md`**: 層別の作業ルール（Common, SharedKernel, Crosscutting, Application, Infrastructure, Presentation, Contexts）
- Clean Architecture（Robert C. Martin）

---

## 🚀 CI/CD での自動検証

依存関係の遵守は現状コードレビューに依存している。`NetArchTest.Rules` を使った型レベルの検証テスト例は [CLEAN_ARCHITECTURE_GUIDELINES.md の「自動検証の導入」](docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md#自動検証の導入) を参照。特に「Presentation → Infrastructure は Program.cs のみ」は `.csproj` の `ProjectReference` だけでは強制できないため、このテストでの担保が必須。

---

## 📝 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-07-31（後）| Application層の依存関係表を修正。汎用Application層と Bounded Context別Application層の区別を明記。「Application（Context別）→ Application（汎用層）」が IUseCase 実装パターンとして許可されることを追記 |
| 2026-07-31 | CLEAN_ARCHITECTURE_GUIDELINES.md の実コードとの不一致修正に合わせて本ファイルも修正。Domain/Common/SharedKernel の依存関係表を実装に合わせて訂正、Crosscutting→Infrastructure禁止を明記、SlnArch（未検証）の記述をNetArchTest.Rulesへの参照に置き換え |
| 2026-07-30 | LocalDateTime 使用規則を追加。全層で IClock 経由の LocalDateTime 使用を明確化 |
| 2026-07-09 | 初版作成。クリーンアーキテクチャ原則と新規プロジェクトチェックリスト |

