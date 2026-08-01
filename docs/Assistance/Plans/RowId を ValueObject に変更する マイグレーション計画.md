# RowId を ValueObject に変更する マイグレーション計画

**目的**: Entity → Entity へのステップバイステップマイグレーション
**範囲**: SharedKernel, 全 Bounded Context, テスト
**リスク**: 高（大規模変更、逆方向影響あり）

------

## 📋 Phase 1: 計画・設計

### 1.1 RowId ValueObject の仕様定義

**仕様案:**

```csharp
// src/SharedKernel/ValueObjects/RowId.cs
public sealed class RowId : ValueObject, IEquatable<RowId>
{
    public long Value { get; }

    private RowId(long value)
    {
        if (value <= 0)
            throw new ArgumentException("RowId must be greater than 0", nameof(value));
        Value = value;
    }

    public static RowId From(long value) => new(value);
    public static RowId New() => new(0);  // DB生成用

    // ValueObject 実装
    public override bool Equals(object? obj) => Equals(obj as RowId);
    public bool Equals(RowId? other) => other != null && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => $"RowId({Value})";

    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }
}
```

**決定項目:**

-  0（未採番）は許可するか？
-  `New()` ファクトリメソッドの名前
-  `From()` vs `Create()` メソッド名

------

### 1.2 影響範囲の確認

**影響を受けるファイル:**

| カテゴリ         | 影響範囲                          | ファイル数 |
| :--------------- | :-------------------------------- | :--------- |
| **Entity 宣言**  | AggregateRoot → AggregateRoot     | ~10+       |
| **イベント定義** | long → RowId パラメータ           | ~15+       |
| **Repository**   | Entity → Entity                   | ~8+        |
| **DataAccess**   | long rowId → RowId型              | ~8+        |
| **ドキュメント** | すべての例を更新                  | ~15+       |
| **テスト**       | TestId 並行維持 or TestRowId 作成 | ~6+        |

**潜在的な問題:**

- ❌ ORM マッピング（Dapper/RepoDb が long → RowId 自動変換）
- ❌ DataAccess層での long ↔ RowId 変換
- ❌ DB マイグレーション（row_id は long のまま）

------

### 1.3 マイグレーション戦略

**推奨アプローチ: 段階的マイグレーション**

```
Week 1-2: SharedKernel に RowId 実装
Week 3-4: Entity<RowId> に変更（コンパイラ駆動修正）
Week 5: ドキュメント・テスト更新
Week 6: 検証・統合テスト
```

**代替案: 並行実装**

- Entity で汎用のまま（RowId は使用側が決定）
- 新規 BC から RowId を使用
- 既存 BC は段階的に移行

------

## 🛠️ Phase 2: 実装準備

### 2.1 RowId ValueObject 実装

**ファイル作成:**

```
src/SharedKernel/ValueObjects/RowId.cs
```

**作業:**

-  RowId.cs を SharedKernel/ValueObjects/ に作成
-  IEquatable, ValueObject 実装
-  ファクトリメソッド実装
-  Dapper/RepoDb グローバルマッピング確認

**検証:**

-  ビルド成功
-  RowId で Entity 宣言できるか確認

------

### 2.2 テスト用 RowId の準備

**オプション A: TestRowId を作成**

```csharp
// tests/SharedKernel.Tests/Entities/Fixtures/TestRowId.cs
public sealed class TestRowId : ValueObject
{
    public long Value { get; }
    
    public TestRowId(long value = 1)
    {
        Value = value;
    }
    
    // ValueObject 実装
}
```

**オプション B: TestId をそのまま使用**

- Entity でテスト継続
- 本番は Entity

**決定:** [ ] オプション A / [ ] オプション B

------

## 🔄 Phase 3: コード更新

### 3.1 SharedKernel の Entity 宣言を変更

**Entity のままか、Entity に特化するか:**

**案1: Entity に特化**

```csharp
public abstract class Entity : IEquatable<Entity>
{
    public RowId Id { get; protected set; }
    // DomainEvents 管理...
}
```

**案2: Entity を保持（汎用性）**

```csharp
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; }
    // ...
}

// 使用者は Entity<RowId> で宣言
```

**推奨**: 案2（汎用性を保つ）

**作業:**

-  変更なし（Entity のまま）
-  例ドキュメントで Entity を使用

------

### 3.2 Entity 継承側の更新（BC 別）

**CarPreferences.Domain 例:**

```csharp
// Before
public class UserPreferences : AggregateRoot<long>
{
    public long Id { get; protected set; }
}

// After
public class UserPreferences : AggregateRoot<RowId>
{
    public RowId Id { get; protected set; }
}
```

**作業チェックリスト:**

| BC             | ファイル           | 変更内容        |
| :------------- | :----------------- | :-------------- |
| CarPreferences | UserPreferences.cs | Entity → Entity |
| (他の BC)      | （未実装）         | （後日）        |

**順序:**

1. CarPreferences を完全に移行（最初の検証ポイント）
2. 他の BC を順次移行

------

### 3.3 イベント定義の更新

**Before:**

```csharp
public class PreferencesUpdatedEvent : IDomainEvent
{
    public long RowId { get; }
}
```

**After:** (変更なし)

- イベントのプロパティ型は long のまま（データベース整合性）
- または AggregateRootId: RowId に変更

**決定:** [ ] long のまま / [ ] RowId に変更

------

### 3.4 DataAccess 層の型変換

**問題**: RepositoryBase が long RowId を期待

