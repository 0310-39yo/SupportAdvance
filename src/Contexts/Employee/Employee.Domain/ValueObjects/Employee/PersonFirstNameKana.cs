using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 個人の名（カナ）を表す ValueObject
/// 【型】string のラッパー
/// 【制約】1文字以上100文字以下、null 不可、カナ文字のみ
/// </summary>
public sealed class PersonFirstNameKana : ValueObject, IEquatable<PersonFirstNameKana>
{
    /// <summary>名（カナ）の値</summary>
    public string Value { get; }

    /// <summary>
    /// 指定された名（カナ）から PersonFirstNameKana を生成する（プライベートコンストラクタ）
    /// </summary>
    private PersonFirstNameKana(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定された値から PersonFirstNameKana を生成する
    /// </summary>
    /// <param name="value">名（カナ）</param>
    /// <returns>PersonFirstNameKana インスタンス</returns>
    public static PersonFirstNameKana From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("名（カナ）は空文字列または null にできません。", nameof(value));
        if (value.Length > 100)
            throw new ArgumentException("名（カナ）は100文字以下である必要があります。", nameof(value));

        return new(value);
    }

    /// <summary>
    /// DB値から PersonFirstNameKana を復元する
    /// </summary>
    public static bool TryFromDbValue(string? value, out PersonFirstNameKana result)
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
    /// 指定された PersonFirstNameKana と等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as PersonFirstNameKana);

    /// <summary>
    /// 指定された PersonFirstNameKana と等価かどうかを判定する
    /// </summary>
    public bool Equals(PersonFirstNameKana? other)
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
