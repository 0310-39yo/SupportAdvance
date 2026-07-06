namespace SupportAdvance.SharedKernel.ValueObjects;

/// <summary>
/// <see cref="ValueObject" /> の等価性コンポーネントを正規化するユーティリティ
///
/// ValueObject.GetEqualityComponents() が IsSet を先頭に yield return した後、
/// 値コンポーネント（GetValueComponents() の結果）に対して単純に IsSet を先頭に付加します。
/// </summary>
internal static class ValueObjectComponentNormalizer
{
    /// <summary>
    /// ValueObject の等価性コンポーネントを正規化します。
    ///
    /// 【処理】
    /// 1. IsSet を先頭に yield return
    /// 2. components（値コンポーネント）の要素をそのまま yield return
    /// </summary>
    /// <param name="instance">正規化対象の ValueObject</param>
    /// <param name="components">値コンポーネント（IsSet を除く）の列挙（null 可）</param>
    /// <returns>IsSet を先頭に含む完全なコンポーネント列挙</returns>
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
