using System.ComponentModel.DataAnnotations.Schema;

namespace SupportAdvance.Contexts.Employee.Infrastructure.Models;

/// <summary>
/// Employee テーブルマッピングモデル
///
/// 【テーブル】m_employees
/// 【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持
/// 【特徴】ビジネスカラムのみ（監査カラムは Repository で自動管理、属性テーブルは別途）
/// </summary>
public class EmployeeDbModel
{
    /// <summary>
    /// データベース行ID（主キー、Sequence自動採番）
    /// 【対応カラム】row_id
    /// </summary>
    [Column("row_id")]
    public long RowId { get; set; }

    /// <summary>
    /// ビジネス区分（M/D/C のいずれか）
    /// 【対応カラム】biz_division
    /// 【値】M=正社員, D=派遣, C=契約
    /// </summary>
    [Column("biz_division")]
    public string BizDivision { get; set; } = string.Empty;

    /// <summary>
    /// ビジネスID（従業員番号、1001以上）
    /// 【対応カラム】biz_id
    /// 【特徴】区分ごとに有効範囲が異なる
    /// </summary>
    [Column("biz_id")]
    public int BizId { get; set; }

    /// <summary>
    /// 退職日（在職状況を示す）
    /// 【対応カラム】retired_on
    /// 【制約】NULL許可（現職時は NULL）
    /// </summary>
    [Column("retired_on")]
    public DateTime? RetiredOn { get; set; }

    /// <summary>
    /// 人物RowId（m_persons の row_id）
    /// 【対応カラム】person_row_id（JOINで取得）
    /// </summary>
    [Column("person_row_id")]
    public long PersonRowId { get; set; }

    /// <summary>
    /// 姓（m_persons から JOIN で取得）
    /// 【対応カラム】last_name
    /// </summary>
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// 名（m_persons から JOIN で取得）
    /// 【対応カラム】first_name
    /// </summary>
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// 姓（カナ）（m_persons から JOIN で取得）
    /// 【対応カラム】last_name_kana
    /// </summary>
    [Column("last_name_kana")]
    public string LastNameKana { get; set; } = string.Empty;

    /// <summary>
    /// 名（カナ）（m_persons から JOIN で取得）
    /// 【対応カラム】first_name_kana
    /// </summary>
    [Column("first_name_kana")]
    public string FirstNameKana { get; set; } = string.Empty;
}
