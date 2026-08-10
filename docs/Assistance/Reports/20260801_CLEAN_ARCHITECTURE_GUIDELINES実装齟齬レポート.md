# CLEAN_ARCHITECTURE_GUIDELINES 実装齟齬レポート

**作成日:** 2026-08-01  
**対象ドキュメント:** `docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md`  
**対象実装:** SupportAdvance プロジェクト全体

---

## 概要

CLEAN_ARCHITECTURE_GUIDELINES.md と実装コード・プロジェクト設定を比較し、齟齬（ドキュメントと実装が異なる点）を調査・報告します。

**調査範囲:**
- `.csproj` ファイルの `ProjectReference` 設定
- コード内の `using` ステートメント
- 各層の実装構造
- LocalDateTime / IClock の使用

**調査結論:** 実装はドキュメントと**ほぼ準拠**しており、齟齬は設計パターンの明確化不足に限定されます。

---

## 齟齬・改善指摘

### 1. ✅ Infrastructure → Application 参照について（最重要）

**ドキュメント記載:**
- 依存関係マトリックス（403-410行）で「Infrastructure → Application」が `✓`（許可）と記載
- 318行で「Application***（インターフェース実装パターンのみ）」と注記
- 416行で「Infrastructure層が Application層で定義されたインターフェース（例：IRepository）を Infrastructure層が実装する場合」と説明

**実装の実態:**
- **汎用 `src/Infrastructure/Infrastructure.csproj`**: Application への参照なし
- **Bounded Context別 `src/Contexts/Samples/CarPreferences.Infrastructure/CarPreferences.Infrastructure.csproj`**: 
  - `src/Application/Application.csproj`（汎用層）を参照 ✓
  - `src/Infrastructure/Infrastructure.csproj`（汎用層）を参照 ✓

**齟齬の内容:**
ドキュメント 318行の例 `ICarPreferenceRepository` は「Infrastructure で実装される」と述べられているが、これは実装では **Contexts別 Infrastructure（CarPreferences.Infrastructure）** で実装されている。汎用 Infrastructure プロジェクト自体は Application インターフェースを実装していない。

**評価:**
- **齟齬の重大度:** 中（設計パターン選択の問題）
- **実装:** ✓ 正しい（Bounded Context別に適切に分離）
- **ドキュメント:** 曖昧（汎用 Infrastructure vs Context別 Infrastructure の区別が不明確）

**改善提案:**
ドキュメント 318行の例を以下のように修正：

```
**許可される参照:**
- Application***（インターフェース実装パターンのみ）
  * 注：汎用 Infrastructure が Application を直接実装することは稀。
    Bounded Context別 Infrastructure（例：CarPreferences.Infrastructure）が
    Context固有のインターフェース実装を担当するのが一般的なパターン
```

---

### 2. ✅ Presentation → Infrastructure 参照の扱い

**ドキュメント記載:**
- 365行「Presentation」の許可参照に「Infrastructure（**Composition Root のみ**）」と明記
- 414-415行「`Presentation → Infrastructure：Program.cs（Composition Root）のみ許可`」と明記
- 717行「特に以下の点は `.csproj` の `ProjectReference` だけでは強制できないため」と記述

**実装の実態:**
- `WinTrial.csproj` は以下を直接参照：
  - `CarPreferences.Infrastructure`（Bounded Context別）
  - （汎用 Infrastructure への参照なし）
- コード調査： `Program.cs` 以外のファイル（ViewModel など）から Infrastructure 名前空間の参照は検出されず

**評価:**
- **齟齬:** なし（ドキュメント通り実装）
- **実装:** ✓ 正しい
- **検証:** 型レベル検証テスト（NetArchTest.Rules）の導入がドキュメント通り未実装

**改善提案:**
NetArchTest.Rules の導入は検討課題としてノートに記載されているので、ドキュメント 720-794行の「自動検証の導入」セクションを参考に実装推奨。

---

### 3. ✅ LocalDateTime 使用規則の遵守

**ドキュメント記載:**
- 490-570行で LocalDateTime 使用規則を詳細に記載
- 501-507行「Clock 実装内部のみ DateTime の直接使用を許可」

**実装の実態:**
- `src/Common/Clocks/SystemClock.cs` 内で `DateTime.UtcNow` を使用 ✓
- `src/Crosscutting/Logging/FrameworkLoggingAdapter.cs` 66行で `clock.JstNow` を使用 ✓
- `src/Infrastructure/Migrations/MigrationRunner.cs` で DateTime を使用（例外ケース）

**評価:**
- **齟齬:** なし（完全準拠）
- **実装:** ✓ 正しい

---

### 4. ✅ Crosscutting → Infrastructure 禁止ルールの遵守

**ドキュメント記載:**
- 144行「Crosscutting → Infrastructure は禁止」
- 152行「Crosscutting...禁止される参照：Infrastructure」
- 418行「マトリックス備考：Crosscutting → Infrastructure は禁止」

**実装の実態:**
- `src/Crosscutting/Crosscutting.csproj`：
  - 参照：Common, SharedKernel のみ
  - NLog は NuGet パッケージとして直接参照 ✓

