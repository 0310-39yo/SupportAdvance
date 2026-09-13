namespace SupportAdvance.Infrastructure.Services;

/// <summary>
/// ICurrentUserService の暫定実装（認証機能未実装のための仮実装 / スタブ）
///
/// 【TODO】Identity BC（認証機能）が実装され次第、ログインユーザー情報を返す
/// 正式な実装（例：HttpContext / Windows認証ベース）に置き換えること
///
/// 【現状の挙動】常に固定のシステムユーザー（m_employees.row_id=2147483667）を返す。
/// 実際のログインユーザーは判別できないため、監査カラム（created_by 等）は
/// すべてシステムユーザーとして記録される。
///
/// 【FK違反リスク解消】m_persons, m_employees に System User レコード（row_id=2147483659/2147483667）
/// が既に用意されているため、created_by の外部キー制約違反は発生しない。
/// </summary>
public sealed class SystemCurrentUserService : ICurrentUserService
{
    /// <summary>
    /// システム処理用の予約 EmployeeRowId
    /// m_persons.row_id=2147483659, m_employees.row_id=2147483667 に対応する System User
    /// </summary>
    public const long SystemUserEmployeeRowId = 2147483667;

    public long EmployeeRowId => SystemUserEmployeeRowId;

    /// <summary>
    /// 認証機能が未実装のため常に false（未認証扱い）
    /// </summary>
    public bool IsAuthenticated => false;
}
