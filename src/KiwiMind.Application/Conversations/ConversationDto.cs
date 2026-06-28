namespace KiwiMind.Application.Conversations;

public record ConversationDto(Guid Id, Guid KnowledgeBaseId, string Title, DateTimeOffset CreatedAt, int MessageCount);
