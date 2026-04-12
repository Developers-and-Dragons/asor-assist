namespace AsorAssistant.Infrastructure.Http;

public static class AsorUrlBuilder
{
    public static string BuildRegistrationUrl(string host, string tenant) =>
        $"https://{host.TrimEnd('/')}/api/asor/v1/{Uri.EscapeDataString(tenant)}/agentDefinition";

    public static string BuildDefinitionUrl(string host, string tenant, string id) =>
        $"https://{host.TrimEnd('/')}/api/asor/v1/{Uri.EscapeDataString(tenant)}/agentDefinition/{Uri.EscapeDataString(id)}";
}
