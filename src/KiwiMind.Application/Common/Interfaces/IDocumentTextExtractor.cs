namespace KiwiMind.Application.Common.Interfaces;

public record ExtractedDocument(string Text, int? PageCount);

public interface IDocumentTextExtractor
{
    Task<ExtractedDocument> ExtractAsync(Stream content, string fileName, CancellationToken cancellationToken);
}
