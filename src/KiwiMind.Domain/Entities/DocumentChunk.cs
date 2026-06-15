using KiwiMind.Domain.Common;

namespace KiwiMind.Domain.Entities;

public class DocumentChunk : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = [];
    public int? Page { get; set; }
    public int TokenCount { get; set; }
}
