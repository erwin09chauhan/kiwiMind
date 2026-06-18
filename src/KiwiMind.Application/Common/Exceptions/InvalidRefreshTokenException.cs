namespace KiwiMind.Application.Common.Exceptions;

public class InvalidRefreshTokenException()
    : Exception("Refresh token is invalid, expired, or revoked.");
