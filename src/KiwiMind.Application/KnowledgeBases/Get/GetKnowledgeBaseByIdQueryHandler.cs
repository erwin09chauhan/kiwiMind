using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.KnowledgeBases.Get;

public class GetKnowledgeBaseByIdQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<GetKnowledgeBaseByIdQuery, KnowledgeBaseDto>
{
    public async Task<KnowledgeBaseDto> Handle(GetKnowledgeBaseByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await db.KnowledgeBases
            .Where(kb => kb.Id == request.Id && kb.UserId == currentUser.UserId)
            .Select(kb => new KnowledgeBaseDto(kb.Id, kb.Name, kb.CreatedAt, kb.Documents.Count))
            .SingleOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException(nameof(Domain.Entities.KnowledgeBase), request.Id);
    }
}
