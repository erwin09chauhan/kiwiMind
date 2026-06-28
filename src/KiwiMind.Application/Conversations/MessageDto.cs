using KiwiMind.Domain.Entities;
using KiwiMind.Domain.Enums;

namespace KiwiMind.Application.Conversations;

public record MessageDto(Guid Id, MessageRole Role, string Content, List<Citation> Citations, int TokensUsed, DateTimeOffset CreatedAt);
