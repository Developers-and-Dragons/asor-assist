using System.Text.Json.Serialization;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Application.Serialization;

[JsonSerializable(typeof(AgentDefinition))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
public partial class AsorJsonContext : JsonSerializerContext;
