using MediatR;

namespace KiwiMind.Application.KnowledgeBases.Delete;

public record DeleteKnowledgeBaseCommand(Guid Id) : IRequest;
