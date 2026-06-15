using KiwiMind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;

namespace KiwiMind.Infrastructure.Persistence.Configurations;

public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public const int EmbeddingDimensions = 1536;

    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content).IsRequired();

        builder.Property(c => c.Embedding)
            .HasConversion(
                v => new Vector(v),
                v => v.ToArray())
            .HasColumnType($"vector({EmbeddingDimensions})");

        builder.HasIndex(c => c.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");
    }
}
