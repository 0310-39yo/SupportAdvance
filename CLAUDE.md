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
| Domain | SharedKernel, Common | ✓ | ドメインロジック完全独立。ドメインイベント発行は Entity.RaiseDomainEvent()で内部完結 |
| Application（Context別）| Domain, SharedKernel, Common, Crosscutting, Application（汎用層） | ✓ | 汎用層の IUseCase 等インターフェースを実装。Infrastructure は DI で注入 |
| Infrastructure | Domain, SharedKernel, Common, Crosscutting, Application（インターフェースのみ） | ✓ | ✓ Repository等のインターフェース実装のため。✗ Use Case 等の実装には依存禁止。Crosscutting → Infrastructure は逆方向で禁止（循環参照） |
| Presentation | Application, Crosscutting, SharedKernel, Common | ✓ | Composition Root（`Program.cs`。WPF は `App.xaml.cs`）のみ Infrastructure 可（型レベルでNetArchTestにより検証） |

### 違反してはいけない依存関係

❌ **禁止:**
- Common → SharedKernel（循環参照になるため）
- Domain → Application / Infrastructure / Presentation
- Application → Infrastructure / Presentation
- **Infrastructure → Application の実装**（Use Case等）/ Presentation（インターフェースのみの参照はOK）
- Crosscutting → Infrastructure（循環参照になるため）
- Presentation → Domain / Infrastructure（Composition Root（`Program.cs` / WPF の `App.xaml.cs`）を除く）

**✓ 許可される例:**
- Infrastructure → Application.Repositories（IRepository等のインターフェース実装）
- Infrastructure は DI で注入された Repository を使用

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

各層の詳細は [docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md](docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md) を参照

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
- 依存注入: Composition Root（Program.cs / WPF の App.xaml.cs）で正しく構成されているか

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
- **Domain/SharedKernel/Application 層**: LocalDateTime を使用（DateTime の直接使用は禁止）
- **Infrastructure/DbModel 層**: DateTime プリミティブ型を使用（ORM マッピング用）
- **Mapper 層**: DateTime ↔ LocalDateTime の明示的な双方向変換を実装
- IClock 経由でのみ日時を取得

### DateTime は Infrastructure の内側に閉じる

`DateTime`（DB の型）は Infrastructure（DbModel・Mapper・Repository）の内側に閉じ込め、**Domain / SharedKernel / Application の公開メンバー（引数・戻り値・プロパティ）には持ち込まない**。**例外は設けない。**

- **値オブジェクトも同じ**: `FromDbValue(DateTime)` / `TryFromDbValue(DateTime?)` / `ToDbValue()`（日時を DB 型で受け渡すメソッド）を値オブジェクトに持たせない。値オブジェクトの入口は `From(LocalDateTime)` / `TryFrom(LocalDateTime?)` のみ
- **DB → Domain**: Infrastructure（Mapper / Repository）が `DateTime?` を `LocalDateTime?` に変換してから、`TryFrom` に渡す（`null` は `TryFrom` が `Unset()` に変換）
- **Domain → DB**: Infrastructure が `LocalDateTime.Value`（DateTime）を DbModel に設定する
- 監査値（CreatedAt / UpdatedAt / DeletedAt）を Repository が読み書きする場合も同じ

```csharp
// Infrastructure（Mapper / Repository）での DB → Domain
if (!UpdatedAt.TryFrom(dbModel.UpdatedAt.ToLocalDateTimeOrNull(), out var updatedAt))   // null → Unset()
    throw new InvalidOperationException("Invalid UpdatedAt");

// Domain → DB
dbModel.UpdatedAt = entity.UpdatedAt.HasUpdated ? entity.UpdatedAt.Value?.Value : null;   // LocalDateTime.Value = DateTime。未設定（Unset）の場合は null
```

> **移行中の注意**: 現状、一部の値オブジェクト（`CreatedAt` / `UpdatedAt` / `DeletedAt`、`AbolishedOn`、`RetiredOn`、`EndOn`、`ExpirationOn`、`EffectiveAt`）に旧形式の `FromDbValue(DateTime)` / `TryFromDbValue(DateTime?)` / `ToDbValue()` が残っている。[原則完全準拠 実装計画](docs/Assistance/Plans/20260926_原則完全準拠_実装計画.md) のフェーズ 4 で削除する。**移行が完了するまで、新規の追加は禁止**。上記の `ToLocalDateTimeOrNull()`（`DateTime?` → `LocalDateTime?`）もフェーズ 4 で Infrastructure に追加する

