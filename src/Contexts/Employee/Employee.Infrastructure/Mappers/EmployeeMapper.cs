namespace SupportAdvance.Contexts.Employee.Infrastructure.Mappers;

using Domain.Entities;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.ValueObjects;
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
    private readonly IClock _clock;

    public EmployeeMapper(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// DbModel から Domain Entity に変換（読み込み用）
    /// 【責務】DB の プリミティブ型 → Domain の ValueObject に変換
    /// </summary>
    public Employee ToDomainEntity(EmployeeDbModel dbModel, Person person)
    {
        // 従業員種別区分の文字列から Enum に変換
        var typeDivision = dbModel.EmployeeDivision switch
        {
            "M" => BizDivision.RegularEmployee(),
            "D" => BizDivision.Dispatched(),
            "C" => BizDivision.Contractor(),
            _ => throw new InvalidOperationException(
                $"Invalid employee division: {dbModel.EmployeeDivision}")
        };

        var bizId = BizId.From(dbModel.BizId);
        var bizCode = BizCode.From(typeDivision, bizId);

        RetiredOn retiredOn = RetiredOn.Unset;
        if (dbModel.RetiredOn.HasValue)
        {
            retiredOn = RetiredOn.From(new LocalDateTime(dbModel.RetiredOn.Value));
        }

        return Employee.Reconstruct(
            EmployeeRowId.From(dbModel.RowId),
            typeDivision,
            bizId,
            bizCode,
            retiredOn,
            person,
            new List<DepartmentMembership>()
        );
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Insert/Update 用）
    /// 【責務】Domain の ValueObject → DB の プリミティブ型に変換
    /// 【注意】このメソッドは未実装（Insert/Update が実装される際に使用予定）
    /// </summary>
    public EmployeeDbModel ToDbModel(Employee entity)
    {
        var divisionCode = ConvertDivisionToCode(entity.TypeDivision);

        return new EmployeeDbModel
        {
            RowId = entity.RowId.Value,
            EmployeeDivision = divisionCode,
            BizId = entity.BizId.Value,
            RetiredOn = entity.RetiredOn.IsSet ? entity.RetiredOn.Value.Value : null
        };
    }

    private string ConvertDivisionToCode(BizDivision typeDivision)
    {
        if (typeDivision.IsRegularEmployee)
            return "M";

        if (typeDivision.IsDispatched)
            return "D";

        if (typeDivision.IsContractor)
            return "C";

        throw new InvalidOperationException($"Invalid division: {typeDivision}");
    }
}
