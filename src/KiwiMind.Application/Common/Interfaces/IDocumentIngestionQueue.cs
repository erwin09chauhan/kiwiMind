namespace KiwiMind.Application.Common.Interfaces;

public interface IDocumentIngestionQueue
{
    ValueTask EnqueueAsync(Guid documentId, CancellationToken cancellationToken);
    IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken);
}
