using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;

/// <summary>
/// 個人の姓を表す ValueObject
/// 【型】string のラッパー
/// 【制約】1文字以上100文字以下、null 不可
/// </summary>
public sealed class LastName : ValueObject, IEquatable<LastName>
{
    /// <summary>
    /// 姓の値
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 指定された姓から LastName を生成する（プライベートコンストラクタ）
    /// </summary>
    private LastName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定された値から LastName を生成する
    /// </summary>
    /// <param name="value">姓</param>
    /// <returns>LastName インスタンス</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> が <see langword="null"/>・空文字・空白のみの場合、または 100 文字を超える場合</exception>
    public static LastName From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("姓は空文字列または null にできません。", nameof(value));
        }

        if (value.Length > 100)
        {
            throw new ArgumentException("姓は100文字以下である必要があります。", nameof(value));
        }

        return new LastName(value);
    }

    /// <summary>
    /// DB値から LastName を復元する
    /// </summary>
    /// <param name="value">DB から読み込んだ姓</param>
    /// <param name="result">成功した場合は復元したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合は <see langword="true"/>。<see langword="null"/>・空文字・空白のみ・100 文字超の場合は <see langword="false"/>（必須項目）</returns>
    public static bool TryFromDbValue(string? value, out LastName result)
    {
        result = null!;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            result = From(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as LastName);

    /// <inheritdoc/>
    public bool Equals(LastName? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    /// <returns>姓の値そのもの</returns>
    public override string ToString() => Value;

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }
}
