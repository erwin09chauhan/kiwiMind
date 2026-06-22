using MediatR;

namespace KiwiMind.Application.Documents.List;

public record GetDocumentsQuery(Guid KnowledgeBaseId) : IRequest<List<DocumentDto>>;