### 層別の責務

| 層 | 型 | 責務 |
|----|----|------|
| **Domain/Entity・値オブジェクト** | LocalDateTime | ビジネスロジック（型安全）。DateTime を持たない |
| **SharedKernel（監査 VO など）** | LocalDateTime | Domain と同じ。DateTime を持たない |
| **Application/DTO** | LocalDateTime | 外部インターフェース |
| **DbModel** | DateTime | ORM マッピング（プリミティブ型） |
| **Mapper / Repository** | 双方向変換 | DateTime ↔ LocalDateTime 変換 |

### DbModel での DateTime 使用

DbModel はすべての日時フィールドを **DateTime（プリミティブ型）** で保持します。理由：

1. **ORM マッピングの明確性** - Dapper/RepoDb のグローバルマッピングで自動変換
2. **責務の分離** - Infrastructure 層は DB ネイティブ型を使用
3. **型安全性** - Mapper で明示的な変換を実施

**DbModel の例**:
```csharp
public class UserPreferencesDbModel
{
    // 監査フィールドは DateTime（プリミティブ型）
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // ビジネスフィールドも DateTime
    public DateTime? RespondedAt { get; set; }
}
```

### Mapper での双方向変換と責務分離

Mapper は Entity ↔ DbModel の変換時に DateTime ↔ LocalDateTime を実装します。
**重要**: 監査フィールド（CreatedAt, UpdatedAt, DeletedAt など）は Mapper では設定せず、Repository が設定します。

**ToDbModel: Entity → DbModel** (LocalDateTime → DateTime、ビジネスフィールドのみ):
```csharp
public EmployeeDbModel ToDbModel(Employee entity)
{
    return new EmployeeDbModel
    {
        // ビジネスフィールドのみ
        RowId = entity.RowId.Value,
        BizDivision = entity.TypeDivision.ToDbValue(),
        HireDate = entity.HireDate.HasValue 
            ? entity.HireDate.Value.Value  // LocalDateTime.Value で DateTime 取得
            : (DateTime?)null,
        
        // ❌ 監査フィールドは設定しない（Repository が責務を持つ）
        // CreatedAt, CreatedBy, UpdatedAt, UpdatedBy は省略
    };
}
```

**ToDomainEntity: DbModel → Entity** (DateTime → LocalDateTime):
```csharp
public Employee ToDomainEntity(EmployeeDbModel dbModel, IClock clock)
{
    var hireDate = dbModel.HireDate.HasValue
        ? new LocalDateTime(dbModel.HireDate.Value)  // DateTime → LocalDateTime
        : null;
    
    return new Employee(
        hireDate: hireDate,
        ...);
}
```

### Mapper と Repository の責務分離

| 責務 | Mapper | Repository |
|------|--------|-----------|
| **ビジネスフィールド変換** | ✓ | - |
| **DateTime ↔ LocalDateTime** | ✓ | - |
| **CreatedAt/CreatedBy 設定** | ✗ | ✓ |
| **UpdatedAt/UpdatedBy 設定** | ✗ | ✓ |
| **DeletedAt/DeletedBy 設定** | ✗ | ✓ |
| **テスト容易性** | Clock 依存なし（テストしやすい） | Clock 依存あり（DI経由） |

**理由:**
- Mapper は純粋な型変換のみに専念
- 監査情報は「保存時刻」を記録する必要があり、Repository が実行時に取得すべき
- Mapper の Clock 依存を削除することで、テスト容易性が向上

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

## 🚫 Domain層 null 厳格性原則

### 基本原則

Domain層では **「未設定状態を null ではなく型で表現」**（Option/Maybe パターン）を採用しています。

これは、Haskell/Scala の Maybe 型の考え方をベースにしており、ビジネスロジックから不確実性（null の存在）を完全に排除することで、型安全性と可読性を確保するものです。

### レイヤ別責務

| 層 | 責務 | 例 |
|----|------|-----|
| **Domain層** | IsSet フラグで状態管理<br/>ビジネスロジックは null-free | `if (entity.UpdatedAt.HasUpdated)` |
| **Application層** | 外部入力で null 許容<br/>TryFrom で自動変換 | `RespondentName.TryFrom(request.Name, out var name)` |
| **Infrastructure層** | DB の `DateTime?` を `LocalDateTime?` に変換し、TryFrom で DB null → Unset() に変換<br/>すべての ValueObject が null-free で Domain に渡す | Mapper / Repository で TryFrom を呼び出し |
| **DB層** | DateTime / DateTime? ネイティブ型<br/>null が存在する可能性 | `updated_at DATETIME2 NULL` |

