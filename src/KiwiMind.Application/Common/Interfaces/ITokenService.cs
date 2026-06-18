using KiwiMind.Domain.Entities;

namespace KiwiMind.Application.Common.Interfaces;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
