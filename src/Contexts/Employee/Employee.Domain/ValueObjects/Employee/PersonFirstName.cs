using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 個人の名を表す ValueObject
/// 【型】string のラッパー
/// 【制約】1文字以上100文字以下、null 不可
/// </summary>
public sealed class PersonFirstName : ValueObject, IEquatable<PersonFirstName>
{
    /// <summary>名の値</summary>
    public string Value { get; }

    /// <summary>
    /// 指定された名から PersonFirstName を生成する（プライベートコンストラクタ）
    /// </summary>
    private PersonFirstName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定された値から PersonFirstName を生成する
    /// </summary>
    /// <param name="value">名</param>
    /// <returns>PersonFirstName インスタンス</returns>
    public static PersonFirstName From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("名は空文字列または null にできません。", nameof(value));
        if (value.Length > 100)
            throw new ArgumentException("名は100文字以下である必要があります。", nameof(value));

        return new(value);
    }

    /// <summary>
    /// DB値から PersonFirstName を復元する
    /// </summary>
    public static bool TryFromDbValue(string? value, out PersonFirstName result)
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
    /// 指定された PersonFirstName と等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as PersonFirstName);

    /// <summary>
    /// 指定された PersonFirstName と等価かどうかを判定する
    /// </summary>
    public bool Equals(PersonFirstName? other)
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
