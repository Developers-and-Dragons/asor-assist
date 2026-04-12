using AsorAssistant.Domain.Models;

namespace AsorAssistant.Core.Models;

public class AgentListResponse
{
    public int Total { get; set; }
    public List<AgentDefinition>? Data { get; set; }
}
