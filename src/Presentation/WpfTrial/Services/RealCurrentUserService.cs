using SupportAdvance.Infrastructure.Services;
using EmployeeRowIdVo = SupportAdvance.SharedKernel.ValueObjects.Identifiers.EmployeeRowId;

namespace SupportAdvance.Presentation.WpfTrial.Services;

/// <summary>
/// 現在のユーザー情報サービス 実装
///
/// 【責務】
/// - ログイン中のユーザー情報を保持
/// - Application全体で現在のユーザーを参照できる
///
/// 【ライフサイクル】
/// - アプリケーション起動時にログイン
/// - ログイン成功後に情報を設定
/// - アプリケーション終了時にログアウト
///
/// 【スレッドセーフティ】
/// - UI スレッドでのみ実行される前提
/// - WPF の Dispatcher に依存
/// </summary>
public sealed class RealCurrentUserService : ICurrentUserService
{
    private long _employeeRowId;
    private string? _loginId;

    public long EmployeeRowId
    {
        get
        {
            if (_employeeRowId <= 0)
                throw new InvalidOperationException("User not logged in");
            return _employeeRowId;
        }
    }

    public string LoginId
    {
        get
        {
            if (string.IsNullOrEmpty(_loginId))
                throw new InvalidOperationException("User not logged in");
            return _loginId;
        }
    }

    public bool IsLoggedIn => _employeeRowId > 0 && !string.IsNullOrEmpty(_loginId);

    public bool IsAuthenticated => IsLoggedIn;

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

    public void SetLoggedOut()
    {
        _employeeRowId = 0;
        _loginId = null;
    }
}
