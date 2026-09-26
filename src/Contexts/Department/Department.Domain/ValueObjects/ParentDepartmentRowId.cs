using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Department.Domain.ValueObjects;

/// <summary>
/// 親部署の行IDを表すオプション ValueObject
/// </summary>
/// <remarks>
/// <para>【範囲】IsSet=true の場合は 1以上、IsSet=false で「親部署なし」を表現</para>
/// <para>【責務】t_departments.parent_department_row_id の管理と検証</para>
/// </remarks>
public sealed class ParentDepartmentRowId : RowId, IEquatable<ParentDepartmentRowId>
{
    /// <summary>
    /// プライベートコンストラクタ（IsSet=false 用）
    /// </summary>
    private ParentDepartmentRowId(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// プライベートコンストラクタ（IsSet=true 用）
    /// </summary>
    private ParentDepartmentRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// ルート部署（親なし）を表す Unset インスタンスの生成
    /// </summary>
    /// <returns>IsSet=false のインスタンス</returns>
    public static ParentDepartmentRowId Unset() => new(false);

    /// <summary>
    /// 指定された親部署行IDからの ParentDepartmentRowId の生成
    /// </summary>
    /// <param name="value">親部署行ID（1以上）</param>
    /// <returns>生成された ParentDepartmentRowId インスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public static ParentDepartmentRowId From(long value) => new(value);

    /// <summary>
    /// 指定された親部署行IDから ParentDepartmentRowId の生成を試みる（型安全版）
    /// null は Unset に変換して成功を返す
    /// </summary>
    /// <param name="input">親部署行ID（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、検証失敗時 false</returns>
    public static bool TryFrom(long? input, out ParentDepartmentRowId result)
    {
        result = null!;

        if (!input.HasValue)
        {
            result = Unset(); // null は Unset に変換（親なし）
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// DB値からの変換（null は Unset に変換）
    /// </summary>
    /// <param name="input">DB から読み込んだ行ID（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、検証失敗時 false</returns>
    public static bool TryFromDbValue(long? input, out ParentDepartmentRowId result)
    {
        result = null!;

        if (!input.HasValue)
        {
            result = Unset(); // DB NULL は Unset に変換（親なし）
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 親部署があるかどうかを判定する（IsSet の別名）
    /// </summary>
    public bool HasParent => IsSet;

    /// <summary>
    /// 値を検証する（RowId の abstract メソッド実装）
    /// </summary>
    /// <param name="normalized">検証対象の値</param>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public override void Validate(long normalized)
    {
        if (normalized <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                "ParentDepartmentRowId must be greater than 0.");
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ParentDepartmentRowId);

    /// <inheritdoc/>
    public bool Equals(ParentDepartmentRowId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IsSet == other.IsSet && Value == other.Value;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();
}