### 3つの基本実装パターン

#### 1. 必須ValueObject（CreatedAt パターン）

入力必須。null は失敗を返す。

```csharp
public static bool TryFrom(LocalDateTime? input, out CreatedAt result)
{
    if (!input.HasValue) return false;  // ← null は失敗
    try { result = From(input.Value); return true; }
    catch { return false; }
}
```

#### 2. オプションValueObject（RespondentName パターン）

入力オプション。null は Unset に変換して成功を返す。

```csharp
public static bool TryFrom(string? input, out RespondentName result)
{
    if (input == null)
    {
        result = Unset();  // ← null は Unset で成功
        return true;
    }
    try { result = From(input); return true; }
    catch { return false; }
}
```

#### 3. 監査ValueObjects（UpdatedAt/DeletedAt パターン）

オプションValueObject と同じ形。DB の null は、Infrastructure が `DateTime?` を `LocalDateTime?` に変換した後、TryFrom が Unset() に変換する。**値オブジェクトは DB の型（DateTime）を知らない。**

```csharp
// 値オブジェクト（Domain / SharedKernel）：LocalDateTime? だけを受け取る
public static bool TryFrom(LocalDateTime? input, out UpdatedAt result)
{
    if (input == null)
    {
        result = Unset();  // ← null → Unset（未更新状態）
        return true;
    }
    try { result = From(input.Value); return true; }
    catch { result = null!; return false; }
}

// Infrastructure（Mapper / Repository）：DB の DateTime? を変換して渡す
UpdatedAt.TryFrom(dbModel.UpdatedAt.ToLocalDateTimeOrNull(), out var updatedAt);
```

### Unset 状態の本質

```csharp
public static UpdatedAt Unset()
    => new(LocalDateTime.MinValue, false);
    //   ──────────────────────  ──────
    //   Value は null ではなくデフォルト値     IsSet = false で「未設定」を表現
```

**重要**: Domain層には null が存在しない。すべての値が有効である。

### よくあるエラー

```csharp
// ❌ 間違い
if (entity.UpdatedAt == null) { ... }      // Domain では起こらない

// ✅ 正しい
if (!entity.UpdatedAt.HasUpdated) { ... }  // IsSet で状態判定
```

### 参考資料

詳細は [**null 厳格性設計ガイド**](docs/Assistance/Guides/null厳格性設計ガイド.md) を参照してください。

以下のドキュメントは本設計に統合されました（廃版）：
- ❌ 監査ValueObject_null処理戦略.md
- ❌ 監査ValueObject_null処理詳細設計.md

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

## 🔗 Context間のデータ共有パターン

複数の Bounded Context が別の Context のドメインモデル（Aggregate）情報を **リアルタイムに読み取る** 場合、**ジェネリック Query Service パターン** を採用します。

### 問題

- Domain Events は非同期・イベント駆動なため、「今この瞬間の最新データ」が必要な場合には不向き
- Context別 Application層同士は参照禁止
- 汎用Application層がContext固有のインターフェース（IEmployeeQuery など）を定義すると、Context肥大化

### 解決法：ジェネリック Query Service パターン

**1. 汎用Application層に抽象的なインターフェースを定義**

```csharp
// src/Application/Queries/IQueryService.cs
namespace SupportAdvance.Application.Queries;

public interface IQueryService<TAggregate, TId> 
    where TAggregate : IAggregateRoot
{
    Task<TAggregate?> GetByIdAsync(TId id);
}
```

**2. 各Contextが実装**

```csharp
// src/Contexts/Employee/Application/Queries/EmployeeQueryService.cs
public class EmployeeQueryService : IQueryService<Employee, EmployeeId>
{
    private readonly IEmployeeRepository _repository;

    public async Task<Employee?> GetByIdAsync(EmployeeId id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
```

**3. 他のContextが使用**

