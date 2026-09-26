# Employee - 単体テスト仕様書

**対象者**: テスト実装者（Employee Entity の単体テストを実装）

**版**: 1.0  
**作成日**: 2026-08-09

---

## 📖 概要

このドキュメントは Employee Entity の単体テストの仕様を定義します。

---

## 1. テスト観点一覧

### VO-CONS: コンストラクタ・生成

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-CONS-01 | 有効なパラメータで Employee が生成される | 正常系 | ✅ EmployeeTests.cs::VO_CONS_01_CreateValidEmployeeReturnsValidEmployee |
| VO-CONS-02 | RegularEmployee（M）で生成できる | 正常系 | ✅ EmployeeTests.cs::VO_CONS_02_CreateRegularEmployeeIsRegularEmployee |
| VO-CONS-03 | DispatchedEmployee（T）で生成できる | 正常系 | ✅ EmployeeTests.cs::VO_CONS_03_CreateDispatchedEmployeeIsDispatched |
| VO-CONS-04 | ContractorEmployee（C）で生成できる | 正常系 | ✅ EmployeeTests.cs::VO_CONS_04_CreateContractorEmployeeIsContractor |
| VO-CONS-05 | RetiredOn を指定して生成できる | 正常系 | ✅ EmployeeTests.cs::VO_CONS_05_CreateWithRetiredOnReturnsRetiredEmployee |

### VO-PROP: プロパティアクセス

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-PROP-01 | RowId プロパティが正しい値を返す | 正常系 | ✅ EmployeeTests.cs::VO_CONS_01_CreateValidEmployeeReturnsValidEmployee |
| VO-PROP-02 | BizDivision プロパティが正しい値を返す | 正常系 | ✅ EmployeeTests.cs::VO_CONS_02_CreateRegularEmployeeIsRegularEmployee |
| VO-PROP-03 | BizId プロパティが正しい値を返す | 正常系 | ✅ EmployeeTests.cs::VO_CONS_01_CreateValidEmployeeReturnsValidEmployee |
| VO-PROP-04 | BizCode プロパティが正しい値を返す | 正常系 | ✅ EmployeeTests.cs::VO_CONS_01_CreateValidEmployeeReturnsValidEmployee |
| VO-PROP-05 | Person プロパティが正しい値を返す | 正常系 | ✅ EmployeeTests.cs::VO_CONS_01_CreateValidEmployeeReturnsValidEmployee |
| VO-PROP-06 | RetiredOn プロパティが正しい値を返す | 正常系 | ✅ EmployeeTests.cs::VO_CONS_05_CreateWithRetiredOnReturnsRetiredEmployee |
| VO-PROP-07 | プロパティが読み取り専用である | 不変性 | ⚠️ 手動確認のみ（コンパイルエラー） |

### VO-EQ: Equals — 等価判定

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EQ-01 | 同じ RowId を持つ Employee は等価である | 正常系 | ❌ 未実装（実装待ち） |
| VO-EQ-02 | 同一参照のオブジェクトは等価である | 正常系 | ❌ 未実装（実装待ち） |
| VO-EQ-03 | object 型で比較しても等価である | 正常系 | ❌ 未実装（実装待ち） |

### VO-NE: Equals — 非等価判定

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-NE-01 | 異なる RowId を持つ Employee は非等価である | 異常系 | ❌ 未実装（実装待ち） |
| VO-NE-02 | null との比較は非等価である | 異常系 | ❌ 未実装（実装待ち） |

### VO-HC: GetHashCode

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値である | 正常系 | ❌ 未実装（実装待ち） |
| VO-HC-02 | HashSet / Dictionary で正しく動作する | 正常系 | ❌ 未実装（実装待ち） |

### VO-VALID: 値検証

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-VALID-01 | 無効な BizCode で生成すると例外が発生する | 異常系 | ⚠️ ValueObject の検証に依存 |
| VO-VALID-02 | 無効な EmployeeRowId で生成すると例外が発生する | 異常系 | ⚠️ ValueObject の検証に依存 |
| VO-VALID-03 | 無効な PersonRowId で生成すると例外が発生する | 異常系 | ⚠️ ValueObject の検証に依存 |

---

## 🧪 テストケース一覧

### グループ 1: 生成メソッド（Create）

#### EMP-CREATE-01: 正常な値で生成成功

```gherkin
When: Employee.Create(...) を呼び出す（すべてのパラメータ有効）
Then: Employee インスタンスが生成される
And:  すべてのプロパティが指定された値である
```

#### EMP-CREATE-02: 異なるEmployeeCodeで複数生成

```gherkin
When: Employee.Create を複数回呼び出す（異なるコード）
Then: 各回で異なる Employee インスタンスが生成される
And:  各 Employee の Code は異なる
```

#### EMP-CREATE-03: 正規従業員（M）で生成

```gherkin
When: Employee.Create(..., code: M1234, ...) を呼び出す
Then: Employee が生成される
And:  Code.Division.IsRegularEmployee は true
```

#### EMP-CREATE-04: 派遣社員（T）で生成

```gherkin
When: Employee.Create(..., code: T7500, ...) を呼び出す
Then: Employee が生成される
And:  Code.Division.IsDispatched は true
```

#### EMP-CREATE-05: 請負者（C）で生成

```gherkin
When: Employee.Create(..., code: C8000, ...) を呼び出す
Then: Employee が生成される
And:  Code.Division.IsContractor は true
```

### グループ 2: 復元メソッド（Reconstruct）

#### EMP-RECONSTRUCT-01: DB値から復元成功

```gherkin
When: Employee.Reconstruct(...) を呼び出す（DB値）
Then: Employee インスタンスが生成される
And:  すべてのプロパティが一致
```

