namespace KiwiMind.Application.Common.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string blobName, string contentType, CancellationToken cancellationToken);
    Task DeleteAsync(string blobUri, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string blobUri, CancellationToken cancellationToken);
}
