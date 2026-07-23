using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.KnowledgeBases.Create;

public class CreateKnowledgeBaseCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<CreateKnowledgeBaseCommand, KnowledgeBaseDto>
{
    private const int MaxKnowledgeBasesPerUser = 5;

    public async Task<KnowledgeBaseDto> Handle(CreateKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
        var existingCount = await db.KnowledgeBases
            .CountAsync(kb => kb.UserId == currentUser.UserId, cancellationToken);
        if (existingCount >= MaxKnowledgeBasesPerUser)
        {
            throw new QuotaExceededException($"You can have at most {MaxKnowledgeBasesPerUser} knowledge bases.");
        }

        var knowledgeBase = new KnowledgeBase
        {
            UserId = currentUser.UserId,
            Name = request.Name.Trim()
        };

        db.KnowledgeBases.Add(knowledgeBase);
        await db.SaveChangesAsync(cancellationToken);

        return new KnowledgeBaseDto(knowledgeBase.Id, knowledgeBase.Name, knowledgeBase.CreatedAt, DocumentCount: 0);
    }
}