#### EMP-RECONSTRUCT-02: 複数の Employee を復元

```gherkin
When: Employee.Reconstruct を複数回呼び出す（異なるDB値）
Then: 各回で異なる Employee インスタンスが生成される
```

### グループ 3: プロパティアクセス

#### EMP-PROP-01: Id プロパティ取得

```gherkin
When: employee.Id を取得
Then: EmployeeId インスタンスが返される
And:  値は Create時に指定した値と一致
```

#### EMP-PROP-02: RowId プロパティ取得

```gherkin
When: employee.RowId を取得
Then: EmployeeRowId インスタンスが返される
And:  値は Create時に指定した値と一致
```

#### EMP-PROP-03: Code プロパティ取得

```gherkin
When: employee.Code を取得
Then: EmployeeCode インスタンスが返される
And:  Code.Division と Code.Number にアクセス可能
```

#### EMP-PROP-04: PersonRowId プロパティ取得

```gherkin
When: employee.PersonRowId を取得
Then: PersonRowId インスタンスが返される
And:  値は Create時に指定した値と一致
```

#### EMP-PROP-05: すべてのプロパティが読み取り専用

```gherkin
When: employee.Code = newCode を試みる
Then: コンパイルエラー（CS0200）が発生する
```

### グループ 4: 等価性（Equality）

#### EMP-EQ-01: 同じEmployeeIdで等価

```gherkin
When: 同じ EmployeeId で 2 つの Employee を生成
Then: employee1 == employee2 は true
And:  employee1.Equals(employee2) は true
```

#### EMP-EQ-02: 異なるEmployeeIdで非等価

```gherkin
When: 異なる EmployeeId で 2 つの Employee を生成
Then: employee1 != employee2 は true
And:  employee1.Equals(employee2) は false
```

#### EMP-EQ-03: ハッシュコードが一致

```gherkin
When: 同じ EmployeeId で 2 つの Employee を生成
Then: employee1.GetHashCode() == employee2.GetHashCode()
```

#### EMP-EQ-04: ディクショナリで使用可能

```gherkin
When: Employee をディクショナリのキーとして使用
Then: EmployeeId の等価性に基づいて検索可能
```

#### EMP-EQ-05: null との比較

```gherkin
When: employee.Equals(null) を呼び出す
Then: false が返される
```

### グループ 5: プロパティ統合テスト

#### EMP-INTEG-01: 複数プロパティの整合性

```gherkin
When: Employee を生成し、すべてのプロパティにアクセス
Then: すべてのプロパティが有効な値を保持
And:  EmployeeCode の Division と RowId、PersonRowId に矛盾がない
```

#### EMP-INTEG-02: プロパティアクセス後の不変性

```gherkin
When: employee.Code にアクセス後、再度アクセス
Then: 同じ EmployeeCode インスタンスが返される（値が変わらない）
```

### グループ 6: ValueObject 検証統合

#### EMP-VO-01: 無効なEmployeeCodeでの生成は失敗

```gherkin
When: Employee.Create(..., code: M7500, ...) を呼び出す（無効な組み合わせ）
Then: ArgumentException が発生する
```

#### EMP-VO-02: 無効なEmployeeRowIdでの生成は失敗

```gherkin
When: Employee.Create(..., rowId: EmployeeRowId.From(0L), ...) を呼び出す
Then: ArgumentOutOfRangeException が発生する
```

#### EMP-VO-03: 無効なPersonRowIdでの生成は失敗

```gherkin
When: Employee.Create(..., personRowId: PersonRowId.From(0L), ...) を呼び出す
Then: ArgumentOutOfRangeException が発生する
```

---

## 📊 テスト概要

| グループ | テスト数 | 備考 |
|---------|--------|------|
| 生成メソッド（EMP-CREATE） | 5 | 正常系3、特殊ケース2 |
| 復元メソッド（EMP-RECONSTRUCT） | 2 | DB復元シナリオ |
| プロパティアクセス（EMP-PROP） | 5 | 各プロパティ+読取専用確認 |
| 等価性（EMP-EQ） | 5 | 等価性判定、ハッシュ、ディクショナリ |
| 統合（EMP-INTEG） | 2 | 複合シナリオ |
| ValueObject検証（EMP-VO） | 3 | 無効値での失敗確認 |
| **合計** | **22** | |

---

## ✅ テスト実装ガイド

### 命名規則

```
Test{グループ}{シーケンス}_{説明}

例：
- TestEMPCREATE01_CreateValidEmployeeReturnsValidEmployee
- TestEMPEQ01_SameEmployeeIdAreEqual
- TestEMPVO01_InvalidCodeThrowsException
```

### Fact vs Theory

```csharp
// Fact（1つのシナリオ）
[Fact]
public void TestEMPCREATE01_CreateValidEmployeeReturnsValidEmployee()
{
    var employee = Employee.Create(
        EmployeeId.NewId(),
        EmployeeRowId.From(1L),
        EmployeeCode.From(...),
        PersonRowId.From(1L));
    
    Assert.NotNull(employee);
    Assert.NotEqual(Guid.Empty, employee.Id.Value);
}

// Theory（複数シナリオ）
[Theory]
[InlineData('M')]
[InlineData('T')]
[InlineData('C')]
public void TestEMPCREATEDataDriven_CreateWithDivision(char divisionChar)
{
    // ...
}
```

---

## 参考資料

- 技術仕様書: `Employee_技術仕様書.md`
- 詳細設計書: `Employee_詳細設計書.md`
- ValueObject: `../../../SharedKernel/ValueObjects/Identifiers/`
