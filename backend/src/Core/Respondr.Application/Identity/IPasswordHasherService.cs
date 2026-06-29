namespace Respondr.Application.Identity;

public interface IPasswordHasherService
{
    string HashPassword(string password);

    bool VerifyPassword(string hashedPassword, string providedPassword);
}
