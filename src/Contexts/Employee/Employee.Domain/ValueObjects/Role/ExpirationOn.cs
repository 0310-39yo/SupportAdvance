using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;

/// <summary>
/// 有効終了日時を表す ValueObject
/// 【型】LocalDateTime のラッパー
/// 【状態】IsSet=true: 有効期限あり、IsSet=false: 無期限
/// 【責務】ロール・権限割り当ての有効終了日時を管理
/// </summary>
public sealed class ExpirationOn : ValueObject, IEquatable<ExpirationOn>
{
    /// <summary>有効終了日時（未設定時は LocalDateTime.MinValue）</summary>
    public LocalDateTime Value { get; }

    /// <summary>設定状態フラグ（IsSet=true で有効期限あり）</summary>
    public new bool IsSet { get; }

    /// <summary>無期限フラグ（HasExpiration=false で無期限）</summary>
    public bool HasExpiration => IsSet;

    /// <summary>無期限状態（IsSet=false）を表す静的プロパティ</summary>
    public static ExpirationOn Unlimited => new(LocalDateTime.MinValue, false);

    /// <summary>
    /// 指定された有効終了日時から ExpirationOn を生成する（プライベートコンストラクタ）
    /// </summary>
    private ExpirationOn(LocalDateTime value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    /// <summary>
    /// 指定された有効終了日から ExpirationOn を生成する
    /// </summary>
    /// <param name="value">有効終了日（JST）</param>
    /// <returns>ExpirationOn インスタンス</returns>
    public static ExpirationOn From(LocalDateTime value) => new(value, true);

    /// <summary>
    /// DB値から ExpirationOn を復元する（null → Unlimited）
    /// </summary>
    /// <param name="value">DB の datetime2 値（NULL 許可）</param>
    /// <param name="result">復元された ExpirationOn</param>
    /// <returns>復元成功時 true</returns>
    public static bool TryFromDbValue(DateTime? value, out ExpirationOn result)
    {
        result = null!;

        if (!value.HasValue)
        {
            result = Unlimited;
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

    /// <summary>
    /// 指定された ExpirationOn と等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as ExpirationOn);

    /// <summary>
    /// 指定された ExpirationOn と等価かどうかを判定する
    /// </summary>
    public bool Equals(ExpirationOn? other)
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
    public override string ToString() => IsSet ? Value.ToString() : "無期限";

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す
    /// </summary>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return IsSet;
        yield return Value;
    }
}
