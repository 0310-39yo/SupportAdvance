namespace SupportAdvance.Contexts.Employee.Infrastructure.Mappers;

using Models;
using Domain.Entities;
using Domain.ValueObjects.Person;

/// <summary>
/// Person ドメインモデル ↔ PersonDbModel のマッピング
///
/// 【責務】ValueObject ↔ プリミティブ型の双方向変換
/// 【層の責務】
///   - Domain/Application: ValueObject（型安全性）
///   - DbModel: プリミティブ型（ORM マッピング）
///   - Mapper: 変換ロジック（層の橋渡し）
/// </summary>
public static class PersonMapper
{
    /// <summary>
    /// DbModel から Domain Entity に変換（読み込み用）
    /// 【責務】DB の プリミティブ型 → Domain の ValueObject に変換
    /// </summary>
    public static Person ToDomainEntity(PersonDbModel dbModel)
    {
        ArgumentNullException.ThrowIfNull(dbModel);

        return Person.Reconstruct(
            PersonRowId.From(dbModel.RowId),
            LastName.From(dbModel.LastName),
            FirstName.From(dbModel.FirstName),
            LastNameKana.From(dbModel.LastNameKana),
            FirstNameKana.From(dbModel.FirstNameKana)
        );
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Insert/Update 用）
    /// 【責務】Domain の ValueObject → DB の プリミティブ型に変換
    /// 【パラメータ】employeeRowId は 1:1 リンク用（employee_row_id FK）
    /// </summary>
    public static PersonDbModel ToDbModel(Person entity, long employeeRowId)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new PersonDbModel
        {
            RowId = entity.RowId.Value,
            EmployeeRowId = employeeRowId,
            LastName = entity.LastName.Value,
            FirstName = entity.FirstName.Value,
            LastNameKana = entity.LastNameKana.Value,
            FirstNameKana = entity.FirstNameKana.Value
        };
    }
}
