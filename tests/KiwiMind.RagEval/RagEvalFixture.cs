using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Domain.Entities;
using KiwiMind.Domain.Enums;
using KiwiMind.Infrastructure;
using KiwiMind.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pgvector;
using Testcontainers.PostgreSql;

namespace KiwiMind.RagEval;

public class RagEvalFixture : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private ServiceProvider _provider = null!;

    public Guid KnowledgeBaseId { get; private set; }
    public IReadOnlyDictionary<string, Guid> DocumentIdsByFileName { get; private set; } = new Dictionary<string, Guid>();
    public IServiceProvider Services => _provider;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("pgvector/pgvector:pg16")
            .Build();
        await _container.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _container.GetConnectionString(),
                ["Jwt:Secret"] = "rag-eval-test-signing-key-not-for-production-use-0000000000",
                ["Jwt:Issuer"] = "KiwiMind.RagEval",
                ["Jwt:Audience"] = "KiwiMind.RagEval",
                ["BlobStorage:ConnectionString"] = "UseDevelopmentStorage=true",
                ["BlobStorage:ContainerName"] = "documents"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Application.Retrieval.SearchKnowledgeBaseQuery).Assembly));
        services.AddSingleton<TestCurrentUserService>();
        services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<TestCurrentUserService>());

        _provider = services.BuildServiceProvider();

        await using (var scope = _provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<KiwiMindDbContext>();
            await db.Database.MigrateAsync();
        }

        await SeedAsync();
    }

    private async Task SeedAsync()
    {
        await using var scope = _provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var extractor = scope.ServiceProvider.GetRequiredService<IDocumentTextExtractor>();
        var chunker = scope.ServiceProvider.GetRequiredService<ITextChunker>();
        var embedder = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();

        var user = new User { Email = "rageval@kiwimind.dev", PasswordHash = "unused" };
        db.Users.Add(user);

        var knowledgeBase = new KnowledgeBase { UserId = user.Id, Name = "RAG Eval Golden Set" };
        db.KnowledgeBases.Add(knowledgeBase);

        var documentIds = new Dictionary<string, Guid>();
        var docsFolder = Path.Combine(AppContext.BaseDirectory, "GoldenData", "docs");

        foreach (var filePath in Directory.GetFiles(docsFolder, "*.txt").OrderBy(f => f))
        {
            var fileName = Path.GetFileName(filePath);

            var document = new Document
            {
                KnowledgeBaseId = knowledgeBase.Id,
                FileName = fileName,
                BlobUri = $"test://golden-data/{fileName}",
                Status = DocumentStatus.Ready
            };
            db.Documents.Add(document);
            documentIds[fileName] = document.Id;

            await using var stream = File.OpenRead(filePath);
            var extracted = await extractor.ExtractAsync(stream, fileName, CancellationToken.None);
            var chunks = chunker.Chunk(extracted.Text);
            var embeddings = await embedder.GenerateEmbeddingsAsync(
                chunks.Select(c => c.Content).ToList(), CancellationToken.None);

            for (var i = 0; i < chunks.Count; i++)
            {
                db.DocumentChunks.Add(new DocumentChunk
                {
                    DocumentId = document.Id,
                    ChunkIndex = chunks[i].Index,
                    Content = chunks[i].Content,
                    Embedding = new Vector(embeddings[i]),
                    TokenCount = chunks[i].TokenCount
                });
            }
        }

        await db.SaveChangesAsync(CancellationToken.None);

        scope.ServiceProvider.GetRequiredService<TestCurrentUserService>().UserId = user.Id;

        KnowledgeBaseId = knowledgeBase.Id;
        DocumentIdsByFileName = documentIds;
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _container.DisposeAsync();
    }
}
