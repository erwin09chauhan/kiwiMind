using MediatR;

namespace KiwiMind.Application.Conversations.Create;

public record CreateConversationCommand(Guid KnowledgeBaseId, string Title) : IRequest<ConversationDto>;
