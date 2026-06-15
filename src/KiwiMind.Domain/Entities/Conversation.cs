using KiwiMind.Domain.Common;

namespace KiwiMind.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid KnowledgeBaseId { get; set; }
    public KnowledgeBase KnowledgeBase { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