```csharp
// src/Contexts/CarPreferences/Application/UseCases/UpdateCarPreferencesUseCase.cs
public class UpdateCarPreferencesUseCase
{
    private readonly IQueryService<Employee, EmployeeId> _employeeQuery;

    public async Task Execute(EmployeeId employeeId, CarModelRequest request)
    {
        var employee = await _employeeQuery.GetByIdAsync(employeeId);
        if (employee == null)
            throw new EmployeeNotFoundException();

        var pref = new CarPreferences(employeeId, request.Model);
        await _repository.SaveAsync(pref);
    }
}
```

**4. DI設定**

```csharp
// Program.cs
services.AddScoped<IQueryService<Employee, EmployeeId>, EmployeeQueryService>();
services.AddScoped<IQueryService<InsuranceProfile, InsuranceId>, InsuranceQueryService>();
```

### メリット

✅ **汎用層が肥大化しない** — ジェネリック定義のみ  
✅ **スケーラブル** — Aggregate追加時も構造不変  
✅ **Context独立** — 各Context が自身の Aggregate を管理  
✅ **依存方向が正** — Context別Application→汎用Application（正常方向）  

---

## 📁 ドキュメント管理

### フォルダ構成

プロジェクトドキュメントは以下のフォルダで管理します：

| フォルダ | 用途 | ファイル例 |
|---|---|---|
| `docs/Assistance/Plans` | 計画書、実装計画、分析計画 | `20260802_実装計画.md` |
| `docs/Assistance/Reports` | 分析結果、準拠調査、レビュー報告書 | `20260802_準拠調査報告.md` |

### 命名規則

**必須:** ファイル名の先頭に日付を `yyyyMMdd_` 形式で付与

```
✓ 20260802_実装計画.md
✓ 20260731_準拠調査報告.md
✗ 実装計画.md（日付なし）
```

### ドキュメント選定ガイド

- **Plans**: これから実施する計画、検討案、設計書
- **Reports**: 実施後の報告、分析結果、調査レポート、レビュー結果

---

## 🗄️ データベース テーブル設計規則

### 監査用カラムの必須化

**すべてのテーブルは以下の8つの監査カラムを必須とします。**

| # | カラム名 | 型 | 制約 | 用途 |
|---|---|---|---|---|
| 1 | `row_id` | `bigint` | PK, Sequence自動採番 | 主キー（システム基本ID） |
| 2 | `row_version` | `timestamp` | NOT NULL | 楽観ロック用タイムスタンプ |
| 3 | `created_at` | `datetime2(7)` | NOT NULL | 作成日時（LocalDateTime/JST） |
| 4 | `created_by` | `bigint` | NOT NULL, FK → m_persons | 作成者の従業員rowId |
| 5 | `updated_at` | `datetime2(7)` | NULL | 更新日時（LocalDateTime/JST） |
| 6 | `updated_by` | `bigint` | NULL, FK → m_persons | 更新者の従業員rowId |
| 7 | `deleted_at` | `datetime2(7)` | NULL | 削除日時（論理削除用） |
| 8 | `deleted_by` | `bigint` | NULL, FK → m_persons | 削除者の従業員rowId |

### テーブル設計チェックリスト

新規テーブルを設計する際、以下を確認してください：

- [ ] **row_id**: Sequence `s_row_id_sequence` でPKを自動採番
- [ ] **row_version**: `timestamp` で楽観ロック対応
- [ ] **created_at/created_by**: NOT NULL、作成時に自動設定
- [ ] **updated_at/updated_by**: NULL許可、更新時に設定
- [ ] **deleted_at/deleted_by**: NULL許可、論理削除用（IS NULLで有効行フィルタ）
- [ ] **foreign keys**: created_by, updated_by, deleted_by → m_persons(row_id)
- [ ] **default values**: created_by=SYSTEM_USER_ID など適切なデフォルト値

### SQL テンプレート

```sql
CREATE TABLE [dbo].[t_YourTable] (
    -- 監査カラム（必須）
    [row_id] [bigint] NOT NULL PRIMARY KEY DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
    [row_version] [timestamp] NOT NULL,
    [created_at] [datetime2](7) NOT NULL,
    [created_by] [bigint] NOT NULL,
    [updated_at] [datetime2](7) NULL,
    [updated_by] [bigint] NULL,
    [deleted_at] [datetime2](7) NULL,
    [deleted_by] [bigint] NULL,
    
    -- ビジネスカラム
    [your_column] [nvarchar](100) NOT NULL,
    
    -- 制約
    CONSTRAINT [CK_tYourTable_YourCheck] CHECK (...)
)
```

### 実装例

