using MediatR;

namespace KiwiMind.Application.Conversations.SendMessage;

public record SendMessageCommand(Guid KnowledgeBaseId, Guid ConversationId, string Content) : IRequest<MessageDto>;
