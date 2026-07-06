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



    /// <summary>
    /// ビジネスロジック検証: 現在時刻を基準として、回答日時が妥当であるかをチェック
    ///
    /// 【検証内容】
    ///   - 未来日が設定されていないか（回答は必ず過去日時であるべき）
    ///
    /// 【基本検証との役割分担】
    ///   - Validate: DateTime の形式チェック（MinValue/MaxValue の除外）
    ///   - ValidateWithClock: ビジネスロジック（未来日の除外）
    /// </summary>
    /// <param name="value">検証対象の回答日時</param>
    /// <param name="clock">現在時刻を供給するクロック</param>
    /// <exception cref="ArgumentException">未来日が指定された場合</exception>
    public void ValidateWithClock(DateTime value, IClock clock)
    {
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
        return IsSet == other.IsSet && (!IsSet || ValueField == other.ValueField);
    }

    public override int GetHashCode()
    {
        if (!IsSet)
        {
            return HashCode.Combine(false);
        }
        return HashCode.Combine(true, ValueField);
    }

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet)
        {
            yield return ValueField;
        }
    }

    /// <summary>
    /// 基本検証: DateTime の形式的な妥当性をチェック
    ///
    /// 【検証内容】
    ///   - DateTime.MinValue/MaxValue の除外（これらは無効な時刻表現）
    ///   - その他の形式チェック（必要に応じて派生クラスで拡張可能）
    ///
    /// 【Clock を必要としない理由】
    ///   DateTime の形式的な有効性のチェックのため、現在時刻は不要です。
    ///   ビジネスロジック的な検証（未来日チェック）は ValidateWithClock で実施します。
    /// </summary>
    /// <param name="normalized">検証対象の日時</param>
    /// <exception cref="ArgumentException">MinValue または MaxValue が指定された場合</exception>
    public override void Validate(DateTime normalized)
    {
        base.Validate(normalized);

        if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
        {
            throw new ArgumentException($"RespondentAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}.");
        }
    }
}