```csharp
// Before
public class RepositoryBase<TEntity, TDbModel>
{
    protected TEntity MapToDomain(TDbModel dbModel, long rowId)
    {
        // ...
    }
}

// After
public class RepositoryBase<TEntity, TDbModel>
{
    protected TEntity MapToDomain(TDbModel dbModel, RowId rowId)
    {
        // dbModel.RowId (long) → RowId 変換
    }
}
```

**作業:**

-  RepositoryBase<TEntity, TDbModel> の Entity 型パラメータを確認
-  rowId パラメータの型を long → RowId に変更
-  呼び出し側（Repository実装）の更新

------

## 📚 Phase 4: ドキュメント更新

### 4.1 技術仕様書の更新

**ファイル:** Entity_And_DomainEvents_技術仕様.md

**更新内容:**

-  Entity → Entity
-  RowId ValueObject の説明追加
-  ファクトリメソッド (From, New) の使用例追加

------

### 4.2 実装ガイドラインの更新

**ファイル:** Entity_設計ガイドライン.md, Bounded_Context_テンプレート.md

**更新内容:**

-  Entity の宣言方法
-  RowId の生成・変換方法
-  ビジネス識別子との分離（重要）

------

### 4.3 テスト仕様書の更新

**ファイル:** Entity_And_DomainEvents_単体テスト仕様.md

**更新内容:**

-  TestRowId の説明追加（選択した場合）
-  Entity でのテスト例
-  RowId のテストケース

------

## ✅ Phase 5: 検証・テスト

### 5.1 ビルド検証

```bash
dotnet build --no-incremental
```

**確認項目:**

-  コンパイルエラーなし
-  警告なし
-  型推論が正しく機能

------

### 5.2 単体テスト実行

```bash
dotnet test tests/SharedKernel.Tests/
```

**確認項目:**

-  Entity テスト（16 テストケース） パス
-  ValueObject テスト パス
-  RowId 固有テスト追加（検証範囲など）

------

### 5.3 統合テスト

**CarPreferences.Domain テスト:**

```bash
dotnet test tests/CarPreferences.Domain.Tests/
```

**確認項目:**

-  UserPreferences の Entity テスト パス
-  Repository マッピング テスト パス
-  イベント発行 テスト パス

------

### 5.4 依存関係検証

**ルール確認:**

-  RowId が SharedKernel に配置されている
-  RowId への逆向き依存がない（Common ← RowId は禁止）
-  循環参照がない

```bash
# 簡易チェック
grep -r "using.*RowId" src/ | grep -v "SharedKernel"
```

------

## 🎯 Phase 6: リスク・緩和

### 潜在的な問題と対応

| 問題           | 影響                                           | 対応                         |
| :------------- | :--------------------------------------------- | :--------------------------- |
| ORM マッピング | Dapper/RepoDb が long → RowId 自動変換できない | カスタム型マッピング実装     |
| DataAccess 層  | dbModel.RowId (long) vs Entity.Id (RowId)      | 変換ロジック集約             |
| 既存 BC        | UserPreferences など Entity 宣言変更必須       | 段階的移行（1 BC ずつ）      |
| パフォーマンス | ValueObject 생성オーバーヘッド                 | 無視（マイクロ最適化は後日） |

------

## 📝 実装チェックリスト

### RowId ValueObject 実装

-  src/SharedKernel/ValueObjects/RowId.cs 作成
-  IEquatable, ValueObject 実装
-  From(), New() ファクトリメソッド
-  Dapper/RepoDb 型マッピング登録

### Entity 更新

-  Entity 宣言のまま（汎用性保持）
-  Example ドキュメント: Entity → Entity

### CarPreferences マイグレーション

-  UserPreferences.cs: AggregateRoot → AggregateRoot
-  PreferencesUpdatedEvent など: long RowId → RowId
-  UserPreferencesRepository.cs 更新
-  ビルド・テスト成功

### ドキュメント更新

-  Entity_And_DomainEvents_技術仕様.md
-  Entity_設計ガイドライン.md
-  Bounded_Context_テンプレート.md
-  相互リンク確認

### テスト・検証

-  dotnet build 成功
-  dotnet test 成功
-  依存関係検証完了

------

## ⏱️ 推定工数

| Phase                     | 推定日数    | 優先度 |
| :------------------------ | :---------- | :----- |
| Phase 1: 計画・設計       | 1 日        | 🔴 高   |
| Phase 2: 実装準備         | 1-2 日      | 🔴 高   |
| Phase 3: コード更新       | 3-5 日      | 🔴 高   |
| Phase 4: ドキュメント更新 | 1-2 日      | 🟡 中   |
| Phase 5: 検証・テスト     | 2-3 日      | 🔴 高   |
| **計**                    | **8-13 日** |        |

------

## 🚦 ゴーノーゴー基準

**Go（実装開始）:**

-  RowId 仕様が確定
-  テスト戦略が決定
-  段階的移行計画が承認
-  リスク緩和策が準備完了

**No-Go（保留）:**

-  RowId 仕様に合意なし
-  ORM マッピング方法が未解決
-  他の優先タスクが重い

------

## 🔗 参考資料

- Entity_And_DomainEvents_技術仕様.md
- ValueObject の実装ガイド
- DDD パターン：RowId as Value Object

------

## 📌 次のステップ

1. **Phase 1 確認**: RowId 仕様に同意するか確認
2. **Phase 2 実装**: RowId ValueObject を作成
3. **Phase 3 開始**: CarPreferences から順次マイグレーション
4. **検証**: 各フェーズ後にビルド・テスト実施