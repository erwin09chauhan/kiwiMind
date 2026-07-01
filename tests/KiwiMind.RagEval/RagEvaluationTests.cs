using System.Text.Json;
using KiwiMind.Application.Common.Interfaces;
using KiwiMind.Application.Retrieval;
using KiwiMind.RagEval.Scoring;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace KiwiMind.RagEval;

public class RagEvaluationTests(RagEvalFixture fixture, ITestOutputHelper output) : IClassFixture<RagEvalFixture>
{
    [Fact]
    public async Task GoldenDataset_ProducesAnswers_WithReportedRetrievalHitRateAndGroundedness()
    {
        var questionsJson = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "GoldenData", "questions.json"));
        var questions = JsonSerializer.Deserialize<List<GoldenQuestion>>(
            questionsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var judge = new HeuristicGroundednessJudge();
        var results = new List<(GoldenQuestion Question, bool Hit, GroundednessScore Score, string Answer)>();

        foreach (var question in questions)
        {
            await using var scope = fixture.Services.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var chatAgent = scope.ServiceProvider.GetRequiredService<IChatAgent>();

            var searchResults = await sender.Send(
                new SearchKnowledgeBaseQuery(fixture.KnowledgeBaseId, question.Question, TopK: 5));

            var expectedDocumentId = fixture.DocumentIdsByFileName[question.ExpectedSourceFileName];
            var hit = searchResults.Any(r => r.DocumentId == expectedDocumentId);

            var agentResult = await chatAgent.AskAsync(fixture.KnowledgeBaseId, [], question.Question, CancellationToken.None);
            var score = await judge.ScoreAsync(
                question.Question, agentResult.Answer, searchResults.Select(r => r.Content).ToList(), CancellationToken.None);

            results.Add((question, hit, score, agentResult.Answer));
        }

        var hitRate = results.Count(r => r.Hit) / (double)results.Count;
        var avgGroundedness = results.Average(r => r.Score.Groundedness);
        var avgRelevance = results.Average(r => r.Score.AnswerRelevance);

        output.WriteLine($"Retrieval hit rate: {hitRate:P0} ({results.Count(r => r.Hit)}/{results.Count})");
        output.WriteLine($"Average groundedness (heuristic): {avgGroundedness:P0}");
        output.WriteLine($"Average answer relevance (heuristic): {avgRelevance:P0}");
        output.WriteLine(string.Empty);
        output.WriteLine("Per-question results:");
        foreach (var r in results)
        {
            output.WriteLine($"  [{(r.Hit ? "hit " : "MISS")}] groundedness={r.Score.Groundedness:F2} relevance={r.Score.AnswerRelevance:F2} \"{r.Question.Question}\"");
        }

        Assert.All(results, r => Assert.False(string.IsNullOrWhiteSpace(r.Answer)));

        // Quality thresholds are intentionally not asserted yet. With the
        // fake/random-vector embedding provider in use today, retrieval hit rate
        // and heuristic groundedness/relevance are not meaningful quality signals -
        // this harness currently verifies the eval pipeline plumbing only. Once a
        // real embedding provider and a real LLM-as-judge are wired in, tune and
        // enable thresholds such as:
        // Assert.True(hitRate >= 0.8, $"Retrieval hit rate {hitRate:P0} below threshold");
        // Assert.True(avgGroundedness >= 0.8, $"Groundedness {avgGroundedness:P0} below threshold");
    }
}
