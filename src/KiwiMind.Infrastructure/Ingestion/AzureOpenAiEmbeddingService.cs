using System.ClientModel;
using Azure.AI.OpenAI;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Common.Settings;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;

namespace KiwiMind.Infrastructure.Ingestion;

public class AzureOpenAiEmbeddingService : IEmbeddingService
{
    // Azure's embeddings endpoint rejects a single request with more than 2048
    // inputs, and large batches blow the per-minute token budget in one shot.
    // Send chunks in modest sub-batches so any document size succeeds; the
    // SDK's built-in retry absorbs transient 429s between batches.
    private const int BatchSize = 96;

    private readonly EmbeddingClient embeddingClient;

    public AzureOpenAiEmbeddingService(IOptions<AzureOpenAiSettings> options)
    {
        var settings = options.Value;
        var client = new AzureOpenAIClient(new Uri(settings.Endpoint), new ApiKeyCredential(settings.ApiKey));
        embeddingClient = client.GetEmbeddingClient(settings.EmbeddingDeploymentName);
    }

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken)
    {
        if (texts.Count == 0)
        {
            return [];
        }

        var options = new EmbeddingGenerationOptions { Dimensions = IEmbeddingService.Dimensions };
        var results = new List<float[]>(texts.Count);

        for (var offset = 0; offset < texts.Count; offset += BatchSize)
        {
            var batch = texts.Skip(offset).Take(BatchSize).ToList();
            var response = await embeddingClient.GenerateEmbeddingsAsync(batch, options, cancellationToken);
            results.AddRange(response.Value.Select(e => e.ToFloats().ToArray()));
        }

        return results;
    }
}
