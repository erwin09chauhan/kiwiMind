namespace KiwiMind.Application.Conversations;

public record ConversationDetailDto(Guid Id, Guid KnowledgeBaseId, string Title, DateTimeOffset CreatedAt, List<MessageDto> Messages);
