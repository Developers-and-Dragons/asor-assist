using System.Text.Json.Serialization;

namespace AsorAssistant.Domain.Models;

public class WorkdayResource
{
    [JsonPropertyName("agent_resource")]
    public AgentResource? AgentResource { get; set; }

    [JsonPropertyName("tool_name")]
    public string? ToolName { get; set; }

    public string? Description { get; set; }
    public List<SecurableItem>? Tools { get; set; }
}
