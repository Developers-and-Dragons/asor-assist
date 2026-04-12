using System.Net;
using AsorAssistant.Infrastructure.Http;

namespace AsorAssistant.Infrastructure.Tests.Http;

public class WqlClientTests
{
    private static (WqlClient client, FakeHandler handler) CreateClient()
    {
        var handler = new FakeHandler();
        var httpClient = new HttpClient(handler);
        return (new WqlClient(httpClient), handler);
    }

    [Fact]
    public async Task Sends_post_to_wql_endpoint()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, """{"data": [], "total": 0}""");

        await client.QueryAsync("token", "SELECT foo FROM bar");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("https://api.workday.com/wql/v1/data", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Sets_bearer_token()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, """{"data": [], "total": 0}""");

        await client.QueryAsync("my-secret-token", "SELECT foo FROM bar");

        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("my-secret-token", handler.LastRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task Sends_query_in_json_body()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK, """{"data": [], "total": 0}""");

        await client.QueryAsync("token", "SELECT webService FROM publicWebServices");

        Assert.NotNull(handler.LastRequestBody);
        Assert.Contains("SELECT webService FROM publicWebServices", handler.LastRequestBody);
        Assert.Equal("application/json", handler.LastRequest!.Content!.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Parses_successful_response()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.OK,
            """{"data": [{"workdayID": "abc123"}], "total": 1}""");

        var result = await client.QueryAsync("token", "SELECT foo FROM bar");

        Assert.Equal(1, result.Total);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task Throws_on_error_response()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.BadRequest, """{"error": "Invalid query"}""");

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.QueryAsync("token", "INVALID QUERY"));
    }

    [Fact]
    public async Task Throws_on_unauthorized()
    {
        var (client, handler) = CreateClient();
        handler.SetResponse(HttpStatusCode.Unauthorized, """{"error": "Unauthorized"}""");

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.QueryAsync("bad-token", "SELECT foo FROM bar"));
    }

    public class FakeHandler : HttpMessageHandler
    {
        private HttpStatusCode _statusCode;
        private string _responseBody = "";

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        public void SetResponse(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _responseBody = body;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (request.Content is not null)
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}
