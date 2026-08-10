# Phase 1: 仕様検討結果 - 実施者情報実装

**フェーズ:** Phase 1（仕様検討）  
**開始日:** 2026-07-19  
**完了予定:** 2026-07-19  
**ステータス:** 進行中  

---

## 📋 Phase 1 の目標

実施者情報（CreatedBy/UpdatedBy/DeletedBy）の実装に向け、以下の項目を確定する：

1. ICurrentUser インターフェース設計
2. CreatedBy/UpdatedBy/DeletedBy の設計
3. Entity 統合パターン設計
4. テスト戦略検討
5. 仕様書構成の確定

---

## 🎯 検討結果

### 1. ValueObject の名前確定

**決定: CreatedBy / UpdatedBy / DeletedBy**

```csharp
// 推奨パターン: 短く、役割が明確
public sealed class CreatedBy : PrimitiveValueObject<long> { }
public sealed class UpdatedBy : PrimitiveValueObject<long> { }
public sealed class DeletedBy : PrimitiveValueObject<long?> { }
```

**理由:**
- 既存の CreatedAt/UpdatedAt/DeletedAt と命名法を統一
- "誰が" という質問に直答する最小限の命名
- 実際には UserId を保持（User詳細は ICurrentUser で参照）

**候補（却下理由）:**
- `CreatedByUser`: 冗長（ValueObject なので前後で Userはわかる）
- `Creator`: CreatedAt/UpdatedAt との命名法と不統一
- `CreatedByActor`: Agent システムとの混同の恐れ

---

### 2. ICurrentUser インターフェース設計

**決定:**

```csharp
namespace SupportAdvance.SharedKernel.Abstractions;

/// <summary>
/// 現在実行中のユーザー情報を提供するインターフェース
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// ユーザー ID（必須）
    /// </summary>
    long UserId { get; }
    
    /// <summary>
    /// ユーザー名（必須）
    /// </summary>
    string UserName { get; }
    
    /// <summary>
    /// メールアドレス（オプション、主に表示用）
    /// </summary>
    string? Email { get; }
}
```

**設計上の判断:**

| プロパティ | 必須 | 理由 |
|---|---|---|
| **UserId** | ✓ | PK として一意に特定可能 |
| **UserName** | ✓ | 監査ログ表示用（読みやすさ） |
| **Email** | ✗ | 環境により存在しない可能性あり |
| **Role** | ✗ | 監査目的には不要（権限は別層で管理） |
| **Department** | ✗ | 監査目的には不要（組織構造は別で管理） |

**推奨:**
- Email はオプション（null許容）にすることで、外部システム連携時の柔軟性を確保
- Role/Department は含めない（関心の分離：ユーザー認証 vs 権限管理）

---

### 3. CreatedBy/UpdatedBy/DeletedBy の設計

#### 3.1 基本設計

```csharp
// CreatedBy と UpdatedBy: UserId 必須
public sealed class CreatedBy : PrimitiveValueObject<long>
{
    public static CreatedBy From(long userId) => new(userId);
    public long Value => ValueField;
}

public sealed class UpdatedBy : PrimitiveValueObject<long>
{
    public static UpdatedBy From(long userId) => new(userId);
    public long Value => ValueField;
}

// DeletedBy: UserId null許容（未削除時は null）
public sealed class DeletedBy : PrimitiveValueObject<long?>
{
    public static DeletedBy From(long userId) => new(userId);
    public static DeletedBy NotDeleted() => new(null);
    
    public long? Value => ValueField;
    public bool IsDeleted => ValueField.HasValue;
}
```

#### 3.2 ライフサイクル比較

| 段階 | CreatedBy | UpdatedBy | DeletedBy |
|---|---|---|---|
| **初期化** | Entity 生成時に一度だけ設定 | Entity 生成時に初期化 | Entity 生成時に未削除状態 |
| **値の由来** | `_currentUser.UserId` | `_currentUser.UserId` | 削除操作時に `_currentUser.UserId` |
| **更新頻度** | なし（完全不変） | 変更のたびに更新 | 最大1回（削除時のみ） |
| **変更可能性** | 不可 | 可能 | 実質不可 |

#### 3.3 Entity への統合パターン

