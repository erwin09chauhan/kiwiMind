using System.Security.Cryptography;
using System.Text;
using KiwiMind.Application.Common.Interfaces;

namespace KiwiMind.Infrastructure.Ingestion;

public class FakeEmbeddingService : IEmbeddingService
{
    public Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken)
    {
        IReadOnlyList<float[]> embeddings = texts.Select(Generate).ToList();
        return Task.FromResult(embeddings);
    }

    private static float[] Generate(string text)
    {
        var seedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        var random = new Random(BitConverter.ToInt32(seedBytes, 0));

        var vector = new float[IEmbeddingService.Dimensions];
        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] = (float)(random.NextDouble() * 2 - 1);
        }

        var magnitude = MathF.Sqrt(vector.Sum(v => v * v));
        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] /= magnitude;
        }

        return vector;
    }
}
