# Employee - 詳細設計書

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain（CarPreferences Bounded Context）  
**種別:** Entity 詳細設計  
**依拠技術仕様書:** Employee技術仕様書 v1.0  
**版:** 1.0 / 2026-08-09

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `Employee` |
| 種別 | `sealed class` |
| 名前空間 | `SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities` |
| 継承元 | `Entity<EmployeeId>`（集約根） |
| 配置レイヤ | Domain（CarPreferences Bounded Context） |

### 1.2 責務

- **従業員ドメインモデルの表現**: ID、コード、人物レコード参照を管理
- **集約根としての役割**: EmployeeId を集約根IDとして提供
- **不変性の確保**: 生成後は値の変更を許可しない

### 1.3 協調クラス

```
Employee（集約根）
  ├─ Entity<EmployeeId>
  │   └─ EmployeeId（集約根ID）
  ├─ EmployeeRowId（DB行ID）
  ├─ EmployeeCode（複合VO）
  │   ├─ EmployeeDivision
  │   └─ EmployeeNumber
  └─ PersonRowId（FK参照）

Repository
  └─ FindByIdAsync(EmployeeId)
  └─ FindByCodeAsync(EmployeeCode)
  └─ FindByRowIdAsync(EmployeeRowId)
```

---

## 2. プロパティ設計

### 2.1 プロパティ（public）

#### Id: EmployeeId

| 項目 | 内容 |
|------|------|
| 型 | `EmployeeId` |
| アクセス | `public get` |
| 実装 | `private set`（Entity<EmployeeId> から継承） |
| 用途 | 集約根ID、ビジネスID |
| 設計判断 | 読み取り専用。生成後は変更不可。 |

#### RowId: EmployeeRowId

| 項目 | 内容 |
|------|------|
| 型 | `EmployeeRowId` |
| アクセス | `public get` |
| 実装 | `private set` |
| 用途 | DB行ID（t_employees.row_id） |
| 設計判断 | 読み取り専用。Repository での INSERT 時に割り当てられる。 |

#### Code: EmployeeCode

| 項目 | 内容 |
|------|------|
| 型 | `EmployeeCode` |
| アクセス | `public get` |
| 実装 | `private set` |
| 用途 | 従業員コード（区分+番号） |
| 設計判断 | 読み取り専用。範囲検証は EmployeeCode で完結。 |

#### PersonRowId: PersonRowId

| 項目 | 内容 |
|------|------|
| 型 | `PersonRowId` |
| アクセス | `public get` |
| 実装 | `private set` |
| 用途 | 人物マスタへの外部キー（m_persons.row_id） |
| 設計判断 | 読み取り専用。人物マスタの参照を保持。 |

---

## 3. コンストラクタ設計

### 3.1 プライベートコンストラクタ

```csharp
private Employee(
    EmployeeId id,
    EmployeeRowId rowId,
    EmployeeCode code,
    PersonRowId personRowId)
{
    Id = id;
    RowId = rowId;
    Code = code;
    PersonRowId = personRowId;
}
```

| 項目 | 内容 |
|------|------|
| 修飾子 | `private` |
| パラメータ | すべてのビジネスプロパティ |
| 処理フロー | 各プロパティを初期化して Entity を構築 |
| 用途 | Entity 生成は通常コンストラクタのみ |

---

## 4. ファクトリメソッド設計

### 4.1 Create メソッド（推奨: Application層での使用）

```csharp
public static Employee Create(
    EmployeeId id,
    EmployeeRowId rowId,
    EmployeeCode code,
    PersonRowId personRowId)
{
    // ビジネスルール検証（今回は不要。ValueObject で検証済み）
    
    return new(id, rowId, code, personRowId);
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | すべてのビジネスプロパティ |
| 戻り値 | `Employee` |
| 例外 | ビジネスルール違反時は例外（現在は検証なし） |
| 処理フロー | 1. パラメータの妥当性チェック（必要に応じて）<br/>2. new で生成 |
| 用途 | Application層の Use Case で Employee を生成 |
| 設計判断 | ValueObject での検証で十分なため、Entity レベルでの追加検証は不要 |

### 4.2 Reconstruct メソッド（Infrastructure層での使用）

```csharp
public static Employee Reconstruct(
    EmployeeId id,
    EmployeeRowId rowId,
    EmployeeCode code,
    PersonRowId personRowId)
{
    // DB から読み込まれた値を復元（検証なし）
    return new(id, rowId, code, personRowId);
}
```

| 項目 | 内容 |
|------|------|
| パラメータ | すべてのビジネスプロパティ |
| 戻り値 | `Employee` |
| 用途 | Repository で DbModel → Entity に変換 |
| 設計判断 | DB値は既に検証済みと仮定。検証なしで復元。 |

---

## 5. メソッド設計

### 5.1 等価性メソッド（Entity<EmployeeId> から継承）

Entity<TId> は自動的に以下を実装します：

- `Equals(object? obj)`: object ベースの等価性判定
- `Equals(Employee? other)`: Employee ベースの等価性判定（EmployeeId で比較）
- `GetHashCode()`: EmployeeId のハッシュコード

---

## 6. レイヤ制約確認

| 項目 | 確認 |
|------|------|
| Domain層への依存のみ | ✓ ValueObject のみ使用（SharedKernel） |
| Application層への依存なし | ✓ （Use Case で生成されるが、逆依存なし） |
| Infrastructure層への依存なし | ✓ （Repository で使用されるが、逆依存なし） |
| Presentation層への依存なし | ✓ |

---

## 7. 実装上の注意点

### 7.1 Entity<EmployeeId> の継承

```csharp
public sealed class Employee : Entity<EmployeeId>
{
    // Id プロパティは Entity<EmployeeId> から自動的に提供される
    public EmployeeId Id { get; private set; }  // 型安全な集約根ID
}
```

### 7.2 ValueObject への検証委譲

Employee レベルでの検証は不要。すべての検証は ValueObject で完結。

```csharp
// ❌ 以下は不要（ValueObject で検証済み）
if (code.Number.Value < 1001) { ... }

// ✅ ValueObject は常に有効な状態を保持
var employee = new Employee(...);  // code は範囲検証済み
```

### 7.3 読み取り専用プロパティ

すべてのビジネスプロパティは `private set` で読み取り専用。

```csharp
// ❌ 変更不可
employee.Code = newCode;  // CS0200

// ✅ 読み取り専用
var code = employee.Code;
```

### 7.4 複数テーブル集約の行ID管理

Employee は複数のテーブル行を参照：
- `t_employees.row_id` → `RowId`
- `m_persons.row_id` → `PersonRowId`

各行IDは独立した ValueObject で型安全に管理。

```csharp
var employee = new Employee(
    id: EmployeeId.NewId(),
    rowId: EmployeeRowId.From(12345L),    // t_employees
    code: ...,
    personRowId: PersonRowId.From(67890L) // m_persons
);
```

---

## 8. 実装手順

1. **クラス定義**: `sealed class Employee : Entity<EmployeeId>`
2. **プロパティ定義**: RowId、Code、PersonRowId（すべて private set）
3. **プライベートコンストラクタ実装**
4. **Create ファクトリメソッド実装**: Application層での使用
5. **Reconstruct ファクトリメソッド実装**: Infrastructure層での使用
6. **ToString オーバーライド（オプション）**: ログ出力用

---

## 参考資料

- 技術仕様書: `Employee_技術仕様書.md`
- 単体テスト仕様: `Employee_単体テスト仕様書.md`
- ValueObject: `../../../SharedKernel/ValueObjects/Identifiers/`
- Entity基底: `../../../SharedKernel/Entities/Entity.cs`
