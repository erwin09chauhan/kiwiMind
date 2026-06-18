using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Common.Settings;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KiwiMind.Application.Auth.Refresh;

public class RefreshTokenCommandHandler(
    IApplicationDbContext db,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings) : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await db.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            throw new InvalidRefreshTokenException();
        }

        existingToken.RevokedAt = DateTimeOffset.UtcNow;

        var (accessToken, accessTokenExpiresAt) = tokenService.GenerateAccessToken(existingToken.User);
        var newRefreshToken = new RefreshToken
        {
            UserId = existingToken.UserId,
            Token = tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(jwtSettings.Value.RefreshTokenDays)
        };
        db.RefreshTokens.Add(newRefreshToken);

        await db.SaveChangesAsync(cancellationToken);

        return new AuthResult(accessToken, accessTokenExpiresAt, newRefreshToken.Token);
    }
}
