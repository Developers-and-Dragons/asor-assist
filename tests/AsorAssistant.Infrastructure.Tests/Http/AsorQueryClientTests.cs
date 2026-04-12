using System.Net;
using System.Text.Json;
using AsorAssistant.Core.Models;
using AsorAssistant.Core.Serialization;
using AsorAssistant.Domain.Models;
using AsorAssistant.Infrastructure.Http;

namespace AsorAssistant.Infrastructure.Tests.Http;

public class AsorQueryClientTests
{
    private static RegistrationContext CreateContext() => new()
    {
        Region = new AsorRegion { Name = "US", BaseUrl = "https://us.agent.workday.com" },
        TenantName = "test-tenant",
        BearerToken = "test-token"
    };

    private static (AsorQueryClient client, FakeHandler handler) CreateClient()
    {
        var handler = new FakeHandler();
        var httpClient = new HttpClient(handler);
        return (new AsorQueryClient(httpClient), handler);
    }

    [Fact]
    public async Task ListDefinitions_sends_get_to_registration_url()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, """{"total": 0, "data": []}""");

        await client.ListDefinitionsAsync(CreateContext());

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("https://us.agent.workday.com/asor/v1/agentDefinition",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task ListDefinitions_sets_bearer_token()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, """{"total": 0, "data": []}""");

        await client.ListDefinitionsAsync(CreateContext());

        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("test-token", handler.LastRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task ListDefinitions_parses_wrapped_response()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, """
        {
          "total": 2,
          "data": [
            {
              "name": "Agent One",
              "description": "First",
              "url": "https://a.com",
              "version": "1.0",
              "provider": {"id": "abc", "descriptor": "Self-Built"},
              "platform": {"id": "def", "descriptor": "Other", "reference_id": "OTHER"},
              "capabilities": {"pushNotifications": false, "streaming": false, "stateTransitionHistory": false},
              "skills": [{"id": "s1", "name": "Skill", "description": "A skill"}]
            },
            {
              "name": "Agent Two",
              "description": "Second",
              "url": "https://b.com",
              "version": "2.0",
              "provider": {"id": "xyz"},
              "platform": {"id": "ghi"},
              "capabilities": {},
              "skills": [{"id": "s2", "name": "Skill 2", "description": "Another"}]
            }
          ]
        }
        """);

        var result = await client.ListDefinitionsAsync(CreateContext());

        Assert.Equal(2, result.Count);
        Assert.Equal("Agent One", result[0].Name);
        Assert.Equal("Self-Built", result[0].Provider!.Descriptor);
        Assert.Equal("OTHER", result[0].Platform!.ReferenceId);
        Assert.Equal("Agent Two", result[1].Name);
    }

    [Fact]
    public async Task ListDefinitions_returns_empty_on_empty_data()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, """{"total": 0, "data": []}""");

        var result = await client.ListDefinitionsAsync(CreateContext());

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListDefinitions_throws_on_error()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.Unauthorized, """{"error": "Unauthorized"}""");

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.ListDefinitionsAsync(CreateContext()));
    }

    [Fact]
    public async Task GetDefinition_returns_null_on_404()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.NotFound, "");

        var result = await client.GetDefinitionAsync(CreateContext(), "nonexistent");

        Assert.Null(result);
    }

    public class FakeHandler : HttpMessageHandler
    {
        private HttpStatusCode _statusCode;
        private string _responseBody = "";

        public HttpRequestMessage? LastRequest { get; private set; }

        public void SetResponse(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _responseBody = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
