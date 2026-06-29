namespace Respondr.Application.Identity;

using Respondr.Domain.Identity;

public interface IJwtTokenService
{
    AuthTokenResult CreateAccessToken(User user, string roleName);

    string CreateRefreshToken();
}
