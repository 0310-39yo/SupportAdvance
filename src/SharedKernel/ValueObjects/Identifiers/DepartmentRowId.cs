namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// データベース上の部署レコードの行ID（rowId）を表す ValueObject
/// </summary>
/// <remarks>
/// <para>【範囲】1以上（long.MaxValue以下）</para>
/// <para>【責務】t_departments.row_id の管理と検証</para>
/// </remarks>
public sealed class DepartmentRowId : RowId, IEquatable<DepartmentRowId>
{
    /// <summary>
    /// 部署行IDの最小有効値
    /// </summary>
    public const long MinValue = 1L;

    /// <summary>
    /// 部署行IDの値の取得
    /// </summary>
    public new long Value => ValueField;

    /// <summary>
    /// 指定された部署行IDから DepartmentRowId を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">部署行ID（1以上）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタでの自動実行</remarks>
    private DepartmentRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された部署行IDからの DepartmentRowId の生成
    /// </summary>
    /// <param name="value">部署行ID（1以上）</param>
    /// <returns>指定された行IDの DepartmentRowId インスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public static DepartmentRowId From(long value) => new(value);

    /// <summary>
    /// 指定された部署行IDから DepartmentRowId の生成を試みる（型安全版）
    /// </summary>
    /// <param name="value">部署行ID</param>
    /// <param name="result">生成された DepartmentRowId インスタンス</param>
    /// <returns>成功時 true、失敗時 false</returns>
    public static bool TryFrom(long value, out DepartmentRowId result)
    {
        result = null!;

        try
        {
            result = From(value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// DB値からの変換
    /// </summary>
    /// <param name="value">DB の department.row_id 値</param>
    /// <param name="result">生成された DepartmentRowId インスタンス</param>
    /// <returns>成功時 true、失敗時 false</returns>
    public static bool TryFromDbValue(long value, out DepartmentRowId result)
    {
        result = null!;

        try
        {
            result = From(value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 部署行IDの文字列表現の取得
    /// </summary>
    /// <returns>数値文字列（例："12345"）</returns>
    public override string ToString() => Value.ToString();

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as DepartmentRowId);

    /// <summary>
    /// 指定された DepartmentRowId と等価かどうかの判定
    /// </summary>
    /// <param name="other">比較対象の DepartmentRowId</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(DepartmentRowId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 部署行IDが有効か検証
    /// </summary>
    /// <param name="normalized">検証対象の値</param>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public override void Validate(long normalized)
    {
        // 1以上か確認
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"DepartmentRowId must be {MinValue} or higher.");
        }
    }
}
