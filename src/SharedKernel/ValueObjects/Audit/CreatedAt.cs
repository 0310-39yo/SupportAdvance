using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.SharedKernel.ValueObjects.Audit;

/// <summary>
/// エンティティの作成日時を表すValueObject
/// 【設計】LocalDateTime を内部値として保持（Domain層での型安全性確保）
/// 【用途】Domain/Application層: LocalDateTime で扱う、Infrastructure層: DateTime に変換
/// </summary>
public sealed class CreatedAt : PrimitiveValueObject<LocalDateTime>, IEquatable<CreatedAt>
{
    /// <summary>
    /// 指定された LocalDateTime からCreatedAtのインスタンスを生成する
    /// 【責務】指定された日時を持つCreatedAtを表現する
    /// </summary>
    /// <param name="value">LocalDateTime値</param>
    /// <returns>指定された日時を持つCreatedAtのインスタンス</returns>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private CreatedAt(LocalDateTime value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された LocalDateTime からCreatedAtのインスタンスを生成する（推奨: Domain層での生成方式）
    /// 【責務】LocalDateTime を持つCreatedAtを表現する
    /// </summary>
    /// <param name="value">LocalDateTime値（IClock.JstNow から取得）</param>
    /// <returns>指定された日時を持つCreatedAtのインスタンス</returns>
    public static CreatedAt From(LocalDateTime value) => new(value);

    /// <summary>
    /// 指定された DateTime からCreatedAtのインスタンスを生成する（Infrastructure層での型変換用）
    /// 【責務】DB から読み込んだ DateTime を LocalDateTime に変換して CreatedAt を生成
    /// </summary>
    /// <param name="value">DateTime値（DB読み込み値）</param>
    /// <returns>指定された日時を持つCreatedAtのインスタンス</returns>
    public static CreatedAt FromDbValue(DateTime value) => new(new LocalDateTime(value));

    /// <summary>
    /// DB値への変換（DateTime を取得）
    /// 【責務】Mapper で Entity → DbModel への変換時に使用
    /// </summary>
    /// <returns>内部保持の LocalDateTime から DateTime を抽出</returns>
    public DateTime ToDbValue() => ValueField.Value;

    /// <summary>
    /// 指定された LocalDateTime からCreatedAtのインスタンスの生成を試みる（型安全版）
    /// 【責務】null安全に CreatedAt を生成する（Domain層での生成方式）
    /// </summary>
    /// <param name="input">LocalDateTime? 値</param>
    /// <param name="result">生成されたCreatedAtのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(LocalDateTime? input, out CreatedAt result)
    {
        if (input == null || !input.HasValue)
        {
            result = null!;
            return false;
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
    /// 指定された DateTime からCreatedAtのインスタンスの生成を試みる（NULL安全版、Infrastructure層での型変換用）
    /// 【責務】DB値から null安全に CreatedAt を生成する（NULL は失敗）
    /// </summary>
    /// <param name="input">DateTime? 値（DB読み込み値）</param>
    /// <param name="result">生成されたCreatedAtのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFromDbValue(DateTime? input, out CreatedAt result)
    {
        if (input == null)
        {
            result = null!;
            return false;
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
    /// 保持する LocalDateTime 値を取得する
    /// 【責務】保持する値を取得する
    /// </summary>
    /// <returns>保持する LocalDateTime 値</returns>
    public LocalDateTime Value => ValueField;

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// 【責務】指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as CreatedAt);

    /// <summary>
    /// 指定されたCreatedAtと等価かどうかを判定する
    /// 【責務】指定されたCreatedAtと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のCreatedAt</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(CreatedAt? other)
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
        yield return ValueField;
    }

    /// <summary>
    /// 正規化済み値の検証を行う
    /// 【責務】業務ルールに基づく値の妥当性のチェック
    /// </summary>
    /// <param name="normalized">正規化済みの LocalDateTime 値</param>
    /// <exception cref="ArgumentException">値が有効な日時でない場合にスローされる</exception>
    public override void Validate(LocalDateTime normalized)
    {
        base.Validate(normalized);

        // DateTime.MinValue や DateTime.MaxValue は除外
        if (normalized.Value == DateTime.MinValue || normalized.Value == DateTime.MaxValue)
        {
            throw new ArgumentException(
                $"CreatedAt must be a valid system timestamp, not {nameof(DateTime.MinValue)} or {nameof(DateTime.MaxValue)}.");
        }
    }
}
