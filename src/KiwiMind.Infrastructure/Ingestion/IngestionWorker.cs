using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Ingestion;
using KiwiMind.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KiwiMind.Infrastructure.Ingestion;

public class IngestionWorker(
    IServiceScopeFactory scopeFactory,
    IDocumentIngestionQueue queue,
    ILogger<IngestionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RequeuePendingDocumentsAsync(stoppingToken);

        await foreach (var documentId in queue.DequeueAllAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            try
            {
                await sender.Send(new ProcessDocumentCommand(documentId), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error processing document {DocumentId}", documentId);
            }
        }
    }

    private async Task RequeuePendingDocumentsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var pendingIds = await db.Documents
            .Where(d => d.Status == DocumentStatus.Queued || d.Status == DocumentStatus.Processing)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        foreach (var id in pendingIds)
        {
            await queue.EnqueueAsync(id, cancellationToken);
        }
    }
}
