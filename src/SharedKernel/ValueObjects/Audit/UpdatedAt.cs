using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.SharedKernel.ValueObjects.Audit;

/// <summary>
/// エンティティの最後の更新日時を表すValueObject（LocalDateTime 保持、null許容、IsSet で未更新状態を表現）
/// 【設計】LocalDateTime を内部値として保持（Domain層での型安全性確保）
/// 【用途】Domain/Application層: LocalDateTime で扱う、Infrastructure層: DateTime に変換
/// </summary>
public sealed class UpdatedAt : PrimitiveValueObject<LocalDateTime?>, IEquatable<UpdatedAt>
{
    /// <summary>
    /// 指定された LocalDateTime からUpdatedAtのインスタンスを生成する
    /// 【責務】指定された日時を持つUpdatedAtを表現する
    /// </summary>
    /// <param name="value">LocalDateTime値</param>
    /// <param name="isSet">IsSet フラグ（デフォルト: true）</param>
    /// <returns>指定された日時を持つUpdatedAtのインスタンス</returns>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private UpdatedAt(LocalDateTime? value, bool isSet = true) : base(value, isSet)
    {
    }

    /// <summary>
    /// 指定された LocalDateTime からUpdatedAtのインスタンスを生成する（推奨: Domain層での生成方式）
    /// 【責務】指定された日時を持つUpdatedAtを表現する
    /// </summary>
    /// <param name="value">LocalDateTime値（IClock.JstNow から取得）</param>
    /// <returns>指定された日時を持つUpdatedAtのインスタンス</returns>
    public static UpdatedAt From(LocalDateTime value) => new(value, true);

    /// <summary>
    /// 未更新状態のUpdatedAtのインスタンスを生成する
    /// 【責務】未更新状態を表現する（IsSet=false）
    /// 【設計】Value は常に LocalDateTime を保持（Domain層での型安全性確保）。IsSet=false で未更新状態を判定
    /// </summary>
    /// <returns>未更新状態のUpdatedAtのインスタンス</returns>
    public static UpdatedAt Unset() => new(new LocalDateTime(DateTime.MinValue), false);

    /// <summary>
    /// 指定された DateTime からUpdatedAtのインスタンスを生成する（Infrastructure層での型変換用）
    /// 【責務】DB から読み込んだ DateTime を LocalDateTime に変換して UpdatedAt を生成
    /// </summary>
    /// <param name="value">DateTime値（DB読み込み値）</param>
    /// <returns>指定された日時を持つUpdatedAtのインスタンス</returns>
    public static UpdatedAt FromDbValue(DateTime value) => new(new LocalDateTime(value), true);

    /// <summary>
    /// DB値への変換（DateTime を取得）
    /// 【責務】Mapper で Entity → DbModel への変換時に使用
    /// </summary>
    /// <returns>内部保持の LocalDateTime から DateTime を抽出（HasUpdated=true の場合のみ有効）</returns>
    public DateTime ToDbValue() => IsSet && ValueField.HasValue ? ValueField.Value.Value : throw new InvalidOperationException("UpdatedAt is not set.");

    /// <summary>
    /// 指定された LocalDateTime からUpdatedAtのインスタンスの生成を試みる（型安全版）
    /// 【責務】null安全に UpdatedAt を生成する（Domain層での生成方式）
    /// </summary>
    /// <param name="input">LocalDateTime? 値</param>
    /// <param name="result">生成されたUpdatedAtのインスタンス</param>
    /// <returns>生成に成功した場合、または null 入力を Unset に変換した場合は true；検証失敗時は false</returns>
    public static bool TryFrom(LocalDateTime? input, out UpdatedAt result)
    {
        if (input == null || !input.HasValue)
        {
            result = Unset(); // ← null → Unset() で成功
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            result = null!;
            return false;
        }
    }

    /// <summary>
    /// 指定された DateTime からUpdatedAtのインスタンスの生成を試みる（NULL安全版、Infrastructure層での型変換用）
    /// 【責務】DB値から null安全に UpdatedAt を生成する（NULL → Unset 状態に変換）
    /// </summary>
    /// <param name="input">DateTime? 値（DB読み込み値）</param>
    /// <param name="result">生成されたUpdatedAtのインスタンス</param>
    /// <returns>生成に成功した場合、または null 入力を Unset に変換した場合は true；検証失敗時は false</returns>
    public static bool TryFromDbValue(DateTime? input, out UpdatedAt result)
    {
        if (input == null)
        {
            result = Unset(); // ← null → Unset() で成功
            return true;
        }

        try
        {
            result = FromDbValue(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            result = null!;
            return false;
        }
    }

    /// <summary>
    /// 保持する LocalDateTime? 値を取得する
    /// 【責務】保持する値を取得する（IsSet = true の時のみ有効）
    /// </summary>
    /// <returns>保持する LocalDateTime? 値（null 許容）</returns>
    public LocalDateTime? Value => ValueField;

    /// <summary>
    /// 更新済み状態を判定する（IsSet の別名）
    /// 【責務】更新済みか未更新かを判定する
    /// </summary>
    /// <returns>更新済みの場合は true、未更新の場合は false</returns>
    public bool HasUpdated => IsSet;

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// 【責務】指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as UpdatedAt);

    /// <summary>
    /// 指定されたUpdatedAtと等価かどうかを判定する
    /// 【責務】指定されたUpdatedAtと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のUpdatedAt</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(UpdatedAt? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ValueField == other.ValueField && IsSet == other.IsSet;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// 【責務】オブジェクトのハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => HashCode.Combine(ValueField, IsSet);

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す（IsSet を除く）
    /// IsSet は ValueObject.GetEqualityComponents で自動的に先頭に付加される
    /// </summary>
    /// <returns>ValueField を含むコンポーネント列</returns>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return ValueField; // LocalDateTime? を返す
    }

    /// <summary>
    /// 正規化済み値の検証を行う
    /// 【責務】業務ルールに基づく値の妥当性のチェック
    /// </summary>
    /// <param name="normalized">正規化済みの LocalDateTime? 値</param>
    /// <exception cref="ArgumentException">値が有効な日時でない場合にスローされる</exception>
    public override void Validate(LocalDateTime? normalized)
    {
        base.Validate(normalized);

        // null は許容（未更新状態を表現）
        if (normalized == null || !normalized.HasValue)
        {
            return;
        }

        var value = normalized.Value;
        // DateTime.MinValue や DateTime.MaxValue は除外
        if (value.Value == DateTime.MinValue || value.Value == DateTime.MaxValue)
        {
            throw new ArgumentException(
                $"UpdatedAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}.");
        }
    }
}
