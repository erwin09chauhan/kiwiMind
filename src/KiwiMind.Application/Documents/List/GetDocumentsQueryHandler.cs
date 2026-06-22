using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.Documents.List;

public class GetDocumentsQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<GetDocumentsQuery, List<DocumentDto>>
{
    public async Task<List<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var knowledgeBaseExists = await db.KnowledgeBases
            .AnyAsync(kb => kb.Id == request.KnowledgeBaseId && kb.UserId == currentUser.UserId, cancellationToken);
        if (!knowledgeBaseExists)
        {
            throw new NotFoundException(nameof(KnowledgeBase), request.KnowledgeBaseId);
        }

        return await db.Documents
            .Where(d => d.KnowledgeBaseId == request.KnowledgeBaseId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DocumentDto(d.Id, d.FileName, d.Status, d.PageCount, d.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
