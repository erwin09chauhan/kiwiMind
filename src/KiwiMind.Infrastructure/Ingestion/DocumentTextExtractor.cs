using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using KiwiMind.Application.Common.Interfaces;
using UglyToad.PdfPig;

namespace KiwiMind.Infrastructure.Ingestion;

public class DocumentTextExtractor : IDocumentTextExtractor
{
    public async Task<ExtractedDocument> ExtractAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => ExtractPdf(content),
            ".docx" => ExtractDocx(content),
            _ => await ExtractPlainTextAsync(content, cancellationToken)
        };
    }

    private static ExtractedDocument ExtractPdf(Stream content)
    {
        using var document = PdfDocument.Open(content);
        var text = string.Join("\n\n", document.GetPages().Select(p => p.Text));
        return new ExtractedDocument(text, document.NumberOfPages);
    }

    private static ExtractedDocument ExtractDocx(Stream content)
    {
        using var document = WordprocessingDocument.Open(content, false);
        var body = document.MainDocumentPart?.Document?.Body;
        var text = body is null ? string.Empty : string.Join("\n", body.Descendants<Paragraph>().Select(p => p.InnerText));
        return new ExtractedDocument(text, PageCount: null);
    }

    private static async Task<ExtractedDocument> ExtractPlainTextAsync(Stream content, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(content);
        var text = await reader.ReadToEndAsync(cancellationToken);
        return new ExtractedDocument(text, PageCount: null);
    }
}