```csharp
public class Entity
{
    private readonly ICurrentUser _currentUser;
    
    // Entity コンストラクタで DI 受け取り
    public Entity(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
        CreatedBy = CreatedBy.From(_currentUser.UserId);
        UpdatedBy = UpdatedBy.From(_currentUser.UserId);
        DeletedBy = DeletedBy.NotDeleted();
    }
    
    // 読み取り専用プロパティ
    public CreatedBy CreatedBy { get; }
    public UpdatedBy UpdatedBy { get; private set; }
    public DeletedBy DeletedBy { get; private set; }
    
    // 更新時に UpdatedBy を更新
    public void Update(string name)
    {
        Name = name;
        UpdatedBy = UpdatedBy.From(_currentUser.UserId);
    }
    
    // 削除時に DeletedBy を設定
    public void SoftDelete()
    {
        if (!DeletedBy.IsDeleted)
        {
            DeletedBy = DeletedBy.From(_currentUser.UserId);
        }
    }
}
```

**重要ポイント:**
- ICurrentUser は IClock と同じく、Entity の DI パターン
- CreatedBy は一度設定されると変更不可
- UpdatedBy は Entity 変更のたびに更新
- DeletedBy は削除時のみ設定（復旧で NotDeleted() に戻す）

---

### 4. デフォルト実施者情報の設計

#### 4.1 ICurrentUser の実装例

```csharp
// 本番環境：HTTP コンテキストから取得
public class HttpContextCurrentUser : ICurrentUser
{
    private readonly HttpContext _context;
    
    public HttpContextCurrentUser(HttpContext context)
    {
        _context = context;
    }
    
    public long UserId => long.Parse(_context.User.FindFirst("sub")?.Value 
        ?? throw new InvalidOperationException("User ID not found"));
    
    public string UserName => _context.User.Identity?.Name 
        ?? throw new InvalidOperationException("Username not found");
    
    public string? Email => _context.User.FindFirst("email")?.Value;
}

// テスト環境：固定値を返す
public class MockCurrentUser : ICurrentUser
{
    public long UserId { get; set; }
    public string UserName { get; set; }
    public string? Email { get; set; }
}

// システム処理（バッチなど）：特殊ユーザー
public static class SystemUser
{
    public const long Id = 0;  // System ユーザーは常に ID=0
    public const string Name = "System";
}
```

#### 4.2 各環境での対応

| 環境 | ICurrentUser 実装 | UserId | UserName | Email |
|---|---|---|---|---|
| **本番環境（Web）** | HttpContextCurrentUser | 認証ユーザーID | 認証ユーザー名 | 認証ユーザーのメール |
| **バッチ処理** | 明示的に注入 | 0（System）| "System" | null |
| **テスト** | MockCurrentUser | 1（テスト用） | "TestUser" | "test@example.com"（任意） |
| **未認証** | エラーまたは Anonymous | -1（Anonymous） | "Anonymous" | null |

**推奨:**
- バッチ/システム処理では明示的に `ICurrentUser` の実装を用意
- 未認証時はエラーとする（or Anonymous ユーザーを定義）

---

### 5. テスト戦略の検討

#### 5.1 単体テスト観点

| テスト項目 | CreatedBy | UpdatedBy | DeletedBy |
|---|---|---|---|
| **生成テスト** | From(userId) | From(userId) | From(userId) / NotDeleted() |
| **等価性テスト** | 同じ userId → 等価 | 同じ userId → 等価 | 同じ userId → 等価（null 同士も等価） |
| **不変性テスト** | 値を変更不可 | 値を変更可 | 実質不変 |
| **削除状態テスト** | — | — | IsDeleted プロパティ |

#### 5.2 Entity 統合テスト

