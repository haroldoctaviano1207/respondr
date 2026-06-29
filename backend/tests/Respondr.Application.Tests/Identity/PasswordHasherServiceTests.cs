namespace Respondr.Application.Tests.Identity;

using Respondr.Application.Identity;
using Respondr.Infrastructure.Identity;

public sealed class PasswordHasherServiceTests
{
    private readonly IPasswordHasherService _passwordHasher = new PasswordHasherService();

    [Fact]
    public void Hash_password_verifies_with_original_value()
    {
        var hashedPassword = _passwordHasher.HashPassword("Dispatcher123!");

        Assert.True(_passwordHasher.VerifyPassword(hashedPassword, "Dispatcher123!"));
    }

    [Fact]
    public void Hash_password_fails_with_invalid_value()
    {
        var hashedPassword = _passwordHasher.HashPassword("Dispatcher123!");

        Assert.False(_passwordHasher.VerifyPassword(hashedPassword, "WrongPassword123!"));
    }
}
