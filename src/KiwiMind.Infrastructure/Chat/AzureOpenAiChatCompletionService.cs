using System.ClientModel;
using System.Text;
using Azure.AI.OpenAI;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Common.Settings;
using KiwiMind.Application.Retrieval;
using KiwiMind.Domain.Enums;
using Microsoft.Extensions.Options;
using OpenAIChatMessage = OpenAI.Chat.ChatMessage;
using SystemChatMessage = OpenAI.Chat.SystemChatMessage;
using UserChatMessage = OpenAI.Chat.UserChatMessage;
using AssistantChatMessage = OpenAI.Chat.AssistantChatMessage;
using ChatCompletionOptions = OpenAI.Chat.ChatCompletionOptions;
using ChatClient = OpenAI.Chat.ChatClient;

namespace KiwiMind.Infrastructure.Chat;

public class AzureOpenAiChatCompletionService : IChatCompletionService
{
    private const int MaxOutputTokens = 800;
    private readonly ChatClient chatClient;

    public AzureOpenAiChatCompletionService(IOptions<AzureOpenAiSettings> options)
    {
        var settings = options.Value;
        var client = new AzureOpenAIClient(new Uri(settings.Endpoint), new ApiKeyCredential(settings.ApiKey));
        chatClient = client.GetChatClient(settings.ChatDeploymentName);
    }

    public async Task<string> GenerateAnswerAsync(
        IReadOnlyList<ChunkSearchResultDto> context,
        IReadOnlyList<ChatMessage> history,
        string question,
        CancellationToken cancellationToken)
    {
        if (context.Count == 0)
        {
            return $"I couldn't find any relevant passages in this knowledge base to answer: \"{question}\".";
        }

        var messages = new List<OpenAIChatMessage>
        {
            new SystemChatMessage(
                "You are a knowledge base assistant. Answer only using the numbered sources below. " +
                "Cite sources inline using their number in square brackets, e.g. [1]. " +
                "If the sources don't contain the answer, say you don't know.")
        };

        foreach (var message in history)
        {
            messages.Add(message.Role == MessageRole.User
                ? new UserChatMessage(message.Content)
                : new AssistantChatMessage(message.Content));
        }

        var sourcesBlock = new StringBuilder();
        for (var i = 0; i < context.Count; i++)
        {
            sourcesBlock.AppendLine($"[{i + 1}] ({context[i].DocumentFileName}) {context[i].Content}");
        }

        messages.Add(new UserChatMessage($"Sources:\n{sourcesBlock}\nQuestion: {question}"));

        var response = await chatClient.CompleteChatAsync(
            messages,
            new ChatCompletionOptions { MaxOutputTokenCount = MaxOutputTokens },
            cancellationToken);

        return response.Value.Content.Count > 0 ? response.Value.Content[0].Text : string.Empty;
    }
}
