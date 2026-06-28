using MediatR;

namespace KiwiMind.Application.Conversations.Get;

public record GetConversationByIdQuery(Guid KnowledgeBaseId, Guid ConversationId) : IRequest<ConversationDetailDto>;
