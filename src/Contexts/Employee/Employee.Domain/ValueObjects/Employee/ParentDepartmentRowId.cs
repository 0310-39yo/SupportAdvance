using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 親部署の行IDを表すオプション ValueObject
///
/// 責務：
/// - 親部署行ID（1以上）をオプション型で型安全に保持
/// - 値の検証（正の整数）
/// - null を Unset に変換（IOptionalValueObject パターン）
/// - 等価性判定とハッシュコード計算
/// </summary>
public sealed class ParentDepartmentRowId : PrimitiveValueObject<long?>, IEquatable<ParentDepartmentRowId>
{
    /// <summary>
    /// 親部署行IDの値を取得する（IsSet=false の場合は null）
    /// </summary>
    public long? Value => ValueField;

    /// <summary>
    /// 指定された親部署行IDから ParentDepartmentRowId を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">親部署行ID</param>
    /// <param name="isSet">設定済みフラグ</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private ParentDepartmentRowId(long? value, bool isSet) : base(value, isSet)
    {
    }

    /// <summary>
    /// ルート部署（親なし）を表す Unset インスタンスを生成する
    /// </summary>
    /// <returns>IsSet=false のインスタンス</returns>
    public static ParentDepartmentRowId Unset() => new(null, false);

    /// <summary>
    /// 指定された親部署行IDから ParentDepartmentRowId を生成する
    /// </summary>
    /// <param name="value">親部署行ID（1以上）</param>
    /// <returns>生成された ParentDepartmentRowId インスタンス</returns>
    /// <exception cref="ArgumentException">値が 0 以下の場合</exception>
    public static ParentDepartmentRowId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("ParentDepartmentRowId must be greater than 0.", nameof(value));
        }

        return new ParentDepartmentRowId(value, true);
    }

    /// <summary>
    /// 指定された親部署行IDから ParentDepartmentRowId の生成を試みる（型安全版）
    /// null は Unset に変換して成功を返す（IOptionalValueObject パターン）
    /// </summary>
    /// <param name="input">親部署行ID（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、検証失敗時 false（例外なし）</returns>
    public static bool TryFrom(long? input, out ParentDepartmentRowId result)
    {
        if (!input.HasValue)
        {
            result = Unset(); // null は Unset に変換して成功（親なし）
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
    public static bool TryFromDbValue(long? input, out ParentDepartmentRowId result)
    {
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
        catch
        {
            result = Unset();
            return false;
        }
    }

    /// <summary>
    /// 親部署があるかどうかを判定する（IsSet の別名）
    /// </summary>
    public bool HasParent => IsSet;

    /// <summary>
    /// オブジェクト等価性を判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as ParentDepartmentRowId);

    /// <summary>
    /// ParentDepartmentRowId 間の等価性を判定する
    /// </summary>
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
            throw new ArgumentException("ParentDepartmentRowId must be greater than 0.", nameof(normalized));
        }
    }
}
