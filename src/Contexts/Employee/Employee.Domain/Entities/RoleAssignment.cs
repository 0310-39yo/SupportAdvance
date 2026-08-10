namespace SupportAdvance.Contexts.Employee.Domain.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ロール割り当てエンティティ（ロール有効期間管理）
///
/// 【集約根ID】RoleAssignmentId（GUID ベース）、Entity&lt;TId&gt;.Id で公開
/// 【公開プロパティ】RoleCode（ロール）、EffectiveDate（開始日）、ExpirationDate（終了日）
/// 【責務】従業員のロール割り当てと有効期間を管理、有効期限チェック
/// </summary>
public sealed class RoleAssignment : Entity<RoleAssignmentId>
{
    /// <summary>
    /// ロールコードを取得する
    /// </summary>
    public RoleCode RoleCode { get; private set; }

    /// <summary>
    /// 有効開始日時を取得する
    /// </summary>
    public LocalDateTime EffectiveDate { get; private set; }

    /// <summary>
    /// 有効終了日時を取得する（null=無期限）
    /// </summary>
    public LocalDateTime? ExpirationDate { get; private set; }

    /// <summary>
    /// 指定されたプロパティから RoleAssignment を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="id">ロール割り当てID</param>
    /// <param name="roleCode">ロールコード</param>
    /// <param name="effectiveDate">有効開始日時</param>
    /// <param name="expirationDate">有効終了日時（null許可）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private RoleAssignment(
        RoleAssignmentId id,
        RoleCode roleCode,
        LocalDateTime effectiveDate,
        LocalDateTime? expirationDate = null)
    {
        Id = id;
        RoleCode = roleCode;
        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
    }

    /// <summary>
    /// 新しい RoleAssignment を生成する（ファクトリメソッド）
    /// </summary>
    /// <param name="roleCode">ロールコード</param>
    /// <param name="effectiveDate">有効開始日時</param>
    /// <param name="expirationDate">有効終了日時（null許可）</param>
    /// <returns>生成された RoleAssignment インスタンス</returns>
    /// <remarks>
    /// パラメータはすべて検証済みの ValueObject として渡される。
    /// RoleAssignment レベルでの追加検証は不要。
    /// </remarks>
    public static RoleAssignment Create(
        RoleCode roleCode,
        LocalDateTime effectiveDate,
        LocalDateTime? expirationDate = null)
    {
        return new(
            RoleAssignmentId.NewId(),
            roleCode,
            effectiveDate,
            expirationDate);
    }

    /// <summary>
    /// DB から読み込んだ値から RoleAssignment を復元する（ファクトリメソッド）
    /// </summary>
    /// <param name="id">ロール割り当てID</param>
    /// <param name="roleCode">ロールコード</param>
    /// <param name="effectiveDate">有効開始日時</param>
    /// <param name="expirationDate">有効終了日時（null許可）</param>
    /// <returns>復元された RoleAssignment インスタンス</returns>
    /// <remarks>
    /// DB 値は既に検証済みと仮定。検証なしで復元。
    /// </remarks>
    public static RoleAssignment Reconstruct(
        RoleAssignmentId id,
        RoleCode roleCode,
        LocalDateTime effectiveDate,
        LocalDateTime? expirationDate = null)
    {
        return new(id, roleCode, effectiveDate, expirationDate);
    }

    /// <summary>
    /// このロール割り当てが指定時点で有効かどうかを判定する
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
    /// RoleAssignment の文字列表現を取得する
    /// </summary>
    /// <returns>ロール割り当ての説明文字列</returns>
    public override string ToString()
        => $"RoleAssignment(Id={Id.Value}, Code={RoleCode}, Effective={EffectiveDate}, Expiration={ExpirationDate})";
}
