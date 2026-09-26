using Moq;
using SupportAdvance.Application.Queries;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.Shared.ViewModels;
using SupportAdvance.Presentation.WpfTrial.ViewModels;

namespace SupportAdvance.Tests.Presentation.WpfTrial.Tests.ViewModels;

/// <summary>
/// <see cref="Form1ViewModel"/> の BizId 検索の各分岐の検証
/// </summary>
public class Form1ViewModelTests
{
    private readonly Mock<IEmployeeQueryService> _queryService = new();

    private Form1ViewModel CreateSut()
    {
        var useCase = new GetEmployeeByBizIdIntegrationUseCase(
            _queryService.Object,
            Mock.Of<IAppLogging<GetEmployeeByBizIdIntegrationUseCase>>());

        return new Form1ViewModel(
            Mock.Of<IAppLogging<Form1ViewModel>>(),
            Mock.Of<IClock>(),
            useCase,
            new BusinessDayClockViewModel());
    }

    private static IEmployeeQueryResult CreateEmployee()
    {
        var employee = new Mock<IEmployeeQueryResult>();
        employee.SetupGet(e => e.PersonLastName).Returns("山田");
        employee.SetupGet(e => e.PersonFirstName).Returns("太郎");
        employee.SetupGet(e => e.DepartmentNames).Returns("営業部");
        return employee.Object;
    }

    [Fact]
    public async Task Search_EmployeeFound_SetsFullNameAndDepartmentNames()
    {
        _queryService.Setup(q => q.GetByBizIdAsync(100)).ReturnsAsync(CreateEmployee());
        var sut = CreateSut();
        sut.BizIdSearchInput = "100";

        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        Assert.Equal("山田 太郎", sut.EmployeeFullName);
        Assert.Equal("営業部", sut.DepartmentNames);
    }

    [Fact]
    public async Task Search_EmployeeNotFound_ClearsPreviousDepartmentNames()
    {
        _queryService.Setup(q => q.GetByBizIdAsync(100)).ReturnsAsync(CreateEmployee());
        _queryService.Setup(q => q.GetByBizIdAsync(200)).ReturnsAsync((IEmployeeQueryResult?)null);
        var sut = CreateSut();
        sut.BizIdSearchInput = "100";
        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        sut.BizIdSearchInput = "200";
        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        Assert.Equal("従業員が見つかりません", sut.EmployeeFullName);
        Assert.Equal(string.Empty, sut.DepartmentNames);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Search_EmptyInput_ClearsResults(string input)
    {
        _queryService.Setup(q => q.GetByBizIdAsync(100)).ReturnsAsync(CreateEmployee());
        var sut = CreateSut();
        sut.BizIdSearchInput = "100";
        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        sut.BizIdSearchInput = input;
        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        Assert.Equal(string.Empty, sut.EmployeeFullName);
        Assert.Equal(string.Empty, sut.DepartmentNames);
        _queryService.Verify(q => q.GetByBizIdAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task Search_NonNumericInput_ShowsErrorWithoutQuery()
    {
        var sut = CreateSut();
        sut.BizIdSearchInput = "abc";

        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        Assert.StartsWith("入力エラー", sut.EmployeeFullName);
        _queryService.Verify(q => q.GetByBizIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Search_QueryThrows_ShowsErrorAndClearsDepartmentNames()
    {
        _queryService.Setup(q => q.GetByBizIdAsync(100)).ThrowsAsync(new InvalidOperationException("boom"));
        var sut = CreateSut();
        sut.BizIdSearchInput = "100";

        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        Assert.Equal("エラーが発生しました", sut.EmployeeFullName);
        Assert.Equal(string.Empty, sut.DepartmentNames);
    }
}
