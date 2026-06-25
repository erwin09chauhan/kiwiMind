using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using KiwiMind.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace KiwiMind.Application.Ingestion;

public class ProcessDocumentCommandHandler(
    IApplicationDbContext db,
    IBlobStorageService blobStorage,
    IDocumentTextExtractor textExtractor,
    ITextChunker chunker,
    IEmbeddingService embeddingService,
    ILogger<ProcessDocumentCommandHandler> logger) : IRequestHandler<ProcessDocumentCommand>
{
    public async Task Handle(ProcessDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await db.Documents.SingleOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);
        if (document is null)
        {
            return;
        }

        document.Status = DocumentStatus.Processing;
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await using var stream = await blobStorage.OpenReadAsync(document.BlobUri, cancellationToken);
            var extracted = await textExtractor.ExtractAsync(stream, document.FileName, cancellationToken);

            var chunks = chunker.Chunk(extracted.Text);
            var embeddings = await embeddingService.GenerateEmbeddingsAsync(
                chunks.Select(c => c.Content).ToList(), cancellationToken);

            for (var i = 0; i < chunks.Count; i++)
            {
                db.DocumentChunks.Add(new DocumentChunk
                {
                    DocumentId = document.Id,
                    ChunkIndex = chunks[i].Index,
                    Content = chunks[i].Content,
                    Embedding = new Vector(embeddings[i]),
                    TokenCount = chunks[i].TokenCount
                });
            }

            document.PageCount = extracted.PageCount;
            document.Status = DocumentStatus.Ready;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process document {DocumentId}", document.Id);
            document.Status = DocumentStatus.Failed;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
