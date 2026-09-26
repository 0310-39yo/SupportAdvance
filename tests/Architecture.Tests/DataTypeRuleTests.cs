using System.Reflection;
using System.Runtime.CompilerServices;
using SupportAdvance.Common.Clocks;
using Xunit;

namespace SupportAdvance.Tests.Architecture.Tests;

/// <summary>
/// データ型に関する原則（DateTime の閉じ込め・Domain の null 排除・Mapper の Clock 非依存）を検証
/// </summary>
/// <remarks>
/// <para>【ルール】</para>
/// <list type="bullet">
/// <item><description>7-1 Application の公開 API に <c>DateTime</c> を持たない</description></item>
/// <item><description>7-2 Infrastructure の Mapper が <c>IClock</c> を保持しない</description></item>
/// <item><description>7-3 Domain の Entity の公開プロパティに nullable を持たない</description></item>
/// <item><description>7-4 Domain と SharedKernel の公開メンバーに <c>DateTime</c> を持たない（例外なし）</description></item>
/// <item><description>7-5 DbModel に <c>LocalDateTime</c> を持たない</description></item>
/// <item><description>7-6 Domain の Entity の公開プロパティに <c>string</c> を持たない（値オブジェクトを使用）</description></item>
/// </list>
/// <para>【方針】リフレクションで対象アセンブリの型を走査する。各ルールは、対象の型が実際に走査されていること（空振りでないこと）と、検出処理が違反を検出できること（自己検証）も確認する</para>
/// <para>【参照】docs/Assistance/Plans/20260926_原則完全準拠_実装計画.md（フェーズ 7）</para>
/// </remarks>
public class DataTypeRuleTests
{
    #region 走査対象のアセンブリ

    private static readonly Assembly[] DomainAssemblies =
    [
        typeof(SupportAdvance.Contexts.Employee.Domain.Entities.Employee).Assembly,
        typeof(SupportAdvance.Contexts.Department.Domain.Entities.Department).Assembly,
        typeof(SupportAdvance.Contexts.Authentication.Domain.Entities.UserAuthSession).Assembly
    ];

    private static readonly Assembly[] ApplicationAssemblies =
    [
        typeof(SupportAdvance.Application.Queries.IQueryService<,>).Assembly,
        typeof(SupportAdvance.Contexts.Employee.Application.Dtos.EmployeeDto).Assembly,
        typeof(SupportAdvance.Contexts.Department.Application.Repositories.IDepartmentRepository).Assembly,
        typeof(SupportAdvance.Contexts.Authentication.Application.Dtos.AuthenticateLocalUserResponse).Assembly
    ];

    private static readonly Assembly[] InfrastructureAssemblies =
    [
        typeof(SupportAdvance.Contexts.Employee.Infrastructure.Mappers.EmployeeMapper).Assembly,
        typeof(SupportAdvance.Contexts.Department.Infrastructure.Mappers.DepartmentMapper).Assembly,
        typeof(SupportAdvance.Contexts.Authentication.Infrastructure.Mappers.UserAuthSessionMapper).Assembly
    ];

    private static readonly Assembly SharedKernelAssembly =
        typeof(SupportAdvance.SharedKernel.ValueObjects.Audit.CreatedAt).Assembly;

    #endregion

    #region ルール

    [Fact]
    public void Application_PublicApi_ShouldNotExposeDateTime()
    {
        var types = ExportedTypes(ApplicationAssemblies);
        Assert.True(types.Count > 20, $"走査対象の型が少なすぎる（空振りの疑い）: {types.Count}");

        var violations = types.SelectMany(FindDateTimeExposure).ToList();

        AssertNoViolations(violations, "Application の公開 API に DateTime がある。LocalDateTime を使用すること");
    }

    [Fact]
    public void DomainAndSharedKernel_PublicApi_ShouldNotExposeDateTime()
    {
        var types = ExportedTypes([.. DomainAssemblies, SharedKernelAssembly]);
        Assert.True(types.Count > 40, $"走査対象の型が少なすぎる（空振りの疑い）: {types.Count}");

        var violations = types.SelectMany(FindDateTimeExposure).ToList();

        AssertNoViolations(
            violations,
            "Domain / SharedKernel の公開メンバーに DateTime がある。DB の型との変換は Infrastructure（Mapper・Repository）の担当");
    }

