using MediatR;

namespace KiwiMind.Application.Retrieval;

public record SearchKnowledgeBaseQuery(Guid KnowledgeBaseId, string Query, int TopK = 5) : IRequest<List<ChunkSearchResultDto>>;
