using KiwiMind.Application.Common.Exceptions;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KiwiMind.Application.Documents.Upload;

public class UploadDocumentCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IBlobStorageService blobStorage,
    IDocumentIngestionQueue ingestionQueue) : IRequestHandler<UploadDocumentCommand, DocumentDto>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".txt", ".md"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const int MaxDocumentsPerKnowledgeBase = 20;

    public async Task<DocumentDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var knowledgeBaseExists = await db.KnowledgeBases
            .AnyAsync(kb => kb.Id == request.KnowledgeBaseId && kb.UserId == currentUser.UserId, cancellationToken);
        if (!knowledgeBaseExists)
        {
            throw new NotFoundException(nameof(KnowledgeBase), request.KnowledgeBaseId);
        }

        var existingDocumentCount = await db.Documents
            .CountAsync(d => d.KnowledgeBaseId == request.KnowledgeBaseId, cancellationToken);
        if (existingDocumentCount >= MaxDocumentsPerKnowledgeBase)
        {
            throw new QuotaExceededException($"A knowledge base can have at most {MaxDocumentsPerKnowledgeBase} documents.");
        }

        var extension = Path.GetExtension(request.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidFileException($"File type '{extension}' is not supported. Allowed types: {string.Join(", ", AllowedExtensions)}.");
        }

        if (request.Length > MaxFileSizeBytes)
        {
            throw new InvalidFileException($"File exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)}MB.");
        }

        var document = new Document
        {
            KnowledgeBaseId = request.KnowledgeBaseId,
            FileName = request.FileName,
            BlobUri = string.Empty
        };

        var blobName = $"{request.KnowledgeBaseId}/{document.Id}{extension}";
        document.BlobUri = await blobStorage.UploadAsync(request.Content, blobName, request.ContentType, cancellationToken);

        db.Documents.Add(document);
        await db.SaveChangesAsync(cancellationToken);

        await ingestionQueue.EnqueueAsync(document.Id, cancellationToken);

        return new DocumentDto(document.Id, document.FileName, document.Status, document.PageCount, document.CreatedAt);
    }
}
