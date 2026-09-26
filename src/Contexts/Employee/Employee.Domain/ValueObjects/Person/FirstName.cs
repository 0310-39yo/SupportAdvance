using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;

/// <summary>
/// 個人の名を表す ValueObject
/// </summary>
/// <remarks>
/// <para>【型】string のラッパー</para>
/// <para>【制約】1文字以上100文字以下、null 不可</para>
/// </remarks>
public sealed class FirstName : ValueObject, IEquatable<FirstName>
{
    /// <summary>
    /// 名の値
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 指定された名から FirstName を生成する（プライベートコンストラクタ）
    /// </summary>
    private FirstName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定された値からの FirstName の生成
    /// </summary>
    /// <param name="value">名</param>
    /// <returns>FirstName インスタンス</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> が <see langword="null"/>・空文字・空白のみの場合、または 100 文字を超える場合</exception>
    public static FirstName From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("名は空文字列または null にできません。", nameof(value));
        }

        if (value.Length > 100)
        {
            throw new ArgumentException("名は100文字以下である必要があります。", nameof(value));
        }

        return new FirstName(value);
    }

    /// <summary>
    /// DB値から FirstName の復元
    /// </summary>
    /// <param name="value">DB から読み込んだ名</param>
    /// <param name="result">成功した場合は復元したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合は <see langword="true"/>。<see langword="null"/>・空文字・空白のみ・100 文字超の場合は <see langword="false"/>（必須項目）</returns>
    public static bool TryFromDbValue(string? value, out FirstName result)
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
    public override bool Equals(object? obj) => Equals(obj as FirstName);

    /// <inheritdoc/>
    public bool Equals(FirstName? other)
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
    /// 文字列表現の取得
    /// </summary>
    /// <returns>名の値そのもの</returns>
    public override string ToString() => Value;

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }
}
