namespace SupportAdvance.Common.Clocks;

/// <summary>
/// BusinessDayClock（<see cref="BusinessDayClock"/>）の ON／OFF を操作するための口
/// </summary>
/// <remarks>
/// <para>【用途】Composition Root での起動時・終了時の自動の ON／OFF と、検証用画面のボタンからの操作</para>
/// <para>【設計】利用側が <see cref="IClock"/> を <see cref="BusinessDayClock"/> にキャストしなくて済むよう、操作用の口を分離。<c>ClockType</c> が <c>BusinessDay</c> 以外の場合は DI に未登録</para>
/// <para>【参照】docs/Common/Clocks/BusinessDayClock_技術仕様書.md</para>
/// </remarks>
public interface IBusinessDayClockControl
{
    /// <summary>
    /// ON 中かどうかを示す値
    /// </summary>
    /// <value>業務日のセッション中の場合は <see langword="true"/>。一度も ON していない場合と OFF 中は <see langword="false"/></value>
    bool IsOn { get; }

    /// <summary>
    /// 現在の業務日
    /// </summary>
    /// <value><see cref="IClock.JstNow"/> の日付部分と同じ値</value>
    DateOnly CurrentBusinessDate { get; }

    /// <summary>
    /// 業務日のセッションの開始
    /// </summary>
    /// <exception cref="IOException">状態ファイルの書き込みに失敗した場合</exception>
    /// <exception cref="UnauthorizedAccessException">状態ファイルへの書き込み権限がない場合</exception>
    /// <remarks>
    /// <para>【副作用】最後に終了した業務日の翌日（なしの場合は基点日）から業務日を開始し、状態ファイルに保存</para>
    /// <para>【注意】すでに ON 中の場合は何もしない（べき等）</para>
    /// </remarks>
    void TurnOn();

    /// <summary>
    /// 業務日のセッションの終了
    /// </summary>
    /// <exception cref="IOException">状態ファイルの書き込みに失敗した場合</exception>
    /// <exception cref="UnauthorizedAccessException">状態ファイルへの書き込み権限がない場合</exception>
    /// <remarks>
    /// <para>【副作用】終了時点の業務日（0 時越えを反映した日）を、最後に終了した業務日として状態ファイルに保存</para>
    /// <para>【注意】ON 中でない場合は何もしない（べき等）</para>
    /// </remarks>
    void TurnOff();

    /// <summary>
    /// 業務日の進行のやり直し（基点日から再開）
    /// </summary>
    /// <exception cref="IOException">状態ファイルの削除に失敗した場合</exception>
    /// <exception cref="UnauthorizedAccessException">状態ファイルの削除権限がない場合</exception>
    /// <remarks>
    /// <para>【副作用】状態ファイルを削除し、一度も ON していない状態に戻す。ON 中の場合もセッションを破棄（終了日の保存なし）</para>
    /// </remarks>
    void Reset();
}
