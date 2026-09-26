namespace SupportAdvance.SharedKernel.ValueObjects.Audit;

using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;



using System.Collections.Generic;


/// <summary>
/// 削除者の従業員行IDを表すオプション ValueObject（論理削除用）
///
/// 責務：
/// - 削除者の行IDを型安全に保持（オプション）
/// - 値が 0 より大きいことを検証
/// - null を Unset に変換（IOptionalValueObject パターン）
/// - 論理削除状態を管理（未削除=Unset, 削除済み=値を保持）
/// - 等価性判定とハッシュコード計算
/// </summary>
public sealed class DeletedBy : PrimitiveValueObject<long?>, IEquatable<DeletedBy>
{
    /// <summary>
    /// 指定された従業員行IDから DeletedBy を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">従業員行ID</param>
    /// <param name="isSet">削除済みフラグ</param>
    private DeletedBy(long? value, bool isSet) : base(value, isSet)
    {
    }

    /// <summary>
    /// 未削除状態の DeletedBy の生成
    /// </summary>
    /// <returns>IsSet=false のインスタンス</returns>
    public static DeletedBy Unset()
    {
        return new(null, false);
    }

    /// <summary>
    /// 指定された従業員行IDからの DeletedBy の生成
    /// </summary>
    /// <param name="value">従業員行ID（1以上）</param>
    /// <returns>生成された DeletedBy インスタンス</returns>
    /// <exception cref="ArgumentException">値が 0 以下の場合</exception>
    public static DeletedBy From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("DeletedBy must be greater than 0.", nameof(value));
        }

        return new(value, true);
    }

    /// <summary>
    /// 指定された従業員行IDから DeletedBy の生成を試みる（型安全版）
    /// null は Unset に変換して成功を返す（IOptionalValueObject パターン）
    /// </summary>
    /// <param name="input">従業員行ID（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、検証失敗時 false（例外なし）</returns>
    public static bool TryFrom(long? input, out DeletedBy result)
    {
        if (!input.HasValue)
        {
            result = Unset();  // null は Unset に変換して成功（未削除）
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
    public static bool TryFromDbValue(long? input, out DeletedBy result)
    {
        if (!input.HasValue)
        {
            result = Unset();  // DB NULL は Unset に変換（未削除）
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
    /// 従業員行IDを取得する（IsSet=false の場合は null）
    /// </summary>
    public long? Value => ValueField;

    /// <summary>
    /// 削除済みかどうかを判定する（IsSet の別名）
    /// </summary>
    public bool IsDeleted => IsSet;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as DeletedBy);

    /// <inheritdoc/>
    public bool Equals(DeletedBy? other)
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
    public override int GetHashCode() => HashCode.Combine(IsSet, Value);

    /// <summary>
    /// 文字列表現の取得
    /// </summary>
    /// <returns>削除者の従業員rowId の文字列。未設定の場合は <c>Unset</c></returns>
    public override string ToString() => IsSet ? Value?.ToString() ?? string.Empty : "Unset";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// 値の検証
    /// </summary>
    /// <param name="normalized">検証対象の値</param>
    /// <exception cref="ArgumentException"><paramref name="normalized"/> が 0 以下の場合（<see langword="null"/> は許容）</exception>
    public override void Validate(long? normalized)
    {
        base.Validate(normalized);

        if (normalized.HasValue && normalized.Value <= 0)
        {
            throw new ArgumentException("DeletedBy must be greater than 0.", nameof(normalized));
        }
    }
}





