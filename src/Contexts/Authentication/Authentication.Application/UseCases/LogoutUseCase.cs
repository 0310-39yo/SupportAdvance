using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Domain.Repositories;
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
