using MediatR;

namespace KiwiMind.Application.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
