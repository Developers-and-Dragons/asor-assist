using System.Text.Json.Serialization;
using AsorAssistant.Application.Models;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Application.Serialization;

[JsonSerializable(typeof(AgentDefinition))]
[JsonSerializable(typeof(DraftEnvelope))]
[JsonSerializable(typeof(AsorValidationErrorResponse))]
[JsonSerializable(typeof(AsorErrorResponse))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
public partial class AsorJsonContext : JsonSerializerContext;

public class AsorValidationErrorResponse
{
    public string? Error { get; set; }
    public List<AsorError>? Errors { get; set; }
}

public class AsorErrorResponse
{
    public string? Error { get; set; }
    public string? Code { get; set; }
    public string? Field { get; set; }
    public string? Path { get; set; }
}
