namespace KiwiMind.Application.Auth;

public record AuthResult(string AccessToken, DateTimeOffset AccessTokenExpiresAt, string RefreshToken);
