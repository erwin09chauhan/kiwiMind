using MediatR;

namespace KiwiMind.Application.KnowledgeBases.Create;

public record CreateKnowledgeBaseCommand(string Name) : IRequest<KnowledgeBaseDto>;
