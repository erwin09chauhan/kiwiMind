using KiwiMind.Domain.Enums;

namespace KiwiMind.Application.Documents;

public record DocumentDto(Guid Id, string FileName, DocumentStatus Status, int? PageCount, DateTimeOffset CreatedAt);
