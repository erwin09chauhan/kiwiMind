namespace KiwiMind.Application.KnowledgeBases;

public record KnowledgeBaseDto(Guid Id, string Name, DateTimeOffset CreatedAt, int DocumentCount);
