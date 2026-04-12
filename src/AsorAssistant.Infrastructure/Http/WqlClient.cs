using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AsorAssistant.Application.Models;
using AsorAssistant.Application.Ports;
using AsorAssistant.Application.Serialization;

namespace AsorAssistant.Infrastructure.Http;

public class WqlClient : IWqlClient
{
    private readonly HttpClient _httpClient;

    public WqlClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WqlResponse> QueryAsync(string bearerToken, string query)
    {
        var url = AsorUrlBuilder.BuildWqlUrl();
        var requestBody = JsonSerializer.Serialize(
            new WqlQueryRequest { Query = query },
            AsorJsonContext.Default.WqlQueryRequest);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize(responseBody, AsorJsonContext.Default.WqlResponse)
            ?? new WqlResponse { Data = [], Total = 0 };
    }
}
