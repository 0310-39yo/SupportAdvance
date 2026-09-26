namespace SupportAdvance.Application.Queries;

/// <summary>
/// Employee クエリ結果を表すインターフェース（汎用層）
/// </summary>
/// <remarks>
/// <para>【責務】Context間で共通化できる Employee データの定義</para>
/// <para>【用途】</para>
/// <list type="bullet">
/// <item><description>IntegrationPrototype など、他 Context が Employee 情報を取得した際の結果型</description></item>
/// <item><description>BC間の参照を避けるための中間層インターフェース</description></item>
/// </list>
/// <para>【実装】Employee Context が実装（EmployeeDto または独自の型）</para>
/// </remarks>
public interface IEmployeeQueryResult
{
    /// <summary>
    /// 従業員RowId（集約根）
    /// </summary>
    long RowId { get; }

    /// <summary>
    /// 従業員種別区分（正社員/派遣/請負）
    /// </summary>
    string TypeDivision { get; }

    /// <summary>
    /// ビジネスID（従業員番号）
    /// </summary>
    string BizId { get; }

    /// <summary>
    /// ビジネスコード（表示用）
    /// </summary>
    string BizCode { get; }

    /// <summary>
    /// 人事マスタ行ID
    /// </summary>
    long PersonRowId { get; }

    /// <summary>
    /// 姓
    /// </summary>
    string PersonLastName { get; }

    /// <summary>
    /// 名
    /// </summary>
    string PersonFirstName { get; }

    /// <summary>
    /// 姓（カナ）
    /// </summary>
    string PersonLastNameKana { get; }

    /// <summary>
    /// 名（カナ）
    /// </summary>
    string PersonFirstNameKana { get; }

    /// <summary>
    /// 所属部署名（カンマ区切り、主部署を先頭に）
    /// </summary>
    string DepartmentNames { get; }
}
