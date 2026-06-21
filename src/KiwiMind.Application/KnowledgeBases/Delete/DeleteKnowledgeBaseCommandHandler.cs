using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.KnowledgeBases.Delete;

public class DeleteKnowledgeBaseCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<DeleteKnowledgeBaseCommand>
{
    public async Task Handle(DeleteKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
        var knowledgeBase = await db.KnowledgeBases
            .SingleOrDefaultAsync(kb => kb.Id == request.Id && kb.UserId == currentUser.UserId, cancellationToken);

        if (knowledgeBase is null)
        {
            throw new NotFoundException(nameof(Domain.Entities.KnowledgeBase), request.Id);
        }

        db.KnowledgeBases.Remove(knowledgeBase);
        await db.SaveChangesAsync(cancellationToken);
    }
}
