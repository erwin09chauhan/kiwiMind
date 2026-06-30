using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Retrieval;
using KiwiMind.Domain.Entities;
using KiwiMind.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.Conversations.SendMessage;

public class SendMessageCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    ISender sender,
    IChatCompletionService chatCompletionService) : IRequestHandler<SendMessageCommand, MessageDto>
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

        var context = await sender.Send(
            new SearchKnowledgeBaseQuery(request.KnowledgeBaseId, request.Content, TopK: 5), cancellationToken);

        var answer = await chatCompletionService.GenerateAnswerAsync(context, history, request.Content, cancellationToken);

        var citations = context
            .Select(c => new Citation { DocumentId = c.DocumentId, ChunkId = c.ChunkId, Page = c.Page })
            .ToList();

        var assistantMessage = new Message
        {
            ConversationId = request.ConversationId,
            Role = MessageRole.Assistant,
            Content = answer,
            Citations = citations,
            TokensUsed = answer.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
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
