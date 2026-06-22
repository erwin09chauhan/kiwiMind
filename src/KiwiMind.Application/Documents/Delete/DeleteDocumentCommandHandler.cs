using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.Documents.Delete;

public class DeleteDocumentCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IBlobStorageService blobStorage) : IRequestHandler<DeleteDocumentCommand>
{
    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await db.Documents
            .SingleOrDefaultAsync(d => d.Id == request.DocumentId
                && d.KnowledgeBaseId == request.KnowledgeBaseId
                && d.KnowledgeBase.UserId == currentUser.UserId, cancellationToken);

        if (document is null)
        {
            throw new NotFoundException(nameof(Document), request.DocumentId);
        }

        await blobStorage.DeleteAsync(document.BlobUri, cancellationToken);

        db.Documents.Remove(document);
        await db.SaveChangesAsync(cancellationToken);
    }
}
