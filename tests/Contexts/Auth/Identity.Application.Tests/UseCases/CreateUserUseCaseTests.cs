using Moq;
using SupportAdvance.Contexts.Auth.Identity.Application.Repositories;
using SupportAdvance.Contexts.Auth.Identity.Application.UseCases;
using SupportAdvance.Contexts.Auth.Identity.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using Xunit;

namespace SupportAdvance.Contexts.Auth.Identity.Application.Tests.UseCases;

public class CreateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly CreateUserUseCase _useCase;

    public CreateUserUseCaseTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _useCase = new CreateUserUseCase(_mockUserRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldCreateUser()
    {
        var loginId = "newuser";
        var email = "newuser@example.com";
        var hashedPassword = "hashed_password";
        var displayName = "New User";

        _mockUserRepository.Setup(r => r.GetByLoginIdAsync(loginId))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(loginId, email, hashedPassword, displayName);

        // CreateUserUseCase returns the created user's RowId
        // Since we're not setting an ID before creation, it defaults to 0 (unset state)
        // The test verifies that CreateAsync was called, not the returned ID
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateLoginId_ShouldThrow()
    {
        var loginId = "existinguser";
        var email = "new@example.com";
        var existingUser = new User(loginId, "existing@example.com", "pwd");

        _mockUserRepository.Setup(r => r.GetByLoginIdAsync(loginId))
            .ReturnsAsync(existingUser);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(loginId, email, "hashed_password"));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateEmail_ShouldThrow()
    {
        var loginId = "newuser";
        var email = "existing@example.com";
        var existingUser = new User("existinguser", email, "pwd");

        _mockUserRepository.Setup(r => r.GetByLoginIdAsync(loginId))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(loginId, email, "hashed_password"));

        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullLoginId_ShouldThrow()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _useCase.ExecuteAsync(null!, "test@example.com", "password"));

        Assert.Equal("loginId", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullEmail_ShouldThrow()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _useCase.ExecuteAsync("testuser", null!, "password"));

        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullPassword_ShouldThrow()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _useCase.ExecuteAsync("testuser", "test@example.com", null!));

        Assert.Equal("hashedPassword", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new CreateUserUseCase(null!));

        Assert.Equal("userRepository", exception.ParamName);
    }
}
