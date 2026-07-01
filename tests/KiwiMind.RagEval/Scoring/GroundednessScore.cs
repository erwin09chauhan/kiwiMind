namespace KiwiMind.RagEval.Scoring;

public record GroundednessScore(double Groundedness, double AnswerRelevance, string Reason);
