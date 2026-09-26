namespace SupportAdvance.Contexts.Authentication.Application.UseCases;

/// <summary>
/// Windows AD 認証で従業員を検索 Use Case
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>現在のユーザーが Windows AD で認証されているか確認</description></item>
/// <item><description>UserAuthSession にセッション情報を記録</description></item>
/// </list>
/// <para>【フロー】</para>
/// <list type="bullet">
/// <item><description>Presentation層から AuthorityRowId（従業員RowId）を受け取る</description></item>
/// <item><description>UserAuthSession を生成（AD認証フラグを立てる）</description></item>
/// <item><description>Repository で保存</description></item>
/// </list>
/// <para>【設計上の注意】</para>
/// <list type="bullet">
/// <item><description>Employee BC への参照は行わない（BC間独立）</description></item>
/// <item><description>AD 統合の詳細は Presentation層で実装</description></item>
/// <item><description>このUseCase はセッション記録のみ担当</description></item>
/// </list>
/// <para>【パターン】</para>
/// <list type="bullet">
/// <item><description>AD認証の詳細な実装（認証確認、属性取得など）は Presentation層で</description></item>
/// <item><description>Authentication BC の責務はセッション記録のみ</description></item>
/// </list>
/// </remarks>
public sealed class FindEmployeeByADUseCase
{
    // 実装予定: Presentation層で AD 認証情報を取得し、
    // AuthorityRowId を受け取ってセッションを生成するロジック
    // 今後の実装で詳細化
}