```csharp
[TestFixture]
public class EntityActorTests
{
    private MockCurrentUser _currentUser;
    
    [SetUp]
    public void SetUp()
    {
        _currentUser = new MockCurrentUser 
        { 
            UserId = 123,
            UserName = "TestUser",
            Email = "test@example.com"
        };
    }
    
    [Test]
    public void Create_Sets_CreatedBy_And_UpdatedBy()
    {
        var entity = new TestEntity(_currentUser);
        
        Assert.That(entity.CreatedBy.Value, Is.EqualTo(123));
        Assert.That(entity.UpdatedBy.Value, Is.EqualTo(123));
        Assert.That(entity.DeletedBy.IsDeleted, Is.False);
    }
    
    [Test]
    public void Update_Changes_UpdatedBy()
    {
        var entity = new TestEntity(_currentUser);
        var originalCreatedBy = entity.CreatedBy;
        
        _currentUser.UserId = 456;  // 別のユーザーに切り替え
        entity.Update("new value");
        
        Assert.That(entity.CreatedBy, Is.EqualTo(originalCreatedBy));  // 変わらない
        Assert.That(entity.UpdatedBy.Value, Is.EqualTo(456));  // 更新される
    }
    
    [Test]
    public void Delete_Sets_DeletedBy()
    {
        var entity = new TestEntity(_currentUser);
        entity.SoftDelete();
        
        Assert.That(entity.DeletedBy.IsDeleted, Is.True);
        Assert.That(entity.DeletedBy.Value, Is.EqualTo(123));
    }
}
```

---

### 6. 依存関係の確認

**Phase 2 で実装する際の依存関係チェック:**

```bash
# ICurrentUser の参照確認（SharedKernel に定義予定）
grep -r "ICurrentUser" src/

# CreatedBy/UpdatedBy/DeletedBy の参照確認
grep -r "CreatedBy\|UpdatedBy\|DeletedBy" src/

# Entity での使用確認
grep -r "public.*CreatedBy\|public.*UpdatedBy\|public.*DeletedBy" src/
```

---

## ✅ 確定項目チェック

- [x] ValueObject の名前: **CreatedBy / UpdatedBy / DeletedBy** に確定
- [x] ICurrentUser のプロパティ: **UserId, UserName, Email(optional)** に確定
- [x] Entity 統合パターン: **IClock と同じ DI パターン** で統一
- [x] テスト戦略: **MockCurrentUser** ベースで実装
- [x] デフォルト実施者: **SystemUser(Id=0)** をシステム処理用に定義

---

## 📊 仕様書構成（Phase 2 予定）

### 8 個の技術仕様書・詳細設計書

| ドキュメント | ステータス | 説明 |
|---|---|---|
| **ICurrentUser_技術仕様書.md** | ⏳ Phase 2 | インターフェース定義、責務、使用パターン |
| **ICurrentUser_詳細設計書.md** | ⏳ Phase 2 | 実装例（HttpContextCurrentUser, MockCurrentUser） |
| **CreatedBy_技術仕様書.md** | ⏳ Phase 2 | ValueObject 定義、ファクトリメソッド |
| **CreatedBy_詳細設計書.md** | ⏳ Phase 2 | 実装詳細、Entity への統合 |
| **UpdatedBy_技術仕様書.md** | ⏳ Phase 2 | ValueObject 定義、可変性 |
| **UpdatedBy_詳細設計書.md** | ⏳ Phase 2 | 実装詳細、更新メカニズム |
| **DeletedBy_技術仕様書.md** | ⏳ Phase 2 | ValueObject 定義（null許容）、IsDeleted プロパティ |
| **DeletedBy_詳細設計書.md** | ⏳ Phase 2 | 実装詳細、削除・復旧メカニズム |

### 3 個のテスト仕様書

| ドキュメント | ステータス | 説明 |
|---|---|---|
| **ICurrentUser_単体テスト仕様書.md** | ⏳ Phase 3 | インターフェース実装のテスト観点 |
| **CreatedBy_単体テスト仕様書.md** | ⏳ Phase 3 | ValueObject の不変性テスト |
| **UpdatedBy_DeletedBy_単体テスト仕様書.md** | ⏳ Phase 3 | 可変性・削除状態のテスト |

---

## 🔗 参考資料

- **既존 仕様書**: docs/SharedKernel/ValueObjects/Audit/Audit_比較表.md
- **Entity DI パターン**: docs/SharedKernel/ValueObjects/Audit/CreatedAt_詳細仕様書.md

---

## 📝 次のステップ

**Phase 2: 仕様書作成（3-4日予定）**

1. ICurrentUser インターフェース仕様書を作成
2. CreatedBy/UpdatedBy/DeletedBy の技術仕様書を作成
3. 各詳細設計書を作成
4. 比較表を更新

**実装予定:** Phase 2 完了後、実装に着手
