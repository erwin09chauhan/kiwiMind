namespace KiwiMind.RagEval.Scoring;

public class HeuristicGroundednessJudge : IGroundednessJudge
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "is", "are", "of", "to", "in", "and", "or", "on", "for", "with",
        "what", "how", "does", "do", "did", "was", "were", "this", "that", "it", "its",
        "can", "which", "at", "by", "be", "as", "each", "any", "most"
    };

    public Task<GroundednessScore> ScoreAsync(string question, string answer, IReadOnlyList<string> retrievedChunks, CancellationToken cancellationToken)
    {
        var answerWords = SignificantWords(answer);
        var contextWords = new HashSet<string>(retrievedChunks.SelectMany(SignificantWords), StringComparer.OrdinalIgnoreCase);
        var questionWords = SignificantWords(question);

        var groundedness = answerWords.Count == 0
            ? 0
            : (double)answerWords.Count(w => contextWords.Contains(w)) / answerWords.Count;

        var answerWordSet = new HashSet<string>(answerWords, StringComparer.OrdinalIgnoreCase);
        var relevance = questionWords.Count == 0
            ? 0
            : (double)questionWords.Count(w => answerWordSet.Contains(w)) / questionWords.Count;

        return Task.FromResult(new GroundednessScore(
            groundedness,
            relevance,
            "Heuristic word-overlap judge (placeholder for a real LLM-as-judge)."));
    }

    private static List<string> SignificantWords(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => new string(w.Where(char.IsLetterOrDigit).ToArray()))
            .Where(w => w.Length > 2 && !StopWords.Contains(w))
            .ToList();
}
