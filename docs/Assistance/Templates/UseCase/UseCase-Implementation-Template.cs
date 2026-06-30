// ========================================
// UseCase テンプレート：実装
// ========================================

namespace SupportAdvance.Application.UseCases.Templates
{
    using Microsoft.Extensions.Logging;
    using SupportAdvance.Application.UseCases;
    using System.Threading.Tasks;
    
    /// <summary>
    /// テンプレート: UseCase の実装
    /// 
    /// 使用方法:
    /// 1. このファイルをコピー
    /// 2. [ActionName] をアクション名に置換
    /// 3. [EntityName] をエンティティ名に置換
    /// 4. Execute メソッドのビジネスロジックを実装
    /// 
    /// パターン:
    /// - Get: 取得
    /// - Create: 作成
    /// - Update: 更新
    /// - Delete: 削除
    /// - List: 一覧
    /// </summary>
    public class [ActionName][EntityName]UseCase 
        : IUseCase<[ActionName][EntityName]Request, [ActionName][EntityName]Response>
    {
        // ステップ1: 依存性注入（DI）
        private readonly I[EntityName]Repository _repository;
        private readonly ILogger<[ActionName][EntityName]UseCase> _logger;
        
        public [ActionName][EntityName]UseCase(
            I[EntityName]Repository repository,
            ILogger<[ActionName][EntityName]UseCase> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// UseCase 実行メソッド
        /// </summary>
        public async Task<[ActionName][EntityName]Response> Execute(
            [ActionName][EntityName]Request request)
        {
            try
            {
                // ステップ2: 入力検証
                ValidateRequest(request);
                
                // ステップ3: 既存データ取得（必要に応じて）
                // 例: Get, Update, Delete の場合
                var entity = await _repository.GetById(request.Id);
                if (entity == null)
                {
                    _logger.LogWarning($"[Entity] not found: ID={request.Id}");
                    return [ActionName][EntityName]Response.NotFound();
                }
                
                // ステップ4: ビジネスロジック実行
                // Domain層のメソッドを呼び出す
                // 例: entity.Update(request.Parameter);
                
                // ステップ5: 永続化
                // 例: await _repository.Save(entity);
                
                // ステップ6: ログ出力
                _logger.LogInformation(
                    $"[ActionName] [EntityName]: ID={request.Id}");
                
                // ステップ7: 結果を Response に変換して返却
                return [ActionName][EntityName]Response.Success(
                    new [EntityName]Dto
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        // 他のプロパティをマッピング
                    },
                    message: "[ActionName] が完了しました");
            }
            catch (DomainException ex)
            {
                // ビジネスルール違反
                _logger.LogWarning($"Business rule violation: {ex.Message}");
                return [ActionName][EntityName]Response.Error(ex.Message);
            }
            catch (ArgumentException ex)
            {
                // 入力エラー
                _logger.LogWarning($"Invalid input: {ex.Message}");
                return [ActionName][EntityName]Response.Error(ex.Message);
            }
            catch (Exception ex)
            {
                // 予期しないエラー
                _logger.LogError($"Unexpected error: {ex}");
                return [ActionName][EntityName]Response.Error(
                    "予期しないエラーが発生しました");
            }
        }
        
        /// <summary>
        /// 入力検証ロジック
        /// </summary>
        private void ValidateRequest([ActionName][EntityName]Request request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            
            if (request.Id <= 0)
                throw new ArgumentException(
                    "ID は正の数である必要があります",
                    nameof(request.Id));
            
            // 他の検証ルールをここに追加
        }
    }
    
    /// <summary>
    /// Repository インターフェース
    /// (Application層で定義、Infrastructure層で実装)
    /// </summary>
    public interface I[EntityName]Repository
    {
        Task<[EntityName]> GetById(int id);
        Task<IEnumerable<[EntityName]>> GetAll();
        Task Save([EntityName] entity);
        Task Delete([EntityName] entity);
    }
}
