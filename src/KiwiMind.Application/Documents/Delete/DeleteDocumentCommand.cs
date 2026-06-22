using MediatR;

namespace KiwiMind.Application.Documents.Delete;

public record DeleteDocumentCommand(Guid KnowledgeBaseId, Guid DocumentId) : IRequest;
