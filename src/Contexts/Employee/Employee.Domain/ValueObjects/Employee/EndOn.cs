using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 異動終了日時を表す ValueObject
/// 【型】LocalDateTime のラッパー（null 許可）
/// 【状態】null: 無期限（継続中）、value: 異動終了日
/// 【責務】部署配属・ロール・権限の終了日を管理
/// </summary>
public sealed class EndOn : ValueObject, IEquatable<EndOn>
{
    /// <summary>終了日時（未設定時は null）</summary>
    public LocalDateTime? Value { get; }

    /// <summary>終了が設定されているか（Value != null）</summary>
    public bool HasEnded => Value.HasValue;

    /// <summary>無期限状態（Value == null）を表す静的プロパティ</summary>
    public static EndOn Unlimited => new(null);

    /// <summary>
    /// 指定された終了日時から EndOn を生成する（プライベートコンストラクタ）
    /// </summary>
    private EndOn(LocalDateTime? value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定された終了日から EndOn を生成する
    /// </summary>
    /// <param name="value">終了日（JST）</param>
    /// <returns>EndOn インスタンス</returns>
    public static EndOn From(LocalDateTime value) => new(value);

    /// <summary>
    /// DB値から EndOn を復元する（null → Unlimited）
    /// </summary>
    /// <param name="value">DB の datetime2 値（NULL 許可）</param>
    /// <param name="result">復元された EndOn</param>
    /// <returns>復元成功時 true</returns>
    public static bool TryFromDbValue(DateTime? value, out EndOn result)
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
    /// 指定された EndOn と等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as EndOn);

    /// <summary>
    /// 指定された EndOn と等価かどうかを判定する
    /// </summary>
    public bool Equals(EndOn? other)
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
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString() => Value?.ToString() ?? "無期限";

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す
    /// </summary>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }
}
