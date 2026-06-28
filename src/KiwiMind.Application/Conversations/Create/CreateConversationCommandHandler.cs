using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.Conversations.Create;

public class CreateConversationCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var knowledgeBaseExists = await db.KnowledgeBases
            .AnyAsync(kb => kb.Id == request.KnowledgeBaseId && kb.UserId == currentUser.UserId, cancellationToken);
        if (!knowledgeBaseExists)
        {
            throw new NotFoundException(nameof(KnowledgeBase), request.KnowledgeBaseId);
        }

        var conversation = new Conversation
        {
            UserId = currentUser.UserId,
            KnowledgeBaseId = request.KnowledgeBaseId,
            Title = request.Title.Trim()
        };

        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(cancellationToken);

        return new ConversationDto(conversation.Id, conversation.KnowledgeBaseId, conversation.Title, conversation.CreatedAt, MessageCount: 0);
    }
}
