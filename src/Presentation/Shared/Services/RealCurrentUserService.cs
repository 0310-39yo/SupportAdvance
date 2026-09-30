using SupportAdvance.Application.Abstractions.Services;
using SupportAdvance.Common.Clocks;
using EmployeeRowIdVo = SupportAdvance.SharedKernel.ValueObjects.Identifiers.EmployeeRowId;

namespace SupportAdvance.Presentation.Shared.Services;

/// <summary>
/// 現在のユーザー情報サービス 実装
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>ログイン中のユーザー情報を保持</description></item>
/// <item><description>Application全体での現在のユーザーの参照が可能</description></item>
/// </list>
/// <para>【ライフサイクル】</para>
/// <list type="bullet">
/// <item><description>アプリケーション起動時にログイン</description></item>
/// <item><description>ログイン成功後に情報を設定</description></item>
/// <item><description>アプリケーション終了時にログアウト</description></item>
/// </list>
/// <para>【スレッドセーフティ】</para>
/// <list type="bullet">
/// <item><description>UI スレッドでのみ実行される前提</description></item>
/// <item><description>UI フレームワーク（WinForms / WPF）に依存しない</description></item>
/// </list>
/// </remarks>
public sealed class RealCurrentUserService : ICurrentUserService
{
    private long _employeeRowId;
    private long _sessionRowId;
    private string? _loginId;
    private LocalDateTime _loggedInAt;
    private bool _isAdAuthenticated;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">ログインしていない場合（<see cref="SetLoggedInUser"/> の呼び出し前、または <see cref="SetLoggedOut"/> の後）</exception>
    /// <remarks>
    /// <para>【注意】ログイン前に監査列を設定する処理（リポジトリの保存など）を行うと例外</para>
    /// </remarks>
    public long EmployeeRowId
    {
        get
        {
            if (_employeeRowId <= 0)
                throw new InvalidOperationException("User not logged in");
            return _employeeRowId;
        }
    }

    /// <inheritdoc/>
    public long CurrentUserSessionRowId
    {
        get
        {
            if (_sessionRowId <= 0)
                throw new InvalidOperationException("User not logged in");
            return _sessionRowId;
        }
    }

    /// <inheritdoc/>
    public LocalDateTime LoggedInAt
    {
        get
        {
            if (_loggedInAt == default)
                throw new InvalidOperationException("User not logged in");
            return _loggedInAt;
        }
    }

    /// <inheritdoc/>
    public bool IsAdAuthenticated
    {
        get
        {
            if (_employeeRowId <= 0)
                throw new InvalidOperationException("User not logged in");
            return _isAdAuthenticated;
        }
    }

    /// <summary>
    /// ログイン中のユーザーのログインID
    /// </summary>
    /// <exception cref="InvalidOperationException">ログインしていない場合</exception>
    public string LoginId
    {
        get
        {
            if (string.IsNullOrEmpty(_loginId))
                throw new InvalidOperationException("User not logged in");
            return _loginId;
        }
    }

    /// <summary>
    /// ログイン中かどうかを示す値
    /// </summary>
    /// <value>従業員rowId とログインID の両方が設定済みの場合は <see langword="true"/></value>
    public bool IsLoggedIn => _employeeRowId > 0 && !string.IsNullOrEmpty(_loginId);

    /// <inheritdoc/>
    /// <value><see cref="IsLoggedIn"/> と同じ値</value>
    public bool IsAuthenticated => IsLoggedIn;

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="employeeRowId"/> が 1 未満の場合、<paramref name="sessionRowId"/> が 1 未満の場合、または <paramref name="loginId"/> が空文字・空白のみの場合
    /// </exception>
    public void SetLoggedInUser(long employeeRowId, long sessionRowId, string loginId, LocalDateTime loggedInAt, bool isAdAuthenticated)
    {
        // EmployeeRowId ValueObject の検証ロジック（1以上）を再利用。
        // 格納自体は ICurrentUserService の契約（監査カラム用の long）に合わせて long のまま保持する。
        if (!EmployeeRowIdVo.TryFrom(employeeRowId, out _))
            throw new ArgumentException("EmployeeRowId must be positive", nameof(employeeRowId));

        if (sessionRowId <= 0)
            throw new ArgumentException("SessionRowId must be positive", nameof(sessionRowId));

        if (string.IsNullOrWhiteSpace(loginId))
            throw new ArgumentException("LoginId cannot be empty", nameof(loginId));

        if (loggedInAt == default)
            throw new ArgumentException("LoggedInAt must not be default", nameof(loggedInAt));

        _employeeRowId = employeeRowId;
        _sessionRowId = sessionRowId;
        _loginId = loginId;
        _loggedInAt = loggedInAt;
        _isAdAuthenticated = isAdAuthenticated;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>【副作用】以降の <see cref="EmployeeRowId"/>／<see cref="LoginId"/> 等の取得は例外</para>
    /// </remarks>
    public void SetLoggedOut()
    {
        _employeeRowId = 0;
        _sessionRowId = 0;
        _loginId = null;
        _loggedInAt = default;
        _isAdAuthenticated = false;
    }
}
