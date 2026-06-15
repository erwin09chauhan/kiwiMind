using System.Text.Json;
using KiwiMind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiwiMind.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content).IsRequired();

        builder.Property(m => m.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.Citations)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<List<Citation>>(v, JsonSerializerOptions.Default) ?? new List<Citation>())
            .Metadata.SetValueComparer(new ValueComparer<List<Citation>>(
                (a, b) => JsonSerializer.Serialize(a, JsonSerializerOptions.Default) == JsonSerializer.Serialize(b, JsonSerializerOptions.Default),
                v => v.GetHashCode(),
                v => v));

        builder.Property(m => m.Citations).HasColumnType("jsonb");
    }
}
