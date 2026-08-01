using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.SharedKernel.ValueObjects.Audit;

/// <summary>
/// エンティティの最後の更新日時を表すValueObject（null許容、IsSet で未更新状態を表現）
/// </summary>
public sealed class UpdatedAt : PrimitiveValueObject<DateTime?>, IEquatable<UpdatedAt>
{
    /// <summary>
    /// 指定された日時からUpdatedAtのインスタンスを生成する
    /// 【責務】指定された日時を持つUpdatedAtを表現する
    /// </summary>
    /// <param name="value">日時値</param>
    /// <param name="isSet">IsSet フラグ（デフォルト: true）</param>
    /// <returns>指定された日時を持つUpdatedAtのインスタンス</returns>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private UpdatedAt(DateTime? value, bool isSet = true) : base(value, isSet)
    {
    }

    /// <summary>
    /// 指定された日時からUpdatedAtのインスタンスを生成する（推奨: LocalDateTime で取得）
    /// 【責務】指定された日時を持つUpdatedAtを表現する
    /// </summary>
    /// <param name="value">LocalDateTime値（IClock.JstNow から取得）</param>
    /// <returns>指定された日時を持つUpdatedAtのインスタンス</returns>
    public static UpdatedAt From(LocalDateTime value) => new(value.Value, true);

    /// <summary>
    /// 未更新状態のUpdatedAtのインスタンスを生成する
    /// 【責務】未更新状態を表現する
    /// </summary>
    /// <returns>未更新状態のUpdatedAtのインスタンス</returns>
    public static UpdatedAt Unset() => new(null, false);

    /// <summary>
    /// 指定された日時からUpdatedAtのインスタンスを生成する（層間の型変換用）
    /// null が来た場合は Unset() で変換（成功）
    /// 【責務】null安全に UpdatedAt を生成する
    /// </summary>
    /// <param name="input">LocalDateTime? 値（DB からの読み込み値）</param>
    /// <param name="result">生成されたUpdatedAtのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
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
    /// 保持する値を取得する
    /// 【責務】保持する値を取得する（IsSet = true の時のみ有効）
    /// </summary>
    /// <returns>保持する日時値（null 許容）</returns>
    public DateTime? Value => ValueField;

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

        return ValueField == other.ValueField;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// 【責務】オブジェクトのハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => ValueField.GetHashCode();

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す（IsSet を除く）
    /// IsSet は ValueObject.GetEqualityComponents で自動的に先頭に付加される
    /// </summary>
    /// <returns>ValueField を含むコンポーネント列</returns>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return ValueField; // DateTime? を返す
    }

    /// <summary>
    /// 正規化済み値の検証を行う
    /// 【責務】業務ルールに基づく値の妥当性のチェック
    /// </summary>
    /// <param name="normalized">正規化済みの値（null 許容）</param>
    /// <exception cref="ArgumentException">値が有効な日時でない場合にスローされる</exception>
    public override void Validate(DateTime? normalized)
    {
        base.Validate(normalized);

        // null は許容（未更新状態を表現）
        if (normalized == null)
        {
            return;
        }

        // DateTime.MinValue や DateTime.MaxValue は除外
        if (normalized == DateTime.MinValue || normalized == DateTime.MaxValue)
        {
            throw new ArgumentException(
                $"UpdatedAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}.");
        }
    }
}
