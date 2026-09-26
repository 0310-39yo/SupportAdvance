using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Department.Domain.ValueObjects;

/// <summary>
/// 部署名を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【制約】必須。空文字は不可、<see cref="MaxLength"/> 文字以内（<c>m_departments.department_name</c> の <c>nvarchar(50)</c> に対応）</para>
/// <para>【null契約】必須。<see langword="null"/> や空文字の入力は <see cref="TryFrom"/> で失敗。Unset なし</para>
/// <para>【設計】他の BC（Employee）には、名前が無い場合を含む表示用の別の型（<c>DepartmentDisplayName</c>）で渡す。この型は Department BC の内側で使用</para>
/// </remarks>
public sealed class DepartmentName : PrimitiveValueObject<string>, IEquatable<DepartmentName>
{
    /// <summary>
    /// 部署名の最大文字数
    /// </summary>
    /// <remarks>
    /// <para>【値の根拠】DB の <c>m_departments.department_name</c> の型（<c>nvarchar(50)</c>）に合わせた値</para>
    /// </remarks>
    public const int MaxLength = 50;

    /// <summary>
    /// 部署名
    /// </summary>
    public string Value => ValueField;

    /// <summary>
    /// 指定された部署名による初期化。生成は <see cref="From"/>／<see cref="TryFrom"/> を使用
    /// </summary>
    /// <param name="value">部署名</param>
    /// <remarks>
    /// <para>【注意】検証（<see cref="Validate"/>）は基底クラスのコンストラクターで自動実行</para>
    /// </remarks>
    private DepartmentName(string value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された部署名を持つ <see cref="DepartmentName"/> の生成
    /// </summary>
    /// <param name="value">部署名（1 文字以上 <see cref="MaxLength"/> 文字以内）</param>
    /// <returns>指定された部署名を持つインスタンス</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> が <see langword="null"/>、空文字、または <see cref="MaxLength"/> 文字を超える場合</exception>
    public static DepartmentName From(string value) => new(value);

    /// <summary>
    /// 部署名からの <see cref="DepartmentName"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="input">部署名。<see langword="null"/> は失敗（必須）</param>
    /// <param name="result">成功した場合は生成したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合は <see langword="true"/>。<paramref name="input"/> が <see langword="null"/>、空文字、または <see cref="MaxLength"/> 文字を超える場合は <see langword="false"/></returns>
    public static bool TryFrom(string? input, out DepartmentName result)
    {
        result = null!;

        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        try
        {
            result = From(input);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as DepartmentName);

    /// <inheritdoc/>
    public bool Equals(DepartmentName? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value; // 大文字小文字区別
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 文字列表現の取得
    /// </summary>
    /// <returns>部署名そのもの</returns>
    public override string ToString() => Value;

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException"><paramref name="normalized"/> が空文字、または <see cref="MaxLength"/> 文字を超える場合</exception>
    public override void Validate(string normalized)
    {
        base.Validate(normalized);

        if (string.IsNullOrEmpty(normalized))
        {
            throw new ArgumentException("DepartmentName must not be null or empty.", nameof(normalized));
        }

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException(
                $"DepartmentName must be {MaxLength} characters or fewer.",
                nameof(normalized));
        }
    }
}
