# PermissionAssignment - 単体テスト仕様書

**対象者**: テスト実装者

**版**: 1.0  
**作成日**: 2026-08-10

---

## 📖 テスト戦略

### テスト範囲

| 対象 | 内容 |
|------|------|
| **ファクトリメソッド** | Create, Reconstruct |
| **プロパティ** | PermissionCode, EffectiveDate, ExpirationDate（読み取り、不変性） |
| **ビジネスロジック** | IsActive（有効期間判定） |
| **等価性** | Equals, GetHashCode, Dictionary キー使用 |
| **統合テスト** | すべてのプロパティの整合性 |

### テスト方法

- **ユニットテスト**: Xunit
- **モック**: なし（ビジネスロジックのみ）
- **テストデータ**: 固定の LocalDateTime を使用

---

## 🧪 テストケース

### グループ 1: 生成メソッド（Create）

#### テスト1.1: 正常系 - 有効期間ありで生成

```csharp
[Fact]
public void TestCreate01_WithExpirationDateReturnsValidPermissionAssignment()
{
    // Arrange
    var permissionCode = PermissionCode.From("read");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
    var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

    // Act
    var permissionAssignment = PermissionAssignment.Create(permissionCode, effectiveDate, expirationDate);

    // Assert
    Assert.NotNull(permissionAssignment);
    Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
    Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate);
    Assert.Equal(expirationDate, permissionAssignment.ExpirationDate);
    Assert.NotEqual(Guid.Empty, permissionAssignment.Id.Value);
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
public void TestCreate02_WithoutExpirationDateReturnsValidPermissionAssignment()
{
    // Arrange
    var permissionCode = PermissionCode.From("write");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

    // Act
    var permissionAssignment = PermissionAssignment.Create(permissionCode, effectiveDate);

    // Assert
    Assert.NotNull(permissionAssignment);
    Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
    Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate);
    Assert.Null(permissionAssignment.ExpirationDate);
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
    var perm1 = PermissionAssignment.Create(
        PermissionCode.From("read"), 
        new LocalDateTime(new DateTime(2026, 1, 1)));
    
    var perm2 = PermissionAssignment.Create(
        PermissionCode.From("write"), 
        new LocalDateTime(new DateTime(2026, 1, 1)));

    // Assert
    Assert.NotEqual(perm1.Id, perm2.Id);
}
```

**検証項目**:
- 生成するたびに異なる ID が採番される

---

### グループ 2: 復元メソッド（Reconstruct）

#### テスト2.1: DB 復元 - 有効期間ありで復元

```csharp
[Fact]
public void TestReconstruct01_WithExpirationDateReturnsValidPermissionAssignment()
{
    // Arrange
    var id = PermissionAssignmentId.NewId();
    var permissionCode = PermissionCode.From("read");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
    var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

    // Act
    var permissionAssignment = PermissionAssignment.Reconstruct(id, permissionCode, effectiveDate, expirationDate);

    // Assert
    Assert.NotNull(permissionAssignment);
    Assert.Equal(id, permissionAssignment.Id);
    Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
    Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate);
    Assert.Equal(expirationDate, permissionAssignment.ExpirationDate);
}
```

**検証項目**:
- 指定の ID で復元される
- すべてのプロパティが正しく設定される

---

#### テスト2.2: DB 復元 - 無期限で復元

