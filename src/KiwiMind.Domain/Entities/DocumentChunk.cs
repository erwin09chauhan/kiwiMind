using KiwiMind.Domain.Common;
using Pgvector;

namespace KiwiMind.Domain.Entities;

public class DocumentChunk : BaseEntity
{
    public const int EmbeddingDimensions = 1536;

    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public Vector Embedding { get; set; } = new(new float[EmbeddingDimensions]);
    public int? Page { get; set; }
    public int TokenCount { get; set; }
}
