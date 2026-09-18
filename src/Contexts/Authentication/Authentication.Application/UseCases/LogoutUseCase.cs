using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Application.Repositories;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Authentication.Application.UseCases;

/// <summary>
/// ログアウト Use Case
///
/// 【責務】
/// - UserAuthSession の LoggedOutAt を設定
/// - セッション終了を記録
///
/// 【フロー】
/// 1. Repository からセッションを取得
/// 2. LoggedOutAt を現在時刻に設定
/// 3. Repository で更新
///
/// 【用途】
/// - アプリケーション終了時に呼び出し（ILogoutUseCase パターン）
/// - Presentation層（WPF/WinForms）の終了処理から実行
///
/// 【セキュリティ】
/// - ログアウト日時を記録することで、セッションの正常終了を監査できる
/// - NULL の logged_out_at は異常終了（クラッシュ等）を示す
/// </summary>
public sealed class LogoutUseCase
{
    private readonly IUserAuthSessionRepository _sessionRepository;
    private readonly IClock _clock;

    /// <summary>
    /// <see cref="LogoutUseCase"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="sessionRepository">ログアウトするセッションの取得・保存先</param>
    /// <param name="clock">ログアウト日時（JST）の取得元</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public LogoutUseCase(
        IUserAuthSessionRepository sessionRepository,
        IClock clock)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// セッションをログアウト
    /// </summary>
    /// <param name="sessionRowId">ログアウト対象のセッション RowId</param>
    /// <exception cref="InvalidOperationException">セッションが見つからない</exception>
    /// <exception cref="ArgumentNullException"><paramref name="sessionRowId"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【副作用】セッションのログアウト日時（JST）を設定して更新</para>
    /// </remarks>
    public async Task ExecuteAsync(UserAuthSessionRowId sessionRowId)
    {
        ArgumentNullException.ThrowIfNull(sessionRowId);

        // Step 1: セッションを取得
        var session = await _sessionRepository.GetByIdAsync(sessionRowId);
        if (session == null)
        {
            throw new InvalidOperationException(
                $"Logout failed: UserAuthSession with RowId {sessionRowId.Value} not found.");
        }

        // Step 2: LoggedOutAt を設定
        var now = _clock.JstNow;
        session.SetLoggedOutAt(now);

        // Step 3: Repository で更新
        await _sessionRepository.UpdateAsync(session);
    }
}
