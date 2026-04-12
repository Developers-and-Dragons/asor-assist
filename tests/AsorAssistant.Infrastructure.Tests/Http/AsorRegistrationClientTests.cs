using System.Net;
using System.Text.Json;
using AsorAssistant.Application.Models;
using AsorAssistant.Application.Serialization;
using AsorAssistant.Domain.Models;
using AsorAssistant.Infrastructure.Http;

namespace AsorAssistant.Infrastructure.Tests.Http;

public class AsorRegistrationClientTests
{
    private static RegistrationContext CreateContext() => new()
    {
        WorkdayHost = "example.workday.com",
        TenantName = "test-tenant",
        BearerToken = "test-token-abc123"
    };

    private static AgentDefinition CreateDefinition() => new()
    {
        Name = "Test Agent",
        Description = "A test agent",
        Url = "https://example.com/agent",
        Version = "1.0.0",
        Provider = new Provider { Id = "test-provider" },
        Platform = new Platform { Id = "OTHER" },
        Capabilities = new Capabilities(),
        Skills =
        [
            new AgentSkill
            {
                Id = "skill-1",
                Name = "Test Skill",
                Description = "A test skill",
                Tags = [new SkillTag { Tag = "test" }]
            }
        ]
    };

    private static (AsorRegistrationClient client, FakeHandler handler) CreateClient()
    {
        var handler = new FakeHandler();
        var httpClient = new HttpClient(handler);
        return (new AsorRegistrationClient(httpClient), handler);
    }

    [Fact]
    public async Task Successful_post_returns_201_with_parsed_definition()
    {
        var (client, handler) = CreateClient();
        var definition = CreateDefinition();
        var responseJson = JsonSerializer.Serialize(definition, AsorJsonContext.Default.AgentDefinition);

        handler.SetResponse(HttpStatusCode.Created, responseJson);

        var result = await client.RegisterAsync(CreateContext(), definition);

        Assert.True(result.Success);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(result.Definition);
        Assert.Equal("Test Agent", result.Definition.Name);
    }

    [Fact]
    public async Task Builds_correct_url_from_host_and_tenant()
    {
        var (client, handler) = CreateClient();
        var definition = CreateDefinition();
        handler.SetResponse(HttpStatusCode.Created,
            JsonSerializer.Serialize(definition, AsorJsonContext.Default.AgentDefinition));

        await client.RegisterAsync(CreateContext(), definition);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(
            "https://example.workday.com/api/asor/v1/test-tenant/agentDefinition",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Sets_bearer_token_in_authorization_header()
    {
        var (client, handler) = CreateClient();
        var definition = CreateDefinition();
        handler.SetResponse(HttpStatusCode.Created,
            JsonSerializer.Serialize(definition, AsorJsonContext.Default.AgentDefinition));

        await client.RegisterAsync(CreateContext(), definition);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("test-token-abc123", handler.LastRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task Sets_content_type_application_json()
    {
        var (client, handler) = CreateClient();
        var definition = CreateDefinition();
        handler.SetResponse(HttpStatusCode.Created,
            JsonSerializer.Serialize(definition, AsorJsonContext.Default.AgentDefinition));

        await client.RegisterAsync(CreateContext(), definition);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("application/json",
            handler.LastRequest.Content!.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Sends_post_method()
    {
        var (client, handler) = CreateClient();
        var definition = CreateDefinition();
        handler.SetResponse(HttpStatusCode.Created,
            JsonSerializer.Serialize(definition, AsorJsonContext.Default.AgentDefinition));

        await client.RegisterAsync(CreateContext(), definition);

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
    }

    [Fact]
    public async Task Validation_error_400_parsed_into_structured_errors()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.BadRequest,
            """{"error": "Validation failed", "errors": [{"error": "Name required", "field": "name"}]}""");

        var result = await client.RegisterAsync(CreateContext(), CreateDefinition());

        Assert.False(result.Success);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Error == "Name required");
    }

    [Fact]
    public async Task Auth_failure_401_returns_failure_result()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.Unauthorized,
            """{"error": "Unauthorized"}""");

        var result = await client.RegisterAsync(CreateContext(), CreateDefinition());

        Assert.False(result.Success);
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task Auth_failure_403_returns_failure_result()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.Forbidden,
            """{"error": "Forbidden"}""");

        var result = await client.RegisterAsync(CreateContext(), CreateDefinition());

        Assert.False(result.Success);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task Network_error_returns_failure_not_exception()
    {
        var (client, handler) = CreateClient();
        handler.SetException(new HttpRequestException("Connection refused"));

        var result = await client.RegisterAsync(CreateContext(), CreateDefinition());

        Assert.False(result.Success);
        Assert.Null(result.StatusCode);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Error!.Contains("Connection refused"));
    }

    [Fact]
    public async Task Request_body_contains_serialized_definition()
    {
        var (client, handler) = CreateClient();
        var definition = CreateDefinition();
        handler.SetResponse(HttpStatusCode.Created,
            JsonSerializer.Serialize(definition, AsorJsonContext.Default.AgentDefinition));

        await client.RegisterAsync(CreateContext(), definition);

        Assert.NotNull(handler.LastRequestBody);
        var parsed = JsonDocument.Parse(handler.LastRequestBody);
        Assert.Equal("Test Agent", parsed.RootElement.GetProperty("name").GetString());
    }

    public class FakeHandler : HttpMessageHandler
    {
        private HttpStatusCode _statusCode;
        private string _responseBody = "";
        private Exception? _exception;

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        public void SetResponse(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _responseBody = body;
            _exception = null;
        }

        public void SetException(Exception exception)
        {
            _exception = exception;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content is not null)
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

            if (_exception is not null)
                throw _exception;

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}
