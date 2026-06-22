using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.Documents.Get;

public class GetDocumentByIdQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<GetDocumentByIdQuery, DocumentDto>
{
    public async Task<DocumentDto> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await db.Documents
            .Where(d => d.Id == request.DocumentId
                && d.KnowledgeBaseId == request.KnowledgeBaseId
                && d.KnowledgeBase.UserId == currentUser.UserId)
            .Select(d => new DocumentDto(d.Id, d.FileName, d.Status, d.PageCount, d.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException(nameof(Document), request.DocumentId);
    }
}
