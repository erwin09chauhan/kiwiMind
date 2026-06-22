using MediatR;

namespace KiwiMind.Application.Documents.Upload;

public record UploadDocumentCommand(
    Guid KnowledgeBaseId,
    string FileName,
    string ContentType,
    long Length,
    Stream Content) : IRequest<DocumentDto>;
