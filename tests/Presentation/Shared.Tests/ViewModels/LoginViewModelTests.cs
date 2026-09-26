using Moq;
using SupportAdvance.Application.Abstractions.Identifiers;
using SupportAdvance.Application.Abstractions.Services;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Authentication.Application.Queries;
using SupportAdvance.Contexts.Authentication.Application.Repositories;
using SupportAdvance.Contexts.Authentication.Application.Services;
using SupportAdvance.Contexts.Authentication.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.Shared.ViewModels;

namespace SupportAdvance.Tests.Presentation.Shared.Tests.ViewModels;

/// <summary>
/// <see cref="LoginViewModel"/> の入力検証・成功・失敗の各分岐の検証
/// </summary>
public class LoginViewModelTests
{
    private readonly Mock<ILoginCredentialsQuery> _credentialsQuery = new();
    private readonly Mock<IPasswordHashService> _passwordHashService = new();
    private readonly Mock<IUserAuthSessionRepository> _sessionRepository = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();

    private LoginViewModel CreateSut()
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(c => c.JstNow).Returns(new LocalDateTime(new DateTime(2026, 9, 26, 9, 0, 0)));

        var sequence = new Mock<ISequenceProvider>();
        sequence.Setup(s => s.GetNextValueAsync()).ReturnsAsync(1000L);

        var useCase = new AuthenticateLocalUserUseCase(
            _credentialsQuery.Object,
            _passwordHashService.Object,
            _sessionRepository.Object,
            clock.Object,
            sequence.Object);

        return new LoginViewModel(
            useCase,
            _currentUserService.Object,
            Mock.Of<IAppLogging<LoginViewModel>>());
    }

    private void SetupActiveCredentials(bool passwordMatches)
    {
        _credentialsQuery.Setup(q => q.GetByLoginIdAsync("user01")).ReturnsAsync(new LoginCredentialsQueryResult
        {
            RowId = 10,
            MappingEmployeeRowId = 20,
            LoginId = "user01",
            PasswordHash = "hash",
            IsActive = true
        });
        _passwordHashService.Setup(p => p.VerifyPassword(It.IsAny<string>(), "hash")).Returns(passwordMatches);
    }

    [Theory]
    [InlineData("", "pass")]
    [InlineData("user01", "")]
    [InlineData("  ", "  ")]
    public async Task Login_EmptyInput_ShowsMessageWithoutAuthenticating(string loginId, string password)
    {
        var sut = CreateSut();
        sut.LoginId = loginId;
        sut.Password = password;

        await sut.LoginCommand.ExecuteAsync(null);

        Assert.False(string.IsNullOrEmpty(sut.ErrorMessage));
        _credentialsQuery.Verify(q => q.GetByLoginIdAsync(It.IsAny<string>()), Times.Never);
        _currentUserService.Verify(c => c.SetLoggedInUser(It.IsAny<long>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_Success_SetsCurrentUserAndRaisesLoginSucceeded()
    {
        SetupActiveCredentials(passwordMatches: true);
        var sut = CreateSut();
        var succeeded = 0;
        sut.LoginSucceeded += (_, _) => succeeded++;
        sut.LoginId = " user01 ";
        sut.Password = "secret";

        await sut.LoginCommand.ExecuteAsync(null);

        Assert.Equal(1, succeeded);
        Assert.Equal(string.Empty, sut.ErrorMessage);
        Assert.False(sut.IsLoading);
        _currentUserService.Verify(c => c.SetLoggedInUser(20, "user01"), Times.Once);
    }

    [Fact]
    public async Task Login_WrongPassword_ShowsMessageAndClearsPassword()
    {
        SetupActiveCredentials(passwordMatches: false);
        var sut = CreateSut();
        var succeeded = 0;
        sut.LoginSucceeded += (_, _) => succeeded++;
        sut.LoginId = "user01";
        sut.Password = "wrong";

        await sut.LoginCommand.ExecuteAsync(null);

        Assert.Equal(0, succeeded);
        Assert.Equal("パスワードが間違っています", sut.ErrorMessage);
        Assert.Equal(string.Empty, sut.Password);
        Assert.False(sut.IsLoading);
        _currentUserService.Verify(c => c.SetLoggedInUser(It.IsAny<long>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_UnexpectedException_ShowsFixedMessageWithoutLeakingDetails()
    {
        _credentialsQuery.Setup(q => q.GetByLoginIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new TimeoutException("db connection string=secret"));
        var sut = CreateSut();
        sut.LoginId = "user01";
        sut.Password = "secret";

        await sut.LoginCommand.ExecuteAsync(null);

        Assert.DoesNotContain("secret", sut.ErrorMessage);
        Assert.False(string.IsNullOrEmpty(sut.ErrorMessage));
        Assert.False(sut.IsLoading);
    }

    [Fact]
    public void CancelCommand_RaisesCancelRequested()
    {
        var sut = CreateSut();
        var cancelled = 0;
        sut.CancelRequested += (_, _) => cancelled++;

        sut.CancelCommand.Execute(null);

        Assert.Equal(1, cancelled);
    }
}
