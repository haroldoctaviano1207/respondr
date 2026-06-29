namespace Respondr.Shared.Security;

using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
