namespace AsorAssistant.Domain.Models;

public static class KnownPlatforms
{
    public static IReadOnlyList<(string Name, string Id)> All { get; } =
    [
        ("Microsoft Copilot Studio", "MICROSOFT_COPILOT_STUDIO"),
        ("Microsoft Azure AI Foundry", "MICROSOFT_AZURE_AI_FOUNDRY"),
        ("Amazon Bedrock AgentCore", "AMAZON_BEDROCK_AGENTCORE"),
        ("Salesforce Agentforce", "SALESFORCE_AGENTFORCE"),
        ("Snowflake Cortex", "SNOWFLAKE_CORTEX"),
        ("Google Agentspace", "GOOGLE_AGENTSPACE"),
        ("Other", "OTHER"),
    ];
}
