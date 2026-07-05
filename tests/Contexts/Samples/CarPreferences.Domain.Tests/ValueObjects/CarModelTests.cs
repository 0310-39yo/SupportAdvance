using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using Xunit;

namespace SupportAdvance.Tests.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// CarModel ValueObject の単体テスト
/// 
/// テスト仕様書: docs/Contexts/Samples/CarPreferences.Domain/ValueObjects/CarModel_単体テスト仕様書.md
/// 対象: SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects.CarModel
/// </summary>
public class CarModelTests
{
  // ================== テストデータ ==================

  public static readonly TheoryData<int, CarModel> ValidStaticFields = new()
  {
    { 0, CarModel.Unknown },
    { 1, CarModel.Sedan },
    { 2, CarModel.SportUtility },
    { 3, CarModel.Hatchback },
    { 4, CarModel.Coupe },
    { 5, CarModel.Minivan },
    { 6, CarModel.Other },
  };

  public static readonly TheoryData<int> InvalidValues = new()
  {
    -1,
    -100,
    7,
    8,
    99,
    int.MinValue,
    int.MaxValue,
  };

  public static readonly TheoryData<int, string> DisplayNameMappings = new()
  {
    { 0, "不明" },
    { 1, "セダン" },
    { 2, "SUV" },
    { 3, "ハッチバック" },
    { 4, "クーペ" },
    { 5, "ワンボックス" },
    { 6, "その他" },
  };

  // ================== T-001: 静的フィールド初期化 ==================

  [Fact]
  public void Unknown_IsInitialized_WithValueFieldZeroAndIsSetTrue()
  {
    // Act & Assert
    Assert.True(CarModel.Unknown.IsSet);
    var hasValue = CarModel.Unknown.TryGetValue(out var value);
    Assert.True(hasValue);
    Assert.Equal(0, value);
  }

  [Fact]
  public void Sedan_IsInitialized_WithValueFieldOneAndIsSetTrue()
  {
    // Act & Assert
    Assert.True(CarModel.Sedan.IsSet);
    var hasValue = CarModel.Sedan.TryGetValue(out var value);
    Assert.True(hasValue);
    Assert.Equal(1, value);
  }

  [Theory]
  [InlineData(2, "SportUtility")]
  [InlineData(3, "Hatchback")]
  [InlineData(4, "Coupe")]
  [InlineData(5, "Minivan")]
  [InlineData(6, "Other")]
  public void VO_IS_01_StaticFields_AreInitialized_WithCorrectValues(int expectedValue, string fieldName)
  {
    // Arrange & Act
    CarModel field = expectedValue switch
    {
      2 => CarModel.SportUtility,
      3 => CarModel.Hatchback,
      4 => CarModel.Coupe,
      5 => CarModel.Minivan,
      6 => CarModel.Other,
      _ => throw new InvalidOperationException()
    };

    // Assert
    Assert.True(field.IsSet);
    Assert.True(field.TryGetValue(out var value));
    Assert.Equal(expectedValue, value);
  }

  // ================== T-002: From メソッド（正常値） ==================

