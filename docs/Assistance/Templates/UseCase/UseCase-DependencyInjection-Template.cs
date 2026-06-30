// ========================================
// UseCase テンプレート：DI登録
// ========================================

namespace SupportAdvance.Application.UseCases.Templates
{
    using Microsoft.Extensions.DependencyInjection;
    
    /// <summary>
    /// テンプレート: UseCase の DI登録
    /// 
    /// 使用方法:
    /// 1. [ContextName].Application 層に DependencyInjection.cs を作成
    /// 2. このコードを参考に、各 UseCase を登録
    /// 3. Presentation層の HostBuilderFactory で呼び出す
    /// 
    /// 例:
    ///   services.AddCarPreferencesApplicationModels();
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// [ContextName] Application層の全UseCase を DI登録
        /// </summary>
        public static IServiceCollection Add[ContextName]ApplicationModels(
            this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            
            // ============================================
            // Get UseCase
            // ============================================
            services.AddScoped<
                IUseCase<Get[EntityName]Request, Get[EntityName]Response>,
                Get[EntityName]UseCase>();
            
            // ============================================
            // Create UseCase
            // ============================================
            services.AddScoped<
                IUseCase<Create[EntityName]Request, Create[EntityName]Response>,
                Create[EntityName]UseCase>();
            
            // ============================================
            // Update UseCase
            // ============================================
            services.AddScoped<
                IUseCase<Update[EntityName]Request, Update[EntityName]Response>,
                Update[EntityName]UseCase>();
            
            // ============================================
            // Delete UseCase
            // ============================================
            services.AddScoped<
                IUseCase<Delete[EntityName]Request, Delete[EntityName]Response>,
                Delete[EntityName]UseCase>();
            
            // ============================================
            // List UseCase
            // ============================================
            services.AddScoped<
                IUseCase<List[EntityName]Request, List[EntityName]Response>,
                List[EntityName]UseCase>();
            
            return services;
        }
    }
    
    /// <summary>
    /// Presentation層での呼び出し例
    /// (Presentation.Shared/HostBuilderFactory.cs 内)
    /// 
    /// 例:
    /// 
    /// public static IHostBuilder CreateHostBuilder()
    /// {
    ///     return Host.CreateDefaultBuilder()
    ///         .ConfigureServices((context, services) =>
    ///         {
    ///             // Application層 DI登録
    ///             services.AddCarPreferencesApplicationModels();
    ///             
    ///             // Infrastructure層 DI登録
    ///             services.AddCarPreferencesInfrastructureModels();
    ///         });
    /// }
    /// </summary>
}
