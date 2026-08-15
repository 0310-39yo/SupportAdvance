using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;

/// <summary>
/// 個人の名を表す ValueObject
/// 【型】string のラッパー
/// 【制約】1文字以上100文字以下、null 不可
/// </summary>
public sealed class FirstName : ValueObject, IEquatable<FirstName>
{
    /// <summary>名の値</summary>
    public string Value { get; }

    /// <summary>
    /// 指定された名から FirstName を生成する（プライベートコンストラクタ）
    /// </summary>
    private FirstName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定された値から FirstName を生成する
    /// </summary>
    /// <param name="value">名</param>
    /// <returns>FirstName インスタンス</returns>
    public static FirstName From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("名は空文字列または null にできません。", nameof(value));
        if (value.Length > 100)
            throw new ArgumentException("名は100文字以下である必要があります。", nameof(value));

        return new(value);
    }

    /// <summary>
    /// DB値から FirstName を復元する
    /// </summary>
    public static bool TryFromDbValue(string? value, out FirstName result)
    {
        result = null!;

        if (string.IsNullOrWhiteSpace(value))
            return false;

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

    /// <summary>
    /// 指定された FirstName と等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as FirstName);

    /// <summary>
    /// 指定された FirstName と等価かどうかを判定する
    /// </summary>
    public bool Equals(FirstName? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Value == other.Value;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す
    /// </summary>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }
}
