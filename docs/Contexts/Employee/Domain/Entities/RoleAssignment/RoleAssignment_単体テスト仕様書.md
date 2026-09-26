# RoleAssignment - 単体テスト仕様書

**対象者**: テスト実装者

**版**: 1.0  
**作成日**: 2026-08-10

---

## 📖 テスト戦略

### テスト範囲

| 対象 | 内容 |
|------|------|
| **ファクトリメソッド** | Create, Reconstruct |
| **プロパティ** | RoleCode, EffectiveDate, ExpirationDate（読み取り、不変性） |
| **ビジネスロジック** | IsActive（有効期間判定） |
| **等価性** | Equals, GetHashCode, Dictionary キー使用 |
| **統合テスト** | すべてのプロパティの整合性 |

### テスト方法

- **ユニットテスト**: Xunit
- **モック**: なし（ビジネスロジックのみ）
- **テストデータ**: 固定の LocalDateTime を使用

---

## 1. テスト観点一覧

### VO-CONS: コンストラクタ・生成

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-CONS-01 | 有効期間ありで生成できる | 正常系 | ✅ RoleAssignmentTests.cs::VO_CONS_01_WithExpirationDateReturnsValidRoleAssignment |
| VO-CONS-02 | 無期限（ExpirationDate = null）で生成できる | 正常系 | ✅ RoleAssignmentTests.cs::VO_CONS_02_WithoutExpirationDateReturnsValidRoleAssignment |
| VO-CONS-03 | 複数生成時に異なるID が採番される | 正常系 | ⏸️ 未実装（テスト未作成） |
| VO-CONS-04 | 有効期間ありで復元できる | 正常系 | ✅ RoleAssignmentTests.cs::VO_CONS_04_ReconstructFromDbValuesReturnsValidRoleAssignment |
| VO-CONS-05 | 無期限で復元できる | 正常系 | ⏸️ 未実装（テスト未作成） |

### VO-PROP: プロパティアクセス

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-PROP-01 | RoleCode プロパティが正しい値を返す | 正常系 | ⏸️ 未実装（テスト未作成） |
| VO-PROP-02 | すべてのプロパティが読み取り専用である | 不変性 | ⏸️ 未実装（テスト未作成） |
| VO-PROP-03 | プロパティ値が複数回アクセスで変わらない | 不変性 | ⏸️ 未実装（テスト未作成） |

### VO-METHOD: ドメインメソッド（IsActive）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-METHOD-01 | 有効期間内で IsActive = true を返す | 正常系 | ⏸️ 未実装（テスト未作成） |
| VO-METHOD-02 | 開始日より前で IsActive = false を返す | 異常系 | ✅ RoleAssignmentTests.cs::VO_METHOD_02_BeforeEffectiveDateReturnsFalse |
| VO-METHOD-03 | 終了日以後で IsActive = false を返す | 異常系 | ✅ RoleAssignmentTests.cs::VO_METHOD_03_AfterExpirationDateReturnsFalse |
| VO-METHOD-04 | 無期限で開始日以後は常に IsActive = true を返す | 正常系 | ⏸️ 未実装（テスト未作成） |
| VO-METHOD-05 | 開始日と同日で IsActive = true を返す | 正常系（境界値） | ✅ RoleAssignmentTests.cs::VO_METHOD_05_OnOrAfterEffectiveDateReturnsTrue |

### VO-EQ: Equals — 等価判定

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EQ-01 | 同じ ID を持つ RoleAssignment は等価である | 正常系 | ⏸️ 未実装（テスト未作成） |
| VO-EQ-02 | object 型で比較しても等価である | 正常系 | ⚠️ 記載なし（等価性テストに含まれる可能性） |

### VO-NE: Equals — 非等価判定

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-NE-01 | 異なる ID を持つ RoleAssignment は非等価である | 異常系 | ⏸️ 未実装（テスト未作成） |
| VO-NE-02 | null との比較は非等価である | 異常系 | ⏸️ 未実装（テスト未作成） |

### VO-HC: GetHashCode

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-HC-01 | 同一 ID のハッシュコードが一致する | 正常系 | ⏸️ 未実装（テスト未作成） |
| VO-HC-02 | Dictionary のキーとして使用可能である | 正常系 | ⏸️ 未実装（テスト未作成） |

### VO-INTEG: 統合テスト

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-INTEG-01 | すべてのプロパティが整合性を持つ | 正常系 | ⏸️ 未実装（テスト未作成） |
| VO-INTEG-02 | Create と Reconstruct が一貫性を持つ | 正常系 | ⏸️ 未実装（テスト未作成） |

---

## 🧪 テストケース詳細

