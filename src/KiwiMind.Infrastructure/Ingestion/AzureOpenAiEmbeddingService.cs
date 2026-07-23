using System.ClientModel;
using Azure.AI.OpenAI;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Common.Settings;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;

namespace KiwiMind.Infrastructure.Ingestion;

public class AzureOpenAiEmbeddingService : IEmbeddingService
{
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

        var response = await embeddingClient.GenerateEmbeddingsAsync(
            texts,
            new EmbeddingGenerationOptions { Dimensions = IEmbeddingService.Dimensions },
            cancellationToken);

        return response.Value.Select(e => e.ToFloats().ToArray()).ToList();
    }
}
