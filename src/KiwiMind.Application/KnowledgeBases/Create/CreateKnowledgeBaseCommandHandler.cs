using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;

namespace KiwiMind.Application.KnowledgeBases.Create;

public class CreateKnowledgeBaseCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<CreateKnowledgeBaseCommand, KnowledgeBaseDto>
{
    public async Task<KnowledgeBaseDto> Handle(CreateKnowledgeBaseCommand request, CancellationToken cancellationToken)
    {
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