- **CarPreferences.Infrastructure**: `t_UserPreferences` テーブル
  - 参考: `src/Contexts/Samples/CarPreferences.Infrastructure/Migrations/002_CreateUserPreferencesTable.sql`

---

## 📝 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-09-10 | LocalDateTime 使用規則セクションを拡充。Mapper と Repository の責務分離を明確化：Mapper は純粋な型変換のみ（Clock 依存なし）、Repository が監査フィールド（CreatedAt/UpdatedAt/DeletedAt）設定を担当。責務分離表を追加。テスト容易性向上の理由を明記 |
| 2026-07-30 | LocalDateTime 使用規則を追加。全層で IClock 経由の LocalDateTime 使用を明確化 |
| 2026-07-09 | 初版作成。クリーンアーキテクチャ原則と新規プロジェクトチェックリスト |

---

## 📚 参考資料

- **CLEAN_ARCHITECTURE_GUIDELINES.md**: 詳細なガイドライン
- **各層の `src/*/CLAUDE.md`**: 層別の作業ルール（Common, SharedKernel, Crosscutting, Application, Infrastructure, Presentation, Contexts）
- **TABLE_DESIGN_STANDARDS.md**: データベース設計の詳細仕様（docs/Assistance/Guides/）
- **[XMLドキュメントコメント_ガイド.md](docs/Assistance/Guides/XMLドキュメントコメント_ガイド.md)**: XMLドキュメントコメントの書き方とテンプレート。文体は体言止め（文の区切りに「。」、最後の文には付けない）。コメントの誤り（CS1570/1572/1573/1574/1734）はビルドエラー
- Clean Architecture（Robert C. Martin）

---

## ⚠️ 検討中のドキュメント

`docs/Assistance/Consider/` ディレクトリ内のドキュメントは**検討中および進行中の項目**です。

❌ **参照しないでください** — 実装やコードレビュー時の参考にしないこと  
❌ **参考資料ではありません** — 不完全または暫定的な内容を含みます  
❌ **修正対象外** — ドキュメント齟齬調査対象外です  

詳細は `docs/Assistance/Consider/README.md` を参照。

---

## 🚀 CI/CD での自動検証

依存関係の遵守は現状コードレビューに依存している。`NetArchTest.Rules` を使った型レベルの検証テスト例は [CLEAN_ARCHITECTURE_GUIDELINES.md の「自動検証の導入」](docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md#自動検証の導入) を参照。特に「Presentation → Infrastructure は Composition Root（`Program.cs` / WPF の `App.xaml.cs`）のみ」は `.csproj` の `ProjectReference` だけでは強制できないため、このテストでの担保が必須。

---

## 📝 更新履歴

| 日付 | 更新内容 |
|---|---|
| 2026-09-26 | ①Composition Root を「`Program.cs`（WPF は `App.xaml.cs`）」に明確化。②「DateTime は Infrastructure の内側に閉じる」を追加：Domain / SharedKernel / Application の公開メンバーに DateTime を持ち込まない（値オブジェクトの `FromDbValue(DateTime)` などの DB 型変換メソッドも同様。例外なし）。変換は Mapper / Repository が行い、値オブジェクトは `TryFrom(LocalDateTime?)` のみ。現状のコードは移行中（[実装計画](docs/Assistance/Plans/20260926_原則完全準拠_実装計画.md) フェーズ 4） |
| 2026-09-06 | Context間のデータ共有パターンを追加。ジェネリック Query Service `IQueryService<TAggregate, TId>` パターンを採用。複数Contextがリアルタイムにドメインモデル情報にアクセスするための標準パターン。汎用層肥大化を防止 |
| 2026-07-31（後）| Application層の依存関係表を修正。汎用Application層と Bounded Context別Application層の区別を明記。「Application（Context別）→ Application（汎用層）」が IUseCase 実装パターンとして許可されることを追記 |
| 2026-07-31 | CLEAN_ARCHITECTURE_GUIDELINES.md の実コードとの不一致修正に合わせて本ファイルも修正。Domain/Common/SharedKernel の依存関係表を実装に合わせて訂正、Crosscutting→Infrastructure禁止を明記、SlnArch（未検証）の記述をNetArchTest.Rulesへの参照に置き換え |
| 2026-07-30 | LocalDateTime 使用規則を追加。全層で IClock 経由の LocalDateTime 使用を明確化 |
| 2026-07-09 | 初版作成。クリーンアーキテクチャ原則と新規プロジェクトチェックリスト |

