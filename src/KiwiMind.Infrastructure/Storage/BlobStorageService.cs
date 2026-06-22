using Azure.Storage.Blobs;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace KiwiMind.Infrastructure.Storage;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IOptions<BlobStorageSettings> settings)
    {
        var serviceClient = new BlobServiceClient(settings.Value.ConnectionString);
        _containerClient = serviceClient.GetBlobContainerClient(settings.Value.ContainerName);
    }

    public async Task<string> UploadAsync(Stream content, string blobName, string contentType, CancellationToken cancellationToken)
    {
        await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(
            content,
            new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string blobUri, CancellationToken cancellationToken)
    {
        var blobClient = _containerClient.GetBlobClient(GetBlobNameFromUri(blobUri));
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<Stream> OpenReadAsync(string blobUri, CancellationToken cancellationToken)
    {
        var blobClient = _containerClient.GetBlobClient(GetBlobNameFromUri(blobUri));
        return await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
    }

    private string GetBlobNameFromUri(string blobUri) =>
        Uri.UnescapeDataString(new Uri(blobUri).AbsolutePath.TrimStart('/').Split('/', 3)[^1]);
}
