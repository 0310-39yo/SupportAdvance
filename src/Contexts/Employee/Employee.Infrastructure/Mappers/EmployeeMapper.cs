namespace SupportAdvance.Contexts.Employee.Infrastructure.Mappers;

using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Infrastructure.DataAccess.Models;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// Employee ドメインモデル ↔ EmployeeDbModel のマッピング
/// 【責務】ValueObject ↔ プリミティブ型の双方向変換
/// 【層の責務】
///   - Domain/Application: ValueObject（型安全性）
///   - DbModel: プリミティブ型（ORM マッピング）
///   - Mapper: 変換ロジック（層の橋渡し）
/// </summary>
public class EmployeeMapper
{
}

    /// <summary>
    /// DbModel から Domain Entity に変換（読み込み用）
    /// </summary>
    /// <param name="dbModel">データベースモデル</param>
    /// <returns>ドメイン Entity</returns>
    public Employee ToDomainEntity(EmployeeDbModel dbModel)
    {
        var employeeId = EmployeeId.From(dbModel.EmployeeId);  // 集約根ID
        var rowId = EmployeeRowId.From(dbModel.RowId);

        // EmployeeCode は EmployeeId のみから構成
        var code = EmployeeCode.From(employeeId);

        // 雇用終了日を変換（DateTime → LocalDateTime）
        var retiredAt = dbModel.RetiredAt.HasValue
            ? LocalDateTime.From(dbModel.RetiredAt.Value)
            : (LocalDateTime?)null;

        return Employee.Reconstruct(
            employeeId,
            rowId,
            code,
            retiredAt
        );
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Insert 用）
    /// </summary>
    /// <param name="entity">ドメイン Entity</param>
    /// <returns>DB 挿入用モデル</returns>
    /// <remarks>
    /// Insert 時には監査カラムを Repository で設定するため、ここでは設定しない
    /// </remarks>
    public EmployeeDbModel ToDbModelForInsert(Employee entity)
    {
        return new EmployeeDbModel
        {
            EmployeeId = entity.Id.Value,  // 集約根ID
            RetiredAt = entity.RetiredAt?.Value,
            // 監査カラムは Repository で設定
        };
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Update 用）
    /// </summary>
    /// <param name="entity">ドメイン Entity</param>
    /// <returns>DB 更新用モデル</returns>
    public EmployeeDbModel ToDbModelForUpdate(Employee entity)
    {
        return new EmployeeDbModel
        {
            RowId = entity.RowId.Value,
            EmployeeId = entity.Id.Value,  // 集約根ID
            RetiredAt = entity.RetiredAt?.Value,
            // 監査カラムは Repository で設定
        };
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Delete 用）
    /// </summary>
    /// <param name="entity">ドメイン Entity</param>
    /// <returns>DB 削除用モデル</returns>
    public EmployeeDbModel ToDbModelForDelete(Employee entity)
    {
        return new EmployeeDbModel
        {
            RowId = entity.RowId.Value,
            EmployeeId = entity.Id.Value,  // 集約根ID
            RetiredAt = entity.RetiredAt?.Value,
            // 削除フラグ（論理削除）は Repository で設定
        };
    }
}
