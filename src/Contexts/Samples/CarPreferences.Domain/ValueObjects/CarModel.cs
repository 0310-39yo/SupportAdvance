using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// 車のモデルを表すValueObject
/// 内部値を int で管理し、定義済みインスタンスを静的フィールドで提供する
/// UI から未設定状態が発生する可能性があるため IOptionalValueObject を実装
/// </summary>
public sealed class CarModel : EnumValueObject<int>, IOptionalValueObject<CarModel, int>, IEquatable<CarModel>
{
    /// <summary>不明</summary>
    public static readonly CarModel Unknown = new(0);

    /// <summary>セダン</summary>
    public static readonly CarModel Sedan = new(1);

    /// <summary>SUV</summary>
    public static readonly CarModel SportUtility = new(2);

    /// <summary>ハッチバック</summary>
    public static readonly CarModel Hatchback = new(3);

    /// <summary>クーペ</summary>
    public static readonly CarModel Coupe = new(4);

    /// <summary>ワンボックス</summary>
    public static readonly CarModel Minivan = new(5);

    /// <summary>その他</summary>
    public static readonly CarModel Other = new(6);

    /// <summary>未設定状態のCarModelインスタンス</summary>
    private static readonly CarModel UnsetInstance = new();

    /// <summary>
    /// 指定された内部値からCarModelのインスタンスを生成する
    /// </summary>
    /// <param name="value">内部値</param>
    private CarModel(int value) : base(value)
    {
    }

    /// <summary>
    /// 未設定状態のCarModelを生成するコンストラクタ
    /// </summary>
    private CarModel() : base()
    {
    }

    /// <summary>
    /// 指定された内部値からCarModelのインスタンスを生成する
    /// </summary>
    /// <param name="value">内部値</param>
    /// <returns>生成されたCarModelのインスタンス</returns>
    public static CarModel From(int value) => new(value);

    /// <summary>
    /// 未設定状態のCarModelのインスタンスを生成する
    /// </summary>
    /// <returns>未設定状態のCarModelのインスタンス</returns>
    public static CarModel Unset() => UnsetInstance;

    /// <summary>
    /// 指定された内部値からCarModelの生成を試みる
    /// null の場合は Unset() を返して true を返す（正常処理）
    /// 値が無効な場合は Unset() を返して false を返す（エラー処理）
    /// </summary>
    /// <param name="input">内部値（null許容）</param>
    /// <param name="result">生成されたCarModelのインスタンス</param>
    /// <returns>生成に成功した場合またはnullの場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(int? input, out CarModel result)
    {
        if (!input.HasValue)
        {
            result = Unset();
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            result = Unset();
            return false;
        }
    }

    /// <summary>
    /// 指定された内部値からCarModelの生成を試みる（整数値による呼び出し）
    /// </summary>
    /// <param name="input">内部値</param>
    /// <param name="result">生成されたCarModelのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(int input, out CarModel result) => TryFrom((int?)input, out result);

    /// <summary>
    /// 指定されたCarModelと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のCarModel</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(CarModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return IsSet == other.IsSet && ValueField == other.ValueField;
    }

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as CarModel);

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => HashCode.Combine(IsSet, ValueField);

    /// <summary>
    /// 指定された内部値の妥当性を検証する
    /// </summary>
    /// <param name="value">検証対象の内部値</param>
    /// <exception cref="ArgumentOutOfRangeException">値が有効な範囲外の場合</exception>
    protected override void Validate(int value)
    {
        if (value < 0 || value > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"CarModel must be between 0 and 6, but got {value}");
        }
    }

    /// <summary>
    /// 車のモデルの業務名称を返す
    /// </summary>
    /// <returns>業務名称</returns>
    protected override string GetDisplayName() => ValueField switch
    {
        0 => "不明",
        1 => "セダン",
        2 => "SUV",
        3 => "ハッチバック",
        4 => "クーペ",
        5 => "ワンボックス",
        6 => "その他",
        _ => throw new ArgumentOutOfRangeException(nameof(ValueField), $"Unknown car model: {ValueField}")
    };
}
