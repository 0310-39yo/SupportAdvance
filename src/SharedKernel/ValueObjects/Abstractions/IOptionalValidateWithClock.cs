using SupportAdvance.Common.Clocks;

namespace SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// IClock（時刻）を必須とするオプション値オブジェクトのインターフェース
///
/// 【責務分離】
///   - Validate(TValue): 形式検証（型・範囲・制約などの基本チェック）- Clock 不要
///   - ValidateWithClock(TValue, IClock): ビジネスロジック検証（時間軸依存のルール）- Clock 必須
///
/// 【使用方法】
///   ⚠️ このインターフェースは Clock パラメータが必須です。
///   Clock なしで Validate() のみを呼ぶことは設計エラーです。
///   必ず From(TValue, IClock) または TryFrom(TValue?, IClock, ...) を使用してください。
///
/// 【検証順序】
///   1. Normalize: 値の正規化（トリム、大文字小文字変換等）
///   2. Validate: 形式検証（基本制約チェック）- コンストラクタで自動実行
///   3. ValidateWithClock: ビジネス検証（時間軸関連ルール）- From/TryFrom で明示的に実行
///
/// 【実装例】
///   RespondentAt（回答日時）
///     - Validate: DateTime.MinValue/MaxValue の除外（形式チェック）
///     - ValidateWithClock: 未来日の除外（ビジネスルール）
///
/// IOptionalValueObject は継承しない（clock パラメータなしの From/TryFrom が不要なため）
/// </summary>
/// <typeparam name="TSelf">オプション値オブジェクト自身の型</typeparam>
/// <typeparam name="TValue">内部値の型</typeparam>
public interface IOptionalValidateWithClock<TSelf, TValue> :
    IValidateWithClock<TValue>,
    IValidatable<TValue>
    where TSelf : IOptionalValidateWithClock<TSelf, TValue>
{
    /// <summary>
    /// 内部値を取得する
    /// 値が設定されている場合は true を返し、<paramref name="value" /> に値を設定する
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
    /// IClock を使用して、指定された値からインスタンスを生成します。
    ///
    /// 【実行される検証】
    ///   1. コンストラクタ内で Validate(TValue) - 形式検証
    ///   2. From メソッド内で ValidateWithClock(TValue, IClock) - ビジネスロジック検証
    ///
    /// 検証失敗時は例外をスロー（呼び出し側で処理が必須）。
    /// エラーハンドリングが必要な場合は TryFrom を使用してください。
    /// </summary>
    /// <param name="value">設定する値（正規化済み）</param>
    /// <param name="clock">現在時刻を供給する IClock インスタンス</param>
    /// <returns>検証済みで IsSet=true の有効なインスタンス（非null）</returns>
    /// <remarks>
    /// 【実装の責務】
    ///   - new インスタンスでコンストラクタを呼び出す（Validate が自動実行される）
    ///   - その後 ValidateWithClock を明示的に呼び出す（例外をキャッチしない）
    ///
    /// 【例外の取り扱い】
    ///   - Validate 失敗: ArgumentException/ArgumentOutOfRangeException/FormatException 等
    ///   - ValidateWithClock 失敗: ArgumentException 等（呼び出し側で処理）
    /// </remarks>
    /// <exception cref="System.ArgumentException">ビジネスロジック検証失敗</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">値が範囲外</exception>
    /// <exception cref="System.FormatException">形式が不正</exception>
    static abstract TSelf From(TValue value, IClock clock);

    /// <summary>
    /// IClock を使用して、指定された値からインスタンスの生成を試みます。
    ///
    /// 【結果の判定】
    ///   - input が null: true 返却、result は Unset（正常な未設定状態）
    ///   - Validate/ValidateWithClock 成功: true 返却、result は設定済みインスタンス
    ///   - Validate/ValidateWithClock 失敗: false 返却、result は Unset（無効な入力）
    ///
    /// API 層や DB 復元時など、外部入力を安全に処理する場合に使用します。
    /// </summary>
    /// <param name="input">生成に使用する値（null 許容）</param>
    /// <param name="clock">現在時刻を供給する IClock インスタンス</param>
    /// <param name="result">生成結果を受け取る out パラメータ（失敗時は Unset）</param>
    /// <returns>
    /// 生成に成功した場合、または null 入力を Unset に変換した場合は true。
    /// 検証失敗（Validate または ValidateWithClock が例外をスロー）した場合は false。
    /// </returns>
    /// <remarks>
    /// 【実装の責務】
    ///   - input が null なら Unset を返して true を返す（正常な未設定）
    ///   - null でない場合、From(input.Value, clock) を try-catch で呼び出す
    ///   - 例外をキャッチして false を返す（検証失敗時）
    ///
    /// 【例外ハンドリング】
    ///   Validate および ValidateWithClock からの例外をキャッチします。
    ///   一般的には ArgumentException とその派生クラスが対象です。
    /// </remarks>
    static abstract bool TryFrom(TValue? input, IClock clock, out TSelf result);
}
