namespace SupportAdvance.Contexts.Department.Infrastructure.Mappers;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.Contexts.Department.Infrastructure.DbModels;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 部署ドメインモデル ↔ DepartmentDbModel のマッピング
///
/// 【責務】ValueObject ↔ プリミティブ型の双方向変換
/// 【層の責務】
///   - Domain: ValueObject（型安全性）
///   - DbModel: プリミティブ型（ORM マッピング）
///   - Mapper: 変換ロジック（層の橋渡し）
/// 【重要】監査フィールド（UpdatedAt/UpdatedBy）は Repository で管理（Mapper では設定しない）
/// 【テスト容易性】Clock 依存なし（純粋な型変換）
/// </summary>
public class DepartmentMapper
{
    /// <summary>
    /// DbModel から Domain Entity に変換（読み込み用）
    /// 【責務】DB の プリミティブ型 → Domain の ValueObject に変換
    /// </summary>
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

        if (!AbolishedOn.TryFromDbValue(dbModel.AbolishedOn, out var abolishedOn))
        {
            throw new InvalidOperationException($"Failed to convert AbolishedOn from DB value: {dbModel.AbolishedOn}");
        }

        // Domain Entity の復元
        return Department.Reconstruct(
            deptRowId,
            deptCode,
            dbModel.DepartmentName,
            level,
            parentId,
            managerId,
            abolishedOn,
            dbModel.RowVersion);
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Insert/Update 用）
    /// 【責務】Domain の ValueObject → DB の プリミティブ型に変換
    /// 【注意】監査フィールド（UpdatedAt/UpdatedBy）は Repository で設定
    /// </summary>
    public DepartmentDbModel ToDbModel(Department entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new()
        {
            RowId = entity.RowId.Value,
            DepartmentCode = entity.DeptCode.Value,
            DepartmentName = entity.Name,
            HierarchyLevel = entity.Level.Value,
            ParentDepartmentRowId = entity.ParentId.IsSet ? entity.ParentId.Value : null,
            ManagerEmployeeRowId = entity.ManagerId.IsSet ? entity.ManagerId.Value : null,
            AbolishedOn = entity.AbolishedOn.IsAbolished ? entity.AbolishedOn.Value.Value : null
            // 監査フィールドは設定しない（Repository が責務を持つ）
        };
    }
}
