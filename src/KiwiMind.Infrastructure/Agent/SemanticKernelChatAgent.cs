using System.Diagnostics;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Common.Telemetry;
using KiwiMind.Application.Retrieval;
using KiwiMind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace KiwiMind.Infrastructure.Agent;

public class SemanticKernelChatAgent(
    IApplicationDbContext db,
    KnowledgeBasePlugin plugin,
    IChatCompletionService chatCompletionService) : IChatAgent
{
    public async Task<AgentResult> AskAsync(
        Guid knowledgeBaseId,
        IReadOnlyList<ChatMessage> history,
        string question,
        CancellationToken cancellationToken)
    {
        using var requestActivity = KiwiMindTelemetry.ActivitySource.StartActivity("rag.chat_request");
        requestActivity?.SetTag("kiwimind.knowledge_base_id", knowledgeBaseId);

        var kernel = new Kernel();
        kernel.Plugins.AddFromObject(plugin, "KnowledgeBase");

        var lowerQuestion = question.ToLowerInvariant();
        var kbDocuments = await db.Documents
            .Where(d => d.KnowledgeBaseId == knowledgeBaseId)
            .Select(d => new { d.Id, d.FileName })
            .ToListAsync(cancellationToken);

        var mentionedDocs = kbDocuments
            .Where(d => lowerQuestion.Contains(d.FileName.ToLowerInvariant()))
            .ToList();

        var toolCalls = new List<AgentToolCall>();

        void RecordToolCall(string toolName, string arguments)
        {
            toolCalls.Add(new AgentToolCall(toolName, arguments));
            KiwiMindTelemetry.ToolCalls.Add(1, new KeyValuePair<string, object?>("tool.name", toolName));
        }

        Activity? StartToolActivity(string toolName) =>
            KiwiMindTelemetry.ActivitySource.StartActivity($"agent.tool.{toolName}");

        if (lowerQuestion.Contains("what documents") || lowerQuestion.Contains("which documents")
            || lowerQuestion.Contains("list documents") || lowerQuestion.Contains("what do you know")
            || lowerQuestion.Contains("which files") || lowerQuestion.Contains("what files"))
        {
            using var toolActivity = StartToolActivity("list_documents");
            var args = new KernelArguments { ["knowledgeBaseId"] = knowledgeBaseId };
            RecordToolCall("list_documents", knowledgeBaseId.ToString());
            var documents = await kernel.InvokeAsync<List<DocumentMetadataResult>>("KnowledgeBase", "list_documents", args, cancellationToken)
                ?? [];
            var answer = documents.Count == 0
                ? "This knowledge base has no documents yet."
                : "This knowledge base contains: " + string.Join(", ", documents.Select(d => $"{d.FileName} ({d.Status})")) + ".";
            return new AgentResult(answer, [], toolCalls);
        }

        if (lowerQuestion.Contains("compare") && mentionedDocs.Count >= 2)
        {
            using var toolActivity = StartToolActivity("compare_documents");
            var args = new KernelArguments { ["documentIdA"] = mentionedDocs[0].Id, ["documentIdB"] = mentionedDocs[1].Id };
            RecordToolCall("compare_documents", $"{mentionedDocs[0].FileName}, {mentionedDocs[1].FileName}");
            var result = await kernel.InvokeAsync<string>("KnowledgeBase", "compare_documents", args, cancellationToken);
            return new AgentResult(result ?? string.Empty, [], toolCalls);
        }

        if (lowerQuestion.Contains("summarize") && mentionedDocs.Count >= 1)
        {
            using var toolActivity = StartToolActivity("summarize_document");
            var args = new KernelArguments { ["documentId"] = mentionedDocs[0].Id };
            RecordToolCall("summarize_document", mentionedDocs[0].FileName);
            var result = await kernel.InvokeAsync<string>("KnowledgeBase", "summarize_document", args, cancellationToken);
            var summary = string.IsNullOrEmpty(result) ? "No content available to summarize." : $"Summary of {mentionedDocs[0].FileName}: {result}";
            return new AgentResult(summary, [], toolCalls);
        }

        if ((lowerQuestion.Contains("metadata") || lowerQuestion.Contains("how many pages") || lowerQuestion.Contains("status") || lowerQuestion.Contains("when was"))
            && mentionedDocs.Count >= 1)
        {
            using var toolActivity = StartToolActivity("get_document_metadata");
            var args = new KernelArguments { ["documentId"] = mentionedDocs[0].Id };
            RecordToolCall("get_document_metadata", mentionedDocs[0].FileName);
            var metadata = await kernel.InvokeAsync<DocumentMetadataResult?>("KnowledgeBase", "get_document_metadata", args, cancellationToken);
            var answer = metadata is null
                ? $"Could not find metadata for {mentionedDocs[0].FileName}."
                : $"{metadata.FileName}: status {metadata.Status}, {metadata.PageCount?.ToString() ?? "unknown"} pages, uploaded {metadata.CreatedAt:g}.";
            return new AgentResult(answer, [], toolCalls);
        }

        using var searchActivity = StartToolActivity("search_knowledge_base");
        var searchArgs = new KernelArguments { ["knowledgeBaseId"] = knowledgeBaseId, ["query"] = question };
        RecordToolCall("search_knowledge_base", question);
        var context = await kernel.InvokeAsync<List<ChunkSearchResultDto>>("KnowledgeBase", "search_knowledge_base", searchArgs, cancellationToken)
            ?? [];

        var searchAnswer = await chatCompletionService.GenerateAnswerAsync(context, history, question, cancellationToken);
        var citations = context.Select(c => new Citation { DocumentId = c.DocumentId, ChunkId = c.ChunkId, Page = c.Page }).ToList();

        return new AgentResult(searchAnswer, citations, toolCalls);
    }
}
