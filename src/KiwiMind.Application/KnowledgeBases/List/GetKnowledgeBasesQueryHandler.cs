using KiwiMind.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.KnowledgeBases.List;

public class GetKnowledgeBasesQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<GetKnowledgeBasesQuery, List<KnowledgeBaseDto>>
{
    public async Task<List<KnowledgeBaseDto>> Handle(GetKnowledgeBasesQuery request, CancellationToken cancellationToken)
    {
        return await db.KnowledgeBases
            .Where(kb => kb.UserId == currentUser.UserId)
            .OrderByDescending(kb => kb.CreatedAt)
            .Select(kb => new KnowledgeBaseDto(kb.Id, kb.Name, kb.CreatedAt, kb.Documents.Count))
            .ToListAsync(cancellationToken);
    }
}
