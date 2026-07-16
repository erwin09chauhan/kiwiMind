using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace KiwiMind.Application.Common.Telemetry;

public static class KiwiMindTelemetry
{
    public const string SourceName = "KiwiMind";

    public static readonly ActivitySource ActivitySource = new(SourceName);

    private static readonly Meter Meter = new(SourceName);

    public static readonly Counter<long> TokensUsed =
        Meter.CreateCounter<long>("kiwimind.tokens_used", unit: "tokens", description: "Tokens used per generated answer.");

    public static readonly Counter<long> ToolCalls =
        Meter.CreateCounter<long>("kiwimind.tool_calls", unit: "calls", description: "Agent tool invocations, by tool name.");
}
