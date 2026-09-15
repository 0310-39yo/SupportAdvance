# Department Context Application層 - 単体テスト仕様書

**バージョン:** 1.0  
**作成日:** 2026-09-16  
**対象読者:** テスト実装者  
**内容:** テストケース一覧、グループ定義、検証方法

---

## 📋 概要

本書は、Department Context Application層の **単体テスト仕様** です。

DepartmentQueryService（汎用 Query Service パターン実装）のテストケースを定義し、Repository への委譲が正確に実行されることを検証します。

---

## 0. 本書の位置づけ

**対象クラス:** `SupportAdvance.Contexts.Department.Application.Queries.DepartmentQueryService`

DepartmentQueryService は、**ジェネリック Query Service パターン** の実装例です。他の Context から Department ドメインモデルをリアルタイムに読み取るための統一インターフェース（`IQueryService<Department, DepartmentRowId>`）を提供します。

本テストは、DepartmentQueryService が Repository に正しく委譲すること、および、Repository の戻り値をそのまま返却することを検証することが目的です。

---

## 1. テスト目的

DepartmentQueryService の各メソッドが、以下の仕様を満たすことを確認する：

- **委譲の正確性**: GetByIdAsync が Repository に正しく委譲すること
- **戻り値の透過性**: Repository の戻り値をそのまま返すこと（null 含む）
- **入力検証**: 不正な入力（null ID）を正しく検出し、例外で拒否すること
- **型安全性**: ジェネリック型パラメータ（Department, DepartmentRowId）の正確性

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | DepartmentQueryService |
| **クラス分類** | ☐ Utility ☑ Helper ☐ Converter ☐ Validator ☐ Calculator ☐ その他 |
| **名前空間** | SupportAdvance.Contexts.Department.Application.Queries |
| **実装インターフェース** | IQueryService<Department, DepartmentRowId> |
| **依存クラス** | IDepartmentRepository（DI 経由注入） |
| **前提** | IDepartmentRepository インターフェースの実装が完了 |

---

## 3. テスト対象メソッド

| # | メソッド名 | シグネチャ | 責務 |
|---|----------|---------|------|
| 1 | GetByIdAsync | `Task<Department?> GetByIdAsync(DepartmentRowId id)` | Repository に id で委譲し、戻り値を返す |

---

## 4. テスト観点一覧

### 観点グループ UC：Query Service 委譲テスト

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| UC-DEL-01 | GetByIdAsync が Repository に正しく委譲される | 正常系 | Query Service パターン | MockDepartmentRepository |
| UC-RET-01 | 存在する ID で Department が返却される | 正常系 | Repository 実装仕様 | MockDepartmentRepository |
| UC-RET-02 | 存在しない ID で null が返却される | 正常系（null 戻り値） | Repository 実装仕様 | MockDepartmentRepository |
| UC-NULL-01 | null ID で ArgumentNullException が発生する | 異常系 | null ガード実装 | DIFixture |

---

## 5. テスト仕様

### 観点 UC-DEL-01：GetByIdAsync が Repository に正しく委譲される

#### 5.1.1 テスト観点

メソッドが、指定された DepartmentRowId を Repository.GetByIdAsync に正しく委譲すること。

#### 5.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.1.2.1 | 正常系 | 有効な ID で Repository に委譲し、戻り値が返される |
| 5.1.2.2 | 正常系 | 複数の異なる ID で委譲が個別に実行される |

#### 5.1.3 前提条件

- IDepartmentRepository が MockDepartmentRepository で提供されている
- MockDepartmentRepository の SetupGetByIdAsync で期待値が事前に設定されている

#### 5.1.4 テストデータ

| パターン | ID 値 | 説明 |
|---------|--------|------|
| 5.1.2.1 | DepartmentRowId.From(1L) | 営業部を表現する有効な ID |
| 5.1.2.2 | DepartmentRowId.From(2L) | 企画部を表現する有効な ID |

#### 5.1.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.1.2.1 | Department インスタンス（RowId = 1） | 返却値が null ではなく、RowId が一致すること |
| 5.1.2.2 | Department インスタンス（RowId = 2） | Repository が異なる ID に対して個別に応答すること |

