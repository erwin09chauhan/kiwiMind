using KiwiMind.Domain.Common;
using KiwiMind.Domain.Enums;

namespace KiwiMind.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<Citation> Citations { get; set; } = [];
    public int TokensUsed { get; set; }
}
