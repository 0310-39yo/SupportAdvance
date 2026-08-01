# 依存関係ルール - SupportAdvance

**最終更新**: 2026-06-30

---

## 目次

1. [基本ルール](#基本ルール)
2. [許可される依存](#許可される依存)
3. [禁止パターン](#禁止パターン)
4. [依存性逆転（DIP）](#依存性逆転dip)
5. [チェックリスト](#チェックリスト)

---

## 基本ルール

### Rule 1: 外側 → 内側のみ

```
外側（UI、技術実装）
  ↓ のみ参照
中間（ユースケース）
  ↓ のみ参照
内側（ビジネスロジック）

❌ 逆向き参照は禁止
❌ 横方向参照は禁止
```

### Rule 2: インターフェースは依存される側で定義

```
❌ 悪い例:
   Infrastructure で IRepository定義
   → Application が参照

✅ 良い例:
   Application で IRepository定義
   → Infrastructure が実装
```

### Rule 3: 循環依存なし

```
❌ 禁止:
   A → B → A

✅ 許可:
   A → B → C (一方向)
```

---

## 許可される依存

### Presentation層からの参照

```
✅ Presentation → Application
✅ Presentation → Presentation.Shared
✅ Presentation → Common
✅ Presentation → Crosscutting
✅ Presentation → Infrastructure (DI設定のみ、エントリーポイント限定)

❌ Presentation → Domain
❌ Presentation → SharedKernel (直接)
```

**具体例**:
```csharp
// ✅ OK: Application経由
var response = await useCase.Execute(request);

// ❌ NG: Domain直接操作
var carPreference = new CarPreference();  // Domain参照
```

### Application層からの参照

```
✅ Application → Domain
✅ Application → SharedKernel
✅ Application → Common
✅ Application → Crosscutting (ロギング等、DI経由のインターフェース参照)

❌ Application → Presentation
❌ Application → Infrastructure (実装に依存。インターフェースはOK)
```

**具体例**:
```csharp
// ✅ OK: Domain参照
var carPreference = await repository.GetById(id);

// ✅ OK: Crosscutting（ロギング）をDI経由で参照
public class GetCarPreferenceUseCase
{
    private readonly ILogger<GetCarPreferenceUseCase> _logger;  // ✅ インターフェース参照
    
    public GetCarPreferenceUseCase(ILogger<GetCarPreferenceUseCase> logger)
    {
        _logger = logger;
    }
}

// ❌ NG: Infrastructure実装に直接依存
public class BadGetCarPreferenceUseCase
{
    private readonly SqlConnection _conn;  // ❌
}
```

### Domain層からの参照

```
✅ Domain → Common
✅ Domain → SharedKernel

❌ Domain → Application
❌ Domain → Presentation
❌ Domain → Infrastructure
❌ Domain → Crosscutting
```

**具体例**:
```csharp
// ✅ OK: ビジネスロジックのみ
public void UpdatePreference(PreferenceCategory category)
{
    this.Category = category;  // Domain logic
}

// ❌ NG: 技術依存
public class CarPreference
{
    public void SaveToDatabase() { }  // ❌
}
```

### Infrastructure層からの参照

```
✅ Infrastructure → Application (インターフェース経由)
✅ Infrastructure → Common
✅ Infrastructure → Crosscutting

❌ Infrastructure → Domain (直接依存)
❌ Infrastructure → Presentation
```

**具体例**:
```csharp
// ✅ OK: Application インターフェース経由
public class CarPreferenceRepository : ICarPreferenceRepository
{
    public async Task<CarPreference> GetById(int id) { }
}

// ❌ NG: Domain直接実装に依存
public class InvalidRepository
{
    public CarPreference GetById(int id)
    {
        // Domain実装を変更 ❌
        var cp = new CarPreference();
    }
}
```

### Crosscutting層からの参照

```
✅ Crosscutting → Common

❌ Crosscutting → Application
❌ Crosscutting → Domain
❌ Crosscutting → Infrastructure
❌ Crosscutting → Presentation
```

**具体例**:
```csharp
// ✅ OK: 共通設定のみ参照
var appSettings = configuration.GetSection("AppSettings");

// ❌ NG: ビジネスロジック参照
var useCase = services.GetRequiredService<IUseCase>();
```

---

## 禁止パターン

### パターン1: Presentation が Domain に直接依存

```csharp
❌ 禁止:

// Presentation/WinTrial/Program.cs
using SupportAdvance.Contexts.Samples.CarPreferences.Domain;

public class Form1
{
    private void buttonClick(object sender, EventArgs e)
    {
        var carPreference = new CarPreference();  // ❌
    }
}
```

**改善**:
```csharp
✅ 推奨:

// Application経由で操作
var response = await useCase.Execute(request);
```

### パターン2: Domain が Infrastructure に依存

```csharp
❌ 禁止:

public class CarPreference : Entity
{
    private readonly ICarPreferenceRepository _repository;  // ❌
    
    public void Save()
    {
        _repository.Save(this);  // Infrastructure依存
    }
}
```

**改善**:
```csharp
✅ 推奨:

// Infrastructure が Domain を参照して永続化
public class CarPreferenceRepository : ICarPreferenceRepository
{
    public async Task Save(CarPreference cp)
    {
        // ここで永続化
    }
}
```

### パターン3: Application が Infrastructure実装に依存

```csharp
❌ 禁止:

public class GetCarPreferenceUseCase
{
    private readonly SqlCarPreferenceRepository _repository;  // ❌ 具象依存
}
```

**改善**:
```csharp
✅ 推奨:

public class GetCarPreferenceUseCase
{
    private readonly ICarPreferenceRepository _repository;  // ✅ インターフェース
}
```

### パターン4: 循環依存

```csharp
❌ 禁止:

// A.cs
public class A
{
    public B Dependency { get; set; }  // A → B
}

// B.cs
public class B
{
    public A Dependency { get; set; }  // B → A (循環!)
}
```

**改善**:
```csharp
✅ 推奨:

// 一方向のみ
public class A
{
    public B Dependency { get; set; }  // A → B のみ
}

public class B
{
    // A を参照しない
}
```

### パターン5: Presentation が Dapper/ORM に直接依存

```csharp
❌ 禁止:

// Presentation/Shared/Shared.csproj
<PackageReference Include="Dapper" Version="2.1.79" />  // ❌

// Presentation層でDapper使用
using Dapper;
```

**改善**:
```csharp
✅ 推奨:

// Infrastructure でのみORM依存
// Infrastructure.csproj
<PackageReference Include="Dapper" Version="2.1.79" />  // ✅

// Presentation は Repository インターフェース経由
var repository = services.GetRequiredService<IRepository>();
```

---

## 依存性逆転（DIP）

### 概念

**依存性逆転原則（Dependency Inversion Principle）**:
- 上位モジュールが下位モジュールに依存してはいけない
- 両者ともに抽象に依存すべき

### 実装方法

```
❌ 標準的な依存関係:
   UseCase → SqlRepository (具象)

✅ 依存性逆転:
   UseCase → IRepository (抽象)
   SqlRepository → IRepository (実装)
```

### コード例

```csharp
// 1. インターフェース定義（Application層）
public interface ICarPreferenceRepository
{
    Task<CarPreference> GetById(int id);
}

// 2. 実装（Infrastructure層）
public class CarPreferenceRepository : ICarPreferenceRepository
{
    public async Task<CarPreference> GetById(int id)
    {
        // DB操作
    }
}

// 3. UseCase（Application層）
public class GetCarPreferenceUseCase
{
    private readonly ICarPreferenceRepository _repository;  // ✅ インターフェース参照
    
    public GetCarPreferenceUseCase(ICarPreferenceRepository repository)
    {
        _repository = repository;  // DI経由で注入
    }
    
    public async Task<CarPreferenceResponse> Execute(...)
    {
        var cp = await _repository.GetById(id);  // インターフェース経由
    }
}

// 4. DI登録（Presentation層）
services.AddScoped<ICarPreferenceRepository, CarPreferenceRepository>();
services.AddScoped<GetCarPreferenceUseCase>();
```

### メリット

- ✅ UseCase は Repository実装を知らない
- ✅ Repository実装を変更してもUseCase不変
- ✅ テスト時にモック Repository を注入可能
- ✅ 新規Repository実装の追加が容易

---

## チェックリスト

### コード レビュー時

- [ ] 依存方向は常に外側 → 内側か？
- [ ] Domain層が Infrastructure参照していないか？
- [ ] Presentation層が Domain直接参照していないか？
- [ ] インターフェースは内側（Application）で定義されているか？
- [ ] 循環依存がないか？

### 新規クラス作成時

- [ ] このクラスはどの層に属するか明確か？
- [ ] この層が参照してはいけない他の層を参照していないか？
- [ ] インターフェースと実装は正しく分離されているか？

### DI設定時

- [ ] DI登録は Presentation.Shared（エントリーポイント）で一括管理されているか？
- [ ] 循環参照による登録順序の問題がないか？

---

## トラブルシューティング

### Q: 「型を解決できない」というビルドエラーが出た

**原因の確認**:
```csharp
// ❌ 考えられる原因
public class MyUseCase
{
    public MyUseCase(ICarPreferenceRepository repository)  // 参照できない？
    {
    }
}
```

**解決方法**:
1. using文を確認（Application.csproj に IRepository定義があるか）
2. DI登録を確認（services.AddScoped<IRepository, ...>() されているか）

### Q: 「プロジェクト参照に問題がある」という警告

**原因**:
- 不正な層間参照が存在する可能性

**確認方法**:
```powershell
# .csproj ファイルをテキストで確認
# ProjectReference が期待される層のみか確認
```

### Q: 新規Contextを追加したい

**手順**:
1. `[ContextName].Domain` 作成
2. `[ContextName].Application` 作成
3. `[ContextName].Infrastructure` 作成
4. Application/Infrastructure が汎用層を参照する設定

詳細は `06-Multi-Context-Design-Pattern.md` 参照

---

## 参考

- **00-Overview.md**: アーキテクチャ全体図
- **01-Layer-Architecture.md**: 各層の詳細責務
- **07-Project-Structure.md**: プロジェクト構成

---

**作成日**: 2026-06-30