  [Fact]
  public void From_WithValidValue0_ReturnsUnknownInstance()
  {
    // Act
    var result = CarModel.From(0);

    // Assert
    Assert.Equal(CarModel.Unknown, result);
    Assert.True(result.IsSet);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  [InlineData(4)]
  [InlineData(5)]
  [InlineData(6)]
  public void VO_OPT_02_From_WithValidValue1to6_ReturnsExpectedInstance(int value)
  {
    // Act
    var result = CarModel.From(value);

    // Assert
    Assert.True(result.IsSet);
    Assert.True(result.TryGetValue(out var resultValue));
    Assert.Equal(value, resultValue);
  }

  [Fact]
  public void From_WithValidValue1_ReturnsSedanEquivalent()
  {
    // Act
    var result = CarModel.From(1);

    // Assert
    Assert.Equal(CarModel.Sedan, result);
  }

  [Fact]
  public void From_WithValidValue6_ReturnsOtherEquivalent()
  {
    // Act
    var result = CarModel.From(6);

    // Assert
    Assert.Equal(CarModel.Other, result);
  }

  // ================== T-003: Unset メソッド ==================

  [Fact]
  public void Unset_ReturnsInstance_WithIsSetFalse()
  {
    // Act
    var result = CarModel.Unset();

    // Assert
    Assert.False(result.IsSet);
  }

  [Fact]
  public void Unset_MultipleInvocations_ReturnsSameInstance()
  {
    // Act
    var result1 = CarModel.Unset();
    var result2 = CarModel.Unset();

    // Assert
    Assert.Same(result1, result2);  // シングルトン
  }

  [Fact]
  public void Unset_TryGetValue_ReturnsFalse()
  {
    // Arrange
    var unset = CarModel.Unset();

    // Act
    var hasValue = unset.TryGetValue(out var value);

    // Assert
    Assert.False(hasValue);
  }

  // ================== T-004: TryFrom(int?, out CarModel) ==================

  [Fact]
  public void TryFrom_WithNullInput_ReturnsUnsetAndTrue()
  {
    // Act
    var result = CarModel.TryFrom(null, out var model);

    // Assert
    Assert.True(result);
    Assert.False(model.IsSet);
    Assert.Equal(CarModel.Unset(), model);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(3)]
  [InlineData(6)]
  public void TryFrom_WithValidValue_ReturnsFromAndTrue(int value)
  {
    // Act
    var result = CarModel.TryFrom(value, out var model);

    // Assert
    Assert.True(result);
    Assert.True(model.IsSet);
  }

  [Fact]
  public void TryFrom_WithValidValue0_ReturnsUnknownAndTrue()
  {
    // Act
    var result = CarModel.TryFrom(0, out var model);

    // Assert
    Assert.True(result);
    Assert.Equal(CarModel.Unknown, model);
  }

  [Fact]
  public void TryFrom_WithValidValue1_ReturnsSedanAndTrue()
  {
    // Act
    var result = CarModel.TryFrom(1, out var model);

    // Assert
    Assert.True(result);
    Assert.Equal(CarModel.Sedan, model);
  }

  // ================== T-005: TryFrom(int, out CarModel) - IOptionalValueObject ==================

  [Fact]
  public void TryFromInt_WithValidValue1_ReturnsTrue()
  {
    // Act
    var result = CarModel.TryFrom(1, out var model);

    // Assert
    Assert.True(result);
    Assert.Equal(CarModel.Sedan, model);
  }

  [Fact]
  public void TryFromInt_WithValidValue6_ReturnsTrue()
  {
    // Act
    var result = CarModel.From(6);

    // Assert
    Assert.True(result.IsSet);
    Assert.Equal(CarModel.Other, result);
  }

  // ================== T-006: GetDisplayName / ToString 正常系 ==================

  [Theory]
  [MemberData(nameof(DisplayNameMappings))]
  public void ToString_WithStaticFields_ReturnsExpectedDisplayName(int value, string expectedName)
  {
    // Arrange
    var carModel = CarModel.From(value);

    // Act
    var result = carModel.ToString();

    // Assert
    Assert.Equal(expectedName, result);
  }

  [Fact]
  public void Unset_ToString_ReturnsUnset()
  {
    // Act
    var result = CarModel.Unset().ToString();

    // Assert
    Assert.Equal("Unset", result);
  }

  [Fact]
  public void Unknown_ToString_ReturnsNotSpecified()
  {
    // Act
    var result = CarModel.Unknown.ToString();

    // Assert
    Assert.Equal("不明", result);
  }

  // ================== T-007: Equals / GetHashCode 正常系 ==================

  [Fact]
  public void VO_EQ_01_Sedan_EqualsSedan_ReturnsTrue()
  {
    // Act & Assert
    Assert.Equal(CarModel.Sedan, CarModel.Sedan);
  }

  [Fact]
  public void VO_EQ_01_From1_EqualsSedan_ReturnsTrue()
  {
    // Act
    var from1 = CarModel.From(1);

    // Assert
    Assert.Equal(CarModel.Sedan, from1);
  }

  [Fact]
  public void VO_HC_04_Sedan_GetHashCode_IsConsistent()
  {
    // Act
    var hashCode1 = CarModel.Sedan.GetHashCode();
    var hashCode2 = CarModel.Sedan.GetHashCode();

    // Assert
    Assert.Equal(hashCode1, hashCode2);
  }

  [Fact]
  public void VO_HC_01_From1_GetHashCode_EqualsSedanHashCode()
  {
    // Act
    var from1 = CarModel.From(1);
    var sedanHashCode = CarModel.Sedan.GetHashCode();
    var from1HashCode = from1.GetHashCode();

    // Assert
    Assert.Equal(sedanHashCode, from1HashCode);
  }

  [Fact]
  public void VO_NE_04_Sedan_EqualsNull_ReturnsFalse()
  {
    // Act & Assert
    Assert.Null(null);  // null 比較
    Assert.False(CarModel.Sedan.Equals(null));
  }

  [Fact]
  public void Sedan_EqualsObjectSedan_ReturnsTrue()
  {
    // Act & Assert
    Assert.Equal((object)CarModel.Sedan, (object)CarModel.Sedan);
  }

  // ================== T-008: From メソッド（異常値） ==================

  [Theory]
  [InlineData(-1)]
  [InlineData(-100)]
  [InlineData(7)]
  [InlineData(8)]
  [InlineData(99)]
  public void From_WithInvalidValue_ThrowsArgumentOutOfRangeException(int invalidValue)
  {
    // Act & Assert
    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => CarModel.From(invalidValue));
    Assert.Contains("0 and 6", ex.Message);
  }

