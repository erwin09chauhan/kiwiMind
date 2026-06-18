using MediatR;

namespace KiwiMind.Application.Auth.Register;

public record RegisterCommand(string Email, string Password) : IRequest<AuthResult>;
