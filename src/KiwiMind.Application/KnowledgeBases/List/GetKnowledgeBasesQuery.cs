using MediatR;

namespace KiwiMind.Application.KnowledgeBases.List;

public record GetKnowledgeBasesQuery : IRequest<List<KnowledgeBaseDto>>;
