namespace SupportAdvance.Infrastructure.Services;

/// <summary>
/// 現在のユーザー（従業員）情報を提供
///
/// 【責務】認証コンテキストから現在のユーザー情報を取得
/// 【用途】監査カラム（createdBy, updatedBy, deletedBy）に従業員rowIdを設定
/// 【実装】認証ミドルウェア / HttpContext から取得
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
}
