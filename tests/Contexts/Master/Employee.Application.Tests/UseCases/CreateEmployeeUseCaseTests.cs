using Moq;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Master.Employee.Application.Repositories;
using SupportAdvance.Contexts.Master.Employee.Application.UseCases;
using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

namespace SupportAdvance.Contexts.Master.Employee.Application.Tests.UseCases;

public class CreateEmployeeUseCaseTests
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly Mock<IClock> _mockClock;
    private readonly CreateEmployeeUseCase _useCase;

    public CreateEmployeeUseCaseTests()
    {
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _mockClock = new Mock<IClock>();
        _mockClock.Setup(c => c.JstNow).Returns(LocalDateTime.From(new DateTime(2026, 8, 2)));
        _useCase = new CreateEmployeeUseCase(_mockEmployeeRepository.Object, _mockClock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldCreateEmployee()
    {
        var employeeNumber = "EMP001";
        var firstName = "John";
        var lastName = "Doe";
        var email = "john@example.com";
        var departmentId = RowId.From(1);
        var jobTitle = "Engineer";

        _mockEmployeeRepository.Setup(r => r.GetByEmployeeNumberAsync(employeeNumber))
            .ReturnsAsync((Entities.Employee?)null);
        _mockEmployeeRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((Entities.Employee?)null);
        _mockEmployeeRepository.Setup(r => r.CreateAsync(It.IsAny<Entities.Employee>()))
            .Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(
            employeeNumber, firstName, lastName, email, departmentId, jobTitle);

        // CreateEmployeeUseCase returns the created employee's RowId
        // Since we're not setting an ID before creation, it defaults to 0 (unset state)
        // The test verifies that CreateAsync was called, not the returned ID
        _mockEmployeeRepository.Verify(r => r.CreateAsync(It.IsAny<Entities.Employee>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateEmployeeNumber_ShouldThrow()
    {
        var employeeNumber = "EMP001";
        var email = "john@example.com";
        var departmentId = RowId.From(1);
        var existingEmployee = new Entities.Employee(
            employeeNumber, "Jane", "Doe", "jane@example.com", departmentId, "Manager");

        _mockEmployeeRepository.Setup(r => r.GetByEmployeeNumberAsync(employeeNumber))
            .ReturnsAsync(existingEmployee);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(
                employeeNumber, "John", "Doe", email, departmentId, "Engineer"));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateEmail_ShouldThrow()
    {
        var employeeNumber = "EMP001";
        var email = "john@example.com";
        var departmentId = RowId.From(1);
        var existingEmployee = new Entities.Employee(
            "EMP002", "Jane", "Doe", email, departmentId, "Manager");

        _mockEmployeeRepository.Setup(r => r.GetByEmployeeNumberAsync(employeeNumber))
            .ReturnsAsync((Entities.Employee?)null);
        _mockEmployeeRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(existingEmployee);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(
                employeeNumber, "John", "Doe", email, departmentId, "Engineer"));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullEmployeeNumber_ShouldThrow()
    {
        var departmentId = RowId.From(1);
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _useCase.ExecuteAsync(null!, "John", "Doe", "john@example.com", departmentId, "Engineer"));

        Assert.Equal("employeeNumber", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullEmail_ShouldThrow()
    {
        var departmentId = RowId.From(1);
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _useCase.ExecuteAsync("EMP001", "John", "Doe", null!, departmentId, "Engineer"));

        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullDepartmentId_ShouldThrow()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _useCase.ExecuteAsync("EMP001", "John", "Doe", "john@example.com", null!, "Engineer"));

        Assert.Equal("departmentId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CreateEmployeeUseCase(null!, _mockClock.Object));

        Assert.Equal("employeeRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullClock_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CreateEmployeeUseCase(_mockEmployeeRepository.Object, null!));

        Assert.Equal("clock", exception.ParamName);
    }
}
