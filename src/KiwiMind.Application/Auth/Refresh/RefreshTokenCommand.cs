using MediatR;

namespace KiwiMind.Application.Auth.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResult>;
