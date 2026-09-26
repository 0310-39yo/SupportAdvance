# Presentation レイヤーでの作業ルール

依存関係の全体像・許可/禁止の完全な一覧はルートの [CLAUDE.md](../../CLAUDE.md) を参照。ここには `Presentation` 特有の注意点のみ書く。

- ビジネスロジックを書かない。Application 層の Use Case / Application Service を呼び出すのみ
- Domain への直接参照を避け、Application 層の DTO 経由でやり取りする
- `Infrastructure` への参照は **Composition Root 内でのみ**許可。Composition Root は `Program.cs`（WinForms など）、および WPF の `App.xaml.cs`（WPF は `Program.cs` を持たないため）。ViewModel / Controller / Decorator など他のファイルから Infrastructure を参照しない

```csharp
// Composition Root（Program.cs / App.xaml.cs）のみで許可
var services = new ServiceCollection();
services.AddInfrastructureModels(configuration);
services.AddApplicationServices();

// ViewModel/Controller では使用しない
```

- Composition Root はプロジェクト参照上 Infrastructure に到達できるため、この規律は NetArchTest 等の自動検証で担保する方針（詳細は [CLEAN_ARCHITECTURE_GUIDELINES.md](../../docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md#自動検証の導入)）

- アイコンは Fluent UI System Icons を `AppIcon`／`FluentIconCatalog`（`Presentation.Shared/Icons`）経由で使う。コードポイントを画面に直接書かない。追加手順は [アイコン_利用ガイド.md](../../docs/Assistance/Guides/アイコン_利用ガイド.md)
