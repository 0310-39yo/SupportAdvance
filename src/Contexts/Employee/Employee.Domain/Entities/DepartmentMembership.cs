namespace SupportAdvance.Contexts.Employee.Domain.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Identifiers;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 部署メンバーシップエンティティ（部署への属性関連付け）
///
/// 【集約根ID】DepartmentMembershipId（GUID ベース）、Entity&lt;TId&gt;.Id で公開
/// 【公開プロパティ】DepartmentCode（部署コード）、IsPrimary（主部署フラグ）、ExpirationDate（有効期限）
/// 【責務】従業員の部署所属管理、有効期限チェック
/// </summary>
public sealed class DepartmentMembership : Entity<DepartmentMembershipId>
{
    /// <summary>
    /// 部署コードを取得する
    /// </summary>
    public DepartmentCode DepartmentCode { get; private set; }

    /// <summary>
    /// 主部署フラグを取得する（true の場合、この部署が従業員の主所属）
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// メンバーシップの有効期限を取得する（null の場合、無期限）
    /// </summary>
    public LocalDateTime? ExpirationDate { get; private set; }

    /// <summary>
    /// 指定されたプロパティから DepartmentMembership を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="id">メンバーシップ識別子</param>
    /// <param name="departmentCode">部署コード</param>
    /// <param name="isPrimary">主部署フラグ</param>
    /// <param name="expirationDate">有効期限（null許可）</param>
    private DepartmentMembership(
        DepartmentMembershipId id,
        DepartmentCode departmentCode,
        bool isPrimary,
        LocalDateTime? expirationDate = null)
    {
        Id = id;
        DepartmentCode = departmentCode;
        IsPrimary = isPrimary;
        ExpirationDate = expirationDate;
    }

    /// <summary>
    /// 新しい DepartmentMembership を生成する（ファクトリメソッド）
    /// </summary>
    /// <param name="departmentCode">部署コード</param>
    /// <param name="isPrimary">主部署フラグ</param>
    /// <param name="startDate">開始日時（記録用、プロパティには保持されない）</param>
    /// <param name="expirationDate">有効期限（null許可）</param>
    /// <returns>生成された DepartmentMembership インスタンス</returns>
    public static DepartmentMembership Create(
        DepartmentCode departmentCode,
        bool isPrimary,
        LocalDateTime startDate,
        LocalDateTime? expirationDate = null)
    {
        return new(
            DepartmentMembershipId.NewId(),
            departmentCode,
            isPrimary,
            expirationDate);
    }

    /// <summary>
    /// DB から読み込んだ値から DepartmentMembership を復元する（ファクトリメソッド）
    /// </summary>
    /// <param name="id">メンバーシップ識別子</param>
    /// <param name="departmentCode">部署コード</param>
    /// <param name="isPrimary">主部署フラグ</param>
    /// <param name="startDate">開始日時（記録用）</param>
    /// <param name="expirationDate">有効期限（null許可）</param>
    /// <returns>復元された DepartmentMembership インスタンス</returns>
    public static DepartmentMembership Reconstruct(
        DepartmentMembershipId id,
        DepartmentCode departmentCode,
        bool isPrimary,
        LocalDateTime startDate,
        LocalDateTime? expirationDate = null)
    {
        return new(id, departmentCode, isPrimary, expirationDate);
    }

    /// <summary>
    /// このメンバーシップが指定時点で有効かどうかを判定する
    /// </summary>
    /// <param name="asOf">判定時点</param>
    /// <returns>有効期限がない、または asOf が有効期限より前の場合 true</returns>
    public bool IsActive(LocalDateTime asOf)
    {
        // 有効期限がない場合は常に有効
        if (ExpirationDate == null)
        {
            return true;
        }

        // asOf が有効期限より前なら有効、以後なら無効
        return asOf < ExpirationDate;
    }

    /// <summary>
    /// DepartmentMembership の文字列表現を取得する
    /// </summary>
    /// <returns>メンバーシップの説明文字列</returns>
    public override string ToString()
        => $"DepartmentMembership(Id={Id.Value}, Code={DepartmentCode}, Primary={IsPrimary})";
}
