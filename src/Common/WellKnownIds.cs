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
    /// システム処理用の予約 PersonRowId（m_persons.row_id）
    /// 【意味】自動処理・バックグラウンド処理など、特定の人間が行ったのではない操作の作成者/更新者/削除者として使う
    /// </summary>
    public const long SystemUserPersonRowId = 2147483659;

    /// <summary>
    /// システム処理用の予約 EmployeeRowId（m_employees.row_id）
    /// 対応する m_persons.row_id は SystemUserPersonRowId（2147483659）
    /// 【意味】SystemUserPersonRowId と同じく「人間ではない自動処理」を表す。
    /// 「正体不明の人物によるログイン試行」には使わないこと（→ UnknownUserEmployeeRowId を使う）
    /// </summary>
    public const long SystemUserEmployeeRowId = 2147483667;

    /// <summary>
    /// employeeマスタ上で本人を特定できない人物（例: 存在しないログインIDでのログイン試行）を表す
    /// 予約 PersonRowId（m_persons.row_id）
    /// 【意味】SystemUser（自動処理）とは異なり、実際に（未登録の）人間がログインを試みた記録であることを表す
    /// 【値の根拠】s_row_id_sequence は既に int.MaxValue（2147483647）を超えて稼働中（2026-09-18 確認時点で
    /// current_value=2147483851）。int.MaxValue 超〜SystemUserPersonRowId（2147483659）未満の間は
    /// シーケンスが既に通過済み（is_cycling=0 のため再利用されない）かつ他の予約値と重複しない安全な空番
    /// </summary>
    public const long UnknownUserPersonRowId = 2147483648;

    /// <summary>
    /// employeeマスタ上で本人を特定できない人物（例: 存在しないログインIDでのログイン試行）を表す
    /// 予約 EmployeeRowId（m_employees.row_id）
    /// 対応する m_persons.row_id は UnknownUserPersonRowId（2147483648）
    /// 【用途】AuthenticateLocalUserUseCase で login_id 自体が見つからない失敗ログの AuthorityRowId に使用
    /// 【重要】SystemUserEmployeeRowId とは意味が異なる。こちらは「正体不明の人物」、
    /// SystemUserEmployeeRowId は「システム自身による自動処理」を表す
    /// 【値の根拠】UnknownUserPersonRowId 同様、シーケンスが既に通過済みの安全な空番を使用
    /// </summary>
    public const long UnknownUserEmployeeRowId = 2147483649;
}
