namespace KiwiMind.Application.Common.Settings;

public class BlobStorageSettings
{
    public const string SectionName = "BlobStorage";

    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "documents";
}
