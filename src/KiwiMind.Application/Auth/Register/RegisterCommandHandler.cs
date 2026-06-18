using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Common.Settings;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KiwiMind.Application.Auth.Register;

public class RegisterCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings) : IRequestHandler<RegisterCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await db.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (emailExists)
        {
            throw new EmailAlreadyInUseException(email);
        }

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password)
        };
        db.Users.Add(user);

        var (accessToken, accessTokenExpiresAt) = tokenService.GenerateAccessToken(user);
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            User = user,
            Token = tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(jwtSettings.Value.RefreshTokenDays)
        };
        db.RefreshTokens.Add(refreshToken);

        await db.SaveChangesAsync(cancellationToken);

        return new AuthResult(accessToken, accessTokenExpiresAt, refreshToken.Token);
    }
}
