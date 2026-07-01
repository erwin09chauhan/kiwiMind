namespace KiwiMind.RagEval.Scoring;

public interface IGroundednessJudge
{
    Task<GroundednessScore> ScoreAsync(string question, string answer, IReadOnlyList<string> retrievedChunks, CancellationToken cancellationToken);
}
