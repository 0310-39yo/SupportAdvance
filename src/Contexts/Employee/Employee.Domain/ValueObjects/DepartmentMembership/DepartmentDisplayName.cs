using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;

/// <summary>
/// 所属先の部署名（表示用）を表すオプションの値オブジェクト
/// </summary>
/// <remarks>
/// <para>【用途】従業員の所属（<c>DepartmentMembership</c>）に持たせる、画面表示用の部署名の写し。名前の正本は Department BC の <c>DepartmentName</c></para>
/// <para>【null契約】任意。部署が削除済みなどで名前を取得できない場合は <see langword="null"/> ではなく <see cref="Unset"/>（<see cref="HasName"/> が <see langword="false"/>）で表現。<see cref="TryFrom"/> は <see langword="null"/> の入力を <see cref="Unset"/> に変換して成功</para>
/// <para>【設計】Employee BC は Department BC の型を参照できないため、別の型として定義。名前の検証は表示用の最低限（空文字は不可）にとどめる</para>
/// <para>【参照】docs/Assistance/Guides/null厳格性設計ガイド.md</para>
/// </remarks>
public sealed class DepartmentDisplayName : PrimitiveValueObject<string>, IEquatable<DepartmentDisplayName>
{
    /// <summary>
    /// 部署名（表示用）
    /// </summary>
    /// <value>未設定の場合は空文字（<see langword="null"/> なし）。判定は <see cref="HasName"/> を使用</value>
    public string Value => IsSet ? ValueField : string.Empty;

    /// <summary>
    /// 部署名が設定されているかどうかを示す値
    /// </summary>
    /// <value>設定済みの場合は <see langword="true"/>。<see cref="SupportAdvance.SharedKernel.ValueObjects.ValueObject.IsSet"/> の、業務上の意味に合わせた別名</value>
    public bool HasName => IsSet;

    /// <summary>
    /// 未設定状態による初期化。生成は <see cref="Unset"/> を使用
    /// </summary>
    /// <param name="isSet">設定済みかどうかを示す値</param>
    private DepartmentDisplayName(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 指定された部署名による初期化。生成は <see cref="From"/> を使用
    /// </summary>
    /// <param name="value">部署名（空文字は不可）</param>
    private DepartmentDisplayName(string value) : base(value, true)
    {
    }

    /// <summary>
    /// 部署名なしの状態を表す <see cref="DepartmentDisplayName"/> の生成
    /// </summary>
    /// <returns><see cref="HasName"/> が <see langword="false"/> のインスタンス（<see langword="null"/> なし）</returns>
    public static DepartmentDisplayName Unset() => new(false);

    /// <summary>
    /// 指定された部署名を持つ <see cref="DepartmentDisplayName"/> の生成
    /// </summary>
    /// <param name="value">部署名（空文字は不可）</param>
    /// <returns>設定済み（<see cref="HasName"/> が <see langword="true"/>）のインスタンス</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> が <see langword="null"/> または空文字の場合</exception>
    public static DepartmentDisplayName From(string value) => new(value);

    /// <summary>
    /// 部署名からの <see cref="DepartmentDisplayName"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="input">部署名。<see langword="null"/> は「名前なし」（DB の値はそのまま渡せる）</param>
    /// <param name="result">成功した場合は生成したインスタンス（<paramref name="input"/> が <see langword="null"/> の場合は <see cref="Unset"/>）。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合、または <see langword="null"/> を <see cref="Unset"/> に変換した場合は <see langword="true"/>。空文字の場合は <see langword="false"/></returns>
    public static bool TryFrom(string? input, out DepartmentDisplayName result)
    {
        result = null!;

        if (input is null)
        {
            result = Unset(); // null は Unset に変換（名前なし）
            return true;
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
    public override bool Equals(object? obj) => Equals(obj as DepartmentDisplayName);

    /// <inheritdoc/>
    public bool Equals(DepartmentDisplayName? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IsSet == other.IsSet && Value == other.Value;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(IsSet, Value);

    /// <summary>
    /// 文字列表現の取得
    /// </summary>
    /// <returns>部署名。未設定の場合は空文字</returns>
    public override string ToString() => Value;

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException"><paramref name="normalized"/> が空文字の場合</exception>
    public override void Validate(string normalized)
    {
        base.Validate(normalized);

        if (string.IsNullOrEmpty(normalized))
        {
            throw new ArgumentException("DepartmentDisplayName must not be null or empty.", nameof(normalized));
        }
    }
}
