// ========================================
// UseCase テンプレート：Response
// ========================================

namespace SupportAdvance.Application.UseCases.Templates
{
    using SupportAdvance.Application.UseCases;
    
    /// <summary>
    /// テンプレート: UseCase の Response DTO
    /// 
    /// 使用方法:
    /// 1. このファイルをコピー
    /// 2. [EntityName] を実際のエンティティ名に置換
    /// 3. [ActionName] をアクション名に置換
    /// 4. Success/Error メソッドを適切に実装
    /// </summary>
    public class [ActionName][EntityName]Response : IResponse
    {
        public bool IsSuccess { get; private set; }
        public string? Message { get; private set; }
        public [EntityName]Dto? Data { get; private set; }
        
        private [ActionName][EntityName]Response() { }
        
        /// <summary>
        /// 成功時のレスポンス
        /// </summary>
        public static [ActionName][EntityName]Response Success(
            [EntityName]Dto data,
            string message = "成功")
        {
            return new()
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }
        
        /// <summary>
        /// 失敗時のレスポンス
        /// </summary>
        public static [ActionName][EntityName]Response Error(
            string message)
        {
            return new()
            {
                IsSuccess = false,
                Message = message,
                Data = null
            };
        }
        
        /// <summary>
        /// データなし時のレスポンス
        /// </summary>
        public static [ActionName][EntityName]Response NotFound(
            string message = "データが見つかりません")
        {
            return Error(message);
        }
    }
    
    /// <summary>
    /// DTO: Entity を Presentation に返す際のデータ型
    /// </summary>
    public class [EntityName]Dto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        // プロパティをビジネス要件に合わせて追加
    }
}
