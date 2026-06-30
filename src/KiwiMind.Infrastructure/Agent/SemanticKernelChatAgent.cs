using KiwiMind.Application.Common.Interfaces;
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

        if (lowerQuestion.Contains("compare") && mentionedDocs.Count >= 2)
        {
            var args = new KernelArguments { ["documentIdA"] = mentionedDocs[0].Id, ["documentIdB"] = mentionedDocs[1].Id };
            toolCalls.Add(new AgentToolCall("compare_documents", $"{mentionedDocs[0].FileName}, {mentionedDocs[1].FileName}"));
            var result = await kernel.InvokeAsync<string>("KnowledgeBase", "compare_documents", args, cancellationToken);
            return new AgentResult(result ?? string.Empty, [], toolCalls);
        }

        if (lowerQuestion.Contains("summarize") && mentionedDocs.Count >= 1)
        {
            var args = new KernelArguments { ["documentId"] = mentionedDocs[0].Id };
            toolCalls.Add(new AgentToolCall("summarize_document", mentionedDocs[0].FileName));
            var result = await kernel.InvokeAsync<string>("KnowledgeBase", "summarize_document", args, cancellationToken);
            var summary = string.IsNullOrEmpty(result) ? "No content available to summarize." : $"Summary of {mentionedDocs[0].FileName}: {result}";
            return new AgentResult(summary, [], toolCalls);
        }

        if ((lowerQuestion.Contains("metadata") || lowerQuestion.Contains("how many pages") || lowerQuestion.Contains("status") || lowerQuestion.Contains("when was"))
            && mentionedDocs.Count >= 1)
        {
            var args = new KernelArguments { ["documentId"] = mentionedDocs[0].Id };
            toolCalls.Add(new AgentToolCall("get_document_metadata", mentionedDocs[0].FileName));
            var metadata = await kernel.InvokeAsync<DocumentMetadataResult?>("KnowledgeBase", "get_document_metadata", args, cancellationToken);
            var answer = metadata is null
                ? $"Could not find metadata for {mentionedDocs[0].FileName}."
                : $"{metadata.FileName}: status {metadata.Status}, {metadata.PageCount?.ToString() ?? "unknown"} pages, uploaded {metadata.CreatedAt:g}.";
            return new AgentResult(answer, [], toolCalls);
        }

        var searchArgs = new KernelArguments { ["knowledgeBaseId"] = knowledgeBaseId, ["query"] = question };
        toolCalls.Add(new AgentToolCall("search_knowledge_base", question));
        var context = await kernel.InvokeAsync<List<ChunkSearchResultDto>>("KnowledgeBase", "search_knowledge_base", searchArgs, cancellationToken)
            ?? [];

        var searchAnswer = await chatCompletionService.GenerateAnswerAsync(context, history, question, cancellationToken);
        var citations = context.Select(c => new Citation { DocumentId = c.DocumentId, ChunkId = c.ChunkId, Page = c.Page }).ToList();

        return new AgentResult(searchAnswer, citations, toolCalls);
    }
}
