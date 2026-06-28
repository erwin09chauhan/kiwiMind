using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.Conversations.Get;

public class GetConversationByIdQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<GetConversationByIdQuery, ConversationDetailDto>
{
    public async Task<ConversationDetailDto> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await db.Conversations
            .Where(c => c.Id == request.ConversationId
                && c.KnowledgeBaseId == request.KnowledgeBaseId
                && c.UserId == currentUser.UserId)
            .Select(c => new ConversationDetailDto(
                c.Id,
                c.KnowledgeBaseId,
                c.Title,
                c.CreatedAt,
                c.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new MessageDto(m.Id, m.Role, m.Content, m.Citations, m.TokensUsed, m.CreatedAt))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);
    }
}
