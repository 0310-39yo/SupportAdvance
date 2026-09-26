using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

/// <summary>
/// 認証セッションがローカル認証で使用した認証情報の行IDを表すオプションの値オブジェクト
/// </summary>
/// <remarks>
/// <para>【用途】<c>m_login_credentials.row_id</c> への参照（監査追跡用）。ローカル認証の場合のみ値を持ち、AD 認証の場合は未設定</para>
/// <para>【null契約】任意。AD 認証の場合は <see langword="null"/> ではなく <see cref="Unset"/>（<see cref="HasCredentials"/> が <see langword="false"/>）で表現。<see cref="TryFrom"/> は <see langword="null"/> の入力を <see cref="Unset"/> に変換して成功</para>
/// <para>【設計】必須型の <see cref="LoginCredentialsRowId"/>（認証情報そのものの識別子）とは別の型。この型は「セッションが使用した認証情報」を、未設定を含めて表す</para>
/// <para>【参照】docs/Assistance/Guides/null厳格性設計ガイド.md</para>
/// </remarks>
/// <seealso cref="LoginCredentialsRowId"/>
public sealed class UsedLoginCredentialsRowId : RowId, IEquatable<UsedLoginCredentialsRowId>
{
    /// <summary>
    /// 未設定状態による初期化。生成は <see cref="Unset"/> を使用
    /// </summary>
    /// <param name="isSet">設定済みかどうかを示す値</param>
    private UsedLoginCredentialsRowId(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 指定行IDによる初期化。生成は <see cref="From"/> を使用
    /// </summary>
    /// <param name="value">認証情報の行ID（1 以上）</param>
    private UsedLoginCredentialsRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 認証情報を使用していない状態（AD 認証など）を表す <see cref="UsedLoginCredentialsRowId"/> の生成
    /// </summary>
    /// <returns><see cref="HasCredentials"/> が <see langword="false"/> のインスタンス（<see langword="null"/> なし）</returns>
    public static UsedLoginCredentialsRowId Unset() => new(false);

    /// <summary>
    /// 指定された行IDを持つ <see cref="UsedLoginCredentialsRowId"/> の生成
    /// </summary>
    /// <param name="value">認証情報の行ID（1 以上）</param>
    /// <returns>設定済み（<see cref="HasCredentials"/> が <see langword="true"/>）のインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> が <see cref="LoginCredentialsRowId.MinValue"/> 未満の場合</exception>
    public static UsedLoginCredentialsRowId From(long value) => new(value);

    /// <summary>
    /// 指定された行IDを持つ <see cref="UsedLoginCredentialsRowId"/> の生成
    /// </summary>
    /// <param name="value">認証情報の行ID。必須型の <see cref="LoginCredentialsRowId"/> から変換</param>
    /// <returns>設定済みのインスタンス</returns>
    public static UsedLoginCredentialsRowId From(LoginCredentialsRowId value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new UsedLoginCredentialsRowId(value.Value);
    }

    /// <summary>
    /// 行IDからの <see cref="UsedLoginCredentialsRowId"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="input">認証情報の行ID。<see langword="null"/> は「使用なし」（DB の値はそのまま渡せる）</param>
    /// <param name="result">成功した場合は生成したインスタンス（<paramref name="input"/> が <see langword="null"/> の場合は <see cref="Unset"/>）。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合、または <see langword="null"/> を <see cref="Unset"/> に変換した場合は <see langword="true"/>。値が検証に通らなかった場合は <see langword="false"/></returns>
    public static bool TryFrom(long? input, out UsedLoginCredentialsRowId result)
    {
        result = null!;

        if (!input.HasValue)
        {
            result = Unset(); // null は Unset に変換（認証情報の使用なし）
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 認証情報を使用しているかどうかを示す値
    /// </summary>
    /// <value>ローカル認証で認証情報を使用した場合は <see langword="true"/>。<see cref="SupportAdvance.SharedKernel.ValueObjects.ValueObject.IsSet"/> の、業務上の意味に合わせた別名</value>
    public bool HasCredentials => IsSet;

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="normalized"/> が <see cref="LoginCredentialsRowId.MinValue"/> 未満の場合</exception>
    public override void Validate(long normalized)
    {
        if (normalized < LoginCredentialsRowId.MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"UsedLoginCredentialsRowId must be {LoginCredentialsRowId.MinValue} or higher.");
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as UsedLoginCredentialsRowId);

    /// <inheritdoc/>
    public bool Equals(UsedLoginCredentialsRowId? other)
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
}
