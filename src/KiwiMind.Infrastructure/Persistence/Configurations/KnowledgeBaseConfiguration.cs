using KiwiMind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiwiMind.Infrastructure.Persistence.Configurations;

public class KnowledgeBaseConfiguration : IEntityTypeConfiguration<KnowledgeBase>
{
    public void Configure(EntityTypeBuilder<KnowledgeBase> builder)
    {
        builder.HasKey(kb => kb.Id);

        builder.Property(kb => kb.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(kb => kb.Documents)
            .WithOne(d => d.KnowledgeBase)
            .HasForeignKey(d => d.KnowledgeBaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(kb => kb.Conversations)
            .WithOne(c => c.KnowledgeBase)
            .HasForeignKey(c => c.KnowledgeBaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