### グループ 1: 生成メソッド（Create）

#### テスト1.1: 正常系 - 有効期間ありで生成

```csharp
[Fact]
public void TestCreate01_WithExpirationDateReturnsValidRoleAssignment()
{
    // Arrange
    var roleCode = RoleCode.From("Admin");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
    var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

    // Act
    var roleAssignment = RoleAssignment.Create(roleCode, effectiveDate, expirationDate);

    // Assert
    Assert.NotNull(roleAssignment);
    Assert.Equal(roleCode, roleAssignment.RoleCode);
    Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
    Assert.Equal(expirationDate, roleAssignment.ExpirationDate);
    Assert.NotEqual(Guid.Empty, roleAssignment.Id.Value);
}
```

**検証項目**:
- インスタンス生成成功
- プロパティが正しく設定されている
- ID が自動採番（GUID）

---

#### テスト1.2: 正常系 - 無期限で生成

```csharp
[Fact]
public void TestCreate02_WithoutExpirationDateReturnsValidRoleAssignment()
{
    // Arrange
    var roleCode = RoleCode.From("Manager");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

    // Act
    var roleAssignment = RoleAssignment.Create(roleCode, effectiveDate);

    // Assert
    Assert.NotNull(roleAssignment);
    Assert.Equal(roleCode, roleAssignment.RoleCode);
    Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
    Assert.Null(roleAssignment.ExpirationDate);
}
```

**検証項目**:
- 有効期間なしで生成可能
- ExpirationDate が null

---

#### テスト1.3: 複数生成 - 異なるIDが生成される

```csharp
[Fact]
public void TestCreate03_MultipleCreatesGenerateDifferentIds()
{
    // Arrange & Act
    var role1 = RoleAssignment.Create(
        RoleCode.From("Admin"), 
        new LocalDateTime(new DateTime(2026, 1, 1)));
    
    var role2 = RoleAssignment.Create(
        RoleCode.From("Manager"), 
        new LocalDateTime(new DateTime(2026, 1, 1)));

    // Assert
    Assert.NotEqual(role1.Id, role2.Id);
}
```

**検証項目**:
- 生成するたびに異なる ID が採番される

---

### グループ 2: 復元メソッド（Reconstruct）

#### テスト2.1: DB 復元 - 有効期間ありで復元

```csharp
[Fact]
public void TestReconstruct01_WithExpirationDateReturnsValidRoleAssignment()
{
    // Arrange
    var id = RoleAssignmentId.NewId();
    var roleCode = RoleCode.From("Admin");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
    var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

    // Act
    var roleAssignment = RoleAssignment.Reconstruct(id, roleCode, effectiveDate, expirationDate);

    // Assert
    Assert.NotNull(roleAssignment);
    Assert.Equal(id, roleAssignment.Id);
    Assert.Equal(roleCode, roleAssignment.RoleCode);
    Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
    Assert.Equal(expirationDate, roleAssignment.ExpirationDate);
}
```

**検証項目**:
- 指定の ID で復元される
- すべてのプロパティが正しく設定される

---

#### テスト2.2: DB 復元 - 無期限で復元

```csharp
[Fact]
public void TestReconstruct02_WithoutExpirationDateReturnsValidRoleAssignment()
{
    // Arrange
    var id = RoleAssignmentId.NewId();
    var roleCode = RoleCode.From("Manager");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

    // Act
    var roleAssignment = RoleAssignment.Reconstruct(id, roleCode, effectiveDate);

    // Assert
    Assert.Equal(id, roleAssignment.Id);
    Assert.Null(roleAssignment.ExpirationDate);
}
```

**検証項目**:
- 無期限で復元可能

---

### グループ 3: ビジネスロジック（IsActive）

#### テスト3.1: 有効期間内 - IsActive = true

```csharp
[Fact]
public void TestIsActive01_WithinEffectivePeriodReturnsTrue()
{
    // Arrange
    var roleAssignment = RoleAssignment.Create(
        RoleCode.From("Admin"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)),
        new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));
    
    var checkDate = new LocalDateTime(new DateTime(2026, 6, 15, 12, 0, 0));

    // Act
    var isActive = roleAssignment.IsActive(checkDate);

    // Assert
    Assert.True(isActive);
}
```

**検証項目**:
- 開始日 ～ 終了日 の間は true

---

#### テスト3.2: 開始日前 - IsActive = false

```csharp
[Fact]
public void TestIsActive02_BeforeEffectiveDateReturnsFalse()
{
    // Arrange
    var roleAssignment = RoleAssignment.Create(
        RoleCode.From("Admin"),
        new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0)),
        new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));
    
    var checkDate = new LocalDateTime(new DateTime(2026, 5, 31, 23, 59, 59));

    // Act
    var isActive = roleAssignment.IsActive(checkDate);

    // Assert
    Assert.False(isActive);
}
```

