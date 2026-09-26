namespace SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// 値が設定されているかどうかを表すオプション値オブジェクトのインターフェイス
/// </summary>
/// <typeparam name="TSelf">オプション値オブジェクト自身の型</typeparam>
/// <typeparam name="TValue">内部値の型</typeparam>
public interface IOptionalValueObject<TSelf, TValue> : IValidatable<TValue>
    where TSelf : IOptionalValueObject<TSelf, TValue>
{
    /// <summary>
    /// 内部値の取得
    /// 値が設定されている場合は true を返し、<paramref name="value" /> に値の設定
    /// 値が未設定の場合は false を返す
    /// </summary>
    /// <param name="value">取得した内部値（out パラメータ）</param>
    /// <returns>値が設定されている場合は true、未設定の場合は false</returns>
    bool TryGetValue(out TValue value);

    /// <summary>
    /// 未設定インスタンスを返す
    /// </summary>
    /// <returns>未設定の <typeparamref name="TSelf" /> インスタンス</returns>
    static abstract TSelf Unset();

    /// <summary>
    /// 値設定済みインスタンス（IsSet=true）を生成する静的ファクトリ
    /// </summary>
    /// <param name="value">検証済みの値（正規化済み）</param>
    /// <returns>設定済み状態のValueObjectインスタンス（非null）</returns>
    /// <remarks>
    /// <para>入力検証失敗時は例外を投げる（ArgumentException/ArgumentOutOfRangeException/FormatException等）</para>
    /// <para>正規化（Normalize）と検証（Validate）をこのメソッド内で実施</para>
    /// <para>返却値は必ずIsSet=trueの有効なインスタンス</para>
    /// </remarks>
    /// <exception cref="System.ArgumentException">入力検証失敗</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">範囲外</exception>
    /// <exception cref="System.FormatException">フォーマット失敗</exception>
    static abstract TSelf From(TValue value);

    /// <summary>
    /// 指定された値からインスタンスの生成の試行。
    /// <paramref name="input" /> が null の場合は <see cref="Unset" /> を返して true を返します（正常）。
    /// 値が無効な場合は <see cref="Unset" /> を返して false を返します（エラー）。
    /// API 層で null 許容の入力を処理する場合の使用
    /// </summary>
    /// <param name="input">生成に使用する値（nullable）</param>
    /// <param name="result">生成結果を受け取る out パラメータ</param>
    /// <returns>生成に成功した場合、または null で Unset に変換した場合は true; 値が無効な場合は false</returns>
    /// <remarks>
    /// <para>エラーを投げない安全な生成メソッド</para>
    /// <para>inputがnull: true返却、resultはUnset（正常な未設定状態）</para>
    /// <para>From成功: true返却、resultは設定済みインスタンス</para>
    /// <para>From失敗（例外）: false返却、resultはUnset（無効な入力）</para>
    /// <para>外部（JSON/API/DB）からのnull入力を許容するためTValue?を使用</para>
    /// </remarks>
    static abstract bool TryFrom(TValue? input, out TSelf result);
}
