namespace SupportAdvance.Contexts.Department.Infrastructure.Mappers;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.Contexts.Department.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 部署（<see cref="Department"/>）と DB モデル（<see cref="DepartmentDbModel"/>）の相互変換を行うマッパー
/// </summary>
/// <remarks>
/// <para>【責務】値オブジェクト ↔ プリミティブ型の双方向変換。Domain は値オブジェクト、DB モデルはプリミティブ型で保持し、この型が橋渡しを行う</para>
/// <para>【重要】監査フィールド（UpdatedAt／UpdatedBy など）の設定なし。設定は Repository の担当</para>
/// <para>【テスト容易性】Clock 依存なし（純粋な型変換）</para>
/// </remarks>
public class DepartmentMapper
{
    /// <summary>
    /// DB モデルからの部署の復元（読み込み用）
    /// </summary>
    /// <param name="dbModel">DB から読み込んだ DB モデル</param>
    /// <returns>復元した部署</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dbModel"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidOperationException">DB の値を値オブジェクトに変換できない場合（DB の整合性エラー）</exception>
    public Department ToDomainEntity(DepartmentDbModel dbModel)
    {
        ArgumentNullException.ThrowIfNull(dbModel);

        // ValueObject の変換
        if (!DepartmentRowId.TryFromDbValue(dbModel.RowId, out var deptRowId))
        {
            throw new InvalidOperationException($"Failed to convert DepartmentRowId from DB value: {dbModel.RowId}");
        }

        if (!DepartmentCode.TryFromDbValue(dbModel.DepartmentCode, out var deptCode))
        {
            throw new InvalidOperationException($"Failed to convert DepartmentCode from DB value: {dbModel.DepartmentCode}");
        }

        if (!HierarchyLevel.TryFromDbValue(dbModel.HierarchyLevel, out var level))
        {
            throw new InvalidOperationException($"Failed to convert HierarchyLevel from DB value: {dbModel.HierarchyLevel}");
        }

        if (!ParentDepartmentRowId.TryFromDbValue(dbModel.ParentDepartmentRowId, out var parentId))
        {
            throw new InvalidOperationException($"Failed to convert ParentDepartmentRowId from DB value: {dbModel.ParentDepartmentRowId}");
        }

        if (!ManagerEmployeeRowId.TryFromDbValue(dbModel.ManagerEmployeeRowId, out var managerId))
        {
            throw new InvalidOperationException($"Failed to convert ManagerEmployeeRowId from DB value: {dbModel.ManagerEmployeeRowId}");
        }

        // DB の DateTime? は LocalDateTime? に変換してから TryFrom に渡す（null は Unset に変換）
        if (!AbolishedOn.TryFrom(dbModel.AbolishedOn.ToLocalDateTimeOrNull(), out var abolishedOn))
        {
            throw new InvalidOperationException($"Failed to convert AbolishedOn from DB value: {dbModel.AbolishedOn}");
        }

        if (!DepartmentName.TryFrom(dbModel.DepartmentName, out var deptName))
        {
            throw new InvalidOperationException($"Failed to convert DepartmentName from DB value: {dbModel.DepartmentName}");
        }

        // Domain Entity の復元
        return Department.Reconstruct(
            deptRowId,
            deptCode,
            deptName,
            level,
            parentId,
            managerId,
            abolishedOn,
            dbModel.RowVersion);
    }

    /// <summary>
    /// 部署の DB モデルへの変換（Insert／Update 用）
    /// </summary>
    /// <param name="entity">変換する部署</param>
    /// <returns>業務データのみ設定した DB モデル（監査列は未設定）</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> が <see langword="null"/> の場合</exception>
    public DepartmentDbModel ToDbModel(Department entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new()
        {
            RowId = entity.RowId.Value,
            DepartmentCode = entity.DeptCode.Value,
            DepartmentName = entity.Name.Value,
            HierarchyLevel = entity.Level.Value,
            ParentDepartmentRowId = entity.ParentId.IsSet ? entity.ParentId.Value : null,
            ManagerEmployeeRowId = entity.ManagerId.IsSet ? entity.ManagerId.Value : null,
            AbolishedOn = entity.AbolishedOn.IsAbolished ? entity.AbolishedOn.Value.Value : null
            // 監査フィールドは設定しない（Repository が責務を持つ）
        };
    }
}
