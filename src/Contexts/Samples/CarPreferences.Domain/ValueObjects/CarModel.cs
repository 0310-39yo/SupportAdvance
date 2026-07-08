using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

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
    /// 指定された内部値からインスタンスを生成するコンストラクタ
    /// </summary>
    /// <remarks>
    /// 【責務】指定された内部値を保持するインスタンスを構築する
    /// </remarks>
    /// <param name="value">内部値</param>
    private CarModel(int value) : base(value)
    {
    }

    /// <summary>
    /// 未設定状態のインスタンスを生成するコンストラクタ
    /// </summary>
    /// <remarks>
    /// 【責務】未設定状態を表現するインスタンスを構築する
    /// </remarks>
    private CarModel() : base()
    {
    }

    /// <summary>
    /// 保持する内部値を読み取り専用で取得
    /// </summary>
    public int? Value => IsSet ? ValueField : null;

    /// <summary>
    /// 指定された内部値からインスタンスを生成する
    /// </summary>
    /// <remarks>
    /// 【責務】指定された内部値を検証してインスタンスを生成する
    /// </remarks>
    /// <param name="value">内部値</param>
    /// <returns>検証済みで設定状態のインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">値が有効な範囲外の場合</exception>
    public static CarModel From(int value) => new(value);

    /// <summary>
    /// 未設定状態のインスタンスを生成する
    /// </summary>
    /// <remarks>
    /// 【責務】未設定状態を表現するインスタンスを返す
    /// </remarks>
    /// <returns>未設定状態のインスタンス</returns>
    public static CarModel Unset() => UnsetInstance;

    /// <summary>
    /// 指定された内部値からインスタンスの生成を試みる（nullable 版）
    /// </summary>
    /// <remarks>
    /// 【責務】外部入力を安全に処理する（null は未設定状態に、検証失敗時も未設定状態に変換）
    /// </remarks>
    /// <param name="input">内部値（null 許容）</param>
    /// <param name="result">生成結果を受け取る out パラメータ</param>
    /// <returns>生成に成功した場合、または null 入力を Unset に変換した場合は true；検証失敗時は false</returns>
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
    /// 指定された内部値からインスタンスの生成を試みる（非 nullable 版）
    /// </summary>
    /// <remarks>
    /// 【責務】nullable 版の TryFrom を呼び出す便利メソッド
    /// </remarks>
    /// <param name="input">内部値</param>
    /// <param name="result">生成結果を受け取る out パラメータ</param>
    /// <returns>生成に成功した場合は true；検証失敗時は false</returns>
    public static bool TryFrom(int input, out CarModel result) => TryFrom((int?)input, out result);

    /// <summary>
    /// 指定された CarModel インスタンスと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象の CarModel</param>
    /// <returns>等価である場合は true、そうでない場合は false</returns>
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
    /// <returns>等価である場合は true、そうでない場合は false</returns>
    public override bool Equals(object? obj) => Equals(obj as CarModel);

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => HashCode.Combine(IsSet, ValueField);

    /// <summary>
    /// 内部値の妥当性を検証する
    /// </summary>
    /// <remarks>
    /// 【責務】内部値が 0～6 の有効な範囲かをチェックする
    /// </remarks>
    /// <param name="value">検証対象の内部値</param>
    /// <exception cref="ArgumentOutOfRangeException">値が有効な範囲外の場合</exception>
    public override void Validate(int value)
    {
        if (value < 0 || value > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"CarModel must be between 0 and 6, but got {value}");
        }
    }

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す（IsSet を除く）
    /// </summary>
    /// <remarks>
    /// IsSet は ValueObject.GetEqualityComponents で自動的に先頭に付加される
    /// </remarks>
    /// <returns>ValueField（IsSet = true の場合）を含むコンポーネント列</returns>
    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet)
        {
            yield return ValueField;
        }
    }

    /// <summary>
    /// 車のモデルの業務名称を返す
    /// </summary>
    /// <remarks>
    /// 【責務】内部値を業務上の日本語表現に変換する
    /// </remarks>
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
