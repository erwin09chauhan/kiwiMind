using MediatR;

namespace KiwiMind.Application.Conversations.List;

public record GetConversationsQuery(Guid KnowledgeBaseId) : IRequest<List<ConversationDto>>;