  // ================== T-009: TryFrom(int?, out CarModel) - 異常値 ==================

  [Theory]
  [InlineData(-1)]
  [InlineData(7)]
  [InlineData(99)]
  public void TryFrom_WithInvalidValue_ReturnsUnsetAndFalse(int invalidValue)
  {
    // Act
    var result = CarModel.TryFrom(invalidValue, out var model);

    // Assert
    Assert.False(result);
    Assert.False(model.IsSet);
  }

  // ================== T-010: Equals メソッド（不等価） ==================

  [Fact]
  public void VO_NE_01_Sedan_NotEqualCoupe_ReturnsFalse()
  {
    // Act & Assert
    Assert.NotEqual(CarModel.Sedan, CarModel.Coupe);
  }

  [Fact]
  public void VO_NE_02_Sedan_NotEqualUnset_ReturnsFalse()
  {
    // Act & Assert
    Assert.NotEqual(CarModel.Sedan, CarModel.Unset());
  }

  [Fact]
  public void VO_EQ_04_Unset_EqualsUnset_ReturnsTrue()
  {
    // Arrange
    var unset1 = CarModel.Unset();
    var unset2 = CarModel.Unset();

    // Act & Assert
    Assert.Same(unset1, unset2);  // シングルトン
    Assert.Equal(unset1, unset2);
  }

  [Fact]
  public void Unknown_NotEqualUnset_ReturnsFalse()
  {
    // Act & Assert
    Assert.NotEqual(CarModel.Unknown, CarModel.Unset());
  }

  // ================== T-011: IOptionalValueObject インターフェース実装検証 ==================

  [Fact]
  public void Unset_StaticMethod_Exists()
  {
    // Act & Assert
    var unset = CarModel.Unset();
    Assert.NotNull(unset);
    Assert.False(unset.IsSet);
  }

  [Fact]
  public void From_StaticMethod_Exists()
  {
    // Act & Assert
    var from = CarModel.From(1);
    Assert.NotNull(from);
    Assert.True(from.IsSet);
  }

  [Fact]
  public void TryFromNullable_StaticMethod_Exists()
  {
    // Act & Assert
    var result = CarModel.TryFrom(null, out var model);
    Assert.True(result);
  }

  [Fact]
  public void TryFromInt_StaticMethod_Exists()
  {
    // Act & Assert
    var result = CarModel.TryFrom(1, out var model);
    Assert.True(result);
  }

