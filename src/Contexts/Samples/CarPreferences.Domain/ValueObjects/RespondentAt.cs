using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// 回答日時を表すValueObject
/// IClock を使用したビジネスロジック検証が必要なため IOptionalValidateWithClock を実装
/// </summary>
public sealed class RespondentAt : PrimitiveValueObject<LocalDateTime>,
    IOptionalValidateWithClock<RespondentAt, LocalDateTime>,
    IEquatable<RespondentAt>
{
    /// <summary>
    /// 未設定状態のインスタンスを生成するコンストラクタ
    /// </summary>
    /// <remarks>
    /// 【責務】未設定状態を表現するインスタンスを構築する
    /// </remarks>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentAt(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 指定された日時からインスタンスを生成するコンストラクタ
    /// </summary>
    /// <remarks>
    /// 【責務1】指定された日時値を正規化・検証してインスタンスを構築する
    /// 【責務2】Validate メソッドは基底クラスのコンストラクタで自動実行される
    /// </remarks>
    /// <param name="value">日時の値</param>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentAt(LocalDateTime value, bool isSet) : base(value, isSet)
    {
    }

    /// <summary>
    /// 未設定インスタンスを生成する
    /// </summary>
    /// <remarks>
    /// 【責務】未設定状態を表現するインスタンスを返す
    /// </remarks>
    /// <returns>未設定状態の RespondentAt インスタンス</returns>
    public static RespondentAt Unset() => new(false);

    /// <summary>
    /// 指定された日時からインスタンスを生成する（Clock を使用した検証付き）
    /// </summary>
    /// <remarks>
    /// 【責務1】指定された日時値を正規化・形式検証する（Validate は基底コンストラクタで自動実行）
    /// 【責務2】ビジネスロジック検証を実施する（ValidateWithClock で未来日を除外）
    /// </remarks>
    /// <param name="value">RespondentAt として設定する日時値</param>
    /// <param name="clock">現在時刻を取得するためのクロックインターフェース</param>
    /// <returns>検証済みで設定状態（IsSet=true）のインスタンス</returns>
    /// <exception cref="ArgumentException">形式検証またはビジネスロジック検証に失敗した場合</exception>
    public static RespondentAt From(LocalDateTime value, IClock clock)
    {
        var instance = new RespondentAt(value, true);
        instance.ValidateWithClock(value, clock);
        return instance;
    }

    /// <summary>
    /// 指定された日時からインスタンスの生成を試みる（Clock を使用した検証付き、nullable 版）
    /// </summary>
    /// <remarks>
    /// 【責務】外部入力を安全に処理する（null は未設定状態に、検証失敗時も未設定状態に変換）
    /// </remarks>
    /// <param name="input">生成に使用する日時値（null 許容）</param>
    /// <param name="clock">現在時刻を取得するためのクロックインターフェース</param>
    /// <param name="result">生成結果を受け取る out パラメータ</param>
    /// <returns>生成に成功した場合、または null 入力を Unset に変換した場合は true；検証失敗時は false</returns>
    public static bool TryFrom(LocalDateTime? input, IClock clock, out RespondentAt result)
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
    /// 指定された日時からインスタンスの生成を試みる（Clock を使用した検証付き、非 nullable 版）
    /// </summary>
    /// <remarks>
    /// 【責務】nullable 版の TryFrom を呼び出す便利メソッド
    /// </remarks>
    /// <param name="input">生成に使用する日時値</param>
    /// <param name="clock">現在時刻を取得するためのクロックインターフェース</param>
    /// <param name="result">生成結果を受け取る out パラメータ</param>
    /// <returns>生成に成功した場合は true；検証失敗時は false</returns>
    public static bool TryFrom(LocalDateTime input, IClock clock, out RespondentAt result)
        => TryFrom((LocalDateTime?)input, clock, out result);

    /// <summary>
    /// ビジネスロジック検証を実施する（Clock を使用）
    /// </summary>
    /// <remarks>
    /// 【責務1】回答日時が未来日でないかをチェックする
    /// 【責務2】Validate（形式検証）との役割分担：Validate は DateTime.MinValue/MaxValue の除外、ValidateWithClock はビジネスルール（未来日除外）を担当
    /// </remarks>
    /// <param name="value">検証対象の回答日時</param>
    /// <param name="clock">現在時刻を供給するクロック</param>
    /// <exception cref="ArgumentException">未来日が指定された場合</exception>
    public void ValidateWithClock(LocalDateTime value, IClock clock)
    {
        var nowJst = clock.JstNow.Value;
        if (value.Value > nowJst)
        {
            throw new ArgumentException("RespondentAt cannot be in the future.", nameof(value));
        }
    }

    /// <summary>
    /// 保持する LocalDateTime 値を取得する
    /// </summary>
    /// <remarks>
    /// IsSet=true の場合は LocalDateTime 値を返し、IsSet=false の場合は LocalDateTime.MinValue を返す
    /// </remarks>
    public LocalDateTime Value => IsSet ? ValueField : LocalDateTime.MinValue;

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合は true、そうでない場合は false</returns>
    public override bool Equals(object? obj) => Equals(obj as RespondentAt);

    /// <summary>
    /// 指定された RespondentAt インスタンスと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象の RespondentAt</param>
    /// <returns>等価である場合は true、そうでない場合は false</returns>
    public bool Equals(RespondentAt? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IsSet == other.IsSet && (!IsSet || ValueField == other.ValueField);
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode()
    {
        if (!IsSet)
        {
            return HashCode.Combine(false);
        }

        return HashCode.Combine(true, ValueField);
    }

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す（IsSet を除く）
    /// </summary>
    /// <remarks>
    /// IsSet は ValueObject.GetEqualityComponents で自動的に先頭に付加される
    /// </remarks>
    /// <returns>ValueField（IsSet = true の場合）を含むコンポーネント列</returns>
    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet)
        {
            yield return ValueField;
        }
    }

    /// <summary>
    /// 形式検証を実施する（LocalDateTime の基本的な妥当性をチェック）
    /// </summary>
    /// <remarks>
    /// 【責務1】LocalDateTime.MinValue/MaxValue を除外する
    /// 【責務2】形式的な有効性をチェックする（現在時刻は不要）
    /// 【責務3】ビジネスロジック検証（未来日チェック）は ValidateWithClock で別途実施
    /// </remarks>
    /// <param name="normalized">検証対象の日時</param>
    /// <exception cref="ArgumentException">MinValue または MaxValue が指定された場合</exception>
    public override void Validate(LocalDateTime normalized)
    {
        base.Validate(normalized);

        if (normalized == LocalDateTime.MinValue || normalized == LocalDateTime.MaxValue)
        {
            throw new ArgumentException(
                $"RespondentAt must be a valid system timestamp, not {nameof(LocalDateTime.MinValue)} or {nameof(LocalDateTime.MaxValue)}.");
        }
    }
}
