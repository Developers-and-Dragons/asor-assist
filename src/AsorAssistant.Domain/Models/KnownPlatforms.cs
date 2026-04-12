namespace AsorAssistant.Domain.Models;

public static class KnownPlatforms
{
    public static IReadOnlyList<(string Name, string Id)> All { get; } =
    [
        ("Microsoft Copilot Studio", "Platform=MICROSOFT_COPILOT_STUDIO"),
        ("Microsoft Azure AI Foundry", "Platform=MICROSOFT_AZURE_AI_FOUNDRY"),
        ("Amazon Bedrock AgentCore", "Platform=AMAZON_BEDROCK_AGENTCORE"),
        ("Salesforce Agentforce", "Platform=SALESFORCE_AGENTFORCE"),
        ("Snowflake Cortex", "Platform=SNOWFLAKE_CORTEX"),
        ("Google Agentspace", "Platform=GOOGLE_AGENTSPACE"),
        ("Other", "Platform=OTHER"),
    ];
}