  [Fact]
  public void TryGetValue_InstanceMethod_OnSetInstance_ReturnsTrue()
  {
    // Arrange
    var carModel = CarModel.Sedan;

    // Act
    var result = carModel.TryGetValue(out var value);

    // Assert
    Assert.True(result);
    Assert.Equal(1, value);
  }

  [Fact]
  public void TryGetValue_InstanceMethod_OnUnsetInstance_ReturnsFalse()
  {
    // Arrange
    var unset = CarModel.Unset();

    // Act
    var result = unset.TryGetValue(out var value);

    // Assert
    Assert.False(result);
  }

  // ================== T-012: DB 保存値マッピング ==================

  [Fact]
  public void Unknown_HasValueFieldZero_ForDbStorage()
  {
    // Arrange
    var unknown = CarModel.Unknown;

    // Act & Assert
    Assert.True(unknown.IsSet);
    Assert.True(unknown.TryGetValue(out var value));
    Assert.Equal(0, value);  // DB に 0 として保存され、IsSet=true で識別可能
  }

  [Fact]
  public void Unset_HasIsSetFalse_ForDbNull()
  {
    // Arrange
    var unset = CarModel.Unset();

    // Act & Assert
    Assert.False(unset.IsSet);  // IsSet=false なら DB に NULL 保存
  }

  [Fact]
  public void IsSetFlag_DifferentiatesBetweenZeroAndNull()
  {
    // Arrange
    var unknown = CarModel.Unknown;     // IsSet=true, ValueField=0
    var unset = CarModel.Unset();       // IsSet=false, ValueField=0 (internal)

    // Act & Assert
    Assert.NotEqual(unknown, unset);    // 等価性でも区別される
    Assert.True(unknown.IsSet);
    Assert.False(unset.IsSet);
  }

  // ================== T-013: DB 復元ロジック ==================

  [Fact]
  public void From0_SimulatesDbRestore_ToUnknown()
  {
    // Arrange: DB に 0 が保存されていた場合をシミュレート
    int? dbValue = 0;

    // Act
    var carModel = dbValue switch
    {
      null => CarModel.Unset(),
      0    => CarModel.Unknown,
      >= 1 and <= 6 => CarModel.From(dbValue.Value),
      _    => throw new InvalidOperationException($"Invalid value: {dbValue}")
    };

    // Assert
    Assert.Equal(CarModel.Unknown, carModel);
  }

  [Fact]
  public void UnsetFrom_SimulatesDbRestore_FromNull()
  {
    // Arrange: DB に NULL が保存されていた場合をシミュレート
    int? dbValue = null;

    // Act
    var carModel = dbValue switch
    {
      null => CarModel.Unset(),
      0    => CarModel.Unknown,
      >= 1 and <= 6 => CarModel.From(dbValue.Value),
      _    => throw new InvalidOperationException($"Invalid value: {dbValue}")
    };

    // Assert
    Assert.False(carModel.IsSet);
    Assert.Equal(CarModel.Unset(), carModel);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(5)]
  [InlineData(6)]
  public void From_SimulatesDbRestore_FromValidValue(int dbValue)
  {
    // Arrange
    int? nullableDbValue = dbValue;

    // Act
    var carModel = nullableDbValue switch
    {
      null => CarModel.Unset(),
      0    => CarModel.Unknown,
      >= 1 and <= 6 => CarModel.From(nullableDbValue.Value),
      _    => throw new InvalidOperationException($"Invalid value: {nullableDbValue}")
    };

    // Assert
    Assert.Equal(CarModel.From(dbValue), carModel);
  }

  [Fact]
  public void TryFrom_HandlesInvalidDbValue_AndReturnsUnset()
  {
    // Arrange: DB に不正な値 99 が入っていた場合
    int? invalidValue = 99;

    // Act
    var success = CarModel.TryFrom(invalidValue, out var carModel);

    // Assert
    Assert.False(success);
    Assert.False(carModel.IsSet);
  }
}
