using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using AsorAssistant.Application.Models;
using AsorAssistant.Application.Ports;
using AsorAssistant.Application.Serialization;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Infrastructure.Http;

public class AsorQueryClient : IAsorQueryClient
{
    private readonly HttpClient _httpClient;

    public AsorQueryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AgentDefinition?> GetDefinitionAsync(RegistrationContext context, string id)
    {
        var url = AsorUrlBuilder.BuildDefinitionUrl(context.Region!.BaseUrl, id);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.BearerToken);

        using var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize(responseBody, AsorJsonContext.Default.AgentDefinition);
    }
}