    [Fact]
    public void Mappers_ShouldNotHoldClock()
    {
        var mappers = InfrastructureTypes().Where(t => t.Name.EndsWith("Mapper", StringComparison.Ordinal)).ToList();
        Assert.True(mappers.Count >= 3, $"走査対象の Mapper が少なすぎる（空振りの疑い）: {mappers.Count}");

        var violations = mappers.SelectMany(FindClockFields).ToList();

        AssertNoViolations(violations, "Mapper が IClock を保持している。復元時に必要な時計は、メソッドの引数で受け取ること");
    }

    [Fact]
    public void DomainEntities_PublicProperties_ShouldNotBeNullable()
    {
        var entities = DomainEntityTypes();
        Assert.True(entities.Count >= 5, $"走査対象の Entity が少なすぎる（空振りの疑い）: {entities.Count}");

        var violations = entities.SelectMany(FindNullableProperties).ToList();

        AssertNoViolations(
            violations,
            "Domain の Entity の公開プロパティが nullable。未設定は null ではなく、値オブジェクトの Unset で表現すること");
    }

    [Fact]
    public void DbModels_ShouldNotHaveLocalDateTime()
    {
        var dbModels = InfrastructureTypes().Where(t => t.Name.EndsWith("DbModel", StringComparison.Ordinal)).ToList();
        Assert.True(dbModels.Count >= 5, $"走査対象の DbModel が少なすぎる（空振りの疑い）: {dbModels.Count}");

        var violations = dbModels.SelectMany(FindLocalDateTimeProperties).ToList();

        AssertNoViolations(violations, "DbModel が LocalDateTime を持っている。DB の型は DateTime（変換は Mapper の担当）");
    }

    [Fact]
    public void DomainEntities_PublicProperties_ShouldNotBeString()
    {
        var entities = DomainEntityTypes();
        Assert.True(entities.Count >= 5, $"走査対象の Entity が少なすぎる（空振りの疑い）: {entities.Count}");

        var violations = entities.SelectMany(FindStringProperties).ToList();

        AssertNoViolations(
            violations,
            "Domain の Entity の公開プロパティが string。名前・コードなどは値オブジェクトを使用すること");
    }

    #endregion

    #region 検出処理の自己検証（違反を実際に検出できることの確認）

    /// <summary>
    /// 違反を含む型（検出処理の確認用）
    /// </summary>
    public sealed class ViolatingSample
    {
        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public Task<List<DateTime>> LoadAsync(DateTimeOffset since) => Task.FromResult(new List<DateTime>());

        public string? NullableName { get; set; }

        public string Name { get; set; } = string.Empty;

        public int? NullableNumber { get; set; }

        public LocalDateTime Local { get; set; }

        public LocalDateTime? LocalOrNull { get; set; }

        public required IClock ClockProperty { get; init; }
    }

    /// <summary>
    /// 違反を含まない型（検出処理の確認用）
    /// </summary>
    public sealed class CompliantSample
    {
        public LocalDateTime At { get; set; }

        public int Count { get; set; }

        public bool Flag { get; set; }

        public byte[] RowVersion { get; set; } = [];

        public Task<LocalDateTime> LoadAsync(LocalDateTime since) => Task.FromResult(since);
    }

    /// <summary>
    /// Clock を保持する型（検出処理の確認用）
    /// </summary>
    public sealed class ClockHoldingSample(IClock clock)
    {
        private readonly IClock _clock = clock;

        public LocalDateTime Now() => _clock.JstNow;
    }

    [Fact]
    public void Detector_FindsDateTimeExposure_InViolatingSample()
    {
        var violations = FindDateTimeExposure(typeof(ViolatingSample)).ToList();

        Assert.Contains(violations, v => v.Contains("CreatedOn"));
        Assert.Contains(violations, v => v.Contains("UpdatedOn"));
        Assert.Contains(violations, v => v.Contains("LoadAsync"));
    }

    [Fact]
    public void Detector_DoesNotFlagCompliantSample()
    {
        Assert.Empty(FindDateTimeExposure(typeof(CompliantSample)));
        Assert.Empty(FindNullableProperties(typeof(CompliantSample)));
        Assert.Empty(FindStringProperties(typeof(CompliantSample)));
        Assert.DoesNotContain(FindLocalDateTimeProperties(typeof(CompliantSample)), v => v.Contains("Count"));
        Assert.Empty(FindClockFields(typeof(CompliantSample)));
    }

