using KiwiMind.Application.Retrieval;
using KiwiMind.Domain.Enums;

namespace KiwiMind.Application.Common.Interfaces;

public record ChatMessage(MessageRole Role, string Content);

public interface IChatCompletionService
{
    Task<string> GenerateAnswerAsync(
        IReadOnlyList<ChunkSearchResultDto> context,
        IReadOnlyList<ChatMessage> history,
        string question,
        CancellationToken cancellationToken);
}
