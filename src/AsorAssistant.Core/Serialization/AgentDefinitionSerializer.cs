using System.Text.Json;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Core.Serialization;

public static class AgentDefinitionSerializer
{
    public static string Serialize(AgentDefinition definition) =>
        JsonSerializer.Serialize(definition, AsorJsonContext.Default.AgentDefinition);

    public static AgentDefinition? Deserialize(string json) =>
        JsonSerializer.Deserialize(json, AsorJsonContext.Default.AgentDefinition);
}
