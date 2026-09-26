using SupportAdvance.Application.Abstractions.Services;
using SupportAdvance.Common;

namespace SupportAdvance.Infrastructure.Services;

/// <summary>
/// ICurrentUserService の暫定実装（認証機能未実装のための仮実装 / スタブ）
/// </summary>
/// <remarks>
/// <para>【TODO】Authentication BC（認証機能）が実装され次第、ログインユーザー情報を返す。正式な実装（例：HttpContext / Windows認証ベース）に置き換えること</para>
/// <para>【現状の挙動】常に固定のシステムユーザー（m_employees.row_id=2147483667）を返す。実際のログインユーザーは判別できないため、監査カラム（created_by 等）は、すべてシステムユーザーとしての記録</para>
/// <para>【FK違反リスク解消】m_persons, m_employees に System User レコード（row_id=2147483659/2147483667）が既に用意されているため、created_by の外部キー制約違反の発生なし</para>
/// </remarks>
public sealed class SystemCurrentUserService : ICurrentUserService
{
    /// <summary>
    /// システム処理用の予約 EmployeeRowId
    /// </summary>
    /// <remarks>
    /// <para>m_persons.row_id=2147483659, m_employees.row_id=2147483667 に対応する System User</para>
    /// <para>【重要】値の実体は WellKnownIds（Common）で一元管理。Infrastructure/Application 両方から同じ値を参照するため、ここでは再定義せず委譲</para>
    /// </remarks>
    public const long SystemUserEmployeeRowId = WellKnownIds.SystemUserEmployeeRowId;

    /// <summary>
    /// 現在のユーザーの従業員rowId
    /// </summary>
    /// <value>常に <see cref="SystemUserEmployeeRowId"/>（ログイン状態に関係なく固定）</value>
    public long EmployeeRowId => SystemUserEmployeeRowId;

    /// <summary>
    /// 認証機能が未実装のため常に false（未認証扱い）
    /// </summary>
    public bool IsAuthenticated => false;

    /// <summary>
    /// SetLoggedInUser は暫定実装のため何もしない（no-op）
    /// Authentication BC 実装時は RealCurrentUserService に置き換え
    /// </summary>
    /// <param name="employeeRowId">ログインした従業員の行ID（未使用）</param>
    /// <param name="loginId">ログインID（未使用）</param>
    public void SetLoggedInUser(long employeeRowId, string loginId)
    {
        // 暫定実装のため何もしない
    }

    /// <summary>
    /// SetLoggedOut は暫定実装のため何もしない（no-op）
    /// Authentication BC 実装時は RealCurrentUserService に置き換え
    /// </summary>
    public void SetLoggedOut()
    {
        // 暫定実装のため何もしない
    }
}
