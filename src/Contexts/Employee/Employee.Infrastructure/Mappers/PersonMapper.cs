namespace SupportAdvance.Contexts.Employee.Infrastructure.Mappers;

using Models;
using Domain.Entities;
using Domain.ValueObjects.Person;

/// <summary>
/// Person ドメインモデル ↔ PersonDbModel のマッピング
/// </summary>
/// <remarks>
/// <para>【責務】ValueObject ↔ プリミティブ型の双方向変換</para>
/// <para>【層の責務】</para>
/// <list type="bullet">
/// <item><description>Domain/Application: ValueObject（型安全性）</description></item>
/// <item><description>DbModel: プリミティブ型（ORM マッピング）</description></item>
/// <item><description>Mapper: 変換ロジック（層の橋渡し）</description></item>
/// </list>
/// </remarks>
public static class PersonMapper
{
    /// <summary>
    /// DbModel から Domain Entity に変換（読み込み用）
    /// </summary>
    /// <param name="dbModel"><c>m_persons</c> の行</param>
    /// <returns>復元した人物</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dbModel"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="ArgumentException">DB の値が値オブジェクトの検証に通らない場合（DB の整合性エラー。<see cref="ArgumentOutOfRangeException"/> を含む）</exception>
    /// <remarks>
    /// <para>【責務】DB の プリミティブ型 → Domain の ValueObject に変換</para>
    /// </remarks>
    public static Person ToDomainEntity(PersonDbModel dbModel)
    {
        ArgumentNullException.ThrowIfNull(dbModel);

        return Person.Reconstruct(
            PersonRowId.From(dbModel.RowId),
            LastName.From(dbModel.LastName),
            FirstName.From(dbModel.FirstName),
            LastNameKana.From(dbModel.LastNameKana),
            FirstNameKana.From(dbModel.FirstNameKana),
            dbModel.RowVersion
        );
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Insert/Update 用）
    /// </summary>
    /// <param name="entity">変換する人物</param>
    /// <param name="employeeRowId">紐づく従業員の行ID（<c>employee_row_id</c>）</param>
    /// <returns>業務データのみ設定した DB モデル（監査列は未設定）</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【責務】Domain の ValueObject → DB の プリミティブ型に変換</para>
    /// <para>【パラメータ】employeeRowId は 1:1 リンク用（employee_row_id FK）</para>
    /// </remarks>
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
