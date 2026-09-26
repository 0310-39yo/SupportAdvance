using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Domain.Entities;

/// <summary>
/// 従業員の部署所属を表す Entity（部署メンバーシップ）
/// </summary>
/// <remarks>
/// <para>【ID型】<see cref="DepartmentMembershipRowId"/>（独立した Entity の ID）。親参照は <see cref="EmployeeRowId"/>（所属する従業員）</para>
/// <para>【責務】配属期間の管理</para>
/// <para>【コレクション構造】従業員が複数の部署に所属可能</para>
/// <para>【独立性】<see cref="EndOn"/>（配属終了日）は Employee の <c>RetiredOn</c>（雇用終了）と独立。配置転換の場合は前の部署の <see cref="EndOn"/> のみ更新し、<c>RetiredOn</c> は変わらない。退職の場合は <c>RetiredOn</c> を設定し、各部署の <see cref="EndOn"/> は別途管理</para>
/// <para>【null契約】名前などの未設定の項目は <see langword="null"/> ではなく、各値オブジェクトの Unset で表現</para>
/// </remarks>
public sealed class DepartmentMembership : Entity<DepartmentMembershipRowId>
{
    /// <summary>
    /// 所属する従業員の行ID
    /// </summary>
    public EmployeeRowId EmployeeRowId { get; private set; }

    /// <summary>
    /// 所属先の部署の行ID
    /// </summary>
    public DepartmentRowId DepartmentRowId { get; private set; }

    /// <summary>
    /// 所属先の部署名（表示用）
    /// </summary>
    /// <value>部署が削除済みなどで名前を取得できない場合は <see cref="DepartmentDisplayName.Unset"/>（<see langword="null"/> なし）</value>
    public DepartmentDisplayName DepartmentDisplayName { get; private set; }

    /// <summary>
    /// 主部署かどうかを示す値（主所属の場合は Primary）
    /// </summary>
    public IsPrimary IsPrimary { get; private set; }

    /// <summary>
    /// 異動終了日時（JST）
    /// </summary>
    /// <value>無期限・継続中の場合は <see cref="EndOn.Unset"/></value>
    public EndOn EndOn { get; private set; }

    private DepartmentMembership(
        DepartmentMembershipRowId membershipRowId,
        EmployeeRowId employeeRowId,
        DepartmentRowId departmentRowId,
        DepartmentDisplayName departmentDisplayName,
        IsPrimary isPrimary,
        EndOn endOn)
    {
        RowId = membershipRowId;
        EmployeeRowId = employeeRowId;
        DepartmentRowId = departmentRowId;
        DepartmentDisplayName = departmentDisplayName;
        IsPrimary = isPrimary;
        EndOn = endOn;
    }

    /// <summary>
    /// 新しい <see cref="DepartmentMembership"/> の生成
    /// </summary>
    /// <param name="membershipRowId">採番済みの部署メンバーシップの行ID</param>
    /// <param name="employeeRowId">所属する従業員の行ID</param>
    /// <param name="departmentRowId">所属先の部署の行ID</param>
    /// <param name="isPrimary">主所属かどうか</param>
    /// <param name="endOn">所属の終了日。<see langword="null"/> の場合は無期限（<see cref="EndOn.Unset"/>）</param>
    /// <param name="departmentDisplayName">表示用の部署名。<see langword="null"/> の場合は名前なし（<see cref="DepartmentDisplayName.Unset"/>）</param>
    /// <returns>生成した部署メンバーシップ</returns>
    public static DepartmentMembership Create(
        DepartmentMembershipRowId membershipRowId,
        EmployeeRowId employeeRowId,
        DepartmentRowId departmentRowId,
        IsPrimary isPrimary,
        EndOn? endOn = null,
        DepartmentDisplayName? departmentDisplayName = null) =>
        new(
            membershipRowId,
            employeeRowId,
            departmentRowId,
            departmentDisplayName ?? DepartmentDisplayName.Unset(),
            isPrimary,
            endOn ?? EndOn.Unset());

    /// <summary>
    /// DB から読み込んだ値による <see cref="DepartmentMembership"/> の復元
    /// </summary>
    /// <param name="membershipRowId">部署メンバーシップの行ID</param>
    /// <param name="employeeRowId">所属する従業員の行ID</param>
    /// <param name="departmentRowId">所属先の部署の行ID</param>
    /// <param name="isPrimary">主所属かどうか</param>
    /// <param name="endOn">所属の終了日（無期限の場合は <see cref="EndOn.Unset"/>）</param>
    /// <param name="departmentDisplayName">表示用の部署名（DB で結合して取得した値）。<see langword="null"/> の場合は名前なし（<see cref="DepartmentDisplayName.Unset"/>）</param>
    /// <returns>復元した部署メンバーシップ</returns>
    public static DepartmentMembership Reconstruct(
        DepartmentMembershipRowId membershipRowId,
        EmployeeRowId employeeRowId,
        DepartmentRowId departmentRowId,
        IsPrimary isPrimary,
        EndOn endOn,
        DepartmentDisplayName? departmentDisplayName = null) =>
        new(
            membershipRowId,
            employeeRowId,
            departmentRowId,
            departmentDisplayName ?? DepartmentDisplayName.Unset(),
            isPrimary,
            endOn);

    /// <summary>
    /// 指定時点で有効かどうかの判定
    /// </summary>
    /// <param name="asOf">判定する日時（JST）</param>
    /// <returns>終了日が未設定の場合、または <paramref name="asOf"/> が終了日より前の場合は <see langword="true"/></returns>
    public bool IsActive(LocalDateTime asOf)
    {
        // 終了日が未設定（無期限）なら常に有効
        if (!EndOn.HasEnded)
        {
            return true;
        }

        // asOf が終了日より前なら有効、以後なら無効
        return asOf < EndOn.Value;
    }

    /// <summary>
    /// デバッグ用の文字列表現の取得
    /// </summary>
    /// <returns><c>DepartmentMembership(RowId=…, DeptRowId=…, Primary=…)</c> 形式の文字列。画面表示やデータの解析には使用禁止</returns>
    public override string ToString()
        => $"DepartmentMembership(RowId={RowId.Value}, DeptRowId={DepartmentRowId}, Primary={IsPrimary})";
}
