namespace AsorAssistant.Infrastructure.Http;

public static class AsorUrlBuilder
{
    public static string BuildRegistrationUrl(string regionBaseUrl) =>
        $"{regionBaseUrl.TrimEnd('/')}/asor/v1/agentDefinition";

    public static string BuildDefinitionUrl(string regionBaseUrl, string id) =>
        $"{regionBaseUrl.TrimEnd('/')}/asor/v1/agentDefinition/{Uri.EscapeDataString(id)}";

    public static string BuildWqlUrl() =>
        "https://api.workday.com/wql/v1/data";

}
