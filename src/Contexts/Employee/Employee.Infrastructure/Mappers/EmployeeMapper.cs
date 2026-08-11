namespace SupportAdvance.Contexts.Employee.Infrastructure.Mappers;

using Domain.Entities;
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
        // 部署区分の文字列から Enum に変換
        var division = dbModel.EmployeeCodeDivision switch
        {
            "M" => EmployeeDivision.RegularEmployee(),
            "T" => EmployeeDivision.Dispatched(),
            "C" => EmployeeDivision.Contractor(),
            _ => throw new InvalidOperationException(
                $"Invalid employee division: {dbModel.EmployeeCodeDivision}")
        };

        var number = EmployeeNumber.From(dbModel.EmployeeCodeNumber);
        var code = EmployeeCode.From(division, number);

        return Employee.Reconstruct(
            EmployeeId.From(dbModel.EmployeeId),
            EmployeeRowId.From(dbModel.RowId),
            code,
            PersonRowId.From(dbModel.PersonRowId)
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
    public EmployeeDbModel ToDbModel(Employee entity)
    {
        var divisionCode = ConvertDivisionToCode(entity.Code.Division);

        return new EmployeeDbModel
        {
            EmployeeId = entity.Id.Value,
            RowId = entity.RowId.Value,
            EmployeeCodeDivision = divisionCode,
            EmployeeCodeNumber = entity.Code.Number.Value,
            PersonRowId = entity.PersonRowId.Value
            // 監査カラムは Repository で設定
        };
    }

    private string ConvertDivisionToCode(EmployeeDivision division)
    {
        if (division.IsRegularEmployee)
        {
            return "M";
        }

        if (division.IsDispatched)
        {
            return "T";
        }

        if (division.IsContractor)
        {
            return "C";
        }

        throw new InvalidOperationException($"Invalid division: {division}");
    }
}