**検証項目**:
- 開始日より前は false

---

#### テスト3.3: 終了日以後 - IsActive = false

```csharp
[Fact]
public void TestIsActive03_AfterExpirationDateReturnsFalse()
{
    // Arrange
    var roleAssignment = RoleAssignment.Create(
        RoleCode.From("Admin"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)),
        new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));
    
    var checkDate = new LocalDateTime(new DateTime(2027, 1, 1, 0, 0, 0));

    // Act
    var isActive = roleAssignment.IsActive(checkDate);

    // Assert
    Assert.False(isActive);
}
```

**検証項目**:
- 終了日以後は false

---

#### テスト3.4: 無期限 - 開始日以後は常に true

```csharp
[Fact]
public void TestIsActive04_WithoutExpirationDateAlwaysReturnsTrueAfterEffectiveDate()
{
    // Arrange
    var roleAssignment = RoleAssignment.Create(
        RoleCode.From("Admin"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    
    var checkDate = new LocalDateTime(new DateTime(2099, 12, 31, 23, 59, 59));

    // Act
    var isActive = roleAssignment.IsActive(checkDate);

    // Assert
    Assert.True(isActive);
}
```

**検証項目**:
- 終了日がない場合、開始日以後は常に true

---

#### テスト3.5: 開始日と同日 - IsActive = true

```csharp
[Fact]
public void TestIsActive05_OnEffectiveDateReturnsTrue()
{
    // Arrange
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
    var roleAssignment = RoleAssignment.Create(
        RoleCode.From("Admin"),
        effectiveDate,
        new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));

    // Act
    var isActive = roleAssignment.IsActive(effectiveDate);

    // Assert
    Assert.True(isActive);
}
```

**検証項目**:
- 開始日と同日は true

---

### グループ 4: プロパティアクセス

#### テスト4.1: プロパティ読み取り専用

```csharp
[Fact]
public void TestProperties01_PropertiesAreReadOnly()
{
    // Arrange
    var roleAssignment = RoleAssignment.Create(
        RoleCode.From("Admin"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

    // Act & Assert
    // 以下はコンパイルエラー（CS0200: Property cannot be assigned to）
    // roleAssignment.RoleCode = newCode;
    // roleAssignment.EffectiveDate = newDate;
    
    // 読み取りのみ可能
    Assert.NotNull(roleAssignment.RoleCode);
    Assert.NotEqual(DateTime.MinValue, roleAssignment.EffectiveDate.Value);
}
```

**検証項目**:
- すべてのプロパティが読み取り専用

---

#### テスト4.2: プロパティ不変性

```csharp
[Fact]
public void TestProperties02_PropertyImmutability()
{
    // Arrange
    var roleAssignment = RoleAssignment.Create(
        RoleCode.From("Admin"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

    // Act
    var code1 = roleAssignment.RoleCode;
    var code2 = roleAssignment.RoleCode;

    // Assert
    Assert.Same(code1, code2);  // 同じインスタンス
}
```

**検証項目**:
- プロパティ値が変わらない

---

### グループ 5: 等価性（Equality）

#### テスト5.1: 同一 ID なら等価

```csharp
[Fact]
public void TestEquality01_SameIdAreEqual()
{
    // Arrange
    var id = RoleAssignmentId.NewId();
    var role1 = RoleAssignment.Reconstruct(
        id,
        RoleCode.From("Admin"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    
    var role2 = RoleAssignment.Reconstruct(
        id,
        RoleCode.From("Manager"),  // 異なるロール
        new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0)));

    // Assert
    Assert.Equal(role1, role2);  // Entity<TId> は Id で比較
}
```

**検証項目**:
- 同じ ID なら等価（プロパティ値は無視）

---

#### テスト5.2: 異なる ID なら非等価

```csharp
[Fact]
public void TestEquality02_DifferentIdAreNotEqual()
{
    // Arrange
    var role1 = RoleAssignment.Create(
        RoleCode.From("Admin"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    
    var role2 = RoleAssignment.Create(
        RoleCode.From("Admin"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

    // Assert
    Assert.NotEqual(role1, role2);  // 異なる ID
}
```

**検証項目**:
- 異なる ID は非等価

---

#### テスト5.3: ハッシュコード一致

