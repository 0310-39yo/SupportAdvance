namespace SupportAdvance.Application.Abstractions.Services;

/// <summary>
/// 現在のユーザー（従業員）情報を提供・管理
///
/// 【責務】
/// - 認証コンテキストから現在のユーザー情報を取得
/// - ログイン・ログアウト時にユーザー情報を管理
///
/// 【用途】
/// - 監査カラム（createdBy, updatedBy, deletedBy）に従業員rowIdを設定
/// - 画面表示時に現在ユーザー情報を参照
///
/// 【実装】
/// - WinForms／WPF: Presentation の RealCurrentUserService（ログイン時に情報を保持）
/// - システム処理: Infrastructure の SystemCurrentUserService（システムユーザー固定）
/// - AspNet: HttpContext から取得（認証ミドルウェア連携）
///
/// 【配置】
/// - Presentation と Infrastructure の双方が参照するため、依存方向を守れる Application 汎用層に定義
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// 現在のユーザーの従業員rowId
    /// </summary>
    long EmployeeRowId { get; }

    /// <summary>
    /// 現在のユーザーが認証されているか
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// ログイン時にユーザー情報を記録
    /// </summary>
    /// <param name="employeeRowId">従業員rowId</param>
    /// <param name="loginId">ログインID</param>
    void SetLoggedInUser(long employeeRowId, string loginId);

    /// <summary>
    /// ログアウト時にユーザー情報をクリア
    /// </summary>
    void SetLoggedOut();
}
