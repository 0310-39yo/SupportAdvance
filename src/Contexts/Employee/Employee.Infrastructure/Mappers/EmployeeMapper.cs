namespace SupportAdvance.Contexts.Employee.Infrastructure.Mappers;

using Domain.Entities;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Models;

/// <summary>
/// Employee ドメインモデル ↔ EmployeeDbModel のマッピング
///
/// 【責務】ValueObject ↔ プリミティブ型の双方向変換
/// 【層の責務】
///   - Domain/Application: ValueObject（型安全性）
///   - DbModel: プリミティブ型（ORM マッピング）
///   - Mapper: 変換ロジック（層の橋渡し）
/// </summary>
public class EmployeeMapper
{
    /// <summary>
    /// DbModel から Domain Entity に変換（読み込み用）
    /// </summary>
    /// <param name="dbModel">データベースモデル</param>
    /// <returns>ドメイン Entity</returns>
    public Employee ToDomainEntity(EmployeeDbModel dbModel)
    {
        // 従業員種別区分の文字列から Enum に変換
        var typeDivision = dbModel.EmployeeCodeDivision switch
        {
            "M" => EmployeeTypeDivision.RegularEmployee(),
            "T" => EmployeeTypeDivision.Dispatched(),
            "C" => EmployeeTypeDivision.Contractor(),
            _ => throw new InvalidOperationException(
                $"Invalid employee division: {dbModel.EmployeeCodeDivision}")
        };

        var bizId = EmployeeBizId.From(dbModel.EmployeeCodeNumber);
        var bizCode = EmployeeBizCode.From(typeDivision, bizId);
        var personRowId = PersonRowId.From(dbModel.PersonRowId);

        RetiredOn? retiredOn = null;
        if (dbModel.RetiredOn.HasValue)
        {
            retiredOn = RetiredOn.From(new LocalDateTime(dbModel.RetiredOn.Value));
        }

        return Employee.Create(
            EmployeeRowId.From(dbModel.RowId),
            typeDivision,
            bizId,
            bizCode,
            personRowId,
            retiredOn
        );
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Insert/Update 用）
    /// </summary>
    /// <param name="entity">ドメイン Entity</param>
    /// <returns>DB 挿入/更新用モデル</returns>
    /// <remarks>
    /// 監査カラム（CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy）は
    /// Repository で設定するため、ここでは設定しない
    /// </remarks>
    public EmployeeDbModel ToDbModel(Employee entity)
    {
        var divisionCode = ConvertDivisionToCode(entity.TypeDivision);

        return new EmployeeDbModel
        {
            RowId = entity.RowId.Value,
            EmployeeCodeDivision = divisionCode,
            EmployeeCodeNumber = entity.BizId.Value,
            PersonRowId = entity.PersonRowId.Value,
            RetiredOn = entity.RetiredOn?.IsSet == true ? entity.RetiredOn.Value.Value : null
        };
    }

    private string ConvertDivisionToCode(EmployeeTypeDivision typeDivision)
    {
        if (typeDivision.IsRegularEmployee)
        {
            return "M";
        }

        if (typeDivision.IsDispatched)
        {
            return "T";
        }

        if (typeDivision.IsContractor)
        {
            return "C";
        }

        throw new InvalidOperationException($"Invalid division: {typeDivision}");
    }
}
