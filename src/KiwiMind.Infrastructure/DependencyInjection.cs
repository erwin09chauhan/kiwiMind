using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Common.Settings;
using KiwiMind.Infrastructure.Agent;
using KiwiMind.Infrastructure.Auth;
using KiwiMind.Infrastructure.Chat;
using KiwiMind.Infrastructure.Ingestion;
using KiwiMind.Infrastructure.Persistence;
using KiwiMind.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KiwiMind.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not configured.");

        services.AddDbContext<KiwiMindDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<KiwiMindDbContext>());

        services.Configure<JwtSettings>(options => configuration.GetSection(JwtSettings.SectionName).Bind(options));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();

        services.Configure<BlobStorageSettings>(options => configuration.GetSection(BlobStorageSettings.SectionName).Bind(options));
        services.AddSingleton<IBlobStorageService, BlobStorageService>();

        services.AddSingleton<IDocumentIngestionQueue, DocumentIngestionQueue>();
        services.AddSingleton<ITextChunker, TextChunker>();
        services.AddSingleton<IDocumentTextExtractor, DocumentTextExtractor>();

        services.Configure<AzureOpenAiSettings>(options => configuration.GetSection(AzureOpenAiSettings.SectionName).Bind(options));
        var azureOpenAiEnabled = configuration.GetSection(AzureOpenAiSettings.SectionName).Get<AzureOpenAiSettings>()?.Enabled ?? false;
        if (azureOpenAiEnabled)
        {
            services.AddSingleton<IEmbeddingService, AzureOpenAiEmbeddingService>();
            services.AddSingleton<IChatCompletionService, AzureOpenAiChatCompletionService>();
        }
        else
        {
            services.AddSingleton<IEmbeddingService, FakeEmbeddingService>();
            services.AddSingleton<IChatCompletionService, FakeChatCompletionService>();
        }

        services.AddScoped<KnowledgeBasePlugin>();
        services.AddScoped<IChatAgent, SemanticKernelChatAgent>();
        services.AddHostedService<IngestionWorker>();

        return services;
    }
}
