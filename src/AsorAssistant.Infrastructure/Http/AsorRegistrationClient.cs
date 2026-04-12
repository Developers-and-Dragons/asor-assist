using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AsorAssistant.Core.Models;
using AsorAssistant.Core.Ports;
using AsorAssistant.Core.Serialization;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Infrastructure.Http;

public class AsorRegistrationClient : IAsorRegistrationClient
{
    private readonly HttpClient _httpClient;

    public AsorRegistrationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RegistrationResult> RegisterAsync(RegistrationContext context, AgentDefinition definition)
    {
        try
        {
            var url = AsorUrlBuilder.BuildRegistrationUrl(context.Region!.BaseUrl);
            var json = JsonSerializer.Serialize(definition, AsorJsonContext.Default.AgentDefinition);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.BearerToken);

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var created = JsonSerializer.Deserialize(responseBody, AsorJsonContext.Default.AgentDefinition);
                return RegistrationResult.Succeeded(response.StatusCode, created!, responseBody);
            }

            var errors = AsorErrorParser.Parse(responseBody);
            return RegistrationResult.Failed(response.StatusCode, errors, responseBody);
        }
        catch (HttpRequestException ex)
        {
            return RegistrationResult.NetworkError(ex.Message);
        }
    }
}
