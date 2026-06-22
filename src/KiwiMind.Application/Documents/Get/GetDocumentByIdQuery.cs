using MediatR;

namespace KiwiMind.Application.Documents.Get;

public record GetDocumentByIdQuery(Guid KnowledgeBaseId, Guid DocumentId) : IRequest<DocumentDto>;
