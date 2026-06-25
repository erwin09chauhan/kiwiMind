using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace KiwiMind.Application.Retrieval;

public class SearchKnowledgeBaseQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IEmbeddingService embeddingService) : IRequestHandler<SearchKnowledgeBaseQuery, List<ChunkSearchResultDto>>
{
    public async Task<List<ChunkSearchResultDto>> Handle(SearchKnowledgeBaseQuery request, CancellationToken cancellationToken)
    {
        var knowledgeBaseExists = await db.KnowledgeBases
            .AnyAsync(kb => kb.Id == request.KnowledgeBaseId && kb.UserId == currentUser.UserId, cancellationToken);
        if (!knowledgeBaseExists)
        {
            throw new NotFoundException(nameof(KnowledgeBase), request.KnowledgeBaseId);
        }

        var embeddings = await embeddingService.GenerateEmbeddingsAsync([request.Query], cancellationToken);
        var queryVector = new Vector(embeddings[0]);

        return await db.DocumentChunks
            .Where(c => c.Document.KnowledgeBaseId == request.KnowledgeBaseId)
            .OrderBy(c => c.Embedding.CosineDistance(queryVector))
            .Take(request.TopK)
            .Select(c => new ChunkSearchResultDto(
                c.Id,
                c.DocumentId,
                c.Document.FileName,
                c.ChunkIndex,
                c.Content,
                c.Page,
                c.Embedding.CosineDistance(queryVector)))
            .ToListAsync(cancellationToken);
    }
}
