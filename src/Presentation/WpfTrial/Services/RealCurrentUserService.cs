using SupportAdvance.Application.Abstractions.Services;
using EmployeeRowIdVo = SupportAdvance.SharedKernel.ValueObjects.Identifiers.EmployeeRowId;

namespace SupportAdvance.Presentation.WpfTrial.Services;

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
/// <item><description>WPF の Dispatcher に依存</description></item>
/// </list>
/// </remarks>
public sealed class RealCurrentUserService : ICurrentUserService
{
    private long _employeeRowId;
    private string? _loginId;

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
    /// <paramref name="employeeRowId"/> が 1 未満の場合、または <paramref name="loginId"/> が空文字・空白のみの場合
    /// </exception>
    public void SetLoggedInUser(long employeeRowId, string loginId)
    {
        // EmployeeRowId ValueObject の検証ロジック（1以上）を再利用。
        // 格納自体は ICurrentUserService の契約（監査カラム用の long）に合わせて long のまま保持する。
        if (!EmployeeRowIdVo.TryFrom(employeeRowId, out _))
            throw new ArgumentException("EmployeeRowId must be positive", nameof(employeeRowId));

        if (string.IsNullOrWhiteSpace(loginId))
            throw new ArgumentException("LoginId cannot be empty", nameof(loginId));

        _employeeRowId = employeeRowId;
        _loginId = loginId;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>【副作用】以降の <see cref="EmployeeRowId"/>／<see cref="LoginId"/> の取得は例外</para>
    /// </remarks>
    public void SetLoggedOut()
    {
        _employeeRowId = 0;
        _loginId = null;
    }
}
