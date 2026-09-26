using SupportAdvance.Common.Clocks;

namespace SupportAdvance.SharedKernel.ValueObjects;

/// <summary>
/// IClock（現在時刻）を使用したビジネスロジック検証を行うインターフェース
/// </summary>
/// <typeparam name="TValue">検証対象の値の型</typeparam>
/// <remarks>
/// <para>【責務】現在時刻に依存するビジネスロジックのチェック</para>
/// <list type="bullet">
/// <item><description>時間軸ベースの制約（「未来日は不可」「有効期限内か」など）</description></item>
/// <item><description>期間の判定（「昨日以降か」「営業時間内か」など）</description></item>
/// <item><description>時刻ベースの権限チェック（時間帯に応じたアクセス制御など）</description></item>
/// </list>
/// <para>【基本検証との違い】</para>
/// <list type="bullet">
/// <item><description>IValidatable: 形式チェック（Clock 不要）</description></item>
/// <item><description>IValidateWithClock: ビジネスロジック（Clock 必須）</description></item>
/// </list>
/// <para>【使用パターン】</para>
/// <list type="bullet">
/// <item><description>IValidatable のみ実装: 時間に関係ない基本的な値オブジェクト。例) RespondentAge（年齢）- 形式のみ検証</description></item>
/// <item><description>IValidateWithClock を実装: Clock 必須の値オブジェクト。例) RespondentAt（回答日時）- 形式検証 + 時間軸検証</description></item>
/// </list>
/// </remarks>
public interface IValidateWithClock<TValue>
{
    /// <summary>
    /// 指定された時刻を基準とした、ビジネスロジック的な検証の実施
    /// </summary>
    /// <param name="value">検証対象の値</param>
    /// <param name="clock">現在時刻を供給する <see cref="IClock" /> インスタンス</param>
    /// <remarks>
    /// <para>【検証対象】現在時刻（IClock から取得）に依存するビジネスルール。例) 「回答日時が未来日でないか」「有効期限内か」など</para>
    /// 【検証失敗時の動作】
    /// ビジネスロジック検証に失敗した場合は例外をスロー（呼び出し側で処理が必須）
    ///   - ArgumentException: 最も一般的（ビジネスルール違反）
    ///   - その他: 特殊なケースで使用可能
    /// </remarks>
    /// <exception cref="ArgumentException">ビジネスロジック検証失敗（例: 未来日）</exception>
    void ValidateWithClock(TValue value, IClock clock);
}
