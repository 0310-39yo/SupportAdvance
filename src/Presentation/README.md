# Presentation - UI / Controllers

## 責務

- UI の構築
- ユーザー入力の受け取り
- Use Cases の呼び出し
- 結果の表示

## 依存関係

**許可:**
- Application（Use Case 呼び出し）
- Crosscutting（ロギング）
- Infrastructure（**Program.cs（Composition Root）のみ**）
- SharedKernel / Common（型の使用）

**禁止:**
- Domain（直接参照は避け、Application DTO 経由）
- Infrastructure（**Program.cs を除く**）

## 実装ガイドライン

- ビジネスロジックを含まない
- Application の Use Case / Application Service を呼び出す
- Infrastructure への依存は Program.cs でのみ許可（DI 構築）
- ViewModels / Controllers では Infrastructure を参照しない

### 正しい DI 構築例

```csharp
// Program.cs のみで許可
var services = new ServiceCollection();
services.AddInfrastructureModels(configuration);
services.AddApplicationServices();

// ViewModel/Controller では使用しない
```

---

詳細は [CLEAN_ARCHITECTURE_GUIDELINES.md](../../docs/CLEAN_ARCHITECTURE_GUIDELINES.md) を参照
