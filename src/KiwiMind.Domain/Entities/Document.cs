using KiwiMind.Domain.Common;
using KiwiMind.Domain.Enums;

namespace KiwiMind.Domain.Entities;

public class Document : BaseEntity
{
    public Guid KnowledgeBaseId { get; set; }
    public KnowledgeBase KnowledgeBase { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string BlobUri { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Queued;
    public int? PageCount { get; set; }

    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}
