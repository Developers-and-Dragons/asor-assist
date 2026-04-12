namespace AsorAssistant.Domain.Models;

public class AgentSkillResource
{
    public string? SkillId { get; set; }
    public ExecutionMode? ExecutionMode { get; set; }
    public List<WorkdayResource>? WorkdayResources { get; set; }
}
