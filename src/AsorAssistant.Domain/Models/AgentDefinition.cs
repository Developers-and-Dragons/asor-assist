namespace AsorAssistant.Domain.Models;

public class AgentDefinition
{
    // Required fields
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? Version { get; set; }
    public Provider? Provider { get; set; }
    public Platform? Platform { get; set; }
    public Capabilities? Capabilities { get; set; }
    public List<AgentSkill>? Skills { get; set; }

    // Optional fields
    public string? Id { get; set; }
    public string? Overview { get; set; }
    public string? IconUrl { get; set; }
    public string? DocumentationUrl { get; set; }
    public string? ExternalAgentID { get; set; }
    public string? ExternalTenantID { get; set; }
    public List<string>? DefaultInputModes { get; set; }
    public List<string>? DefaultOutputModes { get; set; }
    public bool? SupportsAuthenticatedExtendedCard { get; set; }
    public List<AgentSkillResource>? WorkdayConfig { get; set; }
}
