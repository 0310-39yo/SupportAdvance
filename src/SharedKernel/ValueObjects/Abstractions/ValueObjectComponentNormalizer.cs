namespace SupportAdvance.SharedKernel.ValueObjects;

/// <summary>
/// <see cref="ValueObject" /> の等価性コンポーネントを正規化するユーティリティ
///
/// ValueObject.GetEqualityComponents() が IsSet を先頭に yield return した後、
/// 値コンポーネント（GetValueComponents() の結果）に対する、単純な IsSet の先頭への付加
/// </summary>
internal static class ValueObjectComponentNormalizer
{
    /// <summary>
    /// ValueObject の等価性コンポーネントの正規化
    /// </summary>
    /// <param name="instance">正規化対象の ValueObject</param>
    /// <param name="components">値コンポーネント（IsSet を除く）の列挙（null 可）</param>
    /// <returns>IsSet を先頭に含む完全なコンポーネント列挙</returns>
    /// <remarks>
    /// <para>【処理】</para>
    /// <list type="bullet">
    /// <item><description>IsSet を先頭に yield return</description></item>
    /// <item><description>components（値コンポーネント）の要素をそのまま yield return</description></item>
    /// </list>
    /// </remarks>
    internal static IEnumerable<object?> Normalize(ValueObject instance, IEnumerable<object?>? components)
    {
        yield return instance.IsSet;

        if (components is not null)
        {
            foreach (var component in components)
            {
                yield return component;
            }
        }
    }
}
