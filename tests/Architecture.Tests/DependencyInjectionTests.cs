using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SupportAdvance.Application;
using SupportAdvance.Application.Abstractions.Services;
using SupportAdvance.Application.Queries;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.Authentication.Application;
using SupportAdvance.Contexts.Authentication.Infrastructure;
using SupportAdvance.Contexts.Department.Application;
using SupportAdvance.Contexts.Department.Infrastructure;
using SupportAdvance.Contexts.Employee.Application;
using SupportAdvance.Contexts.Employee.Infrastructure;
using SupportAdvance.Contexts.IntegrationPrototype.Application;
using SupportAdvance.Crosscutting;
using SupportAdvance.Infrastructure;
using SupportAdvance.Presentation.Shared.Services;
using SupportAdvance.Presentation.WinTrial;
using SupportAdvance.Presentation.WpfTrial;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

namespace SupportAdvance.Tests.Architecture.Tests;

/// <summary>
/// DI（依存性注入）の登録漏れ・ライフタイム不整合の検証
/// </summary>
/// <remarks>
/// <para>【検証内容】Composition Root（WpfTrial の App、WinTrial の Program）と同じ登録の並びで、コンテナを構築できること</para>
/// <para>【検証内容】構築時に <c>ValidateOnBuild</c>（コンストラクタ引数の未登録）と <c>ValidateScopes</c>（Singleton が Scoped を保持する不整合）を有効にする</para>
/// <para>【検証内容】登録済みの実装型（画面を除く）が、実際にスコープから解決できること</para>
/// <para>【設計】DB へは接続しない（接続文字列は形式だけのダミー）。画面（Window／Form）は UI スレッドが必要なため解決の対象外。ただし構築時の検証には含まれる</para>
/// <para>【注意】Composition Root の登録を変更した場合は、<see cref="AddCompositionRoot"/> も同じ並びに更新する</para>
/// </remarks>
public class DependencyInjectionTests
{
    /// <summary>
    /// 検証対象のアプリケーション（Composition Root）の一覧
    /// </summary>
    public static TheoryData<string> Applications => new() { "WpfTrial", "WinTrial" };

    /// <summary>
    /// Composition Root と同じ登録の並びで <see cref="IServiceCollection"/> を構成する
    /// </summary>
    private static IServiceCollection AddCompositionRoot(string application)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:ClockSettings:ClockType"] = "SYSTEM",
                ["AppSettings:ConnectionStrings:Default"] = "Server=localhost;Database=DiTest;Integrated Security=true;"
            })
            .Build();

        // HostBuilderFactory と同じ: AppSettings を IAppSettings として Singleton 登録
        var appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()
                          ?? throw new InvalidOperationException("AppSettings section not found");

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddSingleton<IAppSettings>(appSettings);

        services
            .AddCrosscuttingModels(configuration)
            .AddInfrastructureModels(configuration)
            .AddEmployeeInfrastructureModels()
            .AddDepartmentInfrastructureModels()
            .AddAuthenticationInfrastructureModels()
            .AddApplicationModels()
            .AddEmployeeApplicationModels()
            .AddDepartmentApplicationModels()
            .AddAuthenticationApplicationModels()
            .AddIntegrationPrototypeApplicationModels();

        if (application == "WpfTrial")
        {
            services.AddWpfTrialModules();
        }
        else
        {
            services.AddWinTrialModules();
        }

        // ログイン情報をスコープをまたいで保持するため Singleton（Composition Root と同じ）
        services.AddSingleton<ICurrentUserService, RealCurrentUserService>();

        return services;
    }

    private static ServiceProvider BuildProvider(string application)
        => AddCompositionRoot(application).BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

    /// <summary>
    /// 画面（WPF の Window／WinForms の Form）かどうかの判定
    /// </summary>
    private static bool IsView(Type type)
        => type.Namespace?.EndsWith(".Views", StringComparison.Ordinal) == true;

    [Theory]
    [MemberData(nameof(Applications))]
    public void Container_Builds_WithoutMissingDependenciesOrScopeViolations(string application)
    {
        // ValidateOnBuild: 未登録の依存があれば例外。ValidateScopes: Singleton→Scoped の保持があれば例外
        using var provider = BuildProvider(application);

        Assert.NotNull(provider);
    }

    [Theory]
    [MemberData(nameof(Applications))]
    public void RegisteredImplementations_ExceptViews_AreResolvableFromScope(string application)
    {
        var services = AddCompositionRoot(application);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        var targets = services
            .Where(d => d.ServiceType is { IsGenericTypeDefinition: false })
            .Where(d => d.ImplementationType is { } impl
                        && impl.Namespace?.StartsWith("SupportAdvance", StringComparison.Ordinal) == true
                        && !IsView(impl))
            .Select(d => d.ServiceType)
            .Distinct()
            .ToList();

        // 対象が空で常に成功する状態を防ぐ
        Assert.NotEmpty(targets);

        using var scope = provider.CreateScope();
        foreach (var serviceType in targets)
        {
            var instance = scope.ServiceProvider.GetService(serviceType);
            Assert.True(instance != null, $"{serviceType.FullName} を解決できません");
        }
    }

    [Theory]
    [MemberData(nameof(Applications))]
    public void Views_AreRegistered(string application)
    {
        var services = AddCompositionRoot(application);

        var views = services.Where(d => d.ImplementationType is { } impl && IsView(impl)).ToList();

        Assert.NotEmpty(views);
    }

    [Theory]
    [MemberData(nameof(Applications))]
    public void EmployeeQueryService_AllRegisteredTypes_ShareTheSameInstanceWithinScope(string application)
    {
        using var provider = BuildProvider(application);
        using var scope = provider.CreateScope();

        var withBizId = ServiceProviderServiceExtensions.GetRequiredService<IQueryServiceWithBizId<IEmployee, EmployeeRowId>>(scope.ServiceProvider);
        var generic = ServiceProviderServiceExtensions.GetRequiredService<IQueryService<IEmployee, EmployeeRowId>>(scope.ServiceProvider);

        Assert.Same(withBizId, generic);
    }

    [Theory]
    [MemberData(nameof(Applications))]
    public void CurrentUserService_IsSharedAcrossScopes(string application)
    {
        // ログイン画面（Scope A）で設定した情報を、終了時（Scope B）のログアウト処理から参照するため
        using var provider = BuildProvider(application);
        using var scopeA = provider.CreateScope();
        using var scopeB = provider.CreateScope();

        var a = ServiceProviderServiceExtensions.GetRequiredService<ICurrentUserService>(scopeA.ServiceProvider);
        var b = ServiceProviderServiceExtensions.GetRequiredService<ICurrentUserService>(scopeB.ServiceProvider);

        Assert.Same(a, b);
    }

    [Theory]
    [MemberData(nameof(Applications))]
    public void Clock_IsSingleton(string application)
    {
        using var provider = BuildProvider(application);
        using var scopeA = provider.CreateScope();
        using var scopeB = provider.CreateScope();

        Assert.Same(
            ServiceProviderServiceExtensions.GetRequiredService<IClock>(scopeA.ServiceProvider),
            ServiceProviderServiceExtensions.GetRequiredService<IClock>(scopeB.ServiceProvider));
    }
}
