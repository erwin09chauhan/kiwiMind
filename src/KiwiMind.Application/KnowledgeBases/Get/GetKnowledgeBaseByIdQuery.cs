using MediatR;

namespace KiwiMind.Application.KnowledgeBases.Get;

public record GetKnowledgeBaseByIdQuery(Guid Id) : IRequest<KnowledgeBaseDto>;
