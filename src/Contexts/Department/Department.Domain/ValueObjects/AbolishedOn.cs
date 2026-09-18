using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Department.Domain.ValueObjects;

/// <summary>
/// 部署の廃止日を表すオプション ValueObject
/// 【範囲】IsSet=true の場合は有効な LocalDateTime、IsSet=false で「廃止されていない」を表現
/// 【責務】t_departments.abolished_on の管理と検証
/// 【業務意味】論理削除ではなく、部署の廃止年月日を記録
/// </summary>
public sealed class AbolishedOn : ValueObject, IEquatable<AbolishedOn>
{
    /// <summary>
    /// 廃止日の値を取得する（IsSet=true の場合のみ有効）
    /// </summary>
    public LocalDateTime Value { get; }

    /// <summary>
    /// 廃止されているかどうかを判定する（IsSet の別名）
    /// </summary>
    public bool IsAbolished => IsSet;

    /// <summary>
    /// IsSet フラグ
    /// </summary>
    public new bool IsSet { get; }

    /// <summary>
    /// プライベートコンストラクタ（IsSet=false 用）
    /// </summary>
    private AbolishedOn(bool isSet)
    {
        IsSet = isSet;
        Value = LocalDateTime.MinValue;
    }

    /// <summary>
    /// プライベートコンストラクタ（IsSet=true 用）
    /// </summary>
    private AbolishedOn(LocalDateTime value)
    {
        IsSet = true;
        Value = value;
    }

    /// <summary>
    /// 廃止されていない部署を表す Unset インスタンスを生成する
    /// </summary>
    /// <returns>IsSet=false のインスタンス</returns>
    public static AbolishedOn Unset() => new(false);

    /// <summary>
    /// 指定された廃止日から AbolishedOn を生成する
    /// </summary>
    /// <param name="value">廃止日（LocalDateTime）</param>
    /// <returns>生成された AbolishedOn インスタンス</returns>
    /// <exception cref="ArgumentNullException">value が null の場合</exception>
    public static AbolishedOn From(LocalDateTime value) => new(value);

    /// <summary>
    /// 指定された廃止日から AbolishedOn の生成を試みる（型安全版）
    /// null は Unset に変換して成功を返す
    /// </summary>
    /// <param name="input">廃止日（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、検証失敗時 false</returns>
    public static bool TryFrom(LocalDateTime? input, out AbolishedOn result)
    {
        result = null!;

        if (input is null)
        {
            result = Unset(); // null は Unset に変換（廃止なし）
            return true;
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
    /// DB値からの変換（null は Unset に変換）
    /// </summary>
    /// <param name="input">DB から読み込んだ廃止日（null許容）</param>
    /// <param name="result">生成されたインスタンス</param>
    /// <returns>成功時 true、検証失敗時 false</returns>
    public static bool TryFromDbValue(DateTime? input, out AbolishedOn result)
    {
        result = null!;

        if (!input.HasValue)
        {
            result = Unset(); // DB NULL は Unset に変換（廃止なし）
            return true;
        }

        try
        {
            var localDateTime = new LocalDateTime(input.Value);
            result = From(localDateTime);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AbolishedOn);

    /// <inheritdoc/>
    public bool Equals(AbolishedOn? other)
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
    /// 文字列表現を取得する
    /// </summary>
    /// <returns>廃止日の文字列。未設定（廃止されていない）場合は <c>Not Abolished</c></returns>
    public override string ToString()
    {
        return IsSet ? Value.ToString() : "Not Abolished";
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return IsSet;
        if (IsSet)
        {
            yield return Value;
        }
    }
}
