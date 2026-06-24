using System.Threading.Channels;
using KiwiMind.Application.Common.Interfaces;

namespace KiwiMind.Infrastructure.Ingestion;

public class DocumentIngestionQueue : IDocumentIngestionQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public ValueTask EnqueueAsync(Guid documentId, CancellationToken cancellationToken) =>
        _channel.Writer.WriteAsync(documentId, cancellationToken);

    public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
