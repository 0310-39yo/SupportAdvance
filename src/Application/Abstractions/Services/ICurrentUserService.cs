using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Application.Abstractions.Services;

/// <summary>
/// 現在のユーザー（従業員）情報を提供・管理
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>認証コンテキストから現在のユーザー情報を取得</description></item>
/// <item><description>ログイン・ログアウト時にユーザー情報を管理</description></item>
/// </list>
/// <para>【用途】</para>
/// <list type="bullet">
/// <item><description>監査カラム（createdBy, updatedBy, deletedBy）に従業員rowIdを設定</description></item>
/// <item><description>画面表示時に現在ユーザー情報を参照</description></item>
/// </list>
/// <para>【実装】</para>
/// <list type="bullet">
/// <item><description>WinForms／WPF: Presentation の RealCurrentUserService（ログイン時に情報を保持）</description></item>
/// <item><description>システム処理: Infrastructure の SystemCurrentUserService（システムユーザー固定）</description></item>
/// <item><description>AspNet: HttpContext から取得（認証ミドルウェア連携）</description></item>
/// </list>
/// <para>【配置】</para>
/// <list type="bullet">
/// <item><description>Presentation と Infrastructure の双方が参照するため、依存方向を守れる Application 汎用層に定義</description></item>
/// </list>
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// 現在のユーザーの従業員rowId
    /// </summary>
    long EmployeeRowId { get; }

    /// <summary>
    /// 現在のセッションのrowId
    /// </summary>
    /// <exception cref="InvalidOperationException">ログインしていない場合</exception>
    long CurrentUserSessionRowId { get; }

    /// <summary>
    /// ログイン時刻（JST）
    /// </summary>
    /// <exception cref="InvalidOperationException">ログインしていない場合</exception>
    LocalDateTime LoggedInAt { get; }

    /// <summary>
    /// AD認証かどうか（true=AD認証、false=ローカル認証）
    /// </summary>
    /// <exception cref="InvalidOperationException">ログインしていない場合</exception>
    bool IsAdAuthenticated { get; }

    /// <summary>
    /// 現在のユーザーが認証されているか
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// ログイン時にユーザー情報を記録
    /// </summary>
    /// <param name="employeeRowId">従業員rowId</param>
    /// <param name="sessionRowId">セッションのrowId</param>
    /// <param name="loginId">ログインID</param>
    /// <param name="loggedInAt">ログイン時刻（JST）</param>
    /// <param name="isAdAuthenticated">AD認証の場合はtrue、ローカル認証の場合はfalse</param>
    void SetLoggedInUser(long employeeRowId, long sessionRowId, string loginId, LocalDateTime loggedInAt, bool isAdAuthenticated);

    /// <summary>
    /// ログアウト時にユーザー情報をクリア
    /// </summary>
    void SetLoggedOut();
}