#### 5.1.6 判定基準

- [ ] GetByIdAsync が Repository に委譲すること（MockDepartmentRepository の GetByIdAsync が呼び出されること）
- [ ] 戻り値が期待値と一致すること
- [ ] 複数の異なる ID で個別に機能すること

---

### 観点 UC-RET-01：存在する ID で Department が返却される

#### 5.2.1 テスト観点

Repository に存在するエンティティが正しく返却されること。

#### 5.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.2.2.1 | 正常系 | 存在する ID で Department インスタンスが返される |

#### 5.2.3 テストデータ

```csharp
var departmentId = DepartmentRowId.From(1L);
var expectedDepartment = Department.Create(
    departmentId,
    DepartmentCode.From("D001"),
    "営業部",
    HierarchyLevel.From(1)
);
```

#### 5.2.4 期待結果

- 返却値: Department インスタンス
- RowId: 1L
- DeptCode: "D001"
- Name: "営業部"
- Level: 1

#### 5.2.5 判定基準

- [ ] Assert.NotNull(result)
- [ ] Assert.Equal(expectedDepartment.RowId, result.RowId)

---

### 観点 UC-RET-02：存在しない ID で null が返却される

#### 5.3.1 テスト観点

Repository に存在しないエンティティに対しても、例外を発生させずに null が返却されること。

#### 5.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.3.2.1 | 正常系（null 戻り値） | 存在しない ID で null が返される |

#### 5.3.3 テストデータ

```csharp
var nonExistentId = DepartmentRowId.From(2L);
((MockDepartmentRepository)repository).SetupGetByIdAsync(nonExistentId, null);
```

#### 5.3.4 期待結果

- 返却値: null

#### 5.3.5 判定基準

- [ ] Assert.Null(result)
- [ ] 例外が発生しないこと

---

### 観点 UC-NULL-01：null ID で ArgumentNullException が発生する

#### 5.4.1 テスト観点

null の DepartmentRowId が渡された場合、ArgumentNullException が発生すること。

#### 5.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.4.2.1 | 異常系 | null ID で ArgumentNullException が発生 |

#### 5.4.3 前提条件

- DepartmentRowId は ValueObject であり、null チェックが実装されている

#### 5.4.4 テストデータ

```csharp
DepartmentRowId id = null!;  // null を渡す
```

#### 5.4.5 期待結果

```csharp
await Assert.ThrowsAsync<ArgumentNullException>(() => queryService.GetByIdAsync(null!));
```

#### 5.4.6 判定基準

- [ ] ArgumentNullException がスローされること
- [ ] メッセージに "null" または "id" を含むこと

---

## 6. テスト用実装と DI 設定

### 6.1 MockDepartmentRepository

テスト用モック実装として、IDepartmentRepository を実装するメモリ内リポジトリを提供：

```csharp
private class MockDepartmentRepository : IDepartmentRepository
{
    private Dictionary<long, Department?> _store = new();

    public void SetupGetByIdAsync(DepartmentRowId id, Department? department)
    {
        _store[id.Value] = department;
    }

    public Task<Department?> GetByIdAsync(DepartmentRowId id)
    {
        if (_store.TryGetValue(id.Value, out var department))
        {
            return Task.FromResult(department);
        }
        return Task.FromResult<Department?>(null);
    }
    
    // 他のメソッドは実装...
}
```

---

## 7. 前提条件・制限事項

| 項目 | 内容 |
|------|------|
| **Repository 実装** | IDepartmentRepository が実装済みであること |
| **テストダブル** | テストコードに組み込まれた MockDepartmentRepository を使用 |
| **DI コンテナ** | テスト内で直接インスタンス化（DI コンテナ不要） |
| **非同期処理** | すべてのテストメソッドで async/await を使用 |
| **言語機能** | C# 11 以上の機能を活用（NullableReferenceTypes 有効） |

---

## 8. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-09-16 | Claude Haiku 4.5 | 初版作成。DepartmentQueryService のテスト仕様を定義。4 つのテスト観点（UC-DEL-01, UC-RET-01, UC-RET-02, UC-NULL-01）を網羅 |

---
