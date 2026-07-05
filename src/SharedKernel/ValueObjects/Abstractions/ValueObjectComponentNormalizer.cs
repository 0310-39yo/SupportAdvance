namespace SupportAdvance.SharedKernel.ValueObjects;

/// <summary>
/// <see cref="ValueObject" />のコンポーネントを正規化するユーティリティ
/// <see cref="ValueObject.GetEqualityComponents()" />の戻り値に<see cref="ValueObject.IsSet" />を
/// 重複なく先頭に付加し、正規化された列挙を返す
/// </summary>
internal static class ValueObjectComponentNormalizer
{
    /// <summary>
    /// <see cref="ValueObject" />のコンポーネントを正規化する
    /// IsSetを先頭に含む正規済のコンポーネントの列挙を返す
    /// </summary>
    /// <param name="instance">正規化対象のValueObject</param>
    /// <param name="components">正規化対象のコンポーネント列挙(null可)</param>
    /// <returns>IsSetを先頭に付加した、正規化されたコンポーネント列挙</returns>
    internal static IEnumerable<object?> Normalize(ValueObject instance, IEnumerable<object?>? components)
    {
        // componentsがnullの場合、IsSetを返す
        if (components is null)
        {
            yield return instance.IsSet;
            yield break;
        }

        using var enumerator = components.GetEnumerator();

        // 空列挙の場合、IsSetを返す
        if (!enumerator.MoveNext())
        {
            yield return instance.IsSet;
            yield break;
        }

        var first = enumerator.Current;

        // 先頭要素がIsSetと一致するbool値の場合はそのまま使用(重複削除)
        if (first is bool b && b == instance.IsSet)
        {
            yield return first;
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current!;
            }

            yield break;
        }

        // それ以外はIsSetを先頭に追加し、残りの要素を返す
        yield return instance.IsSet;
        yield return first!;
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current!;
        }
    }
}
