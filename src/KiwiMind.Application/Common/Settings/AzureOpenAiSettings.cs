namespace KiwiMind.Application.Common.Settings;

public class AzureOpenAiSettings
{
    public const string SectionName = "AzureOpenAI";

    public bool Enabled { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ChatDeploymentName { get; set; } = string.Empty;
    public string EmbeddingDeploymentName { get; set; } = string.Empty;
}
