namespace KiwiMind.Domain.Entities;

public class Citation
{
    public Guid DocumentId { get; set; }
    public Guid ChunkId { get; set; }
    public int? Page { get; set; }
}
