using SupportAdvance.Contexts.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Identity.Application.UseCases;

/// <summary>
/// Windows AD 認証で従業員を検索 Use Case
///
/// 【責務】
/// - 現在のユーザーが Windows AD で認証されているか確認
/// - UserAuthSession にセッション情報を記録
///
/// 【フロー】
/// 1. Presentation層から AuthorityRowId（従業員RowId）を受け取る
/// 2. UserAuthSession を生成（AD認証フラグを立てる）
/// 3. Repository で保存
///
/// 【設計上の注意】
/// - Employee BC への参照は行わない（BC間独立）
/// - AD 統合の詳細は Presentation層で実装
/// - このUseCase はセッション記録のみ担当
///
/// 【パターン】
/// - AD認証の詳細な実装（認証確認、属性取得など）は Presentation層で
/// - Identity BC はセッション記録のみを責務とする
/// </summary>
public sealed class FindEmployeeByADUseCase
{
    // 実装予定: Presentation層で AD 認証情報を取得し、
    // AuthorityRowId を受け取ってセッションを生成するロジック
    // 今後の実装で詳細化
}
