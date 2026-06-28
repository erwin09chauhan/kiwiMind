using MediatR;

namespace KiwiMind.Application.Conversations.Delete;

public record DeleteConversationCommand(Guid KnowledgeBaseId, Guid ConversationId) : IRequest;
