namespace SupportAdvance.Common;

/// <summary>
/// 全層・全BCで共有する予約済みマスタデータの識別子
///
/// 【責務】
/// - システム処理用に DB へ事前投入された予約レコードの row_id を一元管理
/// - Infrastructure（例: SystemCurrentUserService）と Context別 Application
///   （例: Authentication BC の失敗ログイン記録）の両方から同じ値を参照させ、
///   値のハードコード重複によるずれを防ぐ
/// </summary>
public static class WellKnownIds
{
    /// <summary>
    /// システム処理用の予約 EmployeeRowId（m_employees.row_id）
    /// 対応する m_persons.row_id は 2147483659
    /// </summary>
    public const long SystemUserEmployeeRowId = 2147483667;
}
