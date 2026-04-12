namespace AsorAssistant.Domain.Models;

public class AgentSkill
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<SkillTag>? Tags { get; set; }
    public List<string>? InputModes { get; set; }
    public List<string>? OutputModes { get; set; }
}
