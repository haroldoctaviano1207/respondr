namespace Respondr.Application.Tests.Identity;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Respondr.Application.Identity;
using Respondr.Domain.Identity;
using Respondr.Infrastructure.Identity;
using Respondr.Shared.Constants;
using Respondr.Shared.Time;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public void Access_token_contains_expected_user_and_role_claims()
    {
        var user = new User(
            Guid.NewGuid(),
            "dispatcher@respondr.local",
            "hashed-password",
            "Development Dispatcher",
            Guid.NewGuid());

        IJwtTokenService jwtTokenService = new JwtTokenService(
            Options.Create(new JwtOptions
            {
                Issuer = "respondr.tests",
                Audience = "respondr.clients",
                Key = "Respondr_Test_Key_12345678901234567890",
                AccessTokenMinutes = 60
            }),
            new FixedDateTimeProvider());

        var token = jwtTokenService.CreateAccessToken(user, RoleNames.Dispatcher);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);

        Assert.Equal("respondr.tests", jwt.Issuer);
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Email && claim.Value == user.Email);
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == RoleNames.Dispatcher);
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == user.Id.ToString());
    }

    private sealed class FixedDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => new(2026, 6, 28, 0, 0, 0, TimeSpan.Zero);
    }
}
