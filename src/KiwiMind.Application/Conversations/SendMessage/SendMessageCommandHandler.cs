using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using KiwiMind.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KiwiMind.Application.Conversations.SendMessage;

public class SendMessageCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IChatAgent chatAgent,
    ILogger<SendMessageCommandHandler> logger) : IRequestHandler<SendMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversationExists = await db.Conversations
            .AnyAsync(c => c.Id == request.ConversationId
                && c.KnowledgeBaseId == request.KnowledgeBaseId
                && c.UserId == currentUser.UserId, cancellationToken);
        if (!conversationExists)
        {
            throw new NotFoundException(nameof(Conversation), request.ConversationId);
        }

        var history = await db.Messages
            .Where(m => m.ConversationId == request.ConversationId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessage(m.Role, m.Content))
            .ToListAsync(cancellationToken);

        db.Messages.Add(new Message
        {
            ConversationId = request.ConversationId,
            Role = MessageRole.User,
            Content = request.Content
        });

        var agentResult = await chatAgent.AskAsync(request.KnowledgeBaseId, history, request.Content, cancellationToken);

        foreach (var toolCall in agentResult.ToolCalls)
        {
            logger.LogInformation("Agent called tool {ToolName} with arguments {Arguments}", toolCall.ToolName, toolCall.Arguments);
        }

        var assistantMessage = new Message
        {
            ConversationId = request.ConversationId,
            Role = MessageRole.Assistant,
            Content = agentResult.Answer,
            Citations = agentResult.Citations,
            TokensUsed = agentResult.Answer.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
        };
        db.Messages.Add(assistantMessage);

        await db.SaveChangesAsync(cancellationToken);

        return new MessageDto(
            assistantMessage.Id,
            assistantMessage.Role,
            assistantMessage.Content,
            assistantMessage.Citations,
            assistantMessage.TokensUsed,
            assistantMessage.CreatedAt);
    }
}
