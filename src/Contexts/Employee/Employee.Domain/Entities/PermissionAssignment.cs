namespace SupportAdvance.Contexts.Employee.Domain.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;

/// <summary>
/// 権限割り当てエンティティ（権限有効期間管理）
///
/// 【集約根ID】PermissionAssignmentId（GUID ベース）、Entity&lt;TId&gt;.Id で公開
/// 【公開プロパティ】PermissionCode（権限）、EffectiveDate（開始日）、ExpirationDate（終了日）
/// 【責務】従業員の権限割り当てと有効期間を管理、有効期限チェック
/// </summary>
public sealed class PermissionAssignment : Entity<PermissionAssignmentId>
{
    /// <summary>
    /// 権限コードを取得する
    /// </summary>
    public PermissionCode PermissionCode { get; private set; }

    /// <summary>
    /// 有効開始日時を取得する
    /// </summary>
    public LocalDateTime EffectiveDate { get; private set; }

    /// <summary>
    /// 有効終了日時を取得する（null=無期限）
    /// </summary>
    public LocalDateTime? ExpirationDate { get; private set; }

    /// <summary>
    /// 指定されたプロパティから PermissionAssignment を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="id">権限割り当てID</param>
    /// <param name="permissionCode">権限コード</param>
    /// <param name="effectiveDate">有効開始日時</param>
    /// <param name="expirationDate">有効終了日時（null許可）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private PermissionAssignment(
        PermissionAssignmentId id,
        PermissionCode permissionCode,
        LocalDateTime effectiveDate,
        LocalDateTime? expirationDate = null)
    {
        Id = id;
        PermissionCode = permissionCode;
        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
    }

    /// <summary>
    /// 新しい PermissionAssignment を生成する（ファクトリメソッド）
    /// </summary>
    /// <param name="permissionCode">権限コード</param>
    /// <param name="effectiveDate">有効開始日時</param>
    /// <param name="expirationDate">有効終了日時（null許可）</param>
    /// <returns>生成された PermissionAssignment インスタンス</returns>
    /// <remarks>
    /// パラメータはすべて検証済みの ValueObject として渡される。
    /// PermissionAssignment レベルでの追加検証は不要。
    /// </remarks>
    public static PermissionAssignment Create(
        PermissionCode permissionCode,
        LocalDateTime effectiveDate,
        LocalDateTime? expirationDate = null)
    {
        return new(
            PermissionAssignmentId.NewId(),
            permissionCode,
            effectiveDate,
            expirationDate);
    }

    /// <summary>
    /// DB から読み込んだ値から PermissionAssignment を復元する（ファクトリメソッド）
    /// </summary>
    /// <param name="id">権限割り当てID</param>
    /// <param name="permissionCode">権限コード</param>
    /// <param name="effectiveDate">有効開始日時</param>
    /// <param name="expirationDate">有効終了日時（null許可）</param>
    /// <returns>復元された PermissionAssignment インスタンス</returns>
    /// <remarks>
    /// DB 値は既に検証済みと仮定。検証なしで復元。
    /// </remarks>
    public static PermissionAssignment Reconstruct(
        PermissionAssignmentId id,
        PermissionCode permissionCode,
        LocalDateTime effectiveDate,
        LocalDateTime? expirationDate = null)
    {
        return new(id, permissionCode, effectiveDate, expirationDate);
    }

    /// <summary>
    /// この権限割り当てが指定時点で有効かどうかを判定する
    /// </summary>
    /// <param name="asOf">判定時点</param>
    /// <returns>EffectiveDate 以後かつ ExpirationDate 前なら true</returns>
    public bool IsActive(LocalDateTime asOf)
    {
        // EffectiveDate より前なら無効
        if (asOf < EffectiveDate)
        {
            return false;
        }

        // ExpirationDate がない場合は常に有効
        if (ExpirationDate == null)
        {
            return true;
        }

        // asOf が ExpirationDate より前なら有効、以後なら無効
        return asOf < ExpirationDate;
    }

    /// <summary>
    /// PermissionAssignment の文字列表現を取得する
    /// </summary>
    /// <returns>権限割り当ての説明文字列</returns>
    public override string ToString()
        => $"PermissionAssignment(Id={Id.Value}, Code={PermissionCode}, Effective={EffectiveDate}, Expiration={ExpirationDate})";
}
