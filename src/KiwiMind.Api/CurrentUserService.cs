using System.Security.Claims;
using KiwiMind.Application.Common.Interfaces;

namespace KiwiMind.Api;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

            return sub is not null && Guid.TryParse(sub, out var userId)
                ? userId
                : throw new InvalidOperationException("No authenticated user in the current context.");
        }
    }
}
