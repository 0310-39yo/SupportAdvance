using Moq;
using SupportAdvance.Application.Queries;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.Shared.ViewModels;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Tests.Presentation.Shared.Tests.ViewModels;

/// <summary>
/// <see cref="Form1ViewModel"/> の BizId 検索の各分岐の検証
/// </summary>
public class Form1ViewModelTests
{
    private readonly Mock<GetEmployeeByBizIdIntegrationUseCase> _useCase = new();

    private Form1ViewModel CreateSut()
    {
        return new Form1ViewModel(
            Mock.Of<IAppLogging<Form1ViewModel>>(),
            Mock.Of<IClock>(),
            _useCase.Object);
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
        _useCase.Setup(u => u.ExecuteAsync(100)).ReturnsAsync(CreateEmployee());
        var sut = CreateSut();
        sut.BizIdSearchInput = "100";

        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        Assert.Equal("山田 太郎", sut.EmployeeFullName);
        Assert.Equal("営業部", sut.DepartmentNames);
    }

    [Fact]
    public async Task Search_EmployeeNotFound_ClearsPreviousDepartmentNames()
    {
        _useCase.Setup(u => u.ExecuteAsync(100)).ReturnsAsync(CreateEmployee());
        _useCase.Setup(u => u.ExecuteAsync(200)).ReturnsAsync((IEmployeeQueryResult?)null);
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
        _useCase.Setup(u => u.ExecuteAsync(100)).ReturnsAsync(CreateEmployee());
        var sut = CreateSut();
        sut.BizIdSearchInput = "100";
        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        sut.BizIdSearchInput = input;
        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        Assert.Equal(string.Empty, sut.EmployeeFullName);
        Assert.Equal(string.Empty, sut.DepartmentNames);
        _useCase.Verify(u => u.ExecuteAsync(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task Search_NonNumericInput_ShowsErrorWithoutQuery()
    {
        var sut = CreateSut();
        sut.BizIdSearchInput = "abc";

        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        Assert.StartsWith("入力エラー", sut.EmployeeFullName);
        _useCase.Verify(u => u.ExecuteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Search_QueryThrows_ShowsErrorAndClearsDepartmentNames()
    {
        _useCase.Setup(u => u.ExecuteAsync(100)).ThrowsAsync(new InvalidOperationException("boom"));
        var sut = CreateSut();
        sut.BizIdSearchInput = "100";

        await sut.SearchEmployeeByBizIdCommand.ExecuteAsync(null);

        Assert.Equal("エラーが発生しました", sut.EmployeeFullName);
        Assert.Equal(string.Empty, sut.DepartmentNames);
    }
}
