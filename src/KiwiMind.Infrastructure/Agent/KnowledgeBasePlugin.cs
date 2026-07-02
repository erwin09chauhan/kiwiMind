using System.ComponentModel;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Retrieval;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace KiwiMind.Infrastructure.Agent;

public class KnowledgeBasePlugin(IApplicationDbContext db, MediatR.ISender sender)
{
    [KernelFunction("search_knowledge_base")]
    [Description("Searches the knowledge base for chunks relevant to a natural-language query, using vector similarity.")]
    public async Task<List<ChunkSearchResultDto>> SearchKnowledgeBaseAsync(Guid knowledgeBaseId, string query, CancellationToken cancellationToken = default) =>
        await sender.Send(new SearchKnowledgeBaseQuery(knowledgeBaseId, query), cancellationToken);

    [KernelFunction("list_documents")]
    [Description("Lists all documents in the knowledge base with their file name and processing status.")]
    public async Task<List<DocumentMetadataResult>> ListDocumentsAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default) =>
        await db.Documents
            .Where(d => d.KnowledgeBaseId == knowledgeBaseId)
            .OrderBy(d => d.FileName)
            .Select(d => new DocumentMetadataResult(d.FileName, d.Status.ToString(), d.PageCount, d.CreatedAt))
            .ToListAsync(cancellationToken);

    [KernelFunction("get_document_metadata")]
    [Description("Retrieves metadata (file name, processing status, page count, upload date) for a specific document.")]
    public async Task<DocumentMetadataResult?> GetDocumentMetadataAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await db.Documents.SingleOrDefaultAsync(d => d.Id == documentId, cancellationToken);
        return document is null
            ? null
            : new DocumentMetadataResult(document.FileName, document.Status.ToString(), document.PageCount, document.CreatedAt);
    }

    [KernelFunction("summarize_document")]
    [Description("Produces a short summary of a document by concatenating its leading chunks.")]
    public async Task<string> SummarizeDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var chunks = await db.DocumentChunks
            .Where(c => c.DocumentId == documentId)
            .OrderBy(c => c.ChunkIndex)
            .Take(2)
            .Select(c => c.Content)
            .ToListAsync(cancellationToken);

        return chunks.Count == 0 ? string.Empty : string.Join(" ", chunks);
    }

    [KernelFunction("compare_documents")]
    [Description("Compares two documents by their metadata (page count, chunk count, upload date).")]
    public async Task<string> CompareDocumentsAsync(Guid documentIdA, Guid documentIdB, CancellationToken cancellationToken = default)
    {
        var docs = await db.Documents
            .Where(d => d.Id == documentIdA || d.Id == documentIdB)
            .Select(d => new { d.Id, d.FileName, d.PageCount, d.CreatedAt, ChunkCount = d.Chunks.Count })
            .ToListAsync(cancellationToken);

        var a = docs.SingleOrDefault(d => d.Id == documentIdA);
        var b = docs.SingleOrDefault(d => d.Id == documentIdB);
        if (a is null || b is null)
        {
            return "One or both documents could not be found.";
        }

        return $"{a.FileName}: {a.PageCount?.ToString() ?? "unknown"} pages, {a.ChunkCount} chunks, uploaded {a.CreatedAt:d}. " +
               $"{b.FileName}: {b.PageCount?.ToString() ?? "unknown"} pages, {b.ChunkCount} chunks, uploaded {b.CreatedAt:d}.";
    }
}
