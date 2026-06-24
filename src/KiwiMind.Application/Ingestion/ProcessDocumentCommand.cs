using MediatR;

namespace KiwiMind.Application.Ingestion;

public record ProcessDocumentCommand(Guid DocumentId) : IRequest;
