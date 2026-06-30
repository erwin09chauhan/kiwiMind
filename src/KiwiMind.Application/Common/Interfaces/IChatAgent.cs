using KiwiMind.Domain.Entities;

namespace KiwiMind.Application.Common.Interfaces;

public record AgentToolCall(string ToolName, string Arguments);

public record AgentResult(string Answer, List<Citation> Citations, List<AgentToolCall> ToolCalls);

public interface IChatAgent
{
    Task<AgentResult> AskAsync(
        Guid knowledgeBaseId,
        IReadOnlyList<ChatMessage> history,
        string question,
        CancellationToken cancellationToken);
}
