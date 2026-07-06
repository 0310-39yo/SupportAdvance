using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

public sealed class RespondentAt : PrimitiveValueObject<DateTime>, 
    IOptionalValidateWithClock<RespondentAt, DateTime>,
    IEquatable<RespondentAt>
{
    /// <summary>
    /// 未設定状態のRespondentAtのインスタンスを生成する
    /// 【責務】未設定状態のRespondentAtを表現する
    /// </summary>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentAt(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 指定された日時からRespondentAtのインスタンスを生成する
    /// 【責務】指定された日時を表現するRespondentAtを生成する
    /// </summary>
    /// <param name="value">日時の値</param>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentAt(DateTime value,bool isSet) : base(value, true)
    {
    }

    /// <summary>
    /// 未設定インスタンスを取得します。
    ///
    /// 【責務】未設定状態を表現する
    /// </summary>
    public static RespondentAt Unset() => new(false);

    /// <summary>
    /// 指定された日時からインスタンスを生成します。
    /// 
    /// 【責務】値の検証・生成を行う。Validate は基底コンストラクタで呼ばれる。
    /// </summary>
    /// <param name="value">RespondentAt として設定する日時値（DateTime）</param>
    /// <param name="clock">現在時刻を取得するためのクロックインターフェース</param>
    /// <returns>設定済みの <see cref="RespondentAt"></see>
    /// インスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">値が許容範囲外の場合</exception>
    /// <remarks>Validate は,基底クラスのコンストラクタで自動実行される</remarks>
    public static RespondentAt From(DateTime value, IClock clock)
    {
        var instance = new RespondentAt(value, true);
        instance.ValidateWithClock(value, clock);
        return instance;
    }

    public static bool TryFrom(DateTime? input, IClock clock, out RespondentAt result)
    {
        if (!input.HasValue)
        {
            result = Unset();
            return true;
        }

        try
        {
            result = From(input.Value, clock);
            return true;
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;
        }
    }

    /// <summary>
    /// 非 nullable 版の TryFrom（インターフェース実装用）。
    /// </summary>
    public static bool TryFrom(DateTime input, IClock clock, out RespondentAt result) 
        => TryFrom((DateTime?)input, clock, out result);



    public void ValidateWithClock(DateTime value, IClock clock)
    {
        // 未来日のチェック（JstNow.Value は LocalDateTime なので DateTime に変換）
        var nowJst = clock.JstNow.Value;
        if (value > nowJst)
        {
            throw new ArgumentException("RespondentAt cannot be in the future.", nameof(value));
        }
    }



    public DateTime Value => ValueField;

    public override bool Equals(object? obj) => Equals(obj as RespondentAt);

    public bool Equals(RespondentAt? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ValueField == other.ValueField;
    }

    public override int GetHashCode() => ValueField.GetHashCode();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ValueField;
    }

    public override void Validate(DateTime normalized)
    {
        base.Validate(normalized);

        if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
        {
            throw new ArgumentException($"RespondentAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}.");
        }
    }
}