```csharp
[Fact]
public void TestReconstruct02_WithoutExpirationDateReturnsValidPermissionAssignment()
{
    // Arrange
    var id = PermissionAssignmentId.NewId();
    var permissionCode = PermissionCode.From("write");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

    // Act
    var permissionAssignment = PermissionAssignment.Reconstruct(id, permissionCode, effectiveDate);

    // Assert
    Assert.Equal(id, permissionAssignment.Id);
    Assert.Null(permissionAssignment.ExpirationDate);
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
    var permissionAssignment = PermissionAssignment.Create(
        PermissionCode.From("read"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)),
        new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));
    
    var checkDate = new LocalDateTime(new DateTime(2026, 6, 15, 12, 0, 0));

    // Act
    var isActive = permissionAssignment.IsActive(checkDate);

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
    var permissionAssignment = PermissionAssignment.Create(
        PermissionCode.From("read"),
        new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0)),
        new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));
    
    var checkDate = new LocalDateTime(new DateTime(2026, 5, 31, 23, 59, 59));

    // Act
    var isActive = permissionAssignment.IsActive(checkDate);

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
    var permissionAssignment = PermissionAssignment.Create(
        PermissionCode.From("read"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)),
        new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));
    
    var checkDate = new LocalDateTime(new DateTime(2027, 1, 1, 0, 0, 0));

    // Act
    var isActive = permissionAssignment.IsActive(checkDate);

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
    var permissionAssignment = PermissionAssignment.Create(
        PermissionCode.From("read"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    
    var checkDate = new LocalDateTime(new DateTime(2099, 12, 31, 23, 59, 59));

    // Act
    var isActive = permissionAssignment.IsActive(checkDate);

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
    var permissionAssignment = PermissionAssignment.Create(
        PermissionCode.From("read"),
        effectiveDate,
        new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));

    // Act
    var isActive = permissionAssignment.IsActive(effectiveDate);

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
    var permissionAssignment = PermissionAssignment.Create(
        PermissionCode.From("read"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

    // Act & Assert
    // 以下はコンパイルエラー（CS0200: Property cannot be assigned to）
    // permissionAssignment.PermissionCode = newCode;
    // permissionAssignment.EffectiveDate = newDate;
    
    // 読み取りのみ可能
    Assert.NotNull(permissionAssignment.PermissionCode);
    Assert.NotEqual(DateTime.MinValue, permissionAssignment.EffectiveDate.Value);
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
    var permissionAssignment = PermissionAssignment.Create(
        PermissionCode.From("read"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

    // Act
    var code1 = permissionAssignment.PermissionCode;
    var code2 = permissionAssignment.PermissionCode;

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
    var id = PermissionAssignmentId.NewId();
    var perm1 = PermissionAssignment.Reconstruct(
        id,
        PermissionCode.From("read"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    
    var perm2 = PermissionAssignment.Reconstruct(
        id,
        PermissionCode.From("write"),
        new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0)));

    // Assert
    Assert.Equal(perm1, perm2);  // Entity<TId> は Id で比較
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
    var perm1 = PermissionAssignment.Create(
        PermissionCode.From("read"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    
    var perm2 = PermissionAssignment.Create(
        PermissionCode.From("read"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

    // Assert
    Assert.NotEqual(perm1, perm2);
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
    var id = PermissionAssignmentId.NewId();
    var perm1 = PermissionAssignment.Reconstruct(id, PermissionCode.From("read"), new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    var perm2 = PermissionAssignment.Reconstruct(id, PermissionCode.From("read"), new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

    // Assert
    Assert.Equal(perm1.GetHashCode(), perm2.GetHashCode());
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
    var id = PermissionAssignmentId.NewId();
    var perm1 = PermissionAssignment.Reconstruct(id, PermissionCode.From("read"), new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    var perm2 = PermissionAssignment.Reconstruct(id, PermissionCode.From("read"), new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));
    
    var dict = new Dictionary<PermissionAssignment, string>();

    // Act
    dict.Add(perm1, "Permission1");
    dict[perm2] = "Permission2";  // 同じ ID なので上書き

    // Assert
    Assert.Single(dict);
    Assert.Equal("Permission2", dict[perm1]);
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
    var permissionAssignment = PermissionAssignment.Create(
        PermissionCode.From("read"),
        new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

    // Act & Assert
    Assert.False(permissionAssignment.Equals(null));
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
    var permissionCode = PermissionCode.From("read");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
    var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

    // Act
    var permissionAssignment = PermissionAssignment.Create(permissionCode, effectiveDate, expirationDate);

    // Assert - すべてのプロパティが有効
    Assert.NotEqual(Guid.Empty, permissionAssignment.Id.Value);
    Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
    Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate);
    Assert.Equal(expirationDate, permissionAssignment.ExpirationDate);
    Assert.True(permissionAssignment.IsActive(new LocalDateTime(new DateTime(2026, 6, 15, 0, 0, 0))));
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
    var permissionCode = PermissionCode.From("write");
    var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

    // Act
    var created = PermissionAssignment.Create(permissionCode, effectiveDate);
    var reconstructed = PermissionAssignment.Reconstruct(created.Id, permissionCode, effectiveDate);

    // Assert
    Assert.Equal(created.Id, reconstructed.Id);
    Assert.Equal(created.PermissionCode, reconstructed.PermissionCode);
    Assert.Equal(created.EffectiveDate, reconstructed.EffectiveDate);
}
```

**検証項目**:
- Create と Reconstruct の一貫性

---

## 📊 テスト統計

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

- **RoleAssignmentTests**: 同じパターンのテスト例
  - ファイル: `tests/Contexts/Employee.Domain.Tests/Entities/RoleAssignmentTests.cs`

- **Xunit ドキュメント**:
  - Assert: https://xunit.net/docs/getting-started/
  - Fact/Theory: https://xunit.net/docs/getting-started/
