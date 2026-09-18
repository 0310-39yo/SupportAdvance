using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員の退職日を表す ValueObject
/// 【型】LocalDateTime のラッパー
/// 【状態】IsSet=true: 退職済み、IsSet=false: 現職
/// 【タイムゾーン】JST（日本標準時）
/// 【用途】従業員の在職状況判定
/// </summary>
public sealed class RetiredOn : ValueObject, IEquatable<RetiredOn>
{
    /// <summary>
    /// Unset 時のダミー値
    /// </summary>
    /// <remarks>
    /// <para>【注意】現在どこからも参照されていない値。未設定の判定には <see cref="HasRetired"/> を使用</para>
    /// </remarks>
    public const long UnsetValue = 0L; // Unset 時のダミー値

    /// <summary>
    /// 退職日（未設定時は LocalDateTime.MinValue）
    /// </summary>
    public LocalDateTime Value { get; }

    /// <summary>
    /// 未設定状態フラグ（isSet=true で退職済み）
    /// </summary>
    public new bool IsSet { get; }

    /// <summary>
    /// 退職状況フラグ（HasRetired=true で退職済み）
    /// </summary>
    public bool HasRetired => IsSet;

    /// <summary>
    /// Unset 状態（現職）を表す静的メソッド
    /// </summary>
    /// <returns>在職中（<see cref="HasRetired"/> が <see langword="false"/>）のインスタンス（<see langword="null"/> なし）</returns>
    public static RetiredOn Unset() => new(LocalDateTime.MinValue, false);

    /// <summary>
    /// 指定された退職日から RetiredOn を生成する（プライベートコンストラクタ）
    /// </summary>
    private RetiredOn(LocalDateTime value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    /// <summary>
    /// 指定された退職日から RetiredOn を生成する
    /// </summary>
    /// <param name="value">退職日（JST）</param>
    /// <returns>RetiredOn インスタンス</returns>
    public static RetiredOn From(LocalDateTime value) => new(value, true);

    /// <summary>
    /// DB値から RetiredOn を復元する（null → Unset）
    /// </summary>
    /// <param name="value">DB の datetime2 値（NULL 許可）</param>
    /// <param name="result">復元された RetiredOn</param>
    /// <returns>復元成功時 true</returns>
    public static bool TryFromDbValue(DateTime? value, out RetiredOn result)
    {
        result = null!;

        if (!value.HasValue)
        {
            result = Unset();
            return true;
        }

        try
        {
            result = From(new LocalDateTime(value.Value));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RetiredOn);

    /// <inheritdoc/>
    public bool Equals(RetiredOn? other)
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
    /// <returns>退職日の文字列。未設定（在職中）の場合は <c>現職</c></returns>
    public override string ToString() => IsSet ? Value.ToString() : "現職";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return IsSet;
        yield return Value;
    }
}
