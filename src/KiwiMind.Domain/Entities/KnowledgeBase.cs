using KiwiMind.Domain.Common;

namespace KiwiMind.Domain.Entities;

public class KnowledgeBase : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
