namespace KiwiMind.Infrastructure.Agent;

public record DocumentMetadataResult(string FileName, string Status, int? PageCount, DateTimeOffset CreatedAt);
