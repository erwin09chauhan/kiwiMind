namespace KiwiMind.Application.Retrieval;

public record ChunkSearchResultDto(
    Guid ChunkId,
    Guid DocumentId,
    string DocumentFileName,
    int ChunkIndex,
    string Content,
    int? Page,
    double Distance);
