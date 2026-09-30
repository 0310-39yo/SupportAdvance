namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// 他の Bounded Context へ公開する、従業員の読み取り専用の要約
/// </summary>
/// <remarks>
/// <para>【配置】SharedKernel に定義（すべての層から参照可能）</para>
/// <para>【用途】<see cref="IEmployee.ToSummary"/> の戻り値。他 Context は Employee の具象型を参照せずに従業員の情報を読み取る</para>
/// <para>【特徴】値はすべて表示用の文字列または数値（値オブジェクトは含まない）</para>
/// </remarks>
/// <param name="RowId">従業員の行ID</param>
/// <param name="TypeDivision">従業員区分の表示名</param>
/// <param name="BizId">ビジネスID（従業員番号）の文字列</param>
/// <param name="BizCode">ビジネスコードの文字列</param>
/// <param name="PersonRowId">個人の行ID</param>
/// <param name="PersonLastName">姓</param>
/// <param name="PersonFirstName">名</param>
/// <param name="PersonLastNameKana">姓（カナ）</param>
/// <param name="PersonFirstNameKana">名（カナ）</param>
/// <param name="DepartmentNames">所属部署名。主所属を先頭にしたカンマ区切り（所属なしの場合は空文字）</param>
public sealed record EmployeeSummary(
    long RowId,
    string TypeDivision,
    string BizId,
    string BizCode,
    long PersonRowId,
    string PersonLastName,
    string PersonFirstName,
    string PersonLastNameKana,
    string PersonFirstNameKana,
    string DepartmentNames);
