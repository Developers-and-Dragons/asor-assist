using AsorAssistant.Infrastructure.Http;

namespace AsorAssistant.Infrastructure.Tests.Http;

public class AsorErrorParserTests
{
    [Fact]
    public void Parses_validation_error_format()
    {
        var body = """
            {
                "error": "Validation failed",
                "errors": [
                    {"error": "Name is required", "field": "name"},
                    {"error": "URL is required", "field": "url"}
                ]
            }
            """;

        var errors = AsorErrorParser.Parse(body);

        Assert.Equal(2, errors.Count);
        Assert.Equal("Name is required", errors[0].Error);
        Assert.Equal("name", errors[0].Field);
        Assert.Equal("URL is required", errors[1].Error);
    }

    [Fact]
    public void Parses_single_error_model_format()
    {
        var body = """
            {
                "error": "Not found",
                "code": "NOT_FOUND",
                "field": "id",
                "path": "/agentDefinition/abc123"
            }
            """;

        var errors = AsorErrorParser.Parse(body);

        Assert.Single(errors);
        Assert.Equal("Not found", errors[0].Error);
        Assert.Equal("NOT_FOUND", errors[0].Code);
        Assert.Equal("id", errors[0].Field);
        Assert.Equal("/agentDefinition/abc123", errors[0].Path);
    }

    [Fact]
    public void Handles_unparseable_body_gracefully()
    {
        var body = "This is not JSON at all";

        var errors = AsorErrorParser.Parse(body);

        Assert.Single(errors);
        Assert.NotNull(errors[0].Error);
        Assert.Contains("This is not JSON at all", errors[0].Error);
    }

    [Fact]
    public void Handles_empty_body()
    {
        var errors = AsorErrorParser.Parse("");

        Assert.Single(errors);
        Assert.NotNull(errors[0].Error);
    }

    [Fact]
    public void Validation_error_with_only_top_level_error()
    {
        var body = """{"error": "Something went wrong"}""";

        var errors = AsorErrorParser.Parse(body);

        Assert.Single(errors);
        Assert.Equal("Something went wrong", errors[0].Error);
    }
}
