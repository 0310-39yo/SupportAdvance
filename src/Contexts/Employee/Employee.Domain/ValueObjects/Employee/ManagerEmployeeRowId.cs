using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 部署の管理者（従業員）の行IDを表すオプション ValueObject
///
/// 責務：
/// - 管理者従業員行ID（1以上）をオプション型で型安全に保持
/// - 値の検証（正の整数）
/// - null を Unset に変換（IOptionalValueObject パターン）
/// - 等価性判定とハッシュコード計算
/// </summary>
public sealed class ManagerEmployeeRowId : PrimitiveValueObject<long?>, IEquatable<ManagerEmployeeRowId>
{
    /// <summary>
    /// 管理者従業員行IDの値を取得する（IsSet=false の場合は null）
    /// </summary>
    public long? Value => ValueField;

    /// <summary>
    /// 指定された管理者従業員行IDから ManagerEmployeeRowId を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">管理者従業員行ID</param>
    /// <param name="isSet">設定済みフラグ</param>
    private ManagerEmployeeRowId(long? value, bool isSet) : base(value, isSet)
    {
    }

    /// <summary>
    /// 管理者がいない部署を表す Unset インスタンスを生成する
    /// </summary>
    /// <returns>IsSet=false のインスタンス</returns>
    public static ManagerEmployeeRowId Unset() => new(null, false);

    /// <summary>
    /// 指定された管理者従業員行IDから ManagerEmployeeRowId を生成する
    /// </summary>
    /// <param name="value">管理者従業員行ID（1以上）</param>
    /// <returns>生成された ManagerEmployeeRowId インスタンス</returns>
    /// <exception cref="ArgumentException">値が 0 以下の場合</exception>
    public static ManagerEmployeeRowId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("ManagerEmployeeRowId must be greater than 0.", nameof(value));
        }

        return new ManagerEmployeeRowId(value, true);
    }

    /// <summary>
    /// 指定された管理者従業員行IDから ManagerEmployeeRowId の生成を試みる（型安全版）
    /// null は Unset に変換して成功を返す（IOptionalValueObject パターン）
    /// </summary>
    /// <param name="input">管理者従業員行ID（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、検証失敗時 false（例外なし）</returns>
    public static bool TryFrom(long? input, out ManagerEmployeeRowId result)
    {
        if (!input.HasValue)
        {
            result = Unset(); // null は Unset に変換して成功（管理者なし）
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch
        {
            result = Unset();
            return false;
        }
    }

    /// <summary>
    /// DB値からの変換（null は Unset に変換）
    /// </summary>
    /// <param name="input">DB から読み込んだ行ID（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、検証失敗時 false</returns>
    public static bool TryFromDbValue(long? input, out ManagerEmployeeRowId result)
    {
        if (!input.HasValue)
        {
            result = Unset(); // DB NULL は Unset に変換（管理者なし）
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch
        {
            result = Unset();
            return false;
        }
    }

    /// <summary>
    /// 管理者がいるかどうかを判定する（IsSet の別名）
    /// </summary>
    public bool HasManager => IsSet;

    /// <summary>
    /// オブジェクト等価性を判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as ManagerEmployeeRowId);

    /// <summary>
    /// ManagerEmployeeRowId 間の等価性を判定する
    /// </summary>
    public bool Equals(ManagerEmployeeRowId? other)
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

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(IsSet, Value);

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString() => IsSet ? Value?.ToString() ?? string.Empty : "Unset";

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す
    /// </summary>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// 値を検証する
    /// </summary>
    /// <param name="normalized">検証対象の値</param>
    public override void Validate(long? normalized)
    {
        base.Validate(normalized);

        if (normalized.HasValue && normalized.Value <= 0)
        {
            throw new ArgumentException("ManagerEmployeeRowId must be greater than 0.", nameof(normalized));
        }
    }
}