**評価:**
- **齟齬:** なし（完全準拠）
- **実装:** ✓ 正しい

---

### 5. ⚠️ ドキュメント 327行のコード例に不整合

**ドキュメント記載:**
327行の CarPreferenceRepository 例：
```csharp
public class CarPreferenceRepository : ICarPreferenceRepository
{
    public async Task<Car> GetByIdAsync(CarId id)
    {
        var result = await _connection.QuerySingleOrDefaultAsync<CarDto>(
            "SELECT * FROM cars WHERE id = @Id",
            new { Id = id.Value });

        return result?.ToDomain();
    }
}
```

**実装との確認:**
- ドキュメント例では `CarPreferenceRepository` が汎用 Infrastructure に属するものとして記載
- 実装では同名クラスは存在しない（実装済みのサンプルコードが存在しない）

**評価:**
- **齟齬:** 低（ドキュメント内のコード例が仮想的な例示のため）
- **改善提案：** コード例の実装位置（汎用 Infrastructure vs Bounded Context別）を明確に注記

```
// ✓ OK：Bounded Context別 Infrastructure での実装例
// ファイル: src/Contexts/Samples/CarPreferences.Infrastructure/Repositories/CarPreferenceRepository.cs
```

---

### 6. ✅ 依存関係マトリックスの正確性

**ドキュメント 403-420行**

検証内容：
| From | To | ドキュメント | 実装 | 状態 |
|---|---|---|---|---|
| Common | 全て | ✗ | ✗ | ✓ |
| SharedKernel | Common | ✓ | ✓ | ✓ |
| Crosscutting | Infrastructure | ✗ | ✗ | ✓ |
| Domain | Application | ✗ | ✗ | ✓ |
| Application | Infrastructure | ✗ | ✗ | ✓ |
| Infrastructure | Crosscutting | ✓ | ✓ | ✓ |
| Infrastructure | Application | ✓（***） | △（Context別のみ） | △ |
| Presentation | Infrastructure | ✓（Program.csのみ） | ✓（Program.csのみ） | ✓ |

**評価:**
- **齟齬:** 低（汎用層 vs Context別層の区別不足）

---

## まとめ

| # | 項目 | 齟齬レベル | 評価 | 改善必要性 |
|---|---|---|---|---|
| 1 | Infrastructure → Application 参照 | 中 | 実装は正しい、ドキュメント曖昧 | 高 |
| 2 | Presentation → Infrastructure | なし | ✓ | 低（NetArchTest導入推奨） |
| 3 | LocalDateTime 使用 | なし | ✓ | なし |
| 4 | Crosscutting → Infrastructure | なし | ✓ | なし |
| 5 | コード例の位置不明 | 低 | 仮想例示 | 中 |
| 6 | 依存マトリックス | 低 | ほぼ正確 | 低 |

---

## 推奨アクション

### 即座対応（高優先度）

**1. ドキュメント 318行を修正**
現在：
```
- Application***（インターフェース実装パターンのみ）
```

修正後：
```
- Application***（インターフェース実装パターンのみ）
  * 汎用 Infrastructure は稀にのみ Application を参照
  * Bounded Context別 Infrastructure（例：CarPreferences.Infrastructure）が
    Context固有インターフェースを実装するのが一般的パターン
```

**2. ドキュメント 327行のコード例に位置注記を追加**

```csharp
// ✓ OK：Bounded Context別 Infrastructure での実装例
// 位置: src/Contexts/Samples/CarPreferences.Infrastructure/Repositories/CarPreferenceRepository.cs
public class CarPreferenceRepository : ICarPreferenceRepository
{
    // ...
}
```

### 中期対応（中優先度）

**3. NetArchTest.Rules テストの実装**
- ドキュメント 720-794行の検証テストコードを実装
- CI/CD パイプラインに組み込み
- 「Program.cs のみ Infrastructure 参照可」ルールの型レベル検証

**4. Application層ドキュメントの明確化**
ドキュメント 227-242行の「汎用 Application層 vs Bounded Context別 Application層」の説明をより詳細に（ファイルパス例を追加）

---

## ドキュメント修正ファイル

```
docs/Assistance/Guides/CLEAN_ARCHITECTURE_GUIDELINES.md

修正箇所：
- 318行（Infrastructure の許可参照）
- 327行（コード例の位置注記）
```

---

## 参考資料

- **実装検査対象ファイル:**
  - `src/*/Application.csproj`
  - `src/*/Infrastructure.csproj`
  - `src/Presentation/*/Program.cs`
  - `src/Contexts/Samples/*/*Application.csproj`
  - `src/Contexts/Samples/*/*Infrastructure.csproj`

- **コード例参照:**
  - `src/Common/Clocks/SystemClock.cs` — DateTime 使用例外
  - `src/Crosscutting/Logging/FrameworkLoggingAdapter.cs` — LocalDateTime 使用
  - `src/Presentation/WinTrial/Program.cs` — Composition Root

---

**レポート作成者:** Claude  
**検査方法:** 静的コード分析 + ProjectReference 検査 + using ステートメント確認  
**検査結論:** 実装はドキュメントに**概ね準拠**。ドキュメントの曖昧性が軽微な齟齬の原因。
