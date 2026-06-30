// ========================================
// UseCase テンプレート：Request
// ========================================

namespace SupportAdvance.Application.UseCases.Templates
{
    using SupportAdvance.Application.UseCases;
    
    /// <summary>
    /// テンプレート: UseCase の Request DTO
    /// 
    /// 使用方法:
    /// 1. このファイルをコピー
    /// 2. [EntityName] を実際のエンティティ名に置換
    /// 3. [ActionName] をアクション名に置換（Get, Create, Update等）
    /// 4. プロパティをビジネス要件に合わせて調整
    /// </summary>
    public class [ActionName][EntityName]Request : IRequest
    {
        /// <summary>
        /// 例: ID指定での取得
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 例: 更新用パラメータ
        /// </summary>
        public string? Parameter { get; set; }
        
        /// <summary>
        /// 複数パラメータの場合
        /// </summary>
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
