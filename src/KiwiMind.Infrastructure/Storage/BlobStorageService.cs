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

    // BlobUriBuilder correctly extracts the blob name from both host-style
    // Azure URLs (account in the hostname) and path-style emulator URLs
    // (account as a path segment, e.g. Azurite). A hand-rolled segment split
    // silently drops the knowledge-base folder on one of the two layouts.
    private static string GetBlobNameFromUri(string blobUri) =>
        new BlobUriBuilder(new Uri(blobUri)).BlobName;
}
