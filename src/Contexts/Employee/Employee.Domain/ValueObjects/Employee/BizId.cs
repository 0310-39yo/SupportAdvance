using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員の通し番号を表すValueObject（ビジネスID）
/// 【範囲】1001以上（1000は管理者予約。区分ごとの有効範囲が検証される）
/// 【表示】左0埋めで5桁（例："01234"）
/// 【責務】ビジネスIDとしての従業員番号の管理と検証
/// </summary>
public sealed class BizId : PrimitiveValueObject<int>, IEquatable<BizId>
{
    /// <summary>
    /// ビジネスID として使用できる最小値
    /// </summary>
    /// <remarks>
    /// <para>【値の根拠】<see cref="ReservedValue"/>（1000）の次の値。区分ごとの有効範囲は <see cref="BizCode"/> で別途検証</para>
    /// </remarks>
    public const int MinValue = 1001;

    /// <summary>
    /// システム管理者用に予約済みのビジネスID
    /// </summary>
    /// <remarks>
    /// <para>【重要】一般の従業員には使用不可。<see cref="Validate"/> で拒否</para>
    /// </remarks>
    public const int ReservedValue = 1000;

    private BizId(int value) : base(value, true)
    {
    }

    /// <summary>
    /// 整数値からの <see cref="BizId"/> の生成
    /// </summary>
    /// <param name="value">従業員の通し番号</param>
    /// <returns>生成したインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> が <see cref="ReservedValue"/> の場合、または <see cref="MinValue"/> 未満の場合
    /// </exception>
    public static BizId From(int value) => new(value);

    /// <summary>
    /// 整数値からの <see cref="BizId"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="input">従業員の通し番号。外部入力のため <see langword="null"/> 許容</param>
    /// <param name="result">成功した場合は生成したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>
    /// 成功した場合は <see langword="true"/>。<paramref name="input"/> が <see langword="null"/> の場合（必須項目）、
    /// または範囲外の場合は <see langword="false"/>
    /// </returns>
    public static bool TryFrom(int? input, out BizId result)
    {
        result = null!;
        if (!input.HasValue)
        {
            return false;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// DB 値からの <see cref="BizId"/> の生成（Infrastructure 層での型変換用）
    /// </summary>
    /// <param name="value">DB から読み込んだ従業員の通し番号</param>
    /// <returns>生成したインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> が <see cref="ReservedValue"/> の場合、または <see cref="MinValue"/> 未満の場合
    /// </exception>
    public static BizId FromDbValue(int value) => new(value);

    /// <summary>
    /// DB 値からの <see cref="BizId"/> 生成の試行（Infrastructure 層での型変換用）。例外の送出なし
    /// </summary>
    /// <param name="input">DB から読み込んだ従業員の通し番号</param>
    /// <param name="result">成功した場合は生成したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>
    /// 成功した場合は <see langword="true"/>。<paramref name="input"/> が <see langword="null"/> の場合（必須項目）、
    /// または範囲外の場合は <see langword="false"/>
    /// </returns>
    public static bool TryFromDbValue(int? input, out BizId result)
    {
        result = null!;
        if (!input.HasValue)
        {
            return false;
        }

        try
        {
            result = FromDbValue(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 従業員の通し番号
    /// </summary>
    /// <value><see cref="MinValue"/> 以上の整数</value>
    public int Value => ValueField;

    /// <summary>
    /// 表示用の文字列表現
    /// </summary>
    /// <returns>左を 0 で埋めた 5 桁の文字列（例: <c>01234</c>）。5 桁を超える値はそのままの桁数</returns>
    public override string ToString() => ValueField.ToString("D5");

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as BizId);

    /// <summary>
    /// 指定した <see cref="BizId"/> と等しいかどうかの判定
    /// </summary>
    /// <param name="other">比較対象</param>
    /// <returns><see cref="Value"/> が等しい場合は <see langword="true"/></returns>
    public bool Equals(BizId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ValueField == other.ValueField;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => ValueField.GetHashCode();

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return ValueField;
    }

    /// <summary>
    /// 正規化済みの値の検証
    /// </summary>
    /// <param name="normalized">検証する従業員の通し番号</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="normalized"/> が <see cref="ReservedValue"/> の場合、または <see cref="MinValue"/> 未満の場合
    /// </exception>
    /// <remarks>
    /// <para>【呼び出し元】基底クラスのコンストラクターで自動実行</para>
    /// </remarks>
    public override void Validate(int normalized)
    {
        base.Validate(normalized);
        if (normalized == ReservedValue)
        {
            throw new ArgumentOutOfRangeException(nameof(normalized), normalized,
                $"BizId {ReservedValue} is reserved for system administrator.");
        }

        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(normalized), normalized,
                $"BizId must be {MinValue} or higher ({ReservedValue} is reserved).");
        }
    }
}
