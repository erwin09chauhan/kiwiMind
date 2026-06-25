using KiwiMind.Domain.Entities;

namespace KiwiMind.Application.Common.Interfaces;

public interface IEmbeddingService
{
    const int Dimensions = DocumentChunk.EmbeddingDimensions;

    Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken);
}