    [Fact]
    public void Detector_FindsNullableAndStringProperties_InViolatingSample()
    {
        var nullable = FindNullableProperties(typeof(ViolatingSample)).ToList();
        Assert.Contains(nullable, v => v.Contains("NullableName"));
        Assert.Contains(nullable, v => v.Contains("NullableNumber"));
        Assert.Contains(nullable, v => v.Contains("UpdatedOn"));
        Assert.DoesNotContain(nullable, v => v.Contains("ViolatingSample.Name "));

        var strings = FindStringProperties(typeof(ViolatingSample)).ToList();
        Assert.Contains(strings, v => v.Contains("Name"));
    }

    [Fact]
    public void Detector_FindsLocalDateTimeProperties_InViolatingSample()
    {
        var violations = FindLocalDateTimeProperties(typeof(ViolatingSample)).ToList();

        Assert.Contains(violations, v => v.Contains("Local "));
        Assert.Contains(violations, v => v.Contains("LocalOrNull"));
    }

    [Fact]
    public void Detector_FindsClockField_InClockHoldingSample()
    {
        var violations = FindClockFields(typeof(ClockHoldingSample)).ToList();

        Assert.NotEmpty(violations);
    }

    #endregion

    #region 検出処理

    private static List<Type> ExportedTypes(IEnumerable<Assembly> assemblies) =>
        assemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .ToList();

    private static List<Type> InfrastructureTypes() =>
        InfrastructureAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .ToList();

    private static List<Type> DomainEntityTypes() =>
        DomainAssemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsClass
                        && t.Namespace is not null
                        && t.Namespace.EndsWith(".Domain.Entities", StringComparison.Ordinal)
                        && !t.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .ToList();

    private static void AssertNoViolations(List<string> violations, string message)
    {
        Assert.True(
            violations.Count == 0,
            $"{message}（{violations.Count} 件）:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    private static bool MentionsDateTime(Type type)
    {
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return true;
        }

        if (type.HasElementType)
        {
            return MentionsDateTime(type.GetElementType()!);
        }

        return type.IsGenericType && type.GetGenericArguments().Any(MentionsDateTime);
    }

    private static IEnumerable<string> FindDateTimeExposure(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var p in type.GetProperties(flags).Where(p => MentionsDateTime(p.PropertyType)))
        {
            yield return $"{type.FullName}.{p.Name} (プロパティ: {p.PropertyType.Name})";
        }

        foreach (var f in type.GetFields(flags).Where(f => MentionsDateTime(f.FieldType)))
        {
            yield return $"{type.FullName}.{f.Name} (フィールド: {f.FieldType.Name})";
        }

        foreach (var m in type.GetMethods(flags).Where(m => !m.IsSpecialName))
        {
            if (MentionsDateTime(m.ReturnType))
            {
                yield return $"{type.FullName}.{m.Name} (戻り値: {m.ReturnType.Name})";
            }

            foreach (var parameter in m.GetParameters().Where(p => MentionsDateTime(p.ParameterType)))
            {
                yield return $"{type.FullName}.{m.Name} (引数 {parameter.Name}: {parameter.ParameterType.Name})";
            }
        }

        foreach (var c in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            foreach (var parameter in c.GetParameters().Where(p => MentionsDateTime(p.ParameterType)))
            {
                yield return $"{type.FullName}..ctor (引数 {parameter.Name}: {parameter.ParameterType.Name})";
            }
        }
    }

    private static IEnumerable<string> FindClockFields(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                                   | BindingFlags.Static | BindingFlags.DeclaredOnly;

        return type.GetFields(flags)
            .Where(f => typeof(IClock).IsAssignableFrom(f.FieldType))
            .Select(f => $"{type.FullName}.{f.Name} ({f.FieldType.Name})");
    }

    private static IEnumerable<string> FindNullableProperties(Type type)
    {
        var context = new NullabilityInfoContext();

        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var isNullable = p.PropertyType.IsValueType
                ? Nullable.GetUnderlyingType(p.PropertyType) is not null
                : context.Create(p).ReadState == NullabilityState.Nullable;

            if (isNullable)
            {
                yield return $"{type.FullName}.{p.Name} ({p.PropertyType.Name})";
            }
        }
    }

    private static IEnumerable<string> FindStringProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string))
            .Select(p => $"{type.FullName}.{p.Name} (string)");

    private static IEnumerable<string> FindLocalDateTimeProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(LocalDateTime) || Nullable.GetUnderlyingType(p.PropertyType) == typeof(LocalDateTime))
            .Select(p => $"{type.FullName}.{p.Name} ({p.PropertyType.Name})");

    #endregion
}
