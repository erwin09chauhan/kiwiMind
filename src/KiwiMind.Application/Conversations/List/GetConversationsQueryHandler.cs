using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.Conversations.List;

public class GetConversationsQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<GetConversationsQuery, List<ConversationDto>>
{
    public async Task<List<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var knowledgeBaseExists = await db.KnowledgeBases
            .AnyAsync(kb => kb.Id == request.KnowledgeBaseId && kb.UserId == currentUser.UserId, cancellationToken);
        if (!knowledgeBaseExists)
        {
            throw new NotFoundException(nameof(KnowledgeBase), request.KnowledgeBaseId);
        }

        return await db.Conversations
            .Where(c => c.KnowledgeBaseId == request.KnowledgeBaseId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConversationDto(c.Id, c.KnowledgeBaseId, c.Title, c.CreatedAt, c.Messages.Count))
            .ToListAsync(cancellationToken);
    }
}
