# SharedKernel 責務定義 - SupportAdvance

**最終更新**: 2026-06-30

---

## 目次

1. [SharedKernel の位置づけ](#sharedkernel-の位置づけ)
2. [責務分類](#責務分類)
3. [現在のファイル構成](#現在のファイル構成)
4. [各要素の使用ルール](#各要素の使用ルール)
5. [Context固有のDomain](#context固有のdomain)
6. [設計パターン](#設計パターン)

---

## SharedKernel の位置づけ

### 定義

**SharedKernel**は、複数のContext間で共有される **Domain層の基盤** です。

```
Domain層
├─ SharedKernel      ← 全Context共通
│  ├─ Entity.cs      ← エンティティ基底
│  ├─ ValueObject.cs ← 値オブジェクト基底
│  └─ DomainEvent.cs
│
└─ Context別Domain
   ├─ CarPreferences.Domain
   │  ├─ CarPreference.cs      (SharedKernel.Entity継承)
   │  └─ PreferenceCategory.cs (SharedKernel.ValueObject継承)
   │
   └─ [NewContext].Domain
      └─ ...
```

### 参照可能性

```
SharedKernel
  → Common (設定・時刻のみ)
  → (他プロジェクト参照なし)

Context別 Domain
  → SharedKernel (基底クラス継承)
  → Common
  → (他Context参照なし)
```

### 許可されるもの・されないもの

```
✅ 許可:
  - エンティティ基底クラス
  - 値オブジェクト基底クラス
  - ドメインイベント基底
  - ビジネス例外基底

❌ 禁止:
  - Context固有のビジネスルール実装
  - Repository参照
  - Infrastructure参照
  - Application参照
```

---

## 責務分類

### 層別の責務

```
┌─────────────────────────────────────────────┐
│ SharedKernel（共有基盤）                   │
├─────────────────────────────────────────────┤
│
│ 1. エンティティ基盤
│    ├─ Entity.cs           ← ID・等値比較
│    └─ ValueObject.cs      ← 値の等値比較
│
│ 2. ドメインイベント基盤
│    └─ DomainEvent.cs      ← イベント発行・購読
│
│ 3. ビジネス例外基盤
│    └─ DomainException.cs  ← Domain共通例外
│
│ 4. ドメイン値（共有される）
│    └─ (頻出する値・定数)
│
└─────────────────────────────────────────────┘

        ↓ 継承・実装

┌─────────────────────────────────────────────┐
│ Context別 Domain（具体実装）               │
├─────────────────────────────────────────────┤
│
│ CarPreferences.Domain
│ ├─ Entities/
│ │  └─ CarPreference : Entity          ← 継承
│ ├─ ValueObjects/
│ │  └─ PreferenceCategory : ValueObject ← 継承
│ └─ DomainEvents/
│    └─ CarPreferenceUpdatedEvent : DomainEvent ← 継承
│
└─────────────────────────────────────────────┘
```

---

## 現在のファイル構成

```
SupportAdvance/src/SharedKernel/
├─ 保存領域/                   ← Concept
├─ Entities/
│  └─ Entity.cs                (✅ 基底クラス)
├─ ValueObjects/
│  └─ ValueObject.cs           (✅ 基底クラス)
├─ DomainEvents/
│  └─ DomainEvent.cs           (✅ 基底クラス)
├─ Exceptions/
│  └─ DomainException.cs       (✅ 共通例外)
└─ SharedKernel.csproj
```

### 各ファイルの責務

| ファイル | 責務 | 参照先 |
|---------|------|-------|
| **Entity.cs** | エンティティ基底 | - |
| **ValueObject.cs** | 値オブジェクト基底 | - |
| **DomainEvent.cs** | ドメインイベント基底 | - |
| **DomainException.cs** | ビジネス例外 | - |

---

## 各要素の使用ルール

### 1. エンティティ基底クラス（Entity）

**目的**: ID による等値比較、ライフサイクル管理

**定義**:
```csharp
public abstract class Entity
{
    public int Id { get; protected set; }
    
    // ID による等値比較
    public override bool Equals(object? obj)
    {
        return obj is Entity entity && entity.Id == this.Id;
    }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
```

**使用方法**:
```csharp
// ✅ 正しい例：SharedKernel.Entity を継承

// CarPreferences.Domain/Entities/CarPreference.cs
public class CarPreference : Entity
{
    public string Model { get; private set; }
    
    public CarPreference(int id, string model)
    {
        this.Id = id;
        this.Model = model;
    }
}

// 使用
var cp1 = new CarPreference(1, "Model A");
var cp2 = new CarPreference(1, "Model B");

cp1 == cp2;  // true (IDが同じ)
```

**ルール**:
- ✅ ID は永続化から割り当てられる
- ✅ ライフサイクル（作成・削除・更新）を持つ
- ✅ 可変な状態を持つ
- ❌ コンストラクタで複雑な初期化をしない

---

### 2. 値オブジェクト基底クラス（ValueObject）

**目的**: 値による等値比較、イミュータビリティ

**定義**:
```csharp
public abstract class ValueObject
{
    // 値による等値比較
    public override bool Equals(object? obj)
    {
        return obj is ValueObject vo && 
               this.GetEqualityComponents()
                   .SequenceEqual(vo.GetEqualityComponents());
    }
    
    protected abstract IEnumerable<object?> GetEqualityComponents();
    
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(0, (acc, obj) => 
                HashCode.Combine(acc, obj?.GetHashCode()));
    }
}
```

**使用方法**:
```csharp
// ✅ 正しい例：値による比較

// CarPreferences.Domain/ValueObjects/PreferenceCategory.cs
public class PreferenceCategory : ValueObject
{
    public string Value { get; }
    
    public PreferenceCategory(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new DomainException("値は必須です");
        this.Value = value;
    }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

// 使用
var cat1 = new PreferenceCategory("Comfort");
var cat2 = new PreferenceCategory("Comfort");

cat1 == cat2;  // true (値が同じ)
cat1.Equals(cat2);  // true
```

**ルール**:
- ✅ ID を持たない
- ✅ 不変（イミュータブル）
- ✅ 値による等値比較
- ✅ ビジネスルール（バリデーション）を含める
- ❌ 状態変更不可

---

### 3. ドメインイベント基底（DomainEvent）

**目的**: Context内のビジネスイベント通知

**定義**:
```csharp
public abstract class DomainEvent
{
    public LocalDateTime OccurredOn { get; }
    
    // ⚠️ 注意: Domain層では IClock 経由で日時を取得
    // DomainEvent を発行する Entity が IClock を注入され、
    // Entity から DomainEvent へ日時を渡す設計が推奨
    public DomainEvent(LocalDateTime occurredOn)
    {
        OccurredOn = occurredOn;
    }
}
```

**使用方法**:
```csharp
// Domain層でのイベント定義
public class CarPreferenceUpdatedEvent : DomainEvent
{
    public int CarPreferenceId { get; }
    public PreferenceCategory NewCategory { get; }
    
    public CarPreferenceUpdatedEvent(int id, PreferenceCategory category)
    {
        CarPreferenceId = id;
        NewCategory = category;
    }
}

// Domain層で発行
public class CarPreference : Entity
{
    private readonly List<DomainEvent> _domainEvents = new();
    
    public void UpdatePreference(PreferenceCategory newCategory)
    {
        this.Category = newCategory;
        
        // イベント発行
        _domainEvents.Add(
            new CarPreferenceUpdatedEvent(this.Id, newCategory));
    }
    
    public IReadOnlyList<DomainEvent> GetDomainEvents()
    {
        return _domainEvents.AsReadOnly();
    }
}

// Application層で処理
public class UpdateCarPreferenceUseCase
{
    public async Task Execute(UpdateRequest request)
    {
        var cp = await _repository.GetById(request.Id);
        cp.UpdatePreference(new PreferenceCategory(request.Category));
        
        await _repository.Save(cp);
        
        // イベント発行処理（Application層で制御）
        foreach (var @event in cp.GetDomainEvents())
        {
            await _eventPublisher.Publish(@event);
        }
    }
}
```

**ルール**:
- ✅ Domain層で定義・発行
- ✅ Application層で処理
- ✅ 過去形の名前（UpdatedEvent, CreatedEvent）
- ❌ Domain内での購読・処理は避ける

---

### 4. ビジネス例外（DomainException）

**目的**: ビジネスルール違反の通知

**定義**:
```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

**使用方法**:
```csharp
// Domain層：ビジネスルール検証

public class PreferenceCategory : ValueObject
{
    public PreferenceCategory(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new DomainException("カテゴリは必須です");  // ✅
        
        this.Value = value;
    }
}

public class CarPreference : Entity
{
    public void UpdatePreference(PreferenceCategory category)
    {
        if (category == null)
            throw new DomainException("カテゴリは必須です");  // ✅
        
        this.Category = category;
    }
}

// Application層：例外処理

public class UpdateCarPreferenceUseCase
{
    public async Task<Response> Execute(Request request)
    {
        try
        {
            var cp = await _repository.GetById(request.Id);
            cp.UpdatePreference(new PreferenceCategory(request.Category));
            // ...
        }
        catch (DomainException ex)
        {
            // ビジネスルール違反
            return Response.Error(ex.Message);  // ✅
        }
    }
}
```

**ルール**:
- ✅ ビジネスルール違反時にスロー
- ✅ メッセージはユーザー向け
- ✅ Application層で catch
- ❌ 技術的エラーには使わない

---

## Context固有のDomain

### CarPreferences.Domain

```
CarPreferences.Domain/
├─ Entities/
│  └─ CarPreference.cs           ← Entity継承
│     責務: 車両設定のビジネスルール
│
├─ ValueObjects/
│  └─ PreferenceCategory.cs      ← ValueObject継承
│     責務: カテゴリの値と検証
│
├─ DomainEvents/
│  └─ CarPreferenceUpdatedEvent.cs ← DomainEvent継承
│     責備: カテゴリ更新イベント
│
└─ Exceptions/
   (Domain層例外あれば)
```

### 実装例

```csharp
// CarPreferences.Domain/Entities/CarPreference.cs
public class CarPreference : Entity  // ✅ SharedKernel.Entity継承
{
    public string Model { get; private set; }
    public PreferenceCategory Category { get; private set; }
    private readonly List<DomainEvent> _domainEvents = new();
    
    public void UpdatePreference(PreferenceCategory newCategory)
    {
        if (newCategory == null)
            throw new DomainException("カテゴリは必須です");
        
        this.Category = newCategory;
        _domainEvents.Add(
            new CarPreferenceUpdatedEvent(this.Id, newCategory));
    }
    
    public IReadOnlyList<DomainEvent> GetDomainEvents()
    {
        return _domainEvents.AsReadOnly();
    }
}
```

---

## 設計パターン

### パターン1：Aggregate パターン

```
CarPreference (Aggregate Root)
├─ PreferenceCategory (Value Object)
├─ DomainEvent (発行)
└─ DomainException (検証)

→ CarPreference 経由でのみアクセス
→ PreferenceCategory 単独では保存しない
```

### パターン2：ビジネスルール集約

```csharp
// ✅ ビジネスルールを Entity に集約

public class CarPreference : Entity
{
    // ビジネスルール：更新可能か判定
    public bool CanUpdate => this.Status == PreferenceStatus.Active;
    
    public void UpdatePreference(PreferenceCategory category)
    {
        if (!this.CanUpdate)
            throw new DomainException("更新できません");
    }
}
```

### パターン3：不変性確保

```csharp
// ✅ 値オブジェクトは不変

public class PreferenceCategory : ValueObject
{
    public string Value { get; }  // 読み取り専用
    
    public PreferenceCategory(string value)
    {
        this.Value = value;
    }
    
    // 変更は新規生成で対応
    public PreferenceCategory ChangeValue(string newValue)
    {
        return new PreferenceCategory(newValue);
    }
}
```

---

## 新規Context追加時のチェックリスト

- [ ] `[ContextName].Domain` プロジェクト作成
- [ ] `[ContextName].Domain.csproj` が SharedKernel 参照
- [ ] Entity クラスが `Entity` 継承
- [ ] ValueObject クラスが `ValueObject` 継承
- [ ] DomainEvent が `DomainEvent` 継承
- [ ] 例外が `DomainException` 使用
- [ ] ビジネスルール実装が Domain 層のみ
- [ ] Application層 参照なし

---

**作成日**: 2026-06-30  
**参考**: 01-Layer-Architecture.md / 03-CleanArchitecture-Principles.md
