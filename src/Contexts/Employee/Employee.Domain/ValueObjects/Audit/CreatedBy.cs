namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Audit;

using System.Collections.Generic;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// 作成者の従業員行IDを表す必須 ValueObject
///
/// 責務：
/// - 作成者の行IDを型安全に保持
/// - 値が 0 より大きいことを検証
/// - 等価性判定とハッシュコード計算
/// </summary>
public sealed class CreatedBy : PrimitiveValueObject<long>, IEquatable<CreatedBy>
{
    /// <summary>
    /// 指定された従業員行IDから CreatedBy を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">従業員行ID</param>
    private CreatedBy(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された従業員行IDから CreatedBy を生成する
    /// </summary>
    /// <param name="value">従業員行ID（1以上）</param>
    /// <returns>生成された CreatedBy インスタンス</returns>
    /// <exception cref="ArgumentException">値が 0 以下の場合</exception>
    public static CreatedBy From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("CreatedBy must be greater than 0.", nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// 指定された従業員行IDから CreatedBy の生成を試みる（型安全版）
    /// </summary>
    /// <param name="input">従業員行ID（null不許容）</param>
    /// <param name="result">生成されたインスタンス、失敗時は null</param>
    /// <returns>成功時 true、失敗時 false（例外なし）</returns>
    public static bool TryFrom(long? input, out CreatedBy result)
    {
        result = null!;

        if (!input.HasValue)
        {
            return false;  // null は失敗
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// DB値からの変換（null は失敗）
    /// </summary>
    /// <param name="input">DB から読み込んだ行ID（null不許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、失敗時 false</returns>
    public static bool TryFromDbValue(long? input, out CreatedBy result)
    {
        result = null!;

        if (!input.HasValue)
        {
            return false;  // DB NULL は失敗
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 従業員行IDを取得する
    /// </summary>
    public long Value => ValueField;

    /// <summary>
    /// オブジェクト等価性を判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as CreatedBy);

    /// <summary>
    /// CreatedBy 間の等価性を判定する
    /// </summary>
    public bool Equals(CreatedBy? other)
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

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString() => Value.ToString();

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
    public override void Validate(long normalized)
    {
        base.Validate(normalized);

        if (normalized <= 0)
        {
            throw new ArgumentException("CreatedBy must be greater than 0.", nameof(normalized));
        }
    }
}

