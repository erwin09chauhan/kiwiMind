namespace KiwiMind.Application.Common.Interfaces;

public interface IEmbeddingService
{
    const int Dimensions = 1536;

    Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken);
}