```csharp
[Fact]
public void TestEquality03_HashCodesAreEqual()
{
    // Arrange
    var id = RoleAssignmentId.NewId();
    var role1 = RoleAssignment.Reconstruct(id, RoleCode.From("Admin"), new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    var role2 = RoleAssignment.Reconstruct(id, RoleCode.From("Admin"), new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

    // Assert
    Assert.Equal(role1.GetHashCode(), role2.GetHashCode());
}
```

**検証項目**:
- 同一 ID のハッシュコードが一致

---

#### テスト5.4: Dictionary キーとして使用可能

```csharp
[Fact]
public void TestEquality04_CanBeUsedAsDictionaryKey()
{
    // Arrange
    var id = RoleAssignmentId.NewId();
    var role1 = RoleAssignment.Reconstruct(id, RoleCode.From("Admin"), new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    var role2 = RoleAssignment.Reconstruct(id, RoleCode.From("Admin"), new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    
    var dict = new Dictionary<RoleAssignment, string>();

    // Act
    dict.Add(role1, "Role1");
    dict[role2] = "Role2";  // 同じ ID なので上書き

    // Assert
    Assert.Single(dict);
    Assert.Equal("Role2", dict[role1]);
}
```

**検証項目**:
- Dictionary のキーとして使用可能

---

#### テスト5.5: null との比較

```csharp
[Fact]
public void TestEquality05_EqualsNullReturnsFalse()
{
    // Arrange
    var roleAssignment = RoleAssignment.Create(
        RoleCode.From("Admin"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

    // Act & Assert
    Assert.False(roleAssignment.Equals(null));
}
```

**検証項目**:
- null との比較は false

---

### グループ 6: 統合テスト

#### テスト6.1: すべてのプロパティが整合性を持つ

```csharp
[Fact]
public void TestIntegration01_AllPropertiesAreCoherent()
{
    // Arrange
    var roleCode = RoleCode.From("Admin");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
    var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

    // Act
    var roleAssignment = RoleAssignment.Create(roleCode, effectiveDate, expirationDate);

    // Assert - すべてのプロパティが有効
    Assert.NotEqual(Guid.Empty, roleAssignment.Id.Value);
    Assert.Equal(roleCode, roleAssignment.RoleCode);
    Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
    Assert.Equal(expirationDate, roleAssignment.ExpirationDate);
    Assert.True(roleAssignment.IsActive(new LocalDateTime(new DateTime(2026, 6, 15, 0, 0, 0))));
}
```

**検証項目**:
- すべてのプロパティが正しく設定されている
- ビジネスロジックが正しく動作する

---

#### テスト6.2: Create → Reconstruct ラウンドトリップ

```csharp
[Fact]
public void TestIntegration02_CreateAndReconstructAreConsistent()
{
    // Arrange
    var roleCode = RoleCode.From("Manager");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

    // Act
    var created = RoleAssignment.Create(roleCode, effectiveDate);
    var reconstructed = RoleAssignment.Reconstruct(created.Id, roleCode, effectiveDate);

    // Assert
    Assert.Equal(created.Id, reconstructed.Id);
    Assert.Equal(created.RoleCode, reconstructed.RoleCode);
    Assert.Equal(created.EffectiveDate, reconstructed.EffectiveDate);
}
```

**検証項目**:
- Create と Reconstruct の一貫性

---

## 📊 テスト統計

> **実装状況（2026-09-26 時点）**: 仕様 20 観点のうち、実装済みは 6 件（VO-CONS-01/02/04、VO-METHOD-02/03/05）。残り 14 観点（VO-CONS-03/05、VO-PROP-01〜03、VO-METHOD-01/04、VO-EQ-01、VO-NE-01/02、VO-HC-01/02、VO-INTEG-01/02）は「⏸️ 未実装」で、テストコードにない。旧版はこれらを「✅」と記載していたため訂正した。VO-CONS-04・VO-METHOD-05 のテスト名は、実在する名前に直した。以下の表は、仕様としての全体像（実装済みの件数ではない）

| グループ | テスト数 | 対象メソッド |
|---------|---------|------------|
| グループ 1 | 3 | Create |
| グループ 2 | 2 | Reconstruct |
| グループ 3 | 5 | IsActive |
| グループ 4 | 2 | プロパティ |
| グループ 5 | 5 | Equals, GetHashCode |
| グループ 6 | 2 | 統合 |
| **合計** | **19** | - |

---

## 参考資料

- **DepartmentMembershipTests**: 同じパターンのテスト例
  - ファイル: `tests/Contexts/Employee.Domain.Tests/Entities/DepartmentMembershipTests.cs`

- **Xunit ドキュメント**:
  - Assert: https://xunit.net/docs/getting-started/
  - Fact/Theory: https://xunit.net/docs/getting-started/

