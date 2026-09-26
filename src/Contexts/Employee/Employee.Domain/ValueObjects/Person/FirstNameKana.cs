using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;

/// <summary>
/// 個人の名（カナ）を表す ValueObject
/// </summary>
/// <remarks>
/// <para>【型】string のラッパー</para>
/// <para>【制約】1文字以上100文字以下、null 不可、カナ文字のみ</para>
/// </remarks>
public sealed class FirstNameKana : ValueObject, IEquatable<FirstNameKana>
{
    /// <summary>
    /// 名（カナ）の値
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 指定された名（カナ）から FirstNameKana を生成する（プライベートコンストラクタ）
    /// </summary>
    private FirstNameKana(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定された値からの FirstNameKana の生成
    /// </summary>
    /// <param name="value">名（カナ）</param>
    /// <returns>FirstNameKana インスタンス</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> が <see langword="null"/>・空文字・空白のみの場合、または 100 文字を超える場合</exception>
    public static FirstNameKana From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("名（カナ）は空文字列または null にできません。", nameof(value));
        }

        if (value.Length > 100)
        {
            throw new ArgumentException("名（カナ）は100文字以下である必要があります。", nameof(value));
        }

        return new FirstNameKana(value);
    }

    /// <summary>
    /// DB値から FirstNameKana の復元
    /// </summary>
    /// <param name="value">DB から読み込んだ名（カナ）</param>
    /// <param name="result">成功した場合は復元したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合は <see langword="true"/>。<see langword="null"/>・空文字・空白のみ・100 文字超の場合は <see langword="false"/>（必須項目）</returns>
    public static bool TryFromDbValue(string? value, out FirstNameKana result)
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
    public override bool Equals(object? obj) => Equals(obj as FirstNameKana);

    /// <inheritdoc/>
    public bool Equals(FirstNameKana? other)
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
    /// <returns>名（カナ）の値そのもの</returns>
    public override string ToString() => Value;

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }
}
